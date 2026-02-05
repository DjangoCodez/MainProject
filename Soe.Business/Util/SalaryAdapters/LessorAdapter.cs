using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    /**
     * Implementation for Lessor salary
     *      
     * */
    public class LessorAdapter(List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, String externalExportId, DateTime startDate, List<Employee> employees, List<PayrollProduct> payrollProducts, List<TimeDeviationCause> timeDeviationCauses = null) : ISalarySplittedFormatAdapter
    {

        private readonly List<TransactionItem> payrollTransactionItems = payrollTransactions;
        private readonly List<ScheduleItem> scheduleItems = scheduleItems;
        private readonly DateTime startDate = startDate;
        private readonly string externalExportId = externalExportId;
        private readonly List<TimeDeviationCause> timeDeviationCauses = timeDeviationCauses;
        private readonly List<Employee> employees = employees;
        private readonly List<PayrollProduct> payrollProducts = payrollProducts;

        #region Salary

        /// <summary>
        /// Transforming to Lessor format
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>

        public byte[] TransformSalary(XDocument baseXml)
        {
            string transaction = GetTimeTransactions();
            string absence = GetAbsence();
            string guid = this.externalExportId + DateTime.Now.Millisecond.ToString();
            string tempfolder = ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_SALARY_PHYSICAL;
            string zippedpath = $@"{tempfolder}\lessor_{guid}.zip";

            Dictionary<string, string> dict = new()
            {
                { $@"{tempfolder}\payroll_{startDate:yyyyMM}.txt", transaction },
                { $@"{tempfolder}\absence_{startDate:yyyyMM}.txt", absence }

            };

            if (ZipUtility.ZipFiles(zippedpath, dict))
            {
                var result = File.ReadAllBytes(zippedpath);
                File.Delete(zippedpath);

                return result;
            }

            return null;
        }

        #region Data

        private string GetTimeTransactions()
        {
            var sb = new StringBuilder();

            List<IGrouping<String, TransactionItem>> transactionItemsGroupByEmployeeId = [.. payrollTransactionItems.GroupBy(o => o.EmployeeId)];

            foreach (IGrouping<String, TransactionItem> item in transactionItemsGroupByEmployeeId)
            {
                sb.Append(GetEmployeePresenceBasedTransactions([.. item]));
            }

            return sb.ToString();
        }

        private string GetEmployeePresenceBasedTransactions(List<TransactionItem> employeeTransactions)
        {
            var sb = new StringBuilder();

            List<TransactionItem> presenceTransactions = [.. employeeTransactions.Where(t => !t.IsAbsence && t.Amount == 0)];

            List<TransactionItem> additionDeductionTransactions = [.. employeeTransactions.Where(t => !t.IsAbsence && t.Amount != 0)];

            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions = GetCoherentTransactions(presenceTransactions, true);
            coherentPresenceTransactions = MergePresenceTransactions(coherentPresenceTransactions);

            foreach (var item in additionDeductionTransactions)
            {
                coherentPresenceTransactions.Add(Tuple.Create(item.Date, item.AbsenceStartTime, item.Date, item.AbsenceStopTime, item));
            }

            foreach (var transaction in coherentPresenceTransactions)
            {
                sb.Append(GetSalaryTransactionElement(transaction.Item5));  
            }
            return sb.ToString();
        }

        private List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> GetCoherentTransactions(List<TransactionItem> transactionItems, bool isPresence)
        {
            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = [.. transactionItems.GroupBy(o => o.ProductNr)];
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentTransactions = [];

            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                List<TransactionItem> transactionsOrderedByDate = [.. transactionItemsForProductNumber.OrderBy(o => o.Date)];

                DateTime? stopDate = null;
                TimeSpan? stopTime = null;
                TransactionItem firstTransInSequence = null;
                int dayIntervall = 0;
                int counter = 0;

                var accQuantity = decimal.Zero;
                double accTime = 0;
                var accComment = new StringBuilder();
                var accAmount = decimal.Zero;

                foreach (var currentItem in transactionsOrderedByDate)
                {
                    counter++;

                    if (counter == 1)
                    {
                        firstTransInSequence = currentItem;
                    }


                    //look if item is in the current datesequnce if Absence
                    if (IsTransactionCoherent(firstTransInSequence, currentItem, dayIntervall, isPresence))
                    {
                        stopDate = currentItem.Date;
                        stopTime = currentItem.AbsenceStopTime;

                        accQuantity += currentItem.Quantity;
                        accTime += currentItem.Time;
                        accComment.Append(currentItem.Comment);
                        accAmount += currentItem.Amount;

                        dayIntervall++;
                    }
                    else
                    {
                        int.TryParse(firstTransInSequence.EmployeeId, out var firstEmpId);
                        var employee = employees.FirstOrDefault(w => w.EmployeeId == firstEmpId);
                        //end of seqence is reached
                        TransactionItem coherentTrnsaction = new()
                        {
                            EmployeeId = firstTransInSequence.EmployeeId,
                            EmployeeName = firstTransInSequence.EmployeeName,
                            ExternalCode = firstTransInSequence.ExternalCode,
                            EmployeeNr = employee?.ExternalCode != "" ? employee.ExternalCode : firstTransInSequence.EmployeeNr,
                            Quantity = accQuantity,
                            Time = accTime,
                            ProductNr = firstTransInSequence.ProductNr,
                            IsAbsence = firstTransInSequence.IsAbsence,
                            AccountInternals = firstTransInSequence.AccountInternals,
                            Account = firstTransInSequence.Account,
                            Comment = accComment.ToString(),
                            Amount = accAmount,
                            IsRegistrationQuantity = firstTransInSequence.IsRegistrationQuantity,
                            IsRegistrationTime = firstTransInSequence.IsRegistrationTime,
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,

                        };

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));

                        //currentItem is the first item in the new sequence, it can also be the last one!
                        firstTransInSequence = currentItem;
                        stopDate = currentItem.Date;
                        stopTime = currentItem.AbsenceStopTime;
                        accQuantity = currentItem.Quantity;
                        accTime = currentItem.Time;
                        accComment.Append(currentItem.Comment);
                        accAmount = currentItem.Amount;
                        dayIntervall = 1;
                    }

                    if (counter == transactionsOrderedByDate.Count)
                    {
                        TransactionItem coherentTrnsaction = GetCoherentTrnsaction(firstTransInSequence, accQuantity, accTime, accComment.ToString(), accAmount, employees);

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));
                    }
                }
            }

            return coherentTransactions;
        }

        private static TransactionItem GetCoherentTrnsaction(TransactionItem firstTransInSequence, decimal accQuantity, double accTime, string accComment, decimal accAmount, List<Employee> employees)
        {
            int.TryParse(firstTransInSequence.EmployeeId, out var firstEmpId);
            var employee = employees.FirstOrDefault(w => w.EmployeeId == firstEmpId);

            return new TransactionItem
            {
                EmployeeId = firstTransInSequence.EmployeeId,
                EmployeeName = firstTransInSequence.EmployeeName,
                ExternalCode = firstTransInSequence.ExternalCode,
                EmployeeNr = employee?.ExternalCode != "" ? employee.ExternalCode : firstTransInSequence.EmployeeNr,
                Quantity = accQuantity,
                Time = accTime,
                ProductNr = firstTransInSequence.ProductNr,
                IsAbsence = firstTransInSequence.IsAbsence,
                AccountInternals = firstTransInSequence.AccountInternals,
                Account = firstTransInSequence.Account,
                Comment = accComment,
                Amount = accAmount,
                IsRegistrationQuantity = firstTransInSequence.IsRegistrationQuantity,
                IsRegistrationTime = firstTransInSequence.IsRegistrationTime,
                TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,

            };
        }

        private bool IsTransactionCoherent(TransactionItem transactionInSeqence, TransactionItem transactionToMatch, int dayIntervall, bool isPresence)
        {
            if (transactionInSeqence.Date.AddDays(dayIntervall) == transactionToMatch.Date || isPresence || transactionToMatch.Amount != 0)
            {
                if (transactionInSeqence.AccountInternals == null && transactionToMatch.AccountInternals == null)
                {
                    return true;
                }
                else
                {
                    //math the accountinternals that we export
                    if ((GetAccountNr(TermGroup_SieAccountDim.CostCentre, transactionInSeqence.AccountInternals) == GetAccountNr(TermGroup_SieAccountDim.CostCentre, transactionToMatch.AccountInternals)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string GetSalaryTransactionElement(TransactionItem transaction)
        {
            var sb = new StringBuilder();

            if (transaction != null && !transaction.IsAbsence && transaction.Quantity > 0)
            {
                //Separator
                string separator = ";";

                //employeeNr
                sb.Append(transaction.EmployeeNr);
                sb.Append(separator);

                //Productnumber
                sb.Append(transaction.ProductNr);
                sb.Append(separator);

                //Quantity
                sb.Append((transaction.Quantity / 60).ToString());
                sb.Append(separator);

                //Amount
                sb.Append("");  //empty for now
                sb.Append(separator);

                //Tom1
                sb.Append("");  
                sb.Append(separator);

                //Tom2
                sb.Append("");
                sb.Append(separator);

                //Tom3
                sb.Append("");
                sb.Append(separator);

                //Tom4
                sb.Append("");
                sb.Append(separator);

                //Cost centre
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals));

                sb.Append(Environment.NewLine);

            }

            return sb.ToString();
        }

        #endregion

        #endregion

        #region Schedule
        public byte[] TransformSchedule(XDocument baseXml)
        {
            string doc = string.Empty;

            doc += GetAbsence();
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);

        }
        private string GetAbsence()
        {
            var sb = new StringBuilder();

            List<ScheduleItem> scheduleItemsWithoutBreaks = [.. scheduleItems.Where(s => !s.IsBreak)];
            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = [.. scheduleItemsWithoutBreaks.GroupBy(o => o.EmployeeId)];

            foreach (var tempScheduleItemsForEmployee in scheduleItemsGroupByEmployeeId)
            {
                //EmployeeId to get the right transactions in order to find the deviations
                var employeeInSchedule = tempScheduleItemsForEmployee.FirstOrDefault();
                string employeeId = employeeInSchedule.EmployeeId;
                List<TransactionItem> transactionItemsEmployeeAbsence = [.. payrollTransactionItems.Where(o => o.EmployeeId == employeeId && o.IsAbsence)];

                //Create new list of Scheduleitem, wi will later add items to this list
                List<ScheduleItem> scheduleItemsForEmployee = [];

                foreach (ScheduleItem temp in tempScheduleItemsForEmployee)
                {
                    //Temp
                    List<TransactionItem> transactionItemsEmployeeAbsenceForDate = [.. transactionItemsEmployeeAbsence.Where(o => o.Date == temp.Date)];

                    int.TryParse(temp.EmployeeId, out var firstEmpId);
                    var employee = employees.FirstOrDefault(w => w.EmployeeId == firstEmpId);

                    List<IGrouping<String, TransactionItem>> transactionItemsEmployeeAbsenceForDateOnProductNr = [.. transactionItemsEmployeeAbsenceForDate.GroupBy(o => o.ProductNr)];
                    if (transactionItemsEmployeeAbsenceForDateOnProductNr.Any())
                    {
                        foreach (var absenceTransaction in transactionItemsEmployeeAbsenceForDateOnProductNr)
                        {
                            string externalCode = "";
                            double time = 0;
                            absenceTransaction.ToList().ForEach(x => time += x.Time);
                            if (timeDeviationCauses != null && timeDeviationCauses.Any())
                                externalCode = timeDeviationCauses.FirstOrDefault(w => w.TimeDeviationCauseId == absenceTransaction.FirstOrDefault()?.TimeDeviationCauseId)?.ExtCode ?? string.Empty;

                            ScheduleItem newItem = new()
                            {
                                EmployeeId = temp.EmployeeId,
                                EmployeeName = temp.EmployeeName,
                                EmployeeNr = employee?.ExternalCode != "" ? employee.ExternalCode : temp.EmployeeNr,
                                TimeScheduleTemplatePeriodId = temp.TimeScheduleTemplatePeriodId,
                                StartDate = temp.StartDate,
                                DayNumber = temp.DayNumber,
                                Date = temp.Date,
                                IsBreak = temp.IsBreak,
                                ProductNumber = absenceTransaction.Key,
                                AbsenceMinutes = time,
                                TotalMinutes = temp.TotalMinutes,
                                TotalBreakMinutes = temp.TotalBreakMinutes,
                                ExtraShift = temp.ExtraShift,
                                ExternalCode = externalCode
                            };

                            scheduleItemsForEmployee.Add(newItem);
                        }


                    }
                    else
                    {
                        temp.ProductNumber = "";
                        scheduleItemsForEmployee.Add(temp);
                    }
                }

                foreach (var item in scheduleItemsForEmployee)
                {
                    if (item.ProductNumber == "")
                        continue;

                    if (!int.TryParse(item.EmployeeId, out var empId))
                        continue;

                    var wholeDay = false;
                    var payrollProduct = payrollProducts.FirstOrDefault(w => w.ExternalNumberOrNumber == item.ProductNumber);

                    var employee = employees.FirstOrDefault(w => w.EmployeeId == empId);
                    var setting = payrollProduct.GetSetting(employee.GetPayrollGroupId(item.Date));
                    if (setting != null && setting.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.Hours)
                        wholeDay = true;

                    var separator = ";";
                    var quantity = "";

                    sb.Append(item.EmployeeNr);
                    sb.Append(separator);

                    sb.Append(item.Date.ToString("dd-MM-yyyy"));
                    sb.Append(separator);

                    //Deviationcode = PayrollproductNumber
                    sb.Append(item.ProductNumber);
                    sb.Append(separator);

                    //AbsenceTime
                    if (wholeDay) {
                        sb.Append("");
                        if (setting.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.CalenderDays || (item.AbsenceMinutes > 0))
                            quantity = "1";
                       
                    } else
                        sb.Append((item.AbsenceMinutes / 60).ToString());

                    sb.Append(separator);

                    //Quantity1
                    sb.Append(quantity);
                    sb.Append(separator);

                    //Empty
                    sb.Append("");
                    sb.Append(separator);

                    //0
                    sb.Append("0");
                    sb.Append(separator);

                    //Quantity2
                    sb.Append(quantity);

                    sb.Append(Environment.NewLine);
                }

            }
            return sb.ToString();
        }

        #endregion

        #region Help methods

        private List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> MergePresenceTransactions(List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions)
        {
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> mergedCoherentPresenceTransactions = [];
            coherentPresenceTransactions = [.. coherentPresenceTransactions.Where(x => !x.Item5.IsAbsence).OrderBy(x => x.Item1)];

            while (coherentPresenceTransactions.Any())
            {
                Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem> firstTuple = coherentPresenceTransactions.FirstOrDefault();
                TransactionItem firstItem = firstTuple.Item5;
                List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> matchingTransactions = [];
                coherentPresenceTransactions.Remove(firstTuple);
                matchingTransactions.Add(firstTuple);

                List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> tmp = [.. (from i in coherentPresenceTransactions
                                                                                            where
                                                                                            i.Item5.ProductNr == firstItem.ProductNr &&
                                                                                            i.Item5.IsRegistrationQuantity == firstItem.IsRegistrationQuantity &&
                                                                                            i.Item5.IsRegistrationTime == firstItem.IsRegistrationTime &&
                                                                                            i.Item5.Amount == 0 //dont merge transactions with amounts                                             
                                                                                            select i)];

                //Accountinternals must be the same and in the same order
                foreach (var itemInTmp in tmp)
                {
                    var sameAccountCount = itemInTmp.Item5.AccountInternals.Count == firstItem.AccountInternals.Count;
                    if (sameAccountCount)
                    {
                        var allAccountsMatch = true; //accountCount can be zero

                        for (int i = 0; i < firstItem.AccountInternals.Count; i++)
                        {
                            if (firstItem.AccountInternals[i].AccountId != itemInTmp.Item5.AccountInternals[i].AccountId)
                            {
                                allAccountsMatch = false;
                            }
                        }

                        if (allAccountsMatch)
                            matchingTransactions.Add(itemInTmp);
                    }
                }
                int.TryParse(firstItem.EmployeeId, out var firstEmpId);
                var employee = employees.FirstOrDefault(w => w.EmployeeId == firstEmpId);
                TransactionItem newItem = new()
                {
                    EmployeeId = firstItem.EmployeeId,
                    EmployeeName = firstItem.EmployeeName,
                    ExternalCode = firstItem.ExternalCode,
                    EmployeeNr = employee?.ExternalCode != "" ? employee.ExternalCode : firstItem.EmployeeNr,
                    ProductNr = firstItem.ProductNr,
                    IsAbsence = firstItem.IsAbsence,
                    AccountInternals = firstItem.AccountInternals,
                    Account = firstItem.Account,
                    IsRegistrationQuantity = firstItem.IsRegistrationQuantity,
                    IsRegistrationTime = firstItem.IsRegistrationTime,
                    TimeDeviationCauseId = firstItem.TimeDeviationCauseId,
                };

                foreach (var tmpItem in matchingTransactions)
                {
                    newItem.Quantity += tmpItem.Item5.Quantity;
                    newItem.Time += tmpItem.Item5.Time;
                    newItem.Comment += tmpItem.Item5.Comment;
                    newItem.Amount += tmpItem.Item5.Amount;
                    newItem.VatAmount += tmpItem.Item5.VatAmount;
                }
                matchingTransactions.ForEach(i => coherentPresenceTransactions.Remove(i));

                //matchingTransactions is never empty, it always includes atleast firstitem

                DateTime transStartDate = matchingTransactions.FirstOrDefault().Item1;
                TimeSpan transStartTime = matchingTransactions.FirstOrDefault().Item2;
                DateTime transStopDate = matchingTransactions.LastOrDefault().Item3;
                TimeSpan transStopTime = matchingTransactions.LastOrDefault().Item4;

                mergedCoherentPresenceTransactions.Add(Tuple.Create(transStartDate, transStartTime, transStopDate, transStopTime, newItem));
            }

            return mergedCoherentPresenceTransactions;
        }


        private String GetAccountNr(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
        {
            foreach (AccountInternal internalAccount in internalAccounts)
            {
                if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue)
                {
                    if (internalAccount.Account.AccountDim.SysSieDimNr.Value == (int)accountDim)
                        return internalAccount.Account.AccountNr;
                }
            }
            return "";
        }

        #endregion
    }
}
