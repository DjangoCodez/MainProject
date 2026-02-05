namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysJobSetting")]
    public partial class SysJobSetting : SysEntity
    {
        public int SysJobSettingId { get; set; }

        public int Type { get; set; }

        public int DataType { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string Name { get; set; }

        public string StrData { get; set; }

        public int? IntData { get; set; }

        public decimal? DecimalData { get; set; }

        public bool? BoolData { get; set; }

        public DateTime? DateData { get; set; }

        public DateTime? TimeData { get; set; }

        public virtual List<SysJobSettingJob> SysJobSettingJob { get; set; }
        public virtual List<SysJobSettingScheduledJob> SysJobSettingScheduledJob { get; set; }
    }
}
