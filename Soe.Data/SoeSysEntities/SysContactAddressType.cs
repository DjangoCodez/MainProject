namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysContactAddressType")]
    public partial class SysContactAddressType : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysContactAddressType()
        {
            SysContactAddressRowType = new HashSet<SysContactAddressRowType>();
        }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysContactAddressTypeId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysContactTypeId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysContactAddressRowType> SysContactAddressRowType { get; set; }

        public virtual SysContactType SysContactType { get; set; }
    }
}
