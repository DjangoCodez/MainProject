using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.DTO;
using System.Data.Entity;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Util.Config;
using System.Transactions;
using System;

namespace SoftOne.Soe.Business.Core
{
    public class PurchaseDeliveryManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static private readonly string[] purchaseSaveTreat0asNullHead = { "deliverytypeid", "deliveryconditionid" };
        static private readonly string[] purchaseSaveExcludeFields = { "categoryids", "purchasenr", "projectnr", "created", "modified", "createdby", "modifiedby" };
        static private readonly string[] purchaseTreat0asNullRow = { "stockid", "vatcodeid", "purchaseunitid", "productid" };

        #endregion

        #region Ctor

        public PurchaseDeliveryManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public List<PurchaseDeliveryGridDTO> GetDeliveryForGrid(int allItemsSelection, int actorCompanyId, int? purchaseDeliveryId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseDelivery.NoTracking();
            return GetDeliveryForGrid(entities, allItemsSelection, actorCompanyId, purchaseDeliveryId);
        }

        public List<PurchaseDeliveryGridDTO> GetDeliveryForGrid(CompEntities entities, int allItemsSelection, int actorCompanyId, int? purchaseDeliveryId = null)
        {
            IQueryable<PurchaseDeliveryGridDTO> query = (from d in entities.PurchaseDelivery
                                                         where d.State == (int)SoeEntityState.Active && d.ActorCompanyId == actorCompanyId
                                                         select new PurchaseDeliveryGridDTO
                                                         {
                                                             PurchaseDeliveryId = d.PurchaseDeliveryId,
                                                             DeliveryNr = d.DeliveryNr,
                                                             DeliveryDate = d.DeliveryDate,
                                                             SupplierName = d.Supplier.Name,
                                                             SupplierNr = d.Supplier.SupplierNr,
                                                             Created = d.Created,
                                                             PurchaseNr = d.PurchaseDeliveryRow.Select(r => r.PurchaseRow.Purchase.PurchaseNr).FirstOrDefault()
                                                         });

            if (purchaseDeliveryId.HasValue)
                query = query.Where(pd => pd.PurchaseDeliveryId == purchaseDeliveryId);

            return query.ToList();
        }

        public PurchaseDelivery GetPurchaseDelivery(int purchaseDeliveryId, bool loadRows, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseDelivery.NoTracking();
            return GetPurchaseDelivery(entities, purchaseDeliveryId, loadRows, actorCompanyId);
        }

        public PurchaseDelivery GetPurchaseDelivery(CompEntities entities, int purchaseDeliveryId, bool loadRows, int actorCompanyId)
        {
            if (purchaseDeliveryId == 0)
                return null;
            
            IQueryable<PurchaseDelivery> query = (
                from i in entities.PurchaseDelivery
                where 
                    i.PurchaseDeliveryId == purchaseDeliveryId && i.ActorCompanyId == actorCompanyId && 
                    i.State == (int)SoeEntityState.Active
                select i);

            if (loadRows)
            {
                query = query.Include("PurchaseDeliveryRow");
            }

            var purchaseDelivery = query.FirstOrDefault();

            return purchaseDelivery;
        }

        public PurchaseDeliveryDTO GetPurchaseDeliveryDTO(int purchaseDeliveryId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseDelivery.NoTracking();
            return GetPurchaseDeliveryDTO(entities, purchaseDeliveryId, actorCompanyId);
        }

        public PurchaseDeliveryDTO GetPurchaseDeliveryDTO(CompEntities entities, int purchaseDeliveryId,int actorCompanyId)
        {
            if (purchaseDeliveryId == 0)
                return null;

            return (from p in entities.PurchaseDelivery
                    where p.PurchaseDeliveryId == purchaseDeliveryId && 
                        p.ActorCompanyId == actorCompanyId &&
                        p.State == (int)SoeEntityState.Active
                    select p)
                   .Select(EntityExtensions.GetPurchaseDeliveryDTO)
                   .FirstOrDefault();
        }

        public List<PurchaseDeliveryRowDTO> GetDeliveryRows(int purchaseDeliveryId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetPurchaseDeliveryRows(entities, purchaseDeliveryId, actorCompanyId);
        }

