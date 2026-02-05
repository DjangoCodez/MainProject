using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class Misc
    {
        [TestMethod()]
        public void CalculateTimeAccumulatorYearBalanceTest()
        {
            int actorCompanyId = 628935;
            TimeAccumulatorManager tam = new TimeAccumulatorManager(null);
            tam.CalculateTimeAccumulatorYearBalance(actorCompanyId);
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void GetCountries()
        {
            var cm = new CountryCurrencyManager(null);
            var co = new CompanyManager(null);

            using (var entities = new CompEntities())
            {
                entities.Connection.Open();
                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    var company = co.GetCompanySysCountryId(entities, 7);
                    var x = cm.GetEUSysCountrieIds(DateTime.Today);
                }
            }

            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void FindIrregularAbsence()
        {
            int actorCompanyId = 701609;

            TimeRuleManager pm = new TimeRuleManager(null);
            TimeTransactionManager ttmm = new TimeTransactionManager(null);
            StringBuilder sb = new StringBuilder();
            DateTime from = DateTime.Today.AddDays(-365);
            DateTime to = DateTime.Today.AddDays(-265);
            List<TimePayrollTransactionDTO> suspectDTOs = new List<TimePayrollTransactionDTO>();
            var timeAbsenceRulesInput = new GetTimeAbsenceRulesInput(actorCompanyId)
            {
                LoadRows = true,
                LoadTimeCode = true,
            };
            List<TimeAbsenceRuleHead> timeAbsenceRules = pm.GetTimeAbsenceRules(timeAbsenceRulesInput);
            List<Product> products = new List<Product>();
            List<Employee> employees = new List<Employee>();
            foreach (var head in timeAbsenceRules)
            {
                using (CompEntities entities = new CompEntities())
                {
                    if (head.TimeCode.Name.ToLower().Contains("sjuk"))
                    {
                        List<int> payrollProductIds = new List<int>();

                        foreach (var row in head.TimeAbsenceRuleRow.OrderBy(o => o.Start))
                        {
                            if (row.PayrollProductId.HasValue)
                                payrollProductIds.Add(row.PayrollProductId.Value);
                        }

                        payrollProductIds = payrollProductIds.Distinct().ToList();
                        List<int> employeeIds = entities.TimePayrollTransaction.Where(w => payrollProductIds.Contains(w.ProductId) && w.TimeBlockDate.Date > from && w.TimeBlockDate.Date < to && w.State == (int)SoeEntityState.Active).Select(s => s.EmployeeId).Distinct().ToList();
                        products = entities.Product.Where(w => payrollProductIds.Contains(w.ProductId)).ToList();
                        employees = entities.Employee.Include("ContactPerson").Where(w => employeeIds.Contains(w.EmployeeId)).ToList();


                        var transactions = ttmm.GetTimePayrollTransactionDTOForReport(from, to, employeeIds, actorCompanyId);

                        foreach (var pers in transactions)
                        {
                            var validTrans = pers.Value.Where(w => payrollProductIds.Contains(w.PayrollProductId)).OrderBy(o => o.Date).ToList();

                            foreach (var onDate in validTrans.GroupBy(g => g.Date))
                            {
                                foreach (var row in head.TimeAbsenceRuleRow.OrderBy(o => o.Start))
                                {
                                    var nextDay = onDate.Key.Value.AddDays(1);

                                    if (validTrans.Any(a => a.Date == nextDay))
                                    {

                                        var hitOnRows = head.TimeAbsenceRuleRow.Where(w => w.PayrollProductId != null && validTrans.First(f => f.Date == nextDay).PayrollProductId == w.PayrollProductId).ToList();

                                        foreach (var hitOnRow in hitOnRows)
                                        {
                                            if (hitOnRow.Start < row.Start)
                                            {
                                                suspectDTOs.Add(onDate.First());
                                                suspectDTOs.Add(validTrans.First(a => a.Date == nextDay));
                                            }
                                        }
                                    }

                                    if (row.PayrollProductId.HasValue)
                                        payrollProductIds.Add(row.PayrollProductId.Value);
                                }
                            }
                        }
                    }

                    foreach (var group in suspectDTOs.GroupBy(g => g.EmployeeId))
                    {
                        var employee = employees.First(f => f.EmployeeId == group.Key);

                        foreach (var item in group.OrderBy(o => o.Date))
                        {
                            var product = products.FirstOrDefault(f => f.ProductId == item.PayrollProductId);
                            sb.Append($"{employee.NumberAndName} {CalendarUtility.ToShortDateString(item.Date)} {product.Number} {product.Name}");
                        }
                    }
                }
            }

            var result = sb.ToString();
            Assert.IsTrue(result != null);
        }


        [TestMethod()]
        public void RunPerformanceTest()
        {
            ConfigurationSetupUtil.Init();
            const int iterations = 50;
            Console.WriteLine($"Running performance test with {iterations} iterations...\n");

            // Test with a new context for each operation
            Console.WriteLine("Testing with new context each time...");
            var newContextTime = MeasureTime(() =>
            {
                using (var entities = Comp.CreateCompEntities(true))
                {

                    var accounts = entities.Account.Take(1000).ToList();

                    for (int i = 0; i < iterations; i++)
                    {

                        entities.Account.NoTracking();
                        entities.Account.MergeOption = MergeOption.NoTracking; foreach (var account in accounts)
                        {
                            var name = account.Name; // Pure read operation

                        }
                    }
                }
            });
            Console.WriteLine($"Time taken with new context each time: {newContextTime} ms\n");

            // Test with a single reused context
            Console.WriteLine("Testing with reused context...");
            var reusedContextTime = MeasureTime(() =>
            {
                using (var c = new CompEntities())
                {
                    var accounts = c.Account.Take(1000).ToList();


                    for (int i = 0; i < iterations; i++)
                    {
                        foreach (var account in accounts)
                        {
                            var name = account.Name; // Pure read operation
                            account.Name = "Test"+i.ToString();
                        }
                    }
                }
            });
            Console.WriteLine($"Time taken with reused context: {reusedContextTime} ms\n");

            // Compare results
            Console.WriteLine($"Performance difference: {newContextTime - reusedContextTime} ms");
        }

        private long MeasureTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

    }
}