using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class WholeSellerManagerTests:TestBase
    {
        [TestMethod()]
        public void GetCompanyWholesellerPriceListsTest()
        {
            var wm = new WholeSellerManager(null);
            var data = wm.GetCompanyWholesellerPriceLists(7);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void HasCompanyPriceListUpdatesTest()
        {
            var wm = new WholeSellerManager(null);
            var result = wm.HasCompanyPriceListUpdates(7);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void CompanyPriceListsToUpdateTest()
        {
            var wm = new WholeSellerManager(null);
            var data = wm.GetCompanyWholesellerPriceListsToUpdate(7);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetSysWholesellersByCompanyTest()
        {
            var wm = new WholeSellerManager(null);
            var data = wm.GetSysWholesellersByCompany(7);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetSysWholesellerNameTest()
        {
            var wm = new WholeSellerManager(null);
            var data = wm.GetWholesellerName(3);
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetWholsellerPrice()
        {
            var wm = new SysPriceListManager(null);
            var data = wm.GetWholsellerPrices(new List<int>(){ 1972, 629, 1865 }, new List<int>() { 532127 });
            Assert.IsTrue(data != null);
        }

        [TestMethod()]
        public void GetUsedSysPriceListsSmall()
        {
            var wm = new WholeSellerManager(null);
            var data = wm.GetCompanySysWholesellerPriceLists(7);
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        public void GetSysPriceListProviders()
        {
            var data = SysPriceListManager.GetSysPriceListProviders();
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        public void GetSysWholesellersDictTest()
        {
            using (var entities = new Data.CompEntities())
            {
                entities.Connection.Open();

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    var wm = new WholeSellerManager(null);
                    var result = wm.HasCompanyPriceListUpdates(entities, 7);
                    var pl = new SysPriceListManager(null);
                    var data = pl.GetSysWholesellersDict();
                    Assert.IsTrue(data != null);
                }
            }
        }

        [TestMethod()]
        public void GetSysPriceListCode()
        {
            var pl = new SysPriceListManager(null);
            var data = pl.GetProductCodesForWholeseller(new System.Collections.Generic.List<int> { 2, 14, 15 });
            var test = data["QL150B"];
            Assert.IsTrue(data.Any());
        }

        [TestMethod()]
        public void ImportSysPriceList()
        {
            int actorCompanyId = 7;
            string filePath = @"C:\Temp\comfort_ahlsell_test.csv";
            string fileName = Path.GetFileName(filePath);

            var pl = new SysPriceListManager(GetParameterObject(actorCompanyId));
            using (var stream = new MemoryStream(File.ReadAllBytes(filePath)))
            {
                var result = pl.Import(stream,Common.Util.SoeSysPriceListProvider.Comfort_Ahlsell, actorCompanyId, "Prisfil_Ahlsell AB_20250311.csv");
                Assert.IsTrue(result.Success);
            }
        }
    }
}