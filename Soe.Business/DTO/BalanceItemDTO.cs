using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.DTO
{
    public class GrossProfitAccountBalanceItemDTO
    {
        public int AccountId { get; set; }
        public int? GrossProfitInternalAccountId { get; set; }
        public List<GrossProfitBalanceItemDTO> BalanceItems { get; set; }
    }

    public class GrossProfitBalanceItemDTO : BalanceItemDTO
    {
        public int GrossProfitCodeId { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public decimal PeriodGrossProfitPercentage { get; set; }
    }
}
