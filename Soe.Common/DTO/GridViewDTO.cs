using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class SortItem
    {
        public string dir { get; set; }
        public string field { get; set; }

        public string dbPath { get; set; }
    }

    public class FilterItem
    {
        public string field { get; set; }
        public string @operator { get; set; }
        public string value { get; set; }
        public string type { get; set; }
        public string dbPath { get; set; }
    }

    public class Filter
    {
        public List<FilterItem> filters { get; set; }
        public string logic { get; set; }
    }
}
