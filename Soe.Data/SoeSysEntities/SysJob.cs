namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysJob")]
    public partial class SysJob : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysJob()
        {
            SysScheduledJob = new HashSet<SysScheduledJob>();
            SysJobSettingJob = new HashSet<SysJobSettingJob>();
        }

        public int SysJobId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string AssemblyName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string ClassName { get; set; }

        public bool AllowParallelExecution { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }
        public virtual ICollection<SysJobSettingJob> SysJobSettingJob { get; set; }
        public virtual ICollection<SysScheduledJob> SysScheduledJob { get; set; }
    }
}
