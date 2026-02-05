namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysReportTemplate")]
    public partial class SysReportTemplate : SysEntity
    {
        public int SysReportTemplateId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string FileName { get; set; }

        [Column(TypeName = "image")]
        [Required(AllowEmptyStrings = true)]
        public byte[] Template { get; set; }

        public int SysReportTypeId { get; set; }

        [Column("SysTemplateTypeId")]
        public int SysReportTemplateTypeId { get; set; }

        public SysReportTemplateType SysReportTemplateType { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int? SysCountryId { get; set; }

        public int GroupByLevel1 { get; set; }

        public int GroupByLevel2 { get; set; }

        public int GroupByLevel3 { get; set; }

        public int GroupByLevel4 { get; set; }

        public int SortByLevel1 { get; set; }

        public int SortByLevel2 { get; set; }

        public int SortByLevel3 { get; set; }

        public int SortByLevel4 { get; set; }

        public bool IsSortAscending { get; set; }

        [StringLength(1024)]
        public string Special { get; set; }

        public int? ReportNr { get; set; }

        public bool ShowOnlyTotals { get; set; }

        public bool ShowGroupingAndSorting { get; set; }

        public bool IsSystemReport { get; set; }

        [StringLength(100)]
        public string ValidExportTypes { get; set; }

        public virtual SysReportType SysReportType { get; set; }
        public virtual ICollection<SysReportGroup> SysReportGroups { get; set; }
        public virtual ICollection<SysReportTemplateSetting> SysReportTemplateSettings { get; set; }
    }
}
