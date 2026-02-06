using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.PricelistProvider;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.CustomerInvoice;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class WholsellerNetPriceManager:ManagerBase
    {
       #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public WholsellerNetPriceManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Rows

        public WholsellerNetPriceRowDTO GetNetPrice(CompEntities entities, int actorCompanyId, int SysProductId, int sysWholeSellerId, int? priceListTypeId)
        {
            if (!WholeSellerHasNetPriceList(sysWholeSellerId))
            {
                return null;
            }

            IQueryable<WholsellerNetPriceRow> query = (from p in entities.WholsellerNetPriceRow
                                where p.WholsellerNetPrice.ActorCompanyId == actorCompanyId &&
                                        p.WholsellerNetPrice.SysWholeSellerId == sysWholeSellerId &&
                                        p.SysProductId == SysProductId &&
                                        p.State == (int)SoeEntityState.Active
                                select p);

            if (priceListTypeId.GetValueOrDefault() == 0)
            {
                query = query.Where(x => x.WholsellerNetPrice.PriceListTypeId == null);
            }
            else
            {
                query = query.Where(x => x.WholsellerNetPrice.PriceListTypeId == priceListTypeId || x.WholsellerNetPrice.PriceListTypeId == null);
            }

            var priceRows = query.Select(p => new WholsellerNetPriceRowDTO
             {
                 SysProductId = p.SysProductId,
                 NetPrice = p.NetPrice,
                 GNP = p.GNP,
                 SysWholesellerId = p.WholsellerNetPrice.SysWholeSellerId,
                 Date = p.WholsellerNetPrice.Date,
                 PriceListTypeId = p.WholsellerNetPrice.PriceListTypeId ?? 0,
                 WholsellerNetPriceRowId = p.WholsellerNetPriceRowId,
                 WholsellerNetPriceId = p.WholsellerNetPrice.WholsellerNetPriceId,
            }).OrderByDescending(x => x.Date).ToList();

            var result = priceRows.FirstOrDefault(x => x.PriceListTypeId == priceListTypeId);
            if (result == null)
            {
                return priceRows.FirstOrDefault(x => x.PriceListTypeId == 0);
            }
            return result;
        }

        public List<WholsellerNetPriceRowDTO> GetNetPrices(int actorCompanyId, int sysWholeSellerId)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<WholsellerNetPriceRow> query = (from p in entitiesReadOnly.WholsellerNetPriceRow
                                                       where p.WholsellerNetPrice.ActorCompanyId == actorCompanyId && p.State == (int)SoeEntityState.Active
                                                       select p);

            if (sysWholeSellerId > 0)
            {
                query = query.Where(x => x.WholsellerNetPrice.SysWholeSellerId == sysWholeSellerId);
            }

            var products = query.Select(p=> new WholsellerNetPriceRowDTO
            {
                WholsellerNetPriceRowId = p.WholsellerNetPriceRowId,
                SysProductId = p.SysProductId,
                NetPrice = p.NetPrice,
                GNP = p.GNP,
                SysWholesellerId = p.WholsellerNetPrice.SysWholeSellerId,
                PriceListTypeName = p.WholsellerNetPrice.PriceListType.Name,
                Date = p.WholsellerNetPrice.Date,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
            }).ToList();

            var syswholsellers = SysPriceListManager.GetSysWholesellersDict();

            foreach (var grp in products.GroupBy(x=> x.SysWholesellerId ))
            {
                var sysWholseller = syswholsellers[grp.First().SysWholesellerId];
                var sysProducts = SysPriceListManager.GetSysProductsDTODict((ExternalProductType)sysWholseller.Type, sysWholseller.SysCountryId);
                foreach (var p in grp)
                {
                    sysProducts.TryGetValue(p.SysProductId, out var sysproduct);
                    if (sysproduct != null)
                    {
                        p.ProductNr = sysproduct.ProductId;
                        p.ProductName = sysproduct.Name;
                    }
                    p.WholesellerName = sysWholseller.Name;
                }
            }

            return products;
        }

        public List<WholsellerNetPriceRowDTO> GetNetPrices(int actorCompanyId, List<int> sysProductIds, List<int> sysWholesellerIds = null)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<WholsellerNetPriceRow> query = (from p in entitiesReadOnly.WholsellerNetPriceRow
                                                       where p.WholsellerNetPrice.ActorCompanyId == actorCompanyId && p.State == (int)SoeEntityState.Active &&
                                                             sysProductIds.Contains(p.SysProductId)
                                                       select p);

            if (!sysWholesellerIds.IsNullOrEmpty())
            { 
                query = query.Where(x => sysWholesellerIds.Contains(x.WholsellerNetPrice.SysWholeSellerId));
            }

            var products = query.Select(p => new WholsellerNetPriceRowDTO
            {
                WholsellerNetPriceRowId = p.WholsellerNetPriceRowId,
                SysProductId = p.SysProductId,
                NetPrice = p.NetPrice,
                GNP = p.GNP,
                SysWholesellerId = p.WholsellerNetPrice.SysWholeSellerId,
                PriceListTypeId = p.WholsellerNetPrice.PriceListTypeId ?? 0
            }).ToList();

            return products;
        }

        public List<int> HasNetPrices(CompEntities entities, int actorCompanyId, List<int> sysProductIds)
        {
            IQueryable<WholsellerNetPriceRow> query = (from p in entities.WholsellerNetPriceRow
                                                       where p.WholsellerNetPrice.ActorCompanyId == actorCompanyId && p.State == (int)SoeEntityState.Active &&
                                                             sysProductIds.Contains(p.SysProductId)
                                                       select p);

            var products = query.DistinctBy(x=> x.SysProductId).Select(x => x.SysProductId).ToList();

            return products;
        }

        #endregion

        #region Heads

        public static WholsellerNetPrice GetWholsellerNetPrice(CompEntities entities,int actorCompanyId, int sysWholeSellerId, int? priceListTypeId)
        {
            IQueryable<WholsellerNetPrice> query = (from p in entities.WholsellerNetPrice
                                                   where
                                                      p.ActorCompanyId == actorCompanyId &&
                                                      p.SysWholeSellerId == sysWholeSellerId &&
                                                      p.State == (int)SoeEntityState.Active
                                                   select p);

            if (priceListTypeId.HasValue)
            {
                query = query.Where(x => x.PriceListTypeId == priceListTypeId);
            }
            else
            {
                query = query.Where(x => x.PriceListTypeId == null);
            }

            return query.FirstOrDefault();
        }

        public static List<WholsellerNetPrice> GetWholsellerNetPrices(CompEntities entities, int actorCompanyId, List<int> sysWholeSellerIds = null)
        {
            IQueryable<WholsellerNetPrice> query = (from p in entities.WholsellerNetPrice
                                                    where
                                                       p.ActorCompanyId == actorCompanyId &&
                                                       p.State == (int)SoeEntityState.Active
                                                    select p);
            if (!sysWholeSellerIds.IsNullOrEmpty())
            {
                query = query.Where(x=> sysWholeSellerIds.Contains(x.SysWholeSellerId));
            }

            return query.ToList();
        }

        #endregion

        public ActionResult Save(int actorCompanyId, int sysWholesellerId, int priceListTypeId, List<WholsellerNetPriceRowDTO> priceRows)
        {
            var result = new ActionResult();
            var versionDate = DateTime.Today;
            string information = "";
            int rowCount = 0;

            using (var entities = new CompEntities())
            {
                try
                {
                    var sysWholeSeller = WholeSellerManager.GetSysWholesellerDTO(sysWholesellerId);
                    if (sysWholeSeller == null)
                    {
                        return new ActionResult(false, 0, GetText(3802, "Grossist ej funnen") + ": " + sysWholesellerId);
                    }

                    var head = GetWholsellerNetPrice(entities, actorCompanyId, sysWholesellerId, priceListTypeId.ToNullable());

                    if (head != null)
                    {
                        head.State = (int)SoeEntityState.Deleted;
                        SetModifiedProperties(head);
                        entities.DeleteWholsellerNetPrices(actorCompanyId, sysWholesellerId, priceListTypeId.ToNullable());
                        result = SaveChanges(entities);
                        if (!result.Success)
                        {
                            return result;
                        }
                    }

                    head = new WholsellerNetPrice
                    {
                        ActorCompanyId = actorCompanyId,
                        SysWholeSellerId = sysWholesellerId,
                        PriceListTypeId = priceListTypeId.ToNullable(),
                        Date = versionDate,
                    };

                    SetCreatedProperties(head);
                    entities.WholsellerNetPrice.AddObject(head);

                    foreach (var rowByProductType in priceRows.GroupBy(x => x.ProductType))
                    {
                        var type = rowByProductType.Key == ExternalProductType.Unknown ? (ExternalProductType)sysWholeSeller.Type : rowByProductType.Key;
                        var sysProducts = SysPriceListManager.GetSysProductsNumberDict(type, sysWholeSeller.SysCountryId);

                        foreach (var importRow in rowByProductType)
                        {
                            var row = importRow.WholsellerNetPriceRowId > 0 ? entities.WholsellerNetPriceRow.FirstOrDefault(x => x.WholsellerNetPriceRowId == importRow.WholsellerNetPriceRowId && x.State == (int)SoeEntityState.Active) : null;
                            if (row == null)
                            {
                                row = new WholsellerNetPriceRow
                                {
                                    NetPrice = importRow.NetPrice,
                                    GNP = importRow.GNP,
                                    SysProductId = importRow.SysProductId,
                                };

                                if (row.SysProductId == 0)
                                {

                                    var sysProduct = sysProducts[importRow.ProductNr].FirstOrDefault();
                                    if (sysProduct != null)
                                    {
                                        row.SysProductId = sysProduct.SysProductId;
                                    }

                                    //row.SysProductId = SysPriceListManager.GetSysPriceListByProductNumber(importRow.ProductNr, sysWholesellerId)?.SysProductId ?? 0;

                                    if (row.SysProductId == 0)
                                    {

                                        //return new ActionResult("Failed finding sysproduct for:" + importRow.ProductNr);
                                        //information += "Failed finding sysproduct for:" + importRow.ProductNr + "\n";
                                        continue;
                                    }
                                }

                                SetCreatedProperties(row);
                                head.WholsellerNetPriceRow.Add(row);
                                rowCount++;
                            }
                        }
                    }

                    //result = SaveChanges(entities);
                    entities.BulkSaveChanges();
                    result.InfoMessage = information;
                    result.ObjectsAffected = rowCount;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
            }

            return result;
        }

        public ActionResult DeleteRows(List<int> wholesellerNetPriceRowIds, int actorCompanyId)
        {
            var result = new ActionResult();
            var entitiesToDelete = new List<WholsellerNetPriceRow>();
            using (var entities = new CompEntities())
            {
                var deleteGroup = wholesellerNetPriceRowIds.Select((x, idx) => new { x, idx })
                                                            .GroupBy(x => x.idx / 1000)
                                                            .Select(g => g.Select(a => a.x));

                foreach (var group in deleteGroup) {
                    var ids = group.Select(x => x).ToList();
                    entitiesToDelete.AddRange(entities.WholsellerNetPriceRow.Where( x=> ids.Contains(x.WholsellerNetPriceRowId) && x.WholsellerNetPrice.ActorCompanyId == actorCompanyId) );
                }

                foreach (var entity in entitiesToDelete)
                {
                    ChangeEntityState(entities, entity, SoeEntityState.Deleted,false);
                }

                entities.BulkSaveChanges();
            }

            return result;
        }

        public ActionResult Import(List<FileDTO> files, int sysWholesellerId, int priceListTypeId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (sysWholesellerId == (int)SoeWholeseller.RexelFI && SysPriceListManager.CompPriceListHeadExists(entitiesReadOnly,SoeCompPriceListProvider.RexelFINetto, actorCompanyId))
            {
                return new ActionResult(GetText(7771, "Importerade nettopriser enligt tidigare hantering måste tas bort innan import"));
            }

            var newPriceRows = new List<WholsellerNetPriceRowDTO>();

            IPriceListProvider provider = GetProviderAdapter(sysWholesellerId);
            if (provider == null)
            {
                return new ActionResult(GetText(7770, "Nettopris import saknas för vald grossist"+": " + WholeSellerManager.GetWholesellerName(sysWholesellerId) ) );
            }

            foreach (var file in files)
            {
                if (file.Bytes == null)
                    return new ActionResult("File is empty");

                if (ZipUtility.IsZipFile(file.Bytes, file.Name))
                {
                    var filesInZip = ZipUtility.UnzipFilesInZipFile(file.Bytes);
                    foreach(var fileInZip in filesInZip)
                    {
                        var readResult = HandleImportFile(newPriceRows, provider, fileInZip.Value, fileInZip.Key);
                        if (!readResult.Success)
                            return readResult;
                    }
                }
                else
                {
                    var readResult = HandleImportFile(newPriceRows, provider, file.Bytes, file.Name);
                    if (!readResult.Success)
                        return readResult;
                }
            }

            if (newPriceRows.Count == 0)
            {
                return new ActionResult("No product prices found in the files");
            }

            var saveResult = Save(actorCompanyId, sysWholesellerId, priceListTypeId, newPriceRows);
            if (!saveResult.Success)
            {
                return saveResult;
            }
            else
            {
                return new ActionResult { Success = true, InfoMessage = string.Format(GetText(7758, "Läste in {0} artikelpriser"), saveResult.ObjectsAffected)};
            }
        }

        private static ActionResult HandleImportFile(List<WholsellerNetPriceRowDTO> newPriceRows, IPriceListProvider provider, byte[] bytes, string fileName )
        {
            Stream stream = new MemoryStream(bytes);

            var result = provider.Read(stream, fileName);
            if (!result.Success)
                return result;

            var genericProvider = provider.ToGeneric();

            for (int i = 0; i < genericProvider.products.Count; i++)
            {
                var product = genericProvider.products[i] as GenericProduct;

                newPriceRows.Add(new WholsellerNetPriceRowDTO
                {
                    ProductNr = product.ProductId,
                    NetPrice = product.NetPrice,
                    GNP = product.Price,
                    ProductType = SysPriceListManager.GetExternalProductType(product.ProductType)
                });
            }

            return new ActionResult(true);
        }

        public List<InvoiceProductPriceSearchViewDTO> SearchProductPrices(CompEntities entities, int sysProductId, int wholesellerNetPriceid, bool addWholeSellerName, int actorCompanyId)
        {
            var netpriceItems = (from r in entities.WholsellerNetPriceRow
                                 where r.SysProductId == sysProductId &&
                                       r.WholsellerNetPrice.ActorCompanyId == actorCompanyId &&
                                       r.WholsellerNetPrice.WholsellerNetPriceId == wholesellerNetPriceid &&
                                       r.State == (int)SoeEntityState.Active
                                 select new InvoiceProductPriceSearchViewDTO
                                 {
                                     GNP = r.GNP ?? 0,
                                     NettoNettoPrice = r.NetPrice,
                                     SysWholesellerId = r.WholsellerNetPrice.SysWholeSellerId,
                                     ProductId = r.SysProductId,
                                     WholsellerNetPriceId = r.WholsellerNetPrice.WholsellerNetPriceId,
                                     CompanyWholesellerPriceListId = 0,
                                     PriceListOrigin = (int)PriceListOrigin.CompDbNetPriceList
                                 }).ToList();

            if (addWholeSellerName)
            {
                foreach (var item in netpriceItems)
                {
                    item.Wholeseller = WholeSellerManager.GetWholesellerName(item.SysWholesellerId);
                }
            }

            return netpriceItems;
        }
        public List<InvoiceProductPriceSearchViewDTO> SearchProductPrices(CompEntities entities, List<SysProductDTO> products,bool onlySysWholesellersWithCompletePriceList, int actorCompanyId)
        {
            var sysProductIds = products.Select(x => x.SysProductId).ToList();

            IQueryable<WholsellerNetPriceRow> query = (from r in entities.WholsellerNetPriceRow
                                                                 where sysProductIds.Contains(r.SysProductId) &&
                                                                       r.WholsellerNetPrice.ActorCompanyId == actorCompanyId &&
                                                                       r.State == (int)SoeEntityState.Active
                                                                 select r);

            var dbnetpriceItems = query.Select(r=> new InvoiceProductPriceSearchViewDTO
            {
                GNP = r.GNP ?? 0,
                NettoNettoPrice = r.NetPrice,
                SysWholesellerId = r.WholsellerNetPrice.SysWholeSellerId,
                ProductId = r.SysProductId,
                WholsellerNetPriceId = r.WholsellerNetPrice.WholsellerNetPriceId,
            }).ToList();

            var netpriceItems = new List<InvoiceProductPriceSearchViewDTO>();
            foreach (var itemGrp in dbnetpriceItems.GroupBy(x=> x.SysWholesellerId)) 
            {
                if (onlySysWholesellersWithCompletePriceList && !HasCompleteNetPriceList(itemGrp.Key))
                {
                    continue;
                }

                var item = itemGrp.First();

                var currentItem = products.FirstOrDefault(x => x.SysProductId == item.ProductId);
                item.Wholeseller = WholeSellerManager.GetWholesellerName(item.SysWholesellerId);
                if (currentItem != null)
                {
                    item.ProductProviderType = (SoeSysPriceListProviderType)currentItem.Type;
                    item.PurchaseUnit = "";
                    item.Number = currentItem.ProductId;
                    item.Name = currentItem.Name;
                    item.PriceListOrigin = (int)PriceListOrigin.CompDbNetPriceList;
                    item.CompanyWholesellerPriceListId = 0;
                    item.Code = GetText(7759, "Nettopris");
                }

                netpriceItems.Add(item);
            }

            return netpriceItems;
        }
        /*
        public List<InvoiceProductSearchViewDTO> SearchProducts(CompEntities entities, List<InvoiceProductSearchViewDTO> productSearchItems, int actorCompanyId)
        {
            var sysProductIds = productSearchItems.SelectMany(x => x.ProductIds).Select(y => y).Distinct().ToList();
            var sysCountryId = GetCompanySysCountryIdFromCache(entities, actorCompanyId);
            var syswholesellers = SysPriceListManager.GetSysWholesellersDict();
            var dbnetpriceItems = (from r in entities.WholsellerNetPriceRow
                                   where sysProductIds.Contains(r.SysProductId) &&
                                         r.WholsellerNetPrice.ActorCompanyId == actorCompanyId &&
                                         r.State == (int)SoeEntityState.Active
                                   select new InvoiceProductSearchViewDTO
                                   {

                                   }).ToList();
            var netpriceItems = new List<InvoiceProductSearchViewDTO>();
            foreach (var itemGrp in dbnetpriceItems.GroupBy(x => x.SysWholesellerId))
            {
                var item = itemGrp.First();
                var currentItem = productSearchItems.FirstOrDefault(x => x.ProductId == item.ProductId);
                item.Wholeseller = WholeSellerManager.GetSysWholesellerName(item.SysWholesellerId);
                if (currentItem != null)
                {
                    item.ProductProviderType = currentItem.ProductProviderType;
                    item.PurchaseUnit = currentItem.PurchaseUnit;
                    item.Number = currentItem.Number;
                    item.Name = currentItem.Name;
                    item.PriceListOrigin = (int)PriceListOrigin.CompDbPriceList;
                }
                netpriceItems.Add(item);
            }

            return netpriceItems;
        }
        */
        public static bool HasCompleteNetPriceList(int sysWholesellerId)
        {
            return WholesellersWithCompleteNetPriceList().Contains(sysWholesellerId);
        }

        public static List<int> WholesellersWithCompleteNetPriceList()
        {
            return new List<int>
            {
                { (int)SoeWholeseller.Lunda },
                { 85 }, //Lunda Styck Netto, to be removed in future
                { (int)SoeWholeseller.Elkedjan }
            };
        }

        private static List<int> WholesellersWithSeparateFile()
        {
            return new List<int>
            {
                { (int)SoeWholeseller.Lunda },
                { (int)SoeWholeseller.Dahl },
                { (int)SoeWholeseller.Sonepar },
                { (int)SoeWholeseller.Rexel },
                { (int)SoeWholeseller.Elkedjan },
                { (int)SoeWholeseller.RexelFI },
                { (int)SoeWholeseller.AhlsellFI },
                { (int)SoeWholeseller.AhlsellFIPL },
                { (int)SoeWholeseller.SoneparFI },
            };
        }

        public Dictionary<int, string> WholesellersWithNetPricesDict(bool onlyCurrentCountry, bool onlyWithSeparateFile)
        {
            var wholesellers = WholesellersWithNetPrices();

            if (onlyCurrentCountry)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var sysCountryId = GetCompanySysCountryIdFromCache(entitiesReadOnly, ActorCompanyId);
                wholesellers = wholesellers.Where(x => x.SysCountryId == sysCountryId).ToList();
            }

            if (onlyWithSeparateFile)
            {
                wholesellers = wholesellers.Where(x => WholesellersWithSeparateFile().Contains(x.SysWholesellerId)).ToList();
            }

            return wholesellers.OrderBy(x=> x.Name).ToDictionary(x=> x.SysWholesellerId, x => x.Name);
        }

        public static List<SysWholesellerDTO> WholesellersWithNetPrices()
        {
            return new List<SysWholesellerDTO>
            {
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.Ahlsell, Name = SoeWholeseller.Ahlsell.ToString(), SysCountryId = (int)TermGroup_Country.SE } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.Solar, Name = SoeWholeseller.Solar.ToString(), SysCountryId = (int)TermGroup_Country.SE } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.Dahl, Name = SoeWholeseller.Dahl.ToString(), SysCountryId = (int)TermGroup_Country.SE } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.Lunda, Name = SoeWholeseller.Lunda.ToString(), SysCountryId = (int)TermGroup_Country.SE } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.Elkedjan, Name = SoeWholeseller.Elkedjan.ToString(), SysCountryId = (int)TermGroup_Country.SE } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.Sonepar, Name = SoeWholeseller.Sonepar.ToString(), SysCountryId = (int)TermGroup_Country.SE } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.Rexel, Name = SoeWholeseller.Rexel.ToString(), SysCountryId = (int)TermGroup_Country.SE } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.RexelFI, Name = SoeWholeseller.RexelFI.ToString(), SysCountryId = (int)TermGroup_Country.FI } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.AhlsellFI, Name = SoeWholeseller.AhlsellFI.ToString(), SysCountryId = (int)TermGroup_Country.FI } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.AhlsellFIPL, Name = SoeWholeseller.AhlsellFIPL.ToString(), SysCountryId = (int)TermGroup_Country.FI } },
                { new SysWholesellerDTO{ SysWholesellerId = (int)SoeWholeseller.SoneparFI, Name = SoeWholeseller.SoneparFI.ToString(), SysCountryId = (int)TermGroup_Country.FI } },
            };
        }

        public static bool WholeSellerHasNetPriceList(int sysWholesellerId)
        {
            return WholesellersWithNetPrices().Any(x=> x.SysWholesellerId == sysWholesellerId);
        }

        private static IPriceListProvider GetProviderAdapter(int sysWholeSeller)
        {
            switch (sysWholeSeller)
            {
                case (int)SoeWholeseller.Lunda:    
                    return new Lunda(SoeCompPriceListProvider.Lunda);
                case (int)SoeWholeseller.Dahl:
                    return new DahlNetPrice();
                case (int)SoeWholeseller.Elkedjan:
                    return new ElkedjanNetto();
                case (int)SoeWholeseller.Sonepar:
                    return new SoneparNetto();
                case (int) SoeWholeseller.Rexel:
                    return new RexelNetto();
                case (int)SoeWholeseller.RexelFI:
                    return new FinnishProvider(SoeCompPriceListProvider.RexelFINetto);
                case (int)SoeWholeseller.AhlsellFI:
                    return new FinnishProvider(SoeCompPriceListProvider.AhlsellFINetto);
                case (int)SoeWholeseller.AhlsellFIPL:
                    return new FinnishProvider(SoeCompPriceListProvider.AhlsellFIPLNetto);
                case (int)SoeWholeseller.SoneparFI:
                    return new FinnishProvider(SoeCompPriceListProvider.SoneparFINetto);
            }

            return null;
        }
    }
}
