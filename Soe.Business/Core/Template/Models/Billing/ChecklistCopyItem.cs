using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Billing
{
    public class ChecklistCopyItem
    {
        public int ChecklistId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string Description { get; set; }
        public int ReportId { get; set; }
        public List<ChecklistRowCopyItem> ChecklistRowCopyItems { get; set; } = new List<ChecklistRowCopyItem>();
    }

    public class ChecklistRowCopyItem
    {
        public TermGroup_ChecklistRowType Type { get; set; }
        public int RowNr { get; set; }
        public bool Mandatory { get; set; }
        public string Text { get; set; }
        public CheckListMultipleChoiceAnswerHeadCopyItem MultipleChoiceAnswerHead { get; set; }
    }

    public class CheckListMultipleChoiceAnswerHeadCopyItem
    {
        public string Title { get; set; }
        public List<CheckListMultipleChoiceAnswerRowCopyItem> CheckListMultipleChoiceAnswerRows { get; set; } = new List<CheckListMultipleChoiceAnswerRowCopyItem>();
        public int MultipleChoiceAnswerHeadId { get; set; }
    }

    public class CheckListMultipleChoiceAnswerRowCopyItem
    {
        public string Question { get; set; }
    }
}
