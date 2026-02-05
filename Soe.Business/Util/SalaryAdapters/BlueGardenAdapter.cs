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
    public class BlueGardenAdapter : ISalaryAdapter
    {
        private readonly CompEntities context;
        private readonly List<TransactionItem> payrollTransactionItems;
        private readonly List<ScheduleItem> scheduleItems;
        private readonly List<int> employeeIds;
        private readonly List<AccountDim> accountDimInternals;
        private readonly List<Account> accountsWithAccountHierachyPayrollExportExternalCode;
        private readonly List<Account> accountsWithAccountHierachyPayrollExportUnitExternalCode;
        private readonly List<TimeHibernatingAbsenceRowDTO> hibernatingAbsenceRows;
        private readonly List<TimeDeviationCause> timeDeviationCauses;
        private readonly List<TimeAbsenceDetailDTO> timeAbsenceDetails;
        private readonly List<TimeStampEntryDTO> timeStampEntries;
        private readonly DateTime StartDate;
        private readonly List<Employee> employees;
        private readonly bool hasAccountHierachyPayrollExportExternalCodes;
        private readonly bool hasAccountHierachyPayrollExportUnitExternalCodes;
        public const int SCHEDULE_T_FIRST_ROW_END_DAYNR = 16;
        public const int MAX_EMPLOYEE_NR_LENGTH = 10;
        public const int PAYROLLPPRODUCT_LENGTH = 3;
        public const int DEVIATION_CAUSE_LENGTH = 2;
        public const int TIME_HOURS_LENGTH = 3;
        public const int TIME_MINUTES_LENGTH = 2;
        private string unitOnCompany { get; set; }

        private readonly bool useCostCentreOnPosition1OnBTransactions;
        private readonly bool useCostCentreOnFillerPosition1OnBTransactions;
        private readonly bool useCostCentreOnFillerPosition1OnATransactions;
        private readonly bool useCostCentreOnAbsenceOnPosition1OnATransactions;
        private readonly bool applyTele2SkipAccount;
        private readonly bool applyHibernatingAbsenceRow;
        private readonly bool applyHibernatingAbsenceRowInternalAccounts;
        private readonly bool usePayrollCompanyOnTransaction;
        private readonly bool useUnitOnTransaction;
        private readonly bool useLastFillers;
        private readonly bool tryUsingExternalCodeForEmployeeNr;
        private readonly bool mergeOnlyHourlySalaryInTimeBox;
        private readonly bool noTimeBoxOnPresence;
        private readonly bool hasTimeAbsenceDetails;
        private readonly bool hastimeStampEntries;
        private readonly bool partTimeAbsenceInPeriodStringIs2500;

        private String CompanyNr { get; set; }

        #region Constructors

        public BlueGardenAdapter() { }

        public BlueGardenAdapter(CompEntities entities, String externalExportID, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<Employee> employees, DateTime startDate, List<AccountDim> accountDimInternals, BlueGardenCompany blueGardenCompany, List<TimeHibernatingAbsenceRow> hibernatingAbsenceRows, List<TimeDeviationCause> timeDeviationCauses, List<TimeAbsenceDetailDTO> timeAbsenceDetails, List<TimeStampEntryDTO> timeStampEntries, string unit)
        {
            context = entities;
            CompanyNr = externalExportID;
            this.payrollTransactionItems = payrollTransactions;
            this.scheduleItems = scheduleItems;
            this.employeeIds = employees.Select(s => s.EmployeeId).Distinct().ToList();
            this.employees = employees;
            this.StartDate = startDate;
            this.accountDimInternals = accountDimInternals;
            this.accountsWithAccountHierachyPayrollExportExternalCode = new List<Account>();
            this.accountsWithAccountHierachyPayrollExportUnitExternalCode = new List<Account>();
            this.hibernatingAbsenceRows = hibernatingAbsenceRows.ToDTOs(true).ToList();
            this.timeDeviationCauses = timeDeviationCauses;

            if (!string.IsNullOrEmpty(unit))
                this.unitOnCompany = unit;
            else
                this.unitOnCompany = "01";


            if (blueGardenCompany == BlueGardenCompany.Coop)
            {
                this.timeAbsenceDetails = timeAbsenceDetails ?? new List<TimeAbsenceDetailDTO>();
                this.timeStampEntries = timeStampEntries ?? new List<TimeStampEntryDTO>();
                useCostCentreOnPosition1OnBTransactions = false;
                useCostCentreOnFillerPosition1OnBTransactions = true;
                useCostCentreOnFillerPosition1OnATransactions = true;
                applyTele2SkipAccount = false;
                usePayrollCompanyOnTransaction = true;
                useUnitOnTransaction = true;
                useLastFillers = false;
                useCostCentreOnAbsenceOnPosition1OnATransactions = false;
                tryUsingExternalCodeForEmployeeNr = false;
                mergeOnlyHourlySalaryInTimeBox = false;
                applyHibernatingAbsenceRow = false;
                applyHibernatingAbsenceRowInternalAccounts = false;
                noTimeBoxOnPresence = true;
                hasTimeAbsenceDetails = false;
                partTimeAbsenceInPeriodStringIs2500 = true;
            }
            else if (blueGardenCompany == BlueGardenCompany.Tele2)
            {
                this.timeAbsenceDetails = timeAbsenceDetails ?? new List<TimeAbsenceDetailDTO>();
                this.timeStampEntries = timeStampEntries ?? new List<TimeStampEntryDTO>();
                useCostCentreOnPosition1OnBTransactions = false;
                useCostCentreOnFillerPosition1OnBTransactions = false;
                useCostCentreOnFillerPosition1OnATransactions = false;
                applyTele2SkipAccount = true;
                usePayrollCompanyOnTransaction = false;
                useUnitOnTransaction = false;
                useLastFillers = true;
                useCostCentreOnAbsenceOnPosition1OnATransactions = true;
                tryUsingExternalCodeForEmployeeNr = true;
                mergeOnlyHourlySalaryInTimeBox = true;
                applyHibernatingAbsenceRow = false;
                applyHibernatingAbsenceRowInternalAccounts = false;
                noTimeBoxOnPresence = false;
                hasTimeAbsenceDetails = false;
                partTimeAbsenceInPeriodStringIs2500 = false;
            }
            else if (blueGardenCompany == BlueGardenCompany.ICA)
            {
                this.timeAbsenceDetails = timeAbsenceDetails;
                this.timeStampEntries = timeStampEntries;
                hasTimeAbsenceDetails = true;
                useCostCentreOnPosition1OnBTransactions = false;
                useCostCentreOnFillerPosition1OnBTransactions = false;
                useCostCentreOnFillerPosition1OnATransactions = false;
                applyTele2SkipAccount = false;
                usePayrollCompanyOnTransaction = false;
                useUnitOnTransaction = false;
                useLastFillers = true;
                useCostCentreOnAbsenceOnPosition1OnATransactions = true;
                tryUsingExternalCodeForEmployeeNr = false;
                mergeOnlyHourlySalaryInTimeBox = false;
                applyHibernatingAbsenceRow = hibernatingAbsenceRows.Any();
                applyHibernatingAbsenceRowInternalAccounts = false;
                noTimeBoxOnPresence = false;
                hastimeStampEntries = !timeStampEntries.IsNullOrEmpty();
                partTimeAbsenceInPeriodStringIs2500 = false;
            }

            foreach (var dim in accountDimInternals)
            {
                foreach (var acc in dim.Account)
                {
                    if (!string.IsNullOrEmpty(acc.AccountHierachyPayrollExportExternalCode))
                        this.accountsWithAccountHierachyPayrollExportExternalCode.Add(acc);

                    if (!string.IsNullOrEmpty(acc.AccountHierachyPayrollExportUnitExternalCode))
                        this.accountsWithAccountHierachyPayrollExportUnitExternalCode.Add(acc);
                }
            }

            this.hasAccountHierachyPayrollExportExternalCodes = this.accountsWithAccountHierachyPayrollExportExternalCode.Any();
            this.hasAccountHierachyPayrollExportUnitExternalCodes = this.accountsWithAccountHierachyPayrollExportUnitExternalCode.Any();
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
            List<int> handledEmployeeIds = new List<int>();
            foreach (var employeeId in employeeIds)
            {
                List<ScheduleItem> scheduleItemsForEmployee = scheduleItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();
                if (!applyHibernatingAbsenceRow)
                {
                    handledEmployeeIds.Add(employeeId);
                    foreach (var scheduleItems in scheduleItemsForEmployee.GroupBy(g => g.GetExternalEmploymentCode()))
                        parent.Append(GetSchedule(scheduleItems.ToList()));
                }
                else
                {

                    if (payrollTransactionItems.IsNullOrEmpty())
                        continue;

                    var account = payrollTransactionItems.FirstOrDefault(s => s.EmployeeId == employeeId.ToString())?.Account;

                    var hibernatingAbsenceRowsForEmployee = hibernatingAbsenceRows.Where(w => w.EmployeeId == employeeId).ToList();
                    if (hibernatingAbsenceRowsForEmployee.Any())
                    {
                        var employee = employees.FirstOrDefault(f => f.EmployeeId == employeeId);
                        if (employee != null)
                        {
                            List<TransactionItem> hibernatingTransactionItemsForEmployee = new List<TransactionItem>();
                            List<ScheduleItem> hibernatingScheduleItemsForEmployee = scheduleItemsForEmployee.ToList();

                            var employeeAccounts = applyHibernatingAbsenceRowInternalAccounts ? context.EmployeeAccount.Include("Account.AccountInternal").Where(w => w.EmployeeId == employee.EmployeeId && w.State == (int)SoeEntityState.Active).ToList() : new List<EmployeeAccount>();
                            foreach (var row in hibernatingAbsenceRowsForEmployee)
                            {
                                var employment = employee.Employment?.FirstOrDefault(f => f.EmploymentId == row.TimeHibernatingAbsenceHead.EmploymentId);
                                if (employment != null)
                                {
                                    var hibernatingEmployment = employee.GetHibernatingEmployments(employment)?.GetEmployment(row.Date);

                                    if (hibernatingEmployment != null)
                                    {
                                        ScheduleItem item = new ScheduleItem()
                                        {
                                            EmployeeId = employee.EmployeeId.ToString(),
                                            EmployeeNr = employee.EmployeeNr,
                                            StartDate = row.Date,
                                            StopDate = row.Date,
                                            Date = row.Date,
                                            EmployeeSocialSec = employee.SocialSec,
                                            AbsenceMinutes = row.AbsenceTimeMinutes,
                                            TotalBreakMinutes = 0,
                                            TotalMinutes = row.ScheduleTimeMinutes,
                                            ExternalCode = hibernatingEmployment.GetExternalCode(),
                                        };

                                        hibernatingScheduleItemsForEmployee.Add(item);

                                        var matchingTimeDeviationCause = timeDeviationCauses.FirstOrDefault(f => f.TimeDeviationCauseId == row.TimeHibernatingAbsenceHead.TimeDeviationCauseId);

                                        if (matchingTimeDeviationCause != null)
                                        {
                                            var matchingProduct = matchingTimeDeviationCause.TimeCode?.TimeCodePayrollProduct?.FirstOrDefault()?.PayrollProduct;

                                            if (matchingProduct != null)
                                            {
                                                var accounts = employeeAccounts.GetEmployeeAccounts(row.Date, row.Date);
                                                List<AccountInternalDTO> accountInternals = new List<AccountInternalDTO>();
                                                if (!accounts.IsNullOrEmpty())
                                                {
                                                    accountInternals = accounts.Select(s => s.Account?.AccountInternal?.ToDTO()).ToList();
                                                    accountInternals = accountInternals.Where(w => w != null).ToList();
                                                }

                                                TransactionItem transactionItem = new TransactionItem()
                                                {
                                                    EmployeeId = employee.EmployeeId.ToString(),
                                                    EmployeeNr = employee.EmployeeNr,
                                                    Date = row.Date,
                                                    EmployeeSocialSec = employee.SocialSec,
                                                    Quantity = row.AbsenceTimeMinutes,
                                                    ExternalCode = hibernatingEmployment.GetExternalCode(),
                                                    ProductNr = matchingProduct.Number,
                                                    ProductCode = matchingProduct.ExternalNumber,
                                                    IsAbsence = true,
                                                    Account = account,
                                                    TimeDeviationCauseId = matchingTimeDeviationCause.TimeDeviationCauseId,
                                                };

                                                if (applyHibernatingAbsenceRowInternalAccounts)
                                                    transactionItem.SetAccountInternals(accountInternals); //AccountInternalDTOs is not implemented everywhere, if this i needed we need to implement it in all places

                                                hibernatingTransactionItemsForEmployee.Add(transactionItem);
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (var hibernatingScheduleItemsForEmployeeByExternalCode in hibernatingScheduleItemsForEmployee.GroupBy(g => g.GetExternalEmploymentCode()))
                                parent.Append(GetSchedule(hibernatingScheduleItemsForEmployeeByExternalCode.ToList()));
                            parent.Append(GetTimeTransactionsAbsence(hibernatingTransactionItemsForEmployee, null, new List<TimeStampEntryDTO>(), isHibernatingTransaction: true));
                            handledEmployeeIds.Add(employeeId);
                        }
                    }
                }
            }

            var scheduleItemsForEmployeeIdDict = scheduleItems.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());

            foreach (var employeeId in employeeIds)
            {
                if (!handledEmployeeIds.Contains(employeeId) && scheduleItemsForEmployeeIdDict.TryGetValue(employeeId.ToString(), out List<ScheduleItem> scheduleItemsForEmployee))
                {
                    foreach (var scheduleItemGroupByExternalCode in scheduleItemsForEmployee.GroupBy(g => g.GetExternalEmploymentCode()))
                        parent.Append(GetSchedule(scheduleItemGroupByExternalCode.ToList()));
                }
            }

            var payrollTransactionItemsForEmployeeIdDict = payrollTransactionItems.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
            var timeStampEntriesForEmployeeIdDict = timeStampEntries.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
            foreach (var employeeId in employeeIds)
            {
                if (payrollTransactionItemsForEmployeeIdDict.TryGetValue(employeeId.ToString(), out List<TransactionItem> transactionItemsForEmployee))
                {
                    List<TimeAbsenceDetailDTO> timeAbsenceDetailForEmployee = hasTimeAbsenceDetails ? timeAbsenceDetails.Where(w => w.EmployeeId == employeeId).ToList() : null;
                    timeStampEntriesForEmployeeIdDict.TryGetValue(employeeId, out List<TimeStampEntryDTO> timeStampEntriesForEmployee);
                    if (timeStampEntriesForEmployee == null)
                        timeStampEntriesForEmployee = new List<TimeStampEntryDTO>();

                    parent.Append(GetTimeTransactionsAbsence(transactionItemsForEmployee, timeAbsenceDetailForEmployee, timeStampEntriesForEmployee));
                }
            }

            foreach (var employeeId in employeeIds)
            {
                if (payrollTransactionItemsForEmployeeIdDict.TryGetValue(employeeId.ToString(), out List<TransactionItem> transactionItemsForEmployee))
                    parent.Append(GetTimeTransactionsPresence(transactionItemsForEmployee));
            }

            return parent.ToString();
        }

        #region ScheduleTrasacions

        private string GetSchedule(List<ScheduleItem> scheduleItems)
        {
            if (!scheduleItems.Any())
                return "";

            var sb = new StringBuilder();
            var accountingScheduleItem = scheduleItems.Any(x => x.StartDate != x.StopDate) ? scheduleItems.Where(x => x.StartDate != x.StopDate).OrderBy(o => o.Date).FirstOrDefault(f => !f.AccountInternals.IsNullOrEmpty()) : scheduleItems.OrderBy(o => o.Date).FirstOrDefault(f => !f.AccountInternals.IsNullOrEmpty());
            var costCentre = useCostCentreOnPosition1OnBTransactions || useCostCentreOnFillerPosition1OnBTransactions ? GetAccountNr(TermGroup_SieAccountDim.CostCentre, accountingScheduleItem?.AccountInternals) : string.Empty;
            var payrollCompany = usePayrollCompanyOnTransaction && hasAccountHierachyPayrollExportExternalCodes ? SalaryExportUtil.GetAccountHierachyPayrollExportExternalCode(accountingScheduleItem?.AccountInternals, accountsWithAccountHierachyPayrollExportExternalCode) : null;
            var unit = useUnitOnTransaction && hasAccountHierachyPayrollExportUnitExternalCodes ? SalaryExportUtil.GetAccountHierachyPayrollExportUnitExternalCode(accountingScheduleItem?.AccountInternals, accountsWithAccountHierachyPayrollExportUnitExternalCode) : null;

            if (string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(unitOnCompany))
                unit = unitOnCompany;

            int days = DateTime.DaysInMonth(this.StartDate.Year, this.StartDate.Month);

            sb.Append("B");
            sb.Append(!string.IsNullOrEmpty(payrollCompany) ? FillWithBlanksEnd(4, payrollCompany, true) : FillWithBlanksEnd(4, CompanyNr, true)); //Företag längd 4 läge 2
            sb.Append(!string.IsNullOrEmpty(unit) ? FillWithBlanksEnd(2, unit, true) : "01"); //Enhet längd 2 läge 6
            sb.Append(GetFormatetEmployeeNr(GetEmployeeNr(scheduleItems.FirstOrDefault().EmployeeNr, scheduleItems.FirstOrDefault().ExternalCode))); // Arbtagid längd 10 länge 8
            sb.Append(FillWithBlanksEnd(10, GetExternalCode(scheduleItems.FirstOrDefault().EmployeeNr, scheduleItems.FirstOrDefault().ExternalCode), false)); //Anstid Längd 10 läge 18
            sb.Append("000"); // Löneart längd 3 läge 28
            if (!useCostCentreOnPosition1OnBTransactions || string.IsNullOrEmpty(costCentre))
                sb.Append(FillWithBlanksEnd(24, "", false)); // Kostn.ställe 1 längd 24 läge 31
            else
                sb.Append(FillWithBlanksEnd(24, costCentre, false)); // Kostn.ställe 1 längd 24 läge 31
            sb.Append(FillWithBlanksEnd(20, "", false)); // Kostn.ställe 2 längd 20 läge 55
            sb.Append(FillWithBlanksEnd(10, "", false)); // Textfält 1 längd 10 läge 75
            sb.Append(FillWithBlanksEnd(10, "", false)); // Textfält 2 längd 10 läge 85
            sb.Append("01"); //Transnr längd 2 läge 95
            sb.Append(GetDate(this.StartDate, "yyMMdd")); //Startdatum längd 2 läge 103
            sb.Append(days.ToString()); //Periodlängd längd 2 läge 105 
            sb.Append(CreateScheduleForPeriod(scheduleItems)); // Avvikelsedagar längd 140 läge 105
            sb.Append(FillWithZero(4, "", false)); // Filler längd 4 läge 245
            sb.Append(FillWithZero(5, "", false)); //Sem.utt.faktor längd 5 läge 249
            sb.Append(FillWithZero(7, "", false)); //Tjänstled.faktor längd 6 läge 254  7?
            sb.Append(FillWithBlanksEnd(1, "", false)); // Filler längd 1 läge 260
            sb.Append(FillWithZero(7, "", false)); // Filler längd 7 läge 261
            sb.Append(FillWithZero(9, "", false)); // Filler längd 9 läge 268
            sb.Append(FillWithBlanksEnd(1, "", false)); // Filler längd 1 läge 277
            if (!useCostCentreOnFillerPosition1OnBTransactions || string.IsNullOrEmpty(costCentre))
                sb.Append(FillWithBlanksEnd(10, "", false)); // Filler längd 10 läge 278
            else
                sb.Append(FillWithBlanksEnd(10, costCentre, false)); // Filler längd 10 läge 278
            if (useLastFillers)
            {
                sb.Append(FillWithBlanksEnd(6, "", false)); // Filler längd 6 läge 288
                sb.Append(FillWithBlanksEnd(6, "", false)); // Filler längd 6 läge 294
                sb.Append(FillWithZero(7, "", false)); // Filler längd 7 läge 300
                sb.Append(FillWithZero(5, "", false)); //Veckoarbetstid i lönepåverkande uppgifter längd 5 länge 107
                sb.Append(FillWithZero(2, "", false)); // Filler längd 2 läge 312
                sb.Append(FillWithZero(1, "", false)); // Filler längd 1 läge 314
                sb.Append(FillWithZero(10, "", false)); // Filler längd 10 läge 315
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        private string CreateScheduleForPeriod(List<ScheduleItem> scheduleItems)
        {
            StringBuilder sb = new StringBuilder();
            DateTime lookDate = this.StartDate;
            DateTime endDate = lookDate.AddDays(35);

            while (lookDate <= endDate)
            {

                var items = scheduleItems.Where(s => s.Date == lookDate).ToList();
                if (items.Any())
                {
                    var minutes = items.FirstOrDefault().TotalMinutes - items.FirstOrDefault().TotalBreakMinutes;

                    string time = "0000";

                    if (minutes > 0)
                        time = GetTime4PositionFromScheduleMinutes(items.FirstOrDefault().TotalMinutes, items.FirstOrDefault().TotalBreakMinutes);

                    sb.Append(FillWithZerosEnd(4, time, false));
                }
                else
                {
                    sb.Append("0000");
                }
                lookDate = lookDate.AddDays(1);
            }

            return FillWithZerosEnd(139, sb.ToString(), true);
        }


        #endregion

        #region TimeTransactions

        private string GetTimeTransactionsAbsence(List<TransactionItem> transactionItemsForEmployee, List<TimeAbsenceDetailDTO> timeAbsenceDetailsForEmployee, List<TimeStampEntryDTO> timeStampEntriesForEmployee, bool isHibernatingTransaction = false)
        {
            var sb = new StringBuilder();
            sb.Append(GetEmployeeAbsenceTransactions(transactionItemsForEmployee.Where(t => t.IsAbsence).ToList(), timeAbsenceDetailsForEmployee, timeStampEntriesForEmployee, isHibernatingTransaction: isHibernatingTransaction));

            return sb.ToString();
        }

        private string GetTimeTransactionsPresence(List<TransactionItem> transactionItemsForEmployee)
        {
            var sb = new StringBuilder();

            sb.Append(GetEmployeePayrollTransactions(transactionItemsForEmployee.Where(t => !t.IsAbsence).ToList()));

            return sb.ToString();
        }

        public bool SkipAccount(DateTime date, string employeeId)
        {
            if (this.applyTele2SkipAccount)
            {
                bool skipAccountEmployee = true;
                var employee = employees.FirstOrDefault(f => f.EmployeeId.ToString() == employeeId);

                if (employee != null)
                {
                    var employeeGroup = employee.GetEmployeeGroup(date, null);

                    if (employeeGroup != null && (employeeGroup.Name.StartsWith("21") || employeeGroup.Name.StartsWith("22") || employeeGroup.Name.StartsWith("23")))
                        skipAccountEmployee = false;
                    else
                        skipAccountEmployee = true;
                }
                else
                {
                    skipAccountEmployee = false;
                }

                return skipAccountEmployee;
            }

            return false;
        }

        //OBS! This method assumes that transactions are merged on date
        private string GetEmployeeAbsenceTransactions(List<TransactionItem> absenceTransactionItemsForEmployee, List<TimeAbsenceDetailDTO> timeAbsenceDetailsForEmployee, List<TimeStampEntryDTO> timeStampEntriesForEmployee, bool isHibernatingTransaction = false)
        {
            var sb = new StringBuilder();

            if (!absenceTransactionItemsForEmployee.Any())
                return string.Empty;

            var skipAccount = SkipAccount(absenceTransactionItemsForEmployee.OrderBy(o => o.Date).First().Date, absenceTransactionItemsForEmployee.First().EmployeeId);

            if (isHibernatingTransaction && !applyHibernatingAbsenceRowInternalAccounts)
                skipAccount = true;

            var mustGroupOnTimeDeviationCauseIds = timeAbsenceDetailsForEmployee != null ? timeAbsenceDetailsForEmployee.Where(w => w.TimeDeviationCauseId.HasValue && w.Ratio.HasValue && w.Ratio != 0).Select(s => s.TimeDeviationCauseId.Value).Distinct().ToList() : new List<int>();


            var scheduleItemsOnEmployee = scheduleItems != null ? scheduleItems.Where(w => w.EmployeeNr == absenceTransactionItemsForEmployee.First().EmployeeNr).ToList() : new List<ScheduleItem>();
            var allTransactionItemsOnEmployee = this.payrollTransactionItems != null ? this.payrollTransactionItems.Where(w => w.EmployeeNr == absenceTransactionItemsForEmployee.First().EmployeeNr).ToList() : new List<TransactionItem>();
            //Group the transactions by Productnumber
            List<IGrouping<String, TransactionItem>> transactionItemsGroupByProductNumber = absenceTransactionItemsForEmployee.GroupBy(o => o.GroupOnAbsenceBlueGarden(hasTimeAbsenceDetails, mustGroupOnTimeDeviationCauseIds)).ToList();

            foreach (IGrouping<String, TransactionItem> transactionItemsForProductNumber in transactionItemsGroupByProductNumber)
            {
                if (!skipAccount)
                {
                    var splitted = SalaryExportUtil.SplitOnAccounting(transactionItemsForProductNumber.ToList(), GetAccountInternalsOnDim(TermGroup_SieAccountDim.CostCentre, accountDimInternals), this.accountsWithAccountHierachyPayrollExportExternalCode, this.accountsWithAccountHierachyPayrollExportUnitExternalCode);

                    foreach (var split in splitted)
                    {
                        if (split.IsNullOrEmpty())
                            continue;

                        List<TransactionItem> additionalAbsences = new List<TransactionItem>();

                        if (hasTimeAbsenceDetails && timeAbsenceDetailsForEmployee != null)
                        {
                            var timeDeviationCauseId = split?.FirstOrDefault(f => f.TimeDeviationCauseId.HasValue)?.TimeDeviationCauseId;
                            var timeAbsenceDetailsForEmployeeOnTimeDeviationCauses = timeAbsenceDetailsForEmployee.Where(f => f.TimeDeviationCauseId == timeDeviationCauseId && f.Ratio.HasValue && f.Ratio.Value != 0).ToList();

                            List<TransactionItem> mergedTransactions = new List<TransactionItem>();
                            if (split == null || split.Count == 0)
                                continue;

                            foreach (var item in split)
                                item.AbsenceRatio = timeAbsenceDetailsForEmployeeOnTimeDeviationCauses.FirstOrDefault(f => f.Date == item.Date)?.Ratio ?? 0;

                            foreach (var item in split.Where(w => w.AbsenceRatio == 0))
                                item.AbsenceRatio = item.GetAbsenceRatio(scheduleItemsOnEmployee.Where(w => w.Date == item.Date).ToList(), allTransactionItemsOnEmployee.Where(w => w.Date == item.Date).ToList());

                            if (timeDeviationCauseId.HasValue && hastimeStampEntries && !timeStampEntriesForEmployee.IsNullOrEmpty())
                            {
                                foreach (var item in split.Where(w => w.AbsenceRatio != 0))
                                {
                                    var timeStampEntriesOnDate = timeStampEntriesForEmployee.Where(w => w.Date.Date == item.Date && w.TimeDeviationCauseId == timeDeviationCauseId.Value).ToList();

                                    if (timeStampEntriesOnDate.Any())
                                    {
                                        var diff = item.GetAdditionalAbsenceMinutes(scheduleItemsOnEmployee, allTransactionItemsOnEmployee, item.AbsenceRatio, timeDeviationCauseId.Value);

                                        if (diff != 0)
                                        {
                                            var clone = item.CloneDTO();
                                            clone.Quantity = diff;
                                            item.Quantity = item.Quantity - diff;
                                            additionalAbsences.Add(clone);
                                        }
                                    }
                                }
                            }

                            var coherent = SalaryExportUtil.GetCoherentTransactions(split, false, scheduleItemsOnEmployee, doNotincludeComments: true);

                            if (coherent != null)
                            {
                                var withNoAbsenceRatio = new List<TransactionItem>();

                                foreach (var coheren in coherent)
                                {
                                    if (coheren.Item5.GetAbsenceRatio(scheduleItemsOnEmployee, allTransactionItemsOnEmployee) != 0)
                                    {
                                        var absencePeriodString = CreateAbsenceForPeriod(new List<TransactionItem>(), this.StartDate);
                                        var account = skipAccount ? "" : GetAccountNr(TermGroup_SieAccountDim.CostCentre, coheren.Item5.AccountInternals);
                                        sb.Append(CreatePayrollTransaction(coheren.Item5, coheren.Item5.EmployeeNr, coheren.Item5.ExternalCode, coheren.Item5.ProductNr, 0, this.StartDate, this.StartDate, true, account, split.FirstOrDefault().Amount, true, absencePeriodString, coheren.Item1, coheren.Item3, coheren.Item5.AbsenceRatio, isHibernatingTransaction: isHibernatingTransaction));
                                    }
                                    else
                                    {
                                        withNoAbsenceRatio.Add(coheren.Item5);
                                    }
                                }
                                if (additionalAbsences.Any())
                                    withNoAbsenceRatio.AddRange(additionalAbsences);

                                if (withNoAbsenceRatio.Any())
                                {
                                    var absencePeriodString = CreateAbsenceForPeriod(withNoAbsenceRatio, this.StartDate);
                                    var firstWithAccountInternals = withNoAbsenceRatio.FirstOrDefault(f => !f.AccountInternals.IsNullOrEmpty());
                                    var account = skipAccount ? "" : GetAccountNr(TermGroup_SieAccountDim.CostCentre, firstWithAccountInternals != null ? firstWithAccountInternals.AccountInternals :  withNoAbsenceRatio.FirstOrDefault().AccountInternals);
                                    sb.Append(CreatePayrollTransaction(withNoAbsenceRatio.FirstOrDefault(), withNoAbsenceRatio.FirstOrDefault().EmployeeNr, withNoAbsenceRatio.FirstOrDefault().ExternalCode, withNoAbsenceRatio.FirstOrDefault().ProductNr, withNoAbsenceRatio.Sum(s => s.Quantity), this.StartDate, this.StartDate, true, account, withNoAbsenceRatio.FirstOrDefault().Amount, true, absencePeriodString, isHibernatingTransaction: isHibernatingTransaction));
                                }
                            }
                            else
                            {
                                var absencePeriodString = CreateAbsenceForPeriod(split.ToList(), this.StartDate);
                                var firstWithAccountInternals = split.FirstOrDefault(f => !f.AccountInternals.IsNullOrEmpty());
                                var account = skipAccount ? "" : GetAccountNr(TermGroup_SieAccountDim.CostCentre, firstWithAccountInternals != null ? firstWithAccountInternals.AccountInternals: split.FirstOrDefault().AccountInternals);
                                sb.Append(CreatePayrollTransaction(split.FirstOrDefault(), split.FirstOrDefault().EmployeeNr, split.FirstOrDefault().ExternalCode, split.FirstOrDefault().ProductNr, split.Sum(s => s.Quantity), this.StartDate, this.StartDate, true, account, split.FirstOrDefault().Amount, true, absencePeriodString, split.FirstOrDefault().TempStartDate, split.FirstOrDefault().TempStopDate, split.FirstOrDefault().AbsenceRatio, isHibernatingTransaction: isHibernatingTransaction));
                            }
                        }
                        else
                        {
                            if (partTimeAbsenceInPeriodStringIs2500)
                            {
                                var splittedWithNoAbsenceRatio = split.Where(w => w.AbsenceRatio == 0).OrderBy(o => o.Date).ToList();
                                decimal previousRatio = 0.00m;
                                foreach (var item in splittedWithNoAbsenceRatio)
                                {
                                    item.AbsenceRatio = item.GetAbsenceRatioFromTransactions(scheduleItemsOnEmployee.Where(w => w.Date == item.Date).ToList(), allTransactionItemsOnEmployee.Where(w => w.Date == item.Date).ToList());

                                    if (item.AbsenceRatio == 0)
                                        item.AbsenceRatio = previousRatio;

                                    previousRatio = item.AbsenceRatio;
                                }
                            }

                            if (hasAccountHierachyPayrollExportExternalCodes)
                            {
                                foreach (var trans in split.Where(w => !w.AccountInternals.IsNullOrEmpty()))
                                {
                                    trans.PayrollExportExternalCode = SalaryExportUtil.GetAccountHierachyPayrollExportExternalCode(trans.AccountInternals, this.accountsWithAccountHierachyPayrollExportExternalCode);
                                    trans.PayrollExportUnitExternalCode = SalaryExportUtil.GetAccountHierachyPayrollExportUnitExternalCode(trans.AccountInternals, this.accountsWithAccountHierachyPayrollExportUnitExternalCode);
                                }
                            }

                            #region Generate Absence Transactions

                            var absencePeriodString = CreateAbsenceForPeriod(split, this.StartDate);
                            var firstWithAccountInternals = split.FirstOrDefault(f => !f.AccountInternals.IsNullOrEmpty());
                            var account = skipAccount ? "" : GetAccountNr(TermGroup_SieAccountDim.CostCentre, firstWithAccountInternals != null ? firstWithAccountInternals.AccountInternals : split.FirstOrDefault().AccountInternals);
                            var transaction = firstWithAccountInternals != null ? firstWithAccountInternals : split.FirstOrDefault();
                            sb.Append(CreatePayrollTransaction(transaction, transaction.EmployeeNr, transaction.ExternalCode, transaction.ProductNr, split.Sum(s => s.Quantity), this.StartDate, this.StartDate, true, account, transaction.Amount, true, absencePeriodString, isHibernatingTransaction: isHibernatingTransaction));

                            #endregion
                        }
                    }
                }
                else
                {
                    var split = transactionItemsForProductNumber.ToList();

                    if (hasAccountHierachyPayrollExportExternalCodes)
                    {
                        foreach (var trans in split)
                        {
                            trans.PayrollExportExternalCode = SalaryExportUtil.GetAccountHierachyPayrollExportExternalCode(trans.AccountInternals, this.accountsWithAccountHierachyPayrollExportExternalCode);
                            trans.PayrollExportUnitExternalCode = SalaryExportUtil.GetAccountHierachyPayrollExportUnitExternalCode(trans.AccountInternals, this.accountsWithAccountHierachyPayrollExportUnitExternalCode);
                        }
                    }

                    var absencePeriodString = CreateAbsenceForPeriod(split, this.StartDate);
                    var account = skipAccount ? "" : GetAccountNr(TermGroup_SieAccountDim.CostCentre, split.FirstOrDefault().AccountInternals);
                    sb.Append(CreatePayrollTransaction(split.FirstOrDefault(), split.FirstOrDefault()?.EmployeeNr, split.FirstOrDefault()?.ExternalCode, split.FirstOrDefault().ProductNr, split.Sum(s => s.Quantity), this.StartDate, this.StartDate, true, account, split.FirstOrDefault().Amount, true, absencePeriodString, isHibernatingTransaction: isHibernatingTransaction));
                }
            }

            return sb.ToString();
        }

        private string GetEmployeePayrollTransactions(List<TransactionItem> presenceTransactionItemsForEmployee)
        {
            var sb = new StringBuilder();

            //itemsInaYear contains all items for a specific year
            foreach (IGrouping<bool, TransactionItem> itemsOfType in presenceTransactionItemsForEmployee.GroupBy(o => o.IsInTimeBox(mergeOnlyHourlySalaryInTimeBox, noTimeBoxOnPresence)).ToList())
            {
                bool useTimeBox = itemsOfType.Key;
                //Now group the collection for a year on month
                List<IGrouping<string, TransactionItem>> itemsInaYearGroupedByMonth = itemsOfType.GroupBy(o => $"{(useTimeBox || noTimeBoxOnPresence ? "M" + o.Date.Month.ToString() : "D" + o.Date.Date.ToString())}").ToList();

                //itemsInAMonth contains all items for a specific month
                foreach (IGrouping<string, TransactionItem> itemsInAMonth in itemsInaYearGroupedByMonth)
                {
                    //Now group the collection for a month on productnumber
                    List<IGrouping<string, TransactionItem>> itemsInAMonthGroupProductNr = itemsInAMonth.GroupBy(o => (o.ProductNr + "#" + o.GetExternalEmploymentCode())).ToList();

                    //itemsGroupOnMonthAndProductNr contains all items for a specific month and for a specific productnumber
                    foreach (IGrouping<string, TransactionItem> itemsGroupOnMonthAndProductNr in itemsInAMonthGroupProductNr)
                    {
                        // now group on CostPlace

                        List<TransactionItem> transactionsWithCostPlaceInComment = new List<TransactionItem>();

                        foreach (var item in itemsGroupOnMonthAndProductNr)
                        {
                            var skipAccount = SkipAccount(item.Date, item.EmployeeId);

                            transactionsWithCostPlaceInComment.Add(new TransactionItem
                            {
                                Date = item.Date,
                                ProductNr = item.ProductNr,
                                ExternalCode = item.ExternalCode,
                                Quantity = item.Quantity,
                                Time = item.Time,
                                IsRegistrationQuantity = item.IsRegistrationQuantity,
                                IsRegistrationTime = item.IsRegistrationTime,
                                EmployeeNr = item.EmployeeNr,
                                Amount = item.Amount,
                                Comment = skipAccount ? "" : GetAccountNr(TermGroup_SieAccountDim.CostCentre, item.AccountInternals),
                                SysPayrollTypeLevel1 = item.SysPayrollTypeLevel1,
                                SysPayrollTypeLevel2 = item.SysPayrollTypeLevel2,
                                SysPayrollTypeLevel3 = item.SysPayrollTypeLevel3,
                                SysPayrollTypeLevel4 = item.SysPayrollTypeLevel4,
                                TimeDeviationCauseId = item.TimeDeviationCauseId,
                            });
                        }

                        List<IGrouping<string, TransactionItem>> itemsInAMonthGroupProductNrandCostPlace = transactionsWithCostPlaceInComment.GroupBy(o => o.Comment).ToList();

                        foreach (IGrouping<string, TransactionItem> itemsWithProductNrAndCostPlace in itemsInAMonthGroupProductNrandCostPlace)
                        {
                            var ordered = itemsWithProductNrAndCostPlace.OrderBy(i => i.Date);

                            var itemWithProductNrAndCostPlace = ordered.FirstOrDefault();
                            var last = ordered.LastOrDefault();

                            decimal quantity = 0;
                            String productNumber = itemWithProductNrAndCostPlace.ProductNr;
                            DateTime startDate = itemWithProductNrAndCostPlace.Date;
                            DateTime stopDate = last.Date;
                            bool isRegistrationTypeTime = itemWithProductNrAndCostPlace.IsRegistrationTime;
                            bool isRegistrationTypeQuantity = itemWithProductNrAndCostPlace.IsRegistrationQuantity;
                            string employeeNr = itemWithProductNrAndCostPlace.EmployeeNr;
                            string externalCode = itemWithProductNrAndCostPlace.ExternalCode;
                            String accountinternal = itemWithProductNrAndCostPlace.Comment;
                            decimal amount = 0;

                            foreach (var item in itemsWithProductNrAndCostPlace)
                            {
                                amount += item.Amount;
                                quantity += item.Quantity;
                            }

                            string periodTimeString = FillWithZero(140, "", false);

                            if (useTimeBox)
                            {
                                var transItems = itemsWithProductNrAndCostPlace.ToList();
                                periodTimeString = CreatePresenceForPeriod(transItems, this.StartDate);
                            }

                            if (hasAccountHierachyPayrollExportExternalCodes)
                            {
                                last.PayrollExportExternalCode = SalaryExportUtil.GetAccountHierachyPayrollExportExternalCode(last.AccountInternals, this.accountsWithAccountHierachyPayrollExportExternalCode, last.Comment);
                                last.PayrollExportUnitExternalCode = SalaryExportUtil.GetAccountHierachyPayrollExportUnitExternalCode(last.AccountInternals, this.accountsWithAccountHierachyPayrollExportUnitExternalCode, last.Comment);
                            }

                            sb.Append(CreatePayrollTransaction(last, employeeNr, externalCode, productNumber, quantity, startDate, stopDate, false, accountinternal, amount, isRegistrationTypeTime, periodTimeString));
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private string CreatePayrollTransaction(TransactionItem trans, string employeeNr, string externalCode, String productnumber, decimal quantity, DateTime startDate, DateTime stopDate, bool isAbsence, string costCentre, decimal amount, bool isRegistrationTypeTime, string periodString, DateTime? absenseStart = null, DateTime? absenseStop = null, decimal absenceRatio = 0, bool isHibernatingTransaction = false)
        {
            var sb = new StringBuilder();
            bool noDates = periodString != FillWithZero(140, "", false);
            int days = DateTime.DaysInMonth(this.StartDate.Year, this.StartDate.Month);

            if (isRegistrationTypeTime && quantity == 0 && amount == 0 && !isAbsence)
                return string.Empty;

            string costCentreOnAbsenceOnPosition1 = costCentre;

            if ((isAbsence && !useCostCentreOnAbsenceOnPosition1OnATransactions) || (isHibernatingTransaction && !applyHibernatingAbsenceRowInternalAccounts))
                costCentreOnAbsenceOnPosition1 = string.Empty;

            var unit = !string.IsNullOrEmpty(unitOnCompany) ? unitOnCompany : "01";

            //Exempel
            //A200001200001    0000098885030905                                                             011606013000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000003128 0000000000000000 905                   0000000000000000000000000
            //A200001200001    0000098885160905                                                             011606013000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000661 0000000000000000 905                   0000000000000000000000000
            //A200001200001    0000098885150905                                                             011606013000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000175 0000000000000000 905                   0000000000000000000000000

            sb.Append("A");
            sb.Append(!usePayrollCompanyOnTransaction ? FillWithBlanksEnd(4, CompanyNr, true) : (!string.IsNullOrEmpty(trans.PayrollExportExternalCode) ? FillWithBlanksEnd(4, trans.PayrollExportExternalCode, true) : FillWithZero(4, CompanyNr, true))); //Företag längd 4 läge 2
            sb.Append(!useUnitOnTransaction ? unit : (!string.IsNullOrEmpty(trans.PayrollExportUnitExternalCode) ? FillWithBlanksEnd(2, trans.PayrollExportUnitExternalCode, true) : unit)); //Enhet längd 2 läge 6
            sb.Append(GetFormatetEmployeeNr(GetEmployeeNr(employeeNr, externalCode))); // Arbtagid längd 10 länge 8
            sb.Append(FillWithBlanksEnd(10, GetExternalCode(employeeNr, externalCode), false)); //Anstid Längd 10 läge 18
            sb.Append(FillWithZero(3, productnumber, true)); // Löneart längd 3 läge 28
            sb.Append(FillWithBlanksEnd(24, costCentreOnAbsenceOnPosition1, false)); // Kostn.ställe 1 längd 24 läge 31
            sb.Append(FillWithBlanksEnd(20, "", false)); // Kostn.ställe 2 längd 20 läge 55
            sb.Append(FillWithBlanksEnd(10, "", false)); // Textfält 1 längd 10 läge 75
            sb.Append(FillWithBlanksEnd(10, "", false)); // Textfält 2 längd 20 läge 85
            sb.Append("01"); //Transnr längd 2 läge 95
            sb.Append(GetDate(this.StartDate, "yyMMdd")); //Startdatum längd 2 läge 103            
            sb.Append(days.ToString()); //Periodlängd längd 2 läge 105
            sb.Append(periodString); // Avvikelsedagar längd 140 läge 105
            sb.Append(FillWithZero(4, "", false)); // Filler längd 4 läge 245
            sb.Append(FillWithZero(5, "", false)); //Sem.utt.faktor längd 5 läge 249
            if (isRegistrationTypeTime && periodString.Equals(FillWithZero(140, "", false)))
            {
                sb.Append(FillWithZero(6, (GetTime4PositionsFromMinutes(Math.Abs(quantity))), false));  //Antal längd 6 läge 254
            }
            else if (isRegistrationTypeTime && !periodString.Equals(FillWithZero(140, "", false)))
            {
                sb.Append(FillWithZero(6, "", false));
            }
            if (!isRegistrationTypeTime)
            {
                noDates = true;
                sb.Append(FillWithZero(6, (GetQuantityToString(quantity, abs: true)), false));
            }
            sb.Append(quantity < 0 ? "-" : " "); // Antal negativ längd 1 läge 260
            sb.Append(FillWithZero(7, GetQuantityToString(quantity != 0 ? (amount / quantity) : 0, abs: true), true)); // A-pris längd 7 läge 261
            sb.Append(FillWithZero(9, GetQuantityToString(amount, abs: true), true)); //belopp längd 9 läge 268
            sb.Append(amount < 0 ? "-" : " "); // Belopp negativt längd 1 läge 277
            if (!useCostCentreOnFillerPosition1OnATransactions || string.IsNullOrEmpty(costCentre))
                sb.Append(FillWithBlanksEnd(10, "", false)); // Filler längd 10 läge 278
            else if (isHibernatingTransaction && !applyHibernatingAbsenceRowInternalAccounts)
                sb.Append(FillWithBlanksEnd(10, "", false));
            else
                sb.Append(FillWithBlanksEnd(10, costCentre, false)); // Filler längd 10 läge 278
            if (useLastFillers)
            {
                if (!absenseStart.HasValue)
                    sb.Append(noDates ? "      " : GetDate(startDate, "yyMMdd")); //Fromdatum längd 6 läge 288
                else
                    sb.Append(GetDate(absenseStart.Value, "yyMMdd")); //Fromdatum längd 6 läge 288
                if (!absenseStop.HasValue)
                    sb.Append(noDates ? "      " : GetDate(stopDate, "yyMMdd")); //Tomdatum längd 6 läge 294
                else
                    sb.Append(GetDate(absenseStop.Value, "yyMMdd")); //Tomdatum längd 6 läge 294

                if (absenceRatio == 0)
                    sb.Append(FillWithZero(7, "", false)); // frånvaro omfattning längd 7 läge 300
                else
                    sb.Append(FillWithZero(7, decimal.Round(absenceRatio, 2).ToString("0.00").Replace(".", "").Replace(",", ""), false)); // frånvaro omfattning längd 7 läge 300                
                sb.Append(FillWithZero(5, "", false)); //Veckoarbetstid i lönepåverkande uppgifter längd 5 läge 307
                sb.Append(FillWithZero(2, "", false)); // Lönegrad längd 2 läge 312
                sb.Append(FillWithZero(1, "", false)); // Skiftform längd 1 läge 314
                sb.Append(FillWithZero(10, "", false)); // ID/Barn längd 10 läge 315  
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
        private string CreateAbsenceForPeriod(List<TransactionItem> absenceTransactions, DateTime startDate)
        {
            StringBuilder sb = new StringBuilder();
            DateTime lookDate = startDate;
            DateTime endDate = lookDate.AddDays(35);

            while (lookDate <= endDate)
            {
                var items = absenceTransactions.Where(s => s.Date == lookDate).ToList();
                if (items.Any())
                {
                    var minutes = items.Sum(i => i.Quantity);
                    if (minutes != 0)
                    {
                        var time = GetTime4PositionsFromMinutes(minutes);
                        sb.Append(FillWithZerosEnd(4, time, false));
                    }
                    else
                    {
                        if (partTimeAbsenceInPeriodStringIs2500)
                            sb.Append("2500"); // Kod för sammanhängande frånvaro. (halvtid) och nu också för heltid..   
                        else
                            sb.Append("9800"); // Kod för sammanhängande frånvaro.
                    }
                }
                else
                {
                    sb.Append("0000");
                }
                lookDate = lookDate.AddDays(1);
            }

            return FillWithZerosEnd(140, sb.ToString(), true);
        }

        private string CreatePresenceForPeriod(List<TransactionItem> presenceTransactions, DateTime startDate)
        {
            StringBuilder sb = new StringBuilder();
            DateTime lookDate = startDate;
            DateTime endDate = lookDate.AddDays(35);

            while (lookDate <= endDate)
            {
                var items = presenceTransactions.Where(s => s.Date == lookDate).ToList();
                if (items.Any())
                {
                    var minutes = items.Sum(i => i.Quantity);
                    if (minutes != 0)
                    {
                        var time = GetTime4PositionsFromMinutes(minutes);
                        sb.Append(FillWithZerosEnd(4, time, false));
                    }
                    else
                    {
                        sb.Append("0000");
                    }
                }
                else
                {
                    sb.Append("0000");
                }
                lookDate = lookDate.AddDays(1);
            }

            return FillWithZerosEnd(140, sb.ToString(), true);
        }
        #endregion



        #region Help Methods

        public string GetEmployeeNr(string employeeNr, string externalCode)
        {
            if (tryUsingExternalCodeForEmployeeNr)
            {
                if (string.IsNullOrEmpty(externalCode))
                    return employeeNr;

                if (!externalCode.Contains('#'))
                    return employeeNr;

                var array = externalCode.Split('#');
                int count = 0;

                foreach (var item in array)
                {
                    if (count == 1)
                        return item;
                    count++;
                }
            }

            return employeeNr;

        }

        public string GetExternalCode(string employeeNr, string externalCode)
        {
            if (string.IsNullOrEmpty(externalCode))
                return string.Empty;

            if (!externalCode.Contains('#'))
                return externalCode;

            var array = externalCode.Split('#');
            int count = 0;

            foreach (var item in array)
            {
                if (count == 0)
                    return item;
                count++;
            }

            return externalCode;
        }

        private String GetDate(DateTime date, String format)
        {
            return date.ToString(format);
        }

        private String GetFormatetEmployeeNr(String employeeNr)
        {
            String formatedEmployeeNr = FillWithBlanksEnd(MAX_EMPLOYEE_NR_LENGTH, employeeNr, true);
            return formatedEmployeeNr;
        }

        private String FillWithBlanksEnd(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize == originValue.Length)
                return originValue;

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
                return originValue.Substring(0, targetSize);
            else
                return originValue;
        }

        private String FillWithZerosEnd(int targetSize, string originValue, bool truncate = false)
        {
            if (originValue == null)
                originValue = "";

            if (targetSize == originValue.Length)
                return originValue;

            if (targetSize > originValue.Length)
            {
                string blanks = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    blanks += "0";
                }
                return (originValue + blanks);
            }
            else if (truncate)
                return originValue.Substring(0, targetSize);
            else
                return originValue;
        }
        private String FillWithZero(int targetSize, string originValue, bool truncate = false)
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

        private String GetAccountNr(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
        {
            if (!internalAccounts.IsNullOrEmpty())
                foreach (AccountInternal internalAccount in internalAccounts)
                    if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue && internalAccount.Account.AccountDim.SysSieDimNr.Value == (int)accountDim)
                        return internalAccount.Account.AccountNr;

            return "";
        }

        private List<AccountDim> GetAccountInternalsOnDim(TermGroup_SieAccountDim accountDim, List<AccountDim> accountDims)
        {
            return accountDims.Where(d => d.SysSieDimNr.HasValue && d.SysSieDimNr.Value == (int)accountDim).ToList();
        }

        public string GetTime4PositionsFromMinutes(decimal amount)
        {
            string value = string.Empty;
            decimal dec = 0;
            dec = Math.Round(amount / 60, 2, MidpointRounding.ToEven);
            value = dec.ToString().Replace(",", "");
            value = value.Replace(".", "");

            if (dec >= 10)
            {
                while (value.Length < 4)
                {
                    value = value + "0";
                }
            }
            else
            {
                if (value.Length < 3)
                {
                    while (value.Length < 3)
                    {
                        value = value + "0";
                    }
                }
            }

            while (value.Length < 4)
            {
                value = "0" + value;
            }

            return value;
        }

        private string GetTime4PositionFromScheduleMinutes(double totalMinutes, double totalBreakMinutes)
        {
            return GetTime4PositionsFromMinutes(Convert.ToDecimal(totalMinutes - totalBreakMinutes));
        }

        private string GetQuantityToString(decimal amount, bool abs)
        {
            if (amount < 0 && abs)
                amount = decimal.Multiply(amount, -1);

            string value = amount.ToString("0.00");
            value = value.Replace(",", "");
            value = value.Replace(".", "");
            return value;
        }

        #endregion
    }

    public enum BlueGardenCompany
    {
        Tele2 = 0,
        Coop = 1,
        ICA = 2,
    }

}
