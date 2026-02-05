namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPaymentService")]
    public partial class SysPaymentService : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPaymentServiceId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }
    }
}
