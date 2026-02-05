namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCompDb")]
    public partial class SysCompDb : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysCompDb()
        {
            SysCompany = new HashSet<SysCompany>();
            SysInformationSysCompDb = new HashSet<SysInformationSysCompDb>();
        }

        public int SysCompDbId { get; set; }

        public int SysCompServerId { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        [StringLength(256)]
        public string ApiUrl { get; set; }

        public int Type { get; set; }

        public int? ParentSysCompDbId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysCompany> SysCompany { get; set; }

        public virtual SysCompServer SysCompServer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysInformationSysCompDb> SysInformationSysCompDb { get; set; }
    }
}
