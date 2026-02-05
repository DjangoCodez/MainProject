namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysReportTemplateSetting")]
    public partial class SysReportTemplateSetting : SysEntity
    {
        public int SysReportTemplateSettingId { get; set; }

        public int SysReportTemplateId { get; set; }

        public int SettingField { get; set; }

        public int SettingType { get; set; }

        [StringLength(100)]
        public string SettingValue { get; set; }

        public int State { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public virtual SysReportTemplate SysReportTemplate { get; set; }
    }
}
