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
    public class ShiftQueueReportData : IReportDataModel
    {
        private readonly ShiftQueueReportDataInput _reportDataInput;
        private readonly ShiftQueueReportDataOutput _reportDataOutput;

        private EmployeeManager EmployeeManager { get { return _reportDataInput.EmployeeManager; } }
        private TimeScheduleManager TimeScheduleManager { get { return _reportDataInput.TimeScheduleManager; } }
        private TimeReportDataManager TimeReportDataManager { get { return _reportDataInput.TimeReportDataManager; } }
        private CreateReportResult ReportResult { get { return _reportDataInput.ReportResult; } }

        public ShiftQueueReportData(ParameterObject parameterObject, ShiftQueueReportDataInput reportDataInput)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new ShiftQueueReportDataOutput(reportDataInput);
        }

        public static List<ShiftQueueReportDataField> GetPossibleDataFields()
        {
            List<ShiftQueueReportDataField> possibleFields = new List<ShiftQueueReportDataField>();
            EnumUtility.GetValues<TermGroup_ShiftQueueMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new ShiftQueueReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public ShiftQueueReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            _reportDataOutput.Result = LoadData();

            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq

            if (!TimeReportDataManager.TryGetDatesFromSelection(ReportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TimeReportDataManager.TryGetEmployeeIdsFromSelection(ReportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return new ActionResult(false);

            TimeReportDataManager.TryGetIncludeInactiveFromSelection(ReportResult, out bool selectionIncludeInactive, out bool selectionOnlyInactive, out bool? selectionActiveEmployees);

            employees = employees ?? EmployeeManager.GetAllEmployeesByIds(ReportResult.ActorCompanyId, selectionEmployeeIds);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Collections

                    var queues = TimeScheduleManager.GetShiftQueuesForEmployeesForReport(entities, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                    #endregion

                    #region Content

                    var terms = TimeReportDataManager.GetTermGroupDict(TermGroup.TimeScheduleTemplateBlockQueueType);
                    List<int> currentEmployeeIds = queues.Where(w => w.TimeScheduleTemplateBlock.EmployeeId.HasValue).Select(s => s.TimeScheduleTemplateBlock.EmployeeId.Value).Distinct().ToList();
                    
                    var currentEmployees = EmployeeManager.GetAllEmployeesByIds(entities, ReportResult.ActorCompanyId, currentEmployeeIds, loadContact:true);

                    foreach (var queueGroup in queues.GroupBy(g => g.TimeScheduleTemplateBlock.Link))
                    {
                        foreach (var spot in queueGroup)
                        {
                            var employee = employees.FirstOrDefault(w => w.EmployeeId == spot.EmployeeId);
                            if (employee == null)
                                continue;

                            var firstSpot = queueGroup.OrderBy(o => o.TimeScheduleTemplateBlock.StartTime).FirstOrDefault();
                            var lastSpot = queueGroup.OrderByDescending(o => o.TimeScheduleTemplateBlock.StopTime).FirstOrDefault();
                            var currentEmployee = currentEmployees.FirstOrDefault(w => w.EmployeeId == spot.TimeScheduleTemplateBlock.EmployeeId);

                            var item = new ShiftQueueItem()
                            {
                                EmployeeId = spot.EmployeeId,
                                TypeName = terms.ContainsKey(spot.Type) ? terms[spot.Type] : String.Empty,
                                Created = spot.Created ?? DateTime.MinValue,
                                Creator = spot.CreatedBy,
                                Modifier = spot.ModifiedBy,
                                EmployeeNr = employee.EmployeeNr,
                                EmployeeName = employee.Name,
                                CurrentEmployee = currentEmployee?.Name ?? String.Empty,
                                CurrentEmployeeIsHidden = currentEmployee?.Hidden ?? false,
                                Date = spot.TimeScheduleTemplateBlock.Date,
                                StartTime = firstSpot.TimeScheduleTemplateBlock.StartTime,
                                StopTime = lastSpot.TimeScheduleTemplateBlock.StopTime,
                                DateHandled = spot.Modified,
                                QueueTimeSinceShiftCreatedInHours = spot.Created.HasValue ? Convert.ToDecimal((spot.Created - spot.TimeScheduleTemplateBlock.Created).Value.TotalHours) : 0,
                                QueueTimeBeforeShiftStartInHours = spot.Created.HasValue && firstSpot.TimeScheduleTemplateBlock.Date.HasValue ? Convert.ToDecimal((CalendarUtility.MergeDateAndTime(firstSpot.TimeScheduleTemplateBlock.Date ?? DateTime.MinValue, firstSpot.TimeScheduleTemplateBlock.StartTime) - spot.Created).Value.TotalHours) : 0,
                                QueueTimeBeforeQueueWasHandledInHours = spot.Modified.HasValue && spot.Created.HasValue ? Convert.ToDecimal((spot.Modified - spot.Created).Value.TotalHours) : 0,
                                QueueTimeHandledBeforeShiftStartInHours = spot.Modified.HasValue && firstSpot.TimeScheduleTemplateBlock.Date.HasValue ? Convert.ToDecimal((CalendarUtility.MergeDateAndTime(firstSpot.TimeScheduleTemplateBlock.Date ?? DateTime.MinValue, firstSpot.TimeScheduleTemplateBlock.StartTime) - spot.Modified).Value.TotalHours) : 0,
                            };

                            _reportDataOutput.ShiftQueueItems.Add(item);
                        }
                    }

                    #endregion
                }

                return new ActionResult();
            }
        }
    }

    public class ShiftQueueReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_ShiftQueueMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public ShiftQueueReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_ShiftQueueMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_ShiftQueueMatrixColumns.EmployeeNr;
        }
    }

    public class ShiftQueueReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<ShiftQueueReportDataField> Columns { get; set; }

        public readonly EmployeeManager EmployeeManager;
        public readonly TimeScheduleManager TimeScheduleManager;
        public readonly TimeReportDataManager TimeReportDataManager;

        public ShiftQueueReportDataInput(CreateReportResult reportResult, List<ShiftQueueReportDataField> columns, TimeReportDataManager timeReportDataManager, EmployeeManager employeeManager, TimeScheduleManager timeScheduleManager)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.TimeReportDataManager = timeReportDataManager;
            this.EmployeeManager = employeeManager;
            this.TimeScheduleManager = timeScheduleManager;
        }
    }

    public class ShiftQueueReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public ShiftQueueReportDataInput Input { get; set; }
        public List<ShiftQueueItem> ShiftQueueItems { get; set; }

        public ShiftQueueReportDataOutput(ShiftQueueReportDataInput input)
        {
            this.Input = input;
            this.ShiftQueueItems = new List<ShiftQueueItem>();
        }
    }

}

