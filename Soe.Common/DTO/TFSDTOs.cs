using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    public class WorkItemDTO
    {
        public int Id { get; set; }
        public TFSWorkItemType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
