using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Status.Models;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage
{
    public class SoftOneStatusEventReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly SoftOneStatusEventReportDataOutput _reportDataOutput;

        public SoftOneStatusEventReportData(ParameterObject parameterObject, SoftOneStatusEventReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new SoftOneStatusEventReportDataOutput(reportDataInput);
        }

        public SoftOneStatusEventReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Get selections

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);

            #endregion

            #region  Fetch data

            _reportDataOutput.ResultItems = new List<SoftOneStatusEventItem>();

            var events = SoftOneStatusConnector.GetStatusEventDTOs(selectionDateFrom, selectionDateTo);
            var serviceTypes = SoftOneStatusConnector.GetStatusServiceTypes();
            foreach (var ev in events)
            {
                SoftOneStatusEventItem item = new SoftOneStatusEventItem()
                {
                    Start = ev.Start,
                    StatusServiceTypeName = serviceTypes.FirstOrDefault(f => f.StatusServiceTypeId == ev.StatusServiceTypeId)?.ServiceType.ToString() ?? "",
                    End = ev.End,
                    LastMessageSent = ev.LastMessageSent,
                    Message = ev.Message,
                    Minutes = ev.End.HasValue ? Convert.ToInt32((ev.End.Value - ev.Start).TotalMinutes) : Convert.ToInt32((DateTime.UtcNow - ev.Start).TotalMinutes),
                    Prio = ev.Prio,
                    Url = ev.Url,
                    StatusEventTypeName = ev.StatusEventType.ToString(),
                    JobDescriptionName = ev.JobDescription.ToString(),
                };

                _reportDataOutput.ResultItems.Add(item);
            }

            #endregion

            return new ActionResult();
        }
    }

    public class SoftOneStatusEventReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_SoftOneStatusEventMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public SoftOneStatusEventReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_SoftOneStatusEventMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_SoftOneStatusEventMatrixColumns.Unknown;
        }
    }

    public class SoftOneStatusEventReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<SoftOneStatusEventReportDataReportDataField> Columns { get; set; }

        public SoftOneStatusEventReportDataInput(CreateReportResult reportResult, List<SoftOneStatusEventReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class SoftOneStatusEventReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<SoftOneStatusEventItem> ResultItems { get; set; }
        public SoftOneStatusEventReportDataInput Input { get; set; }

        public SoftOneStatusEventReportDataOutput(SoftOneStatusEventReportDataInput input)
        {
            this.ResultItems = new List<SoftOneStatusEventItem>();
            this.Input = input;
        }
    }
}
