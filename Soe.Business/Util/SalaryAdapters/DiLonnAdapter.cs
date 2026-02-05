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
     * Implementation for DI Lönn for the Norwegian market
     *      
     * */
    public class DiLonnAdapter : ISalaryAdapter
    {
        private List<TransactionItem> payrollTransactionItems;

        public DiLonnAdapter(List<TransactionItem> payrollTransactions)
        {
            this.payrollTransactionItems = payrollTransactions;
        }

        #region Salary

        /// <summary>
        /// Transforming to Hogia format
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
            List<Tuple<DateTime, DateTime, TransactionItem>> coherentAbsenceTransactions = GetCoherentTransactions(absenceTransactions);

            foreach (var transaction in coherentAbsenceTransactions)
            {
                sb.Append(GetSalaryTransactionElement(transaction.Item3, transaction.Item1, transaction.Item2));
            }

            return sb.ToString();
        }

        private string GetEmployeePresenceBasedTransactions(List<TransactionItem> employeeTransactions)
        {
            var sb = new StringBuilder();

            List<TransactionItem> presenceTransactions = employeeTransactions.Where(t => !t.IsAbsence).ToList();

            foreach (var transaction in presenceTransactions)
            {
                sb.Append(GetSalaryTransactionElement(transaction, transaction.Date, transaction.Date));
            }

            return sb.ToString();
        }

        private List<Tuple<DateTime, DateTime, TransactionItem>> GetCoherentTransactions(List<TransactionItem> transactionItems)
        {
            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => o.ProductNr).ToList();
            List<Tuple<DateTime, DateTime, TransactionItem>> coherentTransactions = new List<Tuple<DateTime, DateTime, TransactionItem>>();

            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                List<TransactionItem> transactionsOrderedByDate = transactionItemsForProductNumber.OrderBy(o => o.Date).ToList();

                DateTime? stopDate = null;
                TransactionItem firstTransInSequence = null;
                int dayIntervall = 0;
                int counter = 0;

                decimal accQuantity = 0;
                double accTime = 0;
                string accComment = String.Empty;
                decimal accAmount = 0;

                List<List<TransactionItem>> transactionsInSequence = new List<List<TransactionItem>>();

                foreach (var currentItem in transactionsOrderedByDate)
                {
                    counter++;

                    if (counter == 1)
                    {
                        firstTransInSequence = currentItem;
                    }

                    //look if item is in the current datesequnce
                    if (IsTransactionCoherent(firstTransInSequence, currentItem, dayIntervall))
                    {
                        stopDate = currentItem.Date;

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
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,

                        };

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, stopDate.Value, coherentTrnsaction));

                        //currentItem is the first item in the new sequence, it can also be the last one!
                        firstTransInSequence = currentItem;
                        stopDate = currentItem.Date;

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
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,

                        };

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, stopDate.Value, coherentTrnsaction));
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

        private string GetSalaryTransactionElement(TransactionItem transaction, DateTime start, DateTime stop)
        {
            var sb = new StringBuilder();

            if (transaction != null)
            {
                //Identifier
                sb.Append(FillWithBlanks(12, transaction.EmployeeNr));

                //employeeNr
                sb.Append(FillWithBlanks(6, transaction.ProductNr));

                sb.Append(FillWithBlanks(6, GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals)));

                ////Productnumber
                //sb.Append(FillWithBlanks(5, transaction.ProductNr));

                ////Costplace
                //sb.Append(FillWithBlanks(9, GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals)));

                //Project - Not present in filexport from WebbTid therefore outcommented here.
                sb.Append(FillWithBlanks(12, GetAccountNr(TermGroup_SieAccountDim.Project, transaction.AccountInternals)));

                sb.Append(FillWithBlanks(6, GetDiLonnDate(start)));

                sb.Append(FillWithBlanks(17, GetQuantityInHours(transaction.Quantity.ToString())));

                //Note
                //sb.Append(FillWithZero(6, transaction.Comment, true));

                ////Quantity
                //if (transaction.Quantity == 0)
                //    sb.Append(FillWithZero(10, ""));
                //else
                //{
                //    sb.Append(" ");
                //    sb.Append(FillWithZero(9, GetHogiaStringValue(transaction.Quantity.ToString())));
                //}

                ////A-Price
                //sb.Append(FillWithZero(10, (GetPricePerQuantityUnit(transaction))));

                ////Price
                //sb.Append(FillWithZero(17, GetHogiaStringValue(transaction.Amount.ToString())));

                ////From date
                //sb.Append(GetDiLonnDate(start));

                ////To date
                //sb.Append(GetDiLonnDate(stop));

                sb.Append(Environment.NewLine);


            }

            return sb.ToString();
        }

        #endregion

        #region Help methods

        private String GetDiLonnDate(DateTime date)
        {
            return date.ToString("ddMMyy");
        }

        private String GetAccountName(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
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
        private String FillWithBlanks(int targetSize, string originValue, bool truncate = false)
        {
            if (targetSize > originValue.Length)
            {
                string zeros = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    zeros += " ";
                }
                return (originValue + zeros);
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
                return FillWithBlanks(10, "");
            else
            {
                decimal pricePerQuantityUnit;
                pricePerQuantityUnit = transaction.Amount / transaction.Quantity;
                if (pricePerQuantityUnit == 0)
                    return FillWithBlanks(10, "");
                else
                    return FillWithBlanks(9, GetHogiaStringValue(pricePerQuantityUnit.ToString()));
            }
        }

        private string GetQuantityInHours(string quantityInMinutes)
        {
            decimal value = 0;
            quantityInMinutes = quantityInMinutes.Replace(".", ",");
            decimal.TryParse(quantityInMinutes, out value);
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);
            string result = value.ToString();
            result = result.Replace(",", ".");
            return result;
        }

        private String GetHogiaStringValue(String amount)
        {
            decimal value;
            amount = amount.Replace(".", ",");
            decimal.TryParse(amount, out value);
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);
            value *= 100;
            String returnAmount = ((int)value).ToString();

            return returnAmount;
        }

        #endregion
    }
}
        #endregion