namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCurrency")]
    public partial class SysCurrency : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysCurrency()
        {
            SysCountry = new HashSet<SysCountry>();
            SysCurrencyRate = new HashSet<SysCurrencyRate>();
            SysCurrencyRate1 = new HashSet<SysCurrencyRate>();
        }

        public int SysCurrencyId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(10)]
        public string Code { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        [Column(TypeName = "date")]
        public DateTime? IsEUFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime? IsEUTo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysCountry> SysCountry { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysCurrencyRate> SysCurrencyRate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysCurrencyRate> SysCurrencyRate1 { get; set; }
    }
}
