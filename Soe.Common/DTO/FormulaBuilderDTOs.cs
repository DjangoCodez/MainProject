using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class ValidatePriceRuleDTO
    {
        public List<ValidatePriceRuleItemDTO> Items { get; set; }
    }

    public class ValidatePriceRuleItemDTO
    {
        public int Sort { get; set; }
        public PriceRuleItemType PriceRuleType { get; set; }
        public object Data { get; set; }
    }
}
