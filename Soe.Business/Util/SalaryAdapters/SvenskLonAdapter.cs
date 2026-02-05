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
    public class SvenskLonAdapter : ISalaryAdapter
    {
        private readonly CompEntities context;
        // Ttype = Transactionstype
        private String ScheduleTtypeID { get; set; }
        private String AbsenceTtypeID { get; set; }
        private String PayrollTtypeID { get; set; }
        public const int SCHEDULE_T_FIRST_ROW_END_DAYNR = 16;
        public const int MAX_EMPLOYEE_NR_LENGTH = 10;
        public const int PAYROLLPPRODUCT_LENGTH = 3;
        public const int DEVIATION_CAUSE_LENGTH = 2;
        public const int TIME_HOURS_LENGTH = 3;
        public const int TIME_MINUTES_LENGTH = 2;
        private List<ScheduleItem> scheduleItems;
        private String CompanyNr { get; set; }
        private Boolean displayCostPlace = false;
        private DateTime _startDate { get; set; }
        private DateTime _stopDate { get; set; }

        #region Constructors

        public SvenskLonAdapter(CompEntities entities, String externalExportID, DateTime startDate, DateTime stopDate, List<ScheduleItem> scheduleItems)
        {
            this._startDate = startDate;
            this._stopDate = stopDate;
            context = entities;
            ScheduleTtypeID = "43";
            AbsenceTtypeID = "45";
            PayrollTtypeID = "52";
            this.scheduleItems = scheduleItems;
            CompanyNr = externalExportID;
            displayCostPlace = false;
            if (externalExportID.Any() && externalExportID.Right(1) == "#")
            {
                CompanyNr = externalExportID.Left(4);
                displayCostPlace = true;

            }
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

        private string CreateDocument(XDocument baseXml)
        {
            var parent = new StringBuilder();
            IEnumerable<XElement> employees = baseXml.Descendants("employees");

            foreach (XElement employee in employees.Elements("employee"))
            {
                parent.Append(GetScheduleTransactions(employee));
                parent.Append(GetTimeTransactions(employee));
            }

            return parent.ToString();
        }

        #region ScheduleTrasacions

        private string GetScheduleTransactions(XElement employee)
        {
            var scheduleDays = new StringBuilder();
            List<ScheduleItem> scheduleItems = null;

            IEnumerable<XElement> schedules = employee.Descendants("schedules");

            XElement schedule1 = schedules.Elements("schedule").FirstOrDefault();
            if (schedule1 == null)
                return scheduleDays.ToString();

            scheduleItems = new List<ScheduleItem>();

            foreach (XElement schedule in schedules.Elements("schedule"))
            {

                foreach (XElement day in schedule.Elements("day"))
                {
                    //string id = day.Attribute("timescheduletemplateperiodid").Value;
                    //string dayNumber = day.Attribute("daynumber").Value;
                    DateTime date = GetDate(day.Attribute("date").Value);
                    //string start = day.Attribute("starttime").Value;
                    //string stop = day.Attribute("stoptime").Value;
                    double totalMin = 0d;
                    Double.TryParse(day.Attribute("totaltimemin").Value, out totalMin);
                    double totalBreakMin = 0d;
                    Double.TryParse(day.Attribute("totalbreakmin").Value, out totalBreakMin);

                    scheduleItems.Add(new ScheduleItem
                    {
                        Date = date,
                        TotalMinutes = totalMin,
                        TotalBreakMinutes = totalBreakMin,

                    });
                }
            }

            #region Fill days outside of employment

            DateTime currentDate = _startDate;

            while (currentDate <= _stopDate)
            {
                if (!scheduleItems.Any(s => s.Date.Date == currentDate.Date))
                {
                    scheduleItems.Add(new ScheduleItem
                    {
                        Date = currentDate,
                        TotalMinutes = 0,
                        TotalBreakMinutes = 0,
                    });
                }

                currentDate = currentDate.AddDays(1);
            }

            scheduleItems = scheduleItems.OrderBy(o => o.Date).ToList();

            #endregion

            //Group the collection on year
            List<IGrouping<int, ScheduleItem>> scheduleItemsGroupedByYear = scheduleItems.GroupBy(i => i.Date.Year).ToList();

            //itemsInaYear contains all items for a specific year
            foreach (IGrouping<int, ScheduleItem> itemsInAYear in scheduleItemsGroupedByYear)
            {

                //Now group the collection for a year on month
                List<IGrouping<int, ScheduleItem>> itemsInaYearGroupedByMonth = itemsInAYear.GroupBy(o => o.Date.Month).ToList();

                //itemsInAMonth contains all items for a specific month
                foreach (IGrouping<int, ScheduleItem> itemsInAMonth in itemsInaYearGroupedByMonth)
                {
                    var firstSixteenDaysInMonth = new StringBuilder();
                    var remainingDaysInMonth = new StringBuilder();

                    #region SceduleTransactions For a Month

                    //create a datetime that represent the sixteenth day for a specific year and month
                    DateTime sixtennthDay = new DateTime(itemsInAYear.Key, itemsInAMonth.Key, SCHEDULE_T_FIRST_ROW_END_DAYNR);
                    String firstRowBeginDate = GetScheduleTFirstRowBeginDate(itemsInAYear.Key, itemsInAMonth.Key);
                    String firstRowEndDate = GetScheduleTFirstRowEndDate(itemsInAYear.Key, itemsInAMonth.Key);
                    String secondRowBeginDate = GetScheduleTSecondRowBeginDate(itemsInAYear.Key, itemsInAMonth.Key);
                    String secondRowEndDate = GetScheduleTSecondRowEndDate(itemsInAYear.Key, itemsInAMonth.Key);

                    #region Setup first row

                    //Pos 1-4
                    firstSixteenDaysInMonth.Append(CompanyNr);
                    //Pos 5-6
                    firstSixteenDaysInMonth.Append(ScheduleTtypeID);
                    //Pos 7
                    firstSixteenDaysInMonth.Append(GetFinalNumberOfYear(itemsInAYear.Key));
                    //Pos 8-9
                    firstSixteenDaysInMonth.Append(GetPaymentPeriod(itemsInAMonth.Key));
                    //Pos 10-19
                    firstSixteenDaysInMonth.Append(GetFormatetEmployeeNr(employee.Attribute("nr").Value));
                    //Pos 20-25
                    firstSixteenDaysInMonth.Append(firstRowBeginDate);
                    //Pos 26-27, not in use 
                    firstSixteenDaysInMonth.Append("  ");
                    //Pos 28-33
                    firstSixteenDaysInMonth.Append(firstRowEndDate);
                    //Pos 34-73 --> Scheduledtime, region "Add scheduletime to the rows"

                    #endregion

                    #region Setup second row

                    //Pos 1-4                        
                    remainingDaysInMonth.Append(CompanyNr);
                    //Pos 5-6
                    remainingDaysInMonth.Append(ScheduleTtypeID);
                    //Pos 7
                    remainingDaysInMonth.Append(GetFinalNumberOfYear(itemsInAYear.Key));
                    //Pos 8-9
                    remainingDaysInMonth.Append(GetPaymentPeriod(itemsInAMonth.Key));
                    //Pos 10-19
                    remainingDaysInMonth.Append(GetFormatetEmployeeNr(employee.Attribute("nr").Value));
                    //Pos 20-25
                    remainingDaysInMonth.Append(secondRowBeginDate);
                    //Pos 26-27, not in use 
                    remainingDaysInMonth.Append("  ");
                    //Pos 28-33
                    remainingDaysInMonth.Append(secondRowEndDate);
                    //Pos 34-73 --> Scheduledtime, region "Add scheduletime to the rows"

                    #endregion

                    #region Add scheduletime to the rows

                    foreach (ScheduleItem itemInAMonth in itemsInAMonth)
                    {
                        if (itemInAMonth.Date <= sixtennthDay)
                        {
                            //itemInAMonth is in beetween yyMM01 and yyMM16
                            firstSixteenDaysInMonth.Append(GetDayCode((itemInAMonth.TotalMinutes - itemInAMonth.TotalBreakMinutes)));
                        }
                        else
                        {
                            remainingDaysInMonth.Append(GetDayCode((itemInAMonth.TotalMinutes - itemInAMonth.TotalBreakMinutes)));
                        }
                    }

                    #endregion

                    scheduleDays.Append(firstSixteenDaysInMonth);
                    scheduleDays.Append(Environment.NewLine);
                    scheduleDays.Append(remainingDaysInMonth);
                    scheduleDays.Append(Environment.NewLine);

                    #endregion
                }

            }
            //}

            return scheduleDays.ToString();
        }

        #endregion

        #region TimeTransactions

        private string GetTimeTransactions(XElement employee)
        {
            var sb = new StringBuilder();

            sb.Append(GetEmployeeAbsenceTransactions(employee));
            sb.Append(GetEmployeePayrollTransactions(employee));

            return sb.ToString();
        }

        private string GetEmployeeAbsenceTransactions(XElement employee)
        {
            var sb = new StringBuilder();
            List<TransactionItem> transactionItems = new List<TransactionItem>();
            IEnumerable<XElement> transactions = employee.Descendants("absences");
            IEnumerable<XElement> qdaystransactions = employee.Descendants("transactions");

            foreach (XElement payrolltrans in transactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in payrolltrans.Elements("transaction"))
                {
                    DateTime date = GetDate(trans.Attribute("date").Value);
                    string productNr = trans.Attribute("productnumber").Value;
                    decimal quantity = ConvertStringQuantityToDecimnal(trans.Attribute("quantity").Value);
                    bool isAbsence = GetBool(trans.Attribute("isabsence").Value);

                    transactionItems.Add(new TransactionItem
                    {
                        Date = date,
                        ProductNr = productNr,
                        Quantity = quantity,
                        EmployeeId = employee.Attribute("id").Value,
                        IsAbsence = isAbsence,
                        IsRegistrationTime = true
                    }) ; 
                }
            }

            transactionItems = Merge(transactionItems);

            foreach (XElement payrolltrans in qdaystransactions.Elements("payrolltransactions"))
            {
                foreach (XElement trans in payrolltrans.Elements("transaction"))
                {
                    DateTime date = GetDate(trans.Attribute("date").Value);
                    String productNr = trans.Attribute("productnumber").Value;

                    if (productNr == "91")
                    {
                        decimal quantity = ConvertStringQuantityToDecimnal(trans.Attribute("quantity").Value);

                        transactionItems.Add(new TransactionItem
                        {
                            Date = date,
                            ProductNr = productNr,
                            Quantity = quantity,
                        });
                    }
                }
            }

            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => o.ProductNr).ToList();


            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                if (transactionItemsForProductNumber.Key == "05" && transactionItemsForProductNumber.Any())
                {
                    sb.Append(GetEmployeeVacationTransactions(transactionItemsForProductNumber.ToList(), employee, Convert.ToInt32(employee.Attribute("id").Value)));
                }
                else
                {
                    List<TransactionItem> transactionsOrderedByDate = transactionItemsForProductNumber.OrderBy(o => o.Date).ToList();

                    DateTime? endDate = null;
                    TransactionItem firstTransInAbsenceIntervall = null;
                    decimal quantityInAbsenceIntervall = 0;
                    int counter = 0;

                    foreach (var currentItem in transactionsOrderedByDate)
                    {
                        counter++;

                        //if (counter == 1)
                        //{
                        //    firstTransInAbsenceIntervall = currentItem;
                        //}

                        ////look if item is in the current datesequnce
                        //if ((firstTransInAbsenceIntervall.Date.AddDays(dayIntervall) == currentItem.Date))
                        //{
                        //    endDate = currentItem.Date;
                        //    quantityInAbsenceIntervall += currentItem.Quantity;
                        //    dayIntervall = 0;

                        //}
                        //else
                        //{   //end of seqence is reached



                        if (currentItem.Quantity != 0)
                        {
                            //sb.Append(GetAbsenceTransactionElement(employee, firstTransInAbsenceIntervall, endDate.Value, quantityInAbsenceIntervall));
                            sb.Append(GetAbsenceTransactionElement(employee, currentItem, currentItem.Date, currentItem.Quantity));
                            //reset
                            endDate = null;
                            quantityInAbsenceIntervall = 0;


                            //currentItem is the first item in the new sequence, it can also be the last one!
                            firstTransInAbsenceIntervall = currentItem;
                            endDate = currentItem.Date;
                            quantityInAbsenceIntervall += currentItem.Quantity;

                            //}
                        }

                        //if (counter == transactionsOrderedByDate.Count())
                        //{
                        //    sb.Append(GetAbsenceTransactionElement(employee, firstTransInAbsenceIntervall, endDate.Value, quantityInAbsenceIntervall));
                        //}
                    }
                }
            }
            return sb.ToString();
        }

        private List<TransactionItem> Merge(List<TransactionItem> items)
        {
            List<TransactionItem> result = new List<TransactionItem>();

            while (items.Count > 0)
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
                    ProductNr = firstItem.ProductNr,
                    EmployeeId = firstItem.EmployeeId,
                    IsAbsence = firstItem.IsAbsence,
                    IsRegistrationTime = firstItem.IsRegistrationTime
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

        private string GetEmployeeVacationTransactions(List<TransactionItem> absenceTransactionItemsForEmployee, XElement employee, int employeeId)
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
                    sb.Append(GetAbsenceTransactionElement(employee, sequence.Item1, sequence.Item3, sequence.Item2));
                }

                #endregion

            }
            return sb.ToString();
        }


        private string GetEmployeePayrollTransactions(XElement employee)
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
                    if (productNr != "91")
                    {
                        decimal quantity = ConvertStringQuantityToDecimnal(trans.Attribute("quantity").Value);
                        double totalMin = 0d;
                        Double.TryParse(trans.Attribute("totalminutes").Value, out totalMin);
                        bool isRegistrationQuantity = Convert.ToBoolean(trans.Attribute("isRegistrationTypeQuantity").Value);

                        String costPlace = "";

                        if (trans.Element("internalaccounts") != null)
                        {
                            costPlace = GetAccountInfo(trans.Element("internalaccounts"));
                        }

                        transactionItems.Add(new TransactionItem
                        {
                            Date = date,
                            ProductNr = productNr,
                            Quantity = quantity,
                            Time = totalMin,
                            IsRegistrationQuantity = isRegistrationQuantity,
                            Comment = costPlace
                        });
                    }
                }
            }

            //Group the collection on year
            List<IGrouping<int, TransactionItem>> transactionItemsGroupedByYear = transactionItems.GroupBy(i => i.Date.Year).ToList();

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
                        if (displayCostPlace == true)
                        {

                            String costPlace = GetAccountNr(TermGroup_SieAccountDim.CostCentre, itemsGroupOnMonthAndProductNr.FirstOrDefault().AccountInternals);

                            // now group on CostPlace
                            List<IGrouping<string, TransactionItem>> itemsInAMonthGroupProductNrandCostPlace = itemsGroupOnMonthAndProductNr.GroupBy(o => o.Comment).ToList();

                            foreach (IGrouping<string, TransactionItem> itemsWithProductNrAndCostPlace in itemsInAMonthGroupProductNrandCostPlace)
                            {
                                decimal quantity = 0;
                                String productNumber = itemsWithProductNrAndCostPlace.First().ProductNr;
                                DateTime date = itemsWithProductNrAndCostPlace.FirstOrDefault().Date;
                                bool isRegistrationQuantity = itemsWithProductNrAndCostPlace.FirstOrDefault().IsRegistrationQuantity;
                                String costPlaceString = itemsWithProductNrAndCostPlace.Key;

                                foreach (var item in itemsWithProductNrAndCostPlace)
                                {
                                    quantity += item.Quantity;
                                }

                                sb.Append(CreatePayrollTransaction(employee, productNumber, quantity, date, false, isRegistrationQuantity, costPlaceString));
                            }
                        }
                        else
                        {

                            decimal quantity = 0;
                            String productNumber = itemsGroupOnMonthAndProductNr.Key;
                            DateTime date = itemsGroupOnMonthAndProductNr.FirstOrDefault().Date;
                            bool isRegistrationQuantity = itemsGroupOnMonthAndProductNr.FirstOrDefault().IsRegistrationQuantity;
                            String costPlaceString = "";
                            //String accountinternal = GetAccountNr(TermGroup_SieAccountDim.CostCentre, itemsGroupOnMonthAndProductNr.FirstOrDefault().AccountInternals);

                            foreach (var item in itemsGroupOnMonthAndProductNr)
                            {
                                quantity += item.Quantity;
                            }

                            sb.Append(CreatePayrollTransaction(employee, productNumber, quantity, date, false, isRegistrationQuantity, costPlaceString));

                        }
                    }

                }
            }
            return sb.ToString();
        }

        private string CreatePayrollTransaction(XElement employee, String productnumber, decimal quantity, DateTime date, bool isAbsence, bool isRegistrationQuantity, string costPlaceNr)
        {
            // 2413520110000000172010          ****2      00808  
            var transaction = new StringBuilder();
            //Pos 1-4
            transaction.Append(CompanyNr);
            //Pos 5-6
            transaction.Append(PayrollTtypeID);
            //Pos 7
            transaction.Append(GetFinalNumberOfYear(date.Year));
            //Pos 8-9
            transaction.Append(GetPaymentPeriod(date.Month));
            //Pos 10-19
            transaction.Append(GetFormatetEmployeeNr(employee.Attribute("nr").Value));
            //Pos 20-22
            transaction.Append(GetPayrollProduct(productnumber, isAbsence));
            //Pos 23-32, Not in use
            transaction.Append("          ");
            //Pos 33-42, Not in use
            if (costPlaceNr == "")
            {
                transaction.Append("     ");
            }
            else
            {
                transaction.Append(FillWithStars(5, costPlaceNr, true));
            }
            //transaction.Append("          ");
            //Pos 43, Not in use
            transaction.Append("      ");
            //Pos 44-48
            if (isRegistrationQuantity)
            {
                transaction.Append(FillWithZero(5, (GetQuantityfromQuantity(quantity))));
            }
            else
            {
                transaction.Append(FillWithZero(5, (GetTimeFromMinutes(quantity))));
            }
            transaction.Append(Environment.NewLine);

            return transaction.ToString();
        }

        private string GetPayrollTransactionElement(XElement employee, XElement trans)
        {
            if (trans != null)
            {
                DateTime date = GetDate(trans.Attribute("date").Value);
                decimal quantity = ConvertStringQuantityToDecimnal(trans.Attribute("quantity").Value);
                String productnr = trans.Attribute("productnumber").Value;

                return CreatePayrollTransaction(employee, productnr, quantity, date, false, false, "");
            }

            return "";
        }

        private string GetAbsenceTransactionElement(XElement employee, TransactionItem trans, DateTime endDate, decimal quantity)
        {
            var transaction = new StringBuilder();

            if (trans != null)
            {
                DateTime date = trans.Date;

                //Pos: 1-4
                transaction.Append(CompanyNr);
                //Pos: 5-6
                transaction.Append(AbsenceTtypeID);
                //Pos: 7
                transaction.Append(GetFinalNumberOfYear(date.Year));
                //Pos 8-9
                transaction.Append(GetPaymentPeriod(date.Month));
                //Pos 10-19
                transaction.Append(GetFormatetEmployeeNr(employee.Attribute("nr").Value));
                //Pos 20-25
                transaction.Append(GetDate(date, "yyMMdd"));
                //Pos 26-27
                transaction.Append(GetPayrollProduct(trans.ProductNr, true));
                //Pos 28-32
                //No timeamount for vacationtransactions
                string vacation = "05";
                if (trans.ProductNr == vacation)
                {
                    transaction.Append(FillWithZero(5, ""));
                }

                if (trans.ProductNr != vacation)
                {
                    transaction.Append(FillWithZero(5, (GetTimeFromMinutes(quantity))));
                }
                //Pos 33-34 Not in use
                transaction.Append("  ");
                //Pos 35-37 Alternative to "28-32", see specifikation
                transaction.Append("   ");
                //Pos 38-43
                transaction.Append(GetDate(endDate, "yyMMdd"));
                transaction.Append(Environment.NewLine);
            }

            return transaction.ToString();
        }

        #endregion

        #region Help Methods

        private DateTime GetDate(string date)
        {
            DateTime dateTime;
            DateTime.TryParse(date, out dateTime);
            return dateTime;
        }

        private bool GetBool(string value)
        {
            bool result;
            bool.TryParse(value, out result);
            return result;
        }

        private String GetDate(DateTime date, string format)
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

        private String GetScheduleTFirstRowBeginDate(int year, int month)
        {
            DateTime date = new DateTime(year, month, 1);

            return GetDate(date, "yyMMdd");
        }

        private String GetScheduleTFirstRowEndDate(int year, int month)
        {
            DateTime date = new DateTime(year, month, SCHEDULE_T_FIRST_ROW_END_DAYNR);

            return GetDate(date, "yyMMdd");

        }

        private String GetScheduleTSecondRowBeginDate(int year, int month)
        {
            DateTime date = new DateTime(year, month, SCHEDULE_T_FIRST_ROW_END_DAYNR + 1);

            return GetDate(date, "yyMMdd");
        }

        private String GetScheduleTSecondRowEndDate(int year, int month)
        {
            DateTime date = new DateTime(year, month, 1);
            DateTime lastDayInMonth = CalendarUtility.GetLastDateOfMonth(date);
            return GetDate(lastDayInMonth, "yyMMdd");
        }


        //I specen står det "Tabell från Lönesystem finns att tillgå", 
        //skulle vara bra om vi kunde kolla på tabellen

        //Scheduletime, I am not sure this is correct. 
        private String GetDayCode(double minutes)
        {
            if (minutes <= 0)
                return "**";

            if (minutes < 600)
            {
                String dayCode = ((minutes / 60) * 10).ToString();
                if (dayCode.Length > 2)
                    dayCode = dayCode.Substring(0, 2);

                dayCode = dayCode.Replace(",", "");


                if (dayCode == "0" || dayCode.Length == 0)
                    dayCode = "**";

                if (dayCode.Length == 1) //this should not happen
                    dayCode = "0" + dayCode;

                return dayCode;
            }
            else
            {
                decimal value = (decimal)minutes;
                if (value != 0)
                    value /= 60;
                value = Math.Round(value, 2, MidpointRounding.ToEven);

                int hours = (int)value;
                string alfaHours = "**";

                if (hours == 10)
                {
                    alfaHours = "A";
                }
                if (hours == 11)
                {
                    alfaHours = "B";
                }
                if (hours == 12)
                {
                    alfaHours = "C";
                }
                if (hours == 13)
                {
                    alfaHours = "D";
                }
                if (hours == 14)
                {
                    alfaHours = "E";
                }
                if (hours == 15)
                {
                    alfaHours = "F";
                }
                if (hours == 16)
                {
                    alfaHours = "G";
                }
                if (hours == 17)
                {
                    alfaHours = "H";
                }
                if (hours == 18)
                {
                    alfaHours = "I";
                }
                if (hours == 19)
                {
                    alfaHours = "J";
                }
                if (hours == 20)
                {
                    alfaHours = "K";
                }
                if (hours == 21)
                {
                    alfaHours = "L";
                }
                if (hours == 22)
                {
                    alfaHours = "M";
                }
                if (hours == 23)
                {
                    alfaHours = "N";
                }
                if (hours == 24)
                {
                    alfaHours = "O";
                }
                if (hours == 25)
                {
                    alfaHours = "P";
                }
                if (hours == 26)
                {
                    alfaHours = "Q";
                }
                if (hours == 27)
                {
                    alfaHours = "R";
                }
                if (hours > 27)
                {
                    alfaHours = "S";
                }

                String dayCode = ((minutes / 60) * 10).ToString();
                if (dayCode.Length > 3)
                    dayCode = dayCode.Substring(0, 3);

                dayCode = dayCode.Replace(",", "");

                dayCode = dayCode.Right(1);
                dayCode = alfaHours + dayCode;

                return dayCode;
            }
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

        private string GetQuantityfromQuantity(decimal quantity)
        {
            quantity = Math.Round(quantity, 2, MidpointRounding.ToEven);
            string quantityString = quantity.ToString();
            quantityString = quantityString.Replace(",", "");
            quantityString = quantityString.Replace(".", "");

            return quantityString;
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

        private String FillWithBlanks(int targetSize, string originValue, bool truncate = false)
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

        private String FillWithStars(int targetSize, string originValue, bool truncate = false)
        {
            if (targetSize > originValue.Length)
            {
                string stars = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    stars += "*";
                }
                return (stars + originValue);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
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

        private static string GetAccountInfo(XElement internalAccounts)
        {
            var sb = new StringBuilder();
            foreach (XElement internalAccount in internalAccounts.Elements())
            {
                sb.Append(internalAccount.Attribute("nr").Value);
            }
            return sb.ToString();
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
