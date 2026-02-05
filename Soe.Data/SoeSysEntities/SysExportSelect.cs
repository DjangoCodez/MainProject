namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysExportSelect")]
    public partial class SysExportSelect : SysEntity
    {
        public int SysExportSelectId { get; set; }

        public int SysExportHeadId { get; set; }

        public int Level { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(5000)]
        public string Select { get; set; }

        [StringLength(500)]
        public string Where { get; set; }

        [StringLength(500)]
        public string GroupBy { get; set; }

        [StringLength(500)]
        public string OrderBy { get; set; }

        public string Settings { get; set; }

        public virtual SysExportHead SysExportHead { get; set; }
    }
}
