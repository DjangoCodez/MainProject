using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class VoucherRowHistoryDTO
    {
        public int VoucherRowHistoryId { get; set; }
        public int VoucherRowId { get; set; }
        public int AccountId { get; set; }
        public int UserId { get; set; }

        public DateTime? Date { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountEntCurrency { get; set; }
        public decimal? Quantity { get; set; }
        public string Text { get; set; }
        public string EventText { get; set; }
        public int EventType { get; set; }
        public int FieldModified { get; set; }
        public int VoucherHeadIdModified { get; set; }
        public int? AccountDimId { get; set; }
    }
    [TSInclude]
    public class VoucherRowHistoryViewDTO
    {
        public string EventType { get; set; }
        public string FieldModified { get; set; }
        public string EventText { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
        public DateTime DateTime { get; set; }
        public string Time { get; set; }
        public string UserName { get; set; }
    }
}
