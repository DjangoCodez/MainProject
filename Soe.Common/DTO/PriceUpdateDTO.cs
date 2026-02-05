using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class PriceUpdateDTO
    {
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public decimal Rounding { get; set; }

        public decimal NewPrice(decimal inPrice, int maxDecimals = 2)
        {
            decimal newPrice = inPrice;
            if (this.Amount != 0)
            {
                newPrice += this.Amount;
            }
            else if (this.Percentage != 0)
            {
                //Percentage
                newPrice *=  1 + this.Percentage / 100;
            }
            
            if (this.Rounding != 0)
            {
                newPrice = Math.Round(newPrice / this.Rounding, 0, MidpointRounding.AwayFromZero) * this.Rounding;
            }
            else
            {
                newPrice = Math.Round(newPrice, maxDecimals);
            }
            return newPrice;
        }
    }
}
