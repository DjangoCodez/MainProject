using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class CommodityCodeDTO
    {
        public int SysIntrastatCodeId { get; set; }
        public int? IntrastatCodeId { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }
        public bool UseOtherQuantity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    [TSInclude]
    public class IntrastatTransactionDTO
    {
        public int IntrastatTransactionId { get; set; }
        public int OriginId { get; set; }
        public int IntrastatCodeId { get; set; }
        public int IntrastatTransactionType { get; set; }
        public int? ProductUnitId { get; set; }
        public int? SysCountryId { get; set; }
        public decimal? NetWeight { get; set; }
        public string OtherQuantity { get; set; }
        public bool NotIntrastat { get; set; }
        public decimal? Amount { get; set; }
        public SoeEntityState State { get; set; }   

        // Extensions
        public int? CustomerInvoiceRowId { get; set; }
        public int RowNr { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
        public string ProductUnitCode { get; set; }
    }

    [TSInclude]
    public class IntrastatTransactionExportDTO : IntrastatTransactionDTO
    {
        public int? ActorCountryId { get; set; }
        public int CurrencyId { get; set; }
        public int OriginType { get; set; }
        public int? SeqNr { get; set; }
        public string OriginNr { get; set; }
        public string OriginTypeName { get; set; }
        public string Name { get; set; }
        public string VatNr { get; set; }
        public string Country { get; set; }
        public string OriginCountry { get; set; }
        public string IntrastatCode { get; set; }
        public string IntrastatCodeName { get; set; }
        public string IntrastatTransactionTypeName { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal AmountCurrency { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsPrivatePerson { get; set; }
    }
}
