namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysScheduledJobLog")]
    public partial class SysScheduledJobLog : SysEntity
    {
        public int SysScheduledJobLogId { get; set; }

        public int SysScheduledJobId { get; set; }

        public int BatchNr { get; set; }

        public int LogLevel { get; set; }

        public DateTime Time { get; set; }

        public string Message { get; set; }

        public virtual SysScheduledJob SysScheduledJob { get; set; }
    }
}
