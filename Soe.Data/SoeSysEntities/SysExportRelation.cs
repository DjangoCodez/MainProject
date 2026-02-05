namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysExportRelation")]
    public partial class SysExportRelation : SysEntity
    {
        public int SysExportRelationId { get; set; }

        public int SysExportHeadId { get; set; }

        public int LevelParent { get; set; }

        public int LevelChild { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string FieldParent { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string FieldChild { get; set; }

        public virtual SysExportHead SysExportHead { get; set; }
    }
}
