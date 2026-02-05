using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SearchInvoicesPaymentsAndMatchesDTO
    {
        public int ActorId { get; set; }
        public int Type { get; set; }
        public decimal AmountFrom { get; set; }
        public decimal AmountTo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public SoeOriginType OriginType { get; set; }
    }
}