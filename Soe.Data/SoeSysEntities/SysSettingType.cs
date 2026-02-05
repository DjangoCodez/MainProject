namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysSettingType")]
    public partial class SysSettingType : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysSettingType()
        {
            SysSetting = new HashSet<SysSetting>();
        }

        public int SysSettingTypeId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysSetting> SysSetting { get; set; }
    }
}
