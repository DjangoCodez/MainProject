using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    /**
     * Implementation for AdritoLLon salary
     *      
     * */
    public class HLAdapter : ISalarySplittedFormatAdapter
    {
        SettingManager settingManager = new SettingManager(null);

        private List<TransactionItem> payrollTransactionItems;
        private List<ScheduleItem> scheduleItems;
        private List<Employee> employees;
        private List<EmployeeGroup> employeeGroups;
        private List<Tuple<int, DateTime, DateTime>> employmentInPeriod;
        private String externalExportId;
        private bool doNotincludeComments;
        string companyName;
        private DateTime startDate;
        private DateTime stopDate;

        public HLAdapter(List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, String externalExportId, bool doNotIncludeComments, string companyName, List<Employee> employees, List<EmployeeGroup> employeeGroups, DateTime startDate, DateTime stopDate)
        {
            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
            this.externalExportId = externalExportId;
            this.doNotincludeComments = doNotIncludeComments;
            this.companyName = companyName;
            this.employmentInPeriod = new List<Tuple<int, DateTime, DateTime>>();
            this.stopDate = stopDate;
            this.employees = employees;
            this.startDate = startDate;
            this.employeeGroups = employeeGroups;

            if (string.IsNullOrEmpty(this.externalExportId))
            {
                this.externalExportId = "00000";
            }

            if (this.externalExportId.Length > 5)
                this.externalExportId = this.externalExportId.Substring(0, 5);

            foreach (var employee in employees)
            {
                var dates = employee.GetEmploymentDates(startDate, stopDate);

                if (dates.Count > 0)
                {
                    var first = dates.First();
                    var last = dates.Last();
                    employmentInPeriod.Add(Tuple.Create(employee.EmployeeId, first, last));
                }
            }
        }

        #region Salary

        /// <summary>
        /// Transforming to AdritoLLon format (Three files zipped to one)
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>
        public byte[] TransformSalary(XDocument baseXml)
        {
            string events = GetPayRollEvents();
            string absence = GetAbsence();
            string guid = this.externalExportId + DateTime.Now.Millisecond.ToString();
            string tempfolder = ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_SALARY_PHYSICAL;
            string zippedpath = $@"{tempfolder}\{companyName}{guid}.zip";

            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add($@"{tempfolder}\IT{this.externalExportId}TRS.HLW", events);
            dict.Add($@"{tempfolder}\FRV{this.externalExportId}TRS.XML", absence);

            if (ZipUtility.ZipFiles(zippedpath, dict))
            {
                var result = File.ReadAllBytes(zippedpath);
                File.Delete(zippedpath);

                return result;
            }

            return null;
        }

        #region Data

        private string GetAbsence()
        {
            XElement doc = new XElement("Export");
            doc.Add(new XElement("KildeInfo",
                                new XElement("EksportertDato", GetHLXMLDateString(DateTime.Now.Date)),
                                new XElement("KildeInfoNavn", "SoftOne")));

            List<IGrouping<String, TransactionItem>> transactionItemsGroupByEmployeeId = payrollTransactionItems.GroupBy(o => o.EmployeeId).ToList();

            foreach (IGrouping<String, TransactionItem> employeeTrans in transactionItemsGroupByEmployeeId)
            {
                string employeeId = employeeTrans.First().EmployeeId;
                List<ScheduleItem> employeeScheduleItems = this.scheduleItems.Where(w => w.EmployeeId == employeeId).ToList();
                List<TransactionItem> absenceTransactions = employeeTrans.Where(t => t.IsAbsence()).ToList();
                int id = 1;
                var coherents = GetCoherentTransactions(absenceTransactions, false, employeeScheduleItems);

                foreach (var item in coherents)
                {
                    var startDate = item.Item1;
                    var stopDate = item.Item3;
                    var schedules = employeeScheduleItems.Where(w => w.Date >= startDate && w.Date <= stopDate).ToList();
                    var percent = GetAbsencePercent(item.Item5.Quantity, Convert.ToDecimal(schedules.Sum(s => (s.TotalMinutes - s.TotalBreakMinutes))));

                    doc.Add(new XElement("Rec",
                    new XAttribute("id", id),

                    new XElement("Fravar",
                    new XElement("Klient", externalExportId),
                    new XElement("AnsattNr", item.Item5.EmployeeNr),
                    new XElement("FKodeNr", item.Item5.GetProductCode()),
                    new XElement("FraDato", GetHLXMLDateString(startDate)),
                    new XElement("TilDato", GetHLXMLDateString(stopDate)),
                    new XElement("Merknad", ""),
                    new XElement("AntalTimer", GetMinutesToHourXMLString(item.Item5.Quantity)),
                    new XElement("Sykemeldingsprosent", percent))));

                    id++;
                }
            }

            return doc.ToString();
        }

        private string GetWorkDaysPerWeek()
        {
            var sb = new StringBuilder();
            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = this.scheduleItems.GroupBy(o => o.EmployeeId).ToList();

            foreach (IGrouping<String, ScheduleItem> item in scheduleItemsGroupByEmployeeId)
            {
                if (item == null)
                    continue;

                var employee = this.employees.FirstOrDefault(f => f.EmployeeId.ToString() == item.First().EmployeeId);

                if (employee == null)
                    continue;

                sb.Append(GetEmployeeWorkDaysPerWeek(item.ToList(), employee));
            }

            return sb.ToString();
        }

        private string GetWorkTimePerWeek()
        {
            var sb = new StringBuilder();
            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = this.scheduleItems.GroupBy(o => o.EmployeeId).ToList();

            foreach (IGrouping<String, ScheduleItem> item in scheduleItemsGroupByEmployeeId)
            {
                if (item == null)
                    continue;

                var employee = this.employees.FirstOrDefault(f => f.EmployeeId.ToString() == item.First().EmployeeId);

                if (employee == null)
                    continue;

                if (payrollTransactionItems.Any(t => t.EmployeeId == item.First().EmployeeId && t.IsAbsenceSick()))
                    sb.Append(GetEmployeeWorkTimesPerWeek(item.ToList(), employee));
            }

            return sb.ToString();
        }


        private string GetPayRollEvents()
        {
            var sb = new StringBuilder();

            List<IGrouping<String, TransactionItem>> transactionItemsGroupByEmployeeId = payrollTransactionItems.GroupBy(o => o.EmployeeId).ToList();

            foreach (IGrouping<String, TransactionItem> item in transactionItemsGroupByEmployeeId)
            {
                sb.Append(GetEmployeeTransactions(item.ToList()));
            }

            return sb.ToString();
        }

        public string GetEmployeeWorkDaysPerWeek(List<ScheduleItem> employeeScheduleItems, Employee employee)
        {
            if (employeeScheduleItems.Count == 0)
                return "";

            if (employee == null)
                return "";

            decimal workDaysPerWeekAvarage = new decimal(5); //TODO
            string employeeNr = employeeScheduleItems.First().EmployeeNr;
            int employeeId = 0;
            int.TryParse(employeeScheduleItems.First().EmployeeId, out employeeId);
            StringBuilder sb = new StringBuilder();

            foreach (var employment in employee.GetEmployments(this.startDate, this.stopDate))
            {
                //Företag	Företag	AN	5	1 - 5	Enligt Aditro L.
                sb.Append(FillWithBlanksEnd(5, this.externalExportId, true));

                sb.Append(FillWithBlanksEnd(6, ""));

                //Anställningsnummer	AN	12	11-22	Enligt Aditro L.  //10 in example....
                sb.Append(FillWithBlanksEnd(10, employeeNr));

                //Begrepp	Identitet på begreppet	AN	40	18 - 57	Enligt Aditro L:s begreppsförteckning. PArbetsdgr/snitt
                sb.Append(FillWithBlanksEnd(40, "PArbetsdgr/snitt"));

                //Värde	Begreppets värde	AN/N	x	58 - 157 	Minst ett tecken
                sb.Append(FillWithBlanksEnd(100, GetWorkDaysPerWeekAvarage(employeeScheduleItems, employment)));

                //Datum	Ev. historik-datum	AN	10	158 - 167	ÅÅÅÅ-MM-DD.
                sb.Append(FillWithBlanksEnd(10, GetStartDateString(employment)));

                //T o m-datum	T o m-datum	AN	10	168-177	ÅÅÅÅ-MM-DD.          T o m - datum kan anges för datumrelaterade begrepp.Ignoreras för övriga.
                sb.Append(FillWithBlanksEnd(10, GetStopDateString(employment)));

                sb.Append(FillWithBlanksEnd(58, ""));
                sb.Append("N");
                sb.Append(FillWithBlanksEnd(29, ""));
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        public string GetEmployeeWorkTimesPerWeek(List<ScheduleItem> employeeScheduleItems, Employee employee)
        {
            if (employeeScheduleItems.Count == 0)
                return "";

            if (employee == null)
                return "";

            decimal workDaysPerWeekAvarage = new decimal(5); //TODO
            string employeeNr = employeeScheduleItems.First().EmployeeNr;
            int employeeId = 0;
            int.TryParse(employeeScheduleItems.First().EmployeeId, out employeeId);
            StringBuilder sb = new StringBuilder();

            foreach (var tuple in CalendarUtility.GetWeekIntervalsFromDate(this.startDate, this.stopDate))
            {
                foreach (var employment in employee.GetEmployments(tuple.Item1, tuple.Item2))
                {
                    var scheduleItemsWithExtraShiftForWeek = employeeScheduleItems.Where(w => w.ExtraShift && w.Date >= tuple.Item1 && w.Date <= tuple.Item2);
                    var absenceSickTransactionsForWeek = payrollTransactionItems.Where(t => t.IsAbsenceSick() && t.Date >= tuple.Item1 && t.Date <= tuple.Item2).ToList();
                    if (scheduleItemsWithExtraShiftForWeek.Any() && absenceSickTransactionsForWeek.Any())
                    {
                        var fulltime = employment.GetFullTimeWorkTimeWeek(employment.GetEmployeeGroup(tuple.Item1, employeeGroups), tuple.Item1);
                        var weekPlusExtraShiftTime = employment.GetWorkTimeWeek(tuple.Item1) + scheduleItemsWithExtraShiftForWeek.Sum(s => s.TotalMinutes - s.TotalBreakMinutes);

                        if (weekPlusExtraShiftTime > fulltime)
                            weekPlusExtraShiftTime = fulltime;

                        //Fält    Beskrivning             Typ     Längd   Pos         Anm
                        //Företag Företag                 AN      5       1 - 5       Enligt Aditro Lön.
                        //Anstnr  Anställningsnummer      AN      12      6 - 17      Enligt Aditro Lön.
                        //Begrepp Identitet på begreppet  AN      40      18 - 57     Enligt Aditro Lön: s begreppsförteckning. P - PERVASNITT
                        //Värde   Begreppets värde        AN/N    x       58 - 157    Minst ett tecken
                        //Datum   Ev.historik - datum     AN      10      158 - 167   ÅÅÅÅ - MM - DD.
                        //T o m - datum T o m - datum     AN      10      168 - 177   ÅÅÅÅ - MM - DD.
                        //T o m - datum kan anges för datumrelaterade begrepp.Ignoreras för övriga.

                        //Företag	Företag	AN	5	1 - 5	Enligt Aditro L.
                        sb.Append(FillWithBlanksEnd(5, this.externalExportId, true));

                        //Anställningsnummer	AN	12	6-17	Enligt Aditro L.  //10 in example....
                        sb.Append(FillWithBlanksEnd(12, employeeNr));

                        //Begrepp	Identitet på begreppet	AN	40	18 - 57	Enligt Aditro L:s begreppsförteckning. PArbetsdgr/snitt
                        sb.Append(FillWithBlanksEnd(40, "P_PERVASNITT"));

                        //Värde	Begreppets värde	AN/N	x	58 - 157 	Minst ett tecken
                        sb.Append(FillWithBlanksEnd(100, Decimal.Round(Decimal.Divide((decimal)weekPlusExtraShiftTime, 60), 2).ToString()));

                        //Datum	Ev. historik-datum	AN	10	158 - 167	ÅÅÅÅ-MM-DD.
                        sb.Append(FillWithBlanksEnd(10, tuple.Item1.ToShortDateString()));

                        //T o m-datum	T o m-datum	AN	10	168-177	ÅÅÅÅ-MM-DD.          T o m - datum kan anges för datumrelaterade begrepp.Ignoreras för övriga.
                        sb.Append(FillWithBlanksEnd(10, tuple.Item2.ToShortDateString()));

                        sb.Append(Environment.NewLine);
                    }
                }
            }

            return sb.ToString();
        }

        private string GetEmployeeTransactions(List<TransactionItem> employeeTransactions)
        {
            if (!employeeTransactions.Any())
                return "";

            var sb = new StringBuilder();

            string employeeId = employeeTransactions.First().EmployeeId;

            List<ScheduleItem> employeeScheduleItems = this.scheduleItems.Where(w => w.EmployeeId == employeeId).ToList();

            List<TransactionItem> presenceTransactions = employeeTransactions.Where(t => !(t.IsAbsence() || t.IsAbsence_SicknessSalary()) && !t.IsAddition()).ToList();
            List<TransactionItem> AdditionDeductionTransactions = employeeTransactions.Where(t => t.IsAddition()).ToList();
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions = GetCoherentTransactions(presenceTransactions, true, employeeScheduleItems);
            coherentPresenceTransactions = MergePresenceTransactions(coherentPresenceTransactions);

            foreach (var item in AdditionDeductionTransactions)
                coherentPresenceTransactions.Add(Tuple.Create(item.Date, item.AbsenceStartTime, item.Date, item.AbsenceStopTime, item));

            foreach (var transaction in coherentPresenceTransactions)
            {
                transaction.Item5.IsAbsence = false;
                sb.Append(GetSalaryTransactionElement(transaction.Item5, transaction.Item1, transaction.Item2, transaction.Item3, transaction.Item4, employeeScheduleItems, null));
            }

            return sb.ToString();
        }

        private bool IsTransactionCoherent(TransactionItem transactionInSeqence, TransactionItem transactionToMatch, int dayIntervall, bool isPresence)
        {
            if (transactionInSeqence.Date.AddDays(dayIntervall) == transactionToMatch.Date || isPresence || transactionToMatch.Amount != 0)
            {
                if (transactionInSeqence.AccountInternals != null && transactionToMatch.AccountInternals != null && transactionInSeqence.AccountInternals.Count == transactionToMatch.AccountInternals.Count)
                {
                    //math the accountinternals that we export
                    if (GetAccountNr(TermGroup_SieAccountDim.CostCentre, transactionInSeqence.AccountInternals) == GetAccountNr(TermGroup_SieAccountDim.CostCentre, transactionToMatch.AccountInternals))
                        return true;
                }
                else if (transactionInSeqence.AccountInternals == null && transactionToMatch.AccountInternals == null)
                    return true;
            }

            return false;
        }

        private string GetSalaryTransactionElement(TransactionItem transaction, DateTime start, TimeSpan startTime, DateTime stop, TimeSpan stopTime, List<ScheduleItem> employeeScheduleItems, List<TransactionItem> alltransactionItemsOnDate)
        {
            var sb = new StringBuilder();

            //Person Id 6 1 6 Høyrestilt, ledende nuller.
            //Lønnsartnummer 5 7 11 Høyrestilt, ledende nuller.
            //Avdelingsnummer 12 12 23 Høyrestilt, ledende nuller(kun ved numerisk felt)
            //Prosjektnummer 12 24 35 Høyrestilt, ledende nuller(kun ved numerisk felt)
            //Element 1 - nummer 12 36 47 Høyrestilt, ledende nuller(kun ved numerisk felt)
            //Element 2 - nummer 12 48 59 Høyrestilt, ledende nuller(kun ved numerisk felt)
            //Element 3 - nummer 12 60 71 Høyrestilt, ledende nuller(kun ved numerisk felt)
            //Element 4 - nummer 12 72 83 Høyrestilt, ledende nuller(kun ved numerisk felt)
            //Element 5 - nummer 12 84 95 Høyrestilt, ledende nuller(kun ved numerisk felt)
            //Dato 6 96 101 Format: ddmmåå(uten skilletegn)
            //Antall 10 102 111 Høyrestilt, ledende nuller. Uten desimalskilletegn.
            //Sats 10 112 121 Høyrestilt, ledende nuller. Uten desimalskilletegn.
            //Beløp 13 122 134 Høyrestilt, ledende nuller. Uten desimalskilletegn.
            //Filler 30 135 164
            //CR(Carriage return) 1 165 165 ASCII = 13(Hex: 0D)
            //LF(Line feed) 1 166 166 ASCII = 10(Hex: 0A)

            if (transaction != null)
            {
                //Person Id 6 1 6 Høyrestilt, ledende nuller.
                sb.Append(FillWithZeroBeginning(6, transaction.EmployeeNr));

                //Lønnsartnummer 5 7 11 Høyrestilt, ledende nuller.
                sb.Append(FillWithZeroBeginning(5, transaction.GetProductCode()));

                //Avdelingsnummer 12 12 23 Høyrestilt, ledende nuller(kun ved numerisk felt)
                sb.Append(GetAccountNrString(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals));

                //Prosjektnummer 12 24 35 Høyrestilt, ledende nuller(kun ved numerisk felt)
                //sb.Append(FillWithZeroBeginning(12, GetAccountNr(TermGroup_SieAccountDim.Project, transaction.AccountInternals)));

                //Element 1 - nummer 12 36 47 Høyrestilt, ledende nuller(kun ved numerisk felt)
                //Element 2 - nummer 12 48 59 Høyrestilt, ledende nuller(kun ved numerisk felt)
                //Element 3 - nummer 12 60 71 Høyrestilt, ledende nuller(kun ved numerisk felt)
                //Element 4 - nummer 12 72 83 Høyrestilt, ledende nuller(kun ved numerisk felt)
                //Element 5 - nummer 12 84 95 Høyrestilt, ledende nuller(kun ved numerisk felt)
                sb.Append(FillWithBlanksEnd(72, "")); //Not used in exemplefil so not used here?

                //Dato 6 96 101 Format: ddmmåå(uten skilletegn)
                sb.Append(GetHLDateString(start));

                //Antall 10 102 111 Høyrestilt, ledende nuller. Uten desimalskilletegn.
                if (transaction.IsAddition())
                    sb.Append(FillWithZeroBeginning(10, GetQuantityToString(transaction.Quantity)));
                else
                    sb.Append(FillWithZeroBeginning(10, GetMinutesToHourString(transaction.Quantity)));

                //Sats 10 112 121 Høyrestilt, ledende nuller. Uten desimalskilletegn.
                //sb.Append(FillWithZeroBeginning(10, ""));

                //Beløp 13 122 134 Høyrestilt, ledende nuller. Uten desimalskilletegn.
                //sb.Append(FillWithZeroBeginning(10, GetQuantityToString(transaction.Amount)));
                //Filler 30 135 164
                //sb.Append(FillWithZeroBeginning(30, ""));
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        #endregion

        #region Schedule

        public byte[] TransformSchedule(XDocument baseXmlDontUse)
        {
            string doc = string.Empty;

            doc += GetSchedule();
            return Encoding.GetEncoding("ISO-8859-1").GetBytes(doc);

        }

        private string GetSchedule()
        {
            var sb = new StringBuilder();
            //SplitSchduleItemsAccourdingToBreaks();

            List<ScheduleItem> scheduleItemsWithoutBreaks = scheduleItems.Where(s => !s.IsBreak).ToList();
            List<IGrouping<String, ScheduleItem>> scheduleItemsGroupByEmployeeId = scheduleItemsWithoutBreaks.GroupBy(o => o.EmployeeId).ToList();

            foreach (var tempScheduleItemsForEmployee in scheduleItemsGroupByEmployeeId)
            {

                //EmployeeId to get the right transactions in order to find the deviations
                var employeeInSchedule = tempScheduleItemsForEmployee.FirstOrDefault();
                string employeeId = employeeInSchedule.EmployeeId;
                //List<ScheduleItem> zeroDays = new List<ScheduleItem>();
                //foreach (var item in tempScheduleItemsForEmployee.OrderBy(x=> x.Date))
                //{

                //}


                foreach (var item in tempScheduleItemsForEmployee.GroupBy(b => b.Date).OrderBy(o => o.Key))
                {
                    //Företag	AN	5	1-5	Enligt Aditro L.
                    sb.Append(FillWithBlanksEnd(5, this.externalExportId, true));

                    //Anställningsnummer	AN	12	6-17	Enligt Aditro L.
                    sb.Append(FillWithBlanksEnd(12, item.First().EmployeeNr));

                    //Datum	D	10	18-27	ÅÅÅÅ-MM-DD.
                    sb.Append(FillWithBlanksEnd(10, item.Key.ToShortDateString()));

                    //Dagkod	AN	5	28-32	Dagkoden måste finnas upplagd i modul ‘Arbetstidschema’.
                    sb.Append(FillWithBlanksEnd(5, FillWithZeroBeginning(4, GetMinutesToHourString(Convert.ToDecimal(item.Sum(s => s.TotalMinutes) - item.Sum(s => s.TotalBreakMinutes)))), true));

                    ////Schematyp	AN	5	33-37	Sätts till ‘OS’ om justering av dag i ordinarie arbetstidsschema är aktuellt.  Sätts till ‘PF’ om justerning av dag i partiella frånvaroschemat är aktuellt.
                    //sb.Append(FillWithBlanksEnd(5, ""));

                    sb.Append(Environment.NewLine);
                }

            }
            return sb.ToString();
        }
        #endregion

        #region Help methods

        private List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> GetCoherentTransactions(List<TransactionItem> transactionItems, bool isPresence, List<ScheduleItem> scheduleItems)
        {
            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => o.ProductCode).ToList();
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
                bool isWholeDayAbsence = transactionsOrderedByDate.First().IncludedInWholeDayAbsence(scheduleItems);

                List<DateTime> dates = transactionsOrderedByDate.OrderBy(o => o.Date).Select(s => s.Date).ToList();
                var currentDate = dates.First();
                //var lastDate = dates.Last();
                var previousDate = currentDate.AddDays(-1);
                //List<List<DateTime>> datesIngroup = new List<List<DateTime>>();

                //while (currentDate <= lastDate)
                //{
                //    if (currentDate != previousDate.AddDays(-1))
                //        datesIngroup.Add()
                //    currentDate = currentDate.AddDays(1);
                //}

                foreach (var currentItem in transactionsOrderedByDate)
                {
                    counter++;

                    if (counter == 1)
                    {
                        firstTransInSequence = currentItem;
                    }

                    var wholeDayChanged = isWholeDayAbsence != currentItem.IncludedInWholeDayAbsence(scheduleItems);

                    isWholeDayAbsence = currentItem.IncludedInWholeDayAbsence(scheduleItems);

                    if ((counter == 1 && wholeDayChanged))
                        wholeDayChanged = false;

                    if (counter > 1 && !wholeDayChanged && !isPresence && !currentItem.IncludedInWholeDayAbsence(scheduleItems)) // Make sure part of day always only one transaction row in file.
                        wholeDayChanged = true;

                    if (!wholeDayChanged && !isPresence && currentItem.Date.AddDays(-1) != previousDate) // Extra check to make sure no gapes in absence is allowed.
                        wholeDayChanged = true;

                    previousDate = currentItem.Date;

                    //look if item is in the current datesequnce if Absence
                    if (!wholeDayChanged && IsTransactionCoherent(firstTransInSequence, currentItem, dayIntervall, isPresence))
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
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,
                        };

                        coherentTrnsaction.SetIncludedInWholeDayAbsence(firstTransInSequence.IncludedInWholeDayAbsence(scheduleItems));

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
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,

                        };

                        coherentTrnsaction.SetIncludedInWholeDayAbsence(currentItem.IncludedInWholeDayAbsence(scheduleItems));
                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, stopDate.Value, stopTime.Value, coherentTrnsaction));
                    }
                }
            }

            return coherentTransactions;
        }


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
                    ExternalCode = firstItem.ExternalCode,
                    EmployeeNr = firstItem.EmployeeNr,
                    ProductNr = firstItem.ProductNr,
                    IsAbsence = firstItem.IsAbsence,
                    AccountInternals = firstItem.AccountInternals,
                    Account = firstItem.Account,
                    IsRegistrationQuantity = firstItem.IsRegistrationQuantity,
                    IsRegistrationTime = firstItem.IsRegistrationTime,
                    ProductCode = firstItem.ProductCode,
                    TimeDeviationCauseId = firstItem.TimeDeviationCauseId,
                };

                foreach (var tmpItem in matchingTransactions)
                {
                    newItem.Quantity += tmpItem.Item5.Quantity;
                    newItem.Time += tmpItem.Item5.Time;
                    if (doNotincludeComments)
                    {
                        newItem.Comment += "";
                    }
                    else
                    {
                        newItem.Comment += tmpItem.Item5.Comment;
                    }
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

        private string GetWorkDaysPerWeekAvarage(List<ScheduleItem> employeeScheduleItems, Employment employment)
        {
            DateTime startDate = GetStartDate(employment);
            DateTime stopDate = GetStopDate(employment);
            int days = GetNumberOfDays(startDate, stopDate);
            decimal numberOfweeks = GetNumberOfWeeks(days);
            List<DateTime> scheduledDates = employeeScheduleItems.Where(s => s.TotalMinutes != 0 && s.EmployeeId == employment.EmployeeId.ToString()).Select(s => s.Date).Distinct().ToList();
            int numberOfScheduleDays = scheduledDates.Count;

            if (numberOfweeks == 0)
                return "0";

            decimal perweek = decimal.Divide(numberOfScheduleDays, numberOfweeks);
            perweek = decimal.Round(perweek, 2);
            string value = perweek.ToString().Replace(",", ".");

            return value;
        }

        private decimal GetNumberOfWeeks(int days)
        {
            if (days == 0)
                return 0;

            decimal numberOfWeeks = decimal.Divide(days, 7);
            return numberOfWeeks;
        }

        private int GetNumberOfDays(DateTime startDate, DateTime stopDate)
        {
            int days = Convert.ToInt32((stopDate - startDate).TotalDays + 1);

            return days;
        }

        private string GetHLDateString(DateTime date)
        {
            string day = date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString();
            string month = date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString();
            string year = (date.Year - 2000).ToString();
            return day + month + year;
        }

        private string GetHLXMLDateString(DateTime date)
        {
            string day = date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString();
            string month = date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString();
            string year = (date.Year).ToString();
            return day + "." + month + "." + year;
        }

        private string GetStartDateString(Employment employment)
        {
            DateTime date = GetStartDate(employment);

            if (date == DateTime.MinValue)
                return "";
            else
                return date.ToShortDateString();
        }

        private string GetStopDateString(Employment employment)
        {
            DateTime date = GetStopDate(employment);

            if (date.Date == DateTime.MinValue || date.Date == this.stopDate.Date)
                return "";
            else
                return date.ToShortDateString();
        }

        private DateTime GetStartDate(Employment employment)
        {
            if (employment == null)
                return this.startDate;

            var tuple = employmentInPeriod.FirstOrDefault(f => f.Item1 == employment.EmployeeId && f.Item2 >= employment.DateFrom);

            if (tuple == null)
                return DateTime.MinValue;
            else
                return tuple.Item2;
        }

        private DateTime GetStopDate(Employment employment)
        {
            if (employment == null)
                return this.startDate;

            DateTime employmentStopDate = employment.DateTo.HasValue && employment.DateTo.Value != CalendarUtility.DATETIME_DEFAULT ? employment.DateTo.Value : DateTime.MaxValue;

            var tuple = employmentInPeriod.FirstOrDefault(f => f.Item1 == employment.EmployeeId && f.Item3 <= employmentStopDate);

            if (tuple == null)
                return DateTime.MinValue;
            else
                return tuple.Item3;
        }

        private string GetAbsencePercent(decimal AbsenceAmount, decimal ScheduleAmount)
        {
            decimal absencevalue = AbsenceAmount;
            decimal schedulevalue = ScheduleAmount;
            decimal absencePercent = 0;
            if (absencevalue != 0)
                absencePercent = absencevalue / schedulevalue;
            absencePercent = Math.Round(absencePercent * 100, 2, MidpointRounding.ToEven);
            String absencePercentString = Convert.ToString(absencePercent);
            absencePercentString = absencePercentString.Replace(",", ".");
            return absencePercentString;
        }

        private string GetAccountNrString(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
        {
            string account = GetAccountNr(accountDim, internalAccounts);
            int value = 0;
            if (int.TryParse(account, out value))
                return FillWithZeroBeginning(12, account, true);

            return FillWithBlanksBeginning(12, account, true);
        }

        private String GetAccountNr(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
        {

            if (internalAccounts != null)
            {
                foreach (AccountInternal internalAccount in internalAccounts)
                {
                    if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue)
                    {
                        if (internalAccount.Account.AccountDim.SysSieDimNr.Value == (int)accountDim)
                            return internalAccount.Account.AccountNr;
                    }
                }
            }
            return "";
        }

        private String GetAdritoLLonTimeDecimalValue(String amount)
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

        private String FillWithBlanksEnd(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = "";

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

        private string GetMinutesToHourString(decimal minutes)
        {
            decimal inHours = minutes / 60;
            string value = inHours.ToString("0.00");
            value = value.Replace(",", "");
            value = value.Replace(".", "");
            return value;
        }

        private string GetMinutesToHourXMLString(decimal minutes)
        {
            decimal inHours = minutes / 60;
            string value = inHours.ToString("0.00");
            value = value.Replace(",", ".");
            return value;
        }

        private string GetQuantityToString(decimal amount)
        {
            string value = amount.ToString("0.00");
            value = value.Replace(",", "");
            value = value.Replace(".", "");
            return value;
        }

        private String FillWithBlanksBeginning(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize == originValue.Length)
                return originValue;

            if (targetSize > originValue.Length)
            {
                string zeros = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    zeros += "";
                }
                return (zeros + originValue);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }

        private String FillWithZeroBeginning(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize == originValue.Length)
                return originValue;

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

        private String GetAdritoLLonMinutesValue(String quantity, String externalExportId)
        {
            String daySchedule = externalExportId;

            if (externalExportId != "M")
            {
                return quantity;

            }
            else
            {
                //Using Code from Svensklön to create schedule information in only 2 positions

                // Special functionaly for Myrorna, Number Only didn't work for them
                decimal minutes;
                decimal.TryParse(quantity, out minutes);
                String dayCode = String.Empty;

                //Negative values never allowed
                if (minutes < 0)
                    minutes = 0;

                if (minutes < 600)
                {
                    dayCode = ((minutes / 60) * 10).ToString();
                    if (dayCode.Length > 2)
                        dayCode = dayCode.Substring(0, 2);

                    dayCode = dayCode.Replace(",", "");


                    if (dayCode == "0" || dayCode.Length == 0)
                        dayCode = "00";

                    if (dayCode.Length == 1) //this should not happen
                        dayCode = "0" + dayCode;

                }
                else
                {
                    decimal value = (decimal)minutes;
                    if (value != 0)
                        value /= 60;
                    value = Math.Round(value, 2, MidpointRounding.ToEven);

                    int hours = (int)value;
                    string alfaHours = "00";

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

                    dayCode = ((minutes / 60) * 10).ToString();
                    if (dayCode.Length > 3)
                        dayCode = dayCode.Substring(0, 3);

                    dayCode = dayCode.Replace(",", "");

                    dayCode = dayCode.Right(1);
                    dayCode = alfaHours + dayCode;

                }

                daySchedule += dayCode;

                return daySchedule;

            }
        }

        //60 => 6000
        //60.25 => 6025        
        private String GetAdritoLLonDecimalValue(String quantity)
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
