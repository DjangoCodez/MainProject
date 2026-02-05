using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models.Tests
{
    [TestClass()]
    public class PolScheduleTransactionTests
    {
        [TestMethod()]
        public void PolScheduleStringCorrectOnFreeDay()
        {
            // For string result 03434040000080007170301 170301*****
            var transaction = new PolScheduleTransaction()
            {
                Foretag = 3,
                Period = new DateTime(2014, 4, 1),
                AnstNr = 80007,
                Begynnelsedatum = new DateTime(2017, 3, 1),
                Dagkod = 0,
            };
            Assert.IsTrue(transaction.IsValid());
            var stringResult = transaction.ToPolString();
            Assert.AreEqual("03434040000080007170301    170301*****" + Environment.NewLine, stringResult);
        }

        [TestMethod()]
        public void PolScheduleStringCorrectOnWorkDay()
        {
            // For string result 03434040000080007170301 170301*****
            var transaction = new PolScheduleTransaction()
            {
                Foretag = 3,
                Period = new DateTime(2014, 4, 1),
                AnstNr = 80007,
                Begynnelsedatum = new DateTime(2017, 3, 1),
                Dagkod = 8,
            };

            Assert.IsTrue(transaction.IsValid());
            var stringResult = transaction.ToPolString();
            Assert.AreEqual("03434040000080007170301    17030108,00", stringResult);
        }

        [TestMethod()]
        public void GetPolTransactionStringsWithMultipleWeeks()
        {
            List<PolScheduleTransaction> transactionsForMonth = new List<PolScheduleTransaction>();
            DateTime startDate = new DateTime(2014, 4, 1);
            DateTime endDate = new DateTime(2014, 4, 30);
            DateTime currentDate = startDate;

            while (currentDate >= startDate && currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                var transaction = new PolScheduleTransaction()
                {
                    Foretag = 3,
                    Period = startDate,
                    AnstNr = 80007,
                    Begynnelsedatum = currentDate,
                    Dagkod = 8,
                    Slutdatum = currentDate,
                };
                transactionsForMonth.Add(transaction);
                currentDate = currentDate.AddDays(1);
            }

            var resultDict = PolScheduleTransaction.GetTransactionsForWeeks(transactionsForMonth);

            Assert.AreEqual(4, resultDict.Count);
            Assert.AreEqual(7, resultDict.First().Value.Count);

            int counter = 1;
            foreach (var week in resultDict)
            {
                var fistDate = week.Value.OrderBy(o => o.Begynnelsedatum).First().Begynnelsedatum;
                var lastDate = week.Value.OrderBy(o => o.Begynnelsedatum).Last().Begynnelsedatum;

                // make sure lastdate is on sunday if lastdate is earlier than the last date of the month. Lastdate can not be after enddate.
                var lastDayOfWeek = CalendarUtility.GetLastDateOfWeek(lastDate);
                if (lastDayOfWeek < endDate)
                    lastDate = lastDayOfWeek;
                else
                    lastDate = endDate;

                var polOneWeek = week.Value.First().ToPolStringForOneWeek(fistDate, lastDate, transactionsForMonth);

                if (counter == 1)
                {
                    Assert.AreEqual("03434040000080007140401    14040608,0008,0008,0008,00**********", polOneWeek);
                }
                else if (counter == 2)
                {
                    Assert.AreEqual("03434040000080007140407    14041308,0008,0008,0008,0008,00**********", polOneWeek);
                }
                else if (counter == 3)
                {
                    Assert.AreEqual("03434040000080007140414    14042008,0008,0008,0008,0008,00**********", polOneWeek);
                }
                else if (counter == 4)
                {
                    Assert.AreEqual("03434040000080007140421    14042708,0008,0008,0008,0008,00**********", polOneWeek);
                }
                else if (counter == 5)
                {
                    Assert.AreEqual("03434040000080007140428    14043008,0008,0008,00", polOneWeek);
                }
                counter++;
            }

        }
    }
}