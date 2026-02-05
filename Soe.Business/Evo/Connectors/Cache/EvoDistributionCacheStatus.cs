using System;

namespace SoftOne.Soe.Business.Evo.Connectors.Cache
{
    public class EvoDistributionCacheStatus
    {
        public string Key { get; set; }
        public DateTimeOffset LastChangeUtc { get; set; }
        public int OriginalTtlSeconds { get; set; }
    }
}

