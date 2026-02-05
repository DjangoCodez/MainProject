namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysExportDefinition")]
    public partial class SysExportDefinition : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysExportDefinition()
        {
            SysExportDefinitionLevel = new HashSet<SysExportDefinitionLevel>();
        }

        public int SysExportDefinitionId { get; set; }

        public int SysExportHeadId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        public int Type { get; set; }

        [StringLength(1)]
        public string Separator { get; set; }

        [StringLength(100)]
        public string XmlTagHead { get; set; }

        [StringLength(50)]
        public string SpecialFunctionality { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public virtual SysExportHead SysExportHead { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysExportDefinitionLevel> SysExportDefinitionLevel { get; set; }
    }
}
