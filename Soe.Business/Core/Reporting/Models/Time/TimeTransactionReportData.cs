using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class TimeTransactionReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly TimeTransactionReportDataInput _reportDataInput;
        private readonly TimeTransactionReportDataOutput _reportDataOutput;

        private bool AdjustQuantityCalendarDays => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantityCalendarDays);
        private bool AdjustQuantityWorkDays => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTransactionQuantityWorkDays);
        private bool SetUserName => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.UserName);
        private bool LoadEmploymentType => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.EmploymentType);
        private bool LoadEmployeeGroup => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeGroup);
        private bool LoadSysPayrollPrice => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTransactionUnitPrice);
        private bool LoadEmploymentTaxRates => false;
        private bool LoadTimeRules => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.TimeRuleName);
        private bool LoadAllAccumulators => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel3);
        private bool LoadAttestStates => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.AttestStateName);
        private bool LoadExternalAuthId => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.ExternalAuthId);
        private bool LoadCompanyCategoryRecords => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeCategoryCode ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeCategoryName
        );
        private bool LoadPayrollProducts => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollProductDescription ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollProductName ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollProductNumber
        );
        private bool LoadAccountStd => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.AccountName ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.AccountNr
        );
        private bool LoadAccountInternal => EnumUtility.GetNames<TermGroup_TimeTransactionMatrixColumns>().Any(a => a.Contains("AccountInternal"));
        private bool LoadEmployeeAccount => LoadAccountInternal && (
            EnumUtility.GetNames<TermGroup_TimeTransactionMatrixColumns>().Any(a => a.Contains("EmployeeHier")) ||
            EnumUtility.GetNames<TermGroup_TimeTransactionMatrixColumns>().Any(a => a.Contains("EmployeeAccount"))
        );
        private bool LoadEmploymentGroups => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeGroup ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollGroup
        );
        private bool LoadCurrency => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.CurrencyCode ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.CurrencyName
        );
        private bool LoadPayrollTypes => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel1 ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel2 ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel3 ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTypeLevel4 ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel1 ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel2 ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel3 ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.PayrollTransactionPayrollTypeLevel4
        );
        private bool LoadTimeCodes => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.TimeCodeCode ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.TimeCodeName);
        private bool LoadUser => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.UserName ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.ExternalAuthId
        );
        private bool LoadEmployeeAccountInternal => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeAccountInternalNames ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeAccountInternalNrs ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeHierachicalAccountInternalNames ||
            a.Column == TermGroup_TimeTransactionMatrixColumns.EmployeeHierachicalAccountInternalNrs
        );
        private bool LoadRatio => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeTransactionMatrixColumns.Ratio);
        public TimeTransactionReportData(ParameterObject parameterObject, TimeTransactionReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new TimeTransactionReportDataOutput(reportDataInput);
        }

        public static List<TimeTransactionReportDataReportDataField> GetPossibleDataFields()
        {
            List<TimeTransactionReportDataReportDataField> possibleFields = new List<TimeTransactionReportDataReportDataField>();
            EnumUtility.GetValues<TermGroup_TimeTransactionMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new TimeTransactionReportDataReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public TimeTransactionReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            ActionResult result = new ActionResult();

            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out _))
                return new ActionResult(false);

            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetBoolFromSelection(reportResult, out bool skipTimeScheduleTransactions, "skipTimeScheduleTransactions");
            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            bool filterOnPayrollProducts = TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);

            var company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : null;

            var accounts = new List<AccountDTO>();
            var categoryAccounts = new List<CategoryAccount>();
            var employeeAccounts = new List<EmployeeAccount>();
            var companyTransactions = new Dictionary<int, List<TimeTransactionMatrixItem>>();
            var companyTimePayrollScheduleTransaction = new Dictionary<int, List<TimeTransactionMatrixItem>>();
            Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> companyScheduleTransactionDTOs = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
            List<TimeAbsenceDetailDTO> timeAbsenceDetails = new List<TimeAbsenceDetailDTO>();

            #endregion

            try
            {
                #region Data

                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                int sysCountryId = base.GetCompanySysCountryIdFromCache(entities, base.ActorCompanyId);

                var payrollGroups = new List<PayrollGroup>();
                var employeeGroups = new List<EmployeeGroup>();
                var employmentTypes = new List<EmploymentTypeDTO>();

                if (LoadEmploymentGroups)
                {
                    payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId), loadSettings: true);
                    employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                }

                if (LoadEmploymentType)
                    employmentTypes = EmployeeManager.GetEmploymentTypes(entities, reportResult.ActorCompanyId, (TermGroup_Languages)base.GetLangId());

                Parallel.Invoke(() =>
                {
                    var payrollTransactions = TimeTransactionManager.GetTimePayrollTransactionDTOForReport(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.Input.ActorCompanyId);

                    foreach (var item in payrollTransactions)
                    {
                        companyTransactions.Add(item.Key, item.Value.Select(s => new TimeTransactionMatrixItem(s)).ToList());
                    }
                },
                    () =>
                    {
                        if (_reportDataOutput.Employees.IsNullOrEmpty())
                            _reportDataOutput.Employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true).ToDTOs(includeEmployments: true).ToList();

                        if (LoadSysPayrollPrice)
                            _reportDataOutput.SysPayrollPrices = PayrollManager.GetSysPayrollPriceView(sysCountryId);

                        if (LoadEmploymentTaxRates)
                            _reportDataOutput.EmploymentTaxRates = PayrollManager.GetRatesFromPayrollPriceView(entities, reportResult.Input.ActorCompanyId, selectionDateFrom, selectionDateTo, _reportDataOutput.Employees.GetBirthyears(), _reportDataOutput.SysPayrollPrices, TermGroup_SysPayrollPrice.SE_EmploymentTax, sysCountryId);

                        if (LoadCompanyCategoryRecords)
                            _reportDataOutput.CompanyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, reportResult.Input.ActorCompanyId);

                        if (LoadAllAccumulators)
                            _reportDataOutput.AllAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.Input.ActorCompanyId);

                        if (LoadPayrollProducts)
                            _reportDataOutput.PayrollProducts = ProductManager.GetPayrollProducts(reportResult.Input.ActorCompanyId, null, true, true, true).ToDTOs(false, false, true, true, true).ToList();

                        if (LoadAttestStates)
                            _reportDataOutput.AttestStates = AttestManager.GetAttestStates(reportResult.Input.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);

                        if (LoadAccountStd)
                            _reportDataOutput.AccountStds = AccountManager.GetAccountStdsByCompanyIgnoreState(entities, reportResult.Input.ActorCompanyId);

                        if (LoadAccountInternal)
                            _reportDataOutput.AccountInternals = AccountManager.GetAccountsInternalsByCompany(reportResult.Input.ActorCompanyId);

                        if (LoadCurrency)
                        {
                            _reportDataOutput.Currency = CountryCurrencyManager.GetCurrencyFromType(reportResult.Input.ActorCompanyId, TermGroup_CurrencyType.EnterpriseCurrency);
                        }

                        if (LoadPayrollTypes)
                            _reportDataOutput.PayrollTypes = base.GetTermGroupDict(TermGroup.SysPayrollType);

                        if (LoadTimeCodes)
                            _reportDataOutput.TimeCodes = TimeCodeManager.GetTimeCodes(reportResult.Input.ActorCompanyId);

                        if (LoadTimeRules)

                            _reportDataOutput.TimeRules = TimeRuleManager.GetTimeRulesFromCache(entities, reportResult.Input.ActorCompanyId);

                        if (LoadRatio)
                        {

                            timeAbsenceDetails = TimeBlockManager.GetTimeAbsenceDetails(entities, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                            companyScheduleTransactionDTOs = TimeScheduleManager.GetTimeEmployeeScheduleSmallDTODictForReport(selectionDateFrom, selectionDateTo, employees, reportResult.Input.ActorCompanyId, base.RoleId, shiftTypeIds: null, splitOnBreaks: true, removeBreaks: true);
                        }

                        if (LoadAccountInternal && LoadEmployeeAccount)
                        {

                            accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                            if (base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId))
                                employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                            else

                                categoryAccounts = base.GetCategoryAccountsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                        }

                        _reportDataOutput.TimeUnits = GetTermGroupContent(TermGroup.PayrollProductTimeUnit, addEmptyRow: true);
                    },
                    () =>
                    {
                        var scheduleTransaction = skipTimeScheduleTransactions ? new Dictionary<int, List<TimePayrollTransactionDTO>>() : TimeTransactionManager.GetTimePayrollScheduleTransactionDTOForReport(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.Input.ActorCompanyId);

                        foreach (var item in scheduleTransaction)
                        {
                            companyTimePayrollScheduleTransaction.Add(item.Key, item.Value.Select(s => new TimeTransactionMatrixItem(s)).ToList());
                        }
                    });

                foreach (var tpt in companyTransactions)
                {
                    foreach (var tst in companyTimePayrollScheduleTransaction.Where(w => w.Key == tpt.Key))
                    {
                        tpt.Value.AddRange(tst.Value);
                    }
                    _reportDataOutput.TimeTransactions.Add(tpt.Key, tpt.Value);
                }

                if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty())
                {
                    var filtered = new Dictionary<int, List<TimeTransactionMatrixItem>>();

                    foreach (var item in _reportDataOutput.TimeTransactions)
                    {
                        var values = item.Value.Where(w => w.TransactionDTO.AccountInternals != null && w.TransactionDTO.AccountInternals.ValidOnFiltered(validAccountInternals)).ToList();

                        if (values.Any())
                            filtered.Add(item.Key, values);
                    }

                    _reportDataOutput.TimeTransactions = filtered;
                }

                if (filterOnPayrollProducts)
                {
                    var filtered = new Dictionary<int, List<TimeTransactionMatrixItem>>();

                    foreach (var item in _reportDataOutput.TimeTransactions)
                    {
                        var values = item.Value.Where(w => selectionPayrollProductIds.Contains(w.TransactionDTO.PayrollProductId)).ToList();

                        if (values.Any())
                            filtered.Add(item.Key, values);
                    }

                    _reportDataOutput.TimeTransactions = filtered;
                }
                if (LoadRatio)
                {
                    foreach (var items in _reportDataOutput.TimeTransactions)
                    {
                        var trans = items.Value.Select(w => w.TransactionDTO).ToList();

                        IEnumerable<IGrouping<string, TimePayrollTransactionDTO>> transactionGroupByLevel = trans.GroupBy(f => $"{f.SysPayrollTypeLevel1}#{f.SysPayrollTypeLevel2}#{f.SysPayrollTypeLevel3}");

                        companyScheduleTransactionDTOs.TryGetValue(items.Key, out List<TimeEmployeeScheduleDataSmallDTO> scheduleTransactionDTOs);

                        foreach (var ab in transactionGroupByLevel.ToList())
                            CalculateAndSetAbsenceRatio(scheduleTransactionDTOs, ab.ToList(), timeAbsenceDetails, ab.ToList());
                    }
                }

                #region Adjust calendardays

                if (AdjustQuantityCalendarDays || AdjustQuantityWorkDays)
                {
                    foreach (var item in _reportDataOutput.TimeTransactions)
                    {
                        List<string> logUniqueDayRowsCalendarDays = new List<string>();
                        List<string> logUniqueDayRowsWorkDays = new List<string>();
                        foreach (var row in item.Value)
                        {
                            string uniqueKey = $"{row.EmployeeId}#{row.PayrollProductId}#{row.Date}";
                            if (!logUniqueDayRowsCalendarDays.Contains(uniqueKey))
                            {
                                if (row.Extended != null)
                                    row.Extended.QuantityCalendarDays = 1;
                            }
                            else
                            {
                                if (row.Extended != null)
                                    row.Extended.QuantityCalendarDays = 0;
                            }

                            if (logUniqueDayRowsWorkDays.Contains(uniqueKey) && row.Extended != null)
                            {
                                row.Extended.QuantityWorkDays = 0;
                            }

                            logUniqueDayRowsCalendarDays.Add(uniqueKey);
                            logUniqueDayRowsWorkDays.Add(uniqueKey);
                        }

                    }
                }
                foreach (var item in _reportDataOutput.TimeTransactions)
                {
                    var employee = employees.FirstOrDefault(f => f.EmployeeId == item.Key);
                    if (employee != null)
                    {
                        foreach (var row in item.Value)
                        {
                            row.EmployeeExternalCode = employee.ExternalCode;
                        }
                    }
                }
                var hierarchyAccountsOnyAccounts = accounts.Where(a => a.HierarchyOnly).ToList();
                bool hasAccountWithOnlyHierarchy = hierarchyAccountsOnyAccounts.Any();

                if (LoadUser)
                {
                    Guid idProviderGuid = Guid.Empty;
                    if (LoadExternalAuthId)
                    {

                        string value = SettingManager.GetStringSetting(SettingMainType.License, (int)LicenseSettingType.SSO_Key, 0, 0, company.LicenseId);
                        if (!string.IsNullOrEmpty(value) && Guid.TryParse(value, out idProviderGuid)) { }
                    }

                    var userIds = employees.Where(w => w.UserId.HasValue).Select(s => s.UserId.Value).ToList();
                    List<User> users = UserManager.GetUsers(userIds);

                    foreach (var item in _reportDataOutput.TimeTransactions)
                    {
                        var employee = employees.FirstOrDefault(f => f.EmployeeId == item.Key);
                        if (employee == null)
                            continue;

                        if (employee.UserId.HasValue)
                        {
                            var user = users.FirstOrDefault(f => f.UserId == employee.UserId.Value);
                            if (user != null)
                            {
                                if (LoadExternalAuthId && idProviderGuid != Guid.Empty)
                                {
                                    var externalAuthId = SoftOneIdConnector.GetExternalAuthId(user.idLoginGuid.Value, idProviderGuid);

                                    if (externalAuthId != null)
                                    {
                                        foreach (var row in item.Value)
                                        {
                                            row.ExternalAuthId = externalAuthId;
                                        }
                                    }
                                }
                                if (SetUserName)
                                {
                                    foreach (var row in item.Value)
                                    {
                                        row.UserName = user.LoginName;
                                    }
                                }
                            }
                        }
                    }
                }
                if (LoadEmployeeGroup || LoadEmploymentType || LoadEmployeeAccountInternal)
                    ProcessEmployeeData(companyTransactions, employees, employeeAccounts, employeeGroups, payrollGroups, employmentTypes, accounts, LoadAccountInternal, hasAccountWithOnlyHierarchy, LoadEmployeeGroup, LoadEmployeeGroup, LoadEmploymentType);

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(ex);
            }

            return result;
        }
        private void CalculateAndSetAbsenceRatio(List<TimeEmployeeScheduleDataSmallDTO> timeEmployeeSchedules, List<TimePayrollTransactionDTO> items, List<TimeAbsenceDetailDTO> timeAbsenceDetails, List<TimePayrollTransactionDTO> allItems)
        {
            foreach (var tran in items)
            {
                var absenceDetail = timeAbsenceDetails.FirstOrDefault(f => f.EmployeeId == tran.EmployeeId && f.Date == tran.Date && (f.TimeDeviationCauseId == tran.TimeDeviationCauseStartId || f.TimeDeviationCauseId == tran.TimeDeviationCauseStopId));

                if (absenceDetail?.Ratio != null)
                {
                    if (absenceDetail.Ratio != 0)
                        tran.AbsenceRatio = absenceDetail.Ratio.Value;
                    else
                        tran.AbsenceRatio = 0;
                }
            }

            foreach (var transOnDate in items.GroupBy(g => g.Date).OrderBy(o => o.Key))
            {
                if (transOnDate.Key.HasValue && transOnDate.Where(w => w.IsAbsence()).Sum(s => s.Quantity) != 0 && transOnDate.All(a => a.AbsenceRatio == 0))
                {
                    var scheduleOnDate = timeEmployeeSchedules?.Where(f => f.Date == transOnDate.Key).ToList();
                    var absenceTransactionsOnDate = transOnDate.Where(w => w.IsAbsence()).ToList();

                    if (!scheduleOnDate.IsNullOrEmpty())
                    {
                        var schedule = scheduleOnDate.Sum(s => Convert.ToInt16(s.Length));

                        if (schedule != 0)
                        {
                            var ratio = absenceTransactionsOnDate.Sum(a => a.Quantity) / (decimal)schedule;
                            absenceTransactionsOnDate.ForEach(f => f.AbsenceRatio = ratio * 100);
                        }
                    }
                }
                else if (transOnDate.All(a => a.AbsenceRatio == 0))
                {
                    var absenceTransactionsWithZeroRatio = transOnDate.Where(w => w.IsAbsence() && w.AbsenceRatio == 0).ToList();

                    foreach (var transaction in absenceTransactionsWithZeroRatio)
                    {
                        var previousDate = allItems.Where(w => w.Date < transaction.Date && w.AbsenceRatio != 0).OrderByDescending(o => o.Date).FirstOrDefault()?.Date;
                        var futureDate = allItems.Where(w => w.Date > transaction.Date && w.AbsenceRatio != 0).OrderBy(o => o.Date).FirstOrDefault()?.Date;

                        if (previousDate != null)
                        {
                            var previousRatio = allItems.First(f => f.Date == previousDate).AbsenceRatio;
                            transaction.AbsenceRatio = previousRatio;
                        }
                        else if (futureDate != null)
                        {
                            var futureRatio = allItems.First(f => f.Date == futureDate).AbsenceRatio;
                            transaction.AbsenceRatio = futureRatio;
                        }
                        else
                        {
                            transaction.AbsenceRatio = 100;
                        }
                    }
                }
            }
        }

        public void ProcessEmployeeData(Dictionary<int, List<TimeTransactionMatrixItem>> timeTransactions, List<Employee> employees, List<EmployeeAccount> employeeAccounts, List<EmployeeGroup> employeeGroups, List<PayrollGroup> payrollGroups, List<EmploymentTypeDTO> employmentTypes, List<AccountDTO> accounts, bool loadAccountInternal, bool hasAccountWithOnlyHierarchy, bool loadEmployeeGroup, bool loadPayrollGroup, bool loadEmploymentType)
        {
            foreach (var item in timeTransactions)
            {
                var employee = employees.FirstOrDefault(f => f.EmployeeId == item.Key);

                if (employee == null)
                    continue;

                var employeeEmployeeAccounts = LoadEmployeeAccountInternal ? employeeAccounts.FindAll(r => r.EmployeeId == employee.EmployeeId) : new List<EmployeeAccount>();

                foreach (var row in item.Value.Where(w => w.TransactionDTO?.Date != null))
                {
                    var date = row.TransactionDTO.Date.Value;
                    Employment employment = null;

                    if (LoadEmployeeAccountInternal && employeeEmployeeAccounts.Any())
                    {
                        employment = employee.GetEmployment(row.Date);
                        row.EmployeeAccountAnalysisFields = employeeEmployeeAccounts.AccountAnalysisFields(accounts, date, date, doNotFocusOnOnlyHierarchyAccounts: true);

                        if (hasAccountWithOnlyHierarchy)
                        {
                            row.EmployeeHierchicalAnalysisFields = employeeEmployeeAccounts.AccountAnalysisFields(accounts, date, date, focusOnOnlyHierarchyAccounts: true);
                        }
                    }

                    if (loadEmployeeGroup)
                    {
                        employment = employment ?? employee.GetEmployment(date);
                        row.EmployeeGroup = employee.GetEmployeeGroup(date, employeeGroups)?.Name;
                    }

                    if (loadPayrollGroup)
                    {
                        employment = employment ?? employee.GetEmployment(date);
                        row.PayrollGroup = employee.GetPayrollGroup(date, payrollGroups)?.Name;
                    }

                    if (loadEmploymentType)
                    {
                        employment = employment ?? employee.GetEmployment(date);
                        if (employment != null)
                            row.EmploymentType = employment.GetEmploymentTypeName(employmentTypes, date);
                    }
                }
            }
        }
    }

    public class TimeTransactionReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_TimeTransactionMatrixColumns Column { get; set; }
        public string ColumnKey { get; private set; }
        public string OptionKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public TimeTransactionReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.OptionKey = Selection?.Options?.Key;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_TimeTransactionMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_TimeTransactionMatrixColumns.Unknown;
        }
    }

    public class TimeTransactionReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<TimeTransactionReportDataReportDataField> Columns { get; set; }

        public TimeTransactionReportDataInput(CreateReportResult reportResult, List<TimeTransactionReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class TimeTransactionReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public Dictionary<int, List<TimeTransactionMatrixItem>> TimeTransactions { get; set; }
        public List<EmployeeDTO> Employees { get; set; }
        public List<PayrollProductDTO> PayrollProducts { get; set; }
        public TimeTransactionReportDataInput Input { get; set; }
        public List<EmployeeGroup> EmployeeGroups { get; set; }
        public List<PayrollGroup> PayrollGroups { get; set; }
        public List<PayrollPriceType> PayrollPriceTypes { get; set; }
        public List<SysPayrollPriceViewDTO> SysPayrollPrices { get; set; }
        public List<CompanyCategoryRecord> CompanyCategoryRecords { get; set; }
        public List<TimeAccumulator> AllAccumulators { get; set; }
        public List<AttestState> AttestStates { get; set; }
        public List<AccountStd> AccountStds { get; set; }
        public List<Account> AccountInternals { get; set; }
        public List<TimeCode> TimeCodes { get; set; }
        public List<Tuple<int, DateTime, decimal>> EmploymentTaxRates { get; set; }
        public Currency Currency { get; set; }
        public Dictionary<int, string> PayrollTypes { get; set; }
        public List<GenericType> TimeUnits { get; set; }
        public List<TimeRule> TimeRules { get; set; }

        public TimeTransactionReportDataOutput(TimeTransactionReportDataInput input)
        {
            this.TimeTransactions = new Dictionary<int, List<TimeTransactionMatrixItem>>();
            this.Employees = new List<EmployeeDTO>();
            this.PayrollProducts = new List<PayrollProductDTO>();
            this.Input = input;
        }
    }
}
