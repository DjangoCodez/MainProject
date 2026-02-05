namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysCompanySetting")]
    public partial class SysCompanySetting : SysEntity
    {
        public int SysCompanySettingId { get; set; }

        public int SysCompanyId { get; set; }

        public int SettingType { get; set; }

        public string StringValue { get; set; }

        public int? IntValue { get; set; }

        public bool? Boolvalue { get; set; }

        public decimal? DecimalValue { get; set; }

        public virtual SysCompany SysCompany { get; set; }
    }
}
