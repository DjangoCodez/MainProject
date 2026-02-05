using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.Config;
using System.IO;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Business.Core.TimeEngine;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    /**
     * Implementation for SD Worx salary
     *      
     * */
    public class SDWorxAdapter : ISalarySplittedFormatAdapter
    {
        // Fixed code for the selected section
        //private readonly SettingManager settingManager = new SettingManager(null);

        private readonly List<TransactionItem> payrollTransactionItems;
        private readonly List<ScheduleItem> scheduleItems;
        private readonly List<TransactionItem> extendedPayrollTransactionItems;
        private readonly List<ScheduleItem> extendedScheduleItems;
        private readonly List<Employee> employees;
        private readonly List<EmployeeGroup> employeeGroups;
        private readonly List<TimeSchedulePlanningDayDTO> extendedTimeScheduleTemplates;
        private readonly List<Tuple<int, DateTime, DateTime>> employmentInPeriod;
        private readonly string externalExportId;
        private readonly bool doNotincludeComments;
        private readonly string companyName;
        private readonly DateTime startDate;
        private readonly DateTime stopDate;
        private readonly List<EmployeeAccount> _employeeAccounts;
        private const string _kTid = "KTID";
        private readonly SDWorxSettings settings = new SDWorxSettings();

        public SDWorxAdapter(List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, String externalExportId, bool doNotIncludeComments, string companyName, List<Employee> employees, List<EmployeeGroup> employeeGroups, DateTime startDate, DateTime stopDate, List<EmployeeAccount> employeeAccounts, List<TimeSchedulePlanningDayDTO> extendedTemplateBlocks)
        {
            this.payrollTransactionItems = payrollTransactions.Where(w => w.Date >= startDate && w.Date <= stopDate).ToList();
            this.scheduleItems = scheduleItems.Where(w => w.Date >= startDate && w.Date <= stopDate).ToList();
            this.extendedPayrollTransactionItems = payrollTransactions;
            this.extendedScheduleItems = scheduleItems;
            this.externalExportId = externalExportId;
            this.doNotincludeComments = doNotIncludeComments;
            this.companyName = companyName;
            this.employmentInPeriod = new List<Tuple<int, DateTime, DateTime>>();
            this.stopDate = stopDate;
            this.employees = employees;
            this.startDate = startDate;
            this.employeeGroups = employeeGroups;
            this._employeeAccounts = employeeAccounts;

            this.extendedTimeScheduleTemplates = extendedTemplateBlocks;

            if (string.IsNullOrEmpty(this.externalExportId))
            {
                this.externalExportId = "00000";
            }

            if (this.externalExportId.Length > 5)
                this.externalExportId = this.externalExportId.Substring(0, 5);

            foreach (var employee in employees)
            {
                var dates = employee.GetEmploymentDates(startDate, stopDate);

                if (dates.Any())
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
            string workdays = GetWorkDaysPerWeek();
            string workTimeWeek = GetWorkTimePerWeek();
            string schedule = GetSchedule();
            string guid = this.externalExportId + DateTime.Now.Millisecond.ToString();
            string tempfolder = ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_SALARY_PHYSICAL;
            string zippedpath = $@"{tempfolder}\{companyName}{guid}.zip";

            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                { $@"{tempfolder}\LON02Rorlig_Lon02 {companyName} handelser {externalExportId} {guid}.txt", events },
                { $@"{tempfolder}\Person_Sch2 {companyName} arbetsdagar {externalExportId} {guid}.txt", workdays },
                { $@"{tempfolder}\Person_Sch3 {companyName} veckoarbetstid {externalExportId} {guid}.txt", workTimeWeek },
                { $@"{tempfolder}\PSJ_Sch {companyName} schema {externalExportId} {guid}.txt", schedule }
            };

            if (ZipUtility.ZipFiles(zippedpath, dict))
            {
                var result = File.ReadAllBytes(zippedpath);
                File.Delete(zippedpath);

                return result;
            }

            return null;
        }

        #region Data

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

            foreach (var payrollTransactionItem in payrollTransactionItems.Where(w => string.IsNullOrEmpty(GetAccountNr(TermGroup_SieAccountDim.CostCentre, w.AccountInternals))))
            {
                var ea = _employeeAccounts.FirstOrDefault(w => w.EmployeeId.ToString() == payrollTransactionItem.EmployeeId);

                if (ea != null && ea.Account != null && ea.Account.AccountInternal != null)
                {
                    if (payrollTransactionItem.AccountInternals == null)
                        payrollTransactionItem.AccountInternals = new List<AccountInternal>();

                    payrollTransactionItem.AccountInternals.Add(ea.Account.AccountInternal);
                }
            }

            List<IGrouping<String, TransactionItem>> transactionItemsGroupByEmployeeId = payrollTransactionItems.GroupBy(o => o.EmployeeId).ToList();

            foreach (IGrouping<String, TransactionItem> item in transactionItemsGroupByEmployeeId)
            {
                sb.Append(GetEmployeeTransactions(item.ToList()));
            }

            return sb.ToString();
        }

        public string GetEmployeeWorkDaysPerWeek(List<ScheduleItem> employeeScheduleItems, Employee employee)
        {
            if (!employeeScheduleItems.Any())
                return "";

            if (employee == null)
                return "";

            string employeeNr = employeeScheduleItems.First().EmployeeNr;
            int.TryParse(employeeScheduleItems.First().EmployeeId, out _);
            StringBuilder sb = new StringBuilder();

            foreach (var employment in employee.GetEmployments(this.startDate, this.stopDate))
            {
                //Företag	Företag	AN	5	1 - 5	Enligt Aditro L.
                sb.Append(FillWithBlanksEnd(5, this.externalExportId));

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
            if (!employeeScheduleItems.Any())
                return "";

            if (employee == null)
                return "";

            string employeeNr = employeeScheduleItems.First().EmployeeNr;
            int.TryParse(employeeScheduleItems.First().EmployeeId, out _);
            StringBuilder sb = new StringBuilder();

            foreach (var tuple in CalendarUtility.GetWeekIntervalsFromDate(this.startDate, this.stopDate))
            {
                foreach (var employment in employee.GetEmployments(tuple.Item1, tuple.Item2))
                {
                    var scheduleItemsWithExtraShiftForWeek = employeeScheduleItems.Where(w => w.ExtraShift && w.Date >= tuple.Item1 && w.Date <= tuple.Item2);
                    var absenceSickTransactionsForWeek = payrollTransactionItems.Where(t => t.IsAbsenceSick() && t.Date >= tuple.Item1 && t.Date <= tuple.Item2).ToList();
                    if (absenceSickTransactionsForWeek.Any())
                    {
                        var fulltime = employment.GetFullTimeWorkTimeWeek(employment.GetEmployeeGroup(tuple.Item1, employeeGroups), tuple.Item1);
                        var pervaInMinutes = CalculateEmployeeWeekMinutes(employee, employment.GetEmployeeGroup(tuple.Item1, employeeGroups), tuple.Item1, employment.GetWorkTimeWeek(tuple.Item1), Convert.ToInt32(scheduleItemsWithExtraShiftForWeek.Sum(s => s.TotalMinutes - s.TotalBreakMinutes)));

                        if (!pervaInMinutes.HasValue)
                        {
                            if (scheduleItemsWithExtraShiftForWeek.Any())
                                pervaInMinutes = employment.GetWorkTimeWeek(tuple.Item1) + Convert.ToInt32(scheduleItemsWithExtraShiftForWeek.Sum(s => s.TotalMinutes - s.TotalBreakMinutes));
                            else
                                pervaInMinutes = employment.GetWorkTimeWeek(tuple.Item1);
                        }

                        if (pervaInMinutes > fulltime)
                            pervaInMinutes = fulltime;

                        //Fält    Beskrivning             Typ     Längd   Pos         Anm
                        //Företag Företag                 AN      5       1 - 5       Enligt Aditro Lön.
                        //Anstnr  Anställningsnummer      AN      12      6 - 17      Enligt Aditro Lön.
                        //Begrepp Identitet på begreppet  AN      40      18 - 57     Enligt Aditro Lön: s begreppsförteckning. P - PERVASNITT
                        //Värde   Begreppets värde        AN/N    x       58 - 157    Minst ett tecken
                        //Datum   Ev.historik - datum     AN      10      158 - 167   ÅÅÅÅ - MM - DD.
                        //T o m - datum T o m - datum     AN      10      168 - 177   ÅÅÅÅ - MM - DD.
                        //T o m - datum kan anges för datumrelaterade begrepp.Ignoreras för övriga.

                        //Företag	Företag	AN	5	1 - 5	Enligt Aditro L.
                        sb.Append(FillWithBlanksEnd(5, this.externalExportId));

                        //Anställningsnummer	AN	12	6-17	Enligt Aditro L.  //10 in example....
                        sb.Append(FillWithBlanksEnd(12, employeeNr));

                        //Begrepp	Identitet på begreppet	AN	40	18 - 57	Enligt Aditro L:s begreppsförteckning. PArbetsdgr/snitt
                        sb.Append(FillWithBlanksEnd(40, "P_PERVASNITT"));

                        //Värde	Begreppets värde	AN/N	x	58 - 157 	Minst ett tecken
                        sb.Append(FillWithBlanksEnd(100, Decimal.Round(Decimal.Divide((decimal)pervaInMinutes, 60), 2).ToString()));

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

        private int? CalculateEmployeeWeekMinutes(Employee employee, EmployeeGroup employeeGroup, DateTime date, int employeeWorkTimeWeekMinutes, int employeeExtraShiftMinutes)
        {
            if (employee != null && employeeGroup != null)
            {
                switch (employeeGroup.QualifyingDayCalculationRule)
                {
                    case (int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeek:
                        return employee?.GetEmployment(date)?.GetWorkTimeWeek(date) ?? 0;
                    case (int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusExtraShifts:
                        return PayrollManager.GetEmployeeWorkTimeWeekPlusExtraShiftsMinutes(employeeWorkTimeWeekMinutes, employeeExtraShiftMinutes);
                    case (int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusAdditionalContract:
                        DateTime beginningOfWeek = CalendarUtility.GetBeginningOfWeek(date);
                        DateTime endOfWeek = CalendarUtility.GetEndOfWeek(date);

                        if (extendedTimeScheduleTemplates == null || extendedScheduleItems == null || extendedPayrollTransactionItems == null)
                            return null;

                        int templateScheduleInWeek = Convert.ToInt32(extendedTimeScheduleTemplates.Where(w => w.EmployeeId == employee.EmployeeId && w.ActualDate >= beginningOfWeek && w.ActualDate <= endOfWeek).Sum(s => (s.StopTime - s.StartTime).TotalMinutes - s.TotalBreakMinutes));
                        int activeScheduleInWeek = Convert.ToInt32(extendedScheduleItems.Where(w => w.EmployeeNr == employee.EmployeeNr && w.Date >= beginningOfWeek && w.Date <= endOfWeek).Sum(s => s.TotalMinutes - s.TotalBreakMinutes));
                        int leaveOfAbsenceMinutesInWeek = Convert.ToInt32(extendedPayrollTransactionItems.Where(w => w.EmployeeNr == employee.EmployeeNr && w.Date >= beginningOfWeek && w.Date <= endOfWeek && w.IsLeaveOfAbsence()).Sum(s => s.Quantity));
                        return PayrollManager.GetEmployeeWorkTimeWeekPlusAdditionalContractMinutes(employeeWorkTimeWeekMinutes, activeScheduleInWeek, templateScheduleInWeek, leaveOfAbsenceMinutesInWeek);
                    case (int)TermGroup_QualifyingDayCalculationRule.UseAverageCalculationInTimePeriod:
                        var employment = employee.GetEmployment(date);

                        if (extendedTimeScheduleTemplates == null || extendedScheduleItems == null || extendedPayrollTransactionItems == null)
                            return null;

                        int activeSchedule = Convert.ToInt32(extendedScheduleItems.Where(w => w.EmployeeNr == employee.EmployeeNr && w.Date >= startDate && w.Date <= stopDate).Sum(s => s.TotalMinutes - s.TotalBreakMinutes));
                        int leaveOfAbsenceMinutes = Convert.ToInt32(extendedPayrollTransactionItems.Where(w => w.EmployeeNr == employee.EmployeeNr && w.Date >= startDate && w.Date <= stopDate && w.IsLeaveOfAbsence()).Sum(s => s.Quantity));
                        int employmentDaysInPeriod = employee.GetEmploymentDays(startDate, stopDate);
                        int templateSchedule = Convert.ToInt32(extendedTimeScheduleTemplates.Where(w => w.EmployeeId == employee.EmployeeId && w.ActualDate >= startDate && w.ActualDate <= stopDate).Sum(s => s.NetTime - s.TotalBreakMinutes));
                        return PayrollManager.GetEmployeeAverageCalculateMinutes(employmentDaysInPeriod, activeSchedule, templateSchedule, leaveOfAbsenceMinutes, employment.GetPercent(date), employment.GetFullTimeWorkTimeWeek(employeeGroup, date));
                }
            }
            return null;
        }

        private string GetEmployeeTransactions(List<TransactionItem> employeeTransactions)
        {
            if (!employeeTransactions.Any())
                return "";

            var sb = new StringBuilder();

            string employeeId = employeeTransactions.First().EmployeeId;

            List<ScheduleItem> employeeScheduleItems = this.scheduleItems.Where(w => w.EmployeeId == employeeId).ToList();

            List<TransactionItem> presenceTransactions = employeeTransactions.Where(t => !(t.IsAbsence() || t.IsAbsence_SicknessSalary()) && !t.IsAddition()).ToList();
            List<TransactionItem> absenseTransactions = employeeTransactions.Where(t => (t.IsAbsence() || t.IsAbsence_SicknessSalary()) && !t.IsAddition()).ToList();
            List<TransactionItem> AdditionDeductionTransactions = employeeTransactions.Where(t => t.IsAddition()).ToList();
            List<TransactionItem> kTidTransactions = presenceTransactions.Where(w => w.ProductNr == _kTid).ToList();

            List<TransactionItem> absenceSicktransactionItems = absenseTransactions.Where(t => t.IsAbsence_SicknessSalary()).ToList();
            absenseTransactions = absenseTransactions.Where(t => !t.IsAbsence_SicknessSalary()).ToList();
            presenceTransactions = presenceTransactions.Where(w => w.ProductNr != _kTid).ToList();

            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions = GetCoherentTransactions(presenceTransactions, true, employeeScheduleItems);
            if (!this.settings.SendTransactionPerDay)
                coherentPresenceTransactions = MergePresenceTransactions(coherentPresenceTransactions);

            foreach (var ktidGrouOnDay in kTidTransactions.GroupBy(g => $"{g.Date}#{g.EmployeeNr}"))
            {
                List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> ktidCoherentPresenceTransactions = GetCoherentTransactions(ktidGrouOnDay.ToList(), true, employeeScheduleItems);
                if (!this.settings.SendTransactionPerDay)
                    coherentPresenceTransactions.AddRange(MergePresenceTransactions(ktidCoherentPresenceTransactions));
                else
                    coherentPresenceTransactions.AddRange(ktidCoherentPresenceTransactions);
            }

            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentAbsenceTransactions = GetCoherentTransactions(absenseTransactions, false, employeeScheduleItems);

            foreach (var item in AdditionDeductionTransactions)
                coherentPresenceTransactions.Add(Tuple.Create(item.Date, item.AbsenceStartTime, item.Date, item.AbsenceStopTime, item));

            foreach (var transaction in coherentPresenceTransactions)
            {
                transaction.Item5.IsAbsence = false;
                sb.Append(GetSalaryTransactionElement(transaction.Item5, transaction.Item1, transaction.Item3, employeeScheduleItems));
            }


            foreach (var group in absenceSicktransactionItems.GroupBy(g => g.ProductNr + "#" + g.Date.ToString() + GetAccountNr(TermGroup_SieAccountDim.CostCentre, g.AccountInternals)))
            {
                var transaction = group.First();
                transaction.Quantity = group.Sum(s => s.Quantity);
                transaction.IsAbsence = true;
                sb.Append(GetSalaryTransactionElement(transaction, transaction.Date, transaction.Date, employeeScheduleItems));
            }

            foreach (var transaction in coherentAbsenceTransactions)
            {
                transaction.Item5.IsAbsence = true;
                sb.Append(GetSalaryTransactionElement(transaction.Item5, transaction.Item1, transaction.Item3, employeeScheduleItems));
            }
            return sb.ToString();
        }

        private bool IsTransactionCoherent(TransactionItem transactionInSeqence, TransactionItem transactionToMatch, int dayIntervall)
        {
            if (transactionInSeqence.Date.AddDays(dayIntervall) == transactionToMatch.Date || transactionToMatch.Amount != 0)
            {
                if (transactionInSeqence.AccountInternals != null && transactionToMatch.AccountInternals != null)
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

        private string GetSalaryTransactionElement(TransactionItem transaction, DateTime start, DateTime stop, List<ScheduleItem> employeeScheduleItems)
        {
            var sb = new StringBuilder();

            //Speciell behandling av sjukOB löneart 806, 807, 808 och 80980
            bool isSjukOB = transaction.ProductNr.Equals("806") || transaction.ProductNr.Equals("41231") || transaction.ProductNr.Equals("807") || transaction.ProductNr.Equals("808") || transaction.ProductNr.Equals("80980");

            if (transaction != null)
            {
                if (transaction.Quantity == 0 && transaction.Amount == 0)
                    return sb.ToString();

                //Transaktionens identitet	AN	5	1-5	LON02 (Flex)
                sb.Append(FillWithBlanksEnd(5, "LON02"));

                //Företag AN  5   6 - 10    Enligt Aditro L.
                sb.Append(FillWithBlanksEnd(5, this.externalExportId));

                //Anställningsnummer	AN	12	11-22	Enligt Aditro L.
                sb.Append(FillWithBlanksEnd(12, transaction.EmployeeNr));

                //Art	AN	5	23-27	Enligt Aditro L.
                sb.Append(FillWithBlanksEnd(5, FillWithZeroBeginning(3, transaction.GetProductCode())));

                //Datum fr o m	D	10	28-37	ÅÅÅÅ-MM-DD                Obligatoriskt för frånvaro.
                if (transaction.ProductNr == _kTid) // Månadavlönades arbetade tid, en rad per datum o konteringskombo
                {
                    sb.Append(FillWithBlanksEnd(10, start.ToShortDateString()));
                }
                else if (!transaction.IsAbsence && !settings.SendTransactionPerDay)
                {
                    sb.Append(FillWithBlanksEnd(10, ""));
                }
                else
                {
                    sb.Append(FillWithBlanksEnd(10, start.ToShortDateString()));
                }

                //Datum t o m	D	10	38-47	ÅÅÅÅ-MM-DD               Obligatoriskt för frånvarointervall.
                if (transaction.ProductNr == _kTid) // Månadavlönades arbetade tid, en rad per datum o konteringskombo
                {
                    sb.Append(FillWithBlanksEnd(10, stop.ToShortDateString()));
                }
                else if (!transaction.IsAbsence && !settings.SendTransactionPerDay)
                {
                    sb.Append(FillWithBlanksEnd(10, ""));
                }
                else
                {
                    sb.Append(FillWithBlanksEnd(10, stop.ToShortDateString()));
                }

                //Timmar	N	7+2	48-56	
                //if (transaction.IncludedInWholeDayAbsence(employeeScheduleItems))
                //    sb.Append(FillWithZeroBeginning(9, GetMinutesToHourString(transaction.Quantity), true));
                //else

                if (transaction.ProductNr != _kTid && (!settings.UseHours && !transaction.IsAddition()))
                    sb.Append(FillWithZeroBeginning(9, ""));
                else
                    sb.Append(FillWithZeroBeginning(9, GetMinutesToHourString(transaction.Quantity)));

                //Procent	N	3+2	57-61	Frånvaroprocent av schematiden. Anges inte om = 100. Procent för ett komprimerat tillfälle där varje dag har samma procentsats.
                var procent = "";
                if (settings.UsePercent && transaction.IsAbsence && !isSjukOB)
                   CalculateAbsenceProcent(transaction, start, stop, employeeScheduleItems, out procent);

                sb.Append(FillWithZeroBeginning(5, procent));

                //Belopp N   12 + 2    62 - 75   I förekommande fall.Beloppet ska anges i ören.Om negativt belopp ska importeras ska minustecknet anges efter beloppet.
                sb.Append(FillWithZeroBeginning(14, GetQuantityToString(transaction.Amount), true));

                //Arbetsdagar	N	5+2	76-82
                sb.Append(FillWithZeroBeginning(7, ""));

                //Kalenderdagar	N	5+2	83-89
                sb.Append(FillWithZeroBeginning(7, ""));

                //Antal	N	12+2	90-103	För frånvaro del av dag samt ersättningar.
                if (transaction.ProductNr == _kTid || (settings.UseHours && !transaction.IsAddition()))
                    sb.Append(FillWithZeroBeginning(14, ""));
                else if (isSjukOB)
                    sb.Append(FillWithZeroBeginning(14, GetMinutesToHourString(transaction.Quantity), true));
                else if (transaction.IsAddition())
                {
                    if (transaction.IsRegistrationQuantity)
                        sb.Append(FillWithZeroBeginning(14, GetQuantityToString(transaction.Quantity), true));
                    else
                        sb.Append(FillWithZeroBeginning(14, GetMinutesToHourString(transaction.Quantity), true));
                }
                else if (!transaction.IncludedInWholeDayAbsence(employeeScheduleItems))
                    sb.Append(FillWithZeroBeginning(14, GetMinutesToHourString(transaction.Quantity), true));
                else
                    sb.Append(FillWithZeroBeginning(14, ""));

                //Apris	N	7+2	104-112	A-pris.
                sb.Append(FillWithZeroBeginning(9, ""));

                //Kostnadsställe	AN	20	113-132	Avvikande kostnadsställe.
                sb.Append(FillWithBlanksEnd(20, GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals)));

                //Fritt org begrepp 3	AN	20	133-152	Fri användning (från Flex K-bärare).
                sb.Append(FillWithBlanksEnd(20, ""));

                //Fritt org begrepp 4	AN	20	153-172	Fri användning (från Flex project).
                sb.Append(FillWithBlanksEnd(20, ""));

                //Fritt org begrepp 5	AN	20	173-192	Fri användning (från Flex Konto).
                sb.Append(FillWithBlanksEnd(20, ""));

                //Anmärkning	AN	20	193-212	Anm till lönespecifikationen.
                sb.Append(FillWithBlanksEnd(20, ""));

                //Återbetalning	N	1	213	Återbetalningsmarkering “0”, “1” eller “blankt”.
                sb.Append(FillWithBlanksEnd(1, "0"));

                sb.Append(FillWithBlanksEnd(10, ""));

                //Heltidsbelopp	N	12+ 2	224-237	Heltidslön.
                sb.Append(FillWithZeroBeginning(14, ""));

                //Löneändringsorsak	AN	5	238-242	Löneändringsorsak. Värde “1”, “2”, “3” eller “blankt”.“1” = Sysselsättningsgradsförändring.“2” = Löneglidning.“3” = Ej löneglidning.
                sb.Append(FillWithBlanksEnd(5, ""));

                //Revisionspåverkan	N	1	243	Revisionspåverkan. Värde “1” eller “blankt”.“1” = Ej revisionspåverkande.“Blankt” = Revisionspåverkande.
                sb.Append(FillWithBlanksEnd(1, ""));

                //Behandlingskod	AN	1	244	Behandling av posten. Värde “U”, “F”, “D” eller “blankt”.
                //“U” = Uppdatera om det finns en lönehändelse samma art och datum fr o m, annars tillägg av post.
                //“F” = Felsignalera om det finns en lönehändelse med samma art och datum fr o m, annars tillägg av post.
                //“D” = (Delete)Ta bort lönehändelse med angiven art och datum fr o m.
                //“Blankt” = Tillägg av post
                sb.Append(FillWithBlanksEnd(1, ""));

                //Fritt org begrepp 6	AN	20	245-264	Fri användning
                sb.Append(FillWithBlanksEnd(20, ""));

                //Fritt org begrepp 7	AN	20	265-284	Fri användning
                sb.Append(FillWithBlanksEnd(20, ""));

                //Fritt org begrepp 8	AN	20	285-304	Fri användning
                sb.Append(FillWithBlanksEnd(20, ""));

                //Fritt org begrepp 9   AN  20  305-324 Fri användning
                sb.Append(FillWithBlanksEnd(20, ""));

                //Fritt org begrepp 10	AN	20	245-264	Fri användning
                sb.Append(FillWithBlanksEnd(20, ""));

                ////Fritt org begrepp 11	AN	20	325-344	Fri användning
                sb.Append(FillWithBlanksEnd(20, ""));

                ////Fritt org begrepp 12	AN	20	365-384	Fri användning
                sb.Append(FillWithBlanksEnd(20, ""));

                ////Avvikande avtal	AN	5	385-389	Avvikande avtal
                sb.Append(FillWithBlanksEnd(5, ""));

                ////Avvikande pkat	AN	5	390-394	Avvikande pkat
                sb.Append(FillWithBlanksEnd(5, ""));

                ////Avvikande skatt	AN	1	395	Avvikande skatt
                sb.Append(FillWithBlanksEnd(1, ""));

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        #endregion

        #endregion

        #region Schedule

        public byte[] TransformSchedule(XDocument baseXml)
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

                foreach (var item in tempScheduleItemsForEmployee.GroupBy(b => b.Date).OrderBy(o => o.Key))
                {
                    //Företag	AN	5	1-5	Enligt Aditro L.
                    sb.Append(FillWithBlanksEnd(5, this.externalExportId));

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
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = transactionItems.GroupBy(o => GroupOnTimeCode(o)).ToList();

            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentTransactions = new List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>>();

            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                List<TransactionItem> transactionsOrderedByDate = transactionItemsForProductNumber.OrderBy(o => o.Date).ToList();

                DateTime? transactionStopDate = null;
                TimeSpan? stopTime = null;
                TransactionItem firstTransInSequence = null;
                int dayIntervall = 0;
                int counter = 0;

                decimal accQuantity = 0;
                double accTime = 0;
                StringBuilder accComment = new StringBuilder();
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
                    if (isWholeDayAbsence && !wholeDayChanged && IsTransactionCoherent(firstTransInSequence, currentItem, dayIntervall))
                    {
                        transactionStopDate = currentItem.Date;
                        stopTime = currentItem.AbsenceStopTime;

                        accQuantity += currentItem.Quantity;
                        accTime += currentItem.Time;
                        if (doNotincludeComments)
                        {
                            accComment.Append("");
                        }
                        else
                        {
                            accComment.Append(currentItem.Comment);
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
                            Comment = accComment.ToString(),
                            Amount = accAmount,
                            IsRegistrationQuantity = firstTransInSequence.IsRegistrationQuantity,
                            IsRegistrationTime = firstTransInSequence.IsRegistrationTime,
                            ProductCode = firstTransInSequence.ProductCode,
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,
                        };

                        coherentTrnsaction.SetIncludedInWholeDayAbsence(firstTransInSequence.IncludedInWholeDayAbsence(scheduleItems));

                        if (firstTransInSequence.Date == currentItem.Date)
                            coherentTrnsaction.Date = currentItem.Date;

                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, transactionStopDate ?? currentItem.Date, stopTime ?? currentItem.AbsenceStopTime, coherentTrnsaction));

                        //currentItem is the first item in the new sequence, it can also be the last one!
                        firstTransInSequence = currentItem;
                        transactionStopDate = currentItem.Date;
                        stopTime = currentItem.AbsenceStopTime;
                        accQuantity = currentItem.Quantity;
                        accTime = currentItem.Time;
                        if (doNotincludeComments)
                        {
                            accComment.Append("");
                        }
                        else
                        {
                            accComment.Append(currentItem.Comment);
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
                            TimeDeviationCauseId = firstTransInSequence.TimeDeviationCauseId,
                            ExternalCode = firstTransInSequence.ExternalCode,
                            EmployeeNr = firstTransInSequence.EmployeeNr,
                            Quantity = accQuantity,
                            Time = accTime,
                            ProductNr = firstTransInSequence.ProductNr,
                            IsAbsence = firstTransInSequence.IsAbsence,
                            AccountInternals = firstTransInSequence.AccountInternals,
                            Account = firstTransInSequence.Account,
                            Comment = accComment.ToString(),
                            Amount = accAmount,
                            IsRegistrationQuantity = firstTransInSequence.IsRegistrationQuantity,
                            IsRegistrationTime = firstTransInSequence.IsRegistrationTime,
                            ProductCode = firstTransInSequence.ProductCode,

                        };

                        coherentTrnsaction.SetIncludedInWholeDayAbsence(currentItem.IncludedInWholeDayAbsence(scheduleItems));
                        coherentTransactions.Add(Tuple.Create(firstTransInSequence.Date, firstTransInSequence.AbsenceStartTime, transactionStopDate.Value, stopTime.Value, coherentTrnsaction));
                    }
                }
            }

            return coherentTransactions;
        }
        private string GroupOnTimeCode(TransactionItem item)
        {
            return this.settings.SendTransactionPerDay && !item.IsAbsence() 
                ? $"{item.ProductCode}|{item.Date.ToShortDateString()}"
                : item.ProductCode;
        }
        private string CalculateAbsenceProcent(TransactionItem transaction, DateTime start, DateTime stop, List<ScheduleItem> scheduleItems, out string procent)
        {
            procent = "";
            if (transaction.IsAbsence && scheduleItems != null && scheduleItems.Any())
            {
                DateTime absenceStartDate = start.Date;
                DateTime absenceStopDate = stop.Date;

                var scheduledMinutes = scheduleItems.Where(s => s.Date >= absenceStartDate && s.Date <= absenceStopDate).Sum(s => s.TotalMinutes - s.TotalBreakMinutes);
                if (scheduledMinutes > 0)
                {
                    decimal absenceProcent = decimal.Divide(transaction.Quantity, (decimal)scheduledMinutes) * 100;
                    absenceProcent = decimal.Round(absenceProcent, 2);
                    if (absenceProcent >= 100)
                        procent = "";
                    else
                        procent = absenceProcent.ToString().Replace(",", "");
                }
            }
            return procent;
        }

        private List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> MergePresenceTransactions(List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> coherentPresenceTransactions)
        {
            List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>> mergedCoherentPresenceTransactions = new List<Tuple<DateTime, TimeSpan, DateTime, TimeSpan, TransactionItem>>();
            coherentPresenceTransactions = coherentPresenceTransactions.Where(x => !x.Item5.IsAbsence).OrderBy(x => x.Item1).ToList();

            while (coherentPresenceTransactions.Any())
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

                DateTime transStartDate = matchingTransactions.FirstOrDefault().Item1;
                TimeSpan transStartTime = matchingTransactions.FirstOrDefault().Item2;
                DateTime transStopDate = matchingTransactions.LastOrDefault().Item3;
                TimeSpan transStopTime = matchingTransactions.LastOrDefault().Item4;

                mergedCoherentPresenceTransactions.Add(Tuple.Create(transStartDate, transStartTime, transStopDate, transStopTime, newItem));
            }

            return mergedCoherentPresenceTransactions;
        }

        private string GetWorkDaysPerWeekAvarage(List<ScheduleItem> employeeScheduleItems, Employment employment)
        {
            DateTime presenceStartDate = GetStartDate(employment);
            DateTime presenceStopDate = GetStopDate(employment);
            int days = GetNumberOfDays(presenceStartDate, presenceStopDate);
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

       

        private String FillWithBlanksEnd(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize > originValue.Length)
            {
                StringBuilder blanks = new StringBuilder();
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    blanks.Append(" ");
                }
                return (originValue + blanks.ToString());
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

        private string GetQuantityToString(decimal amount)
        {
            string value = amount.ToString("0.00");
            value = value.Replace(",", "");
            value = value.Replace(".", "");
            return value;
        }
        private String FillWithZeroBeginning(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize == originValue.Length)
                return originValue;

            if (targetSize > originValue.Length)
            {
                StringBuilder zeros = new StringBuilder();
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    zeros.Append("0");
                }
                return (zeros.ToString() + originValue);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }
        #endregion
    }
    public class SDWorxSettings
    {
        public bool SendTransactionPerDay { get; set; }
        public bool UsePercent { get;  set; }
        public bool UseHours { get; set; }

        public SDWorxSettings()
        {
            SendTransactionPerDay = true;
            UsePercent = true;
            UseHours = true;
        }
    }
}
