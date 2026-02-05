using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Business.Core.Voucher;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;


namespace SoftOne.Soe.Business.Core.AccountDistribution.Accrual
{
    internal class AccountingRowAccrualGeneratorParameters
    {
        public int ActorCompanyId { get; set; }
        public int DefaultAccrualCostAccountId { get; set; }
        public int DefaultAccrualRevenueAccountId { get; set; }
        public int SourceId { get; set; }
        public AccountDim AccountDimStd { get; set; }
        public List<AccountingRowDTO> AccountingRows { get; set; }
        public List<AccrualAccountMapping> AccrualAccountMappings { get; set; }
        public TermGroup_AccountDistributionRegistrationType RegistrationType;
        public string AccrualName { get; set; }
        public AccountingRowAccrualGeneratorParameters(
            int actorCompanyId,
            int sourceId,
            AccountDim accountDimStd,
            List<AccrualAccountMapping> accrualAccountMappings,
            TermGroup_AccountDistributionRegistrationType registrationType,
            int defaultAccrualCostAccountId,
            int defaultAccrualRevenueAccountId,
            string accrualName)
        {
            ActorCompanyId = actorCompanyId;
            SourceId = sourceId;
            AccountDimStd = accountDimStd;
            AccrualAccountMappings = accrualAccountMappings;
            RegistrationType = registrationType;
            DefaultAccrualCostAccountId = defaultAccrualCostAccountId;
            DefaultAccrualRevenueAccountId = defaultAccrualRevenueAccountId;
            AccrualName = accrualName;
        }
    }
    class AccountingRowAccrualGenerator
    {
        private AccountingRowAccrualGeneratorParameters _params;
        private IAccrualQueryService _queryService;
        private IDBBulkService _dbService;
        private IStateUtility _dbUtility;
        private IAccountService _accountService;
        private AccrualEntryBuilder _entryBuilder;
        private AccrualConverter _converter;

        public AccountingRowAccrualGenerator(AccountingRowAccrualGeneratorParameters parameters, IAccrualQueryService queryService, IAccountDistributionEntryRowCurrencySetter currencySetter, IDBBulkService dbService, IStateUtility dbUtility, IAccountService accountService) 
        {
            _params = parameters;
            _queryService = queryService;
            _dbService = dbService;
            _dbUtility = dbUtility;
            _accountService = accountService;
            _entryBuilder = new AccrualEntryBuilder(parameters.AccountDimStd);
            _converter = new AccrualConverter(parameters.ActorCompanyId, queryService, dbUtility, currencySetter);
        }

        public ActionResult PerformGeneration()
        {
            var accountingRows = new List<MinimalAccountingRow>();

            switch (_params.RegistrationType)
            {
                case TermGroup_AccountDistributionRegistrationType.Voucher:
                    accountingRows = _queryService.GetVoucherRows(_params.SourceId)
                        .Where(ar => ar.NumberOfPeriods > 0 && ar.StartDate != null)
                        .ToList();
                    break;
                default:
                    break;
            }

            foreach (var accountingRow in accountingRows)
            {
                var entriesToCreate = GenerateAccrualEntries(accountingRow);

                foreach (var entry in entriesToCreate)
                {
                    if (!entry.AccountDistributionEntryRow.IsNullOrEmpty())
                        _dbUtility.AddObject("AccountDistributionEntry", entry);
                }

            }

            return _dbService.SaveChanges();
        }

        public IEnumerable<AccountDistributionEntry> GenerateAccrualEntries(MinimalAccountingRow accountingRow)
        {
            if (!accountingRow.StartDate.HasValue)
                throw new ArgumentException("Accounting row must have a start date to generate accrual entries.");

            if (accountingRow.NumberOfPeriods < 1)
                throw new ArgumentException("Accounting row must have a valid number of periods to generate accrual entries.");

            DateTime startDate = accountingRow.StartDate.Value;
            int nbrOfPeriods = accountingRow.NumberOfPeriods;

            var accrualEntries = new List<AccountDistributionEntry>();

            for (int i = 0; i < nbrOfPeriods; i++)
            {
                var accrualPeriod = startDate.AddMonths(i);
                var accrualStartDate = AccountDistributionUtility.GetEntryDate(startDate.Day, accrualPeriod);
                int accrualAccountId = GetAccrualAccountId(accountingRow.AccountId);

                var accrualEntry = _entryBuilder.BuildAccountingRowBasedEntry(
                    accountingRow,
                    accrualStartDate,
                    _params.RegistrationType,
                    accrualAccountId,
                    _params.SourceId,
                    _params.AccrualName
                    );


                if (accrualEntry.IsValidForCreation())
                {
                    var adEntry = _converter.CreateADEntry(accrualEntry);
                    accrualEntries.Add(adEntry);
                }
            }

            return accrualEntries;
        }
        private int GetAccrualAccountId(int accountId)
        {
            var accrualAccount = _params.AccrualAccountMappings
                                    .Find(map => map.Account.AccountId == accountId);

            if (accrualAccount != null)
            {
                return accrualAccount.Account1.AccountId;
            }

            var accountingRowAccount = _accountService.GetAccount(_params.ActorCompanyId, accountId);

            bool isCostAccount = accountingRowAccount.AccountStd.AccountTypeSysTermId
                                 == (int)TermGroup_AccountType.Cost;

            return isCostAccount
                ? _params.DefaultAccrualCostAccountId
                : _params.DefaultAccrualRevenueAccountId;
        }

    }
    
}
