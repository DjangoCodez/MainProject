using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models
{
    public class TemplateResult
    {
        public TemplateResult(ActionResult actionResult)
        {
            if (ActionResults == null)
                ActionResults = new List<ActionResult>();
            ActionResults.Add(actionResult);
        }
        public TemplateResult()
        {
            ActionResults = new List<ActionResult>();
        }
        public List<ActionResult> ActionResults { get; set; }
    }
}
