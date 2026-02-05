using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class AccountStdCopyItem
    {
        public AccountStdCopyItem()
        {
            AccountSRUCopyItems = new List<AccountSRUCopyItem>();
        }
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public int AccountTypeSysTermId { get; set; }
        public int? SysVatAccountId { get; set; }
        public string Unit { get; set; }
        public bool UnitStop { get; set; }
        public int AmountStop { get; set; }
        public List<AccountSRUCopyItem> AccountSRUCopyItems { get; set; }
        public string ExternalCode { get; set; }
    }

    public class AccountSRUCopyItem
    {
        public int SysAccountSruCodeId { get; set; }

        // Add other properties as needed
    }
}
