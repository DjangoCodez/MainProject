using SoftOne.Soe.Business.Core.AccountDistribution.Accrual;
using SoftOne.Soe.Business.Core.Voucher;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.AccountDistribution
{
    /// <summary>
    /// Builds AccountDistributionEntry and EntryRow objects from distribution heads and source data
    /// Pure builder - no persistence, no currency calculations, no EF dependencies
    /// </summary>
    internal class AccrualEntryBuilder
    {
        private readonly int _accountDimStdId;

        public AccrualEntryBuilder(AccountDim accountDimStd)
        {
            _accountDimStdId = accountDimStd.AccountDimId;
        }


        /// <summary>
        /// Builds an entry from a accounting row with fixed amounts (Accounting row based calculation type)
        /// </summary>
        public AccrualEntry BuildAccountingRowBasedEntry(
            MinimalAccountingRow accountingRow,
            DateTime periodStartDate,
            TermGroup_AccountDistributionRegistrationType registrationType,
            int accrualAccountId,
            int sourceId,
            string accrualName)
        {
            var entry = CreateAccountingRowBaseEntry(periodStartDate, registrationType, sourceId, accountingRow.RowId, accrualName);

            int numberOfPeriods = accountingRow.NumberOfPeriods;
            if (numberOfPeriods <= 0)
                return entry;

            entry.AddRow(BuildAccountingRowBasedEntryRow(accountingRow, numberOfPeriods));
            entry.AddRow(BuildAccountingRowBasedEntryRow(accountingRow, numberOfPeriods, accrualAccountId, true));

            return entry;
        }

        /// <summary>
        /// Builds an entry from a head with fixed amounts (Amount-based calculation type)
        /// </summary>
        public AccrualEntry BuildAmountBasedEntry(
            AccountDistributionHead head,
            DateTime periodStartDate)
        {
            var entry = CreateBaseEntry(head, periodStartDate);

            foreach (var headRow in head.AccountDistributionRow.Where(r => r.State == (int)SoeEntityState.Active))
            {
                if (headRow.AccountId == null) continue;

                var entryRow = BuildAmountBasedEntryRow(headRow);
                entry.AddRow(entryRow);
            }

            return entry;
        }

        /// <summary>
        /// Builds an entry from period/year voucher data
        /// </summary>
        public AccrualEntry BuildPeriodBasedEntry(
            AccountDistributionHead head,
            IEnumerable<MinimalAccountingRow> voucherRows,
            DateTime periodStartDate)
        {
            var matchingRows = AccountDistributionUtility.GetRowsMatchingExpression(head, _accountDimStdId, voucherRows);

            // Group rows by account composition and sum amounts
            var groupedData = AccrualCalculator.GroupAndSumByAccount(matchingRows);

            var entry = CreateBaseEntry(head, periodStartDate);

            if (head.GetCalculationType() == TermGroup_AccountDistributionCalculationType.Amount)
            {
                entry.Rows.AddRange(BuildDistributionEntryRows(head.AccountDistributionRow, head.GetCalculationType(), null));
                return entry;
            }

            foreach (var (representative, totalAmount) in groupedData)
            {
                var sourceRow = representative.Copy();
                sourceRow.Amount = totalAmount;

                entry.Rows.AddRange(BuildDistributionEntryRows(head.AccountDistributionRow, head.GetCalculationType(), sourceRow));
            }

            return entry;
        }

        /// <summary>
        /// Builds distribution entry rows from a source accounting row, handling base and derived rows
        /// </summary>
        private List<AccrualEntryRow> BuildDistributionEntryRows(
            IEnumerable<AccountDistributionRow> templateRows,
            TermGroup_AccountDistributionCalculationType calculationType,
            MinimalAccountingRow sourceRow)
        {
            var entryRows = new List<AccrualEntryRow>();
            var rowNbrMap = new Dictionary<int, AccrualEntryRow>();
            decimal sourceAmount = sourceRow != null ? sourceRow.Amount : 0;

            // Process base rows (calculated from source)
            var baseRows = templateRows 
                .Where(r => r.CalculateRowNbr == 0)
                .Where(r => r.State == (int)SoeEntityState.Active)
                .OrderBy(r => r.RowNbr);

            foreach (var row in baseRows)
            {
                var entryRow = BuildPeriodBasedEntryRow(
                    sourceAmount,
                    calculationType,
                    row,
                    sourceRow);

                entryRows.Add(entryRow);
                if (!rowNbrMap.ContainsKey(row.RowNbr))
                    rowNbrMap.Add(row.RowNbr, entryRow);
            }

            // Process derived rows (calculated from other rows)
            var derivedRows = templateRows 
                .Where(r => r.CalculateRowNbr != 0)
                .Where(r => r.State == (int)SoeEntityState.Active)
                .OrderBy(r => r.RowNbr);

            foreach (var row in derivedRows)
            {
                if (!rowNbrMap.ContainsKey(row.CalculateRowNbr)) continue;

                var parentRow = rowNbrMap[row.CalculateRowNbr];
                if (parentRow == null) continue;

                var amount = parentRow.Amount;
                var entryRow = BuildPeriodBasedEntryRow(
                    amount,
                    calculationType,
                    row,
                    sourceRow);

                entryRows.Add(entryRow);
                if (!rowNbrMap.ContainsKey(row.RowNbr))
                    rowNbrMap.Add(row.RowNbr, entryRow);
            }

            return entryRows;
        }

        /// <summary>
        /// Builds an entry row for accounting row based distributions
        /// </summary>
        private AccrualEntryRow BuildAccountingRowBasedEntryRow(MinimalAccountingRow accountingRow, int numberOfPeriods, int accrualAccountId = 0, bool accrualRow = false)
        {
            var entryRow = new AccrualEntryRow()
            {
                AccountId = accrualRow ? accrualAccountId : accountingRow.AccountId,
            };

            decimal debitAmount = accountingRow.Amount > 0 ? accountingRow.Amount : 0;
            decimal creditAmount = accountingRow.Amount < 0 ? Math.Abs(accountingRow.Amount) : 0;


            decimal sameBalance = debitAmount / numberOfPeriods;
            decimal oppositeBalance = creditAmount / numberOfPeriods;

            if (accrualRow)
            {
                entryRow.SetAmount(oppositeBalance, sameBalance);
                return entryRow;
            }

            entryRow.SetAmount(sameBalance, oppositeBalance);

            foreach (var account in accountingRow.Accounts)
            {
                entryRow.AddInternalAccount(account.AccountId); 
            }

            return entryRow;
        }

        /// <summary>
        /// Builds an entry row for amount-based distributions
        /// </summary>
        private AccrualEntryRow BuildAmountBasedEntryRow(AccountDistributionRow headRow)
        {
            var entryRow = new AccrualEntryRow()
            {
                AccountId = headRow.AccountId.Value
            };
            entryRow.SetAmount(headRow.SameBalance, headRow.OppositeBalance);

            foreach (var accountInternal in headRow.AccountDistributionRowAccount)
                entryRow.AddInternalAccount(accountInternal.AccountId);

            return entryRow;
        }

        /// <summary>
        /// Builds an entry row for period-based distributions
        /// </summary>
        private AccrualEntryRow BuildPeriodBasedEntryRow(
            decimal sourceAmount,
            TermGroup_AccountDistributionCalculationType calculationType,
            AccountDistributionRow distributionRow,
            MinimalAccountingRow sourceRow)
        {
            var (debit, credit) = AccrualCalculator.CalculateAmounts(
                calculationType,
                sourceAmount,
                distributionRow.SameBalance,
                distributionRow.OppositeBalance);

            var entryRow = new AccrualEntryRow()
            {
                AccountId = distributionRow.AccountId ?? sourceRow.AccountId,
            };
            entryRow.SetAmount(debit, credit);

            // Handle account dimensions
            foreach (var account in distributionRow.AccountDistributionRowAccount)
            {
                if (account.KeepSourceRowAccount)
                {
                    var accountInSource = sourceRow.Accounts.FirstOrDefault(ai => ai.AccountDimNr == account.DimNr);
                    if (accountInSource != null)
                    {
                        entryRow.AddInternalAccount(accountInSource.AccountId);
                    }
                }
                else if (account.AccountId != null)
                {
                    entryRow.AddInternalAccount(account.AccountId);
                }
            }

            return entryRow;
        }

        /// <summary>
        /// Creates the base entry object with common properties
        /// </summary>
        private AccrualEntry CreateBaseEntry(AccountDistributionHead head, DateTime periodStartDate)
        {
            var entry = new AccrualEntry()
            {
                TriggerType = (TermGroup_AccountDistributionTriggerType)head.TriggerType,
                Date = AccountDistributionUtility.GetEntryDate(head.DayNumber, periodStartDate),
                AccountDistributionHeadId = head.AccountDistributionHeadId,
            };

            return entry;
        }

        /// <summary>
        /// Creates the accounting row base entry object with common properties
        /// </summary>
        private AccrualEntry CreateAccountingRowBaseEntry(DateTime periodStartDate, TermGroup_AccountDistributionRegistrationType registrationType, int sourceId, int sourceRowId, string accrualName)
        {
            var entry = new AccrualEntry()
            {
                AccountDistributionHeadId = null,
                Name = accrualName,
                TriggerType = TermGroup_AccountDistributionTriggerType.Registration,
                RegistrationType = registrationType,
                SourceCustomerInvoiceId = registrationType == TermGroup_AccountDistributionRegistrationType.CustomerInvoice ? sourceId : 0,
                SourceSupplierInvoiceId = registrationType == TermGroup_AccountDistributionRegistrationType.SupplierInvoice ? sourceId : 0,
                SourceVoucherHeadId = registrationType == TermGroup_AccountDistributionRegistrationType.Voucher ? sourceId : 0,
                SourceRowId = sourceRowId,

                Date = periodStartDate,
            };

            return entry;
        }
    }
}
