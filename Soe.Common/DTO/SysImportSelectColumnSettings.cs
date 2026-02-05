using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SysImportSelectColumnSettings
    {
        public int SysImportSelectColumnSettingsId { get; set; }
        public string Column { get; set; }  //CustomerNr        
        public string Text { get; set; } //ex: Kundnr
        public string DataType { get; set; } //ex: Sträng

        public bool Mandatory { get; set; }

        public int Position { get; set; }

    }
}