        public List<PurchaseDeliveryRowDTO> GetPurchaseDeliveryRows(CompEntities entities, int purchaseDeliveryId, int actorCompanyId)
        {
            if (purchaseDeliveryId == 0)
                return new List<PurchaseDeliveryRowDTO>();

            return (
                from 
                    r in entities.PurchaseDeliveryRow
                where 
                    r.PurchaseDeliveryId == purchaseDeliveryId && r.PurchaseDelivery.ActorCompanyId == actorCompanyId &&
                    r.State == (int)SoeEntityState.Active
                select new PurchaseDeliveryRowDTO
                {
                    PurchaseId = r.PurchaseRow.PurchaseId,
                    PurchaseDeliveryId = r.PurchaseDeliveryId,
                    PurchaseDeliveryRowId = r.PurchaseDeliveryRowId,
                    PurchaseRowId = r.PurchaseRowId,
                    DeliveredQuantity = r.DeliveredQuantity,
                    PurchaseQuantity = r.PurchaseRow.Quantity,
                    RemainingQuantity = r.PurchaseRow.Quantity - r.PurchaseRow.DeliveredQuantity ?? 0,
                    DeliveryDate = r.DeliveryDate,
                    ProductName = r.PurchaseRow.Product.Name,
                    ProductNr = r.PurchaseRow.Product.Number,
                    StockCode = r.PurchaseRow.Stock.Code,
                    PurchasePrice = r.PurchasePrice,
                    PurchasePriceCurrency = r.PurchasePriceCurrency,
                    PurchaseNr = r.PurchaseRow.Purchase.PurchaseNr,
                    IsLocked = r.PurchaseRow != null && r.PurchaseRow.Status == (int)SoeOriginStatus.PurchaseDeliveryCompleted,
                }
            ).ToList();
        }

        public List<PurchaseDeliveryRowDTO> GetPurchaseDeliveryRowsByPurchaseId(int purchaseId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetPurchaseDeliveryRowsByPurchaseId(entities, purchaseId, actorCompanyId);
        }

        public List<PurchaseDeliveryRowDTO> GetPurchaseDeliveryRowsByPurchaseId(CompEntities entities, int purchaseId,int actorCompanyId) {

            if (purchaseId == 0)
                return new List<PurchaseDeliveryRowDTO>();

            var lst = (
                from
                    r in entities.PurchaseDeliveryRow
                where
                    r.PurchaseRow.PurchaseId == purchaseId && r.PurchaseDelivery.ActorCompanyId == actorCompanyId &&
                    r.State == (int)SoeEntityState.Active
                select new PurchaseDeliveryRowDTO
                {
                    PurchaseDeliveryId = r.PurchaseDeliveryId,
                    PurchaseDeliveryRowId = r.PurchaseDeliveryRowId,
                    PurchaseRowId = r.PurchaseRowId,
                    DeliveredQuantity = r.DeliveredQuantity,
                    PurchaseQuantity = r.PurchaseRow.Quantity,
                    RemainingQuantity = r.PurchaseRow.Quantity - r.PurchaseRow.DeliveredQuantity ?? 0,
                    DeliveryDate = r.DeliveryDate,
                    ProductName = r.PurchaseRow.Product.Name,
                    ProductNr = r.PurchaseRow.Product.Number,
                    StockCode = r.PurchaseRow.Stock.Code,
                    PurchasePriceCurrency = r.PurchasePriceCurrency,
                    PurchaseNr = r.PurchaseRow.Purchase.PurchaseNr,
                    IsLocked = true,
                    State = SoeEntityState.Active
                }
            ).ToList();
            return lst;
        }

        public List<PurchaseDeliveryRowDTO> GetDeliveryRowsFromPurchase(int purchaseId, int supplierId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetDeliveryRowsFromPurchase(entities, purchaseId, supplierId, actorCompanyId);
        }

