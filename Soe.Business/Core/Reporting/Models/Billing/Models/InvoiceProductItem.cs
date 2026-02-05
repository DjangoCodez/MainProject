namespace SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models
{
    public class InvoiceProductItem
    {
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public bool  IsActive { get; set; }
        public int? ProductGroupId { get; set; }
        public string ProductCategoryNames { get; set; }
        public string ProductEAN { get; set; }
        public bool IsImported { get; set; }
        public int CalculationType { get; set; }
        public int ProductType { get; set; }
        public int VatType { get; set; }
        public int? ProductUnit { get; set; }
        public decimal? HouseholdDeductionPercentage { get; set; }
        public int? HouseholdDeductionType { get; set; }
        public decimal? Weight { get; set; }
        public int? VatCodeId { get; set; }



    }
}
