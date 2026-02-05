using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class PositionCopyItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? SysPositionId { get; set; }
        public List<PositionSkillCopyItem> PositionSkillCopyItems { get; set; } = new List<PositionSkillCopyItem>();
    }

    public class PositionSkillCopyItem
    {
        public string SkillName { get; set; }
        public int SkillLevel { get; set; }
        public SkillTypeCopyItem SkillType { get; set; }
    }



}
