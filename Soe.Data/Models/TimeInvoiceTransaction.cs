using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class TimeInvoiceTransaction : ICreatedModified, IState, IModifiedWithNoCheckes
    {
        
    }

    public static partial class EntityExtensions
    {
        #region TimeInvoiceTransaction

        public static TimeInvoiceTransactionDTO ToDTO(this TimeInvoiceTransaction e)
        {
            if (e == null)
                return null;

            TimeInvoiceTransactionDTO dto = new TimeInvoiceTransactionDTO()
            {
                TimeInvoiceTransactionId = e.TimeInvoiceTransactionId,
                TimeCodeTransactionId = e.TimeCodeTransactionId,
                TimeBlockId = e.TimeBlockId,
                EmployeeId = e.EmployeeId,
                TimeBlockDateId = e.TimeBlockDateId,
                InvoiceProductId = e.ProductId,
                AccountId = e.AccountStdId,
                AttestStateId = e.AttestStateId,
                CustomerInvoiceRowId = e.CustomerInvoiceRowId,
                Amount = e.Amount ?? 0,
                AmountCurrency = e.AmountCurrency ?? 0,
                AmountEntCurrency = e.AmountEntCurrency ?? 0,
                AmountLedgerCurrency = e.AmountLedgerCurrency ?? 0,
                VatAmount = e.VatAmount ?? 0,
                VatAmountCurrency = e.VatAmountCurrency ?? 0,
                VatAmountEntCurrency = e.VatAmountEntCurrency ?? 0,
                VatAmountLedgerCurrency = e.VatAmountLedgerCurrency ?? 0,
                Quantity = e.Quantity,
                InvoiceQuantity = e.InvoiceQuantity,
                Invoice = e.Invoice,
                ManuallyAdded = e.ManuallyAdded,
                Exported = e.Exported,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            dto.AttestState = e.AttestState?.ToDTO();
            dto.AccountStd = e.AccountStd?.Account?.ToDTO();
            dto.AccountInternals = e.AccountInternal?.ToDTOs();

            return dto;
        }

        public static IEnumerable<TimeInvoiceTransactionDTO> ToDTOs(this IEnumerable<TimeInvoiceTransaction> l)
        {
            var dtos = new List<TimeInvoiceTransactionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool HasAccountStd(this TimeInvoiceTransaction e)
        {
            return e.AccountStdId > 0 || e.AccountStd != null;
        }

        public static bool HasAccountInternals(this TimeInvoiceTransaction e)
        {
            return !e.AccountInternal.IsNullOrEmpty();
        }

        #endregion
    }
}
