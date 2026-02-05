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
    public class PAxmaladapter : ISalaryAdapter
    {
        private List<TransactionItem> allTransactionItems;
        private List<TransactionItem> timeTransactionItems;
        private List<TransactionItem> outlayTransactions;
        private List<ScheduleItem> scheduleItems;
        private Company company;
        private List<PayrollProduct> payrollProducts;
        private DateTime startDate;
        private DateTime stopDate;
        private List<int> employeeIds;
        private string externalExportId;

        #region Constructors

        public PAxmaladapter(Company company, List<PayrollProduct> payrollProducts, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<int> employeeIds, DateTime startDate, DateTime stopDate, string externalExportId)
        {
            this.allTransactionItems = payrollTransactions;
            //this.timeTransactionItems = payrollTransactions.Where(p => p.PayrollType != TermGroup_PayrollType.Addition && p.PayrollType != TermGroup_PayrollType.Deduction).ToList();
            this.timeTransactionItems = payrollTransactions.Where(p => !p.IsAddition() && !p.IsDeduction()).ToList();
            //this.outlayTransactions = payrollTransactions.Where(p => p.PayrollType == TermGroup_PayrollType.Addition || p.PayrollType == TermGroup_PayrollType.Deduction).ToList();
            this.outlayTransactions = payrollTransactions.Where(p => p.IsAddition() || p.IsDeduction()).ToList();
            this.scheduleItems = scheduleItems;
            this.company = company;
            this.payrollProducts = payrollProducts;
            this.startDate = startDate;
            this.stopDate = stopDate;
            this.employeeIds = employeeIds;
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
                 new XDeclaration("1.0", "UTF-8", "true"));

            return doc;
        }

        private XElement CreateSalaryData()
        {
            XNamespace name = "http://www.paxml.se/1.0/paxml.xsd";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XElement dimensions = new XElement("dimensioner");
            XElement schedules = new XElement("schematransaktioner");
            XElement times = new XElement("tidtransaktioner");
            XElement outlay = new XElement("lonetransaktioner");

            XElement salaryData = new XElement("paxml",
                new XAttribute(XNamespace.Xmlns + "name", name),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi)
                  //new XAttribute("format", GetString("LÖNIN")),
                  // new XAttribute("version", GetString("1.0")),
                  // new XAttribute("datum", GetString(DateTime.Now.ToShortDateString())),
                  // new XAttribute("foretagorgnr", GetString(company.OrgNr)),
                  // new XAttribute("foretagnamn", GetString(company.Name)),
                  // new XAttribute("programnamn", GetString("SoftOne"))
                  );

            //salaryData.Add(dimensions);

            List<Tuple<String, String, String>> distinctResultUnits = SalaryExportUtil.GetDistinctAccounts(allTransactionItems);
            if (distinctResultUnits.Any())
                salaryData.Add(CreateResultUnits(distinctResultUnits));

            foreach (var employeeIdSchedules in employeeIds)
            {
                List<ScheduleItem> scheduleItemsForEmployee = scheduleItems.Where(s => s.EmployeeId == employeeIdSchedules.ToString()).ToList();

                if (scheduleItemsForEmployee.Count > 0)
                {
                    String empNr = string.Empty;
                    ScheduleItem firstScheduleItem = scheduleItemsForEmployee.FirstOrDefault();
                    empNr = firstScheduleItem.EmployeeNr;

                    foreach (var item in scheduleItemsForEmployee)
                    {
                        if ((item.TotalMinutes - item.TotalBreakMinutes) != 0)
                        {
                            if (this.externalExportId.Contains("!tidschema!"))
                                schedules.Add(CreateNormalWorkTimeWithTimes(item.Date, item.TotalMinutes - item.TotalBreakMinutes, empNr, item.StartDate, item.StopDate));
                            else
                                schedules.Add(CreateNormalWorkTime(item.Date, item.TotalMinutes - item.TotalBreakMinutes, empNr));
                        }
                    }
                }
            }

            if (schedules.HasElements)
                salaryData.Add(schedules);


            foreach (var employeeIdTimes in employeeIds)
            {
                List<TransactionItem> transactionItemsForEmployee = timeTransactionItems.Where(s => s.EmployeeId == employeeIdTimes.ToString()).ToList();

                if (transactionItemsForEmployee.Count > 0)
                {
                    String empNr2 = string.Empty;
                    TransactionItem firstTransactionItem = transactionItemsForEmployee.FirstOrDefault();
                    empNr2 = firstTransactionItem.EmployeeNr;

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
                        empNr3 = firstTransactionItem.EmployeeNr;

                        foreach (var item in transactionItemsForEmployee)
                        {
                            outlay.Add(CreateRegOutlay(item, empNr3));
                        }
                    }
                }
            }

            return salaryData;
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
        
        private XElement CreateNormalWorkTime(DateTime date, double time, string empNr)
        {
            XElement normalWorkTime = new XElement("schema",
                new XAttribute("anstid", GetString(empNr)),
                new XElement(("dag"),
                new XAttribute("datum", GetString(date)),
                new XAttribute("timmar", GetString(SalaryExportUtil.GetTimeFromMinutes(time.ToString())))));
            return normalWorkTime;
        }

        private XElement CreateNormalWorkTimeWithTimes(DateTime date, double time, string empNr, DateTime startTime, DateTime stopTime)
        {
            XElement normalWorkTime = new XElement("schema",
                new XAttribute("anstid", GetString(empNr)),
                new XElement(("dag"),
                new XAttribute("datum", GetString(date)),
                new XAttribute("starttid", CalendarUtility.MergeDateAndTime(date, startTime).ToShortDateShortTimeStringT()),
                new XAttribute("sluttid", CalendarUtility.MergeDateAndTime(date, stopTime).ToShortDateShortTimeStringT()),
                new XAttribute("timmar", GetString(SalaryExportUtil.GetTimeFromMinutes(time.ToString())))));
            return normalWorkTime;
        }

        private XElement CreateTime(TransactionItem transaction, string empNr)
        {
            XElement time = null;
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
            String outlaytype;
            String comment = String.Empty;
            if (!String.IsNullOrEmpty(outlay.Comment))
            {
                comment = outlay.Comment.Length > 50 ? outlay.Comment.Substring(0, 50) : outlay.Comment;
                outlaytype = comment;
            }
            else
                outlaytype = outlay.ProductNr + "_" + outlay.ProductName;

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

        private String GetString(int value)
        {
            return value.ToString();
        }

        private String GetString(decimal value)
        {
            return value.ToString();
        }

        private String GetString(DateTime date)
        {
            return date.Date.ToShortDateString();
        }

        private String GetStringDateTime(DateTime date)
        {
            return date.Date.ToString();
        }

        #endregion
    }
}
