namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysWholeseller")]
    public partial class SysWholeseller : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysWholeseller()
        {
            SysEdiMessageHead = new HashSet<SysEdiMessageHead>();
            SysWholesellerSetting = new HashSet<SysWholesellerSetting>();
            SysPriceListHead = new HashSet<SysPriceListHead>();
        }

        public int SysWholesellerId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        public int Type { get; set; }

        public int SysCountryId { get; set; }

        public int SysCurrencyId { get; set; }

        public bool IsOnlyInComp { get; set; }

        [StringLength(250)]
        public string AvailabilityRequestURL { get; set; }

        public int? SysWholesellerEdiId { get; set; }

        [StringLength(50)]
        public string OrgNr { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysEdiMessageHead> SysEdiMessageHead { get; set; }

        public virtual SysWholesellerEdi SysWholesellerEdi { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysWholesellerSetting> SysWholesellerSetting { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysPriceListHead> SysPriceListHead { get; set; }
    }
}
