
namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysReportGroupMapping")]
    public partial class SysReportGroupMapping : SysEntity
    {
        [Key]
        [Column(Order = 1)]
        public int SysReportGroupId { get; set; }
        [Key]
        [Column(Order = 2)]
        public int SysReportTemplateId { get; set; }
        public int Order { get; set; }

        public virtual SysReportGroup SysReportGroup { get; set; }
        public virtual SysReportTemplate SysReportTemplate { get; set; }
    }
}
