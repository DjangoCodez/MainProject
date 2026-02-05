using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class ScheduledTimeSummaryReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly ScheduledTimeSummaryReportDataOutput _reportDataOutput;

        public ScheduledTimeSummaryReportData(ParameterObject parameterObject, ScheduledTimeSummaryReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new ScheduledTimeSummaryReportDataOutput(reportDataInput);
        }

        public static List<ScheduledTimeSummaryReportDataField> GetPossibleDataFields()
        {
            List<ScheduledTimeSummaryReportDataField> possibleFields = new List<ScheduledTimeSummaryReportDataField>();
            EnumUtility.GetValues<TermGroup_ScheduledTimeSummaryMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new ScheduledTimeSummaryReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public ScheduledTimeSummaryReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            employees = employees ?? EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Collections

                    string templateSchedule = "Grundschema";
                    string schedule = "Schema";

                    List<ScheduledTimeSummaryItem> scheduledTimeSummaryItems = new List<ScheduledTimeSummaryItem>();   
                    var scheduleTimeSummeries = TimeScheduleManager.GetScheduledTimeSummaries(entities, reportResult.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                    #endregion

                    #region Content

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        var scheduleTimeSummeriesOnEmployee = scheduleTimeSummeries.Where(w => w.EmployeeId == employeeId);

                        if (scheduleTimeSummeriesOnEmployee.IsNullOrEmpty())
                            continue;

                        #region Prereq

                        Employee employee = employees?.GetEmployee(employeeId);
                        if (employee == null)
                            continue;

                        #endregion

                        #region periods

                        foreach (var etp in scheduleTimeSummeriesOnEmployee)
                        {
                            ScheduledTimeSummaryItem item = new ScheduledTimeSummaryItem()
                            {
                                EmployeeId = employeeId,
                                EmployeeNr = employee.EmployeeNr,
                                EmployeeName = employee.Name,
                                Date = etp.Date,
                                Type = etp.Type == (int)TimeScheduledTimeSummaryType.ScheduledTime ? schedule : templateSchedule,
                                Time = etp.Minutes,
                            };

                            scheduledTimeSummaryItems.Add(item);
                        }
                    }

                    _reportDataOutput.ScheduledTimeSummaryItems = scheduledTimeSummaryItems.OrderBy(o => o.EmployeeNr).ThenBy(t => t.Date).ToList();

                    #endregion

                    #endregion
                }

                #region Close repository

                base.personalDataRepository.GenerateLogs();

                #endregion

                return new ActionResult();
            }
        }
    }

    public class ScheduledTimeSummaryReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_ScheduledTimeSummaryMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public ScheduledTimeSummaryReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_ScheduledTimeSummaryMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_ScheduledTimeSummaryMatrixColumns.EmployeeNr;
        }
    }

    public class ScheduledTimeSummaryReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<ScheduledTimeSummaryReportDataField> Columns { get; set; }

        public ScheduledTimeSummaryReportDataInput(CreateReportResult reportResult, List<ScheduledTimeSummaryReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class ScheduledTimeSummaryReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public ScheduledTimeSummaryReportDataInput Input { get; set; }
        public List<ScheduledTimeSummaryItem> ScheduledTimeSummaryItems { get; set; }

        public ScheduledTimeSummaryReportDataOutput(ScheduledTimeSummaryReportDataInput input)
        {
            this.Input = input;
            this.ScheduledTimeSummaryItems = new List<ScheduledTimeSummaryItem>();
        }
    }
}

