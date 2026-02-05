namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysServer")]
    public partial class SysServer : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysServer()
        {
            SysServerLogin = new HashSet<SysServerLogin>();
        }

        public int SysServerId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Url { get; set; }

        public bool UseLoadBalancer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysServerLogin> SysServerLogin { get; set; }
    }
}
