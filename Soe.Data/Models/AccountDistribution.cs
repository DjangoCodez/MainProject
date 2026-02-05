using SoftOne.Soe.Common.Util;
using System;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        public static void ReverseAccounting(this AccountDistributionEntry entry)
        {
            foreach (var row in entry.AccountDistributionEntryRow)
                row.ReverseAccounting();
        }

        public static void ReverseAccounting(this AccountDistributionEntryRow row)
        {
            if (row.DebitAmount != 0)
            {
                row.CreditAmount = Math.Abs(row.DebitAmount);
                row.DebitAmount = 0;
                row.CreditAmountCurrency = Math.Abs(row.DebitAmountCurrency);
                row.DebitAmountCurrency = 0;
                row.CreditAmountEntCurrency = Math.Abs(row.DebitAmountEntCurrency);
                row.DebitAmountEntCurrency = 0;
                row.CreditAmountLedgerCurrency = Math.Abs(row.DebitAmountLedgerCurrency);
                row.DebitAmountLedgerCurrency = 0;
            }
            else
            {
                row.DebitAmount = Math.Abs(row.CreditAmount);
                row.CreditAmount = 0;
                row.DebitAmountCurrency = Math.Abs(row.CreditAmountCurrency);
                row.CreditAmountCurrency = 0;
                row.DebitAmountEntCurrency = Math.Abs(row.CreditAmountEntCurrency);
                row.CreditAmountEntCurrency = 0;
                row.DebitAmountLedgerCurrency = Math.Abs(row.CreditAmountLedgerCurrency);
                row.CreditAmountLedgerCurrency = 0;
            }
        }

        public static AccountDistributionEntry Copy(this AccountDistributionEntry original)
        {
            var copy = new AccountDistributionEntry()
            {
                TriggerType = original.TriggerType,
                Date = original.Date,
                State = original.State,
                RegistrationType = original.RegistrationType,
                SourceCustomerInvoiceId = original.SourceCustomerInvoiceId,
                SourceSupplierInvoiceId = original.SourceSupplierInvoiceId,
                SourceVoucherHeadId = original.SourceVoucherHeadId,
                SourceRowId = original.SourceRowId, 
                SupplierInvoiceId = original.SupplierInvoiceId,
                InventoryId = original.InventoryId,
                ActorCompanyId = original.ActorCompanyId,
            };

            if (original.AccountDistributionEntryRow != null)
            {
                foreach (var row in original.AccountDistributionEntryRow)
                {
                    var copyRow = row.Copy();
                    copy.AccountDistributionEntryRow.Add(copyRow);
                }
            }

            return copy;
        }

        public static AccountDistributionEntryRow Copy(this AccountDistributionEntryRow original)
        {
            var copy = new AccountDistributionEntryRow()
            {
                AccountId = original.AccountId,
                DebitAmount = original.DebitAmount,
                CreditAmount = original.CreditAmount,
                DebitAmountCurrency = original.DebitAmountCurrency,
                CreditAmountCurrency = original.CreditAmountCurrency,
                DebitAmountEntCurrency = original.DebitAmountEntCurrency,
                CreditAmountEntCurrency = original.CreditAmountEntCurrency,
                DebitAmountLedgerCurrency = original.DebitAmountLedgerCurrency,
                CreditAmountLedgerCurrency = original.CreditAmountLedgerCurrency,
            };

            if (original.AccountInternal != null) 
            { 
                foreach (var accInternal in original.AccountInternal)
                {
                    copy.AccountInternal.Add(accInternal);
                }
            }

            return copy;
        }

        public static TermGroup_AccountDistributionCalculationType GetCalculationType(this AccountDistributionHead head)
        {
            return (TermGroup_AccountDistributionCalculationType)head.CalculationType;
        }
        public static TermGroup_AccountDistributionPeriodType GetPeriodType(this AccountDistributionHead head)
        {
            return (TermGroup_AccountDistributionPeriodType)head.CalculationType;
        }
    }
}
