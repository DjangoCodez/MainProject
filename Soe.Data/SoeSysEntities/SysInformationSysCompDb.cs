namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysInformationSysCompDb")]
    public partial class SysInformationSysCompDb : SysEntity
    {
        public int SysInformationSysCompDbId { get; set; }

        public int SysInformationId { get; set; }

        public int SysCompDbId { get; set; }

        public DateTime? NotificationSent { get; set; }

        public virtual SysCompDb SysCompDb { get; set; }

        public virtual SysInformation SysInformation { get; set; }
    }
}
