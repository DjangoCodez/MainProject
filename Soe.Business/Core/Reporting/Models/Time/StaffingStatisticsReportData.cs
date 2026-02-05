using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class StaffingStatisticsReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly StaffingStatisticsReportDataInput _reportDataInput;
        private readonly StaffingStatisticsReportDataOutput _reportDataOutput;

        private DateTime selectionDateFrom;
        private DateTime selectionDateTo;

        private bool loadSales => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("Sales"));
        private bool loadBudget => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("Budget"));
        private bool loadForecast => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("Forecast"));
        private bool loadTime => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("Time"));
        private bool loadSchedule => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("Schedule"));
        private bool loadTemplateSchedule => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("TemplateSchedule"));
        private bool hasDate => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("Date"));
        private bool tryToGroupOnEmployee => _reportDataInput.Columns.Any(a => a.Column == TermGroup_StaffingStatisticsMatrixColumns.EmployeeName);
        private bool scheduleAndTime => _reportDataInput.Columns.Any(a => EnumUtility.GetName<TermGroup_StaffingStatisticsMatrixColumns>(a.Column).Contains("ScheduleAndTime"));

        public StaffingStatisticsReportData(ParameterObject parameterObject, StaffingStatisticsReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new StaffingStatisticsReportDataOutput(reportDataInput);
        }

        public static List<StaffingStatisticsReportDataField> GetPossibleDataFields()
        {
            List<StaffingStatisticsReportDataField> possibleFields = new List<StaffingStatisticsReportDataField>();
            EnumUtility.GetValues<TermGroup_StaffingStatisticsMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new StaffingStatisticsReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public StaffingStatisticsReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }
        
        private ActionResult LoadData()
        {
            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out selectionDateFrom, out selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(false);
            if (employees == null)
                return new ActionResult(false);

            if (selectionDateTo.Date == DateTime.MaxValue.Date)
                selectionDateTo = selectionDateFrom;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Prereq                  

                    List<StaffingStatisticsItem> staffingStatisticsItems = new List<StaffingStatisticsItem>();
                    Dictionary<int, List<TimeScheduleTemplateBlock>> scheduleBlocks = new Dictionary<int, List<TimeScheduleTemplateBlock>>();
                    List<AccountDimDTO> selectedDims = new List<AccountDimDTO>();
                    List<int> selectionDimIds = new List<int>();

                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);
                    List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<VacationGroup> vacationGroups = GetVacationGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<AnnualLeaveGroup> annualLeaveGroups = GetAnnualLeaveGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<PayrollPriceType> payrollPriceTypes = GetPayrollPriceTypesFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    List<PayrollProductSettingDTO> payrollProductSettings = base.GetPayrollProductDTOsWithSettingsFromCache(entitiesReadOnly, CacheConfig.Company(ActorCompanyId)).SelectMany(sm => sm.Settings).ToList();
                    List<AccountDTO> accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<int> originalselectionAccountIds = selectionAccountIds.ToList();
                    string cacheKey = Guid.NewGuid().GetHashCode().ToString();
                    List<AccountDimDTO> dims = base.GetAccountDimsFromCache(entitiesReadOnly, CacheConfig.Company(reportResult.ActorCompanyId)).Where(w => w.IsInternal).ToList();
                    dims.CalculateLevels();

                    List<EmployeeDTO> employeeDTOs = employees.ToDTOs(includeEmployments: true,
                        employeeGroups: employeeGroups,
                        payrollGroups: payrollGroups,
                        vacationGroups: vacationGroups,
                        payrollPriceTypes: payrollPriceTypes);

                    MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
                    foreach (var col in matrixColumnsSelection.Columns)
                    {
                        int id = GetIdValueFromColumn(col);
                        if (id != 0 && !selectionAccountIds.Contains(id))
                            selectionDimIds.Add(id);
                    }

                    if (selectionDimIds.Any())
                        selectedDims = dims.Where(w => selectionDimIds.Contains(w.AccountDimId)).ToList();

                    List<StaffingStatisticsGroup> staffingStatisticsGroups = GetStaffingStatisticsGroups(entities, matrixColumnsSelection, selectionAccountIds, selectedDims, dims, accounts, useAccountHierarchy, true);
                    if (!staffingStatisticsGroups.Any()) //Fallback for now
                    {
                        LogInfo($"StaffingStatisticsReportData is using fallBack on actorcompanyid {base.ActorCompanyId} with selectionAccountIds " + string.Join(",", selectionAccountIds));
                        foreach (var dim in selectedDims.Where(w => w.IsInternal).OrderBy(o => o.Level).Take(1))
                        {
                            var level = dim.Level;
                            var accountsOnTopLevel = accounts.Where(w => w.AccountDimId == dim.AccountDimId && selectionAccountIds.Contains(w.AccountId)).ToList();
                            if (!accountsOnTopLevel.Any())
                            {
                                var fromChildParent = entities.Account.Where(w => selectionAccountIds.Contains(w.AccountId) && w.ParentAccountId.HasValue).Select(s => s.ParentAccountId.Value).ToList();
                                accountsOnTopLevel = accounts.Where(w => w.AccountDimId == dim.AccountDimId && fromChildParent.Contains(w.AccountId)).ToList();
                            }

                            if (selectedDims.Count == 2)
                            {
                                foreach (var account in accountsOnTopLevel)
                                {
                                    var children = AccountManager.GetAllChildrenAccounts(entities, reportResult.ActorCompanyId, account.AccountId);
                                    children = children.Where(a => selectionAccountIds.Contains(a.AccountId)).ToList();

                                    foreach (var selectionDim in selectedDims.OrderBy(o => o.Level))
                                    {
                                        var childrenOnDim = children.Where(c => c.AccountDimId == selectionDim.AccountDimId).ToList();
                                        foreach (var child in childrenOnDim)
                                        {
                                            staffingStatisticsGroups.Add(new StaffingStatisticsGroup() { AccountId = child.AccountId, ConnectedAccountIds = new List<int>() { account.AccountId } });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var topAccount in accountsOnTopLevel)
                                {
                                    staffingStatisticsGroups.Add(new StaffingStatisticsGroup() { AccountId = topAccount.AccountId });
                                }
                            }
                        }
                    }

                    var staffingStatisticsGroupItem = TimeScheduleManager.GetStaffingStatisticsGroupItem(staffingStatisticsGroups, selectionEmployeeIds, selectionDateFrom, selectionDateTo, loadSchedule, loadTemplateSchedule, (loadSales || loadBudget || loadForecast), loadTime, accounts, null, null, employees, cacheKey);
                    DateTime validTo = DateTime.UtcNow.AddSeconds(employees.Count);
                    ExtensionCache.Instance.AddToEmployeePayrollGroupExtensionCaches(base.ActorCompanyId, employeeGroups, payrollGroups, payrollPriceTypes, annualLeaveGroups, validTo);
                    StaffingStatisticsEngine engine = new StaffingStatisticsEngine(null, this.parameterObject);
                    var shiftTypeDTOs = base.GetShiftTypesFromCache(entities, CacheConfig.Company(base.ActorCompanyId), loadSkills: true, loadAccounts: true).ToDTOs(includeSkills: true, setAccountInternalIds: true).ToList();
                    var iDTO = TimeScheduleManager.SetupEvaluatePayrollPriceFormulaInputDTO(selectionEmployeeIds, employeeGroups, payrollGroups, Guid.NewGuid().ToString(), reportId: reportResult.ReportId);
                    iDTO.PayrollProductReportSettings = base.GetPayrollProductReportSettingsForCompanyFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId));
                    var fixedPayrollRows = EmployeeManager.GetEmployeeFixedPayrollRows(entities, base.ActorCompanyId, employees, selectionDateFrom, selectionDateTo, iDTO);
                    int iteration = 0;
                    var scheduleAndTimeShiftsInInterval = scheduleAndTime && DateTime.Today > selectionDateFrom && DateTime.Today < selectionDateTo;

                    if (staffingStatisticsGroups.Count > 1000)
                        LogCollector.LogInfo($"StaffingneedsReportData more than 1000 {base.Company.Name} {reportResult.ActorCompanyId}");

                    foreach (var group in staffingStatisticsGroups)
                    {
                        iteration++;
                        var account = accounts.FirstOrDefault(f => f.AccountId == group.AccountId);

                        if (iteration % 500 == 0)
                            LogCollector.LogInfo($"StaffingneedsReportData {base.Company.Name} {reportResult.ActorCompanyId} iteration {iteration} of {staffingStatisticsGroups.Count} in grouplist");

                        if (account == null)
                            continue;

                        if (originalselectionAccountIds.Any() && accounts.Any(w => account.AccountDimId == w.AccountDimId && originalselectionAccountIds.Contains(w.AccountId)) && !originalselectionAccountIds.Contains(account.AccountId))
                            continue;

                        staffingStatisticsGroupItem.StaffingStatisticsRowsDict.TryGetValue(group.Group, out var row);

                        if (row != null && !row.HasData)
                            continue;

                        if (group.AccountId < 0 && group.ConnectedAccountIds.IsNullOrEmpty() && staffingStatisticsGroups.Any(a => a.AccountId > 0))
                            continue;

                        var staffingNeedsHeadssInterval = TimeScheduleManager.GenerateStaffingNeedsHeadsForInterval(TermGroup_StaffingNeedHeadsFilterType.None, TermGroup_TimeSchedulePlanningFollowUpCalculationType.All, selectionDateFrom, selectionDateTo, account.AccountDimId, account.AccountId,
                            calculateNeed: (loadSales || loadBudget || loadForecast),
                            calculateFrequency: (loadSales || loadBudget || loadForecast),
                            calculateRowFrequency: (loadSales || loadBudget || loadForecast),
                            calculateBudget: loadBudget,
                            calculateForecast: loadForecast,
                            calculateTemplate: loadTemplateSchedule,
                            calculateSchedule: loadSchedule,
                            calculateTime: loadTime,
                            employeeIds: selectionEmployeeIds,
                            addSummaryRow: false,
                            includeEmpTaxAndSuppCharge: true,
                            filterAccountIds: group.ConnectedAccountIds,
                            accounts: accounts,
                            trustBoolOverCalculationTypeAll: true,
                            payrollProductSettings: payrollProductSettings,
                            employees: employees,
                            employeeDTOs: employeeDTOs,
                            cacheKey: cacheKey,
                            row: row,
                            engine: engine,
                            employeeGroups: employeeGroups,
                            payrollGroups: payrollGroups,
                            vacationGroups: vacationGroups,
                            payrollPriceTypes: payrollPriceTypes,
                            shiftTypeDTOs: shiftTypeDTOs,
                            accountDims: dims,
                            fixedPayrollRows: fixedPayrollRows,
                            reportId: reportResult.ReportId,
                            payrollProductReportSettings: iDTO.PayrollProductReportSettings,
                            noInterval: !hasDate && !scheduleAndTimeShiftsInInterval,
                            tryToGroupOnEmployee: tryToGroupOnEmployee,
                            annualLeaveGroups: annualLeaveGroups
                            );

                        if (hasDate)
                        {
                            foreach (var interval in staffingNeedsHeadssInterval.Where(w => w.Interval.Date >= selectionDateFrom && w.Interval.Date <= selectionDateTo))
                            {
                                StaffingStatisticsItem item = new StaffingStatisticsItem()
                                {
                                    Date = interval.Interval.Date,
                                    AccountNr = account.AccountNr,
                                    AccountName = account.Name,
                                    BudgetBPAT = interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT),
                                    BudgetFPAT = interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT),
                                    BudgetLPAT = interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT),
                                    BudgetHours = interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours),
                                    BudgetPersonelCost = interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost),
                                    BudgetSalaryPercent = interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent),
                                    BudgetSales = interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales),
                                    ForecastBPAT = interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT),
                                    ForecastFPAT = interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT),
                                    ForecastLPAT = interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT),
                                    ForecastHours = interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours),
                                    ForecastPersonelCost = interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost),
                                    ForecastSalaryPercent = interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent),
                                    ForecastSales = interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales),
                                    ScheduleBPAT = interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT),
                                    ScheduleFPAT = interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT),
                                    ScheduleLPAT = interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT),
                                    ScheduleHours = interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours),
                                    SchedulePersonelCost = interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost),
                                    ScheduleSalaryPercent = interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent),
                                    ScheduleSales = interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales),
                                    TemplateScheduleBPAT = interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT),
                                    TemplateScheduleFPAT = interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT),
                                    TemplateScheduleLPAT = interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT),
                                    TemplateScheduleHours = interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours),
                                    TemplateSchedulePersonelCost = interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost),
                                    TemplateScheduleSalaryPercent = interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent),
                                    TemplateScheduleSales = interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales),
                                    TimeBPAT = interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT),
                                    TimeFPAT = interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT),
                                    TimeLPAT = interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT),
                                    TimeHours = interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours),
                                    TimePersonelCost = interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost),
                                    TimeSalaryPercent = interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent),
                                    TimeSales = interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales),
                                    ScheduleAndTimeHours = interval.GetScheduleAndTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours),
                                    ScheduleAndTimePersonalCost = interval.GetScheduleAndTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost),
                                    AccountAnalysisFields = new List<AccountAnalysisField>()
                                };

                                if (interval.EmployeeId.HasValue)
                                {
                                    var employee = employees.FirstOrDefault(e => e.EmployeeId == interval.EmployeeId.Value);
                                    if (employee != null)
                                        item.EmployeeName = employee.NumberAndName;
                                }

                                if (!NumberUtility.CheckIfAllIntAndDecimalPropertiesAreZeroOrNull(item))
                                {
                                    foreach (var accountId in group.ConnectedAccountIds)
                                    {
                                        var acc = accounts.FirstOrDefault(a => a.AccountId == accountId && a.AccountDimId != account.AccountDimId);
                                        if (acc != null)
                                            item.AccountAnalysisFields.Add(new AccountAnalysisField(acc));
                                    }

                                    item.AccountAnalysisFields.Add(new AccountAnalysisField(account));
                                    staffingStatisticsItems.Add(item);
                                }
                            }
                        }
                        else
                        {
                            foreach (var intervals in staffingNeedsHeadssInterval.GroupBy(g => g.EmployeeId))
                            {
                                StaffingStatisticsItem item = new StaffingStatisticsItem()
                                {
                                    Date = intervals.First().Interval.Date,
                                    AccountNr = account.AccountNr,
                                    AccountName = account.Name,
                                    BudgetBPAT = intervals.Average(interval => interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)),
                                    BudgetFPAT = intervals.Average(interval => interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)),
                                    BudgetLPAT = intervals.Average(interval => interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)),
                                    BudgetHours = intervals.Sum(interval => interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)),
                                    BudgetPersonelCost = intervals.Sum(interval => interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)),
                                    BudgetSalaryPercent = intervals.Average(interval => interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)),
                                    BudgetSales = intervals.Sum(interval => interval.GetBudgetValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)),
                                    ForecastBPAT = intervals.Average(interval => interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)),
                                    ForecastFPAT = intervals.Average(interval => interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)),
                                    ForecastLPAT = intervals.Average(interval => interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)),
                                    ForecastHours = intervals.Sum(interval => interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)),
                                    ForecastPersonelCost = intervals.Sum(interval => interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)),
                                    ForecastSalaryPercent = intervals.Average(interval => interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)),
                                    ForecastSales = intervals.Sum(interval => interval.GetForecastValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)),
                                    ScheduleBPAT = intervals.Average(interval => interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)),
                                    ScheduleFPAT = intervals.Average(interval => interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)),
                                    ScheduleLPAT = intervals.Average(interval => interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)),
                                    ScheduleHours = intervals.Sum(interval => interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)),
                                    SchedulePersonelCost = intervals.Sum(interval => interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)),
                                    ScheduleSalaryPercent = intervals.Average(interval => interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)),
                                    ScheduleSales = intervals.Sum(interval => interval.GetScheduleValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)),
                                    ScheduleAndTimeHours = intervals.Sum(interval => interval.GetScheduleAndTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)),
                                    ScheduleAndTimePersonalCost = intervals.Sum(interval => interval.GetScheduleAndTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)),
                                    TimeSales = intervals.Sum(interval => interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)),
                                    TimeHours = intervals.Sum(interval => interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)),
                                    TimePersonelCost = intervals.Sum(interval => interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)),
                                    TimeSalaryPercent = intervals.Sum(interval => interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)),
                                    TimeLPAT = intervals.Sum(interval => interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)),
                                    TimeFPAT = intervals.Sum(interval => interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)),
                                    TimeBPAT = intervals.Sum(interval => interval.GetTimeValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)),
                                    TemplateScheduleBPAT = intervals.Average(interval => interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT)),
                                    TemplateScheduleFPAT = intervals.Average(interval => interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)),
                                    TemplateScheduleLPAT = intervals.Average(interval => interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)),
                                    TemplateScheduleHours = intervals.Sum(interval => interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)),
                                    TemplateSchedulePersonelCost = intervals.Sum(interval => interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)),
                                    TemplateScheduleSalaryPercent = intervals.Average(interval => interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)),
                                    TemplateScheduleSales = intervals.Sum(interval => interval.GetTemplateValue(group.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)),
                                    AccountAnalysisFields = new List<AccountAnalysisField>()
                                };


                                if (intervals.First().EmployeeId.HasValue)
                                {
                                    var employee = employees.FirstOrDefault(e => e.EmployeeId == intervals.First().EmployeeId.Value);
                                    if (employee != null)
                                        item.EmployeeName = employee.NumberAndName;
                                }

                                if (!NumberUtility.CheckIfAllIntAndDecimalPropertiesAreZeroOrNull(item))
                                {
                                    foreach (var accountId in group.ConnectedAccountIds)
                                    {
                                        var acc = accounts.FirstOrDefault(a => a.AccountId == accountId && a.AccountDimId != account.AccountDimId);
                                        if (acc != null)
                                            item.AccountAnalysisFields.Add(new AccountAnalysisField(acc));
                                    }

                                    item.AccountAnalysisFields.Add(new AccountAnalysisField(account));
                                    staffingStatisticsItems.Add(item);
                                }
                            }
                        }
                    }

                    #endregion

                    #region Content

                    _reportDataOutput.StaffingStatisticsItems = staffingStatisticsItems.OrderBy(o => o.Date).ToList();

                    #endregion
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        public List<StaffingStatisticsGroup> GetStaffingStatisticsGroups(CompEntities entities, MatrixColumnsSelectionDTO matrixColumnsSelection, List<int> selectionAccountIds, List<AccountDimDTO> selectedDims, List<AccountDimDTO> dims, List<AccountDTO> accounts, bool useAccountHierarchy, bool addEmptyAccounts)
        {
            List<StaffingStatisticsGroup> groupList = new List<StaffingStatisticsGroup>();
            List<int> selectionDimIds = selectedDims.Select(s => s.AccountDimId).ToList();
            var selectionDims = dims.Where(w => selectionDimIds.Contains(w.AccountDimId)).OrderBy(o => o.Level).ToList();
            var selectionAccounts = accounts.Where(w => selectionAccountIds.Contains(w.AccountId)).ToList();
            var topDim = selectionDims.FirstOrDefault() ?? dims.OrderBy(o => o.Level).FirstOrDefault();
            var bottomDim = selectionDims.LastOrDefault() ?? dims.OrderBy(o => o.Level).FirstOrDefault();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            if (addEmptyAccounts)
            {
                var groupOnDim = accounts.GroupBy(g => g.AccountDimId);
                List<AccountDTO> emptyAccounts = new List<AccountDTO>();

                foreach (var dim in groupOnDim.Where(w => w.Key == bottomDim.AccountDimId))
                {
                    var emptyAccount = new AccountDTO() { AccountDimId = dim.Key, Name = "", AccountNr = "0", AccountId = -1 * dim.Key };
                    var otherAccountsOnSameDIm = accounts.Where(w => w.AccountDimId == dim.Key).ToList();
                    if (otherAccountsOnSameDIm.All(a => a.ParentAccountId.HasValue))
                    {
                        foreach (var item in otherAccountsOnSameDIm.GroupBy(g => g.ParentAccountId).Take(1))
                        {
                            var clone = emptyAccount.CloneDTO();
                            clone.ParentAccountId = item.Key;
                            emptyAccounts.Add(clone);
                        }
                    }
                    else
                        emptyAccounts.Add(emptyAccount);

                    selectionAccountIds.Add((-1 * dim.Key));
                }

                accounts.AddRange(emptyAccounts);
            }

            if (!useAccountHierarchy)
            {
                var categoryAccounts = CategoryManager.GetCategoryAccountsByCompany(entities, reportResult.ActorCompanyId);
                var categoryDict = CategoryManager.GetCategoriesForRoleFromTypeDict(entities, reportResult.ActorCompanyId, reportResult.UserId, 0, SoeCategoryType.Employee, true, false, false, selectionDateFrom, selectionDateTo);
                selectionAccountIds = categoryAccounts.Where(w => categoryDict.Select(s => s.Key).Contains(w.CategoryId))?.Select(s => s.AccountId).Distinct().ToList() ?? new List<int>();

                if (!selectionAccountIds.IsNullOrEmpty() && !accounts.Any(w => selectionDimIds.Contains(w.AccountDimId) && selectionAccountIds.Contains(w.AccountId)))
                    categoryAccounts = null;

                if (categoryAccounts.IsNullOrEmpty())
                {
                    if (selectedDims.Any())
                    {
                        foreach (var dim in selectedDims.Where(w => w.Level != 0).OrderByDescending(o => o.Level).Take(1))
                        {
                            var accountsOnDim = accounts.Where(w => w.AccountDimId == dim.AccountDimId);

                            foreach (var account in accountsOnDim)
                            {
                                groupList.Add(new StaffingStatisticsGroup() { Info = account.DimNameNumberAndName, AccountId = account.AccountId });
                            }
                        }
                    }
                    else if (!selectedDims.Any())
                    {
                        foreach (var dim in dims.Where(w => w.Level != 0).OrderBy(o => o.Level).Take(1))
                        {
                            var accountsOnDim = accounts.Where(w => w.AccountDimId == dim.AccountDimId);

                            foreach (var account in accountsOnDim)
                            {
                                groupList.Add(new StaffingStatisticsGroup() { Info = account.DimNameNumberAndName, AccountId = account.AccountId, ConnectedAccountIds = accounts.Where(w => w.AccountDimId != account.AccountDimId).Select(s => s.AccountId).ToList() });
                            }
                        }
                    }
                    else
                    {
                        var shiftTypes = base.GetShiftTypesFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId), loadAccounts: true);
                        var shiftTypesForUser = TimeScheduleManager.GetShiftTypesForUser(entities, reportResult.ActorCompanyId, base.RoleId, base.UserId, 0, true, true);
                        var shiftTypeDim = base.GetShiftTypeAccountDimFromCache(entities, reportResult.ActorCompanyId);

                        foreach (var stfu in shiftTypesForUser)
                        {
                            var match = shiftTypes.FirstOrDefault(f => f.ShiftTypeId == stfu.ShiftTypeId);

                            if (match?.AccountInternal != null)
                            {
                                if (shiftTypeDim == null)
                                    shiftTypeDim = selectedDims.FirstOrDefault(f => f.Name.ToLower().Contains("passt"));

                                if (shiftTypeDim == null)
                                    shiftTypeDim = dims.FirstOrDefault(f => f.Name.ToLower().Contains("passt"));


                                var accountsOnWhithSelectionDimId = accounts.Where(w => match.AccountInternal.Select(s => s.AccountId).Contains(w.AccountId) && (shiftTypeDim == null || w.AccountDimId == shiftTypeDim.AccountDimId)).ToList();

                                foreach (var acc in accountsOnWhithSelectionDimId)
                                {
                                    groupList.Add(new StaffingStatisticsGroup() { Info = acc.DimNameNumberAndName, AccountId = acc.AccountId, ConnectedAccountIds = match.AccountInternal.Where(w => w.AccountId != acc.AccountId).Select(s => s.AccountId).Distinct().ToList() });
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var accountId in selectionAccountIds)
                    {
                        var matchingAccount = accounts.FirstOrDefault(f => f.AccountId == accountId);
                        if (matchingAccount != null)
                            groupList.Add(new StaffingStatisticsGroup() { Info = matchingAccount.DimNameNumberAndName, AccountId = accountId, ConnectedAccountIds = accounts.Where(w => w.AccountDimId != matchingAccount.AccountDimId).Select(s => s.AccountId).Distinct().ToList() });
                    }
                }

                groupList = groupList.GroupBy(g => g.Group).Select(s => s.First()).ToList();
            }
            else
            {
                List<Tuple<int, List<AccountHierarchyItem>>> accountHierarchyItemRows = new List<Tuple<int, List<AccountHierarchyItem>>>();
                AccountHierarchyInput input = AccountHierarchyInput.GetInstance();

                if (selectionDimIds.IsNullOrEmpty())
                {
                    if (!selectionAccountIds.IsNullOrEmpty())
                    {
                        if (selectionAccounts.IsNullOrEmpty())
                            selectionAccounts = accounts.Where(w => selectionAccountIds.Contains(w.AccountId)).ToList();

                        if (selectionAccounts.Any())
                            selectionDimIds.Add(selectionAccounts.First().AccountDimId);
                    }
                    else
                    {
                        var hier = AccountManager.GetAccountHierarchyRepositoryByUserSetting(entitiesReadOnly, base.ActorCompanyId, base.RoleId, base.UserId, input: input);
                        var accs = hier.GetAccounts(false);
                        if (!accs.IsNullOrEmpty())
                            selectionDimIds = new List<int>() { accs.First().AccountDimId };

                        if (selectionAccountIds.IsNullOrEmpty() && !accs.IsNullOrEmpty())
                            selectionAccountIds.Add(accs.First().AccountId);
                    }
                }

                if (selectionAccountIds.IsNullOrEmpty() && !selectionDimIds.IsNullOrEmpty())
                    selectionAccountIds = accounts.Where(w => selectionDimIds.Contains(w.AccountDimId)).Select(s => s.AccountId).Distinct().ToList();
                else if (selectionAccountIds.All(a => a < 0))
                {
                    var hier = AccountManager.GetAccountHierarchyRepositoryByUserSetting(entitiesReadOnly, base.ActorCompanyId, base.RoleId, base.UserId, input: input);
                    var accs = hier.GetAccounts(false);
                    var addAccountIds = accs.Where(w => selectionDimIds.Contains(w.AccountDimId)).Select(s => s.AccountId).Distinct().ToList();
                    selectionAccountIds.AddRange(addAccountIds);
                }

                List<AccountHierarchyReportDataField> accountHierarchyFields = new List<AccountHierarchyReportDataField>();
                foreach (var col in matrixColumnsSelection.Columns)
                {
                    var idValue = GetIdValueFromColumn(col);

                    if (idValue != 0 && col.Options?.Key != null && int.TryParse(col.Options.Key, out int value))
                    {
                        EnumUtility.GetValue(col.Field.FirstCharToUpperCase(), out int id, typeof(TermGroup_StaffingStatisticsMatrixColumns), col.Options?.Key == null ? "" : col.Options.Key);

                        if (id != 0)
                        {
                            var staffingStatisticsMatrixColumns = (TermGroup_StaffingStatisticsMatrixColumns)id;
                            var clone = col.CloneDTO();
                            if (staffingStatisticsMatrixColumns == TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNames || staffingStatisticsMatrixColumns == TermGroup_StaffingStatisticsMatrixColumns.AccountInternalNrs)
                            {

                                clone.Field = col.Field.Replace("Internal", "");
                                clone.Field = clone.Field.Replace("Nrs", "Numbers");
                                accountHierarchyFields.Add(new AccountHierarchyReportDataField(clone));
                            }
                        }
                    }
                }
                var acccountRepo = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId) ? AccountManager.GetAccountHierarchyRepositoryByUser(entitiesReadOnly, base.ActorCompanyId, base.UserId, selectionDateFrom, selectionDateTo) : null;
                foreach (var dim in selectionDims)
                {
                    var accountsOnLevel = accounts.Where(w => w.AccountDimId == dim.AccountDimId && w.AccountId > 0 && selectionAccountIds.Contains(w.AccountId)).Select(s => s.AccountId).ToList();

                    if (accountsOnLevel.IsNullOrEmpty())
                    {
                        var additionalAccountIds = new List<int>();
                        foreach (var item in selectionAccountIds)
                        {
                            var internalAndParents = AccountManager.GetAccountInternalAndParents(entitiesReadOnly, item, ActorCompanyId);

                            if (!internalAndParents.IsNullOrEmpty())
                            {
                                foreach (var groupOnDim in internalAndParents.GroupBy(g => g.AccountDimId))
                                {
                                    if (!selectionAccounts.Any(a => a.AccountDimId == groupOnDim.Key))
                                        additionalAccountIds.AddRange(groupOnDim.Select(a => a.AccountId));
                                }
                            }

                            var accountsFromHierarchyById = AccountManager.GetAccountsFromHierarchyById(ActorCompanyId, item, input);

                            if (!accountsFromHierarchyById.IsNullOrEmpty())
                            {
                                foreach (var groupOnDim in accountsFromHierarchyById.GroupBy(g => g.AccountDimId))
                                {
                                    if (!selectionAccounts.Any(a => a.AccountDimId == groupOnDim.Key))
                                        additionalAccountIds.AddRange(groupOnDim.Select(a => a.AccountId));
                                }
                            }
                        }

                        if (acccountRepo != null && !additionalAccountIds.Any() && base.IsMartinServera())
                        {
                            var accountsOnDim = acccountRepo.GetAccounts(false)?.Where(w => w.AccountDimId == dim.AccountDimId);
                            if (!accountsOnDim.IsNullOrEmpty())
                                additionalAccountIds.AddRange(accountsOnDim.Select(s => s.AccountId));
                        }

                        selectionAccountIds.AddRange(additionalAccountIds.Distinct());
                    }
                }

                AccountHierarchyReportData accountHierarchyReportData = new AccountHierarchyReportData(parameterObject, reportDataInput: new AccountHierarchyReportDataInput(reportResult, accountHierarchyFields, dims, accounts, selectionAccountIds));
                AccountHierarchyReportDataOutput output = accountHierarchyReportData.CreateOutput(reportResult, true);

                var accountIdsOnTopLevel = accounts.Where(w => w.AccountDimId == topDim.AccountDimId && selectionAccountIds.Contains(w.AccountId)).Select(s => s.AccountId).ToList();


                foreach (var rowNr in output.AccountHierarchyItems.GroupBy(g => g.Item1))
                {
                    var accountOnDim = rowNr.SelectMany(s => s.Item2).FirstOrDefault(w => w.AccountField.AccountDimId == bottomDim.AccountDimId && selectionAccountIds.Contains(w.AccountField.AccountId));

                    if (accountOnDim != null)
                    {
                        var connectedAccountIds = rowNr.SelectMany(s => s.Item2).Where(w => w.AccountField.AccountDimId != bottomDim.AccountDimId).Select(s => s.AccountField.AccountId).ToList();
                        var accountsFromHierarchy = AccountManager.GetAccountsFromHierarchyById(base.ActorCompanyId, accountOnDim.AccountField.AccountId, input).Where(w => w.AccountDimId != bottomDim.AccountDimId);
                        connectedAccountIds.AddRange(accountsFromHierarchy.Select(s => s.AccountId).Distinct().ToList());
                        groupList.Add(new StaffingStatisticsGroup() { Info = accountOnDim.AccountField.AccountDimName + " c:" + string.Join("_", accountsFromHierarchy.Select(s => s.Name)), AccountId = accountOnDim.AccountField.AccountId, ConnectedAccountIds = connectedAccountIds });
                    }

                }
                if (groupList.Any())
                    return groupList.GroupBy(g => g.Group).Select(s => s.First()).ToList();
                else if (!accountHierarchyFields.Any() && selectionDimIds.Any() && selectionAccountIds.Any())
                    return new List<StaffingStatisticsGroup>() { new StaffingStatisticsGroup() { AccountId = selectionAccountIds.First() } };

                foreach (var accountHierarchyItem in output.AccountHierarchyItems)
                {
                    List<MatrixField> fields = new List<MatrixField>();
                    var itemCount = 0;
                    foreach (AccountHierarchyItem item in accountHierarchyItem.Item2)
                    {
                        if (itemCount == 0 && selectionDimIds.Contains(item.AccountField.AccountDimId) && selectionAccountIds.Contains(item.AccountField.AccountId))
                            accountHierarchyItemRows.Add(Tuple.Create(accountHierarchyItem.Item1, accountHierarchyItem.Item2));
                        itemCount++;
                    }
                }

                int row = 0;
                var numberOfDims = accountHierarchyItemRows.SelectMany(sm => sm.Item2).SelectMany(s => s.GetHierarchyItems(ref row)).Select(s => s.AccountField.AccountDimId).Distinct().Count();

                foreach (var accountHierarchyItemRow in accountHierarchyItemRows)
                {
                    List<AccountAnalysisField> parents = new List<AccountAnalysisField>();
                    StaffingStatisticsGroup group = new StaffingStatisticsGroup();
                    foreach (AccountHierarchyItem accountHierarchyItem in accountHierarchyItemRow.Item2)
                    {
                        if (accountHierarchyItem.ChildrenAccountHierarchyItems.Any())
                        {
                            parents.Add(accountHierarchyItem.AccountField);
                            foreach (var childLevel1Item in accountHierarchyItem.ChildrenAccountHierarchyItems)
                            {
                                if (childLevel1Item.ChildrenAccountHierarchyItems.Any())
                                {
                                    parents.Add(childLevel1Item.AccountField);
                                    foreach (var childLevel2Item in childLevel1Item.ChildrenAccountHierarchyItems)
                                    {
                                        if (childLevel2Item.ChildrenAccountHierarchyItems.Any())
                                        {
                                            parents.Add(childLevel2Item.AccountField);
                                            foreach (var childLevel3Item in childLevel2Item.ChildrenAccountHierarchyItems)
                                            {

                                                if (childLevel3Item.ChildrenAccountHierarchyItems.Any())
                                                {
                                                    parents.Add(childLevel3Item.AccountField);
                                                    foreach (var childLevel4Item in childLevel3Item.ChildrenAccountHierarchyItems)
                                                    {
                                                        if (childLevel4Item.ChildrenAccountHierarchyItems.Any())
                                                        {
                                                            parents.Add(childLevel4Item.AccountField);
                                                            foreach (var childLevel5Item in childLevel4Item.ChildrenAccountHierarchyItems)
                                                            {

                                                                if (childLevel5Item.ChildrenAccountHierarchyItems.Any())
                                                                {
                                                                    parents.Add(childLevel5Item.AccountField);
                                                                }
                                                                else if (numberOfDims == 6)
                                                                {
                                                                    groupList.Add(new StaffingStatisticsGroup() { AccountId = childLevel5Item.AccountField.AccountId, ConnectedAccountIds = parents.Where(w => w.AccountId != childLevel5Item.AccountField.AccountId).Select(s => s.AccountId).ToList() });
                                                                }
                                                            }
                                                        }
                                                        else if (numberOfDims == 5)
                                                        {
                                                            groupList.Add(new StaffingStatisticsGroup() { AccountId = childLevel4Item.AccountField.AccountId, ConnectedAccountIds = parents.Where(w => w.AccountId != childLevel4Item.AccountField.AccountId).Select(s => s.AccountId).ToList() });
                                                        }
                                                    }
                                                }
                                                else if (numberOfDims == 4)
                                                {
                                                    groupList.Add(new StaffingStatisticsGroup() { AccountId = childLevel3Item.AccountField.AccountId, ConnectedAccountIds = parents.Where(w => w.AccountId != childLevel3Item.AccountField.AccountId).Select(s => s.AccountId).ToList() });
                                                }
                                            }
                                        }
                                        else if (numberOfDims == 3)
                                        {
                                            groupList.Add(new StaffingStatisticsGroup() { AccountId = childLevel2Item.AccountField.AccountId, ConnectedAccountIds = parents.Where(w => w.AccountId != childLevel2Item.AccountField.AccountId).Select(s => s.AccountId).ToList() });
                                        }
                                    }
                                }
                                else if (numberOfDims == 2)
                                {
                                    groupList.Add(new StaffingStatisticsGroup() { AccountId = childLevel1Item.AccountField.AccountId, ConnectedAccountIds = parents.Where(w => w.AccountId != childLevel1Item.AccountField.AccountId).Select(s => s.AccountId).ToList() });
                                }
                            }
                        }
                        else if (numberOfDims == 1)
                        {
                            groupList.Add(new StaffingStatisticsGroup() { AccountId = accountHierarchyItem.AccountField.AccountId, ConnectedAccountIds = parents.Where(w => w.AccountId != accountHierarchyItem.AccountField.AccountId).Select(s => s.AccountId).ToList() });
                        }
                    }
                }
            }
            return groupList.GroupBy(g => g.Group).Select(s => s.First()).ToList();
        }
    }

    public class StaffingStatisticsReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_StaffingStatisticsMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public StaffingStatisticsReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            var key = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_StaffingStatisticsMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_StaffingStatisticsMatrixColumns.Unknown;
        }
    }

    public class StaffingStatisticsGroup
    {
        public string Info { get; set; }
        public List<int> ConnectedAccountIds { get; set; }
        public List<int> ChildrenAccountIds { get; set; }
        public int AccountId { get; set; }

        public StaffingStatisticsGroup()
        {
            this.ConnectedAccountIds = new List<int>();
            this.ChildrenAccountIds = new List<int>();
        }

        public int GetNumberOfMatchingAccountIds(List<int> accountIds)
        {
            return accountIds.Count(s => s == AccountId);
        }

        public string Group
        {
            get
            {
                string parents = string.Join("#", ConnectedAccountIds);
                return $"{AccountId}#{parents}";
            }
        }
    }

    public class StaffingStatisticsReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<StaffingStatisticsReportDataField> Columns { get; set; }

        public StaffingStatisticsReportDataInput(CreateReportResult reportResult, List<StaffingStatisticsReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class StaffingStatisticsReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public StaffingStatisticsReportDataInput Input { get; set; }
        public List<StaffingStatisticsItem> StaffingStatisticsItems { get; set; }

        public StaffingStatisticsReportDataOutput(StaffingStatisticsReportDataInput input)
        {
            Input = input;
            StaffingStatisticsItems = new List<StaffingStatisticsItem>();
        }
    }
}


