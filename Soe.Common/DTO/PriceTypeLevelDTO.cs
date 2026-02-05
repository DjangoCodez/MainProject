
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class PriceTypeLevelDTO
    {
        public int PayrollGroupId { get; set; }
        public int PayrollPriceTypeId { get; set; }
        public bool HasLevels { get; set; }
        public List<int> SelectableLevelIds { get; set; }
        public bool LevelIsMandatory { get; set; }
    }
}
