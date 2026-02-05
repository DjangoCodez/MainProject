using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class InvoiceProductPriceResult
    {
        public bool Success { get; set; }
        public bool Warning { get; set; }
        public bool IsForTimeProjectInvoiceProductRow { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? CustomerDiscountPercent { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }
        public bool CurrencyDiffer { get; set; }
        public string PriceFormula { get; set; }
        public string SysWholesellerName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public string ProductUnit { get; set; }
        public virtual bool ProductIsSupplementCharge { get; set; }
        public int RowId { get; set; }

        public bool InclusiveVat { get; set; }

        public InvoiceProductPriceResult()
        {
            Success = true;
        }

        public InvoiceProductPriceResult(ActionResultSelect errorNumber)
        {
            Success = false;
            ErrorNumber = (int)errorNumber;
        }

        public InvoiceProductPriceResult(ActionResultSelect errorNumber, string errorMessage)
        {
            Success = false;
            ErrorNumber = (int)errorNumber;
            ErrorMessage = errorMessage;
        }
    }

    [TSInclude]
    public class InvoiceProductPriceResultIODTO
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public string ErrorMessage { get; set; }
        public bool InclusiveVat { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Quantity { get; set; }
    }

    [TSInclude]
    public class InvoiceProductPriceSearchIODTO
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    [TSInclude]
    public class InvoiceProductExternalDTO
    {
        public InvoiceProductExternalDTO(int externalPriceListId, int externalProductId, int priceListOrigin)
        {
            this.ExternalPriceListId = externalPriceListId;
            this.ExternalProductId = externalProductId;
            this.PriceListOrigin = priceListOrigin;
        }
        public int ExternalPriceListId { get; private set; }
        public int ExternalProductId { get; private set; }
        public int PriceListOrigin { get; private set; }
    }
}
