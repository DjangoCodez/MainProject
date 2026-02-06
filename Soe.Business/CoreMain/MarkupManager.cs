using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class MarkupManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public MarkupManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Markup

        public decimal GetDiscountBySysWholeseller(int actorCompanyId, int sysWholesellerId, string code)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.SupplierAgreement.NoTracking();
            return GetDiscountBySysWholeseller(entities, actorCompanyId, sysWholesellerId, code);
        }

        public decimal GetDiscountBySysWholeseller(CompEntities entities, int actorCompanyId, int sysWholesellerId, string code)
        {
            // Need to use real numbers here because SoeSysPriceListProvider does not match SysWholeSellerId in sys-DB

            // Check if syswholesellerid is AhlsellEl or AhlsellVVS
            if (sysWholesellerId == 14 || sysWholesellerId == 15)
                sysWholesellerId = (int)SoeSupplierAgreementProvider.Ahlsell;

            // Check if syswholesellerid is Storel7 och Storel8
            if (sysWholesellerId == 18 || sysWholesellerId == 19)
                sysWholesellerId = (int)SoeSupplierAgreementProvider.Storel;

            return (from sa in entities.SupplierAgreement
                    where sa.Company.ActorCompanyId == actorCompanyId &&
                    sa.SysWholesellerId == sysWholesellerId &&
                    sa.Code == code
                    select sa.DiscountPercent).FirstOrDefault();

        }

        public List<MarkupDTO> GetMarkup(int actorCompanyId, bool discounts)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Markup.NoTracking();
            return GetMarkup(entities, actorCompanyId, discounts);
        }

        public List<MarkupDTO> GetMarkup(CompEntities entities, int actorCompanyId, bool discounts)
        {
            var wholesellers = SysPriceListManager.GetSysWholesellersDict();
            var markups = new List<MarkupDTO>();

            var markupsData = (from m in entities.Markup
                               where m.Company.ActorCompanyId == actorCompanyId &&
                               m.State == (int)SoeEntityState.Active
                               select m).ToList();

            markupsData = discounts ? markupsData.Where(m => m.MarkupPercent == 0).ToList() : markupsData.Where(m => m.DiscountPercent == null || m.DiscountPercent == 0).ToList();

            foreach (var markup in markupsData)
            {
                markup.WholesellerName = markup.SysWholesellerId > 0 ? wholesellers[markup.SysWholesellerId].Name : "";

                //Ahlsell VVS och El ska båda mappas mot vanliga ahlsell
                int syswholesellerId = markup.SysWholesellerId;
                if (syswholesellerId == 14 || syswholesellerId == 15)
                    syswholesellerId = 2;

                markup.WholesellerDiscountPercent = markup.SysWholesellerId > 0 ? GetDiscountBySysWholeseller(actorCompanyId, syswholesellerId, markup.Code) : 0;
                markups.Add(markup.ToDTO());
            }

            if (discounts)
            {
                //Add the locksmith cases too
                var supAgreements = (from pl in entities.SupplierAgreement
                                     where pl.Company.ActorCompanyId == actorCompanyId && pl.CategoryId > 0
                                     select pl).ToList();


                foreach (var supAgreement in supAgreements)
                {
                    var markup = new MarkupDTO
                    {
                        MarkupId = supAgreement.RebateListId,
                        ActorCompanyId = actorCompanyId,
                        SysWholesellerId = supAgreement.SysWholesellerId,
                        Code = supAgreement.Code,
                        DiscountPercent = supAgreement.DiscountPercent,
                        State = (int)SoeEntityState.Active,
                        WholesellerName = supAgreement.SysWholesellerId > 0 ? wholesellers[supAgreement.SysWholesellerId].Name : "",
                        CategoryId = supAgreement.CategoryId,

                    };
                    markups.Add(markup);
                }
            }
            return markups;
        }

        public Markup GetSingleMarkup(CompEntities entities, int MarkupId)
        {
            return (from m in entities.Markup
                    where m.MarkupId == MarkupId
                    select m).FirstOrDefault();
        }

        public Markup GetSingleMarkup(CompEntities entities, int sysProductId, int companyWholesellerPriceListId, int actorCompanyId, int sysWholesellerId)
        {
            var companyPriceList = entities.CompanyWholesellerPricelist.FirstOrDefault(x => x.CompanyWholesellerPriceListId == companyWholesellerPriceListId && x.Company.ActorCompanyId == actorCompanyId);
            var localsysWholesellerId = companyPriceList?.SysWholesellerId ?? sysWholesellerId;

            return GetSingleMarkup(entities, null, sysProductId, companyPriceList?.SysPriceListHeadId ?? 0, localsysWholesellerId, actorCompanyId, false);
        }

        public Markup GetSingleMarkup(CompEntities entities, List<int> categories, int sysProductId, int sysPriceListHeadId, int sysWholesellerId, int actorCompanyId, bool withDiscount)
        { 
            var sysProductItem = sysPriceListHeadId > 0 ? SysPriceListManager.SearchProductPrice(sysPriceListHeadId, sysProductId) : null;
            SysProductDTO sysProduct = null;

            if (sysProductItem == null)
            {
                sysProduct = SysPriceListManager.GetSysProduct(sysProductId);
            }
             
            if (sysProduct == null && sysProductItem == null)
            {
                return null;
            }

            var productNumber = sysProductItem?.ProductNumber ?? sysProduct.ProductId;

            IQueryable<Markup> query = (from m in entities.Markup
                                        where (m.SysWholesellerId == sysWholesellerId || m.SysWholesellerId < 0) &&
                                        (productNumber.StartsWith(m.ProductIdFilter) || m.ProductIdFilter == string.Empty) &&
                                        m.Company.ActorCompanyId == actorCompanyId &&
                                        // m.DiscountPercent != null &&
                                        m.State == (int)SoeEntityState.Active
                                        orderby m.ProductIdFilter descending, m.Code descending
                                        select m);

            if (sysProductItem != null)
            {
                query = query.Where(m=> m.Code == sysProductItem.ProductCode || m.Code == string.Empty);
            }
            else
            {
                query = query.Where(m => m.Code == string.Empty);
            }

            if (categories != null)
            {
                query = query.Where(m => !m.Category.HasValue || categories.Contains(m.Category.Value));
            }

            if (withDiscount)
            {
                query = query.Where(m => m.DiscountPercent != null);
            }

            return query.FirstOrDefault();
        }

        public decimal? GetDiscountByCustomerAndProduct(CompEntities entities, int customerId, int productGroupId, int actorCompanyId)
        {
            var productGroup = ProductGroupManager.GetProductGroup(entities, productGroupId);
            var categories = CategoryManager.GetCategories(entities, SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, customerId, actorCompanyId, onlyDefaultCategories: false).Select(c => c.CategoryId).ToList();

            //first search discount for a specific customer
            var queryCustomer = (from m in entities.Markup
                                 where
                                 m.Company.ActorCompanyId == actorCompanyId &&
                                 m.ProductIdFilter.ToLower() == productGroup.Code &&
                                (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId) &&
                                 m.DiscountPercent.HasValue &&
                                 m.State == (int)SoeEntityState.Active
                                 orderby m.DiscountPercent.Value descending
                                 select m.DiscountPercent);

            if (queryCustomer.Any())
                return queryCustomer.FirstOrDefault();

            //search with category if no customer discount found
            var queryCategory = (from m in entities.Markup
                                 where
                                 m.Company.ActorCompanyId == actorCompanyId &&
                                 m.ProductIdFilter.ToLower() == productGroup.Code &&
                                 (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value)) &&
                                 m.DiscountPercent.HasValue &&
                                 m.State == (int)SoeEntityState.Active
                                 orderby m.DiscountPercent.Value descending
                                 select m.DiscountPercent);

            return queryCategory.FirstOrDefault();

        }

        public decimal? GetDiscountByCustomerAndProduct(CompEntities entities, int customerId, Product product, int actorCompanyId)
        {
            List<Markup> intervalSearchBase = new List<Markup>();
            string search = product.Number.ToLower();
            bool externalProduct = false;//product.PriceListImportedId.HasValue;
            var categories = CategoryManager.GetCategories(entities, SoeCategoryType.Customer, SoeCategoryRecordEntity.Customer, customerId, actorCompanyId, onlyDefaultCategories: false).Select(c => c.CategoryId).ToList();

            IQueryable<Markup> query = (from m in entities.Markup
                                        where m.Company.ActorCompanyId == actorCompanyId &&
                                            m.State == (int)SoeEntityState.Active &&
                                            m.DiscountPercent.HasValue
                                        select m
                                        );
            if (!externalProduct)
            {
                //first search discount for a specific customer
                var exactCustomer = (from m in query
                                     where
                                        m.ProductIdFilter.Equals(search) &&
                                        (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId) &&
                                        (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                     orderby m.DiscountPercent.Value descending
                                     select m.DiscountPercent).FirstOrDefault();

                if (exactCustomer.HasValue)
                    return exactCustomer;

                //search with category if no customer discount found
                var exactCategory = (from m in query
                                     where
                                        m.ProductIdFilter.Equals(search) &&
                                        (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value)) &&
                                        (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                     orderby m.DiscountPercent.Value descending
                                     select m.DiscountPercent).FirstOrDefault();

                if (exactCategory.HasValue)
                    return exactCategory;
            }
            else
            {
                //first search discount for a specific customer
                var exactCustomer = (from m in query
                                     where
                                        m.ProductIdFilter.Equals(search) &&
                                        (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId)
                                     orderby m.DiscountPercent.Value descending
                                     select m.DiscountPercent).FirstOrDefault();

                if (exactCustomer.HasValue)
                    return exactCustomer;

                //search with category if no customer discount found
                var exactCategory = (from m in query
                                     where
                                        m.ProductIdFilter.Equals(search) &&
                                        (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value))
                                     orderby m.DiscountPercent.Value descending
                                     select m.DiscountPercent).FirstOrDefault();

                if (exactCategory.HasValue)
                    return exactCategory;
            }


            if (!externalProduct)
            {
                //first search discount for a specific customer
                intervalSearchBase = (from m in query
                                      where
                                          m.ProductIdFilter.Contains("#") &&
                                          (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId) &&
                                          (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                      orderby m.DiscountPercent.Value descending
                                      select m).ToList();

                //search with category if no customer discount found
                if (intervalSearchBase.Count == 0)
                {
                    intervalSearchBase = (from m in query
                                          where
                                              m.ProductIdFilter.Contains("#") &&
                                              (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value)) &&
                                              (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                          orderby m.DiscountPercent.Value descending
                                          select m).ToList();
                }

            }
            else
            {
                //first search discount for a specific customer
                intervalSearchBase = (from m in query
                                      where
                                          m.ProductIdFilter.Contains("#") &&
                                          (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId)
                                      orderby m.DiscountPercent.Value descending
                                      select m).ToList();

                //search with category if no customer discount found
                if (intervalSearchBase.Count == 0)
                {
                    intervalSearchBase = (from m in query
                                          where
                                              m.ProductIdFilter.Contains("#") &&
                                              (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value))
                                          orderby m.DiscountPercent.Value descending
                                          select m).ToList();
                }
            }

            foreach (var item in intervalSearchBase.OrderBy(i => i.SysWholesellerId))
            {
                string[] numbers = item.ProductIdFilter.Split('#');
                string start = numbers.FirstOrDefault();
                string end = numbers.LastOrDefault();
                if (product.Number.Length != start.Length)
                    continue;

                int s = string.Compare(product.Number, 0, start, 0, product.Number.Length);
                int m = string.Compare(product.Number, 0, end, 0, product.Number.Length);
                if (s > -1 && m <= 0)
                    return item.DiscountPercent.Value;
            }

            if (!externalProduct)
            {
                //first search discount for a specific customer
                var semiExactCustomer = (from m in query
                                         where
                                             search.StartsWith(m.ProductIdFilter.ToLower()) &&
                                             !m.ProductIdFilter.ToLower().Contains("#") &&
                                             (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId) &&
                                             (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                         orderby m.DiscountPercent.Value descending
                                         select m.DiscountPercent).FirstOrDefault();

                if (semiExactCustomer.HasValue)
                    return semiExactCustomer;

                //search with category if no customer discount found
                var semiExactCategory = (from m in query
                                         where
                                             search.StartsWith(m.ProductIdFilter.ToLower()) &&
                                             !m.ProductIdFilter.ToLower().Contains("#") &&
                                             (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value)) &&
                                             (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                         orderby m.DiscountPercent.Value descending
                                         select m.DiscountPercent).FirstOrDefault();

                if (semiExactCategory.HasValue)
                    return semiExactCategory;
            }
            else
            {
                //first search discount for a specific customer
                var semiExactCustomer = (from m in query
                                         where
                                             search.StartsWith(m.ProductIdFilter.ToLower()) &&
                                             !m.ProductIdFilter.ToLower().Contains("#") &&
                                             (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId)
                                         orderby m.DiscountPercent.Value descending
                                         select m.DiscountPercent).FirstOrDefault();

                if (semiExactCustomer.HasValue)
                    return semiExactCustomer;

                //search with category if no customer discount found
                var semiExactCategory = (from m in query
                                         where
                                            search.StartsWith(m.ProductIdFilter.ToLower()) &&
                                            !m.ProductIdFilter.ToLower().Contains("#") &&
                                            (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value))
                                         orderby m.DiscountPercent.Value descending
                                         select m.DiscountPercent).FirstOrDefault();

                if (semiExactCategory.HasValue)
                    return semiExactCategory;
            }

            if (!externalProduct)
            {
                //first search discount for a specific customer
                var defaultDiscountCustomer = (from m in query
                                               where
                                                   string.IsNullOrEmpty(m.ProductIdFilter) &&
                                                   (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId) &&
                                                   (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                               orderby m.DiscountPercent.Value descending
                                               select m.DiscountPercent).FirstOrDefault();

                if (defaultDiscountCustomer.HasValue)
                    return defaultDiscountCustomer;

                //search with category if no customer discount found
                var defaultDiscountCategory = (from m in query
                                               where
                                                   string.IsNullOrEmpty(m.ProductIdFilter) &&
                                                   (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value)) &&
                                                   (m.SysWholesellerId == -1 || m.SysWholesellerId == -2)
                                               orderby m.DiscountPercent.Value descending
                                               select m.DiscountPercent).FirstOrDefault();

                if (defaultDiscountCategory.HasValue)
                    return defaultDiscountCategory;
            }
            else
            {
                //first search discount for a specific customer
                var defaultDiscountCustomer = (from m in query
                                               where
                                                   string.IsNullOrEmpty(m.ProductIdFilter) &&
                                                   (m.ActorCustomerId.HasValue && m.ActorCustomerId > 0 && m.ActorCustomerId == customerId)
                                               orderby m.DiscountPercent.Value descending
                                               select m.DiscountPercent).FirstOrDefault();

                if (defaultDiscountCustomer.HasValue)
                    return defaultDiscountCustomer;

                //search with category if no customer discount found
                var defaultDiscountCategory = (from m in query
                                               where
                                                   string.IsNullOrEmpty(m.ProductIdFilter) &&
                                                   (m.Category.HasValue && m.Category > 0 && categories.Contains(m.Category.Value))
                                               orderby m.DiscountPercent.Value descending
                                               select m.DiscountPercent).FirstOrDefault();

                if (defaultDiscountCategory.HasValue)
                    return defaultDiscountCategory;
            }

            return 0;
        }

        public ActionResult SaveMarkup(List<MarkupDTO> changedItems, int actorCompanyId)
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
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        foreach (MarkupDTO markupInput in changedItems)
                        {
                            if (markupInput.SysWholesellerId != 0)
                            {
                                /*SysWholeseller wholeSeller = lWholeSellers.Where(e => e.SysWholesellerId == markupInput.SysWholesellerId).FirstOrDefault();
                                if (markupInput.CategoryId > 0)
                                {
                                    //If gategoryId selected then update the supplierAgreement table
                                    SupplierAgreement supAg = null;
                                    if (markupInput.MarkupId == 0)
                                    {
                                        supAg = new SupplierAgreement()
                                        {
                                            Company = company,
                                        };
                                        SetCreatedProperties(supAg);
                                        supAg.Code = markupInput.Code;
                                        supAg.SysWholesellerId = markupInput.SysWholesellerId;
                                        supAg.DiscountPercent = (decimal)markupInput.DiscountPercent;
                                        supAg.CodeType = 1;
                                        supAg.CategoryId = markupInput.CategoryId;

                                        if(wholeSeller != null)
                                            supAg.PriceListOrigin = wholeSeller.IsOnlyInComp ? (int)PriceListOrigin.CompDbPriceList : (int)PriceListOrigin.SysDbPriceList;

                                        entities.AddToSupplierAgreement(supAg);
                                    }
                                    else
                                    {
                                       var supAgreement = (from pl in entities.SupplierAgreement
                                                where pl.Company.ActorCompanyId == actorCompanyId && pl.RebateListId == markupInput.MarkupId
                                                select pl).FirstOrDefault();

                                        if (supAgreement != null)
                                       {
                                           supAg = supAgreement as SupplierAgreement;
                                           supAg.Code = markupInput.Code;
                                           supAg.SysWholesellerId = markupInput.SysWholesellerId;
                                           supAg.DiscountPercent = (decimal)markupInput.DiscountPercent;
                                           supAg.CodeType = 1;
                                           supAg.CategoryId = markupInput.CategoryId;

                                           if (wholeSeller != null)
                                                supAg.PriceListOrigin = wholeSeller.IsOnlyInComp ? (int)PriceListOrigin.CompDbPriceList : (int)PriceListOrigin.SysDbPriceList;

                                           SetModifiedProperties(supAg);
                                       }
                                       else
                                       {
                                            var supplierAgreement = new SupplierAgreement()
                                            {
                                                Company = company,
                                                Code = markupInput.Code,
                                                CodeType = 1,
                                                DiscountPercent = (decimal)markupInput.DiscountPercent,
                                                Date = DateTime.Now,
                                                SysWholesellerId = markupInput.SysWholesellerId,
                                                CategoryId = markupInput.CategoryId
                                            };

                                            SetCreatedProperties(supplierAgreement);

                                            entities.AddToSupplierAgreement(supplierAgreement);
                                       }

                                    }
                                }
                                else
                                {*/
                                Markup markup = null;
                                if (markupInput.MarkupId == 0)
                                {
                                    markup = new Markup()
                                    {
                                        Company = company
                                    };
                                    SetCreatedProperties(markup);
                                }
                                else
                                {
                                    markup = GetSingleMarkup(entities, markupInput.MarkupId);
                                    if (markup == null)
                                    {
                                        var supAgreement = (from pl in entities.SupplierAgreement
                                                            where pl.Company.ActorCompanyId == actorCompanyId && pl.RebateListId == markupInput.MarkupId
                                                            select pl).FirstOrDefault();

                                        if (supAgreement != null)
                                            DeleteEntityItem(entities, supAgreement);

                                        if (markupInput.State == SoeEntityState.Deleted)
                                            continue;

                                        markup = new Markup()
                                        {
                                            Company = company
                                        };
                                        SetCreatedProperties(markup);
                                    }
                                    else
                                    {

                                        SetModifiedProperties(markup);
                                    }
                                }

                                markup.SysWholesellerId = markupInput.SysWholesellerId;
                                markup.Code = markupInput.Code;
                                markup.ActorCustomerId = markupInput.ActorCustomerId;
                                markup.ProductIdFilter = markupInput.ProductIdFilter == null ? "" : markupInput.ProductIdFilter;
                                markup.MarkupPercent = markupInput.MarkupPercent;
                                markup.DiscountPercent = markupInput.DiscountPercent;
                                markup.Category = markupInput.CategoryId;
                                markup.State = (int)markupInput.State;

                                //}
                                result = SaveChanges(entities);
                                if (!result.Success)
                                    break;
                            }
                        }

                        //Commit transaction
                        if (result.Success)
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
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion

        #region PriceBasedMarkup

        public List<PriceBasedMarkupDTO> GetPriceBasedMarkups(int actorCompanyId, int? priceBasedMarkupId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PriceBasedMarkup.NoTracking();
            return GetPriceBasedMarkups(entities, actorCompanyId, priceBasedMarkupId);
        }

        public List<PriceBasedMarkupDTO> GetPriceBasedMarkups(CompEntities entities, int actorCompanyId, int? priceBasedMarkupId = null)
        {
            var markups = (from m in entities.PriceBasedMarkup
                           where m.ActorCompanyId == actorCompanyId &&
                           m.State == (int)SoeEntityState.Active
                           orderby m.PriceListTypeId, m.MinPrice
                           select new PriceBasedMarkupDTO
                           {
                               MaxPrice = m.MaxPrice,
                               MinPrice = m.MinPrice,
                               PriceBasedMarkupId = m.PriceBasedMarkupId,
                               MarkupPercent = m.MarkupPercent,
                               PriceListName = m.PriceListType.Name,
                               PriceListTypeId = m.PriceListTypeId
                           }).ToList();

            foreach (var markup in markups.Where(m => m.MaxPrice == int.MaxValue))
            {
                markup.MaxPrice = null;
            }

            if (priceBasedMarkupId.HasValue)
                markups = markups.Where(m => m.PriceBasedMarkupId == priceBasedMarkupId.Value).ToList();

            return markups;
        }

        public PriceBasedMarkup GetPriceBasedMarkup(CompEntities entities, int actorCompanyId, int priceBasedMarkupId)
        {
            return (from m in entities.PriceBasedMarkup
                    where m.ActorCompanyId == actorCompanyId &&
                    m.PriceBasedMarkupId == priceBasedMarkupId
                    select m).FirstOrDefault();
        }

        public bool PriceBasedMarkupExist(CompEntities entities, int actorCompanyId, int notPriceBasedMarkupId, int priceListTypeId, decimal minPrice, decimal maxPrice)
        {
            IQueryable<PriceBasedMarkup> query = (from m in entities.PriceBasedMarkup
                                                  where m.ActorCompanyId == actorCompanyId &&
                                                        m.PriceBasedMarkupId != notPriceBasedMarkupId &&
                                                        m.State == (int)SoeEntityState.Active &&
                                                        minPrice >= m.MinPrice &&
                                                        maxPrice <= m.MaxPrice
                                                  select m);

            query = (priceListTypeId > 0) ? query.Where(m => m.PriceListTypeId == priceListTypeId) : query.Where(m => !m.PriceListTypeId.HasValue);

            return query.Any();
        }

        public ActionResult SavePriceBasedMarkup(PriceBasedMarkupDTO dto, int actorCompanyId)
        {
            if (dto == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceBasedMarkup");

            // Amount fix
            if (dto.MaxPrice == null || dto.MaxPrice == 0)
                dto.MaxPrice = int.MaxValue;

            if (dto.MinPrice == null)
            {
                dto.MinPrice = 0;
            }

            // Default result is successful
            var result = new ActionResult(true);

            using (var entities = new CompEntities())
            {
                try
                {
                    // Get existing 
                    var priceBasedMarkup = GetPriceBasedMarkup(entities, actorCompanyId, dto.PriceBasedMarkupId);

                    if (dto.MinPrice > dto.MaxPrice)
                    {
                        return new ActionResult(7609, GetText(7609, "Belopp från kan inte vara större än belopp till"));
                    }

                    if (PriceBasedMarkupExist(entities, actorCompanyId, dto.PriceBasedMarkupId, dto.PriceListTypeId.GetValueOrDefault(), dto.MinPrice.GetValueOrDefault(), dto.MaxPrice.GetValueOrDefault()))
                    {
                        return new ActionResult(7608, GetText(7608, "Samma prislista och prisintervall är redan upplagt"));
                    }

                    if (priceBasedMarkup == null)
                    {
                        priceBasedMarkup = new PriceBasedMarkup()
                        {
                            //Set FK
                            ActorCompanyId = actorCompanyId,
                        };
                        SetCreatedProperties(priceBasedMarkup);
                        entities.PriceBasedMarkup.AddObject(priceBasedMarkup);
                    }
                    else
                    {
                        SetModifiedProperties(priceBasedMarkup);
                    }



                    priceBasedMarkup.MinPrice = dto.MinPrice;
                    priceBasedMarkup.MaxPrice = dto.MaxPrice;
                    priceBasedMarkup.MarkupPercent = dto.MarkupPercent;
                    priceBasedMarkup.PriceListTypeId = dto.PriceListTypeId.GetValueOrDefault() > 0 ? dto.PriceListTypeId.Value : (int?)null;

                    result = SaveChanges(entities);

                    if (result.Success)
                        result.IntegerValue = priceBasedMarkup.PriceBasedMarkupId;
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.IntegerValue = 0;
                    result.Value = 0;
                }

                return result;
            }
        }

        public ActionResult SavePriceBasedMarkup2(List<PriceBasedMarkupDTO> changedItems, int actorCompanyId)
        {
            if (changedItems == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "PriceBasedMarkup");

            // Default result is successful
            var result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        foreach (PriceBasedMarkupDTO markupInput in changedItems)
                        {

                            PriceBasedMarkup priceBasedMarkup = null;

                            // Amount fix
                            if (markupInput.MaxPrice == null || markupInput.MaxPrice == 0)
                                markupInput.MaxPrice = int.MaxValue;

                            if (markupInput.MinPrice == null)
                                markupInput.MinPrice = 0;

                            if (markupInput.MinPrice > markupInput.MaxPrice)
                                return new ActionResult(7609, GetText(7609, "Belopp från kan inte vara större än belopp till"));

                            if (PriceBasedMarkupExist(entities, actorCompanyId, markupInput.PriceBasedMarkupId, markupInput.PriceListTypeId.GetValueOrDefault(), markupInput.MinPrice.GetValueOrDefault(), markupInput.MaxPrice.GetValueOrDefault()))
                                return new ActionResult(7608, GetText(7608, "Samma prislista och prisintervall är redan upplagt"));


                            //Create New
                            if (markupInput.PriceBasedMarkupId == 0)
                            {
                                priceBasedMarkup = new PriceBasedMarkup()
                                {
                                    Company = company
                                };
                                SetCreatedProperties(priceBasedMarkup);
                            }
                            else
                            {
                                // Get existing
                                priceBasedMarkup = GetPriceBasedMarkup(entities, actorCompanyId, markupInput.PriceBasedMarkupId);
                                
                                if (priceBasedMarkup != null)
                                {
                                    //Delete
                                    if (markupInput.State == SoeEntityState.Deleted)  
                                        ChangeEntityState(entities, priceBasedMarkup, SoeEntityState.Deleted, true);
                                    
                                    //Update
                                    else
                                        SetModifiedProperties(priceBasedMarkup);
                                }

                                if (priceBasedMarkup == null)
                                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "PriceBasedMarkup");
                            }

                            priceBasedMarkup.MinPrice = markupInput.MinPrice;
                            priceBasedMarkup.MaxPrice = markupInput.MaxPrice;
                            priceBasedMarkup.MarkupPercent = markupInput.MarkupPercent;
                            priceBasedMarkup.PriceListTypeId = markupInput.PriceListTypeId.GetValueOrDefault() > 0 ? markupInput.PriceListTypeId.Value : (int?)null;
                            priceBasedMarkup.State = (int)markupInput.State;

                            result = SaveChanges(entities);
                            if (!result.Success)
                                break;
                            
                        }

                        //Commit transaction
                        if (result.Success)
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

                return result;
            }
        }

        public ActionResult DeletePriceBasedMarkup(int priceBasedMarkupId)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Get voucher head
                var priceBasedMarkup = GetPriceBasedMarkup(entities, base.ActorCompanyId, priceBasedMarkupId);
                if (priceBasedMarkup == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "PriceBasedMarkup");

                // Remove voucher
                return ChangeEntityState(entities, priceBasedMarkup, SoeEntityState.Deleted, true);
            }
        }

        public decimal? GetPriceBasedMarkup(CompEntities entities, int priceListTypeId, decimal gnpPrice, int actorCompanyId)
        {
            var markup = (from m in entities.PriceBasedMarkup
                          where
                            (m.PriceListTypeId == priceListTypeId || !m.PriceListTypeId.HasValue) &&
                            (gnpPrice >= m.MinPrice && gnpPrice <= m.MaxPrice) &&
                            m.State == (int)SoeEntityState.Active &&
                            m.ActorCompanyId == actorCompanyId
                          select m).ToList();

            if (markup.Any(m => m.PriceListTypeId == priceListTypeId))
                return markup.FirstOrDefault(m => m.PriceListTypeId == priceListTypeId).MarkupPercent;
            else if (markup.Any())
                return markup.FirstOrDefault().MarkupPercent;
            else
                return null;
        }

        #endregion

    }
}
