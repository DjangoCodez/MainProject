using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class InvoiceMatchingDTO
    {
        public bool IsSelected { get; set; }
        public bool IsCredit { get; set; }
        public bool IsEditable { get; set; }
        public bool HasMatches { get; set; }
        public int InvoiceId { get; set; }
        public int PaymentRowId { get; set; }
        public int VoucherHeadId { get; set; }
        public int InvoiceMatchingId { get; set; }
        public int ActorId { get; set; }
        public int MatchCodeId { get; set; }
        public string InvoiceNr { get; set; }
        public string OrderNr { get; set; }
        public string PaymentNr { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal InvoiceVatAmount { get; set; }
        public decimal InvoicePayedAmount { get; set; }
        public decimal InvoiceVatRate { get; set; }
        public int InvoiceVatAccountId { get; set; }
        public int Type { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public string TypeName { get; set; }
        public int? TypeNameId { get; set; }
        public decimal Amount { get; set; }
        public string ActorName { get; set; }
        public decimal InvoiceMatchAmount { get; set; }
        public int OriginStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Count { get; set; }
        public DateTime? Date { get; set; }
        public OrderInvoiceRegistrationType registrationType { get; set; }
    }

    public class MatchingCustomerSupplierDTO
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public decimal Sum { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }

        public MatchingCustomerSupplierDTO()
        {
        }

        public MatchingCustomerSupplierDTO(int id, string number, string name, int count, decimal sum)
        {
            Id = id;
            Number = number;
            Name = name;
            Count = count;
            Sum = sum;
        }
    }
}
