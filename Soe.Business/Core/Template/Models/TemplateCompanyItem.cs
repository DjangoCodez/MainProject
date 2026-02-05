namespace SoftOne.Soe.Business.Core.Template.Models
{
    public class TemplateCompanyItem
    {
        public int ActorCompanyId { get; set; }
        public int SysCompDbId { get; set; }
        public string Name { get; set; }
        public string SysCompDbName { get; set; }
        public bool Global { get;  set; }
        public bool Beta { get; set; } = false;
    }
}
