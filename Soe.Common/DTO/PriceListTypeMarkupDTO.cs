using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class PriceListTypeMarkupDTO
    {
        public int PriceListTypeId { get; set; }
        public decimal Markup { get; set; }
    }
}
