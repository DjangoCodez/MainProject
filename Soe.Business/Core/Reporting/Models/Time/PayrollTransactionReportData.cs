using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class PayrollTransactionReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly PayrollTransactionReportDataOutput _reportDataOutput;
        private readonly PayrollTransactionReportDataInput _reportDataInput;

        private bool LoadRatio => _reportDataInput.Columns.Any(a => a.Column == TermGroup_PayrollTransactionMatrixColumns.Ratio);
        public PayrollTransactionReportData(ParameterObject parameterObject, PayrollTransactionReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new PayrollTransactionReportDataOutput(reportDataInput);
             _reportDataInput = reportDataInput;
        }

        public static List<PayrollTransactionReportDataReportDataField> GetPossibleDataFields()
        {
            List<PayrollTransactionReportDataReportDataField> possibleFields = new List<PayrollTransactionReportDataReportDataField>();
            EnumUtility.GetValues<TermGroup_PayrollTransactionMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new PayrollTransactionReportDataReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public PayrollTransactionReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            try
            {
                #region Prereq

                if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                    return new ActionResult(false);
                if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectAccountIds, out _))
                    return new ActionResult(false);

                List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, reportResult.Input.ActorCompanyId).Where(o => selectionTimePeriodIds.Contains(o.TimePeriodId)).ToList();

                var doLogging = selectionEmployeeIds.Count > 1000;

                TryGetPayrollProductIdsFromSelections(reportResult, out List<int> selectionPayrollProductIds);
                TryGetBoolFromSelection(reportResult, out bool selectionIncludeStartValues, "includeStartValues");
                TryGetBoolFromSelection(reportResult, out bool ignoreAccounting, "ignoreAccounting");
                TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");

                AccountDim accountDimStd = AccountManager.GetAccountDimStd(reportResult.Input.ActorCompanyId);
                List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(reportResult.Input.ActorCompanyId, true);
                List<AccountInternalDTO> validAccountInternals = filterOnAccounting && !selectAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectAccountIds.Contains(w.AccountId)).ToDTOs() : null;
                string arbetsplatsnummer = SettingManager.GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportSNKFOWorkPlaceNumber, 0, reportResult.Input.ActorCompanyId, 0)?.StrData?.ToString() ?? string.Empty;
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.Input.RoleId, reportResult.Input.ActorCompanyId);
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);
                Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> companyScheduleTransactionDTOs = new Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>>();
               
                #endregion

                #region Load

                if (_reportDataOutput.Employees.IsNullOrEmpty())
                {
                    employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);
                    _reportDataOutput.Employees = employees.ToDTOs(includeEmployments: true).ToList();
                }

                if (LoadRatio)
                {
                    var selectionDateFrom = timePeriods.OrderBy(o => o.PayrollStartDate)?.FirstOrDefault()?.StartDate;
                    var selectionDateTo = timePeriods.OrderBy(o => o.PayrollStartDate)?.LastOrDefault()?.StopDate;

                    if (selectionDateFrom.HasValue && selectionDateTo.HasValue)
                          companyScheduleTransactionDTOs = TimeScheduleManager.GetTimeEmployeeScheduleSmallDTODictForReport(selectionDateFrom.Value, selectionDateTo.Value, employees, reportResult.Input.ActorCompanyId, base.RoleId, shiftTypeIds: null, splitOnBreaks: true, removeBreaks: true);
                }
                if (doLogging) LogCollector.LogInfo($"PayrollTransactionReportData.LoadData: ActorCompanyId {reportResult.Input.ActorCompanyId} Employees: {_reportDataOutput.Employees.Count}");


                _reportDataOutput.PayrollProducts = ProductManager.GetPayrollProducts(reportResult.Input.ActorCompanyId, null, true, true, true).ToDTOs(true, false, true, true, true).ToList();
                _reportDataOutput.TimeUnits = GetTermGroupContent(TermGroup.PayrollProductTimeUnit, addEmptyRow: true);
                List<TimeAccumulator> allAccumulators = TimeAccumulatorManager.GetTimeAccumulators(reportResult.Input.ActorCompanyId);
                if (doLogging) LogCollector.LogInfo($"PayrollTransactionReportData.LoadData: ActorCompanyId {reportResult.Input.ActorCompanyId} prerequisites loaded");
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                _reportDataOutput.TimePayrollStatistics = TimeTransactionManager.GetTimePayrollStatisticsDTOs(entitiesReadOnly, reportResult.Input.ActorCompanyId, employees, selectionTimePeriodIds, 1, null, selectionPayrollProductIds, ignoreAccounting: ignoreAccounting);
                if (doLogging) LogCollector.LogInfo($"PayrollTransactionReportData.LoadData: ActorCompanyId {reportResult.Input.ActorCompanyId} TimePayrollStatistics loaded {_reportDataOutput.TimePayrollStatistics.Count}");
                Dictionary<int, List<EmployeeSkill>> employeeSkillsForCompany = TimeScheduleManager.GetEmployeeSkillsForCompany(reportResult.Input.ActorCompanyId).GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList());
                Dictionary<int, List<EmployeePosition>> employeePositionsForCompany = EmployeeManager.GetEmployeePositionsForCompany(reportResult.Input.ActorCompanyId).GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList());

                List<string> payrollProductNumbers = new List<string>();
                bool hasPayrollProductIds = !selectionPayrollProductIds.IsNullOrEmpty();

                if (selectionIncludeStartValues)
                {
                    int year = timePeriods.OrderBy(x => x.PayrollStartDate).LastOrDefault().PayrollStopDate.Value.Year;

                    foreach (Employee employee in employees)
                    {
                        List<TimePayrollStatisticsDTO> employeeTimePayrollStatisticsDTOs = _reportDataOutput.TimePayrollStatistics.Where(t => t.EmployeeId == employee.EmployeeId).ToList();
                        List<int> skipTransactionIds = employeeTimePayrollStatisticsDTOs.Select(t => t.TimePayrollTransactionId).ToList();
                        _reportDataOutput.TimePayrollStatistics.AddRange(TimeTransactionManager.GetPayrollStartValuesAsTimePayrollStatisticsDTOs(entitiesReadOnly, employee, reportResult.Input.ActorCompanyId, year, false, base.personalDataRepository.PayrollGroups, base.personalDataRepository.EmployeeGroups, skipTransactionIds, ignoreAccounting));
                    }

                    if (doLogging) LogCollector.LogInfo($"PayrollTransactionReportData.LoadData: ActorCompanyId {reportResult.Input.ActorCompanyId} startvalues loaded");
                }

                if (doLogging) LogCollector.LogInfo($"PayrollTransactionReportData.LoadData: ActorCompanyId {reportResult.Input.ActorCompanyId} data loaded");
                if (LoadRatio)
                {
                    foreach (var trans in _reportDataOutput.TimePayrollStatistics.GroupBy(w => w.EmployeeId)){
                        var employeeId = trans.FirstOrDefault().EmployeeId;

                        IEnumerable<IGrouping<string, TimePayrollStatisticsDTO>> transactionGroupByLevel = trans.GroupBy(f => $"{f.SysPayrollTypeLevel1}#{f.SysPayrollTypeLevel2}#{f.SysPayrollTypeLevel3}");
                        companyScheduleTransactionDTOs.TryGetValue(employeeId, out List<TimeEmployeeScheduleDataSmallDTO> scheduleTransactionDTOs);

                        foreach (var ab in transactionGroupByLevel.ToList())
                        {
                            CalculateAndSetAbsenceRatio(scheduleTransactionDTOs, ab.ToList());
                        }
                    }

                }
                #endregion
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "PayrollTransactionReportData.LoadData");
                return new ActionResult(ex);
            }

            return new ActionResult();
        }
        private void CalculateAndSetAbsenceRatio(List<TimeEmployeeScheduleDataSmallDTO> timeEmployeeSchedules, List<TimePayrollStatisticsDTO> items)
        {

            foreach (var transOnDate in items.GroupBy(g => g.TimeBlockDate).OrderBy(o => o.Key))
            {
                if (transOnDate.Where(w => w.IsAbsence()).Sum(s => s.Quantity) != 0 && transOnDate.All(a => a.AbsenceRatio == 0))
                {
                    var scheduleOnDate = timeEmployeeSchedules.Where(f => f.Date == transOnDate.Key).ToList();
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
                        var previousDate = items.Where(w => w.TimeBlockDate < transaction.TimeBlockDate && w.AbsenceRatio != 0).OrderByDescending(o => o.TimeBlockDate).FirstOrDefault()?.TimeBlockDate;
                        var futureDate = items.Where(w => w.TimeBlockDate > transaction.TimeBlockDate && w.AbsenceRatio != 0).OrderBy(o => o.TimeBlockDate).FirstOrDefault()?.TimeBlockDate;

                        if (previousDate != null)
                        {
                            var previousRatio = items.First(f => f.TimeBlockDate == previousDate).AbsenceRatio;
                            transaction.AbsenceRatio = previousRatio;
                        }
                        else if (futureDate != null)
                        {
                            var futureRatio = items.First(f => f.TimeBlockDate == futureDate).AbsenceRatio;
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
}

    public class PayrollTransactionReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_PayrollTransactionMatrixColumns Column { get; set; }
        public string ColumnKey { get; private set; }
        public string OptionKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public PayrollTransactionReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            var key = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.OptionKey = Selection?.Options?.Key;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_PayrollTransactionMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_PayrollTransactionMatrixColumns.Unknown;
        }
    }

    public class PayrollTransactionReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<PayrollTransactionReportDataReportDataField> Columns { get; set; }

        public PayrollTransactionReportDataInput(CreateReportResult reportResult, List<PayrollTransactionReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class PayrollTransactionReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<TimePayrollStatisticsDTO> TimePayrollStatistics { get; set; }
        public List<EmployeeDTO> Employees { get; set; }
        public List<PayrollProductDTO> PayrollProducts { get; set; }
        public List<GenericType> TimeUnits { get; set; }
        public PayrollTransactionReportDataInput Input { get; set; }

        public PayrollTransactionReportDataOutput(PayrollTransactionReportDataInput input)
        {
            this.TimePayrollStatistics = new List<TimePayrollStatisticsDTO>();
            this.Employees = new List<EmployeeDTO>();
            this.PayrollProducts = new List<PayrollProductDTO>();
            this.TimeUnits = new List<GenericType>();
            this.Input = input;
        }
    }
}

