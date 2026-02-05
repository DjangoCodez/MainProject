namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysParameter")]
    public partial class SysParameter : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysParameterId { get; set; }

        public int SysParameterTypeId { get; set; }

        [StringLength(20)]
        public string Code { get; set; }

        [StringLength(100)]
        public string Name { get; set; }
    }
}
