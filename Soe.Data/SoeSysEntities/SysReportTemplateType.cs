namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysReportTemplateType")]
    public partial class SysReportTemplateType : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysReportTemplateTypeId { get; set; }

        public int SysReportTermId { get; set; }

        public int SelectionType { get; set; }

        public bool GroupMapping { get; set; }

        public int Module { get; set; }

        public int Group { get; set; }
    }
}
