using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SoftOne.Soe.Data
{
    public partial class Purchase : ICreatedModifiedNotNull, IState
    {
        public string StatusName { get; set; }
    }

    public partial class PurchaseRow : ICreatedModifiedNotNull, IState
    {
        public int TempRowId { get; set; }
        public string StatusName { get; set; }
        public string ModifiedBy
        {
            get
            {
                return this.ModifedBy;
            }
            set
            {
                this.ModifedBy = value;
            }
        }
    }

    public partial class PurchaseDelivery : ICreatedModifiedNotNull, IState
    {

    }

    public partial class PurchaseDeliveryRow : ICreatedModifiedNotNull, IState
    {

    }

    public static partial class EntityExtensions
    {
        public readonly static Expression<Func<PurchaseDelivery, PurchaseDeliveryDTO>> GetPurchaseDeliveryDTO =
            p => new PurchaseDeliveryDTO
            {
                PurchaseDeliveryId = p.PurchaseDeliveryId,
                DeliveryNr = p.DeliveryNr,
                DeliveryDate = p.DeliveryDate,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier.Name,
                SupplierNr = p.Supplier.SupplierNr,
                Created = p.Created,
                PurchaseNr = p.PurchaseDeliveryRow.Select(r => r.PurchaseRow.Purchase.PurchaseNr).FirstOrDefault()
            };

        public static PurchaseDeliveryDTO ToDTO(this PurchaseDelivery p)
        {
            var dto = new PurchaseDeliveryDTO
            {
                PurchaseDeliveryId = p.PurchaseDeliveryId,
                DeliveryNr = p.DeliveryNr,
                DeliveryDate = p.DeliveryDate,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier?.Name,
                SupplierNr = p.Supplier?.SupplierNr,
            };

            return dto;
        }

        public static List<PurchaseRowDTO> ToDTOs(this IEnumerable<PurchaseRow> l)
        {
            var dtos = new List<PurchaseRowDTO>();
            if (l != null)
            {
                foreach (var p in l)
                {
                    dtos.Add(p.ToDTO());
                }
            }
            return dtos;
        }

        public static List<PurchaseRowSmallDTO> ToSmallDTOs(this IEnumerable<PurchaseRow> l)
        {
            var dtos = new List<PurchaseRowSmallDTO>();
            if (l != null)
            {
                foreach (var p in l)
                {
                    dtos.Add(p.ToSmallDTO());
                }
            }
            return dtos;
        }
        public static PurchaseRowSmallDTO ToSmallDTO(this PurchaseRow e)
        {
            var dto = new PurchaseRowSmallDTO
            {
                PurchaseRowId = e.PurchaseRowId,
                PurchaseRowNr = e.RowNr,
                ProductId = e.ProductId,
                ProductNumber = e.Product == null ? string.Empty : e.Product.Number,
                DeliveredQuantity = e.DeliveredQuantity,
                Text = e.Text,
                ProductName = e.Product == null ? string.Empty : e.Product.Name
            };

            return dto;
        }

        public static PurchaseRowDTO ToDTO(this PurchaseRow e)
        {
            var dto = new PurchaseRowDTO
            {
                PurchaseRowId = e.PurchaseRowId,
                ProductId = e.ProductId,
                Quantity = e.Quantity,
                DeliveredQuantity = e.DeliveredQuantity,
                DeliveryDate = e.DeliveryDate,
                PurchasePriceCurrency = e.PurchasePriceCurrency,
                VatAmountCurrency = e.VatAmountCurrency,

                VatRate = e.VatRate,
                VatCodeId = e.VatCodeId,
                RowNr = e.RowNr,
                AccDeliveryDate = e.AccDeliveryDate,
                PurchaseUnitId = e.PurchaseUnitId,
                StockId = e.StockId,
                StockCode = e.Stock?.Code,
                Text = e.Text,
                WantedDeliveryDate = e.WantedDeliveryDate,

                OrderId = e.OrderId,
                OrderNr = e.CustomerInvoice?.InvoiceNr,

                DiscountType = e.DiscountType,
                DiscountAmount = e.DiscountAmount,
                DiscountAmountCurrency = e.DiscountAmountCurrency,
                DiscountPercent = e.DiscountPercent,

                Type = (PurchaseRowType)e.Type,
                State = (SoeEntityState)e.State,

                IntrastatTransactionId = e.IntrastatTransactionId
            };

            return dto;
        }
        public static PurchaseDTO ToDTO(this Purchase e)
        {
            if (e == null)
                return null;

            var dto = new PurchaseDTO
            {
                ContactEComId = e.ContactEComId,
                PurchaseId = e.PurchaseId,
                PurchaseDate = e.PurchaseDate,
                PurchaseNr = e.PurchaseNr,
                PurchaseLabel = e.PurchaseLabel,

                CurrencyId = e.CurrencyId,
                CurrencyRate = e.CurrencyRate,
                CurrencyDate = e.CurrencyDate,
                DeliveryTypeId = e.DeliveryTypeId,
                DeliveryConditionId = e.DeliveryConditionId,
                DeliveryAddressId = e.DeliveryAddressId,
                DeliveryAddress = e.DeliveryAddress,
                VatType = e.VatType,
                DefaultDim1AccountId = e.DefaultDim1AccountId,
                DefaultDim2AccountId = e.DefaultDim2AccountId,
                DefaultDim3AccountId = e.DefaultDim3AccountId,
                DefaultDim4AccountId = e.DefaultDim4AccountId,
                DefaultDim5AccountId = e.DefaultDim5AccountId,
                DefaultDim6AccountId = e.DefaultDim6AccountId,
                Origindescription = e.Origin?.Description,
                ProjectId = e.ProjectId,
                ProjectNr = e.Project?.Number,
                OrderId = e.OrderId,
                OrderNr = e.CustomerInvoice?.InvoiceNr,
                PaymentConditionId = e.PaymentConditionId,
                SupplierId = e.SupplierId,
                SupplierEmail = e.SupplierEmail,
                SupplierCustomerNr = e.Supplier?.OurCustomerNr,
                ReferenceOur = e.ReferenceOur,
                ReferenceYour = e.ReferenceYour,

                WantedDeliveryDate = e.WantedDeliveryDate,
                ConfirmedDeliveryDate = e.ConfirmedDeliveryDate,
                StockId = e.StockId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,

                OriginStatus = e.Origin == null ? SoeOriginStatus.None : (SoeOriginStatus)e.Origin.Status,
                OriginUsers = new List<OriginUserSmallDTO>(),
                StatusName = e.StatusName,
                TotalAmountCurrency = e.TotalAmountCurrency,
                TotalAmountExVatCurrency = e.TotalAmountCurrency - e.VATAmountCurrency,
                VatAmountCurrency = e.VATAmountCurrency,
            };

            if (e.Origin != null && e.Origin.OriginUser != null)
            {
                foreach (var user in e.Origin.OriginUser.Where(u => u.State == (int)SoeEntityState.Active).OrderByDescending(u => u.Main).ThenBy(u => u.User.Name))
                {
                    dto.OriginUsers.Add(user.ToSmallDTO());
                }
            }

            return dto;
        }
        public static PurchaseSmallDTO ToPurchaseSmallDTO(this Purchase e)
        {
            if (e == null)
                return null;

            var dto = new PurchaseSmallDTO
            {
                PurchaseId = e.PurchaseId,
                PurchaseNr = e.PurchaseNr,
                SupplierId = e.SupplierId,
                SupplierNr = e.Supplier.SupplierNr,
                SupplierName = e.Supplier.Name,
                OriginDescription = e.Origin == null ? string.Empty :( !string.IsNullOrEmpty(e.Origin.Description) ? e.Origin.Description : ""),
                Status = e.Origin.Status,
            };
            return dto;
        }

        public static List<PurchaseSmallDTO> ToPurchaseSmallDTOs(this IEnumerable<Purchase> l)
        {
            var dtos = new List<PurchaseSmallDTO>();
            if (l != null)
            {
                foreach (var p in l)
                {
                    dtos.Add(p.ToPurchaseSmallDTO());
                }
            }
            return dtos;
        }

        public static PurchaseDeliveryInvoiceDTO ToDTO(this PurchaseDeliveryInvoice e) {
            if (e == null)
                return null;

            var dto = new PurchaseDeliveryInvoiceDTO
            {
                PurchaseDeliveryInvoiceId = e.PurchaseDeliveryInvoiceId,
                SupplierinvoiceId = e.SupplierinvoiceId,
                PurchaseRowId = e.PurchaseRowId,
                //PurchaseProduct = string.Format("{0} {1}",e.PurchaseRow.RowNr,e.PurchaseRow.Product.Number),
                PurchaseId = e.PurchaseRow.PurchaseId,
                PurchaseNr = e.PurchaseRow.Purchase.PurchaseNr,
                DeliveredQuantity = e.PurchaseRow.DeliveredQuantity.HasValue? e.PurchaseRow.DeliveredQuantity.Value : decimal.Zero,
                Price = e.Price,
                Quantity = e.Quantity,
                //ArticleName= e.PurchaseRow.Product.Name
            };
            return dto;
        }

        public static List<PurchaseDeliveryInvoiceDTO> ToDTOs(this IEnumerable<PurchaseDeliveryInvoice> l)
        {
            var dtos = new List<PurchaseDeliveryInvoiceDTO>();
            if (l != null)
            {
                foreach (var p in l)
                {
                    dtos.Add(p.ToDTO());
                }
            }
            return dtos;
        }

    }
}
