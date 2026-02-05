namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class SkillCopyItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillTypeCopyItem SkillType { get; set; }
    }

    public class SkillTypeCopyItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
