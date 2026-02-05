namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysConnectApiKey")]
    public partial class SysConnectApiKey : SysEntity
    {
        public int SysConnectApiKeyId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(128)]
        public string Name { get; set; }

        public Guid ConnectKey { get; set; }
    }
}
