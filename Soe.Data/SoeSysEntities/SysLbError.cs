namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysLbError")]
    public partial class SysLbError : SysEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysErrorId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(8)]
        public string LbErrorCode { get; set; }

        public int? SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }
    }
}
