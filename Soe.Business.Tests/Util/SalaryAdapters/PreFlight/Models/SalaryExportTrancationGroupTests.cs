using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models.Tests
{
    [TestClass()]
    public class SalaryExportTrancationGroupTests
    {
        [TestMethod]
        public void MergeSalaryExportTransaction_InvalidEmployeeCode_ThrowsException()
        {
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = DateTime.Now },
                new SalaryExportTransaction { EmployeeCode = "E2", Code = "P1", Date = DateTime.Now }
            };
            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Day, transactions);
            var group = new SalaryExportTrancationGroup(input);
            group.MergeSalaryExportTransaction();
            Assert.IsTrue(group.ValidationErrors.Any(a => a.Contains("employee")));
        }

        [TestMethod]
        public void MergeSalaryExportTransaction_InvalidDates_ThrowsException()
        {
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = DateTime.Now.AddYears(-11) }
            };
            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Day, transactions);
            var group = new SalaryExportTrancationGroup(input);

            // Act & Assert: expect InvalidDataException to be thrown when merging
            Assert.ThrowsExactly<InvalidDataException>(() => group.MergeSalaryExportTransaction());
        }

        [TestMethod]
        public void MergeSalaryExportTransaction_IncorrectProductCode_ThrowsException()
        {
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = DateTime.Now }
            };
            var input = new SalaryMergeInput("P2", SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Day, transactions);
            var group = new SalaryExportTrancationGroup(input);
            group.MergeSalaryExportTransaction();
            Assert.IsTrue(group.ValidationErrors.Any(a => a.Contains("Product")));
        }

        [TestMethod]
        public void MergeSalaryExportTransaction_ByDay_MergesCorrectly()
        {
            var transactions = new List<SalaryExportTransaction>
        {
            new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 1), Hours = 5, Amount = 100 },
            new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 1), Hours = 3, Amount = 60 }
        };
            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Day, transactions);
            var group = new SalaryExportTrancationGroup(input);

            group.MergeSalaryExportTransaction();

            Assert.AreEqual(1, group.MergedTransactions.Count);
            Assert.AreEqual(8, group.MergedTransactions[0].Hours);
            Assert.AreEqual(160, group.MergedTransactions[0].Amount);
            Assert.AreEqual(new DateTime(2021, 6, 1), group.MergedTransactions[0].Date);
            Assert.IsTrue(group.InitialTransactions.Count() == 2);
            Assert.IsTrue(group.ValidationErrors.Count == 0);
            Assert.IsTrue(group.MergedTransactions[0].EmployeeCode == "E1");
            Assert.IsTrue(group.MergedTransactions[0].Code == "P1");
            Assert.IsTrue(group.MergedTransactions[0].Children.Count == 2);
            Assert.IsTrue(group.MergedTransactions[0].Children.Sum(s => s.Hours) == 8);
        }

        [TestMethod]
        public void MergeSalaryExportTransaction_ByWeek_MergesCorrectly()
        {
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 1), Hours = 5, Amount = 100 },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 3), Hours = 3, Amount = 60 }
            };
            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Week, transactions);
            var group = new SalaryExportTrancationGroup(input);

            group.MergeSalaryExportTransaction();

            Assert.AreEqual(1, group.MergedTransactions.Count);
            Assert.AreEqual(8, group.MergedTransactions[0].Hours);
            Assert.AreEqual(160, group.MergedTransactions[0].Amount);
        }

        [TestMethod]
        public void MergeSalaryExportTransaction_ByMonth_MergesCorrectly()
        {
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 1), Hours = 10, Amount = 200 },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 15), Hours = 10, Amount = 200 }
            };
            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.Month, transactions);
            var group = new SalaryExportTrancationGroup(input);

            group.MergeSalaryExportTransaction();

            Assert.AreEqual(1, group.MergedTransactions.Count);
            Assert.AreEqual(20, group.MergedTransactions[0].Hours);
            Assert.AreEqual(400, group.MergedTransactions[0].Amount);
        }

        [TestMethod]
        public void MergeSalaryExportTransaction_ByCoherentTimeDateIntervals_MergesCorrectly()
        {
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 1), Hours = 5, Amount = 100 },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 2), Hours = 3, Amount = 60 },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = new DateTime(2021, 6, 4), Hours = 2, Amount = 40 }
            };
            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCode, SalaryExportTransactionDateMergeType.CoherentTimeDateIntervals, transactions);
            var group = new SalaryExportTrancationGroup(input);

            group.MergeSalaryExportTransaction();

            // Expecting two merged transactions: one for the consecutive dates (June 1 and 2) and another for the single date (June 4).
            Assert.AreEqual(2, group.MergedTransactions.Count);
            Assert.IsTrue(group.MergedTransactions.Any(t => t.Hours == 8 && t.Amount == 160)); // June 1-2
            Assert.IsTrue(group.MergedTransactions.Any(t => t.Hours == 2 && t.Amount == 40)); // June 4
        }

        [TestMethod]
        public void MergeSalaryExportTransaction_ByCoherentTimeDateIntervals_And_CostCenter()
        {
            var date = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Local);
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 5, Amount = 100, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1" } },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 3, Amount = 60, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1" } },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC2" } },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC3" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(4), Hours = 20, Amount = 395, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC3" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(5), Hours = 1, Amount = 1, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC3" }}
            };
            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCostAllocation, SalaryExportTransactionDateMergeType.CoherentTimeDateIntervals, transactions);
            var group = new SalaryExportTrancationGroup(input);

            group.MergeSalaryExportTransaction();

            // Expecting 5 transactions
            Assert.AreEqual(5, group.MergedTransactions.Count);
            Assert.IsTrue(group.MergedTransactions.Any(t => t.Hours == 8 && t.Amount == 160 && t.CostAllocation.Costcenter == "CC1"));
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC2") == 1);
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC3") == 1);
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 21 && t.Amount == 396 && t.CostAllocation.Costcenter == "CC3") == 1);
        }

        [TestMethod]
        public void MergeSalaryExportTransactions_ByCostCenter()
        {
            DateTime date = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Local);
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 5, Amount = 100, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 3, Amount = 60, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC2"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC3"} }
            };

            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCostAllocation, SalaryExportTransactionDateMergeType.Day, transactions);
            var group = new SalaryExportTrancationGroup(input);
            group.MergeSalaryExportTransaction();

            // Expecting 4 transactions
            Assert.AreEqual(4, group.MergedTransactions.Count);
            Assert.IsTrue(group.MergedTransactions.Any(t => t.Hours == 8 && t.Amount == 160 && t.CostAllocation.Costcenter == "CC1"));
        }

        [TestMethod]
        public void MergeSalaryExportTransactions_ByCostCenterAndProject()
        {
            DateTime date = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Local);
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 5, Amount = 100, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1", Project = "P1"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 3, Amount = 60, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1", Project = "P1"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC2", Project = "P1" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1", Project = "P1"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1", Project = "P2"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1", Project = "P3"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC3", Project = "P1"} }
            };

            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCostAllocation, SalaryExportTransactionDateMergeType.Day, transactions, useCostCenter: true, useProject: true);

            var group = new SalaryExportTrancationGroup(input);
            group.MergeSalaryExportTransaction();

            // Expecting 6 transactions
            Assert.AreEqual(6, group.MergedTransactions.Count);
            // Expecting 2 transactions for the first date (June 1), one with CC1 and P1 and another with CC1 and P1
            Assert.IsTrue(group.MergedTransactions.Any(t => t.Hours == 8 && t.Amount == 160 && t.CostAllocation.Costcenter == "CC1" && t.CostAllocation.Project == "P1"));
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC2" && t.CostAllocation.Project == "P1") == 1);
            // Expecting 1 transaction for the second date (June 3) with CC1 and P1                             
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC1" && t.CostAllocation.Project == "P1") == 1);
            // Expecting 1 transaction for the second date (June 3) with CC1 and P2                             
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC1" && t.CostAllocation.Project == "P2") == 1);
            // Expecting 1 transaction for the second date (June 3) with CC1 and P3                             
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC1" && t.CostAllocation.Project == "P3") == 1);
        }


        [TestMethod]
        public void MergeSalaryExportTransactions_ByProject()
        {
            DateTime date = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Local);
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 5, Amount = 100, CostAllocation = new SalaryExportCostAllocation(){Project = "P1" } },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 3, Amount = 60, CostAllocation = new SalaryExportCostAllocation(){Project = "P1" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Project = "P2"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation() { Project = "P1" } },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 3, CostAllocation = new SalaryExportCostAllocation() { Project = "P2" } },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation() { Project = "P3" } }
            };

            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCostAllocation, SalaryExportTransactionDateMergeType.Day, transactions, useCostCenter: false, useProject: true);
            var group = new SalaryExportTrancationGroup(input);
            group.MergeSalaryExportTransaction();

            // Expecting 5 transactions
            Assert.AreEqual(5, group.MergedTransactions.Count);
            // Expecting 2 transactions for the first date (June 1), one with P1 and another with P2
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 8 && t.Amount == 160 && t.CostAllocation.Project == "P1") == 1);
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Project == "P2" && t.Date == date) == 1);
            // Expecting 1 transaction for the second date (June 3) with P1
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Project == "P1") == 1);
            // Expecting 1 transaction for the second date (June 3) with P2
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 3 && t.CostAllocation.Project == "P2") == 1);
            // Expecting 1 transaction for the second date (June 3) with P3
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Project == "P3") == 1);
        }

        [TestMethod]
        public void MergeSalaryExportTransactions_ByCostCenterAndMonth()
        {
            var date = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Local);
            var transactions = new List<SalaryExportTransaction>
            {
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 5, Amount = 100, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1" } },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 3, Amount = 60, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date, Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC2"} },
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC1" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddDays(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC3" }},
                new SalaryExportTransaction { EmployeeCode = "E1", Code = "P1", Date = date.AddMonths(2), Hours = 2, Amount = 40, CostAllocation = new SalaryExportCostAllocation(){Costcenter = "CC3" }}
            };

            var input = new SalaryMergeInput("P1", SalaryExportTransactionGroupType.ByCostAllocation, SalaryExportTransactionDateMergeType.Month, transactions, useCostCenter: true);

            var group = new SalaryExportTrancationGroup(input);
            group.MergeSalaryExportTransaction();

            // Expecting 3 transactions
            Assert.AreEqual(4, group.MergedTransactions.Count);
            // Expecting 2 transactions for the first date (June 1), one with CC1 and another with CC2 with dateFrom and dateTo set to the first and last date of the grouped transactions
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 10 && t.Amount == 200 && t.CostAllocation.Costcenter == "CC1" && t.FromDate == date && t.ToDate == date.AddDays(2)) == 1);
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC2" && t.FromDate == date && t.ToDate == date) == 1);
            // Expecting 1 transaction for the second date (June 3) with CC1                                     
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC1" && t.Date == date.AddDays(2) && t.FromDate == date.AddDays(2) && t.ToDate == date.AddDays(2)) == 0);
            // Expecting 0 transactions for the second date (June 3) with CC3                                     
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC3" && t.Date == date.AddDays(2) && t.FromDate == date.AddDays(2) && t.ToDate == date.AddDays(2)) == 1);
            // Expecting 1 transaction for the third date (August 1) with CC3                                    
            Assert.IsTrue(group.MergedTransactions.Count(t => t.Hours == 2 && t.Amount == 40 && t.CostAllocation.Costcenter == "CC3" && t.Date == date.AddMonths(2) && t.FromDate == date.AddMonths(2) && t.ToDate == date.AddMonths(2)) == 1);
        }
    }
}