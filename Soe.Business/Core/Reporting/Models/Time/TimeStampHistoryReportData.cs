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
    public class TimeStampHistoryReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly TimeStampHistoryReportDataOutput _reportDataOutput;

        public TimeStampHistoryReportData(ParameterObject parameterObject, TimeStampHistoryReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new TimeStampHistoryReportDataOutput(reportDataInput);
        }

        public static List<TimeStampHistoryReportDataField> GetPossibleDataFields()
        {
            List<TimeStampHistoryReportDataField> possibleFields = new List<TimeStampHistoryReportDataField>();
            EnumUtility.GetValues<TermGroup_TimeStampHistoryMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new TimeStampHistoryReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public TimeStampHistoryReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out _))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Content

                    List<TimeStampEntryDTO> timeStampEntries;
                    List<TrackChangesLogDTO> trackChangesLogs;

                    #region Load employee information

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees);

                    #endregion

                    #region Load Time Stamp Ids from selection (use as recordIds in GetTrackChangeLog)

                    timeStampEntries = TimeStampManager.GetTimeStampEntriesDTO(selectionDateFrom, selectionDateTo, selectionEmployeeIds, reportResult.ActorCompanyId, false);

                    #endregion

                    foreach (TimeStampEntryDTO timeStampEntry in timeStampEntries.OrderBy(s => s.EmployeeNr).ThenBy(s => s.Created).ToList())
                    {
                        Employee employee = employees.FirstOrDefault(e => e.EmployeeId == timeStampEntry.EmployeeId);

                        #region Load track changes log and create item

                        trackChangesLogs = TrackChangesManager.GetTrackChangesLog(reportResult.ActorCompanyId, SoeEntityType.TimeStampEntry, timeStampEntry.TimeStampEntryId, selectionDateFrom, selectionDateTo).OrderBy(s => s.Created).ToList();

                        foreach (TrackChangesLogDTO trackChangesLog in trackChangesLogs)
                        {
                            var item = new GenericTrackChangesItem();
                            item.Created = trackChangesLog.Created;
                            item.CreatedBy = trackChangesLog.CreatedBy ?? string.Empty;
                            item.Action = trackChangesLog.ActionText;
                            item.ActionMethod = trackChangesLog.ActionMethodText;
                            item.Column = trackChangesLog.ColumnText;
                            item.Batch = trackChangesLog.Batch;
                            item.BatchNbr = trackChangesLog.BatchNbr;
                            item.Entity = trackChangesLog.Entity;
                            item.EntityText = trackChangesLog.EntityText;
                            item.TopRecordName = trackChangesLog.TopRecordName;
                            item.TopEntity1Text = employee?.EmployeeNr ?? string.Empty;
                            item.TopEntity2Text = employee?.ContactPerson.Name ?? string.Empty;
                            item.FromValue = trackChangesLog.FromValueText;
                            item.ToValue = trackChangesLog.ToValueText;
                            item.RecordId = trackChangesLog.RecordId;
                            item.RecordName = trackChangesLog.RecordName;
                            item.Role = trackChangesLog.Role;

                            _reportDataOutput.TrackChangesItems.Add(item);
                        }

                        #endregion
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

    public class TimeStampHistoryReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_TimeStampHistoryMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public TimeStampHistoryReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_TimeStampHistoryMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_TimeStampHistoryMatrixColumns.Unknown;
        }
    }

    public class TimeStampHistoryReportDataInput
    {
        private CreateReportResult ReportResult { get; set; }
        public List<TimeStampHistoryReportDataField> Columns { get; set; }

        public TimeStampHistoryReportDataInput(CreateReportResult reportResult, List<TimeStampHistoryReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class TimeStampHistoryReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public TimeStampHistoryReportDataInput Input { get; set; }
        public List<GenericTrackChangesItem> TrackChangesItems { get; set; }
        public List<GenericType> OriginTypes { get; set; }
        public List<GenericType> Statuses { get; set; }

        public TimeStampHistoryReportDataOutput(TimeStampHistoryReportDataInput input)
        {
            this.Input = input;
            this.TrackChangesItems = new List<GenericTrackChangesItem>();
            this.OriginTypes = new List<GenericType>();
            this.Statuses = new List<GenericType>();
        }
    }
}

