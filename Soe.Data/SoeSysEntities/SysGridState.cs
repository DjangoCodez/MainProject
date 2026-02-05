namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysGridState")]
    public partial class SysGridState : SysEntity
    {
        public int SysGridStateId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(1000)]
        public string Grid { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string GridState { get; set; }
    }
}
