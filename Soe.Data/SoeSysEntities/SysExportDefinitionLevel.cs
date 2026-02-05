namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysExportDefinitionLevel")]
    public partial class SysExportDefinitionLevel : SysEntity
    {
        public int SysExportDefinitionLevelId { get; set; }

        public int SysExportDefinitionId { get; set; }

        public int Level { get; set; }

        public string Xml { get; set; }

        public virtual SysExportDefinition SysExportDefinition { get; set; }
    }
}
