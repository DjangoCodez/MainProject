using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    /**
     * Implementation for AgdaLon salary
     *      
     * */
    public class AgdaLonAdapter : ISalarySplittedFormatAdapter
    {
        SettingManager settingManager = new SettingManager(null);

        private List<TransactionItem> payrollTransactionItems;
        private List<ScheduleItem> scheduleItems;
        private String externalExportId;
        private bool doNotincludeComments;
        private List<TimeDeviationCause> timeDeviationCauses;

        public AgdaLonAdapter(List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, String externalExportId, bool doNotIncludeComments, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
            this.externalExportId = externalExportId;
            this.doNotincludeComments = doNotIncludeComments;
            this.timeDeviationCauses = timeDeviationCauses;
        }

        #region Salary

        /// <summary>
        /// Transforming to AgdaLon format
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>
        public byte[] TransformSalary(XDocument baseXml)
        {
            string doc = "";
            doc += GetTimeTransactions();
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);
        }

        #region Data

        private string GetTimeTransactions()
        {
            var sb = new StringBuilder();

            List<IGrouping<String, TransactionItem>> transactionItemsGroupByEmployeeId = payrollTransactionItems.GroupBy(o => o.EmployeeId).ToList();

            foreach (IGrouping<String, TransactionItem> item in transactionItemsGroupByEmployeeId)
            {
                if (externalExportId == "M2")
                    sb.Append(GetEmployeePresenceBasedTransactionsPerDay(item.ToList()));
                else
                    sb.Append(GetEmployeePresenceBasedTransactions(item.ToList()));
            }

            return sb.ToString();
        }

        private string GetEmployeePresenceBasedTransactions(List<TransactionItem> employeeTransactions)
        {
            var sb = new StringBuilder();

            List<TransactionItem> presenceTransactions = employeeTransactions.Where(t => !t.IsAbsence && t.Amount == 0).ToList();

            List<TransactionItem> AdditionDeductionTransactions = employeeTransactions.Where(t => !t.IsAbsence && t.Amount != 0).ToList();

            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions = GetCoherentTransactions(presenceTransactions, true);
            coherentPresenceTransactions = MergePresenceTransactions(coherentPresenceTransactions);

            foreach (var item in AdditionDeductionTransactions)
            {
                coherentPresenceTransactions.Add(Tuple.Create(item.Date, item.AbsenceStartTime, item.Date, item.AbsenceStopTime, item));
            }

            foreach (var transaction in coherentPresenceTransactions)
            {
                sb.Append(GetSalaryTransactionElement(transaction.Item5, transaction.Item1, transaction.Item2, transaction.Item3, transaction.Item4));
            }
            return sb.ToString();
        }

        private string GetEmployeePresenceBasedTransactionsPerDay(List<TransactionItem> employeeTransactions)
        {
            var sb = new StringBuilder();

            foreach (var productCodeGroup in employeeTransactions.Where(t => !t.IsAbsence).GroupBy(g => $"{g.ProductNr}#{g.Date}#{GetAccountNr(TermGroup_SieAccountDim.CostCentre, g.AccountInternals)}#{GetAccountNr(TermGroup_SieAccountDim.Project, g.AccountInternals)}"))
            {
                TransactionItem item = new TransactionItem();
                item = productCodeGroup.First().CloneDTO();
                item.Quantity = productCodeGroup.Sum(s => s.Quantity);
                item.Time = productCodeGroup.Sum(s => s.Time);
                item.Amount = productCodeGroup.Sum(s => s.Amount);
                item.VatAmount = productCodeGroup.Sum(s => s.VatAmount);

                sb.Append(GetSalaryTransactionElement(item, new DateTime(), new TimeSpan(), new DateTime(), new TimeSpan()));
            }
            return sb.ToString();
        }

        private List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> GetCoherentTransactions(List<TransactionItem> transactionItems, bool isPresence)
        {
            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => o.ProductNr).ToList();
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentTransactions = new List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>>();

            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                List<TransactionItem> transactionsOrderedByDate = transactionItemsForProductNumber.OrderBy(o => o.Date).ToList();

                DateTime? stopDate = null;
                TimeSpan? stopTime = null;
                TransactionItem firstTransInSequence = null;
                int dayIntervall = 0;
                int counter = 0;

                decimal accQuantity = 0;
                double accTime = 0;
                string accComment = String.Empty;
                decimal accAmount = 0;

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
                        if (doNotincludeComments)
                        {
                            accComment += "";
                        }
                        else
                        {
                            accComment += currentItem.Comment;
                        }
                        accAmount += currentItem.Amount;

                        dayIntervall++;
                    }
                    else
                    {
                        //end of seqence is reached
                        TransactionItem coherentTrnsaction = new TransactionItem
                        {
                            EmployeeId = firstTransInSequence.EmployeeId,
                            EmployeeName = firstTransInSequence.EmployeeName,
                            ExternalCode = firstTransInSequence.ExternalCode,
                            EmployeeNr = firstTransInSequence.EmployeeNr,
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

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));

                        //currentItem is the first item in the new sequence, it can also be the last one!
                        firstTransInSequence = currentItem;
                        stopDate = currentItem.Date;
                        stopTime = currentItem.AbsenceStopTime;
                        accQuantity = currentItem.Quantity;
                        accTime = currentItem.Time;
                        if (doNotincludeComments)
                        {
                            accComment += "";
                        }
                        else
                        {
                            accComment += currentItem.Comment;
                        }
                        accAmount = currentItem.Amount;
                        dayIntervall = 1;
                    }

                    if (counter == transactionsOrderedByDate.Count)
                    {
                        TransactionItem coherentTrnsaction = new TransactionItem
                        {
                            EmployeeId = firstTransInSequence.EmployeeId,
                            EmployeeName = firstTransInSequence.EmployeeName,
                            ExternalCode = firstTransInSequence.ExternalCode,
                            EmployeeNr = firstTransInSequence.EmployeeNr,
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

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));
                    }
                }
            }

            return coherentTransactions;
        }

        private bool IsTransactionCoherent(TransactionItem transactionInSeqence, TransactionItem transactionToMatch, int dayIntervall, bool isPresence)
        {
            if (transactionInSeqence.Date.AddDays(dayIntervall) == transactionToMatch.Date || isPresence || transactionToMatch.Amount != 0)
            {
                if (transactionInSeqence.AccountInternals != null && transactionToMatch.AccountInternals != null && transactionInSeqence.AccountInternals.Count == transactionToMatch.AccountInternals.Count)
                {
                    //math the accountinternals that we export
                    if ((GetAccountNr(TermGroup_SieAccountDim.CostCentre, transactionInSeqence.AccountInternals) == GetAccountNr(TermGroup_SieAccountDim.CostCentre, transactionToMatch.AccountInternals))
                        &&
                        (GetAccountNr(TermGroup_SieAccountDim.Project, transactionInSeqence.AccountInternals) == GetAccountNr(TermGroup_SieAccountDim.Project, transactionToMatch.AccountInternals))
                        &&
                        (GetAccountNr(TermGroup_SieAccountDim.CostUnit, transactionInSeqence.AccountInternals) == GetAccountNr(TermGroup_SieAccountDim.CostUnit, transactionToMatch.AccountInternals)))
                    {
                        return true;
                    }
                }
                else if (transactionInSeqence.AccountInternals == null && transactionToMatch.AccountInternals == null)
                    return true;
            }

            return false;
        }

        private string GetSalaryTransactionElement(TransactionItem transaction, DateTime start, TimeSpan startTime, DateTime stop, TimeSpan stopTime)
        {
            var sb = new StringBuilder();

            if (transaction != null && !transaction.IsAbsence)
            {
                //Separator
                string separator = ",";

                //employeeNr
                sb.Append(transaction.EmployeeNr);
                sb.Append(separator);

                //Productnumber
                sb.Append(transaction.ProductNr);
                sb.Append(separator);

                //Quantity
                if (transaction.Quantity == 0)
                    sb.Append("");
                else
                {
                    if (transaction.IsRegistrationQuantity)
                        sb.Append(GetAgdaLonDecimalValue(transaction.Quantity.ToString()));
                    else
                        sb.Append(GetAgdaLonTimeDecimalValue(transaction.Quantity.ToString()));
                }
                sb.Append(separator);

                //Sum
                if (transaction.Amount == 0)
                    sb.Append("");
                else
                {
                    sb.Append("");
                    sb.Append(GetAgdaLonDecimalValue(transaction.Amount.ToString()));
                }
                sb.Append(separator);

                //Comment
                if (!doNotincludeComments)
                {
                    sb.Append(transaction.Comment.Replace(separator, " "));
                }
                else
                {
                    sb.Append("");
                }
                sb.Append(separator);

                //Cost centre
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals));

                var project = GetAccountNr(TermGroup_SieAccountDim.Project, transaction.AccountInternals);

                if (!string.IsNullOrEmpty(project))
                {
                    sb.Append(separator);
                    sb.Append(project);
                }

                //Date
                if (externalExportId == "M2")
                {
                    // Special functionaly for Myrorna, added date on the end
                    sb.Append(separator);
                    sb.Append(transaction.Date.ToShortDateString());
                }
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

            doc += GetSchedule();
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);

        }

        private string GetSchedule()
        {
            var sb = new StringBuilder();
            //SplitSchduleItemsAccourdingToBreaks();

            List<ScheduleItem> scheduleItemsWithoutBreaks = scheduleItems.Where(s => !s.IsBreak).ToList();
            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = scheduleItemsWithoutBreaks.GroupBy(o => o.EmployeeId).ToList();

            foreach (var tempScheduleItemsForEmployee in scheduleItemsGroupByEmployeeId)
            {
                //EmployeeId to get the right transactions in order to find the deviations
                var employeeInSchedule = tempScheduleItemsForEmployee.FirstOrDefault();
                string employeeId = employeeInSchedule.EmployeeId;
                List<TransactionItem> transactionItemsEmployeeAbsence = payrollTransactionItems.Where(o => o.EmployeeId == employeeId && o.IsAbsence).ToList();

                //Create new list of Scheduleitem, wi will later add items to this list
                List<ScheduleItem> scheduleItemsForEmployee = new List<ScheduleItem>();

                foreach (ScheduleItem temp in tempScheduleItemsForEmployee)
                {
                    //Temp
                    List<TransactionItem> transactionItemsEmployeeAbsenceForDate = transactionItemsEmployeeAbsence.Where(o => o.Date == temp.Date).ToList();

                    // if C2 format - we need to order the absences so that those with value "AgdaExtra1:" for fixed percentage in ExternalCode are first on each date
                    if (externalExportId == "C2")
                    {
                        transactionItemsEmployeeAbsenceForDate = transactionItemsEmployeeAbsenceForDate
                        .OrderBy(transaction =>
                        {
                            if (timeDeviationCauses != null && transaction.TimeDeviationCauseId.HasValue)
                            {
                                var matchingCause = timeDeviationCauses.FirstOrDefault(tdc => tdc.TimeDeviationCauseId == transaction.TimeDeviationCauseId.Value);
                                if (matchingCause != null && !string.IsNullOrEmpty(matchingCause.ExtCode) && matchingCause.ExtCode.Contains("AgdaExtra1:"))
                                {
                                    return 0;
                                }
                            }
                            return 1;
                        })
                        .ToList();
                    }

                    List<IGrouping<String, TransactionItem>> transactionItemsEmployeeAbsenceForDateOnProductNr = transactionItemsEmployeeAbsenceForDate.GroupBy(o => o.ProductNr).ToList();
                    if (transactionItemsEmployeeAbsenceForDateOnProductNr.Any())
                    {
                        foreach (var absenceTransaction in transactionItemsEmployeeAbsenceForDateOnProductNr)
                        {
                            string externalCode = "";
                            double time = 0;
                            absenceTransaction.ToList().ForEach(x => time += x.Time);
                            if (timeDeviationCauses != null && timeDeviationCauses.Any())
                                externalCode = timeDeviationCauses.FirstOrDefault(w => w.TimeDeviationCauseId == absenceTransaction.FirstOrDefault()?.TimeDeviationCauseId)?.ExtCode ?? string.Empty;

                            ScheduleItem newItem = new ScheduleItem();
                            newItem.EmployeeId = temp.EmployeeId;
                            newItem.EmployeeName = temp.EmployeeName;
                            newItem.EmployeeNr = temp.EmployeeNr;
                            newItem.TimeScheduleTemplatePeriodId = temp.TimeScheduleTemplatePeriodId;
                            newItem.StartDate = temp.StartDate;
                            newItem.DayNumber = temp.DayNumber;
                            newItem.Date = temp.Date;
                            newItem.IsBreak = temp.IsBreak;
                            newItem.ProductNumber = absenceTransaction.Key;
                            newItem.AbsenceMinutes = time;
                            newItem.TotalMinutes = temp.TotalMinutes;
                            newItem.TotalBreakMinutes = temp.TotalBreakMinutes;
                            newItem.ExtraShift = temp.ExtraShift;
                            newItem.ExternalCode = externalCode;

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
                    string separator = ",";
                    bool agdaExtra1 = this.externalExportId == "C2" && item.ExternalCode.Contains("AgdaExtra1:");

                    sb.Append(item.EmployeeNr);
                    sb.Append(separator);

                    sb.Append(item.Date.ToShortDateString());
                    sb.Append(separator);

                    sb.Append(item.Date.ToShortDateString());
                    sb.Append(separator);

                    //WorkType
                    sb.Append(separator);

                    //Deviationcode = PayrollproductNumber
                    sb.Append(item.ProductNumber);
                    sb.Append(separator);

                    //Absence Percent
                    if (externalExportId != "C2")
                        sb.Append(separator);

                    //AbsenceTime
                    if (item.AbsenceMinutes == 0 || (agdaExtra1))
                        sb.Append("");
                    else
                        sb.Append(GetAgdaLonMinutesValue(item.AbsenceMinutes.ToString(), ((externalExportId == "C" || externalExportId == "C2") ? externalExportId : ""), false));

                    sb.Append(separator);

                    //ScheduleTime
                    double scheduleTime = item.TotalMinutes > 0 ? item.TotalMinutes - item.TotalBreakMinutes : 0;
                    sb.Append((GetAgdaLonMinutesValue(scheduleTime.ToString(), externalExportId, true)));

                    if (agdaExtra1)
                    {
                        string externalCode = item.ExternalCode.Split(':')[1].Trim();
                        if (externalCode != "")
                        {
                            sb.Append(separator);
                            sb.Append(externalCode);
                        }
                    }
                    else if (this.externalExportId == "C2")
                    {
                        sb.Append(separator);
                    }
                    sb.Append(Environment.NewLine);
                }

            }
            return sb.ToString();
        }

        private string GetSchedule2()
        {
            var sb = new StringBuilder();
            //SplitSchduleItemsAccourdingToBreaks();

            List<ScheduleItem> scheduleItemsWithoutBreaks = scheduleItems.Where(s => !s.IsBreak).ToList();
            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = scheduleItemsWithoutBreaks.GroupBy(o => o.EmployeeId).ToList();

            foreach (var tempScheduleItemsForEmployee in scheduleItemsGroupByEmployeeId)
            {
                //EmployeeId to get the right transactions in order to find the deviations
                var employeeInSchedule = tempScheduleItemsForEmployee.FirstOrDefault();
                string employeeId = employeeInSchedule.EmployeeId;
                List<TransactionItem> transactionItemsEmployeeAbsence = payrollTransactionItems.Where(o => o.EmployeeId == employeeId && o.IsAbsence).ToList();

                //Create new list of Scheduleitem, wi will later add items to this list
                List<ScheduleItem> scheduleItemsForEmployee = new List<ScheduleItem>();

                foreach (ScheduleItem temp in tempScheduleItemsForEmployee)
                {
                    //Temp
                    List<TransactionItem> transactionItemsEmployeeAbsenceForDate = transactionItemsEmployeeAbsence.Where(o => o.Date == temp.Date).ToList();
                    List<IGrouping<String, TransactionItem>> transactionItemsEmployeeAbsenceForDateOnProductNr = transactionItemsEmployeeAbsenceForDate.GroupBy(o => o.ProductNr).ToList();
                    if (transactionItemsEmployeeAbsenceForDateOnProductNr.Any())
                    {
                        foreach (var absenceTransaction in transactionItemsEmployeeAbsenceForDateOnProductNr)
                        {
                            double time = 0;
                            absenceTransaction.ToList().ForEach(x => time += x.Time);

                            ScheduleItem newItem = new ScheduleItem();
                            newItem.EmployeeId = temp.EmployeeId;
                            newItem.EmployeeName = temp.EmployeeName;
                            newItem.EmployeeNr = temp.EmployeeNr;
                            newItem.TimeScheduleTemplatePeriodId = temp.TimeScheduleTemplatePeriodId;
                            newItem.StartDate = temp.StartDate;
                            newItem.DayNumber = temp.DayNumber;
                            newItem.Date = temp.Date;
                            newItem.IsBreak = temp.IsBreak;
                            newItem.ProductNumber = absenceTransaction.Key;
                            newItem.AbsenceMinutes = time;
                            newItem.TotalMinutes = temp.TotalMinutes;
                            newItem.TotalBreakMinutes = temp.TotalBreakMinutes;
                            newItem.ExtraShift = temp.ExtraShift;

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
                    string separator = ";";

                    sb.Append(item.EmployeeNr);
                    sb.Append(separator);
                    sb.Append(item.Date.ToString("yyyyMMdd"));
                    sb.Append(separator);
                    double scheduleTime = item.TotalMinutes > 0 ? item.TotalMinutes - item.TotalBreakMinutes : 0;
                    var scheduleRounded = decimal.Round(decimal.Divide(Convert.ToInt32(scheduleTime), 60), 2);
                    sb.Append(GetAgdaLonTimeDecimalValue(scheduleRounded.ToString()));
                    sb.Append(separator);
                    sb.Append(item.ProductNumber);
                    sb.Append(separator);
                    //AbsenceTime
                    if (item.AbsenceMinutes == 0)
                        sb.Append("");
                    else
                    {
                        var hours = decimal.Round(decimal.Divide(Convert.ToInt32(item.AbsenceMinutes), 60), 2);
                        sb.Append(hours.ToString().Replace(".", ","));
                    }

                    sb.Append(separator);
                    sb.Append(Environment.NewLine);
                }

            }
            return sb.ToString();
        }
        #endregion

        #region Help methods

        private List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> MergePresenceTransactions(List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions)
        {
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> mergedCoherentPresenceTransactions = new List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>>();
            coherentPresenceTransactions = coherentPresenceTransactions.Where(x => !x.Item5.IsAbsence).OrderBy(x => x.Item1).ToList();

            while (coherentPresenceTransactions.Any())
            {
                Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem> firstTuple = coherentPresenceTransactions.FirstOrDefault();
                TransactionItem firstItem = firstTuple.Item5;
                List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> matchingTransactions = new List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>>();
                coherentPresenceTransactions.Remove(firstTuple);
                matchingTransactions.Add(firstTuple);

                //find similar trasactions to merge (we want to merge on ProductNr)
                List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> tmp = (from i in coherentPresenceTransactions
                                                                                            where
                                                                                            i.Item5.ProductNr == firstItem.ProductNr &&
                                                                                            i.Item5.IsRegistrationQuantity == firstItem.IsRegistrationQuantity &&
                                                                                            i.Item5.IsRegistrationTime == firstItem.IsRegistrationTime &&
                                                                                            i.Item5.Amount == 0 //dont merge transactions with amounts                                             
                                                                                            select i).ToList();

                //Accountinternals must be the same and in the same order
                foreach (var itemInTmp in tmp)
                {
                    bool sameAccountCount = itemInTmp.Item5.AccountInternals.Count == firstItem.AccountInternals.Count;
                    if (sameAccountCount)
                    {
                        bool allAccountsMatch = true; //accountCount can be zero

                        for (int i = 0; i < firstItem.AccountInternals.Count; i++)
                        {
                            if (firstItem.AccountInternals[i].AccountId != itemInTmp.Item5.AccountInternals[i].AccountId)
                                allAccountsMatch = false;
                        }

                        if (allAccountsMatch)
                            matchingTransactions.Add(itemInTmp);
                    }
                }

                TransactionItem newItem = new TransactionItem()
                {
                    EmployeeId = firstItem.EmployeeId,
                    EmployeeName = firstItem.EmployeeName,
                    ExternalCode = firstItem.ExternalCode,
                    EmployeeNr = firstItem.EmployeeNr,
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
                    if (doNotincludeComments)
                    {
                        newItem.Comment += "";
                    }
                    else
                    {
                        newItem.Comment += tmpItem.Item5.Comment;
                    }
                    newItem.Amount += tmpItem.Item5.Amount;
                    newItem.VatAmount += tmpItem.Item5.VatAmount;
                }
                matchingTransactions.ForEach(i => coherentPresenceTransactions.Remove(i));

                //matchingTransactions is never empty, it always includes atleast firstitem

                DateTime startDate = matchingTransactions.FirstOrDefault().Item1;
                TimeSpan startTime = matchingTransactions.FirstOrDefault().Item2;
                DateTime stopDate = matchingTransactions.LastOrDefault().Item3;
                TimeSpan stopTime = matchingTransactions.LastOrDefault().Item4;

                mergedCoherentPresenceTransactions.Add(Tuple.Create(startDate, startTime, stopDate, stopTime, newItem));
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

        private String GetAgdaLonTimeDecimalValue(String amount)
        {
            decimal value;
            amount = amount.Replace(".", ",");
            decimal.TryParse(amount, out value);
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);
            value *= 100;
            String returnAmount = ((int)value).ToString();

            //if (returnAmount.IndexOf(",") > 0)
            //    returnAmount.Substring(returnAmount.IndexOf(","), returnAmount.Length - returnAmount.IndexOf(","));

            return returnAmount;
        }

        public string GetAgdaLonMinutesValue(String quantity, String externalExportId, bool isSchedule)
        {
            String daySchedule = externalExportId;

            if ((externalExportId == "C" || externalExportId == "C2") && int.TryParse(quantity, out int parsedMinutes))
            {
                int hours = Convert.ToInt32(parsedMinutes / 60);
                var remaingMinutes = Convert.ToInt32(parsedMinutes - (hours * 60));
                var decimalMinutes = Convert.ToInt32(decimal.Round(decimal.Multiply(decimal.Round(decimal.Divide(remaingMinutes, 60), 2), 100), 0));
                string minutesPrefix = string.Empty;
                if (decimalMinutes < 10)
                    minutesPrefix = "0";

                if (isSchedule)
                {
                    // Need to be three chars. First for hours (1 char), then decimal for minutes. so 90 minutes (parsedMinutes) is 150.
                    // but if there is more than 10 hours, we start with an A for 10 and B for 11 and so on, up to 20 hours
                    if (hours < 10)
                        return hours.ToString() + minutesPrefix + decimalMinutes.ToString();
                    else
                    {
                        string hoursString = string.Empty;
                        switch (hours)
                        {
                            case 10:
                                hoursString = "A";
                                break;
                            case 11:
                                hoursString = "B";
                                break;
                            case 12:
                                hoursString = "C";
                                break;
                            case 13:
                                hoursString = "D";
                                break;
                            case 14:
                                hoursString = "E";
                                break;
                            case 15:
                                hoursString = "F";
                                break;
                            case 16:
                                hoursString = "G";
                                break;
                            case 17:
                                hoursString = "H";
                                break;
                            case 18:
                                hoursString = "I";
                                break;
                            case 19:
                                hoursString = "J";
                                break;
                            case 20:
                                hoursString = "K";
                                break;
                            default:
                                hoursString = hours.ToString();
                                break;
                        }

                        return hoursString + minutesPrefix + decimalMinutes.ToString();
                    }
                }
                else
                {
                    return hours.ToString() + minutesPrefix + decimalMinutes.ToString();
                }
            }
            else if (externalExportId != "M")
            {
                return quantity;
            }
            else
            {
                //Using Code from Svensklön to create schedule information in only 2 positions

                // Special functionaly for Myrorna, Number Only didn't work for them
                decimal minutes;
                decimal.TryParse(quantity, out minutes);
                String dayCode = String.Empty;

                //Negative values never allowed
                if (minutes < 0)
                    minutes = 0;

                if (minutes < 600)
                {
                    dayCode = ((minutes / 60) * 10).ToString();
                    if (dayCode.Length > 2)
                    {
                        if (decimal.TryParse(dayCode, out decimal dayCodeValue))
                        {
                            dayCodeValue = Math.Round(dayCodeValue, 0);
                            dayCode = dayCodeValue.ToString().Substring(0, 2);
                        }
                        else
                            dayCode = dayCode.Substring(0, 2);
                    }

                    dayCode = dayCode.Replace(",", "");


                    if (dayCode == "0" || dayCode.Length == 0)
                        dayCode = "00";

                    if (dayCode.Length == 1) //this should not happen
                        dayCode = "0" + dayCode;

                }
                else
                {
                    decimal value = minutes;
                    if (value != 0)
                        value /= 60;
                    value = Math.Round(value, 2, MidpointRounding.ToEven);

                    int hours = (int)value;
                    string alfaHours = "00";

                    if (hours == 10)
                    {
                        alfaHours = "A";
                    }
                    if (hours == 11)
                    {
                        alfaHours = "B";
                    }
                    if (hours == 12)
                    {
                        alfaHours = "C";
                    }
                    if (hours == 13)
                    {
                        alfaHours = "D";
                    }
                    if (hours == 14)
                    {
                        alfaHours = "E";
                    }
                    if (hours == 15)
                    {
                        alfaHours = "F";
                    }
                    if (hours == 16)
                    {
                        alfaHours = "G";
                    }
                    if (hours == 17)
                    {
                        alfaHours = "H";
                    }
                    if (hours == 18)
                    {
                        alfaHours = "I";
                    }
                    if (hours == 19)
                    {
                        alfaHours = "J";
                    }
                    if (hours == 20)
                    {
                        alfaHours = "K";
                    }

                    dayCode = ((minutes / 60) * 10).ToString();
                    if (dayCode.Length > 3)
                        dayCode = dayCode.Substring(0, 3);

                    dayCode = dayCode.Replace(",", "");

                    dayCode = dayCode.Right(1);
                    dayCode = alfaHours + dayCode;

                }

                daySchedule += dayCode;

                return daySchedule;
            }
        }

        //60 => 6000
        //60.25 => 6025        
        private String GetAgdaLonDecimalValue(String quantity)
        {
            decimal value;
            quantity = quantity.Replace(".", ",");
            decimal.TryParse(quantity, out value);
            value = Math.Round(value, 2, MidpointRounding.ToEven);
            value *= 100;
            String returnQuantity = ((int)value).ToString();

            return returnQuantity;
        }
        #endregion
    }
}
