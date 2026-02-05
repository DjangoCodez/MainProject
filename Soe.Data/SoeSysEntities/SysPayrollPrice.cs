namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPayrollPrice")]
    public partial class SysPayrollPrice : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysPayrollPrice()
        {
            SysPayrollPriceInterval = new HashSet<SysPayrollPriceInterval>();
        }

        public int SysPayrollPriceId { get; set; }

        public int SysCountryId { get; set; }

        public int SysTermId { get; set; }

        public int Type { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(10)]
        public string Code { get; set; }

        public decimal Amount { get; set; }

        public int AmountType { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FromDate { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public virtual SysCountry SysCountry { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysPayrollPriceInterval> SysPayrollPriceInterval { get; set; }
    }
}
