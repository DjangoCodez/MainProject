using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class SupplierProductManager : ManagerBase
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #endregion

        #region Ctor

        public SupplierProductManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region SupplierProduct


        public Dictionary<int, string> GetSupplierProductsForGridDict(SupplierProductSearchDTO searchModel, int actorCompanyId)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            List<SupplierProductGridDTO> items = GetSupplierProductsForGrid( searchModel,  actorCompanyId);
            foreach (SupplierProductGridDTO item in items)
            {
                dict.Add(item.SupplierProductId, item.SupplierProductNr);
            }

            return dict;
        }

        public List<SupplierProductGridDTO> GetSupplierProductsForGrid(SupplierProductSearchDTO searchModel, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductsForGrid(entities, searchModel, actorCompanyId);
        }

        public List<SupplierProductGridDTO> GetSupplierProductsForGrid(CompEntities entities, SupplierProductSearchDTO searchModel, int actorCompanyId)
        {

            IQueryable<SupplierProduct> query = (
                from p in entities.SupplierProduct
                where
                    p.Supplier.ActorCompanyId == actorCompanyId &&
                    p.State == (int)SoeEntityState.Active
                select p
            );

            if (searchModel != null)
            {
                if (!searchModel.SupplierIds.IsNullOrEmpty())
                {
                    query = query.Where(p => searchModel.SupplierIds.Contains(p.SupplierId));
                }

                if (!string.IsNullOrEmpty(searchModel.SupplierProduct))
                {
                    query = query.Where(p => p.SupplierProductNr.Contains(searchModel.SupplierProduct));
                }

                if (!string.IsNullOrEmpty(searchModel.SupplierProductName))
                {
                    query = query.Where(p => p.Name.Contains(searchModel.SupplierProductName));
                }

                if (!string.IsNullOrEmpty(searchModel.Product))
                {
                    query = query.Where(p => p.Product.Number.Contains(searchModel.Product));
                }

                if (!string.IsNullOrEmpty(searchModel.ProductName))
                {
                    query = query.Where(p => p.Product.Name.Contains(searchModel.ProductName));
                }

                if (searchModel.InvoiceProductId > 0)
                {
                    query = query.Where(p => p.ProductId == searchModel.InvoiceProductId);
                }
            }

            return query.Select(p =>
                           new SupplierProductGridDTO
                           {
                               SupplierProductId = p.SupplierProductId,
                               SupplierProductNr = p.SupplierProductNr,
                               SupplierProductName = p.Name,
                               SupplierProductCode = p.Code,
                               SupplierId = p.SupplierId,
                               SupplierName = p.Supplier.Name,
                               SupplierNr = p.Supplier.SupplierNr,
                               SupplierProductUnitName = p.ProductUnit.Code,
                               ProductNr = p.Product.Number,
                               ProductName = p.Product.Name,
                           }).ToList();
        }


        public Dictionary<int, string> GetSupplierProductsDict(int supplierId, int actorCompanyId)
        {
            var dict = new Dictionary<int, string>();

            var supplierProduct = GetSupplierProductsSmall(supplierId, actorCompanyId);
            foreach (var sp in supplierProduct)
                dict.Add(sp.SupplierProductId, sp.NumberName);

            return dict;
        }

        public List<SupplierProductSmallDTO> GetSupplierProductsSmall(int supplierId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductsSmall(entities, supplierId, actorCompanyId);
        }
        public List<SupplierProductSmallDTO> GetSupplierProductsSmall(CompEntities entities, int supplierId, int actorCompanyId)
        {

            IQueryable<SupplierProduct> query = (
                from p in entities.SupplierProduct
                where
                    p.Supplier.ActorCompanyId == actorCompanyId &&
                    p.SupplierId == supplierId &&
                    p.State == (int)SoeEntityState.Active
                select p
            );

            return query.Select(p =>
                           new SupplierProductSmallDTO
                           {
                               SupplierProductId = p.SupplierProductId,
                               Number = p.SupplierProductNr,
                               Name = p.Name ?? "",
                           }).ToList();
        }

        public SupplierProduct GetSupplierProduct(int supplierProductId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProduct(entities, supplierProductId, actorCompanyId);
        }

        public SupplierProduct GetSupplierProduct(CompEntities entities, int supplierProductId, int actorCompanyId)
        {
            IQueryable<SupplierProduct> query = (
                from i in entities.SupplierProduct
                .Include("Product")
                where
                    i.SupplierProductId == supplierProductId &&
                    i.State == (int)SoeEntityState.Active
                select i
            );

            return query.FirstOrDefault();
        }
        public List<SmallGenericType> GetSuppliersByInvoiceProduct(int actorCompanyId, int invoiceProductId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SupplierProduct.NoTracking();
            return GetSuppliersByInvoiceProduct(entities, actorCompanyId, invoiceProductId);
        }
        public List<SmallGenericType> GetSuppliersByInvoiceProduct(CompEntities entities, int actorCompanyId, int invoiceProductId)
        {
            return entities.SupplierProduct
                .Where(p => p.Supplier.ActorCompanyId == actorCompanyId && p.ProductId == invoiceProductId)
                .Select(s => new SmallGenericType
                {
                    Id = s.SupplierId,
                    Name = s.Supplier.Name
                })
                .ToList();
        }

        public SupplierProduct GetSupplierProductByInvoiceProduct(int invoiceProductId, int supplierId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductByInvoiceProduct(entities, invoiceProductId, supplierId, actorCompanyId);
        }

        public SupplierProduct GetSupplierProductByInvoiceProduct(CompEntities entities, int invoiceProductId, int supplierId, int actorCompanyId)
        {
            IQueryable<SupplierProduct> query = (
                from i in entities.SupplierProduct
                .Include("Product")
                where
                    i.ProductId == invoiceProductId &&
                    i.SupplierId == supplierId &&
                    i.State == (int)SoeEntityState.Active &&
                    i.Supplier.ActorCompanyId == actorCompanyId
                select i
            );

            return query.FirstOrDefault();
        }

        public bool CheckSupplierProductDuplicate(CompEntities entities, int supplierProductId, int supplierId, string supplierProductNr, int actorCompanyId)
        {
            IQueryable<SupplierProduct> query = (
                from i in entities.SupplierProduct.Include("Supplier")
                where
                    i.SupplierId == supplierId &&
                    i.SupplierProductNr == supplierProductNr &&
                    i.State == (int)SoeEntityState.Active &&
                    i.Supplier.ActorCompanyId == actorCompanyId
                select i
            );

            if (supplierProductId != 0)
                query = query.Where(p => p.SupplierProductId != supplierProductId);

            return query.Any();
        }

        public List<SupplierProductExDTO> GetSupplierProductsByInvoiceProducts(CompEntities entities, List<int> invoiceProductIds, int actorCompanyId)
        {
            return entities.SupplierProduct
                .Where(p => p.Supplier.ActorCompanyId == actorCompanyId && invoiceProductIds.Contains(p.ProductId ?? 0) && p.State == 0)
                .Select(p => new SupplierProductExDTO
                {
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier.Name,
                    SupplierNr = p.Supplier.SupplierNr,
                    SupplierProductId = p.SupplierProductId,
                    ProductId = p.ProductId ?? 0,
                    SupplierProductUnitCode = p.ProductUnit.Code,
                    SupplierProductUnitId = p.SupplierProductUnitId,
                    PackSize = p.PackSize,
                    DeliveryLeadTimeDays = p.DeliveryLeadTimeDays,
                })
                .ToList();
        }

        public ActionResult SaveSupplierProduct(SupplierProductDTO saveDto, List<SupplierProductPriceDTO> priceRows)
        {
            var result = new ActionResult();
            var supplierProductId = saveDto.SupplierProductId;

            // Validate duplicate prices in set

            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    if (priceRows.Any())
                    {

                        foreach (var row in priceRows)
                        {
                            SetDefaultDates(row);
                        }

                        foreach (var row in priceRows.Where(r => r.State == 0).ToList())
                        {
                            //duplicate incoming rows
                            if (IsPriceDuplicate(entities, row, priceRows))
                            {
                                return new ActionResult((int)ActionResultSave.Duplicate, GetText(7590, "Produkten har redan ett pris för valt datum och kvantitet") + $": {row.StartDate.ToShortDateString()}/{row.EndDate.ToShortDateString()}/{row.Quantity}");
                            }
                        }
                    }

                    using (var transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (CheckSupplierProductDuplicate(entities, saveDto.SupplierProductId, saveDto.SupplierId, saveDto.SupplierProductNr, base.ActorCompanyId))
                            return new ActionResult(9230, GetText(9230, 1005, "En artikel med samma artikelnummer existerar redan för vald leverantör"));

                        SupplierProduct supplierProduct = supplierProductId != 0 ? GetSupplierProduct(entities, supplierProductId, base.ActorCompanyId) : null;

                        if (supplierProduct == null)
                        {
                            if (saveDto.SupplierId == 0)
                            {
                                return new ActionResult(GetText(1275, "Leverantör hittades inte"));
                            }
                            supplierProduct = new SupplierProduct();
                            supplierProduct.SupplierId = saveDto.SupplierId;
                            entities.SupplierProduct.AddObject(supplierProduct);
                            SetCreatedProperties(supplierProduct);
                        }
                        else
                        {
                            SetModifiedProperties(supplierProduct);
                        }

                        supplierProduct.ProductId = saveDto.ProductId.ToNullable();
                        supplierProduct.SupplierProductNr = saveDto.SupplierProductNr;
                        supplierProduct.Name = saveDto.SupplierProductName;
                        supplierProduct.Code = saveDto.SupplierProductCode;
                        supplierProduct.PackSize = saveDto.PackSize;
                        supplierProduct.DeliveryLeadTimeDays = saveDto.DeliveryLeadTimeDays.ToNullable();
                        supplierProduct.SupplierProductUnitId = saveDto.SupplierProductUnitId;
                        supplierProduct.SysCountryId = saveDto.SysCountryId;

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            List<SupplierProductPrice> saved = new List<SupplierProductPrice>();
                            priceRows = priceRows
                                .OrderByDescending(r => r.SupplierProductPriceId)
                                .ToList();
                            foreach (var row in priceRows)
                            {
                                if (row.SupplierProductId == 0)
                                {
                                    row.SupplierProductId = supplierProduct.SupplierProductId;
                                }
                                if (row.CurrencyId == 0)
                                {
                                    row.CurrencyId = CountryCurrencyManager.GetCompanyBaseCurrencyDTO(entities, base.ActorCompanyId).CurrencyId;
                                }
                                result = SaveSupplierProductPrice(entities, row, saved);
                                if (!result.Success)
                                {
                                    return result;
                                }
                            }

                            result = SaveChanges(entities, transaction);
                        }

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                            result.IntegerValue = supplierProduct.SupplierProductId;
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

        public ActionResult DeleteSupplierProduct(int purchaseId, int actorCompanyId)
        {
            ActionResult result;
            using (var entities = new CompEntities())
            {
                var product = GetSupplierProduct(entities, purchaseId, actorCompanyId);
                if (product != null)
                {
                    product.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(product);
                    result = SaveChanges(entities);
                }
                else
                {
                    result = new ActionResult(GetText(8331, "Artikel kunde inte hittas"));
                }
            }

            return result;
        }

        #endregion

        #region SupplierProductPrices


        public List<SupplierProductPriceDTO> GetSupplierProductPrices(int supplierProductId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductPrices(entities, supplierProductId, actorCompanyId);
        }

        public List<SupplierProductPriceDTO> GetSupplierProductPrices(CompEntities entities, int supplierProductId, int actorCompanyId)
        {
            IQueryable<SupplierProductPrice> query = (
                from i in entities.SupplierProductPrice
                where
                    i.SupplierProductId == supplierProductId &&
                    i.State == (int)SoeEntityState.Active
                select i
            );

            var prices = query.Select(p =>
                           new SupplierProductPriceDTO
                           {
                               SupplierProductPriceId = p.SupplierProductPriceId,
                               SupplierProductId = p.SupplierProductId,
                               SupplierProductPriceListId = p.SupplierProductPriceListId,
                               CurrencyId = p.CurrencyId,
                               SysCurrencyId = p.Currency.SysCurrencyId,
                               Price = p.Price,
                               Quantity = p.Quantity,
                               StartDate = p.StartDate,
                               EndDate = p.EndDate,
                               State = (SoeEntityState)p.State
                           }).ToList();

            prices.ForEach(p =>
                 p.CurrencyCode = CountryCurrencyManager.GetCurrencyCode(p.SysCurrencyId)
             );

            return prices;
        }

        public SupplierProductPrice GetSupplierProductPrice(CompEntities entities, int supplierProductPriceId)
        {
            IQueryable<SupplierProductPrice> query = (
                from i in entities.SupplierProductPrice
                where
                    i.SupplierProductPriceId == supplierProductPriceId &&
                    i.State == (int)SoeEntityState.Active
                select i
            );

            return query.FirstOrDefault();
        }

        public SupplierProductPriceDTO GetSupplierProductPrice(int productId, int supplierId, DateTime? currentDate, decimal quantity, int currencyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.SupplierProduct.NoTracking();
            var supplierProductId = entitiesReadOnly.SupplierProduct.Where(p => p.ProductId == productId && p.SupplierId == supplierId && p.State == (int)SoeEntityState.Active).Select(pr => pr.SupplierProductId).FirstOrDefault();
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSupplierProductPrice(entities, supplierProductId, currentDate, quantity, currencyId);
        }

        public SupplierProductPriceDTO GetSupplierProductPrice(int supplierProductId, DateTime? currentDate, decimal quantity, int currencyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SupplierProduct.NoTracking();
            return GetSupplierProductPrice(entities, supplierProductId, currentDate, quantity, currencyId);
        }

        public SupplierProductPriceDTO GetSupplierProductPrice(CompEntities entities, int supplierProductId, DateTime? currentDate, decimal quantity, int currencyId)
        {
            IQueryable<SupplierProductPrice> query = (
                from i in entities.SupplierProductPrice
                where
                    i.SupplierProductId == supplierProductId &&
                    i.State == (int)SoeEntityState.Active
                select i
            );

            if (currencyId > 0)
            {
                query = query.Where(p => p.CurrencyId == currencyId);
            }

            if (currentDate.HasValue)
            {
                query = query.Where(p => currentDate >= p.StartDate && currentDate <= p.EndDate);
            }

            if (quantity > 0)
            {
                query = query.Where(p => p.Quantity <= quantity);
            }

            query = query.OrderByDescending(x => x.Quantity).ThenBy(y => y.StartDate);

            var rows = query.Select(x => new SupplierProductPriceDTO
            {
                SupplierProductPriceId = x.SupplierProductPriceId,
                SupplierProductPriceListId = x.SupplierProductPriceListId,
                SupplierProductId = x.SupplierProductId,
                CurrencyId = x.CurrencyId,
                EndDate = x.EndDate,
                Price = x.Price,
                StartDate = x.StartDate,
                Quantity = x.Quantity,
            }).ToList();

            var selected = rows.FirstOrDefault();

            if (currentDate.HasValue && selected != null)
            {
                foreach (var row in rows)
                {
                    if (row.StartDate > selected.StartDate)
                    {
                        selected = row;
                    }
                }
            }

            return selected;
        }
        private bool IsPriceDuplicate(CompEntities entities, SupplierProductPriceDTO current, List<SupplierProductPriceDTO> allIncomingprices)
        {
            Expression<Func<SupplierProductPriceDTO, bool>> predicateDto =
               p =>
                p.SupplierProductId == current.SupplierProductId &&
                p.State == SoeEntityState.Active &&
                p.CurrencyId == current.CurrencyId &&
                p.Quantity == current.Quantity &&
                ((current.StartDate >= p.StartDate && current.StartDate <= p.EndDate) ||
                (current.EndDate >= p.StartDate && current.EndDate <= p.EndDate) ||
                (current.StartDate <= p.StartDate && current.EndDate >= p.EndDate));

            var incomingduplicates = allIncomingprices.AsQueryable().Count(predicateDto);
            if (incomingduplicates > 1)
            {
                return true;
            }

            int[] incmoingSupplierProductPriceIds = allIncomingprices.Select(ip => ip.SupplierProductPriceId).ToArray();

            Expression<Func<SupplierProductPrice, bool>> predicate =
                p =>
                p.SupplierProductId == current.SupplierProductId &&
                !incmoingSupplierProductPriceIds.Contains(p.SupplierProductPriceId) && //excluding incoming SupplierProductPriceIds since those are validating above
                p.CurrencyId == current.CurrencyId &&
                p.Quantity == current.Quantity &&
                p.State == (int)SoeEntityState.Active &&
                ((current.StartDate >= p.StartDate && current.StartDate <= p.EndDate) ||
                (current.EndDate >= p.StartDate && current.EndDate <= p.EndDate) ||
                (current.StartDate <= p.StartDate && current.EndDate >= p.EndDate));

            return entities.SupplierProductPrice.Any(predicate);
        }

        private bool IsPriceDuplicate(CompEntities entities, SupplierProductPriceDTO saveDto, List<SupplierProductPrice> priceBatch)
        {
            Expression<Func<SupplierProductPrice, bool>> predicate =
               p =>
                p.SupplierProductId == saveDto.SupplierProductId &&
                p.SupplierProductPriceId != saveDto.SupplierProductPriceId &&
                p.State == (int)SoeEntityState.Active &&
                p.CurrencyId == saveDto.CurrencyId &&
                p.Quantity == saveDto.Quantity &&
                ((saveDto.StartDate >= p.StartDate && saveDto.StartDate <= p.EndDate) ||
                (saveDto.EndDate >= p.StartDate && saveDto.EndDate <= p.EndDate) ||
                (saveDto.StartDate <= p.StartDate && saveDto.EndDate >= p.EndDate));


            var matches = entities.SupplierProductPrice.Where(predicate)
                .Select(r => r.SupplierProductPriceId)
                .ToList();

            if (matches.Count == 0)
            {
                return false;
            }
            if (priceBatch == null || priceBatch.Count == 0)
            {
                return true;
            }

            var matchingBatch = priceBatch
                .Where(r => matches.Contains(r.SupplierProductPriceId))
                .AsQueryable();

            if (matchingBatch.Any(predicate))
            {
                return true;
            }

            return false;
        }

        private void SetDefaultDates(SupplierProductPriceDTO saveDto)
        {
            if (!saveDto.StartDate.HasValue || saveDto.StartDate == new DateTime(1, 1, 1))
                saveDto.StartDate = new DateTime(1901, 1, 1);
            if (!saveDto.EndDate.HasValue || saveDto.EndDate.Value == new DateTime(1, 1, 1) )
                saveDto.EndDate = new DateTime(9999, 1, 1);
        }

        public ActionResult SaveSupplierProductPrice(CompEntities entities, SupplierProductPriceDTO saveDto, List<SupplierProductPrice> savedInBatch)
        {
            var supplierProductPriceId = saveDto.SupplierProductPriceId;

            if (saveDto.State == SoeEntityState.Deleted)
            {
                return DeleteSupplierProductPrice(entities, supplierProductPriceId);
            }

            SupplierProductPrice price = supplierProductPriceId != 0 ? GetSupplierProductPrice(entities, supplierProductPriceId) : null;

            if (price == null)
            {
                price = new SupplierProductPrice();
                entities.SupplierProductPrice.AddObject(price);
                SetCreatedProperties(price);
            }
            else
            {
                SetModifiedProperties(price);
            }

            SetDefaultDates(saveDto);

            if (saveDto.StartDate > saveDto.EndDate)
            {
                return new ActionResult(3592, GetText(3592, "Slutdatum tidigare än startdatum"));
            }

            if (IsPriceDuplicate(entities, saveDto, savedInBatch))
            {
                return new ActionResult(7590, GetText(7590, "Produkten har redan ett pris för valt datum och kvantitet"));
            }

            if (saveDto.SupplierProductPriceListId != null)
            {
                //We don't want to null existing relationship to pricelist.
                price.SupplierProductPriceListId = saveDto.SupplierProductPriceListId;
            }
            price.SupplierProductId = saveDto.SupplierProductId;
            price.Quantity = saveDto.Quantity;
            price.Price = saveDto.Price;
            price.StartDate = saveDto.StartDate.Value;
            price.EndDate = saveDto.EndDate.Value;
            price.CurrencyId = saveDto.CurrencyId;

            if (savedInBatch != null)
            {
                savedInBatch.Add(price);
            }

            return new ActionResult();
        }

        public ActionResult DeleteSupplierProductPrice(CompEntities entities, int supplierProductPriceId)
        {
            var price = GetSupplierProductPrice(entities, supplierProductPriceId);
            if (price != null)
            {
                price.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(price);
                return SaveChanges(entities);
            }
            else
            {
                return new ActionResult(GetText(3272, "Ingen prislista funnen"));
            }
        }

        public ActionResult UpdateSupplierProductPrices(int actorCompanyId, List<int> supplierProductIds, PriceUpdateDTO priceUpdate, bool updateExisting, DateTime? dateFromIn, DateTime? dateToIn, DateTime priceComparisonDate, int currencyId, decimal? quantityFrom, decimal? quantityTo)
        {
            if (!FeatureManager.HasRolePermission(Feature.Billing_Purchase_Products_PriceUpdate, Permission.Modify, RoleId, ActorCompanyId))
                return new ActionResult(false);

            if (supplierProductIds.IsNullOrEmpty())
                return new ActionResult(false);

            var result = new ActionResult();

            var dateTo = dateToIn == new DateTime(1, 1, 1) || dateToIn == null ? new DateTime(9999, 1, 1) : dateToIn.Value;
            var dateFrom = dateFromIn == new DateTime(1, 1, 1) || dateFromIn == null ? new DateTime(1901, 1, 1) : dateFromIn.Value;
            var message = string.Empty;
            var failed = new List<string>();
            var succeeded = new List<string>();
            var objectsAffected = 0;

            using (CompEntities entities = new CompEntities())
            {
                foreach (int supplierProductId in supplierProductIds)
                {
                    var product = GetSupplierProduct(entities, supplierProductId, actorCompanyId);

                    if (product == null)
                    {
                        continue;
                    }

                    var transResult = new ActionResult();
                    try
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            var currentPrices = GetCurrentPrices(actorCompanyId, supplierProductId, priceComparisonDate, dateFrom, currencyId, quantityFrom, quantityTo);
                            var newPrices = CreateNewPriceDTOs(currentPrices, priceUpdate, updateExisting, dateFrom, dateTo);

                            if (newPrices.Count > 0)
                            {
                                transResult = SavePrices(entities, transaction, product, newPrices);
                            }
                            if (transResult.Success)
                                transaction.Complete();
                            else
                                result.Success = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, log);
                        transResult.Exception = ex;
                        transResult.IntegerValue = 0;
                        transResult.Success = false;
                    }

                    if (transResult.Success)
                    {
                        succeeded.Add(product.Name);
                        objectsAffected += transResult.ObjectsAffected;
                    }
                    else
                        failed.Add(product.Name);
                }

                if (failed.Count > 0)
                    message += GetText(9372, "Prisuppdatering för {0} misslyckades.").Replace("{0}", failed.JoinToString(",")) + "\n";
                if (succeeded.Count > 0)
                    message += GetText(9373, "Prisuppdatering för {0} genomfördes framgångsrikt.").Replace("{0}", succeeded.JoinToString(",")) + "\n";

                result.ErrorMessage = message;
                result.ObjectsAffected = objectsAffected;

                return result;
            }
        }

        private List<(decimal BasePrice, SupplierProductPriceDTO PriceRow)> GetCurrentPrices(int actorCompanyId, int supplierProductId, DateTime priceComparisonDate, DateTime dateFrom, int currencyId, decimal? quantityFrom, decimal? quantityTo)
        {
            var prices = GetSupplierProductPrices(supplierProductId, actorCompanyId);

            if (prices.IsNullOrEmpty())
                return null;

            var query = prices.Where(p => p.StartDate <= dateFrom && p.EndDate >= dateFrom && p.CurrencyId == currencyId);
            var priceComparisonQuery = prices.Where(p => p.StartDate <= priceComparisonDate && p.EndDate >= priceComparisonDate && p.CurrencyId == currencyId);

            if (quantityFrom != null)
            {
                query = query.Where(p => p.Quantity >= quantityFrom);
                priceComparisonQuery = priceComparisonQuery.Where(p => p.Quantity >= quantityFrom);
            }

            if (quantityTo != null)
            {
                query = query.Where(p => p.Quantity <= quantityTo);
                priceComparisonQuery = priceComparisonQuery.Where(p => p.Quantity <= quantityTo);
            }

            if (priceComparisonQuery.Count() > 1) //If there are more than one price, narrowing down result by exact From & To Quantity values
            {
                if (quantityFrom != null)
                {
                    query = query.Where(p => p.Quantity == quantityFrom);
                    priceComparisonQuery = priceComparisonQuery.Where(p => p.Quantity == quantityFrom);
                }

                if (quantityTo != null)
                {
                    query = query.Where(p => p.Quantity == quantityTo);
                    priceComparisonQuery = priceComparisonQuery.Where(p => p.Quantity == quantityTo);
                }
            }

            if (priceComparisonQuery.Count() != 1) //If there are more we do not know which to use as basePrice
            {
                return null;
            }

            return query
                .OrderBy(p => p.StartDate)
                .GroupBy(p => new { p.SupplierProductId })
                .Select(g => g.FirstOrDefault(r => r.StartDate == g.Max(r2 => r2.StartDate)))
                .Select(p => (priceComparisonQuery.First().Price, p))
                .ToList();
        }

        private List<SupplierProductPriceDTO> CreateNewPriceDTOs(List<(decimal BasePrice, SupplierProductPriceDTO PriceRow)> prices, PriceUpdateDTO priceUpdate, bool updateExisting, DateTime fromDate, DateTime toDate)
        {
            var result = new List<SupplierProductPriceDTO>();

            if (prices.IsNullOrEmpty())
                return result;

            var defaultStart = new DateTime(1901, 1, 1);
            var defaultStop = new DateTime(9999, 1, 1);

            foreach (var price in prices)
            {
                var old = price.PriceRow.CloneDTO();

                if (updateExisting)
                {
                    //We want to update the existing price, i.e. not create a new one
                    if (toDate != defaultStop)
                        old.EndDate = toDate;

                    if (fromDate != defaultStart)
                        old.StartDate = fromDate;

                    old.Price = priceUpdate.NewPrice(price.BasePrice, 4);
                }
                else
                {
                    //We want to create a new price. Set accurate stop date of original.
                    old.EndDate = fromDate.AddDays(-1);

                    var newPrice = old.CloneDTO();
                    newPrice.SupplierProductPriceId = 0;
                    newPrice.EndDate = toDate;
                    newPrice.StartDate = fromDate;
                    newPrice.Price = priceUpdate.NewPrice(price.BasePrice, 4);
                    result.Add(newPrice);
                }

                //We always want to update the old entry.
                result.Add(old);
            }
            return result;
        }

        private ActionResult SavePrices(CompEntities entities, TransactionScope transaction, SupplierProduct supplierProduct, List<SupplierProductPriceDTO> priceRows)
        {
            var result = new ActionResult();
            var saved = new List<SupplierProductPrice>();

            priceRows = priceRows
                .OrderByDescending(r => r.SupplierProductPriceId)
                .ToList();

            foreach (var row in priceRows)
            {
                if (row.SupplierProductId == 0)
                {
                    row.SupplierProductId = supplierProduct.SupplierProductId;
                }
                if (row.CurrencyId == 0)
                {
                    row.CurrencyId = CountryCurrencyManager.GetCompanyBaseCurrencyDTO(entities, base.ActorCompanyId).CurrencyId;
                }

                result = SaveSupplierProductPrice(entities, row, saved);

                if (!result.Success)
                {
                    return result;
                }
            }

            return SaveChanges(entities, transaction);
        }

        #endregion

        #region SupplierPricelists
        public List<SupplierProductPriceComparisonDTO> GetSupplierProductPriceCompare(int actorCompanyId, int supplierId, int currencyId, DateTime date, bool includePricelessProducts)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductPriceCompare(entities, actorCompanyId, supplierId, currencyId, date, includePricelessProducts);
        }

        public List<SupplierProductPriceComparisonDTO> GetSupplierProductPriceCompare(CompEntities entities, int actorCompanyId, int supplierId, int currencyId, DateTime date, bool includePricelessProducts)
        {
            var rows = entities.SupplierProductPrice
                .Where(p => p.SupplierProduct.SupplierId == supplierId && p.SupplierProduct.Supplier.ActorCompanyId == actorCompanyId && p.StartDate <= date && p.CurrencyId == currencyId && p.State == 0 && p.SupplierProduct.State == 0)
                .OrderBy(p => p.StartDate)
                .GroupBy(p => new { p.SupplierProductId, p.Quantity })
                .Select(p => p.FirstOrDefault(r => r.StartDate == p.Max(r2 => r2.StartDate)))
                .Select(p => new SupplierProductPriceComparisonDTO
                {
                    SupplierProductId = p.SupplierProductId,
                    ProductName = p.SupplierProduct.Name,
                    ProductNr = p.SupplierProduct.SupplierProductNr,
                    OurProductName = p.SupplierProduct.Product.Name,
                    SupplierProductPriceId = 0,
                    Quantity = p.Quantity,
                    Price = 0,
                    StartDate = date,
                    CompareSupplierProductPriceId = p.SupplierProductPriceId,
                    CompareQuantity = p.Quantity,
                    ComparePrice = p.Price,
                    CompareStartDate = p.StartDate,
                    CompareEndDate = p.EndDate,
                    State = 0,
                })
                .ToList();

            if (includePricelessProducts)
            {
                var ids = rows.Select(r => r.SupplierProductId).ToList();
                var complementaryRows = entities.SupplierProduct
                    .Where(p => p.SupplierId == supplierId && p.Supplier.ActorCompanyId == actorCompanyId && p.State == 0)
                    .Where(p => !ids.Contains(p.SupplierProductId))
                    .Select(p => new SupplierProductPriceComparisonDTO
                    {
                        SupplierProductId = p.SupplierProductId,
                        ProductName = p.Name,
                        ProductNr = p.SupplierProductNr,
                        OurProductName = p.Product.Name,
                        SupplierProductPriceId = 0,
                        Quantity = 0,
                        Price = 0,
                        StartDate = date,
                        ComparePrice = 0,
                        CompareQuantity = 0,
                        State = 0,
                    })
                    .ToList();

                foreach (var row in complementaryRows)
                {
                    rows.Add(row);
                }
            }
            return rows;
        }


        public List<SupplierProductPricelistDTO> GetSupplierProductPricelists(int actorCompanyId, int? supplierId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductPricelists(entities, actorCompanyId, supplierId);
        }

        public List<SupplierProductPricelistDTO> GetSupplierProductPricelists(CompEntities entities, int actorCompanyId, int? supplierId = null)
        {
            var query = entities.SupplierProductPriceList
                .Where(p => p.ActorCompanyId == actorCompanyId && p.State == 0);

            if (supplierId.HasValue && supplierId != 0)
                query = query.Where(p => p.SupplierId == supplierId);

            var pricelists = query.Select(p => new SupplierProductPricelistDTO
            {
                SupplierProductPriceListId = p.SupplierProductPriceListId,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier.Name,
                SupplierNr = p.Supplier.SupplierNr,
                SysWholeSellerId = p.SysWholeSellerId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Created = p.Created,
                SysCurrencyId = p.Currency != null ? p.Currency.SysCurrencyId : 0,

            }).ToList();

            var wholeSellerTypes = base.GetTermGroupDict(TermGroup.SysWholesellerType, GetLangId());
            foreach (var pricelist in pricelists)
            {
                pricelist.CurrencyCode = pricelist.SysCurrencyId == 0 ? string.Empty : CountryCurrencyManager.GetCurrencyCode(pricelist.SysCurrencyId);
                if (pricelist.SysWholeSellerId != null)
                {
                    var wholeseller = WholeSellerManager.GetSysWholesellerDTO(pricelist.SysWholeSellerId.Value);
                    pricelist.SysWholeSellerName = wholeseller.Name;
                    pricelist.SysWholeSellerType = wholeseller.Type;
                    pricelist.SysWholeSellerTypeName = wholeSellerTypes.ContainsKey(wholeseller.Type) ? wholeSellerTypes[wholeseller.Type] : "";
                }
            }
            return pricelists;
        }
        public ActionResult DeleteSupplierProductPricelist(int pricelistId)
        {

            using (var entities = new CompEntities())
            {
                var pricelist = entities.SupplierProductPriceList.FirstOrDefault(p => p.SupplierProductPriceListId == pricelistId && p.ActorCompanyId == this.ActorCompanyId);
                if (pricelist != null)
                {
                    SetModifiedProperties(pricelist);
                    pricelist.State = (int)SoeEntityState.Deleted;
                    var rows = entities.SupplierProductPrice.Where(p => p.SupplierProductPriceListId == pricelistId).ToList();
                    foreach (var row in rows)
                    {
                        row.State = (int)SoeEntityState.Deleted;
                        SetModifiedProperties(row);
                    }
                    pricelist.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(pricelist);
                }

                return SaveChanges(entities);
            }

        }
        public SupplierProductPricelistDTO GetSupplierProductPricelist(int actorCompanyId, int pricelistId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductPricelist(entities, actorCompanyId, pricelistId);
        }

        public SupplierProductPricelistDTO GetSupplierProductPricelist(CompEntities entities, int actorCompanyId, int pricelistId)
        {
            var pricelist = entities.SupplierProductPriceList
                .Where(p => p.ActorCompanyId == actorCompanyId && p.SupplierProductPriceListId == pricelistId)
                .Select(p => new SupplierProductPricelistDTO
                {
                    SupplierProductPriceListId = p.SupplierProductPriceListId,
                    SupplierId = p.SupplierId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    SupplierName = p.Supplier.Name,
                    SupplierNr = p.Supplier.SupplierNr,
                    SysWholeSellerId = p.SysWholeSellerId,
                    Created = p.Created,
                    CreatedBy = p.CreatedBy,
                    Modified = p.Modified,
                    ModifiedBy = p.ModifiedBy,
                    SysCurrencyId = p.Currency != null ? p.Currency.SysCurrencyId : 0,
                    CurrencyId = p.Currency != null ? p.Currency.CurrencyId : 0,
                })
                .FirstOrDefault();

            if (pricelist.SysWholeSellerId != null)
            {
                var wholeseller = WholeSellerManager.GetSysWholesellerDTO(pricelist.SysWholeSellerId.Value);
                if (wholeseller != null)
                {
                    int langId = GetLangId();
                    var wholeSellerTypes = base.GetTermGroupDict(TermGroup.SysWholesellerType, langId);

                    pricelist.SysWholeSellerName = wholeseller.Name;
                    pricelist.SysWholeSellerType = wholeseller.Type;
                    pricelist.SysWholeSellerTypeName = wholeSellerTypes.ContainsKey(wholeseller.Type) ? wholeSellerTypes[wholeseller.Type] : "";
                }
            }
            return pricelist;
        }
        public List<SupplierProductPriceComparisonDTO> GetSupplierProductPricelistPrices(int priceListId, bool includeComparison)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Project.NoTracking();
            return GetSupplierProductPricelistPrices(entities, priceListId, includeComparison);
        }

        public List<SupplierProductPriceComparisonDTO> GetSupplierProductPricelistPrices(CompEntities entities, int priceListId, bool includeComparison)
        {
            var rows = entities.SupplierProductPrice
                .Where(p => p.SupplierProductPriceListId == priceListId && p.SupplierProductPriceList.ActorCompanyId == this.ActorCompanyId && p.State == 0)
                .Select(p => new SupplierProductPriceComparisonDTO
                {
                    SupplierProductPriceListId = p.SupplierProductPriceListId,
                    SupplierProductPriceId = p.SupplierProductPriceId,
                    SupplierProductId = p.SupplierProductId,
                    ProductName = p.SupplierProduct.Name,
                    ProductNr = p.SupplierProduct.SupplierProductNr,
                    OurProductName = p.SupplierProduct.Product.Name,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ComparePrice = 0,
                    CompareQuantity = 0,
                    State = 0,
                })
                .ToList();

            return rows;
        }

        public ActionResult SaveSupplierProductPricelist(SupplierProductPricelistDTO priceListDTO, List<SupplierProductPriceDTO> priceRowDTOs)
        {
            var result = new ActionResult();

            //validation
            if (priceListDTO.SupplierId == 0)
            {
                return new ActionResult(GetText(1275, "Leverantör hittades inte"));
            }
            if (priceListDTO.EndDate == null)
            {
                return new ActionResult(GetText(1679, "Felaktigt datum"));
            }
            var rowDict = new Dictionary<string, bool>();

            foreach (var row in priceRowDTOs.Where(r => r.State == 0).ToList())
            {
                row.StartDate = priceListDTO.StartDate;
                row.EndDate = (DateTime)priceListDTO.EndDate;
                row.CurrencyId = priceListDTO.CurrencyId;
                
                var text = $"{row.SupplierProductId}-{row.Quantity}";
                if (rowDict.ContainsKey(text))
                {
                    return new ActionResult(GetText(9336, "Artikel och kvantitet måste vara unik"));
                }
                else
                {
                    rowDict[text] = true;
                }

                // Validate duplicates
                var duplicateRows = priceRowDTOs.Where(p =>
                                        p.SupplierProductId == row.SupplierProductId &&
                                        p.State == (int)SoeEntityState.Active &&
                                        p.CurrencyId == row.CurrencyId &&
                                        p.Quantity == row.Quantity &&
                                        ((row.StartDate <= p.StartDate && row.EndDate >= p.StartDate) ||
                                        (row.EndDate <= p.EndDate && row.EndDate >= p.EndDate)));

                if (duplicateRows.Count() > 1)
                    return new ActionResult(7590, GetText(7590, "Produkten har redan ett pris för valt datum och kvantitet"));
            }


            using (var entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        SupplierProductPriceList priceList;
                        if (priceListDTO.SupplierProductPriceListId == 0)
                        {
                            priceList = new SupplierProductPriceList
                            {
                                ActorCompanyId = this.ActorCompanyId,
                                SupplierId = priceListDTO.SupplierId
                            };

                            SetCreatedProperties(priceList);
                            entities.SupplierProductPriceList.AddObject(priceList);
                        }
                        else
                        {
                            priceList = entities.SupplierProductPriceList.FirstOrDefault(p => p.ActorCompanyId == this.ActorCompanyId && p.SupplierProductPriceListId == priceListDTO.SupplierProductPriceListId);
                            SetModifiedProperties(priceList);
                        }

                        priceList.SysWholeSellerId = priceListDTO.SysWholeSellerId.GetValueOrDefault() > 0 ? priceListDTO.SysWholeSellerId : null;
                        priceList.StartDate = priceListDTO.StartDate;
                        priceList.EndDate = priceListDTO.EndDate;
                        priceList.CurrencyId = priceListDTO.CurrencyId;

                        result = SaveChanges(entities, transaction);

                        if (result.Success)
                        {
                            List<SupplierProductPrice> saved = new List<SupplierProductPrice>();
                            priceRowDTOs = priceRowDTOs
                                .OrderByDescending(r => r.SupplierProductPriceId)
                                .ToList();
                            foreach (var row in priceRowDTOs)
                            {
                                row.SupplierProductPriceListId = priceList.SupplierProductPriceListId;
                                if (row.SupplierProductId == 0)
                                {
                                    continue;
                                }
                                if (row.CurrencyId == 0)
                                {
                                    row.CurrencyId = CountryCurrencyManager.GetCompanyBaseCurrencyDTO(entities, base.ActorCompanyId).CurrencyId;
                                }
                                result = SaveSupplierProductPrice(entities, row, saved);
                                if (!result.Success)
                                {
                                    return result;
                                }
                            }

                            result = SaveChanges(entities, transaction);
                        }

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                            result.IntegerValue = priceList.SupplierProductPriceListId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                }
                return result;
            }
        }


        public ImportDynamicDTO GetSupplierProductPricelistImportFields(bool importProduct, bool importToPriceList, bool importPrices, bool multipleSuppliers)
        {
            using (var entities = new CompEntities())
            {
                var options = new ImportOptionsDTO();
                var fields = new List<ImportFieldDTO>();

                if (multipleSuppliers)
                {
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierNumber",
                        Label = GetText(38),
                        DataType = SettingDataType.String,
                        IsRequired = true
                    });
                }

                fields.Add(new ImportFieldDTO()
                {
                    Field = "supplierProductNr",
                    Label = GetText(9338),
                    DataType = SettingDataType.String,
                    IsRequired = true
                });

                if (importProduct)
                {
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductName",
                        Label = GetText(9339),
                        DataType = SettingDataType.String,
                        IsRequired = true
                    });
                    var productUnits = ProductManager.GetProductUnits(entities, this.ActorCompanyId)
                        .Select(u => new SmallGenericType
                        {
                            Id = u.ProductUnitId,
                            Name = u.Code,
                        })
                        .ToList();
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductUnit",
                        Label = GetText(8212),
                        IsRequired = true,
                        EnableValueMapping = true,
                        DataType = SettingDataType.String,
                        AvailableValues = productUnits
                    });
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductCode",
                        Label = GetText(9340),
                        DataType = SettingDataType.String,
                    });
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductPackSize",
                        Label = GetText(9341),
                        DataType = SettingDataType.Decimal,
                    });
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductLeadTime",
                        Label = GetText(9342),
                        DataType = SettingDataType.Integer,
                    });
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "salesProductNumber",
                        Label = GetText(9343),
                        DataType = SettingDataType.String,
                    });
                }

                if (importPrices)
                {
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductPricePrice",
                        Label = GetText(4632),
                        DataType = SettingDataType.Decimal,
                        IsRequired = true,
                    });
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductPriceQuantity",
                        Label = GetText(9237),
                        DefaultDecimalValue = 1,
                        DataType = SettingDataType.Decimal,
                    });
                    var currencyCodes = CountryCurrencyManager.GetCurrenciesWithSysCurrency(this.ActorCompanyId)
                        .Select(u => new SmallGenericType
                        {
                            Id = u.CurrencyId,
                            Name = u.Code,
                        })
                        .ToList();
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductPriceCurrencyCode",
                        Label = GetText(9247),
                        DataType = SettingDataType.String,
                        EnableValueMapping = true,
                        AvailableValues = currencyCodes
                    });
                }


                if (!importToPriceList && importPrices)
                {
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductPriceDate",
                        Label = GetText(4646),
                        DataType = SettingDataType.Date,
                        DefaultDateTimeValue = DateTime.MinValue,
                    });
                    fields.Add(new ImportFieldDTO()
                    {
                        Field = "supplierProductPriceDateStop",
                        Label = GetText(9205),
                        DataType = SettingDataType.Date,
                        DefaultDateTimeValue = DateTime.MaxValue,
                    });
                }

                var dto = new ImportDynamicDTO()
                {
                    Fields = fields,
                    Options = options
                };
                return dto;
            }
        }
        public ImportDynamicResultDTO PerformSupplierPriceListImport(bool importToPricelist, int? supplierIdIn, int? priceListId, List<SupplierProductImportRawDTO> rows, ImportOptionsDTO options)
        {
            bool multipleSuppliers = false;
            var result = new ImportDynamicResultDTO();
            if (supplierIdIn == 0)
            {
                result.Success = false;
                result.Message = "No supplier selected.";
                return result;
            }
            else if (supplierIdIn == null)
            {
                multipleSuppliers = true;
            }
            if (importToPricelist && priceListId.GetValueOrDefault() == 0)
            {
                result.Success = false;
                result.Message = "No price list selected.";
                return result;
            }

            rows = rows
                .OrderBy(r => r.SupplierNumber)
                .ThenBy(r => r.SupplierProductNr)
                .ThenBy(r => r.SupplierProductPriceCurrencyCode)
                .ThenBy(r => r.SupplierProductPriceQuantity)
                .ThenBy(r => r.SupplierProductPriceDate)
                .ToList();

            List<SupplierSmallDTO> suppliers = new List<SupplierSmallDTO>();
            SupplierSmallDTO getSupplier(string number)
            {
                number = number.Trim();
                return suppliers.FirstOrDefault(s => s.SupplierNr.Trim() == number);
            }

            List<SupplierProduct> getSupplierProducts(CompEntities entities, int inSupplierId)
            {
                IQueryable<SupplierProduct> query = (
                    from i in entities.SupplierProduct
                        .Include("SupplierProductPrice")
                    where
                        i.SupplierId == inSupplierId &&
                        i.State == (int)SoeEntityState.Active
                    select i
                );
                return query.ToList();
            }

            void AddRowError(int rowId, string message)
            {
                result.AddLog(rowId + 1, LogType.Error, message);
                result.SkippedCount++;
            }


            using (var entities = new CompEntities())
            {
                try
                {
                    if (multipleSuppliers)
                    {
                        suppliers = SupplierManager.GetSupplierByCompanySmall(entities, ActorCompanyId, true);
                    }
                    else
                    {
                        suppliers = SupplierManager.GetSupplierByCompanySmall(entities, ActorCompanyId, true, supplierIdIn);
                    }

                    if (suppliers.Count == 0)
                    {
                        result.Success = false;
                        result.Message = GetText(9360, "Ingen {0} vald").Replace("{0}", GetText(1711, "Leverantör"));
                        return result;
                    }
                    var company = entities.Company.FirstOrDefault(c => c.ActorCompanyId == ActorCompanyId);
                    var productUnits = entities.ProductUnit.Where(s => s.Company.ActorCompanyId == ActorCompanyId).ToList();
                    var currencies = CountryCurrencyManager.GetCurrenciesWithSysCurrency(this.ActorCompanyId);
                    var saleProducts = ProductManager.GetInvoiceProducts(ActorCompanyId, null);

                    SupplierProductPriceList priceList = null;
                    if (importToPricelist && priceListId != null)
                    {
                        priceList = entities.SupplierProductPriceList.FirstOrDefault(p => p.SupplierProductPriceListId == priceListId);
                    }
                    else if (importToPricelist)
                    {
                        result.Success = false;
                        result.Message = GetText(9360, "Ingen {0} vald").Replace("{0}", GetText(1905, "Prislista"));
                        return result;
                    }

                    if (importToPricelist)
                    {
                        PrehandleRawRowsDatePriceList(rows, priceList);
                    }
                    else
                    {
                        PrehandleRawRowsDate(rows);
                    }

                    SupplierSmallDTO supplier = null;
                    List<SupplierProduct> products = null;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var productIsNew = false;
                        result.TotalCount++;
                        var row = rows[i];

                        if (multipleSuppliers)
                        {
                            if (String.IsNullOrEmpty(row.SupplierNumber))
                            {
                                AddRowError(i, GetText(9361, "Rad saknar leverantörsnummer"));
                                continue;
                            }
                            if (supplier == null || row.SupplierNumber != supplier.SupplierNr)
                            {
                                supplier = getSupplier(row.SupplierNumber);
                                if (supplier == null)
                                {
                                    AddRowError(i,
                                        GetText(9362, "Kunde inte hitta leverantör med leverantörsnummer {0}")
                                        .Replace("{0}", row.SupplierNumber));
                                    continue;
                                }
                                products = getSupplierProducts(entities, supplier.ActorSupplierId);
                            }
                        }
                        else if (supplier == null)
                        {
                            supplier = suppliers.FirstOrDefault();
                            if (supplier == null)
                            {
                                AddRowError(i, GetText(9363, "Kunde inte hitta leverantör"));
                                continue;
                            }
                            products = getSupplierProducts(entities, supplier.ActorSupplierId);
                        }

                        int supplierId = supplier.ActorSupplierId;

                        if (String.IsNullOrEmpty(row.SupplierProductNr))
                        {
                            result.AddLog(i + 1, LogType.Error, GetText(9364, "Rad saknar lev. artikelnummer"));
                            result.SkippedCount++;
                            continue;
                        }

                        var product = products.FirstOrDefault(p => row.SupplierProductNr == p.SupplierProductNr);
                        if (product == null && options.ImportNew)
                        {
                            product = new SupplierProduct();
                            product.SupplierProductNr = row.SupplierProductNr;
                            SetCreatedProperties(product);
                            entities.SupplierProduct.AddObject(product);
                            result.NewCount++;
                            products.Add(product);
                            productIsNew = true;
                        }
                        else if (product != null && options.UpdateExisting)
                        {
                            SetModifiedProperties(product);
                            result.UpdateCount++;
                        }

                        if (product != null)
                        {
                            var saleProduct = saleProducts.FirstOrDefault(p => p.Number.Trim() == row.SalesProductNumber);
                            var saleProductId = saleProduct?.ProductId;
                            var unit = productUnits.FirstOrDefault(u => u.Code == row.SupplierProductUnit);
                            var currencyId = supplier.CurrencyId;
                            if (!String.IsNullOrEmpty(row.SupplierProductPriceCurrencyCode))
                            {
                                var currency = currencies.FirstOrDefault(c => c.Code == row.SupplierProductPriceCurrencyCode);
                                currencyId = currency != null ? currency.CurrencyId : currencyId;
                            }
                            else if (importToPricelist && priceList.CurrencyId != null)
                            {
                                currencyId = priceList.CurrencyId.Value;
                            }

                            if (unit == null)
                            {
                                unit = new ProductUnit();
                                unit.Code = row.SupplierProductUnit;
                                unit.Name = row.SupplierProductUnit;
                                unit.Company = company;
                                SetCreatedProperties(unit);
                                productUnits.Add(unit);

                                result.AddLog(i + 1, LogType.Info,
                                    GetText(9365, "Skapade ny enhet {0}").Replace("{0}", row.SupplierProductUnit));
                            }

                            if (options.UpdateExisting || productIsNew)
                            {
                                product.SupplierId = supplierId;
                                product.Name = row.SupplierProductName;
                                product.SupplierProductUnitId = unit.ProductUnitId;
                                product.ProductUnit = unit;
                                product.DeliveryLeadTimeDays = row.SupplierProductLeadTime;
                                product.PackSize = row.SupplierProductPackSize;
                                product.Code = row.SupplierProductCode;
                                product.ProductId = saleProductId;
                            }

                            //var priceDate = priceList != null ? priceList.StartDate : row.SupplierProductPriceDate != null ? row.SupplierProductPriceDate : DateTime.MinValue;
                            //var priceDateEnd = priceList != null ? priceList.EndDate : row.SupplierProductPriceDateStop != null ? row.SupplierProductPriceDateStop : DateTime.MaxValue;

                            if (row.SupplierProductPricePrice != 0)
                            {
                                if (row.SupplierProductPriceDate == null)
                                {
                                    AddRowError(i, GetText(9366, "Rad saknar startdatum"));
                                    continue;
                                }

                                var price = product.SupplierProductPrice
                                    .FirstOrDefault(p => p.StartDate == row.SupplierProductPriceDate &&
                                        p.Quantity == row.SupplierProductPriceQuantity && p.CurrencyId == currencyId);
                                if (price == null)
                                {
                                    price = new SupplierProductPrice();
                                    product.SupplierProductPrice.Add(price);
                                    SetCreatedProperties(price);
                                }
                                else
                                {
                                    SetModifiedProperties(price);
                                }
                                price.SupplierProduct = product;
                                price.Price = row.SupplierProductPricePrice;
                                price.Quantity = row.SupplierProductPriceQuantity;
                                price.CurrencyId = currencyId;
                                price.StartDate = row.SupplierProductPriceDate;
                                price.EndDate = row.SupplierProductPriceDateStop;
                                price.SupplierProduct = product;
                                price.SupplierProductPriceList = priceList;
                            }
                        }
                        else
                        {
                            result.SkippedCount++;
                        }
                    }
                    var actionResult = SaveChanges(entities);
                    if (actionResult.Success)
                    {
                        result.Success = true;
                    }
                    else
                    {
                        result.Success = false;
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Success = false;
                }
            }

            return result;
        }
        private void PrehandleRawRowsDate(List<SupplierProductImportRawDTO> sortedRows)
        {
            //Dates are set from each row. We want to ensure none-overlapping dates.
            //SOP does not have end date, which is considered here.

            if (sortedRows.Count <= 1) return;
            for (int i = 0; i < sortedRows.Count - 1; i++)
            {
                var current = sortedRows[i];
                var next = sortedRows[i + 1];

                if (current.SupplierNumber == next.SupplierNumber &&
                    current.SupplierProductNr == next.SupplierProductNr &&
                    current.SupplierProductPriceCurrencyCode == next.SupplierProductPriceCurrencyCode &&
                    current.SupplierProductPriceQuantity == next.SupplierProductPriceQuantity)
                {
                    if (current.SupplierProductPriceDateStop == DateTime.MaxValue && next.SupplierProductPriceDate != DateTime.MinValue)
                    {
                        current.SupplierProductPriceDateStop = next.SupplierProductPriceDate.AddDays(-1);
                    }
                }
            }
        }
        private void PrehandleRawRowsDatePriceList(List<SupplierProductImportRawDTO> sortedRows, SupplierProductPriceList priceList)
        {
            //Date is set from pricelist

            foreach (SupplierProductImportRawDTO row in sortedRows)
            {
                if (priceList != null)
                {
                    row.SupplierProductPriceDate = priceList.StartDate;
                    row.SupplierProductPriceDateStop = priceList.EndDate ?? DateTime.MaxValue;
                }
            }
        }

        #endregion
    }
}
