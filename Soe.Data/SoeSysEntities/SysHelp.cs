namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysHelp")]
    public partial class SysHelp : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysHelp()
        {
            SysHelp1 = new HashSet<SysHelp>();
            SysHelp2 = new HashSet<SysHelp>();
        }

        public int SysHelpId { get; set; }

        public int SysLanguageId { get; set; }

        public int SysFeatureId { get; set; }

        public int? VersionNr { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        public string Text { get; set; }

        public string PlainText { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public virtual SysFeature SysFeature { get; set; }

        public virtual SysLanguage SysLanguage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysHelp> SysHelp1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysHelp> SysHelp2 { get; set; }
    }
}
