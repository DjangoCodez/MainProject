using System;

namespace SoftOne.Soe.Business.Core.Tests
{
    public class SEPAV3TestPaymentRow
    {
        public int PaymentRowId { get; set; }
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal InvoicePaidAmount { get; set; }
        public bool FullyPayed { get; set; }
        public int Status { get; set; }
        public int? InvoiceActorId { get; set; }
        public string InvoiceActorName { get; set; }
        public int BillingType { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
        public DateTime PayDate { get; set; }
        public string PaymentNr { get; set; }
        public int? InvoiceSeqNr { get; set; }
        public int SysPaymentTypeId { get; set; }
    }
}
