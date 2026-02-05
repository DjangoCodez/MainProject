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
    class NetvisorAdapter : BaseSalaryAdapter, ISalaryAdapter
    {

        private readonly CompEntities context;
        // Ttype = Transactionstype
        private String ScheduleTtypeID { get; set; }
        private String AbsenceTtypeID { get; set; }
        private String PayrollTtypeID { get; set; }
        private DateTime periodEndDate;
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

        public NetvisorAdapter(CompEntities entities, String externalExportID, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<int> employeeIds, DateTime periodEndDate)
        {
            context = entities;
            // ScheduleTtypeID = "43";
            // AbsenceTtypeID = "45";
            // PayrollTtypeID = "52"; 
            CompanyNr = externalExportID;
            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
            this.employeeIds = employeeIds;
            this.periodEndDate = periodEndDate;
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
                parent.Append(GetTimeTransactionsPresence(transactionItemsForEmployee.Where(w => !w.IsAbsence).ToList()));
                parent.Append(GetEmployeeAbsenceTransactions(transactionItemsForEmployee.Where(w => w.IsAbsence).ToList(), employeeId));
            }

            return parent.ToString();
        }

        #region TimeTransactions      

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
                    sb.Append(GetTimeTransactionsAbsence(sequence.Item1, sequence.Item3, sequence.Item2, sequence.Item4));
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
            amount = 0;

            foreach (var group in presenceTransactionItemsForEmployee.GroupBy(g => $"{g.EmployeeNr}#{g.ProductNr}#{GetAccountNr(TermGroup_SieAccountDim.CostCentre, g.AccountInternals)}"))
            {
                var itemsToSend = group.First();

                productNumber = itemsToSend.ProductNr;
                date = itemsToSend.Date;
                isRegistrationTypeTime = itemsToSend.IsRegistrationTime;
                isRegistrationTypeQuantity = itemsToSend.IsRegistrationQuantity;
                employeeNr = itemsToSend.EmployeeNr;
                accountinternal = itemsToSend.Comment;
                amount = group.Sum(a => a.Amount);
                quantity = group.Sum(a => a.Quantity);
                stime = itemsToSend.Time;
                pname = itemsToSend.ProductName;
                sb.Append(CreatePayrollTransaction(employeeNr, productNumber, quantity, date, false, accountinternal, amount, isRegistrationTypeTime, false));
            }

            return sb.ToString();
        }

        private string CreatePayrollTransaction(string employeeNr, String productNumber, decimal quantity, DateTime date, bool isAbsence, string Costcentre, decimal amount, bool isRegistrationTypeTime, bool isWholeDayAbsence)
        {
            var transaction = new StringBuilder();

            //Empty (6)
            transaction.Append(FillStrWithBlanks(6));

            //Employeenr 4 chars  pos 7-13
            transaction.Append(GetFormatetEmployeeNr(employeeNr));

            //Empty pos 20  pos 14-35
            transaction.Append(FillStrWithBlanks(20));

            // PayrollTypeId NNNN  pos 36-43
            transaction.Append(FillStrWithBlanks(4, productNumber));

            //Empty pos 20  pos 35
            transaction.Append(FillStrWithBlanks(2));

            //Units
            // 8 char pos 36-43
            // -hours or quantity depending of the payroll product type
            // -units are presented with two decimals and without decimal
            // separator(8 hours = 800, 7 hours 30 mins = 750, 1 day = 100)
            if (isRegistrationTypeTime)
            {
                if (isWholeDayAbsence)
                    transaction.Append(FillStrWithBlanks(8, "100"));
                else
                    transaction.Append(FillStrWithBlanks(8, GetTimeFromMinutes(quantity)));
            }
            else
            {
                transaction.Append(FillStrWithBlanks(8, decimal.Round(quantity, 2).ToString().Replace(".", "").Replace(",", "")));
            }

            //Empty 1 char  pos 44
            transaction.Append(FillStrWithBlanks(1));

            // Costcentre 10 char pos 45-54
            transaction.Append(FillStrWithBlanks(10, Costcentre));

            //Empty 30 char  pos 55-84
            transaction.Append(FillStrWithBlanks(30));

            // Period end date in format 'yyyyMMdd ' (eg. 20220430)
            transaction.Append(FillStrWithBlanks(8, periodEndDate.ToString("yyyyMMdd")));
            transaction.Append(Environment.NewLine);
            return transaction.ToString();
        }

        private string GetTimeTransactionsAbsence(TransactionItem trans, DateTime endDate, decimal quantity, bool wholedayabsence)
        {
            var transaction = new StringBuilder();

            if (trans != null)
                return CreatePayrollTransaction(trans.EmployeeNr, trans.ProductNr, trans.Quantity, trans.Date, true, GetAccountNr(TermGroup_SieAccountDim.CostCentre, trans.AccountInternals), trans.Amount, true, wholedayabsence);

            return string.Empty;
        }

        #endregion

        #region Help Methods

        private String GetFormatetEmployeeNr(String employeeNr)
        {
            int nrOfZeros = MAX_EMPLOYEE_NR_LENGTH - employeeNr.Length;
            String formatedEmployeeNr = FillStrWithBlanks(nrOfZeros);

            formatedEmployeeNr += employeeNr;
            return formatedEmployeeNr;
        }

        #endregion
    }
}