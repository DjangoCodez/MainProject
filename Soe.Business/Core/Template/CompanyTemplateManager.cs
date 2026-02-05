using Common.Util;
using Newtonsoft.Json;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Billing.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Managers;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Evo.Cache;
using SoftOne.Soe.Business.Template.Managers;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.Template
{
    public class CompanyTemplateManager : ManagerBase
    {
        // Create a logger for use in this class
        protected readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public CompanyTemplateManager(ParameterObject parameterObject) : base(parameterObject)
        {
        }

        public bool AlreadySetup(CompEntities entities)
        {
            try
            {
                return entities.License.Any();
            }
            catch
            {
                return true;
            }
        }

        public void InitAgreement100()
        {
            var users = new List<User>();
            var userCompanyRoles = new List<UserCompanyRole>();
            var roles = new List<Role>();
            var licenseFeatures = new List<LicenseFeature>();
            var companyFeatures = new List<CompanyFeature>();
            var roleFeatures = new List<RoleFeature>();

            CompEntities newDbEntities = new CompEntities();

            if (AlreadySetup(newDbEntities))
                return;

            var templateEntities = newDbEntities.ChangeDatabase("soedemo");

            var license100 = templateEntities.License.FirstOrDefault(f => f.LicenseNr == "100" && f.State == (int)SoeEntityState.Active);

            if (license100 != null)
            {
                users = templateEntities.User.Where(w => w.LicenseId == license100.LicenseId && w.State == (int)SoeEntityState.Active).ToList();
                var userIds = users.Select(s => s.UserId).ToList();
                userCompanyRoles = templateEntities.UserCompanyRole.Include("Role").Where(w => userIds.Contains(w.UserId) && w.State == (int)SoeEntityState.Active).ToList();
                roles = userCompanyRoles.Select(s => s.Role).Distinct().ToList();
                var actorCompanyId = userCompanyRoles.GroupBy(g => g.ActorCompanyId).OrderByDescending(o => o.Count()).Select(s => s.Key).FirstOrDefault();
                licenseFeatures = templateEntities.LicenseFeature.Where(w => w.LicenseId == license100.LicenseId).ToList();
                companyFeatures = templateEntities.CompanyFeature.Where(w => w.ActorCompanyId == actorCompanyId).ToList();
                roleFeatures = templateEntities.RoleFeature.Include("Role").Where(w => w.Role.ActorCompanyId == actorCompanyId).ToList();
            }

            templateEntities.Dispose();

            using (CompEntities entities = new CompEntities())
            {
                License license = new License()
                {
                    LicenseNr = "100",
                    Name = "Agreement 100",
                    OrgNr = "100",
                    Support = true,
                    NrOfCompanies = 100,
                    MaxNrOfUsers = 100,
                    MaxNrOfEmployees = 100,
                    MaxNrOfMobileUsers = 100,
                    ConcurrentUsers = 100,
                    TerminationDate = null,
                    Created = DateTime.Now,
                    CreatedBy = "System",
                    Modified = DateTime.Now,
                    ModifiedBy = "System",
                    State = (int)SoeEntityState.Active,
                    AllowDuplicateUserLogin = false,
                    LegalName = "Agreement 100",
                    IsAccountingOffice = false,
                    AccountingOfficeName = "License create by method InitAgreement100",
                    AccountingOfficeId = 0,
                    LicenseGuid = Guid.NewGuid(),
                };
                entities.License.AddObject(license);
                entities.SaveChanges();

                Actor actor = new Actor()
                {
                    ActorType = (int)SoeActorType.Company,
                };
                entities.Actor.AddObject(actor);
                entities.SaveChanges();

                Company company = new Company()
                {
                    Name = "Agreement 100",
                    CompanyNr = 100,
                    OrgNr = "100",
                    LicenseId = license.LicenseId,
                    Created = DateTime.Now,
                    ShortName = "A100",
                    Actor = actor,
                    Template = false,
                    Global = false,
                    CreatedBy = "System",
                    CompanyGuid = Guid.NewGuid(),
                    SysCountryId = 1
                };

                entities.Company.AddObject(company);
                entities.SaveChanges();

                foreach (var oldRole in roles)
                {
                    Role role = new Role()
                    {
                        Name = oldRole.Name,
                        ActorCompanyId = company.ActorCompanyId,
                        Created = DateTime.Now,
                        CreatedBy = "System",
                        Modified = DateTime.Now,
                        ModifiedBy = "System",
                        State = (int)SoeEntityState.Active,
                        Sort = oldRole.Sort,
                        TermId = oldRole.TermId,
                    };
                    entities.Role.AddObject(role);
                    entities.SaveChanges();
                }

                var newRoles = entities.Role.Where(w => w.ActorCompanyId == company.ActorCompanyId).ToList();

                foreach (var oldUser in users)
                {
                    User user = new User()
                    {
                        // copy all but the id
                        LoginName = oldUser.LoginName,
                        idLoginGuid = oldUser.idLoginGuid,
                        Name = oldUser.Name,
                        LicenseId = license.LicenseId,
                        DefaultRoleName = oldUser.DefaultRoleName,
                        EstatusLoginId = oldUser.EstatusLoginId,
                        DefaultActorCompanyId = company.ActorCompanyId,
                        Created = DateTime.Now,
                        CreatedBy = "System",
                        Modified = DateTime.Now,
                        ModifiedBy = "System",
                        State = (int)SoeEntityState.Active,
                        BlockedFromDate = oldUser.BlockedFromDate,
                        Email = oldUser.Email,
                    };

                    entities.User.AddObject(user);
                    entities.SaveChanges();

                    foreach (var oldUserCompanyRole in userCompanyRoles.Where(w => w.UserId == oldUser.UserId && w.Role != null))
                    {
                        var role = newRoles.FirstOrDefault(f => f.Name == oldUserCompanyRole.Role.Name && f.ActorCompanyId == company.ActorCompanyId);

                        if (role?.Name == null)
                            continue;

                        UserCompanyRole userRole = new UserCompanyRole()
                        {
                            User = user,
                            Role = role,
                            Company = company,
                            Created = DateTime.Now,
                            CreatedBy = "System",
                            Modified = DateTime.Now,
                            ModifiedBy = "System",
                            State = (int)SoeEntityState.Active,
                            Default = oldUserCompanyRole.Default,
                            DateFrom = oldUserCompanyRole.DateFrom,
                            DateTo = oldUserCompanyRole.DateTo,
                        };

                        if (oldUserCompanyRole.Default)
                            user.DefaultRoleId = role.RoleId;
                    }
                }


                #region Permission Company

                foreach (var oldLicenseFeature in licenseFeatures)
                {
                    license.LicenseFeature.Add(new LicenseFeature()
                    {
                        SysFeatureId = oldLicenseFeature.SysFeatureId,
                        SysPermissionId = oldLicenseFeature.SysPermissionId,
                        Created = DateTime.Now,
                        CreatedBy = "System",
                        Modified = DateTime.Now,
                        ModifiedBy = "System",
                    });
                }

                foreach (var oldCompanyFeature in companyFeatures)
                {
                    company.CompanyFeature.Add(new CompanyFeature()
                    {
                        SysFeatureId = oldCompanyFeature.SysFeatureId,
                        SysPermissionId = oldCompanyFeature.SysPermissionId,
                        Created = DateTime.Now,
                        CreatedBy = "System",
                        Modified = DateTime.Now,
                        ModifiedBy = "System",
                    });
                }

                foreach (var oldRoleFeature in roleFeatures)
                {
                    var role = entities.Role.FirstOrDefault(f => f.Name == oldRoleFeature.Role.Name && f.ActorCompanyId == company.ActorCompanyId);

                    if (role == null)
                        continue;

                    role.RoleFeature.Add(new RoleFeature()
                    {
                        SysFeatureId = oldRoleFeature.SysFeatureId,
                        SysPermissionId = oldRoleFeature.SysPermissionId,
                        Created = DateTime.Now,
                        CreatedBy = "System",
                        Modified = DateTime.Now,
                        ModifiedBy = "System",
                    });
                }

                entities.SaveChanges();

                #endregion
            }
        }

        protected List<SysCompDBDTO> GetSysCompDbsOfSameType(bool includeProductionOnDemo = false)
        {
            return ConfigurationSetupUtil.GetSysCompDbsOfSameType(includeProductionOnDemo);
        }

        public List<TemplateCompanyItem> GetTemplateCompanyItemsFromApi(int licenseId)
        {
            CoreTemplateManager coreTemplateManager = new CoreTemplateManager(base.parameterObject);
            List<TemplateCompanyItem> templateCompanyItems = new List<TemplateCompanyItem>();
            foreach (var sysCompDb in GetSysCompDbsOfSameType(true))
            {
                if (sysCompDb.SysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                {
                    CompanyManager.GetTemplateCompanies(licenseId).ToList().ForEach(f => templateCompanyItems.Add(new TemplateCompanyItem()
                    {
                        ActorCompanyId = f.ActorCompanyId,
                        SysCompDbId = sysCompDb.SysCompDbId,
                        Name = f.Name,
                        SysCompDbName = sysCompDb.Name + (ConfigurationSetupUtil.IsTestBasedOnMachine() ? UriUtil.GetSubDomainFromUrl(sysCompDb.ApiUrl) : "" ),
                        Global = f.Global
                    }));

                    foreach (var c in coreTemplateManager.GetTemplateCompanyItems(licenseId))
                    {
                        c.Beta = true;
                        templateCompanyItems.Add(c);
                    }
                    foreach (var c in coreTemplateManager.GetGlobalTemplateCompanyItems())
                    {
                        c.Beta = true;
                        templateCompanyItems.Add(c);
                    }
                }
                else
                {
                    templateCompanyItems.AddRange(coreTemplateConnector.GetTemplateCompanyItems(sysCompDb.SysCompDbId));
                }
            }
            return templateCompanyItems;
        }

        public bool CopyFromTemplateCompany(CopyFromTemplateCompanyInputDTO inputDTO)
        {
            try
            {
                #region prereq
                LogInfo($"Using NEW CopyFromTemplateCompany for company {inputDTO.ActorCompanyId} {base.GetCompanyFromCache(inputDTO.ActorCompanyId)?.Name} from template {inputDTO.TemplateCompanyId}{inputDTO.TemplateCompanyName} User {GetUserDetails()}");
                AttestTemplateManager attestTemplateManager = new AttestTemplateManager(base.parameterObject);
                CoreTemplateManager coreTemplateManager = new CoreTemplateManager(base.parameterObject);
                EconomyTemplateManager economyTemplateManager = new EconomyTemplateManager(base.parameterObject);
                TimeTemplateManager timeTemplateManager = new TimeTemplateManager(base.parameterObject);
                BillingTemplateManager billingTemplateManager = new BillingTemplateManager(base.parameterObject);
                var destinationCompany = base.GetCompanyFromCache(inputDTO.ActorCompanyId);

                #endregion

                #region Init

                TemplateCompanyDataItem templateCompanyDataItem = new TemplateCompanyDataItem(inputDTO.SysCompDbId);

#if DEBUG
                try
                {
                    // Testing if we can get json from file
                    if (File.Exists($@"c:\temp\TemplateCompanyDataItem_{inputDTO.TemplateCompanyId}_{inputDTO.ActorCompanyId}.json"))
                    {
                        templateCompanyDataItem = JsonConvert.DeserializeObject<TemplateCompanyDataItem>(File.ReadAllText($@"c:\temp\TemplateCompanyDataItem_{inputDTO.TemplateCompanyId}_{inputDTO.ActorCompanyId}.json"));
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error reading TemplateCompanyDataItem from file {ex}");
                    templateCompanyDataItem = new TemplateCompanyDataItem(inputDTO.SysCompDbId);
                }
#endif

                if (templateCompanyDataItem.SourceActorCompanyId == 0)
                {
                    templateCompanyDataItem.UserId = base.UserId;
                    templateCompanyDataItem.SourceActorCompanyId = inputDTO.TemplateCompanyId;
                    templateCompanyDataItem.DestinationActorCompanyId = inputDTO.ActorCompanyId;
                    templateCompanyDataItem.DestinationLicenseId = destinationCompany?.LicenseId ?? 0;
                    templateCompanyDataItem.InputDTO = inputDTO;
                    templateCompanyDataItem.Update = true;


                    templateCompanyDataItem.TemplateCompanyAttestDataItem = attestTemplateManager.GetTemplateCompanyAttestDataItem(inputDTO);
                    LogInfo($"GetTemplateCompanyAttestDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                    templateCompanyDataItem.TemplateCompanyCoreDataItem = coreTemplateManager.GetTemplateCompanyCoreDataItem(inputDTO);
                    LogInfo($"GetTemplateCompanyCoreDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                    templateCompanyDataItem.TemplateCompanyEconomyDataItem = economyTemplateManager.GetTemplateCompanyEconomyDataItem(inputDTO);
                    LogInfo($"GetTemplateCompanyEconomyDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                    templateCompanyDataItem.TemplateCompanyBillingDataItem = billingTemplateManager.GetTemplateCompanyBillingDataItem(inputDTO);
                    LogInfo($"GetTemplateCompanyBillingDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                    templateCompanyDataItem.TemplateCompanyTimeDataItem = timeTemplateManager.GetTemplateCompanyTimeDataItem(inputDTO);
                    LogInfo($"GetTemplateCompanyTimeDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                    LogInfo(JsonConvert.SerializeObject(templateCompanyDataItem));
                }
                #endregion

                coreTemplateManager.CopyTemplateCompanyCoreDataItem(inputDTO, templateCompanyDataItem);
                LogInfo($"CopyTemplateCompanyCoreDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                attestTemplateManager.CopyTemplateCompanyAttestDataItem(inputDTO, templateCompanyDataItem);
                LogInfo($"CopyTemplateCompanyAttestDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                economyTemplateManager.CopyTemplateCompanyEconomyDataItem(inputDTO, templateCompanyDataItem);
                LogInfo($"CopyTemplateCompanyEconomyDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                billingTemplateManager.CopyTemplateCompanyBillingDataItem(inputDTO, templateCompanyDataItem);
                LogInfo($"CopyTemplateCompanyBillingDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                timeTemplateManager.CopyTemplateCompanyTimeDataItem(inputDTO, templateCompanyDataItem);
                LogInfo($"CopyTemplateCompanyTimeDataItem handled for {inputDTO.TemplateCompanyId} and {inputDTO.ActorCompanyId}");
                coreTemplateManager.CopyAllSettings(templateCompanyDataItem);
                CopyExternalCodes(templateCompanyDataItem, coreTemplateManager);
                var company = base.GetCompanyFromCache(inputDTO.ActorCompanyId);
                EvoSettingCacheInvalidationConnector.InvalidateCacheUserCompanySetting(company.LicenseId, inputDTO.ActorCompanyId, null);

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error while copying from template company {inputDTO.TemplateCompanyId} to {inputDTO.ActorCompanyId} {ex}");
                return false;
            }
        }
        private void CopyExternalCodes(TemplateCompanyDataItem templateCompanyDataItem, CoreTemplateManager coreTemplateManager)
        {
            using (CompEntities entities = new CompEntities())
            {
                var existingExternalCodes = coreTemplateManager.GetExternalCodeCopyItems(templateCompanyDataItem.DestinationActorCompanyId);
                var counter = coreTemplateManager.CopyCompanyExternalCodes(entities, templateCompanyDataItem, existingExternalCodes, TermGroup_CompanyExternalCodeEntity.EmployeeGroup);
                counter += coreTemplateManager.CopyCompanyExternalCodes(entities, templateCompanyDataItem, existingExternalCodes, TermGroup_CompanyExternalCodeEntity.PayrollGroup);
                counter += coreTemplateManager.CopyCompanyExternalCodes(entities, templateCompanyDataItem, existingExternalCodes, TermGroup_CompanyExternalCodeEntity.Role);
                counter += coreTemplateManager.CopyCompanyExternalCodes(entities, templateCompanyDataItem, existingExternalCodes, TermGroup_CompanyExternalCodeEntity.AttestRole);

                if (counter > 0)
                    entities.SaveChanges();
            }
        }
        public void CreateChildCopyItemRequest(TemplateCompanyDataItem item, ChildCopyItemRequestType templateCompanyCopy, List<int> ids)
        {
            if (ids.IsNullOrEmpty())
                return;

            #region prereq

            item.ChildCopyItemRequest = new ChildCopyItemRequest() { Ids = ids, ChildCopyItemRequestType = templateCompanyCopy };

            #endregion

            if (templateCompanyCopy == ChildCopyItemRequestType.TimeDeviationCause)
            {
                if (!item.TemplateCompanyTimeDataItem.TimeDeviationCauseCopyItems.Any())
                    item.TemplateCompanyTimeDataItem.TimeDeviationCauseCopyItems = new TimeTemplateManager(base.parameterObject).GetTimeDeviationCauseCopyItems(item.SourceActorCompanyId);

                new TimeTemplateManager(base.parameterObject).CopyTimeDeviationCausesFromTemplateCompany(item);
            }

            item.ChildCopyItemRequest = null;
        }

        public ActionResult LogCopyError(string type, string pk, int id, string codeOrNr, string name, TemplateCompanyDataItem item, Exception ex = null, bool add = false, bool update = false)
        {
            return LogCopyError(type, pk, id, codeOrNr, name, item.SourceActorCompanyId, item.DestinationActorCompanyId, ex: ex, add: add, update: update);
        }

        public ActionResult LogCopyError(string type, string pk, int id, string codeOrNr, string name, int templateCompanyId, int newCompanyId, Exception ex = null, bool add = false, bool update = false)
        {
            //Action
            string action = "";
            if (add)
                action = "add";
            else if (update)
                action = "update";

            StringBuilder message = new StringBuilder();
            message.Append($"Error while copying {type}. ");
            if (!String.IsNullOrEmpty(codeOrNr))
                message.Append($" {codeOrNr}.");
            if (!String.IsNullOrEmpty(name))
                message.Append($" {name}.");
            if (!String.IsNullOrEmpty(action))
                message.Append($"Could not {action} {type}. ");
            message.Append($" {pk}:{id}");

            return LogCopyError(message.ToString(), (int)ActionResultSave.Unknown, templateCompanyId, newCompanyId, ex);
        }

        public ActionResult LogCopyError(string type, TemplateCompanyDataItem item, Exception ex = null, bool saved = false)
        {
            return LogCopyError(type, item.SourceActorCompanyId, item.DestinationActorCompanyId, ex: ex, saved: saved);
        }

        public ActionResult LogCopyError(string type, int templateCompanyId, int newCompanyId, Exception ex = null, bool saved = false)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Error while copying {type}. ");
            if (saved)
                message.Append("Could not save");

            return LogCopyError(message.ToString(), (int)ActionResultSave.Unknown, templateCompanyId, newCompanyId, ex);
        }

        public ActionResult LogCopyError(string message, int errorNumber, int templateCompanyId, int newCompanyId, Exception ex = null)
        {
            base.LogError(new SoeCopyCompanyException(message, templateCompanyId, newCompanyId, ex, this.ToString()), this.log);

            return new ActionResult(ex)
            {
                ErrorNumber = errorNumber,
                ErrorMessage = message,
            };
        }
    }
}
