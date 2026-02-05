using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine.Cache
{
    internal class TimeEngineCompanyCache
    {
        public int ActorCompanyId;
        public TimeEngineCompanyCache(int actorCompanyId)
        {
            this.ActorCompanyId = actorCompanyId;
        }

        #region Account

        private bool accountHierarchySettingAccountIsSet = false;
        private List<int> accountHierarchySettingAccountIds = null;
        public (List<int> accountIds, bool isSet) GetAccountHierarchySettingAccount()
        {
            return (this.accountHierarchySettingAccountIds, this.accountHierarchySettingAccountIsSet);
        }
        public void SetAccountHierarchySettingAccount(List<int> accounts)
        {
            this.accountHierarchySettingAccountIsSet = true;
            this.accountHierarchySettingAccountIds = accounts;
        }

        #endregion

        #region AccountInternal

        private readonly Dictionary<int, AccountInternal> accountInternalsDict = new Dictionary<int, AccountInternal>(); //individually loaded
        private List<AccountInternal> accountInternalsWithAccount = null; //all loaded
        public List<AccountDTO> accountInternalDTOs = null;
        public List<AccountDTO> GetAccountInternals()
        {
            return this.accountInternalDTOs;
        }
        public List<AccountInternal> GetAccountInternalsWithAccount()
        {
            return this.accountInternalsWithAccount;
        }
        public AccountInternal GetAccountInternalWithAccount(int accountId)
        {
            AccountInternal accountInternal = this.accountInternalsWithAccount?.FirstOrDefault(i => i.AccountId == accountId);
            if (accountInternal == null && this.accountInternalsDict.ContainsKey(accountId))
                accountInternal = this.accountInternalsDict[accountId];
            return accountInternal;
        }
        public void SetAccountInternals(List<AccountInternal> accountInternals)
        {
            if (accountInternals == null)
                return;

            this.accountInternalsWithAccount = accountInternals;
        }
        public void SetAccountInternals(List<AccountDTO> accountInternals)
        {
            if (accountInternals == null)
                return;

            this.accountInternalDTOs = accountInternals;
        }
        public void SetAccountInternal(AccountInternal accountInternal)
        {
            if (accountInternal == null || this.accountInternalsDict.ContainsKey(accountInternal.AccountId))
                return;

            this.accountInternalsDict.Add(accountInternal.AccountId, accountInternal);
        }

        #endregion

        #region AccountStd

        private readonly Dictionary<int, AccountStd> accountStdsWithAccountDict = new Dictionary<int, AccountStd>(); //individually loaded
        private List<AccountStd> accountStdsWithAccount = null; //all loaded
        public List<AccountStd> GetAccountStdsWithAccount()
        {
            return this.accountStdsWithAccount;
        }
        public AccountStd GetAccountStdWithAccount(int accountId)
        {
            AccountStd accountStd = this.accountStdsWithAccount?.FirstOrDefault(i => i.AccountId == accountId);
            if (accountStd == null && this.accountStdsWithAccountDict.ContainsKey(accountId))
                accountStd = this.accountStdsWithAccountDict[accountId];
            return accountStd;
        }
        public void SetAccountStds(List<AccountStd> accountStds)
        {
            if (accountStds == null)
                return;

            this.accountStdsWithAccount = accountStds;
        }
        public void SetAccountStd(AccountStd accountStd)
        {
            if (accountStd == null || this.accountStdsWithAccountDict.ContainsKey(accountStd.AccountId))
                return;

            this.accountStdsWithAccountDict[accountStd.AccountId] = accountStd;
        }

        #endregion

        #region AccountDim

        private List<AccountDim> accountDims = null;
        private List<AccountDim> accountDimsWithAccounts = null;
        private List<AccountDimDTO> accountDimsWithWithParent = null;
        public List<AccountDim> GetAccountDims()
        {
            return accountDims;
        }
        public List<AccountDim> GetAccountDimsWithAccounts()
        {
            return accountDimsWithAccounts;
        }
        public List<AccountDimDTO> GetAccountDimInternalsWithParent()
        {
            return accountDimsWithWithParent;
        }
        public void SetAccountDims(List<AccountDim> accountDims)
        {
            if (accountDims == null)
                return;

            this.accountDims = accountDims;
        }
        public void SetAccountDimsWithAccounts(List<AccountDim> accountDims)
        {
            if (accountDims == null)
                return;

            this.accountDimsWithAccounts = accountDims;
        }
        public void SetAccountDimInternalsWithParent(List<AccountDimDTO> accountDims)
        {
            if (accountDims == null)
                return;

            this.accountDimsWithWithParent = accountDims;
        }

        #endregion

        #region AnnualLeaveGroup

        private List<AnnualLeaveGroup> annualLeaveGroups = null;
        public List<AnnualLeaveGroup> GetAnnualLeaveGroups()
        {
            return this.annualLeaveGroups;
        }
        public void AddAnnualLeaveGroup(AnnualLeaveGroup annualLeaveGroup)
        {
            if (annualLeaveGroup == null)
                return;

            if (this.annualLeaveGroups == null)
                this.annualLeaveGroups = new List<AnnualLeaveGroup>();
            if (!this.annualLeaveGroups.Any(p => p.AnnualLeaveGroupId == annualLeaveGroup.AnnualLeaveGroupId))
                this.annualLeaveGroups.Add(annualLeaveGroup);
        }
        public void SetAnnualLeaveGroups(List<AnnualLeaveGroup> annualLeaveGroups)
        {
            if (annualLeaveGroups != null)
                this.annualLeaveGroups = annualLeaveGroups;
        }

        #endregion

        #region AttestState

        private Dictionary<TermGroup_AttestEntity, AttestStateDTO> attestStateInitialDict = null;
        public (AttestStateDTO attestState, bool exists) GetAttestStateInitial(TermGroup_AttestEntity entity)
        {
            return ContainsAttestStateInitial(entity) ? (this.attestStateInitialDict.GetValue(entity), true) : (null, false);
        }
        public void AddAttestStateInitial(TermGroup_AttestEntity entity, AttestStateDTO attestState)
        {
            if (this.attestStateInitialDict == null)
                this.attestStateInitialDict = new Dictionary<TermGroup_AttestEntity, AttestStateDTO>();

            if (!this.attestStateInitialDict.ContainsKey(entity))
                this.attestStateInitialDict.Add(entity, attestState);
            else if (attestStateInitialDict[entity] == null)
                this.attestStateInitialDict[entity] = attestState;
        }
        private bool ContainsAttestStateInitial(TermGroup_AttestEntity entity)
        {
            return this.attestStateInitialDict?.ContainsKey(entity) ?? false;
        }

        private List<AttestStateDTO> attestStates = null;
        public List<AttestStateDTO> GetAttestStates()
        {
            return this.attestStates;
        }
        public AttestStateDTO GetAttestState(int attestStateId)
        {
            return attestStates?.FirstOrDefault(i => i.AttestStateId == attestStateId);
        }
        public void AddAttestState(AttestStateDTO attestState)
        {
            if (attestState == null)
                return;

            if (this.attestStates == null)
                this.attestStates = new List<AttestStateDTO>();
            if (!this.attestStates.Any(a => a.AttestStateId == attestState.AttestStateId))
                this.attestStates.Add(attestState);
        }
        public void AddAttestStates(List<AttestStateDTO> attestStates)
        {
            if (attestStates != null)
                this.attestStates = attestStates;
        }

        #endregion

        #region Company

        private Company company;
        public Company GetCompany()
        {
            return company;
        }
        public void SetCompany(Company company)
        {
            if (company != null)
                this.company = company;
        }

        #endregion

        #region Company settings

        private readonly Dictionary<CompanySettingType, int> companyIntSettingsDict = new Dictionary<CompanySettingType, int>();
        private readonly Dictionary<CompanySettingType, bool> companyBoolSettingsDict = new Dictionary<CompanySettingType, bool>();
        private readonly Dictionary<CompanySettingType, string> companyStringSettingsDict = new Dictionary<CompanySettingType, string>();
        private readonly Dictionary<CompanySettingType, DateTime> companyDateTimeSettingsDict = new Dictionary<CompanySettingType, DateTime>();
        public int? GetCompanyIntSetting(CompanySettingType companySettingType)
        {
            return this.companyIntSettingsDict.ContainsKey(companySettingType) ? companyIntSettingsDict[companySettingType] : (int?)null;
        }
        public bool? GetCompanyBoolSetting(CompanySettingType companySettingType)
        {
            return this.companyBoolSettingsDict.ContainsKey(companySettingType) ? companyBoolSettingsDict[companySettingType] : (bool?)null;
        }
        public string GetCompanyStringSetting(CompanySettingType companySettingType)
        {
            return this.companyStringSettingsDict.ContainsKey(companySettingType) ? companyStringSettingsDict[companySettingType] : null;
        }
        public DateTime? GetCompanyDateTimeSetting(CompanySettingType companySettingType)
        {
            return this.companyDateTimeSettingsDict.ContainsKey(companySettingType) ? companyDateTimeSettingsDict[companySettingType] : (DateTime?)null;
        }
        public void SetCompanyIntSetting(CompanySettingType companySettingType, int value)
        {
            if (!this.companyIntSettingsDict.ContainsKey(companySettingType))
                this.companyIntSettingsDict.Add(companySettingType, value);
        }
        public void SetCompanyBoolSetting(CompanySettingType companySettingType, bool value)
        {
            if (!this.companyBoolSettingsDict.ContainsKey(companySettingType))
                this.companyBoolSettingsDict.Add(companySettingType, value);
        }
        public void SetCompanyStringSetting(CompanySettingType companySettingType, string value)
        {
            if (!this.companyStringSettingsDict.ContainsKey(companySettingType))
                this.companyStringSettingsDict.Add(companySettingType, value);
        }
        public void SetCompanyDateTimeSetting(CompanySettingType companySettingType, DateTime value)
        {
            if (!this.companyDateTimeSettingsDict.ContainsKey(companySettingType))
                this.companyDateTimeSettingsDict.Add(companySettingType, value);
        }

        #endregion

        #region Employee

        private Employee hiddenEmployee;
        public Employee GetHiddenEmployee()
        {
            return hiddenEmployee;
        }
        public void SetHiddenEmployee(Employee hiddenEmployee)
        {
            if (hiddenEmployee == null)
                return;

            this.hiddenEmployee = hiddenEmployee;
        }

        List<EmployeeAgeDTO> employeeAgeInfos = null;
        public List<EmployeeAgeDTO> GetEmployeeAgeInfo()
        {
            return this.employeeAgeInfos;
        }
        public void AddEmployeeAgeInfo(List<EmployeeAgeDTO> employeeAgeInfos)
        {
            if (employeeAgeInfos == null)
                return;

            if (this.employeeAgeInfos == null)
                this.employeeAgeInfos = new List<EmployeeAgeDTO>();
            this.employeeAgeInfos.AddRange(employeeAgeInfos);
        }

        #endregion

        #region EmployeeGroup

        private List<EmployeeGroup> employeeGroups = null;
        public List<EmployeeGroup> GetEmployeeGroups()
        {
            return this.employeeGroups;
        }
        public void SetEmployeeGroups(List<EmployeeGroup> employeeGroups)
        {
            if (employeeGroups != null)
                this.employeeGroups = employeeGroups;
        }

        private List<EmployeeGroup> employeeGroupsWithWeekendSalaryDayTypes = null;
        public List<EmployeeGroup> GetEmployeeGroupsWithWeekendSalaryDayTypes()
        {
            return this.employeeGroupsWithWeekendSalaryDayTypes;
        }
        public void SetEmployeeGroupsWithWeekendSalaryDayTypes(List<EmployeeGroup> employeeGroups)
        {
            if (employeeGroups != null)
                this.employeeGroupsWithWeekendSalaryDayTypes = employeeGroups;
        }

        private List<EmployeeGroup> employeeGroupsWithDeviationCausesDayTypesAndTransitions = null;
        public List<EmployeeGroup> GetEmployeeGroupsWithDeviationCausesDayTypesAndTransitions()
        {
            return this.employeeGroupsWithDeviationCausesDayTypesAndTransitions;
        }
        public void SetEmployeeGroupsWithDeviationCausesDayTypesAndTransitions(List<EmployeeGroup> employeeGroups)
        {
            if (employeeGroups != null)
                this.employeeGroupsWithDeviationCausesDayTypesAndTransitions = employeeGroups;
        }

        #endregion

        #region EvaluatePayrollPriceFormulaInputDTO

        private EvaluatePayrollPriceFormulaInputDTO evaluatePriceFormulaInputDTO;
        public EvaluatePayrollPriceFormulaInputDTO GetEvaluatePriceFormulaInputDTO()
        {
            return this.evaluatePriceFormulaInputDTO;
        }
        public void SetEvaluatePriceFormulaInputDTO(EvaluatePayrollPriceFormulaInputDTO evaluatePriceFormula)
        {
            if (evaluatePriceFormula != null)
                this.evaluatePriceFormulaInputDTO = evaluatePriceFormula;
        }

        #endregion

        #region DayType

        private List<DayType> dayTypes = null;
        public List<DayType> GetDayTypes()
        {
            return this.dayTypes;
        }
        public void SetDayTypes(List<DayType> dayTypes)
        {
            if (dayTypes != null)
                this.dayTypes = dayTypes;
        }

        #endregion

        #region Holiday

        private List<HolidayDTO> holidaysWithDayType = null;
        public List<HolidayDTO> GetHolidaysWithDayType()
        {
            return this.holidaysWithDayType;
        }
        public void SetHolidaysWithDayType(List<HolidayDTO> holidays)
        {
            if (holidays != null)
                this.holidaysWithDayType = holidays;
        }

        private List<HolidayDTO> holidaysWithDayTypeDiscardedState = null;
        public List<HolidayDTO> GetHolidaysWithDayTypeDiscardedState()
        {
            return this.holidaysWithDayTypeDiscardedState;
        }
        public HolidayDTO GetHolidayWithDayTypeDiscardedState(int holidayId)
        {
            return this.holidaysWithDayTypeDiscardedState?.FirstOrDefault(i => i.HolidayId == holidayId);
        }
        public void SetHolidaysWithDayTypeDiscardedState(List<HolidayDTO> holidays)
        {
            if (holidays != null)
                this.holidaysWithDayTypeDiscardedState = holidays;
        }
        public void AddHolidayWithDayTypeDiscardedState(HolidayDTO holiday)
        {
            if (holiday == null)
                return;

            if (this.holidaysWithDayTypeDiscardedState == null)
                this.holidaysWithDayTypeDiscardedState = new List<HolidayDTO>();
            if (!this.holidaysWithDayTypeDiscardedState.Any(h => h.HolidayId == holiday.HolidayId))
                this.holidaysWithDayTypeDiscardedState.Add(holiday);
        }

        private List<HolidayDTO> holidaysWithDayTypeAndHalfDaySettings = null;
        public List<HolidayDTO> GetHolidaysWithDayTypeAndHalfDaySettings()
        {
            return this.holidaysWithDayTypeAndHalfDaySettings;
        }
        public void SetHolidaysWithDayTypeAndHalfDaySettings(List<HolidayDTO> holidays)
        {
            if (holidays != null)
                this.holidaysWithDayTypeAndHalfDaySettings = holidays;
        }

        #endregion

        #region MassRegistrationTemplate

        private List<MassRegistrationTemplateHeadDTO> massRegistrationTemplatesForPayrollCalculation = null;
        public List<MassRegistrationTemplateHeadDTO> GetMassRegistrationTemplatesForPayrollCalculation()
        {
            return this.massRegistrationTemplatesForPayrollCalculation;
        }
        public void AddMassregistrationTemplatesForPayrollCalculation(List<MassRegistrationTemplateHeadDTO> massRegistrationTemplates)
        {
            if (massRegistrationTemplates != null)
                this.massRegistrationTemplatesForPayrollCalculation = massRegistrationTemplates;
        }

        #endregion

        #region PayrollGroup

        private List<PayrollGroup> payrollGroups = null;
        public List<PayrollGroup> GetPayrollGroups()
        {
            return this.payrollGroups;
        }
        public void AddPayrollGroup(PayrollGroup payrollGroup)
        {
            if (payrollGroup == null)
                return;

            if (this.payrollGroups == null)
                this.payrollGroups = new List<PayrollGroup>();
            if (!this.payrollGroups.Any(p => p.PayrollGroupId == payrollGroup.PayrollGroupId))
                this.payrollGroups.Add(payrollGroup);
        }
        public void SetPayrollGroups(List<PayrollGroup> payrollGroups)
        {
            if (payrollGroups != null)
                this.payrollGroups = payrollGroups;
        }

        private List<PayrollGroup> payrollGroupsWithSettings = null;
        private bool isAllpayrollGroupsWithSettingsLoaded = false;
        public List<PayrollGroup> GetPayrollGroupsWithSettings()
        {
            return this.isAllpayrollGroupsWithSettingsLoaded ? this.payrollGroupsWithSettings : null; //Do not return any if cache is partly loaded
        }
        public void SetPayrollGroupsWithSettings(List<PayrollGroup> payrollGroups)
        {
            if (payrollGroups != null)
            {
                this.payrollGroupsWithSettings = payrollGroups;
                this.isAllpayrollGroupsWithSettingsLoaded = true;
            }
        }
        public void AddPayrollGroupsWithSettings(PayrollGroup payrollGroup)
        {
            if (payrollGroup == null)
                return;
            if (this.payrollGroupsWithSettings == null)
                this.payrollGroupsWithSettings = new List<PayrollGroup>();
            if (!this.payrollGroupsWithSettings.Any(p => p.PayrollGroupId == payrollGroup.PayrollGroupId))
                this.payrollGroupsWithSettings.Add(payrollGroup);
        }

        private List<PayrollGroup> payrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts = null;
        public List<PayrollGroup> GetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts()
        {
            return this.payrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts;
        }
        public void SetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts(List<PayrollGroup> payrollGroups)
        {
            if (payrollGroups != null)
                this.payrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts = payrollGroups;
        }

        public void AddPayrollGroupsAccountStds(List<PayrollGroupAccountStd> payrollGroupsAccountStdsInput)
        {
            if (payrollGroupsAccountStdsInput == null)
                return;

            this.payrollGroupsAccountStds = payrollGroupsAccountStdsInput;
        }
        private List<PayrollGroupAccountStd> payrollGroupsAccountStds = null;
        public List<PayrollGroupAccountStd> GetPayrollGroupsAccountStds()
        {
            return this.payrollGroupsAccountStds;
        }

        #endregion

        #region PayrollStartValueHead

        private List<PayrollStartValueHead> payrollStartValueHeads = null;
        public List<PayrollStartValueHead> GetPayrollStartValueHeads()
        {
            return this.payrollStartValueHeads;
        }
        public void SetPayrollStartValueHeads(List<PayrollStartValueHead> payrollStartValueHeads)
        {
            this.payrollStartValueHeads = payrollStartValueHeads ?? new List<PayrollStartValueHead>();
        }

        private Dictionary<int, bool> companyPayrollStartValuesByYear = null;
        public bool? HasCompanyPayrollStartValues(DateTime date)
        {
            return this.companyPayrollStartValuesByYear?.ContainsKey(date.Year);
        }
        public void SetCompanyHasPayrollStartValues(DateTime date, bool value)
        {
            if (this.companyPayrollStartValuesByYear == null)
                this.companyPayrollStartValuesByYear = new Dictionary<int, bool>();
            if (this.companyPayrollStartValuesByYear.ContainsKey(date.Year))
                this.companyPayrollStartValuesByYear[date.Year] = value;
            else
                this.companyPayrollStartValuesByYear.Add(date.Year, value);
        }

        #endregion

        #region PayrollGroupPayrollProduct

        private Dictionary<int, List<PayrollGroupPayrollProduct>> payrollGroupPayrollProductsDict;
        public List<PayrollGroupPayrollProduct> GetPayrollGroupPayrollProducts(int payrollGroupId)
        {
            return this.payrollGroupPayrollProductsDict.GetList(payrollGroupId, nullIfNotFound: true);
        }
        public void AddPayrollGroupPayrollProducts(int payrollGroupId, List<PayrollGroupPayrollProduct> products)
        {
            if (this.payrollGroupPayrollProductsDict == null)
                this.payrollGroupPayrollProductsDict = new Dictionary<int, List<PayrollGroupPayrollProduct>>();
            if (this.payrollGroupPayrollProductsDict.ContainsKey(payrollGroupId))
                this.payrollGroupPayrollProductsDict[payrollGroupId] = products;
            else
                this.payrollGroupPayrollProductsDict.Add(payrollGroupId, products);
        }

        #endregion

        #region PayrollPriceType

        private List<PayrollPriceType> payrollPriceTypesWithPeriods = null;
        public List<PayrollPriceType> GetPayrollPriceTypesWithPeriods()
        {
            return this.payrollPriceTypesWithPeriods;
        }
        public void SetPayrollPriceTypesWithPeriods(List<PayrollPriceType> payrollPriceTypes)
        {
            if (payrollPriceTypes != null)
                this.payrollPriceTypesWithPeriods = payrollPriceTypes;
        }

        #endregion

        #region Product

        private List<Product> products = null;
        public Product GetProduct(int productId)
        {
            return this.products?.FirstOrDefault(p => p.ProductId == productId);
        }
        public void AddProduct(Product product)
        {
            if (product == null)
                return;

            if (this.products == null)
                this.products = new List<Product>();
            if (!this.products.Any(p => p.ProductId == product.ProductId))
                this.products.Add(product);
        }

        #region InvoiceProduct

        private List<InvoiceProduct> invoiceProducts = null;
        public InvoiceProduct GetInvoiceProduct(int productId)
        {
            return this.invoiceProducts?.FirstOrDefault(p => p.ProductId == productId);
        }
        public void AddInvoiceProduct(InvoiceProduct product)
        {
            if (product == null)
                return;

            if (this.invoiceProducts == null)
                this.invoiceProducts = new List<InvoiceProduct>();
            if (!this.invoiceProducts.Any(p => p.ProductId == product.ProductId))
                this.invoiceProducts.Add(product);
        }

        #endregion

        #region PayrollProduct

        private List<PayrollProduct> payrollProducts = null;
        public PayrollProduct GetPayrollProduct(int productId, bool includeInactive)
        {
            return this.payrollProducts.GetPayrollProduct(productId, includeInactive);
        }
        public PayrollProduct GetPayrollProduct(string number, bool includeInactive)
        {
            return this.payrollProducts.GetPayrollProduct(number, includeInactive);
        }
        public PayrollProduct GetPayrollProduct(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return this.payrollProducts.GetPayrollProduct(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }
        public void AddPayrollProducts(List<PayrollProduct> payrollProducts)
        {
            if (payrollProducts.IsNullOrEmpty())
                return;
            foreach (PayrollProduct payrollProduct in payrollProducts)
            {
                AddPayrollProduct(payrollProduct);
            }
        }
        public void AddPayrollProduct(PayrollProduct payrollProduct)
        {
            this.payrollProducts = this.payrollProducts.TryAddPayrollProduct(payrollProduct);
        }

        private Dictionary<int, List<PayrollProduct>> payrollProductsWithSettingsAndAccountInternalsByLevel2 = null;
        public List<PayrollProduct> GetPayrollProductsWithSettingsAndAccountInternalsByLevel2(int sysPayrollTypeLevel2)
        {
            return this.payrollProductsWithSettingsAndAccountInternalsByLevel2.GetList(sysPayrollTypeLevel2, nullIfNotFound: true);
        }
        public void SetPayrollProductsWithSettingsAndAccountInternalsByLevel2(int sysPayrollTypeLevel2, List<PayrollProduct> payrollProducts)
        {
            if (payrollProducts.IsNullOrEmpty())
                return;
            if (this.payrollProductsWithSettingsAndAccountInternalsByLevel2 == null)
                this.payrollProductsWithSettingsAndAccountInternalsByLevel2 = new Dictionary<int, List<PayrollProduct>>();
            if (this.payrollProductsWithSettingsAndAccountInternalsByLevel2.ContainsKey(sysPayrollTypeLevel2))
                this.payrollProductsWithSettingsAndAccountInternalsByLevel2[sysPayrollTypeLevel2] = payrollProducts;
            else
                this.payrollProductsWithSettingsAndAccountInternalsByLevel2.Add(sysPayrollTypeLevel2, payrollProducts);
        }

        private List<PayrollProduct> payrollProductsWithSettings = null;
        public PayrollProduct GetPayrollProductWithSettings(int productId, bool includeInactive)
        {
            return this.payrollProductsWithSettings.GetPayrollProduct(productId, includeInactive);
        }
        public void AddPayrollProductWithSettings(PayrollProduct payrollProduct)
        {
            this.payrollProductsWithSettings = this.payrollProductsWithSettings.TryAddPayrollProduct(payrollProduct);
        }

        private List<PayrollProduct> payrollProductsWithSettingsAndAccountInternals = null;
        public List<PayrollProduct> GetPayrollProductsWithSettingsAndAccountInternals()
        {
            return this.payrollProductsWithSettingsAndAccountInternals;
        }
        public PayrollProduct GetPayrollProductWithSettingsAndAccountInternals(TermGroup_SysPayrollType sysPayrollTypeLevel1, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            return this.payrollProductsWithSettingsAndAccountInternals.GetPayrollProduct(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }
        public void AddPayrollProductsWithSettingsAndAccountInternals(List<PayrollProduct> payrollProducts)
        {
            if (payrollProducts.IsNullOrEmpty())
                return;
            foreach (PayrollProduct payrollProduct in payrollProducts)
            {
                AddPayrollProductWithSettingsAndAccountInternals(payrollProduct);
            }
        }
        public void AddPayrollProductWithSettingsAndAccountInternals(PayrollProduct payrollProduct)
        {
            this.payrollProductsWithSettingsAndAccountInternals = this.payrollProductsWithSettingsAndAccountInternals.TryAddPayrollProduct(payrollProduct);
        }

        private List<PayrollProduct> payrollProductsWithSettingsAndAccountInternalsAndStds = null;
        public List<PayrollProduct> GetPayrollProductsWithSettingsAndAccountInternalsAndStds()
        {
            return this.payrollProductsWithSettingsAndAccountInternalsAndStds;
        }
        public PayrollProduct GetPayrollProductWithSettingsAndAccountInternalsAndStds(int productId, bool includeInactive)
        {
            return this.payrollProductsWithSettingsAndAccountInternalsAndStds.GetPayrollProduct(productId, includeInactive);
        }
        public void AddPayrollProductWithSettingsAndAccountInternalsAndStds(PayrollProduct payrollProduct)
        {
            this.payrollProductsWithSettingsAndAccountInternalsAndStds = this.payrollProductsWithSettingsAndAccountInternalsAndStds.TryAddPayrollProduct(payrollProduct);
        }

        #endregion

        #endregion

        #region ShiftType

        private List<ShiftType> shiftTypes = null;
        public ShiftType GetShiftType(int shiftTypeId)
        {
            return this.shiftTypes?.FirstOrDefault(i => i.ShiftTypeId == shiftTypeId);
        }
        public ShiftType GetShiftTypeByAccount(int accountId)
        {
            return this.shiftTypes?.FirstOrDefault(i => i.AccountId == accountId);
        }
        public void AddShiftType(ShiftType shiftType)
        {
            if (shiftType == null)
                return;

            if (this.shiftTypes == null)
                this.shiftTypes = new List<ShiftType>();
            if (!this.shiftTypes.Any(s => s.ShiftTypeId == shiftType.ShiftTypeId))
                this.shiftTypes.Add(shiftType);
        }

        private List<int> shiftTypeIdsHandlingMoney = null;
        public List<int> GetShiftTypeIdsHandlingMoney()
        {
            return this.shiftTypeIdsHandlingMoney;
        }
        public void SetShiftTypeIdsHandlingMoney(List<int> shiftTypeIds)
        {
            if (shiftTypeIds != null)
                this.shiftTypeIdsHandlingMoney = shiftTypeIds;
        }

        #endregion

        #region TimeAbsenceRuleHeads

        private List<TimeAbsenceRuleHead> timeAbsenceRuleHeadWithRows = null;
        public List<TimeAbsenceRuleHead> GetTimeAbsenceRuleHeadsWithRows()
        {
            return this.timeAbsenceRuleHeadWithRows;
        }
        public void AddTimeAbsenceRuleHeads(List<TimeAbsenceRuleHead> timeAbsenceRuleHeads)
        {
            if (timeAbsenceRuleHeads != null)
                this.timeAbsenceRuleHeadWithRows = timeAbsenceRuleHeads;
        }

        #endregion

        #region TimeAccumulators

        private List<TimeAccumulator> timeAccumulatorsForTimeWorkAccount = null;
        public List<TimeAccumulator> GetTimeAccumulatorsForTimeWorkAccount()
        {
            return this.timeAccumulatorsForTimeWorkAccount;
        }
        public void SetTimeAccumulatorsForTimeWorkAccount(List<TimeAccumulator> timeAccumulators)
        {
            this.timeAccumulatorsForTimeWorkAccount = timeAccumulators;
        }

        #endregion

        #region TimeBlock

        private List<TimeBlock> timeBlocksWithAccounts = null;
        public TimeBlock GetTimeBlockWithAccounts(int timeBockId)
        {
            return this.timeBlocksWithAccounts?.FirstOrDefault(t => t.TimeBlockId == timeBockId);
        }
        public void AddTimeBlockWithAccounts(TimeBlock timeBlock)
        {
            if (timeBlock == null)
                return;

            if (this.timeBlocksWithAccounts == null)
                this.timeBlocksWithAccounts = new List<TimeBlock>();
            if (!this.timeBlocksWithAccounts.Any(t => t.TimeBlockId == timeBlock.TimeBlockId))
                this.timeBlocksWithAccounts.Add(timeBlock);
        }

        #endregion

        #region TimeCode

        private List<TimeCode> timeCodes = null;
        public TimeCode GetTimeCode(int timeCodeId)
        {
            return this.timeCodes?.FirstOrDefault(i => i.TimeCodeId == timeCodeId);
        }
        public void AddTimeCode(TimeCode timeCode)
        {
            if (timeCode == null)
                return;

            if (this.timeCodes == null)
                this.timeCodes = new List<TimeCode>();
            if (!this.timeCodes.Any(t => t.TimeCodeId == timeCode.TimeCodeId))
                this.timeCodes.Add(timeCode);
        }

        private List<TimeCode> timeCodesWithProducts = null;
        public TimeCode GetTimeCodeWithProducts(int timeCodeId)
        {
            return this.timeCodesWithProducts?.FirstOrDefault(i => i.TimeCodeId == timeCodeId);
        }
        public void AddTimeCodeWithProducts(TimeCode timeCode)
        {
            if (timeCode == null)
                return;

            if (this.timeCodesWithProducts == null)
                this.timeCodesWithProducts = new List<TimeCode>();
            if (!this.timeCodesWithProducts.Any(t => t.TimeCodeId == timeCode.TimeCodeId))
                this.timeCodesWithProducts.Add(timeCode);
        }

        private Dictionary<int, (TimeCode timeCode, PayrollProduct payrollProduct)> vacationGroupReplacementTimeCodeAndProduct = null;
        public bool TryGetVacationGroupReplacementTimeCodeAndProduct(int vacationGroupId, out TimeCode timeCode, out PayrollProduct payrollProduct)
        {
            if (this.vacationGroupReplacementTimeCodeAndProduct != null && vacationGroupReplacementTimeCodeAndProduct.ContainsKey(vacationGroupId))
            {
                timeCode = this.vacationGroupReplacementTimeCodeAndProduct[vacationGroupId].timeCode;
                payrollProduct = this.vacationGroupReplacementTimeCodeAndProduct[vacationGroupId].payrollProduct;
                return true;
            }
            else
            {
                timeCode = null;
                payrollProduct = null;
                return false;
            }
        }
        public void AddVacationGroupReplacementTimeCodeAndProduct(int vacationGroupId, TimeCode timeCode, PayrollProduct product)
        {
            if (this.vacationGroupReplacementTimeCodeAndProduct == null)
                this.vacationGroupReplacementTimeCodeAndProduct = new Dictionary<int, (TimeCode, PayrollProduct)>();
            if (!this.vacationGroupReplacementTimeCodeAndProduct.ContainsKey(vacationGroupId))
                this.vacationGroupReplacementTimeCodeAndProduct.Add(vacationGroupId, (timeCode, product));
        }

        #endregion

        #region TimeCodeRanking

        private bool? useTimeCodeRanking = null;
        public bool? UseTimeCodeRanking() => useTimeCodeRanking;
        public void SetUseTimeCodeRanking(bool useTimeCodeRanking) => this.useTimeCodeRanking = useTimeCodeRanking;

        private Dictionary<DateTime, TimeCodeRankingGroup> timeCodeRankingGroupsByDate = null;
        public TimeCodeRankingGroup GetTimeCodeRankingGroupWithRankings(DateTime date)
        {
            if (timeCodeRankingGroupsByDate.IsNullOrEmpty())
                return null;

            if (timeCodeRankingGroupsByDate.ContainsKey(date))
                return timeCodeRankingGroupsByDate[date];

            foreach (var kvp in timeCodeRankingGroupsByDate)
            {
                var timeCodeRankingGroup = kvp.Value;
                if (timeCodeRankingGroup == null)
                    continue;

                if (timeCodeRankingGroup.StartDate <= date && (timeCodeRankingGroup.StopDate == null || timeCodeRankingGroup.StopDate >= date))
                    return timeCodeRankingGroup;
            }

            return null;
        }
        public void AddTimeCodeRankingGroup(DateTime date, TimeCodeRankingGroup timeCodeRankingGroup)
        {
            timeCodeRankingGroupsByDate ??= new Dictionary<DateTime, TimeCodeRankingGroup>();
            if (!timeCodeRankingGroupsByDate.ContainsKey(date))
                timeCodeRankingGroupsByDate.Add(date, timeCodeRankingGroup);
        }

        #endregion

        #region TimeCodeBreak

        private List<TimeCodeBreak> timeCodeBreaks = null;
        public TimeCodeBreak GetTimeCodeBreak(int timeCodeId)
        {
            return this.timeCodeBreaks?.FirstOrDefault(t => t.TimeCodeBreakGroupId == timeCodeId);
        }
        public TimeCodeBreak GetTimeCodeBreak(int timeCodeBreakGroupId, int employeeGroupId)
        {
            return this.timeCodeBreaks?.FirstOrDefault(t => t.TimeCodeBreakGroupId == timeCodeBreakGroupId && t.EmployeeGroupsForBreak.Any(e => e.EmployeeGroupId == employeeGroupId));
        }
        public void AddTimeCodeBreak(TimeCodeBreak timeCodeBreak)
        {
            if (timeCodeBreak == null)
                return;

            if (this.timeCodeBreaks == null)
                this.timeCodeBreaks = new List<TimeCodeBreak>();
            if (!this.timeCodeBreaks.Any(p => p.TimeCodeBreakGroupId == timeCodeBreak.TimeCodeBreakGroupId))
                this.timeCodeBreaks.Add(timeCodeBreak);
        }

        private Dictionary<string, (DateTime, DateTime)> timeCodeBreakWindow = null;
        private string GetTimeCodeBreakWindowCacheKey(TimeCodeBreak timeCodeBreak, DateTime scheduleIn, DateTime scheduleOut)
        {
            if (timeCodeBreak == null)
                return null;

            if (timeCodeBreak.StartType == (int)SoeTimeCodeBreakTimeType.ScheduleIn || timeCodeBreak.StopType == (int)SoeTimeCodeBreakTimeType.ScheduleOut)
                return $"{timeCodeBreak.TimeCodeId}_{scheduleIn.ToShortTimeString()}_{scheduleOut.ToShortTimeString()}";
            else
                return $"{timeCodeBreak.TimeCodeId}";
        }
        public bool TryGetTimeCodeBreakWindow(TimeCodeBreak timeCodeBreak, DateTime scheduleIn, DateTime scheduleOut, out DateTime start, out DateTime stop)
        {
            start = scheduleIn;
            stop = scheduleOut;
            if (!this.timeCodeBreakWindow.IsNullOrEmpty())
            {
                string cacheKey = GetTimeCodeBreakWindowCacheKey(timeCodeBreak, scheduleIn, scheduleOut);
                if (cacheKey != null && this.timeCodeBreakWindow.ContainsKey(cacheKey))
                {
                    (start, stop) = this.timeCodeBreakWindow[cacheKey];
                    return true;
                }
            }
            return false;
        }
        public void AddTimeCodeBreakWindow(TimeCodeBreak timeCodeBreak, DateTime scheduleIn, DateTime scheduleOut, DateTime breakWindowStart, DateTime breakWindowStop)
        {
            string cacheKey = GetTimeCodeBreakWindowCacheKey(timeCodeBreak, scheduleIn, scheduleOut);
            if (cacheKey == null)
                return;

            if (this.timeCodeBreakWindow == null)
                this.timeCodeBreakWindow = new Dictionary<string, (DateTime, DateTime)>();

            if (this.timeCodeBreakWindow.ContainsKey(cacheKey))
                this.timeCodeBreakWindow[cacheKey] = (breakWindowStart, breakWindowStop);
            else
                this.timeCodeBreakWindow.Add(cacheKey, (breakWindowStart, breakWindowStop));
        }

        #endregion

        #region TimeDeviationCause

        private List<TimeDeviationCause> timeDeviationCauses = null;
        public List<TimeDeviationCause> GetTimeDeviationCauses()
        {
            return this.timeDeviationCauses;
        }
        public TimeDeviationCause GetTimeDeviationCause(int timeDeviationCauseId)
        {
            return this.timeDeviationCauses?.FirstOrDefault(i => i.TimeDeviationCauseId == timeDeviationCauseId);
        }
        public TimeDeviationCause GetTimeDeviationCauseByExtCode(string extCode)
        {
            if (string.IsNullOrEmpty(extCode))
                return null;
            return this.timeDeviationCauses?.FirstOrDefault(i => i.ExtCode == extCode);
        }
        public void SetTimeDeviationCauses(List<TimeDeviationCause> timeDeviationCauses)
        {
            if (timeDeviationCauses != null)
                this.timeDeviationCauses = timeDeviationCauses;
        }
        public void AddTimeDeviationCause(TimeDeviationCause timeDeviationCause)
        {
            if (timeDeviationCause == null)
                return;

            if (this.timeDeviationCauses == null)
                this.timeDeviationCauses = new List<TimeDeviationCause>();
            if (!this.timeDeviationCauses.Any(t => t.TimeDeviationCauseId == timeDeviationCause.TimeDeviationCauseId))
                this.timeDeviationCauses.Add(timeDeviationCause);
        }

        #endregion

        #region TimeLeisureCodes

        private List<TimeLeisureCode> timeLeisureCodes;
        public List<TimeLeisureCode> GetTimeLeisureCodes()
        {
            return this.timeLeisureCodes;
        }
        public void SetTimeLeisureCodes(List<TimeLeisureCode> timeLeisureCodes)
        {
            if (timeLeisureCodes == null)
                return;
            this.timeLeisureCodes = timeLeisureCodes;
        }

        public TimeLeisureCode GetTimeLeisureCode(int timeLeisureCodeId)
        {
            return this.timeLeisureCodes?.FirstOrDefault(i => i.TimeLeisureCodeId == timeLeisureCodeId);
        }

        public List<TimeLeisureCode> GetTimeLeisureCodeByEmployeeGroupId(int employeeGroupId)
        {
            if (employeeGroupId == 0)
                return null;
            var timeLeisureEmployeeGroups = this.timeLeisureCodes?.Where(i => i.EmployeeGroupTimeLeisureCode.Any(e => e.EmployeeGroupId == employeeGroupId));

            return timeLeisureEmployeeGroups?.ToList() ?? new List<TimeLeisureCode>();
        }

        public List<EmployeeGroupTimeLeisureCodeSetting> GetEmployeeGroupTimeLeisureCodeSettings(int timeLeisureCodeId, int employeeGroupId, TermGroup_TimeLeisureCodeSettingType type)
        {

            var onGroup = GetTimeLeisureCodeByEmployeeGroupId(employeeGroupId);

            if (onGroup == null)
                return null;
            var timeLeisureCode = onGroup.FirstOrDefault(i => i.TimeLeisureCodeId == timeLeisureCodeId);
            if (timeLeisureCode == null)
                return null;

            var group = timeLeisureCode.EmployeeGroupTimeLeisureCode.FirstOrDefault(i => i.EmployeeGroupId == employeeGroupId);

            if (group == null)
                return null;

            if (group.EmployeeGroupTimeLeisureCodeSetting.IsNullOrEmpty())
                return null;

            var settings = group.EmployeeGroupTimeLeisureCodeSetting.Where(w => w.Type == (int)type);

            return settings.ToList();
        }

        #endregion

        #region TimePeriod

        private Dictionary<int, List<TimePeriod>> timePeriodsByHeadDict = null;
        public List<TimePeriod> GetTimePeriods(int timePeriodHeadId)
        {
            return this.timePeriodsByHeadDict != null && this.timePeriodsByHeadDict.ContainsKey(timePeriodHeadId) ? this.timePeriodsByHeadDict[timePeriodHeadId] : null;
        }
        public void SetTimePeriods(List<TimePeriod> timePeriods, int timePeriodHeadId)
        {
            if (timePeriods == null)
                return;

            if (this.timePeriodsByHeadDict == null)
                this.timePeriodsByHeadDict = new Dictionary<int, List<TimePeriod>>();
            if (!this.timePeriodsByHeadDict.ContainsKey(timePeriodHeadId))
                this.timePeriodsByHeadDict.Add(timePeriodHeadId, timePeriods);
            else
                this.timePeriodsByHeadDict[timePeriodHeadId] = timePeriods;
        }

        private List<TimePeriod> timePeriods = null;
        public TimePeriod GetTimePeriod(int timePeriodId)
        {
            return this.timePeriods?.FirstOrDefault(x => x.TimePeriodId == timePeriodId);
        }
        public void AddTimePeriod(TimePeriod timePeriod)
        {
            if (timePeriod == null)
                return;

            if (this.timePeriods == null)
                this.timePeriods = new List<TimePeriod>();
            if (!this.timePeriods.Any(t => t.TimePeriodId == timePeriod.TimePeriodId))
                this.timePeriods.Add(timePeriod);
        }

        #endregion

        #region TimeScheduleEmployeePeriod

        private List<TimeScheduleEmployeePeriod> timeScheduleEmployeePeriods = null;
        public TimeScheduleEmployeePeriod GetTimeScheduleEmployeePeriod(int timeScheduleEmployeePeriodId)
        {
            return this.timeScheduleEmployeePeriods?.FirstOrDefault(t => t.TimeScheduleEmployeePeriodId == timeScheduleEmployeePeriodId);
        }
        public TimeScheduleEmployeePeriod GetTimeScheduleEmployeePeriod(int employeeId, DateTime date)
        {
            return this.timeScheduleEmployeePeriods?.FirstOrDefault(t => t.EmployeeId == employeeId && t.Date == date.Date);
        }
        public void AddTimeScheduleEmployeePeriods(List<TimeScheduleEmployeePeriod> timeScheduleEmployeePeriods)
        {
            if (timeScheduleEmployeePeriods.IsNullOrEmpty())
                return;

            foreach (TimeScheduleEmployeePeriod timeScheduleEmployeePeriod in timeScheduleEmployeePeriods)
            {
                AddTimeScheduleEmployeePeriod(timeScheduleEmployeePeriod);
            }
        }
        public void AddTimeScheduleEmployeePeriod(TimeScheduleEmployeePeriod timeScheduleEmployeePeriod)
        {
            if (timeScheduleEmployeePeriod == null)
                return;

            if (this.timeScheduleEmployeePeriods == null)
                this.timeScheduleEmployeePeriods = new List<TimeScheduleEmployeePeriod>();
            if (!this.timeScheduleEmployeePeriods.Any(t => t.TimeScheduleEmployeePeriodId == timeScheduleEmployeePeriod.TimeScheduleEmployeePeriodId))
                this.timeScheduleEmployeePeriods.Add(timeScheduleEmployeePeriod);
        }

        #endregion

        #region TimeScheduletemplateBlock

        private List<TimeScheduleTemplateBlock> timeScheduleTemplateBlocksWithAccounts = null;
        public TimeScheduleTemplateBlock GetScheduleTemplateBlocksBlocksWithAccounts(int timeScheduleTemplateBlockId)
        {
            return this.timeScheduleTemplateBlocksWithAccounts?.FirstOrDefault(i => i.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId);
        }
        public void AddScheduleTemplateBlocksBlocksWithAccounts(TimeScheduleTemplateBlock timeScheduleTemplateBlock)
        {
            if (timeScheduleTemplateBlock == null)
                return;

            if (this.timeScheduleTemplateBlocksWithAccounts == null)
                this.timeScheduleTemplateBlocksWithAccounts = new List<TimeScheduleTemplateBlock>();
            if (!this.timeScheduleTemplateBlocksWithAccounts.Any(p => p.TimeScheduleTemplateBlockId == timeScheduleTemplateBlock.TimeScheduleTemplateBlockId))
                this.timeScheduleTemplateBlocksWithAccounts.Add(timeScheduleTemplateBlock);
        }

        public Dictionary<DateTime, List<TimeScheduleTemplateBlock>> templateBlocksByDateDict = null;
        public bool TryGetScheduleBlocksForCompanyDiscardScenario(DateTime dateFrom, DateTime dateTo, out List<TimeScheduleTemplateBlock> templateBlocks, out List<DateTime> datesNotCached)
        {
            templateBlocks = new List<TimeScheduleTemplateBlock>();
            datesNotCached = new List<DateTime>();

            DateTime date = dateFrom.Date;
            while (date <= dateTo.Date)
            {
                if (this.templateBlocksByDateDict != null && this.templateBlocksByDateDict.ContainsKey(date))
                    templateBlocks.AddRange(this.templateBlocksByDateDict[date]);
                else if (!datesNotCached.Contains(date))
                    datesNotCached.Add(date);

                date = date.AddDays(1).Date;
            }

            return !datesNotCached.Any();
        }
        public void AddScheduleTemplateBlocks(List<TimeScheduleTemplateBlock> templateBlocks)
        {
            if (templateBlocks.IsNullOrEmpty())
                return;

            if (this.templateBlocksByDateDict == null)
                this.templateBlocksByDateDict = new Dictionary<DateTime, List<TimeScheduleTemplateBlock>>();

            foreach (var templateBlockByDate in templateBlocks.Where(i => i.Date.HasValue).GroupBy(i => i.Date.Value.Date))
            {
                if (!this.templateBlocksByDateDict.ContainsKey(templateBlockByDate.Key))
                    this.templateBlocksByDateDict.Add(templateBlockByDate.Key, templateBlockByDate.ToList());
            }
        }

        #endregion

        #region TimeScheduleTemplateBlockTask

        private List<TimeScheduleTemplateBlockTask> timeScheduleTemplateBlockTasks = null;
        public TimeScheduleTemplateBlockTask GetTimeScheduleTemplateBlockTask(int timeScheduleTemplateBlockTaskId)
        {
            return this.timeScheduleTemplateBlockTasks?.FirstOrDefault(i => i.TimeScheduleTemplateBlockTaskId == timeScheduleTemplateBlockTaskId);
        }
        public void AddTimeScheduleTemplateBlockTasks(List<TimeScheduleTemplateBlockTask> timeScheduleTemplateBlockTasks)
        {
            if (timeScheduleTemplateBlockTasks.IsNullOrEmpty())
                return;

            foreach (TimeScheduleTemplateBlockTask timeScheduleTemplateBlockTask in timeScheduleTemplateBlockTasks)
            {
                AddTimeScheduleTemplateBlockTask(timeScheduleTemplateBlockTask);
            }
        }
        public void AddTimeScheduleTemplateBlockTask(TimeScheduleTemplateBlockTask timeScheduleTemplateBlockTask)
        {
            if (timeScheduleTemplateBlockTask == null)
                return;

            if (this.timeScheduleTemplateBlockTasks == null)
                this.timeScheduleTemplateBlockTasks = new List<TimeScheduleTemplateBlockTask>();
            if (!this.timeScheduleTemplateBlockTasks.Any(s => s.TimeScheduleTemplateBlockTaskId == timeScheduleTemplateBlockTask.TimeScheduleTemplateBlockTaskId))
                this.timeScheduleTemplateBlockTasks.Add(timeScheduleTemplateBlockTask);
        }

        #endregion

        #region TimeScheduleTemplateHead

        private List<TimeScheduleTemplateHead> timeScheduleTemplateHeadsWithPeriods = null;
        public TimeScheduleTemplateHead GetTimeScheduleTemplateHeadWithPeriods(int timeScheduleTemplateHeadId)
        {
            return this.timeScheduleTemplateHeadsWithPeriods?.FirstOrDefault(i => i.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId);
        }
        public void AddTimeScheduleTemplateHeadWithPeriods(TimeScheduleTemplateHead timeScheduleTemplateHead)
        {
            if (timeScheduleTemplateHead == null)
                return;

            if (this.timeScheduleTemplateHeadsWithPeriods == null)
                this.timeScheduleTemplateHeadsWithPeriods = new List<TimeScheduleTemplateHead>();
            if (!this.timeScheduleTemplateHeadsWithPeriods.Any(s => s.TimeScheduleTemplateHeadId == timeScheduleTemplateHead.TimeScheduleTemplateHeadId))
                this.timeScheduleTemplateHeadsWithPeriods.Add(timeScheduleTemplateHead);
        }

        private List<TimeScheduleTemplateHead> timeScheduleTemplateHeads = null;
        public TimeScheduleTemplateHead GetTimeScheduleTemplateHead(int timeScheduleTemplateHeadId)
        {
            return this.timeScheduleTemplateHeads?.FirstOrDefault(i => i.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId);
        }
        public void AddTimeScheduleTemplateHead(TimeScheduleTemplateHead timeScheduleTemplateHead)
        {
            if (timeScheduleTemplateHead == null)
                return;

            if (this.timeScheduleTemplateHeads == null)
                this.timeScheduleTemplateHeads = new List<TimeScheduleTemplateHead>();
            if (!this.timeScheduleTemplateHeads.Any(s => s.TimeScheduleTemplateHeadId == timeScheduleTemplateHead.TimeScheduleTemplateHeadId))
                this.timeScheduleTemplateHeads.Add(timeScheduleTemplateHead);
        }

        #endregion

        #region TimeScheduleTemplatePeriod

        private Dictionary<int, TimeScheduleTemplatePeriod> timeScheduleTemplatePeriodsDict = null;
        public TimeScheduleTemplatePeriod GetTimeScheduleTemplatePeriod(int timeScheduleTemplatePeriodId)
        {
            return this.timeScheduleTemplatePeriodsDict != null && this.timeScheduleTemplatePeriodsDict.ContainsKey(timeScheduleTemplatePeriodId)
                ? this.timeScheduleTemplatePeriodsDict[timeScheduleTemplatePeriodId]
                : null;
        }
        public void AddTimeScheduleTemplatePeriod(TimeScheduleTemplatePeriod timeScheduleTemplatePeriod)
        {
            if (timeScheduleTemplatePeriod == null)
                return;

            if (this.timeScheduleTemplatePeriodsDict == null)
                this.timeScheduleTemplatePeriodsDict = new Dictionary<int, TimeScheduleTemplatePeriod>();
            if (!this.timeScheduleTemplatePeriodsDict.ContainsKey(timeScheduleTemplatePeriod.TimeScheduleTemplatePeriodId))
                this.timeScheduleTemplatePeriodsDict.Add(timeScheduleTemplatePeriod.TimeScheduleTemplatePeriodId, timeScheduleTemplatePeriod);
        }

        #endregion

        #region TimeScheduleType

        private List<TimeScheduleType> timeScheduleTypes = null;
        public TimeScheduleType GetTimeScheduleType(int timeScheduleTypeId)
        {
            return this.timeScheduleTypes?.FirstOrDefault(i => i.TimeScheduleTypeId == timeScheduleTypeId);
        }
        public void AddTimeScheduleType(TimeScheduleType timeScheduleType)
        {
            if (timeScheduleType == null)
                return;

            if (this.timeScheduleTypes == null)
                this.timeScheduleTypes = new List<TimeScheduleType>();
            if (!this.timeScheduleTypes.Any(t => t.TimeScheduleTypeId == timeScheduleType.TimeScheduleTypeId))
                this.timeScheduleTypes.Add(timeScheduleType);
        }

        private List<TimeScheduleType> timeScheduleTypesWithFactors = null;
        public List<TimeScheduleType> GetTimeScheduleTypesWithFactor()
        {
            return this.timeScheduleTypesWithFactors;
        }
        public TimeScheduleType GetTimeScheduleTypeWithFactors(int timeScheduleTypeId)
        {
            return this.timeScheduleTypesWithFactors?.FirstOrDefault(i => i.TimeScheduleTypeId == timeScheduleTypeId);
        }
        public void AddTimeScheduleTypesWithFactor(List<TimeScheduleType> timeScheduleTypes)
        {
            if (timeScheduleTypes == null)
                return;

            if (this.timeScheduleTypesWithFactors == null)
                this.timeScheduleTypesWithFactors = new List<TimeScheduleType>();
            this.timeScheduleTypesWithFactors.AddRange(timeScheduleTypes);
        }

        private List<int> timeScheduleTypeIdsIsNotScheduleTime = null;
        public List<int> GetTimeScheduleTypeIdsIsNotScheduleTime()
        {
            return this.timeScheduleTypeIdsIsNotScheduleTime;
        }
        public void SetTimeScheduleTypeIdsIsNotScheduleTime(List<int> timeScheduleTypeIdsIsNotScheduleTime)
        {
            if (timeScheduleTypeIdsIsNotScheduleTime != null)
                this.timeScheduleTypeIdsIsNotScheduleTime = timeScheduleTypeIdsIsNotScheduleTime;
        }

        #endregion

        #region TimeTerminal

        private List<TimeTerminal> timeTerminals = null;
        public TimeTerminal GetTimeTerminal(int timeTerminalId)
        {
            return this.timeTerminals?.FirstOrDefault(i => i.TimeTerminalId == timeTerminalId);
        }
        public void AddTimeTerminal(TimeTerminal timeTerminal)
        {
            if (timeTerminal == null)
                return;

            if (this.timeTerminals == null)
                this.timeTerminals = new List<TimeTerminal>();
            this.timeTerminals.Add(timeTerminal);
        }

        #endregion

        #region TimeWorkReduction

        private bool? useTimeWorkReduction = null;
        public bool? UseTimeWorkReduction() => useTimeWorkReduction;
        public void SetUseTimeWorkReduction(bool useTimeWorkReduction) => this.useTimeWorkReduction = useTimeWorkReduction;

        #endregion

        #region VacationGroup

        private List<VacationGroup> vacationGroupsWithSE = null;
        public List<VacationGroup> GetVacationGroupsWithSE()
        {
            return this.vacationGroupsWithSE;
        }
        public VacationGroup GetVacationGroupWithSE(int vacationGroupId)
        {
            return this.vacationGroupsWithSE?.FirstOrDefault(v => v.VacationGroupId == vacationGroupId);
        }
        public void SetVacationGroups(List<VacationGroup> vacationGroups)
        {
            if (vacationGroups != null)
                this.vacationGroupsWithSE = vacationGroups;
        }
        public void AddVacationGroup(VacationGroup vacationGroup)
        {
            if (vacationGroup == null)
                return;
            if (this.vacationGroupsWithSE == null)
                this.vacationGroupsWithSE = new List<VacationGroup>();
            if (!this.vacationGroupsWithSE.Any(v => v.VacationGroupId == vacationGroup.VacationGroupId))
                this.vacationGroupsWithSE.Add(vacationGroup);
        }

        private List<VacationGroupSE> vacationGroupSEs = null;
        public VacationGroupSE GetVacationGroupSE(int vacationGroupId)
        {
            return this.vacationGroupSEs?.FirstOrDefault(v => v.VacationGroupId == vacationGroupId);
        }
        public void AddVacationGroup(VacationGroupSE vacationGroup)
        {
            if (vacationGroup == null)
                return;
            if (this.vacationGroupSEs == null)
                this.vacationGroupSEs = new List<VacationGroupSE>();
            if (!this.vacationGroupSEs.Any(v => v.VacationGroupId == vacationGroup.VacationGroupId))
                this.vacationGroupSEs.Add(vacationGroup);
        }

        #endregion
    }
}
