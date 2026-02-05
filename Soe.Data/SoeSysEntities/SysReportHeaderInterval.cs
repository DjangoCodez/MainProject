namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysReportHeaderInterval")]
    public partial class SysReportHeaderInterval : SysEntity
    {
        public int SysReportHeaderIntervalId { get; set; }
        public int SysReportHeaderId { get; set; }
        [StringLength(10)]
        [Required(AllowEmptyStrings = false)]
        public string IntervalFrom { get; set; }
        [StringLength(10)]
        [Required(AllowEmptyStrings = false)]
        public string IntervalTo { get; set; }
        public int? SelectValue { get; set; }
        public virtual SysReportHeader SysReportHeader { get; set; }
    }
}
