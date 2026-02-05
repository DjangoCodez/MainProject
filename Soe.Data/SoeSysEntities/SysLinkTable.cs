namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysLinkTable")]
    public partial class SysLinkTable : SysEntity
    {
        public int SysLinkTableId { get; set; }

        public int SysLinkTableRecordType { get; set; }

        public int SysLinkTableKeyItemId { get; set; }

        public int SysLinkTableIntegerValueType { get; set; }

        public int SysLinkTableIntegerValue { get; set; }
    }
}
