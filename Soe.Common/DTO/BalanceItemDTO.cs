using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{

    public class AccountDateBalanceDTO
    {
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int RowCount { get; set; }
    }

    public class BalanceItemBase
    {
        public decimal Balance { get; set; }
        public decimal BalanceEntCurrency { get; set; }
        public decimal? Quantity { get; set; }

        public BalanceItemBase()
        {
            this.Balance = Decimal.Zero;
            this.BalanceEntCurrency = Decimal.Zero;
            this.Quantity = null;
        }
    }

    public class BalanceItemDTO : BalanceItemBase
    {
        public int AccountId { get; set; }
        public List<BalanceItemInternalDTO> BalanceItemInternals { get; set; }

        public bool Flag { get; set; }

        public BalanceItemDTO()
        {
            this.AccountId = 0;
            this.BalanceItemInternals = new List<BalanceItemInternalDTO>();
        }

        public BalanceItemInternalDTO GetBalanceItemInternal(List<AccountInternalDTO> accountInternals)
        {
            foreach (BalanceItemInternalDTO existingBalanceItemInternal in this.BalanceItemInternals)
            {
                if (Validator.IsAccountInInterval(existingBalanceItemInternal.AccountInternals, accountInternals))
                    return existingBalanceItemInternal;
            }

            BalanceItemInternalDTO balanceItemInternal = new BalanceItemInternalDTO()
            {
                AccountInternals = accountInternals,
            };

            this.BalanceItemInternals.Add(balanceItemInternal);
            return balanceItemInternal;
        }
    }

    public class BalanceItemInternalDTO : BalanceItemBase
    {
        public List<AccountInternalDTO> AccountInternals { get; set; }
    }
}
