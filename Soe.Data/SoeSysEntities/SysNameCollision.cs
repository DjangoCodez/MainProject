namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysNameCollision")]
    public partial class SysNameCollision : SysEntity
    {
        public int SysNameCollisionId { get; set; }

        public int SysProductId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(256)]
        public string OldName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(256)]
        public string NewName { get; set; }

        public DateTime Created { get; set; }
    }
}
