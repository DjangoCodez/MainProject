namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPosition")]
    public partial class SysPosition : SysEntity
    {
        public int SysPositionId { get; set; }

        public int SysCountryId { get; set; }

        public int SysLanguageId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string Code { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public virtual SysCountry SysCountry { get; set; }

        public virtual SysLanguage SysLanguage { get; set; }
    }
}
