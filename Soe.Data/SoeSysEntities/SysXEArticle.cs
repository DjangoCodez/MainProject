namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysXEArticle")]
    public partial class SysXEArticle : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysXEArticle()
        {
            SysXEArticleFeature = new HashSet<SysXEArticleFeature>();
        }

        public int SysXEArticleId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(20)]
        public string ArticleNr { get; set; }

        public string Description { get; set; }

        public bool Inactive { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        [StringLength(50)]
        public string ModuleGroup { get; set; }

        [StringLength(50)]
        public string ArticleNrYear1 { get; set; }

        [StringLength(50)]
        public string ArticleNrYear2 { get; set; }

        public decimal? StartPrice { get; set; }

        public decimal? MonthlyPrice { get; set; }

        public int SortOrder { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysXEArticleFeature> SysXEArticleFeature { get; set; }
    }
}
