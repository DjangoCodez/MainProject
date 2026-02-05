namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysSetting")]
    public partial class SysSetting : SysEntity
    {
        public int SysSettingId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public int SysSettingTypeId { get; set; }

        public virtual SysSettingType SysSettingType { get; set; }
    }
}
