using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{

    public static partial class EntityExtensions
    {
        #region Stock

        public static StockDTO ToDTO(this Stock e, bool addStockShelfs = false, bool includeAccountSettings = false, List<AccountDim> accountDims = null)
        {
            if (e == null)
                return null;

            var dto = new StockDTO()
            {
                StockId = e.StockId,
                Code = e.Code,
                Name = e.Name,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                IsExternal = e.IsExternal,
                DeliveryAddressId = e.DeliveryAddressId
            };

            if (addStockShelfs)
            {
                dto.StockShelves = new List<StockShelfDTO>();
                if (e.StockShelf != null)
                {
                    foreach (var l in e.StockShelf)
                    {
                        dto.StockShelves.Add(l.ToDTO());
                    }
                }
            }

            if (includeAccountSettings && accountDims!=null)
            {
                dto.AccountingSettings = new List<AccountingSettingsRowDTO>();

                if (e.StockAccountStd != null && e.StockAccountStd.Count > 0)
                {
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockIn, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockInChange, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockOut, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockOutChange, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockInv, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockInvChange, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockLoss, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockLossChange, accountDims);
                    AddAccountingSettingsRowDTO(e, dto, ProductAccountType.StockTransferChange, accountDims);
                }
            }

            return dto;
        }

        public static IEnumerable<StockDTO> ToDTOs(this IEnumerable<Stock> l, bool addCodes = false)
        {
            var dtos = new List<StockDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(addCodes));
                }
            }
            return dtos;
        }

        public static StockGridDTO ToGridDTO(this Stock e)
        {
            if (e == null)
                return null;

            var dto = new StockGridDTO()
            {
                StockId = e.StockId,
                Code = e.Code,
                Name = e.Name,
                IsExternal = e.IsExternal
            };

            return dto;
        }

        public static IEnumerable<StockGridDTO> ToGridDTOs(this IEnumerable<Stock> l)
        {
            var dtos = new List<StockGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }
        public static IEnumerable<SmallGenericType> ToSmallGenericDTOs(this IEnumerable<Stock> l)
        {
            var dtos = new List<SmallGenericType>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(new SmallGenericType(e.StockId,e.Name));
                }
            }
            return dtos;
        }

        

        private static void AddAccountingSettingsRowDTO(Stock stock, StockDTO dto, ProductAccountType type, List<AccountDim> accountDims)
        {
            AccountingSettingsRowDTO accDto = new AccountingSettingsRowDTO()
            {
                Type = (int)type,
                Percent = 0
            };
            dto.AccountingSettings.Add(accDto);

            StockAccountStd accStd = stock.StockAccountStd.FirstOrDefault(c => c.Type == (int)type);
            Account account = accStd?.AccountStd?.Account;
            if (account != null)
            {
                accDto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                accDto.Account1Id = account.AccountId;
                accDto.Account1Nr = account.AccountNr;
                accDto.Account1Name = account.Name;
            }

            if (accStd != null && accStd.AccountInternal != null)
            {               
                foreach (var accInt in accStd.AccountInternal)
                {
                    account = accInt.Account;
                    var position = accountDims.FindIndex(x => x.AccountDimId == account.AccountDim.AccountDimId) + 1;

                    // TODO: Does not support dim numbers 1 and over 6!!!
                    switch (position)
                    {
                        case 2:
                            accDto.AccountDim2Nr = account.AccountDim.AccountDimNr;
                            accDto.Account2Id = account.AccountId;
                            accDto.Account2Nr = account.AccountNr;
                            accDto.Account2Name = account.Name;
                            break;
                        case 3:
                            accDto.AccountDim3Nr = account.AccountDim.AccountDimNr;
                            accDto.Account3Id = account.AccountId;
                            accDto.Account3Nr = account.AccountNr;
                            accDto.Account3Name = account.Name;
                            break;
                        case 4:
                            accDto.AccountDim4Nr = account.AccountDim.AccountDimNr;
                            accDto.Account4Id = account.AccountId;
                            accDto.Account4Nr = account.AccountNr;
                            accDto.Account4Name = account.Name;
                            break;
                        case 5:
                            accDto.AccountDim5Nr = account.AccountDim.AccountDimNr;
                            accDto.Account5Id = account.AccountId;
                            accDto.Account5Nr = account.AccountNr;
                            accDto.Account5Name = account.Name;
                            break;
                        case 6:
                            accDto.AccountDim6Nr = account.AccountDim.AccountDimNr;
                            accDto.Account6Id = account.AccountId;
                            accDto.Account6Nr = account.AccountNr;
                            accDto.Account6Name = account.Name;
                            break;
                    }
                }
            }
        }

        #endregion

        #region StockInventory

        public readonly static Expression<Func<StockInventoryHead, StockInventoryHeadDTO>> StockInventoryHeadDTO =
        e => new StockInventoryHeadDTO
        {
            StockInventoryHeadId = e.StockInventoryHeadId,
            StockId = e.StockId ?? 0,
            StockName = e.Stock.Name,
            StockCode = e.Stock.Code,
            InventoryStart = e.InventoryStart,
            InventoryStop = e.InventoryStop,
            HeaderText = e.HeaderText,
            Created = e.Created,
            CreatedBy = e.CreatedBy,
            Modified = e.Modified,
            ModifiedBy = e.ModifiedBy
        };

        public readonly static Expression<Func<StockInventoryRow, StockInventoryRowDTO>> StockInventoryRowDTO =
        e => new StockInventoryRowDTO
        {
            StockInventoryRowId = e.StockInventoryRowId,
            StockInventoryHeadId = e.StockInventoryHeadId,
            StockProductId = e.StockProductId,
            StartingSaldo = e.StartingSaldo,
            InventorySaldo = e.InventorySaldo,
            Difference = e.Difference,
            Created = e.Created,
            CreatedBy = e.CreatedBy,
            Modified = e.Modified,
            ModifiedBy = e.ModifiedBy,
            OrderedQuantity = e.StockProduct.OrderedQuantity,
            ReservedQuantity = e.StockProduct.ReservedQuantity,
            AvgPrice = e.StockProduct.AvgPrice,
            ShelfId = e.StockProduct.StockShelfId ?? 0,
            ShelfCode = e.StockProduct.StockShelf.Code,
            ShelfName = e.StockProduct.StockShelf.Name,
            ProductName = e.StockProduct.InvoiceProduct.Name,
            ProductNumber = e.StockProduct.InvoiceProduct.Number,
            ProductGroupId = e.StockProduct.InvoiceProduct.ProductGroupId,
            ProductGroupCode = e.StockProduct.InvoiceProduct.ProductGroup.Code,
            ProductGroupName = e.StockProduct.InvoiceProduct.ProductGroup.Name,
            TransactionDate = e.TransactionDate
        };

        public static IEnumerable<StockInventoryGridDTO> ToGridDTOs(this IEnumerable<StockInventoryHead> l)
        {
            var dtos = new List<StockInventoryGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static StockInventoryGridDTO ToGridDTO(this StockInventoryHead e)
        {
            if (e == null)
                return null;

            StockInventoryGridDTO dto = new StockInventoryGridDTO()
            {
                StockInventoryHeadId = e.StockInventoryHeadId,
                HeaderText = e.HeaderText,
                StockName = e.Stock.Name,
                InventoryStart = e.InventoryStart,
                InventoryStop = e.InventoryStop,
                CreatedBy = e.CreatedBy
            };

            return dto;
        }

        #endregion

        #region StockProduct

        public readonly static Expression<Func<StockProduct, ProductSmallDTO>> ProductSmallDTO =
        p => new ProductSmallDTO
        {
            ProductId = p.InvoiceProductId,
            Name = p.InvoiceProduct.Name,
            Number = p.InvoiceProduct.Number,
        };

        public readonly static Expression<Func<StockProduct, StockProductSmallDTO>> StockProductSmallDTO =
        p => new StockProductSmallDTO
        {
            InvoiceProductId = p.InvoiceProductId,
            StockId = p.StockId,
            StockProductId = p.StockProductId,
            StockShelfId = p.StockShelfId,
            Quantity = p.Quantity,
            ProductName = p.InvoiceProduct.Name,
            ProductNumber = p.InvoiceProduct.Number,
        };

        public readonly static Expression<Func<StockProduct, StockProductAvgPriceDTO>> StockProductAvgPriceDTO =
        p => new StockProductAvgPriceDTO
        {
            InvoiceProductId = p.InvoiceProductId,
            StockProductId = p.StockProductId,
            AvgPrice = p.AvgPrice,
        };

        public static Expression<Func<StockProduct, StockProductDTO>> StockProductDTO(int calculationType)
        {
            return (p) => new StockProductDTO
            {
                InvoiceProductId = p.InvoiceProductId,
                StockId = p.StockId,
                StockProductId = p.StockProductId,
                StockShelfId = p.StockShelfId,
                Quantity = p.Quantity,
                OrderedQuantity = p.OrderedQuantity,
                ReservedQuantity = p.ReservedQuantity,
                StockName = p.Stock.Name,
                StockShelfCode = p.StockShelf.Code,
                StockShelfName = p.StockShelf.Name,
                AvgPrice = p.AvgPrice,
                IsInInventory = p.IsInInventory,
                PurchaseQuantity = p.PurchaseQuantity,
                PurchaseTriggerQuantity = p.PurchaseTriggerQuantity,
                ProductName = p.InvoiceProduct.Name,
                ProductNumber = p.InvoiceProduct.Number,
                ProductGroupCode = p.InvoiceProduct.ProductGroup.Code,
                ProductGroupName = p.InvoiceProduct.ProductGroup.Name,
                ProductUnit = p.InvoiceProduct.ProductUnit.Name ?? "",
                ProductGroupId = p.InvoiceProduct.ProductGroupId ?? 0,
                TransactionPrice = ((p.InvoiceProduct.DefaultGrossMarginCalculationType.HasValue && p.InvoiceProduct.DefaultGrossMarginCalculationType == (int)TermGroup_GrossMarginCalculationType.PurchasePrice) || (!p.InvoiceProduct.DefaultGrossMarginCalculationType.HasValue && calculationType == (int)TermGroup_GrossMarginCalculationType.PurchasePrice)) ? p.InvoiceProduct.PurchasePrice : p.AvgPrice,
                DeliveryLeadTimeDays = p.DeliveryLeadTimeDays,
                ProductState = p.InvoiceProduct.State,
            };
        }

        public readonly static Expression<Func<StockProduct, StockProductDTO>> StockProductShelfDTO =
        p => new StockProductDTO
        {
            InvoiceProductId = p.InvoiceProductId,
            StockId = p.StockId,
            StockProductId = p.StockProductId,
            StockShelfId = p.StockShelfId,
            Quantity = p.Quantity,
            OrderedQuantity = p.OrderedQuantity,
            ReservedQuantity = p.ReservedQuantity,
            StockName = p.Stock.Name,
            StockShelfCode = p.StockShelf.Code,
            StockShelfName = p.StockShelf.Name,
            AvgPrice = p.AvgPrice,
            IsInInventory = p.IsInInventory,
            PurchaseQuantity = p.PurchaseQuantity,
            PurchaseTriggerQuantity = p.PurchaseTriggerQuantity,
        };

        public static StockProductDTO ToDTO(this StockProduct e)
        {
            if (e == null)
                return null;

            return new StockProductDTO
            {
                StockProductId = e.StockProductId,
                StockId = e.StockId,
                InvoiceProductId = e.InvoiceProductId,
                Quantity = e.Quantity,
                OrderedQuantity = e.OrderedQuantity,
                ReservedQuantity = e.ReservedQuantity,
                IsInInventory = e.IsInInventory,
                WarningLevel = e.WarningLevel,
                AvgPrice = e.AvgPrice,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                StockShelfId = e.StockShelfId,
                StockValue = e.Quantity * e.AvgPrice,
                StockName = e.Stock?.Name,
                ProductName = e.InvoiceProduct?.Name,
                ProductNumber = e.InvoiceProduct?.Number,
                ProductUnit = e.InvoiceProduct?.ProductUnit?.Name,
                ProductGroupCode = e.InvoiceProduct?.ProductGroup?.Code ?? "",
            };
        }

        public static IEnumerable<StockProductDTO> ToDTOs(this IEnumerable<StockProduct> l)
        {
            var dtos = new List<StockProductDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region StockShelf

        public static StockShelfDTO ToDTO(this StockShelf e)
        {
            if (e == null)
                return null;

            StockShelfDTO dto = new StockShelfDTO()
            {
                StockId = e.StockId,
                Code = e.Code,
                Name = e.Name,
                StockShelfId = e.StockShelfId,
                StockName = e.Stock == null ? "" : e.Stock.Name,
                IsDelete = false
            };

            return dto;
        }

        public static IEnumerable<StockShelfDTO> ToDTOs(this IEnumerable<StockShelf> l)
        {
            var dtos = new List<StockShelfDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region StockTransaction

        public readonly static Expression<Func<StockTransaction, StockTransactionDTO>> StockTransactionDTO =
        e => new StockTransactionDTO
        {
            StockTransactionId = e.StockTransactionId,
            StockProductId = e.StockProductId,
            InvoiceRowId = e.InvoiceRowId,
            ActionType = (TermGroup_StockTransactionType)e.ActionType,
            Quantity = e.Quantity,
            Price = e.Price,
            AvgPrice = e.AvgPrice,
            Note = e.Note,
            Created = e.Created,
            CreatedBy = e.CreatedBy,
            TransactionDate = e.TransactionDate,
            VoucherId = e.VoucherId,
            VoucherNr = e.VoucherHead.VoucherNr.ToString(),
            StockInventoryHeadId = e.StockInventoryRow.StockInventoryHeadId,
            PurchaseId = e.PurchaseDeliveryRow.PurchaseRow.Purchase.PurchaseId,
            InvoiceId = e.CustomerInvoiceRow.InvoiceId,
        };

        public readonly static Expression<Func<StockTransaction, StockTransactionExDTO>> StockTransactionExDTO =
        e => new StockTransactionExDTO
        {
            StockTransactionId = e.StockTransactionId,
            StockProductId = e.StockProductId,
            InvoiceRowId = e.InvoiceRowId,
            ActionType = (TermGroup_StockTransactionType)e.ActionType,
            Quantity = e.Quantity,
            Price = e.Price,
            AvgPrice = e.AvgPrice,
            Note = e.Note,
            Created = e.Created,
            CreatedBy = e.CreatedBy,
            TransactionDate = e.TransactionDate,
            VoucherId = e.VoucherId,
            VoucherNr = e.VoucherHead.VoucherNr.ToString(),
            StockInventoryHeadId = e.StockInventoryRow.StockInventoryHeadId,
            StockInventoryNr = e.StockInventoryRow.StockInventoryHead.HeaderText,
            PurchaseId = e.PurchaseDeliveryRow.PurchaseRow.Purchase.PurchaseId,
            DeliveryNr = e.PurchaseDeliveryRow.PurchaseDelivery.DeliveryNr,
            InvoiceId = e.CustomerInvoiceRow.InvoiceId,
            InvoiceNr = e.CustomerInvoiceRow.CustomerInvoice.InvoiceNr,
            OriginType = e.CustomerInvoiceRow.CustomerInvoice.Origin.Type,
            PurchaseNr = e.PurchaseDeliveryRow.PurchaseRow.Purchase.PurchaseNr,
            ChildStockTransaction = e.StockTransaction1.FirstOrDefault().StockProduct.Stock.Name
        };

        public readonly static Expression<Func<StockTransaction, StockTransactionSmallDTO>> StockTransactionSmallDTO =
        e => new StockTransactionSmallDTO
        {
            StockTransactionId = e.StockTransactionId,
            StockProductId = e.StockProductId,
            ActionType = (TermGroup_StockTransactionType)e.ActionType,
            Quantity = e.Quantity,
            Price = e.Price,
            TransactionDate = e.TransactionDate,
            AvgPrice = e.AvgPrice
        };

        #endregion
    }
}
