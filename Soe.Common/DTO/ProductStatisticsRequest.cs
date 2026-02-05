using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ProductStatisticsRequest
    {
        public IEnumerable<int> ProductIds { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public SoeOriginType OriginType { get; set; }
        public bool IncludeServiceProducts { get; set; }
    }
}
