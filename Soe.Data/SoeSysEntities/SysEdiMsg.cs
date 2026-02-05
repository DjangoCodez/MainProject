namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysEdiMsg")]
    public partial class SysEdiMsg : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysEdiMsgId { get; set; }

        public int SysWholesellerEdiId { get; set; }

        public int SysEdiTypeId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string SenderSenderNr { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string SenderType { get; set; }

        public virtual SysEdiType SysEdiType { get; set; }

        public virtual SysWholesellerEdi SysWholesellerEdi { get; set; }
    }
}
