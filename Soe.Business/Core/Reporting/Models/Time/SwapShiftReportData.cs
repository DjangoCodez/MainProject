using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SoftOne.Soe.Business.Core.Reporting.Models.Time.SwapShiftReportData;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class SwapShiftReportData(ParameterObject parameterObject, SwapShiftReportDataInput reportDataInput) : EconomyReportDataManager(parameterObject), IReportDataModel
    {
        private readonly SwapShiftReportDataOutput _reportDataOutput = new(reportDataInput);

        public static List<SwapShiftReportDataField> GetPossibleDataFields()
        {
            List<SwapShiftReportDataField> possibleFields = [];
            EnumUtility.GetValues<TermGroup_SwapShiftMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new SwapShiftReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public SwapShiftReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out List<int> selectionEmployeeIds, out _, out _))
                return new ActionResult(false);

            TryGetBoolFromSelection(reportResult, out bool showDetailed, "showDetailed");
            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new())
            {
                if (selectionEmployeeIds.Any())
                {

                    #region Prereq

                  
                    #region Permissions

                    bool permission = FeatureManager.HasRolePermission(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

                    if (!permission)
                        return new ActionResult(false);

                    #endregion

                    #endregion

                    var swapShifts = TimeScheduleManager.GetEmployeeSwapRequestsForPeriod(reportResult.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                    var employeeIds = swapShifts.SelectMany(s => s.Rows.Select(r => r.EmployeeId)).Distinct().ToList();
                    var allEmployees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, employeeIds);

                    #region Content

                    foreach (var swapShift in swapShifts)
                    {
                        var initiator = allEmployees.FirstOrDefault(e => e.EmployeeId == swapShift.InitiatorEmployeeId);
                        var swapTo = allEmployees.FirstOrDefault(e => e.EmployeeId == swapShift.Rows.FirstOrDefault(w => w.EmployeeId != initiator.EmployeeId)?.EmployeeId);

                        if (showDetailed)
                        {
                            foreach (var row in swapShift.Rows.Where(w => w.EmployeeId == initiator.EmployeeId))
                            {
                                var employee = allEmployees.FirstOrDefault(e => e.EmployeeId == row.EmployeeId);
                                if (employee == null)
                                    continue;

                                var acceptor = swapShift.AcceptorEmployeeId != null ? allEmployees.FirstOrDefault(e => e.EmployeeId == swapShift.AcceptorEmployeeId) : null;
                                var shift = row.ScheduleStart.ToString("HH:mm") + " - " + row.ScheduleStop.ToString("HH:mm");
                                var shiftType = string.Empty;

                                var item = new SwapShiftItem
                                {
                                    EmployeeNr = employee.EmployeeNr,
                                    EmployeeName = employee.Name,
                                    Date = row.Date,
                                    HasShift = shift,
                                    ShiftType = shiftType,
                                    AcceptorEmployeeNr = acceptor?.EmployeeNr ?? string.Empty,
                                    AcceptorEmployeeName = acceptor?.Name ?? string.Empty,
                                    AcceptedDate = swapShift.AcceptedDate,
                                    SwappedToEmployeeNr = swapTo.EmployeeNr,
                                    SwappedToEmployeeName = swapTo.Name,
                                    InitiatorEmployeeNr = initiator.EmployeeNr,
                                    InitiatorEmployeeName = initiator.Name,
                                    InitiatedDate = swapShift.InitiatedDate,
                                    ApprovedDate = swapShift.ApprovedDate,
                                    ApprovedBy = swapShift.ApprovedBy,
                                    ShiftLengthInMinutes = row.ShiftLength,
                                };
                                _reportDataOutput.SwapShiftItems.Add(item);
                            }
                            foreach (var row in swapShift.Rows.Where(w => w.EmployeeId == swapTo.EmployeeId))
                            {
                                var employee = allEmployees.FirstOrDefault(e => e.EmployeeId == row.EmployeeId);
                                if (employee == null)
                                    continue;

                                var acceptor = swapShift.AcceptorEmployeeId != null ? allEmployees.FirstOrDefault(e => e.EmployeeId == swapShift.AcceptorEmployeeId) : null;
                                var shift = row.ScheduleStart.ToString("HH:mm") + " - " + row.ScheduleStop.ToString("HH:mm");
                                var shiftType = string.Empty;

                                var item = new SwapShiftItem
                                {
                                    EmployeeNr = employee.EmployeeNr,
                                    EmployeeName = employee.Name,
                                    Date = row.Date,
                                    HasShift = shift,
                                    ShiftType = shiftType,
                                    AcceptorEmployeeNr = acceptor?.EmployeeNr ?? string.Empty,
                                    AcceptorEmployeeName = acceptor?.Name ?? string.Empty,
                                    AcceptedDate = swapShift.AcceptedDate,
                                    SwappedToEmployeeNr = initiator.EmployeeNr,
                                    SwappedToEmployeeName = initiator.Name,
                                    InitiatorEmployeeNr = initiator.EmployeeNr,
                                    InitiatorEmployeeName = initiator.Name,
                                    InitiatedDate = swapShift.InitiatedDate,
                                    ApprovedDate = swapShift.ApprovedDate,
                                    ApprovedBy = swapShift.ApprovedBy,
                                    ShiftLengthInMinutes = row.ShiftLength,
                                };
                                _reportDataOutput.SwapShiftItems.Add(item);
                            }
                        }
                        else
                        {
                            var initiatorRows = swapShift.Rows.Where(w => w.EmployeeId == initiator.EmployeeId).ToList();
                            var swapToRows = swapShift.Rows.Where(w => w.EmployeeId == swapTo.EmployeeId).ToList();

                            var employee = allEmployees.FirstOrDefault(e => e.EmployeeId == initiator.EmployeeId);
                            if (employee == null)
                                continue;

                            var acceptor = swapShift.AcceptorEmployeeId != null ? allEmployees.FirstOrDefault(e => e.EmployeeId == swapShift.AcceptorEmployeeId) : null;
                            var shift = initiatorRows.OrderBy(r => r.ScheduleStart).FirstOrDefault()?.ScheduleStart.ToString("HH:mm") + " - " + initiatorRows.OrderByDescending(r => r.ScheduleStop).FirstOrDefault()?.ScheduleStop.ToString("HH:mm");
                            var shiftType = new StringBuilder();
                            var shiftLength = 0;
                            var date = initiatorRows.OrderBy(r => r.Date).FirstOrDefault()?.Date ?? DateTime.MinValue;

                            var swapToShift = swapToRows.OrderBy(r => r.ScheduleStart).FirstOrDefault()?.ScheduleStart.ToString("HH:mm") + " - " + swapToRows.OrderByDescending(r => r.ScheduleStop).FirstOrDefault()?.ScheduleStop.ToString("HH:mm");
                            var swapToShiftType = new StringBuilder();
                            var swapToShiftLength = 0;
                            var swapToDate = swapToRows.OrderBy(r => r.Date).FirstOrDefault()?.Date ?? DateTime.MinValue;

                            foreach (var row in initiatorRows)
                            {
                                shiftLength += row.ShiftLength;
                            }

                            foreach (var row in swapToRows)
                            {
                                swapToShiftLength += row.ShiftLength;
                            }

                            var item = new SwapShiftItem
                            {
                                EmployeeNr = employee.EmployeeNr,
                                EmployeeName = employee.Name,
                                Date = date,
                                HasShift = shift,
                                ShiftType = shiftType.ToString(),
                                AcceptorEmployeeNr = acceptor?.EmployeeNr ?? string.Empty,
                                AcceptorEmployeeName = acceptor?.Name ?? string.Empty,
                                AcceptedDate = swapShift.AcceptedDate,
                                SwappedToEmployeeNr = swapTo.EmployeeNr,
                                SwappedToEmployeeName = swapTo.Name,
                                InitiatorEmployeeNr = initiator.EmployeeNr,
                                InitiatorEmployeeName = initiator.Name,
                                InitiatedDate = swapShift.InitiatedDate,
                                ApprovedDate = swapShift.ApprovedDate,
                                ApprovedBy = swapShift.ApprovedBy,
                                ShiftLengthInMinutes = shiftLength,
                            };
                            _reportDataOutput.SwapShiftItems.Add(item);

                            employee = allEmployees.FirstOrDefault(e => e.EmployeeId == swapTo.EmployeeId);
                            if (employee == null)
                                continue;

                            var swapToItem = new SwapShiftItem
                            {
                                EmployeeNr = employee.EmployeeNr,
                                EmployeeName = employee.Name,
                                Date = swapToDate,
                                HasShift = swapToShift,
                                ShiftType = swapToShiftType.ToString(),
                                AcceptorEmployeeNr = acceptor?.EmployeeNr ?? string.Empty,
                                AcceptorEmployeeName = acceptor?.Name ?? string.Empty,
                                AcceptedDate = swapShift.AcceptedDate,
                                SwappedToEmployeeNr = initiator.EmployeeNr,
                                SwappedToEmployeeName = initiator.Name,
                                InitiatorEmployeeNr = initiator.EmployeeNr,
                                InitiatorEmployeeName = initiator.Name,
                                InitiatedDate = swapShift.InitiatedDate,
                                ApprovedDate = swapShift.ApprovedDate,
                                ApprovedBy = swapShift.ApprovedBy,
                                ShiftLengthInMinutes = swapToShiftLength,
                            };
                            _reportDataOutput.SwapShiftItems.Add(swapToItem);
                        }

                    }
                    #endregion

                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

    }
    public class SwapShiftReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_SwapShiftMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public SwapShiftReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_SwapShiftMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_SwapShiftMatrixColumns.Unknown;
        }
    }

    public class SwapShiftReportDataInput(CreateReportResult reportResult, List<SwapShiftReportDataField> columns)
    {
        public CreateReportResult ReportResult { get; set; } = reportResult;
        public List<SwapShiftReportDataField> Columns { get; set; } = columns;
    }

    public class SwapShiftReportDataOutput(SwapShiftReportDataInput input) : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public SwapShiftReportDataInput Input { get; set; } = input;
        public List<SwapShiftItem> SwapShiftItems { get; set; } = [];
    }

}


