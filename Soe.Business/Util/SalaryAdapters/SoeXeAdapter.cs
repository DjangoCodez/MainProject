using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    public class SoeXeAdapter : ISalaryAdapter
    {
        private List<TimeCodeBreak> timeCodeBreaks = new List<TimeCodeBreak>();
        private TermGroup_SalaryExportUseSocSecFormat salaryExportUseSocSecFormat = TermGroup_SalaryExportUseSocSecFormat.KeepEmployeeNr;
        private List<TransactionItem> invoiceTransactionItemsOutput = new List<TransactionItem>();
        public List<TransactionItem> InvoiceTransactionItemsOutput
        {
            get
            {
                return invoiceTransactionItemsOutput;
            }
        }

        private List<TransactionItem> payrollTransactionItemsOutput = new List<TransactionItem>();
        public List<TransactionItem> PayrollTransactionItemsOutput
        {
            get
            {
                return payrollTransactionItemsOutput;
            }
        }

        private List<ScheduleItem> scheduleItemsOutput = new List<ScheduleItem>();
        public List<ScheduleItem> ScheduleItemsOutput
        {
            get
            {
                return scheduleItemsOutput;
            }
        }

        private List<TimeCode> timecodes = new List<TimeCode>();
        private List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
        private List<AccountStd> accountStds = new List<AccountStd>();
        private List<AccountDim> accountDims = new List<AccountDim>();
        private bool _showSocialSec = false;

        public byte[] TransformSalary(XDocument baseXml)
        {
            MemoryStream stream = new MemoryStream();
            baseXml.WriteTo(XmlWriter.Create(stream));
            return stream.ToArray();
        }

        private SoeTimeSalaryExportTarget exportTarget;

        #region Create Base XML

        public XDocument CreateXml(CompEntities entities, SoeTimeSalaryExportTarget exportTarget, List<AccountDim> accountDims, List<Employee> employees, List<EmployeeSchedule> schedules, List<TimeScheduleTemplateBlock> schedulesTemplateBlocks, List<TimePayrollTransaction> timePayrollTransactions, List<TimeInvoiceTransaction> timeInvoiceTransactions, List<TimePayrollScheduleTransaction> timeScheduleTransactions, List<TimeCode> timecodes, List<PayrollProduct> payrollProducts, List<AccountStd> accountStds, int actorCompanyId, DateTime start, DateTime stop, bool showSocialSec, TermGroup_SalaryExportUseSocSecFormat salaryExportUseSocSecFormat)
        {
            this.timecodes = timecodes;
            this.payrollProducts = payrollProducts;
            this.accountStds = accountStds;
            this.exportTarget = exportTarget;
            this.accountDims = accountDims;
            this._showSocialSec = showSocialSec;
            Dictionary<int, List<EmployeeSchedule>> employeeSchedulesDict = schedules.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, List<TimeScheduleTemplateBlock>> timeScheduleTemplateBlocksDict = schedulesTemplateBlocks.Where(w => w.EmployeeId.HasValue).GroupBy(g => g.EmployeeId.Value).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, List<TimePayrollTransaction>> timePayrollTransactionsDict = timePayrollTransactions.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, List<TimeInvoiceTransaction>> timeInvoiceTransactionsDict = timeInvoiceTransactions.Where(w => w.EmployeeId.HasValue).GroupBy(g => g.EmployeeId.Value).ToDictionary(x => x.Key, x => x.ToList());
            Dictionary<int, List<TimePayrollScheduleTransaction>> timeScheduleTransactionsDict = timeScheduleTransactions.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());


            this.salaryExportUseSocSecFormat = salaryExportUseSocSecFormat;

            LoadTimeCodeBreaks(entities, actorCompanyId);

            //Create new document with header
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-16", "true"));

            //Create content
            XElement xml = new XElement("employees",

            //Add Employee


            from employee in employees
            orderby employee.EmployeeNr
            select new XElement("employee",
                new XAttribute("id", employee.EmployeeId.ToString()),
                new XAttribute("nr", GetEmployeeIdentifier(employee)),
                new XAttribute("name", employee.ContactPerson.Name != null ? employee.ContactPerson.Name : string.Empty),
                //new XAttribute("employeegroupname", employee.EmployeeGroup.Name),

                //Add schedules
                GetSchedules(entities, employeeSchedulesDict.ContainsKey(employee.EmployeeId) ? employeeSchedulesDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value : new List<EmployeeSchedule>(), timeScheduleTemplateBlocksDict.ContainsKey(employee.EmployeeId) ? timeScheduleTemplateBlocksDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value : new List<TimeScheduleTemplateBlock>(), employee, employee.ContactPerson.Name != null ? employee.ContactPerson.Name : string.Empty, start, stop, actorCompanyId, exportTarget != SoeTimeSalaryExportTarget.AditroL1),

                //Add Punches
                GetPunches(),

                //Add Salary entitled transactions
                GetTransactionsElement(timeInvoiceTransactionsDict.ContainsKey(employee.EmployeeId) ? timeInvoiceTransactionsDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value : new List<TimeInvoiceTransaction>(),
                                        timePayrollTransactionsDict.ContainsKey(employee.EmployeeId) ? timePayrollTransactionsDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value : new List<TimePayrollTransaction>(),
                                        timeScheduleTransactionsDict.ContainsKey(employee.EmployeeId) ? timeScheduleTransactionsDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value : new List<TimePayrollScheduleTransaction>(),
                                        employee,
                                        false,
                                        actorCompanyId),

                //Add Absence transactions
                GetTransactionsElement(timeInvoiceTransactionsDict.ContainsKey(employee.EmployeeId) ? timeInvoiceTransactionsDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value : new List<TimeInvoiceTransaction>(),
                                        timePayrollTransactionsDict.ContainsKey(employee.EmployeeId) ? timePayrollTransactionsDict.FirstOrDefault(f => f.Key == employee.EmployeeId).Value : new List<TimePayrollTransaction>(),
                                        new List<TimePayrollScheduleTransaction>(),
                                        employee,
                                        true,
                                        actorCompanyId)
                ));

            doc.Add(xml);
            return doc;
        }

        private string GetEmployeeIdentifier(Employee employee)
        {
            return StringUtility.GetSalaryExportUseSocSecFormat(employee.EmployeeNr, employee.SocialSec, salaryExportUseSocSecFormat);
        }

        #region Punches

        private object GetPunches()
        {
            //TODO
            return new XElement("punches");
        }

        #endregion

        #region Transactions

        private XElement GetTransactionsElement(List<TimeInvoiceTransaction> invoiceTransactions, List<TimePayrollTransaction> payrollTransactions, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions, Employee employee, bool absences, int actorCompanyId)
        {
            List<TimePayrollTransaction> payrollTransactionsForEmployee = (from p in payrollTransactions where p.EmployeeId == employee.EmployeeId select p).ToList();
            //List<TimeInvoiceTransaction> invoiceTransactionsForEmployee = (from i in invoiceTransactions where i.Employee.EmployeeId == employee.EmployeeId select i).ToList();
            //dont include invoicetransactions accourding to rickard 
            List<TimeInvoiceTransaction> invoiceTransactionsForEmployee = new List<TimeInvoiceTransaction>();

            return new XElement((absences ? "absences" : "transactions"),
                GetTimeInvoiceTransactionsElement(invoiceTransactionsForEmployee, employee, absences, actorCompanyId),
                GetTimePayrollTransactionsElement(payrollTransactionsForEmployee, timePayrollScheduleTransactions, employee, absences, actorCompanyId));
        }

        private XElement GetTimeInvoiceTransactionsElement(List<TimeInvoiceTransaction> invoiceTransactions, Employee employee, bool absences, int actorCompanyId)
        {
            XElement transactions = new XElement("invoicetransactions");

            transactions.Add(GetTimeInvoiceTransactionsElements(invoiceTransactions, employee, absences, actorCompanyId));

            return transactions;
        }

        private XElement GetTimePayrollTransactionsElement(List<TimePayrollTransaction> payTrans, List<TimePayrollScheduleTransaction> payScheduleTrans, Employee employee, bool absences, int actorCompanyId)
        {
            XElement transactions = new XElement("payrolltransactions");
            transactions.Add(GetTimePayrollTransactionsElements(payTrans, payScheduleTrans, employee, absences, actorCompanyId));
            return transactions;
        }

        private List<XElement> GetTimeInvoiceTransactionsElements(List<TimeInvoiceTransaction> invoiceTransactions, Employee employee, bool absences, int actorCompanyId)
        {
            List<XElement> result = new List<XElement>();
            List<TransactionItem> items = new List<TransactionItem>();

            foreach (TimeInvoiceTransaction invoiceTransaction in invoiceTransactions)
            {
                AccountStd accountStd = GetAccountStd(invoiceTransaction.AccountStdId);
                if (accountStd == null)
                    continue;

                if (!invoiceTransaction.TimeCodeTransactionReference.IsLoaded)
                    invoiceTransaction.TimeCodeTransactionReference.Load();

                //Check transaction type
                TimeCodeTransaction timeCodeTransaction = invoiceTransaction.TimeCodeTransaction;
                if (timeCodeTransaction == null)
                    continue;

                if (!timeCodeTransaction.TimeRuleReference.IsLoaded)
                    timeCodeTransaction.TimeRuleReference.Load();

                if (timeCodeTransaction.TimeRule != null && timeCodeTransaction.TimeRule.Type != (int)SoeTimeRuleType.Absence && absences || timeCodeTransaction.TimeRule.Type == (int)SoeTimeRuleType.Absence && !absences)
                    continue;

                if (!invoiceTransaction.TimeBlockDateReference.IsLoaded)
                    invoiceTransaction.TimeBlockDateReference.Load();

                if (!invoiceTransaction.InvoiceProductReference.IsLoaded)
                    invoiceTransaction.InvoiceProductReference.Load();

                if (!invoiceTransaction.AccountStdReference.IsLoaded)
                    invoiceTransaction.AccountStdReference.Load();

                if (!invoiceTransaction.AccountInternal.IsLoaded)
                    invoiceTransaction.AccountInternal.Load();

                TimeCode timeCode = GetAddtionDeductionTimeCode(invoiceTransaction);

                Employment employment = null;
                if (invoiceTransaction.TimeBlockDate != null)
                    employment = employee.GetNearestEmployment(invoiceTransaction.TimeBlockDate.Date);


                //Add transaction item
                TransactionItem item = new TransactionItem()
                {
                    EmployeeId = invoiceTransaction.EmployeeId.ToString(),
                    EmployeeNr = GetEmployeeIdentifier(employee),
                    ExternalCode = employment != null ? employment.GetExternalCode(invoiceTransaction.TimeBlockDate.Date) : string.Empty,
                    EmployeeName = employee.Name,
                    EmployeeFirstName = employee.FirstName,
                    EmployeeLastName = employee.LastName,
                    EmployeeSocialSec = _showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                    Quantity = invoiceTransaction.Quantity,
                    Time = GetTransactionTime(invoiceTransaction),
                    Date = invoiceTransaction.TimeBlockDate.Date,
                    IsAbsence = absences,
                    ProductNr = invoiceTransaction.InvoiceProduct.Number,
                    ProductName = invoiceTransaction.InvoiceProduct.Name,
                    Account = accountStd,
                    AccountInternals = invoiceTransaction.AccountInternal.ToList(),
                    Comment = String.Empty,
                    Amount = invoiceTransaction.Amount.HasValue ? invoiceTransaction.Amount.Value : 0,
                    VatAmount = invoiceTransaction.VatAmount.HasValue ? invoiceTransaction.VatAmount.Value : 0,
                    IsRegistrationQuantity = timeCode != null ? timeCode.IsRegistrationTypeQuantity : false,
                    IsRegistrationTime = timeCode != null ? timeCode.IsRegistrationTypeTime : true,
                };

                if (item.IsAbsence)
                {
                    item.AbsenceStartTime = GetTransactionStartTime(invoiceTransaction.TimeCodeTransaction);
                    item.AbsenceStopTime = GetTransactionStopTime(invoiceTransaction.TimeCodeTransaction);
                }

                items.Add(item);
            }

            //Merge
            items = MergeTransactions(items);
            invoiceTransactionItemsOutput.AddRange(items);

            if (!LimitBaseDocument())
            {
                //Add elements
                foreach (TransactionItem item in items)
                {
                    result.Add(
                        GetTransactionElement(
                            item.Quantity,
                            item.Amount,
                            item.Time,
                            item.Date,
                            item.IsAbsence,
                            item.ProductNr,
                            item.Comment,
                            item.Account,
                            item.AccountInternals,
                            item.IsRegistrationQuantity,
                            item.IsRegistrationTime,
                            item.VatAmount));
                }
            }

            return result;
        }

        private TimeCode GetAddtionDeductionTimeCode(TimeInvoiceTransaction transaction)
        {
            if (transaction != null && transaction.TimeCodeTransaction != null)
                return timecodes.FirstOrDefault(tc => tc.TimeCodeId == transaction.TimeCodeTransaction.TimeCodeId);
            else
                return null;
        }

        private TimeCode GetAddtionDeductionTimeCode(TimePayrollTransaction transaction)
        {
            if (transaction != null && transaction.TimeCodeTransaction != null)
                return timecodes.FirstOrDefault(tc => tc.TimeCodeId == transaction.TimeCodeTransaction.TimeCodeId);
            else
                return null;
        }

        private PayrollProduct GetPayrollProduct(int productId)
        {
            return payrollProducts.FirstOrDefault(x => x.ProductId == productId);
        }

        private AccountStd GetAccountStd(int accountId)
        {
            return accountStds.FirstOrDefault(x => x.AccountId == accountId);
        }

        private List<XElement> GetTimePayrollTransactionsElements(List<TimePayrollTransaction> payrollTransactions, List<TimePayrollScheduleTransaction> payScheduleTrans, Employee employee, bool absences, int actorCompanyId)
        {
            List<XElement> result = new List<XElement>();
            List<TransactionItem> items = new List<TransactionItem>();
            List<int> notValidAccountDimIds = this.accountDims != null ? this.accountDims.Where(w => w.ExcludeinSalaryExport).Select(s => s.AccountDimId).ToList() : new List<int>();

            #region TimePayrollTransactions

            foreach (TimePayrollTransaction payrollTransaction in payrollTransactions)
            {
                PayrollProduct payrollProduct = GetPayrollProduct(payrollTransaction.ProductId);
                if (payrollProduct == null)
                    continue;

                if (absences && !payrollProduct.IsAbsencePayrollExport() ||
                    !absences && payrollProduct.IsAbsencePayrollExport())
                    continue;

                AccountStd accountStd = GetAccountStd(payrollTransaction.AccountStdId);
                if (accountStd == null)
                    continue;

                foreach (var ai in payrollTransaction.AccountInternal)
                {
                    if (!ai.AccountReference.IsLoaded)
                        ai.AccountReference.Load();

                    if (ai.Account != null && !ai.Account.AccountDimReference.IsLoaded)
                        ai.Account.AccountDimReference.Load();
                }

                TimeCode timeCode = GetAddtionDeductionTimeCode(payrollTransaction);


                Employment employment = null;
                if (payrollTransaction.TimeBlockDate != null)
                    employment = employee.GetNearestEmployment(payrollTransaction.TimeBlockDate.Date);

                //Add transaction item
                TransactionItem item = new TransactionItem()
                {
                    EmployeeId = payrollTransaction.EmployeeId.ToString(),
                    EmployeeNr = GetEmployeeIdentifier(employee),
                    ExternalCode = employment != null ? employment.GetExternalCode(payrollTransaction.TimeBlockDate.Date) : string.Empty,
                    EmployeeName = employee.Name,
                    EmployeeFirstName = employee.FirstName,
                    EmployeeLastName = employee.LastName,
                    EmployeeSocialSec = _showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                    EmployeeChildId = payrollTransaction.EmployeeChildId,
                    SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                    SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                    SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                    SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                    TimeDeviationCauseId = payrollTransaction.TimeBlock?.TimeDeviationCauseStopId,
                    Quantity = payrollTransaction.Quantity,
                    Time = GetTransactionTime(payrollTransaction),
                    Date = payrollTransaction.TimeBlockDate.Date,
                    IsAbsence = absences,
                    //ProductNr = payrollProduct.Number,
                    ProductNr = payrollProduct.ExternalNumberOrNumber,
                    ProductName = payrollProduct.Name,
                    Account = accountStd,
                    AccountInternals = payrollTransaction.AccountInternal.Where(w => !notValidAccountDimIds.Contains(w.Account.AccountDimId)).OrderBy(ai => ai.Account.AccountDim.AccountDimNr).ToList(),
                    Comment = GetComment(payrollTransaction),
                    Amount = (payrollTransaction.Amount.HasValue && payrollProduct.IncludeAmountInExport) ? payrollTransaction.Amount.Value : 0,
                    VatAmount = (payrollTransaction.VatAmount.HasValue && payrollProduct.IncludeAmountInExport) ? payrollTransaction.VatAmount.Value : 0,
                    IsRegistrationQuantity = timeCode != null ? timeCode.IsRegistrationTypeQuantity : false,
                    IsRegistrationTime = timeCode != null ? timeCode.IsRegistrationTypeTime : true,
                    ProductCode = payrollProduct.ExternalNumberOrNumber,
                };

                if (item.IsAbsence)
                {
                    item.AbsenceStartTime = GetTransactionStartTime(payrollTransaction.TimeCodeTransaction);
                    item.AbsenceStopTime = GetTransactionStopTime(payrollTransaction.TimeCodeTransaction);
                }

                items.Add(item);
            }

            #endregion

            if (!absences)
            {
                #region TimePayrollScheduleTransaction

                foreach (TimePayrollScheduleTransaction scheduleTransaction in payScheduleTrans)
                {
                    PayrollProduct payrollProduct = GetPayrollProduct(scheduleTransaction.ProductId);
                    if (payrollProduct == null)
                        continue;

                    AccountStd accountStd = GetAccountStd(scheduleTransaction.AccountStdId);
                    if (accountStd == null)
                        continue;

                    foreach (var ai in scheduleTransaction.AccountInternal)
                    {
                        if (!ai.AccountReference.IsLoaded)
                            ai.AccountReference.Load();

                        if (ai.Account != null && !ai.Account.AccountDimReference.IsLoaded)
                            ai.Account.AccountDimReference.Load();
                    }

                    Employment employment = null;
                    if (scheduleTransaction.TimeBlockDate != null)
                        employment = employee.GetNearestEmployment(scheduleTransaction.TimeBlockDate.Date);

                    //Add transaction item
                    TransactionItem item = new TransactionItem()
                    {
                        EmployeeId = scheduleTransaction.EmployeeId.ToString(),
                        EmployeeNr = GetEmployeeIdentifier(employee),
                        ExternalCode = employment != null ? employment.GetExternalCode(scheduleTransaction.TimeBlockDate.Date) : string.Empty,
                        EmployeeName = employee.Name,
                        EmployeeFirstName = employee.FirstName,
                        EmployeeLastName = employee.LastName,
                        EmployeeSocialSec = _showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                        SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                        SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                        SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                        SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                        TimeDeviationCauseId = null,
                        Quantity = scheduleTransaction.Quantity,
                        Time = GetTransactionTime(scheduleTransaction),
                        Date = scheduleTransaction.TimeBlockDate.Date,
                        IsAbsence = false,
                        //ProductNr = payrollProduct.Number,
                        ProductNr = payrollProduct.ExternalNumberOrNumber,
                        ProductName = payrollProduct.Name,
                        Account = accountStd,
                        AccountInternals = scheduleTransaction.AccountInternal.Where(w => !notValidAccountDimIds.Contains(w.Account.AccountDimId)).OrderBy(ai => ai.Account.AccountDim.AccountDimNr).ToList(),
                        Comment = "",
                        Amount = (scheduleTransaction.Amount.HasValue && payrollProduct.IncludeAmountInExport) ? scheduleTransaction.Amount.Value : 0,
                        VatAmount = (scheduleTransaction.VatAmount.HasValue && payrollProduct.IncludeAmountInExport) ? scheduleTransaction.VatAmount.Value : 0,
                        IsRegistrationQuantity = false,
                        IsRegistrationTime = true,
                        ProductCode = payrollProduct.ExternalNumberOrNumber,
                    };

                    items.Add(item);
                }

                #endregion
            }
            //Merge
            items = MergeTransactions(items);
            payrollTransactionItemsOutput.AddRange(items);

            if (!LimitBaseDocument())
            {
                //Add elements
                foreach (TransactionItem ti in items)
                {
                    result.Add(
                        GetTransactionElement(
                            ti.Quantity,
                            ti.Amount,
                            ti.Time,
                            ti.Date,
                            ti.IsAbsence,
                            ti.ProductNr,
                            ti.Comment,
                            ti.Account,
                            ti.AccountInternals,
                            ti.IsRegistrationQuantity,
                            ti.IsRegistrationTime,
                            ti.VatAmount));
                }
            }
            return result;
        }

        private XElement GetTransactionElement(decimal quantity, decimal amount, double minutes, DateTime date, bool isAbsence, string productNumber, string comment, AccountStd account, List<AccountInternal> internalAccounts, bool isRegistrationTypeQuantity, bool isRegistrationTypeTime, decimal vatAmount)
        {
            XElement trans = new XElement("transaction",
                    new XAttribute("quantity", quantity),
                    new XAttribute("amount", amount != 0 ? amount.ToString() : String.Empty),
                    new XAttribute("totalminutes", minutes),
                    new XAttribute("date", date),
                    new XAttribute("isabsence", isAbsence.ToString()),
                    new XAttribute("productnumber", productNumber),
                    new XAttribute("comment", comment),
                    new XAttribute("isRegistrationTypeQuantity", isRegistrationTypeQuantity),
                    new XAttribute("isRegistrationTypeTime", isRegistrationTypeTime),
                    new XAttribute("vatAmount", vatAmount));


            if (account != null && account.Account != null)
            {
                trans.Add(new XElement("account",
                    new XAttribute("id", account.AccountId),
                    new XAttribute("name", account.Account.Name),
                    new XAttribute("nr", account.Account.AccountNr)));
            }

            if (internalAccounts != null)
            {
                XElement internalAcc = new XElement("internalaccounts");
                foreach (AccountInternal ia in internalAccounts)
                {
                    if (ia.Account == null) continue;

                    internalAcc.Add(new XElement("account",
                        new XAttribute("id", ia.AccountId),
                        new XAttribute("name", ia.Account.Name),
                        new XAttribute("nr", ia.Account.AccountNr),
                        new XAttribute("siedimnr", (ia.Account.AccountDim != null && ia.Account.AccountDim.SysSieDimNr.HasValue) ? ia.Account.AccountDim.SysSieDimNr.Value : 0)));
                }
                trans.Add(internalAcc);
            }

            return trans;
        }

        private List<TransactionItem> MergeTransactions(List<TransactionItem> items)
        {
            List<TransactionItem> result = new List<TransactionItem>();

            while (items.Count > 0)
            {
                TransactionItem firstItem = items.FirstOrDefault();
                List<TransactionItem> matchingTransactions = new List<TransactionItem>();
                items.Remove(firstItem);
                matchingTransactions.Add(firstItem);

                //find similar trasactions to merge
                List<TransactionItem> tmp = (from i in items
                                             where i.Date == firstItem.Date &&
                                             i.IsAbsence == firstItem.IsAbsence &&
                                             i.ProductNr == firstItem.ProductNr &&
                                             i.IsRegistrationQuantity == firstItem.IsRegistrationQuantity &&
                                             i.IsRegistrationTime == firstItem.IsRegistrationTime &&
                                             i.Amount == 0 //dont merge transactions with amounts
                                             select i).ToList();

                //Accountinternals must be the same and in the same order
                foreach (var itemInTmp in tmp)
                {
                    bool sameAccountCount = itemInTmp.AccountInternals.Count == firstItem.AccountInternals.Count;
                    if (sameAccountCount)
                    {
                        bool allAccountsMatch = true; //accountCount can be zero

                        for (int i = 0; i < firstItem.AccountInternals.Count; i++)
                        {
                            if (firstItem.AccountInternals[i].AccountId != itemInTmp.AccountInternals[i].AccountId)
                                allAccountsMatch = false;
                        }

                        if (allAccountsMatch)
                            matchingTransactions.Add(itemInTmp);
                    }
                }

                TransactionItem mergedItem = new TransactionItem()
                {
                    EmployeeId = firstItem.EmployeeId,
                    EmployeeName = firstItem.EmployeeName,
                    ExternalCode = firstItem.ExternalCode,
                    EmployeeFirstName = firstItem.EmployeeFirstName,
                    EmployeeLastName = firstItem.EmployeeLastName,
                    TimeDeviationCauseId = firstItem.TimeDeviationCauseId,
                    SysPayrollTypeLevel1 = firstItem.SysPayrollTypeLevel1,
                    SysPayrollTypeLevel2 = firstItem.SysPayrollTypeLevel2,
                    SysPayrollTypeLevel3 = firstItem.SysPayrollTypeLevel3,
                    SysPayrollTypeLevel4 = firstItem.SysPayrollTypeLevel4,
                    EmployeeSocialSec = firstItem.EmployeeSocialSec,
                    EmployeeNr = firstItem.EmployeeNr,
                    EmployeeChildId = firstItem.EmployeeChildId,
                    AccountInternals = firstItem.AccountInternals,
                    Account = firstItem.Account,
                    Date = firstItem.Date,
                    IsAbsence = firstItem.IsAbsence,
                    ProductNr = firstItem.ProductNr,
                    ProductName = firstItem.ProductName,
                    Quantity = 0,
                    Time = 0,
                    Amount = firstItem.Amount,
                    VatAmount = firstItem.VatAmount,
                    IsRegistrationQuantity = firstItem.IsRegistrationQuantity,
                    IsRegistrationTime = firstItem.IsRegistrationTime,
                    ProductCode = firstItem.ProductCode,
                };

                if (matchingTransactions.Count > 0)
                {
                    var firstMatch = matchingTransactions.OrderBy(m => m.AbsenceStartTime).FirstOrDefault();
                    var lastMatch = matchingTransactions.OrderBy(m => m.AbsenceStartTime).LastOrDefault();

                    //AbsenceStartTime
                    if (firstItem.AbsenceStartTime < firstMatch.AbsenceStartTime)
                        mergedItem.AbsenceStartTime = firstItem.AbsenceStartTime;
                    else
                        mergedItem.AbsenceStartTime = firstMatch.AbsenceStartTime;

                    //AbsenceStopTime
                    if (firstItem.AbsenceStopTime > lastMatch.AbsenceStopTime)
                        mergedItem.AbsenceStopTime = firstItem.AbsenceStopTime;
                    else
                        mergedItem.AbsenceStopTime = lastMatch.AbsenceStopTime;
                }


                foreach (var tmpItem in matchingTransactions)
                {
                    mergedItem.Quantity += tmpItem.Quantity;
                    mergedItem.Time += tmpItem.Time;
                    mergedItem.Comment += tmpItem.Comment + (!String.IsNullOrEmpty(tmpItem.Comment) && !tmpItem.Comment.EndsWith(".") ? "." : String.Empty);
                    //newItem.Amount += tmpItem.Amount;
                    //newItem.VatAmount += tmpItem.VatAmount;
                }
                matchingTransactions.ForEach(i => items.Remove(i));
                result.Add(mergedItem);
            }

            return result;
        }

        private List<ScheduleItem> MergeSchedule(List<ScheduleItem> scheduleItems, bool mergeOnExtraShift = true)
        {
            List<ScheduleItem> result = new List<ScheduleItem>();
            scheduleItems = scheduleItems.OrderBy(i => i.StartDate).ToList();

            while (scheduleItems.Count > 0)
            {
                ScheduleItem firstItem = scheduleItems.FirstOrDefault(i => i.IsBreak == false);
                if (firstItem == null)
                    firstItem = scheduleItems.FirstOrDefault();

                List<ScheduleItem> tmp;
                if (mergeOnExtraShift)
                {
                    tmp = (from i in scheduleItems
                           where i.Date == firstItem.Date &&
                           i.TimeScheduleTemplatePeriodId == firstItem.TimeScheduleTemplatePeriodId
                           select i).ToList();
                }
                else
                {
                    tmp = (from i in scheduleItems
                           where i.Date == firstItem.Date &&
                           i.TimeScheduleTemplatePeriodId == firstItem.TimeScheduleTemplatePeriodId &&
                           !i.IsBreak &&
                           i.ExtraShift == firstItem.ExtraShift
                           select i).ToList();

                    List<ScheduleItem> breaks = new List<ScheduleItem>();
                    foreach (var shift in tmp)
                    {
                        foreach (var shiftBreak in scheduleItems.Where(x => x.Date == firstItem.Date && x.TimeScheduleTemplatePeriodId == firstItem.TimeScheduleTemplatePeriodId && x.IsBreak).ToList())
                        {
                            if (CalendarUtility.GetOverlappingMinutes(shiftBreak.StartDate, shiftBreak.StopDate, shift.StartDate, shift.StopDate) > 0 && !breaks.Contains(shiftBreak))
                                breaks.Add(shiftBreak);
                        }
                    }
                    tmp.AddRange(breaks);
                }

                ScheduleItem newItem = new ScheduleItem()
                {
                    EmployeeId = firstItem.EmployeeId,
                    EmployeeName = firstItem.EmployeeName,
                    EmployeeFirstName = firstItem.EmployeeFirstName,
                    EmployeeLastName = firstItem.EmployeeLastName,
                    EmployeeSocialSec = firstItem.EmployeeSocialSec,
                    EmployeeNr = firstItem.EmployeeNr,
                    ExternalCode = firstItem.ExternalCode,
                    TimeScheduleTemplatePeriodId = firstItem.TimeScheduleTemplatePeriodId,
                    Date = firstItem.Date,
                    DayNumber = firstItem.DayNumber,
                    ProductNumber = firstItem.ProductNumber,
                    StartDate = firstItem.StartDate,
                    StopDate = firstItem.StopDate,
                    ExtraShift = firstItem.ExtraShift,
                    TotalBreakMinutes = 0,
                    TotalMinutes = 0,
                    AccountInternals = firstItem.AccountInternals,
                };

                foreach (var tmpItem in tmp)
                {
                    if (tmpItem.IsBreak)
                        newItem.TotalBreakMinutes += tmpItem.TotalBreakMinutes;
                    else
                    {
                        newItem.TotalMinutes += tmpItem.TotalMinutes;
                        if (newItem.StopDate < tmpItem.StopDate)
                        {
                            newItem.StopDate = tmpItem.StopDate;
                        }
                    }

                }
                tmp.ForEach(i => scheduleItems.Remove(i));
                result.Add(newItem);
                if (scheduleItems.Count == scheduleItems.Count(c => c.IsBreak)) //may happen if break is outside of scheduled time
                    scheduleItems.Clear();
            }
            return result.OrderBy(i => i.Date).ThenBy(i => i.StartDate).ToList();
        }

        private double GetTransactionTime(TimePayrollScheduleTransaction transaction)
        {
            //Changed to Quantity from (Stop - Start).TotalMinutes on the related TimeCodeTransaction
            return Convert.ToDouble(transaction.Quantity);
        }
        private double GetTransactionTime(TimePayrollTransaction transaction)
        {
            //Changed to Quantity from (Stop - Start).TotalMinutes on the related TimeCodeTransaction
            return Convert.ToDouble(transaction.Quantity);
        }

        private double GetTransactionTime(TimeInvoiceTransaction transaction)
        {
            //Changed to Quantity from (Stop - Start).TotalMinutes on the related TimeCodeTransaction
            return Convert.ToDouble(transaction.Quantity);
        }

        private TimeSpan GetTransactionStartTime(TimeCodeTransaction transaction)
        {
            if (transaction != null)
                return new TimeSpan(transaction.Start.Hour, transaction.Start.Minute, 0);
            else
                return new TimeSpan(0, 0, 0);
        }

        private TimeSpan GetTransactionStopTime(TimeCodeTransaction transaction)
        {
            if (transaction != null)
            {
                var timeSpan = new TimeSpan(transaction.Stop.Hour, transaction.Stop.Minute, 0);
                if ((transaction.Stop.Date - transaction.Start.Date).Days > 0)
                    timeSpan = timeSpan.Add(new TimeSpan((transaction.Stop.Date - transaction.Start.Date).Days * 24, 0, 0));

                return timeSpan;
            }
            else
                return new TimeSpan(0, 0, 0);
        }

        private double GetTransactionTime(List<TimeCodeTransaction> transactions)
        {
            double minutes = 0;
            transactions.ForEach(i => minutes += (i.Stop - i.Start).TotalMinutes);
            return minutes;
        }

        private String GetComment(TimePayrollTransaction transaction)
        {
            if (!String.IsNullOrEmpty(transaction.Comment))
                return transaction.Comment;

            if (transaction.TimeBlockReference.IsLoaded && transaction.TimeBlock != null && !String.IsNullOrEmpty(transaction.TimeBlock.Comment))
                return transaction.TimeBlock.Comment;

            return String.Empty;
        }

        private bool IsExportTagetHogia()
        {
            return (this.exportTarget == SoeTimeSalaryExportTarget.Hogia214006 || this.exportTarget == SoeTimeSalaryExportTarget.Hogia214002 || this.exportTarget == SoeTimeSalaryExportTarget.Hogia214007);
        }

        private bool IsExportTagetPersonec()
        {
            return (this.exportTarget == SoeTimeSalaryExportTarget.Personec);
        }

        private bool LimitBaseDocument()
        {
            return (this.exportTarget == SoeTimeSalaryExportTarget.SDWorx || this.exportTarget == SoeTimeSalaryExportTarget.AditroL1 || this.exportTarget == SoeTimeSalaryExportTarget.HuldtOgLillevik || this.exportTarget == SoeTimeSalaryExportTarget.Lessor);
        }

        #endregion

        #region Schedule

        private XElement GetSchedules(CompEntities entities, List<EmployeeSchedule> allSchedules, List<TimeScheduleTemplateBlock> allSchedulesTemplateBlocks, Employee employee, string employeeName, DateTime start, DateTime stop, int actorCompanyId, bool mergeOnExtraShift = true)
        {
            XElement schedules = new XElement("schedules", new XAttribute("employeeId", employee.EmployeeId));

            //Extract employeeschedules
            List<EmployeeSchedule> employeeSchedules = (from x in allSchedules where x.EmployeeId == employee.EmployeeId select x).ToList();

            foreach (EmployeeSchedule schema in employeeSchedules)
            {
                if (schema.StopDate < start) continue;
                if (schema.StartDate > stop) continue;

                List<TimeScheduleTemplateBlock> employeeScheduleTemplateBlocks = allSchedulesTemplateBlocks.Where(b => b.EmployeeId.HasValue && b.EmployeeId.Value == employee.EmployeeId).ToList();

                //Get schedule element
                XElement schedule = new XElement("schedule",
                                      new XAttribute("id", schema.EmployeeScheduleId.ToString()),
                                      new XAttribute("name", schema.TimeScheduleTemplateHead.Name),
                                      new XAttribute("generatedName", SalaryExportUtil.GetGeneratedName(employeeName, schema.StartDate)));

                List<ScheduleItem> scheduleItems = new List<ScheduleItem>();

                //Add day elements
                foreach (TimeScheduleTemplatePeriod period in schema.TimeScheduleTemplateHead.TimeScheduleTemplatePeriod)
                {
                    List<TimeScheduleTemplateBlock> periodblocks = employeeScheduleTemplateBlocks.Where(b => b.TimeScheduleTemplatePeriodId == period.TimeScheduleTemplatePeriodId).ToList();

                    foreach (var block in periodblocks)
                    {
                        string productNumber = string.Empty;
                        TimeCodePayrollProduct product = block.TimeCode.TimeCodePayrollProduct.FirstOrDefault();
                        if (product != null)
                        {
                            if (!product.PayrollProductReference.IsLoaded)
                            {
                                product.PayrollProductReference.Load();
                            }
                            PayrollProduct p = product.PayrollProduct;
                            if (p != null)
                                productNumber = p.Number;
                        }

                        if (block.Date <= schema.StopDate && block.Date >= schema.StartDate)
                        {
                            Employment employment = null;
                            if (block.Date.HasValue)
                            {
                                employment = employee.GetEmployment(block.Date.Value, discardTemporaryPrimary: false);

                                if (employment == null)
                                    employment = employee.GetNearestEmployment(block.Date.Value, discardTemporaryPrimary: false);

                                if (employment == null)
                                    employment = employee.GetNearestEmployment(block.Date.Value);
                            }
                            //if (employment.IsTemporaryPrimary)
                            //{
                            //    var noTemp = employee.GetEmployment(block.Date.Value, discardTemporaryPrimary: true);

                            //    if (noTemp != null)
                            //        employment = noTemp;

                            //    if (noTemp == null)
                            //        continue;
                            //}

                            scheduleItems.Add(new ScheduleItem()
                            {
                                EmployeeId = employee.EmployeeId.ToString(),
                                EmployeeNr = GetEmployeeIdentifier(employee),
                                ExternalCode = employment != null ? employment.GetExternalCode(block.Date) : string.Empty,
                                EmployeeName = employee.Name,
                                EmployeeFirstName = employee.FirstName,
                                EmployeeLastName = employee.LastName,
                                EmployeeSocialSec = _showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                                TimeScheduleTemplatePeriodId = block.TimeScheduleTemplatePeriodId,
                                DayNumber = period.DayNumber,
                                Date = block.Date.Value,
                                StartDate = block.IsNotScheduleTimeShift ? block.StartTime.Date : block.StartTime,
                                StopDate = block.IsNotScheduleTimeShift ? block.StopTime.Date : block.StopTime,
                                IsBreak = block.IsBreak,
                                TotalMinutes = ((block.IsBreak || block.IsNotScheduleTimeShift) ? 0 : (block.StopTime - block.StartTime).TotalMinutes),
                                TotalBreakMinutes = (block.IsBreak && !block.BreakIsOverlappedByNotScheduleTimeShift ? GetBreakMinutes(entities, block, actorCompanyId) : 0),
                                ProductNumber = productNumber,
                                ExtraShift = block.ExtraShift,
                                AccountInternals = block.AccountInternal?.ToList()
                            });
                        }
                    }
                }

                //Merge
                scheduleItems = MergeSchedule(scheduleItems, mergeOnExtraShift);
                scheduleItemsOutput.AddRange(scheduleItems);

                if (!LimitBaseDocument())
                {
                    //Add
                    foreach (var item in scheduleItems)
                    {
                        schedule.Add(new XElement("day",
                            new XAttribute("timescheduletemplateperiodid", item.TimeScheduleTemplatePeriodId),
                            new XAttribute("daynumber", item.DayNumber),
                            new XAttribute("date", item.Date),
                            new XAttribute("starttime", SalaryExportUtil.DateToTimeString(item.StartDate)),
                            new XAttribute("stoptime", SalaryExportUtil.DateToTimeString(item.StopDate)),
                            new XAttribute("totaltimemin", item.TotalMinutes),
                            new XAttribute("productnumber", item.ProductNumber),
                            new XAttribute("totalbreakmin", item.TotalBreakMinutes)));
                    }

                    //Add schedule to schedules element
                    schedules.Add(schedule);
                }
            }
            return schedules;
        }

        private int GetBreakMinutes(CompEntities entities, TimeScheduleTemplateBlock block, int actorCompanyId)
        {
            int result = 0;

            TimeCodeBreak timecodeBreak = timeCodeBreaks.FirstOrDefault(b => b.TimeCodeId == block.TimeCode.TimeCodeId);

            if (timecodeBreak != null)
                result = timecodeBreak.DefaultMinutes;

            return result;
        }

        private void LoadTimeCodeBreaks(CompEntities entities, int actorCompanyId)
        {
            TimeCodeManager tcm = new TimeCodeManager(null);
            timeCodeBreaks = tcm.GetTimeCodeBreaks(entities, actorCompanyId).ToList();
        }

        #endregion

        #endregion
    }

    public class ScheduleItem
    {
        public ScheduleItem()
        {
            EmployeeId = String.Empty;
            EmployeeNr = String.Empty;
            ExternalCode = String.Empty;
            EmployeeName = String.Empty;
            EmployeeFirstName = String.Empty;
            EmployeeLastName = String.Empty;
            EmployeeSocialSec = String.Empty;
            TotalMinutes = 0;
            TotalBreakMinutes = 0;
            ProductNumber = String.Empty;
            AbsenceMinutes = 0;
            AccountInternals = new List<AccountInternal>();
        }

        public String EmployeeId { get; set; }
        public String EmployeeNr { get; set; }
        public String ExternalCode { get; set; }
        public String EmployeeName { get; set; }
        public String EmployeeFirstName { get; set; }
        public String EmployeeLastName { get; set; }
        public String EmployeeSocialSec { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public bool IsBreak { get; set; }
        public double TotalMinutes { get; set; }
        public double TotalBreakMinutes { get; set; }
        public string ProductNumber { get; set; }
        public double AbsenceMinutes { get; set; }
        public bool ExtraShift { get; set; }
        public List<AccountInternal> AccountInternals { get; set; }

        public string GetExternalEmploymentCode()
        {
            if (string.IsNullOrEmpty(ExternalCode))
                return string.Empty;

            if (!ExternalCode.Contains('#'))
                return ExternalCode;

            var array = ExternalCode.Split('#');
            int count = 0;

            foreach (var item in array)
            {
                if (count == 0)
                    return item;
                count++;
            }

            return ExternalCode;
        }

    }

    public class TransactionItem : IPayrollType
    {
        public TransactionItem()
        {
            EmployeeId = String.Empty;
            EmployeeNr = String.Empty;
            ExternalCode = String.Empty;
            EmployeeName = String.Empty;
            EmployeeFirstName = String.Empty;
            EmployeeLastName = String.Empty;
            EmployeeSocialSec = String.Empty;
            SysPayrollTypeLevel1 = null;
            SysPayrollTypeLevel2 = null;
            SysPayrollTypeLevel3 = null;
            SysPayrollTypeLevel4 = null;
            Quantity = 0;
            Time = 0;
            ProductNr = String.Empty;
            ProductName = String.Empty;
            Comment = String.Empty;
            Amount = 0;
            VatAmount = 0;
            AccountInternals = new List<AccountInternal>();
        }

        public String EmployeeId { get; set; }
        public String EmployeeNr { get; set; }
        public int? EmployeeChildId { get; set; }
        public String ExternalCode { get; set; }
        public String EmployeeName { get; set; }
        public String EmployeeFirstName { get; set; }
        public String EmployeeLastName { get; set; }
        public String EmployeeSocialSec { get; set; }
        public decimal AbsenceRatio { get; set; }
        public decimal Quantity { get; set; }
        public double Time { get; set; }
        public DateTime Date { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public bool IsAbsence { get; set; }
        public List<AccountInternal> AccountInternals { get; set; }
        public AccountStd Account { get; set; }
        public string Comment { get; set; }
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public bool IsRegistrationQuantity { get; set; }
        public bool IsRegistrationTime { get; set; }
        public TimeSpan AbsenceStartTime { get; set; }
        public TimeSpan AbsenceStopTime { get; set; }
        public string ProductCode { get; set; }
        private bool? includedInWholeDayAbsence { get; set; }
        public string PayrollExportExternalCode { get; set; }
        public string PayrollExportUnitExternalCode { get; set; }
        public int? TimeDeviationCauseId { get; set; }

        public DateTime? TempStartDate { get; set; }
        public DateTime? TempStopDate { get; set; }

        private List<AccountInternalDTO> accountInternalDTOs { get; set; }
        public void SetAccountInternals(List<AccountInternalDTO> accountInternalDTOs)
        {
            this.accountInternalDTOs = accountInternalDTOs;
        }

        public List<AccountInternalDTO> AccountInternalDTOs
        {
            get
            {
                if (accountInternalDTOs == null)
                    return accountInternalDTOs;

                accountInternalDTOs = AccountInternals?.ToDTOs();

                if (accountInternalDTOs == null)
                    return new List<AccountInternalDTO>();

                return accountInternalDTOs;
            }
        }

        public string GroupOnAbsenceBlueGarden(bool hasTimeAbsenceDetails, List<int> mustGroupOnTimeDeviationCauseIds)
        {
            string result = ProductNr + "#" + GetExternalEmploymentCode() + "#";

            if (hasTimeAbsenceDetails && TimeDeviationCauseId.HasValue && mustGroupOnTimeDeviationCauseIds.Contains(TimeDeviationCauseId.Value))
            {
                result += TimeDeviationCauseId.Value.ToString();
            }
            else
            {
                result += "0";
            }

            return result;
        }


        public string GetExternalEmploymentCode()
        {
            if (string.IsNullOrEmpty(ExternalCode))
                return string.Empty;

            if (!ExternalCode.Contains('#'))
                return ExternalCode;

            var array = ExternalCode.Split('#');
            int count = 0;

            foreach (var item in array)
            {
                if (count == 0)
                    return item;
                count++;
            }

            return ExternalCode;
        }

        public void SetIncludedInWholeDayAbsence(bool value)
        {
            includedInWholeDayAbsence = value;
        }

        public decimal GetAbsenceRatio(List<ScheduleItem> scheduleItems, List<TransactionItem> alltransactionItemsOnDate = null)
        {
            return AbsenceRatio != 0 ? AbsenceRatio : IncludedInWholeDayAbsence(scheduleItems, alltransactionItemsOnDate) ? 100.00m : 0;
        }

        public decimal GetAbsenceRatioFromTransactions(List<ScheduleItem> scheduleItems, List<TransactionItem> alltransactionItemsOnDate = null)
        {
            if (alltransactionItemsOnDate.Any() && alltransactionItemsOnDate.Sum(s => s.Quantity) == 0) // Zeroday
                return 0;

            if (IncludedInWholeDayAbsence(scheduleItems, alltransactionItemsOnDate))
                return 100.00m;

            var correspondingScheduleItems = scheduleItems.Where(s => s.Date == this.Date).ToList();

            int scheduleTotalMinutes = 0;
            correspondingScheduleItems.ForEach(s => scheduleTotalMinutes += ((int)s.TotalMinutes - (int)s.TotalBreakMinutes));

            if (this.Quantity == scheduleTotalMinutes)
                return 100.00m;



            if (alltransactionItemsOnDate != null && alltransactionItemsOnDate.Count > 0 && scheduleTotalMinutes != 0)
                return decimal.Multiply(100, decimal.Divide(alltransactionItemsOnDate.Where(w => w.GetProductCode() == this.GetProductCode()).Sum(s => s.Quantity), scheduleTotalMinutes));

            return 0.00m;
        }

        public bool IncludedInWholeDayAbsence(List<ScheduleItem> scheduleItems, List<TransactionItem> alltransactionItemsOnDate = null, bool checkRatio = false)
        {
            if (!this.IsAbsence || !this.IsRegistrationTime)
                return false;
            if (checkRatio && AbsenceRatio != 0)
                return true;

            if (includedInWholeDayAbsence.HasValue)
                return includedInWholeDayAbsence.Value;

            var correspondingScheduleItems = scheduleItems.Where(s => s.Date == this.Date).ToList();

            int scheduleTotalMinutes = 0;
            correspondingScheduleItems.ForEach(s => scheduleTotalMinutes += ((int)s.TotalMinutes - (int)s.TotalBreakMinutes));

            if (this.Quantity == scheduleTotalMinutes)
                return true;

            if (alltransactionItemsOnDate != null && alltransactionItemsOnDate.Count > 0)
            {
                return alltransactionItemsOnDate.Where(w => w.GetProductCode() == this.GetProductCode()).Sum(s => s.Quantity) == scheduleTotalMinutes;
            }

            return false;

        }

        public decimal GetAdditionalAbsenceMinutes(List<ScheduleItem> scheduleItems, List<TransactionItem> alltransactionItemsOnDate, decimal ratio, int timeDeviationCauseId)
        {
            var correspondingScheduleItems = scheduleItems.Where(s => s.Date == this.Date).ToList();

            if (ratio > 1)
                ratio /= 100;

            int scheduleTotalMinutes = 0;
            correspondingScheduleItems.ForEach(s => scheduleTotalMinutes += ((int)s.TotalMinutes - (int)s.TotalBreakMinutes));
            var absenceOnDay = alltransactionItemsOnDate.Where(w => w.Date == this.Date && w.TimeDeviationCauseId == this.TimeDeviationCauseId && w.TimeDeviationCauseId == TimeDeviationCauseId && w.IsAbsence()).Sum(s => s.Quantity);

            if (scheduleTotalMinutes == 0 || absenceOnDay == 0)
                return 0;

            var absenceBasedOnRatio = decimal.Multiply(scheduleTotalMinutes, ratio);

            if (absenceBasedOnRatio > absenceOnDay)
                return 0;

            var diff = decimal.Divide(absenceOnDay, absenceBasedOnRatio);

            if (Math.Abs(diff - ratio) * 100 < 1)
                return 0;

            return Convert.ToInt16(absenceOnDay - (decimal.Multiply(scheduleTotalMinutes, ratio)));
        }

        public bool IsZero()
        {
            return this.Quantity == 0;
        }

        public string GetProductCode()
        {
            if (!string.IsNullOrEmpty(this.ProductCode))
                return this.ProductCode;
            else
                return this.ProductNr;
        }
    }
}