        public List<PurchaseDeliveryRowDTO> GetDeliveryRowsFromPurchase(CompEntities entities, int purchaseId, int supplierId, int actorCompanyId)
        {
            var deliveryRows = new List<PurchaseDeliveryRowDTO>();
            var purchaseManager = new PurchaseManager(this.parameterObject);
            var purchaseRows = new List<PurchaseRowDTO>();

            if (purchaseId > 0)
            {
                purchaseRows = purchaseManager.GetPurchaseRows(entities, purchaseId, actorCompanyId);
            }
            else if (supplierId > 0)
            {
                purchaseRows = purchaseManager.GetPurchaseRowsFromSupplier(entities, supplierId, actorCompanyId);
            }

            foreach (var row in purchaseRows.Where(r=> r.Type == PurchaseRowType.PurchaseRow))
            {
                deliveryRows.Add(
                   new PurchaseDeliveryRowDTO
                   {
                       PurchaseNr = row.PurchaseNr,
                       PurchaseRowId = row.PurchaseRowId,
                       PurchaseQuantity = row.Quantity,
                       PurchasePrice = row.PurchasePrice,
                       PurchasePriceCurrency = row.PurchasePriceCurrency,
                       ProductName = row.ProductName,
                       ProductNr = row.ProductNr,
                       StockCode = row.StockCode,
                       DeliveryDate = row.DeliveryDate,
                       RemainingQuantity = row.Quantity - (row.DeliveredQuantity ?? 0),
                       IsLocked = row.Status == (int)SoeOriginStatus.PurchaseDeliveryCompleted,
                   }
                );
            }

            return deliveryRows;
        }

