using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class SysCurrencyRateDTO
    {
        public TermGroup_Currency CurrencyFrom { get; set; }
        public TermGroup_Currency CurrencyTo { get; set; }
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
    }
}
