using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class FinnishTaxExportDTO
    {
        public int LengthOfTaxPeriod { get; set; }
        public int TaxPeriod { get; set; }
        public int TaxPeriodYear { get; set; }
        public bool NoActivity { get; set; }
        public bool Correction { get; set; }
        public int Cause { get; set; }

        [TSIgnore]
        public string LengthOfTaxPeriodStr
        {
            get 
            {
                switch (LengthOfTaxPeriod)
                {
                    case (int)TermGroup_FinnishTaxReturnExportTaxPeriodLength.Month:
                        return "K";
                    case (int)TermGroup_FinnishTaxReturnExportTaxPeriodLength.Quarter:
                        return "Q";
                    case (int)TermGroup_FinnishTaxReturnExportTaxPeriodLength.Year:
                        return "V";
                    default:
                        throw new ArgumentException("Skatteperiodens längdvärde ogiltigt");
                }
            }
        }

    }

    [TSInclude]
    public class FinnishTaxExporFiletDTO
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Data { get; set; }
    }
}
