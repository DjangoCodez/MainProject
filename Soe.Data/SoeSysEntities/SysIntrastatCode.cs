namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysIntrastatCode")]
    public partial class SysIntrastatCode : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysIntrastatCode()
        {
            SysIntrastatText = new HashSet<SysIntrastatText>();
        }

        public int SysIntrastatCodeId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(30)]
        public string Code { get; set; }

        public bool UseOtherQualifier { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int State { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifedBy { get; set; }

        public virtual ICollection<SysIntrastatText> SysIntrastatText { get; set; }
    }
}
