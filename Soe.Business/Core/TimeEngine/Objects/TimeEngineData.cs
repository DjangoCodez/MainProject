using Newtonsoft.Json;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.BatchHelper;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Company

        private Company GetCompany()
        {
            return (from c in entities.Company
                    where c.ActorCompanyId == actorCompanyId &&
                    c.State == (int)SoeEntityState.Active
                    select c).FirstOrDefault();
        }

        #endregion

        #region Country

        private int GetSysCountryId()
        {
            Company company = GetCompanyFromCache();
            return company != null && company.SysCountryId.HasValue ? company.SysCountryId.Value : (int)TermGroup_Languages.Swedish;
        }

        #endregion

        #region AccountingPrio

        private AccountingPrioDTO GetPayrollProductAccount(ProductAccountType type, Employee employee, int productId, int projectId, DateTime date, bool getInternalAccounts = true)
        {
            return AccountManager.GetPayrollProductAccount(entities, type, actorCompanyId, employee, productId, projectId, 0, getInternalAccounts, date: date, employeeGroups: GetEmployeeGroupsFromCache(), accountDims: GetAccountDimsFromCache());
        }

        #endregion

        #region AnnualLeaveGroup

        private List<AnnualLeaveGroup> GetAnnualLeaveGroups()
        {
            return (from pg in entities.AnnualLeaveGroup
                    where pg.ActorCompanyId == actorCompanyId &&
                    pg.State == (int)SoeEntityState.Active
                    select pg).ToList();
        }

        #endregion

        #region DayType

        private List<DayType> GetDayTypes()
        {
            return (from dt in entities.DayType
                    where dt.ActorCompanyId == actorCompanyId &&
                    dt.State == (int)SoeEntityState.Active
                    select dt).ToList();
        }

        private List<DayType> GetDayTypesWithStandardWeekDay()
        {
            return (from dt in entities.DayType
                    where dt.ActorCompanyId == actorCompanyId &&
                    dt.StandardWeekdayTo.HasValue
                    select dt).ToList();
        }

        private List<DayType> GetCompanyDayTypes(DateTime date, List<HolidayDTO> holidaysForCompany, List<DayType> dayTypesForCompany)
        {
            List<DayType> dayTypes = new List<DayType>();

            List<HolidayDTO> holidaysForDate = holidaysForCompany.Where(i => i.Date == date && i.State == (int)SoeEntityState.Active).ToList();
            foreach (HolidayDTO holiday in holidaysForDate)
            {
                DayType dayTypeForHoliday = holiday.DayType != null ? dayTypesForCompany.FirstOrDefault(w => w.DayTypeId == holiday.DayTypeId) : null;
                if (dayTypeForHoliday != null)
                    dayTypes.Add(dayTypeForHoliday);
            }

            if (!dayTypes.Any())
            {
                int dayOfWeek = CalendarUtility.GetDayNrFromCulture(date);

                dayTypes = (from dt in dayTypesForCompany
                            where dt.StandardWeekdayFrom.HasValue &&
                            dt.StandardWeekdayTo.HasValue &&
                            dt.StandardWeekdayFrom.Value <= dayOfWeek &&
                            dt.StandardWeekdayTo.Value >= dayOfWeek &&
                            dt.State == (int)SoeEntityState.Active
                            select dt).ToList();
            }

            return dayTypes;
        }

        public List<DayType> SortDayTypesOnRelevance(List<DayType> dayTypes, Employee employee, DateTime? date = null)
        {
            List<DayType> sortedDayTypes = new List<DayType>();
            if (dayTypes.IsNullOrEmpty() || employee == null)
                return sortedDayTypes;

            if (dayTypes.Count > 1)
            {
                List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();
                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups);
                if (employeeGroup != null)
                {
                    if (!employeeGroup.DayType.IsLoaded)
                        employeeGroup.DayType.Load();

                    //Add EmployeeGroups DayTypes
                    foreach (DayType dayType in dayTypes)
                    {
                        if (employeeGroup.DayType.Any(i => i.DayTypeId == dayType.DayTypeId))
                            sortedDayTypes.Add(dayType);
                    }

                    //Add remaning DayTypes
                    foreach (DayType dayType in dayTypes)
                    {
                        if (!sortedDayTypes.Any(i => i.DayTypeId == dayType.DayTypeId))
                            sortedDayTypes.Add(dayType);
                    }
                }
            }
            else
            {
                //Add all (1)
                sortedDayTypes.AddRange(dayTypes);
            }

            return sortedDayTypes;
        }

        private DayType GetDayType(Employee employee, DateTime date, bool doNotCheckHoliday = false)
        {
            if (employee == null)
                return null;

            DayType dayType = (!doNotCheckHoliday ? GetDayTypeFromHoliday(employee, date) : null) ?? GetDayTypeForDate(employee, date);
            return dayType;
        }

        private DayType GetDayTypeFromHoliday(Employee employee, DateTime date)
        {
            List<DayType> dayTypes = new List<DayType>();

            List<HolidayDTO> companyHolidays = GetHolidaysWithDayTypeAndHalfDayFromCache(date);
            if (companyHolidays.IsNullOrEmpty())
                return null;

            companyHolidays = (from h in companyHolidays
                               where h.Date.Year == date.Year &&
                               h.Date.Month == date.Month &&
                               h.Date.Day == date.Day &&
                               h.State == (int)SoeEntityState.Active
                               select h).ToList();

            if (!companyHolidays.Any())
                return null;

            List<DayType> cachedDayTypes = GetDayTypesFromCache();

            foreach (HolidayDTO companyHoliday in companyHolidays)
            {
                if (dayTypes.Any(i => i.DayTypeId == companyHoliday.DayTypeId))
                    continue;

                DayType dayType = cachedDayTypes.FirstOrDefault(w => w.DayTypeId == companyHoliday.DayTypeId);
                if (dayType != null)
                    dayTypes.Add(dayType);
            }

            //Sort
            dayTypes = SortDayTypesOnRelevance(dayTypes, employee, date);

            return dayTypes.FirstOrDefault();
        }

        private DayType GetDayType(DateTime date, EmployeeGroup employeeGroup, List<HolidayDTO> holidaysForCompany, List<DayType> dayTypesForCompany)
        {
            //Check if EmployeeGroup work at all
            if (employeeGroup == null || employeeGroup.DayType.IsNullOrEmpty())
                return null;

            List<DayType> dayTypesForDate = GetCompanyDayTypes(date, holidaysForCompany, dayTypesForCompany);
            foreach (DayType dayType in dayTypesForDate)
            {
                List<DayType> dayTypesForEmployeeGroup = employeeGroup.DayType.Where(i => i.DayTypeId == dayType.DayTypeId).ToList();
                if (dayTypesForEmployeeGroup.Any())
                    return dayTypesForEmployeeGroup.First();
            }

            return null;
        }

        private DayType GetDayTypeForDate(Employee employee, DateTime date)
        {
            //Get standard weekday DayTypes 
            List<DayType> dayTypes = GetDayTypesWithStandardWeekDayFromCache(employee);

            //Sort
            dayTypes = SortDayTypesOnRelevance(dayTypes, employee, date);

            //Get DayType from dayofweek
            int dayOfWeekNr = CalendarUtility.GetDayNrFromCulture(date);
            DayType dayType = dayTypes.FirstOrDefault(i => CalendarUtility.IsDayTypeInRange(i.StandardWeekdayFrom, i.StandardWeekdayTo, dayOfWeekNr));

            return dayType;
        }

        private DayType GetDayTypeWithHoliday(int dayTypeId)
        {
            return (from d in entities.DayType
                        .Include("Holiday")
                    where d.DayTypeId == dayTypeId &&
                    d.Company.ActorCompanyId == actorCompanyId &&
                    (d.State == (int)SoeEntityState.Active)
                    select d).FirstOrDefault();
        }

        private bool DayTypeExists(string dayTypeName)
        {
            return (from d in entities.DayType
                    where d.Name == dayTypeName &&
                    d.ActorCompanyId == actorCompanyId &&
                    (d.State == (int)SoeEntityState.Active)
                    select d).Any();
        }

        private DayType CreateDayType(DayTypeDTO dayTypeInput)
        {
            if (dayTypeInput == null)
                return null;

            DayType dayType = new DayType()
            {
                SysDayTypeId = dayTypeInput.SysDayTypeId,
                Type = (int)dayTypeInput.Type,
                Name = dayTypeInput.Name,
                Description = dayTypeInput.Description,
                StandardWeekdayFrom = dayTypeInput.StandardWeekdayFrom,
                StandardWeekdayTo = dayTypeInput.StandardWeekdayTo,

                //Set FK
                ActorCompanyId = actorCompanyId,
            };
            SetCreatedProperties(dayType);
            entities.DayType.AddObject(dayType);
            return dayType;
        }

        #endregion

        #region Employee

        private IQueryable<Employee> GetEmployeesWithEmploymentLoadingsQuery(bool onlyActive = true, bool getHidden = true)
        {
            return EmployeeManager.GetEmployeesWithEmploymentLoadingsQuery(entities, onlyActive, getHidden);
        }

        private List<Employee> GetEmployeesForCompanyWithEmployment(List<int> accountIds = null, DateTime? employeeAccountDate = null)
        {
            bool hasAccountIds = !accountIds.IsNullOrEmpty();
            if (!hasAccountIds && TryGetAllEmployeesFromCache(true, out List<Employee> cachedEmployees))
                return cachedEmployees;
            else
                cachedEmployees = new List<Employee>();

            List<int> cachedEmployeeIds = cachedEmployees.Select(i => i.EmployeeId).Distinct().ToList();
            List<Employee> employees = EmployeeManager.GetEmployeesForCompanyWithEmployment(entities, actorCompanyId, accountIds, employeeAccountDate, cachedEmployeeIds);
            AddEmployeesToCache(employees, isAllEmployees: !hasAccountIds);

            return employees.Concat(cachedEmployees).OrderBy(e => e.EmployeeNrSort).ToList();
        }

        private List<Employee> GetEmployeesWithEmployment(List<int> employeeIds, bool onlyActive = true, int? excludeEmployeeId = null)
        {
            if (employeeIds.IsNullOrEmpty())
                return new List<Employee>();

            List<int> filterEmployeeIds = new List<int>();
            filterEmployeeIds.AddRange(employeeIds);

            if (TryGetAllEmployeesFromCache(onlyActive, out List<Employee> cachedEmployees))
                return cachedEmployees.Where(e => filterEmployeeIds.Contains(e.EmployeeId)).ToList();
            if (employeeIds.Count == cachedEmployees.Count)
                return cachedEmployees;

            if (!cachedEmployees.IsNullOrEmpty())
                filterEmployeeIds.RemoveAll(id => cachedEmployees.Any(e => e.EmployeeId == id));

            var query = GetEmployeesWithEmploymentLoadingsQuery(onlyActive).Where(e => e.ActorCompanyId == actorCompanyId && filterEmployeeIds.Contains(e.EmployeeId));

            List<Employee> employees = query.ToList();
            AddEmployeesToCache(employees);

            if (!employees.IsNullOrEmpty() && excludeEmployeeId.HasValue)
                employees = employees.Where(e => e.EmployeeId != excludeEmployeeId.Value).ToList();

            return employees.Concat(cachedEmployees).OrderBy(e => e.EmployeeNrSort).ToList();
        }

        private Employee GetEmployee(int employeeId, bool getHidden = true)
        {
            return (from e in entities.Employee
                    where e.EmployeeId == employeeId &&
                    (getHidden || !e.Hidden) &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        private Employee GetEmployeeWithContactPerson(int employeeId, bool getHidden = true)
        {
            return (from e in entities.Employee
                    .Include("ContactPerson")
                    where e.EmployeeId == employeeId &&
                    (getHidden || !e.Hidden) &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        private Employee GetEmployeeWithContactPersonAndEmployment(int employeeId, bool getHidden = true, bool onlyActive = true)
        {
            return GetEmployeesWithEmploymentLoadingsQuery(onlyActive, getHidden).FirstOrDefault(e => e.EmployeeId == employeeId);
        }

        private Employee GetCurrentEmployee()
        {
            return EmployeeManager.GetEmployeeForUser(entities, userId, actorCompanyId);
        }

        private Employee GetHiddenEmployee()
        {
            return entities.Employee.FirstOrDefault(e => e.ActorCompanyId == actorCompanyId && e.Hidden);
        }

        private EmployeeGroup GetEmployeeGroup(Employee employee, DateTime date)
        {
            return employee.GetEmployeeGroup(date, GetEmployeeGroupsFromCache());
        }

        private List<TimeAccumulatorEmployeeGroupRule> GetTimeAccumulatorEmployeeGroupRule(int timeAccumulatorId)
        {
            return entities.TimeAccumulatorEmployeeGroupRule.Where(r => r.TimeAccumulatorId == timeAccumulatorId && r.State == (int)SoeEntityState.Active).ToList();
        }

        private bool HasEmployeeAnyChildWithSingelCustody(int employeeId)
        {
            return entities.EmployeeChild.Any(ec => ec.EmployeeId == employeeId && ec.SingelCustody && ec.State == (int)SoeEntityState.Active);
        }

        private bool HasEmployeeSickDuringIwhOrStandyRules(List<DateTime> dates, int employeeId)
        {
            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return false;

            dates = dates.OrderBy(i => i.Date).ToList();
            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(dates.First(), dates.Last(), GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return false;

            List<TimeAbsenceRuleHead> absenceRules = GetTimeAbsenceRuleHeadsWithRowsFromCache(employeeGroup.EmployeeGroupId);
            if (!absenceRules.Any(i => i.IsSickDuringIwhOrStandBy))
                return false;

            return true;
        }

        private TermGroup_PayrollExportSalaryType GetEmployeeSalaryType(Employee employee, DateTime startDate, DateTime stopDate)
        {
            TermGroup_PayrollExportSalaryType salaryType = TermGroup_PayrollExportSalaryType.Unknown;

            if (employee == null)
                return salaryType;

            if (employee.PayrollStatisticsSalaryType.HasValue && employee.PayrollStatisticsSalaryType.Value > 0)
            {
                salaryType = (TermGroup_PayrollExportSalaryType)employee.PayrollStatisticsSalaryType.Value;
            }
            else
            {
                int? payrollGroupId = employee.GetPayrollGroupId(startDate, stopDate);
                if (payrollGroupId.HasValue)
                {
                    PayrollGroup payrollGroup = GetPayrollGroupWithSettingsFromCache(payrollGroupId.Value);
                    if (payrollGroup != null)
                    {
                        PayrollGroupSetting payrollGroupSetting = payrollGroup.PayrollGroupSetting.FirstOrDefault(i => i.Type == (int)PayrollGroupSettingType.PayrollReportsSalaryType);
                        if (payrollGroupSetting != null && payrollGroupSetting.IntData.HasValue)
                            salaryType = (TermGroup_PayrollExportSalaryType)payrollGroupSetting.IntData;

                    }
                }
            }

            return salaryType;
        }

        private Dictionary<DateTime, decimal> GetEmployeeEmploymentRates(Employee employee, DateTime startDate, DateTime stopDate)
        {
            Dictionary<DateTime, decimal> employmentRates = new Dictionary<DateTime, decimal>();

            if (employee == null || stopDate < startDate)
                return employmentRates;

            List<Employment> employments = employee.GetEmployments(startDate, stopDate);
            if (employments.IsNullOrEmpty())
                return employmentRates;

            DateTime currentDate = startDate;
            while (currentDate <= stopDate)
            {
                Employment employment = employments.GetEmployment(currentDate);
                if (employment != null)
                {
                    decimal percent = employment.GetPercent(currentDate);
                    if (employmentRates.Count == 0 || employmentRates.Last().Value != percent)
                        employmentRates.Add(currentDate, percent);
                }

                currentDate = currentDate.AddDays(1);
            }

            return employmentRates;
        }

        private bool CanUserEditShiftOnGivenAcountAndDateForEmployee(Employee employee, int accountId, DateTime date, out string message)
        {
            message = string.Empty;
            if (employee == null)
                return false;

            if (employee.EmployeeId == GetHiddenEmployeeIdFromCache())
                return true;

            if (employee.UserId.HasValue && employee.UserId == userId)
                return true;

            //This makes no sense?
            //List<AttestRoleUser> attestRolesForUsers = GetAttestRoleUsersFromCache(entities, CacheConfig.User(actorCompanyId, userId), date, date, includeAttestRole: true, ignoreDates: false, onlyDefaultAccounts: true);
            //if (attestRolesForUsers.Any(x => x.AttestRole.StaffingByEmployeeAccount))
            //    return true;

            var accounts = GetAccountInternalsFromCache();
            var employeeAccounts = AccountManager.GetSelectableEmployeeShiftAccounts(entities, userId, actorCompanyId, employee.EmployeeId, date, accounts, GetAccountDimInternalsWithParentFromCache(), useEmployeeAccountIfNoAttestRole: true, includeAbstract: true);
            bool valid = employeeAccounts.Any(x => x.AccountId == accountId);
            if (!valid)
            {
                var account = accounts.FirstOrDefault(x => x.AccountId == accountId);
                message = String.Format(GetText(9317, "Anställd {0} tillhör inte {1} {2}"), employee.Name, account?.Name ?? string.Empty, date.ToShortDateString());
            }

            return valid;
        }

        private List<AccountDTO> GetAccountsFromHierarchyByUser(DateTime? dateFrom, DateTime? dateTo)
        {
            int defaultDimId = GetCompanyIntSettingFromCache(CompanySettingType.DefaultEmployeeAccountDimEmployee);
            var accounts = GetAccountInternalsFromCache();
            var accountDims = GetAccountDimInternalsWithParentFromCache();
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector);
            return AccountManager.GetAccountsFromHierarchyByUser(entities, actorCompanyId, userId, dateFrom, dateTo, input, accounts, accountDims).Where(x => x.AccountDimId == defaultDimId).ToList();
        }

        private List<EmployeeAgeDTO> GetEmployeeAgeInfo(List<Employee> employees)
        {
            List<EmployeeAgeDTO> employeeAgeInfos = new List<EmployeeAgeDTO>();

            if (employees.IsNullOrEmpty())
                return employeeAgeInfos;

            foreach (Employee employee in employees)
            {
                if (!employeeAgeInfos.Any(i => i.EmployeeId == employee.EmployeeId))
                    employeeAgeInfos.Add(new EmployeeAgeDTO(employee.EmployeeId, employee.Name, GetEmployeeBirthDateFromCache(employee), employee.Employment.GetEndDate()));
            }

            return employeeAgeInfos;
        }

        private DateTime? GetEmployeeBirthDate(Employee employee)
        {
            return EmployeeManager.GetEmployeeBirthDate(employee);
        }

        #endregion

        #region EmployeeAccount

        private List<EmployeeAccount> GetEmployeeAccountsOnDefaultLevel(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            List<EmployeeAccount> employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, employeeId.ObjToList(), dateFrom, dateTo);
            int defaultEmployeeAccountDimEmployeeAccountDimId = GetCompanyIntSettingFromCache(CompanySettingType.DefaultEmployeeAccountDimEmployee);

            if (defaultEmployeeAccountDimEmployeeAccountDimId > 0)
            {
                var defaultLevelEmployeeAccounts = employeeAccounts.Where(i => i.Account.AccountDimId == defaultEmployeeAccountDimEmployeeAccountDimId).ToList();

                if (defaultLevelEmployeeAccounts.Any())
                    employeeAccounts = defaultLevelEmployeeAccounts;
            }

            return employeeAccounts.Where(i => i.AccountId.HasValue).ToList();
        }

        private List<EmployeeAccount> GetEmployeeAccounts(int employeeId, DateTime dateFrom, DateTime dateTo, bool onlyDefault = false)
        {
            return GetEmployeeAccounts(new List<int>() { employeeId }, dateFrom, dateTo, onlyDefault);
        }

        private List<EmployeeAccount> GetEmployeeAccounts(List<int> employeeIds, DateTime dateFrom, DateTime dateTo, bool onlyDefault = false)
        {
            List<EmployeeAccount> employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, employeeIds, dateFrom, dateTo);
            if (onlyDefault)
                employeeAccounts = employeeAccounts.Where(i => i.Default).ToList();
            return employeeAccounts.Where(i => i.AccountId.HasValue).ToList();
        }

        private Dictionary<int, List<EmployeeAccount>> GetEmployeeAccountsByEmployee(List<int> employeeIds, DateTime dateFrom, DateTime dateTo, bool onlyDefault = false)
        {
            return GetEmployeeAccounts(employeeIds, dateFrom, dateTo, onlyDefault).GroupBy(i => i.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
        }

        #endregion

        #region EmployeePost

        private EmployeePost GetEmployeePost(int employeePostId)
        {
            return (from ep in entities.EmployeePost
                    where ep.EmployeePostId == employeePostId &&
                    ep.State == (int)SoeEntityState.Active
                    select ep).FirstOrDefault();
        }

        private EmployeePost GetEmployeePostWithScheduleCycleRuleTypeAndEmployeeGroup(int employeePostId)
        {
            return (from e in entities.EmployeePost
                    .Include("ScheduleCycle.ScheduleCycleRule.ScheduleCycleRuleType")
                    .Include("EmployeeGroup")
                    where e.EmployeePostId == employeePostId &&
                    e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        private ActionResult IsEmployeeValidForEmployeePost(Employment employment, EmployeePost employeePost, DateTime date)
        {
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));
            if (employeePost == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "EmployeePost");
            if (employment.GetEmployeeGroupId(date) != employeePost.EmployeeGroupId)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(11534, "Anställd och tjänst måste ha samma tidavtal"));
            if (!TimeScheduleManager.EmployeeHasEmployeePostSkills(entities, employment.EmployeeId, employeePost.EmployeePostId, date))
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(11535, "Anställd saknar kompetens som tjänsten kräver"));
            return new ActionResult(true);
        }

        #endregion

        #region Employment

        private DateTime? GetEmploymentEndDate(int employeeId)
        {
            List<Employment> employments = (from e in entities.Employment
                                            where e.EmployeeId == employeeId &&
                                            e.ActorCompanyId == actorCompanyId &&
                                            e.State == (int)SoeEntityState.Active
                                            select e).ToList();

            return employments.GetEndDate();
        }

        #endregion

        #region EmploymentChange

        /// <summary>
        /// Note: Not all changes supported
        /// </summary>
        private List<DateRangeDTO> GetEmploymentChangeRanges(TermGroup_EmploymentChangeFieldType field, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            List<DateRangeDTO> dateRanges = new List<DateRangeDTO>();

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return dateRanges;

            StartRange(dateFrom);
            foreach (var batch in employee.GetDataChanges(dateFrom, dateTo, field).Where(d => d.Key.FromDate.HasValue).Select(d => d.Key))
            {
                string value = GetValue(batch.FromDate.Value);
                if (value == GetLastRange().Value)
                    continue;

                CloseRange(batch.FromDate.Value.AddDays(-1));
                StartRange(batch.FromDate.Value, batch.ToDate, value);
            }

            if (GetLastRange().Stop < dateTo)
                StartRange(GetLastRange().Stop.AddDays(1), dateTo);

            void StartRange(DateTime from, DateTime? to = null, string value = null)
            {
                dateRanges.Add(new DateRangeDTO(from, to ?? dateTo, value ?? GetValue(from)));
            }
            void CloseRange(DateTime to)
            {
                GetLastRange().Stop = to;
            }
            DateRangeDTO GetLastRange()
            {
                return dateRanges.LastOrDefault();
            }
            string GetValue(DateTime date)
            {
                switch (field)
                {
                    case TermGroup_EmploymentChangeFieldType.EmployeeGroupId:
                        return employee.GetEmployeeGroupId(date).ToNullable()?.ToString();
                    case TermGroup_EmploymentChangeFieldType.PayrollGroupId:
                        return employee.GetPayrollGroupId(date)?.ToString();
                    case TermGroup_EmploymentChangeFieldType.Percent:
                        return employee.GetEmployment(date)?.GetPercent(date).ToNullable()?.ToString();
                    case TermGroup_EmploymentChangeFieldType.WorkTimeWeek:
                        return employee.GetEmployment(date)?.GetWorkTimeWeek(date).ToNullable()?.ToString();
                    default:
                        return null;
                }
            }

            return dateRanges;
        }

        #endregion

        #region EmploymentAccountStd

        private List<EmploymentAccountStd> GetEmploymentAccountStds(int employmentId)
        {
            var employmentAccountStds = (from eas in entities.EmploymentAccountStd
                                           .Include("AccountStd.Account")
                                           .Include("AccountInternal.Account.AccountDim")
                                         where eas.EmploymentId == employmentId
                                         select eas).ToList();

            return employmentAccountStds;
        }

        #endregion

        #region EmploymentVacationGroup

        private EmploymentVacationGroup GetEmploymentVacationGroupWithVacationGroup(DateTime date, int employeeId)
        {
            int? employmentId = GetEmployeeFromCache(employeeId)?.GetEmploymentId(date, date);
            if (!employmentId.HasValue)
                return null;

            List<EmploymentVacationGroup> employmentVacationGroups = GetEmploymentVacationGroupWithVacationGroupFromCache(employeeId, employmentId.Value);
            return employmentVacationGroups?.OrderByDescending(evg => evg.FromDate).FirstOrDefault(evg => (!evg.FromDate.HasValue || evg.FromDate.Value <= date));
        }

        private List<EmploymentVacationGroup> GetEmploymentVacationGroupsWithVacationGroup(int employmentId)
        {
            return (from evg in entities.EmploymentVacationGroup
                        .Include("VacationGroup.VacationGroupSE")
                    where evg.EmploymentId == employmentId &&
                    evg.State == (int)SoeEntityState.Active
                    orderby evg.FromDate descending
                    select evg).ToList();
        }

        #endregion

        #region EmployeeVacationSE

        private EmployeeVacationSE GetEmployeeVacationSEDiscardedState(int employeeVacationSEId)
        {
            return (from e in entities.EmployeeVacationSE
                    where e.EmployeeVacationSEId == employeeVacationSEId
                    select e).FirstOrDefault();
        }

        private EmployeeVacationSE GetEmployeeVacationSEFromEmployee(int employeeId)
        {
            return (from e in entities.EmployeeVacationSE
                    where e.EmployeeId == employeeId &&
                    e.State == (int)SoeEntityState.Active
                    orderby e.Created
                    select e).FirstOrDefault();
        }

        #endregion

        #region EmployeeChild

        private EmployeeChild GetEmployeeChild(int employeeChildId)
        {
            return (from ec in entities.EmployeeChild
                    where ec.EmployeeChildId == employeeChildId
                    select ec).FirstOrDefault();
        }

        #endregion

        #region EmployeeGroup

        private List<EmployeeGroup> GetEmployeeGroups()
        {
            return (from eg in entities.EmployeeGroup
                    where eg.ActorCompanyId == actorCompanyId &&
                    eg.State == (int)SoeEntityState.Active
                    select eg).ToList();
        }

        //TODO: Rename method to GetEmployeeGroupsWithDayTypes when EmployeeGroupDayType is used instead of DayTypeEmployeeGroupMapping
        private List<EmployeeGroup> GetEmployeeGroupsWithWeekendSalaryDayTypes()
        {
            return (from eg in entities.EmployeeGroup.Include("EmployeeGroupDayType")
                    where eg.ActorCompanyId == actorCompanyId &&
                    eg.State == (int)SoeEntityState.Active
                    select eg).ToList();
        }

        private List<EmployeeGroup> GetEmployeeGroupsWithDeviationCausesDaytypesAndTransitions()
        {
            return (from eg in entities.EmployeeGroup
                                .Include("TimeDeviationCause")
                                .Include("EmployeeGroupTimeDeviationCause.TimeDeviationCause")
                                .Include("EmployeeGroupTimeDeviationCauseRequest.TimeDeviationCause")
                                .Include("AttestTransition")
                                .Include("DayType")
                    where eg.ActorCompanyId == actorCompanyId &&
                    eg.State == (int)SoeEntityState.Active
                    select eg).ToList();
        }

        #endregion

        #region EmployeeFactor

        private List<EmployeeFactor> GetEmployeeFactors(int employeeId)
        {
            return (from f in entities.EmployeeFactor
                    where f.EmployeeId == employeeId &&
                    f.State == (int)SoeEntityState.Active
                    select f).ToList();
        }

        private List<EmployeeFactor> GetEmployeeFactors(int employeeId, int vacationYearEndRowId)
        {
            return (from f in entities.EmployeeFactor
                    where f.EmployeeId == employeeId &&
                    f.VacationYearEndRowId == vacationYearEndRowId &&
                    f.State == (int)SoeEntityState.Active
                    select f).ToList();
        }

        private decimal? GetEmployeeFactor(int employeeId, TermGroup_EmployeeFactorType type, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            var factor = (from f in entities.EmployeeFactor
                          where f.EmployeeId == employeeId &&
                          f.Type == (int)type &&
                          (f.FromDate <= date.Value || !f.FromDate.HasValue) &&
                          f.State == (int)SoeEntityState.Active
                          orderby f.FromDate descending
                          select f).FirstOrDefault();

            return factor != null ? factor.Factor : (decimal?)null;
        }

        private List<EmployeeFactor> GetEmployeeFactorsForTimeWorkAccount(int timeWorkAccountYearEmployeeId)
        {
            return (from f in entities.EmployeeFactor
                    where f.Type == (int)TermGroup_EmployeeFactorType.TimeWorkAccountPaidLeave &&
                    f.TimeWorkAccountYearEmployeeId == timeWorkAccountYearEmployeeId &&
                    f.State == (int)SoeEntityState.Active
                    select f).ToList();
        }

        #endregion

        #region EmployeeRequest

        private List<EmployeeRequest> GetEmployeeRequests(List<int> employeeIds, DateTime? dateFrom = null, DateTime? dateTo = null, params TermGroup_EmployeeRequestType[] requestTypes)
        {
            List<EmployeeRequest> employeeRequests = (from er in entities.EmployeeRequest
                                                      where er.ActorCompanyId == actorCompanyId &&
                                                      employeeIds.Contains(er.EmployeeId) &&
                                                      er.State == (int)SoeEntityState.Active
                                                      select er).ToList();

            if (dateFrom.HasValue)
            {
                dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
                employeeRequests = employeeRequests.Where(er => er.Start >= dateFrom.Value || er.Stop >= dateFrom.Value).ToList();
            }
            if (dateTo.HasValue)
            {
                dateTo = CalendarUtility.GetEndOfDay(dateTo);
                employeeRequests = employeeRequests.Where(er => er.Start <= dateTo.Value || er.Stop <= dateTo.Value).ToList();
            }
            if (!requestTypes.IsNullOrEmpty())
                employeeRequests = employeeRequests.Where(er => requestTypes.Contains((TermGroup_EmployeeRequestType)er.Type)).ToList();

            return employeeRequests;
        }

        private List<EmployeeRequest> GetEmployeeRequests(int? employeeId, DateTime? dateFrom = null, DateTime? dateTo = null, List<TermGroup_EmployeeRequestType> requestTypes = null, List<TermGroup_EmployeeRequestStatus> requestStatuses = null, List<TermGroup_EmployeeRequestResultStatus> resultStatuses = null, bool ignoreState = false)
        {
            IQueryable<EmployeeRequest> query = (from er in entities.EmployeeRequest
                                                  .Include("Employee")
                                                  .Include("Employee.ContactPerson")
                                                  .Include("TimeDeviationCause")
                                                  .Include("EmployeeChild")
                                                 where er.ActorCompanyId == actorCompanyId &&
                                                 (ignoreState || (er.State == (int)SoeEntityState.Active))
                                                 select er);

            if (employeeId.HasValue)
                query = query.Where(er => er.EmployeeId == employeeId.Value);
            if (!requestTypes.IsNullOrEmpty())
            {
                List<int> l = requestTypes.Select(type => (int)type).ToList();
                query = query.Where(er => l.Contains(er.Type));
            }
            if (dateFrom.HasValue)
            {
                dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
                query = query.Where(er => er.Stop >= dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                dateTo = CalendarUtility.GetEndOfDay(dateTo);
                query = query.Where(er => er.Start <= dateTo.Value);
            }

            List<EmployeeRequest> requests = query.ToList();
            if (!requests.Any())
                return requests;

            if (requestStatuses != null)
            {
                List<int> l = requestStatuses.Select(status => (int)status).ToList();
                requests = requests.Where(er => l.Contains(er.Status)).ToList();
            }
            if (resultStatuses != null)
            {
                List<int> l = resultStatuses.Select(resultStatus => (int)resultStatus).ToList();
                requests = requests.Where(er => l.Contains(er.ResultStatus)).ToList();
            }
            if (employeeId.HasValue)
            {
                if (requestTypes.Contains(TermGroup_EmployeeRequestType.InterestRequest) && requestTypes.Contains(TermGroup_EmployeeRequestType.NonInterestRequest))
                    return requests.OrderBy(d => d.Start).ThenBy(d => d.Stop).ToList();
                else
                    return requests.OrderByDescending(d => d.Start).ThenByDescending(d => d.Stop).ToList();
            }

            //Return requests for user attest roles
            List<EmployeeRequest> userAttestRoleRequests = new List<EmployeeRequest>();
            List<Employee> attestRoleEmployees = EmployeeManager.GetEmployeesForUsersAttestRoles(out _, actorCompanyId, userId, 0, addEmployeeAuthModelInfo: true);
            User user = GetUserFromCache();
            Employee userEmployee = user != null ? EmployeeManager.GetEmployeeForUser(user.UserId, actorCompanyId) : null;

            foreach (EmployeeRequest request in requests)
            {
                if (userEmployee != null && request.EmployeeId == userEmployee.EmployeeId && !requestTypes.Contains(TermGroup_EmployeeRequestType.InterestRequest) && !requestTypes.Contains(TermGroup_EmployeeRequestType.NonInterestRequest))
                    continue;

                Employee attestRoleEmployee = attestRoleEmployees.FirstOrDefault(e => e.EmployeeId == request.EmployeeId);
                if (attestRoleEmployee != null)
                {
                    request.CategoryNames = attestRoleEmployee.CategoryNames;
                    request.AccountNames = attestRoleEmployee.AccountNames;
                    userAttestRoleRequests.Add(request);
                }
            }

            return userAttestRoleRequests.OrderByDescending(d => d.Start).ThenByDescending(d => d.Stop).ToList();
        }

        private EmployeeRequest GetEmployeeRequest(int employeeId, DateTime start, DateTime stop, TermGroup_EmployeeRequestType requestType)
        {
            var query = (from er in entities.EmployeeRequest
                            .Include("Employee")
                            .Include("Employee.ContactPerson")
                            .Include("TimeDeviationCause")
                            .Include("ExtendedAbsenceSetting")
                            .Include("EmployeeChild")
                         where er.EmployeeId == employeeId &&
                         er.State == (int)SoeEntityState.Active &&
                         ((er.Start <= start && er.Stop >= start) || (er.Start <= stop && er.Stop >= start)) // Start or stop is in request intervall
                         select er);

            // Filter on request type
            if (requestType != TermGroup_EmployeeRequestType.Undefined)
                query = query.Where(er => er.Type == (int)requestType);

            return query.FirstOrDefault(x => x.Status != (int)TermGroup_EmployeeRequestStatus.Definate && x.Status != (int)TermGroup_EmployeeRequestStatus.Restored);
        }

        private EmployeeRequest GetEmployeeRequest(int employeeRequestId)
        {
            return (from er in entities.EmployeeRequest
                        .Include("Employee")
                        .Include("Employee.ContactPerson")
                        .Include("TimeDeviationCause")
                        .Include("ExtendedAbsenceSetting")
                        .Include("EmployeeChild")
                    where er.EmployeeRequestId == employeeRequestId
                    select er).FirstOrDefault();
        }

        private List<EmployeeRequest> GetEmployeeRequests(List<int> employeeRequestIds, bool includeExtended)
        {
            IQueryable<EmployeeRequest> query = (from e in entities.EmployeeRequest
                                                 where employeeRequestIds.Contains(e.EmployeeRequestId) &&
                                                    e.ActorCompanyId == actorCompanyId &&
                                                    e.State == (int)SoeEntityState.Active
                                                 select e);

            if (includeExtended)
                query = query.Include("ExtendedAbsenceSetting");


            return query.ToList();
        }
        /// <summary>
        /// Get one EmployeeRequest with exact specified time range
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="start">Start date and time</param>
        /// <param name="stop">Stop date and time</param>
        /// <param name="requestType">Request type, use 'Undefined' if no condition</param>
        /// <param name="onlyAllowOne">If true, null will be returned if more than one request is found</param>
        /// <returns></returns>
        private EmployeeRequest GetEmployeeRequestByExactTime(int employeeId, DateTime start, DateTime stop, TermGroup_EmployeeRequestType requestType, bool onlyAllowOne = true)
        {
            var query = (from er in entities.EmployeeRequest
                         where er.EmployeeId == employeeId &&
                                er.State == (int)SoeEntityState.Active &&
                                er.Start == start &&
                                er.Stop == stop
                         select er);

            // Filter on request type
            if (requestType != TermGroup_EmployeeRequestType.Undefined)
                query = query.Where(er => er.Type == (int)requestType);

            // Execute query
            List<EmployeeRequest> result = query.ToList();

            if (!onlyAllowOne || result.Count == 1)
                return result.FirstOrDefault();

            return null;
        }

        private bool DateIntervalIntersectsWithExistingEmployeeRequest(DateTime start, DateTime stop, int employeeId, int employeeRequestId, TermGroup_EmployeeRequestType requestType, out String message)
        {
            List<EmployeeRequest> requests = null;
            StringBuilder sb = new StringBuilder();

            //New request
            if (employeeRequestId == 0)
            {
                requests = (from er in entities.EmployeeRequest
                            .Include("TimeDeviationCause")
                            where er.EmployeeId == employeeId &&
                            er.Type == (int)requestType &&
                              er.Status != (int)TermGroup_EmployeeRequestStatus.Restored && er.Status != (int)TermGroup_EmployeeRequestStatus.Definate
                            && (
                              (start >= er.Start && start <= er.Stop) || //newstart is between existing start and stop
                              (stop >= er.Start && stop <= er.Stop) || //newstop is between existing start and stop
                              (start <= er.Start && stop >= er.Stop)     //newstart and newstop spans existing start and stop
                            ) &&
                            er.State == (int)SoeEntityState.Active
                            select er).ToList();
            }
            else
            {
                requests = (from er in entities.EmployeeRequest
                            .Include("TimeDeviationCause")
                            where er.EmployeeId == employeeId &&
                            er.EmployeeRequestId != employeeRequestId &&
                            er.Type == (int)requestType &&
                              er.Status != (int)TermGroup_EmployeeRequestStatus.Restored && er.Status != (int)TermGroup_EmployeeRequestStatus.Definate
                            && (
                              (start >= er.Start && start <= er.Stop) || //newstart is between existing start and stop
                              (stop >= er.Start && stop <= er.Stop) || //newstop is between existing start and stop
                              (start <= er.Start && stop >= er.Stop)     //newstart and newstop spans existing start and stop
                            ) &&
                            er.State == (int)SoeEntityState.Active
                            select er).ToList();
            }

            if (requests.Any())
            {
                sb.Append(GetText(8476, "Ansökan krockar med följande ansökningar:") + "\n");

                foreach (var request in requests)
                {
                    sb.Append(GetText(8270, "Ledighetsansökan") + " - " + request.TimeDeviationCauseName + ": ");
                    sb.Append(request.Start.ToShortDateString() + " - " + request.Stop.ToShortDateString());
                    sb.Append("\n");
                }
            }

            message = sb.ToString();

            return requests.Any();
        }

        /// <summary>
        /// Create a new request for the intersected daterange
        /// </summary>        
        private EmployeeRequestDTO CreateIntersectedRequest(EmployeeRequest request, DateRangeDTO dateRange, bool isShortenPlacement)
        {
            EmployeeRequestDTO newRequest = null;

            if (CalendarUtility.IsDateInRange(dateRange.Start.Date, request.Start.Date, request.Stop.Date) || CalendarUtility.IsDateInRange(dateRange.Stop, request.Start.Date, request.Stop.Date))
            {
                if (CalendarUtility.IsNewOverlappedByCurrent(dateRange.Start, dateRange.Stop, request.Start.Date, request.Stop.Date))
                {
                    DateTime start = isShortenPlacement ? dateRange.Start.AddDays(1) : dateRange.Start;
                    newRequest = request.CopyAsNew(start, dateRange.Stop);
                }
                else if (CalendarUtility.IsNewStartInCurrent(dateRange.Start, dateRange.Stop, request.Start.Date, request.Stop.Date))
                {
                    DateTime start = isShortenPlacement ? dateRange.Start.AddDays(1) : dateRange.Start;
                    newRequest = request.CopyAsNew(start, request.Stop);
                }
                else if (CalendarUtility.IsNewStopInCurrent(dateRange.Start, dateRange.Stop, request.Start.Date, request.Stop.Date))
                {
                    newRequest = request.CopyAsNew(request.Start, dateRange.Stop);
                }
            }

            return newRequest;
        }
        #endregion

        #region EmployeeSchedule

        private List<EmployeeSchedule> GetEmployeeSchedulesForEmployeeWithScheduleTemplate(int employeeId, DateTime startDate, DateTime stopDate)
        {
            return (from es in entities.EmployeeSchedule
                    .Include("TimeScheduleTemplateHead")
                    .Include("TimeScheduleTemplateHead.TimeScheduleTemplatePeriod")
                    where es.EmployeeId == employeeId &&
                    ((startDate >= es.StartDate && startDate <= es.StopDate) ||
                    (stopDate >= es.StartDate && stopDate <= es.StopDate) ||
                    (startDate <= es.StartDate && stopDate >= es.StopDate)) &&
                    es.TimeScheduleTemplateHead.ActorCompanyId == actorCompanyId &&
                    es.State != (int)SoeEntityState.Deleted
                    orderby es.StartDate
                    select es).ToList();
        }

        private List<EmployeeSchedule> GetEmployeeSchedulesForEmployee(int employeeId, int? employeeScheduleIdToExclude = null)
        {
            return (from es in entities.EmployeeSchedule
                    where es.EmployeeId == employeeId &&
                    (!employeeScheduleIdToExclude.HasValue || employeeScheduleIdToExclude.Value != es.EmployeeScheduleId) &&
                    es.State == (int)SoeEntityState.Active
                    select es).ToList();
        }

        private List<EmployeeSchedule> GetEmployeeScheduleForTemplateHeadId(int timeScheduleTemplateHeadId)
        {
            return (from es in entities.EmployeeSchedule
                    where es.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId &&
                    es.TimeScheduleTemplateHead.ActorCompanyId == actorCompanyId &&
                    es.State == (int)SoeEntityState.Active
                    select es).ToList();
        }

        private List<int> GetEmployeeIdsWithEmployeeSchedules(List<int> employeeIds, DateTime date)
        {
            return (from es in entities.EmployeeSchedule
                    where es.TimeScheduleTemplateHead.ActorCompanyId == actorCompanyId &&
                    employeeIds.Contains(es.EmployeeId) &&
                    (es.StartDate <= date && es.StopDate >= date) &&
                    es.State == (int)SoeEntityState.Active
                    select es.EmployeeId).Distinct().ToList();
        }

        private List<EmployeeSchedule> GetEmployeeSchedules(List<int> employeeIds)
        {
            return (from es in entities.EmployeeSchedule
                    where es.TimeScheduleTemplateHead.ActorCompanyId == actorCompanyId &&
                    employeeIds.Contains(es.EmployeeId) &&
                    es.State == (int)SoeEntityState.Active
                    select es).ToList();
        }

        private EmployeeSchedule GetEmployeeSchedule(int employeeScheduleId, int employeeId)
        {
            return (from es in entities.EmployeeSchedule
                    where es.EmployeeScheduleId == employeeScheduleId &&
                    es.EmployeeId == employeeId &&
                    es.State == (int)SoeEntityState.Active
                    select es).FirstOrDefault();
        }

        private EmployeeSchedule GetEmployeeScheduleWithEmployee(int employeeScheduleId, int employeeId)
        {
            return (from es in entities.EmployeeSchedule
                        .Include("Employee")
                    where es.EmployeeScheduleId == employeeScheduleId &&
                    es.EmployeeId == employeeId &&
                    es.State == (int)SoeEntityState.Active
                    select es).FirstOrDefault();
        }

        private EmployeeSchedule GetEmployeeScheduleForEmployeeWithTemplateHeadAndPeriod(int employeeId, DateTime date)
        {
            //Important that time not is considered
            date = date.Date;

            return (from es in entities.EmployeeSchedule.Include("TimeScheduleTemplateHead.TimeScheduleTemplatePeriod")
                    where es.EmployeeId == employeeId &&
                    es.StartDate <= date &&
                    es.StopDate >= date &&
                    es.State == (int)SoeEntityState.Active
                    orderby es.TimeScheduleTemplateHead.StartDate descending
                    select es).FirstOrDefault();
        }

        private EmployeeSchedule GetEmployeeSchedule(int employeeId, DateTime date)
        {
            return (from es in entities.EmployeeSchedule
                    where es.EmployeeId == employeeId &&
                    es.StartDate <= date &&
                    es.StopDate >= date &&
                    es.State == (int)SoeEntityState.Active
                    select es).FirstOrDefault();
        }

        private bool HasEmployeePlacement(int employeeId, TimePeriod timePeriod)
        {
            if (timePeriod == null)
                return false;
            if (timePeriod.ExtraPeriod && !timePeriod.PaymentDate.HasValue)
                return false;

            DateTime startDate = timePeriod.ExtraPeriod ? timePeriod.PaymentDate.Value : timePeriod.StartDate;
            DateTime stopDate = timePeriod.ExtraPeriod ? timePeriod.PaymentDate.Value : timePeriod.StopDate;
            return HasEmployeePlacement(employeeId, startDate, stopDate);
        }

        private bool HasEmployeePlacement(int employeeId, DateTime startDate, DateTime stopDate, List<EmployeeSchedule> employeeSchedules = null)
        {
            if (employeeSchedules == null)
                employeeSchedules = GetEmployeeSchedulesForEmployee(employeeId);
            return employeeSchedules.Filter(startDate, stopDate).Any();
        }

        private ActionResult IsOkToDeleteEmployeeSchedule(string employeeInfo, List<TimeBlock> timeBlocks, List<TimeScheduleTemplateBlock> templateBlocks = null, ActivateScheduleControlDTO control = null)
        {
            return IsOkToDeleteEmployeeSchedule(employeeInfo, timeBlocks, templateBlocks, control?.DiscardCheckesAll ?? false, control?.DiscardCheckesForAbsence ?? false, control?.DiscardCheckesForManuallyAdjusted ?? false);
        }

        private ActionResult IsOkToDeleteEmployeeSchedule(string employeeInfo, List<TimeBlock> timeBlocks, List<TimeScheduleTemplateBlock> templateBlocks, bool discardCheckesAll, bool discardCheckesForAbsence, bool discardCheckesForManuallyAdjusted)
        {
            if (!timeBlocks.IsNullOrEmpty())
            {
                //If discardChecks are true, only check that transactions are not transferred to salary
                if (discardCheckesAll)
                {
                    //Check salary TimePayrollTransactions
                    int salaryPayrollAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus);
                    if (salaryPayrollAttestStateId > 0 && timeBlocks.Any(b => b.TimePayrollTransaction.Any(t => t.AttestStateId == salaryPayrollAttestStateId && t.State == (int)SoeEntityState.Active)))
                        return new ActionResult(false, (int)ActionResultSelect.TransactionsAreTransferredToSalary, GetText(5764, "Det finns transaktioner som är överförda till lön på perioden") + "\n" + employeeInfo);
                    //Check salary TimeInvoiceTransactions
                    int salaryInvoiceAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportInvoiceResultingAttestStatus);
                    if (salaryInvoiceAttestStateId > 0 && timeBlocks.Any(b => b.TimeInvoiceTransaction.Any(t => t.AttestStateId == salaryInvoiceAttestStateId && t.State == (int)SoeEntityState.Active)))
                        return new ActionResult(false, (int)ActionResultSelect.TransactionsAreTransferredToSalary, GetText(5764, "Det finns transaktioner som är överförda till lön på perioden") + "\n" + employeeInfo);
                }
                else
                {
                    //Check AbsenceApproved TimeBlocks
                    if (!discardCheckesForAbsence && templateBlocks != null && templateBlocks.Any(tb => tb.ShiftUserStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved && tb.State == (int)SoeEntityState.Active))
                        return new ActionResult(false, (int)ActionResultSelect.TimeBlocksAreManuallyAdded, GetText(5934, "Det finns pass som har frånvaro. Frånvaro eller frånvaroansökan måste återställas") + "\n" + employeeInfo);
                    //Check ManuallyAdjusted TimeBlocks
                    if (!discardCheckesForManuallyAdjusted && timeBlocks != null && timeBlocks.Any(tb => tb.ManuallyAdjusted && tb.State == (int)SoeEntityState.Active))
                        return new ActionResult(false, (int)ActionResultSelect.TimeBlocksAreManuallyAdded, GetText(4512, "Det finns manuellt ändrade tider under perioden") + "\n" + employeeInfo);
                    //Check ManuallyAdded TimePayrollTransactions
                    if (timeBlocks.Any(b => b.TimePayrollTransaction.Any(t => t.ManuallyAdded && t.State == (int)SoeEntityState.Active)))
                        return new ActionResult(false, (int)ActionResultSelect.TransactionsAreManuallyAdded, GetText(4513, "Det finns manuellt tillagda transaktioner på perioden") + "\n" + employeeInfo);
                    //Check ManuallyAdded TimeInvoiceTransactions
                    if (timeBlocks.Any(b => b.TimeInvoiceTransaction.Any(t => t.ManuallyAdded && t.State == (int)SoeEntityState.Active)))
                        return new ActionResult(false, (int)ActionResultSelect.TransactionsAreTransferredToSalary, GetText(4513, "Det finns manuellt tillagda transaktioner på perioden") + "\n" + employeeInfo);
                    //Check attested TimePayrollTransactions
                    AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitialPayroll != null && timeBlocks.Any(b => b.TimePayrollTransaction.Any(t => t.AttestStateId != attestStateInitialPayroll.AttestStateId && t.State == (int)SoeEntityState.Active)))
                        return new ActionResult(false, (int)ActionResultSelect.TransactionsAreHasNotIntialState, GetText(5763, "Det finns transaktioner som är attesterade på perioden") + "\n" + employeeInfo);
                    //Check attested TimeInvoiceTransactions
                    AttestStateDTO attestStateInitialInvoice = GetAttestStateInitialFromCache(TermGroup_AttestEntity.InvoiceTime);
                    if (attestStateInitialInvoice != null && timeBlocks.Any(b => b.TimeInvoiceTransaction.Any(t => t.AttestStateId != attestStateInitialInvoice.AttestStateId && t.State == (int)SoeEntityState.Active)))
                        return new ActionResult(false, (int)ActionResultSelect.TransactionsAreHasNotIntialState, GetText(5763, "Det finns transaktioner som är attesterade på perioden") + "\n" + employeeInfo);
                }
            }

            return new ActionResult(true);
        }

        private ActionResult IsPlacementDatesValid(List<SaveEmployeeSchedulePlacementItem> items)
        {
            items = items.OrderBy(i => i.EmployeeScheduleStartDate).ToList();
            for (int outer = 0; outer < items.Count; outer++)
            {
                for (int inner = 0; inner < items.Count; inner++)
                {
                    //Dont compare with itself
                    if (outer == inner)
                        continue;

                    var outerItem = items[outer];
                    var innerItem = items[inner];

                    //New EmployeeSchedule cannot overlap existing
                    if (CalendarUtility.IsDatesOverlappingNullable(outerItem.StartDate, outerItem.StopDate, innerItem.StartDate, innerItem.StopDate, true))
                        return new ActionResult((int)ActionResultSave.EmployeeScheduleOverlappingDates, GetText(3593, "Det aktiverade schemat överlappar ett annat aktiverat schema" + " " + outerItem.EmployeeInfo));
                }
            }

            return new ActionResult(true);
        }

        private ActionResult IsPlacementDatesValid(int employeeId, int? employeeScheduleIdToExclude, DateTime startDate, DateTime stopDate)
        {
            //Check dates
            List<EmployeeSchedule> employeeSchedules = GetEmployeeSchedulesForEmployee(employeeId, employeeScheduleIdToExclude);
            foreach (EmployeeSchedule employeeSchedule in employeeSchedules)
            {
                //Dont compare with itself (should already be taken care of in method above)
                if (employeeScheduleIdToExclude.HasValue && employeeScheduleIdToExclude.Value == employeeSchedule.EmployeeScheduleId)
                    continue;

                //New EmployeeSchedule cannot overlap existing
                if (CalendarUtility.IsDatesOverlapping(startDate, stopDate, employeeSchedule.StartDate, employeeSchedule.StopDate, validateDatesAreTouching: true))
                    return new ActionResult((int)ActionResultSave.EmployeeScheduleOverlappingDates, GetText(3593, "Det aktiverade schemat överlappar ett annat aktiverat schema"));
            }

            return new ActionResult(true);
        }

        #endregion

        #region EmployeeTimePeriod

        private List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithTimePeriod(int employeeId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("TimePeriod")
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        private EmployeeTimePeriod GetEmployeeTimePeriod(int timePeriodId, int employeeId)
        {
            return (from etp in entities.EmployeeTimePeriod
                    where etp.TimePeriodId == timePeriodId &&
                    etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }

        private List<EmployeeTimePeriod> GetEmployeeTimePeriods(List<int> timePeriodIds, int employeeId)
        {
            return (from etp in entities.EmployeeTimePeriod
                    where timePeriodIds.Contains(etp.TimePeriodId) &&
                    etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        private List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithValuesAndWarnings(int timePeriodId, List<int> employeeIds)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                        .Include("PayrollControlFunctionOutcome")
                    where etp.TimePeriodId == timePeriodId &&
                     employeeIds.Contains(etp.EmployeeId) &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }

        private EmployeeTimePeriod GetEmployeeTimePeriodWithValues(int timePeriodId, int employeeId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                    where etp.TimePeriodId == timePeriodId &&
                    etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }

        private List<EmployeeTimePeriod> GetEmployeeTimePeriodsBasedOnPayrollStopDateWithTimePeriod(int employeeId, DateTime startDate, DateTime stopDate)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("TimePeriod")
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    (
                        (etp.TimePeriod.PayrollStartDate >= startDate && etp.TimePeriod.PayrollStartDate <= stopDate) ||
                        (etp.TimePeriod.PayrollStopDate >= startDate && etp.TimePeriod.PayrollStopDate <= stopDate)
                    ) &&
                    etp.State == (int)SoeEntityState.Active
                    orderby etp.TimePeriod.StopDate
                    select etp).ToList();
        }

        private EmployeeTimePeriod GetEmployeeTimePeriodWithTimePeriod(int employeeId, DateTime date)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("TimePeriod")
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.TimePeriod.StartDate <= date &&
                    etp.TimePeriod.StopDate >= date &&
                    etp.State == (int)SoeEntityState.Active
                    orderby etp.TimePeriod.StopDate
                    select etp).FirstOrDefault();
        }

        private EmployeeTimePeriod GetNextEmployeeTimePeriod(int employeeId, DateTime date)
        {
            return (from etp in entities.EmployeeTimePeriod
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.TimePeriod.StopDate > date &&
                    etp.State == (int)SoeEntityState.Active
                    orderby etp.TimePeriod.StopDate
                    select etp).FirstOrDefault();
        }

        private EmployeeTimePeriod GetEmployeeTimePeriodWithValueAndSettings(int timePeriodId, int employeeId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("EmployeeTimePeriodValue")
                        .Include("EmployeeTimePeriodProductSetting")
                    where etp.TimePeriodId == timePeriodId &&
                    etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }
        private EmployeeTimePeriod GetEmployeeTimePeriodWithOutcome(int timePeriodId, int employeeId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("PayrollControlFunctionOutcome")
                    where etp.TimePeriodId == timePeriodId &&
                    etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active
                    select etp).FirstOrDefault();
        }
        private List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithOutcome(int timePeriodId, List<int> employeeIds)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("PayrollControlFunctionOutcome")
                    where etp.TimePeriodId == timePeriodId &&
                            employeeIds.Contains(etp.EmployeeId) &&
                            etp.ActorCompanyId == actorCompanyId &&
                            etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }
        private List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithOutcome(List<TimePeriod> timePeriods, int employeeId)
        {
            List<int> timePeriodIds = timePeriods.Select(i => i.TimePeriodId).ToList();

            return (from etp in entities.EmployeeTimePeriod
                        .Include("PayrollControlFunctionOutcome")
                    where timePeriodIds.Contains(etp.TimePeriodId) &&
                            etp.EmployeeId == employeeId &&
                            etp.ActorCompanyId == actorCompanyId &&
                            etp.State == (int)SoeEntityState.Active
                    select etp).ToList();
        }
        private EmployeeTimePeriod GetLastPaidEmployeeTimePeriodWithTimePeriod(int employeeId)
        {
            return (from etp in entities.EmployeeTimePeriod
                        .Include("TimePeriod")
                    where etp.EmployeeId == employeeId &&
                    etp.ActorCompanyId == actorCompanyId &&
                    etp.State == (int)SoeEntityState.Active &&
                    etp.Status == (int)SoeEmployeeTimePeriodStatus.Paid
                    select etp).OrderByDescending(i => i.TimePeriod.StopDate).FirstOrDefault();
        }

        private List<EmployeeTimePeriod> GetEmployeeTimePeriodsWithPeriodsUsePayrollDates(DateTime dateFrom, DateTime? dateTo, int employeeId)
        {
            List<EmployeeTimePeriod> validEmployeePeriods = new List<EmployeeTimePeriod>();

            List<EmployeeTimePeriod> employeePeriods = (from etp in entities.EmployeeTimePeriod
                                                            .Include("TimePeriod")
                                                        where etp.EmployeeId == employeeId &&
                                                        etp.ActorCompanyId == actorCompanyId &&
                                                        (etp.TimePeriod.StartDate.Year >= dateFrom.Year ||
                                                        (etp.TimePeriod.PayrollStartDate.HasValue && etp.TimePeriod.PayrollStartDate.Value.Year >= dateFrom.Year)) &&
                                                        (etp.Status == (int)SoeEmployeeTimePeriodStatus.Locked || etp.Status == (int)SoeEmployeeTimePeriodStatus.Paid) &&
                                                        etp.State == (int)SoeEntityState.Active
                                                        select etp).ToList();

            foreach (EmployeeTimePeriod period in employeePeriods)
            {
                if (period.TimePeriod.HasPayrollDates() && CalendarUtility.IsDatesOverlappingNullable(dateFrom, dateTo, period.TimePeriod.PayrollStartDate.Value, period.TimePeriod.PayrollStopDate.Value))
                    validEmployeePeriods.Add(period);
            }

            return validEmployeePeriods;
        }

        private bool IsEmployeeTimePeriodLockedForChanges(int employeeId, int? timePeriodId = null, DateTime? date = null)
        {
            List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriodsFromCache(employeeId);
            if (employeeTimePeriods.IsNullOrEmpty())
                return false;

            EmployeeTimePeriod employeeTimePeriod = null;
            if (timePeriodId.HasValue)
                employeeTimePeriod = employeeTimePeriods.FirstOrDefault(i => i.TimePeriodId == timePeriodId.Value);
            else if (date.HasValue)
                employeeTimePeriod = employeeTimePeriods.FirstOrDefault(i => i.TimePeriod != null && !i.TimePeriod.ExtraPeriod && i.TimePeriod.StartDate <= date.Value.Date && i.TimePeriod.StopDate >= date.Value.Date);

            return employeeTimePeriod != null && employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Open;
        }

        private ActionResult SaveEmployeeTimePeriodValue(EmployeeTimePeriod employeeTimePeriod, SoeEmployeeTimePeriodValueType type, decimal value, bool saveChanges)
        {
            ActionResult result = new ActionResult(true);

            if (employeeTimePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeTimePeriod");

            if (employeeTimePeriod.EmployeeTimePeriodValue == null)
                employeeTimePeriod.EmployeeTimePeriodValue = new EntityCollection<EmployeeTimePeriodValue>();

            EmployeeTimePeriodValue employeePeriodValueTax = employeeTimePeriod.EmployeeTimePeriodValue.FirstOrDefault(i => i.Type == (int)type);
            if (employeePeriodValueTax == null)
            {
                #region Add

                employeePeriodValueTax = new EmployeeTimePeriodValue()
                {
                    Type = (int)type,

                    //References
                    EmployeeTimePeriod = employeeTimePeriod,
                };
                entities.EmployeeTimePeriodValue.AddObject(employeePeriodValueTax);
                SetCreatedProperties(employeePeriodValueTax);

                #endregion
            }

            //Set value
            employeePeriodValueTax.Value = value;

            if (saveChanges)
                result = Save();

            return result;
        }

        #endregion

        #region EmployeeTimeWorkAccount

        public List<EmployeeTimeWorkAccount> GetTimeWorkAccountsForEmployees(List<int> timeWorkAccountIds, List<int> employeeIds)
        {
            if (timeWorkAccountIds.IsNullOrEmpty() && employeeIds.IsNullOrEmpty())
                return new List<EmployeeTimeWorkAccount>();

            var query = entities.EmployeeTimeWorkAccount.Where(e => e.State == (int)SoeEntityState.Active);
            if (!timeWorkAccountIds.IsNullOrEmpty())
                query = query.Where(e => timeWorkAccountIds.Contains(e.TimeWorkAccountId));
            if (!employeeIds.IsNullOrEmpty())
                query = query.Where(e => employeeIds.Contains(e.EmployeeId));
            return query.ToList();
        }

        public List<EmployeeTimeWorkAccount> GetTimeWorkAccountsForEmployees(int timeWorkAccountId, List<int> employeeIds)
        {
            var query = entities.EmployeeTimeWorkAccount.Where(e => e.TimeWorkAccountId == timeWorkAccountId && e.State == (int)SoeEntityState.Active);
            if (!employeeIds.IsNullOrEmpty())
                query = query.Where(e => employeeIds.Contains(e.EmployeeId));
            return query.ToList();
        }

        public List<EmployeeTimeWorkAccount> GetTimeWorkAccountsForEmployees(int timeWorkAccountId, int employeeId)
        {
            return entities.EmployeeTimeWorkAccount.Where(e => e.TimeWorkAccountId == timeWorkAccountId && e.EmployeeId == employeeId && e.State == (int)SoeEntityState.Active).ToList();
        }

        public List<EmployeeTimeWorkAccount> GetTimeWorkAccountsForEmployee(int employeeId)
        {
            return entities.EmployeeTimeWorkAccount.Where(e => e.EmployeeId == employeeId && e.State == (int)SoeEntityState.Active).ToList();
        }

        #endregion

        #region EmployeeUnionFee

        private List<EmployeeUnionFee> GetEmployeeUnionFees(int employeeId)
        {
            return (from e in entities.EmployeeUnionFee
                    where e.EmployeeId == employeeId &&
                    e.State == (int)SoeEntityState.Active
                    select e).ToList();
        }

        #endregion

        #region Expense

        private ExpenseRow GetExpenseRowWithHeadAndInvoiceRow(int expenseRowId)
        {
            return (from er in entities.ExpenseRow
                    .Include("ExpenseHead")
                    .Include("CustomerInvoiceRow")
                    where er.ExpenseRowId == expenseRowId &&
                    er.State == (int)SoeEntityState.Active
                    select er).FirstOrDefault();
        }

        private ExpenseRow GetExpenseRowWithHeadAndInvoiceRowAndTransactions(int expenseRowId)
        {
            return (from er in entities.ExpenseRow
                   .Include("ExpenseHead")
                   .Include("CustomerInvoiceRow")
                   .Include("TimeCodeTransaction.TimePayrollTransaction")
                    where er.ExpenseRowId == expenseRowId &&
                    er.State == (int)SoeEntityState.Active
                    select er).FirstOrDefault();
        }

        #endregion

        #region FixedPayrollRow

        private FixedPayrollRow GetFixedPayrollRow(int fixedPayrollRowId)
        {
            return entities.FixedPayrollRow.FirstOrDefault(f => f.FixedPayrollRowId == fixedPayrollRowId && f.State == (int)SoeEntityState.Active);
        }

        private List<FixedPayrollRow> GetFixedPayrollRows(int employeeId)
        {
            return (from f in entities.FixedPayrollRow
                    where f.ActorCompanyId == this.actorCompanyId &&
                    f.EmployeeId == employeeId &&
                    f.State == (int)SoeEntityState.Active
                    select f).ToList();
        }

        private List<FixedPayrollRow> GetFixedPayrollRows(List<int> employeeIds)
        {
            if (employeeIds.IsNullOrEmpty())
                return null;

            return (from f in entities.FixedPayrollRow
                    where f.ActorCompanyId == this.actorCompanyId &&
                    employeeIds.Contains(f.EmployeeId) &&
                    f.State == (int)SoeEntityState.Active
                    select f).ToList();
        }

        #endregion

        #region Holiday

        private List<HolidayDTO> GetHolidaySalaryHolidays(DateTime dateFrom, DateTime dateTo)
        {
            return CalendarManager.GetHolidaySalaryHolidays(entities, dateFrom, dateTo, actorCompanyId);
        }

        private List<HolidayDTO> GetHolidaysWithDayType(DateTime? fromDate = null)
        {
            DateTime thresholdDate = fromDate.HasValue ? fromDate.Value.AddYears(-1) : DateTime.MinValue;

            var holidays = entities.Holiday
                .Include(h => h.DayType)
                .Where(h => h.ActorCompanyId == actorCompanyId &&
                            h.State == (int)SoeEntityState.Active &&
                            (h.Date >= thresholdDate || h.SysHolidayTypeId.HasValue))
                .ToList();

            return CalendarManager.AddDatesFromSysHoliday(holidays.ToDTOs(true));
        }

        private List<HolidayDTO> GetHolidaysWithDayTypeAndHalfDaySettings(DateTime? fromDate = null)
        {
            if (fromDate.HasValue)
                fromDate = fromDate.Value.AddYears(-1); //do not fetch holidays older than one year

            var holidays = (from h in entities.Holiday
                                .Include("DayType.TimeHalfday.TimeCodeBreak")
                                .Include("DayType.EmployeeGroup")
                            where h.ActorCompanyId == actorCompanyId &&
                            h.State == (int)SoeEntityState.Active &&
                            (!fromDate.HasValue || (h.Date >= fromDate.Value || h.SysHolidayTypeId.HasValue))
                            select h).ToList();

            return CalendarManager.AddDatesFromSysHoliday(holidays.ToDTOs(true).ToList());
        }

        private List<HolidayDTO> GetHolidaysFromDayType(int dayTypeId)
        {
            DayType dayType = GetDayTypeWithHoliday(dayTypeId);
            if (dayType == null || dayType.Holiday == null)
                return new List<HolidayDTO>();

            return CalendarManager.AddDatesFromSysHoliday(dayType.Holiday.ToDTOs(true).ToList());
        }

        private HolidayDTO GetHoliday(int holidayId, int year)
        {

            var holiday = (from h in entities.Holiday
                           where h.ActorCompanyId == actorCompanyId &&
                           h.HolidayId == holidayId &&
                           h.State == (int)SoeEntityState.Active
                           select h).FirstOrDefault();

            if (holiday != null)
            {
                var dtos = CalendarManager.AddDatesFromSysHoliday(new List<HolidayDTO>() { holiday.ToDTO(true) });
                if (dtos.Count > 1)
                    return dtos.FirstOrDefault(w => w.Date.Year == year);
                else
                    return dtos.FirstOrDefault();
            }
            return null;
        }

        private List<HolidayDTO> FilterHolidays(List<HolidayDTO> holidays, DateTime date)
        {
            return holidays?.Where(i => i.Date.Date == date.Date).ToList() ?? new List<HolidayDTO>();
        }

        private List<HolidayDTO> FilterHolidays(List<HolidayDTO> holidays, DateTime date, EmployeeGroup employeeGroup, Dictionary<int, List<HolidayDTO>> holidayDict)
        {
            List<HolidayDTO> holidaysForEmployeeGroup = new List<HolidayDTO>();

            if (employeeGroup != null)
            {
                if (holidayDict.ContainsKey(employeeGroup.EmployeeGroupId))
                {
                    holidaysForEmployeeGroup = holidayDict[employeeGroup.EmployeeGroupId];
                }
                else
                {
                    if (holidays != null)
                    {
                        holidaysForEmployeeGroup = holidays.Where(h => h.DayType != null && h.DayType.EmployeeGroups != null && h.DayType.EmployeeGroups.Any(eg => eg.EmployeeGroupId == employeeGroup.EmployeeGroupId)).ToList();
                        holidayDict.Add(employeeGroup.EmployeeGroupId, holidaysForEmployeeGroup);
                    }
                }

                holidaysForEmployeeGroup = holidaysForEmployeeGroup.Where(h => h.Date == date).ToList();
            }

            return holidaysForEmployeeGroup;
        }

        private Holiday GetHolidayWithDayTypeDiscardedState(int holidayId)
        {
            return (from h in entities.Holiday
                    .Include("DayType")
                    where h.ActorCompanyId == actorCompanyId &&
                    h.HolidayId == holidayId
                    select h).FirstOrDefault<Holiday>();
        }

        private bool HolidayExists(DateTime date, int dayTypeId)
        {
            return (from h in entities.Holiday
                    where h.DayTypeId == dayTypeId &&
                    h.ActorCompanyId == actorCompanyId &&
                    h.Date == date &&
                    h.State == (int)SoeEntityState.Active
                    select h).Any();
        }

        private bool IsDateHalfDay(List<HolidayDTO> holidaysForDate, DayType dayTypeForDate)
        {
            if (dayTypeForDate == null)
                return false;

            foreach (HolidayDTO holidayForDate in holidaysForDate)
            {
                if (holidayForDate.DayType != null && holidayForDate.DayType.TimeHalfdays != null && holidayForDate.DayTypeId == dayTypeForDate.DayTypeId)
                    return true;
            }

            return false;
        }

        private Holiday CreateHoliday(HolidayDTO holidayInput, DayType dayType)
        {
            if (holidayInput == null)
                return null;

            Holiday holiday = new Holiday()
            {
                SysHolidayId = holidayInput.SysHolidayId,
                Date = holidayInput.Date,
                Name = holidayInput.Name,
                IsRedDay = holidayInput.IsRedDay,
                Description = holidayInput.Description,

                //Set FK
                ActorCompanyId = actorCompanyId,

                //Set references
                DayType = dayType,
            };
            SetCreatedProperties(holiday);
            entities.AddToHoliday(holiday);

            return holiday;
        }

        #endregion

        #region Product

        private Product GetProduct(int productId)
        {
            return (from p in entities.Product
                    where p.ProductId == productId
                    select p).FirstOrDefault<Product>();
        }

        #endregion

        #region Project

        public Project GetProject(int projectId)
        {
            return (from p in entities.Project
                    where p.ProjectId == projectId &&
                    p.State == (int)SoeEntityState.Active
                    select p).FirstOrDefault();
        }

        #endregion

        #region InvoiceProduct

        private InvoiceProduct GetInvoiceProduct(int productId)
        {
            return (from p in entities.Product
                    where p.ProductId == productId &&
                    p.State == (int)SoeEntityState.Active
                    select p).OfType<InvoiceProduct>().FirstOrDefault();
        }

        #endregion

        #region MassRegistrationTemplateHead

        private List<MassRegistrationTemplateHeadDTO> GetMassRegistrationTemplatesForPayrollCalculation()
        {
            List<MassRegistrationTemplateHead> templates = (from x in entities.MassRegistrationTemplateHead
                                                            where x.ActorCompanyId == actorCompanyId && x.State == (int)SoeEntityState.Active
                                                            select x).ToList();

            templates = templates.Where(x => x.IsRecurring && x.RecurringDateTo.HasValue).ToList();
            List<MassRegistrationTemplateHeadDTO> templateDtos = new List<MassRegistrationTemplateHeadDTO>();
            if (templates.Any())
            {
                List<AccountDimDTO> dims = GetAccountDimsFromCache(entities, CacheConfig.Company(actorCompanyId));
                List<int> headIds = templates.Select(x => x.MassRegistrationTemplateHeadId).ToList();
                List<MassRegistrationTemplateRow> rows = (from x in entities.MassRegistrationTemplateRow
                                                              .Include("AccountInternal.Account.AccountDim")
                                                          where headIds.Contains(x.MassRegistrationTemplateHeadId) && x.State == (int)SoeEntityState.Active
                                                          select x).ToList();
                foreach (var template in templates)
                {
                    templateDtos.Add(template.ToDTO(false, rows.Where(x => x.MassRegistrationTemplateHeadId == template.MassRegistrationTemplateHeadId).ToList(), dims));
                }
            }

            return templateDtos;
        }

        #endregion

        #region PayrollProduct

        private List<PayrollProduct> GetPayrollProductsWithSettingsAndAccountInternals(TermGroup_SysPayrollType sysPayrollTypeLevel1, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            return entities.Product.OfType<PayrollProduct>()
                .Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal")
                .Filter(actorCompanyId)
                .FilterLevels(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                .ToList();
        }

        private PayrollProduct GetPayrollProduct(int productId, bool includeInactive = false)
        {
            return entities.Product.OfType<PayrollProduct>()
                .FirstOrDefault(productId, includeInactive);
        }

        private PayrollProduct GetPayrollProduct(string number, bool includeInactive = false)
        {
            return entities.Product.OfType<PayrollProduct>()
                .FirstOrDefault(number, actorCompanyId, includeInactive);
        }

        private PayrollProduct GetPayrollProduct(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return entities.Product.OfType<PayrollProduct>()
                .Filter(actorCompanyId)
                .FilterLevels(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                .FirstOrDefault();
        }

        private PayrollProduct GetPayrollProductWithSettings(int productId, bool includeInactive = false)
        {
            return entities.Product.OfType<PayrollProduct>()
                .Include("PayrollProductSetting")
                .FirstOrDefault(productId, includeInactive);
        }

        private PayrollProduct GetPayrollProductWithSettingsAndAccountInternals(int productId, bool includeInactive = false)
        {
            return entities.Product.OfType<PayrollProduct>()
                .Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal")
                .FirstOrDefault(productId, includeInactive);
        }

        private PayrollProduct GetPayrollProductWithSettingsAndAccountInternalsAndStds(int productId, bool includeInactive = false)
        {
            return entities.Product.OfType<PayrollProduct>()
                .Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal.Account")
                .Include("PayrollProductSetting.PayrollProductAccountStd.AccountStd.Account")
                .FirstOrDefault(productId, includeInactive);
        }

        private PayrollProduct GetPayrollProductWithSettingsAndAccountInternals(TermGroup_SysPayrollType sysPayrollTypeLevel1, TermGroup_SysPayrollType? sysPayrollTypeLevel2 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel3 = null, TermGroup_SysPayrollType? sysPayrollTypeLevel4 = null)
        {
            return entities.Product.OfType<PayrollProduct>()
                .Include("PayrollProductSetting.PayrollProductAccountStd.AccountInternal")
                .Filter(actorCompanyId)
                .FilterLevels(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                .FirstOrDefault();
        }

        private ActionResult CheckMandatoryPayrollProductByLevels(List<Tuple<int?, int?, int?, int?>> mandatoryPayrollProductLevels)
        {
            StringBuilder levelsMissingMsg = new StringBuilder();
            foreach (var tuple in mandatoryPayrollProductLevels)
            {
                PayrollProduct payrollProduct = GetPayrollProductFromCache(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
                if (payrollProduct == null)
                {
                    string level1Term = tuple.Item1.HasValue ? GetText(tuple.Item1.Value, (int)TermGroup.SysPayrollType) : string.Empty;
                    string level2Term = tuple.Item2.HasValue ? GetText(tuple.Item2.Value, (int)TermGroup.SysPayrollType) : string.Empty;
                    string level3Term = tuple.Item3.HasValue ? GetText(tuple.Item3.Value, (int)TermGroup.SysPayrollType) : string.Empty;
                    string level4Term = tuple.Item4.HasValue ? GetText(tuple.Item4.Value, (int)TermGroup.SysPayrollType) : string.Empty;

                    if (levelsMissingMsg.Length > 0)
                        levelsMissingMsg.Append(", ");

                    levelsMissingMsg.Append(level1Term);
                    levelsMissingMsg.Append(!string.IsNullOrEmpty(level2Term) ? "-" + level2Term : string.Empty);
                    levelsMissingMsg.Append(!string.IsNullOrEmpty(level3Term) ? "-" + level3Term : string.Empty);
                    levelsMissingMsg.Append(!string.IsNullOrEmpty(level4Term) ? "-" + level4Term : string.Empty);
                }

            }

            if (levelsMissingMsg.Length > 0)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8717, "Löneart med följande typer saknas:") + "\n" + levelsMissingMsg.ToString());
            else
                return new ActionResult(true);
        }

        #endregion

        #region PayrollProductSetting

        private PayrollProductSetting GetPayrollProductSetting(PayrollProduct payrollProduct, Employee employee, TimeBlockDate timeBlockDate)
        {
            if (payrollProduct == null || payrollProduct.PayrollProductSetting.IsNullOrEmpty() || employee == null || timeBlockDate == null)
                return null;

            return GetPayrollProductSetting(payrollProduct, employee.GetPayrollGroupId(timeBlockDate.Date));
        }

        private PayrollProductSetting GetPayrollProductSetting(PayrollProduct payrollProduct, Employee employee, DateTime date)
        {
            if (payrollProduct == null || payrollProduct.PayrollProductSetting.IsNullOrEmpty() || employee == null)
                return null;

            return GetPayrollProductSetting(payrollProduct, employee.GetPayrollGroupId(date));
        }

        private PayrollProductSetting GetPayrollProductSetting(PayrollProduct payrollProduct, int? payrollGroupId)
        {
            if (payrollProduct == null)
                return null;

            TryLoadPayrollProductSetting(payrollProduct);
            if (payrollProduct.PayrollProductSetting.IsNullOrEmpty())
                return null;

            PayrollProductSetting payrollProductSetting = null;
            if (payrollGroupId.HasValue)
                payrollProductSetting = payrollProduct.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && i.PayrollGroupId.HasValue && i.PayrollGroupId.Value == payrollGroupId.Value);
            if (payrollProductSetting == null)
                payrollProductSetting = payrollProduct.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && !i.PayrollGroupId.HasValue);

            return payrollProductSetting;
        }

        #endregion

        #region PayrollGroup

        private List<PayrollGroup> GetPayrollGroups()
        {
            return (from pg in entities.PayrollGroup
                    where pg.ActorCompanyId == actorCompanyId &&
                    pg.State == (int)SoeEntityState.Active
                    select pg).ToList();
        }

        private List<PayrollGroup> GetPayrollGroupsWithSettings()
        {
            return (from pg in entities.PayrollGroup
                        .Include("PayrollGroupSetting")
                    where pg.ActorCompanyId == actorCompanyId &&
                    pg.State == (int)SoeEntityState.Active
                    select pg).ToList();
        }

        private PayrollGroup GetPayrollGroupWithSettings(int payrollGroupId)
        {
            return (from pg in entities.PayrollGroup
                        .Include("PayrollGroupSetting")
                    where pg.PayrollGroupId == payrollGroupId &&
                    pg.State == (int)SoeEntityState.Active
                    select pg).FirstOrDefault();
        }

        private List<PayrollGroup> GetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProducts()
        {
            return (from pg in entities.PayrollGroup
                    .Include("PayrollGroupPriceType.PayrollPriceType")
                    .Include("PayrollGroupPriceType.PayrollGroupPriceTypePeriod")
                    .Include("PayrollGroupSetting")
                    .Include("PayrollGroupReport")
                    .Include("PayrollGroupAccountStd")
                    .Include("PayrollGroupVacationGroup")
                    .Include("PayrollGroupPayrollProduct")
                    where pg.ActorCompanyId == actorCompanyId &&
                    pg.State == (int)SoeEntityState.Active
                    select pg).ToList();
        }

        private List<PayrollGroupAccountStd> GetPayrollGroupAccountStdsForCompany()
        {
            return (from p in entities.PayrollGroupAccountStd
                    where p.PayrollGroup.ActorCompanyId == actorCompanyId
                    select p).ToList();
        }

        private PayrollGroup GetPayrollGroupWithTimePeriod(int payrollGroupId)
        {
            return (from pg in entities.PayrollGroup
                        .Include("TimePeriodHead.TimePeriod")
                    where pg.PayrollGroupId == payrollGroupId &&
                    pg.State == (int)SoeEntityState.Active
                    select pg).FirstOrDefault();
        }

        #endregion

        #region PayrollPriceType

        public List<PayrollPriceType> GetPayrollPriceTypesWithPeriods()
        {
            List<PayrollPriceType> payrollPriceTypes = (from p in entities.PayrollPriceType.Include("PayrollPriceTypePeriod")
                                                        where p.ActorCompanyId == actorCompanyId &&
                                                        p.State == (int)SoeEntityState.Active
                                                        orderby p.Name
                                                        select p).ToList();

            foreach (PayrollPriceType payrollPriceType in payrollPriceTypes)
            {
                payrollPriceType.TypeName = GetText(payrollPriceType.Type, (int)TermGroup.PayrollPriceTypes);
            }

            return payrollPriceTypes;
        }

        #endregion

        #region PayrollGroupPayrollProduct

        private List<PayrollGroupPayrollProduct> GetPayrollGroupPayrollProducts(int payrollGroupId)
        {
            return (from pgpp in entities.PayrollGroupPayrollProduct
                    where pgpp.PayrollGroupId == payrollGroupId &&
                    pgpp.State == (int)SoeEntityState.Active
                    select pgpp).ToList();
        }

        #endregion

        #region PayrollPeriodChangeHead

        private List<PayrollPeriodChangeHead> GetPayrollPeriodChangeHeads(int employeeTimePeriodId)
        {
            return (from ppc in entities.PayrollPeriodChangeHead
                        .Include("PayrollPeriodChangeRow")
                    where ppc.EmployeeTimePeriodId == employeeTimePeriodId &&
                    ppc.State == (int)SoeEntityState.Active
                    select ppc).ToList();
        }

        private List<PayrollPeriodChangeHead> GetPayrollPeriodChangeHeads(List<int> employeeTimePeriodIds, PayrollPeriodChangeHeadType type)
        {
            return (from ppc in entities.PayrollPeriodChangeHead
                        .Include("PayrollPeriodChangeRow")
                    where employeeTimePeriodIds.Contains(ppc.EmployeeTimePeriodId) &&
                    ppc.Type == (int)type &&
                    ppc.State == (int)SoeEntityState.Active
                    select ppc).ToList();
        }

        #endregion

        #region RecalculateTimeHead

        private RecalculateTimeHead GetRecalculateTimeHead(int recalculateTimeHeadId)
        {
            return (from h in entities.RecalculateTimeHead
                        .Include("RecalculateTimeRecord")
                    where h.ActorCompanyId == actorCompanyId &&
                    h.RecalculateTimeHeadId == recalculateTimeHeadId
                    select h).FirstOrDefault();
        }

        private RecalculateTimeHead CreateRecalculateTimeHeadFromPlacement(Guid key, DateTime? startDate, DateTime? stopDate)
        {
            var head = new RecalculateTimeHead
            {
                Action = (int)SoeRecalculateTimeHeadAction.Placement,
                Status = (int)TermGroup_RecalculateTimeHeadStatus.Started,
                StartDate = startDate,
                StopDate = stopDate,
                ExcecutedStartTime = null, //Will be set when any record are scheduled
                ExcecutedStopTime = null,
                Guid = key,

                //Set FK
                ActorCompanyId = actorCompanyId,
                UserId = userId,
            };
            entities.RecalculateTimeHead.AddObject(head);
            SetCreatedProperties(head);

            return head;
        }

        private RecalculateTimeHead CreateRecalculateTimeHeadFromPlacement(List<SaveEmployeeSchedulePlacementItem> placements, Guid guid)
        {
            RecalculateTimeHead head = guid != Guid.Empty ? entities.RecalculateTimeHead.Include("RecalculateTimeRecord").FirstOrDefault(f => f.Guid == guid && f.ActorCompanyId == actorCompanyId) : null;

            List<RecalculateTimeRecord> records = new List<RecalculateTimeRecord>();

            foreach (SaveEmployeeSchedulePlacementItem placement in placements)
            {
                if (head?.RecalculateTimeRecord != null && head.RecalculateTimeRecord.Any(a => a.EmployeeId == placement.EmployeeId && a.StartDate == (placement.StartDate ?? placement.EmployeeScheduleStopDate)))
                    continue;

                if (head?.RecalculateTimeRecord == null || !head.RecalculateTimeRecord.Any(a => a.PlacementId == placement.UniqueId))
                {
                    placement.UniqueId = Guid.NewGuid();

                    records.Add(new RecalculateTimeRecord
                    {
                        Status = (int)TermGroup_RecalculateTimeRecordStatus.Waiting,
                        StartDate = placement.StartDate ?? placement.EmployeeScheduleStopDate,
                        StopDate = placement.StopDate,
                        PlacementId = placement.UniqueId,

                        //Set FK
                        EmployeeId = placement.EmployeeId,
                    });
                }
            }

            if (records.Any())
            {
                if (head == null)
                    head = CreateRecalculateTimeHeadFromPlacement(guid, records.Min(i => i.StartDate), records.Max(i => i.StopDate));
                else
                    SetModifiedProperties(head);

                foreach (RecalculateTimeRecord record in records)
                {
                    if (!head.RecalculateTimeRecord.Any(a => a.PlacementId == record.PlacementId))
                        head.RecalculateTimeRecord.Add(record);
                }

                if (!Save().Success)
                    return null;
            }

            return head;
        }

        #endregion

        #region RecalculateTimeRecord

        private RecalculateTimeRecord GetRecalculateTimeRecord(int recalculateTimeRecordId)
        {
            return (from h in entities.RecalculateTimeRecord
                    where h.RecalculateTimeRecordId == recalculateTimeRecordId
                    select h).FirstOrDefault();
        }

        private RecalculateTimeRecord GetRecalculateTimeRecordWithBlocks(int recalculateTimeRecordId)
        {
            return (from h in entities.RecalculateTimeRecord
                        .Include("TimeScheduleTemplateBlock")
                    where h.RecalculateTimeRecordId == recalculateTimeRecordId
                    select h).FirstOrDefault();
        }

        private List<RecalculateTimeRecord> GetRecalculateTimeRecords(List<int> employeeIds, params TermGroup_RecalculateTimeRecordStatus[] statuses)
        {
            if (employeeIds.IsNullOrEmpty())
                return new List<RecalculateTimeRecord>();

            List<int> statusIds = new List<int>();
            foreach (TermGroup_RecalculateTimeRecordStatus status in statuses)
            {
                int statusId = (int)status;
                if (!statusIds.Contains(statusId))
                    statusIds.Add(statusId);
            }

            return (from r in entities.RecalculateTimeRecord
                    where r.RecalculateTimeHead.ActorCompanyId == actorCompanyId &&
                    employeeIds.Contains(r.EmployeeId) &&
                    statusIds.Contains(r.Status)
                    select r).ToList();
        }

        #endregion

        #region SchoolHoliday

        private List<SchoolHoliday> GetSchoolHolidaysInInterval(DateTime dateFrom, DateTime dateTo, bool? isSummerHoliday)
        {
            if (isSummerHoliday == true && dateFrom.Year != dateTo.Year)
                return new List<SchoolHoliday>();

            List<SchoolHoliday> allSchoolHolidays = (from sh in entities.SchoolHoliday
                                                     where sh.ActorCompanyId == actorCompanyId &&
                                                     sh.State == (int)SoeEntityState.Active
                                                     select sh).ToList();

            if (isSummerHoliday.HasValue)
                allSchoolHolidays = allSchoolHolidays.Where(i => i.IsSummerHoliday == isSummerHoliday.Value).ToList();

            List<SchoolHoliday> schoolHolidays = new List<SchoolHoliday>();
            foreach (SchoolHoliday schoolHoliday in allSchoolHolidays)
            {
                if (CalendarUtility.IsDatesOverlapping(dateFrom, dateTo, schoolHoliday.DateFrom, schoolHoliday.DateTo, validateDatesAreTouching: true))
                    schoolHolidays.Add(schoolHoliday);
            }

            return schoolHolidays;
        }

        private bool IsSchoolWeek(DateTime date)
        {
            DateTime monday = CalendarUtility.GetFirstDateOfWeek(date, offset: DayOfWeek.Monday).Date;
            DateTime friday = monday.AddDays(4);

            bool isSchoolWeek = true;
            List<SchoolHoliday> schoolHolidays = GetSchoolHolidaysInInterval(monday, friday, null);
            foreach (SchoolHoliday schoolHoliday in schoolHolidays)
            {
                if (CalendarUtility.IsCurrentOverlappedByNew(schoolHoliday.DateFrom.Date, schoolHoliday.DateTo.Date, monday, friday))
                    isSchoolWeek = false;
            }
            return isSchoolWeek;
        }

        private bool IsSchoolDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        #endregion

        #region ShiftType

        private List<int> GetShiftTypeIdsHandlingMoney()
        {
            List<ShiftType> shiftTypes = (from s in entities.ShiftType
                                          where s.ActorCompanyId == actorCompanyId &&
                                          s.State == (int)SoeEntityState.Active
                                          select s).ToList();

            return shiftTypes.Where(x => x.HandlingMoney).Select(x => x.ShiftTypeId).ToList();
        }

        private List<ShiftType> GetShiftTypes(List<int> shiftTypeIds)
        {
            List<ShiftType> shiftTypes = new List<ShiftType>();

            foreach (int shiftTypeId in shiftTypeIds.Distinct())
            {
                ShiftType shiftType = GetShiftType(shiftTypeId);
                if (shiftType != null)
                    shiftTypes.Add(shiftType);
            }

            return shiftTypes;
        }

        private ShiftType GetShiftType(int shiftTypeId)
        {
            return (from st in entities.ShiftType
                    where st.ShiftTypeId == shiftTypeId &&
                    st.State == (int)SoeEntityState.Active
                    select st).FirstOrDefault();
        }

        private ShiftType GetShiftTypeWithAccounts(int shiftTypeId)
        {
            return (from st in entities.ShiftType
                        .Include("AccountInternal.Account")
                    where st.ShiftTypeId == shiftTypeId &&
                    st.State == (int)SoeEntityState.Active
                    select st).FirstOrDefault();
        }

        public ShiftType GetShiftTypeByAccountWithAccounts(int accountId)
        {
            return (from st in entities.ShiftType
                        .Include("AccountInternal.Account")
                    where st.AccountId == accountId &&
                    st.ActorCompanyId == actorCompanyId &&
                    st.State == (int)SoeEntityState.Active
                    select st).FirstOrDefault();
        }

        #endregion

        #region Skills

        private List<EmployeeSkill> GetEmployeeSkills(List<int> employeeIds)
        {
            return (from e in entities.EmployeeSkill
                    where employeeIds.Contains(e.EmployeeId) &&
                    e.Skill.State == (int)SoeEntityState.Active
                    select e).ToList();
        }

        private List<ShiftTypeSkill> GetShiftTypeSkills(int shiftTypeId)
        {
            return (from e in entities.ShiftTypeSkill
                    where e.ShiftTypeId == shiftTypeId &&
                    e.Skill.State == (int)SoeEntityState.Active
                    select e).ToList();
        }

        #endregion

        #region SysTerm

        private string GetSysTerm(int sysTermId, int sysTermGroupId, string defaultTerm)
        {
            return GetText(sysTermId, sysTermGroupId, defaultTerm);
        }

        #endregion

        #region TimeAbsenceRule

        private List<TimeAbsenceRuleHead> GetTimeAbsenceRuleHeadsWithRows()
        {
            return (from t in entities.TimeAbsenceRuleHead
                        .Include("TimeAbsenceRuleHeadEmployeeGroup")
                        .Include("TimeAbsenceRuleRow")
                    where t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        #endregion

        #region TimeAccumulator

        private List<TimeAccumulator> GetTimeAccumulatorsForFinalSalary()
        {
            return TimeAccumulatorManager.GetTimeAccumulators(entities, base.ActorCompanyId, loadEmployeeGroupRule: true, onlyFinalSalary: true);
        }
        private List<TimeAccumulator> GetTimeAccumulatorsForTimeWorkAccount()
        {
            return TimeAccumulatorManager
                .GetTimeAccumulators(entities, base.ActorCompanyId, loadEmployeeGroupRule: true)
                .Where(i => i.UseTimeWorkAccount && i.TimeCodeId.HasValue)
                .ToList();
        }
        private TimeAccumulator GetTimeAccumulatorWithTimeCode(int timeAccumulatorId)
        {
            return entities.TimeAccumulator
                .Include(ta => ta.TimeCode)
                .FirstOrDefault(ta => ta.TimeAccumulatorId == timeAccumulatorId);
        }

        private TimeAccumulator GetTimeAccumulatorWithEmployeeGroupRules(int timeAccumulatorId)
        {
            return entities.TimeAccumulator
                .Include(ta => ta.TimeAccumulatorEmployeeGroupRule)
                .FirstOrDefault(ta => ta.TimeAccumulatorId == timeAccumulatorId);
        }

        private List<TimeAccumulatorItem> GetTimeAccumulatorItemsForFinalSalary(List<TimeAccumulator> timeAccumulators, Employee employee, TimePeriod timePeriod, DateTime date)
        {
            if (employee == null || timeAccumulators.IsNullOrEmpty())
                return new List<TimeAccumulatorItem>();

            return TimeAccumulatorManager.GetTimeAccumulatorItems(
                entities, 
                GetTimeAccumulatorItemsInput.CreateInput(this.ActorCompanyId, this.UserId, employee.EmployeeId, CalendarUtility.GetBeginningOfYear(date), date, calculateAccToday: true), 
                timeAccumulators, 
                employee, 
                GetEmployeeGroupsFromCache(), 
                timePeriod
                );
        }
        private TimeAccumulatorItem GetTimeAccumulatorItemForTimeWorkAccountUnusedPaidBalance(List<TimeAccumulator> timeAccumulators, Employee employee, TimePeriod timePeriod, TimeWorkAccountYear timeWorkAccountYear)
        {
            if (employee == null || timeWorkAccountYear == null)
                return new TimeAccumulatorItem();

            return TimeAccumulatorManager.GetTimeAccumulatorItems(
                entities,
                GetTimeAccumulatorItemsInput.CreateInput(this.ActorCompanyId, this.UserId, employee.EmployeeId, timeWorkAccountYear.WithdrawalStart, timeWorkAccountYear.WithdrawalStop, calculateAccToday: true),
                timeAccumulators,
                employee, 
                GetEmployeeGroupsFromCache(), 
                timePeriod,
                useLastEmploymentIfNotExists: true).FirstOrDefault();
        }
        private TimeAccumulatorItem GetTimeAccumulatorItemForTimeWorkReductionBasis(TimeAccumulator timeAccumulator, Employee employee, DateTime dateFrom, DateTime dateTo)
        {
            if (employee == null || timeAccumulator == null)
                return null;

            return TimeAccumulatorManager.GetTimeAccumulatorItem(
                entities,
                GetTimeAccumulatorItemsInput.CreateInput(this.ActorCompanyId, this.UserId, employee.EmployeeId, dateFrom, dateTo, calculatePeriod: true),
                timeAccumulator,
                employee,
                timeAccumulatorBalances: GetTimeAccumulatorBalancesFromCache(employee.EmployeeId),
                clockRounding: GetCompanyIntSettingFromCache(CompanySettingType.TimeSchedulePlanningClockRounding)
                );
        }
        private int GetAccumulatedTimeWorkReductionBasisMinutes(TimeAccumulator timeAccumulator, Employee employee, DateTime dateFrom, DateTime dateTo)
        {
            return (int)Math.Round(GetTimeAccumulatorItemForTimeWorkReductionBasis(timeAccumulator, employee, dateFrom, dateTo)?.SumPeriod ?? 0, MidpointRounding.AwayFromZero);
        }
        private TimeAccumulatorItem GetTimeAccumulatorItemForTimeWorkReductionEarning(TimeAccumulator timeAccumulator, Employee employee, DateTime date)
        {
            if (employee == null || timeAccumulator == null)
                return null;

            return TimeAccumulatorManager.GetTimeAccumulatorItem(
                entities,
                GetTimeAccumulatorItemsInput.CreateInput(this.ActorCompanyId, this.UserId, employee.EmployeeId, date, date, calculateAccToday: true),
                timeAccumulator,
                employee,
                timeAccumulatorBalances: GetTimeAccumulatorBalancesFromCache(employee.EmployeeId),
                clockRounding: GetCompanyIntSettingFromCache(CompanySettingType.TimeSchedulePlanningClockRounding)
                );
        }
        private int GetAccumulatedTimeWorkReductionEarningMinutes(TimeAccumulator timeAccumulator, Employee employee, DateTime date)
        {
            return (int)Math.Round(GetTimeAccumulatorItemForTimeWorkReductionEarning(timeAccumulator, employee, date)?.SumAccToday ?? 0, MidpointRounding.AwayFromZero);
        }

        #endregion

        #region TimeAccumulatorBalance

        public List<TimeAccumulatorBalance> GetTimeAccumulatorBalances(int employeeId)
        {
            return (from tab in entities.TimeAccumulatorBalance
                    where tab.ActorCompanyId == actorCompanyId &&
                    tab.EmployeeId == employeeId
                    select tab).ToList();
        }

        #endregion

        #region TimeBlock

        private List<TimeBlock> GetTimeBlocks(int employeeId, int timeBlockDateId, int? templatePeriodId)
        {
            return (from tb in entities.TimeBlock
                    where tb.EmployeeId == employeeId &&
                    tb.TimeScheduleTemplatePeriodId == templatePeriodId &&
                    tb.TimeBlockDateId == timeBlockDateId &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksInOvertimePeriod(int employeeId, EmployeeGroup employeeGroup, DateTime currentDate, List<TimeBlock> presenceTimeBlocks, List<int> timeDeviationCauseIdsOvertime)
        {
            var (periodStart, periodStop) = GetDatesForOvertimePeriod(currentDate, false);
            var timeBlocks = GetTimeBlocksWithTimeBlockDate(employeeId, periodStart, periodStop);

            if (employeeGroup.AutogenTimeblocks)
            {
                if (!presenceTimeBlocks.IsNullOrEmpty())
                {
                    timeBlocks = timeBlocks.Where(t => t.TimeBlockDate.Date != currentDate).ToList();
                    timeBlocks.AddRange(presenceTimeBlocks);
                }
            }
            else if (presenceTimeBlocks != null)
            {
                var newTimeBlocks = presenceTimeBlocks.Where(t => t.TimeBlockId == 0 && t.State == (int)SoeEntityState.Active).ToList();
                foreach (var newTimeBlock in newTimeBlocks)
                {
                    if (!timeBlocks.Any(tb => tb.TimeBlockDateId == newTimeBlock.TimeBlockDateId && CalendarUtility.IsDatesOverlapping(tb.StartTime, tb.StopTime, newTimeBlock.StartTime, newTimeBlock.StopTime)))
                        timeBlocks.Add(newTimeBlock);
                }
            }

            return timeBlocks.FilterOvertime(timeDeviationCauseIdsOvertime);
        }

        private List<TimeBlock> GetTimeBlocksWithTimeBlockDate(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            DateTime latestDate = GetLatestDateForTimeBlock(employeeId);
            if (latestDate < dateFrom)
                return new List<TimeBlock>();

            return (from tb in entities.TimeBlock
                        .Include("TimeBlockDate")
                    where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                    tb.TimeBlockDate.Date >= dateFrom &&
                    tb.TimeBlockDate.Date <= dateTo &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTimeCodeAndAccountInternal(int employeeId, int timeBlockDateId, int? templatePeriodId)
        {
            return (from tb in entities.TimeBlock
                        .Include("TimeCode")
                        .Include("AccountInternal")
                    where tb.EmployeeId == employeeId &&
                    tb.TimeScheduleTemplatePeriodId == templatePeriodId &&
                    tb.TimeBlockDateId == timeBlockDateId &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTimeCodeAndTransactions(int employeeId, int timeBlockDateId)
        {
            return (from tb in entities.TimeBlock
                        .Include("TimeCode")
                        .Include("TimeInvoiceTransaction")
                        .Include("TimePayrollTransaction")
                        .Include("TimeCodeTransaction")
                    where tb.EmployeeId == employeeId &&
                    tb.TimeBlockDateId == timeBlockDateId &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTimeCodeAndTransactions(int employeeId, List<int> timeBlockDateIds)
        {
            return (from tb in entities.TimeBlock
                        .Include("TimeCode")
                        .Include("TimeCodeTransaction")
                        .Include("TimeInvoiceTransaction")
                        .Include("TimePayrollTransaction")
                    where tb.EmployeeId == employeeId &&
                    timeBlockDateIds.Contains(tb.TimeBlockDateId) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTransactions(int employeeId, List<int> timeBlockDateIds, bool onlyActive = true)
        {
            var query = from tb in entities.TimeBlock
                            .Include("TimeCodeTransaction")
                            .Include("TimeInvoiceTransaction")
                            .Include("TimePayrollTransaction")
                        where tb.EmployeeId == employeeId &&
                        tb.Employee.ActorCompanyId == actorCompanyId &&
                        timeBlockDateIds.Contains(tb.TimeBlockDateId)
                        select tb;

            if (onlyActive)
                query = query.Where(i => i.State == (int)SoeEntityState.Active);

            return query.ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTransactionsAndPayrollTransactionAccount(int employeeId, List<int> timeBlockDateIds, bool onlyActive = true)
        {
            var query = from tb in entities.TimeBlock
                            .Include("TimeCodeTransaction")
                            .Include("TimeInvoiceTransaction")
                            .Include("TimePayrollTransaction.AccountInternal")
                        where tb.EmployeeId == employeeId &&
                        tb.Employee.ActorCompanyId == actorCompanyId &&
                        timeBlockDateIds.Contains(tb.TimeBlockDateId)
                        select tb;

            if (onlyActive)
                query = query.Where(i => i.State == (int)SoeEntityState.Active);

            return query.ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTimePayrollTransactionsAndAccounting(int employeeId, List<int> timeBlockDateIds)
        {
            return (from tb in entities.TimeBlock
                        .Include("AccountInternal")
                        .Include("TimePayrollTransaction.AccountInternal")
                    where tb.EmployeeId == employeeId &&
                    timeBlockDateIds.Contains(tb.TimeBlockDateId) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTransactions(int employeeId, DateTime dateFrom, DateTime? dateTo)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            if (dateTo.HasValue)
                dateTo = CalendarUtility.GetEndOfDay(dateTo.Value);

            DateTime latestDate = GetLatestDateForTimeBlock(employeeId);
            if (latestDate < dateFrom)
                return new List<TimeBlock>();

            return (from tb in entities.TimeBlock
                        .Include("TimeInvoiceTransaction")
                        .Include("TimePayrollTransaction")
                        .Include("TimeCodeTransaction")
                    where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                    tb.TimeBlockDate.Date >= dateFrom &&
                    (!dateTo.HasValue || tb.TimeBlockDate.Date <= dateTo) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithTransactions(int employeeId, int timeBlockDateId, bool onlyActive = true)
        {
            var query = from tb in entities.TimeBlock
                           .Include("TimeInvoiceTransaction")
                           .Include("TimePayrollTransaction")
                           .Include("TimeCodeTransaction")
                        where tb.EmployeeId == employeeId &&
                        tb.TimeBlockDateId == timeBlockDateId
                        select tb;

            if (onlyActive)
                query = query.Where(i => i.State == (int)SoeEntityState.Active);

            return query.ToList();
        }

        private List<TimeBlock> GetTimeBlocksForWorkRuleEvaluation(int employeeId, DateTime startTime, DateTime stopTime, EmployeeGroup employeeGroup, List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            // Make sure the whole day is covered
            DateTime dateFrom = CalendarUtility.GetBeginningOfDay(startTime);
            DateTime dateTo = CalendarUtility.GetEndOfDay(stopTime);

            List<TimeBlock> timeBlocks = (from tb in entities.TimeBlock
                                            .Include("TimeBlockDate")
                                            .Include("TimeCode")
                                            .Include("TimeDeviationCauseStart")
                                          where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                                          tb.TimeBlockDate.Date >= dateFrom &&
                                          tb.TimeBlockDate.Date <= dateTo &&
                                          tb.State == (int)SoeEntityState.Active
                                          select tb).ToList();


            DecideTimeBlockTypes(timeBlocks);
            foreach (var items in timeBlocks.GroupBy(tb => tb.TimeBlockDate.Date))
            {
                DecideTimeBlockStandby(items.Where(x => x.CalculatedAsPresence == true).ToList(), scheduleBlocks.Where(x => x.Date.HasValue && x.Date == items.Key).ToList(), employeeGroup);
                DecideExcludeFromPresenceWorkRules(items.Where(x => x.CalculatedAsPresence == true).ToList());
                items.Where(x => x.CalculatedAsAbsence == true).ToList().ForEach(i => i.CalculatedAsStandby = false);
            }

            return timeBlocks.Where(x => x.CalculatedAsPresence == true && x.CalculatedAsExcludeFromPresenceWorkRules == false && !(x.CalculatedAsStandby ?? false)).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithDateAndTimePayrollTransaction(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from tb in entities.TimeBlock
                        .Include("TimeBlockDate")
                        .Include("TimePayrollTransaction")
                    where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                    tb.TimeBlockDate.Date >= dateFrom &&
                    tb.TimeBlockDate.Date <= dateTo &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithDateAndTimePayrollTransaction(int employeeId, List<int> timeBlockDateIds)
        {
            return (from tb in entities.TimeBlock
                        .Include("TimeBlockDate")
                        .Include("TimePayrollTransaction")
                    where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                    timeBlockDateIds.Contains(tb.TimeBlockDateId) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithDateAndTimePayrollTransactionAndAccountInternals(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from tb in entities.TimeBlock
                        .Include("TimeBlockDate")
                        .Include("TimePayrollTransaction.AccountInternal")
                    where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                    tb.TimeBlockDate.Date >= dateFrom &&
                    tb.TimeBlockDate.Date <= dateTo &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> GetTimeBlocksWithDateAndTimePayrollTransactionAndAccountInternals(int employeeId, List<int> timeBlockDateIds)
        {
            return (from tb in entities.TimeBlock
                        .Include("TimeBlockDate")
                        .Include("TimePayrollTransaction.AccountInternal")
                    where tb.EmployeeId == employeeId && tb.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used
                    timeBlockDateIds.Contains(tb.TimeBlockDateId) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeBlock> FilterTimeBlocksWithUnsavedLink(TimeEngineTemplate template)
        {
            return template?.Identity?.TimeBlocks.Where(i => i.PayrollImportEmployeeTransactionId.HasValue && i.TimeBlockId == 0 && i.State == (int)SoeEntityState.Active).ToList() ?? new List<TimeBlock>();
        }

        private List<TimeBlock> DivideTimeBlockAgainstTemplateBlockBreaks(ref TimeBlock timeBlockToDivide, List<TimeScheduleTemplateBlock> templateBlockBreaks, List<TimeBlock> newTimeBlocks)
        {
            if (timeBlockToDivide.StartTime == timeBlockToDivide.StopTime)
                return newTimeBlocks;

            foreach (TimeScheduleTemplateBlock templateBlockBreak in templateBlockBreaks.OrderBy(x => x.StartTime).ToList())
            {
                if (templateBlockBreak.StartTime >= timeBlockToDivide.StartTime && templateBlockBreak.StopTime <= timeBlockToDivide.StopTime)
                {
                    TimeBlock timeBlockBeforeBreak = new TimeBlock
                    {
                        StartTime = timeBlockToDivide.StartTime,
                        StopTime = templateBlockBreak.StartTime,
                        IsBreak = timeBlockToDivide.IsBreak(),
                        EmployeeChildId = timeBlockToDivide.EmployeeChildId,
                        ShiftTypeId = timeBlockToDivide.ShiftTypeId,
                        TimeScheduleTypeId = timeBlockToDivide.TimeScheduleTypeId,
                    };
                    SetCreatedProperties(timeBlockBeforeBreak);
                    newTimeBlocks.Add(timeBlockBeforeBreak);

                    timeBlockToDivide.StartTime = templateBlockBreak.StopTime;
                    DivideTimeBlockAgainstTemplateBlockBreaks(ref timeBlockToDivide, templateBlockBreaks, newTimeBlocks);
                }
            }
            return newTimeBlocks;
        }

        private List<TimeBlock> GetTimeBlockBreakForScheduleBreak(List<TimeBlock> timeBlockBreaks, int templateBlockBreakId)
        {
            if (timeBlockBreaks.IsNullOrEmpty() || templateBlockBreakId == 0)
                return new List<TimeBlock>();

            return (from tb in timeBlockBreaks
                    where tb.TimeScheduleTemplateBlockBreakId.HasValue &&
                    tb.TimeScheduleTemplateBlockBreakId.Value == templateBlockBreakId
                    orderby tb.StartTime, tb.StopTime
                    select tb).ToList();
        }

        private List<TimeBlock> CopyTimeBlocks(List<TimeBlock> prototypeTimeBlocks, List<TimeScheduleTemplateBlock> templateBlocks, TimeBlockDate timeBlockDate)
        {
            List<TimeBlock> timeBlocks = new List<TimeBlock>();

            if (prototypeTimeBlocks != null)
            {
                foreach (TimeBlock prototypeTimeBlock in prototypeTimeBlocks.OrderBy(i => i.StartTime))
                {
                    TimeBlock timeBlock = CopyTimeBlock(prototypeTimeBlock, templateBlocks, timeBlockDate);
                    if (timeBlock != null)
                        timeBlocks.Add(timeBlock);
                }
            }

            return timeBlocks;
        }

        private List<TimeBlock> SplitTimeBlocks(TimeBlock breakTimeBlock, List<TimeBlock> nonBreakTimeBlocks)
        {
            List<TimeBlock> timeBlocks = new List<TimeBlock>();

            if (!nonBreakTimeBlocks.IsNullOrEmpty())
            {
                if (breakTimeBlock != null)
                {
                    //Has non-break TimeBlocks and break TimeBlock
                    //Will not shrink due to the zero-break (ex: 08.00-11.30,11.30-11.30(break),12.15-16.00 when break originally was 11.30-12.15)
                    timeBlocks.AddRange(RearrangeNewTimeBlockAgainstExisting(breakTimeBlock, nonBreakTimeBlocks, null, false, true));
                    timeBlocks.Add(breakTimeBlock);
                }
                else
                {
                    //Has only non-break TimeBlocks
                    timeBlocks.AddRange(nonBreakTimeBlocks);
                }
            }
            else
            {
                if (breakTimeBlock != null)
                {
                    //Has only break TimeBlock
                    timeBlocks.Add(breakTimeBlock);
                }
            }

            return timeBlocks;
        }

        private TimeBlock GetTimeBlockWithAccountsDiscardedState(int timeBlockId)
        {
            return (from tb in entities.TimeBlock
                        .Include("AccountStd.Account")
                        .Include("AccountInternal.Account")
                    where tb.TimeBlockId == timeBlockId
                    select tb).FirstOrDefault();
        }

        private TimeBlock CopyTimeBlock(TimeBlock prototypeTimeBlock, List<TimeScheduleTemplateBlock> templateBlocks, TimeBlockDate timeBlockDate)
        {
            if (templateBlocks == null || timeBlockDate == null)
                return null;

            TimeBlock cloneTimeBlock = new TimeBlock()
            {
                StartTime = prototypeTimeBlock.StartTime,
                StopTime = prototypeTimeBlock.StopTime,
                IsBreak = prototypeTimeBlock.IsBreak,
                ManuallyAdjusted = prototypeTimeBlock.ManuallyAdjusted,
                IsPreliminary = prototypeTimeBlock.IsPreliminary,

                //Set FK (from template)
                TimeDeviationCauseStartId = prototypeTimeBlock.TimeDeviationCauseStartId,
                TimeDeviationCauseStopId = prototypeTimeBlock.TimeDeviationCauseStopId,
                TimeScheduleTemplatePeriodId = prototypeTimeBlock.TimeScheduleTemplatePeriodId,
                EmployeeChildId = prototypeTimeBlock.EmployeeChildId,
                ShiftTypeId = prototypeTimeBlock.ShiftTypeId,
                TimeScheduleTypeId = prototypeTimeBlock.TimeScheduleTypeId,

                //Set FK (from identity)
                EmployeeId = timeBlockDate.EmployeeId,
                AccountStdId = prototypeTimeBlock.AccountStdId.ToNullable(),

                //Set references (from identity)
                TimeBlockDate = timeBlockDate,
            };
            SetCreatedProperties(cloneTimeBlock);
            entities.TimeBlock.AddObject(cloneTimeBlock);

            if (prototypeTimeBlock.IsBreak)
            {
                //Cannot copy TimeScheduleTemplateBlockBreakId. Must find the corresponding TimeScheduleTemplateBlock break using the passed templateBlocks
                var templateBlockBreak = templateBlocks.GetBreaks().GetFirstFromTime(prototypeTimeBlock.StartTime);
                if (templateBlockBreak != null)
                    cloneTimeBlock.TimeScheduleTemplateBlockBreakId = templateBlockBreak.TimeScheduleTemplateBlockId;
            }

            foreach (TimeCode timeCode in prototypeTimeBlock.TimeCode)
            {
                cloneTimeBlock.TimeCode.Add(timeCode);
            }

            AddAccountInternalsToTimeBlock(cloneTimeBlock, prototypeTimeBlock.AccountInternal);

            return cloneTimeBlock;
        }

        private bool DoCreateExcessTimeBlock(SoeTimeRuleType timeRuleType, DateTime startTime, DateTime stopTime, List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (startTime >= stopTime)
                return false;

            //If trying to add absence block on time when schedule have scheduletype "IsNotScheduleTime" the result shoud be no timeblock at all
            if (timeRuleType == SoeTimeRuleType.Absence && !scheduleBlocks.IsNullOrEmpty() && scheduleBlocks.Any(i => i.TimeScheduleTypeId.HasValue))
            {
                List<TimeScheduleTemplateBlock> scheduleBlocksWithScheduleTypes = scheduleBlocks.Where(b => b.TimeScheduleTypeId.HasValue && b.StartTime < b.StopTime && CalendarUtility.IsNewOverlappedByCurrent(startTime, stopTime, b.StartTime, b.StopTime)).ToList();
                if (!scheduleBlocksWithScheduleTypes.IsNullOrEmpty())
                {
                    List<TimeScheduleType> timeScheduleTypes = GetTimeScheduleTypesWithFactorFromCache();
                    if (!timeScheduleTypes.IsNullOrEmpty())
                    {
                        TimeScheduleTemplateBlock scheduleBlock;

                        //IsNotScheduleTime
                        List<int> timeScheduleTypeIdsIsNotScheduleTime = timeScheduleTypes.GetTimeScheduleTypeIdsIsNotScheduleTime();
                        scheduleBlock = scheduleBlocksWithScheduleTypes.FirstOrDefault(b => timeScheduleTypeIdsIsNotScheduleTime.Contains(b.TimeScheduleTypeId.Value));
                        if (scheduleBlock != null)
                            return false;

                        //Factor zero
                        List<int> timeScheduleTypeIdsIsFactorZero = timeScheduleTypes.GetTimeScheduleTypeIdsFactorZero(startTime, stopTime);
                        scheduleBlock = scheduleBlocksWithScheduleTypes.FirstOrDefault(b => timeScheduleTypeIdsIsFactorZero.Contains(b.TimeScheduleTypeId.Value));
                        if (scheduleBlock != null)
                            return false;
                    }
                }
            }

            return true;
        }

        private TimeBlock CreateExcessTimeBlock(SoeTimeRuleType timeRuleType, DateTime startTime, DateTime stopTime, TimeBlockDate timeBlockDate, EmployeeGroup employeeGroup, int? templatePeriodId, List<TimeScheduleTemplateBlock> scheduleBlocks = null, TimeDeviationCause timeDeviationCause = null, int? timeDeviationCauseIdOld = null, int? shiftTypeId = null, int? timeScheduleTypeId = null, int? employeeChildId = null, string comment = null)
        {
            if (timeBlockDate == null)
                return null;

            return CreateExcessTimeBlock(timeRuleType, startTime, stopTime, timeBlockDate.Date, timeBlockDate.TimeBlockDateId, timeBlockDate.EmployeeId, employeeGroup, templatePeriodId, scheduleBlocks, timeDeviationCause, timeDeviationCauseIdOld, shiftTypeId, timeScheduleTypeId, employeeChildId, comment);
        }

        private TimeBlock CreateExcessTimeBlock(SoeTimeRuleType timeRuleType, DateTime startTime, DateTime stopTime, DateTime date, int timeBlockDateId, int employeeId, EmployeeGroup employeeGroup, int? templatePeriodId, List<TimeScheduleTemplateBlock> scheduleBlocks = null, TimeDeviationCause timeDeviationCause = null, int? timeDeviationCauseIdOld = null, int? shiftTypeId = null, int? timeScheduleTypeId = null, int? employeeChildId = null, string comment = null)
        {
            if (!DoCreateExcessTimeBlock(timeRuleType, startTime, stopTime, scheduleBlocks))
                return null;

            //Must have TimeCodeIds when specifiyng TimeDeviationCause, otherwise dont create excess TimeBlock
            List<int> timeCodeIdsFromRules = new List<int>();
            if (timeDeviationCause == null && (employeeGroup != null && employeeGroup.TimeDeviationCauseId.HasValue))
                timeDeviationCause = GetTimeDeviationCauseFromCache(employeeGroup.TimeDeviationCauseId.Value);
            if (timeDeviationCause != null)
            {
                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
                timeCodeIdsFromRules.AddRange(GetTimeCodesGeneratedByTimeRules(date.Date, startTime, stopTime, timeRuleType, timeDeviationCause.TimeDeviationCauseId, employee, employeeGroup, scheduleBlocks: scheduleBlocks));
                if (!timeCodeIdsFromRules.Any())
                {
                    if (!timeDeviationCause.TimeCodeReference.IsLoaded)
                        timeDeviationCause.TimeCodeReference.Load();
                    if (timeDeviationCause.TimeCode == null && (timeDeviationCauseIdOld.HasValue && timeDeviationCauseIdOld.Value == timeDeviationCause.TimeDeviationCauseId))
                        return null;
                }
            }

            //Only set EmployeeChild when used that TimeDeviationCause
            if (timeDeviationCause == null || !timeDeviationCause.SpecifyChild)
                employeeChildId = null;

            TimeBlock excessTimeBlock = new TimeBlock()
            {
                GuidId = Guid.NewGuid(),
                StartTime = startTime,
                StopTime = stopTime,
                IsBreak = false,
                IsPreliminary = false,
                Comment = !String.IsNullOrEmpty(comment) ? comment : null,
                CalculatedTimeRuleType = timeRuleType,

                //Set FK
                EmployeeId = employeeId,
                TimeBlockDateId = timeBlockDateId,
                TimeScheduleTemplatePeriodId = templatePeriodId,
                TimeDeviationCauseStartId = timeDeviationCause?.TimeDeviationCauseId,
                TimeDeviationCauseStopId = timeDeviationCause?.TimeDeviationCauseId,
                EmployeeChildId = employeeChildId,
                ShiftTypeId = shiftTypeId,
                TimeScheduleTypeId = timeScheduleTypeId
            };
            SetCreatedProperties(excessTimeBlock);
            entities.TimeBlock.AddObject(excessTimeBlock);

            //If no TimeCode is generated from rules, add TimeCode from TimeDeviationCause
            if (timeDeviationCause != null)
            {
                AddTimeCodesGeneratedByBlock(excessTimeBlock, timeCodeIdsFromRules);
                if (!excessTimeBlock.TimeCode.Any() && timeDeviationCause.TimeCode != null)
                    excessTimeBlock.TimeCode.Add(timeDeviationCause.TimeCode);
            }

            return excessTimeBlock;
        }

        private TimeBlock CreateExcessTimeBlockFromStamping(SoeTimeRuleType timeRuleType, DateTime startTime, DateTime stopTime, TimeBlockDate timeBlockDate, TimeStampEntry stampIn, TimeStampEntry stampOut, Employee employee, EmployeeGroup employeeGroup, TimeScheduleTemplatePeriod templatePeriod, List<TimeScheduleTemplateBlock> scheduleBlocks, TimeDeviationCause timeDeviationCause, int? employeeChildId, string debugInfo, TimeCode defaultTimeCode = null, bool connectToStamps = false, bool addAccountingFromStart = false, bool addAccountingFromStop = false, bool addAccountingFromClosest = false)
        {
            if (employee == null || timeBlockDate == null || stampIn == null)
                return null;

            var alignedScheduleBlock = scheduleBlocks?.FirstOrDefault(w => w.StartTime == startTime || w.StopTime == stopTime);

            TimeBlock excessTimeBlock = CreateExcessTimeBlock(
                timeRuleType,
                startTime,
                stopTime,
                timeBlockDate.Date,
                timeBlockDate.TimeBlockDateId,
                employee.EmployeeId,
                employeeGroup,
                templatePeriod?.TimeScheduleTemplatePeriodId,
                scheduleBlocks,
                timeDeviationCause,
                null,
                alignedScheduleBlock?.ShiftTypeId ?? stampIn.ShiftTypeId,
                alignedScheduleBlock?.TimeScheduleTypeId ?? stampIn.TimeScheduleTypeId,
                employeeChildId,
                null);

            if (excessTimeBlock != null)
            {
                // Accounting from schedule
                if (scheduleBlocks != null)
                {
                    List<TimeScheduleTemplateBlock> scheduleBlocksForAccounting = null;
                    if (addAccountingFromStart)
                        scheduleBlocksForAccounting = scheduleBlocks.Where(s => !s.IsBreak && s.StartTime < stopTime).ToList();
                    else if (addAccountingFromStop)
                        scheduleBlocksForAccounting = scheduleBlocks.Where(s => !s.IsBreak && s.StopTime > startTime).ToList();
                    else if (addAccountingFromClosest)
                        scheduleBlocksForAccounting = scheduleBlocks.GetClosest(excessTimeBlock.StartTime, excessTimeBlock.StopTime).ObjToList();

                    if (!scheduleBlocksForAccounting.IsNullOrEmpty())
                    {
                        foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocksForAccounting.OrderBy(s => s.StartTime))
                        {
                            if (!scheduleBlock.AccountInternal.IsLoaded)
                                scheduleBlock.AccountInternal.Load();

                            AddAccountInternalsToTimeBlock(excessTimeBlock, scheduleBlock.AccountInternal);
                        }
                    }
                }

                //Default TimeCode
                if (!excessTimeBlock.TimeCode.Any() && defaultTimeCode != null)
                    excessTimeBlock.TimeCode.Add(defaultTimeCode);

                //Connect TimeStamps
                if (connectToStamps)
                {
                    excessTimeBlock.TimeStampEntry.Add(stampIn);
                    excessTimeBlock.TimeStampEntry.Add(stampOut);
                }

                AddTimeStampEntryExtendedDetailsToTimeBlock(stampIn, excessTimeBlock);

                excessTimeBlock.DebugInfo = debugInfo;
            }
            return excessTimeBlock;
        }

        private List<TimeBlock> CreateBreakWithSurroundedAbsence(TimeScheduleTemplateBlock scheduleAbsence, TimeScheduleTemplateBlock scheduleBreak, DateTime startTime, DateTime stopTime, TimeBlockDate timeBlockDate, TimeStampEntry stampIn, TimeStampEntry stampOut, Employee employee, EmployeeGroup employeeGroup, TimeScheduleTemplatePeriod templatePeriod, List<TimeScheduleTemplateBlock> scheduleBlocks, TimeDeviationCause timeDeviationCauseEmployee)
        {
            if (scheduleAbsence == null || scheduleBreak == null || timeDeviationCauseEmployee == null)
                return new List<TimeBlock>();

            TimeDeviationCause timeDeviationCauseExcess = scheduleAbsence.TimeDeviationCauseId.HasValue ? GetTimeDeviationCauseFromCache(scheduleAbsence.TimeDeviationCauseId.Value) : null;
            if (timeDeviationCauseExcess == null)
                return new List<TimeBlock>();

            TimeBlock timeBlockAbsenceBeforeBreak = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, startTime, scheduleBreak.StartTime, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, timeDeviationCauseExcess, scheduleAbsence.EmployeeChildId, addAccountingFromClosest: true, debugInfo: "B3A");
            TimeBlock timeBlockBreak = CreateBreakTimeBlockFromTimeStamps(scheduleBreak.StartTime, scheduleBreak.StopTime, employee, scheduleAbsence, scheduleBreak, timeBlockDate, stampIn, stampOut, timeDeviationCauseEmployee.TimeDeviationCauseId, debugInfo: "B3B");
            TimeBlock timeBlockAbsenceAfterBreak = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, scheduleBreak.StopTime, stopTime, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, timeDeviationCauseExcess, scheduleAbsence.EmployeeChildId, addAccountingFromClosest: true, debugInfo: "B3C");

            return new List<TimeBlock>
            {
                timeBlockAbsenceBeforeBreak,
                timeBlockBreak,
                timeBlockAbsenceAfterBreak
            };
        }

        private TimeBlock CreateTimeBlockFromStamping(DateTime startTime, DateTime stopTime, TimeBlockDate timeBlockDate, TimeStampEntry stampIn, TimeStampEntry stampOut, DateTime scheduleOut, DateTime? scheduleStartAfterHole, Employee employee, TimeDeviationCause deviationCauseStart, TimeDeviationCause deviationCauseStop, TimeDeviationCause deviationCauseEmployee, TimeScheduleTemplatePeriod templatePeriod, TimeScheduleTemplateBlock templateBlock, TimeCode timeCode, EmployeeChild employeeChild = null, List<AccountInternal> accountInternals = null, int? timeScheduleTypeId = null, string debugInfo = "", bool addTimeStampEntryExtendedDetails = false)
        {
            if (startTime == stopTime || employee == null || timeBlockDate == null)
                return null;

            TimeBlock timeBlock = new TimeBlock()
            {
                StartTime = startTime,
                StopTime = stopTime,
                IsPreliminary = false,

                //Set relations
                Employee = employee,
                TimeBlockDate = timeBlockDate,
                TimeScheduleTemplatePeriod = templatePeriod,
                EmployeeChild = employeeChild,
                //ShiftTypeId = shiftTypeId, //Do not send this property since item 51170
                TimeScheduleTypeId = timeScheduleTypeId
            };
            SetCreatedProperties(timeBlock);
            entities.TimeBlock.AddObject(timeBlock);

            //TimeDeviationCause
            if (stampIn != null && stampOut != null)
            {
                DateTime stampInTime = CalendarUtility.GetScheduleTime(stampIn.Time, timeBlockDate.Date, stampIn.Time);
                DateTime stampOutTime = CalendarUtility.GetScheduleTime(stampOut.Time, timeBlockDate.Date, stampOut.Time);

                //Special case. If TimeBlock is not connected to either stamp-in time or stamp-out time, and has presence causes. Use standard cause.
                var presenceCauses = new List<TermGroup_TimeDeviationCauseType>() { TermGroup_TimeDeviationCauseType.Presence, TermGroup_TimeDeviationCauseType.PresenceAndAbsence };
                bool isPresenceTimeBlockBetweenStamps =
                    stampInTime != timeBlock.StartTime &&
                    stampOutTime != timeBlock.StopTime &&
                    (!deviationCauseStart.CandidateForOvertime || !deviationCauseStop.CandidateForOvertime) &&
                    IsTimeDeviationCauseOfAnyType(presenceCauses, deviationCauseStart, deviationCauseStop);

                if (isPresenceTimeBlockBetweenStamps)
                {
                    //Standard causes
                    timeBlock.TimeDeviationCauseStart = deviationCauseEmployee;
                    timeBlock.TimeDeviationCauseStop = deviationCauseEmployee;
                }
                else
                {
                    bool hasOnlySetStopDeviationCause =
                        deviationCauseStart != null &&
                        deviationCauseStop != null &&
                        deviationCauseEmployee != null &&
                        deviationCauseStart.TimeDeviationCauseId == deviationCauseEmployee.TimeDeviationCauseId &&
                        deviationCauseStop.TimeDeviationCauseId != deviationCauseEmployee.TimeDeviationCauseId;

                    //Start
                    if (scheduleStartAfterHole.HasValue && scheduleStartAfterHole.Value == timeBlock.StartTime)
                    {
                        TimeScheduleType timeScheduleType = templateBlock.TimeScheduleTypeId.HasValue ? GetTimeScheduleTypeFromCache(templateBlock.TimeScheduleTypeId.Value) : null;
                        if (timeScheduleType != null && timeScheduleType.IsNotScheduleTime)
                            timeBlock.TimeDeviationCauseStart = deviationCauseStart;
                        else
                            timeBlock.TimeDeviationCauseStart = deviationCauseEmployee;
                    }
                    else if (stampInTime < timeBlock.StartTime && stampOutTime > scheduleOut && hasOnlySetStopDeviationCause)
                        timeBlock.TimeDeviationCauseStart = deviationCauseStop; //StampIn is before TimeBlock start - take stop cause as start. Solve stampout after schedule out with ex overtime
                    else
                        timeBlock.TimeDeviationCauseStart = deviationCauseStart;

                    //Stop
                    if (stampOutTime > timeBlock.StopTime)
                        timeBlock.TimeDeviationCauseStop = deviationCauseStart; //StampOut is after TimeBlock stop - take start cause as stop
                    else
                        timeBlock.TimeDeviationCauseStop = deviationCauseStop;
                }
            }

            //Standard TimeDeviationCause
            if (timeBlock.TimeDeviationCauseStart == null)
                timeBlock.TimeDeviationCauseStart = deviationCauseEmployee;
            if (timeBlock.TimeDeviationCauseStop == null)
                timeBlock.TimeDeviationCauseStop = deviationCauseEmployee;

            //Only set EmployeeChild when used that TimeDeviationCause
            if (timeBlock.TimeDeviationCauseStart == null || !timeBlock.TimeDeviationCauseStart.SpecifyChild)
                timeBlock.EmployeeChild = null;

            //Special case. If presence TimeCode and absence start and stop cause. Change causes to standard.
            bool hasWorkTimeCodeAndAbsenceCauses = IsTimeDeviationCauseOfType(TermGroup_TimeDeviationCauseType.Absence, timeBlock.TimeDeviationCauseStart, timeBlock.TimeDeviationCauseStop) && IsTimeCodeOfType(SoeTimeCodeType.Work, timeCode);
            if (hasWorkTimeCodeAndAbsenceCauses)
            {
                timeBlock.TimeDeviationCauseStart = deviationCauseEmployee;
                timeBlock.TimeDeviationCauseStop = deviationCauseEmployee;
            }

            // TimeCode
            if (timeCode != null)
                timeBlock.TimeCode.Add(timeCode);

            // TimeStampEntry
            if (stampIn != null)
                timeBlock.TimeStampEntry.Add(stampIn);
            if (stampOut != null)
                timeBlock.TimeStampEntry.Add(stampOut);

            // TimeBlockDate
            timeBlock.TimeBlockDate.Status = (int)SoeTimeBlockDateStatus.Regenerating;

            // Accounting
            ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, templateBlock, employee, accountInternals: accountInternals);

            if (addTimeStampEntryExtendedDetails)
                AddTimeStampEntryExtendedDetailsToTimeBlock(stampIn, timeBlock);

            timeBlock.DebugInfo = debugInfo;

            return timeBlock;
        }

        private TimeBlock CreateBreakTimeBlockFromTimeStamps(DateTime startTime, DateTime stopTime, Employee employee, TimeScheduleTemplateBlock closestSchedule, TimeScheduleTemplateBlock scheduleBreak, TimeBlockDate timeBlockDate, TimeStampEntry stampIn, TimeStampEntry stampOut, int timeDeviationCauseId, List<TimeBlock> existingTimeBlocks = null, string debugInfo = "")
        {
            if (timeBlockDate == null || scheduleBreak == null || employee == null)
                return null;
            if (existingTimeBlocks.IsNewOverlappedByCurrent(startTime, stopTime))
                return null;

            TimeBlock timeBlock = CreateTimeBlock(startTime, stopTime, employee, scheduleBreak, scheduleBreak.TimeCode, timeBlockDate, timeDeviationCauseId, isStamping: true, doApplyAccounting: false);
            if (timeBlock != null)
            {
                ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, closestSchedule, employee);

                if (!timeBlock.IsAdded())
                    entities.TimeBlock.AddObject(timeBlock);

                if (stampIn != null)
                    timeBlock.TimeStampEntry.Add(stampIn);
                if (stampOut != null)
                    timeBlock.TimeStampEntry.Add(stampOut);
            }

            if (timeBlock != null)
                timeBlock.DebugInfo = debugInfo;

            return timeBlock;
        }

        private TimeBlock CreateBreakTimeBlock(ApplyBreakDTO dto, int length, TimeCodeRuleDTO timeCodeRule, ref List<TimeBlock> breakTimeBlocks, ref List<TimeBlock> nonBreakTimeBlocks, bool manuallyAdjusted)
        {
            TimeBlock breakTimeBlock = null;
            if (timeCodeRule == null || breakTimeBlocks == null || nonBreakTimeBlocks == null)
                return breakTimeBlock;

            breakTimeBlock = CreateBreakTimeBlock(
                dto.ScheduleBreakDTO,
                nonBreakTimeBlocks.FirstOrDefault(),
                length,
                manuallyAdjusted);
            if (breakTimeBlock == null)
                return breakTimeBlock;

            breakTimeBlocks.Add(breakTimeBlock);

            //Apply rule
            AddRuleDepictedTimeCodeToTimeBlock(breakTimeBlock, timeCodeRule);

            //Update non-break TimeBlock's
            nonBreakTimeBlocks.AddRange(RearrangeNewTimeBlockAgainstExisting(breakTimeBlock, nonBreakTimeBlocks, null, false, false));

            return breakTimeBlock;
        }

        private TimeBlock CreateBreakTimeBlock(TimeBlock prototype, int timeCodeId, bool manuallyAdjusted = false, bool addTimeCodes = false)
        {
            return CreateBreakTimeBlock(
                prototype,
                prototype?.StartTime ?? CalendarUtility.DATETIME_DEFAULT,
                prototype?.StopTime ?? CalendarUtility.DATETIME_DEFAULT,
                timeCodeId,
                manuallyAdjusted,
                addTimeCodes);
        }

        private TimeBlock CreateBreakTimeBlock(TimeBlock prototype, DateTime startTime, DateTime stopTime, int timeCodeId, bool manuallyAdjusted = false, bool addTimeCodes = false)
        {
            return CreateBreakTimeBlock(
                prototype,
                startTime,
                stopTime,
                timeCodeId,
                prototype?.TimeScheduleTemplateBlockBreakId,
                prototype?.TimeDeviationCauseStartId,
                prototype?.TimeDeviationCauseStopId,
                prototype?.CalculatedOutsideBreakWindow,
                manuallyAdjusted,
                addTimeCodes);
        }

        private TimeBlock CreateBreakTimeBlock(TimeScheduleTemplateBlockDTO scheduleBlock, TimeBlock prototype = null, int? length = null, bool manuallyAdjusted = false, bool addTimeCodes = false, bool forceZero = false)
        {
            if (scheduleBlock == null)
                return null;

            if (forceZero && !length.HasValue)
                length = 0;
            if (length.HasValue && length.Value <= 0 && !forceZero)
                return null;

            return CreateBreakTimeBlock(
                prototype,
                scheduleBlock.StartTime,
                length.HasValue ? scheduleBlock.StartTime.AddMinutes(length.Value) : scheduleBlock.StopTime,
                scheduleBlock.TimeCode?.TimeCodeId ?? scheduleBlock.TimeCodeId,
                scheduleBlock.TimeScheduleTemplateBlockId,
                scheduleBlock.TimeDeviationCauseId,
                scheduleBlock.TimeDeviationCauseId,
                prototype?.CalculatedOutsideBreakWindow,
                manuallyAdjusted,
                addTimeCodes);
        }

        private TimeBlock CreateBreakTimeBlock(TimeBlock prototype, DateTime startTime, DateTime stopTime, int timeCodeId, int? templateBlockBreakId, int? timeDeviationCauseStartId, int? timeDeviationCauseStopId, bool? calculatedOutsideBreakWindow = null, bool manuallyAdjusted = false, bool addTimeCodes = false)
        {
            TimeBlock timeBlock = new TimeBlock
            {
                StartTime = startTime,
                StopTime = stopTime,
                IsBreak = true,
                CalculatedOutsideBreakWindow = calculatedOutsideBreakWindow,
                ManuallyAdjusted = manuallyAdjusted,

                //Set FK
                TimeScheduleTemplateBlockBreakId = templateBlockBreakId,
                TimeDeviationCauseStartId = timeDeviationCauseStartId,
                TimeDeviationCauseStopId = timeDeviationCauseStopId,
                ProjectTimeBlockId = prototype?.ProjectTimeBlockId,
                PayrollImportEmployeeTransactionId = prototype?.PayrollImportEmployeeTransactionId,
            };
            SetCreatedProperties(timeBlock);
            entities.TimeBlock.AddObject(timeBlock);

            AddTimeCodesToTimeBlock(timeBlock, addTimeCodes ? prototype?.TimeCode : null, timeCodeId);
            AddTimeStampEntrysToTimeBlock(timeBlock, prototype);

            return timeBlock;
        }

        private TimeBlock CreateTimeBlockWithoutAccounting(TimeBlock prototype, DateTime startTime, DateTime stopTime, int timeCodeId, bool manuallyAdjusted)
        {
            TimeBlock timeBlock = new TimeBlock
            {
                StartTime = startTime,
                StopTime = stopTime,
                ManuallyAdjusted = manuallyAdjusted,

                //Set FK
                TimeDeviationCauseStartId = prototype?.TimeDeviationCauseStartId,
                TimeDeviationCauseStopId = prototype?.TimeDeviationCauseStopId,
                ProjectTimeBlockId = prototype?.ProjectTimeBlockId,
                PayrollImportEmployeeTransactionId = prototype?.PayrollImportEmployeeTransactionId,
            };
            SetCreatedProperties(timeBlock);
            entities.TimeBlock.AddObject(timeBlock);

            AddTimeCodesToTimeBlock(timeBlock, prototype?.TimeCode, timeCodeId);
            AddTimeStampEntrysToTimeBlock(timeBlock, prototype);

            return timeBlock;
        }

        private TimeBlock CreateTimeBlock(DateTime startTime, DateTime stopTime, Employee employee, TimeScheduleTemplateBlock scheduleBlock, TimeCode timeCode, TimeBlockDate timeBlockDate, int? standardTimeDeviationCauseId, bool isStamping = false, bool temporary = false, bool doApplyAccounting = true)
        {
            if (scheduleBlock == null || timeCode == null || timeBlockDate == null || employee == null)
                return null;

            TimeBlock timeBlock = new TimeBlock()
            {
                StartTime = startTime,
                StopTime = stopTime,
                IsBreak = timeCode.IsBreak(),
                State = temporary ? (int)SoeEntityState.Temporary : (int)SoeEntityState.Active,
                IsPreliminary = scheduleBlock.IsPreliminary,

                //Set FK
                EmployeeId = employee.EmployeeId,
                TimeScheduleTemplatePeriodId = scheduleBlock.TimeScheduleTemplatePeriodId,

                //Set relations
                TimeBlockDate = timeBlockDate,
            };
            SetCreatedProperties(timeBlock);
            entities.TimeBlock.AddObject(timeBlock);

            //Link to schedule
            timeBlock.GuidTemplateBlock = scheduleBlock.Guid;

            //TimeDeviationCause
            if (!isStamping && scheduleBlock.TimeDeviationCauseId.HasValue)
            {
                timeBlock.TimeDeviationCauseStartId = scheduleBlock.TimeDeviationCauseId.Value;
                timeBlock.TimeDeviationCauseStopId = scheduleBlock.TimeDeviationCauseId.Value;
            }
            else if (standardTimeDeviationCauseId.HasValue && standardTimeDeviationCauseId.Value > 0)
            {
                timeBlock.TimeDeviationCauseStartId = standardTimeDeviationCauseId;
                timeBlock.TimeDeviationCauseStopId = standardTimeDeviationCauseId;
            }

            // Break
            if (timeBlock.IsBreak)
                timeBlock.TimeScheduleTemplateBlockBreakId = scheduleBlock.TimeScheduleTemplateBlockId;

            // TimeCode
            timeBlock.TimeCode.Add(timeCode);

            // Accounting
            if (doApplyAccounting)
                ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, scheduleBlock, employee);

            return timeBlock;
        }

        private TimeBlock CreateTempTimeBlock(TimeBlock prototype, bool isSchedulePreliminaryTimeBlock = false, bool isScheduleAbsenceTimeBlock = false, bool isSickDuringIwhTimeBlock = false, bool isSickDuringStandbyTimeBlock = false)
        {
            if (prototype == null)
                return null;

            TimeBlock newTimeBlock = new TimeBlock()
            {
                GuidId = Guid.NewGuid(),
                State = (int)SoeEntityState.Temporary,
                StartTime = prototype.StartTime,
                StopTime = prototype.StopTime,
                IsBreak = false,
                IsPreliminary = false,
                IsSchedulePreliminaryTimeBlock = isSchedulePreliminaryTimeBlock,
                IsScheduleAbsenceTimeBlock = isScheduleAbsenceTimeBlock,
                IsSickDuringIwhTimeBlock = isSickDuringIwhTimeBlock,
                IsSickDuringStandbyTimeBlock = isSickDuringStandbyTimeBlock,
                ManuallyAdjusted = false,
                Comment = null,

                ShiftTypeId = prototype.ShiftTypeId,
                TimeScheduleTypeId = prototype.TimeScheduleTypeId,
                EmployeeChildId = null,

                CalculatedShiftTypeId = prototype.CalculatedShiftTypeId,
                CalculatedTimeScheduleTypeIdFromShift = prototype.CalculatedTimeScheduleTypeIdFromShift,
                CalculatedTimeScheduleTypeIdFromShiftType = prototype.CalculatedTimeScheduleTypeIdFromShiftType,
                CalculatedTimeScheduleTypeId = prototype.CalculatedTimeScheduleTypeId,
                CalculatedTimeScheduleTypeIdsFromEmployee = prototype.CalculatedTimeScheduleTypeIdsFromEmployee,
                CalculatedTimeScheduleTypeIdsFromTimeStamp = prototype.CalculatedTimeScheduleTypeIdsFromTimeStamp,
                CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes = prototype.CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes,
            };
            SetCreatedProperties(newTimeBlock);
            entities.TimeBlock.AddObject(newTimeBlock);

            TimeCode defaultTimeCode = GetTimeCodeFromCache(GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode));
            if (defaultTimeCode != null)
                newTimeBlock.TimeCode.Add(defaultTimeCode);

            return newTimeBlock;
        }

        private TimeBlock ReCreateBreakTimeBlockIfNeeded(TimeBlock prototype, int timeCodeId, bool manuallyAdjusted, out bool created)
        {
            created = false;

            if (prototype == null || prototype.StartTime == prototype.StopTime)
                return null;

            if (prototype.IsAdded() || prototype.EntityState != EntityState.Detached)
            {
                AddTimeCodeToTimeBlock(prototype, timeCodeId);

                if (!prototype.IsAdded() && prototype.ManuallyAdjusted != manuallyAdjusted)
                {
                    prototype.ManuallyAdjusted = manuallyAdjusted;
                    SetModifiedProperties(prototype);
                }
                return prototype;
            }
            else
            {
                created = true;
                return CreateBreakTimeBlock(prototype, timeCodeId, manuallyAdjusted, addTimeCodes: true);
            }
        }

        private void ReCreateBreakTimeBlock(List<TimeBlock> breakTimeBlocks, int position, TimeCodeRuleDTO timeCodeRule, bool manuallyAdjusted)
        {
            if (breakTimeBlocks.IsNullOrEmpty() || breakTimeBlocks.Count <= position)
                return;

            TimeBlock breakTimeBlock = breakTimeBlocks[position];
            TimeBlock newBreakTimeBlock = ReCreateBreakTimeBlock(breakTimeBlock, timeCodeRule, manuallyAdjusted);
            if (newBreakTimeBlock != null)
                breakTimeBlocks[position] = newBreakTimeBlock;
        }

        private TimeBlock ReCreateBreakTimeBlock(TimeBlock prototype, TimeCodeRuleDTO timeCodeRule, bool manuallyAdjusted)
        {
            if (prototype == null || timeCodeRule == null)
                return null;

            TimeCode ruleTimeCode = GetRuleDepictedTimeCode(timeCodeRule);
            if (ruleTimeCode == null)
                return null;

            TimeBlock breakTimeBlock = ReCreateBreakTimeBlockIfNeeded(prototype, ruleTimeCode.TimeCodeId, manuallyAdjusted, out bool created);
            if (created)
                SetTimeBlockAndTransactionsToDeleted(prototype, saveChanges: false);

            return breakTimeBlock;
        }

        private TimeBlock ReCreateGeneratedTimeBlock(TimeBlock prototype)
        {
            if (prototype == null)
                return null;

            //Cannot use EntityUtil.Clone, as it also copies EF dependent properties (ex: EntityState)
            TimeBlock newTimeBlock = new TimeBlock()
            {
                GuidId = Guid.NewGuid(),
                State = prototype.State,
                StartTime = prototype.StartTime,
                StopTime = prototype.StopTime,
                IsBreak = prototype.IsBreak,
                IsPreliminary = prototype.IsPreliminary,
                IsSchedulePreliminaryTimeBlock = false,
                IsScheduleAbsenceTimeBlock = false,
                IsSickDuringIwhTimeBlock = false,
                IsSickDuringStandbyTimeBlock = false,

                CalculatedOutsideBreakWindow = prototype.CalculatedOutsideBreakWindow,
                ManuallyAdjusted = prototype.ManuallyAdjusted,
                Comment = prototype.Comment,

                ShiftTypeId = prototype.ShiftTypeId,
                TimeScheduleTypeId = prototype.TimeScheduleTypeId,
                EmployeeChildId = prototype.EmployeeChildId,
            };
            SetCreatedProperties(newTimeBlock);
            entities.TimeBlock.AddObject(newTimeBlock);

            if (prototype.IsBreakOrGeneratedFromBreak)
            {
                //Add not TimeCode's for breaks, they will be re-generated by rule evaluation
                AddBreakCommonToTimeBlock(newTimeBlock, prototype, null, null);
            }
            else
            {
                List<TimeCode> timeCodes = prototype.TimeCode.ToList();
                AddPresenceCommonToTimeBlock(newTimeBlock, prototype, timeCodes, null);
            }

            return newTimeBlock;
        }

        private TimeBlock FindTempPresenceTimeBlockOriginalAbenceTimeBlock(TimeBlock tempPresenceTimeBlock, List<TimeBlock> absenceTimeBlocks)
        {
            if (tempPresenceTimeBlock == null)
                return null;

            return absenceTimeBlocks?.FirstOrDefault(t => t.StartTime == tempPresenceTimeBlock.StartTime && t.StopTime == tempPresenceTimeBlock.StopTime && t.State != (int)SoeEntityState.Temporary);
        }

        private List<AccountInternal> LoadDeviationAccounts(TimeBlock timeBlock)
        {
            if (timeBlock.DeviationAccountsNotLoaded)
                TimeBlockManager.LoadDeviationAccounts(this.entities, timeBlock, GetAccountInternalsWithAccountFromCache());
            return timeBlock.DeviationAccounts?.ToList();
        }

        private void LoadDeviationAccounts(List<TimeBlock> timeBlocks)
        {
            if (timeBlocks.Any(tb => tb.DeviationAccountsNotLoaded))
                TimeBlockManager.LoadDeviationAccounts(this.entities, timeBlocks, GetAccountInternalsWithAccountFromCache());
        }

        private void SetIsAttested(List<TimeBlock> timeBlocks)
        {
            if (timeBlocks == null)
                return;

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            AttestStateDTO attestStateInitialInvoice = GetAttestStateInitialFromCache(TermGroup_AttestEntity.InvoiceTime);
            timeBlocks.SetIsAttested(attestStateInitialPayroll, attestStateInitialInvoice);
        }

        private void SetIsTransferedToSalary(List<TimeBlock> timeBlocks)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                //TimeInvoiceTransaction
                List<TimeInvoiceTransaction> timeInvoiceTransactions = timeBlock.TimeInvoiceTransaction.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                if (timeInvoiceTransactions.Any())
                {
                    AttestStateDTO attestStateResultingInvoice = GetAttestStateFromCache(GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportInvoiceResultingAttestStatus));
                    if (attestStateResultingInvoice != null && timeInvoiceTransactions.Count(i => i.AttestStateId == attestStateResultingInvoice.AttestStateId) == timeInvoiceTransactions.Count)
                    {
                        timeBlock.IsTransferedToSalary = true;
                        continue;
                    }
                }

                //TimePayrollTransaction
                List<TimePayrollTransaction> timePayrollTransactions = timeBlock.TimePayrollTransaction.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                if (timePayrollTransactions.Any())
                {
                    if (UsePayroll())
                    {
                        AttestStateDTO salaryPaymentExportFileCreatedAttestStateId = GetAttestStateFromCache(GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId));
                        if (salaryPaymentExportFileCreatedAttestStateId != null && timePayrollTransactions.Count(i => i.AttestStateId == salaryPaymentExportFileCreatedAttestStateId.AttestStateId) == timePayrollTransactions.Count)
                            timeBlock.IsTransferedToSalary = true;
                    }
                    else
                    {
                        // Get AttestState resulting payroll
                        AttestStateDTO attestStateResultingPayroll = GetAttestStateFromCache(GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus));
                        if (attestStateResultingPayroll != null && timePayrollTransactions.Count(i => i.AttestStateId == attestStateResultingPayroll.AttestStateId) == timePayrollTransactions.Count)
                            timeBlock.IsTransferedToSalary = true;
                    }
                }
            }
        }

        private void AdjustAfterScheduleInOut(TimeBlock timeBlock, TimeDeviationCause timeDeviationCause, DateTime date, int employeeId, bool adjustFirstTimeBlockStopToScheduleOut, TermGroup_TimeDeviationCauseType choosenDeviationCauseType)
        {
            if (timeBlock == null || timeDeviationCause == null)
                return;

            DateTime? scheduleIn = GetScheduleInFromCache(employeeId, date);
            DateTime? scheduleOut = GetScheduleOutFromCache(employeeId, date);
            if (!scheduleIn.HasValue || !scheduleOut.HasValue)
                return;

            if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence || (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence && choosenDeviationCauseType == TermGroup_TimeDeviationCauseType.Absence))
            {
                if (timeBlock.StartTime < scheduleIn.Value)
                    timeBlock.StartTime = scheduleIn.Value;
                if (timeBlock.StopTime > scheduleOut.Value)
                    timeBlock.StopTime = scheduleOut.Value;
                if (timeBlock.StartTime > timeBlock.StopTime)
                    timeBlock.StartTime = timeBlock.StopTime;
            }

            if (adjustFirstTimeBlockStopToScheduleOut)
                timeBlock.StopTime = scheduleOut.Value;
        }

        private void AddTimeCodesToTimeBlock(TimeBlock timeBlock, IEnumerable<TimeCode> timeCodes, params int[] timeCodeIds)
        {
            if (timeBlock == null)
                return;

            if (!timeCodes.IsNullOrEmpty())
            {
                foreach (TimeCode timeCode in timeCodes)
                    AddTimeCodeToTimeBlock(timeBlock, timeCode);
            }
            if (!timeCodeIds.IsNullOrEmpty())
            {
                foreach (int timeCodeId in timeCodeIds.Where(id => id > 0))
                    AddTimeCodeToTimeBlock(timeBlock, timeCodeId);
            }
        }

        private void AddTimeCodeToTimeBlock(TimeBlock timeBlock, int timeCodeId)
        {
            AddTimeCodeToTimeBlock(timeBlock, GetTimeCodeFromCache(timeCodeId));
        }

        private void AddTimeCodeToTimeBlock(TimeBlock timeBlock, TimeCode timeCode)
        {
            if (timeBlock?.TimeCode == null || timeCode == null || timeBlock.TimeCode.Any(i => i.TimeCodeId == timeCode.TimeCodeId))
                return;

            if (timeBlock.TimeBlockId > 0)
                SetModifiedProperties(timeBlock);

            timeBlock.TimeCode.Add(timeCode);
        }

        private void AddAccountInternalsToTimeBlock(TimeBlock clone, TimeBlock prototype)
        {
            if (clone == null || prototype?.AccountInternal == null)
                return;

            clone.SetDeviationAccounts(prototype);

            if (base.CanEntityLoadReferences(entities, prototype) && !prototype.AccountInternal.IsLoaded)
                prototype.AccountInternal.Load();

            if (!prototype.AccountInternal.IsNullOrEmpty())
            {
                if (CanEntityLoadReferences(entities, clone) && !clone.AccountInternal.IsLoaded)
                    clone.AccountInternal.Load();

                AddAccountInternalsToTimeBlock(clone, prototype.AccountInternal);
            }
        }

        private void AddTimeStampEntrysToTimeBlock(TimeBlock clone, TimeBlock prototype)
        {
            if (clone == null || prototype?.TimeStampEntry == null)
                return;

            if (base.CanEntityLoadReferences(entities, prototype) && !prototype.TimeStampEntry.IsLoaded)
                prototype.TimeStampEntry.Load();

            if (prototype.TimeStampEntry.Any())
            {
                if (base.CanEntityLoadReferences(entities, clone) && !clone.TimeStampEntry.IsLoaded)
                    clone.TimeStampEntry.Load();

                foreach (TimeStampEntry entry in prototype.TimeStampEntry)
                {
                    if (!clone.TimeStampEntry.Any(i => i.TimeStampEntryId == entry.TimeStampEntryId))
                        clone.TimeStampEntry.Add(entry);
                }

                //Set comment
                SetCommentOnTimeBlockFromTimeStamp(clone);
            }
        }

        private void AddCommonToTimeBlock(TimeBlock clone, TimeBlock prototype, List<TimeCode> timeCodes, TimeCodeRuleDTO timeCodeRule)
        {
            if (clone == null || prototype == null)
                return;

            #region TimeScheduleTemplateBlock

            if (!clone.TimeScheduleTemplateBlockBreakId.HasValue && prototype.TimeScheduleTemplateBlockBreakId.HasValue)
                clone.TimeScheduleTemplateBlockBreakId = prototype.TimeScheduleTemplateBlockBreakId.Value;

            #endregion

            #region TimeCode

            if (timeCodes != null)
            {
                //Add TimeCode's passed
                foreach (TimeCode timeCode in timeCodes)
                {
                    if (clone.TimeCode.Any(i => i.TimeCodeId == timeCode.TimeCodeId))
                        continue;

                    //Get TimeCode from cache
                    TimeCode originalTimeCode = GetTimeCodeFromCache(timeCode.TimeCodeId);
                    if (originalTimeCode != null)
                        clone.TimeCode.Add(originalTimeCode);
                }
            }
            else
            {
                timeCodes = new List<TimeCode>();
            }

            //Add TimeCode's for TimeRule
            if (timeCodeRule != null)
            {
                TimeCode ruleTimeCode = GetRuleDepictedTimeCode(timeCodeRule);
                if (ruleTimeCode != null)
                {
                    AddTimeCodeToTimeBlock(clone, ruleTimeCode);
                    timeCodes.Add(ruleTimeCode);
                }
            }

            //Clear all other TimeCode's
            for (int i = 0; i < clone.TimeCode.Count; i++)
            {
                TimeCode timeCode = clone.TimeCode.ToList()[i];

                //Verify against list
                if (timeCodes.Any(tc => tc.TimeCodeId == timeCode.TimeCodeId))
                    continue;

                //Delete if not in list
                clone.TimeCode.Remove(timeCode);
            }

            #endregion

            #region TimeStampEntry

            AddTimeStampEntrysToTimeBlock(clone, prototype);

            #endregion
        }

        private void AddPresenceCommonToTimeBlock(TimeBlock clone, TimeBlock prototype, List<TimeCode> timeCodes, TimeCodeRuleDTO timeCodeRule)
        {
            if (clone == null || prototype == null)
                return;

            AddCommonToTimeBlock(clone, prototype, timeCodes, timeCodeRule);

            #region Properties

            clone.IsBreak = false;

            #endregion

            #region TimeDeviationCause

            if (prototype.TimeDeviationCauseStartId.HasValue)
            {
                //Get TimeDeviationCause from cache
                clone.TimeDeviationCauseStart = GetTimeDeviationCauseFromCache(prototype.TimeDeviationCauseStartId.Value);
                clone.TimeDeviationCauseStop = GetTimeDeviationCauseFromCache(prototype.TimeDeviationCauseStartId.Value);
            }

            #endregion
        }

        private void AddBreakCommonToTimeBlock(TimeBlock clone, TimeBlock prototype, List<TimeCode> timeCodes, TimeCodeRuleDTO timeCodeRule)
        {
            if (clone == null || prototype == null)
                return;

            AddCommonToTimeBlock(clone, prototype, timeCodes, timeCodeRule);

            #region Properties

            clone.IsBreak = true;

            #endregion

            #region TimeDeviationCause

            if (clone.TimeDeviationCauseStart == null && prototype.TimeDeviationCauseStartId.HasValue)
                clone.TimeDeviationCauseStart = GetTimeDeviationCauseFromCache(prototype.TimeDeviationCauseStartId.Value);
            if (clone.TimeDeviationCauseStop == null && prototype.TimeDeviationCauseStopId.HasValue)
                clone.TimeDeviationCauseStop = GetTimeDeviationCauseFromCache(prototype.TimeDeviationCauseStopId.Value);

            #endregion
        }

        private void AddTimeCodesGeneratedByBlock(TimeBlock timeBlock, List<int> timeCodeIds)
        {
            if (timeBlock == null || timeCodeIds.IsNullOrEmpty())
                return;

            foreach (int timeCodeId in timeCodeIds)
            {
                TimeCode timeCode = GetTimeCodeFromCache(timeCodeId);
                if (timeCode != null)
                    timeBlock.TimeCode.Add(timeCode);
            }
        }

        private void AddTimeBlocksToCollection(List<TimeBlock> collection, List<TimeBlock> timeBlocks)
        {
            foreach (TimeBlock timeBlock in timeBlocks)
            {
                AddTimeBlockToCollection(collection, timeBlock);
            }
        }

        private bool AddTimeBlockToCollection(List<TimeBlock> collection, TimeBlock timeBlock)
        {
            if (collection == null || timeBlock == null)
                return false;

            if (collection.Any(tb => tb.StartTime == timeBlock.StartTime))
            {
                ChangeEntityState(timeBlock, SoeEntityState.Deleted);
                CheckAttemptToAddDuplicateTimeBlock(collection, timeBlock);
                return false;
            }

            collection.Add(timeBlock);
            return true;
        }

        private void DecideTimeBlockTypes(List<TimeBlock> timeBlocks)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                DecideTimeBlockType(timeBlock);
            }
        }

        private void DecideTimeBlockType(TimeBlock timeBlock)
        {
            if (timeBlock == null)
                return;

            //Get TimeDeviationCause from cache instead of loading reference, since TimeBlock may not be attached
            if (timeBlock.TimeDeviationCauseStart == null && timeBlock.TimeDeviationCauseStartId.HasValue)
                timeBlock.TimeDeviationCauseStart = GetTimeDeviationCauseFromCache(timeBlock.TimeDeviationCauseStartId.Value);

            bool hasDeviationCause = timeBlock.TimeDeviationCauseStart != null && (timeBlock.TimeDeviationCauseStart.Type == (int)TermGroup_TimeDeviationCauseType.Presence || timeBlock.TimeDeviationCauseStart.Type == (int)TermGroup_TimeDeviationCauseType.Absence);
            bool hasTimeCodes = !timeBlock.TimeCode.IsNullOrEmpty();

            // Prio according to Rickard 2011-03-08:
            // 1) TimeDeviationCause (Presence or Absence)
            // 2) TimeCode (Presence or Absence)
            // 3) Default is absence

            if (hasDeviationCause)
            {
                if (timeBlock.TimeDeviationCauseStart.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                {
                    if (timeBlock.TimeDeviationCauseStart.Payed)
                        timeBlock.CalculatedAsAbsenceAndPresence = true;
                    else
                        timeBlock.CalculatedAsAbsence = true;
                }
                else if (timeBlock.TimeDeviationCauseStart.Type == (int)TermGroup_TimeDeviationCauseType.Presence)
                    timeBlock.CalculatedAsPresence = true;
            }
            else if (hasTimeCodes)
            {
                if (timeBlock.TimeCode.Any(i => i.Type == (int)SoeTimeCodeType.Absense))
                {
                    if (timeBlock.TimeCode.Any(i => i.Type == (int)SoeTimeCodeType.Absense))
                        timeBlock.CalculatedAsAbsenceAndPresence = true;
                    else
                        timeBlock.CalculatedAsAbsence = true;
                }
                else if (timeBlock.TimeCode.Any(i => i.Type == (int)SoeTimeCodeType.Work))
                    timeBlock.CalculatedAsPresence = true;
            }
            else
            {
                timeBlock.CalculatedAsPresence = false;
                timeBlock.CalculatedAsAbsence = true;
                timeBlock.CalculatedAsAbsenceAndPresence = false;
            }
        }

        private void DecideTimeBlockStandby(TimeBlock timeBlock, List<TimeScheduleTemplateBlock> scheduleBlocks, EmployeeGroup employeeGroup, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            DecideTimeBlockStandby(new List<TimeBlock> { timeBlock }, scheduleBlocks, employeeGroup, timeDeviationCauses);
        }

        private void DecideTimeBlockStandby(List<TimeBlock> timeBlocks, List<TimeScheduleTemplateBlock> scheduleBlocks, EmployeeGroup employeeGroup, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            if (!timeBlocks.DoDecideTimeBlockStandby(scheduleBlocks))
                return;

            if (timeDeviationCauses == null)
                timeDeviationCauses = GetTimeDeviationCausesFromCache();

            timeBlocks.DecideTimeBlockStandby(scheduleBlocks, employeeGroup, timeDeviationCauses);
        }

        private void DecideExcludeFromPresenceWorkRules(List<TimeBlock> timeBlocks, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            if (timeBlocks.IsNullOrEmpty() || timeBlocks.All(i => i.CalculatedAsExcludeFromPresenceWorkRules.HasValue))
                return;

            if (timeDeviationCauses == null)
                timeDeviationCauses = GetTimeDeviationCausesFromCache();
            if (timeDeviationCauses.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                TimeDeviationCause timeDeviationCause = timeBlock.TimeDeviationCauseStartId.HasValue ? timeDeviationCauses.FirstOrDefault(i => i.TimeDeviationCauseId == timeBlock.TimeDeviationCauseStartId.Value) : null;
                timeBlock.CalculatedAsExcludeFromPresenceWorkRules = timeDeviationCause?.ExcludeFromPresenceWorkRules;
            }
        }

        private (List<TimeBlock> breakTimeBlocks, List<TimeBlock> presenceTimeBlocks, List<TimeBlock> absenceTimeBlocks, List<TimeBlock> absenceAndPresenceTimeBlocks) SplitAfterTimeBlockTypes(TimeEngineTemplate template, List<TimeAbsenceRuleHead> absenceRules)
        {
            var breakTimeBlocks = new List<TimeBlock>();
            var presenceTimeBlocks = new List<TimeBlock>();
            var absenceTimeBlocks = new List<TimeBlock>();
            var absenceAndPresenceTimeBlocks = new List<TimeBlock>();

            if (template?.Identity?.TimeBlocks != null || template?.Outcome != null)
            {
                foreach (TimeBlock timeBlock in template.Identity.TimeBlocks.OrderBy(i => i.StartTime))
                {
                    if (timeBlock.State == (int)SoeEntityState.Deleted)
                        continue;

                    if (!timeBlock.GuidId.HasValue)
                        timeBlock.GuidId = Guid.NewGuid();

                    if (timeBlock.IsBreakOrGeneratedFromBreak)
                    {
                        #region Break

                        breakTimeBlocks.Add(timeBlock);

                        #endregion
                    }
                    else
                    {
                        #region Presence / Absence

                        DecideTimeBlockType(timeBlock);

                        if (timeBlock.CalculatedAsPresence == true || timeBlock.CalculatedAsAbsenceAndPresence == true)
                        {
                            #region Presence

                            presenceTimeBlocks.Add(timeBlock);

                            #endregion
                        }
                        if (timeBlock.CalculatedAsAbsence == true || timeBlock.CalculatedAsAbsenceAndPresence == true)
                        {
                            #region Absence

                            absenceTimeBlocks.Add(timeBlock);

                            #region Create temp presence TimeBlock for schedule transactions

                            if (UsePayroll())
                            {
                                TimeBlock tempPresenceTimeBlock = CreateTempTimeBlock(timeBlock, isScheduleAbsenceTimeBlock: true);
                                if (tempPresenceTimeBlock != null)
                                    presenceTimeBlocks.Add(tempPresenceTimeBlock);
                            }

                            #endregion

                            #region Create temp presence TimeBlock for sick during iwh and standby

                            if (!absenceRules.IsNullOrEmpty() && absenceRules.Any(i => i.IsSickDuringIwhOrStandBy))
                            {
                                List<TimeAbsenceRuleHead> timeAbsenceRuleHeadsForTimeBlock = new List<TimeAbsenceRuleHead>();
                                bool hasSickDuringStandbyRule = absenceRules.Any(i => i.IsSickDuringStandby);
                                TimeScheduleTemplateBlock matchingTemplateBlock = hasSickDuringStandbyRule ? template.Identity.ScheduleBlocks.GetMatchingScheduleBlock(timeBlock, false) : null;
                                bool isStandby = matchingTemplateBlock?.IsStandby() ?? false;

                                // Match each TimeBlock's TimeCode's against each TimeAbsenceRuleHead
                                foreach (TimeCode timeCode in timeBlock.TimeCode)
                                {
                                    foreach (TimeAbsenceRuleHead absenceRule in absenceRules.Where(i => i.IsSickDuringIwhOrStandBy && i.TimeCodeId == timeCode.TimeCodeId))
                                    {
                                        if (absenceRule.IsSickDuringStandby && !isStandby)
                                            continue;

                                        if (!timeAbsenceRuleHeadsForTimeBlock.Any(i => i.TimeAbsenceRuleHeadId == absenceRule.TimeAbsenceRuleHeadId))
                                            timeAbsenceRuleHeadsForTimeBlock.Add(absenceRule);
                                    }
                                }

                                foreach (TimeAbsenceRuleHead timeAbsenceRuleHead in timeAbsenceRuleHeadsForTimeBlock)
                                {
                                    if (!template.Outcome.TimeAbsenceRules.Any(i => i.TimeAbsenceRuleHeadId == timeAbsenceRuleHead.TimeAbsenceRuleHeadId))
                                        template.Outcome.TimeAbsenceRules.Add(timeAbsenceRuleHead);
                                }

                                if (timeAbsenceRuleHeadsForTimeBlock.Any(i => i.IsSickDuringIwhOrStandBy))
                                {
                                    TimeBlock tempPresenceTimeBlock = CreateTempTimeBlock(timeBlock, isSickDuringIwhTimeBlock: !isStandby, isSickDuringStandbyTimeBlock: isStandby);
                                    if (tempPresenceTimeBlock != null)
                                        presenceTimeBlocks.Add(tempPresenceTimeBlock);
                                }
                            }

                            #endregion

                            #endregion
                        }
                        if (timeBlock.CalculatedAsAbsenceAndPresence == true)
                        {
                            #region Presence and Absence

                            absenceAndPresenceTimeBlocks.Add(timeBlock);

                            #endregion
                        }

                        #endregion
                    }
                }

                if (absenceTimeBlocks.Any())
                    template.Outcome.TimeAbsenceRules.AddRange(absenceRules.Where(i => !i.IsSickDuringIwhOrStandBy));
            }

            return (breakTimeBlocks, presenceTimeBlocks, absenceTimeBlocks, absenceAndPresenceTimeBlocks);
        }

        private void SetTimeBlockTimeDevationCauses(List<TimeBlock> timeBlocks, int? timeDeviationCauseId = null)
        {
            if (timeBlocks == null)
                return;

            if (timeDeviationCauseId.HasValue && timeDeviationCauseId.Value > 0)
            {
                foreach (TimeBlock timeBlock in timeBlocks)
                {
                    if (!timeBlock.TimeDeviationCauseStartId.HasValue)
                        timeBlock.TimeDeviationCauseStartId = timeDeviationCauseId.Value;
                    if (!timeBlock.TimeDeviationCauseStopId.HasValue)
                        timeBlock.TimeDeviationCauseStopId = timeDeviationCauseId.Value;
                }
            }
            else
            {
                foreach (TimeBlock timeBlock in timeBlocks)
                {
                    if (timeBlock.TimeDeviationCauseStart != null)
                        timeBlock.TimeDeviationCauseStartId = timeBlock.TimeDeviationCauseStart.TimeDeviationCauseId;
                    if (timeBlock.TimeDeviationCauseStop != null)
                        timeBlock.TimeDeviationCauseStopId = timeBlock.TimeDeviationCauseStop.TimeDeviationCauseId;
                }
            }
        }

        private void SetTimeBlockRelations(int employeeId, List<TimeBlock> timeBlocks, int timeBlockDateId, int? timeScheduleTemplatePeriodId)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            bool preliminary = IsAnyTimeBlockPreliminary(timeBlocks);

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                SetTimeBlockRelations(timeBlock, preliminary, employeeId, timeBlockDateId, timeScheduleTemplatePeriodId);
            }
        }

        private void SetTimeBlockRelations(TimeBlock timeBlock, bool preliminary, int employeeId, int timeBlockDateId, int? timeScheduleTemplatePeriodId)
        {
            if (timeBlock == null)
                return;

            timeBlock.GuidId = Guid.NewGuid();
            if (employeeId > 0 && timeBlock.EmployeeId == 0)
                timeBlock.EmployeeId = employeeId;
            if (timeBlockDateId > 0 && timeBlock.TimeBlockDateId == 0)
                timeBlock.TimeBlockDateId = timeBlockDateId;
            if (timeScheduleTemplatePeriodId > 0 && (!timeBlock.TimeScheduleTemplatePeriodId.HasValue || timeBlock.TimeScheduleTemplatePeriodId.Value == 0))
                timeBlock.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
            if (preliminary && timeBlock.IsBreak)
                SetTimeBlockToPreliminary(timeBlock);
        }

        private void SetCommentOnTimeBlockFromTimeStamps(List<TimeBlock> timeBlocks)
        {
            if (timeBlocks == null)
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                SetCommentOnTimeBlockFromTimeStamp(timeBlock);
            }
        }

        private void SetCommentOnTimeBlockFromTimeStamp(TimeBlock timeBlock)
        {
            if (timeBlock == null || timeBlock.TimeStampEntry == null)
                return;

            foreach (TimeStampEntry entry in timeBlock.TimeStampEntry)
            {
                timeBlock.Comment = StringUtility.Merge(timeBlock.Comment, entry.Note);
            }
        }

        private void SetTimeCodeOnTimeBlocksFromTransactions(List<TimeBlock> timeBlocks, List<TimeCodeTransaction> timeCodeTransactions)
        {
            if (timeBlocks.IsNullOrEmpty() || timeCodeTransactions.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in timeBlocks.Where(i => i.TimeCode != null))
            {
                //Only set if missing
                if (timeBlock.TimeCode.Any() || !base.IsEntityAvailableInContext(entities, timeBlock))
                    continue;

                TimeCodeTransaction timeCodeTransaction = timeCodeTransactions.FirstOrDefault(i => i.GuidTimeBlock == timeBlock.GuidId);
                if (timeCodeTransaction == null)
                    continue;

                TimeCode timeCode = GetTimeCodeFromCache(timeCodeTransaction.TimeCodeId);
                if (timeCode == null)
                    continue;

                timeBlock.TimeCode.Add(timeCode);
            }
        }

        private void SetTimeCodeIdsOnTimeBlocksFromTransactions(List<TimeBlock> timeBlocks, List<TimeCodeTransaction> timeCodeTransactions)
        {
            if (timeBlocks.IsNullOrEmpty() || timeCodeTransactions.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                timeBlock.TransactionTimeCodeIds = new List<int>();

                List<TimeCodeTransaction> timeCodeTransactionsForTimeBlock = timeCodeTransactions.Where(tc => tc.GuidTimeBlock.HasValue && timeBlock.GuidId.HasValue && tc.GuidTimeBlock.Value == timeBlock.GuidId.Value).ToList();
                foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactionsForTimeBlock)
                {
                    if (!timeBlock.TransactionTimeCodeIds.Contains(timeCodeTransaction.TimeCodeId))
                        timeBlock.TransactionTimeCodeIds.Add(timeCodeTransaction.TimeCodeId);
                }
            }
        }

        private void FixTimeBlockTimes(TimeBlock timeBlock)
        {
            //Fix negative times
            if (timeBlock.StopTime < timeBlock.StartTime)
            {
                timeBlock.StopTime = timeBlock.StartTime;
                SetTimeBlockAndTransactionsToDeleted(timeBlock, saveChanges: false);
            }
        }

        private bool IsManuallyAdjusted(int employeeId, int timeBlockDateId, int? templatePeriodId)
        {
            List<TimeBlock> timeBlocks = GetTimeBlocks(employeeId, timeBlockDateId, templatePeriodId);
            return timeBlocks.Any(i => i.ManuallyAdjusted);
        }

        private bool IsAnyTimeBlockAbsence(List<TimeBlock> timeBlocks)
        {
            if (timeBlocks.IsNullOrEmpty())
                return false;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                DecideTimeBlockType(timeBlock);

                if (timeBlock.IsAbsence())
                    return true;
            }

            return false;
        }

        private bool IsAnyTimeBlockPreliminary(List<TimeBlock> timeBlocks)
        {
            return UseStaffing() && timeBlocks != null && timeBlocks.Any(i => i.IsPreliminary);
        }

        private bool IsAnyAbsenceTimeBlockOverlappingPresenceTimeBlock(List<TimeBlock> absenceTimeBlocks, TimeBlock presenceTimeBlock)
        {
            if (absenceTimeBlocks.IsNullOrEmpty() || presenceTimeBlock == null)
                return false;

            foreach (TimeBlock absenceTimeBlock in absenceTimeBlocks)
            {
                if (IsNewTimeBlockOverlappedByCurrentTimeBlock(presenceTimeBlock, absenceTimeBlock))
                    return true;
            }

            return false;
        }

        private bool IsNewTimeBlockOverlappedByCurrentTimeBlock(TimeBlock newTimeBlock, TimeBlock currentTimeBlock)
        {
            return CalendarUtility.IsNewOverlappedByCurrent(newTimeBlock.StartTime, newTimeBlock.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime);
        }

        private bool IsCurrentTimeBlockOverlappedByNewTimeBlock(TimeBlock newTimeBlock, TimeBlock currentTimeBlock)
        {
            return CalendarUtility.IsCurrentOverlappedByNew(newTimeBlock.StartTime, newTimeBlock.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime);
        }

        private bool IsNewTimeBlockStopInCurrentTimeBlock(TimeBlock newTimeBlock, TimeBlock currentTimeBlock)
        {
            return CalendarUtility.IsNewStopInCurrent(newTimeBlock.StartTime, newTimeBlock.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime);
        }

        private bool IsNewTimeBlockStartInCurrentTimeBlock(TimeBlock newTimeBlock, TimeBlock currentTimeBlock)
        {
            return CalendarUtility.IsNewStartInCurrent(newTimeBlock.StartTime, newTimeBlock.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime);
        }

        private DateTime GetLatestDateForTimeBlock(int employeeId)
        {
            return (from tbd in entities.TimeBlockDate
                    where tbd.EmployeeId == employeeId &&
                    tbd.TimeBlock.Any(tb => tb.State == (int)SoeEntityState.Active)
                    orderby tbd.Date descending
                    select tbd.Date).FirstOrDefault();
        }

        #endregion

        #region TimeBlockDate

        private List<TimeBlockDate> GetTimeBlockDatesForPeriod(int employeeId, int timePeriodId)
        {
            TimePeriod timePeriod = GetTimePeriod(timePeriodId);
            if (timePeriod == null)
                return new List<TimeBlockDate>();

            return GetTimeBlockDates(employeeId, timePeriod.StartDate, timePeriod.StopDate);
        }

        private List<TimeBlockDate> GetTimeBlockDates(int employeeId, DateTime startDate, DateTime stopDate)
        {
            return (from tbd in entities.TimeBlockDate
                    where tbd.EmployeeId == employeeId &&
                    tbd.Date >= startDate &&
                    tbd.Date <= stopDate
                    select tbd).ToList();
        }

        private List<TimeBlockDate> GetTimeBlockDates(int employeeId, List<int> timeBlockDateIds)
        {
            return (from tbd in entities.TimeBlockDate
                    where tbd.EmployeeId == employeeId &&
                    timeBlockDateIds.Contains(tbd.TimeBlockDateId)
                    select tbd).ToList();
        }

        private List<TimeBlockDate> GetTimeBlockDates(int employeeId, List<DateTime> dates)
        {
            return (from tbd in entities.TimeBlockDate
                    where tbd.EmployeeId == employeeId &&
                    dates.Contains(tbd.Date)
                    select tbd).ToList();
        }

        private List<TimeBlockDate> GetTimeBlockDatesWithDetails(int employeeId, List<int> timeBlockDateIds)
        {
            return (from tbd in entities.TimeBlockDate
                        .Include("TimeBlockDateDetail")
                    where tbd.EmployeeId == employeeId &&
                    timeBlockDateIds.Contains(tbd.TimeBlockDateId)
                    select tbd).ToList();
        }

        private TimeBlockDate GetTimeBlockDate(int employeeId, DateTime date, bool createIfNotExists = false)
        {
            return TimeBlockManager.GetTimeBlockDate(entities, actorCompanyId, employeeId, date, createIfNotExists);
        }

        private TimeBlockDate GetTimeBlockDate(int timeBlockDateId, int employeeId)
        {
            return TimeBlockManager.GetTimeBlockDate(entities, timeBlockDateId, employeeId);
        }

        private TimeBlockDate GetTimeBlockDateFromTimeBlock(int timeBlockId)
        {
            return TimeBlockManager.GetTimeBlockDateFromTimeBlock(entities, timeBlockId);
        }

        private TimeBlockDate CreateTimeBlockDate(int employeeId, DateTime date)
        {
            TimeBlockDate timeBlockDate = TimeBlockManager.CreateTimeBlockDate(entities, date.Date, employeeId, this.actorCompanyId, task: base.currentTask);
            if (timeBlockDate != null)
                AddTimeBlockDatesToCache(employeeId, timeBlockDate.ObjToList());
            return timeBlockDate;
        }

        private DateTime GetActualDateTime(DateTime time, DateTime date)
        {
            DateTime tempDate = date.AddDays((time.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days);
            return new DateTime(tempDate.Date.Year, tempDate.Date.Month, tempDate.Date.Day, time.Hour, time.Minute, time.Second);
        }

        private int? GetNearestTimeScheduleTemplatePeriodIdForSpecificDayForTimeBlock(int employeeId, DateTime specificDate, TimeBlock timeBlockToPlace)
        {
            int periodId = 0;

            if (!HasMultipleScheduleTemplatePeriodsOnSameDay(employeeId, specificDate, out List<int> periodIds))
            {
                periodId = periodIds.FirstOrDefault();
                return periodId;
            }

            List<TimeScheduleTemplateBlock> templateBlocks = GetScheduleBlocksFromCache(employeeId, specificDate.Date);

            if (templateBlocks != null && templateBlocks.Count > 0)
            {
                #region Find a scheduleblock that spans over timeBlockToPlace

                DateTime actualStart = GetActualDateTime(timeBlockToPlace.StartTime, specificDate);
                DateTime actualStop = GetActualDateTime(timeBlockToPlace.StopTime, specificDate);

                foreach (var templateBlock in templateBlocks)
                {
                    DateTime templateBlockStart = GetActualDateTime(templateBlock.StartTime, specificDate);
                    DateTime templateBlockStop = GetActualDateTime(templateBlock.StopTime, specificDate);

                    if (actualStart >= templateBlockStart && actualStop <= templateBlockStop)
                        return templateBlock.TimeScheduleTemplatePeriodId;
                }

                #endregion

                #region Find out if block is nearest schemaIn or SchemaOut

                DateTime startTime = timeBlockToPlace.StartTime;
                DateTime stopTime = timeBlockToPlace.StopTime;
                int intervalminutes = CalendarUtility.TimeSpanToMinutes(stopTime, startTime);
                DateTime middleOfBlock = startTime.AddMinutes((double)intervalminutes / 2);
                DateTime scheduleInChooseDate = templateBlocks.GetScheduleIn(out int _);
                DateTime scheduleOutChooseDate = templateBlocks.GetScheduleOut(out int _);

                int minutesFromMiddleOfBlockToChoosenDateScheduleIn = CalendarUtility.TimeSpanToMinutesStartAndStopUnknown(GetActualDateTime(scheduleInChooseDate, specificDate.Date), GetActualDateTime(middleOfBlock, specificDate.Date));
                int minutesFromMiddleOfBlockToChoosenDateScheduleOut = CalendarUtility.TimeSpanToMinutesStartAndStopUnknown(GetActualDateTime(scheduleOutChooseDate, specificDate.Date), GetActualDateTime(middleOfBlock, specificDate.Date));
                bool choosenDatescheduleInIsNearest = minutesFromMiddleOfBlockToChoosenDateScheduleIn < minutesFromMiddleOfBlockToChoosenDateScheduleOut;

                #endregion

                if (choosenDatescheduleInIsNearest)
                {
                    #region Decide which templateblocks starttime is nearest to blocks stoptime

                    templateBlocks.GetScheduleIn(out periodId);

                    #endregion
                }
                else
                {
                    #region Decide which templateblocks stoptime is nearest to blocks starttime

                    templateBlocks.GetScheduleOut(out periodId);

                    #endregion
                }

            }

            if (periodId > 0)
                return periodId;

            return GetTimeScheduleTemplatePeriodIdFromCache(employeeId, specificDate);
        }

        private bool IsTimeStampStatusValid(EmployeeGroup employeeGroup, int stampingStatus, AttestStateDTO attestStateTo)
        {
            return TimeStampManager.IsTimeStampStatusValid(employeeGroup, stampingStatus, attestStateTo);
        }

        private ActionResult ValidateLockedDay(int employeeId, DateTime date)
        {
            return ValidateLockedDays(employeeId, new List<DateTime>() { date });
        }

        private ActionResult ValidateLockedDays(int employeeId, List<DateTime> dates)
        {
            if (dates.IsNullOrEmpty())
                return new ActionResult(true);

            List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesFromCache(employeeId, dates);
            if (timeBlockDates.Any(x => x.IsLocked))
                return new ActionResult((int)ActionResultSave.InsufficientInput, GetText(10117, "Otillåten ändring") + ": " + GetText(8926, "En eller flera dagar är låsta och kan ej behandlas."));

            return new ActionResult(true);
        }

        #endregion

        #region TimeBlockDetail

        private decimal? GetTimeBlockDateDetailRatioFromDeviationCause(TimeBlockDate timeBlockDate, int timeDeviationCauseId)
        {
            if (timeBlockDate == null)
                return null;

            if (!timeBlockDate.TimeBlockDateDetail.IsLoaded)
                timeBlockDate.TimeBlockDateDetail.Load();

            return timeBlockDate.TimeBlockDateDetail.FirstOrDefault(t => t.State == (int)SoeEntityState.Active && t.RecordId == timeDeviationCauseId)?.Ratio;
        }

        private void CreateTimeBlockDetailOutcome(TimeBlockDate timeBlockDate, List<ApplyAbsenceDTO> absenceDays)
        {
            if (absenceDays.IsNullOrEmpty())
                return;

            DateTime batchTimeStamp = DateTime.Now;
            foreach (ApplyAbsenceDTO absenceDay in absenceDays.Where(d => d.Date == timeBlockDate.Date))
            {
                CreateTimeBlockDetail(timeBlockDate, SoeTimeBlockDateDetailType.Absence, null, absenceDay.SysPayrollTypeLevel3, null, false, batchTimeStamp);
            }
        }

        private void CreateTimeBlockDetailOutcome(TimeBlockDate timeBlockDate, List<TimeEngineAbsenceDay> absenceDays)
        {
            if (absenceDays.IsNullOrEmpty())
                return;

            DateTime batchTimeStamp = DateTime.Now;
            foreach (TimeEngineAbsenceDay absenceDay in absenceDays.Where(d => d.Date == timeBlockDate.Date && d.SysPayrollTypeLevel3.HasValue))
            {
                CreateTimeBlockDetail(timeBlockDate, SoeTimeBlockDateDetailType.Absence, absenceDay.TimeDeviationCauseId, absenceDay.SysPayrollTypeLevel3.Value, absenceDay.Ratio, false, batchTimeStamp);
            }
        }

        private void CreateTimeBlockDetail(TimeBlockDate timeBlockDate, SoeTimeBlockDateDetailType type, int? recordId, int? outcomeId, decimal? ratio, bool manuallyAdjusted, DateTime? created = null, string createdBy = null)
        {
            if (timeBlockDate == null)
                return;
            if (!ratio.HasValue && !manuallyAdjusted && outcomeId.HasValue && timeBlockDate.ContainsOutcomeWithRatio(type, outcomeId.Value))
                return;

            if (SetTimeBlockDateDetailsToDeleted(timeBlockDate, type, outcomeId, modified: created, modifiedBy: createdBy, saveChanges: false).Success)
            {
                TimeBlockDateDetail timeBlockDetail = new TimeBlockDateDetail
                {
                    Type = (int)type,
                    RecordId = recordId ?? 0,
                    OutcomeId = outcomeId ?? 0,
                    Ratio = ratio,
                    ManuallyAdjusted = manuallyAdjusted,
                    State = (int)SoeEntityState.Active,

                    //Set references
                    TimeBlockDate = timeBlockDate,
                };
                SetCreatedProperties(timeBlockDetail);
                if (created.HasValue)
                    timeBlockDetail.Created = created.Value;
                if (!createdBy.IsNullOrEmpty())
                    timeBlockDetail.CreatedBy = createdBy;
            }
        }

        #endregion

        #region TimeBreakTemplate

        private List<TimeBreakTemplate> GetTimeBreakTemplatesWithShiftTypesDayTypesAndRows(DateTime date)
        {
            return (from bt in entities.TimeBreakTemplate
                    .Include("ShiftTypes")
                    .Include("DayTypes")
                    .Include("TimeBreakTemplateRow.TimeCodeBreakGroup")
                    where bt.ActorCompanyId == actorCompanyId &&
                    (!bt.StartDate.HasValue || bt.StartDate.Value <= date) &&
                    (!bt.StopDate.HasValue || bt.StopDate.Value >= date) &&
                    bt.State == (int)SoeEntityState.Active
                    select bt).ToList();
        }

        #endregion

        #region TimeCode

        private (List<TimeCode> ToIncrease, List<TimeCode> ToDecrease) GetTimeCodesValidForAdjustment(List<TimeCodeTransaction> timeCodeTransactions)
        {
            List<TimeCode> timeCodesToIncrease = new List<TimeCode>();
            List<TimeCode> timeCodesToDecrease = new List<TimeCode>();

            if (!timeCodeTransactions.IsNullOrEmpty())
            {
                foreach (var timeCodeTransactionsByTimeCode in timeCodeTransactions.GroupBy(i => i.TimeCodeId))
                {
                    TimeCode timeCode = timeCodeTransactionsByTimeCode.FirstOrDefault()?.TimeCode ?? GetTimeCodeFromCache(timeCodeTransactionsByTimeCode.Key);
                    if (timeCode == null || timeCode.AdjustQuantityByBreakTime == (int)TermGroup_AdjustQuantityByBreakTime.None)
                        continue;

                    if (timeCode.AdjustQuantityByBreakTime == (int)TermGroup_AdjustQuantityByBreakTime.Add)
                        timeCodesToIncrease.Add(timeCode);
                    else if (timeCode.AdjustQuantityByBreakTime == (int)TermGroup_AdjustQuantityByBreakTime.Remove)
                        timeCodesToDecrease.Add(timeCode);
                }
            }

            return (timeCodesToIncrease, timeCodesToDecrease);
        }

        private TimeCode GetTimeCode(int timeCodeId)
        {
            return (from tc in entities.TimeCode
                    where tc.TimeCodeId == timeCodeId &&
                    tc.State == (int)SoeEntityState.Active
                    select tc).FirstOrDefault<TimeCode>();
        }

        private TimeCode GetTimeCodeDiscardState(int timeCodeId)
        {
            return (from tc in entities.TimeCode
                    where tc.TimeCodeId == timeCodeId
                    select tc).FirstOrDefault<TimeCode>();
        }

        private TimeCode GetTimeCodeWithRules(int timeCodeId)
        {
            return (from tc in entities.TimeCode
                        .Include("TimeCodeRule")
                    where tc.TimeCodeId == timeCodeId &&
                    tc.State == (int)SoeEntityState.Active
                    select tc).FirstOrDefault<TimeCode>();
        }

        private TimeCode GetTimeCodeWithProducts(int timeCodeId)
        {
            return (from tc in entities.TimeCode
                        .Include("TimeCodeInvoiceProduct.InvoiceProduct")
                        .Include("TimeCodePayrollProduct.PayrollProduct")
                    where tc.TimeCodeId == timeCodeId
                    select tc).FirstOrDefault<TimeCode>();
        }

        private TimeCode GetTimeCodeFromDeviationCause(TimeDeviationCause timeDeviationCause, int defaultTimeCodeId)
        {
            if (timeDeviationCause == null)
                return null;
            if (timeDeviationCause.TimeCode != null)
                return timeDeviationCause.TimeCode;
            return GetTimeCodeFromCache(timeDeviationCause.TimeCodeId ?? defaultTimeCodeId);
        }

        private List<int> GetTimeCodesGeneratedByTimeRules(DateTime date, DateTime startTime, DateTime stopTime, SoeTimeRuleType timeRuleType, int timeDeviationCauseId, Employee employee, EmployeeGroup employeeGroup, List<TimeScheduleTemplateBlock> scheduleBlocks = null)
        {
            List<int> timeCodeIds = new List<int>();

            if (employee == null || employeeGroup == null)
                return timeCodeIds;

            DayType dayType = GetDayTypeForEmployeeFromCache(employee.EmployeeId, date);
            if (dayType == null)
                return timeCodeIds;

            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesFromCache();
            TimeDeviationCause timeDeviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == timeDeviationCauseId);
            List<int> timeDeviationCauseIdsOvertime = timeDeviationCauses.GetOvertimeDeviationCauseIds();
            List<int> timeScheduleTypeIdsIsNotScheduleTime = GetTimeScheduleTypeIdsIsNotScheduleTimeFromCache();
            scheduleBlocks = scheduleBlocks ?? GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, employee.EmployeeId, date);

            List<TimeScheduleTemplateBlock> scheduleBlocksForOvertimePeriod = null;
            List<TimeBlock> presenceTimeBlockForOvertimePeriod = null;

            TimeEngineRuleEvaluatorProgress progress = new TimeEngineRuleEvaluatorProgress(
                TimeRuleManager.GetTimeRulesFromCache(entities, actorCompanyId).Filter(date: date, timeDeviationCauseId: timeDeviationCauseId, dayTypeId: dayType.DayTypeId, employeeGroupId: employeeGroup.EmployeeGroupId, timeScheduleTypeId: null),
                doScheduleContansStandby: scheduleBlocks.ContainsStandby()
            );

            List<TimeRule> timeRulesForType = progress.GetTimeRules(timeRuleType, scheduleBlocks.ContainsStandby());
            if (!timeRulesForType.IsNullOrEmpty())
            {
                TimeChunk inputTimeChunk = new TimeChunk(startTime, stopTime);
                foreach (TimeRule timeRule in timeRulesForType.OrderBy(i => i.Sort))
                {
                    if (timeRule.Internal && timeDeviationCause?.TimeCodeId != timeRule.TimeCodeId)
                        continue;

                    TimeCode timeCode = GetTimeCodeFromCache(timeRule.TimeCodeId);
                    if (timeCode == null || timeCode.State != (int)SoeEntityState.Active)
                        continue;

                    List<TimeChunk> timeChunks = EvaluateRule(inputTimeChunk, date, employee, employeeGroup, timeRule, timeCode, timeDeviationCauseIdsOvertime, timeScheduleTypeIdsIsNotScheduleTime, scheduleBlocks, ref scheduleBlocksForOvertimePeriod, null, ref presenceTimeBlockForOvertimePeriod);
                    progress.SetRuleEvaluated(timeRule, timeChunks);

                    if (timeChunks.Any() && !timeCodeIds.Contains(timeRule.TimeCodeId))
                        timeCodeIds.Add(timeRule.TimeCodeId);
                }
            }

            return timeCodeIds;
        }

        private (TimeCode, PayrollProduct) GetVacationGroupReplacementTimeCodeAndProduct(VacationGroupDTO vacationGroup)
        {
            if (vacationGroup != null && vacationGroup.VacationGroupSE != null && vacationGroup.VacationGroupSE.ReplacementTimeDeviationCauseId.HasValue)
            {
                TimeDeviationCause replacementTimeDeviationCause = GetTimeDeviationCauseFromCache(vacationGroup.VacationGroupSE.ReplacementTimeDeviationCauseId.Value);
                if (replacementTimeDeviationCause != null && replacementTimeDeviationCause.TimeCodeId.HasValue)
                {
                    TimeCode replacementTimeCode = GetTimeCodeWithProductsFromCache(replacementTimeDeviationCause.TimeCodeId.Value);
                    if (replacementTimeCode != null && replacementTimeCode.TimeCodePayrollProduct.Any())
                    {
                        PayrollProduct replacementPayrollProduct = replacementTimeCode.TimeCodePayrollProduct.First().PayrollProduct;
                        if (replacementPayrollProduct != null)
                            return (replacementTimeCode, replacementPayrollProduct);
                    }
                }
            }
            return (null, null);
        }

        private bool IsTimeCodesEqual(IEnumerable<TimeCode> timeCodes1, IEnumerable<TimeCode> timeCodes2)
        {
            if (timeCodes1 == null && timeCodes2 == null)
                return true; //Both null
            if (timeCodes1 == null || timeCodes2 == null)
                return false; //One null
            if (timeCodes1.Count() != timeCodes2.Count())
                return false; //Different size

            //Each account in accountInternals2 must be in accountInternals1
            foreach (TimeCode timeCode in timeCodes1)
            {
                if (!timeCodes2.Any(tc => tc.TimeCodeId == timeCode.TimeCodeId))
                    return false;
            }

            return true;
        }

        private bool IsTimeCodeOfType(SoeTimeCodeType type, params TimeCode[] timeCodes)
        {
            foreach (TimeCode timeCode in timeCodes)
            {
                if (timeCode.Type != (int)type)
                    return false;
            }

            return true;
        }

        #endregion

        #region TimeCodeRanking

        public bool UseTimeCodeRanking()
        {
            return entities.TimeCodeRankingGroup.Any(tcrg => tcrg.ActorCompanyId == this.actorCompanyId && tcrg.State == (int)SoeEntityState.Active);
        }

        public TimeCodeRankingGroup GetTimeCodeRankingGroupWithRankings(DateTime date)
        {
            return (from tcrg in entities.TimeCodeRankingGroup
                        .Include("TimeCodeRanking")
                    where tcrg.ActorCompanyId == this.actorCompanyId &&
                    tcrg.StartDate <= date &&
                    (!tcrg.StopDate.HasValue || tcrg.StopDate.Value >= date) &&
                    tcrg.State == (int)SoeEntityState.Active
                    select tcrg).FirstOrDefault();
        }

        #endregion

        #region TimeCodeBreak

        private TimeCodeBreak GetTimeCodeBreak(int timeCodeId)
        {
            return (from tc in entities.TimeCode.OfType<TimeCodeBreak>()
                    where tc.TimeCodeId == timeCodeId &&
                    tc.State == (int)SoeEntityState.Active
                    select tc).FirstOrDefault<TimeCodeBreak>();
        }

        private TimeCodeBreak GetTimeCodeBreakForEmployeeGroup(Employee employee, EmployeePost employeePost, DateTime date, int? timeCodeBreakGroupId)
        {
            TimeCodeBreak timeCodeBreak = null;
            if (timeCodeBreakGroupId.HasValue)
            {
                int employeeGroupId = 0;
                if (employee != null)
                    employeeGroupId = employee.GetEmployeeGroupId(date);
                else if (employeePost != null && employeePost.EmployeeGroupId.HasValue)
                    employeeGroupId = employeePost.EmployeeGroupId.Value;

                if (employeeGroupId > 0)
                    timeCodeBreak = GetTimeCodeBreakForEmployeeGroupFromCache(timeCodeBreakGroupId.Value, employeeGroupId);
            }
            return timeCodeBreak;
        }

        private TimeCodeBreak GetTimeCodeBreakForEmployeeGroup(int timeCodeBreakGroupId, int employeeGroupId)
        {
            return (from g in entities.TimeCode.OfType<TimeCodeBreak>()
                        .Include("EmployeeGroupsForBreak")
                    where g.TimeCodeBreakGroupId == timeCodeBreakGroupId &&
                    g.EmployeeGroupsForBreak.Any(i => i.EmployeeGroupId == employeeGroupId)
                    select g).FirstOrDefault();
        }

        private int GetTimeCodeBreakIdForEmployee(TimeScheduleTemplateBlock templateBlock, int newEmployeeId)
        {
            if (templateBlock == null)
                return 0;

            return GetTimeCodeBreakIdForEmployee(templateBlock.Date, templateBlock.TimeCodeId, newEmployeeId, templateBlock.EmployeeId);
        }

        private int GetTimeCodeBreakIdForEmployee(DateTime? date, int timeCodeId, int newEmployeeId, int? oldEmployeeId = null)
        {
            //Fall back is always existing break
            int newTimeCodeId = timeCodeId;

            if (oldEmployeeId.HasValue && oldEmployeeId.Value != newEmployeeId)
            {
                #region Has old Employee

                //Break must have Date and Employee and Employee must have changed
                if (date.HasValue && oldEmployeeId.Value != newEmployeeId)
                {
                    //TimeCodeBreak must have TimeCodeBreakGroup
                    TimeCodeBreak timeCodeBreak = GetTimeCodeBreakFromCache(newTimeCodeId);
                    if (timeCodeBreak != null && timeCodeBreak.TimeCodeBreakGroupId.HasValue)
                    {
                        //Check new Employee - cannot be Hidden (dont change TimeCodeBreak when "ledigt pass" gets a break, validate it first when next regular employee gets it)
                        Employee newEmployee = GetEmployeeWithContactPersonAndEmploymentFromCache(newEmployeeId, getHidden: false);
                        if (newEmployee != null)
                        {
                            //Check old Employee - can be hidden
                            Employee oldEmployee = GetEmployeeWithContactPersonAndEmploymentFromCache(oldEmployeeId.Value, getHidden: true);
                            if (oldEmployee != null)
                            {
                                //New Employee must have EmployeeGroupId
                                int newEmployeeGroupId = newEmployee.GetEmployeeGroupId(date);
                                if (newEmployeeGroupId > 0)
                                {
                                    //EmployeeGroup must have changed
                                    int oldEmployeeGroupId = oldEmployee.GetEmployeeGroupId(date);
                                    if (oldEmployeeGroupId != newEmployeeGroupId || oldEmployee.Hidden)
                                    {
                                        TimeCodeBreak timeCodeBreakForEmployeeGroup = GetTimeCodeBreakForEmployeeGroupFromCache(timeCodeBreak.TimeCodeBreakGroupId.Value, newEmployee.GetEmployeeGroupId());
                                        if (timeCodeBreakForEmployeeGroup != null)
                                            newTimeCodeId = timeCodeBreakForEmployeeGroup.TimeCodeId;

                                    }
                                }
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region Has NOT old Employee

                //TimeCodeBreak must have TimeCodeBreakGroup
                TimeCodeBreak timeCodeBreak = GetTimeCodeBreakFromCache(newTimeCodeId);
                if (timeCodeBreak != null && timeCodeBreak.TimeCodeBreakGroupId.HasValue)
                {
                    //Check new Employee - cannot be Hidden (dont change TimeCodeBreak when "ledigt pass" gets a break, validate it first when next regular employee gets it)
                    Employee newEmployee = GetEmployeeWithContactPersonAndEmploymentFromCache(newEmployeeId, getHidden: false);
                    if (newEmployee != null)
                    {
                        //New Employee must have EmployeeGroupId
                        int newEmployeeGroupId = newEmployee.GetEmployeeGroupId(date);
                        if (newEmployeeGroupId > 0)
                        {
                            TimeCodeBreak timeCodeBreakForEmployeeGroup = GetTimeCodeBreakForEmployeeGroupFromCache(timeCodeBreak.TimeCodeBreakGroupId.Value, newEmployeeGroupId);
                            if (timeCodeBreakForEmployeeGroup != null)
                                newTimeCodeId = timeCodeBreakForEmployeeGroup.TimeCodeId;
                        }
                    }
                }

                #endregion
            }

            return newTimeCodeId;
        }

        private ActionResult HasEmployeeValidTimeCodeBreak(DateTime date, int timeCodeId, int employeeId)
        {
            int newTimeCodeId = GetTimeCodeBreakIdForEmployee(date, timeCodeId, employeeId);
            return new ActionResult(newTimeCodeId != timeCodeId);
        }

        #endregion

        #region TimeCodeBreakGroup

        private List<TimeCodeBreakGroup> GetTimeCodeBreakGroups()
        {
            return (from g in entities.TimeCodeBreakGroup
                    where g.ActorCompanyId == this.actorCompanyId &&
                    g.State == (int)SoeEntityState.Active
                    select g).ToList();
        }

        #endregion

        #region TimeDeviationCause

        private List<TimeDeviationCause> GetTimeDeviationCauses()
        {
            return (from t in entities.TimeDeviationCause
                    where t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).OrderBy(t => t.Name).ToList();
        }

        private TimeDeviationCause GetTimeDeviationCause(int timeDeviationCauseId)
        {
            return (from t in entities.TimeDeviationCause
                    where t.ActorCompanyId == actorCompanyId &&
                    t.TimeDeviationCauseId == timeDeviationCauseId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimeDeviationCause GetTimeDeviationCauseWithTimeCode(int timeDeviationCauseId)
        {
            return (from t in entities.TimeDeviationCause
                        .Include("TimeCode")
                    where t.TimeDeviationCauseId == timeDeviationCauseId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        public List<TimeDeviationCause> GetTimeDeviationCausesByEmployeeGroup(int employeeGroupId, bool onlyUseInTimeTerminal)
        {
            return (from t in entities.TimeDeviationCause.Include("EmployeeGroupTimeDeviationCause")
                    where t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active &&
                    (t.EmployeeGroupTimeDeviationCause.Any(e => e.EmployeeGroupId == employeeGroupId && e.State == (int)SoeEntityState.Active && (!onlyUseInTimeTerminal || e.UseInTimeTerminal)))
                    select t).ToList();
        }

        private TimeDeviationCause GetTimeDeviationCauseFromPrio(Employee employee, EmployeeGroup employeeGroup, TimeStampEntry timeStampEntry)
        {
            // Try get from TimeStampEntry
            if (timeStampEntry != null && timeStampEntry.TimeDeviationCauseId.HasValue)
            {
                if (!timeStampEntry.TimeDeviationCauseReference.IsLoaded)
                    timeStampEntry.TimeDeviationCauseReference.Load();
                if (timeStampEntry.TimeDeviationCause != null)
                    return timeStampEntry.TimeDeviationCause;
            }

            //Try get from Employee
            if (employee != null && employee.TimeDeviationCauseId.HasValue)
            {
                if (!employee.TimeDeviationCauseReference.IsLoaded)
                    employee.TimeDeviationCauseReference.Load();
                if (employee.TimeDeviationCause != null)
                    return employee.TimeDeviationCause;
            }

            //Try get from EmployeeGroup
            if (employeeGroup != null && employeeGroup.TimeDeviationCauseId.HasValue)
            {
                if (!employeeGroup.TimeDeviationCauseReference.IsLoaded)
                    employeeGroup.TimeDeviationCauseReference.Load();
                if (employeeGroup.TimeDeviationCause != null)
                    return employeeGroup.TimeDeviationCause;
            }

            return null;
        }

        private int GetTimeDeviationCauseIdFromPrio(Employee employee, EmployeeGroup employeeGroup, TimeBlock timeBlock, bool useStartCause)
        {
            int timeDeviationCauseId = 0;

            if (timeBlock != null)
            {
                //Try get from TimeBlock
                if (useStartCause && timeBlock.TimeDeviationCauseStartId.HasValue)
                    timeDeviationCauseId = timeBlock.TimeDeviationCauseStartId.Value;
                else if (!useStartCause && timeBlock.TimeDeviationCauseStopId.HasValue)
                    timeDeviationCauseId = timeBlock.TimeDeviationCauseStopId.Value;
            }

            //Try get from Employee
            if (timeDeviationCauseId == 0 && employee != null && employee.TimeDeviationCauseId.HasValue)
                timeDeviationCauseId = employee.TimeDeviationCauseId.Value;

            //Try get from EmployeeGroup
            if (timeDeviationCauseId == 0 && employeeGroup != null && employeeGroup.TimeDeviationCauseId.HasValue)
                timeDeviationCauseId = employeeGroup.TimeDeviationCauseId.Value;

            return timeDeviationCauseId;
        }

        private bool IsTimeDeviationCauseOfType(TermGroup_TimeDeviationCauseType type, params TimeDeviationCause[] timeDeviationCauses)
        {
            return IsTimeDeviationCauseOfAnyType(type.ObjToList(), timeDeviationCauses);
        }

        private bool IsTimeDeviationCauseOfAnyType(List<TermGroup_TimeDeviationCauseType> types, params TimeDeviationCause[] timeDeviationCauses)
        {
            foreach (TimeDeviationCause timeDeviationCause in timeDeviationCauses)
            {
                if (timeDeviationCause == null || !types.Contains((TermGroup_TimeDeviationCauseType)timeDeviationCause.Type))
                    return false;
            }

            return true;
        }

        #endregion

        #region TimeHalfDay

        private TimeHalfday GetTimeHalfdayWithDayTypeAndHoliday(int timeHalfdayId)
        {
            return (from hd in entities.TimeHalfday
                        .Include("DayType.Holiday")
                    where hd.TimeHalfdayId == timeHalfdayId &&
                    hd.DayType.ActorCompanyId == actorCompanyId
                    select hd).FirstOrDefault();
        }

        private TimeHalfdayDTO GetTimeHalfday(List<TimeScheduleTemplateBlock> templateBlocks, List<HolidayDTO> holidays, DateTime date)
        {
            if (holidays.IsNullOrEmpty() || templateBlocks.IsNullOrEmpty())
                return null;

            // Check if current date is a holiday. Do not use Holiday/TimeHalfday on zero schedules
            TimeScheduleTemplateBlock templateBlock = templateBlocks.FirstOrDefault(i => !i.IsBreak);
            if (templateBlock == null || templateBlock.StopTime.Subtract(templateBlock.StartTime).TotalMinutes == 0)
                return null;

            HolidayDTO holiday = holidays.FirstOrDefault(i => i.Date.Date == date.Date);
            TimeHalfdayDTO timeHalfDay = holiday?.DayType?.TimeHalfdays?.FirstOrDefault(h => h.State == (int)SoeEntityState.Active);

            return timeHalfDay;
        }

        #endregion

        #region TimeCodeTransaction

        private List<TimeCodeTransaction> GetTimeCodeTransactions(List<int> timeBlockDateIds)
        {
            return (from t in entities.TimeCodeTransaction
                        .Include(t => t.TimeBlockDate)
                    where t.TimeBlockDateId.HasValue &&
                    timeBlockDateIds.Contains(t.TimeBlockDateId.Value) &&
                    t.Type == (int)TimeCodeTransactionType.Time &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimeCodeTransaction> GetTimeCodeTransactionsWithEarningAccumulator(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return (from t in entities.TimeCodeTransaction
                        .Include(t => t.TimeBlockDate)
                    where  t.TimeBlockDateId.HasValue &&
                    t.TimeBlockDate.EmployeeId == employeeId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    t.EarningTimeAccumulatorId.HasValue &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimeCodeTransaction> GetTimeCodeTransactionsDiscardState(int timeBlockDateId)
        {
            return entities.TimeCodeTransaction.Where(tct => tct.TimeBlockDateId == timeBlockDateId).ToList();
        }

        private TimeCodeTransaction GetTimeCodeTransaction(int timeCodeTransactionId, bool onlyActive)
        {
            //Do not check state in query against db. If the TimeCodeTransaction.State is set to 2 and saved, then set to 0 but not saved, EF doesnt seem to recognize that State is 0 in the query
            TimeCodeTransaction timeCodeTransaction = (from t in entities.TimeCodeTransaction
                                                       where t.TimeCodeTransactionId == timeCodeTransactionId
                                                       select t).FirstOrDefault();

            if (timeCodeTransaction != null && timeCodeTransaction.State != (int)SoeEntityState.Active && onlyActive)
                timeCodeTransaction = null;

            return timeCodeTransaction;
        }

        private TimeCodeTransaction GetTimeCodeTransactionWithExternalTransactions(int timeCodeTransactionId, bool onlyActive)
        {
            TimeCodeTransaction timeCodeTransaction = (from t in entities.TimeCodeTransaction
                                                        .Include("TimePayrollTransaction")
                                                        .Include("TimeInvoiceTransaction")
                                                       where t.TimeCodeTransactionId == timeCodeTransactionId
                                                       select t).FirstOrDefault();

            if (timeCodeTransaction != null && timeCodeTransaction.State != (int)SoeEntityState.Active && onlyActive)
                timeCodeTransaction = null;

            return timeCodeTransaction;
        }

        private TimeCodeTransaction GetTimeCodeTransactionWithExternalTransactionDiscardedState(TimeBlock timeBlock, TimeCodeTransactionType type, int timeCodeId, int timeRuleId, bool useTimeBlockTimes = false)
        {
            if (timeBlock == null || timeBlock.TimeBlockId == 0 || timeBlock.TimeBlockDateId == 0 || timeBlock.EmployeeId == 0)
                return null;

            List<TimeCodeTransaction> timeCodeTransactions = GetTimeCodeTransactionsForDayDiscardStateFromCache(timeBlock.EmployeeId, timeBlock.TimeBlockDateId);
            TimeCodeTransaction timeCodeTransaction = timeCodeTransactions.GetDiscardedState(timeBlock, type, timeCodeId, timeRuleId, useTimeBlockTimes);
            if (timeCodeTransaction != null)
            {
                if (!timeCodeTransaction.TimeInvoiceTransaction.IsLoaded)
                    timeCodeTransaction.TimeInvoiceTransaction.Load();
                if (!timeCodeTransaction.TimePayrollTransaction.IsLoaded)
                    timeCodeTransaction.TimePayrollTransaction.Load();
            }

            return timeCodeTransaction;
        }

        #endregion

        #region TimeInvoiceTransaction

        private List<TimeInvoiceTransaction> GetTimeInvoiceTransactions(DateTime dateFrom, DateTime dateTo, int employeeId)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from t in entities.TimeInvoiceTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private TimeInvoiceTransaction CopyTimeInvoiceTransaction(TimeInvoiceTransaction prototypeTimeInvoiceTransaction, TimeBlockDate timeBlockDate, int? timeBlockId, int employeeId)
        {
            TimeInvoiceTransaction cloneTimeInvoiceTransaction = new TimeInvoiceTransaction()
            {
                Amount = prototypeTimeInvoiceTransaction.Amount,
                AmountCurrency = prototypeTimeInvoiceTransaction.AmountCurrency,
                VatAmount = prototypeTimeInvoiceTransaction.VatAmount,
                VatAmountCurrency = prototypeTimeInvoiceTransaction.VatAmountCurrency,
                Quantity = prototypeTimeInvoiceTransaction.Quantity,
                InvoiceQuantity = prototypeTimeInvoiceTransaction.InvoiceQuantity,
                Invoice = prototypeTimeInvoiceTransaction.Invoice,
                ManuallyAdded = prototypeTimeInvoiceTransaction.ManuallyAdded,
                Exported = prototypeTimeInvoiceTransaction.Exported,

                //Set FK (from copy)
                ProductId = prototypeTimeInvoiceTransaction.ProductId,
                AccountStdId = prototypeTimeInvoiceTransaction.AccountStdId,
                AttestStateId = prototypeTimeInvoiceTransaction.AttestStateId,

                //Set FK (from identity)
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                TimeBlockId = timeBlockId,

                //Set references
                TimeBlockDate = timeBlockDate, //Don't set FK becuase it can been created but not saved
            };
            SetCreatedProperties(cloneTimeInvoiceTransaction);
            entities.TimeInvoiceTransaction.AddObject(cloneTimeInvoiceTransaction);

            // Acounting
            cloneTimeInvoiceTransaction.AccountStd = prototypeTimeInvoiceTransaction.AccountStd;
            AddAccountInternalsToTimeInvoiceTransaction(cloneTimeInvoiceTransaction, prototypeTimeInvoiceTransaction.AccountInternal);

            return cloneTimeInvoiceTransaction;
        }

        private void SetTimeInvoiceTransactionTimeBlock(TimeInvoiceTransaction timeInvoiceTransaction, TimeCodeTransaction timeCodeTransaction)
        {
            if (timeInvoiceTransaction == null || timeCodeTransaction == null)
                return;

            if (timeCodeTransaction.TimeBlockId.HasValue && timeCodeTransaction.TimeBlockId > 0)
            {
                timeInvoiceTransaction.TimeBlockId = timeCodeTransaction.TimeBlockId;

                if (base.IsEntityAvailableInContext(entities, timeCodeTransaction.TimeBlock) && !timeCodeTransaction.TimeBlockReference.IsLoaded)
                    timeCodeTransaction.TimeBlockReference.Load();
            }
            else if (IsEntityAvailableInContext(entities, timeCodeTransaction.TimeBlock))
            {
                timeInvoiceTransaction.TimeBlock = timeCodeTransaction.TimeBlock;
            }
        }

        private void SetTimeInvoiceTransactionCurrencyAmounts(TimeInvoiceTransaction timeInvoiceTransaction)
        {
            if (timeInvoiceTransaction == null)
                return;

            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timeInvoiceTransaction);
        }

        #endregion

        #region TimeLeisureCode

        public List<TimeLeisureCode> GetTimeLeisureCodes()
        {
            return (from t in entities.TimeLeisureCode
                    .Include("EmployeeGroupTimeLeisureCode.EmployeeGroupTimeLeisureCodeSetting")
                    where t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        #endregion

        #region TimePayrollTransaction

        private List<TimePayrollTransaction> GetTimePayrollTransactions(int employeeId, List<int> timeBlockDateIds, bool onlyUseInPayroll, bool onlyTransactionsWithoutTimeBlocks)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    timeBlockDateIds.Contains(t.TimeBlockDateId) &&
                    (!onlyUseInPayroll || t.PayrollProduct.UseInPayroll) &&
                    (!onlyTransactionsWithoutTimeBlocks || !t.TimeBlockId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactions(int employeeId, int timePeriodId)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimePeriodId == timePeriodId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactions(int employeeId, DateTime dateFrom, DateTime dateTo, bool onlyUseInPayroll = false, bool onlyTransactionsWithoutTimeBlocks = false)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    (!onlyUseInPayroll || t.PayrollProduct.UseInPayroll) &&
                    (!onlyTransactionsWithoutTimeBlocks || !t.TimeBlockId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactions(List<int> employeeIds, DateTime dateFrom, DateTime dateTo, bool onlyIncludedInWorkTimeSummary = false)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from t in entities.TimePayrollTransaction.Include("TimeBlockDate").Include("AttestState")
                    where employeeIds.Contains(t.EmployeeId) &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    (!onlyIncludedInWorkTimeSummary || !t.PayrollProduct.ExcludeInWorkTimeSummary) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactions(int employeeId, List<int> planningPeriodCalculationIds)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.PlanningPeriodCalculationId.HasValue &&
                    planningPeriodCalculationIds.Contains(t.PlanningPeriodCalculationId.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithAttestState(int employeeId, int timeBlockDateId)
        {
            return (from t in entities.TimePayrollTransaction.Include("AttestState")
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    t.TimeBlockDateId == timeBlockDateId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithTimeBlockDate(int employeeId, List<int> timePayrollTransactionIds)
        {
            if (timePayrollTransactionIds.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            return (from t in entities.TimePayrollTransaction
                        .Include("TimeBlockDate")
                    where t.EmployeeId == employeeId &&
                    timePayrollTransactionIds.Contains(t.TimePayrollTransactionId) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsAfterDateWithTimeBlockDate(int employeeId, DateTime date)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include("TimeBlockDate")
                    where t.EmployeeId == employeeId &&
                    t.State == (int)SoeEntityState.Active &&
                    t.TimeBlockDate.Date > date
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithTimeBlockDate(int employeeId, DateTime dateFrom, DateTime dateTo, bool onlyUseInPayroll = false, bool onlyTransactionsWithoutTimeBlocks = false)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from t in entities.TimePayrollTransaction
                    .Include("TimeBlockDate")
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    (!onlyUseInPayroll || t.PayrollProduct.UseInPayroll) &&
                    (!onlyTransactionsWithoutTimeBlocks || !t.TimeBlockId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithAccountInternals(int employeeId, int timePeriodId)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include("AccountInternal.Account")
                    where t.EmployeeId == employeeId &&
                    t.TimePeriodId == timePeriodId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithAccountInternals(int employeeId, DateTime dateFrom, DateTime dateTo, bool onlyUseInPayroll = false, bool onlyTransactionsWithoutTimeBlocks = false)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from t in entities.TimePayrollTransaction
                        .Include("AccountInternal")
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    (!onlyUseInPayroll || t.PayrollProduct.UseInPayroll) &&
                    (!onlyTransactionsWithoutTimeBlocks || !t.TimeBlockId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithAccountInternals(int employeeId, List<int> timeBlockDateIds, bool onlyUseInPayroll, bool onlyTransactionsWithoutTimeBlocks)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include("AccountInternal")
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    timeBlockDateIds.Contains(t.TimeBlockDateId) &&
                    (!onlyUseInPayroll || t.PayrollProduct.UseInPayroll) &&
                    (!onlyTransactionsWithoutTimeBlocks || !t.TimeBlockId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForLevel3WithTimeBlockDate(int employeeId, List<TimeBlockDate> timeBlockDates, int sysPayrollTypeLevel3, EmployeeChild employeeChild = null, bool isReloadForTimeBlockDatesNotCached = false)
        {
            List<int> timeBlockDateIds = timeBlockDates?.Select(i => i.TimeBlockDateId).Distinct().ToList() ?? new List<int>();

            if (employeeChild != null)
            {
                if (sysPayrollTypeLevel3 != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave)
                    return new List<TimePayrollTransaction>();

                if (isReloadForTimeBlockDatesNotCached)
                {
                    return (from tpt in entities.TimePayrollTransaction
                                .Include("TimeBlockDate")
                            where tpt.EmployeeId == employeeId &&
                            timeBlockDateIds.Contains(tpt.TimeBlockDateId) &&
                            tpt.SysPayrollTypeLevel3 == sysPayrollTypeLevel3 &&
                            tpt.EmployeeChildId == employeeChild.EmployeeChildId &&
                            tpt.State == (int)SoeEntityState.Active &&
                            (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                            select tpt).ToList();
                }
                else
                {
                    return (from tpt in entities.TimePayrollTransaction
                                .Include("TimeBlockDate")
                            where tpt.EmployeeId == employeeId &&
                            tpt.SysPayrollTypeLevel3 == sysPayrollTypeLevel3 &&
                            tpt.EmployeeChildId == employeeChild.EmployeeChildId &&
                            tpt.State == (int)SoeEntityState.Active &&
                            (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                            select tpt).ToList();
                }
            }
            else
            {
                if (timeBlockDates.IsNullOrEmpty())
                    return new List<TimePayrollTransaction>();

                if (timeBlockDates?.Count > Constants.LINQMAXCOUNTCONTAINS)
                {
                    timeBlockDates = timeBlockDates.OrderBy(i => i.Date).ToList();
                    DateTime dateFrom = timeBlockDates.First().Date;
                    DateTime dateTo = timeBlockDates.Last().Date;
                    if (dateTo.Subtract(dateFrom).TotalDays < 400)
                    {
                        List<TimePayrollTransaction> timePayrollTransactions =
                            (from tpt in entities.TimePayrollTransaction
                                .Include("TimeBlockDate")
                             where tpt.EmployeeId == employeeId &&
                             tpt.SysPayrollTypeLevel3 == sysPayrollTypeLevel3 &&
                             tpt.State == (int)SoeEntityState.Active &&
                             tpt.TimeBlockDate.Date >= dateFrom &&
                             tpt.TimeBlockDate.Date <= dateTo &&
                             (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                             select tpt).ToList();

                        //Post execution filter
                        return timePayrollTransactions.Where(tpt => timeBlockDateIds.Contains(tpt.TimeBlockDateId)).ToList();
                    }
                }

                return (from tpt in entities.TimePayrollTransaction
                            .Include("TimeBlockDate")
                        where tpt.EmployeeId == employeeId &&
                        tpt.SysPayrollTypeLevel3 == sysPayrollTypeLevel3 &&
                        tpt.State == (int)SoeEntityState.Active &&
                        (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active) &&
                        timeBlockDateIds.Contains(tpt.TimeBlockDateId)
                        select tpt).ToList();
            }
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForLevel3WithTimeBlockDate(int employeeId, DateTime dateTo, int sysPayrollTypeLevel3)
        {
            return (from tpt in entities.TimePayrollTransaction
                        .Include("TimeBlockDate")
                    where tpt.EmployeeId == employeeId &&
                    tpt.SysPayrollTypeLevel3 == sysPayrollTypeLevel3 &&
                    tpt.TimeBlockDate.Date <= dateTo &&
                    tpt.State == (int)SoeEntityState.Active &&
                    (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                    select tpt).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsConnectedToTimeBlock(int employeeId, int timeBlockDateId)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    t.TimeBlockDateId == timeBlockDateId &&
                    t.TimeBlockId.HasValue &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsConnectedToTimeBlock(int employeeId, DateTime date)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId && //Include EmployeeId two times to be sure index is used &&
                    t.TimeBlockDate.Date == date &&
                    t.TimeBlockId.HasValue &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithExtendedAndTimeBlockDate(List<GetTimePayrollTransactionsForEmployee_Result> transactionItems, bool onlyUseInPayroll)
        {
            if (transactionItems == null)
                return new List<TimePayrollTransaction>();

            List<int> ids = transactionItems.Where(i => (!onlyUseInPayroll || i.PayrollProductUseInPayroll)).Select(i => i.TimePayrollTransactionId).ToList();

            return (from t in entities.TimePayrollTransaction
                    .Include("TimePayrollTransactionExtended")
                    .Include("TimeBlockDate")
                    where ids.Contains(t.TimePayrollTransactionId)
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithExtendedAndTimeBlockDate(EmployeeTimePeriod employeeTimePeriod)
        {
            if (employeeTimePeriod == null)
                return new List<TimePayrollTransaction>();

            return (from t in entities.TimePayrollTransaction
                    .Include("TimePayrollTransactionExtended")
                    .Include("TimeBlockDate")
                    where t.EmployeeTimePeriodId == employeeTimePeriod.EmployeeTimePeriodId
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithExtendedAndTimeBlockDate(List<PayrollCalculationProductDTO> payrollCalculationProducts)
        {
            if (payrollCalculationProducts == null)
                return new List<TimePayrollTransaction>();

            var timePayrollTransactionItemsEmployee = payrollCalculationProducts.GetTransactions(true);
            List<int> ids = timePayrollTransactionItemsEmployee.Select(i => i.TimePayrollTransactionId).ToList();

            return (from t in entities.TimePayrollTransaction
                    .Include("TimePayrollTransactionExtended")
                    .Include("TimeBlockDate")
                    where ids.Contains(t.TimePayrollTransactionId)
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsAdded(int employeeId, int timePeriodId)
        {
            List<TimePayrollTransaction> timePayrollTransactions =
                (from t in entities.TimePayrollTransaction
                 where t.TimePeriodId == timePeriodId &&
                       t.EmployeeId == employeeId &&
                 t.State == (int)SoeEntityState.Active
                 select t).ToList();

            //Post execution filter
            return timePayrollTransactions.Where(x => x.IsAdded).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsFixedWithExtended(int employeeId, int timePeriodId)
        {
            return (from t in entities.TimePayrollTransaction
                          .Include("TimePayrollTransactionExtended")
                    where t.EmployeeId == employeeId &&
                    t.TimePeriodId == timePeriodId &&
                    t.IsFixed &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForVacationReplacement(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from tpt in entities.TimePayrollTransaction
                    where tpt.EmployeeId == employeeId &&
                    tpt.IsVacationReplacement &&
                    tpt.State == (int)SoeEntityState.Active &&
                    tpt.TimeBlockDate.Date >= dateFrom &&
                    tpt.TimeBlockDate.Date <= dateTo &&
                    (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                    select tpt).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForVacationReplacement(int employeeId, List<int> timeBlockDateIds)
        {
            return (from tpt in entities.TimePayrollTransaction
                    where tpt.EmployeeId == employeeId &&
                    tpt.IsVacationReplacement &&
                    timeBlockDateIds.Contains(tpt.TimeBlockDateId) &&
                    tpt.State == (int)SoeEntityState.Active &&
                    (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                    select tpt).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForPayrollStartValues(List<int> payrollStartValueRowIds)
        {
            if (payrollStartValueRowIds.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            return (from t in entities.TimePayrollTransaction
                    where t.PayrollStartValueRowId.HasValue &&
                    payrollStartValueRowIds.Contains(t.PayrollStartValueRowId.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForRetroCalculation(int? employeeId, DateTime dateFrom, DateTime? dateTo, TimePeriod timePeriod, List<int> lockedOrPaidTimePeriodIds, bool onlyAlreadyRetro = false)
        {
            List<TimePayrollTransaction> validTransactions = new List<TimePayrollTransaction>();
            if (!dateTo.HasValue)
                dateTo = timePeriod?.StopDate;
            if (!dateTo.HasValue)
                return validTransactions;

            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo.Value);

            var intervalTransactions = (from t in entities.TimePayrollTransaction
                                        .Include("TimePayrollTransactionExtended")
                                        where t.EmployeeId == employeeId &&
                                        t.TimeBlockDate.Date >= dateFrom &&
                                        t.TimeBlockDate.Date <= dateTo.Value &&
                                        (!onlyAlreadyRetro || t.RetroactivePayrollOutcomeId.HasValue) &&
                                        t.State == (int)SoeEntityState.Active
                                        select t).ToList();

            var periodTransactions = (from t in entities.TimePayrollTransaction
                                      .Include("TimePayrollTransactionExtended")
                                      where t.EmployeeId == employeeId &&
                                      t.TimePeriodId.HasValue &&
                                      lockedOrPaidTimePeriodIds.Contains(t.TimePeriodId.Value) &&
                                      (!onlyAlreadyRetro || t.RetroactivePayrollOutcomeId.HasValue) &&
                                      t.State == (int)SoeEntityState.Active
                                      select t).ToList();

            var intervalTransactionsWithoutMonthlySalary = intervalTransactions.Where(x => !x.IsMonthlySalaryAndFixed()).ToList();
            validTransactions.AddRange(intervalTransactionsWithoutMonthlySalary);

            var monthlySalaryTransactions = periodTransactions.Where(x => x.IsMonthlySalaryAndFixed()).ToList();
            validTransactions.AddRange(monthlySalaryTransactions);

            return validTransactions;
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForCompanyWithTimeCodeAndAccountInternals(DateTime dateFrom, DateTime dateTo)
        {
            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            return (from t in entities.TimePayrollTransaction
                        .Include("TimeBlockDate")
                        .Include("TimeCodeTransaction")
                        .Include("AccountInternal.Account")
                    where t.ActorCompanyId == actorCompanyId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForCompanyWithVacationThatResultedInLeaveOfAbsence(DateTime startDate, DateTime stopDate)
        {
            startDate = CalendarUtility.GetBeginningOfDay(startDate);
            stopDate = CalendarUtility.GetEndOfDay(stopDate);

            List<TimePayrollTransaction> timePayrollTransactions = (from tpt in entities.TimePayrollTransaction
                                                                        .Include("TimeBlockDate")
                                                                        .Include("TimeBlock.TimeDeviationCauseStart")
                                                                    where tpt.ActorCompanyId == actorCompanyId &&
                                                                    tpt.State == (int)SoeEntityState.Active &&
                                                                    tpt.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence &&
                                                                    tpt.TimeBlockDate.Date >= startDate && tpt.TimeBlockDate.Date <= stopDate &&
                                                                    tpt.TimeBlockId.HasValue &&
                                                                    tpt.TimeBlock.TimeDeviationCauseStartId.HasValue
                                                                    select tpt).ToList();

            timePayrollTransactions = timePayrollTransactions.Where(tpt => tpt.TimeBlock.TimeDeviationCauseStart.IsVacation || tpt.TimeBlock.TimeDeviationCauseStart.Name == "Semester").ToList();

            return timePayrollTransactions;
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForCompanyWithVacation3000(DateTime startDate, DateTime stopDate)
        {
            startDate = CalendarUtility.GetBeginningOfDay(startDate);
            stopDate = CalendarUtility.GetEndOfDay(stopDate);

            List<TimePayrollTransaction> timePayrollTransactions = (from tpt in entities.TimePayrollTransaction
                                                                        .Include("TimeBlockDate")
                                                                        .Include("PayrollProduct")
                                                                    where tpt.ActorCompanyId == actorCompanyId &&
                                                                    tpt.State == (int)SoeEntityState.Active &&
                                                                    tpt.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                                                                    tpt.Quantity > 0 &&
                                                                    tpt.TimeBlockDate.Date >= startDate && tpt.TimeBlockDate.Date <= stopDate
                                                                    select tpt).ToList();

            timePayrollTransactions = timePayrollTransactions.Where(tpt => tpt.PayrollProduct.Number == "30000").ToList();

            return timePayrollTransactions;
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForCompanyAndVacationYearEnd(int vacationYearEndRowId, int? timePeriodId = null)
        {
            var query = (from t in entities.TimePayrollTransaction
                         where t.ActorCompanyId == actorCompanyId &&
                         t.VacationYearEndRowId == vacationYearEndRowId &&
                         t.State == (int)SoeEntityState.Active
                         select t);

            if (timePeriodId.HasValue)
                query = query.Where(t => t.TimePeriodId == timePeriodId.Value);

            return query.ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForEmployeeAndVacationYearEndFromDate(int employeeId, DateTime date)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.VacationYearEndRowId.HasValue &&
                    t.TimeBlockDate.Date >= date &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForCompanyAndAccountProvision(DateTime dateFrom, DateTime dateTo)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetBeginningOfDay(dateTo);

            return (from t in entities.TimePayrollTransaction
                    where t.ActorCompanyId == actorCompanyId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    t.TimeCodeTransactionId.HasValue &&
                    t.TimeCodeTransaction.IsProvision &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForCompanyAndAccountProvisionWithTimeCodeAndAccountInternal(List<int> timePayrollTransactionIds)
        {
            if (timePayrollTransactionIds.IsNullOrEmpty())
                return new List<TimePayrollTransaction>();

            return (from t in entities.TimePayrollTransaction
                        .Include("TimeCodeTransaction")
                        .Include("AccountInternal")
                    where timePayrollTransactionIds.Contains(t.TimePayrollTransactionId) &&
                    t.TimeCodeTransaction.IsProvision &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDay(int employeeId, int timeBlockDateId)
        {
            return (from t in entities.TimePayrollTransaction
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDateId == timeBlockDateId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDay(int employeeId, int timeBlockDateId, TermGroup_SysPayrollType sysPayrollTypeLevel1)
        {
            List<TimePayrollTransaction> timePayrollTransactions =
                (from t in entities.TimePayrollTransaction
                 where t.EmployeeId == employeeId &&
                 t.TimeBlockDateId == timeBlockDateId &&
                 t.State == (int)SoeEntityState.Active
                 select t).ToList();

            //Post execution filter
            return timePayrollTransactions.Where(x => x.SysPayrollTypeLevel1 == (int)sysPayrollTypeLevel1).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDay(int employeeId, int timeBlockDateId, TermGroup_SysPayrollType sysPayrollTypeLevel1, TimePeriod timePeriod)
        {
            if (timePeriod.ExtraPeriod)
            {
                List<TimePayrollTransaction> timePayrollTransactions =
                    (from t in entities.TimePayrollTransaction
                     where t.EmployeeId == employeeId &&
                     t.TimeBlockDateId == timeBlockDateId &&
                     t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId &&
                     t.State == (int)SoeEntityState.Active
                     select t).ToList();

                //Post execution filter
                return timePayrollTransactions.Where(x => x.SysPayrollTypeLevel1 == (int)sysPayrollTypeLevel1).ToList();
            }
            else
            {
                List<TimePayrollTransaction> timePayrollTransactions =
                    (from t in entities.TimePayrollTransaction
                     where t.EmployeeId == employeeId &&
                     t.TimeBlockDateId == timeBlockDateId &&
                     (!t.TimePeriodId.HasValue || (t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId)) &&
                     t.State == (int)SoeEntityState.Active
                     select t).ToList();

                //Post execution filter
                return timePayrollTransactions.Where(x => x.SysPayrollTypeLevel1 == (int)sysPayrollTypeLevel1).ToList();
            }

        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDayWithExtendedAndTimeCodeAndAccounting(int employeeId, List<int> timepayrolltransactionIds)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include("TimeCodeTransaction.TimeCode")
                        .Include("TimePayrollTransactionExtended")
                         .Include("AccountInternal")
                    where t.EmployeeId == employeeId &&
                    timepayrolltransactionIds.Contains(t.TimePayrollTransactionId) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDayWithExtendedAndTimeCodeAndAccounting(int employeeId, DateTime dateFrom, DateTime dateTo, TimePeriod timePeriod)
        {
            //timeperiod can be null

            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            if (timePeriod != null && timePeriod.ExtraPeriod)
            {
                return (from t in entities.TimePayrollTransaction
                         .Include("TimePayrollTransactionExtended")
                         .Include("TimeCodeTransaction.TimeCode")
                         .Include("AccountInternal")
                        where t.EmployeeId == employeeId &&
                        t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId &&
                        t.State == (int)SoeEntityState.Active &&
                        (t.TimeBlock == null || t.TimeBlock.State == (int)SoeEntityState.Active)
                        select t).ToList();
            }
            else if (timePeriod != null)
            {
                return (from t in entities.TimePayrollTransaction
                            .Include("TimePayrollTransactionExtended")
                            .Include("TimeCodeTransaction.TimeCode")
                            .Include("AccountInternal")
                        where t.EmployeeId == employeeId &&
                        ((t.TimeBlockDate.Date >= dateFrom && t.TimeBlockDate.Date <= dateTo && !t.TimePeriodId.HasValue)
                        ||
                        (t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId)) &&
                        t.State == (int)SoeEntityState.Active &&
                        (t.TimeBlock == null || t.TimeBlock.State == (int)SoeEntityState.Active)
                        select t).ToList();
            }
            else
            {
                return (from t in entities.TimePayrollTransaction
                            .Include("TimePayrollTransactionExtended")
                            .Include("TimeCodeTransaction.TimeCode")
                            .Include("AccountInternal")
                        where t.EmployeeId == employeeId &&
                        t.TimeBlockDate.Date >= dateFrom &&
                        t.TimeBlockDate.Date <= dateTo &&
                        t.State == (int)SoeEntityState.Active &&
                        (t.TimeBlock == null || t.TimeBlock.State == (int)SoeEntityState.Active)
                        select t).ToList();
            }
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDayWithAccountInternal(int employeeId, int timeBlockDateId, TimePeriod timePeriod)
        {
            if (timePeriod.ExtraPeriod)
            {
                return (from t in entities.TimePayrollTransaction
                     .Include("AccountInternal.Account.AccountDim")
                        where t.EmployeeId == employeeId &&
                            t.TimeBlockDateId == timeBlockDateId &&
                            t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId &&
                            (t.TimeBlock == null || t.TimeBlock.State == (int)SoeEntityState.Active) &&
                            t.State == (int)SoeEntityState.Active
                        select t).ToList();
            }
            else
            {
                return (from t in entities.TimePayrollTransaction
                        .Include("AccountInternal.Account.AccountDim")
                        where t.EmployeeId == employeeId &&
                            t.TimeBlockDateId == timeBlockDateId &&
                            (!t.TimePeriodId.HasValue || (t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId)) &&
                            (t.TimeBlock == null || t.TimeBlock.State == (int)SoeEntityState.Active) &&
                            t.State == (int)SoeEntityState.Active
                        select t).ToList();
            }
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDayWithTimeCodeTransaction(int employeeId, int timeBlockDateId, int sysPayrollTypeLevel2, int sysPayrollTypeLevel3)
        {
            List<TimePayrollTransaction> timePayrollTransactions = (from tpt in entities.TimePayrollTransaction
                                                                        .Include("TimeCodeTransaction")
                                                                    where tpt.EmployeeId == employeeId &&
                                                                    tpt.SysPayrollTypeLevel2 == sysPayrollTypeLevel2 &&
                                                                    tpt.SysPayrollTypeLevel3 == sysPayrollTypeLevel3 &&
                                                                    tpt.State == (int)SoeEntityState.Active &&
                                                                    tpt.TimeBlockDateId == timeBlockDateId &&
                                                                    (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                                                                    select tpt).ToList();

            return timePayrollTransactions;
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDayAndLevel2WithTimeCodeTransaction(int employeeId, int timeBlockDateId, int sysPayrollTypeLevel2, int? excludeSysPayrollTypeLevel3 = null)
        {
            List<TimePayrollTransaction> timePayrollTransactions = (from tpt in entities.TimePayrollTransaction
                                                                        .Include("TimeCodeTransaction")
                                                                    where tpt.EmployeeId == employeeId &&
                                                                    tpt.SysPayrollTypeLevel2 == sysPayrollTypeLevel2 &&
                                                                    tpt.State == (int)SoeEntityState.Active &&
                                                                    tpt.TimeBlockDateId == timeBlockDateId &&
                                                                    (!tpt.TimeBlockId.HasValue || tpt.TimeBlock.State == (int)SoeEntityState.Active)
                                                                    select tpt).ToList();

            if (excludeSysPayrollTypeLevel3.HasValue)
                timePayrollTransactions = timePayrollTransactions.Where(i => i.SysPayrollTypeLevel3 != excludeSysPayrollTypeLevel3.Value).ToList();

            return timePayrollTransactions;
        }

        private List<TimePayrollTransaction> GetQualifyingDeductionBasisTransactions(int employeeId, int timeBlockDateId)
        {
            if (UsePayroll())
            {
                int sysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary;
                int excludeSysPayrollTypeLevel3 = (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Deduction;
                return GetTimePayrollTransactionsForDayAndLevel2WithTimeCodeTransaction(employeeId, timeBlockDateId, sysPayrollTypeLevel2, excludeSysPayrollTypeLevel3);
            }
            else
            {
                int sysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence;
                int sysPayrollTypeLevel3 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick;
                return GetTimePayrollTransactionsForDayWithTimeCodeTransaction(employeeId, timeBlockDateId, sysPayrollTypeLevel2, sysPayrollTypeLevel3);
            }
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsForDayAndEarnedHoliday(List<int> employeeIds, DateTime date)
        {
            // Make sure the whole day is covered
            DateTime dateFrom = CalendarUtility.GetBeginningOfDay(date);
            DateTime dateTo = CalendarUtility.GetEndOfDay(date);

            var result = (from t in entities.TimePayrollTransaction
                       .Include("TimeCodeTransaction")
                          where t.ActorCompanyId == actorCompanyId &&
                          employeeIds.Contains(t.EmployeeId) &&
                          t.TimeBlockDate.Date >= dateFrom &&
                          t.TimeBlockDate.Date <= dateTo &&
                          t.State == (int)SoeEntityState.Active
                          select t).ToList();

            return result.Where(x => x.TimeCodeTransaction != null && x.TimeCodeTransaction.IsEarnedHoliday).ToList();
        }

        private List<TimePayrollTransaction> FilterTimePayrollTransactionsByUseInPayroll(List<TimePayrollTransaction> timePayrollTransactions)
        {
            List<TimePayrollTransaction> filtered = new List<TimePayrollTransaction>();

            if (!timePayrollTransactions.IsNullOrEmpty())
            {
                foreach (var timePayrollTransactionsByProduct in timePayrollTransactions.GroupBy(t => t.ProductId))
                {
                    PayrollProduct product = GetPayrollProductFromCache(timePayrollTransactionsByProduct.Key, includeInactive: true);
                    if (product != null && product.UseInPayroll)
                        filtered.AddRange(timePayrollTransactionsByProduct);
                }
            }

            return filtered;
        }

        private List<TimePayrollTransaction> GetTimePayrollTransactionsWithEarningAccumulator(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include(t => t.TimePayrollTransactionExtended)
                        .Include(t => t.TimeBlockDate)
                    where t.EmployeeId == employeeId &&
                    t.TimeBlockDate.Date >= dateFrom &&
                    t.TimeBlockDate.Date <= dateTo &&
                    t.EarningTimeAccumulatorId.HasValue &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimeTransactionItem> GetTimeTransactionItemsWithoutTimeBlocks(TimeEngineTemplate template, int timeScheduleTemplatePeriodId)
        {
            if (template?.Employee == null)
                return new List<TimeTransactionItem>();

            return TimeTransactionManager.GetTimePayrollTransactionItems(entities, actorCompanyId, template.EmployeeId, template.Date, template.Date, timeScheduleTemplatePeriodId, false, true, includeScheduleTransactions: false, onlyTransactionsWithoutTimeBlocks: true, employeeGroup: template.EmployeeGroup);
        }

        private List<TimeTransactionItem> GetQualifyingDeductionBasisTransactions(List<TimeTransactionItem> transactionItems)
        {
            List<TimeTransactionItem> qualifyingDeductionBasisTransactions = null;

            bool usePayroll = UsePayroll();
            if (usePayroll)
            {
                int sysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary;
                int excludeSysPayrollTypeLevel3 = (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Deduction;
                qualifyingDeductionBasisTransactions = transactionItems.Where(i => i.TransactionSysPayrollTypeLevel2 == sysPayrollTypeLevel2 && i.TransactionSysPayrollTypeLevel3 != excludeSysPayrollTypeLevel3).ToList();
            }
            else
            {
                int sysPayrollTypeLevel2 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence;
                int sysPayrollTypeLevel3 = (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick;
                qualifyingDeductionBasisTransactions = transactionItems.Where(i => i.TransactionSysPayrollTypeLevel2 == sysPayrollTypeLevel2 && i.TransactionSysPayrollTypeLevel3 == sysPayrollTypeLevel3).ToList();
            }

            return qualifyingDeductionBasisTransactions;
        }

        private Dictionary<int, List<TimePayrollTransactionTreeDTO>> GetTimePayrollTransactionsForTreeByEmployee(List<int> employeeIds, DateTime startDate, DateTime stopDate, bool includeAccounting = false)
        {
            return TimeTransactionManager.GetTimePayrollTransactionsForTree(entities, base.ActorCompanyId, startDate, stopDate, null, employeeIds, includeAccounting: includeAccounting).GroupBy(i => i.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
        }

        private TimePayrollTransaction GetTimePayrollTransactionWithTimeBlockDateAndTimePeriod(int timePayrollTransactionId)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include("TimeBlockDate")
                        .Include("TimePeriod")
                    where t.TimePayrollTransactionId == timePayrollTransactionId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimePayrollTransaction GetTimePayrollTransactionWithTimeBlockDateAndExtended(int timePayrollTransactionId)
        {
            return (from t in entities.TimePayrollTransaction
                        .Include("TimeBlockDate")
                        .Include("TimePayrollTransactionExtended")
                    where t.TimePayrollTransactionId == timePayrollTransactionId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimePayrollTransaction GetTimePayrollTransactionWithAccountInternals(int timePayrollTransactionId)
        {
            return (from t in entities.TimePayrollTransaction
                    .Include("AccountInternal")
                    .Include("TimePayrollTransactionExtended")
                    where t.TimePayrollTransactionId == timePayrollTransactionId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimePayrollTransaction GetTimePayrollTransactionLastLockedWithTimeBlockDate(int employeeId, DateTime? fromDate = null)
        {
            if (fromDate.HasValue)
                fromDate = CalendarUtility.GetBeginningOfDay(fromDate.Value);

            int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();

            return (from t in entities.TimePayrollTransaction
                        .Include("TimeBlockDate")
                    where t.ActorCompanyId == actorCompanyId &&
                    t.EmployeeId == employeeId &&
                    t.TimeBlockDate.EmployeeId == employeeId &&
                    (!fromDate.HasValue || fromDate.Value <= t.TimeBlockDate.Date) &&
                    payrollLockedAttestStateIds.Contains(t.AttestStateId) &&
                    t.State == (int)SoeEntityState.Active
                    orderby t.TimeBlockDate.Date descending
                    select t).FirstOrDefault();
        }

        private ActionResult CreateTimePayrollTransaction(List<TimePayrollTransaction> reusableTimePayrollTransactions, PayrollProduct payrollProduct, TimeBlockDate timeBlockDate, Employee employee, decimal quantity, decimal amount, decimal vatAmount, decimal unitPrice, string comment, int attestStateId, int? timePeriodId, bool applyFixedAccounting, bool applyChain, out List<TimePayrollTransaction> newTransactions, bool applyAccounting = true, bool removeFromReusableCollection = false)
        {
            newTransactions = new List<TimePayrollTransaction>();
            var timePayrollTransaction = CreateOrUpdateTimePayrollTransaction(payrollProduct, timeBlockDate, employee, timePeriodId, attestStateId, quantity, unitPrice, amount, vatAmount, comment, reusableTimePayrollTransactions, removeFromReusableCollection: removeFromReusableCollection, applyAccounting: applyAccounting);
            if (timePayrollTransaction != null)
            {
                newTransactions.Add(timePayrollTransaction);
                if (applyChain)
                {
                    ActionResult result = CreateTransactionsFromPayrollProductChain(timePayrollTransaction, employee, timePayrollTransaction.TimeBlockDate, out List<TimePayrollTransaction> childTransactions);
                    if (!result.Success)
                        return result;

                    newTransactions.AddRange(childTransactions);
                }

                if (applyFixedAccounting)
                {
                    ActionResult result = CreateFixedAccountingTransactions(newTransactions, employee, timePayrollTransaction.TimeBlockDate, out List<TimePayrollTransaction> fixedAccountingTransactions);
                    if (!result.Success)
                        return result;

                    newTransactions.AddRange(fixedAccountingTransactions);

                }
            }

            return new ActionResult(true);
        }

        private TimePayrollTransaction CreateOrUpdateTimePayrollTransaction(
            PayrollProduct payrollProduct, 
            TimeBlockDate timeBlockDate, 
            Employee employee, 
            int? timePeriodId, 
            int attestStateId, 
            decimal quantity,
            decimal unitPrice, 
            decimal amount, 
            decimal vatAmount = 0, 
            string comment = null, 
            List<TimePayrollTransaction> reusableTimePayrollTransactions = null, 
            bool removeFromReusableCollection = false, 
            bool applyAccounting = true,
            TimeCodeTransaction timeCodeTransaction = null
            )
        {
            if (timeBlockDate == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = reusableTimePayrollTransactions?.FirstOrDefault(i => i.ProductId == payrollProduct.ProductId);
            if (timePayrollTransaction == null)
                timePayrollTransaction = CreateTimePayrollTransaction(payrollProduct, timeBlockDate, quantity, amount, vatAmount, unitPrice, comment, attestStateId, timePeriodId, employee.EmployeeId);
            else
                timePayrollTransaction = UpdateTimePayrollTransaction(timePayrollTransaction, payrollProduct, timeBlockDate, quantity, amount, vatAmount, unitPrice, comment, employee.EmployeeId, attestStateId, timePeriodId);

            if (timeCodeTransaction != null)
                timePayrollTransaction.TimeCodeTransaction = timeCodeTransaction;

            CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);

            if (applyAccounting)
            {
                DateTime accountingDate = timeBlockDate.Date;

                //This should probably be done for more payrollproducts
                if ((payrollProduct.IsEmploymentTaxCredit() || payrollProduct.IsSupplementChargeCredit()) && employee.GetEmployment(accountingDate) == null && timePeriodId.HasValue)
                {
                    TimePeriod timePeriod = GetTimePeriodFromCache(timePeriodId.Value);
                    accountingDate = GetPayrollAccountingDateIfEmployeeNotEmployedOnTransactionDate(timePeriod, employee, timeBlockDate.Date);
                }

                ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, accountingDate, payrollProduct);
            }

            if (removeFromReusableCollection && timePayrollTransaction.TimePayrollTransactionId != 0 && (reusableTimePayrollTransactions?.Any(x => x.TimePayrollTransactionId == timePayrollTransaction.TimePayrollTransactionId) ?? false))
                reusableTimePayrollTransactions.RemoveAll(s => s.TimePayrollTransactionId == timePayrollTransaction.TimePayrollTransactionId);

            return timePayrollTransaction;
        }

        private TimePayrollTransaction CreateTimePayrollTransaction(PayrollProduct product, TimeBlockDate timeBlockDate, decimal quantity, decimal amount, decimal vatAmount, decimal unitPrice, string comment, int attestStateId, int? timePeriodId, int employeeId, int accountId = 0, TimeCodeTransaction timeCodeTransaction = null, VacationYearEndRow vacationYearEndRow = null)
        {
            if (product == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
            {
                Quantity = quantity,
                Amount = amount,
                VatAmount = vatAmount,
                UnitPrice = unitPrice,
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
                TimePeriodId = timePeriodId != 0 ? timePeriodId : null,

                //References
                PayrollProduct = product,
                TimeBlockDate = timeBlockDate,
                TimeCodeTransaction = timeCodeTransaction,
                VacationYearEndRow = vacationYearEndRow,
            };
            SetCreatedProperties(timePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

            SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);

            return timePayrollTransaction;
        }

        private TimePayrollTransaction CreateTimePayrollTransactionReversed(TimePayrollTransaction prototype, int? timePeriodId, int attestStateId, RetroactivePayrollOutcome retroOutcome = null)
        {
            if (prototype == null)
                return null;

            TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
            {
                Quantity = prototype.Quantity,
                UnitPrice = prototype.UnitPrice.HasValue ? Decimal.Negate(prototype.UnitPrice.Value) : (decimal?)null,
                Amount = prototype.Amount.HasValue ? Decimal.Negate(prototype.Amount.Value) : (decimal?)null,
                VatAmount = prototype.VatAmount.HasValue ? Decimal.Negate(prototype.VatAmount.Value) : (decimal?)null,
                Comment = prototype.Comment,
                ManuallyAdded = prototype.ManuallyAdded,
                IsAdditionOrDeduction = prototype.IsAdditionOrDeduction,
                IsSpecifiedUnitPrice = prototype.IsSpecifiedUnitPrice,
                IsCentRounding = prototype.IsCentRounding,
                IsQuantityRounding = prototype.IsQuantityRounding,
                IsVacationReplacement = prototype.IsVacationReplacement,
                IsFixed = prototype.IsFixed,
                IsAdded = prototype.IsAdded,
                AddedDateFrom = prototype.AddedDateFrom,
                AddedDateTo = prototype.AddedDateTo,
                SysPayrollTypeLevel1 = prototype.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = prototype.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = prototype.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = prototype.SysPayrollTypeLevel4,

                //Clean
                Exported = false,
                IsPreliminary = false,
                IsReversed = false,
                ReversedDate = null,
                ModifiedWithNoCheckes = false,
                AutoAttestFailed = false,

                //Set FK (from prototype)
                ActorCompanyId = prototype.ActorCompanyId,
                EmployeeId = prototype.EmployeeId,
                AccountStdId = prototype.AccountStdId,
                ProductId = prototype.ProductId,
                TimeBlockDateId = prototype.TimeBlockDateId,
                EmployeeChildId = prototype.EmployeeChildId,
                EmployeeVehicleId = prototype.EmployeeVehicleId,
                MassRegistrationTemplateRowId = prototype.MassRegistrationTemplateRowId,
                UnionFeeId = prototype.UnionFeeId,
                PayrollStartValueRowId = prototype.PayrollStartValueRowId,
                ParentId = prototype.ParentId,

                //Set FK (from params)
                TimePeriodId = timePeriodId ?? prototype.TimePeriodId,
                AttestStateId = attestStateId,

                //Set references (from params)
                RetroactivePayrollOutcome = retroOutcome,

                //Set FK (clean)
                TimeSalaryPaymentExportEmployeeId = null,
            };
            SetCreatedProperties(timePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

            SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);

            if (prototype.IsExtended)
            {
                if (!prototype.TimePayrollTransactionExtendedReference.IsLoaded)
                    prototype.TimePayrollTransactionExtendedReference.Load();

                CreateTimePayrollTransactionExtended(timePayrollTransaction, prototype.EmployeeId, prototype.ActorCompanyId);
                if (timePayrollTransaction.TimePayrollTransactionExtended != null)
                {
                    timePayrollTransaction.TimePayrollTransactionExtended.Formula = prototype.TimePayrollTransactionExtended.Formula;
                    timePayrollTransaction.TimePayrollTransactionExtended.Formula = prototype.TimePayrollTransactionExtended.FormulaPlain;
                    timePayrollTransaction.TimePayrollTransactionExtended.Formula = prototype.TimePayrollTransactionExtended.FormulaExtracted;
                    timePayrollTransaction.TimePayrollTransactionExtended.Formula = prototype.TimePayrollTransactionExtended.FormulaNames;
                    timePayrollTransaction.TimePayrollTransactionExtended.Formula = prototype.TimePayrollTransactionExtended.FormulaOrigin;
                    timePayrollTransaction.TimePayrollTransactionExtended.TimeUnit = prototype.TimePayrollTransactionExtended.TimeUnit;
                    timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays = prototype.TimePayrollTransactionExtended.QuantityWorkDays;
                    timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = prototype.TimePayrollTransactionExtended.QuantityCalendarDays;
                    timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = prototype.TimePayrollTransactionExtended.CalenderDayFactor;
                    timePayrollTransaction.TimePayrollTransactionExtended.IsDistributed = prototype.TimePayrollTransactionExtended.IsDistributed;

                    //Clean
                    timePayrollTransaction.TimePayrollTransactionExtended.PayrollCalculationPerformed = false;

                    //Set FK (from prototype)
                    timePayrollTransaction.TimePayrollTransactionExtended.PayrollPriceFormulaId = prototype.TimePayrollTransactionExtended.PayrollPriceFormulaId;
                    timePayrollTransaction.TimePayrollTransactionExtended.PayrollPriceTypeId = prototype.TimePayrollTransactionExtended.PayrollPriceTypeId;
                }
            }

            if (!prototype.AccountInternal.IsLoaded)
                prototype.AccountInternal.Load();

            foreach (AccountInternal accountInternal in prototype.AccountInternal)
            {
                timePayrollTransaction.AccountInternal.Add(accountInternal);
            }

            return timePayrollTransaction;
        }

        private TimePayrollTransaction UpdateTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, PayrollProduct product, TimeBlockDate timeBlockDate, decimal quantity, decimal amount, decimal vatAmount, decimal unitPrice, string comment, int employeeId, int attestStateId, int? timePeriodId)
        {
            if (timePayrollTransaction != null)
            {
                timePayrollTransaction.Quantity = quantity;
                timePayrollTransaction.Amount = amount;
                timePayrollTransaction.VatAmount = vatAmount;
                timePayrollTransaction.UnitPrice = unitPrice;
                timePayrollTransaction.Comment = comment;
                timePayrollTransaction.State = (int)SoeEntityState.Active;

                timePayrollTransaction.SysPayrollTypeLevel1 = product.SysPayrollTypeLevel1;
                timePayrollTransaction.SysPayrollTypeLevel2 = product.SysPayrollTypeLevel2;
                timePayrollTransaction.SysPayrollTypeLevel3 = product.SysPayrollTypeLevel3;
                timePayrollTransaction.SysPayrollTypeLevel4 = product.SysPayrollTypeLevel4;

                //Set FK
                timePayrollTransaction.PayrollProduct = product;
                timePayrollTransaction.ActorCompanyId = actorCompanyId;
                timePayrollTransaction.EmployeeId = employeeId;
                timePayrollTransaction.AccountStdId = 0; //ApplyAccountingOnTimePayrollTransaction must always be called
                timePayrollTransaction.AttestStateId = attestStateId;
                timePayrollTransaction.TimeBlockDate = timeBlockDate;
                timePayrollTransaction.TimePeriodId = timePeriodId;

                SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
            }

            return timePayrollTransaction;
        }

        private TimePayrollTransaction CreateTimePayrollTransactionFromParent(PayrollProduct product, TimePayrollTransaction parentTransaction, Employee employee, int attestStateId, bool includedInPayrollProductChain)
        {
            if (product == null)
                return null;

            if (parentTransaction == null)
                return null;

            TimeBlockDate timeBlockDate = parentTransaction.TimeBlockDate;
            if (timeBlockDate == null)
                return null;

            Employment employment = employee.GetEmployment(timeBlockDate.Date);
            if (employment == null)
                return null;

            TimePayrollTransaction childTimePayrollTransaction = new TimePayrollTransaction
            {
                ManuallyAdded = parentTransaction.ManuallyAdded,
                IsAdded = parentTransaction.IsAdded,
                IsFixed = parentTransaction.IsFixed,
                IsAdditionOrDeduction = parentTransaction.IsAdditionOrDeduction,
                IsSpecifiedUnitPrice = parentTransaction.IsSpecifiedUnitPrice,
                IsPreliminary = parentTransaction.IsPreliminary,
                IsCentRounding = parentTransaction.IsCentRounding,
                IsQuantityRounding = parentTransaction.IsQuantityRounding,
                UnitPrice = parentTransaction.UnitPrice,
                UnitPriceCurrency = parentTransaction.UnitPriceCurrency,
                Comment = parentTransaction.Comment,
                Amount = parentTransaction.Amount,
                VatAmount = 0,
                AddedDateFrom = parentTransaction.AddedDateFrom,
                AddedDateTo = parentTransaction.AddedDateTo,
                IsVacationFiveDaysPerWeek = parentTransaction.IsVacationFiveDaysPerWeek,

                //PayrollProductChain
                IncludedInPayrollProductChain = includedInPayrollProductChain,
                Parent = parentTransaction,

                //Product
                SysPayrollTypeLevel1 = product.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = product.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = product.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = product.SysPayrollTypeLevel4,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = employee.EmployeeId,
                EmployeeChildId = parentTransaction.EmployeeChildId,
                AttestStateId = attestStateId,
                TimePeriodId = parentTransaction.TimePeriodId,
                TimeCodeTransactionId = parentTransaction.TimeCodeTransactionId,
                MassRegistrationTemplateRowId = parentTransaction.MassRegistrationTemplateRowId,
                PayrollImportEmployeeTransactionId = parentTransaction.PayrollImportEmployeeTransactionId,

                //References
                PayrollProduct = product,
                TimeBlockDate = timeBlockDate,
            };
            SetCreatedProperties(childTimePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(childTimePayrollTransaction);

            CreateTimePayrollTransactionExtended(childTimePayrollTransaction, employee.EmployeeId, actorCompanyId);
            SetTimePayrollTransactionQuantity(childTimePayrollTransaction, product, employment, timeBlockDate, parentTransaction.Quantity);

            // TimeBlock
            if (parentTransaction.TimeBlockId.HasValue && parentTransaction.TimeBlockId > 0)
                childTimePayrollTransaction.TimeBlockId = parentTransaction.TimeBlockId;
            else if (base.IsEntityAvailableInContext(entities, parentTransaction.TimeBlock))
                childTimePayrollTransaction.TimeBlock = parentTransaction.TimeBlock;

            // Accounting
            ApplyAccountingOnTimePayrollTransaction(childTimePayrollTransaction, employee, parentTransaction.TimeBlockDate.Date, product, accountInternals: parentTransaction.AccountInternal.ToList(), setAccountInternal: true);

            return childTimePayrollTransaction;
        }

        private TimeTransactionItem CreateTimeTransactionItemFromParent(PayrollProduct payrollProduct, TimeTransactionItem parentTransaction, Employee employee, TimeBlockDate timeBlockDate, bool includedInPayrollProductChain)
        {
            if (payrollProduct == null)
                return null;
            if (parentTransaction == null)
                return null;

            TimeTransactionItem childTimeTimeTransactionItem = new TimeTransactionItem
            {
                GuidInternalFK = parentTransaction.GuidInternalFK,
                GuidTimeBlockFK = parentTransaction.GuidTimeBlockFK,

                //Transaction
                IsAdded = parentTransaction.IsAdded,
                IsFixed = parentTransaction.IsFixed,
                ManuallyAdded = parentTransaction.ManuallyAdded,
                ReversedDate = null,
                IsReversed = false,
                TransactionType = parentTransaction.TransactionType,
                Quantity = parentTransaction.Quantity,
                Comment = parentTransaction.Comment,
                IsScheduleTransaction = parentTransaction.IsScheduleTransaction,
                IsVacationReplacement = false,
                IsCentRounding = parentTransaction.IsCentRounding,
                IsQuantityRounding = parentTransaction.IsQuantityRounding,
                ScheduleTransactionType = parentTransaction.ScheduleTransactionType,
                TransactionSysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                TransactionSysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                TransactionSysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                TransactionSysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,

                //Employee
                EmployeeId = parentTransaction.EmployeeId,
                EmployeeName = parentTransaction.EmployeeName,
                EmployeeChildId = parentTransaction.EmployeeChildId,
                EmployeeChildName = parentTransaction.EmployeeChildName,

                //PayrollProductChain
                IncludedInPayrollProductChain = includedInPayrollProductChain,
                ParentGuidId = parentTransaction.GuidId,

                //Product
                ProductId = payrollProduct.ProductId,
                ProductNr = payrollProduct.Number,
                ProductName = payrollProduct.Name,
                ProductVatType = TermGroup_InvoiceProductVatType.Service,
                PayrollProductSysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                PayrollProductSysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                PayrollProductSysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                PayrollProductSysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,

                //TimeCode
                TimeCodeId = parentTransaction.TimeCodeId,
                Code = parentTransaction.Code,
                CodeName = parentTransaction.CodeName,
                TimeCodeStart = parentTransaction.TimeCodeStart,
                TimeCodeStop = parentTransaction.TimeCodeStop,
                TimeCodeType = parentTransaction.TimeCodeType,
                TimeCodeRegistrationType = parentTransaction.TimeCodeRegistrationType,

                //TimeBlock
                TimeBlockId = parentTransaction.TimeBlockId,

                //TimeBlockDate
                TimeBlockDateId = parentTransaction.TimeBlockDateId,
                Date = parentTransaction.Date,

                //TimeRule
                TimeRuleId = parentTransaction.TimeRuleId,
                TimeRuleName = parentTransaction.TimeRuleName,
                TimeRuleSort = parentTransaction.TimeRuleSort,

                //AttestState
                AttestStateId = parentTransaction.IsScheduleTransaction ? 0 : parentTransaction.AttestStateId,
                AttestStateName = parentTransaction.IsScheduleTransaction ? string.Empty : parentTransaction.AttestStateName,
                AttestStateInitial = !parentTransaction.IsScheduleTransaction && parentTransaction.AttestStateInitial,
                AttestStateColor = parentTransaction.IsScheduleTransaction ? String.Empty : parentTransaction.AttestStateColor,
                AttestStateSort = parentTransaction.IsScheduleTransaction ? 0 : parentTransaction.AttestStateSort,
            };

            // Accounting
            ApplyAccountingOnTimeTransactionItem(childTimeTimeTransactionItem, employee, payrollProduct, timeBlockDate.Date, setAccountInternal: false);
            AddAccountInternalsToTimeTransactionItem(childTimeTimeTransactionItem, parentTransaction);

            return childTimeTimeTransactionItem;
        }

        private TimePayrollTransaction CreateFixedAccountingTimePayrolltransactionFromOriginal(Employee employee, TimePayrollTransaction originalTransaction, PayrollProduct product, decimal originalQuantity, EmploymentAccountStd employmentAccountStd, Employment employment)
        {
            TimePayrollTransaction newTransaction = CopyTimePayrollTransaction(originalTransaction, originalTransaction.TimeBlockDate, originalTransaction.TimeBlockId, employment, false);
            if (newTransaction == null)
                return null;

            //Set properties that is not set in CopyTimePayrollTransaction
            newTransaction.TimeCodeTransactionId = originalTransaction.TimeCodeTransactionId;
            newTransaction.TimePeriodId = originalTransaction.TimePeriodId;
            newTransaction.TimeBlockDateId = originalTransaction.TimeBlockDateId;
            newTransaction.MassRegistrationTemplateRowId = originalTransaction.MassRegistrationTemplateRowId;
            newTransaction.VacationYearEndRowId = originalTransaction.VacationYearEndRowId;
            newTransaction.Quantity = decimal.Round(originalQuantity * decimal.Divide(employmentAccountStd.Percent, 100), 2, MidpointRounding.AwayFromZero);
            if (product.IsQuantity((TermGroup_PayrollResultType)product.ResultType))
                newTransaction.Amount = decimal.Round(newTransaction.Quantity * newTransaction.UnitPrice ?? 0, 2, MidpointRounding.AwayFromZero);

            newTransaction.AccountStdId = 0;
            newTransaction.PayrollImportEmployeeTransactionId = originalTransaction.PayrollImportEmployeeTransactionId;

            if (employmentAccountStd.AccountId.HasValue)
                newTransaction.AccountStdId = employmentAccountStd.AccountId.Value;
            else
                ApplyAccountingOnTimePayrollTransaction(newTransaction, employee, originalTransaction.TimeBlockDate.Date, product, setAccountStd: true, setAccountInternal: false);

            newTransaction.AccountInternal.Clear();
            AddAccountInternalsToTimePayrollTransaction(newTransaction, employmentAccountStd.AccountInternal);

            return newTransaction;
        }

        private TimePayrollTransaction CreateDistributedTimePayrolltransactionFromOriginal(TimePayrollTransaction originalTransaction, Employment employment, decimal newQuantity, int accountStdId, List<AccountInternal> accountInternals)
        {
            TimePayrollTransaction newTransaction = CopyTimePayrollTransaction(originalTransaction, originalTransaction.TimeBlockDate, originalTransaction.TimeBlockId, employment, false);
            if (newTransaction == null)
                return null;

            //Set properties that is not set in CopyTimePayrollTransaction
            newTransaction.TimeCodeTransactionId = originalTransaction.TimeCodeTransactionId;
            newTransaction.TimePeriodId = originalTransaction.TimePeriodId;
            newTransaction.TimeBlockDateId = originalTransaction.TimeBlockDateId;
            newTransaction.MassRegistrationTemplateRowId = originalTransaction.MassRegistrationTemplateRowId;
            newTransaction.Quantity = newQuantity;
            newTransaction.AccountStdId = accountStdId;
            newTransaction.TimePayrollTransactionExtended.IsDistributed = true;

            newTransaction.AccountInternal.Clear();
            AddAccountInternalsToTimePayrollTransaction(newTransaction, accountInternals);

            return newTransaction;
        }

        private TimePayrollTransaction CopyTimePayrollTransaction(TimePayrollTransaction prototypeTimePayrollTransaction, TimeBlockDate timeBlockDate, int? timeBlockId, Employment employment, bool setAccounting = true)
        {
            if (timeBlockDate == null || employment == null)
                return null;

            PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(prototypeTimePayrollTransaction.ProductId);
            if (payrollProduct == null)
                return null;

            TimePayrollTransaction cloneTimePayrollTransaction = new TimePayrollTransaction()
            {
                Amount = prototypeTimePayrollTransaction.Amount,
                AmountCurrency = prototypeTimePayrollTransaction.AmountCurrency,
                VatAmount = prototypeTimePayrollTransaction.VatAmount,
                VatAmountCurrency = prototypeTimePayrollTransaction.VatAmountCurrency,
                Quantity = prototypeTimePayrollTransaction.Quantity,
                UnitPrice = prototypeTimePayrollTransaction.UnitPrice,
                UnitPriceCurrency = prototypeTimePayrollTransaction.UnitPriceCurrency,
                IsPreliminary = prototypeTimePayrollTransaction.IsPreliminary,
                ManuallyAdded = prototypeTimePayrollTransaction.ManuallyAdded,
                Exported = prototypeTimePayrollTransaction.Exported,
                AutoAttestFailed = prototypeTimePayrollTransaction.AutoAttestFailed,
                Comment = prototypeTimePayrollTransaction.Comment,
                IsCentRounding = prototypeTimePayrollTransaction.IsCentRounding,
                IsQuantityRounding = prototypeTimePayrollTransaction.IsQuantityRounding,
                IsAdded = prototypeTimePayrollTransaction.IsAdded,
                IsFixed = prototypeTimePayrollTransaction.IsFixed,
                IsAdditionOrDeduction = prototypeTimePayrollTransaction.IsAdditionOrDeduction,
                IsVacationReplacement = prototypeTimePayrollTransaction.IsVacationReplacement,
                IsSpecifiedUnitPrice = prototypeTimePayrollTransaction.IsSpecifiedUnitPrice,
                AddedDateFrom = prototypeTimePayrollTransaction.AddedDateFrom,
                AddedDateTo = prototypeTimePayrollTransaction.AddedDateTo,
                IncludedInPayrollProductChain = prototypeTimePayrollTransaction.IncludedInPayrollProductChain,

                SysPayrollTypeLevel1 = prototypeTimePayrollTransaction.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = prototypeTimePayrollTransaction.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = prototypeTimePayrollTransaction.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = prototypeTimePayrollTransaction.SysPayrollTypeLevel4,

                //Set FK (from copy)
                ProductId = prototypeTimePayrollTransaction.ProductId,
                AccountStdId = prototypeTimePayrollTransaction.AccountStdId,
                AttestStateId = prototypeTimePayrollTransaction.AttestStateId,

                //Set FK (from identity)
                ActorCompanyId = actorCompanyId,
                EmployeeId = employment.EmployeeId,
                TimeBlockId = timeBlockId,
                EmployeeChildId = prototypeTimePayrollTransaction.EmployeeChildId,

                //Set references
                TimeBlockDate = timeBlockDate, //Don't set FK becuase it can been created but not saved
            };
            SetCreatedProperties(cloneTimePayrollTransaction);
            entities.TimePayrollTransaction.AddObject(cloneTimePayrollTransaction);

            CreateTimePayrollTransactionExtended(cloneTimePayrollTransaction, employment.EmployeeId, actorCompanyId);
            SetTimePayrollTransactionQuantity(cloneTimePayrollTransaction, payrollProduct, employment, timeBlockDate, prototypeTimePayrollTransaction.Quantity);

            // Accounting
            if (setAccounting)
            {
                cloneTimePayrollTransaction.AccountStd = prototypeTimePayrollTransaction.AccountStd;
                AddAccountInternalsToTimePayrollTransaction(cloneTimePayrollTransaction, prototypeTimePayrollTransaction.AccountInternal);
            }

            return cloneTimePayrollTransaction;
        }

        private TimeTransactionItem CreateFixedAccountingTimePayrollTransactionFromOriginal(Employee employee, TimeTransactionItem originalTransaction, PayrollProduct product, decimal originalQuantity, EmploymentAccountStd employmentAccountStd, DateTime date)
        {
            TimeTransactionItem newTransaction = CopyTimeTransactionItem(originalTransaction, originalTransaction.TimeBlockId, false);
            newTransaction.Quantity = decimal.Round(originalQuantity * decimal.Divide(employmentAccountStd.Percent, 100), 2, MidpointRounding.AwayFromZero);

            if (employmentAccountStd.AccountStd != null)
                AddAccountStdToTimeTransactionItem(newTransaction, employmentAccountStd.AccountStd);
            else
                ApplyAccountingOnTimeTransactionItem(newTransaction, employee, product, date, setAccountStd: true, setAccountInternal: false);

            AddAccountInternalsToTimeTransactionItem(newTransaction, employmentAccountStd.AccountInternal.ToList());

            return newTransaction;
        }

        private TimeTransactionItem CopyTimeTransactionItem(TimeTransactionItem prototypeTransaction, int? timeBlockId, bool setAccounting = true)
        {
            if (prototypeTransaction == null)
                return null;

            TimeTransactionItem cloneTimePayrollTransaction = new TimeTransactionItem
            {
                GuidInternalFK = prototypeTransaction.GuidInternalFK,
                GuidTimeBlockFK = prototypeTransaction.GuidTimeBlockFK,

                //Transaction
                IsAdded = prototypeTransaction.IsAdded,
                IsFixed = prototypeTransaction.IsFixed,
                ManuallyAdded = prototypeTransaction.ManuallyAdded,
                ReversedDate = prototypeTransaction.ReversedDate,
                IsReversed = prototypeTransaction.IsReversed,
                TransactionType = prototypeTransaction.TransactionType,
                Quantity = prototypeTransaction.Quantity,
                Comment = prototypeTransaction.Comment,
                IsScheduleTransaction = prototypeTransaction.IsScheduleTransaction,
                IsVacationReplacement = prototypeTransaction.IsVacationReplacement,
                IsCentRounding = prototypeTransaction.IsCentRounding,
                IsQuantityRounding = prototypeTransaction.IsQuantityRounding,
                ScheduleTransactionType = prototypeTransaction.ScheduleTransactionType,
                IncludedInPayrollProductChain = prototypeTransaction.IncludedInPayrollProductChain,

                //Employee
                EmployeeId = prototypeTransaction.EmployeeId,
                EmployeeName = prototypeTransaction.EmployeeName,
                EmployeeChildId = prototypeTransaction.EmployeeChildId,
                EmployeeChildName = prototypeTransaction.EmployeeChildName,

                //Product
                ProductId = prototypeTransaction.ProductId,
                ProductNr = prototypeTransaction.ProductNr,
                ProductName = prototypeTransaction.ProductName,
                ProductVatType = prototypeTransaction.ProductVatType,
                PayrollProductSysPayrollTypeLevel1 = prototypeTransaction.PayrollProductSysPayrollTypeLevel1,
                PayrollProductSysPayrollTypeLevel2 = prototypeTransaction.PayrollProductSysPayrollTypeLevel2,
                PayrollProductSysPayrollTypeLevel3 = prototypeTransaction.PayrollProductSysPayrollTypeLevel3,
                PayrollProductSysPayrollTypeLevel4 = prototypeTransaction.PayrollProductSysPayrollTypeLevel4,

                //TimeCode
                TimeCodeId = prototypeTransaction.TimeCodeId,
                Code = prototypeTransaction.Code,
                CodeName = prototypeTransaction.CodeName,
                TimeCodeStart = prototypeTransaction.TimeCodeStart,
                TimeCodeStop = prototypeTransaction.TimeCodeStop,
                TimeCodeType = prototypeTransaction.TimeCodeType,
                TimeCodeRegistrationType = prototypeTransaction.TimeCodeRegistrationType,

                //TimeBlock
                TimeBlockId = timeBlockId,

                //TimeBlockDate
                TimeBlockDateId = prototypeTransaction.TimeBlockDateId,
                Date = prototypeTransaction.Date,

                //TimeRule
                TimeRuleId = prototypeTransaction.TimeRuleId,
                TimeRuleName = prototypeTransaction.TimeRuleName,
                TimeRuleSort = prototypeTransaction.TimeRuleSort,

                //AttestState
                AttestStateId = prototypeTransaction.IsScheduleTransaction ? 0 : prototypeTransaction.AttestStateId,
                AttestStateName = prototypeTransaction.IsScheduleTransaction ? string.Empty : prototypeTransaction.AttestStateName,
                AttestStateInitial = !prototypeTransaction.IsScheduleTransaction && prototypeTransaction.AttestStateInitial,
                AttestStateColor = prototypeTransaction.IsScheduleTransaction ? String.Empty : prototypeTransaction.AttestStateColor,
                AttestStateSort = prototypeTransaction.IsScheduleTransaction ? 0 : prototypeTransaction.AttestStateSort,
            };

            // Accounting
            if (setAccounting)
            {
                AddAccountStdToTimeTransactionItem(cloneTimePayrollTransaction, prototypeTransaction);
                AddAccountInternalsToTimeTransactionItem(cloneTimePayrollTransaction, prototypeTransaction);
            }

            return cloneTimePayrollTransaction;
        }

        private TimeTransactionItem CreateTimeTransactionItem(PayrollProduct product, Employee employee, TimeCode timeCode, TimeBlockDate timeBlockDate, AttestStateDTO attestState, decimal quantity, Guid? GuidInternalFK, Guid? GuidTimeBlockFK, TimeBlock timeBlock, SoeTimeTransactionType transactionType, bool setAccounting = true)
        {
            if (product == null)
                return null;

            TimeTransactionItem timePayrollTransaction = new TimeTransactionItem
            {
                GuidInternalFK = GuidInternalFK,
                GuidTimeBlockFK = GuidTimeBlockFK,

                //Transaction
                IsAdded = false,
                IsFixed = false,
                ManuallyAdded = false,
                ReversedDate = null,
                IsReversed = false,
                TransactionType = transactionType,
                Quantity = quantity,
                Comment = "",
                IsScheduleTransaction = false,
                IsVacationReplacement = false,
                IsCentRounding = false,
                IsQuantityRounding = false,
                ScheduleTransactionType = SoeTimePayrollScheduleTransactionType.None,
                IncludedInPayrollProductChain = false,

                //Employee
                EmployeeId = employee.EmployeeId,
                EmployeeName = "",
                EmployeeChildId = null,
                EmployeeChildName = "",

                //Product
                ProductId = product.ProductId,
                ProductNr = product.Number,
                ProductName = product.Name,
                ProductVatType = TermGroup_InvoiceProductVatType.None,
                PayrollProductSysPayrollTypeLevel1 = product.SysPayrollTypeLevel1,
                PayrollProductSysPayrollTypeLevel2 = product.SysPayrollTypeLevel2,
                PayrollProductSysPayrollTypeLevel3 = product.SysPayrollTypeLevel3,
                PayrollProductSysPayrollTypeLevel4 = product.SysPayrollTypeLevel4,

                //TimeCode
                TimeCodeId = timeCode?.TimeCodeId ?? 0,
                Code = timeCode?.Code ?? string.Empty,
                CodeName = timeCode?.Name ?? string.Empty,
                TimeCodeStart = null,
                TimeCodeStop = null,
                TimeCodeType = timeCode != null ? (SoeTimeCodeType)timeCode.Type : SoeTimeCodeType.None,
                TimeCodeRegistrationType = timeCode != null ? (TermGroup_TimeCodeRegistrationType)timeCode.RegistrationType : TermGroup_TimeCodeRegistrationType.Unknown,

                //TimeBlock
                TimeBlockId = timeBlock?.TimeBlockId,

                //TimeBlockDate
                TimeBlockDateId = timeBlockDate?.TimeBlockDateId ?? 0,
                Date = timeBlockDate?.Date,

                //TimeRule
                TimeRuleId = 0,
                TimeRuleName = "",
                TimeRuleSort = 0,

                //AttestState
                AttestStateId = attestState?.AttestStateId ?? 0,
                AttestStateName = attestState?.Name ?? string.Empty,
                AttestStateInitial = attestState?.Initial ?? false,
                AttestStateColor = attestState?.Color ?? string.Empty,
                AttestStateSort = attestState?.Sort ?? 0,
            };

            // Accounting
            if (setAccounting && timeBlockDate != null)
                ApplyAccountingOnTimeTransactionItem(timePayrollTransaction, employee, product, timeBlockDate.Date, timeBlock, setAccountInternal: true);

            return timePayrollTransaction;
        }

        private Dictionary<int, decimal> GetTimePayrollTransactionQuantityPaidByAccountInternal(List<TimePayrollTransaction> timePayrollTransactions, int accountDimId, out List<AccountInternal> accountInternals)
        {
            accountInternals = new List<AccountInternal>();
            var accountInternalsPaidQuantity = new Dictionary<int, decimal>();

            foreach (TimePayrollTransaction timePayrolltransaction in timePayrollTransactions.OrderBy(i => i.TimeBlockDateId))
            {
                AccountInternal accountInternal = timePayrolltransaction.GetAccountInternal(accountDimId);
                if (accountInternal == null)
                    continue;

                if (!accountInternalsPaidQuantity.ContainsKey(accountInternal.AccountId))
                {
                    accountInternalsPaidQuantity.Add(accountInternal.AccountId, 0);
                    accountInternals.Add(accountInternal);
                }

                accountInternalsPaidQuantity[accountInternal.AccountId] += timePayrolltransaction.GetPaidTime();
            }

            return accountInternalsPaidQuantity;
        }

        private DateTime? GetLastPaidTimePayrollTransactionDate(int employeeId)
        {
            DateTime? date = null;
            EmployeeTimePeriod employeeTimePeriod = GetLastPaidEmployeeTimePeriodWithTimePeriod(employeeId);
            if (employeeTimePeriod != null)
                date = employeeTimePeriod.TimePeriod.StopDate;
            TimePayrollTransaction timePayrollTransaction = GetTimePayrollTransactionLastLockedWithTimeBlockDate(employeeId, date);
            return timePayrollTransaction != null ? timePayrollTransaction.TimeBlockDate.Date : date;
        }

        private bool IsAnyTransactionOnDayReversed(int timeBlockDateId, int employeeId, out List<int> reversedSysPayrollTypeLevel3)
        {
            //No need to look at TimePayrollScheduleTransactions. They cannot be reversed without any TimePayrollTransaction is reversed
            reversedSysPayrollTypeLevel3 = GetTimePayrollTransactionsForDay(employeeId, timeBlockDateId).Where(i => i.IsReversed && i.SysPayrollTypeLevel3.HasValue).Select(i => i.SysPayrollTypeLevel3.Value).Distinct().ToList();
            return reversedSysPayrollTypeLevel3.Count > 0;
        }

        private bool CheckAbsenceOnReversedDay(PayrollProduct payrollProduct, TimeBlockDate timeBlockDate, int employeeId)
        {
            if (payrollProduct == null || timeBlockDate == null)
                return false;
            if (base.currentTask != SoeTimeEngineTask.SaveWholedayDeviations && base.currentTask != SoeTimeEngineTask.GenerateDeviationsFromTimeInterval && base.currentTask != SoeTimeEngineTask.ReGenerateDayBasedOnTimeStamps)
                return false;
            if (!IsAnyTransactionOnDayReversed(timeBlockDate.TimeBlockDateId, employeeId, out List<int> reversedSysPayrollTypeLevel3))
                return false;
            if (!IsEmployeeTimePeriodLockedForChanges(employeeId, date: timeBlockDate.Date))
                return false;

            this.SetDoNotCollectDaysForRecalculationLocally(true, payrollProduct, reversedSysPayrollTypeLevel3);
            return true;
        }

        private void SetTimePayrollTransactionQuantity(TimePayrollTransaction timePayrollTransaction, PayrollProduct payrollProduct, Employment employment, TimeBlockDate timeBlockDate, decimal transactionQuantity, List<TimeScheduleTemplateBlock> templateBlocks = null, List<VacationGroup> vacationGroups = null)
        {
            if (timePayrollTransaction == null)
                return;

            timePayrollTransaction.Quantity = transactionQuantity;

            if (timePayrollTransaction.TimePayrollTransactionExtended == null)
                return;

            timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays = 0;
            timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = 0;
            timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = 0;

            if (timePayrollTransaction.IsAdded || payrollProduct == null || employment == null || timeBlockDate == null)
                return;

            PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employment.GetPayrollGroupId(timeBlockDate.Date));
            if (payrollProductSetting == null)
                return;

            timePayrollTransaction.TimePayrollTransactionExtended.TimeUnit = payrollProductSetting.TimeUnit;
            switch (payrollProductSetting.TimeUnit)
            {
                case (int)TermGroup_PayrollProductTimeUnit.WorkDays:
                    #region WorkDays
                    if (timePayrollTransaction.IsVacationFiveDaysPerWeek)
                    {
                        timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays = 1;
                    }
                    else
                    {
                        if (templateBlocks == null)
                            templateBlocks = GetScheduleBlocksFromCache(employment.EmployeeId, timeBlockDate.Date);
                        bool hasSchedule = templateBlocks.GetMinutes() > 0;
                        timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays = hasSchedule ? 1 : 0;
                    }
                    #endregion
                    break;
                case (int)TermGroup_PayrollProductTimeUnit.CalenderDays:
                    #region CalenderDays
                    timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = 1;
                    #endregion
                    break;
                case (int)TermGroup_PayrollProductTimeUnit.CalenderDayFactor:
                    #region CalenderDayFactor
                    decimal? employeeCalendarFactor = UsePayroll() ? GetEmployeeFactor(employment.EmployeeId, TermGroup_EmployeeFactorType.CalendarDayFactor, timeBlockDate.Date) : null;
                    timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = employeeCalendarFactor ?? 0;
                    timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = 1 * timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor;
                    #endregion
                    break;
                case (int)TermGroup_PayrollProductTimeUnit.VacationCoefficient:
                    #region VacationCoefficient
                    decimal? vacationCoefficient = UsePayroll() ? GetEmployeeFactor(employment.EmployeeId, TermGroup_EmployeeFactorType.VacationCoefficient, timeBlockDate.Date) : null;
                    timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = vacationCoefficient ?? 0;
                    timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = 1 * timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor;
                    #endregion
                    break;
                case (int)TermGroup_PayrollProductTimeUnit.Hours:
                    #region Hours
                    if (timePayrollTransaction.IsAbsenceVacationAll())
                    {
                        if (vacationGroups == null)
                            vacationGroups = GetVacationGroupsWithSEFromCache();

                        if (vacationGroups.Any())
                        {
                            var group = employment.GetVacationGroup(timeBlockDate.Date, vacationGroups);
                            if (group != null)
                            {
                                var vacationGroupSE = group.VacationGroupSE.FirstOrDefault();
                                if (vacationGroupSE != null && vacationGroupSE.VacationHandleRule == (int)TermGroup_VacationGroupVacationHandleRule.Hours)
                                {
                                    //VacationGroupVacationHandleRule
                                    EmployeeFactor vacationNet = GetEmployeeFactorFromCache(employment.EmployeeId, TermGroup_EmployeeFactorType.Net, timeBlockDate.Date);
                                    decimal netFactor = vacationNet == null ? 5 : vacationNet.Factor;
                                    var workTimeWeek = employment.GetWorkTimeWeek();
                                    if (workTimeWeek == 0)
                                        break;

                                    decimal dayFactor = decimal.Divide(decimal.Divide(workTimeWeek, new decimal(60)), netFactor);
                                    decimal hoursOnDay = decimal.Divide(timePayrollTransaction.Quantity, new decimal(60));

                                    var quantityDays = decimal.Divide(hoursOnDay, dayFactor);
                                    timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = 0;
                                    timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = quantityDays;
                                }
                            }
                        }
                    }
                    #endregion
                    break;
            }
        }

        private void SetTimePayrollTransactionToVacationFiveDaysPerWeek(TimePayrollTransaction timePayrollTransaction, PayrollProduct payrollProduct, ApplyAbsenceDay applyAbsenceOutput)
        {
            if (timePayrollTransaction == null || payrollProduct == null || applyAbsenceOutput == null)
                return;

            timePayrollTransaction.IsVacationFiveDaysPerWeek = IsVacationFiveDaysPerWeekZeroWeekDay(payrollProduct, applyAbsenceOutput);
        }

        private void SetTimePayrollTransactionType(TimePayrollTransaction timePayrollTransaction, PayrollProduct payrollProduct)
        {
            if (timePayrollTransaction == null || payrollProduct == null)
                return;

            timePayrollTransaction.SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1;
            timePayrollTransaction.SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2;
            timePayrollTransaction.SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3;
            timePayrollTransaction.SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4;
        }

        private void SetTimePayrollTransactionTimeBlock(TimePayrollTransaction timePayrollTransaction, TimeCodeTransaction timeCodeTransaction)
        {
            if (timePayrollTransaction == null || timeCodeTransaction == null)
                return;

            if (timeCodeTransaction.TimeBlockId.HasValue && timeCodeTransaction.TimeBlockId > 0)
            {
                timePayrollTransaction.TimeBlockId = timeCodeTransaction.TimeBlockId;

                if (base.IsEntityAvailableInContext(entities, timeCodeTransaction.TimeBlock) && !timeCodeTransaction.TimeBlockReference.IsLoaded)
                    timeCodeTransaction.TimeBlockReference.Load();
            }
            else if (base.IsEntityAvailableInContext(entities, timeCodeTransaction.TimeBlock))
            {
                timePayrollTransaction.TimeBlock = timeCodeTransaction.TimeBlock;
            }
        }

        private void SetTimePayrollTransactionCurrencyAmounts(TimePayrollTransaction timePayrollTransaction)
        {
            if (timePayrollTransaction == null)
                return;

            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollTransaction);
        }

        private void SetTimePayrollTransactionFormulas(TimePayrollTransaction timePayrollTransaction, PayrollPriceFormulaResultDTO formulaResult)
        {
            if (timePayrollTransaction == null || timePayrollTransaction.TimePayrollTransactionExtended == null || formulaResult == null)
                return;

            if (!timePayrollTransaction.EarningTimeAccumulatorId.HasValue)
            {
                timePayrollTransaction.TimePayrollTransactionExtended.Formula = formulaResult.Formula;
                timePayrollTransaction.TimePayrollTransactionExtended.FormulaPlain = formulaResult.FormulaPlain;
                timePayrollTransaction.TimePayrollTransactionExtended.FormulaExtracted = formulaResult.FormulaExtracted;
                timePayrollTransaction.TimePayrollTransactionExtended.FormulaNames = formulaResult.FormulaNames;
                timePayrollTransaction.TimePayrollTransactionExtended.FormulaOrigin = formulaResult.FormulaOrigin;
            }
            timePayrollTransaction.TimePayrollTransactionExtended.PayrollPriceFormulaId = formulaResult.PayrollPriceFormulaId;
            timePayrollTransaction.TimePayrollTransactionExtended.PayrollPriceTypeId = formulaResult.PayrollPriceTypeId;
            timePayrollTransaction.TimePayrollTransactionExtended.PayrollCalculationPerformed = false;
        }

        private void SetTimePayrollTransactionUnitPriceAndAmounts(TimePayrollTransaction timePayrollTransaction, List<TimePayrollTransaction> timePayrollTransactionsForDayAndProduct, PayrollProduct payrollProduct, TimeBlockDate timeBlockDate, Employment employment, PayrollPriceFormulaResultDTO formulaResult)
        {
            if (timePayrollTransaction == null)
                return;

            if (timePayrollTransaction.IsVacationFiveDaysPerWeek && payrollProduct.IsAbsenceVacation())
            {
                timePayrollTransaction.UnitPrice = 0;
                timePayrollTransaction.Amount = 0;
            }
            else if (timePayrollTransaction.IsVacationFiveDaysPerWeek && payrollProduct.IsVacationSalary())
            {
                timePayrollTransaction.UnitPrice = formulaResult.Amount;
                timePayrollTransaction.Amount = formulaResult.Amount;
            }
            else
            {
                bool fixedAccounting = employment?.FixedAccounting ?? false;
                decimal? unitPrice = timePayrollTransaction.UnitPrice;
                decimal? amount = timePayrollTransaction.Amount;
                decimal transactionQuantity = timePayrollTransaction.Quantity;
                decimal? transactionQuantityCalendarDays = timePayrollTransaction.TimePayrollTransactionExtended != null ? timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays : (decimal?)null;
                decimal? transactionCalenderDayFactor = timePayrollTransaction.TimePayrollTransactionExtended != null ? timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor : (decimal?)null;
                string accountingString = fixedAccounting ? timePayrollTransaction.GetAccountingIdString() : "";
                string accountStdIdString = fixedAccounting ? timePayrollTransaction.AccountStdId.ToString() : "";
                CalculateTimePayrollTransactionUnitPriceAndAmounts(transactionQuantity, transactionQuantityCalendarDays, transactionCalenderDayFactor, (TermGroup_PayrollResultType)payrollProduct.ResultType, timePayrollTransactionsForDayAndProduct, payrollProduct, timeBlockDate, employment, formulaResult, accountingString, accountStdIdString, ref unitPrice, ref amount);
                timePayrollTransaction.UnitPrice = unitPrice;
                timePayrollTransaction.Amount = amount;
            }
        }

        private void CalculateTimePayrollTransactionUnitPriceAndAmounts(decimal transactionQuantity, decimal? transactionQuantityCalendarDays, decimal? transactionCalenderDayFactor, TermGroup_PayrollResultType resultType, List<TimePayrollTransaction> timePayrollTransactionsForDayAndProduct, PayrollProduct payrollProduct, TimeBlockDate timeBlockDate, Employment employment, PayrollPriceFormulaResultDTO formulaResult, string transactionAccountingString, string accountStdIdString, ref decimal? unitPrice, ref decimal? amount)
        {
            if (timePayrollTransactionsForDayAndProduct != null && formulaResult != null)
            {
                decimal formulaAmount = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);
                decimal totalQuantityMinutes = Math.Abs(timePayrollTransactionsForDayAndProduct.Sum(i => i.Quantity));
                decimal totalQuantityHours = totalQuantityMinutes / 60;
                bool vacationSpecial = payrollProduct.IsVacationCompensation() || payrollProduct.IsVacationAdditionOrSalaryPrepaymentPaid() || payrollProduct.IsVacationAdditionOrSalaryVariablePrepaymentPaid();

                if (resultType == TermGroup_PayrollResultType.Time)
                {
                    #region Time

                    PayrollProductSetting payrollProductSetting = employment != null && timeBlockDate != null ? GetPayrollProductSetting(payrollProduct, employment.GetPayrollGroupId(timeBlockDate.Date)) : null;
                    TermGroup_PayrollProductTimeUnit timeUnit = payrollProductSetting != null ? (TermGroup_PayrollProductTimeUnit)payrollProductSetting.TimeUnit : TermGroup_PayrollProductTimeUnit.Hours;
                    switch (timeUnit)
                    {
                        case TermGroup_PayrollProductTimeUnit.Hours:
                            unitPrice = formulaAmount;
                            break;
                        case TermGroup_PayrollProductTimeUnit.WorkDays:
                            if (vacationSpecial)
                                unitPrice = formulaAmount;
                            else
                                unitPrice = totalQuantityHours != 0 ? (formulaAmount / totalQuantityHours) : 0;
                            break;
                        case TermGroup_PayrollProductTimeUnit.CalenderDays:
                            if (vacationSpecial)
                                unitPrice = formulaAmount;
                            else if (totalQuantityMinutes == 0 && transactionQuantityCalendarDays.HasValue && transactionQuantityCalendarDays.Value != 0)
                            {
                                if (employment.HasFixedAccounting() && timePayrollTransactionsForDayAndProduct.Any())
                                {
                                    List<EmploymentAccountStd> accountingSettings = GetEmploymentAccountingFromCache(employment).Where(x => x.IsFixedAccounting).ToList();
                                    EmploymentAccountStd employmentAccountStd = accountingSettings.FirstOrDefault(x => x.GetAccountingIdString(accountStdIdString).Equals(transactionAccountingString));
                                    unitPrice = employmentAccountStd != null ? formulaAmount * decimal.Divide(employmentAccountStd.Percent, 100) : 0;
                                }
                                else
                                {
                                    unitPrice = formulaAmount;
                                }
                            }
                            else
                                unitPrice = totalQuantityHours != 0 ? (formulaAmount / totalQuantityHours) : 0;
                            break;
                        case TermGroup_PayrollProductTimeUnit.CalenderDayFactor:
                            if (vacationSpecial)
                                unitPrice = formulaAmount;
                            else
                                unitPrice = totalQuantityHours != 0 ? ((formulaAmount / totalQuantityHours) * (transactionCalenderDayFactor ?? 0)) : 0;

                            break;
                        case TermGroup_PayrollProductTimeUnit.VacationCoefficient:
                            if (vacationSpecial)
                                unitPrice = formulaAmount;
                            else
                                unitPrice = totalQuantityHours != 0 ? (formulaAmount / totalQuantityHours) : 0;
                            break;
                    }

                    #endregion
                }
                else if (resultType == TermGroup_PayrollResultType.Quantity)
                {
                    #region Quantity

                    unitPrice = formulaAmount;

                    #endregion
                }

                if (unitPrice.HasValue)
                {
                    if (vacationSpecial)//we should use resultType == TermGroup_PayrollResultType.Quantity, but I dont dare
                        amount = Decimal.Round(transactionQuantity * unitPrice.Value, 2, MidpointRounding.AwayFromZero);
                    else if (totalQuantityMinutes == 0 && transactionQuantityCalendarDays.HasValue && transactionQuantityCalendarDays.Value != 0)
                        amount = Decimal.Round(transactionQuantityCalendarDays.Value * unitPrice.Value, 2, MidpointRounding.AwayFromZero);
                    else
                        amount = Decimal.Round((transactionQuantity / 60) * unitPrice.Value, 2, MidpointRounding.AwayFromZero);
                }
            }
        }

        private (decimal unitPriceDiff, decimal amount) CalculateRetroAmount(decimal transactionUnitPrice, decimal retroTransactionUnitPrice, decimal quantity, TermGroup_PayrollResultType resultType)
        {
            decimal unitPriceDiff = retroTransactionUnitPrice - transactionUnitPrice;
            decimal amount = (resultType == TermGroup_PayrollResultType.Time ? quantity / 60 : quantity) * unitPriceDiff;
            return (unitPriceDiff, amount);
        }

        #endregion

        #region TimePayrollScheduleTransaction

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(List<PayrollCalculationProductDTO> payrollCalculationProducts)
        {
            if (payrollCalculationProducts == null)
                return new List<TimePayrollScheduleTransaction>();

            List<int> ids = payrollCalculationProducts.GetScheduleTransactions().Select(i => i.TimePayrollTransactionId).ToList();

            return (from t in entities.TimePayrollScheduleTransaction
                    where ids.Contains(t.TimePayrollScheduleTransactionId)
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(EmployeeTimePeriod employeeTimePeriod)
        {
            if (employeeTimePeriod == null)
                return new List<TimePayrollScheduleTransaction>();

            return (from t in entities.TimePayrollScheduleTransaction
                    where t.EmployeeTimePeriodId == employeeTimePeriod.EmployeeTimePeriodId
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(int employeeId, DateTime dateFrom, DateTime? dateTo, int? timePeriodId, SoeTimePayrollScheduleTransactionType? type = null, bool onlyAlreadyRetro = false)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where (!type.HasValue || t.Type == (int)type.Value) &&
                    t.EmployeeId == employeeId &&
                    (
                        (timePeriodId.HasValue && timePeriodId.Value == t.TimePeriodId)
                            ||
                        (t.TimeBlockDate.Date >= dateFrom && (!dateTo.HasValue || t.TimeBlockDate.Date <= dateTo.Value) && !t.TimePeriodId.HasValue)
                    ) &&
                    (!onlyAlreadyRetro || t.RetroactivePayrollOutcomeId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(List<int> employeeIds, DateTime dateFrom, DateTime? dateTo, int? timePeriodId, SoeTimePayrollScheduleTransactionType? type = null, bool onlyAlreadyRetro = false)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where (!type.HasValue || t.Type == (int)type.Value) &&
                    employeeIds.Contains(t.EmployeeId) &&
                    (
                        (timePeriodId.HasValue && timePeriodId.Value == t.TimePeriodId)
                            ||
                        (t.TimeBlockDate.Date >= dateFrom && (!dateTo.HasValue || t.TimeBlockDate.Date <= dateTo.Value) && !t.TimePeriodId.HasValue)
                    ) &&
                    (!onlyAlreadyRetro || t.RetroactivePayrollOutcomeId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(int employeeId, int timeBlockDateId, SoeTimePayrollScheduleTransactionType? type = null)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where t.TimeBlockDateId == timeBlockDateId &&
                    t.EmployeeId == employeeId &&
                    (!type.HasValue || t.Type == (int)type.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(int employeeId, List<int> timeBlockDateIds, SoeTimePayrollScheduleTransactionType type)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where t.EmployeeId == employeeId &&
                    timeBlockDateIds.Contains(t.TimeBlockDateId) &&
                    t.Type == (int)type &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactions(int employeeId, List<int> timePayrollScheduleTransactions)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    where t.EmployeeId == employeeId &&
                    timePayrollScheduleTransactions.Contains(t.TimePayrollScheduleTransactionId) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsWithTimeBlockDate(int employeeId, List<int> timePayrollScheduleTransactions)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                        .Include("TimeBlockDate")
                    where t.EmployeeId == employeeId &&
                    timePayrollScheduleTransactions.Contains(t.TimePayrollScheduleTransactionId) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsAfterDateWithTimeBlockDate(int employeeId, DateTime date)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                        .Include("TimeBlockDate")
                    where t.EmployeeId == employeeId &&
                    t.State == (int)SoeEntityState.Active &&
                    t.TimeBlockDate.Date > date
                    select t).ToList();
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsWithAccountingAndDim(int employeeId, DateTime dateFrom, DateTime? dateTo, int? timePeriodId, SoeTimePayrollScheduleTransactionType? type = null, bool onlyAlreadyRetro = false)
        {
            return (from t in entities.TimePayrollScheduleTransaction
                    .Include("AccountInternal.Account.AccountDim")
                    where (!type.HasValue || t.Type == (int)type.Value) &&
                    t.EmployeeId == employeeId &&
                    (
                        (timePeriodId.HasValue && timePeriodId.Value == t.TimePeriodId)
                            ||
                        (t.TimeBlockDate.Date >= dateFrom && (!dateTo.HasValue || t.TimeBlockDate.Date <= dateTo.Value) && !t.TimePeriodId.HasValue)
                    ) &&
                    (!onlyAlreadyRetro || t.RetroactivePayrollOutcomeId.HasValue) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }
        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsWithAccountingAndDim(int employeeId, int timeBlockDateId, TimePeriod timePeriod, SoeTimePayrollScheduleTransactionType? type = null)
        {

            if (timePeriod.ExtraPeriod)
            {
                return (from t in entities.TimePayrollScheduleTransaction
                         .Include("AccountInternal.Account.AccountDim")
                        where t.EmployeeId == employeeId &&
                        t.TimeBlockDateId == timeBlockDateId &&
                        t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId &&
                        (!type.HasValue || t.Type == (int)type.Value) &&
                        t.State == (int)SoeEntityState.Active
                        select t).ToList();
            }
            else
            {
                return (from t in entities.TimePayrollScheduleTransaction
                         .Include("AccountInternal.Account.AccountDim")
                        where t.EmployeeId == employeeId &&
                        t.TimeBlockDateId == timeBlockDateId &&
                        (!t.TimePeriodId.HasValue || (t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId)) &&
                        (!type.HasValue || t.Type == (int)type.Value) &&
                        t.State == (int)SoeEntityState.Active
                        select t).ToList();
            }
        }

        private List<TimePayrollScheduleTransaction> GetTimePayrollScheduleTransactionsWithAccounting(TimePeriod timePeriod, DateTime dateFrom, DateTime dateTo, int employeeId)
        {
            //timeperiod can be null

            // Make sure the whole day is covered
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            if (timePeriod != null && timePeriod.ExtraPeriod)
            {
                return (from t in entities.TimePayrollScheduleTransaction
                         .Include("AccountInternal")
                        where t.EmployeeId == employeeId &&
                        t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId &&
                        t.State == (int)SoeEntityState.Active
                        select t).ToList();
            }
            else if (timePeriod != null)
            {
                return (from t in entities.TimePayrollScheduleTransaction
                            .Include("AccountInternal")
                        where t.EmployeeId == employeeId &&
                        ((t.TimeBlockDate.Date >= dateFrom && t.TimeBlockDate.Date <= dateTo && !t.TimePeriodId.HasValue)
                        ||
                        (t.TimePeriodId.HasValue && t.TimePeriodId.Value == timePeriod.TimePeriodId)) &&
                        t.State == (int)SoeEntityState.Active
                        select t).ToList();
            }
            else
            {
                return (from t in entities.TimePayrollScheduleTransaction
                            .Include("AccountInternal")
                        where t.EmployeeId == employeeId &&
                        t.TimeBlockDate.Date >= dateFrom &&
                        t.TimeBlockDate.Date <= dateTo &&
                        t.State == (int)SoeEntityState.Active
                        select t).ToList();
            }
        }

        private List<TimePayrollScheduleTransaction> CreateTimePayrollScheduleTransactions(List<TimeTransactionItem> timeTransactionItems, List<TimeBlock> timeBlocks, List<TimeScheduleTemplateBlock> templateBlocks, TimeBlockDate timeBlockDate, Employee employee, bool doCalculateAmount)
        {
            List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = new List<TimePayrollScheduleTransaction>();
            List<Tuple<Guid, TimePayrollScheduleTransaction>> scheduleTransactionMapping = new List<Tuple<Guid, TimePayrollScheduleTransaction>>();

            if (timeTransactionItems != null)
            {
                foreach (TimeTransactionItem timeTransactionItem in timeTransactionItems)
                {
                    TimePayrollScheduleTransaction timePayrollScheduleTransaction = ConvertToTimePayrollScheduleTransaction(timeTransactionItem, timeBlocks, templateBlocks);
                    if (timePayrollScheduleTransaction != null)
                    {
                        timePayrollScheduleTransactions.Add(timePayrollScheduleTransaction);

                        if (timeTransactionItem.IncludedInPayrollProductChain && !timeTransactionItem.ManuallyAdded && timeTransactionItem.GuidId.HasValue)
                            scheduleTransactionMapping.Add(Tuple.Create(timeTransactionItem.GuidId.Value, timePayrollScheduleTransaction));
                    }
                }

                if (doCalculateAmount)
                    SetTimePayrollScheduleTransactionAmounts(timePayrollScheduleTransactions, employee, timeBlockDate);
            }

            SetPayrollProductChainParent(timeTransactionItems, scheduleTransactionMapping);

            return timePayrollScheduleTransactions;
        }

        private TimePayrollScheduleTransaction ConvertToTimePayrollScheduleTransaction(TimeTransactionItem timeTransactionItem, List<TimeBlock> timeBlocks, List<TimeScheduleTemplateBlock> templateBlocks)
        {
            if (timeTransactionItem == null)
                return null;

            TimeBlock timeBlock = timeBlocks.FirstOrDefault(i => i.GuidId == timeTransactionItem.GuidTimeBlockFK);
            if (timeBlock == null)
                return null;

            TimeScheduleTemplateBlock templateBlock = templateBlocks.FirstOrDefault(i => i.Guid == timeBlock.GuidTemplateBlock);
            if (templateBlock == null)
                return null;

            Product product = GetProductFromCache(timeTransactionItem.ProductId);
            if (product == null)
                return null;

            PayrollProduct payrollProduct = product as PayrollProduct;
            List<AccountInternal> accountInternals = GetAccountInternalsWithAccountFromCache();

            TimePayrollScheduleTransaction timePayrollScheduleTransaction = new TimePayrollScheduleTransaction
            {
                Type = (int)SoeTimePayrollScheduleTransactionType.Schedule,
                Quantity = timeTransactionItem.Quantity,
                Amount = 0,
                AmountCurrency = 0,
                AmountLedgerCurrency = 0,
                AmountEntCurrency = 0,
                VatAmount = 0,
                VatAmountCurrency = 0,
                VatAmountLedgerCurrency = 0,
                VatAmountEntCurrency = 0,
                UnitPrice = 0,
                UnitPriceCurrency = 0,
                UnitPriceLedgerCurrency = 0,
                UnitPriceEntCurrency = 0,
                SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4,
                TimeBlockStartTime = timeBlock.StartTime,
                TimeBlockStopTime = timeBlock.StopTime,
                Formula = null,
                FormulaPlain = null,
                FormulaExtracted = null,
                FormulaNames = null,
                FormulaOrigin = null,
                IncludedInPayrollProductChain = timeTransactionItem.IncludedInPayrollProductChain,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = timeTransactionItem.EmployeeId,
                TimeBlockDateId = timeBlock.TimeBlockDateId,
                ProductId = payrollProduct.ProductId,
                TimeScheduleTemplatePeriodId = templateBlock.TimeScheduleTemplatePeriodId,
                TimeScheduleTemplateBlockId = templateBlock.TimeScheduleTemplateBlockId,
                TimePeriodId = null,
                PayrollPriceFormulaId = null,
                PayrollPriceTypeId = null,
            };
            SetCreatedProperties(timePayrollScheduleTransaction);
            entities.AddToTimePayrollScheduleTransaction(timePayrollScheduleTransaction);

            // Accounting
            ApplyAccountingOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction, timeTransactionItem, accountInternals);

            return timePayrollScheduleTransaction;
        }

        private TimePayrollScheduleTransaction CreateTimePayrollScheduleTransaction(PayrollProduct product, TimeBlockDate timeBlockDate, decimal quantity, decimal? amount, decimal? vatAmount, decimal? unitPrice, int type, int employeeId, int accountId = 0, int? timePeriodId = null, DateTime? timeblockStartTime = null, DateTime? timeBlockStopTime = null)
        {
            if (product == null || timeBlockDate == null)
                return null;

            TimePayrollScheduleTransaction timePayrollScheduleTransaction = new TimePayrollScheduleTransaction
            {
                Type = type,
                Quantity = quantity,
                Amount = amount,
                VatAmount = vatAmount,
                UnitPrice = unitPrice,

                SysPayrollTypeLevel1 = product.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = product.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = product.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = product.SysPayrollTypeLevel4,

                TimeBlockStartTime = timeblockStartTime,
                TimeBlockStopTime = timeBlockStopTime,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                AccountStdId = accountId,
                TimePeriodId = timePeriodId,

                //References
                PayrollProduct = product,
                TimeBlockDate = timeBlockDate,
            };
            SetCreatedProperties(timePayrollScheduleTransaction);
            entities.TimePayrollScheduleTransaction.AddObject(timePayrollScheduleTransaction);

            // Currency
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollScheduleTransaction);

            return timePayrollScheduleTransaction;
        }

        private TimePayrollScheduleTransaction CreateTimePayrollScheduleTransactionReversed(TimePayrollScheduleTransaction prototype, int? timePeriodId, RetroactivePayrollOutcome retroOutcome)
        {
            if (prototype == null)
                return null;

            TimePayrollScheduleTransaction timePayrollScheduleTransaction = new TimePayrollScheduleTransaction
            {
                Type = (int)SoeTimePayrollScheduleTransactionType.Absence,
                Quantity = prototype.Quantity,
                UnitPrice = prototype.UnitPrice,
                Amount = prototype.Amount.HasValue ? Decimal.Negate(prototype.Amount.Value) : (decimal?)null,
                VatAmount = prototype.VatAmount.HasValue ? Decimal.Negate(prototype.VatAmount.Value) : (decimal?)null,
                TimeBlockStartTime = prototype.TimeBlockStartTime,
                TimeBlockStopTime = prototype.TimeBlockStopTime,
                Formula = prototype.Formula,
                FormulaPlain = prototype.FormulaPlain,
                FormulaExtracted = prototype.FormulaExtracted,
                FormulaNames = prototype.FormulaNames,
                FormulaOrigin = prototype.FormulaOrigin,
                IncludedInPayrollProductChain = prototype.IncludedInPayrollProductChain,
                SysPayrollTypeLevel1 = prototype.SysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = prototype.SysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = prototype.SysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = prototype.SysPayrollTypeLevel4,

                //Set FK (from prototype)
                ActorCompanyId = prototype.ActorCompanyId,
                EmployeeId = prototype.EmployeeId,
                TimeBlockDateId = prototype.TimeBlockDateId,
                ProductId = prototype.ProductId,
                AccountStdId = prototype.AccountStdId,
                TimeScheduleTemplatePeriodId = prototype.TimeScheduleTemplatePeriodId,
                TimeScheduleTemplateBlockId = prototype.TimeScheduleTemplateBlockId,
                PayrollPriceFormulaId = prototype.PayrollPriceFormulaId,
                PayrollPriceTypeId = prototype.PayrollPriceTypeId,

                //Set FK (from params)
                TimePeriodId = timePeriodId ?? prototype.TimePeriodId,

                //Set references (from params
                RetroactivePayrollOutcome = retroOutcome,
            };
            SetCreatedProperties(timePayrollScheduleTransaction);
            entities.TimePayrollScheduleTransaction.AddObject(timePayrollScheduleTransaction);

            // Currency
            CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, timePayrollScheduleTransaction);

            if (!prototype.AccountInternal.IsLoaded)
                prototype.AccountInternal.Load();

            foreach (AccountInternal accountInternal in prototype.AccountInternal)
            {
                timePayrollScheduleTransaction.AccountInternal.Add(accountInternal);
            }

            return timePayrollScheduleTransaction;
        }

        private void SetTimePayrollScheduleTransactionUnitAndAmounts(TimePayrollScheduleTransaction timePayrollScheduleTransaction, PayrollProduct payrollProduct, TimeBlockDate timeBlockDate, Employment employment, PayrollPriceFormulaResultDTO formulaResult)
        {
            if (timePayrollScheduleTransaction != null && formulaResult != null && employment != null && timeBlockDate != null)
            {
                timePayrollScheduleTransaction.UnitPrice = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);

                if (payrollProduct.ResultType == (int)TermGroup_PayrollResultType.Time)
                {
                    timePayrollScheduleTransaction.Amount = Decimal.Round(((timePayrollScheduleTransaction.Quantity / 60) * (timePayrollScheduleTransaction.UnitPrice.Value)), 2, MidpointRounding.AwayFromZero);
                }
                else if (payrollProduct.ResultType == (int)TermGroup_PayrollResultType.Quantity)
                {
                    timePayrollScheduleTransaction.Amount = Decimal.Round(((timePayrollScheduleTransaction.Quantity) * (timePayrollScheduleTransaction.UnitPrice.Value)), 2, MidpointRounding.AwayFromZero);
                }
            }
        }

        #endregion

        #region TimePeriodHead

        private TimePeriodHead GetTimePeriodHeadWithPeriods(int timePeriodHeadId)
        {
            return (from tph in entities.TimePeriodHead
                        .Include("TimePeriod")
                    where tph.TimePeriodHeadId == timePeriodHeadId &&
                    tph.ActorCompanyId == actorCompanyId
                    select tph).FirstOrDefault();
        }

        private TimePeriodHead GetDefaultTimePeriodHeadWithPeriods(int? payrollGroupId)
        {
            TimePeriodHead timePeriodHead = null;

            //Prio 1: PayrollGroup
            PayrollGroup payrollGroup = payrollGroupId.HasValue && payrollGroupId.Value > 0 ? GetPayrollGroupWithTimePeriod(payrollGroupId.Value) : null;
            if (payrollGroup != null && payrollGroup.TimePeriodHead != null && payrollGroup.TimePeriodHead.State == (int)SoeEntityState.Active && payrollGroup.TimePeriodHead.TimePeriod != null)
                timePeriodHead = payrollGroup.TimePeriodHead;

            //Prio 2: Company
            if (timePeriodHead == null)
            {
                int defaultTimePeriodHeadId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimePeriodHead);
                if (defaultTimePeriodHeadId > 0)
                    timePeriodHead = GetTimePeriodHeadWithPeriods(defaultTimePeriodHeadId);
            }

            return timePeriodHead;
        }

        private int? GetTimePeriodHeadId(int timePeriodId)
        {
            return entities.TimePeriod.Include("TimePeriodHead").FirstOrDefault(x => x.TimePeriodId == timePeriodId && x.TimePeriodHead.ActorCompanyId == ActorCompanyId)?.TimePeriodHead?.TimePeriodHeadId;
        }

        #endregion

        #region TimePeriod

        private List<TimePeriod> GetTimePeriods(int timePeriodHeadId)
        {
            return (from tp in entities.TimePeriod
                    where tp.TimePeriodHead.TimePeriodHeadId == timePeriodHeadId &&
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.TimePeriodHead.State == (int)SoeEntityState.Active &&
                    tp.State == (int)SoeEntityState.Active
                    select tp).ToList();
        }

        private List<TimePeriod> GetUpcomingOpenperiods(Employee employee, DateTime currentDate)
        {
            PayrollGroup payrollGroup = employee.GetPayrollGroup(currentDate, GetPayrollGroupsFromCache());
            if (payrollGroup == null || !payrollGroup.TimePeriodHeadId.HasValue)
                return new List<TimePeriod>();

            TimePeriodHead timeperiodHead = GetTimePeriodHeadWithPeriods(payrollGroup.TimePeriodHeadId.Value);
            if (timeperiodHead == null)
                return new List<TimePeriod>();

            List<TimePeriod> upcomingPeriods = timeperiodHead.TimePeriod.Where(x => x.PaymentDate.HasValue && x.PaymentDate >= currentDate && x.State == (int)SoeEntityState.Active).ToList();
            List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriods(upcomingPeriods.Select(x => x.TimePeriodId).ToList(), employee.EmployeeId);
            List<TimePeriod> openperiods = new List<TimePeriod>();
            foreach (TimePeriod timePeriod in upcomingPeriods)
            {
                bool valid = true;
                EmployeeTimePeriod employeeTimePeriod = employeeTimePeriods.FirstOrDefault(i => i.TimePeriodId == timePeriod.TimePeriodId);
                if (employeeTimePeriod != null && employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Open)
                    valid = false;

                if (valid)
                    openperiods.Add(timePeriod);
            }

            return openperiods;
        }

        private TimePeriod GetTimePeriod(int timePeriodId)
        {
            return (from tp in entities.TimePeriod
                    where tp.TimePeriodId == timePeriodId &&
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.TimePeriodHead.State == (int)SoeEntityState.Active &&
                    tp.State == (int)SoeEntityState.Active
                    select tp).FirstOrDefault();
        }

        private TimePeriod GetTimePeriodParent(int childId, DateTime stopDate)
        {
            return (from tp in entities.TimePeriod
                    where tp.StopDate == stopDate &&
                    tp.State == (int)SoeEntityState.Active &&
                    tp.TimePeriodHead.ChildId.HasValue &&
                    tp.TimePeriodHead.ChildId.Value == childId &&
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId &&
                    tp.TimePeriodHead.State == (int)SoeEntityState.Active
                    select tp).FirstOrDefault();
        }

        private TimePeriod GetTimePeriodWithHead(int timePeriodId)
        {
            return (from tp in entities.TimePeriod
                        .Include("TimePeriodHead")
                    where tp.TimePeriodId == timePeriodId &&
                    tp.TimePeriodHead.ActorCompanyId == actorCompanyId
                    select tp).FirstOrDefault();
        }

        private TimePeriod GetNextOpenPeriodForEmployee(Employee employee, DateTime date)
        {
            Employment employment = employee?.GetFirstEmployment();
            if (employment == null)
                return null;

            DateTime? employmentDate = employment.GetEmploymentDate();
            if (!employmentDate.HasValue)
                return null;

            if (date < employmentDate.Value)
                date = employmentDate.Value;

            TimePeriod timePeriod = GetLatestTimePeriodForEmployee(employee, date, date.AddMonths(1).Month);
            bool firstTryWasNotNull = timePeriod != null;

            if (timePeriod != null)
            {
                EmployeeTimePeriod employeeTimePeriod = GetEmployeeTimePeriod(timePeriod.TimePeriodId, employee.EmployeeId);
                if (employeeTimePeriod != null && employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Open)
                    return timePeriod;

                DateTime currentDate = date.AddMonths(1);

                timePeriod = GetLatestTimePeriodForEmployee(employee, currentDate, currentDate.AddMonths(1).Month);
                if (timePeriod != null)
                    employeeTimePeriod = GetEmployeeTimePeriod(timePeriod.TimePeriodId, employee.EmployeeId);

                if (employeeTimePeriod != null && employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Open)
                    return timePeriod;

                currentDate = date.AddMonths(2);

                timePeriod = GetLatestTimePeriodForEmployee(employee, currentDate, currentDate.AddMonths(1).Month);
                if (timePeriod != null)
                    employeeTimePeriod = GetEmployeeTimePeriod(timePeriod.TimePeriodId, employee.EmployeeId);

                if (employeeTimePeriod != null && employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Open)
                    return timePeriod;
            }

            if (firstTryWasNotNull)
                return GetLatestTimePeriodForEmployee(employee, date, date.AddMonths(1).Month);
            else
                return null;
        }

        private TimePeriod GetTimePeriodForEmployee(Employee employee, DateTime date)
        {
            Employment employment = employee.GetEmployment(date);
            PayrollGroup payrollGroup = employment?.GetPayrollGroup(date, GetPayrollGroupsFromCache());
            if (payrollGroup?.TimePeriodHeadId == null)
                return null;

            return GetTimePeriodsFromCache(payrollGroup.TimePeriodHeadId.Value)
                .Where(tp => !tp.ExtraPeriod)
                .FirstOrDefault(t => CalendarUtility.IsDateInRange(date, t.StartDate, t.StopDate));
        }

        private TimePeriod GetLatestTimePeriodForEmployee(Employee employee, DateTime date, int nextMonth)
        {
            TimePeriod timePeriod = null;

            //kontantprincipen
            if (nextMonth > 1)
                nextMonth--;
            else
                nextMonth = 12;

            int year = date.Month > nextMonth ? date.AddYears(1).Year : date.Year;
            DateTime dateFrom = new DateTime(year, nextMonth, 1);
            DateTime dateTo = CalendarUtility.GetEndOfMonth(dateFrom).Date;

            Employment employment = employee.GetEmployment(dateFrom, dateTo, forward: false);
            if (employment != null)
            {
                PayrollGroup payrollGroup = employment.GetPayrollGroup(dateFrom, dateTo, GetPayrollGroupsFromCache(), forward: false);
                if (payrollGroup != null && payrollGroup.TimePeriodHeadId.HasValue)
                {
                    List<TimePeriod> timePeriods = GetTimePeriodsFromCache(payrollGroup.TimePeriodHeadId.Value).Where(tp => !tp.ExtraPeriod).ToList();
                    if (!timePeriods.IsNullOrEmpty())
                    {
                        timePeriod = date.Date != CalendarUtility.GetLastDateOfMonth(date).Date ? timePeriods.FirstOrDefault(tp => tp.StartDate <= date && tp.StopDate >= date) : null;

                        DateTime currentDate = dateTo.Date;
                        while (currentDate >= dateFrom && timePeriod == null)
                        {
                            timePeriod = timePeriods.FirstOrDefault(tp => tp.StartDate <= currentDate && tp.StopDate >= currentDate);
                            currentDate = currentDate.AddDays(-1);
                        }
                    }
                }
            }

            return timePeriod;
        }

        private TimePeriod GetTimePeriod(Employee employee, DateTime paymentDate, List<TimePeriodHead> timePeriodHeadCache)
        {
            TimePeriod timePeriod = null;
            PayrollGroup payrollGroup = employee.GetPayrollGroup(paymentDate, GetPayrollGroupsFromCache()) ?? employee.GetLastPayrollGroup();
            if (payrollGroup == null)
                return null;

            int timePeriodHeadId = payrollGroup.TimePeriodHeadId ?? GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimePeriodHead);
            if (timePeriodHeadId == 0)
                return null;

            TimePeriodHead timePeriodHead = timePeriodHeadCache.FirstOrDefault(x => x.TimePeriodHeadId == timePeriodHeadId);
            if (timePeriodHead == null)
            {
                timePeriodHead = GetTimePeriodHeadWithPeriods(timePeriodHeadId);
                if (timePeriodHead != null)
                    timePeriodHeadCache.Add(timePeriodHead);
            }
            if (timePeriodHead == null)
                return null;

            timePeriod = timePeriodHead.TimePeriod.Where(x => x.State == (int)SoeEntityState.Active).ToList().GetTimePeriod(paymentDate);
            if (timePeriod == null)
                return null;

            return timePeriod;
        }
        private int? GetGivenOrNextOpenTimePeriodId(int? timePeriodId, DateTime? date, int employeeId)
        {
            if (!UsePayroll())
                return null;
            return GetGivenOrNextOpenTimePeriodId(timePeriodId, date, GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId));
        }

        private int? GetGivenOrNextOpenTimePeriodId(int? timePeriodId, DateTime? date, Employee employee)
        {
            if (!UsePayroll())
                return null;

            int? selectedTimePeriodId = null;
            if (timePeriodId.HasValue && timePeriodId.Value != 0)
                selectedTimePeriodId = timePeriodId.Value;
            else if (date.HasValue)
                selectedTimePeriodId = GetNextOpenPeriodForEmployee(employee, date.Value.Date)?.TimePeriodId;

            return selectedTimePeriodId;
        }

        #endregion

        #region TimePeriodAccountValue

        private List<TimePeriodAccountValue> GetTimePeriodAccountValues(int timePeriodId)
        {
            return (from t in entities.TimePeriodAccountValue
                    where t.TimePeriodId == timePeriodId &&
                    t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private TimePeriodAccountValue GetTimePeriodAccountValue(int timePeriodAccountValueId)
        {
            return (from e in entities.TimePeriodAccountValue
                    where e.ActorCompanyId == actorCompanyId &&
                    e.TimePeriodAccountValueId == timePeriodAccountValueId &&
                    e.State == (int)SoeEntityState.Active
                    select e).FirstOrDefault();
        }

        private ActionResult CreateOrUpdateTimePeriodAccountValue(TimePeriodAccountValueDTO timePeriodAccountValueInput)
        {
            ActionResult result = new ActionResult(true);

            TimePeriodAccountValue timePeriodAccountValue;
            if (timePeriodAccountValueInput.TimePeriodAccountValueId > 0)
            {
                timePeriodAccountValue = GetTimePeriodAccountValue(timePeriodAccountValueInput.TimePeriodAccountValueId);
                if (timePeriodAccountValue == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePeriodAccountValue");

                SetModifiedProperties(timePeriodAccountValue);
            }
            else
            {
                timePeriodAccountValue = new TimePeriodAccountValue()
                {
                    ActorCompanyId = timePeriodAccountValueInput.ActorCompanyId,
                    TimePeriodId = timePeriodAccountValueInput.TimePeriodId,
                    AccountId = timePeriodAccountValueInput.AccountId,
                    Type = (int)timePeriodAccountValueInput.Type,
                    Status = (int)timePeriodAccountValueInput.Status,
                };
                SetCreatedProperties(timePeriodAccountValue);
                entities.TimePeriodAccountValue.AddObject(timePeriodAccountValue);
            }

            timePeriodAccountValue.Value = timePeriodAccountValueInput.Value;

            result.Value = timePeriodAccountValue;
            return result;
        }

        private ActionResult SetTimePeriodAccountValueStatus(int timePeriodId, SoeTimePeriodAccountValueStatus status)
        {
            ActionResult result = new ActionResult(true);

            // Get all TimePeriodAccountValues for specified period
            List<TimePeriodAccountValue> timePeriodAccountValues = GetTimePeriodAccountValues(timePeriodId);

            // Exclude current status
            timePeriodAccountValues = timePeriodAccountValues.Where(t => t.Status != (int)status).ToList();

            foreach (TimePeriodAccountValue timePeriodAccountValue in timePeriodAccountValues.Where(t => t.Status != (int)status))
            {
                timePeriodAccountValue.Status = (int)status;
                SetModifiedProperties(timePeriodAccountValue);
            }

            return result;
        }

        #endregion

        #region TimeRule

        private TimeRule CreateInternalAbsenceTimeRule(TimeDeviationCause timeDeviationCause, int dayTypeId)
        {
            if (timeDeviationCause == null || timeDeviationCause.TimeCode == null)
                return null;

            TimeRule defaultAbsenceTimeRule = new TimeRule
            {
                Name = GetText(5560, "Standard frånvaroregel"),
                Factor = 1,
                Internal = true,
                Type = (int)SoeTimeRuleType.Absence,
                RuleStartDirection = (int)SoeTimeRuleDirection.Forward,

                //Set FK
                ActorCompanyId = actorCompanyId,

                //Set references
                TimeCode = timeDeviationCause.TimeCode,
            };
            SetCreatedProperties(defaultAbsenceTimeRule);

            //Add TimeRuleRow
            TimeRuleManager.CreateTimeRuleRow(defaultAbsenceTimeRule, timeDeviationCause.TimeDeviationCauseId, dayTypeId, null, null, actorCompanyId);

            //Start
            TimeRuleExpression startExpression = new TimeRuleExpression
            {
                IsStart = true,
            };
            TimeRuleOperand startOperand = new TimeRuleOperand
            {
                OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn,
            };
            startExpression.TimeRuleOperand.Add(startOperand);
            defaultAbsenceTimeRule.TimeRuleExpression.Add(startExpression);

            //Stop
            TimeRuleExpression stopExpression = new TimeRuleExpression()
            {
                IsStart = false,
            };
            TimeRuleOperand stopOperand = new TimeRuleOperand
            {
                OperatorType = (int)SoeTimeRuleOperatorType.TimeRuleOperatorScheduleOut,
            };
            stopExpression.TimeRuleOperand.Add(stopOperand);
            defaultAbsenceTimeRule.TimeRuleExpression.Add(stopExpression);

            return defaultAbsenceTimeRule;
        }

        #endregion

        #region TimeScheduleTemplateHead

        private List<TimeScheduleTemplateHead> GetPersonalTemplateHeads(int employeeId)
        {
            return (from th in entities.TimeScheduleTemplateHead
                    where th.Company.ActorCompanyId == actorCompanyId &&
                    th.EmployeeId == employeeId &&
                    th.StartDate.HasValue &&
                    th.State == (int)SoeEntityState.Active
                    select th).ToList();
        }

        private List<TimeScheduleTemplateHead> GetPersonalTemplateHeads(List<int> employeeIds)
        {
            return (from th in entities.TimeScheduleTemplateHead
                    where th.Company.ActorCompanyId == actorCompanyId &&
                    th.EmployeeId.HasValue &&
                    employeeIds.Contains(th.EmployeeId.Value) &&
                    th.StartDate.HasValue &&
                    th.State == (int)SoeEntityState.Active
                    select th).ToList();
        }

        private List<TimeScheduleTemplateHead> GetPersonalTemplateHeads(int employeeId, DateTime? startDate, DateTime? stopDate)
        {
            var allTemplateHeads = GetPersonalTemplateHeads(employeeId);
            return FilterPersonalTemplateHeads(allTemplateHeads, startDate, stopDate);
        }

        private List<TimeScheduleTemplateHead> FilterPersonalTemplateHeads(List<TimeScheduleTemplateHead> allPersonalTemplateHeads, DateTime? startDate, DateTime? stopDate)
        {
            var validTemplateHeads = new List<TimeScheduleTemplateHead>();

            //Remove non-personal
            allPersonalTemplateHeads = allPersonalTemplateHeads.Where(i => i.EmployeeId.HasValue && i.StartDate.HasValue).OrderBy(i => i.StartDate).ToList();
            if (!allPersonalTemplateHeads.IsNullOrEmpty())
            {
                if (!startDate.HasValue)
                    startDate = DateTime.MinValue;
                if (!stopDate.HasValue)
                    stopDate = DateTime.MaxValue;

                TimeScheduleTemplateHead previousTemplateHead = null;
                for (int i = allPersonalTemplateHeads.Count - 1; i >= 0; i--)
                {
                    TimeScheduleTemplateHead personalTemplateHead = allPersonalTemplateHeads[i];
                    if (!personalTemplateHead.StartDate.HasValue)
                        continue;

                    DateTime? templateHeadStop = personalTemplateHead.StopDate ?? previousTemplateHead?.StartDate;
                    if (!templateHeadStop.HasValue)
                        templateHeadStop = DateTime.MaxValue;

                    if (CalendarUtility.IsDatesOverlapping(startDate.Value, stopDate.Value, personalTemplateHead.StartDate.Value.Date, templateHeadStop.Value))
                        validTemplateHeads.Add(personalTemplateHead);

                    previousTemplateHead = personalTemplateHead;
                }
            }

            return validTemplateHeads;
        }

        private TimeScheduleTemplateHead GetPersonalTemplateHead(int employeeId, DateTime startDate)
        {
            return (from th in entities.TimeScheduleTemplateHead
                    where th.Company.ActorCompanyId == actorCompanyId &&
                    th.EmployeeId == employeeId &&
                    (th.StartDate.HasValue && th.StartDate.Value == startDate) &&
                    th.State == (int)SoeEntityState.Active
                    select th).FirstOrDefault();
        }

        private TimeScheduleTemplateHead GetEmployeePostTemplateHead(int employeePostId, DateTime date)
        {
            return (from th in entities.TimeScheduleTemplateHead
                    where th.Company.ActorCompanyId == actorCompanyId &&
                    th.EmployeePostId == employeePostId &&
                    (th.StartDate.HasValue && th.StartDate.Value <= date) &&
                    th.State == (int)SoeEntityState.Active
                    select th).OrderByDescending(x => x.StartDate).FirstOrDefault();
        }

        private TimeScheduleTemplateHead GetTimeScheduleTemplateHead(int templateHeadId, bool onlyActive = false)
        {
            if (templateHeadId == 0)
                return null;

            return (from th in entities.TimeScheduleTemplateHead
                    where th.TimeScheduleTemplateHeadId == templateHeadId &&
                    (!onlyActive || th.State == (int)SoeEntityState.Active)
                    select th).FirstOrDefault();
        }

        private TimeScheduleTemplateHead GetEmployeeTimeScheduleTemplateHeadForEmployeePost(int employeePostId, DateTime startDate, bool loadAccounts = false)
        {
            TimeScheduleTemplateHead templateHead = (from th in entities.TimeScheduleTemplateHead
                                                     where th.EmployeePostId == employeePostId &&
                                                     th.EmployeeId.HasValue &&
                                                     th.State == (int)SoeEntityState.Active &&
                                                     th.StartDate == startDate
                                                     select th).FirstOrDefault();

            if (templateHead != null && loadAccounts)
                LoadTimeScheduleTemplateHeadAccounts(templateHead);

            return templateHead;
        }

        private TimeScheduleTemplateHead GetTimeScheduleTemplateHeadWithPeriods(int templateHeadId)
        {
            return (from th in entities.TimeScheduleTemplateHead
                        .Include("TimeScheduleTemplatePeriod")
                    where th.TimeScheduleTemplateHeadId == templateHeadId
                    select th).FirstOrDefault();
        }

        private TimeScheduleTemplateHead GetTimeScheduleTemplateHeadWithPeriodsBlocksAndTasks(int templateHeadId, bool loadAccounts)
        {
            TimeScheduleTemplateHead templateHead = (from th in entities.TimeScheduleTemplateHead
                                                        .Include("TimeScheduleTemplatePeriod.TimeScheduleTemplateBlock.TimeScheduleTemplateBlockTask")
                                                     where th.TimeScheduleTemplateHeadId == templateHeadId &&
                                                     th.State == (int)SoeEntityState.Active
                                                     select th).FirstOrDefault();

            if (templateHead != null && loadAccounts)
                LoadTimeScheduleTemplateHeadAccounts(templateHead);

            return templateHead;
        }

        private TimeScheduleTemplateHead GetTimeScheduleTemplateHeadWithPeriodsAndActiveBlocks(int templateHeadId, bool loadEmployeeSchedule, bool loadAccounts)
        {
            TimeScheduleTemplateHead templateHead = null;

            //Need to load it bottom up since only some blocks should be included

            #region TimeScheduleTemplateBlock

            var templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                        .Include("TimeScheduleTemplateBlockTask")
                                        .Include("TimeScheduleTemplatePeriod")
                                        .Include("TimeScheduleTemplatePeriod.TimeScheduleTemplateHead")
                                        .Include("TimeCode")
                                        .Include("Employee")
                                        .Include("ShiftType")
                                  where tb.TimeScheduleTemplatePeriod.TimeScheduleTemplateHeadId == templateHeadId &&
                                  tb.Date == null &&
                                  tb.State == (int)SoeEntityState.Active
                                  select tb).ToList();

            templateBlocks = templateBlocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();


            #endregion

            #region TimeScheduleTemplateHead

            if (templateBlocks.Any())
            {
                templateHead = templateBlocks.First().TimeScheduleTemplatePeriod.TimeScheduleTemplateHead;
                if (!templateHead.TimeScheduleTemplatePeriod.IsLoaded)
                    templateHead.TimeScheduleTemplatePeriod.Load();
            }
            else
            {
                templateHead = (from h in entities.TimeScheduleTemplateHead
                                    .Include("TimeScheduleTemplatePeriod.TimeScheduleTemplateBlock")
                                where h.TimeScheduleTemplateHeadId == templateHeadId &&
                                h.State != (int)SoeEntityState.Deleted
                                select h).FirstOrDefault();
            }

            if (templateHead == null || templateHead.State == (int)SoeEntityState.Deleted)
                return null;

            #endregion

            #region EmployeeSchedule

            if (loadEmployeeSchedule && !templateHead.EmployeeSchedule.IsLoaded)
                templateHead.EmployeeSchedule.Load();

            #endregion

            #region Accounts

            if (loadAccounts)
                LoadTimeScheduleTemplateHeadAccounts(templateHead);

            #endregion

            return templateHead;
        }

        private TimeScheduleTemplateHead GetFirstPersonalTemplate(List<TimeScheduleTemplateHead> templateHeads)
        {
            TimeScheduleTemplateHead templateHead = templateHeads.FirstOrDefault(i => !i.StartDate.HasValue) ?? templateHeads.OrderBy(i => i.StartDate).FirstOrDefault();
            return templateHead;
        }

        private DateTime? GetFirstPersonalTemplateStartDate(List<TimeScheduleTemplateHead> templateHeads)
        {
            return GetFirstPersonalTemplate(templateHeads)?.StartDate;
        }

        private bool HasPersonalTemplateHeadsWithSameStartDate(List<TimeScheduleTemplateHead> templateHeads)
        {
            List<TimeScheduleTemplateHead> validTemplateHeads = new List<TimeScheduleTemplateHead>();
            foreach (TimeScheduleTemplateHead templateHead in templateHeads.Where(i => i.EmployeeId.HasValue && i.StartDate.HasValue && i.NoOfDays > 0))
            {
                foreach (TimeScheduleTemplateHead validTemplateHead in validTemplateHeads)
                {
                    if (templateHead.StartDate.Value.Date == validTemplateHead.StartDate.Value.Date)
                        return true;
                }

                validTemplateHeads.Add(templateHead);
            }

            return false;
        }

        private bool IsTemplateHeadUsed(int templateHeadId)
        {
            return (from es in entities.EmployeeSchedule
                    where es.TimeScheduleTemplateHeadId == templateHeadId &&
                    es.State == (int)SoeEntityState.Active
                    select es).Any();
        }

        private void LoadTimeScheduleTemplateHeadAccounts(TimeScheduleTemplateHead templateHead)
        {
            if (templateHead == null)
                return;

            List<TimeScheduleTemplatePeriod> templatePeriodsForHead = templateHead.TimeScheduleTemplatePeriod.Where(p => p.State == (int)SoeEntityState.Active).OrderBy(i => i.DayNumber).ToList();
            foreach (TimeScheduleTemplatePeriod templatePeriod in templatePeriodsForHead)
            {
                List<TimeScheduleTemplateBlock> templateBlocksForPeriod = templatePeriod.TimeScheduleTemplateBlock.Where(b => b.State == (int)SoeEntityState.Active).ToList();
                foreach (TimeScheduleTemplateBlock templateBlock in templateBlocksForPeriod)
                {
                    if (!templateBlock.AccountInternal.IsLoaded)
                        templateBlock.AccountInternal.Load();

                    foreach (AccountInternal accountInternal in templateBlock.AccountInternal)
                    {
                        if (accountInternal.AccountReference != null && !accountInternal.AccountReference.IsLoaded)
                        {
                            accountInternal.AccountReference.Load();

                            if (accountInternal.Account.AccountDimReference != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                                accountInternal.Account.AccountDimReference.Load();
                        }
                    }
                }
            }
        }

        private ActionResult ValidatePersonalTemplates(int timeScheduleTemplateHeadId, int employeeId, DateTime startDate, DateTime? stopDate)
        {
            ActionResult result = new ActionResult(true);

            if (stopDate.HasValue)
            {
                #region Check overlaping templates and placements

                List<TimeScheduleTemplateHeadDTO> personalTemplates = GetPersonalTemplateHeads(employeeId).ToDTOs(false, false, false, false, false, false);

                if (timeScheduleTemplateHeadId == 0)
                {
                    personalTemplates.Add(new TimeScheduleTemplateHeadDTO()
                    {
                        EmployeeId = employeeId,
                        TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId,
                        StartDate = startDate,
                        StopDate = stopDate,
                    });
                }
                else
                {
                    var currentPersonalTemplate = personalTemplates.FirstOrDefault(i => i.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId);
                    if (currentPersonalTemplate != null)
                    {
                        currentPersonalTemplate.StartDate = startDate;
                        currentPersonalTemplate.StopDate = stopDate;
                    }
                }

                //Sort
                personalTemplates = personalTemplates.Where(i => i.StartDate.HasValue).OrderBy(i => i.StartDate).ToList();

                //Set StopDate if missing
                TimeScheduleTemplateHeadDTO lastPersonalTemplate = null;
                for (int i = personalTemplates.Count - 1; i >= 0; i--)
                {
                    var currentPersonalTemplate = personalTemplates[i];
                    if (!currentPersonalTemplate.StopDate.HasValue && lastPersonalTemplate != null)
                        currentPersonalTemplate.StopDate = lastPersonalTemplate.StartDate.Value.AddDays(-1);

                    lastPersonalTemplate = currentPersonalTemplate;
                }

                for (int i = 0; i < personalTemplates.Count; i++)
                {
                    TimeScheduleTemplateHeadDTO currentPersonalTemplate = personalTemplates[i];

                    DateTime currentStart = currentPersonalTemplate.StartDate.Value;
                    DateTime currentStop = currentPersonalTemplate.StopDate ?? DateTime.MaxValue;

                    //Check overlaping templates
                    if (currentPersonalTemplate.TimeScheduleTemplateHeadId != timeScheduleTemplateHeadId && CalendarUtility.IsDatesOverlapping(startDate, stopDate.Value, currentStart, currentStop, validateDatesAreTouching: true))
                        return new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeeTemplateOverlapping, GetText(3397, "Grundschemat överlappar ett befintligt grundschema på den anställde"));

                    //Check existing placements
                    if (currentPersonalTemplate.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId)
                    {
                        List<EmployeeSchedule> placementsForSchedule = GetEmployeeScheduleForTemplateHeadId(currentPersonalTemplate.TimeScheduleTemplateHeadId);
                        if (placementsForSchedule.Any(p => p.StopDate > currentStop))
                            return new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeePlacementsOutOfRange, GetText(3398, "Det finns aktiverade scheman på grundschemat med slutdatum efter grundschemats slutdatum"));
                    }
                }

                #endregion
            }
            else
            {
                #region Check if template exists

                var personalTemplate = GetPersonalTemplateHead(employeeId, startDate).ToDTO(false, false, false, false, false, false);
                if (personalTemplate != null && personalTemplate.TimeScheduleTemplateHeadId != timeScheduleTemplateHeadId)
                    return new ActionResult((int)ActionResultSave.TimeScheduleTemplateEmployeeTemplateExists, GetText(3396, "Den anställde har redan ett grundschema med samma starttdatum"));

                #endregion
            }

            return result;
        }

        #endregion

        #region TimeScheduleTemplateGroup

        private List<TimeScheduleTemplateGroupDTO> GetTimeScheduleTemplateGroups(List<int> employeeIds)
        {
            List<TimeScheduleTemplateGroup> templateGroups =
                (from th in entities.TimeScheduleTemplateGroup
                 where th.Company.ActorCompanyId == actorCompanyId &&
                 th.TimeScheduleTemplateGroupEmployee.Any(a => employeeIds.Contains(a.EmployeeId)) &&
                 th.TimeScheduleTemplateGroupEmployee.Any(a => a.State == (int)SoeEntityState.Active) &&
                 th.State == (int)SoeEntityState.Active
                 select th).ToList();

            return templateGroups.ToDTOs();
        }

        #endregion

        #region TimeScheduleTemplatePeriod

        private List<TimeScheduleTemplatePeriod> GetTimeScheduleTemplatePeriods(int templateHeadId)
        {
            return (from t in entities.TimeScheduleTemplatePeriod
                    where t.TimeScheduleTemplateHeadId == templateHeadId &&
                    t.State == (int)SoeEntityState.Active
                    orderby t.DayNumber
                    select t).ToList();
        }

        private TimeScheduleTemplatePeriod GetTimeScheduleTemplatePeriod(int templatePeriodId)
        {
            return (from t in entities.TimeScheduleTemplatePeriod
                    where t.TimeScheduleTemplatePeriodId == templatePeriodId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private Dictionary<DateTime, TimeScheduleTemplatePeriod> GetTimeScheduleTemplatePeriodsGroupedByDate(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            var templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                    .Include("TimeScheduleTemplatePeriod")
                                  where tb.EmployeeId == employeeId &&
                                  tb.Date.HasValue && tb.Date.Value >= dateFrom && tb.Date.Value <= dateTo &&
                                  tb.State == (int)SoeEntityState.Active &&
                                  tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None
                                  select new
                                  {
                                      tb.Date,
                                      Period = tb.TimeScheduleTemplatePeriod,
                                      tb.TimeScheduleScenarioHeadId
                                  }
                               ).ToList();

            templateBlocks = templateBlocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();

            return templateBlocks
                .Where(x => x.Date.HasValue && x.Period != null)
                .GroupBy(x => x.Date.Value)
                .ToDictionary(x => x.Key, x => x.First().Period);
        }

        private TimeScheduleTemplatePeriod GetTimeScheduleTemplatePeriod(int employeeId, DateTime date, bool checkEmployeeSchedule = true)
        {
            var periods = (from tb in entities.TimeScheduleTemplateBlock
                            .Include("TimeScheduleTemplatePeriod")
                           where !tb.TimeScheduleScenarioHeadId.HasValue &&
                           tb.TimeScheduleTemplatePeriodId.HasValue &&
                           tb.EmployeeId == employeeId &&
                           tb.Date == date &&
                           tb.State == (int)SoeEntityState.Active &&
                           tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None
                           select tb.TimeScheduleTemplatePeriod).Distinct().ToList();

            if (periods.Count == 1)
            {
                var timeScheduleTemplatePeriod = periods.First();
                var timeScheduleTemplateHead = GetTimeScheduleTemplateHeadFromCache(timeScheduleTemplatePeriod?.TimeScheduleTemplateHeadId ?? 0);
                if (timeScheduleTemplateHead != null && (!timeScheduleTemplateHead.EmployeeId.HasValue || (timeScheduleTemplateHead.EmployeeId.HasValue && timeScheduleTemplateHead.EmployeeId.Value == employeeId)))
                    return timeScheduleTemplatePeriod;
            }

            if (checkEmployeeSchedule)
            {
                EmployeeSchedule employeeSchedule = GetEmployeeScheduleForEmployeeWithTemplateHeadAndPeriod(employeeId, date);
                if (employeeSchedule?.TimeScheduleTemplateHead?.TimeScheduleTemplatePeriod != null)
                {
                    int dayNumber = CalendarUtility.GetScheduleDayNumber(date, employeeSchedule.StartDate, employeeSchedule.StartDayNumber, employeeSchedule.TimeScheduleTemplateHead.NoOfDays);
                    List<TimeScheduleTemplatePeriod> templatePeriods = employeeSchedule.TimeScheduleTemplateHead.TimeScheduleTemplatePeriod.Where(p => p.DayNumber == dayNumber && p.State == (int)SoeEntityState.Active).ToList();
                    foreach (TimeScheduleTemplatePeriod templatePeriod in templatePeriods)
                    {
                        bool exists = (from tb in entities.TimeScheduleTemplateBlock
                                       where !tb.TimeScheduleScenarioHeadId.HasValue &&
                                       tb.TimeScheduleTemplatePeriodId == templatePeriod.TimeScheduleTemplatePeriodId &&
                                       !tb.Date.HasValue &&
                                       tb.State == (int)SoeEntityState.Active
                                       select tb).Any();

                        if (exists)
                            return templatePeriod;
                    }
                }
            }
            return null;
        }

        private bool HasMultipleScheduleTemplatePeriodsOnSameDay(int employeeId, DateTime date, out List<int> templatePeriodIds)
        {
            templatePeriodIds = new List<int>();

            var templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                  where tb.EmployeeId.HasValue && tb.EmployeeId == employeeId &&
                                  tb.Date.HasValue && tb.Date == date.Date &&
                                  tb.State == (int)SoeEntityState.Active
                                  select tb).ToList();

            foreach (var templateBlock in templateBlocks)
            {
                if (templateBlock.TimeScheduleTemplatePeriodId.HasValue && !templatePeriodIds.Any(p => p == templateBlock.TimeScheduleTemplatePeriodId))
                    templatePeriodIds.Add(templateBlock.TimeScheduleTemplatePeriodId.Value);
            }

            return templatePeriodIds.Count > 1;
        }

        #endregion

        #region TimeScheduleEmployeePeriod

        private List<TimeScheduleEmployeePeriod> GetTimeScheduleEmployeePeriods(List<int> employeeIds, DateTime startDate, DateTime stopDate)
        {
            return (from t in entities.TimeScheduleEmployeePeriod
                    where employeeIds.Contains(t.EmployeeId) &&
                    t.Date >= startDate &&
                    t.Date <= stopDate &&
                    t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    orderby t.Date
                    select t).ToList();
        }

        private List<TimeScheduleEmployeePeriod> GetTimeScheduleEmployeePeriods(List<int> employeeIds, List<DateTime> dates)
        {
            return (from p in entities.TimeScheduleEmployeePeriod
                    where employeeIds.Contains(p.EmployeeId) &&
                    dates.Contains(p.Date) &&
                    p.ActorCompanyId == actorCompanyId &&
                    p.State == (int)SoeEntityState.Active
                    orderby p.Date
                    select p).ToList();
        }

        private List<TimeScheduleEmployeePeriod> GetTimeScheduleEmployeePeriods(int employeeId, DateTime startDate, DateTime stopDate)
        {
            return (from p in entities.TimeScheduleEmployeePeriod
                    where p.EmployeeId == employeeId &&
                    p.Date >= startDate &&
                    p.Date <= stopDate &&
                    p.ActorCompanyId == actorCompanyId &&
                    p.State == (int)SoeEntityState.Active
                    orderby p.Date
                    select p).ToList();
        }

        private TimeScheduleEmployeePeriod GetTimeScheduleEmployeePeriod(int timeScheduleEmployeePeriodId)
        {
            return (from p in entities.TimeScheduleEmployeePeriod
                    where p.TimeScheduleEmployeePeriodId == timeScheduleEmployeePeriodId &&
                    p.State == (int)SoeEntityState.Active
                    select p).FirstOrDefault();
        }

        private TimeScheduleEmployeePeriod GetTimeScheduleEmployeePeriod(int employeeId, DateTime date)
        {
            var period = (from p in entities.TimeScheduleEmployeePeriod
                          where p.EmployeeId == employeeId &&
                          p.Date == date.Date &&
                          p.ActorCompanyId == actorCompanyId &&
                          p.State == (int)SoeEntityState.Active
                          select p).FirstOrDefault();

            // 2014-01-22 Håkan:
            // For some strange reason if the period has been deleted within the same transaction (as for example in SaveUniqueDay),
            // this method will find it even though state active is specified in the query.
            // So the returned period has state Deleted, therefore we need to make this extra check.
            if (period != null && period.State != (int)SoeEntityState.Active)
                period = null;

            return period;
        }

        private TimeScheduleEmployeePeriod CreateTimeScheduleEmployeePeriodIfNotExist(DateTime date, int employeeId, out bool created, List<TimeScheduleEmployeePeriod> employeePeriods = null)
        {
            created = false;

            TimeScheduleEmployeePeriod employeePeriod = this.CurrentCreatedEmployeePeriods.FirstOrDefault(p => p.EmployeeId == employeeId && p.Date == date.Date);
            if (employeePeriod == null)
            {
                if (employeePeriods != null) //Trust that all is loaded if list is not null.
                    employeePeriod = employeePeriods.FirstOrDefault(i => i.Date == date);
                else
                    employeePeriod = GetTimeScheduleEmployeePeriodFromCache(employeeId, date);
            }
            if (employeePeriod == null)
            {
                employeePeriod = new TimeScheduleEmployeePeriod()
                {
                    Date = date.Date,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                    EmployeeId = employeeId,
                };
                entities.TimeScheduleEmployeePeriod.AddObject(employeePeriod);
                SetCreatedProperties(employeePeriod);
                AddCurrentEmployeePeriodsCreated(employeePeriod);

                created = true;
            }

            return employeePeriod;
        }

        #endregion

        #region TimeScheduleTemplateBlock

        #region Discard/without scenario

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForCompanyDiscardScenario(DateTime dateFrom, DateTime dateTo, List<int> accountIds = null)
        {
            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            List<TimeScheduleTemplateBlock> blocks;
            if (!accountIds.IsNullOrEmpty())
            {
                blocks = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.AccountId.HasValue &&
                          accountIds.Contains(tb.AccountId.Value) &&
                          tb.EmployeeId.HasValue &&
                          tb.Employee.ActorCompanyId == this.actorCompanyId &&
                          (tb.Date.HasValue && tb.Date.Value >= dateFrom && tb.Date.Value <= dateTo) &&
                          tb.StartTime != tb.StopTime &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();
            }
            else
            {
                blocks = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId.HasValue &&
                          tb.Employee.ActorCompanyId == this.actorCompanyId &&
                          (tb.Date.HasValue && tb.Date.Value >= dateFrom && tb.Date.Value <= dateTo) &&
                          tb.StartTime != tb.StopTime &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();
            }

            return blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeDiscardScenario(int employeeId, DateTime date)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId == employeeId &&
                          tb.Date == date.Date &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTimeCodeDiscardScenario(int employeeId, DateTime date)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                        .Include("TimeCode")
                          where tb.EmployeeId == employeeId &&
                          tb.Date == date.Date &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTimeCodeAndStaffingDiscardScenario(int employeeId, DateTime date)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                        .Include("TimeCode")
                        .Include("TimeScheduleEmployeePeriod")
                          where tb.EmployeeId == employeeId &&
                          tb.Date == date.Date &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeOnDatesWithTimeCodeAndPeriodAndAccounting(int employeeId, DateTime date)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                         .Include("TimeCode")
                         .Include("TimeScheduleEmployeePeriod")
                         .Include("AccountInternal")
                          where tb.EmployeeId == employeeId &&
                          tb.Date == date &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeOnDatesWithTimeCodeAndPeriodAndAccounting(int employeeId, IEnumerable<DateTime> dates)
        {
            if (dates.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlock>();

            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                         .Include("TimeCode")
                         .Include("TimeScheduleEmployeePeriod")
                         .Include("AccountInternal")
                          where tb.EmployeeId == employeeId &&
                          tb.Date.HasValue &&
                          dates.Contains(tb.Date.Value) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTimeCodeDiscardScenario(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                    .Include("TimeCode")
                          where tb.EmployeeId == employeeId &&
                          (tb.Date.HasValue && tb.Date.Value >= dateFrom && tb.Date.Value <= dateTo) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTimeCodeAndStaffingDiscardScenario(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                    .Include("TimeCode")
                    .Include("TimeScheduleEmployeePeriod")
                          where tb.EmployeeId == employeeId &&
                          (tb.Date.HasValue && tb.Date.Value >= dateFrom && tb.Date.Value <= dateTo) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTimeCodeDiscardScenario(int employeeId, List<DateTime> dates)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                    .Include("TimeCode")
                          where tb.EmployeeId == employeeId &&
                          (tb.Date.HasValue && dates.Contains(tb.Date.Value)) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTimeCodeAndStaffingDiscardScenario(int employeeId, List<DateTime> dates)
        {
            return (from tb in entities.TimeScheduleTemplateBlock
                        .Include("TimeCode")
                        .Include("TimeScheduleEmployeePeriod")
                    where tb.EmployeeId == employeeId &&
                    (tb.Date.HasValue && dates.Contains(tb.Date.Value)) &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithoutScenario(int employeeId, DateTime date)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId == employeeId &&
                          tb.Date == date &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue && w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithoutScenario(int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId == employeeId &&
                          tb.Date >= dateFrom &&
                          tb.Date <= dateTo &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

            return blocks.Where(w => !w.TimeScheduleScenarioHeadId.HasValue && w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForScheduleEmployeePeriods(List<int> timeScheduleEmployeePeriodIds, bool includeOnDuty = false)
        {
            var blocks = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.TimeScheduleEmployeePeriodId.HasValue && timeScheduleEmployeePeriodIds.Contains(tb.TimeScheduleEmployeePeriodId.Value) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).Where(tb => tb.Date.HasValue).ToList();

            return includeOnDuty ? blocks : blocks.Where(w => w.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
        }

        private bool HasScheduleBlocksForEmployeeWithoutScenario(int employeeId, DateTime date)
        {
            return (from tb in entities.TimeScheduleTemplateBlock
                    where !tb.TimeScheduleScenarioHeadId.HasValue &&
                    tb.EmployeeId == employeeId &&
                    tb.Date == date &&
                    tb.Type != (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty &&
                    tb.State == (int)SoeEntityState.Active
                    select tb).Any();
        }

        #endregion

        #region Template

        private List<TimeScheduleTemplateBlock> GetTemplateScheduleBlocksForTemplateHeadWithEmployeePeriodAndAccounts(int? timeScheduleScenarioHeadId, int timeScheduleTemplateHeadId)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                                .Include("TimeScheduleEmployeePeriod")
                                                                .Include("AccountInternal")
                                                              where (timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue) &&
                                                              tb.TimeScheduleTemplatePeriod.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId &&
                                                              !tb.Date.HasValue &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScheduleType(true, true);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetTemplateScheduleBlocksForHeadWithTaskAndAccounting(int? timeScheduleScenarioHeadId, int templateHeadId, bool includeStandBy = true, bool includeOnDuty = false)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                                .Include("TimeScheduleTemplateBlockTask")
                                                                .Include("AccountInternal")
                                                              where tb.TimeScheduleTemplatePeriod.TimeScheduleTemplateHeadId == templateHeadId &&
                                                              !tb.Date.HasValue &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(includeStandBy, includeOnDuty);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetTemplateScheduleBlocksForPeriod(int? timeScheduleScenarioHeadId, int timeScheduleTemplatePeriodId, bool includeStandBy = true, bool includeOnDuty = false)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                              where tb.TimeScheduleTemplatePeriodId == timeScheduleTemplatePeriodId &&
                                                              !tb.Date.HasValue &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(includeStandBy, includeOnDuty);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetTemplateScheduleBlocksForEmployeePostWithTemplatePeriods(int? timeScheduleScenarioHeadId, TimeScheduleTemplateHead timeScheduleTemplateHead, int employeePostId)
        {
            var cycleTemplateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                        .Include("TimeScheduleTemplatePeriod")
                                       where tb.TimeScheduleTemplatePeriod.TimeScheduleTemplateHeadId == timeScheduleTemplateHead.TimeScheduleTemplateHeadId &&
                                       tb.TimeScheduleTemplatePeriod.TimeScheduleTemplateHead.EmployeePostId == employeePostId &&
                                       !tb.Date.HasValue &&
                                       tb.State == (int)SoeEntityState.Active
                                       select tb).ToList();

            cycleTemplateBlocks = cycleTemplateBlocks.FilterScenario(timeScheduleScenarioHeadId);

            if (timeScheduleTemplateHead.StartDate.HasValue)
            {
                foreach (var templateBlock in cycleTemplateBlocks)
                {
                    templateBlock.Date = timeScheduleTemplateHead.StartDate.Value.Date.AddDays(templateBlock.TimeScheduleTemplatePeriod.DayNumber - 1);
                }
            }

            return cycleTemplateBlocks;
        }

        #endregion

        private List<TimeScheduleTemplateBlock> GetScheduleBlocks(List<int> timeScheduleTemplateBlockIds)
        {
            List<TimeScheduleTemplateBlock> result = new List<TimeScheduleTemplateBlock>();

            if (timeScheduleTemplateBlockIds.IsNullOrEmpty())
                return result;

            BatchHelper batchHelper = BatchHelper.Create(timeScheduleTemplateBlockIds);
            while (batchHelper.HasMoreBatches())
            {
                var batchIds = batchHelper.GetCurrentBatchIds();
                var batch = (from t in entities.TimeScheduleTemplateBlock
                             where batchIds.Contains(t.TimeScheduleTemplateBlockId) &&
                             t.State == (int)SoeEntityState.Active
                             select t).ToList();

                result.AddRange(batch);
                batchHelper.MoveToNextBatch();
            }

            return result;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployees(int? timeScheduleScenarioHeadId, List<int> employeeIds, DateTime date)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                              where tb.EmployeeId.HasValue && employeeIds.Contains(tb.EmployeeId.Value) &&
                                                              tb.Date.Value == date.Date &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(true, true);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployee(int? timeScheduleScenarioHeadId, int employeeId, DateTime date)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                              where tb.EmployeeId == employeeId &&
                                                              tb.Date == date.Date &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(false, false);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployee(int employeeId, IEnumerable<DateTime> dates)
        {
            if (dates.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlock>();

            return (from t in entities.TimeScheduleTemplateBlock
                    where t.EmployeeId == employeeId &&
                    t.Date.HasValue &&
                    dates.Contains(t.Date.Value) &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployee(int? timeScheduleScenarioHeadId, int employeeId, DateTime dateFrom, DateTime dateTo, int? employeeScheduleId = null, bool loadStaffingIfUsed = true, bool includeStandBy = false, bool loadShiftType = false, bool loadScheduleType = false, bool loadDeviationCause = false, bool loadHistory = false, bool includeOnDuty = false)
        {
            if (loadStaffingIfUsed && !UseStaffing())
                loadStaffingIfUsed = false;

            dateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            IQueryable<TimeScheduleTemplateBlock> query = entities.TimeScheduleTemplateBlock;
            if (loadStaffingIfUsed)
                query = query.Include("TimeScheduleEmployeePeriod");
            if (loadShiftType)
                query = query.Include("ShiftType");
            if (loadScheduleType)
                query = query.Include("TimeScheduleType");
            if (loadDeviationCause)
                query = query.Include("TimeDeviationCause");
            if (loadHistory)
                query = query.Include("TimeScheduleTemplateBlockHistory");

            if (IsHiddenEmployeeFromCache(employeeId))
                query = query.Where(b => (!employeeScheduleId.HasValue || b.EmployeeScheduleId.Value == employeeScheduleId.Value));

            List<TimeScheduleTemplateBlock> templateBlocks = (from b in query
                                                              where b.EmployeeId.Value == employeeId &&
                                                              (b.Date.HasValue && b.Date.Value >= dateFrom && b.Date.Value <= dateTo) &&
                                                              b.State == (int)SoeEntityState.Active
                                                              select b).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(includeStandBy, includeOnDuty);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeesWithTask(int? timeScheduleScenarioHeadId, List<int> employeeIds, DateTime dateFrom, DateTime dateTo)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                                .Include("TimeScheduleTemplateBlockTask")
                                                              where tb.EmployeeId.HasValue && employeeIds.Contains(tb.EmployeeId.Value) &&
                                                              (tb.Date.HasValue && tb.Date >= dateFrom && tb.Date <= dateTo) &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(true, false);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTimeCodeAndAccounting(int? timeScheduleScenarioHeadId, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                                .Include("TimeCode")
                                                                .Include("AccountInternal")
                                                              where tb.EmployeeId == employeeId &&
                                                              (tb.Date.HasValue && tb.Date.Value >= dateFrom && tb.Date.Value <= dateTo) &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(false, false);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeAndTemplateHeadWithStaffingAndAccounting(int? timeScheduleScenarioHeadId, int timeScheduleTemplateHeadId, int employeeId, DateTime startDate, DateTime? stopDate)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                                .Include("TimeScheduleEmployeePeriod")
                                                                .Include("AccountInternal")
                                                              where tb.TimeScheduleTemplatePeriod.TimeScheduleTemplateHeadId == timeScheduleTemplateHeadId &&
                                                              tb.EmployeeId == employeeId &&
                                                              (tb.Date.HasValue && tb.Date.Value >= startDate && (!stopDate.HasValue || tb.Date.Value <= stopDate.Value)) &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(true, true);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForEmployeeWithTasks(int? timeScheduleScenarioHeadId, int employeeId, DateTime date)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                                .Include("TimeScheduleTemplateBlockTask")
                                                              where tb.EmployeeId == employeeId &&
                                                              tb.Date == date.Date &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            templateBlocks = templateBlocks.FilterScenario(timeScheduleScenarioHeadId);
            templateBlocks = templateBlocks.FilterScheduleType(false, false);
            return templateBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForAccounts(int? timeScheduleScenarioHeadId, ref DateTime dateFrom, ref DateTime dateTo, List<DateTime> newDates, List<int> accountIds, bool includeStandby = false)
        {
            List<TimeScheduleTemplateBlock> scheduleBlocks = GetTimeScheduleTemplateBlocksForCompanyFromCache(dateFrom, dateTo, accountIds);
            List<DateTime> addedDates = CalendarUtility.GetDates(dateFrom, dateTo);

            if (newDates != null)
            {
                foreach (DateTime newDate in newDates)
                {
                    if (addedDates.Contains(newDate))
                        continue;

                    scheduleBlocks.AddRange(GetTimeScheduleTemplateBlocksForCompanyFromCache(newDate, newDate, accountIds));
                    addedDates.Add(newDate);
                }
            }

            if (addedDates.Any())
            {
                dateFrom = addedDates.Min().Date;
                dateTo = addedDates.Max().Date;
            }

            scheduleBlocks = scheduleBlocks.Where(e => e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule && (includeStandby || !e.IsStandby())).ToList();
            scheduleBlocks = scheduleBlocks.FilterScenario(timeScheduleScenarioHeadId);
            return scheduleBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForWorkRuleEvaluation(int? timeScheduleScenarioHeadId, List<int> employeeIds, DateTime startTime, DateTime stopTime, bool isOrderPlanning)
        {
            List<TimeScheduleTemplateBlock> result = null;

            if (isOrderPlanning)
            {
                result = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId.HasValue &&
                          employeeIds.Contains(tb.EmployeeId.Value) &&
                          tb.TimeScheduleEmployeePeriod != null &&
                          (tb.Date.HasValue && tb.Date.Value >= startTime.Date && tb.Date.Value <= stopTime.Date) && //Date (startdate) should be in intervall                                                            
                          tb.StartTime != tb.StopTime &&
                          (tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Booking || tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Order) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

                result = result.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();
            }
            else
            {
                result = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId.HasValue &&
                          employeeIds.Contains(tb.EmployeeId.Value) &&
                          tb.TimeScheduleEmployeePeriod != null &&
                          (tb.Date.HasValue && tb.Date.Value >= startTime.Date && tb.Date.Value <= stopTime.Date) && //Date (startdate) should be in intervall                                                            
                          tb.StartTime != tb.StopTime &&
                          (tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Booking || tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

                result = result.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();
            }

            return result;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksForWorkRuleEvaluation(int? timeScheduleScenarioHeadId, int employeeId, DateTime startTime, DateTime stopTime, bool isOrderPlanning)
        {
            List<TimeScheduleTemplateBlock> result = null;

            if (isOrderPlanning)
            {
                result = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId == employeeId &&
                          tb.TimeScheduleEmployeePeriod != null &&
                          (tb.Date.HasValue && tb.Date.Value >= startTime.Date && tb.Date.Value <= stopTime.Date) && //Date (startdate) should be in intervall                                                            
                          tb.StartTime != tb.StopTime &&
                          (tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Booking || tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Order) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

                result = result.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();

            }
            else
            {
                result = (from tb in entities.TimeScheduleTemplateBlock
                          where tb.EmployeeId == employeeId &&
                          tb.TimeScheduleEmployeePeriod != null &&
                          (tb.Date.HasValue && tb.Date.Value >= startTime.Date && tb.Date.Value <= stopTime.Date) && //Date (startdate) should be in intervall                                                            
                          tb.StartTime != tb.StopTime &&
                          (tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Booking || tb.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby) &&
                          tb.State == (int)SoeEntityState.Active
                          select tb).ToList();

                result = result.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();

            }

            return result;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksLinked(int? timeScheduleScenarioHeadId, TimeScheduleTemplateBlock scheduleBlock)
        {
            if (scheduleBlock == null || !scheduleBlock.EmployeeId.HasValue || !scheduleBlock.Date.HasValue)
                return new List<TimeScheduleTemplateBlock>();

            List<TimeScheduleTemplateBlock> linkedShifts = (from tb in entities.TimeScheduleTemplateBlock
                                                            where ((tb.EmployeeId.HasValue && tb.EmployeeId.Value == scheduleBlock.EmployeeId.Value) &&
                                                            (tb.Date.HasValue && tb.Date.Value == scheduleBlock.Date.Value) &&
                                                            (tb.StartTime != tb.StopTime) &&
                                                            (tb.TimeScheduleEmployeePeriod != null) &&
                                                            (tb.Type == scheduleBlock.Type) &&
                                                            (tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None) &&
                                                            (tb.State == (int)SoeEntityState.Active) &&
                                                            (tb.Link == scheduleBlock.Link))
                                                            select tb).ToList();

            linkedShifts = linkedShifts.FilterScenario(timeScheduleScenarioHeadId);
            // Removed due to #84214, see no reason for having this limitation here.
            // If we do, it should also be stopped on client side before trying to copy/move shifts or even linking them together.
            //if (scheduleBlock.AccountId.HasValue)
            //    linkedShifts = linkedShifts.Where(tb => tb.AccountId.HasValue && tb.AccountId.Value == scheduleBlock.AccountId.Value).ToList();

            return linkedShifts;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksLinked(int? timeScheduleScenarioHeadId, int employeeId, DateTime date, string linkString, TermGroup_TimeScheduleTemplateBlockType scheduleType, SoeTimeScheduleTemplateBlockBreakType? breakType = SoeTimeScheduleTemplateBlockBreakType.None)
        {
            List<TimeScheduleTemplateBlock> linkedShifts = (from tb in entities.TimeScheduleTemplateBlock
                                                            where ((tb.EmployeeId.HasValue && tb.EmployeeId.Value == employeeId) &&
                                                            (tb.Date.HasValue && tb.Date.Value == date.Date) &&
                                                            (tb.StartTime != tb.StopTime) &&
                                                            (tb.TimeScheduleEmployeePeriod != null) &&
                                                            (tb.Type == (int)scheduleType) &&
                                                            (tb.State == (int)SoeEntityState.Active) &&
                                                            (tb.Link == linkString))
                                                            select tb).ToList();

            linkedShifts = linkedShifts.FilterScenario(timeScheduleScenarioHeadId);
            if (breakType.HasValue)
                linkedShifts = linkedShifts.Where(x => x.BreakType == (int)breakType.Value).ToList();

            return linkedShifts;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlocksLinkedWithPeriodAndStaffing(int? timeScheduleScenarioHeadId, TimeScheduleTemplateBlock scheduleBlock)
        {
            if (!scheduleBlock.EmployeeId.HasValue || !scheduleBlock.Date.HasValue)
                return new List<TimeScheduleTemplateBlock>();

            List<TimeScheduleTemplateBlock> linkedShifts = (from tb in entities.TimeScheduleTemplateBlock
                                                                .Include("TimeScheduleTemplatePeriod")
                                                                .Include("TimeScheduleEmployeePeriod")
                                                            where ((tb.EmployeeId.HasValue && tb.EmployeeId.Value == scheduleBlock.EmployeeId.Value) &&
                                                            (tb.Date.HasValue && tb.Date.Value == scheduleBlock.Date.Value) &&
                                                            (tb.StartTime != tb.StopTime) &&
                                                            (tb.TimeScheduleEmployeePeriod != null) &&
                                                            (tb.Type == scheduleBlock.Type) &&
                                                            (tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None) &&
                                                            (tb.State == (int)SoeEntityState.Active) &&
                                                            (tb.Link == scheduleBlock.Link))
                                                            select tb).ToList();

            linkedShifts = linkedShifts.FilterScenario(timeScheduleScenarioHeadId);
            // Removed due to #84214, see no reason for having this limitation here.
            // If we do, it should also be stopped on client side before trying to copy/move shifts or even linking them together.
            //if (scheduleBlock.AccountId.HasValue)
            //    linkedShifts = linkedShifts.Where(tb => tb.AccountId.HasValue && tb.AccountId.Value == scheduleBlock.AccountId.Value).ToList();

            return linkedShifts;
        }

        private List<TimeScheduleTemplateBlock> GetScheduleBlockZero(int? timeScheduleScenarioHeadId, int employeeId, DateTime date, int? templatePeriodId = null)
        {
            List<TimeScheduleTemplateBlock> scheduleBlocks = (from tb in entities.TimeScheduleTemplateBlock
                                                              where tb.EmployeeId == employeeId &&
                                                              (!templatePeriodId.HasValue || tb.TimeScheduleTemplatePeriodId == templatePeriodId.Value) &&
                                                              tb.Date == date.Date &&
                                                              tb.StartTime == tb.StopTime &&
                                                              tb.State == (int)SoeEntityState.Active
                                                              select tb).ToList();

            scheduleBlocks = scheduleBlocks.FilterScenario(timeScheduleScenarioHeadId);
            return scheduleBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetSourceShiftsAndShiftsInCycle(List<int> sourceShiftIds, int cycleWeek, DateTime cycleDateFrom, DateTime cycleDateTo, int? timeScheduleScenarioHeadId)
        {
            List<TimeScheduleTemplateBlock> sourceShifts = GetScheduleBlocks(sourceShiftIds);
            if (sourceShifts.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlock>();

            List<TimeScheduleTemplateBlock> matches = new List<TimeScheduleTemplateBlock>();
            foreach (var sourceShift in sourceShifts)
            {
                if (!sourceShift.Date.HasValue)
                    continue;

                List<DateTime> cycleDates = new List<DateTime>();
                DateTime nextCycleDate = sourceShift.Date.Value.AddDays(cycleWeek * 7);
                while (cycleDateFrom <= nextCycleDate && nextCycleDate <= cycleDateTo)
                {
                    cycleDates.Add(nextCycleDate);
                    nextCycleDate = nextCycleDate.AddDays(cycleWeek * 7);
                }

                if (cycleDates.Any())
                {
                    List<TimeScheduleTemplateBlock> shiftsInCycle = (from tb in entities.TimeScheduleTemplateBlock
                                                                     where tb.EmployeeId == sourceShift.EmployeeId &&
                                                                     tb.Date.HasValue &&
                                                                     cycleDates.Contains(tb.Date.Value) &&
                                                                     tb.State == (int)SoeEntityState.Active
                                                                     select tb).ToList();

                    shiftsInCycle = shiftsInCycle.Where(tb => timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue).ToList();


                    //Add matches
                    matches.AddRange(shiftsInCycle.Where(x => x.Type == sourceShift.Type && x.StartTime == sourceShift.StartTime && x.StopTime == sourceShift.StopTime));
                }
            }

            sourceShifts.AddRange(matches);
            return sourceShifts.OrderBy(x => x.Date).ToList();
        }

        private List<TimeScheduleTemplateBlock> SplitTemplateBlocksToActualTimes(List<TimeScheduleTemplateBlock> templateBlocks)
        {
            List<TimeScheduleTemplateBlock> splittedTemplateBlocks = new List<TimeScheduleTemplateBlock>();

            List<TimeEngineBlock> blocks = TimeEngineBlock.Create(templateBlocks);
            foreach (TimeEngineBlock block in blocks)
            {
                TimeScheduleTemplateBlock originalTemplateBlock = block.FindOriginal(templateBlocks);
                if (originalTemplateBlock == null)
                    continue;

                TimeScheduleType scheduleType = originalTemplateBlock.TimeScheduleTypeId.HasValue ? TimeScheduleManager.GetTimeScheduleType(this.entities, originalTemplateBlock.TimeScheduleTypeId.Value, false) : null;
                TimeScheduleTemplateBlock newTemplateBlock = CreateTimeScheduleTemplateBlock(originalTemplateBlock, block.StartTime, block.StopTime, timeDeviationCause: originalTemplateBlock.TimeDeviationCause, scheduleType: scheduleType, employeeChildId: originalTemplateBlock.EmployeeChildId);
                if (newTemplateBlock == null)
                    continue;

                newTemplateBlock.TimeScheduleTemplateBlockId = originalTemplateBlock.TimeScheduleTemplateBlockId;
                splittedTemplateBlocks.Add(newTemplateBlock);
            }

            return splittedTemplateBlocks;
        }

        private List<TimeScheduleTemplateBlock> FilterScheduleBasedOnStandby(List<TimeScheduleTemplateBlock> scheduleBlocks, TimeBlock timeBlock)
        {
            if (timeBlock != null && scheduleBlocks.ContainsStandby())
            {
                TimeScheduleTemplateBlock matchingTemplateBlock = scheduleBlocks.GetMatchingScheduleBlock(timeBlock, false);
                if (matchingTemplateBlock != null && matchingTemplateBlock.IsStandby())
                    scheduleBlocks = scheduleBlocks.GetStandby();
                else
                    scheduleBlocks = scheduleBlocks.ExcludeStandby();
            }

            return scheduleBlocks;
        }

        private List<TimeScheduleTemplateBlockDTO> MergeScheduleBlocksByType(List<TimeScheduleTemplateBlockDTO> inputScheduleBlocks)
        {
            var mergedScheduleBlocks = new List<TimeScheduleTemplateBlockDTO>();

            if (inputScheduleBlocks != null)
            {
                TimeScheduleTemplateBlockDTO prevInputScheduleBlock = null;
                foreach (TimeScheduleTemplateBlockDTO inputScheduleBlock in inputScheduleBlocks.OrderBy(i => i.StartTime))
                {
                    if (prevInputScheduleBlock == null)
                    {
                        prevInputScheduleBlock = inputScheduleBlock;
                        continue;
                    }

                    if (prevInputScheduleBlock.TimeCodeId == inputScheduleBlock.TimeCodeId)
                    {
                        //Merge template blocks
                        prevInputScheduleBlock.StopTime = inputScheduleBlock.StopTime;
                    }
                    else
                    {
                        //Add
                        mergedScheduleBlocks.Add(prevInputScheduleBlock);
                        prevInputScheduleBlock = inputScheduleBlock;
                    }
                }

                //Add last block if it was not merged
                if (prevInputScheduleBlock != null)
                    mergedScheduleBlocks.Add(prevInputScheduleBlock);
            }

            return mergedScheduleBlocks;
        }

        private List<TimeScheduleTemplateBlockDTO> AdjustScheduleBlocksAccordingToHalfday(List<TimeScheduleTemplateBlockDTO> inputScheduleBlocks, TimeHalfdayDTO halfDay)
        {
            if (inputScheduleBlocks.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlockDTO>();
            if (halfDay == null)
                return inputScheduleBlocks;

            var templateBlockItems = new List<TimeScheduleTemplateBlockDTO>();

            int minutesOrPercentage = (int)halfDay.Value;
            int totalBreakMinutes = 0;

            // Merge adjacent blocks of the same type
            List<TimeScheduleTemplateBlockDTO> mergedScheduleBlocks = MergeScheduleBlocksByType(inputScheduleBlocks);
            foreach (TimeScheduleTemplateBlockDTO mergedScheduleBlock in mergedScheduleBlocks)
            {
                // Calculate total break tiem (before remove breaks for halfday)
                if (mergedScheduleBlock.IsBreak)
                    totalBreakMinutes += Convert.ToInt32((mergedScheduleBlock.StopTime - mergedScheduleBlock.StartTime).TotalMinutes);

                //Remove breaks not occuring on halfday
                if (mergedScheduleBlock.IsBreak && halfDay.TimeCodeBreaks != null && halfDay.TimeCodeBreaks.Any(i => i.TimeCodeId == mergedScheduleBlock.TimeCodeId))
                    continue;

                templateBlockItems.Add(mergedScheduleBlock);
            }

            List<TimeScheduleTemplateBlockDTO> templateBlockItemsWork = templateBlockItems.GetWork();
            TimeScheduleTemplateBlockDTO templateBlockItemsFirst = templateBlockItemsWork.FirstOrDefault();
            TimeScheduleTemplateBlockDTO templateBlockItemsLast = templateBlockItemsWork.LastOrDefault();
            List<TimeScheduleTemplateBlockDTO> templateBlockItemsBreak = templateBlockItems.GetBreaks();

            switch (halfDay.Type)
            {
                case SoeTimeHalfdayType.ClockInMinutes:
                    #region ClockInMinutes

                    if (templateBlockItemsLast != null)
                    {
                        //Remove
                        templateBlockItems.Remove(templateBlockItemsLast);

                        //Calculate
                        int hours = minutesOrPercentage / 60;
                        minutesOrPercentage -= (hours * 60);

                        templateBlockItemsLast.StopTime = new DateTime(templateBlockItemsLast.StopTime.Year, templateBlockItemsLast.StopTime.Month, templateBlockItemsLast.StopTime.Day, hours, minutesOrPercentage, 0);

                        //Add
                        templateBlockItems.Add(templateBlockItemsLast);
                    }
                    #endregion
                    break;
                case SoeTimeHalfdayType.RelativeStartPercentage:
                    #region RelativeStartPercentage

                    if (templateBlockItemsLast != null)
                    {
                        //Remove
                        templateBlockItems.Remove(templateBlockItemsLast);

                        //Calculate
                        templateBlockItemsLast.StopTime = CalendarUtility.DecreaseDateTimeByPercent(templateBlockItemsLast.StopTime.AddMinutes(-totalBreakMinutes), (int)((templateBlockItemsLast.StopTime - templateBlockItemsLast.StartTime).TotalMinutes - totalBreakMinutes), minutesOrPercentage);

                        //Add
                        templateBlockItems.Add(templateBlockItemsLast);
                    }
                    #endregion
                    break;
                case SoeTimeHalfdayType.RelativeEndPercentage:
                    #region RelativeEndPercentage

                    if (templateBlockItemsFirst != null)
                    {
                        //Remove
                        templateBlockItems.Remove(templateBlockItemsFirst);

                        //Calculate
                        templateBlockItemsFirst.StartTime = CalendarUtility.IncreaseDateTimeByPercent(templateBlockItemsFirst.StartTime.AddMinutes(totalBreakMinutes), (int)((templateBlockItemsFirst.StopTime - templateBlockItemsFirst.StartTime).TotalMinutes - totalBreakMinutes), halfDay.Value);

                        //Add
                        templateBlockItems.Add(templateBlockItemsFirst);
                    }
                    #endregion
                    break;
                case SoeTimeHalfdayType.RelativeEndValue:
                    #region RelativeEndValue
                    if (templateBlockItemsLast != null)
                    {
                        //Remove
                        templateBlockItems.Remove(templateBlockItemsLast);

                        //Calculate
                        templateBlockItemsLast.StopTime = templateBlockItemsLast.StopTime.Subtract(new TimeSpan(0, minutesOrPercentage, 0));

                        //Add
                        templateBlockItems.Add(templateBlockItemsLast);
                    }
                    #endregion
                    break;
                case SoeTimeHalfdayType.RelativeStartValue:
                    #region RelativeStartValue
                    if (templateBlockItemsFirst != null)
                    {
                        //Remove
                        templateBlockItems.Remove(templateBlockItemsFirst);

                        //Calculate
                        templateBlockItemsFirst.StartTime = templateBlockItemsFirst.StartTime.AddMinutes(minutesOrPercentage);

                        //Add
                        templateBlockItems.Add(templateBlockItemsFirst);
                    }
                    #endregion
                    break;
            }

            if (templateBlockItemsWork.Count == 1)
            {
                #region Zero-schedule

                if (templateBlockItemsLast != null && templateBlockItemsLast.StopTime <= templateBlockItemsLast.StartTime)
                {
                    //Set as zero day
                    templateBlockItemsLast.StartTime = CalendarUtility.DATETIME_DEFAULT;
                    templateBlockItemsLast.StopTime = CalendarUtility.DATETIME_DEFAULT;
                }

                #endregion
            }
            else
            {
                #region Adjust times

                //If the last TimeBlock doesn't span over the reduced time we need to remove it and lessen previous TimeBlock's with the remaining part
                while (templateBlockItemsLast != null && templateBlockItemsLast.StopTime <= templateBlockItemsLast.StartTime)
                {
                    templateBlockItems.Remove(templateBlockItemsLast);

                    templateBlockItemsLast = templateBlockItems.LastOrDefault(b => b.TimeCode.Type == SoeTimeCodeType.Work);
                    if (templateBlockItemsLast == null)
                        break;

                    int differenceInMinutes = (int)(templateBlockItemsLast.StartTime - templateBlockItemsLast.StopTime).TotalMinutes;
                    templateBlockItemsLast.StopTime = templateBlockItemsLast.StopTime.Subtract(new TimeSpan(0, differenceInMinutes, 0));
                }

                #endregion
            }

            #region Breaks

            foreach (TimeScheduleTemplateBlockDTO templateBlockItemBreak in templateBlockItemsBreak)
            {
                ParseBreak(templateBlockItemBreak, templateBlockItemsWork);
            }

            #endregion

            return templateBlockItems;
        }

        private List<TimeScheduleTemplateBlockDTO> GetAvailableBlocksForOrderAssignment(List<TimeScheduleTemplateBlockDTO> blocks, DateTime startTime, DateTime? stopTime, int? employeeId = null, int? orderId = null, int? shiftTypeId = null, int remainingMinutes = int.MaxValue, bool ignoreBreaks = false)
        {
            if (blocks.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlockDTO>();

            List<Tuple<DateTime, DateTime, bool>> scheduleintervals = new List<Tuple<DateTime, DateTime, bool>>();
            blocks.Where(w => !w.IsBreak && !w.CustomerInvoiceId.HasValue && w.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule).ToList().ForEach(f => scheduleintervals.Add(Tuple.Create(f.ActualStartTime.Value, f.ActualStopTime.Value, false)));

            if (scheduleintervals.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlockDTO>();

            List<Tuple<DateTime, DateTime, bool>> unavailableIntervals = new List<Tuple<DateTime, DateTime, bool>>();
            blocks.Where(w => w.IsBreak || w.CustomerInvoiceId.HasValue || w.Type == TermGroup_TimeScheduleTemplateBlockType.Booking).ToList().ForEach(f => unavailableIntervals.Add(Tuple.Create(f.ActualStartTime.Value, f.ActualStopTime.Value, f.IsBreak)));

            DateTime currentTime = CalendarUtility.GetLatestDate(startTime, scheduleintervals.OrderBy(o => o.Item1).FirstOrDefault()?.Item1);
            TimeScheduleTemplateBlockDTO block = null;
            List<TimeScheduleTemplateBlockDTO> availableBlocks = new List<TimeScheduleTemplateBlockDTO>();
            int rest = 0;
            int breakTime = 0;
            while (currentTime < stopTime && (remainingMinutes - rest) > 0)
            {
                var breakTimeAdded = false;
                if (block == null &&
                    (
                    scheduleintervals.Any(w => CalendarUtility.IsDatesOverlapping(w.Item1, w.Item2, currentTime, currentTime))
                    &&
                    !unavailableIntervals.Any(w => CalendarUtility.IsCurrentInsideNew(w.Item1, w.Item2, currentTime, currentTime)))
                    )
                {
                    block = new TimeScheduleTemplateBlockDTO
                    {
                        Date = currentTime.Date,
                        StartTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, currentTime)
                    };
                }

                else if (
                            block != null &&
                            (
                                unavailableIntervals.Any(w => 
                                        (
                                            (!ignoreBreaks && w.Item3) || 
                                            !w.Item3
                                        ) && 
                                        CalendarUtility.IsDatesOverlapping(w.Item1, w.Item2, currentTime, currentTime)
                                )
                                ||
                                !scheduleintervals.Any(w => CalendarUtility.IsCurrentInsideNew(w.Item1, w.Item2, currentTime, currentTime))
                            )
                        )
                {
                    block.StopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, currentTime);
                    block.EmployeeId = employeeId;
                    block.CustomerInvoiceId = orderId;
                    block.ShiftTypeId = shiftTypeId;
                    block.Link = Guid.NewGuid();
                    block.Type = TermGroup_TimeScheduleTemplateBlockType.Order;
                    if (block.TotalMinutes > 1)
                    {
                        if (ignoreBreaks)
                        {
                            block.PlannedTime = (block.TotalMinutes - (breakTime - 1));
                            availableBlocks.Add(block.CloneDTO());
                            remainingMinutes -= (block.TotalMinutes - (breakTime - 1));
                        }
                        else
                        {
                            availableBlocks.Add(block.CloneDTO());
                            remainingMinutes -= block.TotalMinutes;
                        }
                        rest = 0;
                        breakTime = 0;
                    }
                    block = null;

                    if (unavailableIntervals.Any(w => CalendarUtility.IsDatesOverlapping(w.Item1, w.Item2, currentTime, currentTime)))
                    {
                        var interval = unavailableIntervals.FirstOrDefault(w => CalendarUtility.IsDatesOverlapping(w.Item1, w.Item2, currentTime, currentTime));
                        if (interval != null)
                            currentTime = interval.Item2.AddMinutes(-1);
                        rest--;
                    }
                    else if (scheduleintervals.Any(w => CalendarUtility.IsCurrentInsideNew(w.Item1, w.Item2, currentTime, currentTime)))
                    {
                        var interval = scheduleintervals.FirstOrDefault(w => w.Item1 > currentTime);
                        if (interval != null)
                            currentTime = interval.Item1.AddMinutes(-1);
                    }
                }
                else if (ignoreBreaks && unavailableIntervals.Any(w => w.Item3 && CalendarUtility.IsDatesOverlapping(w.Item1, w.Item2, currentTime, currentTime)))
                {
                    breakTime++;
                    breakTimeAdded = true;
                }
                else if (
                    unavailableIntervals.Any(w => CalendarUtility.IsCurrentInsideNew(w.Item1, w.Item2, currentTime, currentTime)) ||
                    scheduleintervals.Any(w => CalendarUtility.IsCurrentInsideNew(w.Item1, w.Item2, currentTime, currentTime)))
                {
                    var nextTimeMatch = scheduleintervals.FirstOrDefault(w => CalendarUtility.IsCurrentInsideNew(w.Item1, w.Item2, currentTime, currentTime));
                    var nextTime = nextTimeMatch != null ? nextTimeMatch.Item1 : currentTime;
                    currentTime = nextTime > currentTime ? nextTime.AddMinutes(-1) : currentTime;
                }

                currentTime = currentTime.AddMinutes(1);

                if (block != null && !breakTimeAdded)
                    rest++;
            }

            if (block != null)
            {
                block.StopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, currentTime);
                block.EmployeeId = employeeId;
                block.CustomerInvoiceId = orderId;
                block.ShiftTypeId = shiftTypeId;
                block.Link = Guid.NewGuid();
                block.Type = TermGroup_TimeScheduleTemplateBlockType.Order;
                if (block.TotalMinutes > 1)
                {
                    if (ignoreBreaks)
                        block.PlannedTime = (block.TotalMinutes - (breakTime - 1));
                    availableBlocks.Add(block.CloneDTO());
                }
            }

            return availableBlocks;
        }

        private List<TimeScheduleTemplateBlock> GetOverlappingOnDutyShifts(int? employeeId, int? accountId, DateTime? date, DateTime startTime, DateTime stopTime)
        {
            List<TimeScheduleTemplateBlock> overlappingOnDutyShifts = new List<TimeScheduleTemplateBlock>();

            if (employeeId == null)
                return overlappingOnDutyShifts;

            overlappingOnDutyShifts = (
                from t in entities.TimeScheduleTemplateBlock
                where t.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty &&
                t.State == (int)SoeEntityState.Active &&
                (t.AccountId == accountId || !t.AccountId.HasValue || !accountId.HasValue) &&
                t.EmployeeId == employeeId &&
                t.Date == date &&
                t.StartTime < stopTime &&
                t.StopTime > startTime
                select t).ToList();

            return overlappingOnDutyShifts;
        }

        private TimeScheduleTemplateBlock GetScheduleBlock(int timeScheduleTemplateBlockId)
        {
            if (timeScheduleTemplateBlockId == 0)
                return null;

            return (from t in entities.TimeScheduleTemplateBlock
                    where t.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimeScheduleTemplateBlock GetScheduleBlockWithTimeCodeAndAccounting(int timeScheduleTemplateBlockId)
        {
            if (timeScheduleTemplateBlockId == 0)
                return null;

            return (from t in entities.TimeScheduleTemplateBlock
                        .Include("TimeCode")
                        .Include("AccountInternal")
                    where t.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimeScheduleTemplateBlock GetScheduleBlockWithAccounting(int timeScheduleTemplateBlockId)
        {
            if (timeScheduleTemplateBlockId == 0)
                return null;

            return (from t in entities.TimeScheduleTemplateBlock
                        .Include("AccountInternal")
                    where t.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId
                    select t).FirstOrDefault();
        }

        private TimeScheduleTemplateBlock GetScheduleBlockWithPeriodAndStaffing(int timeScheduleTemplateBlockId)
        {
            if (timeScheduleTemplateBlockId == 0)
                return null;

            return (from t in entities.TimeScheduleTemplateBlock
                        .Include("TimeScheduleTemplatePeriod")
                        .Include("TimeScheduleEmployeePeriod")
                    where t.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId &&
                    //t.TimeScheduleEmployeePeriod != null &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimeScheduleTemplateBlock GetScheduleBlockWithPeriodAndStaffingAndAccounting(int timeScheduleTemplateBlockId)
        {
            if (timeScheduleTemplateBlockId == 0)
                return null;

            return (from t in entities.TimeScheduleTemplateBlock
                        .Include("TimeScheduleTemplatePeriod")
                        .Include("TimeScheduleEmployeePeriod")
                        .Include("AccountInternal")
                    where t.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId &&
                    //t.TimeScheduleEmployeePeriod != null &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private TimeScheduleTemplateBlock GetScheduleBlockClosestBreak(List<TimeScheduleTemplateBlock> scheduleBlocks, DateTime holeStart, DateTime holeStop)
        {
            TimeScheduleTemplateBlock closestBreak = null;

            // Check if any breaks exist on schedule
            List<TimeScheduleTemplateBlock> scheduleBreaks = scheduleBlocks.GetBreaks();
            if (scheduleBreaks.Any())
            {
                // Get middle time of hole
                DateTime breakMiddle = CalendarUtility.GetMiddleTime(holeStart, holeStop);
                TimeSpan closestDiff = new TimeSpan(24, 0, 0);

                // Find closest scheduled break by its middle time
                foreach (TimeScheduleTemplateBlock scheduleBreak in scheduleBreaks)
                {
                    DateTime scheduleBreakMiddle = CalendarUtility.GetMiddleTime(scheduleBreak.StartTime, scheduleBreak.StopTime);
                    TimeSpan diff = breakMiddle > scheduleBreakMiddle ? breakMiddle - scheduleBreakMiddle : scheduleBreakMiddle - breakMiddle;
                    if (diff < closestDiff)
                    {
                        // This break is closer, keep it
                        closestDiff = diff;
                        closestBreak = scheduleBreak;
                    }
                }

                if (closestBreak != null && !closestBreak.TimeCodeReference.IsLoaded)
                    closestBreak.TimeCodeReference.Load();
            }

            return closestBreak;
        }

        private List<string> GetScheduleBlockLinks(int? timeScheduleScenarioHeadId, int employeeId, DateTime date)
        {
            if (!timeScheduleScenarioHeadId.HasValue)
            {
                return (from tb in entities.TimeScheduleTemplateBlock
                        where ((tb.EmployeeId.HasValue && tb.EmployeeId.Value == employeeId) &&
                        (tb.Date.HasValue && tb.Date.Value == date.Date) &&
                        (tb.StartTime != tb.StopTime) &&
                        (tb.State == (int)SoeEntityState.Active))
                        select tb.Link).ToList();
            }
            else
            {
                var shifts = (from tb in entities.TimeScheduleTemplateBlock
                              where ((tb.EmployeeId.HasValue && tb.EmployeeId.Value == employeeId) &&
                              (tb.Date.HasValue && tb.Date.Value == date.Date) &&
                              (tb.StartTime != tb.StopTime) &&
                              (tb.State == (int)SoeEntityState.Active))
                              select tb).ToList();

                return shifts.Where(tb => tb.TimeScheduleScenarioHeadId.HasValue && tb.TimeScheduleScenarioHeadId.Value == timeScheduleScenarioHeadId).Select(s => s.Link).ToList();

            }
        }

        private int GetNoOfScheduleBlocks(int? timeScheduleScenarioHeadId, DateTime date, int employeeId)
        {
            return (from tb in entities.TimeScheduleTemplateBlock
                    where (timeScheduleScenarioHeadId.HasValue ? tb.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value : !tb.TimeScheduleScenarioHeadId.HasValue) &&
                    tb.EmployeeId == employeeId &&
                    tb.Date == date.Date.Date &&
                    tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && //dont count breaks
                    tb.State == (int)SoeEntityState.Active
                    select tb).Count();
        }

        private bool ExistsOtherNonZeroShifts(TimeScheduleTemplateBlock shift)
        {
            return (from tb in entities.TimeScheduleTemplateBlock
                    where tb.TimeScheduleScenarioHeadId == shift.TimeScheduleScenarioHeadId &&
                    ((tb.EmployeeId.HasValue && shift.EmployeeId.HasValue && tb.EmployeeId.Value == shift.EmployeeId.Value) &&
                    (tb.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None) &&
                    (tb.TimeScheduleEmployeePeriodId == shift.TimeScheduleEmployeePeriodId) &&
                    (tb.Date.HasValue && shift.Date.HasValue && tb.Date.Value == shift.Date.Value) &&
                    (tb.State == (int)SoeEntityState.Active) &&
                    (tb.StartTime != tb.StopTime) &&
                    (tb.TimeScheduleTemplateBlockId != shift.TimeScheduleTemplateBlockId))
                    select tb).Any();
        }

        #endregion

        #region TimeScheduleTemplateBlockHistory

        private List<TimeScheduleTemplateBlockHistory> GetShiftHistory(int timeScheduleTemplateBlockId)
        {
            return (from t in entities.TimeScheduleTemplateBlockHistory
                    where t.TimeScheduleTemplateBlockId == timeScheduleTemplateBlockId
                    orderby t.Created
                    select t).ToList();
        }

        #endregion

        #region TimeScheduleTemplateBlockTask

        private TimeScheduleTemplateBlockTask GetTimeScheduleTemplateBlockTask(int timeScheduleTemplateBlockTaskId)
        {
            if (timeScheduleTemplateBlockTaskId == 0)
                return null;

            return (from t in entities.TimeScheduleTemplateBlockTask
                    where t.TimeScheduleTemplateBlockTaskId == timeScheduleTemplateBlockTaskId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private List<TimeScheduleTemplateBlockTask> GetTimeScheduleTemplateBlockTasks(int timeScheduleTemplateBlockId)
        {
            return (from t in entities.TimeScheduleTemplateBlockTask
                    where
                    t.TimeScheduleTemplateBlockId.HasValue &&
                    t.TimeScheduleTemplateBlockId.Value == timeScheduleTemplateBlockId &&
                    t.ActorCompanyId == this.actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private List<TimeScheduleTemplateBlockTask> GetTimeScheduleTemplateBlockTasks(List<int> timeScheduleTemplateBlockIds)
        {
            List<TimeScheduleTemplateBlockTask> result = new List<TimeScheduleTemplateBlockTask>();

            if (timeScheduleTemplateBlockIds.IsNullOrEmpty())
                return result;

            BatchHelper batchHelper = BatchHelper.Create(timeScheduleTemplateBlockIds);
            while (batchHelper.HasMoreBatches())
            {
                var batchIds = batchHelper.GetCurrentBatchIds();
                var batch = (from t in entities.TimeScheduleTemplateBlockTask
                             where t.TimeScheduleTemplateBlockId.HasValue &&
                             batchIds.Contains(t.TimeScheduleTemplateBlockId.Value) &&
                             t.ActorCompanyId == this.actorCompanyId &&
                             t.State == (int)SoeEntityState.Active
                             select t).ToList();

                result.AddRange(batch);
                batchHelper.MoveToNextBatch();
            }

            return result;
        }

        private ActionResult CopyTimeScheduleTemplateBlockTasks(int fromTimeScheduleTemplateBlockId, int toTimeScheduleTemplateBlockId)
        {
            List<TimeScheduleTemplateBlockTaskDTO> tasks = GetTimeScheduleTemplateBlockTasks(fromTimeScheduleTemplateBlockId).ToDTOs().ToList();
            TimeScheduleTemplateBlock toTimeScheduleTemplateBlock = GetScheduleBlock(toTimeScheduleTemplateBlockId);
            return CopyTimeScheduleTemplateBlockTasks(tasks, toTimeScheduleTemplateBlock);
        }

        private ActionResult CopyTimeScheduleTemplateBlockTasks(List<TimeScheduleTemplateBlockTaskDTO> tasks, TimeScheduleTemplateBlock toTimeScheduleTemplateBlock)
        {
            if (toTimeScheduleTemplateBlock == null || !toTimeScheduleTemplateBlock.Date.HasValue)
                return new ActionResult();

            foreach (TimeScheduleTemplateBlockTaskDTO task in tasks)
            {
                task.TimeScheduleTemplateBlockTaskId = 0; //Create new task
                task.TimeScheduleTemplateBlockId = toTimeScheduleTemplateBlock.TimeScheduleTemplateBlockId;
                TimeSpan timeSpan = task.StopTime - task.StartTime;
                task.StartTime = CalendarUtility.MergeDateAndTime(toTimeScheduleTemplateBlock.Date.Value, task.StartTime);
                task.StopTime = task.StartTime.Add(timeSpan);

                ActionResult result = SaveTimeScheduleTemplateBlockTask(task);
                if (!result.Success)
                    return result;
            }

            return Save();
        }

        private ActionResult MoveTimeScheduleTemplateBlockTasks(int fromTimeScheduleTemplateBlockId, int toTimeScheduleTemplateBlockId)
        {
            var tasks = GetTimeScheduleTemplateBlockTasks(fromTimeScheduleTemplateBlockId).ToDTOs().ToList();
            var toTimeScheduleTemplateBlock = GetScheduleBlock(toTimeScheduleTemplateBlockId);
            return MoveTimeScheduleTemplateBlockTasks(tasks, toTimeScheduleTemplateBlock);

        }

        private ActionResult MoveTimeScheduleTemplateBlockTasks(List<TimeScheduleTemplateBlockTaskDTO> tasks, TimeScheduleTemplateBlock toTimeScheduleTemplateBlock)
        {
            if (toTimeScheduleTemplateBlock == null)
                return new ActionResult();

            foreach (var task in tasks)
            {
                task.TimeScheduleTemplateBlockId = toTimeScheduleTemplateBlock.TimeScheduleTemplateBlockId;//possibly new shiftId
                TimeSpan timeSpan = task.StopTime - task.StartTime;
                task.StartTime = CalendarUtility.MergeDateAndTime(toTimeScheduleTemplateBlock.Date.Value, task.StartTime);
                task.StopTime = task.StartTime.Add(timeSpan);
                ActionResult result = SaveTimeScheduleTemplateBlockTask(task);
                if (!result.Success)
                    return result;
            }

            return Save();
        }

        private TimeScheduleTemplateBlockTaskDTO CreateTimeScheduleTemplateBlockTaskDTO(StaffingNeedsTaskDTO needsTask, int timeScheduleTemplateBlockId, DateTime date)
        {
            if (!needsTask.StartTime.HasValue || !needsTask.StopTime.HasValue)
                return null;

            DateTime startTime = CalendarUtility.MergeDateAndTime(date, needsTask.StartTime.Value);

            return new TimeScheduleTemplateBlockTaskDTO()
            {
                TimeScheduleTemplateBlockId = timeScheduleTemplateBlockId,
                StartTime = startTime,
                StopTime = startTime.Add(needsTask.StopTime.Value - needsTask.StartTime.Value),
                TimeScheduleTaskId = needsTask.Type == SoeStaffingNeedsTaskType.Task ? needsTask.Id : (int?)null,
                IncomingDeliveryRowId = needsTask.Type == SoeStaffingNeedsTaskType.Delivery ? needsTask.Id : (int?)null,
                State = SoeEntityState.Active,
            };
        }

        private ActionResult SaveTimeScheduleTemplateBlockTasks(List<TimeScheduleTemplateBlockTaskDTO> tasks)
        {
            foreach (var task in tasks)
            {
                ActionResult result = SaveTimeScheduleTemplateBlockTask(task);
                if (!result.Success)
                    return result;
            }

            return Save();
        }

        private ActionResult SaveTimeScheduleTemplateBlockTask(TimeScheduleTemplateBlockTaskDTO inputTask, bool saveChanges = false)
        {
            // Get existing
            TimeScheduleTemplateBlockTask timeScheduleTemplateBlockTask = GetTimeScheduleTemplateBlockTask(inputTask.TimeScheduleTemplateBlockTaskId);
            if (timeScheduleTemplateBlockTask == null)
            {
                #region Add

                timeScheduleTemplateBlockTask = new TimeScheduleTemplateBlockTask()
                {
                    ActorCompanyId = actorCompanyId,
                };
                SetCreatedProperties(timeScheduleTemplateBlockTask);
                entities.TimeScheduleTemplateBlockTask.AddObject(timeScheduleTemplateBlockTask);

                #endregion
            }
            else
            {
                #region Update

                SetModifiedProperties(timeScheduleTemplateBlockTask);

                #endregion
            }

            timeScheduleTemplateBlockTask.TimeScheduleTemplateBlockId = inputTask.TimeScheduleTemplateBlockId;
            timeScheduleTemplateBlockTask.TimeScheduleTaskId = inputTask.TimeScheduleTaskId;
            timeScheduleTemplateBlockTask.IncomingDeliveryRowId = inputTask.IncomingDeliveryRowId;
            timeScheduleTemplateBlockTask.StartTime = inputTask.StartTime.RemoveSeconds();
            timeScheduleTemplateBlockTask.StopTime = inputTask.StopTime.RemoveSeconds();
            timeScheduleTemplateBlockTask.State = (int)inputTask.State;

            ActionResult result = new ActionResult();
            if (saveChanges)
                result = Save();

            return result;
        }

        private ActionResult SplitTasks(int shiftId, List<DateTime> times)
        {
            List<TimeScheduleTemplateBlockTaskDTO> originalTasks = GetTimeScheduleTemplateBlockTasks(shiftId).ToDTOs().ToList();
            List<TimeScheduleTemplateBlockTaskDTO> tasks = new List<TimeScheduleTemplateBlockTaskDTO>();
            tasks.AddRange(originalTasks);
            foreach (var time in times.OrderBy(x => x))
            {
                List<TimeScheduleTemplateBlockTaskDTO> newTasks = new List<TimeScheduleTemplateBlockTaskDTO>();
                foreach (var task in tasks)
                {
                    if (task.IsOverlapped(time))
                    {
                        #region Split task                                

                        // Copy DTO
                        TimeScheduleTemplateBlockTaskDTO taskClone = new TimeScheduleTemplateBlockTaskDTO();
                        EntityUtil.CopyDTO<TimeScheduleTemplateBlockTaskDTO>(taskClone, task);

                        //Change properties on original task
                        task.StopTime = time;

                        //Change properties on new task
                        taskClone.TimeScheduleTemplateBlockTaskId = 0;
                        taskClone.StartTime = time;
                        newTasks.Add(taskClone);

                        #endregion

                    }
                }
                tasks.AddRange(newTasks);
                tasks = tasks.OrderBy(x => x.StartTime).ToList();
            }

            return SaveTimeScheduleTemplateBlockTasks(tasks);
        }

        private ActionResult ConnectTasksWithinShift(List<TimeScheduleTemplateBlockTaskDTO> tasks, TimeSchedulePlanningDayDTO shift)
        {
            //Find tasks within shift
            List<TimeScheduleTemplateBlockTaskDTO> tasksWithinNewShift = tasks.Where(x => x.IsWithInRange(shift.StartTime, shift.StopTime)).ToList();
            foreach (var taskWithinShift in tasksWithinNewShift)
            {
                taskWithinShift.TimeScheduleTemplateBlockId = shift.TimeScheduleTemplateBlockId; //set parent
            }

            tasks.RemoveRange(tasksWithinNewShift);
            return SaveTimeScheduleTemplateBlockTasks(tasksWithinNewShift);
        }

        private ActionResult FreeTimeScheduleTemplateBlockTasks(int timeScheduleTemplateBlockId, bool saveChanges = false)
        {
            var tasks = GetTimeScheduleTemplateBlockTasks(timeScheduleTemplateBlockId).ToDTOs().ToList();
            return FreeTimeScheduleTemplateBlockTasks(tasks, saveChanges);
        }

        private ActionResult FreeTimeScheduleTemplateBlockTasks(List<int> timeScheduleTemplateBlockIds, bool saveChanges = false)
        {
            var tasks = GetTimeScheduleTemplateBlockTasks(timeScheduleTemplateBlockIds).ToDTOs().ToList();
            return FreeTimeScheduleTemplateBlockTasks(tasks, saveChanges);
        }

        private ActionResult FreeTimeScheduleTemplateBlockTasks(List<TimeScheduleTemplateBlockTaskDTO> tasks, bool saveChanges = false)
        {
            foreach (var task in tasks)
            {
                task.TimeScheduleTemplateBlockId = null;
                ActionResult result = SaveTimeScheduleTemplateBlockTask(task, saveChanges);
                if (!result.Success)
                    return result;
            }

            return Save();
        }

        #endregion

        #region TimeScheduleType

        private List<TimeScheduleType> GetTimeScheduleTypesWithFactor()
        {
            return (from t in entities.TimeScheduleType
                        .Include("TimeScheduleTypeFactor")
                    where t.ActorCompanyId == actorCompanyId &&
                    t.State == (int)SoeEntityState.Active
                    select t).ToList();
        }

        private TimeScheduleType GetTimeScheduleType(int timeScheduleTypeId)
        {
            return (from t in entities.TimeScheduleType
                    where t.ActorCompanyId == actorCompanyId &&
                    t.TimeScheduleTypeId == timeScheduleTypeId &&
                    t.State == (int)SoeEntityState.Active
                    select t).FirstOrDefault();
        }

        private List<TimeBlock> SplitTimeBlocksAfterTimeScheduleType(List<TimeBlock> timeBlocks, List<TimeScheduleTemplateBlock> templateBlocks)
        {
            if (timeBlocks.IsNullOrEmpty())
                return new List<TimeBlock>();
            if (templateBlocks.IsNullOrEmpty() || !templateBlocks.Any(i => i.TimeScheduleTypeId.HasValue))
                return timeBlocks;

            DateTime scheduleOut = templateBlocks.GetScheduleOut();

            List<TimeBlock> splittedTimeBlocks = new List<TimeBlock>();

            foreach (TimeBlock timeBlock in timeBlocks.GetWork(excludeGeneratedFromBreak: true))
            {
                #region TimeBlock

                //Keep original stoptime to determine when to stop splitting
                DateTime timeBlockStopTime = timeBlock.StopTime;

                //Split after TimeScheduleType
                DateTime scheduleTypeStopTime = templateBlocks.GetTemplateBlockScheduleTypeStopTime(timeBlock.StartTime, timeBlock.StopTime, scheduleOut, true);

                //Split while current stoptime is before original stoptime (max 5 trys)
                int maxTrys = 5;
                int currentTry = 1;
                while (scheduleTypeStopTime < timeBlockStopTime && currentTry <= maxTrys)
                {
                    //Update original TimeBlock
                    timeBlock.StopTime = scheduleTypeStopTime;

                    //Start values for excess TimeBlock
                    DateTime startTime = scheduleTypeStopTime;
                    DateTime stopTime = timeBlockStopTime;

                    //Split after TimeScheduleType
                    scheduleTypeStopTime = templateBlocks.GetTemplateBlockScheduleTypeStopTime(startTime, stopTime, scheduleOut, false);

                    //Change stoptime if original stoptime is not reached
                    if (timeBlockStopTime > scheduleTypeStopTime)
                        stopTime = scheduleTypeStopTime;

                    //Create excess TimeBlock
                    TimeBlock splittedTimeBlock = CreateTimeBlockWithoutAccounting(timeBlock, startTime, stopTime, 0, true);
                    if (splittedTimeBlock != null)
                        splittedTimeBlocks.Add(splittedTimeBlock);

                    currentTry++;
                }

                #endregion
            }

            //Add splitted TimeBlock's and Sort
            if (splittedTimeBlocks.Any())
                timeBlocks.AddRange(splittedTimeBlocks);

            return timeBlocks;
        }

        private void SetTimeBlockTypes(int employeeId, DateTime date, List<TimeBlock> timeBlocks, List<TimeScheduleTemplateBlock> scheduleBlocks, bool setScheduleTypeOnConnectedTimeBlocksOutsideSchedule)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            string methodName = nameof(SetTimeBlockTypes);
            string parameters = GenerateParameterKey(employeeId, date.ToShortDateString(), timeBlocks.Select(tb => tb.TimeBlockDateId).ToCommaSeparated(), scheduleBlocks?.Select(sb => sb.TimeScheduleTemplateBlockId).ToCommaSeparated(), setScheduleTypeOnConnectedTimeBlocksOutsideSchedule);
            if (HasMethodBeenExecutedBefore(methodName, parameters, keepOnlyLatest: true))
                return;

            List<TimeScheduleTemplateBlock> scheduleBlocksWork = scheduleBlocks
                .GetWork();
            List<int> timeScheduleTypesFromEmployeeSettings = GetEmployeeSettingsFromCache(employeeId, date)
                .FilterByTypes(TermGroup_EmployeeSettingType.Additions, TermGroup_EmployeeSettingType.Additions_ScheduleType)
                .Select(s => s.EmployeeSettingType)
                .ToList();

            List<int> timeScheduleTypesFromTimeLeisureCodes = new List<int>();
            TimeScheduleEmployeePeriod timeSchedulePeriod = null;

            if (base.HasTimeLeisureCodesFromCache(entities, actorCompanyId))
            {
                if (!scheduleBlocks.IsNullOrEmpty())
                    timeSchedulePeriod = GetTimeScheduleEmployeePeriodFromCache(scheduleBlocks.First().TimeScheduleEmployeePeriodId.Value);
                else
                    timeSchedulePeriod = GetTimeScheduleEmployeePeriodFromCache(employeeId, date);

                if (timeSchedulePeriod != null)
                {
                    var periodDetails = entities.TimeScheduleEmployeePeriodDetail
                        .Where(w => w.TimeScheduleEmployeePeriodId == timeSchedulePeriod.TimeScheduleEmployeePeriodId && w.State == (int)SoeEntityState.Active);

                    var leisureCodeIds = periodDetails
                        .Where(t => t.TimeLeisureCodeId.HasValue)
                        .Select(t => t.TimeLeisureCodeId.Value)
                        .ToList();

                    if (!leisureCodeIds.IsNullOrEmpty())
                    {
                        var employeeGroupId = GetEmployeeGroupFromCache(employeeId, date)?.EmployeeGroupId;
                        if (employeeGroupId.HasValue)
                            timeScheduleTypesFromTimeLeisureCodes.AddRange(GetTimeLeisureCodesTimeScheduleTypeIdsFromCache(leisureCodeIds, employeeGroupId.Value));
                    }
                }
            }

            foreach (TimeBlock timeBlock in timeBlocks.GetWork(excludeGeneratedFromBreak: true))
            {
                SetTimeBlockTimeScheduleTypes(timeBlock, scheduleBlocksWork, timeScheduleTypesFromEmployeeSettings, timeScheduleTypesFromTimeLeisureCodes, setScheduleTypeOnConnectedTimeBlocksOutsideSchedule);
                SetTimeBlockTimeStampEntryExtendedDetails(timeBlock);
                SetTimeBlockTimeDeviationCauseFromTimeScheduleType(timeBlock, scheduleBlocksWork);
            }
        }

        private void SetTimeBlockTimeScheduleTypes(TimeBlock timeBlock, List<TimeScheduleTemplateBlock> scheduleBlocksWork, List<int> timeScheduleTypesFromEmployeeSettings = null, List<int> timeScheduleTypesFromTimeLeisureCodes = null, bool acceptConnectedOutsideSchedule = false)
        {
            if (timeBlock == null)
                return;

            timeBlock.CalculatedShiftTypeId = timeBlock.ShiftTypeId;
            timeBlock.CalculatedTimeScheduleTypeId = timeBlock.TimeScheduleTypeId;

            TimeScheduleTemplateBlock matchingScheduleBlock = null;
            if (!timeBlock.CalculatedShiftTypeId.HasValue || !timeBlock.CalculatedTimeScheduleTypeId.HasValue && !scheduleBlocksWork.IsNullOrEmpty())
            {
                matchingScheduleBlock = scheduleBlocksWork.GetMatchingScheduleBlock(timeBlock, acceptConnectedOutsideSchedule);
                if (matchingScheduleBlock != null)
                {
                    if (!timeBlock.CalculatedShiftTypeId.HasValue && matchingScheduleBlock.ShiftTypeId.HasValue)
                        timeBlock.CalculatedShiftTypeId = matchingScheduleBlock.ShiftTypeId;
                    if (!timeBlock.CalculatedTimeScheduleTypeId.HasValue && matchingScheduleBlock.TimeScheduleTypeId.HasValue)
                        timeBlock.CalculatedTimeScheduleTypeId = matchingScheduleBlock.TimeScheduleTypeId;
                    if (matchingScheduleBlock.TimeScheduleTypeId.HasValue && matchingScheduleBlock.TimeScheduleTypeId.Value != timeBlock.CalculatedTimeScheduleTypeId)
                        timeBlock.CalculatedTimeScheduleTypeIdFromShift = matchingScheduleBlock.TimeScheduleTypeId;
                }
            }

            if (timeBlock.CalculatedShiftTypeId.HasValue && matchingScheduleBlock?.TimeScheduleTypeId != null)
                timeBlock.CalculatedTimeScheduleTypeIdFromShiftType = GetShiftTypeFromCache(timeBlock.CalculatedShiftTypeId.Value)?.TimeScheduleTypeId;

            if (!timeScheduleTypesFromEmployeeSettings.IsNullOrEmpty())
                timeBlock.CalculatedTimeScheduleTypeIdsFromEmployee = timeScheduleTypesFromEmployeeSettings;

            if (!timeScheduleTypesFromTimeLeisureCodes.IsNullOrEmpty())
                timeBlock.CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes = timeScheduleTypesFromTimeLeisureCodes;
        }

        private static void SetTimeBlockTimeStampEntryExtendedDetails(TimeBlock timeBlock)
        {
            if (timeBlock == null || timeBlock.TimeStampEntryExtendedDetails.IsNullOrEmpty())
                return;

            timeBlock.CalculatedTimeScheduleTypeIdsFromTimeStamp = JsonConvert
                .DeserializeObject<List<TimeStampEntryExtendedDetailsDTO>>(timeBlock.TimeStampEntryExtendedDetails)
                .Where(d => d.TimeScheduleTypeId.HasValue && d.TimeScheduleTypeId > 0)
                .Select(d => d.TimeScheduleTypeId.Value)
                .ToList();
        }

        private void SetTimeBlockTimeDeviationCauseFromTimeScheduleType(TimeBlock timeBlock, List<TimeScheduleTemplateBlock> scheduleBlocksWork)
        {
            if (timeBlock == null || !timeBlock.TimeDeviationCauseStartId.HasValue || scheduleBlocksWork.IsNullOrEmpty())
                return;

            //TODO: Only allow standard TimeDeviationCause?
            TimeDeviationCause timeDeviationCause = timeBlock.TimeDeviationCauseStart ?? GetTimeDeviationCauseFromCache(timeBlock.TimeDeviationCauseStartId.Value);
            if (timeDeviationCause?.Type != (int)TermGroup_TimeDeviationCauseType.Presence && timeDeviationCause?.Type != (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence)
                return;

            TimeScheduleTemplateBlock matchingScheduleBlock = scheduleBlocksWork.GetMatchingScheduleBlock(timeBlock, acceptConnectedOutsideSchedule: false);
            if (matchingScheduleBlock?.TimeScheduleTypeId == null)
                return;

            TimeScheduleType timeScheduleType = GetTimeScheduleTypeFromCache(matchingScheduleBlock.TimeScheduleTypeId.Value);
            if (timeScheduleType?.TimeDeviationCauseId == null || timeScheduleType.TimeDeviationCauseId == timeBlock.TimeDeviationCauseStartId)
                return;

            TimeDeviationCause newTimeDeviationCause = GetTimeDeviationCauseFromCache(timeScheduleType.TimeDeviationCauseId.Value);
            if (newTimeDeviationCause == null)
                return;

            timeBlock.TimeDeviationCauseStart = newTimeDeviationCause;
        }

        #endregion

        #region TimeTerminal

        private TimeTerminal GetTimeTerminal(int timeTerminalId, bool discardState = false)
        {
            return (from t in entities.TimeTerminal
                    where t.TimeTerminalId == timeTerminalId &&
                    t.ActorCompanyId == actorCompanyId &&
                    (discardState || t.State == (int)SoeEntityState.Active)
                    select t).FirstOrDefault();
        }

        private int GetTimeTerminalAccountDimId(int timeTerminalId)
        {
            return TimeStampManager.GetTimeTerminalAccountDimId(entities, actorCompanyId, timeTerminalId);
        }

        #endregion

        #region TimeScheduleTemplateBlockQueue

        private bool IsEmployeeInShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType type, int timeScheduleTemplateBlockId, int employeeId)
        {
            List<TimeScheduleTemplateBlockQueue> queues = TimeScheduleManager.GetShiftQueue(entities, timeScheduleTemplateBlockId, type);
            return queues.Any(q => q.EmployeeId == employeeId);
        }

        private List<int> GetShiftsWhereEmployeeIsInQueue(TermGroup_TimeScheduleTemplateBlockQueueType type, int employeeId, DateTime startTime, DateTime stopTime)
        {
            DateTime scheduleStartTime = CalendarUtility.GetScheduleTime(startTime);
            DateTime scheduleStopTime = CalendarUtility.GetScheduleTime(stopTime, startTime, stopTime);

            // Get queues that intersects with specified time interval
            IEnumerable<TimeScheduleTemplateBlockQueue> queues = (from q in entities.TimeScheduleTemplateBlockQueue
                                                                  where q.Type == (int)type &&
                                                                  q.EmployeeId == employeeId &&
                                                                  q.TimeScheduleTemplateBlock.Date == startTime.Date &&
                                                                  q.TimeScheduleTemplateBlock.StartTime < scheduleStopTime &&
                                                                  q.TimeScheduleTemplateBlock.StopTime > scheduleStartTime
                                                                  select q);
            List<int> shiftIds = new List<int>();
            foreach (var queue in queues.Where(w => w.State == (int)SoeEntityState.Active))
            {
                shiftIds.Add(queue.TimeScheduleTemplateBlockId);
            }

            return shiftIds;
        }

        private ActionResult AddEmployeeToShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType type, int shiftId, int employeeId, bool includeLinkedShifts)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();
            List<TimeScheduleTemplateBlock> linkedTemplateBlocks = new List<TimeScheduleTemplateBlock>();
            var shift = GetScheduleBlock(shiftId);

            templateBlocks.Add(shift);
            if (includeLinkedShifts)
                linkedTemplateBlocks = GetScheduleBlocksLinked(null, shift).Where(x => x.TimeScheduleTemplateBlockId != shift.TimeScheduleTemplateBlockId).ToList();

            templateBlocks.AddRange(linkedTemplateBlocks);

            foreach (var templateBlock in templateBlocks)
            {
                #region Prereq

                if (IsEmployeeInShiftQueue(type, templateBlock.TimeScheduleTemplateBlockId, employeeId))
                    return new ActionResult((int)ActionResultSave.HandleTimeScheduleShift_EmployeeAlreadyInQueue, GetText(9337, "Passet är redan önskat"));

                bool isHidden = (templateBlock.EmployeeId == GetHiddenEmployeeIdFromCache() && templateBlock.ShiftStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftStatus.Open);
                bool isUnwanted = templateBlock.ShiftUserStatus == (int)TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted;
                if (!isHidden && !isUnwanted)
                    return new ActionResult((int)ActionResultSave.HandleTimeScheduleShift_EmployeeAlreadyInQueue, String.Format(GetText(405, (int)TermGroup.XEMailGrid, "Annan anställd tilldelades önskat pass {0}"), string.Empty));

                // Get next sort number in queue
                List<TimeScheduleTemplateBlockQueue> queues = TimeScheduleManager.GetShiftQueue(entities, templateBlock.TimeScheduleTemplateBlockId, type);
                int sort = queues.IsNullOrEmpty() ? 0 : queues[queues.Count - 1].Sort;
                sort++;

                #endregion

                #region TimeScheduleTemplateBlockQueue

                TimeScheduleTemplateBlockQueue queue = new TimeScheduleTemplateBlockQueue()
                {
                    TimeScheduleTemplateBlockId = templateBlock.TimeScheduleTemplateBlockId,
                    Type = (int)type,
                    Date = DateTime.Now,
                    EmployeeId = employeeId,
                    Sort = sort,
                    Status = (int)TermGroup_TimeScheduleTemplateBlockQueueStatus.Active
                };

                SetCreatedProperties(queue);
                entities.TimeScheduleTemplateBlockQueue.AddObject(queue);

                #endregion

                #region TimeScheduleTemplateBlock

                // Update number in queue                
                if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Suggestion)
                    templateBlock.NbrOfSuggestionsInQueue++;
                else if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Wanted)
                    templateBlock.NbrOfWantedInQueue++;
                SetModifiedProperties(templateBlock);

                #endregion
            }

            ActionResult result = Save();

            return result;
        }

        private ActionResult RemoveEmployeeFromShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType type, int shiftId, int employeeId, bool includeLinkedShifts)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();

            var shift = GetScheduleBlock(shiftId);
            if (shift != null)
            {
                templateBlocks.Add(shift);
                if (includeLinkedShifts)
                {
                    List<TimeScheduleTemplateBlock> linkedTemplateBlocks = GetScheduleBlocksLinked(null, shift).Where(x => x.TimeScheduleTemplateBlockId != shift.TimeScheduleTemplateBlockId).ToList();
                    templateBlocks.AddRange(linkedTemplateBlocks);
                }
            }

            foreach (var templateBlock in templateBlocks)
            {
                #region TimeScheduleTemplateBlockQueue

                // Get shift queue
                List<TimeScheduleTemplateBlockQueue> queues = TimeScheduleManager.GetShiftQueue(entities, templateBlock.TimeScheduleTemplateBlockId, type);
                TimeScheduleTemplateBlockQueue queue = queues.FirstOrDefault(q => q.EmployeeId == employeeId);
                var employee = EmployeeManager.GetEmployeeByUser(entities, ActorCompanyId, UserId);

                if (queue != null)
                {
                    SetModifiedProperties(queue);
                    queue.State = (int)SoeEntityState.Deleted;

                    if (employee != null && employee.EmployeeId == queue.EmployeeId)
                        queue.Status = (int)TermGroup_TimeScheduleTemplateBlockQueueStatus.DeletedByEmployee;
                    else
                        queue.Status = (int)TermGroup_TimeScheduleTemplateBlockQueueStatus.DeletedByAdmin;

                    // Resort queue
                    if (queues.Any())
                    {
                        int i = 1;
                        foreach (var que in queues.Where(q => q.TimeScheduleTemplateBlockQueueId != queue.TimeScheduleTemplateBlockQueueId).OrderBy(q => q.Sort))
                        {
                            if (que.Sort != i)
                            {
                                que.Sort = i;
                                SetModifiedProperties(que);
                            }
                            i++;
                        }
                    }

                    #region TimeScheduleTemplateBlock

                    // Update number in queue                    
                    if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Suggestion)
                        templateBlock.NbrOfSuggestionsInQueue = queues.Count;
                    else if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Wanted)
                        templateBlock.NbrOfWantedInQueue = queues.Count(w => w.State == (int)SoeEntityState.Active);
                    SetModifiedProperties(templateBlock);

                    #endregion
                }

                #endregion
            }

            ActionResult result = Save();
            if (!result.Success)
                LogError($"RemoveEmployeeFromShiftQueue failed. EmployeeId:{employeeId}. Date:{templateBlocks.FirstOrDefault()?.Date}. ShiftId:{templateBlocks.FirstOrDefault()?.TimeScheduleTemplateBlockId}");

            return result;
        }

        private ActionResult ClearShiftQueue(int timeScheduleTemplateBlockId, TermGroup_TimeScheduleTemplateBlockQueueType type = TermGroup_TimeScheduleTemplateBlockQueueType.Unspecified)
        {
            #region TimeScheduleTemplateBlockQueue

            // Get shift queue                                                                                                                  
            List<TimeScheduleTemplateBlockQueue> queues = TimeScheduleManager.GetShiftQueue(entities, timeScheduleTemplateBlockId, type);
            var employee = EmployeeManager.GetEmployeeByUser(entities, ActorCompanyId, UserId);
            foreach (var queue in queues)
            {
                SetModifiedProperties(queue);
                queue.State = (int)SoeEntityState.Deleted;
                if (employee != null && employee.EmployeeId == queue.EmployeeId)
                    queue.Status = (int)TermGroup_TimeScheduleTemplateBlockQueueStatus.DeletedByEmployee;
                else
                    queue.Status = (int)TermGroup_TimeScheduleTemplateBlockQueueStatus.DeletedByAdmin;
            }

            #endregion

            #region TimeScheduleTemplateBlock

            // Update number in queue
            TimeScheduleTemplateBlock templateBlock = GetScheduleBlockWithPeriodAndStaffing(timeScheduleTemplateBlockId);
            if (templateBlock != null)
            {
                if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Suggestion)
                    templateBlock.NbrOfSuggestionsInQueue = 0;
                else if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Wanted)
                    templateBlock.NbrOfWantedInQueue = 0;
                else if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Unspecified)
                {
                    templateBlock.NbrOfSuggestionsInQueue = 0;
                    templateBlock.NbrOfWantedInQueue = 0;
                }
                SetModifiedProperties(templateBlock);
            }

            #endregion

            ActionResult result = Save();

            return result;
        }
        private ActionResult ClearShiftQueue(TimeScheduleTemplateBlock timeScheduleTemplateBlock, TermGroup_TimeScheduleTemplateBlockQueueType type)
        {
            if (!timeScheduleTemplateBlock.TimeScheduleTemplateBlockQueue.IsLoaded)
                timeScheduleTemplateBlock.TimeScheduleTemplateBlockQueue.Load();
            List<TimeScheduleTemplateBlockQueue> queues = timeScheduleTemplateBlock.TimeScheduleTemplateBlockQueue.Where(x => x.Type == (int)type).ToList();
            var employee = EmployeeManager.GetEmployeeByUser(entities, ActorCompanyId, UserId);

            foreach (var queue in queues)
            {
                SetModifiedProperties(queue);
                queue.State = (int)SoeEntityState.Deleted;
                if (employee != null && employee.EmployeeId == queue.EmployeeId)
                    queue.Status = (int)TermGroup_TimeScheduleTemplateBlockQueueStatus.DeletedByEmployee;
                else
                    queue.Status = (int)TermGroup_TimeScheduleTemplateBlockQueueStatus.DeletedByAdmin;
            }

            if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Suggestion)
                timeScheduleTemplateBlock.NbrOfSuggestionsInQueue = 0;
            else if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Wanted)
                timeScheduleTemplateBlock.NbrOfWantedInQueue = 0;
            else if (type == TermGroup_TimeScheduleTemplateBlockQueueType.Unspecified)
            {
                timeScheduleTemplateBlock.NbrOfSuggestionsInQueue = 0;
                timeScheduleTemplateBlock.NbrOfWantedInQueue = 0;
            }
            SetModifiedProperties(timeScheduleTemplateBlock);

            ActionResult result = Save();
            if (!result.Success)
                LogError($"ClearShiftQueue failed. Date:{timeScheduleTemplateBlock.Date}. ShiftId:{timeScheduleTemplateBlock.TimeScheduleTemplateBlockId}");

            return result;
        }

        #endregion

        #region TimeScheduleSwapRequest

        private TimeScheduleSwapRequestRow CreateTimeScheduleSwapRequestRow(TimeSchedulePlanningDayDTO shift, TimeScheduleSwapRequest swapRequest, TermGroup_TimeScheduleSwapRequestRowStatus status)
        {
            int length = CalendarUtility.GetLength(shift.StartTime, shift.StopTime) - shift.GetMyBreaks().Sum(x => x.BreakMinutes);

            TimeScheduleSwapRequestRow row = new TimeScheduleSwapRequestRow()
            {
                EmployeeId = shift.EmployeeId,
                Date = shift.ActualDate,
                ShiftsInfo = shift.SwapShiftInfo,
                ScheduleStart = shift.StartTime,
                ScheduleStop = shift.StopTime,
                Status = (int)status,
                State = (int)SoeEntityState.Active,
                TimeScheduleSwapRequest = swapRequest,
                ShiftLength = length
            };

            SetCreatedProperties(row);
            entities.AddToTimeScheduleSwapRequestRow(row);

            return row;
        }

        private TimeScheduleSwapRequest GetTimeScheduleSwapRequest(int timeScheduleSwapRequestId)
        {
            TimeScheduleSwapRequest requests = (from t in entities.TimeScheduleSwapRequest.Include("TimeScheduleSwapRequestRow")
                                                where t.ActorCompanyId == actorCompanyId &&
                                                  t.TimeScheduleSwapRequestId == timeScheduleSwapRequestId
                                                select t).FirstOrDefault();
            return requests;
        }

        private ActionResult ApproveScheduleSwapRequestByEmployee(TimeScheduleSwapRequest timeScheduleSwapRequest, int employeeId, bool approved, bool isInitiator)
        {

            if (!isInitiator)
            {
                if (approved)
                {
                    timeScheduleSwapRequest.AcceptedDate = DateTime.Now;
                    timeScheduleSwapRequest.AcceptorEmployeeId = employeeId;
                    timeScheduleSwapRequest.AcceptorUserId = base.UserId;
                }

                List<TimeScheduleSwapRequestRow> timeSheduleSwapRequestForEmployee = timeScheduleSwapRequest.TimeScheduleSwapRequestRow.Where(e => e.EmployeeId == employeeId && e.State == (int)SoeEntityState.Active).ToList();

                foreach (TimeScheduleSwapRequestRow timeScheduleSwapRequestRow in timeSheduleSwapRequestForEmployee)
                {
                    if (approved)
                        timeScheduleSwapRequestRow.Status = (int)TermGroup_TimeScheduleSwapRequestRowStatus.ApprovedByEmployee;
                    else
                        timeScheduleSwapRequestRow.Status = (int)TermGroup_TimeScheduleSwapRequestRowStatus.NotApprovedByEmployee;

                    SetModifiedProperties(timeScheduleSwapRequestRow);
                }
            }
            if (!approved)
            {
                timeScheduleSwapRequest.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(timeScheduleSwapRequest);

                foreach (TimeScheduleSwapRequestRow timeScheduleSwapRequestRow in timeScheduleSwapRequest.TimeScheduleSwapRequestRow)
                {
                    timeScheduleSwapRequestRow.State = (int)SoeEntityState.Deleted;
                    SetModifiedProperties(timeScheduleSwapRequestRow);
                }
            }
            ActionResult result = Save();

            return result;
        }

        private ActionResult ApproveScheduleSwapRequestByAdmin(TimeScheduleSwapRequest timeScheduleSwapRequest, bool approved, out bool allRowsApproved, out bool allRowsDenied)
        {
            ActionResult result;
            allRowsApproved = false;
            allRowsDenied = false;
            foreach (TimeScheduleSwapRequestRow timeScheduleSwapRequestRow in timeScheduleSwapRequest.TimeScheduleSwapRequestRow)
            {
                if (TimeScheduleManager.ValidateEmployeeSwapRequestRowAdmin(entities, actorCompanyId, timeScheduleSwapRequestRow.EmployeeId, base.UserId, timeScheduleSwapRequestRow.Date))
                {
                    timeScheduleSwapRequestRow.Status = approved ? (int)TermGroup_TimeScheduleSwapRequestRowStatus.ApprovedByAdmin : (int)TermGroup_TimeScheduleSwapRequestRowStatus.NotApprovedByAdmin;
                    SetModifiedProperties(timeScheduleSwapRequestRow);
                }

            }

            if (timeScheduleSwapRequest.TimeScheduleSwapRequestRow.All(w => w.Status == (int)TermGroup_TimeScheduleSwapRequestRowStatus.ApprovedByAdmin))
                allRowsApproved = true;

            else if (timeScheduleSwapRequest.TimeScheduleSwapRequestRow.All(w => w.Status == (int)TermGroup_TimeScheduleSwapRequestRowStatus.NotApprovedByAdmin))
                allRowsDenied = true;

            if (allRowsApproved && TimeScheduleManager.GetEmployeeSwapRequestRowWithShifts(entities, base.ActorCompanyId, timeScheduleSwapRequest.TimeScheduleSwapRequestId, out List<TimeScheduleTemplateBlock> initiatorShifts, out List<TimeScheduleTemplateBlock> swapwithShifts))
            {
                result = SwapTimeScheduleShifts(initiatorShifts, swapwithShifts, TermGroup_ShiftHistoryType.ScheduleSwapRequest, true);

                if (!result.Success)
                    return result;
                else
                {
                    timeScheduleSwapRequest.Status = (int)TermGroup_TimeScheduleSwapRequestStatus.Done;
                    timeScheduleSwapRequest.ApprovedDate = DateTime.Now;
                    timeScheduleSwapRequest.ApprovedBy = GetUserDetails();
                    SetModifiedProperties(timeScheduleSwapRequest);
                }

            }
            else if (allRowsDenied)
            {
                timeScheduleSwapRequest.State = (int)SoeEntityState.Deleted;
                SetModifiedProperties(timeScheduleSwapRequest);
            }

            result = Save();

            return result;
        }
        #endregion

        #region TimeWorkReductionEarning

        public bool UseTimeWorkReduction()
        {
            return base.UseTimeWorkReductionFromCache(entities, actorCompanyId);
        }

        public List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroup> GetEarningTimeAccumulatorEmployeeGroups(int employeeGroupId, DateTime dateFrom, DateTime dateTo)
        {
            return entities.TimeAccumulatorTimeWorkReductionEarningEmployeeGroup
                .Include(ta => ta.TimeWorkReductionEarning.TimeAccumulator)
                .Where(eg =>
                    eg.EmployeeGroupId == employeeGroupId &&
                    eg.State == (int)SoeEntityState.Active &&
                    (eg.DateFrom == null || eg.DateFrom <= dateTo) &&
                    (eg.DateTo == null || eg.DateTo >= dateFrom) &&
                    eg.TimeWorkReductionEarning.State == (int)SoeEntityState.Active)
                .ToList();
        }

        #endregion

        #region UnionFee

        private UnionFee GetUnionFeeWithPriceTypeAndPeriods(int unionFeeId)
        {
            return (from u in entities.UnionFee
                    .Include("PayrollPriceTypePercent.PayrollPriceTypePeriod")
                    .Include("PayrollPriceTypePercentCeiling.PayrollPriceTypePeriod")
                    .Include("PayrollPriceTypeFixedAmount.PayrollPriceTypePeriod")
                    where u.UnionFeeId == unionFeeId &&
                    u.ActorCompanyId == actorCompanyId &&
                    u.State == (int)SoeEntityState.Active
                    select u).FirstOrDefault();
        }

        #endregion

        #region User

        private User GetUser(int userId)
        {
            return UserManager.GetUser(entities, userId);
        }

        #endregion

        #region VacationGroup

        private VacationGroup GetVacationGroupWithVacationGroupSE(int vacationGroupId)
        {
            return (from vg in entities.VacationGroup.Include("VacationGroupSE")
                    where vg.VacationGroupId == vacationGroupId &&
                    vg.State == (int)SoeEntityState.Active
                    select vg).FirstOrDefault();
        }

        private List<VacationGroup> GetVacationGroupsWithSE(int actorCompanyId)
        {
            return (from vg in entities.VacationGroup.Include("VacationGroupSE")
                    where vg.ActorCompanyId == actorCompanyId &&
                    vg.State == (int)SoeEntityState.Active
                    select vg).ToList();
        }

        #endregion

        #region VacationGroupSE

        private VacationGroupSE GetVacationGroupSE(int vacationGroupId)
        {
            return (from t in entities.VacationGroupSE
                    where t.VacationGroupId == vacationGroupId
                    select t).FirstOrDefault();
        }

        private List<VacationGroupSEDayType> GetVacationGroupSEDayTypes(int vacationGroupSEId)
        {
            return (from evg in entities.VacationGroupSEDayType
                    where evg.VacationGroupSEId == vacationGroupSEId &&
                    evg.State == (int)SoeEntityState.Active
                    select evg).ToList();
        }

        #endregion
    }
}
