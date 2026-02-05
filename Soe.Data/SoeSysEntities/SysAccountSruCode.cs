namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysAccountSruCode")]
    public partial class SysAccountSruCode : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysAccountSruCode()
        {
            SysAccountStd = new HashSet<SysAccountStd>();
        }

        public int SysAccountSruCodeId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string SruCode { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysAccountStd> SysAccountStd { get; set; }
    }
}
