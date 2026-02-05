using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{

    public class AccountInternalCopyItem
    {
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string Name { get; set; }
        public string ExternalCode { get; set; }
        public string Description { get; set; }

        public int AccountDimNr { get; set; }
        public string AccountDimName { get; set; }
        public string AccountDimShortName { get; set; }
        public int? AccountDimSysSieDimNr { get; set; }
        public int? AccountDimSysAccountStdTypeParentId { get; set; }
        public bool AccountDimUseInSchedulePlanning { get; set; }
        public bool AccountDimExcludeinSalaryExport { get; set; }
        public bool AccountDimUseVatDeduction { get; set; }
        public bool AccountDimLinkedToProject { get; set; }
        public bool AccountDimLinkedToShiftType { get; set; }
        public int AccountDimId { get; set; }
        public int? ParentId { get; set; }
    }
}


