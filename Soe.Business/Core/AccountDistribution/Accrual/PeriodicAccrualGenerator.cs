
using SoftOne.Soe.Business.Core.AccountDistribution.Accrual;
using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;

namespace SoftOne.Soe.Business.Core.AccountDistribution
{
    internal class AccrualGeneratorParameters
    {
        public int ActorCompanyId { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate {
            get => PeriodStartDate.AddMonths(1);
        }
        public AccountYear AccountYear { get; set; }
        public AccountDim AccountDimStd { get; set; }
        public AccrualGeneratorParameters(int actorCompanyId, DateTime dateInPeriod, AccountYear accountYear, AccountDim accountDimStd)
        {
            ActorCompanyId = actorCompanyId;
            PeriodStartDate = new DateTime(dateInPeriod.Year, dateInPeriod.Month, 1);
            AccountYear = accountYear;
            AccountDimStd = accountDimStd;
        }
    }

    internal class PeriodicAccrualGenerator
    {
        private AccrualGeneratorParameters _params;
        private IAccrualQueryService _queryService;
        private IDBBulkService _dbService;
        private IStateUtility _dbUtility;
        private AccrualEntryBuilder _entryBuilder;
        private AccrualConverter _converter;


        public PeriodicAccrualGenerator(AccrualGeneratorParameters parameters, IAccrualQueryService queryService, IAccountDistributionEntryRowCurrencySetter currencySetter, IDBBulkService dbService, IStateUtility dbUtility) 
        {
            _params = parameters;
            _queryService = queryService;
            _dbService = dbService;
            _dbUtility = dbUtility;
            _entryBuilder = new AccrualEntryBuilder(parameters.AccountDimStd);
            _converter = new AccrualConverter(parameters.ActorCompanyId, queryService, dbUtility, currencySetter);

        }

        public ActionResult PerformGeneration()
        {
            // Identify entries to delete
            var entriesInPeriod = _queryService.GetEntriesInPeriod(_params.PeriodStartDate, _params.PeriodEndDate);
            var entriesToDelete = _queryService.GetPreliminaryEntries(entriesInPeriod);
            var deleteResult = MarkEntriesForDelete(entriesToDelete);
            if (!deleteResult.Success)
                return deleteResult;

            var heads = _queryService.GetPeriodDistributionHeads();

            // Get active heads eligible for generation
            var activeHeads = _queryService.GetActiveNonTransferredHeads(
                heads,
                entriesInPeriod,
                _params.PeriodStartDate,
                _params.PeriodEndDate);
            if (activeHeads.IsNullOrEmpty())
                return new ActionResult();

            // Generate entries to create
            var entriesToCreate = new List<AccountDistributionEntry>();
            entriesToCreate.AddRange(GenerateAmountEntries(activeHeads));
            entriesToCreate.AddRange(GeneratePeriodEntries(activeHeads, TermGroup_AccountDistributionPeriodType.Period));
            entriesToCreate.AddRange(GeneratePeriodEntries(activeHeads, TermGroup_AccountDistributionPeriodType.Year));

            if (entriesToDelete.IsNullOrEmpty() && entriesToCreate.IsNullOrEmpty())
                return new ActionResult();

            foreach (var entry in entriesToCreate)
            {
                if (!entry.AccountDistributionEntryRow.IsNullOrEmpty())
                    _dbUtility.AddObject("AccountDistributionEntry", entry);
            }

            return _dbService.SaveChanges();
        }

        public ActionResult MarkEntriesForDelete(IEnumerable<AccountDistributionEntry> entries)
        {
            foreach (var entry in entries)
            {
                var result = _dbUtility.MarkAsDeleted(entry);
                if (!result.Success)
                    return result;
            }
            return new ActionResult();
        }

        #region Period Based Entries

        private IEnumerable<AccountDistributionEntry> GeneratePeriodEntries(
            IEnumerable<AccountDistributionHead> heads,
            TermGroup_AccountDistributionPeriodType periodType)
        {
            var headsForPeriodType = heads.Where(h => h.PeriodType == (int)periodType);

            if (headsForPeriodType.IsNullOrEmpty())
                return Enumerable.Empty<AccountDistributionEntry>();

            (DateTime from, DateTime to) = AccrualCalculator.GetVoucherDateInterval(
                periodType,
                _params.PeriodStartDate,
                _params.AccountYear.From);

            var accountingRows = _queryService.GetRelevantVoucherRows(headsForPeriodType, from, to);

            if (periodType == TermGroup_AccountDistributionPeriodType.Year)
            {
                var openingBalanceRows = _queryService.GetAccountBalances(_params.AccountYear.AccountYearId);
                accountingRows = accountingRows.Concat(openingBalanceRows);
            }

            var entriesToCreate = new List<AccountDistributionEntry>();
            foreach (var head in headsForPeriodType)
            {
                if (accountingRows.IsNullOrEmpty() && head.GetCalculationType() == TermGroup_AccountDistributionCalculationType.Percent)
                    continue;

                var accrualEntry = _entryBuilder.BuildPeriodBasedEntry(head, accountingRows, _params.PeriodStartDate);

                if (accrualEntry.IsValidForCreation())
                {
                    var adEntry = _converter.CreateADEntry(accrualEntry);
                    entriesToCreate.Add(adEntry);
                }
            }

            if (entriesToCreate.IsNullOrEmpty()) 
                return Enumerable.Empty<AccountDistributionEntry>();

            return entriesToCreate;
        }

        #endregion
        #region Amount Based Entries

        private IEnumerable<AccountDistributionEntry> GenerateAmountEntries(
            IEnumerable<AccountDistributionHead> activeHeads
            )
        {
            var amountHeads = activeHeads
                .Where(h => h.PeriodType == (int)TermGroup_AccountDistributionPeriodType.Amount);

            var entriesToCreate = new List<AccountDistributionEntry>();
            foreach (var head in amountHeads)
            {
                var entry = _entryBuilder.BuildAmountBasedEntry(head, _params.PeriodStartDate);
                if (entry.IsValidForCreation())
                {
                    var adEntry = _converter.CreateADEntry(entry);
                    entriesToCreate.Add(adEntry);
                }
            }
            return entriesToCreate;
        }

        #endregion
    }
}
