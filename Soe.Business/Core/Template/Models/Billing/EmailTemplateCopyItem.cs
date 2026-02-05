using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Billing
{    public class EmailTemplateCopyItem
    {
        public int TemplateId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool BodyIsHTML { get;  set; }
        public EmailTemplateType Type { get; set; }
    }
}
