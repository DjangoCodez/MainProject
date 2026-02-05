using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    class SaumaAdapter : ISalaryAdapter
    {
        private readonly CompEntities context;
        private String ScheduleTtypeID { get; set; }
        private String AbsenceTtypeID { get; set; }
        private String PayrollTtypeID { get; set; }
        public const int SCHEDULE_T_FIRST_ROW_END_DAYNR = 16;
        public const int MAX_EMPLOYEE_NR_LENGTH = 6;
        public const int PAYROLLPPRODUCT_LENGTH = 3;
        public const int DEVIATION_CAUSE_LENGTH = 3;
        public const int TIME_HOURS_LENGTH = 4;
        public const int TIME_MINUTES_LENGTH = 2;

        private String CompanyNr { get; set; }

        #region Constructors

        public SaumaAdapter(CompEntities entities)
        {
            context = entities;

        }



        #endregion

        #region Public methods

        public byte[] TransformSalary(XDocument baseXml)
        {
            string doc = string.Empty;
            if (context != null)
                doc = CreateDocument(baseXml);

            return Encoding.UTF8.GetBytes(doc);
        }

        #endregion

        private string GenerateStringValue(string stringdefault, string stringnew)
        {
            return stringdefault.Substring(0, stringdefault.Length - stringnew.Length) + stringnew;
        }

        private string CreateDocument(XDocument baseXml)
        {
            var parent = new StringBuilder();
            IEnumerable<XElement> employees = baseXml.Descendants("employees");

            foreach (XElement employee in employees.Elements("employee"))
            {
                parent.Append(GetTimeTransactions(employee));
            }

            return parent.ToString();
        }


        #region TimeTransactions

        private string GetTimeTransactions(XElement employee)
        {
            var sb = new StringBuilder();

            sb.Append(GetEmployeeAbsenceTransactions(employee));
            sb.Append(GetEmployeePayrollTransactions(employee, false));

            return sb.ToString();
        }

        private string GetEmployeeAbsenceTransactions(XElement employee)
        {
            var sb = new StringBuilder();
            List<TransactionItem> transactionItems = new List<TransactionItem>();
            IEnumerable<XElement> transactions = employee.Descendants("absences");

            // Employee emp = new Employee();


            foreach (XElement payrolltrans in transactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in payrolltrans.Elements("transaction"))
                {
                    DateTime date = GetDate(trans.Attribute("date").Value);
                    String productNr = trans.Attribute("productnumber").Value;
                    decimal quantity = ConvertStringQuantityToDecimnal(trans.Attribute("quantity").Value);
                    String SieDimNro = "";
                    String costPlace = "";

                    foreach (XElement internalaccount in trans.Elements("internalaccounts"))
                    {
                        foreach (XElement account in internalaccount.Elements("account"))
                        {
                            SieDimNro = account.Attribute("siedimnr").Value;
                            if (SieDimNro == "1")
                                costPlace = account.Attribute("nr").Value;
                        }
                    }

                    transactionItems.Add(new TransactionItem
                    {
                        Date = date,
                        ProductNr = productNr,
                        Quantity = quantity,
                        Comment = costPlace,  // Let's put the costplace into the Comment field 

                    });
                }
            }

            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => o.ProductNr).ToList();

            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                List<TransactionItem> transactionsOrderedByDate = transactionItemsForProductNumber.OrderBy(o => o.Date).ToList();

                DateTime? endDate = null;
                TransactionItem firstTransInAbsenceIntervall = null;
                int dayIntervall = 0;
                decimal quantityInAbsenceIntervall = 0;
                int counter = 0;
                string costplace = "";



                foreach (var currentItem in transactionsOrderedByDate)
                {
                    counter++;
                    costplace = currentItem.Comment;  // Costplace transferred via Comment

                    if (counter == 1)
                    {
                        firstTransInAbsenceIntervall = currentItem;
                    }

                    {
                        endDate = currentItem.Date;
                        quantityInAbsenceIntervall += currentItem.Quantity;
                        dayIntervall++;

                    }

                    if (counter == transactionsOrderedByDate.Count)
                    {
                        sb.Append(GetAbsenceTransactionElement(employee, firstTransInAbsenceIntervall, endDate.Value, quantityInAbsenceIntervall, costplace));
                    }
                }
            }
            return sb.ToString();
        }

        private string GetEmployeePayrollTransactions(XElement employee, bool absencetransaction)
        {
            var sb = new StringBuilder();

            List<TransactionItem> transactionItems = new List<TransactionItem>();
            IEnumerable<XElement> transactions = employee.Descendants("transactions");


            foreach (XElement payrolltrans in transactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in payrolltrans.Elements("transaction"))
                {
                    DateTime date = GetDate(trans.Attribute("date").Value);
                    String productNr = trans.Attribute("productnumber").Value;
                    decimal quantity = ConvertStringQuantityToDecimnal(trans.Attribute("quantity").Value);
                    double totalMin = 0d;
                    Double.TryParse(trans.Attribute("totalminutes").Value, out totalMin);
                    String SieDimNro = "";
                    String costPlace = "";
                    //IEnumerable<XElement> internalaccounts = transactions.Descendants("internalaccounts"); // Haetaan alempaa XML:ää
                    foreach (XElement internalaccount in trans.Elements("internalaccounts"))
                    {
                        foreach (XElement account in internalaccount.Elements("account"))
                        {
                            SieDimNro = account.Attribute("siedimnr").Value;
                            if (SieDimNro == "1")
                                costPlace = account.Attribute("nr").Value;
                        }
                    }

                    // Let's put the costplace into comment field.                     
                    transactionItems.Add(new TransactionItem
                    {
                        Date = date,
                        ProductNr = productNr,
                        Quantity = quantity,
                        Time = totalMin,
                        Comment = costPlace,

                    });
                }
            }

            //Now group the collection on Comment & productnumber to get grouped by costplace and productnumbers, FIX 6.5. 2014 / Jukka
            List<IGrouping<string, TransactionItem>> itemsInAGroup = transactionItems.GroupBy(x => x.Comment + x.ProductNr).ToList();   // Ryhmitelty kustannuspaikoittain



            foreach (IGrouping<string, TransactionItem> items in itemsInAGroup)
            {
                decimal quantity = 0;
                string productNumber = ""; // itemsGroupOnProductNr.Key;

                DateTime date = DateTime.Today; /// itemsInAMonthGroupProductNr.FirstOrDefault().Date;
                string costplace = "";

                foreach (TransactionItem A in items)
                {
                    costplace = A.Comment;
                    productNumber = A.ProductNr;
                    quantity += A.Quantity;
                }
                sb.Append(CreatePayrollTransaction(employee, productNumber, quantity, date, costplace));
            }

            return sb.ToString();
        }

        private string CreatePayrollTransaction(XElement employee, String productnumber, decimal quantity, DateTime date, string costplace)
        // Example    0000999900000000000130000 001455 00000000 0000000000       0       5000   
        //            1234567890123456789012345678901234567890123456789012345678901234567890
        {

            var transaction = new StringBuilder();
            //Pos 1-2 - Betalingsgrupnumret (Not used) / Maksuryhmä
            transaction.Append("00");

            //Pos 3-8 (6) - Personnumret (EmployeeNumber)
            transaction.Append(GetFormatetEmployeeNr(employee.Attribute("nr").Value));

            //Pos 9-13 (5)   Lönetiden  (VVKKn) (Not used)
            transaction.Append("00000");

            //Pos 14-19 (6)   Inspelningsdatum   (VVKKPP)  (Not used)
            transaction.Append("000000");

            //Pos 20-22 (3)   Löneartiklanumret (Payroll product number)
            transaction.Append(GetPayrollProduct(productnumber, false));

            //Pos 23-25 (3)   Beräkningrekelnumret (Not used)
            transaction.Append("000");

            //Pos 26-26(1)   Enheterstecken(+ eller -) (- if negativ else + )
            if (quantity < 0)
                transaction.Append("-");
            else
                transaction.Append("+");  // + is required! / Jukka 25.5.2012

            //Pos 27-32 (6) ( -Enheter (Quantity) in hours,decminutes. Two decimals)
            transaction.Append(GetTimeFromMinutes(quantity));

            //Pos 33 A hinnan etumerkki
            transaction.Append(" ");  // 1 empty character

            //Pos 34-41  (8)  A hinta
            transaction.Append("        ");  // 8 empty characters

            // 42 (1)  Yht etum
            transaction.Append(" ");  // 1 empty character

            // 43 - 51 (9) Yhteensä
            transaction.Append("         ");  // 9 empty characters

            // 52 - 59 (8) Project
            transaction.Append("        ");  // 8 empty characters

            // 60 - 67 (8) Cost place
            transaction.Append(FillWithBlanksBeginning(8, costplace));

            //                  
            transaction.Append(Environment.NewLine);

            return transaction.ToString();
        }



        private string GetAbsenceTransactionElement(XElement employee, TransactionItem trans, DateTime endDate, decimal quantity, string costplace)
        {
            var transaction = new StringBuilder();

            if (trans != null)
            {

                //Pos 1-2 - Betalingsgrupnumret (Not used)
                transaction.Append("00");

                //Pos 3-9 (6) - Personnumret (EmployeeNumber)
                transaction.Append(GetFormatetEmployeeNr(employee.Attribute("nr").Value));

                //Pos 10-14(5)   Lönetiden  (VVKKn) (Not used)
                transaction.Append("00000");

                //Pos 14-19(6)   Inspelningsdatum   (VVKKPP)  (Not used)
                transaction.Append("000000");

                //Pos 20-22(3)   Löneartiklanumret (Payroll product number)
                transaction.Append(GetPayrollProduct(trans.ProductNr, false));

                //Pos 23-24(3)   Beräkningrekelnumret (Not used)
                transaction.Append("000");

                //Pos 25-25(1)   Enheterstecken(+ eller -) (- if negativ else + )
                if
                    (quantity < 0)
                    transaction.Append("-");
                else
                    transaction.Append("+"); // + sign is required / Jukka 25.5.2012
                //Pos 26-32 (6) ( -Enheter(Q in hours,minutes. Two decimals)
                transaction.Append(GetTimeFromMinutes(quantity));

                //Pos 33 A hinnan etumerkki
                transaction.Append(" ");  // 1 empty character

                //Pos 34-41  (8)  A hinta
                transaction.Append("        ");  // 8 empty characters

                // 42 (1)  Yht etum
                transaction.Append(" ");  // 1 empty character

                // 43 - 51 (9) Yhteensä
                transaction.Append("         ");  // 9 empty characters

                // 52 - 59 (8) Project
                transaction.Append("        ");  // 8 empty characters

                // 60 - 67 (8) Cost place
                transaction.Append(FillWithBlanksBeginning(8, costplace));
                //              
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

        /*   private String GetAccountNr(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
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
           }  */

        private static string GetAccountInfo(XElement account, XElement internalAccounts)
        {
            if (account == null)
                return string.Empty;
            var sb = new StringBuilder();
            sb.Append(account.Attribute("nr").Value);
            return sb.ToString();
        }

        private static string GetInternelAccountInfo(XElement internalAccounts)
        {
            var sb = new StringBuilder();
            foreach (XElement internalAccount in internalAccounts.Elements())
            {
                sb.Append(";");
                sb.Append(internalAccount.Attribute("nr").Value);
            }
            return sb.ToString();
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



        private String GetPayrollProduct(String payrollProductNumber, bool absenseTransaction)
        {
            int nrOfZeros = 0;
            String formatedPayrollProduct = "";


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

        private string GetTimeFromMinutes(decimal amount)
        {
            decimal value = amount;
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);

            int hours = (int)value;
            int decminutes = 0;
            value -= hours;
            value *= 100;
            decminutes = (int)value;


            String sHours = FillStrWithZeros(TIME_HOURS_LENGTH - hours.ToString().Length) + hours.ToString();
            String sMinutes = decminutes.ToString();
            if (sMinutes.Length > 2)
                sMinutes = sMinutes.Substring(0, 2);

            sMinutes = FillStrWithZeros(TIME_MINUTES_LENGTH - sMinutes.Length) + sMinutes;

            String formatedTime = sHours + sMinutes; //i.e 15 timmar 45 minuter => 01575
            return formatedTime;
        }

        private decimal ConvertStringQuantityToDecimnal(String amount)
        {
            decimal value;
            amount = amount.Replace(".", ",");
            decimal.TryParse(amount, out value);
            value = Math.Round(value, 2, MidpointRounding.ToEven);

            return value;
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


        #endregion
    }
}
