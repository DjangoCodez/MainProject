using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.AccountDistribution.Accrual
{
    internal class AccrualEntry
    {
        public int? AccountDistributionHeadId { get; set; }
        public string Name { get; set; }
        public TermGroup_AccountDistributionTriggerType TriggerType { get; set; }
        public TermGroup_AccountDistributionRegistrationType RegistrationType { get; set; }
        public DateTime Date { get; set; }

        public List<AccrualEntryRow> Rows { get; set; } = new List<AccrualEntryRow>();
        public int SourceCustomerInvoiceId { get; set; }
        public int SourceVoucherHeadId { get; set; }
        public int SourceSupplierInvoiceId { get; set; }
        public int SourceRowId { get; set; }

        public void AddRow(AccrualEntryRow row)
        {
            Rows.Add(row);
        }

        private void FinalizeAndReconcile()
        {
            MergeRows();
            AdjustAmountDifference();
            Rows = Rows.Where(r => r.Amount != 0).ToList();
        }

        private void MergeRows()
        {
            var mergedRows = new List<AccrualEntryRow>();
            foreach (var group in Rows.GroupBy(r => r.GetAccountsKey()))
            {
                if (group.Count() <= 1)
                    mergedRows.Add(group.First());
                else
                {
                    var firstRow = group.First();
                    foreach (var duplicate in group.Skip(1))
                        firstRow.AdjustAmount(duplicate.Amount);

                    mergedRows.Add(firstRow);
                }
            }
            Rows = mergedRows;
        }
        private void AdjustAmountDifference()
        {
            var diff = Rows.Sum(r => r.Amount);
            if (diff != 0)
            {
                var lastRow = Rows.Last();
                lastRow.AdjustAmount(diff);
            }
        }

        public bool IsValidForCreation()
        {
            FinalizeAndReconcile();
            if (Rows.IsNullOrEmpty() || Rows.Count == 0) return false;
            return true;
        }
    }

    internal class AccrualEntryRow
    {
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
        public List<int> InternalAccountIds { get; set; } = new List<int>();

        public void SetAmount(decimal debit, decimal credit)
        {
            Amount = debit - credit;
        }
        public void AddInternalAccount(int? accountId)
        {
            if (accountId.GetValueOrDefault() > 0)
                InternalAccountIds.Add(accountId.Value);
        }

        public string GetAccountsKey()
        {
            InternalAccountIds.Sort();
            var internalAccountsKey = string.Join(";", InternalAccountIds);
            return $"{AccountId};{internalAccountsKey}";
        }
        
        public void AdjustAmount(decimal adjustment)
        {
            Amount += adjustment;
        }
    }
}
