namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysProduct")]
    public partial class SysProduct : SysEntity
    {
        public int SysProductId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(256)]
        public string Name { get; set; }

        [StringLength(50)]
        public string EAN { get; set; }

        [StringLength(50)]
        public string ProductId { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public DateTime? Modified { get; set; }

        public int Type { get; set; }

        public int SysCountryId { get; set; }

        public DateTime? EndAt { get; set; }

        [StringLength(2048)]
        public string ExtendedInfo { get; set; }

        [StringLength(100)]
        public string Manufacturer { get; set; }

        public int? SysProductGroupId { get; set; }

        [StringLength(255)]
        public string ImageFileName { get; set; }
        public int? ExternalId { get; set; }

        [ForeignKey(nameof(SysProductGroupId))]
        public virtual SysProductGroup SysProductGroup { get; set; }
    }
}
