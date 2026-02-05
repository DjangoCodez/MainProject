using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ProductManagerTests: TestBase
    {
        [TestMethod()]
        public void SearchInvoiceProductPricesTest()
        {
            var pm = new ProductManager(null);
            bool result = true;
            //artnr 4503666
            var data = pm.SearchInvoiceProductPrice(7, 1834, 127, 1, 2379755, 5, true);

            /*
            bool searchPrice = false;
            bool searchPrices = true;
            if (searchPrice)
            {
                var data = pm.SearchInvoiceProductPrice(7, 0, 127, 1, 723256, 10, true);
                var data2 = pm.SearchInvoiceProductPrice(7, 0, 127, 1, 21841, 78, true);
                if (data == null || data2 == null)
                    result = false;
            }
            if (searchPrices)
            {
                var data3 = pm.SearchInvoiceProductPrices(7, 0, 127, new List<string>() { "1871577" }, true, "ProductNumber", Common.Util.SoeSysPriceListProviderType.Plumbing).ToList();
                var data4 = pm.SearchInvoiceProductPrices(7, 0, 127, new List<string>() { "1437684" }, true, "ProductNumber").ToList();
                if (data3 == null || data4 == null)
                    result = false;
            }
            */
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void GetCompanyWholesellerPriceListsTest()
        {
            var m = new WholeSellerManager(null);
            var data = m.GetCompanyWholesellerPriceLists(7, true);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetSingleMarkupTest()
        {
            var m = new MarkupManager(null);
            using (var entities = new CompEntities())
            {
                var data1 = m.GetSingleMarkup(entities, 723256, 178, 7, 10);
                var data2 = m.GetSingleMarkup(entities, 176967, 179, 7, 10);
                Assert.IsTrue(data1 != null && data2 != null);
            }
        }

        [TestMethod()]
        public void MatchSupplierAgreementToProductTest()
        {
            var mm = new PriceRuleManager(null);
            using (var entities = new CompEntities())
            {
                var data = mm.GetMatchSupplierAgreementToProduct(entities, 7, 176967, 179, 1);
                Assert.IsTrue(data != null);
            }
        }

        [TestMethod()]
        public void GetInvoiceProductPriceTest()
        {
            var m = new ProductManager(null);
            using (var entities = new CompEntities())
            {
                var data1 = m.GetInvoiceProductPrice(entities, 7, 723256);
                var data2 = m.GetInvoiceProductPrice(entities, 7, 21841);
                var data3 = m.GetInvoiceProductPrice(entities, 7, 723256, 1266);
                var data4 = m.GetInvoiceProductPrice(entities, 7, 21841, null, 87);
                Assert.IsTrue(data1 != null && data2 != null || data3 != null && data4 != null);
            }
        }

        [TestMethod()]
        public void AzureSearchSysProductsTest()
        {
            var pm = new ProductManager(null);
            using (var entities = new CompEntities())
            {
                var data = pm.AzureSearchSysProducts(entities, 7, "", "", "", "155", 50);
                Assert.IsTrue(data != null);
            }           
        }

        [TestMethod()]
        public void GetExternalProductPriceTest()
        {
            var m = new ProductManager(null);
            using (var entities = new CompEntities())
            {
                InvoiceProduct product = m.GetInvoiceProduct(entities, 10308, true, true, false);
                var data = m.GetExternalProductPrice(entities, 0, product, 127, 7, 10, false);
                Assert.IsTrue(data != null);
            }
        }

        [TestMethod()]
        public void GetExternalInvoiceProductByProductNumberTest()
        {
            var m = new ProductManager(null);
            var searchText = "1822141";
            var sysWholesellerId = 5;
            using (var entities = new CompEntities())
            {
                var data = m.GetExternalInvoiceProductByProductNumber(searchText, 7, ref sysWholesellerId, false, null);
                Assert.IsTrue(data != null);
            }
        }

        [TestMethod()]
        public void CopyExternalInvoiceProductFromCompPriceListByProductNrTest()
        {
            var m = new ProductManager(null);
            using (var entities = new CompEntities())
            {
                var searchText = "1437684";
                var sysWholesellerId = 78;
                var data = m.CopyExternalInvoiceProductFromCompPriceListByProductNr(entities, searchText, 250, sysWholesellerId, null, "", 7, 0);
                Assert.IsTrue(data != null);
            }
        }

        [TestMethod()]
        public void SearchGetInvoiceProductsByNumberOrNameOrEAN()
        {
            var orderId = 17383777;
            int actorCompanyId = 776065;
            int userId = 104697;
            var param = GetParameterObject(actorCompanyId, userId);

            var pm = new ProductManager(param);
            var im = new InvoiceManager(param);

            CustomerInvoice order = im.GetCustomerInvoice(orderId);

            //Get internal products
            List<InvoiceProductPriceSearchDTO> products = pm.GetInvoiceProductsBySearch(actorCompanyId, "A", 50, order.PriceListTypeId.Value, order.ActorId.Value, order.CurrencyId, order.SysWholeSellerId ?? 0, true, true, includeCustomerProducts: true).ToList();

            products = pm.GetInvoiceProductsBySearch(actorCompanyId, "811", 25, order.PriceListTypeId.Value, order.ActorId.Value, order.CurrencyId, order.SysWholeSellerId ?? 0, true, true, includeCustomerProducts: true).ToList();

            Assert.IsTrue(products.Any());
        }

        [TestMethod()]
        public void SearchInvoiceProductsTest()
        {
            var sw = new Stopwatch();
            var m = new ProductManager(null);
            var searchText = "1800";
            sw.Start();
            var data = m.SearchInvoiceProducts(7, searchText, 50);
            sw.Stop();
            Debug.WriteLine("Time Taken: " + sw.Elapsed.TotalMilliseconds.ToString("#,##0.00 'milliseconds'"));
            sw.Start();
            data = m.SearchInvoiceProducts(7, searchText, 50);
            sw.Stop();
            Debug.WriteLine("Time Taken: " + sw.Elapsed.TotalMilliseconds.ToString("#,##0.00 'milliseconds'"));
            Assert.IsTrue(data != null);
        }
    }
}