namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysImportSelect")]
    public partial class SysImportSelect : SysEntity
    {
        public int SysImportSelectId { get; set; }

        public int SysImportHeadId { get; set; }

        public int Level { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(5000)]
        public string Select { get; set; }

        [StringLength(500)]
        public string Where { get; set; }

        [StringLength(500)]
        public string GroupBy { get; set; }

        [StringLength(500)]
        public string OrderBy { get; set; }

        public string Settings { get; set; }

        public virtual SysImportHead SysImportHead { get; set; }
    }
}
