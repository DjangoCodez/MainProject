using SoftOne.Soe.Business.Util.SalaryAdapters;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public static class SalaryExportUtil
    {
        public static string DateToTimeString(DateTime dateTime)
        {
            StringBuilder sb = new StringBuilder();
            if (dateTime.Hour < 10)
                sb.Append("0");
            sb.Append(dateTime.Hour);
            sb.Append(":");
            if (dateTime.Minute < 10)
                sb.Append("0");
            sb.Append(dateTime.Minute);
            return sb.ToString();
        }

        public static string DateToTimeDecimalString(DateTime dateTime)
        {
            StringBuilder sb = new StringBuilder();
            DateTime beginofHour = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
            TimeSpan timespan = dateTime - beginofHour;
            sb.Append(dateTime.Hour);
            sb.Append(".");
            int minutes = timespan.Minutes;
            int dec = (minutes * 100) / 60;
            if (dec >= 0 && dec < 10)
                sb.Append("0");
            string decstring = Convert.ToString(dec);

            sb.Append(decstring);

            return sb.ToString();
        }

        public static string CalculateTotalBreakMin(TimeScheduleTemplatePeriod schemaPeriod)
        {
            decimal result = 0;
            foreach (var timeBlock in schemaPeriod.TimeBlock)
            {
                foreach (var timeCodeTransaction in timeBlock.TimeCodeTransaction)
                {
                    if (timeCodeTransaction.TimeCode.Type == (int)SoeTimeCodeType.Break)
                        result += timeCodeTransaction.Quantity;
                }
            }
            return result.ToString();
        }

        public static string CalculateTotalTimeMin(TimeScheduleTemplatePeriod schemaPeriod)
        {
            decimal result = 0;
            foreach (var timeBlock in schemaPeriod.TimeBlock)
            {
                foreach (var timeCodeTransaction in timeBlock.TimeCodeTransaction)
                {
                    result += timeCodeTransaction.Quantity;
                }
            }
            return result.ToString();
        }

        public static string GetGeneratedName(string name, DateTime? date)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            string tmpName = name.Length > 4 ? name.Substring(0, 4) : name;
            StringBuilder sb = new StringBuilder();
            sb.Append(tmpName);
            sb.Append(date.Value.Year.ToString().Substring(2));
            if (date.Value.Day < 10)
                sb.Append("0");
            sb.Append(date.Value.Month.ToString());
            if (date.Value.Day < 10)
                sb.Append("0");
            sb.Append(date.Value.Day.ToString());
            return sb.ToString();
        }

        public static string GetTimeFromMinutes(string amount)
        {
            decimal value;
            amount = amount.Replace(".", ",");
            decimal.TryParse(amount, out value);
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);

            int hours = (int)value;
            int minutes = 0;
            value -= hours;
            value *= 100;
            minutes = (int)value;
            minutes = Math.Abs(minutes);

            //return hours.minutes i.e 8h3m -> 8.03
            return hours.ToString() + "." + (minutes.ToString().Length == 1 ? "0" + minutes.ToString() : minutes.ToString());
        }

        public static string GetClock(double minutes, string delimeter = ":")
        {
            var clock = new StringBuilder();
            int hours = (int)minutes / 60;
            if (hours < 10)
                clock.Append("0");
            clock.Append(hours);
            clock.Append(delimeter);
            int min = (int)minutes - (hours * 60);
            if (min < 10)
                clock.Append("0");
            clock.Append(min);
            return clock.ToString();
        }

        public static List<Tuple<String, String>> GetDistinctAccounts(TermGroup_SieAccountDim accountDim, List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem> transactions)
        {
            List<Tuple<String, String>> accounts = new List<Tuple<String, String>>();

            foreach (var transaction in transactions)
            {
                if (transaction.AccountInternals == null)
                    continue;

                foreach (AccountInternal internalAccount in transaction.AccountInternals)
                {
                    if (internalAccount.Account?.AccountDim?.SysSieDimNr != null && internalAccount.Account.AccountDim.SysSieDimNr.Value == (int)accountDim && !accounts.Any(x => x.Item1 == internalAccount.Account.AccountNr))
                        accounts.Add(Tuple.Create<String, String>(internalAccount.Account.AccountNr, internalAccount.Account.Name));
                }
            }
            return accounts;
        }

        public static List<List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem>> SplitOnAccounting(List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem> transactions, List<AccountDim> accountDimInternals, List<Account> accountsWithAccountHierachyPayrollExportExternalCode, List<Account> accountsWithAccountHierachyPayrollExportUnitExternalCode)
        {
            List<List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem>> splitTrans = new List<List<SalaryAdapters.TransactionItem>>();
            List<Tuple<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem, String>> diffTrans = new List<Tuple<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem, string>>();

            string accountIdString = string.Empty;

            var accountEmptyTransactions = new List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem>();

            foreach (var trans in transactions.OrderBy(o => o.Date))
            {
                var transAccountIdString = GetAccountIdString(trans, accountDimInternals);
                if (accountsWithAccountHierachyPayrollExportExternalCode.Any())
                    trans.PayrollExportExternalCode = GetAccountHierachyPayrollExportExternalCode(trans.AccountInternals, accountsWithAccountHierachyPayrollExportExternalCode);

                if (accountsWithAccountHierachyPayrollExportUnitExternalCode.Any())
                    trans.PayrollExportUnitExternalCode = GetAccountHierachyPayrollExportUnitExternalCode(trans.AccountInternals, accountsWithAccountHierachyPayrollExportUnitExternalCode);

                if (string.IsNullOrEmpty(accountIdString) && IsAccountStringIdEmpty(transAccountIdString))  //Fix if first day is missing internal accounting
                {
                    accountEmptyTransactions.Add(trans);
                    continue;
                }

                if (string.IsNullOrEmpty(accountIdString))
                    accountIdString = transAccountIdString;

                if (!IsAccountStringIdEmpty(transAccountIdString))
                    accountIdString = transAccountIdString;

                if (accountEmptyTransactions.Count > 0)
                {
                    foreach (var tr in accountEmptyTransactions)
                    {
                        diffTrans.Add(Tuple.Create(tr, accountIdString));
                    }

                    accountEmptyTransactions = new List<TransactionItem>();
                }

                diffTrans.Add(Tuple.Create(trans, accountIdString));
            }

            if (accountEmptyTransactions.Count > 0)
            {
                foreach (var tr in accountEmptyTransactions)
                {
                    diffTrans.Add(Tuple.Create(tr, GetAccountIdString(tr, accountDimInternals)));
                }
            }

            foreach (var accountTrans in diffTrans.GroupBy(i => i.Item2))
            {
                List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem> accountTransList = new List<SalaryAdapters.TransactionItem>();

                foreach (var t in accountTrans)
                    accountTransList.Add(t.Item1);

                splitTrans.Add(accountTransList.OrderBy(d => d.Date).ToList());
            }

            return splitTrans;
        }

        private static bool IsAccountStringIdEmpty(string transAccountIdString)
        {
            var accounts = transAccountIdString.Split('#');
            int count = 0;

            foreach (var item in accounts)
            {
                if (count != 0)
                {
                    if (item != "0")
                        return false;
                }

                count++;
            }

            return true;
        }


        public static string GetAccountIdString(SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem transaction, List<AccountDim> accountDimInternals)
        {

            int dim1Id = 0;
            int dim2Id = 0;
            int dim3Id = 0;
            int dim4Id = 0;
            int dim5Id = 0;
            int dim6Id = 0;

            dim1Id = transaction.Account.AccountId;


            int dimCounter = 2;

            if (transaction.AccountInternals.Count > 0)
            {
                foreach (var accountDim in accountDimInternals)
                {
                    var accountInternal = transaction.AccountInternals.FirstOrDefault(i => i.Account != null && i.Account.AccountDimId == accountDim.AccountDimId);

                    if (accountInternal == null && !transaction.AccountInternals.IsNullOrEmpty() && !accountDim.Account.IsNullOrEmpty())
                        accountInternal = transaction.AccountInternals.FirstOrDefault(i => accountDim.Account.FirstOrDefault(f => f.AccountId == i.AccountId)?.AccountDimId != null);

                    if (accountInternal != null)
                    {
                        #region Dim 2

                        if (dimCounter == 2)
                        {
                            dim2Id = accountInternal.AccountId;
                        }

                        #endregion

                        #region Dim 3

                        if (dimCounter == 3)
                        {
                            dim3Id = accountInternal.AccountId;
                        }

                        #endregion

                        #region Dim 4

                        if (dimCounter == 4)
                        {
                            dim4Id = accountInternal.AccountId;
                        }

                        #endregion

                        #region Dim 5

                        if (dimCounter == 5)
                        {
                            dim5Id = accountInternal.AccountId;
                        }

                        #endregion

                        #region Dim 6

                        if (dimCounter == 6)
                        {
                            dim6Id = accountInternal.AccountId;

                        }

                        #endregion
                    }

                    dimCounter++;
                }
            }

            return dim1Id.ToString() + "#" + dim2Id.ToString() + "#" + dim3Id.ToString() + "#" + dim4Id.ToString() + "#" + dim5Id.ToString() + "#" + dim6Id.ToString();

        }

        public static string GetAccountHierachyPayrollExportExternalCode(List<AccountInternal> accountInternals, List<Account> accounts, string fallBack = null)
        {
            if (!accountInternals.IsNullOrEmpty())
            {
                List<int> accountIds = accountInternals.Select(s => s.AccountId).ToList();
                var accountsInCollection = accounts.Where(w => accountIds.Contains(w.AccountId)).ToList();

                foreach (var account in accountsInCollection)
                {
                    var accountInternal = accountInternals.FirstOrDefault(i => i.AccountId == account.AccountId && !string.IsNullOrEmpty(i.Account.AccountHierachyPayrollExportExternalCode));

                    if (account?.AccountHierachyPayrollExportExternalCode != null)
                    {
                        return account.AccountHierachyPayrollExportExternalCode;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(fallBack))
            {
                var match = accounts.FirstOrDefault(w => w.AccountNr.ToString() == fallBack && !string.IsNullOrEmpty(w.AccountHierachyPayrollExportExternalCode));

                if (match != null)
                    return match.AccountHierachyPayrollExportExternalCode;
            }

            return string.Empty;
        }


        public static string GetAccountHierachyPayrollExportUnitExternalCode(List<AccountInternal> accountInternals, List<Account> accounts, string fallBack = null)
        {
            if (!accountInternals.IsNullOrEmpty())
            {
                List<int> accountIds = accountInternals.Select(s => s.AccountId).ToList();
                var accountsInCollection = accounts.Where(w => accountIds.Contains(w.AccountId)).ToList();

                foreach (var account in accountsInCollection)
                {
                    var accountInternal = accountInternals.FirstOrDefault(i => i.AccountId == account.AccountId && !string.IsNullOrEmpty(i.Account.AccountHierachyPayrollExportUnitExternalCode));

                    if (account != null)
                    {
                        return account.AccountHierachyPayrollExportUnitExternalCode;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(fallBack))
            {
                var match = accounts.FirstOrDefault(w => w.AccountNr.ToString() == fallBack && !string.IsNullOrEmpty(w.AccountHierachyPayrollExportUnitExternalCode));

                if (match?.AccountHierachyPayrollExportUnitExternalCode != null)
                    return match.AccountHierachyPayrollExportUnitExternalCode;
            }

            return string.Empty;
        }



        public static List<Tuple<String, String, String>> GetDistinctAccounts(List<SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem> transactions)
        {
            List<Tuple<String, String, String>> accounts = new List<Tuple<String, String, String>>();

            foreach (var transaction in transactions)
            {
                if (transaction.AccountInternals == null)
                    continue;

                foreach (AccountInternal internalAccount in transaction.AccountInternals)
                {
                    if (internalAccount.Account?.AccountDim?.SysSieDimNr != null && !accounts.Any(x => x.Item1 == internalAccount.Account.AccountNr))
                        accounts.Add(Tuple.Create<String, String, String>(internalAccount.Account.AccountNr, internalAccount.Account.Name, internalAccount.Account.AccountDim.AccountDimNr.ToString()));
                }
            }
            return accounts;
        }

        public static List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> GetCoherentTransactions(List<TransactionItem> transactionItems, bool isPresence, List<ScheduleItem> scheduleItems, bool doNotincludeComments)
        {
            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => o.ProductCode + "#" + o.GetAbsenceRatio(scheduleItems.Where(w => w.Date == o.Date).ToList(), transactionItems.Where(w => w.Date == o.Date).ToList()).ToString()).ToList();
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
                bool isWholeDayAbsenceOrAbsenceRatioIsNotZero = transactionsOrderedByDate.First().IncludedInWholeDayAbsence(scheduleItems, checkRatio: true);

                List<DateTime> dates = transactionsOrderedByDate.OrderBy(o => o.Date).Select(s => s.Date).ToList();
                var currentDate = dates.First();
                var previousDate = currentDate.AddDays(-1);

                foreach (var currentItem in transactionsOrderedByDate)
                {
                    counter++;

                    if (counter == 1)
                    {
                        firstTransInSequence = currentItem;
                    }

                    var wholeDayChanged = isWholeDayAbsenceOrAbsenceRatioIsNotZero != currentItem.IncludedInWholeDayAbsence(scheduleItems, checkRatio: true);

                    isWholeDayAbsenceOrAbsenceRatioIsNotZero = currentItem.IncludedInWholeDayAbsence(scheduleItems, checkRatio: true);

                    if ((counter == 1 && wholeDayChanged))
                        wholeDayChanged = false;

                    if (counter > 1 && !wholeDayChanged && !isPresence && !currentItem.IncludedInWholeDayAbsence(scheduleItems, checkRatio: true)) // Make sure part of day always only one transaction row in file.
                        wholeDayChanged = true;

                    if (!wholeDayChanged && !isPresence && currentItem.Date.AddDays(-1) != previousDate) // Extra check to make sure no gapes in absence is allowed.
                        wholeDayChanged = true;

                    previousDate = currentItem.Date;

                    //look if item is in the current datesequnce if Absence
                    if (!wholeDayChanged && IsTransactionCoherent(firstTransInSequence, currentItem, dayIntervall))
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
                            ProductCode = firstTransInSequence.ProductCode,
                            Date = firstTransInSequence.Date,
                            AbsenceRatio = firstTransInSequence.AbsenceRatio,
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,
                        };

                        coherentTrnsaction.SetIncludedInWholeDayAbsence(firstTransInSequence.IncludedInWholeDayAbsence(scheduleItems, checkRatio: true));

                        if (firstTransInSequence.Date == currentItem.Date)
                            coherentTrnsaction.Date = currentItem.Date;

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.HasValue ? stopDate.Value : currentItem.Date, stopTime.HasValue ? stopTime.Value : currentItem.AbsenceStopTime, coherentTrnsaction));

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
                            ProductCode = firstTransInSequence.ProductCode,
                            Date = firstTransInSequence.Date,
                            AbsenceRatio = firstTransInSequence.AbsenceRatio,
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,
                        };

                        coherentTrnsaction.SetIncludedInWholeDayAbsence(currentItem.IncludedInWholeDayAbsence(scheduleItems, checkRatio: true));
                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));
                    }
                }
            }

            return coherentTransactions;
        }



        public static bool IsTransactionCoherent(SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem transactionInSeqence, SoftOne.Soe.Business.Util.SalaryAdapters.TransactionItem transactionToMatch, int dayIntervall)
        {
            if (transactionInSeqence.Date.AddDays(dayIntervall) == transactionToMatch.Date)
            {
                if (transactionInSeqence.AccountInternals != null && transactionToMatch.AccountInternals != null && transactionInSeqence.AccountInternals.Count == transactionToMatch.AccountInternals.Count)
                {
                    //math the accountinternals that we export
                    if ((GetAccountName(TermGroup_SieAccountDim.CostCentre, transactionInSeqence.AccountInternals) == GetAccountName(TermGroup_SieAccountDim.CostCentre, transactionToMatch.AccountInternals))
                        &&
                        (GetAccountName(TermGroup_SieAccountDim.Project, transactionInSeqence.AccountInternals) == GetAccountName(TermGroup_SieAccountDim.Project, transactionToMatch.AccountInternals))
                        &&
                        (GetAccountName(TermGroup_SieAccountDim.CostUnit, transactionInSeqence.AccountInternals) == GetAccountName(TermGroup_SieAccountDim.CostUnit, transactionToMatch.AccountInternals)))
                    {
                        return true;
                    }
                }
                else if (transactionInSeqence.AccountInternals == null && transactionToMatch.AccountInternals == null)
                    return true;
            }
            return false;
        }

        public static String GetAccountName(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
        {
            foreach (AccountInternal internalAccount in internalAccounts)
            {
                if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue)
                {
                    if (internalAccount.Account.AccountDim.SysSieDimNr.Value == (int)accountDim)
                        return internalAccount.Account.Name;
                }
            }
            return "";
        }
    }
}
