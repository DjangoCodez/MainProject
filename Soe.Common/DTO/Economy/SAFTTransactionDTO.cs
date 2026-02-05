using System;

namespace SoftOne.Soe.Common.DTO
{

    public class SAFTTransactionDTO
    {
        public long VoucherNr { get; set; }
        public int RowNr { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime Date { get; set; }
        public string VoucherText { get; set; }
        public decimal DebetAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal? VatRate { get; set; }
        public string VatCode { get; set; }
        public decimal TaxAmount { get; set; }
        public string CustomerId { get; set; }
        public string SupplierId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public int VoucherHeadId { get; set; }
        public string SupplierCustomerName { get; set; }
        public string AccountCode { get; set; }
    }
    
    public class SAFTActorBalanceDTO
    {
        public int ActorId { get; set; }
        public decimal StartBalance { get; set; }
        public decimal EndBalance { get; set; }
    }

}
