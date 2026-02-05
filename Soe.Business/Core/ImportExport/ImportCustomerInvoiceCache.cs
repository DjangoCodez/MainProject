using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class ImportCustomerInvoiceCache
    {

        private readonly Dictionary<string, AccountStd> AccountStdByNrCache = new Dictionary<string, AccountStd>();
        private readonly Dictionary<int, AccountStd> AccountStdByIdCache = new Dictionary<int, AccountStd>();
        private readonly Dictionary<string, InvoiceProduct> InvoiceProductCache = new Dictionary<string, InvoiceProduct>();
        private readonly Dictionary<string, Customer> CustomerCache = new Dictionary<string, Customer>();
        private InvoiceProduct MiscProduct;
        private List<AccountInternal> AccountInternals;
        private readonly AccountManager accountManager;
        private readonly ProductManager productManager;

        public int DefaultWholesellerId { get; private set; }
        public List<string> SpecialFunctionality { get; private set; }
        public Dictionary<string, string> SpecialFunctionalityDict { get; private set; }
        public List<SysWholeseller> SysWholesellers { get; private set; }
        public List<AccountDim> AccountDims { get; private set; }
        public ILookup<int,PriceListTypeGridDTO> PriceListTypes { get; private set; }
        public CustomerInvoiceHeadIO CustomerInvoiceHeadIO { get; private set; }

        public ImportCustomerInvoiceCache(AccountManager accountManager, ProductManager productManager)
        {
            this.accountManager = accountManager;
            this.productManager = productManager;
        }

        public void FillCache(int actorCompanyId, int importId, ImportExportManager importExportManager, SettingManager settingManager, WholeSellerManager wholeSellerManager, ProductPricelistManager pricelistManager)
        {
            using (CompEntities entities = new CompEntities())
            {
                SpecialFunctionality = importExportManager.GetSpecialFunctionality(actorCompanyId, importId);
                SpecialFunctionalityDict = importExportManager.GetSpecialFunctionalityDict(actorCompanyId, importId);

                DefaultWholesellerId = settingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultWholeseller, 0, actorCompanyId, 0);

                if (DefaultWholesellerId == 0)
                {
                    SysWholesellers = wholeSellerManager.GetSysWholesellersByCompany(actorCompanyId);

                    if (SysWholesellers.Any())
                        DefaultWholesellerId = SysWholesellers.First().SysWholesellerId;
                }

                AccountDims = accountManager.GetAccountDimsByCompany(entities, actorCompanyId, loadAccounts: true);
                PriceListTypes = pricelistManager.GetPriceListTypesForGrid(entities, actorCompanyId).ToLookup(x=> x.PriceListTypeId);
            }
        }

        public void ClearCachePerConnection(CustomerInvoiceHeadIO customerInvoiceHeadIO)
        {
            AccountStdByNrCache.Clear();
            AccountStdByIdCache.Clear();
            InvoiceProductCache.Clear();
            MiscProduct = null;
            AccountInternals = null;
            CustomerInvoiceHeadIO = customerInvoiceHeadIO;
        }
        public AccountInternal GetAccountInternal(CompEntities entities, int actorCompanyId, int accountId)
        {
            if (AccountInternals == null)
            {
                AccountInternals = accountManager.GetAccountInternals(entities, actorCompanyId, true);
            }

            return AccountInternals.FirstOrDefault(a => a.AccountId == accountId);
        }

        public Customer GetCustomer(CustomerManager customerManager, string customerNr, int actorCompanyId)
        {
            Customer customer;
            if (CustomerCache.TryGetValue(customerNr.ToLower(), out customer))
            {
                return customer;
            }

            customer = customerManager.GetCustomerByNr(actorCompanyId, customerNr, null, true);
            if (customer != null && customer.State == (int)SoeEntityState.Active)
            {
                CustomerCache.Add(customerNr.ToLower(), customer);
            }
            
            return customer;
        }

        public AccountStd GetAccountStdByNr(CompEntities entities, string accountNr, int actorCompanyId)
        {
            AccountStd account;
            if (AccountStdByNrCache.TryGetValue(accountNr, out account))
            {
                return account;
            }

            account = accountManager.GetAccountStdByNr(entities, accountNr, actorCompanyId);
            if (account != null)
            {
                AccountStdByNrCache.Add(accountNr, account);
                if (!AccountStdByIdCache.ContainsKey(account.AccountId))
                {
                    AccountStdByIdCache.Add(account.AccountId, account);
                }
            }

            return account;
        }

        public AccountStd GetAccountStdById(CompEntities entities, int accountId, int actorCompanyId)
        {
            AccountStd account;
            if (AccountStdByIdCache.TryGetValue(accountId, out account))
            {
                return account;
            }

            account = accountManager.GetAccountStd(entities, accountId, actorCompanyId, true, false);
            if (account != null)
            {
                AccountStdByIdCache.Add(accountId, account);
            }

            return account;
        }

        public InvoiceProduct GetMiscProduct(CompEntities entities, int actorCompanyId)
        {
            if (MiscProduct == null)
                MiscProduct = productManager.GetInvoiceProductFromSetting(entities, CompanySettingType.ProductMisc, actorCompanyId, false, true);

            return MiscProduct;
        }

        public InvoiceProduct GetInvoiceProduct(CompEntities entities, string productNr, string productName, int actorCompanyId) {

            // Prio 1 : By productnr
            InvoiceProduct product;
            if (!string.IsNullOrEmpty(productNr))
            {
                if (InvoiceProductCache.TryGetValue(productNr.ToLower(), out product))
                {
                    return product;
                }

                product = productManager.GetInvoiceProductByProductNr(entities, productNr, actorCompanyId);
                if (product != null)
                {
                    InvoiceProductCache.Add(productNr.ToLower(), product);
                    return product;
                }
            }

            //Prio 2: By productname
            if (!string.IsNullOrEmpty(productName))
            {
                product = productManager.GetInvoiceProductByProductName(entities, productName, actorCompanyId);
                //check for same name but different numers
                if (product != null && (!InvoiceProductCache.ContainsKey(product.Number.ToLower())))
                {
                    InvoiceProductCache.Add(product.Number.ToLower(), product);
                    return product;
                }
            }

            return null;
        }
    }
}
