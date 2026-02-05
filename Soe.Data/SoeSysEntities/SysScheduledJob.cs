namespace SoftOne.Soe.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysScheduledJob")]
    public partial class SysScheduledJob : SysEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysScheduledJob()
        {
            SysJobSettingScheduledJob = new HashSet<SysJobSettingScheduledJob>();
            SysScheduledJobLog = new HashSet<SysScheduledJobLog>();
        }

        public int SysScheduledJobId { get; set; }

        public int SysJobId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string DatabaseName { get; set; }

        public DateTime ExecuteTime { get; set; }

        public int ExecuteUserId { get; set; }

        public bool AllowParallelExecution { get; set; }

        public int RecurrenceType { get; set; }

        public int RecurrenceCount { get; set; }

        public DateTime? RecurrenceDate { get; set; }

        [StringLength(512)]
        public string RecurrenceInterval { get; set; }

        public int RetryTypeForInternalError { get; set; }

        public int RetryTypeForExternalError { get; set; }

        public int RetryCount { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }

        public int Type { get; set; }

        public DateTime? LastStartTime { get; set; }

        public virtual SysJob SysJob { get; set; }

        public virtual ICollection<SysJobSettingScheduledJob> SysJobSettingScheduledJob { get; set; }
        public virtual ICollection<SysScheduledJobLog> SysScheduledJobLog { get; set; }
    }
}
