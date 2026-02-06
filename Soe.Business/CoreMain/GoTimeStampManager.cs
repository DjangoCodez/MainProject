using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.Business.Core
{
    public class GoTimeStampManager : ManagerBase
    {
        #region Ctor

        public GoTimeStampManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Account

        public List<Account> GetAccounts(int actorCompanyId, int timeTerminalId, int employeeId, int dimNr)
        {
            // Get terminal
            TimeTerminal terminal = TimeStampManager.GetTimeTerminalDiscardState(timeTerminalId);
            if (terminal == null)
                return new List<Account>();

            // Get terminal account dim
            int accountDimId = TimeStampManager.GetTimeTerminalIntSetting(dimNr == 1 ? TimeTerminalSettingType.AccountDim : TimeTerminalSettingType.AccountDim2, terminal);
            if (accountDimId == 0)
                return new List<Account>();

            // Get all accounts for account dim
            List<Account> allAccounts = AccountManager.GetAccountsByDim(accountDimId, actorCompanyId, true);

            // Check account limits
            List<int> validAccountIds = new List<int>();
            bool limitSelectableAccounts = TimeStampManager.GetTimeTerminalBoolSetting(dimNr == 1 ? TimeTerminalSettingType.LimitSelectableAccounts : TimeTerminalSettingType.LimitSelectableAccounts2, terminal);
            if (limitSelectableAccounts)
            {
                string selectedAccounts = TimeStampManager.GetTimeTerminalStringSetting(dimNr == 1 ? TimeTerminalSettingType.SelectedAccounts : TimeTerminalSettingType.SelectedAccounts2, terminal);
                if (!string.IsNullOrEmpty(selectedAccounts))
                    validAccountIds = selectedAccounts.Split(',').Select(a => int.Parse(a)).ToList();
            }
            else
            {
                // If not limited, return all accounts
                bool limitToAccounts = TimeStampManager.GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, terminal);
                bool limitToCategories = TimeStampManager.GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, terminal);
                if (!limitToAccounts && !limitToCategories)
                    return allAccounts.Where(a => !a.HierarchyOnly).ToList();

                List<int> allAccountIds = allAccounts.Select(a => a.AccountId).ToList();
                validAccountIds = limitToAccounts ? GetAccountIdsWithAccountLimits(actorCompanyId, timeTerminalId, allAccountIds) : GetAccountsWithCategoryLimits(actorCompanyId, timeTerminalId, allAccountIds);
            }

            return allAccounts.Where(a => !a.HierarchyOnly && validAccountIds.Contains(a.AccountId)).ToList();
        }

        private List<int> GetAccountIdsWithAccountLimits(int actorCompanyId, int timeTerminalId, List<int> accountIds)
        {
            // Get accounts linked to terminal
            List<int> terminalAccountIds = TimeStampManager.GetAccountIdsByTimeTerminal(timeTerminalId);
            List<int> terminalHierarchyAccountIds = new List<int>();

            AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.IncludeVirtualParented);
            foreach (int terminalAccountId in terminalAccountIds)
            {
                terminalHierarchyAccountIds.AddRange(AccountManager.GetAccountsFromHierarchyById(actorCompanyId, terminalAccountId, input).Select(a => a.AccountId));
            }
            terminalHierarchyAccountIds = terminalHierarchyAccountIds.Distinct().ToList();

            return accountIds.Where(a => terminalHierarchyAccountIds.Contains(a)).ToList();
        }

        private List<int> GetAccountsWithCategoryLimits(int actorCompanyId, int timeTerminalId, List<int> accountIds)
        {
            List<int> validAccountIds = new List<int>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.CategoryAccount.NoTracking();

            // Get categories linked to terminal
            List<int> terminalCategoryIds = TimeStampManager.GetCategoriesByTimeTerminal(actorCompanyId, timeTerminalId).Select(c => c.CategoryId).ToList();

            // Loop through accounts and check if they are connected to a category specified on the terminal
            entitiesReadOnly.CategoryAccount.NoTracking();
            foreach (int accountId in accountIds)
            {
                List<int> catAccountIds = (from ca in entitiesReadOnly.CategoryAccount
                                           where ca.AccountId == accountId &&
                                           ca.State == (int)SoeEntityState.Active
                                           select ca.CategoryId).ToList();

                if (terminalCategoryIds.ContainsAny(catAccountIds))
                    validAccountIds.Add(accountId);
            }

            return validAccountIds;
        }

        #endregion

        #region DeviationCause

        public List<TimeDeviationCause> SyncDeviationCauses(int actorCompanyId, int timeTerminalId)
        {
            return TimeDeviationCauseManager.GetTimeDeviationCauses(actorCompanyId);
        }

        public List<TimeDeviationCause> GetCurrentDeviationCauses(int actorCompanyId, int timeTerminalId, int employeeId, DateTime localTime)
        {
            return TimeDeviationCauseManager.GetTimeDeviationCausesCurrent(actorCompanyId, employeeId, localTime);
        }

        #endregion

        #region Employee

        public GoTimeStampSyncEmployeeResult SyncEmployees(int actorCompanyId, int timeTerminalId, bool? includeLastEmployeeGroup = false, bool? includeDeviationCauses = false, int? employeeId = null)
        {
            GoTimeStampSyncEmployeeResult result = new GoTimeStampSyncEmployeeResult();

            // Get valid employees for terminal
            List<int> employeeIds = TimeStampManager.GetEmployeeIdsForTerminal(actorCompanyId, timeTerminalId);

            if (employeeId.HasValue && employeeIds != null && !employeeIds.Contains(employeeId.Value))
            {
                // Only one employee should be fetched and it is not valid for this terminal
                // Return empty employee collection to remove it on the terminal
                result.Employees = new List<GoTimeStampEmployee>();
                result.Success = true;
                return result;
            }
            else
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                entities.EmployeeGroup.NoTracking();
                entities.Employee.NoTracking();

                List<EmployeeGroup> employeeGroups = EmployeeManager.GetEmployeeGroups(entities, actorCompanyId);


                IQueryable<Employee> query = (from e in entities.Employee
                                                .Include("ContactPerson")
                                                .Include("Employment.EmploymentChangeBatch.EmploymentChange")
                                              where e.ActorCompanyId == actorCompanyId &&
                                              !e.Hidden &&
                                              e.State < (int)SoeEntityState.Temporary
                                              select e);

                // Get one or all valid employees
                if (employeeId.HasValue)
                    query = query.Where(e => e.EmployeeId == employeeId.Value);
                else if (employeeIds != null)
                    query = query.Where(e => employeeIds.Contains(e.EmployeeId));

                // Execute query
                List<Employee> employees = query.ToList();

                result.Employees = employees.Where(e => e.CurrentEmployeeGroupAutogenTimeblocks == false ||
                    (includeLastEmployeeGroup == true && e.CurrentEmployeeGroup == null && e.GetLastEmployeeGroup(null) != null && !e.GetLastEmployeeGroup(null).AutogenTimeblocks)).ToGoTimeStampDTOs();

                // Add employees that only have future employment.
                foreach (Employee employee in employees)
                {
                    // Prevent duplicates
                    if (result.Employees.Select(s => s.EmployeeId).Contains(employee.EmployeeId))
                        continue;

                    EmployeeGroup employeeGroup = employee.GetNextEmployeeGroup(DateTime.Today, employeeGroups);
                    if (employeeGroup == null || employeeGroup.AutogenTimeblocks)
                        continue;

                    result.Employees.Add(employee.ToGoTimeStampDTO());
                }

                if (includeDeviationCauses == true)
                {
                    foreach (GoTimeStampEmployee emp in result.Employees)
                    {
                        Employee employee = employees.FirstOrDefault(e => e.EmployeeId == emp.EmployeeId);
                        if (employee != null)
                        {
                            EmployeeGroup group = employee.GetCurrentEmployeeGroup(employeeGroups);
                            if (group != null)
                            {
                                emp.EmployeeGroupId = group.EmployeeGroupId;

                                if (!group.EmployeeGroupTimeDeviationCause.IsLoaded)
                                    group.EmployeeGroupTimeDeviationCause.Load();
                                emp.DeviationCauseIds = group.EmployeeGroupTimeDeviationCause.Where(c => c.UseInTimeTerminal).Select(c => c.TimeDeviationCauseId).ToList();
                            }
                        }
                    }
                }
            }

            result.Success = true;
            return result;
        }

        public GoTimeStampEmployee GetEmployeeByCardNumber(int actorCompanyId, int timeTerminalId, string cardNumber)
        {
            // Find employee
            Employee employee = EmployeeManager.GetEmployeeByCardNumber(actorCompanyId, cardNumber);
            if (employee == null)
                return null;

            if (!ValidateEmployeeForTerminal(actorCompanyId, timeTerminalId, employee).Success)
                return null;

            // Create employee return object
            GoTimeStampEmployee gtsEmployee = employee.ToGoTimeStampDTO();

            // Add deviation causes
            EmployeeGroup group = employee.CurrentEmployeeGroup;
            if (group != null)
            {
                gtsEmployee.EmployeeGroupId = group.EmployeeGroupId;

                if (!group.EmployeeGroupTimeDeviationCause.IsLoaded)
                    group.EmployeeGroupTimeDeviationCause.Load();
                gtsEmployee.DeviationCauseIds = group.EmployeeGroupTimeDeviationCause.Select(c => c.TimeDeviationCauseId).ToList();
            }

            return gtsEmployee;
        }

        public ActionResult LinkCardNumberToEmployee(int actorCompanyId, int timeTerminalId, string cardNumber, string employeeNr)
        {
            using (CompEntities entities = new CompEntities())
            {
                Employee employee = EmployeeManager.GetEmployeeByNr(entities, employeeNr, actorCompanyId);
                if (employee == null)
                    return new ActionResult((int)ActionResultSave.EmployeeNrNotFound, String.Format(GetText(3533, "Ingen anställd med nummer {0} funnen"), employeeNr));

                if (!string.IsNullOrEmpty(employee.CardNumber) && employee.CardNumber != cardNumber)
                    return new ActionResult((int)ActionResultSave.EmployeeHasAnotherCardNumber, String.Format(GetText(12514, "Anställd med nummer {0} har redan en annan bricka/kort kopplad. Administratör måste ta bort denna innan ny kan registreras."), employeeNr));

                Employee existing = EmployeeManager.GetEmployeeByCardNumber(entities, actorCompanyId, cardNumber, loadEmployment: true);
                if (existing != null)
                    return new ActionResult((int)ActionResultSave.CardNumberExistsOnAnotherEmployee, String.Format(GetText(12015, "Denna bricka/kort är redan kopplad till anställd med nummer {0}"), existing.EmployeeNr));

                ActionResult validateResult = ValidateEmployeeForTerminal(actorCompanyId, timeTerminalId, employee);
                if (!validateResult.Success)
                    return validateResult;

                employee.CardNumber = cardNumber;
                SetModifiedProperties(employee);

                return SaveChanges(entities);
            }
        }

        private ActionResult ValidateEmployeeForTerminal(int actorCompanyId, int timeTerminalId, Employee employee)
        {
            // Validate terminal settings
            if (TimeStampManager.GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, timeTerminalId))
            {
                // Category
                List<int> employeeIds = TimeStampManager.GetEmployeeIdsByTimeTerminalCategory(actorCompanyId, timeTerminalId);
                if (!employeeIds.Contains(employee.EmployeeId))
                    return new ActionResult((int)ActionResultSave.EmployeeNotValidForTerminal, String.Format(GetText(12012, "Anställd med nummer {0} tillhör ej någon kategori som är kopplad till denna terminal"), employee.EmployeeNr));
            }
            else if (TimeStampManager.GetTimeTerminalBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, timeTerminalId))
            {
                // Account
                List<int> employeeIds = TimeStampManager.GetEmployeeIdsByTimeTerminalAccount(actorCompanyId, timeTerminalId);
                if (!employeeIds.Contains(employee.EmployeeId))
                    return new ActionResult((int)ActionResultSave.EmployeeNotValidForTerminal, String.Format(GetText(12013, "Anställd med nummer {0} har ej någon tillhörighet som är kopplad till denna terminal"), employee.EmployeeNr));
            }

            // Validate employee group
            EmployeeGroup group = employee.CurrentEmployeeGroup;
            if (group == null)
                return new ActionResult((int)ActionResultSave.EmployeeHasNoGroup);

            bool? autogenTimeblocks = employee.CurrentEmployeeGroupAutogenTimeblocks;
            if (!autogenTimeblocks.HasValue || autogenTimeblocks.Value)
                return new ActionResult((int)ActionResultSave.EmployeeGroupNotValidForTerminal, String.Format(GetText(12014, "Anställd med nummer {0} tillhör ett tidavtal som ej är konfigurerat för stämpling"), employee.EmployeeNr));

            return new ActionResult(true);
        }

        public List<GoTimeStampEmployeeStampStatus> GetTimeStampAttendance(int actorCompanyId, int timeTerminalId, bool includeImage)
        {
            List<GoTimeStampEmployeeStampStatus> statuses = new List<GoTimeStampEmployeeStampStatus>();

            List<TimeStampAttendanceGaugeDTO> employeesStampedIn = DashboardManager.GetTimeStampAttendance(actorCompanyId, 0, 0, TermGroup_TimeStampAttendanceGaugeShowMode.AllLast24Hours, true, onlyIncludeAttestRoleEmployees: false, includeEmployeeNrInString: false, timeTerminalId: timeTerminalId, includeBreaks: true).ToList();

            foreach (TimeStampAttendanceGaugeDTO employee in employeesStampedIn)
            {
                GoTimeStampEmployee emp = new GoTimeStampEmployee()
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeNr = employee.EmployeeNr,
                    Name = employee.Name,
                };

                statuses.Add(new GoTimeStampEmployeeStampStatus()
                {
                    Employee = emp,
                    StampedIn = employee.Type == TimeStampEntryType.In,
                    TimeStamp = employee.Time,
                    OnBreak = employee.IsBreak,
                    OnPaidBreak = employee.IsPaidBreak,
                    OnDistanceWork = employee.IsDistanceWork
                });
            }

            return statuses;
        }

        public GoTimeStampEmployeeStampStatus GetTimeStampAttendanceForEmployee(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            GoTimeStampEmployeeStampStatus status = null;

            TimeStampAttendanceGaugeDTO employee = DashboardManager.GetTimeStampAttendance(actorCompanyId, 0, 0, TermGroup_TimeStampAttendanceGaugeShowMode.AllLast24Hours, true, onlyIncludeAttestRoleEmployees: false, includeEmployeeNrInString: false, timeTerminalId: timeTerminalId, includeBreaks: true, employeeId: employeeId).FirstOrDefault();

            GoTimeStampEmployee emp = new GoTimeStampEmployee()
            {
                EmployeeId = employee?.EmployeeId ?? employeeId,
                EmployeeNr = employee?.EmployeeNr ?? string.Empty,
                Name = employee?.Name ?? string.Empty,
            };

            status = new GoTimeStampEmployeeStampStatus()
            {
                Employee = emp,
                StampedIn = employee == null ? false : employee.Type == TimeStampEntryType.In,
                TimeStamp = employee?.Time ?? DateTime.Now,
                OnBreak = employee?.IsBreak ?? false,
                OnPaidBreak = employee?.IsPaidBreak ?? false,
                OnDistanceWork = employee?.IsDistanceWork ?? false
            };

            return status;
        }

        #endregion

        #region Information

        public List<InformationDTO> GetUnreadInformation(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            User user = UserManager.GetUserByEmployeeId(entitiesReadOnly, employeeId, actorCompanyId);
            if (user != null)
                return GeneralManager.GetUnreadInformations(user.LicenseId, actorCompanyId, user.DefaultRoleId, user.UserId, false, false, true, UserManager.GetUserLangId(user.UserId)).OrderBy(i => i.DisplayDate).ToList();

            return new List<InformationDTO>();
        }

        public InformationDTO GetInformation(int actorCompanyId, int timeTerminalId, int employeeId, int informationId, SoeInformationSourceType sourceType)
        {
            if (sourceType == SoeInformationSourceType.Company)
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                int userId = UserManager.GetUserIdByEmployeeId(entitiesReadOnly, employeeId, actorCompanyId);
                return GeneralManager.GetCompanyInformation(informationId, actorCompanyId, userId, false);
            }
            else
            {
                return GeneralManager.GetSysInformation(informationId, false);
            }
        }

        public ActionResult SetInformationAsRead(int actorCompanyId, int timeTerminalId, int employeeId, int informationId, SoeInformationSourceType sourceType, bool confirmed)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            int userId = UserManager.GetUserIdByEmployeeId(entitiesReadOnly, employeeId, actorCompanyId);
            return GeneralManager.SetInformationAsRead(sourceType == SoeInformationSourceType.Company ? informationId : 0, sourceType == SoeInformationSourceType.Sys ? informationId : 0, userId, confirmed, false);
        }

        #endregion

        #region Schedule

        public List<GoTimeStampScheduleBlock> GetCurrentSchedule(int actorCompanyId, int employeeId, int timeTerminalId)
        {
            SetLanguageFromTerminal(timeTerminalId, actorCompanyId);
            return GetScheduleBlocksForDay(employeeId, DateTime.Today, actorCompanyId);
        }

        public List<GoTimeStampScheduleBlock> GetScheduleForEmployees(int actorCompanyId, int timeTerminalId)
        {
            GoTimeStampSyncEmployeeResult result = SyncEmployees(actorCompanyId, timeTerminalId);
            if (result?.Employees != null)
                return GetCurrentSchedule(actorCompanyId, DateTime.Today, result.Employees.Select(s => s.EmployeeId).ToList(), timeTerminalId);

            return new List<GoTimeStampScheduleBlock>();
        }

        public List<GoTimeStampScheduleBlock> GetCurrentSchedule(int actorCompanyId, DateTime date, List<int> employeeIds, int timeTerminalId)
        {
            SetLanguageFromTerminal(timeTerminalId, actorCompanyId);
            return GetScheduleBlocksForEmployeesAndDay(employeeIds, date, actorCompanyId);
        }

        public List<GoTimeStampScheduleBlock> GetNextSchedule(int actorCompanyId, int employeeId, int timeTerminalId)
        {
            SetLanguageFromTerminal(timeTerminalId, actorCompanyId);
            List<GoTimeStampScheduleBlock> blocks = new List<GoTimeStampScheduleBlock>();

            // Check for next working day, max 14 days from today
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime stopDate = startDate.AddDays(14);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            DateTime? firstDate = entitiesReadOnly.TimeScheduleTemplateBlock.Where(f => f.EmployeeId == employeeId && f.Date.HasValue && f.Date >= startDate && f.Date <= stopDate && f.State == (int)SoeEntityState.Active && (f.StartTime != f.StopTime || f.TimeDeviationCauseId.HasValue)).OrderBy(o => o.Date).FirstOrDefault()?.Date;

            startDate = firstDate.HasValue ? firstDate.Value : stopDate;

            while (!blocks.Any() && startDate < stopDate)
            {
                blocks = GetScheduleBlocksForDay(employeeId, startDate, actorCompanyId);
                if (blocks.Any())
                    break;
                else
                    startDate = startDate.AddDays(1);
            }

            return blocks;
        }

        private List<GoTimeStampScheduleBlock> GetScheduleBlocksForEmployeesAndDay(List<int> employeeIds, DateTime date, int actorCompanyId)
        {
            List<GoTimeStampScheduleBlock> result = new List<GoTimeStampScheduleBlock>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<TimeScheduleTemplateBlock> blocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entitiesReadOnly, employeeIds, date, date, false);
            List<TimeScheduleTemplateBlockDTO> dtos = blocks.Where(b => b.StartTime != b.StopTime || b.TimeDeviationCauseId.HasValue).ToDTOs();
            string absence = GetText(3150);
            string onDuty = GetText(12165);

            foreach (var group in dtos.GroupBy(g => $"{g.EmployeeId}#{g.Date}"))
            {
                result.AddRange(group.ToList().ToGoTimeStampDTOs(absence, onDuty, GetShiftTypesDictFromCache(actorCompanyId)).OrderBy(b => b.StartTime).ToList());
            }

            return result;
        }

        private List<GoTimeStampScheduleBlock> GetScheduleBlocksForDay(int employeeId, DateTime date, int actorCompanyId)
        {
            List<TimeScheduleTemplateBlock> blocks = TimeScheduleManager.GetTimeScheduleTemplateBlocksForDay(employeeId, date);
            List<TimeScheduleTemplateBlockDTO> dtos = blocks.Where(b => b.StartTime != b.StopTime || b.TimeDeviationCauseId.HasValue).ToDTOs();
            string absence = GetText(3150);
            string onDuty = GetText(12165);

            return dtos.ToGoTimeStampDTOs(absence, onDuty, GetShiftTypesDictFromCache(actorCompanyId)).OrderBy(b => b.StartTime).ToList();
        }

        private Dictionary<int, ShiftTypeDTO> GetShiftTypesDictFromCache(int actorCompanyId)
        {
            string key = "GTOSGetShiftTypesFromCache#" + actorCompanyId.ToString();
            Dictionary<int, ShiftTypeDTO> shiftTypeDict = BusinessMemoryCache<Dictionary<int, ShiftTypeDTO>>.Get(key);
            if (shiftTypeDict != null)
                return shiftTypeDict;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<ShiftTypeDTO> frombaseCache = GetShiftTypesFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId))?.ToDTOs().ToList();
            if (frombaseCache != null)
            {
                shiftTypeDict = frombaseCache.GroupBy(g => g.ShiftTypeId).ToDictionary(k => k.Key, v => v.First());
                BusinessMemoryCache<Dictionary<int, ShiftTypeDTO>>.Set(key, shiftTypeDict, 60 * 60);
            }

            return shiftTypeDict;
        }

        #endregion

        #region Terminal

        public GoTimeStampTerminal SyncTerminal(int actorCompanyId, int timeTerminalId, DateTime? prevSyncDate, bool? includeSettings = false)
        {
            GoTimeStampTerminal syncTerminal = null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeTerminal.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<TimeTerminal> query = entitiesReadOnly.TimeTerminal;
            if (includeSettings == true)
                query = query.Include("TimeTerminalSetting");

            TimeTerminal terminal = (from t in query
                                     where t.TimeTerminalId == timeTerminalId &&
                                     t.ActorCompanyId == actorCompanyId &&
                                     (!prevSyncDate.HasValue ||
                                     ((t.Modified.HasValue && t.Modified.Value > prevSyncDate.Value) ||
                                     (!t.Modified.HasValue && t.Created.HasValue && t.Created.Value > prevSyncDate.Value) || !t.Created.HasValue))
                                     select t).FirstOrDefault();

            if (terminal != null)
            {
                syncTerminal = new GoTimeStampTerminal()
                {
                    ActorCompanyId = terminal.ActorCompanyId,
                    TimeTerminalId = terminal.TimeTerminalId,
                    Name = terminal.Name,
                    Type = (TimeTerminalType)terminal.Type,
                    TimeTerminalGuid = terminal.TimeTerminalGuid,
                    LastSync = terminal.LastSync
                };

                if (includeSettings == true)
                {
                    List<TimeTerminalSettingType> validTypes = TimeStampManager.GetValidSettingTypes((TimeTerminalType)terminal.Type);
                    syncTerminal.Settings = terminal.TimeTerminalSetting.Where(s => s.State == (int)SoeEntityState.Active && validTypes.Contains((TimeTerminalSettingType)s.Type)).OrderBy(s => s.Name).ToGoTimeStampDTOs();

                    // Company settings
                    syncTerminal.Settings.Add(new GoTimeStampTerminalSetting()
                    {
                        Type = TimeTerminalSettingType.PossibilityToRegisterAdditionsInTerminal,
                        DataType = TimeTerminalSettingDataType.Boolean,
         
                        BoolData = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.PossibilityToRegisterAdditionsInTerminal, 0, actorCompanyId, 0)
                    });

                    // App settings
                    syncTerminal.Settings.Add(new GoTimeStampTerminalSetting()
                    {
                        Type = TimeTerminalSettingType.GtsHandshakeInterval,
                        DataType = TimeTerminalSettingDataType.Integer,
                        IntData = SettingManager.GetIntSetting(entities, SettingMainType.Application, (int)ApplicationSettingType.GtsHandshakeInterval, 0, 0, 0)
                    });
                }

                ActionResult result = TimeStampManager.SetTimeTerminalLastSync(timeTerminalId);
                if (result.Success)
                    syncTerminal.LastSync = result.DateTimeValue;

                if (syncTerminal.LastSync.HasValue)
                {
                    double offset = TimeStampManager.GetTimeZoneOffsetFromDefault(terminal.TimeTerminalId);
                    if (offset != 0)
                        syncTerminal.LastSync = syncTerminal.LastSync.Value.AddHours(offset);
                }
            }

            return syncTerminal;
        }

        public GoTimeStampTerminalSetting GetTerminalSetting(int actorCompanyId, int timeTerminalId, TimeTerminalSettingType type)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeTerminalSetting.NoTracking();
            TimeTerminalSetting setting = (from s in entities.TimeTerminalSetting
                                           where s.TimeTerminal.TimeTerminalId == timeTerminalId &&
                                           s.TimeTerminal.ActorCompanyId == actorCompanyId &&
                                           s.Type == (int)type &&
                                           s.State == (int)SoeEntityState.Active
                                           select s).FirstOrDefault();

            return setting != null ? setting.ToGoTimeStampDTO() : null;
        }

        public void SetLanguageFromTerminal(int timeTerminalId, int actorCompanyId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            SetLanguage(TimeStampManager.GetTimeTerminalDefaultLanguage(entitiesReadOnly, actorCompanyId, timeTerminalId));
        }

        #endregion

        #region TimeAccumulator

        public GoTimeStampAccumulator GetBreakAccumulator(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            EmployeeGroup group = EmployeeManager.GetEmployeeGroupForEmployee(employeeId, actorCompanyId, DateTime.Today);
            if (group == null)
                return null;

            DateTime time = DateTime.Now;
            TimeAccumulatorItem item = TimeAccumulatorManager.GetBreakTimeAccumulatorItem(actorCompanyId, time, employeeId, group.EmployeeGroupId);

            GoTimeStampAccumulator accumulator = new GoTimeStampAccumulator()
            {
                TimeAccumulatorId = item.TimeAccumulatorId,
                EmployeeId = employeeId,
                Name = item.Name,
                SumToday = item.SumToday,
                SumPeriod = item.SumPeriod,
                SumAccToday = item.SumAccToday,
                SumYear = item.SumYear,
                SyncDate = time
            };

            return accumulator;
        }

        public List<GoTimeStampAccumulator> GetOtherAccumulators(int actorCompanyId, int timeTerminalId, int employeeId)
        {
            DateTime time = DateTime.Now;
            List<GoTimeStampAccumulator> accumulators = new List<GoTimeStampAccumulator>();

            GetTimeAccumulatorItemsInput accInput = GetTimeAccumulatorItemsInput.CreateInput(actorCompanyId, 0, employeeId, time.Date, time.Date, calculateDay: true, calculatePeriod: true, calculateYear: true, calculateAccToday: true);
            List<TimeAccumulatorItem> items = TimeAccumulatorManager.GetTimeAccumulatorItems(accInput);
            foreach (TimeAccumulatorItem item in items)
            {
                GoTimeStampAccumulator accumulator = new GoTimeStampAccumulator()
                {
                    TimeAccumulatorId = item.TimeAccumulatorId,
                    EmployeeId = employeeId,
                    Name = item.Name,
                    SumToday = item.SumToday,
                    SumPeriod = item.SumPeriod,
                    SumAccToday = item.SumAccToday,
                    SumYear = item.SumYear,
                    SyncDate = time
                };
                accumulators.Add(accumulator);
            }

            return accumulators;
        }

        #endregion

        #region TimeStamp

        public List<GoTimeStampEmployeeStampStatus> SynchGTSTimeStamps(List<GoTimeStampTimeStamp> timeStampEntryItems, int timeTerminalId, int actorCompanyId)
        {
            return TimeEngineManager(actorCompanyId, 0).SynchGTSTimeStamps(timeStampEntryItems, timeTerminalId);
        }

        public List<TimeStampEntry> GetTimeStampHistory(int actorCompanyId, int timeTerminalId, int employeeId, int nbrOfEntries, bool loadExtended)
        {
            return TimeStampManager.GetLastTimeStampEntriesForEmployee(employeeId, nbrOfEntries, loadExtended);
        }

        #endregion

        #region TimeStampAddition

        public List<TimeStampAdditionDTO> GetTimeStampAdditions(int actorCompanyId, int timeTerminalId)
        {
            return TimeStampManager.GetTimeStampAdditions(actorCompanyId, false, timeTerminalId);
        }

        #endregion

        #region User

        public List<TimeTerminalDTO> GetTimeTerminalsForUser(int userId, int actorCompanyId, DateTime date)
        {
            List<TimeTerminalDTO> validTimeTerminals = new List<TimeTerminalDTO>();
            int employeeId = EmployeeManager.GetEmployeeIdForUser(userId, actorCompanyId);
            if (employeeId > 0)
            {
                bool useCache = false;
                List<TimeTerminal> allTimeTerminals = TimeStampManager.GetTimeTerminals(actorCompanyId, TimeTerminalType.GoTimeStamp, true, false, false);
                foreach (TimeTerminal timeTerminal in allTimeTerminals)
                {
                    if (TimeStampManager.IsEmployeeConnectedToTimeTerminal(actorCompanyId, timeTerminal.TimeTerminalId, employeeId, date, useCache))
                        validTimeTerminals.Add(timeTerminal.ToDTO(true, false, false));
                    useCache = true;
                }

                validTimeTerminals.ForEach(t => t.SetUri());
            }

            return validTimeTerminals;
        }

        #endregion
    }

    #region Models

    public class GoTimeStampAccount
    {
        // Corresponds to the Account class in the GoTimeStamp project

        public int Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
    }

    public class GoTimeStampAccumulator
    {
        // Corresponds to the Accumulator class in the GoTimeStamp project

        public int TimeAccumulatorId { get; set; }
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public decimal SumToday { get; set; }
        public decimal SumPeriod { get; set; }
        public decimal SumAccToday { get; set; }
        public decimal SumYear { get; set; }
        public DateTime SyncDate { get; set; }
    }

    public class GoTimeStampActionResult
    {
        // Corresponds to the ActionResult class in the GoTimeStamp project

        public bool Success { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; }

        public int IntegerValue { get; set; }
        public string StringValue { get; set; }
        public bool BooleanValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
    }

    public class GoTimeStampDeviationCause
    {
        // Corresponds to the DeviationCause class in the GoTimeStamp project

        public int Id { get; set; }
        public string Name { get; set; }
        public TermGroup_TimeDeviationCauseType Type { get; set; }
    }

    public class GoTimeStampInformation
    {
        // Corresponds to the Information class in the GoTimeStamp project

        public int InformationId { get; set; }
        public SoeInformationSourceType SourceType { get; set; }
        public TermGroup_InformationSeverity Severity { get; set; }
        public DateTime? DisplayDate { get; set; }
        public string Subject { get; set; }
        public string ShortText { get; set; }
        public string Text { get; set; }
        public bool NeedsConfirmation { get; set; }
    }

    public class GoTimeStampSyncEmployeeResult
    {
        // Corresponds to the SyncEmployeesResult class in the GoTimeStamp project

        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<GoTimeStampEmployee> Employees { get; set; }
    }

    public class GoTimeStampEmployee
    {
        // Corresponds to the Employee class in the GoTimeStamp project

        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CardNumber { get; set; }
        public int EmployeeGroupId { get; set; }
        public List<int> DeviationCauseIds { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class GoTimeStampEmployeeStampStatus
    {
        // Corresponds to the EmployeeStampStatus class in the GoTimeStamp project

        public GoTimeStampEmployee Employee { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool StampedIn { get; set; }
        public bool OnBreak { get; set; }
        public bool OnPaidBreak { get; set; }
        public bool OnDistanceWork { get; set; }
    }

    public class GoTimeStampLinkEmployeeToCardNumber
    {
        // Corresponds to the LinkEmployeeToCardNumber class in the GoTimeStamp project

        public string EmployeeNr { get; set; }
        public string CardNumber { get; set; }
    }

    public class GoTimeStampScheduleBlock
    {
        // Corresponds to the ScheduleBlock class in the GoTimeStamp project

        public TermGroup_TimeScheduleTemplateBlockType Type { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeColor { get; set; }
        public string ShiftTypeTextColor { get; set; }
        public bool IsBreak { get; set; }
        public bool IsAbsence { get; set; }
    }

    public class GoTimeStampSetInformationAsRead
    {
        // Corresponds to the SetInformationAsRead class in the GoTimeStamp project

        public int EmployeeId { get; set; }
        public int InformationId { get; set; }
        public SoeInformationSourceType SourceType { get; set; }
        public bool Confirmed { get; set; }
    }

    public class GoTimeStampTerminal
    {
        // Corresponds to the Terminal class in the GoTimeStamp project

        public int ActorCompanyId { get; set; }
        public int TimeTerminalId { get; set; }
        public Guid TimeTerminalGuid { get; set; }
        public string Name { get; set; }
        public TimeTerminalType Type { get; set; }
        public List<GoTimeStampTerminalSetting> Settings { get; set; }
        public DateTime? LastSync { get; set; }
    }

    public class GoTimeStampTerminalSetting
    {
        // Corresponds to the TerminalSetting class in the GoTimeStamp project

        public TimeTerminalSettingType Type { get; set; }
        public TimeTerminalSettingDataType DataType { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public decimal? DecimalData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public DateTime? TimeData { get; set; }
        public bool IsParent { get; set; }
    }

    public class GoTimeStampTimeStamp
    {
        // Corresponds to the EmployeeTimeStamp class in the GoTimeStamp project

        public int EmployeeId { get; set; }
        public int? DeviationCauseId { get; set; }
        public int? AccountId { get; set; }
        public DateTime TimeStamp { get; set; }
        public TimeStampEntryType EntryType { get; set; }
        public TermGroup_TimeStampEntryOriginType OriginType { get; set; }
        public bool IsBreak { get; set; }
        public bool IsPaidBreak { get; set; }
        public bool IsDistanceWork { get; set; }
        public bool InvalidIPAddress { get; set; }
        public string Data { get; set; }

        // Relations
        public List<GoTimeStampTimeStampEntryExtended> Extended { get; set; }

        // Extensions
        public string DeviationCauseName { get; set; }
        public string AccountName { get; set; }
        public string TerminalName { get; set; }

        public override string ToString()
        {
            return $"{EmployeeId}#{DeviationCauseId}#{AccountId}#{TimeStamp}#{EntryType}#{Data}#{IsBreak}";
        }

        public void SetExtendedData()
        {
            if (Data == null)
                Data = string.Empty;

            Data = $"A{AccountId}#D{DeviationCauseId}#S{TimeStamp}#T{EntryType}#{Data}#{IsBreak}";
        }
    }

    public class GoTimeStampTimeStampEntryExtended
    {
        // Corresponds to the EmployeeTimeStampExtended class in the GoTimeStamp project

        public int? TimeScheduleTypeId { get; set; }
        public int? TimeCodeId { get; set; }
        public int? AccountId { get; set; }
        public decimal? Quantity { get; set; }

        // Extensions
        public string TimeScheduleTypeName { get; set; }
        public string TimeCodeName { get; set; }
        public string AccountName { get; set; }
    }

    public class GoTimeStampTimeStampAddition
    {
        // Corresponds to the TimeStampAddition class in the GoTimeStamp project

        public int Id { get; set; }
        public string Name { get; set; }
        public TimeStampAdditionType Type { get; set; }
        public decimal? FixedQuantity { get; set; }
    }

    #endregion

    #region Model Extensions

    public static class GoTimeStampExtensions
    {
        #region Account

        public static GoTimeStampAccount ToGoTimeStampDTO(this Account e)
        {
            if (e == null)
                return null;

            GoTimeStampAccount dto = new GoTimeStampAccount()
            {
                Id = e.AccountId,
                Number = e.AccountNr,
                Name = e.Name,
                ParentId = e.ParentAccountId
            };

            return dto;
        }

        public static List<GoTimeStampAccount> ToGoTimeStampDTOs(this IEnumerable<Account> l)
        {
            List<GoTimeStampAccount> dtos = new List<GoTimeStampAccount>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region ActionResult

        public static GoTimeStampActionResult ToGoTimeStampDTO(this ActionResult e)
        {
            if (e == null)
                return null;

            GoTimeStampActionResult dto = new GoTimeStampActionResult()
            {
                Success = e.Success,
                ErrorNumber = e.ErrorNumber,
                ErrorMessage = e.ErrorMessage,
                IntegerValue = e.IntegerValue,
                StringValue = e.StringValue,
                BooleanValue = e.BooleanValue,
                DateTimeValue = (e.DateTimeValue == DateTime.MinValue ? (DateTime?)null : e.DateTimeValue)
            };

            return dto;
        }

        #endregion

        #region DeviationCause

        public static GoTimeStampDeviationCause ToGoTimeStampDTO(this TimeDeviationCause e)
        {
            if (e == null)
                return null;

            GoTimeStampDeviationCause dto = new GoTimeStampDeviationCause()
            {
                Id = e.TimeDeviationCauseId,
                Name = e.Name,
                Type = (TermGroup_TimeDeviationCauseType)e.Type
            };

            return dto;
        }

        public static List<GoTimeStampDeviationCause> ToGoTimeStampDTOs(this IEnumerable<TimeDeviationCause> l)
        {
            List<GoTimeStampDeviationCause> dtos = new List<GoTimeStampDeviationCause>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region Employee

        public static GoTimeStampEmployee ToGoTimeStampDTO(this Employee e)
        {
            if (e == null)
                return null;

            GoTimeStampEmployee dto = new GoTimeStampEmployee()
            {
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                Name = e.Name,
                FirstName = e.FirstName,
                LastName = e.LastName,
                CardNumber = e.CardNumber,
                State = (SoeEntityState)e.State
            };

            return dto;
        }

        public static List<GoTimeStampEmployee> ToGoTimeStampDTOs(this IEnumerable<Employee> l)
        {
            List<GoTimeStampEmployee> dtos = new List<GoTimeStampEmployee>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region Information

        public static GoTimeStampInformation ToGoTimeStampDTO(this InformationDTO e)
        {
            if (e == null)
                return null;

            GoTimeStampInformation dto = new GoTimeStampInformation()
            {
                InformationId = e.InformationId,
                SourceType = e.SourceType,
                Severity = e.Severity,
                DisplayDate = e.ValidFrom.HasValue ? e.ValidFrom : e.Created,
                Subject = e.Subject,
                ShortText = e.ShortText,
                Text = e.Text,
                NeedsConfirmation = e.NeedsConfirmation
            };

            return dto;
        }

        public static List<GoTimeStampInformation> ToGoTimeStampDTOs(this IEnumerable<InformationDTO> l)
        {
            List<GoTimeStampInformation> dtos = new List<GoTimeStampInformation>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region Schedule

        public static List<GoTimeStampScheduleBlock> ToGoTimeStampDTOs(this List<TimeScheduleTemplateBlockDTO> l, string absence, string onDuty, Dictionary<int, ShiftTypeDTO> shiftTypes)
        {
            List<GoTimeStampScheduleBlock> dtos = new List<GoTimeStampScheduleBlock>();
            foreach (var dto in l)
            {
                ShiftTypeDTO shiftType = shiftTypes.FirstOrDefault(f => f.Key == dto.ShiftTypeId).Value;
                if (dto.IsBreak && shiftType == null)
                {
                    int? overlappingShiftTypeId = l.Where(w => w.Type != TermGroup_TimeScheduleTemplateBlockType.OnDuty).FirstOrDefault(w => CalendarUtility.GetOverlappingMinutes(w.StartTime, w.StopTime, dto.StartTime, dto.StopTime) > 0)?.ShiftTypeId;
                    if (overlappingShiftTypeId.HasValue)
                    {
                        ShiftTypeDTO overlappingShiftType = shiftTypes.FirstOrDefault(w => w.Key == overlappingShiftTypeId).Value;
                        if (overlappingShiftType != null)
                            shiftType = overlappingShiftType;
                    }
                }

                dtos.Add(dto.ToGoTimeStampDTO(absence, onDuty, shiftType));
            }

            return dtos;
        }

        public static GoTimeStampScheduleBlock ToGoTimeStampDTO(this TimeScheduleTemplateBlockDTO e, string absence, string onDuty, ShiftTypeDTO shiftTypeDTO = null)
        {
            GoTimeStampScheduleBlock dto = new GoTimeStampScheduleBlock()
            {
                Type = e.Type,
                EmployeeId = e.EmployeeId.Value,
                Date = e.Date.Value,
                StartTime = e.ActualStartTime.Value,
                StopTime = e.ActualStopTime.Value,
                ShiftTypeName = "",
                ShiftTypeColor = Constants.SHIFT_TYPE_DEFAULT_COLOR,
                IsBreak = e.IsBreak
            };

            if (shiftTypeDTO != null)
            {
                dto.ShiftTypeName = shiftTypeDTO.Name;
                dto.ShiftTypeColor = GraphicsUtil.RemoveAlphaValue(shiftTypeDTO.Color);
            }

            if (e.TimeDeviationCauseId.HasValue)
            {
                dto.ShiftTypeColor = "#8B0000";
                dto.ShiftTypeName = absence;
                dto.IsAbsence = true;
            }
            else if (e.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty)
            {
                dto.ShiftTypeName += $" ({onDuty.ToLowerInvariant()})";
            }

            dto.ShiftTypeTextColor = GraphicsUtil.ForegroundColorByBackgroundBrightness(dto.ShiftTypeColor);

            return dto;
        }

        #endregion

        #region Terminal

        public static GoTimeStampTerminalSetting ToGoTimeStampDTO(this TimeTerminalSetting e)
        {
            if (e == null)
                return null;

            GoTimeStampTerminalSetting dto = new GoTimeStampTerminalSetting()
            {
                Type = (TimeTerminalSettingType)e.Type,
                DataType = (TimeTerminalSettingDataType)e.DataType,
                StrData = e.StrData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                BoolData = e.BoolData,
                DateData = e.DateData,
                TimeData = e.TimeData,
                IsParent = !e.ParentId.HasValue
            };

            return dto;
        }

        public static List<GoTimeStampTerminalSetting> ToGoTimeStampDTOs(this IEnumerable<TimeTerminalSetting> l)
        {
            List<GoTimeStampTerminalSetting> dtos = new List<GoTimeStampTerminalSetting>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region TimeStamp

        public static GoTimeStampTimeStamp ToGoTimeStampDTO(this TimeStampEntry e)
        {
            if (e == null)
                return null;

            GoTimeStampTimeStamp dto = new GoTimeStampTimeStamp()
            {
                EmployeeId = e.EmployeeId,
                DeviationCauseId = e.TimeDeviationCauseId,
                AccountId = e.AccountId,
                TimeStamp = e.Time,
                EntryType = (TimeStampEntryType)e.Type,
                OriginType = (TermGroup_TimeStampEntryOriginType)e.OriginType,
                IsBreak = e.IsBreak,
                IsPaidBreak = e.IsPaidBreak,
                IsDistanceWork = e.IsDistanceWork
            };

            // Relations
            if (e.TimeStampEntryExtended.IsLoaded)
                dto.Extended = e.TimeStampEntryExtended.Where(t => t.State == (int)SoeEntityState.Active).ToGoTimeStampDTOs();

            // Extensions
            dto.DeviationCauseName = e.DeviationCauseName;
            dto.AccountName = e.AccountName;
            dto.TerminalName = e.TerminalName;

            return dto;
        }

        public static List<GoTimeStampTimeStamp> ToGoTimeStampDTOs(this IEnumerable<TimeStampEntry> l)
        {
            List<GoTimeStampTimeStamp> dtos = new List<GoTimeStampTimeStamp>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        public static GoTimeStampTimeStampEntryExtended ToGoTimeStampDTO(this TimeStampEntryExtended e)
        {
            if (e == null)
                return null;

            GoTimeStampTimeStampEntryExtended dto = new GoTimeStampTimeStampEntryExtended()
            {
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                TimeCodeId = e.TimeCodeId,
                AccountId = e.AccountId,
                Quantity = e.Quantity
            };

            if (e.TimeScheduleTypeReference.IsLoaded && e.TimeScheduleType != null)
                dto.TimeScheduleTypeName = e.TimeScheduleType.Name;

            if (e.TimeCodeReference.IsLoaded && e.TimeCode != null)
                dto.TimeCodeName = e.TimeCode.Name;

            if (e.AccountReference.IsLoaded && e.Account != null)
                dto.AccountName = e.Account.Name;

            return dto;
        }

        public static List<GoTimeStampTimeStampEntryExtended> ToGoTimeStampDTOs(this IEnumerable<TimeStampEntryExtended> l)
        {
            List<GoTimeStampTimeStampEntryExtended> dtos = new List<GoTimeStampTimeStampEntryExtended>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region TimeStampAddition

        public static GoTimeStampTimeStampAddition ToGoTimeStampDTO(this TimeStampAdditionDTO e)
        {
            if (e == null)
                return null;

            GoTimeStampTimeStampAddition dto = new GoTimeStampTimeStampAddition()
            {
                Id = e.Id,
                Name = e.Name,
                Type = e.Type,
                FixedQuantity = e.FixedQuantity
            };

            return dto;
        }

        public static List<GoTimeStampTimeStampAddition> ToGoTimeStampDTOs(this IEnumerable<TimeStampAdditionDTO> l)
        {
            List<GoTimeStampTimeStampAddition> dtos = new List<GoTimeStampTimeStampAddition>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGoTimeStampDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region PubSub

        public const string employeePrefix = "EM";
        public const string employeeGroupPrefix = "EG";
        public const string timeCodePrefix = "TC";
        public const string timeDeviationCausePrefix = "DC";
        public const string timeScheduleTypePrefix = "ST";
        public const string timeStampEntryPrefix = "TS";
        public const string timeTerminalPrefix = "TT";

        public static string GetEmployeeUpdateMessage(int actorCompanyId, int employeeId, WebPubSubMessageAction action)
        {
            if (employeeId == 0)
                return string.Empty;

            return $"{employeePrefix}#{actorCompanyId}#{employeeId}#{WebPubSubUtil.GetMessageActionKey(action)}";
        }

        public static string GetUpdateMessage(this EmployeeGroup employeeGroup, WebPubSubMessageAction action)
        {
            if (employeeGroup == null)
                return string.Empty;

            return $"{employeeGroupPrefix}#{employeeGroup.ActorCompanyId}#{employeeGroup.EmployeeGroupId}#{WebPubSubUtil.GetMessageActionKey(action)}";
        }

        public static string GetUpdateMessage(this TimeCode timeCode, WebPubSubMessageAction action)
        {
            if (timeCode == null)
                return string.Empty;

            return $"{timeCodePrefix}#{timeCode.ActorCompanyId}#{timeCode.TimeCodeId}#{WebPubSubUtil.GetMessageActionKey(action)}";
        }

        public static string GetUpdateMessage(this TimeDeviationCause timeDeviationCause, WebPubSubMessageAction action)
        {
            if (timeDeviationCause == null)
                return string.Empty;

            return $"{timeDeviationCausePrefix}#{timeDeviationCause.ActorCompanyId}#{timeDeviationCause.TimeDeviationCauseId}#{WebPubSubUtil.GetMessageActionKey(action)}";
        }

        public static string GetUpdateMessage(this TimeScheduleType timeScheduleType, WebPubSubMessageAction action)
        {
            if (timeScheduleType == null)
                return string.Empty;

            return $"{timeScheduleTypePrefix}#{timeScheduleType.ActorCompanyId}#{timeScheduleType.TimeScheduleTypeId}#{WebPubSubUtil.GetMessageActionKey(action)}";
        }

        public static string GetUpdateMessage(this TimeStampEntry timeStampEntry, WebPubSubMessageAction action)
        {
            if (timeStampEntry == null)
                return string.Empty;

            return $"{timeStampEntryPrefix}#{timeStampEntry.ActorCompanyId}#{timeStampEntry.TimeTerminalId ?? 0}#{timeStampEntry.EmployeeId}#{timeStampEntry.Type}#{timeStampEntry.IsBreak}#{timeStampEntry.Time.ToString("yyyy-MM-ddTHH:mm:ss")}#{WebPubSubUtil.GetMessageActionKey(action)}";
        }

        public static string GetUpdateMessage(this TimeTerminal timeTerminal)
        {
            if (timeTerminal == null)
                return string.Empty;

            return $"{timeTerminalPrefix}#{timeTerminal.ActorCompanyId}#{timeTerminal.TimeTerminalId}";
        }

        public static string GetTerminalPubSubKey(this TimeTerminal timeTerminal)
        {
            return GetTerminalPubSubKey(timeTerminal.ActorCompanyId, timeTerminal.TimeTerminalId);
        }

        public static string GetTerminalPubSubKey(int actorCompanyId, int timeTerminalId)
        {
            return $"PS#{actorCompanyId}#{timeTerminalId}";
        }


        #endregion
    }

    #endregion
}
