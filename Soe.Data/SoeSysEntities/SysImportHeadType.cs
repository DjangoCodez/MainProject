namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysImportHeadType")]
    public partial class SysImportHeadType : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysImportHeadType()
        {
            SysImportHead = new HashSet<SysImportHead>();
        }

        public int SysImportHeadTypeId { get; set; }

        public int TermGroupTermId { get; set; }

        public int SoeImportHeadTypeEnum { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysImportHead> SysImportHead { get; set; }
    }
}
