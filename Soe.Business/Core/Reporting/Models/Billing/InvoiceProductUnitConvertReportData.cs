using SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Billing
{
    public class InvoiceProductUnitConvertReportData : BillingReportDataManager, IReportDataModel
    {
        private readonly InvoiceProductUnitConvertReportDataInput _reportDataInput;
        private readonly InvoiceProductUnitConvertReportDataOutput _reportDataOutput;


        public InvoiceProductUnitConvertReportData(ParameterObject parameterObject, InvoiceProductUnitConvertReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new InvoiceProductUnitConvertReportDataOutput(reportDataInput);
        }

        public InvoiceProductUnitConvertReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            TryGetBoolFromSelection(reportResult, out bool selectionIncludeInactive, "includeInactive");

            using (var entities = new CompEntities())
            {
                
                var conversions = ProductManager.GetProductUnitConvertDTOs(entities, selectionIncludeInactive);
                
                foreach (var convert in conversions)
                {
                    #region Prereq
                    
                    #endregion
                    
                    #region InvoiceProduct element
                    var item = new InvoiceProductUnitConvertItem
                    {
                        ProductId = convert.ProductId,
                        ProductNr = convert.ProductNr,
                        ProductName = convert.ProductName,
                        ProductUnitName = convert.BaseProductUnitName,
                        ProductConvertUnitName = convert.ProductUnitName,
                        ConvertFactor = convert.ConvertFactor,
                        CreatedBy = convert.CreatedBy,
                        Created = convert.Created,
                        ModifiedBy = convert.ModifiedBy,
                        Modified = convert.Modified
                    };

                    #endregion

                    _reportDataOutput.Items.Add(item);
                }
            }
            return new ActionResult();
        }
    }

    public class InvoiceProductUnitConvertReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_InvoiceProductUnitConvertMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public InvoiceProductUnitConvertReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_InvoiceProductUnitConvertMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_InvoiceProductUnitConvertMatrixColumns.Unknown;
        }
    }

    public class InvoiceProductUnitConvertReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<InvoiceProductUnitConvertReportDataField> Columns { get; set; }

        public InvoiceProductUnitConvertReportDataInput(CreateReportResult reportResult, List<InvoiceProductUnitConvertReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class InvoiceProductUnitConvertReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<InvoiceProductUnitConvertItem> Items { get; set; }
        public InvoiceProductUnitConvertReportDataInput Input { get; set; }

        public InvoiceProductUnitConvertReportDataOutput(InvoiceProductUnitConvertReportDataInput input)
        {
            this.Items = new List<InvoiceProductUnitConvertItem>();
            this.Input = input;
        }
    }
}
