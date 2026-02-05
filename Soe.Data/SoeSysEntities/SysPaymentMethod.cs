namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPaymentMethod")]
    public partial class SysPaymentMethod : SysEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SysPaymentMethodId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }
    }
}
