using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    public class ImportCopyItem
    {

        // Keys
        public int ImportId { get; set; }
        public int ActorCompanyId { get; set; }
        public int ImportDefinitionId { get; set; }

        public int Module { get; set; }
        public string Name { get; set; }
        public SoeEntityState State { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }
        public TermGroup_SysImportDefinitionType Type { get; set; }
        public Guid Guid { get; set; }
        public string SpecialFunctionality { get; set; }

        //Flags
        public bool IsStandard { get; set; }
 
    }
}
