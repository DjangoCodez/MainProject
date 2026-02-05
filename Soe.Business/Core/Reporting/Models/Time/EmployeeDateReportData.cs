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
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class EmployeeDateReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeDateReportDataInput _reportDataInput;
        private readonly EmployeeDateReportDataOutput _reportDataOutput;

        private bool LoadEmployment
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeDateMatrixColumns.EmployeeGroupName ||
                       a.Column == TermGroup_EmployeeDateMatrixColumns.PayrollGroupName ||
                       a.Column == TermGroup_EmployeeDateMatrixColumns.VacationGroupName ||
                       a.Column == TermGroup_EmployeeDateMatrixColumns.EmploymentFte ||
                       a.Column == TermGroup_EmployeeDateMatrixColumns.EmploymentPercent);
            }
        }
        private bool LoadSchedule
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_EmployeeDateMatrixColumns.ScheduleTime ||
                        a.Column == TermGroup_EmployeeDateMatrixColumns.ScheduleAbsenceTime ||
                        a.Column == TermGroup_EmployeeDateMatrixColumns.PercentScheduleAbsenceTime);
            }
        }
        private bool LoadEmployees
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_EmployeeDateMatrixColumns.EmployeeName ||
                        a.Column == TermGroup_EmployeeDateMatrixColumns.EmployeeNr ||
                        a.Column == TermGroup_EmployeeDateMatrixColumns.PercentScheduleAbsenceTime);
            }
        }
        private bool CheckScheduleAbsence
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                        a.Column == TermGroup_EmployeeDateMatrixColumns.ScheduleAbsenceTime ||
                        a.Column == TermGroup_EmployeeDateMatrixColumns.PercentScheduleAbsenceTime);
            }
        }
        private bool LoadDayTypes
        {
            get
            {
                return _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeDateMatrixColumns.DateTypeName);
            }
        }

        public EmployeeDateReportData(ParameterObject parameterObject, EmployeeDateReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeDateReportDataOutput(reportDataInput);
        }

        public static List<EmployeeDateReportDataField> GetPossibleDataFields()
        {
            List<EmployeeDateReportDataField> possibleFields = new List<EmployeeDateReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeDateMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeDateReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeDateReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);
            TryGetBoolFromSelection(reportResult, out bool onlyDateWithChanges, "onlyDateWithChanges");

            bool useExperienceMonthsOnEmploymentAsStartValue = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseEmploymentExperienceAsStartValue, 0, Company.ActorCompanyId, 0);

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

                    ConcurrentBag<EmployeeDateItem> employeeDateItems = new ConcurrentBag<EmployeeDateItem>();
                    ConcurrentBag<DateTime> dateWithChanges = new ConcurrentBag<DateTime>();
                    Dictionary<int, List<TimeScheduleTemplateBlock>> scheduleBlocks = new Dictionary<int, List<TimeScheduleTemplateBlock>>();
                    List<HolidayDTO> companyHolidays = null;
                    List<DayType> dayTypesForCompany = null;

                    List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<PayrollPriceType> payrollPriceTypes = GetPayrollPriceTypesFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<VacationGroup> vacationGroups = GetVacationGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));
                    List<AnnualLeaveGroup> annualLeaveGroups = GetAnnualLeaveGroupsFromCache(entities, CacheConfig.Company(reportResult.ActorCompanyId));

                    if (LoadDayTypes)
                    {
                        companyHolidays = CalendarManager.GetHolidaysByCompany(reportResult.ActorCompanyId, loadDayType: true);
                        dayTypesForCompany = CalendarManager.GetDayTypesByCompany(reportResult.ActorCompanyId, true);
                        employeeGroups = EmployeeManager.GetEmployeeGroups(reportResult.ActorCompanyId, false, false, true);
                    }


                    if (LoadSchedule)
                    {
                        using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                        scheduleBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entitiesReadOnly, employees?.Select(s => s.EmployeeId).ToList(), selectionDateFrom, selectionDateTo, false).GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key.Value, l => l.ToList());
                    }

                    #endregion

                    if (selectionEmployeeIds.Count == 0)
                        employees = EmployeeManager.GetAllEmployees(entities, reportResult.ActorCompanyId, active: selectionActiveEmployees, loadEmployment: LoadEmployment, loadContact: false);
                    else
                        employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: LoadEmployment, loadContact: false);

                    #region Content
                    if (employees != null)
                    {
                        Parallel.ForEach(employees, GetDefaultParallelOptions(), employee =>
                        {
                            List<EmploymentCalenderDTO> items = EmployeeManager.GetEmploymentCalenderDTOs(employee, selectionDateFrom, selectionDateTo, employeeGroups, payrollGroups, payrollPriceTypes, vacationGroups, companyHolidays, dayTypesForCompany, !LoadEmployees, annualLeaveGroups: annualLeaveGroups);

                            if (onlyDateWithChanges)
                            {
                                EmploymentCalenderDTO prevItem = null;
                                foreach (EmploymentCalenderDTO item in items)
                                {
                                    // Get unique dates with actual changes
                                    if (prevItem == null ||
                                        prevItem.EmploymentId != item.EmploymentId ||
                                        prevItem.PayrollGroupId != item.PayrollGroupId ||
                                        prevItem.EmployeeGroupId != item.EmployeeGroupId ||
                                        prevItem.VacationGroupId != item.VacationGroupId ||
                                        prevItem.Percent != item.Percent ||
                                        prevItem.DayTypeId != item.DayTypeId ||
                                        item.Date == items.Last().Date)
                                    {
                                        dateWithChanges.Add(item.Date);
                                    }
                                    prevItem = item;
                                    employeeDateItems.Add(new EmployeeDateItem(item));
                                }
                            }
                            else
                            {
                                items.ForEach(f => employeeDateItems.Add(new EmployeeDateItem(f)));
                            }
                        });
                    }

                    List<DateTime> uniqueDates = dateWithChanges.Distinct().ToList();
                    List<EmployeeDateItem> returnedItems = new List<EmployeeDateItem>();

                    foreach (var onEmployee in employeeDateItems.GroupBy(g => g.EmployeeId))
                    {
                        List<TimeScheduleTemplateBlock> blocks = scheduleBlocks.ContainsKey(onEmployee.Key) ? scheduleBlocks.FirstOrDefault(f => f.Key == onEmployee.Key).Value : new List<TimeScheduleTemplateBlock>();

                        foreach (var item in onEmployee)
                        {
                            if (onlyDateWithChanges && !uniqueDates.Contains(item.Date) && !uniqueDates.Contains(item.Date.AddDays(1)))
                                continue;

                            item.EmployeeGroupName = item.EmployeeGroupId.HasValue ? employeeGroups.FirstOrDefault(f => f.EmployeeGroupId == item.EmployeeGroupId.Value)?.Name ?? string.Empty : string.Empty;
                            item.PayrollGroupName = item.PayrollGroupId.HasValue ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == item.PayrollGroupId.Value)?.Name ?? string.Empty : string.Empty;
                            item.VacationGroupName = item.VacationGroupId.HasValue ? vacationGroups.FirstOrDefault(f => f.VacationGroupId == item.VacationGroupId.Value)?.Name ?? string.Empty : string.Empty;

                            if (LoadSchedule && !blocks.IsNullOrEmpty())
                            {
                                var ondate = blocks.Where(w => w.Date == item.Date).ToList();

                                if (!ondate.IsNullOrEmpty())
                                {
                                    var scheduleMinutes = ondate.GetWorkMinutes();
                                    item.ScheduleTime = decimal.Round(decimal.Divide(scheduleMinutes, 60), 2);
                                    if (CheckScheduleAbsence)
                                    {
                                        var absenceMinutes = ondate.IsWholeDayAbsence() ? scheduleMinutes : ondate.Where(w => w.TimeDeviationCauseId.HasValue).Sum(s => s.TotalMinutes) - ondate.Where(w => w.IsBreak && w.IsBreakSurroundedByAbsence(ondate.ToList())).Sum(s => s.TotalMinutes);
                                        var percentAbsence = absenceMinutes != 0 ? decimal.Round(decimal.Multiply(decimal.Divide(absenceMinutes, scheduleMinutes), 100), 2) : 0;
                                        item.ScheduleAbsenceTime = decimal.Round(decimal.Divide(absenceMinutes, 60), 2);
                                        item.PercentScheduleAbsenceTime = percentAbsence;
                                    }
                                }
                            }

                            returnedItems.Add(item);
                        }
                    }

                    List<EmployeeDateItem> mergedItems = new List<EmployeeDateItem>();
                    var columns = _reportDataInput.Columns.Select(s => s.Column).ToList();

                    foreach (var grouped in returnedItems.GroupBy(g => g.GroupOn(columns)))
                    {
                        var first = grouped.First();
                        var last = grouped.Last();

                        var mergedItem = first;
                        mergedItem.FTE = grouped.Sum(s => s.Percent) / 100;
                        mergedItem.PercentScheduleAbsenceTime = grouped.Sum(s => s.PercentScheduleAbsenceTime);
                        mergedItem.ScheduleAbsenceTime = grouped.Sum(s => s.ScheduleAbsenceTime);
                        mergedItems.Add(mergedItem);
                    }

                    _reportDataOutput.EmployeeDateItems = mergedItems.OrderBy(o => o.EmployeeId).ThenBy(t => t.Date).ToList();

                    #endregion
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }
    }

    public class EmployeeDateReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeDateMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeDateReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeDateMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeDateMatrixColumns.EmployeeNr;
        }
    }

    public class EmployeeDateReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeDateReportDataField> Columns { get; set; }

        public EmployeeDateReportDataInput(CreateReportResult reportResult, List<EmployeeDateReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeDateReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmployeeDateReportDataInput Input { get; set; }
        public List<EmployeeDateItem> EmployeeDateItems { get; set; }

        public EmployeeDateReportDataOutput(EmployeeDateReportDataInput input)
        {
            this.Input = input;
            this.EmployeeDateItems = new List<EmployeeDateItem>();
        }
    }
}

