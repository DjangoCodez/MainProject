using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        public readonly static Expression<Func<SupplierInvoice, SupplierInvoiceSmallDTO>> GetSupplierInvoiceSmallDTO =
        i => new SupplierInvoiceSmallDTO
        {
            SupplierId = i.ActorId,
            SupplierName = i.Actor.Supplier.Name,
            SupplierNr = i.Actor.Supplier.SupplierNr,
            InvoiceId = i.InvoiceId,
            InvoiceNr = i.InvoiceNr,
            SeqNr = i.SeqNr,
            VoucherHeadId = i.VoucherHeadId,
        };

        public readonly static Expression<Func<SupplierInvoice, InvoiceTinyDTO>> GetSupplierInvoiceTinyDTO =
        i => new InvoiceTinyDTO
        {
            InvoiceId = i.InvoiceId,
            InvoiceNr = i.InvoiceNr,
            SeqNr = i.SeqNr,
        };

        public readonly static Expression<Func<SupplierInvoiceProductRow, SupplierInvoiceOrderGridDTO>> SupplierInvoiceProductRowToSupplierInvoiceOrderGridDTO =
        row => new SupplierInvoiceOrderGridDTO
        {
         CustomerInvoiceId = row.CustomerInvoiceRow.InvoiceId,
         SupplierInvoiceId = row.SupplierInvoiceId,
         CustomerInvoiceRowId = row.CustomerInvoiceRowId,
         BillingType = row.SupplierInvoice != null ? (TermGroup_BillingType)row.SupplierInvoice.BillingType : TermGroup_BillingType.None,
         SupplierNr = row.SupplierInvoice.Actor.Supplier.SupplierNr,
         SupplierName = row.SupplierInvoice.Actor.Supplier.Name,
         InvoiceNr = row.SupplierInvoice.InvoiceNr,
         SeqNr = row.SupplierInvoice.SeqNr,
         Amount = row.CustomerInvoiceRow.SumAmountCurrency,
         InvoiceAmountExVat = row.SupplierInvoice != null ? (row.SupplierInvoice.TotalAmountCurrency - row.SupplierInvoice.VATAmountCurrency) : 0,
         IncludeImageOnInvoice = row.CustomerInvoiceRow.IncludeSupplierInvoiceImage ?? false,
         SupplierInvoiceOrderLinkType = SupplierInvoiceOrderLinkType.LinkToOrder,
         InvoiceDate = row.SupplierInvoice.InvoiceDate,
         CustomerInvoiceRowAttestStateId = row.CustomerInvoiceRow.AttestStateId,
         TargetCustomerInvoiceDate = row.CustomerInvoiceRow.TargetRow.CustomerInvoice.InvoiceDate,
         TargetCustomerInvoiceNr = row.CustomerInvoiceRow.TargetRow.CustomerInvoice.InvoiceNr,
        };

        public readonly static Expression<Func<CustomerInvoiceRow, SupplierInvoiceOrderGridDTO>> CustomerInvoiceRowToSupplierInvoiceOrderGridDTO =
        row => new SupplierInvoiceOrderGridDTO
        {
            CustomerInvoiceId = row.InvoiceId,
            SupplierInvoiceId = row.SupplierInvoiceId ?? 0,
            CustomerInvoiceRowId = row.CustomerInvoiceRowId,
            BillingType = row.SupplierInvoice != null ? (TermGroup_BillingType)row.SupplierInvoice.BillingType : TermGroup_BillingType.None,
            SupplierNr = row.SupplierInvoice.Actor.Supplier.SupplierNr,
            SupplierName = row.SupplierInvoice.Actor.Supplier.Name,
            InvoiceNr = row.SupplierInvoice.InvoiceNr,
            SeqNr = row.SupplierInvoice.SeqNr,
            Amount = row.SumAmountCurrency,
            InvoiceAmountExVat = row.SupplierInvoice != null ? (row.SupplierInvoice.TotalAmountCurrency - row.SupplierInvoice.VATAmountCurrency) : 0,
            IncludeImageOnInvoice = row.IncludeSupplierInvoiceImage ?? false,
            SupplierInvoiceOrderLinkType = SupplierInvoiceOrderLinkType.LinkToOrder,
            InvoiceDate = row.SupplierInvoice.InvoiceDate,
            EdiEntryId = row.EdiEntryId,
            CustomerInvoiceRowAttestStateId = row.AttestStateId,
            TargetCustomerInvoiceDate = row.TargetRow.CustomerInvoice.InvoiceDate,
            TargetCustomerInvoiceNr = row.TargetRow.CustomerInvoice.InvoiceNr,
        };

        public readonly static Expression<Func<TimeCodeTransaction, SupplierInvoiceOrderGridDTO>> TimeCodeTransactionToSupplierInvoiceOrderGridDTO =
        (row) => new SupplierInvoiceOrderGridDTO
        {
            CustomerInvoiceId = row.CustomerInvoiceId ?? 0,
            SupplierInvoiceId = row.SupplierInvoiceId.Value,
            CustomerInvoiceRowId = row.TimeInvoiceTransaction.FirstOrDefault(r => r.State == (int)SoeEntityState.Active && r.CustomerInvoiceRowId != null).CustomerInvoiceRowId,
            BillingType = (TermGroup_BillingType)row.SupplierInvoice.BillingType,
            SupplierNr = row.SupplierInvoice.Actor.Supplier.SupplierNr,
            SupplierName = row.SupplierInvoice.Actor.Supplier.Name,
            InvoiceNr = row.SupplierInvoice.InvoiceNr,
            SeqNr = row.SupplierInvoice.SeqNr,
            Amount = 0,
            InvoiceAmountExVat = (row.SupplierInvoice.TotalAmountCurrency - row.SupplierInvoice.VATAmountCurrency),
            IncludeImageOnInvoice = row.IncludeSupplierInvoiceImage,
            TimeCodeTransactionId = row.TimeCodeTransactionId,
            SupplierInvoiceOrderLinkType = SupplierInvoiceOrderLinkType.LinkToProject,
            InvoiceDate = row.SupplierInvoice.InvoiceDate,
            CustomerInvoiceRowAttestStateId = row.CustomerInvoiceRow.AttestStateId,
            TargetCustomerInvoiceDate = row.CustomerInvoiceRow.TargetRow.CustomerInvoice.InvoiceDate,
            TargetCustomerInvoiceNr = row.CustomerInvoiceRow.TargetRow.CustomerInvoice.InvoiceNr,
        };

        public readonly static Expression<Func<SupplierInvoice, SupplierInvoiceIncomingHallGridDTO>> GetSupplierInvoiceIncomingHallGridDTO =
        (row) => new SupplierInvoiceIncomingHallGridDTO
        {
            InvoiceId = row.InvoiceId,
            InvoiceNr = row.InvoiceNr,
            BillingTypeId = (TermGroup_BillingType)row.BillingType,
            SupplierNr = row.Actor.Supplier.SupplierNr,
            SupplierName = row.Actor.Supplier.Name,
            SupplierId = row.Actor.Supplier.ActorSupplierId,
            TotalAmount = row.TotalAmount,
            TotalAmountCurrency = row.TotalAmountCurrency,
            InternalText = row.Origin.Description,
            InvoiceDate = row.InvoiceDate,
            DueDate = row.DueDate,
            Created = row.Created,

            InvoiceSource = TermGroup_SupplierInvoiceSource.UserEntered,
            InvoiceState = TermGroup_SupplierInvoiceStatus.InProgress,
        };
    }
}
