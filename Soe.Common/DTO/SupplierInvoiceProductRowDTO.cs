using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SupplierInvoiceProductRowDTO
    {
        public int SupplierInvoiceProductRowId { get; set; }
        public int SupplierInvoiceId { get; set; }
        public int? CustomerInvoiceRowId { get; set; }
        public int? CustomerInvoiceId { get; set; }
        public string SellerProductNumber { get; set; }
        public string Text { get; set; }
        public string UnitCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceCurrency { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatRate { get; set; }
        public string CustomerInvoiceNumber { get; set; }
        public SupplierInvoiceRowType RowType { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }
}
