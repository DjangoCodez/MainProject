using SoftOne.Soe.Business.Core.Template;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class CompanyManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public CompanyManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Company

        public List<Company> GetCompanies(bool loadLicense = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompanies(entities, loadLicense);
        }

        public List<Company> GetCompanies(CompEntities entities, bool loadLicense = false)
        {
            if (loadLicense)
                return entities.Company.Include("License").ToList();
            else
                return entities.Company.ToList();
        }

        public List<Company> GetCompanies(IEnumerable<int> companyIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompanies(entities, companyIds);
        }

        public List<Company> GetCompanies(CompEntities entities, IEnumerable<int> companyIds)
        {
            if (companyIds.IsNullOrEmpty())
                return new List<Company>();

            companyIds = companyIds.Distinct();

            return (from c in entities.Company
                    where companyIds.Contains(c.ActorCompanyId)
                    select c).ToList();
        }

        public Dictionary<int, string> GetCompaniesDict(CompEntities entities, IEnumerable<int> companyIds)
        {
            return GetCompanies(entities, companyIds).ToDictionary(k => k.ActorCompanyId, v => v.Name);
        }

        public List<Company> GetTemplateCompanies(int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return (from c in entities.Company
                    where c.Global || (c.Template && c.LicenseId == licenseId)
                    orderby c.Name
                    select c).ToList();
        }

        public List<SmallGenericType> GetTemplateCompaniesDict(int licenseId, bool addEmpty, bool useLabelValue = false)
        {
            List<SmallGenericType> companies = new List<SmallGenericType>();

            var coretest = new CompanyTemplateManager(base.parameterObject);
            var comps = coretest.GetTemplateCompanyItemsFromApi(licenseId);

            if (addEmpty)
            {
                companies.Add(new SmallGenericType()
                {
                    Id = 0,
                    Name = useLabelValue ? GetText(5333, 1006, "Välj mallföretag") : " ",
                });
            }

            foreach (var c in comps.OrderBy(c => c.Global))
            {
                if (c.SysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                {
                    if (c.Beta)
                    {
                        var sgt = new SmallGenericType()
                        {
                            Id = GenerateUniqueKey(c.ActorCompanyId, c.SysCompDbId),
                            Name = "(Beta) " + c.Name + $" ({GetText(2138, "Global")} d{c.SysCompDbId})"
                        };

                        if (ConfigurationSetupUtil.GetSiteType() == TermGroup_SysPageStatusSiteType.Test)
                            sgt.Name = "(Beta) " + c.Name + $" ({GetText(2138, "Global")} {c.SysCompDbName})";

                        companies.Add(sgt);
                    }
                    else
                        companies.Add(new SmallGenericType()
                        {
                            Id = c.ActorCompanyId,
                            Name = c.Name + (c.Global ? "(" + GetText(2138, "Global") + ")" : ""),
                        });
                }
                else
                {
                    var sgt = new SmallGenericType()
                    {
                        Id = GenerateUniqueKey(c.ActorCompanyId, c.SysCompDbId),
                        Name = "(Beta) " + c.Name + $" ({GetText(2138, "Global")} d{c.SysCompDbId})"
                    };

                    if (ConfigurationSetupUtil.GetSiteType() == TermGroup_SysPageStatusSiteType.Test)
                        sgt.Name = "(Beta) " + c.Name + $" ({GetText(2138, "Global")} {c.SysCompDbName})";

                    companies.Add(sgt);
                }
            }

            return companies.OrderBy(o => o.Name).ToList();
        }

        public static void AddKeyToCompanyInCache(int actorCompanyId, int sysCompDbId, string key)
        {
            var uniqueKey = $"KeyCompanyCache_{key}";
            Tuple<int, int> storedObject = new Tuple<int, int>(actorCompanyId, sysCompDbId);
            BusinessMemoryCache<Tuple<int, int>>.Set(uniqueKey, storedObject, 60 * 10);
        }

        public static Tuple<int, int> GetKeyFromCompanyInCache(int index)
        {
            var uniqueKey = $"KeyCompanyCache_{index}";
            var value = BusinessMemoryCache<Tuple<int, int>>.Get(uniqueKey);

            if (value == null)
                return null;

            return value;
        }

        public Dictionary<int, string> GetCompaniesByUserDict(int userId, int licenseId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompaniesByUser(entities, userId, licenseId).ToDictionary(c => c.ActorCompanyId, c => c.Name);
        }

        public List<Company> GetCompaniesByUser(int userId, int licenseId, bool loadRoles = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompaniesByUser(entities, userId, licenseId, loadRoles);
        }
        public List<Company> GetCompaniesByUser(CompEntities entities, int userId, int licenseId, bool loadRoles = false)
        {
            DateTime date = DateTime.Today;
            if (loadRoles)
            {
                List<int> companyIds = (from ucr in entities.UserCompanyRole
                                        where ucr.User.UserId == userId &&
                                        ucr.Company.LicenseId == licenseId &&
                                        ucr.User.State != (int)SoeEntityState.Deleted &&
                                        ucr.Company.State == (int)SoeEntityState.Active &&
                                        ucr.State == (int)SoeEntityState.Active &&
                                        (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                                        (!ucr.DateTo.HasValue || ucr.DateTo >= date)
                                        select ucr.ActorCompanyId).Distinct().ToList();

                return (from c in entities.Company.Include("Role")
                        where companyIds.Contains(c.ActorCompanyId)
                        orderby c.Name
                        select c).ToList();
            }
            else
            {
                return (from ucr in entities.UserCompanyRole
                        where ucr.User.UserId == userId &&
                        ucr.Company.LicenseId == licenseId &&
                        ucr.User.State != (int)SoeEntityState.Deleted &&
                        ucr.Company.State == (int)SoeEntityState.Active &&
                        ucr.State == (int)SoeEntityState.Active &&
                        (!ucr.DateFrom.HasValue || ucr.DateFrom <= date) &&
                        (!ucr.DateTo.HasValue || ucr.DateTo >= date)
                        orderby ucr.Company.Name
                        select ucr.Company).Distinct().ToList();
            }
        }

        public List<Company> GetCompaniesByLicense(int licenseId, bool excludeDemo = false, bool loadRoles = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompaniesByLicense(entities, licenseId, excludeDemo, loadRoles);
        }

        public List<Company> GetCompaniesByLicense(CompEntities entities, int licenseId, bool excludeDemo = false, bool loadRoles = false)
        {
            var query = (from c in entities.Company.Include("License")
                         where c.LicenseId == licenseId &&
                         (!excludeDemo || !c.Demo) &&
                         c.State == (int)SoeEntityState.Active
                         orderby c.Name
                         select c);

            if (loadRoles)
                query = (IOrderedQueryable<Company>)query.Include("Role");

            return query.ToList();
        }

        public List<Company> GetCompaniesByLicense(string licenseNr, bool onlyTemplates)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompaniesByLicense(entities, licenseNr, onlyTemplates);
        }

        public List<int> GetSupportLoginAllowedCompanyIdsByLicenseId(int licenseId)
        {
            List<int> ids = new List<int>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<Company> companies = GetCompaniesByLicense(entitiesReadOnly, licenseId);
            foreach (var company in companies)
            {
                if (company.IsSupportLoginAllowed())
                    ids.Add(company.ActorCompanyId);
            }

            return ids;
        }

        public List<Company> GetCompaniesByLicense(CompEntities entities, string licenseNr, bool onlyTemplates)
        {
            if (onlyTemplates)
            {
                return (from c in entities.Company
                            .Include("License")
                        where c.License.LicenseNr == licenseNr &&
                        c.Template &&
                        c.State == (int)SoeEntityState.Active
                        orderby c.Name
                        select c).ToList();
            }
            else
            {
                return (from c in entities.Company
                             .Include("License")
                        where c.License.LicenseNr == licenseNr &&
                        c.State == (int)SoeEntityState.Active
                        orderby c.Name
                        select c).ToList();
            }
        }

        public List<Company> GetGlobalTemplateCompanies()
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetGlobalTemplateCompanies(entitiesReadOnly);
        }

        public List<Company> GetGlobalTemplateCompanies(CompEntities entities)
        {
            return (from c in entities.Company
                    where c.Global &&
                    c.State == (int)SoeEntityState.Active
                    orderby c.Name
                    select c).ToList();
        }

        public List<Company> GetCompaniesBySearch(string search)
        {
            search = search.ToLower();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return (from c in entities.Company
                    where (c.Name.ToLower().Contains(search) || c.License.LicenseNr == search) &&
                    c.State == (int)SoeEntityState.Active
                    orderby c.Name
                    select c).ToList();
        }

        public List<CompanySearchResultDTO> GetCompaniesBySearch(CompanySearchFilterDTO filter)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompaniesBySearch(entities, filter);
        }
        public List<CompanySearchResultDTO> GetCompaniesBySearch(CompEntities entities, CompanySearchFilterDTO filter)
        {
            IQueryable<Company> query = (from c in entities.Company
                                         where c.State == (int)SoeEntityState.Active
                                         orderby c.Name
                                         select c
                                         );
            bool filterUsed = false;

            if (!string.IsNullOrEmpty(filter.BankAccountNr) && !string.IsNullOrEmpty(filter.BankAccountBIC))
            {

                filterUsed = true;

                if (filter.BankConnected.HasValue)
                {
                    if (filter.BankAccountType != TermGroup_SysPaymentType.Unknown)
                    {
                        query = query.Where(c => c.Actor.PaymentInformation.Any(x => x.PaymentInformationRow.Any(y => y.PaymentNr == filter.BankAccountNr && y.BIC == filter.BankAccountBIC && y.SysPaymentTypeId == (int)filter.BankAccountType && y.BankConnected == filter.BankConnected)));
                    }
                    else
                    {
                        query = query.Where(c => c.Actor.PaymentInformation.Any(x => x.PaymentInformationRow.Any(y => y.PaymentNr == filter.BankAccountNr && y.BIC == filter.BankAccountBIC && y.BankConnected == filter.BankConnected)));
                    }
                }
                else
                {
                    if (filter.BankAccountType != TermGroup_SysPaymentType.Unknown)
                    {
                        query = query.Where(c => c.Actor.PaymentInformation.Any(x => x.PaymentInformationRow.Any(y => y.PaymentNr == filter.BankAccountNr && y.BIC == filter.BankAccountBIC && y.SysPaymentTypeId == (int)filter.BankAccountType)));
                    }
                    else
                    {
                        query = query.Where(c => c.Actor.PaymentInformation.Any(x => x.PaymentInformationRow.Any(y => y.PaymentNr == filter.BankAccountNr && y.BIC == filter.BankAccountBIC)));
                    }
                }
            }

            if (filter.Demo.HasValue)
            {
                query = query.Where(x => x.Demo == filter.Demo.Value);
            }

            if (!string.IsNullOrEmpty(filter.OrgNr))
            {
                filterUsed = true;
                var orgWithoutDash = filter.OrgNr.Replace("-", "");
                query = query.Where(c => c.OrgNr.Replace("-", "") == orgWithoutDash);
            }

            if (!string.IsNullOrEmpty(filter.NameOrLicense))
            {
                filterUsed = true;
                query = query.Where(c => c.Name.ToLower().Contains(filter.NameOrLicense) || c.License.LicenseNr == filter.NameOrLicense);
            }

            if (filterUsed)
            {
                return query.Select(c =>
                         new CompanySearchResultDTO
                         {
                             ActorCompanyId = c.ActorCompanyId,
                             Name = c.Name,
                             CompanyGuid = c.CompanyGuid.ToString(),
                         }
                        ).ToList();
            }
            else
            {
                return new List<CompanySearchResultDTO>();
            }
        }

        public List<SmallGenericType> GetChildCompaniesByLicenseDict(int licenseId, int actorCompanyId, bool addEmptyRow, bool addEmptyRowAsAll)
        {
            List<SmallGenericType> dict = new List<SmallGenericType>();

            if (addEmptyRow)
                dict.Add(new SmallGenericType(0, " "));
            else if (addEmptyRowAsAll)
                dict.Add(new SmallGenericType(0, GetText(4366, "Alla")));

            List<Company> companies = GetCompaniesByLicense(licenseId).ToList();

            foreach (Company company in companies)
            {
                if (company != null && company.ActorCompanyId != actorCompanyId && !dict.Any(c => c.Id == company.ActorCompanyId))
                    dict.Add(new SmallGenericType(company.ActorCompanyId, company.Name));
            }

            return dict;
        }

        public List<CompanyRolesDTO> GetCompanyRolesDTO(bool isAdmin, int userId, int licenseId)
        {
            List<CompanyRolesDTO> dtos = new List<CompanyRolesDTO>();

            //Admin can see all Companies in License
            List<Company> companies = isAdmin ? GetCompaniesByLicense(licenseId) : GetCompaniesByUser(userId, licenseId);
            foreach (Company company in companies)
            {
                CompanyRolesDTO dto = new CompanyRolesDTO()
                {
                    ActorCompanyId = company.ActorCompanyId,
                    CompanyName = company.Name,
                    Roles = new List<UserCompanyRoleDTO>(),
                    AttestRoles = new List<CompanyAttestRoleDTO>(),
                };

                // Roles
                List<Role> roles = RoleManager.GetRolesByCompany(company.ActorCompanyId);
                foreach (Role role in roles)
                {
                    dto.Roles.Add(new UserCompanyRoleDTO()
                    {
                        RoleId = role.RoleId,
                        Name = role.Name
                    });
                }

                // Attest roles
                List<AttestRole> attestRoles = AttestManager.GetAttestRoles(company.ActorCompanyId, includeInactive: true).Where(x => x.Module != (int)SoeModule.Manage).ToList();
                foreach (AttestRole attestRole in attestRoles)
                {
                    dto.AttestRoles.Add(new CompanyAttestRoleDTO()
                    {
                        AttestRoleId = attestRole.AttestRoleId,
                        Name = attestRole.Name,
                        ModuleName = AttestManager.GetAttestRoleModuleName(attestRole.Module),
                        DefaultMaxAmount = attestRole.DefaultMaxAmount,
                        ShowAllCategories = attestRole.ShowAllCategories,
                        ShowUncategorized = attestRole.ShowUncategorized,
                        ShowTemplateSchedule = attestRole.ShowTemplateSchedule,
                        AlsoAttestAdditionsFromTime = attestRole.AlsoAttestAdditionsFromTime,
                        HumanResourcesPrivacy = attestRole.HumanResourcesPrivacy,
                        IsExecutive = attestRole.IsExecutive,
                        AttestByEmployeeAccount = attestRole.AttestByEmployeeAccount,
                        StaffingByEmployeeAccount = attestRole.StaffingByEmployeeAccount,
                        State = (SoeEntityState)attestRole.State,
                    });
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public Dictionary<Company, List<Role>> GetValidCompanyAndRoles(bool isAdmin, int userId, int licenseId)
        {
            var dict = new Dictionary<Company, List<Role>>();

            //Admin can see all Companies in License
            List<Company> companies;
            if (isAdmin)
                companies = GetCompaniesByLicense(licenseId);
            else
                companies = GetCompaniesByUser(userId, licenseId);

            foreach (Company company in companies)
            {
                List<Role> roles = RoleManager.GetRolesByCompany(company.ActorCompanyId);
                dict.Add(company, roles);
            }

            return dict;
        }

        public Dictionary<Company, List<AttestRole>> GetValidCompanyAndAttestRoles(bool isAdmin, int userId, int licenseId)
        {
            var dict = new Dictionary<Company, List<AttestRole>>();

            //Admin can see all Companies in License
            List<Company> companies;
            if (isAdmin)
                companies = GetCompaniesByLicense(licenseId);
            else
                companies = GetCompaniesByUser(userId, licenseId);

            foreach (Company company in companies)
            {
                List<AttestRole> attestRoles = AttestManager.GetAttestRolesAndRoleUser(company.ActorCompanyId);
                dict.Add(company, attestRoles);
            }

            return dict;
        }

        public List<int> GetActiveCompanyIds()
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return (from c in entities.Company
                    where c.State == (int)SoeEntityState.Active
                    select c.ActorCompanyId).ToList();
        }

        public Company GetCompany(int actorCompanyId, bool loadLicense = false, bool loadEdiConnection = false, bool loadActorAndContact = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompany(entities, actorCompanyId, loadLicense, loadEdiConnection, loadActorAndContact);
        }

        public Company GetCompany(CompEntities entities, int actorCompanyId, bool loadLicense = false, bool loadEdiConnection = false, bool loadActorAndContact = false)
        {
            var query = (from c in entities.Company
                         where c.ActorCompanyId == actorCompanyId &&
                         c.State == (int)SoeEntityState.Active
                         select c);

            if (loadLicense)
                query = query.Include("License");
            if (loadEdiConnection)
                query = query.Include("EdiConnection");
            if (loadActorAndContact)
                query = query.Include("Actor.Contact");

            return query.FirstOrDefault();
        }

        public CompanyDTO GetCompanyDTO(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompanyDTO(entities, actorCompanyId);
        }
        public CompanyDTO GetCompanyDTO(CompEntities entities, int actorCompanyId)
        {
            return (from c in entities.Company
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select new CompanyDTO
                    {
                        ActorCompanyId = c.ActorCompanyId,
                        _companyGuid = c.CompanyGuid,
                        Name = c.Name,
                        Number = c.CompanyNr,
                        ShortName = c.ShortName,
                        VatNr = c.VatNr,
                        LicenseId = c.LicenseId,
                        _licenseGuid = c.License.LicenseGuid,
                        OrgNr = c.OrgNr,
                        SysCountryId = c.SysCountryId,
                        AllowSupportLogin = c.AllowSupportLogin ?? false,
                        AllowSupportLoginTo = c.AllowSupportLoginTo,
                    }
                    ).FirstOrDefault();
        }

        public CompanyEditDTO GetCompanyEdit(int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                var comp = (from c in entities.Company
                            .Include("License")
                            .Include("Actor.Contact.ContactECom")
                            .Include("Actor.Contact.ContactAddress.ContactAddressRow")
                            where c.ActorCompanyId == actorCompanyId &&
                                           c.State == (int)SoeEntityState.Active
                            select c).FirstOrDefault();

                var company = new CompanyEditDTO
                {
                    ActorCompanyId = comp.ActorCompanyId,
                    Number = comp.CompanyNr,
                    Name = comp.Name,
                    ShortName = comp.ShortName,
                    OrgNr = comp.OrgNr,
                    VatNr = comp.VatNr,
                    CompanyTaxSupport = comp.CompanyTaxSupport ?? false,
                    Language = comp.SysCountryId.HasValue ? (TermGroup_Languages)comp.SysCountryId.Value : TermGroup_Languages.Unknown,
                    AllowSupportLogin = comp.AllowSupportLogin == true,
                    AllowSupportLoginTo = comp.AllowSupportLoginTo ?? DateTime.MinValue,
                    LicenseId = comp.LicenseId,
                    LicenseNr = comp.License?.LicenseNr ?? string.Empty,
                    LicenseSupport = comp.License?.Support ?? false,
                    Template = comp.Template,
                    Global = comp.Global,
                    SysCountryId = comp.SysCountryId,
                    TimeSpotId = comp.TimeSpotId,
                    MaxNrOfSMS = comp.MaxNrOfSMS,
                    Created = comp.Created,
                    CreatedBy = comp.CreatedBy,
                    Modified = comp.Modified,
                    ModifiedBy = comp.ModifiedBy,
                    Demo = comp.Demo,
                };

                // Currencies
                company.BaseSysCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);
                company.BaseEntCurrencyId = CountryCurrencyManager.GetCompanyBaseEntSysCurrencyId(company.ActorCompanyId);

                // API
                company.CompanyApiKey = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, base.UserId, company.ActorCompanyId, 0);

                // Payment Information
                company.PaymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, actorCompanyId, true, false).ToDTO(true);
                if (company.PaymentInformation == null)
                    company.PaymentInformation = new PaymentInformationDTO();

                // Contacts
                if (comp.Actor != null && comp.Actor.Contact != null)
                    company.ContactAddresses = ExtensionsComp.GetContactAddressItems(comp.Actor.Contact.FirstOrDefault(c => c.State == (int)SoeEntityState.Active));

                // EDI
                var xeEdiFeature = FeatureManager.GetCompanyFeature(entities, actorCompanyId, (int)Feature.Billing_Import_XEEdi);
                company.IsEdiGOActivated = xeEdiFeature != null && xeEdiFeature.SysPermissionId == (int)Permission.Modify;

                var ediItem = EdiManager.GetCompanyEdi(entities, actorCompanyId, type: TermGroup_CompanyEdiType.Symbrio, onlyActive: true);
                if (ediItem != null && ediItem.State == (int)SoeEntityState.Active)
                {
                    company.EdiUsername = ediItem.Username;
                    company.EdiPassword = ediItem.Password;
                    company.IsEdiActivated = (!ediItem.Password.IsNullOrEmpty() && !ediItem.Username.IsNullOrEmpty());
                    company.EdiActivated = ediItem.Created;
                    company.EdiActivatedBy = ediItem.CreatedBy;
                    company.Modified = ediItem.Modified;
                    company.ModifiedBy = ediItem.ModifiedBy;
                }

                return company;
            }
        }

        public Company GetCompanyByRoleId(int roleId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompanyByRoleId(entities, roleId);
        }

        public Company GetCompanyByRoleId(CompEntities entities, int roleId)
        {
            return (from r in entities.Role
                    where r.RoleId == roleId &&
                    r.Company.State == (int)SoeEntityState.Active
                    select r.Company).FirstOrDefault();
        }

        public Company GetCompanyFromTimeSpot(int timeSpotId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompanyFromTimeSpot(entities, timeSpotId);
        }

        public Company GetCompanyFromTimeSpot(CompEntities entities, int timeSpotId)
        {
            return (from c in entities.Company
                    where c.TimeSpotId == timeSpotId &&
                    c.State == (int)SoeEntityState.Active
                    select c).FirstOrDefault();
        }

        public Company GetPrevNextCompany(int userId, int actorCompanyId, int licenseId, SoeFormMode mode)
        {
            Company company = null;
            List<Company> companies = GetCompaniesByUser(userId, licenseId);

            if (mode == SoeFormMode.Next)
            {
                company = (from c in companies
                           where c.ActorCompanyId > actorCompanyId
                           orderby c.ActorCompanyId ascending
                           select c).FirstOrDefault<Company>();
            }
            else if (mode == SoeFormMode.Prev)
            {
                company = (from c in companies
                           where c.ActorCompanyId < actorCompanyId
                           orderby c.ActorCompanyId descending
                           select c).FirstOrDefault<Company>();
            }

            return company;
        }

        public CompanyDTO GetSoeCompany(int actorCompanyId)
        {
            return GetSoeCompany(GetCompany(actorCompanyId, loadLicense: true));
        }

        public CompanyDTO GetSoeCompany(Company company)
        {
            return company?.ToCompanyDTO();
        }

        public string GetCompanyName(int actorCompanyId, bool shortName = false, bool includeLicenseName = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetCompanyName(entities, actorCompanyId, shortName, includeLicenseName);
        }

        public string GetCompanyName(CompEntities entities, int actorCompanyId, bool shortName = false, bool includeLicenseName = false)
        {
            string name = "";
            Company company = null;

            if (includeLicenseName)
            {
                company = (from c in entities.Company
                            .Include("License")
                           where c.ActorCompanyId == actorCompanyId
                           select c).FirstOrDefault();
            }
            else
            {
                company = (from c in entities.Company
                           where c.ActorCompanyId == actorCompanyId
                           select c).FirstOrDefault();
            }

            if (company != null)
            {
                name = shortName ? company.ShortName : company.Name;
                if (includeLicenseName && company.License != null)
                    name += String.Format(" - {0}", company.License.Name);
            }

            return name;
        }

        public string GetNextCompanyNr(int licenseId)
        {
            int lastNr = 0;

            List<Company> companies = GetCompaniesByLicense(licenseId);
            if (companies.Any())
            {
                Company company = companies.Last();
                if (company != null && company.CompanyNr.HasValue)
                    lastNr = company.CompanyNr.Value;

                // If unable to parse, numeric values are not used
                if (lastNr == 0)
                    return String.Empty;
            }

            lastNr++;

            // Check that number is not used
            if (companies.Any(c => c.CompanyNr == lastNr))
                return String.Empty;

            return lastNr.ToString();
        }

        public int GetCompanySysCountryId(int actorCompanyId, TermGroup_Languages defaultCountry = TermGroup_Languages.Swedish)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanySysCountryId(entities, actorCompanyId, defaultCountry);
        }

        public int GetCompanySysCountryId(CompEntities entities, int actorCompanyId, TermGroup_Languages defaultCountry = TermGroup_Languages.Swedish)
        {
            var sysCountryId = (from c in entities.Company
                                where c.ActorCompanyId == actorCompanyId &&
                                c.State == (int)SoeEntityState.Active
                                select c.SysCountryId).FirstOrDefault();

            return sysCountryId ?? (int)defaultCountry;
        }

        public int? GetActorCompanyIdFromApiKey(string apiKey)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetActorCompanyIdFromApiKey(entities, apiKey);
        }

        public int? GetActorCompanyIdFromApiKey(CompEntities entities, string apiKey)
        {
            UserCompanySetting setting = SettingManager.GetCompanySettingWithUniqueStringValue(entities, (int)CompanySettingType.CompanyAPIKey, apiKey);
            return setting != null ? setting.ActorCompanyId : (int?)null;
        }
        public int? GetActorCompanyIdFromCompanyGuid(string companyGuid)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetActorCompanyIdFromCompanyGuid(entities, companyGuid);
        }

        public int? GetActorCompanyIdFromCompanyGuid(CompEntities entities, string companyGuid)
        {
            return (from c in entities.Company
                    where c.CompanyGuid.ToString() == companyGuid &&
                    c.State == (int)SoeEntityState.Active
                    select c.ActorCompanyId).FirstOrDefault();
        }

        public string GetCompanyGuid(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetCompanyGuid(entities, actorCompanyId);
        }

        public string GetCompanyGuid(CompEntities entities, int actorCompanyId)
        {
            return (from c in entities.Company
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c.CompanyGuid.ToString()).FirstOrDefault();
        }

        public int GetNrOfCompaniesByLicense(CompEntities entities, int licenseId, bool excludeDemo = false)
        {
            List<Company> companies = GetCompaniesByLicense(entities, licenseId, excludeDemo);
            return companies.Count;
        }

        public bool CompanyExist(int licenseId, string orgNr, int? discardCompanyId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return CompanyExist(entities, licenseId, orgNr, discardCompanyId);
        }

        public bool CompanyExist(CompEntities entities, int licenseId, string orgNr, int? discardCompanyId = null)
        {
            return (from c in entities.Company
                    where c.LicenseId == licenseId &&
                    c.OrgNr == orgNr &&
                    (!discardCompanyId.HasValue || c.ActorCompanyId != discardCompanyId.Value) &&
                    c.State == (int)SoeEntityState.Active
                    select c).Any();
        }

        public bool CompanyHasRoles(CompEntities entities, int actorCompanyId)
        {
            int systemadmin = 1;

            return (from r in entities.Role
                    where r.ActorCompanyId == actorCompanyId &&
                    r.TermId != systemadmin &&
                    r.State == (int)SoeEntityState.Active
                    select r).Any();
        }

        public bool CompanyHasRoles(Company company)
        {
            if (company == null)
                return false;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int systemadmin = 1;

            return (from r in entitiesReadOnly.Role
                    where r.ActorCompanyId == company.ActorCompanyId &&
                    r.TermId != systemadmin &&
                    r.State == (int)SoeEntityState.Active
                    select r).Any();
        }

        public bool UserInCompany(int userId, int licenseId, int actorCompanyId)
        {
            List<Company> companies = GetCompaniesByUser(userId, licenseId);
            int counter = companies.Count(c => c.ActorCompanyId == actorCompanyId);
            if (counter > 0)
                return true;
            return false;
        }

        public bool IsTemplateCompany(int actorCompanyId)
        {
            return GetCompany(actorCompanyId)?.Template ?? false;
        }

        public ActionResult AddCompany(Company company, int licenseId)
        {
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

            using (CompEntities entities = new CompEntities())
            {
                Actor actor = new Actor()
                {
                    ActorType = (int)SoeActorType.Company,
                };
                actor.Created = DateTime.Now;
                actor.CreatedBy = GetUserDetails();

                company.CompanyGuid = Guid.NewGuid();

                company.License = LicenseManager.GetLicense(entities, licenseId);
                if (company.License == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11889, "Licensen hittades inte"));

                company.Actor = actor;
                if (company.Actor == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Actor");

                var result = AddEntityItem(entities, company, "Company");
                if (!result.Success)
                    return result;

                result = VoucherManager.AddTemplateVoucherSeriesType(company.ActorCompanyId);
                if (!result.Success)
                    return result;

                return result;
            }
        }

        public ActionResult UpdateCompany(Company company)
        {
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

            using (CompEntities entities = new CompEntities())
            {
                Company originalCompany = GetCompany(entities, company.ActorCompanyId);
                if (originalCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return UpdateEntityItem(entities, originalCompany, company, "Company");
            }
        }

        /// <summary>
        /// Sets a Company to Deleted
        /// </summary>
        /// <param name="company">Company to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteCompany(int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                //Check relation dependencies
                if (CompanyHasRoles(entities, actorCompanyId))
                    return new ActionResult((int)ActionResultDelete.CompanyHasRoles, GetText(1285, "Företag kunde inte tas bort, kontrollera att det inte används"));

                var originalCompany = GetCompany(entities, actorCompanyId);
                if (originalCompany == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Company");

                SetModifiedProperties(originalCompany);
                return ChangeEntityState(entities, originalCompany, SoeEntityState.Deleted, true);
            }
        }

        /// <summary>
        /// Sets a Company to Deleted
        /// </summary>
        /// <param name="company">Company to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteCompany(Company company)
        {
            if (company == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "Company");

            //Check relation dependencies
            if (CompanyHasRoles(company))
                return new ActionResult((int)ActionResultDelete.CompanyHasRoles);

            using (CompEntities entities = new CompEntities())
            {
                Company originalCompany = GetCompany(entities, company.ActorCompanyId);
                if (originalCompany == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Company");

                return ChangeEntityState(entities, originalCompany, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult SaveCompany(CompanyEditDTO companyInput, int actorCompanyId)
        {
            if (companyInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

            // Default result is successful
            ActionResult result = new ActionResult();

            // Load currencys
            SysCurrency baseSysCurrency = null;
            if (companyInput.BaseSysCurrencyId > 0)
                baseSysCurrency = CountryCurrencyManager.GetSysCurrency(companyInput.BaseSysCurrencyId, true);

            SysCurrency baseEntSysCurrency = null;
            if (companyInput.BaseEntCurrencyId > 0 && companyInput.BaseEntCurrencyId != companyInput.BaseSysCurrencyId)
                baseEntSysCurrency = CountryCurrencyManager.GetSysCurrency(companyInput.BaseEntCurrencyId, true);

            using (var entities = new CompEntities())
            {
                int companyId = companyInput.ActorCompanyId;

                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Company

                        License license = LicenseManager.GetLicense(entities, companyInput.LicenseId);
                        if (license == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, "License");

                        // Validate
                        result = LicenseManager.ValidateCompany(entities, license, companyInput.ActorCompanyId, companyInput.OrgNr, companyInput.Demo);
                        if (!result.Success)
                        {
                            if (result.ErrorNumber == (int)ActionResultSave.CompanyExists)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(1240, "Företag finns redan"));
                            else
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2090, "Licensen tillåter inte fler företag"));
                        }

                        Company company = companyInput.ActorCompanyId != 0 ? GetCompany(entities, companyInput.ActorCompanyId, true, true, true) : null;
                        if (company == null)
                        {
                            #region Company Add

                            company = new Company
                            {
                                CompanyNr = companyInput.Number,
                                Name = companyInput.Name,
                                ShortName = companyInput.ShortName,
                                OrgNr = companyInput.OrgNr,
                                VatNr = companyInput.VatNr,
                                CompanyTaxSupport = companyInput.CompanyTaxSupport,
                                Template = companyInput.Template,
                                Global = companyInput.Global,
                                Demo = companyInput.Demo,
                                SysCountryId = companyInput.SysCountryId,
                                MaxNrOfSMS = companyInput.MaxNrOfSMS,
                                AllowSupportLogin = true,
                                AllowSupportLoginTo = DateTime.Today.AddDays(30),
                                CompanyGuid = Guid.NewGuid(),

                                // Set References
                                License = license,
                            };

                            SetCreatedProperties(company);
                            entities.Company.AddObject(company);

                            #region Actor Add

                            var actor = new Actor()
                            {
                                ActorType = (int)SoeActorType.Company,
                            };

                            SetCreatedProperties(actor);
                            entities.Actor.AddObject(actor);

                            #endregion

                            var role = new Role()
                            {
                                TermId = (int)TermGroup_Roles.Systemadmin,

                                //Set references
                                Company = company,
                            };

                            SetCreatedProperties(role);
                            entities.Role.AddObject(role);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2089, "Företag kunde inte sparas"));

                            companyId = company.ActorCompanyId;

                            if (license.LicenseId == base.LicenseId)
                                UserManager.AddUserCompanyRoleMapping(entities, base.UserId, company.ActorCompanyId, role.RoleId, true);

                            #region Defaul features

                            Dictionary<Feature, Permission> featurePermissionsDict = new Dictionary<Feature, Permission>();
                            featurePermissionsDict.Add(Feature.Manage, Permission.Modify);
                            featurePermissionsDict.Add(Feature.Manage_Companies, Permission.Modify);
                            featurePermissionsDict.Add(Feature.Manage_Companies_Edit, Permission.Modify);
                            featurePermissionsDict.Add(Feature.Manage_Companies_Edit_Permission, Permission.Modify);
                            featurePermissionsDict.Add(Feature.Manage_Roles, Permission.Modify);
                            featurePermissionsDict.Add(Feature.Manage_Roles_Edit, Permission.Modify);
                            featurePermissionsDict.Add(Feature.Manage_Roles_Edit_Permission, Permission.Modify);

                            //Add Permission to Company
                            FeatureManager.AddCompanyPermissions(entities, featurePermissionsDict, company.ActorCompanyId);

                            //Add Permission to Role
                            var roleFeatures = new List<RoleFeature>();
                            roleFeatures.Add(new RoleFeature() { SysFeatureId = (int)Feature.Manage, SysPermissionId = (int)Permission.Modify, RoleId = role.RoleId });
                            roleFeatures.Add(new RoleFeature() { SysFeatureId = (int)Feature.Manage_Companies, SysPermissionId = (int)Permission.Modify, RoleId = role.RoleId });
                            roleFeatures.Add(new RoleFeature() { SysFeatureId = (int)Feature.Manage_Companies_Edit, SysPermissionId = (int)Permission.Modify, RoleId = role.RoleId });
                            roleFeatures.Add(new RoleFeature() { SysFeatureId = (int)Feature.Manage_Companies_Edit_Permission, SysPermissionId = (int)Permission.Modify, RoleId = role.RoleId });
                            roleFeatures.Add(new RoleFeature() { SysFeatureId = (int)Feature.Manage_Roles, SysPermissionId = (int)Permission.Modify, RoleId = role.RoleId });
                            roleFeatures.Add(new RoleFeature() { SysFeatureId = (int)Feature.Manage_Roles_Edit, SysPermissionId = (int)Permission.Modify, RoleId = role.RoleId });
                            roleFeatures.Add(new RoleFeature() { SysFeatureId = (int)Feature.Manage_Roles_Edit_Permission, SysPermissionId = (int)Permission.Modify, RoleId = role.RoleId });

                            FeatureManager.AddRolePermissions(entities, roleFeatures);
                            #endregion

                            #region AccountDim

                            // Add AccountDim std
                            var accountDim = new AccountDim()
                            {
                                AccountDimNr = Constants.ACCOUNTDIM_STANDARD,
                                Name = GetText(1258, "Konto"),
                                ShortName = GetText(3776, "Std"),
                                SysSieDimNr = null,
                                MinChar = null,
                                MaxChar = null,
                            };

                            result = AccountManager.AddAccountDim(entities, accountDim, company.ActorCompanyId);
                            if (!result.Success)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2089, "Företag kunde inte sparas"));

                            #endregion

                            #region VoucherSeriesType

                            // Add standard VoucherSeriesType and related sttings
                            VoucherSeriesType voucherSeriesType = new VoucherSeriesType()
                            {
                                StartNr = 1,
                                Name = "Manuell",
                                VoucherSeriesTypeNr = 1,
                            };

                            result = VoucherManager.AddVoucherSeriesType(entities, voucherSeriesType, company.ActorCompanyId);
                            if (!result.Success)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2089, "Företag kunde inte sparas"));

                            //Settings
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierInvoiceVoucherSeriesType, voucherSeriesType.VoucherSeriesTypeId, base.UserId, company.ActorCompanyId, 0);
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SupplierPaymentVoucherSeriesType, voucherSeriesType.VoucherSeriesTypeId, base.UserId, company.ActorCompanyId, 0);
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerInvoiceVoucherSeriesType, voucherSeriesType.VoucherSeriesTypeId, base.UserId, company.ActorCompanyId, 0);
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CustomerPaymentVoucherSeriesType, voucherSeriesType.VoucherSeriesTypeId, base.UserId, company.ActorCompanyId, 0);
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingVoucherSeriesTypeManual, voucherSeriesType.VoucherSeriesTypeId, base.UserId, company.ActorCompanyId, 0);

                            #endregion

                            #region Currency

                            // Currency
                            if (baseSysCurrency != null)
                            {
                                Currency baseCurrency = new Currency()
                                {
                                    SysCurrencyId = baseSysCurrency.SysCurrencyId,
                                    IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                                    UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
                                };
                                CountryCurrencyManager.AddCurrency(entities, baseCurrency, DateTime.Today, company.ActorCompanyId);
                            }

                            //BaseEntCurrency
                            if (baseEntSysCurrency != null)
                            {
                                Currency baseEntCurrency = new Currency()
                                {
                                    SysCurrencyId = baseEntSysCurrency.SysCurrencyId,
                                    IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                                    UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
                                };
                                CountryCurrencyManager.AddCurrency(entities, baseEntCurrency, DateTime.Today, company.ActorCompanyId);
                            }

                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, baseSysCurrency.SysCurrencyId, base.UserId, company.ActorCompanyId, 0);
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreBaseEntCurrency, companyInput.BaseEntCurrencyId, base.UserId, company.ActorCompanyId, 0);
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingCurrencySource, Constants.CURRENCY_SOURCE_DEFAULT, base.UserId, company.ActorCompanyId, 0);
                            SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingCurrencyIntervalType, Constants.CURRENCY_INTERVALTYPE_DEFAULT, base.UserId, company.ActorCompanyId, 0);

                            #endregion

                            // API
                            SettingManager.UpdateInsertStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, Guid.NewGuid().ToString(), base.UserId, company.ActorCompanyId, 0);

                            #endregion

                        }
                        else
                        {
                            #region Company Update

                            company.CompanyNr = companyInput.Number;
                            company.Name = companyInput.Name;
                            company.ShortName = companyInput.ShortName;
                            company.OrgNr = companyInput.OrgNr;
                            company.VatNr = companyInput.VatNr;
                            company.CompanyTaxSupport = companyInput.CompanyTaxSupport;
                            company.Template = companyInput.Template;
                            company.Global = companyInput.Global;
                            company.Demo = companyInput.Demo;
                            company.SysCountryId = companyInput.SysCountryId;

                            SetModifiedProperties(company);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2092, "Företag kunde inte uppdateras"));

                            // Currency
                            if (baseSysCurrency != null)
                            {
                                Currency existingBaseCurrency = CountryCurrencyManager.GetCurrencyAndRate(entities, baseSysCurrency.SysCurrencyId, company.ActorCompanyId, false);
                                if (existingBaseCurrency == null)
                                {
                                    existingBaseCurrency = new Currency()
                                    {
                                        SysCurrencyId = baseSysCurrency.SysCurrencyId,
                                        IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                                        UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
                                    };
                                    CountryCurrencyManager.AddCurrency(entities, existingBaseCurrency, DateTime.Today, company.ActorCompanyId);
                                }

                                SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreBaseCurrency, baseSysCurrency.SysCurrencyId, base.UserId, company.ActorCompanyId, 0);
                            }

                            if (baseEntSysCurrency != null)
                            {
                                Currency baseEntCurrency = CountryCurrencyManager.GetCurrencyAndRate(entities, baseEntSysCurrency.SysCurrencyId, company.ActorCompanyId, false);
                                if (baseEntCurrency == null)
                                {
                                    baseEntCurrency = new Currency()
                                    {
                                        SysCurrencyId = baseEntSysCurrency.SysCurrencyId,
                                        IntervalType = Constants.CURRENCY_INTERVALTYPE_DEFAULT,
                                        UseSysRate = Constants.CURRENCY_USESYSRATE_DEFAULT,
                                    };
                                    CountryCurrencyManager.AddCurrency(entities, baseEntCurrency, DateTime.Today, company.ActorCompanyId);
                                }

                                //Settings
                                SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.CoreBaseEntCurrency, companyInput.BaseEntCurrencyId, base.UserId, company.ActorCompanyId, 0);
                            }

                            #endregion
                        }

                        #endregion

                        #region Addresses

                        result = ContactManager.SaveContactAddresses(entities, companyInput.ContactAddresses, company.ActorCompanyId, TermGroup_SysContactType.Company);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region PaymentInformation

                        if (companyInput.PaymentInformation?.Rows != null)
                        {
                            result = PaymentManager.SavePaymentInformation(entities, transaction, companyInput.PaymentInformation.Rows, companyId, companyInput.PaymentInformation.DefaultSysPaymentTypeId, company.ActorCompanyId, false, true, SoeEntityType.Company);
                            if (!result.Success)
                            {
                                if(result.ErrorNumber == (int)ActionResultSave.PaymentInformationRowUsedByPaymentMethod)
                                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(7888, "Betalkontot används på en eller flera betalmetoder och kan därmed ej tas bort."));
                                else
                                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(11011, "Alla kontakt- och tele/webb-uppgifter kunde inte sparas"));
                            }
                        }

                        #endregion PaymentInformation

                        #region EDI

                        var xeEdiFeature = FeatureManager.GetCompanyFeature(entities, company.ActorCompanyId, (int)Feature.Billing_Import_XEEdi);
                        if (companyInput.IsEdiGOActivated)
                        {
                            if (xeEdiFeature == null)
                            {
                                var ediFeature = new CompanyFeature()
                                {
                                    SysFeatureId = (int)Feature.Billing_Import_XEEdi,
                                    SysPermissionId = (int)Permission.Modify,

                                    //Set references
                                    Company = company,
                                };
                                SetCreatedProperties(ediFeature);
                                entities.CompanyFeature.AddObject(ediFeature);
                            }
                            else
                            {
                                xeEdiFeature.SysPermissionId = (int)Permission.Modify;
                                SetModifiedProperties(xeEdiFeature);
                            }
                        }
                        else if (xeEdiFeature != null)
                        {
                            DeleteEntityItem(entities, xeEdiFeature);
                        }

                        var companyEdi = EdiManager.GetCompanyEdi(entities, company.ActorCompanyId, type: TermGroup_CompanyEdiType.Symbrio, onlyActive: true);
                        if (companyEdi != null)
                        {
                            int ediState = companyInput.IsEdiActivated ? (int)SoeEntityState.Active : (int)SoeEntityState.Inactive;

                            #region Update

                            companyEdi.Username = companyInput.EdiUsername;
                            companyEdi.Password = companyInput.EdiPassword;
                            companyEdi.State = ediState;

                            SetModifiedProperties(companyEdi);

                            #endregion
                        }
                        else if (!String.IsNullOrEmpty(companyInput.EdiUsername) || !String.IsNullOrEmpty(companyInput.EdiPassword))
                        {
                            #region Add

                            companyEdi = new CompanyEdi()
                            {
                                Username = companyInput.EdiUsername,
                                Password = companyInput.EdiPassword,
                                State = (int)SoeEntityState.Active,
                                Type = (int)TermGroup_CompanyEdiType.Symbrio,

                                //Set references
                                Company = company,
                            };

                            SetCreatedProperties(companyEdi);
                            entities.CompanyEdi.AddObject(companyEdi);

                            #endregion
                        }
                        #endregion

                        result = SaveChanges(entities, transaction);

                        // Commit transaction
                        if (result.Success)
                            transaction.Complete();

                        if (!result.Success)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2089, "Företag kunde inte sparas"));
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = companyId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                SessionCache.ReloadCompany(actorCompanyId);
                return result;
            }
        }

        private decimal GetDefaultVatRate(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return GetDefaultVatRate(entities, actorCompanyId);
        }

        public decimal GetDefaultVatRate(CompEntities entities, int actorCompanyId)
        {
            var company = GetCompany(entities, actorCompanyId);

            switch (company.SysCountryId ?? 0)
            {
                case (int)TermGroup_Country.FI:
                    return 0.24M;
                case (int)TermGroup_Country.SE:
                default:
                    return 0.25M;
            }
        }

        #endregion

        #region CompanyGroup

        #region CompanyGroupAdministration

        public CompanyGroupAdministration GetCompanyGroupAdministration(int actorCompanyId, int companyGroupAdministrationId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyGroupAdministration.NoTracking();
            return GetCompanyGroupAdministration(entities, actorCompanyId, companyGroupAdministrationId);
        }

        public CompanyGroupAdministration GetCompanyGroupAdministration(CompEntities entities, int actorCompanyId, int companyGroupAdministrationId)
        {
            return (from c in entities.CompanyGroupAdministration
                    where c.CompanyGroupAdministrationId == companyGroupAdministrationId &&
                    c.GroupCompanyActorCompanyId == actorCompanyId
                    select c).FirstOrDefault();
        }

        public CompanyGroupAdministration GetCompanyGroupAdministrationByChildCompanyId(int actorCompanyId, int childCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyGroupAdministration.NoTracking();
            return GetCompanyGroupAdministrationByChildCompanyId(entities, actorCompanyId, childCompanyId);
        }

        public CompanyGroupAdministration GetCompanyGroupAdministrationByChildCompanyId(CompEntities entities, int actorCompanyId, int childCompanyId)
        {
            return (from c in entities.CompanyGroupAdministration
                    where c.GroupCompanyActorCompanyId == actorCompanyId &&
                    c.ChildActorCompanyId == childCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c).FirstOrDefault();
        }

        public List<CompanyGroupAdministration> GetCompanyGroupAdministrationList(int actorCompanyId, int? id = null, bool onlyActive = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CompanyGroupAdministration.NoTracking();
            return GetCompanyGroupAdministrationList(entities, actorCompanyId, onlyActive, id);

        }

        public List<CompanyGroupAdministration> GetCompanyGroupAdministrationList(CompEntities entities, int actorCompanyId, bool onlyActive = true, int? id = null)
        {
            var query = from i in entities.CompanyGroupAdministration
                        .Include("Company")
                        where i.GroupCompanyActorCompanyId == actorCompanyId
                        select i;

            if (id.HasValue)
                query = query.Where(x => x.CompanyGroupAdministrationId == id.Value);

            if (onlyActive)
                query = query.Where(i => i.State == (int)SoeEntityState.Active);

            return query.ToList();
        }

        public ActionResult SaveCompanyGroupAdministration(int actorCompanyId, CompanyGroupAdministrationDTO companyGroupAdministrationInput, bool setChildCompanyValues = false)
        {
            if (companyGroupAdministrationInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyGroupAdministration");

            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (setChildCompanyValues)
                        {
                            var company = GetCompany(entities, companyGroupAdministrationInput.ChildActorCompanyId);

                            if (company == null)
                                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

                            companyGroupAdministrationInput.ChildActorCompanyNr = company.CompanyNr ?? 0;
                            companyGroupAdministrationInput.ChildActorCompanyName = company.Name;
                        }

                        result = SaveCompanyGroupAdministration(entities, transaction, companyGroupAdministrationInput, actorCompanyId);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    result.Value = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveCompanyGroupAdministration(CompEntities entities, TransactionScope transaction, CompanyGroupAdministrationDTO companyGroupAdministrationInput, int actorCompanyId)
        {
            #region CompanyGroupAdministration

            // Get existing 
            CompanyGroupAdministration companyGroupAdministration = GetCompanyGroupAdministration(entities, actorCompanyId, companyGroupAdministrationInput.CompanyGroupAdministrationId);

            if (companyGroupAdministration == null)
            {
                #region CompanyGroupAdministration Add

                companyGroupAdministration = new CompanyGroupAdministration()
                {
                    GroupCompanyActorCompanyId = companyGroupAdministrationInput.GroupCompanyActorCompanyId,
                    ChildActorCompanyId = companyGroupAdministrationInput.ChildActorCompanyId,
                    CompanyGroupMappingHeadId = companyGroupAdministrationInput.CompanyGroupMappingHeadId,
                    ChildActorCompanyName = companyGroupAdministrationInput.ChildActorCompanyName,
                    ChildActorCompanyNr = companyGroupAdministrationInput.ChildActorCompanyNr,
                    AccountId = companyGroupAdministrationInput.AccountId,
                    Conversionfactor = companyGroupAdministrationInput.Conversionfactor,
                    Note = companyGroupAdministrationInput.Note,
                    MatchInternalAccountsOnNr = companyGroupAdministrationInput.MatchInternalAccountOnNr,
                };
                SetCreatedProperties(companyGroupAdministration);
                entities.CompanyGroupAdministration.AddObject(companyGroupAdministration);

                #endregion
            }
            else
            {
                #region Update
                companyGroupAdministration.GroupCompanyActorCompanyId = companyGroupAdministrationInput.GroupCompanyActorCompanyId;
                companyGroupAdministration.ChildActorCompanyId = companyGroupAdministrationInput.ChildActorCompanyId;
                companyGroupAdministration.ChildActorCompanyName = companyGroupAdministrationInput.ChildActorCompanyName;
                companyGroupAdministration.ChildActorCompanyNr = companyGroupAdministrationInput.ChildActorCompanyNr;
                companyGroupAdministration.CompanyGroupMappingHeadId = companyGroupAdministrationInput.CompanyGroupMappingHeadId;
                companyGroupAdministration.AccountId = companyGroupAdministrationInput.AccountId;
                companyGroupAdministration.Conversionfactor = companyGroupAdministrationInput.Conversionfactor;
                companyGroupAdministration.Note = companyGroupAdministrationInput.Note;
                companyGroupAdministration.MatchInternalAccountsOnNr = companyGroupAdministrationInput.MatchInternalAccountOnNr;
                SetModifiedProperties(companyGroupAdministration);
                #endregion
            }

            #endregion            

            var result = SaveChanges(entities, transaction);
            if (result.Success)
            {
                //Set success properties
                result.IntegerValue = companyGroupAdministration.CompanyGroupAdministrationId;
                result.Value = companyGroupAdministration.CompanyGroupAdministrationId;
                result.StringValue = companyGroupAdministration.CompanyGroupAdministrationId.ToString();
            }

            return result;
        }

        public ActionResult DeleteCompanyGroupAdministration(int actorCompanyId, int companyGroupAdministrationId)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Get 
                CompanyGroupAdministration companyGroupAdministration = GetCompanyGroupAdministration(entities, actorCompanyId, companyGroupAdministrationId);
                if (companyGroupAdministration == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyGroupAdministration");

                // Delete 
                return ChangeEntityState(entities, companyGroupAdministration, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region CompanyGroupMapping

        public List<CompanyGroupMappingHead> GetCompanyGroupMappingHeadList(int ActorCompanyId, int? companyGroupMappingHeadId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            IQueryable<CompanyGroupMappingHead> query = (from c in entities.CompanyGroupMappingHead
                                                         where c.ActorCompanyId == ActorCompanyId &&
                                                         c.State == (int)SoeEntityState.Active
                                                         orderby c.Number
                                                         select c);

            if (companyGroupMappingHeadId.HasValue)
            {
                query = query.Where(x => x.CompanyGroupMappingHeadId == companyGroupMappingHeadId);
            }
            return query.ToList();
        }

        public List<SmallGenericType> GetCompanyGroupMappingHeadsDict(int ActorCompanyId, bool AddEmptyRow = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            var heads = (from c in entities.CompanyGroupMappingHead
                         where c.ActorCompanyId == ActorCompanyId &&
                         c.State == (int)SoeEntityState.Active
                         orderby c.Number
                         select c).ToList();

            var headsDict = new List<SmallGenericType>();

            if (AddEmptyRow)
                headsDict.Add(new SmallGenericType(0, " "));

            foreach (var head in heads)
            {
                headsDict.Add(new SmallGenericType(head.CompanyGroupMappingHeadId, string.Format("{0} {1}", head.Number, head.Name)));
            }
            return headsDict;
        }

        public CompanyGroupMappingHead GetCompanyGroupMapping(int CompanyGroupMappingHeadId, bool loadMappingRows)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Company.NoTracking();
            CompanyGroupMappingHead groupHead = null;
            entitiesReadOnly.CompanyGroupMapping.NoTracking();

            if (loadMappingRows)
            {
                groupHead = (from c in entitiesReadOnly.CompanyGroupMappingHead
                             .Include("CompanyGroupMappingRow")
                             where c.CompanyGroupMappingHeadId == CompanyGroupMappingHeadId
                             select c).FirstOrDefault();
            }
            else
            {
                groupHead = (from c in entitiesReadOnly.CompanyGroupMappingHead
                             where c.CompanyGroupMappingHeadId == CompanyGroupMappingHeadId
                             select c).FirstOrDefault();
            }

            return groupHead;
        }

        public CompanyGroupMappingHead GetCompanyGroupMapping(CompEntities entities, int CompanyGroupMappingHeadId, bool loadMappingRows)
        {
            CompanyGroupMappingHead groupHead = null;

            if (loadMappingRows)
            {
                groupHead = (from c in entities.CompanyGroupMappingHead
                             .Include("CompanyGroupMappingRow")
                             where c.CompanyGroupMappingHeadId == CompanyGroupMappingHeadId
                             select c).FirstOrDefault();
            }
            else
            {
                groupHead = (from c in entities.CompanyGroupMappingHead
                             where c.CompanyGroupMappingHeadId == CompanyGroupMappingHeadId
                             select c).FirstOrDefault();
            }

            return groupHead;
        }

        public CompanyGroupMappingRow GetCompanyGroupMappingRow(CompEntities entities, int CompanyGroupMappingRowId, int CompanyGroupMappingHeadId)
        {
            return (from c in entities.CompanyGroupMappingRow
                    where c.CompanyGroupMappingHeadId == CompanyGroupMappingHeadId &&
                    c.CompanyGroupMappingRowId == CompanyGroupMappingRowId
                    select c).FirstOrDefault();
        }

        public bool IsCompanyGroupMappingHeadNumberExists(int companyGroupMappingHeadId, int number, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Company.NoTracking();
            return entitiesReadOnly.CompanyGroupMappingHead
                    .Any(c =>
                        c.ActorCompanyId == actorCompanyId &&
                        c.State == (int)SoeEntityState.Active &&
                        c.Number == number &&
                        ((companyGroupMappingHeadId == 0) || c.CompanyGroupMappingHeadId != companyGroupMappingHeadId)
                    );
        }

        public ActionResult SaveCompanyGroupMapping(CompanyGroupMappingHeadDTO companyGroupMappingHeadInput, List<CompanyGroupMappingRowDTO> companyGroupMappingRowDTOs, int actorCompanyId)
        {
            if (companyGroupMappingHeadInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "CompanyGroupHead");

            if (this.IsCompanyGroupMappingHeadNumberExists(companyGroupMappingHeadInput.CompanyGroupMappingHeadId, companyGroupMappingHeadInput.Number, base.ActorCompanyId))
            {
                return new ActionResult((int)ActionResultSave.NumberExists, GetText(110677, "Numret används redan"));
            }

            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveCompanyGroupMapping(entities, transaction, companyGroupMappingHeadInput, companyGroupMappingRowDTOs, actorCompanyId);

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    result.Value = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveCompanyGroupMapping(CompEntities entities, TransactionScope transaction, CompanyGroupMappingHeadDTO companyGroupMappingHeadInput, List<CompanyGroupMappingRowDTO> companyGroupMappingRowDTOs, int actorCompanyId)
        {
            #region CompanyGroupMappingHead

            // Get existing Voucher
            CompanyGroupMappingHead companyGroupMappingHead = GetCompanyGroupMapping(entities, companyGroupMappingHeadInput.CompanyGroupMappingHeadId, true);

            if (companyGroupMappingHead == null)
            {
                #region CompanyGroupMappingHead Add

                companyGroupMappingHead = new CompanyGroupMappingHead()
                {

                    Number = companyGroupMappingHeadInput.Number,
                    Name = companyGroupMappingHeadInput.Name,
                    Description = companyGroupMappingHeadInput.Description,
                    Type = companyGroupMappingHeadInput.Type,

                    //Set FK
                    ActorCompanyId = actorCompanyId,


                };
                SetCreatedProperties(companyGroupMappingHead);
                entities.CompanyGroupMappingHead.AddObject(companyGroupMappingHead);

                #endregion
            }
            else
            {
                #region VoucherHead Update

                companyGroupMappingHead.Number = companyGroupMappingHeadInput.Number;
                companyGroupMappingHead.Name = companyGroupMappingHeadInput.Name;
                companyGroupMappingHead.Description = companyGroupMappingHeadInput.Description;
                companyGroupMappingHead.Type = companyGroupMappingHeadInput.Type;
                SetModifiedProperties(companyGroupMappingHead);

                #endregion
            }

            #endregion

            #region CompanyGroupMappingRow

            #region CompanyGroupMappingRow Update/Delete

            List<CompanyGroupMappingRow> allRows = companyGroupMappingHead.CompanyGroupMappingRow?.ToList();

            // Update or Delete existing MappingRows
            foreach (CompanyGroupMappingRowDTO companyGroupMappingRowInput in companyGroupMappingRowDTOs)
            {
                // Try get MappingRow from input
                CompanyGroupMappingRow companyGroupMappingRow = GetCompanyGroupMappingRow(entities, companyGroupMappingRowInput.CompanyGroupMappingRowId, companyGroupMappingRowInput.CompanyGroupMappingHeadId);

                if (companyGroupMappingRow == null)
                {
                    #region CompanyGroupMappingRow Add

                    companyGroupMappingRow = new CompanyGroupMappingRow()
                    {
                        ChildAccountFrom = companyGroupMappingRowInput.ChildAccountFrom,
                        ChildAccountTo = companyGroupMappingRowInput.ChildAccountTo,
                        GroupCompanyAccount = companyGroupMappingRowInput.GroupCompanyAccount,
                    };

                    SetCreatedProperties(companyGroupMappingRow);

                    companyGroupMappingHead.CompanyGroupMappingRow.Add(companyGroupMappingRow);

                    #endregion
                }
                else
                {
                    #region CompanyGroupMappingRow Update / Delete

                    if (allRows != null && allRows.Any(x => x.CompanyGroupMappingRowId == companyGroupMappingRowInput.CompanyGroupMappingRowId))
                    {
                        allRows.RemoveAll(r => r.CompanyGroupMappingRowId == companyGroupMappingRowInput.CompanyGroupMappingRowId);
                    }

                    companyGroupMappingRow.ChildAccountFrom = companyGroupMappingRowInput.ChildAccountFrom;
                    companyGroupMappingRow.ChildAccountTo = companyGroupMappingRowInput.ChildAccountTo;
                    companyGroupMappingRow.GroupCompanyAccount = companyGroupMappingRowInput.GroupCompanyAccount;
                    companyGroupMappingRow.State = companyGroupMappingRowInput.IsDeleted ? (int)SoeEntityState.Deleted : (int)companyGroupMappingRowInput.State;

                    SetModifiedProperties(companyGroupMappingRow);


                    #endregion
                }
            }

            // Set status to deleted on not found
            if (allRows != null)
            {
                foreach (CompanyGroupMappingRow cGNotFound in allRows)
                {
                    if (cGNotFound.State != (int)SoeEntityState.Deleted)
                        ChangeEntityState(cGNotFound, SoeEntityState.Deleted);
                }
            }

            #endregion

            #endregion

            var result = SaveChanges(entities, transaction);
            if (result.Success)
            {
                //Set success properties
                result.IntegerValue = companyGroupMappingHead.CompanyGroupMappingHeadId;
                result.Value = companyGroupMappingHead.Number;
                result.StringValue = companyGroupMappingHead.Name;
            }

            return result;
        }

        public ActionResult DeleteCompanyGroupMapping(int companyGroupMappingHeadId)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Get voucher head
                CompanyGroupMappingHead companyGroupMappingHead = GetCompanyGroupMapping(entities, companyGroupMappingHeadId, true);
                if (companyGroupMappingHead == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyGroupMappingHead");

                // Remove the rows
                foreach (CompanyGroupMappingRow mappingRow in companyGroupMappingHead.CompanyGroupMappingRow.ToList())
                {
                    if (!ChangeEntityState(entities, mappingRow, SoeEntityState.Deleted, false).Success)
                        return new ActionResult((int)ActionResultDelete.CompanyGroupMappingNotDeleted);
                }

                // Remove voucher
                return ChangeEntityState(entities, companyGroupMappingHead, SoeEntityState.Deleted, true);
            }
        }

        #endregion

        #region Transfer

        public ActionResult TransferCompanyGroupConsolidation(int actorCompanyId, int licenseId, int accountYearId, int voucherSeriesId, int periodFrom, int periodTo, bool includeIB, int companyGroupDimId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region preReq
                    List<string> missingAccounts = new List<string>();

                    var dimToMapCompanyTo = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.MapCompanyToAccountDimInConsolidation, 0, actorCompanyId, 0);
                    if (dimToMapCompanyTo == 0)
                        return new ActionResult((int)ActionResultDelete.NothingSaved, GetText(7479, (int)TermGroup.General, "Inställning för vilken dimension företagsnamnet mappas mot saknas."));

                    AccountPeriod accountPeriodFrom = AccountManager.GetAccountPeriod(entities, periodFrom, false);
                    if (accountPeriodFrom == null)
                        return new ActionResult((int)ActionResultDelete.EntityIsNull, "AccountPeriod");

                    AccountPeriod accountPeriodTo = AccountManager.GetAccountPeriod(entities, periodTo, false);
                    if (accountPeriodTo == null)
                        return new ActionResult((int)ActionResultDelete.EntityIsNull, "AccountPeriod");

                    List<CompanyGroupAdministration> companyGroupChildCompanys = GetCompanyGroupAdministrationList(entities, actorCompanyId, true);
                    if (companyGroupChildCompanys == null)
                        return new ActionResult((int)ActionResultDelete.EntityIsNull, "CompanyGroupAdministration");

                    AccountDim companyMappingDim = null;
                    List<Account> mainAccounts = new List<Account>();
                    var mainAccountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, loadAccounts: true, loadInternalAccounts: true);
                    if (mainAccountDims != null && dimToMapCompanyTo > 0)
                    {
                        companyMappingDim = mainAccountDims.FirstOrDefault(d => d.AccountDimId == dimToMapCompanyTo);
                        var dim = mainAccountDims.FirstOrDefault(d => d.AccountDimId == companyGroupDimId);
                        if (dim != null)
                            mainAccounts = dim.Account.ToList();
                    }

                    VoucherSeries voucherSerie = VoucherManager.GetVoucherSerie(entities, voucherSeriesId, actorCompanyId, true);
                    if (voucherSerie == null)
                        return new ActionResult((int)ActionResultDelete.EntityIsNull, "VoucherSeries");

                    long voucherSeriesLatestNr = voucherSerie.VoucherNrLatest ?? 0;
                    DateTime? latestVoucherDate = null;

                    List<AccountPeriod> listAccountPeriods = AccountManager.GetAccountPeriodsInDateInterval(entities, accountYearId, accountPeriodFrom.From, accountPeriodTo.To);

                    bool anyErrorsOnMappingAnyChildCompanies = false;
                    StringBuilder loggBuilder = new StringBuilder();
                    List<CompanyGroupTransferRow> companyGroupTransferRows = new List<CompanyGroupTransferRow>();

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION(new TimeSpan(0, 10, 0))))
                    {
                        foreach (CompanyGroupAdministration childCompany in companyGroupChildCompanys)
                        {
                            #region preReq

                            Account companyMappingAccount = null;

                            // Validate period statuses
                            if (listAccountPeriods.Any(p => p.Status != (int)TermGroup_AccountStatus.Open))
                                return new ActionResult((int)ActionResultDelete.NothingSaved, GetText(7544, (int)TermGroup.General, "En eller flera perioder på redovisningsåret har felaktig status för att en överföring ska kunna göras.\nKontrollera, ändra och försök igen."));

                            CompanyGroupMappingHead companyGroupMapping = CompanyManager.GetCompanyGroupMapping(entities, childCompany.CompanyGroupMappingHeadId, true);
                            if (companyGroupMapping == null)
                                return new ActionResult((int)ActionResultDelete.EntityIsNull, "CompanyGroupMapping");

                            decimal childCompanyConversionFactor = childCompany.Conversionfactor ?? 1;

                            Dictionary<int, int> dictAccountMapping = MappingToAccountDictionary(companyGroupMapping.GetCompanyGroupMappingRows());

                            bool anyErrorsOnMappingForChildCompany = false;

                            List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, childCompany.ChildActorCompanyId).ToList();
                            if (dims == null)
                                return new ActionResult((int)ActionResultDelete.EntityIsNull, "AccountDim");

                            #endregion

                            #region CompanyAccount

                            if (dimToMapCompanyTo > 0 && companyMappingAccount == null)
                            {
                                companyMappingAccount = companyMappingDim.Account.FirstOrDefault(a => a.AccountNr == childCompany.ChildActorCompanyNr.ToString() && a.Name == childCompany.ChildActorCompanyName);
                                if (companyMappingAccount == null)
                                {
                                    // Add missing account 
                                    companyMappingAccount = new Account()
                                    {
                                        AccountNr = childCompany.ChildActorCompanyNr.ToString(),
                                        Name = childCompany.ChildActorCompanyName,

                                        //Set references
                                        ActorCompanyId = actorCompanyId,

                                        AccountDim = companyMappingDim,
                                        State = (int)SoeEntityState.Active,
                                    };
                                    SetCreatedProperties(companyMappingAccount);

                                    entities.Account.AddObject(companyMappingAccount);

                                    //AccountInternal
                                    var accountInternal = new AccountInternal()
                                    {
                                        Account = companyMappingAccount,
                                    };

                                    entities.AccountInternal.AddObject(accountInternal);

                                    SaveChanges(entities);
                                }
                                else
                                {
                                    if (companyMappingAccount.AccountInternalReference.IsLoaded)
                                        companyMappingAccount.AccountInternalReference.Load();
                                }
                            }

                            #endregion

                            #region Balances

                            /*if (transferOpeningBalances)
                            {
                                var updatedAccountStds = new List<AccountStd>();

                                // Get balances
                                var accountYear = AccountManager.GetAccountYear(entities, listAccountPeriods[0].From.Date, childCompany.ChildActorCompanyId, false);
                                var balances = AccountBalanceManager(actorCompanyId).GetAccountYearBalanceHeads(entities, accountYear.AccountYearId, childCompany.ChildActorCompanyId);
                                if(balances == null || balances.Count ==  0)
                                    loggBuilder.AppendLine(string.Format("Ingånde balanser saknas för valt år för företag {0}", childCompany.ChildActorCompanyName));

                                // Get target account year
                                var targetAccountYear = AccountManager.GetAccountYear(entities, accountYearId);

                                foreach (var balance in balances)
                                {
                                    var balanceHead = new AccountYearBalanceHead()
                                    {
                                        Quantity = balance.Quantity,
                                        Balance = balance.Balance * childCompanyConversionFactor,

                                        // FK
                                        AccountYear = targetAccountYear,
                                    };

                                    //Set currency amounts
                                    CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, balanceHead);

                                    #region Account

                                    string strToAccountNr = "";

                                    if (string.IsNullOrEmpty(balance.AccountStd.Account.AccountNr))
                                        continue;

                                    if (balance.AccountStd.Account.AccountNr.All(char.IsDigit))
                                    {
                                        if (dictAccountMapping.ContainsKey(Convert.ToInt32(balance.AccountStd.Account.AccountNr)))
                                        {                                                                          
                                            dictAccountMapping.TryGetValue(Convert.ToInt32(balance.AccountStd.Account.AccountNr), out int toAccountNr);
                                            strToAccountNr = toAccountNr.ToString();
                                        }
                                        else
                                        {
                                            //NOT IN THE MAPPING DICTIONARY, TRANSFER TO THE SAME ACCOUNTNR                                   
                                            strToAccountNr = balance.AccountStd.Account.AccountNr;
                                        }
                                    }
                                    else
                                    {
                                        // The accountnr is text (not numeric) so we transfer the account without checking the accountmapping
                                        strToAccountNr = balance.AccountStd.Account.AccountNr;
                                    }

                                    Account parentAccount = mainAccounts.FirstOrDefault(a => a.AccountNr == strToAccountNr);
                                    if(parentAccount == null)
                                    {
                                        loggBuilder.AppendLine(string.Format("Konto med nummer ({0}), existerar inte på moderbolaget", strToAccountNr));
                                        continue;
                                    }

                                    balanceHead.AccountStd = parentAccount.AccountStd;
                                    if(!updatedAccountStds.Contains(parentAccount.AccountStd))
                                        updatedAccountStds.Add(parentAccount.AccountStd);

                                    if (companyMappingAccount != null)
                                        balanceHead.AccountInternal.Add(companyMappingAccount.AccountInternal);

                                    foreach (var accInternal in balance.AccountInternal)
                                    {
                                        if (!accInternal.Account.AccountDimReference.IsLoaded)
                                            accInternal.Account.AccountDimReference.Load();

                                        var accDim = mainAccountDims.FirstOrDefault(d => d.SysSieDimNr == accInternal.Account.AccountDim.SysSieDimNr);
                                        if (accDim != null && (accDim.AccountDimId != dimToMapCompanyTo || companyMappingAccount != null))
                                        {
                                            var acc = childCompany.MatchInternalAccountsOnNr ? accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr) : accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr && a.Name == accInternal.Account.Name);
                                            if (acc != null)
                                            {
                                                if (!acc.AccountInternalReference.IsLoaded)
                                                    acc.AccountInternalReference.Load();

                                                if (acc.AccountInternal != null)
                                                    balanceHead.AccountInternal.Add(acc.AccountInternal);
                                            }
                                        }
                                    }

                                    #endregion

                                    SetCreatedProperties(balanceHead);
                                    entities.AccountYearBalanceHead.AddObject(balanceHead);

                                    var transferRow = new CompanyGroupTransferRow()
                                    {
                                        ChildActorCompanyId = childCompany.ChildActorCompanyId,
                                        ConversionFactor = childCompanyConversionFactor,
                                        AccountYearBalanceHead = balanceHead,
                                    };

                                    SetCreatedProperties(transferRow);

                                    companyGroupTransferRows.Add(transferRow);
                                }

                                var companyGroupTransferHead = new CompanyGroupTransferHead()
                                {
                                    ActorCompanyId = base.ActorCompanyId,
                                    AccountYearId = accountYearId,
                                    TransferType = (int)CompanyGroupTransferType.Balance,
                                    Status = (int)CompanyGroupTransferStatus.Transfered,
                                };

                                SetCreatedProperties(companyGroupTransferHead);
                                entities.CompanyGroupTransferHead.AddObject(companyGroupTransferHead);

                                foreach (var row in companyGroupTransferRows)
                                {
                                    row.CompanyGroupTransferHead = companyGroupTransferHead;
                                    entities.CompanyGroupTransferRow.AddObject(row);
                                }

                                result = SaveChanges(entities, transaction); 
                                if (!result.Success)
                                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2205, "Kunde inte spara"));

                                //Update balance on all accounts that was updated
                                result = AccountBalanceManager(actorCompanyId).CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, accountYearId, updatedAccountStds);
                            }*/

                            #endregion

                            foreach (AccountPeriod accPeriod in listAccountPeriods) //One voucherhead per accountperiod (month) per childcompany
                            {
                                #region VoucherHead
                                bool errorsOnMappingForAccPeriod = false;
                                List<VoucherRow> voucherRowList = new List<VoucherRow>();

                                voucherSeriesLatestNr++;
                                latestVoucherDate = accPeriod.From;

                                // Create Empty Voucher Head DTO
                                VoucherHeadDTO voucherHeadDto = new VoucherHeadDTO()
                                {
                                    VoucherHeadId = 0,
                                    VoucherNr = voucherSeriesLatestNr,
                                    VoucherSeriesId = voucherSeriesId,
                                    Date = accPeriod.From,
                                    Text = string.Format("{0} - Period från {1} till {2}", childCompany.ChildActorCompanyName, accPeriod.From.ToShortDateString(), accPeriod.To.ToShortDateString()),
                                    Template = false,
                                    VatVoucher = false,
                                    CompanyGroupVoucher = true,
                                    Status = TermGroup_AccountStatus.Open,
                                    Note = "Koncernöverföring",
                                    ActorCompanyId = actorCompanyId,
                                    AccountPeriodId = accPeriod.AccountPeriodId,
                                };


                                // Get the voucherheads for this accountperiod and this child company
                                List<VoucherHead> voucherHeads = VoucherManager.GetVoucherHeadPeriodFromPeriodTo(entities, accPeriod.From.Date, accPeriod.To.Date, childCompany.ChildActorCompanyId);

                                #endregion

                                // loop through all voucherheads for this accountperiod and this childcompany 
                                foreach (VoucherHead voucherHeadItem in voucherHeads)
                                {
                                    // loop through all voucherrows 
                                    foreach (VoucherRow childVoucherRow in voucherHeadItem.VoucherRow)
                                    {
                                        #region VoucherRows
                                        string strToAccountNr = "";

                                        if (childVoucherRow.State == (int)SoeEntityState.Deleted)
                                            continue;

                                        if (string.IsNullOrEmpty(childVoucherRow.AccountNr))
                                        {
                                            loggBuilder.AppendLine(string.Format("Verifikatrad på verifikat {0} saknar kontonummer, kopiering försöktes från företag {1}", childVoucherRow.VoucherNr, childCompany.ChildActorCompanyName));
                                            continue;
                                        }

                                        if (childVoucherRow.AccountNr.All(char.IsDigit))
                                        {
                                            if (dictAccountMapping.ContainsKey(Convert.ToInt32(childVoucherRow.AccountNr)))
                                            {
                                                //THIS VOUCHER ROW EXISTS IN THE MAPPING TRANSFER FROM (dictionary Key) TO (dictionary Value)                                                                              
                                                dictAccountMapping.TryGetValue(Convert.ToInt32(childVoucherRow.AccountNr), out int toAccountNr);
                                                strToAccountNr = toAccountNr.ToString();
                                            }
                                            else
                                            {
                                                //NOT IN THE MAPPING DICTIONARY, TRANSFER TO THE SAME ACCOUNTNR                                   
                                                strToAccountNr = childVoucherRow.AccountNr;
                                            }
                                        }
                                        else
                                        {
                                            // The accountnr is text (not numeric) so we transfer the account without checking the accountmapping
                                            strToAccountNr = childVoucherRow.AccountNr;
                                            loggBuilder.AppendLine(string.Format("Verifikatrad på verifikat {0} med kontonummer ({1}), är överfört från företag {2}", childVoucherRow.VoucherNr, strToAccountNr, childCompany.ChildActorCompanyName));
                                        }

                                        // CHECK IF ACCOUNTNR ALREADY EXISTS IN VOUCHERROWLIST FOR THIS CHILDCOMPANY                                                                     
                                        if (voucherRowList.Any(i => i.AccountNr == strToAccountNr))
                                        {
                                            // Get the parent company accountid for the accountnr 
                                            Account parentAccount = mainAccounts.FirstOrDefault(a => a.AccountNr == strToAccountNr);

                                            // Create row to compare
                                            VoucherRow voucherRow = new VoucherRow()
                                            {
                                                Amount = childVoucherRow.Amount * childCompanyConversionFactor,
                                                AmountEntCurrency = childVoucherRow.AmountEntCurrency * childCompanyConversionFactor,
                                                Date = childVoucherRow.Date,
                                                AccountStd = parentAccount.AccountStd,
                                                AccountId = parentAccount.AccountId,
                                                Merged = false,
                                                State = 0,
                                            };

                                            if (companyMappingAccount != null)
                                                voucherRow.AccountInternal.Add(companyMappingAccount.AccountInternal);

                                            var accInternalIds = new List<int>();
                                            foreach (var accInternal in childVoucherRow.AccountInternal)
                                            {
                                                if (!accInternal.Account.AccountDimReference.IsLoaded)
                                                    accInternal.Account.AccountDimReference.Load();

                                                var accDim = mainAccountDims.FirstOrDefault(d => d.SysSieDimNr == accInternal.Account.AccountDim.SysSieDimNr);
                                                if (accDim != null && (accDim.AccountDimId != dimToMapCompanyTo || companyMappingAccount != null))
                                                {
                                                    var acc = childCompany.MatchInternalAccountsOnNr ? accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr) : accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr && a.Name == accInternal.Account.Name);
                                                    if (acc != null)
                                                    {
                                                        if (!acc.AccountInternalReference.IsLoaded)
                                                            acc.AccountInternalReference.Load();

                                                        if (acc.AccountInternal != null)
                                                        {
                                                            voucherRow.AccountInternal.Add(acc.AccountInternal);
                                                            accInternalIds.Add(acc.AccountId);
                                                        }
                                                    }
                                                }
                                            }

                                            VoucherRow matching = null;
                                            foreach (var existingRow in voucherRowList.Where(i => i.AccountNr == strToAccountNr))
                                            {
                                                if (existingRow.AccountInternal.Select(a => a.AccountId) == accInternalIds)
                                                {
                                                    matching = existingRow;
                                                    break;
                                                }
                                            }

                                            if (matching != null)
                                            {
                                                voucherRowList.FirstOrDefault(i => i.AccountNr == strToAccountNr).Amount += (childVoucherRow.Amount * childCompanyConversionFactor);
                                                voucherRowList.FirstOrDefault(i => i.AccountNr == strToAccountNr).AmountEntCurrency += (childVoucherRow.AmountEntCurrency * childCompanyConversionFactor);
                                                foreach (AccountInternal accountInternal in childVoucherRow.AccountInternal)
                                                {
                                                    voucherRowList.FirstOrDefault(i => i.AccountNr == strToAccountNr).AccountInternal.Add(accountInternal);
                                                }
                                            }
                                            else
                                            {
                                                voucherRowList.Add(voucherRow);
                                            }
                                        }
                                        else
                                        {
                                            // Get the parent company accountid for the accountnr 
                                            Account parentAccount = mainAccounts.FirstOrDefault(a => a.AccountNr == strToAccountNr);
                                            if (parentAccount != null)
                                            {
                                                if (!parentAccount.AccountStdReference.IsLoaded)
                                                    parentAccount.AccountStdReference.Load();
                                            }
                                            else
                                            {
                                                errorsOnMappingForAccPeriod = true;
                                                anyErrorsOnMappingForChildCompany = true;
                                                anyErrorsOnMappingAnyChildCompanies = true;
                                                if (!missingAccounts.Contains(strToAccountNr))
                                                {
                                                    loggBuilder.AppendLine(string.Format("Konto {0} existerar inte på moderbolaget. Kopiering försöktes från företag {1}", strToAccountNr, childCompany.ChildActorCompanyName));
                                                    missingAccounts.Add(strToAccountNr);
                                                }

                                                continue;
                                            }

                                            // New Row
                                            VoucherRow voucherRow = new VoucherRow()
                                            {
                                                Amount = childVoucherRow.Amount * childCompanyConversionFactor,
                                                AmountEntCurrency = childVoucherRow.AmountEntCurrency * childCompanyConversionFactor,
                                                Date = childVoucherRow.Date,
                                                AccountStd = parentAccount.AccountStd,
                                                AccountId = parentAccount.AccountId,
                                                Merged = false,
                                                State = 0,
                                            };

                                            if (companyMappingAccount != null)
                                                voucherRow.AccountInternal.Add(companyMappingAccount.AccountInternal);

                                            foreach (var accInternal in childVoucherRow.AccountInternal)
                                            {
                                                if (!accInternal.Account.AccountDimReference.IsLoaded)
                                                    accInternal.Account.AccountDimReference.Load();

                                                var accDim = mainAccountDims.FirstOrDefault(d => d.SysSieDimNr == accInternal.Account.AccountDim.SysSieDimNr);
                                                if (accDim != null && (accDim.AccountDimId != dimToMapCompanyTo || companyMappingAccount != null))
                                                {
                                                    var acc = childCompany.MatchInternalAccountsOnNr ? accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr) : accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr && a.Name == accInternal.Account.Name);
                                                    if (acc != null)
                                                    {
                                                        if (!acc.AccountInternalReference.IsLoaded)
                                                            acc.AccountInternalReference.Load();

                                                        if (acc.AccountInternal != null)
                                                            voucherRow.AccountInternal.Add(acc.AccountInternal);
                                                    }
                                                }
                                            }

                                            voucherRowList.Add(voucherRow);
                                        }
                                        #endregion
                                    } // End of foreach "childVoucherRow"                                

                                } // End of foreach "voucherHeadItem"

                                if (!errorsOnMappingForAccPeriod)
                                {
                                    result.Success = true;

                                    result = VoucherManager.SaveVoucherFromCompanyGroup(entities, transaction, voucherHeadDto, voucherRowList, voucherSeriesId, accPeriod.AccountPeriodId, actorCompanyId, true, updateSeqNr: false);

                                    if (result.Success)
                                    {
                                        var transferRow = new CompanyGroupTransferRow()
                                        {
                                            VoucherHeadId = result.IntegerValue,
                                            ChildActorCompanyId = childCompany.ChildActorCompanyId,
                                            ConversionFactor = childCompanyConversionFactor,
                                        };

                                        SetCreatedProperties(transferRow);

                                        companyGroupTransferRows.Add(transferRow);
                                    }
                                    else
                                    {
                                        loggBuilder.AppendLine(string.Format("Överföring genomfördes ej pga fel. Verifikat kunde ej sparas" + result.Exception != null ? ": " + result.Exception.Message : "", accPeriod.From.ToShortDateString()));
                                        anyErrorsOnMappingForChildCompany = true;
                                    }

                                    voucherRowList.Clear();
                                    errorsOnMappingForAccPeriod = false;
                                }
                                else
                                {
                                    loggBuilder.AppendLine(string.Format("Överföring genomfördes ej pga fel (Period {0})", accPeriod.From.ToShortDateString()));
                                }
                            }

                            if (anyErrorsOnMappingForChildCompany)
                            {
                                result.InfoMessage = loggBuilder.ToString();
                                result.Success = false;
                            }

                        } // End of foreach "childCompany"


                        //Commit transaction
                        if (anyErrorsOnMappingAnyChildCompanies)
                        {
                            result.InfoMessage = loggBuilder.ToString();
                            result.Value2 = result.InfoMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            result.Success = false;
                        }
                        else
                        {
                            if (result.Success)
                            {
                                // Update seq nr on voucher serie
                                voucherSerie.VoucherNrLatest = voucherSeriesLatestNr;
                                voucherSerie.VoucherDateLatest = latestVoucherDate;

                                #region Save Transfer

                                var companyGroupTransferHead = new CompanyGroupTransferHead()
                                {
                                    ActorCompanyId = base.ActorCompanyId,
                                    AccountYearId = accountYearId,
                                    FromAccountPeriodId = accountPeriodFrom.AccountPeriodId,
                                    ToAccountPeriodId = accountPeriodTo.AccountPeriodId,
                                    TransferType = (int)CompanyGroupTransferType.Consolidation,
                                    Status = (int)CompanyGroupTransferStatus.Transfered,
                                };

                                SetCreatedProperties(companyGroupTransferHead);
                                entities.CompanyGroupTransferHead.AddObject(companyGroupTransferHead);

                                foreach (var row in companyGroupTransferRows)
                                {
                                    row.CompanyGroupTransferHead = companyGroupTransferHead;
                                    entities.CompanyGroupTransferRow.AddObject(row);
                                }

                                result = SaveChanges(entities, transaction);

                                if (!result.Success)
                                    return result;

                                #endregion

                                result.InfoMessage = loggBuilder.ToString();
                                result.Value2 = result.InfoMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                transaction.Complete();
                            }
                        }

                    } // End of "using transaction"
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                } // End of try/catch/finally 

            } // End of "using entities" 

            return result;

        }

        public ActionResult TransferCompanyGroupBudget(int actorCompanyId, int accountYearId, int? budgetToId, int? childCompanyId, int? budgetFromId, int companyGroupDimId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        #region prereq

                        var anyErrorsOnMappingAnyChildCompanies = false;
                        var companyGroupTransferRows = new List<CompanyGroupTransferRow>();
                        var loggBuilder = new StringBuilder();

                        var accountYear = AccountManager.GetAccountYear(entities, accountYearId);

                        var dimToMapCompanyTo = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.MapCompanyToAccountDimInConsolidation, 0, actorCompanyId, 0);
                        if (dimToMapCompanyTo == 0)
                            return new ActionResult((int)ActionResultDelete.NothingSaved, GetText(7479, (int)TermGroup.General, "Inställning för vilken dimension företagsnamnet mappas mot saknas."));

                        var childCompanyGroupAdministration = GetCompanyGroupAdministrationByChildCompanyId(entities, actorCompanyId, childCompanyId.Value);
                        if (childCompanyGroupAdministration == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(1446, (int)TermGroup.AngularEconomy, "Mappning mellan företag saknas"));

                        BudgetHead mainCompanyGroupBudget = budgetToId.Value == 0 ? null : BudgetManager.GetBudgetHeadIncludingRows(entities, budgetToId.Value);
                        if (budgetToId.Value != 0 && mainCompanyGroupBudget == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(1444, (int)TermGroup.AngularEconomy, "Koncernbudget kunde inte hittas"));

                        var childCompanyGroupBudget = BudgetManager.GetBudgetHeadIncludingRows(entities, budgetFromId.Value);
                        if (childCompanyGroupBudget == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(1445, (int)TermGroup.AngularEconomy, "Dotterbolaget budget kunde inte hittas"));

                        AccountDim companyMappingDim = null;
                        List<Account> mainAccounts = new List<Account>();
                        var mainAccountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, loadAccounts: true, loadInternalAccounts: true);
                        if (mainAccountDims != null && dimToMapCompanyTo > 0)
                        {
                            companyMappingDim = mainAccountDims.FirstOrDefault(d => d.AccountDimId == dimToMapCompanyTo);
                            var dim = mainAccountDims.FirstOrDefault(d => d.AccountDimId == companyGroupDimId);
                            if (dim != null)
                                mainAccounts = dim.Account.ToList();
                        }

                        CompanyGroupMappingHead companyGroupMapping = GetCompanyGroupMapping(entities, childCompanyGroupAdministration.CompanyGroupMappingHeadId, true);
                        Dictionary<int, int> dictAccountMapping = MappingToAccountDictionary(companyGroupMapping.GetCompanyGroupMappingRows());

                        #endregion

                        #region mapping account

                        var companyMappingAccount = companyMappingDim?.Account.FirstOrDefault(a => a.AccountNr == childCompanyGroupAdministration.ChildActorCompanyNr.ToString() && a.Name == childCompanyGroupAdministration.ChildActorCompanyName);
                        if (companyMappingAccount == null)
                        {
                            // Add missing account 
                            companyMappingAccount = new Account()
                            {
                                AccountNr = childCompanyGroupAdministration.ChildActorCompanyNr.ToString(),
                                Name = childCompanyGroupAdministration.ChildActorCompanyName,

                                //Set references
                                ActorCompanyId = actorCompanyId,

                                AccountDim = companyMappingDim,
                                State = (int)SoeEntityState.Active,
                            };
                            SetCreatedProperties(companyMappingAccount);

                            entities.Account.AddObject(companyMappingAccount);

                            //AccountInternal
                            var accountInternal = new AccountInternal()
                            {
                                Account = companyMappingAccount,
                            };

                            entities.AccountInternal.AddObject(accountInternal);

                            SaveChanges(entities);
                        }
                        else
                        {
                            if (companyMappingAccount.AccountInternalReference.IsLoaded)
                                companyMappingAccount.AccountInternalReference.Load();
                        }

                        #endregion

                        #region transfer

                        var conversionFactor = childCompanyGroupAdministration.Conversionfactor ?? 1;

                        if (mainCompanyGroupBudget == null)
                        {
                            mainCompanyGroupBudget = new BudgetHead()
                            {
                                ActorCompanyId = actorCompanyId,
                                Type = (int)DistributionCodeBudgetType.AccountingBudget,
                                AccountYearId = accountYearId,
                                Name = GetText(1447, (int)TermGroup.AngularEconomy, "Budget skapad från Koncernredovisning") + " " + accountYear.From.ToShortDateString() + " - " + accountYear.To.ToShortDateString(),
                                NoOfPeriods = childCompanyGroupBudget.NoOfPeriods,
                                Status = 1,
                                UseDim2 = childCompanyGroupBudget.UseDim2,
                                UseDim3 = childCompanyGroupBudget.UseDim3,
                            };

                            SetCreatedProperties(mainCompanyGroupBudget);
                            entities.BudgetHead.AddObject(mainCompanyGroupBudget);

                            result = SaveChanges(entities);
                            if (!result.Success)
                                return result;
                        }
                        else
                        {
                            if (childCompanyGroupBudget.UseDim2 == true && mainCompanyGroupBudget.UseDim2 == false)
                                mainCompanyGroupBudget.UseDim2 = true;

                            if (childCompanyGroupBudget.UseDim3 == true && mainCompanyGroupBudget.UseDim3 == false)
                                mainCompanyGroupBudget.UseDim3 = true;
                        }

                        foreach (var item in childCompanyGroupBudget.BudgetRow.ToList())
                        {
                            var budgetRow = new BudgetRow()
                            {
                                BudgetHead = mainCompanyGroupBudget,
                                TotalQuantity = item.TotalQuantity,
                                Type = item.Type,
                                TotalAmount = item.TotalAmount,
                            };

                            if (item.AccountId.HasValue)
                            {
                                var childAccount = AccountManager.GetAccount(entities, childCompanyId.Value, item.AccountId.Value, false, true);
                                if (childAccount != null)
                                {
                                    string strToAccountNr = String.Empty;
                                    if (childAccount.AccountNr.All(char.IsDigit))
                                    {
                                        if (dictAccountMapping.ContainsKey(Convert.ToInt32(childAccount.AccountNr)))
                                        {
                                            dictAccountMapping.TryGetValue(Convert.ToInt32(childAccount.AccountNr), out int toAccountNr);
                                            strToAccountNr = toAccountNr.ToString();
                                        }
                                        else
                                        {
                                            strToAccountNr = childAccount.AccountNr;
                                        }
                                    }
                                    else
                                    {
                                        strToAccountNr = childAccount.AccountNr;
                                    }

                                    var masterAccount = mainAccounts.FirstOrDefault(a => a.AccountNr == strToAccountNr);
                                    if (masterAccount != null)
                                    {
                                        budgetRow.AccountId = masterAccount.AccountId;

                                        if (!masterAccount.AccountStdReference.IsLoaded)
                                            masterAccount.AccountStdReference.Load();

                                        budgetRow.AccountStd = masterAccount.AccountStd;
                                    }
                                    else
                                    {
                                        loggBuilder.AppendLine(GetText(1448, (int)TermGroup.AngularEconomy, "Kunde ej hitta matchande konto till") + " " + childAccount.AccountNr + " - " + childAccount.Name);
                                        anyErrorsOnMappingAnyChildCompanies = true;
                                    }
                                }
                            }

                            if (companyMappingAccount != null)
                                budgetRow.AccountInternal.Add(companyMappingAccount.AccountInternal);

                            foreach (var accInternal in item.AccountInternal)
                            {
                                if (!accInternal.Account.AccountDimReference.IsLoaded)
                                    accInternal.Account.AccountDimReference.Load();

                                var accDim = mainAccountDims.FirstOrDefault(d => d.SysSieDimNr == accInternal.Account.AccountDim.SysSieDimNr);
                                if (accDim != null && (accDim.AccountDimId != dimToMapCompanyTo || companyMappingAccount != null))
                                {
                                    var acc = childCompanyGroupAdministration.MatchInternalAccountsOnNr ? accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr) : accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr && a.Name == accInternal.Account.Name);
                                    if (acc != null)
                                    {
                                        if (!acc.AccountInternalReference.IsLoaded)
                                            acc.AccountInternalReference.Load();

                                        if (acc.AccountInternal != null)
                                            budgetRow.AccountInternal.Add(acc.AccountInternal);
                                    }
                                }
                            }

                            foreach (var rowPeriod in item.BudgetRowPeriod)
                            {
                                var period = new BudgetRowPeriod()
                                {
                                    BudgetRow = budgetRow,
                                    PeriodNr = rowPeriod.PeriodNr,
                                    Amount = Decimal.Round(rowPeriod.Amount * conversionFactor, 2),
                                    Quantity = rowPeriod.Quantity,
                                    Type = rowPeriod.Type,
                                };

                                SetCreatedProperties(period);
                                budgetRow.BudgetRowPeriod.Add(period);
                            }

                            if (conversionFactor != 1)
                                budgetRow.TotalAmount = Decimal.Round(budgetRow.BudgetRowPeriod.Sum(p => p.Amount), 2);

                            SetCreatedProperties(budgetRow);
                            mainCompanyGroupBudget.BudgetRow.Add(budgetRow);

                            var transferRow = new CompanyGroupTransferRow()
                            {
                                BudgetRow = budgetRow,
                                ChildActorCompanyId = childCompanyId.Value,
                                ConversionFactor = conversionFactor,
                            };

                            SetCreatedProperties(transferRow);

                            companyGroupTransferRows.Add(transferRow);
                        }

                        #endregion

                        if (anyErrorsOnMappingAnyChildCompanies)
                        {
                            result.InfoMessage = loggBuilder.ToString();
                            result.Value2 = result.InfoMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            result.Success = false;
                            transaction.Dispose();
                        }
                        else
                        {
                            var companyGroupTransferHead = new CompanyGroupTransferHead()
                            {
                                ActorCompanyId = base.ActorCompanyId,
                                AccountYearId = accountYearId,
                                TransferType = (int)CompanyGroupTransferType.Budget,
                                Status = (int)CompanyGroupTransferStatus.Transfered,
                            };

                            SetCreatedProperties(companyGroupTransferHead);
                            entities.CompanyGroupTransferHead.AddObject(companyGroupTransferHead);

                            foreach (var row in companyGroupTransferRows)
                            {
                                row.CompanyGroupTransferHead = companyGroupTransferHead;
                                entities.CompanyGroupTransferRow.AddObject(row);
                            }

                            result = SaveChanges(entities, transaction);

                            if (!result.Success)
                                return result;

                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

            }

            return result;
        }

        public List<CompanyGroupTransferHeadDTO> GetCompanyGroupTransferHistoryBudget(int accountYearId, int actorCompanyId, int transferType)
        {
            var dtos = new List<CompanyGroupTransferHeadDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            var items = (from view in entitiesReadOnly.CompanyGroupTransferHistoryView
                         where view.ActorCompanyId == actorCompanyId &&
                         view.TransferType == transferType
                         orderby view.TransferDate descending
                         select view);

            foreach (var view in items)
            {

                var dto = dtos.FirstOrDefault(t => t.CompanyGroupTransferHeadId == view.CompanyGroupTransferHeadId);
                if (dto == null)
                {
                    dto = new CompanyGroupTransferHeadDTO()
                    {
                        CompanyGroupTransferHeadId = view.CompanyGroupTransferHeadId,
                        ActorCompanyId = view.ActorCompanyId,
                        AccountYearId = view.AccountYearId,
                        AccountYearText = view.AccountYearFrom.ToShortDateString() + " - " + view.AccountYearTo.ToShortDateString(),
                        FromAccountPeriodText = view.AccountYearFrom.Year.ToString() + " - " + view.AccountYearFrom.Month.ToString(),
                        ToAccountPeriodText = view.AccountYearTo.Year.ToString() + " - " + view.AccountYearTo.Month.ToString(),
                        TransferDate = view.TransferDate,
                        TransferType = view.TransferType.HasValue ? (CompanyGroupTransferType)view.TransferType.Value : CompanyGroupTransferType.None,
                        TransferStatus = view.TransferStatus.HasValue ? (CompanyGroupTransferStatus)view.TransferStatus.Value : CompanyGroupTransferStatus.None,
                        IsOnlyVoucher = false,
                        CompanyGroupTransferRows = new List<CompanyGroupTransferRowDTO>(),
                    };

                    dto.TransferTypeName = dto.TransferType == CompanyGroupTransferType.Consolidation ? GetText(11, (int)TermGroup.ReportDrilldownGrid, "Utfall") : GetText(14, (int)TermGroup.ReportDrilldownGrid, "Budget");

                    if (dto.TransferStatus == CompanyGroupTransferStatus.Transfered)
                        dto.TransferStatusName = GetText(8847, (int)TermGroup.General, "Överförd");
                    else if (dto.TransferStatus == CompanyGroupTransferStatus.PartlyDeleted)
                        dto.TransferStatusName = GetText(8848, (int)TermGroup.General, "Delvis borttagen");
                    else
                        dto.TransferStatusName = GetText(2244, (int)TermGroup.General, "Borttagen");

                    var row = new CompanyGroupTransferRowDTO()
                    {
                        CompanyGroupTransferRowId = view.CompanyGroupTransferRowId,
                        ChildActorCompanyNrName = view.CompanyNr.HasValue ? view.CompanyNr.Value.ToString() + " - " + view.CompanyName : view.CompanyName,
                        Status = dto.TransferStatusName,
                        BudgetHeadId = view.BudgetHeadId,
                        BudgetName = view.Text,
                        VoucherSeriesId = view.VoucherSeriesId,
                        VoucherSeriesName = view.VoucherSeriesName,
                        ConversionFactor = view.ConversionFactor,
                        Created = view.TransferDate.Value,
                        AccountPeriodText = view.AccountPeriodFrom.HasValue ? view.AccountPeriodFrom.Value.Year.ToString() + " - " + view.AccountPeriodFrom.Value.Month.ToString() : " ",
                    };

                    dto.CompanyGroupTransferRows.Add(row);

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        public ActionResult TransferCompanyGroupIncomingBalance(int actorCompanyId, int accountYearId, int companyGroupDimId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region preReq
                    List<string> missingAccounts = new List<string>();

                    var dimToMapCompanyTo = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.MapCompanyToAccountDimInConsolidation, 0, actorCompanyId, 0);
                    if (dimToMapCompanyTo == 0)
                        return new ActionResult((int)ActionResultDelete.NothingSaved, GetText(7479, (int)TermGroup.General, "Inställning för vilken dimension företagsnamnet mappas mot saknas."));

                    List<CompanyGroupAdministration> companyGroupChildCompanys = GetCompanyGroupAdministrationList(entities, actorCompanyId, true);
                    if (companyGroupChildCompanys == null)
                        return new ActionResult((int)ActionResultDelete.EntityIsNull, "CompanyGroupAdministration");

                    AccountDim companyMappingDim = null;
                    List<Account> mainAccounts = new List<Account>();
                    var mainAccountDims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, loadAccounts: true, loadInternalAccounts: true);
                    if (mainAccountDims != null && dimToMapCompanyTo > 0)
                    {
                        companyMappingDim = mainAccountDims.FirstOrDefault(d => d.AccountDimId == dimToMapCompanyTo);
                        var dim = mainAccountDims.FirstOrDefault(d => d.AccountDimId == companyGroupDimId);
                        if (dim != null)
                            mainAccounts = dim.Account.ToList();
                    }

                    AccountYear parentAccountYear = AccountManager.GetAccountYear(entities, accountYearId);
                    if (parentAccountYear == null)
                        return new ActionResult((int)ActionResultDelete.EntityIsNull, "AccountYear");

                    StringBuilder loggBuilder = new StringBuilder();
                    List<CompanyGroupTransferRow> companyGroupTransferRows = new List<CompanyGroupTransferRow>();

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION(new TimeSpan(0, 10, 0))))
                    {
                        foreach (CompanyGroupAdministration childCompany in companyGroupChildCompanys)
                        {
                            #region preReq

                            Account companyMappingAccount = null;

                            CompanyGroupMappingHead companyGroupMapping = CompanyManager.GetCompanyGroupMapping(entities, childCompany.CompanyGroupMappingHeadId, true);
                            if (companyGroupMapping == null)
                                return new ActionResult((int)ActionResultDelete.EntityIsNull, "CompanyGroupMapping");

                            decimal childCompanyConversionFactor = childCompany.Conversionfactor ?? 1;

                            Dictionary<int, int> dictAccountMapping = MappingToAccountDictionary(companyGroupMapping.GetCompanyGroupMappingRows());

                            List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, childCompany.ChildActorCompanyId).ToList();
                            if (dims == null)
                                return new ActionResult((int)ActionResultDelete.EntityIsNull, "AccountDim");

                            #endregion

                            #region CompanyAccount

                            if (dimToMapCompanyTo > 0 && companyMappingAccount == null)
                            {
                                companyMappingAccount = companyMappingDim.Account.FirstOrDefault(a => a.AccountNr == childCompany.ChildActorCompanyNr.ToString() && a.Name == childCompany.ChildActorCompanyName);
                                if (companyMappingAccount == null)
                                {
                                    // Add missing account 
                                    companyMappingAccount = new Account()
                                    {
                                        AccountNr = childCompany.ChildActorCompanyNr.ToString(),
                                        Name = childCompany.ChildActorCompanyName,

                                        //Set references
                                        ActorCompanyId = actorCompanyId,

                                        AccountDim = companyMappingDim,
                                        State = (int)SoeEntityState.Active,
                                    };
                                    SetCreatedProperties(companyMappingAccount);

                                    entities.Account.AddObject(companyMappingAccount);

                                    //AccountInternal
                                    var accountInternal = new AccountInternal()
                                    {
                                        Account = companyMappingAccount,
                                    };

                                    entities.AccountInternal.AddObject(accountInternal);

                                    SaveChanges(entities);
                                }
                                else
                                {
                                    if (companyMappingAccount.AccountInternalReference.IsLoaded)
                                        companyMappingAccount.AccountInternalReference.Load();
                                }
                            }

                            #endregion

                            #region Balances

                            var updatedAccountStds = new List<AccountStd>();

                            // Get balances
                            var accountYear = AccountManager.GetAccountYear(entities, parentAccountYear.From.Date, childCompany.ChildActorCompanyId, false);

                            if (accountYear == null) {
                                return new ActionResult((int)ActionResultSave.AccountYearNotFound, GetText(517, "Redovisningsår saknas för dotterbolaget."));
                            }
                            var balances = AccountBalanceManager(actorCompanyId).GetAccountYearBalanceHeads(entities, accountYear.AccountYearId, childCompany.ChildActorCompanyId);
                            if (balances == null || balances.Count == 0)
                                loggBuilder.AppendLine(string.Format("Ingånde balanser saknas för valt år för företag {0}", childCompany.ChildActorCompanyName));

                            // Get target account year
                            var targetAccountYear = AccountManager.GetAccountYear(entities, accountYearId);

                            foreach (var balance in balances)
                            {
                                #region Account

                                string strToAccountNr = "";

                                if (string.IsNullOrEmpty(balance.AccountStd.Account.AccountNr))
                                    continue;

                                if (balance.AccountStd.Account.AccountNr.All(char.IsDigit))
                                {
                                    if (dictAccountMapping.ContainsKey(Convert.ToInt32(balance.AccountStd.Account.AccountNr)))
                                    {
                                        dictAccountMapping.TryGetValue(Convert.ToInt32(balance.AccountStd.Account.AccountNr), out int toAccountNr);
                                        strToAccountNr = toAccountNr.ToString();
                                    }
                                    else
                                    {
                                        //NOT IN THE MAPPING DICTIONARY, TRANSFER TO THE SAME ACCOUNTNR                                   
                                        strToAccountNr = balance.AccountStd.Account.AccountNr;
                                    }
                                }
                                else
                                {
                                    // The accountnr is text (not numeric) so we transfer the account without checking the accountmapping
                                    strToAccountNr = balance.AccountStd.Account.AccountNr;
                                }

                                Account parentAccount = mainAccounts.FirstOrDefault(a => a.AccountNr == strToAccountNr);
                                if (parentAccount == null)
                                {
                                    loggBuilder.AppendLine(string.Format("Konto med nummer ({0}), existerar inte på moderbolaget", strToAccountNr));
                                    continue;
                                }

                                var balanceHead = new AccountYearBalanceHead()
                                {
                                    Quantity = balance.Quantity,
                                    Balance = balance.Balance * childCompanyConversionFactor,

                                    // FK
                                    AccountYear = targetAccountYear,
                                    AccountStd = parentAccount.AccountStd,
                                };

                                //Set currency amounts
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, balanceHead);

                                if (!updatedAccountStds.Contains(parentAccount.AccountStd))
                                    updatedAccountStds.Add(parentAccount.AccountStd);

                                if (companyMappingAccount != null)
                                    balanceHead.AccountInternal.Add(companyMappingAccount.AccountInternal);

                                foreach (var accInternal in balance.AccountInternal)
                                {
                                    if (!accInternal.Account.AccountDimReference.IsLoaded)
                                        accInternal.Account.AccountDimReference.Load();

                                    var accDim = mainAccountDims.FirstOrDefault(d => d.SysSieDimNr == accInternal.Account.AccountDim.SysSieDimNr);
                                    if (accDim != null && (accDim.AccountDimId != dimToMapCompanyTo || companyMappingAccount != null))
                                    {
                                        var acc = childCompany.MatchInternalAccountsOnNr ? accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr) : accDim.Account.FirstOrDefault(a => a.AccountNr == accInternal.Account.AccountNr && a.Name == accInternal.Account.Name);
                                        if (acc != null)
                                        {
                                            if (!acc.AccountInternalReference.IsLoaded)
                                                acc.AccountInternalReference.Load();

                                            if (acc.AccountInternal != null)
                                                balanceHead.AccountInternal.Add(acc.AccountInternal);
                                        }
                                    }
                                }

                                #endregion

                                SetCreatedProperties(balanceHead);
                                entities.AccountYearBalanceHead.AddObject(balanceHead);

                                var transferRow = new CompanyGroupTransferRow()
                                {
                                    ChildActorCompanyId = childCompany.ChildActorCompanyId,
                                    ConversionFactor = childCompanyConversionFactor,
                                    AccountYearBalanceHead = balanceHead,
                                };

                                SetCreatedProperties(transferRow);

                                companyGroupTransferRows.Add(transferRow);
                            }

                            var companyGroupTransferHead = new CompanyGroupTransferHead()
                            {
                                ActorCompanyId = base.ActorCompanyId,
                                AccountYearId = accountYearId,
                                TransferType = (int)CompanyGroupTransferType.Balance,
                                Status = (int)CompanyGroupTransferStatus.Transfered,
                            };

                            SetCreatedProperties(companyGroupTransferHead);
                            entities.CompanyGroupTransferHead.AddObject(companyGroupTransferHead);

                            foreach (var row in companyGroupTransferRows)
                            {
                                row.CompanyGroupTransferHead = companyGroupTransferHead;
                                entities.CompanyGroupTransferRow.AddObject(row);
                            }

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2205, "Kunde inte spara"));

                            //Update balance on all accounts that was updated
                            result = AccountBalanceManager(actorCompanyId).CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, accountYearId, updatedAccountStds);

                            if (result.Success)
                            {
                                result.InfoMessage = loggBuilder.ToString();
                                result.Value2 = result.InfoMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                transaction.Complete();
                            }


                            #endregion

                        } // End of foreach "childCompany"

                    } // End of "using transaction"
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                } // End of try/catch/finally 

            } // End of "using entities" 

            return result;

        }

        public List<CompanyGroupTransferHeadDTO> GetCompanyGroupTransferHistoryBalance(int accountYearId, int actorCompanyId, int transferType)
        {
            var dtos = new List<CompanyGroupTransferHeadDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.CompanyGroupTransferHistoryView.NoTracking();

            var items = (from view in entitiesReadOnly.CompanyGroupTransferHistoryView
                         where view.ActorCompanyId == actorCompanyId &&
                         view.TransferType == transferType
                         orderby view.TransferDate descending
                         select view);

            foreach (var group in items.GroupBy(i => i.CompanyGroupTransferHeadId))
            {
                var head = group.FirstOrDefault();
                if (head != null)
                {
                    var dto = new CompanyGroupTransferHeadDTO()
                    {
                        CompanyGroupTransferHeadId = head.CompanyGroupTransferHeadId,
                        ActorCompanyId = head.ActorCompanyId,
                        AccountYearId = head.AccountYearId,
                        AccountYearText = head.AccountYearFrom.ToShortDateString() + " - " + head.AccountYearTo.ToShortDateString(),
                        FromAccountPeriodText = head.AccountYearFrom.Year.ToString() + " - " + head.AccountYearFrom.Month.ToString(),
                        ToAccountPeriodText = head.AccountYearTo.Year.ToString() + " - " + head.AccountYearTo.Month.ToString(),
                        TransferDate = head.TransferDate,
                        TransferType = head.TransferType.HasValue ? (CompanyGroupTransferType)head.TransferType.Value : CompanyGroupTransferType.None,
                        TransferStatus = head.TransferStatus.HasValue ? (CompanyGroupTransferStatus)head.TransferStatus.Value : CompanyGroupTransferStatus.None,
                        IsOnlyVoucher = false,
                        CompanyGroupTransferRows = new List<CompanyGroupTransferRowDTO>(),
                    };

                    dto.TransferTypeName = GetText(2201, "Ingående balanser");

                    if (dto.TransferStatus == CompanyGroupTransferStatus.Transfered)
                        dto.TransferStatusName = GetText(8847, (int)TermGroup.General, "Överförd");
                    else if (dto.TransferStatus == CompanyGroupTransferStatus.PartlyDeleted)
                        dto.TransferStatusName = GetText(8848, (int)TermGroup.General, "Delvis borttagen");
                    else
                        dto.TransferStatusName = GetText(2244, (int)TermGroup.General, "Borttagen");

                    foreach (var view in group)
                    {
                        var row = new CompanyGroupTransferRowDTO()
                        {
                            CompanyGroupTransferRowId = view.CompanyGroupTransferRowId,
                            ChildActorCompanyNrName = view.CompanyNr.HasValue ? view.CompanyNr.Value.ToString() + " - " + view.CompanyName : view.CompanyName,
                            Status = dto.TransferStatusName,
                            BudgetHeadId = view.BudgetHeadId,
                            BudgetName = view.Text,
                            VoucherSeriesId = view.VoucherSeriesId,
                            VoucherSeriesName = view.VoucherSeriesName,
                            ConversionFactor = view.ConversionFactor,
                            Created = view.TransferDate.Value,
                            AccountPeriodText = view.AccountPeriodFrom.HasValue ? view.AccountPeriodFrom.Value.Year.ToString() + " - " + view.AccountPeriodFrom.Value.Month.ToString() : " ",
                        };

                        dto.CompanyGroupTransferRows.Add(row);
                    }

                    dtos.Add(dto);
                }
            }

            return dtos.OrderByDescending(d => d.TransferDate).ToList();
        }

        public ActionResult DeleteCompanyGroupTransfer(int actorCompanyId, int companyGroupTransferHeadId)
        {
            ActionResult result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        List<int> vouchersToDelete = new List<int>();

                        var head = (from h in entities.CompanyGroupTransferHead
                                    .Include("CompanyGroupTransferRow.VoucherHead")
                                    .Include("CompanyGroupTransferRow.BudgetRow.BudgetRowPeriod")
                                    .Include("CompanyGroupTransferRow.AccountYearBalanceHead")
                                    where h.ActorCompanyId == actorCompanyId &&
                                    h.CompanyGroupTransferHeadId == companyGroupTransferHeadId
                                    select h).FirstOrDefault();

                        if (head == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "CompanyGroupTransferHead");

                        if (head.TransferType == (int)CompanyGroupTransferType.Budget)
                        {
                            foreach (var row in head.CompanyGroupTransferRow.Where(r => r.BudgetRowId.HasValue))
                            {
                                foreach (var period in row.BudgetRow.BudgetRowPeriod.ToList())
                                {
                                    DeleteEntityItem(entities, period, transaction);
                                }

                                if (!row.BudgetRow.AccountStdReference.IsLoaded)
                                    row.BudgetRow.AccountStdReference.Load();

                                row.BudgetRow.AccountInternal.Clear();
                                ChangeEntityState(row.BudgetRow, SoeEntityState.Deleted);

                                row.BudgetRowId = null;
                                SetModifiedProperties(row);

                                head.Status = (int)CompanyGroupTransferStatus.Deleted;
                                SetModifiedProperties(head);
                            }

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                            {
                                transaction.Dispose();
                                return result;
                            }

                        }
                        else if (head.TransferType == (int)CompanyGroupTransferType.Balance)
                        {
                            var updatedAccountStds = new List<AccountStd>();
                            foreach (var row in head.CompanyGroupTransferRow.Where(r => r.AccountYearBalanceHeadId.HasValue))
                            {
                                if (!row.AccountYearBalanceHead.AccountStdReference.IsLoaded)
                                    row.AccountYearBalanceHead.AccountStdReference.Load();

                                if (!updatedAccountStds.Contains(row.AccountYearBalanceHead.AccountStd))
                                    updatedAccountStds.Add(row.AccountYearBalanceHead.AccountStd);

                                var accountYearBalanceHead = AccountBalanceManager(actorCompanyId).GetAccountYearBalanceHeadWithInternals(entities, row.AccountYearBalanceHeadId.Value, actorCompanyId);
                                if (accountYearBalanceHead != null)
                                {
                                    accountYearBalanceHead.AccountInternal.Clear();
                                    entities.DeleteObject(accountYearBalanceHead);
                                }

                                SetModifiedProperties(row);

                                head.Status = (int)CompanyGroupTransferStatus.Deleted;
                                SetModifiedProperties(head);
                            }

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                            {
                                transaction.Dispose();
                                return result;
                            }

                            //Update balance on all accounts that was updated
                            result = AccountBalanceManager(actorCompanyId).CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, head.AccountYearId, updatedAccountStds);
                        }
                        else
                        {
                            foreach (var row in head.CompanyGroupTransferRow.Where(r => r.VoucherHeadId.HasValue))
                            {
                                vouchersToDelete.Add(row.VoucherHeadId.Value);

                                row.VoucherHeadId = null;
                                SetModifiedProperties(row);
                            }


                            head.Status = (int)CompanyGroupTransferStatus.Deleted;
                            SetModifiedProperties(head);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                            {
                                transaction.Dispose();
                                return result;
                            }

                            foreach (var id in vouchersToDelete)
                            {
                                entities.DeleteVoucherSuperSupport(id);
                            }
                        }


                        transaction.Complete();
                    } // End of "using transaction"
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

            }

            return result;
        }

        private Dictionary<int, int> MappingToAccountDictionary(List<CompanyGroupMappingRow> mappingRows)
        {
            Dictionary<int, int> dictAccounts = new Dictionary<int, int>();

            if (mappingRows != null)
            {
                foreach (CompanyGroupMappingRow row in mappingRows)
                {
                    if (row.ChildAccountTo == null)
                    {
                        if (!dictAccounts.ContainsKey(row.ChildAccountFrom))
                            dictAccounts.Add(row.ChildAccountFrom, row.GroupCompanyAccount);
                    }
                    else
                    {
                        for (int i = row.ChildAccountFrom; i <= row.ChildAccountTo; i++)
                        {
                            if (!dictAccounts.ContainsKey(i))
                                dictAccounts.Add(i, row.GroupCompanyAccount);
                        }
                    }
                }
            }

            return dictAccounts;
        }

        #endregion

        #endregion

        #region Copy from template

        public bool CopyAllFromTemplateCompany(int templateCompanyId, int actorCompanyId, int userId, bool update, bool liberCopy = false)
        {
            CopyFromTemplateCompanyInputDTO inputDTO = new CopyFromTemplateCompanyInputDTO()
            {
                TemplateCompanyId = templateCompanyId,
                ActorCompanyId = actorCompanyId,
                UserId = userId,
                Update = update,
                LiberCopy = liberCopy,
                TemplateCompanyName = GetCompanyFromCache(templateCompanyId)?.Name ?? null

            };
            inputDTO.DoCopy(TemplateCompanyCopy.All);

            return this.CopyFromTemplateCompany(inputDTO);
        }

        public bool CopyFromTemplateCompany(CopyFromTemplateCompanyInputDTO inputDTO)
        {
            var isCombinedkey = IsCombinedKey(inputDTO.TemplateCompanyId); // Show that a beta template company was picked.
            var actorcompany = GetActorCompanyIdFromCombinedKey(inputDTO.TemplateCompanyId);

            if (actorcompany != inputDTO.TemplateCompanyId)
            {
                inputDTO.SysCompDbId = GetSysCompDbIdFromCombinedKey(inputDTO.TemplateCompanyId);

                if (inputDTO.SysCompDbId != 0 && isCombinedkey)
                {
                    inputDTO.TemplateCompanyId = actorcompany;
                    CompanyTemplateManager companyTemplateManager = new CompanyTemplateManager(base.parameterObject);
                    companyTemplateManager.CopyFromTemplateCompany(inputDTO);
                    return true;
                }
            }
            LogInfo($"Using OLD CopyFromTemplateCompany for company {inputDTO.ActorCompanyId}  {base.GetCompanyFromCache(inputDTO.ActorCompanyId)?.Name} from template {inputDTO.TemplateCompanyId} {inputDTO.TemplateCompanyName} User {GetUserDetails()}");
            bool copyError = false;

            #region Init

            var accountDimMapping = new Dictionary<int, AccountDim>();
            var accountStdMapping = new Dictionary<int, AccountStd>();
            var accountInternalMapping = new Dictionary<int, AccountInternal>();
            var attestRoleMapping = new Dictionary<int, AttestRole>();
            var categoryMapping = new Dictionary<int, Category>();
            var dayTypeMapping = new Dictionary<int, DayType>();
            var employeeGroupMapping = new Dictionary<int, EmployeeGroup>();
            var payrollGroupMapping = new Dictionary<int, PayrollGroup>();
            var vacationGroupMapping = new Dictionary<int, VacationGroup>();
            var payrollPriceFormulaMapping = new Dictionary<int, PayrollPriceFormula>();
            var payrollPriceTypeMapping = new Dictionary<int, PayrollPriceType>();
            var pricelistMapping = new Dictionary<int, int>();
            var reportMapping = new Dictionary<int, Report>();
            var roleMapping = new Dictionary<int, Role>();
            var shiftTypeMapping = new Dictionary<int, ShiftType>();
            var timeDeviationCausesMapping = new Dictionary<int, TimeDeviationCause>();
            var timeCodeMapping = new Dictionary<int, TimeCode>();
            var timeCodeBreakGroupMapping = new Dictionary<int, TimeCodeBreakGroup>();
            var timeScheduleTypeMapping = new Dictionary<int, TimeScheduleType>();
            var vatCodeMapping = new Dictionary<int, int>();
            var payrollLevelMapping = new Dictionary<int, PayrollLevel>();

            #endregion

            #region Core

            if (inputDTO.DoCopy(TemplateCompanyCopy.All) &&
                !CopyImportsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyFieldSettings) &&
                !CopyCompanyFieldSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.RolesAndFeatures) &&
                !CopyRolesAndFeaturesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref roleMapping).Success)
                copyError = true;

            #endregion

            #region Reports
            // Packages previously removed - inputDTO.DoCopy(TemplateCompanyCopy.ReportPackages)
            if (inputDTO.DoCopy(TemplateCompanyCopy.ReportsAndReportTemplates) &&
                !CopyReportsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, false, inputDTO.DoCopy(TemplateCompanyCopy.ReportGroupsAndReportHeaders), inputDTO.DoCopy(TemplateCompanyCopy.ReportSettings), inputDTO.DoCopy(TemplateCompanyCopy.ReportSelections), inputDTO.Update, ref reportMapping, ref roleMapping).Success)
                copyError = true;

            #endregion

            #region Economy

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestSupplier) &&
                !CopyAttestFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, SoeModule.Economy, inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestSupplier), ref categoryMapping, ref attestRoleMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountStds) &&
                !CopyAccountStdsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref accountStdMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountInternals) &&
                !CopyAccountInternalsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref accountDimMapping, ref accountInternalMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.VoucherSeriesTypes) &&
                !CopyVoucherSeriesTypesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountYearsAndPeriods) &&
                !CopyAccountYearsAndPeriodsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PaymentMethods) &&
                !CopyPaymentMethodsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PaymentConditions) &&
                !CopyPaymentConditionsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.BillingSettings), inputDTO.DoCopy(TemplateCompanyCopy.CustomerSettings), inputDTO.DoCopy(TemplateCompanyCopy.SupplierSettings)).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.GrossProfitCodes) &&
                !CopyGrossProfitCodesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Inventory) &&
                !CopyInventoryFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.AutomaticAccountDistributionTemplates) &&
                !CopyAccountDistributionTemplatesFromTemplateCompany(SoeAccountDistributionType.Auto, inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PeriodAccountDistributionTemplates) &&
                !CopyAccountDistributionTemplatesFromTemplateCompany(SoeAccountDistributionType.Period, inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.DistributionCodes) &&
                !CopyDistributionCodesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.VoucherTemplates) &&
             !CopyVoucherTemplatesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.ResidualCodes) &&
             !CopyResidualCodesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, accountStdMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Suppliers) &&
             !CopySuppliersFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;

            #endregion

            #region Billing

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestBilling) &&
                !CopyAttestFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, SoeModule.Billing, inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestBilling), ref categoryMapping, ref attestRoleMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.BaseProductsBilling) &&
                !CopyBaseProductsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, false, SoeModule.Billing, ref vatCodeMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.VatCodes) &&
                !CopyVatCodesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref vatCodeMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyProducts) &&
                !CopyCompanyProductsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, false, inputDTO.DoCopy(TemplateCompanyCopy.CompanyExternalProducts), SoeModule.Billing, ref vatCodeMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PricesLists) &&
                !CopyPriceListsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref pricelistMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.SupplierAgreements) && !CopySupplierAgreementsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, false, SoeModule.Billing, ref pricelistMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Checklists) &&
                !CopyChecklistsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, reportMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.EmailTemplates) &&
                !CopyEmailTemplatesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyWholesellerPricelists) &&
                !CopyCompanyWholesellerPricelistsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if ((inputDTO.DoCopy(TemplateCompanyCopy.Customers) || inputDTO.LiberCopy) &&
                !CopyCustomersFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.Update, ref reportMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Contracts) &&
                !CopyCustomerInvoicesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, SoeOriginType.Contract).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Orders) &&
                !CopyCustomerInvoicesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, SoeOriginType.Order).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Orders) &&
                !CopyOrderTemplatesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, reportMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PriceRules) &&
                !CopyPriceRulesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;

            #endregion

            #region Time

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestTime) &&
                !CopyAttestFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, SoeModule.Time, inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestTime), ref categoryMapping, ref attestRoleMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.BaseAccountsTime) &&
                !CopyBaseAccountsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, SoeModule.Time).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.DaytypesHalfDaysAndHolidays) &&
                !CopyDaytypesHalfDaysAndHolidaysFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref dayTypeMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.TimePeriods) &&
                !CopyTimePeriodsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Positions) &&
                !CopyPositionsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas) || inputDTO.DoCopy(TemplateCompanyCopy.VacationGroups))
                CreateFormulasAndPriceTypeDicts(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, !inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas), ref payrollPriceFormulaMapping, ref payrollPriceTypeMapping);
            if (inputDTO.DoCopy(TemplateCompanyCopy.VacationGroups) &&
                !CopyVacationGroupsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref payrollPriceFormulaMapping, ref payrollPriceTypeMapping, ref accountStdMapping, ref vacationGroupMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas) &&
               !CopyPayrollLevelsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, ref payrollLevelMapping).Success)
                copyError = true;
            //Must be after vacationGoups
            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas) &&
                !CopyPayrollGroupsPriceTypesAndPriceFormulasFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref payrollGroupMapping, ref payrollPriceFormulaMapping, ref payrollPriceTypeMapping, ref reportMapping, ref accountStdMapping, ref payrollLevelMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeScheduleTypesAndShiftTypes) &&
                !CopyTimeScheduleTypesAndShiftTypesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref shiftTypeMapping, ref timeScheduleTypeMapping, ref accountInternalMapping, ref categoryMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.Skills) &&
                !CopySkillsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.ScheduleCykles) &&
                !CopyScheduleCyclesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.FollowUpTypes) &&
                !CopyFollowUpTypesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            //Must be after PayrollGroups, PayrollPriceTypes, PayrollPriceFormulas and TimeScheduleTypesAndShiftTypes
            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollProductsAndTimeCodes) &&
                !CopyPayrollProductsAndTimeCodesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.TimeBreakTemplate), inputDTO.DoCopy(TemplateCompanyCopy.ProjectSettings), ref timeCodeMapping, ref timeCodeBreakGroupMapping, ref payrollGroupMapping, ref payrollPriceFormulaMapping, ref payrollPriceTypeMapping, ref accountStdMapping, ref accountInternalMapping).Success)
                copyError = true;
            //Must be after TimeCodes
            if (inputDTO.DoCopy(TemplateCompanyCopy.DeviationCauses) &&
                !CopyTimeDeviationCausesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.VacationGroups), ref timeDeviationCausesMapping, ref timeCodeMapping).Success)
                copyError = true;
            //Must be after Attest, TimeCodes, TimeDeviationCauses, DayTypes and TimePeriods
            if (inputDTO.DoCopy(TemplateCompanyCopy.EmployeeGroups) &&
                !CopyEmployeeGroupsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref employeeGroupMapping, ref timeDeviationCausesMapping, ref timeCodeMapping).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.EmploymentTypes) &&
                !CopyEmploymentTypesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId).Success)
                copyError = true;
            //Must be after TimeCodes and PayrollProducts
            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAccumulators) &&
                !CopyTimeAccumulatorsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref employeeGroupMapping, ref timeCodeMapping).Success)
                copyError = true;
            //Must be after TimeCodes, EmployeeGroups, TimeCodes, TimeDeviationCauses and TimeScheduleTypes
            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeRules) &&
                !CopyTimeRulesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref employeeGroupMapping, ref timeCodeMapping, ref timeDeviationCausesMapping, ref timeScheduleTypeMapping, ref dayTypeMapping).Success)
                copyError = true;
            //Must be after PayrollProducts and TimeCodes
            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAbsenseRules) &&
                !CopyTimeAbsenseRulesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            //Must be after PayrollProducts, TimeCodes, DayTypes and EmployeeGroups
            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeAttestRules) &&
                !CopyTimeAttestRulesFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, ref dayTypeMapping, ref employeeGroupMapping, ref timeCodeMapping).Success)
                copyError = true;
            if (!CopyCompanyExternalCodes(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, employeeGroupMapping, payrollGroupMapping, roleMapping, attestRoleMapping).Success)
                copyError = true;
            if (employeeGroupMapping.Any())
                if (!CopyCompanyCollectiveAgreements(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, employeeGroupMapping, payrollGroupMapping, vacationGroupMapping).Success)
                    copyError = true;
            if (payrollPriceFormulaMapping.Any())
                if (!CopyEmployeeTemplates(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, false, payrollPriceFormulaMapping).Success)
                    copyError = true;

            #endregion


            #region Settings

            if (inputDTO.DoCopy(TemplateCompanyCopy.ManageSettings) &&
                !CopyManageSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountingSettings) &&
                !CopyAccountingSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.VoucherSeriesTypes), inputDTO.DoCopy(TemplateCompanyCopy.AccountStds)).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.SupplierSettings) &&
                !CopySupplierSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.AccountStds), inputDTO.DoCopy(TemplateCompanyCopy.VoucherSeriesTypes)).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.CustomerSettings) &&
                !CopyCustomerSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.AccountStds), inputDTO.DoCopy(TemplateCompanyCopy.VoucherSeriesTypes)).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.BillingSettings) &&
                !CopyBillingSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.AccountStds)).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.TimeSettings) &&
                !CopyTimeSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.AccountStds), inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestTime), inputDTO.DoCopy(TemplateCompanyCopy.EmployeeGroups), inputDTO.DoCopy(TemplateCompanyCopy.PayrollGroupsPriceTypesAndPriceFormulas), inputDTO.DoCopy(TemplateCompanyCopy.VacationGroups), inputDTO.DoCopy(TemplateCompanyCopy.PayrollProductsAndTimeCodes), inputDTO.DoCopy(TemplateCompanyCopy.TimePeriods)).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.PayrollSettings) &&
               !CopyPayrollSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, inputDTO.DoCopy(TemplateCompanyCopy.AccountStds), inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestTime), inputDTO.DoCopy(TemplateCompanyCopy.PayrollProductsAndTimeCodes)).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.ProjectSettings) &&
                !CopyProjectSettingsFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update).Success)
                copyError = true;
            if (inputDTO.DoCopy(TemplateCompanyCopy.SigningSettings) &&
                !CopyAttestFromTemplateCompany(inputDTO.ActorCompanyId, inputDTO.TemplateCompanyId, inputDTO.UserId, inputDTO.Update, SoeModule.Manage, true, ref categoryMapping, ref attestRoleMapping).Success)
                copyError = true;

            #endregion

            return !copyError;
        }

        #region Core

        public ActionResult CopyCompanyFieldSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            List<CompanyFieldSetting> existingCompanyFieldSettings = FieldSettingManager.GetCompanyFieldSettings(newCompanyId);
            List<CompanyFieldSetting> templateCompanyFieldSettings = FieldSettingManager.GetCompanyFieldSettings(templateCompanyId);

            #endregion

            #region CompanyFieldSetting

            foreach (CompanyFieldSetting templateCompanyFieldSetting in templateCompanyFieldSettings)
            {
                CompanyFieldSetting setting = existingCompanyFieldSettings.FirstOrDefault(s => s.FormId == templateCompanyFieldSetting.FormId && s.FieldId == templateCompanyFieldSetting.FieldId && s.SysSettingId == templateCompanyFieldSetting.SysSettingId);
                if (setting == null)
                {
                    try
                    {
                        result = FieldSettingManager.SaveCompanyFieldSetting(templateCompanyFieldSetting.FormId, templateCompanyFieldSetting.FieldId, templateCompanyFieldSetting.SysSettingId, templateCompanyFieldSetting.Value, newCompanyId);
                        if (!result.Success)
                            result = LogCopyError("CompanyFieldSetting", "SysSettingId", templateCompanyFieldSetting.SysSettingId, "", "", templateCompanyId, newCompanyId, add: true);
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("CompanyFieldSetting", "SysSettingId", templateCompanyFieldSetting.SysSettingId, "", "", templateCompanyId, newCompanyId, ex);
                    }
                }
            }

            #endregion

            return result;
        }

        public ActionResult CopyRolesAndFeaturesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, Role> roleMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            #region Prereq

            List<Role> templateRoles = RoleManager.GetRolesByCompany(templateCompanyId, loadExternalCode: true);
            List<RoleFeature> templateRoleFeatures = FeatureManager.GetRoleFeaturesForCompany(templateCompanyId);
            List<CompanyFeature> templateCompanyFeatures = FeatureManager.GetCompanyFeatures(templateCompanyId);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                List<Role> existingRoles = RoleManager.GetRolesByCompany(entities, newCompanyId, loadExternalCode: true);
                List<RoleFeature> existingRoleFeatures = FeatureManager.GetRoleFeaturesForCompany(entities, newCompanyId);
                List<CompanyFeature> existingCompanyFeatures = FeatureManager.GetCompanyFeatures(entities, newCompanyId);

                #region CompanyFeatures

                foreach (CompanyFeature templateCompanyFeature in templateCompanyFeatures)
                {
                    #region CompanyFeature

                    try
                    {
                        CompanyFeature companyFeature = existingCompanyFeatures.FirstOrDefault(cf => cf.SysFeatureId == templateCompanyFeature.SysFeatureId);
                        if (companyFeature == null)
                        {
                            companyFeature = new CompanyFeature()
                            {
                                SysFeatureId = templateCompanyFeature.SysFeatureId,
                                SysPermissionId = templateCompanyFeature.SysPermissionId,

                                //Set FK
                                ActorCompanyId = newCompanyId,
                            };
                            entities.CompanyFeature.AddObject(companyFeature);
                            SetCreatedProperties(companyFeature);
                            existingCompanyFeatures.Add(companyFeature);
                        }
                        else
                        {
                            companyFeature.SysPermissionId = templateCompanyFeature.SysPermissionId;
                            SetModifiedProperties(companyFeature);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCopyError("CompanyFeature", "SysFeatureId", templateCompanyFeature.SysFeatureId, "", "", templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                result = SaveChanges(entities, null, useBulkSaveChanges: true);
                if (!result.Success)
                    result = LogCopyError("CompanyFeature", "SysFeatureId", 0, "", "", templateCompanyId, newCompanyId);

                #endregion

                #region Roles/RoleFeatures

                foreach (Role templateRole in templateRoles)
                {
                    try
                    {
                        #region Role

                        Role role = existingRoles.FirstOrDefault(r => r.TermId == templateRole.TermId && r.Name == templateRole.Name);
                        if (role == null)
                        {
                            //Do not copy role with termid = 1, will be created automatically
                            if (templateRole.IsAdmin)
                            {
                                #region Admin

                                role = RoleManager.GetRoleAdmin(entities, newCompanyId);
                                if (role == null)
                                    continue;

                                #endregion
                            }
                            else
                            {
                                #region Add

                                role = new Role()
                                {
                                    TermId = templateRole.TermId,
                                    Name = templateRole.Name,
                                    ExternalCodesString = templateRole.ExternalCodesString,
                                    Sort = templateRole.Sort,

                                    //Set FK
                                    ActorCompanyId = newCompanyId,
                                };
                                SetCreatedProperties(role);
                                entities.Role.AddObject(role);
                                existingRoles.Add(role);

                                #endregion
                            }
                        }

                        result = SaveChanges(entities, null, useBulkSaveChanges: true);
                        if (!result.Success)
                            LogCopyError("Role", "RoleId", templateRole.RoleId, "", templateRole.Name, templateCompanyId, newCompanyId, add: true);

                        roleMapping.Add(templateRole.RoleId, role);

                        #endregion

                        #region RoleFeatures

                        foreach (RoleFeature templateRoleFeature in templateRoleFeatures.Where(rf => rf.RoleId == templateRole.RoleId))
                        {
                            #region RoleFeature

                            try
                            {
                                RoleFeature roleFeature = existingRoleFeatures.FirstOrDefault(rf => rf.SysFeatureId == templateRoleFeature.SysFeatureId && rf.RoleId == role.RoleId);
                                if (roleFeature == null)
                                {
                                    roleFeature = new RoleFeature()
                                    {
                                        SysFeatureId = templateRoleFeature.SysFeatureId,
                                        SysPermissionId = templateRoleFeature.SysPermissionId,

                                        //Set FK
                                        RoleId = role.RoleId,
                                    };
                                    entities.RoleFeature.AddObject(roleFeature);
                                    SetCreatedProperties(roleFeature);
                                    existingRoleFeatures.Add(roleFeature);
                                }
                                else
                                {
                                    roleFeature.SysPermissionId = templateRoleFeature.SysPermissionId;
                                    SetModifiedProperties(roleFeature);
                                }
                            }
                            catch (Exception ex)
                            {
                                result = LogCopyError("RoleFeature", "SysFeatureId", templateRoleFeature.SysFeatureId, "", templateRole.Name, templateCompanyId, newCompanyId, ex);
                            }

                            #endregion
                        }

                        result = SaveChanges(entities, null, useBulkSaveChanges: true);
                        if (!result.Success)
                            result = LogCopyError("RoleFeature", "SysFeatureId", 0, "", "", templateCompanyId, newCompanyId);

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("Role", "RoleId", templateRole.RoleId, "", templateRole.Name, templateCompanyId, newCompanyId, ex);
                    }
                }

                #endregion
            }

            return result;
        }

        #endregion

        #region Economy/Voucher/Ledger

        public ActionResult CopyAccountStdsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, AccountStd> accountStdMapping)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Get Company
                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                AccountDim newAccountDimStd = AccountManager.GetAccountDimStd(entities, newCompanyId);
                if (newAccountDimStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                AccountDim templateAccountDimStd = AccountManager.GetAccountDimStd(entities, templateCompanyId);
                if (templateAccountDimStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<Account> templateAccounts = AccountManager.GetAccountsByDim(entities, templateAccountDimStd.AccountDimId, templateCompanyId, true, true, true).ToList();
                if (templateAccounts.IsNullOrEmpty())
                    return result;

                List<Account> existingAccounts = update ? AccountManager.GetAccountsByDim(entities, newAccountDimStd.AccountDimId, newCompanyId, true, true, false).ToList() : new List<Account>();

                #endregion

                foreach (Account templateAccount in templateAccounts)
                {
                    #region AccountStd

                    try
                    {
                        if (templateAccount.AccountStd == null)
                            continue;

                        Account existingAccount = existingAccounts.FirstOrDefault(i => i.AccountNr == templateAccount.AccountNr && i.Name == templateAccount.Name);
                        if (existingAccount == null)
                        {
                            //Account
                            existingAccount = new Account()
                            {
                                AccountNr = templateAccount.AccountNr,
                                Name = templateAccount.Name,
                                ExternalCode = templateAccount.ExternalCode,

                                //Set references
                                Company = newCompany,
                                AccountDim = newAccountDimStd,
                            };
                            SetCreatedProperties(existingAccount);

                            //AccountStd
                            existingAccount.AccountStd = new AccountStd()
                            {
                                AccountTypeSysTermId = templateAccount.AccountStd.AccountTypeSysTermId,
                                SysVatAccountId = templateAccount.AccountStd.SysVatAccountId,
                                Unit = templateAccount.AccountStd.Unit,
                                UnitStop = templateAccount.AccountStd.UnitStop,
                                AmountStop = templateAccount.AccountStd.AmountStop,
                            };

                            if (!templateAccount.AccountStd.AccountSru.IsLoaded)
                                templateAccount.AccountStd.AccountSru.Load();

                            //AccountSRU
                            foreach (AccountSru accountSru in templateAccount.AccountStd.AccountSru)
                            {
                                AccountSru accountSruNew = new AccountSru()
                                {
                                    SysAccountSruCodeId = accountSru.SysAccountSruCodeId,
                                };
                                existingAccount.AccountStd.AccountSru.Add(accountSruNew);
                            }

                            //AccountHistory
                            AccountHistory accountHistory = new AccountHistory()
                            {
                                Name = existingAccount.Name,
                                AccountNr = existingAccount.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = null,
                                SieKpTyp = existingAccount.AccountStd.SieKpTyp,

                                //Set references
                                Account = existingAccount,
                                User = user,
                            };
                            SetCreatedProperties(accountHistory);
                        }

                        if (!accountStdMapping.ContainsKey(templateAccount.AccountId))
                            accountStdMapping.Add(templateAccount.AccountId, existingAccount.AccountStd);
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("Account", "AccountId", templateAccount.AccountId, templateAccount.AccountNr, templateAccount.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                result = SaveChangesWithTransaction(entities);
                if (!result.Success)
                    result = LogCopyError("AccountStd", templateCompanyId, newCompanyId, saved: true);
            }

            return result;
        }

        public ActionResult CopyAccountInternalsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, AccountDim> accountDimMapping, ref Dictionary<int, AccountInternal> accountInternalMapping)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Get Company
                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Get User
                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                //Get AccountDims once
                List<AccountDim> templateAccountDimInternals = AccountManager.GetAccountDimInternalsByCompany(entities, templateCompanyId).ToList();
                if (templateAccountDimInternals.IsNullOrEmpty())
                    return result;

                // If update, get accountinternals from new company also
                List<AccountDim> existingAccountDimInternals = update ? AccountManager.GetAccountDimInternalsByCompany(entities, newCompanyId) : new List<AccountDim>();

                //Get Accounts once
                List<Account> templateAccountInternals = AccountManager.GetAccountsInternalsByCompany(entities, templateCompanyId, true, true, true).ToList();

                #endregion

                foreach (AccountDim templateAccountDimInternal in templateAccountDimInternals)
                {
                    #region AccountDim

                    try
                    {
                        AccountDim newAccountDimInternal = existingAccountDimInternals.FirstOrDefault(i => i.Name == templateAccountDimInternal.Name && i.AccountDimNr == templateAccountDimInternal.AccountDimNr);
                        if (newAccountDimInternal == null)
                        {
                            newAccountDimInternal = new AccountDim()
                            {
                                AccountDimNr = templateAccountDimInternal.AccountDimNr,
                                Name = templateAccountDimInternal.Name,
                                ShortName = templateAccountDimInternal.ShortName,
                                SysSieDimNr = templateAccountDimInternal.SysSieDimNr,
                                SysAccountStdTypeParentId = templateAccountDimInternal.SysAccountStdTypeParentId,
                                LinkedToProject = templateAccountDimInternal.LinkedToProject,
                                UseInSchedulePlanning = templateAccountDimInternal.UseInSchedulePlanning,
                                ExcludeinAccountingExport = templateAccountDimInternal.ExcludeinAccountingExport,
                                ExcludeinSalaryExport = templateAccountDimInternal.ExcludeinSalaryExport,
                                UseVatDeduction = templateAccountDimInternal.UseVatDeduction,
                                MandatoryInCustomerInvoice = templateAccountDimInternal.MandatoryInCustomerInvoice,
                                MandatoryInOrder = templateAccountDimInternal.MandatoryInOrder,
                                OnlyAllowAccountsWithParent = templateAccountDimInternal.OnlyAllowAccountsWithParent,

                                //Set FK
                                AccountDimId = templateAccountDimInternal.AccountDimId,

                                //Set references
                                Company = newCompany,
                            };
                            SetCreatedProperties(newAccountDimInternal);

                            List<Account> templateAccountsForDim = templateAccountInternals.Where(a => a.AccountDimId == templateAccountDimInternal.AccountDimId).ToList();
                            foreach (Account templateAccountInternal in templateAccountsForDim)
                            {
                                #region AccountInternal

                                try
                                {
                                    //Account
                                    Account newAccount = new Account()
                                    {
                                        AccountNr = templateAccountInternal.AccountNr,
                                        Name = templateAccountInternal.Name,
                                        ExternalCode = templateAccountInternal.ExternalCode,
                                        Description = templateAccountInternal.Description,

                                        //Set references
                                        Company = newCompany,
                                        AccountDim = newAccountDimInternal,
                                    };
                                    SetCreatedProperties(newAccount);

                                    //AccountInternal
                                    newAccount.AccountInternal = new AccountInternal()
                                    {
                                    };

                                    //AccountHistory
                                    AccountHistory accountHistory = new AccountHistory()
                                    {
                                        User = user,
                                        Account = newAccount,
                                        Name = newAccount.Name,
                                        AccountNr = newAccount.AccountNr,
                                        Date = DateTime.Now,
                                        SysAccountStdTypeId = null,
                                        SieKpTyp = null,
                                    };
                                    SetCreatedProperties(accountHistory);

                                    if (!accountInternalMapping.ContainsKey(templateAccountInternal.AccountId))
                                        accountInternalMapping.Add(templateAccountInternal.AccountId, newAccount.AccountInternal);
                                }
                                catch (Exception ex)
                                {
                                    result = LogCopyError("AccountInternal", "AccountInternalId", templateAccountInternal.AccountId, templateAccountInternal.AccountNr, templateAccountInternal.Name, templateCompanyId, newCompanyId, ex);
                                }

                                #endregion
                            }

                            accountDimMapping.Add(templateAccountDimInternal.AccountDimId, newAccountDimInternal);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("AccountDim", "AccountDimId", templateAccountDimInternal.AccountDimId, templateAccountDimInternal.AccountDimNr.ToString(), templateAccountDimInternal.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                foreach (AccountDim templateAccountDimInternal in templateAccountDimInternals)
                {
                    #region AccountDim parent

                    try
                    {
                        if (!accountDimMapping.ContainsKey(templateAccountDimInternal.AccountDimId))
                            continue;

                        AccountDim newAccountDimInternal = accountDimMapping[templateAccountDimInternal.AccountDimId];
                        if (newAccountDimInternal == null)
                            continue;

                        if (!templateAccountDimInternal.ParentReference.IsLoaded)
                            templateAccountDimInternal.ParentReference.Load();
                        if (templateAccountDimInternal.Parent != null && accountDimMapping.ContainsKey(templateAccountDimInternal.Parent.AccountDimId))
                            newAccountDimInternal.Parent = accountDimMapping[templateAccountDimInternal.Parent.AccountDimId];

                        #region AccountInternal parent

                        List<Account> templateAccountsForDim = templateAccountInternals.Where(a => a.AccountDimId == templateAccountDimInternal.AccountDimId).ToList();
                        foreach (Account templateAccountInternal in templateAccountsForDim)
                        {
                            if (!accountInternalMapping.ContainsKey(templateAccountInternal.AccountId))
                                continue;
                            if (!templateAccountInternal.ParentAccountId.HasValue || !accountInternalMapping.ContainsKey(templateAccountInternal.ParentAccountId.Value))
                                continue;

                            AccountInternal newAccountInternal = accountInternalMapping[templateAccountInternal.AccountId];
                            if (newAccountInternal == null)
                                continue;

                            AccountInternal newAccountInternalParent = accountInternalMapping[templateAccountInternal.ParentAccountId.Value];
                            if (newAccountInternalParent == null)
                                continue;

                            newAccountInternal.Account.Account2 = newAccountInternalParent.Account;
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogCopyError("AccountDim (parent)", "AccountDimId", templateAccountDimInternal.AccountDimId, templateAccountDimInternal.AccountDimNr.ToString(), templateAccountDimInternal.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                result = SaveChangesWithTransaction(entities);
                if (!result.Success)
                    result = LogCopyError("AccountInternal", templateCompanyId, newCompanyId, saved: true);
            }

            return result;
        }

        public ActionResult CopyAccountYearsAndPeriodsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Get Company
                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Always check existing, because standard voucher serie type is added elsewhere 
                List<AccountYear> existingAccountYears = AccountManager.GetAccountYears(entities, newCompanyId, false, false);
                List<AccountYear> templateAccountYears = AccountManager.GetAccountYears(entities, templateCompanyId, false, false, loadVoucherSeries: true);

                List<VoucherSeriesType> templateVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(entities, templateCompanyId, false);
                List<VoucherSeriesType> existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(entities, newCompanyId, false);

                #endregion

                foreach (AccountYear templateAccountYear in templateAccountYears)
                {
                    #region AccountYearsAndPeriods

                    try
                    {
                        AccountYear existingAccountYear = existingAccountYears.FirstOrDefault(a => a.From == templateAccountYear.From && a.To == templateAccountYear.To && a.Status == templateAccountYear.Status);
                        if (existingAccountYear == null)
                        {
                            existingAccountYear = new AccountYear()
                            {
                                Company = newCompany,
                                From = templateAccountYear.From,
                                To = templateAccountYear.To,
                                Status = templateAccountYear.Status,
                                Created = DateTime.Now,
                            };

                            result = AccountManager.AddAccountYear(entities, existingAccountYear, newCompanyId);
                            if (!result.Success)
                                result = LogCopyError("AccountYear", templateCompanyId, newCompanyId, saved: false);

                            // Account periods
                            List<AccountPeriod> templateAccountPeriods = AccountManager.GetAccountPeriods(templateAccountYear.AccountYearId, false);
                            foreach (AccountPeriod templateAccountPeriod in templateAccountPeriods)
                            {
                                var newAccountPeriod = new AccountPeriod()
                                {
                                    PeriodNr = templateAccountPeriod.PeriodNr,
                                    From = templateAccountPeriod.From,
                                    To = templateAccountPeriod.To,
                                    Status = templateAccountPeriod.Status,
                                    Created = DateTime.Now,
                                };

                                result = AccountManager.AddAccountPeriod(newAccountPeriod, existingAccountYear);
                                if (!result.Success)
                                    result = LogCopyError("AccountPeriod", templateCompanyId, newCompanyId, saved: false);
                            }

                            // Voucher series
                            foreach (var serie in templateAccountYear.VoucherSeries)
                            {
                                var templateSerieType = templateVoucherSeriesTypes.FirstOrDefault(s => s.VoucherSeriesTypeId == serie.VoucherSeriesTypeId);
                                if (templateSerieType != null)
                                {
                                    var matchingSerieType = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == templateSerieType.Name);
                                    if (matchingSerieType != null)
                                    {
                                        VoucherSeries voucherSerie = new VoucherSeries()
                                        {
                                            Status = serie.Status,
                                            VoucherDateLatest = serie.VoucherDateLatest,
                                            VoucherNrLatest = serie.VoucherNrLatest,
                                            VoucherSeriesTypeId = matchingSerieType.VoucherSeriesTypeId
                                        };

                                        result = VoucherManager.AddVoucherSeries(entities, voucherSerie, newCompanyId, existingAccountYear.AccountYearId, matchingSerieType.VoucherSeriesTypeId);
                                        if (!result.Success)
                                            result = LogCopyError("VoucherSerie", templateCompanyId, newCompanyId, saved: false);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("AccountYear", templateCompanyId, newCompanyId, ex, saved: false);
                    }

                    #endregion
                }
            }

            return result;
        }

        public ActionResult CopyVoucherSeriesTypesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            //Always check existing, because standard voucher serie type is added elsewhere 
            List<VoucherSeriesType> existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(newCompanyId, true);

            #endregion

            List<VoucherSeriesType> templateVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(templateCompanyId, true);
            foreach (VoucherSeriesType templateVoucherSeriesType in templateVoucherSeriesTypes)
            {
                #region VoucherSeriesType

                try
                {
                    VoucherSeriesType voucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(i => i.Name == templateVoucherSeriesType.Name && i.VoucherSeriesTypeNr == templateVoucherSeriesType.VoucherSeriesTypeNr && i.Template == templateVoucherSeriesType.Template);
                    if (voucherSeriesType == null)
                    {
                        voucherSeriesType = new VoucherSeriesType()
                        {
                            Name = templateVoucherSeriesType.Name,
                            StartNr = templateVoucherSeriesType.StartNr,
                            VoucherSeriesTypeNr = templateVoucherSeriesType.VoucherSeriesTypeNr,
                            Template = templateVoucherSeriesType.Template,
                        };

                        result = VoucherManager.AddVoucherSeriesType(voucherSeriesType, newCompanyId);
                        if (!result.Success)
                            result = LogCopyError("VoucherSeriesType", "VoucherSeriesTypeId", templateVoucherSeriesType.VoucherSeriesTypeId, templateVoucherSeriesType.VoucherSeriesTypeNr.ToString(), templateVoucherSeriesType.Name, templateCompanyId, newCompanyId, add: true);
                    }
                }
                catch (Exception ex)
                {
                    result = LogCopyError("VoucherSeriesType", "VoucherSeriesTypeId", templateVoucherSeriesType.VoucherSeriesTypeId, templateVoucherSeriesType.VoucherSeriesTypeNr.ToString(), templateVoucherSeriesType.Name, templateCompanyId, newCompanyId, ex);
                }

                #endregion
            }

            return result;
        }

        public ActionResult CopyResidualCodesFromTemplateCompany(int newCompanyId, int templateCompanyId, Dictionary<int, AccountStd> accountStdMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            List<MatchCode> existingMatchCodes = InvoiceManager.GetMatchCodes(newCompanyId, null, false).ToList();
            List<Account> existingAccounts = AccountManager.GetAccounts(newCompanyId);
            #endregion

            List<MatchCode> templateMatchCodes = InvoiceManager.GetMatchCodes(templateCompanyId, null, false).ToList();

            foreach (MatchCode templateMatchCode in templateMatchCodes)
            {
                #region ResidualCode

                try
                {
                    MatchCode matchCode = existingMatchCodes.FirstOrDefault(i => i.Name == templateMatchCode.Name && i.Type == templateMatchCode.Type);
                    var mappedAccount = existingAccounts.FirstOrDefault(f => f.AccountNr == templateMatchCode.Account.AccountNr);
                    if (mappedAccount != null)
                    {

                        var mappedVatAccount = templateMatchCode.VatAccount != null ? existingAccounts.FirstOrDefault(f => f.AccountNr == templateMatchCode.VatAccount.AccountNr) : null;
                        if (matchCode == null)
                        {
                            matchCode = new MatchCode()
                            {
                                MatchCodeId = 0,
                                Name = templateMatchCode.Name,
                                Description = templateMatchCode.Description,
                                Type = templateMatchCode.Type,
                                AccountId = mappedAccount.AccountId,
                                AccountNr = mappedAccount.AccountNr,
                                VatAccountId = mappedVatAccount != null ? mappedVatAccount.AccountId : (int?)null,
                                VatAccountNr = mappedVatAccount != null ? mappedVatAccount.AccountNr : string.Empty,
                                State = templateMatchCode.State,
                                ActorCompanyId = newCompanyId,
                            };

                            result = InvoiceManager.AddMatchCode(matchCode);
                            if (!result.Success)
                                result = LogCopyError("MatchCode", "MatchCodeId", matchCode.MatchCodeId, templateMatchCode.Name, templateMatchCode.Name, templateCompanyId, newCompanyId, add: true);
                        }
                        else
                        {
                            matchCode.Name = templateMatchCode.Name;
                            matchCode.Description = templateMatchCode.Description;
                            matchCode.Type = templateMatchCode.Type;
                            matchCode.AccountId = mappedAccount.AccountId;
                            matchCode.AccountNr = mappedAccount.AccountNr;
                            matchCode.VatAccountId = mappedVatAccount != null ? mappedVatAccount.AccountId : (int?)null;
                            matchCode.VatAccountNr = mappedVatAccount != null ? mappedVatAccount.AccountNr : string.Empty;
                            matchCode.State = templateMatchCode.State;
                            matchCode.ActorCompanyId = newCompanyId;
                            result = InvoiceManager.UpdateMatchCode(matchCode);
                            if (!result.Success)
                                result = LogCopyError("MatchCode", "MatchCodeId", matchCode.MatchCodeId, templateMatchCode.Name, templateMatchCode.Name, templateCompanyId, newCompanyId, add: false, update: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = LogCopyError("MatchCode", "MatchCodeId", templateMatchCode.MatchCodeId, templateMatchCode.Name, templateMatchCode.Name, templateCompanyId, newCompanyId, ex);
                }

                #endregion
            }

            return result;
        }

        public ActionResult CopyVoucherTemplatesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Always check existing, because standard voucher serie type is added elsewhere 
                List<VoucherHead> existingVoucherTemplates = VoucherManager.GetVoucherTemplates(newCompanyId);
                List<VoucherSeriesType> existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(newCompanyId, true);
                List<VoucherSeries> existingVoucherSeries = VoucherManager.GetVoucherSeries(newCompanyId, false);
                List<AccountYear> existingAccountYears = AccountManager.GetAccountYears(entities, newCompanyId, false, true);
                List<AccountPeriod> existingAccountPeriods = AccountManager.GetAccountPeriods(newCompanyId);

                List<AccountDistributionHead> existingAccountDistributionHeads = AccountDistributionManager.GetAccountDistributionHeads(newCompanyId);
                List<Account> existingAccounts = AccountManager.GetAccounts(newCompanyId);

                #endregion

                List<VoucherHead> templateVoucherTemplates = VoucherManager.GetVoucherTemplates(templateCompanyId);
                foreach (VoucherHead templateVoucherTemplate in templateVoucherTemplates)
                {
                    #region VoucherTemplates

                    try
                    {
                        VoucherSeriesType existingVoucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(i => i.Name == templateVoucherTemplate.VoucherSeriesTypeName && i.VoucherSeriesTypeNr == templateVoucherTemplate.VoucherSeriesTypeNr);
                        AccountYear existingAccountYear = existingAccountYears.FirstOrDefault(a => a.From == templateVoucherTemplate.VoucherSeries.AccountYear.From && a.To == templateVoucherTemplate.VoucherSeries.AccountYear.To && a.Status == templateVoucherTemplate.VoucherSeries.AccountYear.Status);

                        if (existingVoucherSeriesType != null && existingAccountYear != null)
                        {
                            var accountPeriod = existingAccountYear.AccountPeriod.FirstOrDefault(x => x.PeriodNr == templateVoucherTemplate.AccountPeriod.PeriodNr && x.From == templateVoucherTemplate.AccountPeriod.From && x.To == templateVoucherTemplate.AccountPeriod.To);
                            if (accountPeriod != null)
                            {
                                VoucherSeries selectedVoucherSerie = existingVoucherSeries.OrderByDescending(o => o.VoucherSeriesId).FirstOrDefault(x => x.AccountYearId == existingAccountYear.AccountYearId && x.VoucherSeriesTypeId == existingVoucherSeriesType.VoucherSeriesTypeId);

                                if (selectedVoucherSerie == null)
                                {
                                    selectedVoucherSerie = new VoucherSeries();
                                    selectedVoucherSerie.VoucherSeriesTypeId = existingVoucherSeriesType.VoucherSeriesTypeId;
                                    selectedVoucherSerie.AccountYearId = existingAccountYear.AccountYearId;
                                    selectedVoucherSerie.VoucherNrLatest = 0;
                                    result = VoucherManager.AddVoucherSeries(entities, selectedVoucherSerie, newCompanyId, existingAccountYear.AccountYearId, existingVoucherSeriesType.VoucherSeriesTypeId);
                                    if (!result.Success)
                                        result = LogCopyError("VoucherSeries", "VoucherSeriesId", selectedVoucherSerie.VoucherSeriesId, "", "", templateCompanyId, newCompanyId, add: true);
                                }
                                if (selectedVoucherSerie.VoucherSeriesId > 0)
                                {
                                    VoucherHead voucherTemplate = existingVoucherTemplates.FirstOrDefault(i => i.VoucherNr == templateVoucherTemplate.VoucherNr && i.VoucherSeriesTypeNr == templateVoucherTemplate.VoucherSeriesTypeNr && i.AccountPeriod.PeriodNr == templateVoucherTemplate.AccountPeriod.PeriodNr);

                                    if (voucherTemplate == null)
                                    {
                                        voucherTemplate = new VoucherHead()
                                        {
                                            VoucherHeadId = 0,
                                            AccountPeriodId = accountPeriod.AccountPeriodId,
                                            VoucherSeriesId = selectedVoucherSerie.VoucherSeriesId,
                                            VoucherNr = templateVoucherTemplate.VoucherNr,
                                            Date = templateVoucherTemplate.Date,
                                            Text = templateVoucherTemplate.Text,
                                            Status = templateVoucherTemplate.Status,
                                            TypeBalance = templateVoucherTemplate.TypeBalance,
                                            VatVoucher = templateVoucherTemplate.VatVoucher,
                                            Note = templateVoucherTemplate.Note,
                                            CompanyGroupVoucher = templateVoucherTemplate.CompanyGroupVoucher,
                                            SourceType = templateVoucherTemplate.SourceType,
                                            Template = true,
                                            ActorCompanyId = newCompanyId,
                                        };

                                        result = VoucherManager.AddVoucherTemplate(voucherTemplate, newCompanyId);
                                        if (!result.Success)
                                            result = LogCopyError("VoucherHead", "VoucherTemplateId", voucherTemplate.VoucherHeadId, voucherTemplate.Text.ToString(), voucherTemplate.Text, templateCompanyId, newCompanyId, add: true);
                                    }

                                    foreach (VoucherRow templateVoucherRow in templateVoucherTemplate.VoucherRow)
                                    {
                                        var existingAccount = existingAccounts.FirstOrDefault(x => x.Name == templateVoucherRow.AccountName && x.AccountNr == templateVoucherRow.AccountNr);
                                        var existingAccountDistributionHead = existingAccountDistributionHeads.FirstOrDefault(x => x.VoucherSeriesTypeId == selectedVoucherSerie.VoucherSeriesId && x.Type == templateVoucherRow.AccountDistributionHead.Type && x.Name == templateVoucherRow.AccountDistributionHead.Name);

                                        if (existingAccount != null)
                                        {
                                            VoucherRow row = voucherTemplate.VoucherRow.FirstOrDefault(x => x.RowNr == templateVoucherRow.RowNr && x.Text == templateVoucherRow.Text && x.AccountNr == templateVoucherRow.AccountNr);
                                            if (row == null)
                                            {
                                                row = new VoucherRow();
                                                row.Text = templateVoucherRow.Text;
                                                row.Amount = templateVoucherRow.Amount;
                                                row.AccountId = existingAccount.AccountId;
                                                row.Quantity = templateVoucherRow.Quantity;
                                                row.Date = templateVoucherRow.Date;
                                                row.Merged = templateVoucherRow.Merged;
                                                row.State = templateVoucherRow.State;
                                                row.AccountDistributionHeadId = existingAccountDistributionHead != null ? existingAccountDistributionHead.AccountDistributionHeadId : (int?)null;
                                                row.AmountEntCurrency = templateVoucherRow.AmountEntCurrency;
                                                row.RowNr = templateVoucherRow.RowNr;
                                                row.VoucherHeadId = voucherTemplate.VoucherHeadId;
                                                var resultVoucherRow = VoucherManager.AddVoucherTemplateRow(row, newCompanyId);
                                                if (!resultVoucherRow.Success)
                                                    result = LogCopyError("VoucherRow", "VoucherRowId", row.VoucherRowId, row.Text.ToString(), row.Text, templateCompanyId, newCompanyId, add: true);
                                            }
                                            else
                                            {
                                                row.Text = templateVoucherRow.Text;
                                                row.Amount = templateVoucherRow.Amount;
                                                row.AccountId = existingAccount.AccountId;
                                                row.Quantity = templateVoucherRow.Quantity;
                                                row.Date = templateVoucherRow.Date;
                                                row.Merged = templateVoucherRow.Merged;
                                                row.State = templateVoucherRow.State;
                                                row.AccountDistributionHeadId = existingAccountDistributionHead != null ? existingAccountDistributionHead.AccountDistributionHeadId : (int?)null;
                                                row.AmountEntCurrency = templateVoucherRow.AmountEntCurrency;
                                                row.RowNr = templateVoucherRow.RowNr;
                                                var resultVoucherRow = VoucherManager.UpdateVoucherTemplateRow(row);
                                                if (!resultVoucherRow.Success)
                                                    result = LogCopyError("VoucherRow", "VoucherRowId", row.VoucherRowId, row.Text.ToString(), row.Text, templateCompanyId, newCompanyId, add: false, update: true);
                                            }
                                        }
                                    }




                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("VoucherHead", "VoucherTemplateId", templateVoucherTemplate.VoucherHeadId, templateVoucherTemplate.Text.ToString(), templateVoucherTemplate.Text, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }
            }
            return result;
        }
        public ActionResult CopyPaymentMethodsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                List<PaymentMethod> existingPaymentMethods;
                List<PaymentMethod> templatePaymentMethods;

                #region SupplierPaymentMethods

                #region Prereq

                //Get Company
                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Always check existing, because standard voucher serie type is added elsewhere 
                existingPaymentMethods = PaymentManager.GetPaymentMethods(entities, SoeOriginType.SupplierPayment, newCompanyId).ToList();
                templatePaymentMethods = PaymentManager.GetPaymentMethods(entities, SoeOriginType.SupplierPayment, templateCompanyId).ToList();

                #endregion

                foreach (PaymentMethod templatePaymentMethod in templatePaymentMethods)
                {
                    try
                    {
                        PaymentMethod existingPaymentMethod = existingPaymentMethods.FirstOrDefault(p => p.SysPaymentMethodId == templatePaymentMethod.SysPaymentMethodId && p.PaymentType == templatePaymentMethod.PaymentType && p.Name == templatePaymentMethod.Name);

                        if (existingPaymentMethod == null)
                        {
                            if (!templatePaymentMethod.AccountStdReference.IsLoaded)
                                templatePaymentMethod.AccountStdReference.Load();

                            if (templatePaymentMethod.AccountStd != null && !templatePaymentMethod.AccountStd.AccountReference.IsLoaded)
                                templatePaymentMethod.AccountStd.AccountReference.Load();

                            if (!templatePaymentMethod.PaymentInformationRowReference.IsLoaded)
                                templatePaymentMethod.PaymentInformationRowReference.Load();

                            existingPaymentMethod = new PaymentMethod()
                            {
                                Company = newCompany,
                                AccountStd = templatePaymentMethod.AccountStd,
                                PaymentInformationRow = templatePaymentMethod.PaymentInformationRow,
                                SysPaymentMethodId = templatePaymentMethod.SysPaymentMethodId,
                                PaymentType = templatePaymentMethod.PaymentType,
                                Name = templatePaymentMethod.Name,
                                CustomerNr = templatePaymentMethod.CustomerNr,
                                State = (int)SoeEntityState.Active,
                            };

                            result = PaymentManager.AddPaymentMethod(existingPaymentMethod, templatePaymentMethod.PaymentInformationRow?.PaymentInformationRowId ?? 0, templatePaymentMethod.AccountStd.Account.AccountNr, newCompanyId, SoeOriginType.SupplierPayment, entities);

                            if (!result.Success)
                                result = LogCopyError("PaymentMethod", templateCompanyId, newCompanyId, saved: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("PaymentMethod", templateCompanyId, newCompanyId, ex, saved: false);
                    }

                }

                #endregion

                #region CustomerPaymentMethods

                #region Prereq

                //Always check existing, because standard voucher serie type is added elsewhere 
                existingPaymentMethods = PaymentManager.GetPaymentMethods(entities, SoeOriginType.CustomerPayment, newCompanyId).ToList();
                templatePaymentMethods = PaymentManager.GetPaymentMethods(entities, SoeOriginType.CustomerPayment, templateCompanyId).ToList();

                #endregion

                foreach (PaymentMethod templatePaymentMethod in templatePaymentMethods)
                {
                    try
                    {
                        PaymentMethod existingPaymentMethod = existingPaymentMethods.FirstOrDefault(p => p.SysPaymentMethodId == templatePaymentMethod.SysPaymentMethodId && p.PaymentType == templatePaymentMethod.PaymentType && p.Name == templatePaymentMethod.Name);
                        if (existingPaymentMethod == null)
                        {
                            if (!templatePaymentMethod.AccountStdReference.IsLoaded)
                                templatePaymentMethod.AccountStdReference.Load();

                            if (templatePaymentMethod.AccountStd != null && !templatePaymentMethod.AccountStd.AccountReference.IsLoaded)
                                templatePaymentMethod.AccountStd.AccountReference.Load();

                            if (!templatePaymentMethod.PaymentInformationRowReference.IsLoaded)
                                templatePaymentMethod.PaymentInformationRowReference.Load();

                            existingPaymentMethod = new PaymentMethod()
                            {
                                Company = newCompany,
                                AccountStd = templatePaymentMethod.AccountStd,
                                PaymentInformationRow = templatePaymentMethod.PaymentInformationRow,
                                SysPaymentMethodId = templatePaymentMethod.SysPaymentMethodId,
                                PaymentType = templatePaymentMethod.PaymentType,
                                Name = templatePaymentMethod.Name,
                                CustomerNr = templatePaymentMethod.CustomerNr,
                                State = (int)SoeEntityState.Active,
                            };

                            result = PaymentManager.AddPaymentMethod(existingPaymentMethod, templatePaymentMethod.PaymentInformationRow?.PaymentInformationRowId ?? 0, templatePaymentMethod.AccountStd.Account.AccountNr, newCompanyId, SoeOriginType.CustomerPayment, entities);

                            if (!result.Success)
                                result = LogCopyError("PaymentMethod", templateCompanyId, newCompanyId, saved: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("PaymentMethod", templateCompanyId, newCompanyId, ex, saved: false);
                    }

                }

                #endregion
            }

            return result;
        }

        public ActionResult CopyPaymentConditionsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool copyBillingSettings, bool copyCustomerSettings, bool copySupplierSettings)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            Dictionary<int, int> intValues = new Dictionary<int, int>();
            bool copyError = false;

            #region Prereq

            int defaultCustomerPaymentCondition = -1;
            int defaultCustomerPaymentConditionClamAndInterest = -1;
            if (copyBillingSettings || copyCustomerSettings)
            {
                defaultCustomerPaymentCondition = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, 0, templateCompanyId, 0);
                defaultCustomerPaymentConditionClamAndInterest = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest, 0, templateCompanyId, 0);
            }

            int defaultCustomerPaymentConditionHouseholdDeduction = -1;
            if (copyBillingSettings || copyCustomerSettings)
                defaultCustomerPaymentConditionHouseholdDeduction = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction, 0, templateCompanyId, 0);

            int defaultSupplierPaymentCondition = -1;
            if (copySupplierSettings)
                defaultSupplierPaymentCondition = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierPaymentDefaultPaymentCondition, 0, templateCompanyId, 0);

            List<PaymentCondition> existingPaymentConditions = update ? PaymentManager.GetPaymentConditions(newCompanyId) : new List<PaymentCondition>();
            List<PaymentCondition> templatePaymentConditions = PaymentManager.GetPaymentConditions(templateCompanyId);

            #endregion

            foreach (PaymentCondition templatePaymentCondition in templatePaymentConditions)
            {
                #region PaymentCondition

                try
                {
                    PaymentCondition existingPaymentCondition = existingPaymentConditions.FirstOrDefault(i => i.Code == templatePaymentCondition.Code && i.Name == templatePaymentCondition.Name);
                    if (existingPaymentCondition == null)
                    {
                        existingPaymentCondition = new PaymentCondition()
                        {
                            Code = templatePaymentCondition.Code,
                            Name = templatePaymentCondition.Name,
                            Days = templatePaymentCondition.Days,
                        };

                        result = PaymentManager.AddPaymentCondition(existingPaymentCondition, newCompanyId);
                        if (!result.Success)
                        {
                            result = LogCopyError("PaymentCondition", "PaymentConditionId", templatePaymentCondition.PaymentConditionId, templatePaymentCondition.Code, templatePaymentCondition.Name, templateCompanyId, newCompanyId, add: true);
                            copyError = true;
                        }
                    }

                    if (templatePaymentCondition.PaymentConditionId == defaultCustomerPaymentCondition)
                        intValues.Add((int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, existingPaymentCondition.PaymentConditionId);
                    if (templatePaymentCondition.PaymentConditionId == defaultCustomerPaymentConditionClamAndInterest)
                        intValues.Add((int)CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest, existingPaymentCondition.PaymentConditionId);
                    if (templatePaymentCondition.PaymentConditionId == defaultCustomerPaymentConditionHouseholdDeduction)
                        intValues.Add((int)CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction, existingPaymentCondition.PaymentConditionId);
                    if (templatePaymentCondition.PaymentConditionId == defaultSupplierPaymentCondition)
                        intValues.Add((int)CompanySettingType.SupplierPaymentDefaultPaymentCondition, existingPaymentCondition.PaymentConditionId);
                }
                catch (Exception ex)
                {
                    result = LogCopyError("PaymentCondition", "PaymentConditionId", templatePaymentCondition.PaymentConditionId, templatePaymentCondition.Code, templatePaymentCondition.Name, templateCompanyId, newCompanyId, ex);
                    copyError = true;
                }

                #endregion
            }

            #region Settings

            if (!SettingManager.UpdateInsertIntSettings(SettingMainType.Company, intValues, 0, newCompanyId, 0).Success)
                copyError = true;

            #endregion

            if (copyError)
                result = new ActionResult(false);
            else
                result = new ActionResult(true);

            return result;
        }

        public ActionResult CopyGrossProfitCodesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            bool copyError = false;

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                // Get open account year
                AccountYear currentAccountYear = AccountManager.GetCurrentAccountYear(entities, newCompanyId, false);
                if (currentAccountYear == null)
                    currentAccountYear = AccountManager.GetFirstAccountYear(entities, newCompanyId);
                if (currentAccountYear == null)
                    return new ActionResult(true);

                //Get account dims
                List<AccountDim> accountDimsTemplate = AccountManager.GetAccountDimsByCompany(entities, templateCompanyId, loadAccounts: true);
                List<AccountDim> accountDimsNew = AccountManager.GetAccountDimsByCompany(entities, newCompanyId, loadAccounts: true);
                if (accountDimsNew.IsNullOrEmpty())
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                //Map account dims and accounts
                List<Tuple<int, int, Dictionary<int, int>>> mappings = this.GetAccountDimMappingsWithAccounts(accountDimsTemplate, accountDimsNew);

                //Get gross profit codes
                List<GrossProfitCode> templateGrossProfitCodes = GrossProfitManager.GetGrossProfitCodes(entities, templateCompanyId);
                List<GrossProfitCode> existingGrossProfitCodes = update ? GrossProfitManager.GetGrossProfitCodes(entities, newCompanyId) : new List<GrossProfitCode>();

                #endregion

                foreach (var templateGrossProfitCode in templateGrossProfitCodes)
                {
                    #region GrossProfitCode

                    try
                    {
                        GrossProfitCode existingGrossProfitCode = existingGrossProfitCodes.FirstOrDefault(g => g.Code == templateGrossProfitCode.Code && g.Name == templateGrossProfitCode.Name);
                        if (existingGrossProfitCode == null)
                        {
                            int? accountDimId = null;
                            int? accountId = null;
                            if (templateGrossProfitCode.AccountDimId.HasValue)
                            {
                                var dim = mappings.FirstOrDefault(m => m.Item1 == templateGrossProfitCode.AccountDimId.Value);
                                if (dim != null)
                                {
                                    accountDimId = dim.Item2;
                                    if (templateGrossProfitCode.AccountId.HasValue)
                                    {
                                        var accounts = dim.Item3.Where(a => a.Key == templateGrossProfitCode.AccountId.Value);
                                        if (!accounts.IsNullOrEmpty())
                                            accountId = accounts.FirstOrDefault().Value;
                                    }
                                }
                            }

                            existingGrossProfitCode = new GrossProfitCode()
                            {
                                ActorCompanyId = newCompanyId,
                                AccountYearId = currentAccountYear.AccountYearId,
                                AccountDimId = accountDimId,
                                AccountId = accountId,
                                Code = templateGrossProfitCode.Code,
                                Name = templateGrossProfitCode.Name,
                                Description = templateGrossProfitCode.Description,
                                OpeningBalance = templateGrossProfitCode.OpeningBalance,
                                Period1 = templateGrossProfitCode.Period1,
                                Period2 = templateGrossProfitCode.Period2,
                                Period3 = templateGrossProfitCode.Period3,
                                Period4 = templateGrossProfitCode.Period4,
                                Period5 = templateGrossProfitCode.Period5,
                                Period6 = templateGrossProfitCode.Period6,
                                Period7 = templateGrossProfitCode.Period7,
                                Period8 = templateGrossProfitCode.Period8,
                                Period9 = templateGrossProfitCode.Period9,
                                Period10 = templateGrossProfitCode.Period10,
                                Period11 = templateGrossProfitCode.Period11,
                                Period12 = templateGrossProfitCode.Period12,
                                State = (int)SoeEntityState.Active,
                            };

                            SetCreatedProperties(existingGrossProfitCode);
                            entities.GrossProfitCode.AddObject(existingGrossProfitCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("GrossProfitCode", "GrossProfitCodeId", templateGrossProfitCode.GrossProfitCodeId, templateGrossProfitCode.Code.ToString(), templateGrossProfitCode.Name, templateCompanyId, newCompanyId, ex);
                        copyError = true;
                    }

                    #endregion
                }

                //Save codes
                result = SaveChangesWithTransaction(entities);
            }

            #region Settings

            /*if (!SettingManager.UpdateInsertIntSettings(SettingMainType.Company, intValues, 0, newCompanyId).Success)
                copyError = true;*/

            #endregion

            if (copyError)
                result = new ActionResult(false);
            else
                result = new ActionResult(true);

            return result;
        }

        public ActionResult CopyInventoryFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            bool copyError = false;

            try
            {

                #region Prereq

                // Voucher series types
                var existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(newCompanyId, false);
                var templateVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(templateCompanyId, false);

                // Write off methods
                var existingWriteOffMethods = InventoryManager.GetInventoryWriteOffMethods(newCompanyId);
                var templateWriteOffMethods = InventoryManager.GetInventoryWriteOffMethods(templateCompanyId);

                #endregion

                #region Copy

                // Settings 
                List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
                {
                    CompanySettingType.InventoryEditTriggerAccounts,
                };

                CopyCompanySettings(CompanySettingTypeGroup.Inventory, newCompanyId, templateCompanyId, excludeSettingTypes);

                // Base accounts
                CopyCompanyAccountSettings(CompanySettingTypeGroup.BaseAccountsInventory, newCompanyId, templateCompanyId);

                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    // Write off templates
                    var existingWriteOffTemplates = InventoryManager.GetInventoryWriteOffTemplates(entities, newCompanyId).ToList();
                    var templateWriteOffTemplates = InventoryManager.GetInventoryWriteOffTemplates(entities, templateCompanyId).ToList();

                    //Get account dims
                    List<AccountDim> accountDimsTemplate = AccountManager.GetAccountDimsByCompany(entities, templateCompanyId, loadAccounts: true);
                    List<AccountDim> accountDimsNew = AccountManager.GetAccountDimsByCompany(entities, newCompanyId, loadAccounts: true);
                    if (accountDimsNew.IsNullOrEmpty())
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                    //Map account dims and accounts
                    List<Tuple<int, int, Dictionary<int, int>>> mappings = this.GetAccountDimMappingsWithAccounts(accountDimsTemplate, accountDimsNew);

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        // Write off methods
                        foreach (var method in templateWriteOffMethods)
                        {
                            var existing = existingWriteOffMethods.FirstOrDefault(m => m.Name == method.Name && m.Type == method.Type);
                            if (existing == null)
                            {
                                existing = new InventoryWriteOffMethod()
                                {
                                    ActorCompanyId = newCompanyId,
                                    Name = method.Name,
                                    Description = method.Description,
                                    Type = method.Type,
                                    PeriodType = method.PeriodType,
                                    PeriodValue = method.PeriodValue,
                                    YearPercent = method.YearPercent
                                };

                                SetCreatedProperties(existing);

                                entities.InventoryWriteOffMethod.AddObject(existing);

                                existingWriteOffMethods.Add(existing);
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                        {
                            transaction.Dispose();
                            return result;
                        }

                        // Write of templates
                        foreach (var template in templateWriteOffTemplates)
                        {
                            var existing = existingWriteOffTemplates.FirstOrDefault(m => m.Name == template.Name);
                            if (existing == null)
                            {
                                var templateMethod = templateWriteOffMethods.FirstOrDefault(m => m.InventoryWriteOffMethodId == template.InventoryWriteOffMethodId);
                                if (templateMethod == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "InventoryWriteOffMethod");

                                var templateSerie = templateVoucherSeriesTypes.FirstOrDefault(s => s.VoucherSeriesTypeId == template.VoucherSeriesTypeId);
                                if (templateSerie == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, "VoucherSeriesType");

                                var newMethod = existingWriteOffMethods.FirstOrDefault(m => m.Name == templateMethod.Name && m.Type == templateMethod.Type);
                                var newSerie = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == templateSerie.Name && s.VoucherSeriesTypeNr == templateSerie.VoucherSeriesTypeNr);

                                existing = new InventoryWriteOffTemplate()
                                {
                                    Name = template.Name,
                                    Description = template.Description,

                                    //Set references
                                    ActorCompanyId = newCompanyId,

                                    //Set FK
                                    InventoryWriteOffMethodId = newMethod.InventoryWriteOffMethodId,
                                    VoucherSeriesTypeId = newSerie.VoucherSeriesTypeId,
                                };

                                if (!template.InventoryAccountStd.IsLoaded)
                                    template.InventoryAccountStd.Load();

                                foreach (var inventoryAccountStd in template.InventoryAccountStd)
                                {
                                    if (!inventoryAccountStd.AccountStdReference.IsLoaded)
                                        inventoryAccountStd.AccountStdReference.Load();

                                    if (inventoryAccountStd.AccountId.HasValue 
                                        && !inventoryAccountStd.AccountStd.AccountReference.IsLoaded)
                                    {
                                        inventoryAccountStd.AccountStd.AccountReference.Load();
                                    }

                                    var mapping = mappings.FirstOrDefault(m => inventoryAccountStd.AccountId.HasValue 
                                        && m.Item1 == inventoryAccountStd.AccountStd.Account.AccountDimId);
                                    if (mapping != null)
                                    {
                                        var accounts = mapping.Item3.Where(a => a.Key == inventoryAccountStd.AccountStd.AccountId);
                                        if (!accounts.IsNullOrEmpty())
                                        {
                                            // Std
                                            var newInventoryAccountStd = new InventoryAccountStd()
                                            {
                                                AccountId = accounts.FirstOrDefault().Value,
                                                Type = inventoryAccountStd.Type,
                                                InventoryWriteOffTemplate = existing,
                                            };

                                            if (!inventoryAccountStd.AccountInternal.IsLoaded)
                                                inventoryAccountStd.AccountInternal.Load();

                                            // Internal
                                            foreach (var internalAcc in inventoryAccountStd.AccountInternal)
                                            {
                                                if (!internalAcc.AccountReference.IsLoaded)
                                                    internalAcc.AccountReference.Load();

                                                var internalMapping = mappings.FirstOrDefault(m => m.Item1 == internalAcc.Account.AccountDimId);
                                                if (internalMapping != null)
                                                {
                                                    var internalAccounts = internalMapping.Item3.Where(a => a.Key == internalAcc.AccountId);
                                                    if (!internalAccounts.IsNullOrEmpty())
                                                    {
                                                        var newInternalAccount = AccountManager.GetAccountInternal(entities, internalAccounts.FirstOrDefault().Value, newCompanyId);
                                                        if (newInternalAccount != null)
                                                            newInventoryAccountStd.AccountInternal.Add(newInternalAccount);
                                                    }
                                                }
                                            }

                                            SetCreatedProperties(newInventoryAccountStd);
                                            entities.InventoryAccountStd.AddObject(newInventoryAccountStd);
                                        }
                                    }
                                }

                                existingWriteOffTemplates.Add(existing);

                                SetCreatedProperties(existing);
                                entities.InventoryWriteOffTemplate.AddObject(existing);
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                        {
                            transaction.Dispose();
                            return result;
                        }

                        // template trigger accounts
                        var templateInventoryTriggerAccounts = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.InventoryEditTriggerAccounts, 0, templateCompanyId, 0);

                        if (!String.IsNullOrEmpty(templateInventoryTriggerAccounts))
                        {
                            string[] records = templateInventoryTriggerAccounts.Split(',');
                            if (records.Length > 0)
                            {
                                var templateAccountStds = AccountManager.GetAccountStdsByCompany(entities, templateCompanyId, null);

                                StringBuilder settingStr = new StringBuilder();
                                foreach (var record in records)
                                {
                                    string[] valuePair = record.Split(':');

                                    Int32.TryParse(valuePair[0], out int templateAccountId);
                                    if (templateAccountId == 0)
                                        continue;

                                    Int32.TryParse(valuePair[1], out int templateWriteOffTemplateId);
                                    if (templateWriteOffTemplateId == 0)
                                        continue;

                                    var accountStd = templateAccountStds.FirstOrDefault(a => a.AccountId == templateAccountId);
                                    var templateWriteOffTemplate = templateWriteOffTemplates.FirstOrDefault(t => t.InventoryWriteOffTemplateId == templateWriteOffTemplateId);
                                    if (accountStd != null && templateWriteOffTemplate != null)
                                    {
                                        var existingWriteOffTemplate = existingWriteOffTemplates.FirstOrDefault(t => t.Name == templateWriteOffTemplate.Name);
                                        if (existingWriteOffTemplate != null)
                                        {
                                            var mapping = mappings.FirstOrDefault(m => m.Item1 == accountStd.Account.AccountDimId);
                                            if (mapping != null)
                                            {
                                                var accounts = mapping.Item3.Where(a => a.Key == accountStd.AccountId);
                                                if (!accounts.IsNullOrEmpty())
                                                {
                                                    if (settingStr.Length == 0)
                                                        settingStr.Append(accounts.FirstOrDefault().Value.ToString() + ":" + existingWriteOffTemplate.InventoryWriteOffTemplateId.ToString());
                                                    else
                                                        settingStr.Append("," + accounts.FirstOrDefault().Value.ToString() + ":" + existingWriteOffTemplate.InventoryWriteOffTemplateId.ToString());
                                                }
                                            }
                                        }
                                    }
                                }

                                result = SettingManager.UpdateInsertStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.InventoryEditTriggerAccounts, settingStr.ToString(), 0, newCompanyId, 0);
                                if (!result.Success)
                                {
                                    transaction.Dispose();
                                    return result;
                                }
                            }
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }
            finally
            {
                if (copyError)
                    result = new ActionResult(false);
                else
                    result = new ActionResult(true);
            }

            return result;
        }

        public ActionResult CopyAccountDistributionTemplatesFromTemplateCompany(SoeAccountDistributionType distributionType, int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            bool copyError = false;

            try
            {

                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    #region Prereq

                    var newCompany = CompanyManager.GetCompany(entities, newCompanyId);

                    var templateAccountDistributionHeads = AccountDistributionManager.GetAccountDistributionHeads(entities, templateCompanyId, distributionType, true, true, false, false, true);
                    var existingAccountDistributionHeads = AccountDistributionManager.GetAccountDistributionHeads(entities, newCompanyId, distributionType, true, true, false, false, true);

                    var newAccountStds = AccountManager.GetAccountStdsByCompany(entities, newCompanyId, true);

                    var templateAccountInternals = AccountManager.GetAccountInternals(entities, templateCompanyId, true, true);
                    var newAccountInternals = AccountManager.GetAccountInternals(entities, newCompanyId, true, true);

                    var templateVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(entities, templateCompanyId, false);
                    var existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(entities, newCompanyId, false);

                    List<AccountDim> accountDimsTemplate = AccountManager.GetAccountDimsByCompany(entities, templateCompanyId, loadAccounts: true);
                    List<AccountDim> accountDimsNew = AccountManager.GetAccountDimsByCompany(entities, newCompanyId, loadAccounts: true);
                    if (accountDimsNew.IsNullOrEmpty())
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                    // Get full account dim mapping
                    List<Tuple<int, int, Dictionary<int, int>>> mappings = this.GetAccountDimMappingsWithAccounts(accountDimsTemplate, accountDimsNew);

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var templateAccDistrHead in templateAccountDistributionHeads)
                        {
                            var addDistribution = false;
                            var newAccountDistributionHead = existingAccountDistributionHeads.FirstOrDefault(h => h.Name == templateAccDistrHead.Name);
                            if (newAccountDistributionHead == null)
                            {
                                #region new
                                addDistribution = true;

                                newAccountDistributionHead = new AccountDistributionHead()
                                {
                                    Type = templateAccDistrHead.Type,
                                    Name = templateAccDistrHead.Name,
                                    Description = templateAccDistrHead.Description,
                                    TriggerType = templateAccDistrHead.TriggerType,
                                    CalculationType = templateAccDistrHead.CalculationType,
                                    Calculate = templateAccDistrHead.Calculate,
                                    PeriodType = templateAccDistrHead.PeriodType,
                                    PeriodValue = templateAccDistrHead.PeriodValue,
                                    Sort = templateAccDistrHead.Sort,
                                    StartDate = templateAccDistrHead.StartDate,
                                    EndDate = templateAccDistrHead.EndDate,
                                    DayNumber = templateAccDistrHead.DayNumber,
                                    Amount = templateAccDistrHead.Amount,
                                    AmountOperator = templateAccDistrHead.AmountOperator,
                                    KeepRow = templateAccDistrHead.KeepRow,
                                    UseInVoucher = templateAccDistrHead.UseInVoucher,
                                    UseInSupplierInvoice = templateAccDistrHead.UseInSupplierInvoice,
                                    UseInCustomerInvoice = templateAccDistrHead.UseInCustomerInvoice,
                                    UseInImport = templateAccDistrHead.UseInImport,
                                    State = (int)SoeEntityState.Active,
                                    UseInPayrollVoucher = templateAccDistrHead.UseInPayrollVoucher,
                                    UseInPayrollVacationVoucher = templateAccDistrHead.UseInPayrollVacationVoucher,

                                    // references
                                    Company = newCompany,
                                };

                                if (distributionType == SoeAccountDistributionType.Period && templateAccDistrHead.VoucherSeriesTypeId.HasValue)
                                {
                                    var templateVoucherSeriesType = templateVoucherSeriesTypes.FirstOrDefault(s => s.VoucherSeriesTypeId == templateAccDistrHead.VoucherSeriesTypeId.Value);
                                    if (templateVoucherSeriesType != null)
                                        newAccountDistributionHead.VoucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == templateVoucherSeriesType.Name);
                                }

                                foreach (var accountExpression in templateAccDistrHead.AccountDistributionHeadAccountDimMapping)
                                {
                                    var mapping = mappings.FirstOrDefault(m => m.Item1 == accountExpression.AccountDimId);
                                    if (mapping != null)
                                    {
                                        var newMapping = new AccountDistributionHeadAccountDimMapping()
                                        {
                                            AccountDim = accountDimsNew.FirstOrDefault(d => d.AccountDimId == mapping.Item2),
                                            AccountExpression = accountExpression.AccountExpression,
                                        };

                                        newAccountDistributionHead.AccountDistributionHeadAccountDimMapping.Add(newMapping);
                                    }
                                }

                                SetCreatedProperties(newAccountDistributionHead);

                                #endregion
                            }
                            else
                            {
                                if (!update)
                                    continue;

                                #region update

                                newAccountDistributionHead.Type = templateAccDistrHead.Type;
                                newAccountDistributionHead.Name = templateAccDistrHead.Name;
                                newAccountDistributionHead.Description = templateAccDistrHead.Description;
                                newAccountDistributionHead.TriggerType = templateAccDistrHead.TriggerType;
                                newAccountDistributionHead.CalculationType = templateAccDistrHead.CalculationType;
                                newAccountDistributionHead.Calculate = templateAccDistrHead.Calculate;
                                newAccountDistributionHead.PeriodType = templateAccDistrHead.PeriodType;
                                newAccountDistributionHead.PeriodValue = templateAccDistrHead.PeriodValue;
                                newAccountDistributionHead.Sort = templateAccDistrHead.Sort;
                                newAccountDistributionHead.StartDate = templateAccDistrHead.StartDate;
                                newAccountDistributionHead.EndDate = templateAccDistrHead.EndDate;
                                newAccountDistributionHead.DayNumber = templateAccDistrHead.DayNumber;
                                newAccountDistributionHead.Amount = templateAccDistrHead.Amount;
                                newAccountDistributionHead.AmountOperator = templateAccDistrHead.AmountOperator;
                                newAccountDistributionHead.KeepRow = templateAccDistrHead.KeepRow;
                                newAccountDistributionHead.UseInVoucher = templateAccDistrHead.UseInVoucher;
                                newAccountDistributionHead.UseInSupplierInvoice = templateAccDistrHead.UseInSupplierInvoice;
                                newAccountDistributionHead.UseInCustomerInvoice = templateAccDistrHead.UseInCustomerInvoice;
                                newAccountDistributionHead.UseInImport = templateAccDistrHead.UseInImport;
                                newAccountDistributionHead.UseInPayrollVoucher = templateAccDistrHead.UseInPayrollVoucher;
                                newAccountDistributionHead.UseInPayrollVacationVoucher = templateAccDistrHead.UseInPayrollVacationVoucher;

                                if (distributionType == SoeAccountDistributionType.Period && templateAccDistrHead.VoucherSeriesTypeId.HasValue)
                                {
                                    var templateVoucherSeriesType = templateVoucherSeriesTypes.FirstOrDefault(s => s.VoucherSeriesTypeId == templateAccDistrHead.VoucherSeriesTypeId.Value);
                                    if (templateVoucherSeriesType != null)
                                        newAccountDistributionHead.VoucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == templateVoucherSeriesType.Name);
                                }

                                foreach (var accountExpression in templateAccDistrHead.AccountDistributionHeadAccountDimMapping)
                                {
                                    var mapping = mappings.FirstOrDefault(m => m.Item1 == accountExpression.AccountDimId);
                                    if (mapping != null)
                                    {
                                        var existingExpression = newAccountDistributionHead.AccountDistributionHeadAccountDimMapping.FirstOrDefault(m => m.AccountDimId == mapping.Item2);
                                        if (existingExpression != null)
                                        {
                                            existingExpression.AccountExpression = accountExpression.AccountExpression;
                                        }
                                        else
                                        {
                                            var newMapping = new AccountDistributionHeadAccountDimMapping()
                                            {
                                                AccountDim = accountDimsNew.FirstOrDefault(d => d.AccountDimId == mapping.Item2),
                                                AccountExpression = accountExpression.AccountExpression,
                                            };

                                            newAccountDistributionHead.AccountDistributionHeadAccountDimMapping.Add(newMapping);
                                        }
                                    }
                                }

                                SetModifiedProperties(newAccountDistributionHead);

                                #endregion
                            }

                            foreach (var accountDistributionRow in templateAccDistrHead.AccountDistributionRow)
                            {
                                AccountDistributionRow newAccountDistrRow = null;
                                if (update)
                                    newAccountDistrRow = newAccountDistributionHead.AccountDistributionRow.FirstOrDefault(r => r.RowNbr == accountDistributionRow.RowNbr);

                                if (newAccountDistrRow == null)
                                {
                                    var mapping = mappings.FirstOrDefault();
                                    var accountStdId = accountDistributionRow.AccountId.HasValue && mapping.Item3.ContainsKey(accountDistributionRow.AccountId.Value) ? mapping.Item3[accountDistributionRow.AccountId.Value] : 0;

                                    newAccountDistrRow = new AccountDistributionRow()
                                    {
                                        RowNbr = accountDistributionRow.RowNbr,
                                        CalculateRowNbr = accountDistributionRow.CalculateRowNbr,
                                        SameBalance = accountDistributionRow.SameBalance,
                                        OppositeBalance = accountDistributionRow.OppositeBalance,
                                        Description = accountDistributionRow.Description,

                                        //Set references
                                        AccountStd = accountStdId > 0 ? newAccountStds.FirstOrDefault(a => a.AccountId == accountStdId) : null,
                                    };

                                    newAccountDistributionHead.AccountDistributionRow.Add(newAccountDistrRow);
                                }
                                else
                                {
                                    var mapping = mappings.FirstOrDefault();
                                    var accountStdId = accountDistributionRow.AccountId.HasValue && mapping.Item3.ContainsKey(accountDistributionRow.AccountId.Value) ? mapping.Item3[accountDistributionRow.AccountId.Value] : 0;

                                    newAccountDistrRow.RowNbr = accountDistributionRow.RowNbr;
                                    newAccountDistrRow.CalculateRowNbr = accountDistributionRow.CalculateRowNbr;
                                    newAccountDistrRow.SameBalance = accountDistributionRow.SameBalance;
                                    newAccountDistrRow.OppositeBalance = accountDistributionRow.OppositeBalance;
                                    newAccountDistrRow.Description = accountDistributionRow.Description;

                                    //Set references
                                    newAccountDistrRow.AccountStd = accountStdId > 0 ? newAccountStds.FirstOrDefault(a => a.AccountId == accountStdId) : null;
                                }

                                foreach (var accountDistrRowAccount in accountDistributionRow.AccountDistributionRowAccount)
                                {
                                    var newAccountDistrRowAccount = newAccountDistrRow.AccountDistributionRowAccount.FirstOrDefault(a => a.DimNr == accountDistrRowAccount.DimNr);
                                    if (newAccountDistrRowAccount == null)
                                    {
                                        var templateAccountInternal = templateAccountInternals.FirstOrDefault(a => a.AccountId == accountDistrRowAccount.AccountId);
                                        if (templateAccountInternal != null)
                                        {
                                            var internalMapping = mappings.FirstOrDefault(a => a.Item1 == templateAccountInternal.Account.AccountDim.AccountDimId);
                                            if (internalMapping != null)
                                            {
                                                var accountInternalId = templateAccountInternal.AccountId > 0 && internalMapping.Item3.ContainsKey(templateAccountInternal.AccountId) ? internalMapping.Item3[templateAccountInternal.AccountId] : 0;
                                                newAccountDistrRowAccount = new AccountDistributionRowAccount()
                                                {
                                                    DimNr = 2,
                                                    AccountInternal = accountInternalId != 0 ? newAccountInternals.FirstOrDefault(a => a.AccountId == accountInternalId) : null,
                                                    KeepSourceRowAccount = accountDistrRowAccount.KeepSourceRowAccount
                                                };
                                                newAccountDistrRow.AccountDistributionRowAccount.Add(newAccountDistrRowAccount);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!update)
                                            continue;

                                        var templateAccountInternal = templateAccountInternals.FirstOrDefault(a => a.AccountId == accountDistrRowAccount.AccountId);
                                        if (templateAccountInternal != null)
                                        {
                                            var internalMapping = mappings.FirstOrDefault(a => a.Item1 == templateAccountInternal.Account.AccountDim.AccountDimId);
                                            if (internalMapping != null)
                                            {
                                                var accountInternalId = templateAccountInternal.AccountId > 0 && internalMapping.Item3.ContainsKey(templateAccountInternal.AccountId) ? internalMapping.Item3[templateAccountInternal.AccountId] : 0;

                                                newAccountDistrRowAccount.AccountInternal = accountInternalId != 0 ? newAccountInternals.FirstOrDefault(a => a.AccountId == accountInternalId) : null;
                                                newAccountDistrRowAccount.KeepSourceRowAccount = accountDistrRowAccount.KeepSourceRowAccount;

                                            }
                                        }
                                    }
                                }
                            }

                            // Add AccountDistributionHead to context
                            if (addDistribution)
                                entities.AccountDistributionHead.AddObject(newAccountDistributionHead);
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                        {
                            transaction.Dispose();
                            return result;
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }
            finally
            {
                if (copyError)
                    result = new ActionResult(false);
                else
                    result = new ActionResult(true);
            }

            return result;
        }

        public ActionResult CopyDistributionCodesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Prereq

                        var newCompany = CompanyManager.GetCompany(entities, newCompanyId);

                        var templateDistributionCodes = BudgetManager.GetDistributionCodes(entities, templateCompanyId, true, true);
                        var existingDistributionCodes = BudgetManager.GetDistributionCodes(entities, newCompanyId, true, true);

                        var templateAccountDims = AccountManager.GetAccountDimsByCompany(entities, templateCompanyId, loadAccounts: true);
                        var existingAccountDims = AccountManager.GetAccountDimsByCompany(entities, newCompanyId, loadAccounts: true);

                        var templateOpeningHours = CalendarManager.GetOpeningHoursForCompany(entities, templateCompanyId);
                        var existingOpeningHours = CalendarManager.GetOpeningHoursForCompany(entities, newCompanyId);

                        #endregion

                        #region Perform
                        Dictionary<int, DistributionCodeHead> mappedDistributionCodeHeads = new Dictionary<int, DistributionCodeHead>();

                        foreach (var templateDistributionCode in templateDistributionCodes)
                        {
                            var head = existingDistributionCodes.FirstOrDefault(h => h.Name == templateDistributionCode.Name);
                            if (head != null)
                            {
                                mappedDistributionCodeHeads.Add(templateDistributionCode.DistributionCodeHeadId, head);
                            }
                            else
                            {
                                //New
                                head = new DistributionCodeHead()
                                {
                                    Type = templateDistributionCode.Type,
                                    Name = templateDistributionCode.Name,
                                    NoOfPeriods = templateDistributionCode.NoOfPeriods,
                                    SubType = templateDistributionCode.SubType,
                                    OpeningHoursId = templateDistributionCode.OpeningHoursId,
                                    FromDate = templateDistributionCode.FromDate,

                                    Company = newCompany,
                                };

                                if (templateDistributionCode.AccountDimId.HasValue)
                                {
                                    var templateAccountDim = templateAccountDims.FirstOrDefault(d => d.AccountDimId == templateDistributionCode.AccountDimId);
                                    if (templateAccountDim != null)
                                        head.AccountDim = existingAccountDims.FirstOrDefault(d => d.AccountDimNr == templateAccountDim.AccountDimNr && d.Name == templateAccountDim.Name);
                                }

                                if (templateDistributionCode.OpeningHoursId.HasValue)
                                {
                                    var templateOpeningHour = templateOpeningHours.FirstOrDefault(o => o.OpeningHoursId == templateDistributionCode.OpeningHoursId.Value);
                                    if (templateOpeningHour != null)
                                        head.OpeningHours = existingOpeningHours.FirstOrDefault(o => o.Name == templateDistributionCode.Name);
                                }

                                SetCreatedProperties(head);
                                entities.DistributionCodeHead.AddObject(head);

                                mappedDistributionCodeHeads.Add(templateDistributionCode.DistributionCodeHeadId, head);
                            }
                        }

                        foreach (var templateDistributionCode in templateDistributionCodes)
                        {
                            var head = mappedDistributionCodeHeads[templateDistributionCode.DistributionCodeHeadId];
                            if (head != null)
                            {
                                if (!templateDistributionCode.ParentId.IsNullOrEmpty())
                                {
                                    var parentHead = mappedDistributionCodeHeads[templateDistributionCode.ParentId.Value];
                                    if (parentHead != null)
                                        head.ParentId = parentHead.DistributionCodeHeadId;
                                }

                                foreach (var templateDistributionCodePeriod in templateDistributionCode.DistributionCodePeriod)
                                {
                                    var newPeriod = new DistributionCodePeriod()
                                    {
                                        Percent = templateDistributionCodePeriod.Percent,
                                        Comment = templateDistributionCodePeriod.Comment,

                                        DistributionCodeHead = head,
                                    };

                                    if (!templateDistributionCode.ParentId.IsNullOrEmpty())
                                    {
                                        var parentHead = mappedDistributionCodeHeads[templateDistributionCode.ParentId.Value];
                                        if (parentHead != null)
                                            newPeriod.ParentToDistributionCodeHeadId = parentHead.DistributionCodeHeadId;
                                    }

                                    SetCreatedProperties(newPeriod);
                                    entities.DistributionCodePeriod.AddObject(newPeriod);
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult CopySuppliersFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Prereq

                        var newCompany = CompanyManager.GetCompany(entities, newCompanyId);

                        List<Supplier> templateSuppliers = SupplierManager.GetSuppliers(entities, templateCompanyId, true, true, true, true, true, false);
                        List<Supplier> newSuppliers = SupplierManager.GetSuppliers(entities, newCompanyId, true, true, true, true, true, false);

                        List<Currency> templateCurrencies = CountryCurrencyManager.GetCurrencies(entities, templateCompanyId);
                        List<Currency> newCurrencies = CountryCurrencyManager.GetCurrencies(entities, newCompanyId);

                        List<PaymentCondition> templatePaymentConditions = PaymentManager.GetPaymentConditions(entities, templateCompanyId);
                        List<PaymentCondition> newPaymentConditions = PaymentManager.GetPaymentConditions(entities, newCompanyId);

                        List<VatCode> templateVatCodes = AccountManager.GetVatCodes(entities, templateCompanyId);
                        List<VatCode> newVatCodes = AccountManager.GetVatCodes(entities, newCompanyId);

                        List<DeliveryCondition> templateDeliveryConditions = InvoiceManager.GetDeliveryConditions(entities, templateCompanyId);
                        List<DeliveryCondition> newDeliveryConditions = InvoiceManager.GetDeliveryConditions(entities, newCompanyId);

                        List<DeliveryType> templateDeliveryTypes = InvoiceManager.GetDeliveryTypes(entities, templateCompanyId);
                        List<DeliveryType> newDeliveryTypes = InvoiceManager.GetDeliveryTypes(entities, newCompanyId);

                        List<AttestWorkFlowGroup> templateAttestWorkFlowGroups = AttestManager.GetAttestWorkFlowGroupsSimple(entities, templateCompanyId);
                        List<AttestWorkFlowGroup> newAttestWorkFlowGroups = AttestManager.GetAttestWorkFlowGroupsSimple(entities, newCompanyId);

                        List<CommodityCodeDTO> templateIntrastatCodes = CommodityCodeManager.GetCustomerCommodityCodes(entities, templateCompanyId, true, true);
                        List<CommodityCodeDTO> newIntrastatCodes = CommodityCodeManager.GetCustomerCommodityCodes(entities, newCompanyId, true, true);

                        var newAccountStds = AccountManager.GetAccountStdsByCompany(entities, newCompanyId, true);

                        var templateAccountInternals = AccountManager.GetAccountInternals(entities, templateCompanyId, true, true);
                        var newAccountInternals = AccountManager.GetAccountInternals(entities, newCompanyId, true, true);

                        List<AccountDim> accountDimsTemplate = AccountManager.GetAccountDimsByCompany(entities, templateCompanyId, loadAccounts: true);
                        List<AccountDim> accountDimsNew = AccountManager.GetAccountDimsByCompany(entities, newCompanyId, loadAccounts: true);
                        if (accountDimsNew.IsNullOrEmpty())
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                        // Get full account dim mapping
                        List<Tuple<int, int, Dictionary<int, int>>> mappings = GetAccountDimMappingsWithAccounts(accountDimsTemplate, accountDimsNew);

                        #endregion

                        #region Perform

                        foreach (var templateSupplier in templateSuppliers)
                        {
                            var newSupplier = newSuppliers.FirstOrDefault(s => s.Name == templateSupplier.Name && s.OrgNr == templateSupplier.OrgNr);
                            if (newSupplier == null)
                            {
                                newSupplier = new Supplier
                                {
                                    VatType = (int)templateSupplier.VatType,
                                    CurrencyId = templateSupplier.CurrencyId,
                                    SysCountryId = templateSupplier.SysCountryId,
                                    SysLanguageId = templateSupplier.SysLanguageId,
                                    SupplierNr = templateSupplier.SupplierNr.Trim(),
                                    Name = templateSupplier.Name.Trim(),
                                    OrgNr = templateSupplier.OrgNr,
                                    VatNr = templateSupplier.VatNr,
                                    InvoiceReference = templateSupplier.InvoiceReference,
                                    OurReference = templateSupplier.OurReference,
                                    BIC = templateSupplier.BIC,
                                    OurCustomerNr = templateSupplier.OurCustomerNr,
                                    CopyInvoiceNrToOcr = templateSupplier.CopyInvoiceNrToOcr,
                                    Interim = templateSupplier.Interim,
                                    ManualAccounting = templateSupplier.ManualAccounting,
                                    BlockPayment = templateSupplier.BlockPayment,
                                    RiksbanksCode = templateSupplier.RiksbanksCode,
                                    State = (int)SoeEntityState.Active,
                                    IsEDISupplier = templateSupplier.IsEDISupplier,
                                    ShowNote = templateSupplier.ShowNote,
                                    Note = templateSupplier.Note,
                                    SysWholeSellerId = templateSupplier.SysWholeSellerId.ToNullable(),
                                    IsPrivatePerson = templateSupplier.IsPrivatePerson,
                                };
                                SetCreatedProperties(newSupplier);
                                entities.Supplier.AddObject(newSupplier);

                                #region Actor Add

                                var actor = new Actor()
                                {
                                    ActorType = (int)SoeActorType.Supplier,

                                    //Set references
                                    Supplier = newSupplier,

                                };
                                SetCreatedProperties(newSupplier);
                                entities.Actor.AddObject(actor);

                                //supplierId = newSupplier.ActorSupplierId;

                                #endregion
                            }
                            else
                            {
                                if (!update)
                                    continue;
                            }

                            #region Add references

                            newSupplier.Company = newCompany;

                            // Payment condition
                            if (templateSupplier.PaymentConditionId.HasValue)
                            {
                                var templatePaymentCondition = templatePaymentConditions.FirstOrDefault(p => p.PaymentConditionId == templateSupplier.PaymentConditionId.Value);
                                if (templatePaymentCondition != null)
                                {
                                    var newPaymentCondition = newPaymentConditions.FirstOrDefault(p => p.Code == templatePaymentCondition.Code && p.Name == templatePaymentCondition.Name);
                                    if (newPaymentCondition != null)
                                    {
                                        newSupplier.PaymentCondition = newPaymentCondition;
                                    }
                                    else
                                    {
                                        newPaymentCondition = new PaymentCondition()
                                        {
                                            Code = templatePaymentCondition.Code,
                                            Name = templatePaymentCondition.Name,
                                            Days = templatePaymentCondition.Days,
                                            DiscountDays = templatePaymentCondition.DiscountDays,
                                            DiscountPercent = templatePaymentCondition.DiscountPercent,

                                            Company = newCompany,
                                        };

                                        SetCreatedProperties(newPaymentCondition);
                                        entities.PaymentCondition.AddObject(newPaymentCondition);

                                        newSupplier.PaymentCondition = newPaymentCondition;
                                    }
                                }
                            }

                            // Vat code
                            if (templateSupplier.VatCodeId.HasValue)
                            {
                                var templateVatCode = templateVatCodes.FirstOrDefault(p => p.VatCodeId == templateSupplier.VatCodeId.Value);
                                if (templateVatCode != null)
                                {
                                    var newVatCode = newVatCodes.FirstOrDefault(p => p.Code == templateVatCode.Code && p.Name == templateVatCode.Name);
                                    if (newVatCode != null)
                                    {
                                        newSupplier.VatCode = newVatCode;
                                    }
                                    else
                                    {
                                        var stdMapping = mappings.FirstOrDefault();
                                        if (stdMapping != null)
                                        {
                                            int? purchaseVatAccountId = null;
                                            if (templateVatCode.PurchaseVATAccountId.HasValue)
                                                purchaseVatAccountId = stdMapping.Item3[(int)templateVatCode.PurchaseVATAccountId];

                                            var vatCode = new VatCode()
                                            {
                                                AccountId = stdMapping.Item3[templateVatCode.AccountId],
                                                Code = templateVatCode.Code,
                                                Name = templateVatCode.Name,
                                                Percent = templateVatCode.Percent,
                                                PurchaseVATAccountId = purchaseVatAccountId,

                                                Company = newCompany,
                                            };

                                            SetCreatedProperties(vatCode);
                                            entities.VatCode.AddObject(vatCode);

                                            newSupplier.VatCode = newVatCode;
                                        }
                                    }
                                }
                            }

                            // Delivery condition
                            if (templateSupplier.DeliveryConditionId.HasValue)
                            {
                                var templateDeliveryCondition = templateDeliveryConditions.FirstOrDefault(p => p.DeliveryConditionId == templateSupplier.DeliveryConditionId.Value);
                                if (templateDeliveryCondition != null)
                                {
                                    var newDeliveryCondition = newDeliveryConditions.FirstOrDefault(p => p.Code == templateDeliveryCondition.Code && p.Name == templateDeliveryCondition.Name);
                                    if (newDeliveryCondition != null)
                                    {
                                        newSupplier.DeliveryCondition = newDeliveryCondition;
                                    }
                                    else
                                    {
                                        newDeliveryCondition = new DeliveryCondition()
                                        {
                                            Code = templateDeliveryCondition.Code,
                                            Name = templateDeliveryCondition.Name,

                                            Company = newCompany,
                                        };

                                        SetCreatedProperties(newDeliveryCondition);
                                        entities.DeliveryCondition.AddObject(newDeliveryCondition);

                                        newSupplier.DeliveryCondition = newDeliveryCondition;
                                    }
                                }
                            }

                            // Delivery type
                            if (templateSupplier.DeliveryTypeId.HasValue)
                            {
                                var templateDeliveryType = templateDeliveryTypes.FirstOrDefault(p => p.DeliveryTypeId == templateSupplier.DeliveryTypeId.Value);
                                if (templateDeliveryType != null)
                                {
                                    var newDeliveryType = newDeliveryTypes.FirstOrDefault(p => p.Code == templateDeliveryType.Code && p.Name == templateDeliveryType.Name);
                                    if (newDeliveryType != null)
                                    {
                                        newSupplier.DeliveryType = newDeliveryType;
                                    }
                                    else
                                    {
                                        newDeliveryType = new DeliveryType()
                                        {
                                            Code = templateDeliveryType.Code,
                                            Name = templateDeliveryType.Name,

                                            Company = newCompany,
                                        };

                                        SetCreatedProperties(newDeliveryType);
                                        entities.DeliveryType.AddObject(newDeliveryType);

                                        newSupplier.DeliveryType = newDeliveryType;
                                    }
                                }
                            }

                            // Attest work flow group - only set if copied before hand
                            if (templateSupplier.AttestWorkFlowGroupId.HasValue)
                            {
                                var templateAttestWorkFlowGroup = templateAttestWorkFlowGroups.FirstOrDefault(p => p.AttestWorkFlowHeadId == templateSupplier.AttestWorkFlowGroupId.Value);
                                if (templateAttestWorkFlowGroup != null)
                                {
                                    var newAttestWorkFlowGroup = newAttestWorkFlowGroups.FirstOrDefault(p => p.AttestGroupCode == templateAttestWorkFlowGroup.AttestGroupCode && p.AttestGroupName == templateAttestWorkFlowGroup.AttestGroupName);
                                    if (newAttestWorkFlowGroup != null)
                                    {
                                        newSupplier.AttestWorkFlowGroup = newAttestWorkFlowGroup;
                                    }
                                }
                            }

                            // Intrastat code - only set if copied before hand
                            if (templateSupplier.IntrastatCodeId.HasValue)
                            {
                                var templateIntrastatCode = templateIntrastatCodes.FirstOrDefault(p => p.IntrastatCodeId == templateSupplier.IntrastatCodeId.Value);
                                if (templateIntrastatCode != null)
                                {
                                    var newIntrastatCode = newIntrastatCodes.FirstOrDefault(p => p.SysIntrastatCodeId == templateIntrastatCode.SysIntrastatCodeId);
                                    if (newIntrastatCode != null)
                                    {
                                        newSupplier.IntrastatCodeId = newIntrastatCode.IntrastatCodeId;
                                    }
                                }
                            }

                            #endregion

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;

                            #region Addresses

                            // Template contact
                            Contact templateContact = ContactManager.GetContactFromActor(entities, templateSupplier.ActorSupplierId, loadAllContactInfo: true);

                            // Get contact
                            Contact newContact = ContactManager.GetContactFromActor(entities, newSupplier.ActorSupplierId, loadAllContactInfo: true);
                            if (newContact == null)
                            {
                                // Create new Contact
                                newContact = new Contact()
                                {
                                    Actor = newSupplier.Actor,
                                    SysContactTypeId = (int)TermGroup_SysContactType.Company,
                                };
                                SetCreatedProperties(newContact);
                                entities.Contact.AddObject(newContact);
                            }

                            foreach (var templateContactAddress in templateContact.ContactAddress)
                            {
                                var newContactAddress = newContact.ContactAddress.FirstOrDefault(c => c.SysContactAddressTypeId == templateContactAddress.SysContactAddressTypeId && c.Name == templateContactAddress.Name);
                                if (newContactAddress == null)
                                {
                                    newContactAddress = new ContactAddress()
                                    {
                                        SysContactAddressTypeId = templateContactAddress.SysContactAddressTypeId,
                                        Name = templateContactAddress.Name,
                                        IsSecret = templateContactAddress.IsSecret,
                                    };

                                    foreach (var templateAddressRow in templateContactAddress.ContactAddressRow)
                                    {
                                        var newContactAddressRow = new ContactAddressRow()
                                        {
                                            SysContactAddressRowTypeId = templateAddressRow.SysContactAddressRowTypeId,
                                            Text = templateAddressRow.Text,
                                        };

                                        SetCreatedProperties(newContactAddressRow);
                                        newContactAddress.ContactAddressRow.Add(newContactAddressRow);
                                    }

                                    SetCreatedProperties(newContactAddress);
                                    newContact.ContactAddress.Add(newContactAddress);
                                }
                            }

                            foreach (var templateContactEcom in templateContact.ContactECom)
                            {
                                var newContactECom = newContact.ContactECom.FirstOrDefault(c => c.SysContactEComTypeId == templateContactEcom.SysContactEComTypeId && c.Name == templateContactEcom.Name);
                                if (newContactECom == null)
                                {
                                    newContactECom = new ContactECom()
                                    {
                                        SysContactEComTypeId = templateContactEcom.SysContactEComTypeId,
                                        Name = templateContactEcom.Name,
                                        Text = templateContactEcom.Text,
                                        Description = templateContactEcom.Description,
                                        IsSecret = templateContactEcom.IsSecret,
                                    };

                                    SetCreatedProperties(newContactECom);
                                    newContact.ContactECom.Add(newContactECom);
                                }

                                if (templateSupplier.ContactEcomId.HasValue && templateContactEcom.ContactEComId == templateSupplier.ContactEcomId.Value)
                                    newSupplier.ContactEcomId = newContactECom.ContactEComId;
                            }

                            #endregion

                            #region Categories

                            List<CompanyCategoryRecord> templateCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Supplier, SoeCategoryRecordEntity.Supplier, templateSupplier.ActorSupplierId, templateCompanyId);
                            List<CompanyCategoryRecord> newCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Supplier, SoeCategoryRecordEntity.Supplier, newSupplier.ActorSupplierId, newCompanyId);
                            List<Category> newCategories = CategoryManager.GetCategories(entities, SoeCategoryType.Supplier, new List<int>(), newCompanyId);

                            foreach (var templateCategoryRecord in templateCategoryRecords)
                            {
                                if (templateCategoryRecord.Category != null)
                                {
                                    var newCategory = newCategories.FirstOrDefault(c => c.Name == templateCategoryRecord.Category.Name);
                                    if (newCategory != null)
                                    {
                                        if (!newCategoryRecords.Any(c => c.CategoryId == newCategory.CategoryId))
                                        {
                                            CompanyCategoryRecord categoryRecord = new CompanyCategoryRecord()
                                            {
                                                ActorCompanyId = newCompanyId,
                                                CategoryId = newCategory.CategoryId,
                                                RecordId = newSupplier.ActorSupplierId,
                                                Entity = (int)SoeCategoryRecordEntity.Supplier,
                                            };
                                            entities.CompanyCategoryRecord.AddObject(categoryRecord);
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region ContactPersons

                            List<ContactPerson> templateContactPersons = ContactManager.GetContactPersons(entities, templateSupplier.ActorSupplierId);
                            List<ContactPerson> newContactPersons = ContactManager.GetContactPersons(entities, newSupplier.ActorSupplierId);
                            List<ContactPerson> allNewContactPersons = ContactManager.GetContactPersonsAll(entities, newCompanyId);

                            List<int> idsToMapTo = new List<int>();
                            foreach (var contactPerson in templateContactPersons)
                            {
                                if (!newContactPersons.Any(p => p.FirstName == contactPerson.FirstName && p.LastName == contactPerson.LastName))
                                {
                                    var newContactPerson = allNewContactPersons.FirstOrDefault(p => p.FirstName == contactPerson.FirstName && p.LastName == contactPerson.LastName);
                                    if (newContactPerson != null)
                                    {
                                        idsToMapTo.Add(newContactPerson.ActorContactPersonId);
                                    }
                                }
                            }

                            if (idsToMapTo.Count > 0)
                            {
                                result = ContactManager.SaveContactPersonMappings(entities, idsToMapTo, newSupplier.ActorSupplierId);
                                if (!result.Success)
                                    return result;
                            }

                            #endregion

                            #region Payment information

                            var templatePaymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, templateSupplier.ActorSupplierId, true, false);

                            if (templatePaymentInformation != null)
                            {
                                var newPaymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, newSupplier.ActorSupplierId, true, false);

                                if (newPaymentInformation == null)
                                {
                                    newPaymentInformation = new PaymentInformation()
                                    {
                                        DefaultSysPaymentTypeId = templatePaymentInformation.DefaultSysPaymentTypeId,

                                        //Set references
                                        Actor = newSupplier.Actor,
                                    };
                                    SetCreatedProperties(newPaymentInformation);
                                    entities.PaymentInformation.AddObject(newPaymentInformation);
                                }

                                foreach (var templateRow in templatePaymentInformation.ActivePaymentInformationRows)
                                {
                                    var newPaymentInformationRow = newPaymentInformation.PaymentInformationRow.FirstOrDefault(r => r.SysPaymentTypeId == templateRow.SysPaymentTypeId && r.PaymentNr == templateRow.PaymentNr);
                                    if (newPaymentInformationRow == null)
                                    {
                                        int? currencyId = null;
                                        if (templateRow.CurrencyId.HasValue)
                                        {
                                            var templateCurrency = templateCurrencies.FirstOrDefault(c => c.CurrencyId == templateRow.CurrencyId);
                                            if (templateCurrency != null)
                                            {
                                                var newCurrency = newCurrencies.FirstOrDefault(c => c.SysCurrencyId == templateCurrency.SysCurrencyId);
                                                if (newCurrency != null)
                                                    currencyId = newCurrency.CurrencyId;
                                            }
                                        }

                                        newPaymentInformationRow = new PaymentInformationRow
                                        {
                                            SysPaymentTypeId = templateRow.SysPaymentTypeId,
                                            PaymentNr = templateRow.PaymentNr,
                                            Default = templateRow.Default,
                                            ShownInInvoice = templateRow.ShownInInvoice,
                                            // Foreign payments
                                            BIC = templateRow.BIC,
                                            ClearingCode = templateRow.ClearingCode,
                                            PaymentCode = templateRow.PaymentCode,
                                            PaymentMethodCode = templateRow.PaymentMethodCode,
                                            PaymentForm = templateRow.PaymentForm,
                                            ChargeCode = templateRow.ChargeCode,
                                            IntermediaryCode = templateRow.IntermediaryCode,
                                            CurrencyAccount = templateRow.CurrencyAccount,
                                            //Set references
                                            PaymentInformation = newPaymentInformation,
                                            BankConnected = templateRow.BankConnected,
                                            CurrencyId = currencyId,
                                        };

                                        SetCreatedProperties(newPaymentInformationRow);
                                        entities.PaymentInformationRow.AddObject(newPaymentInformationRow);
                                    }
                                }
                            }

                            #endregion

                            #region Account settings

                            if (!templateSupplier.SupplierAccountStd.IsLoaded)
                                templateSupplier.SupplierAccountStd.Load();

                            if (!newSupplier.SupplierAccountStd.IsLoaded)
                                newSupplier.SupplierAccountStd.Load();

                            foreach (var templateSupplierAccountStd in templateSupplier.SupplierAccountStd)
                            {
                                if (newSupplier.SupplierAccountStd == null)
                                    newSupplier.SupplierAccountStd = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<SupplierAccountStd>();

                                var newSupplierAccountStd = newSupplier.SupplierAccountStd.FirstOrDefault(a => a.Type == templateSupplierAccountStd.Type);
                                if (newSupplierAccountStd == null)
                                {
                                    var stdMapping = mappings.FirstOrDefault(m => m.Item1 == templateSupplierAccountStd.AccountStd.Account.AccountDimId);
                                    if (stdMapping != null)
                                    {
                                        var accountId = stdMapping.Item3.Keys.Contains(templateSupplierAccountStd.AccountStd.AccountId) ? stdMapping.Item3[templateSupplierAccountStd.AccountStd.AccountId] : 0;
                                        if (accountId > 0)
                                        {
                                            var accountStd = newAccountStds.FirstOrDefault(a => a.AccountId == accountId);
                                            SupplierAccountStd supplierAccountStd = new SupplierAccountStd
                                            {
                                                Type = templateSupplierAccountStd.Type,
                                                AccountStd = accountStd,

                                                Supplier = newSupplier,
                                            };

                                            foreach (var templateInternalAccount in templateSupplierAccountStd.AccountInternal)
                                            {
                                                var mapping = mappings.FirstOrDefault(m => m.Item1 == templateInternalAccount.Account.AccountDimId);
                                                if (mapping != null)
                                                {
                                                    var newInternalAccountId = mapping.Item3.Keys.Contains(templateInternalAccount.AccountId) ? mapping.Item3[templateInternalAccount.AccountId] : 0;
                                                    var internalAccount = newAccountInternals.FirstOrDefault(a => a.AccountId == newInternalAccountId);
                                                    if (internalAccount != null)
                                                        supplierAccountStd.AccountInternal.Add(internalAccount);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex, this.log);
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        #region Billing

        public ActionResult CopyCompanyProductsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool includeExternal, SoeModule module, ref Dictionary<int, int> vatCodeMapping)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company company = CompanyManager.GetCompany(entities, newCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                List<ProductUnit> existingProductUnits = update ? ProductManager.GetProductUnits(entities, newCompanyId).ToList() : new List<ProductUnit>();
                List<ProductGroup> existingProductGroups = update ? ProductGroupManager.GetProductGroups(entities, newCompanyId).ToList() : new List<ProductGroup>();

                List<TimeCode> templateTimeCodes = TimeCodeManager.GetTimeCodes(entities, templateCompanyId, SoeTimeCodeType.Material);
                List<TimeCode> existingTimeCodes = update ? TimeCodeManager.GetTimeCodes(entities, newCompanyId, SoeTimeCodeType.Material) : new List<TimeCode>();

                List<InvoiceProductCopyDTO> templateCompanyInvoiceProducts = ProductManager.GetInvoiceProductsForTemplateCopying(entities, templateCompanyId, true, includeExternal);

                #endregion

                #region InvoiceProducts

                foreach (InvoiceProductCopyDTO templateProduct in templateCompanyInvoiceProducts)
                {
                    InvoiceProduct product = ProductManager.GetInvoiceProductByProductNr(entities, templateProduct.Number, newCompanyId);
                    if (product == null)
                    {
                        #region Invoice Product

                        int vatCodeId = -1;
                        if (templateProduct.VatCodeId.HasValue)
                            vatCodeMapping.TryGetValue(templateProduct.VatCodeId.Value, out vatCodeId);

                        product = new InvoiceProduct()
                        {
                            Type = templateProduct.Type,
                            Number = templateProduct.Number,
                            Name = templateProduct.Name,
                            Description = templateProduct.Description,
                            AccountingPrio = templateProduct.AccountingPrio ?? "",
                            CalculationType = templateProduct.CalculationType,
                            VatType = templateProduct.VatType,
                            VatCodeId = vatCodeId.ToNullable(),
                            PurchasePrice = templateProduct.PurchasePrice,

                            ShowDescriptionAsTextRow = templateProduct.ShowDescriptionAsTextRow,
                            ShowDescrAsTextRowOnPurchase = templateProduct.ShowDescrAsTextRowOnPurchase,
                        };

                        if (includeExternal)
                        {
                            product.ExternalProductId = templateProduct.ExternalProductId;
                            product.ExternalPriceListHeadId = templateProduct.ExternalPriceListHeadId;
                            product.SysWholesellerName = templateProduct.SysWholesellerName;
                        }

                        SetCreatedProperties(product);
                        product.Company.Add(company);

                        #endregion
                    }

                    #region ProductUnit

                    if (!string.IsNullOrEmpty(templateProduct.ProductUnitCode))
                    {
                        product.ProductUnit = existingProductUnits.FirstOrDefault(i => i.Code.ToLower() == templateProduct.ProductUnitCode.ToLower());
                        if (product.ProductUnit == null)
                        {
                            product.ProductUnit = new ProductUnit()
                            {
                                Code = templateProduct.ProductUnitCode,
                                Name = templateProduct.ProductUnitName,

                                //Set references
                                Company = company,
                            };
                            SetCreatedProperties(product.ProductUnit);
                            existingProductUnits.Add(product.ProductUnit);
                        }
                    }

                    #endregion

                    #region ProductGroup

                    if (!string.IsNullOrEmpty(templateProduct.ProductGroupCode))
                    {
                        product.ProductGroup = existingProductGroups.FirstOrDefault(i => i.Code.ToLower() == templateProduct.ProductGroupCode.ToLower());
                        if (product.ProductGroup == null)
                        {
                            product.ProductGroup = new ProductGroup()
                            {
                                Code = templateProduct.ProductGroupCode,
                                Name = templateProduct.ProductGroupName,

                                //Set references
                                Company = company,
                            };
                            SetCreatedProperties(product.ProductGroup);
                            existingProductGroups.Add(product.ProductGroup);
                        }
                    }

                    #endregion

                    #region ProductAccountStd

                    if (templateProduct.ProductAccounts != null && templateProduct.ProductAccounts.Count > 0)
                    {
                        foreach (ProductAccountStdDTO productAccountStd in templateProduct.ProductAccounts)
                        {
                            AccountStd accountStd = AccountManager.GetAccountStdByNr(entities, productAccountStd.AccountStd.AccountNr ?? String.Empty, newCompanyId);
                            if (accountStd == null)
                                continue;

                            ProductAccountStd newProductAccountStd = new ProductAccountStd()
                            {
                                Type = (int)productAccountStd.Type,
                                Percent = productAccountStd.Percent,

                                //Set references
                                AccountStd = accountStd,
                            };
                            SetCreatedProperties(newProductAccountStd);
                            product.ProductAccountStd.Add(newProductAccountStd);
                        }
                    }

                    #endregion

                    #region TimeCode

                    if (templateProduct.TimeCodeId != null)
                    {
                        var templateTimeCode = templateTimeCodes.FirstOrDefault(t => t.TimeCodeId == templateProduct.TimeCodeId);
                        if (templateTimeCode != null)
                        {
                            product.TimeCode = existingTimeCodes.FirstOrDefault(i => i.Code.ToLower() == templateTimeCode.Code.ToLower());
                            if (product.TimeCode == null)
                            {
                                product.TimeCode = new TimeCodeMaterial()
                                {
                                    Code = templateTimeCode.Code,
                                    Name = templateTimeCode.Name,
                                    Type = templateTimeCode.Type,
                                    RegistrationType = templateTimeCode.RegistrationType,
                                    Classification = templateTimeCode.Classification,
                                    Description = templateTimeCode.Description,

                                    //Set references
                                    Company = company,
                                };
                                SetCreatedProperties(product.TimeCode);

                                existingTimeCodes.Add(product.TimeCode);
                            }
                        }
                    }

                    #endregion
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("InvoiceProduct", templateCompanyId, newCompanyId, saved: true);

                #endregion

                return result;
            }
        }

        public ActionResult CopyPriceListsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, int> priceListsMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                List<PriceListType> templatePriceListTypes = ProductPricelistManager.GetPriceListTypes(templateCompanyId);
                List<PriceListType> existingPriceListTypes = update ? ProductPricelistManager.GetPriceListTypes(entities, newCompanyId, true) : new List<PriceListType>();

                //Mappings between template and new
                Dictionary<int, PriceListType> priceListyMapping = new Dictionary<int, PriceListType>();

                List<Currency> newCurrencies = CountryCurrencyManager.GetCurrencies(entities, newCompanyId);

                #endregion

                foreach (PriceListType templatePriceListType in templatePriceListTypes)
                {
                    #region PriceListType

                    if (templatePriceListType.Currency == null)
                        continue;

                    PriceListType priceListType = null;
                    bool exists = false;

                    if (update)
                    {
                        priceListType = existingPriceListTypes.FirstOrDefault(p => p.Name == templatePriceListType.Name);
                        exists = priceListType != null;
                    }

                    if (!exists)
                        priceListType = new PriceListType();

                    //Set values
                    priceListType.Name = templatePriceListType.Name;
                    priceListType.Description = templatePriceListType.Description;
                    priceListType.DiscountPercent = templatePriceListType.DiscountPercent;
                    priceListType.InclusiveVat = templatePriceListType.InclusiveVat;

                    //Set references
                    priceListType.Company = newCompany;
                    priceListType.Currency = newCurrencies.FirstOrDefault(i => i.SysCurrencyId == templatePriceListType.Currency.SysCurrencyId);
                    if (priceListType.Currency == null)
                    {
                        priceListType.Currency = new Currency()
                        {
                            SysCurrencyId = templatePriceListType.Currency.SysCurrencyId,
                            IntervalType = templatePriceListType.Currency.IntervalType,
                            UseSysRate = templatePriceListType.Currency.UseSysRate,

                            //Set references
                            Company = newCompany,
                        };
                        SetCreatedProperties(priceListType.Currency);

                        newCurrencies.Add(priceListType.Currency);
                    }

                    if (exists)
                        SetModifiedProperties(priceListType);
                    else
                        SetCreatedProperties(priceListType);

                    //Mapping
                    priceListyMapping.Add(templatePriceListType.PriceListTypeId, priceListType);

                    // Pricelist mapping
                    priceListsMapping.Add(templatePriceListType.PriceListTypeId, priceListType.PriceListTypeId);

                    #endregion

                    #region PriceList

                    IEnumerable<PriceList> templatePriceLists = ProductPricelistManager.GetPriceListsByType(entities, templatePriceListType.PriceListTypeId);
                    IEnumerable<PriceList> existingPriceLists = ProductPricelistManager.GetPriceListsByType(entities, priceListType.PriceListTypeId);

                    foreach (PriceList templatePriceList in templatePriceLists)
                    {
                        InvoiceProduct templateProduct = ProductManager.GetInvoiceProduct(entities, templatePriceList.ProductId);

                        if (templateProduct != null)
                        {
                            InvoiceProduct existingProduct = ProductManager.GetInvoiceProductByProductNr(entities, templateProduct.Number, newCompanyId);

                            if (existingProduct != null)
                            {
                                bool addNew = false;
                                PriceList priceList = null;

                                if (priceListType.PriceList != null)
                                    priceList = existingPriceLists.FirstOrDefault(p => p.ProductId == existingProduct.ProductId && p.StartDate == templatePriceList.StartDate);

                                if (priceList == null)
                                {
                                    addNew = true;
                                    priceList = new PriceList
                                    {
                                        ProductId = existingProduct.ProductId,
                                        PriceListType = priceListType,
                                        StartDate = templatePriceList.StartDate,
                                    };
                                }

                                priceList.Price = templatePriceList.Price;
                                priceList.DiscountPercent = templatePriceList.DiscountPercent;
                                priceList.StopDate = templatePriceList.StopDate;

                                if (addNew)
                                    SetCreatedProperties(priceList);
                                else
                                    SetModifiedProperties(priceList);
                            }
                        }
                    }

                    #endregion

                    result = SaveChangesWithTransaction(entities);
                }

                if (!result.Success)
                    result = LogCopyError("PriceList", templateCompanyId, newCompanyId, saved: true);
            }

            return result;
        }

        public ActionResult CopySupplierAgreementsFromTemplateCompany(int newCompanyId, int templateCompanyId, bool update, SoeModule module, ref Dictionary<int, int> priceListMappings)
        {
            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company company = CompanyManager.GetCompany(entities, newCompanyId);
                if (company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                List<SupplierAgreement> templateSupplierAgreements = SupplierAgreementManager.GetSupplierAgreements(templateCompanyId);
                List<SupplierAgreement> newSupplierAgreements = SupplierAgreementManager.GetSupplierAgreements(newCompanyId);

                #endregion

                #region InvoiceProducts

                foreach (SupplierAgreement templateSupplierAgreement in templateSupplierAgreements)
                {
                    if (newSupplierAgreements != null && newSupplierAgreements.Any(s => s.SysWholesellerId == templateSupplierAgreement.SysWholesellerId && s.Code == templateSupplierAgreement.Code && s.CodeType == templateSupplierAgreement.CodeType))
                        continue;

                    int priceListTypeId = -1;
                    if (templateSupplierAgreement.PriceListTypeId.HasValue)
                        priceListMappings.TryGetValue(templateSupplierAgreement.PriceListTypeId.Value, out priceListTypeId);

                    SupplierAgreement newAgreement = new SupplierAgreement()
                    {
                        CategoryId = templateSupplierAgreement.CategoryId,
                        CodeType = templateSupplierAgreement.CodeType,
                        Code = templateSupplierAgreement.Code,
                        Company = company,
                        Date = templateSupplierAgreement.Date,
                        DiscountPercent = templateSupplierAgreement.DiscountPercent,
                        PriceListOrigin = templateSupplierAgreement.PriceListOrigin,
                        PriceListTypeId = priceListTypeId > 0 ? priceListTypeId : (int?)null,
                        SysWholesellerId = templateSupplierAgreement.SysWholesellerId,
                    };

                    SetCreatedProperties(newAgreement);
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("SupplierAgreements", templateCompanyId, newCompanyId, saved: true);

                #endregion

                return result;
            }
        }

        public ActionResult CopyChecklistsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, Dictionary<int, Report> reportMapping)
        {
            #region Prereq
            ActionResult result = new ActionResult(true);
            List<ChecklistHead> templateChecklists = ChecklistManager.GetChecklistHeadsForType(TermGroup_ChecklistHeadType.Order, templateCompanyId, true);
            List<ChecklistHead> existingChecklists = ChecklistManager.GetChecklistHeadsForType(TermGroup_ChecklistHeadType.Order, newCompanyId, false);
            var multipleChoiceHeadMapping = new Dictionary<int, CheckListMultipleChoiceAnswerHead>();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                #endregion

                foreach (ChecklistHead templateChecklist in templateChecklists)
                {
                    ChecklistHead checklist = existingChecklists.FirstOrDefault(c => c.Name == templateChecklist.Name && c.Type == templateChecklist.Type);

                    var report = reportMapping.GetValue<int, Report>(templateChecklist.ReportId.HasValue ? templateChecklist.ReportId.Value : 0);

                    #region Checklist
                    if (checklist == null)
                    {
                        checklist = new ChecklistHead();
                        checklist.CopyFrom(templateChecklist);

                        if (report != null)
                            checklist.ReportId = report.ReportId;

                        ChecklistRow row;
                        foreach (var templateRow in templateChecklist.ChecklistRow)
                        {
                            row = new ChecklistRow();
                            row.CopyFrom(templateRow);
                            if (row.Type == (int)TermGroup_ChecklistRowType.MultipleChoice && row.CheckListMultipleChoiceAnswerHeadId.HasValue)
                            {
                                CheckListMultipleChoiceAnswerHead head = null;
                                if (multipleChoiceHeadMapping.ContainsKey(templateRow.CheckListMultipleChoiceAnswerHeadId.Value))
                                {
                                    head = multipleChoiceHeadMapping[templateRow.CheckListMultipleChoiceAnswerHeadId.Value];
                                }
                                else
                                {
                                    if (!templateRow.CheckListMultipleChoiceAnswerHeadReference.IsLoaded)
                                        templateRow.CheckListMultipleChoiceAnswerHeadReference.Load();

                                    head = new CheckListMultipleChoiceAnswerHead();
                                    head.CopyFrom(templateRow.CheckListMultipleChoiceAnswerHead);
                                    head.ActorCompanyId = newCompanyId;
                                    SetCreatedProperties(head);

                                    multipleChoiceHeadMapping.Add(row.CheckListMultipleChoiceAnswerHeadId.Value, head);
                                }

                                row.CheckListMultipleChoiceAnswerHeadId = null;
                                row.CheckListMultipleChoiceAnswerHead = head;
                            }

                            checklist.ChecklistRow.Add(row);
                            SetCreatedProperties(row);
                        }

                        SetCreatedProperties(checklist);
                        newCompany.ChecklistHead.Add(checklist);
                    }
                    else
                    {
                        checklist.ReportId = report != null ? report.ReportId : 0;
                    }
                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = LogCopyError("Checklist", templateCompanyId, newCompanyId, saved: true);
                    #endregion
                }

                return result;
            }
        }

        public ActionResult CopyChecklistFromAnotherCompany(int newCompanyId, int sourceCompanyId, bool update, int checkListHeadId)
        {
            ChecklistHead sourceCheckList = ChecklistManager.GetChecklistHead(checkListHeadId, sourceCompanyId, true);
            var multipleChoiceHeadMapping = new Dictionary<int, CheckListMultipleChoiceAnswerHead>();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                #region ChecklistHead

                ChecklistHead checklist = new ChecklistHead();
                checklist.CopyFrom(sourceCheckList);

                #region ChecklistRow

                foreach (var sourceRow in sourceCheckList.ChecklistRow)
                {
                    ChecklistRow row = new ChecklistRow();
                    row.CopyFrom(sourceRow);

                    if (row.Type == (int)TermGroup_ChecklistRowType.MultipleChoice && row.CheckListMultipleChoiceAnswerHeadId.HasValue)
                    {
                        CheckListMultipleChoiceAnswerHead head;
                        if (multipleChoiceHeadMapping.ContainsKey(sourceRow.CheckListMultipleChoiceAnswerHeadId.Value))
                        {
                            head = multipleChoiceHeadMapping[sourceRow.CheckListMultipleChoiceAnswerHeadId.Value];
                        }
                        else
                        {
                            if (!sourceRow.CheckListMultipleChoiceAnswerHeadReference.IsLoaded)
                                sourceRow.CheckListMultipleChoiceAnswerHeadReference.Load();
                            head = new CheckListMultipleChoiceAnswerHead();
                            head.CopyFrom(sourceRow.CheckListMultipleChoiceAnswerHead);
                            head.ActorCompanyId = newCompanyId;
                            SetCreatedProperties(head);
                            multipleChoiceHeadMapping.Add(row.CheckListMultipleChoiceAnswerHeadId.Value, head);
                        }

                        row.CheckListMultipleChoiceAnswerHeadId = null;
                        row.CheckListMultipleChoiceAnswerHead = head;
                    }

                    checklist.ChecklistRow.Add(row);
                    SetCreatedProperties(row);
                }
                #endregion

                SetCreatedProperties(checklist);
                newCompany.ChecklistHead.Add(checklist);

                #endregion

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("Checklist", sourceCompanyId, newCompanyId, saved: true);

                return result;
            }
        }

        public ActionResult CopyEmailTemplatesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            Dictionary<int, int> templateMapping = new Dictionary<int, int>();
            int templateCompanyEmailTemplateId = -1;
            #region Prereq

            List<EmailTemplate> templateEmailTemplates = EmailManager.GetEmailTemplates(templateCompanyId).ToList();
            List<EmailTemplate> existingEmailTemplate = EmailManager.GetEmailTemplates(newCompanyId).ToList();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                templateCompanyEmailTemplateId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultEmailTemplate, 0, templateCompanyId, 0);

                #endregion

                foreach (EmailTemplate templateEmailTemplate in templateEmailTemplates)
                {
                    EmailTemplate emailTemplate = existingEmailTemplate.FirstOrDefault(e => e.Name == templateEmailTemplate.Name && e.Subject == templateEmailTemplate.Subject && e.Body == templateEmailTemplate.Body && e.BodyIsHTML == templateEmailTemplate.BodyIsHTML && e.Type == templateEmailTemplate.Type);


                    #region EmailTemplate
                    if (emailTemplate == null)
                    {
                        emailTemplate = new EmailTemplate()
                        {
                            Name = templateEmailTemplate.Name,
                            BodyIsHTML = templateEmailTemplate.BodyIsHTML,
                            Body = templateEmailTemplate.Body,
                            Subject = templateEmailTemplate.Subject,
                            Type = templateEmailTemplate.Type
                        };

                        SetCreatedProperties(emailTemplate);
                        result = EmailManager.AddEmailTemplate(emailTemplate, newCompanyId);
                        if (!result.Success)
                            result = LogCopyError("EmailTemplate", templateCompanyId, newCompanyId, saved: true);

                    }
                    templateMapping.Add(templateEmailTemplate.EmailTemplateId, emailTemplate.EmailTemplateId);
                    #endregion
                }
                #region Settings

                if (templateCompanyEmailTemplateId > 0 && templateMapping.Any(a => a.Key == templateCompanyEmailTemplateId))
                {
                    Dictionary<int, int> intValues = new Dictionary<int, int>();
                    var copiedValue = templateMapping.Where(a => a.Key == templateCompanyEmailTemplateId).Select(s => s.Value).FirstOrDefault();
                    intValues.Add((int)CompanySettingType.BillingDefaultEmailTemplate, copiedValue);

                    result = SettingManager.UpdateInsertIntSettings(SettingMainType.Company, intValues, 0, newCompanyId, 0);
                }

                #endregion
                return result;
            }
        }

        public ActionResult CopyVatCodesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, int> vatCodesMapping)
        {
            // We do not support updating existing vatcodes
            var vatCodesExists = AccountManager.GetVatCodes(newCompanyId).Any();
            if (vatCodesExists)
                return new ActionResult(true);

            // Get accountidmapping
            ActionResult result = this.GetAccountIdMapping(newCompanyId, templateCompanyId);
            if (!result.Success || result.IntDict == null)
                return result;

            var accountIdMapping = result.IntDict;
            var templateVatCodes = AccountManager.GetVatCodes(templateCompanyId);

            using (var entities = new CompEntities())
            {
                try
                {
                    foreach (var item in templateVatCodes.ToList())
                    {
                        int? purchaseVatAccountId = null;
                        if (item.PurchaseVATAccountId.HasValue)
                            purchaseVatAccountId = accountIdMapping[(int)item.PurchaseVATAccountId];

                        var vatCode = new VatCode()
                        {
                            AccountId = accountIdMapping[item.AccountId],
                            ActorCompanyId = newCompanyId,
                            Code = item.Code,
                            Name = item.Name,
                            Percent = item.Percent,
                            PurchaseVATAccountId = purchaseVatAccountId,
                        };
                        SetCreatedProperties(vatCode);
                        entities.VatCode.AddObject(vatCode);

                        result = SaveChanges(entities);

                        //Add mapping
                        vatCodesMapping.Add(item.VatCodeId, vatCode.VatCodeId);
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    result.Success = false;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else if (result.Exception != null)
                        base.LogError(result.Exception, this.log);
                    else if (result.ErrorMessage != null)
                        base.LogError(result.ErrorMessage);
                    else
                        base.LogError("Error in CopyVatCodesFromTemplateCompany");

                    entities.Connection.Close();
                }

            }

            return new ActionResult();
        }

        public ActionResult CopyImportsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq
            List<Import> templateImports = ImportExportManager.GetImports(templateCompanyId, false);
            List<Import> existingImports = ImportExportManager.GetImports(newCompanyId, false);

            if (templateImports.IsNullOrEmpty())
                return result;

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                #endregion

                foreach (Import templateImport in templateImports)
                {
                    if (existingImports.Any(e => e.Name == templateImport.Name))
                        continue;

                    #region New Import

                    Import newImport = new Import()
                    {
                        Name = templateImport.Name,
                        ImportDefinitionId = templateImport.ImportDefinitionId,
                        Type = templateImport.Type,
                        Module = templateImport.Module,
                        Standard = templateImport.Standard,
                        DeleteAfter = templateImport.DeleteAfter,
                        State = templateImport.State
                    };

                    SetCreatedProperties(newImport);
                    newCompany.Import.Add(newImport);

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("Import", templateCompanyId, newCompanyId, saved: true);
            }

            return result;
        }

        public ActionResult CopyCompanyWholesellerPricelistsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            var result = new ActionResult();

            List<CompanyWholesellerPricelist> templatePriceLists = WholeSellerManager.GetAllCompanyWholesellerPriceLists(templateCompanyId);
            List<CompanyWholesellerPricelist> existingPriceLists = WholeSellerManager.GetAllCompanyWholesellerPriceLists(newCompanyId);

            var templatePriceListItems = WholeSellerManager.GetCompanyWholesellerPriceLists(templateCompanyId, null, true);

            using (var entities = new CompEntities())
            {
                try
                {
                    List<CompanyWholesellerPriceListViewDTO> priceListItems = new List<CompanyWholesellerPriceListViewDTO>();

                    foreach (CompanyWholesellerPricelist templatePriceList in templatePriceLists)
                    {
                        CompanyWholesellerPricelist existingPriceList = existingPriceLists.FirstOrDefault(v => v.SysPriceListHeadId == templatePriceList.SysPriceListHeadId && v.SysWholesellerId == templatePriceList.SysWholesellerId);
                        if (existingPriceList == null)
                        {
                            CompanyWholesellerPriceListViewDTO priceListItem = templatePriceListItems.FirstOrDefault(v => v.CompanyWholesellerPriceListId == templatePriceList.CompanyWholesellerPriceListId);
                            if (priceListItem != null)
                            {
                                priceListItem.ActorCompanyId = newCompanyId;
                                priceListItem.CompanyWholesellerPriceListId = null;
                                priceListItem.CompanyWholesellerId = null;
                                priceListItems.Add(priceListItem);
                            }
                        }
                    }

                    result = WholeSellerManager.SaveCompanyWholesellerPriceLists(priceListItems, newCompanyId);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    result.Success = false;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else if (result.Exception != null)
                        base.LogError(result.Exception, this.log);
                    else if (result.ErrorMessage != null)
                        base.LogError(result.ErrorMessage);
                    else
                        base.LogError("Error in CopyCompanyWholesellerPricelistsFromTemplateCompany");

                    entities.Connection.Close();
                }

            }

            return result;
        }

        public ActionResult CopyCustomersFromTemplateCompany(int newCompanyId, int templateCompanyId, bool update, ref Dictionary<int, Report> reportMapping)
        {
            var result = new ActionResult();

            #region Prereq

            // Copy template company billing settings
            CopyBillingSettingsFromTemplateCompany(newCompanyId, templateCompanyId, update, true);

            // Copy template company price lists
            var priceListsMapping = new Dictionary<int, int>();
            CopySupplierAgreementsFromTemplateCompany(newCompanyId, templateCompanyId, update, SoeModule.Billing, ref priceListsMapping);

            List<Customer> templateCustomers = CustomerManager.GetCustomersByCompany(templateCompanyId, true, loadCustomerProducts: true, loadAccount: true);
            if (templateCustomers == null || templateCustomers.Count == 0)
                return new ActionResult(true);

            Company newCompany = CompanyManager.GetCompany(newCompanyId);
            if (newCompany == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            List<Currency> templateCurrencies = CountryCurrencyManager.GetCurrencies(templateCompanyId);
            List<Currency> newCurrencies = CountryCurrencyManager.GetCurrencies(newCompanyId);

            List<PaymentCondition> templatePaymentConditions = PaymentManager.GetPaymentConditions(templateCompanyId);
            List<PaymentCondition> newPaymentConditions = PaymentManager.GetPaymentConditions(newCompanyId);

            List<DeliveryCondition> templateDeliveryConditions = InvoiceManager.GetDeliveryConditions(templateCompanyId);
            List<DeliveryCondition> newDeliveryConditions = InvoiceManager.GetDeliveryConditions(newCompanyId);

            List<DeliveryType> templateDeliveryTypes = InvoiceManager.GetDeliveryTypes(templateCompanyId);
            List<DeliveryType> newDeliveryTypes = InvoiceManager.GetDeliveryTypes(newCompanyId);

            List<PriceListType> templatePriceListTypes = ProductPricelistManager.GetPriceListTypes(templateCompanyId);
            List<PriceListType> newPriceListTypes = ProductPricelistManager.GetPriceListTypes(newCompanyId);

            List<SysWholeseller> templateWholesellers = WholeSellerManager.GetSysWholesellersByCompany(templateCompanyId);
            List<SysWholeseller> newWholesellers = WholeSellerManager.GetSysWholesellersByCompany(newCompanyId);

            List<Report> templateReports = ReportManager.GetReports(templateCompanyId, null, loadReportSelection: true);
            List<Report> newReports = ReportManager.GetReports(newCompanyId, null, loadReportSelection: true);

            var templateAccountInternals = AccountManager.GetAccountInternals(templateCompanyId, true, true);
            var newAccountInternals = AccountManager.GetAccountInternals(newCompanyId, true, true);

            List<AccountDim> accountDimsTemplate = AccountManager.GetAccountDimsByCompany(templateCompanyId, loadAccounts: true);
            List<AccountDim> accountDimsNew = AccountManager.GetAccountDimsByCompany(newCompanyId, loadAccounts: true);
            if (accountDimsNew.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

            // Get full account dim mapping
            List<Tuple<int, int, Dictionary<int, int>>> mappings = GetAccountDimMappingsWithAccounts(accountDimsTemplate, accountDimsNew);

            List<InvoiceProductSmallDTO> templateCompanyInvoiceProducts = ProductManager.GetInvoiceProductsSmall(templateCompanyId, true);
            List<InvoiceProductSmallDTO> newCompanyInvoiceProducts = ProductManager.GetInvoiceProductsSmall(newCompanyId, true);

            #endregion

            try
            {
                foreach (Customer customer in templateCustomers)
                {
                    if (!customer.CurrencyReference.IsLoaded)
                        customer.CurrencyReference.Load();

                    #region Currency

                    Currency currency = newCurrencies.FirstOrDefault(c => c.SysCurrencyId == customer.Currency.SysCurrencyId);
                    if (currency == null)
                    {
                        Currency templateCurrency = templateCurrencies.FirstOrDefault(c => c.SysCurrencyId == customer.Currency.SysCurrencyId);
                        if (templateCurrency != null)
                        {
                            Currency curr = new Currency()
                            {
                                ActorCompanyId = newCompanyId,
                                SysCurrencyId = templateCurrency.SysCurrencyId,
                                UseSysRate = templateCurrency.UseSysRate,
                                IntervalType = templateCurrency.IntervalType,
                            };

                            result = CountryCurrencyManager.AddCurrency(curr, DateTime.Now, newCompanyId);
                            if (!result.Success)
                                continue;
                            else
                                newCurrencies.Add(curr);
                        }
                        else
                            continue;
                    }

                    #endregion

                    #region PaymentCondition

                    if (templatePaymentConditions != null && customer.PaymentConditionId.HasValue)
                    {
                        PaymentCondition paymentCondition = templatePaymentConditions.FirstOrDefault(p => p.PaymentConditionId == customer.PaymentConditionId.Value);
                        if (paymentCondition != null)
                        {
                            if (newPaymentConditions != null)
                            {
                                PaymentCondition newPaymentCondition = newPaymentConditions.FirstOrDefault(p => p.Name == paymentCondition.Name);

                                if (newPaymentCondition != null)
                                {
                                    customer.PaymentConditionId = newPaymentCondition.PaymentConditionId;
                                }
                                else
                                {
                                    PaymentCondition newPC = new PaymentCondition()
                                    {
                                        Code = paymentCondition.Code,
                                        Name = paymentCondition.Name,
                                        Days = paymentCondition.Days,
                                        DiscountDays = paymentCondition.DiscountDays,
                                        DiscountPercent = paymentCondition.DiscountPercent,
                                    };

                                    PaymentManager.AddPaymentCondition(newPC, newCompanyId);

                                    if (newPC.PaymentConditionId != 0)
                                        customer.PaymentConditionId = newPC.PaymentConditionId;
                                }
                            }
                            else
                            {
                                PaymentCondition newPC = new PaymentCondition()
                                {
                                    Code = paymentCondition.Code,
                                    Name = paymentCondition.Name,
                                    Days = paymentCondition.Days,
                                    DiscountDays = paymentCondition.DiscountDays,
                                    DiscountPercent = paymentCondition.DiscountPercent,
                                };

                                PaymentManager.AddPaymentCondition(newPC, newCompanyId);

                                if (newPC.PaymentConditionId != 0)
                                {
                                    customer.PaymentConditionId = newPC.PaymentConditionId;
                                    newPaymentConditions.Add(newPC);
                                }
                            }
                        }
                    }

                    #endregion

                    #region OfferTemplates

                    if (customer.OfferTemplate != null)
                    {
                        Report offerReport = templateReports.FirstOrDefault(r => r.ReportId == (int)customer.OfferTemplate);

                        if (offerReport != null)
                        {
                            Report newOfferReport = newReports.FirstOrDefault(r => r.ReportNr == offerReport.ReportNr && r.Name == offerReport.Name);

                            if (newOfferReport != null)
                            {
                                customer.OfferTemplate = newOfferReport.ReportId;
                            }
                            else
                            {
                                newOfferReport = new Report()
                                {
                                    ReportTemplateId = offerReport.ReportTemplateId,
                                    Module = offerReport.Module,
                                    ReportNr = offerReport.ReportNr,
                                    Name = offerReport.Name,
                                    Description = offerReport.Description,
                                    Standard = offerReport.Standard,
                                    Original = offerReport.Original,
                                    ExportType = offerReport.ExportType,
                                    FileType = offerReport.FileType,
                                    IncludeAllHistoricalData = offerReport.IncludeAllHistoricalData,
                                    IncludeBudget = offerReport.IncludeBudget,
                                    NoOfYearsBackinPreviousYear = offerReport.NoOfYearsBackinPreviousYear,
                                    ShowInAccountingReports = offerReport.ShowInAccountingReports,
                                    GetDetailedInformation = offerReport.GetDetailedInformation,
                                    State = (int)SoeEntityState.Active,
                                };

                                ReportManager.AddReport(newOfferReport, newCompanyId);

                                if (newOfferReport.ReportId != 0)
                                {
                                    customer.OfferTemplate = newOfferReport.ReportId;
                                    newReports.Add(newOfferReport);
                                }
                            }

                            if (!reportMapping.ContainsKey(offerReport.ReportId))
                                reportMapping.Add(offerReport.ReportId, newOfferReport);
                        }
                    }

                    #endregion

                    #region OrderTemplates

                    if (customer.OrderTemplate != null)
                    {
                        Report orderReport = templateReports.FirstOrDefault(r => r.ReportId == (int)customer.OrderTemplate);

                        if (orderReport != null)
                        {
                            Report newOrderReport = newReports.FirstOrDefault(r => r.ReportNr == orderReport.ReportNr && r.Name == orderReport.Name);

                            if (newOrderReport != null)
                            {
                                customer.OrderTemplate = newOrderReport.ReportId;
                            }
                            else
                            {
                                newOrderReport = new Report()
                                {
                                    ReportTemplateId = orderReport.ReportTemplateId,
                                    Module = orderReport.Module,
                                    ReportNr = orderReport.ReportNr,
                                    Name = orderReport.Name,
                                    Description = orderReport.Description,
                                    Standard = orderReport.Standard,
                                    Original = orderReport.Original,
                                    ExportType = orderReport.ExportType,
                                    FileType = orderReport.FileType,
                                    IncludeAllHistoricalData = orderReport.IncludeAllHistoricalData,
                                    IncludeBudget = orderReport.IncludeBudget,
                                    NoOfYearsBackinPreviousYear = orderReport.NoOfYearsBackinPreviousYear,
                                    ShowInAccountingReports = orderReport.ShowInAccountingReports,
                                    GetDetailedInformation = orderReport.GetDetailedInformation,
                                    State = (int)SoeEntityState.Active,
                                };

                                ReportManager.AddReport(newOrderReport, newCompanyId);

                                if (newOrderReport.ReportId != 0)
                                {
                                    customer.OrderTemplate = newOrderReport.ReportId;
                                    newReports.Add(newOrderReport);
                                }
                            }

                            if (!reportMapping.ContainsKey(orderReport.ReportId))
                                reportMapping.Add(orderReport.ReportId, newOrderReport);
                        }
                    }

                    #endregion

                    #region InvoiceTemplates

                    if (customer.BillingTemplate != null)
                    {
                        Report invoiceReport = templateReports.FirstOrDefault(r => r.ReportId == (int)customer.BillingTemplate);

                        if (invoiceReport != null)
                        {
                            Report newInvoiceReport = newReports.FirstOrDefault(r => r.ReportNr == invoiceReport.ReportNr && r.Name == invoiceReport.Name);

                            if (newInvoiceReport != null)
                            {
                                customer.BillingTemplate = newInvoiceReport.ReportId;
                            }
                            else
                            {
                                newInvoiceReport = new Report()
                                {
                                    ReportTemplateId = invoiceReport.ReportTemplateId,
                                    Module = invoiceReport.Module,
                                    ReportNr = invoiceReport.ReportNr,
                                    Name = invoiceReport.Name,
                                    Description = invoiceReport.Description,
                                    Standard = invoiceReport.Standard,
                                    Original = invoiceReport.Original,
                                    ExportType = invoiceReport.ExportType,
                                    FileType = invoiceReport.FileType,
                                    IncludeAllHistoricalData = invoiceReport.IncludeAllHistoricalData,
                                    IncludeBudget = invoiceReport.IncludeBudget,
                                    NoOfYearsBackinPreviousYear = invoiceReport.NoOfYearsBackinPreviousYear,
                                    ShowInAccountingReports = invoiceReport.ShowInAccountingReports,
                                    GetDetailedInformation = invoiceReport.GetDetailedInformation,
                                    State = (int)SoeEntityState.Active,
                                };

                                ReportManager.AddReport(newInvoiceReport, newCompanyId);

                                if (newInvoiceReport.ReportId != 0)
                                {
                                    customer.BillingTemplate = newInvoiceReport.ReportId;
                                    newReports.Add(newInvoiceReport);
                                }
                            }

                            if (!reportMapping.ContainsKey(invoiceReport.ReportId))
                                reportMapping.Add(invoiceReport.ReportId, newInvoiceReport);
                        }
                    }

                    #endregion

                    #region ContractTemplates

                    if (customer.AgreementTemplate != null)
                    {
                        Report contractReport = templateReports.FirstOrDefault(r => r.ReportId == (int)customer.AgreementTemplate);
                        if (contractReport != null)
                        {
                            Report newContractReport = newReports.FirstOrDefault(r => r.ReportNr == contractReport.ReportNr && r.Name == contractReport.Name);
                            if (newContractReport != null)
                            {
                                customer.AgreementTemplate = newContractReport.ReportId;
                            }
                            else
                            {
                                newContractReport = new Report()
                                {
                                    ReportTemplateId = contractReport.ReportTemplateId,
                                    Module = contractReport.Module,
                                    ReportNr = contractReport.ReportNr,
                                    Name = contractReport.Name,
                                    Description = contractReport.Description,
                                    Standard = contractReport.Standard,
                                    Original = contractReport.Original,
                                    ExportType = contractReport.ExportType,
                                    FileType = contractReport.FileType,
                                    IncludeAllHistoricalData = contractReport.IncludeAllHistoricalData,
                                    IncludeBudget = contractReport.IncludeBudget,
                                    NoOfYearsBackinPreviousYear = contractReport.NoOfYearsBackinPreviousYear,
                                    ShowInAccountingReports = contractReport.ShowInAccountingReports,
                                    GetDetailedInformation = contractReport.GetDetailedInformation,
                                    State = (int)SoeEntityState.Active,
                                };

                                ReportManager.AddReport(newContractReport, newCompanyId);

                                if (newContractReport.ReportId != 0)
                                {
                                    customer.AgreementTemplate = newContractReport.ReportId;
                                    newReports.Add(newContractReport);
                                }
                            }

                            if (!reportMapping.ContainsKey(contractReport.ReportId))
                                reportMapping.Add(contractReport.ReportId, newContractReport);
                        }
                    }

                    #endregion

                    #region DeliveryConditions

                    if (templateDeliveryConditions != null && customer.DeliveryConditionId != null)
                    {
                        DeliveryCondition deliveryCondition = templateDeliveryConditions.FirstOrDefault(p => p.DeliveryConditionId == (int)customer.DeliveryConditionId);

                        if (deliveryCondition != null && newDeliveryConditions != null)
                        {
                            DeliveryCondition newDeliveryCondition = newDeliveryConditions.FirstOrDefault(p => p.Name == deliveryCondition.Name);

                            if (newDeliveryCondition != null)
                            {
                                customer.DeliveryConditionId = newDeliveryCondition.DeliveryConditionId;
                            }
                            else
                            {
                                newDeliveryCondition = new DeliveryCondition()
                                {
                                    Code = deliveryCondition.Code,
                                    Name = deliveryCondition.Name,
                                };

                                InvoiceManager.AddDeliveryCondition(newDeliveryCondition, newCompanyId);

                                if (newDeliveryCondition.DeliveryConditionId != 0)
                                {
                                    customer.DeliveryConditionId = newDeliveryCondition.DeliveryConditionId;
                                    newDeliveryConditions.Add(newDeliveryCondition);
                                }
                            }
                        }
                    }

                    #endregion

                    #region DeliveryTypes

                    if (templateDeliveryTypes != null && customer.DeliveryTypeId != null)
                    {
                        DeliveryType deliveryType = templateDeliveryTypes.FirstOrDefault(p => p.DeliveryTypeId == (int)customer.DeliveryTypeId);
                        if (deliveryType != null && newDeliveryTypes != null)
                        {
                            DeliveryType newDeliveryType = newDeliveryTypes.FirstOrDefault(p => p.Name == deliveryType.Name);
                            if (newDeliveryType != null)
                            {
                                customer.DeliveryTypeId = newDeliveryType.DeliveryTypeId;
                            }
                            else
                            {
                                newDeliveryType = new DeliveryType()
                                {
                                    Code = deliveryType.Code,
                                    Name = deliveryType.Name,
                                };

                                InvoiceManager.AddDeliveryType(newDeliveryType, newCompanyId);

                                if (newDeliveryType.DeliveryTypeId != 0)
                                {
                                    customer.DeliveryTypeId = newDeliveryType.DeliveryTypeId;
                                    newDeliveryTypes.Add(newDeliveryType);
                                }
                            }
                        }
                    }

                    #endregion

                    #region PriceListTypes

                    if (templatePriceListTypes != null && customer.PriceListTypeId != null)
                    {
                        PriceListType priceListType = templatePriceListTypes.FirstOrDefault(p => p.PriceListTypeId == (int)customer.PriceListTypeId);
                        if (priceListType != null && newPriceListTypes != null)
                        {
                            PriceListType newPriceListType = newPriceListTypes.FirstOrDefault(p => p.Name == priceListType.Name);
                            if (newPriceListType != null)
                            {
                                customer.PriceListTypeId = newPriceListType.PriceListTypeId;
                            }
                            else
                            {
                                Currency curr = newCurrencies.FirstOrDefault(c => c.SysCurrencyId == priceListType.Currency.SysCurrencyId);
                                if (curr != null)
                                {
                                    newPriceListType = new PriceListType()
                                    {
                                        Name = priceListType.Name,
                                        Description = priceListType.Description,
                                        DiscountPercent = priceListType.DiscountPercent,
                                        InclusiveVat = priceListType.InclusiveVat,
                                        State = (int)SoeEntityState.Active,
                                    };

                                    ProductPricelistManager.AddPriceListType(newPriceListType, newCompanyId, curr.CurrencyId);

                                    if (newPriceListType.PriceListTypeId != 0)
                                    {
                                        customer.PriceListTypeId = newPriceListType.PriceListTypeId;
                                        newPriceListTypes.Add(newPriceListType);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Wholesellers

                    if (templateWholesellers != null && customer.SysWholeSellerId != null)
                    {
                        SysWholeseller wholeseller = templateWholesellers.FirstOrDefault(s => s.SysWholesellerId == (int)customer.SysWholeSellerId);
                        if (wholeseller != null && newWholesellers != null)
                        {
                            SysWholeseller newWholeseller = newWholesellers.FirstOrDefault(s => s.SysWholesellerId == wholeseller.SysWholesellerId);
                            if (newWholeseller != null)
                                customer.SysWholeSellerId = newWholeseller.SysWholesellerId;
                        }
                    }

                    #endregion

                    CustomerDTO newCustomer = customer.ToDTO(false, false, true);

                    newCustomer.ActorCustomerId = 0;
                    newCustomer.CurrencyId = currency.CurrencyId;

                    #region Account settings

                    if (customer.CustomerAccountStd.Count > 0 && newCustomer.AccountingSettings == null)
                        newCustomer.AccountingSettings = new List<AccountingSettingsRowDTO>();

                    var standardMapping = mappings.FirstOrDefault();
                    var newStandardDim = accountDimsNew.FirstOrDefault(d => d.AccountDimNr == 1);
                    foreach (var templateCustomerStandard in customer.CustomerAccountStd)
                    {
                        AccountingSettingsRowDTO accDto = new AccountingSettingsRowDTO()
                        {
                            Type = (int)templateCustomerStandard.Type,
                            Percent = 0
                        };

                        if (templateCustomerStandard.AccountStd != null)
                        {
                            var newAccountStdId = standardMapping.Item3.ContainsKey(templateCustomerStandard.AccountStd.AccountId) ? standardMapping.Item3[templateCustomerStandard.AccountStd.AccountId] : 0;
                            if (newAccountStdId > 0)
                            {
                                var newAccount = newStandardDim.Account.FirstOrDefault(a => a.AccountId == newAccountStdId);
                                if (newAccount != null)
                                {
                                    accDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                                    accDto.Account1Id = newAccount.AccountId;
                                    accDto.Account1Nr = newAccount.AccountNr;
                                    accDto.Account1Name = newAccount.Name;
                                }
                            }
                        }

                        #region Internals

                        if (templateCustomerStandard != null && templateCustomerStandard.AccountInternal != null)
                        {
                            int dimCounter = 2;
                            foreach (var accInt in templateCustomerStandard.AccountInternal.Where(a => a.Account != null && a.Account.AccountDim != null && a.Account.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(a => a.Account.AccountDim.AccountDimNr))
                            {
                                var internalMapping = mappings.FirstOrDefault(a => a.Item1 == accInt.Account.AccountDim.AccountDimId);
                                var accountId = internalMapping.Item3.ContainsKey(accInt.AccountId) ? internalMapping.Item3[accInt.AccountId] : 0;

                                var dim = accountDimsNew.FirstOrDefault(a => a.AccountDimId == internalMapping.Item2);
                                var account = dim.Account.FirstOrDefault(a => a.AccountId == accountId);

                                if (account != null)
                                {
                                    // TODO: Does not support dim numbers over 6!!!
                                    if (dimCounter == 2)
                                    {
                                        accDto.AccountDim2Nr = account.AccountDim.AccountDimNr;
                                        accDto.Account2Id = account.AccountId;
                                        accDto.Account2Nr = account.AccountNr;
                                        accDto.Account2Name = account.Name;
                                    }
                                    else if (dimCounter == 3)
                                    {
                                        accDto.AccountDim3Nr = account.AccountDim.AccountDimNr;
                                        accDto.Account3Id = account.AccountId;
                                        accDto.Account3Nr = account.AccountNr;
                                        accDto.Account3Name = account.Name;
                                    }
                                    else if (dimCounter == 4)
                                    {
                                        accDto.AccountDim4Nr = account.AccountDim.AccountDimNr;
                                        accDto.Account4Id = account.AccountId;
                                        accDto.Account4Nr = account.AccountNr;
                                        accDto.Account4Name = account.Name;
                                    }
                                    else if (dimCounter == 5)
                                    {
                                        accDto.AccountDim5Nr = account.AccountDim.AccountDimNr;
                                        accDto.Account5Id = account.AccountId;
                                        accDto.Account5Nr = account.AccountNr;
                                        accDto.Account5Name = account.Name;
                                    }
                                    else if (dimCounter == 6)
                                    {
                                        accDto.AccountDim6Nr = account.AccountDim.AccountDimNr;
                                        accDto.Account6Id = account.AccountId;
                                        accDto.Account6Nr = account.AccountNr;
                                        accDto.Account6Name = account.Name;
                                    }

                                    dimCounter++;
                                }
                            }
                        }

                        #endregion

                        newCustomer.AccountingSettings.Add(accDto);
                    }

                    #endregion

                    #region Addresses

                    if (!customer.ActorReference.IsLoaded)
                        customer.ActorReference.Load();

                    List<ContactAddressItem> addressItems = ContactManager.GetContactAddressItems(customer.Actor.ActorId);

                    List<ContactAddressItem> newAddressItems = new List<ContactAddressItem>();

                    if (addressItems != null && addressItems.Count > 0)
                    {
                        foreach (ContactAddressItem addressItem in addressItems)
                        {
                            ContactAddressItem newAddressItem = new ContactAddressItem()
                            {
                                Address = addressItem.Address,
                                AddressCO = addressItem.AddressCO,
                                AddressCOIsSecret = addressItem.AddressCOIsSecret,
                                AddressIsSecret = addressItem.AddressIsSecret,
                                ContactAddressItemType = addressItem.ContactAddressItemType,
                                Country = addressItem.Country,
                                DisplayAddress = addressItem.DisplayAddress,
                                EComDescription = addressItem.EComDescription,
                                EComIsSecret = addressItem.EComIsSecret,
                                EComText = addressItem.EComText,
                                EntranceCode = addressItem.EntranceCode,
                                IsAddress = addressItem.IsAddress,
                                Name = addressItem.Name,
                                PostalAddress = addressItem.PostalAddress,
                                PostalCode = addressItem.PostalCode,
                                StreetAddress = addressItem.StreetAddress,
                                SysContactAddressTypeId = addressItem.SysContactAddressTypeId,
                                SysContactEComTypeId = addressItem.SysContactEComTypeId
                            };

                            newAddressItems.Add(newAddressItem);
                        }
                    }

                    newCustomer.ContactAddresses = newAddressItems;

                    #endregion

                    #region CustomerProducts

                    if (newCustomer.CustomerProducts == null)
                        newCustomer.CustomerProducts = new List<CustomerProductPriceSmallDTO>();

                    foreach (var customerProduct in customer.CustomerProduct)
                    {
                        var templateProduct = templateCompanyInvoiceProducts.FirstOrDefault(p => p.ProductId == customerProduct.ProductId);
                        if (templateProduct != null)
                        {
                            var newProduct = newCompanyInvoiceProducts.FirstOrDefault(p => p.Number == templateProduct.Number && p.Name == templateProduct.Name);
                            if (newProduct != null)
                            {
                                newCustomer.CustomerProducts.Add(new CustomerProductPriceSmallDTO() { ProductId = newProduct.ProductId, Price = customerProduct.Price, Name = newProduct.Name, Number = newProduct.Number });
                            }
                        }
                    }

                    #endregion

                    result = CustomerManager.SaveCustomer(newCustomer, null, null, null, newCompany.ActorCompanyId);

                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }

            return result;

        }

        public ActionResult CopyCustomerInvoicesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, SoeOriginType originType)
        {
            var result = new ActionResult();

            #region Prereq

            // Get Categories
            List<Category> templateContractCategories = CategoryManager.GetCategories(SoeCategoryType.Contract, templateCompanyId);
            List<Category> newContactCategories = CategoryManager.GetCategories(SoeCategoryType.Contract, newCompanyId);
            List<CompanyCategoryRecord> templateContractCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Contract, templateCompanyId);

            // Get ContractGroups
            List<ContractGroup> templateContractGroups = ContractManager.GetContractGroups(templateCompanyId).ToList();
            List<ContractGroup> newContractGroups = ContractManager.GetContractGroups(newCompanyId).ToList();

            // Get Currencies
            List<Currency> currencies = CountryCurrencyManager.GetCurrencies(newCompanyId);

            // Get Customers
            List<Customer> customers = CustomerManager.GetCustomersByCompany(newCompanyId, false);
            List<int> customerIds = customers.Select(c => c.ActorCustomerId).ToList();

            // Get open account year
            AccountYear currentAccountYear = AccountManager.GetCurrentAccountYear(newCompanyId, false);

            // Get voucher series types
            List<VoucherSeriesType> voucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(newCompanyId, false);

            // Get voucher series
            List<VoucherSeries> voucherSeries = null;
            if (currentAccountYear != null)
                voucherSeries = VoucherManager.GetVoucherSeriesByYear(currentAccountYear.AccountYearId, newCompanyId, false);

            // Get PaymentConditions
            List<PaymentCondition> templatePaymentConditions = PaymentManager.GetPaymentConditions(templateCompanyId);
            List<PaymentCondition> newPaymentConditions = PaymentManager.GetPaymentConditions(newCompanyId);

            // Get DeliveryTypes
            List<DeliveryType> templateDeliveryTypes = InvoiceManager.GetDeliveryTypes(templateCompanyId);
            List<DeliveryType> newDeliveryTypes = InvoiceManager.GetDeliveryTypes(newCompanyId);

            // Get DeliverConditions
            List<DeliveryCondition> templateDeliveryConditions = InvoiceManager.GetDeliveryConditions(templateCompanyId);
            List<DeliveryCondition> newDeliveryConditions = InvoiceManager.GetDeliveryConditions(newCompanyId);

            // Get PriceListTypes
            List<PriceListType> templatePriceListTypes = ProductPricelistManager.GetPriceListTypes(templateCompanyId);
            List<PriceListType> newPriceListTypes = ProductPricelistManager.GetPriceListTypes(newCompanyId);

            // Get Projects
            List<Project> templateProjects = ProjectManager.GetProjects(templateCompanyId, TermGroup_ProjectType.TimeProject, null, true, false, false, false);
            List<Project> newProjects = ProjectManager.GetProjects(templateCompanyId, TermGroup_ProjectType.TimeProject, null, true, false, false, false);

            // Get Checklists            
            List<ChecklistHead> newChecklistHeads = ChecklistManager.GetChecklistHeadsIncludingRows(newCompanyId);

            // Get AttestStates
            List<AttestState> templateAttestStates = new List<AttestState>();
            List<AttestState> newAttestStates = new List<AttestState>();

            if (originType == SoeOriginType.Contract)
            {
                templateAttestStates = AttestManager.GetAttestStates(templateCompanyId, TermGroup_AttestEntity.Contract, SoeModule.Billing);
                newAttestStates = AttestManager.GetAttestStates(newCompanyId, TermGroup_AttestEntity.Contract, SoeModule.Billing);
            }
            else if (originType == SoeOriginType.Order)
            {
                templateAttestStates = AttestManager.GetAttestStates(templateCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);
                newAttestStates = AttestManager.GetAttestStates(newCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);
            }

            // Get ProductUnits
            List<ProductUnit> templateProductUnits = ProductManager.GetProductUnits(templateCompanyId);
            List<ProductUnit> newProductUnits = ProductManager.GetProductUnits(newCompanyId);

            // Get Products
            List<InvoiceProduct> templateInvoiceProducts = ProductManager.GetInvoiceProducts(templateCompanyId, null, loadProductUnitAndGroup: false);
            List<InvoiceProduct> newInvoiceProducts = ProductManager.GetInvoiceProducts(templateCompanyId, null, loadProductUnitAndGroup: false);

            // Get Accounts
            List<Account> templateAccounts = AccountManager.GetAccounts(templateCompanyId);
            List<Account> newAccounts = AccountManager.GetAccounts(newCompanyId);

            // Get AccountDims
            List<AccountDim> templateAccountDims = AccountManager.GetAccountDimsByCompany(templateCompanyId);
            List<AccountDim> newAccountDims = AccountManager.GetAccountDimsByCompany(newCompanyId);

            // Get VatCodes
            List<VatCode> templateVatCodes = AccountManager.GetVatCodes(templateCompanyId);
            List<VatCode> newVatCodes = AccountManager.GetVatCodes(templateCompanyId);

            int count = 0;

            #endregion

            using (var entities = new CompEntities())
            {
                var newContactAddresses = entities.ContactAddress.Include("ContactAddressRow").Where(v => customerIds.Contains(v.Contact.Actor.ActorId)).ToList();

                foreach (var invoiceId in InvoiceManager.GetInvoiceIds(templateCompanyId, originType))
                {
                    count++;
                    CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, invoiceId, loadOrigin: true);

                    if (customerInvoice == null)
                        continue;

                    if (originType == SoeOriginType.Order && (customerInvoice.Status == (int)SoeOriginStatus.OrderFullyInvoice || customerInvoice.Status == (int)SoeOriginStatus.OrderClosed || customerInvoice.Status == (int)SoeOriginStatus.Cancel || string.IsNullOrEmpty(customerInvoice.InvoiceNr)))
                        continue;

                    if (customerInvoice.SeqNr.HasValue)
                    {
                        var oldInvoice = InvoiceManager.GetInvoiceBySeqNr(entities, newCompanyId, customerInvoice.SeqNr.Value, originType);
                        if (oldInvoice != null && !string.IsNullOrEmpty(oldInvoice.InvoiceNr) && oldInvoice.InvoiceNr.Equals(customerInvoice.InvoiceNr))
                            continue;
                    }

                    customerInvoice = InvoiceManager.GetCustomerInvoice(entities, invoiceId, loadOrigin: true, loadActor: true, loadInvoiceRow: true, loadInvoiceAccountRow: true);


                    CustomerInvoice clone = InvoiceManager.CopyCustomerInvoice(entities, customerInvoice, originType, false);

                    #region Origin

                    clone.Origin.OriginId = 0;
                    clone.Origin.Status = customerInvoice.Origin.Status;

                    if (voucherSeries != null)
                    {
                        VoucherSeries voucherSerie = VoucherManager.GetVoucherSerie(clone.Origin.VoucherSeriesId, templateCompanyId, true);
                        if (voucherSeries != null)
                        {
                            VoucherSeries newVoucherSeries = voucherSeries.FirstOrDefault(v => v.VoucherSeriesType.Name.Trim().ToLower().Equals(voucherSerie.VoucherSeriesType.Name.Trim().ToLower()));
                            if (newVoucherSeries != null)
                            {
                                clone.Origin.VoucherSeriesId = newVoucherSeries.VoucherSeriesId;
                                clone.Origin.VoucherSeriesTypeId = newVoucherSeries.VoucherSeriesTypeId;
                            }
                            else
                            {
                                VoucherSeriesType voucherSeriesType = voucherSeriesTypes.FirstOrDefault(v => v.Name == voucherSerie.VoucherSeriesType.Name);
                                if (voucherSeriesType != null)
                                {
                                    newVoucherSeries = new VoucherSeries()
                                    {
                                        VoucherSeriesTypeId = voucherSeriesType.VoucherSeriesTypeId,
                                        AccountYearId = currentAccountYear.AccountYearId,
                                        VoucherNrLatest = voucherSeriesType.StartNr,
                                        VoucherDateLatest = currentAccountYear.From,
                                    };

                                    result = VoucherManager.AddVoucherSeries(entities, newVoucherSeries, newCompanyId, currentAccountYear.AccountYearId, voucherSeriesType.VoucherSeriesTypeId);
                                    if (result.Success)
                                    {
                                        clone.Origin.VoucherSeriesId = newVoucherSeries.VoucherSeriesId;
                                        clone.Origin.VoucherSeriesTypeId = newVoucherSeries.VoucherSeriesTypeId;
                                        voucherSeries.Add(newVoucherSeries);
                                    }
                                }
                                else
                                    continue;
                            }
                        }
                        else
                            continue;
                    }

                    #endregion

                    #region Invoice

                    // Reset values
                    clone.Origin.ActorCompanyId = newCompanyId;
                    clone.VoucherHead = null;
                    clone.VoucherHead2 = null;
                    clone.VoucherHead2Id = null;
                    clone.ProjectId = null;
                    clone.InvoiceDate = customerInvoice.InvoiceDate;
                    clone.DueDate = customerInvoice.DueDate;
                    clone.VoucherDate = customerInvoice.VoucherDate;
                    clone.FullyPayed = customerInvoice.FullyPayed;
                    clone.PaymentNr = customerInvoice.PaymentNr;
                    clone.ManuallyAdjustedAccounting = customerInvoice.ManuallyAdjustedAccounting;
                    clone.BillingAddressId = 0;
                    clone.DeliveryAddressId = 0;
                    clone.ContactEComId = null;

                    #region Contact

                    if (customerInvoice.ContactEComId.HasValue)
                    {
                        var templateEcom = ContactManager.GetContactECom(customerInvoice.ContactEComId.Value, false);
                        var newEcom = entities.ContactEcomView.FirstOrDefault(v => v.ActorCompanyId == newCompanyId && v.Text.ToLower().Equals(templateEcom.Text.ToLower()) && v.SysContactEComTypeId == templateEcom.SysContactEComTypeId);

                        if (newEcom != null)
                            clone.ContactEComId = newEcom.ContactEComId;
                    }

                    if (customerInvoice.BillingAddressId != 0)
                    {
                        var templateContactAddress = ContactManager.GetContactAddress(customerInvoice.BillingAddressId, false, true);
                        if (templateContactAddress != null && templateContactAddress.Name != null)
                        {
                            var newDeliveryContactAddresses = newContactAddresses.Where(v => v.Name != null && v.Name.ToLower().Equals(templateContactAddress.Name.ToLower()) && v.SysContactAddressTypeId == templateContactAddress.SysContactAddressTypeId).ToList();

                            newDeliveryContactAddresses = newDeliveryContactAddresses.Where(n => n.ContactAddressRow != null && n.ContactAddressRow.Count == templateContactAddress.ContactAddressRow.Count).ToList();

                            StringBuilder templateAddressString = new StringBuilder();
                            foreach (var row in templateContactAddress.ContactAddressRow.OrderBy(a => a.SysContactAddressRowTypeId))
                                templateAddressString.Append(row.Text);

                            foreach (var address in newDeliveryContactAddresses)
                            {
                                StringBuilder newAddressString = new StringBuilder();
                                foreach (var row in address.ContactAddressRow.OrderBy(a => a.SysContactAddressRowTypeId))
                                    newAddressString.Append(row.Text);

                                if (!string.IsNullOrWhiteSpace(newAddressString.ToString()) && newAddressString.ToString().ToLower().Trim().Equals(templateAddressString.ToString().ToLower().Trim()))
                                    clone.BillingAddressId = address.ContactAddressId;

                            }
                        }

                    }

                    if (customerInvoice.DeliveryAddressId != 0)
                    {
                        var templateContactAddress = ContactManager.GetContactAddress(customerInvoice.DeliveryAddressId, false, true);

                        if (templateContactAddress != null && templateContactAddress.Name != null)
                        {
                            var newDeliveryContactAddresses = newContactAddresses.Where(v => v.Name != null && v.Name.ToLower().Equals(templateContactAddress.Name.ToLower()) && v.SysContactAddressTypeId == templateContactAddress.SysContactAddressTypeId).ToList();

                            newDeliveryContactAddresses = newDeliveryContactAddresses.Where(n => n.ContactAddressRow != null && n.ContactAddressRow.Count == templateContactAddress.ContactAddressRow.Count).ToList();

                            StringBuilder templateAddressString = new StringBuilder();
                            foreach (var row in templateContactAddress.ContactAddressRow.OrderBy(a => a.SysContactAddressRowTypeId))
                                templateAddressString.Append(row.Text);

                            foreach (var address in newDeliveryContactAddresses)
                            {
                                StringBuilder newAddressString = new StringBuilder();
                                foreach (var row in address.ContactAddressRow.OrderBy(a => a.SysContactAddressRowTypeId))
                                    newAddressString.Append(row.Text);

                                if (!string.IsNullOrWhiteSpace(newAddressString.ToString()) && newAddressString.ToString().ToLower().Trim().Equals(templateAddressString.ToString().ToLower().Trim()))
                                    clone.DeliveryAddressId = address.ContactAddressId;
                            }
                        }
                    }


                    #endregion

                    // Customer
                    if (customerInvoice.ActorId != null && !customerInvoice.ActorReference.IsLoaded)
                        customerInvoice.ActorReference.Load();

                    if (customerInvoice.Actor != null && customerInvoice.Actor.ActorType == (int)SoeActorType.Customer)
                    {
                        if (!customerInvoice.Actor.CustomerReference.IsLoaded)
                            customerInvoice.Actor.CustomerReference.Load();

                        if (customerInvoice.Actor.Customer != null)
                        {
                            // Customer
                            Customer customer = customers.FirstOrDefault(c => c.CustomerNr == customerInvoice.Actor.Customer.CustomerNr);
                            if (customer != null)
                                clone.ActorId = customer.ActorCustomerId;
                            else
                                clone.ActorId = null;
                        }
                    }

                    #region Currency

                    Currency invoiceCurrency = currencies.FirstOrDefault(c => c.SysCurrencyId == customerInvoice.Currency.SysCurrencyId);
                    if (invoiceCurrency != null)
                        clone.CurrencyId = invoiceCurrency.CurrencyId;

                    decimal rate = CountryCurrencyManager.GetCurrencyRate(newCompanyId, invoiceCurrency.SysCurrencyId, DateTime.Now, true);

                    clone.CurrencyRate = rate;
                    clone.CurrencyDate = DateTime.Now;

                    #endregion

                    #region Amounts

                    clone.TotalAmount = customerInvoice.TotalAmount;
                    clone.TotalAmountCurrency = customerInvoice.TotalAmountCurrency;
                    clone.TotalAmountEntCurrency = customerInvoice.TotalAmountEntCurrency;
                    clone.TotalAmountLedgerCurrency = customerInvoice.TotalAmountLedgerCurrency;
                    clone.VATAmount = customerInvoice.VATAmount;
                    clone.PaidAmount = customerInvoice.PaidAmount;
                    clone.PaidAmountCurrency = customerInvoice.PaidAmountCurrency;
                    clone.PaidAmountEntCurrency = customerInvoice.PaidAmountEntCurrency;
                    clone.PaidAmountLedgerCurrency = customerInvoice.PaidAmountLedgerCurrency;
                    clone.RemainingAmount = customerInvoice.RemainingAmount;
                    clone.RemainingAmountExVat = customerInvoice.RemainingAmountExVat;
                    clone.RemainingAmountVat = customerInvoice.RemainingAmountVat;
                    clone.VATAmount = customerInvoice.VATAmount;
                    clone.VATAmountCurrency = customerInvoice.VATAmountCurrency;
                    clone.VATAmountEntCurrency = customerInvoice.VATAmountEntCurrency;
                    clone.VATAmountLedgerCurrency = customerInvoice.VATAmountLedgerCurrency;

                    #endregion

                    #endregion

                    #region CustomerInvoice

                    clone.OriginateFrom = customerInvoice.OriginateFrom;
                    clone.DeliveryAddressId = customerInvoice.DeliveryAddressId;
                    clone.BillingAddressId = customerInvoice.BillingAddressId;
                    clone.OrderDate = customerInvoice.OrderDate;
                    clone.DeliveryDate = customerInvoice.DeliveryDate;
                    clone.MarginalIncomeRatio = customerInvoice.MarginalIncomeRatio;
                    clone.NoOfReminders = customerInvoice.NoOfReminders;
                    clone.HasHouseholdTaxDeduction = customerInvoice.HasHouseholdTaxDeduction;
                    clone.BillingInvoicePrinted = customerInvoice.BillingInvoicePrinted;
                    clone.FixedPriceOrder = customerInvoice.FixedPriceOrder;
                    clone.MultipleAssetRows = customerInvoice.MultipleAssetRows;
                    clone.ContractGroupId = customerInvoice.ContractGroupId;
                    clone.NextContractPeriodYear = customerInvoice.NextContractPeriodYear;
                    clone.NextContractPeriodValue = customerInvoice.NextContractPeriodValue;
                    clone.InsecureDebt = customerInvoice.InsecureDebt;
                    clone.ShiftTypeId = customerInvoice.ShiftTypeId;
                    clone.PlannedStartDate = customerInvoice.PlannedStartDate;
                    clone.PlannedStopDate = customerInvoice.PlannedStopDate;
                    clone.EstimatedTime = customerInvoice.EstimatedTime;
                    clone.RemainingTime = customerInvoice.RemainingTime;
                    clone.Priority = customerInvoice.Priority;
                    clone.KeepAsPlanned = customerInvoice.KeepAsPlanned;
                    clone.OrderNumbers = customerInvoice.OrderNumbers;


                    #region Amounts

                    clone.FreightAmount = customerInvoice.FreightAmount;
                    clone.FreightAmountCurrency = customerInvoice.FreightAmountCurrency;
                    clone.FreightAmountEntCurrency = customerInvoice.FreightAmountEntCurrency;
                    clone.FreightAmountLedgerCurrency = customerInvoice.FreightAmountLedgerCurrency;
                    clone.InvoiceFee = customerInvoice.InvoiceFee;
                    clone.InvoiceFeeCurrency = customerInvoice.InvoiceFeeCurrency;
                    clone.InvoiceFeeEntCurrency = customerInvoice.InvoiceFeeEntCurrency;
                    clone.InvoiceFeeLedgerCurrency = customerInvoice.InvoiceFeeLedgerCurrency;
                    clone.CentRounding = customerInvoice.CentRounding;
                    clone.SumAmount = customerInvoice.SumAmount;
                    clone.SumAmountCurrency = customerInvoice.SumAmountCurrency;
                    clone.SumAmountEntCurrency = customerInvoice.SumAmountEntCurrency;
                    clone.SumAmountLedgerCurrency = customerInvoice.SumAmountLedgerCurrency;
                    clone.MarginalIncome = customerInvoice.MarginalIncome;
                    clone.MarginalIncomeCurrency = customerInvoice.MarginalIncomeCurrency;
                    clone.MarginalIncomeEntCurrency = customerInvoice.MarginalIncomeEntCurrency;
                    clone.MarginalIncomeLedgerCurrency = customerInvoice.MarginalIncomeLedgerCurrency;


                    #endregion

                    #region Accounts

                    if (customerInvoice.DefaultDim1AccountId.HasValue && customerInvoice.DefaultDim1AccountId != 0)
                    {
                        AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
                        if (accountDim != null)
                        {
                            Account account = templateAccounts.FirstOrDefault(p => p.AccountId == customerInvoice.DefaultDim1AccountId.Value);
                            if (account != null && newAccounts != null)
                            {
                                Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(account.AccountNr.Trim().ToLower()));
                                if (newAccount != null)
                                    customerInvoice.DefaultDim1AccountId = newAccount.AccountId;
                                else
                                    customerInvoice.DefaultDim1AccountId = null;
                            }
                        }
                    }

                    if (customerInvoice.DefaultDim2AccountId.HasValue && customerInvoice.DefaultDim2AccountId != 0)
                    {
                        var internalAccount = templateAccounts.FirstOrDefault(a => a.AccountId == customerInvoice.DefaultDim2AccountId.Value);
                        if (internalAccount != null)
                        {
                            foreach (var dim in templateAccountDims)
                            {
                                if (internalAccount.AccountDimId == dim.AccountDimId)
                                {
                                    AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == dim.AccountDimNr);
                                    if (accountDim != null && newAccounts != null)
                                    {
                                        Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(internalAccount.AccountNr.Trim().ToLower()));
                                        if (newAccount != null)
                                            customerInvoice.DefaultDim1AccountId = newAccount.AccountId;
                                        else
                                            customerInvoice.DefaultDim1AccountId = null;
                                    }
                                }
                            }
                        }
                    }

                    if (customerInvoice.DefaultDim3AccountId.HasValue && customerInvoice.DefaultDim3AccountId != 0)
                    {
                        var internalAccount = templateAccounts.FirstOrDefault(a => a.AccountId == customerInvoice.DefaultDim3AccountId.Value);
                        if (internalAccount != null)
                        {
                            foreach (var dim in templateAccountDims)
                            {
                                if (internalAccount.AccountDimId == dim.AccountDimId)
                                {
                                    AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == dim.AccountDimNr);
                                    if (accountDim != null && newAccounts != null)
                                    {
                                        Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(internalAccount.AccountNr.Trim().ToLower()));
                                        if (newAccount != null)
                                            customerInvoice.DefaultDim1AccountId = newAccount.AccountId;
                                        else
                                            customerInvoice.DefaultDim1AccountId = null;
                                    }
                                }
                            }
                        }
                    }

                    if (customerInvoice.DefaultDim4AccountId.HasValue && customerInvoice.DefaultDim4AccountId != 0)
                    {
                        var internalAccount = templateAccounts.FirstOrDefault(a => a.AccountId == customerInvoice.DefaultDim4AccountId.Value);
                        if (internalAccount != null)
                        {
                            foreach (var dim in templateAccountDims)
                            {
                                if (internalAccount.AccountDimId == dim.AccountDimId)
                                {
                                    AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == dim.AccountDimNr);
                                    if (accountDim != null && newAccounts != null)
                                    {
                                        Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(internalAccount.AccountNr.Trim().ToLower()));
                                        if (newAccount != null)
                                            customerInvoice.DefaultDim1AccountId = newAccount.AccountId;
                                        else
                                            customerInvoice.DefaultDim1AccountId = null;
                                    }
                                }
                            }
                        }
                    }

                    if (customerInvoice.DefaultDim5AccountId.HasValue && customerInvoice.DefaultDim5AccountId != 0)
                    {
                        var internalAccount = templateAccounts.FirstOrDefault(a => a.AccountId == customerInvoice.DefaultDim5AccountId.Value);
                        if (internalAccount != null)
                        {
                            foreach (var dim in templateAccountDims)
                            {
                                if (internalAccount.AccountDimId == dim.AccountDimId)
                                {
                                    AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == dim.AccountDimNr);
                                    if (accountDim != null && newAccounts != null)
                                    {
                                        Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(internalAccount.AccountNr.Trim().ToLower()));
                                        if (newAccount != null)
                                            customerInvoice.DefaultDim1AccountId = newAccount.AccountId;
                                        else
                                            customerInvoice.DefaultDim1AccountId = null;
                                    }
                                }
                            }
                        }
                    }

                    if (customerInvoice.DefaultDim6AccountId.HasValue && customerInvoice.DefaultDim6AccountId != 0)
                    {
                        var internalAccount = templateAccounts.FirstOrDefault(a => a.AccountId == customerInvoice.DefaultDim6AccountId.Value);
                        if (internalAccount != null)
                        {
                            foreach (var dim in templateAccountDims)
                            {
                                if (internalAccount.AccountDimId == dim.AccountDimId)
                                {
                                    AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == dim.AccountDimNr);
                                    if (accountDim != null && newAccounts != null)
                                    {
                                        Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(internalAccount.AccountNr.Trim().ToLower()));
                                        if (newAccount != null)
                                            customerInvoice.DefaultDim1AccountId = newAccount.AccountId;
                                        else
                                            customerInvoice.DefaultDim1AccountId = null;
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region PaymentCondition

                    if (templatePaymentConditions != null && customerInvoice.PaymentConditionId != null)
                    {
                        PaymentCondition paymentCondition = templatePaymentConditions.FirstOrDefault(p => p.PaymentConditionId == customerInvoice.PaymentConditionId);
                        if (paymentCondition != null && newPaymentConditions != null)
                        {
                            PaymentCondition newPaymentCondition = newPaymentConditions.FirstOrDefault(p => p.Code == paymentCondition.Code);
                            if (newPaymentCondition != null)
                                clone.PaymentConditionId = newPaymentCondition.PaymentConditionId;
                        }
                    }

                    #endregion

                    #region DeliveryTypes

                    if (templateDeliveryTypes != null && customerInvoice.DeliveryTypeId != null)
                    {
                        DeliveryType deliveryType = templateDeliveryTypes.FirstOrDefault(p => p.DeliveryTypeId == customerInvoice.DeliveryTypeId);
                        if (deliveryType != null && newDeliveryTypes != null)
                        {
                            DeliveryType newDeliveryType = newDeliveryTypes.FirstOrDefault(p => p.Code == deliveryType.Code);
                            if (newDeliveryType != null)
                                clone.DeliveryTypeId = newDeliveryType.DeliveryTypeId;
                        }
                    }

                    #endregion

                    #region DeliveryConditions

                    if (templateDeliveryConditions != null && customerInvoice.DeliveryConditionId != null)
                    {
                        DeliveryCondition deliveryCondition = templateDeliveryConditions.FirstOrDefault(p => p.DeliveryConditionId == customerInvoice.DeliveryConditionId);
                        if (deliveryCondition != null && newDeliveryConditions != null)
                        {
                            DeliveryCondition newDeliveryCondition = newDeliveryConditions.FirstOrDefault(p => p.Code == deliveryCondition.Code);
                            if (newDeliveryCondition != null)
                                clone.DeliveryConditionId = newDeliveryCondition.DeliveryConditionId;
                        }
                    }

                    #endregion

                    #region PriceListTypes

                    if (templatePriceListTypes != null && customerInvoice.PriceListTypeId != null)
                    {
                        PriceListType priceListType = templatePriceListTypes.FirstOrDefault(p => p.PriceListTypeId == customerInvoice.PriceListTypeId);
                        if (priceListType != null && newPriceListTypes != null)
                        {
                            PriceListType newPriceListType = newPriceListTypes.FirstOrDefault(p => p.Name == priceListType.Name);
                            if (newPriceListType != null)
                                clone.PriceListTypeId = newPriceListType.PriceListTypeId;
                        }
                    }

                    #endregion

                    #region Projects

                    if (templateProjects != null && customerInvoice.ProjectId != null)
                    {
                        Project project = templateProjects.FirstOrDefault(p => p.ProjectId == customerInvoice.ProjectId);
                        if (project != null && newProjects != null)
                        {
                            Project newProject = newProjects.FirstOrDefault(p => p.Number == project.Number);
                            if (newProject != null)
                                clone.ProjectId = newProject.ProjectId;
                        }
                    }

                    #endregion

                    #region ContractGroups

                    if (templateContractGroups != null && customerInvoice.ContractGroupId != null)
                    {
                        ContractGroup contractGroup = templateContractGroups.FirstOrDefault(p => p.ContractGroupId == customerInvoice.ContractGroupId);
                        if (contractGroup != null && newContractGroups != null)
                        {
                            ContractGroup newContractGroup = newContractGroups.FirstOrDefault(p => p.Name == contractGroup.Name);
                            if (newContractGroup != null)
                                clone.ContractGroupId = newContractGroup.ContractGroupId;
                        }
                    }

                    #endregion

                    #endregion

                    result = SaveChanges(entities);

                    if (!result.Success)
                        return result;

                    #region Rows

                    if (customerInvoice.CustomerInvoiceRow != null)
                    {
                        foreach (var customerInvoiceRow in customerInvoice.CustomerInvoiceRow.Where(r => r.State == (int)SoeEntityState.Active).ToList())
                        {
                            CustomerInvoiceRow cloneRow = InvoiceManager.CopyCustomerInvoiceRow(entities, customerInvoiceRow);
                            cloneRow.InvoiceId = clone.InvoiceId;
                            cloneRow.ParentRowId = null;
                            cloneRow.TargetRowId = null;

                            #region AttestStates

                            if (templateAttestStates != null && customerInvoiceRow.AttestStateId != null)
                            {
                                AttestState attestState = templateAttestStates.FirstOrDefault(p => p.AttestStateId == customerInvoiceRow.AttestStateId);
                                if (attestState != null && newAttestStates != null)
                                {
                                    AttestState newAttestState = newAttestStates.FirstOrDefault(p => p.Name.Trim().ToLower().Equals(attestState.Name.Trim().ToLower()));
                                    if (newAttestState != null)
                                        cloneRow.AttestStateId = newAttestState.AttestStateId;
                                    else
                                        cloneRow.AttestStateId = null;
                                }
                            }

                            #endregion

                            #region Products

                            if (templateInvoiceProducts != null && customerInvoiceRow.ProductId != null)
                            {
                                Product product = templateInvoiceProducts.FirstOrDefault(p => p.ProductId == customerInvoiceRow.ProductId);
                                if (product != null && newInvoiceProducts != null)
                                {
                                    Product newProduct = newInvoiceProducts.FirstOrDefault(p => p.Number.ToLower().Equals(product.Number.ToLower()));
                                    if (newProduct != null)
                                        cloneRow.ProductId = newProduct.ProductId;
                                    else
                                        cloneRow.ProductId = null;
                                }
                            }

                            #endregion

                            #region ProductUnits

                            if (templateProductUnits != null && cloneRow.ProductUnitId != null)
                            {
                                ProductUnit productUnit = templateProductUnits.FirstOrDefault(p => p.ProductUnitId == (int)customerInvoiceRow.ProductUnitId);
                                if (productUnit != null && newProductUnits != null)
                                {
                                    ProductUnit newProductUnit = newProductUnits.FirstOrDefault(p => p.Code.ToLower().Equals(productUnit.Code.ToLower()));
                                    if (newProductUnit != null)
                                        cloneRow.ProductUnitId = newProductUnit.ProductUnitId;
                                    else
                                        cloneRow.ProductUnitId = null;
                                }
                            }

                            #endregion

                            #region AccountVat

                            if (templateAccounts != null && cloneRow.VatAccountId != null)
                            {
                                AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
                                if (accountDim != null)
                                {
                                    Account account = templateAccounts.FirstOrDefault(p => p.AccountId == (int)customerInvoiceRow.VatAccountId);
                                    if (account != null && newAccounts != null)
                                    {
                                        Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(account.AccountNr.Trim().ToLower()));
                                        if (newAccount != null)
                                            cloneRow.VatAccountId = newAccount.AccountId;
                                        else
                                            cloneRow.VatAccountId = null;
                                    }
                                }
                            }

                            #endregion

                            #region VatCodes

                            if (templateVatCodes != null && cloneRow.VatCodeId != null)
                            {
                                VatCode vatCode = templateVatCodes.FirstOrDefault(p => p.VatCodeId == (int)customerInvoiceRow.VatCodeId);
                                if (vatCode != null && newVatCodes != null)
                                {
                                    VatCode newVatCode = newVatCodes.FirstOrDefault(p => p.Code.ToLower().Equals(vatCode.Code.ToLower()));
                                    if (newVatCode != null)
                                        cloneRow.VatCodeId = newVatCode.VatCodeId;
                                    else
                                        cloneRow.VatCodeId = null;
                                }
                            }

                            #endregion

                            cloneRow.CustomerInvoiceInterestId = null;
                            cloneRow.CustomerInvoiceReminderId = null;
                            cloneRow.EdiEntryId = null;
                            cloneRow.RowNr = customerInvoiceRow.RowNr;
                            cloneRow.StockId = null;
                            cloneRow.SupplierInvoiceId = null; //TODO
                            cloneRow.HouseholdDeductionType = customerInvoiceRow.HouseholdDeductionType;
                            cloneRow.DateTo = customerInvoiceRow.DateTo;
                            cloneRow.DeliveryDateText = customerInvoiceRow.DeliveryDateText;

                            if (customerInvoiceRow.CustomerInvoiceAccountRow != null)
                            {
                                foreach (var customerInvoiceAccountRow in customerInvoiceRow.CustomerInvoiceAccountRow.ToList())
                                {
                                    CustomerInvoiceAccountRow cloneAccountRow = new CustomerInvoiceAccountRow()
                                    {
                                        RowNr = customerInvoiceAccountRow.RowNr,
                                        Amount = customerInvoiceAccountRow.Amount,
                                        AmountCurrency = customerInvoiceAccountRow.AmountCurrency,
                                        AmountEntCurrency = customerInvoiceAccountRow.AmountEntCurrency,
                                        AmountLedgerCurrency = customerInvoiceAccountRow.AmountLedgerCurrency,
                                        CreditRow = customerInvoiceAccountRow.CreditRow,
                                        DebitRow = customerInvoiceAccountRow.DebitRow,
                                        ContractorVatRow = customerInvoiceAccountRow.ContractorVatRow,
                                        Quantity = customerInvoiceAccountRow.Quantity,
                                        Text = customerInvoiceAccountRow.Text,
                                        SplitPercent = customerInvoiceAccountRow.SplitPercent,
                                        SplitType = customerInvoiceAccountRow.SplitType,
                                        VatRow = customerInvoiceAccountRow.VatRow,
                                        State = customerInvoiceAccountRow.State,
                                    };

                                    #region Account

                                    if (templateAccounts != null)
                                    {
                                        AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
                                        if (accountDim != null)
                                        {
                                            Account account = templateAccounts.FirstOrDefault(p => p.AccountId == customerInvoiceAccountRow.AccountId);
                                            if (account != null && newAccounts != null)
                                            {
                                                Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(account.AccountNr.Trim().ToLower()));
                                                if (newAccount != null)
                                                    cloneAccountRow.AccountId = newAccount.AccountId;
                                                else
                                                    continue;
                                            }
                                        }
                                    }

                                    #endregion

                                    cloneAccountRow.VoucherRowId = null;

                                    try
                                    {
                                        if (customerInvoiceAccountRow.AccountInternal != null)
                                        {
                                            cloneAccountRow.AccountInternal.Clear();

                                            foreach (var accountInternal in customerInvoiceAccountRow.AccountInternal.ToList())
                                            {
                                                var internalAccount = templateAccounts.FirstOrDefault(a => a.AccountId == accountInternal.AccountId);
                                                if (internalAccount != null)
                                                {
                                                    foreach (var dim in templateAccountDims)
                                                    {
                                                        if (internalAccount.AccountDimId == dim.AccountDimId)
                                                        {
                                                            AccountDim accountDim = newAccountDims.FirstOrDefault(a => a.AccountDimNr == dim.AccountDimNr);
                                                            if (accountDim != null && newAccounts != null)
                                                            {
                                                                Account newAccount = newAccounts.FirstOrDefault(p => p.AccountDimId == accountDim.AccountDimId && p.AccountNr.Trim().ToLower().Equals(internalAccount.AccountNr.Trim().ToLower()));
                                                                if (newAccount != null)
                                                                {
                                                                    var accInternal = AccountManager.GetAccount(entities, newCompanyId, newAccount.AccountId, false, loadAccount: true);
                                                                    if (accInternal.AccountInternal != null && (cloneAccountRow.AccountInternal == null || !cloneAccountRow.AccountInternal.Any(i => i.AccountId == newAccount.AccountId)))
                                                                        cloneAccountRow.AccountInternal.Add(accInternal.AccountInternal);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }

                                    if (cloneRow.CustomerInvoiceAccountRow == null)
                                    {
                                        cloneRow.CustomerInvoiceAccountRow = new EntityCollection<CustomerInvoiceAccountRow>();
                                    }
                                    cloneRow.CustomerInvoiceAccountRow.Add(cloneAccountRow);
                                }
                            }
                            if (clone.CustomerInvoiceRow == null)
                            {
                                clone.CustomerInvoiceRow = new EntityCollection<CustomerInvoiceRow>();
                            }
                            clone.CustomerInvoiceRow.Add(cloneRow);
                        }
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        return result;

                    #endregion

                    #region Category

                    if (templateContractCategoryRecords != null && templateContractCategories != null)
                    {
                        var records = templateContractCategoryRecords.Where(r => r.RecordId == customerInvoice.InvoiceId).ToList();
                        if (records != null)
                        {
                            foreach (var record in records)
                            {
                                var templateCategory = templateContractCategories.FirstOrDefault(c => c.CategoryId == record.CategoryId);
                                if (templateCategory != null)
                                {
                                    var newCategory = newContactCategories.FirstOrDefault(c => c.Code.Trim().ToLower().Equals(templateCategory.Code.Trim().ToLower()));

                                    CompanyCategoryRecord categoryRecord = new CompanyCategoryRecord()
                                    {
                                        ActorCompanyId = newCompanyId,
                                        RecordId = clone.InvoiceId,
                                        Entity = (int)SoeCategoryRecordEntity.Contract,
                                        DateFrom = null,
                                        DateTo = null,
                                    };

                                    if (newCategory != null)
                                    {
                                        categoryRecord.CategoryId = newCategory.CategoryId;
                                    }
                                    else
                                    {
                                        Category category = new Category()
                                        {
                                            ActorCompanyId = newCompanyId,
                                            ParentId = null, //TODO
                                            Type = templateCategory.Type,
                                            Code = templateCategory.Code,
                                            Name = templateCategory.Name,
                                            State = templateCategory.State,
                                            CategoryGroupId = null  //TODO 
                                        };

                                        SetCreatedProperties(category);

                                        entities.Category.AddObject(category);

                                        if (SaveChanges(entities).Success)
                                        {
                                            categoryRecord.CategoryId = category.CategoryId;
                                            newContactCategories.Add(category);
                                        }
                                    }
                                }
                            }
                        }
                    }


                    #endregion

                    result = SaveChanges(entities);
                    if (!result.Success)
                        return result;


                    #region Checklist Records

                    List<ChecklistHeadRecord> headRecords = ChecklistManager.GetChecklistHeadRecords(SoeEntityType.Order, customerInvoice.InvoiceId, templateCompanyId, true);

                    if (headRecords != null)
                    {
                        //ChecklistHead newHead = newChecklistHeads.Where(h => h.Type == 

                        foreach (ChecklistHeadRecord headRecord in headRecords)
                        {
                            if (!headRecord.ChecklistRowRecord.IsLoaded)
                                headRecord.ChecklistRowRecord.Load();

                            ChecklistHead newHead = newChecklistHeads.FirstOrDefault(h => h.Type == headRecord.ChecklistHead.Type && h.Name == headRecord.ChecklistHead.Name);
                            if (newHead != null)
                            {
                                ChecklistHeadRecord newHeadRecord = new ChecklistHeadRecord()
                                {
                                    ChecklistHeadId = newHead.ChecklistHeadId,
                                    ActorCompanyId = newCompanyId,
                                    Entity = (int)SoeEntityType.Order,
                                    RecordId = clone.InvoiceId,
                                };

                                SetCreatedProperties(newHeadRecord);
                                entities.ChecklistHeadRecord.AddObject(newHeadRecord);

                                if (!result.Success)
                                    return result;

                                foreach (ChecklistRowRecord rowRecord in headRecord.ChecklistRowRecord)
                                {
                                    if (!rowRecord.ChecklistRowReference.IsLoaded)
                                        rowRecord.ChecklistRowReference.Load();

                                    ChecklistRow row = newHead.ChecklistRow.FirstOrDefault(r => r.Type == rowRecord.ChecklistRow.Type && r.RowNr == rowRecord.ChecklistRow.RowNr);
                                    if (row != null)
                                    {
                                        ChecklistRowRecord newRowRecord = new ChecklistRowRecord()
                                        {
                                            ChecklistHeadRecordId = newHeadRecord.ChecklistHeadRecordId,
                                            ChecklistRowId = row.ChecklistRowId,
                                            ActorCompanyId = newCompanyId,
                                            Date = rowRecord.Date,
                                            Text = rowRecord.Text,
                                            Type = rowRecord.Type,
                                            Comment = rowRecord.Comment,
                                            DataTypeId = rowRecord.DataTypeId,
                                            StrData = rowRecord.StrData,
                                            IntData = rowRecord.IntData,
                                            BoolData = rowRecord.BoolData,
                                            DateData = rowRecord.DateData,
                                            DecimalData = rowRecord.DecimalData,
                                        };

                                        SetCreatedProperties(newRowRecord);
                                        entities.ChecklistRowRecord.AddObject(newRowRecord);
                                    }
                                }

                                result = SaveChanges(entities);
                            }
                        }
                    }

                    #endregion
                }

                result = SaveChanges(entities);
            }


            return result;
        }

        public ActionResult CopyOrderTemplatesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, Dictionary<int, Report> reportMapping)
        {
            var result = new ActionResult();

            #region Prereq

            // Copy Checklists
            CopyChecklistsFromTemplateCompany(newCompanyId, templateCompanyId, userId, update, reportMapping);

            // Get Currencies
            List<Currency> currencies = CountryCurrencyManager.GetCurrencies(newCompanyId);

            // Get Invoice Templates
            Dictionary<int, string> invoiceTemplates = InvoiceManager.GetInvoiceTemplatesDict(templateCompanyId, SoeOriginType.Order, SoeInvoiceType.CustomerInvoice);
            Dictionary<int, string> newInvoiceTemplates = InvoiceManager.GetInvoiceTemplatesDict(newCompanyId, SoeOriginType.Order, SoeInvoiceType.CustomerInvoice);

            // Get Customers
            List<Customer> customers = CustomerManager.GetCustomersByCompany(newCompanyId, false);

            // Get open account year
            AccountYear currentAccountYear = AccountManager.GetCurrentAccountYear(newCompanyId, false);

            // Get voucher series types
            List<VoucherSeriesType> voucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(newCompanyId, false);

            // Get voucher series
            List<VoucherSeries> voucherSeries = null;
            if (currentAccountYear != null)
                voucherSeries = VoucherManager.GetVoucherSeriesByYear(currentAccountYear.AccountYearId, newCompanyId, false);

            // Get PaymentConditions
            List<PaymentCondition> templatePaymentConditions = PaymentManager.GetPaymentConditions(templateCompanyId);
            List<PaymentCondition> newPaymentConditions = PaymentManager.GetPaymentConditions(newCompanyId);

            // Get DeliveryTypes
            List<DeliveryType> templateDeliveryTypes = InvoiceManager.GetDeliveryTypes(templateCompanyId);
            List<DeliveryType> newDeliveryTypes = InvoiceManager.GetDeliveryTypes(newCompanyId);

            // Get DeliverConditions
            List<DeliveryCondition> templateDeliveryConditions = InvoiceManager.GetDeliveryConditions(templateCompanyId);
            List<DeliveryCondition> newDeliveryConditions = InvoiceManager.GetDeliveryConditions(newCompanyId);

            // Get PriceListTypes
            List<PriceListType> templatePriceListTypes = ProductPricelistManager.GetPriceListTypes(templateCompanyId);
            List<PriceListType> newPriceListTypes = ProductPricelistManager.GetPriceListTypes(newCompanyId);

            // Get Checklists            
            List<ChecklistHead> newChecklistHeads = ChecklistManager.GetChecklistHeadsIncludingRows(newCompanyId);

            #endregion

            using (var entities = new CompEntities())
            {
                foreach (KeyValuePair<int, string> kvp in invoiceTemplates)
                {
                    if (newInvoiceTemplates.Values.Contains(kvp.Value))
                        continue;

                    CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, kvp.Key, true, false, true, true, true, false, false, false, false, false, false);

                    if (customerInvoice == null)
                        continue;

                    CustomerInvoice clone = InvoiceManager.CopyCustomerInvoice(entities, customerInvoice, SoeOriginType.Order, false);

                    #region Origin

                    clone.Origin.OriginId = 0;

                    if (voucherSeries != null)
                    {
                        VoucherSeries voucherSerie = VoucherManager.GetVoucherSerie(clone.Origin.VoucherSeriesId, templateCompanyId, true);
                        if (voucherSeries != null)
                        {
                            VoucherSeries newVoucherSeries = voucherSeries.FirstOrDefault(v => v.VoucherSeriesType.Name == voucherSerie.VoucherSeriesType.Name);
                            if (newVoucherSeries != null)
                            {
                                clone.Origin.VoucherSeriesId = newVoucherSeries.VoucherSeriesId;
                                clone.Origin.VoucherSeriesTypeId = newVoucherSeries.VoucherSeriesTypeId;
                            }
                            else
                            {
                                VoucherSeriesType voucherSeriesType = voucherSeriesTypes.FirstOrDefault(v => v.Name == voucherSerie.VoucherSeriesType.Name);
                                if (voucherSeriesType != null)
                                {
                                    newVoucherSeries = new VoucherSeries()
                                    {
                                        VoucherSeriesTypeId = voucherSeriesType.VoucherSeriesTypeId,
                                        AccountYearId = currentAccountYear.AccountYearId,
                                        VoucherNrLatest = voucherSeriesType.StartNr,
                                        VoucherDateLatest = currentAccountYear.From,
                                    };

                                    result = VoucherManager.AddVoucherSeries(entities, newVoucherSeries, newCompanyId, currentAccountYear.AccountYearId, voucherSeriesType.VoucherSeriesTypeId);

                                    if (result.Success)
                                    {
                                        clone.Origin.VoucherSeriesId = newVoucherSeries.VoucherSeriesId;
                                        clone.Origin.VoucherSeriesTypeId = newVoucherSeries.VoucherSeriesTypeId;
                                        voucherSeries.Add(newVoucherSeries);
                                    }
                                }
                                else
                                    continue;
                            }
                        }
                        else
                            continue;
                    }

                    #endregion

                    #region Invoice

                    // Reset values
                    clone.Origin.ActorCompanyId = newCompanyId;
                    clone.VoucherHead = null;
                    clone.VoucherHead2 = null;
                    clone.VoucherHead2Id = null;
                    clone.ProjectId = null;
                    clone.InvoiceDate = null;
                    clone.DueDate = null;
                    clone.VoucherDate = null;
                    clone.FullyPayed = false;
                    clone.PaymentNr = null;
                    clone.ManuallyAdjustedAccounting = false;

                    // Customer
                    if (customerInvoice.ActorId != null && !customerInvoice.ActorReference.IsLoaded)
                        customerInvoice.ActorReference.Load();

                    if (customerInvoice.Actor != null && customerInvoice.Actor.ActorType == (int)SoeActorType.Customer)
                    {
                        if (!customerInvoice.Actor.CustomerReference.IsLoaded)
                            customerInvoice.Actor.CustomerReference.Load();

                        if (customerInvoice.Actor.Customer != null)
                        {
                            // Customer
                            Customer customer = customers.FirstOrDefault(c => c.Name == customerInvoice.Actor.Customer.Name && c.CustomerNr == customerInvoice.Actor.Customer.CustomerNr);
                            if (customer != null)
                            {
                                clone.ActorId = customer.ActorCustomerId;
                            }
                        }
                    }

                    #region Currency

                    Currency invoiceCurrency = currencies.FirstOrDefault(c => c.SysCurrencyId == customerInvoice.Currency.SysCurrencyId);
                    if (invoiceCurrency != null)
                        clone.CurrencyId = invoiceCurrency.CurrencyId;

                    decimal rate = CountryCurrencyManager.GetCurrencyRate(newCompanyId, invoiceCurrency != null ? invoiceCurrency.SysCurrencyId : 0, DateTime.Now, true);

                    clone.CurrencyRate = rate;
                    clone.CurrencyDate = DateTime.Now;

                    #endregion

                    #region Amounts

                    clone.TotalAmount = 0;
                    clone.TotalAmountCurrency = 0;
                    clone.TotalAmountEntCurrency = 0;
                    clone.TotalAmountLedgerCurrency = 0;
                    clone.VATAmount = 0;
                    clone.PaidAmount = 0;
                    clone.PaidAmountCurrency = 0;
                    clone.PaidAmountEntCurrency = 0;
                    clone.PaidAmountLedgerCurrency = 0;
                    clone.RemainingAmount = 0;
                    clone.RemainingAmountExVat = 0;
                    clone.RemainingAmountVat = 0;
                    clone.VATAmount = 0;
                    clone.VATAmountCurrency = 0;
                    clone.VATAmountEntCurrency = 0;
                    clone.VATAmountLedgerCurrency = 0;

                    #endregion

                    #endregion

                    #region CustomerInvoice

                    clone.OriginateFrom = 0;
                    clone.DeliveryAddressId = 0;
                    clone.BillingAddressId = 0;
                    clone.OrderDate = null;
                    clone.DeliveryDate = null;
                    clone.MarginalIncomeRatio = 0;
                    clone.NoOfReminders = 0;
                    clone.HasHouseholdTaxDeduction = false;
                    clone.BillingInvoicePrinted = false;
                    clone.FixedPriceOrder = false;
                    clone.MultipleAssetRows = false;
                    clone.ContractGroupId = null;
                    clone.NextContractPeriodYear = DateTime.Now.Year;
                    clone.NextContractPeriodValue = DateTime.Now.Month;
                    clone.InsecureDebt = false;
                    clone.ShiftTypeId = null;
                    clone.PlannedStartDate = null;
                    clone.PlannedStopDate = null;
                    clone.EstimatedTime = 0;
                    clone.RemainingTime = 0;
                    clone.Priority = null;
                    clone.KeepAsPlanned = false;
                    clone.OrderNumbers = null;

                    #region Amounts

                    clone.FreightAmount = 0;
                    clone.FreightAmountCurrency = 0;
                    clone.FreightAmountEntCurrency = 0;
                    clone.FreightAmountLedgerCurrency = 0;
                    clone.InvoiceFee = 0;
                    clone.InvoiceFeeCurrency = 0;
                    clone.InvoiceFeeEntCurrency = 0;
                    clone.InvoiceFeeLedgerCurrency = 0;
                    clone.CentRounding = 0;
                    clone.SumAmount = 0;
                    clone.SumAmountCurrency = 0;
                    clone.SumAmountEntCurrency = 0;
                    clone.SumAmountLedgerCurrency = 0;
                    clone.MarginalIncome = 0;
                    clone.MarginalIncomeCurrency = 0;
                    clone.MarginalIncomeEntCurrency = 0;
                    clone.MarginalIncomeLedgerCurrency = 0;

                    #endregion

                    #region PaymentCondition

                    if (templatePaymentConditions != null && clone.PaymentConditionId != null)
                    {
                        PaymentCondition paymentCondition = templatePaymentConditions.FirstOrDefault(p => p.PaymentConditionId == (int)clone.PaymentConditionId);
                        if (paymentCondition != null && newPaymentConditions != null)
                        {
                            PaymentCondition newPaymentCondition = newPaymentConditions.FirstOrDefault(p => p.Name == paymentCondition.Name);
                            if (newPaymentCondition != null)
                                clone.PaymentConditionId = newPaymentCondition.PaymentConditionId;
                        }
                    }

                    #endregion

                    #region DeliveryTypes

                    if (templateDeliveryTypes != null && clone.DeliveryTypeId != null)
                    {
                        DeliveryType deliveryType = templateDeliveryTypes.FirstOrDefault(p => p.DeliveryTypeId == (int)clone.DeliveryTypeId);
                        if (deliveryType != null && newDeliveryTypes != null)
                        {
                            DeliveryType newDeliveryType = newDeliveryTypes.FirstOrDefault(p => p.Name == deliveryType.Name);
                            if (newDeliveryType != null)
                                clone.DeliveryTypeId = newDeliveryType.DeliveryTypeId;
                        }
                    }

                    #endregion

                    #region DeliveryConditions

                    if (templateDeliveryConditions != null && clone.DeliveryConditionId != null)
                    {
                        DeliveryCondition deliveryCondition = templateDeliveryConditions.FirstOrDefault(p => p.DeliveryConditionId == (int)clone.DeliveryConditionId);
                        if (deliveryCondition != null && newDeliveryConditions != null)
                        {
                            DeliveryCondition newDeliveryCondition = newDeliveryConditions.FirstOrDefault(p => p.Name == deliveryCondition.Name);
                            if (newDeliveryCondition != null)
                                clone.DeliveryConditionId = newDeliveryCondition.DeliveryConditionId;
                        }
                    }

                    #endregion

                    #region PriceListTypes

                    if (templatePriceListTypes != null && clone.PriceListTypeId != null)
                    {
                        PriceListType priceListType = templatePriceListTypes.FirstOrDefault(p => p.PriceListTypeId == (int)clone.PriceListTypeId);
                        if (priceListType != null && newPriceListTypes != null)
                        {
                            PriceListType newPriceListType = newPriceListTypes.FirstOrDefault(p => p.Name == priceListType.Name);
                            if (newPriceListType != null)
                                clone.PriceListTypeId = newPriceListType.PriceListTypeId;
                        }
                    }

                    #endregion

                    #endregion

                    result = SaveChanges(entities);

                    if (!result.Success)
                        return result;

                    #region Checklist Records

                    List<ChecklistHeadRecord> headRecords = ChecklistManager.GetChecklistHeadRecords(SoeEntityType.Order, customerInvoice.InvoiceId, templateCompanyId, true);
                    if (headRecords != null)
                    {
                        //ChecklistHead newHead = newChecklistHeads.Where(h => h.Type == 

                        foreach (ChecklistHeadRecord headRecord in headRecords)
                        {
                            if (!headRecord.ChecklistRowRecord.IsLoaded)
                                headRecord.ChecklistRowRecord.Load();

                            ChecklistHead newHead = newChecklistHeads.FirstOrDefault(h => h.Type == headRecord.ChecklistHead.Type && h.Name == headRecord.ChecklistHead.Name);
                            if (newHead != null)
                            {
                                ChecklistHeadRecord newHeadRecord = new ChecklistHeadRecord()
                                {
                                    ChecklistHeadId = newHead.ChecklistHeadId,
                                    ActorCompanyId = newCompanyId,
                                    Entity = (int)SoeEntityType.Order,
                                    RecordId = clone.InvoiceId,
                                };

                                SetCreatedProperties(newHeadRecord);
                                entities.ChecklistHeadRecord.AddObject(newHeadRecord);

                                if (!result.Success)
                                    return result;

                                foreach (ChecklistRowRecord rowRecord in headRecord.ChecklistRowRecord)
                                {
                                    if (!rowRecord.ChecklistRowReference.IsLoaded)
                                        rowRecord.ChecklistRowReference.Load();

                                    ChecklistRow row = newHead.ChecklistRow.FirstOrDefault(r => r.Type == rowRecord.ChecklistRow.Type && r.RowNr == rowRecord.ChecklistRow.RowNr);
                                    if (row != null)
                                    {
                                        ChecklistRowRecord newRowRecord = new ChecklistRowRecord()
                                        {
                                            ChecklistHeadRecordId = newHeadRecord.ChecklistHeadRecordId,
                                            ChecklistRowId = row.ChecklistRowId,
                                            ActorCompanyId = newCompanyId,
                                            Date = rowRecord.Date,
                                            Text = rowRecord.Text,
                                            Type = rowRecord.Type,
                                            Comment = rowRecord.Comment,
                                            DataTypeId = rowRecord.DataTypeId,
                                            StrData = rowRecord.StrData,
                                            IntData = rowRecord.IntData,
                                            BoolData = rowRecord.BoolData,
                                            DateData = rowRecord.DateData,
                                            DecimalData = rowRecord.DecimalData,
                                        };

                                        SetCreatedProperties(newRowRecord);
                                        entities.ChecklistRowRecord.AddObject(newRowRecord);
                                    }
                                }

                                result = SaveChanges(entities);
                            }
                        }
                    }

                    #endregion
                }
            }

            return result;
        }

        public ActionResult CopyPriceRulesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            var result = new ActionResult();

            using (var entities = new CompEntities())
            {
                #region Prereq

                Company company = CompanyManager.GetCompany(entities, newCompanyId);

                List<PriceRule> templatePriceRules = PriceRuleManager.GetPriceRules(entities, templateCompanyId, true);
                List<PriceRule> existingPriceRules = PriceRuleManager.GetPriceRules(entities, newCompanyId, true);

                List<PriceListType> templatePriceListTypes = ProductPricelistManager.GetPriceListTypes(entities, templateCompanyId);
                List<PriceListType> existingPriceListTypes = ProductPricelistManager.GetPriceListTypes(entities, newCompanyId);

                #endregion

                try
                {
                    //Get sub rules
                    List<int> subPriceRuleIds = new List<int>();
                    foreach (PriceRule sourcePriceRule in templatePriceRules)
                    {
                        if (sourcePriceRule.RRule != null)
                        {
                            if (!subPriceRuleIds.Contains(sourcePriceRule.RRule.RuleId))
                                subPriceRuleIds.Add(sourcePriceRule.RRule.RuleId);
                        }
                        if (sourcePriceRule.LRule != null)
                        {
                            if (!subPriceRuleIds.Contains(sourcePriceRule.LRule.RuleId))
                                subPriceRuleIds.Add(sourcePriceRule.LRule.RuleId);
                        }
                    }

                    foreach (var templatePriceRule in templatePriceRules)
                    {
                        if (subPriceRuleIds.Contains(templatePriceRule.RuleId))
                            continue;

                        var templatePriceListType = templatePriceListTypes.FirstOrDefault(t => t.PriceListTypeId == templatePriceRule.PriceListType.PriceListTypeId);
                        if (templatePriceListType == null)
                            continue;

                        var mappedPriceListType = existingPriceListTypes.FirstOrDefault(t => t.Name == templatePriceListType.Name);
                        if (mappedPriceListType == null)
                            continue;

                        var existingPriceRule = existingPriceRules.FirstOrDefault(r => r.PriceListType == mappedPriceListType && r.LValueType == templatePriceRule.LValueType && r.RValueType == templatePriceRule.RValueType && r.LValue == templatePriceRule.LValue && r.RValue == templatePriceRule.RValue);

                        if (existingPriceRule == null)
                        {
                            PriceRule newPriceRule = CopyPriceRuleRecursive(templatePriceRule, mappedPriceListType, company, templatePriceRules);
                            if (newPriceRule == null)
                                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceRule");

                            result = AddEntityItem(entities, newPriceRule, "PriceRule");
                            if (result.Success)
                                result.IntegerValue = newPriceRule.RuleId;
                        }
                    }

                    result = SaveChanges(entities);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    result.Success = false;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else if (result.Exception != null)
                        base.LogError(result.Exception, this.log);
                    else if (result.ErrorMessage != null)
                        base.LogError(result.ErrorMessage);
                    else
                        base.LogError("Error in CopyPriceRulesFromTemplateCompany");

                    entities.Connection.Close();
                }

            }
            return result;
        }

        private PriceRule CopyPriceRuleRecursive(PriceRule priceRule, PriceListType priceListType, Company company, List<PriceRule> templatePriceRules)
        {
            PriceRule leftPriceRule = null;
            if (priceRule.LRule != null)
                leftPriceRule = CopyPriceRuleRecursive(templatePriceRules.FirstOrDefault(r => r.RuleId == priceRule.LRule.RuleId), priceListType, company, templatePriceRules);
            PriceRule rightPriceRule = null;
            if (priceRule.RRule != null)
                rightPriceRule = CopyPriceRuleRecursive(templatePriceRules.FirstOrDefault(r => r.RuleId == priceRule.RRule.RuleId), priceListType, company, templatePriceRules);

            PriceRule newPriceRule = new PriceRule()
            {
                OperatorType = priceRule.OperatorType,
                lExampleType = priceRule.lExampleType,
                rExampleType = priceRule.rExampleType,
                LValue = priceRule.LValue,
                RValue = priceRule.RValue,
                LValueType = priceRule.LValueType,
                RValueType = priceRule.RValueType,

                //References
                LRule = leftPriceRule,
                RRule = rightPriceRule,
                PriceListType = priceListType,
                Company = company,
            };

            return newPriceRule;
        }

        #endregion

        #region Time

        public ActionResult CopyEmployeeGroupsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, EmployeeGroup> employeeGroupMapping, ref Dictionary<int, TimeDeviationCause> timeDeviationCausesMapping, ref Dictionary<int, TimeCode> timeCodeMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                List<TermGroup_AttestEntity> entitys = new List<TermGroup_AttestEntity>()
                {
                    TermGroup_AttestEntity.InvoiceTime,
                    TermGroup_AttestEntity.PayrollTime,
                };

                //Existing for template Company
                List<EmployeeGroup> templateEmployeeGroups = EmployeeManager.GetEmployeeGroups(entities, templateCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true, loadAttestTransitions: true, loadTimeCodeBreaks: true, loadTimeStampRounding: true).ToList();
                List<TimeCode> templateTimeCodes = TimeCodeManager.GetTimeCodes(entities, templateCompanyId);
                List<DayType> templateDaytypes = CalendarManager.GetDayTypesByCompany(templateCompanyId);
                List<AttestTransition> templateAttestTransitions = AttestManager.GetAttestTransitions(entities, entitys, SoeModule.Time, false, templateCompanyId);
                List<TimeDeviationCause> templateTimeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, templateCompanyId, loadTimeCode: true);
                List<AttestState> templateAttestStates = AttestManager.GetAttestStates(entities, templateCompanyId, entitys, SoeModule.Time);

                //Existing for new Company (before copy)
                List<EmployeeGroup> existingEmployeeGroups = EmployeeManager.GetEmployeeGroups(entities, newCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true, loadAttestTransitions: true, loadTimeCodeBreaks: true, loadTimeStampRounding: true).ToList();
                List<TimeCode> existingTimeCodes = TimeCodeManager.GetTimeCodes(entities, newCompanyId);
                List<DayType> existingDayTypes = CalendarManager.GetDayTypesByCompany(entities, newCompanyId);
                List<TimeCodeBreak> existingTimeCodeBreaks = TimeCodeManager.GetTimeCodeBreaks(entities, newCompanyId);
                List<AttestTransition> existingAttestTransitions = AttestManager.GetAttestTransitions(entities, entitys, SoeModule.Time, false, newCompanyId);
                List<TimeDeviationCause> existingTimeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, newCompanyId, loadTimeCode: true);
                List<AttestState> existingAttestStates = AttestManager.GetAttestStates(entities, newCompanyId, entitys, SoeModule.Time);

                #endregion

                foreach (EmployeeGroup templateEmployeeGroup in templateEmployeeGroups)
                {
                    #region EmployeeGroup

                    EmployeeGroup newEmployeeGroup = existingEmployeeGroups.FirstOrDefault(eg => eg.Name == templateEmployeeGroup.Name);
                    if (newEmployeeGroup == null)
                    {
                        #region Add

                        newEmployeeGroup = new EmployeeGroup()
                        {
                            //References
                            Company = newCompany,
                        };
                        SetCreatedProperties(newEmployeeGroup);
                        entities.EmployeeGroup.AddObject(newEmployeeGroup);

                        if (templateEmployeeGroup.TimeDeviationCauseId.HasValue && timeDeviationCausesMapping.ContainsKey(templateEmployeeGroup.TimeDeviationCauseId.Value))
                            newEmployeeGroup.TimeDeviationCauseId = timeDeviationCausesMapping[templateEmployeeGroup.TimeDeviationCauseId.Value].TimeDeviationCauseId;
                        if (templateEmployeeGroup.TimeCodeId.HasValue && timeDeviationCausesMapping.ContainsKey(templateEmployeeGroup.TimeCodeId.Value))
                            newEmployeeGroup.TimeCodeId = timeCodeMapping[templateEmployeeGroup.TimeCodeId.Value].TimeCodeId;

                        newCompany.EmployeeGroup.Add(newEmployeeGroup);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        SetModifiedProperties(newEmployeeGroup);

                        #endregion
                    }

                    employeeGroupMapping.Add(templateEmployeeGroup.EmployeeGroupId, newEmployeeGroup);

                    #region Common

                    newEmployeeGroup.Name = templateEmployeeGroup.Name;
                    newEmployeeGroup.DeviationAxelStartHours = templateEmployeeGroup.DeviationAxelStartHours;
                    newEmployeeGroup.DeviationAxelStopHours = templateEmployeeGroup.DeviationAxelStopHours;
                    newEmployeeGroup.PayrollProductAccountingPrio = templateEmployeeGroup.PayrollProductAccountingPrio;
                    newEmployeeGroup.InvoiceProductAccountingPrio = templateEmployeeGroup.InvoiceProductAccountingPrio;
                    newEmployeeGroup.AutogenTimeblocks = templateEmployeeGroup.AutogenTimeblocks;
                    newEmployeeGroup.AutogenBreakOnStamping = templateEmployeeGroup.AutogenBreakOnStamping;
                    newEmployeeGroup.MergeScheduleBreaksOnDay = templateEmployeeGroup.MergeScheduleBreaksOnDay;
                    newEmployeeGroup.BreakDayMinutesAfterMidnight = templateEmployeeGroup.BreakDayMinutesAfterMidnight;
                    newEmployeeGroup.KeepStampsTogetherWithinMinutes = templateEmployeeGroup.KeepStampsTogetherWithinMinutes;
                    newEmployeeGroup.RuleWorkTimeWeek = templateEmployeeGroup.RuleWorkTimeWeek;
                    newEmployeeGroup.RuleRestTimeDay = templateEmployeeGroup.RuleRestTimeDay;
                    newEmployeeGroup.RuleRestTimeWeek = templateEmployeeGroup.RuleRestTimeWeek;
                    newEmployeeGroup.AlwaysDiscardBreakEvaluation = templateEmployeeGroup.AlwaysDiscardBreakEvaluation;
                    newEmployeeGroup.MergeScheduleBreaksOnDay = templateEmployeeGroup.MergeScheduleBreaksOnDay;
                    newEmployeeGroup.ReminderAttestStateId = templateEmployeeGroup.ReminderAttestStateId;
                    newEmployeeGroup.ReminderNoOfDays = templateEmployeeGroup.ReminderNoOfDays;
                    newEmployeeGroup.ReminderPeriodType = templateEmployeeGroup.ReminderPeriodType;
                    newEmployeeGroup.RuleRestTimeWeek = templateEmployeeGroup.RuleRestTimeWeek;
                    newEmployeeGroup.RuleWorkTimeYear2014 = templateEmployeeGroup.RuleWorkTimeYear2014;
                    newEmployeeGroup.RuleWorkTimeYear2015 = templateEmployeeGroup.RuleWorkTimeYear2015;
                    newEmployeeGroup.RuleWorkTimeYear2016 = templateEmployeeGroup.RuleWorkTimeYear2016;
                    newEmployeeGroup.RuleWorkTimeYear2017 = templateEmployeeGroup.RuleWorkTimeYear2017;
                    newEmployeeGroup.RuleWorkTimeYear2018 = templateEmployeeGroup.RuleWorkTimeYear2018;
                    newEmployeeGroup.RuleWorkTimeYear2019 = templateEmployeeGroup.RuleWorkTimeYear2019;
                    newEmployeeGroup.RuleWorkTimeYear2020 = templateEmployeeGroup.RuleWorkTimeYear2020;
                    newEmployeeGroup.RuleWorkTimeYear2021 = templateEmployeeGroup.RuleWorkTimeYear2021;
                    newEmployeeGroup.MaxScheduleTimeFullTime = templateEmployeeGroup.MaxScheduleTimeFullTime;
                    newEmployeeGroup.MinScheduleTimeFullTime = templateEmployeeGroup.MinScheduleTimeFullTime;
                    newEmployeeGroup.MaxScheduleTimePartTime = templateEmployeeGroup.MaxScheduleTimePartTime;
                    newEmployeeGroup.MinScheduleTimePartTime = templateEmployeeGroup.MinScheduleTimePartTime;
                    newEmployeeGroup.MaxScheduleTimeWithoutBreaks = templateEmployeeGroup.MaxScheduleTimeWithoutBreaks;
                    newEmployeeGroup.RuleWorkTimeDayMinimum = templateEmployeeGroup.RuleWorkTimeDayMinimum;
                    newEmployeeGroup.RuleWorkTimeDayMaximumWorkDay = templateEmployeeGroup.RuleWorkTimeDayMaximumWorkDay;
                    newEmployeeGroup.RuleWorkTimeDayMaximumWeekend = templateEmployeeGroup.RuleWorkTimeDayMaximumWeekend;
                    newEmployeeGroup.MaxScheduleTimeWithoutBreaks = templateEmployeeGroup.MaxScheduleTimeWithoutBreaks;
                    newEmployeeGroup.TimeReportType = templateEmployeeGroup.TimeReportType;
                    newEmployeeGroup.QualifyingDayCalculationRule = templateEmployeeGroup.QualifyingDayCalculationRule;
                    newEmployeeGroup.QualifyingDayCalculationRuleLimitFirstDay = templateEmployeeGroup.QualifyingDayCalculationRuleLimitFirstDay;
                    newEmployeeGroup.TimeWorkReductionCalculationRule = templateEmployeeGroup.TimeWorkReductionCalculationRule;
                    newEmployeeGroup.AutoGenTimeAndBreakForProject = templateEmployeeGroup.AutoGenTimeAndBreakForProject;
                    newEmployeeGroup.BreakRoundingUp = templateEmployeeGroup.BreakRoundingUp;
                    newEmployeeGroup.BreakRoundingDown = templateEmployeeGroup.BreakRoundingDown;
                    newEmployeeGroup.NotifyChangeOfDeviations = templateEmployeeGroup.NotifyChangeOfDeviations;
                    newEmployeeGroup.RuleRestDayIncludePresence = templateEmployeeGroup.RuleRestDayIncludePresence;
                    newEmployeeGroup.RuleRestWeekIncludePresence = templateEmployeeGroup.RuleRestWeekIncludePresence;
                    newEmployeeGroup.AllowShiftsWithoutAccount = templateEmployeeGroup.AllowShiftsWithoutAccount;
                    newEmployeeGroup.AlsoAttestAdditionsFromTime = templateEmployeeGroup.AlsoAttestAdditionsFromTime;
                    newEmployeeGroup.RuleScheduleFreeWeekendsMinimumYear = templateEmployeeGroup.RuleScheduleFreeWeekendsMinimumYear;
                    newEmployeeGroup.RuleScheduledDaysMaximumWeek = templateEmployeeGroup.RuleScheduledDaysMaximumWeek;
                    newEmployeeGroup.CandidateForOvertimeOnZeroDayExcluded = templateEmployeeGroup.CandidateForOvertimeOnZeroDayExcluded;
                    newEmployeeGroup.ExtraShiftAsDefault = templateEmployeeGroup.ExtraShiftAsDefault;
                    newEmployeeGroup.State = templateEmployeeGroup.State;

                    #endregion

                    #region TimeCode

                    if (templateEmployeeGroup.TimeCodeId.HasValue)
                    {
                        TimeCode templateTimeCode = templateTimeCodes.FirstOrDefault(t => t.TimeCodeId == templateEmployeeGroup.TimeCodeId);
                        if (templateTimeCode != null)
                        {
                            TimeCode existingTimeCode = existingTimeCodes.FirstOrDefault(t => t.Type == templateTimeCode.Type && t.Code == templateTimeCode.Code && t.Name == templateTimeCode.Name && t.RoundingType == templateTimeCode.RoundingType && t.RoundingValue == templateTimeCode.RoundingValue);
                            if (existingTimeCode != null)
                                newEmployeeGroup.TimeCode = existingTimeCode;
                        }
                    }

                    #endregion

                    #region TimeCodeBreak

                    newEmployeeGroup.TimeCodeBreak.Clear();

                    foreach (TimeCodeBreak templateTimeCodeBreak in templateEmployeeGroup.TimeCodeBreak)
                    {
                        if (!timeCodeMapping.ContainsKey(templateTimeCodeBreak.TimeCodeId))
                            continue;

                        int? existingTimeCodeBreakId = timeCodeMapping[templateTimeCodeBreak.TimeCodeId]?.TimeCodeId;
                        if (!existingTimeCodeBreakId.HasValue)
                            continue;

                        TimeCode timeCode = existingTimeCodes.FirstOrDefault(i => i.TimeCodeId == existingTimeCodeBreakId.Value);
                        if (timeCode is TimeCodeBreak timeCodeBreak)
                            newEmployeeGroup.TimeCodeBreak.Add(timeCodeBreak);
                    }

                    #endregion

                    #region DayTypes

                    if (!templateEmployeeGroup.DayType.IsLoaded)
                        templateEmployeeGroup.DayType.Load();

                    if (newEmployeeGroup.EmployeeGroupId > 0 && !newEmployeeGroup.DayType.IsLoaded)
                        newEmployeeGroup.DayType.Load();

                    foreach (DayType templateEmployeeGroupDayType in templateEmployeeGroup.DayType)
                    {
                        DayType templateDaytype = templateDaytypes.FirstOrDefault(i => i.DayTypeId == templateEmployeeGroupDayType.DayTypeId);
                        if (templateDaytype != null)
                        {
                            DayType existingDayType = existingDayTypes.FirstOrDefault(i => i.Name == templateDaytype.Name);
                            if (existingDayType != null && !newEmployeeGroup.DayType.Any(i => i.DayTypeId == existingDayType.DayTypeId))
                                newEmployeeGroup.DayType.Add(existingDayType);
                        }
                    }

                    #endregion

                    #region TimeCodeBreak

                    if (!templateEmployeeGroup.TimeCodeBreak.IsLoaded)
                        templateEmployeeGroup.TimeCodeBreak.Load();

                    if (newEmployeeGroup.EmployeeGroupId != 0 && !newEmployeeGroup.TimeCodeBreak.IsLoaded)
                        newEmployeeGroup.TimeCodeBreak.Load();

                    foreach (TimeCodeBreak templateEmployeeGroupTimeCodeBreak in templateEmployeeGroup.TimeCodeBreak)
                    {
                        TimeCodeBreak templateTimeCodeBreak = existingTimeCodeBreaks.FirstOrDefault(i => i.TimeCodeId == templateEmployeeGroupTimeCodeBreak.TimeCodeId);
                        if (templateTimeCodeBreak != null)
                        {
                            TimeCodeBreak existingTimeCodeBreak = existingTimeCodeBreaks.FirstOrDefault(i => i.Name == templateTimeCodeBreak.Name && i.DefaultMinutes == templateTimeCodeBreak.DefaultMinutes);
                            if (existingTimeCodeBreak != null && !newEmployeeGroup.TimeCodeBreak.Any(i => i.TimeCodeId == existingTimeCodeBreak.TimeCodeId))
                                newEmployeeGroup.TimeCodeBreak.Add(existingTimeCodeBreak);
                        }
                    }

                    #endregion

                    #region AttestTransitions

                    if (!templateEmployeeGroup.AttestTransition.IsLoaded)
                        templateEmployeeGroup.AttestTransition.Load();

                    if (newEmployeeGroup.EmployeeGroupId != 0 && !newEmployeeGroup.AttestTransition.IsLoaded)
                        newEmployeeGroup.AttestTransition.Load();

                    foreach (AttestTransition templateEmployeeGroupAttestTransition in templateEmployeeGroup.AttestTransition)
                    {
                        AttestTransition templateAttestTransition = templateAttestTransitions.FirstOrDefault(i => i.AttestTransitionId == templateEmployeeGroupAttestTransition.AttestTransitionId);
                        if (templateAttestTransition != null)
                        {
                            AttestState templateAttestStateFrom = templateAttestStates.FirstOrDefault(i => i.AttestStateId == templateAttestTransition.AttestStateFromId);
                            if (templateAttestStateFrom == null)
                                continue;
                            AttestState templateAttestStateTo = templateAttestStates.FirstOrDefault(i => i.AttestStateId == templateAttestTransition.AttestStateToId);
                            if (templateAttestStateTo == null)
                                continue;
                            AttestState existingAttestStateFrom = existingAttestStates.FirstOrDefault(i => i.Name == templateAttestStateFrom.Name && i.Entity == templateAttestStateFrom.Entity);
                            if (existingAttestStateFrom == null)
                                continue;
                            AttestState existingAttestStateTo = existingAttestStates.FirstOrDefault(i => i.Name == templateAttestStateTo.Name && i.Entity == templateAttestStateTo.Entity);
                            if (existingAttestStateTo == null)
                                continue;

                            AttestTransition existingAttestTransition = existingAttestTransitions.FirstOrDefault(i => i.Name == templateAttestTransition.Name && i.Module == templateAttestTransition.Module);
                            if (existingAttestTransition != null)
                            {
                                bool isMapped = newEmployeeGroup.AttestTransition.Any(i => i.AttestTransitionId == existingAttestTransition.AttestTransitionId);
                                if (!isMapped)
                                    newEmployeeGroup.AttestTransition.Add(existingAttestTransition);
                            }
                        }
                    }

                    #endregion

                    #region TimeStampRounding

                    if (!templateEmployeeGroup.TimeStampRounding.IsLoaded)
                        templateEmployeeGroup.TimeStampRounding.Load();

                    //newEmployeeGroup.TimeStampRounding.Clear();

                    foreach (TimeStampRounding templateTimeStampRounding in templateEmployeeGroup.TimeStampRounding)
                    {
                        if (newEmployeeGroup.TimeStampRounding.Any(r =>
                            r.RoundInNeg == templateTimeStampRounding.RoundInNeg ||
                            r.RoundInPos == templateTimeStampRounding.RoundInPos ||
                            r.RoundOutNeg == templateTimeStampRounding.RoundOutNeg ||
                            r.RoundOutPos == templateTimeStampRounding.RoundOutPos))
                            continue;

                        TimeStampRounding newTimeStampRounding = new TimeStampRounding()
                        {
                            RoundInNeg = templateTimeStampRounding.RoundInNeg,
                            RoundInPos = templateTimeStampRounding.RoundInPos,
                            RoundOutNeg = templateTimeStampRounding.RoundOutNeg,
                            RoundOutPos = templateTimeStampRounding.RoundOutPos,
                        };
                        SetCreatedProperties(newTimeStampRounding);
                        newEmployeeGroup.TimeStampRounding.Add(newTimeStampRounding);
                    }

                    #endregion

                    #endregion

                    result = SaveChanges(entities);

                    if (result.Success)
                    {
                        #region Mappings (require EmployeeGroupId so must be after save)

                        #region TimeDeviationCauses

                        if (!templateEmployeeGroup.EmployeeGroupTimeDeviationCause.IsLoaded)
                            templateEmployeeGroup.EmployeeGroupTimeDeviationCause.Load();

                        if (newEmployeeGroup.EmployeeGroupId > 0 && !newEmployeeGroup.EmployeeGroupTimeDeviationCause.IsLoaded)
                            newEmployeeGroup.EmployeeGroupTimeDeviationCause.Load();

                        foreach (EmployeeGroupTimeDeviationCause templateEmployeeGroupTimeDeviationCause in templateEmployeeGroup.EmployeeGroupTimeDeviationCause.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            TimeDeviationCause templateTimeDeviationCause = templateTimeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == templateEmployeeGroupTimeDeviationCause.TimeDeviationCauseId);
                            if (templateTimeDeviationCause != null)
                            {
                                TimeDeviationCause existingTimeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(t => t.Type == templateTimeDeviationCause.Type && t.Name == templateTimeDeviationCause.Name && t.Description == templateTimeDeviationCause.Description && t.OnlyWholeDay == templateTimeDeviationCause.OnlyWholeDay);
                                if (existingTimeDeviationCause != null)
                                {
                                    bool exists = newEmployeeGroup.EmployeeGroupTimeDeviationCause.Any(i => i.TimeDeviationCauseId == existingTimeDeviationCause.TimeDeviationCauseId && i.State == (int)SoeEntityState.Active);
                                    if (!exists)
                                    {
                                        EmployeeGroupTimeDeviationCause employeeGroupTimeDeviationCause = EmployeeManager.CreateEmployeeGroupTimeDeviationCause(entities, newEmployeeGroup, existingTimeDeviationCause.TimeDeviationCauseId, newCompanyId, useInTimeTerminal: templateEmployeeGroupTimeDeviationCause.UseInTimeTerminal);
                                        if (employeeGroupTimeDeviationCause == null)
                                            LogCopyError("EmployeeGroupTimeDeviationCause", templateCompanyId, newCompanyId, saved: true);
                                    }

                                    if (templateEmployeeGroup.TimeDeviationCauseId.HasValue && templateTimeDeviationCause.TimeDeviationCauseId == templateEmployeeGroup.TimeDeviationCauseId.Value)
                                        newEmployeeGroup.TimeDeviationCause = existingTimeDeviationCause;
                                }
                            }
                        }

                        #endregion

                        #region TimeDeviationCauseRequest

                        if (!templateEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.IsLoaded)
                            templateEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.Load();

                        if (newEmployeeGroup.EmployeeGroupId != 0 && !newEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.IsLoaded)
                            newEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.Load();

                        foreach (EmployeeGroupTimeDeviationCauseRequest templateEmployeeGroupDeviationCauseRequest in templateEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest)
                        {
                            TimeDeviationCause templateTimeDeviationCause = templateTimeDeviationCauses.FirstOrDefault(i => i.TimeDeviationCauseId == templateEmployeeGroupDeviationCauseRequest.TimeDeviationCauseId);
                            if (templateTimeDeviationCause != null)
                            {
                                TimeDeviationCause existingTimeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(i => i.Name == templateTimeDeviationCause.Name && i.Description == templateTimeDeviationCause.Description && i.Type == templateTimeDeviationCause.Type && i.OnlyWholeDay == templateTimeDeviationCause.OnlyWholeDay);
                                if (existingTimeDeviationCause != null)
                                {
                                    bool isMapped = newEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.Any(i => i.TimeDeviationCauseId == existingTimeDeviationCause.TimeDeviationCauseId);
                                    if (!isMapped)
                                        newEmployeeGroup.EmployeeGroupTimeDeviationCauseRequest.Add(EmployeeGroupTimeDeviationCauseRequest.CreateEmployeeGroupTimeDeviationCauseRequest(newEmployeeGroup.EmployeeGroupId, existingTimeDeviationCause.TimeDeviationCauseId));
                                }
                            }
                        }

                        #endregion

                        #endregion

                        result = SaveChanges(entities);
                    }

                    if (!result.Success)
                        LogCopyError("EmployeeGroup", templateCompanyId, newCompanyId, saved: true);
                }
            }

            return result;
        }

        public ActionResult CopyEmploymentTypesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Existing for template Company
                List<EmploymentType> templateEmploymentTypes = entities.EmploymentType.Where(e => e.ActorCompanyId == templateCompanyId && e.State != (int)SoeEntityState.Deleted).ToList();

                //Existing for new Company (before copy)
                List<EmploymentType> existingEmploymentTypes = entities.EmploymentType.Where(e => e.ActorCompanyId == newCompanyId && e.State != (int)SoeEntityState.Deleted).ToList();

                #endregion

                foreach (EmploymentType templateEmploymentType in templateEmploymentTypes)
                {
                    EmploymentType newEmploymentType = existingEmploymentTypes.FirstOrDefault(t => t.Name == templateEmploymentType.Name);
                    if (newEmploymentType == null)
                    {
                        #region Add

                        newEmploymentType = new EmploymentType()
                        {
                            //References
                            Company = newCompany,
                        };
                        SetCreatedProperties(newEmploymentType);
                        entities.EmploymentType.AddObject(newEmploymentType);
                        newCompany.EmploymentType.Add(newEmploymentType);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        SetModifiedProperties(newEmploymentType);

                        #endregion
                    }

                    #region Common

                    newEmploymentType.Description = templateEmploymentType.Description;
                    newEmploymentType.Type = templateEmploymentType.Type;
                    newEmploymentType.Name = templateEmploymentType.Name;
                    newEmploymentType.Code = templateEmploymentType.Code;
                    newEmploymentType.State = templateEmploymentType.State;

                    #endregion

                    result = SaveChanges(entities);
                    if (!result.Success)
                        LogCopyError("EmploymentType", templateCompanyId, newCompanyId, saved: true);
                }
            }

            return result;
        }

        public ActionResult CopyPayrollProductsAndTimeCodesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool copyBreakTemplates, bool copyProjectSettings, ref Dictionary<int, TimeCode> timeCodeMapping, ref Dictionary<int, TimeCodeBreakGroup> timeCodeBreakGroupMapping, ref Dictionary<int, PayrollGroup> payrollGroupMapping, ref Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping, ref Dictionary<int, PayrollPriceType> payrollPriceTypeMapping, ref Dictionary<int, AccountStd> accountStdMapping, ref Dictionary<int, AccountInternal> accountInternalMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            Dictionary<int, PayrollProduct> payrollProductMappingDict = new Dictionary<int, PayrollProduct>();
            Dictionary<int, InvoiceProduct> invoiceProductMappingDict = new Dictionary<int, InvoiceProduct>();

            List<PayrollProduct> templatePayrollProducts = ProductManager.GetPayrollProducts(templateCompanyId, active: null, loadPriceTypesAndPriceFormulas: true, loadPayrollProductSettingAccounts: true);
            List<InvoiceProduct> templateInvoiceProducts = ProductManager.GetInvoiceProducts(templateCompanyId, active: true);
            List<TimeCodeBreakGroup> templateTimeCodeBreakGroups = TimeCodeManager.GetTimeCodeBreakGroups(templateCompanyId);
            List<TimeCode> templateTimeCodes = TimeCodeManager.GetTimeCodes(templateCompanyId);
            List<TimeBreakTemplate> templateTimeBreakTemplateRules = TimeScheduleManager.GetTimeBreakTemplates(templateCompanyId);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Existing for new Company
                List<PayrollProduct> existingPayrollProducts = update ? ProductManager.GetPayrollProducts(entities, newCompanyId, active: null, loadPriceTypesAndPriceFormulas: true, loadPayrollProductSettingAccounts: true) : new List<PayrollProduct>();
                List<InvoiceProduct> existingInvoiceProducts = ProductManager.GetInvoiceProducts(entities, newCompanyId, active: true);
                List<TimeCode> existingTimeCodes = update ? TimeCodeManager.GetTimeCodes(entities, newCompanyId) : new List<TimeCode>();
                List<TimeCodeBreakGroup> existingTimeCodeBreakGroups = TimeCodeManager.GetTimeCodeBreakGroups(entities, newCompanyId);
                List<TimeBreakTemplate> existingTimeBreakTemplates = TimeScheduleManager.GetTimeBreakTemplates(entities, newCompanyId);
                List<ShiftType> existingShiftTypes = TimeScheduleManager.GetShiftTypes(entities, newCompanyId);

                #endregion

                #region PayrollProduct

                foreach (PayrollProduct templatePayrollProduct in templatePayrollProducts)
                {
                    #region PayrollProduct

                    PayrollProduct payrollProduct = null;
                    if (update)
                        payrollProduct = GetExistingPayrollProduct(existingPayrollProducts, templatePayrollProducts, templatePayrollProduct.ProductId);

                    if (payrollProduct == null)
                    {
                        payrollProduct = new PayrollProduct();
                        SetCreatedProperties(payrollProduct);
                    }
                    else if (payrollProduct != null && !templatePayrollProduct.PayrollProductSetting.IsNullOrEmpty() && payrollProduct.PayrollProductSetting.IsNullOrEmpty())
                        SetModifiedProperties(payrollProduct);
                    else
                        continue; //accourding to rickard - too dangerous

                    payrollProduct.SetProperties(templatePayrollProduct);

                    if (payrollProduct.ProductId == 0)
                        newCompany.Product.Add(payrollProduct);
                    if (!payrollProductMappingDict.ContainsKey(templatePayrollProduct.ProductId))
                        payrollProductMappingDict.Add(templatePayrollProduct.ProductId, payrollProduct);

                    #endregion
                }

                #region Save

                result = SaveChanges(entities);
                if (!result.Success)
                {
                    result = LogCopyError("PayrollProduct", templateCompanyId, newCompanyId, saved: true);
                    return result;
                }

                #endregion

                #region PayrollProductSetting

                foreach (PayrollProduct templatePayrollProduct in templatePayrollProducts)
                {
                    if (!payrollProductMappingDict.ContainsKey(templatePayrollProduct.ProductId))
                        continue;

                    PayrollProduct payrollProduct = payrollProductMappingDict[templatePayrollProduct.ProductId];
                    if (payrollProduct.PayrollProductSetting.Count > 0)
                        continue;

                    bool usedGenericSetting = false;
                    foreach (PayrollProductSetting templatePayrollProductSetting in templatePayrollProduct.PayrollProductSetting.Where(i => i.State == (int)SoeEntityState.Active))
                    {
                        #region PayrollProductSetting

                        PayrollGroup payrollGroup = null;
                        if (templatePayrollProductSetting.PayrollGroupId.HasValue)
                        {
                            if (payrollGroupMapping.ContainsKey(templatePayrollProductSetting.PayrollGroupId.Value))
                                payrollGroup = payrollGroupMapping[templatePayrollProductSetting.PayrollGroupId.Value];
                            //Do not continue if PayrollGroup is not found (otherwise meaning it is upgraded to generic)
                            if (payrollGroup == null)
                                continue;
                        }

                        //Can only have one PayrollProductSetting without PayrollGroup
                        if (payrollGroup == null)
                        {
                            if (usedGenericSetting)
                                continue;
                            usedGenericSetting = true;
                        }

                        PayrollProduct childPayrollProduct = null;
                        if (templatePayrollProductSetting.ChildProductId.HasValue && payrollProductMappingDict.ContainsKey(templatePayrollProductSetting.ChildProductId.Value))
                            childPayrollProduct = payrollProductMappingDict[templatePayrollProductSetting.ChildProductId.Value];

                        PayrollProductSetting payrollProductSetting = ProductManager.CreatePayrollProductSetting(payrollProduct, templatePayrollProductSetting, payrollGroup?.PayrollGroupId, childPayrollProduct?.ProductId);
                        entities.PayrollProductSetting.AddObject(payrollProductSetting);

                        #endregion

                        #region PayrollProductAccountStd

                        foreach (PayrollProductAccountStd templatePayrollProductAccountStd in templatePayrollProductSetting.PayrollProductAccountStd)
                        {
                            AccountStd accountStd = null;
                            if (templatePayrollProductAccountStd.AccountId.HasValue && accountStdMapping.ContainsKey(templatePayrollProductAccountStd.AccountId.Value))
                                accountStd = accountStdMapping[templatePayrollProductAccountStd.AccountId.Value];

                            PayrollProductAccountStd payrollProductAccountStd = new PayrollProductAccountStd()
                            {
                                Type = templatePayrollProductAccountStd.Type,
                                Percent = templatePayrollProductAccountStd.Percent,

                                //Set FK
                                AccountId = accountStd != null ? accountStd.AccountId : (int?)null,

                                //Set reference
                                PayrollProductSetting = payrollProductSetting,
                            };
                            entities.PayrollProductAccountStd.AddObject(payrollProductAccountStd);

                            #region PayrollProductAccountInternal

                            foreach (AccountInternal templateAccountInternal in templatePayrollProductAccountStd.AccountInternal)
                            {
                                AccountInternal accountInternal = null;
                                if (accountInternalMapping.ContainsKey(templateAccountInternal.AccountId))
                                    accountInternal = AccountManager.GetAccountInternal(entities, accountInternalMapping[templateAccountInternal.AccountId].AccountId, newCompanyId);
                                if (accountInternal != null)
                                    payrollProductAccountStd.AccountInternal.Add(accountInternal);
                            }

                            #endregion
                        }

                        #endregion

                        #region PayrollProductPriceFormula

                        foreach (PayrollProductPriceFormula templatePayrollProductPriceFormula in templatePayrollProductSetting.PayrollProductPriceFormula.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            PayrollPriceFormula payrollPriceFormula = null;
                            if (payrollPriceFormulaMapping.ContainsKey(templatePayrollProductPriceFormula.PayrollPriceFormulaId))
                                payrollPriceFormula = payrollPriceFormulaMapping[templatePayrollProductPriceFormula.PayrollPriceFormulaId];
                            if (payrollPriceFormula == null)
                                continue;

                            PayrollProductPriceFormula payrollProductPriceFormula = new PayrollProductPriceFormula()
                            {
                                FromDate = templatePayrollProductPriceFormula.FromDate,
                                ToDate = templatePayrollProductPriceFormula.ToDate,

                                //Set FK
                                PayrollPriceFormulaId = payrollPriceFormula.PayrollPriceFormulaId,

                                //Set reference
                                PayrollProductSetting = payrollProductSetting,
                            };
                            SetCreatedProperties(payrollProductPriceFormula);
                            entities.PayrollProductPriceFormula.AddObject(payrollProductPriceFormula);
                        }

                        #endregion

                        #region PayrollProductPriceType

                        foreach (PayrollProductPriceType templatePayrollProductPriceType in templatePayrollProductSetting.PayrollProductPriceType.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            PayrollPriceType payrollPriceType = null;
                            if (payrollPriceTypeMapping.ContainsKey(templatePayrollProductPriceType.PayrollPriceTypeId))
                                payrollPriceType = payrollPriceTypeMapping[templatePayrollProductPriceType.PayrollPriceTypeId];
                            if (payrollPriceType == null)
                                continue;

                            PayrollProductPriceType payrollProductPriceType = new PayrollProductPriceType()
                            {
                                //Set FK
                                PayrollPriceTypeId = payrollPriceType.PayrollPriceTypeId,

                                //Set reference
                                PayrollProductSetting = payrollProductSetting,
                            };
                            SetCreatedProperties(payrollProductPriceType);
                            entities.PayrollProductPriceType.AddObject(payrollProductPriceType);

                            #region PayrollProductPriceTypePeriod

                            foreach (PayrollProductPriceTypePeriod templatePayrollProductPriceTypePeriod in templatePayrollProductPriceType.PayrollProductPriceTypePeriod.Where(i => i.State == (int)SoeEntityState.Active))
                            {
                                PayrollProductPriceTypePeriod payrollProductPriceTypePeriod = new PayrollProductPriceTypePeriod()
                                {
                                    Amount = templatePayrollProductPriceTypePeriod.Amount,
                                    FromDate = templatePayrollProductPriceTypePeriod.FromDate,

                                    //Set references
                                    PayrollProductPriceType = payrollProductPriceType,
                                };
                                SetCreatedProperties(payrollProductPriceType);
                                entities.PayrollProductPriceTypePeriod.AddObject(payrollProductPriceTypePeriod);
                            }

                            #endregion
                        }

                        #endregion
                    }
                }

                #endregion

                #region Save

                result = SaveChanges(entities);
                if (!result.Success)
                {
                    result = LogCopyError("PayrollProductSetting", templateCompanyId, newCompanyId, saved: true);
                    return result;
                }

                #endregion

                #endregion

                #region InvoiceProduct

                foreach (InvoiceProduct templateInvoiceProduct in templateInvoiceProducts)
                {
                    #region InvoiceProduct

                    InvoiceProduct invoiceProduct = null;
                    if (update)
                        invoiceProduct = existingInvoiceProducts.FirstOrDefault(f => f.Name.ToLower() == templateInvoiceProduct.Name.ToLower() && f.Number.ToLower() == templateInvoiceProduct.Number.ToLower());// GetExistingInvoiceProduct(existingInvoiceProducts, templateInvoiceProducts, templateInvoiceProduct.ProductId);

                    if (invoiceProduct == null)
                    {
                        invoiceProduct = new InvoiceProduct();
                        SetCreatedProperties(invoiceProduct);
                    }
                    else
                    {
                        if (!invoiceProductMappingDict.ContainsKey(templateInvoiceProduct.ProductId))
                            invoiceProductMappingDict.Add(templateInvoiceProduct.ProductId, invoiceProduct);
                        continue; //accourding to rickard - too dangerous
                    }


                    //Product
                    invoiceProduct.Type = (int)SoeProductType.InvoiceProduct;
                    invoiceProduct.Number = templateInvoiceProduct.Number;
                    invoiceProduct.Name = templateInvoiceProduct.Name;
                    invoiceProduct.Description = templateInvoiceProduct.Description;
                    invoiceProduct.AccountingPrio = templateInvoiceProduct.AccountingPrio;

                    //InvoiceProduct
                    invoiceProduct.VatType = templateInvoiceProduct.VatType;
                    invoiceProduct.VatFree = templateInvoiceProduct.VatFree;
                    invoiceProduct.EAN = templateInvoiceProduct.EAN;
                    invoiceProduct.PurchasePrice = templateInvoiceProduct.PurchasePrice;
                    invoiceProduct.SysWholesellerName = templateInvoiceProduct.SysWholesellerName;
                    invoiceProduct.CalculationType = templateInvoiceProduct.CalculationType;
                    invoiceProduct.PriceListOrigin = templateInvoiceProduct.PriceListOrigin;
                    invoiceProduct.ShowDescriptionAsTextRow = templateInvoiceProduct.ShowDescriptionAsTextRow;
                    invoiceProduct.DontUseDiscountPercent = templateInvoiceProduct.DontUseDiscountPercent;
                    invoiceProduct.HouseholdDeductionPercentage = templateInvoiceProduct.HouseholdDeductionPercentage;
                    invoiceProduct.IsStockProduct = templateInvoiceProduct.IsStockProduct;
                    invoiceProduct.GuaranteePercentage = templateInvoiceProduct.GuaranteePercentage;
                    invoiceProduct.HouseholdDeductionType = templateInvoiceProduct.HouseholdDeductionType;
                    invoiceProduct.UseCalculatedCost = templateInvoiceProduct.UseCalculatedCost;
                    invoiceProduct.Weight = templateInvoiceProduct.Weight;
                    invoiceProduct.ShowDescrAsTextRowOnPurchase = templateInvoiceProduct.ShowDescrAsTextRowOnPurchase;

                    if (invoiceProduct.ProductId == 0)
                        newCompany.Product.Add(invoiceProduct);

                    if (!invoiceProductMappingDict.ContainsKey(templateInvoiceProduct.ProductId))
                        invoiceProductMappingDict.Add(templateInvoiceProduct.ProductId, invoiceProduct);

                    #endregion
                }

                #region Save

                result = SaveChanges(entities);
                if (!result.Success)
                {
                    result = LogCopyError("InvoiceProduct", templateCompanyId, newCompanyId, saved: true);
                    return result;
                }

                #endregion

                #endregion

                #region TimeCodeBreakGroup

                foreach (TimeCodeBreakGroup templateTimeCodeBreakGroup in templateTimeCodeBreakGroups)
                {
                    #region TimeCodeBreakGroup

                    TimeCodeBreakGroup timeCodeBreakGroup = update ? existingTimeCodeBreakGroups.FirstOrDefault(t => t.Name == templateTimeCodeBreakGroup.Name) : null;
                    if (timeCodeBreakGroup != null)
                        continue;
                    //accourding to rickard - too dangerous

                    timeCodeBreakGroup = new TimeCodeBreakGroup()
                    {
                        Name = templateTimeCodeBreakGroup.Name,
                        Description = templateTimeCodeBreakGroup.Description,

                        //Set references
                        Company = newCompany,
                    };
                    SetCreatedProperties(timeCodeBreakGroup);

                    existingTimeCodeBreakGroups.Add(timeCodeBreakGroup);

                    #endregion

                    #region Save

                    result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        LogCopyError("TimeCodeBreakGroup", templateCompanyId, newCompanyId, saved: true);
                        break;
                    }

                    #endregion

                    #region Set TimeCodeBreakGroup mapping

                    timeCodeBreakGroupMapping.Add(templateTimeCodeBreakGroup.TimeCodeBreakGroupId, timeCodeBreakGroup);

                    #endregion
                }

                #endregion

                #region TimeCode

                foreach (TimeCode templateTimeCode in templateTimeCodes)
                {
                    #region TimeCode

                    TimeCode timeCode = update ? existingTimeCodes.FirstOrDefault(t => t.Name == templateTimeCode.Name && t.Code == templateTimeCode.Code && t.Type == templateTimeCode.Type) : null;
                    if (timeCode == null)
                    {
                        switch (templateTimeCode.Type)
                        {
                            case (int)SoeTimeCodeType.Work:
                                #region TimeCodeWork

                                TimeCodeWork timeCodeWork = timeCode != null ? timeCode as TimeCodeWork : null;
                                if (timeCodeWork == null)
                                {
                                    timeCodeWork = new TimeCodeWork();
                                    SetCreatedProperties(timeCodeWork);
                                }
                                else
                                {
                                    SetModifiedProperties(timeCodeWork);
                                }

                                if (templateTimeCode is TimeCodeWork timeCodeWorkTemplate)
                                {
                                    timeCodeWork.IsWorkOutsideSchedule = timeCodeWorkTemplate.IsWorkOutsideSchedule;
                                }

                                timeCode = timeCodeWork;

                                #endregion
                                break;
                            case (int)SoeTimeCodeType.Absense:
                                #region TimeCodeAbsense

                                TimeCodeAbsense timeCodeAbsense = timeCode != null ? timeCode as TimeCodeAbsense : null;
                                if (timeCodeAbsense == null)
                                {
                                    timeCodeAbsense = new TimeCodeAbsense();
                                    SetCreatedProperties(timeCodeAbsense);
                                }
                                else
                                {
                                    SetModifiedProperties(timeCodeAbsense);
                                }

                                if (templateTimeCode is TimeCodeAbsense timeCodeAbsenceTemplate)
                                {
                                    timeCodeAbsense.IsAbsence = timeCodeAbsenceTemplate.IsAbsence;
                                    timeCodeAbsense.KontekId = timeCodeAbsenceTemplate.KontekId;
                                }

                                timeCode = timeCodeAbsense;

                                #endregion
                                break;
                            case (int)SoeTimeCodeType.Break:
                                #region TimeCodeBreak

                                TimeCodeBreak timeCodeBreak = timeCode != null ? timeCode as TimeCodeBreak : null;
                                if (timeCodeBreak == null)
                                {
                                    timeCodeBreak = new TimeCodeBreak();
                                    SetCreatedProperties(timeCodeBreak);
                                }
                                else
                                {
                                    SetModifiedProperties(timeCodeBreak);
                                }

                                if (templateTimeCode is TimeCodeBreak timeCodeBreakTemplate)
                                {
                                    timeCodeBreak.MinMinutes = timeCodeBreakTemplate.MinMinutes;
                                    timeCodeBreak.MaxMinutes = timeCodeBreakTemplate.MaxMinutes;
                                    timeCodeBreak.DefaultMinutes = timeCodeBreakTemplate.DefaultMinutes;
                                    timeCodeBreak.StartType = timeCodeBreakTemplate.StartType;
                                    timeCodeBreak.StopType = timeCodeBreakTemplate.StopType;
                                    timeCodeBreak.StartTimeMinutes = timeCodeBreakTemplate.StartTimeMinutes;
                                    timeCodeBreak.StopTimeMinutes = timeCodeBreakTemplate.StopTimeMinutes;
                                    timeCodeBreak.StartTime = timeCodeBreakTemplate.StartTime;
                                    if (timeCodeBreakTemplate.TimeCodeBreakGroupId.HasValue && timeCodeBreakGroupMapping.ContainsKey(timeCodeBreakTemplate.TimeCodeBreakGroupId.Value))
                                        timeCodeBreak.TimeCodeBreakGroupId = timeCodeBreakGroupMapping[timeCodeBreakTemplate.TimeCodeBreakGroupId.Value]?.TimeCodeBreakGroupId;

                                    //EmployeeGroups for TimeCodeBreaks are copied in EmpolyeeGroup copy
                                }

                                timeCode = timeCodeBreak;

                                #endregion
                                break;
                            case (int)SoeTimeCodeType.AdditionDeduction:
                                #region AdditionAndDeduction

                                TimeCodeAdditionDeduction timeCodeAdditionDeduction = timeCode != null ? timeCode as TimeCodeAdditionDeduction : null;
                                if (timeCodeAdditionDeduction == null)
                                {
                                    timeCodeAdditionDeduction = new TimeCodeAdditionDeduction();
                                    SetCreatedProperties(timeCodeAdditionDeduction);
                                }
                                else
                                {
                                    SetModifiedProperties(timeCodeAdditionDeduction);
                                }

                                if (templateTimeCode is TimeCodeAdditionDeduction timeCodeAdditionDeductionTemplate)
                                {
                                    timeCodeAdditionDeduction.ExpenseType = timeCodeAdditionDeductionTemplate?.ExpenseType ?? (int)TermGroup_ExpenseType.Time;
                                    timeCodeAdditionDeduction.Comment = timeCodeAdditionDeductionTemplate?.Comment ?? string.Empty;
                                    timeCodeAdditionDeduction.StopAtDateStart = timeCodeAdditionDeductionTemplate?.StopAtDateStart ?? false;
                                    timeCodeAdditionDeduction.StopAtDateStop = timeCodeAdditionDeductionTemplate?.StopAtDateStop ?? false;
                                    timeCodeAdditionDeduction.StopAtPrice = timeCodeAdditionDeductionTemplate?.StopAtPrice ?? false;
                                    timeCodeAdditionDeduction.StopAtVat = timeCodeAdditionDeductionTemplate?.StopAtVat ?? false;
                                    timeCodeAdditionDeduction.StopAtAccounting = timeCodeAdditionDeductionTemplate?.StopAtAccounting ?? false;
                                    timeCodeAdditionDeduction.StopAtComment = timeCodeAdditionDeductionTemplate?.StopAtComment ?? false;
                                    timeCodeAdditionDeduction.CommentMandatory = timeCodeAdditionDeductionTemplate?.CommentMandatory ?? false;
                                    timeCodeAdditionDeduction.HideForEmployee = timeCodeAdditionDeductionTemplate?.HideForEmployee ?? false;
                                    timeCodeAdditionDeduction.ShowInTerminal = timeCodeAdditionDeductionTemplate?.ShowInTerminal ?? false;
                                    timeCodeAdditionDeduction.FixedQuantity = timeCodeAdditionDeductionTemplate?.FixedQuantity ?? null;
                                }

                                timeCode = timeCodeAdditionDeduction;

                                #endregion
                                break;
                            case (int)SoeTimeCodeType.Material:
                                #region TimeCodeMaterial

                                TimeCodeMaterial timeCodeMaterial = timeCode != null ? timeCode as TimeCodeMaterial : null;
                                if (timeCodeMaterial == null)
                                {
                                    timeCodeMaterial = new TimeCodeMaterial();
                                    SetCreatedProperties(timeCodeMaterial);
                                }
                                else
                                {
                                    SetModifiedProperties(timeCodeMaterial);
                                }

                                if (templateTimeCode is TimeCodeMaterial timeCodeMaterialTemplate)
                                {
                                    timeCodeMaterial.Note = timeCodeMaterialTemplate.Note;
                                }

                                timeCode = timeCodeMaterial;

                                #endregion
                                break;
                        }

                        if (timeCode == null)
                            continue;

                        #region Base

                        timeCode.Company = newCompany;
                        timeCode.Code = templateTimeCode.Code;
                        timeCode.Name = templateTimeCode.Name;
                        timeCode.Description = templateTimeCode.Description;
                        timeCode.Type = templateTimeCode.Type;
                        timeCode.RegistrationType = templateTimeCode.RegistrationType;
                        timeCode.Classification = templateTimeCode.Classification;
                        timeCode.Payed = templateTimeCode.Payed;
                        timeCode.MinutesByConstantRules = templateTimeCode.MinutesByConstantRules;
                        timeCode.FactorBasedOnWorkPercentage = templateTimeCode.FactorBasedOnWorkPercentage;                        

                        //Rounding
                        timeCode.RoundingType = templateTimeCode.RoundingType;
                        timeCode.RoundingValue = templateTimeCode.RoundingValue;
                        timeCode.RoundingTimeCodeId = null; //Cannot be set due to TimeCodes are copied later and dependent on TimeCode..
                        timeCode.RoundingGroupKey = null; //Cannot be set due to TimeCodes are copied later and dependent on TimeCode..
                        timeCode.RoundStartTime = templateTimeCode.RoundStartTime;

                        //Adjustment
                        timeCode.AdjustQuantityByBreakTime = templateTimeCode.AdjustQuantityByBreakTime;
                        timeCode.AdjustQuantityTimeScheduleTypeId = null; //Cannot be set due to TimeScheduleTypes are copied later and dependent on TimeCode..
                        timeCode.AdjustQuantityTimeCodeId = null; //Cannot be set due to TimeCodes are copied later and dependent on TimeCode..

                        #endregion

                        #region TimeCodePayrollProduct

                        if (!templateTimeCode.TimeCodePayrollProduct.IsLoaded)
                            templateTimeCode.TimeCodePayrollProduct.Load();

                        if (timeCode.TimeCodeId != 0 && !timeCode.TimeCodePayrollProduct.IsLoaded)
                            timeCode.TimeCodePayrollProduct.Load();

                        if (timeCode.TimeCodePayrollProduct != null && timeCode.TimeCodePayrollProduct.Count == 0)
                        {
                            foreach (TimeCodePayrollProduct templateTimeCodePayrollProduct in templateTimeCode.TimeCodePayrollProduct)
                            {
                                if (!payrollProductMappingDict.ContainsKey(templateTimeCodePayrollProduct.ProductId))
                                    continue;

                                PayrollProduct payrollProduct = payrollProductMappingDict[templateTimeCodePayrollProduct.ProductId];
                                if (payrollProduct != null)
                                {
                                    TimeCodePayrollProduct timeCodePayrollProduct = new TimeCodePayrollProduct()
                                    {
                                        Factor = templateTimeCodePayrollProduct.Factor,

                                        //Set references
                                        TimeCode = timeCode,
                                        PayrollProduct = payrollProduct,
                                    };
                                    SetCreatedProperties(timeCodePayrollProduct);
                                    entities.TimeCodePayrollProduct.AddObject(timeCodePayrollProduct);
                                }
                            }
                        }

                        

                        #endregion

                        #region Save

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            LogCopyError("TimeCode", templateCompanyId, newCompanyId, saved: true);
                            break;
                        }

                        #endregion
                    }

                    #region TimeCodeInvoiceProduct

                    if (!templateTimeCode.TimeCodeInvoiceProduct.IsLoaded)
                        templateTimeCode.TimeCodeInvoiceProduct.Load();

                    if (timeCode.TimeCodeId != 0 && !timeCode.TimeCodeInvoiceProduct.IsLoaded)
                        timeCode.TimeCodeInvoiceProduct.Load();

                    if (timeCode.TimeCodeInvoiceProduct != null && timeCode.TimeCodeInvoiceProduct.Count == 0)
                    {
                        foreach (TimeCodeInvoiceProduct templateTimeCodeInvoiceProduct in templateTimeCode.TimeCodeInvoiceProduct)
                        {
                            //if (!invoiceProductMappingDict.ContainsKey(templateTimeCodeInvoiceProduct.ProductId))
                            //    continue;

                            InvoiceProduct invoiceProduct = invoiceProductMappingDict[templateTimeCodeInvoiceProduct.ProductId];
                            if (invoiceProduct != null)
                            {
                                TimeCodeInvoiceProduct timeCodeInvoiceProduct = new TimeCodeInvoiceProduct()
                                {
                                    Factor = templateTimeCodeInvoiceProduct.Factor,

                                    //Set references
                                    TimeCode = timeCode,
                                    InvoiceProduct = invoiceProduct,
                                };
                                SetCreatedProperties(timeCodeInvoiceProduct);
                                entities.TimeCodeInvoiceProduct.AddObject(timeCodeInvoiceProduct);
                            }
                        }
                        #region Save

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            LogCopyError("InvoiceProduct", templateCompanyId, newCompanyId, saved: true);
                            break;
                        }

                        #endregion
                    }

                    #endregion

                    #endregion

                    #region Set TempCompTimeCodeMapping

                    timeCodeMapping.Add(templateTimeCode.TimeCodeId, timeCode);

                    #endregion
                }

                #region Settings

                var templateCompanyTimeCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.ProjectDefaultTimeCodeId, 0, templateCompanyId, 0);
                if (templateCompanyTimeCodeId > 0 && copyProjectSettings)
                {
                    var mappedTimeCode = timeCodeMapping.GetValue(templateCompanyTimeCodeId);
                    if (mappedTimeCode != null)
                        SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.ProjectDefaultTimeCodeId, mappedTimeCode.TimeCodeId, 0, newCompanyId, 0);
                }

                #endregion

                #region Rounding / Adjustment (Must be done after all timeCodes are copied)

                foreach (TimeCode templateTimeCode in templateTimeCodes.Where(x => x.RoundingTimeCodeId.HasValue || x.RoundingInterruptionTimeCodeId.HasValue || x.AdjustQuantityTimeCodeId.HasValue))
                {
                    if (!timeCodeMapping.TryGetValue(templateTimeCode.TimeCodeId, out TimeCode timeCode) || timeCode == null)
                        continue;

                    if (templateTimeCode.RoundingTimeCodeId.HasValue)
                        timeCode.RoundingTimeCodeId = timeCodeMapping.GetValue(templateTimeCode.RoundingTimeCodeId.Value)?.TimeCodeId;
                    if (templateTimeCode.RoundingInterruptionTimeCodeId.HasValue)
                        timeCode.RoundingInterruptionTimeCodeId = timeCodeMapping.GetValue(templateTimeCode.RoundingInterruptionTimeCodeId.Value)?.TimeCodeId;
                    if (templateTimeCode.AdjustQuantityTimeCodeId.HasValue)
                        timeCode.AdjustQuantityTimeCodeId = timeCodeMapping.GetValue(templateTimeCode.AdjustQuantityTimeCodeId.Value)?.TimeCodeId;
                }

                #endregion

                #region TimeCodeRule (Must be done after all timeCodes are copied)

                foreach (TimeCode templateTimeCode in templateTimeCodes.Where(x => x.IsBreak() || x.IsWork() || x.IsAbsence()))
                {
                    #region TimeCodeRule

                    if (!templateTimeCode.TimeCodeRule.IsLoaded)
                        templateTimeCode.TimeCodeRule.Load();

                    if (!timeCodeMapping.ContainsKey(templateTimeCode.TimeCodeId))
                        continue;

                    TimeCode timeCode = timeCodeMapping[templateTimeCode.TimeCodeId];
                    if (timeCode == null)
                        continue;

                    if (!timeCode.TimeCodeRule.IsLoaded)
                        timeCode.TimeCodeRule.Load();
                    while (timeCode.TimeCodeRule.Any())
                    {
                        entities.DeleteObject(timeCode.TimeCodeRule.First());
                    }
                    timeCode.TimeCodeRule.Clear();

                    foreach (var templateTimeCodeRule in templateTimeCode.TimeCodeRule)
                    {
                        if (!timeCodeMapping.ContainsKey(templateTimeCodeRule.Value))
                            continue;

                        TimeCode timeCodeForRule = timeCodeMapping[templateTimeCodeRule.Value];
                        if (timeCodeForRule == null)
                            continue;

                        TimeCodeRule rule = new TimeCodeRule()
                        {
                            Type = templateTimeCodeRule.Type,
                            Value = timeCodeForRule.TimeCodeId,
                            Time = templateTimeCodeRule.Time,

                            //Set reference
                            TimeCode = timeCode,

                        };
                        entities.TimeCodeRule.AddObject(rule);
                    }

                    #endregion
                }

                #endregion

                #endregion

                #region TimeBreakTemplate

                if (copyBreakTemplates)
                {
                    foreach (TimeBreakTemplate existingTimeBreakTemplate in existingTimeBreakTemplates)
                    {
                        ChangeEntityState(existingTimeBreakTemplate, SoeEntityState.Deleted);
                    }

                    foreach (TimeBreakTemplate templateTimeBreakTemplate in templateTimeBreakTemplateRules)
                    {
                        #region TimeBreakTemplate

                        TimeBreakTemplate timeBreakTemplate = new TimeBreakTemplate()
                        {
                            StartDate = templateTimeBreakTemplate.StartDate,
                            StopDate = templateTimeBreakTemplate.StopDate,
                            UseMaxWorkTimeBetweenBreaks = templateTimeBreakTemplate.UseMaxWorkTimeBetweenBreaks,
                            ShiftLength = templateTimeBreakTemplate.ShiftLength,
                            ShiftStartFromTime = templateTimeBreakTemplate.ShiftStartFromTime,
                            MinTimeBetweenBreaks = templateTimeBreakTemplate.MinTimeBetweenBreaks,
                            DayOfWeeks = templateTimeBreakTemplate.DayOfWeeks,

                            //Set FK
                            ActorCompanyId = newCompanyId,
                        };
                        SetCreatedProperties(timeBreakTemplate);
                        entities.TimeBreakTemplate.AddObject(timeBreakTemplate);

                        #endregion

                        #region ShiftTypes

                        if (!templateTimeBreakTemplate.ShiftTypes.IsNullOrEmpty())
                        {
                            foreach (ShiftType templateShiftType in templateTimeBreakTemplate.ShiftTypes)
                            {
                                ShiftType existingShiftType = existingShiftTypes.FirstOrDefault(i => i.Name == templateShiftType.Name);
                                if (existingShiftType != null)
                                    timeBreakTemplate.ShiftTypes.Add(existingShiftType);
                            }
                        }

                        #endregion

                        #region TimeBreakTemplateRows

                        if (!templateTimeBreakTemplate.TimeBreakTemplateRow.IsNullOrEmpty())
                        {
                            foreach (TimeBreakTemplateRow templateTimeBreakTemplateRow in templateTimeBreakTemplate.TimeBreakTemplateRow.Where(w => w.State == (int)SoeEntityState.Active))
                            {
                                TimeCodeBreakGroup templateTimeCodeBreakGroup = templateTimeCodeBreakGroups.FirstOrDefault(w => w.TimeCodeBreakGroupId == templateTimeBreakTemplateRow.TimeCodeBreakGroupId);
                                if (templateTimeCodeBreakGroup == null)
                                    continue;

                                TimeCodeBreakGroup existingTimeCodeBreakGroup = existingTimeCodeBreakGroups.FirstOrDefault(i => i.Name == templateTimeCodeBreakGroup.Name);
                                if (existingTimeCodeBreakGroup == null)
                                    continue;

                                TimeBreakTemplateRow timeBreakTemplateRow = new TimeBreakTemplateRow()
                                {
                                    Type = templateTimeBreakTemplateRow.Type,
                                    MinTimeAfterStart = templateTimeBreakTemplateRow.MinTimeAfterStart,
                                    MinTimeBeforeEnd = templateTimeBreakTemplateRow.MinTimeBeforeEnd,

                                    //References
                                    TimeCodeBreakGroup = existingTimeCodeBreakGroup
                                };
                                SetCreatedProperties(timeBreakTemplateRow);
                                timeBreakTemplate.TimeBreakTemplateRow.Add(timeBreakTemplateRow);
                            }
                        }

                        #endregion

                        #region Save

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            LogCopyError("TimeBreakTemplate", templateCompanyId, newCompanyId, saved: true);
                            break;
                        }

                        #endregion

                    }
                }

                #endregion

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("TimeCode", templateCompanyId, newCompanyId, saved: true);
            }

            return result;
        }

        public ActionResult CopyTimeDeviationCausesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool mapDeviationCauseOnVacationGroup, ref Dictionary<int, TimeDeviationCause> timeDeviationCausesMapping, ref Dictionary<int, TimeCode> timeCodeMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            List<TimeDeviationCause> templateTimeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(templateCompanyId);
            List<VacationGroup> templateVacationGroups = mapDeviationCauseOnVacationGroup ? PayrollManager.GetVacationGroups(templateCompanyId, loadExternalCode: true) : null;

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Always load, could have been added earlier
                List<TimeDeviationCause> existingTimeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, newCompanyId);
                List<TimeCode> existingTimeCodes = TimeCodeManager.GetTimeCodes(entities, newCompanyId);

                List<Tuple<int, int>> vacationGroupTimeDeviation = new List<Tuple<int, int>>();
                if (mapDeviationCauseOnVacationGroup)
                {
                    var templates = templateVacationGroups.Where(w => w.VacationGroupSE != null).SelectMany(s => s.VacationGroupSE).Where(w => w.ReplacementTimeDeviationCauseId.HasValue).ToList();

                    if (!templates.IsNullOrEmpty())
                    {
                        foreach (var item in templates)
                        {
                            var matching = entities.VacationGroupSE.FirstOrDefault(w => w.VacationGroup.ActorCompanyId == newCompanyId && item.VacationGroup.Name == w.VacationGroup.Name);

                            if (matching != null)
                            {
                                vacationGroupTimeDeviation.Add(Tuple.Create(item.ReplacementTimeDeviationCauseId.Value, matching.VacationGroupId));
                            }
                        }
                    }
                }

                #endregion

                foreach (TimeDeviationCause templateTimeDeviationCause in templateTimeDeviationCauses)
                {
                    #region TimeDeviationCause

                    TimeDeviationCause timeDeviationCause = existingTimeDeviationCauses.FirstOrDefault(t => t.Name == templateTimeDeviationCause.Name ||
                        (!string.IsNullOrEmpty(t.ExtCode) && !string.IsNullOrEmpty(templateTimeDeviationCause.ExtCode) && t.ExtCode == templateTimeDeviationCause.ExtCode));

                    if (timeDeviationCause == null)
                    {
                        #region Add

                        timeDeviationCause = new TimeDeviationCause();
                        SetCreatedProperties(timeDeviationCause);

                        newCompany.TimeDeviationCause.Add(timeDeviationCause);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        SetModifiedProperties(timeDeviationCause);

                        #endregion
                    }

                    #region Common

                    timeDeviationCause.Type = templateTimeDeviationCause.Type;
                    timeDeviationCause.Name = templateTimeDeviationCause.Name;
                    timeDeviationCause.Description = templateTimeDeviationCause.Description;
                    timeDeviationCause.ExtCode = templateTimeDeviationCause.ExtCode;
                    timeDeviationCause.ImageSource = templateTimeDeviationCause.ImageSource;
                    timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBefore = templateTimeDeviationCause.EmployeeRequestPolicyNbrOfDaysBefore;
                    timeDeviationCause.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride = templateTimeDeviationCause.EmployeeRequestPolicyNbrOfDaysBeforeCanOverride;
                    timeDeviationCause.AttachZeroDaysNbrOfDaysBefore = templateTimeDeviationCause.AttachZeroDaysNbrOfDaysBefore;
                    timeDeviationCause.AttachZeroDaysNbrOfDaysAfter = templateTimeDeviationCause.AttachZeroDaysNbrOfDaysAfter;
                    timeDeviationCause.ChangeDeviationCauseAccordingToPlannedAbsence = templateTimeDeviationCause.ChangeDeviationCauseAccordingToPlannedAbsence;
                    timeDeviationCause.ChangeCauseOutsideOfPlannedAbsence = templateTimeDeviationCause.ChangeCauseOutsideOfPlannedAbsence;
                    timeDeviationCause.ChangeCauseInsideOfPlannedAbsence = templateTimeDeviationCause.ChangeCauseInsideOfPlannedAbsence;
                    timeDeviationCause.AdjustTimeOutsideOfPlannedAbsence = templateTimeDeviationCause.AdjustTimeOutsideOfPlannedAbsence;
                    timeDeviationCause.AdjustTimeInsideOfPlannedAbsence = templateTimeDeviationCause.AdjustTimeInsideOfPlannedAbsence;
                    timeDeviationCause.AllowGapToPlannedAbsence = templateTimeDeviationCause.AllowGapToPlannedAbsence;
                    timeDeviationCause.ShowZeroDaysInAbsencePlanning = templateTimeDeviationCause.ShowZeroDaysInAbsencePlanning;
                    timeDeviationCause.IsVacation = templateTimeDeviationCause.IsVacation;
                    timeDeviationCause.Payed = templateTimeDeviationCause.Payed;
                    timeDeviationCause.NotChargeable = templateTimeDeviationCause.NotChargeable;
                    timeDeviationCause.OnlyWholeDay = templateTimeDeviationCause.OnlyWholeDay;
                    timeDeviationCause.SpecifyChild = templateTimeDeviationCause.SpecifyChild;
                    timeDeviationCause.ExcludeFromPresenceWorkRules = templateTimeDeviationCause.ExcludeFromPresenceWorkRules;
                    timeDeviationCause.ExcludeFromScheduleWorkRules = templateTimeDeviationCause.ExcludeFromScheduleWorkRules;
                    timeDeviationCause.ValidForHibernating = templateTimeDeviationCause.ValidForHibernating;
                    timeDeviationCause.ValidForStandby = templateTimeDeviationCause.ValidForStandby;
                    timeDeviationCause.CandidateForOvertime = templateTimeDeviationCause.CandidateForOvertime;
                    timeDeviationCause.MandatoryNote = templateTimeDeviationCause.MandatoryNote;
                    timeDeviationCause.MandatoryTime = templateTimeDeviationCause.MandatoryTime;
                    timeDeviationCause.State = templateTimeDeviationCause.State;
                    timeDeviationCause.CalculateAsOtherTimeInSales = templateTimeDeviationCause.CalculateAsOtherTimeInSales;

                    if (templateTimeDeviationCause.TimeCodeId.HasValue && timeCodeMapping.ContainsKey(templateTimeDeviationCause.TimeCodeId.Value))
                    {
                        TimeCode timeCode = timeCodeMapping[templateTimeDeviationCause.TimeCodeId.Value];
                        if (timeCode != null)
                            timeDeviationCause.TimeCodeId = existingTimeCodes.FirstOrDefault(i => i.TimeCodeId == timeCode.TimeCodeId)?.TimeCodeId;
                    }

                    timeDeviationCausesMapping.Add(templateTimeDeviationCause.TimeDeviationCauseId, timeDeviationCause);

                    #endregion

                    #endregion

                    result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        result = LogCopyError("TimeDeviationCause", templateCompanyId, newCompanyId, saved: true);
                        break;
                    }

                    if (mapDeviationCauseOnVacationGroup && vacationGroupTimeDeviation.Any(a => a.Item1 == templateTimeDeviationCause.TimeDeviationCauseId))
                    {
                        var matches = vacationGroupTimeDeviation.Where(a => a.Item1 == templateTimeDeviationCause.TimeDeviationCauseId);

                        foreach (var match in matches)
                        {
                            if (match != null)
                            {
                                var group = entities.VacationGroupSE.FirstOrDefault(f => f.VacationGroupId == match.Item2);

                                if (group != null && !group.ReplacementTimeDeviationCauseId.HasValue)
                                {
                                    group.ReplacementTimeDeviationCauseId = timeDeviationCause.TimeDeviationCauseId;
                                    SaveChanges(entities);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public ActionResult CopyDaytypesHalfDaysAndHolidaysFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, DayType> dayTypeMapping)
        {
            ActionResult result;

            #region Prereq

            List<DayType> templateDaytypes = CalendarManager.GetDayTypesByCompany(templateCompanyId);
            List<TimeHalfday> templateHalfDays = CalendarManager.GetTimeHalfdays(templateCompanyId, false).ToList();
            List<HolidayDTO> templateHolidays = CalendarManager.GetHolidaysByCompany(templateCompanyId, loadDayType: true).ToList();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Existing for new Company
                List<DayType> existingDayTypes = new List<DayType>();
                List<TimeHalfday> existingTimeHalfDays = new List<TimeHalfday>();
                List<HolidayDTO> existingHolidays = new List<HolidayDTO>();

                if (update)
                {
                    existingDayTypes = CalendarManager.GetDayTypesByCompany(entities, newCompanyId);
                    existingTimeHalfDays = CalendarManager.GetTimeHalfdays(entities, newCompanyId, false).ToList();
                    existingHolidays = CalendarManager.GetHolidaysByCompany(entities, newCompanyId, loadDayType: true).ToList();
                }
                #endregion

                #region DayTypes

                foreach (DayType templateDaytype in templateDaytypes)
                {
                    #region DayType

                    DayType dayType = update ? existingDayTypes.FirstOrDefault(d => d.Name == templateDaytype.Name) : null;
                    if (dayType == null)
                    {
                        #region Add

                        dayType = new DayType();
                        AddDayTypeFromTemplateToNewCompany(newCompany, ref dayType, templateDaytype);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        UpdateExistingDayType(ref dayType, templateDaytype);

                        #endregion
                    }

                    dayTypeMapping.Add(templateDaytype.DayTypeId, dayType);

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    LogCopyError("DayTypes", templateCompanyId, newCompanyId, saved: true);

                //Get existing DayTypes again after save
                if (update)
                    existingDayTypes = CalendarManager.GetDayTypesByCompany(entities, newCompanyId);

                #endregion

                #region HalfDays

                foreach (TimeHalfday templateHalfDay in templateHalfDays)
                {
                    #region HalfDay

                    TimeHalfday timeHalfDay = update ? existingTimeHalfDays.FirstOrDefault(h => h.Name == templateHalfDay.Name) : null;
                    DayType existingDaytype = update && templateHalfDay.DayType != null ? existingDayTypes.FirstOrDefault(d => d.Name == templateHalfDay.DayType.Name) : null;
                    if (timeHalfDay == null)
                    {
                        #region Add

                        timeHalfDay = new TimeHalfday();
                        SetCreatedProperties(timeHalfDay);
                        timeHalfDay.CopyFrom(templateHalfDay);

                        if (templateHalfDay.DayType != null)
                        {
                            if (existingDaytype == null)
                                AddDayTypeFromTemplateToNewCompany(newCompany, ref existingDaytype, templateHalfDay.DayType);
                            else
                                UpdateExistingDayType(ref existingDaytype, templateHalfDay.DayType);

                            timeHalfDay.DayType = existingDaytype;
                        }

                        #endregion
                    }
                    else
                    {
                        #region Update

                        timeHalfDay.CopyFrom(templateHalfDay);
                        SetModifiedProperties(timeHalfDay);

                        if (templateHalfDay.DayType != null)
                        {
                            if (timeHalfDay.DayType != null)
                            {
                                existingDaytype = timeHalfDay.DayType;
                                UpdateExistingDayType(ref existingDaytype, templateHalfDay.DayType);
                            }
                            else
                            {
                                if (existingDaytype == null)
                                    AddDayTypeFromTemplateToNewCompany(newCompany, ref existingDaytype, templateHalfDay.DayType);
                                else
                                    UpdateExistingDayType(ref existingDaytype, templateHalfDay.DayType);

                                timeHalfDay.DayType = existingDaytype;
                            }
                        }

                        #endregion
                    }

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    LogCopyError("TimeHalfDay", templateCompanyId, newCompanyId, saved: true);

                #endregion

                #region Holidays

                foreach (var templateHolidaysById in templateHolidays.GroupBy(g => g.HolidayId))
                {
                    #region Holiday

                    HolidayDTO templateHoliday = templateHolidaysById.First();

                    Holiday holiday = null;
                    DayType dayType = null;
                    if (update && existingHolidays.Any())
                    {
                        holiday = existingHolidays.FirstOrDefault(h => h.Name == templateHoliday.Name).FromDTO();
                        if (templateHoliday.DayType != null)
                            dayType = existingDayTypes.FirstOrDefault(d => d.Name == templateHoliday.DayType.Name);
                    }

                    if (holiday == null)
                    {
                        #region Add

                        holiday = new Holiday();

                        holiday.CopyFrom(templateHoliday.FromDTO());

                        if (holiday.SysHolidayTypeId.HasValue)
                            holiday.Date = CalendarUtility.DATETIME_DEFAULT;

                        SetCreatedProperties(holiday);
                        newCompany.Holiday.Add(holiday);

                        if (templateHoliday.DayType != null)
                        {
                            if (dayType == null)
                                dayType = existingDayTypes.FirstOrDefault(d => d.Name == templateHoliday.DayType.Name);

                            if (dayType == null)
                                AddDayTypeFromTemplateToNewCompany(newCompany, ref dayType, templateDaytypes.FirstOrDefault(w => w.DayTypeId == templateHoliday.DayType.DayTypeId));
                            else
                                UpdateExistingDayType(ref dayType, templateDaytypes.FirstOrDefault(w => w.DayTypeId == templateHoliday.DayType.DayTypeId));

                            holiday.DayType = dayType;
                        }
                        #endregion
                    }
                    else
                    {
                        #region Update

                        holiday.CopyFrom(templateHoliday.FromDTO());
                        SetModifiedProperties(holiday);

                        if (templateHoliday.DayType != null)
                        {
                            if (holiday.DayType != null)
                            {
                                dayType = holiday.DayType;
                                UpdateExistingDayType(ref dayType, templateDaytypes.FirstOrDefault(w => w.DayTypeId == templateHoliday.DayType.DayTypeId));
                            }
                            else
                            {
                                if (dayType == null)
                                    AddDayTypeFromTemplateToNewCompany(newCompany, ref dayType, templateDaytypes.FirstOrDefault(w => w.DayTypeId == templateHoliday.DayType.DayTypeId));
                                else
                                    UpdateExistingDayType(ref dayType, templateDaytypes.FirstOrDefault(w => w.DayTypeId == templateHoliday.DayType.DayTypeId));

                                holiday.DayType = dayType;
                            }
                        }

                        #endregion
                    }

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("Holiday", templateCompanyId, newCompanyId, saved: true);

                #endregion
            }

            return result;
        }

        public ActionResult CopyTimePeriodsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            #region Prereq

            List<TimePeriodHead> templateTimePeriodHeads = TimePeriodManager.GetTimePeriodHeads(templateCompanyId, TermGroup_TimePeriodType.Unknown, false, false);
            if (templateTimePeriodHeads.Count == 0)
                return new ActionResult(true);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<TimePeriodHead> existingTimePeriodHeads = TimePeriodManager.GetTimePeriodHeads(newCompanyId, TermGroup_TimePeriodType.Unknown, false, false);

                #endregion

                #region TimePeriodHead

                foreach (TimePeriodHead templateTimePeriodHead in templateTimePeriodHeads)
                {
                    #region TimePeriodHead

                    TimePeriodHead timePeriodHead = existingTimePeriodHeads.FirstOrDefault(pt => pt.Name == templateTimePeriodHead.Name);
                    if (timePeriodHead == null)
                    {
                        timePeriodHead = new TimePeriodHead();
                        SetCreatedProperties(timePeriodHead, user);
                        entities.TimePeriodHead.AddObject(timePeriodHead);
                    }
                    else
                    {
                        SetModifiedProperties(timePeriodHead, user);
                    }

                    timePeriodHead.TimePeriodType = templateTimePeriodHead.TimePeriodType;
                    timePeriodHead.Name = templateTimePeriodHead.Name;
                    timePeriodHead.Description = templateTimePeriodHead.Description;

                    //Set FK
                    timePeriodHead.ActorCompanyId = newCompanyId;

                    if (timePeriodHead.TimePeriod == null)
                        timePeriodHead.TimePeriod = new EntityCollection<TimePeriod>();

                    if (!templateTimePeriodHead.TimePeriod.IsLoaded)
                        templateTimePeriodHead.TimePeriod.Load();

                    foreach (TimePeriod templateTimePeriod in templateTimePeriodHead.TimePeriod.Where(x => x.State == (int)SoeEntityState.Active))
                    {
                        #region TimePeriod

                        TimePeriod timePeriod = timePeriodHead.TimePeriod.FirstOrDefault(pt => pt.Name == templateTimePeriodHead.Name);
                        if (timePeriod == null)
                        {
                            timePeriod = new TimePeriod();
                            SetCreatedProperties(timePeriod, user);
                            timePeriodHead.TimePeriod.Add(timePeriod);
                        }
                        else
                        {
                            SetModifiedProperties(timePeriod, user);
                        }

                        timePeriod.RowNr = templateTimePeriod.RowNr;
                        timePeriod.Name = templateTimePeriod.Name;
                        timePeriod.StartDate = templateTimePeriod.StartDate;
                        timePeriod.StopDate = templateTimePeriod.StopDate;
                        timePeriod.PayrollStartDate = templateTimePeriod.PayrollStartDate;
                        timePeriod.PayrollStopDate = templateTimePeriod.PayrollStopDate;
                        timePeriod.PaymentDate = templateTimePeriod.PaymentDate;

                        #endregion
                    }

                    #endregion
                }

                #endregion

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("PayrollPrice", templateCompanyId, newCompanyId, saved: true);

                return result;
            }
        }

        public ActionResult CopyPositionsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            ActionResult result;

            #region Prereq

            var templatePositions = EmployeeManager.GetPositions(templateCompanyId, true);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                var existingPositions = EmployeeManager.GetPositions(entities, newCompanyId, true);

                #endregion

                foreach (var templatePosition in templatePositions)
                {
                    #region Position

                    var pos = existingPositions.FirstOrDefault(p => p.Name == templatePosition.Name && p.Code == templatePosition.Code);
                    if (pos == null)
                    {
                        pos = new Position();
                        SetCreatedProperties(pos, user);
                        entities.Position.AddObject(pos);
                    }
                    else
                    {
                        SetModifiedProperties(pos, user);
                    }

                    pos.Code = templatePosition.Code;
                    pos.Name = templatePosition.Name;
                    pos.Description = templatePosition.Description;
                    pos.ActorCompanyId = newCompanyId;
                    pos.SysPositionId = templatePosition.SysPositionId;

                    #endregion

                    #region PositionSkill

                    // Only supports adding, no updating
                    if (pos.PositionSkill == null)
                        pos.PositionSkill = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<PositionSkill>();
                    else
                    {
                        while (pos.PositionSkill.Any())
                        {
                            entities.DeleteObject(pos.PositionSkill.First());
                        }

                        pos.PositionSkill.Clear();
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = LogCopyError("Positions", templateCompanyId, newCompanyId, saved: true);

                    if (result.Success)
                    {
                        foreach (var tempPosSkill in templatePosition.PositionSkill)
                        {
                            if (!tempPosSkill.SkillReference.IsLoaded)
                                tempPosSkill.SkillReference.Load();
                            if (!tempPosSkill.Skill.SkillTypeReference.IsLoaded)
                                tempPosSkill.Skill.SkillTypeReference.Load();

                            var skill = TimeScheduleManager.GetSkill(entities, newCompanyId, tempPosSkill.Skill.Name);
                            var skillType = skill?.SkillType;

                            if (skill == null)
                            {
                                if (tempPosSkill.Skill.SkillType != null)
                                {
                                    skillType = new SkillType()
                                    {
                                        ActorCompanyId = newCompanyId,
                                        Description = tempPosSkill.Skill.SkillType.Description,
                                        Name = tempPosSkill.Skill.SkillType.Name,
                                    };
                                    SetCreatedProperties(skillType, user);
                                }

                                skill = new Skill()
                                {
                                    ActorCompanyId = newCompanyId,
                                    Description = tempPosSkill.Skill.Description,
                                    Name = tempPosSkill.Skill.Name,
                                    SkillType = skillType,
                                };
                                SetCreatedProperties(skill, user);
                            }

                            var positionSkill = new PositionSkill()
                            {
                                Skill = skill,
                                SkillLevel = tempPosSkill.SkillLevel,
                                Position = pos,
                            };
                            SetCreatedProperties(positionSkill);
                        }
                    }

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("Positions", templateCompanyId, newCompanyId, saved: true);

                return result;
            }
        }

        public ActionResult CopyVacationGroupsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping, ref Dictionary<int, PayrollPriceType> payrollPriceTypeMapping, ref Dictionary<int, AccountStd> accountStdMapping, ref Dictionary<int, VacationGroup> vacationGroupMapping)
        {
            List<VacationGroup> templateVacationGroups = PayrollManager.GetVacationGroupsWithVacationGroupSE(templateCompanyId);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<VacationGroup> existingVacationGroups = PayrollManager.GetVacationGroupsWithVacationGroupSE(entities, newCompanyId);

                #endregion

                foreach (VacationGroup templateVacationGroup in templateVacationGroups)
                {
                    #region VacationGroup

                    VacationGroup vacationGroup = existingVacationGroups.FirstOrDefault(v => v.Name == templateVacationGroup.Name);
                    if (vacationGroup == null)
                    {
                        vacationGroup = new VacationGroup()
                        {
                            //Set FK
                            ActorCompanyId = newCompanyId,
                        };
                        SetCreatedProperties(vacationGroup, user);
                        entities.VacationGroup.AddObject(vacationGroup);
                    }
                    else
                    {
                        //accourding to rickard - too dangerous
                        continue;
                    }

                    vacationGroup.Type = templateVacationGroup.Type;
                    vacationGroup.Name = templateVacationGroup.Name;
                    vacationGroup.FromDate = templateVacationGroup.FromDate;
                    vacationGroup.VacationDaysPaidByLaw = templateVacationGroup.VacationDaysPaidByLaw;

                    #endregion

                    #region VacationGroupSE

                    var templateVacationGroupSE = templateVacationGroup.VacationGroupSE?.FirstOrDefault();
                    if (templateVacationGroupSE != null)
                    {
                        VacationGroupSE vacationGroupSE = vacationGroup.VacationGroupSE?.FirstOrDefault();
                        if (vacationGroupSE == null)
                        {
                            vacationGroupSE = new VacationGroupSE()
                            {

                                //Set reference
                                VacationGroup = vacationGroup,
                            };
                            SetCreatedProperties(vacationGroupSE);
                            entities.VacationGroupSE.AddObject(vacationGroupSE);
                        }
                        else
                        {
                            //accourding to rickard - too dangerous
                            continue;
                        }

                        //set properties           
                        vacationGroupSE.CalculationType = templateVacationGroupSE.CalculationType;
                        vacationGroupSE.UseAdditionalVacationDays = templateVacationGroupSE.UseAdditionalVacationDays;
                        vacationGroupSE.NbrOfAdditionalVacationDays = templateVacationGroupSE.NbrOfAdditionalVacationDays;
                        vacationGroupSE.AdditionalVacationDaysFromAge1 = templateVacationGroupSE.AdditionalVacationDaysFromAge1;
                        vacationGroupSE.AdditionalVacationDays1 = templateVacationGroupSE.AdditionalVacationDays1;
                        vacationGroupSE.AdditionalVacationDaysFromAge2 = templateVacationGroupSE.AdditionalVacationDaysFromAge2;
                        vacationGroupSE.AdditionalVacationDays2 = templateVacationGroupSE.AdditionalVacationDays2;
                        vacationGroupSE.AdditionalVacationDaysFromAge3 = templateVacationGroupSE.AdditionalVacationDaysFromAge3;
                        vacationGroupSE.AdditionalVacationDays3 = templateVacationGroupSE.AdditionalVacationDays3;
                        vacationGroupSE.VacationHandleRule = templateVacationGroupSE.VacationHandleRule;
                        vacationGroupSE.VacationDaysHandleRule = templateVacationGroupSE.VacationDaysHandleRule;
                        vacationGroupSE.VacationDaysGrossUseFiveDaysPerWeek = templateVacationGroupSE.VacationDaysGrossUseFiveDaysPerWeek;
                        vacationGroupSE.RemainingDaysRule = templateVacationGroupSE.RemainingDaysRule;
                        vacationGroupSE.UseMaxRemainingDays = templateVacationGroupSE.UseMaxRemainingDays;
                        vacationGroupSE.MaxRemainingDays = templateVacationGroupSE.MaxRemainingDays;
                        vacationGroupSE.RemainingDaysPayoutMonth = templateVacationGroupSE.RemainingDaysPayoutMonth;
                        vacationGroupSE.EarningYearAmountFromDate = templateVacationGroupSE.EarningYearAmountFromDate;
                        vacationGroupSE.EarningYearVariableAmountFromDate = templateVacationGroupSE.EarningYearVariableAmountFromDate;
                        vacationGroupSE.VacationDayPercent = templateVacationGroupSE.VacationDayPercent;
                        vacationGroupSE.VacationDayAdditionPercent = templateVacationGroupSE.VacationDayAdditionPercent;
                        vacationGroupSE.VacationVariablePercent = templateVacationGroupSE.VacationVariablePercent;
                        vacationGroupSE.UseGuaranteeAmount = templateVacationGroupSE.UseGuaranteeAmount;
                        vacationGroupSE.GuaranteeAmountAccordingToHandels = templateVacationGroupSE.GuaranteeAmountAccordingToHandels;
                        vacationGroupSE.GuaranteeAmountMaxNbrOfDaysRule = templateVacationGroupSE.GuaranteeAmountMaxNbrOfDaysRule;
                        vacationGroupSE.GuaranteeAmountEmployedNbrOfYears = templateVacationGroupSE.GuaranteeAmountEmployedNbrOfYears;
                        vacationGroupSE.GuaranteeAmountJuvenile = templateVacationGroupSE.GuaranteeAmountJuvenile;
                        vacationGroupSE.GuaranteeAmountJuvenileAgeLimit = templateVacationGroupSE.GuaranteeAmountJuvenileAgeLimit;
                        vacationGroupSE.VacationAbsenceCalculationRule = templateVacationGroupSE.VacationAbsenceCalculationRule;
                        vacationGroupSE.VacationSalaryPayoutRule = templateVacationGroupSE.VacationSalaryPayoutRule;
                        vacationGroupSE.VacationSalaryPayoutDays = templateVacationGroupSE.VacationSalaryPayoutDays;
                        vacationGroupSE.VacationSalaryPayoutMonth = templateVacationGroupSE.VacationSalaryPayoutMonth;
                        vacationGroupSE.VacationVariablePayoutRule = templateVacationGroupSE.VacationVariablePayoutRule;
                        vacationGroupSE.VacationVariablePayoutDays = templateVacationGroupSE.VacationVariablePayoutDays;
                        vacationGroupSE.VacationVariablePayoutMonth = templateVacationGroupSE.VacationVariablePayoutMonth;
                        vacationGroupSE.YearEndRemainingDaysRule = templateVacationGroupSE.YearEndRemainingDaysRule;
                        vacationGroupSE.YearEndOverdueDaysRule = templateVacationGroupSE.YearEndOverdueDaysRule;
                        vacationGroupSE.YearEndVacationVariableRule = templateVacationGroupSE.YearEndVacationVariableRule;
                        vacationGroupSE.ValueDaysAccountInternalOnDebit = templateVacationGroupSE.ValueDaysAccountInternalOnDebit;
                        vacationGroupSE.ValueDaysAccountInternalOnCredit = templateVacationGroupSE.ValueDaysAccountInternalOnCredit;
                        vacationGroupSE.UseEmploymentTaxAcccount = templateVacationGroupSE.UseEmploymentTaxAcccount;
                        vacationGroupSE.EmploymentTaxAccountInternalOnDebit = templateVacationGroupSE.EmploymentTaxAccountInternalOnDebit;
                        vacationGroupSE.EmploymentTaxAccountInternalOnCredit = templateVacationGroupSE.EmploymentTaxAccountInternalOnCredit;
                        vacationGroupSE.UseSupplementChargeAccount = templateVacationGroupSE.UseSupplementChargeAccount;
                        vacationGroupSE.SupplementChargeAccountInternalOnDebit = templateVacationGroupSE.SupplementChargeAccountInternalOnDebit;
                        vacationGroupSE.SupplementChargeAccountInternalOnCredit = templateVacationGroupSE.SupplementChargeAccountInternalOnCredit;


                        //FK Accounts
                        vacationGroupSE.ValueDaysDebitAccountId = templateVacationGroupSE.ValueDaysDebitAccountId.HasValue && accountStdMapping.ContainsKey(templateVacationGroupSE.ValueDaysDebitAccountId.Value) ? accountStdMapping[templateVacationGroupSE.ValueDaysDebitAccountId.Value].AccountId : (int?)null;
                        vacationGroupSE.ValueDaysCreditAccountId = templateVacationGroupSE.ValueDaysCreditAccountId.HasValue && accountStdMapping.ContainsKey(templateVacationGroupSE.ValueDaysCreditAccountId.Value) ? accountStdMapping[templateVacationGroupSE.ValueDaysCreditAccountId.Value].AccountId : (int?)null;
                        vacationGroupSE.EmploymentTaxDebitAccountId = templateVacationGroupSE.EmploymentTaxDebitAccountId.HasValue && accountStdMapping.ContainsKey(templateVacationGroupSE.EmploymentTaxDebitAccountId.Value) ? accountStdMapping[templateVacationGroupSE.EmploymentTaxDebitAccountId.Value].AccountId : (int?)null;
                        vacationGroupSE.EmploymentTaxCredidAccountId = templateVacationGroupSE.EmploymentTaxCredidAccountId.HasValue && accountStdMapping.ContainsKey(templateVacationGroupSE.EmploymentTaxCredidAccountId.Value) ? accountStdMapping[templateVacationGroupSE.EmploymentTaxCredidAccountId.Value].AccountId : (int?)null;
                        vacationGroupSE.SupplementChargeDebitAccountId = templateVacationGroupSE.SupplementChargeDebitAccountId.HasValue && accountStdMapping.ContainsKey(templateVacationGroupSE.SupplementChargeDebitAccountId.Value) ? accountStdMapping[templateVacationGroupSE.SupplementChargeDebitAccountId.Value].AccountId : (int?)null;
                        vacationGroupSE.SupplementChargeCreditAccountId = templateVacationGroupSE.SupplementChargeCreditAccountId.HasValue && accountStdMapping.ContainsKey(templateVacationGroupSE.SupplementChargeCreditAccountId.Value) ? accountStdMapping[templateVacationGroupSE.SupplementChargeCreditAccountId.Value].AccountId : (int?)null;

                        //TimeDeviationCause
                        vacationGroupSE.ReplacementTimeDeviationCauseId = templateVacationGroupSE.ReplacementTimeDeviationCauseId;

                        //FK
                        vacationGroupSE.MonthlySalaryFormulaId = GetPayrollPriceFormulaId(payrollPriceFormulaMapping, templateVacationGroupSE.MonthlySalaryFormulaId);
                        vacationGroupSE.HourlySalaryFormulaId = GetPayrollPriceFormulaId(payrollPriceFormulaMapping, templateVacationGroupSE.HourlySalaryFormulaId);
                        vacationGroupSE.VacationDayPercentPriceTypeId = GetPayrollPriceTypeId(payrollPriceTypeMapping, templateVacationGroupSE.VacationDayPercentPriceTypeId);
                        vacationGroupSE.VacationDayAdditionPercentPriceTypeId = GetPayrollPriceTypeId(payrollPriceTypeMapping, templateVacationGroupSE.VacationDayAdditionPercentPriceTypeId);
                        vacationGroupSE.VacationVariablePercentPriceTypeId = GetPayrollPriceTypeId(payrollPriceTypeMapping, templateVacationGroupSE.VacationVariablePercentPriceTypeId);
                        vacationGroupSE.GuaranteeAmountPerDayPriceTypeId = GetPayrollPriceTypeId(payrollPriceTypeMapping, templateVacationGroupSE.GuaranteeAmountPerDayPriceTypeId);
                        vacationGroupSE.GuaranteeAmountJuvenilePerDayPriceTypeId = GetPayrollPriceTypeId(payrollPriceTypeMapping, templateVacationGroupSE.GuaranteeAmountJuvenilePerDayPriceTypeId);

                        if (!vacationGroupMapping.ContainsKey(templateVacationGroup.VacationGroupId))
                            vacationGroupMapping.Add(templateVacationGroup.VacationGroupId, vacationGroup);
                    }

                    #endregion
                }

                ActionResult result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("VacationGroups", templateCompanyId, newCompanyId, saved: true);



                return result;
            }
        }

        public ActionResult CopyTimeScheduleTypesAndShiftTypesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, ShiftType> shiftTypeMapping, ref Dictionary<int, TimeScheduleType> timeScheduleTypeMapping, ref Dictionary<int, AccountInternal> accountInternalMapping, ref Dictionary<int, Category> categoryMapping)
        {
            ActionResult result;

            #region Prereq

            List<TimeScheduleType> templateTimeScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(templateCompanyId, getAll: true, onlyActive: true);
            List<ShiftType> templateShiftTypes = TimeScheduleManager.GetShiftTypes(templateCompanyId, loadAccounts: true);
            List<CompanyCategoryRecord> templateShiftTypeCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.ShiftType, templateCompanyId);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<TimeScheduleType> existingTimeScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(entities, newCompanyId, getAll: true, onlyActive: true);
                List<ShiftType> existingShiftTypes = TimeScheduleManager.GetShiftTypes(entities, newCompanyId, loadAccounts: true);

                #endregion

                #region TimeScheduleType

                foreach (TimeScheduleType templateTimeScheduleType in templateTimeScheduleTypes)
                {
                    TimeScheduleType timeScheduleType = existingTimeScheduleTypes.FirstOrDefault(i => i.Name == templateTimeScheduleType.Name && i.Code == templateTimeScheduleType.Code);
                    if (timeScheduleType == null)
                    {
                        timeScheduleType = new TimeScheduleType()
                        {
                            //Set FK
                            ActorCompanyId = newCompanyId,
                        };
                        SetCreatedProperties(timeScheduleType, user);
                        entities.TimeScheduleType.AddObject(timeScheduleType);
                    }
                    else
                    {
                        SetModifiedProperties(timeScheduleType, user);
                    }

                    timeScheduleType.Code = templateTimeScheduleType.Code;
                    timeScheduleType.Name = templateTimeScheduleType.Name;
                    timeScheduleType.Description = templateTimeScheduleType.Description;
                    timeScheduleType.IsAll = templateTimeScheduleType.IsAll;
                    timeScheduleType.IsNotScheduleTime = templateTimeScheduleType.IsNotScheduleTime;
                    timeScheduleType.IgnoreIfExtraShift = templateTimeScheduleType.IgnoreIfExtraShift;
                    timeScheduleType.ShowInTerminal = templateTimeScheduleType.ShowInTerminal;
                    timeScheduleType.UseScheduleTimeFactor = templateTimeScheduleType.UseScheduleTimeFactor;
                    timeScheduleType.IsBilagaJ = templateTimeScheduleType.IsBilagaJ;
                    timeScheduleType.State = templateTimeScheduleType.State;

                    timeScheduleTypeMapping.Add(templateTimeScheduleType.TimeScheduleTypeId, timeScheduleType);
                }

                #endregion

                #region ShiftType

                foreach (ShiftType templateShiftType in templateShiftTypes)
                {
                    ShiftType shiftType = existingShiftTypes.FirstOrDefault(i => i.Name == templateShiftType.Name);
                    if (shiftType == null)
                    {
                        shiftType = new ShiftType()
                        {
                            ActorCompanyId = newCompanyId
                        };
                        SetCreatedProperties(shiftType, user);
                        entities.ShiftType.AddObject(shiftType);
                    }
                    else
                    {
                        SetModifiedProperties(shiftType, user);
                    }

                    shiftType.Name = templateShiftType.Name;
                    shiftType.Description = templateShiftType.Description;
                    shiftType.TimeScheduleTemplateBlockType = templateShiftType.TimeScheduleTemplateBlockType;
                    shiftType.Color = templateShiftType.Color;
                    shiftType.ExternalId = templateShiftType.ExternalId;
                    shiftType.ExternalCode = templateShiftType.ExternalCode;
                    shiftType.DefaultLength = templateShiftType.DefaultLength;
                    shiftType.StartTime = templateShiftType.StartTime;
                    shiftType.StopTime = templateShiftType.StopTime;
                    shiftType.NeedsCode = templateShiftType.NeedsCode;
                    shiftType.HandlingMoney = templateShiftType.HandlingMoney;

                    if (templateShiftType.AccountId.HasValue && accountInternalMapping.ContainsKey(templateShiftType.AccountId.Value))
                        shiftType.AccountId = accountInternalMapping[templateShiftType.AccountId.Value].AccountId.ToNullable();

                    if (shiftType.ShiftTypeId == 0)
                    {
                        //AccountInternals
                        foreach (int templateAccountInternalId in templateShiftType.AccountInternal.Select(a => a.AccountId))
                        {
                            AccountInternal accountInternal = null;
                            if (accountInternalMapping.ContainsKey(templateAccountInternalId))
                                accountInternal = AccountManager.GetAccountInternal(entities, accountInternalMapping[templateAccountInternalId].AccountId, newCompanyId);
                            if (accountInternal != null)
                                shiftType.AccountInternal.Add(accountInternal);
                        }
                    }

                    shiftTypeMapping.Add(templateShiftType.ShiftTypeId, shiftType);
                }

                #endregion

                #region Save

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("TimeScheduleTypeAndShiftType", templateCompanyId, newCompanyId, saved: true);

                foreach (int templateShiftTypeId in templateShiftTypes.Select(s => s.ShiftTypeId))
                {
                    if (!shiftTypeMapping.ContainsKey(templateShiftTypeId))
                        continue;

                    ShiftType shiftType = shiftTypeMapping[templateShiftTypeId];

                    //Categories
                    List<CompanyCategoryRecord> templateCategoryRecords = templateShiftTypeCategoryRecords.Where(i => i.RecordId == templateShiftTypeId).ToList();
                    foreach (CompanyCategoryRecord templateCategoryRecord in templateCategoryRecords)
                    {
                        if (!categoryMapping.ContainsKey(templateCategoryRecord.CategoryId))
                            continue;

                        CompanyCategoryRecord categoryRecord = new CompanyCategoryRecord()
                        {
                            RecordId = shiftType.ShiftTypeId,
                            Entity = (int)SoeCategoryRecordEntity.ShiftType,
                            DateFrom = templateCategoryRecord.DateFrom,
                            DateTo = templateCategoryRecord.DateTo,
                            IsExecutive = templateCategoryRecord.IsExecutive,

                            //Set FK
                            CategoryId = categoryMapping[templateCategoryRecord.CategoryId].CategoryId,

                            //Set references
                            Company = newCompany,
                        };
                        SetCreatedProperties(categoryRecord);
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = LogCopyError("CompanyCategoryRecord", templateCompanyId, newCompanyId, saved: true);
                }

                #endregion
            }

            return result;
        }

        public ActionResult CopySkillsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            ActionResult result = null;

            #region Prereq

            Dictionary<int, SkillType> skillTypeMappingDict = new Dictionary<int, SkillType>();
            Dictionary<int, Skill> skillsMappingDict = new Dictionary<int, Skill>();

            List<SkillType> templateSkillTypes = TimeScheduleManager.GetSkillTypes(templateCompanyId);
            List<Skill> templateSkills = TimeScheduleManager.GetSkills(templateCompanyId);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<SkillType> existingSkillTypes = TimeScheduleManager.GetSkillTypes(entities, newCompanyId);
                List<Skill> existingSkills = TimeScheduleManager.GetSkills(entities, newCompanyId);

                #endregion

                #region SkillType

                foreach (SkillType templateSkillType in templateSkillTypes)
                {
                    SkillType skillType = existingSkillTypes.FirstOrDefault(i => i.Name == templateSkillType.Name);
                    if (skillType == null)
                    {
                        skillType = new SkillType()
                        {
                            ActorCompanyId = newCompanyId
                        };
                        SetCreatedProperties(skillType, user);
                        entities.SkillType.AddObject(skillType);
                    }
                    else
                    {
                        SetModifiedProperties(skillType, user);
                    }

                    skillType.Name = templateSkillType.Name;
                    skillType.Description = templateSkillType.Description;

                    skillTypeMappingDict.Add(templateSkillType.SkillTypeId, skillType);
                }

                #endregion

                #region Skills

                foreach (Skill templateSkill in templateSkills)
                {
                    Skill skill = existingSkills.FirstOrDefault(i => i.Name == templateSkill.Name);
                    if (skill == null)
                    {
                        skill = new Skill()
                        {
                            ActorCompanyId = newCompanyId
                        };
                        SetCreatedProperties(skill, user);
                        entities.Skill.AddObject(skill);
                    }
                    else
                    {
                        SetModifiedProperties(skill, user);
                    }

                    skill.Name = templateSkill.Name;
                    skill.Description = templateSkill.Description;
                    skill.SkillType = skillTypeMappingDict[templateSkill.SkillTypeId];

                    skillsMappingDict.Add(templateSkill.SkillId, skill);
                }

                #endregion

                #region Save

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("TimeScheduleTypeAndShiftType", templateCompanyId, newCompanyId, saved: true);

                #endregion

                return result;
            }
        }

        public ActionResult CopyScheduleCyclesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            ActionResult result = null;

            #region Prereq

            List<ScheduleCycle> templateScheduleCycles = TimeScheduleManager.GetScheduleCycleWithRulesAndRuleTypesFromCompany(templateCompanyId);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<ScheduleCycleRuleType> existingCycleRuleTypes = new List<ScheduleCycleRuleType>();
                List<ScheduleCycleRule> existingCycleRules = new List<ScheduleCycleRule>();
                List<ScheduleCycle> existingScheduleCycles = TimeScheduleManager.GetScheduleCycleWithRulesAndRuleTypesFromCompany(entities, newCompanyId);
                foreach (ScheduleCycle existingScheduleCycle in existingScheduleCycles)
                {
                    foreach (ScheduleCycleRule existingCycleRule in existingScheduleCycle.ScheduleCycleRule)
                    {
                        if (!existingCycleRules.Any(w => w.ScheduleCycleRuleId == existingCycleRule.ScheduleCycleRuleId))
                            existingCycleRules.Add(existingCycleRule);

                        if (!existingCycleRuleTypes.Any(w => w.ScheduleCycleRuleTypeId == existingCycleRule.ScheduleCycleRuleTypeId))
                            existingCycleRuleTypes.Add(existingCycleRule.ScheduleCycleRuleType);
                    }
                }

                #endregion

                foreach (ScheduleCycle templateScheduleCycle in templateScheduleCycles)
                {
                    #region ScheduleCycle

                    ScheduleCycle newScheduleCycle = existingScheduleCycles.FirstOrDefault(p => p.Name == templateScheduleCycle.Name);
                    if (newScheduleCycle != null)
                        continue;

                    newScheduleCycle = new ScheduleCycle()
                    {
                        Name = templateScheduleCycle.Name,
                        Description = templateScheduleCycle.Description,
                        NbrOfWeeks = templateScheduleCycle.NbrOfWeeks,
                        ActorCompanyId = newCompanyId,
                    };
                    SetCreatedProperties(newScheduleCycle, user);
                    entities.ScheduleCycle.AddObject(newScheduleCycle);

                    #endregion

                    #region ScheduleCycleRule

                    foreach (ScheduleCycleRule templateScheduleCycleRule in templateScheduleCycle.ScheduleCycleRule.Where(w => w.State == (int)SoeEntityState.Active))
                    {
                        ScheduleCycleRule existingScheduleCycleRule = existingCycleRules.FirstOrDefault(f => f.MinOccurrences == templateScheduleCycleRule.MinOccurrences && f.MaxOccurrences == templateScheduleCycleRule.MinOccurrences && f.ScheduleCycleRuleType.Name == templateScheduleCycleRule.ScheduleCycleRuleType.Name);
                        if (existingScheduleCycleRule != null)
                            continue;

                        ScheduleCycleRule newScheduleCycleRule = new ScheduleCycleRule()
                        {
                            MaxOccurrences = templateScheduleCycleRule.MaxOccurrences,
                            MinOccurrences = templateScheduleCycleRule.MinOccurrences
                        };

                        if (templateScheduleCycleRule.ScheduleCycleRuleType != null)
                        {
                            ScheduleCycleRuleType existingScheduleCycleRuleType = existingCycleRuleTypes.FirstOrDefault(f => f.Name == templateScheduleCycleRule.ScheduleCycleRuleType.Name);
                            if (existingScheduleCycleRuleType != null)
                            {
                                newScheduleCycleRule.ScheduleCycleRuleType = existingScheduleCycleRuleType;
                            }
                            else
                            {
                                ScheduleCycleRuleType newScheduleCycleRuleType = new ScheduleCycleRuleType()
                                {
                                    Name = templateScheduleCycleRule.ScheduleCycleRuleType.Name,
                                    DayOfWeeks = templateScheduleCycleRule.ScheduleCycleRuleType.DayOfWeeks,
                                    StartTime = templateScheduleCycleRule.ScheduleCycleRuleType.StartTime,
                                    StopTime = templateScheduleCycleRule.ScheduleCycleRuleType.StopTime,
                                    ActorCompanyId = newCompanyId
                                };

                                existingCycleRuleTypes.Add(newScheduleCycleRuleType);
                                newScheduleCycleRule.ScheduleCycleRuleType = newScheduleCycleRuleType;
                                entities.ScheduleCycleRuleType.AddObject(newScheduleCycleRuleType);
                            }
                        }

                        existingCycleRules.Add(newScheduleCycleRule);
                        newScheduleCycle.ScheduleCycleRule.Add(newScheduleCycleRule);
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        LogCopyError("ScheduleCycles", templateCompanyId, newCompanyId, saved: true);

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("ScheduleCycles", templateCompanyId, newCompanyId, saved: true);

            }

            return result;
        }

        public ActionResult CreateFormulasAndPriceTypeDicts(int newCompanyId, int templateCompanyId, int userId, bool onlyVacationGroup, ref Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping, ref Dictionary<int, PayrollPriceType> payrollPriceTypeMapping)
        {
            ActionResult result = new ActionResult();
            List<PayrollPriceType> templatePriceTypes = PayrollManager.GetPayrollPriceTypes(templateCompanyId, null, true);
            List<PayrollPriceFormula> templatePayrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(templateCompanyId, false);
            List<VacationGroup> templateVacationGroups = PayrollManager.GetVacationGroupsWithVacationGroupSE(templateCompanyId);

            List<int> validFormulaIds = onlyVacationGroup ? new List<int>() : null;
            List<int> validPriceTypeIds = onlyVacationGroup ? new List<int>() : null;

            if (onlyVacationGroup)
            {
                foreach (var vacationGroup in templateVacationGroups)
                {
                    validFormulaIds.Add(vacationGroup.VacationGroupSE?.FirstOrDefault().MonthlySalaryFormulaId ?? 0);
                    validFormulaIds.Add(vacationGroup.VacationGroupSE?.FirstOrDefault().HourlySalaryFormulaId ?? 0);

                    validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.FirstOrDefault().VacationDayPercentPriceTypeId ?? 0);
                    validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.FirstOrDefault().VacationDayAdditionPercentPriceTypeId ?? 0);
                    validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.FirstOrDefault().VacationVariablePercentPriceTypeId ?? 0);
                    validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.FirstOrDefault().GuaranteeAmountPerDayPriceTypeId ?? 0);
                    validPriceTypeIds.Add(vacationGroup.VacationGroupSE?.FirstOrDefault().GuaranteeAmountJuvenilePerDayPriceTypeId ?? 0);
                }

                validFormulaIds = validFormulaIds.Where(w => w != 0).Distinct().ToList();
                validPriceTypeIds = validPriceTypeIds.Where(w => w != 0).Distinct().ToList();
            }

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<PayrollPriceType> existingPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, newCompanyId, null, false);
                List<PayrollPriceFormula> existingPayrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(entities, newCompanyId, false);

                #endregion

                #region PriceType

                foreach (PayrollPriceType templatePriceType in templatePriceTypes)
                {
                    if (onlyVacationGroup && !validPriceTypeIds.Contains(templatePriceType.PayrollPriceTypeId))
                        continue;

                    PayrollPriceType priceType = existingPriceTypes.FirstOrDefault(pt => pt.Code == templatePriceType.Code);
                    if (priceType == null)
                    {
                        priceType = new PayrollPriceType()
                        {
                            //Set FK
                            ActorCompanyId = newCompanyId,
                        };
                        SetCreatedProperties(priceType, user);
                        entities.PayrollPriceType.AddObject(priceType);
                    }
                    else
                    {
                        //Add mapping
                        if (!payrollPriceTypeMapping.ContainsKey(templatePriceType.PayrollPriceTypeId))
                            payrollPriceTypeMapping.Add(templatePriceType.PayrollPriceTypeId, priceType);

                        //accourding to rickard - too dangerous
                        continue;
                    }

                    priceType.Type = templatePriceType.Type;
                    priceType.Code = templatePriceType.Code;
                    priceType.Name = templatePriceType.Name;
                    priceType.Description = templatePriceType.Description;
                    priceType.ConditionEmployedMonths = templatePriceType.ConditionEmployedMonths;
                    priceType.ConditionExperienceMonths = templatePriceType.ConditionExperienceMonths;
                    priceType.ConditionAgeYears = templatePriceType.ConditionAgeYears;

                    if (templatePriceType.PayrollPriceTypePeriod != null)
                    {
                        foreach (var period in templatePriceType.PayrollPriceTypePeriod)
                        {
                            priceType.PayrollPriceTypePeriod.Add(new PayrollPriceTypePeriod()
                            {
                                Amount = period.Amount,
                                FromDate = period.FromDate,
                                Created = DateTime.Now
                            });
                        }
                    }

                    //Add mapping
                    if (!payrollPriceTypeMapping.ContainsKey(templatePriceType.PayrollPriceTypeId))
                        payrollPriceTypeMapping.Add(templatePriceType.PayrollPriceTypeId, priceType);
                }

                #endregion

                #region PayrollPriceFormula

                foreach (var templatePayrollPriceFormula in templatePayrollPriceFormulas)
                {
                    if (onlyVacationGroup && !validFormulaIds.Contains(templatePayrollPriceFormula.PayrollPriceFormulaId))
                        continue;

                    PayrollPriceFormula priceFormula = existingPayrollPriceFormulas.FirstOrDefault(pf => pf.Code == templatePayrollPriceFormula.Code);

                    if (priceFormula == null)
                    {
                        priceFormula = new PayrollPriceFormula()
                        {
                            //Set FK
                            ActorCompanyId = newCompanyId,
                        };
                        SetCreatedProperties(priceFormula, user);
                        entities.PayrollPriceFormula.AddObject(priceFormula);
                    }
                    else
                    {
                        //Add 
                        if (!payrollPriceFormulaMapping.ContainsKey(templatePayrollPriceFormula.PayrollPriceFormulaId))
                            payrollPriceFormulaMapping.Add(templatePayrollPriceFormula.PayrollPriceFormulaId, priceFormula);

                        //accourding to rickard - too dangerous
                        continue;
                    }

                    priceFormula.Code = templatePayrollPriceFormula.Code;
                    priceFormula.Name = templatePayrollPriceFormula.Name;
                    priceFormula.Description = templatePayrollPriceFormula.Description;
                    priceFormula.Formula = string.Empty; //will be calculated last
                    priceFormula.FormulaPlain = templatePayrollPriceFormula.FormulaPlain;

                    //Add mapping
                    if (!payrollPriceFormulaMapping.ContainsKey(templatePayrollPriceFormula.PayrollPriceFormulaId))
                        payrollPriceFormulaMapping.Add(templatePayrollPriceFormula.PayrollPriceFormulaId, priceFormula);
                }

                #endregion

                #region Save

                result = SaveChanges(entities);
                if (!result.Success)
                    return LogCopyError("CreateFormulasAndPriceTypeDicts", templateCompanyId, newCompanyId, saved: true);

                #endregion

                #region Calculate Formula for PayrollPriceFormula (should be done after save payrollpricetype and payrollpriceformula)

                foreach (var mapping in payrollPriceFormulaMapping)
                {
                    var priceFormula = mapping.Value;
                    priceFormula.Formula = PayrollManager.ConvertPayrollPriceFormulaToDB(newCompanyId, priceFormula.FormulaPlain);
                }

                #endregion

                #region Save

                result = SaveChanges(entities);
                if (!result.Success)
                    return LogCopyError("Formula for PayrollPriceFormula", templateCompanyId, newCompanyId, saved: true);

                #endregion

                return result;
            }
        }
        public ActionResult CopyPayrollLevelsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, ref Dictionary<int, PayrollLevel> payrollLevelMapping)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq
                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Existing for template Company
                List<PayrollLevel> templatePayrollLevels = entities.PayrollLevel.Where(e => e.ActorCompanyId == templateCompanyId && e.State != (int)SoeEntityState.Deleted).ToList();

                //Existing for new Company (before copy)
                List<PayrollLevel> existingPayrollLevels = entities.PayrollLevel.Where(e => e.ActorCompanyId == newCompanyId && e.State != (int)SoeEntityState.Deleted).ToList();

                #endregion

                foreach (PayrollLevel templatePayrollLevel in templatePayrollLevels)
                {
                    PayrollLevel newPayrollLevel = existingPayrollLevels.FirstOrDefault(t => t.Name == templatePayrollLevel.Name);
                    if (newPayrollLevel == null)
                    {
                        #region Add

                        newPayrollLevel = new PayrollLevel()
                        {
                            //References
                            Company = newCompany,
                        };
                        SetCreatedProperties(newPayrollLevel);
                        entities.PayrollLevel.AddObject(newPayrollLevel);
                        newCompany.PayrollLevel.Add(newPayrollLevel);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        SetModifiedProperties(newPayrollLevel);

                        #endregion
                    }

                    newPayrollLevel.ExternalCode = templatePayrollLevel.ExternalCode;
                    newPayrollLevel.Code = templatePayrollLevel.Code;
                    newPayrollLevel.Name = templatePayrollLevel.Name;
                    newPayrollLevel.Description = templatePayrollLevel.Description;
                    newPayrollLevel.State = templatePayrollLevel.State;

                    if (!payrollLevelMapping.ContainsKey(templatePayrollLevel.PayrollLevelId))
                        payrollLevelMapping.Add(templatePayrollLevel.PayrollLevelId, newPayrollLevel);

                    result = SaveChanges(entities);
                    if (!result.Success)
                        LogCopyError("PayrollLevel", templateCompanyId, newCompanyId, saved: true);
                }
            }

            return result;
        }

        public ActionResult CopyPayrollGroupsPriceTypesAndPriceFormulasFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, PayrollGroup> payrollGroupMapping, ref Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping, ref Dictionary<int, PayrollPriceType> payrollPriceTypeMapping, ref Dictionary<int, Report> reportMapping, ref Dictionary<int, AccountStd> accountStdMapping, ref Dictionary<int, PayrollLevel> payrollLevelMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            List<TimePeriodHead> templateTimePeriodHeads = TimePeriodManager.GetTimePeriodHeads(templateCompanyId, TermGroup_TimePeriodType.Unknown, false, false);
            List<PayrollGroup> templatePayrollGroups = PayrollManager.GetPayrollGroups(templateCompanyId, loadPriceTypes: true, loadTimePeriods: true, loadSettings: true, loadAccountStd: true);
            List<VacationGroup> templateVacationGroups = PayrollManager.GetVacationGroups(templateCompanyId);
            List<PayrollProduct> templatePayrollProducts = ProductManager.GetPayrollProducts(templateCompanyId, active: null);
            List<ReportTemplate> templateReportTemplates = ReportManager.GetReportTemplates(templateCompanyId).Where(x => x.SysTemplateTypeId == (int)SoeReportTemplateType.TimeEmploymentContract).ToList();
            List<Report> templateReports = ReportManager.GetReportsByTemplateType(templateCompanyId, null, SoeReportTemplateType.TimeEmploymentContract, module: (int)SoeModule.Time, onlyOriginal: true, loadSysReportTemplate: true);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    #region Prereq

                    Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                    if (newCompany == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    User user = UserManager.GetUser(entities, userId);
                    if (user == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                    List<TimePeriodHead> existingTimePeriodHeads = TimePeriodManager.GetTimePeriodHeads(entities, newCompanyId, TermGroup_TimePeriodType.Unknown, false, false);
                    List<PayrollGroup> existingPayrollGroups = PayrollManager.GetPayrollGroups(entities, newCompanyId, loadPriceTypes: true, loadTimePeriods: true, loadSettings: true, loadAccountStd: true);
                    List<VacationGroup> existingVacationGroups = PayrollManager.GetVacationGroups(entities, newCompanyId);
                    List<PayrollProduct> existingPayrollProducts = ProductManager.GetPayrollProducts(entities, newCompanyId, active: null);
                    List<ReportTemplate> existingReportTemplates = ReportManager.GetReportTemplates(entities, newCompanyId).Where(x => x.SysTemplateTypeId == (int)SoeReportTemplateType.TimeEmploymentContract).ToList();
                    List<Report> existingReports = ReportManager.GetReports(entities, newCompanyId, null, onlyOriginal: true);

                    Dictionary<int, TimePeriodHead> timePeriodHeadMapping = new Dictionary<int, TimePeriodHead>();
                    Dictionary<int, ReportTemplate> reportTemplateMapping = new Dictionary<int, ReportTemplate>();
                    Dictionary<int, VacationGroup> vacationGroupMapping = new Dictionary<int, VacationGroup>();
                    Dictionary<int, PayrollProduct> payrollProductMapping = new Dictionary<int, PayrollProduct>();

                    #endregion

                    #region TimePeriodHead

                    // Create mapping dictionary
                    foreach (TimePeriodHead templateTimePeriodHead in templateTimePeriodHeads)
                    {
                        TimePeriodHead timePeriodHead = existingTimePeriodHeads.FirstOrDefault(pt => pt.Name == templateTimePeriodHead.Name);
                        if (timePeriodHead != null)
                            timePeriodHeadMapping.Add(templateTimePeriodHead.TimePeriodHeadId, timePeriodHead);
                    }

                    #endregion              

                    #region Save

                    result = SaveChanges(entities);
                    if (!result.Success)
                        return LogCopyError("PayrollPriceFormula", templateCompanyId, newCompanyId, saved: true);

                    #endregion

                    #region VacationGroup

                    // Create mapping dictionary
                    foreach (VacationGroup templateVacationGroup in templateVacationGroups)
                    {
                        VacationGroup vacationGroup = existingVacationGroups.FirstOrDefault(v => v.Name == templateVacationGroup.Name);
                        if (vacationGroup != null)
                            vacationGroupMapping.Add(templateVacationGroup.VacationGroupId, vacationGroup);
                    }

                    #endregion

                    #region PayrollProduct

                    // Create mapping dictionary
                    foreach (PayrollProduct templatePayrollProduct in templatePayrollProducts)
                    {
                        PayrollProduct payrollProduct = existingPayrollProducts.FirstOrDefault(v => v.Number == templatePayrollProduct.Number);
                        if (payrollProduct != null)
                            payrollProductMapping.Add(templatePayrollProduct.ProductId, payrollProduct);
                    }

                    #endregion

                    #region ReportTemplate and Report

                    #region ReportTemplate

                    foreach (ReportTemplate templateReportTemplate in templateReportTemplates)
                    {
                        ReportTemplate reportTemplate = existingReportTemplates.FirstOrDefault(i => i.Name == templateReportTemplate.Name && i.SysReportTypeId == templateReportTemplate.SysReportTypeId);
                        if (reportTemplate == null)
                        {
                            reportTemplate = new ReportTemplate()
                            {
                                Name = templateReportTemplate.Name,
                                Description = templateReportTemplate.Description,
                                FileName = templateReportTemplate.FileName,
                                Template = templateReportTemplate.Template,
                                SysReportTypeId = templateReportTemplate.SysReportTypeId,
                                SysTemplateTypeId = templateReportTemplate.SysTemplateTypeId,

                                //Set reference
                                Company = newCompany,
                            };

                            SetCreatedProperties(reportTemplate, user);
                            entities.ReportTemplate.AddObject(reportTemplate);
                            reportTemplateMapping.Add(templateReportTemplate.ReportTemplateId, reportTemplate);
                        }
                        else
                        {
                            reportTemplateMapping.Add(templateReportTemplate.ReportTemplateId, reportTemplate);
                        }
                    }

                    #endregion

                    result = SaveChanges(entities);
                    if (!result.Success)
                        return LogCopyError("ReportTemplate", templateCompanyId, newCompanyId, saved: true);


                    #region Report

                    foreach (Report templateReport in templateReports)
                    {
                        Report report = existingReports.FirstOrDefault(i => i.Name == templateReport.Name && i.ReportNr == templateReport.ReportNr);
                        if (report == null)
                        {
                            report = new Report()
                            {
                                Module = templateReport.Module,
                                ReportNr = templateReport.ReportNr,
                                Name = templateReport.Name,
                                Description = templateReport.Description,
                                Standard = templateReport.Standard,
                                Original = true,
                                ExportType = templateReport.ExportType,
                                FileType = templateReport.FileType,
                                IncludeAllHistoricalData = templateReport.IncludeAllHistoricalData,
                                IncludeBudget = templateReport.IncludeBudget,
                                NoOfYearsBackinPreviousYear = templateReport.NoOfYearsBackinPreviousYear,
                                ShowInAccountingReports = templateReport.ShowInAccountingReports,
                                GetDetailedInformation = templateReport.GetDetailedInformation,
                                State = (int)SoeEntityState.Active,

                                //Set references
                                ActorCompanyId = newCompanyId,
                            };

                            bool foundReportTemplate = false;
                            if (templateReport.Standard)
                            {
                                //ReportTemplate is in SOESys database
                                report.ReportTemplateId = templateReport.ReportTemplateId;
                                foundReportTemplate = true;
                            }
                            else
                            {
                                //ReportTemplate is in SOEComp database
                                if (reportTemplateMapping.ContainsKey(templateReport.ReportTemplateId))
                                {
                                    //Get the mapped ReportTemplateId
                                    report.ReportTemplateId = reportTemplateMapping[templateReport.ReportTemplateId].ReportTemplateId;
                                    foundReportTemplate = true;
                                }
                                else
                                {
                                    //ReportTemplate is deleted
                                    foundReportTemplate = false;
                                }
                            }

                            if (foundReportTemplate)
                            {
                                SetCreatedProperties(report, user);
                                entities.Report.AddObject(report);
                                reportMapping.Add(templateReport.ReportId, report);
                            }
                            else
                                return LogCopyError("Report", "ReportId", templateReport.ReportId, templateReport.ReportNr.ToString(), templateReport.Name, templateCompanyId, newCompanyId, add: true);
                        }
                        else
                        {
                            if (!reportMapping.Keys.Contains(templateReport.ReportId))
                                reportMapping.Add(templateReport.ReportId, report);
                        }
                    }

                    #endregion

                    #endregion

                    #region PayrollGroup

                    foreach (PayrollGroup templatePayrollGroup in templatePayrollGroups.Where(i => i.State == (int)SoeEntityState.Active))
                    {
                        #region PayrollGroup

                        PayrollGroup payrollGroup = existingPayrollGroups.FirstOrDefault(pt => pt.Name == templatePayrollGroup.Name);
                        if (payrollGroup == null)
                        {
                            payrollGroup = new PayrollGroup()
                            {
                                //Set FK
                                ActorCompanyId = newCompanyId,
                            };
                            SetCreatedProperties(payrollGroup, user);
                            entities.PayrollGroup.AddObject(payrollGroup);

                            if (!payrollGroupMapping.ContainsKey(templatePayrollGroup.PayrollGroupId))
                                payrollGroupMapping.Add(templatePayrollGroup.PayrollGroupId, payrollGroup);
                        }
                        else
                        {
                            if (!payrollGroupMapping.ContainsKey(templatePayrollGroup.PayrollGroupId))
                                payrollGroupMapping.Add(templatePayrollGroup.PayrollGroupId, payrollGroup);

                            //accourding to rickard - to dangerous
                            continue;
                        }

                        payrollGroup.Name = templatePayrollGroup.Name;

                        #endregion

                        #region TimePeriodHead

                        TimePeriodHead timePeriodHead = null;
                        if (templatePayrollGroup.TimePeriodHeadId.HasValue && timePeriodHeadMapping.ContainsKey(templatePayrollGroup.TimePeriodHeadId.Value))
                        {
                            timePeriodHead = timePeriodHeadMapping[templatePayrollGroup.TimePeriodHeadId.Value];
                            payrollGroup.TimePeriodHead = timePeriodHead;
                        }

                        #endregion

                        #region OneTimeTaxFormula

                        PayrollPriceFormula oneTimeTaxFormula = null;
                        if (templatePayrollGroup.OneTimeTaxFormulaId.HasValue && payrollPriceFormulaMapping.ContainsKey(templatePayrollGroup.OneTimeTaxFormulaId.Value))
                        {
                            oneTimeTaxFormula = payrollPriceFormulaMapping[templatePayrollGroup.OneTimeTaxFormulaId.Value];
                            if (oneTimeTaxFormula != null)
                                payrollGroup.OneTimeTaxFormulaId = oneTimeTaxFormula.PayrollPriceFormulaId;
                        }

                        #endregion

                        #region PayrollGroupSetting

                        if (!templatePayrollGroup.PayrollGroupSetting.IsLoaded)
                            templatePayrollGroup.PayrollGroupSetting.Load();

                        foreach (PayrollGroupSetting templateSetting in templatePayrollGroup.PayrollGroupSetting.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            PayrollGroupSetting setting = new PayrollGroupSetting()
                            {
                                Type = templateSetting.Type,
                                DataType = templateSetting.DataType,
                                Name = templateSetting.Name,
                                StrData = templateSetting.StrData,
                                IntData = templateSetting.IntData,
                                DecimalData = templateSetting.DecimalData,
                                BoolData = templateSetting.BoolData,
                                DateData = templateSetting.DateData,
                                TimeData = templateSetting.TimeData,

                                //Set references
                                PayrollGroup = payrollGroup,
                            };
                            SetCreatedProperties(setting);
                            payrollGroup.PayrollGroupSetting.Add(setting);
                        }

                        #endregion

                        #region PayrollGroupAccountStd

                        if (!templatePayrollGroup.PayrollGroupAccountStd.IsLoaded)
                            templatePayrollGroup.PayrollGroupAccountStd.Load();

                        foreach (PayrollGroupAccountStd templatePayrollGroupAccountStd in templatePayrollGroup.PayrollGroupAccountStd)
                        {
                            if (!accountStdMapping.ContainsKey(templatePayrollGroupAccountStd.AccountId))
                                continue;

                            AccountStd accountStd = accountStdMapping[templatePayrollGroupAccountStd.AccountId];
                            if (accountStd == null)
                                continue;

                            PayrollGroupAccountStd newPayrollGroupAccountStd = new PayrollGroupAccountStd()
                            {
                                Type = templatePayrollGroupAccountStd.Type,
                                Percent = templatePayrollGroupAccountStd.Percent,
                                FromInterval = templatePayrollGroupAccountStd.FromInterval,
                                ToInterval = templatePayrollGroupAccountStd.ToInterval,

                                AccountId = accountStd.AccountId,

                                //Set references
                                PayrollGroup = payrollGroup,
                            };
                            SetCreatedProperties(newPayrollGroupAccountStd);
                            payrollGroup.PayrollGroupAccountStd.Add(newPayrollGroupAccountStd);
                        }

                        #endregion

                        #region PayrollGroupPriceType

                        if (!templatePayrollGroup.PayrollGroupPriceType.IsLoaded)
                            templatePayrollGroup.PayrollGroupPriceType.Load();

                        foreach (PayrollGroupPriceType templatePayrollGroupPriceType in templatePayrollGroup.PayrollGroupPriceType.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            PayrollPriceType payrollPriceType = null;
                            if (payrollPriceTypeMapping.ContainsKey(templatePayrollGroupPriceType.PayrollPriceTypeId))
                                payrollPriceType = payrollPriceTypeMapping[templatePayrollGroupPriceType.PayrollPriceTypeId];
                            if (payrollPriceType == null)
                                continue;

                            PayrollLevel payrollLevel = null;
                            if (templatePayrollGroupPriceType.PayrollLevelId.HasValue && payrollLevelMapping.ContainsKey(templatePayrollGroupPriceType.PayrollLevelId.Value))
                                payrollLevel = payrollLevelMapping[templatePayrollGroupPriceType.PayrollLevelId.Value];

                            PayrollGroupPriceType payrollGroupPriceType = new PayrollGroupPriceType()
                            {
                                Sort = templatePayrollGroupPriceType.Sort,
                                ShowOnEmployee = templatePayrollGroupPriceType.ShowOnEmployee,
                                ReadOnlyOnEmployee = templatePayrollGroupPriceType.ReadOnlyOnEmployee,

                                //Set references
                                PayrollGroup = payrollGroup,
                                PayrollPriceTypeId = payrollPriceType.PayrollPriceTypeId,
                                PayrollLevelId = payrollLevel?.PayrollLevelId ?? null,
                            };


                            if (templatePayrollGroupPriceType.PayrollGroupPriceTypePeriod.IsLoaded)
                                templatePayrollGroupPriceType.PayrollGroupPriceTypePeriod.Load();

                            if (!templatePayrollGroupPriceType.PayrollGroupPriceTypePeriod.IsNullOrEmpty())
                            {
                                foreach (var period in templatePayrollGroupPriceType.PayrollGroupPriceTypePeriod)
                                {
                                    payrollGroupPriceType.PayrollGroupPriceTypePeriod.Add(new PayrollGroupPriceTypePeriod()
                                    {
                                        Amount = period.Amount,
                                        FromDate = period.FromDate,
                                        Created = DateTime.Now
                                    });
                                }
                            }

                            SetCreatedProperties(payrollGroupPriceType);
                            payrollGroup.PayrollGroupPriceType.Add(payrollGroupPriceType);
                        }

                        #endregion

                        #region PayrollGroupPriceFormula

                        if (!templatePayrollGroup.PayrollGroupPriceFormula.IsLoaded)
                            templatePayrollGroup.PayrollGroupPriceFormula.Load();

                        foreach (PayrollGroupPriceFormula templatePayrollGroupPriceFormula in templatePayrollGroup.PayrollGroupPriceFormula.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            PayrollPriceFormula payrollPriceFormula = null;
                            if (payrollPriceFormulaMapping.ContainsKey(templatePayrollGroupPriceFormula.PayrollPriceFormulaId))
                                payrollPriceFormula = payrollPriceFormulaMapping[templatePayrollGroupPriceFormula.PayrollPriceFormulaId];
                            if (payrollPriceFormula == null)
                                continue;

                            PayrollGroupPriceFormula payrollGroupPriceFormula = new PayrollGroupPriceFormula()
                            {
                                FromDate = templatePayrollGroupPriceFormula.FromDate,
                                ToDate = templatePayrollGroupPriceFormula.ToDate,
                                ShowOnEmployee = templatePayrollGroupPriceFormula.ShowOnEmployee,

                                //Set references
                                PayrollGroup = payrollGroup,
                                PayrollPriceFormulaId = payrollPriceFormula.PayrollPriceFormulaId,
                            };

                            SetCreatedProperties(payrollGroupPriceFormula);
                            payrollGroup.PayrollGroupPriceFormula.Add(payrollGroupPriceFormula);
                        }

                        #endregion

                        #region PayrollGroupVacationGroup

                        List<PayrollGroupVacationGroup> templatePayrollGroupVacationGroups = PayrollManager.GetPayrollGroupVacationGroups(templatePayrollGroup.PayrollGroupId, false);
                        foreach (PayrollGroupVacationGroup templatePayrollGroupVacationGroup in templatePayrollGroupVacationGroups.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            if (!templatePayrollGroupVacationGroup.VacationGroupReference.IsLoaded)
                                templatePayrollGroupVacationGroup.VacationGroupReference.Load();

                            VacationGroup vacationGroup = null;
                            if (vacationGroupMapping.ContainsKey(templatePayrollGroupVacationGroup.VacationGroupId))
                                vacationGroup = vacationGroupMapping[templatePayrollGroupVacationGroup.VacationGroupId];
                            if (vacationGroup == null)
                                continue;

                            PayrollGroupVacationGroup payrollGroupVacationGroup = new PayrollGroupVacationGroup()
                            {
                                Default = templatePayrollGroupVacationGroup.Default,

                                //Set references
                                PayrollGroup = payrollGroup,
                                VacationGroupId = vacationGroup.VacationGroupId,
                            };

                            SetCreatedProperties(payrollGroupVacationGroup);
                            payrollGroup.PayrollGroupVacationGroup.Add(payrollGroupVacationGroup);
                        }

                        #endregion

                        #region PayrollGroupPayrollProduct

                        List<PayrollGroupPayrollProduct> templatePayrollGroupPayrollProducts = PayrollManager.GetPayrollGroupPayrollProducts(templatePayrollGroup.PayrollGroupId);
                        foreach (PayrollGroupPayrollProduct templatePayrollGroupPayrollProduct in templatePayrollGroupPayrollProducts.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            if (!templatePayrollGroupPayrollProduct.PayrollProductReference.IsLoaded)
                                templatePayrollGroupPayrollProduct.PayrollProductReference.Load();

                            PayrollProduct payrollProduct = null;
                            if (payrollProductMapping.ContainsKey(templatePayrollGroupPayrollProduct.ProductId))
                                payrollProduct = payrollProductMapping[templatePayrollGroupPayrollProduct.ProductId];
                            if (payrollProduct == null)
                                continue;

                            var product = entities.Product.FirstOrDefault(f => f.ProductId == payrollProduct.ProductId) as PayrollProduct;

                            PayrollGroupPayrollProduct payrollGroupPayrollProduct = new PayrollGroupPayrollProduct()
                            {
                                Distribute = templatePayrollGroupPayrollProduct.Distribute,

                                //Set references
                                PayrollGroup = payrollGroup,
                                PayrollProduct = product,
                            };

                            SetCreatedProperties(payrollGroupPayrollProduct);
                            payrollGroup.PayrollGroupPayrollProduct.Add(payrollGroupPayrollProduct);
                        }

                        #endregion

                        #region PayrollGroupReport

                        List<PayrollGroupReport> templatePayrollGroupReports = PayrollManager.GetPayrollGroupReports(templateCompanyId, templatePayrollGroup.PayrollGroupId);
                        foreach (PayrollGroupReport templatePayrollGroupReport in templatePayrollGroupReports.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            if (!templatePayrollGroupReport.ReportReference.IsLoaded)
                                templatePayrollGroupReport.ReportReference.Load();

                            Report report = null;
                            if (reportMapping.ContainsKey(templatePayrollGroupReport.ReportId))
                                report = reportMapping[templatePayrollGroupReport.ReportId];
                            if (report == null)
                                continue;

                            PayrollGroupReport payrollGroupReport = new PayrollGroupReport()
                            {
                                SysReportTemplateTypeId = templatePayrollGroupReport.SysReportTemplateTypeId,

                                //Set FK
                                ActorCompanyId = newCompanyId,

                                //Set references
                                PayrollGroup = payrollGroup,
                                ReportId = report.ReportId,
                            };
                            SetCreatedProperties(payrollGroupReport);
                            payrollGroup.PayrollGroupReport.Add(payrollGroupReport);
                        }

                        #endregion
                    }

                    #endregion

                    #region Save

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = LogCopyError("CopyPriceTypesAndPayrollGroups", templateCompanyId, newCompanyId, saved: true);

                    #endregion
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Success = false;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else if (result.Exception != null)
                        base.LogError(result.Exception, this.log);
                    else if (result.ErrorMessage != null)
                        base.LogError(result.ErrorMessage);
                    else
                        base.LogError("Error in CopyPayrollGroupsPriceTypesAndPriceFormulasFromTemplateCompany");

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult CopyTimeAccumulatorsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, EmployeeGroup> employeeGroupMapping, ref Dictionary<int, TimeCode> timeCodeMapping)
        {
            ActionResult result = new ActionResult(true);

            List<TimeAccumulator> templateTimeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(templateCompanyId, loadEmployeeGroupRule: true, loadTimeCode: true, loadPayrollProduct: true, loadInvoiceProduct: true, loadTimeWorkReductionEarning: true);
            List<PayrollProduct> templatePayrollProducts = ProductManager.GetPayrollProducts(templateCompanyId, active: null);
            List<InvoiceProduct> templateInvoiceProducts = ProductManager.GetInvoiceProducts(templateCompanyId, active: null);

            using (CompEntities entities = new CompEntities())
            {
                User user = UserManager.GetUser(entities, userId);
                if (user == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                List<TimeAccumulator> existingTimeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(entities, newCompanyId, loadEmployeeGroupRule: true, loadTimeCode: true, loadPayrollProduct: true, loadTimeWorkReductionEarning: true);
                List<PayrollProduct> existingPayrollProducts = ProductManager.GetPayrollProducts(entities, newCompanyId, active: null);
                List<InvoiceProduct> existingInvoiceProducts = ProductManager.GetInvoiceProducts(entities, newCompanyId, active: null);

                foreach (TimeAccumulator templateTimeAccumulator in templateTimeAccumulators)
                {
                    try
                    {
                        if (existingTimeAccumulators.Any(i => i.Name.Trim().ToLower().Equals(templateTimeAccumulator.Name.Trim().ToLower())))
                            continue;

                        TimeAccumulator newTimeAccumulator = new TimeAccumulator()
                        {
                            Name = templateTimeAccumulator.Name,
                            Description = templateTimeAccumulator.Description,
                            ShowInTimeReports = templateTimeAccumulator.ShowInTimeReports,
                            Type = templateTimeAccumulator.Type,
                            FinalSalary = templateTimeAccumulator.FinalSalary,
                            UseTimeWorkAccount = templateTimeAccumulator.UseTimeWorkAccount,
                            UseTimeWorkReductionWithdrawal = templateTimeAccumulator.UseTimeWorkReductionWithdrawal,

                            //Set FK
                            ActorCompanyId = newCompanyId,
                        };
                        if (templateTimeAccumulator.TimeCodeId.HasValue)
                            newTimeAccumulator.TimeCodeId = timeCodeMapping[templateTimeAccumulator.TimeCodeId.Value]?.TimeCodeId ?? null;
                        
                        SetCreatedProperties(newTimeAccumulator, user);
                        entities.TimeAccumulator.AddObject(newTimeAccumulator);

                        foreach (TimeAccumulatorEmployeeGroupRule templateEmployeeGroupRule in templateTimeAccumulator.TimeAccumulatorEmployeeGroupRule.Where(x => x.State == (int)SoeEntityState.Active))
                        {
                            if (!employeeGroupMapping.ContainsKey(templateEmployeeGroupRule.EmployeeGroupId))
                                continue;

                            TimeAccumulatorEmployeeGroupRule newEmployeeGroupRule = new TimeAccumulatorEmployeeGroupRule()
                            {
                                Type = templateEmployeeGroupRule.Type,
                                MinMinutes = templateEmployeeGroupRule.MinMinutes,
                                MaxMinutes = templateEmployeeGroupRule.MaxMinutes,
                                MaxMinutesWarning = templateEmployeeGroupRule.MaxMinutesWarning,
                                MinMinutesWarning = templateEmployeeGroupRule.MinMinutesWarning,
                                ThresholdMinutes = templateEmployeeGroupRule.ThresholdMinutes,

                                //Set FK
                                EmployeeGroupId = employeeGroupMapping[templateEmployeeGroupRule.EmployeeGroupId].EmployeeGroupId,
                            };
                            SetCreatedProperties(newEmployeeGroupRule, user);
                            entities.TimeAccumulatorEmployeeGroupRule.AddObject(newEmployeeGroupRule);
                            newTimeAccumulator.TimeAccumulatorEmployeeGroupRule.Add(newEmployeeGroupRule);

                            if (templateEmployeeGroupRule.MinTimeCodeId.HasValue && timeCodeMapping.ContainsKey(templateEmployeeGroupRule.MinTimeCodeId.Value))
                                newEmployeeGroupRule.MinTimeCodeId = timeCodeMapping[templateEmployeeGroupRule.MinTimeCodeId.Value]?.TimeCodeId;
                            if (templateEmployeeGroupRule.MaxTimeCodeId.HasValue && timeCodeMapping.ContainsKey(templateEmployeeGroupRule.MaxTimeCodeId.Value))
                                newEmployeeGroupRule.MaxTimeCodeId = timeCodeMapping[templateEmployeeGroupRule.MaxTimeCodeId.Value]?.TimeCodeId;
                        }

                        foreach (TimeAccumulatorTimeCode templateTimeAccumulatorTimeCode in templateTimeAccumulator.TimeAccumulatorTimeCode)
                        {
                            if (!timeCodeMapping.ContainsKey(templateTimeAccumulatorTimeCode.TimeCodeId))
                                continue;

                            TimeAccumulatorTimeCode timeAccumulatorTimeCode = new TimeAccumulatorTimeCode()
                            {
                                Factor = templateTimeAccumulatorTimeCode.Factor,
                                IsHeadTimeCode = templateTimeAccumulatorTimeCode.IsHeadTimeCode,
                                ImportDefault = templateTimeAccumulatorTimeCode.ImportDefault,

                                //Set FK
                                TimeCodeId = timeCodeMapping[templateTimeAccumulatorTimeCode.TimeCodeId].TimeCodeId,
                            };
                            entities.TimeAccumulatorTimeCode.AddObject(timeAccumulatorTimeCode);
                            newTimeAccumulator.TimeAccumulatorTimeCode.Add(timeAccumulatorTimeCode);
                        }

                        foreach (TimeAccumulatorPayrollProduct templateTimeAccumulatorPayrollProduct in templateTimeAccumulator.TimeAccumulatorPayrollProduct)
                        {
                            PayrollProduct existingPayrollProduct = GetExistingPayrollProduct(existingPayrollProducts, templatePayrollProducts, templateTimeAccumulatorPayrollProduct.PayrollProductId);
                            if (existingPayrollProduct == null)
                                continue;

                            TimeAccumulatorPayrollProduct timeAccumulatorPayrollProduct = new TimeAccumulatorPayrollProduct()
                            {
                                Factor = templateTimeAccumulatorPayrollProduct.Factor,

                                //Set FK
                                PayrollProductId = existingPayrollProduct.ProductId,
                            };
                            entities.TimeAccumulatorPayrollProduct.AddObject(timeAccumulatorPayrollProduct);
                            newTimeAccumulator.TimeAccumulatorPayrollProduct.Add(timeAccumulatorPayrollProduct);
                        }

                        foreach (TimeAccumulatorInvoiceProduct templateTimeInvoiceProduct in templateTimeAccumulator.TimeAccumulatorInvoiceProduct)
                        {
                            InvoiceProduct existingInvoiceProduct = GetExistingInvoiceProduct(existingInvoiceProducts, templateInvoiceProducts, templateTimeInvoiceProduct.InvoiceProductId);
                            if (existingInvoiceProduct == null)
                                continue;

                            TimeAccumulatorInvoiceProduct timeAccumulatorInvoiceProduct = new TimeAccumulatorInvoiceProduct()
                            {
                                Factor = templateTimeInvoiceProduct.Factor,

                                //Set FK
                                InvoiceProductId = templateTimeInvoiceProduct.InvoiceProductId,
                            };
                            entities.TimeAccumulatorInvoiceProduct.AddObject(timeAccumulatorInvoiceProduct);
                            newTimeAccumulator.TimeAccumulatorInvoiceProduct.Add(timeAccumulatorInvoiceProduct);
                        }


                        if(templateTimeAccumulator.TimeWorkReductionEarning != null)
                        {
                            if (templateTimeAccumulator.TimeWorkReductionEarning.State != (int)SoeEntityState.Active)
                                continue;

                            var newTimeWorkReductionEarning = TimeWorkReductionEarning.Create(
                                templateTimeAccumulator.TimeWorkReductionEarning.MinutesWeight,
                                templateTimeAccumulator.TimeWorkReductionEarning.PeriodType,
                                newTimeAccumulator
                            );
                            SetCreatedProperties(newTimeWorkReductionEarning, user);

                            foreach (TimeAccumulatorTimeWorkReductionEarningEmployeeGroup templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup in templateTimeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Where(w=> w.TimeWorkReductionEarningId == templateTimeAccumulator.TimeWorkReductionEarning.TimeWorkReductionEarningId && w.State == (int)SoeEntityState.Active))
                            {
                                if (!employeeGroupMapping.ContainsKey(templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.EmployeeGroupId))
                                    continue;

                                TimeAccumulatorTimeWorkReductionEarningEmployeeGroup newTimeAccumulatorTimeWorkReductionEarningEmployeeGroup = new TimeAccumulatorTimeWorkReductionEarningEmployeeGroup()
                                {
                                    EmployeeGroupId = employeeGroupMapping[templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.EmployeeGroupId].EmployeeGroupId,
                                    DateFrom = templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.DateFrom,
                                    DateTo = templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.DateTo,

                                    //Set FK
                                    TimeWorkReductionEarningId = templateTimeAccumulatorTimeWorkReductionEarningEmployeeGroup.TimeWorkReductionEarningId,
                                };
                                entities.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.AddObject(newTimeAccumulatorTimeWorkReductionEarningEmployeeGroup);
                                newTimeAccumulator.TimeWorkReductionEarning.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup.Add(newTimeAccumulatorTimeWorkReductionEarningEmployeeGroup);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("TimeAccumulator", "TimeAccumulatorId", templateTimeAccumulator.TimeAccumulatorId, "", templateTimeAccumulator.Name, templateCompanyId, newCompanyId, ex);
                    }
                }

                if (result.Success)
                    result = SaveChanges(entities);
            }

            return result;
        }

        public ActionResult CopyTimeRulesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, EmployeeGroup> employeeGroupMapping, ref Dictionary<int, TimeCode> timeCodeMapping, ref Dictionary<int, TimeDeviationCause> timeDeviationCausesMapping, ref Dictionary<int, TimeScheduleType> timeScheduleTypeMapping, ref Dictionary<int, DayType> dayTypeMapping)
        {
            ActionResult result = new ActionResult();

            List<TimeRule> templateTimeRules = TimeRuleManager.GetAllTimeRulesRecursive(templateCompanyId);
            List<TimeRule> existingTimeRules = TimeRuleManager.GetAllTimeRulesRecursive(newCompanyId);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    User user = UserManager.GetUser(entities, userId);
                    if (user == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                    foreach (TimeRule templateTimeRule in templateTimeRules)
                    {
                        try
                        {
                            if (existingTimeRules.Any(r => r.Type == templateTimeRule.Type && r.Name.Trim().ToLower().Equals(templateTimeRule.Name.Trim().ToLower())))
                                continue;
                            if (!timeCodeMapping.ContainsKey(templateTimeRule.TimeCodeId))
                                continue;

                            TimeRule newTimeRule = new TimeRule()
                            {
                                Type = templateTimeRule.Type,
                                Name = templateTimeRule.Name,
                                Description = templateTimeRule.Description,
                                StartDate = templateTimeRule.StartDate,
                                StopDate = templateTimeRule.StopDate,
                                RuleStartDirection = templateTimeRule.RuleStartDirection,
                                RuleStopDirection = templateTimeRule.RuleStopDirection,
                                Factor = templateTimeRule.Factor,
                                BelongsToGroup = templateTimeRule.BelongsToGroup,
                                IsInconvenientWorkHours = templateTimeRule.IsInconvenientWorkHours,
                                TimeCodeMaxLength = templateTimeRule.TimeCodeMaxLength,
                                TimeCodeMaxPerDay = templateTimeRule.TimeCodeMaxPerDay,
                                Sort = templateTimeRule.Sort,
                                Internal = templateTimeRule.Internal,
                                StandardMinutes = templateTimeRule.StandardMinutes,
                                BreakIfAnyFailed = templateTimeRule.BreakIfAnyFailed,
                                AdjustStartToTimeBlockStart = templateTimeRule.AdjustStartToTimeBlockStart,

                                //Set FK
                                ActorCompanyId = newCompanyId,
                                TimeCodeId = timeCodeMapping[templateTimeRule.TimeCodeId].TimeCodeId,
                            };
                            SetCreatedProperties(newTimeRule, user);
                            entities.TimeRule.AddObject(newTimeRule);

                            foreach (TimeRuleRow templateTimeRuleRow in templateTimeRule.TimeRuleRow)
                            {
                                if (!timeDeviationCausesMapping.ContainsKey(templateTimeRuleRow.TimeDeviationCauseId))
                                    continue; //TimeDeviationCause may be deleted and we cant create a TimeRule without TimeDeviationCause

                                TimeRuleRow newTimeRuleRow = new TimeRuleRow()
                                {
                                    //Set FK
                                    ActorCompanyId = newCompanyId,
                                };
                                entities.TimeRuleRow.AddObject(newTimeRuleRow);

                                newTimeRuleRow.TimeDeviationCauseId = timeDeviationCausesMapping[templateTimeRuleRow.TimeDeviationCauseId].TimeDeviationCauseId;
                                if (templateTimeRuleRow.EmployeeGroupId.HasValue && employeeGroupMapping.ContainsKey(templateTimeRuleRow.EmployeeGroupId.Value))
                                    newTimeRuleRow.EmployeeGroupId = employeeGroupMapping[templateTimeRuleRow.EmployeeGroupId.Value].EmployeeGroupId;
                                if (templateTimeRuleRow.TimeScheduleTypeId.HasValue && timeScheduleTypeMapping.ContainsKey(templateTimeRuleRow.TimeScheduleTypeId.Value))
                                    newTimeRuleRow.TimeScheduleTypeId = timeScheduleTypeMapping[templateTimeRuleRow.TimeScheduleTypeId.Value].TimeScheduleTypeId;
                                if (templateTimeRuleRow.DayTypeId.HasValue && dayTypeMapping.ContainsKey(templateTimeRuleRow.DayTypeId.Value))
                                    newTimeRuleRow.DayTypeId = dayTypeMapping[templateTimeRuleRow.DayTypeId.Value].DayTypeId;

                                newTimeRule.TimeRuleRow.Add(newTimeRuleRow);
                            }

                            foreach (TimeRuleExpression templateTimeRuleExpression in templateTimeRule.TimeRuleExpression)
                            {
                                #region TimeRuleExpression

                                TimeRuleExpression newTimeRuleExpression = new TimeRuleExpression
                                {
                                    IsStart = templateTimeRuleExpression.IsStart,
                                };
                                entities.TimeRuleExpression.AddObject(newTimeRuleExpression);

                                #endregion

                                foreach (TimeRuleOperand templateTimeRuleOperand in templateTimeRuleExpression.TimeRuleOperand)
                                {
                                    #region TimeRuleOperand

                                    TimeRuleOperand newTimeRuleOperand = new TimeRuleOperand
                                    {
                                        OperatorType = templateTimeRuleOperand.OperatorType,
                                        LeftValueType = templateTimeRuleOperand.LeftValueType,
                                        RightValueType = templateTimeRuleOperand.RightValueType,
                                        Minutes = templateTimeRuleOperand.Minutes,
                                        ComparisonOperator = templateTimeRuleOperand.ComparisonOperator,
                                        OrderNbr = templateTimeRuleOperand.OrderNbr,
                                    };
                                    entities.TimeRuleOperand.AddObject(newTimeRuleOperand);

                                    if (templateTimeRuleOperand.LeftValueId.HasValue)
                                    {
                                        if (templateTimeRuleOperand.IsLeftValueTimeCode())
                                            newTimeRuleOperand.LeftValueId = timeCodeMapping.GetValue(templateTimeRuleOperand.LeftValueId.Value)?.TimeCodeId;
                                        else
                                            newTimeRuleOperand.LeftValueId = templateTimeRuleOperand.LeftValueId;
                                    }
                                    if (templateTimeRuleOperand.RightValueId.HasValue)
                                    {
                                        if (templateTimeRuleOperand.IsRightValueTimeCode())
                                            newTimeRuleOperand.RightValueId = timeCodeMapping.GetValue(templateTimeRuleOperand.LeftValueId.Value)?.TimeCodeId;
                                        else
                                            newTimeRuleOperand.RightValueId = templateTimeRuleOperand.RightValueId;
                                    }

                                    #endregion

                                    #region TimeRuleExpression (recursive)

                                    if (!templateTimeRuleOperand.TimeRuleExpressionRecursiveReference.IsLoaded)
                                        templateTimeRuleOperand.TimeRuleExpressionRecursiveReference.Load();

                                    if (templateTimeRuleOperand.TimeRuleExpressionRecursive != null)
                                    {
                                        TimeRuleExpression newTimeRuleExpressionRecursive = new TimeRuleExpression
                                        {
                                            IsStart = templateTimeRuleOperand.TimeRuleExpressionRecursive.IsStart,
                                        };
                                        newTimeRuleOperand.TimeRuleExpressionRecursive = newTimeRuleExpressionRecursive;
                                    }

                                    #endregion

                                    newTimeRuleExpression.TimeRuleOperand.Add(newTimeRuleOperand);
                                }

                                newTimeRule.TimeRuleExpression.Add(newTimeRuleExpression);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogCopyError("TimeRule", "TimeRuleId", templateTimeRule.TimeRuleId, "", templateTimeRule.Name, templateCompanyId, newCompanyId, ex);
                        }
                    }

                    result = SaveChanges(entities);
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Success = false;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else if (result.Exception != null)
                        base.LogError(result.Exception, this.log);
                    else if (result.ErrorMessage != null)
                        base.LogError(result.ErrorMessage);
                    else
                        base.LogError("Error in CopyTimeRulesFromTemplateCompany");

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult CopyTimeAbsenseRulesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            try
            {
                var templateInput = new GetTimeAbsenceRulesInput(templateCompanyId)
                {
                    LoadTimeCode = true,
                    LoadCompany = true,
                    LoadEmployeeGroups = true,
                    LoadRows = true,
                    LoadRowProducts = true,
                };
                List<TimeAbsenceRuleHead> templateTimeAbsenceRuleHeads = TimeRuleManager.GetTimeAbsenceRules(templateInput);
                var newInput = new GetTimeAbsenceRulesInput(newCompanyId)
                {
                    LoadTimeCode = true,
                    LoadCompany = true,
                    LoadEmployeeGroups = true,
                    LoadRows = true,
                    LoadRowProducts = true,
                };
                List<TimeAbsenceRuleHead> existingTimeAbsenceRuleHeads = TimeRuleManager.GetTimeAbsenceRules(newInput);
                List<PayrollProduct> templatePayrollProducts = ProductManager.GetPayrollProducts(templateCompanyId, active: null);
                List<PayrollProduct> existingPayrollProducts = ProductManager.GetPayrollProducts(newCompanyId, active: null);
                List<TimeCode> templateTimeCodes = TimeCodeManager.GetTimeCodes(templateCompanyId);
                List<TimeCode> existingTimeCodes = TimeCodeManager.GetTimeCodes(newCompanyId);
                List<EmployeeGroup> templateEmployeeGroups = EmployeeManager.GetEmployeeGroups(templateCompanyId);
                List<EmployeeGroup> existingEmployeeGroups = EmployeeManager.GetEmployeeGroups(newCompanyId);

                using (CompEntities entities = new CompEntities())
                {
                    User user = UserManager.GetUser(entities, userId);
                    if (user == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                    foreach (TimeAbsenceRuleHead templateTimeAbsenceRuleHead in templateTimeAbsenceRuleHeads)
                    {
                        if (existingTimeAbsenceRuleHeads.Any(r => r.Type == templateTimeAbsenceRuleHead.Type && r.Name.Trim().ToLower().Equals(templateTimeAbsenceRuleHead.Name.Trim().ToLower())))
                            continue;

                        TimeAbsenceRuleHead newTimeAbsenceRuleHead = new TimeAbsenceRuleHead()
                        {
                            ActorCompanyId = newCompanyId,
                            Type = templateTimeAbsenceRuleHead.Type,
                            Name = templateTimeAbsenceRuleHead.Name,
                            Description = templateTimeAbsenceRuleHead.Description,
                            State = templateTimeAbsenceRuleHead.State
                        };
                        SetCreatedProperties(newTimeAbsenceRuleHead);
                        entities.TimeAbsenceRuleHead.AddObject(newTimeAbsenceRuleHead);

                        #region TimeCode

                        if (templateTimeCodes != null && existingTimeCodes != null)
                        {
                            TimeCode templateTimeCode = templateTimeCodes.FirstOrDefault(p => p.TimeCodeId == templateTimeAbsenceRuleHead.TimeCodeId);
                            if (templateTimeCode != null)
                            {
                                TimeCode newTimeCode = existingTimeCodes.FirstOrDefault(p => p.Code.Trim().ToLower().Equals(templateTimeCode.Code.Trim().ToLower()));
                                if (newTimeCode != null)
                                {
                                    newTimeAbsenceRuleHead.TimeCodeId = newTimeCode.TimeCodeId;
                                }
                                else
                                {
                                    string error = $"TimeCode not found {templateTimeCode.Code} {templateTimeCode.Name}";
                                    LogError("CopyTimeAbsenseRulesFromTemplateCompany " + error);
                                    continue;
                                }
                            }
                        }

                        #endregion

                        #region EmployeeGroup

                        List<int> templateEmployeeGroupIds = templateTimeAbsenceRuleHead.GetEmployeeGroupIds();
                        if (!templateEmployeeGroupIds.IsNullOrEmpty() && templateEmployeeGroups != null && existingEmployeeGroups != null)
                        {
                            foreach (var templateEmployeeGroupId in templateEmployeeGroupIds)
                            {
                                EmployeeGroup templateEmployeeGroup = templateEmployeeGroups.FirstOrDefault(p => p.EmployeeGroupId == templateEmployeeGroupId);
                                if (templateEmployeeGroup == null)
                                    continue;

                                EmployeeGroup newEmployeeGroup = existingEmployeeGroups.FirstOrDefault(p => p.Name.Trim().ToLower().Equals(templateEmployeeGroup.Name.Trim().ToLower()));
                                if (newEmployeeGroup == null)
                                {
                                    LogError($"CopyTimeAbsenseRulesFromTemplateCompany: EmployeeGroup not found {templateEmployeeGroup.Name}");
                                    continue;
                                }

                                TimeRuleManager.CreateTimeAbsenceRuleHeadEmployeeGroup(entities, newTimeAbsenceRuleHead, newEmployeeGroup.EmployeeGroupId);
                            }
                        }

                        #endregion

                        #region TimeAbsenceRuleRow

                        foreach (TimeAbsenceRuleRow templateTimeAbsenceRuleRow in templateTimeAbsenceRuleHead.TimeAbsenceRuleRow.Where(i => i.State == (int)SoeEntityState.Active))
                        {
                            TimeAbsenceRuleRow newTimeAbsenceRuleRow = new TimeAbsenceRuleRow()
                            {
                                HasMultiplePayrollProducts = templateTimeAbsenceRuleRow.HasMultiplePayrollProducts,
                                Type = templateTimeAbsenceRuleRow.Type,
                                Scope = templateTimeAbsenceRuleRow.Scope,
                                Start = templateTimeAbsenceRuleRow.Start,
                                Stop = templateTimeAbsenceRuleRow.Stop,
                                State = templateTimeAbsenceRuleRow.State
                            };
                            SetCreatedProperties(newTimeAbsenceRuleRow);

                            #region PayrollProduct

                            if (templatePayrollProducts != null && templateTimeAbsenceRuleRow.PayrollProductId.HasValue)
                            {
                                PayrollProduct existingPayrollProduct = GetExistingPayrollProduct(existingPayrollProducts, templatePayrollProducts, templateTimeAbsenceRuleRow.PayrollProductId.Value);
                                if (existingPayrollProduct == null)
                                {
                                    LogError($"CopyTimeAbsenseRulesFromTemplateCompany: PayrollProduct not found {templatePayrollProducts.FirstOrDefault(i => i.ProductId == templateTimeAbsenceRuleRow.PayrollProductId)?.NumberAndName ?? ""}");
                                    continue;
                                }

                                newTimeAbsenceRuleRow.PayrollProductId = existingPayrollProduct.ProductId;
                            }

                            #endregion

                            #region TimeAbsenceRuleRowPayrollProducts

                            if (templateTimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts != null && templatePayrollProducts != null)
                            {
                                foreach (var templateTimeAbsenceRuleRowPayrollProducts in templateTimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts)
                                {
                                    //SourcePayrollProduct
                                    PayrollProduct existingSourcePayrollProduct = GetExistingPayrollProduct(existingPayrollProducts, templatePayrollProducts, templateTimeAbsenceRuleRowPayrollProducts.SourcePayrollProductId);
                                    if (existingSourcePayrollProduct == null)
                                    {
                                        LogError($"CopyTimeAbsenseRulesFromTemplateCompany: SourcePayrollProduct not found {templatePayrollProducts.FirstOrDefault(i => i.ProductId == templateTimeAbsenceRuleRowPayrollProducts.SourcePayrollProductId)?.NumberAndName ?? ""}");
                                        continue;
                                    }

                                    //TargetPayrollProduct
                                    PayrollProduct existingTargetPayrollProduct = null;
                                    if (templateTimeAbsenceRuleRowPayrollProducts.TargetPayrollProductId.HasValue)
                                    {
                                        existingTargetPayrollProduct = GetExistingPayrollProduct(existingPayrollProducts, templatePayrollProducts, templateTimeAbsenceRuleRowPayrollProducts.TargetPayrollProductId.Value);
                                        if (existingTargetPayrollProduct == null)
                                        {
                                            LogError($"CopyTimeAbsenseRulesFromTemplateCompany: TargetPayrollProduct not found {templatePayrollProducts.FirstOrDefault(i => i.ProductId == templateTimeAbsenceRuleRowPayrollProducts.SourcePayrollProductId)?.NumberAndName ?? ""}");
                                            continue;
                                        }
                                    }

                                    newTimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Add(new TimeAbsenceRuleRowPayrollProducts()
                                    {
                                        SourcePayrollProductId = existingSourcePayrollProduct.ProductId,
                                        TargetPayrollProductId = existingTargetPayrollProduct?.ProductId,
                                    });
                                }
                            }

                            #endregion

                            newTimeAbsenceRuleHead.TimeAbsenceRuleRow.Add(newTimeAbsenceRuleRow);
                        }

                        entities.TimeAbsenceRuleHead.AddObject(newTimeAbsenceRuleHead);

                        #endregion
                    }

                    return SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(false);
            }
        }

        public ActionResult CopyTimeAttestRulesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, ref Dictionary<int, DayType> dayTypeMapping, ref Dictionary<int, EmployeeGroup> employeeGroupMapping, ref Dictionary<int, TimeCode> timeCodeMapping)
        {
            try
            {
                List<AttestRuleHead> templateAttestRuleHeads = AttestManager.GetAttestRuleHeads(SoeModule.Time, templateCompanyId, true, loadEmployeeGroups: true, loadRows: true);
                List<PayrollProduct> templatePayrollProducts = ProductManager.GetPayrollProducts(templateCompanyId, active: null);
                List<InvoiceProduct> templateInvoiceProducts = ProductManager.GetInvoiceProducts(templateCompanyId, null, loadProductUnitAndGroup: false);

                using (CompEntities entities = new CompEntities())
                {
                    User user = UserManager.GetUser(entities, userId);
                    if (user == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "User");

                    List<AttestRuleHead> existingAttestRuleHeads = AttestManager.GetAttestRuleHeads(entities, SoeModule.Time, newCompanyId, true, loadEmployeeGroups: true, loadRows: true);
                    List<EmployeeGroup> existingEmployeeGroups = EmployeeManager.GetEmployeeGroups(entities, newCompanyId);
                    List<PayrollProduct> existingPayrollProducts = ProductManager.GetPayrollProducts(entities, newCompanyId, active: null);
                    List<InvoiceProduct> existingInvoiceProducts = ProductManager.GetInvoiceProducts(entities, newCompanyId, null, loadProductUnitAndGroup: false);

                    foreach (AttestRuleHead templateAttestRuleHead in templateAttestRuleHeads)
                    {
                        if (existingAttestRuleHeads.Any(r => r.Module == templateAttestRuleHead.Module && r.Name.Trim().ToLower().Equals(templateAttestRuleHead.Name.Trim().ToLower())))
                            continue;

                        if (templateAttestRuleHead.DayTypeId.HasValue && !dayTypeMapping.ContainsKey(templateAttestRuleHead.DayTypeId.Value))
                            continue;

                        AttestRuleHead newAttestRuleHead = new AttestRuleHead()
                        {
                            Module = templateAttestRuleHead.Module,
                            Name = templateAttestRuleHead.Name,
                            Description = templateAttestRuleHead.Description,
                            State = templateAttestRuleHead.State,

                            //Set FK
                            ActorCompanyId = newCompanyId,
                        };
                        SetCreatedProperties(newAttestRuleHead, user);
                        entities.AttestRuleHead.AddObject(newAttestRuleHead);

                        if (templateAttestRuleHead.DayTypeId.HasValue)
                            newAttestRuleHead.DayTypeId = dayTypeMapping[templateAttestRuleHead.DayTypeId.Value].DayTypeId;

                        if (templateAttestRuleHead.EmployeeGroup != null)
                        {
                            foreach (EmployeeGroup templateEmployeeGroup in templateAttestRuleHead.EmployeeGroup)
                            {
                                if (!employeeGroupMapping.ContainsKey(templateEmployeeGroup.EmployeeGroupId))
                                    continue;

                                int newEmployeeGroupId = employeeGroupMapping[templateEmployeeGroup.EmployeeGroupId].EmployeeGroupId;
                                EmployeeGroup newEmployeeGroup = existingEmployeeGroups.FirstOrDefault(i => i.EmployeeGroupId == newEmployeeGroupId);
                                if (newEmployeeGroup == null)
                                    continue;

                                newAttestRuleHead.EmployeeGroup.Add(newEmployeeGroup);
                            }
                        }

                        #region Rows

                        foreach (AttestRuleRow templateAttestRuleRow in templateAttestRuleHead.AttestRuleRow)
                        {
                            AttestRuleRow newAttestRuleRow = new AttestRuleRow()
                            {
                                LeftValueType = templateAttestRuleRow.LeftValueType,
                                LeftValueId = 0,
                                ComparisonOperator = templateAttestRuleRow.ComparisonOperator,
                                RightValueType = templateAttestRuleRow.RightValueType,
                                RightValueId = 0,
                                Minutes = templateAttestRuleRow.Minutes,
                            };
                            SetCreatedProperties(newAttestRuleRow, user);

                            switch (templateAttestRuleRow.LeftValueType)
                            {
                                case (int)TermGroup_AttestRuleRowLeftValueType.TimeCode:
                                    newAttestRuleRow.LeftValueId = timeCodeMapping.ContainsKey(templateAttestRuleRow.LeftValueId) ? (timeCodeMapping[templateAttestRuleRow.LeftValueId]?.TimeCodeId ?? 0) : 0;
                                    break;
                                case (int)TermGroup_AttestRuleRowLeftValueType.PayrollProduct:
                                    newAttestRuleRow.LeftValueId = GetExistingPayrollProductId(existingPayrollProducts, templatePayrollProducts, templateAttestRuleRow.LeftValueId);
                                    break;
                                case (int)TermGroup_AttestRuleRowLeftValueType.InvoiceProduct:
                                    newAttestRuleRow.LeftValueId = GetExistingInvoiceProductId(existingInvoiceProducts, templateInvoiceProducts, templateAttestRuleRow.LeftValueId);
                                    break;
                            }

                            switch (templateAttestRuleRow.RightValueType)
                            {
                                case (int)TermGroup_AttestRuleRowRightValueType.TimeCode:
                                    newAttestRuleRow.RightValueId = timeCodeMapping.ContainsKey(templateAttestRuleRow.RightValueId) ? (timeCodeMapping[templateAttestRuleRow.RightValueId]?.TimeCodeId ?? 0) : 0;
                                    break;
                                case (int)TermGroup_AttestRuleRowRightValueType.PayrollProduct:
                                    newAttestRuleRow.RightValueId = GetExistingPayrollProductId(existingPayrollProducts, templatePayrollProducts, templateAttestRuleRow.RightValueId);
                                    break;
                                case (int)TermGroup_AttestRuleRowRightValueType.InvoiceProduct:
                                    newAttestRuleRow.RightValueId = GetExistingInvoiceProductId(existingInvoiceProducts, templateInvoiceProducts, templateAttestRuleRow.RightValueId);
                                    break;
                            }

                            newAttestRuleHead.AttestRuleRow.Add(newAttestRuleRow);
                        }

                        #endregion
                    }

                    return SaveChanges(entities);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(false);
            }
        }

        public ActionResult CopyFollowUpTypesFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Prereq

            List<FollowUpType> templateFollowUpTypes = EmployeeManager.GetFollowUpTypes(templateCompanyId, true);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Always load, could have been added earlier
                List<FollowUpType> existingFollowUpTypes = EmployeeManager.GetFollowUpTypes(entities, newCompanyId, true);

                #endregion

                foreach (FollowUpType templateFollowUpType in templateFollowUpTypes)
                {
                    #region FollowUpType

                    FollowUpType followUpType = existingFollowUpTypes.FirstOrDefault(f => f.Name == templateFollowUpType.Name);
                    if (followUpType == null)
                    {
                        #region Add

                        followUpType = new FollowUpType();
                        followUpType.CopyFrom(templateFollowUpType);
                        SetCreatedProperties(followUpType);

                        newCompany.FollowUpType.Add(followUpType);

                        #endregion
                    }
                    else
                    {
                        #region Update

                        followUpType.CopyFrom(templateFollowUpType);
                        SetModifiedProperties(followUpType);

                        #endregion
                    }

                    #endregion

                    result = SaveChanges(entities);
                    if (!result.Success)
                    {
                        result = LogCopyError("FollowUpType", templateCompanyId, newCompanyId, saved: true);
                        break;
                    }
                }
            }

            return result;
        }

        public ActionResult CopyCompanyExternalCodes(int newCompanyId, int templateCompanyId, Dictionary<int, EmployeeGroup> employeeGroupMapping, Dictionary<int, PayrollGroup> payrollGroupMapping, Dictionary<int, Role> roleMapping, Dictionary<int, AttestRole> attestRoleMapping)
        {
            List<CompanyExternalCode> templateExternalCodes = ActorManager.GetCompanyExternalCodes(templateCompanyId);
            if (templateExternalCodes.IsNullOrEmpty())
                return new ActionResult();

            int counter = 0;

            using (CompEntities entities = new CompEntities())
            {
                List<CompanyExternalCode> newExternalCodes = ActorManager.GetCompanyExternalCodes(entities, newCompanyId);

                if (!employeeGroupMapping.IsNullOrEmpty())
                    counter += CopyCompanyExternalCodes<EmployeeGroup>(entities, newCompanyId, templateExternalCodes, newExternalCodes, TermGroup_CompanyExternalCodeEntity.EmployeeGroup, employeeGroupMapping);
                if (!payrollGroupMapping.IsNullOrEmpty())
                    counter += CopyCompanyExternalCodes<PayrollGroup>(entities, newCompanyId, templateExternalCodes, newExternalCodes, TermGroup_CompanyExternalCodeEntity.PayrollGroup, payrollGroupMapping);
                if (!roleMapping.IsNullOrEmpty())
                    counter += CopyCompanyExternalCodes<Role>(entities, newCompanyId, templateExternalCodes, newExternalCodes, TermGroup_CompanyExternalCodeEntity.Role, roleMapping);
                if (!attestRoleMapping.IsNullOrEmpty())
                    counter += CopyCompanyExternalCodes<AttestRole>(entities, newCompanyId, templateExternalCodes, newExternalCodes, TermGroup_CompanyExternalCodeEntity.AttestRole, attestRoleMapping);

                if (counter == 0)
                    return new ActionResult(true); //nothing to save

                return SaveChanges(entities);
            }
        }

        public ActionResult CopyCompanyCollectiveAgreements(int newCompanyId, int templateCompanyId, Dictionary<int, EmployeeGroup> employeeGroupMapping, Dictionary<int, PayrollGroup> payrollGroupMapping, Dictionary<int, VacationGroup> vacactionGroupMapping)
        {
            ActionResult result = new ActionResult();
            try
            {
                employeeGroupMapping = employeeGroupMapping.IsNullOrEmpty() ? GetEmployeeGroupDict(templateCompanyId, newCompanyId) : employeeGroupMapping;
                payrollGroupMapping = payrollGroupMapping.IsNullOrEmpty() ? GetPayrollGroupDict(templateCompanyId, newCompanyId) : payrollGroupMapping;
                vacactionGroupMapping = vacactionGroupMapping.IsNullOrEmpty() ? GetVacationGroupDict(templateCompanyId, newCompanyId) : vacactionGroupMapping;

                List<EmployeeCollectiveAgreement> templateCollectiveAgreements = EmployeeManager.GetEmployeeCollectiveAgreements(templateCompanyId);
                if (templateCollectiveAgreements.IsNullOrEmpty())
                    return new ActionResult();

                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                    if (newCompany == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    //Always load, could have been added earlie)
                    List<EmployeeCollectiveAgreement> existingCollectiveAgreements = EmployeeManager.GetEmployeeCollectiveAgreements(entities, newCompanyId);

                    #endregion

                    foreach (EmployeeCollectiveAgreement templateCollectiveAgreement in templateCollectiveAgreements)
                    {
                        #region FollowUpType

                        EmployeeCollectiveAgreement collectiveAgreement = existingCollectiveAgreements.FirstOrDefault(t => t.Name == templateCollectiveAgreement.Name);
                        if (collectiveAgreement == null)
                        {
                            #region Add

                            int? payrollGroupId = templateCollectiveAgreement.PayrollGroupId.HasValue && payrollGroupMapping.ContainsKey(templateCollectiveAgreement.PayrollGroupId.Value) ? payrollGroupMapping[templateCollectiveAgreement.PayrollGroupId.Value]?.PayrollGroupId : (int?)null;
                            int? vacationGroupId = templateCollectiveAgreement.VacationGroupId.HasValue && vacactionGroupMapping.ContainsKey(templateCollectiveAgreement.VacationGroupId.Value) ? vacactionGroupMapping[templateCollectiveAgreement.VacationGroupId.Value]?.VacationGroupId : (int?)null;

                            EmployeeGroup employeeGroup = employeeGroupMapping[templateCollectiveAgreement.EmployeeGroupId];
                            if (employeeGroup == null)
                            {
                                var templateName = templateCollectiveAgreement.Name.Length > 4 ? templateCollectiveAgreement.Name.Substring(0, 4) : templateCollectiveAgreement.Name;
                                employeeGroup = entities.EmployeeGroup.FirstOrDefault(f => f.ActorCompanyId == newCompanyId && f.State == (int)SoeEntityState.Active && f.Name.StartsWith(templateName));
                            }
                            if (employeeGroup == null)
                                employeeGroup = entities.EmployeeGroup.FirstOrDefault(f => f.ActorCompanyId == newCompanyId && f.State == (int)SoeEntityState.Active);
                            if (employeeGroup == null)
                                continue;

                            collectiveAgreement = templateCollectiveAgreement.CopyFrom(employeeGroup.EmployeeGroupId, payrollGroupId, vacationGroupId);
                            SetCreatedProperties(collectiveAgreement);
                            newCompany.EmployeeCollectiveAgreement.Add(collectiveAgreement);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            collectiveAgreement.Name = templateCollectiveAgreement.Name;
                            collectiveAgreement.Code = templateCollectiveAgreement.Code;
                            collectiveAgreement.Description = templateCollectiveAgreement.Description;
                            collectiveAgreement.ExternalCode = templateCollectiveAgreement.ExternalCode;
                            SetModifiedProperties(collectiveAgreement);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            result = LogCopyError("FollowUpType", templateCompanyId, newCompanyId, saved: true);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                result.Success = false;
            }

            return result;
        }

        public ActionResult CopyEmployeeTemplates(int newCompanyId, int templateCompanyId, bool createCollectiveAgreementIfNotExist, Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping)
        {
            ActionResult result = new ActionResult();
            try
            {
                payrollPriceFormulaMapping = payrollPriceFormulaMapping.IsNullOrEmpty() ? GetPayrollPriceFormulaDict(templateCompanyId, newCompanyId) : payrollPriceFormulaMapping;

                List<EmployeeTemplate> templateEmployeeTemplates = EmployeeManager.GetEmployeeTemplates(templateCompanyId, loadCollectiveAgreement: true, loadGroups: true, loadRows: true, onlyActive: true);
                if (templateEmployeeTemplates.IsNullOrEmpty())
                    return new ActionResult();

                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                    if (newCompany == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    //Always load, could have been added earlier
                    List<EmployeeTemplate> existingEmployeeTemplates = EmployeeManager.GetEmployeeTemplates(entities, newCompanyId, loadCollectiveAgreement: true, loadGroups: true, loadRows: true, onlyActive: true);
                    DateTime batchTimeStamp = DateTime.Now;

                    #endregion

                    foreach (EmployeeTemplate templateEmployeeTemplate in templateEmployeeTemplates)
                    {
                        #region FollowUpType

                        EmployeeTemplate existingEmployeeTemplate = existingEmployeeTemplates.FirstOrDefault(t => t.Name == templateEmployeeTemplate.Name);
                        if (existingEmployeeTemplate != null)
                            continue;

                        if (existingEmployeeTemplate == null)
                        {
                            #region Add

                            var templateCollectiveAgreement = templateEmployeeTemplate.EmployeeCollectiveAgreement;
                            if (templateCollectiveAgreement == null)
                                continue;

                            var collectiveAgreement = entities.EmployeeCollectiveAgreement.FirstOrDefault(f => f.ActorCompanyId == newCompanyId && f.Name == templateCollectiveAgreement.Name);
                            if (collectiveAgreement == null)
                            {
                                if (!createCollectiveAgreementIfNotExist)
                                    continue;

                                var any = entities.EmployeeCollectiveAgreement.FirstOrDefault(f => f.ActorCompanyId == newCompanyId && f.State == (int)SoeEntityState.Active);
                                if (any != null)
                                    collectiveAgreement = any;
                                else
                                {
                                    var firstEmployeeGroup = entities.EmployeeGroup.FirstOrDefault(f => f.ActorCompanyId == newCompanyId && f.State == (int)SoeEntityState.Active);
                                    if (firstEmployeeGroup != null)
                                    {
                                        collectiveAgreement = new EmployeeCollectiveAgreement() { ActorCompanyId = newCompanyId, Name = firstEmployeeGroup.Name, EmployeeGroup = firstEmployeeGroup };
                                    }
                                }
                            }

                            if (collectiveAgreement == null)
                                continue;

                            existingEmployeeTemplate = new EmployeeTemplate()
                            {
                                Code = templateEmployeeTemplate.Code,
                                ExternalCode = templateEmployeeTemplate.ExternalCode,
                                Name = templateEmployeeTemplate.Name,
                                Description = templateEmployeeTemplate.Description,
                                Title = templateEmployeeTemplate.Title,
                                EmployeeCollectiveAgreement = collectiveAgreement
                            };
                            newCompany.EmployeeTemplate.Add(existingEmployeeTemplate);
                            SetCreatedProperties(existingEmployeeTemplate, created: batchTimeStamp);

                            foreach (var templateGroup in templateEmployeeTemplate.EmployeeTemplateGroup.Where(w => w.State == (int)SoeEntityState.Active).OrderBy(o => o.SortOrder))
                            {
                                var newGroup = new EmployeeTemplateGroup()
                                {
                                    Code = templateGroup.Code,
                                    Name = templateGroup.Name,
                                    Description = templateGroup.Description,
                                    SortOrder = templateGroup.SortOrder
                                };
                                SetCreatedProperties(newGroup, created: batchTimeStamp);
                                existingEmployeeTemplate.EmployeeTemplateGroup.Add(newGroup);

                                foreach (var templateRow in templateGroup.EmployeeTemplateGroupRow.Where(w => w.State == (int)SoeEntityState.Active))
                                {
                                    List<TermGroup_EmployeeTemplateGroupRowType> invalidTypesForCopyRightNowToday = new List<TermGroup_EmployeeTemplateGroupRowType>()
                                    {
                                        TermGroup_EmployeeTemplateGroupRowType.ExtraFieldAccount, //Needs for ExtraField on accounts to be added in own method
                                        TermGroup_EmployeeTemplateGroupRowType.ExtraFieldEmployee, //Needs for ExtraField on employee to be added in own method
                                    };

                                    if (invalidTypesForCopyRightNowToday.Contains((TermGroup_EmployeeTemplateGroupRowType)templateRow.Type))
                                        continue;

                                    var newRow = new EmployeeTemplateGroupRow()
                                    {
                                        Type = templateRow.Type,
                                        MandatoryLevel = templateRow.MandatoryLevel,
                                        RegistrationLevel = templateRow.RegistrationLevel,
                                        Title = templateRow.Title,
                                        DefaultValue = templateRow.DefaultValue,
                                        Comment = templateRow.Comment,
                                        Row = templateRow.Row,
                                        StartColumn = templateRow.StartColumn,
                                        SpanColumns = templateRow.SpanColumns,
                                        Format = templateRow.Format,
                                        Created = templateRow.Created,
                                        CreatedBy = templateRow.CreatedBy,
                                        Modified = templateRow.Modified,
                                        ModifiedBy = templateRow.ModifiedBy,
                                        HideInReport = templateRow.HideInReport,
                                        HideInReportIfEmpty = templateRow.HideInReportIfEmpty,
                                        HideInRegistration = templateRow.HideInRegistration,
                                        HideInEmploymentRegistration = templateRow.HideInEmploymentRegistration,
                                        Entity = templateRow.Entity,
                                    };
                                    SetCreatedProperties(newRow, created: batchTimeStamp);

                                    if ((TermGroup_EmployeeTemplateGroupRowType)templateRow.Type == TermGroup_EmployeeTemplateGroupRowType.PayrollFormula && int.TryParse(templateRow.DefaultValue, out int formulaId))
                                    {
                                        PayrollPriceFormula formula = GetPayrollPriceFormula(payrollPriceFormulaMapping, formulaId);
                                        if (formula != null)
                                            newRow.DefaultValue = formula.PayrollPriceFormulaId.ToString();
                                    }

                                    newGroup.EmployeeTemplateGroupRow.Add(newRow);
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            #region Update

                            existingEmployeeTemplate.Name = templateEmployeeTemplate.Name;
                            existingEmployeeTemplate.Code = templateEmployeeTemplate.Code;
                            existingEmployeeTemplate.Description = templateEmployeeTemplate.Description;
                            existingEmployeeTemplate.ExternalCode = templateEmployeeTemplate.ExternalCode;
                            SetModifiedProperties(existingEmployeeTemplate);

                            #endregion
                        }

                        #endregion

                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            result = LogCopyError("FollowUpType", templateCompanyId, newCompanyId, saved: true);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                result.Success = false;
            }

            return result;
        }

        #region Help-methods

        private int CopyCompanyExternalCodes<T>(CompEntities entities, int newCompanyId, List<CompanyExternalCode> templateExternalCodes, List<CompanyExternalCode> newExternalCodes, TermGroup_CompanyExternalCodeEntity entity, Dictionary<int, T> mappings)
        {
            int counter = 0;

            List<CompanyExternalCode> templateExternalCodesForEntity = templateExternalCodes.Filter(entity);
            if (templateExternalCodesForEntity.IsNullOrEmpty())
                return counter;

            List<CompanyExternalCode> newExternalCodesForEntity = newExternalCodes.Filter(entity);

            foreach (CompanyExternalCode templateExternalCode in templateExternalCodesForEntity)
            {
                if (!mappings.ContainsKey(templateExternalCode.RecordId))
                    continue;

                int recordId = 0;
                T value = mappings[templateExternalCode.RecordId];
                if (value is EmployeeGroup employeeGroup)
                    recordId = employeeGroup?.EmployeeGroupId ?? 0;
                else if (value is PayrollGroup payrollGroup)
                    recordId = payrollGroup?.PayrollGroupId ?? 0;
                else if (value is Role role)
                    recordId = role?.RoleId ?? 0;
                else if (value is AttestRole attestRole)
                    recordId = attestRole?.AttestRoleId ?? 0;

                CompanyExternalCode newExternalCode = newExternalCodesForEntity.Get(entity, recordId);
                if (newExternalCode == null)
                {
                    newExternalCode = new CompanyExternalCode()
                    {
                        RecordId = recordId,
                        Entity = (int)entity,
                        ExternalCode = templateExternalCode.ExternalCode,

                        //Set FK
                        ActorCompanyId = newCompanyId
                    };
                    entities.CompanyExternalCode.AddObject(newExternalCode);
                }
                else
                {
                    newExternalCode.ExternalCode = templateExternalCode.ExternalCode;
                }

                counter++;
            }

            return counter;
        }

        private Dictionary<int, PayrollGroup> GetPayrollGroupDict(int templateCompanyId, int newCompanyId)
        {
            Dictionary<int, PayrollGroup> PayrollGroupMapping = new Dictionary<int, PayrollGroup>();
            List<PayrollGroup> templatePayrollGroups = PayrollManager.GetPayrollGroups(templateCompanyId);
            List<PayrollGroup> existingPayrollGroups = PayrollManager.GetPayrollGroups(newCompanyId);
            foreach (PayrollGroup templatePayrollGroup in templatePayrollGroups.Where(i => i.State == (int)SoeEntityState.Active))
            {
                PayrollGroup payrollGroup = existingPayrollGroups.FirstOrDefault(pt => pt.Name == templatePayrollGroup.Name);

                if (payrollGroup == null && !string.IsNullOrEmpty(templatePayrollGroup.ExternalCodesString))
                    payrollGroup = existingPayrollGroups.FirstOrDefault(pt => !string.IsNullOrEmpty(pt.ExternalCodesString) && pt.ExternalCodesString == templatePayrollGroup.ExternalCodesString);

                if (!PayrollGroupMapping.ContainsKey(templatePayrollGroup.PayrollGroupId))
                    PayrollGroupMapping.Add(templatePayrollGroup.PayrollGroupId, payrollGroup);
            }

            return PayrollGroupMapping;
        }

        private Dictionary<int, VacationGroup> GetVacationGroupDict(int templateCompanyId, int newCompanyId)
        {
            Dictionary<int, VacationGroup> VacationGroupMapping = new Dictionary<int, VacationGroup>();
            List<VacationGroup> templateVacationGroups = PayrollManager.GetVacationGroups(templateCompanyId);
            List<VacationGroup> existingVacationGroups = PayrollManager.GetVacationGroups(newCompanyId);
            foreach (VacationGroup templateVacationGroup in templateVacationGroups.Where(i => i.State == (int)SoeEntityState.Active))
            {
                VacationGroup vacationGroup = existingVacationGroups.FirstOrDefault(pt => pt.Name == templateVacationGroup.Name);

                if (vacationGroup == null && !string.IsNullOrEmpty(templateVacationGroup.ExternalCodesString))
                    vacationGroup = existingVacationGroups.FirstOrDefault(pt => !string.IsNullOrEmpty(pt.ExternalCodesString) && pt.ExternalCodesString == templateVacationGroup.ExternalCodesString);

                if (!VacationGroupMapping.ContainsKey(templateVacationGroup.VacationGroupId))
                    VacationGroupMapping.Add(templateVacationGroup.VacationGroupId, vacationGroup);
            }

            return VacationGroupMapping;
        }

        private Dictionary<int, EmployeeGroup> GetEmployeeGroupDict(int templateCompanyId, int newCompanyId)
        {
            Dictionary<int, EmployeeGroup> employeeGroupMapping = new Dictionary<int, EmployeeGroup>();
            List<EmployeeGroup> templateEmployeeGroups = EmployeeManager.GetEmployeeGroups(templateCompanyId);
            List<EmployeeGroup> existingEmployeeGroups = EmployeeManager.GetEmployeeGroups(newCompanyId);
            foreach (EmployeeGroup templateEmployeeGroup in templateEmployeeGroups.Where(i => i.State == (int)SoeEntityState.Active))
            {
                EmployeeGroup employeeGroup = existingEmployeeGroups.FirstOrDefault(pt => pt.Name == templateEmployeeGroup.Name);

                if (employeeGroup == null && !string.IsNullOrEmpty(templateEmployeeGroup.ExternalCodesString))
                    employeeGroup = existingEmployeeGroups.FirstOrDefault(pt => !string.IsNullOrEmpty(pt.ExternalCodesString) && pt.ExternalCodesString == templateEmployeeGroup.ExternalCodesString);

                if (!employeeGroupMapping.ContainsKey(templateEmployeeGroup.EmployeeGroupId))
                    employeeGroupMapping.Add(templateEmployeeGroup.EmployeeGroupId, employeeGroup);
            }

            return employeeGroupMapping;
        }

        private int? GetPayrollPriceTypeId(Dictionary<int, PayrollPriceType> payrollPriceTypeMapping, int? templatePayrollPriceTypeId)
        {
            PayrollPriceType priceType = GetPayrollPriceType(payrollPriceTypeMapping, templatePayrollPriceTypeId);
            if (priceType != null)
                return priceType.PayrollPriceTypeId;
            else
                return null;
        }

        private PayrollPriceType GetPayrollPriceType(Dictionary<int, PayrollPriceType> payrollPriceTypeMapping, int? templatePayrollPriceTypeId)
        {
            if (!templatePayrollPriceTypeId.HasValue)
                return null;

            PayrollPriceType payrollPriceType = null;
            if (payrollPriceTypeMapping.ContainsKey(templatePayrollPriceTypeId.Value))
                payrollPriceType = payrollPriceTypeMapping[templatePayrollPriceTypeId.Value];

            return payrollPriceType;
        }

        private Dictionary<int, PayrollPriceFormula> GetPayrollPriceFormulaDict(int templateCompanyId, int newCompanyId)
        {
            Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping = new Dictionary<int, PayrollPriceFormula>();
            List<PayrollPriceFormula> templatePayrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(templateCompanyId);
            List<PayrollPriceFormula> existingPayrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(newCompanyId);
            foreach (PayrollPriceFormula templatePayrollPriceFormula in templatePayrollPriceFormulas.Where(i => i.State == (int)SoeEntityState.Active))
            {
                PayrollPriceFormula existingPayrollPriceFormula = existingPayrollPriceFormulas.FirstOrDefault(pt => pt.Name == templatePayrollPriceFormula.Name || pt.Code == templatePayrollPriceFormula.Code);

                if (!payrollPriceFormulaMapping.ContainsKey(templatePayrollPriceFormula.PayrollPriceFormulaId))
                    payrollPriceFormulaMapping.Add(templatePayrollPriceFormula.PayrollPriceFormulaId, existingPayrollPriceFormula);
            }

            return payrollPriceFormulaMapping;
        }

        private int? GetPayrollPriceFormulaId(Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping, int? templatePayrollPriceFormulaId)
        {
            PayrollPriceFormula formula = GetPayrollPriceFormula(payrollPriceFormulaMapping, templatePayrollPriceFormulaId);
            if (formula != null)
                return formula.PayrollPriceFormulaId;
            else
                return null;
        }

        private PayrollPriceFormula GetPayrollPriceFormula(Dictionary<int, PayrollPriceFormula> payrollPriceFormulaMapping, int? templatePayrollPriceFormulaId)
        {
            if (!templatePayrollPriceFormulaId.HasValue)
                return null;

            PayrollPriceFormula payrollPayrollPriceFormula = null;
            if (payrollPriceFormulaMapping.ContainsKey(templatePayrollPriceFormulaId.Value))
                payrollPayrollPriceFormula = payrollPriceFormulaMapping[templatePayrollPriceFormulaId.Value];

            return payrollPayrollPriceFormula;
        }

        private void AddDayTypeFromTemplateToNewCompany(Company newCompany, ref DayType dayType, DayType prototype)
        {
            if (dayType == null)
                dayType = new DayType();

            SetCreatedProperties(dayType);
            dayType.CopyFrom(prototype);
            newCompany.DayType.Add(dayType);
        }

        private void UpdateExistingDayType(ref DayType dayType, DayType prototype)
        {
            dayType.CopyFrom(prototype);
            SetModifiedProperties(dayType);
        }

        #endregion

        #endregion

        #region Attest

        public ActionResult CopyAttestFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, SoeModule module, bool copyAttest, ref Dictionary<int, Category> categoryMapping, ref Dictionary<int, AttestRole> attestRoleMapping)
        {
            // Default result is successful
            ActionResult result;

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);
                if (newCompany == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                //Existing for template Company
                List<AttestRole> templateAttestRoles = AttestManager.GetAttestRoles(entities, templateCompanyId, module);
                List<AttestState> templateAttestStates = AttestManager.GetAttestStates(entities, templateCompanyId, TermGroup_AttestEntity.Unknown, module);
                List<AttestTransition> templateAttestTransitions = AttestManager.GetAttestTransitions(entities, TermGroup_AttestEntity.Unknown, module, true, templateCompanyId);
                List<Category> templateCategories = module == SoeModule.Time ? CategoryManager.GetCategories(entities, SoeCategoryType.Employee, templateCompanyId, loadChildren: true) : null;
                List<AttestWorkFlowTemplateHead> templateAttestWorkFlowTemplates = module == SoeModule.Economy ? AttestManager.GetAttestWorkFlowTemplateHeadsIncludingRows(entities, templateCompanyId, TermGroup_AttestEntity.Unknown) : null;

                //Existing for new Company (before copy)
                List<Category> existingCategories = new List<Category>();
                List<AttestRole> existingAttestRoles = new List<AttestRole>();
                List<AttestState> existingAttestStates = new List<AttestState>();
                List<AttestTransition> existingAttestTransitions = new List<AttestTransition>();
                List<AttestWorkFlowTemplateHead> existingAttestWorkFlowTemplates = new List<AttestWorkFlowTemplateHead>();

                //Mappings between template and new
                Dictionary<int, AttestState> attestStateMapping = new Dictionary<int, AttestState>();
                Dictionary<int, AttestTransition> attestTransitionMapping = new Dictionary<int, AttestTransition>();

                #endregion

                #region Categories

                if (templateCategories != null) // For now only employee categories are copied and only when copying attest for Time
                {
                    if (update)
                        existingCategories = CategoryManager.GetCategories(entities, SoeCategoryType.Employee, newCompanyId, loadChildren: true);

                    foreach (Category templateCategory in templateCategories)
                    {
                        #region Category

                        Category category = update ? existingCategories.FirstOrDefault(c => c.Name == templateCategory.Name) : null;
                        if (category == null)
                        {
                            category = new Category();
                            SetCreatedProperties(category);
                        }
                        else
                            SetModifiedProperties(category);

                        category.Company = newCompany;
                        category.Name = templateCategory.Name;
                        category.Code = templateCategory.Code;
                        category.Type = templateCategory.Type;

                        //Mapping
                        categoryMapping.Add(templateCategory.CategoryId, category);

                        #endregion
                    }
                }

                #endregion

                if (copyAttest)
                {
                    #region AttestRoles

                    if (update)
                        existingAttestRoles = AttestManager.GetAttestRoles(entities, newCompanyId, module, loadExternalCode: true);

                    foreach (AttestRole templateAttestRole in templateAttestRoles)
                    {
                        #region AttestRole

                        AttestRole attestRole = update ? existingAttestRoles.FirstOrDefault(a => a.Name == templateAttestRole.Name) : null;
                        if (attestRole == null)
                        {
                            attestRole = new AttestRole();
                            SetCreatedProperties(attestRole);
                        }
                        else
                            SetModifiedProperties(attestRole);


                        attestRole.Company = newCompany;
                        attestRole.Name = templateAttestRole.Name;
                        attestRole.Description = templateAttestRole.Description;
                        attestRole.DefaultMaxAmount = templateAttestRole.DefaultMaxAmount;
                        attestRole.Module = templateAttestRole.Module;
                        attestRole.ShowUncategorized = templateAttestRole.ShowUncategorized;
                        attestRole.ShowAllCategories = templateAttestRole.ShowAllCategories;
                        attestRole.ShowAllSecondaryCategories = templateAttestRole.ShowAllSecondaryCategories;
                        attestRole.ShowTemplateSchedule = templateAttestRole.ShowTemplateSchedule;
                        attestRole.ReminderNoOfDays = templateAttestRole.ReminderNoOfDays;
                        attestRole.ReminderPeriodType = templateAttestRole.ReminderPeriodType;
                        attestRole.AlsoAttestAdditionsFromTime = templateAttestRole.AlsoAttestAdditionsFromTime;
                        attestRole.HumanResourcesPrivacy = templateAttestRole.HumanResourcesPrivacy;
                        attestRole.Sort = templateAttestRole.Sort;

                        //Mappings
                        attestRoleMapping.Add(templateAttestRole.AttestRoleId, attestRole);

                        #endregion
                    }

                    #endregion

                    #region AttestStates

                    if (update)
                        existingAttestStates = AttestManager.GetAttestStates(entities, newCompanyId, TermGroup_AttestEntity.Unknown, module);

                    foreach (AttestState templateAttestState in templateAttestStates)
                    {
                        #region AttestState

                        AttestState attestState = update ? existingAttestStates.FirstOrDefault(a => a.Name == templateAttestState.Name && a.Entity == templateAttestState.Entity) : null;
                        if (attestState == null)
                        {
                            attestState = new AttestState();
                            SetCreatedProperties(attestState);
                        }
                        else
                            SetModifiedProperties(attestState);

                        attestState.Company = newCompany;
                        attestState.Name = templateAttestState.Name;
                        attestState.Description = templateAttestState.Description;
                        attestState.Initial = templateAttestState.Initial;
                        attestState.Closed = templateAttestState.Closed;
                        attestState.Sort = templateAttestState.Sort;
                        attestState.Color = templateAttestState.Color;
                        attestState.Entity = templateAttestState.Entity;
                        attestState.Module = templateAttestState.Module;
                        attestState.Hidden = templateAttestState.Hidden;

                        //Mappings
                        attestStateMapping.Add(templateAttestState.AttestStateId, attestState);

                        #endregion
                    }

                    #endregion

                    #region AttestTransitions

                    if (update)
                        existingAttestTransitions = AttestManager.GetAttestTransitions(entities, TermGroup_AttestEntity.Unknown, module, true, newCompanyId);

                    foreach (AttestTransition templateAttestTransition in templateAttestTransitions)
                    {
                        #region AttestTransition

                        if (templateAttestTransition.AttestStateFrom == null || templateAttestTransition.AttestStateTo == null)
                            continue;

                        AttestTransition attestTransition = update ? existingAttestTransitions.FirstOrDefault(a => a.Name == templateAttestTransition.Name && a.AttestStateFrom != null && a.AttestStateFrom.Entity == templateAttestTransition.AttestStateFrom.Entity) : null;
                        if (attestTransition == null)
                        {
                            attestTransition = new AttestTransition();
                            SetCreatedProperties(attestTransition);
                        }
                        else
                            SetModifiedProperties(attestTransition);

                        attestTransition.Company = newCompany;
                        attestTransition.AttestStateFrom = attestStateMapping[templateAttestTransition.AttestStateFrom.AttestStateId];
                        attestTransition.AttestStateTo = attestStateMapping[templateAttestTransition.AttestStateTo.AttestStateId];
                        attestTransition.Name = templateAttestTransition.Name;
                        attestTransition.Module = templateAttestTransition.Module;
                        attestTransition.NotifyChangeOfAttestState = templateAttestTransition.NotifyChangeOfAttestState;

                        //Mappings
                        attestTransitionMapping.Add(templateAttestTransition.AttestTransitionId, attestTransition);

                        #region AttestRoleTransitionMapping

                        foreach (AttestRole attestRole in templateAttestTransition.AttestRole.Where(x => x.State == (int)SoeEntityState.Active))
                        {
                            #region Update

                            if (update && attestTransition.AttestRole != null && attestTransition.AttestRole.Any(i => i.AttestRoleId == attestRole.AttestRoleId))
                                continue;

                            if (attestRoleMapping.ContainsKey(attestRole.AttestRoleId))
                                attestTransition.AttestRole.Add(attestRoleMapping[attestRole.AttestRoleId]);

                            #endregion
                        }

                        #endregion

                        #endregion
                    }

                    #endregion

                    #region AttestWorkFlowTemplateHead

                    if (templateAttestWorkFlowTemplates != null)
                    {
                        if (update)
                            existingAttestWorkFlowTemplates = AttestManager.GetAttestWorkFlowTemplateHeads(entities, newCompanyId, TermGroup_AttestEntity.Unknown);

                        foreach (AttestWorkFlowTemplateHead head in templateAttestWorkFlowTemplates)
                        {
                            AttestWorkFlowTemplateHead newAttestWorkFlowTemplateHead = update ? existingAttestWorkFlowTemplates.FirstOrDefault(a => a.Name == head.Name && a.Type == head.Type && a.AttestEntity == head.AttestEntity && a.Description == head.Description) : null;
                            bool headExists = newAttestWorkFlowTemplateHead != null;
                            if (!headExists)
                                newAttestWorkFlowTemplateHead = new AttestWorkFlowTemplateHead();

                            newAttestWorkFlowTemplateHead.Type = head.Type;
                            newAttestWorkFlowTemplateHead.Name = head.Name;
                            newAttestWorkFlowTemplateHead.AttestEntity = head.AttestEntity;
                            newAttestWorkFlowTemplateHead.Description = head.Description;
                            newAttestWorkFlowTemplateHead.Company = newCompany;

                            if (headExists)
                            {
                                SetModifiedProperties(newAttestWorkFlowTemplateHead);
                            }
                            else
                            {
                                SetCreatedProperties(newAttestWorkFlowTemplateHead);
                                entities.AttestWorkFlowTemplateHead.AddObject(newAttestWorkFlowTemplateHead);
                                SaveChanges(entities);
                            }

                            foreach (AttestWorkFlowTemplateRow row in head.AttestWorkFlowTemplateRow)
                            {
                                AttestWorkFlowTemplateRow newAttestWorkFlowTemplateRow = update ? newAttestWorkFlowTemplateHead.AttestWorkFlowTemplateRow.FirstOrDefault(a => a.AttestTransition == row.AttestTransition) : null;

                                if (headExists)
                                {
                                    if (newAttestWorkFlowTemplateRow == null)
                                    {
                                        newAttestWorkFlowTemplateRow = new AttestWorkFlowTemplateRow();
                                        SetCreatedProperties(newAttestWorkFlowTemplateRow);
                                        entities.AttestWorkFlowTemplateRow.AddObject(newAttestWorkFlowTemplateRow);
                                    }
                                    else
                                        SetModifiedProperties(newAttestWorkFlowTemplateRow);

                                    newAttestWorkFlowTemplateRow.AttestWorkFlowTemplateHeadId = newAttestWorkFlowTemplateHead.AttestWorkFlowTemplateHeadId;
                                    newAttestWorkFlowTemplateRow.AttestTransition = attestTransitionMapping[row.AttestTransitionId];
                                }
                                else
                                {
                                    newAttestWorkFlowTemplateRow = new AttestWorkFlowTemplateRow()
                                    {
                                        AttestWorkFlowTemplateHeadId = newAttestWorkFlowTemplateHead.AttestWorkFlowTemplateHeadId
                                    };

                                    if (attestTransitionMapping.TryGetValue(row.AttestTransitionId, out AttestTransition transition))
                                    {
                                        newAttestWorkFlowTemplateRow.AttestTransition = transition;
                                        SetCreatedProperties(newAttestWorkFlowTemplateRow);
                                        entities.AttestWorkFlowTemplateRow.AddObject(newAttestWorkFlowTemplateRow);
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }

                result = SaveChangesWithTransaction(entities);
                if (result.Success)
                {
                    #region ReminderAttestStateId

                    foreach (var pair in attestRoleMapping)
                    {
                        AttestRole templateAttestRole = templateAttestRoles.FirstOrDefault(i => i.AttestRoleId == pair.Key);
                        AttestRole newAttestRole = pair.Value;
                        if (templateAttestRole == null || newAttestRole == null)
                            continue;
                        if (!templateAttestRole.ReminderAttestStateId.HasValue || attestStateMapping.ContainsKey(templateAttestRole.ReminderAttestStateId.Value))
                            continue;

                        newAttestRole.ReminderAttestStateId = attestStateMapping[templateAttestRole.ReminderAttestStateId.Value]?.AttestStateId;
                    }

                    #endregion

                    #region CompanyCategoryRecords

                    foreach (KeyValuePair<int, AttestRole> pair in attestRoleMapping)
                    {
                        #region AttestRole

                        AttestRole templateAttestRole = templateAttestRoles.FirstOrDefault(i => i.AttestRoleId == pair.Key);
                        AttestRole attestRole = pair.Value;
                        if (templateAttestRole == null || attestRole == null)
                            continue;

                        #region CompanyCategoryRecord

                        List<CompanyCategoryRecord> existingCompanyCategoryRecords = new List<CompanyCategoryRecord>();
                        if (update)
                        {
                            existingCompanyCategoryRecords.AddRange(CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, attestRole.AttestRoleId, newCompanyId));
                            existingCompanyCategoryRecords.AddRange(CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRoleSecondary, attestRole.AttestRoleId, newCompanyId));
                        }

                        List<CompanyCategoryRecord> templateRecords = new List<CompanyCategoryRecord>();
                        templateRecords.AddRange(CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRole, templateAttestRole.AttestRoleId, templateCompanyId, true));
                        templateRecords.AddRange(CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.AttestRoleSecondary, templateAttestRole.AttestRoleId, templateCompanyId, true));

                        foreach (CompanyCategoryRecord templateCategoryRecord in templateRecords)
                        {
                            if (templateCategoryRecord.Category == null)
                                continue;

                            CompanyCategoryRecord existingRecord = existingCompanyCategoryRecords.FirstOrDefault(c => c.Category.Name == templateCategoryRecord.Category.Name && c.Category.Type == templateCategoryRecord.Category.Type);
                            if (existingRecord == null)
                            {
                                CompanyCategoryRecord record = new CompanyCategoryRecord()
                                {
                                    RecordId = attestRole.AttestRoleId,
                                    Entity = templateCategoryRecord.Entity,
                                    Default = templateCategoryRecord.Default,

                                    //Set references
                                    Company = newCompany,
                                    Category = categoryMapping[templateCategoryRecord.CategoryId],
                                };
                                SetCreatedProperties(record);
                            }
                        }

                        #endregion

                        #endregion
                    }

                    result = SaveChangesWithTransaction(entities);
                    if (!result.Success)
                        result = LogCopyError("CompanyCategoryRecord", templateCompanyId, newCompanyId, saved: true);

                    #endregion
                }
                else
                    result = LogCopyError("Attest", templateCompanyId, newCompanyId, saved: true);
            }

            return result;
        }

        #endregion

        #region Reports

        public ActionResult CopyReportsFromTemplateCompany(int newCompanyId, int templateCompanyId, bool copyReportPackages, bool copyReportGroupsAndReportHeaders, bool copyReportSettings, bool copyReportExportSelections, bool update, ref Dictionary<int, Report> reportMapping, ref Dictionary<int, Role> roleMapping)
        {
            ActionResult result = new ActionResult(true);

            #region ReportTemplates and Reports

            Dictionary<int, ReportTemplate> reportTemplateMapping = new Dictionary<int, ReportTemplate>();
            bool original = !copyReportExportSelections;

            List<ReportTemplate> templateReportTemplates = ReportManager.GetReportTemplates(templateCompanyId);
            List<Report> templateReports = ReportManager.GetReports(templateCompanyId, null, onlyOriginal: original, loadReportSelection: true, loadRolePermission: true);

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, newCompanyId);

                #region ReportTemplate

                List<ReportTemplate> existingReportTemplates = update ? ReportManager.GetReportTemplates(entities, newCompanyId) : new List<ReportTemplate>();

                foreach (ReportTemplate templateReportTemplate in templateReportTemplates)
                {
                    #region ReportTemplate

                    try
                    {
                        ReportTemplate reportTemplate = existingReportTemplates.FirstOrDefault(i => i.Name == templateReportTemplate.Name && i.SysReportTypeId == templateReportTemplate.SysReportTypeId);
                        if (reportTemplate == null)
                        {
                            reportTemplate = new ReportTemplate()
                            {
                                Name = templateReportTemplate.Name,
                                Description = templateReportTemplate.Description,
                                FileName = templateReportTemplate.FileName,
                                Template = templateReportTemplate.Template,
                                SysReportTypeId = templateReportTemplate.SysReportTypeId,
                                SysTemplateTypeId = templateReportTemplate.SysTemplateTypeId,
                                Module = templateReportTemplate.Module,
                                GroupByLevel1 = templateReportTemplate.GroupByLevel1,
                                GroupByLevel2 = templateReportTemplate.GroupByLevel2,
                                GroupByLevel3 = templateReportTemplate.GroupByLevel3,
                                GroupByLevel4 = templateReportTemplate.GroupByLevel4,
                                SortByLevel1 = templateReportTemplate.SortByLevel1,
                                SortByLevel2 = templateReportTemplate.SortByLevel2,
                                SortByLevel3 = templateReportTemplate.SortByLevel3,
                                SortByLevel4 = templateReportTemplate.SortByLevel4,
                                IsSortAscending = templateReportTemplate.IsSortAscending,
                                Special = templateReportTemplate.Special,
                                ReportNr = templateReportTemplate.ReportNr,
                                ShowOnlyTotals = templateReportTemplate.ShowOnlyTotals,
                                ShowGroupingAndSorting = templateReportTemplate.ShowGroupingAndSorting,
                                ValidExportTypes = templateReportTemplate.ValidExportTypes,

                                //References
                                Company = newCompany,
                            };
                            entities.ReportTemplate.AddObject(reportTemplate);
                        }
                        reportTemplateMapping.Add(templateReportTemplate.ReportTemplateId, reportTemplate);
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("ReportTemplate", "ReportTemplateId", templateReportTemplate.ReportTemplateId, templateReportTemplate.FileName, templateReportTemplate.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    LogCopyError("ReportTemplate", "", 0, "", "", templateCompanyId, newCompanyId, add: true);

                #endregion

                #region Report

                List<Report> existingReports = ReportManager.GetReports(entities, newCompanyId, null, onlyOriginal: original, loadReportSelection: true, loadRolePermission: true);

                foreach (Report templateReport in templateReports)
                {
                    #region Report

                    try
                    {
                        Report report = existingReports.FirstOrDefault(i => i.Name == templateReport.Name && i.ReportNr == templateReport.ReportNr);
                        if (report == null)
                        {
                            report = new Report()
                            {
                                ReportNr = templateReport.ReportNr,
                                Name = templateReport.Name,
                                Description = templateReport.Description,
                                Standard = templateReport.Standard,
                                Original = templateReport.Original,
                                Module = templateReport.Module,
                                IncludeAllHistoricalData = templateReport.IncludeAllHistoricalData,
                                GetDetailedInformation = templateReport.GetDetailedInformation,
                                NoOfYearsBackinPreviousYear = templateReport.NoOfYearsBackinPreviousYear,
                                IncludeBudget = templateReport.IncludeBudget,
                                ShowInAccountingReports = templateReport.ShowInAccountingReports,
                                ExportType = templateReport.ExportType,
                                FileType = templateReport.FileType,
                                GroupByLevel1 = templateReport.GroupByLevel1,
                                GroupByLevel2 = templateReport.GroupByLevel2,
                                GroupByLevel3 = templateReport.GroupByLevel3,
                                GroupByLevel4 = templateReport.GroupByLevel4,
                                SortByLevel1 = templateReport.SortByLevel1,
                                SortByLevel2 = templateReport.SortByLevel2,
                                SortByLevel3 = templateReport.SortByLevel3,
                                SortByLevel4 = templateReport.SortByLevel4,
                                IsSortAscending = templateReport.IsSortAscending,
                                Special = templateReport.Special,

                                //Set FK
                                ActorCompanyId = newCompany.ActorCompanyId,
                            };
                            SetCreatedProperties(report);
                            entities.Report.AddObject(report);

                            #region Export selections

                            if (!report.Original && copyReportExportSelections)
                            {
                                report.NameWithReportSelectionText = templateReport.NameWithReportSelectionText;
                                report.SysReportTemplateTypeId = templateReport.SysReportTemplateTypeId;
                            }

                            #endregion

                            #region ReportTemplate

                            bool foundReportTemplate = false;
                            if (templateReport.Standard)
                            {
                                //ReportTemplate is in SOESys database
                                report.ReportTemplateId = templateReport.ReportTemplateId;
                                foundReportTemplate = true;
                            }
                            else
                            {
                                //ReportTemplate is in SOEComp database
                                if (reportTemplateMapping.ContainsKey(templateReport.ReportTemplateId))
                                {
                                    //Get the mapped ReportTemplateId
                                    report.ReportTemplateId = reportTemplateMapping[templateReport.ReportTemplateId].ReportTemplateId;
                                    foundReportTemplate = true;
                                }
                                else
                                {
                                    //ReportTemplate is deleted
                                    foundReportTemplate = false;
                                }
                            }

                            #endregion

                            if (foundReportTemplate)
                            {
                                //if (!reportMapping.ContainsKey(templateReport.ReportId))
                                //    reportMapping.Add(templateReport.ReportId, report);

                                #region ReportRolePermission

                                if (!templateReport.ReportRolePermission.IsNullOrEmpty())
                                {
                                    foreach (ReportRolePermission templateReportRolePermission in templateReport.ReportRolePermission)
                                    {
                                        if (!roleMapping.ContainsKey(templateReportRolePermission.RoleId))
                                            continue;

                                        ReportRolePermission reportRolePermission = new ReportRolePermission()
                                        {
                                            //Set references
                                            Report = report,

                                            //Set FK
                                            RoleId = roleMapping[templateReportRolePermission.RoleId].RoleId,
                                            ActorCompanyId = newCompanyId,
                                        };
                                        SetCreatedProperties(reportRolePermission);
                                        report.ReportRolePermission.Add(reportRolePermission);
                                    }
                                }

                                #endregion

                                #region ReportSelection

                                if (templateReport.ReportSelection != null)
                                {
                                    report.ReportSelectionText = templateReport.ReportSelection.ReportSelectionText;

                                    #region ReportSelection

                                    report.ReportSelection = new ReportSelection()
                                    {
                                        ReportSelectionText = templateReport.ReportSelection.ReportSelectionText,
                                    };

                                    #endregion

                                    #region ReportSelectionInt

                                    if (templateReport.ReportSelection.ReportSelectionInt != null)
                                    {
                                        foreach (ReportSelectionInt templateReportSelectionInt in templateReport.ReportSelection.ReportSelectionInt)
                                        {
                                            ReportSelectionInt reportSelectionInt = new ReportSelectionInt()
                                            {
                                                ReportSelectionType = templateReportSelectionInt.ReportSelectionType,
                                                SelectFrom = templateReportSelectionInt.SelectFrom,
                                                SelectTo = templateReportSelectionInt.SelectTo,
                                                SelectGroup = templateReportSelectionInt.SelectGroup,
                                                Order = templateReportSelectionInt.Order,
                                            };
                                            report.ReportSelection.ReportSelectionInt.Add(reportSelectionInt);
                                        }
                                    }

                                    #endregion

                                    #region ReportSelectionStr

                                    if (templateReport.ReportSelection.ReportSelectionStr != null)
                                    {
                                        foreach (ReportSelectionStr templateReportSelectionStr in templateReport.ReportSelection.ReportSelectionStr)
                                        {
                                            ReportSelectionStr reportSelectionStr = new ReportSelectionStr()
                                            {
                                                ReportSelectionType = templateReportSelectionStr.ReportSelectionType,
                                                SelectFrom = templateReportSelectionStr.SelectFrom,
                                                SelectTo = templateReportSelectionStr.SelectTo,
                                                SelectGroup = templateReportSelectionStr.SelectGroup,
                                                Order = templateReportSelectionStr.Order
                                            };
                                            report.ReportSelection.ReportSelectionStr.Add(reportSelectionStr);
                                        }
                                    }

                                    #endregion

                                    #region ReportSelectionDate

                                    if (templateReport.ReportSelection.ReportSelectionDate != null)
                                    {
                                        foreach (ReportSelectionDate templateReportSelectionDate in templateReport.ReportSelection.ReportSelectionDate)
                                        {
                                            ReportSelectionDate reportSelectionDate = new ReportSelectionDate()
                                            {
                                                ReportSelectionType = templateReportSelectionDate.ReportSelectionType,
                                                SelectFrom = templateReportSelectionDate.SelectFrom,
                                                SelectTo = templateReportSelectionDate.SelectTo,
                                                SelectGroup = templateReportSelectionDate.SelectGroup,
                                                Order = templateReportSelectionDate.Order
                                            };
                                            report.ReportSelection.ReportSelectionDate.Add(reportSelectionDate);
                                        }
                                    }

                                    #endregion
                                }

                                #endregion
                            }
                            else
                                result = LogCopyError("Report", "ReportId", templateReport.ReportId, templateReport.ReportNr.ToString(), templateReport.Name, templateCompanyId, newCompanyId, add: true);
                        }

                        if (!reportMapping.ContainsKey(templateReport.ReportId))
                            reportMapping.Add(templateReport.ReportId, report);
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("Report", "ReportId", templateReport.ReportId, templateReport.ReportNr.ToString(), templateReport.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                result = SaveChanges(entities);
                if (!result.Success)
                    result = LogCopyError("Report", "", 0, "", "", templateCompanyId, newCompanyId, add: true);

                #endregion
            }

            #endregion

            #region Settings

            if (copyReportSettings)
            {
                CopyCompanyReportSettings(reportMapping, newCompanyId, templateCompanyId);
            }

            #endregion

            #region ReportPackages

            if (copyReportPackages)
            {
                List<ReportPackage> existingReportPackages = update ? ReportManager.GetReportPackages(newCompanyId, true) : new List<ReportPackage>();
                List<ReportPackage> templateReportPackages = ReportManager.GetReportPackages(templateCompanyId, true);

                foreach (ReportPackage templateReportPackage in templateReportPackages)
                {
                    #region ReportPackage

                    try
                    {
                        ReportPackage reportPackage = existingReportPackages.FirstOrDefault(i => i.Name == templateReportPackage.Name);
                        if (reportPackage == null)
                        {
                            reportPackage = new ReportPackage()
                            {
                                Name = templateReportPackage.Name,
                                Description = templateReportPackage.Description,
                            };

                            if (ReportManager.AddReportPackage(reportPackage, newCompanyId).Success)
                            {
                                //Find ReportPackageMappings for ReportPackage
                                if (templateReportPackage.Report != null && templateReportPackage.Report.Count > 0)
                                {
                                    int[] reportArr = new int[templateReportPackage.Report.Count];

                                    int i = 0;
                                    foreach (Report report in templateReportPackage.Report.Where(r => r.Original))
                                    {
                                        if (reportMapping.ContainsKey(report.ReportId))
                                        {
                                            reportArr[i] = reportMapping[report.ReportId].ReportId;
                                            i++;
                                        }
                                    }

                                    ReportManager.UpdateReportPackage(reportPackage, reportArr, newCompanyId);
                                }
                            }
                            else
                                result = LogCopyError("ReportPackage", "ReportPackageId", templateReportPackage.ReportPackageId, "", templateReportPackage.Name, templateCompanyId, newCompanyId, add: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("ReportPackage", "ReportPackageId", templateReportPackage.ReportPackageId, "", templateReportPackage.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }
            }

            #endregion

            #region ReportGroups and ReportHeaders

            if (copyReportGroupsAndReportHeaders)
            {
                Dictionary<int, int> reportGroupIdMapping = new Dictionary<int, int>();
                Dictionary<int, int> reportHeaderIdMapping = new Dictionary<int, int>();

                #region Copy ReportHeaders

                List<ReportHeader> templateReportHeaders = ReportManager.GetReportHeadersByCompany(templateCompanyId, true);
                List<ReportHeader> existingReportHeaders = update ? ReportManager.GetReportHeadersByCompany(newCompanyId, false) : new List<ReportHeader>();

                foreach (ReportHeader templateReportHeader in templateReportHeaders)
                {
                    #region ReportHeader

                    try
                    {
                        ReportHeader reportHeader = existingReportHeaders.FirstOrDefault(i => i.Name == templateReportHeader.Name && i.Description == templateReportHeader.Description);
                        if (reportHeader == null)
                        {
                            reportHeader = new ReportHeader()
                            {
                                Name = templateReportHeader.Name,
                                Description = templateReportHeader.Description,
                                ShowRow = templateReportHeader.ShowRow,
                                ShowZeroRow = templateReportHeader.ShowZeroRow,
                                ShowSum = templateReportHeader.ShowSum,
                                ShowLabel = templateReportHeader.ShowLabel,
                                TemplateTypeId = templateReportHeader.TemplateTypeId,
                                Module = templateReportHeader.Module,
                                DoNotSummarizeOnGroup = templateReportHeader.DoNotSummarizeOnGroup,
                            };

                            Collection<FormIntervalEntryItem> formIntervalEntryItems = new Collection<FormIntervalEntryItem>();
                            if (templateReportHeader.ReportHeaderInterval != null)
                            {
                                foreach (ReportHeaderInterval reportHeaderInterval in templateReportHeader.ReportHeaderInterval)
                                {
                                    formIntervalEntryItems.Add(new FormIntervalEntryItem()
                                    {
                                        From = reportHeaderInterval.IntervalFrom,
                                        To = reportHeaderInterval.IntervalTo,
                                        LabelType = reportHeaderInterval.SelectValue ?? 0,
                                    });
                                }
                            }

                            if (ReportManager.AddReportHeader(reportHeader, newCompanyId, formIntervalEntryItems).Success)
                                reportHeaderIdMapping.Add(templateReportHeader.ReportHeaderId, reportHeader.ReportHeaderId);
                            else
                                result = LogCopyError("ReportHeader", "ReportHeaderId", templateReportHeader.ReportHeaderId, "", templateReportHeader.Name, templateCompanyId, newCompanyId, add: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("ReportHeader", "ReportHeaderId", templateReportHeader.ReportHeaderId, "", templateReportHeader.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                #endregion

                #region Copy ReportGroups

                List<ReportGroup> templateReportGroups = ReportManager.GetReportGroupsByCompany(templateCompanyId, true, true);
                List<ReportGroup> existingReportGroups = update ? ReportManager.GetReportGroupsByCompany(newCompanyId) : new List<ReportGroup>();

                foreach (ReportGroup templateReportGroup in templateReportGroups)
                {
                    #region ReportGroup

                    try
                    {
                        ReportGroup reportGroup = existingReportGroups.FirstOrDefault(i => i.Name == templateReportGroup.Name && i.Description == templateReportGroup.Description);
                        if (reportGroup == null)
                        {
                            reportGroup = new ReportGroup()
                            {
                                Name = templateReportGroup.Name,
                                Description = templateReportGroup.Description,
                                ShowSum = templateReportGroup.ShowSum,
                                ShowLabel = templateReportGroup.ShowLabel,
                                TemplateTypeId = templateReportGroup.TemplateTypeId,
                                Module = templateReportGroup.Module,
                                InvertRow = templateReportGroup.InvertRow,

                            };

                            if (ReportManager.AddReportGroup(reportGroup, newCompanyId).Success)
                            {
                                #region ReportGroupMapping

                                reportGroupIdMapping.Add(templateReportGroup.ReportGroupId, reportGroup.ReportGroupId);

                                //Find ReportGroupMappings for ReportGroup
                                if (templateReportGroup.ReportGroupMapping != null && templateReportGroup.ReportGroupMapping.Count > 0)
                                {
                                    foreach (ReportGroupMapping reportGroupMapping in templateReportGroup.ReportGroupMapping)
                                    {
                                        ReportGroupMapping newReportGroupMapping = new ReportGroupMapping()
                                        {
                                            Order = reportGroupMapping.Order,
                                        };

                                        if (reportMapping.ContainsKey(reportGroupMapping.ReportId))
                                        {
                                            int reportId = reportMapping[reportGroupMapping.ReportId].ReportId;
                                            if (!ReportManager.AddReportGroupMapping(newReportGroupMapping, reportGroup.ReportGroupId, reportId, newCompanyId).Success)
                                                result = LogCopyError("ReportGroupMapping", "ReportId", reportId, "", templateReportGroup.Name, templateCompanyId, newCompanyId, add: true);
                                        }
                                    }
                                }

                                #endregion

                                #region ReportGroupHeaderMapping

                                //Find ReportGroupHeaderMappings for ReportGroup
                                if (templateReportGroup.ReportGroupHeaderMapping != null && templateReportGroup.ReportGroupHeaderMapping.Count > 0)
                                {
                                    foreach (ReportGroupHeaderMapping reportGroupHeaderMapping in templateReportGroup.ReportGroupHeaderMapping)
                                    {
                                        ReportGroupHeaderMapping newReportGroupHeaderMapping = new ReportGroupHeaderMapping()
                                        {
                                            Order = reportGroupHeaderMapping.Order,
                                        };

                                        if (reportHeaderIdMapping.ContainsKey(reportGroupHeaderMapping.ReportHeaderId))
                                        {
                                            int reportHeaderId = reportHeaderIdMapping[reportGroupHeaderMapping.ReportHeaderId];
                                            if (!ReportManager.AddReportGroupHeaderMapping(newReportGroupHeaderMapping, reportGroup.ReportGroupId, reportHeaderId, newCompanyId).Success)
                                                result = LogCopyError("ReportGroupHeaderMapping", "ReportHeaderId", reportHeaderId, "", templateReportGroup.Name, templateCompanyId, newCompanyId, add: true);
                                        }
                                    }
                                }

                                #endregion
                            }
                            else
                                result = LogCopyError("ReportGroup", "ReportGroupId", templateReportGroup.ReportGroupId, "", templateReportGroup.Name, templateCompanyId, newCompanyId, add: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        result = LogCopyError("ReportGroup", "ReportGroupId", templateReportGroup.ReportGroupId, "", templateReportGroup.Name, templateCompanyId, newCompanyId, ex);
                    }

                    #endregion
                }

                #endregion
            }

            #endregion

            return result;
        }

        #endregion

        #region Settings

        public ActionResult CopyManageSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Group Manage settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.CompanyAPIKey, //Do not copy
                CompanySettingType.DefaultRole, //Copy sepearate later
            };

            CopyCompanySettings(CompanySettingTypeGroup.Manage, newCompanyId, templateCompanyId, excludeSettingTypes);
            CopyCompanyDefaultRoleSetting(newCompanyId, templateCompanyId);

            #endregion

            return result;
        }

        public ActionResult CopyAccountingSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool copyVoucherSettings, bool copyAccountSettings)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Group Accounting settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {
                CompanySettingType.AccountingVoucherSeriesTypeManual, //Copied below
                CompanySettingType.AccountingVoucherSeriesTypeVat,
                CompanySettingType.StockDefaultVoucherSeriesType,
                CompanySettingType.PayrollAccountExportVoucherSeriesType,
                CompanySettingType.AccountdistributionVoucherSeriesType,

                //Report
                CompanySettingType.AccountingDefaultAccountingOrder,
                CompanySettingType.AccountingDefaultVoucherList,
                CompanySettingType.AccountingDefaultAnalysisReport,

            };

            CopyCompanySettings(CompanySettingTypeGroup.Accounting, newCompanyId, templateCompanyId, excludeSettingTypes);

            //VoucherSeries
            if (copyVoucherSettings)
            {
                CopyCompanyVoucherSeriesSetting(CompanySettingType.AccountingVoucherSeriesTypeManual, newCompanyId, templateCompanyId);
                CopyCompanyVoucherSeriesSetting(CompanySettingType.AccountingVoucherSeriesTypeVat, newCompanyId, templateCompanyId);
                CopyCompanyVoucherSeriesSetting(CompanySettingType.StockDefaultVoucherSeriesType, newCompanyId, templateCompanyId);
                CopyCompanyVoucherSeriesSetting(CompanySettingType.PayrollAccountExportVoucherSeriesType, newCompanyId, templateCompanyId);
                CopyCompanyVoucherSeriesSetting(CompanySettingType.AccountdistributionVoucherSeriesType, newCompanyId, templateCompanyId);
            }

            #endregion

            #region Group BaseAccountsCommon settings

            if (copyAccountSettings)
            {
                CopyCompanyAccountSettings(CompanySettingTypeGroup.BaseAccountsCommon, newCompanyId, templateCompanyId);
            }

            #endregion

            return result;
        }

        public ActionResult CopySupplierSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool copyAccountSettings, bool copyVoucherSettings)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Group Supplier settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {
                //VoucherSeries
                CompanySettingType.SupplierInvoiceVoucherSeriesType, //Copied below
                CompanySettingType.SupplierPaymentVoucherSeriesType, //Copied below

                //Payment
                CompanySettingType.SupplierPaymentDefaultPaymentCondition, //PaymentConditions copied separate

                //Invoice
                CompanySettingType.SupplierDefaultBalanceList, //Reports copied separate                
                CompanySettingType.SupplierDefaultPaymentSuggestionList,
                CompanySettingType.SupplierDefaultChecklistPayments,

                // AccountPayable Invoice Attestation
                CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestTemplate,
                CompanySettingType.SupplierInvoiceAttestFlowStatusToStartAccountsPayableFlow,
            };

            CopyCompanySettings(CompanySettingTypeGroup.Supplier, newCompanyId, templateCompanyId, excludeSettingTypes);

            //VoucherSeries
            if (copyVoucherSettings)
            {
                CopyCompanyVoucherSeriesSetting(CompanySettingType.SupplierInvoiceVoucherSeriesType, newCompanyId, templateCompanyId);
                CopyCompanyVoucherSeriesSetting(CompanySettingType.SupplierPaymentVoucherSeriesType, newCompanyId, templateCompanyId);
            }

            #endregion

            #region Group BaseAccountsSupplier settings

            if (copyAccountSettings)
            {
                CopyCompanyAccountSettings(CompanySettingTypeGroup.BaseAccountsSupplier, newCompanyId, templateCompanyId);
            }

            #endregion

            #region AccountPayable Invoice Attestation settings

            CopySupplierInvoiceAttestation(newCompanyId, templateCompanyId);

            #endregion
            return result;
        }

        public ActionResult CopyCustomerSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool copyAccountSettings, bool copyVoucherSettings)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Group Customer settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {
                //VoucherSeries
                CompanySettingType.CustomerInvoiceVoucherSeriesType, //Copied below
                CompanySettingType.CustomerPaymentVoucherSeriesType, //Copied below

                //Invoice
                CompanySettingType.CustomerInvoiceTemplate, //Reports copied separate
                CompanySettingType.CustomerDefaultBalanceList, //Reports copied separate

                //Payment
                CompanySettingType.CustomerPaymentDefaultPaymentCondition, //PaymentConditions copied separate
                CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction, //PaymentConditions copied separate

                CompanySettingType.CustomerDefaultReminderTemplate, //Copied in reports
                CompanySettingType.CustomerDefaultInterestTemplate, //Copied in reports
                CompanySettingType.CustomerDefaultInterestRateCalculationTemplate,

                CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest, //Copied in reports
        };

            CopyCompanySettings(CompanySettingTypeGroup.Customer, newCompanyId, templateCompanyId, excludeSettingTypes);

            //VoucherSeries
            if (copyVoucherSettings)
            {
                CopyCompanyVoucherSeriesSetting(CompanySettingType.CustomerInvoiceVoucherSeriesType, newCompanyId, templateCompanyId);
                CopyCompanyVoucherSeriesSetting(CompanySettingType.CustomerPaymentVoucherSeriesType, newCompanyId, templateCompanyId);
            }

            #endregion

            #region Group BaseAccountsCustomer settings

            if (copyAccountSettings)
            {
                CopyCompanyAccountSettings(CompanySettingTypeGroup.BaseAccountsCustomer, newCompanyId, templateCompanyId);
            }

            #endregion

            return result;
        }

        public ActionResult CopyBillingSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, bool update, bool copyAccountSettings)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            bool copyError = false;

            #region Group Billing settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {
                //Invoice
                CompanySettingType.BillingDefaultPriceListType, //Copied below
                CompanySettingType.BillingDefaultDeliveryType, //Copied below
                CompanySettingType.BillingDefaultDeliveryCondition, //Copied below
                CompanySettingType.BillingDefaultInvoiceProductUnit, //Copied below
                CompanySettingType.BillingDefaultWholeseller, //Copied below
  
                //Reports
                CompanySettingType.BillingDefaultInvoiceTemplate, //Reports copied separate
                CompanySettingType.CustomerDefaultReminderTemplate, //Reports copied separate
                CompanySettingType.BillingDefaultTimeProjectReportTemplate, //Reports copied separate
                CompanySettingType.BillingOfferDefaultEmailTemplate, //Reports copied separate
                CompanySettingType.BillingOrderDefaultEmailTemplate, //Reports copied separate
                CompanySettingType.BillingContractDefaultEmailTemplate, //Reports copied separate
                CompanySettingType.BillingDefaultOrderTemplate, //Reports copied separate
                CompanySettingType.BillingDefaultWorkingOrderTemplate, //Reports copied separate
                CompanySettingType.BillingDefaultOfferTemplate, //Reports copied separate
                CompanySettingType.BillingDefaultContractTemplate, //Reports copied separate
                CompanySettingType.BillingDefaultExpenseReportTemplate, //Reports copied separate
                CompanySettingType.BillingDefaultHouseholdDeductionTemplate, //Reports copied separate

                //Offer and Order status
                CompanySettingType.BillingStatusTransferredOfferToOrder, //Copied below
                CompanySettingType.BillingStatusTransferredOfferToInvoice, //Copied below
                CompanySettingType.BillingStatusTransferredOrderToInvoice, //Copied below
                CompanySettingType.BillingStatusOrderReadyMobile, //Copied below

                //Other
                CompanySettingType.BillingDefaultVatCode, //Copied below

                
                //Email Templates
                CompanySettingType.BillingDefaultEmailTemplate,
            };

            CopyCompanySettings(CompanySettingTypeGroup.Billing, newCompanyId, templateCompanyId, excludeSettingTypes);

            Dictionary<int, int> intValues = new Dictionary<int, int>();

            #region DeliveryTypes

            int defaultTemplateDeliveryType = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultDeliveryType, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<DeliveryType> existingDeliveryTypes = InvoiceManager.GetDeliveryTypes(newCompanyId);
            List<DeliveryType> templateDeliveryTypes = InvoiceManager.GetDeliveryTypes(templateCompanyId);

            foreach (DeliveryType templateDeliveryType in templateDeliveryTypes)
            {
                #region DeliveryType

                try
                {
                    DeliveryType deliveryType = existingDeliveryTypes.FirstOrDefault(i => i.Code == templateDeliveryType.Code && i.Name == templateDeliveryType.Name);
                    if (deliveryType == null)
                    {
                        deliveryType = new DeliveryType()
                        {
                            Code = templateDeliveryType.Code,
                            Name = templateDeliveryType.Name,
                        };

                        result = InvoiceManager.AddDeliveryType(deliveryType, newCompanyId);
                        if (result.Success)
                        {
                            if (templateDeliveryType.DeliveryTypeId == defaultTemplateDeliveryType)
                                intValues.Add((int)CompanySettingType.BillingDefaultDeliveryType, deliveryType.DeliveryTypeId);
                        }
                        else
                        {
                            result = LogCopyError("DeliveryType", "DeliveryTypeId", templateDeliveryType.DeliveryTypeId, templateDeliveryType.Code, templateDeliveryType.Name, templateCompanyId, newCompanyId, add: true);
                            copyError = true;
                        }
                    }
                    else
                    {
                        if (templateDeliveryType.DeliveryTypeId == defaultTemplateDeliveryType)
                            intValues.Add((int)CompanySettingType.BillingDefaultDeliveryType, deliveryType.DeliveryTypeId);
                    }
                }
                catch (Exception ex)
                {
                    result = LogCopyError("DeliveryType", "DeliveryTypeId", templateDeliveryType.DeliveryTypeId, templateDeliveryType.Code, templateDeliveryType.Name, templateCompanyId, newCompanyId, ex);
                    copyError = true;
                }

                #endregion
            }

            #endregion

            #region DeliveryConditions

            int defaultTemplateDeliveryCondition = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultDeliveryCondition, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<DeliveryCondition> existingDeliveryConditions = InvoiceManager.GetDeliveryConditions(newCompanyId);
            List<DeliveryCondition> templateDeliveryConditions = InvoiceManager.GetDeliveryConditions(templateCompanyId);

            foreach (DeliveryCondition templateDeliveryCondition in templateDeliveryConditions)
            {
                #region DeliveryCondition

                try
                {
                    DeliveryCondition deliveryCondition = existingDeliveryConditions.FirstOrDefault(i => i.Code == templateDeliveryCondition.Code && i.Name == templateDeliveryCondition.Name);
                    if (deliveryCondition == null)
                    {
                        deliveryCondition = new DeliveryCondition()
                        {
                            Code = templateDeliveryCondition.Code,
                            Name = templateDeliveryCondition.Name,
                        };

                        result = InvoiceManager.AddDeliveryCondition(deliveryCondition, newCompanyId);
                        if (result.Success)
                        {
                            if (templateDeliveryCondition.DeliveryConditionId == defaultTemplateDeliveryCondition)
                                intValues.Add((int)CompanySettingType.BillingDefaultDeliveryCondition, deliveryCondition.DeliveryConditionId);
                        }
                        else
                        {
                            result = LogCopyError("DeliveryCondition", "DeliveryConditionId", templateDeliveryCondition.DeliveryConditionId, templateDeliveryCondition.Code, templateDeliveryCondition.Name, templateCompanyId, newCompanyId, add: true);
                            copyError = true;
                        }
                    }
                    else
                    {
                        if (templateDeliveryCondition.DeliveryConditionId == defaultTemplateDeliveryCondition)
                            intValues.Add((int)CompanySettingType.BillingDefaultDeliveryCondition, deliveryCondition.DeliveryConditionId);
                    }
                }
                catch (Exception ex)
                {
                    result = LogCopyError("DeliveryCondition", "DeliveryConditionId", templateDeliveryCondition.DeliveryConditionId, templateDeliveryCondition.Code, templateDeliveryCondition.Name, templateCompanyId, newCompanyId, ex);
                    copyError = true;
                }

                #endregion
            }

            #endregion

            #region ProductUnits

            // Get default product unit
            int defaultTemplateProductUnit = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultInvoiceProductUnit, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<ProductUnit> existingProductUnits = ProductManager.GetProductUnits(newCompanyId).ToList();
            List<ProductUnit> templateProductUnits = ProductManager.GetProductUnits(templateCompanyId).ToList();

            foreach (ProductUnit templateProductUnit in templateProductUnits)
            {
                #region ProductUnit

                try
                {
                    ProductUnit productUnit = existingProductUnits.FirstOrDefault(i => i.Code == templateProductUnit.Code && i.Name == templateProductUnit.Name);
                    if (productUnit == null)
                    {
                        productUnit = new ProductUnit()
                        {
                            Code = templateProductUnit.Code,
                            Name = templateProductUnit.Name,
                        };

                        result = ProductManager.AddProductUnit(productUnit, newCompanyId);
                        if (result.Success)
                        {
                            if (templateProductUnit.ProductUnitId == defaultTemplateProductUnit)
                                intValues.Add((int)CompanySettingType.BillingDefaultInvoiceProductUnit, productUnit.ProductUnitId);
                        }
                        else
                        {
                            result = LogCopyError("ProductUnit", "ProductUnitId", templateProductUnit.ProductUnitId, templateProductUnit.Code, templateProductUnit.Name, templateCompanyId, newCompanyId, add: true);
                            copyError = true;
                        }
                    }
                    else
                    {
                        if (templateProductUnit.ProductUnitId == defaultTemplateProductUnit)
                            intValues.Add((int)CompanySettingType.BillingDefaultInvoiceProductUnit, productUnit.ProductUnitId);
                    }
                }
                catch (Exception ex)
                {
                    result = LogCopyError("ProductUnit", "ProductUnitId", templateProductUnit.ProductUnitId, templateProductUnit.Code, templateProductUnit.Name, templateCompanyId, newCompanyId, ex);
                    copyError = true;
                }

                #endregion
            }

            #endregion

            #region PriceList

            int defaultPriceListType = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultPriceListType, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<PriceListType> existingPriceListTypes = ProductPricelistManager.GetPriceListTypes(newCompanyId);
            List<PriceListType> templatePriceListTypes = ProductPricelistManager.GetPriceListTypes(templateCompanyId);

            foreach (PriceListType templatePriceListType in templatePriceListTypes)
            {
                #region PriceListType

                //Copied separate, only set setting
                if (templatePriceListType.PriceListTypeId != defaultPriceListType)
                    continue;

                PriceListType priceListType = existingPriceListTypes.FirstOrDefault(i => i.Name == templatePriceListType.Name);
                if (priceListType != null)
                    intValues.Add((int)CompanySettingType.BillingDefaultPriceListType, priceListType.PriceListTypeId);

                #endregion
            }

            #endregion

            #region Wholeseller

            int defaultWholeseller = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultWholeseller, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<SysWholeseller> existingSysWholesellers = WholeSellerManager.GetSysWholesellersByCompany(newCompanyId).ToList();
            List<SysWholeseller> templateSysWholesellers = WholeSellerManager.GetSysWholesellersByCompany(templateCompanyId).ToList();

            foreach (SysWholeseller templateSysWholeseller in templateSysWholesellers)
            {
                #region SysWholeseller

                //Copied separate, only set setting
                if (templateSysWholeseller.SysWholesellerId != defaultWholeseller)
                    continue;

                SysWholeseller sysWholeseller = existingSysWholesellers.FirstOrDefault(i => i.Name == templateSysWholeseller.Name);
                if (sysWholeseller != null)
                    intValues.Add((int)CompanySettingType.BillingDefaultWholeseller, sysWholeseller.SysWholesellerId);

                #endregion
            }

            #endregion

            #region AttestStates

            #region Offer

            int defaultStatusTransferredOfferToOrder = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToOrder, 0, templateCompanyId, 0);
            int defaultStatusTransferredOfferToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOfferToInvoice, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<AttestState> existingOfferAttestStates = AttestManager.GetAttestStates(newCompanyId, TermGroup_AttestEntity.Offer, SoeModule.Billing);
            List<AttestState> templateOfferAttestStates = AttestManager.GetAttestStates(templateCompanyId, TermGroup_AttestEntity.Offer, SoeModule.Billing);

            foreach (AttestState templateOfferAttestState in templateOfferAttestStates)
            {
                #region AttestState

                //Copied separate, only set setting
                if (templateOfferAttestState.AttestStateId != defaultStatusTransferredOfferToOrder &&
                    templateOfferAttestState.AttestStateId != defaultStatusTransferredOfferToInvoice)
                    continue;

                AttestState attestState = existingOfferAttestStates.FirstOrDefault(i => i.Name == templateOfferAttestState.Name);
                if (attestState != null)
                {
                    if (templateOfferAttestState.AttestStateId == defaultStatusTransferredOfferToOrder)
                        intValues.Add((int)CompanySettingType.BillingStatusTransferredOfferToOrder, attestState.AttestStateId);
                    if (templateOfferAttestState.AttestStateId == defaultStatusTransferredOfferToInvoice)
                        intValues.Add((int)CompanySettingType.BillingStatusTransferredOfferToInvoice, attestState.AttestStateId);
                }

                #endregion
            }

            #endregion

            #region Order

            int defaultStatusTransferredOrderToInvoice = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusTransferredOrderToInvoice, 0, templateCompanyId, 0);
            int defaultStatusOrderReadyMobile = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStatusOrderReadyMobile, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<AttestState> existingOrderAttestStates = AttestManager.GetAttestStates(newCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);
            List<AttestState> templateOrderAttestStates = AttestManager.GetAttestStates(templateCompanyId, TermGroup_AttestEntity.Order, SoeModule.Billing);

            foreach (AttestState templateOrderAttestState in templateOrderAttestStates)
            {
                #region AttestState

                //Copied separate, only set setting
                if (templateOrderAttestState.AttestStateId != defaultStatusTransferredOrderToInvoice &&
                    templateOrderAttestState.AttestStateId != defaultStatusOrderReadyMobile)
                    continue;

                AttestState attestState = existingOrderAttestStates.FirstOrDefault(i => i.Name == templateOrderAttestState.Name);
                if (attestState != null)
                {
                    if (templateOrderAttestState.AttestStateId == defaultStatusTransferredOrderToInvoice)
                        intValues.Add((int)CompanySettingType.BillingStatusTransferredOrderToInvoice, attestState.AttestStateId);
                    if (templateOrderAttestState.AttestStateId == defaultStatusOrderReadyMobile)
                        intValues.Add((int)CompanySettingType.BillingStatusOrderReadyMobile, attestState.AttestStateId);
                }

                #endregion
            }

            #endregion

            #endregion

            #region VatCode

            int defaultVatCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultVatCode, 0, templateCompanyId, 0);

            var defaultVatCode = AccountManager.GetVatCode(defaultVatCodeId);
            if (defaultVatCode != null)
            {
                int newDefaultVatCodeId = AccountManager.GetVatCodes(newCompanyId).FirstOrDefault(c => c.Code == defaultVatCode.Code && c.Name == defaultVatCode.Name && c.Percent == defaultVatCode.Percent)?.VatCodeId ?? 0;
                if (newDefaultVatCodeId > 0 && !SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultVatCode, newDefaultVatCodeId, 0, newCompanyId, 0).Success)
                    copyError = true;
            }

            #endregion

            #region MaterialCode

            int defaultMaterialCode = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingStandardMaterialCode, 0, templateCompanyId, 0);

            //Always load, could have been added earlier
            List<TimeCode> existingTimeCodes = TimeCodeManager.GetTimeCodes(newCompanyId).ToList();
            List<TimeCode> templateTimeCodes = TimeCodeManager.GetTimeCodes(templateCompanyId).ToList();

            foreach (TimeCode templateTimeCode in templateTimeCodes)
            {

                //Copied separate, only set setting
                if (templateTimeCode.TimeCodeId != defaultMaterialCode)
                    continue;

                TimeCode standardMaterialCode = existingTimeCodes.FirstOrDefault(i => i.Name == templateTimeCode.Name);
                if (standardMaterialCode != null)
                    intValues.Add((int)CompanySettingType.BillingStandardMaterialCode, standardMaterialCode.TimeCodeId);

            }

            #endregion

            if (!SettingManager.UpdateInsertIntSettings(SettingMainType.Company, intValues, 0, newCompanyId, 0).Success)
                copyError = true;

            #endregion

            #region Group BaseAccountsInvoiceProduct settings

            if (copyAccountSettings)
            {
                CopyCompanyAccountSettings(CompanySettingTypeGroup.BaseAccountsInvoiceProduct, newCompanyId, templateCompanyId);
            }

            #endregion

            if (copyError)
                result = new ActionResult(false);
            else
                result = new ActionResult(true);

            return result;
        }

        public ActionResult CopyTimeSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool copyAccountSettings, bool copyAttestSettings, bool copyEmployeeGroups, bool copyPayrollGroups, bool copyVacationGroups, bool copyPayrollProductsAndTimeCodes, bool copyTimePeriods)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Group Time settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {
                //Time
                CompanySettingType.TimeDefaultTimeCode, //Copied below
                CompanySettingType.TimeDefaultEmployeeGroup, //Copied below
                CompanySettingType.TimeDefaultPayrollGroup, //Copied below
                CompanySettingType.TimeDefaultVacationGroup, //Copied below
                CompanySettingType.TimeDefaultTimePeriodHead, //Copied below

                //SalaryExport
                CompanySettingType.SalaryExportPayrollMinimumAttestStatus, //Copied below
                CompanySettingType.SalaryExportPayrollResultingAttestStatus, //Copied below
                CompanySettingType.SalaryExportInvoiceMinimumAttestStatus, //Copied below
                CompanySettingType.SalaryExportInvoiceResultingAttestStatus, //Copied below
                CompanySettingType.SalaryExportExternalExportID, //Not relevant

                //SalaryPayment
                CompanySettingType.SalaryPaymentLockedAttestStateId, //Copied below
                CompanySettingType.SalaryPaymentApproved1AttestStateId, //Copied below
                CompanySettingType.SalaryPaymentApproved2AttestStateId, //Copied below
                CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, //Copied below

                //Attest 
                CompanySettingType.TimeAutoAttestSourceAttestStateId, //Copied below
                CompanySettingType.TimeAutoAttestTargetAttestStateId, //Copied below
                CompanySettingType.MobileTimeAttestResultingAttestStatus, // Copied below
                //Payroll
                CompanySettingType.PayrollAccountingDistributionPayrollProduct, //Copied below

                //Staffing
                CompanySettingType.TimeStaffingShiftAccountDimId, //TODO

                //Not relevant
                CompanySettingType.PayrollExportForaAgreementNumber,
                CompanySettingType.PayrollExportITP1Number,
                CompanySettingType.PayrollExportITP2Number,
                CompanySettingType.PayrollExportKPAAgreementNumber,
                CompanySettingType.PayrollExportSNKFOMemberNumber,
                CompanySettingType.PayrollExportSNKFOWorkPlaceNumber,
                CompanySettingType.PayrollExportSNKFOAffiliateNumber,
                CompanySettingType.PayrollExportSNKFOAgreementNumber,
                CompanySettingType.PayrollExportCommunityCode,
                CompanySettingType.PayrollExportSCBWorkSite,
                CompanySettingType.PayrollExportCFARNumber,
                CompanySettingType.PayrollArbetsgivarintygnuApiNyckel,
                CompanySettingType.PayrollArbetsgivarintygnuArbetsgivarId,
                CompanySettingType.PayrollExportKPAManagementNumber
            };

            CopyCompanySettings(CompanySettingTypeGroup.Time, newCompanyId, templateCompanyId, excludeSettingTypes);
            CopyCompanySettings(CompanySettingTypeGroup.Payroll, newCompanyId, templateCompanyId, excludeSettingTypes);
            if (copyPayrollProductsAndTimeCodes)
                CopyCompanyDefaultTimeCodeSetting(newCompanyId, templateCompanyId);
            if (copyEmployeeGroups)
                CopyCompanyDefaultEmployeeGroupSetting(newCompanyId, templateCompanyId);
            if (copyPayrollGroups)
                CopyCompanyDefaultPayrollGroupSetting(newCompanyId, templateCompanyId);
            if (copyVacationGroups)
                CopyCompanyDefaultVacationGroupSetting(newCompanyId, templateCompanyId);
            if (copyTimePeriods)
                CopyCompanyDefaultTimePeriodHead(newCompanyId, templateCompanyId);
            if (copyAttestSettings)
            {
                CopyCompanyDefaultShiftAccountDim(newCompanyId, templateCompanyId);

                //SalaryExport
                CopyCompanyAttestSetting(CompanySettingType.SalaryExportPayrollMinimumAttestStatus, newCompanyId, templateCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.SalaryExportPayrollResultingAttestStatus, newCompanyId, templateCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.SalaryExportInvoiceMinimumAttestStatus, newCompanyId, templateCompanyId, TermGroup_AttestEntity.InvoiceTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.SalaryExportInvoiceResultingAttestStatus, newCompanyId, templateCompanyId, TermGroup_AttestEntity.InvoiceTime, SoeModule.Time);

                //SalaryPayment
                CopyCompanyAttestSetting(CompanySettingType.SalaryPaymentLockedAttestStateId, newCompanyId, templateCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.SalaryPaymentApproved1AttestStateId, newCompanyId, templateCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.SalaryPaymentApproved2AttestStateId, newCompanyId, templateCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId, newCompanyId, templateCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);

                //Attest
                CopyCompanyAttestSetting(CompanySettingType.TimeAutoAttestSourceAttestStateId, newCompanyId, templateCompanyId, TermGroup_AttestEntity.InvoiceTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.TimeAutoAttestTargetAttestStateId, newCompanyId, templateCompanyId, TermGroup_AttestEntity.InvoiceTime, SoeModule.Time);
                CopyCompanyAttestSetting(CompanySettingType.MobileTimeAttestResultingAttestStatus, newCompanyId, templateCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);

                //Payroll
                CopyPayrollProductSetting(CompanySettingType.PayrollAccountingDistributionPayrollProduct, newCompanyId, templateCompanyId);
            }

            CopyCompanySetting(CompanySettingType.UseAccountHierarchy, newCompanyId, templateCompanyId);
            CopyCompanyAccountDimSetting(CompanySettingType.DefaultEmployeeAccountDimEmployee, newCompanyId, templateCompanyId);
            CopyCompanyAccountDimSetting(CompanySettingType.DefaultEmployeeAccountDimSelector, newCompanyId, templateCompanyId);

            #endregion

            return result;
        }

        public ActionResult CopyPayrollSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, bool copyAccountSettings, bool copyattestSettings, bool copyPayrollProductsAndTimeCodes)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Group Payroll settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {

            };

            CopyCompanySettings(CompanySettingTypeGroup.Payroll, newCompanyId, templateCompanyId, excludeSettingTypes);

            #endregion

            return result;
        }

        public ActionResult CopyProjectSettingsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            #region Group Project Settings

            //Exclude unique settings (id dependent values, copy real id later if needed)
            List<CompanySettingType> excludeSettingTypes = new List<CompanySettingType>()
            {
                //Project
                CompanySettingType.ProjectIncludeTimeProjectReport,
                CompanySettingType.ProjectDefaultTimeCodeId
            };

            CopyCompanySettings(CompanySettingTypeGroup.Project, newCompanyId, templateCompanyId, excludeSettingTypes);

            #endregion

            return result;
        }

        #endregion

        #region BaseProducts

        public ActionResult CopyBaseProductsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, SoeModule module, ref Dictionary<int, int> vatCodeMapping)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    #region Prereq

                    Company company = CompanyManager.GetCompany(entities, newCompanyId);
                    if (company == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    CompanySettingTypeGroup settingTypeGroup = GetBaseProductCompanySettingTypeGroup(module);
                    if (settingTypeGroup == CompanySettingTypeGroup.Unknown)
                        return result;

                    Dictionary<int, InvoiceProduct> productSettingsDict = new Dictionary<int, InvoiceProduct>();

                    List<ProductUnit> existingProductUnits = update ? ProductManager.GetProductUnits(entities, newCompanyId).ToList() : new List<ProductUnit>();
                    List<ProductGroup> existingProductGroups = update ? ProductGroupManager.GetProductGroups(entities, newCompanyId).ToList() : new List<ProductGroup>();
                    List<UserCompanySetting> templateSettings = SettingManager.GetCompanySettings(entities, (int)CompanySettingTypeGroup.BaseProducts, templateCompanyId);

                    #endregion

                    #region InvoiceProducts

                    foreach (UserCompanySetting templateSetting in templateSettings)
                    {
                        int templateProductId = SettingManager.GetIntSetting(SettingMainType.Company, templateSetting.SettingTypeId, 0, templateCompanyId, 0);
                        InvoiceProduct templateProduct = ProductManager.GetInvoiceProduct(templateProductId, true, true, true);
                        if (templateProduct == null)
                            continue;

                        int vatCodeId = -1;
                        if (templateProduct.VatCodeId.HasValue)
                            vatCodeMapping.TryGetValue(templateProduct.VatCodeId.Value, out vatCodeId);

                        InvoiceProduct product = ProductManager.GetInvoiceProductByProductNr(entities, templateProduct.Number, newCompanyId);
                        if (product == null)
                        {
                            #region Invoice Product

                            product = new InvoiceProduct()
                            {
                                Type = templateProduct.Type,
                                Number = templateProduct.Number,
                                Name = templateProduct.Name,
                                Description = templateProduct.Description,
                                AccountingPrio = templateProduct.AccountingPrio,
                                CalculationType = templateProduct.CalculationType,
                                VatType = templateProduct.VatType,
                                VatCodeId = vatCodeId.ToNullable(),
                            };

                            SetCreatedProperties(product);
                            product.Company.Add(company);

                            if (!update)
                            {
                                #region ProductUnit

                                if (templateProduct.ProductUnit != null)
                                {
                                    product.ProductUnit = existingProductUnits.FirstOrDefault(i => i.Code.ToLower() == templateProduct.ProductUnit.Code.ToLower());
                                    if (product.ProductUnit == null)
                                    {
                                        product.ProductUnit = new ProductUnit()
                                        {
                                            Code = templateProduct.ProductUnit.Code,
                                            Name = templateProduct.ProductUnit.Name,

                                            //Set references
                                            Company = company,
                                        };
                                        SetCreatedProperties(product.ProductUnit);
                                        existingProductUnits.Add(product.ProductUnit);
                                    }
                                }

                                #endregion

                                #region ProductGroup

                                if (templateProduct.ProductGroup != null)
                                {
                                    product.ProductGroup = existingProductGroups.FirstOrDefault(i => i.Code.ToLower() == templateProduct.ProductGroup.Code.ToLower());
                                    if (product.ProductGroup == null)
                                    {
                                        product.ProductGroup = new ProductGroup()
                                        {
                                            Code = templateProduct.ProductGroup.Code,
                                            Name = templateProduct.ProductGroup.Name,

                                            //Set references
                                            Company = company,
                                        };
                                        SetCreatedProperties(product.ProductGroup);
                                        existingProductGroups.Add(product.ProductGroup);
                                    }
                                }

                                #endregion

                                #region ProductAccountStd

                                if (templateProduct.ProductAccountStd != null && templateProduct.ProductAccountStd.Count > 0)
                                {
                                    foreach (ProductAccountStd productAccountStd in templateProduct.ProductAccountStd)
                                    {
                                        AccountStd accountStd = AccountManager.GetAccountStdByNr(entities, productAccountStd.AccountStd?.Account?.AccountNr ?? String.Empty, newCompanyId);
                                        if (accountStd == null)
                                            continue;

                                        ProductAccountStd newProductAccountStd = new ProductAccountStd()
                                        {
                                            Type = productAccountStd.Type,
                                            Percent = productAccountStd.Percent,

                                            //Set references
                                            AccountStd = accountStd,
                                        };
                                        SetCreatedProperties(productAccountStd);
                                        product.ProductAccountStd.Add(newProductAccountStd);
                                    }
                                }

                                #endregion
                            }

                            productSettingsDict.Add(templateSetting.SettingTypeId, product);

                            #endregion
                        }
                    }

                    result = SaveChanges(entities);
                    if (!result.Success)
                        result = LogCopyError("InvoiceProduct", templateCompanyId, newCompanyId, saved: true);

                    #endregion

                    #region UserCompanySettings

                    if (result.Success)
                    {
                        Dictionary<int, int> intSettings = new Dictionary<int, int>();
                        foreach (var pair in productSettingsDict)
                        {
                            intSettings.Add(pair.Key, pair.Value.ProductId);
                        }

                        result = SettingManager.UpdateInsertIntSettings(SettingMainType.Company, intSettings, 0, newCompanyId, 0);
                        if (!result.Success)
                            result = LogCopyError("UserCompanySetting", templateCompanyId, newCompanyId, saved: true);
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Success = false;
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else if (result.Exception != null)
                        base.LogError(result.Exception, this.log);
                    else if (result.ErrorMessage != null)
                        base.LogError(result.ErrorMessage);
                    else
                        base.LogError("Error in CopyPayrollGroupsPriceTypesAndPriceFormulasFromTemplateCompany");

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public CompanySettingTypeGroup GetBaseProductCompanySettingTypeGroup(SoeModule module)
        {
            CompanySettingTypeGroup settingTypeGroup = CompanySettingTypeGroup.Unknown;
            switch (module)
            {
                case SoeModule.Billing:
                    settingTypeGroup = CompanySettingTypeGroup.BaseProducts;
                    break;
                case SoeModule.Time:
                    settingTypeGroup = CompanySettingTypeGroup.BaseAccountsEmployeeGroup;
                    break;
            }
            return settingTypeGroup;
        }

        #endregion

        #region BaseAccounts

        public ActionResult CopyBaseAccountsFromTemplateCompany(int newCompanyId, int templateCompanyId, int userId, bool update, SoeModule module)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                CompanySettingTypeGroup settingTypeGroup = GetBaseProductCompanySettingTypeGroup(module);
                if (settingTypeGroup == CompanySettingTypeGroup.Unknown)
                    return result;

                #endregion

                #region Account

                //Account not copied from template Company. Should already be added.

                #endregion

                #region UserCompanySettings

                CopyCompanyAccountSettings(settingTypeGroup, newCompanyId, templateCompanyId);

                #endregion
            }

            return result;
        }

        #endregion

        #region Help-methods

        public static int GenerateUniqueKey(int actorCompanyId, int sysCompDbId)
        {
            // Shift the actorCompanyId to the left by 7 bits and then add sysCompDbId
            int combinedKey = (actorCompanyId << 7) | (sysCompDbId & 0x7F); // Use 7 bits for sysCompDbId

            // Set the most significant bit (bit 31) to indicate that it's a combined key
            combinedKey |= (1 << 31);

            AddKeyToCompanyInCache(actorCompanyId, sysCompDbId, combinedKey.ToString());

            return combinedKey;
        }

        public static bool IsCombinedKey(int key)
        {
            // Check if the most significant bit (bit 31) is set
            return (key & (1 << 31)) != 0;
        }

        public static int GetActorCompanyIdFromCombinedKey(int key)
        {
            // First check if this is a combined key, if not, return the key
            if (!IsCombinedKey(key))
                return key;

            var keys = GetKeyFromCompanyInCache(key);

            if (keys != null)
                return keys.Item1;

            // Mask the key to exclude the flag bit and the sysCompDbId bits,
            // then shift right by 7 bits to extract the actorCompanyId
            return (key & 0x7FFFFF80) >> 7; // Mask with 23 bits for actorCompanyId
        }

        public static int GetSysCompDbIdFromCombinedKey(int key)
        {
            // First check if this is a combined key, if not return 0
            if (!IsCombinedKey(key))
                return 0;

            var keys = GetKeyFromCompanyInCache(key);

            if (keys != null)
                return keys.Item2;

            // Use bitwise AND to extract the sysCompDbId (last 7 bits of the key)
            return key & 0x7F;
        }

        private void CopyCompanySettings(CompanySettingTypeGroup settingTypeGroup, int newCompanyId, int templateCompanyId, List<CompanySettingType> excludeSettingTypes = null)
        {
            if (excludeSettingTypes == null)
                excludeSettingTypes = new List<CompanySettingType>();
            List<CompanySettingType> reportSettingTypes = GetReportSettingTypes();
            excludeSettingTypes.AddRange(reportSettingTypes);

            List<UserCompanySetting> templateSettings = SettingManager.GetCompanySettings((int)settingTypeGroup, templateCompanyId);
            foreach (UserCompanySetting templateSetting in templateSettings)
            {
                if (excludeSettingTypes.Contains((CompanySettingType)templateSetting.SettingTypeId))
                    continue;

                CopyCompanySetting(templateSetting, newCompanyId, templateCompanyId);
            }
        }

        private void CopyCompanySetting(CompanySettingType settingType, int newCompanyId, int templateCompanyId)
        {
            UserCompanySetting templateSetting = SettingManager.GetUserCompanySetting(SettingMainType.Company, (int)settingType, 0, templateCompanyId, 0);
            if (templateSetting != null)
                CopyCompanySetting(templateSetting, newCompanyId, templateCompanyId);
        }

        private void CopyCompanySetting(UserCompanySetting templateSetting, int newCompanyId, int templateCompanyId)
        {
            if (templateSetting == null)
                return;

            ActionResult result = new ActionResult(true);

            switch (templateSetting.DataTypeId)
            {
                case (int)SettingDataType.String:
                    #region String

                    if (!String.IsNullOrEmpty(templateSetting.StrData))
                        result = SettingManager.UpdateInsertStringSetting(SettingMainType.Company, templateSetting.SettingTypeId, templateSetting.StrData, 0, newCompanyId, 0);

                    #endregion
                    break;
                case (int)SettingDataType.Integer:
                    #region Integer

                    if (templateSetting.IntData.HasValue)
                    {
                        result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, templateSetting.SettingTypeId, templateSetting.IntData.Value, 0, newCompanyId, 0);
                        if (templateSetting.SettingTypeId == (int)CompanySettingType.DefaultEmploymentContractShortSubstituteReport)
                            LogCopyError("UserCompanySetting", "UserCompanySettingId", templateSetting.UserCompanySettingId, "", $"DefaultEmploymentContractShortSubstituteReport success={result.Success}", templateCompanyId, newCompanyId, add: true);
                    }

                    #endregion
                    break;
                case (int)SettingDataType.Boolean:
                    #region Boolean

                    if (templateSetting.BoolData.HasValue)
                        result = SettingManager.UpdateInsertBoolSetting(SettingMainType.Company, templateSetting.SettingTypeId, templateSetting.BoolData.Value, 0, newCompanyId, 0);

                    #endregion
                    break;
                case (int)SettingDataType.Date:
                case (int)SettingDataType.Time:
                    #region Date / Time

                    if (templateSetting.DateData.HasValue)
                        result = SettingManager.UpdateInsertDateSetting(SettingMainType.Company, templateSetting.SettingTypeId, templateSetting.DateData.Value, 0, newCompanyId, 0);

                    #endregion
                    break;
                case (int)SettingDataType.Decimal:
                    #region Decimal

                    if (templateSetting.DecimalData.HasValue)
                        result = SettingManager.UpdateInsertDecimalSetting(SettingMainType.Company, templateSetting.SettingTypeId, templateSetting.DecimalData.Value, 0, newCompanyId, 0);

                    #endregion
                    break;
            }

            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", templateSetting.SettingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyAccountSettings(CompanySettingTypeGroup settingTypeGroup, int newCompanyId, int templateCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<UserCompanySetting> templateSettings = SettingManager.GetCompanySettings(entities, (int)settingTypeGroup, templateCompanyId);
                foreach (UserCompanySetting templateSetting in templateSettings)
                {
                    if (templateSetting.IntData.HasValue)
                    {
                        int accountId = templateSetting.IntData.Value;

                        Account templateAccount = AccountManager.GetAccount(templateCompanyId, accountId);
                        if (templateAccount != null)
                        {
                            Account account = AccountManager.GetAccountByDimNr(templateAccount.AccountNr, Constants.ACCOUNTDIM_STANDARD, newCompanyId);
                            if (account != null)
                            {
                                var result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, templateSetting.SettingTypeId, account.AccountId, 0, newCompanyId, 0);
                                if (!result.Success)
                                    LogCopyError("UserCompanySetting", "SettingTypeId", templateSetting.SettingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
                            }
                        }
                    }
                }
            }
        }

        private void CopyCompanyVoucherSeriesSetting(CompanySettingType companySettingType, int newCompanyId, int templateCompanyId)
        {
            int setting = SettingManager.GetIntSetting(SettingMainType.Company, (int)companySettingType, 0, templateCompanyId, 0);
            if (setting > 0)
            {
                VoucherSeriesType templateVoucherSeriesType = VoucherManager.GetVoucherSeriesType(setting, templateCompanyId);
                if (templateVoucherSeriesType != null)
                {
                    VoucherSeriesType voucherSeriesType = VoucherManager.GetVoucherSeriesTypeByName(templateVoucherSeriesType.Name, newCompanyId, templateVoucherSeriesType.VoucherSeriesTypeNr);
                    if (voucherSeriesType != null)
                    {
                        var result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)companySettingType, voucherSeriesType.VoucherSeriesTypeId, 0, newCompanyId, 0);
                        if (!result.Success)
                            LogCopyError("UserCompanySetting", "SettingTypeId", (int)companySettingType, "", "", templateCompanyId, newCompanyId, add: true);
                    }
                }
            }
        }

        private void CopySupplierInvoiceAttestation(int newCompanyId, int templateCompanyId)
        {
            this.CopySupplierInvoiceAttestFlowDefaultAttest(
                newCompanyId, templateCompanyId);
            this.CopySupplierInvoiceAttestFlowStatusToStartAccountsPayableFlow(
                newCompanyId, templateCompanyId);
        }

        private void CopySupplierInvoiceAttestFlowDefaultAttest(
            int newCompanyId, 
            int templateCompanyId)
        {
            Dictionary<int, string> templateDict = AttestManager
                .GetAttestWorkFlowTemplateHeadDict(
                    templateCompanyId,
                    false,
                    TermGroup_AttestEntity.SupplierInvoice
                );

            Dictionary<int, string> newCompanyDict = AttestManager
                .GetAttestWorkFlowTemplateHeadDict(
                    newCompanyId,
                    false,
                    TermGroup_AttestEntity.SupplierInvoice
                );

            this.CopyIdSettingFromMatchedValue(
                CompanySettingType.SupplierInvoiceAttestFlowDefaultAttestTemplate,
                templateDict,
                newCompanyDict,
                templateCompanyId,
                newCompanyId);
        }

        private void CopySupplierInvoiceAttestFlowStatusToStartAccountsPayableFlow(
            int newCompanyId,
            int templateCompanyId)
        {
            Dictionary<int, string> templateDict = AttestManager
                .GetAttestStatesDict(
                    templateCompanyId,
                    TermGroup_AttestEntity.SupplierInvoice, 
                    SoeModule.Economy, 
                    false, 
                    false
                );

            Dictionary<int, string> newCompanyDict = AttestManager
                .GetAttestStatesDict(
                    newCompanyId,
                    TermGroup_AttestEntity.SupplierInvoice,
                    SoeModule.Economy,
                    false,
                    false
                );

            this.CopyIdSettingFromMatchedValue(
                CompanySettingType.SupplierInvoiceAttestFlowStatusToStartAccountsPayableFlow, 
                templateDict, 
                newCompanyDict, 
                templateCompanyId, 
                newCompanyId);

        }

        private void CopyIdSettingFromMatchedValue(
            CompanySettingType settingTypeId,
            Dictionary<int, string> templateDict,
            Dictionary<int, string> newCompanyDict, 
            int templateCompanyId,
            int newCompanyId)
        {
            if (newCompanyDict.Count > 0 && templateDict.Count > 0)
            {

                UserCompanySetting templateSetting = SettingManager.GetUserCompanySetting(
                    SettingMainType.Company,
                    (int)settingTypeId,
                    0,
                    templateCompanyId,
                    0);


                if (templateSetting?.IntData.HasValue == true
                && templateDict.ContainsKey(templateSetting.IntData.Value))
                {
                    string templateName = templateDict[templateSetting.IntData.Value];

                    if (newCompanyDict.ContainsValue(templateName))
                    {
                        int newSettingId = newCompanyDict
                            .FirstOrDefault(x => x.Value == templateName).Key;

                        ActionResult result = SettingManager.UpdateInsertIntSetting(
                            SettingMainType.Company,
                            templateSetting.SettingTypeId,
                            newSettingId,
                            0,
                            newCompanyId,
                            0);
                        if (!result.Success)
                        {
                            LogCopyError(
                                "UserCompanySetting",
                                "SettingTypeId",
                                templateSetting.SettingTypeId,
                                "",
                                "",
                                templateCompanyId,
                                newCompanyId,
                                add: true);
                        }
                    }
                }
            }

        }

        private void CopyCompanyAttestSetting(CompanySettingType companySettingType, int newCompanyId, int templateCompanyId, TermGroup_AttestEntity entity, SoeModule module)
        {
            int setting = SettingManager.GetIntSetting(SettingMainType.Company, (int)companySettingType, 0, templateCompanyId, 0);
            if (setting > 0)
            {
                AttestState templateAttestState = AttestManager.GetAttestState(setting);
                if (templateAttestState != null)
                {
                    AttestState attestState = AttestManager.GetAttestState(entity, newCompanyId, module, templateAttestState.Name);
                    if (attestState != null)
                    {
                        var result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)companySettingType, attestState.AttestStateId, 0, newCompanyId, 0);
                        if (!result.Success)
                            LogCopyError("UserCompanySetting", "SettingTypeId", (int)companySettingType, "", "", templateCompanyId, newCompanyId, add: true);
                    }
                }
            }
        }

        private void CopyPayrollProductSetting(CompanySettingType companySettingType, int newCompanyId, int templateCompanyId)
        {
            int setting = SettingManager.GetIntSetting(SettingMainType.Company, (int)companySettingType, 0, templateCompanyId, 0);
            if (setting > 0)
            {
                PayrollProduct templatePayrollProduct = ProductManager.GetPayrollProduct(setting);
                if (templatePayrollProduct != null)
                {
                    PayrollProduct payrollProduct = ProductManager.GetPayrollProductByNumber(templatePayrollProduct.Number, newCompanyId);
                    if (payrollProduct != null)
                    {
                        var result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)companySettingType, payrollProduct.ProductId, 0, newCompanyId, 0);
                        if (!result.Success)
                            LogCopyError("UserCompanySetting", "SettingTypeId", (int)companySettingType, "", "", templateCompanyId, newCompanyId, add: true);
                    }
                }
            }
        }

        private void CopyCompanyAccountDimSetting(CompanySettingType companySettingType, int newCompanyId, int templateCompanyId)
        {
            int setting = SettingManager.GetIntSetting(SettingMainType.Company, (int)companySettingType, 0, templateCompanyId, 0);
            if (setting > 0)
            {
                AccountDim templateAccountDim = AccountManager.GetAccountDim(setting, templateCompanyId);
                if (templateAccountDim != null)
                {
                    AccountDim newAccountDim = AccountManager.GetAccountDimByNr(templateAccountDim.AccountDimNr, newCompanyId);
                    if (newAccountDim != null)
                    {
                        var result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)companySettingType, newAccountDim.AccountDimId, 0, newCompanyId, 0);
                        if (!result.Success)
                            LogCopyError("UserCompanySetting", "SettingTypeId", (int)companySettingType, "", "", templateCompanyId, newCompanyId, add: true);
                    }
                }
            }
        }

        private void CopyCompanyReportSettings(Dictionary<int, Report> reportMapping, int newCompanyId, int templateCompanyId)
        {
            foreach (SoeModule module in Enum.GetValues(typeof(SoeModule)))
            {
                if (module == SoeModule.None)
                    continue;
                CopyCompanyReportSettings(reportMapping, module, newCompanyId, templateCompanyId);
            }
        }

        private void CopyCompanyReportSettings(Dictionary<int, Report> reportMapping, SoeModule module, int newCompanyId, int templateCompanyId)
        {
            List<CompanySettingType> companySettingTypes = GetReportSettingTypes(module);
            foreach (CompanySettingType companySettingType in companySettingTypes)
            {
                CopyCompanyReportSetting(companySettingType, reportMapping, newCompanyId, templateCompanyId);
            }
        }

        private List<CompanySettingType> GetReportSettingTypes(SoeModule module = SoeModule.None)
        {
            List<CompanySettingType> settingTypes = new List<CompanySettingType>();

            if (module == SoeModule.Economy || module == SoeModule.None)
            {
                settingTypes.Add(CompanySettingType.SupplierDefaultBalanceList);//report
                settingTypes.Add(CompanySettingType.SupplierDefaultPaymentSuggestionList);//report
                settingTypes.Add(CompanySettingType.SupplierDefaultChecklistPayments);//report
                settingTypes.Add(CompanySettingType.CustomerDefaultBalanceList);
                settingTypes.Add(CompanySettingType.CustomerDefaultReminderTemplate);
                settingTypes.Add(CompanySettingType.CustomerDefaultInterestTemplate);
                settingTypes.Add(CompanySettingType.CustomerDefaultInterestRateCalculationTemplate);
                settingTypes.Add(CompanySettingType.CustomerDefaultPaymentConditionClaimAndInterest);//
                settingTypes.Add(CompanySettingType.AccountingDefaultAccountingOrder);//report
                settingTypes.Add(CompanySettingType.AccountingDefaultVoucherList);//report
                settingTypes.Add(CompanySettingType.AccountingDefaultAnalysisReport);//report
                settingTypes.Add(CompanySettingType.AccountingVoucherSeriesTypeVat);//


            }
            if (module == SoeModule.Billing || module == SoeModule.None)
            {
                settingTypes.Add(CompanySettingType.BillingDefaultContractTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultOfferTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultOrderTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultWorkingOrderTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultInvoiceTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultTimeProjectReportTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultHouseholdDeductionTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultExpenseReportTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultEmailTemplate);
                settingTypes.Add(CompanySettingType.BillingOfferDefaultEmailTemplate);
                settingTypes.Add(CompanySettingType.BillingOrderDefaultEmailTemplate);
                settingTypes.Add(CompanySettingType.BillingContractDefaultEmailTemplate);
                settingTypes.Add(CompanySettingType.BillingDefaultHouseholdDeductionTemplate);//report
            }
            if (module == SoeModule.Time || module == SoeModule.None)
            {
                settingTypes.Add(CompanySettingType.TimeDefaultEmployeeScheduleDayReport);
                settingTypes.Add(CompanySettingType.TimeDefaultEmployeeScheduleWeekReport);
                settingTypes.Add(CompanySettingType.TimeDefaultEmployeeTemplateScheduleDayReport);
                settingTypes.Add(CompanySettingType.TimeDefaultEmployeeTemplateScheduleWeekReport);
                settingTypes.Add(CompanySettingType.TimeDefaultEmployeePostTemplateScheduleDayReport);
                settingTypes.Add(CompanySettingType.TimeDefaultEmployeePostTemplateScheduleWeekReport);
                settingTypes.Add(CompanySettingType.TimeDefaultScheduleTasksAndDeliverysDayReport);
                settingTypes.Add(CompanySettingType.TimeDefaultScheduleTasksAndDeliverysWeekReport);
                settingTypes.Add(CompanySettingType.TimeDefaultMonthlyReport);
                settingTypes.Add(CompanySettingType.DefaultEmployeeVacationDebtReport);
                settingTypes.Add(CompanySettingType.TimeDefaultTimeSalarySpecificationReport);
                settingTypes.Add(CompanySettingType.TimeDefaultTimeSalaryControlInfoReport);
                settingTypes.Add(CompanySettingType.TimeDefaultKU10Report);
                settingTypes.Add(CompanySettingType.DefaultEmploymentContractShortSubstituteReport);
                settingTypes.Add(CompanySettingType.DefaultPayrollSlipReport);
                settingTypes.Add(CompanySettingType.PayrollSettingsDefaultReport);
            }

            return settingTypes;
        }

        private void CopyCompanyReportSetting(CompanySettingType companySettingType, Dictionary<int, Report> reportMapping, int newCompanyId, int templateCompanyId)
        {
            int id = SettingManager.GetIntSetting(SettingMainType.Company, (int)companySettingType, 0, templateCompanyId, 0);
            if (id <= 0 || !reportMapping.ContainsKey(id))
                return;

            Report report = reportMapping[id];
            if (report == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)companySettingType, report.ReportId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", (int)companySettingType, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyDefaultRoleSetting(int newCompanyId, int templateCompanyId)
        {
            int settingTypeId = (int)CompanySettingType.DefaultRole;
            int id = SettingManager.GetIntSetting(SettingMainType.Company, settingTypeId, 0, templateCompanyId, 0);

            Role templateDefaultRole = id > 0 ? RoleManager.GetRole(id, templateCompanyId) : null;
            if (templateDefaultRole == null)
                return;

            Role newDefaultRole = RoleManager.GetRoleByName(templateDefaultRole.Name, newCompanyId);
            if (newDefaultRole == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, settingTypeId, newDefaultRole.RoleId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", settingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyDefaultTimeCodeSetting(int newCompanyId, int templateCompanyId)
        {
            int settingTypeId = (int)CompanySettingType.TimeDefaultTimeCode;
            int id = SettingManager.GetIntSetting(SettingMainType.Company, settingTypeId, 0, templateCompanyId, 0);

            TimeCode templateTimeCode = id > 0 ? TimeCodeManager.GetTimeCode(id, templateCompanyId, false) : null;
            if (templateTimeCode == null)
                return;

            TimeCode newTimeCode = TimeCodeManager.GetTimeCode(templateTimeCode.Name, templateTimeCode.Code, templateTimeCode.Type, newCompanyId);
            if (newTimeCode == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, settingTypeId, newTimeCode.TimeCodeId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", settingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyDefaultEmployeeGroupSetting(int newCompanyId, int templateCompanyId)
        {
            int settingTypeId = (int)CompanySettingType.TimeDefaultEmployeeGroup;
            int id = SettingManager.GetIntSetting(SettingMainType.Company, settingTypeId, 0, templateCompanyId, 0);

            EmployeeGroup templateEmployeeGroup = id > 0 ? EmployeeManager.GetEmployeeGroup(id) : null;
            if (templateEmployeeGroup == null)
                return;

            EmployeeGroup newEmployeeGroup = EmployeeManager.GetEmployeeGroupByName(templateEmployeeGroup.Name, newCompanyId);
            if (newEmployeeGroup == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, settingTypeId, newEmployeeGroup.EmployeeGroupId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", settingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyDefaultPayrollGroupSetting(int newCompanyId, int templateCompanyId)
        {
            int settingTypeId = (int)CompanySettingType.TimeDefaultPayrollGroup;
            int id = SettingManager.GetIntSetting(SettingMainType.Company, settingTypeId, 0, templateCompanyId, 0);

            PayrollGroup templatePayrollGroup = id > 0 ? PayrollManager.GetPayrollGroup(id) : null;
            if (templatePayrollGroup == null)
                return;

            PayrollGroup newPayrollGroup = PayrollManager.GetPayrollGroupByName(templatePayrollGroup.Name, newCompanyId);
            if (newPayrollGroup == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, settingTypeId, newPayrollGroup.PayrollGroupId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", settingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyDefaultVacationGroupSetting(int newCompanyId, int templateCompanyId)
        {
            int settingTypeId = (int)CompanySettingType.TimeDefaultVacationGroup;
            int id = SettingManager.GetIntSetting(SettingMainType.Company, settingTypeId, 0, templateCompanyId, 0);

            VacationGroup templateVacationGroup = id > 0 ? PayrollManager.GetVacationGroup(id) : null;
            if (templateVacationGroup == null)
                return;

            VacationGroup newVacationGroup = PayrollManager.GetVacationGroupByName(templateVacationGroup.Name, newCompanyId);
            if (newVacationGroup == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, settingTypeId, newVacationGroup.VacationGroupId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", settingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyDefaultTimePeriodHead(int newCompanyId, int templateCompanyId)
        {
            int settingTypeId = (int)CompanySettingType.TimeDefaultTimePeriodHead;
            int id = SettingManager.GetIntSetting(SettingMainType.Company, settingTypeId, 0, templateCompanyId, 0);

            TimePeriodHead templateTimePeriodHead = id > 0 ? TimePeriodManager.GetTimePeriodHead(id, templateCompanyId) : null;
            if (templateTimePeriodHead == null)
                return;

            TimePeriodHead newTimePeriodHead = TimePeriodManager.GetTimePeriodHeadByName(templateTimePeriodHead.Name, newCompanyId);
            if (newTimePeriodHead == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, settingTypeId, newTimePeriodHead.TimePeriodHeadId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", settingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private void CopyCompanyDefaultShiftAccountDim(int newCompanyId, int templateCompanyId)
        {
            int settingTypeId = (int)CompanySettingType.TimeStaffingShiftAccountDimId;
            int id = SettingManager.GetIntSetting(SettingMainType.Company, settingTypeId, 0, templateCompanyId, 0);

            AccountDim templateAccountDim = id > 0 ? AccountManager.GetAccountDim(id, templateCompanyId) : null;
            if (templateAccountDim == null)
                return;

            AccountDim newAccountDim = AccountManager.GetAccountDimByNr(templateAccountDim.AccountDimNr, newCompanyId);
            if (newAccountDim == null)
                return;

            ActionResult result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, settingTypeId, newAccountDim.AccountDimId, 0, newCompanyId, 0);
            if (!result.Success)
                LogCopyError("UserCompanySetting", "SettingTypeId", settingTypeId, "", "", templateCompanyId, newCompanyId, add: true);
        }

        private int GetExistingPayrollProductId(List<PayrollProduct> existingProducts, List<PayrollProduct> templateProducts, int templateProductId, int defaultValue = 0)
        {
            return GetExistingPayrollProduct(existingProducts, templateProducts, templateProductId)?.ProductId ?? defaultValue;
        }

        private PayrollProduct GetExistingPayrollProduct(List<PayrollProduct> existingProducts, List<PayrollProduct> templateProducts, int templateProductId)
        {
            PayrollProduct templateProduct = templateProducts.FirstOrDefault(i => i.ProductId == templateProductId);
            if (templateProduct == null)
                return null;

            PayrollProduct existingProduct = existingProducts.FirstOrDefault(p => p.Number.Trim().ToLower().Equals(templateProduct.Number.Trim().ToLower()));
            if (existingProduct == null)
                existingProduct = existingProducts.FirstOrDefault(p => p.SysPayrollTypeLevel1 == templateProduct.SysPayrollTypeLevel1 && p.SysPayrollTypeLevel2 == templateProduct.SysPayrollTypeLevel2 && p.SysPayrollTypeLevel3 == templateProduct.SysPayrollTypeLevel3 && p.SysPayrollTypeLevel4 == templateProduct.SysPayrollTypeLevel4);
            return existingProduct;
        }

        private int GetExistingInvoiceProductId(List<InvoiceProduct> existingProducts, List<InvoiceProduct> templateProducts, int templateProductId, int defaultValue = 0)
        {
            return GetExistingInvoiceProduct(existingProducts, templateProducts, templateProductId)?.ProductId ?? defaultValue;
        }

        private InvoiceProduct GetExistingInvoiceProduct(List<InvoiceProduct> existingProducts, List<InvoiceProduct> templateProducts, int templateProductId)
        {
            InvoiceProduct templateProduct = templateProducts.FirstOrDefault(i => i.ProductId == templateProductId);
            if (templateProduct == null)
                return null;

            return existingProducts.FirstOrDefault(p => p.Number.Trim().ToLower().Equals(templateProduct.Number.Trim().ToLower()));
        }

        private ActionResult GetAccountIdMapping(int newCompanyId, int templateCompanyId)
        {
            using (var entities = new CompEntities())
            {
                AccountDim templateAccountDimStd = AccountManager.GetAccountDimStd(entities, templateCompanyId);
                if (templateAccountDimStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                List<Account> templateAccounts = AccountManager.GetAccountsByDim(entities, templateAccountDimStd.AccountDimId, templateCompanyId, true, true, true).ToList();
                if (templateAccounts.IsNullOrEmpty())
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                AccountDim newAccountDimStd = AccountManager.GetAccountDimStd(entities, newCompanyId);
                if (newAccountDimStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                List<Account> existingAccounts = AccountManager.GetAccountsByDim(entities, newAccountDimStd.AccountDimId, newCompanyId, true, true, false).ToList();
                var dict = new Dictionary<int, int>(templateAccounts.Count);

                foreach (Account templateAccount in templateAccounts)
                {
                    #region AccountStd

                    if (templateAccount.AccountStd == null)
                        continue;

                    Account existingAccount = existingAccounts.FirstOrDefault(i => i.Name == templateAccount.Name && i.AccountNr == templateAccount.AccountNr);
                    if (existingAccount == null)
                        continue;

                    if (!dict.ContainsKey(templateAccount.AccountId))
                        dict.Add(templateAccount.AccountId, existingAccount.AccountId);

                    #endregion
                }

                return new ActionResult() { IntDict = dict };
            }
        }

        private List<Tuple<int, int, Dictionary<int, int>>> GetAccountDimMappingsWithAccounts(List<AccountDim> templateDims, List<AccountDim> newDims)
        {
            List<Tuple<int, int, Dictionary<int, int>>> mappings = new List<Tuple<int, int, Dictionary<int, int>>>();

            foreach (var dim in templateDims)
            {
                var newDim = dim.AccountDimNr == 1 ? newDims.OrderBy(d => d.AccountDimNr).FirstOrDefault(d => d.AccountDimNr == dim.AccountDimNr) : newDims.OrderBy(d => d.AccountDimNr).FirstOrDefault(d => d.AccountDimNr == dim.AccountDimNr && d.Name == dim.Name);
                if (newDim != null)
                {
                    Tuple<int, int, Dictionary<int, int>> dimTuple = new Tuple<int, int, Dictionary<int, int>>(dim.AccountDimId, newDim.AccountDimId, new Dictionary<int, int>());
                    foreach (var account in dim.Account)
                    {
                        var newAccount = newDim.Account.FirstOrDefault(a => a.AccountNr == account.AccountNr);
                        if (newAccount != null)
                            dimTuple.Item3.Add(account.AccountId, newAccount.AccountId);
                    }
                    mappings.Add(dimTuple);
                }
            }

            return mappings;
        }

        private ActionResult LogCopyError(string type, string pk, int id, string codeOrNr, string name, int templateCompanyId, int newCompanyId, Exception ex = null, bool add = false, bool update = false)
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

        private ActionResult LogCopyError(string type, int templateCompanyId, int newCompanyId, Exception ex = null, bool saved = false)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Error while copying {type}. ");
            if (saved)
                message.Append("Could not save");

            return LogCopyError(message.ToString(), (int)ActionResultSave.Unknown, templateCompanyId, newCompanyId, ex);
        }

        private ActionResult LogCopyError(string message, int errorNumber, int templateCompanyId, int newCompanyId, Exception ex = null)
        {
            base.LogError(new SoeCopyCompanyException(message, templateCompanyId, newCompanyId, ex, this.ToString()), this.log);

            return new ActionResult(ex)
            {
                ErrorNumber = errorNumber,
                ErrorMessage = message,
            };
        }

        #endregion

        #endregion    
    }
}
