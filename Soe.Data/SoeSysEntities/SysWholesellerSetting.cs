namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysWholesellerSetting")]
    public partial class SysWholesellerSetting : SysEntity
    {
        public int SysWholesellerSettingId { get; set; }

        public int SysWholesellerId { get; set; }

        public int SettingType { get; set; }

        public string StringValue { get; set; }

        public int? IntValue { get; set; }

        public bool? Boolvalue { get; set; }

        public decimal? DecimalValue { get; set; }

        public virtual SysWholeseller SysWholeseller { get; set; }
    }
}
