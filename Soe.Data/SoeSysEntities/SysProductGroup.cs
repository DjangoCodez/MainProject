namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics;

    [Table("SysProductGroup")]
    public partial class SysProductGroup : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysProductGroup()
        {
            SysProductGroupChildren = new HashSet<SysProductGroup>();
        }

        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SysProductGroupId { get; set; }

        public int? ParentSysProductGroupId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string Identifier { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(250)]
        public string Name { get; set; }

        public int SysCountryId { get; set; }

        public int Type { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifedBy { get; set; }

        public int State { get; set; }

        public int? ExternalId { get; set; }

        [ForeignKey(nameof(ParentSysProductGroupId))]
        public virtual SysProductGroup ParentSysProductGroup { get; set; }

        public virtual ICollection<SysProductGroup> SysProductGroupChildren { get; set;}
    }
}
