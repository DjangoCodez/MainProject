namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysImportRelation")]
    public partial class SysImportRelation : SysEntity
    {
        public int SysImportRelationId { get; set; }

        public int SysImportHeadId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string TableParent { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string TableChild { get; set; }

        public virtual SysImportHead SysImportHead { get; set; }
    }
}
