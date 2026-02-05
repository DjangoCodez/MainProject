using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class InventoryWriteOffMethodCopyItem
    {
        public int InventoryWriteOffMethodId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public int PeriodType { get; set; }
        public int PeriodValue { get; set; }
        public decimal YearPercent { get; set; }
    }

    public class InventoryWriteOffTemplateCopyItem
    {
        public int InventoryWriteOffTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public InventoryWriteOffMethodCopyItem WriteOffMethod { get; set; }
        public VoucherSeriesTypeCopyItem VoucherSeriesType { get; set; }
        public List<InventoryAccountStdCopyItem> InventoryAccountStds { get; set; } = new List<InventoryAccountStdCopyItem>();

    }

    public class InventoryAccountStdCopyItem
    {
        public int Type { get; set; }
        public int? AccountId { get; set; }
        public List<InventoryInternalAccountCopyItem> InternalAccounts { get; set; } = new List<InventoryInternalAccountCopyItem>();
    }

    public class InventoryInternalAccountCopyItem
    {
        public int AccountId { get; set; }
        public int AccountDimId { get; set; }
    }
}
