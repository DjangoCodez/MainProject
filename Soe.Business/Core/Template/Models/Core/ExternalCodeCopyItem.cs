using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    public class ExternalCodeCopyItem
    {
        public int CompanyExternalCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public string ExternalCode { get; set; }
        public TermGroup_CompanyExternalCodeEntity Entity { get; set; }
        public int RecordId { get; set; }
    }
}
