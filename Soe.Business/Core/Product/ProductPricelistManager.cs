using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ProductPricelistManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ProductPricelistManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region PriceListType

        public List<PriceListTypeGridDTO> GetPriceListTypesForGrid(int actorCompanyId, int? priceListTypeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceListType.NoTracking();
            return GetPriceListTypesForGrid(entities, actorCompanyId, priceListTypeId);
        }

        public List<PriceListTypeGridDTO> GetPriceListTypesForGrid(CompEntities entities, int actorCompanyId, int? priceListTypeId = null)
        {
            IQueryable<PriceListType> query = (from plt in entities.PriceListType
                                               where ((plt.Company.ActorCompanyId == actorCompanyId) &&
                                              (plt.State == (int)SoeEntityState.Active))
                                               orderby plt.Name
                                               select plt);
                        
            if (priceListTypeId.HasValue)
            {
                query = query.Where(x => x.PriceListTypeId == priceListTypeId);
            }

            var data = query.Select(plt => new PriceListTypeGridDTO
            {
                Name = plt.Name,
                Description = plt.Description,
                CurrencyId = plt.Currency.CurrencyId,
                PriceListTypeId = plt.PriceListTypeId,
                SysCurrencyId = plt.Currency.SysCurrencyId,
                InclusiveVat = plt.InclusiveVat,
                IsProjectPriceList = plt.IsProjectPriceList ?? false
            }).ToList();

            foreach (var row in data)
            {
                if (row.SysCurrencyId.HasValue)
                {
                    row.Currency = CountryCurrencyManager.GetCurrencyCode(row.SysCurrencyId.Value);
                }
            }

            return data;
        }

        public List<PriceListType> GetPriceListTypes(int actorCompanyId, bool includePriceList = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceListType.NoTracking();
            return GetPriceListTypes(entities, actorCompanyId, includePriceList);
        }

        public List<PriceListType> GetPriceListTypes(CompEntities entities, int actorCompanyId, bool includePriceList = false)
        {
            IQueryable<PriceListType> query = entities.PriceListType.Include("Currency").Include("Company");

            if (includePriceList)
            {
                query = query.Include("PriceList");
            }

            return (from plt in query
                    where ((plt.Company.ActorCompanyId == actorCompanyId) &&
                    (plt.State == (int)SoeEntityState.Active))
                    orderby plt.Name
                    select plt).ToList();
        }

        public Dictionary<int, string> GetPriceListTypesDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            var priceListsTypes = GetPriceListTypes(actorCompanyId);
            foreach (PriceListType priceListType in priceListsTypes)
            {
                dict.Add(priceListType.PriceListTypeId, priceListType.Name);
            }

            return dict;
        }

        public PriceListType GetPriceListType(int priceListTypeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceListType.NoTracking();
            return GetPriceListType(entities, priceListTypeId, actorCompanyId);
        }

        public PriceListType GetPriceListType(CompEntities entities, int priceListTypeId, int actorCompanyId)
        {
            if (priceListTypeId == 0)
                return null;

            return (from plt in entities.PriceListType
                        .Include("Currency")
                    where (plt.PriceListTypeId == priceListTypeId &&
                    plt.Company.ActorCompanyId == actorCompanyId &&
                    plt.State == (int)SoeEntityState.Active)
                    select plt).FirstOrDefault();
        }

        public PriceListType GetStandardPriceListType(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            int defaultPriceListTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultPriceListType, 0, actorCompanyId, 0);
            if (defaultPriceListTypeId != 0)
            {
                return GetPriceListType(entities, defaultPriceListTypeId, actorCompanyId);
            }
            return null;
        }

        public bool PriceListExist(string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceListType.NoTracking();
            return PriceListExist(entities, name, actorCompanyId);
        }

        public bool PriceListExist(CompEntities entities, string name, int actorCompanyId)
        {
            int counter = (from plt in entities.PriceListType
                           where ((plt.Name.ToLower() == name.ToLower()) &&
                           (plt.Company.ActorCompanyId == actorCompanyId) &&
                           (plt.State == (int)SoeEntityState.Active))
                           select plt).Count();

            if (counter > 0)
                return true;
            return false;
        }

        public bool PriceListExistCheckId(int priceListTypeId, string name, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceListType.NoTracking();
            return PriceListExistCheckId(entities, priceListTypeId, name, actorCompanyId);
        }

        public bool PriceListExistCheckId(CompEntities entities, int priceListTypeId, string name, int actorCompanyId)
        {
            return (from plt in entities.PriceListType
                    where (priceListTypeId == 0 || plt.PriceListTypeId != priceListTypeId) &&
                    ((plt.Name.ToLower() == name.ToLower()) &&
                    (plt.Company.ActorCompanyId == actorCompanyId) &&
                    (plt.State == (int)SoeEntityState.Active))
                    select plt).Any();

        }

        public ActionResult SavePriceListType(PriceListTypeDTO priceListTypeDTO, List<PriceListDTO> priceLists)
        {
            ActionResult result = new ActionResult();
            int priceListTypeId = priceListTypeDTO.PriceListTypeId;

            using (CompEntities entities = new CompEntities())
            {
                if (!FeatureManager.HasRolePermission(Feature.Billing_Preferences_InvoiceSettings_Pricelists_Edit, Permission.Modify, base.RoleId, base.ActorCompanyId, entities: entities))
                {
                    return new ActionResult(GetText(5973, "Behörighet saknas"));
                }

                bool isNew = priceListTypeDTO.PriceListTypeId == 0;

                PriceListType priceListType = isNew ? new PriceListType() : GetPriceListType(entities, priceListTypeDTO.PriceListTypeId, this.ActorCompanyId);

                if (priceListType == null)
                {
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceListType");
                }

                priceListType.Name = priceListTypeDTO.Name;
                priceListType.Description = priceListTypeDTO.Description;
                priceListType.DiscountPercent = priceListTypeDTO.DiscountPercent;
                priceListType.InclusiveVat = priceListTypeDTO.InclusiveVat;
                priceListType.IsProjectPriceList = priceListTypeDTO.IsProjectPriceList;

                if (isNew)
                {
                    result = AddPriceListType(entities, priceListType, this.ActorCompanyId, priceListTypeDTO.CurrencyId);
                }
                else
                {
                    result = UpdatePriceListType(entities, priceListType, this.ActorCompanyId, priceListTypeDTO.CurrencyId);
                }

                priceListTypeId = result.IntegerValue = priceListType.PriceListTypeId;

                if (!result.Success || priceLists.IsNullOrEmpty())
                {
                    return result;
                }

                result = SavePriceLists(entities, null, priceListType, priceLists, base.ActorCompanyId);
                if (result.Success)
                {
                    result = SaveChanges(entities);
                }
            }

            if (result.Success)
            {
                result.IntegerValue = priceListTypeId;
            }
            return result;
        }
        public ActionResult SavePriceListTypeDTO(PriceListTypeDTO priceListTypeDTO, int actorCompanyId)
        {
            PriceListType priceListType = new PriceListType()
            {
                PriceListTypeId = priceListTypeDTO.PriceListTypeId,
                Name = priceListTypeDTO.Name,
                Description = priceListTypeDTO.Description,
                DiscountPercent = priceListTypeDTO.DiscountPercent,
                InclusiveVat = priceListTypeDTO.InclusiveVat,
                IsProjectPriceList = priceListTypeDTO.IsProjectPriceList,
            };

            if (priceListType.PriceListTypeId > 0)
                return this.UpdatePriceListType(priceListType, base.ActorCompanyId, priceListTypeDTO.CurrencyId);
            else
                return this.AddPriceListType(priceListType, base.ActorCompanyId, priceListTypeDTO.CurrencyId);
        }

        public ActionResult AddPriceListType(PriceListType priceListType, int actorCompanyId, int currencyId)
        {
            if (priceListType == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceListType");

            using (CompEntities entities = new CompEntities())
            {
                return AddPriceListType(entities, priceListType, actorCompanyId, currencyId);
            }
        }

        public ActionResult AddPriceListType(CompEntities entities, PriceListType priceListType, int actorCompanyId, int currencyId)
        {
            if (priceListType == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceListType");

            if (PriceListExist(priceListType.Name, actorCompanyId))
                return new ActionResult(1902, GetText(1902, "Prislista finns redan"));

            priceListType.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (priceListType.Company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            priceListType.Currency = CountryCurrencyManager.GetCurrency(entities, currencyId);
            if (priceListType.Currency == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

            var result = AddEntityItem(entities, priceListType, "PriceListType");

            if (result.Success)
                result.IntegerValue = priceListType.PriceListTypeId;

            return result;
        }

        public ActionResult UpdatePriceListType(PriceListType priceListType, int actorCompanyId, int currencyId)
        {
            if (priceListType == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceListType");

            using (CompEntities entities = new CompEntities())
            {
                return UpdatePriceListType(entities, priceListType, ActorCompanyId, currencyId);
            }
        }

        public ActionResult UpdatePriceListType(CompEntities entities, PriceListType priceListType, int actorCompanyId, int currencyId)
        {
            if (priceListType == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceListType");

            if (PriceListExistCheckId(priceListType.PriceListTypeId, priceListType.Name, actorCompanyId))
                return new ActionResult(1902, GetText(1902, "Prislista finns redan"));

            PriceListType originalPriceListType = GetPriceListType(entities, priceListType.PriceListTypeId, actorCompanyId);
            if (originalPriceListType == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "PriceListType");

            originalPriceListType.Currency = CountryCurrencyManager.GetCurrency(entities, currencyId);
            if (originalPriceListType.Currency == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

            return UpdateEntityItem(entities, originalPriceListType, priceListType, "PriceListType");
        }

        public ActionResult DeletePriceListType(int priceListTypeId, int actorCompanyId)
        {
            if (!FeatureManager.HasRolePermission(Feature.Billing_Preferences_InvoiceSettings_Pricelists_Edit, Permission.Modify, base.RoleId, base.ActorCompanyId))
            {
                return new ActionResult(GetText(5973, "Behörighet saknas"));
            }

            using (CompEntities entities = new CompEntities())
            {
                PriceListType originalPriceListType = GetPriceListType(entities, priceListTypeId, actorCompanyId);
                if (originalPriceListType == null)
                {
                    return new ActionResult(GetText(3272, "Ingen prislista funnen"));
                }

                var result = ValidatePriceListInUse(entities, priceListTypeId, actorCompanyId);
                if (!result.Success)
                {
                    return result;
                }

                var priceLists = GetPriceListsByType(entities, priceListTypeId).ToList();
                foreach(var priceList in priceLists)
                {
                    priceList.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(priceList);
                }

                originalPriceListType.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(originalPriceListType);
                return SaveChanges(entities);
            }
        }

        private ActionResult ValidatePriceListInUse(CompEntities entities, int priceListTypeId, int actorCompanyId)
        {
            int defaultPriceListTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultPriceListType, 0, actorCompanyId, 0);
            if (defaultPriceListTypeId == priceListTypeId)
            {
                return new ActionResult(GetText(7746, "Prislistan används och kan därför inte tas bort") + ": " + GetText(6414, "Standardprislista"));
            }

            var hasCustomer = entities.Customer.Any(x => x.ActorCompanyId == actorCompanyId && x.State == (int)SoeEntityState.Active && x.PriceListTypeId == priceListTypeId);
            if (hasCustomer)
            {
                return new ActionResult(GetText(7746, "Prislistan används och kan därför inte tas bort") + ": " + GetText(1050, "Kunder"));
            }

            var hasPriceFormula = entities.PriceRule.Any(x => x.Company.ActorCompanyId == actorCompanyId && x.PriceListType.PriceListTypeId == priceListTypeId);
            if (hasPriceFormula)
            {
                return new ActionResult(GetText(7640, "Prislistan är kopplad mot en prisformel och kan därför ej tas bort"));
            }

            return new ActionResult();
        }

        public ActionResult UpdatePrices(int actorCompanyId, List<int> priceListTypeIds, PriceUpdateDTO priceUpdate, bool updateExisting, DateTime? dateFromIn, DateTime? dateToIn, string productNrFrom, string productNrTo, int materialCodeId, int vatType, int productGroupId, DateTime priceComparisonDate, decimal quantityFrom, decimal? quantityTo)
        {
            if (!FeatureManager.HasRolePermission(Feature.Billing_Preferences_InvoiceSettings_Pricelists_PriceUpdate, Permission.Modify, base.RoleId, base.ActorCompanyId))
                return new ActionResult(false);

            if (priceListTypeIds.IsNullOrEmpty())
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
                foreach (int priceListTypeId in priceListTypeIds)
                {
                    var priceListType = GetPriceListType(entities, priceListTypeId, actorCompanyId);
                    var transResult = new ActionResult();
                    try
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            var currentPrices = this.GetPriceListPrices(entities, actorCompanyId, priceListTypeId, false, priceComparisonDate);
                            
                            var filtered = GetFilteredPriceDTOs(entities, actorCompanyId, currentPrices, productNrFrom, productNrTo, materialCodeId, vatType, productGroupId, quantityFrom, quantityTo);
                            transResult = CreateNewPriceDTOs(filtered, priceUpdate, updateExisting, dateFrom, dateTo, out var priceLists);

                            var updatePrices = priceLists.Where(x=> x.PriceListId > 0).ToList();
                            var newPriceLists = priceLists.Where(x => x.PriceListId == 0).ToList();

                            //So the database contains the prices with new stop date...
                            if (transResult.Success && updatePrices.Any())
                            {
                                transResult = SavePriceLists(entities, transaction, null, priceListType, updatePrices, actorCompanyId);
                            }

                            if (transResult.Success && newPriceLists.Any())
                            {
                                transResult = SavePriceLists(entities, transaction, null, priceListType, newPriceLists, actorCompanyId);
                            }

                            if (transResult.Success)
                            {
                                transaction.Complete();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        base.LogError(ex, this.log);
                        transResult.Exception = ex;
                        transResult.IntegerValue = 0;
                        transResult.Success = false;
                    }

                    if (transResult.Success)
                    {
                        succeeded.Add(priceListType.Name);
                        objectsAffected += transResult.ObjectsAffected;
                    }
                    else
                    {
                        result.Success = false;
                        failed.Add(priceListType.Name+ ":"+ transResult.ErrorMessage);
                    }
                }
            }

            if (failed.Count > 0)
                message += GetText(9372, "Prisuppdatering för {0} misslyckades.").Replace("{0}", failed.JoinToString(",")) + "\n";
            if (succeeded.Count > 0)
                message += GetText(9373, "Prisuppdatering för {0} genomfördes framgångsrikt.").Replace("{0}", succeeded.JoinToString(",")) + "\n";

            result.ErrorMessage = message;
            result.ObjectsAffected = objectsAffected;

            return result;
        }

        private List<PriceListProductDTO> GetFilteredPriceDTOs(CompEntities entities, int actorCompanyId, List<PriceListProductDTO> prices, string productNrFrom, string productNrTo, int materialCodeId, int vatType, int productGroupId, decimal quantityFrom, decimal? quantityTo)
        {
            if (prices.IsNullOrEmpty())
                return null;

            var query = prices.AsQueryable();


            var productNrFromPadded = StringUtility.IsNumeric(productNrFrom) ? productNrFrom.PadLeft(100, '0') : productNrFrom;
            var productNrToPadded = StringUtility.IsNumeric(productNrTo) ? productNrTo.PadLeft(100, '0') : productNrTo;

            if (!string.IsNullOrEmpty(productNrFrom))
            {
                query = query.Where(p => string.Compare(p.NumberSort, productNrFromPadded) >= 0);
            }
            if (!string.IsNullOrEmpty(productNrTo))
            {
                query = query.Where(p => string.Compare(p.NumberSort, productNrToPadded) <= 0);
            }

            if (materialCodeId > 0 || vatType > 0 || productGroupId > 0)
            {
                HashSet<int> productIds = new HashSet<int>();
                prices.ForEach(p => productIds.Add(p.ProductId));
                var productIdQuery = entities.Product.OfType<InvoiceProduct>()
                    .Where(p => p.Company.Any(c => c.ActorCompanyId == actorCompanyId))
                    .Where(p => productIds.Contains(p.ProductId));

                if (materialCodeId > 0)
                    productIdQuery = productIdQuery.Where(p => p.TimeCodeId == materialCodeId);

                if (vatType > 0)
                    productIdQuery = productIdQuery.Where(p => p.VatType == vatType);

                if (productGroupId > 0)
                    productIdQuery = productIdQuery.Where(p => p.ProductGroupId == productGroupId);

                var filteredProductIds = productIdQuery.Select(r => r.ProductId).ToHashSet();
                query = query.Where(p => filteredProductIds.Contains(p.ProductId));
            }

            return query.ToList();
        }

        private ActionResult CreateNewPriceDTOs(List<PriceListProductDTO> prices, PriceUpdateDTO priceUpdate, bool updateExisting, DateTime fromDate, DateTime toDate, out List<PriceListDTO> newPriceLists)
        {
            newPriceLists = new List<PriceListDTO>();

            if (prices.IsNullOrEmpty())
                return new ActionResult();

            var defaultStart = new DateTime(1901, 1, 1);
            var defaultStop = new DateTime(9999, 1, 1);

            foreach (var price in prices)
            {
                var old = price.CloneDTO();

                if (updateExisting)
                {
                    //We want to update the existing price, i.e. not create a new one
                    if (toDate != defaultStop)
                        old.StopDate = toDate;

                    if (fromDate != defaultStart)
                        old.StartDate = fromDate;

                    old.Price = priceUpdate.NewPrice(price.Price, 4);
                }
                else
                {
                    if (fromDate == old.StartDate)
                    {
                        return new ActionResult(GetText(7733,"Det finns priser som har samma startdatum som nytt startdatum") + ": " + price.Number + " - " + price.Name);
                    }
                    //We want to create a new price. Set accurate stop date of original.
                    if (old.StopDate >= fromDate)
                    {
                        old.StopDate = fromDate.AddDays(-1);
                    }

                    var newPrice = old.CloneDTO();
                    newPrice.PriceListId = 0;
                    newPrice.StopDate = toDate;
                    newPrice.StartDate = fromDate;
                    newPrice.Price = priceUpdate.NewPrice(price.Price, 4);
                    newPriceLists.Add(newPrice);
                }

                //We always want to update the old entry.
                newPriceLists.Add(old);
            }

            return new ActionResult();
        }
        #endregion

        #region PriceList

        public PriceList GetPriceList(CompEntities entities, int priceListId)
        {
            return (from p in entities.PriceList
                    where (p.PriceListId == priceListId)
                    select p).FirstOrDefault();
        }

        public PriceList GetPriceList(CompEntities entities, int productId, int priceListTypeId)
        {
            return (
                    from p in entities.PriceList
                    where (p.ProductId == productId && 
                        p.PriceListTypeId == priceListTypeId &&
                        p.State == (int)SoeEntityState.Active
                        )
                    select p).FirstOrDefault();
        }

        public PriceList GetPriceList(CompEntities entities, int productId, int priceListTypeId, DateTime? priceDate, bool? inclusiveVat = null, bool includePriceListType = false, decimal quantity = 0)
        {
            DateTime currentDate = (priceDate != null) ? priceDate.Value : DateTime.Today;

            IQueryable<PriceList> query = (from p in entities.PriceList
                                           where p.ProductId == productId && 
                                            p.PriceListTypeId == priceListTypeId &&
                                            p.State == (int)SoeEntityState.Active
                                            //orderby p.StartDate
                                           select p);

            query = query.Where(p => (currentDate >= p.StartDate && currentDate <= p.StopDate) && p.Quantity <= quantity);
            query = query.OrderByDescending(x => x.Quantity).ThenBy(y => y.StartDate);

            if (includePriceListType)
            {
                query = query.Include("PriceListType");
            }

            var priceLists = (inclusiveVat.HasValue) ? query.Where(x => x.PriceListType.InclusiveVat == inclusiveVat.Value).ToList() : query.ToList();

            var priceList = priceLists.FirstOrDefault(p => p.StartDate != new DateTime(1901, 1, 1) && p.StopDate != new DateTime(9999, 1, 1));
            if (priceList != null)
                return priceList;

            priceList = priceLists.FirstOrDefault(p => p.StartDate != new DateTime(1901, 1, 1));
            if (priceList != null)
                return priceList; 

            return priceLists.FirstOrDefault(); //priceLists.LastOrDefault(p => currentDate.Date.CompareTo(p.StartDate) > -1 && currentDate.Date.CompareTo(p.StopDate.Date) < 1);
        }
               
        public PriceList GetPriceListForDate(CompEntities entities, int productId, int priceListTypeId, DateTime startDate)
        {
            return (from p in entities.PriceList
                    where (p.ProductId == productId && 
                            p.PriceListTypeId == priceListTypeId && 
                            p.State == (int)SoeEntityState.Active &&
                            p.StartDate == startDate)
                    select p).FirstOrDefault();
        }
        public List<PriceList> GetPriceListsForProduct(int productId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceList.NoTracking();
            return GetPriceListsForProduct(entities, productId, null);
        }
        public List<PriceList> GetPriceListsForProduct(CompEntities entities, int productId, bool? inclusiveVat = null)
        {
            var query = (from p in entities.PriceList.Include("PriceListType")
                         where (p.ProductId == productId &&
                                  p.State == (int)SoeEntityState.Active
                                )
                         select p);

            if (inclusiveVat.HasValue)
            {
                query = query.Where(x => x.PriceListType.InclusiveVat == inclusiveVat.Value);
            }

            return query.ToList();
        }

        public List<PriceList> GetPriceListsForProducts(CompEntities entities, List<int> productIds, int actorCompanyId)
        {
            var query = (from p in entities.PriceList.Include("PriceListType")
                         where productIds.Contains(p.ProductId) && 
                         p.PriceListType.State == (int)SoeEntityState.Active && 
                         p.State == (int)SoeEntityState.Active &&
                         p.PriceListType.Company.ActorCompanyId == actorCompanyId
                         select p);

            return query.ToList();
        }

        public IEnumerable<PriceList> GetPriceListsByType(CompEntities entities, int priceListTypeId)
        {
            return (from p in entities.PriceList
                    where (
                            p.PriceListTypeId == priceListTypeId &&
                            p.State == (int)SoeEntityState.Active
                            )
                    select p);
        }

        public List<PriceListProductDTO> GetPriceListPrices(int actorCompanyId, int priceListTypeId, bool loadAll)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetPriceListPrices(entities, actorCompanyId, priceListTypeId, loadAll);
        }

        public List<PriceListProductDTO> GetPriceListPrices(CompEntities entities, int actorCompanyId, int priceListTypeId, bool loadAll, DateTime? priceDate = null)
        {
            bool showSalesPrice = FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowSalesPrice, Permission.Readonly, base.RoleId, base.ActorCompanyId);
            bool showPurchasePrice = FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowPurchasePrice, Permission.Readonly, base.RoleId, base.ActorCompanyId);

            IQueryable<PriceList> query = (from item in entities.PriceList
                                          where item.PriceListTypeId == priceListTypeId &&
                                                item.State == (int)SoeEntityState.Active
                                          select item);

            if (priceDate.HasValue)
            {
                query = query.Where(p => priceDate >= p.StartDate && priceDate <= p.StopDate);
            }

            var priceListItems = (from item in query
                                           join ip in entities.Product.OfType<InvoiceProduct>()
                                           on item.ProductId equals ip.ProductId
                                           where item.PriceListTypeId == priceListTypeId &&
                                                 !ip.ExternalProductId.HasValue &&
                                                 ip.State == (int)SoeEntityState.Active
                                           select new PriceListProductDTO
                                           {
                                               Name = item.Product.Name,
                                               Number = item.Product.Number,
                                               ProductId = item.ProductId,
                                               Price = item.Price,
                                               PurchasePrice = ip.PurchasePrice,
                                               StartDate = item.StartDate,
                                               StopDate = item.StopDate,
                                               Quantity = item.Quantity,
                                               PriceListId = item.PriceListId,
                                               PriceListTypeId = priceListTypeId,
                                           }).ToList();

            //priceListItems = query.OrderByDescending(x => x.ProductId).ThenBy(q => q.Quantity).ThenBy(y => y.StartDate).ToList();
            if (loadAll)
            {
                var productsItems = (from p in entities.Product.OfType<InvoiceProduct>()
                                     where
                                         p.Company.Any(i => i.ActorCompanyId == actorCompanyId) &&
                                         p.State == (int)SoeEntityState.Active &&
                                         !p.ExternalProductId.HasValue &&
                                         !p.PriceList.Any(pl => pl.PriceListTypeId == priceListTypeId && pl.State == (int)SoeEntityState.Active)
                                     select new PriceListProductDTO
                                     {
                                         Number = p.Number,
                                         Name = p.Name,
                                         ProductId = p.ProductId,
                                         Price = 0,
                                         PurchasePrice = p.PurchasePrice,
                                         StartDate = new DateTime(1901, 1, 1),
                                         StopDate = new DateTime(9999, 1, 1),
                                     }).ToList();
                priceListItems.AddRange(productsItems);
            }

            if (!showPurchasePrice || !showSalesPrice)
            {
                priceListItems.ForEach(r =>
                {
                    r.PurchasePrice = showPurchasePrice ? r.PurchasePrice : 0;
                    r.Price = showSalesPrice ? r.PurchasePrice : 0;
                });
            }

            return priceListItems.OrderBy(o => o.Number).ThenBy(o => o.StartDate).ToList();
        }
        #region SavePriceList
        public ActionResult SavePriceList(CompEntities entities, TransactionScope transaction, int priceListTypeId, string name, Dictionary<int, decimal> prices, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            PriceListType priceListType = null;

            //New pricelisttype
            if (priceListTypeId == 0)
            {
                var company = CompanyManager.GetCompany(entities, actorCompanyId);
                var compCurrency = CountryCurrencyManager.GetCompanyBaseCurrency(entities, actorCompanyId);
                var currency = (from entry in entities.Currency
                                where entry.CurrencyId == compCurrency.CurrencyId
                                select entry).FirstOrDefault();

                if (currency == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Currency");

                priceListType = new PriceListType()
                {
                    Name = name,

                    //Set references
                    Company = company,
                    Currency = currency,
                };
                SetCreatedProperties(priceListType);
                entities.PriceListType.AddObject(priceListType);
            }
            else
            {
                priceListType = ProductPricelistManager.GetPriceListType(entities, priceListTypeId, actorCompanyId);
            }

            foreach (var productIdPricePair in prices)
            {
                Product product = ProductManager.GetProduct(entities, productIdPricePair.Key, false, loadPriceList: true);
                if (product == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "Product");

                PriceList priceList = GetPriceList(entities, product.ProductId, priceListType.PriceListTypeId);
                if (priceList == null)
                {
                    //Insert new
                    priceList = new PriceList()
                    {
                        Price = productIdPricePair.Value,

                        //Set references
                        Product = product,
                        PriceListType = priceListType,
                        StartDate = new DateTime(1901, 1, 1),
                        StopDate = new DateTime(9999, 1, 1),
                    };
                    entities.PriceList.AddObject(priceList);
                    priceListType.PriceList.Add(priceList);
                }
                else if (priceListTypeId == 0)
                {
                    //It should not be possible to end up here
                    return new ActionResult(1902, GetText(1902, "Prislista finns redan"));
                }
                else
                {
                    //Update
                    priceList.Price = productIdPricePair.Value;
                }

                result = this.SaveChanges(entities, transaction);
                if (!result.Success)
                    break;
            }

            result.IntegerValue = priceListType.PriceListTypeId;

            return result;
        }
        private void SetDefaultDates(PriceListDTO saveDto)
        {
            if (saveDto.StartDate == new DateTime(1, 1, 1))
                saveDto.StartDate = new DateTime(1901, 1, 1);
            if (saveDto.StopDate == new DateTime(1, 1, 1))
                saveDto.StopDate = new DateTime(9999, 1, 1);
        }
        private bool IsPriceDuplicate(CompEntities entities, PriceListDTO current, List<PriceListDTO> allIncomingPriceLists)
        {
            Expression<Func<PriceListDTO, bool>> predicateDto =
               p =>
                p.ProductId == current.ProductId &&
                p.State == SoeEntityState.Active &&
                p.PriceListTypeId == current.PriceListTypeId &&
                p.Quantity == current.Quantity &&
                ((current.StartDate >= p.StartDate && current.StartDate <= p.StopDate) || //We have allowed this before...
                (current.StopDate >= p.StartDate && current.StopDate <= p.StopDate) ||
                (current.StartDate <= p.StartDate && current.StopDate >= p.StopDate));

            var incomingduplicates = allIncomingPriceLists.AsQueryable().Count(predicateDto);
            if (incomingduplicates > 1)
            {
                return true;
            }

            Expression<Func<PriceList, bool>> predicate =
                p =>
                p.ProductId == current.ProductId &&
                p.PriceListId != current.PriceListId &&
                p.PriceListTypeId == current.PriceListTypeId &&
                p.Quantity == current.Quantity &&
                p.State == (int)SoeEntityState.Active &&
                ((current.StartDate >= p.StartDate && current.StartDate <= p.StopDate) ||
                (current.StopDate >= p.StartDate && current.StopDate <= p.StopDate) ||
                (current.StartDate <= p.StartDate && current.StopDate >= p.StopDate));

            return entities.PriceList.Any(predicate);
        }
        public ActionResult SavePriceListPrices(int actorCompanyId, int priceListTypeId, List<PriceListDTO> priceLists, List<PriceListDTO> deletedPriceLists)
        {
            if (priceLists == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceList");

            ActionResult result = new ActionResult(false);

            using (var entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        Product product = null;
                        PriceListType type = null;
                        DateTime startDate = new DateTime(1901, 1, 1);
                        DateTime stopDate = new DateTime(9999, 1, 1);

                        foreach (PriceListDTO dto in priceLists)
                        {
                            if (dto.StartDate == new DateTime(1, 1, 1))
                                dto.StartDate = startDate;
                            if (dto.StopDate == new DateTime(1, 1, 1))
                                dto.StopDate = stopDate;

                            PriceList priceList = /*dto.PriceListId > 0 ? GetPriceList(entities,dto.PriceListId) : */ GetPriceListForDate(entities, dto.ProductId, priceListTypeId, dto.StartDate);
                            if (priceList != null)
                            {
                                //Update
                                priceList.Price = dto.Price;
                                priceList.StartDate = dto.StartDate;
                                priceList.StopDate = dto.StopDate;
                                ProductManager.SetProductModified(entities, priceList.ProductId);
                            }
                            else
                            {
                                product = ProductManager.GetInvoiceProduct(entities, dto.ProductId, true);
                                type = ProductPricelistManager.GetPriceListType(entities, priceListTypeId, actorCompanyId);
                                priceList = new PriceList
                                {
                                    Product = product,
                                    ProductId = product.ProductId,
                                    PriceListTypeId = priceListTypeId,
                                    Price = dto.Price,
                                    DiscountPercent = 0,
                                    Quantity = dto.Quantity,
                                    PriceListType = type,
                                    SysPriceListTypeName = dto.SysPriceListTypeName,
                                    StartDate = dto.StartDate,
                                    StopDate = dto.StopDate
                                };
                                entities.PriceList.AddObject(priceList);
                                product.PriceList.Add(priceList);
                                SetModifiedProperties(product);
                            }
                        }
                        foreach (PriceListDTO deleted in deletedPriceLists)
                        {
                            if (deleted.StartDate == new DateTime(1, 1, 1))
                                deleted.StartDate = startDate;
                            if (deleted.StopDate == new DateTime(1, 1, 1))
                                deleted.StopDate = stopDate;

                            PriceList priceList = deleted.PriceListId > 0 ? GetPriceList(entities, deleted.PriceListId) : GetPriceListForDate(entities, deleted.ProductId, priceListTypeId, deleted.StartDate);
                            product = ProductManager.GetInvoiceProduct(entities, deleted.ProductId);

                            if (priceList != null)
                            {
                                //Delete
                                DeleteEntityItem(entities, priceList);
                                priceList.State = (int)SoeEntityState.Deleted;
                                SetModifiedProperties(product);
                            }
                        }

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
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }


                return result;
            } //END using entities
        }
        public ActionResult SavePriceLists(CompEntities entities, Product product, PriceListType priceListType, List<PriceListDTO> priceLists, int actorCompanyId)
        {
            if (priceLists == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceList");

            // Remove all current pricelists
            if (product != null && !product.PriceList.IsLoaded)
            {
                product.PriceList.Load();
            }
            if (priceListType != null && !priceListType.PriceList.IsLoaded)
            {
                priceListType.PriceList.Load();
            }

            bool trySetReferences(PriceList priceList, PriceListDTO dto)
            {
                if (product != null)
                    priceList.Product = product;
                else if (dto.ProductId > 0)
                    priceList.ProductId = dto.ProductId;
                else
                    return false;

                if (priceListType != null)
                    priceList.PriceListType = priceListType;
                else if (dto.PriceListTypeId > 0)
                    priceList.PriceListTypeId = dto.PriceListTypeId;
                else
                    return false;

                return true;
            }

            //Set dates before so duplicate check works...
            priceLists.ForEach(x => SetDefaultDates(x));

            foreach (var priceListDTO in priceLists)
            {
                PriceList priceList = priceListDTO.PriceListId > 0 ? GetPriceList(entities, priceListDTO.PriceListId) : new PriceList();

                if (priceList == null)
                    continue;

                if (priceList.PriceListId == 0)
                {
                    SetCreatedProperties(priceList);
                }
                else
                {
                    SetModifiedProperties(priceList);
                }

                if (priceListDTO.State == SoeEntityState.Deleted)
                {
                    priceList.State = (int)SoeEntityState.Deleted;
                    continue;
                }

                if (priceListDTO.Quantity < 0)
                {
                    return new ActionResult((int)ActionResultSave.DatesInvalid, GetText(7679, "Antal kan inte vara mindre än 0"));
                }

                if (priceListDTO.StartDate > priceListDTO.StopDate)
                {
                    return new ActionResult((int)ActionResultSave.DatesInvalid, GetText(3592, "Slutdatum tidigare än startdatum"));
                }

                if (IsPriceDuplicate(entities, priceListDTO, priceLists))
                {
                    return new ActionResult((int)ActionResultSave.Duplicate, GetText(7590, "Produkten har redan ett pris för valt datum och kvantitet") + $": {priceListDTO.StartDate}/{priceListDTO.StopDate}/{priceListDTO.Quantity}");
                }

                if (trySetReferences(priceList, priceListDTO)) //pricelist has been removed....
                {
                    priceList.Price = priceListDTO.Price;
                    priceList.DiscountPercent = 0;
                    priceList.Quantity = priceListDTO.Quantity;
                    priceList.SysPriceListTypeName = priceListDTO.SysPriceListTypeName;
                    priceList.StartDate = priceListDTO.StartDate;
                    priceList.StopDate = priceListDTO.StopDate;

                    if (priceList.PriceListId == 0)
                    {
                        if (product != null)
                            product.PriceList.Add(priceList);
                        if (priceListType != null)
                            priceListType.PriceList.Add(priceList);
                    }
                }
                else
                {
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceListType|Product");
                }
            }
            return new ActionResult();
        }

        public ActionResult SavePriceLists(CompEntities entities, TransactionScope transaction, Product product, PriceListType type, List<PriceListDTO> priceLists, int actorCompanyId)
        {
            var result = SavePriceLists(entities, product, type, priceLists, actorCompanyId);
            if (result.Success)
                return SaveChanges(entities, transaction);
            else
                return result;
        }

        #endregion

        #endregion
    }
}
