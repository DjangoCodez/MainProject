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
    class PersonecAdapter : ISalaryAdapter
    {
        private readonly CompEntities context;
        // Ttype = Transactionstype
        private String ScheduleTtypeID { get; set; }
        private String AbsenceTtypeID { get; set; }
        private String PayrollTtypeID { get; set; }
        private List<TransactionItem> payrollTransactionItems;
        private List<ScheduleItem> scheduleItems;
        private List<int> employeeIds;
        public const int SCHEDULE_T_FIRST_ROW_END_DAYNR = 16;
        public const int MAX_EMPLOYEE_NR_LENGTH = 10;
        public const int PAYROLLPPRODUCT_LENGTH = 3;
        public const int DEVIATION_CAUSE_LENGTH = 2;
        public const int TIME_HOURS_LENGTH = 3;
        public const int TIME_MINUTES_LENGTH = 2;

        private String CompanyNr { get; set; }

        #region Constructors

        public PersonecAdapter(CompEntities entities, String externalExportID, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<int> employeeIds)
        {
            context = entities;
            ScheduleTtypeID = "43";
            AbsenceTtypeID = "45";
            PayrollTtypeID = "52";
            CompanyNr = externalExportID;
            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
            this.employeeIds = employeeIds;
        }

        #endregion

        #region Public methods

        public byte[] TransformSalary(XDocument baseXml)
        {
            string doc = string.Empty;
            if (context != null)
                doc = CreateDocument();

            return Encoding.UTF8.GetBytes(doc);
        }

        #endregion

        private string CreateDocument()
        {
            var parent = new StringBuilder();

            foreach (var employeeId in employeeIds)
            {


                List<TransactionItem> transactionItemsForEmployee = new List<TransactionItem>();
                transactionItemsForEmployee = payrollTransactionItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();

                parent.Append(GetTimeTransactionsAbsence(transactionItemsForEmployee, employeeId));

            }

            foreach (var employeeId in employeeIds)
            {
                List<ScheduleItem> scheduleItemsForEmployee = new List<ScheduleItem>();
                scheduleItemsForEmployee = scheduleItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();


                parent.Append(GetSchedule(scheduleItemsForEmployee));

            }

            foreach (var employeeId in employeeIds)
            {
                List<TransactionItem> transactionItemsForEmployee = new List<TransactionItem>();
                transactionItemsForEmployee = payrollTransactionItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();


                parent.Append(GetTimeTransactionsPresence(transactionItemsForEmployee));
            }

            return parent.ToString();
        }

        #region ScheduleTrasacions

        private string GetSchedule(List<ScheduleItem> scheduleItems)
        {
            var sb = new StringBuilder();

            foreach (var scheduleItem in scheduleItems)
            {
                //Get schedule transactions
                string totalHours = GetHoursFromTotalMinutes(scheduleItem.TotalMinutes, scheduleItem.TotalBreakMinutes);

                // Do nothing if zero day
                if (totalHours != "0000")
                {
                    //Period not implemented (same as WebbTid)
                    sb.Append(CompanyNr);
                    sb.Append(ScheduleTtypeID);
                    sb.Append("0000");
                    sb.Append(GetFormatetEmployeeNr(scheduleItem.EmployeeNr));

                    //Same start date as stop date since we created one transaction per day.
                    sb.Append(GetDate(scheduleItem.Date, "yyyyMMdd"));
                    sb.Append("0000");
                    sb.Append(GetDate(scheduleItem.Date, "yyyyMMdd"));
                    sb.Append(FillWithBlanksEnd(120, (totalHours), false));
                    sb.Append(DateTime.Today.ToString("yyyy-MM-dd"));
                    sb.Append(Environment.NewLine);
                }
            }
            return sb.ToString();
        }

        #endregion

        #region TimeTransactions

        private string GetTimeTransactionsAbsence(List<TransactionItem> transactionItemsForEmployee, int employeeId)
        {
            // List<TransactionItem> transitem = new List<TransactionItem>();
            var sb = new StringBuilder();

            sb.Append(GetEmployeeAbsenceTransactions(transactionItemsForEmployee.Where(t => t.IsAbsence).ToList(), employeeId));

            return sb.ToString();
        }

        private string GetTimeTransactionsPresence(List<TransactionItem> transactionItemsForEmployee)
        {
            // List<TransactionItem> transitem = new List<TransactionItem>();
            var sb = new StringBuilder();

            sb.Append(GetEmployeePayrollTransactions(transactionItemsForEmployee.Where(t => !t.IsAbsence).ToList()));

            return sb.ToString();
        }

        //OBS! This method assumes that transactions are merged on date
        private string GetEmployeeAbsenceTransactions(List<TransactionItem> absenceTransactionItemsForEmployee, int employeeId)
        {
            var sb = new StringBuilder();

            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = absenceTransactionItemsForEmployee.GroupBy(o => o.ProductNr).ToList();

            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                List<TransactionItem> transactionsOrderedByDate = transactionItemsForProductNumber.OrderBy(o => o.Date).ToList();

                List<ScheduleItem> scheduleItemsAbsence;

                scheduleItemsAbsence = scheduleItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();

                List<TransactionItem> fractionAbsences = transactionsOrderedByDate.Where(t => !t.IncludedInWholeDayAbsence(scheduleItemsAbsence)).ToList();
                List<TransactionItem> wholedayAbsences = transactionsOrderedByDate.Where(t => t.IncludedInWholeDayAbsence(scheduleItemsAbsence)).ToList();

                //Transaction,quantity,stopdate,wholeday
                List<Tuple<TransactionItem, decimal, DateTime, bool>> sequences = new List<Tuple<TransactionItem, decimal, DateTime, bool>>();


                #region Fractions

                foreach (var item in fractionAbsences)
                {
                    sequences.Add(Tuple.Create(item, item.Quantity, item.Date, false));
                }

                #endregion

                #region Whole days

                DateTime? endDate = null;
                TransactionItem firstTransInAbsenceIntervall = null;
                int dayIntervall = 0;
                decimal quantityInAbsenceIntervall = 0;
                int counter = 0;

                foreach (var currentItem in wholedayAbsences)
                {
                    counter++;

                    if (counter == 1)
                    {
                        firstTransInAbsenceIntervall = currentItem;
                    }

                    //look if item is in the current datesequnce                    
                    if (SalaryExportUtil.IsTransactionCoherent(firstTransInAbsenceIntervall, currentItem, dayIntervall))
                    {
                        endDate = currentItem.Date;
                        quantityInAbsenceIntervall += currentItem.Quantity;
                        dayIntervall++;

                    }
                    else
                    {   //end of seqence is reached
                        //sb.Append(GetAbsenceTransactionElement(firstTransInAbsenceIntervall, endDate.Value, quantityInAbsenceIntervall));
                        sequences.Add(Tuple.Create(firstTransInAbsenceIntervall, quantityInAbsenceIntervall, endDate.Value, true));

                        //reset
                        endDate = null;
                        quantityInAbsenceIntervall = 0;
                        dayIntervall = 0;

                        //currentItem is the first item in the new sequence, it can also be the last one!
                        firstTransInAbsenceIntervall = currentItem;
                        endDate = currentItem.Date;
                        quantityInAbsenceIntervall += currentItem.Quantity;
                        dayIntervall = 1;
                    }

                    if (counter == wholedayAbsences.Count)
                    {
                        //sb.Append(GetAbsenceTransactionElement(firstTransInAbsenceIntervall, endDate.Value, quantityInAbsenceIntervall));
                        sequences.Add(Tuple.Create(firstTransInAbsenceIntervall, quantityInAbsenceIntervall, endDate.Value, true));
                    }
                }

                #endregion

                #region Generate Absence Transactions

                //order by startdate
                sequences = sequences.OrderBy(s => s.Item1.Date).ToList();

                foreach (var sequence in sequences)
                {
                    sb.Append(GetAbsenceTransactionElement(sequence.Item1, sequence.Item3, sequence.Item2, sequence.Item4));
                }

                #endregion

            }
            return sb.ToString();
        }

        private string GetEmployeePayrollTransactions(List<TransactionItem> presenceTransactionItemsForEmployee)
        {
            var sb = new StringBuilder();

            //Group the collection on year
            List<IGrouping<int, TransactionItem>> transactionItemsGroupedByYear = presenceTransactionItemsForEmployee.GroupBy(i => i.Date.Year).ToList();

            //itemsInaYear contains all items for a specific year
            foreach (IGrouping<int, TransactionItem> itemsInAYear in transactionItemsGroupedByYear)
            {
                //Now group the collection for a year on month
                List<IGrouping<int, TransactionItem>> itemsInaYearGroupedByMonth = itemsInAYear.GroupBy(o => o.Date.Month).ToList();

                //itemsInAMonth contains all items for a specific month
                foreach (IGrouping<int, TransactionItem> itemsInAMonth in itemsInaYearGroupedByMonth)
                {
                    //Now group the collection for a month on productnumber
                    List<IGrouping<string, TransactionItem>> itemsInAMonthGroupProductNr = itemsInAMonth.GroupBy(o => o.ProductNr).ToList();

                    //itemsGroupOnMonthAndProductNr contains all items for a specific month and for a specific productnumber
                    foreach (IGrouping<string, TransactionItem> itemsGroupOnMonthAndProductNr in itemsInAMonthGroupProductNr)
                    {

                        // now group on CostPlace

                        List<TransactionItem> transactionsWithCostPlaceInComment = new List<TransactionItem>();

                        foreach (var item in itemsGroupOnMonthAndProductNr)
                        {
                            transactionsWithCostPlaceInComment.Add(new TransactionItem
                            {
                                Date = item.Date,
                                ProductNr = item.ProductNr,
                                Quantity = item.Quantity,
                                Time = item.Time,
                                IsRegistrationQuantity = item.IsRegistrationQuantity,
                                IsRegistrationTime = item.IsRegistrationTime,
                                EmployeeNr = item.EmployeeNr,
                                Amount = item.Amount,
                                Comment = GetAccountNr(TermGroup_SieAccountDim.CostCentre, item.AccountInternals)
                            });

                        }



                        List<IGrouping<string, TransactionItem>> itemsInAMonthGroupProductNrandCostPlace = transactionsWithCostPlaceInComment.GroupBy(o => o.Comment).ToList();

                        foreach (IGrouping<string, TransactionItem> itemsWithProductNrAndCostPlace in itemsInAMonthGroupProductNrandCostPlace)
                        {

                            decimal quantity = 0;
                            String productNumber = itemsWithProductNrAndCostPlace.FirstOrDefault().ProductNr;
                            DateTime date = itemsWithProductNrAndCostPlace.FirstOrDefault().Date;
                            bool isRegistrationTypeTime = itemsWithProductNrAndCostPlace.FirstOrDefault().IsRegistrationTime;
                            bool isRegistrationTypeQuantity = itemsWithProductNrAndCostPlace.FirstOrDefault().IsRegistrationQuantity;
                            string employeeNr = itemsWithProductNrAndCostPlace.FirstOrDefault().EmployeeNr;
                            String accountinternal = itemsWithProductNrAndCostPlace.FirstOrDefault().Comment;
                            decimal amount = 0;


                            foreach (var item in itemsWithProductNrAndCostPlace)
                            {
                                amount += item.Amount;
                                quantity += item.Quantity;
                            }

                            sb.Append(CreatePayrollTransaction(employeeNr, productNumber, quantity, date, false, accountinternal, amount, isRegistrationTypeTime));
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private string CreatePayrollTransaction(string employeeNr, String productnumber, decimal quantity, DateTime date, bool isAbsence, string Costcentre, decimal amount, bool isRegistrationTypeTime)
        {
            //exempel
            //00205200000000003302410          8501                           0000200       000000000  
            var transaction = new StringBuilder();

            string Stringamount = System.Convert.ToString(amount);
            Stringamount = Stringamount.Replace(",", "");

            transaction.Append(CompanyNr);
            //Type 52
            transaction.Append(PayrollTtypeID);
            //Empty space
            transaction.Append("0000");
            //Employeenr
            transaction.Append(GetFormatetEmployeeNr(employeeNr));
            //Product
            transaction.Append(GetPayrollProduct(productnumber, false));
            // Costcentre
            transaction.Append(FillWithBlanksEnd(41, (FillWithBlanksBeginning(14, Costcentre)), false));
            //transaction.Append(FillWithZero(6, Costcentre));
            //transaction.Append("       ");
            //29
            if (isRegistrationTypeTime == true)
            {
                transaction.Append(FillWithBlanksEnd(10, (FillWithZero(7, (GetTimeFromMinutes(quantity)), false)), false));
            }
            if (isRegistrationTypeTime == false)
            {
                transaction.Append(FillWithBlanksEnd(10, (FillWithZero(7, (GetQuantityToString(quantity)), false)), false));
            }

            //transaction.Append(FillWithZero(6, Stringamount));
            //Pos 23-32, Not in use
            transaction.Append("    ");
            //Quantity Pos 44-48
            transaction.Append(FillWithBlanksEnd(82, (FillWithZero(9, Stringamount)), false));
            //transaction.Append("                                                                        ");
            transaction.Append(DateTime.Today.ToString("yyyy-MM-dd"));
            transaction.Append(Environment.NewLine);

            return transaction.ToString();
        }

        //private string GetPayrollTransactionElement(XElement employee, XElement trans, TransactionItem transitem)
        //{
        //    if (trans != null)
        //    {
        //        DateTime date = GetDate(trans.Attribute("date").Value);
        //        decimal quantity = ConvertStringQuantityToDecimnal(trans.Attribute("quantity").Value);
        //        String productnr = trans.Attribute("productnumber").Value;
        //        String accountinternal = GetAccountNr(TermGroup_SieAccountDim.CostCentre, transitem.AccountInternals);
        //        String amount = trans.Attribute("amount").Value;

        //        return CreatePayrollTransaction(employee, productnr, quantity, date, false, accountinternal, amount);
        //    }

        //    return "";
        //}

        private string GetAbsenceTransactionElement(TransactionItem trans, DateTime endDate, decimal quantity, bool wholedayabsence)
        {
            var transaction = new StringBuilder();

            if (trans != null)
            {
                DateTime date = trans.Date;

                //Pos: 1-4
                transaction.Append(CompanyNr);
                //Pos: 5-6
                transaction.Append(AbsenceTtypeID);
                //Pos: 7-8
                //transaction.Append(GetFinalNumberOfYear(date.Year));
                transaction.Append("");
                //Pos 9-10
                //transaction.Append(GetPaymentPeriod(date.Month));
                transaction.Append("0000");
                //Pos 11-20
                transaction.Append(GetFormatetEmployeeNr(trans.EmployeeNr));
                //Pos 21-28
                transaction.Append(GetDate(date, "yyyyMMdd"));
                //Pos 29-30
                transaction.Append(GetPayrollProduct(trans.ProductNr, true));
                //Pos 31-35'
                transaction.Append(FillWithBlanksEnd(8, wholedayabsence ? "" : FillWithZero(5, GetTimeFromMinutes(quantity)), false));
                // transaction.Append(GetTimeFromMinutes(quantity));
                //Pos 36-38 Not in use
                // transaction.Append("   ");
                //Pos 39-46
                transaction.Append(GetDate(endDate, "yyyyMMdd"));
                //Pos 35-37 Alternative to "28-32", see specifikation
                transaction.Append(FillWithBlanksEnd(114, (GetAbsencePercent(quantity, quantity)), false));
                transaction.Append(DateTime.Today.ToString("yyyy-MM-dd"));

                transaction.Append(Environment.NewLine);
            }

            return transaction.ToString();
        }

        #endregion

        #region Help Methods

        private DateTime GetDate(String date)
        {
            DateTime dateTime;
            DateTime.TryParse(date, out dateTime);
            return dateTime;
        }

        private String GetDate(DateTime date, String format)
        {
            return date.ToString(format);
        }


        //Returns the last digit in a year. i.e: 2009 => 9 
        private String GetFinalNumberOfYear(int yearIn)
        {
            String year = yearIn.ToString();
            if (year.Length > 1)
                return year.Substring(year.Length - 1, 1);

            return year;
        }

        //returns a string representation of monthIn, that is 2 character long, 4 => 04
        private String GetPaymentPeriod(int monthIn)
        {
            String month = monthIn.ToString();
            if (month.Length > 1)
                return month;

            return "0" + month;
        }

        private String GetFormatetEmployeeNr(String employeeNr)
        {
            int nrOfZeros = MAX_EMPLOYEE_NR_LENGTH - employeeNr.Length;
            String formatedEmployeeNr = FillStrWithZeros(nrOfZeros);

            formatedEmployeeNr += employeeNr;
            return formatedEmployeeNr;
        }

        private String GetFormatDate(String employeeNr)
        {
            int nrOfZeros = MAX_EMPLOYEE_NR_LENGTH - employeeNr.Length;
            String formatedEmployeeNr = FillStrWithZeros(nrOfZeros);

            formatedEmployeeNr += employeeNr;
            return formatedEmployeeNr;
        }

        private String GetScheduleTFirstRowBeginDate(int year, int month)
        {
            DateTime date = new DateTime(year, month, 1);

            return GetDate(date, "yyMMdd");
        }

        //private String GetScheduleTFirstRowEndDate(int year, int month)
        //{
        //    DateTime date = new DateTime(year, month, SCHEDULE_T_FIRST_ROW_END_DAYNR);

        //    return GetDate(date, "yyMMdd");

        //}

        private static string GetHoursFromTotalMinutes(double totalMinutes, double totalBreakMinutes)
        {
            double breakMinutes = totalBreakMinutes;
            double minutes = totalMinutes;
            minutes -= breakMinutes;
            string stringMinutes;
            stringMinutes = minutes.ToString();
            return GetTimeFromMinutes(stringMinutes);
        }

        private String FillWithBlanksEnd(int targetSize, string originValue, bool truncate = false)
        {
            if (targetSize > originValue.Length)
            {
                string blanks = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    blanks += " ";
                }
                return (originValue + blanks);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }

        private String FillWithBlanksBeginning(int targetSize, string originValue, bool truncate = false)
        {
            if (targetSize > originValue.Length)
            {
                string blanks = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    blanks += " ";
                }
                return (blanks + originValue);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
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


        private static string GetTimeFromMinutes(string amount)
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
            return (hours.ToString().Length == 1 ? "0" + hours.ToString() : hours.ToString()) + "" + (minutes.ToString().Length == 1 ? "0" + minutes.ToString() : minutes.ToString());
        }

        private static string GetClock(double minutes)
        {
            var clock = new StringBuilder();
            int hours = (int)minutes / 60;
            if (hours < 10)
                clock.Append("0");
            clock.Append(hours);
            clock.Append("");
            int min = (int)minutes - (hours * 60);
            if (min < 10)
                clock.Append("0");
            clock.Append(min);
            return clock.ToString();
        }

        private String GetPayrollProduct(String payrollProductNumber, bool absenseTransaction)
        {
            int nrOfZeros = 0;
            String formatedPayrollProduct = "";

            if (absenseTransaction)
            {
                nrOfZeros = DEVIATION_CAUSE_LENGTH - payrollProductNumber.Length;
            }
            else
            {
                nrOfZeros = PAYROLLPPRODUCT_LENGTH - payrollProductNumber.Length;
            }

            formatedPayrollProduct = FillStrWithZeros(nrOfZeros);

            formatedPayrollProduct += payrollProductNumber;

            return formatedPayrollProduct;
        }

        private String FillStrWithZeros(int nrOfZeros)
        {
            String str = "";

            for (int i = 0; i < nrOfZeros; i++)
            {
                str += "0";
            }
            return str;
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

        private string GetAbsencePercent(decimal AbsenceAmount, decimal ScheduleAmount)
        {
            decimal absencevalue = AbsenceAmount;
            decimal schedulevalue = ScheduleAmount;
            decimal absencePercent = 0;
            int absencePercent100 = 0;
            if (absencevalue != 0)
                absencePercent = absencevalue / schedulevalue;
            absencePercent = Math.Round(absencePercent, 2, MidpointRounding.ToEven);
            absencePercent100 = Convert.ToInt16(Math.Round(absencePercent, 2, MidpointRounding.ToEven));

            String absencePercentString = Convert.ToString(absencePercent100);
            absencePercentString = absencePercentString.Replace(".", "");
            absencePercentString = absencePercentString.Replace(",", "");
            absencePercentString = FillStrWithZeros(2);
            if (absencePercent100 == 1)
                absencePercentString = Convert.ToString(' ');

            return absencePercentString;
        }

        private string GetTimeFromMinutes(decimal amount)
        {
            decimal value = amount;
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
            return hours.ToString() + "" + (minutes.ToString().Length == 1 ? "0" + minutes.ToString() : minutes.ToString());
        }

        private string GetQuantityToString(decimal amount)
        {
            string value = System.Convert.ToString(amount);
            value = value.Replace(",", "");
            value = value.Replace(".", "");
            return value;
        }

        private decimal ConvertStringQuantityToDecimnal(String amount)
        {
            decimal value;
            amount = amount.Replace(".", ",");
            decimal.TryParse(amount, out value);
            value = Math.Round(value, 2, MidpointRounding.ToEven);

            return value;
        }



        //private List<TransactionItem> Merge(List<TransactionItem> items)
        //{
        //    List<TransactionItem> result = new List<TransactionItem>();

        //    while (items.Count() > 0)
        //    {
        //        TransactionItem firstItem = items.FirstOrDefault();
        //        List<TransactionItem> matchingTransactions = new List<TransactionItem>();

        //        //find similar trasactions to merge
        //        List<TransactionItem> tmp = (from i in items
        //                                     where i.Date == firstItem.Date &&
        //                                     i.ProductNr == firstItem.ProductNr
        //                                     select i).ToList();

        //        //Accountinternals must be the same and in the same order
        //        foreach (var itemInTmp in tmp)
        //        {
        //            bool sameAccountCount = itemInTmp.AccountInternals.Count == firstItem.AccountInternals.Count;
        //            if (sameAccountCount)
        //            {
        //                bool allAccountsMatch = true; //accountCount can be zero

        //                for (int i = 0; i < firstItem.AccountInternals.Count; i++)
        //                {
        //                    if (firstItem.AccountInternals[i].AccountId != itemInTmp.AccountInternals[i].AccountId)
        //                        allAccountsMatch = false;
        //                }

        //                if (allAccountsMatch)
        //                    matchingTransactions.Add(itemInTmp);
        //            }
        //        }

        //        TransactionItem newItem = new TransactionItem()
        //        {

        //            Date = firstItem.Date,
        //            ProductNr = firstItem.ProductNr
        //        };

        //        foreach (var tmpItem in matchingTransactions)
        //        {
        //            newItem.Quantity += tmpItem.Quantity;
        //        }
        //        matchingTransactions.ForEach(i => items.Remove(i));
        //        result.Add(newItem);
        //    }

        //    return result;
        //}
        #endregion
    }
}
