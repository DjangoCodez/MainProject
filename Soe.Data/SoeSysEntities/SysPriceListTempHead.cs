namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPriceListTempHead")]
    public partial class SysPriceListTempHead : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysPriceListTempHead()
        {
            SysPriceListTempItem = new HashSet<SysPriceListTempItem>();
        }

        public int SysPriceListTempHeadId { get; set; }

        public DateTime Date { get; set; }

        public int Version { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysPriceListTempItem> SysPriceListTempItem { get; set; }
    }
}
