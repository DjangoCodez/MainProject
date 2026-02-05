using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class AccountDimCopyItem
    {
        public int? ParentId { get; set; }
        public int AccountDimId { get; set; }
        public int AccountDimNr { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? SysSieDimNr { get; set; }
        public bool LinkedToProject { get; set; }
        public bool UseInSchedulePlanning { get; set; }
        public bool ExcludeinAccountingExport { get; set; }
        public bool ExcludeinSalaryExport { get; set; }
        public bool UseVatDeduction { get; set; }
        public bool MandatoryInCustomerInvoice { get; set; }
        public bool MandatoryInOrder { get; set; }
        public bool OnlyAllowAccountsWithParent { get; set; }

    }
}
