namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysReportHeader")]
    public partial class SysReportHeader
    {
        public int SysReportHeaderId { get; set; }
        [StringLength(100)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Description { get; set; }
        public bool ShowRow { get; set; }
        public bool ShowSum { get; set; }
        public bool ShowLabel { get; set; }
        public bool ShowZeroRow { get; set; }
        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public bool DoNotSummarizeOnGroup { get; set; }
        public bool InvertRow { get; set; }

        public int TemplateTypeId { get; set; }

        public virtual ICollection<SysReportGroup> SysReportGroups { get; set; }
        public virtual ICollection<SysReportGroupHeaderMapping> SysReportGroupHeaderMapping { get; set; }
        public virtual ICollection<SysReportHeaderInterval> SysReportHeaderInterval { get; set; }
    }
}
