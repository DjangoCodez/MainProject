using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    public class PAxmaladapter2_1 : ISalaryAdapter
    {
        private readonly List<TransactionItem> allTransactionItems;
        private readonly List<TransactionItem> timeTransactionItems;
        private readonly List<TransactionItem> outlayTransactions;
        private readonly List<ScheduleItem> scheduleItems;
        private readonly List<PayrollProduct> payrollProducts;
        private readonly List<int> employeeIds;
        private readonly DateTime startDate;
        private readonly DateTime stopDate;
        private readonly string externalExportId;

        #region Constructors

        public PAxmaladapter2_1(List<PayrollProduct> payrollProducts, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<int> employeeIds,DateTime startDate, DateTime stopDate, string externalExportId)
        {
            this.allTransactionItems = payrollTransactions;
            this.timeTransactionItems = payrollTransactions.Where(p => !p.IsAddition() && !p.IsDeduction()).ToList();
            this.outlayTransactions = payrollTransactions.Where(p => p.IsAddition() || p.IsDeduction()).ToList();
            this.scheduleItems = scheduleItems;
            this.payrollProducts = payrollProducts;
            this.employeeIds = employeeIds;
            this.startDate = startDate;
            this.stopDate = stopDate;
            this.externalExportId = externalExportId.Trim().ToLower();
        }

        #endregion

        public byte[] TransformSalary(XDocument baseXml)
        {
            XDocument doc = CreateDocumentDeclaration();
            doc.Add(CreateSalaryData());
            MemoryStream stream = new MemoryStream();
            doc.Save(stream);
            return stream.ToArray();
        }

        #region XML structure methods

        private XDocument CreateDocumentDeclaration()
        {
            XDocument doc = new XDocument(
                 new XDeclaration("1.0", Constants.ENCODING_LATIN1_NAME, "true"));
               
            return doc;
        }

        private XElement CreateSalaryData()
        {
            XNamespace name = "http://www.paxml.se/2.1/paxml.xsd";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            XElement schedules = new XElement("schematransaktioner");
            XElement times = new XElement("tidtransaktioner");
            XElement outlay = new XElement("lonetransaktioner");

            XElement salaryData = new XElement("paxml",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "noNamespaceSchemaLocation", name));
           
            salaryData.Add(CreateHeader());
            salaryData.Add(CreateDimension(GetDistinctDims(allTransactionItems)));

            List<Tuple<String, String, String>> distinctResultUnits = SalaryExportUtil.GetDistinctAccounts(allTransactionItems);
            if (distinctResultUnits.Any())
                salaryData.Add(CreateResultUnits(distinctResultUnits));

            salaryData.Add(CreatePayrollProducts(payrollProducts));

            foreach (var employeeIdTimes in employeeIds)
            {
                List<TransactionItem> transactionItemsForEmployee = timeTransactionItems.Where(s => s.EmployeeId == employeeIdTimes.ToString()).ToList();

                if (transactionItemsForEmployee.Count > 0)
                {
                    String empNr2 = string.Empty;
                    TransactionItem firstTransactionItem = transactionItemsForEmployee.FirstOrDefault();
                    empNr2 = firstTransactionItem != null ? firstTransactionItem.EmployeeNr : string.Empty;

                    foreach (var item in transactionItemsForEmployee)
                    {
                        times.Add(CreateTime(item, empNr2));
                    }
                }
            }
            if (times.HasElements)
                salaryData.Add(times);

            if (outlayTransactions.Any())
            {
                salaryData.Add(outlay);

                foreach (var employeeIdOutlay in employeeIds)
                {
                    List<TransactionItem> transactionItemsForEmployee = outlayTransactions.Where(s => s.EmployeeId == employeeIdOutlay.ToString()).ToList();

                    if (transactionItemsForEmployee.Count > 0)
                    {
                        String empNr3 = string.Empty;
                        TransactionItem firstTransactionItem = transactionItemsForEmployee.FirstOrDefault();
                        empNr3 = firstTransactionItem != null ? firstTransactionItem.EmployeeNr : string.Empty;


                        foreach (var item in transactionItemsForEmployee)
                        {
                            outlay.Add(CreateRegOutlay(item, empNr3));
                        }
                    }
                }
            }
            if (this.externalExportId.Contains("!noschedule!"))
                return salaryData; 

            foreach (var employeeIdSchedules in employeeIds)
            {
                List<ScheduleItem> scheduleItemsForEmployee = scheduleItems.Where(s => s.EmployeeId == employeeIdSchedules.ToString()).ToList();

                if (scheduleItemsForEmployee.Count > 0)
                {
                    String empNr = string.Empty;
                    ScheduleItem firstScheduleItem = scheduleItemsForEmployee.FirstOrDefault();
                    empNr = firstScheduleItem != null ? firstScheduleItem.EmployeeNr : string.Empty; 

                    XElement employeeSchedule = new XElement("schema",
                        new XAttribute("anstid", GetString(empNr)));
                    for (var day = startDate; day.Date <= stopDate; day = day.AddDays(1))
                    {
                        ScheduleItem item = new ScheduleItem()
                        {
                            TotalMinutes = 0,
                            TotalBreakMinutes = 0,
                            Date = day,
                            StartDate = day,
                            StopDate = day
                        };
                        var schedule = scheduleItemsForEmployee.FirstOrDefault(w => w.Date == day);
                        if (schedule != null)
                            item = schedule;
                     
                        if (this.externalExportId.Contains("!tidschema!"))
                            employeeSchedule.Add(CreateNormalWorkTimeWithTimes(item.Date, item.TotalMinutes - item.TotalBreakMinutes, item.StartDate, item.StopDate));
                        else
                            employeeSchedule.Add(CreateNormalWorkTime(item.Date, item.TotalMinutes - item.TotalBreakMinutes));
                        
                        
                    }
                    schedules.Add(employeeSchedule);
                }
            }

            if (schedules.HasElements)
                salaryData.Add(schedules);

            return salaryData;
        }

        private XElement CreateHeader()
        {
            XElement header = new XElement("header",
                new XElement("version", GetString("2.1")),
                new XElement("datum", GetString(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))));
                  return header;
        }

        private XElement CreateDimension(List<Tuple<String, String>> distinctDims)
        {
            XElement dimensions = new XElement("dimensioner");
            foreach (var dim in distinctDims) {
                dimensions.Add(new XElement("dimension",
                new XAttribute("dim", GetString(dim.Item1)),
                new XAttribute("namn", GetString(dim.Item2))));
            }
            return dimensions;
        }

        private XElement CreatePayrollProducts(List<PayrollProduct> payrollProducts)
        {
            XElement payrollProductsElement = new XElement("koder");
            foreach (PayrollProduct payrollProduct in payrollProducts)
            {
                payrollProductsElement.Add(new XElement("kod",
                new XAttribute("id", GetString(payrollProduct.ExternalNumberOrNumber)),
                new XAttribute("namn", GetString(payrollProduct.Name))));
            }
            return payrollProductsElement;
        }

        private XElement CreateResultUnits(List<Tuple<String, String, String>> distinctResultUnits)
        {
            XElement resultUnits = new XElement("resultatenheter");
            foreach (var item in distinctResultUnits)
            {
                resultUnits.Add(CreateResultUnit(item.Item3, item.Item1, item.Item2));
            }
            return resultUnits;
        }

        private XElement CreateResultUnit(String accountdim, String accountNr, String accountName)
        {
            XElement resultUnit = new XElement("resultatenhet",
                new XAttribute("dim", GetString(accountdim)),
                new XAttribute("id", GetString(accountNr)),
                new XAttribute("namn", GetString(accountName)));
            return resultUnit;
        }

        private XElement CreateNormalWorkTime(DateTime date, double time)
        {
            XElement normalWorkTime =
                new XElement(("dag"),
                new XAttribute("datum", GetString(date)),
                new XAttribute("timmar", GetString(SalaryExportUtil.GetTimeFromMinutes(time.ToString()))));
            return normalWorkTime;
        }

        private XElement CreateNormalWorkTimeWithTimes(DateTime date, double time, DateTime startTime, DateTime stopTime)
        {
            XElement normalWorkTime =
                new XElement(("dag"),
                new XAttribute("datum", GetString(date)),
                new XAttribute("starttid", CalendarUtility.MergeDateAndTime(date, startTime).ToShortDateShortTimeStringT()),
                new XAttribute("sluttid", CalendarUtility.MergeDateAndTime(date, stopTime).ToShortDateShortTimeStringT()),
                new XAttribute("timmar", GetString(SalaryExportUtil.GetTimeFromMinutes(time.ToString()))));
            return normalWorkTime;
        }

        private XElement CreateTime(TransactionItem transaction, string empNr)
        {
            XElement time;
            if (externalExportId == "datum")
            {
                time = new XElement("tidtrans",
                    new XAttribute("anstid", GetString(empNr)),
                    new XElement("tidkod", GetString(transaction.ProductNr)),
                    new XElement("datumfrom", (transaction.Date).ToShortDateString()),
                    new XElement("datumtom", (transaction.Date).ToShortDateString()),
                    new XElement("timmar", GetString(SalaryExportUtil.GetTimeFromMinutes(transaction.Quantity.ToString()))));
            }
            else
            {
                time = new XElement("tidtrans",
                   new XAttribute("anstid", GetString(empNr)),
                   new XElement("tidkod", GetString(transaction.ProductNr)),
                   new XElement("datum", (transaction.Date).ToShortDateString()),
                   new XElement("timmar", GetString(SalaryExportUtil.GetTimeFromMinutes(transaction.Quantity.ToString()))));
            }

            // Check if there is any internal accounts
            var accountInternals = transaction.AccountInternals.ToList<AccountInternal>();
            if (accountInternals.Count > 0)
            {
                XElement internalaccounts = new XElement("resenheter");
                time.Add(internalaccounts);

                foreach (var internalAccount in accountInternals)
                {
                    if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue)
                    {
                        XElement Internalaccountrow = new XElement("resenhet",
                            new XAttribute("dim", (internalAccount.Account.AccountDim.SysSieDimNr)),
                            new XAttribute("id", (internalAccount.Account.AccountNr)));

                        internalaccounts.Add(Internalaccountrow);
                    }
                }
            }
            return time;
        }

        private XElement CreateRegOutlay(TransactionItem outlay, string empNr)
        {
             XElement regOutlay = new XElement("lonetrans",
              new XAttribute("anstid", GetString(empNr)),
              new XElement("datum", GetString(outlay.Date)),
              new XElement("lonart", GetString(outlay.ProductNr)),
              new XElement("benamning", GetString(outlay.ProductName)),
              new XElement("antal", (outlay.Quantity)),
              new XElement("belopp", (outlay.Amount)),
              new XElement("moms", (outlay.VatAmount)));

            // Check if there is any internal accounts
            var accountInternals = outlay.AccountInternals.ToList<AccountInternal>();
            if (accountInternals.Count > 0)
            {
                XElement internalaccounts = new XElement("resenheter");
                regOutlay.Add(internalaccounts);

                foreach (var internalAccount in accountInternals)
                {
                    if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue)
                    {
                        XElement Internalaccountrow = new XElement("resenhet",
                            new XAttribute("dim", (internalAccount.Account.AccountDim.SysSieDimNr)),
                            new XAttribute("id", (internalAccount.Account.AccountNr)));

                        internalaccounts.Add(Internalaccountrow);

                    }
                }
            }
            return regOutlay;
        }

        #endregion

        #region Help methods

        private String GetString(String value)
        {
            return value;
        }

        private String GetString(DateTime date)
        {
            return date.Date.ToShortDateString();
        }

        public static List<Tuple<String, String>> GetDistinctDims(List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem> transactions)
        {
            List<Tuple<String, String >> accounts = new List<Tuple<String, String>>();

            foreach (var transaction in transactions)
            {
                if (transaction.AccountInternals == null)
                    continue;

                foreach (AccountInternal internalAccount in transaction.AccountInternals)
                {
                    if (internalAccount.Account?.AccountDim?.SysSieDimNr != null && !accounts.Any(x => x.Item1 == internalAccount.Account.AccountDim.AccountDimNr.ToString()))
                        accounts.Add(Tuple.Create<String, String>(internalAccount.Account.AccountDim.AccountDimNr.ToString(), internalAccount.Account.AccountDim.Name));
                }
            }
            return accounts;
        }
        #endregion
    }
}
