using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.PaymentIO.SEPA;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soe.Business.Test
{
    [TestClass]
    public class SEPAExportTest
    {
        [TestMethod]
        public void FormatUtcOffset_Test()
        {
            TimeSpan ts = new TimeSpan(10, 25, 44);
            string fts = ts.ToString("hh':'mm");
            Assert.AreEqual("10:25", fts);
        }

        [TestMethod]
        public void FormatDate_Test()
        {
            DateTime dt = new DateTime(2011, 04, 18);
            string fdt = dt.ToString("yy-MM-dd");
            Assert.AreEqual("11-04-18", fdt);
        }

        [TestMethod]
        public void Enumerable_Take_Test()
        {
            int[] numbers = new int[] { 1, 2, 3 };
            var fiveNumbers = numbers.Take(5);
            Assert.AreEqual(3, fiveNumbers.Count());
        }

        [TestMethod]
        public void FormatDecimal_Test()
        {
            Decimal dec = new decimal(123456.7890);
            string decStr = SEPABase.FormatAmount(dec);
            Assert.AreEqual("123456.79", decStr);
        }
    }
}