        public ActionResult SaveDelivery(PurchaseDeliverySaveDTO saveData, int actorCompanyId)
        {
            ActionResult result = new ActionResult();
            PurchaseDelivery purchaseDelivery = null;
            int deliveryNr = 0;
            Purchase purchase = null;

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    var baseCurrency = CountryCurrencyManager.GetCompanyBaseCurrencyDTO(entities, base.ActorCompanyId);

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        purchaseDelivery = GetPurchaseDelivery(entities, saveData.PurchaseDeliveryId, true, actorCompanyId);

                        if (purchaseDelivery == null)
                        {
                            // Get next sequence number
                            var entityName = "PurchaseDelivery"; //Enum.GetName(typeof(SoeOriginType), origin.Type);
                            deliveryNr = SequenceNumberManager.GetNextSequenceNumber(entities, base.ActorCompanyId, entityName, 1, false);

                            // Set invoice number to same as sequence number (padded to a specified number of digits)
                            purchaseDelivery = new PurchaseDelivery
                            {
                                PurchaseDeliveryId = saveData.PurchaseDeliveryId,
                                ActorCompanyId = actorCompanyId,
                                SupplierId = saveData.SupplierId,
                                DeliveryNr = deliveryNr,
                                DeliveryDate = saveData.DeliveryDate
                            };

                            entities.PurchaseDelivery.AddObject(purchaseDelivery);
                            SetCreatedProperties(purchaseDelivery);
                        }

                        //new rows
                        var allPurchaseRowIds = saveData.Rows.Select(x=> x.PurchaseRowId);
                        var purchaseRows = entities.PurchaseRow.Where(r => allPurchaseRowIds.Contains(r.PurchaseRowId)).ToList();
                        
                        
                        foreach (var row in saveData.Rows)
                        {
                            var purchaseRow = purchaseRows.FirstOrDefault(x=> x.PurchaseRowId == row.PurchaseRowId);

                            if (purchaseRow == null)
                            {
                                return new ActionResult("Beställningsrad saknas");
                            }

                            if (purchase == null || purchase.PurchaseId != purchaseRow.PurchaseId)
                            {
                                purchase = entities.Purchase.FirstOrDefault(x => x.PurchaseId == purchaseRow.PurchaseId);
                                if (purchase == null)
                                {
                                    return new ActionResult("Beställning saknas");
                                }
                            }

                            if (row.PurchasePriceCurrency != purchaseRow.PurchasePriceCurrency && purchase.CurrencyId != baseCurrency.CurrencyId)
                            {
                                row.PurchasePrice = CountryCurrencyManager.GetBaseAmountFromCurrencyAmount(row.PurchasePriceCurrency, purchase.CurrencyRate);
                            }
                            else if (purchase.CurrencyId == baseCurrency.CurrencyId)
                            {
                                row.PurchasePrice = row.PurchasePriceCurrency;
                            }

                            PurchaseDeliveryRow deliveryRow = null;

                            if (row.PurchaseDeliveryRowId == 0)
                            {
                                deliveryRow = new PurchaseDeliveryRow
                                {
                                    PurchasePrice = row.PurchasePrice,
                                    PurchasePriceCurrency = row.PurchasePriceCurrency,
                                    DeliveredQuantity = row.DeliveredQuantity,
                                    DeliveryDate = row.DeliveryDate,
                                    PurchaseRowId = row.PurchaseRowId,
                                };

                                SetCreatedProperties(deliveryRow);
                                purchaseDelivery.PurchaseDeliveryRow.Add(deliveryRow);
                            }
                            else if (row.IsModified)
                            {
                                deliveryRow = purchaseDelivery.PurchaseDeliveryRow.FirstOrDefault(r => r.PurchaseDeliveryRowId == row.PurchaseDeliveryRowId);
                                if (deliveryRow == null) continue;
                                deliveryRow.PurchasePrice = row.PurchasePrice;
                                deliveryRow.PurchasePriceCurrency = row.PurchasePriceCurrency;
                                deliveryRow.DeliveredQuantity = row.DeliveredQuantity;
                                deliveryRow.DeliveryDate = row.DeliveryDate;
                                SetModifiedProperties(deliveryRow);
                            }

                            if (purchaseRow.StockId.GetValueOrDefault() > 0 && deliveryRow != null)
                            {
                                var stockSaveResult = CreateStockTransaction(entities, transaction, deliveryRow, purchaseRow);
                                if (!stockSaveResult.Success)
                                {
                                    return stockSaveResult;
                                }
                            }
                        }
                        
                        result = UpdatePurchaseRowsFromDelivery(entities, transaction, purchaseRows, saveData);
                        if (!result.Success)
                        {
                            return result;
                        }

                        result = SaveChanges(entities, transaction);


                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                            result.IntegerValue = purchaseDelivery.PurchaseDeliveryId;
                            result.IntegerValue2 = deliveryNr;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }

            }

            return result;
        }

        public ActionResult UpdatePurchaseRowsFromDelivery(CompEntities entities, TransactionScope transaction, List<PurchaseRow> purchaseRows, PurchaseDeliverySaveDTO delivery)
        {
            var modifiedPurchaseRowIds = new List<int>();
            var purchaseRowIds = new List<int>();
            var modifiedDeliveryRowIds = new List<int>();
            PurchaseManager pm = new PurchaseManager(this.parameterObject);

            foreach (var row in delivery.Rows)
            {
                if (row.PurchaseRowId == 0) continue;

                if (row.PurchaseDeliveryRowId == 0)
                {
                    purchaseRowIds.Add(row.PurchaseRowId);
                }
                else if (row.IsModified)
                {
                    modifiedPurchaseRowIds.Add(row.PurchaseRowId);
                    modifiedDeliveryRowIds.Add(row.PurchaseDeliveryRowId);
                }
            }
            
            var oldDeliveryRows = entities.PurchaseDeliveryRow.Where(r => modifiedDeliveryRowIds.Contains(r.PurchaseDeliveryRowId)).ToList();
            var purchaseIds = new List<int>();
            
            foreach (var row in delivery.Rows)
            {
                PurchaseRow purchaseRow = purchaseRows.FirstOrDefault(r => r.PurchaseRowId == row.PurchaseRowId);
                if (purchaseRow == null) continue;
                purchaseRow.DeliveredQuantity = purchaseRow.DeliveredQuantity ?? 0;

                if (row.PurchaseDeliveryRowId == 0 && row.DeliveredQuantity != 0)
                {
                    purchaseRow.DeliveredQuantity += row.DeliveredQuantity;
                    purchaseRow.DeliveryDate = row.DeliveryDate;
                }
                else if (row.IsModified)
                {
                    var oldDeliveryRow = oldDeliveryRows.FirstOrDefault(r => r.PurchaseDeliveryRowId == row.PurchaseDeliveryRowId);
                    if (oldDeliveryRow != null) 
                        purchaseRow.DeliveredQuantity += row.DeliveredQuantity - oldDeliveryRow.DeliveredQuantity;
                }
                
                if (row.SetRowAsDelivered)
                {
                    purchaseRow.Status = (int)SoeOriginStatus.PurchaseDeliveryCompleted;
                    if (!purchaseIds.Contains(purchaseRow.PurchaseId)) purchaseIds.Add(purchaseRow.PurchaseId);
                }
                else if (purchaseRow.DeliveredQuantity != 0)
                {
                    purchaseRow.Status = (int)SoeOriginStatus.PurchasePartlyDelivered;
                    if (!purchaseIds.Contains(purchaseRow.PurchaseId)) purchaseIds.Add(purchaseRow.PurchaseId);
                }
                else
                {
                    purchaseRow.Status = (int)SoeOriginStatus.Origin;
                }

                SetModifiedProperties(purchaseRow);
            }

            var result = SaveChanges(entities, transaction);

            if (!result.Success)
            {
                return result;
            }

            foreach (var id in purchaseIds)
            {
                var origin = entities.Origin.FirstOrDefault(p => p.OriginId == id);
                if (origin == null) continue;
                var totalCount = entities.PurchaseRow
                    .Where(p => p.PurchaseId == id && p.State == (int)SoeEntityState.Active && p.Type == (int)PurchaseRowType.PurchaseRow)
                    .Count();
                var partlyDeliveredCount = entities.PurchaseRow
                    .Where(p => p.PurchaseId == id && p.Status == (int)SoeOriginStatus.PurchasePartlyDelivered && p.State == (int)SoeEntityState.Active && p.Type == (int)PurchaseRowType.PurchaseRow)
                    .Count(); 
                var completedCount = entities.PurchaseRow
                    .Where(p => p.PurchaseId == id && p.Status == (int)SoeOriginStatus.PurchaseDeliveryCompleted && p.State == (int)SoeEntityState.Active && p.Type == (int)PurchaseRowType.PurchaseRow)
                    .Count();


                if (totalCount == completedCount)
                {
                    result = pm.SavePurchaseStatus(entities, id, SoeOriginStatus.PurchaseDeliveryCompleted, delivery.DeliveryDate);
                }
                else if (completedCount > 0 || partlyDeliveredCount > 0)
                {
                    result = pm.SavePurchaseStatus(entities, id, SoeOriginStatus.PurchasePartlyDelivered);
                }

                if (!result.Success)
                {
                    return result;
                }
            }

            return result;
        }


        private ActionResult CreateStockTransaction(CompEntities entities, TransactionScope transaction, PurchaseDeliveryRow deliveryRow, PurchaseRow purchaseRow)
        {
            int defaultGrossMarginCalculationType = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultGrossMarginCalculationType, 0, base.ActorCompanyId, 0);
            StockProduct stPr = StockManager.GetStockProductFromInvoiceProduct(entities, purchaseRow.ProductId.GetValueOrDefault(), purchaseRow.StockId.GetValueOrDefault() );
    
            var stockTransactionDTO = new StockTransactionDTO
            {
                StockTransactionId = 0, //always new
                StockProductId = stPr.StockProductId,
                Quantity = deliveryRow.DeliveredQuantity,
                ActionType = TermGroup_StockTransactionType.Add,
                ReservedQuantity = 0,
                TransactionDate = deliveryRow.DeliveryDate,
            };

            if (defaultGrossMarginCalculationType == (int)TermGroup_GrossMarginCalculationType.StockAveragePrice)
                stockTransactionDTO.Price = stPr.AvgPrice;
            else
                stockTransactionDTO.Price = deliveryRow.PurchasePrice ?? 0;

            //Create transactions
            var result = StockManager.SaveStockTransaction(entities, transaction, stockTransactionDTO, base.ActorCompanyId, false, true);
            if (result.Success && result.Value != null)
            {
                ((StockTransaction)result.Value).PurchaseDeliveryRow = deliveryRow;
            }

            return result;
        }

        public List<PurchaseDeliveryInvoice> GetSupplierPurchaseDeliveryInvoices(int supplierInvoiceId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PurchaseDeliveryInvoice.NoTracking();
            return GetSupplierPurchaseDeliveryInvoices(entities, supplierInvoiceId, actorCompanyId);
        }

        private List<PurchaseDeliveryInvoice> GetSupplierPurchaseDeliveryInvoices(CompEntities entities, int supplierInvoiceId, int actorCompanyId = 0) {

            IQueryable<PurchaseDeliveryInvoice> query = entities.PurchaseDeliveryInvoice
                .Include("SupplierInvoice")
                .Include("SupplierInvoice.Origin")
                .Include("PurchaseRow")
                .Include("PurchaseRow.Product")
                .Include("PurchaseRow.Purchase");
            return (from s in query
                    where s.SupplierinvoiceId == supplierInvoiceId &&
                           s.SupplierInvoice.Origin.ActorCompanyId == actorCompanyId
                    select s).ToList();
        }
    }
}
