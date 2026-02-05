using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.PricelistProvider;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class ProductManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ProductManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Product

        public Product GetProduct(int productId, bool onlyActive, bool loadPriceList = false, bool loadProductUnit = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetProduct(entities, productId, onlyActive, loadPriceList, loadProductUnit);
        }

        public Product GetProduct(CompEntities entities, int productId, bool onlyActive, bool loadPriceList = false, bool loadProductUnit = false)
        {
            IQueryable<Product> query = entities.Product;
            if (loadPriceList)
                query = query.Include("PriceList");
            if (loadProductUnit)
                query = query.Include("ProductUnit");

            return (from p in query
                    where p.ProductId == productId &&
                    (!onlyActive || p.State == (int)SoeEntityState.Active)
                    select p).FirstOrDefault();
        }

        public Product GetProduct(int actorCompanyId, string number)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetProduct(entities, actorCompanyId, number);
        }

        public Product GetProduct(CompEntities entities, int actorCompanyId, string number)
        {
            return (from p in entities.Product
                        .Include("ProductUnit")
                        .Include("ProductGroup")
                    where p.State == (int)SoeEntityState.Active && p.Number == number &&
                    p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                    select p).FirstOrDefault();
        }

        public decimal GetProductPriceDecimal(int productId, int priceListTypeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetProductPriceDecimal(entities, productId, priceListTypeId);
        }

        public decimal GetProductPriceDecimal(CompEntities entities, int productId, int priceListTypeId)
        {
            return (from p in entities.PriceList
                    where p.ProductId == productId &&
                            p.PriceListType.PriceListTypeId == priceListTypeId &&
                            p.State == (int)SoeEntityState.Active
                    select p.Price).FirstOrDefault();
        }

        public decimal GetProductPriceForCustomerInvoice(int productId, int customerInvoiceId, int actorCompanyId, decimal quantity)
        {
            decimal result = 0;
            using (var entities = new CompEntities())
            {
                entities.Product.NoTracking();
                var invoice = InvoiceManager.GetCustomerInvoiceSmallEx(entities, customerInvoiceId);

                var price = GetProductPrice(actorCompanyId, new ProductPriceRequestDTO { PriceListTypeId = invoice.PriceListTypeId.GetValueOrDefault(), ProductId = productId, CustomerId = invoice.ActorId.GetValueOrDefault(), CurrencyId = invoice.CurrencyId, WholesellerId = invoice.SysWholesellerId.GetValueOrDefault(), Quantity = quantity });
                if (price != null)
                {
                    result = price.SalesPrice;
                }

            }

            return result;
        }

        public int GetProductId(int actorCompanyId, string number)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetProductId(entities, actorCompanyId, number);
        }

        public int GetProductId(CompEntities entities, int actorCompanyId, string number)
        {
            return (from p in entities.Product
                    where p.Number == number &&
                    p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                    select p.ProductId).FirstOrDefault();
        }

        public IEnumerable<Product> GetProducts(int actorCompanyId, DateTime? modifiedSince, bool includeInactive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetProducts(entities, actorCompanyId, modifiedSince, includeInactive);
        }

        public IEnumerable<Product> GetProducts(CompEntities entities, int actorCompanyId, DateTime? modifiedSince, bool includeInactive)
        {
            IQueryable<Product> query = (from p in entities.Product
                                         where
                                         p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                         select p);
            if (!includeInactive)
                query = query.Where(p => p.State == (int)SoeEntityState.Active);

            if (modifiedSince != null)
                query = query.Where(p => p.Modified >= modifiedSince || p.Created >= modifiedSince);

            return query.OrderBy(p => p.Number).ToList();
        }

        private ActionResult IsOkToInactivateProduct(CompEntities entities, InvoiceProduct product, int actorCompanyId)
        {
            if (product.IsStockProduct.GetValueOrDefault() && StockManager.InvoiceProductHasStockBalance(entities, product.ProductId, actorCompanyId))
            {
                return new ActionResult((int)ActionResultDelete.ProductHasStockBalanse, GetText(7651, "Kan inte ta bort/inaktivera artiklar med lagersaldo") + ": " + product.Number);
            }
            else
            {
                return new ActionResult();
            }

        }
        private ActionResult IsOkToChangeStateOnProduct(CompEntities entities, int productId, SoeProductType type, bool? isStockProduct, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            if (type == SoeProductType.InvoiceProduct)
            {
                //CustomerInvoiceRow
                if (entities.CustomerInvoiceRow.Any(i => i.ProductId.HasValue && i.ProductId.Value == productId && i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ProductHasInvoiceRows, GetText(8553, "Artikeln kunde inte tas bort, används för fakturarader"));
                //CustomerInvoiceInterest
                if (entities.CustomerInvoiceInterest.Any(i => i.InvoiceProduct.ProductId == productId))
                    return new ActionResult((int)ActionResultDelete.ProductHasInterestRows);
                //CustomerInvoiceReminder
                if (entities.CustomerInvoiceReminder.Any(i => i.InvoiceProduct.ProductId == productId))
                    return new ActionResult((int)ActionResultDelete.ProductHasReminderRows);
                //TimeCode
                if (entities.TimeCodeInvoiceProduct.Any(i => i.ProductId == productId))
                    return new ActionResult((int)ActionResultDelete.ProductHasTimeCodes);

                if (isStockProduct.GetValueOrDefault() && StockManager.InvoiceProductHasStockBalance(entities, productId, actorCompanyId))
                {
                    return new ActionResult((int)ActionResultDelete.ProductHasStockBalanse, GetText(7651, "Kan inte ta bort/inaktivera artiklar med lagersaldo"));
                }
            }
            else if (type == SoeProductType.PayrollProduct)
            {
                //TimePayrollTransaction
                if (entities.TimePayrollTransaction.Any(i => i.ProductId == productId && i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ProductHasTransactions);
                //TimePayrollScheduleTransaction
                if (entities.TimePayrollScheduleTransaction.Any(i => i.ProductId == productId && i.State == (int)SoeEntityState.Active))
                    return new ActionResult((int)ActionResultDelete.ProductHasTransactions);
                //TimeCodePayrollProduct
                if (entities.TimeCodePayrollProduct.Any(i => i.ProductId == productId))
                    return new ActionResult((int)ActionResultDelete.ProductHasTimeCodes);

                // PayrollProductSetting
                IEnumerable<PayrollProductSetting> settings = entities.PayrollProductSetting.Where(p => p.ProductId == productId && p.State == (int)SoeEntityState.Active);
                foreach (var setting in settings)
                {
                    //PayrollProductPriceFormula
                    if (setting.PayrollProductPriceFormula.Any())
                        return new ActionResult((int)ActionResultDelete.ProductHasPayrollProductPriceFormulas);
                    //PayrollProductPriceType
                    if (setting.PayrollProductPriceType.Any())
                        return new ActionResult((int)ActionResultDelete.ProductHasPayrollProductPriceTypes);
                }

                //TimeAbsenceRuleRow
                if (entities.TimeAbsenceRuleRow.Any(i => i.PayrollProductId == productId))
                    return new ActionResult((int)ActionResultDelete.ProductHasTimeAbsenceRules);
                //TimeAbsenceRuleRowPayrollProducts
                if (entities.TimeAbsenceRuleRowPayrollProducts.Any(i => i.SourcePayrollProductId == productId || i.TargetPayrollProductId == productId))
                    return new ActionResult((int)ActionResultDelete.ProductHasTimeAbsenceRulePayrollProducts);
            }

            return result;
        }

        public List<ProductStatisticsDTO> GetProductStatistics(IEnumerable<int> productIds, DateTime fromDate, DateTime toDate, SoeOriginType originType, bool includeServiceProducts)
        {
            var items = new List<ProductStatisticsDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            #region Prereq

            var actorCompanyId = base.ActorCompanyId;
            var userId = base.UserId;

            List<CompanySettingType> baseProductsToExclude = new List<CompanySettingType>();
            baseProductsToExclude.Add(CompanySettingType.ProductFreight);
            baseProductsToExclude.Add(CompanySettingType.ProductInvoiceFee);
            baseProductsToExclude.Add(CompanySettingType.ProductCentRounding);
            baseProductsToExclude.Add(CompanySettingType.ProductHouseholdTaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductHouseholdTaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductHousehold50TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductHousehold50TaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductFlatPrice);
            baseProductsToExclude.Add(CompanySettingType.ProductMisc);
            baseProductsToExclude.Add(CompanySettingType.ProductGuarantee);
            baseProductsToExclude.Add(CompanySettingType.ProductFlatPriceKeepPrices);
            baseProductsToExclude.Add(CompanySettingType.ProductRUTTaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductRUTTaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen15TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen15TaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen50TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen50TaxDeductionDenied);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen20TaxDeduction);
            baseProductsToExclude.Add(CompanySettingType.ProductGreen20TaxDeductionDenied);
            List<int> baseProductIds = GetBaseProductIds(baseProductsToExclude, actorCompanyId, userId);

            var invoiceText = GetText(2, (int)TermGroup.OriginType);
            var purchaseText = GetText(8, (int)TermGroup.OriginType);

            #endregion

            if (!FeatureManager.HasRolePermission(Feature.Billing_Statistics_Product, Permission.Readonly, base.RoleId, actorCompanyId))
            {
                return new List<ProductStatisticsDTO>();
            }

            if ((originType == SoeOriginType.CustomerInvoice || originType == SoeOriginType.None) && FeatureManager.HasRolePermission(Feature.Billing_Invoice_Invoices, Permission.Readonly, base.RoleId, actorCompanyId))
            {
                items.AddRange((from p in entitiesReadOnly.GetInvoiceProductStatisticsSales(base.ActorCompanyId, fromDate, toDate, string.Join(",", productIds), includeServiceProducts)
                                where !baseProductIds.Contains(p.ProductId)
                                select new ProductStatisticsDTO()
                                {
                                    ProductId = p.ProductId,
                                    ProductNr = p.ProductNr,
                                    ProductName = p.ProductName,
                                    Year = p.InvoiceDate.Value.Date.Year.ToString(),
                                    Month = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.InvoiceDate.Value.Date.Month)),
                                    CustomerInvoiceQty = p.InvoiceQuantity.Value,
                                    CustomerInvoiceAmount = p.CustomerInvoiceAmount,
                                    InvoiceDate = p.InvoiceDate.Value,
                                    InvoiceId = p.InvoiceId,
                                    InvoiceNr = p.InvoiceNr,
                                    OrderId = p.OrderId,
                                    OrderNr = p.OrderNr,
                                    OriginType = (SoeOriginType)p.OriginType,
                                    OriginTypeName = invoiceText,
                                    CustomerNr = p.ActorNr,
                                    CustomerName = p.ActorName,
                                    MarginalIncome = p.MarginalIncomeCurrency,
                                    MarginalRatio = p.MarginalIncomeRatio.Value,
                                    VatType = (TermGroup_InvoiceProductVatType)p.CalculationType
                                }));
            }
            if ((originType == SoeOriginType.Purchase || originType == SoeOriginType.None) && FeatureManager.HasRolePermission(Feature.Billing_Purchase, Permission.Readonly, base.RoleId, actorCompanyId))
            {
                items.AddRange((from p in entitiesReadOnly.GetInvoiceProductStatisticsPurchase(base.ActorCompanyId, fromDate, toDate, string.Join(",", productIds), includeServiceProducts)
                                where !baseProductIds.Contains(p.ProductId)
                                select new ProductStatisticsDTO()
                                {
                                    ProductId = p.ProductId,
                                    ProductNr = p.ProductNr,
                                    ProductName = p.ProductName,
                                    Year = p.PurchaseDeliveryDate.Value.Date.Year.ToString(),
                                    Month = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(p.PurchaseDeliveryDate.Value.Date.Month)),
                                    PurchaseQty = p.PurchaseQuantity,
                                    PurchaseDeliveryDate = p.PurchaseDeliveryDate.Value,
                                    PurchaseId = p.PurchaseId,
                                    PurchaseNr = p.PurchaseNr,
                                    OriginType = (SoeOriginType)p.OriginType,
                                    OriginTypeName = purchaseText,
                                    SupplierNr = p.ActorNr,
                                    SupplierName = p.ActorName,
                                    VatType = (TermGroup_InvoiceProductVatType)p.CalculationType
                                }));
            }

            return items;
        }

        public List<ProductCleanupDTO> GetProductsForCleanup(int actorCompanyId, DateTime lastUsedDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CustomerInvoiceRow.NoTracking();
            return GetProductsForCleanup(entities, actorCompanyId, lastUsedDate);
        }

        public List<ProductCleanupDTO> GetProductsForCleanup(CompEntities entities, int actorCompanyId, DateTime lastUsedDate)
        {
            var usedProducts = entities.CustomerInvoiceRow
                .Where(r => 
                    r.State == (int)SoeEntityState.Active && r.Type == (int)SoeInvoiceRowType.ProductRow 
                    && r.CustomerInvoice.Origin.ActorCompanyId == actorCompanyId)
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    LastUsedDate = g.Max(r => r.Created)
                });

            var query = from p in entities.Product.OfType<InvoiceProduct>()
                        join u in usedProducts on p.ProductId equals u.ProductId into joined
                        from sub in joined.DefaultIfEmpty()
                        where p.Company.Any(c => c.ActorCompanyId == actorCompanyId) && p.State == (int)SoeEntityState.Active
                        && (sub != null && sub.LastUsedDate < lastUsedDate)
                        select new ProductCleanupDTO
                        {
                            ProductId = p.ProductId,
                            ProductNumber = p.Number,
                            ProductName = p.Name,
                            IsExternal = p.ExternalProductId.HasValue,
                            LastUsedDate = (DateTime)sub.LastUsedDate,
                            IsActive = p.State == (int)SoeEntityState.Active,
                        };
            return query.OrderByDescending(p => p.LastUsedDate).ToList();  
        }

        #endregion

        #region ProductAccountStd

        public List<ProductComparisonDTO> GetProductComparisonDTOs(int actorCompanyId, int comparisonPriceListTypeId, int priceListTypeId, bool loadAllProducts, DateTime? priceDate = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            using (var entities = entitiesReadOnly)
            {
                IQueryable<InvoiceProduct> query;

                query = from entry in entities.Product.OfType<InvoiceProduct>()
                        .Include("PriceList")
                        where
                        entry.Company.Any(i => i.ActorCompanyId == actorCompanyId) &&
                        entry.State == (int)SoeEntityState.Active &&
                        !entry.ExternalProductId.HasValue
                        select entry;

                if (!loadAllProducts)
                {
                    query = from item in query
                            where
                            item.PriceList.Any(pl => (priceListTypeId > 0 && pl.PriceListTypeId == priceListTypeId) || (comparisonPriceListTypeId > 0 && pl.PriceListTypeId == comparisonPriceListTypeId))
                            select item;
                }

                var invoiceProducts = query.ToList();
                if (invoiceProducts.IsNullOrEmpty())
                    return new List<ProductComparisonDTO>();

                return query.ToProductComparisonDTOs(comparisonPriceListTypeId, priceListTypeId, priceDate).ToList();
            }
        }

        #endregion

        #region AccountingSettingDTO

        private void SaveAccountingSettings(CompEntities entities, Product product, List<AccountingSettingsRowDTO> accountingSettings, int actorCompanyId)
        {

            List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

            #region Delete AccountingSettings

            // Loop over existing settings
            foreach (ProductAccountStd productAccountStd in product.ProductAccountStd.ToList())
            {
                //Delete account
                productAccountStd.AccountInternal.Clear();
                product.ProductAccountStd.Remove(productAccountStd);
                entities.DeleteObject(productAccountStd);
            }

            #endregion

            #region Add AccountingSettings                           

            if (product.ProductAccountStd == null)
                product.ProductAccountStd = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<ProductAccountStd>();

            foreach (AccountingSettingsRowDTO settingInput in accountingSettings)
            {
                // Standard account

                AccountStd accStd = settingInput.Account1Id > 0 ? AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true) : null;

                var productAccountStd = new ProductAccountStd
                {
                    Type = settingInput.Type,
                    AccountStd = accStd
                };
                product.ProductAccountStd.Add(productAccountStd);

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
                        productAccountStd.AccountInternal.Add(accountInternal);
                }
            }

            #endregion            
        }

        private void SaveAccountingSettings(CompEntities entities, int actorCompanyId, PayrollProductSetting setting, List<AccountingSettingsRowDTO> accountItems)
        {
            #region Prereq

            List<AccountDim> dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true);

            #endregion

            #region Delete AccountingSettings

            // Loop over existing settings
            foreach (PayrollProductAccountStd productAccountStd in setting.PayrollProductAccountStd.ToList())
            {
                //Delete account
                productAccountStd.AccountInternal.Clear();
                setting.PayrollProductAccountStd.Remove(productAccountStd);
                entities.DeleteObject(productAccountStd);
            }

            #endregion

            #region Add AccountingSettings                           

            if (setting.PayrollProductAccountStd == null)
                setting.PayrollProductAccountStd = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<PayrollProductAccountStd>();

            foreach (AccountingSettingsRowDTO settingInput in accountItems)
            {
                // Standard account
                if (settingInput.Account1Id != 0)
                {
                    AccountStd accStd = AccountManager.GetAccountStd(entities, settingInput.Account1Id, actorCompanyId, true, true);
                    if (accStd != null)
                    {
                        PayrollProductAccountStd productAccountStd = new PayrollProductAccountStd
                        {
                            Type = settingInput.Type,
                            AccountStd = accStd
                        };
                        setting.PayrollProductAccountStd.Add(productAccountStd);

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
                                productAccountStd.AccountInternal.Add(accountInternal);
                        }
                    }
                }
            }

            #endregion            

        }

        private void AddInternalAccountsToStdAccount(CompEntities entities, int actorCompanyId, ProductAccountStd productAccountStd, List<AccountingSettingDTO> accountItems, int index)
        {
            // Add internal accounts
            foreach (AccountingSettingDTO intItem in accountItems.Where(a => a.DimNr != Constants.ACCOUNTDIM_STANDARD))
            {
                int intAccountId = 0;
                if (index == 1)
                    intAccountId = intItem.Account1Id;
                else if (index == 2)
                    intAccountId = intItem.Account2Id;
                else if (index == 3)
                    intAccountId = intItem.Account3Id;
                else if (index == 4)
                    intAccountId = intItem.Account4Id;
                else if (index == 5)
                    intAccountId = intItem.Account5Id;

                AccountInternal accountInt = AccountManager.GetAccountInternal(entities, intAccountId, actorCompanyId);
                if (accountInt != null)
                    productAccountStd.AccountInternal.Add(accountInt);
            }
        }

        #endregion

        #region InvoiceProductSearchView

        public List<CompanyWholesellerPriceListViewDTO> GetCompanyWholeSellerPriceLists(CompEntities entities, int actorCompanyId, int? sysWholeSellerId)
        {
            var query = entities.CompanyWholeSellerPriceListUsedView.Where(p => p.ActorCompanyId == actorCompanyId && p.PriceListOrigin == (int)PriceListOrigin.SysDbPriceList);
            if (sysWholeSellerId.HasValue)
            {
                query = query.Where(p => p.SysWholesellerId == sysWholeSellerId.Value);
            }

            return query.ToDTOs().ToList();
        }

        public InvoiceProductPriceSearchViewDTO GetExternalInvoiceProductByProductNumber(string productNumber, int actorCompanyId, ref int sysWholeSellerId, bool tryAll = false, List<SysWholeseller> sysWholesellers = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetExternalInvoiceProductByProductNr(entities, productNumber, actorCompanyId, ref sysWholeSellerId, tryAll, sysWholesellers);
        }

        public InvoiceProductPriceSearchViewDTO GetExternalInvoiceProductByProductNr(CompEntities entities, string productNumber, int actorCompanyId, ref int sysWholeSellerId, bool tryAll = false, List<SysWholeseller> sysWholesellers = null)
        {
            try
            {
                //External search with stored procedure to find sysproduct by number

                var companyPriceList = GetCompanyWholeSellerPriceLists(entities, actorCompanyId, sysWholeSellerId);
                var result = SysPriceListManager.SearchProductPrice(productNumber, companyPriceList);

                //var result2 = entities.ExternalProductSearchByNumber(actorCompanyId, productNumber, sysWholeSellerId).FirstOrDefault();
                if (result == null)
                {
                    bool tryAgain = false;

                    #region SysWholesellerId

                    // TODO: Hardcoded workarounds! Fix this with SysWholeSellerGroups
                    if (sysWholeSellerId == 5)
                    {
                        // Alvesta has been merged into Solar.
                        // If product not found in Solars pricelist, check Alvestas pricelist.
                        sysWholeSellerId = 9;
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 2)
                    {
                        // Ahlsell have three pricelists (2, 14 and 15) check them all
                        sysWholeSellerId = 14;
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 14)
                    {
                        sysWholeSellerId = 15;
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 7)
                    {
                        // Storel have three pricelists (7, 18 and 19) check them all
                        sysWholeSellerId = 18;
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 18)
                    {
                        sysWholeSellerId = 19;
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 20)
                    {
                        // Lunda have three pricelists (20, 21 and 22) check them all
                        sysWholeSellerId = 21;
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 21)
                    {
                        sysWholeSellerId = 22;
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 37)
                    {
                        sysWholeSellerId = 41; //Onninen PL
                        tryAgain = true;
                    }
                    else if (sysWholeSellerId == 34)
                    {
                        sysWholeSellerId = 40; //Ahlsell PL
                        tryAgain = true;
                    }

                    #endregion

                    if (tryAgain)
                        return GetExternalInvoiceProductByProductNr(entities, productNumber, actorCompanyId, ref sysWholeSellerId, tryAll: tryAll, sysWholesellers: sysWholesellers);

                    if (sysWholesellers == null && tryAll)
                    {
                        sysWholesellers = WholeSellerManager.GetSysWholesellersByCompany(entities, actorCompanyId);
                    }

                    if (tryAll && sysWholesellers != null && sysWholesellers.Any())
                    {
                        //TODO Use priorityList (when we have that!)                        
                        foreach (var wholeseller in sysWholesellers)
                        {
                            companyPriceList = GetCompanyWholeSellerPriceLists(entities, actorCompanyId, wholeseller.SysWholesellerId);
                            result = SysPriceListManager.SearchProductPrice(productNumber, companyPriceList);
                            //result = entities.ExternalProductSearchByNumber(actorCompanyId, productNumber, wholeseller.SysWholesellerId).FirstOrDefault();

                            if (result != null)
                                break;
                        }
                    }
                }

                //Is recursive
                if (result != null)
                    return result;
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }

            return null;
        }

        #endregion

        #region InvoiceProductSearchView

        public List<ExternalProductSmallDTO> AzureSearchSysProducts(CompEntities entities, int actorCompanyId, string number, string name, string groupIdentifier, string text, int fetchSize)
        {
            var dtos = new List<ExternalProductSmallDTO>();
            var sysServiceManager = new SysServiceManager(parameterObject);
            int sysCountryId = GetCompanySysCountryIdFromCache(entities, actorCompanyId);

            var sysPriceListHeads = WholeSellerManager.GetCompanySysWholesellerPriceLists(entities, actorCompanyId);
            var sysPriceListHeadIds = sysPriceListHeads.Select(w => w.SysPriceListHeadId).ToList();

            var externalTypesToSearch = new List<ExternalProductType>();

            if (sysPriceListHeads.Any())
            {
                foreach (var item in sysPriceListHeads)
                {
                    var externalType = SysPriceListManager.GetExternalProductType((SoeSysPriceListProvider)item.Provider, item.SysWholesellerId);
                    if (!externalTypesToSearch.Contains(externalType))
                        externalTypesToSearch.Add(externalType);
                }

                foreach (var type in externalTypesToSearch)
                {
                    //new List<int> {0}) = to trigger correct search in sys service...no filtering is done there...
                    dtos.AddRange(sysServiceManager.SysProductAzureSearch((TermGroup_Country)sysCountryId, type, fetchSize, number, name, groupIdentifier, text, new List<int> { 0 }));
                }

                //Only return items which are on pricelist which we use....
                dtos = dtos.Where(x => sysPriceListHeadIds.Intersect(x.SysPriceListSmallDTOs.Select(p => p.SysPriceListHeadId)).Any()).ToList();
                //to be actived next sprint instead of the row above
                //dtos = dtos.Where(x => sysPriceListHeadIds.Intersect(x.SysPriceListSmallDTOs.Select(p => p.SysPriceListHeadId)).Any()).ToList();
            }

            //do this check just for Finnish products since there are products having wrong type which are not welcome to search result
            if ((TermGroup_Country)sysCountryId == TermGroup_Country.FI)
            {
                var dtosFixed = new List<ExternalProductSmallDTO>();
                List<CompanyWholesellerPricelist> companyPricelists = WholeSellerManager.GetAllCompanyWholesellerPriceLists(entities, actorCompanyId);

                for (var i = 0; i < dtos.Count; i++)
                {
                    var dto = dtos[i];

                    List<int> sysWholeSellersIds = dto.SysPriceListSmallDTOs.Select(s => s.SysWholesellerId).Distinct().ToList();

                    for (var x = 0; x < sysWholeSellersIds.Count; x++)
                    {
                        var id = sysWholeSellersIds[x];
                        var sysWholeseller = SysPriceListManager.GetSysWholesellerFromCache(id);
                        if (sysWholeseller != null && sysWholeseller.Type != dto.Type)
                        {
                            sysWholeseller = null;
                        }

                        var compPriceList = sysWholeseller != null ? companyPricelists.FirstOrDefault(y => y.SysWholesellerId == sysWholeseller.SysWholesellerId) : null;
                        var priceList = compPriceList != null ? dto.SysPriceListSmallDTOs.FirstOrDefault(y => y.SysPriceListHeadId == compPriceList.SysPriceListHeadId) : null;

                        if (priceList != null)
                        {
                            dtosFixed.Add(dto);
                            x = sysWholeSellersIds.Count;
                        }
                    }

                }

                dtos = dtosFixed;
            }

            var netpriceSysWholeSellerIds = WholsellerNetPriceManager.GetWholsellerNetPrices(entities, actorCompanyId, WholsellerNetPriceManager.WholesellersWithCompleteNetPriceList()).Select(x => x.SysWholeSellerId).Distinct();
            if (netpriceSysWholeSellerIds.Any())
            {
                externalTypesToSearch.Clear();
                foreach (var sysWholeSellerId in netpriceSysWholeSellerIds)
                {
                    var wholeseller = SysPriceListManager.GetSysWholesellerFromCache(sysWholeSellerId);

                    var externalType = SysPriceListManager.GetExternalProductType((SoeSysPriceListProviderType)wholeseller.Type);
                    if (!externalTypesToSearch.Contains(externalType))
                        externalTypesToSearch.Add(externalType);
                }

                var existingExternalids = dtos.Select(x => x.ExternalProductId).Distinct().ToList();
                foreach (var type in externalTypesToSearch)
                {
                    var searchResult = sysServiceManager.SysProductAzureSearch((TermGroup_Country)sysCountryId, type, fetchSize, number, name, groupIdentifier, text, new List<int> { 0 });
                    var itemsToAdd = searchResult.Where(x => !existingExternalids.Contains(x.ExternalProductId));

                    //Only return items which has prices
                    if (itemsToAdd.Any())
                    {
                        var hasProductPrices = WholsellerNetPriceManager.HasNetPrices(entities, actorCompanyId, itemsToAdd.Select(x => x.ExternalProductId).ToList());
                        itemsToAdd = itemsToAdd.Where(x => hasProductPrices.Contains(x.ExternalProductId)).ToList();
                        dtos.AddRange(itemsToAdd);
                    }
                }
            }

            return dtos;
        }

        private List<InvoiceProductSearchViewDTO> AzureSearchInvoiceProducts(CompEntities entities, int actorCompanyId, string number, string name, string groupIdentifier = "", string text = "", int fetchSize = 100)
        {
            List<InvoiceProductSearchViewDTO> productPriceItems = new List<InvoiceProductSearchViewDTO>();
            var externalProductSmallDTOs = AzureSearchSysProducts(entities, actorCompanyId, number, name, groupIdentifier, text, fetchSize);

            foreach (var externalProduct in externalProductSmallDTOs)
            {
                var dto = new InvoiceProductSearchViewDTO()
                {
                    Name = externalProduct.Name,
                    Number = externalProduct.ProductId,
                    ProductIds = new List<int>() { externalProduct.ExternalProductId },
                    PriceListOrigin = (PriceListOrigin)(Convert.ToInt32(externalProduct.PriceListOrigin)),
                    Type = (SoeSysPriceListProviderType)externalProduct.Type,
                    ExtendedInfo = externalProduct.ExtendedInfo,
                    ExternalId = externalProduct.ExternalId,
                    Manufacturer = externalProduct.Manufacturer,
                    ImageUrl = externalProduct.ImageUrl,
                    EndAt = externalProduct.EndAt,
                };

                if (dto.EndAt.HasValue)
                {
                    if (dto.EndAt.Value.Date < DateTime.Now.Date)
                    {
                        dto.EndAtTooltip = GetText(4209, TermGroup.AngularCommon) + " " + dto.EndAt.Value.ToShortDateString();
                        dto.EndAtIcon = "fal fa-ban errorColor";
                    }
                    else
                    {
                        dto.EndAtTooltip = GetText(4210, TermGroup.AngularCommon) + " " + dto.EndAt.ToShortDateString();
                        dto.EndAtIcon = "fal fa-triangle-exclamation warningColor";
                    }
                }

                productPriceItems.Add(dto);
            }

            return productPriceItems;
        }

        public List<InvoiceProductSearchViewDTO> SearchProducts(int actorCompanyId, InvoiceProductSearchDTO searchDTO, int maxNrRows = 100)
        {
            var searchResult = new List<InvoiceProductSearchViewDTO>();
            using (var entities = new CompEntities())
            {
                if (searchDTO.External)
                {
                    searchResult.AddRange(AzureSearchInvoiceProducts(entities, actorCompanyId, searchDTO.Number, searchDTO.Name, string.Empty, string.Empty, maxNrRows).Select(p => new InvoiceProductSearchViewDTO { ProductIds = p.ProductIds, Name = p.Name, Number = p.Number, PriceListOrigin = p.PriceListOrigin, Type = p.Type }).ToList());
                    searchResult.AddRange(entities.PriceListImportedSearch(actorCompanyId, searchDTO.Number, searchDTO.Name, null, maxNrRows).Select(p => new InvoiceProductSearchViewDTO { ProductIds = new List<int> { p.ProductId }, Name = p.Name, Number = p.Number, PriceListOrigin = (PriceListOrigin)p.PriceListOrigin, Type = (SoeSysPriceListProviderType)p.Type }).ToList());
                }
                else
                {
                    searchResult.AddRange(SearchInvoiceProducts(entities, actorCompanyId, searchDTO, maxNrRows, 0).Select(p => new InvoiceProductSearchViewDTO { ProductIds = new List<int> { p.ProductId }, Name = p.ProductName, Number = p.ProductNr }));
                }
            }

            return searchResult.Take(maxNrRows).ToList();
        }

        public List<InvoiceProductSearchViewDTO> SearchInvoiceProducts(int actorCompanyId, string search, int fetchSize)
        {
            var searchAsNumber = StringUtility.GetLong(search, 0);

            // Search by text
            var result = SearchInvoiceProducts(actorCompanyId, null, null, String.Empty, search, fetchSize);

            /*
            //Search by number
            var result = SearchInvoiceProducts(actorCompanyId, search, null, String.Empty, String.Empty, fetchSize);
            if ((result.Any() && searchAsNumber > 0) || result.Count >= fetchSize)
                return result;

            //Search by name
            var resultSearchByName = SearchInvoiceProducts(actorCompanyId, null, search, String.Empty, String.Empty, fetchSize);
            result.AddRange(resultSearchByName.Take(fetchSize - result.Count));
            */

            //search by EAN
            if (searchAsNumber > 0 && result.Count < fetchSize)
            {
                /*
                var resultSearchByEan = CompEntities.SysProductSearch(actorCompanyId, string.Empty, string.Empty, searchAsNumber).Take(fetchSize).Select(p => new InvoiceProductSearchViewDTO
                {
                    Name = p.Name,
                    PriceListOrigin = (PriceListOrigin)p.PriceListOrigin,
                    Type = (SoeSysPriceListProviderType)p.Type,
                    Number = p.Number,
                    ProductIds = new List<int> { p.ProductId }
                }).ToList();
                */

                //SysPriceLists...
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var companyPriceLists = GetCompanyWholeSellerPriceLists(entitiesReadOnly, actorCompanyId, null);
                var resultSearchByEan = SysPriceListManager.SearchProductPriceByEAN(searchAsNumber, companyPriceLists).Select(p => new InvoiceProductSearchViewDTO
                {
                    Name = p.Name,
                    PriceListOrigin = (PriceListOrigin)p.PriceListOrigin,
                    Type = (SoeSysPriceListProviderType)p.Type,
                    Number = p.Number,
                    ProductIds = new List<int> { p.ProductId }
                }).ToList();

                //Own imported pricelists...
                if (!resultSearchByEan.Any())
                {
                    resultSearchByEan = entitiesReadOnly.PriceListImportedSearch(actorCompanyId, null, null, searchAsNumber, fetchSize).Select(p => new InvoiceProductSearchViewDTO
                    {
                        Name = p.Name,
                        PriceListOrigin = (PriceListOrigin)p.PriceListOrigin,
                        Type = (SoeSysPriceListProviderType)p.Type,
                        Number = p.Number,
                        ProductIds = new List<int> { p.ProductId }
                    }).ToList();
                }
                result.AddRange(resultSearchByEan.Take(fetchSize - result.Count));
            }

            return result;
        }

        public List<InvoiceProductSearchViewDTO> SearchInvoiceProducts(int actorCompanyId, string number, string name, string groupIdentifier = "", string text = "", int fetchSize = 100)
        {
            var products = new List<InvoiceProductSearchViewDTO>();
            var searchResult = new List<InvoiceProductSearchViewDTO>();

            using (var entities = new CompEntities())
            {
                int sysCountryId = GetCompanySysCountryIdFromCache(entities, actorCompanyId);

                var hasCompanySpecificPriceLists = entities.PriceListImportedHead.Any(x => x.ActorCompanyId == actorCompanyId);
                //var hasImportedNetPrices = entities.WholsellerNetPrice.Any(x => x.ActorCompanyId == actorCompanyId && x.SysWholeSellerId == 20);

                searchResult.AddRange(AzureSearchInvoiceProducts(entities, actorCompanyId, number, name, groupIdentifier, text, fetchSize).Select(p => new InvoiceProductSearchViewDTO { ProductIds = p.ProductIds, Name = p.Name, Number = p.Number, PriceListOrigin = p.PriceListOrigin, Type = p.Type, ExtendedInfo = p.ExtendedInfo, Manufacturer = p.Manufacturer, ExternalId = p.ExternalId, ImageUrl = p.ImageUrl, EndAt = p.EndAt, EndAtTooltip = p.EndAtTooltip, EndAtIcon = p.EndAtIcon }).ToList());

                if (hasCompanySpecificPriceLists && string.IsNullOrEmpty(groupIdentifier))
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        searchResult.AddRange(entities.PriceListImportedSearch(actorCompanyId, number, name, null, fetchSize).Select(p => new InvoiceProductSearchViewDTO { ProductIds = new List<int> { p.ProductId }, Name = p.Name, Number = p.Number, PriceListOrigin = (PriceListOrigin)p.PriceListOrigin, Type = (SoeSysPriceListProviderType)p.Type, ExtendedInfo = "", Manufacturer = "", ExternalId = (int?)null, ImageUrl = "" }).ToList());
                    }
                    else
                    {
                        searchResult.AddRange(entities.PriceListImportedSearch(actorCompanyId, text, "", null, fetchSize).Select(p => new InvoiceProductSearchViewDTO { ProductIds = new List<int> { p.ProductId }, Name = p.Name, Number = p.Number, PriceListOrigin = (PriceListOrigin)p.PriceListOrigin, Type = (SoeSysPriceListProviderType)p.Type, ExtendedInfo = "", Manufacturer = "", ExternalId = (int?)null, ImageUrl = "" }).ToList());
                        searchResult.AddRange(entities.PriceListImportedSearch(actorCompanyId, "", text, null, fetchSize).Select(p => new InvoiceProductSearchViewDTO { ProductIds = new List<int> { p.ProductId }, Name = p.Name, Number = p.Number, PriceListOrigin = (PriceListOrigin)p.PriceListOrigin, Type = (SoeSysPriceListProviderType)p.Type, ExtendedInfo = "", Manufacturer = "", ExternalId = (int?)null, ImageUrl = "" }).ToList());
                    }
                }
                /*
                if (hasImportedNetPrices)
                {
                    searchResult.AddRange( WholsellerNetPriceManager.SearchProductPrices(entities, searchResult.Select(x=> new InvoiceProductPriceSearchViewDTO
                    {
                        ProductId = x.ProductIds.First(),
                    }).ToList(), actorCompanyId).ToList());
                }
                */
                var keys = new List<string>(searchResult.Count);
                foreach (var item in searchResult)
                {
                    if (keys.Contains(item.Number))
                    {
                        //Try to select the product with a different origin
                        var productsToMerge = products.Where(p => p.Number == item.Number && p.Type == item.Type && (p.PriceListOrigin != item.PriceListOrigin));
                        if (productsToMerge.Any() || productsToMerge.GroupBy(p => p.Type).Any(g => g.Count() == 1))
                        {
                            //Product found, so do not add a new to the list but merge them instead
                            foreach (var product in productsToMerge)
                            {
                                product.PriceListOrigin = PriceListOrigin.SysAndCompDbPriceList;
                                product.ProductIds.Add(item.ProductIds.First());
                            }
                        }
                        else
                        {
                            products.Add(new InvoiceProductSearchViewDTO()
                            {
                                Name = item.Name,
                                Number = item.Number,
                                ProductIds = new List<int>() { item.ProductIds.First() },
                                PriceListOrigin = (PriceListOrigin)(Convert.ToInt32(item.PriceListOrigin)),
                                Type = item.Type,
                            });
                        }
                    }
                    else
                    {
                        products.Add(item);

                        keys.Add(item.Number);
                    }
                }

                foreach (var product in products)
                {
                    product.ExternalUrl = GetProductExternalUrl((TermGroup_Country)sysCountryId, (ExternalProductType)product.Type, product.Number, product.Name);
                }
            }

            if (!string.IsNullOrEmpty(number) && string.IsNullOrEmpty(name))
            {
                products = products.OrderBy(p => p.Number).ToList();
            }
            else if (string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(name))
            {
                products = products.OrderBy(p => p.Name).ToList();
            }
            else
            {
                products = products.OrderBy(p => p.Number).ThenBy(p2 => p2.Name).ToList();
            }

            return products.Take(fetchSize).ToList();
        }

        #endregion

        #region InvoiceProductPriceSearchView

        public List<InvoiceProductPriceSearchViewDTO> SearchInvoiceProductPricesByNumber(int actorCompanyId, int priceListTypeId, int customerId, List<string> productNumbers, bool applyPriceRule, SoeSysPriceListProviderType providerType)
        {
            return SearchInvoiceProductPrices(actorCompanyId, priceListTypeId, customerId, productNumbers, applyPriceRule, providerType);
        }

        public List<InvoiceProductPriceSearchViewDTO> SearchInvoiceProductPrices<T>(int actorCompanyId, int priceListTypeId, int customerId, List<T> productNumbers, bool applyPriceRule, SoeSysPriceListProviderType providerType = SoeSysPriceListProviderType.Unknown)
        {
            if (typeof(T) == typeof(string))
            {
                for (int i = 0; i < productNumbers.Count; i++)
                {
                    productNumbers[i] = (T)(object)("\"" + productNumbers[i] + "\"");
                }
            }

            string productNrWhereCondition = productNumbers.Count == 1 ? string.Format("= {0} ", productNumbers.First()) : string.Format("IN {0} ", "{" + StringUtility.GetCommaSeparatedString(productNumbers.ToList(), addWhiteSpace: true) + "}");

            var sysPriceListCondition = $"it.{"ProductNumber"} " + productNrWhereCondition;

            if (providerType != SoeSysPriceListProviderType.Unknown)
            {
                sysPriceListCondition += $" AND it.SysPriceListProviderType = {(int)providerType}";
            }

            List<InvoiceProductPriceSearchViewDTO> productPriceItems = new List<InvoiceProductPriceSearchViewDTO>();
            using (var entities = new CompEntities())
            {
                //sys pricelists
                var companyPriceList = entities.CompanyWholeSellerPriceListUsedView.Where(p => p.ActorCompanyId == actorCompanyId && p.PriceListOrigin == (int)PriceListOrigin.SysDbPriceList).ToDTOs().ToList();
                productPriceItems.AddRange(SysPriceListManager.SearchProductPrices(sysPriceListCondition, companyPriceList));

                //new imported net prices
                string condition = $"it.{"ProductId"} " + productNrWhereCondition;
                if (providerType != SoeSysPriceListProviderType.Unknown)
                {
                    condition += $" AND it.Type = {(int)providerType}";
                }

                var sysproducts = SysPriceListManager.SearchSysProduct(condition);
                if (sysproducts.Any())
                {
                    var netPrices = WholsellerNetPriceManager.SearchProductPrices(entities, sysproducts, true, actorCompanyId);
                    if (netPrices.Any())
                    {
                        productPriceItems.AddRange(netPrices);
                    }
                }

                //Old imported net prices
                condition = $"it.ActorCompanyId = {actorCompanyId} AND " + sysPriceListCondition;
                var priceSearchListsImportedResult = entities.PriceListImportedPriceSearchView.Where(condition).ToDTOs();
                foreach (var productItem in priceSearchListsImportedResult)
                {
                    productItem.Wholeseller = WholeSellerManager.GetWholesellerName(productItem.SysWholesellerId);
                }

                productPriceItems.AddRange(priceSearchListsImportedResult);

                // Apply pricerules
                if (applyPriceRule)
                    productPriceItems = ApplyPriceRule(entities, productPriceItems, priceListTypeId, customerId, actorCompanyId);
            }

            //Calculate MarginalIncome, MarginalIncomeRatio
            foreach (var productPriceItem in productPriceItems)
            {
                productPriceItem.MarginalIncome = (productPriceItem.CustomerPrice.HasValue && productPriceItem.NettoNettoPrice.HasValue) ? (productPriceItem.CustomerPrice.Value - productPriceItem.NettoNettoPrice.Value) : 0;
                productPriceItem.MarginalIncomeRatio = ((productPriceItem.CustomerPrice.HasValue && productPriceItem.CustomerPrice.Value != 0) ? Decimal.Round(productPriceItem.MarginalIncome / productPriceItem.CustomerPrice.Value, 2) : 1) * 100;
            }

            return productPriceItems;
        }

        public InvoiceProductPriceSearchViewDTO SearchInvoiceProductPrice(int actorCompanyId, int priceListTypeId, int customerId, int currencyId, int productId, int sysWholeSellerId, bool applyPriceRule)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return SearchInvoiceProductPrice(entities, actorCompanyId, priceListTypeId, customerId, currencyId, productId, sysWholeSellerId, applyPriceRule);
        }

        public InvoiceProductPriceSearchViewDTO SearchInvoiceProductPrice(CompEntities entities, int actorCompanyId, int priceListTypeId, int customerId, int currencyId, int productId, int sysWholeSellerId, bool applyPriceRule)
        {
            var companyPriceList = GetCompanyWholeSellerPriceLists(entities, actorCompanyId, sysWholeSellerId);

            string sysCondition = $"it.ProductId = {productId} AND it.SysWholeSellerId = {sysWholeSellerId}";
            InvoiceProductPriceSearchViewDTO productPrice = SysPriceListManager.SearchProductPrices(sysCondition, companyPriceList).FirstOrDefault();
            if (productPrice == null)
            {
                string condition = $"it.ActorCompanyId = {actorCompanyId} AND " + sysCondition;
                productPrice = entities.PriceListImportedPriceSearchView.Where(condition).FirstOrDefault()?.ToDTO();
                if (productPrice != null)
                {
                    productPrice.Wholeseller = WholeSellerManager.GetWholesellerName(productPrice.SysWholesellerId);
                }
            }

            if (productPrice != null && applyPriceRule)
            {
                // Apply pricerules
                // Only expect one result since one price is passed as param
                productPrice = ApplyPriceRule(new List<InvoiceProductPriceSearchViewDTO>() { productPrice }, priceListTypeId, customerId, actorCompanyId).FirstOrDefault();
            }

            return productPrice;
        }

        private List<InvoiceProductPriceSearchViewDTO> ApplyPriceRule(List<InvoiceProductPriceSearchViewDTO> productPriceItems, int priceListTypeId, int customerId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return ApplyPriceRule(entities, productPriceItems, priceListTypeId, customerId, actorCompanyId);
        }

        private List<InvoiceProductPriceSearchViewDTO> ApplyPriceRule(CompEntities entities, List<InvoiceProductPriceSearchViewDTO> productPriceItems, int priceListTypeId, int customerId, int actorCompanyId, bool ignoreWholesellerDiscount = false, decimal ediPurchasePrice = 0, bool useMisc = false)
        {
            int companyPriceListTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultPriceListType, 0, actorCompanyId, 0);

            // Apply price rule on all products
            List<RuleResult> ruleResults = PriceRuleManager.ApplyPriceRules(entities, productPriceItems, companyPriceListTypeId, priceListTypeId, customerId, actorCompanyId, ignoreWholesellerDiscount, ediPurchasePrice, useMisc);
            foreach (RuleResult ruleResult in ruleResults)
            {
                foreach (var productPriceItem in productPriceItems)
                {
                    if (
                            (productPriceItem.ProductId != ruleResult.SysProductId) ||
                            (productPriceItem.CompanyWholesellerPriceListId != ruleResult.CompanyWholesellerPriceListId) ||
                            (productPriceItem.SysWholesellerId != ruleResult.SysWholesellerId)
                        )
                        continue;

                    productPriceItem.CustomerPrice = NumberUtility.GetFormattedDecimalValue(ruleResult.Value, 2);
                    productPriceItem.NettoNettoPrice = NumberUtility.GetFormattedDecimalValue(ruleResult.NetPrice, 2);
                    productPriceItem.PriceFormula = ruleResult.Formula.NullToEmpty();
                    if (ruleResult.NetPriceFromNetPriceList)
                    {
                        productPriceItem.Code = GetText(7759, "Nettopris");
                    }
                }
            }

            return productPriceItems;
        }

        public InvoiceProductPriceSearchViewDTO GetInvoiceProductPrice(CompEntities entities, int actorCompanyId, int productId, int? companyWholesellerPriceListId = null, int? priceListImportedHeadId = null, int? syswholesellerId = null, int? wholsellerNetPriceId = null)
        {
            return GetInvoiceProductPrices(entities, actorCompanyId, productId, companyWholesellerPriceListId, priceListImportedHeadId, syswholesellerId, wholsellerNetPriceId: wholsellerNetPriceId).FirstOrDefault();
        }

        public List<InvoiceProductPriceSearchViewDTO> GetInvoiceProductPrices(CompEntities entities, int actorCompanyId, int productId, int? companyWholesellerPriceListId = null, int? priceListImportedHeadId = null, int? syswholesellerId = null, int? sysPriceListHeadId = null, int? wholsellerNetPriceId = null)
        {
            var result = new List<InvoiceProductPriceSearchViewDTO>();

            var noSpecificPricelistChoosen = !priceListImportedHeadId.HasValue && !companyWholesellerPriceListId.HasValue && !sysPriceListHeadId.HasValue && !wholsellerNetPriceId.HasValue;

            if (companyWholesellerPriceListId.GetValueOrDefault() > 0 || sysPriceListHeadId.HasValue || noSpecificPricelistChoosen)
            {
                var companyPriceList = entities.CompanyWholeSellerPriceListUsedView.Where(p => p.ActorCompanyId == actorCompanyId && p.PriceListOrigin == (int)PriceListOrigin.SysDbPriceList);
                if (companyWholesellerPriceListId.HasValue)
                {
                    companyPriceList = companyPriceList.Where(p => p.CompanyWholesellerPriceListId == companyWholesellerPriceListId.Value);
                }
                else if (sysPriceListHeadId.HasValue)
                {
                    companyPriceList = companyPriceList.Where(p => p.SysPriceListHeadId == sysPriceListHeadId.Value);
                }

                string sysCondition = $"it.ProductId = {productId}";
                if (syswholesellerId.HasValue)
                    sysCondition += $" AND it.SysWholeSellerId = {syswholesellerId}";
                result = SysPriceListManager.SearchProductPrices(sysCondition, companyPriceList.ToDTOs().ToList());
            }

            if (wholsellerNetPriceId.GetValueOrDefault() > 0)
            {
                var netPrice = WholsellerNetPriceManager.SearchProductPrices(entities, productId, wholsellerNetPriceId.Value, true, actorCompanyId);
                if (netPrice != null)
                {
                    result.AddRange(netPrice);
                }
            }

            if (priceListImportedHeadId.GetValueOrDefault() > 0 || (noSpecificPricelistChoosen && !result.Any()))
            {
                var importedPriceList = entities.PriceListImportedPriceSearchView.Where(p => p.ProductId == productId && p.ActorCompanyId == actorCompanyId);
                if (priceListImportedHeadId.HasValue)
                {
                    importedPriceList = importedPriceList.Where(p => p.PriceListImportedHeadId == priceListImportedHeadId.Value);
                }
                result = importedPriceList.ToDTOs().ToList();
                foreach (var item in result)
                {
                    item.Wholeseller = WholeSellerManager.GetWholesellerName(item.SysWholesellerId);
                }
            }

            return result;
        }

        #endregion

        #region InvoiceProductPriceResult

        public InvoiceProductPriceResult GetExternalProductPrice(CompEntities entities, int priceListTypeId, InvoiceProduct product, int customerId, int actorCompanyId, int wholeSellerId, bool copyProduct, string wholeSellerName = null, bool checkProduct = false, bool ignoreWholesellerDiscount = false, decimal ediPurchasePrice = 0, bool usesMisc = false, bool overrideCompDbCheck = false)
        {
            InvoiceProductPriceResult productPriceResult = new InvoiceProductPriceResult();
            List<CompanyWholesellerPricelist> companyWholesellerPricelists = null;

            #region Prereq

            var externalPriceLists = new List<InvoiceProductExternalDTO>();
            int companyWholesellerPricelistId = 0;

            // Try to get wholeseller id from wholesellername if specifed
            if (wholeSellerId == 0 && !string.IsNullOrEmpty(wholeSellerName))
            {
                WholeSellerManager.TryGetSysWholesellerIdByName(wholeSellerName, ref wholeSellerId);
            }

            var wholsellerWithoutPriceList = ((ediPurchasePrice != 0) && (wholeSellerId == 65)); //Comfort

            if (wholeSellerId > 0 && (product.PriceListOrigin != (int)PriceListOrigin.CompDbPriceList || overrideCompDbCheck))
            {
                //ugly fix for handling Ahlsell El & Ahlsell VVS
                if (wholeSellerId == 2 || wholeSellerId == 15)
                {
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where ((cpl.SysWholesellerId == 2) || (cpl.SysWholesellerId == 14) || (cpl.SysWholesellerId == 15)) &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }
                //ugly fix for handling Solar El & Solar VVS
                else if (wholeSellerId == 5)
                {
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where ((cpl.SysWholesellerId == 5) || (cpl.SysWholesellerId == 9)) &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }
                else
                {
                    //If wholesellerid is supplied then use the pricelisthead that is connected the wholeseller
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where cpl.SysWholesellerId == wholeSellerId &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }

                //ugly fix for handling Onninen FI VVS
                if (!companyWholesellerPricelists.Any() && wholeSellerId == 37)
                {
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where (cpl.SysWholesellerId == 41) &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }

                //ugly fix for handling Ahlsell FI VVS
                if (!companyWholesellerPricelists.Any() && wholeSellerId == 34)
                {
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where (cpl.SysWholesellerId == 40) &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }

                //ugly fix for Storel => has become Rexel....
                if (!companyWholesellerPricelists.Any() && wholeSellerId == 7)
                {
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where (cpl.SysWholesellerId == 3) &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }

                if (!companyWholesellerPricelists.IsNullOrEmpty())
                {
                    var companyWholesellerPricelistItem = companyWholesellerPricelists.First();
                    companyWholesellerPricelistId = companyWholesellerPricelistItem.CompanyWholesellerPriceListId;

                    if (companyWholesellerPricelistItem.SysPriceListHeadId == 0)
                    {
                        //if no pricelistheadid was found for the given wholeseller use the pricelistheadid that is connected to the product
                        externalPriceLists.Clear();
                        if (product.ExternalPriceListHeadId.HasValue)
                        {
                            externalPriceLists.Add(new InvoiceProductExternalDTO(product.ExternalPriceListHeadId.Value, product.ExternalProductId.GetValueOrDefault(), product.PriceListOrigin));
                        }
                    }
                    else
                    {
                        externalPriceLists.AddRange(companyWholesellerPricelists.Select(x => new InvoiceProductExternalDTO(x.SysPriceListHeadId, product.ExternalProductId.GetValueOrDefault(), product.PriceListOrigin)).ToList());
                    }
                }
            }
            else if (product.ExternalPriceListHeadId.HasValue && (checkProduct || product.PriceListOrigin == (int)PriceListOrigin.CompDbPriceList))
            {
                externalPriceLists.Add(new InvoiceProductExternalDTO(product.ExternalPriceListHeadId.GetValueOrDefault(), product.ExternalProductId.GetValueOrDefault(), product.PriceListOrigin));
            }

            if (!externalPriceLists.Any() && WholeSellerManager.CanHaveImportedPriceList(wholeSellerId))
            {
                var importedPriceList = GetPriceListImportedExternal(entities, actorCompanyId, wholeSellerId, product.Number, product.ExternalProductId.GetValueOrDefault(), priceListTypeId);
                if (importedPriceList.PriceListOrigin != (int)PriceListOrigin.Unknown)
                {
                    externalPriceLists.Add(importedPriceList);
                }
            }

            if (!externalPriceLists.Any() && !wholsellerWithoutPriceList)
                return new InvoiceProductPriceResult(ActionResultSelect.PriceNotFound);

            #endregion

            // Get prices from view
            List<InvoiceProductPriceSearchViewDTO> productPriceItems = null;
            int usedSysPriceListHeadId = 0;
            foreach (var priceList in externalPriceLists)
            {
                if (priceList.PriceListOrigin == (int)PriceListOrigin.CompDbPriceList)
                {
                    productPriceItems = GetInvoiceProductPrices(entities, actorCompanyId, priceList.ExternalProductId, null, priceList.ExternalPriceListId);
                    usedSysPriceListHeadId = priceList.ExternalPriceListId;
                }
                else if (priceList.PriceListOrigin == (int)PriceListOrigin.CompDbNetPriceList)
                {
                    productPriceItems = GetInvoiceProductPrices(entities, actorCompanyId, priceList.ExternalProductId, null, null, null, null, priceList.ExternalPriceListId);
                }
                else
                {
                    productPriceItems = GetInvoiceProductPrices(entities, actorCompanyId, priceList.ExternalProductId, null, null, null, priceList.ExternalPriceListId);
                    usedSysPriceListHeadId = priceList.ExternalPriceListId;
                }

                if ((productPriceItems != null) && (productPriceItems.Any()))
                {
                    break;
                }
            }

            if (productPriceItems == null)
                productPriceItems = new List<InvoiceProductPriceSearchViewDTO>();

            if (!productPriceItems.Any())
            {
                if (usesMisc || wholsellerWithoutPriceList)
                {
                    var miscView = new InvoiceProductPriceSearchViewDTO
                    {
                        GNP = ediPurchasePrice,
                        Name = product.Name,
                        Number = product.Number,
                        ProductId = (wholsellerWithoutPriceList && product.ExternalProductId.GetValueOrDefault() > 0 ? product.ExternalProductId.Value : product.ProductId),
                        SysPriceListHeadId = usedSysPriceListHeadId,
                        SysWholesellerId = wholeSellerId,
                        CompanyWholesellerPriceListId = companyWholesellerPricelistId,
                    };

                    productPriceItems.Add(miscView);
                }
                else
                {
                    var wholesellerName = WholeSellerManager.GetWholesellerName(wholeSellerId);
                    return new InvoiceProductPriceResult(ActionResultSelect.PriceNotFound) { SysWholesellerName = wholesellerName, ErrorMessage = string.Format(GetText(7423, "Pris kunde inte hittas för vald grossist ({0})"), wholesellerName) };
                }
            }

            // Apply pricerules
            productPriceItems = ApplyPriceRule(entities, productPriceItems, priceListTypeId, customerId, actorCompanyId, ignoreWholesellerDiscount, ediPurchasePrice, usesMisc);

            var productPrice = productPriceItems.FirstOrDefault();
            if (productPrice != null)
            {
                productPriceResult.PurchasePrice = NumberUtility.GetFormattedDecimalValue(productPrice.NettoNettoPrice ?? 0, 2);
                productPriceResult.SalesPrice = NumberUtility.GetFormattedDecimalValue(productPrice.CustomerPrice ?? 0, 2);
                productPriceResult.SysWholesellerName = productPrice.Wholeseller;
                productPriceResult.ProductUnit = productPrice.SalesUnit ?? productPrice.PurchaseUnit;
            }

            // Update prices on imported product
            if (copyProduct)
            {
                CopyExternalInvoiceProduct(entities, product.ExternalProductId.Value, productPriceResult.PurchasePrice, productPriceResult.SalesPrice, productPriceResult.ProductUnit, priceListTypeId, usedSysPriceListHeadId, productPriceResult.SysWholesellerName, customerId, actorCompanyId, true, (PriceListOrigin)product.PriceListOrigin, out decimal outPurchasePrice, out decimal outSalesPrice);
                if (outPurchasePrice != 0 && outSalesPrice != 0 && outSalesPrice != productPriceResult.SalesPrice)
                {
                    //We have converted prices based on different units...
                    productPriceResult.PurchasePrice = outPurchasePrice;
                    productPriceResult.SalesPrice = outSalesPrice;
                    productPriceResult.ProductUnit = product.ProductUnit != null ? product.ProductUnit.Code : productPriceResult.ProductUnit;
                }
            }

            return productPriceResult;
        }

        /*
        public InvoiceProductPriceResult GetExternalProductPrice(CompEntities entities, int priceListTypeId, InvoiceProduct product, int customerId, int currencyId, int actorCompanyId, int wholeSellerId, bool returnFormula, bool copyProduct, string wholeSellerName = null, bool checkProduct = false, bool ignoreWholesellerDiscount = false, decimal ediPurchasePrice = 0, bool usesMisc = false, bool overrideCompDbCheck = false)
        {
            InvoiceProductPriceResult productPriceResult = new InvoiceProductPriceResult();
            List<CompanyWholesellerPricelist> companyWholesellerPricelists = null;

            #region Prereq

            var sysPriceListHeadIds = new List<int>();
            int companyWholesellerPricelistId = 0;

            // Try to get wholeseller id from wholesellername if specifed
            if (wholeSellerId == 0 && !string.IsNullOrEmpty(wholeSellerName))
            {
                WholeSellerManager.TryGetSysWholesellerIdByName(wholeSellerName, ref wholeSellerId);
            }

            if (wholeSellerId > 0 && (product.PriceListOrigin != (int)PriceListOrigin.CompDbPriceList || overrideCompDbCheck))
            {
                //If wholesellerid is supplied then use the pricelisthead that is connected the wholeseller
                companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                where cpl.SysWholesellerId == wholeSellerId &&
                                                cpl.Company.ActorCompanyId == actorCompanyId
                                                select cpl).ToList();

                //ugly fix for handling Ahlsell El & Ahlsell VVS
                if ((companyWholesellerPricelists == null || !companyWholesellerPricelists.Any()) && wholeSellerId == 2)
                {
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where ((cpl.SysWholesellerId == 14) || (cpl.SysWholesellerId == 15)) &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }

                //ugly fix for Storel => has become Rexel....
                if ((companyWholesellerPricelists == null || !companyWholesellerPricelists.Any()) && wholeSellerId == 7)
                {
                    companyWholesellerPricelists = (from cpl in entities.CompanyWholesellerPricelist
                                                    where (cpl.SysWholesellerId == 3) &&
                                                    cpl.Company.ActorCompanyId == actorCompanyId
                                                    select cpl).ToList();
                }

                if ((companyWholesellerPricelists == null) || !companyWholesellerPricelists.Any())
                {
                    return new InvoiceProductPriceResult(ActionResultSelect.PriceNotFound);
                }

                var companyWholesellerPricelistItem = companyWholesellerPricelists.First();
                companyWholesellerPricelistId = companyWholesellerPricelistItem.CompanyWholesellerPriceListId;

                if (companyWholesellerPricelistItem.SysPriceListHeadId == 0)
                {
                    //if no pricelistheadid was found for the given wholeseller use the pricelistheadid that is connected to the product
                    sysPriceListHeadIds.Clear();
                    if (product.ExternalPriceListHeadId.HasValue)
                    {
                        //sysPriceListHeadId = product.ExternalPriceListHeadId.Value;
                        sysPriceListHeadIds.Add(product.ExternalPriceListHeadId.Value);
                    }
                }
                else
                {
                    sysPriceListHeadIds.AddRange(companyWholesellerPricelists.Select(x => x.SysPriceListHeadId).ToList());
                }
            }
            else if (product.ExternalPriceListHeadId.HasValue && (checkProduct || product.PriceListOrigin == (int)PriceListOrigin.CompDbPriceList) )
            {
                sysPriceListHeadIds.Add(product.ExternalPriceListHeadId.Value);
            }

            if (!sysPriceListHeadIds.Any())
                return new InvoiceProductPriceResult(ActionResultSelect.PriceNotFound);

            #endregion

            int compDbPriceList = (int)PriceListOrigin.CompDbPriceList;

            // Get prices from view
            List<InvoiceProductPriceSearchViewDTO> productPriceItems = null;
            int usedSysPriceListHeadId = 0;
            foreach (var sysPriceListHeadId in sysPriceListHeadIds)
            {
                if (product.PriceListOrigin == compDbPriceList)
                {
                    productPriceItems = GetInvoiceProductPrices(entities, actorCompanyId, product.ExternalProductId.GetValueOrDefault(), null, sysPriceListHeadId);
                }
                else
                {
                    productPriceItems = GetInvoiceProductPrices(entities, actorCompanyId, product.ExternalProductId.GetValueOrDefault(), null, null, null, sysPriceListHeadId);
                }

                usedSysPriceListHeadId = sysPriceListHeadId;
                if ((productPriceItems != null) && (productPriceItems.Any()))
                {
                    break;
                }
            }

            if (productPriceItems == null)
                productPriceItems = new List<InvoiceProductPriceSearchViewDTO>();
            
            if (!productPriceItems.Any())
            {
                if (usesMisc)
                {
                    var miscView = new InvoiceProductPriceSearchViewDTO
                    {
                        GNP = ediPurchasePrice,
                        Name = product.Name,
                        Number = product.Number,
                        ProductId = product.ProductId,
                        SysPriceListHeadId = usedSysPriceListHeadId,
                        SysWholesellerId = wholeSellerId,
                        CompanyWholesellerPriceListId = companyWholesellerPricelistId,
                    };

                    productPriceItems.Add(miscView);
                }
                else
                {
                    var wholesellerName = WholeSellerManager.GetSysWholesellerName(wholeSellerId);
                    return new InvoiceProductPriceResult(ActionResultSelect.PriceNotFound) { SysWholesellerName = wholesellerName, ErrorMessage = string.Format(GetText(7423, "Pris kunde inte hittas för vald grossist ({0})"), wholesellerName) };
                }
            }

            // Apply pricerules
            productPriceItems = ApplyPriceRule(entities, productPriceItems, priceListTypeId, customerId, actorCompanyId, ignoreWholesellerDiscount, ediPurchasePrice, usesMisc);

            var productPrice = productPriceItems.FirstOrDefault();
            if (productPrice != null)
            {
                productPriceResult.PurchasePrice = NumberUtility.GetFormattedDecimalValue(productPrice.NettoNettoPrice ?? 0, 2);
                productPriceResult.SalesPrice = NumberUtility.GetFormattedDecimalValue(productPrice.CustomerPrice.Value, 2);
                productPriceResult.SysWholesellerName = productPrice.Wholeseller;
                productPriceResult.ProductUnit = productPrice.SalesUnit ?? productPrice.PurchaseUnit;
            }

            // Update prices on imported product
            if (copyProduct)
            {
                CopyExternalInvoiceProduct(entities, product.ExternalProductId.Value, productPriceResult.PurchasePrice, productPriceResult.SalesPrice, productPriceResult.ProductUnit, priceListTypeId, usedSysPriceListHeadId, productPriceResult.SysWholesellerName, customerId, actorCompanyId, true, (PriceListOrigin)product.PriceListOrigin, out decimal outPurchasePrice, out decimal outSalesPrice);
                if (outPurchasePrice != 0 && outSalesPrice != 0 && outSalesPrice != productPriceResult.SalesPrice)
                {
                    //We have converted prices based on different units...
                    productPriceResult.PurchasePrice = outPurchasePrice;
                    productPriceResult.SalesPrice = outSalesPrice;
                    productPriceResult.ProductUnit = product.ProductUnit != null ? product.ProductUnit.Code : productPriceResult.ProductUnit;
                }
            }

            return productPriceResult;
        }
        */
        #endregion

        #region InvoiceProduct

        public Dictionary<int, string> GetProductsSmallDict(int actorCompanyId, bool active)
        {
            var dict = new Dictionary<int, string>();

            var deliveryConditions = GetProductsSmall(actorCompanyId, active);
            foreach (var dc in deliveryConditions)
                dict.Add(dc.ProductId, dc.NumberName);

            return dict;
        }

        public List<ProductSmallDTO> GetProductsSmall(int actorCompanyId, bool active)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetProductsSmall(entities, actorCompanyId, active);
        }

        public List<ProductSmallDTO> GetProductsSmall(CompEntities entities, int actorCompanyId, bool active)
        {

            IQueryable<InvoiceProduct> query = (from p in entities.Product.OfType<InvoiceProduct>()
                                                where
                                                    p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                                select p).OfType<InvoiceProduct>();

            if (active)
                query = query.Where(p => p.State == (int)SoeEntityState.Active);
            else
            {
                query = query.Where(p => p.State == (int)SoeEntityState.Active || p.State == (int)SoeEntityState.Inactive);
            }

            return query.Select(p => new ProductSmallDTO
            {
                Name = p.Name,
                Number = p.Number,
                ProductId = p.ProductId,
            }).AsEnumerable().OrderBy(p => p.NumberSort).ToList();
        }

        public List<InvoiceProduct> GetInvoiceProducts(int actorCompanyId, bool? active, bool loadProductUnitAndGroup = false, bool loadAccounts = false, bool loadCategories = false, int langId = 0, bool loadTimeCode = false, int? productId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetInvoiceProducts(entities, actorCompanyId, active, loadProductUnitAndGroup, loadAccounts, loadCategories, langId, loadTimeCode, productId);
        }

        public List<InvoiceProduct> GetInvoiceProducts(CompEntities entities, int actorCompanyId, bool? active, bool loadProductUnitAndGroup = false, bool loadAccounts = false, bool loadCategories = false, int langId = 0, bool loadTimeCode = false, int? productId = null)
        {
            List<CompanyCategoryRecord> categoryRecordsForCompany = null;
            IQueryable<InvoiceProduct> query = entities.Product.OfType<InvoiceProduct>();
            if (loadProductUnitAndGroup)
            {
                query = query.Include("ProductUnit");
                query = query.Include("ProductGroup");
            }
            if (loadAccounts)
            {
                query = query.Include("ProductAccountStd.AccountStd");
                query = query.Include("ProductAccountStd.AccountInternal.Account.AccountDim");
            }
            if (loadTimeCode)
            {
                query = query.Include("TimeCode");
            }
            if (loadCategories)
            {
                categoryRecordsForCompany = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Product, actorCompanyId);
            }

            var productQuery = (from p in query
                                where p.State != (int)SoeEntityState.Deleted &&
                                p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                select p).OfType<InvoiceProduct>();

            if (active == true)
                productQuery = productQuery.Where(p => p.State == (int)SoeEntityState.Active);

            if (productId.HasValue)
                productQuery = productQuery.Where(p => p.ProductId == productId.Value);

            List<InvoiceProduct> products = productQuery.ToList();

            if (categoryRecordsForCompany != null)
            {
                foreach (InvoiceProduct product in products)
                {
                    product.CategoryNames = new List<string>();
                    foreach (CompanyCategoryRecord ccr in categoryRecordsForCompany.GetCategoryRecords(SoeCategoryRecordEntity.Product, product.ProductId, date: null, discardDateIfEmpty: true))
                    {
                        product.CategoryNames.Add(ccr.Category.Name);
                    }

                }
            }

            return products.OrderBy(p => p.NumberSort).ToList();
        }

        public List<ProductDTO> GetInvoiceProductsTiny(int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            entitiesReadOnly.Product.NoTracking();
            return entitiesReadOnly.Product.OfType<InvoiceProduct>()
                .Where(p => p.Company.Any(c => c.ActorCompanyId == actorCompanyId && p.State == 0))
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductUnitId = p.ProductUnitId,
                    ProductUnitCode = p.ProductUnit.Code,
                    Number = p.Number,
                    Name = p.Name,
                })
                .ToList();
        }

        public List<InvoiceProduct> GetInvoiceProducts(int actorCompanyId, List<int> productIds, bool loadProductUnit, bool includeInActive, DateTime? modifiedSince)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Product.NoTracking();
            IQueryable<InvoiceProduct> query = (from p in entitiesReadOnly.Product.OfType<InvoiceProduct>()
                                                where
                                                   p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                                select p);

            if (loadProductUnit)
                query = query.Include("ProductUnit");

            if (productIds != null)
            {
                query = query.Where(p => productIds.Contains(p.ProductId));
            }

            if (includeInActive)
            {
                query = query.Where(p => p.State == (int)SoeEntityState.Active || p.State == (int)SoeEntityState.Inactive);
            }
            else
            {
                query = query.Where(p => p.State == (int)SoeEntityState.Active);
            }

            if (modifiedSince.HasValue)
                query = query.Where(p => p.Modified >= modifiedSince || p.Created >= modifiedSince);

            return query.OrderBy(p => p.Number).ToList();

        }

        public List<InvoiceProduct> GetInvoiceProductsBySysPriceList(CompEntities entities, int actorCompanyId, int sysPriceListHeadId)
        {
            return (from ip in entities.Product.OfType<InvoiceProduct>()
                        .Include("PriceList")
                    where ip.Company.Any(i => i.ActorCompanyId == actorCompanyId) &&
                    ip.ExternalPriceListHeadId == sysPriceListHeadId
                    select ip).ToList();
        }

        public List<InvoiceProductCopyDTO> GetInvoiceProductsForTemplateCopying(CompEntities entities, int actorCompanyId, bool loadAccounts, bool includeExternal)
        {
            IQueryable<InvoiceProduct> query = (from ip in entities.Product.OfType<InvoiceProduct>()
                                                .Include("PriceList")
                                                .Include("ProductUnit")
                                                .Include("ProductGroup")
                                                where ip.Company.Any(i => i.ActorCompanyId == actorCompanyId)
                                                select ip);

            if (loadAccounts)
            {
                query = query.Include("ProductAccountStd.AccountStd.Account")
                            .Include("ProductAccountStd.AccountInternal.Account.AccountDim");
            }

            if (!includeExternal)
                query = query.Where(ip => !ip.ExternalProductId.HasValue);

            return query.Select(p => new InvoiceProductCopyDTO
            {
                ProductId = p.ProductId,
                Number = p.Number,
                Name = p.Name,
                Description = p.Description,
                EAN = p.EAN,
                VatType = p.VatType,
                VatCodeId = p.VatCode.VatCodeId,
                ProductUnitCode = p.ProductUnit.Code,
                ProductUnitName = p.ProductUnit.Name,
                ExternalProductId = p.ExternalProductId,
                ExternalPriceListHeadId = p.ExternalPriceListHeadId,
                SysWholesellerName = p.SysWholesellerName,
                ProductGroupCode = p.ProductGroup.Code,
                ProductGroupName = p.ProductGroup.Name,
                TimeCodeId = p.TimeCode.TimeCodeId,
                ShowDescriptionAsTextRow = p.ShowDescriptionAsTextRow,
                ShowDescrAsTextRowOnPurchase = p.ShowDescrAsTextRowOnPurchase,

                ProductAccounts = p.ProductAccountStd.Select(a => new ProductAccountStdDTO
                {
                    AccountId = a.AccountStd.AccountId,
                    Percent = a.Percent,
                    Type = (ProductAccountType)a.Type,
                    AccountStd = new AccountDTO
                    {
                        AccountNr = a.AccountStd.Account.AccountNr,
                    }
                }).ToList(),
            }
                ).ToList();

            /*if (loadAccounts)
            {
                return (from ip in entities.Product.OfType<InvoiceProduct>()
                            .Include("PriceList")
                            .Include("ProductUnit")
                            .Include("ProductGroup")
                            .Include("ProductAccountStd.AccountStd.Account")
                            .Include("ProductAccountStd.AccountInternal.Account.AccountDim")
                        where ip.Company.Any(i => i.ActorCompanyId == actorCompanyId) && !ip.ExternalProductId.HasValue
                        select ip).ToList<InvoiceProduct>();
            }
            else
            {
                return (from ip in entities.Product.OfType<InvoiceProduct>()
                            .Include("PriceList")
                            .Include("ProductUnit")
                            .Include("ProductGroup")
                        where ip.Company.Any(i => i.ActorCompanyId == actorCompanyId) &&
                        !ip.ExternalProductId.HasValue
                        select ip).ToList<InvoiceProduct>();
            }*/
        }


        public List<InvoiceProduct> GetInvoiceProductsBySearch(CompEntities entities, int actorCompanyId, string searchString, int number)
        {
            return (from p in entities.Product
                    where p.State == (int)SoeEntityState.Active &&
                    p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    (p.Number.StartsWith(searchString) || p.Name.Contains(searchString))
                    select p).OfType<InvoiceProduct>().Take(number).OrderBy(p => p.Number).ToList();
        }

        public List<InvoiceProduct> GetInvoiceProductsBySearch(int actorCompanyId, string searchString, bool onlyFixed)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetInvoiceProductsBySearch(entities, actorCompanyId, searchString, onlyFixed);
        }

        public List<InvoiceProduct> GetInvoiceProductsBySearch(CompEntities entities, int actorCompanyId, string searchString, bool onlyFixed)
        {
            var companyId = actorCompanyId != 0 ? actorCompanyId : base.ActorCompanyId;
            if (onlyFixed)
            {
                return (from p in entities.Product
                        where p.State == (int)SoeEntityState.Active &&
                        p.Company.Any(c => c.ActorCompanyId == companyId) &&
                        (p.Number.StartsWith(searchString) || p.Name.Contains(searchString))
                        select p).OfType<InvoiceProduct>().Where(p => p.CalculationType == (int)TermGroup_InvoiceProductCalculationType.FixedPrice).Take(200).OrderBy(p => p.Number).ToList();
            }
            else
            {
                return (from p in entities.Product
                        where p.State == (int)SoeEntityState.Active &&
                        p.Company.Any(c => c.ActorCompanyId == companyId) &&
                        (p.Number.StartsWith(searchString) || p.Name.Contains(searchString))
                        select p).OfType<InvoiceProduct>().OrderBy(p => p.Number).Take(200).ToList();
            }
        }

        public List<InvoiceProductSmallDTO> GetInvoiceProductsByCalculationType(int actorCompanyId, TermGroup_InvoiceProductCalculationType calculationType)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetInvoiceProductsByCalculationType(entities, actorCompanyId, calculationType);
        }

        public List<InvoiceProductSmallDTO> GetInvoiceProductsByCalculationType(CompEntities entities, int actorCompanyId, TermGroup_InvoiceProductCalculationType calculationType)
        {
            return (from p in entities.Product.OfType<InvoiceProduct>()
                    where p.State == (int)SoeEntityState.Active && p.CalculationType == (int)calculationType &&
                    p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                    select new InvoiceProductSmallDTO
                    {
                        Name = p.Name,
                        ProductId = p.ProductId,
                        Number = p.Number,
                        CalculationType = p.CalculationType,
                        ProductGroupId = p.ProductGroupId,
                        ProductUnitId = p.ProductUnitId
                    }).ToList();
        }

        public List<InvoiceProductPriceSearchDTO> GetInvoiceProductsBySearch(int actorCompanyId, string searchString, int fetchSize, int priceListTypeId, int customerId, int currencyId, int wholeSellerId, bool returnFormula, bool copySysProduct, bool? includeCustomerProducts = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetInvoiceProductsByNumberOrNameOrEAN(entities, actorCompanyId, searchString, fetchSize, priceListTypeId, customerId, currencyId, wholeSellerId, returnFormula, copySysProduct, includeCustomerProducts: includeCustomerProducts);
        }

        public List<InvoiceProductSearchResultDTO> SearchInvoiceProducts(CompEntities entities, int actorCompanyId, InvoiceProductSearchDTO search, int maxNrRows, int pageNumber)
        {
            IQueryable<InvoiceProduct> query = (from p in entities.Product.OfType<InvoiceProduct>()
                                                where
                                                p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                                //(p.Number.StartsWith(searchString) || p.Name.Contains(searchString) || p.EAN.ToString().Contains(searchString))
                                                select p).OfType<InvoiceProduct>();

            if (search.IncludeInactive)
            {
                query = query.Where(p => p.State == (int)SoeEntityState.Active || p.State == (int)SoeEntityState.Inactive);
            }
            else
            {
                query = query.Where(p => p.State == (int)SoeEntityState.Active);
            }

            if (!string.IsNullOrEmpty(search.Number))
            {
                if (search.Number.StartsWith("*") && search.Number.EndsWith("*"))
                    query = query.Where(c => c.Number.Contains(search.Number.Replace("*", "")));
                else if (search.Number.StartsWith("*"))
                    query = query.Where(c => c.Number.EndsWith(search.Number.Replace("*", "")));
                else if (search.Number.EndsWith("*"))
                    query = query.Where(c => c.Number.StartsWith(search.Number.Replace("*", "")));
                else
                    query = query.Where(c => c.Number == search.Number);
            }

            if (!string.IsNullOrEmpty(search.Name))
            {
                if (search.Name.StartsWith("*") && search.Name.EndsWith("*"))
                    query = query.Where(c => c.Name.Contains(search.Name.Replace("*", "")));
                else if (search.Name.StartsWith("*"))
                    query = query.Where(c => c.Name.EndsWith(search.Name.Replace("*", "")));
                else if (search.Name.EndsWith("*"))
                    query = query.Where(c => c.Name.StartsWith(search.Name.Replace("*", "")));
                else
                    query = query.Where(c => c.Name == search.Name);
            }

            if (search.ModifiedSince.HasValue)
            {
                query = query.Where(c => c.Created >= search.ModifiedSince || c.Modified >= search.ModifiedSince);
            }

            if (!search.ProductGroupIds.IsNullOrEmpty())
            {
                query = query.Where(c => c.ProductGroupId.HasValue && search.ProductGroupIds.Contains(c.ProductGroupId ?? 0));
            }

            if (pageNumber > 0)
            {
                query = query.OrderBy(x => x.ProductId).Skip(maxNrRows * (pageNumber - 1));
            }

            return query.Take(maxNrRows).Select(p => new InvoiceProductSearchResultDTO
            {
                ProductId = p.ProductId,
                ProductNr = p.Number,
                ProductName = p.Name,
                Description = p.Description,
                EAN = p.EAN,
                VatType = p.VatType,
                VatCodeNr = p.VatCode.Code,
                State = p.State,
                ProductUnitCode = p.ProductUnit.Code ?? string.Empty,
                External = p.ExternalProductId.HasValue,
                ProductGroupCode = p.ProductGroup.Code ?? string.Empty,
                DontUseDiscountPercent = p.DontUseDiscountPercent ?? false,
                IsStockProduct = p.IsStockProduct ?? false
            }).ToList();
        }

        public List<InvoiceProductPriceSearchDTO> GetInvoiceProductsByNumberOrNameOrEAN(CompEntities entities, int actorCompanyId, string searchString, int fetchSize, int priceListTypeId, int customerId, int currencyId, int wholeSellerId, bool returnFormula, bool copySysProduct, bool? includeCustomerProducts = null)
        {
            int sysCountryId = GetCompanySysCountryIdFromCache(entities, actorCompanyId);
            List<InvoiceProductPriceSearchDTO> result = new List<InvoiceProductPriceSearchDTO>();

            var products = (from p in entities.Product.OfType<InvoiceProduct>()
                            where p.State == (int)SoeEntityState.Active &&
                            p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                            (p.Number.StartsWith(searchString) || p.Name.Contains(searchString) || p.EAN.ToString().Contains(searchString))
                            select p).OfType<InvoiceProduct>()
                                             //.OrderByDescending(p => SqlFunctions.PatIndex(searchString, p.Number))
                                             //.ThenByDescending(p => SqlFunctions.PatIndex(searchString, p.Name))
                                             //.Take(fetchSize).
                                             .Select(x => new InvoiceProductPriceSearchExDTO
                                             {
                                                 ProductId = x.ProductId,
                                                 ProductNr = x.Number,
                                                 ProductName = x.Name,
                                                 ProductUnitCode = x.ProductUnit.Code ?? string.Empty,
                                                 ExternalProductId = x.ExternalProductId,
                                             }
                                             ).ToList();
            foreach (var p in products)
            {
                p.NumberIndexOf = p.ProductNr.IndexOf(searchString);
                p.NameIndexOf = p.ProductName.IndexOf(searchString);
                if (p.NumberIndexOf < 0)
                {
                    p.NumberIndexOf = int.MaxValue;
                }

                if (p.NameIndexOf < 0)
                {
                    p.NameIndexOf = int.MaxValue;
                }
                p.External = p.ExternalProductId.GetValueOrDefault() > 0;
            }

            products = products.OrderBy(x => x.NumberIndexOf).ThenBy(y => y.NameIndexOf).ThenBy(z => z.ProductName).ThenBy(s => s.ProductNr).Take(fetchSize).ToList();

            foreach (var product in products)
            {
                InvoiceProductPriceResult priceResult = GetProductPrice(entities, actorCompanyId, new ProductPriceRequestDTO { PriceListTypeId = priceListTypeId, ProductId = product.ProductId, CustomerId = customerId, CurrencyId = currencyId, WholesellerId = wholeSellerId, ReturnFormula = returnFormula, CopySysProduct = copySysProduct }, includeCustomerPrices: includeCustomerProducts);
                if (priceResult != null)
                {
                    product.SalesPrice = priceResult.SalesPrice;
                    product.SysWholesellerName = priceResult.SysWholesellerName;
                }
                else
                    product.SalesPrice = 0;

                if (product.ExternalProductId.GetValueOrDefault() > 0)
                {
                    var sysProduct = SysPriceListManager.GetSysProduct(product.ExternalProductId.Value);
                    var sysProductType = sysProduct?.Type;
                    if (sysProductType.HasValue)
                    {
                        product.ExternalUrl = GetProductExternalUrl((TermGroup_Country)sysCountryId, (ExternalProductType)sysProductType.Value, product.ProductNr, product.ProductName);
                        product.ExternalId = sysProduct.ExternalId;
                        product.ExtendedInfo = sysProduct.ExtendedInfo;
                        product.ImageFileName = sysProduct.ImageFileName;
                    }
                }
                result.Add(product);
            }

            return result;
        }

        public Dictionary<int, string> GetInvoiceProductsDict(int actorCompanyId, TermGroup_InvoiceProductVatType vatType, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<InvoiceProduct> invoiceProducts = GetInvoiceProducts(actorCompanyId, true);
            foreach (var invoiceProduct in invoiceProducts.OrderBy(p => p.Number))
            {
                if (vatType != TermGroup_InvoiceProductVatType.None && (int)vatType != invoiceProduct.VatType)
                    continue;

                dict.Add(invoiceProduct.ProductId, invoiceProduct.Name);
            }

            return dict.Sort();
        }

        public InvoiceProduct GetInvoiceProductFromSetting(CompanySettingType settingType, int actorCompanyId, bool loadPriceList = false, bool loadPriceListUnitAndGroup = false, bool loadAccounts = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetInvoiceProductFromSetting(entities, settingType, actorCompanyId, loadPriceList, loadPriceListUnitAndGroup, loadAccounts);
        }

        public InvoiceProduct GetInvoiceProductFromSetting(CompEntities entities, CompanySettingType settingType, int actorCompanyId, bool loadPriceList = false, bool loadProductUnitAndGroup = false, bool loadAccounts = false)
        {
            int productId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)settingType, 0, actorCompanyId, 0);
            return GetInvoiceProduct(entities, productId, loadPriceList, loadProductUnitAndGroup, loadAccounts);
        }

        public List<InvoiceProductSmallDTO> GetInvoiceProductsSmall(int actorCompanyId, bool excludeExternal)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetInvoiceProductsSmall(entities, actorCompanyId, excludeExternal);
        }
        public List<InvoiceProductSmallDTO> GetInvoiceProductsSmall(CompEntities entities, int actorCompanyId, bool excludeExternal)
        {
            bool showPurchasePrice = FeatureManager.HasRolePermission(Feature.Billing_Product_Products_ShowPurchasePrice, Permission.Readonly, base.RoleId, base.ActorCompanyId);

            var query = entities.Product.OfType<InvoiceProduct>()
                .Where(p => p.Company.Any(c => c.ActorCompanyId == actorCompanyId) && p.State == (int)SoeEntityState.Active);

            if (excludeExternal)
            {
                query = query.Where(p => p.ExternalProductId == null);
            }

            var result = query.Select(p => new InvoiceProductSmallDTO
            {
                Name = p.Name,
                Number = p.Number,
                PurchasePrice = p.PurchasePrice,
                ProductUnitId = p.ProductUnitId,
                ProductId = p.ProductId,
            })
                .OrderBy(p => p.Number)
                .ToList();

            if (!showPurchasePrice)
            {
                result.ForEach(p => p.PurchasePrice = 0);
            }

            return result;
        }

        public InvoiceProductSmallDTO GetInvoiceProductSmall(CompEntities entities, int productId, int actorCompanyId)
        {
            if (productId == 0)
                return null;

            string cacheKey = $"InvoiceProductSmall#productId{productId}#actorCompanyId{actorCompanyId}";
            InvoiceProductSmallDTO product = BusinessMemoryCache<InvoiceProductSmallDTO>.Get(cacheKey);

            if (product == null)
            {
                product = (from p in entities.Product.OfType<InvoiceProduct>()
                           where p.ProductId == productId
                           select new InvoiceProductSmallDTO
                           {
                               ProductId = p.ProductId,
                               Name = p.Name,
                               Number = p.Number,
                               CalculationType = p.CalculationType,
                               ProductGroupId = p.ProductGroupId,
                               ProductUnitId = p.ProductUnitId,
                               GuaranteePercentage = p.GuaranteePercentage,
                               UseCalculatedCost = p.UseCalculatedCost,
                               PurchasePrice = p.PurchasePrice
                           }).FirstOrDefault();

                if (product != null)
                {
                    BusinessMemoryCache<InvoiceProductSmallDTO>.Set(cacheKey, product, 120);
                }
            }

            return product;
        }

        public InvoiceProductSmallDTO GetInvoiceProductSmall(CompEntities entities, string productNr, int actorCompanyId)
        {
            if (productNr.IsNullOrEmpty())
                return null;

            string cacheKey = $"InvoiceProductSmall#productNr{productNr}#actorCompanyId{actorCompanyId}";
            InvoiceProductSmallDTO product = BusinessMemoryCache<InvoiceProductSmallDTO>.Get(cacheKey);

            if (product == null)
            {
                product = (from p in entities.Product.OfType<InvoiceProduct>()
                           where p.Number == productNr &&
                                 p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                                 p.State == (int)SoeEntityState.Active
                           select new InvoiceProductSmallDTO
                           {
                               ProductId = p.ProductId,
                               Name = p.Name,
                               Number = p.Number,
                               CalculationType = p.CalculationType,
                               ProductGroupId = p.ProductGroupId,
                               ProductUnitId = p.ProductUnitId,
                               GuaranteePercentage = p.GuaranteePercentage,
                               UseCalculatedCost = p.UseCalculatedCost,
                               PurchasePrice = p.PurchasePrice
                           }).FirstOrDefault();

                if (product != null)
                {
                    BusinessMemoryCache<InvoiceProductSmallDTO>.Set(cacheKey, product, 120);
                }
            }

            return product;
        }

        public InvoiceProduct GetInvoiceProduct(int productId, bool loadPriceList = false, bool loadProductUnitAndGroup = false, bool loadAccounts = false, bool loadCategories = false, bool loadVatCode = false, bool loadTimeCode = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetInvoiceProduct(entities, productId, loadPriceList, loadProductUnitAndGroup, loadAccounts, loadCategories, loadVatCode, loadTimeCode);
        }

        public InvoiceProduct GetInvoiceProduct(CompEntities entities, int productId, bool loadPriceList = false, bool loadProductUnitAndGroup = false, bool loadAccounts = false, bool loadCategories = false, bool loadVatCode = false, bool loadTimeCode = false)
        {
            if (productId == 0)
                return null;

            IQueryable<InvoiceProduct> query = entities.Product.OfType<InvoiceProduct>();

            if (loadProductUnitAndGroup)
            {
                query = query.Include("ProductUnit");
                query = query.Include("ProductGroup");
            }

            if (loadAccounts)
            {
                query = query.Include("ProductAccountStd.AccountStd");
                query = query.Include("ProductAccountStd.AccountInternal.Account.AccountDim");
            }

            if (loadVatCode)
            {
                query = query.Include("VatCode");
            }

            if (loadTimeCode)
            {
                query = query.Include("TimeCode");
            }

            var invoiceProduct = (from p in query
                                  where p.ProductId == productId
                                  select p).OfType<InvoiceProduct>().FirstOrDefault();

            if (invoiceProduct != null)
            {
                if (loadPriceList && !invoiceProduct.PriceList.IsLoaded)
                {
                    invoiceProduct.PriceList.Load();
                }

                if (loadCategories)
                {

                    invoiceProduct.CategoryIds = (from c in entities.CompanyCategoryRecord
                                                  where c.RecordId == invoiceProduct.ProductId &&
                                                  c.Entity == (int)SoeCategoryRecordEntity.Product &&
                                                  c.Category.Type == (int)SoeCategoryType.Product &&
                                                  c.Category.ActorCompanyId == ActorCompanyId &&
                                                  c.Category.State == (int)SoeEntityState.Active
                                                  select c.CategoryId).ToList();
                }

                if (invoiceProduct.ExternalProductId.GetValueOrDefault() > 0)
                {
                    invoiceProduct.SysProductType = SysPriceListManager.GetSysProduct(invoiceProduct.ExternalProductId.Value)?.Type;
                }
            }

            return invoiceProduct;
        }

        public InvoiceProduct GetInvoiceProduct(CompEntities entities, int sysProductId, int actorCompanyId, PriceListOrigin origin, int priceListHeadId)
        {
            InvoiceProduct product;
            var query = (from p in entities.Product.OfType<InvoiceProduct>()
                            .Include("PriceList")
                            .Include("ProductUnit")
                            .Include("ProductGroup")
                         where p.ExternalProductId == sysProductId &&
                         p.ExternalPriceListHeadId == priceListHeadId &&
                         p.State == (int)SoeEntityState.Active &&
                         p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                         select p);

            if (origin != PriceListOrigin.SysAndCompDbPriceList || origin != PriceListOrigin.Unknown)
                query = query.Where(p => p.PriceListOrigin == (int)PriceListOrigin.Unknown || p.PriceListOrigin == (int)origin);

            product = query.OrderByDescending(p => p.ProductId).FirstOrDefault();

            return product;
        }

        public InvoiceProduct GetInvoiceProduct(CompEntities entities, int sysProductId, int actorCompanyId, PriceListOrigin origin)
        {
            InvoiceProduct product;
            var query = (from p in entities.Product.OfType<InvoiceProduct>()
                            .Include("PriceList")
                            .Include("ProductUnit")
                            .Include("ProductGroup")
                         where p.ExternalProductId == sysProductId &&
                         p.State == (int)SoeEntityState.Active &&
                         p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                         select p);

            if (origin == PriceListOrigin.CompDbNetPriceList)
                query = query.Where(p => p.PriceListOrigin == (int)PriceListOrigin.SysDbPriceList || p.PriceListOrigin == (int)origin);
            else if (origin != PriceListOrigin.SysAndCompDbPriceList || origin != PriceListOrigin.Unknown)
                query = query.Where(p => p.PriceListOrigin == (int)PriceListOrigin.Unknown || p.PriceListOrigin == (int)origin);

            product = query.OrderByDescending(p => p.ProductId).FirstOrDefault();

            return product;
        }

        public InvoiceProduct GetInvoiceProductByProductNr(string productNr, int actorCompanyId, bool loadAccounts = false, bool loadProductGroup = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetInvoiceProductByProductNr(entities, productNr, actorCompanyId, 0, loadAccounts, loadProductGroup);
        }

        public InvoiceProduct GetInvoiceProductByProductNr(CompEntities entities, string productNr, int actorCompanyId, int prioExternalPriceListHeadId = 0, bool loadAccounts = false, bool loadProductGroup = false)
        {

            IQueryable<InvoiceProduct> query = entities.Product.OfType<InvoiceProduct>();

            query = query.Include("PriceList");
            if (loadAccounts)
            {
                query = query.Include("ProductAccountStd.AccountStd");
                query = query.Include("ProductAccountStd.AccountInternal.Account.AccountDim");
            }

            if (loadProductGroup)
            {
                query = query.Include("ProductGroup");

            }

            var products = (from p in query
                            where p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                              p.Number == productNr && p.State == (int)SoeEntityState.Active
                            select p).ToList();

            if (prioExternalPriceListHeadId > 0 && products.Count > 1 && products.Any(p => p.ExternalPriceListHeadId == prioExternalPriceListHeadId))
            {
                return products.FirstOrDefault(p => p.ExternalPriceListHeadId == prioExternalPriceListHeadId);
            }

            return products.FirstOrDefault();
        }

        public InvoiceProduct GetInvoiceProductByProductName(CompEntities entities, string productName, int actorCompanyId, bool loadAccounts = false, bool loadProductGroup = false)
        {
            IQueryable<InvoiceProduct> query = entities.Product.OfType<InvoiceProduct>();

            query = query.Include("PriceList");
            if (loadAccounts)
            {
                query = query.Include("ProductAccountStd.AccountStd");
                query = query.Include("ProductAccountStd.AccountInternal.Account.AccountDim");
            }

            if (loadProductGroup)
            {
                query = query.Include("ProductGroup");

            }

            var products = (from p in query
                            where p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                              p.Name == productName && p.State == (int)SoeEntityState.Active
                            select p).ToList();

            return products.FirstOrDefault();
        }

        public InvoiceProduct GetInvoiceProductFromSys(int sysProductId, int sysWholeSellerId, int actorCompanyId, decimal salesPrice, decimal purchasePrice, PriceListOrigin priceListOrigin, string productUnitString)
        {
            using (CompEntities entities = new CompEntities())
            {
                // Check if product already exists
                InvoiceProduct invoiceProduct = GetInvoiceProduct(entities, sysProductId, actorCompanyId, priceListOrigin);

                ProductUnit productUnit = null;
                if (!string.IsNullOrEmpty(productUnitString))
                {
                    productUnit = GetProductUnit(entities, productUnitString, actorCompanyId);
                }

                if (invoiceProduct != null && productUnit != null)
                {
                    ConvertPrice(entities, actorCompanyId, invoiceProduct.ProductId, productUnit, purchasePrice, salesPrice, out decimal outPurchasePrice, out decimal outSalesPrice);
                    invoiceProduct.PurchasePrice = NumberUtility.GetFormattedDecimalValue(outPurchasePrice, 2);
                    invoiceProduct.SalesPrice = NumberUtility.GetFormattedDecimalValue(outSalesPrice, 2);
                }

                return invoiceProduct;
            }
        }

        private void ConvertPrice(CompEntities entities, int actorCompanyId, int invoiceProductId, ProductUnit productUnit, decimal purchasePrice, decimal salesPrice, out decimal outPurchasePrice, out decimal outSalesPrice)
        {
            #region Prereq
            outPurchasePrice = purchasePrice;
            outSalesPrice = salesPrice;
            #endregion

            bool useProductUnitConvert = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUseProductUnitConvert, 0, actorCompanyId, 0, false);
            if (useProductUnitConvert && productUnit != null)
            {
                var unitConvert = ProductManager.GetProductUnitConvert(entities, invoiceProductId, productUnit.ProductUnitId);
                if (unitConvert != null && unitConvert.ConvertFactor != 0)
                {
                    if (purchasePrice != 0)
                        outPurchasePrice = NumberUtility.GetFormattedDecimalValue(purchasePrice / unitConvert.ConvertFactor, 2);

                    if (salesPrice != 0)
                        outSalesPrice = NumberUtility.GetFormattedDecimalValue(salesPrice / unitConvert.ConvertFactor, 2);
                }
            }
        }

        public int GetProductAccountId(int productId, ProductAccountType productAccountType)
        {
            InvoiceProduct product = GetInvoiceProduct(productId, loadAccounts: true);
            if (product != null)
            {
                if (!product.ProductAccountStd.IsLoaded)
                    product.ProductAccountStd.Load();

                ProductAccountStd productAccountStd = product.ProductAccountStd.FirstOrDefault(a => a.Type == (int)productAccountType);
                if (productAccountStd != null)
                {
                    if (!productAccountStd.AccountStdReference.IsLoaded)
                        productAccountStd.AccountStdReference.Load();

                    if (productAccountStd.AccountStd != null)
                        return productAccountStd.AccountStd.AccountId;
                }
            }

            return 0;
        }

        public ActionResult SaveInvoiceProduct(InvoiceProductDTO productInput, List<PriceListDTO> priceLists, List<CompanyCategoryRecordDTO> categoryRecords, int actorCompanyId, List<StockDTO> stocks = null, List<CompTermDTO> inputTranslations = null, List<ExtraFieldRecordDTO> extrafields = null, bool allowOnlyInserts = false)
        {
            if (productInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "InvoiceProduct");

            if (productInput.Weight != null && productInput.Weight < 0)
                return new ActionResult((int)ActionResultSave.ProductWeightInvalid, "InvoiceProduct");

            // Default result is successful
            ActionResult result = new ActionResult(true);

            int productId = productInput.ProductId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Product

                        // Default accounting prio (if not set)
                        if (string.IsNullOrEmpty(productInput.AccountingPrio))
                            productInput.AccountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0";

                        // Get existing product
                        InvoiceProduct product = null;
                        if (productId != 0)
                        {
                            product = GetInvoiceProduct(entities, productId, true, true, true);
                            if (product.Number != productInput.Number && (from p in entities.Product.OfType<InvoiceProduct>() where p.Company.Any(c => c.ActorCompanyId == actorCompanyId) && p.Number == productInput.Number && p.ProductId != productId && p.State == (int)SoeEntityState.Active select p).Any())
                                return new ActionResult((int)ActionResultSave.ProductExists, string.Format(GetText(7494, "En artikel med nummer {0} existerar redan. Ange ett annat nummer för att spara."), productInput.Number));
                        }
                        else
                        {
                            // New - check number
                            if ((from p in entities.Product.OfType<InvoiceProduct>()
                                 where p.Company.Any(c => c.ActorCompanyId == actorCompanyId) && p.Number == productInput.Number && p.State == (int)SoeEntityState.Active
                                 select p).Any())
                                return new ActionResult((int)ActionResultSave.ProductExists, string.Format(GetText(7494, "En artikel med nummer {0} existerar redan. Ange ett annat nummer för att spara."), productInput.Number));
                        }

                        if (allowOnlyInserts && product?.Number == productInput.Number && InvoiceProductExist(entities, productInput.Number, actorCompanyId))
                            return new ActionResult((int)ActionResultSave.ProductExists);

                        if (product == null)
                        {
                            #region Product Add

                            product = new InvoiceProduct()
                            {
                                ProductUnitId = productInput.ProductUnitId.ToNullable(),
                                ProductGroupId = productInput.ProductGroupId.ToNullable(),
                                Type = (int)SoeProductType.InvoiceProduct,
                                Number = productInput.Number,
                                Name = productInput.Name,
                                Description = productInput.Description,
                                ShowDescriptionAsTextRow = productInput.ShowDescriptionAsTextRow,
                                ShowDescrAsTextRowOnPurchase = productInput.ShowDescrAsTextRowOnPurchase,
                                DontUseDiscountPercent = productInput.DontUseDiscountPercent,
                                VatType = (int)productInput.VatType,
                                EAN = productInput.EAN,
                                TimeCodeId = productInput.TimeCodeId.HasValue && productInput.TimeCodeId.Value > 0 ? productInput.TimeCodeId.Value : (int?)null,
                                VatCodeId = productInput.VatCodeId.ToNullable(),
                                AccountingPrio = productInput.AccountingPrio,
                                PurchasePrice = productInput.PurchasePrice,
                                SysWholesellerName = productInput.SysWholesellerName,
                                ExternalProductId = productInput.SysProductId,
                                CalculationType = (int)productInput.CalculationType,
                                State = productInput.State,
                                HouseholdDeductionPercentage = productInput.HouseholdDeductionPercentage,
                                IsStockProduct = productInput.IsStockProduct,
                                GuaranteePercentage = productInput.GuaranteePercentage,
                                HouseholdDeductionType = productInput.HouseholdDeductionType,
                                UseCalculatedCost = productInput.UseCalculatedCost,
                                Weight = productInput.Weight,
                                IntrastatCodeId = productInput.IntrastatCodeId.ToNullable(),
                                SysCountryId = productInput.SysCountryId.ToNullable(),
                                DefaultGrossMarginCalculationType = productInput.DefaultGrossMarginCalculationType.ToNullable(),
                            };

                            SetCreatedProperties(product);

                            result = AddEntityItem(entities, product, "Product");
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.ProductNotSaved;
                                return result;
                            }

                            result = MapCompanyToInvoiceProduct(entities, product, actorCompanyId);

                            #endregion

                            #region Accounts

                            if (!productInput.AccountingSettings.IsNullOrEmpty())
                            {
                                //Angular
                                SaveAccountingSettings(entities, product, productInput.AccountingSettings, actorCompanyId);
                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProductAccountsNotSaved;
                                    return result;
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            if (productInput.State == (int)SoeEntityState.Deleted)
                            {
                                result = IsOkToChangeStateOnProduct(entities, product.ProductId, SoeProductType.InvoiceProduct, product.IsStockProduct, actorCompanyId);

                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProductInUse;
                                }
                            }
                            else if (productInput.State == (int)SoeEntityState.Inactive)
                            {
                                result = IsOkToInactivateProduct(entities, product, actorCompanyId);
                                if (!result.Success)
                                {
                                    return result;
                                }
                            }

                            if (result.Success)
                            {
                                #region Product Update

                                // Update Product
                                product.ProductUnitId = productInput.ProductUnitId.ToNullable();
                                product.ProductGroupId = productInput.ProductGroupId.ToNullable();
                                product.Number = productInput.Number;
                                product.Name = productInput.Name;
                                product.Description = productInput.Description;
                                product.ShowDescriptionAsTextRow = productInput.ShowDescriptionAsTextRow;
                                product.ShowDescrAsTextRowOnPurchase = productInput.ShowDescrAsTextRowOnPurchase;
                                product.DontUseDiscountPercent = productInput.DontUseDiscountPercent;
                                product.VatType = (int)productInput.VatType;
                                product.EAN = productInput.EAN;
                                product.TimeCodeId = productInput.TimeCodeId;
                                product.VatCodeId = productInput.VatCodeId;
                                product.AccountingPrio = productInput.AccountingPrio;
                                product.PurchasePrice = productInput.PurchasePrice;
                                product.SysWholesellerName = productInput.SysWholesellerName;
                                product.CalculationType = (int)productInput.CalculationType;
                                product.State = productInput.State;
                                product.HouseholdDeductionPercentage = productInput.HouseholdDeductionPercentage;
                                if (productInput.IsStockProduct)
                                {
                                    product.IsStockProduct = productInput.IsStockProduct;
                                }
                                product.GuaranteePercentage = productInput.GuaranteePercentage;
                                product.HouseholdDeductionType = productInput.HouseholdDeductionType;
                                product.UseCalculatedCost = productInput.UseCalculatedCost;
                                product.Weight = productInput.Weight;
                                product.IntrastatCodeId = productInput.IntrastatCodeId.ToNullable();
                                product.SysCountryId = productInput.SysCountryId.ToNullable();
                                product.DefaultGrossMarginCalculationType = productInput.DefaultGrossMarginCalculationType.ToNullable();

                                if (productInput.IsExternal.HasValue && productInput.IsExternal == false && (product.ExternalPriceListHeadId.HasValue || product.ExternalProductId.HasValue))
                                {
                                    // Remove connection to external product
                                    product.ExternalProductId = product.ExternalPriceListHeadId = null;
                                }

                                SetModifiedProperties(product);

                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProductNotSaved;
                                    return result;
                                }

                                #endregion

                                #region Accounts

                                if (!productInput.AccountingSettings.IsNullOrEmpty())
                                {
                                    //Angular
                                    SaveAccountingSettings(entities, product, productInput.AccountingSettings, actorCompanyId);
                                    result = SaveChanges(entities, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.ProductAccountsNotSaved;
                                        return result;
                                    }
                                }

                                #endregion
                            }
                        }

                        if (result.Success)
                            productId = product.ProductId;
                        else
                        {
                            if (result.ErrorNumber == 0)
                                result.ErrorNumber = (int)ActionResultSave.ProductNotSaved;

                            return result;
                        }

                        #endregion

                        #region Pricelists

                        if (priceLists != null)
                        {
                            result = ProductPricelistManager.SavePriceLists(entities, transaction, product, null, priceLists, actorCompanyId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.ProductPriceListNotSaved;
                                return result;
                            }
                        }

                        #endregion

                        #region Categories

                        if (categoryRecords != null)
                        {
                            // Silverlight
                            result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, categoryRecords, actorCompanyId, SoeCategoryType.Product, SoeCategoryRecordEntity.Product, productId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.ProductCategoriesNotSaved;
                                result.ErrorMessage = GetText(11012, "Alla kategorier kunde inte sparas");
                                return result;
                            }
                        }

                        #endregion

                        #region Stocks

                        if (stocks != null)
                        {
                            result = StockManager.SaveStockProducts(entities, transaction, stocks, product, actorCompanyId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                return result;
                            }
                        }

                        #endregion

                        #region Translations
                        if (inputTranslations != null)
                        {
                            var langIdsToSave = inputTranslations.Select(i => (int)i.Lang).Distinct().ToList();
                            var existingTranslations = TermManager.GetCompTerms(entities, CompTermsRecordType.ProductName, product.ProductId);

                            #region Delete existing translations for other languages

                            foreach (var existingTranslation in existingTranslations)
                            {
                                if (langIdsToSave.Contains(existingTranslation.LangId))
                                    continue;

                                existingTranslation.State = (int)SoeEntityState.Deleted;
                            }

                            #endregion

                            #region Add or update translations for languages

                            foreach (int langId in langIdsToSave)
                            {
                                CompTerm translation = null;
                                var inputTranslation = inputTranslations.FirstOrDefault(i => (int)i.Lang == langId);

                                var existingTranslationsForLang = existingTranslations.Where(i => i.LangId == langId).ToList();
                                if (existingTranslationsForLang.Count == 0)
                                {
                                    #region Add

                                    translation = new CompTerm { ActorCompanyId = actorCompanyId };
                                    entities.CompTerm.AddObject(translation);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    for (int i = 0; i < existingTranslationsForLang.Count; i++)
                                    {
                                        if (i > 0)
                                        {
                                            //Remove duplicates
                                            existingTranslationsForLang[i].State = (int)SoeEntityState.Deleted;
                                            continue;
                                        }

                                        translation = existingTranslationsForLang[i];
                                    }

                                    #endregion
                                }

                                #region Set values

                                translation.RecordType = (int)inputTranslation.RecordType;
                                translation.RecordId = inputTranslation.RecordId;
                                translation.LangId = (int)inputTranslation.Lang;
                                translation.Name = inputTranslation?.Name ?? string.Empty;
                                translation.State = (int)inputTranslation.State > 0 ? (int)inputTranslation.State : (int)SoeEntityState.Active;

                                #endregion
                            }

                            #endregion

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.TranslationsSaveFailed;
                                return result;
                            }
                        }
                        #endregion

                        #region ExtraFields

                        if (extrafields != null && extrafields.Count > 0)
                        {
                            result = ExtraFieldManager.SaveExtraFieldRecords(entities, extrafields, (int)SoeEntityType.InvoiceProduct, productId, actorCompanyId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                return result;
                            }
                        }

                        #endregion

                        //Commit transaction
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
                        result.IntegerValue = productId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult UpdateProductsState(Dictionary<int, bool> products, int actorCompanyId)
        {
            using (var entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> product in products)
                {
                    var originalProduct = GetInvoiceProduct(entities, product.Key);
                    if (originalProduct == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "Product");

                    var result = IsOkToInactivateProduct(entities, originalProduct, actorCompanyId);
                    if (!result.Success)
                    {
                        return result;
                    }

                    ChangeEntityState(originalProduct, product.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult DeleteInvoiceProduct(int productId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return DeleteInvoiceProduct(entities, productId, actorCompanyId);
        }

        public ActionResult DeleteProducts(List<int> productIds, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);
            using (var entities = new CompEntities())
            {
                foreach (int productId in productIds)
                {
                    result = DeleteInvoiceProduct(entities, productId, actorCompanyId);
                    if (!result.Success)
                        return result;
                }
                return result;
            }
        }

        public ActionResult DeleteInvoiceProduct(CompEntities entities, int productId, int actorCompanyId)
        {
            InvoiceProduct originalInvoiceProduct = GetInvoiceProduct(entities, productId);
            if (originalInvoiceProduct == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, base.GetText(8553, 1, "InvoiceProduct"));

            // Check that product is not used anywhere
            var result = IsOkToChangeStateOnProduct(entities, productId, SoeProductType.InvoiceProduct, originalInvoiceProduct.IsStockProduct, actorCompanyId);
            if (!result.Success)
                return result;

            result = UnMapCompanyFromInvoiceProduct(entities, originalInvoiceProduct, actorCompanyId);
            if (!result.Success)
                return result;

            // Set the Product to deleted if no other Companies use it
            if (originalInvoiceProduct.Company.Count == 0)
            {
                result = ChangeEntityState(entities, originalInvoiceProduct, SoeEntityState.Deleted, true);
                if (!result.Success)
                    result.ErrorNumber = (int)ActionResultDelete.ProductNotDeleted;
            }
            else
                result.Success = false;

            return result;
        }

        public ActionResult InactivateProducts(List<int> productIds, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);
            using (var entities = new CompEntities())
            {
                foreach (int productId in productIds)
                {
                    result = InactivateInvoiceProduct(entities, productId, actorCompanyId);
                    if (!result.Success)
                        return result;
                }
                return result;
            }
        }

        public ActionResult InactivateInvoiceProduct(CompEntities entities, int productId, int actorCompanyId)
        {
            InvoiceProduct originalInvoiceProduct = GetInvoiceProduct(entities, productId);
            if (originalInvoiceProduct == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, base.GetText(8331,1, "InvoiceProduct"));

            var result = IsOkToInactivateProduct(entities, originalInvoiceProduct, actorCompanyId);
            if (!result.Success)
            {
                return result;
            }

            if (originalInvoiceProduct.Company.Count == 0)
            {
                result = ChangeEntityState(entities, originalInvoiceProduct, SoeEntityState.Inactive, true);
                if (!result.Success)
                    result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
            }
            else
                result.Success = false;

            return result;
        }



        public List<string> GetProductExternalUrls(int actorCompanyId, List<int> productIds)
        {
            var urls = new List<string>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<InvoiceProduct> query = (from p in entitiesReadOnly.Product.OfType<InvoiceProduct>()
                                                where
                                                   p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                                select p);

            if (productIds != null)
            {
                query = query.Where(p => productIds.Contains(p.ProductId));
            }

            int sysCountryId = GetCompanySysCountryIdFromCache(entitiesReadOnly, actorCompanyId);

            var products = query.Select(x => new
            {
                x.Number,
                x.ExternalProductId,
                x.PriceListOrigin
            }).ToList();

            foreach (var product in products)
            {
                if (product.ExternalProductId.GetValueOrDefault() > 0 && product.PriceListOrigin == (int)PriceListOrigin.SysDbPriceList)
                {
                    var sysProduct = SysPriceListManager.GetSysProduct(product.ExternalProductId.Value);

                    if (sysProduct != null)
                    {
                        var url = GetProductExternalUrl((TermGroup_Country)sysCountryId, (ExternalProductType)sysProduct.Type, product.Number, sysProduct.Name);
                        if (!string.IsNullOrEmpty(url))
                        {
                            urls.Add(url);
                        }
                    }
                }
            }

            return urls;
        }

        private static string GetProductExternalUrl(TermGroup_Country country, ExternalProductType productType, string productNumber, string productName)
        {
            if (country == TermGroup_Country.SE && productType == ExternalProductType.Plumbing)
            {
                return ProductExternalUrls.RskDatabaseSearch + productNumber;
            }
            else if (country == TermGroup_Country.FI && productType == ExternalProductType.Electric)
            {
                return ProductExternalUrls.Sahkonumerot + productNumber;
            }
            else if (country == TermGroup_Country.FI && productType == ExternalProductType.Plumbing)
            {
                return ProductExternalUrls.LviInfo + productNumber;
            }
            else if (country == TermGroup_Country.SE && productType == ExternalProductType.Bevego)
            {
                return ProductExternalUrls.Bevego + Bevego.NameToUrl(productName);
            }
            else if (country == TermGroup_Country.SE && productType == ExternalProductType.Lindab)
            {
                return ProductExternalUrls.Lindab + productNumber;
            }
            else
            {
                return "";
            }
        }

        public bool InvoiceProductExist(CompEntities entities, string productNr, int actorCompanyId)
        {
            InvoiceProduct invoiceProduct = GetInvoiceProductByProductNr(entities, productNr, actorCompanyId);
            return invoiceProduct != null;
        }

        public ActionResult MapCompanyToInvoiceProduct(CompEntities entities, InvoiceProduct invoiceProduct, int actorCompanyId)
        {
            if (invoiceProduct == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "InvoiceProduct");

            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            if (!invoiceProduct.Company.IsLoaded)
                invoiceProduct.Company.Load();
            invoiceProduct.Company.Add(company);

            return SaveEntityItem(entities, invoiceProduct);
        }

        public ActionResult UnMapCompanyFromInvoiceProduct(CompEntities entities, InvoiceProduct invoiceProduct, int actorCompanyId)
        {
            if (invoiceProduct == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "InvoiceProduct");

            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            if (!invoiceProduct.Company.IsLoaded)
                invoiceProduct.Company.Load();
            invoiceProduct.Company.Remove(company);

            return SaveEntityItem(entities, invoiceProduct);
        }

        public InvoiceProduct CopyExternalInvoiceProductFromCompPriceListByProductNr(CompEntities entities, string productNbr, decimal purchasePrice, int sysWholesellerId, string productUnit, string sysWholesellerName, int actorCompanyId, int actorCustomerId)
        {
            InvoiceProduct invoiceProduct = null;
            var item = entities.PriceListImportedSearch(actorCompanyId, productNbr, "", null, 1).FirstOrDefault();

            if (item != null)
            {
                var result2 = SearchInvoiceProductPrice(actorCompanyId, (int)PriceListOrigin.CompDbPriceList, actorCustomerId, 0, item.ProductId, sysWholesellerId, false);

                //fix for also checking etman pipe pricelist...
                if ((result2 == null) && (sysWholesellerId == 78))
                {
                    result2 = SearchInvoiceProductPrice(actorCompanyId, (int)PriceListOrigin.CompDbPriceList, actorCustomerId, 0, item.ProductId, 79, false);
                }

                //fix for LundaNetto/LundaBrutto
                if ((result2 == null) && (sysWholesellerId == 20))
                {
                    result2 = SearchInvoiceProductPrice(actorCompanyId, (int)PriceListOrigin.CompDbPriceList, actorCustomerId, 0, item.ProductId, 85, false);
                }
                if ((result2 == null) && (sysWholesellerId == 20))
                {
                    result2 = SearchInvoiceProductPrice(actorCompanyId, (int)PriceListOrigin.CompDbPriceList, actorCustomerId, 0, item.ProductId, 21, false);
                }

                if (result2 != null)
                {
                    invoiceProduct = CopyExternalInvoiceProduct(entities, item.ProductId, purchasePrice, purchasePrice, productUnit, 0, result2.SysPriceListHeadId, sysWholesellerName, actorCustomerId, actorCompanyId, true, PriceListOrigin.CompDbPriceList);
                }
            }

            return invoiceProduct;
        }

        public InvoiceProduct CopyExternalInvoiceProductFromSysByProductNr(CompEntities entities, int sysProductId, string productNbr, decimal purchasePrice, int sysWholesellerId, string productUnit, int sysPriceListHeadId, string sysWholesellerName, int actorCompanyId, int actorCustomerId, bool saveChanges)
        {
            InvoiceProduct invoiceProduct = null;
            if (productNbr.StartsWith("E") && productNbr.Length == 8)
                productNbr = productNbr.Substring(1);

            decimal salesPrice = purchasePrice; //TODO: Apply SupplierAgreement

            if (sysProductId == 0)
            {
                var result = AzureSearchSysProducts(entities, actorCompanyId, productNbr, "", string.Empty, string.Empty, 10);

                var filtered = result.Where(r => r.SysPriceListSmallDTOs.Any(p => p.SysWholesellerId == sysWholesellerId) && r.ProductId.ToLower() == productNbr.ToLower()).ToList();

                if (filtered != null && filtered.Any() && filtered.FirstOrDefault() != null)
                    sysProductId = filtered.FirstOrDefault().ExternalProductId;
                else
                {
                    var tryAgain = GetExternalInvoiceProductByProductNr(entities, productNbr, actorCompanyId, ref sysWholesellerId);
                    if (tryAgain != null)
                    {
                        sysProductId = tryAgain.ProductId;
                        sysPriceListHeadId = tryAgain.SysPriceListHeadId;
                    }
                }
            }

            if (sysProductId != 0)
            {
                if (sysPriceListHeadId == 0)
                {
                    // TODO: Hardcoded workarounds! Fix this with SysWholeSellerGroups
                    sysPriceListHeadId = WholeSellerManager.GetMostRecentCompanyWholesellerPriceListEx(entities, actorCompanyId, ref sysWholesellerId);
                }

                invoiceProduct = CopyExternalInvoiceProductFromSys(entities, sysProductId, purchasePrice, salesPrice, productUnit, 0, sysPriceListHeadId, sysWholesellerName, actorCustomerId, actorCompanyId, saveChanges);
            }

            return invoiceProduct;
        }

        public InvoiceProduct CopyExternalInvoiceProduct(int productId, decimal purchasePrice, decimal salesPrice, string productUnit, int priceListTypeId, int priceListHeadId, string sysWholesellerName, int actorCustomerId, int actorCompanyId, bool saveChanges, PriceListOrigin origin)
        {
            using (CompEntities entities = new CompEntities())
            {
                return CopyExternalInvoiceProduct(entities, productId, purchasePrice, salesPrice, productUnit, priceListTypeId, priceListHeadId, sysWholesellerName, actorCustomerId, actorCompanyId, saveChanges, origin);
            }
        }

        public InvoiceProduct CopyExternalInvoiceProduct(int productId, decimal purchasePrice, decimal salesPrice, string productUnit, int priceListTypeId, int priceListHeadId, string sysWholesellerName, int actorCustomerId, int actorCompanyId, bool saveChanges, PriceListOrigin origin, out decimal outPurchasePrice, out decimal outSalesPrice)
        {
            using (CompEntities entities = new CompEntities())
            {
                return CopyExternalInvoiceProduct(entities, productId, purchasePrice, salesPrice, productUnit, priceListTypeId, priceListHeadId, sysWholesellerName, actorCustomerId, actorCompanyId, saveChanges, origin, out outPurchasePrice, out outSalesPrice);
            }
        }

        public InvoiceProduct CopyExternalInvoiceProduct(CompEntities entities, int productId, decimal purchasePrice, decimal salesPrice, string productUnitString, int priceListTypeId, int priceListHeadId, string sysWholesellerName, int actorCustomerId, int actorCompanyId, bool saveChanges, PriceListOrigin origin)
        {
            return CopyExternalInvoiceProduct(entities, productId, purchasePrice, salesPrice, productUnitString, priceListTypeId, priceListHeadId, sysWholesellerName, actorCustomerId, actorCompanyId, saveChanges, origin, out _, out _);
        }

        public InvoiceProduct CopyExternalInvoiceProduct(CompEntities entities, int productId, decimal purchasePrice, decimal salesPrice, string productUnitString, int priceListTypeId, int priceListHeadId, string sysWholesellerName, int actorCustomerId, int actorCompanyId, bool saveChanges, PriceListOrigin origin, out decimal outPurchasePrice, out decimal outSalesPrice)
        {
            #region Prereq
            outPurchasePrice = purchasePrice;
            outSalesPrice = salesPrice;
            // Get price list type from customer or company setting if not specified
            if (priceListTypeId == 0)
                priceListTypeId = CustomerManager.GetCustomerPriceListTypeId(entities, actorCustomerId, actorCompanyId);

            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return null;

            if (origin == PriceListOrigin.CompDbNetPriceList)
                origin = PriceListOrigin.SysDbPriceList;

            #endregion

            #region InvoiceProduct

            // Check if product already exists
            InvoiceProduct invoiceProduct = GetInvoiceProduct(entities, productId, actorCompanyId, origin);

            //Try to find the unit
            ProductUnit productUnit = null;
            if (!string.IsNullOrEmpty(productUnitString))
            {
                productUnit = GetProductUnit(entities, productUnitString, actorCompanyId);
                if (productUnit == null)
                {
                    //Insert product unit since it doesn't exist
                    productUnit = new ProductUnit
                    {
                        Code = productUnitString,
                        Name = productUnitString,
                    };

                    AddProductUnit(entities, productUnit, actorCompanyId);
                }
            }

            #region Validate

            if (invoiceProduct != null)
            {
                // Validate productnr with existing product
                string productNr;
                if (invoiceProduct.PriceListOrigin == (int)PriceListOrigin.CompDbPriceList)
                {
                    productNr = (from entry in entities.ProductImported
                                 where entry.ProductImportedId == productId &&
                                 entry.PriceListImported.Any(p => p.PriceListImportedHead.ActorCompanyId == actorCompanyId)
                                 select entry.ProductId).FirstOrDefault();
                }
                else
                {
                    productNr = SysPriceListManager.GetSysProduct(productId)?.ProductId;
                }

                if (productNr != invoiceProduct.Number)
                    invoiceProduct = null;
            }

            #endregion

            PriceList priceList = null;

            if (invoiceProduct != null)
            {
                #region Update

                if (productUnit != null && productUnit.ProductUnitId != invoiceProduct.ProductUnitId)
                {
                    ConvertPrice(entities, actorCompanyId, invoiceProduct.ProductId, productUnit, purchasePrice, salesPrice, out outPurchasePrice, out outSalesPrice);
                    purchasePrice = outPurchasePrice;
                    salesPrice = outSalesPrice;
                }

                // Update product
                invoiceProduct.PurchasePrice = NumberUtility.GetFormattedDecimalValue(purchasePrice, 2);
                invoiceProduct.SysWholesellerName = sysWholesellerName;
                invoiceProduct.ExternalProductId = productId;

                if (invoiceProduct.ProductUnit == null)
                {
                    invoiceProduct.ProductUnit = productUnit;
                }

                if (priceListHeadId != 0)
                    invoiceProduct.ExternalPriceListHeadId = priceListHeadId;

                // Check if customers has a price list of specified type
                priceList = invoiceProduct.PriceList.FirstOrDefault(p => p.PriceListTypeId == priceListTypeId);
                invoiceProduct.PriceListOrigin = (int)origin;

                SaveChanges(entities);

                #endregion
            }
            else
            {
                #region Add

                if (origin == PriceListOrigin.CompDbPriceList)
                {
                    #region Copy CompProduct

                    var compProduct = (from entry in entities.ProductImported
                                       where entry.ProductImportedId == productId &&
                                       entry.PriceListImported.Any(p => p.PriceListImportedHead.ActorCompanyId == actorCompanyId)
                                       select entry).FirstOrDefault();

                    // Create new InvoiceProduct
                    invoiceProduct = new InvoiceProduct
                    {
                        Type = (int)SoeProductType.InvoiceProduct,
                        VatType = (int)TermGroup_InvoiceProductVatType.Merchandise,
                        Name = compProduct.Name,
                        Number = compProduct.ProductId,
                        EAN = compProduct.EAN,
                        ExternalProductId = compProduct.ProductImportedId,
                        PurchasePrice = NumberUtility.GetFormattedDecimalValue(purchasePrice, 2),
                        SysWholesellerName = sysWholesellerName,
                        AccountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0",
                        PriceListOrigin = (int)PriceListOrigin.CompDbPriceList,

                        //Set FK
                        ExternalPriceListHeadId = priceListHeadId,

                        //Set references
                        ProductUnit = productUnit,
                    };

                    #endregion
                }
                else
                {
                    #region Copy SysProduct

                    // Copy Sys product to Comp and connect it to current company
                    if (invoiceProduct == null)
                    {
                        var sysProduct = SysPriceListManager.GetSysProduct(productId);
                        if (sysProduct == null)
                            return null;

                        // Create new InvoiceProduct
                        invoiceProduct = new InvoiceProduct()
                        {
                            Type = (int)SoeProductType.InvoiceProduct,
                            VatType = (int)TermGroup_InvoiceProductVatType.Merchandise,
                            Name = sysProduct.Name,
                            Number = sysProduct.ProductId,
                            EAN = sysProduct.EAN,
                            ExternalProductId = sysProduct.SysProductId,
                            PurchasePrice = NumberUtility.GetFormattedDecimalValue(purchasePrice, 2),
                            SysWholesellerName = sysWholesellerName,
                            AccountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0",
                            PriceListOrigin = (int)PriceListOrigin.SysDbPriceList,

                            //Set FK
                            ExternalPriceListHeadId = priceListHeadId,

                            //Set references
                            ProductUnit = productUnit,
                        };
                    }

                    #endregion
                }

                SetCreatedProperties(invoiceProduct);
                entities.Product.AddObject(invoiceProduct);

                // Map InvoiceProduct to Company
                invoiceProduct.Company.Add(company);

                #endregion
            }

            #endregion

            #region PriceList

            if (priceList == null)
            {
                #region Add

                if (priceListTypeId > 0)
                {
                    var priceListType = ProductPricelistManager.GetPriceListType(entities, priceListTypeId, actorCompanyId);
                    if (priceListType != null)
                    {
                        priceList = new PriceList()
                        {
                            Price = NumberUtility.GetFormattedDecimalValue(salesPrice, 2),
                            DiscountPercent = 0, // Not used

                            //Set references
                            Product = invoiceProduct,
                            PriceListType = priceListType,
                            StartDate = new DateTime(1901, 1, 1),
                            StopDate = new DateTime(9999, 1, 1),
                        };
                        entities.PriceList.AddObject(priceList);

                        // Map InvoiceProduct to PriceList
                        invoiceProduct.PriceList.Add(priceList);
                    }
                }

                #endregion
            }
            else
            {
                #region Update

                // Update sales price on existing price list
                priceList.Price = NumberUtility.GetFormattedDecimalValue(salesPrice, 2);

                #endregion
            }

            #endregion

            if (saveChanges)
                SaveChanges(entities);

            return invoiceProduct;
        }

        public InvoiceProduct CopyExternalInvoiceProductFromSys(CompEntities entities, int sysProductId, decimal purchasePrice, decimal salesPrice, string productUnit, int priceListTypeId, int sysPriceListHeadId, string sysWholesellerName, int actorCustomerId, int actorCompanyId, bool saveChanges)
        {
            return CopyExternalInvoiceProduct(entities, sysProductId, purchasePrice, salesPrice, productUnit, priceListTypeId, sysPriceListHeadId, sysWholesellerName, actorCustomerId, actorCompanyId, saveChanges, PriceListOrigin.SysDbPriceList);
        }

        #endregion

        #region InvoiceProductStatistics

        public List<InvoiceProductStatisticsDTO> GetInvoiceProductStatistics(CompEntities entities, int actorCompanyId, DateTime from, DateTime to)
        {
            return entities.CustomerInvoiceRow
                .Where(r => r.CustomerInvoice.Origin.ActorCompanyId == actorCompanyId)
                .Where(r => r.State == (int)SoeEntityState.Active && r.CustomerInvoice.State == (int)SoeEntityState.Active)
                .Where(r => r.CustomerInvoice.InvoiceDate > from && r.CustomerInvoice.InvoiceDate < to)
                .GroupBy(r => r.ProductId)
                .Select(g => new InvoiceProductStatisticsDTO { ProductId = g.Key ?? 0, SalesQuantity = g.Sum(s => s.Quantity) ?? 0, SalesAmount = g.Sum(s => s.Amount) })
                .ToList();
        }

        #endregion

        #region PayrollProduct

        public List<PayrollProduct> GetPayrollProducts(int actorCompanyId, bool? active, bool loadAccounts = false, bool loadPriceTypesAndPriceFormulas = false, bool loadPayrollProductSettingAccounts = false, bool setSysPayrollTypeLevelNames = false, bool setResultTypeText = false, bool checkValidForAddedTransactionDialog = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetPayrollProducts(entities, actorCompanyId, active, loadAccounts, loadPriceTypesAndPriceFormulas, loadPayrollProductSettingAccounts, setSysPayrollTypeLevelNames, setResultTypeText, checkValidForAddedTransactionDialog);
        }

        public List<PayrollProduct> GetPayrollProducts(CompEntities entities, int actorCompanyId, bool? active, bool loadAccounts = false, bool loadPriceTypesAndPriceFormulas = false, bool loadPayrollProductSettingAccounts = false, bool setSysPayrollTypeLevelNames = false, bool setResultTypeText = false, bool checkValidForAddedTransactionDialog = false)
        {
            IQueryable<PayrollProduct> query = entities.Product.OfType<PayrollProduct>();
            if (loadAccounts)
            {
                query = query.Include("ProductAccountStd.AccountStd");
            }
            if (loadPriceTypesAndPriceFormulas)
            {
                query = query.Include("PayrollProductSetting.PayrollProductPriceType.PayrollProductPriceTypePeriod");
                query = query.Include("PayrollProductSetting.PayrollProductPriceFormula");
            }
            if (loadPayrollProductSettingAccounts)
            {
                query = query.Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal");
            }

            var productQuery = (from p in query
                                where p.State != (int)SoeEntityState.Deleted &&
                                p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                select p).OfType<PayrollProduct>();

            if (active == true)
                productQuery = productQuery.Where(p => p.State == (int)SoeEntityState.Active);
            else if (active == false)
                productQuery = productQuery.Where(p => p.State == (int)SoeEntityState.Inactive);

            List<PayrollProduct> products = productQuery.ToList();

            if (setSysPayrollTypeLevelNames)
            {
                // If any products are actually accumulators, load all accumulators to be able to set its names
                List<TimeAccumulator> accumulators = new List<TimeAccumulator>();
                if (products.Any(p => p.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator))
                    accumulators = TimeAccumulatorManager.GetTimeAccumulators(actorCompanyId);

                foreach (PayrollProduct product in products)
                {
                    if (product.SysPayrollTypeLevel1.HasValue)
                        product.SysPayrollTypeLevel1Name = GetText(product.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType);
                    if (product.SysPayrollTypeLevel2.HasValue)
                        product.SysPayrollTypeLevel2Name = GetText(product.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType);
                    if (product.SysPayrollTypeLevel3.HasValue)
                    {
                        if (product.SysPayrollTypeLevel2.HasValue && product.SysPayrollTypeLevel2.Value == (int)TermGroup_SysPayrollType.SE_Time_Accumulator)
                        {
                            // Get name of accumulator
                            TimeAccumulator accumulator = accumulators.FirstOrDefault(a => a.TimeAccumulatorId == product.SysPayrollTypeLevel3.Value);
                            if (accumulator != null)
                                product.SysPayrollTypeLevel3Name = accumulator.Name;
                        }
                        else
                            product.SysPayrollTypeLevel3Name = GetText(product.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType);
                    }
                    if (product.SysPayrollTypeLevel4.HasValue)
                        product.SysPayrollTypeLevel4Name = GetText(product.SysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType);
                }
            }

            if (setResultTypeText)
            {
                List<GenericType> resultTypes = GetTermGroupContent(TermGroup.PayrollPriceFormulaResultType);
                foreach (PayrollProduct product in products)
                {
                    product.ResultTypeText = resultTypes.FirstOrDefault(r => r.Id == product.ResultType)?.Name ?? String.Empty;
                }
            }

            if (checkValidForAddedTransactionDialog)
                products = products.Where(p => p.IsValidForAddedTransactionDialog()).ToList();

            return products.OrderBy(p => p.NumberSort).ToList();
        }

        public List<PayrollProduct> GetPayrollProducts(List<int> productIds, bool loadSettings = false, bool loadAccounts = false, bool loadPriceTypes = false, bool loadPriceFormulas = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetPayrollProducts(entities, productIds, loadSettings, loadAccounts, loadPriceTypes, loadPriceFormulas);
        }

        public List<PayrollProduct> GetPayrollProducts(CompEntities entities, List<int> productIds, bool loadSettings = false, bool loadAccounts = false, bool loadPriceTypes = false, bool loadPriceFormulas = false)
        {
            IQueryable<PayrollProduct> query = entities.Product.OfType<PayrollProduct>();
            if (loadSettings)
                query = query.Include("PayrollProductSetting.PayrollGroup");
            if (loadAccounts)
                query = query.Include("PayrollProductSetting.PayrollProductAccountStd.AccountStd.Account").Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal.Account.AccountDim");
            if (loadPriceTypes)
                query = query.Include("PayrollProductSetting.PayrollProductPriceType.PayrollProductPriceTypePeriod").Include("PayrollProductSetting.PayrollProductPriceType.PayrollPriceType");
            if (loadPriceFormulas)
                query = query.Include("PayrollProductSetting.PayrollProductPriceFormula.PayrollPriceFormula");

            return (from p in query
                    where productIds.Contains(p.ProductId)
                    select p).ToList();
        }

        public List<PayrollProduct> GetPayrollProductsWithSettings(CompEntities entities, int actorCompanyId, bool? active)
        {
            IQueryable<PayrollProduct> query = entities.Product.OfType<PayrollProduct>();
            query = query.Include("PayrollProductSetting");

            var productQuery = (from p in query
                                where p.State != (int)SoeEntityState.Deleted &&
                                p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                select p).OfType<PayrollProduct>();

            if (active == true)
                productQuery = productQuery.Where(p => p.State == (int)SoeEntityState.Active);
            else if (active == false)
                productQuery = productQuery.Where(p => p.State == (int)SoeEntityState.Inactive);

            List<PayrollProduct> products = productQuery.ToList();
            return products;
        }

        public List<PayrollProduct> GetPayrollProductsIgnoreState(CompEntities entities, int actorCompanyId, bool includeSettings = false)
        {
            IQueryable<PayrollProduct> query = entities.Product.OfType<PayrollProduct>();
            if (includeSettings)
                query = query.Include("PayrollProductSetting");

            var products = (from p in query
                                where p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                select p).OfType<PayrollProduct>().ToList();
    
            return products;
        }

        public List<PayrollProduct> GetPayrollProductsBySearch(int actorCompanyId, string search, int no)
        {
            //new funktion for the quicksearch in time module 
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            IEnumerable<PayrollProduct> products = (from p in entities.Product
                                                    where (p.State != (int)SoeEntityState.Deleted) &&
                                                    (p.Number.ToLower().Contains(search.ToLower()) || p.Name.ToLower().Contains(search.ToLower())) &&
                                                    p.Company.Any(c => c.ActorCompanyId == actorCompanyId)
                                                    select p).OfType<PayrollProduct>().Take(no).ToList();

            return products.OrderBy(p => p.NumberSort).ToList();
        }

        public Dictionary<int, string> GetPayrollProductsDict(int actorCompanyId, bool addEmptyRow, bool concatNumberAndName = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<PayrollProduct> products = GetPayrollProducts(actorCompanyId, active: true);
            foreach (var product in products)
            {
                dict.Add(product.ProductId, concatNumberAndName ? product.Number + " " + product.Name : product.Name);
            }

            return dict.Sort();
        }

        public Dictionary<int, string> GetSelectableChildPayrollProducts(int actorCompanyId, int? excludeProductId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetSelectableChildPayrollProducts(entities, actorCompanyId, excludeProductId);
        }

        public Dictionary<int, string> GetSelectableChildPayrollProducts(CompEntities entities, int actorCompanyId, int? excludeProductId = null)
        {
            var products = (from p in entities.Product
                            where p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                            p.State == (int)SoeEntityState.Active
                            select p).OfType<PayrollProduct>().ToList();

            products = products.FilterSelectableChildPayrollProducts().OrderBy(x => x.Number).ToList();

            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(0, "");
            foreach (PayrollProduct product in products)
            {
                if (!excludeProductId.HasValue || excludeProductId.Value != product.ProductId)
                    dict.Add(product.ProductId, product.Number + " " + product.Name);
            }

            return dict;
        }

        public List<int> GetPayrollProductIdsByType(int actorCompanyId, int? level1, int? level2, int? level3, int? level4)
        {
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Product.NoTracking();
            IQueryable<PayrollProduct> query = (from pp in entitiesReadOnly.Product.OfType<PayrollProduct>()
                                                where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                                                pp.State == (int)SoeEntityState.Active
                                                select pp);

            if (!level1.IsNullOrEmpty())
                query = query.Where(w => w.SysPayrollTypeLevel1 == level1);

            if (!level2.IsNullOrEmpty())
                query = query.Where(w => w.SysPayrollTypeLevel2 == level2);

            if (!level3.IsNullOrEmpty())
                query = query.Where(w => w.SysPayrollTypeLevel3 == level3);

            if (!level4.IsNullOrEmpty())
                query = query.Where(w => w.SysPayrollTypeLevel4 == level4);

            return query.Select(s => s.ProductId).ToList();
        }

        public List<int> GetPayrollProductIdsByType(int actorCompanyId, TermGroup_SysPayrollType level1, TermGroup_SysPayrollType level2)
        {

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from pp in entitiesReadOnly.Product.OfType<PayrollProduct>()
                    where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    pp.SysPayrollTypeLevel1 == (int)level1 &&
                    pp.SysPayrollTypeLevel2 == (int)level2 &&
                    pp.State == (int)SoeEntityState.Active
                    select pp.ProductId).ToList<int>();
        }

        public List<int> GetPayrollProductIdsByType(int actorCompanyId, TermGroup_SysPayrollType level1, TermGroup_SysPayrollType level2, TermGroup_SysPayrollType level3)
        {

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from pp in entitiesReadOnly.Product.OfType<PayrollProduct>()
                    where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    pp.SysPayrollTypeLevel1 == (int)level1 &&
                    pp.SysPayrollTypeLevel2 == (int)level2 &&
                    pp.SysPayrollTypeLevel3 == (int)level3 &&
                    pp.State == (int)SoeEntityState.Active
                    select pp.ProductId).ToList<int>();
        }

        public List<int> GetPayrollProductIdsByType(int actorCompanyId, TermGroup_SysPayrollType level1, TermGroup_SysPayrollType level2, TermGroup_SysPayrollType level3, TermGroup_SysPayrollType level4)
        {

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from pp in entitiesReadOnly.Product.OfType<PayrollProduct>()
                    where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    pp.SysPayrollTypeLevel1 == (int)level1 &&
                    pp.SysPayrollTypeLevel2 == (int)level2 &&
                    pp.SysPayrollTypeLevel3 == (int)level3 &&
                    pp.SysPayrollTypeLevel3 == (int)level4 &&
                    pp.State == (int)SoeEntityState.Active
                    select pp.ProductId).ToList<int>();
        }

        public PayrollProduct GetPayrollProduct(int productId, bool loadSettings = false, bool loadAccounts = false, bool loadPriceTypes = false, bool loadPriceFormulas = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetPayrollProduct(entities, productId, loadSettings, loadAccounts, loadPriceTypes, loadPriceFormulas);
        }

        public PayrollProduct GetPayrollProduct(CompEntities entities, int productId, bool loadSettings = false, bool loadAccounts = false, bool loadPriceTypes = false, bool loadPriceFormulas = false)
        {
            if (productId == 0)
                return null;

            IQueryable<PayrollProduct> query = entities.Product.OfType<PayrollProduct>();
            if (loadSettings)
                query = query.Include("PayrollProductSetting.PayrollGroup");
            if (loadAccounts)
                query = query.Include("PayrollProductSetting.PayrollProductAccountStd.AccountStd.Account").Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal.Account.AccountDim");
            if (loadPriceTypes)
                query = query.Include("PayrollProductSetting.PayrollProductPriceType.PayrollProductPriceTypePeriod").Include("PayrollProductSetting.PayrollProductPriceType.PayrollPriceType");
            if (loadPriceFormulas)
                query = query.Include("PayrollProductSetting.PayrollProductPriceFormula.PayrollPriceFormula");

            PayrollProduct payrollProduct = (from p in query
                                             where p.ProductId == productId
                                             select p).FirstOrDefault();

            return payrollProduct;
        }

        public PayrollProduct GetPayrollProductByNumber(string number, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Product.NoTracking();
            return GetPayrollProductByNumber(entities, number, actorCompanyId);
        }

        public PayrollProduct GetPayrollProductByNumber(CompEntities entities, string number, int actorCompanyId)
        {
            return (from pp in entities.Product.OfType<PayrollProduct>()
                    where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    pp.Number == number &&
                    pp.State == (int)SoeEntityState.Active
                    select pp).FirstOrDefault();
        }

        public List<PayrollProductSettingLookupDTO> GetPayrollProductSettingsFromSysLookup(CompEntities entities, int actorCompanyId, SysExtraFieldType sysExtraFieldType, List<SysExtraField> sysExtraFields = null)
        {
            List<ExtraField> extraFields = ExtraFieldManager.GetExtraFieldsAndRecordsForSysType(entities, SoeEntityType.PayrollProductSetting, sysExtraFieldType, sysExtraFields);

            List<ExtraFieldRecord> extraFieldRecords = new List<ExtraFieldRecord>();
            foreach (ExtraField extraField in extraFields)
            {
                var extraFieldRecord = extraField.ExtraFieldRecord?.Where(er => er.State == (int)SoeEntityState.Active).ToList();
                if (!extraFieldRecord.IsNullOrEmpty())
                    extraFieldRecords.AddRange(extraFieldRecord);
            }

            List<PayrollProductSetting> payrollProductSettings = ProductManager.GetPayrollProductsSettings(entities, actorCompanyId);

            // filter payroll products settings and only select those that PayrollProductSettingId is equal to RecordId on any item in extraFieldRecord collection
            var query = from pps in payrollProductSettings
                        join efr in extraFieldRecords on pps.PayrollProductSettingId equals efr.RecordId
                        select new PayrollProductSettingLookupDTO
                        {
                            PayrollProductId = pps.ProductId,
                            PayrollGroupId = pps.PayrollGroupId,
                            Type = (SettingDataType)efr.DataTypeId,
                            Value = ExtraFieldManager.GetExtraFieldRecordValueAsString(efr),
                            SysExtraFieldType = sysExtraFieldType,
                            Entity = SoeEntityType.PayrollProductSetting
                        };

            return query.ToList();
        }

        public PayrollProductSettingLookupDTO GetPayrollProductInLookup(int payrollProductId, int payrollGroupId, SysExtraFieldType sysExtraFieldType, List<PayrollProductSettingLookupDTO> lookup)
        {
            return lookup.FirstOrDefault(l => l.PayrollProductId == payrollProductId && l.PayrollGroupId == payrollGroupId && l.Entity == SoeEntityType.PayrollProductSetting && l.SysExtraFieldType == sysExtraFieldType);
        }

        public bool PayrollProductExists(CompEntities entities, string number, int actorCompanyId)
        {
            return (from p in entities.Product.OfType<PayrollProduct>()
                    where p.Number == number &&
                    p.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    p.State != (int)SoeEntityState.Deleted
                    select p).Count() > 0;
        }

        public bool PayrollProductWithSameTypeExists(CompEntities entities, PayrollProductDTO payrollProduct, int actorCompanyId, TermGroup_SysPayrollType level1 = TermGroup_SysPayrollType.None, TermGroup_SysPayrollType level2 = TermGroup_SysPayrollType.None, TermGroup_SysPayrollType level3 = TermGroup_SysPayrollType.None, TermGroup_SysPayrollType level4 = TermGroup_SysPayrollType.None)
        {
            if (payrollProduct == null)
                return false;

            if (level1 != TermGroup_SysPayrollType.None)
            {
                return (from pp in entities.Product.OfType<PayrollProduct>()
                        where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                        (pp.ProductId != payrollProduct.ProductId) &&
                        (pp.SysPayrollTypeLevel1 == (int)level1)
                        select pp).Any();
            }
            else if (level2 != TermGroup_SysPayrollType.None)
            {
                return (from pp in entities.Product.OfType<PayrollProduct>()
                        where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                        (pp.ProductId != payrollProduct.ProductId) &&
                        (pp.SysPayrollTypeLevel2 == (int)level2)
                        select pp).Any();
            }
            else if (level3 != TermGroup_SysPayrollType.None)
            {
                return (from pp in entities.Product.OfType<PayrollProduct>()
                        where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                        (pp.ProductId != payrollProduct.ProductId) &&
                        (pp.SysPayrollTypeLevel3 == (int)level3)
                        select pp).Any();
            }
            else if (level4 != TermGroup_SysPayrollType.None)
            {
                return (from pp in entities.Product.OfType<PayrollProduct>()
                        where pp.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                        (pp.ProductId != payrollProduct.ProductId) &&
                        (pp.SysPayrollTypeLevel4 == (int)level4)
                        select pp).Any();
            }

            return false;
        }

        public ActionResult SavePayrollProduct(PayrollProductDTO payrollProductInput, int actorCompanyId)
        {
            if (payrollProductInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(91923, "Löneart hittades inte"));

            ActionResult result = new ActionResult();
            int productId = payrollProductInput.ProductId;

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        Company company = CompanyManager.GetCompany(entities, actorCompanyId);
                        if (company == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                        PayrollProduct payrollProduct = productId > 0 ? GetPayrollProduct(entities, productId, true, true, true, true) : null;

                        result = IsPayrollProductDuplicate(entities, actorCompanyId, payrollProduct, payrollProductInput);
                        if (!result.Success)
                            return result;

                        #endregion

                        #region PayrollProduct                      

                        if (payrollProduct == null)
                        {
                            payrollProduct = new PayrollProduct();
                            SetCreatedProperties(payrollProduct);
                            entities.Product.AddObject(payrollProduct);
                            payrollProduct.Company.Add(company);
                        }
                        else
                            SetModifiedProperties(payrollProduct);

                        payrollProduct.SetProperties(payrollProductInput);

                        #endregion

                        #region PayrollProductSetting

                        result = SavePayrollProductSettings(entities, transaction, payrollProductInput, payrollProduct);
                        if (!result.Success)
                            return result;

                        #region ExtraFields

                        foreach (PayrollProductSettingDTO setting in payrollProductInput.Settings)
                        {
                            if (setting.ExtraFields == null || !setting.ExtraFields.Any())
                                continue;

                            result = ExtraFieldManager.SaveExtraFieldRecords(entities, setting.ExtraFields, (int)SoeEntityType.PayrollProductSetting, setting.PayrollProductSettingId, base.ActorCompanyId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                return result;
                            }
                        }

                        #endregion

                        payrollProduct.ValidateTimeUnitForSettings();

                        #endregion

                        #region PriceTypes, formulas and accounts

                        if (payrollProduct.PayrollProductSetting != null)
                        {
                            foreach (PayrollProductSetting setting in payrollProduct.PayrollProductSetting.Where(s => s.State == (int)SoeEntityState.Active).ToList())
                            {
                                PayrollProductSettingDTO settingInput = payrollProductInput.Settings.FirstOrDefault(s => s.PayrollProductSettingId == setting.PayrollProductSettingId);

                                #region PayrollProductPriceTypes

                                #region Update/Delete

                                if (setting.PayrollProductPriceType != null)
                                {
                                    foreach (PayrollProductPriceType priceType in setting.PayrollProductPriceType.Where(p => p.State == (int)SoeEntityState.Active).ToList())
                                    {
                                        PayrollProductPriceTypeDTO priceTypeInput = settingInput?.PriceTypes.FirstOrDefault(p => p.PayrollProductPriceTypeId == priceType.PayrollProductPriceTypeId);
                                        if (priceTypeInput != null)
                                        {
                                            #region PayrollProductPriceType

                                            if (priceType.PayrollPriceTypeId != priceTypeInput.PayrollPriceTypeId)
                                            {
                                                priceType.PayrollPriceTypeId = priceTypeInput.PayrollPriceTypeId;
                                                SetModifiedProperties(priceType);
                                            }

                                            #endregion

                                            #region PayrollProductPriceTypePeriod

                                            foreach (PayrollProductPriceTypePeriod period in priceType.PayrollProductPriceTypePeriod.Where(p => p.State == (int)SoeEntityState.Active).ToList())
                                            {
                                                PayrollProductPriceTypePeriodDTO periodInput = priceTypeInput.Periods.FirstOrDefault(p => p.PayrollProductPriceTypePeriodId == period.PayrollProductPriceTypePeriodId);
                                                if (periodInput != null)
                                                {
                                                    #region Update PayrollProductPriceTypePeriod

                                                    if (period.FromDate != periodInput.FromDate || period.Amount != periodInput.Amount)
                                                    {
                                                        period.FromDate = periodInput.FromDate;
                                                        period.Amount = periodInput.Amount;
                                                        SetModifiedProperties(period);
                                                    }

                                                    // Remove from input to prevent adding it again below
                                                    priceTypeInput.Periods.Remove(periodInput);

                                                    #endregion
                                                }
                                                else
                                                {
                                                    #region Delete PayrollProductPriceTypePeriod

                                                    period.State = (int)SoeEntityState.Deleted;
                                                    SetModifiedProperties(period);

                                                    #endregion
                                                }
                                            }

                                            #region Add PayrollProductPriceTypePeriod

                                            foreach (PayrollProductPriceTypePeriodDTO periodInput in priceTypeInput.Periods)
                                            {
                                                PayrollProductPriceTypePeriod period = new PayrollProductPriceTypePeriod()
                                                {
                                                    FromDate = periodInput.FromDate,
                                                    Amount = periodInput.Amount
                                                };
                                                SetCreatedProperties(period);
                                                priceType.PayrollProductPriceTypePeriod.Add(period);
                                            }

                                            #endregion

                                            #endregion
                                        }
                                        else
                                        {
                                            #region Delete PayrollProductPriceType and PayrollProductPriceTypePeriods

                                            priceType.State = (int)SoeEntityState.Deleted;
                                            SetModifiedProperties(priceType);

                                            foreach (PayrollProductPriceTypePeriod period in priceType.PayrollProductPriceTypePeriod.Where(p => p.State == (int)SoeEntityState.Active))
                                            {
                                                period.State = (int)SoeEntityState.Deleted;
                                                SetModifiedProperties(period);
                                            }

                                            #endregion
                                        }
                                    }
                                }

                                #endregion

                                #region Add

                                if (settingInput.PriceTypes != null)
                                {
                                    foreach (PayrollProductPriceTypeDTO priceTypeInput in settingInput.PriceTypes.Where(p => p.PayrollProductPriceTypeId == 0).ToList())
                                    {
                                        #region PayrollProductPriceType

                                        PayrollProductPriceType priceType = new PayrollProductPriceType()
                                        {
                                            PayrollPriceTypeId = priceTypeInput.PayrollPriceTypeId,
                                        };
                                        SetCreatedProperties(priceType);
                                        setting.PayrollProductPriceType.Add(priceType);

                                        #endregion

                                        #region PayrollProductPriceTypePeriod

                                        if (priceTypeInput.Periods != null)
                                        {
                                            foreach (PayrollProductPriceTypePeriodDTO periodInput in priceTypeInput.Periods)
                                            {
                                                PayrollProductPriceTypePeriod period = new PayrollProductPriceTypePeriod()
                                                {
                                                    FromDate = periodInput.FromDate,
                                                    Amount = periodInput.Amount
                                                };
                                                SetCreatedProperties(period);
                                                priceType.PayrollProductPriceTypePeriod.Add(period);
                                            }
                                        }

                                        #endregion
                                    }
                                }

                                #endregion

                                #endregion

                                #region PayrollProductPriceFormulas

                                #region Update/Delete

                                if (setting.PayrollProductPriceFormula != null)
                                {
                                    foreach (PayrollProductPriceFormula priceFormula in setting.PayrollProductPriceFormula.Where(p => p.State == (int)SoeEntityState.Active).ToList())
                                    {
                                        PayrollProductPriceFormulaDTO priceFormulaInput = settingInput.PriceFormulas.FirstOrDefault(p => p.PayrollProductPriceFormulaId == priceFormula.PayrollProductPriceFormulaId);
                                        if (priceFormulaInput != null)
                                        {
                                            #region Update

                                            if (priceFormula.PayrollPriceFormulaId != priceFormulaInput.PayrollPriceFormulaId ||
                                                priceFormula.FromDate != priceFormulaInput.FromDate ||
                                                priceFormula.ToDate != priceFormulaInput.ToDate)
                                            {
                                                priceFormula.PayrollPriceFormulaId = priceFormulaInput.PayrollPriceFormulaId;
                                                priceFormula.FromDate = priceFormulaInput.FromDate;
                                                priceFormula.ToDate = priceFormulaInput.ToDate;
                                                SetModifiedProperties(priceFormula);
                                            }

                                            #endregion
                                        }
                                        else
                                        {
                                            #region Delete

                                            priceFormula.State = (int)SoeEntityState.Deleted;
                                            SetModifiedProperties(priceFormula);

                                            #endregion
                                        }
                                    }
                                }

                                #endregion

                                #region Add

                                if (settingInput.PriceFormulas != null)
                                {
                                    foreach (PayrollProductPriceFormulaDTO priceFormulaInput in settingInput.PriceFormulas.Where(p => p.PayrollProductPriceFormulaId == 0).ToList())
                                    {
                                        PayrollProductPriceFormula priceFormula = new PayrollProductPriceFormula()
                                        {
                                            PayrollPriceFormulaId = priceFormulaInput.PayrollPriceFormulaId,
                                            FromDate = priceFormulaInput.FromDate,
                                            ToDate = priceFormulaInput.ToDate
                                        };
                                        SetCreatedProperties(priceFormula);
                                        setting.PayrollProductPriceFormula.Add(priceFormula);
                                    }
                                }

                                #endregion

                                #endregion

                                #region Accounts

                                if (settingInput.AccountSettings != null)
                                {
                                    // Silverlight
                                    result = SavePayrollProductAccounts(entities, transaction, setting, settingInput.AccountSettings, actorCompanyId);
                                    if (!result.Success)
                                        return result;
                                }
                                if (settingInput.AccountingSettings != null)
                                {
                                    // Angular
                                    SaveAccountingSettings(entities, actorCompanyId, setting, settingInput.AccountingSettings);
                                    result = SaveChanges(entities, transaction);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.ProductAccountsNotSaved;
                                        return result;
                                    }
                                }

                                #endregion
                            }
                        }

                        #endregion

                        result = SaveChanges(entities, transaction);
                        if (result.Success)
                        {
                            productId = payrollProduct.ProductId;
                        }
                        else
                        {
                            result.ErrorNumber = (int)ActionResultSave.ProductNotSaved;
                            return result;
                        }

                        #region Check PayrollProductChain

                        PayrollProduct payrollProductWitsettings = GetPayrollProduct(entities, productId, loadSettings: true);
                        if (payrollProductWitsettings != null)
                        {
                            List<int?> payrollGroupIds = payrollProductWitsettings.PayrollProductSetting.Where(x => x.State == (int)SoeEntityState.Active).Select(x => x.PayrollGroupId).ToList();
                            foreach (int? payrollGroupId in payrollGroupIds)
                            {
                                List<PayrollProduct> payrollProductChain = new List<PayrollProduct>();
                                result = GetPayrollProductChain(entities, productId, payrollGroupId, payrollProductChain);
                                if (!result.Success)
                                {
                                    result.ErrorNumber = (int)ActionResultSave.ProductChainWronglyConfigured;
                                    string payrollGroupName = string.Empty;
                                    if (payrollGroupId.HasValue)
                                    {
                                        PayrollGroup payrollGroup = PayrollManager.GetPayrollGroup(entities, payrollGroupId.Value, onlyActive: false);
                                        if (payrollGroup != null)
                                            payrollGroupName = payrollGroup.Name;
                                    }
                                    else
                                    {
                                        payrollGroupName = GetText(8574, "Alla löneavtal");
                                    }

                                    StringBuilder errormessage = new StringBuilder();
                                    errormessage.Append(GetText(8573, "Löneartskedjan är felaktig pga cirkelreferens för") + " " + payrollGroupName + ", ");
                                    int listCount = payrollProductChain.Count;
                                    int counter = 0;
                                    foreach (PayrollProduct product in payrollProductChain)
                                    {
                                        counter++;
                                        errormessage.Append(product.Number + " " + product.Name);
                                        if (listCount != counter)
                                            errormessage.Append("->");
                                    }
                                    result.ErrorMessage = errormessage.ToString();
                                    return result;
                                }
                            }
                        }

                        #endregion

                        #region Save PayrollProductSetting categories

                        // Must be handled after save of PayrollProductSetting, since we need its ID for RecordId
                        if (payrollProduct.PayrollProductSetting != null)
                        {
                            foreach (PayrollProductSetting setting in payrollProduct.PayrollProductSetting.Where(s => s.State == (int)SoeEntityState.Active).ToList())
                            {
                                PayrollProductSettingDTO settingInput = payrollProductInput.Settings.FirstOrDefault(s => s.PayrollProductSettingId == setting.PayrollProductSettingId);
                                if (settingInput == null)
                                    settingInput = payrollProductInput.Settings.FirstOrDefault(s => s.PayrollGroupId == setting.PayrollGroupId);

                                if (settingInput?.CategoryRecords != null)
                                {
                                    result = CategoryManager.SaveCompanyCategoryRecords(entities, transaction, settingInput.CategoryRecords, company.ActorCompanyId, SoeCategoryType.PayrollProduct, SoeCategoryRecordEntity.Product, setting.PayrollProductSettingId);
                                    if (!result.Success)
                                    {
                                        result.ErrorNumber = (int)ActionResultSave.ProductCategoriesNotSaved;
                                        return result;
                                    }
                                }
                            }
                        }

                        #endregion

                        // Commit transaction
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
                        result.IntegerValue = productId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeletePayrollProduct(int productId, int actorCompanyId)
        {
            using (CompEntities entities = new CompEntities())
            {
                PayrollProduct originalPayrollProduct = GetPayrollProduct(entities, productId);
                if (originalPayrollProduct == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(91923, "Löneart hittades inte"));

                // Check that product is not used anywhere
                ActionResult result = IsOkToChangeStateOnProduct(entities, productId, SoeProductType.PayrollProduct, false, actorCompanyId);
                if (!result.Success)
                    return result;

                result = UnMapCompanyFromPayrollProduct(entities, originalPayrollProduct, actorCompanyId);
                if (!result.Success)
                    return result;

                // Set the Product to deleted if no other Companies use it
                if (originalPayrollProduct.Company.Count == 0)
                {
                    result = ChangeEntityState(entities, originalPayrollProduct, SoeEntityState.Deleted, true);
                    if (!result.Success)
                        result.ErrorNumber = (int)ActionResultDelete.ProductNotDeleted;
                }
                else
                    result.Success = false;

                return result;
            }
        }

        public ActionResult UnMapCompanyFromPayrollProduct(CompEntities entities, PayrollProduct payrollProduct, int actorCompanyId)
        {
            if (payrollProduct == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, GetText(91923, "Löneart hittades inte"));

            Company company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            if (!payrollProduct.Company.IsLoaded)
                payrollProduct.Company.Load();
            payrollProduct.Company.Remove(company);

            return SaveEntityItem(entities, payrollProduct);
        }

        public ActionResult GetPayrollProductChain(CompEntities entities, int payrollProducId, int? payrollGroupId, List<PayrollProduct> payrollProductChain)
        {
            var payrollProductWithSettings = GetPayrollProduct(entities, payrollProducId, loadSettings: true);
            if (payrollProductWithSettings != null)
                payrollProductChain.Add(payrollProductWithSettings);
            else
                return new ActionResult(false);

            PayrollProductSetting payrollProductSetting = null;
            if (payrollGroupId.HasValue)
                payrollProductSetting = payrollProductWithSettings.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && i.PayrollGroupId.HasValue && i.PayrollGroupId.Value == payrollGroupId.Value);
            if (payrollProductSetting == null)
                payrollProductSetting = payrollProductWithSettings.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && !i.PayrollGroupId.HasValue);

            if (payrollProductSetting != null && payrollProductSetting.ChildProductId.HasValue)
            {
                if (payrollProductChain.Any(x => x.ProductId == payrollProductSetting.ChildProductId.Value))
                {
                    payrollProductWithSettings = GetPayrollProduct(entities, payrollProductSetting.ChildProductId.Value, loadSettings: true);
                    payrollProductChain.Add(payrollProductWithSettings);//need for the error message
                    //The payrollproduct chain is inifinte, it is wrongly configured!
                    return new ActionResult(false);
                }
                else
                {
                    var result = GetPayrollProductChain(entities, payrollProductSetting.ChildProductId.Value, payrollGroupId, payrollProductChain);
                    if (!result.Success)
                        return result;
                }
            }

            return new ActionResult();
        }

        private ActionResult IsPayrollProductDuplicate(CompEntities entities, int actorCompanyId, PayrollProduct payrollProduct, PayrollProductDTO payrollProductInput)
        {
            #region Check product number

            if ((payrollProduct == null || payrollProduct.Number != payrollProductInput.Number) && PayrollProductExists(entities, payrollProductInput.Number, actorCompanyId))
                return new ActionResult((int)ActionResultSave.ProductExists, GetText(94045, "En löneart med angivet nummer finns redan"));

            #endregion

            #region Check products that can have only one of each type

            bool isDuplicate = false;

            //TableTax
            if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_TableTax)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level2: TermGroup_SysPayrollType.SE_Tax_TableTax);
            //OneTax
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_OneTimeTax)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level2: TermGroup_SysPayrollType.SE_Tax_OneTimeTax);
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_ASINK)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level2: TermGroup_SysPayrollType.SE_Tax_ASINK);
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_SINK)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level2: TermGroup_SysPayrollType.SE_Tax_SINK);
            //EmploymentTax
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level1: TermGroup_SysPayrollType.SE_EmploymentTaxCredit);
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level1: TermGroup_SysPayrollType.SE_EmploymentTaxDebit);
            //SupplementCharge
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level1: TermGroup_SysPayrollType.SE_SupplementChargeCredit);
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level1: TermGroup_SysPayrollType.SE_SupplementChargeDebit);
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_NetSalary && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_NetSalary_Paid)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level2: TermGroup_SysPayrollType.SE_NetSalary_Paid);
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_NetSalary && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_NetSalary_Rounded)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level2: TermGroup_SysPayrollType.SE_NetSalary_Rounded);
            //SalaryDistress
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_SalaryDistress && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Deduction_SalaryDistressAmount)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Deduction_SalaryDistressAmount);
            //VacationCompensation direct paid
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_DirectPaid)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_DirectPaid);
            //Benefit Invert Other
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Other)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_Other);
            //Benefit Invert PropertyNotHouse 
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyNotHouse)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyNotHouse);
            //Benefit Invert PropertyHouse
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyHouse)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyHouse);
            //Benefit Invert Fuel
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Fuel)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_Fuel);
            //Benefit Invert ROT
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_ROT)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_ROT);
            //Check Benefit Invert RUT
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_RUT)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_RUT);
            //Benefit Invert Food
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Food)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_Food);
            //Benefit Invert BorrowedComputer
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_BorrowedComputer)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_BorrowedComputer);
            //Benefit Invert Parking
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Parking)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_Parking);
            //Benefit Invert Interest
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Interest)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_Interest);
            //Benefit Invert CompanyCar 
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_CompanyCar)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_CompanyCar);
            //Benefit Invert Standard
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert && payrollProductInput.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Standard)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level3: TermGroup_SysPayrollType.SE_Benefit_Invert_Standard);
            //Weekendsalary
            else if (payrollProductInput.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary && payrollProductInput.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_WeekendSalary)
                isDuplicate = PayrollProductWithSameTypeExists(entities, payrollProductInput, actorCompanyId, level2: TermGroup_SysPayrollType.SE_GrossSalary_WeekendSalary);

            if (isDuplicate)
                return new ActionResult((int)ActionResultSave.ProductWithSysPayrollTypeCannotBeDuplicate, GetText(8935, "Det finns redan en löneart med samma lönetyp. Bara en löneart med den lönetypen får finnas."));

            #endregion

            return new ActionResult(true);
        }

        public ActionResult ChangePayrollProductStates(Dictionary<int, bool> products)
        {
            using (CompEntities entities = new CompEntities())
            {
                foreach (KeyValuePair<int, bool> product in products)
                {
                    Product originalProduct = GetProduct(entities, product.Key, false);
                    if (originalProduct == null)
                        return new ActionResult((int)ActionResultDelete.EntityNotFound, "Product");

                    ChangeEntityState(originalProduct, product.Value ? SoeEntityState.Active : SoeEntityState.Inactive);
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region PayrollProductSetting
        public PayrollProductSetting GetPayrollProductSetting(List<PayrollProductSetting> payrollProductSettings, int payrollProductId, int? payrollGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollProductSetting(entities, payrollProductSettings, payrollProductId, payrollGroupId);
        }
        public PayrollProductSetting GetPayrollProductSetting(CompEntities entities, List<PayrollProductSetting> payrollProductSettings, int payrollProductId, int? payrollGroupId)
        {
            List<PayrollProductSetting> list = (from s in payrollProductSettings
                                                where s.ProductId == payrollProductId &&
                                                s.State == (int)SoeEntityState.Active
                                                select s).ToList();

            List<PayrollProductSetting> filteredList = null;
            if (payrollGroupId.HasValue)
                filteredList = list.Where(s => s.PayrollGroupId == payrollGroupId).ToList();
            else
                filteredList = list.Where(s => !s.PayrollGroupId.HasValue).ToList();

            PayrollProductSetting setting = filteredList.FirstOrDefault();

            if (setting == null)
            {
                filteredList = list.Where(s => !s.PayrollGroupId.HasValue).ToList();
                setting = filteredList.FirstOrDefault();

                if (setting == null)
                {
                    setting = (from s in entities.PayrollProductSetting.Include("PayrollProduct")
                               where s.ProductId == payrollProductId &&
                               !s.PayrollGroupId.HasValue &&
                               s.State == (int)SoeEntityState.Active
                               select s).FirstOrDefault();
                }
            }

            return setting;
        }

        public List<PayrollProductSetting> GetPayrollProductsSettings(CompEntities entities, int actorCompanyId)
        {
            List<PayrollProductSetting> settings = new List<PayrollProductSetting>();

            List<PayrollProduct> products = GetPayrollProducts(entities, actorCompanyId, null, loadPayrollProductSettingAccounts: true);
            foreach (PayrollProduct product in products)
            {
                var productSettings = product.PayrollProductSetting?.Where(ps => ps.State == (int)SoeEntityState.Active).ToList();
                if (!productSettings.IsNullOrEmpty())
                    settings.AddRange(productSettings);
            }

            return settings;
        }

        private ActionResult SavePayrollProductSettings(CompEntities entities, TransactionScope transaction, PayrollProductDTO payrollProductInput, PayrollProduct payrollProduct)
        {
            ActionResult result = new ActionResult(true);

            payrollProductInput.Settings.Where(s => s.PayrollGroupId == 0).ToList().ForEach(s => s.PayrollGroupId = null);

            if (payrollProduct.PayrollProductSetting != null)
            {
                foreach (PayrollProductSetting setting in payrollProduct.PayrollProductSetting.Where(s => s.State == (int)SoeEntityState.Active).ToList())
                {
                    PayrollProductSettingDTO settingInput = payrollProductInput.Settings.FirstOrDefault(s => s.PayrollProductSettingId == setting.PayrollProductSettingId);
                    if (settingInput != null)
                        setting.SetProperties(settingInput);
                    else
                        SetPayrollProductSettingToDeleted(setting);
                    SetModifiedProperties(setting);
                }
            }

            foreach (PayrollProductSettingDTO settingInput in payrollProductInput.Settings.Where(s => s.PayrollProductSettingId == 0).ToList())
            {
                PayrollProductSetting setting = CreatePayrollProductSetting(payrollProduct, settingInput);
                if (setting == null)
                    continue;

                result = SaveChanges(entities, transaction);
                if (!result.Success)
                    return result;

                settingInput.PayrollProductSettingId = setting.PayrollProductSettingId;
            }

            return result;
        }

        public PayrollProductSetting CreatePayrollProductSetting(PayrollProduct payrollProduct, int? payrollGroupId, int? childProductId = null)
        {
            if (payrollProduct == null)
                return null;

            PayrollProductSetting setting = new PayrollProductSetting()
            {
                ProductId = payrollProduct.ProductId,
                ChildProductId = childProductId.ToNullable(),
                PayrollGroupId = payrollGroupId.ToNullable(),
                AccountingPrio = PayrollProductSetting.DEFAULT_ACCOUNTINGPRIO,
            };
            SetCreatedProperties(setting);
            payrollProduct.PayrollProductSetting.Add(setting);
            return setting;
        }

        public PayrollProductSetting CreatePayrollProductSetting(PayrollProduct payrollProduct, IPayrollProductSetting settingInput)
        {
            return CreatePayrollProductSetting(payrollProduct, settingInput, settingInput.PayrollGroupId, settingInput.ChildProductId);
        }

        public PayrollProductSetting CreatePayrollProductSetting(PayrollProduct payrollProduct, IPayrollProductSetting settingInput, int? payrollGroupId, int? childProductId)
        {
            if (payrollProduct == null || settingInput == null)
                return null;

            PayrollProductSetting setting = CreatePayrollProductSetting(payrollProduct, payrollGroupId, childProductId);
            setting?.SetProperties(settingInput, setKeys: false);
            return setting;
        }

        private void SetPayrollProductSettingToDeleted(PayrollProductSetting setting)
        {
            if (setting == null)
                return;

            ChangeEntityState(setting, SoeEntityState.Deleted);
            if (setting.PayrollProductPriceFormula != null)
                setting.PayrollProductPriceFormula.Where(p => p.State != (int)SoeEntityState.Deleted).ToList().ForEach(p => ChangeEntityState(p, SoeEntityState.Deleted));
            if (setting.PayrollProductPriceType != null)
                setting.PayrollProductPriceType.Where(p => p.State != (int)SoeEntityState.Deleted).ToList().ForEach(p => ChangeEntityState(p, SoeEntityState.Deleted));
        }

        #endregion

        #region PayrollProductAccount

        public ActionResult SavePayrollProductAccounts(CompEntities entities, TransactionScope transaction, PayrollProductSetting setting, List<AccountingSettingDTO> accountSettings, int actorCompanyId)
        {
            //NULL meaning dont save
            if (setting == null || accountSettings == null)
                return new ActionResult();

            #region Prereq

            if (!setting.IsAdded() && !setting.PayrollProductAccountStd.IsLoaded)
                setting.PayrollProductAccountStd.Load();

            AccountingSettingDTO accountSettingStd = accountSettings.FirstOrDefault(a => a.DimNr == Constants.ACCOUNTDIM_STANDARD);

            #endregion

            foreach (ProductAccountType accountStdType in EnumUtility.GetPayrollProductAccountTypes())
            {
                #region Prereq

                int accountId = 0;
                if (accountSettingStd != null && accountStdType == ProductAccountType.Purchase)
                    accountId = accountSettingStd.Account1Id;

                #endregion

                PayrollProductAccountStd payrollProductAccountStd = setting.PayrollProductAccountStd.FirstOrDefault(e => e.Type == (int)accountStdType);
                if (payrollProductAccountStd == null)
                {
                    #region Add

                    payrollProductAccountStd = new PayrollProductAccountStd
                    {
                        Type = (int)accountStdType,
                        Percent = null,

                        //Set FK
                        AccountId = accountId.ToNullable(),
                    };

                    // Add AccountInternals
                    AddAccountInternalToPayrollProductAccountStd(entities, payrollProductAccountStd, accountSettings, accountStdType, actorCompanyId);

                    setting.PayrollProductAccountStd.Add(payrollProductAccountStd);

                    #endregion
                }
                else
                {
                    #region Update/Delete

                    if (!payrollProductAccountStd.AccountInternal.IsLoaded)
                        payrollProductAccountStd.AccountInternal.Load();

                    // Always delete AccountInternal (re-add when update)
                    payrollProductAccountStd.AccountInternal.Clear();

                    if (accountSettingStd != null)
                    {
                        #region Update

                        // Update AccountStd
                        payrollProductAccountStd.AccountId = accountId.ToNullable();

                        // Add AccountInternal
                        AddAccountInternalToPayrollProductAccountStd(entities, payrollProductAccountStd, accountSettings, accountStdType, actorCompanyId);

                        #endregion
                    }
                    else
                    {
                        #region Delete

                        // Delete AccountStd
                        payrollProductAccountStd.AccountId = null;

                        setting.PayrollProductAccountStd.Remove(payrollProductAccountStd);
                        entities.DeleteObject(payrollProductAccountStd);

                        #endregion
                    }

                    #endregion
                }
            }

            return SaveChanges(entities, transaction);
        }

        private void AddAccountInternalToPayrollProductAccountStd(CompEntities entities, PayrollProductAccountStd payrollProductAccountStd, List<AccountingSettingDTO> accountSettings, ProductAccountType accountStdType, int actorCompanyId)
        {
            if (payrollProductAccountStd == null || accountSettings == null)
                return;

            foreach (AccountingSettingDTO accountSettingInternal in accountSettings.Where(a => a.DimNr != Constants.ACCOUNTDIM_STANDARD))
            {
                #region Prereq

                int accountId = 0;
                if (accountStdType == ProductAccountType.Purchase)
                    accountId = accountSettingInternal.Account1Id;

                #endregion

                #region AccountInternal

                AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, accountId, actorCompanyId);
                if (accountInternal != null)
                    payrollProductAccountStd.AccountInternal.Add(accountInternal);

                #endregion
            }
        }

        #endregion

        #region PayrollProductPriceFormula

        public List<PayrollProductPriceFormula> GetPayrollProductPriceFormulas(int productId, int? payrollGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.PayrollProductPriceFormula.NoTracking();
            return GetPayrollProductPriceFormulas(entities, productId, payrollGroupId);
        }

        public List<PayrollProductPriceFormula> GetPayrollProductPriceFormulas(CompEntities entities, int productId, int? payrollGroupId)
        {
            var formulas = (from p in entities.PayrollProductPriceFormula.Include("PayrollPriceFormula")
                            where p.PayrollProductSetting.ProductId == productId &&
                            (p.PayrollProductSetting.PayrollGroupId == payrollGroupId || !p.PayrollProductSetting.PayrollGroupId.HasValue) &&
                            p.State == (int)SoeEntityState.Active
                            select p).ToList();

            if (payrollGroupId.HasValue && formulas.Any(a => a.PayrollProductSetting?.PayrollGroupId != null && a.PayrollProductSetting.PayrollGroupId == payrollGroupId))
                formulas = formulas.Where(w => w.PayrollProductSetting?.PayrollGroupId != null && w.PayrollProductSetting.PayrollGroupId == payrollGroupId).ToList();

            return formulas;
        }

        public List<PayrollProductPriceFormula> GetPayrollProductPriceFormulas(CompEntities entities, int actorCompanyId)
        {
            return (from p in entities.PayrollProductPriceFormula
                    .Include("PayrollPriceFormula")
                    .Include("PayrollProductSetting")
                    where p.PayrollProductSetting.PayrollProduct.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        public List<PayrollPriceTypeAndFormulaDTO> GetPayrollPriceTypesAndFormulas(int actorCompanyId)
        {
            string priceTypeLabel = GetText(3145, (int)TermGroup.General, "Lönetyp");
            string priceFormulaLabel = GetText(3146, (int)TermGroup.General, "Löneformel");

            var priceTypeDTOs = from priceType in PayrollManager.GetPayrollPriceTypes(actorCompanyId, null, false)
                                select new PayrollPriceTypeAndFormulaDTO
                                {
                                    ID = priceType.PayrollPriceTypeId,
                                    PayrollPriceTypeId = priceType.PayrollPriceTypeId,
                                    Name = String.Format("{0}: {1}", priceTypeLabel, priceType.Name)
                                };

            var priceFormulaDTOs = from priceFormula in PayrollManager.GetPayrollPriceFormulas(actorCompanyId, false)
                                   select new PayrollPriceTypeAndFormulaDTO
                                   {
                                       ID = priceFormula.PayrollPriceFormulaId,
                                       PayrollPriceFormulaId = priceFormula.PayrollPriceFormulaId,
                                       Name = String.Format("{0}: {1}", priceFormulaLabel, priceFormula.Name)
                                   };

            return priceTypeDTOs.Concat(priceFormulaDTOs).ToList();
        }

        public PayrollProductPriceFormula GetPayrollProductPriceFormula(CompEntities entities, int payrollProductPriceFormulaId)
        {
            return (from p in entities.PayrollProductPriceFormula
                    where p.PayrollProductPriceFormulaId == payrollProductPriceFormulaId
                    select p).FirstOrDefault();
        }

        public PayrollProductPriceFormula GetPayrollProductPriceFormula(CompEntities entities, int productId, int? payrollGroupId, DateTime date)
        {
            IQueryable<PayrollProductPriceFormula> query = (from p in entities.PayrollProductPriceFormula
                                                            where p.PayrollProductSetting.ProductId == productId &&
                                                            (!p.ToDate.HasValue || p.ToDate.Value >= date) &&
                                                            (!p.FromDate.HasValue || p.FromDate <= date) &&
                                                            p.State == (int)SoeEntityState.Active
                                                            select p);

            if (payrollGroupId.HasValue)
                query = query.Where(p => p.PayrollProductSetting.PayrollGroupId == payrollGroupId);
            else
                query = query.Where(p => !p.PayrollProductSetting.PayrollGroupId.HasValue);

            query = query.OrderBy(p => p.FromDate);

            var priceFormulas = query.ToList();

            // Find formula with latest start
            var priceFormula = priceFormulas.Where(i => i.FromDate.HasValue).OrderByDescending(i => i.FromDate).FirstOrDefault();
            if (priceFormula == null)
                priceFormula = priceFormulas.FirstOrDefault(i => !i.FromDate.HasValue);

            // If no formula found for specified payroll group,
            // try find a general one (with no payroll group specified).
            if (priceFormula == null && payrollGroupId.HasValue)
                return GetPayrollProductPriceFormula(entities, productId, null, date);

            return priceFormula;
        }

        public PayrollProductPriceFormula GetPayrollProductPriceFormula(List<PayrollProductPriceFormula> payrollProductPriceFormulas, int productId, int? payrollGroupId, DateTime date)
        {
            var query = (from p in payrollProductPriceFormulas
                         where p.PayrollProductSetting.ProductId == productId &&
                         (!p.ToDate.HasValue || p.ToDate.Value >= date) &&
                         (!p.FromDate.HasValue || p.FromDate <= date) &&
                         p.State == (int)SoeEntityState.Active
                         select p);

            if (payrollGroupId.HasValue)
                query = query.Where(p => p.PayrollProductSetting.PayrollGroupId == payrollGroupId).ToList();
            else
                query = query.Where(p => !p.PayrollProductSetting.PayrollGroupId.HasValue).ToList();

            query = query.OrderBy(p => p.FromDate).ToList();

            var priceFormulas = query.ToList();

            // Find formula with latest start
            var priceFormula = priceFormulas.Where(i => i.FromDate.HasValue).OrderByDescending(i => i.FromDate).FirstOrDefault();
            if (priceFormula == null)
                priceFormula = priceFormulas.FirstOrDefault(i => !i.FromDate.HasValue);

            // If no formula found for specified payroll group,
            // try find a general one (with no payroll group specified).
            if (priceFormula == null && payrollGroupId.HasValue)
                return GetPayrollProductPriceFormula(payrollProductPriceFormulas, productId, null, date);

            return priceFormula;
        }

        #endregion

        #region PayrollProductPriceType

        public List<PayrollProductPriceType> GetPayrollProductPriceTypes(CompEntities entities, int productid, int? payrollGroupId)
        {
            var priceTypes = (from p in entities.PayrollProductPriceType.Include("PayrollProductPriceTypePeriod").Include("PayrollPriceType")
                              where p.PayrollProductSetting.ProductId == productid &&
                              (p.PayrollProductSetting.PayrollGroupId == payrollGroupId || !p.PayrollProductSetting.PayrollGroupId.HasValue) &&
                              p.State == (int)SoeEntityState.Active
                              select p).ToList();

            if (payrollGroupId.HasValue && priceTypes.Any(a => a.PayrollProductSetting?.PayrollGroupId != null && a.PayrollProductSetting.PayrollGroupId == payrollGroupId))
                priceTypes = priceTypes.Where(w => w.PayrollProductSetting?.PayrollGroupId != null && w.PayrollProductSetting.PayrollGroupId == payrollGroupId).ToList();

            return priceTypes;
        }

        public List<PayrollProductPriceType> GetPayrollProductPriceTypes(CompEntities entities, int actorCompanyId)
        {
            return (from p in entities.PayrollProductPriceType
                    .Include("PayrollProductPriceTypePeriod")
                    .Include("PayrollProductSetting")
                    where p.PayrollProductSetting.PayrollProduct.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }

        public PayrollPriceTypePeriod GetPayrollPriceTypePeriod(CompEntities entities, int payrollPriceTypeId, DateTime date)
        {
            return (from p in entities.PayrollPriceTypePeriod
                    where p.PayrollPriceTypeId == payrollPriceTypeId &&
                    (!p.FromDate.HasValue || p.FromDate <= date) &&
                    p.State == (int)SoeEntityState.Active
                    orderby p.FromDate descending
                    select p).FirstOrDefault();
        }

        public PayrollProductPriceType GetPayrollProductPriceType(CompEntities entities, int productId, int? payrollGroupId, int payrollPriceTypeId)
        {
            return (from p in entities.PayrollProductPriceType.Include("PayrollProductPriceTypePeriod")
                    where p.PayrollProductSetting.ProductId == productId &&
                    p.PayrollProductSetting.PayrollGroupId == payrollGroupId &&
                    p.PayrollPriceTypeId == payrollPriceTypeId &&
                    p.State == (int)SoeEntityState.Active
                    select p).FirstOrDefault();
        }

        #endregion

        #region PayrollProductPriceTypePeriod

        public PayrollProductPriceTypePeriod GetPayrollProductPriceTypePeriod(CompEntities entities, int productId, int? payrollGroupId, DateTime date)
        {
            IQueryable<PayrollProductPriceTypePeriod> query = (from p in entities.PayrollProductPriceTypePeriod.Include("PayrollProductPriceType.PayrollProductSetting")
                                                               where p.PayrollProductPriceType.PayrollProductSetting.ProductId == productId &&
                                                               (!p.FromDate.HasValue || p.FromDate <= date) &&
                                                               p.State == (int)SoeEntityState.Active &&
                                                               p.PayrollProductPriceType.State == (int)SoeEntityState.Active &&
                                                               p.PayrollProductPriceType.PayrollProductSetting.State == (int)SoeEntityState.Active
                                                               select p);

            if (payrollGroupId.HasValue)
                query = query.Where(p => p.PayrollProductPriceType.PayrollProductSetting.PayrollGroupId == payrollGroupId);
            else
                query = query.Where(p => !p.PayrollProductPriceType.PayrollProductSetting.PayrollGroupId.HasValue);

            query = query.OrderBy(p => p.FromDate);

            var periods = query.ToList();

            // Find formula with latest start
            var period = periods.Where(i => i.FromDate.HasValue).OrderByDescending(i => i.FromDate).FirstOrDefault();
            if (period == null)
                period = periods.FirstOrDefault(i => !i.FromDate.HasValue);

            // If no price type found for specified payroll group,
            // try find a general one (with no payroll group specified).
            if (period == null && payrollGroupId.HasValue)
                return GetPayrollProductPriceTypePeriod(entities, productId, null, date);

            return period;
        }

        public PayrollProductPriceTypePeriod GetPayrollProductPriceTypePeriod(List<PayrollProductPriceType> payrollProductPriceTypes, int productId, int? payrollGroupId, DateTime date)
        {

            List<PayrollProductPriceTypePeriod> payrollProductPriceTypePeriods = new List<PayrollProductPriceTypePeriod>();

            foreach (var payrollProductPriceType in payrollProductPriceTypes)
                payrollProductPriceTypePeriods.AddRange(payrollProductPriceType.PayrollProductPriceTypePeriod.ToList());


            var query = (from p in payrollProductPriceTypePeriods
                         where p.PayrollProductPriceType.PayrollProductSetting.ProductId == productId &&
                         (!p.FromDate.HasValue || p.FromDate <= date) &&
                         p.State == (int)SoeEntityState.Active &&
                         p.PayrollProductPriceType.State == (int)SoeEntityState.Active &&
                         p.PayrollProductPriceType.PayrollProductSetting.State == (int)SoeEntityState.Active
                         select p).ToList();

            if (payrollGroupId.HasValue)
                query = query.Where(p => p.PayrollProductPriceType.PayrollProductSetting.PayrollGroupId == payrollGroupId).ToList();
            else
                query = query.Where(p => !p.PayrollProductPriceType.PayrollProductSetting.PayrollGroupId.HasValue).ToList();

            query = query.OrderBy(p => p.FromDate).ToList();

            var periods = query.ToList();

            // Find formula with latest start
            var period = periods.Where(i => i.FromDate.HasValue).OrderByDescending(i => i.FromDate).FirstOrDefault();
            if (period == null)
                period = periods.FirstOrDefault(i => !i.FromDate.HasValue);

            // If no price type found for specified payroll group,
            // try find a general one (with no payroll group specified).
            if (period == null && payrollGroupId.HasValue)
                return GetPayrollProductPriceTypePeriod(payrollProductPriceTypes, productId, null, date);

            return period;
        }

        #endregion

        #region PayrollProductReportSetting

        public List<PayrollProductReportSetting> GetPayrollProductReportSettings(CompEntities entities, int productid, int? payrollGroupId)
        {
            var priceTypes = (from p in entities.PayrollProductReportSetting.Include("PayrollProductSetting.PayrollProduct")
                              where p.PayrollProductSetting.ProductId == productid &&
                              (p.PayrollProductSetting.PayrollGroupId == payrollGroupId || !p.PayrollProductSetting.PayrollGroupId.HasValue) &&
                              p.State == (int)SoeEntityState.Active
                              select p).ToList();

            if (payrollGroupId.HasValue && priceTypes.Any(a => a.PayrollProductSetting?.PayrollGroupId != null && a.PayrollProductSetting.PayrollGroupId == payrollGroupId))
                priceTypes = priceTypes.Where(w => w.PayrollProductSetting?.PayrollGroupId != null && w.PayrollProductSetting.PayrollGroupId == payrollGroupId).ToList();

            return priceTypes;
        }

        public List<PayrollProductReportSetting> GetPayrollProductReportSettings(CompEntities entities, int actorCompanyId)
        {
            return (from p in entities.PayrollProductReportSetting
                    .Include("PayrollProductSetting.PayrollProduct")
                    where p.PayrollProductSetting.PayrollProduct.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    p.State == (int)SoeEntityState.Active
                    select p).ToList();
        }
        #endregion

        #region ProductUnit

        public ProductUnit GetProductUnit(string code, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductUnit.NoTracking();
            return GetProductUnit(entities, code, actorCompanyId);
        }

        public ProductUnit GetProductUnit(CompEntities entities, string code, int actorCompanyId)
        {
            return (from pu in entities.ProductUnit
                    where pu.Code.ToLower() == code.ToLower()
                    && pu.Company.ActorCompanyId == actorCompanyId
                    select pu).FirstOrDefault();
        }

        public ProductUnit GetProductUnit(int productUnitId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductUnit.NoTracking();
            return GetProductUnit(entities, productUnitId);
        }

        public ProductUnit GetProductUnit(CompEntities entities, int productUnitId)
        {
            return (from pu in entities.ProductUnit
                    where pu.ProductUnitId == productUnitId
                    select pu).FirstOrDefault();
        }

        public Dictionary<int, string> GetProductUnitsDict(int actorCompanyId)
        {
            var dict = new Dictionary<int, string>();

            var productUnits = GetProductUnits(actorCompanyId);
            foreach (var u in productUnits)
                dict.Add(u.ProductUnitId, u.Name);

            return dict;
        }

        public List<ProductUnit> GetProductUnits(int actorCompanyId, int? productUnitId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductUnit.NoTracking();
            return GetProductUnits(entities, actorCompanyId, productUnitId);
        }

        public List<ProductUnit> GetProductUnits(CompEntities entities, int actorCompanyId, int? productUnitId = null)
        {
            IQueryable<ProductUnit> query = (from pu in entities.ProductUnit
                                             where pu.Company.ActorCompanyId == actorCompanyId
                                             orderby pu.Code
                                             select pu);


            if (productUnitId.HasValue)
                query = query.Where(dc => dc.ProductUnitId == productUnitId);

            return query.ToList();
        }

        public List<ProductUnitConvert> GetProductUnitConverts(int productId, bool addEmptyRow)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductUnit.NoTracking();
            return GetProductUnitConverts(entities, productId, addEmptyRow);
        }

        public List<ProductUnitConvert> GetProductUnitConverts(CompEntities entities, int productId, bool addEmptyRow)
        {
            var result = (from puc in entities.ProductUnitConvert.Include("ProductUnit")
                          where puc.InvoiceProductId == productId
                          select puc).ToList();

            if (addEmptyRow)
                result.Insert(0, new ProductUnitConvert { ProductUnitConvertId = 0 });
            return result;
        }


        public ProductUnitConvert GetProductUnitConvert(CompEntities entities, int productUnitConvertId)
        {
            return (from puc in entities.ProductUnitConvert.Include("ProductUnit")
                    where puc.ProductUnitConvertId == productUnitConvertId
                    select puc).FirstOrDefault();
        }


        public ProductUnitConvert GetProductUnitConvert(CompEntities entities, int productId, string unitName, int actorCompanyId)
        {
            return (from puc in entities.ProductUnitConvert
                    join pu in entities.ProductUnit
                    on puc.ProductUnitId equals pu.ProductUnitId
                    where pu.Name == unitName && puc.InvoiceProductId == productId && pu.Company.ActorCompanyId == actorCompanyId
                    select puc).FirstOrDefault();
        }

        public ProductUnitConvert GetProductUnitConvert(CompEntities entities, int productId, int productUnitId)
        {
            return (from puc in entities.ProductUnitConvert.Include("ProductUnit")
                    where puc.InvoiceProductId == productId && puc.ProductUnitId == productUnitId
                    select puc).FirstOrDefault();
        }

        public ActionResult AddUpdateProductUnitConverts(List<ProductUnitConvertDTO> unitConvertDTOs)
        {
            var result = new ActionResult();
            var productIds = unitConvertDTOs.Select(p => p.ProductId);
            if (productIds.Count() == 1)
            {
                var query = unitConvertDTOs.Where(u => !u.IsDeleted).GroupBy(x => x.ProductUnitId).Where(g => g.Count() > 1).ToList();
                if (query.Any())
                {
                    return new ActionResult(GetText(7473, "Enhetsomvandlingslistan innehåller samma enhet flera gånger"));
                }
            }

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq
                var company = CompanyManager.GetCompany(entities, base.ActorCompanyId);

                var products = (from p in entities.Product.OfType<InvoiceProduct>()
                                .Include("ProductUnitConvert")
                                where p.State == (int)SoeEntityState.Active &&
                                productIds.Contains(p.ProductId)
                                select p).OfType<InvoiceProduct>();

                var productUnits = GetProductUnits(entities, base.ActorCompanyId);

                #endregion

                foreach (var unitConvertDTO in unitConvertDTOs.Where(x => x.IsModified))
                {
                    if (unitConvertDTO.ConvertFactor <= 0)
                    {
                        return new ActionResult("Omvandlingsfaktorn får inte vara 0 eller mindre!");
                    }

                    if (unitConvertDTO.ProductUnitConvertId == 0 && !unitConvertDTO.IsDeleted)
                    {
                        var currentProduct = products.FirstOrDefault(p => p.ProductId == unitConvertDTO.ProductId);

                        if (unitConvertDTO.ProductUnitId == 0 && !String.IsNullOrEmpty(unitConvertDTO.ProductUnitName))
                        {
                            var productUnit = productUnits.FirstOrDefault(p => p.Name == unitConvertDTO.ProductUnitName && p.Code == unitConvertDTO.ProductUnitName);
                            if (productUnit == null)
                            {
                                // Add new
                                productUnit = new ProductUnit()
                                {
                                    Company = company,
                                    Code = unitConvertDTO.ProductUnitName,
                                    Name = unitConvertDTO.ProductUnitName,
                                };

                                SetCreatedProperties(productUnit);
                                entities.ProductUnit.AddObject(productUnit);

                                entities.SaveChanges();

                                productUnits.Add(productUnit);
                            }
                            currentProduct.ProductUnit = productUnit;
                            SetModifiedProperties(currentProduct);
                        }
                        else if (currentProduct.ProductUnitId != unitConvertDTO.ProductUnitId)
                        {
                            var productUnit = productUnits.FirstOrDefault(p => p.ProductUnitId == unitConvertDTO.ProductUnitId);
                            if (productUnit == null)
                            {
                                // Add new
                                productUnit = new ProductUnit()
                                {
                                    Company = company,
                                    Code = unitConvertDTO.ProductUnitName,
                                    Name = unitConvertDTO.ProductUnitName,
                                };

                                SetCreatedProperties(productUnit);
                                entities.ProductUnit.AddObject(productUnit);

                                entities.SaveChanges();

                                productUnits.Add(productUnit);
                            }

                            //currentProduct.ProductUnit = productUnit;
                            SetModifiedProperties(currentProduct);
                        }

                        if (!unitConvertDTO.BaseProductUnitId.HasValue && !String.IsNullOrEmpty(unitConvertDTO.BaseProductUnitName))
                        {
                            var baseProductUnit = productUnits.FirstOrDefault(p => p.Name == unitConvertDTO.BaseProductUnitName && p.Code == unitConvertDTO.BaseProductUnitName);
                            if (baseProductUnit == null)
                            {
                                // Add new and connect to product
                                baseProductUnit = new ProductUnit()
                                {
                                    Company = company,
                                    Code = unitConvertDTO.BaseProductUnitName,
                                    Name = unitConvertDTO.BaseProductUnitName,
                                };

                                SetCreatedProperties(baseProductUnit);
                                entities.ProductUnit.AddObject(baseProductUnit);

                                entities.SaveChanges();

                                productUnits.Add(baseProductUnit);
                            }

                            if (currentProduct.ProductUnitConvert.Any(c => c.ProductUnitId == baseProductUnit.ProductUnitId && c.ConvertFactor == unitConvertDTO.ConvertFactor))
                                continue;

                            var unitConvert = new ProductUnitConvert
                            {
                                InvoiceProduct = currentProduct,
                                ProductUnit = baseProductUnit,
                                ConvertFactor = unitConvertDTO.ConvertFactor
                            };

                            SetCreatedProperties(unitConvert);
                            entities.ProductUnitConvert.AddObject(unitConvert);
                        }
                        else
                        {
                            if (unitConvertDTO.BaseProductUnitId.HasValue)
                            {
                                if (currentProduct.ProductUnitConvert.Any(c => c.ProductUnitId == unitConvertDTO.BaseProductUnitId.Value && c.ConvertFactor == unitConvertDTO.ConvertFactor))
                                    continue;
                            }

                            if (unitConvertDTO.BaseProductUnitId.HasValue)
                            {
                                // Import
                                var unitConvert = new ProductUnitConvert
                                {
                                    InvoiceProductId = unitConvertDTO.ProductId,
                                    ProductUnitId = unitConvertDTO.BaseProductUnitId.Value,
                                    ConvertFactor = unitConvertDTO.ConvertFactor
                                };

                                SetCreatedProperties(unitConvert);
                                entities.ProductUnitConvert.AddObject(unitConvert);
                            }
                            else if (unitConvertDTO.ProductUnitId > 0)
                            {
                                // Manually added
                                var unitConvert = new ProductUnitConvert
                                {
                                    InvoiceProductId = unitConvertDTO.ProductId,
                                    ProductUnitId = unitConvertDTO.ProductUnitId,
                                    ConvertFactor = unitConvertDTO.ConvertFactor
                                };

                                SetCreatedProperties(unitConvert);
                                entities.ProductUnitConvert.AddObject(unitConvert);
                            }
                        }
                    }
                    else if (unitConvertDTO.IsModified)
                    {
                        var unitConvert = this.GetProductUnitConvert(entities, unitConvertDTO.ProductUnitConvertId);
                        if (unitConvert != null)
                        {
                            if (unitConvertDTO.IsDeleted)
                            {
                                entities.ProductUnitConvert.DeleteObject(unitConvert);
                            }
                            else
                            {
                                unitConvert.ConvertFactor = unitConvertDTO.ConvertFactor;
                                unitConvert.ProductUnitId = unitConvertDTO.ProductUnitId;
                            }
                        }
                    }
                }

                entities.SaveChanges();
            }

            return result;
        }

        public List<ProductUnitConvertExDTO> GetProductUnitConvertDTOs(CompEntities entities, bool includeInactive)
        {
            IQueryable<ProductUnitConvert> query = (from puc in entities.ProductUnitConvert
                                                    where
                                                        puc.InvoiceProduct.Company.Any(c => c.ActorCompanyId == ActorCompanyId)
                                                    select puc);

            if (includeInactive)
                query = query.Where(puc => puc.InvoiceProduct.State != (int)SoeEntityState.Deleted);
            else
                query = query.Where(puc => puc.InvoiceProduct.State == (int)SoeEntityState.Active);

            return query.Select(puc => new ProductUnitConvertExDTO
            {
                ConvertFactor = puc.ConvertFactor,
                ProductId = puc.InvoiceProductId,
                ProductUnitConvertId = puc.ProductUnitConvertId,
                ProductUnitId = puc.ProductUnitId,
                ProductUnitName = puc.ProductUnit.Name,
                BaseProductUnitId = puc.InvoiceProduct.ProductUnitId,
                BaseProductUnitName = puc.InvoiceProduct.ProductUnit.Name,
                ProductNr = puc.InvoiceProduct.Number,
                ProductName = puc.InvoiceProduct.Name,
                CreatedBy = puc.CreatedBy,
                Created = puc.Created,
                ModifiedBy = puc.ModifiedBy,
                Modified = puc.Modified,

            }).ToList();
        }

        public Dictionary<int, string> GetProductUnitsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<ProductUnit> productUnits = GetProductUnits(actorCompanyId);
            foreach (ProductUnit productUnit in productUnits)
            {
                dict.Add(productUnit.ProductUnitId, productUnit.CodeAndName);
            }

            return dict;
        }

        public ProductUnit GetPrevNextProductUnit(int productUnitId, int actorCompanyId, SoeFormMode mode)
        {
            List<ProductUnit> productUnits = GetProductUnits(actorCompanyId);

            // Get index of current ProductUnit
            int i = 0;
            foreach (ProductUnit productUnit in productUnits)
            {
                if (productUnit.ProductUnitId == productUnitId)
                    break;
                i++;
            }

            if (mode == SoeFormMode.Next && i < productUnits.Count - 1)
                i++;
            else if (mode == SoeFormMode.Prev && i > 0)
                i--;

            return productUnits.ElementAt(i);
        }

        public bool ProductUnitExists(string code, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductUnit.NoTracking();
            return ProductUnitExists(entities, code, actorCompanyId);
        }

        public bool ProductUnitExists(CompEntities entities, string code, int actorCompanyId)
        {
            return (from pu in entities.ProductUnit
                    where pu.Company.ActorCompanyId == actorCompanyId &&
                    pu.Code == code
                    select pu).Any();
        }

        public ActionResult AddProductUnit(ProductUnit unit, int actorCompanyId)
        {
            if (unit == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ProductUnit");

            using (CompEntities entities = new CompEntities())
            {
                return AddProductUnit(entities, unit, actorCompanyId);
            }
        }

        public ActionResult AddProductUnit(CompEntities entities, ProductUnit unit, int actorCompanyId)
        {
            // Get company
            unit.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (unit.Company == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Company");

            ActionResult result = AddEntityItem(entities, unit, "ProductUnit");
            result.IntegerValue = unit.ProductUnitId;
            return result;
        }

        public ProductUnit AddProductUnit(CompEntities entities, string unitName, string unitCode, int actorCompanyId)
        {
            var productUnit = new ProductUnit
            {
                Code = unitName,
                Name = unitCode,
            };

            var result = AddProductUnit(entities, productUnit, actorCompanyId);

            return result.Success ? productUnit : null;
        }

        public ActionResult UpdateProductUnit(ProductUnit unit)
        {
            if (unit == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ProductUnit");

            using (CompEntities entities = new CompEntities())
            {
                // Get original unit
                ProductUnit originalUnit = GetProductUnit(entities, unit.ProductUnitId);
                if (originalUnit == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ProductUnit");

                // Modify it
                originalUnit.Code = unit.Code;
                originalUnit.Name = unit.Name;

                // Save it
                return SaveEntityItem(entities, originalUnit);
            }
        }

        public ActionResult SaveProductUnit(ProductUnitDTO unit, List<CompTermDTO> inputTranslations = null)
        {
            ActionResult result = new ActionResult();

            if (unit == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ProductUnit");

            using (CompEntities entities = new CompEntities())
            {
                ProductUnit productUnit = null;
                Company company = CompanyManager.GetCompany(entities, base.ActorCompanyId);

                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {

                        if (unit.ProductUnitId > 0)
                        {
                            productUnit = GetProductUnit(entities, unit.ProductUnitId);
                            if (productUnit == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "ProductUnit");

                            SetModifiedProperties(productUnit);
                        }
                        else
                        {
                            if (ProductUnitExists(entities, unit.Code, base.ActorCompanyId))
                                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(3207, 1005, "Det finns redan en enhet med samma kod."));

                            productUnit = new ProductUnit();
                            productUnit.Company = company;

                            SetCreatedProperties(productUnit);
                            entities.ProductUnit.AddObject(productUnit);
                        }

                        productUnit.Code = unit.Code;
                        productUnit.Name = unit.Name;

                        result = SaveChanges(entities);

                        if (!result.Success)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(3208, 1005, "Enheten kunde inte sparas."));

                        #region Translations

                        if (inputTranslations != null)
                        {
                            var langIdsToSave = inputTranslations.Select(i => (int)i.Lang).Distinct().ToList();
                            var existingTranslations = TermManager.GetCompTerms(entities, CompTermsRecordType.ProductUnitName, productUnit.ProductUnitId);

                            #region Delete existing translations for other languages

                            foreach (var existingTranslation in existingTranslations)
                            {
                                if (langIdsToSave.Contains(existingTranslation.LangId))
                                    continue;

                                existingTranslation.State = (int)SoeEntityState.Deleted;
                            }

                            #endregion

                            #region Add or update translations for languages

                            foreach (int langId in langIdsToSave)
                            {
                                CompTerm translation = null;
                                CompTermDTO inputTranslation = inputTranslations.FirstOrDefault(i => (int)i.Lang == langId);
                                List<CompTerm> existingTranslationsForLang = existingTranslations.Where(i => i.LangId == langId).ToList();
                                if (!existingTranslationsForLang.Any())
                                {
                                    #region Add

                                    translation = new CompTerm { ActorCompanyId = base.ActorCompanyId };
                                    entities.CompTerm.AddObject(translation);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    for (int i = 0; i < existingTranslationsForLang.Count; i++)
                                    {
                                        if (i > 0)
                                        {
                                            //Remove duplicates
                                            existingTranslationsForLang[i].State = (int)SoeEntityState.Deleted;
                                            continue;
                                        }

                                        translation = existingTranslationsForLang[i];
                                    }

                                    #endregion
                                }

                                #region Set values

                                if (transaction != null)
                                {
                                    translation.RecordType = (int)inputTranslation.RecordType;
                                    translation.RecordId = productUnit.ProductUnitId;
                                    translation.LangId = (int)inputTranslation.Lang;
                                    translation.Name = inputTranslation.Name;
                                    translation.State = (int)SoeEntityState.Active;
                                }

                                #endregion
                            }

                            #endregion

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(3209, 1005, "Översättningar kunde inte sparas."));
                        }

                        #endregion

                        //Commit transaction
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
                    if (!result.Success)
                        base.LogTransactionFailed(this.ToString(), this.log);
                    else
                        result.IntegerValue = productUnit != null ? productUnit.ProductUnitId : 0;

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteProductUnit(ProductUnit unit)
        {
            if (unit == null || unit.ProductUnitId == 0)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "ProductUnit");

            using (CompEntities entities = new CompEntities())
            {
                ProductUnit orginalUnit = GetProductUnit(entities, unit.ProductUnitId);

                if (orginalUnit == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "ProductUnit");

                var result = DeleteEntityItem(entities, orginalUnit);

                if (!result.Success && result.ErrorNumber == (int)ActionResultDelete.EntityInUse)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(3210, 1005, "Enheten används och kan därför inte tas bort."));

                return result;
            }
        }

        public List<ProductUnitConvertDTO> ParseProductUnitConversionFile(List<int> productIds, List<byte[]> fileData)
        {
            List<ProductUnitConvertDTO> dtos = new List<ProductUnitConvertDTO>();

            #region Prereqs
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var products = (from p in entitiesReadOnly.Product.OfType<InvoiceProduct>()
                            where p.State == (int)SoeEntityState.Active &&
                            productIds.Contains(p.ProductId)
                            select p).OfType<InvoiceProduct>();

            var productUnits = GetProductUnits(base.ActorCompanyId);

            #endregion

            #region Parse file

            string stringConvert = System.Text.Encoding.UTF8.GetString(fileData[0]);
            var currentLocale = System.Globalization.CultureInfo.CurrentCulture;

            using (var reader = new StringReader(stringConvert))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var item = line.Split(',');
                    if (item.Length != 4)
                        continue;

                    var productNr = item[0];
                    if (string.IsNullOrEmpty(productNr))
                        continue;

                    var product = products.Where(p => p.Number == productNr).FirstOrDefault();
                    if (product == null)
                        continue;

                    var unitT = item[2];
                    if (string.IsNullOrEmpty(unitT))
                        continue;

                    var unitTo = productUnits.FirstOrDefault(u => u.Name == unitT);
                    var unitF = item[1];
                    if (string.IsNullOrEmpty(unitF))
                        continue;

                    var unitFrom = productUnits.FirstOrDefault(u => u.Name == unitF);

                    var convF = item[3].Replace(".", currentLocale.NumberFormat.NumberDecimalSeparator);
                    if (!decimal.TryParse(item[3], out decimal conversionFactor))
                        continue;

                    dtos.Add(new ProductUnitConvertDTO()
                    {
                        ProductId = product.ProductId,
                        ProductNr = product.Number,
                        ProductName = product.Name,
                        ProductUnitId = unitTo?.ProductUnitId ?? 0,
                        ProductUnitName = unitTo?.Name ?? unitT,
                        ConvertFactor = conversionFactor,
                        BaseProductUnitId = unitFrom?.ProductUnitId,
                        BaseProductUnitName = unitFrom?.Name ?? unitF,
                        IsModified = true,
                    });
                }
            }

            #endregion

            return dtos;
        }

        #endregion

        #region PriceListType


        public InvoiceProductExternalDTO GetPriceListImportedExternal(CompEntities entities, int actorCompanyId, int sysWholeSellerId, string productNr, int sysProductId, int priceListTypeId)
        {
            //special for extra lund wholsellers
            var importedWolesellerId = sysWholeSellerId == 20 ? 85 : sysWholeSellerId;
            var importedProduct = entities.PriceListImportedPriceSearchView.Where(x => x.ActorCompanyId == actorCompanyId && x.SysWholesellerId == importedWolesellerId && x.ProductNumber == productNr).FirstOrDefault();

            if (importedProduct != null)
            {
                return new InvoiceProductExternalDTO(importedProduct.PriceListImportedHeadId, importedProduct.ProductId, (int)PriceListOrigin.CompDbPriceList);
            }
            else if (WholsellerNetPriceManager.HasCompleteNetPriceList(sysWholeSellerId))
            {
                var netPrice = WholsellerNetPriceManager.GetNetPrice(entities, actorCompanyId, sysProductId, sysWholeSellerId, priceListTypeId);

                if (netPrice != null)
                {
                    return new InvoiceProductExternalDTO(netPrice.WholsellerNetPriceId, sysProductId, (int)PriceListOrigin.CompDbNetPriceList);
                }
            }

            return new InvoiceProductExternalDTO(0, 0, (int)PriceListOrigin.Unknown);
        }

        public ActionResult DeleteCompPriceList(int actorCompanyId, int priceListImportedHeadId)
        {
            try
            {
                using (var entities = new CompEntities())
                {
                    entities.CommandTimeout = (5 * 60); // Increase timeout for this operation, should take less than 2 minutes

                    // Validate
                    if (PriceRuleManager.PricelistConnectedToPriceRule(entities, priceListImportedHeadId, actorCompanyId))
                        return new ActionResult((int)ActionResultSave.NothingSaved, GetText(7640, "Prislistan är kopplad mot en prisformel och kan därför ej tas bort"));

                    var result = entities.DeletePriceListImportedHead(actorCompanyId, priceListImportedHeadId).FirstOrDefault();
                    if (result != null && result.ProductImportedDeleted.HasValue && result.PriceListImportedDeleted.HasValue)
                    {
                        return new ActionResult()
                        {
                            Success = true,
                            IntegerValue = result.PriceListImportedDeleted.Value,
                            IntegerValue2 = result.ProductImportedDeleted.Value
                        };
                    }
                    else
                    {
                        return new ActionResult((int)ActionResultDelete.NothingSaved);
                    }
                }
            }
            catch (Exception)
            {
                return new ActionResult((int)ActionResultDelete.NothingSaved, "Critical error");
            }
        }

        #endregion

        #region PriceList

        // Variable checkProduct used to override compdbcheck on product
        public List<InvoiceProductPriceResult> GetProductPrices(int actorCompanyId, ProductPricesRequestDTO dto)
        {
            List<InvoiceProductPriceResult> result = new List<InvoiceProductPriceResult>(dto.Products.Count);

            using (var entities = new CompEntities())
            {
                foreach (var item in dto.Products)
                {
                    var tmpProductPrice = GetProductPrice(entities, actorCompanyId, new ProductPriceRequestDTO
                    {
                        PriceListTypeId = dto.PriceListTypeId,
                        ProductId = item.ProductId,
                        CustomerId = dto.CustomerId,
                        CurrencyId = dto.CurrencyId,
                        WholesellerId = dto.WholesellerId,
                        Quantity = item.Quantity,
                        ReturnFormula = dto.ReturnFormula,
                        CopySysProduct = dto.CopySysProduct,
                        PurchasePrice = item.PurchasePrice,
                    }, item.WholesellerName, dto.IncludeCustomerPrices, null, dto.CheckProduct);
                    tmpProductPrice.RowId = item.TempRowId;
                    result.Add(tmpProductPrice);
                }
            }

            return result;
        }

        public InvoiceProductPriceResult GetProductPrice(int actorCompanyId, ProductPriceRequestDTO getProductPriceDTO, string wholeSellerName = null, bool? includeCustomerPrices = true, bool? checkProduct = null)
        {
            using (CompEntities entities = new CompEntities())
            {
                return GetProductPrice(entities, actorCompanyId, getProductPriceDTO, wholeSellerName, includeCustomerPrices, checkProduct);
            }
        }

        /// <summary>
        /// Returns the calculated price for the products including customer and pricelist rebate, excluding tax
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="priceListTypeId">PriceListType ID. If zero, it will be fetched from customer or company default</param>
        /// <param name="productId">Product ID</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="currencyId">ID for the currency the price should be returned in</param>
        /// <param name="wholeSellerId">The id for the wholeseller to search for, if 0 the last used will be used.</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="returnFormula">If the price calculation formula should be included in the result</param>
        /// <param name="copySysProduct">If exteranal product, copy it to internal list</param>
        /// <param name="wholeSellerName">Overrides sysWholeSellerId, tries to find the id by quering the name.</param>
        /// <returns></returns>
        public InvoiceProductPriceResult GetProductPrice(CompEntities entities, int actorCompanyId, ProductPriceRequestDTO getProductPriceDTO, string wholeSellerName = null, bool? includeCustomerPrices = null, bool? checkProduct = null, bool? overrideCompDbCheck = null)
        {
            InvoiceProduct product = GetInvoiceProduct(entities, getProductPriceDTO.ProductId, true, true, false);
            if (product == null)
                return new InvoiceProductPriceResult(ActionResultSelect.ProductNotFound, GetText(8331, "Artikel kunde inte hittas"));

            var productPriceResult = new InvoiceProductPriceResult();
            productPriceResult.ProductIsSupplementCharge = product.IsSupplementCharge;

            // Get customer
            Customer customer = CustomerManager.GetCustomer(entities, getProductPriceDTO.CustomerId);

            bool TrySetCustomerSpecificPrices()
            {
                if (includeCustomerPrices.GetValueOrDefault() && customer != null)
                {
                    var customerProduct = customer.CustomerProduct.IsLoaded ? customer.CustomerProduct.FirstOrDefault(cp => cp.ProductId == getProductPriceDTO.ProductId) :
                                                                              entities.CustomerProduct.FirstOrDefault(x => x.ActorCustomerId == customer.ActorCustomerId && x.ProductId == getProductPriceDTO.ProductId);
                    if (customerProduct != null)
                    {
                        int baseCurrencyId = CountryCurrencyManager.GetCompanyBaseSysCurrencyId(entities, actorCompanyId);

                        productPriceResult.SalesPrice = NumberUtility.GetFormattedDecimalValue(customerProduct.Price, 4);
                        productPriceResult.SysWholesellerName = GetText(9156, "Kundunikt pris");
                        productPriceResult.PurchasePrice = product.PurchasePrice;
                        productPriceResult.ProductUnit = product.ProductUnit?.Code;
                        productPriceResult.ErrorNumber = 0;
                        productPriceResult.Warning = false;
                        productPriceResult.ErrorMessage = null;
                        productPriceResult.CurrencyDiffer = (baseCurrencyId != getProductPriceDTO.CurrencyId);

                        return true;
                    }
                }
                return false;
            }

            // Check if product is external
            if (product.ExternalProductId.GetValueOrDefault() > 0 && (product.ExternalPriceListHeadId.GetValueOrDefault() > 0 || getProductPriceDTO.WholesellerId > 0) || !string.IsNullOrEmpty(wholeSellerName))
            {
                decimal? customerPrice = null;
                if (TrySetCustomerSpecificPrices())
                {
                    customerPrice = productPriceResult.SalesPrice;
                }

                var purchasePrice = getProductPriceDTO.PurchasePrice.GetValueOrDefault();
                productPriceResult = GetExternalProductPrice(entities, getProductPriceDTO.PriceListTypeId, product, getProductPriceDTO.CustomerId, actorCompanyId, getProductPriceDTO.WholesellerId, getProductPriceDTO.CopySysProduct, wholeSellerName, checkProduct ?? false, overrideCompDbCheck: overrideCompDbCheck ?? false, ediPurchasePrice: purchasePrice, ignoreWholesellerDiscount: purchasePrice > 0);
                if (customerPrice.HasValue)
                {
                    productPriceResult.SalesPrice = customerPrice.Value;
                }
            }
            else
            {
                #region Internal Product
                // Get default pricelist type from company setting            
                int defaultPriceListTypeId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultPriceListType, 0, actorCompanyId, 0);
                PriceList priceList = null;

                bool IsProjectPriceList()
                {
                    if (priceList == null) return false;
                    if (!priceList.PriceListTypeReference.IsLoaded)
                    {
                        priceList.PriceListTypeReference.Load();
                    }

                    return priceList.PriceListType.IsProjectPriceList.GetValueOrDefault();
                }

                if (customer == null)
                {
                    productPriceResult.ErrorMessage = GetText(1284, "Företag hittades inte");
                }

                if (!product.DontUseDiscountPercent.GetValueOrDefault() && customer != null)
                {
                    switch ((TermGroup_InvoiceProductVatType)product.VatType)
                    {
                        case TermGroup_InvoiceProductVatType.None:
                            productPriceResult.CustomerDiscountPercent = 0;
                            break;
                        case TermGroup_InvoiceProductVatType.Merchandise:
                            productPriceResult.CustomerDiscountPercent = customer.DiscountMerchandise;
                            break;
                        case TermGroup_InvoiceProductVatType.Service:
                            productPriceResult.CustomerDiscountPercent = customer.DiscountService;
                            break;
                    }
                }
                PriceListType priceListType = getProductPriceDTO.PriceListTypeId > 0 ? ProductPricelistManager.GetPriceListType(entities, getProductPriceDTO.PriceListTypeId, actorCompanyId) : null;

                if (getProductPriceDTO.PriceListTypeId != 0)
                {
                    // Pricelist type specified, get pricelist for specified product
                    priceList = ProductPricelistManager.GetPriceList(entities, getProductPriceDTO.ProductId, getProductPriceDTO.PriceListTypeId, null, null, false, getProductPriceDTO.Quantity);

                    // The specified product does not have a pricelist for specified type,
                    // use default pricelist type instead
                    if (priceList == null && getProductPriceDTO.PriceListTypeId != defaultPriceListTypeId)
                        priceList = ProductPricelistManager.GetPriceList(entities, getProductPriceDTO.ProductId, defaultPriceListTypeId, null, priceListType == null ? (bool?)null : priceListType.InclusiveVat);

                    if (priceList == null && customer != null && customer.PriceListTypeId.HasValue && customer.PriceListTypeId.Value != getProductPriceDTO.PriceListTypeId && customer.PriceListTypeId.Value != defaultPriceListTypeId)
                    {
                        // The default pricelist type did not exist on product either
                        // Try to get it from the customer
                        getProductPriceDTO.PriceListTypeId = customer.PriceListTypeId.Value;
                        priceList = ProductPricelistManager.GetPriceList(entities, getProductPriceDTO.ProductId, getProductPriceDTO.PriceListTypeId, null);
                    }

                    if (priceList == null)
                    {
                        // Default pricelist or customers pricelist not found on product
                        // Check if any pricelist at all exists and take the first that matches one with same VAT setting...
                        IEnumerable<PriceList> priceLists = ProductPricelistManager.GetPriceListsForProduct(entities, getProductPriceDTO.ProductId, priceListType == null ? (bool?)null : priceListType.InclusiveVat);
                        if (priceLists != null && priceLists.Any())
                            priceList = priceLists.First();
                        productPriceResult.Warning = true;
                        if (priceList != null)
                            productPriceResult.ErrorMessage = string.Format(GetText(3444, "Artikeln saknar både företagets och kundens standardprislista. \nPrislista '{0}' används istället"), priceList.PriceListType.Name);
                        else
                            productPriceResult.ErrorMessage = GetText(3522, "Artikeln saknar prislista");
                    }
                }
                else
                {
                    // No pricelist type specified
                    // Try to get price list type from customer
                    if (customer != null && customer.PriceListTypeId.HasValue)
                    {
                        // Customer is specified and has a pricelist type specified
                        // It is not certain that the product has a pricelist of the customers type,
                        // therefore we check it below and use company setting standard if not found
                        getProductPriceDTO.PriceListTypeId = customer.PriceListTypeId.Value;
                        priceList = ProductPricelistManager.GetPriceList(entities, getProductPriceDTO.ProductId, getProductPriceDTO.PriceListTypeId, null, null, true, getProductPriceDTO.Quantity);
                    }

                    if (priceList == null)
                    {
                        // No customer specified, or customers pricelist type did not exist on product
                        // Get default pricelist (company setting) for specified product
                        priceList = ProductPricelistManager.GetPriceList(entities, getProductPriceDTO.ProductId, defaultPriceListTypeId, null, null, true, getProductPriceDTO.Quantity);
                        //result.Warning = true;
                        //result.ErrorMessage = GetText(3293, "Artikeln saknar kundens prislista, standardprislistan används istället");
                    }

                    if (priceList == null)
                    {
                        // The default pricelist type did not exist on product either
                        // Check if any pricelist at all exists and take the first
                        IEnumerable<PriceList> priceLists = ProductPricelistManager.GetPriceListsForProduct(entities, getProductPriceDTO.ProductId);
                        if (priceLists != null && priceLists.Any())
                            priceList = priceLists.First();
                        if (priceList != null)
                        {
                            productPriceResult.Warning = true;
                            productPriceResult.ErrorMessage = string.Format(GetText(3444, "Artikeln saknar både företagets och kundens standardprislista. \nPrislista '{0}' används istället"), priceList.PriceListType.Name);
                        }
                    }
                }

                if (!IsProjectPriceList() && TrySetCustomerSpecificPrices())
                {
                    return productPriceResult;
                }
                else if (priceList == null)
                {
                    return new InvoiceProductPriceResult(ActionResultSelect.EntityNotFound, GetText(3272, "Ingen prislista funnen"));
                }

                // Get pricelist type
                if (getProductPriceDTO.PriceListTypeId == 0)
                {
                    getProductPriceDTO.PriceListTypeId = priceList.PriceListTypeId;
                    priceListType = ProductPricelistManager.GetPriceListType(entities, getProductPriceDTO.PriceListTypeId, actorCompanyId);
                }

                // Get purchase price
                productPriceResult.PurchasePrice = NumberUtility.GetFormattedDecimalValue(product.PurchasePrice, 4);
                productPriceResult.InclusiveVat = priceList.PriceListType?.InclusiveVat ?? false;
                productPriceResult.ProductUnit = product.ProductUnit?.Code;

                // Product price (without any discount)
                RuleValue productPriceValue = new RuleValue(priceList.Price, GetText(4156, "Grundpris"));

                // Pricelist discount
                decimal priceListDiscount = priceListType != null ? priceListType.DiscountPercent : 0M;
                RuleValue priceListDiscountValue = new RuleValue(1 - (priceListDiscount / 100), GetText(4157, "Prislisterabatt"));

                // Calculate price
                Business.Util.Rule priceRule = new Business.Util.Rule(productPriceValue, RuleOperatorType.multiplication, priceListDiscountValue);
                decimal price = priceRule.ApplyRule().Value;

                // Get all currencys
                List<CompCurrency> compCurrencies = CountryCurrencyManager.GetCompCurrencies(entities, actorCompanyId, false);

                // Check if expected currency is different from the pricelists currency
                CompCurrency priceListCurrency = null;
                if (priceList.PriceListType != null && !priceList.PriceListType.CurrencyReference.IsLoaded)
                {
                    priceList.PriceListType.CurrencyReference.Load();
                }

                int priceListCurrencyId = priceList.PriceListType != null && priceList.PriceListType.Currency != null ? priceList.PriceListType.Currency.CurrencyId : getProductPriceDTO.CurrencyId;
                if (priceListCurrencyId != getProductPriceDTO.CurrencyId)
                {
                    // If the pricelists currency differ from the expected currency,
                    // Recalculate the price to the expected currency.
                    // The price will be calculated via the base currency.
                    productPriceResult.CurrencyDiffer = true;
                }

                // Create price formula
                if (getProductPriceDTO.ReturnFormula)
                {
                    if (priceListCurrency == null)
                        priceListCurrency = compCurrencies.FirstOrDefault(c => c.CurrencyId == priceListCurrencyId);

                    string formula = string.Format(GetText(3298, "Prislista {0}"), priceListType?.Name ?? GetText(2244, "Borttagen"));
                    formula += "\n";
                    formula += string.Format(GetText(3299, "Grundpris {0} {1}"), priceList.Price.ToString("N2"), priceListCurrency.Code);
                    if (priceListDiscount != 0)
                        formula += "\n" + string.Format(GetText(3299, "Prislisterabatt {0}%"), priceListDiscount.ToString("N2"));
                    productPriceResult.PriceFormula = formula;
                }

                productPriceResult.SalesPrice = NumberUtility.GetFormattedDecimalValue(price, 4);

                #endregion
            }

            // Check discount by productgroup and customercategory (locksmith)
            if (SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingUseProductGroupCustomerCategoryDiscount, 0, actorCompanyId, 0))
            {
                decimal? discountPercent = null;

                if (product.ProductGroupId.HasValue)
                    discountPercent = this.MarkupManager.GetDiscountByCustomerAndProduct(entities, getProductPriceDTO.CustomerId, product.ProductGroupId.Value, actorCompanyId);
                else
                    discountPercent = this.MarkupManager.GetDiscountByCustomerAndProduct(entities, getProductPriceDTO.CustomerId, product, actorCompanyId);
                productPriceResult.DiscountPercent = discountPercent;
            }

            return productPriceResult;
        }

        public void SetProductModified(CompEntities entities, int productId)
        {
            SetModifiedProperties(GetProduct(entities, productId, false));
        }

        #endregion

        #region SysHouseholdType

        public List<SysHouseholdType> GetAllSysHouseholdTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysHouseholdType
                            .ToList<SysHouseholdType>();
        }

        public Dictionary<int, string> GetSysHouseholdTypeDict(bool addEmptyRow = false)
        {
            return GetSysHouseholdType(addEmptyRow).ToDictionary(s => s.SysHouseholdTypeId, s => s.Name);
        }

        public List<SysHouseholdType> GetSysHouseholdType(bool addEmptyRow = false)
        {
            List<SysHouseholdType> sysHouseholdTypes = new List<SysHouseholdType>();

            int langId = GetLangId();
            int companyCountryId = GetCompanySysCountryIdFromCache(base.ActorCompanyId);

            //Uses SysDbCache
            using (SOESysEntities entities = new SOESysEntities())
            {
                var query = (from ht in entities.SysHouseholdType
                             join st in entities.SysTerm on ht.SysTermId equals st.SysTermId
                             where st.SysTermGroupId == (int)TermGroup.SysHouseholdType &&
                             ht.SysCountryId == companyCountryId &&
                             st.LangId == langId &&
                             ht.State == (int)SoeEntityState.Active
                             orderby ht.XMLOrder
                             select new
                             {
                                 SysHouseholdTypeId = ht.SysHouseholdTypeId,
                                 SysHouseholdTypeClassification = ht.SysHouseholdTypeClassification,
                                 SysTermId = ht.SysTermId,
                                 Name = st.Name,
                                 XMLtagName = ht.XMLTagName,
                             }).ToList();

                if (addEmptyRow)
                {
                    sysHouseholdTypes.Add(new SysHouseholdType()
                    {
                        SysHouseholdTypeId = 0,
                        SysHouseholdTypeClassification = 0,
                        SysTermId = 0,
                        Name = String.Empty,
                        XMLTagName = String.Empty,
                    });
                }

                foreach (var item in query)
                {
                    sysHouseholdTypes.Add(new SysHouseholdType()
                    {
                        SysHouseholdTypeId = item.SysHouseholdTypeId,
                        SysHouseholdTypeClassification = item.SysHouseholdTypeClassification,
                        SysTermId = item.SysTermId,
                        Name = item.Name,
                        XMLTagName = item.XMLtagName,
                    });
                }
            }

            return sysHouseholdTypes;
        }

        #endregion

        #region SysProductGroups

        public List<SysProductGroupSmallDTO> GetVVSProductGroupsForSearch()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return (from g in sysEntitiesReadOnly.SysProductGroup
                    where g.Type == (int)ExternalProductType.Plumbing &&
                    g.SysCountryId == (int)TermGroup_Country.SE &&
                    g.Identifier.Length < 5
                    select new SysProductGroupSmallDTO()
                    {
                        SysProductGroupId = g.SysProductGroupId,
                        ParentSysProductGroupId = g.ParentSysProductGroupId,
                        Identifier = g.Identifier,
                        Name = g.Identifier + " " + g.Name,
                    }).ToList();
        }

        #endregion

        #region Help methods

        public void ExcludeBaseProducts(ref List<InvoiceProductPriceSearchDTO> products, List<CompanySettingType> baseProductCompanySettingTypes, int actorCompanyId, int userId)
        {
            List<int> productIds = GetBaseProductIds(baseProductCompanySettingTypes, actorCompanyId, userId);

            foreach (var productId in productIds)
            {
                InvoiceProductPriceSearchDTO product = products.FirstOrDefault(p => p.ProductId == productId);
                products.Remove(product);
            }
        }

        private List<int> GetBaseProductIds(List<CompanySettingType> baseProductCompanySettingTypes, int actorCompanyId, int userId)
        {
            List<int> ids = new List<int>();
            foreach (var baseProductCompanySettingType in baseProductCompanySettingTypes)
            {
                ids.Add(GetBaseProductId(baseProductCompanySettingType, actorCompanyId, userId));
            }
            return ids;
        }

        private int GetBaseProductId(CompanySettingType baseProductCompanySettingType, int actorCompanyId, int userId)
        {
            return SettingManager.GetIntSetting(SettingMainType.Company, (int)baseProductCompanySettingType, userId, actorCompanyId, 0);
        }

        public ActionResult FixSharedPayrollProducts()
        {
            // 1.Kopiera lönearten till varje företag som är kopplad till den

            //    a.Kopiera INTE inställningar(PayrollProductSetting), måste göras manuellt
            //  (dvs konton, prisformler, pristyper per löneavtal)
            //2.Ställ om kopplingen till nya företaget(tabell: CompanyProductMapping)
            //3.Ställ om alla kopplingar till tidkoder(tabell: TimeCodePayrollProduct)
            //4.Ställ om alla kopplingar till saldon(tabell: TimeAccumulatorPayrollProduct)
            //5.Ställ om alla kopplingar till frånvaroregler(tabell: TimmeAbsenceRuleRow)
            //6.Ställ om löneartskategorier (tabell: CompanyCategoryRecord)
            //7.Ställ om schmeatransaktioner (tabell: TimePayrollScheduleTransaction)
            //8.Ställ om löneartstransaktioner (tabell: TimePayrollTransaction)   

            ActionResult result = new ActionResult();

            string newProducts = string.Empty;
            string changedDicts = string.Empty;

            using (CompEntities entities = new CompEntities())
            {
                entities.CommandTimeout = 600;
                List<int> productIds = new List<int>();

                #region ProductIds

                using (SqlConnection sqlConnection = new SqlConnection(FrownedUponSQLClient.GetADOConnectionString()))
                {
                    var reader = FrownedUponSQLClient.ExcuteQuery(sqlConnection, @"Select productid from CompanyProductMapping group by productid having count(ActorCompanyId) > 1");

                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            productIds.Add((int)reader["productid"]);
                        }
                    }
                }

                #endregion

                #region TimeOut

                var transactionOptions = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.FromMinutes(20),
                };

                #endregion

                #region Affected products

                //Get affected products

                IQueryable<PayrollProduct> query = entities.Product.OfType<PayrollProduct>();
                query = query.Include("PayrollProductSetting.PayrollGroup");
                query = query.Include("TimeCodePayrollProduct.TimeCode");
                query = query.Include("Company");
                query = query.Include("TimeAccumulatorPayrollProduct.TimeAccumulator");
                query = query.Include("TimeAbsenceRuleRow.TimeAbsenceRuleHead");
                query = query.Include("TimePayrollScheduleTransaction");
                query = query.Include("PayrollGroupPayrollProduct.PayrollGroup");
                query = query.Include("MassRegistrationTemplateHead");
                query = query.Include("EmployeeTimePeriodProductSetting.EmployeeTimePeriod");
                query = query.Include("FixedPayrollRow");

                var payrollProducts = (from p in query
                                       where productIds.Contains(p.ProductId) &&
                                       (p.ModifiedBy == null || !p.ModifiedBy.Equals("SoftOne System (FMPP)"))
                                       select p).OfType<PayrollProduct>().ToList();

                #endregion

                #region Handle products

                List<PayrollProduct> newPayrollProducts = new List<PayrollProduct>();

                foreach (PayrollProduct oldProduct in payrollProducts)
                {
                    if (!oldProduct.Company.IsLoaded)
                        oldProduct.Company.Load();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, transactionOptions))
                    {


                        // Create new products for each company
                        foreach (var company in oldProduct.Company)
                        {
                            #region Create product 

                            PayrollProduct newProduct = new PayrollProduct()
                            {
                                Number = oldProduct.Number,
                                Name = oldProduct.Name,
                                ProductUnitId = oldProduct.ProductUnitId,
                                ProductGroupId = oldProduct.ProductGroupId,
                                Type = oldProduct.Type,
                                Description = oldProduct.Description,
                                AccountingPrio = oldProduct.AccountingPrio,
                                Created = oldProduct.Created,
                                CreatedBy = oldProduct.CreatedBy,
                                Modified = oldProduct.Modified,
                                ModifiedBy = oldProduct.ModifiedBy,
                                State = oldProduct.State
                            };

                            newProduct.SysPayrollProductId = oldProduct.SysPayrollProductId;
                            newProduct.SysPayrollTypeLevel1 = oldProduct.SysPayrollTypeLevel1;
                            newProduct.SysPayrollTypeLevel2 = oldProduct.SysPayrollTypeLevel2;
                            newProduct.SysPayrollTypeLevel3 = oldProduct.SysPayrollTypeLevel3;
                            newProduct.SysPayrollTypeLevel4 = oldProduct.SysPayrollTypeLevel4;
                            newProduct.ShortName = oldProduct.ShortName;
                            newProduct.Factor = oldProduct.Factor;
                            newProduct.ResultType = oldProduct.ResultType;
                            newProduct.Payed = oldProduct.Payed;
                            newProduct.ExcludeInWorkTimeSummary = oldProduct.ExcludeInWorkTimeSummary;
                            newProduct.AverageCalculated = oldProduct.AverageCalculated;
                            newProduct.Export = oldProduct.Export;
                            newProduct.IncludeAmountInExport = oldProduct.IncludeAmountInExport;
                            newProduct.UseInPayroll = oldProduct.UseInPayroll;
                            newProduct.Company.Add(company);

                            #endregion

                            #region AddToEntities

                            entities.Product.AddObject(newProduct);

                            #endregion

                            #region save

                            result = base.SaveChanges(entities);

                            if (!result.Success)
                                return result;

                            if (!string.IsNullOrEmpty(newProducts))
                                newProducts += Environment.NewLine;
                            else
                                newProducts = "ActorCompanyId;Name;OldProductId;NewProductId" + Environment.NewLine;

                            newProducts += company.ActorCompanyId.ToString() + ";" + company.Name + ";" + oldProduct.ProductId.ToString() + ";" + newProduct.ProductId.ToString();

                            #endregion

                            #region PayrollProductSetting

                            foreach (var payrollProductSetting in newProduct.PayrollProductSetting)
                            {
                                //None found in next
                                if (payrollProductSetting.PayrollGroup != null && payrollProductSetting.PayrollGroup.ActorCompanyId == company.ActorCompanyId)
                                {
                                    newProduct.PayrollProductSetting = oldProduct.PayrollProductSetting;
                                    changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " PayrollProductSetting changed";
                                }
                            }

                            #endregion

                            #region   TimePayrollScheduleTransaction


                            if (!oldProduct.TimePayrollScheduleTransaction.IsLoaded)
                                oldProduct.TimePayrollScheduleTransaction.Load();

                            List<TimePayrollScheduleTransaction> oldTimePayrollScheduleTransactions = oldProduct.TimePayrollScheduleTransaction.Where(t => t.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldTimePayrollScheduleTransaction in oldTimePayrollScheduleTransactions)
                            {
                                oldTimePayrollScheduleTransaction.PayrollProduct = newProduct;
                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " TimePayrollScheduleTransaction changed id: " + oldTimePayrollScheduleTransaction.TimePayrollScheduleTransactionId.ToString();
                            }

                            #endregion

                            #region TimeCodePayrollProduct

                            if (!oldProduct.TimeCodePayrollProduct.IsLoaded)
                                oldProduct.TimeCodePayrollProduct.Load();

                            List<TimeCodePayrollProduct> oldTimeCodePayrollProducts = oldProduct.TimeCodePayrollProduct.Where(t => t.TimeCode.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldTimeCodePayrollProduct in oldTimeCodePayrollProducts)
                            {
                                oldTimeCodePayrollProduct.PayrollProduct = newProduct;
                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " TimeCodePayrollProduct changed id: " + oldTimeCodePayrollProduct.TimeCodePayrollProductId.ToString();
                            }

                            #endregion

                            #region TimeAccumulatorPayrollProduct

                            if (!oldProduct.TimeAccumulatorPayrollProduct.IsLoaded)
                                oldProduct.TimeAccumulatorPayrollProduct.Load();

                            List<TimeAccumulatorPayrollProduct> oldTimeAccumulatorPayrollProducts = oldProduct.TimeAccumulatorPayrollProduct.Where(t => t.TimeAccumulator.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldTimeAccumulatorPayrollProduct in oldTimeAccumulatorPayrollProducts)
                            {
                                //creating new mapping since old one for some reason could be changed.
                                TimeAccumulatorPayrollProduct newTimeAccumulatorPayrollProduct = new TimeAccumulatorPayrollProduct()
                                {
                                    TimeAccumulatorId = oldTimeAccumulatorPayrollProduct.TimeAccumulatorId,
                                    PayrollProduct = newProduct,
                                    Factor = oldTimeAccumulatorPayrollProduct.Factor
                                };

                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " TimeAccumulatorPayrollProduct changed id: " + oldTimeAccumulatorPayrollProduct.TimeAccumulatorId.ToString();

                                entities.TimeAccumulatorPayrollProduct.AddObject(newTimeAccumulatorPayrollProduct);
                            }

                            #endregion

                            #region TimeAbsenceRuleRow

                            if (!oldProduct.TimeAbsenceRuleRow.IsLoaded)
                                oldProduct.TimeAbsenceRuleRow.Load();

                            List<TimeAbsenceRuleRow> timeAbsenceRuleRows = oldProduct.TimeAbsenceRuleRow.Where(t => t.TimeAbsenceRuleHead.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldTimeAbsenceRuleRow in timeAbsenceRuleRows)
                            {
                                oldTimeAbsenceRuleRow.PayrollProduct = newProduct;

                                List<TimeAbsenceRuleRowPayrollProducts> timeAbsenceRuleRowPayrollProducts = oldTimeAbsenceRuleRow.TimeAbsenceRuleRowPayrollProducts.Where(t => t.TimeAbsenceRuleRow.TimeAbsenceRuleHead.ActorCompanyId == company.ActorCompanyId).ToList();

                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " TimeAbsenceRuleRow changed id: " + oldTimeAbsenceRuleRow.TimeAbsenceRuleRowId.ToString();

                                foreach (var row in timeAbsenceRuleRowPayrollProducts)
                                {
                                    if (row.SourcePayrollProductId == oldProduct.ProductId)
                                    {
                                        row.SourcePayrollProduct = newProduct;
                                        changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " timeAbsenceRuleRowPayrollProducts Source changed id: " + oldTimeAbsenceRuleRow.TimeAbsenceRuleRowId.ToString();
                                    }
                                    if (row.TargetPayrollProductId == oldProduct.ProductId)
                                    {
                                        row.TargetPayrollProduct = newProduct;
                                        changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " timeAbsenceRuleRowPayrollProducts Target changed id: " + oldTimeAbsenceRuleRow.TimeAbsenceRuleRowId.ToString();
                                    }
                                }
                            }



                            #endregion

                            #region save

                            result = base.SaveChanges(entities);

                            if (!result.Success)
                                return result;

                            #endregion

                            #region TimePayrollTransaction


                            #region update

                            result = base.BulkUpdateChanges<TimePayrollTransaction>(entities.TimePayrollTransaction.Where(t => t.ActorCompanyId == company.ActorCompanyId && t.ProductId == oldProduct.ProductId), u => new TimePayrollTransaction { ProductId = newProduct.ProductId });

                            if (!result.Success)
                                return result;
                            else
                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " TimePayrollTransaction affected " + result.ObjectsAffected.ToString() + " from Productid: " + oldProduct.ProductId.ToString() + " to ProductId: " + newProduct.ProductId.ToString();

                            #endregion

                            #endregion

                            #region save

                            result = base.SaveChanges(entities);

                            if (!result.Success)
                                return result;

                            #endregion

                            #region FixedPayrollRow

                            if (!oldProduct.FixedPayrollRow.IsLoaded)
                                oldProduct.FixedPayrollRow.Load();

                            List<FixedPayrollRow> fixedPayrollRow = oldProduct.FixedPayrollRow.Where(f => f.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldFixedPayrollRow in fixedPayrollRow)
                            {
                                oldFixedPayrollRow.PayrollProduct = newProduct;
                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " FixedPayrollRow changed id: " + oldFixedPayrollRow.FixedPayrollRowId.ToString();

                            }

                            #endregion

                            #region PayrollGroupPayrollProduct

                            if (!oldProduct.PayrollGroupPayrollProduct.IsLoaded)
                                oldProduct.PayrollGroupPayrollProduct.Load();

                            List<PayrollGroupPayrollProduct> payrollGroupPayrollProduct = oldProduct.PayrollGroupPayrollProduct.Where(p => p.PayrollGroup.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldPayrollGroupPayrollProduct in payrollGroupPayrollProduct)
                            {
                                oldPayrollGroupPayrollProduct.PayrollProduct = newProduct;
                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " PayrollGroupPayrollProduct changed id: " + oldPayrollGroupPayrollProduct.PayrollGroupPayrollProductId.ToString();
                            }

                            #endregion

                            #region MassRegistrationTemplateHead


                            if (!oldProduct.MassRegistrationTemplateHead.IsLoaded)
                                oldProduct.MassRegistrationTemplateHead.Load();

                            List<MassRegistrationTemplateHead> massRegistrationTemplateHead = oldProduct.MassRegistrationTemplateHead.Where(m => m.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldMassRegistrationTemplateHead in massRegistrationTemplateHead)
                            {
                                oldMassRegistrationTemplateHead.PayrollProduct = newProduct;
                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " MassRegistrationTemplateHead changed id: " + oldMassRegistrationTemplateHead.MassRegistrationTemplateHeadId.ToString();

                            }

                            #endregion

                            #region EmployeTimePeriodProductSetting

                            if (!oldProduct.EmployeeTimePeriodProductSetting.IsLoaded)
                                oldProduct.EmployeeTimePeriodProductSetting.Load();

                            List<EmployeeTimePeriodProductSetting> employeTimePeriodProductSetting = oldProduct.EmployeeTimePeriodProductSetting.Where(e => e.EmployeeTimePeriod.ActorCompanyId == company.ActorCompanyId).ToList();

                            foreach (var oldEmployeTimePeriodProductSetting in employeTimePeriodProductSetting)
                            {
                                oldEmployeTimePeriodProductSetting.PayrollProduct = newProduct;
                                changedDicts += Environment.NewLine + "Company: " + company.ActorCompanyId + " EmployeeTimePeriodProductSetting changed id: " + oldEmployeTimePeriodProductSetting.EmployeeTimePeriodProductSettingId.ToString();


                            }

                            #endregion

                            #region save

                            result = base.SaveChanges(entities);

                            if (!result.Success)
                                return result;

                            #endregion
                        }

                        #region Delete old connections

                        ChangeEntityState(oldProduct, SoeEntityState.Deleted);
                        oldProduct.ModifiedBy = "SoftOne System (FMPP)";
                        oldProduct.Modified = DateTime.Now;

                        #endregion

                        #region save

                        result = base.SaveChanges(entities);

                        if (!result.Success)
                            return result;

                        #endregion

                        #region commit


                        if (result.Success)
                            transaction.Complete();

                        #endregion
                    }
                    #endregion

                }
            }

            result.StringValue = newProducts + Environment.NewLine + Environment.NewLine + changedDicts;

            return result;
        }

        #endregion
    }

    public static class ProductExternalUrls
    {
        public static readonly string Sahkonumerot = "http://www.sahkonumerot.fi/";
        public static readonly string RskDatabaseSearch = "https://www.rskdatabasen.se/sok?Query=";
        public static readonly string LviInfo = "https://www.lvi-info.fi/tuotehaku/?q=";
        public static readonly string Bevego = "https://www.bevego.se/kategorier/";
        public static readonly string Lindab = "https://www.lindab.se/article/";
    }
}
