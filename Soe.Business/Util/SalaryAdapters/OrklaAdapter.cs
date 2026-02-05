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
    class OrklaAdapter : ISalaryAdapter
    {

        private readonly CompEntities context;
        // Ttype = Transactionstype        
        readonly private List<TransactionItem> payrollTransactionItems;        
        readonly List<Employee> employees;
        public const int SCHEDULE_T_FIRST_ROW_END_DAYNR = 16;
        public const int MAX_EMPLOYEE_NR_LENGTH = 4;
        public const int PAYROLLPPRODUCT_LENGTH = 4;
        public const int DEVIATION_CAUSE_LENGTH = 2;
        public const int TIME_HOURS_LENGTH = 3;
        public const int TIME_MINUTES_LENGTH = 2;

        private String CompanyNr { get; set; }

        #region Constructors

        public OrklaAdapter(CompEntities entities, String externalExportID, List<TransactionItem> payrollTransactions, List<Employee> employees)
        {
            context = entities;
            CompanyNr = externalExportID;
            this.payrollTransactionItems = payrollTransactions;            
            this.employees = employees;
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
            foreach (var employee in employees.OrderBy(e => e.LastName))
            {                 
                List<TransactionItem> transactionItemsForEmployee = payrollTransactionItems.Where(s => s.EmployeeId == employee.EmployeeId.ToString()).ToList();
                parent.Append(GetTimeTransactionsPresence(transactionItemsForEmployee));
                parent.Append(GetEmployeeAbsenceTransactions(transactionItemsForEmployee));
            }

            return parent.ToString();
        }

        #region TimeTransactions

        private string GetTimeTransactionsPresence(List<TransactionItem> transactionItemsForEmployee)
        {            
            var sb = new StringBuilder();

            // Tuntipalkkalaisten merkinnät. 
            sb.Append(GetEmployeePayrollTransactions(transactionItemsForEmployee.Where(t => !t.IsAbsence).ToList()));

            return sb.ToString();
        }

        private string GetEmployeeAbsenceTransactions(List<TransactionItem> transactionItemsForEmployee)
        {         
            var sb = new StringBuilder();

            // Tuntipalkkalaisten merkinnät. 
            sb.Append(GetEmployeePayrollTransactions(transactionItemsForEmployee.Where(t => t.IsAbsence).ToList()));

            return sb.ToString();
        }

        //OBS! This method assumes that transactions are merged on date

        private string GetEmployeePayrollTransactions(List<TransactionItem> transactionItemsForEmployee)
        {
            var sb = new StringBuilder();

            foreach (var items in transactionItemsForEmployee.GroupBy(t => t.ProductNr + t.Date.ToString() + GetAccountNr(TermGroup_SieAccountDim.CostCentre, t.AccountInternals)))
            {
                string productNumber = "";                 
                bool isRegistrationTypeTime = false;                
                string employeeNr = "";
                decimal amount;
                decimal quantity;               
                string externalCode = string.Empty;

                var firstItem = items.FirstOrDefault();

                productNumber = firstItem.ProductNr;
                DateTime date = firstItem.Date;
                isRegistrationTypeTime = firstItem.IsRegistrationTime;                
                employeeNr = firstItem.EmployeeNr;
                amount = items.Sum(i => i.Amount);
                quantity = items.Sum(i => i.Quantity);                
                externalCode = firstItem.ExternalCode;

                String costPlace = GetAccountNr(TermGroup_SieAccountDim.CostCentre, firstItem.AccountInternals);

                sb.Append(CreatePayrollTransaction(externalCode, employeeNr, productNumber, quantity, date, false, costPlace, amount, isRegistrationTypeTime));
            }

            return sb.ToString();
        }

        private string CreatePayrollTransaction(string externalCode, string employeeNr, String productNumber, decimal quantity, DateTime date, bool isAbsence, string costPlace, decimal amount, bool isRegistrationTypeTime)
        {

            var transaction = new StringBuilder();

            // MARY = 750 | HENO = 40560 | PVM8 = 2015 - 09 - 16 | PALA = 10100 | MAARA = 8

            //MARY – Det är Lönegruppen för Timavlönade på Haraldsbyfabriken
            //HENO – Anställningsnummer
            //PVM8 – Datum
            //PALA – Löneart
            //MAARA – Arbetad tid
            //KUSTP - Kostnadställe

            transaction.Append("" + externalCode);
            transaction.Append("|");
            transaction.Append("" + employeeNr);
            transaction.Append("|");
            transaction.Append("" + date.ToShortDateString());
            transaction.Append("|");
            transaction.Append("" + productNumber);
            transaction.Append("|");
            transaction.Append("" + (isRegistrationTypeTime ? GetTimeFromMinutes(quantity) : quantity.ToString()));

            if (!string.IsNullOrEmpty(costPlace))
            {
                transaction.Append("|");
                transaction.Append("" + costPlace);
            }

            transaction.Append(Environment.NewLine);
            return transaction.ToString();
        }

        #endregion

        #region Help Methods
       
        private string GetTimeFromMinutes(decimal amount)
        {
            decimal value = amount;
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);

            int hours = (int)value;
            int minutes = 0;
            minutes = Convert.ToInt32((amount - (hours * 60)));
            minutes = Math.Abs(minutes);
            minutes = (int)Decimal.Divide(minutes * 100, 60);

            //0 - 0,12”timmar” => 0 timmar
            //0,13 - 0,37 => 0,25 timmar
            //0,38 - 0,62 => 0,5 timmar
            //0,63 - 0,87 => 0,75 timmar
            //0,87 - 0,99 =>1,0 timmar

            if (minutes <= 12)
                minutes = 0;

            if (minutes > 12 && minutes <= 37)
                minutes = 25;

            if (minutes > 37 && minutes <= 62)
                minutes = 50;

            if (minutes > 62 && minutes <= 87)
                minutes = 75;

            if (minutes > 87)
            {
                hours++;
                minutes = 0;
            }

            //return hours.minutes i.e 8h3m -> 8.03
            return hours.ToString() + "." + (minutes.ToString().Length == 1 ? "0" + minutes.ToString() : minutes.ToString());
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