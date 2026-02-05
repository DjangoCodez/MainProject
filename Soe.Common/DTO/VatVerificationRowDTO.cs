using System;

namespace SoftOne.Soe.Common.DTO
{
    public class VatVerificationVoucherRowDTO
    {
        public int VoucherHeadId { get; set; }
        public int VoucherRowId { get; set; }
        public long VoucherNr { get; set; }
        public string VoucherText { get; set; }
        public string VoucherSeriesName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        public int PeriodNr { get; set; }
        public string PeriodMonthYear { get; set; }
        public decimal Vat25Amount { get; set; }
        public decimal Vat12Amount { get; set; }
        public decimal Vat6Amount { get; set; }
        public decimal Tax25Amount { get; set; }
        public decimal Tax12Amount { get; set; }
        public decimal Tax6Amount { get; set; }
        public decimal IncomingVATAmount { get; set; }
        public decimal VATSalesSumAmount { get; set; }
        public decimal DiffAmount { get; set; }
        public decimal TaxSalesDueVATAmount { get; set; }

        public DateTime VoucherDate { get; set; }

        public VoucherHeadDTO VoucherHead { get; set; }

    }
}
