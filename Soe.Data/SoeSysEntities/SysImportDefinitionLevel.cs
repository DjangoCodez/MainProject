namespace SoftOne.Soe.Data
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SysImportDefinitionLevel")]
    public partial class SysImportDefinitionLevel : SysEntity
    {
        public int SysImportDefinitionLevelId { get; set; }

        public int SysImportDefinitionId { get; set; }

        public int Level { get; set; }

        public string Xml { get; set; }

        public virtual SysImportDefinition SysImportDefinition { get; set; }
    }
}
