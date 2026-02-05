using SoftOne.Soe.Business.Core.ManagerWrappers;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.AccountDistribution.Accrual
{
    internal class AccrualConverter
    {
        private int _actorCompanyId;
        private IAccrualQueryService _queryService;
        private IStateUtility _stateUtility;
        private IAccountDistributionEntryRowCurrencySetter _currencySetter;

        public AccrualConverter(int actorCompanyId, IAccrualQueryService queryService, IStateUtility stateUtility, IAccountDistributionEntryRowCurrencySetter currencySetter)
        {
            _actorCompanyId = actorCompanyId;
            _queryService = queryService;
            _stateUtility = stateUtility;
            _currencySetter = currencySetter;
        }

        public AccountDistributionEntry CreateADEntry(AccrualEntry data)
        {
            var entry = new AccountDistributionEntry
            {
                AccountDistributionHeadId = data.AccountDistributionHeadId,
                TriggerType = (int)data.TriggerType,
                ActorCompanyId = _actorCompanyId,
                Date = data.Date,
                RegistrationType = (int)data.RegistrationType,
                SourceCustomerInvoiceId = data.SourceCustomerInvoiceId,
                SourceSupplierInvoiceId = data.SourceSupplierInvoiceId,
                SourceVoucherHeadId = data.SourceVoucherHeadId,
                SourceRowId = data.SourceRowId,

            };
            _stateUtility.SetCreatedProperties(entry);

            foreach (var row in data.Rows)
            {
                var entryRow = CreateADEntryRow(row);
                entry.AccountDistributionEntryRow.Add(entryRow);
            }

            return entry;
        }

        AccountDistributionEntryRow CreateADEntryRow(AccrualEntryRow row)
        {
            var entryRow = new AccountDistributionEntryRow()
            {
                AccountId = row.AccountId,
                DebitAmount = Math.Max(0, row.Amount),
                CreditAmount = Math.Max(0, -row.Amount),
            };
            _currencySetter.SetCurrencyAmounts(entryRow);

            foreach (var accountId in row.InternalAccountIds)
            {
                var acc = _queryService.GetAccountInternal(accountId);
                if (acc != null)
                    entryRow.AccountInternal.Add(acc);
            }

            return entryRow;
        }
        public void UpdateExistingEntryRows(AccountDistributionEntry existingEntry, List<AccrualEntryRow> newEntryRows)
        {
            foreach (var rowData in newEntryRows)
            {
                var newRowEntry = CreateADEntryRow(rowData);
                existingEntry.AccountDistributionEntryRow.Add(newRowEntry);
            }
        }
    }
}
