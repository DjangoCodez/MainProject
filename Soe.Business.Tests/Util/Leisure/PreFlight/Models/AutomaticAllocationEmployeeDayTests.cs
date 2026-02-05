using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Util.Leisure.PreFlight.Models
{
    [TestClass]
    public class AutomaticAllocationEmployeeDayTests
    {
        [TestMethod]
        public void TestIsWorkDay()
        {
            var workDay = new AutomaticAllocationEmployeeDay
            {
                Date = DateTime.Now,
                StartTime = DateTime.Now.AddHours(9),
                StopTime = DateTime.Now.AddHours(17)
            };

            Assert.IsTrue(workDay.IsWorkDay());
        }

        [TestMethod]
        public void TestIsNotWorkDay()
        {
            var nonWorkDay = new AutomaticAllocationEmployeeDay
            {
                Date = DateTime.Now,
                StartTime = DateTime.Now.AddHours(17),
                StopTime = DateTime.Now.AddHours(9)
            };

            Assert.IsFalse(nonWorkDay.IsWorkDay());
        }

        [TestMethod]
        public void TestIsFreeDay()
        {
            var freeDay = new AutomaticAllocationEmployeeDay
            {
                Date = DateTime.Now,
                StartTime = DateTime.Now,
                StopTime = DateTime.Now
            };
            Assert.IsTrue(freeDay.IsPossibleLeisureDay());
        }

        [TestMethod]
        public void TestGetFirstAvailableDay()
        {
            var m = new DateTime(2025, 3, 31);
            var week = new AutomaticAllocationEmployeeWeek
            {
                Monday = m,
                EmployeeDays = new List<AutomaticAllocationEmployeeDay>
                {
                   new AutomaticAllocationEmployeeDay(){Date = m, StartTime = m.AddHours(9), StopTime = m.AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(1), StartTime = m.AddDays(1).AddHours(9), StopTime = m.AddDays(1).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(2), StartTime = m.AddDays(2).AddHours(9), StopTime = m.AddDays(2).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(3), StartTime = m.AddDays(3).AddHours(9), StopTime = m.AddDays(3).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(4), StartTime = m.AddDays(4).AddHours(9), StopTime = m.AddDays(4).AddHours(17) },
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(5), StartTime = m.AddDays(5), StopTime = m.AddDays(5) }, //Free day
                   new AutomaticAllocationEmployeeDay(){Date = m.AddDays(6), StartTime = m.AddDays(6), StopTime = m.AddDays(6)} // Free day
                }
            };

            var firstAvailableDay = week.GetFirstPossibleLeisureDay(new List<AutomaticAllocationEmployee>(), LeisureCodeType.V);
            Assert.AreEqual(m.AddDays(5), firstAvailableDay);
        }
    }
}

