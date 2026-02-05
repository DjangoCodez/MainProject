namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SystemAdminMessage")]
    public partial class SystemAdminMessage : SysEntity
    {
        public int SystemAdminMessageId { get; set; }

        public int LangId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        public string Header { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(512)]
        public string Message { get; set; }

        public DateTime? Expires { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }
    }
}
