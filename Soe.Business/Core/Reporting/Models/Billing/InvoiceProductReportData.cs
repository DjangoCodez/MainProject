using SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Billing
{
    public class InvoiceProductReportData : BillingReportDataManager, IReportDataModel
    {
        private readonly InvoiceProductReportDataInput _reportDataInput;
        private readonly InvoiceProductReportDataOutput _reportDataOutput;

        private bool loadProductCalculationTypes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_InvoiceProductMatrixColumns.CalculationType);
        private bool loadProductVatTypes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_InvoiceProductMatrixColumns.ProductType);
        private bool loadProductVatCodes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_InvoiceProductMatrixColumns.VatCodeName);
        private bool loadProductUnits => _reportDataInput.Columns.Any(a => a.Column == TermGroup_InvoiceProductMatrixColumns.ProductUnitName);
        private bool loadHouseholdDeductionTypes => _reportDataInput.Columns.Any(a => a.Column == TermGroup_InvoiceProductMatrixColumns.HouseholdDeductionType);
        private bool loadProductGroups => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_InvoiceProductMatrixColumns.ProductGroupName ||
            a.Column == TermGroup_InvoiceProductMatrixColumns.ProductGroupCode
        );
        private bool loadInvoiceRowData => _reportDataInput.Columns.Any(a =>
            a.Column == TermGroup_InvoiceProductMatrixColumns.SalesAmount ||
            a.Column == TermGroup_InvoiceProductMatrixColumns.SalesQuantity
        );

        public InvoiceProductReportData(ParameterObject parameterObject, InvoiceProductReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new InvoiceProductReportDataOutput(reportDataInput);
        }

        public InvoiceProductReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            using (CompEntities entities = new CompEntities())
            {
                if (loadProductCalculationTypes)
                    _reportDataOutput.ProductCalculationTypes = GetTermGroupContent(TermGroup.InvoiceProductCalculationType);

                if (loadProductVatTypes)
                    _reportDataOutput.ProductVatTypes = GetTermGroupContent(TermGroup.InvoiceProductVatType);

                if (loadProductVatCodes)
                    _reportDataOutput.ProductVatCodes = AccountManager.GetVatCodes(base.ActorCompanyId).ToDTOs().ToList();

                if (loadProductUnits)
                    _reportDataOutput.ProductUnits = ProductManager.GetProductUnits(base.ActorCompanyId).ToList();

                if (loadHouseholdDeductionTypes)
                    _reportDataOutput.HouseholdDeductionTypes = ProductManager.GetSysHouseholdType(false);

                if (loadProductGroups)
                    _reportDataOutput.ProductGroups = ProductGroupManager.GetProductGroups(base.ActorCompanyId);

                if (loadInvoiceRowData)
                    _reportDataOutput.ProductStatistics = ProductManager.GetInvoiceProductStatistics(entities, base.ActorCompanyId, DateTime.MinValue, DateTime.MaxValue);

                List<InvoiceProduct> invoiceProducts = ProductManager.GetInvoiceProducts(base.ActorCompanyId, !selectionIncludeInactive);
                
                foreach (InvoiceProduct invoiceProduct in invoiceProducts)
                {
                    #region Prereq
                    
                    #endregion
                    
                    #region InvoiceProduct element
                    InvoiceProductItem invoiceProductItem = new InvoiceProductItem()
                    {
                        ProductId = invoiceProduct.ProductId,
                        ProductNr = invoiceProduct.Number,
                        ProductName = invoiceProduct.Name,
                        ProductDescription = invoiceProduct.Description,
                        IsActive = invoiceProduct.State == (int)SoeEntityState.Active,
                        ProductGroupId = invoiceProduct.ProductGroupId,
                        ProductCategoryNames = invoiceProduct.CategoryNamesString ?? String.Empty,
                        ProductEAN = invoiceProduct.EAN,
                        IsImported = invoiceProduct.ExternalProductId.HasValue,
                        ProductType = invoiceProduct.Type,
                        VatType = invoiceProduct.VatType,
                        CalculationType = invoiceProduct.CalculationType,
                        ProductUnit = invoiceProduct.ProductUnitId,
                        VatCodeId = invoiceProduct.VatCodeId,
                        HouseholdDeductionPercentage = invoiceProduct.HouseholdDeductionPercentage,
                        HouseholdDeductionType = invoiceProduct.HouseholdDeductionType,
                        Weight = invoiceProduct.Weight
                    };

                    #endregion

                    _reportDataOutput.InvoiceProductItems.Add(invoiceProductItem);
                }
            }
            return new ActionResult();
        }
    }

    public class InvoiceProductReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_InvoiceProductMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public InvoiceProductReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_InvoiceProductMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_InvoiceProductMatrixColumns.Unknown;
        }
    }

    public class InvoiceProductReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<InvoiceProductReportDataReportDataField> Columns { get; set; }

        public InvoiceProductReportDataInput(CreateReportResult reportResult, List<InvoiceProductReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class InvoiceProductReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<InvoiceProductItem> InvoiceProductItems { get; set; }
        public InvoiceProductReportDataInput Input { get; set; }
        public List<GenericType> ProductCalculationTypes { get; set; }
        public List<GenericType> ProductVatTypes { get; set; }
        public List<VatCodeDTO> ProductVatCodes { get; set; }
        public List<ProductUnit> ProductUnits { get; set; }
        public List<SysHouseholdType> HouseholdDeductionTypes { get; set; }
        public List<ProductGroup> ProductGroups { get; set; }
        public List<InvoiceProductStatisticsDTO> ProductStatistics { get; set; }

        public InvoiceProductReportDataOutput(InvoiceProductReportDataInput input)
        {
            this.InvoiceProductItems = new List<InvoiceProductItem>();
            this.Input = input;
            this.ProductCalculationTypes = new List<GenericType>();
            this.ProductVatTypes = new List<GenericType>();
            this.ProductVatCodes = new List<VatCodeDTO>();
            this.ProductUnits = new List<ProductUnit>();
            this.HouseholdDeductionTypes = new List<SysHouseholdType>();
            this.ProductGroups = new List<ProductGroup>();
        }
    }
}
