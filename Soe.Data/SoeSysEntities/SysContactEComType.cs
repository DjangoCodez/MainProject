namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysContactEComType")]
    public partial class SysContactEComType : SysEntity
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysContactEComTypeId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysContactTypeId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public virtual SysContactType SysContactType { get; set; }
    }
}
