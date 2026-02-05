namespace SoftOne.Soe.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysVehicleType")]
    public partial class SysVehicleType : SysEntity
    {
        public int SysVehicleTypeId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(100)]
        public string Filename { get; set; }

        public int ManufacturingYear { get; set; }

        [Column(TypeName = "xml")]
        [Required(AllowEmptyStrings = true)]
        public string XML { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? Created { get; set; }

        [StringLength(50)]
        public string CreatedBy { get; set; }

        public DateTime? Modified { get; set; }

        [StringLength(50)]
        public string ModifiedBy { get; set; }

        public int State { get; set; }
    }
}
