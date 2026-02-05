using SoftOne.Soe.Business.Core.Template.Models.Economy;
using SoftOne.Soe.Business.Core.Template.Models.Time;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Billing
{
    public class InvoiceProductCopyItem
    {
        public int ProductId { get; set; }

        public int Type { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AccountingPrio { get; set; }
        public int CalculationType { get; set; }
        public int VatType { get; set; }
        public int? VatCodeId { get; set; }
        public decimal PurchasePrice { get; set; }

        public ProductUnitCopyItem ProductUnit { get; set; }
        public ProductGroupCopyItem ProductGroup { get; set; }
        public TimeCodeCopyItem TimeCode { get; set; }
        public List<ProductAccountStdCopyItem> ProductAccountStds { get; set; } = new List<ProductAccountStdCopyItem>();
        public bool VatFree { get; set; }
        public string EAN { get; set; }
        public string SysWholesellerName { get; set; }
        public int PriceListOrigin { get; set; }
        public bool ShowDescriptionAsTextRow { get; set; }
        public bool? DontUseDiscountPercent { get; set; }
        public decimal? HouseholdDeductionPercentage { get; set; }
        public bool? IsStockProduct { get; set; }
        public decimal? GuaranteePercentage { get; set; }
        public int? HouseholdDeductionType { get; set; }
        public bool? UseCalculatedCost { get; set; }
        public decimal? Weight { get; set; }
        public bool ShowDescrAsTextRowOnPurchase { get; set; }
        public int? ExternalProductId { get; set; }
        public int? ExternalPriceListHeadId { get; set; }
        // Add additional properties as needed

    }

    #region ProductUnitCopyItem

    public class ProductUnitCopyItem
    {
        public int ProductUnitId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

    }

    #endregion

    #region ProductGroupCopyItem

    public class ProductGroupCopyItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region ProductAccountStdCopyItem

    public class ProductAccountStdCopyItem
    {
        public int Type { get; set; }
        public int? Percent { get; set; }
        public AccountStdCopyItem AccountStd { get; set; }
    }

    #endregion


}
