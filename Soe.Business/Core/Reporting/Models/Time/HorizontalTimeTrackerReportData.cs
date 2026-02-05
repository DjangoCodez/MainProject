using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class HorizontalTimeTrackerReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly HorizontalTimeTrackerReportDataOutput _reportDataOutput;

        public HorizontalTimeTrackerReportData(ParameterObject parameterObject, HorizontalTimeTrackerReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new HorizontalTimeTrackerReportDataOutput(reportDataInput);
        }

        public HorizontalTimeTrackerReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return null;
        }

        private ActionResult LoadData()
        {
            try
            {
                return new ActionResult(true);
            }
            catch
            {
                return new ActionResult(false);
            }
        }

    }

    public class HorizontalTimeTrackerReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_VerticalTimeTrackerMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public HorizontalTimeTrackerReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_VerticalTimeTrackerMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_VerticalTimeTrackerMatrixColumns.EmployeeNr;
        }
    }

    public class HorizontalTimeTrackerItem : VerticalTimeTrackerItem
    {
        public HorizontalTimeTrackerItem() : base()
        {
        }
    }

    public class HorizontalTimeTrackerReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<HorizontalTimeTrackerReportDataField> Columns { get; set; }

        public HorizontalTimeTrackerReportDataInput(CreateReportResult reportResult, List<HorizontalTimeTrackerReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class HorizontalTimeTrackerReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public HorizontalTimeTrackerReportDataInput Input { get; set; }
        public List<HorizontalTimeTrackerItem> HorizontalTimeTrackerItems { get; set; }

        public HorizontalTimeTrackerReportDataOutput(HorizontalTimeTrackerReportDataInput input)
        {
            this.Input = input;
            this.HorizontalTimeTrackerItems = new List<HorizontalTimeTrackerItem>();
        }
    }
}
