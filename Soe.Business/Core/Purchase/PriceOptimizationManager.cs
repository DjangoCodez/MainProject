using Pipelines.Sockets.Unofficial.Arenas;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Linq.Expressions;
using System.Data.Entity;

namespace SoftOne.Soe.Business.Core
{
    public class PriceOptimizationManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public PriceOptimizationManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region PriceOptimization

        public List<PurchaseCartDTO> GetPriceOptimizationsForGrid(int allItemsSelection, int[] status, int actorCompanyId, int? shoppingCartId = null)
        {
            List<TermGroup_PurchaseCartStatus> cartStatus = new List<TermGroup_PurchaseCartStatus>();
            status.ToList().ForEach(f => { cartStatus.Add((TermGroup_PurchaseCartStatus)f); });
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPriceOptimizationsForGrid(entities, allItemsSelection, cartStatus, actorCompanyId, shoppingCartId);
        }

        public ActionResult DeletePriceOptimizations(List<PurchaseCartDTO> shoppingCartRows, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                foreach (PurchaseCartDTO row in shoppingCartRows.ToList())
                {
                    result = DeletePriceOptimization(entities, row.PurchaseCartId, actorCompanyId);

                    if (!result.Success)
                        return result;
                }
                return result;
            }
        }

        public ActionResult Delete(int shoppingCartId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return DeletePriceOptimization(entities, shoppingCartId, actorCompanyId);
        }

        public ActionResult DeletePriceOptimization(CompEntities entities, int shoppingCartId, int actorCompanyId)
        {
            PurchaseCart shoppingCart = GetPriceOptimization(entities, shoppingCartId, actorCompanyId);
            if (shoppingCart == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, "PurchaseCart");

            return ChangeEntityState(entities, shoppingCart, SoeEntityState.Deleted, true);
            
        }

        public ActionResult ChangePriceOptimizationStatus(List<int> ids, TermGroup_PurchaseCartStatus statusTo)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                foreach (int id in ids)
                {
                    PurchaseCart purchaseCart = GetPriceOptimization(entities, id, base.ActorCompanyId);

                    if (purchaseCart == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "PurchaseCart");
                    else
                    {
                        if (purchaseCart.Status != (int)statusTo)
                        {
                            purchaseCart.Status = (int)statusTo;
                            SetModifiedProperties(purchaseCart);
                        }
                    }
                }
                result = SaveChanges(entities);
                return result;

            }
        }
        
        public ActionResult SavePriceOptimization(PurchaseCartDTO purchaseInput, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                entities.CommandTimeout = 180;
                return SavePriceOptimization(entities, purchaseInput, actorCompanyId);
            }
        }

        public ActionResult SavePriceOptimization(CompEntities entities, PurchaseCartDTO purchaseInput, int actorCompanyId, int invoiceId = 0)
        {
            ActionResult result = new ActionResult();

            if (purchaseInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PurchaseCart");
            int purchaseCartId = purchaseInput.PurchaseCartId;
            int sequenceNr = 0;

            try
            {
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    PurchaseCart purchaseCart = purchaseInput.PurchaseCartId != 0 ? GetPriceOptimization(entities, purchaseInput.PurchaseCartId, actorCompanyId, false, true) : null;

                    if (purchaseCart == null)
                    {
                        #region New PurchaseCart

                        sequenceNr = SequenceNumberManager.GetNextSequenceNumber(entities, actorCompanyId, "PurchaseCart", 1, false);
                        purchaseCart = new PurchaseCart
                        {
                            ActorCompanyId = actorCompanyId,
                            SeqNr = sequenceNr,
                            Status = (int)TermGroup_PurchaseCartStatus.Open,
                        };

                        SetCreatedProperties(purchaseCart);                        
                        entities.PurchaseCart.AddObject(purchaseCart);

                        #endregion
                    }
                    else
                    {
                        #region Edit PurchaseCart

                        purchaseCart.Status = purchaseInput.Status;
                        SetModifiedProperties(purchaseCart);

                        #endregion
                    }

                    #region PriceOptimizationRows



                    #region Delete Rows

                    var incomingRowIds = purchaseInput.PurchaseCartRows
                        .Where(r => r.PurchaseCartRowId > 0)
                        .Select(r => r.PurchaseCartRowId)
                        .ToHashSet();

                    foreach (var existingRow in purchaseCart.PurchaseCartRow)
                    {
                        if (!incomingRowIds.Contains(existingRow.PurchaseCartRowId))
                        {
                            ChangeEntityState(existingRow, SoeEntityState.Deleted);
                        }
                    }

                    #endregion


                    foreach (PurchaseCartRowDTO purchaseCartRow in purchaseInput.PurchaseCartRows)
                    {
                        PurchaseCartRow cartRow = GetPriceOptimizationRow(entities, purchaseInput.PurchaseCartId, purchaseCartRow.PurchaseCartRowId);

                        if (cartRow == null)
                        {
                            cartRow = new PurchaseCartRow
                            {
                                PurchaseCartId = purchaseCart.PurchaseCartId,
                                SysProductId = purchaseCartRow.SysProductId,
                            };

                            SetCreatedProperties(cartRow);
                            entities.PurchaseCartRow.AddObject(cartRow);
                        }
                        else
                            SetModifiedProperties(cartRow);

                        cartRow.Quantity = purchaseCartRow.Quantity;
                        cartRow.PurchasePrice = purchaseCartRow.PurchasePrice ?? 0;
                        cartRow.SysWholesellerId = purchaseCartRow.SysWholesellerId;
                        cartRow.SysPricelistHeadId = (int?)purchaseCartRow.SysPricelistHeadId;
                        cartRow.State = (int)purchaseCartRow.State;

                    }
                    #endregion

                    purchaseCart.Name = purchaseInput.Name;
                    purchaseCart.Description = purchaseInput.Description;
                    purchaseCart.PriceStrategy = purchaseInput.PriceStrategy;
                    purchaseCart.SelectedWholesellerIds = string.Join(",", purchaseInput.SelectedWholesellerIds);

                    result = SaveChanges(entities, transaction);

                    if (result.Success)
                    {
                        //transfer from offer/order
                        if (invoiceId > 0)
                        {
                            AddTracking(entities, purchaseCart.PurchaseCartId, invoiceId);
                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;
                        }

                        transaction.Complete();
                        purchaseCartId = purchaseCart.PurchaseCartId;
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
                    //Set success properties
                    result.StringValue = sequenceNr.ToString();
                    result.IntegerValue = purchaseCartId;
                }
                else
                    base.LogTransactionFailed(this.ToString(), this.log);

            }

            return result;
        }

        public ActionResult CreatePriceOptimization(int invoiceId, SoeOriginStatusChange originStatus, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ActionResult result = new ActionResult();
                try
                {
                    entities.Connection.Open();

                    List<int> wholesalerIds = WholeSellerManager.GetWholesellerDictByCompany(entities, actorCompanyId, false).Select(x => x.Key).ToList();
                    CustomerInvoice customerInvoice = InvoiceManager.GetCustomerInvoice(entities, invoiceId, loadActor: true, loadInvoiceRow: true);

                    var poInput = new PurchaseCartDTO
                    {
                        Name = originStatus == SoeOriginStatusChange.Billing_OfferToPriceOptimization ? $"Offer: {customerInvoice.InvoiceNr}_{customerInvoice.ActorName}" : $"Order: {customerInvoice.InvoiceNr}_{customerInvoice.ActorName}",
                        Description = string.Empty,
                        PriceStrategy = (int)TermGroup_PurchaseCartPriceStrategy.WholesalerPriceList,
                        PurchaseCartRows = new List<PurchaseCartRowDTO>(),
                        SelectedWholesellerIds = wholesalerIds,
                    };

                    foreach (CustomerInvoiceRow row in customerInvoice.CustomerInvoiceRow)
                    {
                        if ((SoeEntityState)row.State == SoeEntityState.Deleted) continue;

                        if (row?.ProductId.GetValueOrDefault() > 0)
                        {
                            var SysProductId = 0;
                            var invoiceProduct = ProductManager.GetInvoiceProduct(entities, row.ProductId.Value);
                            if (invoiceProduct.ExternalProductId.GetValueOrDefault() == 0) 
                            {
                                if (invoiceProduct.Number.Length > 4)
                                {
                                    var externalProduct = GetInvoiceProductExternalSearch(entities, invoiceProduct.Number, customerInvoice, actorCompanyId);
                                    SysProductId = externalProduct?.ExternalProductId ?? 0;
                                }
                            }
                            else
                            {
                                SysProductId = (int)invoiceProduct.ExternalProductId;
                            }

                            if (SysProductId > 0)
                            {
                                poInput.PurchaseCartRows.Add(new PurchaseCartRowDTO
                                {
                                    SysProductId = SysProductId,
                                    ProductNr = invoiceProduct.Number,
                                    Quantity = row.Quantity.GetValueOrDefault(),
                                    PurchasePrice = row.PurchasePrice,
                                    SysWholesellerId = (int)customerInvoice.SysWholeSellerId
                                });
                            }
                        }
                    }

                   result = SavePriceOptimization(entities, poInput, actorCompanyId, invoiceId);
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

                return result;
            }
        }

        public void AddTracking(CompEntities entities, int purchaseCartId, int invoiceId)
        {
            PurchaseCart priceOpt = GetPriceOptimization(entities, purchaseCartId, base.ActorCompanyId, true);
            Origin origin = OriginManager.GetOrigin(entities, invoiceId, false);
            bool hasDuplicates = priceOpt.Origin.Any(o => o.OriginId == invoiceId);

            if (!hasDuplicates)
                priceOpt.Origin.Add(origin);
        }

        public List<PriceOptimizationTraceDTO> GetPriceOptimizationTraceRows(int shoppingCartId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPriceOptimizationTraceRows(entities, shoppingCartId, actorCompanyId);
        }

        public List<PriceOptimizationTraceDTO> GetPriceOptimizationTraceRows(CompEntities entities, int shoppingCartId, int actorCompanyId)
        {
            IQueryable<Origin> query = (from pc in entities.Origin
                                    where (pc.ActorCompanyId == actorCompanyId) &&
                                            (pc.PurchaseCart.Any(x=> x.PurchaseCartId == shoppingCartId))
                                    select pc);

            List<PriceOptimizationTraceDTO> traceRows = query
                 .Select(PurchaseCartExtensions.PriceOptimizationTraceDTO)
                 .ToList();

            foreach (var dto in traceRows)
            {
                dto.OriginTypeName = GetText((int)dto.OriginType, (int)TermGroup.OriginType);
                dto.OriginStatusName = GetText((int)dto.OriginStatus, (int)TermGroup.OriginStatus);
            }

            return traceRows;
        }

        public PurchaseCart GetPriceOptimization(int shoppingCartId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPriceOptimization(entities, shoppingCartId, actorCompanyId);
        }

        public PurchaseCart GetPriceOptimization(CompEntities entities, int shoppingCartId, int actorCompanyId, bool includeOrigin = false, bool includeRows = false)
        {
            IQueryable<PurchaseCart> query =  (from pc in entities.PurchaseCart
                    where ((pc.Company.ActorCompanyId == actorCompanyId) &&
                    (pc.PurchaseCartId == shoppingCartId) &&
                    (pc.State == (int)SoeEntityState.Active))
                    select pc);

            if (includeOrigin)
                query = query.Include("Origin");

            if (includeRows)
                query = query.Include("PurchaseCartRow");

            return query.FirstOrDefault(p => p.PurchaseCartRow.Any(r => r.State == (int)SoeEntityState.Active));
        }

        public List<PurchaseCartDTO> GetPriceOptimizationsForGrid(CompEntities entities, int allItemsSelection, List<TermGroup_PurchaseCartStatus> status, int actorCompanyId, int? shoppingCartId = null)
        {
            var purchaseCart = (from pc in entities.PurchaseCart
                                               where ((pc.Company.ActorCompanyId == actorCompanyId) &&
                                              (pc.State == (int)SoeEntityState.Active))
                                               orderby pc.Name
                                               select pc).ToList();

            if (shoppingCartId.HasValue)
            {
                purchaseCart = purchaseCart.Where(x => x.PurchaseCartId == shoppingCartId).ToList();
            }
            // status filter 
            if (status.Count > 0)
            {
                purchaseCart = purchaseCart.Where(i => status.Contains((TermGroup_PurchaseCartStatus)i.Status)).ToList();
            }

            // time‐period filter: skip “All” (value 99)
            if (allItemsSelection > 0 && allItemsSelection < 99)
            {
                var cutoff = DateTime.UtcNow.AddMonths(-allItemsSelection);
                purchaseCart = purchaseCart.Where(pc => pc.Created >= cutoff).ToList();
            }

            var data = purchaseCart.Select(pc => new PurchaseCartDTO
            {
                PurchaseCartId = pc.PurchaseCartId,
                SeqNr = pc.SeqNr,
                Name = pc.Name,
                Description = pc.Description,
                Status = pc.Status,
                Created = pc.Created,
                CreatedBy = pc.CreatedBy,
                Modified = pc.Modified,
                ModifiedBy = pc.ModifiedBy
            }).ToList();

            return data;
        }

        #endregion

        #region PriceOptimization Row

        public List<PurchaseCartRowDTO> GetPriceOptimizationRow(int shoppingCartId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPriceOptimizationRows(entities, shoppingCartId);
        }


        public List<PurchaseCartRowDTO> GetPriceOptimizationRows(CompEntities entities, int shoppingCartId)
        {
            List<PurchaseCartRowDTO> productRows = new List<PurchaseCartRowDTO>();
            var query = (from pc in entities.PurchaseCartRow
                         where (pc.PurchaseCartId == shoppingCartId) &&
                         (pc.State == (int)SoeEntityState.Active)
                         select pc).ToList();
            if (query.Count != 0)
            {
                var sysProductIds = query
                            .Select(pc => pc.SysProductId)
                            .Distinct()
                            .ToList();

                var sysProducts = SysPriceListManager.GetSysProductsDTOById(sysProductIds);

                foreach (var row in query)
                {
                    sysProducts.TryGetValue(row.SysProductId, out var sysProduct);
                    var dto = new PurchaseCartRowDTO
                    {
                        PurchaseCartRowId = row.PurchaseCartRowId,
                        PurchaseCartId = row.PurchaseCartId,
                        SysProductId = row.SysProductId,
                        SysWholesellerId = row.SysWholesellerId,
                        PurchasePrice = row.PurchasePrice,
                        Quantity = row.Quantity,
                        State = (SoeEntityState)row.State,
                        ProductNr = sysProduct?.ProductId,
                        ProductName = sysProduct?.Name,
                        ProductInfo = sysProduct?.ExtendedInfo,
                        ImageUrl = sysProduct.ImageFileName,
                        ExternalId = sysProduct.ExternalId,
                        Type = sysProduct.Type,
                    };
                    productRows.Add(dto);
                }
            }

            return productRows;
        }

        public PurchaseCartRow GetPriceOptimizationRow(CompEntities entities, int shoppingCartId, int cartRowId)
        {
            return (from pc in entities.PurchaseCartRow
                    where (pc.PurchaseCartId == shoppingCartId) &&
                    (pc.PurchaseCartRowId == cartRowId) &&
                    (pc.State == (int)SoeEntityState.Active)
                    select pc).FirstOrDefault();
        }

        public ActionResult TransferPriceOptimizationRowsToOrderOffer(int invoiceId, int purchaseCartId)
        {
            using (CompEntities entities = new CompEntities())
            {
                ActionResult result = new ActionResult();
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        CustomerInvoice order = invoiceId != 0 ? InvoiceManager.GetCustomerInvoice(entities, invoiceId, true, true, false, false, false, false, false, true) : null;
                        int sysCurrencyIdInvoice = CountryCurrencyManager.GetSysCurrencyId(entities, order.CurrencyId);
                        int sysCurrencyIdBase = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, base.ActorCompanyId);
                        int productMiscId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductMisc, 0, base.ActorCompanyId, 0);
                        List<PurchaseCartRowDTO> cartRows = GetPriceOptimizationRows(entities, purchaseCartId);
                       
                        foreach (var cartRow in cartRows)
                        {
                            SysWholesellerDTO syswholsellers = WholeSellerManager.GetSysWholesellerDTO(cartRow.SysWholesellerId);
                            if (syswholsellers != null)
                            {
                                InvoiceProduct product = GetInvoiceProduct(entities, cartRow.ProductNr, cartRow.SysWholesellerId, productMiscId);
                                if (product == null) continue;

                                // get sales price
                                ProductPriceRequestDTO productPriceRequestDTO = new ProductPriceRequestDTO {
                                    CurrencyId = order.CurrencyId,
                                    ProductId = product.ProductId,
                                    PurchasePrice = cartRow.PurchasePrice,
                                    CustomerId = (int)order.ActorId,
                                    PriceListTypeId = (int)order.PriceListTypeId,
                                    WholesellerId = cartRow.SysWholesellerId,
                                    Quantity = cartRow.Quantity,
                                    ReturnFormula = false,
                                    CopySysProduct = false,
                                };
                                InvoiceProductPriceResult productPrice = ProductManager.GetProductPrice(entities, base.ActorCompanyId, productPriceRequestDTO, syswholsellers.Name);

                                result = InvoiceManager.SaveCustomerInvoiceRow(transaction, entities, base.ActorCompanyId, order, 0, product.ProductId, cartRow.Quantity, productPrice.SalesPrice, "", SoeInvoiceRowType.ProductRow, sysWholeSellerName: syswholsellers.Name, saveChanges: false, productPurchasePrice: cartRow.PurchasePrice);
                                if (!result.Success)
                                    return result;
                            }
                            else
                                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(4173, "Grossist måste anges"));
                           
                        }
                        if (result.Success)
                        {
                            AddTracking(entities, purchaseCartId, invoiceId);

                            var accountRowNr = order.CustomerInvoiceRow.SelectMany(r => r.CustomerInvoiceAccountRow).Max(r => r.RowNr) + cartRows.Count;
                            result = InvoiceManager.UpdateInvoiceAfterRowModification(entities, order, accountRowNr, base.ActorCompanyId, false, sysCurrencyIdInvoice != sysCurrencyIdBase);
                            if (result.Success)
                            {
                                result = SaveChanges(entities, transaction);
                                if (result.Success)
                                {
                                    result.IntegerValue = order.InvoiceId;
                                    transaction.Complete();
                                }
                            }
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
                    entities.Connection.Close();
                }

                return result;
            }
        }

        public List<SysWholsesellerPriceSearchDTO> GetPriceOptimizationRowPrices(List<int> sysProductIds)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<CompanyWholesellerPriceListSmallDTO> priceList = WholeSellerManager.GetCompanySysWholesellerPriceLists(entities, base.ActorCompanyId);
                List<int> sysPriceListHeadIds = priceList.Select(p => p.SysPriceListHeadId).ToList();
                List<SysWholsesellerPriceSearchDTO> wholesalerPrices = SysPriceListManager.GetWholsellerPrices(sysPriceListHeadIds, sysProductIds);

                return wholesalerPrices;
            }
        }

        #endregion

        #region Helper methods

        public InvoiceProduct GetInvoiceProduct(CompEntities entities, string productNr, int sysWholesellerId, int productMiscId)
        {
            #region Internal search

            InvoiceProduct invoiceProduct = ProductManager.GetInvoiceProductByProductNr(entities, productNr, base.ActorCompanyId);

            #endregion

            #region External/Wholeseller search

            if (invoiceProduct == null)
                invoiceProduct = ProductManager.CopyExternalInvoiceProductFromSysByProductNr(entities, 0, productNr, 0, sysWholesellerId, null, 0, null, base.ActorCompanyId, 0, true);

            #endregion

            return invoiceProduct;
        }

        public ExternalProductSmallDTO GetInvoiceProductExternalSearch(CompEntities entities, string productNr, CustomerInvoice customerInvoice, int actorCompanyId)
        {
                List<ExternalProductSmallDTO> externalInvoiceProduct = ProductManager.AzureSearchSysProducts(entities, actorCompanyId, productNr, "", string.Empty, string.Empty, 10);
                return externalInvoiceProduct.Where(r => r.ProductId.ToLower() == productNr.ToLower()).FirstOrDefault();
        }

        #endregion

        public static class PurchaseCartExtensions
        {
            public readonly static Expression<Func<Origin, PriceOptimizationTraceDTO>> PriceOptimizationTraceDTO =
                p => new PriceOptimizationTraceDTO
                {
                    Number = p.Invoice.SeqNr ?? 0,
                    OriginType = (SoeOriginType)p.Type,
                    Description = p.Description ?? "",
                    Date = p.Invoice.InvoiceDate,
                    OriginStatus = (SoeOriginStatus)p.Status,
                };
        }

    }
}
