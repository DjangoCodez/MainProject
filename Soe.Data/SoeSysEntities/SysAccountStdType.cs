namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysAccountStdType")]
    public partial class SysAccountStdType : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysAccountStdType()
        {
            SysAccountStd = new HashSet<SysAccountStd>();
            SysAccountStdType1 = new HashSet<SysAccountStdType>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysAccountStdTypeId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(30)]
        public string ShortName { get; set; }

        public int? SysAccountStdTypeParentId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysAccountStd> SysAccountStd { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysAccountStdType> SysAccountStdType1 { get; set; }

        public virtual SysAccountStdType SysAccountStdType2 { get; set; }
    }
}
