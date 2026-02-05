namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysMicroService")]
    public partial class SysMicroService : SysEntity
    {
        public int SysMicroServiceId { get; set; }

        public int Type { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(2048)]
        public string Url { get; set; }

        public int Status { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public DateTime? LastActive { get; set; }

        public int State { get; set; }
    }
}
