namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysPaymentType")]
    public partial class SysPaymentType : SysEntity
    {
        public int SysPaymentTypeId { get; set; }

        public int SysTermId { get; set; }

        public int SysTermGroupId { get; set; }
    }
}
