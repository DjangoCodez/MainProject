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
    public class SoftOneStatusResultReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly SoftOneStatusResultReportDataOutput _reportDataOutput;

        public SoftOneStatusResultReportData(ParameterObject parameterObject, SoftOneStatusResultReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new SoftOneStatusResultReportDataOutput(reportDataInput);
        }

        public SoftOneStatusResultReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            _reportDataOutput.ResultItems = new List<SoftOneStatusResultItem>();

            var aggregates = SoftOneStatusConnector.GetStatusResultAggregates(selectionDateFrom, selectionDateTo);
            var serviceTypes = SoftOneStatusConnector.GetStatusServiceTypes();

            foreach (var agg in aggregates)
            {
                SoftOneStatusResultItem item = new SoftOneStatusResultItem()
                {
                    Date = agg.Date,
                    Succeded = agg.Succeded,
                    Average = agg.Average,
                    Created = agg.Created,
                    Failed = agg.Failed,
                    Hour = agg.Hour,
                    Max = agg.Max,
                    Median = agg.Median,
                    Percential10 = agg.Percential10,
                    Min = agg.Min,
                    Percential90 = agg.Percential90,
                    ServiceTypeName = serviceTypes.FirstOrDefault(f => f.StatusServiceTypeId == agg.StatusServiceTypeId)?.ServiceType.ToString() ?? ""
                };

                _reportDataOutput.ResultItems.Add(item);
            }
            #endregion

            return new ActionResult();
        }
    }

    public class SoftOneStatusResultReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_SoftOneStatusResultMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public SoftOneStatusResultReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_SoftOneStatusResultMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_SoftOneStatusResultMatrixColumns.Unknown;
        }
    }

    public class SoftOneStatusResultReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<SoftOneStatusResultItem> ResultItems { get; set; }
        public SoftOneStatusResultReportDataInput Input { get; set; }

        public SoftOneStatusResultReportDataOutput(SoftOneStatusResultReportDataInput input)
        {
            this.ResultItems = new List<SoftOneStatusResultItem>();
            this.Input = input;
        }
    }

    public class SoftOneStatusResultReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<SoftOneStatusResultReportDataReportDataField> Columns { get; set; }

        public SoftOneStatusResultReportDataInput(CreateReportResult reportResult, List<SoftOneStatusResultReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }
}
