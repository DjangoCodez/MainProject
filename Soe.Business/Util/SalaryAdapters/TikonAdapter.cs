// TODO Jukka 9.8.2013
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
    class TikonAdapter : ISalaryAdapter
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
        public const int MAX_EMPLOYEE_NR_LENGTH = 4;
        public const int PAYROLLPPRODUCT_LENGTH = 4;
        public const int DEVIATION_CAUSE_LENGTH = 2;
        public const int TIME_HOURS_LENGTH = 3;
        public const int TIME_MINUTES_LENGTH = 2;

        private String CompanyNr { get; set; }

        #region Constructors

        public TikonAdapter(CompEntities entities, String externalExportID, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<int> employeeIds)
        {
            context = entities;
            // ScheduleTtypeID = "43";
            // AbsenceTtypeID = "45";
            // PayrollTtypeID = "52"; 
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




            // Kaikki valitut työntekijät joilla on transaktioita. 
            foreach (var employeeId in employeeIds)
            {
                List<TransactionItem> transactionItemsForEmployee = new List<TransactionItem>();
                transactionItemsForEmployee = payrollTransactionItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();
                parent.Append(GetTimeTransactionsPresence(transactionItemsForEmployee));
            }

            return parent.ToString();
        }

        #region ScheduleTrasacions

        #endregion

        #region TimeTransactions       



        // Hae paikallaolotiedot
        private string GetTimeTransactionsPresence(List<TransactionItem> transactionItemsForEmployee)
        {
            // List<TransactionItem> transitem = new List<TransactionItem>();
            var sb = new StringBuilder();

            // Tuntipalkkalaisten merkinnät. 
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
            // List<IGrouping<int, TransactionItem>> transactionItemsGroupedByYear = presenceTransactionItemsForEmployee.GroupBy(i => i.Date.Year).ToList();

            //itemsInaYear contains all items for a specific year
            //foreach (IGrouping<int, TransactionItem> itemsToSend in presenceTransactionItemsForEmployee)

            string productNumber = "";
            DateTime date = DateTime.Now;
            bool isRegistrationTypeTime = false;
            bool isRegistrationTypeQuantity = false;
            string employeeNr = "";
            string accountinternal = "";
            decimal amount;
            decimal quantity;
            double stime;
            string pname = "";
            string account = "";
            decimal presencedays = 0;

            amount = 0;

            foreach (var itemsToSend in presenceTransactionItemsForEmployee)
            {
                //   Now group the collection for a year on month
                //   List<IGrouping<int, TransactionItem>> itemsInaYearGroupedByMonth = itemsInAYear.GroupBy(o => o.Date.Month).ToList();
                //   itemsInAMonth contains all items for a specific month
                //   foreach (IGrouping<int, TransactionItem> itemsToSend in itemsInaYearGroupedByMonth)
                //    {

                productNumber = itemsToSend.ProductNr;
                date = itemsToSend.Date;
                isRegistrationTypeTime = itemsToSend.IsRegistrationTime;
                isRegistrationTypeQuantity = itemsToSend.IsRegistrationQuantity;
                employeeNr = itemsToSend.EmployeeNr;
                accountinternal = itemsToSend.Comment;
                amount = itemsToSend.Amount;
                quantity = itemsToSend.Quantity;
                stime = itemsToSend.Time;
                pname = itemsToSend.ProductName;
                account = itemsToSend.Account.Account.AccountNr;
                if (itemsToSend.IsWork())
                {
                    // Tää on työaikaa jos ei ole poissaolo. Lasketaan yhteen. 
                    if (itemsToSend.IsAbsence == false)
                    {
                        presencedays++;
                    }
                }

                sb.Append(CreatePayrollTransaction(employeeNr, productNumber, quantity, date, false, accountinternal, amount, isRegistrationTypeTime, account));
            } // foreach.....

            // Läsnäolopäivät lasketaan precensdays funkkarilla
            if (presencedays > 0)
            {
                sb.Append(CreatePayrollTransaction(employeeNr, "1", 0, date, false, accountinternal, presencedays, false, account));
            }

            return sb.ToString();
        }

        private string CreatePayrollTransaction(string employeeNr, String productNumber, decimal quantity, DateTime date, bool isAbsence, string Costcentre, decimal amount, bool isRegistrationTypeTime, string account)
        {
            //malli
            //
            //
            //Pvm Hlö.Plaji Kpl      Aika  Aika Markat     Kustp   Kiptili
            //PPKK    NNNN  NNNNNN.NN      NN.NN           AAAAAAAA
            //    NNNN    NN         NNN.NN     NNNNNN.NNNN        AAAAAA
            //15011002006000000000.00075.0000.00000000.00006020    5000
            var transaction = new StringBuilder();


            // string Stringamount = System.Convert.ToString(amount, "C2");
            string Stringamount = amount.ToString("000000.00");
            Stringamount = Stringamount.Replace(",", ".");

            string Quantityamount = System.Convert.ToString(quantity);
            Quantityamount = Quantityamount.Replace(",", ".");

            //Transaction Date 4 chars  pos 1-4 
            transaction.Append(date.ToString("ddMM"));
            //Employeenr 4 chars  pos 5-8

            transaction.Append(GetFormatetEmployeeNr(employeeNr));

            // PayrollTypeId NNNN  pos 9-12
            transaction.Append(FillWithZero(4, productNumber));

            // Requirement Group NN  pos 13-14
            transaction.Append("00");

            // Units NNNNNN.NN pos 15-23 
            if (isRegistrationTypeTime == false)
                transaction.Append(Stringamount);
            else
                transaction.Append("000000.00");

            // Time NNN.NN pos 24-29
            if (isRegistrationTypeTime == true)
                transaction.Append((FillWithZero(6, GetTimeFromMinutes(Quantityamount), false)));
            else
                transaction.Append("000.00");

            // EndTime  30-34 
            transaction.Append("00.00");
            // Markat pos 35-45
            transaction.Append("000000.0000");
            // Costcentre  pos 46-53
            transaction.Append(FillWithBlanksEnd(8, (Costcentre), false));
            // Account  54-59
            transaction.Append(FillWithBlanksEnd(6, (account), false));

            //transaction.Append("5000  ");

            // Project 60-67
            transaction.Append("        ");
            // project account 68-73
            transaction.Append("      ");
            // xpense type 74-79
            transaction.Append("      ");
            // 80th char
            transaction.Append(" ");

            //  transaction.Append("stime:"+ System.Convert.ToString(stime));
            // transaction.Append("pname:" + pname);

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

        private static string GetHoursFromTotalMinutes(double totalMinutes, double totalBreakMinutes)
        {
            double breakMinutes = totalBreakMinutes;
            double minutes = totalMinutes;
            minutes -= breakMinutes;
            string stringMinutes;
            stringMinutes = minutes.ToString();
            return GetTimeFromMinutes(stringMinutes);
        }


        private string GetAbsenceTransactionElement(TransactionItem trans, DateTime endDate, decimal quantity, bool wholedayabsence)
        {
            var transaction = new StringBuilder();

            if (trans != null)
            {
                DateTime date = trans.Date;

                //malli
                //
                //
                //Pvm Hlö.Plaji Kpl      Aika  Aika Markat     Kustp   Kiptili
                //PPKK    NNNN  NNNNNN.NN      NN.NN           AAAAAAAA
                //    NNNN    NN         NNN.NN     NNNNNN.NNNN        AAAAAA
                //15011002006000000000.00075.0000.00000000.00006020    5000


                //string Stringamount = System.Convert.ToString(amount);
                //Stringamount = Stringamount.Replace(",", "");

                //Transaction Date 4 chars  pos 1-4 
                transaction.Append(date.ToString("A_ddMM"));
                //Employeenr 4 chars  pos 5-8

                transaction.Append(GetFormatetEmployeeNr(trans.EmployeeNr));

                //transaction.Append(GetPayrollProduct(productnumber, false));

                // PayrollTypeId NNNN  pos 9-12
                transaction.Append(FillWithZero(4, AbsenceTtypeID));  // Poissaolo

                // Requirement Group NN  pos 13-14
                transaction.Append("00");

                // Units NNNNNN.NN pos 15-23 
                if (quantity > 0)
                {
                    transaction.Append((FillWithZero(9, (GetTimeFromMinutes(quantity)), false)));
                }
                else transaction.Append("000000.00");

                // Time NNN.NN pos 24-29
                if (quantity > 0)
                {
                    transaction.Append((FillWithZero(5, (GetTimeFromMinutes(quantity)), false)));

                }
                else transaction.Append("000.00");

                // EndTime  30-34 
                transaction.Append("00.00");
                // Markat pos 35-45
                transaction.Append("000000.0000");
                // Costcentre  pos 46-53
                transaction.Append(FillWithBlanksEnd(8, (" "), false));
                // Account  54-59
                transaction.Append("0000  ");
                // Project 60-67
                transaction.Append("        ");
                // project account 68-73
                transaction.Append("      ");
                // xpense type 74-79
                transaction.Append("      ");
                // 80th char
                transaction.Append(" ");
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


        private static string GetTimeFromMinutes(string amount)
        {
            decimal value;

            amount = amount.Replace(".", ",");
            decimal.TryParse(amount, out value);
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);


            string newamount = value.ToString();

            newamount = newamount.Replace(",", ".");
            //return hours.minutes i.e 8h3m -> 8.03
            return (newamount);
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

        /*
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
                */
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