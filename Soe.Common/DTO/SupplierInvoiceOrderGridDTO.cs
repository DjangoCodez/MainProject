using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class SupplierInvoiceOrderGridDTO
    {
        public int CustomerInvoiceId { get; set; }
        public int SupplierInvoiceId { get; set; }
        public int? CustomerInvoiceRowId { get; set; }
        public int? CustomerInvoiceRowAttestStateId { get; set; }
        public int? TimeCodeTransactionId { get; set; }
        public int? SeqNr { get; set; }
        public string Icon { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNr { get; set; }
        public decimal Amount { get; set; }
        public decimal InvoiceAmountExVat { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public bool IncludeImageOnInvoice { get; set; }
        public bool HasImage { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public SupplierInvoiceOrderLinkType SupplierInvoiceOrderLinkType { get; set; }


        // Target row 
        public string TargetCustomerInvoiceNr { get; set; }
        public DateTime? TargetCustomerInvoiceDate { get; set; }

        // Extras
        public int? EdiEntryId { get; set; }

        public List<int?> CustomerInvoiceRowIds { get; set; }
    }
}
