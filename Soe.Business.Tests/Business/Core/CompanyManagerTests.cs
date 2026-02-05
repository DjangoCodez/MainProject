using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class CompanyManagerTests
    {
        [TestMethod()]
        public void GetActorCompanyIdFromCombinedKeyTest()
        {
            var key = -1705097198;
            var resultCompany = CompanyManager.GetActorCompanyIdFromCombinedKey(key);
            var resultSysCompDb = CompanyManager.GetSysCompDbIdFromCombinedKey(key);
            Assert.AreEqual(432018, resultCompany);
            Assert.AreEqual(18, resultSysCompDb);
        }

        [TestMethod()]
        public void GenerateUniqueKeyTest()
        {
            var testCompany = 1;

            while (testCompany <= 100000000)
            {
                var key = CompanyManager.GenerateUniqueKey(testCompany, 18);
                var company = CompanyManager.GetActorCompanyIdFromCombinedKey(key);
                var sysCompDb = CompanyManager.GetSysCompDbIdFromCombinedKey(key);

                Assert.AreEqual(testCompany, company);
                Assert.AreEqual(18, sysCompDb);

                testCompany *= 10;
            }
        }
    }
}