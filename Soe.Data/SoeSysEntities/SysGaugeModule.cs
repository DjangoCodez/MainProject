namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysGaugeModule")]
    public partial class SysGaugeModule : SysEntity
    {
        public int SysGaugeModuleId { get; set; }

        public int SysGaugeId { get; set; }

        public int Module { get; set; }

        public virtual SysGauge SysGauge { get; set; }
    }
}
