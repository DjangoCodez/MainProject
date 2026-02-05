using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class TimeStampEntryReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly TimeStampEntryReportDataInput _reportDataInput;
        private readonly TimeStampEntryReportDataOutput _reportDataOutput;

        private bool loadShiftTypes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeStampMatrixColumns.ShiftTypeName);
        private bool loadOriginTypes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeStampMatrixColumns.OriginType);
        private bool loadStatuses => _reportDataInput.Columns.Any(a => a.Column == TermGroup_TimeStampMatrixColumns.Status);

        public TimeStampEntryReportData(ParameterObject parameterObject, TimeStampEntryReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new TimeStampEntryReportDataOutput(reportDataInput);
        }

        public static List<TimeStampEntryReportDataField> GetPossibleDataFields()
        {
            List<TimeStampEntryReportDataField> possibleFields = new List<TimeStampEntryReportDataField>();
            EnumUtility.GetValues<TermGroup_TimeStampMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new TimeStampEntryReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public TimeStampEntryReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out List<int> selectionEmployeeIds, out List<int> selectionAccountIds, out _))
                return new ActionResult(false);

            TryGetBoolFromSelection(reportResult, out bool selectionIncludeDeleted, "includeDeleted");
            TryGetBoolFromSelection(reportResult, out bool filterOnAccounting, "filterOnAccounting");
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Content

                    List<TimeStampEntryDTO> timeStamps = TimeStampManager.GetTimeStampEntriesDTO(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.ActorCompanyId, !selectionIncludeDeleted);

                    if (loadShiftTypes)
                        _reportDataOutput.ShiftTypes = base.GetShiftTypesFromCache(entitiesReadOnly, CacheConfig.Company(base.ActorCompanyId), false).ToDTOs().ToList();

                    if (loadOriginTypes)
                        _reportDataOutput.OriginTypes = GetTermGroupContent(TermGroup.TimeStampEntryOriginType);

                    if (loadStatuses)
                        _reportDataOutput.Statuses = GetTermGroupContent(TermGroup.TimeStampEntryStatus);


                    foreach (TimeStampEntryDTO timestamp in timeStamps)
                    {
                        if (filterOnAccounting && !selectionAccountIds.IsNullOrEmpty() && timestamp.AccountId.HasValue && !selectionAccountIds.Contains(timestamp.AccountId.Value))
                            continue;

                        var item = new TimeStampEntryItem();
                        item.EmployeeNr = timestamp.EmployeeNr;
                        item.EmployeeName = timestamp.EmployeeName;
                        item.Time = timestamp.Time;
                        item.AccountName = timestamp.AccountName;
                        item.Date = timestamp.Date;
                        item.IsBreak = timestamp.IsBreak;
                        item.IsPaidBreak = timestamp.IsPaidBreak;
                        item.IsDistanceWork = timestamp.IsDistanceWork;
                        item.Created = timestamp.Created;
                        item.CreatedBy = timestamp.CreatedBy ?? string.Empty;
                        item.Modified = timestamp.Modified;
                        item.ModifiedBy = timestamp.ModifiedBy ?? string.Empty;
                        item.ShiftTypeId = timestamp.ShiftTypeId ?? 0;
                        item.TimeDeviationCauseName = timestamp.TimeDeviationCauseName;
                        item.TimeScheduleTypeName = timestamp.TimeScheduleTypeName ?? string.Empty;
                        item.TimeTerminalName = timestamp.TimeTerminalName;
                        item.TypeName = timestamp.TypeName;
                        item.Note = timestamp.Note ?? string.Empty;
                        item.OriginType = (int)timestamp.OriginType;
                        item.OriginalTime = (DateTime)timestamp.OriginalTime;
                        item.Status = (int)timestamp.Status;
                        item.State = (int)timestamp.State;

                        _reportDataOutput.TimeStampEntryItems.Add(item);
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

    public class TimeStampEntryReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_TimeStampMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }
        public TimeStampEntryReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_TimeStampMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_TimeStampMatrixColumns.Unknown;
        }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }
    }

    public class TimeStampEntryReportDataInput
    {
        private CreateReportResult ReportResult { get; set; }
        public List<TimeStampEntryReportDataField> Columns { get; set; }

        public TimeStampEntryReportDataInput(CreateReportResult reportResult, List<TimeStampEntryReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class TimeStampEntryReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public TimeStampEntryReportDataInput Input { get; set; }
        public List<TimeStampEntryItem> TimeStampEntryItems { get; set; }
        public List<GenericType> OriginTypes { get; set; }
        public List<GenericType> Statuses { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }

        public TimeStampEntryReportDataOutput(TimeStampEntryReportDataInput input)
        {
            this.Input = input;
            this.TimeStampEntryItems = new List<TimeStampEntryItem>();
            this.OriginTypes = new List<GenericType>();
            this.Statuses = new List<GenericType>();
        }
    }
}

