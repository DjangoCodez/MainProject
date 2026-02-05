using SoftOne.Soe.Business.Util.ImportSpecials.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class SoftOneLonClassic : IPayrollImportable
    {
        public List<string> GetEmployeeNrs(byte[] file)
        {
            GetInformation(out List<Transaction> transactions, out List<ScheduleDefinition> definitions, out List<Schedule> schedules, file, null);

            var employeeNrs = transactions.Select(s => s.EmployeeNr).ToList();
            employeeNrs.AddRange(schedules.Select(s => s.EmployeeNr));
            return employeeNrs.Distinct().ToList();
        }

        private void GetInformation(out List<Transaction> transactions, out List<ScheduleDefinition> definitions, out List<Schedule> schedules, byte[] file, List<AccountDimDTO> accountDims)
        {
            using (MemoryStream ms = new MemoryStream(file))
            {
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms, Encoding.Default))
                {
                    string line = reader.ReadLine();

                    List<string> scheduleDefinitionLines = new List<string>();
                    List<string> transactionLines = new List<string>();
                    List<string> scheduleLines = new List<string>();

                    while (line != null)
                    {
                        line = line.Trim();
                        if (line != String.Empty)
                        {
                            if (line.StartsWith("//"))
                            {
                                line = reader.ReadLine();
                                continue;
                            }
                            else if (line.StartsWith("@"))
                            {
                                scheduleDefinitionLines.Add(line);
                            }
                            else if (line.Contains("@"))
                            {
                                scheduleLines.Add(line);
                            }
                            else
                            {
                                transactionLines.Add(line);
                            }
                        }
                        line = reader.ReadLine();
                    }

                    transactions = GetTransactions(transactionLines, accountDims);
                    definitions = GetScheduleDefinitions(scheduleDefinitionLines);
                    schedules = GetSchedules(scheduleLines, definitions);
                }
            }
        }


        public PayrollImportHeadDTO ParseToPayrollImportHead(int actorCompanyId, byte[] file, DateTime? paymentDate, List<EmployeeDTO> employees, List<AccountDimDTO> accountDims, List<PayrollProductGridDTO> payrollProducts, List<TimeDeviationCauseDTO> timeDeviationCauses)
        {
            PayrollImportHeadDTO payrollImportHead = new PayrollImportHeadDTO();

            payrollImportHead.ActorCompanyId = actorCompanyId;
            payrollImportHead.File = file;
            payrollImportHead.PaymentDate = paymentDate;

            GetInformation(out List<Transaction> transactions, out List<ScheduleDefinition> definitions, out List<Schedule> schedules, file, accountDims);

            var employeeNrs = transactions.Select(s => s.EmployeeNr).ToList();
            employeeNrs.AddRange(schedules.Select(s => s.EmployeeNr));
            employeeNrs = employeeNrs.Distinct().ToList();

            foreach (var employeeNr in employeeNrs)
            {
                var employee = employees.FirstOrDefault(f => f.EmployeeNr == employeeNr);

                if (employee == null)
                    continue;

                PayrollImportEmployeeDTO payrollImportEmployee = new PayrollImportEmployeeDTO()
                {
                    EmployeeId = employee.EmployeeId
                };

                var schedulesForEmployee = schedules.Where(w => w.EmployeeNr == employee.EmployeeNr);

                if (!schedulesForEmployee.IsNullOrEmpty())
                {
                    payrollImportEmployee.Schedule = new List<PayrollImportEmployeeScheduleDTO>();

                    foreach (var schedule in schedulesForEmployee)
                    {
                        var payrollImportEmployeeSchedule = new PayrollImportEmployeeScheduleDTO()
                        {
                            Date = schedule.date,
                            Quantity = schedule.Definition.Hours * 60,
                            StartTime = schedule.Definition.Start,
                            StopTime = schedule.Definition.Stop,
                        };

                        payrollImportEmployee.Schedule.Add(payrollImportEmployeeSchedule);
                        var breakTime = Convert.ToDecimal((payrollImportEmployeeSchedule.StopTime - payrollImportEmployeeSchedule.StartTime).TotalMinutes) - payrollImportEmployeeSchedule.Quantity;

                        if (breakTime > 0)
                        {
                            var payrollImportEmployeeScheduleBreak = new PayrollImportEmployeeScheduleDTO()
                            {
                                Date = schedule.date,
                                Quantity = breakTime,
                                StartTime = CalendarUtility.AdjustAccordingToInterval(CalendarUtility.GetMiddleTime(schedule.Definition.Start, schedule.Definition.Stop).AddMinutes(-Convert.ToInt32(decimal.Divide(breakTime, 2))), Convert.ToInt32(breakTime), 15),
                                IsBreak = true,
                            };

                            payrollImportEmployeeScheduleBreak.StopTime = payrollImportEmployeeScheduleBreak.StartTime.AddMinutes(Convert.ToInt32(breakTime));
                            payrollImportEmployee.Schedule.Add(payrollImportEmployeeScheduleBreak);
                        }
                    }
                }

                var transactionsForEmployee = transactions.Where(w => w.EmployeeNr == employee.EmployeeNr);

                if (!transactionsForEmployee.IsNullOrEmpty())
                {
                    payrollImportEmployee.Transactions = new List<PayrollImportEmployeeTransactionDTO>();

                    foreach (var trans in transactionsForEmployee)
                    {
                        PayrollImportEmployeeTransactionDTO transaction = new PayrollImportEmployeeTransactionDTO();
                        TermGroup_PayrollResultType payrollResultType = TermGroup_PayrollResultType.Time;
                        if (!string.IsNullOrEmpty(trans.PayrollProductNr))
                        {
                            var deviation = timeDeviationCauses.FirstOrDefault(w => !string.IsNullOrEmpty(w.ExtCode) && w.ExtCode.ToLower() == trans.PayrollProductNr.ToLower());

                            if (deviation == null)
                            {
                                foreach (var item in timeDeviationCauses.Where(w => !string.IsNullOrEmpty(w.ExtCode) && w.ExtCode.Contains("#")))
                                {
                                    var match = item.ExternalCodes.FirstOrDefault(w => !string.IsNullOrEmpty(w) && w.ToLower() == trans.PayrollProductNr.ToLower());

                                    if (!string.IsNullOrWhiteSpace(match))
                                    {
                                        deviation = item;
                                        break;
                                    }
                                }
                            }

                            if (deviation != null)
                            {
                                transaction.TimeDeviationCauseId = deviation.TimeDeviationCauseId;
                                transaction.Type = TermGroup_PayrollImportEmployeeTransactionType.DeviationCause;
                            }

                            var payrollProduct = payrollProducts.FirstOrDefault(f => f.ExternalNumber == trans.PayrollProductNr);

                            if (payrollProduct == null)
                                payrollProduct = payrollProducts.FirstOrDefault(f => f.Number == trans.PayrollProductNr);

                            if (payrollProduct != null)
                            {
                                transaction.PayrollProductId = payrollProduct.ProductId;
                                transaction.Type = TermGroup_PayrollImportEmployeeTransactionType.PayrollProduct;
                                payrollResultType = payrollProduct.ResultType;
                            }

                            if (deviation != null && payrollProduct != null)
                            {
                                payrollImportHead.ErrorMessage = trans.PayrollProductNr + " Exists as both payrollproduct and timedeviationcause, adjust in file and try again";
                            }

                        }

                        transaction.Date = trans.DateFrom;
                        transaction.Quantity = payrollResultType == TermGroup_PayrollResultType.Time || transaction.TimeDeviationCauseId.HasValue ? decimal.Multiply(trans.Quantity, 60) : trans.Quantity;
                        transaction.Amount = trans.UnitPrice;
                        transaction.Code = trans.PayrollProductNr;
                        transaction.AccountCode = trans.AccountStd?.AccountNr;
                        transaction.AccountStdId = trans.AccountStd?.AccountId;

                        foreach (var acc in trans.AccountInternals)
                        {
                            var dim = accountDims.FirstOrDefault(f => f.AccountDimId == acc.AccountDimId);
                            transaction.AccountInternals.Add(new PayrollImportEmployeeTransactionAccountInternalDTO()
                            {
                                AccountSIEDimNr = dim?.SysSieDimNr ?? 0,
                                AccountCode = acc.AccountNr,
                                AccountId = acc.AccountId
                            });
                            //TODO InternalAccounts
                        }
                        payrollImportEmployee.Transactions.Add(transaction);

                    }
                }
                payrollImportHead.Employees.Add(payrollImportEmployee);
            }

            var dateFrom = payrollImportHead.Employees.Where(w => w.Schedule != null).SelectMany(s => s.Schedule?.OrderBy(o => o.Date))?.Select(s => s.Date).OrderBy(s => s)?.FirstOrDefault(f => f != null).Date;
            if (!dateFrom.HasValue)
                dateFrom = payrollImportHead.Employees.Where(w => w.Transactions != null).SelectMany(s => s.Transactions?.OrderBy(o => o.Date))?.FirstOrDefault(f => f != null)?.Date;

            var dateTo = payrollImportHead.Employees.Where(w => w.Schedule != null).SelectMany(s => s.Schedule?.OrderByDescending(o => o.Date))?.Select(s => s.Date).OrderByDescending(s => s)?.FirstOrDefault(f => f != null).Date;
            if (!dateTo.HasValue)
                dateTo = payrollImportHead.Employees.Where(w => w.Transactions != null).SelectMany(s => s.Transactions?.OrderByDescending(o => o.Date))?.FirstOrDefault(f => f != null)?.Date;


            payrollImportHead.DateFrom = dateFrom.HasValue ? dateFrom.Value : DateTime.Today;
            payrollImportHead.DateTo = dateTo.HasValue ? dateTo.Value : DateTime.Today;

            return payrollImportHead;
        }

        public List<MassRegistrationTemplateRowDTO> ConvertMassRegistrationSoftOneClassicFileToDTOs(List<AccountDimDTO> accountDims, List<EmployeeDTO> employees, List<PayrollProductGridDTO> payrollProducts, MemoryStream ms, DateTime? paymentDate)
        {
            List<MassRegistrationTemplateRowDTO> dtos = new List<MassRegistrationTemplateRowDTO>();

            #region Prereq

            ms.Position = 0;
            StreamReader reader = new StreamReader(ms, Encoding.Default);

            #endregion

            string line = reader.ReadLine();

            List<string> scheduleDefinitionLines = new List<string>();
            List<string> transactionLines = new List<string>();
            List<string> scheduleLines = new List<string>();

            while (line != null)
            {
                line = line.Trim();
                if (line != String.Empty)
                {
                    if (line.StartsWith("//"))
                    {

                        line = reader.ReadLine();
                        continue;
                    }

                    if (line.StartsWith("@"))
                    {
                        scheduleDefinitionLines.Add(line);
                    }
                    else if (line.Contains("@"))
                    {
                        scheduleLines.Add(line);
                    }
                    else
                    {
                        transactionLines.Add(line);
                    }
                }

                line = reader.ReadLine();
            }

            var transactions = GetTransactions(transactionLines, accountDims);

            if (!transactions.IsNullOrEmpty())
            {
                foreach (var transaction in transactions)
                {

                    #region Data

                    // Create DTO and set data
                    MassRegistrationTemplateRowDTO dto = new MassRegistrationTemplateRowDTO();

                    if (!string.IsNullOrEmpty(transaction.EmployeeNr))
                    {
                        var employee = employees.FirstOrDefault(f => f.EmployeeNr == transaction.EmployeeNr);
                        if (employee != null)
                        {
                            dto.EmployeeId = employee.EmployeeId;
                            dto.EmployeeNr = employee.EmployeeNr;
                            dto.EmployeeName = employee.Name;
                        }
                    }

                    dto.PaymentDate = paymentDate;

                    if (!string.IsNullOrEmpty(transaction.PayrollProductNr))
                    {
                        var payrollProduct = payrollProducts.FirstOrDefault(f => f.ExternalNumber == transaction.PayrollProductNr);

                        if (payrollProduct == null)
                            payrollProduct = payrollProducts.FirstOrDefault(f => f.Number == transaction.PayrollProductNr);

                        if (payrollProduct != null)
                        {
                            dto.ProductId = payrollProduct.ProductId;
                            dto.ProductNr = payrollProduct.Number;
                            dto.ProductName = payrollProduct.Name;
                        }
                    }

                    dto.Quantity = transaction.Quantity;
                    dto.DateFrom = transaction.DateFrom;
                    dto.DateTo = transaction.DateTo;
                    dto.UnitPrice = transaction.UnitPrice;

                    //TODO Accounts
                    dtos.Add(dto);

                    #endregion
                }
            }


            line = reader.ReadLine();


            return dtos;
        }

        private List<ScheduleDefinition> GetScheduleDefinitions(List<string> scheduleDefinitionLines)
        {

            //Definition av en schematransaktion
            //En schematransaktion består av ett antal fält på följande vis:
            //
            //Fältnamn              Kommentar       Exempel
            //Anställningsnummer    Obligatoriskt   1001
            //Dagkod                Obligatoriskt   DK
            //Identifierare         Alltid @        @
            //Datum                 Obligatoriskt   2003-01-16

            List<ScheduleDefinition> definitions = new List<ScheduleDefinition>();

            foreach (var l in scheduleDefinitionLines)
            {
                var line = l.Replace("@,", "").Trim();
                if (line != String.Empty)
                {
                    string[] row = line.Split(',');

                    if (row.Count() < 4)
                        continue;

                    string keyStr = row.Length > 0 ? row[0] : String.Empty;
                    string startStr = row.Length > 1 ? row[1] : String.Empty;
                    string stopStr = row.Length > 2 ? row[2] : String.Empty;
                    string hoursStr = row.Length > 3 ? row[3] : String.Empty;

                    ScheduleDefinition scheduleDefinition = new ScheduleDefinition()
                    {
                        Key = keyStr,
                        Start = GetTime(startStr),
                        Stop = GetTime(stopStr),
                        Hours = decimal.Parse(hoursStr.Split('.')[0]) + decimal.Divide(decimal.Parse(hoursStr.Split('.')[1]), 60)
                    };

                    definitions.Add(scheduleDefinition);
                }
            }

            return definitions;
        }

        private List<Transaction> GetTransactions(List<string> transactionLines, List<AccountDimDTO> accountDims)
        {
            //Definition av tidstransaktion
            //En tidstransaktion består av ett antal fält på följande vis:
            //
            //Fältnamn              Kommentar                   Exempel
            //Anställningsnummer    Obligatoriskt               1001
            //Avdelning             Option                      Marknad
            //Löneart/Tidkod        Obligatoriskt               110
            //Antal/Timmar          Obligatoriskt               12.45
            //From Datum            Option                      20040101
            //Tom Datum             Option                      20040110
            //Kalenderdagar         Option                      10
            //Arbetsdagar           Option                      7
            //Pris                  Option                      134.90
            //KSK                   Option                      7210; 10; 20; 30
            //Behandlingsregel      Option                      1
            //Omfattning/Scope      Option                      80


            List<Transaction> transactions = new List<Transaction>();

            foreach (var l in transactionLines)
            {
                var line = l.Trim();
                if (line != String.Empty)
                {
                    // Inside transaction section

                    #region Extract data

                    // Read line data into strings
                    string[] row = line.Split(',');


                    if (row.Count() < 4)
                        continue;

                    string employeeNr = row.Length > 0 ? row[0] : String.Empty;
                    string department = row.Length > 1 ? row[1] : String.Empty;
                    string payrollProductNr = row.Length > 2 ? row[2] : String.Empty;
                    string quantityStr = row.Length > 3 ? row[3] : String.Empty;
                    string dateFromStr = row.Length > 4 ? row[4] : String.Empty;
                    string dateToStr = row.Length > 5 ? row[5] : String.Empty;
                    string calendarDaysStr = row.Length > 6 ? row[6] : String.Empty;
                    string workDaysStr = row.Length > 7 ? row[7] : String.Empty;
                    string unitPriceStr = row.Length > 8 ? row[8] : String.Empty;
                    string accountStr = row.Length > 9 ? row[9] : String.Empty;
                    string processingRuleStr = row.Length > 10 ? row[10] : String.Empty;

                    //Behandlingsregel
                    //Om behandlingsregel utelämnas används det förvalda alternativet att
                    //SoftOne skall prissätta och kontera den befintliga
                    //tidstransaktionen, dvs alternativ 0.Följande varianter finns:
                    //0 Både prissättning och kontering av systemet
                    //1 Endast kontering av systemet
                    //2 Endast prissättning av systemet
                    //3 Ingen prissättning och kontering av systemet(Transen styr)

                    string scopeStr = row.Length > 11 ? row[11] : String.Empty;

                    // Accounts are semicolon separated inside its own placeholder
                    string[] accounts = !string.IsNullOrEmpty(accountStr) ? accountStr.Split(';') : new string[0];

                    #endregion

                    #region Data

                    // Create DTO and set data
                    Transaction transaction = new Transaction()
                    {
                        EmployeeNr = employeeNr,
                        Department = department,
                        PayrollProductNr = payrollProductNr,
                        Quantity = NumberUtility.ToDecimal(quantityStr),
                        DateFrom = CalendarUtility.GetDateTime(dateFromStr, "yyyyMMdd"),
                        DateTo = CalendarUtility.GetDateTime(dateToStr, "yyyyMMdd"),
                        CalendarDays = NumberUtility.ToInteger(calendarDaysStr),
                        WorkDays = NumberUtility.ToInteger(workDaysStr),
                        UnitPrice = NumberUtility.ToDecimal(unitPriceStr),
                        AccountInternals = GetAccounts(accountDims, accountStr),
                        ProcessingRule = processingRuleStr,
                        AccountStd = GetAccountStd(accountDims, accountStr),
                        Scope = scopeStr,
                    };

                    transactions.Add(transaction);

                    #endregion
                }
            }

            return transactions;

        }

        private List<Schedule> GetSchedules(List<string> scheduleLines, List<ScheduleDefinition> scheduleDefinitions)
        {
            //Definition av schemadag
            //En schemadagsdefinition består av ett antal fält på följande vis:
            //
            //Fältnamn          Kommentar       Exempel
            //Identifierare     Alltid @        @
            //Dagkod            Obligatorisk    DK
            //Starttid          Obligatorisk    7.00
            //Sluttid           Obligatorisk    14.15
            //Arbetstid         Obligatorisk    6.45

            List<Schedule> schedules = new List<Schedule>();

            foreach (var line in scheduleLines)
            {
                if (line != String.Empty)
                {
                    string[] row = line.Split(',');

                    if (row.Count() < 4)
                        continue;

                    string employeeNr = row.Length > 0 ? row[0] : String.Empty;
                    string key = row.Length > 1 ? row[1] : String.Empty;
                    string dateStr = row.Length > 3 ? row[3] : String.Empty;

                    var definition = scheduleDefinitions.FirstOrDefault(f => f.Key == key);

                    if (definition != null)
                    {

                        Schedule schedule = new Schedule()
                        {
                            Key = key,
                            date = CalendarUtility.GetDateTime(dateStr, "yyyy-MM-dd"),
                            Definition = definition,
                            EmployeeNr = employeeNr,
                        };

                        schedules.Add(schedule);
                    }
                }
            }

            return schedules;
        }

        private AccountDTO GetAccountStd(List<AccountDimDTO> accountDims, string accountStr)
        {
            if (accountDims.IsNullOrEmpty())
                return null;

            return GetAccounts(accountDims.Where(f => f.AccountDimNr == Constants.ACCOUNTDIM_STANDARD).ToList(), accountStr)?.FirstOrDefault();
        }

        private List<AccountDTO> GetAccounts(List<AccountDimDTO> accountDims, string accountStr)
        {

            //KSK
            // KSK är en förkortning för kontosträngskod. Denna kod används endastnär man vill att transaktionen skall vara styrande för den kontering som
            // den slutliga lönetransaktionen skall ha.Konterinsgsträngen består av ett konto utifrån den befintliga kontoplanen där sedan en
            // underkontering / kostnadsställe anges.Formatet på konteringssträngen är: Konto; uk1; uk2; ...; uk9
            // Följande exempel illustuerar olika varianter:
            // 7210; 10; 20; 30(Konto 7210 med underkonteringen 10, 20, 30)
            // ;10;20;30 (Konto från löneart men med styrd underkontering)

            List<AccountDTO> accountDTOs = new List<AccountDTO>();

            // Accounts are semicolon separated inside its own placeholder
            string[] accounts = !string.IsNullOrEmpty(accountStr) ? accountStr.Split(';') : new string[0];

            if (accounts.Length > 0 && !accountDims.IsNullOrEmpty())
            {
                if (accountDims.Count == 1)
                {
                    AccountDTO std = GetAccountFromAccountNr(0, accounts, accountDims, TermGroup_SieAccountDim.CostCentre);
                    if (std != null)
                        return new List<AccountDTO>() { std };
                }

                AccountDTO costCentre = GetAccountFromAccountNr(1, accounts, accountDims, TermGroup_SieAccountDim.CostCentre);
                if (costCentre != null)
                    accountDTOs.Add(costCentre);

                AccountDTO costUnit = GetAccountFromAccountNr(2, accounts, accountDims, TermGroup_SieAccountDim.CostUnit);
                if (costUnit != null)
                    accountDTOs.Add(costUnit);

                AccountDTO project = GetAccountFromAccountNr(3, accounts, accountDims, TermGroup_SieAccountDim.Project);
                if (project != null)
                    accountDTOs.Add(project);

                AccountDTO employee = GetAccountFromAccountNr(4, accounts, accountDims, TermGroup_SieAccountDim.Employee);
                if (employee != null)
                    accountDTOs.Add(employee);

                AccountDTO customer = GetAccountFromAccountNr(5, accounts, accountDims, TermGroup_SieAccountDim.Customer);
                if (customer != null)
                    accountDTOs.Add(customer);

                AccountDTO department = GetAccountFromAccountNr(6, accounts, accountDims, TermGroup_SieAccountDim.Department);
                if (department != null)
                    accountDTOs.Add(department);
            }

            return accountDTOs;
        }

        private AccountDTO GetAccountFromAccountNr(int index, string[] accounts, List<AccountDimDTO> dims, TermGroup_SieAccountDim sieAccountDim)
        {
            if (accounts.Length <= index)
                return null;

            try
            {
                string accountNr = accounts[index];
                if (string.IsNullOrEmpty(accountNr))
                    return null;

                AccountDimDTO dim = null;
                if (index > 0)
                {
                    dim = dims.FirstOrDefault(f => f.SysSieDimNr == (int)sieAccountDim);
                    if (dim == null)
                        return null;
                }
                else
                {
                    dim = dims.FirstOrDefault(f => f.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
                    if (dim == null)
                        return null;
                }

                return dim.Accounts?.FirstOrDefault(a => a.AccountNr == accountNr || (!string.IsNullOrEmpty(a.ExternalCode) && a.ExternalCode.Equals(accountNr, StringComparison.OrdinalIgnoreCase)));
            }
            catch
            {
                return null;
            }
        }

        private DateTime GetTime(string timeStr)
        {
            string[] row = timeStr.Split('.');

            if (int.TryParse(row[0], out int hours))
            {
                if (int.TryParse(row[1], out int minutes))

                    return CalendarUtility.DATETIME_DEFAULT.AddHours(hours).AddMinutes(minutes);
            }
            return CalendarUtility.DATETIME_DEFAULT;
        }


        private class Transaction
        {
            public string EmployeeNr { get; set; }
            public string PayrollProductNr { get; set; }
            public string Department { get; set; }
            public decimal Quantity { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public int CalendarDays { get; set; }
            public int WorkDays { get; set; }
            public decimal UnitPrice { get; set; }
            public AccountDTO AccountStd { get; set; }
            public List<AccountDTO> AccountInternals { get; set; }
            public string ProcessingRule { get; set; }
            public string Scope { get; set; }
        }

        private class ScheduleDefinition
        {
            public string Key { get; set; }
            public DateTime Start { get; set; }
            public DateTime Stop { get; set; }
            public decimal Hours { get; set; }
        }

        private class Schedule
        {
            public string EmployeeNr { get; set; }
            public string Key { get; set; }
            public DateTime date { get; set; }
            public ScheduleDefinition Definition { get; set; }
        }
    }
}
