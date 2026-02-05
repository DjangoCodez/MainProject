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
    public class SoftOneStatusUpTimeReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly SoftOneStatusUpTimeReportDataOutput _reportDataOutput;

        public SoftOneStatusUpTimeReportData(ParameterObject parameterObject, SoftOneStatusUpTimeReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new SoftOneStatusUpTimeReportDataOutput(reportDataInput);
        }

        public SoftOneStatusUpTimeReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            _reportDataOutput.ResultItems = new List<SoftOneStatusUpTimeItem>();

            var aggregates = SoftOneStatusConnector.GetStatusServiceGroupUpTimes(selectionDateFrom, selectionDateTo, null);
            var groups = SoftOneStatusConnector.GetStatusServiceGroups();

            foreach (var agg in aggregates)
            {
                SoftOneStatusUpTimeItem item = new SoftOneStatusUpTimeItem()
                {
                    Date    = agg.UpTimeDate,
                    UpTimeOnDate = agg.UpTimeOnDate,
                    MobileUpTimeOnDate = agg.MobileUpTimeOnDate,
                    WebUpTimeOnDate = agg.WebUpTimeOnDate,
                    TotalUpTimeOnDate = agg.TotalUpTimeOnDate,
                    StatusServiceGroupName = groups.FirstOrDefault(f => f.StatusServiceGroupId == agg.StatusServiceGroupId)?.Name.ToString() ?? ""
                };

                _reportDataOutput.ResultItems.Add(item);

            }
            #endregion

            return new ActionResult();
        }
    }

    public class SoftOneStatusUpTimeReportDataReportDataField
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

        public SoftOneStatusUpTimeReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_SoftOneStatusEventMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_SoftOneStatusEventMatrixColumns.Unknown;
        }
    }

    public class SoftOneStatusUpTimeReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<SoftOneStatusUpTimeItem> ResultItems { get; set; }
        public SoftOneStatusUpTimeReportDataInput Input { get; set; }

        public SoftOneStatusUpTimeReportDataOutput(SoftOneStatusUpTimeReportDataInput input)
        {
            this.ResultItems = new List<SoftOneStatusUpTimeItem>();
            this.Input = input;
        }
    }

    public class SoftOneStatusUpTimeReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<SoftOneStatusUpTimeReportDataReportDataField> Columns { get; set; }

        public SoftOneStatusUpTimeReportDataInput(CreateReportResult reportResult, List<SoftOneStatusUpTimeReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

}
