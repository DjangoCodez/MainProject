namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysHolidayType")]
    public partial class SysHolidayType : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysHolidayType()
        {
            SysHoliday = new HashSet<SysHoliday>();
        }

        public int SysHolidayTypeId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public int SysCountryId { get; set; }

        public virtual SysCountry SysCountry { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysHoliday> SysHoliday { get; set; }
    }
}
