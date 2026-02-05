namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysFeature")]
    public partial class SysFeature : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysFeature()
        {
            SysHelp = new HashSet<SysHelp>();
            SysInformationFeature = new HashSet<SysInformationFeature>();
            SysPageStatus = new HashSet<SysPageStatus>();
            SysXEArticleFeature = new HashSet<SysXEArticleFeature>();
            SysFeature1 = new HashSet<SysFeature>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysFeatureId { get; set; }

        public int? ParentFeatureId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int Order { get; set; }

        public bool Inactive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysHelp> SysHelp { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysInformationFeature> SysInformationFeature { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysPageStatus> SysPageStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysXEArticleFeature> SysXEArticleFeature { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysFeature> SysFeature1 { get; set; }

        public virtual SysFeature SysFeature2 { get; set; }
    }
}
