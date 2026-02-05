using Newtonsoft.Json.Linq;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Business.Util.WebApiInternal.Template;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    /// <summary>
    /// Temporary solution that this layer is partial managerbase and belongs to core.
    /// Should be a layer between business and the cache implemented (memory atm)
    /// </summary>

    public abstract partial class ManagerBase
    {
        private T GetFromCache<T>(string key, CacheConfig config = null)
        {
            var value = BusinessMemoryCache<T>.Get(key, config?.BusinessMemoryDistributionSetting ?? BusinessMemoryDistributionSetting.Disabled);
            if ((config?.KeepAlive ?? false) && value != null)
            {
                this.DeleteFromCache<T>(key, config);
                this.AddToCache<T>(key, value, config);
            }

            return value;
        }

        private T GetFromEvoCache<T>(string key, CacheConfig config = null)
        {
            try
            {
                var value = EvoDistributionCacheConnector.GetCachedValue<T>(key);

                if (value == null)
                    return default;

                return value;
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "Failed to get from Evo cache");
                return default;
            }
        }

        private void AddToCache<T>(string key, T value, CacheConfig config)
        {
            if (config != null && !config.DiscardCache)
                BusinessMemoryCache<T>.Set(key, value, config.Seconds, config.BusinessMemoryDistributionSetting);
        }

        private void AddToEvoCache<T>(string key, T value, int seconds)
        {
            try
            {
                EvoDistributionCacheConnector.UpsertCachedValue(key, value, TimeSpan.FromSeconds(seconds));
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "Failed to add to Evo cache");
            }
        }

        private void DeleteFromCache<T>(string key, CacheConfig config = null)
        {
            BusinessMemoryCache<T>.Delete(key, config?.BusinessMemoryDistributionSetting ?? BusinessMemoryDistributionSetting.Disabled);
        }

        #region Get

        #region Template

        protected CoreTemplateConnector coreTemplateConnector = new CoreTemplateConnector();
        protected EconomyTemplateConnector economyTemplateConnector = new EconomyTemplateConnector();
        protected AttestTemplateConnector attestTemplateConnector = new AttestTemplateConnector();
        protected TimeTemplateConnector timeTemplateConnector = new TimeTemplateConnector();
        protected BillingTemplateConnector billingTemplateConnector = new BillingTemplateConnector();

        #endregion

        #region Accounts

        public List<AccountDimDTO> GetAccountDimsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.AccountDim);

            List<AccountDimDTO> accountDims = GetFromCache<List<AccountDimDTO>>(key, config);
            if (accountDims == null)
            {
                accountDims = AccountManager.GetAccountDimsByCompany(entities, config.ActorCompanyId, loadParentOrCalculateLevels: true).ToDTOs();
                AddToCache(key, accountDims, config);
            }

            return accountDims?.CloneDTOs();
        }

        protected List<AccountDTO> GetAccountInternalsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.AccountInternal);

            List<AccountDTO> accountInternals = GetFromCache<List<AccountDTO>>(key);
            if (accountInternals == null)
            {
                accountInternals = AccountManager.GetAccountsInternalsByCompany(entities, config.ActorCompanyId, includeInactive: true, loadAccountDim: true).ToDTOs(includeAccountDim: true);
                AddToCache(key, accountInternals, config);
            }

            var clones = accountInternals?.CloneDTOs();

            if (!this.IncludeInactiveAccounts)
                clones = clones.Where(a => a.State == (int)SoeEntityState.Active).ToList();

            return clones;
        }

        protected Dictionary<string, string> GetAccountHierarchyStringsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null || !config.UserId.HasValue)
                return new Dictionary<string, string>();

            string key = config.GetCacheKey((int)BusinessCacheType.AccountHierarchyStrings);

            Dictionary<string, string> accountHierarchyStrings = GetFromCache<Dictionary<string, string>>(key);
            if (accountHierarchyStrings == null)
            {
                AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector);
                AccountRepository accountRepository = AccountManager.GetAccountHierarchyRepositoryByUser(entities, config.ActorCompanyId, config.UserId.Value, null, null, input: input);
                accountHierarchyStrings = accountRepository?.GetAccountStrings() ?? new Dictionary<string, string>();
                AddToCache(key, accountHierarchyStrings, config);
            }

            return accountHierarchyStrings;

        }

        #endregion

        #region AttestState

        protected List<AttestState> GetAttestStatesForTimeFromCache(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAttestStatesForTimeFromCache(entities, CacheConfig.Company(actorCompanyId));
        }

        protected List<AttestState> GetAttestStatesForTimeFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.AttestStatesTime);

            List<AttestState> attestStates = GetFromCache<List<AttestState>>(key);
            if (attestStates == null)
            {
                attestStates = AttestManager.GetAttestStates(entities, config.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                AddToCache(key, attestStates, config);
            }

            return attestStates;
        }

        #endregion

        #region Bridge

        protected bool HasEventActivatedScheduledJob(CompEntities entities, int actorCompanyId, TermGroup_ScheduleJobEventActivationType scheduleJobEventActivationType)
        {
            try
            {
                CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 5);
                string key = config.GetCacheKey((int)BusinessCacheType.HasEventActivatedScheduledJob, scheduleJobEventActivationType.ToString());
                bool? hasEventActivatedScheduledJob = GetFromCache<bool?>(key);

                if (!hasEventActivatedScheduledJob.HasValue)
                {
                    hasEventActivatedScheduledJob = ScheduledJobManager.GetScheduledJobSettingsWithEventActivaction(entities, actorCompanyId).Any(a => a.IntData.HasValue && a.IntData == (int)scheduleJobEventActivationType);
                    if (hasEventActivatedScheduledJob.Value)
                        config = CacheConfig.Company(actorCompanyId, 60 * 60);
                    AddToCache(key, hasEventActivatedScheduledJob.Value, config);
                }

                return hasEventActivatedScheduledJob.Value;

            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Categories

        protected List<CategoryAccount> GetCategoryAccountsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.CategoryAccount);

            List<CategoryAccount> categoryAccounts = GetFromCache<List<CategoryAccount>>(key);
            if (categoryAccounts == null)
            {
                categoryAccounts = CategoryManager.GetCategoryAccountsByCompany(entities, config.ActorCompanyId).ToList();
                AddToCache(key, categoryAccounts, config);
            }

            return categoryAccounts;
        }

        protected List<CompanyCategoryRecord> GetCompanyCategoryRecordsFromCache(CompEntities entities, CacheConfig config, bool onlyEntityEmployee = false, List<int> categoryIds = null)
        {
            if (config == null)
                return new List<CompanyCategoryRecord>();

            string key = config.GetCacheKey((int)BusinessCacheType.CategoryRecords);

            List<CompanyCategoryRecord> companyCategoryRecords = GetFromCache<List<CompanyCategoryRecord>>(key);
            if (companyCategoryRecords == null)
            {
                companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, config.ActorCompanyId);
                AddToCache(key, companyCategoryRecords, config);
            }

            //Post cache filter
            if (categoryIds != null)
                companyCategoryRecords = companyCategoryRecords.Where(i => categoryIds.Contains(i.CategoryId)).ToList();
            if (onlyEntityEmployee)
                companyCategoryRecords = companyCategoryRecords.Where(i => i.Entity == (int)SoeCategoryRecordEntity.Employee).ToList();

            return companyCategoryRecords;
        }

        #endregion

        #region Core

        protected bool IsLicense100(CompEntities entities = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetLicenseFromCache(entities ?? entitiesReadOnly, LicenseId)?.LicenseNr == "100";
        }
        protected bool IsMartinServera(CompEntities entities = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetLicenseFromCache(entities ?? entitiesReadOnly, LicenseId)?.LicenseNr?.StartsWith("500") ?? false;
        }

        protected bool IsAxfood(CompEntities entities = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetLicenseFromCache(entities ?? entitiesReadOnly, LicenseId)?.LicenseNr?.StartsWith("80") ?? false;
        }
        protected bool IsTele2(int actorCompanyId) => actorCompanyId == 170244;


        public List<Company> GetCompaniesByLicenseFromCache(int licenseId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetCompaniesByLicenseFromCache(entitiesReadOnly, licenseId);
        }
        public List<Company> GetCompaniesByLicenseFromCache(CompEntities entities, int licenseId)
        {
            CacheConfig config = CacheConfig.License(licenseId);
            string key = config.GetCacheKey((int)BusinessCacheType.CompaniesOnLicense);

            List<Company> companiesFromCache = GetFromCache<List<Company>>(key);
            if (companiesFromCache == null)
            {
                companiesFromCache = CompanyManager.GetCompaniesByLicense(entities, licenseId);
                AddToCache(key, companiesFromCache, config);
            }

            return companiesFromCache;
        }


        public Company GetCompanyFromCache(int actorCompany) { using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetCompanyFromCache(entitiesReadOnly, actorCompany); }
        public Company GetCompanyFromCache(CompEntities entities, int actorCompany)
        {
            CacheConfig config = CacheConfig.Company(actorCompany);
            string key = config.GetCacheKey((int)BusinessCacheType.Company, actorCompany.ToString());

            Company companyFromCache = GetFromCache<Company>(key);
            if (companyFromCache == null)
            {
                companyFromCache = CompanyManager.GetCompany(entities, actorCompany);
                AddToCache(key, companyFromCache, config);
            }

            return companyFromCache;
        }

        public License GetLicenseFromCache(int licenseId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetLicenseFromCache(entitiesReadOnly, licenseId);
        }
        public License GetLicenseFromCache(CompEntities entities, int licenseId)
        {
            if (licenseId == 0)
                return null;

            CacheConfig config = CacheConfig.License(licenseId);
            string key = config.GetCacheKey((int)BusinessCacheType.LicenseById, licenseId.ToString());

            License license = GetFromCache<License>(key);
            if (license == null)
            {
                license = LicenseManager.GetLicense(entities, licenseId);
                AddToCache(key, license, config);
            }

            return license;
        }


        public License GetLicenseByNrFromCache(string licenseNr) { using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetLicenseByNrFromCache(entitiesReadOnly, licenseNr); }
        public License GetLicenseByNrFromCache(CompEntities entities, string licenseNr)
        {
            if (string.IsNullOrEmpty(licenseNr) || (int.TryParse(licenseNr, out int licenseNrAsInt) && licenseNrAsInt == 0))
                return null;

            CacheConfig config = CacheConfig.License(int.Parse(licenseNr));
            string key = config.GetCacheKey((int)BusinessCacheType.LicenseByNr, licenseNr);

            License license = GetFromCache<License>(key);
            if (license == null)
            {
                license = LicenseManager.GetLicenseByNr(entities, licenseNr);
                AddToCache(key, license, config);
            }

            return license;
        }


        protected int GetCompanySysCountryIdFromCache(int actorCompanyId, TermGroup_Languages defaultCountry = TermGroup_Languages.Swedish) { using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext(); return GetCompanySysCountryIdFromCache(entitiesReadOnly, actorCompanyId, defaultCountry); }
        protected int GetCompanySysCountryIdFromCache(CompEntities entities, int actorCompanyId, TermGroup_Languages defaultCountry = TermGroup_Languages.Swedish)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 240);
            string key = config.GetCacheKey((int)BusinessCacheType.CompanySysCountryId, defaultCountry.ToString());

            int? sysCountryId = GetFromCache<int?>(key);
            if (!sysCountryId.HasValue)
            {
                sysCountryId = CompanyManager.GetCompanySysCountryId(entities, actorCompanyId, defaultCountry);
                AddToCache(key, sysCountryId, config);
            }

            return sysCountryId.Value;
        }

        #endregion

        #region DayType

        protected List<DayType> GetDayTypesFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.DayType);

            List<DayType> dayTypes = GetFromCache<List<DayType>>(key);
            if (dayTypes == null)
            {
                dayTypes = CalendarManager.GetDayTypesByCompany(entities, config.ActorCompanyId);
                AddToCache(key, dayTypes, config);
            }

            return dayTypes;
        }

        #endregion

        #region ExtraFields

        protected List<ExtraFieldDTO> GetExtraFieldsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.ExtraField);

            List<ExtraFieldDTO> extraFields = GetFromCache<List<ExtraFieldDTO>>(key);
            if (extraFields == null)
            {
                extraFields = ExtraFieldManager.GetExtraFields(entities, config.ActorCompanyId, false, true).ToDTOs().ToList();
                AddToCache(key, extraFields, config);
            }

            return extraFields?.CloneDTOs();
        }

        #endregion

        #region Employee/Employment

        public bool UseAccountHierarchyOnCompanyFromCache(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);
        }

        public bool UseAccountHierarchyOnCompanyFromCache(CompEntities entities, int actorCompanyId)
        {
            if (actorCompanyId == 0)
                return false;

            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 5);
            string key = config.GetCacheKey((int)BusinessCacheType.UseAccountHierarchy);

            bool? useAccountHierarchy = GetFromCache<bool?>(key);
            if (!useAccountHierarchy.HasValue)
            {
                useAccountHierarchy = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, actorCompanyId, 0);
                if (useAccountHierarchy.Value)
                    config = CacheConfig.Company(actorCompanyId, 60 * 10);

                AddToCache(key, useAccountHierarchy.Value, config);
            }

            return useAccountHierarchy.Value;
        }

        protected List<int> GetEmployeeIdsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<int>();

            string key = config.GetCacheKey((int)BusinessCacheType.AllEmployees);

            List<int> employeeIds = GetFromCache<List<int>>(key);
            if (employeeIds == null)
            {
                employeeIds = EmployeeManager.GetAllEmployeeIds(entities, config.ActorCompanyId).ToList();
                AddToCache(key, employeeIds, config);
            }

            return employeeIds;
        }

        protected List<Employee> GetEmployeesWithRepositoryFromCache(
            CompEntities entities,
            CacheConfig config,
            out EmployeeAuthModelRepository repository,
            int userId,
            int roleId,
            DateTime startDate,
            DateTime stopDate,
            List<int> employeeFilter,
            bool includeEnded = false,
            bool discardLimitSettingForEnded = false,
            bool? onlyDefaultEmployeeAuthModel = null,
            bool useDefaultEmployeeAccountDimEmployee = false,
            string searchPattern = ""
            )
        {
            repository = null;

            if (config == null)
                return new List<Employee>();

            string key = config.GetCacheKey((int)BusinessCacheType.EmployeesWithRepository);
            bool hasSettings = includeEnded || onlyDefaultEmployeeAuthModel.HasValue || !String.IsNullOrEmpty(searchPattern) || useDefaultEmployeeAccountDimEmployee;

            List<Employee> employees = null;
            EmployeesRepositoryOutput employeesRepositoryOutput = !hasSettings ? GetFromCache<EmployeesRepositoryOutput>(key, config) : null;
            if (employeesRepositoryOutput == null)
            {
                employees = EmployeeManager.GetEmployeesForUsersAttestRoles(
                    entities,
                    out repository,
                    config.ActorCompanyId,
                    userId,
                    roleId,
                    dateFrom: startDate,
                    dateTo: stopDate,
                    employeeFilter: employeeFilter,
                    employeeSearchPattern: searchPattern,
                    getVacant: false,
                    includeEnded: includeEnded,
                    discardLimitSettingForEnded: discardLimitSettingForEnded,
                    onlyDefaultEmployeeAuthModel: onlyDefaultEmployeeAuthModel,
                    useDefaultEmployeeAccountDimEmployee: useDefaultEmployeeAccountDimEmployee
                    );
                employeesRepositoryOutput = new EmployeesRepositoryOutput(employees, repository);
                AddToCache(key, employeesRepositoryOutput, config);
            }
            else
            {
                repository = employeesRepositoryOutput.EmployeeAuthModelRepository;
                employees = employeesRepositoryOutput.Employees ?? new List<Employee>();
                if (employeeFilter != null)
                    employees = employeesRepositoryOutput.Employees.Where(i => employeeFilter.Contains(i.EmployeeId)).ToList();
            }

            return employees;
        }

        protected List<EmployeeAccount> GetEmployeeAccountsFromCache(CompEntities entities, CacheConfig config, List<int> employeeIds = null, List<int> accountIds = null)
        {
            if (config == null)
                return new List<EmployeeAccount>();

            string extraKey = "NoKey";
            string key = config.GetCacheKey((int)BusinessCacheType.EmployeeAccounts, extraKey);

            List<EmployeeAccount> employeeAccounts = GetFromCache<List<EmployeeAccount>>(key);
            if (employeeAccounts == null)
            {
                //Get from db and add to cache
                employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, config.ActorCompanyId);
                AddToCache(key, employeeAccounts, config);
            }

            var filtered = employeeAccounts ?? new List<EmployeeAccount>();

            //Post cache filter
            if (employeeIds != null)
                filtered = filtered.Where(i => employeeIds.Contains(i.EmployeeId)).ToList();
            if (accountIds != null)
                filtered = filtered.Where(i => i.AccountId.HasValue && accountIds.Contains(i.AccountId.Value)).ToList();


            return filtered;
        }

        protected int GetHiddenEmployeeIdFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return 0;

            string key = config.GetCacheKey((int)BusinessCacheType.HiddenEmployeeId);

            int? employeeId = GetFromCache<int?>(key);
            if (employeeId == null)
            {
                employeeId = EmployeeManager.GetHiddenEmployeeId(entities, config.ActorCompanyId);
                AddToCache(key, employeeId, config);
            }

            return employeeId.Value;
        }

        protected int GetAccountDimIdOnAccountFromCache(CompEntities entities, int actorCompanyId, int accountId)
        {
            var config = CacheConfig.Company(actorCompanyId, accountId);
            string key = config.GetCacheKey((int)BusinessCacheType.AccountDimIdOnAccount, accountId.ToString());

            int? accountDimId = GetFromCache<int?>(key);
            if (accountDimId == null)
            {
                var accountInternals = GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));
                AccountDTO account = accountInternals.FirstOrDefault(a => a.AccountId == accountId) ?? AccountManager.GetAccount(entities, actorCompanyId, accountId, false).ToDTO();
                if (account != null)
                {
                    accountDimId = account.AccountDimId;
                    AddToCache(key, accountDimId, CacheConfig.Company(actorCompanyId, 10000));

                }
            }

            return accountDimId ?? 0;
        }

        protected List<Employment> GetEmploymentsFromCache(CompEntities entities, CacheConfig config, List<int> employeeIds = null)
        {
            if (config == null)
                return new List<Employment>();

            string key = config.GetCacheKey((int)BusinessCacheType.Employments);

            List<Employment> employments = GetFromCache<List<Employment>>(key);
            if (employments == null)
            {
                employments = EmployeeManager.GetEmployments(entities, config.ActorCompanyId);
                AddToCache(key, employments, config);
            }

            //Post cache filter
            if (employeeIds != null)
                employments = employments.Where(i => employeeIds.Contains(i.EmployeeId)).ToList();

            return employments;
        }

        protected List<EmploymentPriceTypeDTO> GetEmploymentPriceTypesDTOsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<EmploymentPriceTypeDTO>();

            string key = config.GetCacheKey((int)BusinessCacheType.EmploymentPriceTypeDTOs);
            List<EmploymentPriceTypeDTO> employmentPriceTypes = GetFromCache<List<EmploymentPriceTypeDTO>>(key);

            if (employmentPriceTypes == null)
            {
                var employmentPriceTypesEntities = GetEmploymentPriceTypeFromCache(entities, CacheConfig.Company(config.ActorCompanyId));
                employmentPriceTypes = employmentPriceTypesEntities.ToDTOs(true, false).ToList();
                AddToCache(key, employmentPriceTypes, config);
            }

            return employmentPriceTypes;
        }

        protected List<EmploymentPriceType> GetEmploymentPriceTypeFromCache(CompEntities entities, CacheConfig config, List<int> employeeIds = null)
        {
            if (config == null)
                return new List<EmploymentPriceType>();

            string extraKey = string.Empty;
            if (!employeeIds.IsNullOrEmpty())
                extraKey = CryptographyUtility.GetMd5Hash(string.Join("#", employeeIds));

            string key = config.GetCacheKey((int)BusinessCacheType.EmploymentPriceTypes, extraKey);
            List<EmploymentPriceType> employmentPriceTypes = GetFromCache<List<EmploymentPriceType>>(key);

            if (employmentPriceTypes == null)
            {
                employmentPriceTypes = EmployeeManager.GetEmploymentPriceTypesForCompany(entities, config.ActorCompanyId, employeeIds);
                AddToCache(key, employmentPriceTypes, config);
            }

            //Post cache filter
            if (employeeIds != null)
                employmentPriceTypes = employmentPriceTypes.Where(i => employeeIds.Contains(i.Employment.EmployeeId)).ToList();

            return employmentPriceTypes;
        }

        protected List<EmploymentTypeDTO> GetEmploymentTypesFromCache(CompEntities entities, CacheConfig config, TermGroup_Languages language)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.EmploymentType, ((int)language).ToString());

            List<EmploymentTypeDTO> employmentTypes = GetFromCache<List<EmploymentTypeDTO>>(key);
            if (employmentTypes == null)
            {
                employmentTypes = EmployeeManager.GetEmploymentTypesFromDB(entities, config.ActorCompanyId, language, true).ToList();
                AddToCache(key, employmentTypes, config);
            }

            return employmentTypes;
        }

        protected List<EmployeeFactor> GetEmployeeFactorsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.EmployeeFactors);

            List<EmployeeFactor> employmentTypes = GetFromCache<List<EmployeeFactor>>(key);
            if (employmentTypes == null)
            {
                employmentTypes = EmployeeManager.GetEmployeesFactorsForCompany(entities, config.ActorCompanyId).ToList();
                AddToCache(key, employmentTypes, config);
            }

            return employmentTypes;
        }

        public List<Employer> GetEmployersFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.Employers);
            List<Employer> employers = GetFromCache<List<Employer>>(key);
            if (employers == null)
            {
                employers = EmployeeManager.GetEmployers(entities, config.ActorCompanyId).ToList();
                AddToCache(key, employers, config);
            }
            return employers;
        }

        public List<EmployeeEmployer> GetEmployeeEmployersForCompanyFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.EmployeeEmployers);
            List<EmployeeEmployer> employeeEmployers = GetFromCache<List<EmployeeEmployer>>(key);
            if (employeeEmployers == null)
            {
                employeeEmployers = EmployeeManager.GetEmployeeEmployersForCompany(entities, config.ActorCompanyId).ToList();
                AddToCache(key, employeeEmployers, config);
            }
            return employeeEmployers;
        }
        protected List<PositionDTO> GetPositionsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.Positions);

            List<PositionDTO> positions = GetFromCache<List<PositionDTO>>(key);
            if (positions == null)
            {
                positions = EmployeeManager.GetPositions(entities, config.ActorCompanyId).ToDTOs().ToList();
                AddToCache(key, positions, config);
            }

            return positions;
        }

        protected List<EndReasonDTO> GetEndReasonsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.EndReason);

            List<EndReasonDTO> endReasons = GetFromCache<List<EndReasonDTO>>(key);
            if (endReasons == null)
            {
                endReasons = EmployeeManager.GetEndReasons(entities, config.ActorCompanyId);
                AddToCache(key, endReasons, config);
            }

            return endReasons;
        }

        protected bool HasEmployeeTemplatesFromCache(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 2);
            string key = config.GetCacheKey((int)BusinessCacheType.HasEmployeeTemplates);
            bool? hasEmployeeTemplates = GetFromCache<bool?>(key);

            if (!hasEmployeeTemplates.HasValue)
            {
                hasEmployeeTemplates = entities.EmployeeTemplate.Any(w => w.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active);
                if (hasEmployeeTemplates.Value)
                    config = CacheConfig.Company(actorCompanyId, 60 * 60);
                AddToCache(key, hasEmployeeTemplates.Value, config);
            }

            return hasEmployeeTemplates.Value;
        }



        #endregion

        #region EmployeeGroup/PayrollGroup/VacationGroup

        public List<EmployeeGroup> GetEmployeeGroupsFromCache(int actorCompanyId, bool loadExternalCode = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var result = GetEmployeeGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId), loadExternalCode);
            return result.IsNullOrEmpty() ? null : result;
        }

        protected List<EmployeeGroup> GetEmployeeGroupsFromCache(CompEntities entities, CacheConfig config, bool loadExternalCode = false)
        {
            if (config == null)
                return new List<EmployeeGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.EmployeeGroups, $"{loadExternalCode}");

            List<EmployeeGroup> employeeGroups = GetFromCache<List<EmployeeGroup>>(key);
            if (employeeGroups == null)
            {
                employeeGroups = EmployeeManager.GetEmployeeGroups(entities, config.ActorCompanyId, loadExternalCode: loadExternalCode);
                AddToCache(key, employeeGroups, config);
            }
            else if (loadExternalCode)
            {
                EmployeeManager.LoadEmployeeGroupExternalCodes(entities, employeeGroups, config.ActorCompanyId);
            }

            return employeeGroups;
        }

        protected List<EmployeeGroup> GetEmployeeGroupsWithDayTypesFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<EmployeeGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.EmployeeGroupsWithDayTypes);

            List<EmployeeGroup> employeeGroups = GetFromCache<List<EmployeeGroup>>(key);
            if (employeeGroups == null)
            {
                employeeGroups = EmployeeManager.GetEmployeeGroups(entities, config.ActorCompanyId, loadDayTypes: true);
                AddToCache(key, employeeGroups, config);
            }

            return employeeGroups;
        }

        protected List<EmployeeGroup> GetEmployeeGroupsForPersonalDataRepoFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<EmployeeGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.PersonalDataRepoEmployeeGroups);

            List<EmployeeGroup> employeeGroups = GetFromCache<List<EmployeeGroup>>(key);
            if (employeeGroups == null)
            {
                employeeGroups = EmployeeManager.GetEmployeeGroups(entities, config.ActorCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true, loadAttestTransitions: true);
                AddToCache(key, employeeGroups, config);
            }

            return employeeGroups;
        }

        protected List<PayrollGroupAccountStd> GetPayrollGroupAccountStdFromCache(CompEntities entities, int actorCompanyId)
        {
            return GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId), loadAccountStd: true)?.SelectMany(s => s.PayrollGroupAccountStd).ToList();
        }

        protected List<PayrollGroup> GetPayrollGroupsFromCache(int actorCompanyId, bool loadExternalCode = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var result = GetPayrollGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId), loadExternalCode);
            return result.IsNullOrEmpty() ? null : result;
        }

        protected List<PayrollGroup> GetPayrollGroupsFromCache(CompEntities entities, CacheConfig config, bool loadExternalCode = false, bool loadSettings = true, bool loadAccountStd = true)
        {
            if (config == null)
                return new List<PayrollGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollGroups, $"{loadExternalCode}#{loadSettings}#{loadAccountStd}");

            List<PayrollGroup> payrollGroups = GetFromCache<List<PayrollGroup>>(key);
            if (payrollGroups == null)
            {
                payrollGroups = PayrollManager.GetPayrollGroups(entities, config.ActorCompanyId, loadExternalCode: loadExternalCode, loadSettings: loadSettings, loadAccountStd: loadAccountStd);
                AddToCache(key, payrollGroups, config);
            }
            else if (loadExternalCode)
            {
                PayrollManager.LoadPayrollGroupExternalCodes(entities, payrollGroups, config.ActorCompanyId);
            }

            return payrollGroups;
        }

        protected List<PayrollGroup> GetPayrollGroupsForPersonalDataRepoFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.PersonalDataRepoPayrollGroups);

            List<PayrollGroup> payrollGroups = GetFromCache<List<PayrollGroup>>(key);
            if (payrollGroups == null)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

                payrollGroups = PayrollManager.GetPayrollGroups(entities ?? entitiesReadOnly, config.ActorCompanyId, loadSettings: true);
                AddToCache(key, payrollGroups, config);
            }

            return payrollGroups;
        }

        protected List<VacationGroup> GetVacationGroupsFromCache(int actorCompanyId, bool loadExternalCode = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var result = GetVacationGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId), loadExternalCode);
            return result.IsNullOrEmpty() ? null : result;
        }

        protected List<VacationGroup> GetVacationGroupsFromCache(CompEntities entities, CacheConfig config, bool loadExternalCode = false, bool setTypeName = false)
        {
            if (config == null)
                return new List<VacationGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.VacationGroups);

            List<VacationGroup> vacationGroups = GetFromCache<List<VacationGroup>>(key);
            if (vacationGroups == null)
            {
                vacationGroups = PayrollManager.GetVacationGroups(config.ActorCompanyId, onlyActive: false, loadExternalCode: loadExternalCode);
                AddToCache(key, vacationGroups, config);
            }
            else if (loadExternalCode)
            {
                PayrollManager.LoadVacationGroupExternalCodes(entities, vacationGroups, config.ActorCompanyId);
            }

            if (setTypeName)
                PayrollManager.SetVacationGroupsTypeNames(vacationGroups);

            return vacationGroups;
        }

        protected List<AnnualLeaveGroup> GetAnnualLeaveGroupsFromCache(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var result = GetAnnualLeaveGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            return result.IsNullOrEmpty() ? null : result;
        }

        protected List<AnnualLeaveGroup> GetAnnualLeaveGroupsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<AnnualLeaveGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.AnnualLeaveGroups);

            List<AnnualLeaveGroup> annualLeaveGroups = GetFromCache<List<AnnualLeaveGroup>>(key);
            if (annualLeaveGroups == null)
            {
                annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(entities, config.ActorCompanyId);
                AddToCache(key, annualLeaveGroups, config);
            }

            return annualLeaveGroups;
        }

        protected List<AnnualLeaveGroup> GetAnnualLeaveGroupsForPersonalDataRepoFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<AnnualLeaveGroup>();

            string key = config.GetCacheKey((int)BusinessCacheType.PersonalDataRepoAnnualLeaveGroups);

            List<AnnualLeaveGroup> annualLeaveGroups = GetFromCache<List<AnnualLeaveGroup>>(key);
            if (annualLeaveGroups == null)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

                annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(entities ?? entitiesReadOnly, config.ActorCompanyId);
                AddToCache(key, annualLeaveGroups, config);
            }

            return annualLeaveGroups;
        }

        #endregion

        #region Schedule

        protected List<HolidayDTO> GetHolidaysFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.Holiday);
            List<HolidayDTO> holidays = GetFromCache<List<HolidayDTO>>(key, config);
            if (holidays == null)
            {
                holidays = CalendarManager.GetHolidaysByCompany(entities, config.ActorCompanyId);
                AddToCache(key, holidays, config);
            }

            return holidays;
        }

        protected List<HolidayDTO> GetHolidaySalaryHolidaysFromCache(CompEntities entities, DateTime dateFrom, DateTime dateTo, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.HolidaySalaryHoliday, dateFrom.ToShortDateString() + "#" + dateTo.ToShortDateString());

            List<HolidayDTO> holidays = GetFromCache<List<HolidayDTO>>(key);
            if (holidays == null)
            {
                holidays = CalendarManager.GetHolidaySalaryHolidays(entities, dateFrom, dateTo, config.ActorCompanyId);
                AddToCache(key, holidays, config);
            }

            return holidays;
        }

        protected List<TimeCodeBreak> GetTimeCodeBreaksFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.TimeCodeBreak);

            List<TimeCodeBreak> timeCodeBreaks = GetFromCache<List<TimeCodeBreak>>(key);
            if (timeCodeBreaks == null)
            {
                timeCodeBreaks = TimeCodeManager.GetTimeCodeBreaks(entities, config.ActorCompanyId);
                AddToCache(key, timeCodeBreaks, config);
            }

            return timeCodeBreaks;
        }

        protected List<TimeScheduleType> GetTimeScheduleTypesFromCache(CompEntities entities, CacheConfig config, bool loadFactors = false)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.TimeScheduleType);

            List<TimeScheduleType> timeScheduleTypes = GetFromCache<List<TimeScheduleType>>(key);
            if (loadFactors && timeScheduleTypes != null && timeScheduleTypes.Any(i => !i.TimeScheduleTypeFactor.IsLoaded))
                timeScheduleTypes = null;

            if (timeScheduleTypes == null)
            {
                timeScheduleTypes = TimeScheduleManager.GetTimeScheduleTypes(entities, config.ActorCompanyId, getAll: true, loadFactors: true);
                AddToCache(key, timeScheduleTypes, config);
            }

            return timeScheduleTypes;
        }

        protected List<ShiftType> GetShiftTypesFromCache(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetShiftTypesFromCache(entities, CacheConfig.Company(actorCompanyId));
        }

        protected List<ShiftType> GetShiftTypesFromCache(CompEntities entities, CacheConfig config, bool loadTimeScheduleTypes = false, bool loadSkills = false, bool loadAccounts = false, bool loadHierarchyAccounts = false)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.ShiftTypes);

            List<ShiftType> shiftTypes = GetFromCache<List<ShiftType>>(key);
            if (loadTimeScheduleTypes && shiftTypes != null && shiftTypes.Any(i => !i.TimeScheduleTypeReference.IsLoaded))
                shiftTypes = null;

            if (loadSkills && shiftTypes != null && shiftTypes.Any(i => !i.ShiftTypeSkill.IsLoaded))
                shiftTypes = null;

            if (loadAccounts && shiftTypes != null && (shiftTypes.Any(i => !i.AccountReference.IsLoaded) || shiftTypes.Any(i => !i.AccountInternal.IsLoaded)))
            {
                loadSkills = true;
                shiftTypes = null;
            }
            if (loadHierarchyAccounts && shiftTypes != null && shiftTypes.Any(i => !i.ShiftTypeHierarchyAccount.IsLoaded))
            {
                loadSkills = true;
                loadAccounts = true;
                shiftTypes = null;
            }

            if (shiftTypes == null)
            {
                shiftTypes = TimeScheduleManager.GetShiftTypes(entities, config.ActorCompanyId, loadTimeScheduleTypes: true, loadSkills: loadSkills, loadAccounts: loadAccounts, loadAccountInternals: loadAccounts, loadHierarchyAccounts: loadHierarchyAccounts);
                AddToCache(key, shiftTypes, config);
            }

            return shiftTypes;
        }

        protected AccountDimDTO GetShiftTypeAccountDimFromCache(CompEntities entities, int actorCompanyId, bool loadAccounts = false)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, loadAccounts ? 30 : 60 * 10);
            string key = config.GetCacheKey((int)BusinessCacheType.AccountDimShiftType, optionalKeyPart: loadAccounts.ToString());
            AccountDimDTO accountDim = GetFromCache<AccountDimDTO>(key, config);
            if (accountDim == null)
            {
                accountDim = AccountManager.GetShiftTypeAccountDim(entities, actorCompanyId, loadAccounts)?.ToDTO(loadAccounts, loadAccounts);

                if (accountDim != null)
                    AddToCache(key, accountDim, config);
                else
                {
                    accountDim = new AccountDimDTO() { AccountDimId = -1 };
                    AddToCache(key, accountDim, config);
                }
            }

            return accountDim.AccountDimId < 0 ? null : accountDim;
        }

        protected List<ShiftTypeLink> GetShiftTypeLinksFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.ShiftTypeLink);

            List<ShiftTypeLink> shiftTypeLinks = GetFromCache<List<ShiftTypeLink>>(key);

            if (shiftTypeLinks == null)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

                shiftTypeLinks = entitiesReadOnly.ShiftTypeLink.Where(s => s.ActorCompanyId == config.ActorCompanyId).ToList();
                AddToCache(key, shiftTypeLinks, config);
            }

            return shiftTypeLinks;
        }

        protected bool HasTimeScheduleTemplateGroupsFromCache(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId);
            string key = config.GetCacheKey((int)BusinessCacheType.HasTimeScheduletemplateGroups);
            bool? hasTimeScheduletemplateGroups = GetFromCache<bool?>(key);
            if (!hasTimeScheduletemplateGroups.HasValue)
            {
                hasTimeScheduletemplateGroups = TimeScheduleManager.GetTimeScheduleTemplateGroups(entities, config.ActorCompanyId, false, false).Any();
                AddToCache(key, hasTimeScheduletemplateGroups.Value, config);
            }

            return hasTimeScheduletemplateGroups.Value;
        }

        protected bool HasTimeValidRuleWorkTimeSettingsFromCache(CompEntities entities, int actorCompanyId, DateTime date)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60);
            string key = config.GetCacheKey((int)BusinessCacheType.HasTimeValidRuleWorkTimePeriodSettings);
            bool? hasTimeValidRuleWorkTimeSettings = GetFromCache<bool?>(key);

            if (!hasTimeValidRuleWorkTimeSettings.HasValue)
            {
                hasTimeValidRuleWorkTimeSettings = TimePeriodManager.CompanyHasWithValidRuleWorkTimeSettings(entities, config.ActorCompanyId, date);
                AddToCache(key, hasTimeValidRuleWorkTimeSettings.Value, config);
            }

            return hasTimeValidRuleWorkTimeSettings.Value;
        }

        protected bool HasCalculatePayrollOnChanges(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60);
            string key = config.GetCacheKey((int)BusinessCacheType.HasCalculatePayrollOnChanges);
            bool? hasCalculatePayrollOnChanges = GetFromCache<bool?>(key);

            if (!hasCalculatePayrollOnChanges.HasValue)
            {
                hasCalculatePayrollOnChanges = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.CalculatePayrollFromChanges, 0, actorCompanyId, 0);
                if (!hasCalculatePayrollOnChanges.Value)
                    config = CacheConfig.Company(actorCompanyId, 300);
                AddToCache(key, hasCalculatePayrollOnChanges.Value, config);
            }

            return hasCalculatePayrollOnChanges.Value;
        }

        protected bool HasPayrollImportHeadsFromCache(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 5);
            string key = config.GetCacheKey((int)BusinessCacheType.HasPayrollImportHeads);
            bool? hasPayrollImports = GetFromCache<bool?>(key);

            if (!hasPayrollImports.HasValue)
            {
                hasPayrollImports = entities.PayrollImportHead.Any(w => w.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active);
                AddToCache(key, hasPayrollImports.Value, config);
            }

            return hasPayrollImports.Value;
        }

        protected bool HasTimeLeisureCodesFromCache(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 5);
            string key = config.GetCacheKey((int)BusinessCacheType.HasTimeLeisureCodes);
            bool? hasTimeLeisureCodes = GetFromCache<bool?>(key);
            if (!hasTimeLeisureCodes.HasValue)
            {
                hasTimeLeisureCodes = entities.TimeLeisureCode.Any(w => w.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active);
                AddToCache(key, hasTimeLeisureCodes.Value, config);
            }
            return hasTimeLeisureCodes.Value;
        }

        protected bool UsePayroll(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 10);
            string key = config.GetCacheKey((int)BusinessCacheType.UsePayroll);
            bool? usepayroll = GetFromCache<bool?>(key, config);

            if (!usepayroll.HasValue)
            {
                usepayroll = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, actorCompanyId, 0);

                if (usepayroll.Value)
                    config = CacheConfig.Company(actorCompanyId, 60 * 60);

                AddToCache(key, usepayroll.Value, config);
            }

            return usepayroll.Value;
        }

        protected bool UsesWeekendSalaryFromCache(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 5);
            string key = config.GetCacheKey((int)BusinessCacheType.UsesWeekendSalary);
            bool? usesWeekendSalary = GetFromCache<bool?>(key, config);

            if (!usesWeekendSalary.HasValue)
            {
                bool hasDaytypes = (from dt in entities.DayType
                                    where dt.ActorCompanyId == actorCompanyId &&
                                    dt.State == (int)SoeEntityState.Active
                                    select dt).Any(x => x.WeekendSalary);

                bool hasProduct = (from pp in entities.Product.OfType<PayrollProduct>()
                                   where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                                    (pp.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary) &&
                                    (pp.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_WeekendSalary) &&
                                    (pp.State == (int)SoeEntityState.Active)
                                   select pp).Any();

                usesWeekendSalary = hasDaytypes && hasProduct;


                AddToCache(key, usesWeekendSalary.Value, config);
            }

            return usesWeekendSalary.Value;
        }

        #endregion

        #region StaffingFrequency

        protected List<StaffingNeedsFrequency> GetStaffingNeedsFrequencyFromCache(CompEntities entities, int actorCompanyId, DateTime startDate, DateTime stopDate, List<int> validAccountIds = null)
        {
            try
            {
                return GetStaffingNeedsFrequency(entities, actorCompanyId, startDate, stopDate, validAccountIds);
            }

            catch (Exception ex)
            {
                LogError(ex, log);
                return new List<StaffingNeedsFrequency>();
            }
        }

        private List<StaffingNeedsFrequency> GetStaffingNeedsFrequency(CompEntities entities, int actorCompanyId, DateTime startDate, DateTime stopDate, List<int> validAccountIds = null, bool retry = false)
        {
            try
            {
                StaffingNeedsFrequency lastRow = entities.StaffingNeedsFrequency.Where(w => w.ActorCompanyId == actorCompanyId).OrderByDescending(o => o.StaffingNeedsFrequencyId).FirstOrDefault();
                int lastUpdate = lastRow != null ? lastRow.StaffingNeedsFrequencyId : 0;
                CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 60 * 3);
                List<StaffingNeedsFrequency> result = new List<StaffingNeedsFrequency>();
                List<int> years = new List<int>(CalendarUtility.GetDates(startDate, stopDate).Select(y => y.Year).Distinct());

                if (years.Count < 5)
                {
                    foreach (var year in years)
                    {
                        DateTime startInYear = new DateTime(year, 1, 1);
                        DateTime stopInYear = new DateTime(year, 12, 31);
                        string key = config.GetCacheKey((int)BusinessCacheType.StaffingNeedsFrequency, year.ToString() + "#" + lastUpdate.ToString());
                        List<StaffingNeedsFrequency> staffingNeedsFrequency = GetFromCache<List<StaffingNeedsFrequency>>(key);

                        if (staffingNeedsFrequency == null)
                        {
                            staffingNeedsFrequency = TimeScheduleManager.GetStaffingNeedsFrequencys(entities, config.ActorCompanyId, startInYear, CalendarUtility.GetEndOfDay(stopInYear));
                            AddToCache(key, staffingNeedsFrequency, config);
                        }

                        result.AddRange(staffingNeedsFrequency);
                    }
                }
                else
                    result = TimeScheduleManager.GetStaffingNeedsFrequencys(config.ActorCompanyId, startDate, stopDate);

                return TimeScheduleManager.GetStaffingNeedsFrequencys(entities, config.ActorCompanyId, startDate, stopDate, result, validAccountIds: validAccountIds);
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                if (!retry)
                {
                    var existingTimeout = entities.CommandTimeout;
                    entities.CommandTimeout = 300;
                    var result = GetStaffingNeedsFrequency(entities, actorCompanyId, startDate, stopDate, validAccountIds, true);
                    entities.CommandTimeout = existingTimeout;
                    return result;

                }
                return new List<StaffingNeedsFrequency>();
            }
        }

        #endregion

        #region StaffingNeeds

        public bool HasStaffingNeedsFromCache(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 10);
            string key = config.GetCacheKey((int)BusinessCacheType.HasStaffingNeedsSetting);
            bool? hasStaffingNeeds = GetFromCache<bool?>(key, config);

            if (!hasStaffingNeeds.HasValue)
            {
                hasStaffingNeeds = !GetTimeScheduleTasksFromCache(entities, actorCompanyId, DateTime.Today.AddYears(-10), DateTime.Today.AddYears(10)).IsNullOrEmpty() || !GetIncomingDeliveryHeadsFromCache(entities, actorCompanyId, DateTime.Today.AddYears(-10), DateTime.Today.AddYears(10)).IsNullOrEmpty();
                AddToCache(key, hasStaffingNeeds, config);
            }

            return hasStaffingNeeds.Value;
        }

        public List<TimeScheduleTask> GetTimeScheduleTasksFromCache(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo, bool setRecurringDescription = false, bool doIncludeRemovedDates = false)
        {
            var result = GetTimeScheduleTasksFromCache(entities, CacheConfig.Company(actorCompanyId), dateFrom, dateTo, loadShiftType: false, setRecurringDescription, doIncludeRemovedDates: false);
            return result;
        }

        protected List<TimeScheduleTask> GetTimeScheduleTasksFromCache(CompEntities entities, CacheConfig config, DateTime dateFrom, DateTime dateTo, bool loadShiftType = false, bool setRecurringDescription = false, bool doIncludeRemovedDates = false)
        {
            if (config == null)
                return new List<TimeScheduleTask>();

            string key = config.GetCacheKey((int)BusinessCacheType.TimeScheduleTasks, $"{loadShiftType}#{setRecurringDescription}#{doIncludeRemovedDates}");

            List<TimeScheduleTask> timeScheduleTasks = GetFromCache<List<TimeScheduleTask>>(key);
            if (timeScheduleTasks == null)
            {
                timeScheduleTasks = TimeScheduleManager.GetTimeScheduleTasks(config.ActorCompanyId, DateTime.Today.AddYears(-10), DateTime.Today.AddYears(10), loadShiftType: loadShiftType, loadTimeScheduleTemplateBlockTask: false);
                AddToCache(key, timeScheduleTasks, config);
            }

            return TimeScheduleManager.GetValidatedTimeScheduleTasks(config.ActorCompanyId, timeScheduleTasks.ToList(), dateFrom, dateTo, setRecurringDescription, doIncludeRemovedDates);
        }


        public List<IncomingDeliveryHead> GetIncomingDeliveryHeadsFromCache(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo, bool loadShiftType = false, bool setRecurringDescription = false, bool doIncludeRemovedDates = false)
        {
            var result = GetIncomingDeliveryHeadsFromCache(entities, CacheConfig.Company(actorCompanyId), dateFrom, dateTo, loadShiftType: loadShiftType, setRecurringDescription: setRecurringDescription, doIncludeRemovedDates: doIncludeRemovedDates);
            return result;
        }

        protected List<IncomingDeliveryHead> GetIncomingDeliveryHeadsFromCache(CompEntities entities, CacheConfig config, DateTime dateFrom, DateTime dateTo, bool loadShiftType = false, bool setRecurringDescription = false, bool doIncludeRemovedDates = false)
        {
            if (config == null)
                return new List<IncomingDeliveryHead>();

            string key = config.GetCacheKey((int)BusinessCacheType.IncomingDeliveryHeads, $"{loadShiftType}#{setRecurringDescription}#{doIncludeRemovedDates}");

            List<IncomingDeliveryHead> incomingDeliveryHeads = GetFromCache<List<IncomingDeliveryHead>>(key);
            if (incomingDeliveryHeads == null)
            {
                incomingDeliveryHeads = TimeScheduleManager.GetIncomingDeliveries(config.ActorCompanyId, DateTime.Today.AddYears(-10), DateTime.Today.AddYears(10), loadShiftType: loadShiftType, loadTimeScheduleTemplateBlockTask: false);
                AddToCache(key, incomingDeliveryHeads, config);
            }
            return TimeScheduleManager.GetValidatedInComingDeliveryHeads(config.ActorCompanyId, incomingDeliveryHeads.ToList(), dateFrom, dateTo, setRecurringDescription: setRecurringDescription, doIncludeRemovedDates: doIncludeRemovedDates);
        }

        #endregion

        #region Time

        protected List<TimeDeviationCause> GetTimeDeviationCausesFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.TimeDeviationCause);

            List<TimeDeviationCause> timeDeviationCauses = GetFromCache<List<TimeDeviationCause>>(key);
            if (timeDeviationCauses == null)
            {
                timeDeviationCauses = TimeDeviationCauseManager.GetTimeDeviationCauses(entities, config.ActorCompanyId);
                AddToCache(key, timeDeviationCauses, config);
            }

            return timeDeviationCauses;
        }

        protected List<TimeRule> GetTimeRulesFromCache(CompEntities entities, CacheConfig config, bool onlyFromCache = false)
        {
            if (config == null)
                return new List<TimeRule>();

            string key = config.GetCacheKey((int)BusinessCacheType.TimeRules);

            List<TimeRule> timeRules = GetFromCache<List<TimeRule>>(key);
            if (timeRules == null && !onlyFromCache)
            {
                timeRules = TimeRuleManager.GetAllTimeRulesRecursive(entities, config.ActorCompanyId);
                AddToCache(key, timeRules, config);
            }

            return timeRules;
        }

        protected List<TimePayrollTransactionTreeDTO> GetTimePayrollTransactionsForTreeFromCache(CompEntities entities, CacheConfig config, DateTime? startDate = null, DateTime? stopDate = null, TimePeriod timePeriod = null, List<int> employeeIds = null, bool onlyUseInPayroll = false, bool includeAccounting = false, bool flushCache = false)
        {
            if (config == null || (employeeIds != null && !employeeIds.Any()))
                return new List<TimePayrollTransactionTreeDTO>();

            string key = config.GetCacheKey((int)BusinessCacheType.TimePayrollTransactions);

            List<TimePayrollTransactionTreeDTO> transactionItems = !flushCache ? GetFromCache<List<TimePayrollTransactionTreeDTO>>(key) : null;
            if (transactionItems == null)
            {
                transactionItems = TimeTransactionManager.GetTimePayrollTransactionsForTree(entities, config.ActorCompanyId, startDate, stopDate, timePeriod, employeeIds, onlyUseInPayroll, includeAccounting);
                AddToCache(key, transactionItems, config);
            }

            return transactionItems;
        }

        protected List<EmployeeChild> GetEmployeeChildsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.EmployeeChild);

            List<EmployeeChild> employeeChilds = GetFromCache<List<EmployeeChild>>(key);
            if (employeeChilds == null)
            {
                employeeChilds = EmployeeManager.GetEmployeeChildsForCompnay(entities, config.ActorCompanyId);
                AddToCache(key, employeeChilds, config);
            }

            return employeeChilds;
        }

        protected List<AttestTransitionDTO> GetAttestTransitionsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.AttestTransitions);

            List<AttestTransitionDTO> attestTransitions = GetFromCache<List<AttestTransitionDTO>>(key);
            if (attestTransitions == null)
            {
                attestTransitions = AttestManager.GetAttestTransitions(entities, TermGroup_AttestEntity.Unknown, SoeModule.None, true, config.ActorCompanyId).ToDTOs(true).ToList();
                AddToCache(key, attestTransitions, config);
            }

            return attestTransitions;
        }

        protected List<AttestTransitionDTO> GetAttestTransitionsForEmployeeGroupFromCache(CompEntities entities, int employeeGroupId, TermGroup_AttestEntity entity, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.AttestTransitionsForEmployeeGroup);

            List<AttestTransitionDTO> attestTransitions = GetFromCache<List<AttestTransitionDTO>>(key);
            if (attestTransitions == null)
            {
                attestTransitions = AttestManager.GetAttestTransitionsForEmployeeGroup(entities, entity, employeeGroupId).ToDTOs(true).ToList();
                AddToCache(key, attestTransitions, config);
            }

            return attestTransitions;
        }

        protected List<TimeWorkAccount> GetTimeWorkAccountsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<TimeWorkAccount>();

            string key = config.GetCacheKey((int)BusinessCacheType.TimeWorkAccount);

            List<TimeWorkAccount> timeWorkAccounts = GetFromCache<List<TimeWorkAccount>>(key);
            if (timeWorkAccounts == null)
            {
                timeWorkAccounts = TimeWorkAccountManager.GetTimeWorkAccounts(entities);
                AddToCache(key, timeWorkAccounts, config);
            }

            return timeWorkAccounts;
        }

        protected AccountDim GetDefaultEmployeeAccountDimFromCache(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60);
            string key = config.GetCacheKey((int)BusinessCacheType.DefaultEmployeeAccountDim);
            AccountDim accountDim = GetFromCache<AccountDim>(key);
            if (accountDim == null)
            {
                accountDim = AccountManager.GetDefaultEmployeeAccountDim(entities, actorCompanyId);
                AddToCache(key, accountDim, config);
            }

            return accountDim;
        }

        #endregion

        #region Reports

        protected List<ReportRolePermissionDTO> GetReportRolePermissionsFromCache(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetReportRolePermissionsFromCache(entities, CacheConfig.Company(actorCompanyId, 60));
        }

        protected List<ReportRolePermissionDTO> GetReportRolePermissionsFromCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.ReportRolePermission);
            config.SetBusinessMemoryDistributionSetting(BusinessMemoryDistributionSetting.FullyHybridCache);

            List<ReportRolePermissionDTO> reportRolePermissions = GetFromCache<List<ReportRolePermissionDTO>>(key, config);
            if (reportRolePermissions == null)
            {
                reportRolePermissions = ReportManager.GetReportRolePermissionsByCompany(entities, config.ActorCompanyId).ToDTOs().ToList();
                AddToCache(key, reportRolePermissions, config);
            }

            return reportRolePermissions;
        }

        #endregion

        #region Roles

        protected List<AttestRole> GetTimeAttestRolesFromCache(CompEntities entities, CacheConfig config, bool includeInactive = false, bool loadExternalCode = false)
        {
            if (config == null)
                return new List<AttestRole>();

            string key = config.GetCacheKey((int)BusinessCacheType.AttestRole);

            List<AttestRole> attestRoles = GetFromCache<List<AttestRole>>(key);
            if (attestRoles == null)
            {
                attestRoles = AttestManager.GetAttestRoles(entities, config.ActorCompanyId, SoeModule.Time, includeInactive: true, loadAttestRoleUser: false, loadExternalCode: loadExternalCode);
                BusinessMemoryCache<List<AttestRole>>.Set(key, attestRoles, config.Seconds);
            }
            else if (loadExternalCode)
            {
                AttestManager.LoadAttestRoleExternalCodes(entities, attestRoles, config.ActorCompanyId);
            }

            if (!includeInactive)
                attestRoles = attestRoles.Where(ar => ar.State == (int)SoeEntityState.Active).ToList();

            return attestRoles;
        }

        protected List<AttestRoleUser> GetAttestRoleUsersFromCache(CompEntities entities, CacheConfig config, DateTime? startDate = null, DateTime? stopDate = null, SoeModule? module = SoeModule.Time, bool onlyWithAccountId = false, bool ignoreDates = false, bool onlyDefaultAccounts = true)
        {
            if (config == null)
                return new List<AttestRoleUser>();

            string key = config.GetCacheKey((int)BusinessCacheType.AttestRoleUser) + $"#{module}";

            List<AttestRoleUser> attestRoleUsers = GetFromCache<List<AttestRoleUser>>(key);
            if (attestRoleUsers == null)
            {
                attestRoleUsers = AttestManager.GetAttestRoleUsersByCompany(entities, config.ActorCompanyId, module);
                BusinessMemoryCache<List<AttestRoleUser>>.Set(key, attestRoleUsers, config.Seconds);
            }

            return attestRoleUsers.Filter(startDate, stopDate, onlyWithAccountId, ignoreDates, onlyDefaultAccounts);
        }


        protected List<Role> GetRolesFromCache(CompEntities entities, CacheConfig config, bool loadExternalCode = false)
        {
            if (config == null)
                return new List<Role>();

            string key = config.GetCacheKey((int)BusinessCacheType.UserRole);

            List<Role> userRoles = GetFromCache<List<Role>>(key);
            if (userRoles == null)
            {
                userRoles = RoleManager.GetRolesByCompany(entities, config.ActorCompanyId, loadExternalCode: loadExternalCode);
                BusinessMemoryCache<List<Role>>.Set(key, userRoles, config.Seconds);
            }
            else if (loadExternalCode)
            {
                RoleManager.LoadRoleExternalCodes(entities, userRoles, config.ActorCompanyId);
            }

            return userRoles;
        }

        protected List<UserCompanyRole> GetUserCompanyRolesForCompanyFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<UserCompanyRole>();

            string key = config.GetCacheKey((int)BusinessCacheType.UserCompanyRole);

            List<UserCompanyRole> userCompanyRoles = GetFromCache<List<UserCompanyRole>>(key);
            if (userCompanyRoles == null)
            {
                userCompanyRoles = RoleManager.GetAllUserCompanyRolesByCompany(entities, config.ActorCompanyId);
                BusinessMemoryCache<List<UserCompanyRole>>.Set(key, userCompanyRoles, config.Seconds);
            }

            return userCompanyRoles;
        }

        #endregion

        #region Payroll

        protected List<PayrollProduct> GetPayrollProductsFromCache(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var result = GetPayrollProductsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            return result.IsNullOrEmpty() ? null : result;
        }

        protected List<PayrollProduct> GetPayrollProductsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return null;

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollProducts);

            List<PayrollProduct> payrollProducts = GetFromCache<List<PayrollProduct>>(key);
            if (payrollProducts == null)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

                payrollProducts = ProductManager.GetPayrollProductsWithSettings(entities ?? entitiesReadOnly, config.ActorCompanyId, null);
                AddToCache(key, payrollProducts, config);
            }

            return payrollProducts;
        }

        protected List<PayrollProductDTO> GetPayrollProductDTOsWithSettingsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollProductDTO>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollProductDTOs);
            config.SetBusinessMemoryDistributionSetting(BusinessMemoryDistributionSetting.FullyHybridCache);

            List<PayrollProductDTO> payrollProducts = GetFromCache<List<PayrollProductDTO>>(key, config);
            if (payrollProducts == null)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

                payrollProducts = ProductManager.GetPayrollProductsWithSettings(entities ?? entitiesReadOnly, config.ActorCompanyId, null).ToDTOs(true, false, false, false, false).ToList();
                AddToCache(key, payrollProducts, config);
            }

            return payrollProducts;
        }

        protected PayrollProductDTO GetPayrollProductFromCache(CompEntities entities, CacheConfig config, int productId)
        {
            if (config == null)
                return null;

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollProduct, productId.ToString());
            PayrollProductDTO payrollProduct = GetFromCache<PayrollProductDTO>(key);
            if (payrollProduct == null)
            {
                payrollProduct = ProductManager.GetPayrollProduct(entities, productId).ToDTO(false, false, false, false, false);
                AddToCache(key, payrollProduct, config);
            }

            return payrollProduct;
        }

        protected PayrollPriceTypePeriodDTO GetPayrollPriceTypePeriodFromCache(CompEntities entities, int payrollPriceTypeId, DateTime? date, int actorCompanyId)
        {
            return GetPayrollPriceTypeFromCache(entities, payrollPriceTypeId, CacheConfig.Company(actorCompanyId, 120))?.GetPeriod(date);
        }

        protected PayrollPriceTypeDTO GetPayrollPriceTypeFromCache(CompEntities entities, int payrollPriceTypeId, CacheConfig config)
        {
            return GetPayrollPriceTypeDTOsFromCache(entities, config)?.FirstOrDefault(f => f.PayrollPriceTypeId == payrollPriceTypeId);
        }

        protected List<PayrollPriceTypeDTO> GetPayrollPriceTypeDTOsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollPriceTypeDTO>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollPriceTypeDTOs);
            List<PayrollPriceTypeDTO> payrollPriceTypes = GetFromCache<List<PayrollPriceTypeDTO>>(key);
            if (payrollPriceTypes == null)
            {
                payrollPriceTypes = GetPayrollPriceTypesFromCache(entities, config).ToDTOs(true).ToList();
                AddToCache(key, payrollPriceTypes, config);
            }

            return payrollPriceTypes;
        }

        protected List<PayrollPriceType> GetPayrollPriceTypesFromCache(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var result = GetPayrollPriceTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            return result.IsNullOrEmpty() ? null : result;
        }

        protected List<PayrollPriceType> GetPayrollPriceTypesFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollPriceType>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollPriceTypes);

            List<PayrollPriceType> payrollPriceTypes = GetFromCache<List<PayrollPriceType>>(key);
            if (payrollPriceTypes == null)
            {
                payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, config.ActorCompanyId, null, true);
                AddToCache(key, payrollPriceTypes, config);
            }

            return payrollPriceTypes;
        }

        protected List<PayrollGroupPriceType> GetPayrollGroupPriceTypesForCompanyFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollGroupPriceType>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollGroupPriceTypes);

            List<PayrollGroupPriceType> payrollPriceTypes = GetFromCache<List<PayrollGroupPriceType>>(key);
            if (payrollPriceTypes == null)
            {
                payrollPriceTypes = PayrollManager.GetPayrollGroupPriceTypesForCompany(entities, config.ActorCompanyId);
                AddToCache(key, payrollPriceTypes, config);
            }

            return payrollPriceTypes;
        }

        protected List<PayrollProductPriceType> GetPayrollProductPriceTypesForCompanyFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollProductPriceType>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollProductPriceTypes);

            List<PayrollProductPriceType> payrollPriceTypes = GetFromCache<List<PayrollProductPriceType>>(key);
            if (payrollPriceTypes == null)
            {
                payrollPriceTypes = ProductManager.GetPayrollProductPriceTypes(entities, config.ActorCompanyId);
                AddToCache(key, payrollPriceTypes, config);
            }

            return payrollPriceTypes;
        }

        protected List<PayrollProductReportSetting> GetPayrollProductReportSettingsForCompanyFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollProductReportSetting>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollProductReportSettings);

            List<PayrollProductReportSetting> payrollReportSettings = GetFromCache<List<PayrollProductReportSetting>>(key);
            if (payrollReportSettings == null)
            {
                payrollReportSettings = ProductManager.GetPayrollProductReportSettings(entities, config.ActorCompanyId);
                AddToCache(key, payrollReportSettings, config);
            }

            return payrollReportSettings;
        }

        protected List<PayrollLevel> GetPayrollLevelsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollLevel>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollLevel);

            List<PayrollLevel> payrollLevels = GetFromCache<List<PayrollLevel>>(key);
            if (payrollLevels == null)
            {
                payrollLevels = PayrollManager.GetPayrollLevels(entities, config.ActorCompanyId);
                AddToCache(key, payrollLevels, config);
            }

            return payrollLevels;
        }

        protected PayrollPriceFormulaDTO GetPayrollPriceFormulaDTOFromCache(CompEntities entities, int payrollPriceFormulaId, CacheConfig config)
        {
            var formulas = GetPayrollPriceFormulaDTOsFromCache(entities, config);
            return formulas?.FirstOrDefault(f => f.PayrollPriceFormulaId == payrollPriceFormulaId);
        }

        protected List<PayrollPriceFormula> GetPayrollPriceFormulasFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollPriceFormula>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollPriceFormulas);

            List<PayrollPriceFormula> payrollPriceFormulas = GetFromCache<List<PayrollPriceFormula>>(key);
            if (payrollPriceFormulas == null)
            {
                payrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(entities, config.ActorCompanyId, false);
                AddToCache(key, payrollPriceFormulas, config);
            }

            return payrollPriceFormulas;
        }

        protected List<PayrollPriceFormulaDTO> GetPayrollPriceFormulaDTOsFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollPriceFormulaDTO>();

            string key = config.GetCacheKey((int)BusinessCacheType.PayrollPriceFormulas);
            List<PayrollPriceFormulaDTO> payrollPriceFormulaDTOs;
            List<PayrollPriceFormula> payrollPriceFormulas = GetFromCache<List<PayrollPriceFormula>>(key);
            if (payrollPriceFormulas == null)
            {
                payrollPriceFormulas = GetPayrollPriceFormulasFromCache(entities, config);
                payrollPriceFormulaDTOs = payrollPriceFormulas.IsNullOrEmpty() ? new List<PayrollPriceFormulaDTO>() : payrollPriceFormulas.ToDTOs().ToList();
                AddToCache(key, payrollPriceFormulas, config);
            }
            else
                payrollPriceFormulaDTOs = payrollPriceFormulas.ToDTOs().ToList();

            return payrollPriceFormulaDTOs;
        }

        protected List<PayrollPriceType> GetPayrollPriceTypesForPersonalDataRepoFromCache(CompEntities entities, CacheConfig config)
        {
            if (config == null)
                return new List<PayrollPriceType>();

            string key = config.GetCacheKey((int)BusinessCacheType.PersonalDataRepoPayrollPriceTypes);

            List<PayrollPriceType> payrollPriceTypes = GetFromCache<List<PayrollPriceType>>(key);
            if (payrollPriceTypes == null)
            {
                payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, config.ActorCompanyId, null, loadPeriods: true);
                AddToCache(key, payrollPriceTypes, config);
            }

            return payrollPriceTypes;
        }

        #endregion

        #region SysTerm

        protected List<SysTermDTO> GetSystermsWithDescriptionFromCache(SOESysEntities entities, CacheConfig config, int sysTermGroupId, int langId)
        {
            if (config == null)
                return new List<SysTermDTO>();

            string key = config.GetCacheKey((int)BusinessCacheType.SysTermWithDescription, optionalKeyPart: sysTermGroupId.ToString());

            List<SysTermDTO> sysTerms = GetFromCache<List<SysTermDTO>>(key);
            if (sysTerms == null)
            {
                var terms = entities.SysTerm.Where(s => s.SysTermGroupId == sysTermGroupId && s.LangId == langId).ToList();
                sysTerms = terms.ToDTOs().ToList();
                BusinessMemoryCache<List<SysTermDTO>>.Set(key, sysTerms, config.Seconds);
            }

            return sysTerms.OrderBy(s => s.SysTermId).ThenBy(s => s.LangId).ToList();
        }

        #endregion

        #region TimeWorkReduction

        public bool UseTimeWorkReductionFromCache(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return UseTimeWorkReductionFromCache(entities, actorCompanyId);
        }

        public bool UseTimeWorkReductionFromCache(CompEntities entities, int actorCompanyId)
        {
            if (actorCompanyId == 0)
                return false;

            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 5);
            string key = config.GetCacheKey((int)BusinessCacheType.UseTimeWorkReduction);

            bool? useTimeWorkReduction = GetFromCache<bool?>(key);
            if (!useTimeWorkReduction.HasValue)
            {
                useTimeWorkReduction = TimeWorkReductionManager.UseTimeWorkReduction(entities, actorCompanyId);
                if (useTimeWorkReduction.Value)
                    config = CacheConfig.Company(actorCompanyId, 60 * 10);

                AddToCache(key, useTimeWorkReduction.Value, config);
            }

            return useTimeWorkReduction.Value;
        }

        #endregion

        #region TimeCode

        protected List<TimeCode> GetTimeCodeFromsCache(CompEntities entities, CacheConfig config)
        {
            string key = config.GetCacheKey((int)BusinessCacheType.TimeCode);

            List<TimeCode> timeCodes = GetFromCache<List<TimeCode>>(key);
            if (timeCodes == null)
            {
                timeCodes = TimeCodeManager.GetTimeCodes(entities, config.ActorCompanyId);
                AddToCache(key, timeCodes, config);
            }

            return timeCodes;
        }

        #endregion

        #region misc

        protected bool IsTele2(CompEntities entities, int actorCompanyId)
        {
            CacheConfig config = CacheConfig.Company(actorCompanyId, 60 * 60 * 2);
            string key = config.GetCacheKey((int)BusinessCacheType.MiscWithKey, "Tele2" + actorCompanyId.ToString());
            bool? tele2 = GetFromCache<bool?>(key);

            if (!tele2.HasValue)
            {
                tele2 = entities.Company.Any(f => f.ActorCompanyId == actorCompanyId && f.LicenseId == 489 && f.License.LicenseNr == "564");
                AddToCache(key, tele2, config);
            }

            return tele2 ?? false;
        }

        #endregion

        #endregion

        #region Flush

        public void FlushCompanyCategoryRecordsFromCache(CacheConfig config)
        {
            if (config == null)
                return;

            string key = config.GetCacheKey((int)BusinessCacheType.CategoryRecords);
            DeleteFromCache<List<CompanyCategoryRecord>>(key);
        }

        public void FlushEmployeeAccountsFromCache(CacheConfig config, string optionalKeyPart = null)
        {
            if (config == null)
                return;

            string key = config.GetCacheKey((int)BusinessCacheType.EmployeeAccounts, optionalKeyPart);
            DeleteFromCache<List<EmployeeAccount>>(key);
        }

        public void FlushEmploymentTypesFromCache(CacheConfig config)
        {
            if (config == null)
                return;

            foreach (int language in Enum.GetValues(typeof(TermGroup_Languages)))
            {
                string key = config.GetCacheKey((int)BusinessCacheType.EmploymentType, language.ToString());
                DeleteFromCache<List<EmployeeAccount>>(key);
            }
        }

        public void FlushFromCache<T>(CacheConfig config, BusinessCacheType businessCacheType)
        {
            if (config == null)
                return;

            string key = config.GetCacheKey((int)businessCacheType);
            DeleteFromCache<T>(key);
        }

        public void FlushTimeRulesFromCache(CacheConfig config)
        {
            if (config == null)
                return;

            string key = config.GetCacheKey((int)BusinessCacheType.TimeRules);
            DeleteFromCache<List<TimeRule>>(key);
        }

        public void FlushAccountDimsFromCache(CacheConfig config)
        {
            if (config == null)
                return;

            string key = config.GetCacheKey((int)BusinessCacheType.AccountDim);
            DeleteFromCache<List<AccountDimDTO>>(key);
        }

        #endregion
    }
}
