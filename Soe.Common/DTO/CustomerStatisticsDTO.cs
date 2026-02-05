using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class OriginInvoiceMappingDTO
    {
        public int OriginInvoiceMappingId { get; set; }
        public int OriginId { get; set; }
        public int InvoiceId { get; set; }
        public string OriginNr { get; set; }
        public string InvoiceNr { get; set; }
        public SoeOriginInvoiceMappingType Type { get; set; }
    }

    [TSInclude]
    public class CustomerStatisticsDTO
    {
        public DateTime? Date { get; set; }
        public SoeOriginType OriginType { get; set; }
        public string OriginUsers { get; set; }
        public string MainUserName { get; set; }
        public int OrderType { get; set; }
        public string OrderTypeName { get; set; }
        public string InvoiceNr { get; set; }
        public string OrderNr { get; set; }

        public string CustomerName { get; set; }
        public string CustomerStreetAddress { get; set; }
        public string CustomerPostalAddress { get; set; }
        public string CustomerPostalCode { get; set; }
        public string CustomerCountry { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal ProductSumAmount { get; set; }
        public decimal ProductPurchasePrice { get; set; }
        public decimal ProductPurchasePriceCurrency { get; set; }
        public decimal ProductPurchaseAmount { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductMarginalIncome { get; set; }
        public decimal ProductMarginalRatio { get; set; }
        public string ProjectNr { get; set; }
        public string ContractCategory { get; set; }
        public string CustomerCategory { get; set; }
        public string OrderCategory { get; set; }
        public string ProductCategory { get; set; }
        public string CostCentre { get; set; }
        public string WholeSellerName { get; set; }
        public string ReferenceOur { get; set; }

        public string CurrencyCode { get; set; }
        public decimal ProductSumAmountCurrency { get; set; }

        public string PayingCustomerName { get; set; }

        public int AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }

        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }
        public string ParentProductCategories { get; set; }
    }
}

