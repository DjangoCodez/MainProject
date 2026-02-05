using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class SearchAgeDistributionDTO
    {
        public SoeInvoiceType Type { get; set; }
        public DateTime CompareDate { get; set; }
        public bool? InsecureDebts { get; set; }
        public int CurrencyType { get; set; }
        public string ActorNrFrom { get; set; }
        public string ActorNrTo { get; set; }
        public int? SeqNrFrom { get; set; }
        public int? SeqNrTo { get; set; }
        public string InvNrFrom { get; set; }
        public string InvNrTo { get; set; }
        public DateTime? InvDateFrom { get; set; }
        public DateTime? InvDateTo { get; set; }
        public DateTime? ExpDateFrom { get; set; }
        public DateTime? ExpDateTo { get; set; }
    }
}