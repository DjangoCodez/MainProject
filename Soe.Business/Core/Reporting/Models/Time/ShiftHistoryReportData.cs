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
    public class ShiftHistoryReportData : IReportDataModel
    {
        private readonly ShiftHistoryReportDataInput _reportDataInput;
        private readonly ShiftHistoryReportDataOutput _reportDataOutput;

        private EmployeeManager EmployeeManager { get { return _reportDataInput.EmployeeManager; } }
        private TimeScheduleManager TimeScheduleManager { get { return _reportDataInput.TimeScheduleManager; } }
        private TimeReportDataManager TimeReportDataManager { get { return _reportDataInput.TimeReportDataManager; } }
        private CreateReportResult ReportResult { get { return _reportDataInput.ReportResult; } }

        public ShiftHistoryReportData(ParameterObject parameterObject, ShiftHistoryReportDataInput reportDataInput)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new ShiftHistoryReportDataOutput(reportDataInput);
        }

        public static List<ShiftHistoryReportDataField> GetPossibleDataFields()
        {
            List<ShiftHistoryReportDataField> possibleFields = new List<ShiftHistoryReportDataField>();
            EnumUtility.GetValues<TermGroup_ShiftHistoryMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new ShiftHistoryReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public ShiftHistoryReportDataOutput CreateOutput(CreateReportResult reportResult)
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
         
            var employeeGroups = EmployeeManager.GetEmployeeGroupsFromCache(ReportResult.ActorCompanyId);
            employees = employees ?? EmployeeManager.GetAllEmployeesByIds(ReportResult.ActorCompanyId, selectionEmployeeIds);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Collections

                    #endregion

                    #region Content

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees?.GetEmployee(employeeId, selectionActiveEmployees);
                        if (employee == null)
                            continue;

                        EmployeeGroup employeeGroup = employee.GetEmployeeGroup(selectionDateFrom, employeeGroups);
                        List<TimeScheduleTemplateBlock> templateBlocks = TimeScheduleManager.GetTimeScheduleTemplateBlocks(entities, employee.EmployeeId, selectionDateFrom, selectionDateTo);
                        List<ShiftHistoryDTO> shiftHistorys = TimeScheduleManager.GetTimeScheduleTemplateBlockHistory(_reportDataInput.ReportResult.ActorCompanyId, templateBlocks.Select(i => i.TimeScheduleTemplateBlockId).ToList());

                        foreach (var shiftHistory in shiftHistorys)
                        {
                            ShiftHistoryItem shiftHistoryItem = new ShiftHistoryItem()
                            {
                                EmployeeId = employeeId,
                                TimeScheduleTemplateBlockId = shiftHistory.TimeScheduleTemplateBlockId,
                                TypeName = shiftHistory.TypeName,
                                FromShiftStatus = shiftHistory.FromShiftStatus,
                                ToShiftStatus = shiftHistory.ToShiftStatus,
                                ShiftStatusChanged = shiftHistory.ShiftStatusChanged,
                                FromShiftUserStatus = shiftHistory.FromShiftUserStatus,
                                ToShiftUserStatus = shiftHistory.ToShiftUserStatus,
                                ShiftUserStatusChanged = shiftHistory.ShiftUserStatusChanged,
                                FromEmployeeName = shiftHistory.FromEmployeeName,
                                ToEmployeeName = shiftHistory.ToEmployeeName,
                                FromEmployeeNr = shiftHistory.FromEmployeeNr,
                                ToEmployeeNr = shiftHistory.ToEmployeeNr,
                                EmployeeChanged = shiftHistory.EmployeeChanged,
                                FromTime = shiftHistory.FromTime,
                                ToTime = shiftHistory.ToTime,
                                TimeChanged = shiftHistory.TimeChanged,
                                FromDateAndTime = shiftHistory.FromDateAndTime,
                                ToDateAndTime = shiftHistory.ToDateAndTime,
                                DateAndTimeChanged = shiftHistory.DateAndTimeChanged,
                                FromShiftType = shiftHistory.FromShiftType,
                                ToShiftType = shiftHistory.ToShiftType,
                                ShiftTypeChanged = shiftHistory.ShiftTypeChanged,
                                FromTimeDeviationCause = shiftHistory.FromTimeDeviationCause,
                                ToTimeDeviationCause = shiftHistory.ToTimeDeviationCause,
                                TimeDeviationCauseChanged = shiftHistory.TimeDeviationCauseChanged,
                                Created = shiftHistory.Created,
                                CreatedBy = shiftHistory.CreatedBy,
                                AbsenceRequestApprovedText = shiftHistory.AbsenceRequestApprovedText,
                                FromStart = shiftHistory.FromStart,
                                FromStop = shiftHistory.FromStop,
                                ToStart = shiftHistory.ToStart,
                                ToStop = shiftHistory.ToStop,
                                OriginEmployeeNr = shiftHistory.OriginEmployeeNr,
                                OriginEmployeeName = shiftHistory.OriginEmployeeName,
                                FromEmployeeId = shiftHistory.FromEmployeeId,
                                ToEmployeeId = shiftHistory.ToEmployeeId,
                                FromExtraShift = shiftHistory.FromExtraShift,
                                ToExtraShift = shiftHistory.ToExtraShift,
                                ExtraShiftChanged = shiftHistory.ExtraShiftChanged,
                            };

                            _reportDataOutput.ShiftHistoryItems.Add(shiftHistoryItem);
                        }


                        #endregion
                    }

                    #endregion
                }
            }
            return new ActionResult();
        }
    }

    public class ShiftHistoryReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_ShiftHistoryMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public ShiftHistoryReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_ShiftHistoryMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_ShiftHistoryMatrixColumns.FromEmployeeNr;
        }
    }

    public class ShiftHistoryReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<ShiftHistoryReportDataField> Columns { get; set; }

        public readonly EmployeeManager EmployeeManager;
        public readonly TimeScheduleManager TimeScheduleManager;
        public readonly TimeReportDataManager TimeReportDataManager;

        public ShiftHistoryReportDataInput(CreateReportResult reportResult, List<ShiftHistoryReportDataField> columns, TimeReportDataManager timeReportDataManager, EmployeeManager employeeManager, TimeScheduleManager timeScheduleManager)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.TimeReportDataManager = timeReportDataManager;
            this.EmployeeManager = employeeManager;
            this.TimeScheduleManager = timeScheduleManager;
        }
    }

    public class ShiftHistoryReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public ShiftHistoryReportDataInput Input { get; set; }
        public List<ShiftHistoryItem> ShiftHistoryItems { get; set; }

        public ShiftHistoryReportDataOutput(ShiftHistoryReportDataInput input)
        {
            this.Input = input;
            this.ShiftHistoryItems = new List<ShiftHistoryItem>();
        }
    }
}

