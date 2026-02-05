namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysInformation")]
    public partial class SysInformation : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysInformation()
        {
            SysInformationFeature = new HashSet<SysInformationFeature>();
            SysInformationSysCompDb = new HashSet<SysInformationSysCompDb>();
        }

        public int SysInformationId { get; set; }

        public int SysLanguageId { get; set; }

        public int Type { get; set; }

        public int Severity { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Subject { get; set; }

        [StringLength(255)]
        public string ShortText { get; set; }

        public string Text { get; set; }

        public string PlainText { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(16)]
        public byte[] TextHash { get; set; }

        [StringLength(255)]
        public string Folder { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public int StickyType { get; set; }

        public bool NeedsConfirmation { get; set; }

        public bool ShowInWeb { get; set; }

        public bool ShowInMobile { get; set; }

        public bool ShowInTerminal { get; set; }

        public bool Notify { get; set; }

        public bool ShowOnAllFeatures { get; set; }

        public bool ShowOnAllSysCompDbs { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public DateTime? NotificationSent { get; set; }

        public virtual SysLanguage SysLanguage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysInformationFeature> SysInformationFeature { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysInformationSysCompDb> SysInformationSysCompDb { get; set; }
    }
}
