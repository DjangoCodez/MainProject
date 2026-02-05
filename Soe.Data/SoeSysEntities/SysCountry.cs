namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCountry")]
    public partial class SysCountry : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysCountry()
        {
            SysBank = new HashSet<SysBank>();
            SysPayrollPrice = new HashSet<SysPayrollPrice>();
            SysPosition = new HashSet<SysPosition>();
            SysHolidayType = new HashSet<SysHolidayType>();
        }

        public int SysCountryId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(10)]
        public string Code { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public int? SysCurrencyId { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        [StringLength(10)]
        public string AreaCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime? IsEUFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime? IsEUTo { get; set; }

        [StringLength(5)]
        public string CultureCode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysBank> SysBank { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysPayrollPrice> SysPayrollPrice { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysPosition> SysPosition { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysExtraField> SysExtraField { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysHolidayType> SysHolidayType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysHouseholdType> SysHouseholdType { get; set; }

        public virtual SysCurrency SysCurrency { get; set; }
    }
}
