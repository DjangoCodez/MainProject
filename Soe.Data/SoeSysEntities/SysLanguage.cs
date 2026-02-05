namespace SoftOne.Soe.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysLanguage")]
    public partial class SysLanguage : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysLanguage()
        {
            SysHelp = new HashSet<SysHelp>();
            SysInformation = new HashSet<SysInformation>();
            SysNews = new HashSet<SysNews>();
            SysPosition = new HashSet<SysPosition>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysLanguageId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(32)]
        public string LangCode { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(48)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(10)]
        public string ShortName { get; set; }

        public bool Translated { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysHelp> SysHelp { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysInformation> SysInformation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysNews> SysNews { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysPosition> SysPosition { get; set; }
    }
}
