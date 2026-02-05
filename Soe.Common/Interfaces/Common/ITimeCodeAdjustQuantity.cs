using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface ITimeCodeAdjustQuantity
    {
        TermGroup_AdjustQuantityByBreakTime AdjustQuantityByBreakTime { get; set; }
        int? AdjustQuantityTimeCodeId { get; set; }
        int? AdjustQuantityTimeScheduleTypeId { get; set; }
    }
}
