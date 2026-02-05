using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Business.Core.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.AccountDistribution
{

    interface IAccrualQueryService
    {
        IEnumerable<AccountDistributionHead> GetActiveNonTransferredHeads(
           IEnumerable<AccountDistributionHead> allHeads,
           IEnumerable<AccountDistributionEntry> existingEntries,
           DateTime periodStartDate,
           DateTime periodEndDate);
        IEnumerable<MinimalAccountingRow> GetVoucherRows(int voucherHeadId);
        IEnumerable<MinimalAccountingRow> GetRelevantVoucherRows(
            IEnumerable<AccountDistributionHead> heads,
            DateTime from,
            DateTime to);
        IEnumerable<MinimalAccountingRow> GetAccountBalances(int accountYearId);
        IEnumerable<AccountDistributionEntry> GetPreliminaryEntries(IEnumerable<AccountDistributionEntry> entriesInPeriod);
        IEnumerable<AccountDistributionEntry> GetEntriesInPeriod(DateTime periodStartDate, DateTime periodEndDate);
        IEnumerable<AccountDistributionHead> GetPeriodDistributionHeads();
        bool HasTransferredEntryForHeadInPeriod(int headId, DateTime periodDate);
        AccountDistributionHead GetPeriodDistributionHead(int headId);
        IEnumerable<AccountDistributionEntry> GetEntriesInHead(int headId);
        AccountInternal GetAccountInternal(int accountId);
    }

    /// <summary>
    /// Handles all query operations for accrual generation - can be tested with in-memory collections
    /// </summary>
    internal class AccrualQueryService : IAccrualQueryService
    {
        private readonly CompEntities _entities;
        private readonly int _actorCompanyId;
        private readonly AccountDim _accountDimStd;

        public AccrualQueryService(CompEntities entities, int actorCompanyId, AccountDim accountDim)
        {
            _entities = entities;
            _actorCompanyId = actorCompanyId;
            _accountDimStd = accountDim;
        }

        /// <summary>
        /// Gets active, non-transferred distribution heads eligible for generation
        /// </summary>
        public IEnumerable<AccountDistributionHead> GetActiveNonTransferredHeads(
            IEnumerable<AccountDistributionHead> allHeads,
            IEnumerable<AccountDistributionEntry> existingEntries,
            DateTime periodStartDate,
            DateTime periodEndDate)
        {
            var activeHeads = allHeads
                .Where(h => h.TriggerType == (int)TermGroup_AccountDistributionTriggerType.Distribution)
                .Where(h => h.StartDate == null || h.StartDate < periodEndDate)
                .Where(h => h.EndDate == null || h.EndDate >= periodStartDate)
                .Where(h => h.State == (int)SoeEntityState.Active)
                .OrderBy(h => h.Sort);

            var transferredHeadIds = existingEntries
                .Where(e => e.VoucherHeadId != null)
                .Select(e => e.AccountDistributionHeadId)
                .ToHashSet();

            return activeHeads.Where(h => !transferredHeadIds.Contains(h.AccountDistributionHeadId));
        }


        /// <summary>
        /// Gets voucher rows matching the voucher head
        /// </summary>
        public IEnumerable<MinimalAccountingRow> GetVoucherRows(int voucherHeadId)
        {
            return _entities.VoucherRow
                .Where(vr => vr.VoucherHead.ActorCompanyId == _actorCompanyId)
                .Where(vr => vr.State == (int)SoeEntityState.Active)
                .Where(vr => vr.VoucherHead.VoucherHeadId == voucherHeadId)
                .Select(VoucherRowProjections.MinimalVoucherRowQuery)
                .ToList();
        }

        /// <summary>
        /// Gets voucher rows matching the distribution heads' account expressions within date range
        /// </summary>
        public IEnumerable<MinimalAccountingRow> GetRelevantVoucherRows(
            IEnumerable<AccountDistributionHead> heads,
            DateTime from,
            DateTime to)
        {
            var accountIds = GetRelevantAccountStdIds(heads, _accountDimStd);

            if (accountIds.IsNullOrEmpty())
                return new List<MinimalAccountingRow>();

            return _entities.VoucherRow
                .Where(vr => vr.VoucherHead.ActorCompanyId == _actorCompanyId)
                .Where(vr => vr.State == (int)SoeEntityState.Active)
                .Where(vr => vr.VoucherHead.Date >= from)
                .Where(vr => vr.VoucherHead.Date < to)
                .Where(vr => accountIds.Contains(vr.AccountId))
                .Select(VoucherRowProjections.MinimalVoucherRowQuery)
                .ToList();
        }

        /// <summary>
        /// Gets account year opening balances
        /// </summary>
        public IEnumerable<MinimalAccountingRow> GetAccountBalances(int accountYearId)
        {
            return _entities.AccountYearBalanceHead
                .Where(h => h.AccountYear.AccountYearId == accountYearId)
                .Select(VoucherRowProjections.MinimalYearBalanceQuery)
                .ToList();
        }

        /// <summary>
        /// Get all entries in period.
        /// </summary>
        public IEnumerable<AccountDistributionEntry> GetEntriesInPeriod(DateTime periodStartDate, DateTime periodEndDate)
        {
            return this.accountDistributionEntries()
                        .Where(e => e.Date >= periodStartDate && e.Date < periodEndDate);
        }

        public IEnumerable<AccountDistributionHead> GetPeriodDistributionHeads()
        {
            return this.accountDistributionHeads()
                        .Where(h => h.Type == (int)TermGroup_AccountDistributionTriggerType.Distribution)
                        .Where(h => h.CalculationType == (int)TermGroup_AccountDistributionCalculationType.Percent || h.CalculationType == (int)TermGroup_AccountDistributionCalculationType.Amount)
                        .ToList();
        }

        public bool HasTransferredEntryForHeadInPeriod(int headId, DateTime periodDate)
        {
            DateTime startDate = new DateTime(periodDate.Year, periodDate.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            return this.accountDistributionEntries()
                .Where(e => e.AccountDistributionHeadId == headId)
                .Where(e => e.Date >= startDate && e.Date < endDate)
                .Where(e => e.VoucherHeadId != null) 
                .Any();
        }

        public AccountDistributionHead GetPeriodDistributionHead(int headId)
        {
            return this.accountDistributionHeads()
                        .Where(h => h.AccountDistributionHeadId == headId)
                        .Where(h => h.Type == (int)TermGroup_AccountDistributionTriggerType.Distribution)
                        .Where(h => h.CalculationType == (int)TermGroup_AccountDistributionCalculationType.Percent || h.CalculationType == (int)TermGroup_AccountDistributionCalculationType.Amount)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Gets entries belongs to head
        /// </summary>
        public IEnumerable<AccountDistributionEntry> GetEntriesInHead(int headId)
        {
            return this.accountDistributionEntries()
                        .Where(e => e.AccountDistributionHeadId == headId);
        }

        /// <summary>
        /// Gets preliminary entries that need to be deleted before generation
        /// </summary>
        public IEnumerable<AccountDistributionEntry> GetPreliminaryEntries(IEnumerable<AccountDistributionEntry> entriesInPeriod)
        {
            return entriesInPeriod
                .Where(e => e.AccountDistributionHead != null)
                .Where(e => e.AccountDistributionHead.TriggerType == (int)TermGroup_AccountDistributionTriggerType.Distribution)
                .Where(e => e.VoucherHeadId == null);
        }

        public AccountInternal GetAccountInternal(int accountId)
        {
            return _entities.AccountInternal.FirstOrDefault(a => a.AccountId == accountId && a.Account.ActorCompanyId == _actorCompanyId);
        }

        #region Private Helper Methods

        private IEnumerable<int> GetRelevantAccountStdIds(IEnumerable<AccountDistributionHead> heads, AccountDim dimStd)
        {
            var regexList = new List<Regex>();

            foreach (var head in heads)
            {
                var regex = GetDim1Regex(head, dimStd);
                if (regex != null)
                    regexList.Add(regex);
            }

            var accountIds = new List<int>();

            foreach (var account in dimStd.Account)
            {
                if (regexList.Any(r => r.IsMatch(account.AccountNr)))
                    accountIds.Add(account.AccountId);
            }

            return accountIds;
        }

        private Regex GetDim1Regex(AccountDistributionHead head, AccountDim dimStd)
        {
            var dim1Mapping = head.AccountDistributionHeadAccountDimMapping
                .FirstOrDefault(m => m.AccountDimId == dimStd.AccountDimId);

            if (dim1Mapping == null) return null;

            return AccountDistributionUtility.GetRegexFromExpression(dim1Mapping);
        }

        private IQueryable<AccountDistributionHead> accountDistributionHeads() 
        {
            return _entities.AccountDistributionHead
                        .Include("AccountDistributionHeadAccountDimMapping")
                        .Include("AccountDistributionRow.AccountDistributionRowAccount")
                        .Where(h => h.ActorCompanyId == _actorCompanyId && h.State == (int)SoeEntityState.Active);
        }

        private IQueryable<AccountDistributionEntry> accountDistributionEntries()
        {
            return _entities.AccountDistributionEntry
                        .Include("AccountDistributionEntryRow")
                        .Include("AccountDistributionHead")
                        .Where(e => e.ActorCompanyId == _actorCompanyId && e.State == (int)SoeEntityState.Active);
        }

        #endregion
    }
}
