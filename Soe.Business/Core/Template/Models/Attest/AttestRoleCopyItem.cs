using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Attest
{
    public class AttestRoleCopyItem
    {
        public int AttestRoleId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public int AttestRoleDefinitionId { get; set; }
        public int Type { get; set; }
        public SoeModule Module { get; set; }
        public bool IsStandard { get; set; }
        public SoeEntityState State { get; set; }
        public int SpecialFunctionality { get; set; }
        public Guid Guid { get; set; }
        public int AttestRoleHeadType { get; set; }
        public string Description { get; set; }
        public decimal DefaultMaxAmount { get; set; }
        public bool ShowUncategorized { get; set; }
        public bool ShowAllCategories { get; set; }
        public bool ShowAllSecondaryCategories { get; set; }
        public bool ShowTemplateSchedule { get; set; }
        public int? ReminderNoOfDays { get; set; }
        public int? ReminderPeriodType { get; set; }
        public bool AlsoAttestAdditionsFromTime { get; set; }
        public bool HumanResourcesPrivacy { get; set; }
        public int Sort { get; set; }
        public int? ReminderAttestStateId { get; set; }
        public List<AttestTransitionCopyItem> AttestTransitions { get; set; }
        public List<AttestStateCopyItem> VisiableAttestStates { get; set; }
    }

    public class AttestStateCopyItem
    {
        public int AttestStateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Initial { get; set; }
        public bool Closed { get; set; }
        public bool Hidden { get; set; }
        public int Sort { get; set; }
        public string Color { get; set; }
        public TermGroup_AttestEntity Entity { get; set; }
        public SoeModule Module { get; set; }
    }

    public class AttestTransitionCopyItem
    {
        public int AttestTransitionId { get; set; }
        public int? AttestStateFromId { get; set; }
        public int? AttestStateToId { get; set; }
        public string Name { get; set; }
        public SoeModule Module { get; set; }
        public bool NotifyChangeOfAttestState { get; set; }
        public TermGroup_AttestEntity Entity { get; set; }
        public List<int> AttestRoleIds { get; set; } = new List<int>();

    }

    public class AttestWorkFlowTemplateHeadCopyItem
    {
        public int AttestWorkFlowTemplateHeadId { get; set; }
        public TermGroup_AttestWorkFlowType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ActorCompanyId { get; set; }
        public TermGroup_AttestEntity AttestEntity { get; set; }
        public List<AttestWorkFlowTemplateRowCopyItem> AttestWorkFlowTemplateRowCopyItems { get; set; } = new List<AttestWorkFlowTemplateRowCopyItem>();

    }

    public class AttestWorkFlowTemplateRowCopyItem
    {
        public int AttestTransitionId { get; set; }
        public int? Type { get; set; }
        public int Sort { get; set; }
    }

    public class CategoryCopyItem
    {
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public SoeCategoryType Type { get; set; }

        public List<CompanyCategoryRecordCopyItem> CompanyCategoryRecordCopyItems { get; set; } = new List<CompanyCategoryRecordCopyItem>();
    }

    public class CompanyCategoryRecordCopyItem
    {
        public int RecordId { get; set; }
        public SoeCategoryRecordEntity Entity { get; set; }
        public bool Default { get; set; }
        public int CategoryId { get; set; }
    }
}
