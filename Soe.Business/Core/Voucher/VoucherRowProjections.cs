using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SoftOne.Soe.Business.Core.Voucher
{
    sealed class MinimalAccountingRow
    {
        public int RowId { get; set; }
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public int? AccountDistributionHeadId { get; set; }
        public decimal Amount { get; set; }
        public int NumberOfPeriods { get; set; }
        public DateTime? StartDate { get; set; }
        public List<MinimalAccountInternal> Accounts { get; set; }

        private string _hashKey = string.Empty;
        public MinimalAccountingRow Copy()
        {
            return new MinimalAccountingRow()
            {
                RowId = RowId,
                AccountId = AccountId,
                AccountNr = AccountNr,
                AccountName = AccountName,
                AccountDistributionHeadId = AccountDistributionHeadId,
                Amount = Amount,
                Accounts = Accounts
                    .Select(a => a.Copy())
                    .ToList()
           };
        }
        public string GetAccountCompositionKey()
        {
            if (!string.IsNullOrEmpty(_hashKey)) return _hashKey;
            var accs = Accounts
                .Select(a => a.GetKey())
                .OrderBy(k => k)
                .ToArray();

            return (_hashKey = $"{AccountId};{string.Join(";", accs)}");
        }
    }
    sealed class MinimalAccountInternal
    {
        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public int AccountDimId { get; set; }
        public int AccountDimNr { get; set; }
        
        public MinimalAccountInternal Copy()
        {
            return new MinimalAccountInternal()
            {
                AccountId = AccountId,
                AccountNr = AccountNr,
                AccountName = AccountName,
                AccountDimId = AccountDimId,
                AccountDimNr = AccountDimNr,
            };
        }
        public string GetKey()
        {
            return $"{AccountDimId}:{AccountId}";
        }
    }
    static class VoucherRowProjections 
    {

        public static Expression<Func<VoucherRow, MinimalAccountingRow>> MinimalVoucherRowQuery =
            r => new MinimalAccountingRow
            {
                RowId = r.VoucherRowId,
                AccountId = r.AccountId,
                AccountName = r.AccountStd.Account.Name,
                AccountNr = r.AccountStd.Account.AccountNr,
                AccountDistributionHeadId = r.AccountDistributionHeadId,
                Amount = r.Amount,
                NumberOfPeriods = r.NumberOfPeriods ?? 0,
                StartDate = r.StartDate ?? null,
                Accounts = r.AccountInternal
                        .Select(ai => new MinimalAccountInternal
                        {
                            AccountId = ai.AccountId,
                            AccountDimId = ai.Account.AccountDimId,
                            AccountName = ai.Account.Name,
                            AccountNr = ai.Account.AccountNr,
                            AccountDimNr = ai.Account.AccountDim.AccountDimNr
                        })
                        .ToList()
            };

        public static Expression<Func<AccountYearBalanceHead, MinimalAccountingRow>> MinimalYearBalanceQuery =
            r => new MinimalAccountingRow
            {
                RowId = r.AccountYearBalanceHeadId,
                AccountId = r.AccountStd.AccountId,
                AccountName = r.AccountStd.Account.Name,
                AccountNr = r.AccountStd.Account.AccountNr,
                Amount = r.Balance,
                Accounts = r.AccountInternal
                        .Select(ai => new MinimalAccountInternal
                        {
                            AccountId = ai.AccountId,
                            AccountDimId = ai.Account.AccountDimId,
                            AccountName = ai.Account.Name,
                            AccountNr = ai.Account.AccountNr,
                            AccountDimNr = ai.Account.AccountDim.AccountDimNr
                        })
                        .ToList()
            };


    }
}
