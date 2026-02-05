using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SoftOne.Soe.Business.Util.IO
{
    public static class AxfoodExportItem
    {

        public static string storeId = "StoreId";

        public static List<Tuple<string, XDocument>> CreateDocuments(Dictionary<int, List<TimePayrollTransactionDTO>> timePayrollTransactionDTODict, Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> timeEmployeeScheduleDataSmallDTODict, List<Employee> employees, List<EmployeeAccount> employeeAccounts, List<PayrollProduct> payrollProducts, List<Account> accounts, List<AccountDim> accountDims, List<TimeDeviationCause> timeDeviationCauses, List<int> approvedAttestStates, bool split800, bool allowSplittingEDW = false)
        {

            var departmentAccountDim = accountDims.FirstOrDefault(w => w.Name.Equals("Avdelning", StringComparison.OrdinalIgnoreCase));
            var shiftTypeAccountDim = accountDims.FirstOrDefault(w => w.Name.Equals("PassTyp", StringComparison.OrdinalIgnoreCase));
            var storeAccountDim = accountDims.FirstOrDefault(w => w.Name.Equals("Butik", StringComparison.OrdinalIgnoreCase));

            int departmentAccountDimId = departmentAccountDim != null ? departmentAccountDim.AccountDimId : 0;
            int shiftTypeAccountDimId = shiftTypeAccountDim != null ? shiftTypeAccountDim.AccountDimId : 0;
            int storeAccountDimId = storeAccountDim != null ? storeAccountDim.AccountDimId : 0;
            List<Tuple<string, XDocument>> docs = new List<Tuple<string, XDocument>>();

            if (!allowSplittingEDW || timePayrollTransactionDTODict.Count() < 5000)
                docs.Add(Tuple.Create("EDW", CreateTransactionDocument(timePayrollTransactionDTODict, employees, employeeAccounts, payrollProducts, accounts, storeAccountDimId, departmentAccountDimId, shiftTypeAccountDimId, approvedAttestStates, adato: false, split800: split800)));
            else
            {
                var remainingEmployeeIds = employees.Select(e => e.EmployeeId).ToList();
                var employeeIds = remainingEmployeeIds;
                int batchSize = 5000;
                while (remainingEmployeeIds.Any())
                {
                    var employeeIdsInBatch = remainingEmployeeIds.Take(batchSize).ToList();
                    var employeesInBatch = employees.Where(employee => employeeIdsInBatch.Contains(employee.EmployeeId)).ToList();
                    docs.Add(Tuple.Create("EDW", CreateTransactionDocument(timePayrollTransactionDTODict, employeesInBatch, employeeAccounts, payrollProducts, accounts, storeAccountDimId, departmentAccountDimId, shiftTypeAccountDimId, approvedAttestStates, adato: false, split800: split800)));
                    remainingEmployeeIds = remainingEmployeeIds.Where(w => !employeeIdsInBatch.Contains(w)).ToList();
                }
            }

            if (!allowSplittingEDW || timeEmployeeScheduleDataSmallDTODict.Count() < 5000)
                docs.Add(Tuple.Create("EDW", CreateScheduleDocument(timeEmployeeScheduleDataSmallDTODict, employees, employeeAccounts, payrollProducts, accounts, timeDeviationCauses, storeAccountDimId, departmentAccountDimId, shiftTypeAccountDimId)));
            else
            {
                // take 5000 employees at a time
                var remainingEmployeeIds = employees.Select(e => e.EmployeeId).ToList();
                var employeeIds = remainingEmployeeIds;
                int batchSize = 5000;
                while(remainingEmployeeIds.Any())
                {
                    var employeeIdsInBatch = remainingEmployeeIds.Take(batchSize).ToList();
                    var employeesInBatch = employees.Where(employee => employeeIdsInBatch.Contains(employee.EmployeeId)).ToList();
                    docs.Add(Tuple.Create("EDW", CreateScheduleDocument(timeEmployeeScheduleDataSmallDTODict, employeesInBatch, employeeAccounts, payrollProducts, accounts, timeDeviationCauses, storeAccountDimId, departmentAccountDimId, shiftTypeAccountDimId)));
                    remainingEmployeeIds = remainingEmployeeIds.Where(w => !employeeIdsInBatch.Contains(w)).ToList();
                }
            }

            if (timePayrollTransactionDTODict.Count() < 20000)
                docs.Add(Tuple.Create("Adato", CreateTransactionDocument(timePayrollTransactionDTODict, employees, employeeAccounts, payrollProducts, accounts, storeAccountDimId, departmentAccountDimId, shiftTypeAccountDimId, approvedAttestStates, adato: true, split800: false)));
            else
            {
                var remainingEmployeeIds = employees.Select(e => e.EmployeeId).ToList();
                var employeeIds = remainingEmployeeIds;
                int batchSize = 5000;
                while (remainingEmployeeIds.Any())
                {
                    var employeeIdsInBatch = remainingEmployeeIds.Take(batchSize).ToList(); 
                    var employeesInBatch = employees.Where(employee => employeeIdsInBatch.Contains(employee.EmployeeId)).ToList();
                    docs.Add(Tuple.Create("Adato", CreateTransactionDocument(timePayrollTransactionDTODict, employeesInBatch, employeeAccounts, payrollProducts, accounts, storeAccountDimId, departmentAccountDimId, shiftTypeAccountDimId, approvedAttestStates, adato: true, split800: false)));
                    remainingEmployeeIds = remainingEmployeeIds.Where(w => !employeeIdsInBatch.Contains(w)).ToList();
                }
            }
                return docs;
        }

        public static string GetSchedule(Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> timeEmployeeScheduleDataSmallDTODict, List<Employee> employees, string externalExportId, bool ignoreEmploymentPercent = false)
        {
            var sb = new StringBuilder();
            //SplitSchduleItemsAccourdingToBreaks();

            foreach (var item in timeEmployeeScheduleDataSmallDTODict)
            {
                var employee = employees.FirstOrDefault(w => w.EmployeeId == item.Key);
                if (employee != null && !employee.Hidden && !employee.Vacant)
                {
                    foreach (var tempScheduleItemsForEmployee in item.Value.GroupBy(b => b.Date).OrderBy(o => o.Key))
                    {
                        var employment = employee.GetEmployment(tempScheduleItemsForEmployee.Key);
                        if (employment != null && (ignoreEmploymentPercent || Decimal.Round(employment.GetPercent(tempScheduleItemsForEmployee.Key), 0) == 1))
                        {
                            //Filen som skapas och skickas till oss den 13/12 kl 03:00 bör innehålla dagar med datum  2023-12-08 till 2023-12-12.
                            if (tempScheduleItemsForEmployee.Key < DateTime.Today.AddDays(-5) || tempScheduleItemsForEmployee.Key >= DateTime.Today)
                                continue;

                            //Företag	AN	5	1-5	Enligt Aditro L.
                            sb.Append(FillWithBlanksEnd(5, externalExportId));

                            //Anställningsnummer	AN	12	6-17	Enligt Aditro L.
                            sb.Append(FillWithBlanksEnd(12, employee.EmployeeNr));

                            //Datum	D	10	18-27	ÅÅÅÅ-MM-DD.
                            sb.Append(FillWithBlanksEnd(10, tempScheduleItemsForEmployee.Key.ToShortDateString()));

                            //Dagkod	AN	5	28-32	Dagkoden måste finnas upplagd i modul ‘Arbetstidschema’.
                            sb.Append(FillWithBlanksEnd(5, FillWithZeroBeginning(4, GetMinutesToHourString(Convert.ToDecimal(tempScheduleItemsForEmployee.Sum(s => s.IsBreak ? 0 : s.Quantity)))), true));

                            ////Schematyp	AN	5	33-37	Sätts till ‘OS’ om justering av dag i ordinarie arbetstidsschema är aktuellt.  Sätts till ‘PF’ om justerning av dag i partiella frånvaroschemat är aktuellt.
                            //sb.Append(FillWithBlanksEnd(5, ""));

                            sb.Append(Environment.NewLine);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public static string GetLasForOneEmploymentPercent(Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> timeEmployeeScheduleDataSmallDTODict, Dictionary<int, List<SubstituteShiftDTO>> substituteShiftsDict, DateTime fromDate, DateTime toDate, List<EmployeeDTO> employees, string externalExportId, bool ignoreEmploymentPercent = false, bool isInitRun = false, string extra = "19", string vik = "33")
        {
            var sb = new StringBuilder();
            var schedulesOnDates = timeEmployeeScheduleDataSmallDTODict.SelectMany(s => s.Value)
                .Where(w => w.Date >= fromDate.AddDays(-2) && w.Date <= toDate)
                .GroupBy(g => g.EmployeeId)
                .ToDictionary(k => k.Key, v => v.ToList());

            foreach (var employee in employees)
            {
                var employeeSchedules = schedulesOnDates.ContainsKey(employee.EmployeeId) ? schedulesOnDates[employee.EmployeeId] : new List<TimeEmployeeScheduleDataSmallDTO>();
                var substituteShifts = substituteShiftsDict.ContainsKey(employee.EmployeeId) ? substituteShiftsDict[employee.EmployeeId].Where(s => s.IsAssignedDueToAbsence).ToList() : new List<SubstituteShiftDTO>();
                TimeEmployeeScheduleDataSmallDTO scheduleDayBefore = null;

                foreach (var date in CalendarUtility.GetDates(fromDate, toDate))
                {
                    var employment = employee.GetEmployment(date);

                    if (employment != null && (ignoreEmploymentPercent || Decimal.Round(employment.Percent, 0) == 1))
                    {
                        #region onDate
                        var groupedSchedulesOnDate = employeeSchedules.Where(w => w.Date == date).GroupBy(g => g.TimeScheduleTemplateBlockId).Select(g =>
                        {
                            var oneSchedule = g.OrderByDescending(o => o.StartTime).First();
                            oneSchedule.StartTime = g.Min(x => x.StartTime);
                            oneSchedule.StopTime = g.Max(x => x.StopTime);
                            return oneSchedule;
                        }).ToList();

                        var schedulesOnDate = groupedSchedulesOnDate.OrderBy(s => substituteShifts.Where(y => y.Date == date).Any(x => x.StartTime == s.StartTime || x.StopTime == s.StopTime) ? 1 : 0).ThenBy(s => s.StartTime).ToList();
                        var scheduleOnDate = schedulesOnDate.FirstOrDefault();

                        #endregion

                        #region DayBefore

                        if (date == fromDate)
                        {
                            var groupedSchedulesDayBefore = employeeSchedules.Where(w => w.Date == date.AddDays(-1)).GroupBy(g => g.TimeScheduleTemplateBlockId).Select(g =>
                            {
                                var oneSchedule = g.OrderByDescending(o => o.StartTime).First();
                                oneSchedule.StartTime = g.Min(x => x.StartTime);
                                oneSchedule.StopTime = g.Max(x => x.StopTime);
                                return oneSchedule;
                            }).ToList();

                            var schedulesDayBefore = groupedSchedulesDayBefore.OrderBy(s => substituteShifts.Where(y => y.Date == date.AddDays(-1)).Any(x => x.StartTime == s.StartTime || x.StopTime == s.StopTime) ? 1 : 0).ThenBy(s => s.StartTime).ToList();
                            scheduleDayBefore = schedulesDayBefore.FirstOrDefault();
                        }

                        #endregion

                        var rows = GetRows(scheduleOnDate, isInitRun ? null : scheduleDayBefore, externalExportId, substituteShifts, date, employee, employeeSchedules, fromDate, date == fromDate, extra, vik);
                        foreach (var row in rows)
                        {
                            sb.AppendLine(row.Data);
                        }
                        scheduleDayBefore = scheduleOnDate;
                    }
                }
            }

            return sb.ToString();
        }

        private class RowData
        {
            public RowData(DateTime date, string data)
            {
                Date = date;
                Data = data;
            }
            public DateTime Date { get; set; }
            public string Data { get; set; }
        }

        private static List<RowData> GetRows(TimeEmployeeScheduleDataSmallDTO scheduleOnDate, TimeEmployeeScheduleDataSmallDTO scheduleDayBefore, string externalExportId, List<SubstituteShiftDTO> substituteShifts, DateTime date, EmployeeDTO employee, List<TimeEmployeeScheduleDataSmallDTO> employeeSchedules, DateTime fromDate, bool isFirstInterval, string extraOrNormal = "30", string vik = "33")
        {
            var rows = new List<RowData>();
            bool scheduleOnDateIsChangedWithin = scheduleOnDate != null ? (scheduleOnDate.LastChanged ?? DateTime.MinValue) >= fromDate : false;

            /*if (isFirstInterval && scheduleDayBefore != null && scheduleDayBefore.LastChanged > fromDate)
            {
                var scheduleDayBeforeBefore = employeeSchedules.FirstOrDefault(w => w.Date == scheduleDayBefore.Date.AddDays(-1));
                rows.AddRange(GetRows(scheduleDayBefore, scheduleDayBeforeBefore, externalExportId, substituteShifts, scheduleDayBefore.Date, employee, employeeSchedules, fromDate, isFirstInterval, extraOrNormal, vik));
            }*/

            if (scheduleOnDate == null || scheduleOnDate.StartTime == scheduleOnDate.StopTime)
            {
                // Om det inte finns något pass idag, skicka TOMT
                if (scheduleDayBefore != null)
                {
                    // Om det var ledigt igår också så skicka ingen rad
                    if (scheduleDayBefore != null && scheduleDayBefore.StartTime != scheduleDayBefore.StopTime)
                        rows.Add(new RowData(date, CreateRow(externalExportId, date, employee, "")));
                }
            }
            else
            {
                // Koden tittar först på om det är en ledig dag, och skickar den som ledig dag om dagen innan var något annat.
                //Alla pass som kommer från en frånvarande medarbetare ska vara vikariat
                //Alla andra pass oberoende x-pass bock eller inte ska tolkas som SÄVA / Temporärt behov
                //Finns det två olika anställningsformer samma dag ska det vara en Säva / Temporärt behovs - dag som skickas

                // if (substituteShifts.Any(w => w.IsExtraShift && w.Date == date))
                //     scheduleOnDate.ExtraShift = true;

                if (substituteShifts.Any(w => w.Date == date && (w.StartTime == scheduleOnDate.StartTime || w.StopTime == scheduleOnDate.StopTime)))
                {
                    // Om det var vik igår så skicka ingen rad
                    if (scheduleDayBefore != null && scheduleDayBefore.StartTime != scheduleDayBefore.StopTime && substituteShifts.Any(a => a.Date == scheduleDayBefore.Date) && !scheduleOnDateIsChangedWithin)
                        return new List<RowData>();

                    // Det är ett vikariat pass
                    rows.Add(new RowData(date, CreateRow(externalExportId, date, employee, vik)));
                }
                else if (scheduleOnDate.ExtraShift)
                {
                    scheduleOnDate.SetNormal();

                    // Om det var extra pass igår så skicka ingen rad
                    if (scheduleDayBefore != null && scheduleDayBefore.StartTime != scheduleDayBefore.StopTime && (scheduleDayBefore.ExtraShift || scheduleDayBefore.IsNormal()) && !scheduleOnDateIsChangedWithin)
                        return new List<RowData>();

                    // Det är ett nytt extra pass
                    rows.Add(new RowData(date, CreateRow(externalExportId, date, employee, extraOrNormal)));
                }
                else
                {
                    scheduleOnDate.SetNormal();

                    if (scheduleDayBefore != null && (scheduleDayBefore.ExtraShift || scheduleDayBefore.IsNormal()) && !scheduleOnDateIsChangedWithin)
                        return new List<RowData>();

                    // Det är ett vanligt pass men blir ändå ett extra pass
                    rows.Add(new RowData(date, CreateRow(externalExportId, date, employee, extraOrNormal)));
                }

            }

            return rows.OrderBy(o => o.Date).ToList();
        }

        private static string CreateRow(string externalExportId, DateTime date, EmployeeDTO employee, string anställningsform)
        {
            StringBuilder sb = new StringBuilder();
            // Företag 1 - 11
            sb.Append(FillWithBlanksEnd(11, externalExportId));
            // anställningsnummer 12 - 21
            sb.Append(FillWithBlanksEnd(10, employee.EmployeeNr));
            // Identitet på begreppet 22 - 61
            sb.Append(FillWithBlanksEnd(40, "PE02Anstform"));
            // PE02Anstform Begreppets värde 62 - 161
            sb.Append(FillWithBlanksEnd(100, anställningsform));
            // Datum  162 - 180
            sb.Append(FillWithBlanksEnd(19, date.ToString("yyyy-MM-dd")));
            // Lägg till Typ "A" på  position 240
            sb.Append(FillWithZeroBeginning(240 - sb.Length, "A").Replace("0", " "));
            sb.Append("A");
            return sb.ToString();
        }


        private static string GetMinutesToHourString(decimal minutes)
        {
            decimal inHours = minutes / 60;
            string value = inHours.ToString("0.00");
            value = value.Replace(",", "");
            value = value.Replace(".", "");
            return value;
        }

        private static string FillWithZeroBeginning(int targetSize, string originValue, bool truncate = false)
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

        private static string FillWithBlanksEnd(int targetSize, string originValue, bool truncate = false)
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

        private static XDocument CreateTransactionDocument(Dictionary<int, List<TimePayrollTransactionDTO>> timePayrollTransactionDTODict, List<Employee> employees, List<EmployeeAccount> employeeAccounts, List<PayrollProduct> payrollProducts, List<Account> accounts, int storeAccountDimId, int departmentAccountDimId, int shiftTypeAccountDimId, List<int> approvedAttestStates, bool adato = false, bool split800 = false)
        {
            var doc = new XElement("Transactions");
            int transXmlId = 1;
            int dayXmlId = 1;
            var presenseProductIds = payrollProducts.Where(w =>
            w.Number.Equals("070") ||
            w.Number.Equals("010") ||
            w.Number.Equals("300") ||
            w.Number.Equals("301") ||
            w.Number.Equals("302") ||
            w.Number.Equals("303")
            ).Select(s => s.ProductId).ToList();
            var obProductIds = payrollProducts.Where(w =>
            w.Number.Equals("340") ||
            w.Number.Equals("341") ||
            w.Number.Equals("342") ||
            w.Number.Equals("357") ||
            w.Number.Equals("806") ||
            w.Number.Equals("807") ||
            w.Number.Equals("808") ||
            w.Number.Equals("80980")).Select(s => s.ProductId).ToList();

            var adatoAddedAbsense = payrollProducts.Where(w => w.ExternalNumberOrNumber.Equals("F06")).Select(s => s.ProductId).ToList();

            var storeAccounts = accounts.Where(w => w.AccountDimId == storeAccountDimId).ToList();
            var departmentAccounts = accounts.Where(w => w.AccountDimId == departmentAccountDimId).ToList();
            var shiftTypeAccounts = accounts.Where(w => w.AccountDimId == shiftTypeAccountDimId).ToList();

            doc.Add(CreateAccountElement(storeAccounts, storeId));
            doc.Add(CreateAccountElement(departmentAccounts, "DepartmentId"));
            doc.Add(CreateAccountElement(shiftTypeAccounts, "ShiftType"));
            doc.Add(CreateProductElement(payrollProducts));
            int count = 0;
            foreach (var employee in employees)
            {
                List<TimePayrollTransactionDTO> trans;
                timePayrollTransactionDTODict.TryGetValue(employee.EmployeeId, out trans);

                if (adato && trans.IsNullOrEmpty())
                    continue;

                if (adato && !trans.Any(w => (w.IsAbsenceSick() || adatoAddedAbsense.Contains(w.PayrollProductId) || w.IsAbsence_SicknessSalary()) && !obProductIds.Contains(w.PayrollProductId)))
                    continue;

                XElement employeeElement = new XElement("Employee");
                employeeElement.Add(new XElement("EmployeeNr", employee.EmployeeNr));
                var ea = employeeAccounts.FirstOrDefault(w => w.EmployeeId == employee.EmployeeId);
                var storeAccountOnEmployee = ea != null && ea.Account != null && ea.Account.AccountInternal != null ? ea.Account.AccountInternal.ToDTO() : null;

                if (trans != null && trans.Any())
                {
                    count++;
                    var dates = trans.Select(s => s.Date).Distinct().OrderBy(o => o).ToList();

                    foreach (var date in dates)
                    {
                        if (!date.HasValue)
                            continue;

                        XElement dayElement = new XElement("Day",
                          new XAttribute("id", dayXmlId),
                          new XElement("Date", date));
                        dayXmlId++;

                        var transactionsOnDate = trans.Where(w => w.Date == date).ToList();
                        var presenceTransactions = adato ? new List<TimePayrollTransactionDTO>() : transactionsOnDate.Where(w => w.Quantity != 0 && presenseProductIds.Contains(w.PayrollProductId)).ToList();
                        var absenceTransactions = adato ? transactionsOnDate.Where(w => (w.IsAbsenceSick() || adatoAddedAbsense.Contains(w.PayrollProductId) || w.IsAbsence_SicknessSalary()) && !obProductIds.Contains(w.PayrollProductId)).ToList() : transactionsOnDate.Where(w => w.Quantity != 0 && w.IsAbsence()).ToList();
                        var obAdditionTransactions = adato ? new List<TimePayrollTransactionDTO>() : transactionsOnDate.Where(w => w.Quantity != 0 && obProductIds.Contains(w.PayrollProductId)).ToList();

                        foreach (var presenceTransaction in presenceTransactions)
                        {
                            var product = payrollProducts.FirstOrDefault(f => f.ProductId == presenceTransaction.PayrollProductId);
                            var storeAccount = presenceTransaction.AccountInternals.FirstOrDefault(f => f.AccountDimId == storeAccountDimId);

                            if (storeAccount == null)
                                storeAccount = storeAccountOnEmployee;

                            if (product == null || storeAccount == null)
                                continue;

                            var account = accounts.FirstOrDefault(f => f.AccountId == storeAccount.AccountId);

                            if (account == null)
                                continue;

                            var departmentAccount = presenceTransaction.AccountInternals.FirstOrDefault(f => f.AccountDimId == departmentAccountDimId);
                            var shiftTypeAccount = presenceTransaction.AccountInternals.FirstOrDefault(f => f.AccountDimId == shiftTypeAccountDimId);
                            var approved = approvedAttestStates.Any(a => a == presenceTransaction.AttestStateId);

                            XElement transactionElement = new XElement("Transaction",
                                     new XAttribute("id", transXmlId),
                                     new XElement("PaymentType", product.ExternalNumberOrNumber),
                                     new XElement("StoreId", account.AccountNr),
                                     new XElement("DepartmentId", departmentAccount != null ? departmentAccount.AccountNr : "0"),
                                     new XElement("ShiftType", shiftTypeAccount != null ? shiftTypeAccount.AccountNr : "0"),
                                     new XElement("Minutes", Convert.ToInt32(presenceTransaction.Quantity)),
                                     new XElement("Amount", presenceTransaction.Amount),
                                     new XElement("StartTime", CalendarUtility.MergeDateAndTime(date.Value, presenceTransaction.StartTime)),
                                     new XElement("StopTime", CalendarUtility.MergeDateAndTime(date.Value, presenceTransaction.StopTime)),
                                     new XElement("Approved", approved.ToInt()));
                            transXmlId++;

                            dayElement.Add(transactionElement);
                        }

                        if (obAdditionTransactions.Any())
                        {
                            var includedInObProducts = payrollProducts.Where(w => obAdditionTransactions.Select(s => s.PayrollProductId).ToList().Contains(w.ProductId)).ToList();

                            foreach (var obTransactionGroup in includedInObProducts.GroupBy(g => g.ExternalNumberOrNumber))
                            {
                                var productIdsInGroup = obTransactionGroup.Select(s => s.ProductId).ToList();
                                var transactions = obAdditionTransactions.Where(w => productIdsInGroup.Contains(w.PayrollProductId)).ToList();

                                foreach (var accountStringGroup in transactions.GroupBy(g => g.GetAccountingIdString() + "#" + g.AttestStateId.ToString()))
                                {
                                    var storeAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == storeAccountDimId);

                                    if (storeAccount == null)
                                        storeAccount = storeAccountOnEmployee;

                                    if (storeAccount == null)
                                        continue;

                                    var account = accounts.FirstOrDefault(f => f.AccountId == storeAccount.AccountId);

                                    if (account == null)
                                        continue;

                                    var approved = approvedAttestStates.Any(a => a == accountStringGroup.First().AttestStateId);

                                    var departmentAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == departmentAccountDimId);
                                    var shiftTypeAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == shiftTypeAccountDimId);

                                    XElement obTransactionElement = new XElement("OBTransaction",
                                             new XAttribute("id", transXmlId),
                                             new XElement("PaymentType", obTransactionGroup.Key),
                                             new XElement("StoreId", account.AccountNr),
                                             new XElement("DepartmentId", departmentAccount != null ? departmentAccount.AccountNr : "0"),
                                             new XElement("ShiftType", shiftTypeAccount != null ? shiftTypeAccount.AccountNr : "0"),
                                             new XElement("Amount", accountStringGroup.Sum(s => s.Amount)),
                                             new XElement("Minutes", Convert.ToInt32(accountStringGroup.Sum(s => s.Quantity))),
                                             new XElement("Approved", approved.ToInt()));

                                    transXmlId++;

                                    dayElement.Add(obTransactionElement);
                                }
                            }
                        }

                        if (absenceTransactions.Any())
                        {
                            var includedInAbsenceProducts = payrollProducts.Where(w => absenceTransactions.Select(s => s.PayrollProductId).ToList().Contains(w.ProductId)).ToList();

                            foreach (var obTransactionGroup in includedInAbsenceProducts.GroupBy(g => g.GetProductNumberForAxfood(split800, adato)))
                            {
                                var productIdsInGroup = obTransactionGroup.Select(s => s.ProductId).ToList();
                                var transactions = absenceTransactions.Where(w => productIdsInGroup.Contains(w.PayrollProductId)).ToList();

                                foreach (var accountStringGroup in transactions.GroupBy(g => g.GetAccountingIdString() + "#" + g.AttestStateId.ToString()))
                                {
                                    var storeAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == storeAccountDimId);

                                    if (storeAccount == null)
                                        storeAccount = storeAccountOnEmployee;

                                    if (storeAccount == null)
                                        continue;

                                    var account = accounts.FirstOrDefault(f => f.AccountId == storeAccount.AccountId);

                                    if (account == null)
                                        continue;

                                    var approved = approvedAttestStates.Any(a => a == accountStringGroup.First().AttestStateId);

                                    var departmentAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == departmentAccountDimId);
                                    var shiftTypeAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == shiftTypeAccountDimId);

                                    XElement absenceTransactionElement = new XElement("AbTransaction",
                                             new XAttribute("id", transXmlId),
                                             new XElement("PaymentType", obTransactionGroup.Key),
                                             new XElement("StoreId", account.AccountNr),
                                             new XElement("DepartmentId", departmentAccount != null ? departmentAccount.AccountNr : "0"),
                                             new XElement("ShiftType", shiftTypeAccount != null ? shiftTypeAccount.AccountNr : "0"),
                                             new XElement("Minutes", Convert.ToInt32(accountStringGroup.Sum(s => s.Quantity))),
                                             new XElement("Approved", approved.ToInt()));
                                    transXmlId++;

                                    dayElement.Add(absenceTransactionElement);
                                }
                            }
                        }

                        if (presenceTransactions.Any() || obAdditionTransactions.Any() || absenceTransactions.Any())
                            employeeElement.Add(dayElement);
                    }
                    doc.Add(employeeElement);
                }
            }

            if (adato && count == 0)
                return null;

            XDocument document = new XDocument();
            document.Add(doc);

            string validate = ValidateXDocument(document, false);

            if (!string.IsNullOrEmpty(validate))
                SysLogConnector.LogErrorString("Axfood Transaction xml failed to validate " + validate);

            return document;
        }

        private static XDocument CreateScheduleDocument(Dictionary<int, List<TimeEmployeeScheduleDataSmallDTO>> timeEmployeeScheduleDataSmallDTODict, List<Employee> employees, List<EmployeeAccount> employeeAccounts, List<PayrollProduct> payrollProducts, List<Account> accounts, List<TimeDeviationCause> timeDeviationCauses, int storeAccountDimId, int departmentAccountDimId, int shiftTypeAccountDimId)
        {
            var doc = new XElement("Schedule");
            bool addAbsence = false;
            int transXmlId = 1;
            int dayXmlId = 1;
            var presenseProductIds = payrollProducts.Where(w => w.Number.Equals("070") || w.Number.Equals("010")).Select(s => s.ProductId).ToList();
            var obProducts = payrollProducts.Where(w => w.Number.Equals("340") || w.Number.Equals("341") || w.Number.Equals("342")).ToList();
            List<Tuple<int, string>> timeDeviationCauseProductNumberMapping = new List<Tuple<int, string>>();
            var storeAccounts = accounts.Where(w => w.AccountDimId == storeAccountDimId).ToList();
            var departmentAccounts = accounts.Where(w => w.AccountDimId == departmentAccountDimId).ToList();
            var shiftTypeAccounts = accounts.Where(w => w.AccountDimId == shiftTypeAccountDimId).ToList();

            doc.Add(CreateAccountElement(storeAccounts, storeId));
            doc.Add(CreateAccountElement(departmentAccounts, "DepartmentId"));
            doc.Add(CreateAccountElement(shiftTypeAccounts, "ShiftType"));
            doc.Add(CreateProductElement(payrollProducts));

            foreach (var deviation in timeDeviationCauses)
            {
                if (deviation.TimeCode == null)
                    continue;

                if (!deviation.TimeCode.TimeCodePayrollProduct.IsLoaded)
                    deviation.TimeCode.TimeCodePayrollProduct.Load();

                foreach (var item in deviation.TimeCode.TimeCodePayrollProduct)
                {
                    var product = payrollProducts.FirstOrDefault(w => w.ProductId == item.ProductId);

                    if (product != null && !timeDeviationCauseProductNumberMapping.Any(a => a.Item1 == deviation.TimeDeviationCauseId))
                        timeDeviationCauseProductNumberMapping.Add(Tuple.Create(deviation.TimeDeviationCauseId, product.ExternalNumberOrNumber));
                }
            }

            foreach (var employee in employees)
            {
                List<TimeEmployeeScheduleDataSmallDTO> trans;
                timeEmployeeScheduleDataSmallDTODict.TryGetValue(employee.EmployeeId, out trans);
                XElement employeeElement = new XElement("Employee");
                employeeElement.Add(new XElement("EmployeeNr", employee.EmployeeNr));
                var ea = employeeAccounts.FirstOrDefault(w => w.EmployeeId == employee.EmployeeId);
                var storeAccountOnEmployee = ea != null && ea.Account != null && ea.Account.AccountInternal != null ? ea.Account.AccountInternal.ToDTO() : null;

                if (trans != null && trans.Any())
                {
                    var dates = trans.Select(s => s.Date).Distinct().OrderBy(o => o).ToList();

                    foreach (var date in dates)
                    {
                        XElement dayElement = new XElement("Day",
                          new XAttribute("id", dayXmlId),
                          new XElement("Date", date));
                        dayXmlId++;

                        var transactionsOnDate = trans.Where(w => w.Date == date);
                        var presenceTransactions = transactionsOnDate.Where(w => w.Quantity != 0 && !w.TimeDeviationCauseId.HasValue);
                        var absenceTransactions = transactionsOnDate.Where(w => w.Quantity != 0 && w.TimeDeviationCauseId.HasValue);
                        var obAdditionTransactions = transactionsOnDate.Where(w => w.GrossTimeRules != null);

                        foreach (var groupOnLinkIfHidden in presenceTransactions.GroupBy(g => employee.Hidden ? g.Link : ""))
                        {
                            foreach (var presenceTransaction in groupOnLinkIfHidden.OrderBy(o => o.StartTime))
                            {
                                //var product = payrollProducts.FirstOrDefault(f => f.ProductId == presenceTransaction.PayrollProductId);
                                var storeAccount = presenceTransaction.AccountInternals.FirstOrDefault(f => f.AccountDimId == storeAccountDimId);

                                if (storeAccount == null)
                                    storeAccount = storeAccountOnEmployee;

                                if (storeAccount == null)
                                    continue;

                                var account = accounts.FirstOrDefault(f => f.AccountId == storeAccount.AccountId);

                                if (account == null)
                                    continue;

                                var departmentAccount = presenceTransaction.AccountInternals.FirstOrDefault(f => f.AccountDimId == departmentAccountDimId);
                                var shiftTypeAccount = presenceTransaction.AccountInternals.FirstOrDefault(f => f.AccountDimId == shiftTypeAccountDimId);

                                XElement transactionElement = new XElement("Transaction",
                                         new XAttribute("id", transXmlId),
                                         new XElement("PaymentType", "010"),
                                         new XElement("StoreId", account.AccountNr),
                                         new XElement("DepartmentId", departmentAccount != null ? departmentAccount.AccountNr : "0"),
                                         new XElement("ShiftType", shiftTypeAccount != null ? shiftTypeAccount.AccountNr : "0"),
                                         new XElement("Minutes", Convert.ToInt32(presenceTransaction.Quantity)),
                                         new XElement("StartTime", CalendarUtility.MergeDateAndTime(date, presenceTransaction.StartTime)),
                                         new XElement("StopTime", CalendarUtility.MergeDateAndTime(date, presenceTransaction.StopTime)),
                                         new XElement("Amount", presenceTransaction.Amount),
                                         new XElement("GrossAmount", presenceTransaction.GrossAmount));

                                transXmlId++;

                                dayElement.Add(transactionElement);
                            }
                        }
                        if (obAdditionTransactions.Any())
                        {
                            foreach (var obTransactionAccountingGroup in obAdditionTransactions.GroupBy(g => g.GetAccountingIdString()))
                            {
                                foreach (var transaction in obTransactionAccountingGroup)
                                {
                                    foreach (var transactionGrossTimeRule in transaction.GrossTimeRules)
                                    {

                                        var storeAccount = obTransactionAccountingGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == storeAccountDimId);

                                        if (storeAccount == null)
                                            storeAccount = storeAccountOnEmployee;

                                        if (storeAccount == null)
                                            continue;

                                        var account = accounts.FirstOrDefault(f => f.AccountId == storeAccount.AccountId);

                                        if (account == null)
                                            continue;

                                        var departmentAccount = obTransactionAccountingGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == departmentAccountDimId);
                                        var shiftTypeAccount = obTransactionAccountingGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == shiftTypeAccountDimId);

                                        var cals = transaction.GrossTimeCalcs.Where(f => f.Rule.TimeRuleId == transactionGrossTimeRule.TimeRuleId);

                                        var overlappingMinutes = cals?.Sum(s => s.AddedOverlappingMinutes) ?? 0;
                                        var addedGrossAmount = cals?.Sum(s => s.AddedGrossAmount) ?? 0;

                                        XElement obTransactionElement = new XElement("OBTransaction",
                                                 new XAttribute("id", transXmlId),
                                                 new XElement("PaymentType", transactionGrossTimeRule.ExternalCode),
                                                 new XElement("StoreId", account.AccountNr),
                                                 new XElement("DepartmentId", departmentAccount != null ? departmentAccount.AccountNr : "0"),
                                                 new XElement("ShiftType", shiftTypeAccount != null ? shiftTypeAccount.AccountNr : "0"),
                                                 new XElement("Minutes", overlappingMinutes),
                                                 new XElement("Amount", addedGrossAmount));

                                        transXmlId++;

                                        dayElement.Add(obTransactionElement);
                                    }
                                }
                            }
                        }

                        if (addAbsence && absenceTransactions.Any() && timeDeviationCauseProductNumberMapping != null)
                        {
                            foreach (var accountStringGroup in absenceTransactions.GroupBy(g => g.GetAccountingIdString()))
                            {
                                if (!accountStringGroup.First().TimeDeviationCauseId.HasValue)
                                    continue;

                                var storeAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == storeAccountDimId);

                                if (storeAccount == null)
                                    storeAccount = storeAccountOnEmployee;

                                if (storeAccount == null)
                                    continue;

                                var account = accounts.FirstOrDefault(f => f.AccountId == storeAccount.AccountId);

                                if (account == null)
                                    continue;

                                var code = timeDeviationCauseProductNumberMapping.FirstOrDefault(f => f.Item1 == accountStringGroup.First().TimeDeviationCauseId.Value) != null ? timeDeviationCauseProductNumberMapping.FirstOrDefault(f => f.Item1 == accountStringGroup.First().TimeDeviationCauseId.Value).Item2 : string.Empty;

                                var departmentAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == departmentAccountDimId);
                                var shiftTypeAccount = accountStringGroup.First().AccountInternals.FirstOrDefault(f => f.AccountDimId == shiftTypeAccountDimId);

                                XElement absenceTransactionElement = new XElement("AbTransaction",
                                         new XAttribute("id", transXmlId),
                                         new XElement("PaymentType", code),
                                         new XElement("StoreId", account.AccountNr),
                                         new XElement("DepartmentId", departmentAccount != null ? departmentAccount.AccountNr : "0"),
                                         new XElement("ShiftType", shiftTypeAccount != null ? shiftTypeAccount.AccountNr : "0"),
                                         new XElement("Minutes", Convert.ToInt32(accountStringGroup.Sum(s => s.Quantity))));

                                transXmlId++;

                                dayElement.Add(absenceTransactionElement);
                            }
                        }

                        if (presenceTransactions.Any() || obAdditionTransactions.Any() || (addAbsence && absenceTransactions.Any()))
                            employeeElement.Add(dayElement);
                    }

                    doc.Add(employeeElement);
                }
            }

            XDocument document = new XDocument();
            document.Add(doc);

            string validate = ValidateXDocument(document, true);

            if (!string.IsNullOrEmpty(validate))
                SysLogConnector.LogErrorString("Axfood Schedule xml failed to validate " + validate);

            return document;
        }

        private static List<XElement> CreateAccountElement(List<Account> accounts, string elementName)
        {
            List<XElement> elements = new List<XElement>();

            foreach (var account in accounts)
            {
                var element = new XElement(elementName,
                      new XElement("Nr", account.AccountNr),
                      new XElement("Name", account.Name));

                if (elementName == storeId)
                    element.Add(new XElement("External", 0));

                elements.Add(element);

            }

            return elements;
        }

        private static List<XElement> CreateProductElement(List<PayrollProduct> products)
        {
            List<XElement> elements = new List<XElement>();

            foreach (var product in products)
            {
                elements.Add(new XElement("PaymentType",
                    new XElement("ProductNr", product.Number),
                    new XElement("ProductName", product.Name),
                    new XElement("Factor", product.Factor)));
            }

            return elements;
        }

        private static string ValidateXDocument(XDocument document, bool isSchedule)
        {
            string value = string.Empty;
            bool errors = false;
            string message = "";
            string path = isSchedule ? ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_AXFOOD_PHYSICAL + "Schedule.xsd" : ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_AXFOOD_PHYSICAL + "Transactions.xsd";
            string file = File.ReadAllText(path);

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(file)));
            document.Validate(schemas, (o, e) =>
            {
                message = e.Message;
                errors = true;
            });

            if (errors)
            {
                return message;
            }
            return "";
        }
    }
    public static class AxfoodExtensions
    {
        public static string GetProductNumberForAxfood(this PayrollProduct product, bool split800, bool adato)
        {
            var code = product.ExternalNumberOrNumber;

            if (split800 && code == "800")
                return product.Number;

            if (!adato && code == "F06")
                return "800";

            return code;
        }

        public static void SetNormal(this TimeEmployeeScheduleDataSmallDTO dto)
        {
            dto.Description = "Normal";
        }

        public static bool IsNormal(this TimeEmployeeScheduleDataSmallDTO dto)
        {
            return dto.Description == "Normal";
        }
    }



}
