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
     * Implementation for Flex Lön salary
     *      
     * */
    public class FlexLonAdapter : ISalarySplittedFormatAdapter
    {
        private List<TransactionItem> payrollTransactionItems;
        private List<ScheduleItem> scheduleItems;
        private DateTime startDate;
        private DateTime stopDate;

        public FlexLonAdapter(List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, DateTime startDate, DateTime stopDate)
        {
            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
            this.startDate = startDate;
            this.stopDate = stopDate;
            
        }

        #region Salary

        /// <summary>
        /// Transforming to Flex Lön format
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>
        public byte[] TransformSalary(XDocument baseXml)
        {
            string doc = GetTimeTransactions();
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);
        }

        #region Data

        private string GetTimeTransactions()
        {
            var sb = new StringBuilder();

            DateTime now = DateTime.Now;

            string start = GetFlexDate(startDate);
            string stop = GetFlexDate(stopDate);
            
            sb.Append("Version: 1.3 Tid och Datum: " + now + " Lönefil till FLEX Lön från SoftOne");
            sb.Append(Environment.NewLine);
            sb.Append(stop + "\t" + start + "\t" + stop);
            sb.Append(Environment.NewLine);

            List<IGrouping<String, TransactionItem>> transactionItemsGroupByEmployeeId = payrollTransactionItems.GroupBy(o => o.EmployeeId).ToList();

            foreach (IGrouping<String, TransactionItem> item in transactionItemsGroupByEmployeeId)
            {
                sb.Append(GetEmployeePresenceBasedTransactions(item.ToList()));
                sb.Append(GetEmployeeAbsenceBasedTransactions(item.ToList()));
            }

            return sb.ToString();
        }

        private string GetEmployeeAbsenceBasedTransactions(List<TransactionItem> employeeTransactions)
        {
            var sb = new StringBuilder();

            List<TransactionItem> absenceTransactions = employeeTransactions.Where(t => t.IsAbsence).ToList();
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentAbsenceTransactions = GetCoherentTransactions(absenceTransactions, false);

            foreach (var transaction in coherentAbsenceTransactions)
            {
                sb.Append(GetSalaryTransactionElement(transaction.Item5, transaction.Item1, transaction.Item2, transaction.Item3, transaction.Item4));
            }

            return sb.ToString();
        }

        private string GetEmployeePresenceBasedTransactions(List<TransactionItem> employeeTransactions)
        {
            var sb = new StringBuilder();


            List<TransactionItem> presenceTransactions = employeeTransactions.Where(t => !t.IsAbsence).ToList();

            //presenceTransactions = Merge(presenceTransactions);

            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions = GetCoherentTransactions(presenceTransactions, true);

            foreach (var transaction in coherentPresenceTransactions)
            {
                sb.Append(GetSalaryTransactionElement(transaction.Item5, transaction.Item1, transaction.Item2, transaction.Item3, transaction.Item4));
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
                    if (IsTransactionCoherent(firstTransInSequence, currentItem, dayIntervall) || isPresence)
                    {
                        stopDate = currentItem.Date;
                        stopTime = currentItem.AbsenceStopTime;

                        accQuantity += currentItem.Quantity;
                        accTime += currentItem.Time;
                        accComment += currentItem.Comment;
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
                            EmployeeNr = firstTransInSequence.EmployeeNr,
                            Quantity = accQuantity,
                            Time = accTime,
                            ProductNr = firstTransInSequence.ProductNr,
                            IsAbsence = firstTransInSequence.IsAbsence,
                            AccountInternals = firstTransInSequence.AccountInternals,
                            Account = firstTransInSequence.Account,
                            Comment = accComment,
                            Amount = accAmount,

                        };

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));

                        //currentItem is the first item in the new sequence, it can also be the last one!
                        firstTransInSequence = currentItem;
                        stopDate = currentItem.Date;
                        stopTime = currentItem.AbsenceStopTime;
                        accQuantity = currentItem.Quantity;
                        accTime = currentItem.Time;
                        accComment = currentItem.Comment;
                        accAmount = currentItem.Amount;
                        dayIntervall = 1;
                    }

                    if (counter == transactionsOrderedByDate.Count)
                    {
                        TransactionItem coherentTrnsaction = new TransactionItem
                        {
                            EmployeeId = firstTransInSequence.EmployeeId,
                            EmployeeName = firstTransInSequence.EmployeeName,
                            EmployeeNr = firstTransInSequence.EmployeeNr,
                            Quantity = accQuantity,
                            Time = accTime,
                            ProductNr = firstTransInSequence.ProductNr,
                            IsAbsence = firstTransInSequence.IsAbsence,
                            AccountInternals = firstTransInSequence.AccountInternals,
                            Account = firstTransInSequence.Account,
                            Comment = accComment,
                            Amount = accAmount,

                        };

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));
                    }
                }
            }

            return coherentTransactions;
        }

        private bool IsTransactionCoherent(TransactionItem transactionInSeqence, TransactionItem transactionToMatch, int dayIntervall)
        {
            if (transactionInSeqence.Date.AddDays(dayIntervall) == transactionToMatch.Date)
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

            if (transaction != null)
            {
                //employeeNr
                sb.Append(transaction.EmployeeNr);
                sb.Append("\t");

                //Productnumber
                sb.Append(transaction.ProductNr);
                sb.Append("\t");

                //Account (only when addition or deduction)
                if (transaction.IsRegistrationQuantity)
                {
                    sb.Append(transaction.Account.Account.AccountNr.ToString());
                    sb.Append("\t");
                }
                else
                {
                    sb.Append("");
                    sb.Append("\t");
                }


                //Cost centre
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals));
                sb.Append("\t");

                //Project
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.Project, transaction.AccountInternals));
                sb.Append("\t");

                //Cost Unit
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.CostUnit, transaction.AccountInternals));
                sb.Append("\t");

                //Employee SIE
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.Employee, transaction.AccountInternals));
                sb.Append("\t");

                //Customer SIE
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.Customer, transaction.AccountInternals));
                sb.Append("\t");

                //Invoice SIE
                sb.Append(GetAccountNr(TermGroup_SieAccountDim.Invoice, transaction.AccountInternals));
                sb.Append("\t");

                //3 extra tabs for other accountinternals
                sb.Append("\t" + "\t" + "\t");

                //Quantity
                if (transaction.Quantity == 0)
                {
                    sb.Append("0");
                    sb.Append("\t" + "");
                }
                else
                {
                    if (transaction.IsRegistrationQuantity)
                    {
                        sb.Append(GetFlexDecimalValue(transaction.Quantity));
                        sb.Append("\t" + "");
                    }
                    else
                    {
                        sb.Append(GetFlexTimeDecimalValue(transaction.Quantity));
                        sb.Append("\t" + "tim");
                    }
                }

                sb.Append("\t");

                //A-Price               
                sb.Append(GetPricePerQuantityUnit(transaction));
                sb.Append("\t");

                //Amount
                if (transaction.Amount == 0)
                    sb.Append("0" + "\t");
                else
                {
                    sb.Append(" ");
                    sb.Append(GetFlexDecimalValue(transaction.Amount));
                }


                //From date
                sb.Append(GetFlexDate(start));
                sb.Append("\t");

                //To date
                sb.Append(GetFlexDate(stop));
                sb.Append(Environment.NewLine);

            }

            return sb.ToString();
        }

        #endregion

        #endregion

        #region Schedule

        public byte[] TransformSchedule(XDocument baseXml)
        {
            string doc = GetSchedule();

            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);

        }

        private string GetSchedule()
        {
            var sb = new StringBuilder();
            
            List<ScheduleItem> scheduleItemsWithoutBreaksAndZerodays = scheduleItems.Where(s => !s.IsBreak && s.TotalMinutes > 0).ToList();

            List<ScheduleItem> mergedSchedules = MergeSchedule(scheduleItemsWithoutBreaksAndZerodays);

            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = mergedSchedules.GroupBy(o => o.EmployeeId).ToList();
             
            foreach (var scheduleItemsForEmployee in scheduleItemsGroupByEmployeeId)
            {

                foreach (var scheduleItem in scheduleItemsForEmployee)
                {
                    sb.Append(scheduleItem.EmployeeNr);
                    sb.Append(";"); //Skips DayScheduleName (using schedule time in minutes instead)
                    sb.Append(CleanStringRepresentingInt((scheduleItem.TotalMinutes - scheduleItem.TotalBreakMinutes).ToString()));
                    sb.Append(";");
                    sb.Append(GetFlexDate(scheduleItem.Date));
                    sb.Append(";");
                    sb.Append(SalaryExportUtil.DateToTimeDecimalString(scheduleItem.StartDate).ToString());
                    sb.Append(";");
                    sb.Append(GetFlexDate(scheduleItem.StartDate, scheduleItem.Date));
                    sb.Append(";");
                    sb.Append(SalaryExportUtil.DateToTimeDecimalString(scheduleItem.StopDate).ToString());
                    sb.Append(";");
                    sb.Append(CleanStringRepresentingInt(scheduleItem.TotalBreakMinutes.ToString()));

                    sb.Append(Environment.NewLine);
                }
            }
            return sb.ToString();
        }
        #endregion

        #region Help methods

        private List<TransactionItem> Merge(List<TransactionItem> items)
        {
            List<TransactionItem> result = new List<TransactionItem>();

            while (items.Any())
            {
                TransactionItem firstItem = items.FirstOrDefault();
                List<TransactionItem> matchingTransactions = new List<TransactionItem>();

                //find similar trasactions to merge
                List<TransactionItem> tmp = (from i in items
                                             where i.Date == firstItem.Date &&
                                             i.ProductNr == firstItem.ProductNr
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

                TransactionItem newItem = new TransactionItem()
                {

                    Date = firstItem.Date,
                    ProductNr = firstItem.ProductNr
                };

                foreach (var tmpItem in matchingTransactions)
                {
                    newItem.Quantity += tmpItem.Quantity;
                }
                matchingTransactions.ForEach(i => items.Remove(i));
                result.Add(newItem);
            }

            return result;
        }
          

        private String GetFlexDate(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

        private String GetFlexDate(DateTime date1900, DateTime scheduleDate)
        {
            string date = "";
            DateTime beginningOfDay1900 = date1900.Date;
            if (beginningOfDay1900 == new DateTime(1900, 01, 01))
                date = scheduleDate.ToString("yyyyMMdd");
            else if ((beginningOfDay1900 > new DateTime(1900, 01, 01)))
                date = scheduleDate.AddDays(1).ToString("yyyyMMdd");
                   
            return date;
        }

        private String GetAccountName(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
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

        private String FillWithZero(int targetSize, string originValue, bool truncate = false)
        {
            if (targetSize > originValue.Length)
            {
                string zeros = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    zeros += "0";
                }
                return (zeros + originValue);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }

        private String FillWithSpace(int targetSize, string originValue)
        {
            if (targetSize > originValue.Length)
            {
                string space = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    space += " ";
                }
                return (originValue + space);
            }
            else
                return originValue;
        }

        private String GetPricePerQuantityUnit(TransactionItem transaction)
        {
            if (transaction.Quantity == 0)
                return "0";
            else
            {
                decimal pricePerQuantityUnit;
                pricePerQuantityUnit = transaction.Amount / transaction.Quantity;
                if (pricePerQuantityUnit == 0)
                    return "0";
                else
                    return "" + GetFlexDecimalValue(pricePerQuantityUnit);
            }
        }

        private String GetFlexTimeDecimalValue(decimal minutes)
        {
            var clock = new StringBuilder();
            int hours = (int)minutes / 60;
            clock.Append(hours);
            clock.Append(".");
            int dec = ((int)minutes - (hours * 60))*100 / 60;
            if (dec >= 0 && dec < 10)
                clock.Append("0");
            string decstring = Convert.ToString(dec);  

            clock.Append(dec);
            return clock.ToString();

        }
  
        private String GetFlexDecimalValue(decimal quantity)
        {
            return quantity.ToString();
         
        }

        private String CleanStringRepresentingInt(string quantity)
        {
            String value = quantity;
            value = quantity.ToString();
            value = value.Replace(".00", "");
            value = value.Replace(",00", "");

            return value;
        }

        private List<ScheduleItem> MergeSchedule(List<ScheduleItem> scheduleItems)
        {
            List<ScheduleItem> result = new List<ScheduleItem>();
            while (scheduleItems.Any())
            {
                ScheduleItem firstItem = scheduleItems.FirstOrDefault(i => !i.IsBreak) ?? scheduleItems.FirstOrDefault();

                List<ScheduleItem> tmp = (from i in scheduleItems
                                          where i.Date == firstItem.Date &&
                                                i.EmployeeId == firstItem.EmployeeId &&
                                                i.TimeScheduleTemplatePeriodId == firstItem.TimeScheduleTemplatePeriodId
                                          select i).ToList();

                ScheduleItem newItem = new ScheduleItem()
                {
                    EmployeeId = firstItem.EmployeeId,
                    EmployeeName = firstItem.EmployeeName,
                    EmployeeFirstName = firstItem.EmployeeFirstName,
                    EmployeeLastName = firstItem.EmployeeLastName,
                    EmployeeSocialSec = firstItem.EmployeeSocialSec,
                    EmployeeNr = firstItem.EmployeeNr,
                    TimeScheduleTemplatePeriodId = firstItem.TimeScheduleTemplatePeriodId,
                    Date = firstItem.Date,
                    DayNumber = firstItem.DayNumber,
                    ProductNumber = firstItem.ProductNumber,
                    StartDate = firstItem.StartDate,
                    StopDate = firstItem.StopDate,
                    TotalBreakMinutes = firstItem.TotalBreakMinutes,
                    TotalMinutes = firstItem.TotalMinutes,
                    AbsenceMinutes = firstItem.AbsenceMinutes,
                    ExtraShift = firstItem.ExtraShift,
                    ExternalCode = firstItem.ExternalCode,
                    IsBreak = firstItem.IsBreak
                };

                tmp.ForEach(i => scheduleItems.Remove(i));
                result.Add(newItem);
            }
            return result.OrderBy(i => i.Date).ThenBy(i => i.StartDate).ToList();
        }

        #endregion
    }
}
