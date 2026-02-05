namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCompServer")]
    public partial class SysCompServer : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysCompServer()
        {
            SysCompDb = new HashSet<SysCompDb>();
        }

        public int SysCompServerId { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(100)]
        public string SysServiceUrl { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysCompDb> SysCompDb { get; set; }
    }
}
