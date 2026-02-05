using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class CountryCurrencyManagerTests
    {
        [TestMethod()]
        public void TestFetchCurrenciesFromECB()
        {
            var m = new CountryCurrencyManager(null);

            var currencyFilter = new List<string> { 
                "SEK",
                "EUR",
            };

            var success = m.TryGetCurrencyRatesFromECB(out List<SysCurrencyRateDTO> dtos, currencyFilter);
            Assert.IsTrue(success);
            Assert.IsTrue(dtos != null && dtos.Count > 0, "No currency rates fetched from ECB");
            Assert.IsTrue(dtos.All(x => x.Rate > 0), "Some currency rates are zero or negative");

            var eurToSek = dtos.FirstOrDefault(r => r.CurrencyFrom == TermGroup_Currency.EUR && r.CurrencyTo == TermGroup_Currency.SEK);
            Assert.IsTrue(eurToSek.Rate > 1 && eurToSek.Rate < 100, "EUR to ");

            var sekToEur = dtos.FirstOrDefault(r => r.CurrencyFrom == TermGroup_Currency.SEK && r.CurrencyTo == TermGroup_Currency.EUR);
            Assert.IsTrue(sekToEur.Rate > 0 && sekToEur.Rate < 1, "SEK to EUR rate should be less than 1");
        }

    }
}
