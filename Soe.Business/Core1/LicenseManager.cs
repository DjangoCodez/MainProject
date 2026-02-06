using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class LicenseManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public LicenseManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region License

        public List<License> GetAllLicensesOnServer(bool setSysServerUrl = false, string defaultHost = "---")
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return GetAllLicensesOnServer(entities, setSysServerUrl, defaultHost);
        }

        public List<License> GetAllLicensesOnServer(CompEntities entities, bool setSysServerUrl = false, string defaultHost = "")
        {
            var license100 = entities.License.FirstOrDefault(w => w.LicenseNr == "100");

            if (license100 != null)
            {
                bool? settingValue = SettingManager.GetUserCompanySetting(SettingMainType.License, (int)LicenseSettingType.AllLicensesOnServer, 0, 0, license100.LicenseId)?.BoolData;

                if (settingValue.HasValue && settingValue.Value)
                {
                    List<License> licenses = new List<License>();
                    int? sysCompDbId = SysServiceManager.GetSysCompDBId();

                    if (sysCompDbId.HasValue)
                    {
                        var sysCompDBs = SysServiceManager.GetSysCompDBs();

                        if (!sysCompDBs.IsNullOrEmpty())
                        {
                            int? sysCompServerId = sysCompDBs.FirstOrDefault(f => f.SysCompDbId == sysCompDbId.Value)?.SysCompServerId;

                            if (!sysCompServerId.IsNullOrEmpty())
                            {
                                sysCompDBs = sysCompDBs.Where(w => w.SysCompServerId == sysCompServerId.Value).ToList();

                                if (!sysCompDBs.IsNullOrEmpty())
                                {
                                    foreach (var db in sysCompDBs)
                                    {
                                        using (CompEntities ent = new CompEntities())
                                        {
                                            ent.ChangeDatabase(db.Name);
                                            string empty = "---";
                                            var lic = GetLicenses(ent, setSysServerUrl, empty);

                                            if (!lic.IsNullOrEmpty())
                                            {
                                                foreach (var item in lic)
                                                {
                                                    item.OnOtherServer = db.SysCompDbId != sysCompDbId.Value;

                                                    if (item.SysServerUrl.IsNullOrEmpty() || item.SysServerUrl.Equals(empty))
                                                    {
                                                        item.SysServerUrl = db.ApiUrl.ToLower().Replace("apix", "").Replace("apiinternal", "");
                                                    }

                                                    if (item.OnOtherServer)
                                                    {
                                                        item.EditUrl = item.SysServerUrl + item.EditUrl;
                                                        item.CompaniesUrl = item.SysServerUrl + item.CompaniesUrl;
                                                        item.UsersUrl = item.SysServerUrl + item.UsersUrl;
                                                    }

                                                }
                                                licenses.AddRange(lic);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return licenses;
                    }
                }
            }

            return GetLicenses(entities, setSysServerUrl, defaultHost);
        }

        public List<License> GetLicenses(bool setSysServerUrl = false, string defaultHost = "", bool loadCompany = false, bool loadRoles = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return GetLicenses(entities, setSysServerUrl, defaultHost, loadCompany, loadRoles);
        }

        public List<License> GetLicenses(CompEntities entities, bool setSysServerUrl = false, string defaultHost = "", bool loadCompany = false, bool loadRoles = false)
        {
            var query = from l in entities.License
                        where l.State == (int)SoeEntityState.Active
                        select l;

            if (loadCompany && loadRoles)
                query = query.Include(x => x.Company.Select(c => c.Role));
            else if (loadCompany)
                query = query.Include(l => l.Company);
            
            var licenses = query.ToList();

            if (setSysServerUrl)
            {
                if (String.IsNullOrEmpty(defaultHost))
                    defaultHost = "https://s1s1d1.softone.se"; //default

                var sysServers = SysServiceManager.GetSysservers();
                foreach (License license in licenses)
                {
                    license.EditUrl = String.Format("edit/?license={0}&licenseNr={1}", license.LicenseId, license.LicenseNr);
                    license.CompaniesUrl = String.Format("../companies/?license={0}&licenseNr={1}", license.LicenseId, license.LicenseNr);
                    license.UsersUrl = String.Format("../users/?license={0}&licenseNr={1}", license.LicenseId, license.LicenseNr);

                    if (license.SysServerId.HasValue && license.SysServerId.Value > 0)
                    {
                        SysServerDTO sysServer = sysServers?.FirstOrDefault(i => i.SysServerId == license.SysServerId.Value);
                        if (sysServer == null)
                        {
                            SysServer sysServerFromDB = LoginManager.GetSysServer(license.SysServerId.Value);
                            if (sysServerFromDB != null)
                            {
                                sysServer = new SysServerDTO() { SysServerId = sysServerFromDB.SysServerId, Url = sysServerFromDB.Url, UseLoadBalancer = sysServerFromDB.UseLoadBalancer };
                                license.SysServerUrl = sysServer.Url;
                                if (sysServers != null)
                                {
                                    sysServers.Add(sysServer);
                                    sysServer = sysServers.FirstOrDefault(i => i.SysServerId == license.SysServerId.Value);
                                }
                            }
                        }

                        if (sysServer != null)
                            license.SysServerUrl = sysServer.Url;
                    }
                    else
                    {
                        license.SysServerUrl = defaultHost;
                    }
                }
            }

            licenses = licenses.OrderBy(l => l.LicenseNr.PadLeft(50, '0')).ToList();

            return licenses;
        }

        public Dictionary<int, string> GetLicensesDict(bool addEmptyRow, bool concatNrAndName)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            var licenses = GetLicenses();
            foreach (License license in licenses)
            {
                dict.Add(license.LicenseId, concatNrAndName ? String.Format("{0} {1}", license.LicenseNr, license.Name) : license.Name);
            }

            return dict;
        }

        public License GetLicense(int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return GetLicense(entities, licenseId);
        }

        public License GetLicense(CompEntities entities, int licenseId)
        {
            return (from l in entities.License
                    where l.LicenseId == licenseId &&
                    l.State == (int)SoeEntityState.Active
                    select l).FirstOrDefault();
        }

        public License GetLicenseByNr(string licenseNr)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return GetLicenseByNr(entities, licenseNr);
        }

        public License GetLicenseByNr(CompEntities entities, string licenseNr)
        {
            return (from l in entities.License
                    where l.LicenseNr == licenseNr &&
                    l.State == (int)SoeEntityState.Active
                    select l).FirstOrDefault();
        }

        public License GetLicenseByGuid(Guid licenseGuid)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return GetLicenseByGuid(entities, licenseGuid);
        }

        public License GetLicenseByGuid(CompEntities entities, Guid licenseGuid)
        {
            return (from l in entities.License
                    where l.LicenseGuid == licenseGuid &&
                    l.State == (int)SoeEntityState.Active
                    select l).FirstOrDefault();
        }

        public License GetLicenseByOrgNr(string orgNr)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return (from l in entities.License
                    where l.OrgNr == orgNr &&
                    l.State == (int)SoeEntityState.Active
                    select l).FirstOrDefault();
        }

        public License GetLicenseByCompany(int actorCompanyId, bool loadFeatures = false, bool loadArticles = false, bool loadUsers = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return GetLicenseByCompany(entities, actorCompanyId, loadFeatures, loadArticles, loadUsers);
        }

        public License GetLicenseByCompany(CompEntities entities, int actorCompanyId, bool loadFeatures = false, bool loadArticles = false, bool loadUsers = false)
        {
            License license = (from c in entities.Company
                               where c.ActorCompanyId == actorCompanyId &&
                               c.State == (int)SoeEntityState.Active
                               orderby c.License.LicenseNr ascending
                               select c.License).FirstOrDefault();

            if (license != null)
            {
                if (loadFeatures && !license.LicenseFeature.IsLoaded)
                    license.LicenseFeature.Load();
                if (loadArticles && !license.LicenseArticle.IsLoaded)
                    license.LicenseArticle.Load();
                if (loadUsers && !license.User.IsLoaded)
                    license.User.Load();
            }

            return license;
        }

        public License GetLicenseAndFeatures(CompEntities entities, int licenseId)
        {
            return (from l in entities.License
                        .Include("LicenseFeature")
                    where l.LicenseId == licenseId &&
                    l.State == (int)SoeEntityState.Active
                    select l).FirstOrDefault();
        }

        public License GetSupportLicense()
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return (from l in entities.License
                    where l.Support &&
                    l.State == (int)SoeEntityState.Active
                    select l).FirstOrDefault();
        }

        public int GetLicenseIdByCompanyId(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.License.NoTracking();
            return (from c in entities.Company
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c.LicenseId).FirstOrDefault();
        }

        public string GetNextLicenseNr()
        {
            int lastNr = 0;
            IEnumerable<License> licenses = GetLicenses().OrderBy(a => a.LicenseNrSort);

            if (licenses.Any())
            {
                Int32.TryParse(licenses.Last().LicenseNr, out lastNr);
                // If unable to parse, numeric values are not used
                if (lastNr == 0)
                    return String.Empty;
            }

            lastNr++;

            // Check that number is not used
            if (licenses.Any(l => l.LicenseNr == lastNr.ToString()))
                return String.Empty;

            return lastNr.ToString();
        }

        public bool LicenseHasCompanies(int licenseId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Company.NoTracking();
            return entitiesReadOnly.Company.Any(c => c.LicenseId == licenseId && c.State == (int)SoeEntityState.Active);
        }

        public bool LicenseHasDemoCompany(int licenseId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Company.NoTracking();
            return entitiesReadOnly.Company.Any(c => c.LicenseId == licenseId && c.Demo && c.State == (int)SoeEntityState.Active);
        }

        public bool LicenseExist(string licenseNr)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.License.NoTracking();
            return entitiesReadOnly.License.Any(l => l.LicenseNr == licenseNr && l.State == (int)SoeEntityState.Active);
        }

        public bool IsLicenseTerminated(string licenseNr)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.License.NoTracking();
            DateTime? terminationDate = (from l in entitiesReadOnly.License
                                         where l.LicenseNr == licenseNr
                                         select l.TerminationDate).FirstOrDefault();

            return terminationDate.HasValue && terminationDate.Value <= DateTime.Now;
        }

        public ActionResult ChangeSysServIdOnLicense(int licenseid, int? toSysServerId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var license = entities.License.FirstOrDefault(w => w.LicenseId == licenseid);
                if (license != null)
                {
                    license.SysServerId = toSysServerId;
                    return SaveChanges(entities);
                }
            }
            return new ActionResult();
        }

        public ActionResult ChangeSysServerId(int? fromSysServerid, int? toSysServerId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var licenses = entities.License.Where(w => w.SysServerId == fromSysServerid).ToList();
                if (!licenses.IsNullOrEmpty())
                {
                    foreach (var license in licenses)
                    {
                        license.SysServerId = toSysServerId;
                    }

                    return SaveChanges(entities);
                }
            }

            return new ActionResult();
        }

        public ActionResult ValidateCompany(License license, Company company = null, string orgNr = "", bool discardAddCheck = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return ValidateCompany(entities, license, company?.ActorCompanyId, orgNr, discardAddCheck);
        }

        public ActionResult ValidateCompany(CompEntities entities, License license, int? actorCompanyId = null, string orgNr = "", bool discardAddCheck = false)
        {
            if (license == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "License");

            bool isNewCompany = !actorCompanyId.HasValue || actorCompanyId.Value == 0;
            if (!string.IsNullOrEmpty(orgNr) && CompanyManager.CompanyExist(entities, license.LicenseId, orgNr, actorCompanyId))
                return new ActionResult((int)ActionResultSave.CompanyExists);

            if (isNewCompany && !discardAddCheck && license.NrOfCompanies.HasValue && CompanyManager.GetNrOfCompaniesByLicense(entities, license.LicenseId, excludeDemo: true) >= license.NrOfCompanies.Value)
                return new ActionResult((int)ActionResultSave.CompanyCannotBeAddedLicenseViolation, license.NrOfCompanies.Value);

            return new ActionResult(true);
        }

        public ActionResult ValidateUser(CompEntities entities, License license, Common.DTO.EmployeeUserDTO user, bool? active = true, int userStateTransition = 0)
        {
            if (user == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

            return ValidateUser(entities, license, user.ActorCompanyId, user.UserId, user.LoginName, false, active, userStateTransition);
        }

        public ActionResult ValidateUser(CompEntities entities, License license, int actorCompanyId, User user = null, string loginName = "", bool discardAddCheck = false, bool? active = true, int userStateTransition = 0)
        {
            return ValidateUser(entities, license, actorCompanyId, user?.UserId, loginName, discardAddCheck, active, userStateTransition);
        }

        public ActionResult ValidateUser(CompEntities entities, License license, int actorCompanyId, int? userId = null, string loginName = "", bool discardAddCheck = false, bool? active = true, int userStateTransition = 0)
        {
            ActionResult result = new ActionResult(true);

            if (license == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "License");

            bool isTransitionFromActive = userStateTransition != (int)SoeEntityStateTransition.None && userStateTransition != (int)SoeEntityStateTransition.ActiveToDeleted && userStateTransition != (int)SoeEntityStateTransition.ActiveToInactive;
            if (!String.IsNullOrEmpty(loginName) && isTransitionFromActive && UserManager.UserExist(entities, license.LicenseId, loginName, false, userId.ToNullable()))
                return new ActionResult((int)ActionResultSave.UserExists, String.Format(GetText(11043, "Användare med användarnamn '{0}' finns redan på aktuell licens"), loginName));

            bool isNewUser = (!userId.HasValue || userId.Value == 0);
            bool isExistingUser = !isNewUser;
            bool checkNewUser = isNewUser && !discardAddCheck;
            bool checkExistingUser = isExistingUser && userStateTransition == (int)SoeEntityStateTransition.InactiveToActive;
            bool checkUser = checkNewUser || checkExistingUser;

            if (checkUser)
            {
                var company = CompanyManager.GetCompany(entities, actorCompanyId, false);
                if (!company.Demo)
                {
                    int nrOfUsers = UserManager.GetNrOfUsersByLicense(entities, license.LicenseId, actorCompanyId, 0, 0, active, userStateTransition);
                    if (nrOfUsers > license.MaxNrOfUsers)
                        return new ActionResult(false, (int)ActionResultSave.UserCannotBeAddedLicenseViolation, String.Format(GetText(11045, "Licensen tillåter inte fler användare. Max {0} st"), license.MaxNrOfUsers), integerValue: license.MaxNrOfUsers);
                }
            }

            return result;
        }

        public ActionResult ValidateEmployee(CompEntities entities, License license, Common.DTO.EmployeeUserDTO employee, bool? active = true, int employeeStateTransition = 0)
        {
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "User");

            return ValidateEmployee(entities, license, employee.ActorCompanyId, employee.EmployeeId, employee.EmployeeNr, active: active, employeeStateTransition: employeeStateTransition);
        }

        public ActionResult ValidateEmployee(License license, int actorCompanyId, Employee employee = null, string employeeNr = "", bool discardAddCheck = false, int employeeStateTransition = 0)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return ValidateEmployee(entities, license, actorCompanyId, employee, employeeNr, discardAddCheck, employeeStateTransition: employeeStateTransition);
        }

        public ActionResult ValidateEmployee(CompEntities entities, License license, int actorCompanyId, Employee employee = null, string employeeNr = "", bool discardAddCheck = false, int employeeStateTransition = 0)
        {
            return ValidateEmployee(entities, license, actorCompanyId, employee?.EmployeeId, employeeNr, discardAddCheck, employeeStateTransition: employeeStateTransition);
        }

        public ActionResult ValidateEmployee(CompEntities entities, License license, int actorCompanyId, int? employeeId, string employeeNr, bool discardAddCheck = false, bool? active = true, int employeeStateTransition = 0)
        {
            if (license == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "License");

            if (!String.IsNullOrEmpty(employeeNr) && EmployeeManager.EmployeeExists(entities, employeeNr, actorCompanyId, false, employeeId.ToNullable()))
            {
                return new ActionResult((int)ActionResultSave.EmployeeNumberExists, String.Format(GetText(5882, "Anställningsnumret '{0}' är upptaget"), employeeNr))
                {
                    StringValue = employeeNr
                };
            }

            ActionResult result = new ActionResult(true);

            bool isNewUser = (!employeeId.HasValue || employeeId.Value == 0);
            bool isExistingUser = !isNewUser;
            bool checkNewEmployee = isNewUser && !discardAddCheck;
            bool checkExistingEmployee = isExistingUser && employeeStateTransition == (int)SoeEntityStateTransition.InactiveToActive;
            bool checkEmployee = checkNewEmployee || checkExistingEmployee;

            if (checkEmployee)
            {
                var company = CompanyManager.GetCompany(entities, actorCompanyId, false);
                if (license.NrOfCompanies.HasValue && !company.Demo)
                {
                    int nrOfEmployees = EmployeeManager.GetNrOfEmployeesByLicense(entities, license.LicenseId, active);
                    if (nrOfEmployees >= license.MaxNrOfEmployees)
                        return new ActionResult(false, (int)ActionResultSave.EmployeeCannotBeAddedLicenseViolation, String.Format(GetText(91898, "Licensen tillåter inte fler anställda. Max {0} st"), license.MaxNrOfEmployees), integerValue: license.MaxNrOfEmployees);
                }
            }

            return result;
        }

        public ActionResult AddLicense(License license)
        {
            if (license == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "License");

            // There can only be one support license
            if (license.Support)
            {
                License supportLicense = GetSupportLicense();
                if (supportLicense != null)
                    return new ActionResult((int)ActionResultSave.SupportLicenseAlreadyExists);
            }

            using (CompEntities entities = new CompEntities())
            {
                return AddEntityItem(entities, license, "License");
            }
        }

        public ActionResult UpdateLicense(License license)
        {
            if (license == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11889, "Licensen hittades inte"));

            // There can only be one support license
            if (license.Support)
            {
                License supportLicense = GetSupportLicense();
                if (supportLicense != null && supportLicense.LicenseId != license.LicenseId)
                    return new ActionResult((int)ActionResultSave.SupportLicenseAlreadyExists);
            }

            using (CompEntities entities = new CompEntities())
            {
                License originalLicense = GetLicense(entities, license.LicenseId);
                if (originalLicense == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11889, "Licensen hittades inte"));

                bool sysServerHasChanged = license.SysServerId != originalLicense.SysServerId && license.SysServerId.HasValue;
                ActionResult result = UpdateEntityItem(entities, originalLicense, license, "License");
                if (result.Success && sysServerHasChanged)
                {
                    int? sysCompDbId = SysServiceManager.GetSysCompDBId();
                    if (sysCompDbId.HasValue)
                    {
                        SysServerDTO sysServer = SysServiceManager.GetSysserver(license.SysServerId.Value);
                        if (sysServer != null)
                        {
                            string domain = StringUtility.GetSubDomainFromUrl(sysServer.Url);
                            if (!string.IsNullOrEmpty(domain))
                                SoftOneIdConnector.UpdateDomain(Guid.NewGuid(), license.LicenseId, sysCompDbId.Value, domain, license.LicenseGuid);
                        }
                    }
                }
                return result;
            }
        }

        public ActionResult DeleteLicense(License license)
        {
            if (license == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "License");

            //Check relation dependencies
            if (LicenseHasCompanies(license.LicenseId))
                return new ActionResult((int)ActionResultDelete.LicenseHasCompanies);

            using (CompEntities entities = new CompEntities())
            {
                License originalLicense = GetLicense(entities, license.LicenseId);
                if (originalLicense == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "License");

                return ChangeEntityState(entities, originalLicense, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult ChangeSysServerId(License license, int dbid, int fromServerId, int toServerId, bool moveBack, List<SysServer> sysServers = null)
        {
            var message = $"Signature {license?.LicenseNr} dbid {dbid} fromServerId {fromServerId}, toServerId {toServerId} moveBack {moveBack}";

            try
            {
                if (!license.SysServerId.HasValue)
                    return new ActionResult() { Success = false, StringValue = "No SysServerId" };

                sysServers = sysServers ?? LoginManager.GetSysServers();
  
                foreach (var sys in sysServers.Where(w => w.Url.ToLower().Contains($"https://s{fromServerId}s") && w.Url.ToLower().Contains("d" + dbid.ToString())))
                {
                    var currentUrl = sys.Url.ToLower();
                    var newUrl = currentUrl.Replace($"https://s{fromServerId}s", $"https://s{toServerId}s");
                    var sysServerMatch = sysServers.FirstOrDefault(f => f.Url.ToLower() == newUrl);

                    if (sysServerMatch == null)
                        continue;

                    if (sysServerMatch.SysServerId == license.SysServerId)
                        return new ActionResult() { Success = false, StringValue = "Already on server " + message };

                    if (license.SysServerId.Value == sys.SysServerId)
                    {
                        if (moveBack && license.AccountingOfficeId != 0 && license.AccountingOfficeId != license.SysServerId)
                        {
                            license.SysServerId = license.AccountingOfficeId;
                            license.AccountingOfficeId = 0;
                        }
                        else
                        {
                            license.AccountingOfficeId = license.SysServerId ?? 0;
                            license.SysServerId = sysServerMatch.SysServerId;
                        }

                        var saveResult = UpdateLicense(license);
                        saveResult.StringValue = $"License {license.LicenseNr} moved from {sys.Url} to {sysServerMatch?.Url} " + message;
                        return saveResult;
                    }
                    else
                    {
                        continue;
                    }
                }
                return new ActionResult() { Success = false, StringValue = "No match " + message };
            }
            catch (Exception ex)
            {
                return new ActionResult(message + ex);
            }
        }

        #endregion

        #region LicenseArticle

        public List<LicenseArticle> GetLicenseArticles(int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.LicenseArticle.NoTracking();
            return GetLicenseArticles(entities, licenseId);
        }

        public List<LicenseArticle> GetLicenseArticles(CompEntities entities, int licenseId)
        {
            return (from la in entities.LicenseArticle
                    where la.License.LicenseId == licenseId
                    select la).ToList<LicenseArticle>();
        }

        public ActionResult SaveLicenseArticles(List<int> sysXEArticleIds, int licenseId, out List<int> deletedSysXEArticleIds)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Default result is unsuccessful
                ActionResult result = new ActionResult(false);

                deletedSysXEArticleIds = new List<int>();

                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        License license = GetLicense(entities, licenseId);
                        if (license == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11889, "Licensen hittades inte"));

                        //Add to new collection to be able to remove without affecting the inparameter, as it can be used by calling method again after
                        List<int> sysXEArticleIdsToSave = new List<int>();
                        sysXEArticleIdsToSave.AddRange(sysXEArticleIds);

                        #region Update/Delete

                        //Go through existing LicenseArticles
                        List<LicenseArticle> licenseArticles = GetLicenseArticles(entities, licenseId);
                        foreach (LicenseArticle licenseArticle in licenseArticles)
                        {
                            if (sysXEArticleIdsToSave.Contains(licenseArticle.SysXEArticleId))
                            {
                                //Exists in database and in input List, delete from collection
                                sysXEArticleIdsToSave.Remove(licenseArticle.SysXEArticleId);
                            }
                            else
                            {
                                //Exists in database but not in input List, delete from database
                                entities.DeleteObject(licenseArticle);

                                if (!deletedSysXEArticleIds.Contains(licenseArticle.SysXEArticleId))
                                    deletedSysXEArticleIds.Add(licenseArticle.SysXEArticleId);
                            }
                        }

                        #endregion

                        #region Add

                        //Add LicenseArticle remaining in collection
                        foreach (int sysXEArticleId in sysXEArticleIdsToSave)
                        {
                            LicenseArticle licenseArticle = new LicenseArticle()
                            {
                                License = license,
                                SysXEArticleId = sysXEArticleId,
                            };
                            SetCreatedProperties(licenseArticle);
                            entities.LicenseArticle.AddObject(licenseArticle);
                        }

                        #endregion

                        result = SaveChanges(entities);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion
    }
}
