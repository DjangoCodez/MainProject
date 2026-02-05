using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        public static InventoryLog Copy(this InventoryLog original)
        {
            return new InventoryLog
            {
                Amount = original.Amount,
                AmountCurrency = original.AmountCurrency,
                AmountEntCurrency = original.AmountEntCurrency,
                AmountLedgerCurrency = original.AmountLedgerCurrency,
                Date = original.Date,
                Type = original.Type,

                //Set references
                Inventory = original.Inventory,
                AccountDistributionEntry = original.AccountDistributionEntry,
                ActorCompanyId = original.ActorCompanyId,
                UserId = original.UserId,
            };
        }
        public static void Reverse(this InventoryLog log)
        {
            log.Amount = -log.Amount;
            log.AmountCurrency = -log.AmountCurrency;
            log.AmountEntCurrency = -log.AmountEntCurrency;
            log.AmountLedgerCurrency = -log.AmountLedgerCurrency;
        }
    }
}
