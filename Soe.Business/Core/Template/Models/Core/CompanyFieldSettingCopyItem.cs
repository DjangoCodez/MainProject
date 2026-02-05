using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    public class CompanyFieldSettingCopyItem
    {
        public int FormId { get; set; }
        public int FieldId { get; set; }
        public int SysSettingId { get; set; }
        public string Value { get; set; }
        public int ActorCompanyId { get; set; }
    }
}
