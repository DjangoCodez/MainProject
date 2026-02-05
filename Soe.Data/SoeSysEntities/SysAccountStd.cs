namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysAccountStd")]
    public partial class SysAccountStd : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysAccountStd()
        {
            SysAccountSruCode = new HashSet<SysAccountSruCode>();
        }

        public int SysAccountStdId { get; set; }

        public int SysAccountStdTypeId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(10)]
        public string AccountNr { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(255)]
        public string Name { get; set; }

        public int AccountTypeSysTermId { get; set; }

        public int? SysVatAccountId { get; set; }

        public int AmountStop { get; set; }

        [StringLength(10)]
        public string Unit { get; set; }

        public bool UnitStop { get; set; }

        public virtual SysAccountStdType SysAccountStdType { get; set; }

        public virtual SysVatAccount SysVatAccount { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysAccountSruCode> SysAccountSruCode { get; set; }
    }
}
