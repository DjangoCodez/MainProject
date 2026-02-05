using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SearchVoucherRowDTO
    {
        public int ActorCompanyId { get; set; }

        //Voucher head and row
        public int VoucherHeadId { get; set; }
        public int VoucherRowId { get; set; }
        public long VoucherNr { get; set; }
        public string VoucherText { get; set; }
        public string VoucherSeriesName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }

        //Dimensions
        public int Dim1AccountId { get; set; }
        public string Dim1AccountNr { get; set; }
        public string Dim1AccountName { get; set; }
        public int Dim2AccountId { get; set; }
        public string Dim2AccountNr { get; set; }
        public string Dim2AccountName { get; set; }
        public int Dim3AccountId { get; set; }
        public string Dim3AccountNr { get; set; }
        public string Dim3AccountName { get; set; }
        public int Dim4AccountId { get; set; }
        public string Dim4AccountNr { get; set; }
        public string Dim4AccountName { get; set; }
        public int Dim5AccountId { get; set; }
        public string Dim5AccountNr { get; set; }
        public string Dim5AccountName { get; set; }
        public int Dim6AccountId { get; set; }
        public string Dim6AccountNr { get; set; }
        public string Dim6AccountName { get; set; }

        public DateTime VoucherDate { get; set; }

        [TSIgnore]
        public VoucherHeadDTO VoucherHead { get; set; }

        // Extensions
        public int? InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public SoeOriginType? InvoiceOriginType { get; set; }
        public OrderInvoiceRegistrationType? InvoiceRegistrationType { get; set; }
    }

    [TSInclude]
    public class SearchVoucherRowsAngDTO
    {
        public int ActorCompanyId { get; set; }

        //Voucher search
        public DateTime? VoucherDateFrom { get; set; }
        public DateTime? VoucherDateTo { get; set; }
        public int VoucherSeriesIdFrom { get; set; }
        public int VoucherSeriesIdTo { get; set; }
        public decimal DebitFrom { get; set; }
        public decimal DebitTo { get; set; }
        public decimal CreditFrom { get; set; }
        public decimal CreditTo { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
        public string VoucherText { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string CreatedBy { get; set; }

        //Dimensions
        public int Dim1AccountId { get; set; }
        public string Dim1AccountFr { get; set; }
        public string Dim1AccountTo { get; set; }
        public int Dim2AccountId { get; set; }
        public string Dim2AccountFr { get; set; }
        public string Dim2AccountTo { get; set; }
        public int Dim3AccountId { get; set; }
        public string Dim3AccountFr { get; set; }
        public string Dim3AccountTo { get; set; }
        public int Dim4AccountId { get; set; }
        public string Dim4AccountFr { get; set; }
        public string Dim4AccountTo { get; set; }
        public int Dim5AccountId { get; set; }
        public string Dim5AccountFr { get; set; }
        public string Dim5AccountTo { get; set; }
        public int Dim6AccountId { get; set; }
        public string Dim6AccountFr { get; set; }
        public string Dim6AccountTo { get; set; }

        //For New Angular
        public int[] VoucherSeriesTypeIds { get; set; }

    }



}
