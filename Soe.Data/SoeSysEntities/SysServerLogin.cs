namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysServerLogin")]
    public partial class SysServerLogin : SysEntity
    {
        public int SysServerLoginId { get; set; }

        public int SysServerId { get; set; }

        public Guid Guid { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(32)]
        public byte[] passwordhash { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public virtual SysServer SysServer { get; set; }
    }
}
