using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SysImportDefinitionLevelColumnSettings
    {
        public int SysImportDefinitionLevelColumnSettingsId { get; set; }
        public int Level { get; set; }
        public bool IsModified { get; set; }

        public string Text { get; set; } //CustomerNr

        public string Column { get; set; } //Customer.CustomerNr

        public int UpdateTypeId { get; set; }
        public string UpdateTypeText { get; set; }

        // Type XML
        public string XmlTag { get; set; }

        //Type Separator
        public int Position { get; set; }

        //Type Fixed
        public int From { get; set; }
        public int Characters { get; set; }

        public string Convert { get; set; }
        public string Standard { get; set; }
    }
}
