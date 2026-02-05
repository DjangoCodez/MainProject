using SoftOne.Soe.Business.Core.AccountDistribution;
using SoftOne.Soe.Business.Core.Voucher;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Business.Core.AccountDistribution.Stubs
{
    internal class StubQueryService : IAccrualQueryService
    {
        public AccountDistributionHead HeadToReturn { get; set; }
        public List<AccountDistributionHead> HeadsListToReturn { get; set; } = new List<AccountDistributionHead>();
        public List<AccountDistributionEntry> EntriesToReturn { get; set; } = new List<AccountDistributionEntry>();
        public List<MinimalAccountingRow> VoucherRowsToReturn { get; set; } = new List<MinimalAccountingRow>();
        public List<MinimalAccountingRow> BalancesToReturn { get; set; } = new List<MinimalAccountingRow>();
        public AccountInternal AccountInternalToReturn { get; set; }
        public bool HasTransferredEntry { get; set; }

        public IEnumerable<AccountDistributionHead> GetActiveNonTransferredHeads(
            IEnumerable<AccountDistributionHead> allHeads,
            IEnumerable<AccountDistributionEntry> existingEntries,
            DateTime periodStartDate,
            DateTime periodEndDate)
        {
            return HeadsListToReturn;
        }

        public IEnumerable<MinimalAccountingRow> GetRelevantVoucherRows(
            IEnumerable<AccountDistributionHead> heads,
            DateTime from,
            DateTime to)
        {
            return VoucherRowsToReturn;
        }

        public IEnumerable<MinimalAccountingRow> GetAccountBalances(int accountYearId)
        {
            return BalancesToReturn;
        }

        public IEnumerable<AccountDistributionEntry> GetPreliminaryEntries(IEnumerable<AccountDistributionEntry> entriesInPeriod)
        {
            if (EntriesToReturn != null && EntriesToReturn.Any()) return EntriesToReturn;
            
            return entriesInPeriod.Where(e => e.VoucherHeadId == null).ToList();
        }

        public IEnumerable<AccountDistributionEntry> GetEntriesInPeriod(DateTime periodStartDate, DateTime periodEndDate)
        {
            return EntriesToReturn
                .Where(e => e.Date >= periodStartDate && e.Date < periodEndDate)
                .ToList();
        }

        public IEnumerable<AccountDistributionHead> GetPeriodDistributionHeads()
        {
            return HeadsListToReturn;
        }

        public bool HasTransferredEntryForHeadInPeriod(int headId, DateTime periodDate)
        {
            return HasTransferredEntry;
        }

        public AccountDistributionHead GetPeriodDistributionHead(int headId)
        {
            return HeadToReturn;
        }

        public IEnumerable<AccountDistributionEntry> GetEntriesInHead(int headId)
        {
            return EntriesToReturn.Where(e => e.AccountDistributionHeadId == headId).ToList();
        }

        public AccountInternal GetAccountInternal(int accountId)
        {
            if (AccountInternalToReturn != null) return AccountInternalToReturn;
            
            return new AccountInternal { AccountId = accountId };
        }

        IEnumerable<MinimalAccountingRow> IAccrualQueryService.GetVoucherRows(int voucherHeadId)
        {
            throw new NotImplementedException();
        }
    }
}
