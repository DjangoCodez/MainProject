namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysLog")]
    public partial class SysLog : SysEntity
    {
        public int SysLogId { get; set; }

        [StringLength(10)]
        public string LicenseId { get; set; }

        [StringLength(10)]
        public string ActorCompanyId { get; set; }

        [StringLength(10)]
        public string RoleId { get; set; }

        [StringLength(10)]
        public string UserId { get; set; }

        public DateTime Date { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(32)]
        public string Thread { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(255)]
        public string Level { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(512)]
        public string Logger { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(512)]
        public string LogClass { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Message { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Exception { get; set; }

        [StringLength(10)]
        public string LineNumber { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(512)]
        public string Source { get; set; }

        public string TargetSite { get; set; }

        [StringLength(255)]
        public string RequestUri { get; set; }

        [StringLength(255)]
        public string ReferUri { get; set; }

        public string Form { get; set; }

        public string Application { get; set; }

        public string Session { get; set; }

        [StringLength(256)]
        public string HostName { get; set; }

        [StringLength(128)]
        public string IpNr { get; set; }

        public int? RecordId { get; set; }

        public int RecordType { get; set; }

        [StringLength(64)]
        public string LicenseNr { get; set; }

        [StringLength(128)]
        public string CompanyName { get; set; }

        [StringLength(128)]
        public string LoginName { get; set; }

        [StringLength(64)]
        public string RoleTermId { get; set; }

        [StringLength(128)]
        public string RoleName { get; set; }

        [StringLength(128)]
        public string UserName { get; set; }

        public long? TaskWatchLogId { get; set; }
    }
}
