using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class AggregatedTimeStatisticsReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly AggregatedTimeStatisticsReportDataInput _reportDataInput;
        private readonly AggregatedTimeStatisticsReportDataOutput _reportDataOutput;

        private bool includeEmpTaxAndSuppCharge;
        private bool includeSupplementCharge;
        private List<PayrollGroup> payrollGroups = new List<PayrollGroup>();
        private List<PayrollGroupAccountStd> PayrollGroupAccountStds;

        private bool LoadAmountOnSchedule
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleGrossAmount ||
                        a.Column == TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleNetAmount);
            }
        }
        private bool GroupOnEmployee
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeName ||
                        a.Column == TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeNr);
            }
        }
        private bool LoadAbsenceCost
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_AggregatedTimeStatisticsMatrixColumns.AbsenceCost);
            }
        }
        private bool LoadVacationCost
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_AggregatedTimeStatisticsMatrixColumns.VacationCost);
            }
        }
        private bool LoadSicknessCost
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_AggregatedTimeStatisticsMatrixColumns.SicknessCost);
            }
        }
        private bool LoadPayrollProductSettings
        {
            get
            {
                return LoadAbsenceCost || LoadVacationCost || LoadSicknessCost;
            }
        }

        public AggregatedTimeStatisticsReportData(ParameterObject parameterObject, AggregatedTimeStatisticsReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new AggregatedTimeStatisticsReportDataOutput(reportDataInput);
        }

        public static List<AggregatedTimeStatisticsReportDataField> GetPossibleDataFields()
        {
            List<AggregatedTimeStatisticsReportDataField> possibleFields = new List<AggregatedTimeStatisticsReportDataField>();
            EnumUtility.GetValues<TermGroup_AggregatedTimeStatisticsMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new AggregatedTimeStatisticsReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public AggregatedTimeStatisticsReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out _))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            TryGetBoolFromSelection(reportResult, out includeEmpTaxAndSuppCharge, "includeEmpTaxAndSuppCharge");
            includeSupplementCharge = includeEmpTaxAndSuppCharge;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Prereq                  

                    #endregion

                    #region Collections

                    var aggregatedTimeStatisticsItems = new ConcurrentBag<AggregatedTimeStatisticsItem>();

                    this.PayrollGroupAccountStds = base.GetPayrollGroupAccountStdFromCache(entities, reportResult.ActorCompanyId);
                    payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId), loadSettings: true);

                    var accounts = AccountManager.GetAccounts(reportResult.ActorCompanyId).ToDTOs(false, false);
                    var validAccountInternals = filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() ? AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).Where(w => selectionAccountIds.Contains(w.AccountId)).ToDTOs() : AccountManager.GetAccountInternals(reportResult.Input.ActorCompanyId, null).ToDTOs();
                    var numberOfDays = CalendarUtility.GetTotalDays(selectionDateFrom, selectionDateTo);
                    using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                    var payrollProductSettings = LoadPayrollProductSettings ? base.GetPayrollProductDTOsWithSettingsFromCache(entitiesReadOnly, CacheConfig.Company(ActorCompanyId)).SelectMany(sm => sm.Settings).ToList() : new List<PayrollProductSettingDTO>();
                    var payrollProductSettingsWithDontIncludeInAbsenceCost = payrollProductSettings.Where(a => a.DontIncludeInAbsenceCost).ToList();
                    var schedules = TimeScheduleManager.GetTimeEmployeeScheduleSmallDTOForReport(selectionDateFrom, selectionDateTo, employees, reportResult.ActorCompanyId, reportResult.RoleId, addAmounts: LoadAmountOnSchedule, removeBreaks: true, includeEmpTaxAndSuppCharge: includeEmpTaxAndSuppCharge);
                    var transactions = TimeTransactionManager.GetTimePayrollTransactionDTOForReport(selectionDateFrom, selectionDateTo, employees.Select(s => s.EmployeeId).ToList(), reportResult.ActorCompanyId);
                    var scheduleTransactions = TimeTransactionManager.GetTimePayrollScheduleTransactionDTOForReport(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.Input.ActorCompanyId);

                    bool hasAnyDontIncludeInAbsenceCostSetting = payrollProductSettingsWithDontIncludeInAbsenceCost.Any();
                    int defaultEmployeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, ActorCompanyId, 0);

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: false, loadContact: true);

                    foreach (var employee in employees)
                    {
                        var employeeTransactions = transactions.GetValue(employee.EmployeeId);
                        if (scheduleTransactions.TryGetValue(employee.EmployeeId, out List<TimePayrollTransactionDTO> schTrans) && schTrans != null)
                            employeeTransactions.AddRange(schTrans);
                        var employeeSchedules = schedules.Where(w => w.EmployeeId == employee.EmployeeId);
                        List<int> employeeScheduleAccountDimIds = defaultEmployeeAccountDimId > 0 ? new List<int>() { defaultEmployeeAccountDimId } : employeeSchedules.SelectMany(s => s.AccountInternals.Select(d => d.AccountDimId)).Distinct().ToList()  ?? new List<int>();
                        
                        var positions = EmployeeManager.GetEmployeePositions(employee.EmployeeId, true);
                        TermGroup_PayrollExportSalaryType salaryType = EmployeeManager.GetEmployeeSalaryType(employee, selectionDateFrom, selectionDateTo);
                        decimal devisor = 0;
                        if (salaryType == TermGroup_PayrollExportSalaryType.Monthly && selectionDateFrom == CalendarUtility.GetBeginningOfMonth(selectionDateFrom) && selectionDateTo.Date == CalendarUtility.GetEndOfMonth(selectionDateTo).Date)
                        {
                            if (!employeeTransactions.IsNullOrEmpty())
                                devisor = employeeTransactions.Any(w => w.IsWorkTime()) ? employeeTransactions.Where(w => w.IsWorkTime()).Sum(s => decimal.Round(decimal.Divide(s.Quantity, 60), 2)) : schedules.Sum(s => s.QuantityHours);
                            else
                                devisor = schedules.Sum(s => s.QuantityHours);
                        }

                        decimal payPerHour = PayrollManager.GetEmployeeHourlyPays(reportResult.ActorCompanyId, employee, selectionDateTo, selectionDateTo, devisor: devisor).FirstOrDefault().Value;
                        DateTime? birthDate = CalendarUtility.GetBirthDateFromSecurityNumber(employee.SocialSec);

                        foreach (var employeeAccountId in validAccountInternals.Where(a => employeeScheduleAccountDimIds.Contains(a.AccountDimId)).Select(a => a.AccountId).Distinct())
                        {
                            AggregatedTimeStatisticsItem item = new AggregatedTimeStatisticsItem();

                            var account = accounts.FirstOrDefault(f => f.AccountId == employeeAccountId);
                            var groupSchedules = employeeSchedules.Where(s => s.AccountId == employeeAccountId).ToList();
                            var grouptransactions = employeeTransactions?.Where(w => w.AccountInternals?.FirstOrDefault(f => f.AccountId == employeeAccountId) != null).ToList() ?? new List<TimePayrollTransactionDTO>();
                            if (!grouptransactions.Any() && (!groupSchedules.Any() || groupSchedules?.Sum(s => s.Quantity) == 0))
                                continue;

                            foreach (var col in _reportDataInput.Columns)
                            {
                                switch (col.Column)
                                {
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.Unknown:
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountDimName:
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountNr:
                                        item.AccountNr = account?.AccountNr ?? "";
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AccountName:
                                        item.AccountName = account?.Name ?? "";
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeNr:
                                        item.EmployeeNr = employee.EmployeeNr;
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeName:
                                        item.EmployeeName = employee.Name;
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeePosition:
                                        item.EmployeePosition = !positions.IsNullOrEmpty() ? string.Join(",", positions?.Select(s => s.Position?.Name)) : string.Empty;
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeWeekWorkHours:
                                        item.EmployeeWeekWorkHours = decimal.Round(decimal.Divide(employee.GetEmployment(selectionDateFrom, selectionDateTo)?.GetWorkTimeWeek(selectionDateFrom) ?? 0, 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.EmployeeSalary:

                                        if (salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                                        {
                                            var payrollGroup = employee.GetEmployment(selectionDateTo)?.GetPayrollGroup(selectionDateTo, payrollGroups);
                                            if (payrollGroup != null)
                                            {
                                                var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                                                if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                                                    item.EmployeeSalary = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employee.GetEmployment(selectionDateTo), null, selectionDateTo, null, null, setting.IntData.Value)?.Amount ?? 0;
                                            }
                                        }
                                        else
                                        {
                                            item.EmployeeSalary = payPerHour;
                                        }
                                        item.EmployeeSalary = AddEmploymentTaxAndSupplementCharge(selectionDateTo, birthDate, employee, item.EmployeeSalary);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.WorkHoursTotal:
                                        item.WorkHoursTotal = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsWorkTime()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHours:
                                        item.InconvinientWorkingHours = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel40:
                                        item.InconvinientWorkingHoursLevel40 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition40()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel57:
                                        item.InconvinientWorkingHoursLevel57 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition57()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel79:
                                        item.InconvinientWorkingHoursLevel79 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition79()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel113:
                                        item.InconvinientWorkingHoursLevel113 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition113()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel50:
                                        item.InconvinientWorkingHoursLevel50 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition50()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel70:
                                        item.InconvinientWorkingHoursLevel70 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition70()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingHoursLevel100:
                                        item.InconvinientWorkingHoursLevel100 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOBAddition100()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHours:
                                        item.AddedTimeHours = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAddedTime()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel35:
                                        item.AddedTimeHoursLevel35 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAddedTimeCompensation35()).Sum(s => s.Quantity), 60), 2);
                                        if (item.AddedTimeHoursLevel35 == 0)
                                            item.AddedTimeHoursLevel35 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAddedTimeCompensation35()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel70:
                                        item.AddedTimeHoursLevel70 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAddedTimeCompensation70()).Sum(s => s.Quantity), 60), 2);
                                        if (item.AddedTimeHoursLevel70 == 0)
                                            item.AddedTimeHoursLevel70 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAddedTimeCompensation70()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeHoursLevel100:
                                        item.AddedTimeHoursLevel100 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAddedTimeCompensation100()).Sum(s => s.Quantity), 60), 2);
                                        if (item.AddedTimeHoursLevel100 == 0)
                                            item.AddedTimeHoursLevel100 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAddedTimeCompensation100()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHours:
                                        item.OverTimeHours = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOverTimeAddition()).Sum(s => s.Quantity), 60), 2);
                                        if (item.OverTimeHours == 0)
                                            item.OverTimeHours = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeCompensation()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel35:
                                        item.OverTimeHoursLevel35 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeAddition35()).Sum(s => s.Quantity), 60), 2);
                                        if (item.OverTimeCostLevel35 == 0)
                                            item.OverTimeHoursLevel35 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeCompensation35()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel50:
                                        item.OverTimeHoursLevel50 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeAddition50()).Sum(s => s.Quantity), 60), 2);
                                        if (item.OverTimeCostLevel50 == 0)
                                            item.OverTimeHoursLevel50 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeCompensation50()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel70:
                                        item.OverTimeHoursLevel70 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeAddition70()).Sum(s => s.Quantity), 60), 2);
                                        if (item.OverTimeCostLevel70 == 0)
                                            item.OverTimeHoursLevel70 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeCompensation70()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeHoursLevel100:
                                        item.OverTimeHoursLevel100 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeAddition100()).Sum(s => s.Quantity), 60), 2);
                                        if (item.OverTimeCostLevel100 == 0)
                                            item.OverTimeHoursLevel100 = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsOvertimeCompensation100()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.SicknessHours:
                                        item.SicknessHours = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAbsenceSick()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.VacationHours:
                                        item.VacationHours = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAbsenceVacation()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AbsenceHours:
                                        item.AbsenceHours = decimal.Round(decimal.Divide(grouptransactions.Where(w => w.IsAbsence()).Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostTotal:
                                        item.CostTotal = decimal.Round(grouptransactions.Where(w => !w.IsFixed && !w.IsNetSalary() && !w.IsTax() && !w.IsMonthlySalary() && (includeEmpTaxAndSuppCharge || !w.IsEmploymentTaxCredit()) &&
                                        (includeSupplementCharge || !w.IsSupplementChargeCredit())).Sum(s => s.Amount), 2);

                                        if (payPerHour > 0 && grouptransactions.Any(w => w.Amount == 0))
                                            item.CostTotal += decimal.Round(decimal.Multiply(decimal.Divide(grouptransactions.Where(w => w.Amount == 0 && w.IsWorkTime()).Sum(s => s.Quantity), 60), payPerHour), 2);

                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostCalenderDayWeek:
                                        if (salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                                        {
                                            var payrollGroup = employee.GetEmployment(selectionDateTo)?.GetPayrollGroup(selectionDateTo, payrollGroups);
                                            if (payrollGroup != null)
                                            {
                                                var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                                                if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                                                {
                                                    var monthly = item.EmployeeSalary = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employee.GetEmployment(selectionDateTo), null, selectionDateTo, null, null, setting.IntData.Value)?.Amount ?? 0;
                                                    if (monthly != 0)
                                                    {
                                                        item.CostCalenderDayWeek = decimal.Round(decimal.Multiply(decimal.Divide(monthly, new decimal(30.4)), 7), 2);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            item.CostCalenderDayWeek = decimal.Round(grouptransactions.Where(w => w.IsWorkTime()).Sum(s => s.Amount), 2);
                                        }
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostCalenderDay:
                                        if (salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                                        {
                                            var payrollGroup = employee.GetEmployment(selectionDateTo)?.GetPayrollGroup(selectionDateTo, payrollGroups);
                                            if (payrollGroup != null)
                                            {
                                                var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                                                if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                                                {
                                                    var monthly = item.EmployeeSalary = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employee.GetEmployment(selectionDateTo), null, selectionDateTo, null, null, setting.IntData.Value)?.Amount ?? 0;
                                                    if (monthly != 0)
                                                    {
                                                        item.CostCalenderDay = decimal.Round(decimal.Multiply(decimal.Divide(monthly, new decimal(30.4)), numberOfDays), 2);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            item.CostCalenderDay = decimal.Round(grouptransactions.Where(w => w.IsWorkTime()).Sum(s => s.Amount), 2);
                                        }

                                        item.CostCalenderDay += decimal.Round(grouptransactions.Where(w => w.IsOBAddition() || w.IsAddedTime() || w.IsOvertimeAddition() || w.IsOvertimeCompensation() || w.IsAbsencePayedAbsence()).Sum(s => s.Amount), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.CostNetHours:
                                        item.CostNetHours = decimal.Round((grouptransactions.Where(w => w.IsWorkTime()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount))), 2);
                                        if (payPerHour > 0 && grouptransactions.Any(w => w.Amount == 0 && w.IsWorkTime()))
                                            item.CostNetHours += decimal.Round(decimal.Multiply(decimal.Divide(grouptransactions.Where(w => w.Amount == 0 && w.IsWorkTime()).Sum(s => s.Quantity), 60), payPerHour), 2);
                                        item.CostNetHours = AddEmploymentTaxAndSupplementCharge(selectionDateTo, birthDate, employee, item.CostNetHours);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCost:
                                        item.InconvinientWorkingCost = decimal.Round(grouptransactions.Where(w => w.IsOBAddition()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel40:
                                        item.InconvinientWorkingCostLevel40 = decimal.Round(grouptransactions.Where(w => w.IsOBAddition40()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel50:
                                        item.InconvinientWorkingCostLevel50 = decimal.Round(grouptransactions.Where(w => w.IsOBAddition50()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel57:
                                        item.InconvinientWorkingCostLevel57 = decimal.Round(grouptransactions.Where(w => w.IsOBAddition57()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel70:
                                        item.InconvinientWorkingCostLevel70 = decimal.Round(grouptransactions.Where(w => w.IsOBAddition70()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel79:
                                        item.InconvinientWorkingCostLevel79 = decimal.Round(grouptransactions.Where(w => w.IsOBAddition79()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel100:
                                        item.InconvinientWorkingCostLevel100 = decimal.Round(grouptransactions.Where(w => w.IsOBAddition100()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.InconvinientWorkingCostLevel113:
                                        item.InconvinientWorkingCostLevel113 = decimal.Round(grouptransactions.Where(w => w.IsOBAddition113()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCost:
                                        item.AddedTimeCost = decimal.Round(grouptransactions.Where(w => w.IsAddedTime()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel35:
                                        item.AddedTimeCostLevel35 = decimal.Round(grouptransactions.Where(w => w.IsAddedTimeCompensation35()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel70:
                                        item.AddedTimeCostLevel70 = decimal.Round(grouptransactions.Where(w => w.IsAddedTimeCompensation70()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AddedTimeCostLevel100:
                                        item.AddedTimeCostLevel100 = decimal.Round(grouptransactions.Where(w => w.IsAddedTimeCompensation100()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCost:
                                        item.OverTimeCost = decimal.Round(grouptransactions.Where(w => w.IsOverTimeAddition() || w.IsOvertimeCompensation()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel35:
                                        item.OverTimeCostLevel35 = decimal.Round(grouptransactions.Where(w => w.IsOvertimeCompensation35() || w.IsOvertimeAddition35()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel50:
                                        item.OverTimeCostLevel50 = decimal.Round(grouptransactions.Where(w => w.IsOvertimeCompensation50() || w.IsOvertimeAddition50()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel70:
                                        item.OverTimeCostLevel70 = decimal.Round(grouptransactions.Where(w => w.IsOvertimeCompensation70() || w.IsOvertimeAddition70()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.OverTimeCostLevel100:
                                        item.OverTimeCostLevel100 = decimal.Round(grouptransactions.Where(w => w.IsOvertimeCompensation100() || w.IsOvertimeAddition100()).Sum(s => AddEmploymentTaxAndSupplementCharge(s.Date, birthDate, employee, s.Amount)), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.SicknessCost:
                                        if (LoadSicknessCost)
                                        {
                                            List<TimePayrollTransactionDTO> sicknessTransactions = grouptransactions.Where(w => w.IsAbsence_SicknessSalary()).ToList();
                                            List<TimePayrollTransactionDTO> validatedTransactions = new List<TimePayrollTransactionDTO>();
                                            decimal taxAndSupplementCharge = 0;

                                            if (!hasAnyDontIncludeInAbsenceCostSetting && !includeEmpTaxAndSuppCharge && !includeSupplementCharge)
                                            {
                                                validatedTransactions = sicknessTransactions;
                                            }
                                            else
                                            {
                                                foreach (var transactionsOnDate in sicknessTransactions.GroupBy(g => g.Date))
                                                {
                                                    bool dontIncludeInAbsenceCost = false;
                                                    var hasSetting = payrollProductSettingsWithDontIncludeInAbsenceCost.Any(a => transactionsOnDate.Select(s => s.PayrollProductId).Contains(a.ProductId));
                                                    var employment = employee.GetEmployment(transactionsOnDate.Key);
                                                    if (employment != null)
                                                    {
                                                        int? payrollGroupId = employment.GetPayrollGroupId(transactionsOnDate.Key);

                                                        foreach (var transaction in transactionsOnDate)
                                                        {
                                                            dontIncludeInAbsenceCost = hasSetting ? payrollProductSettingsWithDontIncludeInAbsenceCost.GetSetting(payrollGroupId, transaction.PayrollProductId)?.DontIncludeInAbsenceCost ?? false : false;
                                                            if (!dontIncludeInAbsenceCost)
                                                            {
                                                                var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                                                var taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, transaction.Date.Value) ?? transaction.Date.Value : transaction.Date.Value;
                                                                taxAndSupplementCharge += includeEmpTaxAndSuppCharge ? PayrollManager.CalculateEmploymentTaxSimple(reportResult.ActorCompanyId, taxDate, transaction.Amount, birthDate) : 0;
                                                                taxAndSupplementCharge += includeSupplementCharge ? PayrollManager.CalculateSupplementChargeSE(reportResult.ActorCompanyId, taxDate, transaction.Amount, payrollGroupId, birthDate, null, base.GetPayrollGroupAccountStdFromCache(entities, reportResult.ActorCompanyId)) : 0;
                                                                validatedTransactions.Add(transaction);
                                                            }
                                                        }
                                                    }
                                                    else
                                                        validatedTransactions.AddRange(transactionsOnDate);
                                                }
                                            }

                                            item.SicknessCost = decimal.Round(validatedTransactions.Sum(s => s.Amount) + taxAndSupplementCharge, 2);
                                        }
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.VacationCost:
                                        if (LoadVacationCost)
                                        {
                                            List<TimePayrollTransactionDTO> vacationTransactions = grouptransactions.Where(w => w.IsVacationCost()).ToList();
                                            List<TimePayrollTransactionDTO> validatedTransactions = new List<TimePayrollTransactionDTO>();
                                            decimal taxAndSupplementCharge = 0;
                                            if (!hasAnyDontIncludeInAbsenceCostSetting && !includeEmpTaxAndSuppCharge && !includeSupplementCharge)
                                            {
                                                validatedTransactions = vacationTransactions;
                                            }
                                            else
                                            {
                                                foreach (var transactionsOnDate in vacationTransactions.GroupBy(g => g.Date))
                                                {
                                                    bool dontIncludeInAbsenceCost = false;
                                                    var hasSetting = payrollProductSettingsWithDontIncludeInAbsenceCost.Any(a => transactionsOnDate.Select(s => s.PayrollProductId).Contains(a.ProductId));
                                                    var employment = employee.GetEmployment(transactionsOnDate.Key);
                                                    if (employment != null)
                                                    {
                                                        int? payrollGroupId = employment.GetPayrollGroupId(transactionsOnDate.Key);

                                                        foreach (var transaction in transactionsOnDate)
                                                        {
                                                            dontIncludeInAbsenceCost = hasSetting ? payrollProductSettingsWithDontIncludeInAbsenceCost.GetSetting(payrollGroupId, transaction.PayrollProductId)?.DontIncludeInAbsenceCost ?? false : false;
                                                            if (!dontIncludeInAbsenceCost)
                                                            {
                                                                var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                                                var taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, transaction.Date.Value) ?? transaction.Date.Value : transaction.Date.Value;
                                                                taxAndSupplementCharge += includeEmpTaxAndSuppCharge ? PayrollManager.CalculateEmploymentTaxSimple(reportResult.ActorCompanyId, taxDate, transaction.Amount, birthDate) : 0;
                                                                taxAndSupplementCharge += includeSupplementCharge ? PayrollManager.CalculateSupplementChargeSE(reportResult.ActorCompanyId, taxDate, transaction.Amount, payrollGroupId, birthDate, null, base.GetPayrollGroupAccountStdFromCache(entities, reportResult.ActorCompanyId)) : 0;
                                                                validatedTransactions.Add(transaction);
                                                            }
                                                        }
                                                    }
                                                    else
                                                        validatedTransactions.AddRange(transactionsOnDate);
                                                }
                                            }

                                            item.VacationCost = decimal.Round(validatedTransactions.Sum(s => s.Amount) + taxAndSupplementCharge, 2);
                                        }
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.AbsenceCost:
                                        if (LoadAbsenceCost)
                                        {
                                            List<TimePayrollTransactionDTO> absencetransactions = grouptransactions.Where(w => w.IsAbsenceCost()).ToList();
                                            List<TimePayrollTransactionDTO> validatedTransactions = new List<TimePayrollTransactionDTO>();
                                            decimal taxAndSupplementCharge = 0;
                                            if (!hasAnyDontIncludeInAbsenceCostSetting && !includeEmpTaxAndSuppCharge && !includeSupplementCharge)
                                            {
                                                validatedTransactions = absencetransactions;
                                            }
                                            else
                                            {
                                                foreach (var transactionsOnDate in absencetransactions.GroupBy(g => g.Date))
                                                {
                                                    bool dontIncludeInAbsenceCost = false;
                                                    var hasSetting = payrollProductSettingsWithDontIncludeInAbsenceCost.Any(a => transactionsOnDate.Select(s => s.PayrollProductId).Contains(a.ProductId));
                                                    var employment = employee.GetEmployment(transactionsOnDate.Key);
                                                    if (employment != null)
                                                    {
                                                        int? payrollGroupId = employment.GetPayrollGroupId(transactionsOnDate.Key);
                                                        foreach (var transaction in transactionsOnDate)
                                                        {
                                                            dontIncludeInAbsenceCost = hasSetting ? payrollProductSettingsWithDontIncludeInAbsenceCost.GetSetting(payrollGroupId, transaction.PayrollProductId)?.DontIncludeInAbsenceCost ?? false : false;
                                                            if (!dontIncludeInAbsenceCost)
                                                            {
                                                                var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                                                var taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, transaction.Date.Value) ?? transaction.Date.Value : transaction.Date.Value;
                                                                taxAndSupplementCharge += includeEmpTaxAndSuppCharge ? PayrollManager.CalculateEmploymentTaxSimple(reportResult.ActorCompanyId, taxDate, transaction.Amount, birthDate) : 0;
                                                                taxAndSupplementCharge += includeSupplementCharge ? PayrollManager.CalculateSupplementChargeSE(reportResult.ActorCompanyId, taxDate, transaction.Amount, payrollGroupId, birthDate, null, base.GetPayrollGroupAccountStdFromCache(entities, reportResult.ActorCompanyId)) : 0;
                                                                validatedTransactions.Add(transaction);
                                                            }
                                                        }
                                                    }
                                                    else
                                                        validatedTransactions.AddRange(transactionsOnDate);
                                                }
                                            }

                                            item.AbsenceCost = decimal.Round(validatedTransactions.Sum(s => s.Amount) + taxAndSupplementCharge, 2);
                                        }
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleNetQuantity:
                                        item.ScheduleNetQuantity = decimal.Round(decimal.Divide(groupSchedules.Sum(s => s.Quantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleGrossQuantity:
                                        item.ScheduleGrossQuantity = decimal.Round(decimal.Divide(groupSchedules.Sum(s => s.GrossQuantity), 60), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleNetAmount:
                                        item.ScheduleNetAmount = decimal.Round(groupSchedules.Sum(s => s.Amount), 2);
                                        break;
                                    case TermGroup_AggregatedTimeStatisticsMatrixColumns.ScheduleGrossAmount:
                                        item.ScheduleGrossAmount = decimal.Round(groupSchedules.Sum(s => s.GrossAmount), 2);
                                        break;
                                    default:
                                        break;
                                }
                            }


                            aggregatedTimeStatisticsItems.Add(item);
                        }
                    }

                    if (!GroupOnEmployee)
                    {
                        List<AggregatedTimeStatisticsItem> aggregatedTimeStatisticsItemsGrouped = new List<AggregatedTimeStatisticsItem>();
                        foreach (var group in aggregatedTimeStatisticsItems.GroupBy(g => g.AccountNr))
                        {
                            AggregatedTimeStatisticsItem item = new AggregatedTimeStatisticsItem()
                            {
                                AccountNr = group.First().AccountNr,
                                AccountName = group.First().AccountName,
                                AccountDimName = group.First().AccountDimName,
                                EmployeeWeekWorkHours = group.Sum(s => s.EmployeeWeekWorkHours),
                                WorkHoursTotal = group.Sum(s => s.WorkHoursTotal),
                                InconvinientWorkingHours = group.Sum(s => s.InconvinientWorkingHours),
                                InconvinientWorkingHoursLevel40 = group.Sum(s => s.InconvinientWorkingHoursLevel40),
                                InconvinientWorkingHoursLevel57 = group.Sum(s => s.InconvinientWorkingHoursLevel57),
                                InconvinientWorkingHoursLevel79 = group.Sum(s => s.InconvinientWorkingHoursLevel79),
                                InconvinientWorkingHoursLevel113 = group.Sum(s => s.InconvinientWorkingHoursLevel113),
                                InconvinientWorkingHoursLevel50 = group.Sum(s => s.InconvinientWorkingHoursLevel50),
                                InconvinientWorkingHoursLevel70 = group.Sum(s => s.InconvinientWorkingHoursLevel70),
                                InconvinientWorkingHoursLevel100 = group.Sum(s => s.InconvinientWorkingHoursLevel100),
                                AddedTimeHours = group.Sum(s => s.AddedTimeHours),
                                AddedTimeHoursLevel35 = group.Sum(s => s.AddedTimeHoursLevel35),
                                AddedTimeHoursLevel70 = group.Sum(s => s.AddedTimeHoursLevel70),
                                AddedTimeHoursLevel100 = group.Sum(s => s.AddedTimeHoursLevel100),
                                OverTimeHours = group.Sum(s => s.OverTimeHours),
                                OverTimeHoursLevel35 = group.Sum(s => s.OverTimeHoursLevel35),
                                OverTimeHoursLevel50 = group.Sum(s => s.OverTimeHoursLevel50),
                                OverTimeHoursLevel70 = group.Sum(s => s.OverTimeHoursLevel70),
                                OverTimeHoursLevel100 = group.Sum(s => s.OverTimeHoursLevel100),
                                SicknessHours = group.Sum(s => s.SicknessHours),
                                VacationHours = group.Sum(s => s.VacationHours),
                                AbsenceHours = group.Sum(s => s.AbsenceHours),
                                CostTotal = group.Sum(s => s.CostTotal),
                                CostNetHours = group.Sum(s => s.CostNetHours),
                                InconvinientWorkingCost = group.Sum(s => s.InconvinientWorkingCost),
                                InconvinientWorkingCostLevel40 = group.Sum(s => s.InconvinientWorkingCostLevel40),
                                InconvinientWorkingCostLevel50 = group.Sum(s => s.InconvinientWorkingCostLevel50),
                                InconvinientWorkingCostLevel57 = group.Sum(s => s.InconvinientWorkingCostLevel57),
                                InconvinientWorkingCostLevel70 = group.Sum(s => s.InconvinientWorkingCostLevel70),
                                InconvinientWorkingCostLevel79 = group.Sum(s => s.InconvinientWorkingCostLevel79),
                                InconvinientWorkingCostLevel100 = group.Sum(s => s.InconvinientWorkingCostLevel100),
                                InconvinientWorkingCostLevel113 = group.Sum(s => s.InconvinientWorkingCostLevel113),
                                AddedTimeCost = group.Sum(s => s.AddedTimeCost),
                                AddedTimeCostLevel35 = group.Sum(s => s.AddedTimeCostLevel35),
                                AddedTimeCostLevel70 = group.Sum(s => s.AddedTimeCostLevel70),
                                AddedTimeCostLevel100 = group.Sum(s => s.AddedTimeCostLevel100),
                                OverTimeCost = group.Sum(s => s.OverTimeCost),
                                OverTimeCostLevel35 = group.Sum(s => s.OverTimeCostLevel35),
                                OverTimeCostLevel50 = group.Sum(s => s.OverTimeCostLevel50),
                                OverTimeCostLevel70 = group.Sum(s => s.OverTimeCostLevel70),
                                OverTimeCostLevel100 = group.Sum(s => s.OverTimeCostLevel100),
                                SicknessCost = group.Sum(s => s.SicknessCost),
                                VacationCost = group.Sum(s => s.VacationCost),
                                AbsenceCost = group.Sum(s => s.AbsenceCost),
                                ScheduleNetQuantity = group.Sum(s => s.ScheduleNetQuantity),
                                ScheduleGrossQuantity = group.Sum(s => s.ScheduleGrossQuantity),
                                ScheduleNetAmount = group.Sum(s => s.ScheduleNetAmount),
                                ScheduleGrossAmount = group.Sum(s => s.ScheduleGrossAmount),
                                CostCalenderDayWeek = group.Sum(s => s.CostCalenderDayWeek),
                                CostCalenderDay = group.Sum(s => s.CostCalenderDay)

                            };

                            aggregatedTimeStatisticsItemsGrouped.Add(item);
                        }

                        _reportDataOutput.AggregatedTimeStatisticsItems = aggregatedTimeStatisticsItemsGrouped.OrderBy(o => o.AccountNr).ToList();
                    }
                    else
                        _reportDataOutput.AggregatedTimeStatisticsItems = aggregatedTimeStatisticsItems.OrderBy(o => o.AccountNr).ThenBy(tb => tb.EmployeeNr).ToList();

                    #endregion
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        private decimal AddEmploymentTaxAndSupplementCharge(DateTime? date, DateTime? birthDate, Employee employee, decimal amount)
        {
            if (!includeEmpTaxAndSuppCharge && !includeSupplementCharge)
                return amount;

            if (!date.HasValue)
                return amount;

            var employment = employee.GetEmployment(date);
            if (employment == null)
                return amount;

            int? payrollGroupId = employment.GetPayrollGroupId(date);
            if (!payrollGroupId.HasValue)
                return amount;

            decimal taxAndSupplementCharge = 0;
            if (includeEmpTaxAndSuppCharge)
                taxAndSupplementCharge += PayrollManager.CalculateEmploymentTaxSimple(reportResult.ActorCompanyId, date.Value, amount, birthDate);
            if (includeSupplementCharge)
                taxAndSupplementCharge += PayrollManager.CalculateSupplementChargeSE(reportResult.ActorCompanyId, date.Value, amount, payrollGroupId, birthDate, null, this.PayrollGroupAccountStds);

            return taxAndSupplementCharge + amount;
        }
    }

    public class AggregatedTimeStatisticsReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_AggregatedTimeStatisticsMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public AggregatedTimeStatisticsReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_AggregatedTimeStatisticsMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_AggregatedTimeStatisticsMatrixColumns.Unknown;
        }
    }

    public class AggregatedTimeStatisticsReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<AggregatedTimeStatisticsReportDataField> Columns { get; set; }

        public AggregatedTimeStatisticsReportDataInput(CreateReportResult reportResult, List<AggregatedTimeStatisticsReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class AggregatedTimeStatisticsReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public AggregatedTimeStatisticsReportDataInput Input { get; set; }
        public List<AggregatedTimeStatisticsItem> AggregatedTimeStatisticsItems { get; set; }

        public AggregatedTimeStatisticsReportDataOutput(AggregatedTimeStatisticsReportDataInput input)
        {
            this.Input = input;
            this.AggregatedTimeStatisticsItems = new List<AggregatedTimeStatisticsItem>();
        }
    }
}

