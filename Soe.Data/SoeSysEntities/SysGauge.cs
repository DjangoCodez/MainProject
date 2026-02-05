namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysGauge")]
    public partial class SysGauge : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysGauge()
        {
            SysGaugeModule = new HashSet<SysGaugeModule>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysGaugeId { get; set; }

        public int SysFeatureId { get; set; }

        public int SysTermId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string GaugeName { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysGaugeModule> SysGaugeModule { get; set; }
    }
}
