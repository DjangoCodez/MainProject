namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPriceListHead")]
    public partial class SysPriceListHead : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysPriceListHead()
        {
            SysWholeseller = new HashSet<SysWholeseller>();
        }

        public int SysPriceListHeadId { get; set; }

        public DateTime Date { get; set; }

        public int? Version { get; set; }

        public int Provider { get; set; }

        public int SysWholesellerId { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysWholeseller> SysWholeseller { get; set; }
        public virtual ICollection<SysPriceList> SysPriceLists { get; set; }
    }
}
