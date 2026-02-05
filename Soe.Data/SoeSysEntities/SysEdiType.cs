namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysEdiType")]
    public partial class SysEdiType : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysEdiType()
        {
            SysEdiMsg = new HashSet<SysEdiMsg>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysEdiTypeId { get; set; }

        [StringLength(50)]
        public string TypeCode { get; set; }

        [StringLength(50)]
        public string TypeName { get; set; }

        [StringLength(50)]
        public string TypeFolder { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysEdiMsg> SysEdiMsg { get; set; }
    }
}
