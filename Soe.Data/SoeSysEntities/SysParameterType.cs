namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysParameterType")]
    public partial class SysParameterType : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysParameterTypeId { get; set; }

        [StringLength(50)]
        public string SysParameterDescription { get; set; }

        public virtual SysParameterType SysParameterType1 { get; set; }

        public virtual SysParameterType SysParameterType2 { get; set; }
    }
}
