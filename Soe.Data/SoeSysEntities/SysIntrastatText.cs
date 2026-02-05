namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysIntrastatText")]
    public partial class SysIntrastatText : SysEntity
    {
        public SysIntrastatText()
        {
        }

        public int SysIntrastatTextId { get; set; }
        public int SysIntrastatCodeId { get; set; }
        public int SysLanguageId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(1024)]
        public string Text { get; set; }
    }
}
