namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysWholesellerEdi")]
    public partial class SysWholesellerEdi : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysWholesellerEdi()
        {
            SysEdiMsg = new HashSet<SysEdiMsg>();
            SysWholeseller = new HashSet<SysWholeseller>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysWholesellerEdiId { get; set; }

        [StringLength(50)]
        public string SenderId { get; set; }

        [StringLength(50)]
        public string SenderName { get; set; }

        [StringLength(500)]
        public string EdiFolder { get; set; }

        [StringLength(50)]
        public string FtpUser { get; set; }

        [StringLength(50)]
        public string FtpPassword { get; set; }
                
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysEdiMsg> SysEdiMsg { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysWholeseller> SysWholeseller { get; set; }
    }
}
