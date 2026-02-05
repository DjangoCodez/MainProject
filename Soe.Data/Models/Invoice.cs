using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Linq.Expressions;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        public readonly static Expression<Func<Invoice, CustomerInvoiceAmountDTO>> GetSupplierInvoiceAmountDTO =
         i => new CustomerInvoiceAmountDTO
         {
             InvoiceId = i.InvoiceId,
             ActorId = i.ActorId ?? 0,
             ActorName = i.Actor.Supplier.Name,
             ActorNr = i.Actor.Supplier.SupplierNr,
             InvoiceNr = i.InvoiceNr,
             SeqNr = i.SeqNr,
             TotalAmount = i.TotalAmount,
             PaidAmount = i.PaidAmount,
             DueDate = i.DueDate,
             InvoiceDate = i.InvoiceDate,
             FullyPayed = i.FullyPayed,
             CurrencyId = i.CurrencyId,
             TotalAmountCurrency = i.TotalAmountCurrency,
             PaidAmountCurrency = i.PaidAmountCurrency,
         };

        public readonly static Expression<Func<Invoice, CustomerInvoiceAmountDTO>> GetCustomerInvoiceAmountDTO =
         i => new CustomerInvoiceAmountDTO
         {
             InvoiceId = i.InvoiceId,
             ActorId = i.ActorId ?? 0,
             ActorName = i.Actor.Customer.Name,
             ActorNr = i.Actor.Customer.CustomerNr,
             InvoiceNr = i.InvoiceNr,
             SeqNr = i.SeqNr,
             TotalAmount = i.TotalAmount,
             PaidAmount = i.PaidAmount,
             DueDate = i.DueDate,
             InvoiceDate = i.InvoiceDate,
             FullyPayed = i.FullyPayed,
             CurrencyId = i.CurrencyId,
             TotalAmountCurrency = i.TotalAmountCurrency,
             PaidAmountCurrency = i.PaidAmountCurrency,
         };

        public readonly static Expression<Func<CustomerInvoice, CustomerInvoiceDistributionDTO>> GetCustomerInvoiceDistributionDTO =
            i => new CustomerInvoiceDistributionDTO
            {
                InvoiceId = i.InvoiceId,
                ActorId = i.ActorId ?? 0,
                ActorName = i.Actor.Customer.Name,
                ActorNr = i.Actor.Customer.CustomerNr,
                ActorOrgNr = i.Actor.Customer.OrgNr ?? string.Empty,
                ActorVatNr = i.Actor.Customer.VatNr ?? string.Empty,
                ActorSupplierNr = i.Actor.Customer.SupplierNr,
                InvoiceNr = i.InvoiceNr,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                DueDate = i.DueDate,
                InvoiceDate = i.InvoiceDate,
                FullyPayed = i.FullyPayed,
                SysCurrencyId = i.Currency.SysCurrencyId,
                ActorSysCountryId = i.Actor.Customer.SysCountryId,
                ActorSysLanguageId = i.Actor.Customer.SysLanguageId,
                ContactEComId = i.ContactEComId,
                InvoiceDeliveryType = i.InvoiceDeliveryType,
                ReferenceYour = i.ReferenceYour,
                ReferenceOur = i.ReferenceOur,
                WorkingDescription = i.WorkingDescription,
                ShowWorkingDescription = i.IncludeOnInvoice,
                InvoiceDeliveryProvider = i.InvoiceDeliveryProvider,
                IncludeInvoicedTime = i.IncludeOnlyInvoicedTime,
                PaymentConditionCode = i.PaymentCondition.Code,
                PaymentConditionDays = i.PaymentCondition.Days,
                BillingType = i.BillingType,
                BillingAddressId = i.BillingAddressId,
                InternalDescription = i.Origin.Description,
                VatType = i.VatType,
                OCR = i.OCR,
                InvoiceText = i.InvoiceText,
                InvoiceHeadText = i.InvoiceHeadText,
                InvoiceLabel = i.InvoiceLabel,
                Freight = i.FreightAmountCurrency,
                InvoiceFee = i.InvoiceFeeCurrency,
                ExportStatus = i.ExportStatus,
            };

        public static IEnumerable<InvoiceDTO> ToDTOs(this IEnumerable<Invoice> l, bool includeOrigin)
        {
            var dtos = new List<InvoiceDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeOrigin));
                }
            }
            return dtos;
        }

        public static InvoiceDTO ToDTO(this Invoice e, bool includeOrigin)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeOrigin)
                    {
                        if (!e.OriginReference.IsLoaded)
                        {
                            e.OriginReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.OriginReference");
                        }
                        if (e.Origin != null && !e.Origin.OriginUser.IsLoaded)
                        {
                            e.Origin.OriginUser.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.Origin.OriginUser");
                        }
                    }

                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            InvoiceDTO dto = new InvoiceDTO()
            {
                InvoiceId = e.InvoiceId,
                ActorId = e.ActorId,
                ContactEComId = e.ContactEComId,
                ContactGLNId = e.ContactGLNId,
                VoucheHeadId = e.VoucherHeadId,
                VoucheHead2Id = e.VoucherHead2Id,
                SysPaymentTypeId = e.SysPaymentTypeId,
                ProjectId = e.ProjectId,
                VatCodeId = e.VatCodeId,
                DeliveryCustomerId = e.DeliveryCustomerId,
                Type = (SoeInvoiceType)e.Type,
                BillingType = (TermGroup_BillingType)e.BillingType,
                VatType = e.VatType == 2 ? TermGroup_InvoiceVatType.Merchandise : (TermGroup_InvoiceVatType)e.VatType,
                InvoiceNr = e.InvoiceNr,
                SeqNr = e.SeqNr,
                OCR = e.OCR,
                CurrencyId = e.CurrencyId,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,
                InvoiceDate = e.InvoiceDate,
                DueDate = e.DueDate,
                TimeDiscountDate = e.TimeDiscountDate,
                TimeDiscountPercent = e.TimeDiscountPercent,
                VoucherDate = e.VoucherDate,
                ReferenceOur = e.ReferenceOur,
                ReferenceYour = e.ReferenceYour,
                TotalAmount = e.TotalAmount,
                TotalAmountCurrency = e.TotalAmountCurrency,
                TotalAmountEntCurrency = e.TotalAmountEntCurrency,
                TotalAmountLedgerCurrency = e.TotalAmountLedgerCurrency,
                VatAmount = e.VATAmount,
                VatAmountCurrency = e.VATAmountCurrency,
                VatAmountEntCurrency = e.VATAmountEntCurrency,
                VatAmountLedgerCurrency = e.VATAmountLedgerCurrency,
                PaidAmount = e.PaidAmount,
                PaidAmountCurrency = e.PaidAmountCurrency,
                PaidAmountEntCurrency = e.PaidAmountEntCurrency,
                PaidAmountLedgerCurrency = e.PaidAmountLedgerCurrency,
                RemainingAmount = e.RemainingAmount,
                RemainingAmountExVat = e.RemainingAmountExVat,

                FullyPayed = e.FullyPayed,
                OnlyPayment = e.OnlyPayment,
                PaymentNr = e.PaymentNr,
                ManuallyAdjustedAccounting = e.ManuallyAdjustedAccounting,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                StatusIcon = (SoeStatusIcon)e.StatusIcon,
                IsTemplate = e.IsTemplate,
                DefaultDim1AccountId = e.DefaultDim1AccountId,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId,
            };

            // Extensions
            dto.OriginStatusName = e.StatusName;
            if (includeOrigin && e.Origin != null)
            {
                dto.OriginStatus = (SoeOriginStatus)e.Origin.Status;
                dto.OriginDescription = e.Origin.Description;
                dto.VoucherSeriesId = e.Origin.VoucherSeriesId;
                dto.VoucherSeriesTypeId = e.Origin.VoucherSeriesTypeId.GetValueOrDefault(0);

                dto.OriginUsers = new List<OriginUserDTO>();
                if (e.Origin.OriginUser != null)
                {
                    foreach (var user in e.Origin.OriginUser.OrderByDescending(u => u.Main).ThenBy(u => u.User.Name))
                    {
                        dto.OriginUsers.Add(user.ToDTO());
                    }
                }
            }
            else
                dto.OriginStatus = SoeOriginStatus.None;

            return dto;
        }
    }
}
