namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysVatAccount")]
    public partial class SysVatAccount : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysVatAccount()
        {
            SysAccountStd = new HashSet<SysAccountStd>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysVatAccountId { get; set; }

        [StringLength(10)]
        public string AccountCode { get; set; }

        public int? VatCode { get; set; }

        public int? LangId { get; set; }

        public int? VatNr1 { get; set; }

        public int? VatNr2 { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysAccountStd> SysAccountStd { get; set; }

        public virtual SysVatRate SysVatRate { get; set; }
    }
}
