using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    /**
     * Implementation for Hogia salary, "Posttype 214006"
     *      
     * */
    public class Hogia214007Adapter : ISalarySplittedFormatAdapter
    {
        private string postIdentifier;
        private List<TransactionItem> payrollTransactionItems;
        private List<ScheduleItem> scheduleItems;
        private bool _Is214006;

        public Hogia214007Adapter(List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, bool is214006)
        {
            this._Is214006 = is214006;

            if (is214006)
                this.postIdentifier = "214006";
            else
                postIdentifier = "214007";

            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
        }

        #region Salary

        /// <summary>
        /// Transforming to Hogia format
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>
        public byte[] TransformSalary(XDocument baseXml)
        {
            string doc = "000000";
            doc += Environment.NewLine;
            doc += GetTimeTransactions();
            doc += "999999";
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);
        }

        #region Data

        private string GetTimeTransactions()
        {
            var sb = new StringBuilder();

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
                            IsRegistrationQuantity = firstTransInSequence.IsRegistrationQuantity,
                            IsRegistrationTime = firstTransInSequence.IsRegistrationTime,

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
                            IsRegistrationQuantity = firstTransInSequence.IsRegistrationQuantity,
                            IsRegistrationTime = firstTransInSequence.IsRegistrationTime,

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
                {
                    return true;
                }
                else if (transactionToMatch.IsZero())
                {
                    return true;
                }
            }

            return false;
        }

        private string GetSalaryTransactionElement(TransactionItem transaction, DateTime start, TimeSpan startTime, DateTime stop, TimeSpan stopTime)
        {
            var sb = new StringBuilder();

            if (transaction != null)
            {
                //Identifier
                sb.Append(postIdentifier);

                //employeeNr
                sb.Append(FillWithZero(_Is214006 ? 11 : 13, transaction.EmployeeNr));

                //Deviation/payroll
                sb.Append(transaction.IsAbsence ? "A" : "L");

                //Productnumber
                sb.Append(FillWithZero(3, transaction.ProductNr));

                //Quantity
                sb.Append(" ");
                if (transaction.Quantity == 0 && transaction.Amount == 0)
                    return string.Empty;
                else if (transaction.Quantity == 0)
                    sb.Append(FillWithZero(9, ""));
                else if (transaction.IsRegistrationQuantity)
                    sb.Append(FillWithZero(9, GetHogiaDecimalValue(transaction.Quantity.ToString())));
                else
                    sb.Append(FillWithZero(9, GetHogiaTimeDecimalValue(transaction.Quantity.ToString())));

                //A-Price               
                sb.Append(GetPricePerQuantityUnit(transaction));

                //Sum
                if (transaction.Amount == 0)
                    sb.Append(FillWithZero(17, ""));
                else
                {
                    sb.Append(" ");
                    sb.Append(FillWithZero(16, GetHogiaDecimalValue(transaction.Amount.ToString())));
                }

                //From date
                sb.Append(GetHogiaDate(start));

                //From time
                if (transaction.IsAbsence)
                    sb.Append(startTime.ToShortTimeString());
                else
                    sb.Append("00:00");

                //To date
                sb.Append(GetHogiaDate(stop.Add(stopTime).Date));

                //To time
                if (transaction.IsAbsence)
                    sb.Append(stopTime.ToShortTimeString());
                else
                    sb.Append("00:00");

                //From (Deviation) date
                if (transaction.IsAbsence)
                    sb.Append(GetHogiaDate(start));
                else
                    sb.Append("00000000");

                //To (Deviation) date
                if (transaction.IsAbsence)
                    sb.Append(GetHogiaDate(stop));
                else
                    sb.Append("00000000");

                //Reference
                sb.Append("000"); //we dont use it right now                

                //Attested
                sb.Append("01"); //we only export attested transactions

                //Treated
                sb.Append("0"); //only used by HogiaLön32 

                //Cost centre
                sb.Append(FillWithSpace(20, GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals)));

                //Project
                sb.Append(FillWithSpace(20, GetAccountNr(TermGroup_SieAccountDim.Project, transaction.AccountInternals)));

                //Account
                sb.Append(FillWithSpace(12, "")); //we dont use it right now

                //Cost unit
                sb.Append(FillWithSpace(20, GetAccountNr(TermGroup_SieAccountDim.CostUnit, transaction.AccountInternals)));

                //Range
                sb.Append("000000");

                //Comment
                //if(!String.IsNullOrEmpty(transaction.Comment) && transaction.Comment.Length > 65)
                //    sb.Append(transaction.Comment.Substring(0,65));
                //else
                //    sb.Append(String.IsNullOrEmpty(transaction.Comment) ? "" : transaction.Comment);


                // . not needed
                //sb.Append(Environment.NewLine);
                //sb.Append(".");
                sb.Append(Environment.NewLine);

            }

            return sb.ToString();
        }

        #endregion

        #endregion

        #region Schedule

        public byte[] TransformSchedule(XDocument baseXml)
        {
            string doc = "Filhuvud";
            doc += Environment.NewLine;
            doc += "\t";
            doc += Environment.NewLine;
            doc += "\t";
            doc += "Typ=\"Personschema\"";
            doc += Environment.NewLine;
            doc += "\t";
            doc += "SkapadAv=\"STANDARD\"";
            doc += Environment.NewLine;
            doc += "\t";
            doc += "DatumTid=#" + DateTime.Now + "#";
            doc += Environment.NewLine;

            doc += GetSchedule();
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);

        }

        private string GetSchedule()
        {
            var sb = new StringBuilder();
            SplitSchduleItemsAccourdingToBreaks();

            List<ScheduleItem> scheduleItemsWithoutBreaks = scheduleItems.Where(s => !s.IsBreak).ToList();
            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = scheduleItemsWithoutBreaks.GroupBy(o => o.EmployeeId).ToList();

            foreach (var scheduleItemsForEmployee in scheduleItemsGroupByEmployeeId)
            {

                ScheduleItem first = scheduleItemsForEmployee.OrderBy(s => s.Date).FirstOrDefault();
                if (first == null)
                    continue;

                ScheduleItem last = scheduleItemsForEmployee.OrderByDescending(s => s.Date).FirstOrDefault();

                string numberofScheduledDays = ((last.Date - first.Date).Days + 1).ToString();


                sb.Append("Pstart");
                sb.Append(Environment.NewLine);
                sb.Append("\t");
                sb.Append("Typ=\"Personschema\"");
                sb.Append(Environment.NewLine);
                sb.Append("\t");
                sb.Append("\t");
                sb.Append("Anställningsnummer=" + first.EmployeeNr);
                sb.Append(Environment.NewLine);
                sb.Append("\t");
                sb.Append("\t");
                sb.Append("StartDatum=#" + first.Date.ToShortDateString() + "#");
                sb.Append(Environment.NewLine);
                sb.Append("\t");
                sb.Append("\t");
                sb.Append("Längd=" + numberofScheduledDays);
                sb.Append(Environment.NewLine);

                foreach (var scheduleItem in scheduleItemsForEmployee.Where(w => w.TotalMinutes != 0))
                {
                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("Arbetspass");
                    sb.Append(Environment.NewLine);

                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("StartDag=#" + scheduleItem.Date.ToShortDateString() + "#");
                    sb.Append(Environment.NewLine);

                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("StartTid=#" + SalaryExportUtil.DateToTimeString(scheduleItem.StartDate) + "#");
                    sb.Append(Environment.NewLine);

                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("SlutDag=#" + scheduleItem.Date.AddDays((scheduleItem.StopDate.Date - scheduleItem.StartDate.Date).Days).ToShortDateString() + "#");
                    sb.Append(Environment.NewLine);

                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("SlutTid=#" + SalaryExportUtil.DateToTimeString(scheduleItem.StopDate) + "#");
                    sb.Append(Environment.NewLine);

                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("\t");
                    sb.Append("Längd=#" + SalaryExportUtil.GetClock((scheduleItem.TotalMinutes - scheduleItem.TotalBreakMinutes)).ToString() + "#");
                    sb.Append(Environment.NewLine);

                }

                sb.Append("Pslut");
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
        #endregion

        #region Help methods

        /// <summary>
        /// Should only be called for presence transactions and after GetCoherentPresenceTransactions is done
        /// It will merge sequences with same productnr and accountinternal
        /// EX:
        /// Input:
        /// 000000
        /// 21400600000002065L010 0000009580000000000000000000000000002013101000:002013101100:0000000000000000000000103                                                                       000000
        /// 21400600000002065L010 0000004170000000000000000000000000002013101300:002013101300:0000000000000000000000109                                                                       000000
        /// 21400600000002065L010 0000004330000000000000000000000000002013101400:002013101400:0000000000000000000000103                                                                       000000
        /// 21400600000002065L310 0000001830000000000000000000000000002013101000:002013101100:0000000000000000000000103                                                                       000000
        /// 21400600000002065L214 0000000050000000000000000000000000002013101100:002013101100:0000000000000000000000103                                                                       000000
        /// 21400600000002065L312 0000004170000000000000000000000000002013101300:002013101300:0000000000000000000000109                                                                       000000
        /// 999999
        /// 
        /// Output:
        /// 000000
        /// 21400600000002065L010 0000013920000000000000000000000000002013101000:002013101400:0000000000000000000000103   (1st and 3rd line is now one)                                       000000
        /// 21400600000002065L310 0000001830000000000000000000000000002013101000:002013101100:0000000000000000000000103                                                                       000000
        /// 21400600000002065L214 0000000050000000000000000000000000002013101100:002013101100:0000000000000000000000103                                                                       000000
        /// 21400600000002065L010 0000004170000000000000000000000000002013101300:002013101300:0000000000000000000000109                                                                       000000
        /// 21400600000002065L312 0000004170000000000000000000000000002013101300:002013101300:0000000000000000000000109                                                                       000000
        /// 999999

        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> MergePresenceTransactions(List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions)
        {
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> mergedCoherentPresenceTransactions = new List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>>();
            coherentPresenceTransactions = coherentPresenceTransactions.Where(x => !x.Item5.IsAbsence).OrderBy(x => x.Item1).ToList();

            while (coherentPresenceTransactions.Count > 0)
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
                    EmployeeNr = firstItem.EmployeeNr,
                    ProductNr = firstItem.ProductNr,
                    IsAbsence = firstItem.IsAbsence,
                    AccountInternals = firstItem.AccountInternals,
                    Account = firstItem.Account,
                    IsRegistrationQuantity = firstItem.IsRegistrationQuantity,
                    IsRegistrationTime = firstItem.IsRegistrationTime,
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

                DateTime startDate = matchingTransactions.FirstOrDefault().Item1;
                TimeSpan startTime = matchingTransactions.FirstOrDefault().Item2;
                DateTime stopDate = matchingTransactions.LastOrDefault().Item3;
                TimeSpan stopTime = matchingTransactions.LastOrDefault().Item4;

                mergedCoherentPresenceTransactions.Add(Tuple.Create(startDate, startTime, stopDate, stopTime, newItem));
            }

            return mergedCoherentPresenceTransactions;
        }

        private void SplitSchduleItemsAccourdingToBreaks()
        {
            List<ScheduleItem> newBlocks = new List<ScheduleItem>();

            foreach (var scheduleItem in scheduleItems)
            {
                #region FIX for wholes on day (between schedule blocks) workitem 10225
                //scheduleItem representerar dagen: startdate är första passets startdate och stopdate är sista passets stopdate
                //det innebär att startdate och stopdate inkluderar eventuella hål på dagen.
                //scheduleItem.TotalMinutes representerar totalt antal minuter för alla pass på dagen (se MergeSchedule i SoeXeAdapter) , den inkluderar alltså inte hålen

                // variabeln scheduleTotalMinutes inkluderar alltså hålen på dagen, variabeln diff kommer därmed att tala om det finns hål på dagen
                //vi lägger till hålen i minuter till TotalBreakMinutes för att simulera ett större hål helt enkelt

                double scheduleTotalMinutes = (scheduleItem.StopDate - scheduleItem.StartDate).TotalMinutes;

                bool diff = (scheduleTotalMinutes) != scheduleItem.TotalMinutes;
                if (diff)
                {
                    double diffinMinutes = scheduleTotalMinutes - scheduleItem.TotalMinutes;

                    scheduleItem.TotalBreakMinutes += diffinMinutes;
                }

                #endregion

                if (scheduleItem.TotalBreakMinutes > 0 && scheduleItem.TotalMinutes > 0)
                {
                    #region Split ScheduleBlock against break
                    /***
                     * Ex: Scheduleitem : 08:00 - 17:00, breakminutes 60 => 
                     *      08:00 - 12:00 and 13:00 - 17:00
                     * */

                    ScheduleItem newItem = new ScheduleItem();
                    newItem.EmployeeId = scheduleItem.EmployeeId;
                    newItem.EmployeeName = scheduleItem.EmployeeName;
                    newItem.EmployeeNr = scheduleItem.EmployeeNr;
                    newItem.TimeScheduleTemplatePeriodId = scheduleItem.TimeScheduleTemplatePeriodId;
                    newItem.StartDate = scheduleItem.StartDate;
                    newItem.DayNumber = scheduleItem.DayNumber;
                    newItem.Date = scheduleItem.Date;
                    newItem.IsBreak = scheduleItem.IsBreak;
                    newItem.ProductNumber = scheduleItem.ProductNumber;
                    newItem.ExtraShift = scheduleItem.ExtraShift;


                    newItem.StopDate = (newItem.StartDate.AddMinutes(Math.Round((scheduleItem.TotalMinutes / 2 - scheduleItem.TotalBreakMinutes / 2), 0, MidpointRounding.ToEven)));
                    scheduleItem.StartDate = newItem.StopDate.AddMinutes(scheduleItem.TotalBreakMinutes);

                    //Calculate new totalminutes
                    newItem.TotalMinutes = (newItem.StopDate - newItem.StartDate).TotalMinutes;
                    scheduleItem.TotalMinutes = (scheduleItem.StopDate - scheduleItem.StartDate).TotalMinutes;

                    //Reset breakminutes
                    newItem.TotalBreakMinutes = 0;
                    scheduleItem.TotalBreakMinutes = 0;

                    newBlocks.Add(newItem);

                    #endregion
                }
            }

            newBlocks.ForEach(b => scheduleItems.Add(b));
            scheduleItems = scheduleItems.OrderBy(o => o.Date).ThenBy(i => i.StartDate).ToList();
        }

        private String GetHogiaDate(DateTime date)
        {
            return date.ToString("yyyyMMdd");
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
                return FillWithZero(10, "");
            else
            {
                decimal pricePerQuantityUnit;
                pricePerQuantityUnit = transaction.Amount / transaction.Quantity;
                if (pricePerQuantityUnit == 0)
                    return FillWithZero(10, "");
                else
                    return " " + FillWithZero(9, GetHogiaDecimalValue(pricePerQuantityUnit.ToString()));
            }
        }

        private String GetHogiaTimeDecimalValue(String amount)
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

        //60 => 6000
        //60.25 => 6025        
        private String GetHogiaDecimalValue(String quantity)
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
