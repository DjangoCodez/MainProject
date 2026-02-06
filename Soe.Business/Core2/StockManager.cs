using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class StockManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public StockManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Stock

        public Stock GetStock(int stockId, bool includeAccounts = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Stock.NoTracking();
            return GetStock(entities, stockId, includeAccounts);
        }

        public Stock GetStock(CompEntities entities, int stockId, bool includeAccounts = false)
        {
            if (stockId == 0)
                return null;

            var query = (from a in entities.Stock
                         where a.StockId == stockId && a.ActorCompanyId == ActorCompanyId
                         select a);

            query = query.Include("StockShelf");

            if (includeAccounts)
            {
                query = query.Include("StockAccountStd.AccountStd.Account");
                query = query.Include("StockAccountStd.AccountInternal.Account.AccountDim");
            }

            return query.FirstOrDefault();
        }

        public StockDTO GetStockDTO(int actorCompanyId, int stockId, bool includeShelves = false, bool includeAccounts = false)
        {
            var stock = this.GetStock(stockId, includeAccounts);

            return stock.ToDTO(includeShelves, includeAccounts, includeAccounts ? AccountManager.GetAccountDimsByCompany(actorCompanyId) : null);
        }

        public Stock GetStockByCode(int actorCompanyId, string code)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Stock.NoTracking();
            return GetStockByCode(entities, actorCompanyId, code);
        }

        public Stock GetStockByCode(CompEntities entities, int actorCompanyId, string code)
        {
            return (from a in entities.Stock
                    where a.Code.ToLower() == code.ToLower() &&
                    a.ActorCompanyId == actorCompanyId && a.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault();
        }

        public List<Stock> GetStocks(int actorCompanyId, int? stockId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Stock.NoTracking();
            return GetStocks(entities, actorCompanyId, stockId);
        }

        public List<Stock> GetStocks(CompEntities entities, int actorCompanyId, int? stockId = null)
        {
            IQueryable<Stock> query = (from s in entities.Stock
                                       where s.ActorCompanyId == actorCompanyId &&
                                       s.State == (int)SoeEntityState.Active
                                       select s);

            query = query.Include("StockShelf");
            if (stockId.HasValue)
            {
                query = query.Where(x => x.StockId == stockId);
            }

            return query.ToList();
        }

        public Dictionary<int, string> GetStocksDict(int actorCompanyId, bool addEmptyRow, bool? sort = true)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            var stocks = GetStocks(actorCompanyId);

            if (sort.HasValue && sort.Value)
                stocks = stocks.OrderBy(x => x.Name).ToList();

            foreach (Stock st in stocks)
            {
                dict.Add(st.StockId, st.Name);
            }

            return dict;
        }

        public Dictionary<int, string> GetStockShelfsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<StockShelfDTO> shelfs = GetStockShelfDTOs(actorCompanyId, 0);
            foreach (StockShelfDTO sh in shelfs)
            {
                dict.Add(sh.StockShelfId, sh.Name);
            }

            return dict;
        }

        public List<StockDTO> GetStocksForInvoiceProduct(int actorCompanyId, int invoiceProductId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetStocksForInvoiceProduct(entities, actorCompanyId, invoiceProductId);
        }

        public List<StockDTO> GetStocksForInvoiceProduct(CompEntities entities, int actorCompanyId, int invoiceProductId)
        {
            /* TO TEST!
            var stockDto = (from sp in entities.StockProduct
                                              where sp.ActorCompanyId == actorCompanyId &&
                                              sp.InvoiceProductId == invoiceProductId
                                              select sp).Select(s =>  new StockDTO { 
                                                  StockId  = s.StockId,
                                                  AvgPrice = s.AvgPrice,
                                                  StockProductId = s.StockProductId,
                                                  StockShelfId = s.StockShelfId ?? 0,
                                                  PurchaseTriggerQuantity = s.PurchaseTriggerQuantity,
                                                  PurchaseQuantity = s.PurchaseQuantity,
                                                  DeliveryLeadTimeDays = s.DeliveryLeadTimeDays,

                                                  //Show only free saldo
                                                  Saldo = (int)s.ReservedQuantity > 0 ? (int)s.Quantity - (int)s.ReservedQuantity : (int)s.Quantity,

                                                  StockShelfName = s.StockShelf.Name,

                                                  Code = s.Stock.Code,
                                                  Name = s.Stock.Name,
                                                  Created = s.Stock.Created,
                                                  CreatedBy = s.Stock.CreatedBy,
                                                  Modified = s.Stock.Modified,
                                                  ModifiedBy = s.Stock.ModifiedBy,
                                                  State = (SoeEntityState)s.Stock.State,
                                                  IsExternal = s.Stock.IsExternal,
                                                  DeliveryAddressId = s.Stock.DeliveryAddressId
                                              }).ToList();

            */

            //all stocks
            List<Stock> stocks = GetStocks(entities, actorCompanyId);

            //stockIds for certain invoiceproductId            
            List<int> stockIdsProduct = (from sp in entities.StockProduct
                                         where sp.ActorCompanyId == actorCompanyId &&
                                         sp.InvoiceProductId == invoiceProductId
                                         select sp.StockId).Distinct().ToList();

            List<StockDTO> dtos = stocks.Where(a => stockIdsProduct.Contains(a.StockId)).ToDTOs().ToList();

            if (!dtos.IsNullOrEmpty())
            {
                foreach (StockDTO dto in dtos)
                {
                    StockProduct stockProduct = (from sp in entities.StockProduct
                                                 where sp.InvoiceProductId == invoiceProductId &&
                                                 sp.StockId == dto.StockId
                                                 select sp).FirstOrDefault();

                    if (stockProduct != null)
                    {
                        dto.AvgPrice = stockProduct.AvgPrice;
                        dto.StockShelfId = stockProduct.StockShelfId.GetValueOrDefault();
                        dto.StockProductId = stockProduct.StockProductId;

                        dto.PurchaseTriggerQuantity = stockProduct.PurchaseTriggerQuantity;
                        dto.PurchaseQuantity = stockProduct.PurchaseQuantity;
                        dto.DeliveryLeadTimeDays = stockProduct.DeliveryLeadTimeDays;

                        //Show only free saldo
                        if (stockProduct.ReservedQuantity > 0)
                            dto.Saldo = (int)stockProduct.Quantity - (int)stockProduct.ReservedQuantity;
                        else
                            dto.Saldo = (int)stockProduct.Quantity;
                    }

                    StockShelf stockShelf = (from ss in entities.StockShelf
                                             where ss.StockShelfId == stockProduct.StockShelfId
                                             select ss).FirstOrDefault();

                    if (stockShelf != null)
                    {
                        dto.StockShelfName = stockShelf.Name;
                    }
                }
            }

            return dtos;
        }

        public Dictionary<int, string> GetStocksDictForInvoiceProduct(int actorCompanyId, int invoiceProductId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<StockDTO> dtos = GetStocksForInvoiceProduct(actorCompanyId, invoiceProductId);

            foreach (StockDTO dto in dtos)
            {
                if (!dict.ContainsKey(dto.StockId))
                    dict.Add(dto.StockId, dto.Code);
            }

            return dict;
        }

        public ActionResult SaveStock(StockDTO stockInput, int actorCompanyId)
        {
            if (stockInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Stock");

            // Default result is successful                        
            var result = new ActionResult();

            Stock stock = GetStockByCode(actorCompanyId, stockInput.Code);
            // Check if code exists already
            if ((stockInput.StockId == 0 && stock != null) || (stock != null && stock.StockId != stockInput.StockId))
            {
                result.Success = false;
                result.ErrorMessage = string.Format("{0}:{1}", GetText(110446, "Det finns redan en lagerplats med samma kod"), stock.Name);
                result.ErrorNumber = (int)ActionResultSave.Duplicate;
                return result;
            }
            // Check if shelf code exists in the warehouse
            if (stockInput.StockShelves != null && stockInput.StockShelves.Any())
            {
                var hasShelfCodeDuplicate = stockInput.StockShelves.Where(x => !x.IsDelete).GroupBy(x => x.Code.ToLower()).Any(g => g.Count() > 1);
                if (hasShelfCodeDuplicate)
                {
                    result.Success = false;
                    result.ErrorMessage = string.Format("{0}:{1}", GetText(110447, "Det finns redan en hyllplats med samma kod på denna lagerplats."), stockInput.Name);
                    result.ErrorNumber = (int)ActionResultSave.Duplicate;
                    return result;
                }
            }

            var eStock = new Stock()
            {
                ActorCompanyId = actorCompanyId,
                Name = stockInput.Name,
                Code = stockInput.Code,
                StockId = stockInput.StockId,
                IsExternal = stockInput.IsExternal,
                DeliveryAddressId = stockInput.DeliveryAddressId
            };

            result = SaveStock(eStock, actorCompanyId, stockInput.StockShelves, stockInput.AccountingSettings, stockInput.StockProducts);

            return result;

        }

        public ActionResult SaveStock(Stock stockInput, int actorCompanyId, List<StockShelfDTO> shelfs = null, List<AccountingSettingsRowDTO> accountSettings = null, List<StockProductDTO> stockProducts = null)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        result = SaveStock(entities, transaction, stockInput, actorCompanyId, shelfs, accountSettings, stockProducts);
                        if (result.Success)
                        {
                            // Commit transaction
                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SaveStock(CompEntities entities, TransactionScope transaction, Stock stockInput, int actorCompanyId, List<StockShelfDTO> shelfs = null, List<AccountingSettingsRowDTO> accountSettings = null, List<StockProductDTO> stockProducts = null)
        {
            if (stockInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Stock");

            // Default result is successful
            var result = new ActionResult();

            #region Stock

            // Get existing record
            Stock eStock = new Stock();
            if (stockInput.StockId > 0)
                eStock = GetStock(entities, stockInput.StockId, true);

            if (eStock == null || eStock.StockId == 0)
            {
                #region stock Add

                eStock = new Stock()
                {
                    ActorCompanyId = actorCompanyId,
                    Name = stockInput.Name,
                    Code = stockInput.Code,
                    IsExternal = stockInput.IsExternal,
                    DeliveryAddressId = stockInput.DeliveryAddressId
                };


                if (shelfs != null && shelfs.Any())
                {
                    eStock.StockShelf = new EntityCollection<StockShelf>();
                    shelfs.ForEach(f =>
                    {
                        if (!f.IsDelete)
                        {
                            eStock.StockShelf.Add(new StockShelf()
                            {
                                StockShelfId = 0,
                                Name = f.Name,
                                Code = f.Code,
                            });
                        }
                    });
                }


                entities.Stock.AddObject(eStock);
                #endregion
            }
            else
            {
                #region stock Update

                eStock.Name = stockInput.Name;
                eStock.Code = stockInput.Code;
                eStock.IsExternal = stockInput.IsExternal;
                eStock.DeliveryAddressId = stockInput.DeliveryAddressId;

                if (shelfs != null && shelfs.Any())
                {
                    #region Deleted Shelfs

                    var deletedShelfs = shelfs.Where(x => x.IsDelete && x.StockShelfId > 0).ToList();

                    // Delete existing Stock Shelfs
                    foreach (var del in deletedShelfs)
                    {
                        //Validating whether deleting shelf is being used in stock products before deleting it.
                        if (ValidateStockShelfForDelete(entities, del.StockShelfId))
                        {
                            result.Success = false;
                            result.ErrorMessage = this.GetText(92019, "Lageruppdateringen misslyckades. En eller flera borttagna hyllor används av produkt(er)");
                            return result;
                        }

                        var shelf = eStock.StockShelf.Where(x => x.StockShelfId == del.StockShelfId).FirstOrDefault();
                        if (shelf != null)
                            DeleteEntityItem(entities, shelf);
                    }


                    #endregion

                    #region New Shelfs

                    var newShelfs = shelfs.Where(x => !x.IsDelete && x.StockShelfId <= 0).ToList();

                    foreach (var shelf in newShelfs)
                    {
                        eStock.StockShelf.Add(new StockShelf()
                        {
                            StockId = eStock.StockId,
                            Name = shelf.Name,
                            Code = shelf.Code,
                        });
                    }
                    #endregion

                    #region Update Shelfs

                    var updatedShelfs = shelfs.Where(x => !x.IsDelete && x.StockShelfId > 0).ToList();

                    foreach (var udt in updatedShelfs)
                    {
                        var shelf = eStock.StockShelf.Where(x => x.StockShelfId == udt.StockShelfId).FirstOrDefault();
                        if (shelf != null)
                        {
                            shelf.Name = udt.Name;
                            shelf.Code = udt.Code;
                        }
                    }

                    #endregion
                }

                #endregion
            }

            #endregion

            #region StockProducts

            if (!stockProducts.IsNullOrEmpty())
            {
                if (!eStock.StockProduct.IsLoaded)
                    eStock.StockProduct.Load();

                foreach (var product in stockProducts)
                {
                    var stockProduct = eStock.StockProduct.FirstOrDefault(s => s.StockProductId == product.StockProductId);
                    
                    if (stockProduct == null) continue;

                    stockProduct.StockShelfId = product.StockShelfId;
                    stockProduct.PurchaseQuantity = product.PurchaseQuantity;
                    stockProduct.PurchaseTriggerQuantity = product.PurchaseTriggerQuantity;
                    stockProduct.DeliveryLeadTimeDays = product.DeliveryLeadTimeDays;
                }
            }
            #endregion

            result = SaveChanges(entities, transaction);

            if (!result.Success)
                return result;

            #region Accounts

            if (!accountSettings.IsNullOrEmpty())
            {
                SaveAccountingSettings(entities, actorCompanyId, eStock, accountSettings);
                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;
            }

            #endregion

            if (result.Success)
                result.IntegerValue = eStock.StockId;

            return result;

        }

        public ActionResult DeleteStock(int stockId)
        {
            // Default result is successful
            var result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    var stockInUse = (from a in entities.StockProduct
                                      where a.StockId == stockId && a.ActorCompanyId == ActorCompanyId
                                      select a).Any();

                    if (stockInUse)
                    {
                        //StocProduct found for stock, cannot delete
                        return new ActionResult(GetText(7698, "Lagerplatsen används och kan därför inte tas bort."));
                    }

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Stock

                        Stock stock = GetStock(entities, stockId);
                        if (stock == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "Stock");

                        result = ChangeEntityState(entities, stock, SoeEntityState.Deleted, true);
                        if (!result.Success)
                            return result;

                        #endregion

                        // Commit transaction
                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private void SaveAccountingSettings(CompEntities entities, int actorCompanyId, Stock stock, List<AccountingSettingsRowDTO> accountItems)
        {
            #region Prereq

            List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

            #endregion

            #region Delete AccountingSettings

            // Loop over existing settings
            foreach (StockAccountStd stockAccountStd in stock.StockAccountStd.ToList())
            {
                //Delete account
                stockAccountStd.AccountInternal.Clear();
                stock.StockAccountStd.Remove(stockAccountStd);
                entities.DeleteObject(stockAccountStd);
            }

            #endregion

            #region Add AccountingSettings                           

            if (stock.StockAccountStd == null)
                stock.StockAccountStd = new EntityCollection<StockAccountStd>();

            foreach (AccountingSettingsRowDTO settingInput in accountItems)
            {

                AccountStd accStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);

                var stockAccountStd = new StockAccountStd
                {
                    Type = settingInput.Type,
                    AccountStd = accStd ?? null
                };
                stock.StockAccountStd.Add(stockAccountStd);

                // Internal accounts
                int dimCounter = 1;
                foreach (AccountDim dim in dims)
                {
                    // Get internal account from input
                    dimCounter++;
                    int accountId = 0;
                    if (dimCounter == 2)
                        accountId = settingInput.Account2Id;
                    else if (dimCounter == 3)
                        accountId = settingInput.Account3Id;
                    else if (dimCounter == 4)
                        accountId = settingInput.Account4Id;
                    else if (dimCounter == 5)
                        accountId = settingInput.Account5Id;
                    else if (dimCounter == 6)
                        accountId = settingInput.Account6Id;

                    // Add account internal
                    AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                    if (accountInternal != null)
                        stockAccountStd.AccountInternal.Add(accountInternal);
                }
            }

            #endregion            

        }

        public ActionResult ValidateStockShelfForDelete(int stockShelfId)
        {
            ActionResult result = new ActionResult();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.StockProduct.NoTracking();
            if (ValidateStockShelfForDelete(entitiesReadOnly, stockShelfId))
            {
                result.Success = false;
                result.ErrorMessage = this.GetText(92028, "Hyllan används på en eller flera artiklar.");
            }
            return result;
        }

        public bool ValidateStockShelfForDelete(CompEntities entities, int stockShelfId)
        {
            return entities.StockProduct.Any(p => p.StockShelfId == stockShelfId);
        }

        #endregion

        #region StockInventory

        public Dictionary<int, string> GetStockInventoriesDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<StockInventoryHead> heads = GetStockInventories(actorCompanyId);
            foreach (StockInventoryHead head in heads)
            {
                dict.Add(head.StockInventoryHeadId, head.HeaderText);
            }

            return dict;
        }

        public List<StockInventoryHead> GetStockInventories(int actorCompanyId, bool includeCompleted = true, int? stockInventoryId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.StockInventoryHead.NoTracking();
            return GetStockInventories(entities, actorCompanyId, includeCompleted, stockInventoryId);
        }

        public List<StockInventoryHead> GetStockInventories(CompEntities entities, int actorCompanyId, bool includeCompleted = true, int? stockInventoryId = null)
        {
            IQueryable<StockInventoryHead> query = (from a in entities.StockInventoryHead
                    .Include("Stock")
                                                    where a.ActorCompanyId == actorCompanyId &&
                                                    (includeCompleted || a.InventoryStop == null)
                                                    select a);

            if (stockInventoryId.HasValue)
                query = query.Where(x => x.StockInventoryHeadId == stockInventoryId.Value);

            return query.ToList();
        }

        public List<StockInventoryHeadDTO> GetStockInventoryHeadDTOs(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.StockInventoryHead.NoTracking();
            return GetStockInventoryHeadDTOs(entities, actorCompanyId);
        }

        public List<StockInventoryHeadDTO> GetStockInventoryHeadDTOs(CompEntities entities, int actorCompanyId)
        {
            return (from a in entities.StockInventoryHead
                    where a.ActorCompanyId == actorCompanyId
                    select a).Select(EntityExtensions.StockInventoryHeadDTO).ToList();
        }

        public StockInventoryHead GetStockInventory(int stockInventoryId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.StockInventoryHead.NoTracking();
            return GetStockInventory(entities, stockInventoryId);
        }

        public StockInventoryHead GetStockInventory(CompEntities entities, int stockInventoryId, bool loadRows = false)
        {
            if (stockInventoryId == 0)
                return null;

            var stockHead = (from a in entities.StockInventoryHead
                             .Include("Stock")
                             where a.StockInventoryHeadId == stockInventoryId
                             select a).FirstOrDefault();

            if ((loadRows) && !(stockHead is null) && (!stockHead.StockInventoryRow.IsLoaded))
            {
                stockHead.StockInventoryRow.Load();
            }

            return stockHead;
        }

        public StockInventoryHeadDTO GetStockInventoryHeadDTO(int stockInventoryId, bool loadRows = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetStockInventoryHeadDTO(entities, stockInventoryId, loadRows);
        }

        public StockInventoryHeadDTO GetStockInventoryHeadDTO(CompEntities entities, int stockInventoryId, bool loadRows = false)
        {
            IQueryable<StockInventoryHead> query = (from a in entities.StockInventoryHead
                                                    where a.StockInventoryHeadId == stockInventoryId
                                                    select a);

            var stockInventoryDto = query.Select(EntityExtensions.StockInventoryHeadDTO).FirstOrDefault();

            if ((stockInventoryDto != null) && loadRows)
            {
                stockInventoryDto.StockInventoryRows = GetStockInventoryRowDTOs(stockInventoryId);
            }

            return stockInventoryDto;
        }

        public ActionResult SaveStockInventory(int actorCompanyId, StockInventoryHeadDTO stockInventoryInput, List<StockInventoryRowDTO> stockInventoryRows)
        {
            if (stockInventoryInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "StockInventoryHead");

            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Stock

                        // Get existing record
                        var stockInventoryHead = GetStockInventory(entities, stockInventoryInput.StockInventoryHeadId);

                        if (stockInventoryHead == null)
                        {
                            #region stockInventory Add

                            stockInventoryHead = new StockInventoryHead()
                            {
                                ActorCompanyId = actorCompanyId,
                                HeaderText = stockInventoryInput.HeaderText,
                                StockId = stockInventoryInput.StockId,
                                InventoryStart = DateTime.Now,
                            };

                            entities.StockInventoryHead.AddObject(stockInventoryHead);
                            SetCreatedProperties(stockInventoryHead);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success) return result;


                            var newInventoryRows = new List<StockInventoryRow>();
                            //add inventory rows
                            foreach (StockInventoryRowDTO row in stockInventoryRows)
                            {
                                //StockProduct sp = GetStockProduct(entities, row.StockProductId);
                                var eStockInventoryRow = new StockInventoryRow()
                                {
                                    StockInventoryHeadId = stockInventoryHead.StockInventoryHeadId,
                                    StockProductId = row.StockProductId,
                                    StartingSaldo = row.StartingSaldo,
                                    //StockInventoryHead = stockInventoryHead,
                                    //StockProduct = sp
                                };
                                SetCreatedProperties(eStockInventoryRow);

                                newInventoryRows.Add(eStockInventoryRow);

                                //sp.IsInInventory = true;
                            }

                            result = BulkInsert(entities, newInventoryRows, transaction);
                            if (!result.Success)
                                return result;

                            var stockProductIds = stockInventoryRows.Select(x => x.StockProductId).ToList();
                            var stockProducts = entities.StockProduct.Where(x => stockProductIds.Contains(x.StockProductId) && x.ActorCompanyId == actorCompanyId).ToList();
                            stockProducts.ForEach(x => { x.IsInInventory = true; });
                            //result = BulkUpdate(entities, stockProducts, transaction);
                            result = SaveChanges(entities, transaction);

                            #endregion
                        }
                        else
                        {
                            #region stockInventory Update

                            /*
                            stockInventoryHead.HeaderText = stockInventoryInput.HeaderText;
                            stockInventoryHead.InventoryStart = stockInventoryInput.InventoryStart;
                            stockInventoryHead.InventoryStop = stockInventoryInput.InventoryStop;
                            */

                            SetModifiedProperties(stockInventoryHead);

                            result = SaveChanges(entities, transaction);
                            if (!result.Success) return result;

                            var inventoryRows = GetStockInventoryRows(entities, stockInventoryHead.StockInventoryHeadId, false);
                            var inventoryRowsLookup = inventoryRows.ToLookup(x => x.StockInventoryRowId);

                            //change inventory rows
                            foreach (var row in stockInventoryRows)
                            {
                                var inventoryRow = inventoryRowsLookup[row.StockInventoryRowId].First();
                                inventoryRow.InventorySaldo = row.InventorySaldo;
                                inventoryRow.Difference = row.InventorySaldo - row.StartingSaldo;
                                inventoryRow.TransactionDate = row.TransactionDate;

                                SetModifiedProperties(inventoryRow);
                            }

                            result = BulkUpdate(entities, inventoryRows, transaction);

                            #endregion
                        }

                        #endregion

                        if (result.Success)
                        {
                            // Commit transaction
                            transaction.Complete();
                            result.IntegerValue = stockInventoryHead.StockInventoryHeadId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public List<StockInventoryRow> GetStockInventoryRows(int stockInventoryHeadId, bool includeStockProduct)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.StockInventoryRow.NoTracking();
            return GetStockInventoryRows(entities, stockInventoryHeadId, includeStockProduct);
        }

        public List<StockInventoryRow> GetStockInventoryRows(CompEntities entities, int stockInventoryHeadId, bool includeStockProduct)
        {
            IQueryable<StockInventoryRow> query = (from a in entities.StockInventoryRow
                                                   where a.StockInventoryHeadId == stockInventoryHeadId
                                                   select a);

            if (includeStockProduct)
            {
                query = query.Include("StockProduct").Include("StockProduct.Stock");
            }

            return query.ToList();
        }

        public StockInventoryRow GetStockInventoryRow(CompEntities entities, int stockInventoryRowId)
        {
            return (from a in entities.StockInventoryRow
                    where a.StockInventoryRowId == stockInventoryRowId
                    select a).FirstOrDefault();
        }

        public List<StockInventoryRowDTO> GetStockInventoryRowDTOs(int stockInventoryHeadId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<StockInventoryRow> query = (from p in entitiesReadOnly.StockInventoryRow
                                                   where
                                                          p.StockInventoryHeadId == stockInventoryHeadId
                                                   select p);

            return query.Select(EntityExtensions.StockInventoryRowDTO).ToList();
        }

        public List<StockInventoryRowDTO> GenerateStockInventoryRows(StockInventoryFilterDTO filter)
        {
            if (filter.ProductNrFrom == "undefined" || filter.ProductNrFrom == "null")
                filter.ProductNrFrom = string.Empty;
            if (filter.ProductNrTo == "undefined" || filter.ProductNrTo == "null")
                filter.ProductNrTo = string.Empty;

            var inventoryRows = new List<StockInventoryRowDTO>();
            var stockProducts = GetStockProductDTOs(base.ActorCompanyId, null, filter.StockId, false);
            foreach (StockProductDTO dto in stockProducts)
            {
                if (dto.IsInInventory)
                    continue;

                if (!String.IsNullOrWhiteSpace(filter.ProductNrFrom) && !string.IsNullOrWhiteSpace(filter.ProductNrTo) &&
                    !StringUtility.IsInInterval(dto.ProductNumber, filter.ProductNrFrom, filter.ProductNrTo))
                    continue;

                if (!filter.ShelfIds.IsNullOrEmpty() && !filter.ShelfIds.Any(x => x == dto.StockShelfId))
                    continue;

                if (!filter.ProductGroupIds.IsNullOrEmpty() && !filter.ProductGroupIds.Any(x => x == dto.ProductGroupId))
                    continue;

                var rowDTO = new StockInventoryRowDTO
                {
                    StockProductId = dto.StockProductId,
                    ProductNumber = dto.ProductNumber,
                    ProductName = dto.ProductName,
                    ShelfId = dto.StockShelfId.GetValueOrDefault(),
                    ShelfCode = dto.StockShelfCode,
                    ShelfName = dto.StockShelfName,
                    StartingSaldo = dto.Quantity,
                    InventorySaldo = 0,
                    Difference = 0,
                    OrderedQuantity = dto.OrderedQuantity,
                    ReservedQuantity = dto.ReservedQuantity
                };
                inventoryRows.Add(rowDTO);
            }

            return inventoryRows;
        }

        public ActionResult CloseInventory(int stockInventoryHeadId)
        {
            // Default result is successful
            var result = new ActionResult();
            var stockTransactions = new List<StockTransaction>();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        //Head
                        StockInventoryHead stockInventory = (from a in entities.StockInventoryHead
                                                             where a.StockInventoryHeadId == stockInventoryHeadId
                                                             select a).First();

                        stockInventory.InventoryStop = DateTime.Now;

                        List<StockInventoryRow> stockInventoryRows = GetStockInventoryRows(entities, stockInventoryHeadId, true);
                        foreach (var row in stockInventoryRows)
                        {
                            StockProduct stPr = row.StockProduct;
                            stPr.IsInInventory = false;
                            SetModifiedProperties(stPr);

                            if (row.StartingSaldo != row.InventorySaldo)
                            {
                                decimal currentStartingQuantity;
                                var stockTransactionsUnderInventory = GetStockTransactionDTOs(entities, stPr.StockProductId, stockInventory.InventoryStart.Value.Date.AddDays(1), row.TransactionDate ?? stockInventory.InventoryStop.Value.Date,
                                                                                                    false,
                                                                                                    new List<TermGroup_StockTransactionType> { TermGroup_StockTransactionType.Take, TermGroup_StockTransactionType.Add });
                                if (stockTransactionsUnderInventory.Any())
                                {
                                    currentStartingQuantity = row.StartingSaldo + CalculateTransactionNetSaldo(stockTransactionsUnderInventory);
                                }
                                else
                                {
                                    currentStartingQuantity = row.StartingSaldo;
                                }

                                if (currentStartingQuantity != row.InventorySaldo)
                                {
                                    //Create a transaction
                                    var stockTransaction = new StockTransaction
                                    {
                                        //allways new
                                        StockProductId = row.StockProductId,
                                        Quantity = row.InventorySaldo - currentStartingQuantity,
                                        ActionType = (int)TermGroup_StockTransactionType.Correction,
                                        StockInventoryRowId = row.StockInventoryRowId,
                                        Price = stPr.AvgPrice,
                                        StockProduct = stPr,
                                        TransactionDate = row.TransactionDate ?? stockInventory.InventoryStop.Value,
                                    };
                                    SetCreatedProperties(stockTransaction);

                                    stPr.Quantity = stPr.Quantity + stockTransaction.Quantity;

                                    stockTransactions.Add(stockTransaction);

                                    SetModifiedProperties(row);
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            if (stockTransactions.Any())
                            {
                                if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingCreateVouchersForStockTransactions, 0, base.ActorCompanyId, 0, true))
                                {
                                    CreateVoucherFromTransactions(entities, transaction, stockTransactions, base.UserId, base.ActorCompanyId, TermGroup_StockTransactionType.Correction, DateTime.Now, null);
                                }
                            }

                            transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);

                    }

                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult DeleteStockInventory(int stockInventoryHeadId, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();


                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region StockInventory

                        StockInventoryHead head = GetStockInventory(entities, stockInventoryHeadId);
                        if (head == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "StockInventoryHead");
                        if (!head.StockInventoryRow.IsLoaded)
                            head.StockInventoryRow.Load();
                        var inventoryRows = GetStockInventoryRows(stockInventoryHeadId, false);
                        foreach (StockInventoryRow row in inventoryRows)
                        {
                            StockProduct product = GetStockProduct(row.StockProductId);
                            product.IsInInventory = false;

                            head.StockInventoryRow.Remove(row);

                            DeleteEntityItem(entities, row);

                        }
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        if (!head.CompanyReference.IsLoaded)
                            head.CompanyReference.Load();
                        company.StockInventoryHead.Remove(head);

                        head.StockId = null;
                        result = DeleteEntityItem(entities, head);
                        if (!result.Success)
                            return result;

                        #endregion

                        // Commit transaction
                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        #region StockProduct

        public StockProduct GetStockProduct(int stockProductId, bool loadInvoiceProductAndStock = false, bool loadInvoiceProductGroup = false, bool includeAccountSettings = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.StockProduct.NoTracking();
            return GetStockProduct(entities, stockProductId, loadInvoiceProductAndStock, loadInvoiceProductGroup, includeAccountSettings);
        }

        public StockProduct GetStockProduct(CompEntities entities, int stockProductId, bool loadInvoiceProductAndStock = false, bool loadInvoiceProductGroup = false, bool includeAccountSettings = false)
        {
            var query = (from a in entities.StockProduct
                         where a.StockProductId == stockProductId
                         select a);

            if (loadInvoiceProductAndStock)
            {
                query = query.Include("InvoiceProduct");
                query = query.Include("InvoiceProduct.ProductUnit");
                query = query.Include("Stock");

                if (loadInvoiceProductGroup)
                    query = query.Include("InvoiceProduct.ProductGroup");
            }

            if (includeAccountSettings)
            {
                if (!loadInvoiceProductAndStock)
                    query = query.Include("Stock");
                query = query.Include("Stock.StockAccountStd.AccountStd.Account");
                query = query.Include("Stock.StockAccountStd.AccountInternal.Account.AccountDim");
            }

            return query.FirstOrDefault();
        }

        public List<StockProductSmallDTO> GetStockProductSmallDTOs(int actorCompanyId, bool includeInactive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetStockProductSmallDTOs(entities, actorCompanyId, includeInactive);
        }

        public List<StockProductSmallDTO> GetStockProductSmallDTOs(CompEntities entities, int actorCompanyId, bool includeInactive)
        {
            IQueryable<StockProduct> query = (from p in entities.StockProduct
                                              where
                                             p.ActorCompanyId == actorCompanyId
                                              select p);
            if (!includeInactive)
            {
                query = query.Where(x => x.InvoiceProduct.State == (int)SoeEntityState.Active || x.Quantity > 0);
            }

            return query.Select(EntityExtensions.StockProductSmallDTO)
                .ToList();
        }

        public List<StockProductAvgPriceDTO> GetStockProductAvgPriceDTOs(CompEntities entities, int actorCompanyId, int invoiceProductId)
        {
            IQueryable<StockProduct> query = (from p in entities.StockProduct
                                              where
                                                p.ActorCompanyId == actorCompanyId &&
                                                p.InvoiceProductId == invoiceProductId
                                              select p);

            return query.Select(EntityExtensions.StockProductAvgPriceDTO).ToList();
        }

        public StockProductAvgPriceDTO GetStockProductAvgPriceDTO(int stockId, int invoiceProductId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<StockProduct> query = (from p in entitiesReadOnly.StockProduct
                                              where
                                                p.ActorCompanyId == actorCompanyId &&
                                                p.StockId == stockId &&
                                                p.InvoiceProductId == invoiceProductId
                                              select p);

            return query.Select(EntityExtensions.StockProductAvgPriceDTO).FirstOrDefault();
        }

        public StockProductDTO GetStockProductDTO(int stockProductId)
        {
            using (var entities = new CompEntities())
            {
                var marginCalculationType = GetGrossMarginCalculationType(entities);
                IQueryable<StockProduct> query = (from a in entities.StockProduct
                                                  where a.StockProductId == stockProductId
                                                  select a);
                var st = query
                   .Select(EntityExtensions.StockProductDTO((int)marginCalculationType))
                   .FirstOrDefault();

                if (st != null)
                {
                    st.StockValue = st.Quantity * st.AvgPrice;
                }

                return st;
            }
        }

        public List<StockProductDTO> GetStockProductDTOs(int actorCompanyId, int? productId, int? stockId = null, bool? includeInactive = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetStockProductDTOs(entities, actorCompanyId, productId, stockId, includeInactive);
        }

        public List<StockProductDTO> GetStockProductDTOs(CompEntities entities, int actorCompanyId, int? productId, int? stockId = null, bool? includeInactive = null, int? stockProductId = null)
        {
            var marginCalculationType = GetGrossMarginCalculationType(entities);

            IQueryable<StockProduct> query = (from p in entities.StockProduct
                                              where
                                                p.ActorCompanyId == actorCompanyId
                                              select p);

            if (includeInactive.GetValueOrDefault())
            {
                query = query.Where(x => x.InvoiceProduct.State == (int)SoeEntityState.Active || x.InvoiceProduct.State == (int)SoeEntityState.Inactive);
            }
            else
            {
                query = query.Where(x => x.InvoiceProduct.State == (int)SoeEntityState.Active || (x.Quantity > 0 && x.InvoiceProduct.State == (int)SoeEntityState.Inactive));
            }

            if (productId.HasValue)
            {
                query = query.Where(x => x.InvoiceProductId == productId.Value);
            }

            if (stockId.HasValue)
            {
                query = query.Where(x => x.StockId == stockId.Value);
            }

            if (stockProductId.HasValue)
            {
                query = query.Where(x => x.StockProductId == stockProductId.Value);
            }

            return query
                   .Select(EntityExtensions.StockProductDTO((int)marginCalculationType))
                   .ToList();
        }

        private TermGroup_GrossMarginCalculationType GetGrossMarginCalculationType(CompEntities entities)
        {
            var defaultSettingUseAvgPrice = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultGrossMarginCalculationType, 0, base.ActorCompanyId, 0);
            if (defaultSettingUseAvgPrice == 0)
                return TermGroup_GrossMarginCalculationType.StockAveragePrice;
            else
                return (TermGroup_GrossMarginCalculationType)defaultSettingUseAvgPrice;
        }

        public List<StockProductDTO> GetStockProductDTOs(int actorCompanyId, List<int> productIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetStockProductDTOs(entities, actorCompanyId, productIds);
        }

        public List<StockProductDTO> GetStockProductDTOs(CompEntities entities, int actorCompanyId, List<int> productIds)
        {
            return (from p in entities.StockProduct
                    where
                    p.ActorCompanyId == actorCompanyId &&
                    productIds.Contains(p.InvoiceProductId)
                    select p).Select(EntityExtensions.StockProductDTO(0)).ToList();
        }


        public List<StockProductDTO> GetStockProductsByStockId(int actorCompanyId, int stockId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetStockProductDTOs(entities, actorCompanyId, null, stockId, false, null);
        }

        public List<StockProductDTO> GetStockProductDTOsWithSaldo(int actorCompanyId, bool includeInactive, int? stockProductId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetStockProductDTOsWithSaldo(entities, actorCompanyId, includeInactive, stockProductId);
        }

        public List<StockProductDTO> GetStockProductDTOsWithSaldo(CompEntities entities, int actorCompanyId, bool includeInactive, int? stockProductId = null)
        {
            var stockProducts = GetStockProductDTOs(entities, actorCompanyId, null, null, includeInactive, stockProductId);

            foreach (var sp in stockProducts)
            {
                sp.StockValue = sp.Quantity * sp.AvgPrice;
            }

            if (FeatureManager.HasRolePermission(Feature.Billing_Purchase, Permission.Readonly, this.RoleId, actorCompanyId, entities: entities))
            {
                var purchaseRows = entities.PurchaseRow
                    .Where(r => r.Purchase.Origin.ActorCompanyId == actorCompanyId &&
                        r.State == 0 && r.Purchase.State == 0 &&
                        r.Purchase.Origin.Status != (int)SoeOriginStatus.PurchaseDeliveryCompleted &&
                        r.StockId > 0 && r.ProductId > 0)
                    .GroupBy(r => new { r.StockId, r.ProductId })
                    .Select(r => new
                    {
                        StockId = r.Key.StockId,
                        ProductId = r.Key.ProductId,
                        Quantity = r.Sum(g => g.Quantity),
                        DeliveredQuantity = r.Sum(g => g.DeliveredQuantity),
                    })
                    .ToList();
                foreach (var item in purchaseRows)
                {
                    var stockProduct = stockProducts.Find(sp => sp.StockId == item.StockId && sp.InvoiceProductId == item.ProductId);
                    if (stockProduct is null) continue;
                    stockProduct.PurchasedQuantity = item.Quantity - (item.DeliveredQuantity ?? 0);
                }
            }

            return stockProducts.OrderBy(x => x.NumberSort).ToList();
        }

        public List<ProductSmallDTO> GetProductNotInStocktProductSmallDTOs(int actorCompanyId, int stockId, List<int> productIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetProductNotInStocktProductSmallDTOs(entities, actorCompanyId, stockId, productIds);
        }

        public List<ProductSmallDTO> GetStockProductProductSmallDTOs(int actorCompanyId, int? stockId = null, bool? onlyActive = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseRow.NoTracking();
            return GetStockProductProductSmallDTOs(entities, actorCompanyId, stockId, onlyActive);
        }

        public List<ProductSmallDTO> GetStockProductProductSmallDTOs(CompEntities entities, int actorCompanyId, int? stockId = null, bool? onlyActive = false)
        {
            var query = (from p in entities.StockProduct
                         where
                        p.ActorCompanyId == actorCompanyId
                         select p);

            if (stockId.HasValue && stockId != 0)
            {
                query = query.Where(x => x.StockId == stockId);
            }

            if (onlyActive.HasValue && onlyActive.Value)
                query = query.Where(x => x.InvoiceProduct.State == (int)SoeEntityState.Active);

            return query
               .Select(EntityExtensions.ProductSmallDTO)
               .DistinctBy(x => x.ProductId)
               .OrderBy(p => p.Number)
               .ToList();
        }

        public List<ProductSmallDTO> GetProductNotInStocktProductSmallDTOs(CompEntities entities, int actorCompanyId, int stockId, List<int> productIds)
        {
            var query = (from p in entities.StockProduct
                         where
                        p.ActorCompanyId == actorCompanyId && p.StockId == stockId && !productIds.Contains(p.InvoiceProduct.ProductId)
                         select p);


            return query
               .Select(EntityExtensions.ProductSmallDTO)
               .DistinctBy(x => x.ProductId)
               .OrderBy(p => p.Number)
               .ToList();
        }

        private StockProduct GetAddStockProductFromInvoiceProduct(CompEntities entities, TransactionScope transaction, int invoiceProductId, int stockId, int actorCompanyId, decimal price, int stockShelfId)
        {
            var stockProduct = this.GetStockProductFromInvoiceProduct(entities, invoiceProductId, stockId, stockShelfId);
            if (stockProduct == null)
            {
                var stockDto = new StockDTO
                {
                    StockId = stockId,
                    AvgPrice = price,
                    StockShelfId = stockShelfId
                };

                this.SaveStockProducts(entities, transaction, new List<StockDTO> { stockDto }, invoiceProductId, actorCompanyId);
                stockProduct = this.GetStockProductFromInvoiceProduct(entities, invoiceProductId, stockId, stockShelfId);

                var invoiceProduct = ProductManager.GetInvoiceProduct(entities, invoiceProductId);
                if ((!invoiceProduct.IsStockProduct.HasValue) || (!invoiceProduct.IsStockProduct.Value))
                {
                    invoiceProduct.IsStockProduct = true;
                }
            }

            return stockProduct;
        }

        public StockProduct GetStockProductFromInvoiceProduct(int invoiceProductId, int stockId, int stockShelfId = 0)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetStockProductFromInvoiceProduct(entities, invoiceProductId, stockId, stockShelfId);
        }

        public StockProduct GetStockProductFromInvoiceProduct(CompEntities entities, int invoiceProductId, int stockId, int stockShelfId = 0)
        {
            if (stockShelfId == 0)
                return (from sp in entities.StockProduct
                        where sp.InvoiceProductId == invoiceProductId &&
                        sp.StockId == stockId
                        select sp).FirstOrDefault();
            else
                return (from sp in entities.StockProduct
                        where sp.InvoiceProductId == invoiceProductId &&
                        sp.StockId == stockId && sp.StockShelfId == stockShelfId
                        select sp).FirstOrDefault();
        }

        public List<StockProduct> GetStockProductsFromInvoiceProduct(CompEntities entities, int invoiceProductId, int? stockId, int actorCompanyId)
        {
            IQueryable<StockProduct> query = (from sp in entities.StockProduct.Include("Stock")
                                              where sp.InvoiceProductId == invoiceProductId && sp.ActorCompanyId == actorCompanyId
                                              select sp);

            if (stockId.HasValue)
            {
                query = query.Where(x => x.StockId == stockId);
            }

            return query.ToList();
        }

        public ActionResult SaveStockProducts(CompEntities entities, TransactionScope transaction, List<StockDTO> stocksInput, int invoiceProductId, int actorCompanyId)
        {

            //Get the invoiceproduct
            //Get the existing stocks for product
            List<StockProductDTO> stockProducts = (from a in entities.StockProduct
                                                   where a.InvoiceProductId == invoiceProductId
                                                   select a).ToDTOs().ToList();

            //update = do nothing, add if does not exist
            foreach (var stock in stocksInput)
            {
                var stockExist = stockProducts.Where(s => s.StockId == stock.StockId);
                if (stockExist.Any())
                {

                    StockProduct exist = (from a in entities.StockProduct.OfType<StockProduct>()
                                          where a.InvoiceProductId == invoiceProductId &&
                                                a.StockId == stock.StockId
                                          select a).FirstOrDefault();

                    if (exist != null)
                    {
                        exist.StockShelfId = stock.StockShelfId.ToNullable();
                        SetModifiedProperties(exist);
                    }
                }
                else
                {
                    var eStockProduct = new StockProduct()
                    {
                        ActorCompanyId = actorCompanyId,
                        InvoiceProductId = invoiceProductId,
                        StockId = stock.StockId,
                        AvgPrice = stock.AvgPrice,
                        IsInInventory = false,
                        OrderedQuantity = 0,
                        ReservedQuantity = 0,
                        Quantity = stock.Saldo,
                        StockShelfId = stock.StockShelfId.ToNullable()
                    };

                    entities.StockProduct.AddObject(eStockProduct);
                    SetCreatedProperties(eStockProduct);
                }
            }

            return SaveChanges(entities, transaction);
        }

        public ActionResult SaveStockProducts(CompEntities entities, TransactionScope transaction, List<StockDTO> stocksInput, InvoiceProduct invoiceProduct, int actorCompanyId)
        {

            var duplicates = stocksInput.GroupBy(x => x.StockId).FirstOrDefault(x => x.Count() > 1);
            if (duplicates != null)
            {
                return new ActionResult(GetText(7678, "Artikel kan bara finnas en gång per lagerplats") + ":" + duplicates.First().Name);
            }

            //Get the existing stocks for product
            List<StockProductDTO> stockProducts = GetStockProductDTOs(entities, actorCompanyId, invoiceProduct.ProductId);

            //update = do nothing, add if does not exist
            foreach (var stockInput in stocksInput)
            {
                var stockExist = stockProducts.Where(s => s.StockId == stockInput.StockId);
                if (stockExist.Any())
                {
                    StockProduct exist = GetStockProductFromInvoiceProduct(entities, invoiceProduct.ProductId, stockInput.StockId);

                    exist.StockShelfId = stockInput.StockShelfId.ToNullable();
                    exist.DeliveryLeadTimeDays = stockInput.DeliveryLeadTimeDays;
                    exist.PurchaseQuantity = stockInput.PurchaseQuantity;
                    exist.PurchaseTriggerQuantity = stockInput.PurchaseTriggerQuantity;
                    SetModifiedProperties(exist);
                }
                else
                {
                    var eStockProduct = new StockProduct()
                    {
                        ActorCompanyId = actorCompanyId,
                        InvoiceProduct = invoiceProduct,
                        StockId = stockInput.StockId,
                        AvgPrice = stockInput.AvgPrice,
                        IsInInventory = false,
                        OrderedQuantity = 0,
                        ReservedQuantity = 0,
                        Quantity = stockInput.Saldo,
                        StockShelfId = stockInput.StockShelfId.ToNullable(),
                        DeliveryLeadTimeDays = stockInput.DeliveryLeadTimeDays,
                        PurchaseQuantity = stockInput.PurchaseQuantity,
                        PurchaseTriggerQuantity = stockInput.PurchaseTriggerQuantity,
                    };

                    entities.StockProduct.AddObject(eStockProduct);
                    SetCreatedProperties(eStockProduct);
                }
            }

            return SaveChanges(entities, transaction);
        }

        public static bool InvoiceProductHasStockBalance(CompEntities entities, int invoiceProductId, int actorCompanyId)
        {
            IQueryable<StockProduct> query = (from sp in entities.StockProduct
                                              where sp.InvoiceProductId == invoiceProductId &&
                                                    sp.ActorCompanyId == actorCompanyId &&
                                                    sp.Quantity != 0
                                              select sp);

            return query.Any();
        }

        #endregion

        #region StockShelf

        public StockShelf GetStockShelf(int stockShelfId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.StockShelf.NoTracking();
            return GetStockShelf(entities, stockShelfId);
        }

        public StockShelf GetStockShelf(CompEntities entities, int stockShelfId)
        {
            if (stockShelfId == 0)
                return null;

            return (from a in entities.StockShelf
                    where a.StockShelfId == stockShelfId
                    select a).FirstOrDefault();
        }

        public StockShelf GetStockShelfByCode(int stockId, string code)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Stock.NoTracking();
            return GetStockShelfByCode(entities, stockId, code);
        }

        public StockShelf GetStockShelfByCode(CompEntities entities, int stockId, string code)
        {
            StockShelf shelf = (from a in entities.StockShelf
                                where a.Code.ToLower() == code.ToLower() &&
                                a.StockId == stockId
                                select a).FirstOrDefault();

            return shelf;
        }

        public List<StockShelfDTO> GetStockShelfDTOs(int actorCompanyId, int stockId, bool addEmptyRow = false)
        {
            List<int> stockIds;
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (stockId == 0)
            {
                stockIds = (from a in entitiesReadOnly.Stock
                            where a.ActorCompanyId == actorCompanyId
                            select a.StockId).Distinct().ToList();
            }
            else
            {
                stockIds = new List<int> { stockId };
            }


            var stockShelfs = (from a in entitiesReadOnly.StockShelf
                               where stockIds.Contains(a.StockId)
                               select a).Select(s => new StockShelfDTO
                               {
                                   StockId = s.StockId,
                                   Code = s.Code,
                                   Name = s.Name,
                                   StockShelfId = s.StockShelfId,
                                   StockName = s.Stock.Name ?? ""
                               }).ToList();

            if (addEmptyRow)
            {
                stockShelfs.Insert(0, new StockShelfDTO { StockShelfId = 0, Name = "" });
            }

            return stockShelfs;
        }

        public ActionResult SaveStockShelf(StockShelfDTO stockShelfDto)
        {
            if (stockShelfDto == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "StockShelf");

            // Default result is successful
            ActionResult result = new ActionResult();
            // Check if already exists
            if (stockShelfDto.StockShelfId == 0)
            {
                StockShelf shelf = GetStockShelfByCode(stockShelfDto.StockId, stockShelfDto.Code);
                if (shelf != null)
                {
                    result.Success = false;
                    result.ErrorNumber = (int)ActionResultSave.Duplicate;
                    return result;
                }
            }

            var eStock = new StockShelf()
            {
                StockId = stockShelfDto.StockId,
                StockShelfId = stockShelfDto.StockShelfId,
                Name = stockShelfDto.Name,
                Code = stockShelfDto.Code,
            };

            return SaveStockShelf(eStock);
        }

        public ActionResult SaveStockShelf(StockShelf stockShelfInput)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Stock.NoTracking();
            return SaveStockShelf(entities, stockShelfInput);
        }

        public ActionResult SaveStockShelf(CompEntities entities, StockShelf stockShelfInput)
        {
            if (stockShelfInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "StockShelf");

            if (stockShelfInput.StockId == 0)
            {
                return new ActionResult(false, 0, GetText(7439, "Lagerplats saknas"));
            }

            var result = new ActionResult();

            try
            {

                #region StockShelf

                // Get existing record
                var eStock = (stockShelfInput.StockShelfId > 0) ? GetStockShelf(entities, stockShelfInput.StockShelfId) : stockShelfInput;

                if (eStock.StockShelfId == 0)
                {
                    entities.StockShelf.AddObject(eStock);
                }
                else
                {
                    #region Update

                    eStock.Name = stockShelfInput.Name;
                    eStock.Code = stockShelfInput.Code;
                    eStock.StockId = stockShelfInput.StockId;

                    #endregion
                }

                #endregion

                result = SaveChanges(entities);
                if (result.Success)
                {
                    result.IntegerValue = eStock.StockShelfId;
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                result.Exception = ex;
            }
            finally
            {
                if (result.Success)
                {
                    // Set success properties
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

                entities.Connection.Close();
            }

            return result;
        }

        public ActionResult DeleteStockShelf(int stockShelfId, int actorCompanyId)
        {
            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    StockProduct st = (from a in entities.StockProduct
                                       where a.StockShelfId == stockShelfId
                                       select a).FirstOrDefault();

                    if (st != null)
                    {
                        //StocProduct found for stockPlace, cannot delete
                        result.Success = false;
                        return result;
                    }

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region StockShelf

                        StockShelf stockShelf = GetStockShelf(entities, stockShelfId);
                        if (stockShelf == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "StockShelf");

                        result = DeleteEntityItem(entities, stockShelf);
                        if (!result.Success)
                            return result;

                        #endregion

                        // Commit transaction
                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion

        #region StockTransactions

        public static decimal CalculateTransactionNetSaldo(List<StockTransactionDTO> transactions)
        {
            return transactions.Where(x => x.ActionType == TermGroup_StockTransactionType.Add).Sum(x => x.Quantity)
                   - transactions.Where(x => x.ActionType == TermGroup_StockTransactionType.Take).Sum(x => x.Quantity);
        }

        public List<StockTransactionDTO> GetStockTransactionDTOs(int stockProductId, DateTime? transactionDateFrom = null, DateTime? transactionDateTo = null, bool includeActionName = false, List<TermGroup_StockTransactionType> types = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.StockTransaction.NoTracking();
            return GetStockTransactionDTOs(entities, stockProductId, transactionDateFrom, transactionDateTo, includeActionName, types);
        }

        public List<StockTransactionDTO> GetStockTransactionDTOs(CompEntities entities, int stockProductId, DateTime? transactionDateFrom = null, DateTime? transactionDateTo = null, bool includeActionName = false, List<TermGroup_StockTransactionType> types = null)
        {
            IQueryable<StockTransaction> query = (from a in entities.StockTransaction
                                                  where a.StockProductId == stockProductId
                                                  select a);

            if (transactionDateFrom.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= transactionDateFrom.Value);
            }

            if (transactionDateTo.HasValue)
            {
                query = query.Where(x => x.TransactionDate <= transactionDateTo.Value);
            }

            if (!types.IsNullOrEmpty())
            {
                query = query.Where(x => types.Select(y => (int)y).Contains(x.ActionType));
            }

            var stockTransactions = query.OrderByDescending(a => a.Created).Select(EntityExtensions.StockTransactionExDTO).ToList();

            if (includeActionName)
            {
                var langId = base.GetLangId();
                var actionlist = base.GetTermGroupDict(TermGroup.StockTransactionType, langId);
                var originTypes = base.GetTermGroupDict(TermGroup.OriginType, langId);

                foreach (var stockTransaction in stockTransactions)
                {
                    stockTransaction.ActionTypeName = actionlist[(int)stockTransaction.ActionType];

                    if (!string.IsNullOrEmpty(stockTransaction.StockInventoryNr))
                    {
                        stockTransaction.SourceLabel = GetText(4642, "Inventering") + ": " + stockTransaction.StockInventoryNr;
                        stockTransaction.SourceNr = stockTransaction.StockInventoryNr;
                    }
                    else if (!string.IsNullOrEmpty(stockTransaction.PurchaseNr) && stockTransaction.DeliveryNr.HasValue)
                    {
                        stockTransaction.SourceLabel = $"{GetText(7538, "Beställning")}: {stockTransaction.PurchaseNr}, {GetText(7563, "Inleverans")}: {stockTransaction.DeliveryNr}";
                        stockTransaction.SourceNr = stockTransaction.PurchaseNr;
                    }
                    else if (!string.IsNullOrEmpty(stockTransaction.InvoiceNr) && stockTransaction.OriginType.HasValue)
                    {
                        stockTransaction.SourceLabel = $"{originTypes[stockTransaction.OriginType.Value]}: {stockTransaction.InvoiceNr}";
                        stockTransaction.SourceNr = stockTransaction.InvoiceNr;
                    }
                }
            }

            return stockTransactions.Select(x => x as StockTransactionDTO).ToList();
        }

        public List<StockTransactionSmallDTO> GetStockTransactionSmallDTOs(CompEntities entities, int stockProductId,DateTime? transactionDateFrom = null, DateTime? transactionDateTo = null, List<TermGroup_StockTransactionType> types = null, bool orderByDescendng = false, int maxCount = 100)
        {
            IQueryable<StockTransaction> query = (from a in entities.StockTransaction
                                                  where a.StockProductId == stockProductId
                                                  select a);

            if (transactionDateFrom.HasValue)
            {
                query = query.Where(x => x.TransactionDate >= transactionDateFrom.Value);
            }

            if (transactionDateTo.HasValue)
            {
                query = query.Where(x => x.TransactionDate <= transactionDateTo.Value);
            }

            if (!types.IsNullOrEmpty())
            {
                query = query.Where(x => types.Select(y => (int)y).Contains(x.ActionType));
            }

            if (orderByDescendng)
            {
                return query.OrderByDescending(a => a.TransactionDate).Select(EntityExtensions.StockTransactionSmallDTO).Take(maxCount).ToList(); 
            }
            else
            {
                return query.OrderBy(a => a.TransactionDate).Select(EntityExtensions.StockTransactionSmallDTO).Take(maxCount).ToList();
            }
        }

        public ActionResult SaveStockTransactions(List<StockTransactionDTO> stockTransactionInputs, int actorCompanyId)
        {
            var result = new ActionResult();
            var stockTransactionsToVoucher = new List<StockTransaction>();
            var containsStockTransfer = false;
            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        foreach (var transByAction in stockTransactionInputs.GroupBy(x => new { x.ActionType, TransactionDate = x.TransactionDate.HasValue ? x.TransactionDate.Value.Date : DateTime.Today }))
                        {
                            foreach (var trans in transByAction)
                            {
                                result = SaveStockTransaction(entities, transaction, trans, actorCompanyId, true, false);
                                if (!result.Success)
                                {
                                    return result;
                                }

                                if (trans.ActionType != TermGroup_StockTransactionType.AveragePriceChange)
                                {
                                    stockTransactionsToVoucher.Add((StockTransaction)result.Value);
                                }

                                if (trans.ActionType == TermGroup_StockTransactionType.StockTransfer)
                                {
                                    stockTransactionsToVoucher.Add((StockTransaction)result.Value2);
                                    containsStockTransfer = true;
                                }
                            }

                            if (stockTransactionsToVoucher.Any() && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingCreateVouchersForStockTransactions, 0, base.ActorCompanyId, 0, true))
                            {
                                var stockTran = stockTransactionsToVoucher.First();
                                CreateVoucherFromTransactions(entities, transaction, stockTransactionsToVoucher, base.UserId, actorCompanyId, containsStockTransfer ? TermGroup_StockTransactionType.StockTransfer : (TermGroup_StockTransactionType)stockTran.ActionType, stockTran.TransactionDate.Date, stockTran.Note);
                            }

                            stockTransactionsToVoucher.Clear();
                        }

                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        public ActionResult SaveStockTransaction(CompEntities entities, TransactionScope transaction, StockTransactionDTO stockTransactionInput, int actorCompanyId, bool saveChanges = true, bool createVoucher = true)
        {
            var result = new ActionResult();
            int voucherId = 0;

            #region StockTransaction

            if (stockTransactionInput.StockProductId == 0 && stockTransactionInput.ProductId > 0 && stockTransactionInput.StockId > 0)
            {
                var stockProduct = GetAddStockProductFromInvoiceProduct(entities, transaction, stockTransactionInput.ProductId, stockTransactionInput.StockId, actorCompanyId, stockTransactionInput.Price, stockTransactionInput.StockShelfId);
                stockTransactionInput.StockProductId = stockProduct == null ? 0 : stockProduct.StockProductId;
            }

            if (stockTransactionInput.StockProductId == 0)
            {
                throw new Exception("SaveStockTransaction was called without StockProductId");
            }

            //Create a transaction
            var stockTransaction = new StockTransaction
            {
                //allways new
                StockProductId = stockTransactionInput.StockProductId,
                Quantity = stockTransactionInput.Quantity,
                ActionType = (int)stockTransactionInput.ActionType,
                Price = stockTransactionInput.Price,
                AvgPrice = stockTransactionInput.AvgPrice,
                Note = stockTransactionInput.Note,
                InvoiceRowId = stockTransactionInput.InvoiceRowId,
                InvoiceId = stockTransactionInput.InvoiceId,
                TransactionDate = stockTransactionInput.TransactionDate?.Date ?? DateTime.Today,
                ParentStockTransactionId = stockTransactionInput.ParentStockTransactionId,
            };
            SetCreatedProperties(stockTransaction);

            StockTransactionDTO stockTransactionToDTO = null;
            if (stockTransactionInput.ActionType == TermGroup_StockTransactionType.StockTransfer)
            {

                if (stockTransactionInput.TargetStockId == 0)
                {
                    return new ActionResult(GetText(7439, "Lagerplats saknas"));
                }
                var stockProduct = GetStockProductFromInvoiceProduct(entities, stockTransactionInput.ProductId, stockTransactionInput.TargetStockId);
                if (stockProduct == null)
                {
                    return new ActionResult(GetText(7533, "Lagerartikel saknas"));
                }

                stockTransaction.ActionType = (int)TermGroup_StockTransactionType.Take;

                stockTransactionToDTO = new StockTransactionDTO
                {
                    ActionType = TermGroup_StockTransactionType.Add,
                    Quantity = stockTransaction.Quantity,
                    StockProductId = stockProduct.StockProductId,
                    Price = stockTransaction.Price,
                    Note = stockTransactionInput.Note,
                    TransactionDate = stockTransaction.TransactionDate
                };
            }

            if (stockTransactionInput.ProductUnitConvertId != null && stockTransactionInput.ProductUnitConvertId > 0)
            {
                var unitConvert = ProductManager.GetProductUnitConvert(entities, (int)stockTransactionInput.ProductUnitConvertId);
                if (unitConvert != null)
                {
                    stockTransaction.Quantity = stockTransactionInput.Quantity * unitConvert.ConvertFactor;
                    stockTransaction.Price = stockTransactionInput.Price / (stockTransactionInput.Quantity * unitConvert.ConvertFactor);

                    stockTransactionInput.ReservedQuantity = stockTransactionInput.ReservedQuantity * unitConvert.ConvertFactor;
                }
            }

            if (stockTransaction.ActionType == (int)TermGroup_StockTransactionType.Reserve && stockTransaction.Quantity == 0)
            {
                stockTransaction.Quantity = stockTransactionInput.ReservedQuantity;
            }

            //Don´t save if Quantity is 0
            if (stockTransaction.Quantity != 0 ||
                stockTransaction.ActionType == (int)TermGroup_StockTransactionType.AveragePriceChange
             )
            {
                entities.StockTransaction.AddObject(stockTransaction);

                if (saveChanges)
                    result = SaveChanges(entities, transaction);
            }

            #endregion

            if (result.Success)
            {
                //update the saldo and avg price
                if (stockTransactionInput.ActionType == TermGroup_StockTransactionType.Reserve)
                    result = UpdateInvoiceProductStockSaldo(entities, transaction, stockTransaction, saveChanges, stockTransactionInput.ReservedQuantity, stockTransactionInput.Quantity);

                else
                    result = UpdateInvoiceProductStockSaldo(entities, transaction, stockTransaction, saveChanges);

                result.IntegerValue = stockTransaction.StockProductId;

                //Create Voucher...//Don´t save if Quantity is 0
                if (createVoucher && stockTransaction.Quantity != 0 && stockTransaction.ActionType != 4) //for reserve no voucher
                {
                    if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingCreateVouchersForStockTransactions, 0, base.ActorCompanyId, 0, true))
                    {
                        voucherId = CreateVoucherFromTransaction(entities, transaction, stockTransaction, base.UserId, actorCompanyId, saveChanges);
                    }
                    result.IntegerValue2 = voucherId;
                }

                if (stockTransactionToDTO != null)
                {
                    stockTransactionToDTO.ParentStockTransactionId = stockTransaction.StockTransactionId;
                    var saveToResult = SaveStockTransaction(entities, transaction, stockTransactionToDTO, actorCompanyId, saveChanges, createVoucher);
                    if (!saveToResult.Success)
                    {
                        return saveToResult;
                    }
                    else
                    {
                        result.Value2 = saveToResult.Value;
                    }
                }
            }

            result.Value = stockTransaction;

            return result;
        }

        #endregion

        #region StockVoucher

        public class AccountingInfo
        {
            public AccountPeriod accPeriod;
            public VoucherSeries voucherSeries;
        }

        private AccountingInfo GetAccountingInfo(CompEntities entities, int actorCompanyId, DateTime transDate, int userId)
        {
            AccountingInfo info = new AccountingInfo();
            int serieId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.StockDefaultVoucherSeriesType, userId, actorCompanyId, 0);
            var accYearId = AccountManager.GetAccountYearId(entities, transDate, actorCompanyId);

            //no account year found for transDate!
            if (accYearId == 0)
                throw new Exception(GetText(1281, "Redovisningsår hittades inte") + ": " + transDate.ToShortDateString());

            info.accPeriod = AccountManager.GetAccountPeriod(entities, accYearId, transDate, actorCompanyId);

            if (info.accPeriod == null)
                throw new Exception(GetText(8693, "Period hittades inte") + ": " + transDate.ToShortDateString());

            info.accPeriod = AccountManager.GetAccountPeriod(entities, accYearId, transDate, actorCompanyId);

            info.voucherSeries = VoucherManager.GetVoucherSerieByType(entities, serieId, accYearId);

            if (info.voucherSeries == null)
                throw new Exception(GetText(8403, "Verifikatserie saknas") + ": " + transDate.ToShortDateString());

            return info;
        }

        public List<AccountInternal> AddAccountInternalsFromInvoice(CompEntities entities, int invoiceRowId, InvoiceProduct product, int actorCompanyId)
        {
            var result = new List<AccountInternal>();

            var invoice = InvoiceManager.GetCustomerInvoiceRow(entities, invoiceRowId, false, true)?.CustomerInvoice;
            var dto = AccountManager.GetInvoiceProductAccount(entities, actorCompanyId, product.ProductId, invoice?.ProjectId ?? 0, invoice?.ActorId ?? 0, 0, ProductAccountType.Sales, (TermGroup_InvoiceVatType)invoice.VatType, true);
            if (dto != null && dto.AccountId.HasValue)
            {
                if (
                (invoice.DefaultDim2AccountId.GetValueOrDefault() > 0) ||
                (invoice.DefaultDim3AccountId.GetValueOrDefault() > 0) ||
                (invoice.DefaultDim4AccountId.GetValueOrDefault() > 0) ||
                (invoice.DefaultDim5AccountId.GetValueOrDefault() > 0) ||
                (invoice.DefaultDim6AccountId.GetValueOrDefault() > 0)
               )

                {
                    if (invoice.DefaultDim2AccountId.HasValue)
                    {
                        AccountInternal accInt = AccountManager.GetAccountInternal(entities, invoice.DefaultDim2AccountId.Value, actorCompanyId);
                        if (accInt != null)
                            result.Add(accInt);
                    }

                    if (invoice.DefaultDim3AccountId.HasValue)
                    {
                        AccountInternal accInt = AccountManager.GetAccountInternal(entities, invoice.DefaultDim3AccountId.Value, actorCompanyId);
                        if (accInt != null)
                            result.Add(accInt);
                    }

                    if (invoice.DefaultDim4AccountId.HasValue)
                    {
                        AccountInternal accInt = AccountManager.GetAccountInternal(entities, invoice.DefaultDim4AccountId.Value, actorCompanyId);
                        if (accInt != null)
                            result.Add(accInt);
                    }

                    if (invoice.DefaultDim5AccountId.HasValue)
                    {
                        AccountInternal accInt = AccountManager.GetAccountInternal(entities, invoice.DefaultDim5AccountId.Value, actorCompanyId);
                        if (accInt != null)
                            result.Add(accInt);
                    }

                    if (invoice.DefaultDim6AccountId.HasValue)
                    {
                        AccountInternal accInt = AccountManager.GetAccountInternal(entities, invoice.DefaultDim6AccountId.Value, actorCompanyId);
                        if (accInt != null)
                            result.Add(accInt);
                    }
                }
                else
                {
                    // Add internal accounts
                    if (dto.AccountInternals != null && dto.AccountInternals.Count > 0)
                    {
                        foreach (AccountInternalDTO accountInternal in dto.AccountInternals)
                        {
                            AccountInternal accInt = AccountManager.GetAccountInternal(entities, accountInternal.AccountId, actorCompanyId);
                            if (accInt != null)
                                result.Add(accInt);
                        }
                    }
                }
            }

            return result;
        }
        public int CreateVoucherFromTransactions(CompEntities entities, TransactionScope trans, List<StockTransaction> stockTransList, int userId, int actorCompanyId, TermGroup_StockTransactionType actionType, DateTime transDate, string extraText)
        {
            var accountingInfo = GetAccountingInfo(entities, actorCompanyId, transDate, userId);
            return CreateVoucherFromTransactions(entities, trans, stockTransList, userId, actorCompanyId, actionType, transDate, extraText, accountingInfo);
        }

        public int CreateVoucherFromTransactions(CompEntities entities, TransactionScope trans, List<StockTransaction> stockTransList, int userId, int actorCompanyId, TermGroup_StockTransactionType actionType, DateTime transDate, string extraText, AccountingInfo accountingInfo)
        {
            string verText;
            switch (actionType)
            {
                case TermGroup_StockTransactionType.Correction:
                    verText = GetText(4642, "Inventering");
                    break;
                case TermGroup_StockTransactionType.Add:
                    verText = GetText(4788, "Lager, inleverans");
                    break;
                case TermGroup_StockTransactionType.Take:
                    verText = GetText(4789, "Lager, utleverans");
                    break;
                case TermGroup_StockTransactionType.StockTransfer:
                    verText = GetText(7691, "Lager, lageromföring");
                    break;
                case TermGroup_StockTransactionType.Loss:
                    verText = GetText(7735, "Lager, kassation");
                    break;
                default:
                    verText = GetText(4601, "Lager");
                    break;
            }
            if (!string.IsNullOrEmpty(extraText))
            {
                verText += ", " + extraText;
            }
            var serie = accountingInfo.voucherSeries;

            // Create voucher head
            var voucherHead = new VoucherHead
            {
                AccountPeriodId = accountingInfo.accPeriod.AccountPeriodId,
                VoucherSeries = serie,
                VoucherNr = serie.VoucherNrLatest.GetValueOrDefault() + 1,
                Date = transDate.Date.Date,
                Text = verText,
                Template = false,
                VatVoucher = false,
                Status = (int)TermGroup_AccountStatus.Open,
                ActorCompanyId = actorCompanyId,
            };
            serie.VoucherNrLatest = voucherHead.VoucherNr;

            SetCreatedProperties(voucherHead);
            entities.VoucherHead.AddObject(voucherHead);

            SaveChanges(entities, trans);

            int voucherRowNr = 1;
            foreach (var stTrans in stockTransList)
            {
                var stockProduct = GetStockProduct(entities, stTrans.StockProductId, true, true, true);
                var product = stockProduct.InvoiceProduct;
                List<AccountInternal> accountInternals = null;
                if ((stTrans.ActionType == (int)TermGroup_StockTransactionType.Add || stTrans.ActionType == (int)TermGroup_StockTransactionType.Take) && stTrans.InvoiceRowId.GetValueOrDefault() > 0)

                {
                    accountInternals = AddAccountInternalsFromInvoice(entities, stTrans.InvoiceRowId.Value, product, actorCompanyId);
                }

                stTrans.VoucherId = voucherHead.VoucherHeadId;

                decimal amount = 0;
                if (stTrans.Price != 0)
                {
                    amount = (stTrans.Price * stTrans.Quantity);
                }
                else
                {
                    amount = (stockProduct.AvgPrice * stTrans.Quantity);
                }
                var rowText = GetText(1867, "artikelnr") + ": " + product.Number;

                // rounding
                amount = Decimal.Round(amount, 2);

                //debit row
                var voucherRowDeb = CreateVoucherRowForTransactionType(entities, voucherHead, stockProduct.Stock, product, (TermGroup_StockTransactionType)stTrans.ActionType, false, userId, actorCompanyId, actionType);
                voucherRowDeb.Quantity = stTrans.Quantity;
                voucherRowDeb.Amount = amount;
                voucherRowDeb.AmountEntCurrency = amount;
                voucherRowDeb.Text = rowText;
                voucherRowDeb.RowNr = voucherRowNr;
                if (!accountInternals.IsNullOrEmpty())
                {
                    if (voucherRowDeb.AccountInternal == null)
                        voucherRowDeb.AccountInternal = new EntityCollection<AccountInternal>();
                    voucherRowDeb.AccountInternal.AddRange(accountInternals);
                }

                //special for piratförlaget until we have account settings on productgroups
                if ((actorCompanyId == 2091986 || actorCompanyId == 2092485 || actorCompanyId == 2091990) && product.ProductGroupId != null)
                {
                    AccountDimDTO accountDimStdDTO = AccountManager.GetAccountDimStd(entities, actorCompanyId).ToDTO();
                    AccountDim accountDimStd = AccountManager.GetAccountDimStd(entities, actorCompanyId);
                    Account piratAccount = null;
                    switch (product.ProductGroup.Code)
                    {
                        case "10":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debtering inbunder
                                piratAccount = AccountManager.GetAccountByNr(entities, "4020", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering inbunder
                                piratAccount = AccountManager.GetAccountByNr(entities, "6190", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "20":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debitering krt/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "4025", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering krt/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "6191", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "30":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debitering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "4026", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "6192", accountDimStd.AccountDimId, actorCompanyId);
                                break;
                            }
                            break;
                        case "40":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debitering inbundet
                                piratAccount = AccountManager.GetAccountByNr(entities, "4020", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering inbundet
                                piratAccount = AccountManager.GetAccountByNr(entities, "6190", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "50":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debitering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "4027", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "6193", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "60":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debitering kart/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "4025", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering kart/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "6191", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "70":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debitering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "4026", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "6192", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "80":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Take)
                            {
                                //PIRAT Debitering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "4027", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "6193", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                            //                 case "90":                            // accountNumber = ""; break;  //används ej
                    }
                    if (piratAccount != null)
                    {
                        voucherRowDeb.AccountId = piratAccount.AccountId;
                    }
                }

                entities.VoucherRow.AddObject(voucherRowDeb);
                voucherRowNr++;

                var voucherRowCredit = CreateVoucherRowForTransactionType(entities, voucherHead, stockProduct.Stock, product, (TermGroup_StockTransactionType)stTrans.ActionType, true, userId, actorCompanyId, actionType);
                voucherRowCredit.Quantity = stTrans.Quantity * -1;
                voucherRowCredit.Amount = amount * -1;
                voucherRowCredit.AmountEntCurrency = amount * -1;
                voucherRowCredit.Text = rowText;
                voucherRowCredit.RowNr = voucherRowNr;

                if (!accountInternals.IsNullOrEmpty())
                {
                    if (voucherRowCredit.AccountInternal == null)
                        voucherRowCredit.AccountInternal = new EntityCollection<AccountInternal>();
                    voucherRowCredit.AccountInternal.AddRange(accountInternals);
                }

                if ((actorCompanyId == 2091986 || actorCompanyId == 2092485 || actorCompanyId == 2091990) && product.ProductGroupId != null)
                {
                    AccountDimDTO accountDimStdDTO = AccountManager.GetAccountDimStd(entities, actorCompanyId).ToDTO();
                    AccountDim accountDimStd = AccountManager.GetAccountDimStd(entities, actorCompanyId);
                    Account piratAccount = null;
                    switch (product.ProductGroup.Code)
                    {
                        case "10":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debtering inbunder
                                piratAccount = AccountManager.GetAccountByNr(entities, "4020", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering inbunder
                                piratAccount = AccountManager.GetAccountByNr(entities, "6190", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "20":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debitering krt/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "4025", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering krt/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "6191", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "30":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debitering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "4026", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "6192", accountDimStd.AccountDimId, actorCompanyId);
                                break;
                            }
                            break;
                        case "40":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debitering inbundet
                                piratAccount = AccountManager.GetAccountByNr(entities, "4020", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering inbundet
                                piratAccount = AccountManager.GetAccountByNr(entities, "6190", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "50":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debitering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "4027", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "6193", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "60":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debitering kart/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "4025", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering kart/storp
                                piratAccount = AccountManager.GetAccountByNr(entities, "6191", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "70":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debitering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "4026", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering pocket
                                piratAccount = AccountManager.GetAccountByNr(entities, "6192", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                        case "80":
                            if (stTrans.ActionType == (int)TermGroup_StockTransactionType.Add)
                            {
                                //PIRAT Debitering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "4027", accountDimStdDTO.AccountDimId, actorCompanyId);
                            }
                            else
                            {
                                //PIRAT Kreditering CD/mp3
                                piratAccount = AccountManager.GetAccountByNr(entities, "6193", accountDimStd.AccountDimId, actorCompanyId);
                            }
                            break;
                            //                 case "90":                            // accountNumber = ""; break;  //används ej
                    }
                    if (piratAccount != null)
                    {
                        voucherRowCredit.AccountId = piratAccount.AccountId;
                    }
                }
                entities.VoucherRow.AddObject(voucherRowCredit);
                voucherRowNr++;
            }

            SaveChanges(entities, trans);

            return voucherHead.VoucherHeadId;
        }

        public int CreateVoucherFromTransaction(CompEntities entities, TransactionScope trans, StockTransaction stTrans, int userId, int actorCompanyId, bool saveChanges)
        {
            var transDate = stTrans.TransactionDate.Date;
            transDate = transDate.Date;

            int serieId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.StockDefaultVoucherSeriesType, userId, actorCompanyId, 0);
            var accYearId = AccountManager.GetAccountYearId(entities, transDate, actorCompanyId);

            //no account year found for transDate!
            if (accYearId == 0)
                throw new Exception(GetText(1281, "Redovisningsår hittades inte") + ": " + transDate.ToShortDateString());

            var accPeriod = AccountManager.GetAccountPeriod(entities, accYearId, transDate, actorCompanyId);

            //no account period found for transDate!
            if (accPeriod == null)
                throw new Exception(GetText(8693, "Period hittades inte") + ": " + transDate.ToShortDateString());

            var serie = VoucherManager.GetVoucherSerieByType(entities, serieId, accYearId);

            var stockProduct = GetStockProduct(entities, stTrans.StockProductId, false, false, true);

            var product = ProductManager.GetInvoiceProduct(entities, stockProduct.InvoiceProductId, false, true, false);

            string verText;
            switch (stTrans.ActionType)
            {
                case (int)TermGroup_StockTransactionType.Correction:
                    verText = GetText(4642, "Inventering");
                    break;
                case (int)TermGroup_StockTransactionType.Add:
                    verText = GetText(4788, "Lager, inleverans");
                    break;
                default:
                    verText = GetText(4601, "Lager");
                    break;
            }

            // Create voucher head
            var voucherHead = new VoucherHead
            {
                AccountPeriodId = accPeriod.AccountPeriodId,
                VoucherSeries = serie,
                VoucherNr = serie.VoucherNrLatest.GetValueOrDefault() + 1,
                Date = transDate,
                Text = verText + ", " + GetText(1867, "artikelnr") + ": " + product.Number,
                Template = false,
                VatVoucher = false,
                Status = (int)TermGroup_AccountStatus.Open,
                ActorCompanyId = actorCompanyId,
            };
            serie.VoucherNrLatest = voucherHead.VoucherNr;

            SetCreatedProperties(voucherHead);
            entities.VoucherHead.AddObject(voucherHead);

            if (saveChanges)
                SaveChanges(entities, trans);
            //Get accounts for action type
            stTrans.VoucherId = voucherHead.VoucherHeadId;

            var voucherRowDeb = CreateVoucherRowForTransactionType(entities, voucherHead, stockProduct.Stock, product, (TermGroup_StockTransactionType)stTrans.ActionType, false, userId, actorCompanyId);
            voucherRowDeb.Quantity = stTrans.Quantity;
            voucherRowDeb.Amount = stTrans.Price * stTrans.Quantity;
            voucherRowDeb.AmountEntCurrency = stTrans.Price * stTrans.Quantity;
            entities.VoucherRow.AddObject(voucherRowDeb);

            var voucherRowCredit = CreateVoucherRowForTransactionType(entities, voucherHead, stockProduct.Stock, product, (TermGroup_StockTransactionType)stTrans.ActionType, true, userId, actorCompanyId);
            voucherRowCredit.Quantity = stTrans.Quantity * -1;
            voucherRowCredit.Amount = (stTrans.Price * stTrans.Quantity) * -1;
            voucherRowCredit.AmountEntCurrency = (stTrans.Price * stTrans.Quantity) * -1;
            entities.VoucherRow.AddObject(voucherRowCredit);

            //Set reference
            stTrans.VoucherHead = voucherHead;

            if (saveChanges)
                SaveChanges(entities, trans);

            return stTrans.StockProductId;
        }

        private VoucherRow CreateVoucherRowForTransactionType(CompEntities entities, VoucherHead head, Stock stock, InvoiceProduct product, TermGroup_StockTransactionType actionType, bool isCredit, int userId, int actorCompanyId, TermGroup_StockTransactionType mainActionType = TermGroup_StockTransactionType.Unknown)
        {
            var voucherRow = new VoucherRow
            {
                VoucherHead = head,
                Date = head.Date,
                Merged = false,
                State = 0,
            };

            ProductAccountType productAccountType = ProductAccountType.Unknown;
            CompanySettingType companySettingType = CompanySettingType.Unknown;

            switch (actionType)
            {
                case TermGroup_StockTransactionType.Take:
                    productAccountType = isCredit ? ProductAccountType.StockOut : ProductAccountType.StockOutChange;
                    companySettingType = isCredit ? CompanySettingType.AccountStockOut : CompanySettingType.AccountStockOutChange;
                    break;
                case TermGroup_StockTransactionType.Add:
                    productAccountType = isCredit ? ProductAccountType.StockInChange : ProductAccountType.StockIn;
                    companySettingType = isCredit ? CompanySettingType.AccountStockInChange : CompanySettingType.AccountStockIn;
                    break;
                case TermGroup_StockTransactionType.Correction:
                    productAccountType = isCredit ? ProductAccountType.StockInvChange : ProductAccountType.StockInv;
                    companySettingType = isCredit ? CompanySettingType.AccountStockInventoryChange : CompanySettingType.AccountStockInventory;
                    break;
                case TermGroup_StockTransactionType.Loss:
                    productAccountType = isCredit ? ProductAccountType.StockLossChange : ProductAccountType.StockLoss;
                    companySettingType = isCredit ? CompanySettingType.AccountStockLoss : CompanySettingType.AccountStockLossChange;
                    break;
            }


            if (mainActionType == TermGroup_StockTransactionType.StockTransfer &&
                (
                actionType == TermGroup_StockTransactionType.Take && !isCredit ||
                actionType == TermGroup_StockTransactionType.Add && isCredit
                ))
            {
                var transferAccountsAdded = AddAccountsByPrio(voucherRow, stock, product, ProductAccountType.StockTransferChange);
                if (transferAccountsAdded && voucherRow.AccountId == 0)
                {
                    var transferChangeAccount = AccountManager.GetAccount(entities, actorCompanyId, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountStockTransferChange, userId, actorCompanyId, 0));
                    if (transferChangeAccount != null)
                    {
                        voucherRow.AccountId = transferChangeAccount.AccountId;
                    }
                }

                if (voucherRow.AccountId != 0)
                {
                    return voucherRow;
                }
            }

            if (AddAccountsByPrio(voucherRow, stock, product, productAccountType))
            {
                if (voucherRow.AccountId != 0)
                {
                    return voucherRow;
                }
            }

            //Standard accounts
            Account accountStock = AccountManager.GetAccount(entities, actorCompanyId, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)companySettingType, userId, actorCompanyId, 0));

            if (accountStock != null)
            {
                voucherRow.AccountId = accountStock.AccountId;
                return voucherRow;
            }
            else
                throw new Exception("Failed finding account for inventory transaction:" + actionType.ToString());
        }

        private bool AddAccountsByPrio(VoucherRow voucherRow, Stock stock, InvoiceProduct product, ProductAccountType type)
        {
            #region Account from product
            bool found = false;
            if (!product.ProductAccountStd.IsLoaded)
            {
                product.ProductAccountStd.Load();
            }


            if (product.ProductAccountStd.Any())
            {
                var productAccount = product.ProductAccountStd.FirstOrDefault(x => x.Type == (int)type);
                if (productAccount != null)
                {
                    found = true;
                    if (!productAccount.AccountStdReference.IsLoaded)
                    {
                        productAccount.AccountStdReference.Load();
                    }

                    if (!productAccount.AccountInternal.IsLoaded)
                    {
                        productAccount.AccountInternal.Load();
                    }

                    if (productAccount.AccountStd != null)
                    {
                        foreach (var internalAccount in productAccount.AccountInternal)
                        {
                            voucherRow.AccountInternal.Add(internalAccount);
                        }

                        voucherRow.AccountId = productAccount.AccountStd.AccountId;
                    }
                }
            }

            #endregion

            #region Account from stock

            if (stock.StockAccountStd.Any())
            {
                var stockAccount = stock.StockAccountStd.FirstOrDefault(x => x.Type == (int)type);
                if (stockAccount != null)
                {
                    found = true;
                    if (!stockAccount.AccountStdReference.IsLoaded)
                    { stockAccount.AccountStdReference.Load(); }

                    if (!stockAccount.AccountInternal.IsLoaded)
                    { stockAccount.AccountInternal.Load(); }

                    foreach (var internalAccount in stockAccount.AccountInternal)
                    {
                        voucherRow.AccountInternal.Add(internalAccount);
                    }

                    if (stockAccount.AccountStd != null)
                        voucherRow.AccountId = stockAccount.AccountStd.AccountId;
                }
            }

            #endregion

            return found;
        }
        #endregion

        #region Saldo

        public ActionResult MoveSaldoFromStock(int actorCompanyId, int invoiceProductId, int fromStockId, int toStockId, decimal quantity)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                var manuallyUpdateAvgPrice = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingManuallyUpdatedAvgPrices, 0, base.ActorCompanyId, 0);

                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        //Get stockproducts
                        StockProduct stockProductFrom = (from p in entities.StockProduct
                                                         where p.InvoiceProductId == invoiceProductId && p.StockId == fromStockId
                                                         select p).FirstOrDefault();

                        StockProduct stockProductTo = (from p in entities.StockProduct
                                                       where p.InvoiceProductId == invoiceProductId && p.StockId == toStockId
                                                       select p).FirstOrDefault();

                        //Create transactions for from and to stockproduct
                        StockTransaction stockTransactionFrom = new StockTransaction
                        {
                            //allways new
                            StockProductId = stockProductFrom.StockProductId,
                            Quantity = quantity,
                            ActionType = (int)TermGroup_StockTransactionType.Take,
                            Price = stockProductFrom.AvgPrice,
                            Note = string.Empty
                        };

                        entities.StockTransaction.AddObject(stockTransactionFrom);
                        SetCreatedProperties(stockTransactionFrom);

                        StockTransaction stockTransactionTo = new StockTransaction
                        {
                            //allways new
                            StockProductId = stockProductTo.StockProductId,
                            Quantity = quantity,
                            ActionType = (int)TermGroup_StockTransactionType.Add,
                            Price = stockProductFrom.AvgPrice,
                            Note = string.Empty
                        };

                        if (stockProductTo.AvgPrice == 0)
                        {
                            //Update only if price originally 0
                            stockProductTo.AvgPrice = stockProductFrom.AvgPrice;
                            stockTransactionTo.Price = stockProductTo.AvgPrice;
                        }

                        entities.StockTransaction.AddObject(stockTransactionTo);
                        SetCreatedProperties(stockTransactionTo);

                        // Update avg.price for stockProduktTo
                        if (!manuallyUpdateAvgPrice)
                            stockProductTo.AvgPrice = ((stockProductTo.AvgPrice * stockProductTo.Quantity) + (stockTransactionTo.Price * quantity)) / (stockProductTo.Quantity + quantity);

                        //Update saldos
                        stockProductFrom.Quantity = stockProductFrom.Quantity - quantity;
                        stockProductTo.Quantity = stockProductTo.Quantity + quantity;

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        // Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }

            return result;
        }

        private ActionResult UpdateInvoiceProductStockSaldo(CompEntities entities, TransactionScope trans, StockTransaction stockTransaction, bool saveChanges, decimal reservedQuantity = 0, decimal orderQuantity = 0)
        {
            ActionResult result = new ActionResult(false);
            decimal newAvgPrice = 0;

            StockProduct productToUpdate = GetStockProduct(entities, stockTransaction.StockProductId);
            if (productToUpdate != null)
            {
                var manuallyUpdateAvgPrice = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingManuallyUpdatedAvgPrices, 0, base.ActorCompanyId, 0);

                //AvgPrice update...
                if (!manuallyUpdateAvgPrice && stockTransaction.ActionType == (int)TermGroup_StockTransactionType.Add)
                {
                    //AvgPrice only if addition
                    if (productToUpdate.AvgPrice <= 0 || productToUpdate.Quantity <= 0)
                    {
                        //the set the avgprice same as newly purchased
                        productToUpdate.AvgPrice = stockTransaction.Price;
                    }
                    else
                    {
                        productToUpdate.AvgPrice = Math.Round(((productToUpdate.AvgPrice * productToUpdate.Quantity) + (stockTransaction.Price * stockTransaction.Quantity)) / (productToUpdate.Quantity + stockTransaction.Quantity), 2);
                        //let client know about new price to show in UI
                        newAvgPrice = productToUpdate.AvgPrice;
                    }
                }
                else if (stockTransaction.ActionType == (int)TermGroup_StockTransactionType.AveragePriceChange)
                {
                    productToUpdate.AvgPrice = stockTransaction.Price;
                }

                //save new/old avg price for logging
                stockTransaction.AvgPrice = productToUpdate.AvgPrice;

                //Quantity update...
                switch ((TermGroup_StockTransactionType)stockTransaction.ActionType)
                {
                    case TermGroup_StockTransactionType.Reserve:
                        {
                            productToUpdate.OrderedQuantity = productToUpdate.OrderedQuantity + orderQuantity;
                            productToUpdate.ReservedQuantity = productToUpdate.ReservedQuantity + reservedQuantity;
                            break;
                        }
                    case TermGroup_StockTransactionType.Take:
                        {
                            productToUpdate.Quantity = productToUpdate.Quantity - stockTransaction.Quantity;
                            break;
                        }
                    case TermGroup_StockTransactionType.Add:
                        {
                            productToUpdate.Quantity = productToUpdate.Quantity + stockTransaction.Quantity;
                            break;
                        }
                    case TermGroup_StockTransactionType.Correction:
                        {
                            if (stockTransaction.Quantity < 0)
                                productToUpdate.Quantity = productToUpdate.Quantity - Math.Abs(stockTransaction.Quantity);
                            else
                                productToUpdate.Quantity = productToUpdate.Quantity + stockTransaction.Quantity;
                            break;
                        }
                    case TermGroup_StockTransactionType.Loss:
                        {
                            productToUpdate.Quantity = productToUpdate.Quantity - stockTransaction.Quantity;
                            break;
                        }
                }
                if (saveChanges)
                    result = SaveChanges(entities, trans);
                else
                    result.Success = true;
            }

            result.DecimalValue = newAvgPrice;
            return result;
        }

        public ActionResult RecalculateStockBalance(int actorCompanyId, int inStockId)
        {
            var result = new ActionResult();

            try
            {
                using (var entities = new CompEntities())
                {
                    var invoiceAttestStates = new InvoiceAttestStates(entities, SoeOriginType.Order, SettingManager, AttestManager, actorCompanyId, UserId, false);
                    var klarState = invoiceAttestStates.OrderLockedIds.FirstOrDefault(x => x != invoiceAttestStates.TransferredOrderToInvoiceId && x != invoiceAttestStates.DeliverOrderToStockId);
                    var regState = invoiceAttestStates.OrderInitialId;

                    if (inStockId > 0)
                    {
                        entities.RecalculateStockBalance(actorCompanyId, inStockId, regState, klarState);
                    }
                    else
                    {
                        var stocks = GetStocks(entities, actorCompanyId);

                        foreach (var stock in stocks)
                        {
                            entities.RecalculateStockBalance(actorCompanyId, stock.StockId, regState, klarState);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }
            return result;
        }

        #endregion

        #region Purchase

        public List<StockProductDTO> GetStockProductDTOs(CompEntities entities, int actorCompanyId, List<int> stockPlaceIds, bool excludeMissingTriggerQuantity, bool excludeMissingPurchaseQuantity, string productNrFrom, string productNrTo)
        {
            var query = GetStockProductDTOsWithSaldo(entities, actorCompanyId, false).AsQueryable();

            if (stockPlaceIds != null && stockPlaceIds.Any())
            {
                query = query.Where(r => stockPlaceIds.Contains(r.StockId));
            }
            if (excludeMissingTriggerQuantity)
            {
                query = query.Where(r => r.PurchaseTriggerQuantity != 0);
            }
            if (excludeMissingPurchaseQuantity)
            {
                query = query.Where(r => r.PurchaseQuantity != 0);
            }
            if (!string.IsNullOrEmpty(productNrFrom))
            {
                query = query.Where(r => string.Compare(productNrFrom, r.ProductNumber) <= 0);
            }
            if (!string.IsNullOrEmpty(productNrTo))
            {
                query = query.Where(r => string.Compare(productNrTo, r.ProductNumber) >= 0);
            }

            return query.ToList();
        }

        public List<PurchaseRowFromStockDTO> GetStockPurchaseSugggestion(int actorCompanyId, GenerateStockPurchaseSuggestionDTO dto)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetStockPurchaseSugggestion(entities, actorCompanyId, dto);
        }
        public List<PurchaseRowFromStockDTO> GetStockPurchaseSugggestion(CompEntities entities, int actorCompanyId, GenerateStockPurchaseSuggestionDTO dto)
        {
            var result = new List<PurchaseRowFromStockDTO>();

            var stockProducts = GetStockProductDTOs(entities, actorCompanyId, dto.StockPlaceIds, dto.ExcludeMissingTriggerQuantity, dto.ExcludeMissingPurchaseQuantity, dto.ProductNrFrom, dto.ProductNrTo);

            var invoiceProductIds = stockProducts.Select(p => p.InvoiceProductId).ToList();
            var supplierProducts = SupplierProductManager.GetSupplierProductsByInvoiceProducts(entities, invoiceProductIds, actorCompanyId);
            var purchaseDate = DateTime.UtcNow;
            var currencies = CountryCurrencyManager.GetCompCurrenciesDict(actorCompanyId, false, false);

            var adjustmentDecimal = decimal.Round((dto.TriggerQuantityPercent + 100), 2) / 100;

            var extendedInvoiceProductData = entities.Product.OfType<InvoiceProduct>()
                .Where(r => invoiceProductIds.Contains(r.ProductId))
                .Select(r => new StockProductExtension
                {
                    ProductId = r.ProductId,
                    VatCodeId = r.VatCodeId ?? 0,
                    ProductUnitId = r.ProductUnitId ?? 0,
                    Code = r.ProductUnit.Code
                })
                .ToDictionary(r => r.ProductId);

            decimal GetAdjustedTriggerQuantity(PurchaseRowFromStockDTO row)
            {
                return decimal.Round(row.StockPurchaseTriggerQuantity * adjustmentDecimal, 2);
            }

            void CalculateQuantity(PurchaseRowFromStockDTO row)
            {
                switch (dto.PurchaseGenerationType)
                {
                    case TermGroup_StockPurchaseGenerationOptions.TotalStockCompareToTriggerQuantity:
                        row.Quantity = row.TotalStockQuantity <= GetAdjustedTriggerQuantity(row) ? row.StockPurchaseQuantity : 0;
                        break;
                    case TermGroup_StockPurchaseGenerationOptions.AvailableStockCompareToTriggerQuantity:
                        row.Quantity = row.AvailableStockQuantity <= GetAdjustedTriggerQuantity(row) ? row.StockPurchaseQuantity : 0;
                        break;
                    case TermGroup_StockPurchaseGenerationOptions.PurchaseQuantity:
                        row.Quantity = row.StockPurchaseQuantity;
                        break;
                }
                if (row.Quantity < 0) row.Quantity = 0;
            }


            int counter = 0;
            foreach (var stockProduct in stockProducts)
            {
                //Get leadtime from proce   dure...

                var row = new PurchaseRowFromStockDTO()
                {
                    TempId = counter++,
                    ProductId = stockProduct.InvoiceProductId,
                    ProductNr = stockProduct.ProductNumber,
                    ProductName = stockProduct.ProductName,
                    StockId = stockProduct.StockId,
                    StockName = stockProduct.StockName,
                    StockPurchaseQuantity = stockProduct.PurchaseQuantity,
                    StockPurchaseTriggerQuantity = stockProduct.PurchaseTriggerQuantity,
                    TotalStockQuantity = stockProduct.Quantity,
                    AvailableStockQuantity = stockProduct.Quantity - stockProduct.ReservedQuantity,
                    PurchasedQuantity = stockProduct.PurchasedQuantity,
                    DeliveryAddress = dto.DefaultDeliveryAddress,
                    ExclusivePurchase = false,
                    ReferenceOur = dto.Purchaser
                };

                if (extendedInvoiceProductData.TryGetValue(stockProduct.InvoiceProductId, out StockProductExtension extendedInfo))
                {
                    row.UnitId = extendedInfo.ProductUnitId;
                    row.UnitCode = extendedInfo.Code;
                    row.VatCodeId = extendedInfo.VatCodeId;
                }

                CalculateQuantity(row);

                if (dto.ExcludePurchaseQuantityZero && row.Quantity == 0)
                    continue;

                var supplierProductMatches = supplierProducts.Where(s => s.ProductId == stockProduct.InvoiceProductId).ToList();
                var supplierProduct = supplierProductMatches.FirstOrDefault();
                if (supplierProduct != null)
                {
                    row.SupplierId = supplierProduct.SupplierId;
                    row.SupplierName = supplierProduct.SupplierName;
                    row.SupplierProductId = supplierProduct.SupplierProductId;
                    row.SupplierUnitId = supplierProduct.SupplierProductUnitId;
                    row.SupplierUnitCode = supplierProduct.SupplierProductUnitCode;
                    row.DeliveryLeadTimeDays = supplierProduct.DeliveryLeadTimeDays ?? row.DeliveryLeadTimeDays;
                    row.MultipleSupplierMatches = supplierProductMatches.Count > 1;

                    var price = SupplierProductManager.GetSupplierProductPrice(entities, supplierProduct.SupplierProductId, purchaseDate, row.Quantity, 0);
                    if (price != null)
                    {
                        row.Price = price.Price;
                        row.CurrencyId = price.CurrencyId;
                        if (currencies.TryGetValue(row.CurrencyId, out string currencyCode))
                            row.CurrencyCode = currencyCode;
                    }
                }

                row.DiscountPercentage = 0;
                row.RequestedDeliveryDate = DateTime.Now.AddDays(row.DeliveryLeadTimeDays);
                result.Add(row);
            }

            return result;
        }
        #endregion

        #region Import

        private sealed class StockImportItem
        {
            public InvoiceProduct Product;
            public decimal Quantity = 0;
            public decimal Price = 0;
            public string ShelfCode;
        }

        private List<StockImportItem> ImportInvoiceProductsFromFile(int actorCompanyId, int wholeSellerId, List<byte[]> fileData)
        {
            string stringConvert = System.Text.Encoding.UTF8.GetString(fileData[0]);
            var currentLocale = System.Globalization.CultureInfo.CurrentCulture;
            var saveChanges = false;
            var unitCache = new Dictionary<string, int>();

            var stockImportItems = new List<StockImportItem>();
            using (var entities = new CompEntities())
            {
                using (var reader = new StringReader(stringConvert))
                {
                    string line;
                    var syswholeSellerId = wholeSellerId;
                    int sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListEx(entities, actorCompanyId, ref syswholeSellerId);

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!line.StartsWith("Artikel"))
                        {
                            var item = line.Split(';');

                            if (item.Length < 3)
                                continue;

                            var productNr = item[0];
                            if (string.IsNullOrEmpty(productNr))
                                continue;

                            decimal quantity = 0;
                            decimal price = 0;

                            item[1] = item[1].Replace(".", currentLocale.NumberFormat.NumberDecimalSeparator);
                            if (!decimal.TryParse(item[1], out quantity))
                            {
                                throw new Exception("Failed reading quantity for product: " + productNr);
                            }

                            item[2] = item[2].Replace(".", currentLocale.NumberFormat.NumberDecimalSeparator);
                            if (!decimal.TryParse(item[2], out price))
                            {
                                throw new Exception("Failed reading price for product: " + productNr);
                            }

                            var invoiceProduct = ProductManager.GetInvoiceProductByProductNr(entities, productNr, actorCompanyId);
                            if (invoiceProduct == null)
                            {
                                int sysProductId = 0;
                                invoiceProduct = ProductManager.CopyExternalInvoiceProductFromSysByProductNr(entities, sysProductId, productNr, price, syswholeSellerId, null, sysPriceListHeadId, null, actorCompanyId, 0 /*customer.ActorCustomerId*/, true);
                            }

                            if ((invoiceProduct == null) || (invoiceProduct.Number != productNr))
                            {
                                throw new Exception(GetText(8331, "Artikel kunde inte hittas") + ":" + productNr);
                            }

                            if (invoiceProduct != null && invoiceProduct.ProductUnitId.GetValueOrDefault() == 0 && item.Length > 3)
                            {
                                var productUnitStr = item[3];
                                if (!string.IsNullOrEmpty(productUnitStr))
                                {
                                    int productUnitId = 0;
                                    if (!unitCache.TryGetValue(productUnitStr, out productUnitId))
                                    {
                                        var productUnit = ProductManager.GetProductUnit(productUnitStr, actorCompanyId);
                                        if (productUnit == null)
                                        {
                                            productUnit = ProductManager.AddProductUnit(entities, productUnitStr, productUnitStr, actorCompanyId);
                                        }

                                        if (productUnit != null)
                                        {
                                            productUnitId = productUnit.ProductUnitId;
                                            unitCache.Add(productUnitStr, productUnitId);
                                        }
                                    }

                                    if (productUnitId > 0)
                                    {
                                        invoiceProduct.ProductUnitId = productUnitId;
                                        saveChanges = true;
                                    }
                                }
                            }

                            var importItem = new StockImportItem { Product = invoiceProduct, Price = price, Quantity = quantity };

                            if (item.Length > 3)
                            {
                                importItem.ShelfCode = item[3];
                            }

                            stockImportItems.Add(importItem);
                        }
                    }
                }

                if (saveChanges)
                {
                    entities.SaveChanges();
                }
            }
            return stockImportItems;
        }

        public ActionResult ImportStockBalances(int actorCompanyId, int wholeSellerId, int stockId, string fileName, List<byte[]> fileData)
        {
            var result = new ActionResult();

            //Make sure cache is set...
            var wholeseller = SysPriceListManager.GetSysWholesellerFromCache(wholeSellerId);

            var stockTransactions = new List<StockTransaction>();

            List<StockImportItem> importItems;
            try
            {
                importItems = ImportInvoiceProductsFromFile(actorCompanyId, wholeSellerId, fileData);
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                return new ActionResult(false, 0, ex.Message);
            }

            if (importItems.Count == 0)
            {
                return new ActionResult(GetText(11890, "Inget data skickades"));
            }

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        //var timer = Stopwatch.StartNew();

                        // Voucher setting
                        var createVoucherSetting = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingCreateVouchersForStockTransactions, 0, base.ActorCompanyId, 0, true);

                        foreach (var importItem in importItems)
                        {
                            var invoiceProduct = importItem.Product;
                            var quantity = importItem.Quantity;
                            var price = importItem.Price;

                            if (!invoiceProduct.IsStockProduct.HasValue || (invoiceProduct.IsStockProduct.HasValue && !invoiceProduct.IsStockProduct.Value))
                            {
                                invoiceProduct.IsStockProduct = true;
                            }

                            StockShelf stockShelf = null;
                            if (!string.IsNullOrEmpty(importItem.ShelfCode))
                            {
                                stockShelf = GetStockShelfByCode(entities, stockId, importItem.ShelfCode);
                                if (stockShelf == null)
                                {
                                    stockShelf = new StockShelf { Code = importItem.ShelfCode, Name = importItem.ShelfCode, StockId = stockId };
                                    SaveStockShelf(entities, stockShelf);
                                }
                            }

                            StockProduct stockProduct = this.GetAddStockProductFromInvoiceProduct(entities, transaction, invoiceProduct.ProductId, stockId, actorCompanyId, price, stockShelf == null ? 0 : stockShelf.StockShelfId);

                            if (stockProduct == null)
                            {
                                return new ActionResult(GetText(7533, "Lagerartikel saknas") + ": " + invoiceProduct.Number);
                            }

                            if (stockProduct.Quantity != 0)
                            {
                                //Only take or add the difference 
                                quantity = quantity - stockProduct.Quantity;
                            }

                            //Add the new imported balance
                            if (quantity != 0)
                            {
                                var stockTransactionDTO = new StockTransactionDTO()
                                {
                                    StockTransactionId = 0, //allways new
                                    StockProductId = stockProduct.StockProductId,
                                    StockShelfId = stockShelf == null ? 0 : stockShelf.StockShelfId,
                                    Quantity = Math.Abs(quantity),
                                    Price = price,
                                    ActionType = quantity > 0 ? TermGroup_StockTransactionType.Add : TermGroup_StockTransactionType.Take,
                                    ReservedQuantity = 0,
                                    Note = $"Imported from file:{fileName}"
                                };

                                result = StockManager.SaveStockTransaction(entities, transaction, stockTransactionDTO, actorCompanyId, true, false);

                                if (result.Success)
                                {
                                    stockTransactions.Add((StockTransaction)result.Value);
                                }
                                else
                                {
                                    return result;
                                }
                            }

                            stockProduct.AvgPrice = price;
                        }
                        /*
                        timer.Stop();
                        var timespan = timer.Elapsed;
                        Debug.Print(string.Format("stock {0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10));
                        */
                        var addStockTransactions = stockTransactions.Where(x => x.ActionType == (int)TermGroup_StockTransactionType.Add).ToList();
                        if (addStockTransactions.Any() && createVoucherSetting)
                        {
                            //timer = Stopwatch.StartNew();
                            var accountInfo = GetAccountingInfo(entities, actorCompanyId, DateTime.Now, base.UserId);
                            CreateVoucherFromTransactions(entities, transaction, addStockTransactions, base.UserId, actorCompanyId, TermGroup_StockTransactionType.Add, DateTime.Now, null, accountInfo);
                            //timer.Stop();
                            //timespan = timer.Elapsed;
                            //Debug.Print(string.Format("ver {0:00}:{1:00}:{2:00}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds / 10));
                        }

                        var takeStockTransactions = stockTransactions.Where(x => x.ActionType == (int)TermGroup_StockTransactionType.Take).ToList();
                        if (takeStockTransactions.Any() && createVoucherSetting)
                        {
                            CreateVoucherFromTransactions(entities, transaction, takeStockTransactions, base.UserId, actorCompanyId, TermGroup_StockTransactionType.Take, DateTime.Now, null);
                        }

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            transaction.Complete();
                        }

                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        result.IntegerValue2 = importItems.Count;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }

        public ActionResult ImportStockInventory(int actorCompanyId, int stockInventoryHeadId, string fileName, List<byte[]> fileData)
        {
            if (fileData.Count == 0 || fileData[0].Length == 0)
            {
                return new ActionResult(GetText(11890, "Inget data skickades"));
            }

            var result = new ActionResult();
            string stringConvert = System.Text.Encoding.UTF8.GetString(fileData[0]);
            var currentLocale = System.Globalization.CultureInfo.CurrentCulture;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var stockInventoryHead = this.GetStockInventory(entities, stockInventoryHeadId, true);

                        using (var reader = new StringReader(stringConvert))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (!line.StartsWith("Artikel"))
                                {
                                    var item = line.Split(';');

                                    if (item.Length != 3)
                                        continue;

                                    var productNr = item[0];
                                    if (string.IsNullOrEmpty(productNr))
                                        continue;

                                    decimal quantity = 0;
                                    decimal price = 0;

                                    item[1] = item[1].Replace(".", currentLocale.NumberFormat.NumberDecimalSeparator);
                                    if (!decimal.TryParse(item[1], out quantity))
                                    {
                                        return new ActionResult(false, 0, "Failed reading quantity for product: " + productNr);
                                    }

                                    item[2] = item[2].Replace(".", currentLocale.NumberFormat.NumberDecimalSeparator);
                                    if (!decimal.TryParse(item[2], out price))
                                    {
                                        return new ActionResult(false, 0, "Failed reading price for product: " + productNr);
                                    }

                                    var invoiceProduct = ProductManager.GetInvoiceProductByProductNr(entities, productNr, actorCompanyId);

                                    if (invoiceProduct == null)
                                    {
                                        return new ActionResult(false, 0, "Failed finding product with number: " + productNr);
                                    }

                                    StockProduct stockProduct = this.GetStockProductFromInvoiceProduct(entities, invoiceProduct.ProductId, (int)stockInventoryHead.StockId);

                                    var inventoryRow = stockInventoryHead.StockInventoryRow.FirstOrDefault(x => x.StockProductId == stockProduct.StockProductId);
                                    if (inventoryRow != null)
                                    {
                                        inventoryRow.Difference = quantity - inventoryRow.StartingSaldo;
                                        inventoryRow.InventorySaldo = quantity;
                                        SetModifiedProperties(inventoryRow);
                                    }
                                }
                            }
                            result = SaveChanges(entities, transaction);
                            if (result.Success)
                                transaction.Complete();

                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                finally
                {
                    if (result.Success)
                    {
                        // Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }
            }
            return result;
        }

        #endregion
    }
    //.Select(r => new { ProductId = r.ProductId, VatCodeId = r.VatCodeId ?? 0, ProductUnitId = r.ProductUnitId ?? 0, Code = r.ProductUnit.})

    public class StockProductExtension
    {
        public int ProductId { get; set; }
        public int VatCodeId { get; set; }
        public int ProductUnitId { get; set; }
        public string Code { get; set; }
    }
}
