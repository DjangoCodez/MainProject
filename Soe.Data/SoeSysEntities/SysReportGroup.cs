namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysReportGroup")]
    public partial class SysReportGroup : SysEntity
    {
        public int SysReportGroupId { get; set; }
        [StringLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
        public bool ShowLabel { get; set; }
        public bool ShowSum { get; set; }
        public bool InvertRow { get; set; }
        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public int TemplateTypeId { get; set; }

        public virtual ICollection<SysReportTemplate> SysReportTemplates { get; set; }
        public virtual ICollection<SysReportHeader> SysReportHeaders { get; set; }
        public virtual ICollection<SysReportGroupHeaderMapping> SysReportGroupHeaderMapping { get; set; }
        public virtual ICollection<SysReportGroupMapping> SysReportGroupMapping { get; set; }
    }
}
