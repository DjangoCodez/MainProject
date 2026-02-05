using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ProjectProductRowDTO
    {
        public string ProjectName { get; set; }
        public string ProjectNumber { get; set; }
        public int? ProjectId { get; set; }
        public string InvoiceNumber { get; set; }
        public int InvoiceId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string ArticleNumber { get; set; }
        public string ArticleName { get; set; }
        public int ProductType { get; set; }
        public string ProductGroupName { get; set; }
        public string MaterialCode { get; set; }
        public string Description { get; set; }
        public string AttestState { get; set; }
        public string AttestColor { get; set; }
        public string Unit { get; set; }
        public int CustomerInvoiceRowId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? PurchaseAmount { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal MarginalIncome { get; set; }
        public decimal? MarginalIncomeRatio { get; set; }
        public bool IsTimeProjectRow { get; set; }
        public int? SupplierInvoiceId { get; set; }
        public string Wholeseller { get; set; }

        public int? EdiEntryId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string SupplierName { get; set; }
        public int OriginStatus { get; set; }
    }
}
