namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysTerm")]
    public partial class SysTerm : SysEntity
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysTermId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysTermGroupId { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LangId { get; set; }

        [StringLength(255)]
        public string TranslationKey { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Tooltip { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public virtual SysTermGroup SysTermGroup { get; set; }
    }
}
