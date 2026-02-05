using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class EmployeeTemplateCopyItem
    {
        public string Code { get; set; }
        public string ExternalCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public EmployeeCollectiveAgreementCopyItem EmployeeCollectiveAgreement { get; set; }
        public List<EmployeeTemplateGroupCopyItem> EmployeeTemplateGroups { get; set; } = new List<EmployeeTemplateGroupCopyItem>();
    }



    public class EmployeeTemplateGroupCopyItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public List<EmployeeTemplateGroupRowCopyItem> EmployeeTemplateGroupRows { get; set; } = new List<EmployeeTemplateGroupRowCopyItem>();
    }

    public class EmployeeTemplateGroupRowCopyItem
    {
        public int Type { get; set; }
        public int MandatoryLevel { get; set; }
        public int RegistrationLevel { get; set; }
        public string Title { get; set; }
        public string DefaultValue { get; set; }
        public string Comment { get; set; }
        public int Row { get; set; }
        public int StartColumn { get; set; }
        public int SpanColumns { get; set; }
        public string Format { get; set; }
        public bool HideInReport { get; set; }
        public bool HideInReportIfEmpty { get; set; }
        public bool HideInRegistration { get; set; }
        public bool HideInEmploymentRegistration { get; set; }
        public int? Entity { get; set; }
    }

}
