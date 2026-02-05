using System;

namespace SoftOne.Soe.Common.DTO
{
    public class CustomerInvoiceInsecureDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string InvoiceNrSort { get; set; }

        public int Status { get; set; }
        public string StatusName { get; set; }

        public int? ActorId { get; set; }
        public string CustomerName { get; set; }

        public bool InsecureDebt { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string InternalText { get; set; }

        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; }
    }
}
