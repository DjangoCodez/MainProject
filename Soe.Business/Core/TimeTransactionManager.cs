using Newtonsoft.Json;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.BatchHelper;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Data.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class TimeTransactionManager : ManagerBase
    {
        #region Constructors

        public TimeTransactionManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<TimeCode> cachedTimeCodes = new List<TimeCode>();
        private readonly List<Product> cachedProducts = new List<Product>();

        #endregion

        #region Earned Holidays

        public List<EmployeeEarnedHolidayDTO> LoadEarnedHolidaysContent(int holidayId, int year, bool loadSuggestions, int userId, int roleId, int actorCompanyId, List<EmployeeEarnedHolidayDTO> employeeEarnedHolidaysInput = null)
        {
            List<EmployeeEarnedHolidayDTO> employeeEarnedHolidays = new List<EmployeeEarnedHolidayDTO>();

            HolidayDTO holiday = CalendarManager.GetHoliday(holidayId, actorCompanyId, year);
            if (holiday == null)
                return employeeEarnedHolidays;

            string yes = GetText(5713, "Ja");
            string no = GetText(5714, "Nej");

            List<Employee> employees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, roleId, getVacant: false);
            foreach (Employee employee in employees)
            {
                EmployeeEarnedHolidayDTO earnedHoliday = employeeEarnedHolidaysInput?.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId);
                if (earnedHoliday == null)
                {
                    Employment employment = employee.GetEmployment(holiday.Date);
                    if (employment == null)
                        continue;

                    int? payrollGroupId = employment.GetPayrollGroupId(holiday.Date);
                    if (!payrollGroupId.HasValue)
                        continue;

                    PayrollGroupSetting setting = PayrollManager.GetPayrollGroupSetting(payrollGroupId.Value, PayrollGroupSettingType.EarnedHoliday);
                    if (setting == null || !setting.BoolData.HasValue || !setting.BoolData.Value)
                        continue;

                    earnedHoliday = new EmployeeEarnedHolidayDTO()
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeNr = employee.EmployeeNr,
                        EmployeeName = employee.FirstName + " " + employee.LastName,
                        EmployeePercent = Decimal.Round(employment.GetPercent(holiday.Date), 2),
                    };
                }

                decimal avarageDaysPerWeek = TimeScheduleManager.GetAverageDaysPerWeek(actorCompanyId, employee.EmployeeId, holiday.Date, out bool hasTemplateSchedule);
                bool fullTimeEligible = earnedHoliday.EmployeePercent == 100 || avarageDaysPerWeek >= 5;
                bool hasEligableAbsence = false;
                bool eligible = false;
                StringBuilder suggestionNote = null;

                if (loadSuggestions)
                {
                    //Heltidsanställda kan få en extra ledighetsdag om de arbetar, har ordinarie ledigt enligt schema eller har semester på en helgdag samt på jul-, nyårs - och midsommarafton som infaller måndag – fredag.
                    //Detsamma gäller för deltidare som jobbar i genomsnitt fem dagar i veckan. 

                    //För lördagar gäller speciella regler, se rutan här intill.På röda lördagar finns det också en chans att tjäna in en extra ledighetsdag.
                    //Men speciella regler gäller. Du måste vara heltidsanställd (eller deltid med i snitt fem arbetsdagar i veckan) och du måste ha en arbetsdag på lördagen.

                    bool hasPresence = false;
                    bool hasAbsence = false;
                    string absenceName = string.Empty;
                    List<TimePayrollTransaction> timePayrollTransactions = TimeTransactionManager.GetTimePayrollTransactionsForEmployee(employee.EmployeeId, includePayrollProduct: true, fromDate: holiday.Date, toDate: holiday.Date, includeTimeCodeTransactions: true);
                    if (!timePayrollTransactions.IsNullOrEmpty())
                    {
                        if (timePayrollTransactions.Any(t => !t.IsAbsence()))
                        {
                            hasPresence = true;
                        }
                        if (timePayrollTransactions.Any(t => t.IsAbsence()))
                        {
                            hasAbsence = true;
                            hasEligableAbsence = timePayrollTransactions.Any(t => t.IsAbsenceVacation());
                            absenceName = timePayrollTransactions.FirstOrDefault(t => t.IsAbsence())?.PayrollProduct.Name ?? string.Empty;
                        }
                    }

                    // Eligibility                    
                    eligible = fullTimeEligible &&
                        !(holiday.IsSaturday && !hasTemplateSchedule) &&
                        !(hasAbsence && !hasEligableAbsence) &&
                        hasPresence;

                    // Suggestion note
                    suggestionNote = new StringBuilder(fullTimeEligible
                        ? GetText(11030, "Heltid eller fler än 5 dagar i snitt")
                        : GetText(11031, "Uppfyller inte heltid eller fler än 5 dagar i snitt"));

                    if (holiday.IsSaturday && hasTemplateSchedule)
                        suggestionNote.Append(". ").Append(GetText(11032, "Arbetslördag"));

                    if (hasAbsence)
                    {
                        if (hasEligableAbsence)
                            suggestionNote.Append(". ").Append(GetText(11033, "Giltig frånvaro:")).Append(' ').Append(absenceName);
                        else
                            suggestionNote.Append(". ").Append(GetText(11034, "Ogiltig frånvaro:")).Append(' ').Append(absenceName);
                    }

                    if (!hasPresence)
                        suggestionNote.Append(". ").Append(GetText(0, "Ingen närvaro rapporterad"));

                    earnedHoliday.HasTransaction = timePayrollTransactions.Any(t => t.TimeCodeTransaction != null && t.TimeCodeTransaction.IsEarnedHoliday);
                    earnedHoliday.HasTransactionString = earnedHoliday.HasTransaction ? yes : no;
                }

                earnedHoliday.Work5DaysPerWeek = fullTimeEligible;
                earnedHoliday.Work5DaysPerWeekString = fullTimeEligible ? yes : no;
                earnedHoliday.Suggestion = eligible;
                earnedHoliday.SuggestionString = eligible ? yes : no;
                earnedHoliday.SuggestionNote = suggestionNote?.ToString() ?? string.Empty;

                employeeEarnedHolidays.Add(earnedHoliday);
            }

            //If not loading suggestions (and thus loading transactions for holiday by date), load TimeCodeTransactions for all employees separately 
            if (!loadSuggestions)
            {
                var employeeIds = employeeEarnedHolidays.Select(e => e.EmployeeId).ToList();
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                var employeeIdsWithTransactions = GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entitiesReadOnly, base.ActorCompanyId, employeeIds, holiday.Date, holiday.Date), TimeTransactionManager.GetEmployeesWithTimeCodeTransactionsForEarnedHoliday);

                foreach (var employeeEarnedHoliday in employeeEarnedHolidays)
                {
                    employeeEarnedHoliday.HasTransaction = employeeIdsWithTransactions.Contains(employeeEarnedHoliday.EmployeeId);
                    employeeEarnedHoliday.HasTransactionString = employeeEarnedHoliday.HasTransaction ? yes : no;
                }
            }

            return employeeEarnedHolidays;
        }

        public List<int> GetEmployeesWithTimeCodeTransactionsForEarnedHoliday(GetDataInBatchesModel model)
        {
            var employeeIds = model.Entities.TimeCodeTransaction
                .AsNoTracking()
                .Where(t =>
                    t.State == (int)SoeEntityState.Active &&
                    t.TimeBlockDate != null &&
                    t.TimeBlockDate.Date == model.StartDate &&
                    model.BatchIds.Contains(t.TimeBlockDate.EmployeeId))
                .Select(t => new { t.TimeBlockDate.EmployeeId, t.IsEarnedHoliday })
                .ToList();

            return employeeIds
                .Where(x => x.IsEarnedHoliday)
                .Select(x => x.EmployeeId)
                .Distinct()
                .ToList();
        }

        #endregion

        #region TimeCalendar

        public List<TimeCalendarPeriodDTO> GetTimeCalendarPeriods(int actorCompanyId, int employeeId, DateTime fromDate, DateTime toDate, int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, Dictionary<int, List<int>> excludedLevels = null, bool includeHolidays = false)
        {
            List<TimeCalendarPeriodDTO> dtos = new List<TimeCalendarPeriodDTO>();
            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();

            #region Holidays

            if (includeHolidays)
            {
                List<HolidayDTO> holidays = CalendarManager.GetHolidaysByCompany(actorCompanyId, fromDate, toDate);
                foreach (var holidaysByDate in holidays.GroupBy(i => i.Date))
                {
                    TimeCalendarPeriodDTO dto = new TimeCalendarPeriodDTO(holidaysByDate.Key);
                    dto.DayDescription = holidaysByDate.Select(i => i.Description).Distinct().ToCommaSeparated();
                    dtos.Add(dto);
                }
            }

            #endregion

            #region Transactions

            List<TimePayrollTransaction> transactions = (from tpt in entitiesReadOnly.TimePayrollTransaction
                                                        .Include("TimeBlockDate").Include("PayrollProduct")
                                                         where tpt.EmployeeId == employeeId &&
                                                         tpt.TimeBlockDate.Date >= fromDate && tpt.TimeBlockDate.Date <= toDate &&
                                                         tpt.State == (int)SoeEntityState.Active &&
                                                         (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                                                         select tpt).ToList();

            transactions = transactions.Where(tpt => !tpt.ReversedDate.HasValue).ToList();

            // Filter on levels
            if (sysPayrollTypeLevel1.HasValue)
                transactions = transactions.Where(tpt => tpt.SysPayrollTypeLevel1 == sysPayrollTypeLevel1.Value).ToList();
            if (sysPayrollTypeLevel2.HasValue)
                transactions = transactions.Where(tpt => tpt.SysPayrollTypeLevel2 == sysPayrollTypeLevel2.Value).ToList();
            if (sysPayrollTypeLevel3.HasValue)
                transactions = transactions.Where(tpt => tpt.SysPayrollTypeLevel3 == sysPayrollTypeLevel3.Value).ToList();
            if (sysPayrollTypeLevel4.HasValue)
                transactions = transactions.Where(tpt => tpt.SysPayrollTypeLevel4 == sysPayrollTypeLevel4.Value).ToList();

            // Exclude levels
            if (excludedLevels != null)
            {
                if (excludedLevels.ContainsKey(1))
                {
                    List<int> excludedLevels1 = excludedLevels[1];
                    transactions = transactions.Where(tpt => !tpt.SysPayrollTypeLevel1.HasValue || !excludedLevels1.Contains(tpt.SysPayrollTypeLevel1.Value)).ToList();
                }
                if (excludedLevels.ContainsKey(2))
                {
                    List<int> excludedLevels2 = excludedLevels[2];
                    transactions = transactions.Where(tpt => !tpt.SysPayrollTypeLevel2.HasValue || !excludedLevels2.Contains(tpt.SysPayrollTypeLevel2.Value)).ToList();
                }
                if (excludedLevels.ContainsKey(3))
                {
                    List<int> excludedLevels3 = excludedLevels[3];
                    transactions = transactions.Where(tpt => !tpt.SysPayrollTypeLevel3.HasValue || !excludedLevels3.Contains(tpt.SysPayrollTypeLevel3.Value)).ToList();
                }
                if (excludedLevels.ContainsKey(4))
                {
                    List<int> excludedLevels4 = excludedLevels[4];
                    transactions = transactions.Where(tpt => !tpt.SysPayrollTypeLevel4.HasValue || !excludedLevels4.Contains(tpt.SysPayrollTypeLevel4.Value)).ToList();
                }
            }

            foreach (TimePayrollTransaction transaction in transactions)
            {
                TimeCalendarPeriodDTO dto = dtos.FirstOrDefault(d => d.Date == transaction.TimeBlockDate.Date);
                if (dto == null)
                {
                    dto = new TimeCalendarPeriodDTO(transaction.TimeBlockDate.Date);
                    dtos.Add(dto);
                }

                TimeCalendarPeriodPayrollProductDTO productDTO = new TimeCalendarPeriodPayrollProductDTO()
                {
                    PayrollProductId = transaction.ProductId,
                    Number = transaction.PayrollProduct.Number,
                    Name = transaction.PayrollProduct.Name,
                    SysPayrollTypeLevel1 = transaction.SysPayrollTypeLevel1,
                    SysPayrollTypeLevel2 = transaction.SysPayrollTypeLevel2,
                    SysPayrollTypeLevel3 = transaction.SysPayrollTypeLevel3,
                    SysPayrollTypeLevel4 = transaction.SysPayrollTypeLevel4,
                    Amount = transaction.Quantity
                };

                dto.PayrollProducts.Add(productDTO);
            }

            #endregion

            return dtos;
        }

        #endregion

        #region TimeInvoiceTransaction (external)

        public TimeInvoiceTransaction GetTimeInvoiceTransaction(CompEntities entities, int timeInvoiceTransactionId)
        {
            return (from t in entities.TimeInvoiceTransaction
                    where t.TimeInvoiceTransactionId == timeInvoiceTransactionId
                    select t).FirstOrDefault();
        }

        public TimeInvoiceTransaction GetTimeInvoiceTransactionForInvoiceRow(CompEntities entities, int customerInvoiceRowId)
        {
            return (from t in entities.TimeInvoiceTransaction
                    where t.CustomerInvoiceRowId == customerInvoiceRowId && t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        public ProjectInvoiceDay GetProjectInvoiceDay(CompEntities entities, int ProjectInvoiceDayId)
        {
            return (from t in entities.ProjectInvoiceDay
                    .Include("ProjectInvoiceWeek")
                    where t.ProjectInvoiceDayId == ProjectInvoiceDayId
                    select t).FirstOrDefault();
        }

        public List<TimeInvoiceTransaction> GetTimeInvoiceTransactionsForInvoiceRow(int customerInvoiceRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeInvoiceTransaction.NoTracking();
            return GetTimeInvoiceTransactionsForInvoiceRow(entities, customerInvoiceRowId);
        }

        public List<TimeInvoiceTransaction> GetTimeInvoiceTransactionsForInvoiceRow(CompEntities entities, int customerInvoiceRowId)
        {
            return (from t in entities.TimeInvoiceTransaction
                    .Include("TimeCodeTransaction.TimeCode")
                    .Include("TimeCodeTransaction.ProjectInvoiceDay")
                    .Include("TimeBlockDate")
                    where t.CustomerInvoiceRowId == customerInvoiceRowId && t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimeInvoiceTransaction> GetTimeInvoiceTransactions(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo)
        {
            return (from tit in entities.TimeInvoiceTransaction
                    where tit.ActorCompanyId == actorCompanyId &&
                          tit.TimeBlockDate.Date >= dateFrom &&
                          tit.TimeBlockDate.Date <= dateTo &&
                          tit.State == (int)SoeEntityState.Active
                    select tit).ToList();
        }

        public TimeInvoiceTransaction CreateTimeInvoiceTransaction(CompEntities entities, int actorCompanyId, InvoiceProduct product, TimeBlockDate timeBlockDate, int quantity, int employeeId, int accountId, int attestStateId, int amount = 0, int vatAmount = 0, List<AccountInternalDTO> accountInternals = null)
        {
            if (product == null)
                return null;

            TimeInvoiceTransaction timeInvoiceTransaction = new TimeInvoiceTransaction
            {
                Quantity = quantity,
                InvoiceQuantity = quantity,
                Amount = amount,
                VatAmount = vatAmount,
                Invoice = true,
                ManuallyAdded = false,
                Exported = false,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                AccountStdId = accountId,
                AttestStateId = attestStateId,

                //References
                InvoiceProduct = product,
                TimeBlockDate = timeBlockDate,
            };
            SetCreatedProperties(timeInvoiceTransaction);
            entities.TimeInvoiceTransaction.AddObject(timeInvoiceTransaction);

            if (!accountInternals.IsNullOrEmpty())
            {
                if (timeInvoiceTransaction.AccountInternal == null)
                    timeInvoiceTransaction.AccountInternal = new EntityCollection<AccountInternal>();

                foreach (var ai in accountInternals)
                {
                    AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, ai.AccountId, actorCompanyId);
                    if (accountInternal != null)
                        timeInvoiceTransaction.AccountInternal.Add(accountInternal);
                }
            }

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeInvoiceTransaction);

            return timeInvoiceTransaction;
        }

        public TimeInvoiceTransaction CreateTimeInvoiceTransaction(CompEntities entities, InvoiceProduct product, ProjectTimeBlock projectTimeBlock, int accountId, int attestStateId, int amount = 0, int vatAmount = 0)
        {
            if (product == null)
                return null;

            var transaction = new TimeInvoiceTransaction
            {
                Quantity = projectTimeBlock.InvoiceQuantity,
                InvoiceQuantity = projectTimeBlock.InvoiceQuantity,
                Amount = amount,
                VatAmount = vatAmount,
                Invoice = true,
                ManuallyAdded = false,
                Exported = false,

                //Set FK
                ActorCompanyId = projectTimeBlock.ActorCompanyId,
                EmployeeId = projectTimeBlock.EmployeeId,
                AccountStdId = accountId,
                AttestStateId = attestStateId,
                TimeBlockDateId = projectTimeBlock.TimeBlockDateId,

                //References
                InvoiceProduct = product,
            };
            SetCreatedProperties(transaction);
            entities.TimeInvoiceTransaction.AddObject(transaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, projectTimeBlock.ActorCompanyId, transaction);

            return transaction;
        }

        private ActionResult CreateTimeInvoiceTransaction(CompEntities entities, int actorCompanyId, TimeCodeTransaction timeCodetransaction, CustomerInvoiceRow customerInvoiceRow, InvoiceProduct product)
        {
            int accountId = 0;

            CustomerInvoiceAccountRow accountRow = customerInvoiceRow.ActiveCustomerInvoiceAccountRows.FirstOrDefault(a => a.AccountId != customerInvoiceRow.VatAccountId);
            if (accountRow != null && accountRow.AccountStd != null)
            {
                accountId = accountRow.AccountStd.AccountId;
            }
            else
            {
                accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, 0, actorCompanyId, 0);
                if (accountId == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8332, "Standardkonto för tidavtal saknas"));
            }

            /* KOM ÄVEN IHÅG ATT ÄNDRA I UPDATE OM DETTA BACKAS
             * 
             * int accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, 0, actorCompanyId);
            if (accountId == 0)
                return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8332, "Standardkonto för tidavtal saknas"));*/

            TimeInvoiceTransaction timeInvoiceTransaction = new TimeInvoiceTransaction()
            {
                Quantity = customerInvoiceRow.Quantity ?? 0,
                InvoiceQuantity = customerInvoiceRow.Quantity ?? 0,
                Amount = customerInvoiceRow.Amount,
                AmountCurrency = customerInvoiceRow.AmountCurrency,
                VatAmount = customerInvoiceRow.VatAmount,
                VatAmountCurrency = customerInvoiceRow.VatAmountCurrency,
                Exported = false,

                //Set FK
                ActorCompanyId = actorCompanyId,
                AccountStdId = accountId,

                //References
                InvoiceProduct = product,
            };
            SetCreatedProperties(timeInvoiceTransaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeInvoiceTransaction, customerInvoiceRow);

            customerInvoiceRow.TimeInvoiceTransaction.Add(timeInvoiceTransaction);
            timeCodetransaction.TimeInvoiceTransaction.Add(timeInvoiceTransaction);

            return new ActionResult(true);
        }

        private ActionResult UpdateTimeInvoiceTransaction(CompEntities entities, int actorCompanyId, TimeInvoiceTransaction invoiceTransaction, CustomerInvoiceRow invoiceRow, InvoiceProduct product)
        {
            int accountId = 0;

            CustomerInvoiceAccountRow accountRow = invoiceRow.ActiveCustomerInvoiceAccountRows.FirstOrDefault(a => a.AccountId != invoiceRow.VatAccountId);
            if (accountRow != null && accountRow.AccountStd != null)
            {
                accountId = accountRow.AccountStd.AccountId;
            }
            else
            {
                accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, 0, actorCompanyId, 0);
                if (accountId == 0)
                    return new ActionResult((int)ActionResultSave.ProjectTransactionsNotCreated, GetText(8332, "Standardkonto för tidavtal saknas"));
            }

            invoiceTransaction.Quantity = invoiceRow.Quantity ?? 0;
            invoiceTransaction.InvoiceQuantity = invoiceRow.Quantity ?? 0;

            invoiceTransaction.Amount = invoiceRow.Amount;
            invoiceTransaction.AmountCurrency = invoiceRow.AmountCurrency;
            invoiceTransaction.VatAmount = invoiceRow.VatAmount;
            invoiceTransaction.VatAmountCurrency = invoiceRow.VatAmountCurrency;

            //FK
            invoiceTransaction.InvoiceProduct = product;
            invoiceTransaction.AccountStdId = accountId;

            SetModifiedProperties(invoiceTransaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, invoiceTransaction);

            return new ActionResult(true);
        }

        #endregion

        #region TimePayrollTransaction (external)

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForCompany(int actorCompanyId, DateTime dateFrom, DateTime dateTo, bool loadTimeCodeTransaction = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return GetTimePayrollTransactionsForCompany(entities, actorCompanyId, dateFrom, dateTo, loadTimeCodeTransaction);
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForCompany(CompEntities entities, int actorCompanyId, DateTime dateFrom, DateTime dateTo, bool loadTimeCodeTransaction = false)
        {
            var query = from tpt in entities.TimePayrollTransaction
                        where tpt.ActorCompanyId == actorCompanyId &&
                        tpt.TimeBlockDate.Date >= dateFrom &&
                        tpt.TimeBlockDate.Date <= dateTo &&
                        tpt.State == (int)SoeEntityState.Active
                        select tpt;

            if (loadTimeCodeTransaction)
                query = query.Include("TimeCodeTransaction");

            return query.ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForCompany(CompEntities entities, int actorCompanyId, DateTime date, TermGroup_SysPayrollType? sysPayrollTypeLevel1 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            date = CalendarUtility.GetBeginningOfDay(date);

            IQueryable<TimePayrollTransaction> timePayrollTransactionsQuery = (from tpt in entities.TimePayrollTransaction
                                                                               where tpt.ActorCompanyId == actorCompanyId &&
                                                                               tpt.State == (int)SoeEntityState.Active &&
                                                                               (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active) &&
                                                                               tpt.TimeBlockDate.Date == date
                                                                               select tpt);

            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterPayrollType(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);

            return timePayrollTransactionsQuery.ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployee(CompEntities entities, int employeeId, int timeBlockDateId, TermGroup_SysPayrollType? sysPayrollTypeLevel1 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            IQueryable<TimePayrollTransaction> timePayrollTransactionsQuery = (from tpt in entities.TimePayrollTransaction
                                                                               where tpt.EmployeeId == employeeId &&
                                                                               tpt.TimeBlockDateId == timeBlockDateId &&
                                                                               tpt.State == (int)SoeEntityState.Active &&
                                                                               (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                                                                               select tpt);

            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterPayrollType(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);

            return timePayrollTransactionsQuery.ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployee(int employeeId, DateTime dateFrom, DateTime dateTo, bool loadExtended = false, bool loadTimeBlockDate = false, bool loadTimeCodeTransaction = false, bool loadAccountInternal = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return GetTimePayrollTransactionsForEmployee(entities, employeeId, dateFrom, dateTo, loadExtended, loadTimeBlockDate, loadTimeCodeTransaction, loadAccountInternal);
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployee(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo, bool loadExtended = false, bool loadTimeBlockDate = false, bool loadTimeCodeTransaction = false, bool loadAccountInternal = false)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            var query = (from tpt in entities.TimePayrollTransaction
                         where tpt.EmployeeId == employeeId &&
                         tpt.TimeBlockDate.Date >= dateFrom &&
                         tpt.TimeBlockDate.Date <= dateTo &&
                         tpt.State == (int)SoeEntityState.Active
                         select tpt);

            if (loadTimeBlockDate)
                query = query.Include("TimeBlockDate");
            if (loadExtended)
                query = query.Include("TimePayrollTransactionExtended");
            if (loadTimeCodeTransaction)
                query = query.Include("TimeCodeTransaction");
            if (loadAccountInternal)
                query = query.Include("AccountInternal");

            return query.ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployee(int employeeId, DateTime? fromDate = null, DateTime? toDate = null, TermGroup_SysPayrollType? sysPayrollTypeLevel1 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null, bool onlyCurrent = true, bool includePayrollProduct = false, bool includeTimeCodeTransactions = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return GetTimePayrollTransactionsForEmployee(entities, employeeId, fromDate, toDate, sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4, onlyCurrent, includePayrollProduct);
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployee(CompEntities entities, int employeeId, DateTime? fromDate = null, DateTime? toDate = null, TermGroup_SysPayrollType? sysPayrollTypeLevel1 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null, bool onlyCurrent = true, bool includePayrollProduct = false, bool includeTimeCodeTransactions = false)
        {
            IQueryable<TimePayrollTransaction> timePayrollTransactionsQuery = (from tpt in entities.TimePayrollTransaction
                                                                                .Include(t => t.TimeBlockDate)
                                                                               where tpt.EmployeeId == employeeId &&
                                                                               tpt.State == (int)SoeEntityState.Active &&
                                                                               (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                                                                               select tpt);
            if (includeTimeCodeTransactions)
                timePayrollTransactionsQuery = timePayrollTransactionsQuery.Include(t => t.TimeCodeTransaction);
            if (includePayrollProduct)
                timePayrollTransactionsQuery = timePayrollTransactionsQuery.Include(t => t.PayrollProduct.PayrollProductSetting);

            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterPayrollType(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterDates(fromDate, toDate);
            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterCurrent(onlyCurrent);

            return timePayrollTransactionsQuery.ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployee(CompEntities entities, int employeeId, List<int> timeBlockDateIds)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    timeBlockDateIds.Contains(t.TimeBlockDateId) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployees(CompEntities entities, List<int> employeeIds, DateTime? fromDate = null, DateTime? toDate = null, TermGroup_SysPayrollType? sysPayrollTypeLevel1 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null, bool onlyCurrent = true, bool includePayrollProduct = false, bool includeTimeBlockDate = false)
        {
            IQueryable<TimePayrollTransaction> timePayrollTransactionsQuery = (from tpt in entities.TimePayrollTransaction
                                                                               where employeeIds.Contains(tpt.EmployeeId) &&
                                                                               tpt.State == (int)SoeEntityState.Active &&
                                                                               (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                                                                               select tpt);

            if (includePayrollProduct)
                timePayrollTransactionsQuery = timePayrollTransactionsQuery.Include("PayrollProduct.PayrollProductSetting");

            if (includeTimeBlockDate)
                timePayrollTransactionsQuery = timePayrollTransactionsQuery.Include("TimeBlockDate");

            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterPayrollType(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterDates(fromDate, toDate);
            timePayrollTransactionsQuery = timePayrollTransactionsQuery.FilterCurrent(onlyCurrent);

            return timePayrollTransactionsQuery.ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForStartValues(int actorCompanyId, int employeeId, int payrollStartValueRowId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return GetTimePayrollTransactionsForStartValues(entities, actorCompanyId, employeeId, payrollStartValueRowId);
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForStartValues(CompEntities entities, int actorCompanyId, int employeeId, int payrollStartValueRowId)
        {
            return (from tpt in entities.TimePayrollTransaction
                    .Include("TimeBlockDate")
                    .Include("PayrollProduct")
                    where tpt.ActorCompanyId == actorCompanyId &&
                    tpt.EmployeeId == employeeId &&
                    tpt.PayrollStartValueRowId.HasValue && tpt.PayrollStartValueRowId.Value == payrollStartValueRowId &&
                    tpt.State == (int)SoeEntityState.Active
                    select tpt).ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForRetro(int retroactivePayrollOutcomeId, int employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return GetTimePayrollTransactionsForRetro(entities, retroactivePayrollOutcomeId, employeeId, actorCompanyId);
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForRetro(CompEntities entities, int retroactivePayrollOutcomeId, int employeeId, int actorCompanyId)
        {
            return (from tpt in entities.TimePayrollTransaction
                    .Include("PayrollProduct")
                    .Include("TimeBlockDate")
                    .Include("AccountStd.Account")
                    .Include("AccountInternal.Account")
                    where tpt.ActorCompanyId == actorCompanyId &&
                    tpt.EmployeeId == employeeId &&
                    tpt.RetroactivePayrollOutcomeId == retroactivePayrollOutcomeId &&
                    tpt.State == (int)SoeEntityState.Active
                    select tpt).ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsTimeAccumulator(CompEntities entities, int timeAccumulatorId, List<DateRangeDTO> dateRanges)
        {
            int actorCompanyId = base.ActorCompanyId;

            DateTime startDate = CalendarUtility.GetBeginningOfWeek(dateRanges.Min(d => d.Start));
            DateTime stopDate = CalendarUtility.GetEndOfWeek(dateRanges.Max(d => d.Stop));

            var timePayrollTransactions = (from tpt in entities.TimePayrollTransaction
                    .Include("TimeBlockDate")
                where tpt.ActorCompanyId == actorCompanyId &&
                tpt.EarningTimeAccumulatorId == timeAccumulatorId &&
                tpt.TimeBlockDate.Date >= startDate &&
                tpt.TimeBlockDate.Date <= stopDate &&
                tpt.State == (int)SoeEntityState.Active
                select tpt).ToList();

            return timePayrollTransactions
                .Where(tpt => dateRanges.Any(dr =>
                    tpt.TimeBlockDate.Date >= CalendarUtility.GetBeginningOfWeek(dr.Start) && 
                    tpt.TimeBlockDate.Date <= CalendarUtility.GetEndOfWeek(dr.Stop)))
                .ToList();
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForAccountProvision(int actorCompanyId, List<Employee> employees, TimePeriod timePeriod, bool loadExtended = false, bool loadTimeBlockDate = false, bool loadAccountInternals = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return GetTimePayrollTransactionsForAccountProvision(entities, actorCompanyId, employees, timePeriod, loadExtended, loadTimeBlockDate, loadAccountInternals);
        }

        public List<TimePayrollTransaction> GetTimePayrollTransactionsForAccountProvision(CompEntities entities, int actorCompanyId, List<Employee> employees, TimePeriod timePeriod, bool loadExtended = false, bool loadTimeBlockDate = false, bool loadAccountInternals = false)
        {
            List<int> employeeIds = employees.Select(e => e.EmployeeId).ToList();
            List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction>();
            if (employees != null && timePeriod != null)
            {
                IQueryable<TimePayrollTransaction> query = entities.TimePayrollTransaction;
                if (loadExtended)
                    query = query.Include("TimePayrollTransactionExtended");
                if (loadTimeBlockDate)
                    query = query.Include("TimeBlockDate");
                if (loadAccountInternals)
                    query = query.Include("AccountInternal.Account");

                entities.CommandTimeout = 180; // 3 minutes
                timePayrollTransactions = (from t in query
                                           where //employeeIds.Contains(t.EmployeeId) &&
                                           t.ActorCompanyId == actorCompanyId &&
                                           t.TimeBlockDate.Date >= timePeriod.StartDate &&
                                           t.TimeBlockDate.Date <= timePeriod.StopDate &&
                                           t.TimeCodeTransactionId.HasValue &&
                                           t.TimeCodeTransaction.IsProvision &&
                                           t.State == (int)SoeEntityState.Active
                                           select t).ToList();
            }

            return timePayrollTransactions.Where(x => employeeIds.Contains(x.EmployeeId)).ToList();
        }

        public List<TimePayrollTransactionTreeDTO> GetTimePayrollTransactionsForTree(CompEntities entities, int actorCompanyId, DateTime? startDate, DateTime? stopDate, TimePeriod timePeriod, List<int> employeeIds, bool onlyUseInPayroll = false, bool includeAccounting = false)
        {
            if (timePeriod == null && !startDate.HasValue && !stopDate.HasValue)
                return new List<TimePayrollTransactionTreeDTO>();

            List<TimePayrollTransactionTreeDTO> transactionsToReturn = new List<TimePayrollTransactionTreeDTO>();

            bool useEmployeesInQuery = EmployeeManager.UseEmployeeIdsInQuery(entities, employeeIds);
            bool isValidPeriod = timePeriod.IsValid();
            bool isExtraPeriod = isValidPeriod && timePeriod.IsExtraPeriod();

            if (isValidPeriod)
            {
                startDate = !isExtraPeriod ? timePeriod.StartDate : (DateTime?)null;
                stopDate = !isExtraPeriod ? timePeriod.StopDate : (DateTime?)null;
            }

            const int STAGE_DATES = 1;
            const int STAGE_TIMEPERIOD = 2;
            int stages = isValidPeriod ? STAGE_TIMEPERIOD : STAGE_DATES;

            for (int stage = 1; stage <= stages; stage++)
            {
                if (stage == STAGE_DATES && isExtraPeriod)
                    continue;

                IQueryable<TimePayrollTransaction> query = (from tpt in entities.TimePayrollTransaction
                                                            where tpt.ActorCompanyId == actorCompanyId &&
                                                            tpt.State == (int)SoeEntityState.Active
                                                            select tpt);

                if (useEmployeesInQuery)
                    query = query.Where(tpt => employeeIds.Contains(tpt.EmployeeId));

                if (stage == STAGE_DATES)
                {
                    if (stages == 1)
                        query = query.Where(tpt => tpt.TimeBlockDate.Date >= startDate && tpt.TimeBlockDate.Date <= stopDate);
                    else
                        query = query.Where(tpt => !tpt.TimePeriodId.HasValue && tpt.TimeBlockDate.Date >= startDate && tpt.TimeBlockDate.Date <= stopDate);
                }
                else if (stage == STAGE_TIMEPERIOD)
                    query = query.Where(tpt => tpt.TimePeriodId.HasValue && tpt.TimePeriodId.Value == timePeriod.TimePeriodId);

                List<TimePayrollTransactionTreeDTO> transactions = null;

                if (onlyUseInPayroll)
                {
                    transactions = (from tpt in query
                                    select new TimePayrollTransactionTreeDTO
                                    {
                                        Id = tpt.TimePayrollTransactionId, //Without this unique id EF returns duplicates
                                        EmployeeId = tpt.EmployeeId,
                                        TimeCodeTransactionId = tpt.TimeCodeTransactionId,
                                        ProductId = tpt.ProductId,
                                        AttestStateId = tpt.AttestStateId,
                                        TimePeriodId = tpt.TimePeriodId,
                                        TimeBlockDateId = tpt.TimeBlockDateId,
                                        UnionFeeId = tpt.UnionFeeId,
                                        EmployeeVehicleId = tpt.EmployeeVehicleId,
                                        RetroactivePayrollOutcomeId = tpt.RetroactivePayrollOutcomeId,
                                        Quantity = tpt.Quantity,
                                        Amount = tpt.Amount,
                                        Date = tpt.TimeBlockDate.Date,
                                        SysPayrollTypeLevel1 = tpt.SysPayrollTypeLevel1,
                                        SysPayrollTypeLevel2 = tpt.SysPayrollTypeLevel2,
                                        SysPayrollTypeLevel3 = tpt.SysPayrollTypeLevel3,
                                        SysPayrollTypeLevel4 = tpt.SysPayrollTypeLevel4,
                                        IsAdded = tpt.IsAdded,
                                        IsFixed = tpt.IsFixed,
                                        IsCentRounding = tpt.IsCentRounding,
                                        IsQuantityRounding = tpt.IsQuantityRounding,
                                        IsAdditionOrDeduction = tpt.IsAdditionOrDeduction,
                                        PayrollStartValueRowId = tpt.PayrollStartValueRowId,
                                        PayrollProductUseInPayroll = tpt.PayrollProduct.UseInPayroll,
                                    }).ToList();

                    //Filter after db-execution
                    transactions = transactions.Where(tpt => tpt.PayrollProductUseInPayroll).ToList();
                }
                else
                {
                    if (includeAccounting)
                    {
                        transactions = (from tpt in query
                                        select new TimePayrollTransactionTreeDTO
                                        {
                                            Id = tpt.TimePayrollTransactionId, //Without this unique id EF returns duplicates
                                            EmployeeId = tpt.EmployeeId,
                                            TimeCodeTransactionId = tpt.TimeCodeTransactionId,
                                            ProductId = tpt.ProductId,
                                            AttestStateId = tpt.AttestStateId,
                                            TimePeriodId = tpt.TimePeriodId,
                                            TimeBlockDateId = tpt.TimeBlockDateId,
                                            UnionFeeId = tpt.UnionFeeId,
                                            EmployeeVehicleId = tpt.EmployeeVehicleId,
                                            RetroactivePayrollOutcomeId = tpt.RetroactivePayrollOutcomeId,
                                            Quantity = tpt.Quantity,
                                            Amount = tpt.Amount,
                                            Date = tpt.TimeBlockDate.Date,
                                            SysPayrollTypeLevel1 = tpt.SysPayrollTypeLevel1,
                                            SysPayrollTypeLevel2 = tpt.SysPayrollTypeLevel2,
                                            SysPayrollTypeLevel3 = tpt.SysPayrollTypeLevel3,
                                            SysPayrollTypeLevel4 = tpt.SysPayrollTypeLevel4,
                                            IsAdded = tpt.IsAdded,
                                            IsFixed = tpt.IsFixed,
                                            IsCentRounding = tpt.IsCentRounding,
                                            IsQuantityRounding = tpt.IsQuantityRounding,
                                            IsAdditionOrDeduction = tpt.IsAdditionOrDeduction,
                                            PayrollStartValueRowId = tpt.PayrollStartValueRowId,
                                            PayrollProductUseInPayroll = false,
                                            AccountInternalIds = tpt.AccountInternal.Select(i => i.AccountId).ToList(),
                                        }).ToList();
                    }
                    else
                    {
                        transactions = (from tpt in query
                                        select new TimePayrollTransactionTreeDTO
                                        {
                                            Id = tpt.TimePayrollTransactionId, //Without this unique id EF returns duplicates
                                            EmployeeId = tpt.EmployeeId,
                                            TimeCodeTransactionId = tpt.TimeCodeTransactionId,
                                            ProductId = tpt.ProductId,
                                            AttestStateId = tpt.AttestStateId,
                                            TimePeriodId = tpt.TimePeriodId,
                                            TimeBlockDateId = tpt.TimeBlockDateId,
                                            UnionFeeId = tpt.UnionFeeId,
                                            EmployeeVehicleId = tpt.EmployeeVehicleId,
                                            RetroactivePayrollOutcomeId = tpt.RetroactivePayrollOutcomeId,
                                            Quantity = tpt.Quantity,
                                            Amount = tpt.Amount,
                                            Date = tpt.TimeBlockDate.Date,
                                            SysPayrollTypeLevel1 = tpt.SysPayrollTypeLevel1,
                                            SysPayrollTypeLevel2 = tpt.SysPayrollTypeLevel2,
                                            SysPayrollTypeLevel3 = tpt.SysPayrollTypeLevel3,
                                            SysPayrollTypeLevel4 = tpt.SysPayrollTypeLevel4,
                                            IsAdded = tpt.IsAdded,
                                            IsFixed = tpt.IsFixed,
                                            IsCentRounding = tpt.IsCentRounding,
                                            IsQuantityRounding = tpt.IsQuantityRounding,
                                            IsAdditionOrDeduction = tpt.IsAdditionOrDeduction,
                                            PayrollStartValueRowId = tpt.PayrollStartValueRowId,
                                            PayrollProductUseInPayroll = false,
                                        }).ToList();
                    }
                }

                transactionsToReturn.AddRange(transactions);
            }

            transactionsToReturn = transactionsToReturn.Where(t => !t.PayrollStartValueRowId.HasValue).ToList();
            if (!employeeIds.IsNullOrEmpty() && !useEmployeesInQuery)
                transactionsToReturn = transactionsToReturn.Where(t => employeeIds.Contains(t.EmployeeId)).ToList();

            return transactionsToReturn;
        }
        
        public List<TimePayrollTransaction> GetTimePayrollTransactionsWithPlanningPeriodCalculationId(CompEntities entities, List<int> employeeIds, int planningPeriodCalculationId, int actorCompanyId)
        {
            return (from t in entities.TimePayrollTransaction
                    .Include("PayrollProduct")
                    where employeeIds.Contains(t.EmployeeId) &&
                    t.ActorCompanyId == actorCompanyId &&
                    t.PlanningPeriodCalculationId.HasValue &&
                    t.PlanningPeriodCalculationId.Value == planningPeriodCalculationId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }
        
        public List<T> FilterTimePayrollTransactions<T>(List<T> transactionItems, Employee employee, DateTime startDate, DateTime stopDate, List<DateTime> employmentDates = null, bool skipFilterOnAccounts = false) where T : IPayrollTransaction
        {
            return FilterTimePayrollTransactions(transactionItems, null, employee, startDate, stopDate, employmentDates, skipFilterOnAccounts);
        }

        public List<T> FilterTimePayrollTransactions<T>(List<T> transactionItems, EmployeeAuthModelRepository repository, Employee employee, DateTime startDate, DateTime stopDate, List<DateTime> employmentDates = null, bool skipFilterOnAccounts = false) where T : IPayrollTransaction
        {
            if (transactionItems.IsNullOrEmpty() || employee == null)
                return new List<T>();

            if (employmentDates == null)
                employmentDates = employee.GetEmploymentDates(startDate, stopDate);

            List<T> transactionItemsEmployee = transactionItems.Where(i => i.EmployeeId == employee.EmployeeId && (i.TimePeriodId.HasValue || employmentDates.Contains(i.Date))).ToList();

            if (!skipFilterOnAccounts && !transactionItems.IsNullOrEmpty() && repository is AccountRepository)
            {
                AccountRepository accountRepository = repository as AccountRepository;
                List<EmployeeAccount> employeeAccounts = accountRepository?.GetEmployeeAccounts(employee.EmployeeId);
                List<AccountDTO> validAccounts = employeeAccounts.GetValidAccounts(employee.EmployeeId, startDate, stopDate, accountRepository?.AllAccountInternalsDict, accountRepository?.GetAccountsDict(true), onlyDefaultAccounts: true);
                List<DateTime> validDates = employeeAccounts.GetValidDates(employee.EmployeeId, validAccounts?.Select(i => i.AccountId).ToList(), startDate, stopDate);

                if (validDates.IsNullOrEmpty())
                    transactionItemsEmployee.Clear();
                else
                    transactionItemsEmployee = transactionItemsEmployee.Where(i => validDates.Contains(i.Date)).ToList();
            }

            return transactionItemsEmployee;
        }

        public TimePayrollTransaction GetTimePayrollTransactionWithAccountStd(int timePayrollTransactionId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return GetTimePayrollTransactionWithAccountStd(entities, timePayrollTransactionId, actorCompanyId);
        }

        public TimePayrollTransaction GetTimePayrollTransactionWithAccountStd(CompEntities entities, int timePayrollTransactionId, int actorCompanyId)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include("AccountStd.Account.AccountDim")
                    where t.TimePayrollTransactionId == timePayrollTransactionId &&
                    t.ActorCompanyId == actorCompanyId
                    select t).FirstOrDefault();
        }

        public TimePayrollTransaction GetTimePayrollTransactionDiscardState(CompEntities entities, int timePayrollTransactionId)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.TimePayrollTransactionId == timePayrollTransactionId
                    select t).FirstOrDefault();
        }

        public TimePayrollTransaction CreateTimePayrollTransaction(CompEntities entities, int actorCompanyId, PayrollProduct product, ProjectTimeBlock projectTimeBlock, int worktimeQuantity, int accountId, int attestStateId, string comment = null, int amount = 0, int vatAmount = 0)
        {
            if (product == null || projectTimeBlock == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
            {
                Quantity = worktimeQuantity,
                Amount = amount,
                VatAmount = vatAmount,
                Comment = comment,
                SysPayrollTypeLevel1 = product.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = product.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = product.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = product.SysPayrollTypeLevel4,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = projectTimeBlock.EmployeeId,
                AccountStdId = accountId,
                AttestStateId = attestStateId,
                TimeBlockDateId = projectTimeBlock.TimeBlockDateId,

                //References
                PayrollProduct = product,
            };
            SetCreatedProperties(timePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollTransaction);

            return timePayrollTransaction;
        }

        public TimePayrollTransaction CreateTimePayrollTransaction(CompEntities entities, int actorCompanyId, PayrollProduct product, ProjectInvoiceDay day, TimeBlockDate timeBlockDate, int employeeId, int accountId, int attestStateId, string comment = null, int amount = 0, int vatAmount = 0)
        {
            if (product == null || day == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
            {
                Quantity = day.WorkTimeInMinutes,
                Amount = amount,
                VatAmount = vatAmount,
                Comment = comment,
                SysPayrollTypeLevel1 = product.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = product.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = product.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = product.SysPayrollTypeLevel4,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                AccountStdId = accountId,
                AttestStateId = attestStateId,

                //References
                PayrollProduct = product,
                TimeBlockDate = timeBlockDate,
            };
            SetCreatedProperties(timePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollTransaction);

            return timePayrollTransaction;
        }

        public TimePayrollTransaction CreateTimePayrollTransaction(CompEntities entities, int actorCompanyId, PayrollProduct product, decimal workTimeInMinutes, TimeBlockDate timeBlockDate, int employeeId, int accountId, int attestStateId, string comment = null, decimal amount = 0, decimal vatAmount = 0, List<AccountInternalDTO> accountInternals = null)
        {
            if (product == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
            {
                Quantity = workTimeInMinutes,
                Amount = amount,
                VatAmount = vatAmount,
                Comment = comment,
                SysPayrollTypeLevel1 = product.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = product.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = product.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = product.SysPayrollTypeLevel4,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                AccountStdId = accountId,
                AttestStateId = attestStateId,

                //References
                PayrollProduct = product,
                TimeBlockDate = timeBlockDate,
            };

            if (!accountInternals.IsNullOrEmpty())
            {
                if (timePayrollTransaction.AccountInternal == null)
                    timePayrollTransaction.AccountInternal = new EntityCollection<AccountInternal>();

                foreach (var ai in accountInternals)
                {
                    AccountInternal accountInternal = AccountManager.GetAccountInternal(entities, ai.AccountId, actorCompanyId);
                    if (accountInternal != null)
                        timePayrollTransaction.AccountInternal.Add(accountInternal);
                }
            }

            SetCreatedProperties(timePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollTransaction);

            return timePayrollTransaction;
        }

        public List<int> GetEmployeeWithScheduleWithoutTransactions(CompEntities entities, DateTime startDate, DateTime stopDate, Dictionary<int, List<TimePayrollTransactionTreeDTO>> transactionsByEmployee, List<int> employeeIds = null)
        {
            return GetEmployeesAndDatesWithScheduleWithoutTransactions(entities, startDate, stopDate, transactionsByEmployee, employeeIds).Select(i => i.Key).Distinct().ToList();
        }

        public Dictionary<int, List<DateTime>> GetEmployeesAndDatesWithScheduleWithoutTransactions(CompEntities entities, DateTime startDate, DateTime stopDate, Dictionary<int, List<TimePayrollTransactionTreeDTO>> transactionsByEmployee, List<int> employeeIds = null)
        {
            Dictionary<int, List<DateTime>> result = new Dictionary<int, List<DateTime>>();

            if (stopDate < startDate)
                return result;

            List<TimeScheduleTemplateBlockSmallDTO> scheduleBlocks = GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entities, base.ActorCompanyId, employeeIds, startDate, stopDate), TimeScheduleManager.GetTimeScheduleTemplateBlocksSmall);
            if (scheduleBlocks.IsNullOrEmpty())
                return result;

            scheduleBlocks = scheduleBlocks.Where(e => e.EmployeeId.HasValue && e.Date.HasValue && !e.IsBreak && !e.TimeScheduleScenarioHeadId.HasValue).ToList();
            foreach (var scheduleBlocksByEmployee in scheduleBlocks.GroupBy(i => i.EmployeeId.Value))
            {
                int employeeId = scheduleBlocksByEmployee.Key;
                List<DateTime> transactionDates = transactionsByEmployee.GetValue(employeeId)?.Select(i => i.Date).Distinct().ToList() ?? new List<DateTime>();

                List<DateTime> scheduleWithoutTransactiondates = new List<DateTime>();
                foreach (var scheduleBlocksByEmployeeAndDate in scheduleBlocksByEmployee.GroupBy(i => i.Date.Value))
                {
                    DateTime date = scheduleBlocksByEmployeeAndDate.Key;
                    if (!transactionDates.Any(i => i.Date == date) && CalendarUtility.IsBeforeNow(date, scheduleBlocksByEmployeeAndDate.Max(i => i.StopTime)))
                        scheduleWithoutTransactiondates.Add(date);
                }

                if (!scheduleWithoutTransactiondates.IsNullOrEmpty() && !result.ContainsKey(employeeId))
                    result.Add(employeeId, scheduleWithoutTransactiondates);
            }

            return result;
        }

        public List<int> GetEmployeesWithTimeStampsWithoutTransactions(CompEntities entities, DateTime startDate, DateTime stopDate, Dictionary<int, List<TimePayrollTransactionTreeDTO>> transactionsByEmployee, List<int> employeeIds)
        {
            return GetEmployeesAndDatesWithTimeStampsWithoutTransactions(entities, startDate, stopDate, transactionsByEmployee, employeeIds).Select(i => i.Key).ToList();
        }

        public Dictionary<int, List<DateTime>> GetEmployeesAndDatesWithTimeStampsWithoutTransactions(CompEntities entities, DateTime startDate, DateTime stopDate, Dictionary<int, List<TimePayrollTransactionTreeDTO>> transactionsByEmployee, List<int> employeeIds)
        {
            Dictionary<int, List<DateTime>> result = new Dictionary<int, List<DateTime>>();

            if (employeeIds.IsNullOrEmpty() || stopDate < startDate)
                return result;

            List<EmployeeDatesDTO> timeStampEntryDatesByEmployee = GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entities, base.ActorCompanyId, employeeIds, startDate, stopDate), TimeStampManager.GetTimeStampEntryDatesByEmployee);
            if (timeStampEntryDatesByEmployee.IsNullOrEmpty())
                return result;

            foreach (EmployeeDatesDTO timeStampEntryDates in timeStampEntryDatesByEmployee)
            {
                List<DateTime> transactionDates = transactionsByEmployee.GetValue(timeStampEntryDates.EmployeeId)?.Select(i => i.Date).ToList() ?? new List<DateTime>();

                List<DateTime> timeStampWithoutTransactionsDates = new List<DateTime>();
                foreach (DateTime date in timeStampEntryDates.Dates)
                {
                    if (!transactionDates.Any(i => i.Date == date))
                        timeStampWithoutTransactionsDates.Add(date);
                }

                if (!timeStampWithoutTransactionsDates.IsNullOrEmpty() && !result.ContainsKey(timeStampEntryDates.EmployeeId))
                    result.Add(timeStampEntryDates.EmployeeId, timeStampWithoutTransactionsDates);
            }

            return result;
        }

        public int[] GetPayrollLockedAttestStateIds(CompEntities entities)
        {
            return new int[]
            {
                SettingManager.GetCompanyIntSetting(entities, CompanySettingType.SalaryExportPayrollResultingAttestStatus),
                SettingManager.GetCompanyIntSetting(entities, CompanySettingType.SalaryPaymentLockedAttestStateId),
                SettingManager.GetCompanyIntSetting(entities, CompanySettingType.SalaryPaymentApproved1AttestStateId),
                SettingManager.GetCompanyIntSetting(entities, CompanySettingType.SalaryPaymentApproved2AttestStateId),
                SettingManager.GetCompanyIntSetting(entities, CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId),
            };
        }

        public bool HasTimeStampsWithoutTransactions<T>(int employeeId, EmployeeDatesDTO timeStampEntryDates, List<T> transactions) where T : IPayrollTransaction
        {
            if (!timeStampEntryDates.HasDates())
                return false;

            var transactionsEmployee = transactions?.Where(i => i.EmployeeId == employeeId).ToList();
            if (transactionsEmployee.IsNullOrEmpty())
                return true;

            foreach (DateTime date in timeStampEntryDates.Dates)
            {
                if (!transactionsEmployee.Any(i => i.Date == date))
                    return true;
            }

            return false;
        }

        public bool HasTimePayrollTransactions(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return (from t in entities.TimePayrollTransaction
                    where t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).Any();
        }

        public bool HasEmployeeTimePayrollTransactions(int employeeId, DateTime dateFrom)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.State == (int)SoeEntityState.Active &&
                    t.TimeBlockDate.Date >= dateFrom
                    select t).Any();
        }

        public bool HasEmployeeEarningTimePayrollTransactions(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.State == (int)SoeEntityState.Active &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    t.EarningTimeAccumulatorId.HasValue
                    select t).Any();
        }

        public ActionResult DeleteTimePayrollTransaction(int timePayrollTransactionId, bool deleteChilds)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    TimePayrollTransaction timePayrollTransaction = GetTimePayrollTransactionDiscardState(entities, timePayrollTransactionId);
                    if (timePayrollTransaction != null)
                    {
                        ChangeEntityState(timePayrollTransaction, SoeEntityState.Deleted);

                        if (deleteChilds)
                        {
                            List<TimePayrollTransaction> chainedTransactions = new List<TimePayrollTransaction>();
                            List<TimePayrollTransaction> timePayrollTransactionsForDate = GetTimePayrollTransactionsForEmployee(entities, timePayrollTransaction.EmployeeId, timePayrollTransaction.TimeBlockDateId);
                            timePayrollTransactionsForDate.GetChain(timePayrollTransaction, chainedTransactions);

                            foreach (TimePayrollTransaction childTransaction in chainedTransactions)
                            {
                                ChangeEntityState(childTransaction, SoeEntityState.Deleted);
                            }
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
            }

            return result;
        }

        public ActionResult SetLevelOnTransactionsFromPayrollProduction(int actorCompanyId, int payrollProductId, DateTime startDate, DateTime stopDate)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();
                    entities.CommandTimeout = 1200;

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var transactions = entities.TimePayrollTransaction.Where(w => w.ActorCompanyId == actorCompanyId && w.ProductId == payrollProductId && w.TimeBlockDate.Date >= startDate && w.TimeBlockDate.Date <= stopDate).ToList();
                        var product = ProductManager.GetPayrollProduct(entities, payrollProductId);

                        if (product != null)
                        {
                            foreach (var item in transactions.Where(w => w.SysPayrollTypeLevel1.ToString() + w.SysPayrollTypeLevel2.ToString() + w.SysPayrollTypeLevel3.ToString() + w.SysPayrollTypeLevel4.ToString() !=
                                                                          product.SysPayrollTypeLevel1.ToString() + product.SysPayrollTypeLevel2.ToString() + product.SysPayrollTypeLevel3.ToString() + product.SysPayrollTypeLevel4.ToString()))
                            {
                                item.SysPayrollTypeLevel1 = product.SysPayrollTypeLevel1;
                                item.SysPayrollTypeLevel2 = product.SysPayrollTypeLevel2;
                                item.SysPayrollTypeLevel3 = product.SysPayrollTypeLevel3;
                                item.SysPayrollTypeLevel4 = product.SysPayrollTypeLevel4;
                            }
                        }

                        entities.SaveChanges();
                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                    result.Value = 0;
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        #region TimePayrollStatisticsSmallDTO

        public List<TimePayrollStatisticsSmallDTO> GetTimePayrollStatisticsSmallDTOs_new(CompEntities entities, int actorCompanyId, List<Employee> employees, List<int> timePeriodIds, int sysCountryId = (int)TermGroup_Country.SE, List<TimePayrollTransaction> filteredTransactions = null, bool applyEmploymentTaxMinimumRule = false, List<EmploymentTaxTimePeriodHeadItemDTO> employmentTaxTimePeriodItems = null, bool setPensionCompany = false, bool ignoreAccounting = false, bool isAgd = false)
        {
            #region Init

            List<TimePayrollStatisticsSmallDTO> dtos = new List<TimePayrollStatisticsSmallDTO>();

            if (!timePeriodIds.Any() || !employees.Any())
                return dtos;

            #endregion

            #region Prereq

            List<Byte[]> zippedDTOs = new List<byte[]>();
            bool zipToSaveMemory = false;

            #endregion

            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
            List<AccountDTO> allAccountDTOs = !ignoreAccounting ? AccountManager.GetAccountsByCompany(actorCompanyId, loadAccount: true, loadAccountDim: true).ToDTOs(includeAccountDim: true, includeInternalAccounts: true) : null;

            if (employees.Count > 50)
            {
                payrollProducts = ProductManager.GetPayrollProducts(entities, actorCompanyId, null, true, true, true, true);

                if (timePeriodIds.Count > 4)
                    zipToSaveMemory = true;
            }

            List<int> employeeIds = employees.Select(e => e.EmployeeId).ToList();
            List<EmploymentTypeDTO> employmentTypes = setPensionCompany ? EmployeeManager.GetEmploymentTypes(entities, actorCompanyId) : new List<EmploymentTypeDTO>();
            List<PayrollPriceType> payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, actorCompanyId, null, false);
            List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId, true, true, true, true);
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, actorCompanyId, true, true, true, true, true, false);
            List<AnnualLeaveGroup> annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(entities, actorCompanyId);
            List<EmployeeTimePeriod> employeeTimePeriods = entities.EmployeeTimePeriod.Include("EmployeeTimePeriodValue").Include("EmployeeTimePeriodProductSetting").Where(w => timePeriodIds.Contains(w.TimePeriodId)).ToList();
            List<int> distinctEmployeeIdsInTimePeriods = employeeTimePeriods.Select(s => s.EmployeeId).Distinct().ToList();
            employees = employees.Where(W => distinctEmployeeIdsInTimePeriods.Contains(W.EmployeeId)).ToList();
            Dictionary<int, List<int>> validEmployeesForTimePeriods = PayrollManager.GetValidEmployeesForTimePeriod(entities, actorCompanyId, timePeriodIds, employees, payrollGroups, true, employeeTimePeriods);

            DateTime validTo = DateTime.UtcNow.AddSeconds(employees.Count * timePeriodIds.Count);
            ExtensionCache.Instance.AddToEmployeePayrollGroupExtensionCaches(actorCompanyId, employeeGroups, payrollGroups, payrollPriceTypes, annualLeaveGroups, validTo);

            entities.CommandTimeout = 600;
            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(timePeriodIds, actorCompanyId);

            try
            {
                int count = 1;
                foreach (int timePeriodId in timePeriodIds)
                {
                    TimePeriod timePeriod = timePeriods.FirstOrDefault(t => t.TimePeriodId == timePeriodId);
                    if (timePeriod == null)
                        continue;

                    if (!timePeriod.PaymentDate.HasValue)
                        continue;

                    if (timePeriodIds.Count * employeeIds.Count > 500)
                        LogInfo($"GetTimePayrollStatisticsSmallDTOs_new period {count} of {timePeriodIds.Count} started PaymentDate: {timePeriod.PaymentDate} head: {timePeriod.TimePeriodHead.Name} zippedFiles: {zippedDTOs.Count}");

                    count++;

                    List<Employee> validEmployees = new List<Employee>();
                    List<PayrollCalculationProductDTO> payrollCalculationProductDTOs = new List<PayrollCalculationProductDTO>();
                    decimal employmentTax = PayrollManager.GetSysPayrollPriceIntervalAmount(actorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, 1977, timePeriod.PaymentDate.Value);

                    foreach (Employee employee in employees)
                    {
                        if (validEmployeesForTimePeriods.Any(v => v.Key == timePeriodId && v.Value.Contains(employee.EmployeeId)))
                            validEmployees.Add(employee);
                    }

                    var employeePeriods = employeeTimePeriods.Where(w => w.TimePeriodId == timePeriodId && validEmployees.Select(s => s.EmployeeId).Contains(w.EmployeeId)).ToList();

                    if (applyEmploymentTaxMinimumRule)
                    {
                        List<int> validemployeeIds = validEmployees.Select(e => e.EmployeeId).ToList();

                        if (validemployeeIds.Any())
                        {
                            employmentTaxTimePeriodItems = TimePeriodManager.GetEmploymentTaxTimePeriodHeadDTOs(entities, actorCompanyId, timePeriod.PaymentDate.Value.Year, validemployeeIds);

                            payrollCalculationProductDTOs.AddRange(TimeTreePayrollManager.GetPayrollCalculationProducts(
                                entities,
                                actorCompanyId,
                                timePeriod,
                                validEmployees,
                                showAllTransactions: true,
                                applyEmploymentTaxMinimumRule: true,                                
                                isAgd: isAgd,
                                employmentTaxTimePeriodItems: employmentTaxTimePeriodItems,
                                employeeTimePeriods: employeePeriods,
                                employeeGroups: employeeGroups
                             ));
                        }
                    }
                    else
                    {
                        payrollCalculationProductDTOs.AddRange(TimeTreePayrollManager.GetPayrollCalculationProducts(
                            entities,
                            actorCompanyId,
                            timePeriodId,
                            validEmployees,
                            showAllTransactions: true,
                            applyEmploymentTaxMinimumRule: false,
                            ignoreAccounting: ignoreAccounting,
                            employeeTimePeriods: employeePeriods,
                            allAccountDTOs: allAccountDTOs,
                            employeeGroups: employeeGroups
                        ));
                    }

                    List<AttestPayrollTransactionDTO> transactions = new List<AttestPayrollTransactionDTO>();
                    foreach (var payrollCalculationProductDTO in payrollCalculationProductDTOs)
                    {
                        transactions.AddRange(payrollCalculationProductDTO.AttestPayrollTransactions);
                    }

                    foreach (Employee employee in validEmployees)
                    {
                        #region Employee

                        List<AttestPayrollTransactionDTO> attestPayrollTransactions = new List<AttestPayrollTransactionDTO>();

                        if (!validEmployeesForTimePeriods.Any(v => v.Key == timePeriodId && v.Value.Contains(employee.EmployeeId)))
                            continue;

                        foreach (var payrollCalculationProductDTO in payrollCalculationProductDTOs.Where(p => p.EmployeeId == employee.EmployeeId))
                        {
                            attestPayrollTransactions.AddRange(payrollCalculationProductDTO.AttestPayrollTransactions);
                        }

                        if (!employee.Employment.IsLoaded)
                            employee.Employment.Load();

                        #region EmploymentTax

                        bool employmentTaxhasChanged = false;
                        decimal employmentTaxFactor = 1;

                        if (applyEmploymentTaxMinimumRule && employmentTaxTimePeriodItems != null && employmentTaxTimePeriodItems.Any(e => e.EmployeeId == employee.EmployeeId))
                        {
                            decimal employeeEmploymentTax = employmentTax;
                            var birthYear = EmployeeManager.GetEmployeeBirthDate(employee);
                            decimal grossSalarySumFromPrevoiusPeriods = 0;

                            var employmentTaxTimePeriodHeadDTO = employmentTaxTimePeriodItems.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);
                            if (employmentTaxTimePeriodHeadDTO != null)
                            {
                                if (!employmentTaxTimePeriodHeadDTO.IsEmploymentTaxMinimumLimitReached(employmentTaxTimePeriodHeadDTO.StartValueEmploymentTaxBasis))
                                    grossSalarySumFromPrevoiusPeriods += employmentTaxTimePeriodHeadDTO.StartValueEmploymentTaxBasis;

                                foreach (var period in employmentTaxTimePeriodHeadDTO.Periods.Where(p => p.PaymentDate < timePeriod.PaymentDate.Value).OrderBy(o => o.PaymentDate))
                                {

                                    if (employmentTaxTimePeriodHeadDTO.IsEmploymentTaxMinimumLimitReachedBeforeGivenPeriod(period.PaymentDate))
                                    {
                                        grossSalarySumFromPrevoiusPeriods = 0;
                                        break;
                                    }

                                    if (employmentTaxTimePeriodHeadDTO.IsEmploymentTaxMinimumLimitReachedIncludingGivenPeriod(period.PaymentDate, period.EmploymentTaxBasis))
                                    {
                                        if (period.TimePeriodId != timePeriodId)
                                            grossSalarySumFromPrevoiusPeriods = 0;

                                        break;
                                    }

                                    // In the loop, because in most cases we will not end up here anyway
                                    if (birthYear.HasValue)
                                        employeeEmploymentTax = PayrollManager.GetSysPayrollPriceIntervalAmount(actorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthYear.Value.Year, timePeriod.PaymentDate.Value);

                                    var employeeEmploymentTaxPeriod = employeeEmploymentTax;

                                    if (birthYear.HasValue)
                                        employeeEmploymentTaxPeriod = PayrollManager.GetSysPayrollPriceIntervalAmount(actorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthYear.Value.Year, period.PaymentDate);

                                    if (employeeEmploymentTaxPeriod != employmentTax && employeeEmploymentTaxPeriod != 0)
                                    {
                                        employmentTaxhasChanged = true;
                                        employmentTaxFactor = Decimal.Round(Decimal.Divide(employeeEmploymentTaxPeriod, employeeEmploymentTax), 4);
                                    }

                                    grossSalarySumFromPrevoiusPeriods += period.EmploymentTaxBasis;
                                }
                            }
                        }

                        #endregion

                        foreach (AttestPayrollTransactionDTO attestTransaction in attestPayrollTransactions)
                        {
                            if (attestTransaction != null)
                            {
                                #region PayrollProduct

                                PayrollProduct payrollProduct = null;
                                if (payrollProducts.Any(p => p.ProductId == attestTransaction.PayrollProductId))
                                {
                                    payrollProduct = payrollProducts.FirstOrDefault(p => p.ProductId == attestTransaction.PayrollProductId);
                                }
                                else
                                {
                                    payrollProduct = ProductManager.GetPayrollProduct(entities, attestTransaction.PayrollProductId, loadSettings: true);
                                    payrollProducts.Add(payrollProduct);
                                }

                                #endregion

                                #region Transaction

                                decimal amount = attestTransaction.Amount.HasValue ? (decimal)attestTransaction.Amount : 0;
                                decimal amountEntCurrency = attestTransaction.AmountEntCurrency ?? (attestTransaction.Amount ?? 0);
                                decimal amountCurrency = attestTransaction.AmountCurrency ?? 0;
                                if (attestTransaction.IsEmploymentTax() && attestTransaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods && employmentTaxhasChanged && employmentTaxFactor != 0)
                                {
                                    if (amount != 0)
                                        amount = Decimal.Round(Decimal.Divide(amount, employmentTaxFactor), 2);

                                    if (amountEntCurrency != 0)
                                        amountEntCurrency = Decimal.Round(Decimal.Divide(amountEntCurrency, employmentTaxFactor), 2);

                                    if (amountCurrency != 0)
                                        amountCurrency = Decimal.Round(Decimal.Divide(amountCurrency, employmentTaxFactor), 2);
                                }

                                TimePayrollStatisticsSmallDTO dto = new TimePayrollStatisticsSmallDTO();
                                dto.TransactionId = attestTransaction.TimePayrollTransactionId;
                                dto.IsScheduleTransaction = attestTransaction.IsScheduleTransaction;
                                dto.EmployeeName = employee.Name;
                                dto.EmployeeNr = employee.EmployeeNr;
                                dto.EmployeeId = employee.EmployeeId;
                                dto.TimePeriodId = timePeriod.TimePeriodId;
                                dto.Amount = amount;
                                dto.AmountEntCurrency = amountEntCurrency;
                                dto.AmountCurrency = amountCurrency;
                                dto.AmountLedgerCurrency = 0;
                                dto.VatAmount = attestTransaction.VatAmount ?? 0;
                                dto.VatAmountEntCurrency = attestTransaction.VatAmountEntCurrency ?? (attestTransaction.VatAmount ?? 0);
                                dto.VatAmountLedgerCurrency = 0;
                                dto.VatAmountCurrency = attestTransaction.VatAmountCurrency ?? 0;
                                dto.Quantity = attestTransaction.Quantity;
                                dto.Date = attestTransaction.Date;
                                dto.SysPayrollTypeLevel1 = attestTransaction.TransactionSysPayrollTypeLevel1;
                                dto.SysPayrollTypeLevel2 = attestTransaction.TransactionSysPayrollTypeLevel2;
                                dto.SysPayrollTypeLevel3 = attestTransaction.TransactionSysPayrollTypeLevel3;
                                dto.SysPayrollTypeLevel4 = attestTransaction.TransactionSysPayrollTypeLevel4;
                                dto.SysPayrollTypeLevel1Name = attestTransaction.TransactionSysPayrollTypeLevel1.HasValue ? GetText(attestTransaction.TransactionSysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                                dto.SysPayrollTypeLevel2Name = attestTransaction.TransactionSysPayrollTypeLevel2.HasValue ? GetText(attestTransaction.TransactionSysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                                dto.SysPayrollTypeLevel3Name = attestTransaction.TransactionSysPayrollTypeLevel3.HasValue ? GetText(attestTransaction.TransactionSysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                                dto.SysPayrollTypeLevel4Name = attestTransaction.TransactionSysPayrollTypeLevel4.HasValue ? GetText(attestTransaction.TransactionSysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                                dto.TimePeriodName = timePeriod.Name;

                                //TimePayrollTransactionExtended
                                dto.QuantityWorkDays = attestTransaction.QuantityWorkDays;
                                dto.QuantityCalendarDays = attestTransaction.QuantityCalendarDays;
                                dto.CalenderDayFactor = attestTransaction.CalenderDayFactor;

                                //PayrollProduct
                                dto.PayrollProductId = attestTransaction.PayrollProductId;
                                dto.PayrollProductNumber = attestTransaction.PayrollProductNumber;
                                dto.PayrollProductName = attestTransaction.PayrollProductName;
                                dto.PayrollProductDescription = payrollProduct.Description;
                                dto.ResultType = (TermGroup_PayrollResultType)payrollProduct.ResultType;

                                if (setPensionCompany)
                                {
                                    DateTime date = attestTransaction.Date;

                                    Employment employment = employee.GetEmployment(attestTransaction.Date);
                                    if (employment == null)
                                    {
                                        DateTime firstEmploymentDate = DateTime.Now.AddYears(-2);
                                        Employment firstEmployment = employee.GetFirstEmployment();
                                        if (firstEmployment == null)
                                            continue;

                                        if (firstEmployment.DateFrom.HasValue)
                                            firstEmploymentDate = firstEmployment.DateFrom.Value;

                                        DateTime? lastEmploymentDate = employee.GetLatestEmploymentDate(firstEmploymentDate, attestTransaction.Date);
                                        employment = employee.GetEmployment(lastEmploymentDate);

                                        if (employment == null && firstEmployment?.DateFrom != null && firstEmployment.DateFrom.Value > date)
                                        {
                                            employment = firstEmployment;
                                            date = firstEmployment.DateFrom.Value;
                                        }
                                        else if (lastEmploymentDate.HasValue)
                                            date = lastEmploymentDate.Value;
                                    }

                                    if (employment != null)
                                    {

                                        dto.EmploymentTypeDTO = employment.GetEmploymentTypeDTO(employmentTypes, date);

                                        //PayrollGroup
                                        PayrollGroup payrollGroup = employment.GetPayrollGroup(date, payrollGroups);
                                        if (payrollGroup != null)
                                        {
                                            PayrollProductSetting setting = payrollProduct.GetSetting(payrollGroup.PayrollGroupId);
                                            dto.PensionCompany = setting != null ? (TermGroup_PensionCompany)setting.PensionCompany : TermGroup_PensionCompany.NotSelected;

                                            PayrollGroupSetting kpaSetting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.KPAAgreementNumber);
                                            dto.KPAAgreementNumber = kpaSetting != null && !string.IsNullOrEmpty(kpaSetting.StrData) ? kpaSetting.StrData : string.Empty;

                                            PayrollGroupSetting kpaAgreementTypeSetting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.KPAAgreementType);
                                            dto.KPAAgreementType = kpaAgreementTypeSetting != null && !string.IsNullOrEmpty(kpaAgreementTypeSetting.StrData) ? kpaAgreementTypeSetting.StrData : string.Empty;

                                            PayrollGroupSetting foraSetting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.ForaCollectiveAgreement);
                                            dto.ForaCollectiveAgreementId = foraSetting != null && foraSetting.IntData.HasValue ? foraSetting.IntData.Value : 0;

                                            PayrollGroupSetting gtpSetting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.GTPAgreementNumber);
                                            dto.GTPAgreementNumber = gtpSetting?.IntData != null ? gtpSetting.IntData.Value : 0;
                                        }
                                        else
                                        {
                                            dto.PensionCompany = TermGroup_PensionCompany.NotSelected;
                                            dto.KPAAgreementNumber = string.Empty;
                                            dto.ForaCollectiveAgreementId = 0;
                                        }

                                        if (employee.GTPAgreementNumber != 0)
                                            dto.GTPAgreementNumber = employee.GTPAgreementNumber;
                                    }
                                }
                                else
                                {
                                    dto.PensionCompany = TermGroup_PensionCompany.NotSelected;
                                }

                                dto.IsBelowEmploymentTaxLimitRuleHidden = attestTransaction.IsBelowEmploymentTaxLimitRuleHidden;
                                dto.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods = attestTransaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods;
                                dto.IsEmploymentTaxAndHidden = attestTransaction.IsEmploymentTaxAndHidden;
                                dto.PaymentDate = timePeriod.PaymentDate;

                                dtos.Add(dto);

                                #endregion
                            }
                        }


                        if (dtos.Count > 8000 && zipToSaveMemory)
                        {
                            zippedDTOs.Add(ZipUtility.CompressString(JsonConvert.SerializeObject(dtos)));
                            dtos = new List<TimePayrollStatisticsSmallDTO>();
                        }

                        #endregion
                    }
                }

                if (zippedDTOs.Any())
                {
                    while (zippedDTOs.Any())
                    {
                        var item = zippedDTOs.First();
                        dtos.AddRange(JsonConvert.DeserializeObject<List<TimePayrollStatisticsSmallDTO>>(ZipUtility.UnzipString(item)));
                        zippedDTOs.Remove(item);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
            }

            return dtos;
        }

        public List<TimePayrollStatisticsDTO> GetPayrollStartValuesAsTimePayrollStatisticsDTOs(CompEntities entities, Employee employee, int actorCompanyId, int year, bool setPensionCompany = false, List<PayrollGroup> payrollGroups = null, List<EmployeeGroup> employeeGroups = null, List<int> skipTransactionIds = null, bool ignoreAccounting = false)
        {
            List<TimePayrollStatisticsDTO> timePayrollStatisticsDTOs = new List<TimePayrollStatisticsDTO>();
            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();
            List<Account> accountStds = new List<Account>();
            List<Account> accountInternals = new List<Account>();
            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, false, true, true).OrderBy(a => a.AccountDimNr).ToList();

            List<int> payrollStartValueRowIds = PayrollManager.GetStartValueRowIdsForYear(entities, actorCompanyId, new List<int>() { employee.EmployeeId }, year);
            if (payrollStartValueRowIds.Any())
            {
                if (!employee.Employment.IsLoaded)
                    employee.Employment.Load();

                List<TimePayrollTransaction> timePayrollTransactions = PayrollManager.GetTimePayrollTransactionsFromPayrollStartValueRowIds(entities, actorCompanyId, employee.EmployeeId, payrollStartValueRowIds);
                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                {
                    if (skipTransactionIds != null && skipTransactionIds.Contains(timePayrollTransaction.TimePayrollTransactionId))
                        continue;

                    DateTime date = timePayrollTransaction.TimeBlockDate.Date;
                    Employment employment = employee.GetEmployment(date);

                    TimePayrollStatisticsDTO timePayrollStatisticsDTO = new TimePayrollStatisticsDTO();
                    timePayrollStatisticsDTO.TimePayrollTransactionId = timePayrollTransaction.TimePayrollTransactionId;
                    timePayrollStatisticsDTO.EmployeeId = employee.EmployeeId;
                    timePayrollStatisticsDTO.Amount = timePayrollTransaction.Amount.HasValue ? (decimal)timePayrollTransaction.Amount : 0;
                    timePayrollStatisticsDTO.AmountEntCurrency = timePayrollTransaction.AmountEntCurrency ?? (timePayrollTransaction.Amount ?? 0);
                    timePayrollStatisticsDTO.AmountCurrency = timePayrollTransaction.AmountCurrency ?? 0;
                    timePayrollStatisticsDTO.AmountLedgerCurrency = 0;
                    timePayrollStatisticsDTO.VatAmount = timePayrollTransaction.VatAmount ?? 0;
                    timePayrollStatisticsDTO.VatAmountEntCurrency = timePayrollTransaction.VatAmountEntCurrency ?? (timePayrollTransaction.VatAmount ?? 0);
                    timePayrollStatisticsDTO.VatAmountLedgerCurrency = 0;
                    timePayrollStatisticsDTO.VatAmountCurrency = timePayrollTransaction.VatAmountCurrency ?? 0;
                    timePayrollStatisticsDTO.UnitPrice = timePayrollTransaction.UnitPrice ?? 0;
                    timePayrollStatisticsDTO.UnitPriceEntCurrency = timePayrollTransaction.UnitPriceEntCurrency ?? (timePayrollTransaction.UnitPrice ?? 0);
                    timePayrollStatisticsDTO.UnitPriceLedgerCurrency = 0;
                    timePayrollStatisticsDTO.UnitPriceCurrency = timePayrollTransaction.UnitPriceCurrency ?? 0;
                    timePayrollStatisticsDTO.Quantity = timePayrollTransaction.Quantity;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel1 = timePayrollTransaction.SysPayrollTypeLevel1;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel2 = timePayrollTransaction.SysPayrollTypeLevel2;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel3 = timePayrollTransaction.SysPayrollTypeLevel3;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel4 = timePayrollTransaction.SysPayrollTypeLevel4;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel1Name = timePayrollTransaction.SysPayrollTypeLevel1.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel2Name = timePayrollTransaction.SysPayrollTypeLevel2.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel3Name = timePayrollTransaction.SysPayrollTypeLevel3.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                    timePayrollStatisticsDTO.SysPayrollTypeLevel4Name = timePayrollTransaction.SysPayrollTypeLevel4.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                    timePayrollStatisticsDTO.ManuallyAdded = timePayrollTransaction.IsAdded;
                    timePayrollStatisticsDTO.AutoAttestFailed = false;
                    timePayrollStatisticsDTO.Exported = timePayrollTransaction.Exported;
                    timePayrollStatisticsDTO.IsEmploymentTaxBelowLimitHidden = false;
                    timePayrollStatisticsDTO.IsPreliminary = timePayrollTransaction.IsPreliminary;
                    timePayrollStatisticsDTO.Comment = timePayrollTransaction.Comment;
                    timePayrollStatisticsDTO.Created = timePayrollTransaction.Created;
                    timePayrollStatisticsDTO.CreatedBy = timePayrollTransaction.CreatedBy;
                    timePayrollStatisticsDTO.Modified = null;
                    timePayrollStatisticsDTO.ModifiedBy = string.Empty;
                    timePayrollStatisticsDTO.State = (SoeEntityState)0;

                    //TimePayrollTransactionExtended
                    timePayrollStatisticsDTO.QuantityWorkDays = timePayrollTransaction.TimePayrollTransactionExtended?.QuantityWorkDays ?? 0;
                    timePayrollStatisticsDTO.QuantityCalendarDays = timePayrollTransaction.TimePayrollTransactionExtended?.QuantityCalendarDays ?? 0;
                    timePayrollStatisticsDTO.CalenderDayFactor = timePayrollTransaction.TimePayrollTransactionExtended?.CalenderDayFactor ?? 0;
                    timePayrollStatisticsDTO.TimeUnit = timePayrollTransaction.TimePayrollTransactionExtended?.TimeUnit ?? 0;
                    timePayrollStatisticsDTO.Formula = timePayrollTransaction.TimePayrollTransactionExtended?.Formula ?? string.Empty;
                    timePayrollStatisticsDTO.FormulaExtracted = timePayrollTransaction.TimePayrollTransactionExtended?.FormulaExtracted ?? string.Empty;
                    timePayrollStatisticsDTO.FormulaNames = timePayrollTransaction.TimePayrollTransactionExtended?.FormulaNames ?? string.Empty;
                    timePayrollStatisticsDTO.FormulaOrigin = timePayrollTransaction.TimePayrollTransactionExtended?.FormulaOrigin ?? string.Empty;
                    timePayrollStatisticsDTO.FormulaPlain = timePayrollTransaction.TimePayrollTransactionExtended?.FormulaPlain ?? string.Empty;
                    timePayrollStatisticsDTO.PayrollCalculationPerformed = timePayrollTransaction.TimePayrollTransactionExtended?.PayrollCalculationPerformed ?? false;
                    timePayrollStatisticsDTO.IsDistributed = timePayrollTransaction.TimePayrollTransactionExtended?.IsDistributed ?? false;

                    //PayrollProduct
                    PayrollProduct payrollProduct = GetPayrollProductWithSettings(entities, timePayrollTransaction.ProductId, payrollProducts);
                    timePayrollStatisticsDTO.PayrollProductId = timePayrollTransaction.ProductId;
                    timePayrollStatisticsDTO.PayrollProductNumber = payrollProduct?.Number ?? string.Empty;
                    timePayrollStatisticsDTO.PayrollProductName = payrollProduct?.Name ?? string.Empty;
                    timePayrollStatisticsDTO.PayrollProductDescription = payrollProduct?.Description ?? string.Empty;
                    timePayrollStatisticsDTO.TimeCodeNumber = string.Empty; //TODO
                    timePayrollStatisticsDTO.TimeCodeName = string.Empty; //TODO
                    timePayrollStatisticsDTO.TimeCodeDescription = string.Empty; //TODO

                    //AttestState
                    timePayrollStatisticsDTO.AttestStateId = timePayrollTransaction.AttestStateId;
                    timePayrollStatisticsDTO.AttestStateName = string.Empty;

                    //TimeBlockDate
                    timePayrollStatisticsDTO.TimeBlockDate = timePayrollTransaction.TimeBlockDate.Date;
                    timePayrollStatisticsDTO.TimeBlockDateId = timePayrollTransaction.TimeBlockDateId;

                    //Other period
                    timePayrollStatisticsDTO.IsFromOtherPeriod = false; //TODO

                    //TimeBlock
                    timePayrollStatisticsDTO.TimeBlockId = timePayrollTransaction.TimeBlockId ?? 0;
                    timePayrollStatisticsDTO.TimeBlockStartTime = CalendarUtility.DATETIME_DEFAULT;
                    timePayrollStatisticsDTO.TimeBlockStopTime = CalendarUtility.DATETIME_DEFAULT;

                    //Employment
                    DateTime payrollGroupDate = date;
                    if (employment == null)
                    {
                        Employment firstEmployment = employee.GetFirstEmployment();
                        if (firstEmployment == null)
                            continue;

                        DateTime firstEmploymentDate = DateTime.Now.AddYears(-2);
                        if (firstEmployment.DateFrom.HasValue)
                            firstEmploymentDate = firstEmployment.DateFrom.Value;

                        DateTime? lastEmploymentDate = employee.GetLatestEmploymentDate(firstEmploymentDate, payrollGroupDate);

                        employment = employee.GetEmployment(lastEmploymentDate);

                        if (lastEmploymentDate.HasValue)
                            payrollGroupDate = lastEmploymentDate.Value;
                    }

                    if (employment == null)
                        continue;

                    //PayrollGroup
                    PayrollGroup payrollGroup = employment.GetPayrollGroup(payrollGroupDate, payrollGroups);
                    timePayrollStatisticsDTO.PayrollGroupName = payrollGroup?.Name ?? string.Empty;
                    timePayrollStatisticsDTO.PayrollGroupId = payrollGroup?.PayrollGroupId ?? 0;
                    timePayrollStatisticsDTO.WorkTimeWeek = employment?.GetWorkTimeWeek(date) ?? 0;

                    //EmployeeGroup
                    EmployeeGroup employeeGroup = employment?.GetEmployeeGroup(date, employeeGroups);
                    timePayrollStatisticsDTO.EmployeeGroupName = employeeGroup?.Name ?? string.Empty;
                    timePayrollStatisticsDTO.EmployeeGroupWorkTimeWeek = employeeGroup?.RuleWorkTimeWeek ?? 0;

                    //PensionCompany
                    if (payrollGroup != null)
                    {
                        PayrollProductSetting setting = payrollProduct.GetSetting(payrollGroup.PayrollGroupId);
                        timePayrollStatisticsDTO.PensionCompany = setting != null ? (TermGroup_PensionCompany)setting.PensionCompany : TermGroup_PensionCompany.NotSelected;
                    }
                    else
                        timePayrollStatisticsDTO.PensionCompany = TermGroup_PensionCompany.NotSelected;

                    //Accounting

                    Account accountStd = accountStds.FirstOrDefault(a => a.AccountId == timePayrollTransaction.AccountStd.AccountId);
                    if (accountStd == null)
                    {
                        accountStd = AccountManager.GetAccount(entities, actorCompanyId, timePayrollTransaction.AccountStd.AccountId, onlyActive: false);
                        if (accountStd != null)
                            accountStds.Add(accountStd);
                    }

                    timePayrollStatisticsDTO.Dim1Id = timePayrollTransaction.AccountStd.AccountId;
                    timePayrollStatisticsDTO.Dim1Name = timePayrollTransaction.AccountStd?.Account?.Name ?? string.Empty;
                    timePayrollStatisticsDTO.Dim1Nr = timePayrollTransaction.AccountStd?.Account?.AccountDim?.AccountDimNr.ToString() ?? "1";

                    #region AccountInternals

                    int dimCounter = 2;
                    if (!ignoreAccounting && timePayrollTransaction.AccountInternal.Any())
                    {
                        foreach (AccountDim accountDimInternal in accountDimInternals)
                        {
                            #region Dim 2

                            if (dimCounter == 2)
                            {
                                foreach (AccountInternal transactionAccountInternal in timePayrollTransaction.AccountInternal)
                                {
                                    Account account = GetAccountWithDim(entities, transactionAccountInternal.AccountId, actorCompanyId, accountInternals);
                                    if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                    {
                                        timePayrollStatisticsDTO.Dim2Id = transactionAccountInternal.AccountId;
                                        timePayrollStatisticsDTO.Dim2Name = account.Name;
                                        timePayrollStatisticsDTO.Dim2Nr = account.AccountNr;
                                        timePayrollStatisticsDTO.Dim2SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                    }
                                }
                            }

                            #endregion

                            #region Dim 3

                            if (dimCounter == 3)
                            {
                                foreach (AccountInternal transactionAccountInternal in timePayrollTransaction.AccountInternal)
                                {
                                    Account account = GetAccountWithDim(entities, transactionAccountInternal.AccountId, actorCompanyId, accountInternals);
                                    if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                    {
                                        timePayrollStatisticsDTO.Dim3Id = transactionAccountInternal.AccountId;
                                        timePayrollStatisticsDTO.Dim3Name = account.Name;
                                        timePayrollStatisticsDTO.Dim3Nr = account.AccountNr;
                                        timePayrollStatisticsDTO.Dim3SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                    }
                                }
                            }

                            #endregion

                            #region Dim 4

                            if (dimCounter == 4)
                            {
                                foreach (AccountInternal transactionAccountInternal in timePayrollTransaction.AccountInternal)
                                {
                                    Account account = GetAccountWithDim(entities, transactionAccountInternal.AccountId, actorCompanyId, accountInternals);
                                    if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                    {
                                        timePayrollStatisticsDTO.Dim4Id = transactionAccountInternal.AccountId;
                                        timePayrollStatisticsDTO.Dim4Name = account.Name;
                                        timePayrollStatisticsDTO.Dim4Nr = account.AccountNr;
                                        timePayrollStatisticsDTO.Dim4SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                    }
                                }
                            }

                            #endregion

                            #region Dim 5

                            if (dimCounter == 5)
                            {
                                foreach (AccountInternal transactionAccountInternal in timePayrollTransaction.AccountInternal)
                                {
                                    Account account = GetAccountWithDim(entities, transactionAccountInternal.AccountId, actorCompanyId, accountInternals);
                                    if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                    {
                                        timePayrollStatisticsDTO.Dim5Id = transactionAccountInternal.AccountId;
                                        timePayrollStatisticsDTO.Dim5Name = account.Name;
                                        timePayrollStatisticsDTO.Dim5Nr = account.AccountNr;
                                        timePayrollStatisticsDTO.Dim5SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                    }
                                }
                            }

                            #endregion

                            #region Dim 6

                            if (dimCounter == 6)
                            {
                                foreach (AccountInternal transactionAccountInternal in timePayrollTransaction.AccountInternal)
                                {
                                    Account account = GetAccountWithDim(entities, transactionAccountInternal.AccountId, actorCompanyId, accountInternals);
                                    if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                    {
                                        timePayrollStatisticsDTO.Dim6Id = transactionAccountInternal.AccountId;
                                        timePayrollStatisticsDTO.Dim6Name = account.Name;
                                        timePayrollStatisticsDTO.Dim6Nr = account.AccountNr;
                                        timePayrollStatisticsDTO.Dim6SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal?.SysSieDimNr : 0;
                                    }
                                }
                            }

                            #endregion

                            dimCounter++;
                        }
                    }

                    timePayrollStatisticsDTO.AccountString = timePayrollStatisticsDTO.Dim1Nr + ";" + timePayrollStatisticsDTO.Dim2Nr + ";" + timePayrollStatisticsDTO.Dim3Nr + ";" + timePayrollStatisticsDTO.Dim4Nr + ";" + timePayrollStatisticsDTO.Dim5Nr + ";" + timePayrollStatisticsDTO.Dim6Nr;

                    #endregion

                    timePayrollStatisticsDTO.IsPayrollStartValues = true;

                    timePayrollStatisticsDTOs.Add(timePayrollStatisticsDTO);

                }
            }

            return timePayrollStatisticsDTOs;
        }

        public List<TimePayrollStatisticsSmallDTO> GetPayrollStartValuesAsTimePayrollStatisticsSmallDTOs(Employee employee, int actorCompanyId, int year, bool setPensionCompany = false, List<PayrollGroup> payrollGroups = null, List<int> skipTransactionIds = null)
        {
            List<TimePayrollStatisticsSmallDTO> timePayrollStatisticsSmallDTOs = new List<TimePayrollStatisticsSmallDTO>();
            List<PayrollProduct> payrollProducts = new List<PayrollProduct>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<int> payrollStartValueRowIds = PayrollManager.GetStartValueRowIdsForYear(entitiesReadOnly, actorCompanyId, new List<int>() { employee.EmployeeId }, year);
            if (payrollStartValueRowIds.Any())
            {
                if (!employee.Employment.IsLoaded)
                    employee.Employment.Load();

                var timePayrollTransactions = PayrollManager.GetTimePayrollTransactionsFromPayrollStartValueRowIds(entitiesReadOnly, actorCompanyId, employee.EmployeeId, payrollStartValueRowIds);

                foreach (var timePayrollTransaction in timePayrollTransactions)
                {
                    if (skipTransactionIds != null && skipTransactionIds.Contains(timePayrollTransaction.TimePayrollTransactionId))
                        continue;

                    var date = timePayrollTransaction.TimeBlockDate.Date;
                    var employment = employee.GetEmployment(date);

                    TimePayrollStatisticsSmallDTO timePayrollStatisticsSmallDTO = new TimePayrollStatisticsSmallDTO();
                    timePayrollStatisticsSmallDTO.TransactionId = timePayrollTransaction.TimePayrollTransactionId;
                    timePayrollStatisticsSmallDTO.IsScheduleTransaction = false;
                    timePayrollStatisticsSmallDTO.EmployeeName = employee.Name;
                    timePayrollStatisticsSmallDTO.EmployeeNr = employee.EmployeeNr;
                    timePayrollStatisticsSmallDTO.EmployeeId = timePayrollTransaction.EmployeeId;
                    timePayrollStatisticsSmallDTO.Amount = timePayrollTransaction.Amount.HasValue ? (decimal)timePayrollTransaction.Amount : 0;
                    timePayrollStatisticsSmallDTO.AmountEntCurrency = timePayrollTransaction.AmountEntCurrency ?? (timePayrollTransaction.Amount ?? 0);
                    timePayrollStatisticsSmallDTO.AmountCurrency = timePayrollTransaction.AmountCurrency ?? 0;
                    timePayrollStatisticsSmallDTO.AmountLedgerCurrency = timePayrollTransaction.AmountLedgerCurrency ?? 0;
                    timePayrollStatisticsSmallDTO.VatAmount = timePayrollTransaction.VatAmount ?? 0;
                    timePayrollStatisticsSmallDTO.VatAmountEntCurrency = timePayrollTransaction.VatAmountEntCurrency ?? (timePayrollTransaction.VatAmount ?? 0);
                    timePayrollStatisticsSmallDTO.VatAmountLedgerCurrency = timePayrollTransaction.VatAmountLedgerCurrency ?? 0;
                    timePayrollStatisticsSmallDTO.VatAmountCurrency = timePayrollTransaction.VatAmountCurrency ?? 0;
                    timePayrollStatisticsSmallDTO.Quantity = timePayrollTransaction.Quantity;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel1 = timePayrollTransaction.SysPayrollTypeLevel1;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel2 = timePayrollTransaction.SysPayrollTypeLevel2;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel3 = timePayrollTransaction.SysPayrollTypeLevel3;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel4 = timePayrollTransaction.SysPayrollTypeLevel4;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel1Name = timePayrollTransaction.SysPayrollTypeLevel1.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel2Name = timePayrollTransaction.SysPayrollTypeLevel2.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel3Name = timePayrollTransaction.SysPayrollTypeLevel3.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                    timePayrollStatisticsSmallDTO.SysPayrollTypeLevel4Name = timePayrollTransaction.SysPayrollTypeLevel4.HasValue ? GetText(timePayrollTransaction.SysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType) : String.Empty;

                    timePayrollStatisticsSmallDTO.QuantityWorkDays = timePayrollTransaction.TimePayrollTransactionExtended != null ? timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays : 0;
                    timePayrollStatisticsSmallDTO.QuantityCalendarDays = timePayrollTransaction.TimePayrollTransactionExtended != null ? timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays : 0;
                    timePayrollStatisticsSmallDTO.CalenderDayFactor = timePayrollTransaction.TimePayrollTransactionExtended != null ? timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor : 0;

                    PayrollProduct payrollProduct = null;

                    if (payrollProducts.Any(p => p.ProductId == timePayrollTransaction.ProductId))
                        payrollProduct = payrollProducts.FirstOrDefault(p => p.ProductId == timePayrollTransaction.ProductId);
                    else
                    {
                        payrollProduct = ProductManager.GetPayrollProduct(entitiesReadOnly, timePayrollTransaction.ProductId, loadSettings: true);
                        payrollProducts.Add(payrollProduct);
                    }

                    timePayrollStatisticsSmallDTO.PayrollProductId = timePayrollTransaction.ProductId;
                    timePayrollStatisticsSmallDTO.PayrollProductNumber = payrollProduct != null ? payrollProduct.Number : String.Empty;
                    timePayrollStatisticsSmallDTO.PayrollProductName = payrollProduct != null ? payrollProduct.Name : String.Empty;
                    timePayrollStatisticsSmallDTO.PayrollProductDescription = payrollProduct != null ? payrollProduct.Description : String.Empty;
                    timePayrollStatisticsSmallDTO.IsPayrollStartValues = true;

                    timePayrollStatisticsSmallDTOs.Add(timePayrollStatisticsSmallDTO);


                    if (setPensionCompany)
                    {
                        DateTime pensionDate = date;

                        if (employment == null)
                        {
                            var firstEmployment = employee.GetFirstEmployment();
                            DateTime firstEmploymentDate = DateTime.Now.AddYears(-2);

                            if (firstEmployment == null)
                                continue;

                            if (firstEmployment.DateFrom.HasValue)
                                firstEmploymentDate = firstEmployment.DateFrom.Value;

                            DateTime? lastEmploymentDate = employee.GetLatestEmploymentDate(firstEmploymentDate, pensionDate);

                            employment = employee.GetEmployment(lastEmploymentDate);

                            if (lastEmploymentDate.HasValue)
                                pensionDate = lastEmploymentDate.Value;
                        }

                        if (employment == null)
                            continue;

                        //PayrollGroup
                        PayrollGroup payrollGroup = employment.GetPayrollGroup(pensionDate, payrollGroups);

                        if (payrollGroup != null)
                        {
                            var setting = payrollProduct.GetSetting(payrollGroup.PayrollGroupId);
                            var kpaSetting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.KPAAgreementNumber);
                            var foraSetting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.ForaCollectiveAgreement);

                            timePayrollStatisticsSmallDTO.PensionCompany = setting != null ? (TermGroup_PensionCompany)setting.PensionCompany : TermGroup_PensionCompany.NotSelected;
                            timePayrollStatisticsSmallDTO.KPAAgreementNumber = kpaSetting != null && !string.IsNullOrEmpty(kpaSetting.StrData) ? kpaSetting.StrData : string.Empty;
                            timePayrollStatisticsSmallDTO.ForaCollectiveAgreementId = foraSetting != null && foraSetting.IntData.HasValue ? foraSetting.IntData.Value : 0;
                            timePayrollStatisticsSmallDTO.PayrollGroupId = payrollGroup.PayrollGroupId;
                        }
                        else
                        {
                            timePayrollStatisticsSmallDTO.PensionCompany = TermGroup_PensionCompany.NotSelected;
                            timePayrollStatisticsSmallDTO.KPAAgreementNumber = string.Empty;
                            timePayrollStatisticsSmallDTO.ForaCollectiveAgreementId = 0;
                        }
                    }
                    else
                    {
                        timePayrollStatisticsSmallDTO.PensionCompany = TermGroup_PensionCompany.NotSelected;
                    }
                }
            }

            return timePayrollStatisticsSmallDTOs;
        }

        public List<TimePayrollStatisticsDTO> GetTimePayrollStatisticsDTOs(CompEntities entities, int actorCompanyId, List<Employee> employees, List<int> timePeriodIds, int sysCountryId = (int)TermGroup_Country.SE, List<TimePayrollTransaction> filteredTransactions = null, List<int> payrollProductIds = null, bool ignoreAccounting = false)
        {
            #region Init

            List<TimePayrollStatisticsDTO> dtos = new List<TimePayrollStatisticsDTO>();

            if (timePeriodIds.IsNullOrEmpty())
                return dtos;

            #endregion

            try
            {
                #region Prereq

                int nrOfTimePeriods = timePeriodIds.Count;
                ConcurrentBag<EmploymentCalenderDTO> employmentCalenderDTOs = new ConcurrentBag<EmploymentCalenderDTO>();
                List<AccountDim> accountDimInternals = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, false, true, true).OrderBy(a => a.AccountDimNr).ToList();
                List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId, true, true, true, true);
                List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, actorCompanyId, true, true, true, true, true, false);
                List<VacationGroup> vacationGroups = PayrollManager.GetVacationGroups(entities, actorCompanyId, false, false);
                List<PayrollPriceType> payrollPriceTypes = PayrollManager.GetPayrollPriceTypes(entities, actorCompanyId, null, false);
                List<AnnualLeaveGroup> annualLeaveGroups = AnnualLeaveManager.GetAnnualLeaveGroups(entities, actorCompanyId);
                List<PayrollProduct> payrollProducts = ProductManager.GetPayrollProducts(entities, actorCompanyId, null, true, true, true, true);
                List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(timePeriodIds, actorCompanyId);
                List<Account> accountInternals = AccountManager.GetAccountsByCompany(entities, actorCompanyId, onlyInternal: true);
                List<EmployeeTimePeriod> employeeTimePeriods = entities.EmployeeTimePeriod.Include("EmployeeTimePeriodValue").Include("EmployeeTimePeriodProductSetting").Where(w => timePeriodIds.Contains(w.TimePeriodId)).ToList();
                List<int> employeeIdsInTimePeriods = employeeTimePeriods.Select(s => s.EmployeeId).Distinct().ToList();
                employees = employees.Where(e => employeeIdsInTimePeriods.Contains(e.EmployeeId)).ToList();
                Dictionary<int, List<int>> validEmployeesForTimePeriods = PayrollManager.GetValidEmployeesForTimePeriod(entities, actorCompanyId, timePeriodIds, employees, payrollGroups, true, employeeTimePeriods);

                DateTime startDate = timePeriods.OrderBy(o => o.StartDate).First().StartDate.AddMonths(-2);
                DateTime stopDate = timePeriods.OrderBy(o => o.StopDate).Last().StartDate.AddMonths(2);
                DateTime validTo = DateTime.UtcNow.AddSeconds(employees.Count * timePeriodIds.Count * 2);
                if (validTo < DateTime.UtcNow.AddSeconds(300))
                    validTo = DateTime.UtcNow.AddSeconds(300);
                if (validTo > DateTime.UtcNow.AddSeconds(1500))
                    validTo = DateTime.UtcNow.AddSeconds(1500);
                ExtensionCache.Instance.AddToEmployeePayrollGroupExtensionCaches(actorCompanyId, employeeGroups, payrollGroups, payrollPriceTypes, annualLeaveGroups, validTo);

                Parallel.ForEach(employees, GetDefaultParallelOptions(), employee =>
                {
                    var items = EmployeeManager.GetEmploymentCalenderDTOs(employee, startDate, stopDate, employeeGroups, payrollGroups, payrollPriceTypes, vacationGroups, annualLeaveGroups: annualLeaveGroups);
                    items.ForEach(f => employmentCalenderDTOs.Add(f));
                    ExtensionCache.Instance.AddToEmploymentCalendarExtensionCaches(items, validTo);
                });

                entities.CommandTimeout = 600;

                #endregion

                int timePeriodCounter = 1;

                if (actorCompanyId == 701609)
                    LogCollector.LogInfo("GetTimePayrollStatisticsDTOs After AddToEmploymentCalendarExtensionCache");

                foreach (int timePeriodId in timePeriodIds)
                {
                    #region Prereq

                    TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, actorCompanyId);
                    if (timePeriod == null || !timePeriod.PaymentDate.HasValue)
                        continue;

                    List<Employee> validEmployees = new List<Employee>();
                    foreach (Employee employee in employees)
                    {
                        if (validEmployeesForTimePeriods.Any(v => v.Key == timePeriodId && v.Value.Contains(employee.EmployeeId)))
                            validEmployees.Add(employee);
                    }
                    if (validEmployees.IsNullOrEmpty())
                        continue;

                    List<int> remaingEmployeeIds = validEmployees.Select(s => s.EmployeeId).ToList();
                    List<EmployeeTimePeriod> employeeTimePeriodsForPeriod = employeeTimePeriods.Where(etp => etp.TimePeriodId == timePeriodId && remaingEmployeeIds.Contains(etp.EmployeeId)).ToList();

                    if (actorCompanyId == 701609)
                        LogCollector.LogInfo($"GetTimePayrollStatisticsDTOs Period {timePeriod.TimePeriodId} {timePeriod.Name} valid employees {remaingEmployeeIds.Count}");

                    List<PayrollCalculationProductDTO> payrollCalculationProductsForPeriod = TimeTreePayrollManager.GetPayrollCalculationProducts(
                        entities,
                        actorCompanyId,
                        timePeriodId,
                        validEmployees,
                        showAllTransactions: true,
                        applyEmploymentTaxMinimumRule: false,
                        ignoreAccounting: ignoreAccounting,
                        employeeTimePeriods: employeeTimePeriodsForPeriod,
                        employeeGroups: employeeGroups);

                    if (nrOfTimePeriods > 12 || actorCompanyId == 701609)
                        LogInfo($"GetTimePayrollStatisticsDTOs actorCompanyId: {actorCompanyId} period {timePeriodCounter} of {timePeriodIds.Count} started PaymentDate: {timePeriod.PaymentDate} head: {timePeriod.TimePeriodHead.Name}");
                    timePeriodCounter++;

                    #endregion

                    List<AttestPayrollTransactionDTO> attestPayrollTransactions = new List<AttestPayrollTransactionDTO>();
                    foreach (PayrollCalculationProductDTO payrollCalculationProductDTO in payrollCalculationProductsForPeriod)
                        attestPayrollTransactions.AddRange(payrollCalculationProductDTO.AttestPayrollTransactions);

                    var dict = attestPayrollTransactions.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, v => v.ToList());
                    var calenderDict = employmentCalenderDTOs.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, v => v.ToList());

                    foreach (Employee employee in validEmployees)
                    {
                        #region Employee

                        employee.TryLoadEmployments();
                        if (!dict.TryGetValue(employee.EmployeeId, out List<AttestPayrollTransactionDTO> attestPayrollTransactionsOnEmployee))
                            continue;

                        if (attestPayrollTransactionsOnEmployee.IsNullOrEmpty())
                            continue;

                        attestPayrollTransactionsOnEmployee = attestPayrollTransactionsOnEmployee.OrderBy(o => o.Date).ToList();

                        calenderDict.TryGetValue(employee.EmployeeId, out List<EmploymentCalenderDTO> employeeCalenderDTOs);
                        if (employeeCalenderDTOs.IsNullOrEmpty() && !attestPayrollTransactionsOnEmployee.Any())
                            continue;

                        if (employeeCalenderDTOs == null)
                            employeeCalenderDTOs = new List<EmploymentCalenderDTO>();

                        DateTime firstDate = attestPayrollTransactionsOnEmployee.First().Date;
                        DateTime LastDate = attestPayrollTransactionsOnEmployee.Last().Date;
                        employeeCalenderDTOs = employeeCalenderDTOs.Where(w => w.Date >= firstDate && w.Date <= LastDate).ToList();

                        bool isCoherentCalender = employeeCalenderDTOs.IsCoherent(firstDate, LastDate);
                        Employment employment = null;
                        EmployeeGroup employeeGroup = null;
                        PayrollGroup payrollGroup = null;
                        int workTimeWeek = 0;

                        foreach (var attestPayrollTransactionGroup in attestPayrollTransactionsOnEmployee.GroupBy(a => a.Date))
                        {
                            #region TimePayrollTransaction

                            if (attestPayrollTransactionGroup == null)
                                continue;

                            DateTime date = attestPayrollTransactionGroup.Key;
                            employment = isCoherentCalender && employment != null ? employment : employee.GetEmployment(date);
                            if (employment == null)
                            {
                                Employment firstEmployment = employee.GetFirstEmployment();
                                if (firstEmployment == null)
                                    continue;

                                DateTime firstEmploymentDate = firstEmployment.DateFrom ?? DateTime.Now.AddYears(-2);
                                if (firstEmploymentDate <= date)
                                {
                                    DateTime? lastEmploymentDate = employee.GetLatestEmploymentDate(firstEmploymentDate, date);
                                    if (lastEmploymentDate.HasValue)
                                        employment = employee.GetEmployment(lastEmploymentDate);
                                }

                                if (employment == null)
                                    employment = firstEmployment;
                            }

                            employeeGroup = isCoherentCalender && employeeGroup != null ? employeeGroup : (employment?.GetEmployeeGroup(date, employeeGroups) ?? null);
                            payrollGroup = isCoherentCalender && payrollGroup != null ? payrollGroup : (employment?.GetPayrollGroup(date, payrollGroups) ?? null);
                            workTimeWeek = isCoherentCalender && workTimeWeek != 0 ? workTimeWeek : employment?.GetWorkTimeWeek(date) ?? 0;

                            foreach (var attestPayrollTransaction in attestPayrollTransactionGroup)
                            {
                                if (!payrollProductIds.IsNullOrEmpty() && !payrollProductIds.Contains(attestPayrollTransaction.PayrollProductId))
                                    continue;

                                TimePayrollStatisticsDTO dto = new TimePayrollStatisticsDTO
                                {
                                    TimePayrollTransactionId = attestPayrollTransaction.TimePayrollTransactionId,
                                    EmployeeId = employee.EmployeeId,
                                    RetroactivePayrollOutcomeId = attestPayrollTransaction.RetroactivePayrollOutcomeId,
                                    Amount = attestPayrollTransaction.Amount.HasValue ? (decimal)attestPayrollTransaction.Amount : 0,
                                    AmountEntCurrency = attestPayrollTransaction.AmountEntCurrency ?? attestPayrollTransaction.Amount ?? 0,
                                    AmountCurrency = attestPayrollTransaction.AmountCurrency ?? 0,
                                    AmountLedgerCurrency = 0,
                                    VatAmount = attestPayrollTransaction.VatAmount ?? 0,
                                    VatAmountEntCurrency = attestPayrollTransaction.VatAmountEntCurrency ?? attestPayrollTransaction.VatAmount ?? 0,
                                    VatAmountLedgerCurrency = 0,
                                    VatAmountCurrency = attestPayrollTransaction.VatAmountCurrency ?? 0,
                                    UnitPrice = attestPayrollTransaction.UnitPrice ?? 0,
                                    UnitPriceEntCurrency = attestPayrollTransaction.UnitPriceEntCurrency ?? attestPayrollTransaction.UnitPrice ?? 0,
                                    UnitPriceLedgerCurrency = 0,
                                    UnitPriceCurrency = attestPayrollTransaction.UnitPriceCurrency ?? 0,
                                    Quantity = attestPayrollTransaction.Quantity,
                                    SysPayrollTypeLevel1 = attestPayrollTransaction.PayrollProductSysPayrollTypeLevel1,
                                    SysPayrollTypeLevel2 = attestPayrollTransaction.PayrollProductSysPayrollTypeLevel2,
                                    SysPayrollTypeLevel3 = attestPayrollTransaction.PayrollProductSysPayrollTypeLevel3,
                                    SysPayrollTypeLevel4 = attestPayrollTransaction.PayrollProductSysPayrollTypeLevel4,
                                    SysPayrollTypeLevel1Name = attestPayrollTransaction.TransactionSysPayrollTypeLevel1.HasValue ? GetText(attestPayrollTransaction.TransactionSysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType) : String.Empty,
                                    SysPayrollTypeLevel2Name = attestPayrollTransaction.TransactionSysPayrollTypeLevel2.HasValue ? GetText(attestPayrollTransaction.TransactionSysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType) : String.Empty,
                                    SysPayrollTypeLevel3Name = attestPayrollTransaction.TransactionSysPayrollTypeLevel3.HasValue ? GetText(attestPayrollTransaction.TransactionSysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : String.Empty,
                                    SysPayrollTypeLevel4Name = attestPayrollTransaction.TransactionSysPayrollTypeLevel4.HasValue ? GetText(attestPayrollTransaction.TransactionSysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType) : String.Empty,
                                    TimeCodeNumber = string.Empty,
                                    TimeCodeName = string.Empty,
                                    TimeCodeDescription = string.Empty,
                                    ManuallyAdded = attestPayrollTransaction.IsAdded,
                                    AutoAttestFailed = false,
                                    Exported = attestPayrollTransaction.IsExported,
                                    IsEmploymentTaxBelowLimitHidden = attestPayrollTransaction.IsBelowEmploymentTaxLimitRuleHidden,
                                    IsPreliminary = attestPayrollTransaction.IsPreliminary,
                                    IsScheduleTransaction = attestPayrollTransaction.IsScheduleTransaction,
                                    IsCentrounding = attestPayrollTransaction.IsCentRounding,
                                    ScheduleTransactionType = attestPayrollTransaction.ScheduleTransactionType,
                                    Comment = attestPayrollTransaction.Comment,
                                    Created = attestPayrollTransaction.Created,
                                    CreatedBy = attestPayrollTransaction.CreatedBy,
                                    Modified = attestPayrollTransaction.Modified,
                                    ModifiedBy = attestPayrollTransaction.ModifiedBy,
                                    State = SoeEntityState.Active,

                                    //TimePayrollTransactionExtended
                                    QuantityWorkDays = attestPayrollTransaction.QuantityWorkDays,
                                    QuantityCalendarDays = attestPayrollTransaction.QuantityCalendarDays,
                                    CalenderDayFactor = attestPayrollTransaction.CalenderDayFactor,
                                    TimeUnit = attestPayrollTransaction.TimeUnit,
                                    Formula = attestPayrollTransaction.Formula,
                                    FormulaExtracted = attestPayrollTransaction.FormulaExtracted,
                                    FormulaNames = attestPayrollTransaction.FormulaNames,
                                    FormulaOrigin = attestPayrollTransaction.FormulaOrigin,
                                    FormulaPlain = attestPayrollTransaction.FormulaPlain,
                                    PayrollCalculationPerformed = attestPayrollTransaction.PayrollCalculationPerformed == true,
                                    IsDistributed = attestPayrollTransaction.IsDistributed
                                };

                                //PayrollProduct
                                PayrollProduct payrollProduct = payrollProducts.FirstOrDefault(p => p.ProductId == attestPayrollTransaction.PayrollProductId);
                                if (payrollProduct == null)
                                {
                                    payrollProduct = ProductManager.GetPayrollProduct(entities, attestPayrollTransaction.PayrollProductId, loadSettings: true);
                                    if (payrollProduct != null)
                                        payrollProducts.Add(payrollProduct);
                                }
                                dto.PayrollProductId = attestPayrollTransaction.PayrollProductId;
                                dto.PayrollProductNumber = payrollProduct?.Number ?? String.Empty;
                                dto.PayrollProductName = payrollProduct?.Name ?? String.Empty;
                                dto.PayrollProductDescription = payrollProduct?.Description ?? String.Empty;
                                dto.ResultType = (TermGroup_PayrollResultType)payrollProduct.ResultType;

                                //AttestState
                                dto.AttestStateId = attestPayrollTransaction.AttestStateId;
                                dto.AttestStateName = attestPayrollTransaction.AttestStateName;

                                //TimeBlockDate
                                dto.TimeBlockDate = attestPayrollTransaction.Date;
                                dto.TimeBlockDateId = attestPayrollTransaction.TimeBlockDateId;

                                if (!dto.IsMonthlySalary())
                                    dto.IsFromOtherPeriod = dto.TimeBlockDate < timePeriod.StartDate;

                                dto.PaymentDate = timePeriod.PaymentDate;

                                //TimeBlock
                                dto.TimeBlockId = attestPayrollTransaction.TimeBlockId ?? 0;
                                dto.TimeBlockStartTime = attestPayrollTransaction.StartTime ?? CalendarUtility.DATETIME_DEFAULT;
                                dto.TimeBlockStopTime = attestPayrollTransaction.StopTime ?? CalendarUtility.DATETIME_DEFAULT;

                                //Employment
                                dto.WorkTimeWeek = workTimeWeek;

                                //EmployeeGroup
                                dto.EmployeeGroupName = employeeGroup?.Name ?? String.Empty;
                                dto.EmployeeGroupWorkTimeWeek = employeeGroup?.RuleWorkTimeWeek ?? 0;

                                //PayrollGroup
                                PayrollProductSetting setting = payrollGroup != null ? payrollProduct.GetSetting(payrollGroup.PayrollGroupId) : null;
                                dto.PayrollGroupName = payrollGroup?.Name ?? String.Empty;
                                dto.PayrollGroupId = payrollGroup?.PayrollGroupId ?? 0;
                                dto.PensionCompany = setting != null ? (TermGroup_PensionCompany)setting.PensionCompany : TermGroup_PensionCompany.NotSelected;

                                #region Accounting

                                //AccountStd
                                if (attestPayrollTransaction.AccountStd != null)
                                {
                                    dto.Dim1Id = attestPayrollTransaction.AccountStd.AccountId;
                                    dto.Dim1Name = attestPayrollTransaction.AccountStd.Name ?? "";
                                    dto.Dim1Nr = attestPayrollTransaction.AccountStd.AccountDimNr != 0 && !string.IsNullOrEmpty(attestPayrollTransaction.AccountStd.AccountNr) ? attestPayrollTransaction.AccountStd.AccountNr : "0";
                                }

                                //AccountInternals
                                int dimCounter = 2;
                                dto.AccountInternals = attestPayrollTransaction.AccountInternals?.Select(s => new AccountInternalDTO() { AccountId = s.AccountId, Account = s, AccountDimId = s.AccountDimId, AccountNr = s.AccountNr, Name = s.Name }).ToList();
                                if (attestPayrollTransaction.AccountInternals.Any())
                                {
                                    foreach (var accountDimInternal in accountDimInternals)
                                    {
                                        #region Dim 2

                                        if (dimCounter == 2)
                                        {
                                            foreach (AccountDTO transactionAccountInternal in attestPayrollTransaction.AccountInternals)
                                            {
                                                Account account = accountInternals.FirstOrDefault(a => a.AccountId == transactionAccountInternal.AccountId);
                                                if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                                {
                                                    dto.Dim2Id = transactionAccountInternal.AccountId;
                                                    dto.Dim2Name = account.Name;
                                                    dto.Dim2Nr = account.AccountNr;
                                                    dto.Dim2SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                                }
                                            }
                                        }

                                        #endregion

                                        #region Dim 3

                                        if (dimCounter == 3)
                                        {
                                            foreach (AccountDTO transactionAccountInternal in attestPayrollTransaction.AccountInternals)
                                            {
                                                Account account = accountInternals.FirstOrDefault(a => a.AccountId == transactionAccountInternal.AccountId);
                                                if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                                {
                                                    dto.Dim3Id = transactionAccountInternal.AccountId;
                                                    dto.Dim3Name = account.Name;
                                                    dto.Dim3Nr = account.AccountNr;
                                                    dto.Dim3SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                                }
                                            }
                                        }

                                        #endregion

                                        #region Dim 4

                                        if (dimCounter == 4)
                                        {
                                            foreach (AccountDTO transactionAccountInternal in attestPayrollTransaction.AccountInternals)
                                            {
                                                Account account = accountInternals.FirstOrDefault(a => a.AccountId == transactionAccountInternal.AccountId);
                                                if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                                {
                                                    dto.Dim4Id = transactionAccountInternal.AccountId;
                                                    dto.Dim4Name = account.Name;
                                                    dto.Dim4Nr = account.AccountNr;
                                                    dto.Dim4SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                                }
                                            }
                                        }

                                        #endregion

                                        #region Dim 5

                                        if (dimCounter == 5)
                                        {
                                            foreach (AccountDTO transactionAccountInternal in attestPayrollTransaction.AccountInternals)
                                            {
                                                Account account = accountInternals.FirstOrDefault(a => a.AccountId == transactionAccountInternal.AccountId);
                                                if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                                {
                                                    dto.Dim5Id = transactionAccountInternal.AccountId;
                                                    dto.Dim5Name = account.Name;
                                                    dto.Dim5Nr = account.AccountNr;
                                                    dto.Dim5SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                                }
                                            }
                                        }

                                        #endregion

                                        #region Dim 6

                                        if (dimCounter == 6)
                                        {
                                            foreach (AccountDTO transactionAccountInternal in attestPayrollTransaction.AccountInternals)
                                            {
                                                Account account = accountInternals.FirstOrDefault(a => a.AccountId == transactionAccountInternal.AccountId);
                                                if (account != null && account.AccountDimId == accountDimInternal.AccountDimId)
                                                {
                                                    dto.Dim6Id = transactionAccountInternal.AccountId;
                                                    dto.Dim6Name = account.Name;
                                                    dto.Dim6Nr = account.AccountNr;
                                                    dto.Dim6SIENr = accountDimInternal.SysSieDimNr != null ? (int)accountDimInternal.SysSieDimNr : 0;
                                                }
                                            }
                                        }

                                        #endregion

                                        dimCounter++;
                                    }
                                }
                                dto.AccountString = dto.Dim1Nr + ";" + dto.Dim2Nr + ";" + dto.Dim3Nr + ";" + dto.Dim4Nr + ";" + dto.Dim5Nr + ";" + dto.Dim6Nr;

                                #endregion

                                dtos.Add(dto);

                            }

                            #endregion
                        }

                        DateTime newValidTo = DateTime.UtcNow.AddSeconds(remaingEmployeeIds.Count * timePeriodIds.Count * 2);

                        if (validTo > DateTime.UtcNow.AddSeconds(-20) && validTo < DateTime.UtcNow)
                        {
                            ExtensionCache.Instance.ExtendEmploymentCalendarCache(remaingEmployeeIds, validTo, newValidTo);
                            ExtensionCache.Instance.ExtendEmployeePayrollGroupExtensionCache(actorCompanyId, validTo, newValidTo);
                            validTo = newValidTo;
                        }
                        else if (validTo < DateTime.UtcNow)
                            LogInfo($"GetTimePayrollStatisticsDTOs actorCompanyId: {actorCompanyId} period {timePeriodCounter} of {timePeriodIds.Count} cache has expired validTo UTC {validTo} UtcNow: {DateTime.UtcNow}");

                        remaingEmployeeIds.Remove(employee.EmployeeId);

                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new List<TimePayrollStatisticsDTO>();
            }

            return dtos;
        }

        #endregion

        #region PayrollVoucherHead

        public List<PayrollVoucherHeadDTO> GetTimePayrollVoucherHeadDTOs_new(CompEntities entities, int actorCompanyId, List<int> employeeIds, List<int> timePeriodIds, bool skipQuantity, bool merge = true, DateTime? voucherDate = null, List<int> includeAccountDimIds = null, bool excludeAccountingExport = false)
        {
            #region Init

            List<PayrollVoucherHeadDTO> payrollVoucherHeadDtos = new List<PayrollVoucherHeadDTO>();
            List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(entities, ActorCompanyId, employeeIds, loadEmployment: true);
            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(actorCompanyId, loadPriceTypes: true, loadTimePeriods: true);
            List<EmployeeTimePeriod> employeeTimePeriods = entities.EmployeeTimePeriod.Include("EmployeeTimePeriodValue").Include("EmployeeTimePeriodProductSetting").Where(w => timePeriodIds.Contains(w.TimePeriodId)).ToList();
            List<AccountInternalDTO> accountInternals = AccountManager.GetAccountInternals(actorCompanyId, null, false).ToDTOs();
            List<AccountDimDTO> accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(actorCompanyId));
            var accounts = AccountManager.GetAccountsByCompany(actorCompanyId, loadAccount: true).ToDTOs();
            Dictionary<int, List<int>> validEmployeesForTimePeriods = PayrollManager.GetValidEmployeesForTimePeriod(actorCompanyId, timePeriodIds, employees, payrollGroups, true, employeeTimePeriods);

            if (timePeriodIds == null)
                return payrollVoucherHeadDtos;

            #endregion

            #region Prereq

            List<AccountDim> accountDimInternals = AccountManager.GetAccountDimsByCompany(actorCompanyId, false, true, true).OrderBy(a => a.AccountDimNr).ToList();
            Dictionary<int, int?> sieDict = accountDimInternals.ToDictionary(o => o.AccountDimId, v => v.SysSieDimNr);

            if (includeAccountDimIds.IsNullOrEmpty())
                includeAccountDimIds = accountDimInternals.Select(s => s.AccountDimId).Distinct().ToList();

            #endregion

            foreach (int timePeriodId in timePeriodIds)
            {
                PayrollVoucherHeadDTO payrollVoucherHead = new PayrollVoucherHeadDTO();
                List<PayrollVoucherRowDTO> dtos = new List<PayrollVoucherRowDTO>();
                List<PayrollVoucherRowDTO> mergedDtos = new List<PayrollVoucherRowDTO>();
                List<PayrollCalculationProductDTO> payrollCalculationProductDTOs = new List<PayrollCalculationProductDTO>();
                TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(timePeriodId, actorCompanyId);
                List<Employee> validEmployees = new List<Employee>();
                List<PayrollProduct> payrollProducts = ProductManager.GetPayrollProducts(actorCompanyId, null);

                Dictionary<int, TermGroup_PayrollResultType> payrollProductsResultDict = payrollProducts.ToDictionary(k => k.ProductId, v => (TermGroup_PayrollResultType)v.ResultType);
                if (timePeriod == null)
                    continue;

                if (!timePeriod.PaymentDate.HasValue)
                    continue;

                foreach (var employee in employees)
                {
                    if (validEmployeesForTimePeriods.Any(v => v.Key == timePeriodId && v.Value.Contains(employee.EmployeeId)))
                        validEmployees.Add(employee);
                    else
                        continue;
                }

                int reasonableTimeout = Convert.ToInt32(decimal.Divide(validEmployees.Count(), 15));

                if (reasonableTimeout < 30)
                    reasonableTimeout = 30;

                int? orginalTimeout = entities.CommandTimeout;
                if (entities.CommandTimeout == null || entities.CommandTimeout < reasonableTimeout)
                    entities.CommandTimeout = reasonableTimeout;

                BatchHelper batchHelper = BatchHelper.Create(validEmployees.Select(s => s.EmployeeId).ToList(), 500);
                while (batchHelper.HasMoreBatches())
                {
                    payrollCalculationProductDTOs.AddRange(TimeTreePayrollManager.GetPayrollCalculationProducts(
                    entities,
                    actorCompanyId,
                    timePeriodId,
                    validEmployees.Where(x => batchHelper.GetCurrentBatchIds().Contains(x.EmployeeId)).ToList(),
                    showAllTransactions: true,
                    applyEmploymentTaxMinimumRule: true));

                    batchHelper.MoveToNextBatch();
                }

                foreach (var payrollCalculationProductDTO in payrollCalculationProductDTOs)
                {
                    foreach (var attestPayrollTransactionDTO in payrollCalculationProductDTO.AttestPayrollTransactions.Where(t => !t.IsEmploymentTaxAndHidden))
                    {
                        PayrollVoucherRowDTO rowDTO = new PayrollVoucherRowDTO();

                        //Ta ej med bruttolön från föregående period
                        if (attestPayrollTransactionDTO.IsEmploymentTaxBasis() && attestPayrollTransactionDTO.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods)
                            continue;

                        if (attestPayrollTransactionDTO != null)
                        {
                            #region TimePayrollTransaction

                            decimal quantity = attestPayrollTransactionDTO.Quantity;

                            if (quantity != 0 && payrollProductsResultDict.ContainsKey(attestPayrollTransactionDTO.PayrollProductId))
                            {
                                var type = payrollProductsResultDict.GetValue(attestPayrollTransactionDTO.PayrollProductId);

                                if (type == TermGroup_PayrollResultType.Time)
                                    quantity = decimal.Round(decimal.Divide(attestPayrollTransactionDTO.Quantity, 60), 2);
                            }

                            rowDTO.TimePayrollTransactionId = attestPayrollTransactionDTO.TimePayrollTransactionId;
                            rowDTO.TimePayrollScheduleTransactionId = 0;
                            rowDTO.Amount = attestPayrollTransactionDTO.Amount ?? 0;
                            rowDTO.AmountEntCurrency = attestPayrollTransactionDTO.AmountEntCurrency ?? (attestPayrollTransactionDTO.Amount ?? 0);
                            rowDTO.Quantity = skipQuantity ? 0 : quantity;
                            rowDTO.Text = employees.FirstOrDefault(e => e.EmployeeId == payrollCalculationProductDTO.EmployeeId)?.EmployeeNrAndName ?? string.Empty;
                            rowDTO.TimeCodeTransactionId = attestPayrollTransactionDTO.TimeCodeTransactionId;
                            rowDTO.SysPayrollTypeLevel1 = attestPayrollTransactionDTO.SysPayrollTypeLevel1;
                            rowDTO.SysPayrollTypeLevel2 = attestPayrollTransactionDTO.SysPayrollTypeLevel2;
                            rowDTO.SysPayrollTypeLevel3 = attestPayrollTransactionDTO.SysPayrollTypeLevel3;
                            rowDTO.SysPayrollTypeLevel4 = attestPayrollTransactionDTO.SysPayrollTypeLevel4;
                            #region NetSalary

                            if (attestPayrollTransactionDTO.IsNetSalaryPaid())
                            {
                                rowDTO.Amount = rowDTO.Amount * -1;
                                rowDTO.AmountEntCurrency = rowDTO.AmountEntCurrency * -1;
                            }

                            #endregion

                            #region Accounting

                            rowDTO.Dim1Id = attestPayrollTransactionDTO.AccountStd != null ? attestPayrollTransactionDTO.AccountStd.AccountId : 0;
                            rowDTO.Dim1Name = attestPayrollTransactionDTO.AccountStd != null && attestPayrollTransactionDTO.AccountStd.Name != null ? attestPayrollTransactionDTO.AccountStd.Name : "";
                            rowDTO.Dim1Nr = attestPayrollTransactionDTO.AccountStd != null && attestPayrollTransactionDTO.AccountStd.AccountNr != null ? attestPayrollTransactionDTO.AccountStd.AccountNr : "";



                            rowDTO.PayrollVoucherRowOriginDTOs.Add(new PayrollVoucherRowOriginDTO()
                            {
                                TimePayrollScheduleTransactionId = rowDTO.TimePayrollScheduleTransactionId,
                                TimePayrollTransactionAccountId = rowDTO.TimePayrollTransactionAccountId,
                                TimePayrollTransactionId = rowDTO.TimePayrollTransactionId,
                                EmployeeId = attestPayrollTransactionDTO.EmployeeId,
                                Amount = rowDTO.Amount,
                                Code = attestPayrollTransactionDTO.PayrollProductNumber,
                                DateTime = attestPayrollTransactionDTO.Date,
                                Quantity = skipQuantity ? 0 : rowDTO.Quantity.GetValueOrDefault(),
                            });

                            if (!accountInternals.IsNullOrEmpty() && !attestPayrollTransactionDTO.AccountInternals.IsNullOrEmpty())
                            {
                                foreach (var ai in attestPayrollTransactionDTO.AccountInternals.Where(w => includeAccountDimIds.Contains(w.AccountDimId)))
                                {
                                    sieDict.TryGetValue(ai.AccountDimId, out int? sieNr);

                                    rowDTO.AccountInternals.Add(new AccountInternalDTO()
                                    {
                                        AccountId = ai.AccountId,
                                        Account = ai,
                                        AccountDimId = ai.AccountDimId,
                                        AccountDimNr = ai.AccountDimNr,
                                        Name = ai.Name,
                                        SysSieDimNr = sieNr,
                                        AccountNr = ai.AccountNr,
                                    });
                                }
                            }

                            rowDTO.accountString = rowDTO.Dim1Nr + ";" + string.Join(";", rowDTO.AccountInternals.OrderBy(o => o.AccountDimNr).Select(s => s.AccountNr));

                            #endregion

                            dtos.Add(rowDTO);

                            #endregion
                        }
                    }
                }
                int nrOfDtos = dtos.Count;
                payrollVoucherHead.Rows = dtos;

                if (merge && payrollVoucherHead.Rows.Any())
                {
                    foreach (var accountGroup in payrollVoucherHead.Rows.GroupBy(r => r.accountString))
                    {
                        foreach (var group in accountGroup.GroupBy(r => r.SignGroup))
                        {
                            PayrollVoucherRowDTO mergedDto = new PayrollVoucherRowDTO();

                            mergedDto.TimePayrollTransactionId = 0;
                            mergedDto.Amount = group.Sum(r => r.Amount);
                            mergedDto.AmountEntCurrency = group.Sum(r => r.AmountEntCurrency);
                            mergedDto.Quantity = skipQuantity ? 0 : group.Sum(r => r.Quantity);
                            mergedDto.accountString = group.FirstOrDefault().accountString;
                            mergedDto.PayrollVoucherRowOriginDTOs = group.SelectMany(s => s.PayrollVoucherRowOriginDTOs).ToList();
                            mergedDto.AccountInternals = group.First().AccountInternals;
                            mergedDto.accountString = group.First().Dim1Nr + ";" + string.Join(";", group.First().AccountInternals.OrderBy(o => o.AccountDimNr).Select(s => s.AccountNr));
                            mergedDto.AccountDistributionName = group.FirstOrDefault(f => !string.IsNullOrEmpty(f.AccountDistributionName))?.AccountDistributionName;
                            #region Accounting

                            mergedDto.Dim1Id = group.FirstOrDefault() != null ? group.FirstOrDefault().Dim1Id : mergedDto.Dim1Id;
                            mergedDto.Dim1Name = group.FirstOrDefault() != null ? group.FirstOrDefault().Dim1Name : mergedDto.Dim1Name;
                            mergedDto.Dim1Nr = group.FirstOrDefault() != null ? group.FirstOrDefault().Dim1Nr : mergedDto.Dim1Nr;

                            #endregion

                            mergedDtos.Add(mergedDto);
                        }
                    }

                    mergedDtos = mergedDtos.Where(i => i != null).OrderBy(m => m.accountString).ToList();

                    payrollVoucherHead.Text = GetText(11880, "Verifikat skapat från Lön. Totalt antal transaktioner i underlaget: ") + nrOfDtos.ToString();
                    payrollVoucherHead.VoucherNr = 1;
                    payrollVoucherHead.Note = "";
                    payrollVoucherHead.Date = voucherDate ?? (timePeriod.PaymentDate ?? CalendarUtility.DATETIME_DEFAULT);
                    payrollVoucherHead.Rows = new List<PayrollVoucherRowDTO>();
                    payrollVoucherHead.Rows.AddRange(mergedDtos);

                    payrollVoucherHeadDtos.Add(payrollVoucherHead);

                }
                else
                {
                    dtos = payrollVoucherHead.Rows.Where(m => m != null).OrderBy(m => m.accountString).ToList();

                    payrollVoucherHead.Text = GetText(11880, "Verifikat skapat från Lön. Totalt antal transaktioner i underlaget: ") + nrOfDtos.ToString();
                    payrollVoucherHead.VoucherNr = 1;
                    payrollVoucherHead.Date = voucherDate ?? (timePeriod.PaymentDate ?? CalendarUtility.DATETIME_DEFAULT);
                    payrollVoucherHead.Note = "";
                    payrollVoucherHead.Rows = new List<PayrollVoucherRowDTO>();
                    payrollVoucherHead.Rows.AddRange(dtos);
                    payrollVoucherHeadDtos.Add(payrollVoucherHead);
                }

                VoucherManager.ApplyAutomaticAccountDistributionOnPayrollVoucherHead(entities, actorCompanyId, payrollVoucherHead, accounts, accountDims, accountInternals);
                nrOfDtos = payrollVoucherHead.Rows.Count;

                foreach (var rowDTO in payrollVoucherHead.Rows)
                {
                    int dimCounter = 2;

                    if (rowDTO.AccountInternals.Any())
                    {
                        foreach (var accountDim in accountDimInternals)
                        {
                            var accountInternal = rowDTO.AccountInternals.FirstOrDefault(i => i.AccountDimId == accountDim.AccountDimId);
                            if (accountInternal != null && (!excludeAccountingExport || (excludeAccountingExport && !accountDim.ExcludeinAccountingExport)))
                            {
                                #region Dim 2

                                if (dimCounter == 2)
                                {
                                    rowDTO.Dim2Id = accountInternal.AccountId;
                                    rowDTO.Dim2Name = accountInternal.Name;
                                    rowDTO.Dim2Nr = accountInternal.AccountNr;
                                    rowDTO.Dim2SIENr = accountDim.SysSieDimNr != null ? (int)accountDim.SysSieDimNr : 0;
                                }

                                #endregion

                                #region Dim 3

                                if (dimCounter == 3)
                                {
                                    rowDTO.Dim3Id = accountInternal.AccountId;
                                    rowDTO.Dim3Name = accountInternal.Name;
                                    rowDTO.Dim3Nr = accountInternal.AccountNr;
                                    rowDTO.Dim3SIENr = accountDim.SysSieDimNr != null ? (int)accountDim.SysSieDimNr : 0;
                                }

                                #endregion

                                #region Dim 4

                                if (dimCounter == 4)
                                {
                                    rowDTO.Dim4Id = accountInternal.AccountId;
                                    rowDTO.Dim4Name = accountInternal.Name;
                                    rowDTO.Dim4Nr = accountInternal.AccountNr;
                                    rowDTO.Dim4SIENr = accountDim.SysSieDimNr != null ? (int)accountDim.SysSieDimNr : 0;
                                }

                                #endregion

                                #region Dim 5

                                if (dimCounter == 5)
                                {
                                    rowDTO.Dim5Id = accountInternal.AccountId;
                                    rowDTO.Dim5Name = accountInternal.Name;
                                    rowDTO.Dim5Nr = accountInternal.AccountNr;
                                    rowDTO.Dim5SIENr = accountDim.SysSieDimNr != null ? (int)accountDim.SysSieDimNr : 0;
                                }

                                #endregion

                                #region Dim 6

                                if (dimCounter == 6)
                                {
                                    rowDTO.Dim6Id = accountInternal.AccountId;
                                    rowDTO.Dim6Name = accountInternal.Name;
                                    rowDTO.Dim6Nr = accountInternal.AccountNr;
                                    rowDTO.Dim6SIENr = accountDim.SysSieDimNr != null ? (int)accountDim.SysSieDimNr : 0;
                                }

                                #endregion
                            }

                            dimCounter++;
                        }
                    }
                    rowDTO.accountString = rowDTO.Dim1Nr + ";" + rowDTO.Dim2Nr + ";" + rowDTO.Dim3Nr + ";" + rowDTO.Dim4Nr + ";" + rowDTO.Dim5Nr + ";" + rowDTO.Dim6Nr;
                }

                if (entities.CommandTimeout != orginalTimeout)
                    entities.CommandTimeout = orginalTimeout;
            }

            return payrollVoucherHeadDtos;
        }

        #endregion

        #region GetTimePayrollTransactionsForCompany_Result

        public List<GetTimePayrollTransactionsForCompany_Result> GetTimePayrollTransactionsForCompany(CompEntities entities, DateTime? startDate, DateTime? stopDate, int? timePeriodId, int actorCompanyId, List<int> employeeIds = null)
        {
            return entities.GetTimePayrollTransactionsForCompany(actorCompanyId, employeeIds?.ToCommaSeparated(), startDate, stopDate, timePeriodId).ToList();
        }

        #endregion

        #region GetTimePayrollTransactionsForEmployee_Result

        public List<GetTimePayrollTransactionsForEmployee_Result> GetTimePayrollTransactionItemsForEmployees(CompEntities entities, int actorCompanyId, List<int> employeeIds, List<int> timePeriodIds, DateTime startDate, DateTime stopDate)
        {
            return entities.GetTimePayrollTransactionsForTimePeriodsCompany(employeeIds.ToCommaSeparated(), startDate, stopDate, timePeriodIds.ToCommaSeparated(), actorCompanyId).ToList();
        }

        public List<GetTimePayrollTransactionsForEmployee_Result> GetTimePayrollTransactionItemsForEmployees(CompEntities entities, string employeeIds, int timePeriodId, DateTime? startDate, DateTime? stopDate)
        {
            return entities.GetTimePayrollTransactionsForTimePeriodAndEmployees(employeeIds, startDate, stopDate, timePeriodId).ToList();
        }

        public List<GetTimePayrollTransactionsForEmployee_Result> GetTimePayrollTransactionItemsForEmployee(CompEntities entities, int employeeId, List<TimeBlockDate> timeBlockDates, int? timePeriodId = null)
        {
            if (timeBlockDates.IsNullOrEmpty())
                return new List<GetTimePayrollTransactionsForEmployee_Result>();

            return entities.GetTimePayrollTransactionsForEmployee(employeeId, timeBlockDates.Min(i => i.Date), timeBlockDates.Max(i => i.Date), timePeriodId).ToList();
        }

        public List<GetTimePayrollTransactionsForEmployee_Result> GetTimePayrollTransactionItemsForEmployee(CompEntities entities, int employeeId, TimePeriod timePeriod)
        {
            if (timePeriod == null)
                return new List<GetTimePayrollTransactionsForEmployee_Result>();

            return GetTimePayrollTransactionItemsForEmployee(entities, employeeId, timePeriod.TimePeriodId, timePeriod.StartDate, timePeriod.StopDate, timePeriod.ExtraPeriod);
        }

        public List<GetTimePayrollTransactionsForEmployee_Result> GetTimePayrollTransactionItemsForEmployee(CompEntities entities, int employeeId, int? timePeriodId, DateTime startDate, DateTime stopDate, bool isExtraPeriod)
        {
            if (isExtraPeriod)
                return entities.GetTimePayrollTransactionsForEmployee(employeeId, null, null, timePeriodId).ToList();
            else
                return entities.GetTimePayrollTransactionsForEmployee(employeeId, startDate, stopDate, timePeriodId).ToList();
        }

        public List<GetTimePayrollTransactionsForEmployee_Result> GetTimePayrollTransactionItemsForEmployee(CompEntities entities, int employeeId, DateTime? startDate, DateTime? stopDate, int? timePeriodId = null)
        {
            return entities.GetTimePayrollTransactionsForEmployee(employeeId, startDate, stopDate, timePeriodId).ToList();
        }

        #endregion

        #region GetTimePayrollTransactionAccountsForEmployee_Result

        public List<GetTimePayrollTransactionAccountsForEmployee_Result> GetTimePayrollTransactionAccountsForTimePeriodsCompany(CompEntities entities, DateTime startDate, DateTime stopDate, List<int> timePeriodIds, List<int> employeeIds, int actorCompanyId)
        {
            return entities.GetTimePayrollTransactionAccountsForTimePeriodsCompany(employeeIds.ToCommaSeparated(), startDate, stopDate, timePeriodIds.ToCommaSeparated(), actorCompanyId).ToList();
        }

        public List<GetTimePayrollTransactionAccountsForEmployee_Result> GetTimePayrollTransactionAccountsForTimePeriodAndEmployees(CompEntities entities, DateTime? startDate, DateTime? stopDate, int timePeriodId, string employeeIds)
        {
            return entities.GetTimePayrollTransactionAccountsForTimePeriodAndEmployees(employeeIds, startDate, stopDate, timePeriodId).ToList();
        }

        public List<GetTimePayrollTransactionAccountsForEmployee_Result> GetTimePayrollTransactionAccountsForEmployee(CompEntities entities, DateTime? startDate, DateTime? stopDate, int? timePeriodId, int employeeId, bool isExtraPeriod)
        {
            if (isExtraPeriod)
                return entities.GetTimePayrollTransactionAccountsForEmployee(employeeId, null, null, timePeriodId).ToList();
            else
                return entities.GetTimePayrollTransactionAccountsForEmployee(employeeId, startDate, stopDate, timePeriodId).ToList();
        }

        public List<GetTimePayrollTransactionAccountsForEmployee_Result> GetTimePayrollTransactionAccountsForEmployee(CompEntities entities, DateTime? startDate, DateTime? stopDate, int? timePeriodId, int employeeId)
        {
            return entities.GetTimePayrollTransactionAccountsForEmployee(employeeId, startDate, stopDate, timePeriodId).ToList();
        }

        #endregion

        #region GetTimePayrollTransactionsWithAccIntsForEmployee_Result

        public List<GetTimePayrollTransactionsWithAccIntsForEmployee_Result> GetTimePayrollTransactionsWithAccIntsForEmployee(CompEntities entities, DateTime startDate, DateTime stopDate, int timeScheduleTemplatePeriodId, int actorCompanyId, int employeeId)
        {
            return entities.GetTimePayrollTransactionsWithAccIntsForEmployee(employeeId, actorCompanyId, startDate, stopDate, timeScheduleTemplatePeriodId).ToList();
        }

        #endregion

        #region GetTimePayrollTransactionsWithAccIntsForProject_Result

        public List<GetTimePayrollTransactionsWithAccIntsForProject_Result> GetTimePayrollTransactionsWithAccIntsForProject(CompEntities entities, int projectId, int actorCompanyId)
        {
            return entities.GetTimePayrollTransactionsWithAccIntsForProject(projectId, actorCompanyId).ToList();
        }

        #endregion

        #endregion

        #region TimePayrollScheduleTransactions (external)

        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(List<int> timeScheduleTemplateBlockIds)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollScheduleTransaction.NoTracking();
            return GetTimePayrollScheduleTransactions(entities, timeScheduleTemplateBlockIds);
        }

        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(CompEntities entities, List<int> timeScheduleTemplateBlockIds)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                        .Include("PayrollProduct")
                    where t.Type == (int)SoeTimePayrollScheduleTransactionType.Schedule &&
                    t.TimeScheduleTemplateBlockId.HasValue &&
                    timeScheduleTemplateBlockIds.Contains(t.TimeScheduleTemplateBlockId.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsForEmployee(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo, SoeTimePayrollScheduleTransactionType? type = null)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    (!type.HasValue || t.Type == (int)type.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsForEmployeeNotLockedWithPayrollProduct(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo, SoeTimePayrollScheduleTransactionType? type = null)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    .Include("TimePeriod")
                    .Include("PayrollProduct")
                    .Include("TimeBlockDate")
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    (!type.HasValue || t.Type == (int)type.Value) &&
                    t.State == (int)SoeEntityState.Active &&
                    !t.EmployeeTimePeriodId.HasValue
                    select t).ToList();
        }
        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsForEmployee(CompEntities entities, int employeeId, List<DateTime> dates, SoeTimePayrollScheduleTransactionType? type = null)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where t.EmployeeId == employeeId &&
                    dates.Contains(t.TimeBlockDate.Date) &&
                    (!type.HasValue || t.Type == (int)type.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsForEmployee(CompEntities entities, int employeeId, List<int> timeBlockDateIds)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where t.EmployeeId == employeeId &&
                     timeBlockDateIds.Contains(t.TimeBlockDateId) &&
                     t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsForRetro(int retroactivePayrollOutcomeId, int employeeId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollScheduleTransaction.NoTracking();
            return GetTimePayrollScheduleTransactionsForRetro(entities, retroactivePayrollOutcomeId, employeeId, actorCompanyId);
        }

        public List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsForRetro(CompEntities entities, int retroactivePayrollOutcomeId, int employeeId, int actorCompanyId)
        {
            return (from tpt in entities.TimePayrollScheduleTransaction
                        .Include("PayrollProduct")
                        .Include("TimeBlockDate")
                        .Include("AccountStd.Account")
                        .Include("AccountInternal.Account")
                    where tpt.ActorCompanyId == actorCompanyId &&
                    tpt.EmployeeId == employeeId &&
                    tpt.RetroactivePayrollOutcomeId == retroactivePayrollOutcomeId &&
                    tpt.State == (int)SoeEntityState.Active
                    select tpt).ToList();
        }

        #region GetTimePayrollScheduleTransactionsForEmployee_Result

        public List<GetTimePayrollScheduleTransactionsForEmployee_Result> GetTimePayrollScheduleTransactionsForEmployee(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int? timePeriodId, int employeeId)
        {
            return entities.GetTimePayrollScheduleTransactionsForEmployee(employeeId, type, startDate, stopDate, timePeriodId).ToList();
        }

        public List<GetTimePayrollScheduleTransactionsForEmployee_Result> GetTimePayrollScheduleTransactionsForTimePeriodAndEmployees(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int timePeriodId, string employeeIds)
        {
            return entities.GetTimePayrollScheduleTransactionsForTimePeriodAndEmployees(employeeIds, type, startDate, stopDate, timePeriodId).ToList();
        }

        public List<GetTimePayrollScheduleTransactionsForEmployee_Result> GetTimePayrollScheduleTransactionsForTimePeriodsCompany(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, List<int> timePeriodIds, List<int> employeeIds, int actorCompanyId)
        {
            return entities.GetTimePayrollScheduleTransactionsForTimePeriodsCompany(employeeIds.ToCommaSeparated(), type, startDate, stopDate, timePeriodIds.ToCommaSeparated(), actorCompanyId).ToList();
        }

        #endregion

        #region GetTimePayrollScheduleTransactionAccountsForEmployee_Result

        public List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> GetTimePayrollScheduleTransactionAccountsForTimePeriodsCompany(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int actorCompanyId, List<int> timePeriodIds, List<int> employeeIds)
        {
            return GetTimePayrollScheduleTransactionAccountsForTimePeriodsCompany(entities, type, startDate, stopDate, actorCompanyId, timePeriodIds.ToCommaSeparated(), employeeIds.ToCommaSeparated());
        }

        public List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> GetTimePayrollScheduleTransactionAccountsForTimePeriodsCompany(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int actorCompanyId, string timePeriodIds, string employeeIds)
        {
            return entities.GetTimePayrollScheduleTransactionAccountsForTimePeriodsCompany(type, employeeIds, startDate, stopDate, timePeriodIds, actorCompanyId).ToList();
        }

        public List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> GetTimePayrollScheduleTransactionAccountsForTimePeriodAndEmployees(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int? timePerioId, List<int> employeeIds)
        {
            return GetTimePayrollScheduleTransactionAccountsForTimePeriodAndEmployees(entities, type, startDate, stopDate, timePerioId, employeeIds.ToCommaSeparated());
        }

        public List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> GetTimePayrollScheduleTransactionAccountsForTimePeriodAndEmployees(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int? timePerioId, string employeeIds)
        {
            return entities.GetTimePayrollScheduleTransactionAccountsForTimePeriodAndEmployees(employeeIds, type, startDate, stopDate, timePerioId).ToList();
        }

        public List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> GetTimePayrollScheduleTransactionAccountsForEmployee(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int? timePeriodId, int employeeId)
        {
            return entities.GetTimePayrollScheduleTransactionAccountsForEmployee(employeeId, type, startDate, stopDate, timePeriodId).ToList();
        }

        #endregion

        #region GetTimePayrollScheduleTransactionsWithAccIntsForEmployee_Result

        public List<GetTimePayrollScheduleTransactionsWithAccIntsForEmployee_Result> GetTimePayrollScheduleTransactionsWithAccIntsForEmployee(CompEntities entities, int? type, DateTime startDate, DateTime stopDate, int actorCompanyId, int employeeId)
        {
            return entities.GetTimePayrollScheduleTransactionsWithAccIntsForEmployee(employeeId, actorCompanyId, type, startDate, stopDate).ToList();
        }

        #endregion

        #endregion

        #region TimeCodeTransaction (internal)

        public List<TimeCodeTransaction> GetTimeCodeTransactionsForEmployee(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return (from t in entities.TimeCodeTransaction
                    where t.TimeBlockDate != null &&
                    t.TimeBlockDate.EmployeeId == employeeId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimeCodeTransaction> GetTimeCodeTransactionsForEmployee(CompEntities entities, int employeeId, List<DateTime> dates)
        {
            return (from t in entities.TimeCodeTransaction
                    where t.TimeBlockDate != null &&
                    t.TimeBlockDate.EmployeeId == employeeId &&
                    dates.Contains(t.TimeBlockDate.Date) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimeCodeTransaction> GetTimeCodeTransactionsForSupplierInvoice(CompEntities entities, int supplierInvoiceId, int actorCompanyId)
        {
            return (from t in entities.TimeCodeTransaction
                        .Include("TimeInvoiceTransaction")
                        .Include("TimeCode")
                    where t.TimeCode.ActorCompanyId == actorCompanyId &&
                    (t.SupplierInvoiceId != null && t.SupplierInvoiceId == supplierInvoiceId) &&
                    t.Type == (int)TimeCodeTransactionType.TimeProject &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimeCodeTransaction> GetTimeCodeTransactionsForProjectInvoiceDay(CompEntities entities, int projectInvoiceDayId)
        {
            return (from t in entities.TimeCodeTransaction
                    where t.ProjectInvoiceDayId == projectInvoiceDayId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public List<TimeCodeTransaction> GetTimeCodeTransactionsFromInvoiceRow(CompEntities entities, int customerInvoiceRowId)
        {
            return (from t in entities.TimeCodeTransaction
                    where t.CustomerInvoiceRowId == customerInvoiceRowId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        public TimeCodeTransaction GetTimeCodeTransaction(CompEntities entities, int timeCodeTransactionId)
        {
            return (from tct in entities.TimeCodeTransaction
                    where tct.TimeCodeTransactionId == timeCodeTransactionId
                    select tct).FirstOrDefault();
        }

        public TimeCodeTransaction CreateTimeCodeTransaction(CompEntities entities, int actorCompanyId, TimeCodeTransactionDTO timeCodeTransactionDTO, int? timeSheetWeekId = null)
        {
            Project project = null;

            if (timeCodeTransactionDTO.ProjectId != 0 && timeCodeTransactionDTO.ProjectId != null)
                project = ProjectManager.GetProject(entities, (int)timeCodeTransactionDTO.ProjectId);

            entities.Project.NoTracking();

            TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
            {
                Type = (int)TimeCodeTransactionType.TimeProject,
                Start = timeCodeTransactionDTO.Start,
                Stop = timeCodeTransactionDTO.Stop,
                TimeSheetWeekId = timeSheetWeekId,

                Comment = timeCodeTransactionDTO.Comment,
                Quantity = timeCodeTransactionDTO.Quantity,
                InvoiceQuantity = timeCodeTransactionDTO.InvoiceQuantity,
                Amount = timeCodeTransactionDTO.Amount,
                AmountCurrency = timeCodeTransactionDTO.AmountCurrency,
                AmountEntCurrency = timeCodeTransactionDTO.AmountEntCurrency,
                AmountLedgerCurrency = timeCodeTransactionDTO.AmountLedgerCurrency,
                VatCurrency = timeCodeTransactionDTO.VatCurrency,
                VatEntCurrency = timeCodeTransactionDTO.VatEntCurrency,
                VatLedgerCurrency = timeCodeTransactionDTO.VatLedgerCurrency,
                Vat = timeCodeTransactionDTO.Vat,
                Project = project,

                //Set FK
                TimeCodeId = timeCodeTransactionDTO.TimeCodeId,
                ProjectId = timeCodeTransactionDTO.ProjectId,
            };
            SetCreatedProperties(timeCodeTransaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction>();
            List<TimeInvoiceTransaction> timeInvoiceTransactions = new List<TimeInvoiceTransaction>();

            if (!timeCodeTransactionDTO.TimePayrollTransactionItems.IsNullOrEmpty())
            {
                foreach (var trans in timeCodeTransactionDTO.TimePayrollTransactionItems)
                {
                    if (trans.EmployeeId == 0 || trans.TimeBlockDateId == 0 || trans.PayrollProductId == 0)
                        continue;

                    #region AttestStateId

                    int attestStateId = 0;
                    if (trans.AttestStateId == 0)
                    {
                        List<AttestState> attestStates = AttestManager.GetAttestStates(actorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);

                        if (attestStates.Any(s => s.Initial))
                            attestStateId = attestStates.FirstOrDefault(s => s.Initial)?.AttestStateId ?? 0;
                        if (attestStateId == 0 && attestStates.Any())
                            attestStateId = attestStates.FirstOrDefault()?.AttestStateId ?? 0;
                        if (attestStateId == 0)
                            continue;
                    }

                    #endregion

                    #region Purchase account

                    int purchaseAccountId = 0;
                    if (trans.AccountId == 0)
                    {
                        purchaseAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeCost, 0, actorCompanyId, 0);
                        if (purchaseAccountId == 0)
                        {
                            List<AccountDTO> accountsBySearch = AccountManager.GetAccountStdBySearch(entities, actorCompanyId, "Lön", 1);
                            if (!accountsBySearch.IsNullOrEmpty())
                                purchaseAccountId = accountsBySearch.FirstOrDefault()?.AccountId ?? 0;
                        }
                    }

                    if (purchaseAccountId == 0)
                        continue;

                    trans.AccountId = purchaseAccountId;

                    #endregion

                    #region TimeBlockDate and PayrollProduct

                    TimeBlockDate timeblockDate = TimeBlockManager.GetTimeBlockDate(entities, trans.TimeBlockDateId, trans.EmployeeId);
                    PayrollProduct product = ProductManager.GetPayrollProduct(entities, trans.PayrollProductId);
                    if (timeblockDate == null || product == null)
                        continue;

                    #endregion

                    #region AccountInternals

                    List<AccountInternalDTO> accountInternals = new List<AccountInternalDTO>();
                    if (!trans.AccountInternals.IsNullOrEmpty())
                    {
                        foreach (AccountInternalDTO accountInternal in trans.AccountInternals)
                        {
                            accountInternals.Add(accountInternal);
                        }
                    }

                    AccountingPrioDTO accountingPrioDTO = AccountManager.GetPayrollProductAccount(ProductAccountType.Purchase, actorCompanyId, trans.EmployeeId, trans.PayrollProductId, timeCodeTransactionDTO.ProjectId != null ? (int)timeCodeTransactionDTO.ProjectId : 0, 0, true, (DateTime?)timeCodeTransactionDTO.Start);
                    if (accountingPrioDTO != null)
                    {
                        if (accountingPrioDTO.AccountId != null)
                            trans.AccountId = (int)accountingPrioDTO.AccountId;

                        if (accountInternals.IsNullOrEmpty() && !accountingPrioDTO.AccountInternals.IsNullOrEmpty())
                        {
                            foreach (AccountInternalDTO accountInternal in accountingPrioDTO.AccountInternals)
                            {
                                if (!accountInternals.Any(a => a.AccountId == accountInternal.AccountId))
                                    accountInternals.Add(accountInternal);
                            }
                        }
                    }

                    #endregion

                    #region TimePayrollTransaction

                    TimePayrollTransaction timePayrollTransaction = CreateTimePayrollTransaction(entities, actorCompanyId, product, trans.Quantity, timeblockDate, trans.EmployeeId, trans.AccountId, attestStateId, trans.Comment, trans.Amount, trans.VatAmount, accountInternals);
                    if (timePayrollTransaction != null)
                    {
                        entities.TimePayrollTransaction.AddObject(timePayrollTransaction);
                        timePayrollTransactions.Add(timePayrollTransaction);
                    }

                    #endregion
                }
            }

            if (!timeCodeTransactionDTO.TimeInvoiceTransactionItems.IsNullOrEmpty())
            {
                foreach (var trans in timeCodeTransactionDTO.TimeInvoiceTransactionItems)
                {
                    if (trans.EmployeeId == null || trans.EmployeeId == 0 || trans.InvoiceProductId == 0)
                        continue;
                    if ((!trans.TimeBlockDateId.HasValue || trans.TimeBlockDateId == 0))
                        continue;

                    #region AttestStateId

                    int attestStateId = 0;
                    if (!trans.AttestStateId.HasValue || trans.AttestStateId.Value == 0)
                    {
                        List<AttestState> attestStates = AttestManager.GetAttestStates(actorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                        if (attestStates.Any(s => s.Initial))
                            attestStateId = attestStates.FirstOrDefault(s => s.Initial)?.AttestStateId ?? 0;
                        if (attestStateId == 0 && attestStates.Any())
                            attestStateId = attestStates.FirstOrDefault()?.AttestStateId ?? 0;
                        if (attestStateId == 0)
                            continue;
                    }

                    #endregion

                    #region IncomeAccountId

                    int incomeAccountId = 0;
                    if (trans.AccountId == 0)
                    {
                        incomeAccountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountEmployeeCost, 0, actorCompanyId, 0);
                        if (incomeAccountId == 0)
                        {
                            List<AccountDTO> accountsBySearch = AccountManager.GetAccountStdBySearch(entities, actorCompanyId, "Lön", 1);
                            if (!accountsBySearch.IsNullOrEmpty())
                                incomeAccountId = accountsBySearch.FirstOrDefault()?.AccountId ?? 0;
                        }
                    }

                    if (incomeAccountId == 0)
                        continue;

                    trans.AccountId = incomeAccountId;

                    #endregion

                    #region Validate TimeBlockDate and PayrollProduct

                    TimeBlockDate timeblockDate = TimeBlockManager.GetTimeBlockDate(entities, (int)trans.TimeBlockDateId, (int)trans.EmployeeId);
                    InvoiceProduct invoiceProduct = ProductManager.GetInvoiceProduct(entities, trans.InvoiceProductId);
                    if (timeblockDate == null || invoiceProduct == null)
                        continue;

                    #endregion

                    #region AccountInternals

                    List<AccountInternalDTO> accountInternals = new List<AccountInternalDTO>();

                    if (!trans.AccountInternals.IsNullOrEmpty())
                    {
                        foreach (AccountInternalDTO accountInternal in trans.AccountInternals)
                        {
                            accountInternals.Add(accountInternal);
                        }
                    }

                    AccountingPrioDTO accountingPrioDTO = AccountManager.GetInvoiceProductAccount(actorCompanyId, trans.InvoiceProductId, timeCodeTransactionDTO.ProjectId != null ? (int)timeCodeTransactionDTO.ProjectId : 0, 0, (int)trans.EmployeeId, ProductAccountType.Sales, TermGroup_InvoiceVatType.Merchandise, true, false, (DateTime?)timeCodeTransactionDTO.Start);
                    if (accountingPrioDTO != null)
                    {
                        if (accountingPrioDTO.AccountId != null)
                            trans.AccountId = (int)accountingPrioDTO.AccountId;

                        if (accountInternals.IsNullOrEmpty() && !accountingPrioDTO.AccountInternals.IsNullOrEmpty())
                        {
                            foreach (AccountInternalDTO accountInternal in accountingPrioDTO.AccountInternals)
                            {
                                if (!accountInternals.Any(a => a.AccountId == accountInternal.AccountId))
                                    accountInternals.Add(accountInternal);
                            }
                        }
                    }

                    #endregion

                    #region TimeInvoiceTransaction

                    TimeInvoiceTransaction timeInvoiceTransaction = CreateTimeInvoiceTransaction(entities, actorCompanyId, invoiceProduct, timeblockDate, Convert.ToInt32(trans.Quantity), (int)trans.EmployeeId, trans.AccountId, attestStateId, Convert.ToInt32(trans.Amount), Convert.ToInt32(trans.Amount), accountInternals: accountInternals);
                    if (timeInvoiceTransaction != null)
                    {
                        entities.TimeInvoiceTransaction.AddObject(timeInvoiceTransaction);
                        timeInvoiceTransactions.Add(timeInvoiceTransaction);
                    }

                    #endregion
                }
            }

            if (timeInvoiceTransactions.Any())
                timeCodeTransaction.TimeInvoiceTransaction.AddRange(timeInvoiceTransactions);
            if (timePayrollTransactions.Any())
                timeCodeTransaction.TimePayrollTransaction.AddRange(timePayrollTransactions);

            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

            return timeCodeTransaction;
        }

        public TimeCodeTransaction CreateTimeCodeTransaction(CompEntities entities, int actorCompanyId, ProjectInvoiceDay day, ProjectInvoiceWeek week, string comment = null)
        {
            if (day == null || week == null || !week.TimeCodeId.HasValue)
                return null;

            TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
            {
                Type = (int)TimeCodeTransactionType.TimeProject,
                Start = CalendarUtility.DATETIME_DEFAULT,
                Stop = CalendarUtility.DATETIME_DEFAULT,
                Comment = comment,
                Quantity = day.WorkTimeInMinutes,
                InvoiceQuantity = day.InvoiceTimeInMinutes,
                Amount = 0,
                Vat = 0,

                //Set FK
                TimeCodeId = week.TimeCodeId.Value,
                ProjectId = week.ProjectId,
            };
            SetCreatedProperties(timeCodeTransaction);
            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            return timeCodeTransaction;
        }

        public TimeCodeTransaction CreateTimeCodeTransaction(CompEntities entities, int actorCompanyId, ProjectInvoiceDay day, int timeCodeId, int projectId, int amount = 0, int vat = 0)
        {
            if (day == null)
                return null;

            TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction
            {
                Type = (int)TimeCodeTransactionType.TimeProject,
                Start = CalendarUtility.DATETIME_DEFAULT,
                Stop = CalendarUtility.DATETIME_DEFAULT,
                Quantity = day.WorkTimeInMinutes,
                InvoiceQuantity = day.InvoiceTimeInMinutes,
                Amount = amount,
                Vat = vat,
                Comment = day.CommentExternal.NullToEmpty(),
                ExternalComment = day.Note.NullToEmpty(),

                //Set FK
                TimeCodeId = timeCodeId,
                ProjectId = projectId,
            };
            SetCreatedProperties(timeCodeTransaction);
            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            return timeCodeTransaction;
        }

        public TimeCodeTransaction CreateTimeCodeTransaction(CompEntities entities, int actorCompanyId, ProjectTimeBlock projectTimeBlock, int timeCodeId, int projectId, int amount = 0, int vat = 0)
        {
            if (projectTimeBlock == null)
                return null;

            var timeCodeTransaction = new TimeCodeTransaction
            {
                Type = (int)TimeCodeTransactionType.TimeProject,
                Start = projectTimeBlock.StartTime,
                Stop = projectTimeBlock.StopTime,
                Quantity = CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime),
                InvoiceQuantity = projectTimeBlock.InvoiceQuantity,
                Amount = amount,
                Vat = vat,
                Comment = projectTimeBlock.InternalNote,
                ExternalComment = projectTimeBlock.ExternalNote,

                //Set FK
                TimeCodeId = timeCodeId,
                ProjectId = projectId,
                ProjectTimeBlockId = projectTimeBlock.ProjectTimeBlockId
            };
            SetCreatedProperties(timeCodeTransaction);
            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            return timeCodeTransaction;
        }

        public TimeCodeTransaction CreateTimeCodeTransaction(CompEntities entities, int actorCompanyId, TimeCode timeCode, TimeBlock timeBlock, string comment = "", int quantity = 0, int invoiceQuantity = 0, int amount = 0, int vat = 0)
        {
            TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
            {
                Type = (int)TimeCodeTransactionType.Time,
                Start = timeBlock != null ? timeBlock.StartTime : CalendarUtility.DATETIME_DEFAULT,
                Stop = timeBlock != null ? timeBlock.StopTime : CalendarUtility.DATETIME_DEFAULT,
                Comment = comment,
                Quantity = quantity,
                InvoiceQuantity = invoiceQuantity,
                Amount = amount,
                Vat = vat,

                //References
                TimeCode = timeCode,
                TimeBlock = timeBlock,
            };
            SetCreatedProperties(timeCodeTransaction);
            entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

            //Set currency amounts
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

            return timeCodeTransaction;
        }

        public ActionResult CreateTimeCodeTransaction(CompEntities entities, int actorCompanyId, CustomerInvoiceRow invoiceRow, Project project, int standardMaterialCodeId, string comment = null, int quantity = 0, int invoiceQuantity = 0)
        {
            if (!invoiceRow.ProductId.HasValue || invoiceRow.IsTimeProjectRow || invoiceRow.IsCentRoundingRow || invoiceRow.IsInterestRow || invoiceRow.IsReminderRow)
                return new ActionResult(true);

            InvoiceProduct product = ProductManager.GetInvoiceProduct(entities, invoiceRow.ProductId.Value);
            if (product == null)
                return new ActionResult((int)ActionResultSave.ProductNotFound, GetText(8331));

            if (product.TimeCodeId.HasValue || standardMaterialCodeId > 0)
            {
                TimeCodeTransaction timeCodeTransaction = new TimeCodeTransaction()
                {
                    Type = (int)TimeCodeTransactionType.InvoiceProduct,
                    Start = CalendarUtility.DATETIME_DEFAULT,
                    Stop = CalendarUtility.DATETIME_DEFAULT,
                    Comment = comment,
                    Quantity = quantity,
                    InvoiceQuantity = invoiceQuantity,

                    //Set FK
                    TimeCodeId = product.TimeCodeId ?? standardMaterialCodeId,

                    //References
                    CustomerInvoiceRow = invoiceRow,
                    Project = project,
                };
                SetCreatedProperties(timeCodeTransaction);
                entities.TimeCodeTransaction.AddObject(timeCodeTransaction);

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeCodeTransaction);

                return CreateTimeInvoiceTransaction(entities, actorCompanyId, timeCodeTransaction, invoiceRow, product);
            }
            else
                return new ActionResult((int)ActionResultSave.TimeCodeMaterialStandardMissing, GetText(8423, "Inställning för standard materialkod saknas"));
        }

        public ActionResult UpdateTimeCodeTransactions(CompEntities entities, int actorCompanyId, CustomerInvoiceRow invoiceRow, Project project, int standardMaterialCodeId, string comment = null, int quantity = 0, int invoiceQuantity = 0)
        {
            #region Prereq

            if (!invoiceRow.ProductId.HasValue || invoiceRow.ProductId == 0 || invoiceRow.TimeCodeTransaction == null || invoiceRow.IsTimeProjectRow || invoiceRow.IsCentRoundingRow || invoiceRow.IsInterestRow || invoiceRow.IsReminderRow)
                return new ActionResult(true);

            if (invoiceRow.CustomerInvoiceRowId != 0 && !invoiceRow.TimeCodeTransaction.IsLoaded)
                invoiceRow.TimeCodeTransaction.Load();

            #endregion

            TimeCodeTransaction timeCodeTransaction = invoiceRow.TimeCodeTransaction.FirstOrDefault(x => x.State == (int)SoeEntityState.Active); //We only support 1 transaction for the moment
            if (timeCodeTransaction == null)
            {
                return CreateTimeCodeTransaction(entities, actorCompanyId, invoiceRow, project, standardMaterialCodeId);
            }
            else
            {
                InvoiceProduct product = ProductManager.GetInvoiceProduct(entities, invoiceRow.ProductId.Value);
                if (product == null)
                    return new ActionResult((int)ActionResultSave.ProductNotFound);

                timeCodeTransaction.Comment = comment;
                timeCodeTransaction.Quantity = quantity;
                timeCodeTransaction.InvoiceQuantity = invoiceQuantity;

                //Set FK
                timeCodeTransaction.TimeCodeId = product.TimeCodeId ?? standardMaterialCodeId;
                SetModifiedProperties(timeCodeTransaction);

                TimeInvoiceTransaction timeInvoiceTransaction = GetTimeInvoiceTransactionForInvoiceRow(entities, invoiceRow.CustomerInvoiceRowId);
                if (timeInvoiceTransaction == null)
                    return CreateTimeInvoiceTransaction(entities, actorCompanyId, timeCodeTransaction, invoiceRow, product);
                else
                    return UpdateTimeInvoiceTransaction(entities, actorCompanyId, timeInvoiceTransaction, invoiceRow, product);
            }
        }

        public void DeleteTimeCodeTransactions(CustomerInvoiceRow invoiceRow)
        {
            if (invoiceRow.TimeCodeTransaction == null)
                return;

            if (!invoiceRow.TimeCodeTransaction.IsLoaded)
                invoiceRow.TimeCodeTransaction.Load();

            foreach (var transaction in invoiceRow.TimeCodeTransaction)
            {
                ChangeEntityState(transaction, SoeEntityState.Deleted);

                if (!transaction.TimeInvoiceTransaction.IsLoaded)
                    transaction.TimeInvoiceTransaction.Load();

                foreach (var invoiceTransaction in transaction.TimeInvoiceTransaction)
                {
                    ChangeEntityState(invoiceTransaction, SoeEntityState.Deleted);
                }
            }
        }

        #region GetTimeCodeTransactionsForAcc_Result

        public List<GetTimeCodeTransactionsForAcc_Result> GetTimeCodeTransactionsForAcc(CompEntities entities, int timeAccumulatorId, int employeeId, DateTime startDate, DateTime stopDate, bool loadTimeCode = true)
        {
            List<GetTimeCodeTransactionsForAcc_Result> transactions = new List<GetTimeCodeTransactionsForAcc_Result>();

            var transactionsForAcc = entities.GetTimeCodeTransactionsForAcc(timeAccumulatorId, employeeId, startDate, stopDate).ToList();
            foreach (var transactionsForAccGrouping in transactionsForAcc.GroupBy(i => i.TimeCodeId))
            {
                TimeCode timeCode = null;
                if (loadTimeCode)
                {
                    int timeCodeId = transactionsForAccGrouping.Key;
                    if (!cachedTimeCodes.Any(t => t?.TimeCodeId == timeCodeId))
                        cachedTimeCodes.Add(TimeCodeManager.GetTimeCode(entities, timeCodeId, base.ActorCompanyId, false));

                    timeCode = cachedTimeCodes.FirstOrDefault(tc => tc != null && tc.TimeCodeId == timeCodeId);
                    if (timeCode == null)
                        continue;
                }

                foreach (var transaction in transactionsForAccGrouping)
                {
                    transaction.TimeCode = timeCode;
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        #endregion

        #region GetTimeInvoiceTransactionsForAcc_Result

        public List<GetTimeInvoiceTransactionsForAcc_Result> GetTimeInvoiceTransactionsForAcc(CompEntities entities, int timeAccumulatorId, int employeeId, DateTime startDate, DateTime stopDate, bool loadProduct = true)
        {
            List<GetTimeInvoiceTransactionsForAcc_Result> transactions = new List<GetTimeInvoiceTransactionsForAcc_Result>();

            var transactionsForAcc = entities.GetTimeInvoiceTransactionsForAcc(timeAccumulatorId, employeeId, startDate, stopDate).ToList();
            foreach (var transactionsForAccGrouping in transactionsForAcc.GroupBy(i => i.ProductId))
            {
                Product product = null;
                if (loadProduct)
                {
                    int productId = transactionsForAccGrouping.Key;
                    if (!cachedProducts.Any(t => t?.ProductId == productId))
                        cachedProducts.Add(ProductManager.GetProduct(entities, productId, false));

                    product = cachedProducts.FirstOrDefault(tc => tc.ProductId == productId);
                    if (product == null)
                        continue;
                }

                foreach (var transaction in transactionsForAccGrouping)
                {
                    transaction.Product = product;
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        #endregion

        #region GetTimePayrollTransactionsForAcc_Result

        public List<GetTimePayrollTransactionsForAcc_Result> GetTimePayrollTransactionsForAcc(CompEntities entities, int timeAccumulatorId, int employeeId, DateTime startDate, DateTime stopDate, bool loadProduct = true)
        {
            List<GetTimePayrollTransactionsForAcc_Result> transactions = new List<GetTimePayrollTransactionsForAcc_Result>();

            var transactionsForAcc = entities.GetTimePayrollTransactionsForAcc(timeAccumulatorId, employeeId, startDate, stopDate).ToList();
            foreach (var transactionsForAccGrouping in transactionsForAcc.GroupBy(i => i.ProductId))
            {
                Product product = null;
                if (loadProduct)
                {
                    int productId = transactionsForAccGrouping.Key;
                    if (!cachedProducts.Any(t => t?.ProductId == productId))
                        cachedProducts.Add(ProductManager.GetProduct(entities, productId, false));

                    product = cachedProducts.FirstOrDefault(tc => tc.ProductId == productId);
                    if (product == null)
                        continue;
                }

                foreach (var transaction in transactionsForAccGrouping)
                {
                    transaction.Product = product;
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        #endregion

        #endregion

        #region TimeTransactionItem

        #region Internal

        #region TimeCodeTransaction

        public List<TimeCodeTransaction> GetTimeCodeTransactions(CompEntities entities, List<int> timeBlockDateIds, bool loadTimeCode)
        {
            var query = (from t in entities.TimeCodeTransaction
                         where t.TimeBlockDateId.HasValue &&
                         timeBlockDateIds.Contains(t.TimeBlockDateId.Value) &&
                         (!t.TimeBlockId.HasValue || t.TimeBlock.State == (int)SoeEntityState.Active) &&
                         t.State == (int)SoeEntityState.Active
                         select t);

            if (loadTimeCode)
                query = query.Include("TimeCode");

            return query.ToList();
        }

        public List<TimeCodeTransaction> GetTimeCodeTransactions(CompEntities entities, int actorCompanyId, List<int> EmployeeIds, DateTime dateFrom, DateTime dateTo)
        {
            List<TimeCodeTransaction> timeCodeTransations = new List<TimeCodeTransaction>();

            var timepayrollTransactions = (from t in entities.TimePayrollTransaction
                                                           .Include("TimeCodeTransaction")
                                                           .Include("AccountInternal")
                                                           .Include("PayrollProduct")
                                                           .Include("TimeCodeTransaction.TimeCode")
                                                           .Include("AttestState")
                                                           .Include("TimeBlockDate")
                                           where t.ActorCompanyId == actorCompanyId &&
                                            EmployeeIds.Contains(t.EmployeeId) &&
                                            t.TimeBlockDate.Date >= dateFrom &&
                                            t.TimeBlockDate.Date <= dateTo &&
                                            t.State == (int)SoeEntityState.Active
                                           select t).ToList();

            foreach (var timepayrollTransaction in timepayrollTransactions)
            {
                if (timepayrollTransaction.TimeCodeTransaction != null && !timeCodeTransations.Any(t => t.TimeCodeTransactionId == timepayrollTransaction.TimeCodeTransactionId))
                    timeCodeTransations.Add(timepayrollTransaction.TimeCodeTransaction);
            }

            var timeInvoiceTransactions = (from t in entities.TimeInvoiceTransaction
                                                             .Include("AccountInternal")
                                                             .Include("InvoiceProduct")
                                                             .Include("TimeCodeTransaction.TimeCode")
                                                             .Include("AttestState")
                                                             .Include("TimeBlockDate")
                                           where t.ActorCompanyId == actorCompanyId &&
                                           (t.EmployeeId.HasValue && EmployeeIds.Contains(t.EmployeeId.Value)) &&
                                            t.TimeBlockDate.Date >= dateFrom &&
                                            t.TimeBlockDate.Date <= dateTo &&
                                            t.State == (int)SoeEntityState.Active
                                           select t).ToList();

            foreach (var timeInvoiceTransaction in timeInvoiceTransactions)
            {
                if (timeInvoiceTransaction.TimeCodeTransaction != null && !timeCodeTransations.Any(t => t.TimeCodeTransactionId == timeInvoiceTransaction.TimeCodeTransactionId))
                    timeCodeTransations.Add(timeInvoiceTransaction.TimeCodeTransaction);
            }

            timeCodeTransations.AddRange((from t in entities.TimeCodeTransaction
                                                             .Include("TimeInvoiceTransaction.AccountInternal")
                                                             .Include("TimeInvoiceTransaction.InvoiceProduct")
                                                             .Include("TimeCode")
                                                             .Include("TimeInvoiceTransaction.AttestState")
                                          where t.TimeCode.ActorCompanyId == actorCompanyId &&
                                         t.CustomerInvoiceRowId != null && t.CustomerInvoiceId != null &&
                                          (t.CustomerInvoice.InvoiceDate.HasValue && (t.CustomerInvoice.InvoiceDate >= dateFrom &&
                                            t.CustomerInvoice.InvoiceDate <= dateTo)) &&
                                            t.State == (int)SoeEntityState.Active
                                          select t).ToList());

            return timeCodeTransations;
        }

        public List<TimeTransactionItem> GetTimeCodeTransactionItems(CompEntities entities, int actorCompanyId, int employeeId, DateTime dateFrom, DateTime dateTo, int timeScheduleTemplatePeriodId)
        {
            List<TimeTransactionItem> items = new List<TimeTransactionItem>();

            var transactionItems = entities.GetTimeCodeTransactionsForEmployee(employeeId, actorCompanyId, dateFrom, dateTo, timeScheduleTemplatePeriodId).ToList();
            foreach (var transactionItem in transactionItems)
            {
                items.Add(new TimeTransactionItem
                {
                    //Transaction
                    TimeTransactionId = transactionItem.TimeCodeTransactionId,
                    TransactionType = SoeTimeTransactionType.TimeCode,
                    Quantity = transactionItem.Quantity,
                    InvoiceQuantity = 0,
                    Comment = String.Empty,
                    ManuallyAdded = false,
                    IsAdded = false,
                    IsFixed = false,
                    IsReversed = false,
                    ReversedDate = null,
                    TransactionSysPayrollTypeLevel1 = 0,
                    TransactionSysPayrollTypeLevel2 = 0,
                    TransactionSysPayrollTypeLevel3 = 0,
                    TransactionSysPayrollTypeLevel4 = 0,
                    IsScheduleTransaction = false,
                    IsVacationReplacement = false,
                    ScheduleTransactionType = SoeTimePayrollScheduleTransactionType.None,

                    //Employee
                    EmployeeId = employeeId,
                    EmployeeName = String.Empty,
                    EmployeeChildId = null,
                    EmployeeChildName = String.Empty,

                    //Product
                    ProductId = 0,
                    ProductNr = String.Empty,
                    ProductName = String.Empty,
                    ProductVatType = TermGroup_InvoiceProductVatType.None,
                    PayrollProductSysPayrollTypeLevel1 = 0,
                    PayrollProductSysPayrollTypeLevel2 = 0,
                    PayrollProductSysPayrollTypeLevel3 = 0,
                    PayrollProductSysPayrollTypeLevel4 = 0,

                    //TimeCode
                    TimeCodeId = transactionItem.TimeCodeId,
                    Code = transactionItem.TimeCodeCode,
                    CodeName = transactionItem.TimeCodeName,
                    TimeCodeStart = transactionItem.Start,
                    TimeCodeStop = transactionItem.Stop,
                    TimeCodeType = (SoeTimeCodeType)transactionItem.TimeCodeType,
                    TimeCodeRegistrationType = (TermGroup_TimeCodeRegistrationType)transactionItem.TimeCodeRegistrationType,

                    //TimeBlock
                    TimeBlockId = transactionItem.TimeBlockId,

                    //TimeBlockDate
                    TimeBlockDateId = null,
                    Date = null,

                    //TimeRule
                    TimeRuleName = transactionItem.TimeRuleName,
                    TimeRuleSort = transactionItem.TimeRuleSort,
                    TimeRuleId = transactionItem.TimeRuleId ?? 0,

                    //AttestState
                    AttestStateId = 0,
                    AttestStateName = String.Empty,
                    AttestStateInitial = false,
                    AttestStateColor = String.Empty,
                    AttestStateSort = 0,

                    //Accounting
                    //..
                });
            }

            return items;
        }

        public List<TimeTransactionItem> GetProjectTimeCodeTransactionItems(int actorCompanyId, int projectId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeInvoiceTransaction.NoTracking();
            return GetProjectTimeCodeTransactionItems(entities, actorCompanyId, projectId);
        }

        public List<TimeTransactionItem> GetProjectTimeCodeTransactionItems(CompEntities entities, int actorCompanyId, int projectId)
        {
            List<TimeTransactionItem> items = new List<TimeTransactionItem>();

            var transactionItems = entities.GetTimeCodeTransactionsForProject(projectId, actorCompanyId).ToList();
            foreach (var transactionItem in transactionItems)
            {
                items.Add(new TimeTransactionItem
                {
                    //Transaction
                    TimeTransactionId = transactionItem.TimeCodeTransactionId,
                    TransactionType = SoeTimeTransactionType.TimeCode,
                    Quantity = transactionItem.Quantity,
                    InvoiceQuantity = 0,
                    Comment = String.Empty,
                    ManuallyAdded = false,
                    IsAdded = false,
                    IsFixed = false,
                    IsReversed = false,
                    ReversedDate = null,
                    TransactionSysPayrollTypeLevel1 = 0,
                    TransactionSysPayrollTypeLevel2 = 0,
                    TransactionSysPayrollTypeLevel3 = 0,
                    TransactionSysPayrollTypeLevel4 = 0,
                    IsScheduleTransaction = false,
                    IsVacationReplacement = false,
                    ScheduleTransactionType = SoeTimePayrollScheduleTransactionType.None,

                    //Employee
                    EmployeeId = 0,
                    EmployeeName = String.Empty,
                    EmployeeChildId = null,
                    EmployeeChildName = String.Empty,

                    //Product
                    ProductId = 0,
                    ProductNr = String.Empty,
                    ProductName = String.Empty,
                    ProductVatType = TermGroup_InvoiceProductVatType.None,
                    PayrollProductSysPayrollTypeLevel1 = 0,
                    PayrollProductSysPayrollTypeLevel2 = 0,
                    PayrollProductSysPayrollTypeLevel3 = 0,
                    PayrollProductSysPayrollTypeLevel4 = 0,

                    //TimeCode
                    TimeCodeId = transactionItem.TimeCodeId,
                    Code = transactionItem.TimeCodeCode,
                    CodeName = transactionItem.TimeCodeName,
                    TimeCodeStart = transactionItem.Start,
                    TimeCodeStop = transactionItem.Stop,
                    TimeCodeType = (SoeTimeCodeType)transactionItem.TimeCodeType,
                    TimeCodeRegistrationType = (TermGroup_TimeCodeRegistrationType)transactionItem.TimeCodeRegistrationType,

                    //TimeBlock
                    TimeBlockId = transactionItem.TimeBlockId,

                    //TimeBlockDate
                    TimeBlockDateId = null,
                    Date = null,

                    //TimeRule
                    TimeRuleId = transactionItem.TimeRuleId ?? 0,
                    TimeRuleName = transactionItem.TimeRuleName,
                    TimeRuleSort = null,

                    //AttestState
                    AttestStateId = 0,
                    AttestStateName = String.Empty,
                    AttestStateInitial = false,
                    AttestStateColor = String.Empty,
                    AttestStateSort = 0,

                    //SupplierInvoice
                    SupplierInvoiceId = transactionItem.SupplierInvoiceId ?? 0,

                    //Accounting
                    //..
                });
            }

            return items;
        }

        #endregion

        #endregion

        #region External

        public bool CheckIfTransactionsExistForAttestState(int attestStateId)
        {
            return CheckIfTimeInvoiceTransactionsExistForAttestState(attestStateId) || CheckIfTimePayrollTransactionsExistForAttestState(attestStateId);
        }

        #region TimeInvoiceTransaction

        public List<TimeTransactionItem> GetTimeInvoiceTransactionItems(CompEntities entities, int actorCompanyId, int employeeId, DateTime dateFrom, DateTime dateTo, int timeScheduleTemplatePeriodId)
        {
            List<TimeTransactionItem> items = new List<TimeTransactionItem>();

            var accountDimsMapping = AccountManager.GetAccountDimsMapping(actorCompanyId);

            var transactionItems = entities.GetTimeInvoiceTransactionsWithAccIntsForEmployee(employeeId, actorCompanyId, dateFrom, dateTo, timeScheduleTemplatePeriodId).ToList();
            foreach (var transactionItem in transactionItems)
            {
                var timeTransactionItem = new TimeTransactionItem()
                {
                    //Transaction
                    TimeTransactionId = transactionItem.TimeInvoiceTransactionId,
                    TransactionType = SoeTimeTransactionType.TimePayroll,
                    Quantity = transactionItem.Quantity,
                    InvoiceQuantity = 0,
                    Comment = String.Empty,
                    ManuallyAdded = transactionItem.ManuallyAdded,
                    IsAdded = false,
                    IsFixed = false,
                    IsReversed = false,
                    ReversedDate = null,
                    TransactionSysPayrollTypeLevel1 = 0,
                    TransactionSysPayrollTypeLevel2 = 0,
                    TransactionSysPayrollTypeLevel3 = 0,
                    TransactionSysPayrollTypeLevel4 = 0,

                    //Employee
                    EmployeeId = employeeId,
                    EmployeeName = String.Empty,
                    EmployeeChildId = 0,
                    EmployeeChildName = String.Empty,

                    //Product
                    ProductId = transactionItem.ProductId,
                    ProductNr = transactionItem.ProductNumber,
                    ProductName = transactionItem.ProductName,
                    ProductVatType = (TermGroup_InvoiceProductVatType)transactionItem.InvoiceProductVatType,
                    PayrollProductSysPayrollTypeLevel1 = 0,
                    PayrollProductSysPayrollTypeLevel2 = 0,
                    PayrollProductSysPayrollTypeLevel3 = 0,
                    PayrollProductSysPayrollTypeLevel4 = 0,

                    //TimeCode
                    TimeCodeId = transactionItem.TimeCodeId ?? 0,
                    Code = transactionItem.TimeCodeCode,
                    CodeName = transactionItem.TimeCodeName,
                    TimeCodeStart = null,
                    TimeCodeStop = null,
                    TimeCodeType = (SoeTimeCodeType)transactionItem.TimeCodeType,
                    TimeCodeRegistrationType = (TermGroup_TimeCodeRegistrationType)transactionItem.TimeCodeRegistrationType,

                    //TimeBlock
                    TimeBlockId = transactionItem.TimeBlockId,

                    //TimeBlockDate
                    TimeBlockDateId = null,
                    Date = null,

                    //TimeRule
                    TimeRuleId = 0,
                    TimeRuleName = String.Empty,
                    TimeRuleSort = 0,

                    //Attest
                    AttestStateId = transactionItem.AttestStateId,
                    AttestStateName = transactionItem.AttestStateName,
                    AttestStateInitial = false,
                    AttestStateColor = String.Empty,
                    AttestStateSort = 0,

                    //AccountStd
                    Dim1Id = transactionItem.AccountStdId,
                    Dim1Nr = transactionItem.AccountStdNr,
                    Dim1Name = transactionItem.AccountStdName,
                };

                //AccountInternals
                SetAccountInternalsOnTimeTransactionItem(timeTransactionItem, transactionItem.AccountInternalsStr, accountDimsMapping);

                items.Add(timeTransactionItem);
            }

            return items;
        }

        public List<TimeTransactionItem> GetProjectTimeInvoiceTransactionItems(int actorCompanyId, int projectId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeInvoiceTransaction.NoTracking();
            return GetProjectTimeInvoiceTransactionItems(entities, actorCompanyId, projectId);
        }

        public List<TimeTransactionItem> GetProjectTimeInvoiceTransactionItems(CompEntities entities, int actorCompanyId, int projectId)
        {
            List<TimeTransactionItem> items = new List<TimeTransactionItem>();

            var accountDimsMapping = AccountManager.GetAccountDimsMapping(actorCompanyId);

            var transactionItems = entities.GetTimeInvoiceTransactionsWithAccIntsForProject(projectId, actorCompanyId).ToList();
            foreach (var transactionItem in transactionItems)
            {
                Employee employee = null;
                if (transactionItem.EmployeeId.HasValue)
                    employee = EmployeeManager.GetEmployee(entities, transactionItem.EmployeeId.Value, actorCompanyId, loadContactPerson: true);

                var timeTransactionItem = new TimeTransactionItem()
                {
                    //Transaction
                    TimeTransactionId = transactionItem.TimeInvoiceTransactionId,
                    TransactionType = SoeTimeTransactionType.TimeInvoice,
                    Quantity = transactionItem.Quantity,
                    InvoiceQuantity = transactionItem.InvoiceQuantity,
                    Comment = String.Empty,
                    ManuallyAdded = transactionItem.ManuallyAdded,
                    IsAdded = false,
                    IsFixed = false,
                    IsReversed = false,
                    ReversedDate = null,
                    TransactionSysPayrollTypeLevel1 = 0,
                    TransactionSysPayrollTypeLevel2 = 0,
                    TransactionSysPayrollTypeLevel3 = 0,
                    TransactionSysPayrollTypeLevel4 = 0,
                    IsScheduleTransaction = false,
                    IsVacationReplacement = false,
                    ScheduleTransactionType = SoeTimePayrollScheduleTransactionType.None,

                    //Employee
                    EmployeeId = employee != null ? employee.EmployeeId : 0,
                    EmployeeName = employee != null ? employee.Name : String.Empty,
                    EmployeeChildId = null,
                    EmployeeChildName = String.Empty,

                    //Product
                    ProductId = transactionItem.ProductId,
                    ProductNr = transactionItem.ProductNumber,
                    ProductName = transactionItem.ProductName,
                    ProductVatType = (TermGroup_InvoiceProductVatType)transactionItem.VatType,
                    PayrollProductSysPayrollTypeLevel1 = 0,
                    PayrollProductSysPayrollTypeLevel2 = 0,
                    PayrollProductSysPayrollTypeLevel3 = 0,
                    PayrollProductSysPayrollTypeLevel4 = 0,

                    //TimeCode
                    TimeCodeId = 0,
                    Code = String.Empty,
                    CodeName = String.Empty,
                    TimeCodeStart = null,
                    TimeCodeStop = null,
                    TimeCodeType = SoeTimeCodeType.None,
                    TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,

                    //TimeBlock
                    TimeBlockId = transactionItem.TimeBlockId,

                    //TimeBlockDate
                    TimeBlockDateId = null,
                    Date = null,

                    //TimeRule
                    TimeRuleId = 0,
                    TimeRuleName = String.Empty,
                    TimeRuleSort = 0,

                    //Attest
                    AttestStateId = transactionItem.AttestStateId,
                    AttestStateName = transactionItem.AttestStateName,
                    AttestStateInitial = false,
                    AttestStateColor = String.Empty,
                    AttestStateSort = 0,

                    //Accounting
                    Dim1Id = transactionItem.AccountStdId,
                    Dim1Nr = transactionItem.AccountStdNr,
                    Dim1Name = transactionItem.AccountStdName,
                };

                //AccountInternals
                SetAccountInternalsOnTimeTransactionItem(timeTransactionItem, transactionItem.AccountInternalsStr, accountDimsMapping);

                items.Add(timeTransactionItem);
            }

            return items;
        }

        public bool CheckIfTimeInvoiceTransactionsExistForAttestState(int attestStateId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeInvoiceTransaction.NoTracking();
            return CheckIfTimeInvoiceTransactionsExistForAttestState(entities, attestStateId);
        }

        public bool CheckIfTimeInvoiceTransactionsExistForAttestState(CompEntities entities, int attestStateId)
        {
            return (from t in entities.TimeInvoiceTransaction
                    where t.AttestStateId == attestStateId
                    select t).Any();
        }

        #endregion

        #region TimePayrollTransaction

        public List<TimeTransactionItem> GetTimePayrollTransactionItems(CompEntities entities, int actorCompanyId, int employeeId, DateTime dateFrom, DateTime dateTo, int timeScheduleTemplatePeriodId, bool excludeNotExported = false, bool excludePayrollCalculationTransactions = false, bool includeScheduleTransactions = true, bool onlyTransactionsWithoutTimeBlocks = false, EmployeeGroup employeeGroup = null)
        {
            List<TimeTransactionItem> items = new List<TimeTransactionItem>();

            if (employeeGroup == null)
                employeeGroup = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, onlyActive: true, loadEmployment: true)?.GetEmployeeGroup(dateFrom, dateTo, GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId)));
            if (employeeGroup == null)
                return items;

            Dictionary<int, int> accountDimsMapping = AccountManager.GetAccountDimsMapping(actorCompanyId);

            #region TimePayrollTransaction

            var timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollTransactionsWithAccIntsForEmployee(entities, dateFrom, dateTo, timeScheduleTemplatePeriodId, actorCompanyId, employeeId).ToList();
            foreach (var timePayrollTransactionItem in timePayrollTransactionItems)
            {
                if (onlyTransactionsWithoutTimeBlocks && timePayrollTransactionItem.TimeBlockId > 0)
                    continue;

                var timeTransactionItem = new TimeTransactionItem()
                {
                    //Transaction
                    TimeTransactionId = timePayrollTransactionItem.TimePayrollTransactionId,
                    TransactionType = SoeTimeTransactionType.TimePayroll,
                    Quantity = timePayrollTransactionItem.Quantity,
                    InvoiceQuantity = 0,
                    Comment = String.IsNullOrEmpty(timePayrollTransactionItem.TransactionComment) ? timePayrollTransactionItem.DeviationComment : timePayrollTransactionItem.TransactionComment,
                    ManuallyAdded = timePayrollTransactionItem.ManuallyAdded,
                    IsAdded = timePayrollTransactionItem.IsAdded,
                    IsFixed = timePayrollTransactionItem.IsFixed,
                    IsReversed = timePayrollTransactionItem.IsReversed,
                    ReversedDate = timePayrollTransactionItem.ReversedDate,
                    TransactionSysPayrollTypeLevel1 = timePayrollTransactionItem.TransactionSysPayrollTypeLevel1,
                    TransactionSysPayrollTypeLevel2 = timePayrollTransactionItem.TransactionSysPayrollTypeLevel2,
                    TransactionSysPayrollTypeLevel3 = timePayrollTransactionItem.TransactionSysPayrollTypeLevel3,
                    TransactionSysPayrollTypeLevel4 = timePayrollTransactionItem.TransactionSysPayrollTypeLevel4,
                    IsScheduleTransaction = false,
                    IsVacationReplacement = timePayrollTransactionItem.IsVacationReplacement,
                    ScheduleTransactionType = SoeTimePayrollScheduleTransactionType.None,
                    IsCentRounding = timePayrollTransactionItem.IsCentRounding,
                    IsQuantityRounding = timePayrollTransactionItem.IsQuantityRounding,
                    IncludedInPayrollProductChain = timePayrollTransactionItem.IncludedInPayrollProductChain,

                    //Employee
                    EmployeeId = employeeId,
                    EmployeeName = String.Empty,
                    EmployeeChildId = timePayrollTransactionItem.ChildId,
                    EmployeeChildName = timePayrollTransactionItem.GetEmployeeChildName(),

                    //Product
                    ProductId = timePayrollTransactionItem.ProductId,
                    ProductNr = timePayrollTransactionItem.ProductNumber,
                    ProductName = timePayrollTransactionItem.ProductName,
                    ProductVatType = TermGroup_InvoiceProductVatType.Service,
                    PayrollProductSysPayrollTypeLevel1 = timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel1,
                    PayrollProductSysPayrollTypeLevel2 = timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel2,
                    PayrollProductSysPayrollTypeLevel3 = timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel3,
                    PayrollProductSysPayrollTypeLevel4 = timePayrollTransactionItem.PayrollProductSysPayrollTypeLevel4,

                    //TimeCode
                    TimeCodeId = timePayrollTransactionItem.TimeCodeId ?? 0,
                    Code = timePayrollTransactionItem.TimeCodeCode,
                    CodeName = timePayrollTransactionItem.TimeCodeName,
                    TimeCodeStart = null,
                    TimeCodeStop = null,
                    TimeCodeType = timePayrollTransactionItem.TimeCodeType.HasValue ? (SoeTimeCodeType)timePayrollTransactionItem.TimeCodeType.Value : SoeTimeCodeType.None,
                    TimeCodeRegistrationType = timePayrollTransactionItem.TimeCodeRegistrationType.HasValue ? (TermGroup_TimeCodeRegistrationType)timePayrollTransactionItem.TimeCodeRegistrationType.Value : TermGroup_TimeCodeRegistrationType.Unknown,

                    //TimeBlock
                    TimeBlockId = timePayrollTransactionItem.TimeBlockId,

                    //TimeBlockDate
                    TimeBlockDateId = null,
                    Date = null,

                    //TimeRule
                    TimeRuleId = 0,
                    TimeRuleName = String.Empty,
                    TimeRuleSort = 0,

                    //Attest
                    AttestStateId = timePayrollTransactionItem.AttestStateId,
                    AttestStateName = timePayrollTransactionItem.AttestStateName,
                    AttestStateInitial = false,
                    AttestStateColor = String.Empty,
                    AttestStateSort = 0,

                    //Accounting
                    Dim1Id = timePayrollTransactionItem.AccountStdId,
                    Dim1Nr = timePayrollTransactionItem.AccountStdNr,
                    Dim1Name = timePayrollTransactionItem.AccountStdName,
                };

                if (excludeNotExported && !timePayrollTransactionItem.PayrollProductExport)
                    continue;
                if (excludePayrollCalculationTransactions && timeTransactionItem.IsExcludedInTime())
                    continue;

                //AccountInternals
                SetAccountInternalsOnTimeTransactionItem(timeTransactionItem, timePayrollTransactionItem.AccountInternalsStr, accountDimsMapping);

                items.Add(timeTransactionItem);
            }

            SetParentGuidId(items, timePayrollTransactionItems);

            #endregion

            #region TimePayrollScheduleTransaction

            if (includeScheduleTransactions)
            {
                bool usePayroll = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, actorCompanyId, 0);
                if (usePayroll)
                {
                    var timePayrollScheduleTransactionItemsAll = TimeTransactionManager.GetTimePayrollScheduleTransactionsWithAccIntsForEmployee(entities, null, dateFrom, dateTo, actorCompanyId, employeeId).ToList();

                    foreach (var transactionsByDate in timePayrollScheduleTransactionItemsAll.GroupBy(x => x.Date))
                    {
                        List<GetTimePayrollScheduleTransactionsWithAccIntsForEmployee_Result> timePayrollScheduleTransactionItems = new List<GetTimePayrollScheduleTransactionsWithAccIntsForEmployee_Result>();

                        var timePayrollScheduleTransactionItemsTypeAbsence = (from tpt in transactionsByDate
                                                                              where tpt.Type == (int)SoeTimePayrollScheduleTransactionType.Absence
                                                                              select tpt).ToList();

                        if (timePayrollScheduleTransactionItemsTypeAbsence.Count > 0)
                        {
                            #region Absence

                            timePayrollScheduleTransactionItems = timePayrollScheduleTransactionItemsTypeAbsence;

                            #endregion
                        }
                        else
                        {
                            #region Schedule

                            if (!employeeGroup.AutogenTimeblocks && !timePayrollTransactionItems.Any(x => x.Date == transactionsByDate.Key && !x.IsNetSalary() && !x.IsEmploymentTax() && !x.IsTaxAndNotOptional()))
                            {
                                timePayrollScheduleTransactionItems = (from tpt in transactionsByDate
                                                                       where tpt.Type == (int)SoeTimePayrollScheduleTransactionType.Schedule
                                                                       select tpt).ToList();
                            }

                            #endregion
                        }

                        foreach (var transactionItem in timePayrollScheduleTransactionItems)
                        {
                            var timeTransactionItem = new TimeTransactionItem()
                            {
                                //Transaction
                                TimeTransactionId = transactionItem.TimePayrollScheduleTransactionId,
                                TransactionType = SoeTimeTransactionType.TimePayroll,
                                Quantity = transactionItem.Quantity,
                                InvoiceQuantity = 0,
                                Comment = String.Empty,
                                ManuallyAdded = false,
                                IsAdded = false,
                                IsFixed = false,
                                IsReversed = false,
                                ReversedDate = null,
                                TransactionSysPayrollTypeLevel1 = transactionItem.TransactionSysPayrollTypeLevel1,
                                TransactionSysPayrollTypeLevel2 = transactionItem.TransactionSysPayrollTypeLevel2,
                                TransactionSysPayrollTypeLevel3 = transactionItem.TransactionSysPayrollTypeLevel3,
                                TransactionSysPayrollTypeLevel4 = transactionItem.TransactionSysPayrollTypeLevel4,
                                IsScheduleTransaction = true,
                                IsVacationReplacement = false,
                                ScheduleTransactionType = (SoeTimePayrollScheduleTransactionType)transactionItem.Type,
                                IncludedInPayrollProductChain = transactionItem.IncludedInPayrollProductChain,

                                //Employee
                                EmployeeId = employeeId,
                                EmployeeName = String.Empty,
                                EmployeeChildId = null,
                                EmployeeChildName = String.Empty,

                                //Product
                                ProductId = transactionItem.ProductId,
                                ProductNr = transactionItem.ProductNumber,
                                ProductName = transactionItem.ProductName,
                                ProductVatType = TermGroup_InvoiceProductVatType.Service,
                                PayrollProductSysPayrollTypeLevel1 = transactionItem.PayrollProductSysPayrollTypeLevel1,
                                PayrollProductSysPayrollTypeLevel2 = transactionItem.PayrollProductSysPayrollTypeLevel2,
                                PayrollProductSysPayrollTypeLevel3 = transactionItem.PayrollProductSysPayrollTypeLevel3,
                                PayrollProductSysPayrollTypeLevel4 = transactionItem.PayrollProductSysPayrollTypeLevel4,

                                //TimeCode
                                TimeCodeId = 0,
                                Code = String.Empty,
                                CodeName = String.Empty,
                                TimeCodeStart = null,
                                TimeCodeStop = null,
                                TimeCodeType = SoeTimeCodeType.None,
                                TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,

                                //TimeBlock
                                TimeBlockId = null,

                                //TimeBlockDate
                                TimeBlockDateId = null,
                                Date = null,

                                //TimeRule
                                TimeRuleId = 0,
                                TimeRuleName = String.Empty,
                                TimeRuleSort = 0,

                                //Attest
                                AttestStateId = 0,
                                AttestStateName = ((SoeTimePayrollScheduleTransactionType)transactionItem.Type == SoeTimePayrollScheduleTransactionType.Schedule) ? GetText(8570, "Preliminär") : String.Empty,
                                AttestStateInitial = false,
                                AttestStateColor = String.Empty,
                                AttestStateSort = 0,

                                //Accounting
                                Dim1Id = transactionItem.AccountStdId,
                                Dim1Nr = transactionItem.AccountStdNr,
                                Dim1Name = transactionItem.AccountStdName,
                            };

                            if (excludeNotExported && !transactionItem.PayrollProductExport)
                                continue;
                            if (excludePayrollCalculationTransactions && timeTransactionItem.IsExcludedInTime())
                                continue;

                            //AccountInternals
                            SetAccountInternalsOnTimeTransactionItem(timeTransactionItem, transactionItem.AccountInternalsStr, accountDimsMapping);

                            items.Add(timeTransactionItem);
                        }
                    }

                    SetParentGuidId(items, timePayrollScheduleTransactionItemsAll);
                }
            }

            #endregion

            return items;
        }

        private void SetParentGuidId(List<TimeTransactionItem> items, List<GetTimePayrollTransactionsWithAccIntsForEmployee_Result> timePayrollTransactionItems)
        {
            foreach (var timePayrollTransactionItem in timePayrollTransactionItems.Where(x => x.ParentId.HasValue))
            {
                var timeTransactionItem = items.FirstOrDefault(x => x.ScheduleTransactionType == SoeTimePayrollScheduleTransactionType.None && x.TimeTransactionId == timePayrollTransactionItem.TimePayrollTransactionId);
                if (timeTransactionItem != null)
                {
                    var parentTimeTransactionItem = items.FirstOrDefault(x => x.ScheduleTransactionType == SoeTimePayrollScheduleTransactionType.None && x.TimeTransactionId == timePayrollTransactionItem.ParentId.Value);
                    if (parentTimeTransactionItem != null)
                        timeTransactionItem.ParentGuidId = parentTimeTransactionItem.GuidId;
                }
            }
        }

        private void SetParentGuidId(List<TimeTransactionItem> items, List<GetTimePayrollScheduleTransactionsWithAccIntsForEmployee_Result> scheduleTransactionItems)
        {
            foreach (var scheduleTransactionItem in scheduleTransactionItems.Where(x => x.ParentId.HasValue))
            {
                var timeTransactionItem = items.FirstOrDefault(x => x.ScheduleTransactionType != SoeTimePayrollScheduleTransactionType.None && x.TimeTransactionId == scheduleTransactionItem.TimePayrollScheduleTransactionId);
                if (timeTransactionItem != null)
                {
                    var parentTimeTransactionItem = items.FirstOrDefault(x => x.ScheduleTransactionType != SoeTimePayrollScheduleTransactionType.None && x.TimeTransactionId == scheduleTransactionItem.ParentId.Value);
                    if (parentTimeTransactionItem != null)
                        timeTransactionItem.ParentGuidId = parentTimeTransactionItem.GuidId;
                }
            }
        }

        public List<TimeTransactionItem> GetProjectTimePayrollTransactionItems(CompEntities entities, int actorCompanyId, int projectId)
        {
            List<TimeTransactionItem> items = new List<TimeTransactionItem>();

            var accountDimsMapping = AccountManager.GetAccountDimsMapping(actorCompanyId);

            var transactionItems = TimeTransactionManager.GetTimePayrollTransactionsWithAccIntsForProject(entities, projectId, actorCompanyId);
            foreach (var transactionItem in transactionItems)
            {
                Employee employee = null;
                if (transactionItem.EmployeeId != 0)
                    employee = EmployeeManager.GetEmployee(entities, transactionItem.EmployeeId, actorCompanyId, loadContactPerson: true);

                var timeTransactionItem = new TimeTransactionItem()
                {
                    //Transaction
                    TimeTransactionId = transactionItem.TimePayrollTransactionId,
                    TransactionType = SoeTimeTransactionType.TimePayroll,
                    Quantity = transactionItem.Quantity,
                    InvoiceQuantity = 0,
                    Comment = String.IsNullOrEmpty(transactionItem.TransactionComment) ? transactionItem.DeviationComment : transactionItem.TransactionComment,
                    ManuallyAdded = transactionItem.ManuallyAdded,
                    IsAdded = false,
                    IsFixed = false,
                    IsReversed = false,
                    ReversedDate = null,
                    TransactionSysPayrollTypeLevel1 = 0,
                    TransactionSysPayrollTypeLevel2 = 0,
                    TransactionSysPayrollTypeLevel3 = 0,
                    TransactionSysPayrollTypeLevel4 = 0,
                    IsScheduleTransaction = false,
                    IsVacationReplacement = false,
                    ScheduleTransactionType = SoeTimePayrollScheduleTransactionType.None,

                    //Employee
                    EmployeeId = transactionItem.EmployeeId,
                    EmployeeName = employee != null ? employee.EmployeeNrAndName : String.Empty,
                    EmployeeChildId = 0,
                    EmployeeChildName = String.Empty,

                    //Product
                    ProductId = transactionItem.ProductId,
                    ProductNr = transactionItem.ProductNumber,
                    ProductName = transactionItem.ProductName,
                    ProductVatType = TermGroup_InvoiceProductVatType.Service,
                    PayrollProductSysPayrollTypeLevel1 = 0,
                    PayrollProductSysPayrollTypeLevel2 = 0,
                    PayrollProductSysPayrollTypeLevel3 = 0,
                    PayrollProductSysPayrollTypeLevel4 = 0,

                    //TimeCode
                    TimeCodeId = 0,
                    Code = String.Empty,
                    CodeName = String.Empty,
                    TimeCodeStart = null,
                    TimeCodeStop = null,
                    TimeCodeType = SoeTimeCodeType.None,
                    TimeCodeRegistrationType = TermGroup_TimeCodeRegistrationType.Unknown,

                    //TimeBlock
                    TimeBlockId = transactionItem.TimeBlockId,

                    //TimeBlockDate
                    TimeBlockDateId = null,
                    Date = null,

                    //TimeRule
                    TimeRuleId = 0,
                    TimeRuleName = String.Empty,
                    TimeRuleSort = 0,

                    //AttestState
                    AttestStateId = transactionItem.AttestStateId,
                    AttestStateName = transactionItem.AttestStateName,
                    AttestStateInitial = false,
                    AttestStateColor = String.Empty,
                    AttestStateSort = 0,

                    //Accounting
                    Dim1Id = transactionItem.AccountStdId,
                    Dim1Nr = transactionItem.AccountStdNr,
                    Dim1Name = transactionItem.AccountStdName,
                };

                //AccountInternals
                SetAccountInternalsOnTimeTransactionItem(timeTransactionItem, transactionItem.AccountInternalsStr, accountDimsMapping);

                items.Add(timeTransactionItem);
            }

            return items;
        }

        public bool CheckIfTimePayrollTransactionsExistForAttestState(int attestStateId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimePayrollTransaction.NoTracking();
            return CheckIfTimePayrollTransactionsExistForAttestState(entities, attestStateId);
        }

        public bool CheckIfTimePayrollTransactionsExistForAttestState(CompEntities entities, int attestStateId)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.AttestStateId == attestStateId
                    select t).Any();
        }

        public Dictionary<int, List<TimePayrollTransactionDTO>> GetTimePayrollTransactionDTOForReport(DateTime dateFrom, DateTime dateTo, List<int> employeeIds, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimePayrollTransactionDTOForReport(entities, dateFrom, dateTo, employeeIds, actorCompanyId);
        }
        
        public Dictionary<int, List<TimePayrollTransactionDTO>> GetTimePayrollTransactionDTOForReport(CompEntities entities, DateTime dateFrom, DateTime dateTo, List<int> employeeIds, int actorCompanyId)
        {
            var companyTimePayrollTransactionDTOs = new Dictionary<int, List<TimePayrollTransactionDTO>>();

            BatchHelper batchHelper = BatchHelper.Create(employeeIds.Distinct().ToList(), 999);
            while (batchHelper.HasMoreBatches())
            {
                var temp = GetTimePayrollTransactionDTOForReportBatch(entities, dateFrom, dateTo, batchHelper.GetCurrentBatchIds(), actorCompanyId);
                foreach (var item in temp)
                {
                    if (!companyTimePayrollTransactionDTOs.ContainsKey(item.Key))
                        companyTimePayrollTransactionDTOs.Add(item.Key, item.Value);
                }
                batchHelper.MoveToNextBatch();
            }

            return companyTimePayrollTransactionDTOs;
        }

        private Dictionary<int, List<TimePayrollTransactionDTO>> GetTimePayrollTransactionDTOForReportBatch(CompEntities entities, DateTime dateFrom, DateTime dateTo, List<int> employeeIds, int actorCompanyId)
        {
            var companyTimePayrollTransactionDTOs = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            if (!employeeIds.Any())
                return companyTimePayrollTransactionDTOs;

            try
            {
                List<TimePayrollTransactionDTO> dtos = new List<TimePayrollTransactionDTO>();
                List<AccountDimDTO> accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(actorCompanyId));

                using (SqlConnection connection = new SqlConnection(FrownedUponSQLClient.GetADOConnectionString(500)))
                {
                    string sql = "";

                    List<int> filteredEmployeeIds = new List<int>();
                    if (employeeIds.Count > 6000)
                    {
                        var fromTrans = dateFrom.AddDays(-30);
                        var toTrans = dateTo.AddDays(30);
                        var employeeIdsWithTransactions = entities.TimePayrollTransaction.Where(w => w.ActorCompanyId == actorCompanyId && w.TimeBlockDate.ActorCompanyId == actorCompanyId && w.State == (int)SoeEntityState.Active && w.TimeBlockDate.Date >= fromTrans && w.TimeBlockDate.Date <= toTrans).Select(s => s.EmployeeId).Distinct().ToList();
                        filteredEmployeeIds = employeeIds.Where(w => employeeIdsWithTransactions.Contains(w)).ToList();
                    }
                    else
                        filteredEmployeeIds.AddRange(employeeIds);

                    var tempTableName = $"#TempEmployeeIds{dateFrom.Day}{actorCompanyId}";

                    string tempTableSql = $@"
                                            CREATE TABLE {tempTableName} (EmployeeId INT PRIMARY KEY);
                                            INSERT INTO {tempTableName}  (EmployeeId)
                                            VALUES " + string.Join(",", filteredEmployeeIds.Select(id => $"({id})")) + ";";

                    sql = $@"
                            {tempTableSql}

                            SELECT 		
                                tpt.TimeCodeTransactionId,
		                        tpt.TimePayrollTransactionId,
		                        tpt.Quantity,
		                        tpt.Amount,
		                        tpt.AmountCurrency,
		                        tpt.AmountLedgerCurrency,
		                        tpt.AmountEntCurrency,
		                        tpt.VatAmount,
		                        tpt.VatAmountCurrency,
		                        tpt.VatAmountLedgerCurrency,
		                        tpt.VatAmountEntCurrency,
		                        tpt.IsPreliminary,
		                        tpt.ManuallyAdded,
		                        tpt.IsAdded,
		                        tpt.IsFixed,
		                        tpt.Exported,
		                        tpt.IsReversed,
		                        tpt.ReversedDate,
		                        tpt.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
		                        tpt.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
		                        tpt.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
		                        tpt.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
		                        tpt.IsCentRounding,
		                        tpt.IsQuantityRounding,
		                        tpt.IncludedInPayrollProductChain,
		                        tpt.ParentId,
		                        tpt.IsVacationReplacement,		                                
		                        tpt.EmployeeId,		                                
		                        tpt.TimeBlockDateId,
                                tpt.payrollproductid,
                                tpt.created,
                                tpt.createdby,
                                tpt.modified,
                                tpt.modifiedby,
                                tpt.Comment,
		                        tbd.Date,
		                        LEFT(o.list,LEN(o.list) - 1) AS AccountInternalsStr,
		                        tpte.QuantityCalendarDays,
		                        tpte.QuantityWorkDays,
		                        tpte.CalenderDayFactor,
                                tpte.formulanames,
		                        tpte.formula,
		                        tpte.formulaextracted,
		                        tpte.formulaorigin,
		                        tpte.TimeUnit,
								tpt.AccountId,
                                tpt.AttestStateId,
                                tpt.PayrollStartValueRowId,
                                tpt.UnitPrice,
                                tb.StartTime,
                                tb.StopTime,
                                tct.Start as 'TimeCodeTransactionStartTime',
                                tct.Stop as 'TimeCodeTransactionStopTime',
                                tb.TimeDeviationCauseStartId,
                                tb.TimeDeviationCauseStopId
	                        FROM 
		                        dbo.TimePayrollTransaction AS tpt WITH (NOLOCK) inner join
                                {tempTableName} as ei on tpt.EmployeeId = ei.EmployeeId inner join
								dbo.TimeBlockDate AS tbd WITH (NOLOCK) ON tbd.TimeBlockDateId = tpt.TimeBlockDateId LEFT OUTER JOIN
                                dbo.TimeBlock AS tb WITH (NOLOCK) ON tb.TimeBlockId = tpt.TimeBlockId  LEFT OUTER JOIN
                                dbo.TimeCodeTransaction AS tct WITH (NOLOCK) ON tct.TimeCodeTransactionId = tpt.TimeCodeTransactionId LEFT OUTER JOIN
		                        dbo.TimePayrollTransactionExtended as tpte WITH (NOLOCK) on tpt.TimePayrollTransactionId=tpte.TimePayrollTransactionId  CROSS APPLY 
		                            (
			                        SELECT   
				                        CONVERT(VARCHAR(100),tptad.AccountDimNr) + '|' + CONVERT(VARCHAR(100),tacc.AccountId) + '|' + CONVERT(VARCHAR(100),tacc.AccountNr) + '|' + CONVERT(VARCHAR(100),tacc.Name) + ',' AS [text()]
			                        FROM
				                        dbo.TimePayrollTransactionAccount AS ta WITH (NOLOCK) INNER JOIN 
				                        dbo.Account AS tacc WITH (NOLOCK) ON tacc.AccountId = ta.AccountId INNER JOIN 
				                        dbo.AccountDim AS tptad WITH (NOLOCK) ON tptad.AccountDimId = tacc.AccountDimId
			                        WHERE    
				                        ta.TimePayrollTransactionId = tpt.TimePayrollTransactionId and tptad.AccountDimNr > 1
			                        ORDER BY
				                        tptad.AccountDimNr
			                        FOR XML PATH('')
		                            ) o(list)	
	                        WHERE
		                        (tbd.Date BETWEEN '{CalendarUtility.ToSqlFriendlyDateTime(dateFrom)}' AND '{CalendarUtility.ToSqlFriendlyDateTime(dateTo)}') AND                                        
		                        tpt.ActorCompanyId = {actorCompanyId} and
		                        tpt.[State] = 0
	                        ORDER BY
		                        tbd.EmployeeId, tbd.date

                            DROP TABLE {tempTableName}";


                    var magnitud = CalendarUtility.GetTotalDays(dateFrom, dateTo) + employeeIds.Count;
                    var queryResult = FrownedUponSQLClient.ExcuteQueryNew(connection, sql, magnitud < 30 ? (int?)null : magnitud > 1000 ? 1000 : magnitud);
                    var reader = queryResult.SqlDataReader;

                    if (!queryResult.Result.Success)
                        LogError($"{queryResult.Result.Exception} {queryResult.Result.ErrorMessage}");

                    if (reader != null)
                    {
                        int prevEmployeeId = 0;

                        while (reader.Read())
                        {
                            int currentEmployeeId = (int)reader["employeeid"];

                            if (currentEmployeeId != prevEmployeeId)
                            {
                                companyTimePayrollTransactionDTOs.Add(prevEmployeeId, dtos);
                                dtos = new List<TimePayrollTransactionDTO>();
                                prevEmployeeId = currentEmployeeId;
                            }

                            var extended = new TimePayrollTransactionExtendedDTO()
                            {
                                EmployeeId = (int)reader["employeeid"],
                                CalenderDayFactor = reader["calenderdayfactor"].ToString() != "" ? (decimal)reader["calenderdayfactor"] : 0,
                                QuantityCalendarDays = reader["quantitycalendardays"].ToString() != "" ? (decimal)reader["quantitycalendardays"] : 0,
                                QuantityWorkDays = reader["quantityworkdays"].ToString() != "" ? (decimal)reader["quantityworkdays"] : 0,
                                TimeUnit = reader["timeunit"].ToString() != "" ? (int)reader["timeunit"] : 0,
                                FormulaNames = reader["formulanames"].ToString() != "" ? reader["formulanames"].ToString() : string.Empty,
                                Formula = reader["formula"].ToString() != "" ? reader["formula"].ToString() : string.Empty,
                                FormulaExtracted = reader["formulaextracted"].ToString() != "" ? reader["formulaextracted"].ToString() : string.Empty,
                                FormulaOrigin = reader["formulaorigin"].ToString() != "" ? reader["formulaorigin"].ToString() : string.Empty,
                            };

                            var dto = new TimePayrollTransactionDTO();

                            dto.UnitPrice = reader["unitprice"].ToString() != "" ? (decimal?)reader["unitprice"] ?? 0 : 0;
                            dto.AttestStateId = (int)reader["atteststateid"];
                            dto.EmployeeId = (int)reader["employeeid"];
                            dto.PayrollProductId = (int)reader["payrollproductid"];
                            dto.Date = (DateTime)reader["date"];
                            dto.TimePayrollTransactionId = (int)reader["timepayrolltransactionid"];
                            dto.Quantity = (decimal)reader["quantity"];
                            dto.Amount = reader["amount"].ToString() != "" ? (decimal?)reader["amount"] ?? 0 : 0;
                            dto.VatAmount = reader["vatamount"].ToString() != "" ? (decimal?)reader["vatamount"] ?? 0 : 0;
                            dto.IsPreliminary = (bool)reader["ispreliminary"];
                            dto.IsFixed = (bool)reader["isfixed"];
                            dto.ManuallyAdded = (bool)reader["manuallyadded"];
                            dto.IsAdded = (bool)reader["isadded"];
                            dto.Exported = (bool)reader["exported"];
                            dto.SysPayrollTypeLevel1 = reader["transactionsyspayrolltypelevel1"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel1"] : null;
                            dto.SysPayrollTypeLevel2 = reader["transactionsyspayrolltypelevel2"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel2"] : null;
                            dto.SysPayrollTypeLevel3 = reader["transactionsyspayrolltypelevel3"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel3"] : null;
                            dto.SysPayrollTypeLevel4 = reader["transactionsyspayrolltypelevel4"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel4"] : null;
                            dto.Extended = extended;
                            dto.TimeCodeTransactionId = reader["timecodetransactionid"].ToString() != "" ? (int?)reader["timecodetransactionid"] : null;
                            dto.PayrollStartValueRowId = reader["payrollstartvaluerowid"].ToString() != "" ? (int?)reader["payrollstartvaluerowid"] : null;
                            dto.TimeBlockDateId = (int)reader["timeblockdateId"];
                            dto.AccountId = (int)reader["accountId"];
                            dto.AccountInternals = reader["accountinternalsstr"].ToString() != "" ? TimeTransactionManager.GetAccountInternalDTOs(reader["accountinternalsstr"].ToString(), accountDims: accountDims) : new List<AccountInternalDTO>();
                            dto.StartTime = reader["starttime"].ToString() != "" ? (DateTime?)reader["starttime"] : null;
                            dto.StopTime = reader["stoptime"].ToString() != "" ? (DateTime?)reader["stoptime"] : null;
                            dto.TimeCodeTransactionStartTime = reader["TimeCodeTransactionStartTime"].ToString() != "" ? (DateTime?)reader["TimeCodeTransactionStartTime"] : null;
                            dto.TimeCodeTransactionStopTime = reader["TimeCodeTransactionStopTime"].ToString() != "" ? (DateTime?)reader["TimeCodeTransactionStopTime"] : null;
                            dto.TimeDeviationCauseStartId = reader["timedeviationcausestartid"].ToString() != "" ? (int?)reader["timedeviationcausestartid"] : null;
                            dto.TimeDeviationCauseStopId = reader["timedeviationcausestopid"].ToString() != "" ? (int?)reader["timedeviationcausestopid"] : null;
                            dto.Comment = reader["comment"].ToString() != "" ? reader["comment"].ToString() : string.Empty;
                            dto.Created = reader["created"].ToString() != "" ? (DateTime?)reader["created"] : null;
                            dto.CreatedBy = reader["createdby"].ToString() != "" ? reader["createdby"].ToString() : string.Empty;
                            dto.Modified = reader["modified"].ToString() != "" ? (DateTime?)reader["modified"] : null;
                            dto.ModifiedBy = reader["modifiedby"].ToString() != "" ? reader["modifiedby"].ToString() : string.Empty;
                            dtos.Add(dto);
                        }

                        if (dtos.Any())
                            companyTimePayrollTransactionDTOs.Add(prevEmployeeId, dtos);
                    }


                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                throw new Exception("GetTimePayrollTransactionDTOForReport", ex);
            }
            return companyTimePayrollTransactionDTOs;
        }

        public Dictionary<int, List<TimePayrollTransactionDTO>> AddAmounts(int actorCompanyId, Dictionary<int, List<TimePayrollTransactionDTO>> dict, List<Employee> employees, List<PayrollGroup> payrollGroups)
        {
            var payrollGroupSettings = PayrollManager.GetPayrollGroupSettingsForCompany(actorCompanyId);
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var payrollGroupPriceTypes = base.GetPayrollGroupPriceTypesForCompanyFromCache(entities, CacheConfig.Company(actorCompanyId)).ToDTOs(true).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var employmentPriceTypes = EmployeeManager.GetEmploymentPriceTypesForCompany(entitiesReadOnly, actorCompanyId).ToDTOs(true, false).ToList();
            var employmentPriceTypesForCompanyDict = EmployeeManager.GetEmploymentPriceTypesForCompany(entitiesReadOnly, actorCompanyId).ToDTOs(true, false).GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, v => v.ToList());
            var firstDate = dict.SelectMany(s => s.Value).Where(w => w.Date != null).OrderBy(o => o.Date).FirstOrDefault()?.Date;
            var lastDate = dict.SelectMany(s => s.Value).Where(w => w.Date != null).OrderByDescending(o => o.Date).FirstOrDefault()?.Date;  
            var fixedPayrollRows = EmployeeManager.GetEmployeeFixedPayrollRows(actorCompanyId, employees, firstDate ?? DateTime.Today.AddYears(-20), lastDate ?? DateTime.Today.AddYears(10));
            var fixedPayrollRowsDict = fixedPayrollRows.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, v => v.ToList());

            decimal fallBackHourCost = 0;

            foreach (var pair in dict)
            {
                var employee = employees.FirstOrDefault(f => f.EmployeeId == pair.Key);
                if (employee != null && pair.Value != null)
                {
                    DateTime? start = pair.Value.OrderBy(i => i.Date).First().Date;
                    DateTime? stop = pair.Value.OrderBy(i => i.Date).Last().Date;
                    if (!start.HasValue || !stop.HasValue)
                        continue;

                    var fixedPayrollRowsForEmployee = fixedPayrollRowsDict.ContainsKey(employee.EmployeeId) ? fixedPayrollRowsDict[employee.EmployeeId] : new List<FixedPayrollRowDTO>();

                    var paymentDict = PayrollManager.GetEmployeeHourlyPays(actorCompanyId, employee, start.Value, stop.Value, payrollGroups: payrollGroups, payrollGroupSettings: payrollGroupSettings, payrollGroupPriceTypes: payrollGroupPriceTypes, employmentPriceTypesForCompanyDict: employmentPriceTypesForCompanyDict, fixedPayrollRows: fixedPayrollRowsForEmployee);

                    foreach (var transOnDateGroup in pair.Value.Where(w => w.Date.HasValue).GroupBy(g => g.Date.Value))
                    {
                        if (paymentDict.TryGetValue(transOnDateGroup.Key, out decimal hourlyPay) && hourlyPay != 0)
                        {
                            var minuteSalary = hourlyPay != 0 ? decimal.Divide(hourlyPay, 60) : decimal.Divide(fallBackHourCost, 60);

                            foreach (var item in transOnDateGroup.Where(w => w.Amount == 0 && w.Quantity != 0))
                            {
                                item.Amount = decimal.Round(decimal.Multiply(item.Quantity, minuteSalary), 2);
                            }
                        }
                    }
                }
            }

            return dict;
        }

        public Dictionary<int, List<TimePayrollTransactionDTO>> GetTimePayrollScheduleTransactionDTOForReport(DateTime dateFrom, DateTime dateTo, List<int> employeeIds, int actorCompanyId)
        {
            Dictionary<int, List<TimePayrollTransactionDTO>> companyTimePayrollTransactionDTOs = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            List<TimePayrollTransactionDTO> dtos = new List<TimePayrollTransactionDTO>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<AccountDimDTO> accountDims = base.GetAccountDimsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));

            using (SqlConnection connection = new SqlConnection(FrownedUponSQLClient.GetADOConnectionString()))
            {
                string separator = ",";
                var sql = $@"	
	                                 SELECT 		                                
		                                tpt.TimePayrollScheduleTransactionId,
		                                tpt.Quantity,
		                                tpt.Amount,
		                                tpt.AmountCurrency,
		                                tpt.AmountLedgerCurrency,
		                                tpt.AmountEntCurrency,
		                                tpt.VatAmount,
		                                tpt.VatAmountCurrency,
		                                tpt.VatAmountLedgerCurrency,
		                                tpt.VatAmountEntCurrency,
		                                tpt.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
		                                tpt.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
		                                tpt.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
		                                tpt.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
		                                tpt.IncludedInPayrollProductChain,
		                                tpt.ParentId,		                                
		                                tpt.EmployeeId,	
                                        tpt.ProductId,	                                
		                                tpt.TimeBlockDateId,
                                        tpt.created,
                                        tpt.createdby,
                                        tpt.modified,
                                        tpt.modifiedby,
		                                tbd.Date,
		                                LEFT(o.list,LEN(o.list) - 1) AS AccountInternalsStr,
										tpt.AccountId,
                                        tpt.UnitPrice,
                                        tpt.Type,
                                        tpt.TimeBlockStartTime,
                                        tpt.TimeBlockStopTime
	                                FROM 
		                                dbo.TimePayrollScheduleTransaction AS tpt WITH (NOLOCK) inner join
										dbo.TimeBlockDate AS tbd WITH (NOLOCK) ON tbd.TimeBlockDateId = tpt.TimeBlockDateId  CROSS APPLY 
		                              (
			                                SELECT   
				                                CONVERT(VARCHAR(100),tptad.AccountDimNr) + '|' + CONVERT(VARCHAR(100),tacc.AccountId) + '|' + CONVERT(VARCHAR(100),tacc.AccountNr) + '|' + CONVERT(VARCHAR(100),tacc.Name) + ',' AS [text()]
			                                FROM
				                                dbo.TimePayrollScheduleTransactionAccount AS ta WITH (NOLOCK) INNER JOIN 
				                                dbo.Account AS tacc WITH (NOLOCK) ON tacc.AccountId = ta.AccountId INNER JOIN 
				                                dbo.AccountDim AS tptad WITH (NOLOCK) ON tptad.AccountDimId = tacc.AccountDimId
			                                WHERE    
				                                ta.TimePayrollScheduleTransactionId = tpt.TimePayrollScheduleTransactionId and tptad.AccountDimNr > 1
			                                ORDER BY
				                                tptad.AccountDimNr
			                                FOR XML PATH('')
		                                 ) o(list)	
	                                WHERE
		                                tbd.EmployeeId in ({string.Join(separator, employeeIds.ToArray())}) AND 
		                                (tbd.Date BETWEEN '{CalendarUtility.ToSqlFriendlyDateTime(dateFrom)}' AND '{CalendarUtility.ToSqlFriendlyDateTime(dateTo)}') 	AND                               
		                                tpt.[State] = 0 AND
                                        tpt.Type = 2
	                                ORDER BY
		                                tbd.EmployeeId, tbd.date
                            ";

                var reader = FrownedUponSQLClient.ExcuteQuery(connection, sql);

                if (reader != null)
                {
                    int prevEmployeeId = 0;

                    while (reader.Read())
                    {
                        int currentEmployeeId = (int)reader["employeeid"];

                        if (currentEmployeeId != prevEmployeeId)
                        {
                            companyTimePayrollTransactionDTOs.Add(prevEmployeeId, dtos);
                            dtos = new List<TimePayrollTransactionDTO>();
                            prevEmployeeId = currentEmployeeId;
                        }

                        var dto = new TimePayrollTransactionDTO
                        {
                            UnitPrice = reader["unitprice"].ToString() != "" ? (decimal?)reader["unitprice"] ?? 0 : 0,
                            EmployeeId = (int)reader["employeeid"],
                            PayrollProductId = (int)reader["productid"],
                            Date = (DateTime)reader["date"],
                            Quantity = (decimal)reader["quantity"],
                            Amount = reader["amount"].ToString() != "" ? (decimal?)reader["amount"] ?? 0 : 0,
                            VatAmount = reader["vatamount"].ToString() != "" ? (decimal?)reader["vatamount"] ?? 0 : 0,
                            SysPayrollTypeLevel1 = reader["transactionsyspayrolltypelevel1"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel1"] : null,
                            SysPayrollTypeLevel2 = reader["transactionsyspayrolltypelevel2"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel2"] : null,
                            SysPayrollTypeLevel3 = reader["transactionsyspayrolltypelevel3"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel3"] : null,
                            SysPayrollTypeLevel4 = reader["transactionsyspayrolltypelevel4"].ToString() != "" ? (int?)reader["transactionsyspayrolltypelevel4"] : null,
                            TimeBlockDateId = (int)reader["timeblockdateId"],
                            AccountId = (int)reader["accountId"],
                            Created = reader["created"].ToString() != "" ? (DateTime?)reader["created"] : null,
                            CreatedBy = reader["createdby"].ToString() != "" ? reader["createdby"].ToString() : string.Empty,
                            Modified = reader["modified"].ToString() != "" ? (DateTime?)reader["modified"] : null,
                            ModifiedBy = reader["modifiedby"].ToString() != "" ? reader["modifiedby"].ToString() : string.Empty,
                            AccountInternals = reader["accountinternalsstr"].ToString() != "" ? TimeTransactionManager.GetAccountInternalDTOs(reader["accountinternalsstr"].ToString(), accountDims: accountDims) : new List<AccountInternalDTO>(),
                            ScheduleTransaction = true,
                            ScheduleTransactionType = (SoeTimePayrollScheduleTransactionType)reader["type"],
                            StartTime = reader["timeblockstarttime"].ToString() != "" ? (DateTime?)reader["timeblockstarttime"] : null,
                            StopTime = reader["timeblockstoptime"].ToString() != "" ? (DateTime?)reader["timeblockstoptime"] : null,
                        };
                        dtos.Add(dto);
                    }

                    if (dtos.Any())
                        companyTimePayrollTransactionDTOs.Add(prevEmployeeId, dtos);
                }
            }
            return companyTimePayrollTransactionDTOs;
        }

        public List<AccountInternalDTO> GetAccountInternalDTOs(string accountInternalsStr, bool ignoreAccountDimNr = false, List<AccountDimDTO> accountDims = null)
        {
            List<AccountInternalDTO> dtos = new List<AccountInternalDTO>();
            if (!String.IsNullOrEmpty(accountInternalsStr))
            {
                string[] accountInternalStr = accountInternalsStr.Split(',');

                foreach (var str in accountInternalStr)
                {
                    //AccounDimNr|AccounId|AccountNr|AccountName
                    var parts = str.Split('|');
                    if (parts.Count() != 4)
                        continue;

                    int accountDimNr = 0;
                    if (!ignoreAccountDimNr && !Int32.TryParse(parts[0], out accountDimNr))
                        continue;

                    if (!Int32.TryParse(parts[1], out int accountId))
                        continue;

                    string accountNr = parts.Count() < 3 ? string.Empty : parts[2];
                    string accountName = parts.Count() < 4 ? string.Empty : parts[3];

                    int accountDimId = 0;

                    if (accountDims != null)
                    {
                        var match = accountDims.FirstOrDefault(w => w.AccountDimNr == accountDimNr);

                        if (match != null)
                            accountDimId = match.AccountDimId;
                    }

                    dtos.Add(new AccountInternalDTO() { AccountId = accountId, AccountDimNr = accountDimNr, AccountDimId = accountDimId, AccountNr = accountNr, Name = accountName });
                }
            }

            return dtos;
        }

        #endregion

        #region Help-methods

        private void SetAccountInternalsOnTimeTransactionItem(TimeTransactionItem timeTransactionItem, string accountInternalsStr, Dictionary<int, int> accountDimsMapping)
        {
            if (timeTransactionItem == null || accountDimsMapping == null)
                return;

            if (!String.IsNullOrEmpty(accountInternalsStr))
            {
                string[] accountInternalStr = accountInternalsStr.Split(',');

                foreach (var str in accountInternalStr)
                {
                    //AccounDimNr:AccounId:AccountNr:AccountName
                    var parts = str.Split(':');
                    if (parts.Count() != 4)
                        continue;

                    if (!Int32.TryParse(parts[0], out int accountDimNr))
                        continue;
                    if (!accountDimsMapping.ContainsKey(accountDimNr))
                        continue;
                    if (!Int32.TryParse(parts[1], out int accountId))
                        continue;

                    string accountNr = parts[2];
                    string accountName = parts[3];

                    switch (accountDimsMapping[accountDimNr])
                    {
                        case 2:
                            timeTransactionItem.Dim2Id = accountId;
                            timeTransactionItem.Dim2Nr = accountNr;
                            timeTransactionItem.Dim2Name = accountName;
                            break;
                        case 3:
                            timeTransactionItem.Dim3Id = accountId;
                            timeTransactionItem.Dim3Nr = accountNr;
                            timeTransactionItem.Dim3Name = accountName;
                            break;
                        case 4:
                            timeTransactionItem.Dim4Id = accountId;
                            timeTransactionItem.Dim4Nr = accountNr;
                            timeTransactionItem.Dim4Name = accountName;
                            break;
                        case 5:
                            timeTransactionItem.Dim5Id = accountId;
                            timeTransactionItem.Dim5Nr = accountNr;
                            timeTransactionItem.Dim5Name = accountName;
                            break;
                        case 6:
                            timeTransactionItem.Dim6Id = accountId;
                            timeTransactionItem.Dim6Nr = accountNr;
                            timeTransactionItem.Dim6Name = accountName;
                            break;
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region TimePayrollTransactionDatesView

        public List<TimePayrollTransactionDatesView> GetTimePayrollTransactionDates(DateTime startDate, DateTime stopDate, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetTimePayrollTransactionDates(entities, startDate, stopDate, actorCompanyId);
        }

        public List<TimePayrollTransactionDatesView> GetTimePayrollTransactionDates(CompEntities entities, DateTime startDate, DateTime stopDate, int actorCompanyId)
        {
            return (from v in entities.TimePayrollTransactionDatesView
                    where v.ActorCompanyId == actorCompanyId &&
                    v.Date >= startDate &&
                    v.Date <= stopDate
                    select v).ToList();
        }

        #endregion

        #region Help-methods

        private readonly List<int> deletedAccountIds = new List<int>();
        private Account GetAccountWithDim(CompEntities entities, int accountId, int actorCompanyId, List<Account> accounts)
        {
            if (deletedAccountIds.Contains(accountId))
                return null;

            Account account = accounts?.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
            {
                account = AccountManager.GetAccount(entities, actorCompanyId, accountId, onlyActive: false, loadAccountDim: true);
                if (account != null && accounts != null)
                    accounts.Add(account);
                else if (account == null)
                    deletedAccountIds.Add(accountId);
            }
            return account;
        }

        private readonly List<int> deletedProductIds = new List<int>();
        private PayrollProduct GetPayrollProductWithSettings(CompEntities entities, int productId, List<PayrollProduct> payrollProducts)
        {
            PayrollProduct payrollProduct = payrollProducts?.FirstOrDefault(p => p.ProductId == productId);
            if (payrollProduct == null)
            {
                payrollProduct = ProductManager.GetPayrollProduct(entities, productId, loadSettings: true);
                if (payrollProduct != null && payrollProducts != null)
                    payrollProducts.Add(payrollProduct);
                else if (payrollProduct == null)
                    deletedProductIds.Add(productId);
            }
            return payrollProduct;
        }

        #endregion
    }
}
