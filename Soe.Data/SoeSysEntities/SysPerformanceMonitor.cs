namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPerformanceMonitor")]
    public partial class SysPerformanceMonitor : SysEntity
    {
        public int SysPerformanceMonitorId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string DatabaseName { get; set; }

        [StringLength(255)]
        public string HostName { get; set; }

        public int Task { get; set; }

        public int ActorCompanyId { get; set; }

        public int? RecordId { get; set; }

        public DateTime Timestamp { get; set; }

        public int Duration { get; set; }

        public int Size { get; set; }

        public int NbrOfRecords { get; set; }

        public int NbrOfSubRecords { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }
    }
}
