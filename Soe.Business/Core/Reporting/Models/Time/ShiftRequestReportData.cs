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
    public class ShiftRequestReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly ShiftRequestReportDataInput _reportDataInput;
        private readonly ShiftRequestReportDataOutput _reportDataOutput;

        private CreateReportResult ReportResult { get { return _reportDataInput.ReportResult; } }

        public ShiftRequestReportData(ParameterObject parameterObject, ShiftRequestReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new ShiftRequestReportDataOutput(reportDataInput);
        }

        public static List<ShiftRequestReportDataField> GetPossibleDataFields()
        {
            List<ShiftRequestReportDataField> possibleFields = new List<ShiftRequestReportDataField>();
            EnumUtility.GetValues<TermGroup_ShiftRequestMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new ShiftRequestReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public ShiftRequestReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TimeReportDataManager.TryGetEmployeeIdsFromSelection(ReportResult, selectionDateFrom, selectionDateTo, out _, out List<int> selectionEmployeeIds))
                return new ActionResult(false);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Collections

                    Dictionary<int, string> yesNoDict = base.GetTermGroupDict(TermGroup.YesNo, GetLangId());
                    List<ShiftRequestStatusForReportDTO> shiftRequestStatuses = TimeScheduleManager.GetShiftRequestStatusForReport(entities, ReportResult.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);

                    #endregion

                    #region Content

                    foreach (ShiftRequestStatusForReportDTO shiftRequestStatus in shiftRequestStatuses.OrderBy(r => r.EmployeeNr).ThenBy(r => r.ShiftDate).ThenBy(r => r.ShiftStartTime))
                    {
                        ShiftRequestItem item = new ShiftRequestItem()
                        {
                            EmployeeId = shiftRequestStatus.EmployeeId,
                            Sender = shiftRequestStatus.SenderName,
                            SentDate = shiftRequestStatus.SentDate,
                            EmployeeNr = shiftRequestStatus.EmployeeNr,
                            EmployeeName = shiftRequestStatus.EmployeeName,
                            ReadDate = shiftRequestStatus.ReadDate,
                            Answer = GetValueFromDict((int)shiftRequestStatus.AnswerType, yesNoDict),
                            AnswerDate = shiftRequestStatus.AnswerDate,
                            Subject = shiftRequestStatus.Subject,
                            Text = shiftRequestStatus.Text,
                            RequestCreated = shiftRequestStatus.Created ?? CalendarUtility.DATETIME_DEFAULT,
                            RequestCreatedBy = shiftRequestStatus.CreatedBy,
                            ShiftDate = shiftRequestStatus.ShiftDate,
                            ShiftStartTime = shiftRequestStatus.ShiftStartTime ?? CalendarUtility.DATETIME_DEFAULT,
                            ShiftStopTime = shiftRequestStatus.ShiftStopTime ?? CalendarUtility.DATETIME_DEFAULT,
                            ShiftTypeId = shiftRequestStatus.ShiftTypeId,
                            ShiftTypeName = shiftRequestStatus.ShiftTypeName,
                            ShiftCreated = shiftRequestStatus.ShiftCreated,
                            ShiftCreatedBy = shiftRequestStatus.ShiftCreatedBy,
                            ShiftModified = shiftRequestStatus.ShiftModified,
                            ShiftModifiedBy = shiftRequestStatus.ShiftModifiedBy,
                            ShiftDeleted = shiftRequestStatus.ShiftState == SoeEntityState.Deleted, 
                            ShiftAccountNr = shiftRequestStatus.ShiftAccountNr,
                            ShiftAccountName = shiftRequestStatus.ShiftAccountName,
                        };

                        _reportDataOutput.ShiftRequestItems.Add(item);
                    }

                    #endregion
                }

                return new ActionResult();
            }
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }
    }

    public class ShiftRequestReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_ShiftRequestMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public ShiftRequestReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_ShiftRequestMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_ShiftRequestMatrixColumns.EmployeeNr;
        }
    }

    public class ShiftRequestReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<ShiftRequestReportDataField> Columns { get; set; }

        public readonly EmployeeManager EmployeeManager;
        public readonly TimeScheduleManager TimeScheduleManager;
        public readonly TimeReportDataManager TimeReportDataManager;

        public ShiftRequestReportDataInput(CreateReportResult reportResult, List<ShiftRequestReportDataField> columns, TimeReportDataManager timeReportDataManager, EmployeeManager employeeManager, TimeScheduleManager timeScheduleManager)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.TimeReportDataManager = timeReportDataManager;
            this.EmployeeManager = employeeManager;
            this.TimeScheduleManager = timeScheduleManager;
        }
    }

    public class ShiftRequestReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public ShiftRequestReportDataInput Input { get; set; }
        public List<ShiftRequestItem> ShiftRequestItems { get; set; }

        public ShiftRequestReportDataOutput(ShiftRequestReportDataInput input)
        {
            this.Input = input;
            this.ShiftRequestItems = new List<ShiftRequestItem>();
        }
    }
}

