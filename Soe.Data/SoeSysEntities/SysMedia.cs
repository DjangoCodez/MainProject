namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysMedia")]
    public partial class SysMedia : SysEntity
    {
        public int SysMediaId { get; set; }

        public int SysLanguageId { get; set; }

        public int Type { get; set; }

        public int MediaType { get; set; }

        public int FormatType { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string Filename { get; set; }

        [StringLength(255)]
        public string Path { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }
    }
}
