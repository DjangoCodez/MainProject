namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysImportHead")]
    public partial class SysImportHead : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysImportHead()
        {
            SysImportDefinition = new HashSet<SysImportDefinition>();
            SysImportRelation = new HashSet<SysImportRelation>();
            SysImportSelect = new HashSet<SysImportSelect>();
        }

        public int SysImportHeadId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int Sortorder { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public int Module { get; set; }

        public int? SysImportHeadTypeId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysImportDefinition> SysImportDefinition { get; set; }

        public virtual SysImportHeadType SysImportHeadType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysImportRelation> SysImportRelation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysImportSelect> SysImportSelect { get; set; }
    }
}
