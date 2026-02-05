using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftOne.Soe.Data
{
    [Table("SysReportGroupHeaderMapping")]
    public partial class SysReportGroupHeaderMapping : SysEntity
    {
        [Key]
        [Column(Order = 1)]
        public int SysReportGroupId { get; set; }
        [Key]
        [Column(Order = 2)]
        public int SysReportHeaderId { get; set; }
        public int Order { get; set; }

        public virtual SysReportGroup SysReportGroup { get; set; }
        public virtual SysReportHeader SysReportHeader { get; set; }
    }
}
