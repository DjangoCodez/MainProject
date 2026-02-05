using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    class TikonCSVAdapter : ISalaryAdapter
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
        private List<PayrollProductDTO> _payrollProductsWithQuantity;

        private String CompanyNr { get; set; }

        #region Constructors

        public TikonCSVAdapter(CompEntities entities, List<PayrollProductDTO> payrollProducts)
        {
            _payrollProductsWithQuantity = payrollProducts.Where(w => w.ResultType == (int)TermGroup_PayrollResultType.Quantity).ToList();
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

            //First add headers to the file
            parent.Append("PVM;TYNRO;PLNRO;KPL/MISTÄ;AIKA/MIHIN;MK/EUR;OSASTO;KPTILI;PROJ;");
            parent.Append(Environment.NewLine);

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
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => o.ProductNr + (_payrollProductsWithQuantity.Select(s => s.Number).Contains(o.ProductNr) ? o.Date.ToString() : "")).ToList();

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

                    endDate = currentItem.Date;
                    quantityInAbsenceIntervall += currentItem.Quantity;
                    dayIntervall++;

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
            List<IGrouping<string, TransactionItem>> itemsInAGroup = transactionItems.GroupBy(o => o.Comment+ o.ProductNr + (_payrollProductsWithQuantity.Select(s => s.Number).Contains(o.ProductNr) ? o.Date.ToString() : "")).ToList();   // Ryhmitelty kustannuspaikoittain            

            foreach (IGrouping<string, TransactionItem> items in itemsInAGroup)
            {

                decimal quantity = 0;
                string productNumber = ""; // itemsGroupOnProductNr.Key;

                DateTime date = items.FirstOrDefault().Date;
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
        {
            //Example    
            //PVM;TYNRO;PLNRO;KPL/MISTÄ;AIKA/MIHIN;MK/EUR;OSASTO;KPTILI;PROJ   
            //25.5.2018;101;70;;67,93;;10;5000            

            var transaction = new StringBuilder();

            //PVM (transaction date, not mandatory)
            transaction.Append(date.ToString("dd.MM.yyyy") + ";");

            //TYNRO (employee number, mandatory)
            transaction.Append(employee.Attribute("nr").Value + ";");

            //PLNRO (payroll product number, mandatory
            transaction.Append(productnumber + ";");

            //KPL/MISTÄ (quantity or time from, not used)
            if (!IsQuantityNotTime(productnumber))
                transaction.Append(";");
            else
                transaction.Append(GetQuantityString(GetQuantity(productnumber, quantity)) + ";");

            //AIKA/MIHIN (hours or time to)
            if (!IsQuantityNotTime(productnumber))
                transaction.Append(GetTimeFromMinutes(quantity) + ";");
            else
                transaction.Append(";");

            //MK/EUR (euros, not used)
            transaction.Append(";");

            //OSASTO (costplace)
            transaction.Append(costplace + ";");

            //KPTILI (account number, not used)
            transaction.Append(";");

            //PROJ (project number, not used)
            transaction.Append(";");

            transaction.Append(Environment.NewLine);

            return transaction.ToString();
        }

        private string GetAbsenceTransactionElement(XElement employee, TransactionItem trans, DateTime endDate, decimal quantity, string costplace)
        {
            var transaction = new StringBuilder();

            if (trans != null)
            {
                //PVM (transaction date, not mandatory)
                transaction.Append(trans.Date.ToString("dd.MM.yyyy") + ";");

                //TYNRO (employee number, mandatory)
                transaction.Append(employee.Attribute("nr").Value + ";");

                //PLNRO (payroll product number, mandatory
                transaction.Append(trans.ProductNr + ";");

                //KPL/MISTÄ (quantity or time from, not used)
                transaction.Append(";");

                //AIKA/MIHIN (hours or time to)
                transaction.Append(GetTimeFromMinutes(quantity) + ";");

                //MK/EUR (euros, not used)
                transaction.Append(";");

                //OSASTO (costplace)
                transaction.Append(costplace + ";");

                //KPTILI (account number, not used)
                transaction.Append(";");

                //PROJ (project number, not used)
                transaction.Append(";");

                transaction.Append(Environment.NewLine);

            }

            return transaction.ToString();
        }

        #endregion

        #region Help Methods

        private bool IsQuantityNotTime(string productNr)
        {
            return _payrollProductsWithQuantity.Any(a => a.Number.Equals(productNr));
        }

        private decimal GetQuantity(string productNr, decimal quantity)
        {
            if (IsQuantityNotTime(productNr))
                return new decimal(1);
            else
                return quantity;
        }

        private DateTime GetDate(String date)
        {
            DateTime dateTime;
            DateTime.TryParse(date, out dateTime);
            return dateTime;
        }

        private string GetQuantityString(decimal quantity)
        {
            return Convert.ToInt32(quantity).ToString();
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


            String sHours = hours.ToString();
            String sMinutes = decminutes.ToString();
            if (sMinutes.Length > 2)
                sMinutes = sMinutes.Substring(0, 2);

            //sMinutes = FillStrWithZeros(TIME_MINUTES_LENGTH - sMinutes.Length) + sMinutes;

            String formatedTime = sHours + "," + sMinutes; //i.e 15 timmar 45 minuter => 15,75
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


        #endregion
    }
}
