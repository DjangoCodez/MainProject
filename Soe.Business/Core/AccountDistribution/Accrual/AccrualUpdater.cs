using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.AccountDistribution.Accrual
{
    internal class AccrualUpdaterParameters
    {
        public int ActorCompanyId { get; set; }
        public int AccountDistributionHeadId { get; set; }
        public bool DatesChanged { get; set; }
        public bool DistributionRowsChanged { get; set; }
        public AccountDim AccountDimStd { get; set; }
        public AccountYear AccountYear { get; set; }


        public AccrualUpdaterParameters(int actorCompanyId, int headId, bool datesChanged, bool rowsChanged, AccountDim accountDimStd, AccountYear accountYear)
        {
            ActorCompanyId = actorCompanyId;
            AccountDistributionHeadId = headId;
            DatesChanged = datesChanged;
            DistributionRowsChanged = rowsChanged;
            AccountDimStd = accountDimStd;
            AccountYear = accountYear;
        }
    }

    internal class AccrualUpdater
    {
        private AccrualUpdaterParameters _params;
        private IAccrualQueryService _queryService;
        private IDBBulkService _dbService;
        private IStateUtility _dbUtility;
        private AccrualEntryBuilder _entryBuilder;
        private AccrualConverter _converter;

        public AccrualUpdater(AccrualUpdaterParameters parameters, IAccrualQueryService queryService, IAccountDistributionEntryRowCurrencySetter currencySetter, IDBBulkService dbService, IStateUtility dbUtility)
        {
            _params = parameters;
            _queryService = queryService;
            _dbService = dbService;
            _dbUtility = dbUtility;
            _entryBuilder = new AccrualEntryBuilder(parameters.AccountDimStd);
            _converter = new AccrualConverter(parameters.ActorCompanyId, queryService, dbUtility, currencySetter);
        }

        public ActionResult PerformUpdate()
        {
            var head = _queryService.GetPeriodDistributionHead(_params.AccountDistributionHeadId);
            if (head == null) return new ActionResult((int)ActionResultSave.EntityNotFound);

            if (head.CalculationType != (int)TermGroup_AccountDistributionCalculationType.Amount)
                return new ActionResult((int)ActionResultSave.IncorrectInput);

            var existingEntries = _queryService.GetEntriesInHead(_params.AccountDistributionHeadId);
            var processedDates = new HashSet<DateTime>();

            (DateTime start, DateTime end) = AccrualCalculator.GetHeadDuration(head);
            int months = AccrualCalculator.GetMonthCount(head);

            for (int i = 0; i < months; i++)
            {
                DateTime periodDate = start.AddMonths(i);
                DateTime entryDate = AccountDistributionUtility.GetEntryDate(head.DayNumber, periodDate);

                processedDates.Add(entryDate);

                var existingEntry = existingEntries.FirstOrDefault(e => e.Date.Year == entryDate.Year && e.Date.Month == entryDate.Month);

                ActionResult result;
                if (existingEntry != null)
                {
                    result = UpdateEntry(existingEntry, head, periodDate, entryDate);
                }
                else
                {
                    result = CreateEntry(head, periodDate);
                }

                if (!result.Success) return result;
            }

            if (_params.DatesChanged)
            {
                var deleteResult = DeleteOutdatedEntries(existingEntries, processedDates);
                if (!deleteResult.Success) return deleteResult;
            }

            return _dbService.SaveChanges();
        }

        private ActionResult UpdateEntry(AccountDistributionEntry existingEntry, AccountDistributionHead head, DateTime periodDate, DateTime entryDate)
        {
            if (existingEntry.VoucherHeadId != null) return new ActionResult();

            if (!_params.DistributionRowsChanged) return new ActionResult();

            existingEntry.Date = entryDate;

            existingEntry.AccountDistributionEntryRow.ToList().ForEach(row =>
            {
                _dbUtility.DeleteObject(row);
            });
            existingEntry.AccountDistributionEntryRow.Clear();

            var newEntryData = BuildEntryData(head, periodDate);
            if (newEntryData == null || !newEntryData.IsValidForCreation()) return new ActionResult(); 

            _converter.UpdateExistingEntryRows(existingEntry, newEntryData.Rows);

            return new ActionResult();
        }

        private ActionResult CreateEntry(AccountDistributionHead head, DateTime periodDate)
        {
           var hasTransferred = _queryService.HasTransferredEntryForHeadInPeriod(head.AccountDistributionHeadId, periodDate);
            if (hasTransferred) return new ActionResult();

            var newEntryData = BuildEntryData(head, periodDate);

            if (newEntryData != null && newEntryData.IsValidForCreation())
            {
                var adEntry = _converter.CreateADEntry(newEntryData);
                _dbUtility.AddObject("AccountDistributionEntry", adEntry);
            }

            return new ActionResult();
        }

        private ActionResult DeleteOutdatedEntries(IEnumerable<AccountDistributionEntry> existingEntries, HashSet<DateTime> validDates)
        {
            foreach (var entry in existingEntries)
            {
                if (!validDates.Contains(entry.Date.Date))
                {
                    var result = _dbUtility.MarkAsDeleted(entry);
                    if (!result.Success) return result;
                }
            }
            return new ActionResult();
        }

        #region Helpers

        private AccrualEntry BuildEntryData(AccountDistributionHead head, DateTime periodDate)
        {
            if (head.PeriodType == (int)TermGroup_AccountDistributionPeriodType.Amount)
            {
                return _entryBuilder.BuildAmountBasedEntry(head, periodDate);
            }

            DateTime periodStart = new DateTime(periodDate.Year, periodDate.Month, 1);

            (DateTime from, DateTime to) = AccrualCalculator.GetVoucherDateInterval(
                (TermGroup_AccountDistributionPeriodType)head.PeriodType,
                periodStart,
                _params.AccountYear.From
            );

            var accountingRows = _queryService.GetRelevantVoucherRows([head], from, to);

            if (head.PeriodType == (int)TermGroup_AccountDistributionPeriodType.Year)
            {
                var balances = _queryService.GetAccountBalances(_params.AccountYear.AccountYearId);
                accountingRows = accountingRows.Concat(balances);
            }

            return _entryBuilder.BuildPeriodBasedEntry(head, accountingRows, periodStart);

        }

        #endregion
    }
}
