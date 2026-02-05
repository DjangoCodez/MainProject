using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.TimeTree
{
    public class TimeTreeAttestManager : TimeTreeBaseManager
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeTreeAttestManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Tree

        public List<Employee> GetAttestTreeEmployees(CompEntities entities, DateTime startDate, DateTime stopDate, int? timePeriodId, List<int> employeeFilter = null, TimeEmployeeTreeSettings settings = null)
        {
            return GetAttestTreeEmployees(entities, out _, startDate, stopDate, timePeriodId, employeeFilter, settings);
        }

        public List<Employee> GetAttestTreeEmployees(CompEntities entities, out EmployeeAuthModelRepository repository, DateTime startDate, DateTime stopDate, int? timePeriodId, List<int> employeeFilter = null, TimeEmployeeTreeSettings settings = null)
        {
            settings = TimeEmployeeTreeSettings.Init(settings);
            if (settings.LoadMode == SoeAttestTreeLoadMode.Full)
                settings.LoadMode = SoeAttestTreeLoadMode.OnlyEmployees;

            GetAttestTree(entities, out repository, out List<Employee> employeesInTree, TermGroup_AttestTreeGrouping.All, TermGroup_AttestTreeSorting.EmployeeNr, startDate, stopDate, timePeriodId, settings);

            if (employeeFilter != null)
                employeesInTree = employeesInTree.Where(i => employeeFilter.Contains(i.EmployeeId)).ToList();

            return employeesInTree;
        }

        public TimeEmployeeTreeDTO GetAttestTree(TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting, DateTime startDate, DateTime stopDate, int? timePeriodId, TimeEmployeeTreeSettings settings = null)
        {
            using (CompEntities entities = new CompEntities())
            {
                entities.Employee.NoTracking();
                entities.TimePayrollTransaction.NoTracking();
                return GetAttestTree(entities, out _, out _, grouping, sorting, startDate, stopDate, timePeriodId, settings);
            }
        }

        public TimeEmployeeTreeDTO GetAttestTree(CompEntities entities, out EmployeeAuthModelRepository repository, out List<Employee> employeesInTree, TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting, DateTime startDate, DateTime stopDate, int? timePeriodId, TimeEmployeeTreeSettings settings = null)
        {
            TimeEmployeeTreeDTO tree;

            try
            {
                #region Init

                settings = TimeEmployeeTreeSettings.Init(settings);
                employeesInTree = null;

                if (string.IsNullOrEmpty(settings.CacheKeyToUse) || !Guid.TryParse(settings.CacheKeyToUse, out Guid cacheKey))
                    cacheKey = Guid.NewGuid();

                #endregion

                #region Prereq

                var timePeriod = timePeriodId.HasValue ? TimePeriodManager.GetTimePeriod(entities, timePeriodId.Value, base.ActorCompanyId, loadTimePeriodHead: true) : null;
                if (timePeriod != null)
                    tree = new TimeEmployeeTreeDTO(cacheKey, base.ActorCompanyId, timePeriod.ToDTO(), SoeAttestTreeMode.TimeAttest, grouping, sorting, settings);
                else
                    tree = new TimeEmployeeTreeDTO(cacheKey, base.ActorCompanyId, startDate, stopDate, SoeAttestTreeMode.TimeAttest, grouping, sorting, settings);

                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);
                if (!useAccountHierarchy)
                {
                    settings.DoNotShowDaysOutsideEmployeeAccount = false;
                    settings.IncludeAdditionalEmployees = false;
                }
                int currentAccountId = AccountManager.GetAccountHierarchySettingAccountId(entities, useAccountHierarchy);

                var user = UserManager.GetUser(entities, base.UserId);
                var accountInternalsCompany = settings.DoNotShowDaysOutsideEmployeeAccount ? base.GetAccountInternalsFromCache(entities, CacheConfig.Company(base.ActorCompanyId)) : null;
                var employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                var payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                var attestStates = GetAttestStatesForTimeFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                var highestAttestState = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.SalaryExportPayrollMinimumAttestStatus, tree.Settings.DoNotShowAttested);
                var messageGroup = settings.HasFilterOnMessageGroupId ? CommunicationManager.GetMessageGroup(settings.FilterMessageGroupId.Value, true) : null;

                if (settings.DoShowOnlyShiftSwaps)
                {
                    var scheduleSwapShiftRequestRows = TimeScheduleManager.GetTimeScheduleSwapRequestRows(entities, base.ActorCompanyId, settings.FilterEmployeeIds, tree.StartDate, tree.StopDate);
                    settings.FilterEmployeeIds = scheduleSwapShiftRequestRows.Where(e => e.IsApproved).Select(e => e.EmployeeId).Distinct().ToList(); //TODO: Only shorter or longer
                }

                var employees = GetEmployeesWithRepositoryFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey, keepAlive: true),
                    out repository,
                    base.UserId,
                    base.RoleId,
                    tree.StartDate,
                    tree.StopDate,
                    settings.FilterEmployeeIds,
                    includeEnded: settings.IncludeEnded,
                    onlyDefaultEmployeeAuthModel: true,
                    useDefaultEmployeeAccountDimEmployee: true,
                    searchPattern: settings.SearchPattern);

                var employeeIds = employees.GetIds();
                var accountRepository = repository as AccountRepository;
                var templateBlocksCompany = settings.DoNotShowDaysOutsideEmployeeAccount ? TimeScheduleManager.GetTimeScheduleTemplateBlocksForEmployees(entities, employeeIds, tree.StartDate, tree.StopDate) : null;

                //Transactions (only add to cache so warnings can use it, never reuse between reloads)
                var transactionsCompany = settings.DoLoadTransactions ? GetTimePayrollTransactionsForTreeFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey), tree.StartDate, tree.StopDate, timePeriod, employeeIds: employeeIds, includeAccounting: settings.DoNotShowDaysOutsideEmployeeAccount, flushCache: true) : null;
                var transactionsByEmployee = transactionsCompany?.GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList()) ?? new Dictionary<int, List<TimePayrollTransactionTreeDTO>>();

                #endregion

                #region Validate Employees

                List<Employee> validEmployees = new List<Employee>();
                List<Employee> endedEmployees = new List<Employee>();

                foreach (Employee employee in employees)
                {
                    List<TimePayrollTransactionTreeDTO> transactions = null;
                    if (settings.DoLoadTransactions)
                    {
                        transactions = transactionsByEmployee.GetList(employee.EmployeeId);
                        transactions = TimeTransactionManager.FilterTimePayrollTransactions(transactions, accountRepository, employee, tree.StartDate, tree.StopDate);
                        if (settings.DoNotShowDaysOutsideEmployeeAccount)
                        {
                            List<DateTime> validDates = AccountManager.GetValidDatesOnGivenAccounts(employee.EmployeeId, tree.StartDate, tree.StopDate, currentAccountId, accountInternalsCompany, templateBlocksCompany, transactions).Keys.ToList();
                            transactions = transactions.Where(transaction => validDates.Any(date => date == transaction.Date)).ToList();
                        }
                    }

                    if (!TrySetTimeTreeEmployeeProperties(tree, employee, tree.StartDate, tree.StopDate, timePeriod, attestStates, transactions, out bool employeeIsEnded, true, employeeGroups, payrollGroups, highestAttestState))
                        continue;
                    if (messageGroup != null && !CommunicationManager.IsUserInMessageGroup(messageGroup, base.ActorCompanyId, employee.UserId, user, base.RoleId, employee, tree.StartDate, tree.StopDate, employeeAccounts: accountRepository?.EmployeeAccounts.GetList(employee.EmployeeId)))
                        continue;

                    if (employeeIsEnded)
                        endedEmployees.Add(employee);
                    else
                        validEmployees.Add(employee);
                }

                #endregion

                #region Generate tree

                EmployeeAuthModel employeeAuthModel = EmployeeAuthModel.Create(repository, settings.FilterEmployeeAuthModelIds);
                GenerateTimeEmployeeTreeInput input = new GenerateTimeEmployeeTreeInput(tree.StartDate, tree.StopDate, timePeriod, validEmployees, endedEmployees, employeeAuthModel, employeeGroups, payrollGroups, attestStates, highestAttestState, transactionsCompany);
                GenerateTimeEmployeeTree(entities, ref tree, input);

                #endregion

                #region Include Employees

                if (settings.LoadMode == SoeAttestTreeLoadMode.OnlyEmployees || settings.LoadMode == SoeAttestTreeLoadMode.OnlyEmployeesAndIsAttested)
                {
                    employeeIds = tree.GetEmployeeIdsFromNodes();
                    employeesInTree = employees.Where(e => employeeIds.Contains(e.EmployeeId)).ToList();
                    employeeIds = employees.Select(i => i.EmployeeId).ToList();

                    if (settings.IncludeAdditionalEmployees)
                    {
                        Dictionary<int, List<int>> additionalEmployeeIds = tree.GetAdditionalEmployeeIds(excludeEmployeeIds: employeeIds);
                        if (additionalEmployeeIds.Any())
                        {
                            List<Employee> additionalEmployees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, additionalEmployeeIds.Keys.ToList(), loadEmployment: true);
                            if (!additionalEmployees.IsNullOrEmpty())
                            {
                                additionalEmployees.ForEach(e => e.AdditionalOnAccountIds = additionalEmployeeIds.GetList(e.EmployeeId, nullIfNotFound: true));
                                employeesInTree.AddRange(additionalEmployees);
                            }
                        }
                    }

                    if (settings.LoadMode == SoeAttestTreeLoadMode.OnlyEmployeesAndIsAttested)
                    {
                        //Only supported from mobile
                        AttestState resultingAttestState = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.MobileTimeAttestResultingAttestStatus);
                        if (resultingAttestState != null)
                        {
                            List<TimeEmployeeTreeNodeDTO> employeeNodes = tree.GetEmployeeNodes();
                            foreach (Employee employee in employeesInTree)
                            {
                                TimeEmployeeTreeNodeDTO employeeNode = employeeNodes.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId);
                                if (employeeNode?.AttestStates != null)
                                    employee.IsAttested = !employeeNode.AttestStates.Any(i => i.Sort <= resultingAttestState.Sort);
                            }
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                throw;
            }

            return tree;
        }

        public TimeEmployeeTreeDTO RefreshAttestTree(TimeEmployeeTreeDTO tree, DateTime startDate, DateTime stopDate, int? timePeriodId = null, TimeEmployeeTreeSettings settings = null)
        {
            if (tree == null)
                return tree;

            try
            {
                tree.SetSettings(settings);
                Guid cacheKey = tree.GetCacheKey(flush: true);

                using (CompEntities entities = new CompEntities())
                {
                    if (!tree.GroupNodes.IsNullOrEmpty())
                    {
                        List<AttestState> attestStates = AttestManager.GetAttestStates(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                        AttestState highestAttestState = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.SalaryExportPayrollMinimumAttestStatus, tree.Settings.DoNotShowAttested);
                        TimePeriod timePeriod = timePeriodId.HasValue ? TimePeriodManager.GetTimePeriod(entities, timePeriodId.Value, base.ActorCompanyId, loadTimePeriodHead: true) : null;
                        List<TimePayrollTransactionTreeDTO> transactions = GetTimePayrollTransactionsForTreeFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey), startDate, stopDate, timePeriod, employeeIds: tree.Settings?.FilterEmployeeIds);

                        foreach (TimeEmployeeTreeGroupNodeDTO groupNode in tree.GroupNodes.ToList())
                        {
                            RefreshGroupNode(entities, tree, groupNode, SoeAttestTreeMode.TimeAttest, transactions, attestStates, highestAttestState);
                        }
                    }

                    tree.Sort();
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                throw;
            }

            return tree;
        }

        public TimeEmployeeTreeDTO GetAttestTreeWarnings(TimeEmployeeTreeDTO tree, DateTime startDate, DateTime stopDate, List<int> employeeIds, int? timePeriodId = null, bool doShowOnlyWithWarnings = false, bool flushCache = false)
        {
            if (tree == null || tree.GroupNodes.IsNullOrEmpty())
                return tree;

            try
            {
                Guid cacheKey = tree.GetCacheKey(flushCache);

                tree.Settings = TimeEmployeeTreeSettings.Init();
                tree.Settings.WarningFilter = doShowOnlyWithWarnings ? TermGroup_TimeTreeWarningFilter.Time : TermGroup_TimeTreeWarningFilter.None;

                using (CompEntities entities = new CompEntities())
                {
                    #region Load company data

                    TimePeriod timePeriod = timePeriodId.HasValue ? TimePeriodManager.GetTimePeriod(entities, timePeriodId.Value, base.ActorCompanyId, loadTimePeriodHead: true) : null;
                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey));
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);

                    employeeIds = employeeIds ?? tree.GetEmployeeIdsFromNodes();

                    #endregion

                    #region Invoke 1

                    Dictionary<int, List<TimePayrollTransactionTreeDTO>> transactionsByEmployee = null;
                    AccountRepository accountRepository = null;
                    Dictionary<int, List<EmployeeAccount>> employeeAccounts = null;

                    Parallel.Invoke(GetDefaultParallelOptions(),
                    () =>
                    {
                        #region TimePayrollTransactions
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.TimePayrollTransaction.NoTracking();

                            List<TimePayrollTransactionTreeDTO> transactions = GetTimePayrollTransactionsForTreeFromCache(entitiesParallel, CacheConfig.Company(base.ActorCompanyId, cacheKey), startDate, stopDate, timePeriod, employeeIds);
                            if (tree.Mode == SoeAttestTreeMode.TimeAttest)
                                transactions = transactions.Where(t => !t.IsExcludedInTime()).ToList();
                            else if (tree.Mode == SoeAttestTreeMode.PayrollCalculation)
                                transactions = transactions.Where(t => !t.IsExcludedInPayroll()).ToList();
                            transactionsByEmployee = transactions?.GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList()) ?? new Dictionary<int, List<TimePayrollTransactionTreeDTO>>();
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region AccountHiearachy
                        if (useAccountHierarchy)
                        {
                            using (CompEntities entitiesParallel = new CompEntities())
                            {
                                entitiesParallel.Account.NoTracking();
                                entitiesParallel.EmployeeAccount.NoTracking();

                                accountRepository = AccountManager.GetAccountHierarchyRepositoryByUserSetting(entitiesParallel, base.ActorCompanyId, base.RoleId, base.UserId, startDate, stopDate, input: AccountHierarchyInput.GetInstance(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee));
                                employeeAccounts = accountRepository != null ? GetEmployeeAccountsFromCache(entitiesParallel, CacheConfig.Company(base.ActorCompanyId), employeeIds).ToDict() : null;
                            }
                        }
                        #endregion
                    }
                    );

                    #endregion

                    #region Invoke 2 (depending on first invoke)

                    Dictionary<int, List<DateTime>> employeesAndDatesWithStampingErrors = null;
                    Dictionary<int, List<DateTime>> employeesAndDatesWithPayrollImportError = null;
                    Dictionary<int, List<DateTime>> employeesAndDatesWithTimeStampsWithoutTransactions = null;
                    Dictionary<int, List<DateTime>> employeesAndDatesWithScheduleWithoutTransactions = null;
                    List<int> employeesIdsWithScheduledPlacements = null;

                    Parallel.Invoke(GetDefaultParallelOptions(), () =>
                    {
                        #region Schedule without transactions
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.TimeScheduleTemplateBlock.NoTracking();

                            if (transactionsByEmployee != null)
                                employeesAndDatesWithScheduleWithoutTransactions = TimeTransactionManager.GetEmployeesAndDatesWithScheduleWithoutTransactions(entitiesParallel, startDate, stopDate > DateTime.Today ? CalendarUtility.GetEndOfDay(DateTime.Now.Date) : stopDate, transactionsByEmployee, employeeIds);
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region Schedule placements
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.RecalculateTimeRecord.AsNoTracking();

                            employeesIdsWithScheduledPlacements = TimeScheduleManager.GetEmployeeIdsWithScheduledPlacements(base.ActorCompanyId, employeeIds, tree.StartDate, tree.StopDate);
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region TimeStamps with invalid status
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.Employee.AsNoTracking();
                            entitiesParallel.TimeBlockDate.AsNoTracking();

                            employeesAndDatesWithStampingErrors = TimeBlockManager.GetTimeBlockDatesWithStampingErrors(entitiesParallel, base.ActorCompanyId, employeeIds, startDate, stopDate);
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region TimeStamps without transactions
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.TimeStampEntry.AsNoTracking();

                            if (transactionsByEmployee != null && employeeGroups.Any(i => !i.AutogenTimeblocks))
                                employeesAndDatesWithTimeStampsWithoutTransactions = TimeTransactionManager.GetEmployeesAndDatesWithTimeStampsWithoutTransactions(entitiesParallel, startDate, stopDate, transactionsByEmployee, employeeIds: employeeIds);
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region PayrollImport errors
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.PayrollImportEmployeeTransaction.AsNoTracking();

                            employeesAndDatesWithPayrollImportError = GetPayrollImportErrors(entitiesParallel, ActorCompanyId, employeeIds, startDate, stopDate);
                        }
                        #endregion
                    }
                    );

                    #endregion

                    if (useAccountHierarchy)
                    {
                        List<int> employeeIdsWithWarnings = new List<int>();
                        if (employeesAndDatesWithScheduleWithoutTransactions != null)
                            employeeIdsWithWarnings.AddRange(employeesAndDatesWithScheduleWithoutTransactions.Select(i => i.Key));
                        if (employeesIdsWithScheduledPlacements != null)
                            employeeIdsWithWarnings.AddRange(employeesIdsWithScheduledPlacements);
                        if (employeesAndDatesWithStampingErrors != null)
                            employeeIdsWithWarnings.AddRange(employeesAndDatesWithStampingErrors.Select(i => i.Key));
                        if (employeesAndDatesWithTimeStampsWithoutTransactions != null)
                            employeeIdsWithWarnings.AddRange(employeesAndDatesWithTimeStampsWithoutTransactions.Select(i => i.Key));
                        employeeIdsWithWarnings = employeeIdsWithWarnings.Distinct().ToList();

                        if (!employeeAccounts.IsNullOrEmpty())
                        {
                            foreach (var employeeId in employeeIdsWithWarnings)
                            {
                                if (!employeeAccounts.ContainsKey(employeeId))
                                    continue;

                                List<EmployeeAccount> employeeAccountsByEmployee = employeeAccounts[employeeId];
                                List<AccountDTO> validAccounts = employeeAccountsByEmployee.GetValidAccounts(employeeId, startDate, stopDate, accountRepository.AllAccountInternalsDict, accountRepository.GetAccountsDict(true), onlyDefaultAccounts: true);
                                List<DateTime> validDates = employeeAccountsByEmployee.GetValidDates(employeeId, validAccounts?.Select(i => i.AccountId).ToList(), startDate, stopDate);

                                employeesAndDatesWithStampingErrors = employeesAndDatesWithStampingErrors.FilterValues(employeeId, validDates);
                                employeesAndDatesWithTimeStampsWithoutTransactions = employeesAndDatesWithTimeStampsWithoutTransactions.FilterValues(employeeId, validDates);
                                employeesAndDatesWithScheduleWithoutTransactions = employeesAndDatesWithScheduleWithoutTransactions.FilterValues(employeeId, validDates);
                            }
                        }
                    }

                    var warningRepository = new TimeTreeWarningsRepository();
                    warningRepository.AddTimeWarnings(SoeTimeAttestWarning.ScheduleWithoutTransactions, employeesAndDatesWithScheduleWithoutTransactions?.GetKeysWithValue());
                    warningRepository.AddTimeWarnings(SoeTimeAttestWarning.PlacementIsScheduled, employeesIdsWithScheduledPlacements);
                    warningRepository.AddTimeWarnings(SoeTimeAttestWarning.TimeStampErrors, employeesAndDatesWithStampingErrors?.GetKeysWithValue());
                    warningRepository.AddTimeWarnings(SoeTimeAttestWarning.TimeStampsWithoutTransactions, employeesAndDatesWithTimeStampsWithoutTransactions?.GetKeysWithValue());
                    warningRepository.AddTimeWarnings(SoeTimeAttestWarning.PayrollImport, employeesAndDatesWithPayrollImportError?.GetKeysWithValue());

                    GenerateWarningsForTimeTree(tree, warningRepository, timePeriod, employeeGroups, employeeIds);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                throw;
            }

            return tree;
        }

        public List<MessageGroupDTO> GetAttestTreeMessageGroups()
        {
            return CommunicationManager.GetMessageGroups(base.ActorCompanyId, base.UserId, setNames: false);
        }

        #endregion

        #region Group

        public List<AttestEmployeePeriodDTO> GetAttestEmployeePeriods(GetAttestEmployeePeriodsInput input)
        {
            List<AttestEmployeePeriodDTO> attestEmployePeriods = new List<AttestEmployeePeriodDTO>();

            if (string.IsNullOrEmpty(input.CacheKeyToUse) || !Guid.TryParse(input.CacheKeyToUse, out Guid cacheKey))
                cacheKey = Guid.NewGuid();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    #region Load employees

                    var (employees, repository) = LoadAttestPeriodEmployees(entities, input, cacheKey);
                    if (employees.IsNullOrEmpty())
                        return new List<AttestEmployeePeriodDTO>();

                    List<int> employeeIds = employees.Select(i => i.EmployeeId).ToList();
                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));

                    #endregion

                    #region Load company data

                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);
                    if (!useAccountHierarchy)
                        input.DoNotShowDaysOutsideEmployeeAccount = false;
                    if (input.IsAdditional)
                        input.DoNotShowDaysOutsideEmployeeAccount = true;

                    int currentAccountId = AccountManager.GetAccountHierarchySettingAccountId(entities, useAccountHierarchy);
                    bool loadAllSums = input.DoLoad(InputLoadType.SumsAll);
                    int mobileResultingAttestStateId = input.IsMobile ? SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.MobileTimeAttestResultingAttestStatus, 0, base.ActorCompanyId, 0) : 0;
                    bool useProjectExtendedTimeRegistration = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProjectUseExtendedTimeRegistration, 0, base.ActorCompanyId, 0);

                    bool loadUnhandledShiftChanges = input.DoLoad(InputLoadType.UnhandledShiftChanges);
                    bool useExtraShifts = loadUnhandledShiftChanges && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningSetShiftAsExtra, 0, input.ActorCompanyId, 0);
                    List<int> timeDeviationCausesOvertime = loadUnhandledShiftChanges ? base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(input.ActorCompanyId, cacheKey)).GetOvertimeDeviationCauseIds() : null;
                    bool doLoadUnhandledOvertimeDays = loadUnhandledShiftChanges && !timeDeviationCausesOvertime.IsNullOrEmpty();

                    TimePeriod timePeriod = input.TimePeriodId.HasValue ? TimePeriodManager.GetTimePeriod(entities, input.TimePeriodId.Value, input.ActorCompanyId) : null;
                    AttestStateDTO mobileResultingAttestState = AttestManager.GetAttestState(entities, mobileResultingAttestStateId)?.ToDTO();
                    List<AttestState> attestStates = AttestManager.GetAttestStates(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                    List<AccountDTO> accountInternalsCompany = input.DoNotShowDaysOutsideEmployeeAccount ? base.GetAccountInternalsFromCache(entities, CacheConfig.Company(base.ActorCompanyId)) : null;

                    #endregion

                    #region Load employee data

                    var employeeInfos = GetValidEmployeeInfos(employees, employeeGroups, input.StartDate, input.StopDate);
                    var scheduleByEmployee = GetAttestPeriodSchedule(entities, input.StartDate, input.StopDate, employeeIds);
                    var scheduleSwapShiftRequestRowsByEmployee = GetTimeScheduleSwapRequestRows(entities, input.StartDate, input.StopDate, employeeIds);
                    var timeBlocksByEmployee = GetAttestPeriodTimBlocks(entities, input.StartDate, input.StopDate, employeeIds);
                    var projectTimeBlocksByEmployee = useProjectExtendedTimeRegistration ? GetProjectTimeBlocks(entities, input.StartDate, input.StopDate, employeeIds) : null;
                    var transactionsByEmployee = GetAttestPeriodTransactions(entities, input, employeeIds, timePeriod, cacheKey);
                    var payrollImportErrorsByEmployee = GetPayrollImportErrors(entities, input.ActorCompanyId, input.FilterEmployeeIds, input.StartDate, input.StopDate);
                    var (timeStampEntryDatesByEmployee, timeBlockDatesInvalidByEmployee) = GetAttestPeriodStamping(entities, input, employeeInfos);

                    #endregion

                    #region Process Employees

                    foreach (var employeeInfo in employeeInfos)
                    {
                        #region Filter data

                        var scheduleBlocks = scheduleByEmployee.GetList(employeeInfo.Employee.EmployeeId);
                        var scheduleSwapShiftRequestRows = scheduleSwapShiftRequestRowsByEmployee.GetList(employeeInfo.Employee.EmployeeId);
                        var timeBlocks = timeBlocksByEmployee.GetList(employeeInfo.Employee.EmployeeId);
                        var projectTimeBlocks = projectTimeBlocksByEmployee.GetList(employeeInfo.EmployeeId, true);
                        var transactions = transactionsByEmployee.GetList(employeeInfo.Employee.EmployeeId);
                        if (!transactions.IsNullOrEmpty())
                            transactions = TimeTransactionManager.FilterTimePayrollTransactions(transactions, repository, employeeInfo.Employee, input.StartDate, input.StopDate, employmentDates: employeeInfo.EmploymentDates, skipFilterOnAccounts: input.DoNotShowDaysOutsideEmployeeAccount || !employeeInfo.Employee.AdditionalOnAccountIds.IsNullOrEmpty());
                        var timeStampEntryDates = !employeeInfo.AutogenTimeblocks ? timeStampEntryDatesByEmployee.FirstOrDefault(e => e.EmployeeId == employeeInfo.EmployeeId) : null;
                        var timeBlockDatesInvalid = timeBlockDatesInvalidByEmployee.GetList(employeeInfo.EmployeeId, true, !employeeInfo.AutogenTimeblocks);

                        var payrollImportDates = payrollImportErrorsByEmployee.GetList(employeeInfo.EmployeeId);
                        int? standardTimeDeviationCauseId = TimeDeviationCauseManager.GetTimeDeviationCauseIdFromPrio(employeeInfo.Employee, input.StartDate, employeeGroups: employeeGroups);

                        List<DateTime> validDates = null;
                        if (input.DoNotShowDaysOutsideEmployeeAccount)
                        {
                            validDates = AccountManager.GetValidDatesOnGivenAccounts(employeeInfo.EmployeeId, input.StartDate, input.StopDate, currentAccountId, accountInternalsCompany, scheduleBlocks, transactions).Keys.ToList();
                            validDates = validDates.Intersect(employeeInfo.EmploymentDates).ToList();
                            transactions = transactions.Where(transaction => validDates.Any(date => date == transaction.Date)).ToList();
                            scheduleBlocks = scheduleBlocks.Where(templateBlock => validDates.Any(date => date == templateBlock.Date)).ToList();
                        }
                        else
                        {
                            validDates = employeeInfo.EmploymentDates;
                        }

                        #endregion

                        #region Process data

                        var attestEmployePeriod = new AttestEmployeePeriodDTO()
                        {
                            StartDate = validDates.Any() ? validDates.Min() : input.StartDate,
                            StopDate = validDates.Any() ? validDates.Max() : input.StopDate,
                            EmployeeId = employeeInfo.EmployeeId,
                            EmployeeNr = employeeInfo.Employee.EmployeeNr,
                            EmployeeName = employeeInfo.Employee.Name,
                            EmployeeNrAndName = employeeInfo.Employee.EmployeeNrAndName,
                            EmployeeSex = employeeInfo.Employee.Sex,
                            AdditionalOnAccountIds = employeeInfo.Employee.AdditionalOnAccountIds,
                            AttestStates = employeeInfo.AttestStates?.ToDTOs()?.ToList(),
                        };

                        List<TimeTreeDayDTO> treeDays = new List<TimeTreeDayDTO>();
                        SetEmployeePeriodSchedule(attestEmployePeriod, ref treeDays, validDates, scheduleBlocks, scheduleSwapShiftRequestRows);
                        SetEmployeePeriodTimeBlocks(attestEmployePeriod, ref treeDays, validDates, timeBlocks, timeDeviationCausesOvertime, standardTimeDeviationCauseId);
                        SetEmployeePeriodTransactions(entities, attestEmployePeriod, ref treeDays, input, transactions, projectTimeBlocks, attestStates, loadAllSums);
                        SetEmployeePeriodPayrollImports(attestEmployePeriod, payrollImportDates.Any());
                        if (!employeeInfo.AutogenTimeblocks)
                            SetEmployeePeriodTimeStamps(attestEmployePeriod, ref treeDays, timeStampEntryDates, timeBlockDatesInvalid, transactions);
                        SetEmployeePeriodTotals(attestEmployePeriod, treeDays, mobileResultingAttestState);

                        attestEmployePeriods.Add(attestEmployePeriod);

                        #endregion
                    }

                    if (loadUnhandledShiftChanges)
                        SetEmployeeUnhandledShiftChanges(doLoadUnhandledOvertimeDays, useExtraShifts, attestEmployePeriods);

                    #endregion
                }
                catch (Exception ex)
                {
                    LogError(ex, this.log);
                }
            }

            return attestEmployePeriods.SortAlphanumeric();
        }

        public List<AttestEmployeePeriodDTO> GetAttestEmployeePeriodsPreview(TimeEmployeeTreeDTO tree, TimeEmployeeTreeGroupNodeDTO groupNode)
        {
            if (tree == null || groupNode?.EmployeeNodes == null)
                return new List<AttestEmployeePeriodDTO>();

            List<AttestEmployeePeriodDTO> attestEmployePeriod = new List<AttestEmployeePeriodDTO>();

            foreach (TimeEmployeeTreeNodeDTO employeeNode in groupNode.GetEmployeeNodes().Where(i => i.Visible))
            {
                attestEmployePeriod.Add(new AttestEmployeePeriodDTO
                {
                    StartDate = tree.StartDate,
                    StopDate = tree.StopDate,
                    EmployeeId = employeeNode.EmployeeId,
                    EmployeeNr = employeeNode.EmployeeNr,
                    EmployeeName = employeeNode.EmployeeName,
                    EmployeeNrAndName = employeeNode.EmployeeNrAndName,
                    EmployeeSex = employeeNode.EmployeeSex,
                    AttestStates = employeeNode.AttestStates,
                    AttestStateSort = employeeNode.AttestStateSort,
                    AttestStateColor = employeeNode.AttestStateColor,
                    AttestStateName = employeeNode.AttestStateName,
                });
            }

            return attestEmployePeriod;
        }

        public TimeEmployeeTreeGroupNodeDTO RefreshAttestTreeGroupNode(TimeEmployeeTreeDTO tree, TimeEmployeeTreeGroupNodeDTO groupNode)
        {
            if (tree == null || groupNode == null)
                return null;

            TimeEmployeeTreeGroupNodeDTO refreshedGroupNode = tree.GetGroupNode(groupNode.Guid);
            if (refreshedGroupNode != null)
            {
                refreshedGroupNode.TimeEmployeePeriods = groupNode.TimeEmployeePeriods;
                if (refreshedGroupNode.TimeEmployeePeriods != null)
                {
                    foreach (AttestEmployeePeriodDTO timeEmployeePeriod in refreshedGroupNode.TimeEmployeePeriods)
                    {
                        TimeEmployeeTreeNodeDTO employeeNode = refreshedGroupNode.EmployeeNodes?.FirstOrDefault(i => i.EmployeeId == timeEmployeePeriod.EmployeeId);
                        if (employeeNode != null)
                        {
                            timeEmployeePeriod.AttestStates = employeeNode.AttestStates;
                            timeEmployeePeriod.AttestStateSort = employeeNode.AttestStateSort;
                            timeEmployeePeriod.AttestStateColor = employeeNode.AttestStateColor;
                            timeEmployeePeriod.AttestStateName = employeeNode.AttestStateName;
                        }
                    }
                }
            }
            return refreshedGroupNode;
        }

        private void SetEmployeePeriodSchedule(AttestEmployeePeriodDTO attestEmployePeriod, ref List<TimeTreeDayDTO> treeDays, List<DateTime> validDates, List<TimeScheduleTemplateBlockSmallDTO> scheduleBlocks, List<TimeScheduleSwapRequestRow> scheduleSwapShiftRequestRows)
        {
            if (attestEmployePeriod == null || treeDays == null || scheduleBlocks.IsNullOrEmpty())
                return;

            foreach (DateTime scheduleDate in scheduleBlocks.Select(i => i.Date.Value).Distinct())
            {
                if (validDates != null && !validDates.Contains(scheduleDate))
                    continue;

                var treeDay = new TimeTreeDayDTO(attestEmployePeriod.EmployeeId, scheduleDate);

                var scheduleBlocksByDate = scheduleBlocks.GetSchedule(scheduleDate);
                if (scheduleBlocksByDate.Any())
                {
                    var templateBlockWork = scheduleBlocksByDate.GetWork();
                    var templateBlockBreaks = scheduleBlocksByDate.GetBreaks();

                    treeDay.ScheduleIn = templateBlockWork.FirstOrDefault()?.StartTime ?? CalendarUtility.DATETIME_DEFAULT;
                    treeDay.ScheduleOut = templateBlockWork.LastOrDefault()?.StopTime ?? CalendarUtility.DATETIME_DEFAULT;
                    treeDay.ScheduleTime = new TimeSpan(templateBlockWork.Sum(i => i.Length.Ticks));
                    treeDay.ScheduleBreakTime = new TimeSpan(templateBlockBreaks.Sum(i => i.Length.Ticks));
                }

                var standbyBlocksByDate = scheduleBlocks.GetStandby(scheduleDate);
                if (standbyBlocksByDate.Any())
                {
                    treeDay.StandbyIn = standbyBlocksByDate.FirstOrDefault()?.StartTime;
                    treeDay.StandbyOut = standbyBlocksByDate.LastOrDefault()?.StopTime;
                    treeDay.StandbyTime = new TimeSpan(standbyBlocksByDate.Sum(i => i.Length.Ticks));
                }

                treeDays.Add(treeDay);
            }

            attestEmployePeriod.ScheduleDays = treeDays.Count(i => i.HasSchedule);
            attestEmployePeriod.ScheduleBreakTime = new TimeSpan(treeDays.Sum(i => i.ScheduleBreakTime.Ticks));
            attestEmployePeriod.ScheduleTime = new TimeSpan(treeDays.Sum(i => i.ScheduleTime.Ticks)).Subtract(attestEmployePeriod.ScheduleBreakTime);
            attestEmployePeriod.StandbyTime = new TimeSpan(treeDays.Sum(i => i.StandbyTime.Ticks));
            attestEmployePeriod.HasShiftSwaps = scheduleSwapShiftRequestRows.Any(s => s.IsApproved); //TODO: Only shorter or longer
        }

        private void SetEmployeePeriodTimeBlocks(AttestEmployeePeriodDTO attestEmployePeriod, ref List<TimeTreeDayDTO> treeDays, List<DateTime> validDates, List<TimeTreeTimeBlockDTO> timeBlocks, List<int> timeDeviationCausesOvertime, int? standardTimeDeviationCauseId)
        {
            if (attestEmployePeriod == null || treeDays == null)
                return;

            if (!timeBlocks.IsNullOrEmpty())
            {
                foreach (var timeBlocksBydate in timeBlocks.Where(i => i.StartTime < i.StopTime).GroupBy(i => i.Date))
                {
                    DateTime date = timeBlocksBydate.Key;
                    if (validDates != null && !validDates.Contains(date))
                        continue;

                    var treeDay = treeDays.FirstOrDefault(i => i.Date == date) ?? new TimeTreeDayDTO(attestEmployePeriod.EmployeeId, date);
                    foreach (var timeBlock in timeBlocksBydate.OrderBy(i => i.Date).ThenBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList())
                    {
                        TimeSpan timeBlockTime = timeBlock.StopTime.Subtract(timeBlock.StartTime);
                        if (timeBlockTime.TotalMinutes <= 0)
                            continue;

                        var (_, insideSchedule, outsideSchedule) = GetTimeBlockLengths(treeDay.ScheduleIn, treeDay.ScheduleOut, treeDay.StandbyIn, treeDay.StandbyOut, timeBlock.StartTime, timeBlock.StopTime, timeBlock.TimeDeviationCauseStartId, standardTimeDeviationCauseId);
                        var (isPresence, isAbsence, isBreak) = GetTimeBlockTypes(timeBlock.TimeCodeTypes);

                        if (isPresence)
                        {
                            treeDay.PresenceTime = treeDay.PresenceTime.Add(timeBlockTime);
                            if (insideSchedule.TotalMinutes > 0)
                                treeDay.HasWorkedInsideSchedule = true;
                            if (outsideSchedule.TotalMinutes > 0)
                                treeDay.HasWorkedOutsideSchedule = true;
                        }
                        if (isAbsence && !isBreak)
                            treeDay.AbsenceTime = treeDay.AbsenceTime.Add(timeBlockTime);
                        if (isBreak)
                            treeDay.PresenceBreakTime = treeDay.PresenceBreakTime.Add(timeBlockTime);
                    }

                    if (!treeDays.Any(i => i.Key == treeDay.Key))
                        treeDays.Add(treeDay);
                }
            }

            attestEmployePeriod.PresenceDays = treeDays.Count(i => i.HasPresenceTime);
            attestEmployePeriod.HasWorkedInsideSchedule = treeDays.Any(i => i.HasWorkedInsideSchedule);
            attestEmployePeriod.HasWorkedOutsideSchedule = treeDays.Any(i => i.HasWorkedOutsideSchedule);
            attestEmployePeriod.PresenceTime = new TimeSpan(treeDays.Sum(i => i.PresenceTime.Ticks));
            attestEmployePeriod.PresenceBreakTime = new TimeSpan(treeDays.Sum(i => i.PresenceBreakTime.Ticks));
            attestEmployePeriod.AbsenceTime = new TimeSpan(treeDays.Sum(i => i.AbsenceTime.Ticks));
            attestEmployePeriod.OvertimeDates = timeBlocks.GetDatesWithTimeDeviationCause(timeDeviationCausesOvertime);
        }

        private void SetEmployeePeriodTransactions(CompEntities entities, AttestEmployeePeriodDTO attestEmployePeriod, ref List<TimeTreeDayDTO> treeDays, GetAttestEmployeePeriodsInput input, List<TimePayrollTransactionTreeDTO> transactionsForEmployee, List<TimeTreeProjectTimeBlockDTO> projectTimeBlocks, List<AttestState> attestStates, bool loadAllSums)
        {
            if (attestEmployePeriod == null || transactionsForEmployee.IsNullOrEmpty())
                return;

            attestEmployePeriod.AttestStates = GetAttestStatesForEmployee(transactionsForEmployee, SoeAttestTreeMode.TimeAttest, attestEmployePeriod.EmployeeId, attestStates).ToDTOs().ToList();

            foreach (var transactionsByDate in transactionsForEmployee.GroupBy(i => i.Date))
            {
                var treeDay = treeDays.GetOrCreateDay(attestEmployePeriod.EmployeeId, transactionsByDate.Key);
                treeDay.HasTransactions = true;
            }

            SetEmployeePeriodTransactionSums(entities, attestEmployePeriod, input, transactionsForEmployee, projectTimeBlocks, loadAllSums);
        }

        private void SetEmployeePeriodTotals(AttestEmployeePeriodDTO attestEmployePeriod, List<TimeTreeDayDTO> treeDays, AttestStateDTO mobileResultingAttestState)
        {
            if (attestEmployePeriod == null || treeDays == null)
                return;

            attestEmployePeriod.HasTransactions = treeDays.Any(d => d.HasTransactions);
            attestEmployePeriod.HasScheduleWithoutTransactions = treeDays.Any(d => d.HasScheduleWithoutTransactions);
            attestEmployePeriod.SetLowestAttestState(attestEmployePeriod.AttestStates);
            attestEmployePeriod.IsAttested = mobileResultingAttestState != null && !attestEmployePeriod.AttestStates.Any(i => i.Sort <= mobileResultingAttestState.Sort);
            if (!attestEmployePeriod.IsAttested)
                attestEmployePeriod.IsToBeAttested = treeDays.Any(d => d.HasSchedule || d.HasTimeStampEntrys || d.HasTransactions);
        }

        private void SetEmployeePeriodTransactionSums(CompEntities entities, AttestEmployeePeriodDTO attestEmployePeriod, GetAttestEmployeePeriodsInput input, List<TimePayrollTransactionTreeDTO> transactions, List<TimeTreeProjectTimeBlockDTO> projectTimeBlocks, bool loadAllSums)
        {
            if (input == null || attestEmployePeriod == null || transactions.IsNullOrEmpty())
                return;

            foreach (var transaction in transactions)
            {
                CalculateTransactionSumsPayroll(transaction);
                CalculateTransactionSumsInvoice(transaction);
            }

            FormatSums();

            void CalculateTransactionSumsPayroll(TimePayrollTransactionTreeDTO transaction)
            {
                if (transaction.Quantity == 0 || transaction.RetroactivePayrollOutcomeId.HasValue)
                    return;

                var payrollProduct = GetPayrollProductFromCache(entities, CacheConfig.Company(ActorCompanyId), transaction.ProductId);
                if (payrollProduct == null)
                    return;

                TimeSpan quantity = new TimeSpan(0, (int)transaction.Quantity, 0);

                if (payrollProduct.Payed)
                    attestEmployePeriod.PresencePayedTime = attestEmployePeriod.PresencePayedTime.Add(quantity);

                if (transaction.IsAdditionOrDeduction && (DoLoad(InputLoadType.SumExpenseRows, forceMobile: true) || DoLoad(InputLoadType.SumExpenseAmount, forceMobile: true)))
                    attestEmployePeriod.AddExpense(transaction.TimeCodeTransactionId, transaction.Amount, payrollProduct.ShortName);

                if (input.IsWeb)
                {
                    //Time
                    if (DoLoad(InputLoadType.SumTimeAccumulator) && transaction.IsTimeAccumulator())
                        attestEmployePeriod.SumTimeAccumulator = transaction.IsTimeAccumulatorNegate() ? attestEmployePeriod.SumTimeAccumulator.Subtract(CalendarUtility.GetTimeSpanFromMinutes(transaction.Quantity)) : attestEmployePeriod.SumTimeAccumulator.Add(CalendarUtility.GetTimeSpanFromMinutes(transaction.Quantity));
                    if (DoLoad(InputLoadType.SumTimeAccumulatorOverTime) && transaction.IsTimeAccumulatorOverTime())
                        attestEmployePeriod.SumTimeAccumulatorOverTime = attestEmployePeriod.SumTimeAccumulatorOverTime.Add(quantity);
                    if (DoLoad(InputLoadType.SumTimeWorkedScheduledTime) && transaction.IsTimeScheduledTime())
                        attestEmployePeriod.SumTimeWorkedScheduledTime = attestEmployePeriod.SumTimeWorkedScheduledTime.Add(quantity);

                    //Absence
                    if (DoLoad(InputLoadType.SumGrossSalaryAbsence) && transaction.IsAbsence())
                        attestEmployePeriod.SumGrossSalaryAbsence = attestEmployePeriod.SumGrossSalaryAbsence.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryAbsenceVacation) && transaction.IsAbsenceVacation())
                        attestEmployePeriod.SumGrossSalaryAbsenceVacation = attestEmployePeriod.SumGrossSalaryAbsenceVacation.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryAbsenceSick) && transaction.IsAbsenceSickOrWorkInjury())
                    {
                        attestEmployePeriod.SumGrossSalaryAbsenceSick = attestEmployePeriod.SumGrossSalaryAbsenceSick.Add(quantity);
                        if (!attestEmployePeriod.AbsenceSickDates.Contains(transaction.Date))
                            attestEmployePeriod.AbsenceSickDates.Add(transaction.Date);
                    }
                    if (DoLoad(InputLoadType.SumGrossSalaryAbsenceLeaveOfAbsence) && transaction.IsLeaveOfAbsence())
                        attestEmployePeriod.SumGrossSalaryAbsenceLeaveOfAbsence = attestEmployePeriod.SumGrossSalaryAbsenceLeaveOfAbsence.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryAbsenceParentalLeave) && transaction.IsParentalLeave())
                        attestEmployePeriod.SumGrossSalaryAbsenceParentalLeave = attestEmployePeriod.SumGrossSalaryAbsenceParentalLeave.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryAbsenceTemporaryParentalLeave) && transaction.IsAbsenceTemporaryParentalLeave())
                        attestEmployePeriod.SumGrossSalaryAbsenceTemporaryParentalLeave = attestEmployePeriod.SumGrossSalaryAbsenceTemporaryParentalLeave.Add(quantity);

                    //Weekend salary
                    if (DoLoad(InputLoadType.SumGrossSalaryWeekendSalary) && transaction.IsWeekendSalary())
                        attestEmployePeriod.SumGrossSalaryWeekendSalary = attestEmployePeriod.SumGrossSalaryWeekendSalary.Add(quantity);

                    //Duty salary
                    if (DoLoad(InputLoadType.SumGrossSalaryDuty) && transaction.IsDutySalary())
                        attestEmployePeriod.SumGrossSalaryDuty = attestEmployePeriod.SumGrossSalaryDuty.Add(quantity);

                    //Additional time
                    if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime) && transaction.IsAddedTime())
                        attestEmployePeriod.SumGrossSalaryAdditionalTime = attestEmployePeriod.SumGrossSalaryAdditionalTime.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime) && transaction.IsAddedTimeCompensation35())
                        attestEmployePeriod.SumGrossSalaryAdditionalTime35 = attestEmployePeriod.SumGrossSalaryAdditionalTime35.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime) && transaction.IsAddedTimeCompensation70())
                        attestEmployePeriod.SumGrossSalaryAdditionalTime70 = attestEmployePeriod.SumGrossSalaryAdditionalTime70.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime) && transaction.IsAddedTimeCompensation100())
                        attestEmployePeriod.SumGrossSalaryAdditionalTime100 = attestEmployePeriod.SumGrossSalaryAdditionalTime100.Add(quantity);

                    //Overtime
                    if (DoLoad(InputLoadType.SumGrossSalaryOvertime) && (transaction.IsOvertimeCompensation() || transaction.IsOvertimeAddition()))
                        attestEmployePeriod.SumGrossSalaryOvertime = attestEmployePeriod.SumGrossSalaryOvertime.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOvertime35) && (transaction.IsOvertimeCompensation35() || transaction.IsOvertimeAddition35()))
                        attestEmployePeriod.SumGrossSalaryOvertime35 = attestEmployePeriod.SumGrossSalaryOvertime35.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOvertime50) && (transaction.IsOvertimeCompensation50() || transaction.IsOvertimeAddition50()))
                        attestEmployePeriod.SumGrossSalaryOvertime50 = attestEmployePeriod.SumGrossSalaryOvertime50.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOvertime70) && (transaction.IsOvertimeCompensation70() || transaction.IsOvertimeAddition70()))
                        attestEmployePeriod.SumGrossSalaryOvertime70 = attestEmployePeriod.SumGrossSalaryOvertime70.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOvertime100) && (transaction.IsOvertimeCompensation100() || transaction.IsOvertimeAddition100()))
                        attestEmployePeriod.SumGrossSalaryOvertime100 = attestEmployePeriod.SumGrossSalaryOvertime100.Add(quantity);

                    //OB addition
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition) && transaction.IsOBAddition())
                        attestEmployePeriod.SumGrossSalaryOBAddition = attestEmployePeriod.SumGrossSalaryOBAddition.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition40) && transaction.IsOBAddition40())
                        attestEmployePeriod.SumGrossSalaryOBAddition40 = attestEmployePeriod.SumGrossSalaryOBAddition40.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition50) && transaction.IsOBAddition50())
                        attestEmployePeriod.SumGrossSalaryOBAddition50 = attestEmployePeriod.SumGrossSalaryOBAddition50.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition57) && transaction.IsOBAddition57())
                        attestEmployePeriod.SumGrossSalaryOBAddition57 = attestEmployePeriod.SumGrossSalaryOBAddition57.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition70) && transaction.IsOBAddition70())
                        attestEmployePeriod.SumGrossSalaryOBAddition70 = attestEmployePeriod.SumGrossSalaryOBAddition70.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition79) && transaction.IsOBAddition79())
                        attestEmployePeriod.SumGrossSalaryOBAddition79 = attestEmployePeriod.SumGrossSalaryOBAddition79.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition100) && transaction.IsOBAddition100())
                        attestEmployePeriod.SumGrossSalaryOBAddition100 = attestEmployePeriod.SumGrossSalaryOBAddition100.Add(quantity);
                    if (DoLoad(InputLoadType.SumGrossSalaryOBAddition113) && transaction.IsOBAddition113())
                        attestEmployePeriod.SumGrossSalaryOBAddition113 = attestEmployePeriod.SumGrossSalaryOBAddition113.Add(quantity);
                }
            }

            void CalculateTransactionSumsInvoice(TimePayrollTransactionTreeDTO transaction)
            {
                if (projectTimeBlocks.IsNullOrEmpty() || !transaction.TimeCodeTransactionId.HasValue || !DoLoad(InputLoadType.SumInvoicedTime))
                    return;

                int invoiceQuantity = projectTimeBlocks?.FirstOrDefault(b => b.TimeCodeTransactionIds.Contains(transaction.TimeCodeTransactionId.Value))?.InvoiceQuantity ?? 0;
                if (invoiceQuantity > 0)
                    attestEmployePeriod.SumInvoicedTime = attestEmployePeriod.SumInvoicedTime.Add(new TimeSpan(0, invoiceQuantity, 0));
            }

            void FormatSums()
            {
                attestEmployePeriod.SumGrossSalaryAbsenceText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAbsence);
                attestEmployePeriod.SumGrossSalaryAbsenceVacationText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAbsenceVacation);
                attestEmployePeriod.SumGrossSalaryAbsenceSickText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAbsenceSick);
                attestEmployePeriod.SumGrossSalaryAbsenceLeaveOfAbsenceText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAbsenceLeaveOfAbsence);
                attestEmployePeriod.SumGrossSalaryAbsenceParentalLeaveText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAbsenceParentalLeave);
                attestEmployePeriod.SumGrossSalaryAbsenceTemporaryParentalLeaveText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAbsenceTemporaryParentalLeave);
                attestEmployePeriod.SumGrossSalaryWeekendSalaryText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryWeekendSalary);
                attestEmployePeriod.SumGrossSalaryDutyText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryDuty);
                attestEmployePeriod.SumGrossSalaryAdditionalTimeText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAdditionalTime);
                attestEmployePeriod.SumGrossSalaryAdditionalTime35Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAdditionalTime35);
                attestEmployePeriod.SumGrossSalaryAdditionalTime70Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAdditionalTime70);
                attestEmployePeriod.SumGrossSalaryAdditionalTime100Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryAdditionalTime100);
                attestEmployePeriod.SumGrossSalaryOBAdditionText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition);
                attestEmployePeriod.SumGrossSalaryOBAddition40Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition40);
                attestEmployePeriod.SumGrossSalaryOBAddition50Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition50);
                attestEmployePeriod.SumGrossSalaryOBAddition57Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition57);
                attestEmployePeriod.SumGrossSalaryOBAddition70Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition70);
                attestEmployePeriod.SumGrossSalaryOBAddition79Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition79);
                attestEmployePeriod.SumGrossSalaryOBAddition100Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition100);
                attestEmployePeriod.SumGrossSalaryOBAddition113Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOBAddition113);
                attestEmployePeriod.SumGrossSalaryOvertimeText = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOvertime);
                attestEmployePeriod.SumGrossSalaryOvertime35Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOvertime35);
                attestEmployePeriod.SumGrossSalaryOvertime50Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOvertime50);
                attestEmployePeriod.SumGrossSalaryOvertime70Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOvertime70);
                attestEmployePeriod.SumGrossSalaryOvertime100Text = FormatTimeSpan(attestEmployePeriod.SumGrossSalaryOvertime100);
                attestEmployePeriod.SumTimeAccumulatorText = FormatTimeSpan(attestEmployePeriod.SumTimeAccumulator);
                attestEmployePeriod.SumTimeAccumulatorOverTimeText = FormatTimeSpan(attestEmployePeriod.SumTimeAccumulatorOverTime);
                attestEmployePeriod.SumTimeWorkedScheduledTimeText = FormatTimeSpan(attestEmployePeriod.SumTimeWorkedScheduledTime);
                attestEmployePeriod.SumInvoicedTimeText = FormatTimeSpan(attestEmployePeriod.SumInvoicedTime);
            }

            string FormatTimeSpan(TimeSpan timeSpan)
            {
                if (timeSpan.TotalMinutes == 0)
                    return string.Empty;
                return CalendarUtility.FormatTimeSpan(timeSpan, false, false, true, true);
            }

            bool DoLoad(InputLoadType loadType, bool forceMobile = false)
            {
                if (forceMobile && input.Device == SoeAttestDevice.Mobile)
                    return true;
                return input.DoLoad(loadType, loadAllSums);
            }
        }

        private void SetEmployeePeriodPayrollImports(AttestEmployeePeriodDTO attestEmployePeriod, bool value)
        {
            attestEmployePeriod.HasPayrollImports = value;
        }

        private void SetEmployeePeriodTimeStamps(AttestEmployeePeriodDTO attestEmployePeriod, ref List<TimeTreeDayDTO> treeDays, EmployeeDatesDTO timeStampEntryDates, List<TimeBlockDate> timeBlockDatesInvalid, List<TimePayrollTransactionTreeDTO> transactions)
        {
            attestEmployePeriod.HasTimeStampsWithoutTransactions = TimeTransactionManager.HasTimeStampsWithoutTransactions(attestEmployePeriod.EmployeeId, timeStampEntryDates, transactions);
            attestEmployePeriod.HasInvalidTimeStamps = !timeBlockDatesInvalid.IsNullOrEmpty();

            if (timeStampEntryDates.HasDates())
            {
                foreach (DateTime date in timeStampEntryDates.Dates)
                {
                    TimeTreeDayDTO treeDay = treeDays.GetOrCreateDay(timeStampEntryDates.EmployeeId, date);
                    treeDay.HasTimeStampEntrys = true;
                }
            }
        }

        private (List<Employee>, EmployeeAuthModelRepository) LoadAttestPeriodEmployees(CompEntities entities, GetAttestEmployeePeriodsInput input, Guid cacheKey)
        {
            if (input == null)
                return (null, null);

            List<Employee> employees;
            EmployeeAuthModelRepository repository;

            bool fetchByTree = false;
            if (input.IsMobile && (input.FilterEmployeeIds.IsNullOrEmpty() || input.IncludeAdditionalEmployees))
                fetchByTree = true;
            else if (input.IsWeb && input.IsAdditional && input.IncludeAdditionalEmployees)
                fetchByTree = true;

            if (fetchByTree)
            {
                var settings = new TimeEmployeeTreeSettings()
                {
                    IncludeAdditionalEmployees = input.IncludeAdditionalEmployees,
                    DoNotShowDaysOutsideEmployeeAccount = input.DoNotShowDaysOutsideEmployeeAccount,
                };
                employees = GetAttestTreeEmployees(entities, out repository, input.StartDate, input.StopDate, input.TimePeriodId, input.FilterEmployeeIds, settings: settings);
            }
            else
            {
                bool? onlyDefaultEmployeeAuthModel = input.IncludeAdditionalEmployees ? false : (bool?)null;
                employees = GetEmployeesWithRepositoryFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey, keepAlive: true), out repository, base.UserId, base.RoleId, input.StartDate, input.StopDate, input.FilterEmployeeIds, onlyDefaultEmployeeAuthModel: onlyDefaultEmployeeAuthModel);
            }

            return (employees, repository);
        }

        private Dictionary<int, List<TimeScheduleTemplateBlockSmallDTO>> GetAttestPeriodSchedule(CompEntities entities, DateTime startDate, DateTime stopDate, List<int> employeeIds)
        {
            return GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entities, base.ActorCompanyId, employeeIds, startDate, stopDate), GetAttestPeriodSchedule)
                .Where(b => b.StartTime <= b.StopTime && !b.TimeScheduleScenarioHeadId.HasValue)
                .GroupBy(b => b.EmployeeId.Value).ToDictionary(k => k.Key, v => v.ToList());
        }

        private List<TimeScheduleTemplateBlockSmallDTO> GetAttestPeriodSchedule(GetDataInBatchesModel model)
        {
            if (!model.IsValid(requireDates: true))
                return new List<TimeScheduleTemplateBlockSmallDTO>();

            return (from b in model.Entities.TimeScheduleTemplateBlock
                    where b.EmployeeId.HasValue &&
                    model.BatchIds.Contains(b.EmployeeId.Value) &&
                    b.Date >= model.StartDate &&
                    b.Date <= model.StopDate &&
                    b.State == (int)SoeEntityState.Active
                    select new TimeScheduleTemplateBlockSmallDTO()
                    {
                        TimeScheduleScenarioHeadId = b.TimeScheduleScenarioHeadId,
                        EmployeeId = b.EmployeeId.Value,
                        Date = b.Date,
                        StartTime = b.StartTime,
                        StopTime = b.StopTime,
                        Type = b.Type,
                        IsBreak = b.BreakType > 0 || b.TimeCode.Type == (int)SoeTimeCodeType.Break,
                        TimeCodeId = b.TimeCodeId,
                        AccountId = b.AccountId,
                    }).ToList();

        }

        private Dictionary<int, List<TimeScheduleSwapRequestRow>> GetTimeScheduleSwapRequestRows(CompEntities entities, DateTime startDate, DateTime stopDate, List<int> employeeIds)
        {
            return TimeScheduleManager.GetTimeScheduleSwapRequestRows(entities, base.ActorCompanyId, employeeIds, startDate, stopDate)
                .GroupBy(b => b.EmployeeId)
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        private Dictionary<int, List<TimeTreeTimeBlockDTO>> GetAttestPeriodTimBlocks(CompEntities entities, DateTime startDate, DateTime stopDate, List<int> employeeIds)
        {
            return GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entities, base.ActorCompanyId, employeeIds, startDate, stopDate), GetAttestPeriodTimBlocks)
                .Where(i => i.StartTime <= i.StopTime)
                .GroupBy(b => b.EmployeeId)
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        private List<TimeTreeTimeBlockDTO> GetAttestPeriodTimBlocks(GetDataInBatchesModel model)
        {
            if (!model.IsValid(requireDates: true))
                return new List<TimeTreeTimeBlockDTO>();

            return (from b in model.Entities.TimeBlock
                    where model.BatchIds.Contains(b.EmployeeId) &&
                    b.TimeBlockDate.EmployeeId == b.EmployeeId &&
                    b.TimeBlockDate.Date >= model.StartDate &&
                    b.TimeBlockDate.Date <= model.StopDate &&
                    b.State == (int)SoeEntityState.Active
                    select new TimeTreeTimeBlockDTO
                    {
                        TimeBlockId = b.TimeBlockId,
                        TimeBlockDateId = b.TimeBlockDateId,
                        EmployeeId = b.TimeBlockDate.EmployeeId,
                        TimeDeviationCauseStartId = b.TimeDeviationCauseStartId,
                        TimeDeviationCauseStopId = b.TimeDeviationCauseStopId,
                        Date = b.TimeBlockDate.Date,
                        StartTime = b.StartTime,
                        StopTime = b.StopTime,
                        TimeCodeTypes = b.TimeCode.Select(i => i.Type)
                    }).ToList();
        }

        private Dictionary<int, List<TimePayrollTransactionTreeDTO>> GetAttestPeriodTransactions(CompEntities entities, GetAttestEmployeePeriodsInput input, List<int> employeeIds, TimePeriod timePeriod, Guid cacheKey)
        {
            List<TimePayrollTransactionTreeDTO> transactions = GetTimePayrollTransactionsForTreeFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey), input.StartDate, input.StopDate, timePeriod, employeeIds, includeAccounting: input.DoNotShowDaysOutsideEmployeeAccount);
            transactions = transactions.Where(i => !i.PayrollStartValueRowId.HasValue).ToList();
            return transactions.GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList());
        }

        private Dictionary<int, List<TimeTreeProjectTimeBlockDTO>> GetProjectTimeBlocks(CompEntities entities, DateTime startDate, DateTime stopDate, List<int> employeeIds)
        {
            List<TimeTreeProjectTimeBlockDTO> projectTimeBlocks = null;

            if (employeeIds != null)
            {
                projectTimeBlocks = (from b in entities.ProjectTimeBlock
                                     where employeeIds.Contains(b.EmployeeId) &&
                                     b.TimeBlockDate.EmployeeId == b.EmployeeId &&
                                     b.TimeBlockDate.Date >= startDate &&
                                     b.TimeBlockDate.Date <= stopDate &&
                                     b.State == (int)SoeEntityState.Active
                                     select new TimeTreeProjectTimeBlockDTO
                                     {
                                         ProjectTimeBlockId = b.ProjectTimeBlockId,
                                         TimeBlockDateId = b.TimeBlockDateId,
                                         EmployeeId = b.TimeBlockDate.EmployeeId,
                                         TimeCodeTransactionIds = b.TimeCodeTransaction.Select(i => i.TimeCodeTransactionId).ToList(),
                                         InvoiceQuantity = b.InvoiceQuantity,
                                     }).ToList();
            }
            else
            {
                projectTimeBlocks = (from b in entities.ProjectTimeBlock
                                     where b.ActorCompanyId == ActorCompanyId &&
                                     b.TimeBlockDate.EmployeeId == b.EmployeeId &&
                                     b.TimeBlockDate.Date >= startDate &&
                                     b.TimeBlockDate.Date <= stopDate &&
                                     b.State == (int)SoeEntityState.Active
                                     select new TimeTreeProjectTimeBlockDTO
                                     {
                                         ProjectTimeBlockId = b.ProjectTimeBlockId,
                                         TimeBlockDateId = b.TimeBlockDateId,
                                         EmployeeId = b.TimeBlockDate.EmployeeId,
                                         TimeCodeTransactionIds = b.TimeCodeTransaction.Select(i => i.TimeCodeTransactionId).ToList(),
                                         InvoiceQuantity = b.InvoiceQuantity,
                                     }).ToList();

            }

            return projectTimeBlocks.Where(b => !b.TimeCodeTransactionIds.IsNullOrEmpty()).GroupBy(b => b.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
        }

        private (List<EmployeeDatesDTO>, Dictionary<int, List<TimeBlockDate>>) GetAttestPeriodStamping(CompEntities entities, GetAttestEmployeePeriodsInput input, List<TimeTreeEmployeeInfoDTO> employeeInfos)
        {
            List<int> employeeIdsStamping = employeeInfos?.Where(e => !e.AutogenTimeblocks).Select(i => i.EmployeeId).ToList();
            if (employeeIdsStamping.IsNullOrEmpty())
                return (new List<EmployeeDatesDTO>(), new Dictionary<int, List<TimeBlockDate>>());

            DateTime stopDate = CalendarUtility.GetEarliestDate(input.StopDate, DateTime.Now).Date;
            List<EmployeeDatesDTO> timeStampEntryDatesByEmployee = GetEmployeeDataInBatches(GetDataInBatchesModel.Create(entities, input.ActorCompanyId, employeeIdsStamping, input.StartDate, stopDate), TimeStampManager.GetTimeStampEntryDatesByEmployee);

            List<int> stampingStatusIds = new List<int>
            {
                (int)TermGroup_TimeBlockDateStampingStatus.FirstStampIsNotIn,
                (int)TermGroup_TimeBlockDateStampingStatus.OddNumberOfStamps,
                (int)TermGroup_TimeBlockDateStampingStatus.InvalidSequenceOfStamps,
                (int)TermGroup_TimeBlockDateStampingStatus.StampsWithInvalidType,
                (int)TermGroup_TimeBlockDateStampingStatus.AttestedDay,
                (int)TermGroup_TimeBlockDateStampingStatus.InvalidDoubleStamp,
            };
            List<TimeBlockDate> timeBlockDatesInvalid = TimeBlockManager.GetTimeBlockDates(entities, input.ActorCompanyId, employeeIdsStamping, input.StartDate, stopDate, stampingStatusIds);

            return (
                timeStampEntryDatesByEmployee,
                timeBlockDatesInvalid?.GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList())
                );
        }

        #endregion

        #region Employee

        public List<AttestEmployeeDayDTO> GetAttestEmployeeDays(GetAttestEmployeeInput input)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAttestEmployeeDays(entities, input);
        }

        public List<AttestEmployeeDayDTO> GetAttestEmployeeDays(CompEntities entities, GetAttestEmployeeInput input)
        {
            List<AttestEmployeeDayDTO> attestEmployeeDays = new List<AttestEmployeeDayDTO>();

            if (string.IsNullOrEmpty(input.CacheKeyToUse) || !Guid.TryParse(input.CacheKeyToUse, out Guid cacheKey))
                cacheKey = Guid.Empty;

            try
            {
                #region Decide date range

                bool loadTimeWorkReductionTransactionForWeek = input.StartDate == input.StopDate && input.HasDayFilter && base.UseTimeWorkReductionFromCache(entities, input.ActorCompanyId);
                if (loadTimeWorkReductionTransactionForWeek)
                {
                    var week = CalendarUtility.GetWeek(input.StartDate);
                    if (TimeTransactionManager.HasEmployeeEarningTimePayrollTransactions(entities, input.EmployeeId, week.DateFrom, week.DateTo))
                        input.UpdateDates(week.DateFrom, week.DateTo);
                }

                #endregion

                #region Load Employees

                var (employee, _, accountRepository, categoryRepository) = LoadAttestDayEmployee(entities, input);
                if (employee == null)
                    return attestEmployeeDays;

                List<EmployeeSchedule> employeeSchedules = TimeScheduleManager.GetEmployeeSchedulesForEmployee(entities, employee.EmployeeId, input.StartDate, input.StopDate, input.ActorCompanyId, true);
                if (!input.IsScenario && employeeSchedules.IsNullOrEmpty())
                    return attestEmployeeDays;

                #endregion

                #region Load company lookups

                var accountDims = base.GetAccountDimsFromCache(entities, CacheConfig.Company(input.ActorCompanyId, cacheKey));
                var shiftTypes = base.GetShiftTypesFromCache(entities, CacheConfig.Company(input.ActorCompanyId, cacheKey));
                var timeScheduleTypes = base.GetTimeScheduleTypesFromCache(entities, CacheConfig.Company(input.ActorCompanyId, cacheKey));
                var timeDeviationCauses = base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(input.ActorCompanyId, cacheKey));
                var timeDeviationCausesOvertime = timeDeviationCauses.GetOvertimeDeviationCauseIds();

                #endregion

                #region Load settings

                TimePeriod timePeriod = input.TimePeriodId.HasValue ? TimePeriodManager.GetTimePeriod(entities, input.TimePeriodId.Value, input.ActorCompanyId) : null;
                bool isExtraPeriod = timePeriod.IsExtraPeriod();
                bool isMySelf = IsAttestEmployeeMySelf(entities, input, employee);
                bool usePayroll = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, input.ActorCompanyId, 0);
                bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, input.ActorCompanyId);
                bool useGrossNetTime = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing, 0, input.ActorCompanyId, 0);
                bool loadDaysOutOfRange = input.IsWeb && input.TimePeriodId.HasValue && input.TimePeriodId.Value > 0;
                bool loadUnhandledShiftChanges = input.IsWeb && input.DoLoad(InputLoadType.UnhandledShiftChanges);
                bool loadUnhandledSickDays = loadUnhandledShiftChanges && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningSetShiftAsExtra, 0, input.ActorCompanyId, 0);
                bool doInactivateLending = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeSchedulePlanningInactivateLending, 0, input.ActorCompanyId, 0);
                bool loadUnhandledOvertimeDays = loadUnhandledShiftChanges && !timeDeviationCausesOvertime.IsNullOrEmpty();

                #endregion

                #region Decide loadings

                SetOptionalParameters(entities, input);

                bool loadGrossNetCost = useGrossNetTime && input.DoLoad(InputLoadType.GrossNetCost);
                bool loadShifts = input.DoLoad(InputLoadType.Shifts);
                bool loadDayTypes = input.DoLoad(InputLoadType.DayType);
                bool loadPresenceAbsenceDetails = input.DoLoad(InputLoadType.PresenceAbsenceDetails);
                bool loadTemplateSchedule = input.DoLoad(InputLoadType.TemplateSchedule);
                bool loadTimeBlocks = input.DoLoad(InputLoadType.TimeBlocks);
                bool loadTimeCodeTransactions = input.DoLoad(InputLoadType.TimeCodeTransactions) || input.MobileMyTime;
                bool loadTimeStamps = input.DoLoad(InputLoadType.TimeStamps);
                bool loadProjectTimeBlocks = input.DoLoad(InputLoadType.ProjectTimeBlocks);
                bool loadTimePayrollTransactions = input.DoLoad(InputLoadType.TimePayrollTransactions);
                bool loadAttestTransitionLogs = input.DoLoad(InputLoadType.AttestTransitionLog);
                bool loadTemplateBlockAccounts = input.EmployeeSelectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount || !input.SelectionValidAccountInternals.IsNullOrEmpty() || (input.HasFilterAccountIds && !useAccountHierarchy);
                bool loadTimePayrollTransactionAccounts = input.IsWeb || input.EmployeeSelectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimePayrollTransactionAccount || !input.SelectionValidAccountInternals.IsNullOrEmpty();
                bool loadTimeScheduleTransactions = usePayroll && input.IsWeb && !isExtraPeriod;
                bool loadTimeScheduleTransactionAccountStds = usePayroll && input.IsWeb && !isExtraPeriod;
                bool loadTimeScheduleTransactionAccountInternals = usePayroll && (input.IsWeb || input.EmployeeSelectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimePayrollTransactionAccount || !input.SelectionValidAccountInternals.IsNullOrEmpty());
                bool loadTimeRules = loadTimeCodeTransactions;
                bool loadAttestStateInitialPayroll = loadTimeBlocks || loadTimeStamps;
                bool loadEmployeeChilds = loadTimePayrollTransactions;
                bool loadAccountInternals = useAccountHierarchy;
                bool loadHierarchySettingAccount = useAccountHierarchy;
                bool loadValidDatesByScheduleAndTransactionsByDate = useAccountHierarchy && !isMySelf && !input.IsScenario;
                bool hasSalaryPermission =
                    (!isMySelf && FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId, entities: entities)) ||
                    (isMySelf && FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId, entities: entities));

                #endregion

                #region Load mandatory data

                var (validDates, dateEmployeeScheduleDict, templateHeadScheduleDict) = LoadEmployeeDaysValidByEmployeeAuth(entities, input, employee, employeeSchedules, accountRepository, categoryRepository, useAccountHierarchy, isMySelf);

                var templateBlocksEmployee = GetAttestEmployeeSchedule(entities, input, shiftTypes, timeScheduleTypes).ToList();
                var scheduleBlocksEmployee = templateBlocksEmployee.Where(b => b.IsSchedule() || (input.DoGetOnDuty && b.IsOnDuty())).ToList();
                var scheduleBlocksEmployeeAndDate = scheduleBlocksEmployee.GroupBy(i => i.Date).ToDictionary(k => k.Key.Value, v => v.ToList());
                var standbyBlocksEmployeeAndEmployee = templateBlocksEmployee.Where(b => b.IsStandby()).GroupBy(i => i.Date).ToDictionary(k => k.Key.Value, v => v.ToList());
                var scheduleTypeFactorsEmployee = TimeScheduleManager.GetTimeScheduleTypeFactorsWithinShiftsDict(entities, input.ActorCompanyId, input.EmployeeId, input.StartDate, input.StopDate, scheduleBlocksEmployee, timeScheduleTypes);
                var scheduleSwapShiftRequestRows = TimeScheduleManager.GetTimeScheduleSwapRequestRows(entities, employee.EmployeeId, input.StartDate, input.StopDate);
                var timeBlockDates = input.TimeBlockDatesForEmployee?.Filter(input.EmployeeId, validDates) ?? TimeBlockManager.GetTimeBlockDates(entities, input.EmployeeId, validDates);
                var timeBlockdateIds = timeBlockDates.Select(s => s.TimeBlockDateId).ToList();
                var timeBlocks = TimeBlockManager.GetTimeBlocks(entities, input.EmployeeId, timeBlockdateIds, loadDeviationAccounts: input.DoLoad(InputLoadType.TimeBlocks), loadTimeCode: true).ExcludeZero();
                var timeBlocksEmployeeByTimeBlockDate = timeBlocks.GroupBy(t => t.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());
                var timePayrollTransactionsEmployee = GetAttestEmployeeTransactions(entities, input, timePeriod, isExtraPeriod);
                var timePayrollTransactionsEmployeeByTimeBlockDate = timePayrollTransactionsEmployee.GroupBy(t => t.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());
                var attestStateSalaryExportPayrollMin = AttestManager.GetAttestState(entities, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, input.UserId, input.ActorCompanyId, 0))?.ToDTO();

                #endregion

                #region Load optional data

                var templateBlockAccountsEmployee = loadTemplateBlockAccounts ? entities.GetTimeScheduleTemplateBlockAccountsForEmployee(input.EmployeeId, input.StartDate, input.StopDate).ToList() : new List<GetTimeScheduleTemplateBlockAccountsForEmployeeResult>();
                var dayTypesDict = loadDayTypes ? TimeEngineManager(input.ActorCompanyId, input.UserId).CalculateDayTypesForEmployee(validDates, input.EmployeeId, false, input.Holidays) : null;
                var timeStampEntrys = loadTimeStamps ? TimeStampManager.GetTimeStampEntries(entities, timeBlockdateIds, true) : null;
                var timeCodeTransactionsEmployee = loadTimeCodeTransactions ? TimeTransactionManager.GetTimeCodeTransactions(entities, timeBlockdateIds, true) : null;
                var timeCodeTransactionsEmployeeByTimeBlockDate = timeCodeTransactionsEmployee?.GroupBy(t => t.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());
                var timeRulesEmployee = loadTimeRules ? LoadEmployeeDayTimeRules(entities, timeCodeTransactionsEmployee.GetTimeRuleIds()) : null;
                var timePayrollTransactionAccountStdsEmployee = loadTimePayrollTransactionAccounts ? AccountManager.GetAccountStds(entities, input.ActorCompanyId, timePayrollTransactionsEmployee.Select(i => i.AccountId).Distinct().ToList(), false) : null;
                var timePayrollTransactionAccountInternalsEmployee = loadTimePayrollTransactionAccounts ? TimeTransactionManager.GetTimePayrollTransactionAccountsForEmployee(entities, input.StartDate, input.StopDate, input.TimePeriodId, input.EmployeeId, isExtraPeriod) : null;
                var timeScheduleTransactionsEmployee = loadTimeScheduleTransactions ? TimeTransactionManager.GetTimePayrollScheduleTransactionsForEmployee(entities, null, input.StartDate, input.StopDate, input.TimePeriodId, input.EmployeeId).ToList() : null;
                var timeScheduleTransactionsEmployeeByTimeBlockDate = timeScheduleTransactionsEmployee?.GroupBy(t => t.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());
                var timeScheduleTransactionAccountStdsEmployee = loadTimeScheduleTransactionAccountStds ? AccountManager.GetAccountStds(entities, input.ActorCompanyId, timeScheduleTransactionsEmployee.Select(i => i.AccountId).Distinct().ToList(), false) : null;
                var scheduleTransactionAccountInternalsEmployee = loadTimeScheduleTransactionAccountInternals ? TimeTransactionManager.GetTimePayrollScheduleTransactionAccountsForEmployee(entities, null, input.StartDate, input.StopDate, input.TimePeriodId, input.EmployeeId).ToList() : null;
                var attestStateIdIntitialPayroll = loadAttestStateInitialPayroll ? AttestManager.GetInitialAttestStateId(input.ActorCompanyId, TermGroup_AttestEntity.PayrollTime) : (int?)null;
                var attestTransitionLogsEmployee = loadAttestTransitionLogs ? entities.GetAttestTransitionLogsForEmployee(input.EmployeeId, input.StartDate, input.StopDate).ToList() : new List<GetAttestTransitionLogsForEmployeeResult>();
                var projectTimeBlocks = loadProjectTimeBlocks ? ProjectManager.GetProjectTimeBlockDTOs(input.StartDate, input.StopDate, new List<int> { input.EmployeeId }, null, null) : null;
                var payrollImportTransactions = GetPayrollImportTransactions(entities, base.ActorCompanyId, input.EmployeeId, input.StartDate, input.StopDate);
                var payrollImportErrors = payrollImportTransactions != null ? GetPayrollImportErrors(entities, base.ActorCompanyId, input.EmployeeId.ObjToList(), input.StartDate, input.StopDate, payrollImportTransactions) : null;
                var employeeChilds = loadEmployeeChilds ? EmployeeManager.GetEmployeeChildsDict(input.EmployeeId, false) : null;
                var accountInternals = loadAccountInternals ? base.GetAccountInternalsFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey)) : null;
                var hierarchySettingAccount = loadHierarchySettingAccount ? AccountManager.GetAccountHierarchySettingAccount(entities, useAccountHierarchy) : null;
                var validDatesByScheduleAndTransactionsByDate = loadValidDatesByScheduleAndTransactionsByDate ? LoadEmployeeDaysValidByScheduleAndTransactions(entities, input, ref validDates, employee, templateBlocksEmployee, timePayrollTransactionsEmployee, timePayrollTransactionAccountInternalsEmployee, accountInternals, useAccountHierarchy) : null;
                int? standardTimeDeviationCauseId = standbyBlocksEmployeeAndEmployee.Any() ? TimeDeviationCauseManager.GetTimeDeviationCauseIdFromPrio(employee, input.StartDate) : (int?)null;

                #endregion

                #region Process days

                Dictionary<DateTime, EmployeeGroupDTO> dateEmployeeGroup = new Dictionary<DateTime, EmployeeGroupDTO>();
                bool allowShiftTypeIdNullValues = false;
                int dayCounter = 1;

                foreach (DateTime date in validDates)
                {
                    #region Day

                    try
                    {
                        #region Filter data

                        EmployeeSchedule employeeScheduleForDay = dateEmployeeScheduleDict.GetValue(date);
                        if (!input.IsScenario && employeeScheduleForDay == null)
                            continue;

                        List<TimeTreeScheduleBlockDTO> standbyBlocksOnDay = standbyBlocksEmployeeAndEmployee.GetList(date);
                        List<TimeTreeScheduleBlockDTO> scheduleBlocksDay = scheduleBlocksEmployeeAndDate.GetList(date).Where(W => W.StartTime != W.StopTime && (W.IsSchedule() || (input.DoGetOnDuty && W.IsOnDuty()))).ToList();
                        if (input.HasFilterShiftTypeIds)
                        {
                            allowShiftTypeIdNullValues = input.IncludeDeviationOnZeroDayFromScenario && scheduleBlocksDay.Any(s => !s.TimeDeviationCauseId.IsNullOrEmpty() && s.IsZeroBlock);
                            scheduleBlocksDay = scheduleBlocksDay.FilterOnScheduleType(input.FilterShiftTypeIds, employee.Hidden, allowShiftTypeIdNullValues);
                        }
                        TimeScheduleTemplatePeriod templatePeriod = TimeScheduleManager.GetTimeScheduleTemplatePeriod(date, employeeScheduleForDay, scheduleBlocksDay.Concat(standbyBlocksOnDay));
                        if (!input.SelectionValidAccountInternals.IsNullOrEmpty())
                            scheduleBlocksDay = FilterBlocksOnEmployeeAndDayOnAccountingType(input.EmployeeSelectionAccountingType, scheduleBlocksDay, templateBlockAccountsEmployee, input.SelectionValidAccountInternals);
                        EmployeeGroupDTO employeeGroup = employee.GetEmployeeGroup(date, employeeGroups: input.EmployeeGroups)?.ToDTO();
                        dateEmployeeGroup.Add(date, employeeGroup);
                        HolidayDTO holiday = input?.Holidays?.FirstOrDefault(h => h.Date == date);
                        DayTypeDTO dayType = dayTypesDict.GetValue(date)?.ToDTO();
                        TimeBlockDate timeBlockDate = timeBlockDates.FirstOrDefault(i => i.Date == date);
                        List<GetTemplateSchedule_Result> templateScheduleDay = loadTemplateSchedule ? templateHeadScheduleDict.GetList(employeeScheduleForDay.TimeScheduleTemplateHeadId) : null;

                        #endregion

                        #region Process data

                        AttestEmployeeDayDTO attestEmployeeDay = new AttestEmployeeDayDTO(input.EmployeeId, date, employeeGroup);
                        SetEmployeeDayCalendar(attestEmployeeDay, templatePeriod, employeeScheduleForDay, timeBlockDate, holiday, dayType);
                        SetEmployeeDaySchedule(attestEmployeeDay, employee, scheduleBlocksDay, scheduleTypeFactorsEmployee, scheduleSwapShiftRequestRows, timeDeviationCauses, loadGrossNetCost, loadShifts);
                        SetEmployeeDayScheduleStandby(attestEmployeeDay, standbyBlocksOnDay, timeDeviationCauses, loadShifts, loadPresenceAbsenceDetails);
                        SetEmployeeDayScheduleTemplate(attestEmployeeDay, templatePeriod, templateScheduleDay);

                        var timeBlocksDay = new List<TimeBlock>();
                        if (!input.IsScenario && timeBlockDate != null)
                        {
                            var timeCodeTransactionsDay = timeCodeTransactionsEmployeeByTimeBlockDate?
                                .GetList(timeBlockDate.TimeBlockDateId)
                                .OrderBy(t => t.IsReversed)
                                .ThenBy(t => t.Start)
                                .ThenBy(t => t.Stop)
                                .ThenBy(t => t.TimeRuleId).ToList() ?? new List<TimeCodeTransaction>();

                            var timePayrollTransactionsDay = timePayrollTransactionsEmployeeByTimeBlockDate?
                                .GetList(timeBlockDate.TimeBlockDateId)
                                .OrderBy(t => t.IsReversed)
                                .ThenBy(t => t.StartTime)
                                .ThenBy(t => t.StopTime)
                                .ThenByDescending(t => t.AttestStateInitial)
                                .ThenBy(t => t.AttestStateId)
                                .ToList() ?? new List<GetTimePayrollTransactionsForEmployee_Result>();

                            var timePayrollScheduleTransactionDay = timeScheduleTransactionsEmployeeByTimeBlockDate?
                                .GetList(timeBlockDate.TimeBlockDateId)
                                .Where(t => t.Type == (int)SoeTimePayrollScheduleTransactionType.Absence)
                                .ToList() ?? new List<GetTimePayrollScheduleTransactionsForEmployee_Result>();

                            timeBlocksDay = timeBlocksEmployeeByTimeBlockDate?
                                .GetList(timeBlockDate.TimeBlockDateId)
                                .Where(t => templatePeriod == null || t.TimeScheduleTemplatePeriodId == templatePeriod.TimeScheduleTemplatePeriodId)
                                .OrderBy(tb => tb.StartTime)
                                .ToList() ?? new List<TimeBlock>();

                            if (!input.SelectionValidAccountInternals.IsNullOrEmpty())
                            {
                                timePayrollTransactionsDay = FilterTimePayrollTransactionsOnAccountingType(timePayrollTransactionsDay, timePayrollTransactionAccountInternalsEmployee, input.SelectionValidAccountInternals);
                                timePayrollScheduleTransactionDay = FilterTimePayrollScheduleTransactionsOnccountingType(input.EmployeeSelectionAccountingType, timePayrollScheduleTransactionDay, scheduleTransactionAccountInternalsEmployee, input.SelectionValidAccountInternals);
                            }

                            SetEmployeeDayTimeCodeTransactions(attestEmployeeDay, input, timeCodeTransactionsDay, timeRulesEmployee);
                            SetEmployeeDayTimePayrollTransactions(attestEmployeeDay, input, timePayrollTransactionsDay, timePayrollTransactionAccountStdsEmployee, timePayrollTransactionAccountInternalsEmployee, attestTransitionLogsEmployee, employeeChilds, hasSalaryPermission);
                            if (isExtraPeriod && !attestEmployeeDay.HasTransactions)
                                continue;

                            SetEmployeeDayTimeScheduleTransaction(attestEmployeeDay, input, timePayrollTransactionsDay, timePayrollScheduleTransactionDay, scheduleTransactionAccountInternalsEmployee, timeScheduleTransactionAccountStdsEmployee, hasSalaryPermission);
                            SetEmployeeDayTimeBlocks(attestEmployeeDay, input, employeeGroup, timeBlockDate, timeBlocksDay, timePayrollTransactionsDay, timeDeviationCauses, accountDims, attestStateIdIntitialPayroll, standardTimeDeviationCauseId);
                            SetEmployeeDayTimeStampEntrys(attestEmployeeDay, entities, input, timeBlockDates, timeStampEntrys, timeScheduleTypes, loadTimeStamps);
                            SetEmployeeDayPayrollImportWarnings(attestEmployeeDay, employee, payrollImportErrors);
                            SetEmployeeDayPayrollImportTransactions(attestEmployeeDay, employee, payrollImportTransactions);
                        }
                        SetEmployeeDaysProjectTimeBlocks(input, attestEmployeeDay, projectTimeBlocks);
                        if (!doInactivateLending)
                            SetEmployeeDayLended(attestEmployeeDay, validDatesByScheduleAndTransactionsByDate);
                        SetEmployeeDayTotals(attestEmployeeDay, timeBlockDate, employeeGroup, scheduleBlocksDay, timeBlocksDay, timeDeviationCausesOvertime, attestStateSalaryExportPayrollMin, hierarchySettingAccount);

                        attestEmployeeDays.Add(attestEmployeeDay);

                        #endregion
                    }
                    finally
                    {
                        dayCounter++;
                    }

                    #endregion
                }

                SetEmployeeWeekTotals(attestEmployeeDays);
                if (loadUnhandledShiftChanges)
                    SetEmployeeUnhandledShiftChanges(loadUnhandledOvertimeDays, loadUnhandledSickDays, attestEmployeeDays, timeBlockDates, dateEmployeeGroup);
                if (loadDaysOutOfRange)
                    attestEmployeeDays.AddRange(LoadDaysOutOfRange(entities, input, dateEmployeeScheduleDict, timeBlockDates, timePayrollTransactionsEmployee, timePayrollTransactionAccountStdsEmployee, timePayrollTransactionAccountInternalsEmployee, attestTransitionLogsEmployee));
                if (loadGrossNetCost)
                    SetEmployeeDayGrossNetCost(attestEmployeeDays, input);

                #endregion
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }
            finally
            {
                entities.Connection.Close();
            }

            return attestEmployeeDays.OrderBy(i => i.Date).ThenBy(i => i.PresenceStartTime).ThenBy(i => i.TimeScheduleTemplatePeriodId).ToList();
        }

        private List<GetTimePayrollTransactionsForEmployee_Result> GetAttestEmployeeTransactions(CompEntities entities, GetAttestEmployeeInput input, TimePeriod timePeriod, bool isExtraPeriod)
        {
            var transactions = TimeTransactionManager
                .GetTimePayrollTransactionItemsForEmployee(entities, input.EmployeeId, timePeriod?.TimePeriodId, input.StartDate, input.StopDate, isExtraPeriod)
                .Where(t => !t.PayrollStartValueRowId.HasValue)
                .ToList();

            if (input.IsModeTimeAttest)
                transactions = transactions.Where(t => !t.IsExcludedInTime()).ToList();
            else if (input.IsModePayrollCalculation)
                transactions = transactions.Where(t => !t.IsExcludedInPayroll()).ToList();

            return transactions;
        }

        public AttestEmployeeOverviewDTO GetAttestEmployeeOverview(GetAttestEmployeeInput input)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAttestEmployeeOverview(entities, input);
        }

        public AttestEmployeeOverviewDTO GetAttestEmployeeOverview(CompEntities entities, GetAttestEmployeeInput input)
        {
            var overview = new AttestEmployeeOverviewDTO()
            {
                EmployeeId = input.EmployeeId,
            };

            var employeeDays = GetAttestEmployeeDays(entities, input);
            if (!employeeDays.IsNullOrEmpty())
            {
                int mobileResultingAttestStateId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.MobileTimeAttestResultingAttestStatus, 0, base.ActorCompanyId, 0);
                AttestState mobileResultingAttestState = AttestManager.GetAttestState(entities, mobileResultingAttestStateId);
                AttestState initialAttestState = AttestManager.GetInitialAttestState(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime);

                foreach (var employeeDay in employeeDays)
                {
                    #region Day

                    if (!employeeDay.IsPrel && employeeDay.PresenceInsideScheduleTime.HasValue)
                    {
                        overview.PresenceInsideScheduleTime = overview.PresenceInsideScheduleTime.Add(employeeDay.PresenceInsideScheduleTime.Value);
                        overview.PresenceInsideScheduleTimeDetails = overview.PresenceInsideScheduleTimeDetails.AddTimes(employeeDay.PresenceInsideScheduleTimeDetails);
                    }
                    if (!employeeDay.IsPrel && employeeDay.PresenceOutsideScheduleTime.HasValue)
                    {
                        overview.PresenceOutsideScheduleTime = overview.PresenceOutsideScheduleTime.Add(employeeDay.PresenceOutsideScheduleTime.Value);
                        overview.PresenceOutsideScheduleTimeDetails = overview.PresenceOutsideScheduleTimeDetails.AddTimes(employeeDay.PresenceOutsideScheduleTimeDetails);
                    }
                    if (!employeeDay.IsPrel && employeeDay.AbsenceTime.HasValue)
                    {
                        overview.AbsenceTime = overview.AbsenceTime.Add(employeeDay.AbsenceTime.Value);
                        overview.AbsenceTimeDetails = overview.AbsenceTimeDetails.AddTimes(employeeDay.AbsenceTimeDetails);
                    }
                    if (!employeeDay.IsPrel && employeeDay.StandbyTime.HasValue)
                    {
                        overview.StandbyTime = overview.StandbyTime.Add(employeeDay.StandbyTime.Value);
                        overview.StandbyTimeDetails = overview.StandbyTimeDetails.AddTimes(employeeDay.StandbyTimeDetails);
                    }
                    if (employeeDay.HasExpense)
                    {
                        overview.SumExpeseRows += employeeDay.SumExpenseRows;
                        overview.SumExpenseAmount += employeeDay.SumExpenseAmount;
                    }
                    if (employeeDay.HasWarnings)
                    {
                        overview.Warnings.AddRange(employeeDay.Warnings);
                    }
                    if (!employeeDay.IsPrel && mobileResultingAttestState != null && !employeeDay.AttestStates.IsNullOrEmpty())
                    {
                        if (employeeDay.AttestStates.Any(i => i.Sort < mobileResultingAttestState.Sort))
                            overview.NrOfDaysWithInitialAttest++;
                        else if (employeeDay.AttestStates.Any(i => i.Sort >= mobileResultingAttestState.Sort))
                            overview.NrOfDaysWithResultingAttest++;
                    }

                    #endregion
                }

                overview.ExpenseDetails = new Dictionary<string, string>();
                foreach (var expensesByName in employeeDays.Where(day => day.Expenses != null).SelectMany(day => day.Expenses).GroupBy(e => e.Name))
                    overview.ExpenseDetails.Add(expensesByName.Key, $"{expensesByName.Count()} / {expensesByName.Sum(i => i.Amount)}");

                //Warnings
                if (overview.Warnings.Any())
                    overview.Messages.Add(new AttestEmployeeOverviewMessage(string.Format(GetText(7589, "{0} dagar behöver kontrolleras"), overview.Warnings.Count), true));

                //Initial atteststates
                if (overview.NrOfDaysWithInitialAttest > 0)
                    overview.Messages.Add(new AttestEmployeeOverviewMessage(string.Format(GetText(11920, "{0} dagar behöver klarmarkeras"), overview.NrOfDaysWithInitialAttest), initialAttestState.Color));

                //Resulting atteststates
                if (overview.NrOfDaysWithResultingAttest > 0 && overview.NrOfDaysWithInitialAttest > 0)
                {
                    overview.Messages.Add(new AttestEmployeeOverviewMessage(string.Format(GetText(11921, "{0} dagar är klarmarkerade"), overview.NrOfDaysWithResultingAttest), mobileResultingAttestState.Color));
                }
                else if (overview.NrOfDaysWithResultingAttest > 0)
                {
                    overview.Messages.Add(new AttestEmployeeOverviewMessage(GetText(11922, "Alla dagar är klarmarkerade"), mobileResultingAttestState.Color));
                    overview.IsPeriodAttested = true;
                }

                //Default
                if (overview.NrOfDaysWithInitialAttest == 0 && overview.NrOfDaysWithResultingAttest == 0)
                    overview.Messages.Add(new AttestEmployeeOverviewMessage(GetText(11923, "Perioden innehåller inga dagar med tid"), mobileResultingAttestState?.Color ?? "#FFFFFF"));
            }

            return overview;
        }

        public List<AttestEmployeeAdditionDeductionDTO> GetAttestEmployeeAdditionDeductions(int employeeId, DateTime dateFrom, DateTime dateTo, int? timePeriodId, bool isMySelf = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.TimeCodeTransaction.NoTracking();
            return GetAttestEmployeeAdditionDeductions(entities, employeeId, dateFrom, dateTo, timePeriodId, isMySelf);
        }

        public List<AttestEmployeeAdditionDeductionDTO> GetAttestEmployeeAdditionDeductions(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo, int? timePeriodId, bool isMySelf = false)
        {
            List<AttestEmployeeAdditionDeductionDTO> dtos = new List<AttestEmployeeAdditionDeductionDTO>();

            #region Prereq

            dateTo = CalendarUtility.GetEndOfDay(dateTo);

            List<ExpenseRow> expenseRows = GetQuery().Where(row => (row.ExpenseHead.Start >= dateFrom && row.ExpenseHead.Start <= dateTo) && (!timePeriodId.HasValue || !row.TimePeriodId.HasValue)).ToList();
            if (timePeriodId.HasValue && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, ActorCompanyId, 0))
                expenseRows.AddRange(GetQuery().Where(row => row.TimePeriodId == timePeriodId).ToList());

            if (!expenseRows.Any())
                return dtos;

            IQueryable<ExpenseRow> GetQuery()
            {
                return from row in entities.ExpenseRow
                        .Include("ExpenseHead")
                        .Include("TimeCodeTransaction.TimeInvoiceTransaction")
                        .Include("TimeCodeTransaction.TimePayrollTransaction")
                       where row.ActorCompanyId == ActorCompanyId &&
                       row.EmployeeId == employeeId &&
                       row.State == (int)SoeEntityState.Active &&
                       row.TimeCode.Type == (int)SoeTimeCodeType.AdditionDeduction
                       select row;
            }

            List<TimeCodeTransaction> timeCodeTransactions = expenseRows.Select(s => s.TimeCodeTransaction).ToList();
            if (!timeCodeTransactions.Any())
                return dtos;

            List<AttestState> attestStates = AttestManager.GetAttestStates(entities, ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
            int attestStateIdSalaryExportPayrollMinimumAttestStatus = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.SalaryExportPayrollMinimumAttestStatus, base.UserId, base.ActorCompanyId, 0);
            AttestState attestStateSalaryExportPayrollMinimumAttestStatus = attestStates.FirstOrDefault(i => i.AttestStateId == attestStateIdSalaryExportPayrollMinimumAttestStatus);
            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(entities, base.ActorCompanyId);

            #endregion

            #region Perform

            List<TimeCodeAdditionDeduction> timeCodes = new List<TimeCodeAdditionDeduction>();
            List<InvoiceProduct> invoiceProducts = new List<InvoiceProduct>();
            List<Account> accountStds = new List<Account>();

            foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactions)
            {
                #region Prereq

                TimeCodeAdditionDeduction timeCode = timeCodes.FirstOrDefault(i => i.TimeCodeId == timeCodeTransaction.TimeCodeId);
                if (timeCode == null)
                {
                    timeCode = TimeCodeManager.GetTimeCode<TimeCodeAdditionDeduction>(entities, ActorCompanyId, timeCodeTransaction.TimeCodeId);
                    if (timeCode != null)
                        timeCodes.Add(timeCode);
                }
                if (timeCode == null)
                    continue;

                if (isMySelf && timeCode.HideForEmployee)
                    continue;

                List<int> transactionsAttestStateIds = new List<int>();

                List<TimePayrollTransaction> timePayrollTransactions = timeCodeTransaction.TimePayrollTransaction.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                if (timePayrollTransactions.Count > 0)
                    transactionsAttestStateIds.AddRange(timePayrollTransactions.Select(i => i.AttestStateId).ToList());

                List<TimeInvoiceTransaction> timeInvoiceTransactions = timeCodeTransaction.TimeInvoiceTransaction.Where(i => i.State == (int)SoeEntityState.Active).ToList();
                if (timeInvoiceTransactions.Count > 0)
                    transactionsAttestStateIds.AddRange(timeInvoiceTransactions.Where(i => i.AttestStateId.HasValue).Select(i => i.AttestStateId.Value).ToList());

                List<AttestState> transactionsAttestStates = new List<AttestState>();
                foreach (int transactionsAttestStateId in transactionsAttestStateIds.Distinct())
                {
                    AttestState transactionAttestState = attestStates.FirstOrDefault(i => i.AttestStateId == transactionsAttestStateId);
                    if (transactionAttestState != null)
                        transactionsAttestStates.Add(transactionAttestState);
                }

                transactionsAttestStates.GetAttestStateLowest(out int? attestStateId, out _, out string attestStateLowestColor, out string attestStateLowestName);

                bool isAttested = false;
                AttestState attestState = attestStates.FirstOrDefault(i => i.AttestStateId == attestStateId);
                if (attestState != null && attestStateSalaryExportPayrollMinimumAttestStatus != null && (attestStateSalaryExportPayrollMinimumAttestStatus.AttestStateId == attestState.AttestStateId || attestState.Sort > attestStateSalaryExportPayrollMinimumAttestStatus.Sort))
                    isAttested = true;

                bool isReadOnly = attestState != null && !attestState.Initial;
                bool isSpecifiedUnitPrice = timePayrollTransactions.Any(i => i.IsSpecifiedUnitPrice);

                int? projectId = timeCodeTransaction.ExpenseRow.First().ProjectId;
                int? customerInvoiceRowId = timeCodeTransaction.ExpenseRow.First().CustomerInvoiceRowId;
                int? customerInvoiceId = timeCodeTransaction.ExpenseRow.First().CustomerInvoiceId;
                int? timePeriodOnRowId = timeCodeTransaction.ExpenseRow.First().TimePeriodId;

                string invoiceNumber = string.Empty;
                string projectNumber = string.Empty;
                string projectName = string.Empty;
                string customerNumber = string.Empty;
                string customerName = string.Empty;
                bool isPriceListInclVat = false;

                if (projectId.HasValue)
                {
                    var project = entities.Project.FirstOrDefault(f => f.ProjectId == projectId.Value);

                    if (project != null)
                    {
                        projectNumber = project.Number;
                        projectName = project.Name;
                    }
                }

                if (customerInvoiceId.HasValue)
                {
                    Invoice invoice = entities.Invoice.Include("Customer").FirstOrDefault(f => f.InvoiceId == customerInvoiceId.Value);
                    if (invoice is CustomerInvoice customerInvoice)
                    {
                        invoiceNumber = customerInvoice.InvoiceNr;

                        if (customerInvoice.PriceListTypeId != null)
                        {
                            if (!customerInvoice.PriceListTypeReference.IsLoaded)
                                customerInvoice.PriceListTypeReference.Load();

                            isPriceListInclVat = customerInvoice.PriceListType?.InclusiveVat ?? false;
                        }

                        if (customerInvoice.Customer != null)
                            customerName = customerInvoice.Customer.Name;
                    }
                }

                #endregion

                #region TimeCodeTransaction -> AttestEmployeeAdditionDeductionDTO

                var dto = new AttestEmployeeAdditionDeductionDTO()
                {
                    ExpenseRowId = timeCodeTransaction.ExpenseRow.First().ExpenseRowId,
                    TimeCodeTransactionId = timeCodeTransaction.TimeCodeTransactionId,
                    TimeCodeId = timeCode.TimeCodeId,
                    TimeCodeName = timeCode.Name,
                    TimeCodeType = (SoeTimeCodeType)timeCode.Type,
                    TimeCodeExpenseType = (TermGroup_ExpenseType)timeCode.ExpenseType,
                    TimeCodeComment = timeCode.Comment,
                    TimeCodeStopAtDateStart = timeCode.StopAtDateStart,
                    TimeCodeStopAtDateStop = timeCode.StopAtDateStop,
                    TimeCodeStopAtPrice = timeCode.StopAtPrice,
                    TimeCodeStopAtVat = timeCode.StopAtVat,
                    TimeCodeStopAtAccounting = timeCode.StopAtAccounting,
                    TimeCodeStopAtComment = timeCode.StopAtComment,
                    TimeCodeCommentMandatory = timeCode.CommentMandatory,
                    AttestStateId = attestStateId,
                    AttestStateName = attestStateLowestName,
                    AttestStateColor = attestStateLowestColor,
                    IsAttested = isAttested,
                    IsReadOnly = isReadOnly,
                    Start = timeCodeTransaction.Start,
                    Stop = timeCodeTransaction.Stop,
                    Quantity = timeCodeTransaction.Quantity,
                    QuantityText = timeCode.IsRegistrationTypeTime ? CalendarUtility.GetHoursAndMinutesString((int)timeCodeTransaction.Quantity, false) : timeCodeTransaction.Quantity.ToString(),
                    UnitPrice = timeCodeTransaction.UnitPrice,
                    IsSpecifiedUnitPrice = isSpecifiedUnitPrice,
                    Amount = timeCodeTransaction.Amount,
                    VatAmount = timeCodeTransaction.Vat,
                    Comment = timeCodeTransaction.Comment,
                    Accounting = timeCodeTransaction.Accounting,
                    State = (SoeEntityState)timeCodeTransaction.State,
                    RegistrationType = (TermGroup_TimeCodeRegistrationType)timeCode.RegistrationType,
                    CustomerInvoiceId = customerInvoiceId,
                    CustomerInvoiceRowId = customerInvoiceRowId,
                    ProjectId = projectId,
                    TimePeriodId = timePeriodOnRowId,
                    InvoiceNumber = invoiceNumber,
                    CustomerNumber = customerNumber,
                    CustomerName = customerName,
                    ProjectNumber = projectNumber,
                    ProjectName = projectName,
                    PriceListInclVat = isPriceListInclVat,
                    HasFiles = GeneralManager.HasDataStorageRecords(ActorCompanyId, SoeDataStorageRecordType.Expense, SoeEntityType.Expense, timeCodeTransaction.ExpenseRow.First().ExpenseRowId),
                };

                #endregion

                #region TimePayrollTransaction -> AttestEmployeeAdditionDeductionTransactionDTO

                dto.Transactions = new List<AttestEmployeeAdditionDeductionTransactionDTO>();

                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                {
                    #region Prereq

                    AttestState transactionAttestState = transactionsAttestStates.FirstOrDefault(i => i.AttestStateId == timePayrollTransaction.AttestStateId);
                    if (transactionAttestState == null)
                        continue;

                    bool transactionsIsAttested = false;
                    if (attestState != null && attestStateSalaryExportPayrollMinimumAttestStatus != null && (attestStateSalaryExportPayrollMinimumAttestStatus.AttestStateId == attestState.AttestStateId || attestState.Sort > attestStateSalaryExportPayrollMinimumAttestStatus.Sort))
                        transactionsIsAttested = true;

                    PayrollProductDTO payrollProduct = GetPayrollProductFromCache(entities, CacheConfig.Company(ActorCompanyId), timePayrollTransaction.ProductId);
                    if (payrollProduct == null)
                        continue;

                    Account accountStd = accountStds.FirstOrDefault(i => i.AccountId == timePayrollTransaction.AccountStdId);
                    if (accountStd == null)
                    {
                        accountStd = AccountManager.GetAccount(entities, base.ActorCompanyId, timePayrollTransaction.AccountStdId, onlyActive: false);
                        if (accountStd != null)
                            accountStds.Add(accountStd);
                    }
                    if (accountStd == null)
                        continue;

                    List<Account> accountInternals = new List<Account>();
                    if (!timePayrollTransaction.AccountInternal.IsLoaded)
                        timePayrollTransaction.AccountInternal.Load();
                    foreach (AccountInternal accountInternal in timePayrollTransaction.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();
                        accountInternals.Add(accountInternal.Account);
                    }

                    #endregion

                    #region AttestEmployeeAdditionDeductionTransactionDTO

                    AttestEmployeeAdditionDeductionTransactionDTO transactionDTO = new AttestEmployeeAdditionDeductionTransactionDTO()
                    {
                        TransactionType = SoeTimeTransactionType.TimePayroll,
                        Date = timeCodeTransaction.Stop.Date,
                        TransactionId = timePayrollTransaction.TimePayrollTransactionId,
                        ProductId = payrollProduct.ProductId,
                        ProductName = payrollProduct.Name,
                        AttestStateId = transactionAttestState.AttestStateId,
                        AttestStateName = transactionAttestState.Name,
                        AttestStateColor = transactionAttestState.Color,
                        IsReadOnly = !transactionAttestState.Initial,
                        IsAttested = transactionsIsAttested,
                        Quantity = timePayrollTransaction.Quantity,
                        QuantityText = timeCode.IsRegistrationTypeTime ? CalendarUtility.GetHoursAndMinutesString((int)timePayrollTransaction.Quantity, false) : timePayrollTransaction.Quantity.ToString(),
                        UnitPrice = timePayrollTransaction.UnitPrice,
                        Amount = timePayrollTransaction.Amount,
                        VatAmount = timePayrollTransaction.VatAmount,
                        Comment = timePayrollTransaction.Comment,
                        AccountingString = AccountManager.GetAccountingString(accountDims, accountStd, accountInternals, false),
                    };

                    dto.Transactions.Add(transactionDTO);

                    #endregion
                }

                #endregion

                #region TimeInvoiceTransaction -> AttestEmployeeAdditionDeductionTransactionDTO

                foreach (TimeInvoiceTransaction timeInvoiceTransaction in timeInvoiceTransactions)
                {
                    #region Prereq

                    AttestState transactionAttestState = transactionsAttestStates.FirstOrDefault(i => i.AttestStateId == timeInvoiceTransaction.AttestStateId);
                    if (transactionAttestState == null)
                        continue;

                    bool transactionIsAttested = false;
                    if (attestState != null && attestStateSalaryExportPayrollMinimumAttestStatus != null && (attestStateSalaryExportPayrollMinimumAttestStatus.AttestStateId == attestState.AttestStateId || attestState.Sort > attestStateSalaryExportPayrollMinimumAttestStatus.Sort))
                        transactionIsAttested = true;

                    InvoiceProduct invoiceProduct = invoiceProducts.FirstOrDefault(i => i.ProductId == timeInvoiceTransaction.ProductId);
                    if (invoiceProduct == null)
                    {
                        invoiceProduct = ProductManager.GetInvoiceProduct(entities, timeInvoiceTransaction.ProductId);
                        if (invoiceProduct != null)
                            invoiceProducts.Add(invoiceProduct);
                    }

                    Account accountStd = accountStds.FirstOrDefault(i => i.AccountId == timeInvoiceTransaction.AccountStdId);
                    if (accountStd == null)
                    {
                        accountStd = AccountManager.GetAccount(entities, base.ActorCompanyId, timeInvoiceTransaction.AccountStdId, onlyActive: false);
                        if (accountStd != null)
                            accountStds.Add(accountStd);
                    }
                    if (accountStd == null)
                        continue;

                    List<Account> accountInternals = new List<Account>();
                    if (!timeInvoiceTransaction.AccountInternal.IsLoaded)
                        timeInvoiceTransaction.AccountInternal.Load();
                    foreach (AccountInternal accountInternal in timeInvoiceTransaction.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();
                        accountInternals.Add(accountInternal.Account);
                    }

                    #endregion

                    #region AttestEmployeeAdditionDeductionTransactionDTO

                    AttestEmployeeAdditionDeductionTransactionDTO transactionDTO = new AttestEmployeeAdditionDeductionTransactionDTO()
                    {
                        TransactionType = SoeTimeTransactionType.TimePayroll,
                        TransactionId = timeInvoiceTransaction.TimeInvoiceTransactionId,
                        Date = timeCodeTransaction.Start.Date,
                        ProductId = invoiceProduct.ProductId,
                        ProductName = invoiceProduct.Name,
                        AttestStateId = transactionAttestState.AttestStateId,
                        AttestStateName = transactionAttestState.Name,
                        AttestStateColor = transactionAttestState.Color,
                        IsReadOnly = !transactionAttestState.Initial,
                        IsAttested = transactionIsAttested,
                        Quantity = timeInvoiceTransaction.Quantity,
                        QuantityText = timeCode.IsRegistrationTypeTime ? CalendarUtility.GetHoursAndMinutesString((int)timeInvoiceTransaction.Quantity, false) : timeInvoiceTransaction.Quantity.ToString(),
                        UnitPrice = null, //timeInvoiceTransaction.UnitPrice,
                        Amount = timeInvoiceTransaction.Amount,
                        VatAmount = timeInvoiceTransaction.VatAmount,
                        Comment = String.Empty, //timeInvoiceTransaction.Comment,
                        AccountingString = AccountManager.GetAccountingString(accountDims, accountStd, accountInternals, false),
                    };

                    dto.Transactions.Add(transactionDTO);

                    #endregion
                }

                #endregion

                if (String.IsNullOrEmpty(dto.Accounting) || dto.Accounting == ",,,,,")
                {
                    List<string> transactionAccountingStrings = dto.Transactions.Select(i => i.AccountingString).Distinct().ToList();
                    if (transactionAccountingStrings.Count == 1)
                        dto.Accounting = transactionAccountingStrings.First();
                }

                dtos.Add(dto);
            }

            #endregion

            return dtos;
        }

        private List<TimeRule> LoadEmployeeDayTimeRules(CompEntities entities, List<int> timeRuleIds)
        {
            List<TimeRule> timeRules = TimeRuleManager.GetTimeRulesFromCache(entities, base.ActorCompanyId, onlyFromCache: true);
            if (timeRules != null)
                timeRules = timeRules.Where(tr => timeRuleIds.Contains(tr.TimeRuleId)).ToList();
            else
                timeRules = entities.TimeRule.Where(tr => timeRuleIds.Contains(tr.TimeRuleId)).ToList();
            return timeRules;
        }

        private void SetOptionalParameters(CompEntities entities, GetAttestEmployeeInput input)
        {
            if (input.IsWeb && (input.Holidays == null || input.AccountDims == null))
            {
                input.SetOptionalParameters(
                    holidays: input.Holidays ?? base.GetHolidaysFromCache(entities, CacheConfig.Company(input.ActorCompanyId)),
                    accountDims: input.AccountDims ?? base.GetAccountDimsFromCache(entities, CacheConfig.Company(input.ActorCompanyId)));
            }
        }

        private void SetEmployeeDayCalendar(AttestEmployeeDayDTO attestEmployeeDay, TimeScheduleTemplatePeriod templatePeriod, EmployeeSchedule employeeSchedule, TimeBlockDate timeBlockDate, HolidayDTO holiday, DayTypeDTO dayType)
        {
            if (attestEmployeeDay == null)
                return;

            attestEmployeeDay.Day = attestEmployeeDay.Date.Day;
            attestEmployeeDay.DayName = CalendarUtility.GetDayNameFromCulture(attestEmployeeDay.Date);
            attestEmployeeDay.DayOfWeekNr = CalendarUtility.GetDayNrFromCulture(attestEmployeeDay.Date);
            attestEmployeeDay.WeekNr = CalendarUtility.GetWeekNr(attestEmployeeDay.Date);
            attestEmployeeDay.WeekNrMonday = CalendarUtility.GetWeekNr(CalendarUtility.GetBeginningOfWeek(attestEmployeeDay.Date)).ToString();
            attestEmployeeDay.WeekInfo = GetText(5393, "Vecka:") + " " + attestEmployeeDay.WeekNr;
            attestEmployeeDay.TimeScheduleTemplatePeriodId = templatePeriod?.TimeScheduleTemplatePeriodId ?? 0;
            attestEmployeeDay.DayNumber = templatePeriod?.DayNumber ?? 0;
            attestEmployeeDay.TimeScheduleTemplateHeadId = employeeSchedule?.TimeScheduleTemplateHeadId ?? 0;
            attestEmployeeDay.TimeScheduleTemplateHeadName = employeeSchedule?.TimeScheduleTemplateHead.Name;
            attestEmployeeDay.EmployeeScheduleId = employeeSchedule?.EmployeeScheduleId ?? 0;
            attestEmployeeDay.TimeBlockDateId = timeBlockDate?.TimeBlockDateId ?? 0;
            attestEmployeeDay.HolidayId = holiday?.HolidayId;
            attestEmployeeDay.HolidayName = holiday?.Name;
            attestEmployeeDay.IsHoliday = holiday != null;
            attestEmployeeDay.DayTypeId = dayType?.DayTypeId ?? 0;
            attestEmployeeDay.DayTypeName = dayType?.Name;
        }

        private void SetEmployeeDaySchedule(AttestEmployeeDayDTO attestEmployeeDay, Employee employee, List<TimeTreeScheduleBlockDTO> scheduleItems, Dictionary<DateTime, int> scheduleTypeFactorsEmployee, List<TimeScheduleSwapRequestRow> scheduleSwapShiftRequestRows, List<TimeDeviationCause> timeDeviationCauses, bool loadGrossNetCost, bool loadShifts)
        {
            if (!scheduleItems.IsNullOrEmpty())
            {
                var scheduleItemsWork = scheduleItems.GetWork();
                var scheduleItemsBreaks = scheduleItems.GetBreaks();

                foreach (TimeTreeScheduleBlockDTO scheduleItem in scheduleItemsWork)
                {
                    SetEmployeeDayScheduleWork(attestEmployeeDay, scheduleItem);
                    SetEmployeeDayScheduleShiftStatus(attestEmployeeDay, scheduleItem);
                    if (loadGrossNetCost)
                        SetEmployeeDayScheduleGrossNetCost(attestEmployeeDay, scheduleItem);
                    if (loadShifts)
                        SetEmployeeDayScheduleShifts(attestEmployeeDay, employee, scheduleItem, scheduleItemsBreaks, timeDeviationCauses, scheduleItem.IsOnDuty() ? TermGroup_TimeScheduleTemplateBlockType.OnDuty : TermGroup_TimeScheduleTemplateBlockType.Schedule);
                }

                SetEmployeeDayPreliminary(attestEmployeeDay, scheduleItemsWork);
                SetEmployeeDayScheduleTypeFactors(attestEmployeeDay, scheduleTypeFactorsEmployee);
                SetEmployeeScheduleSwapShiftRequestRows(attestEmployeeDay, scheduleSwapShiftRequestRows);
                SetEmployeeDayScheduleBreaks(attestEmployeeDay, scheduleItemsBreaks);
                if (loadGrossNetCost)
                    SetEmployeeDayScheduleGrossNetCostBreaks(attestEmployeeDay);

                if (attestEmployeeDay.ScheduleTime.TotalMinutes > 0)
                    attestEmployeeDay.ScheduleTime = attestEmployeeDay.ScheduleTime.Subtract(attestEmployeeDay.ScheduleBreakTime);

                if (attestEmployeeDay.OccupiedTime.TotalMinutes > 0)
                    attestEmployeeDay.OccupiedTime = attestEmployeeDay.OccupiedTime.Subtract(attestEmployeeDay.ScheduleBreakTime);

                attestEmployeeDay.IsPreliminary = attestEmployeeDay.IsPrel ? GetText(5713, "Ja") : GetText(5714, "Nej");
            }

            attestEmployeeDay.IsScheduleZeroDay = attestEmployeeDay.ScheduleTime.TotalMinutes == 0;
        }

        private void SetEmployeeDayScheduleWork(AttestEmployeeDayDTO attestEmployeeDay, TimeTreeScheduleBlockDTO scheduleItem)
        {
            if (scheduleItem == null)
                return;

            if (!scheduleItem.IsNotScheduleTime)
                attestEmployeeDay.ScheduleTime = attestEmployeeDay.ScheduleTime.Add(scheduleItem.StopTime.Subtract(scheduleItem.StartTime));
            else
                attestEmployeeDay.OccupiedTime = attestEmployeeDay.OccupiedTime.Add(scheduleItem.StopTime.Subtract(scheduleItem.StartTime));

            if (!scheduleItem.IsZeroBlock)
            {
                if (attestEmployeeDay.ScheduleStartTime == CalendarUtility.DATETIME_DEFAULT || attestEmployeeDay.ScheduleStartTime > scheduleItem.StartTime)
                    attestEmployeeDay.ScheduleStartTime = scheduleItem.StartTime;
                if (attestEmployeeDay.ScheduleStopTime < scheduleItem.StopTime)
                    attestEmployeeDay.ScheduleStopTime = scheduleItem.StopTime;
            }

            attestEmployeeDay.IsNotScheduleTime = scheduleItem.IsNotScheduleTime;

            //RecalculateTimeRecordId
            attestEmployeeDay.RecalculateTimeRecordId = scheduleItem.RecalculateTimeRecordId ?? 0;
            attestEmployeeDay.RecalculateTimeRecordStatus = (TermGroup_RecalculateTimeRecordStatus)scheduleItem.RecalculateTimeRecordStatus;
            attestEmployeeDay.HasScheduledPlacement = attestEmployeeDay.RecalculateTimeRecordStatus == TermGroup_RecalculateTimeRecordStatus.Unprocessed;
        }

        private void SetEmployeeDayScheduleShiftStatus(AttestEmployeeDayDTO attestEmployeeDay, TimeTreeScheduleBlockDTO scheduleItem)
        {
            attestEmployeeDay.AddShiftUserStatus((TermGroup_TimeScheduleTemplateBlockShiftUserStatus)scheduleItem.ShiftUserStatus);
        }

        private void SetEmployeeDayPreliminary(AttestEmployeeDayDTO attestEmployeeDay, List<TimeTreeScheduleBlockDTO> scheduleItems)
        {
            if (attestEmployeeDay == null || scheduleItems.IsNullOrEmpty())
                return;

            if (scheduleItems.Any(s => s.IsPreliminary))
                attestEmployeeDay.IsPrel = true;
        }

        private void SetEmployeeDayScheduleTypeFactors(AttestEmployeeDayDTO attestEmployeeDay, Dictionary<DateTime, int> timeScheduleTypeFactorsEmployee)
        {
            if (timeScheduleTypeFactorsEmployee == null || !timeScheduleTypeFactorsEmployee.ContainsKey(attestEmployeeDay.Date))
                return;

            attestEmployeeDay.TimeScheduleTypeFactorMinutes = timeScheduleTypeFactorsEmployee[attestEmployeeDay.Date];
            attestEmployeeDay.ScheduleTime = attestEmployeeDay.ScheduleTime.Add(new TimeSpan(0, attestEmployeeDay.TimeScheduleTypeFactorMinutes, 0));
        }

        private void SetEmployeeScheduleSwapShiftRequestRows(AttestEmployeeDayDTO attestEmployeeDay, List<TimeScheduleSwapRequestRow> scheduleSwapShiftRequestRows)
        {
            if (attestEmployeeDay == null || scheduleSwapShiftRequestRows.IsNullOrEmpty())
                return;

            attestEmployeeDay.HasShiftSwaps = scheduleSwapShiftRequestRows.Any(s => s.Date == attestEmployeeDay.Date && s.IsApproved);
        }

        private void SetEmployeeDayScheduleShifts(AttestEmployeeDayDTO attestEmployeeDay, Employee employee, TimeTreeScheduleBlockDTO scheduleItem, List<TimeTreeScheduleBlockDTO> scheduleItemsBreaks, List<TimeDeviationCause> timeDeviationCauses, TermGroup_TimeScheduleTemplateBlockType type = TermGroup_TimeScheduleTemplateBlockType.Schedule)
        {
            AttestEmployeeDayShiftDTO shift = CreateEmployeeDayScheduleShift(attestEmployeeDay, scheduleItem, timeDeviationCauses, type);
            SetEmployeeDayScheduleShiftBreaks(shift, employee, scheduleItemsBreaks);
            attestEmployeeDay.Shifts.Add(shift);
        }

        private void SetEmployeeDayScheduleShiftBreaks(AttestEmployeeDayShiftDTO shift, Employee employee, List<TimeTreeScheduleBlockDTO> scheduleBreaks)
        {
            if (scheduleBreaks.IsNullOrEmpty())
                return;

            bool isHidden = employee.Hidden && !string.IsNullOrEmpty(shift.Link);
            string breakPrefix = GetText(5990, "Rast");
            int breakNumber = 0;

            foreach (var scheduleBreak in scheduleBreaks.Where(b => !isHidden || b.Link == shift.Link))
            {
                DateTime startTime = CalendarUtility.MergeDateAndTime(scheduleBreak.Date.Value.AddDays((scheduleBreak.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), scheduleBreak.StartTime);

                SetEmployeeDayScheduleShiftBreak(breakNumber, shift, scheduleBreak, startTime, breakPrefix);

                breakNumber++;
            }
        }

        private void SetEmployeeDayScheduleShiftBreak(int breakNumber, AttestEmployeeDayShiftDTO shift, TimeTreeScheduleBlockDTO scheduleBreak, DateTime startTime, string breakPrefix)
        {
            if (breakNumber == 0)
            {
                shift.Break1Id = scheduleBreak.TimeScheduleTemplateBlockId;
                shift.Break1StartTime = startTime;
                shift.Break1Minutes = scheduleBreak.BreakMinutes;
                shift.Break1TimeCodeId = scheduleBreak.TimeCodeId;
                shift.Break1TimeCode = scheduleBreak.TimeCodeName;
                if (!shift.Break1TimeCode.StartsWith(breakPrefix))
                    shift.Break1TimeCode = $"{breakPrefix} {shift.Break1TimeCode}";

            }
            if (breakNumber == 1)
            {
                shift.Break2Id = scheduleBreak.TimeScheduleTemplateBlockId;
                shift.Break2StartTime = startTime;
                shift.Break2Minutes = scheduleBreak.BreakMinutes;
                shift.Break2TimeCodeId = scheduleBreak.TimeCodeId;
                shift.Break2TimeCode = scheduleBreak.TimeCodeName;
                if (!shift.Break2TimeCode.StartsWith(breakPrefix))
                    shift.Break2TimeCode = $"{breakPrefix} {shift.Break2TimeCode}";
            }
            if (breakNumber == 2)
            {
                shift.Break3Id = scheduleBreak.TimeScheduleTemplateBlockId;
                shift.Break3StartTime = startTime;
                shift.Break3Minutes = scheduleBreak.BreakMinutes;
                shift.Break3TimeCodeId = scheduleBreak.TimeCodeId;
                shift.Break3TimeCode = scheduleBreak.TimeCodeName;
                if (!shift.Break3TimeCode.StartsWith(breakPrefix))
                    shift.Break3TimeCode = $"{breakPrefix} {shift.Break3TimeCode}";
            }
            if (breakNumber == 3)
            {
                shift.Break4Id = scheduleBreak.TimeScheduleTemplateBlockId;
                shift.Break4StartTime = startTime;
                shift.Break4Minutes = scheduleBreak.BreakMinutes;
                shift.Break4TimeCodeId = scheduleBreak.TimeCodeId;
                shift.Break4TimeCode = scheduleBreak.TimeCodeName;
                if (!shift.Break4TimeCode.StartsWith(breakPrefix))
                    shift.Break4TimeCode = $"{breakPrefix} {shift.Break4TimeCode}";
            }
        }

        private void SetEmployeeDayScheduleBreaks(AttestEmployeeDayDTO attestEmployeeDay, List<TimeTreeScheduleBlockDTO> scheduleItemsBreaks)
        {
            if (scheduleItemsBreaks.IsNullOrEmpty())
                return;

            foreach (var scheduleBreakItem in scheduleItemsBreaks)
            {
                int breakMinutes = (int)scheduleBreakItem.StopTime.Subtract(scheduleBreakItem.StartTime).TotalMinutes;

                attestEmployeeDay.NoOfScheduleBreaks++;
                attestEmployeeDay.ScheduleBreakTime = attestEmployeeDay.ScheduleBreakTime.Add(new TimeSpan(0, breakMinutes, 0));
                if (attestEmployeeDay.NoOfScheduleBreaks == 1)
                {
                    attestEmployeeDay.ScheduleBreak1Start = scheduleBreakItem.StartTime;
                    attestEmployeeDay.ScheduleBreak1Minutes = breakMinutes;
                }
                if (attestEmployeeDay.NoOfScheduleBreaks == 2)
                {
                    attestEmployeeDay.ScheduleBreak2Start = scheduleBreakItem.StartTime;
                    attestEmployeeDay.ScheduleBreak2Minutes = breakMinutes;
                }
                if (attestEmployeeDay.NoOfScheduleBreaks == 3)
                {
                    attestEmployeeDay.ScheduleBreak3Start = scheduleBreakItem.StartTime;
                    attestEmployeeDay.ScheduleBreak3Minutes = breakMinutes;
                }
                if (attestEmployeeDay.NoOfScheduleBreaks == 4)
                {
                    attestEmployeeDay.ScheduleBreak4Start = scheduleBreakItem.StartTime;
                    attestEmployeeDay.ScheduleBreak4Minutes = breakMinutes;
                }
            }
        }

        private void SetEmployeeDayScheduleGrossNetCost(AttestEmployeeDayDTO attestEmployeeDay, TimeTreeScheduleBlockDTO scheduleItem)
        {
            attestEmployeeDay.GrossNetCosts.Add(GrossNetCostDTO.Create(attestEmployeeDay.EmployeeId, scheduleItem.TimeScheduleTemplateBlockId, scheduleItem.TimeScheduleTypeId, scheduleItem.TimeDeviationCauseId, attestEmployeeDay.Date, scheduleItem.StartTime, scheduleItem.StopTime));
        }

        private void SetEmployeeDayScheduleGrossNetCostBreaks(AttestEmployeeDayDTO attestEmployeeDay)
        {
            if (attestEmployeeDay.GrossNetCosts == null)
                return;

            foreach (var scheduleCost in attestEmployeeDay.GrossNetCosts)
            {
                scheduleCost.SetBreaks(attestEmployeeDay.ScheduleBreak1Start, attestEmployeeDay.ScheduleBreak1Minutes, attestEmployeeDay.ScheduleBreak2Start, attestEmployeeDay.ScheduleBreak2Minutes, attestEmployeeDay.ScheduleBreak3Start, attestEmployeeDay.ScheduleBreak3Minutes, attestEmployeeDay.ScheduleBreak4Start, attestEmployeeDay.ScheduleBreak4Minutes);
            }
        }

        private void SetEmployeeDayScheduleStandby(AttestEmployeeDayDTO attestEmployeeDay, List<TimeTreeScheduleBlockDTO> standbyScheduleItems, List<TimeDeviationCause> timeDeviationCauses, bool loadShifts, bool loadPresenceAbsenceDetails)
        {
            attestEmployeeDay.StandbyTimeDetails = new Dictionary<string, TimeSpan>();

            foreach (var standbyScheduleItem in standbyScheduleItems.GetWork())
            {
                SetEmployeeDayScheduleStandby(attestEmployeeDay, standbyScheduleItem);
                if (loadPresenceAbsenceDetails)
                    SetEmployeeDayScheduleStandbyPresenceAbsenceDetails(attestEmployeeDay, standbyScheduleItem);
                if (loadShifts)
                    SetEmployeeDayScheduleStandbyShifts(attestEmployeeDay, standbyScheduleItem, timeDeviationCauses);
            }
        }

        private void SetEmployeeDayScheduleStandbyPresenceAbsenceDetails(AttestEmployeeDayDTO attestEmployeeDay, TimeTreeScheduleBlockDTO standbyScheduleItem)
        {
            if (!standbyScheduleItem.ShiftTypeId.HasValue)
                return;

            attestEmployeeDay.StandbyTimeDetails.AddTime(standbyScheduleItem.ShiftTypeName, standbyScheduleItem.StopTime.Subtract(standbyScheduleItem.StartTime));
        }

        private void SetEmployeeDayScheduleStandby(AttestEmployeeDayDTO attestEmployeeDay, TimeTreeScheduleBlockDTO standbyScheduleItem)
        {
            attestEmployeeDay.StandbyTime = attestEmployeeDay.StandbyTime.Update(standbyScheduleItem.StopTime.Subtract(standbyScheduleItem.StartTime));
            if (!standbyScheduleItem.IsZeroBlock)
            {
                if (attestEmployeeDay.StandByStartTime == CalendarUtility.DATETIME_DEFAULT || attestEmployeeDay.StandByStartTime > standbyScheduleItem.StartTime)
                    attestEmployeeDay.StandByStartTime = standbyScheduleItem.StartTime;
                if (attestEmployeeDay.StandByStopTime < standbyScheduleItem.StopTime)
                    attestEmployeeDay.StandByStopTime = standbyScheduleItem.StopTime;
            }
        }

        private void SetEmployeeDayScheduleStandbyShifts(AttestEmployeeDayDTO attestEmployeeDay, TimeTreeScheduleBlockDTO standbyScheduleItem, List<TimeDeviationCause> timeDeviationCauses)
        {
            AttestEmployeeDayShiftDTO shift = CreateEmployeeDayScheduleShift(attestEmployeeDay, standbyScheduleItem, timeDeviationCauses, TermGroup_TimeScheduleTemplateBlockType.Standby);
            attestEmployeeDay.StandbyShifts.Add(shift);
        }

        private void SetEmployeeDayScheduleTemplate(AttestEmployeeDayDTO attestEmployeeDay, TimeScheduleTemplatePeriod templatePeriod, List<GetTemplateSchedule_Result> templateScheduleItems)
        {
            if (attestEmployeeDay == null || templatePeriod == null || templateScheduleItems == null)
                return;

            attestEmployeeDay.IsTemplateSheduleLoaded = true;

            int periodCounter = 0;
            var templateSchedulePeriod = templateScheduleItems.GetScheduleForPeriod(templatePeriod.TimeScheduleTemplatePeriodId);
            foreach (var templateScheduleBlockItem in templateSchedulePeriod.GetWork())
            {
                TimeSpan length = templateScheduleBlockItem.StopTime.Subtract(templateScheduleBlockItem.StartTime);
                attestEmployeeDay.TemplateScheduleTime = attestEmployeeDay.TemplateScheduleTime.Add(length);
                if (periodCounter == 0 || attestEmployeeDay.TemplateScheduleStartTime > templateScheduleBlockItem.StartTime)
                    attestEmployeeDay.TemplateScheduleStartTime = templateScheduleBlockItem.StartTime;
                if (periodCounter == 0 || attestEmployeeDay.TemplateScheduleStopTime < templateScheduleBlockItem.StopTime)
                    attestEmployeeDay.TemplateScheduleStopTime = templateScheduleBlockItem.StopTime;

                periodCounter++;
            }

            if (periodCounter == 0)
            {
                attestEmployeeDay.TemplateScheduleStartTime = CalendarUtility.DATETIME_DEFAULT;
                attestEmployeeDay.TemplateScheduleStopTime = CalendarUtility.DATETIME_DEFAULT;
            }

            int templateScheduleBreakNumber = 0;
            foreach (var templateScheduleBreakItem in templateSchedulePeriod.GetBreaks())
            {
                int minutes = (int)templateScheduleBreakItem.StopTime.Subtract(templateScheduleBreakItem.StartTime).TotalMinutes;

                attestEmployeeDay.TemplateScheduleBreakTime = attestEmployeeDay.TemplateScheduleBreakTime.Add(new TimeSpan(0, minutes, 0));

                if (templateScheduleBreakNumber == 0)
                {
                    attestEmployeeDay.TemplateScheduleBreak1Start = templateScheduleBreakItem.StartTime;
                    attestEmployeeDay.TemplateScheduleBreak1Minutes = minutes;
                }
                if (templateScheduleBreakNumber == 1)
                {
                    attestEmployeeDay.TemplateScheduleBreak2Start = templateScheduleBreakItem.StartTime;
                    attestEmployeeDay.TemplateScheduleBreak2Minutes = minutes;
                }
                if (templateScheduleBreakNumber == 2)
                {
                    attestEmployeeDay.TemplateScheduleBreak3Start = templateScheduleBreakItem.StartTime;
                    attestEmployeeDay.TemplateScheduleBreak3Minutes = minutes;
                }
                if (templateScheduleBreakNumber == 3)
                {
                    attestEmployeeDay.TemplateScheduleBreak4Start = templateScheduleBreakItem.StartTime;
                    attestEmployeeDay.TemplateScheduleBreak4Minutes = minutes;
                }

                templateScheduleBreakNumber++;
            }

            attestEmployeeDay.TemplateScheduleTime = attestEmployeeDay.TemplateScheduleTime.Subtract(attestEmployeeDay.TemplateScheduleBreakTime);
            attestEmployeeDay.IsTemplateScheduleZeroDay = attestEmployeeDay.TemplateScheduleTime.TotalMinutes == 0;
        }

        private void SetEmployeeDayTimeBlocks(AttestEmployeeDayDTO attestEmployeeDay, GetAttestEmployeeInput input, EmployeeGroupDTO employeeGroup, TimeBlockDate timeBlockDate, List<TimeBlock> timeBlocksDay, List<GetTimePayrollTransactionsForEmployee_Result> timePayrollTransactionsDay, List<TimeDeviationCause> timeDeviationCauses, List<AccountDimDTO> accountDims, int? attestStateIdIntitialPayroll, int? standardTimeDeviationCauseId)
        {
            if (input == null || attestEmployeeDay == null || timeBlockDate == null || timeBlocksDay.IsNullOrEmpty())
                return;

            var absenceTimeDeviationCauses = new Dictionary<int, TimeSpan>();
            var presenceInsideScheduleTimeDeviationCauses = new Dictionary<int, TimeSpan>();
            var presenceOutsideScheduleTimeDeviationCauses = new Dictionary<int, TimeSpan>();

            foreach (TimeBlock timeBlock in timeBlocksDay)
            {
                #region TimeBlock

                var (total, insideSchedule, outsideSchedule) = GetTimeBlockLengths(timeBlock, attestEmployeeDay.ScheduleStartTime, attestEmployeeDay.ScheduleStopTime, attestEmployeeDay.StandByStartTime, attestEmployeeDay.StandByStopTime, standardTimeDeviationCauseId);
                var (isPresence, isAbsence, isBreak) = GetTimeBlockTypes(timeBlock.TimeCode?.Select(i => i.Type), timePayrollTransactionsDay, timeBlock.TimeBlockId);

                if (timeBlock.TimeDeviationCauseStartId.HasValue)
                {
                    if (isAbsence && !isBreak)
                    {
                        absenceTimeDeviationCauses.AddTime(timeBlock.TimeDeviationCauseStartId.Value, total);
                    }
                    else if (isPresence || outsideSchedule.TotalMinutes > 0)
                    {
                        presenceInsideScheduleTimeDeviationCauses.AddTime(timeBlock.TimeDeviationCauseStartId.Value, insideSchedule);
                        presenceOutsideScheduleTimeDeviationCauses.AddTime(timeBlock.TimeDeviationCauseStartId.Value, outsideSchedule);
                    }
                }

                if (total.TotalMinutes > 0)
                {
                    if (isAbsence && !isBreak)
                    {
                        #region Absence

                        attestEmployeeDay.AbsenceTime = attestEmployeeDay.AbsenceTime.Update(total);

                        #endregion
                    }
                    else if (isPresence)
                    {
                        #region Presence

                        if (!attestEmployeeDay.PresenceStartTime.HasValue || attestEmployeeDay.PresenceStartTime.Value > timeBlock.StartTime)
                        {
                            attestEmployeeDay.PresenceStartTime = timeBlock.StartTime;
                            attestEmployeeDay.FirstPresenceTimeBlockId = timeBlock.TimeBlockId;
                            attestEmployeeDay.FirstPresenceTimeBlockDeviationCauseId = timeBlock.TimeDeviationCauseStartId;
                            attestEmployeeDay.FirstPresenceTimeBlockComment = timeBlock.Comment;
                        }
                        if (!attestEmployeeDay.PresenceStopTime.HasValue || attestEmployeeDay.PresenceStopTime.Value < timeBlock.StopTime)
                        {
                            attestEmployeeDay.PresenceStopTime = timeBlock.StopTime;
                            attestEmployeeDay.LastPresenceTimeBlockId = timeBlock.TimeBlockId;
                            attestEmployeeDay.LastPresenceTimeBlockDeviationCauseId = timeBlock.TimeDeviationCauseStartId;
                            attestEmployeeDay.LastPresenceTimeBlockComment = timeBlock.Comment;
                        }

                        attestEmployeeDay.PresenceTime = attestEmployeeDay.PresenceTime.Update(total);
                        attestEmployeeDay.PresenceInsideScheduleTime = attestEmployeeDay.PresenceInsideScheduleTime.Update(insideSchedule);
                        attestEmployeeDay.PresenceOutsideScheduleTime = attestEmployeeDay.PresenceOutsideScheduleTime.Update(outsideSchedule);

                        #endregion
                    }

                    #region Break

                    if (isBreak && !isPresence)
                    {
                        AttestEmployeeBreakDTO breakItem = new AttestEmployeeBreakDTO()
                        {
                            TimeBlockDateId = timeBlock.TimeBlockDateId,
                            TimeBlockId = timeBlock.TimeBlockId,
                            StartTime = timeBlock.StartTime,
                            StopTime = timeBlock.StopTime,
                            Minutes = Convert.ToInt32(total.TotalMinutes),
                        };
                        attestEmployeeDay.PresenceBreakItems.Add(breakItem);
                        attestEmployeeDay.PresenceBreakMinutes = attestEmployeeDay.PresenceBreakMinutes.HasValue ? (attestEmployeeDay.PresenceBreakMinutes.Value + breakItem.Minutes) : breakItem.Minutes;
                    }

                    #endregion
                }

                #endregion
            }

            #region TimeDeviationCauses - optional return value

            if (!timeDeviationCauses.IsNullOrEmpty())
            {
                List<int> timeDeviationCauseIds = absenceTimeDeviationCauses.GetKeys(presenceInsideScheduleTimeDeviationCauses, presenceOutsideScheduleTimeDeviationCauses);
                attestEmployeeDay.TimeDeviationCauseNames = StringUtility.GetCommaSeparatedString(timeDeviationCauses.GetNames(timeDeviationCauseIds, excludeTimeDeviationCauseId: employeeGroup?.TimeDeviationCauseId));

                if (input.DoLoad(InputLoadType.PresenceAbsenceDetails))
                {
                    attestEmployeeDay.AbsenceTimeDetails = timeDeviationCauses.ConvertToNameDict(absenceTimeDeviationCauses);
                    attestEmployeeDay.PresenceInsideScheduleTimeDetails = timeDeviationCauses.ConvertToNameDict(presenceInsideScheduleTimeDeviationCauses);
                    attestEmployeeDay.PresenceOutsideScheduleTimeDetails = timeDeviationCauses.ConvertToNameDict(presenceOutsideScheduleTimeDeviationCauses);
                }
            }

            #endregion

            #region TimeBlocks - optional return value

            if (input.DoLoad(InputLoadType.TimeBlocks))
            {
                if (attestStateIdIntitialPayroll.HasValue)
                    timeBlocksDay.SetIsAttestedPayroll(attestStateIdIntitialPayroll.Value, attestEmployeeDay.AttestPayrollTransactions);

                attestEmployeeDay.TimeBlocks = timeBlocksDay.ToAttestEmployeeTimeBlockDTOs(timeBlockDate, attestEmployeeDay.ScheduleStartTime, attestEmployeeDay.ScheduleStopTime, attestEmployeeDay.AttestPayrollTransactions, timeDeviationCauses, accountDims: accountDims);
                SetEmployeeDayTimeBlocksReadonlyFlags(attestEmployeeDay, timeDeviationCauses);
            }

            #endregion

            attestEmployeeDay.ContainsDuplicateTimeBlocks = timeBlocksDay.ContainsDuplicateTimeBlocks();
            attestEmployeeDay.ManuallyAdjusted = timeBlocksDay.Any(i => i.ManuallyAdjusted);
        }

        private void SetEmployeeDayTimeBlocksReadonlyFlags(AttestEmployeeDayDTO attestEmployeeDay, List<TimeDeviationCause> timeDeviationCauses)
        {
            if (attestEmployeeDay == null || attestEmployeeDay.TimeBlocks.IsNullOrEmpty())
                return;

            bool isReadonly = attestEmployeeDay.IsReadonly || timeDeviationCauses.IsWholeDayAbsence(attestEmployeeDay.TimeBlocks.FirstOrDefault()?.TimeDeviationCauseStartId ?? 0);
            if (!isReadonly)
                return;

            foreach (var timeBlock in attestEmployeeDay.TimeBlocks)
            {
                timeBlock.IsReadonlyLeft = true;
                timeBlock.IsReadonlyRight = true;
            }
        }

        private void SetEmployeeDayPayrollImportWarnings(AttestEmployeeDayDTO attestEmployeeDay, Employee employee, Dictionary<int, List<DateTime>> employeesAndDatesWithPayrollImports)
        {
            if (attestEmployeeDay == null || employee == null || employeesAndDatesWithPayrollImports == null)
                return;

            attestEmployeeDay.HasPayrollImportWarnings = employeesAndDatesWithPayrollImports.GetList(employee.EmployeeId).Any(d => d == attestEmployeeDay.Date);
        }

        private void SetEmployeeDayPayrollImportTransactions(AttestEmployeeDayDTO attestEmployeeDay, Employee employee, List<PayrollImportEmployeeTransaction> payrollImportEmployeeTransactions)
        {
            if (attestEmployeeDay == null || employee == null || payrollImportEmployeeTransactions == null)
                return;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var products = base.GetPayrollProductDTOsWithSettingsFromCache(entities, CacheConfig.Company(ActorCompanyId)).Where(w => payrollImportEmployeeTransactions.Where(ww => ww.PayrollProductId.HasValue).Select(s => s.PayrollProductId).Contains(w.ProductId)).ToList();
            var deviationCodes = base.GetTimeDeviationCausesFromCache(entities, CacheConfig.Company(ActorCompanyId)).ToDTOs().Where(w => payrollImportEmployeeTransactions.Where(ww => ww.TimeDeviationCauseId.HasValue).Select(s => s.TimeDeviationCauseId).Contains(w.TimeDeviationCauseId)).ToList();

            foreach (PayrollImportEmployeeTransaction importTransaction in payrollImportEmployeeTransactions)
            {
                importTransaction.TypeName = GetText(importTransaction.Type, (int)TermGroup.PayrollImportEmployeeTransactionType);
                importTransaction.StatusName = GetText(importTransaction.Status, (int)TermGroup.PayrollImportEmployeeTransactionStatus);
            }

            attestEmployeeDay.PayrollImportEmployeeTransactions = payrollImportEmployeeTransactions.Where(w => w.Date == attestEmployeeDay.Date && w.PayrollImportEmployee != null && w.PayrollImportEmployee.EmployeeId == employee.EmployeeId).ToDTOs(products, deviationCodes).ToList();
            attestEmployeeDay.HasPayrollImportEmployeeTransactions = !attestEmployeeDay.PayrollImportEmployeeTransactions.IsNullOrEmpty();
        }

        private void SetEmployeeDayTimeStampEntrys(AttestEmployeeDayDTO attestEmployeeDay, CompEntities entities, GetAttestEmployeeInput input, List<TimeBlockDate> timeBlockDates, List<TimeStampEntry> timeStampEntrys, List<TimeScheduleType> timeScheduleTypes, bool loadTimeStamps)
        {
            if (input == null || attestEmployeeDay == null)
                return;

            //Look for days with TimeStampEntrys but no transactions
            if (timeStampEntrys == null && !attestEmployeeDay.HasTransactions && !attestEmployeeDay.AutogenTimeblocks)
            {
                //Dont look for future days
                DateTime newStopDate = input.StopDate > DateTime.Now ? CalendarUtility.GetEndOfDay(DateTime.Now.Date) : input.StopDate;
                timeStampEntrys = TimeStampManager.GetTimeStampEntries(entities, timeBlockDates.Where(i => i.Date <= newStopDate).Select(s => s.TimeBlockDateId).ToList(), true);
            }

            attestEmployeeDay.HasTimeStampEntries = timeStampEntrys?.Any(i => i.TimeBlockDateId == attestEmployeeDay.TimeBlockDateId) ?? false;

            if (loadTimeStamps && timeStampEntrys != null)
            {
                attestEmployeeDay.TimeStampEntrys = new List<AttestEmployeeDayTimeStampDTO>();

                List<TimeStampEntry> timeStampEntrysForDate = timeStampEntrys.Where(w => w.TimeBlockDateId == attestEmployeeDay.TimeBlockDateId).OrderBy(w => w.Time).ThenBy(w => w.TimeStampEntryId).ToList();
                foreach (TimeStampEntry timeStampEntry in timeStampEntrysForDate)
                {
                    AttestEmployeeDayTimeStampDTO entry = new AttestEmployeeDayTimeStampDTO()
                    {
                        TimeStampEntryId = timeStampEntry.TimeStampEntryId,
                        TimeTerminalId = timeStampEntry.TimeTerminalId,
                        EmployeeId = timeStampEntry.EmployeeId,
                        AccountId = timeStampEntry.AccountId,
                        TimeTerminalAccountId = timeStampEntry.TimeTerminalAccountId,
                        TimeScheduleTemplatePeriodId = timeStampEntry.TimeScheduleTemplatePeriodId,
                        TimeBlockDateId = timeStampEntry.TimeBlockDateId,
                        TimeDeviationCauseId = timeStampEntry.TimeDeviationCauseId,
                        EmployeeChildId = timeStampEntry.EmployeeChildId,
                        ShiftTypeId = timeStampEntry.ShiftTypeId,
                        TimeScheduleTypeId = timeStampEntry.TimeScheduleTypeId,
                        Type = (TimeStampEntryType)timeStampEntry.Type,
                        OriginType = (TermGroup_TimeStampEntryOriginType)timeStampEntry.OriginType,
                        Status = (TermGroup_TimeStampEntryStatus)timeStampEntry.Status,
                        Time = timeStampEntry.Time,
                        Note = timeStampEntry.Note,
                        IsBreak = timeStampEntry.IsBreak,
                        IsPaidBreak = timeStampEntry.IsPaidBreak,
                        IsDistanceWork = timeStampEntry.IsDistanceWork,
                        ManuallyAdjusted = timeStampEntry.ManuallyAdjusted,
                        EmployeeManuallyAdjusted = timeStampEntry.EmployeeManuallyAdjusted,
                        AutoStampOut = timeStampEntry.AutoStampOut,
                    };

                    if (timeStampEntry.TimeStampEntryExtended != null)
                        entry.Extended = timeStampEntry.TimeStampEntryExtended.Where(t => t.State == (int)SoeEntityState.Active).ToDTOs();

                    if (entry.TimeScheduleTypeId.HasValue)
                        entry.TimeScheduleTypeName = timeScheduleTypes?.FirstOrDefault(i => i.TimeScheduleTypeId == entry.TimeScheduleTypeId.Value)?.Name ?? string.Empty;

                    attestEmployeeDay.TimeStampEntrys.Add(entry);
                }
            }
        }

        private void SetEmployeeDaysProjectTimeBlocks(GetAttestEmployeeInput input, AttestEmployeeDayDTO employeeDay, List<ProjectTimeBlockDTO> projectTimeBlocks)
        {
            if (employeeDay == null || employeeDay.TimeReportType != (int)TermGroup_TimeReportType.ERP || projectTimeBlocks.IsNullOrEmpty())
                return;

            employeeDay.SetProjectTimeBlocks(projectTimeBlocks);
            if (!employeeDay.ProjectTimeBlocks.Any())
                return;

            if (input.DoLoad(InputLoadType.SumInvoicedTime, input.DoLoad(InputLoadType.SumsAll)))
                employeeDay.SumInvoicedTime = employeeDay.SumInvoicedTime.Add(new TimeSpan(0, Convert.ToInt32(employeeDay.ProjectTimeBlocks.Sum(i => i.InvoiceQuantity)), 0));
        }

        private void SetEmployeeDayTimeCodeTransactions(AttestEmployeeDayDTO attestEmployeeDay, GetAttestEmployeeInput input, List<TimeCodeTransaction> timeCodeTransactionsDay, List<TimeRule> timeRules)
        {
            if (input == null || attestEmployeeDay == null || timeCodeTransactionsDay == null)
                return;

            attestEmployeeDay.TimeCodeTransactions = timeCodeTransactionsDay.ToAttestEmployeeTimeCodeTransactionDTOs(timeRules);
        }

        private void SetEmployeeDayTimePayrollTransactions(AttestEmployeeDayDTO attestEmployeeDay, GetAttestEmployeeInput input, List<GetTimePayrollTransactionsForEmployee_Result> timePayrollTransactionsDay, List<AccountDTO> transactionAccountStdsEmployee, List<GetTimePayrollTransactionAccountsForEmployee_Result> transactionAccountInternalsEmployee, List<GetAttestTransitionLogsForEmployeeResult> attestTransitionLogsEmployee, Dictionary<int, string> employeeChilds, bool hasSalaryPermission)
        {
            if (input == null || attestEmployeeDay == null || timePayrollTransactionsDay.IsNullOrEmpty())
                return;

            bool loadAllSums = input.DoLoad(InputLoadType.SumsAll);

            foreach (var timePayrollTransaction in timePayrollTransactionsDay)
            {
                List<AccountDTO> accountInternals = transactionAccountInternalsEmployee.GetAccountInternals(timePayrollTransaction.TimePayrollTransactionId);
                AccountDTO accountStd = transactionAccountStdsEmployee?.FirstOrDefault(i => i.AccountId == timePayrollTransaction.AccountId);

                AttestPayrollTransactionDTO transactionItem = timePayrollTransaction.CreateTransactionItem(accountInternals, accountStd, input.AccountDims);
                if (transactionItem != null)
                {
                    if (!hasSalaryPermission && !transactionItem.EarningTimeAccumulatorId.HasValue)
                        transactionItem.ClearFormulasAndAmounts();

                    attestEmployeeDay.HasPayrollTransactions = true;
                    attestEmployeeDay.CalculateTransactionAggregations(transactionItem);
                    if (!attestEmployeeDay.AttestStates.Any(a => a.AttestStateId == transactionItem.AttestStateId))
                        attestEmployeeDay.AttestStates.Add(transactionItem.GetAttestState());
                    if (transactionItem.EmployeeChildId.HasValue && employeeChilds != null && employeeChilds.ContainsKey(transactionItem.EmployeeChildId.Value))
                        transactionItem.EmployeeChildName = employeeChilds[transactionItem.EmployeeChildId.Value];

                    SetEmployeeDayTransactionSums(attestEmployeeDay, input, transactionItem, loadAllSums);
                    AddEmployeeDayTransaction(attestEmployeeDay, transactionItem, timePayrollTransactionsDay, attestTransitionLogsEmployee, input.DoMergeTransactions, input.DoLoad(InputLoadType.AttestTransitionLog));
                }
            }
        }

        private void SetEmployeeDayTimeScheduleTransaction(AttestEmployeeDayDTO attestEmployeeDay, GetAttestEmployeeInput input, List<GetTimePayrollTransactionsForEmployee_Result> timePayrollTransactionsDay, List<GetTimePayrollScheduleTransactionsForEmployee_Result> timeScheduleTransactionsDay, List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> scheduleTransactionAccountInternalsEmployee, List<AccountDTO> scheduleTransactionAccountStdsEmployee, bool hasSalaryPermission)
        {
            if (timeScheduleTransactionsDay == null)
                return;

            if (timeScheduleTransactionsDay.Any())
            {
                #region Type Absence

                foreach (var timeScheduleTransaction in timeScheduleTransactionsDay)
                {
                    List<AccountDTO> accountInternalItems = scheduleTransactionAccountInternalsEmployee.GetAccountInternals(timeScheduleTransaction.TimePayrollScheduleTransactionId);
                    AccountDTO accountStdItem = scheduleTransactionAccountStdsEmployee?.FirstOrDefault(i => i.AccountId == timeScheduleTransaction.AccountId);

                    AttestPayrollTransactionDTO transactionItem = timeScheduleTransaction.CreateTransactionItem(accountInternalItems, accountStdItem, input.AccountDims, SoeTimePayrollScheduleTransactionType.Absence);
                    if (transactionItem != null)
                    {
                        if (!hasSalaryPermission)
                            transactionItem.ClearFormulasAndAmounts();

                        attestEmployeeDay.HasPayrollScheduleTransactions = true;
                        attestEmployeeDay.CalculateTransactionAggregations(transactionItem);

                        AddEmployeeDayTransaction(attestEmployeeDay, transactionItem, null, null, input.DoMergeTransactions, false);
                    }
                }

                #endregion
            }
            else
            {
                #region Type Schedule (Show only for stamping and if no TimePayrollTransactions exists)

                if (attestEmployeeDay.AutogenTimeblocks)
                    return;

                bool hasTimePayrollTransactions = timePayrollTransactionsDay?.Any(x => !x.IsNetSalary() && !x.IsEmploymentTax() && !x.IsTaxAndNotOptional()) ?? false;
                if (hasTimePayrollTransactions)
                    return;

                foreach (var timeScheduleTransaction in timeScheduleTransactionsDay.Where(t => t.TimeBlockDateId == attestEmployeeDay.TimeBlockDateId && t.Type == (int)SoeTimePayrollScheduleTransactionType.Schedule).ToList())
                {
                    List<AccountDTO> accountInternals = scheduleTransactionAccountInternalsEmployee.GetAccountInternals(timeScheduleTransaction.TimePayrollScheduleTransactionId);
                    AccountDTO accountStd = scheduleTransactionAccountStdsEmployee?.FirstOrDefault(i => i.AccountId == timeScheduleTransaction.AccountId);

                    AttestPayrollTransactionDTO transactionItem = timeScheduleTransaction.CreateTransactionItem(accountInternals, accountStd, input.AccountDims, SoeTimePayrollScheduleTransactionType.Schedule, attestStateName: GetText(5981, "Preliminär"));
                    if (transactionItem != null)
                    {
                        if (input.IsModeTimeAttest && transactionItem.IsExcludedInTime())
                            continue;
                        else if (input.IsModePayrollCalculation && transactionItem.IsExcludedInPayroll())
                            continue;

                        if (!hasSalaryPermission)
                            transactionItem.ClearFormulasAndAmounts();

                        attestEmployeeDay.CalculateTransactionAggregations(transactionItem);

                        AddEmployeeDayTransaction(attestEmployeeDay, transactionItem, null, null, input.DoMergeTransactions, false);

                    }
                }

                #endregion
            }
        }

        private void SetEmployeeDayTransactionSums(AttestEmployeeDayDTO attestEmployeeDay, GetAttestEmployeeInput input, AttestPayrollTransactionDTO transactionItem, bool loadAllSums)
        {
            if (input == null || attestEmployeeDay == null || transactionItem == null || transactionItem.Quantity == 0 || transactionItem.RetroactivePayrollOutcomeId.HasValue)
                return;

            if ((transactionItem.IsAdditionOrDeduction || transactionItem.TimeCodeType == SoeTimeCodeType.AdditionDeduction) && (DoLoad(InputLoadType.SumExpenseRows, forceMobile: true) || DoLoad(InputLoadType.SumExpenseAmount, forceMobile: true)))
            {
                if (input.MobileMyTime && transactionItem.TimeCodeTransactionId.HasValue)
                {
                    int? timeCodeId = attestEmployeeDay.TimeCodeTransactions?.FirstOrDefault(w => w.TimeCodeTransactionId == transactionItem.TimeCodeTransactionId.Value)?.TimeCodeId ?? null;
                    if (timeCodeId != null)
                    {
                        var timeCode = TimeCodeManager.GetTimeCode<TimeCodeAdditionDeduction>(base.ActorCompanyId, timeCodeId.Value);
                        if (timeCode != null && timeCode.HideForEmployee)
                            return;
                    }

                }

                attestEmployeeDay.AddExpense(transactionItem.TimeCodeTransactionId, transactionItem.Amount, transactionItem.PayrollProductShortName);
            }

            if (input.IsWeb)
            {
                TimeSpan quantity = new TimeSpan(0, (int)transactionItem.Quantity, 0);

                //Time
                if (DoLoad(InputLoadType.SumTimeAccumulator, loadAllSums) && transactionItem.IsTimeAccumulator())
                    attestEmployeeDay.SumTimeAccumulator = transactionItem.IsTimeAccumulatorNegate() ? attestEmployeeDay.SumTimeAccumulator.Subtract(CalendarUtility.GetTimeSpanFromMinutes(transactionItem.Quantity)) : attestEmployeeDay.SumTimeAccumulator.Add(CalendarUtility.GetTimeSpanFromMinutes(transactionItem.Quantity));
                if (DoLoad(InputLoadType.SumTimeAccumulatorOverTime, loadAllSums) && transactionItem.IsTimeAccumulatorOverTime())
                    attestEmployeeDay.SumTimeAccumulatorOverTime = attestEmployeeDay.SumTimeAccumulatorOverTime.Add(quantity);
                if (DoLoad(InputLoadType.SumTimeWorkedScheduledTime, loadAllSums) && transactionItem.IsWorkTime())
                    attestEmployeeDay.SumTimeWorkedScheduledTime = attestEmployeeDay.SumTimeWorkedScheduledTime.Add(quantity);

                //Absence
                if (DoLoad(InputLoadType.SumGrossSalaryAbsence, loadAllSums) && transactionItem.IsAbsence())
                    attestEmployeeDay.SumGrossSalaryAbsence = attestEmployeeDay.SumGrossSalaryAbsence.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAbsenceVacation, loadAllSums) && transactionItem.IsAbsenceVacation())
                    attestEmployeeDay.SumGrossSalaryAbsenceVacation = attestEmployeeDay.SumGrossSalaryAbsenceVacation.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAbsenceSick, loadAllSums) && transactionItem.IsAbsenceSickOrWorkInjury())
                    attestEmployeeDay.SumGrossSalaryAbsenceSick = attestEmployeeDay.SumGrossSalaryAbsenceSick.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAbsenceLeaveOfAbsence, loadAllSums) && transactionItem.IsLeaveOfAbsence())
                    attestEmployeeDay.SumGrossSalaryAbsenceLeaveOfAbsence = attestEmployeeDay.SumGrossSalaryAbsenceLeaveOfAbsence.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAbsenceParentalLeave, loadAllSums) && transactionItem.IsParentalLeave())
                    attestEmployeeDay.SumGrossSalaryAbsenceParentalLeave = attestEmployeeDay.SumGrossSalaryAbsenceParentalLeave.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAbsenceTemporaryParentalLeave, loadAllSums) && transactionItem.IsAbsenceTemporaryParentalLeave())
                    attestEmployeeDay.SumGrossSalaryAbsenceTemporaryParentalLeave = attestEmployeeDay.SumGrossSalaryAbsenceTemporaryParentalLeave.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryWeekendSalary, loadAllSums) && transactionItem.IsWeekendSalary())
                    attestEmployeeDay.SumGrossSalaryWeekendSalary = attestEmployeeDay.SumGrossSalaryWeekendSalary.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryDuty, loadAllSums) && transactionItem.IsDutySalary())
                    attestEmployeeDay.SumGrossSalaryDuty = attestEmployeeDay.SumGrossSalaryDuty.Add(quantity);

                if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime, loadAllSums) && transactionItem.IsAddedTime())
                    attestEmployeeDay.SumGrossSalaryAdditionalTime = attestEmployeeDay.SumGrossSalaryAdditionalTime.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime, loadAllSums) && transactionItem.IsAddedTimeCompensation35())
                    attestEmployeeDay.SumGrossSalaryAdditionalTime35 = attestEmployeeDay.SumGrossSalaryAdditionalTime35.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime, loadAllSums) && transactionItem.IsAddedTimeCompensation70())
                    attestEmployeeDay.SumGrossSalaryAdditionalTime70 = attestEmployeeDay.SumGrossSalaryAdditionalTime70.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryAdditionalTime, loadAllSums) && transactionItem.IsAddedTimeCompensation100())
                    attestEmployeeDay.SumGrossSalaryAdditionalTime100 = attestEmployeeDay.SumGrossSalaryAdditionalTime100.Add(quantity);

                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition, loadAllSums) && transactionItem.IsOBAddition())
                    attestEmployeeDay.SumGrossSalaryOBAddition = attestEmployeeDay.SumGrossSalaryOBAddition.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition40, loadAllSums) && transactionItem.IsOBAddition40())
                    attestEmployeeDay.SumGrossSalaryOBAddition40 = attestEmployeeDay.SumGrossSalaryOBAddition40.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition50, loadAllSums) && transactionItem.IsOBAddition50())
                    attestEmployeeDay.SumGrossSalaryOBAddition50 = attestEmployeeDay.SumGrossSalaryOBAddition50.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition57, loadAllSums) && transactionItem.IsOBAddition57())
                    attestEmployeeDay.SumGrossSalaryOBAddition57 = attestEmployeeDay.SumGrossSalaryOBAddition57.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition70, loadAllSums) && transactionItem.IsOBAddition70())
                    attestEmployeeDay.SumGrossSalaryOBAddition70 = attestEmployeeDay.SumGrossSalaryOBAddition70.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition79, loadAllSums) && transactionItem.IsOBAddition79())
                    attestEmployeeDay.SumGrossSalaryOBAddition79 = attestEmployeeDay.SumGrossSalaryOBAddition79.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition100, loadAllSums) && transactionItem.IsOBAddition100())
                    attestEmployeeDay.SumGrossSalaryOBAddition100 = attestEmployeeDay.SumGrossSalaryOBAddition100.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOBAddition113, loadAllSums) && transactionItem.IsOBAddition113())
                    attestEmployeeDay.SumGrossSalaryOBAddition113 = attestEmployeeDay.SumGrossSalaryOBAddition113.Add(quantity);

                if (DoLoad(InputLoadType.SumGrossSalaryOvertime, loadAllSums) && (transactionItem.IsOvertimeCompensation() || transactionItem.IsOvertimeAddition()))
                    attestEmployeeDay.SumGrossSalaryOvertime = attestEmployeeDay.SumGrossSalaryOvertime.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOvertime35, loadAllSums) && (transactionItem.IsOvertimeCompensation35() || transactionItem.IsOvertimeAddition35()))
                    attestEmployeeDay.SumGrossSalaryOvertime35 = attestEmployeeDay.SumGrossSalaryOvertime35.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOvertime50, loadAllSums) && (transactionItem.IsOvertimeCompensation50() || transactionItem.IsOvertimeAddition50()))
                    attestEmployeeDay.SumGrossSalaryOvertime50 = attestEmployeeDay.SumGrossSalaryOvertime50.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOvertime70, loadAllSums) && (transactionItem.IsOvertimeCompensation70() || transactionItem.IsOvertimeAddition70()))
                    attestEmployeeDay.SumGrossSalaryOvertime70 = attestEmployeeDay.SumGrossSalaryOvertime70.Add(quantity);
                if (DoLoad(InputLoadType.SumGrossSalaryOvertime100, loadAllSums) && (transactionItem.IsOvertimeCompensation100() || transactionItem.IsOvertimeAddition100()))
                    attestEmployeeDay.SumGrossSalaryOvertime100 = attestEmployeeDay.SumGrossSalaryOvertime100.Add(quantity);
            }

            bool DoLoad(InputLoadType loadType, bool forceMobile = false)
            {
                if (forceMobile && input.Device == SoeAttestDevice.Mobile)
                    return true;
                return input.DoLoad(loadType, loadAllSums);
            }
        }

        private void SetEmployeeDayTotals(AttestEmployeeDayDTO attestEmployeeDay, TimeBlockDate timeBlockDate, EmployeeGroupDTO employeeGroup, List<TimeTreeScheduleBlockDTO> scheduleItems, List<TimeBlock> timeBlocks, List<int> timeDeviationCausesOvertime, AttestStateDTO attestStateSalaryExportPayrollMin, Account hierarchySettingAccount = null)
        {
            if (attestEmployeeDay == null)
                return;

            List<AttestPayrollTransactionDTO> timePayrollTransactions = attestEmployeeDay.AttestPayrollTransactions.Where(i => !i.IsScheduleTransaction).ToList();
            List<AttestPayrollTransactionDTO> timePayrollTransactionsPresence = timePayrollTransactions.Where(i => i.IsPresence).ToList();
            List<AttestPayrollTransactionDTO> timePayrollTransactionsAbsence = timePayrollTransactions.Where(i => i.IsAbsence).ToList();

            decimal presenseMinutes = (decimal)(attestEmployeeDay.PresenceTime.HasValue ? attestEmployeeDay.PresenceTime.Value.TotalMinutes : 0);
            decimal absenseMinutes = (timePayrollTransactionsAbsence.Sum(i => i.Quantity));

            if (attestEmployeeDay.HasScheduledPlacement)
                attestEmployeeDay.AttestStates.Add(GetAttestStateWarningScheduledPlacement(false).ToDTO());

            attestEmployeeDay.SetAttestStateProperties();
            attestEmployeeDay.HasNoneInitialTransactions = attestEmployeeDay.EvaluateNoneInitialTransactions(timePayrollTransactions, discardExpense: true);
            attestEmployeeDay.IsAbsenceDay = absenseMinutes > 0 && presenseMinutes == 0;
            attestEmployeeDay.HasAbsenceTime = attestEmployeeDay.HasPayrollTransactions && attestEmployeeDay.AttestPayrollTransactions.Any(i => i.IsAbsence());
            attestEmployeeDay.HasOvertime = timeBlocks.ContainsTimeDeviationCause(timeDeviationCausesOvertime);
            attestEmployeeDay.HasTimeWorkReduction = timePayrollTransactions.Any(t => t.EarningTimeAccumulatorId.HasValue && t.EarningTimeAccumulatorId.Value > 0);
            attestEmployeeDay.IsWholedayAbsence = timePayrollTransactionsAbsence.Any() && !timePayrollTransactionsPresence.Any() && (absenseMinutes > 0 && presenseMinutes == 0 || timePayrollTransactions.All(i => i.IsAbsence));
            if (attestEmployeeDay.IsWholedayAbsence && scheduleItems.IsNullOrEmpty())
            {
                attestEmployeeDay.WholedayAbsenseTimeDeviationCauseFromTimeBlock = timeBlockDate.TimeBlock.FirstOrDefault()?.TimeDeviationCauseStartId;
            }
            attestEmployeeDay.SumGrossSalaryAbsenceText = attestEmployeeDay.IsWholedayAbsence ? StringUtility.GetCommaSeparatedString<string>(timePayrollTransactionsAbsence.OrderBy(i => i.PayrollProductName).Select(tpt => tpt.PayrollProductName).Distinct().ToList()) : string.Empty; //Must set empty string for angular to detect changes (text --> null doesnt empty field in angular)
            attestEmployeeDay.HasTimeStampsWithoutTransactions = attestEmployeeDay.HasTimeStampEntries && !attestEmployeeDay.HasTransactions;
            attestEmployeeDay.HasScheduleWithoutTransactions = attestEmployeeDay.CalculateHasScheduleWithoutTransactions(untilNow: true);
            attestEmployeeDay.HasWorkedInsideSchedule = attestEmployeeDay.PresenceInsideScheduleTime.HasValue && attestEmployeeDay.PresenceInsideScheduleTime.Value.TotalMinutes > 0;
            attestEmployeeDay.HasWorkedOutsideSchedule = attestEmployeeDay.PresenceOutsideScheduleTime.HasValue && attestEmployeeDay.PresenceOutsideScheduleTime.Value.TotalMinutes > 0;
            attestEmployeeDay.PresenceBreakMinutes = !attestEmployeeDay.IsWholedayAbsence && attestEmployeeDay.PresenceTime.HasValue ? attestEmployeeDay.PresenceBreakMinutes : null;
            attestEmployeeDay.IsAttested = attestEmployeeDay.EvaluateIsDayAttested(attestStateSalaryExportPayrollMin, timePayrollTransactions, discardExpense: true);
            if (!attestEmployeeDay.IsAttested)
                attestEmployeeDay.IsToBeAttested = attestEmployeeDay.HasTransactions || attestEmployeeDay.CalculateHasScheduleWithoutTransactions(untilNow: false) || attestEmployeeDay.HasTimeStampsWithoutTransactions;

            if (timeBlockDate != null)
            {
                attestEmployeeDay.TimeBlockDateStatus = timeBlockDate.Status;
                attestEmployeeDay.TimeBlockDateStampingStatus = timeBlockDate.StampingStatus;
                attestEmployeeDay.IsGeneratingTransactions = attestEmployeeDay.IsGeneratingTransactions || timeBlockDate.Status == (int)SoeTimeBlockDateStatus.Regenerating;
                attestEmployeeDay.DiscardedBreakEvaluation = attestEmployeeDay.DiscardedBreakEvaluation || timeBlockDate.DoDiscardedBreakEvaluation(key: hierarchySettingAccount?.AccountId);
                attestEmployeeDay.HasUnhandledExtraShiftChanges = timeBlockDate.HasUnhandledExtraShiftChanges;
                attestEmployeeDay.HasUnhandledShiftChanges = timeBlockDate.HasUnhandledShiftChanges;
            }
            attestEmployeeDay.CalculateHasDeviations();
            SetEmployeeDayFlagsDependedOnScheduleHole(attestEmployeeDay, employeeGroup, scheduleItems);
        }

        private void SetEmployeeWeekTotals(List<AttestEmployeeDayDTO> attestEmployeeDays)
        {
            if (attestEmployeeDays.IsNullOrEmpty())
                return;

            foreach (var attestEmployeeDaysByWeek in attestEmployeeDays.GroupBy(day => day.WeekNr))
            {
                if (attestEmployeeDaysByWeek.Any(day => day.HasOvertime))
                    attestEmployeeDaysByWeek.ToList().ForEach(day => day.HasPeriodOvertime = true);
                if (attestEmployeeDaysByWeek.Any(day => day.HasTimeWorkReduction))
                    attestEmployeeDaysByWeek.ToList().ForEach(day => day.HasPeriodTimeWorkReduction = true);
            }
        }

        private void SetEmployeeUnhandledShiftChanges(bool doLoadOvertimeDays, bool doLoadSickDays, List<AttestEmployeePeriodDTO> attestEmployeePeriods)
        {
            if (attestEmployeePeriods.IsNullOrEmpty())
                return;

            doLoadOvertimeDays = doLoadOvertimeDays && attestEmployeePeriods.HasOvertime();
            doLoadSickDays = doLoadSickDays && attestEmployeePeriods.HasAbsenceSick();
            if (!doLoadOvertimeDays && !doLoadSickDays)
                return;

            List<EmployeeGroup> employeeGroups = doLoadSickDays ? GetEmployeeGroupsFromCache(base.ActorCompanyId) : null;

            foreach (AttestEmployeePeriodDTO attestEmployeePeriod in attestEmployeePeriods)
            {
                bool doLoadOvertimeDaysForEmployee = doLoadOvertimeDays && attestEmployeePeriod.HasOvertime;
                bool doLoadSickDaysForEmployee = doLoadSickDays && attestEmployeePeriod.HasAbsenceSick();
                if (!doLoadOvertimeDaysForEmployee && !doLoadSickDaysForEmployee)
                    continue;

                DateTime dateFrom = CalendarUtility.GetFirstDateOfWeek(attestEmployeePeriod.StartDate);
                DateTime dateTo = CalendarUtility.GetFirstDateOfWeek(attestEmployeePeriod.StopDate);
                var weeks = CalendarUtility.GetWeeks(dateFrom, dateTo);
                List<TimeBlockDate> days = TimeBlockManager.GetTimeBlockDates(attestEmployeePeriod.EmployeeId, dateFrom, dateTo);

                TimeUnhandledShiftChangesEmployeeDTO unhandledEmployee = new TimeUnhandledShiftChangesEmployeeDTO(attestEmployeePeriod.EmployeeId);
                LoadOvertimeDays();
                LoadSickDays();
                if (unhandledEmployee.HasDays)
                    attestEmployeePeriod.UnhandledEmployee = unhandledEmployee;

                void LoadOvertimeDays()
                {
                    if (!doLoadOvertimeDaysForEmployee || !days.HasUnhandledShiftChanges(out List<TimeBlockDate> unhandledDays))
                        return;

                    foreach (var week in weeks)
                    {
                        if (!unhandledDays.HasUnhandledShiftChanges(out List<TimeBlockDate> unhandledDaysForWeek, week))
                            continue;
                        if (!attestEmployeePeriod.HasOvertimeInWeek(week))
                            continue;

                        unhandledEmployee.AddDaysToWeek(week, shiftDays: unhandledDaysForWeek.GetUniqueById());
                    }
                }
                void LoadSickDays()
                {
                    if (!doLoadSickDaysForEmployee || !days.HasUnhandledExtraShiftChanges(out List<TimeBlockDate> unhandledDays))
                        return;

                    Employee employee = EmployeeManager.GetEmployeeWithEmploymentAndEmploymentChangeBatch(attestEmployeePeriod.EmployeeId, base.ActorCompanyId);
                    if (employee == null)
                        return;

                    foreach (var week in weeks)
                    {
                        EmployeeGroup employeeGroup = employee.GetEmployeeGroup(week.WeekStart, week.WeekStop, employeeGroups);
                        if (employeeGroup == null || !employeeGroup.UseQualifyingDayCalculationRuleWorkTimeWeekPlusExtraShifts())
                            continue;
                        if (!unhandledDays.HasUnhandledExtraShiftChanges(out List<TimeBlockDate> unhandledDaysForWeek, week))
                            continue;
                        if (!attestEmployeePeriod.HasAbsenceSickInWeek(week))
                            continue;

                        unhandledEmployee.AddDaysToWeek(week, extraShiftDays: unhandledDaysForWeek.GetUniqueById());
                    }
                }
            }
        }

        private void SetEmployeeUnhandledShiftChanges(bool loadUnhandledOvertimeDays, bool doLoadSickDays, List<AttestEmployeeDayDTO> attestEmployeeDays, List<TimeBlockDate> days, Dictionary<DateTime, EmployeeGroupDTO> dateEmployeeGroup)
        {
            if (attestEmployeeDays.IsNullOrEmpty())
                return;

            loadUnhandledOvertimeDays = loadUnhandledOvertimeDays && attestEmployeeDays.HasUnhandledShiftChanges() && attestEmployeeDays.HasOvertime();
            doLoadSickDays = doLoadSickDays && attestEmployeeDays.HasUnhandledExtraShiftChanges() && attestEmployeeDays.HasAbsenceSick() && dateEmployeeGroup.Values.Any(eg => eg.UseQualifyingDayCalculationRuleWorkTimeWeekPlusExtraShifts());
            if (!loadUnhandledOvertimeDays && !doLoadSickDays)
                return;

            int employeeId = attestEmployeeDays.First().EmployeeId;
            DateTime dateFrom = CalendarUtility.GetFirstDateOfWeek(attestEmployeeDays.Min(day => day.Date));
            DateTime dateTo = CalendarUtility.GetLastDateOfWeek(attestEmployeeDays.Max(day => day.Date));
            List<DateTime> missingDates = CalendarUtility.GetDates(dateFrom, dateTo).Except(days.Select(tbd => tbd.Date)).ToList();
            if (missingDates.Any())
                days.AddRange(TimeBlockManager.GetTimeBlockDates(employeeId, missingDates));
            var weeks = CalendarUtility.GetWeeks(dateFrom, dateTo);

            TimeUnhandledShiftChangesEmployeeDTO unhandledEmployee = new TimeUnhandledShiftChangesEmployeeDTO(employeeId);
            AddOvertimeDays();
            AddSickDays();
            if (unhandledEmployee.HasDays)
                attestEmployeeDays.First().UnhandledEmployee = unhandledEmployee;

            void AddOvertimeDays()
            {
                if (!loadUnhandledOvertimeDays || !days.HasUnhandledShiftChanges(out List<TimeBlockDate> unhandledDays))
                    return;

                foreach (var week in weeks)
                {
                    if (!unhandledDays.HasUnhandledShiftChanges(out List<TimeBlockDate> unhandledDaysForWeek, week))
                        continue;
                    if (!attestEmployeeDays.Filter(week.WeekStart, week.WeekStop).HasOvertime())
                        continue;

                    unhandledEmployee.AddDaysToWeek(week, shiftDays: unhandledDaysForWeek.GetUniqueById());
                }
            }
            void AddSickDays()
            {
                if (!doLoadSickDays || !days.HasUnhandledExtraShiftChanges(out List<TimeBlockDate> unhandledDays))
                    return;

                foreach (var week in weeks)
                {
                    EmployeeGroupDTO employeeGroup = dateEmployeeGroup.FirstOrDefault(eg => eg.Value != null && eg.Key >= week.WeekStart && eg.Key <= week.WeekStop).Value;
                    if (employeeGroup == null || !employeeGroup.UseQualifyingDayCalculationRuleWorkTimeWeekPlusExtraShifts())
                        continue;
                    if (!unhandledDays.HasUnhandledExtraShiftChanges(out List<TimeBlockDate> unhandledDaysForWeek, week))
                        continue;
                    if (!attestEmployeeDays.Filter(week.WeekStart, week.WeekStop).HasAbsenceSick())
                        continue;

                    unhandledEmployee.AddDaysToWeek(week, extraShiftDays: unhandledDaysForWeek.GetUniqueById());
                }
            }
        }

        private void SetEmployeeDayFlagsDependedOnScheduleHole(AttestEmployeeDayDTO attestEmployeeDay, EmployeeGroupDTO employeeGroup, List<TimeTreeScheduleBlockDTO> scheduleItems)
        {
            if (employeeGroup?.AutogenTimeblocks ?? false)
                return;
            if (attestEmployeeDay.HasScheduleWithoutTransactions && attestEmployeeDay.HasTimeStampsWithoutTransactions)
                return;

            List<WorkIntervalDTO> shifts = scheduleItems.GetShiftsBasedOnScheduleHoles();
            if (shifts.Count >= 2)
            {
                if (!attestEmployeeDay.HasTimeStampsWithoutTransactions && DoAnyShiftHaveTimeStampsWithoutTransactions(attestEmployeeDay, shifts))
                    attestEmployeeDay.HasTimeStampsWithoutTransactions = true;
                if (!attestEmployeeDay.HasScheduleWithoutTransactions && DoAnyShiftHaveScheduleWithoutTransactions(attestEmployeeDay, shifts) && CalendarUtility.IsBeforeNow(attestEmployeeDay.Date, attestEmployeeDay.ScheduleStopTime))
                    attestEmployeeDay.HasScheduleWithoutTransactions = true;
            }
        }

        private void SetEmployeeDayLended(AttestEmployeeDayDTO employeeDay, Dictionary<DateTime, bool> validDatesByScheduleAndTransactions)
        {
            if (employeeDay == null || validDatesByScheduleAndTransactions == null)
                return;

            employeeDay.IsPartlyAdditional = validDatesByScheduleAndTransactions.ContainsKey(employeeDay.Date) && !validDatesByScheduleAndTransactions[employeeDay.Date];
            employeeDay.IsCompletelyAdditional = !validDatesByScheduleAndTransactions.ContainsKey(employeeDay.Date);
        }

        private void SetEmployeeDayTransactionsForDayOutOfRange(AttestEmployeeDayDTO attestEmployeeDay, GetAttestEmployeeInput input, List<AccountDTO> payrollTransactionAccountStdsEmployee, List<GetTimePayrollTransactionAccountsForEmployee_Result> payrollTransactionAccountInternalsEmployee, List<GetTimePayrollTransactionsForEmployee_Result> timePayrollTransactionItemsForDay, List<GetAttestTransitionLogsForEmployeeResult> attestTransitionLogsEmployee)
        {
            if (timePayrollTransactionItemsForDay.IsNullOrEmpty())
                return;

            foreach (var timePayrollTransactionItem in timePayrollTransactionItemsForDay)
            {
                List<AccountDTO> accountInternals = payrollTransactionAccountInternalsEmployee.GetAccountInternals(timePayrollTransactionItem.TimePayrollTransactionId);
                AccountDTO accountStd = payrollTransactionAccountStdsEmployee?.FirstOrDefault(i => i.AccountId == timePayrollTransactionItem.AccountId);
                AttestPayrollTransactionDTO transactionItem = timePayrollTransactionItem.CreateTransactionItem(accountInternals, accountStd, input.AccountDims);
                if (transactionItem != null)
                {
                    attestEmployeeDay.HasPayrollTransactions = true;
                    attestEmployeeDay.CalculateTransactionAggregations(transactionItem);
                    if (!attestEmployeeDay.AttestStates.Any(a => a.AttestStateId == transactionItem.AttestStateId))
                        attestEmployeeDay.AttestStates.Add(transactionItem.GetAttestState());

                    AddEmployeeDayTransaction(attestEmployeeDay, transactionItem, timePayrollTransactionItemsForDay, attestTransitionLogsEmployee, input.DoMergeTransactions, input.DoLoad(InputLoadType.AttestTransitionLog));
                }
            }
        }

        private void SetEmployeeDayGrossNetCost(List<AttestEmployeeDayDTO> attestEmployeeDays, GetAttestEmployeeInput input)
        {
            if (input == null || attestEmployeeDays.IsNullOrEmpty())
                return;

            List<GrossNetCostDTO> grossNetCosts = new List<GrossNetCostDTO>();
            foreach (var item in attestEmployeeDays)
            {
                grossNetCosts.AddRange(item.GrossNetCosts);
            }

            TimeScheduleManager.CalculateGrossNetAndCost(grossNetCosts, input.ActorCompanyId, input.UserId, input.RoleId, input.DoGetOnlyActive, includeEmploymentTaxAndSupplementChargeCost: input.DoCalculateEmploymentTaxAndSupplementChargeCost, employeeGroups: input.EmployeeGroups, payrollGroups: input.PayrollGroups);
        }

        private bool IsAttestEmployeeMySelf(CompEntities entities, GetAttestEmployeeInput input, Employee employee)
        {
            if (input == null || employee == null)
                return false;

            Employee employeeForUser = input.EmployeeForUser ?? EmployeeManager.GetEmployeeForUser(entities, input.UserId, input.ActorCompanyId);
            bool isMySelf = employee.EmployeeId == employeeForUser?.EmployeeId;
            if (isMySelf)
                input.ResetDoNotShowDaysOutsideEmployeeAccount();

            return isMySelf;
        }

        public List<GetTimePayrollTransactionsForEmployee_Result> FilterTimePayrollTransactionsOnAccountingType(List<GetTimePayrollTransactionsForEmployee_Result> transactions, List<GetTimePayrollTransactionAccountsForEmployee_Result> accountInternals, List<AccountInternalDTO> validAccountInternals)
        {
            if (accountInternals == null || validAccountInternals.IsNullOrEmpty() || transactions.IsNullOrEmpty())
                return transactions;

            List<GetTimePayrollTransactionsForEmployee_Result> filtered = new List<GetTimePayrollTransactionsForEmployee_Result>();
            foreach (var trans in transactions)
            {
                var accounts = accountInternals.Where(w => w.TimePayrollTransactionId == trans.TimePayrollTransactionId).ToList();
                if (accounts.ValidOnFiltered(validAccountInternals))
                    filtered.Add(trans);
            }

            return filtered;
        }

        public List<GetTimePayrollScheduleTransactionsForEmployee_Result> FilterTimePayrollScheduleTransactionsOnccountingType(TermGroup_EmployeeSelectionAccountingType employeeSelectionAccountingType, List<GetTimePayrollScheduleTransactionsForEmployee_Result> transactions, List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> accountInternals, List<AccountInternalDTO> validAccountInternals)
        {
            if (accountInternals == null || validAccountInternals.IsNullOrEmpty() || transactions.IsNullOrEmpty())
                return transactions;

            List<GetTimePayrollScheduleTransactionsForEmployee_Result> filtered = new List<GetTimePayrollScheduleTransactionsForEmployee_Result>();
            foreach (var trans in transactions)
            {
                var accounts = accountInternals.Where(w => w.TimePayrollScheduleTransactionId == trans.TimePayrollScheduleTransactionId).ToList();
                if (accounts.ValidOnFiltered(validAccountInternals))
                    filtered.Add(trans);
            }

            return filtered;
        }

        public List<TimeTreeScheduleBlockDTO> FilterBlocksOnEmployeeAndDayOnAccountingType(TermGroup_EmployeeSelectionAccountingType employeeSelectionAccountingType, List<TimeTreeScheduleBlockDTO> scheduleBlocksOnDay, List<GetTimeScheduleTemplateBlockAccountsForEmployeeResult> templateBlockAccountsEmployee, List<AccountInternalDTO> validAccountInternals)
        {
            if (validAccountInternals.IsNullOrEmpty() || templateBlockAccountsEmployee.IsNullOrEmpty())
                return scheduleBlocksOnDay;

            if (employeeSelectionAccountingType != TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount &&
                employeeSelectionAccountingType != TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock)
                return scheduleBlocksOnDay;

            List<TimeTreeScheduleBlockDTO> filtered = new List<TimeTreeScheduleBlockDTO>();
            List<int> selectionAccountIds = validAccountInternals.Select(s => s.AccountId).ToList();

            foreach (var scheduleBlock in scheduleBlocksOnDay)
            {
                if (employeeSelectionAccountingType == TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlockAccount)
                {
                    var accounts = templateBlockAccountsEmployee.Where(w => w.TimeScheduleTemplateBlockId == scheduleBlock.TimeScheduleTemplateBlockId).ToList();
                    if (accounts.ValidOnFiltered(validAccountInternals))
                    {
                        filtered.Add(scheduleBlock);
                        filtered.AddRange(scheduleBlock.GetOverlappedBreaks(scheduleBlocksOnDay));
                    }
                }
                else
                {
                    if (!scheduleBlock.IsBreak && scheduleBlock.AccountId.HasValue && selectionAccountIds.Contains(scheduleBlock.AccountId.Value))
                    {
                        filtered.Add(scheduleBlock);
                        filtered.AddRange(scheduleBlock.GetOverlappedBreaks(scheduleBlocksOnDay.Where(w => w.IsBreak).ToList()));
                    }
                }
            }

            return filtered.Distinct().ToList();
        }

        private bool DoAnyShiftHaveTimeStampsWithoutTransactions(AttestEmployeeDayDTO attestEmployeeDay, List<WorkIntervalDTO> shifts)
        {
            if (shifts.IsNullOrEmpty() || attestEmployeeDay == null || attestEmployeeDay.TimeStampEntrys.IsNullOrEmpty())
                return false;

            List<DateTime> stamps = attestEmployeeDay.TimeStampEntrys.Select(tse => CalendarUtility.GetScheduleTime(tse.Time, tse.Time.Date, tse.Time.Date)).ToList();

            int shiftNr = 1;
            foreach (WorkIntervalDTO shift in shifts)
            {
                if (HasStamping(shift) && !HasTransaction(shift))
                    return true;
                shiftNr++;
            }

            return false;

            bool HasStamping(WorkIntervalDTO shift)
            {
                DateTime startTime = shiftNr > 1 ? shift.StartTime : DateTime.MinValue;
                DateTime stopTime = shiftNr < shifts.Count ? shift.StopTime : DateTime.MaxValue;
                return stamps.Any(stamp => stamp >= startTime && stamp <= stopTime);
            }
            bool HasTransaction(WorkIntervalDTO shift)
            {
                if (attestEmployeeDay.AttestPayrollTransactions.IsNullOrEmpty())
                    return false;

                var (startTime, stopTime) = shifts.GetStartAndStopTime(shift, shiftNr);
                return attestEmployeeDay.HasAnyTransaction(startTime, stopTime);
            }
        }

        private bool DoAnyShiftHaveScheduleWithoutTransactions(AttestEmployeeDayDTO attestEmployeeDay, List<WorkIntervalDTO> shifts)
        {
            if (shifts.IsNullOrEmpty() || attestEmployeeDay == null)
                return false;

            int shiftNr = 1;
            foreach (WorkIntervalDTO shift in shifts)
            {
                if (!HasTransaction(shift))
                    return true;
                shiftNr++;
            }

            bool HasTransaction(WorkIntervalDTO shift)
            {
                if (attestEmployeeDay.AttestPayrollTransactions.IsNullOrEmpty())
                    return false;

                var (startTime, stopTime) = shifts.GetStartAndStopTime(shift, shiftNr);
                return attestEmployeeDay.HasAnyTransaction(startTime, stopTime);
            }

            return false;
        }

        private void AddEmployeeDayTransaction(AttestEmployeeDayDTO day, AttestPayrollTransactionDTO transactionItem, List<GetTimePayrollTransactionsForEmployee_Result> transactions, List<GetAttestTransitionLogsForEmployeeResult> attestTransitionLogItemsEmployee, bool mergeTransactions, bool loadAttestTransitionLogs)
        {
            if (transactionItem.TimeBlockId.HasValue && transactions != null)
            {
                transactionItem.IsPresence = transactions.IsPayrollTransactionPresence(transactionItem.TimePayrollTransactionId);
                transactionItem.IsAbsence = transactions.IsPayrollTransactionAbsence(transactionItem.TimePayrollTransactionId);
            }

            if (!mergeTransactions || !day.MatchTransactions(transactionItem))
            {
                if (!day.HasComment)
                    day.HasComment = transactionItem.HasComment;

                if (loadAttestTransitionLogs && attestTransitionLogItemsEmployee != null)
                    transactionItem.AttestTransitionLogs = attestTransitionLogItemsEmployee.GetAttestTransitionLogs(transactionItem.TimePayrollTransactionId);

                transactionItem.AttestItemUniqueId = day.UniqueId;
                day.AttestPayrollTransactions.Add(transactionItem);
            }
        }

        private AttestEmployeeDayShiftDTO CreateEmployeeDayScheduleShift(AttestEmployeeDayDTO attestEmployeeDay, TimeTreeScheduleBlockDTO scheduleItem, List<TimeDeviationCause> timeDeviationCauses, TermGroup_TimeScheduleTemplateBlockType type)
        {
            return new AttestEmployeeDayShiftDTO()
            {
                EmployeeId = attestEmployeeDay.EmployeeId,
                TimeScheduleTemplateBlockId = scheduleItem.TimeScheduleTemplateBlockId,
                ShiftTypeId = scheduleItem.ShiftTypeId ?? 0,
                ShiftTypeName = scheduleItem.ShiftTypeName,
                ShiftTypeColor = scheduleItem.ShiftTypeColor,
                ShiftTypeDescription = scheduleItem.ShiftTypeDescription,
                TimeScheduleTypeId = scheduleItem.TimeScheduleTypeId ?? 0,
                TimeScheduleTypeName = scheduleItem.TimeScheduleTypeName,
                TimeDeviationCauseId = scheduleItem.TimeDeviationCauseId,
                TimeDeviationCauseName = timeDeviationCauses?.FirstOrDefault(i => i.TimeDeviationCauseId == scheduleItem.TimeDeviationCauseId)?.Name ?? string.Empty,
                AccountId = scheduleItem.AccountId,
                StartTime = CalendarUtility.MergeDateAndTime(scheduleItem.Date.Value.AddDays((scheduleItem.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), scheduleItem.StartTime),
                StopTime = CalendarUtility.MergeDateAndTime(scheduleItem.Date.Value.AddDays((scheduleItem.StopTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), scheduleItem.StopTime),
                Link = scheduleItem.Link,
                Type = type,
            };
        }

        private List<TimeTreeScheduleBlockDTO> GetAttestEmployeeSchedule(CompEntities entities, GetAttestEmployeeInput input, List<ShiftType> shiftTypes, List<TimeScheduleType> timeScheduleTypes)
        {
            List<TimeTreeScheduleBlockDTO> scheduleItems = (from b in entities.TimeScheduleTemplateBlock
                                                            where b.EmployeeId == input.EmployeeId &&
                                                            b.Date >= input.StartDate &&
                                                            b.Date <= input.StopDate &&
                                                            b.State == (int)SoeEntityState.Active
                                                            select new TimeTreeScheduleBlockDTO()
                                                            {
                                                                EmployeeId = b.EmployeeId,
                                                                TimeScheduleTemplateBlockId = b.TimeScheduleTemplateBlockId,
                                                                TimeScheduleTemplatePeriodId = b.TimeScheduleTemplatePeriodId,
                                                                TimeScheduleScenarioHeadId = b.TimeScheduleScenarioHeadId,
                                                                TimeDeviationCauseId = b.TimeDeviationCauseId,
                                                                TimeScheduleTypeId = b.TimeScheduleTypeId,
                                                                TimeCodeId = b.TimeCodeId,
                                                                ShiftTypeId = b.ShiftTypeId,
                                                                AccountId = b.AccountId,
                                                                RecalculateTimeRecordId = b.RecalculateTimeRecordId,
                                                                Date = b.Date,
                                                                StartTime = b.StartTime,
                                                                StopTime = b.StopTime,
                                                                Type = b.Type,
                                                                Link = b.Link,
                                                                BreakType = b.BreakType,
                                                                BreakNumber = b.BreakNumber,
                                                                IsBreak = b.BreakType > (int)SoeTimeScheduleTemplateBlockBreakType.None,
                                                                IsPreliminary = b.IsPreliminary,
                                                                ShiftUserStatus = b.ShiftUserStatus,
                                                                AbsenceType = b.AbsenceType,
                                                            }).ToList();

            if (input.TimeScheduleScenarioHeadId.HasValue)
                scheduleItems = scheduleItems.Where(b => b.TimeScheduleScenarioHeadId == input.TimeScheduleScenarioHeadId.Value).ToList();
            else
                scheduleItems = scheduleItems.Where(b => !b.TimeScheduleScenarioHeadId.HasValue).ToList();
            scheduleItems = scheduleItems.Where(b => b.IsSchedule() || b.IsStandby() || (input.DoGetOnDuty && b.IsOnDuty())).ToList();
            if (scheduleItems.Any())
            {
                LoadAttestEmployeeScheduleRecaculateTimeRecord(entities, scheduleItems);
                LoadAttestEmployeeScheduleTimeCode(entities, scheduleItems);
                LoadAttestEmployeeScheduleShiftType(scheduleItems, shiftTypes);
                LoadAttestEmployeeScheduleTimeScheduleType(scheduleItems, timeScheduleTypes);
            }
            return scheduleItems;
        }

        private void LoadAttestEmployeeScheduleRecaculateTimeRecord(CompEntities entities, List<TimeTreeScheduleBlockDTO> scheduleItems)
        {
            List<int> recalculateTimeRecordIds = scheduleItems.Where(i => i.RecalculateTimeRecordId.HasValue).Select(i => i.RecalculateTimeRecordId.Value).Distinct().ToList();
            if (recalculateTimeRecordIds.IsNullOrEmpty())
                return;

            List<RecalculateTimeRecord> recalculateTimeRecords = entities.RecalculateTimeRecord.Where(i => recalculateTimeRecordIds.Contains(i.RecalculateTimeRecordId)).ToList();
            foreach (RecalculateTimeRecord recalculateTimeRecord in recalculateTimeRecords)
            {
                foreach (TimeTreeScheduleBlockDTO scheduleItem in scheduleItems.Where(i => i.RecalculateTimeRecordId == recalculateTimeRecord.RecalculateTimeRecordId))
                {
                    scheduleItem.RecalculateTimeRecordStatus = recalculateTimeRecord.Status;
                }
            }

            foreach (TimeTreeScheduleBlockDTO scheduleItem in scheduleItems.Where(i => !i.RecalculateTimeRecordId.HasValue))
            {
                scheduleItem.RecalculateTimeRecordStatus = (int)TermGroup_RecalculateTimeRecordStatus.None;
            }
        }

        private void LoadAttestEmployeeScheduleTimeCode(CompEntities entities, List<TimeTreeScheduleBlockDTO> scheduleItems)
        {
            List<int> timeCodeIds = scheduleItems.Select(i => i.TimeCodeId).Distinct().ToList();
            if (timeCodeIds.IsNullOrEmpty())
                return;

            List<TimeCode> timeCodes = entities.TimeCode.Where(i => timeCodeIds.Contains(i.TimeCodeId)).ToList();
            foreach (TimeCode timeCode in timeCodes)
            {
                int breakMinutes = timeCode is TimeCodeBreak ? (timeCode as TimeCodeBreak).DefaultMinutes : 0;

                foreach (TimeTreeScheduleBlockDTO scheduleItem in scheduleItems.Where(i => i.TimeCodeId == timeCode.TimeCodeId))
                {
                    scheduleItem.BreakMinutes = breakMinutes;
                    scheduleItem.TimeCodeType = timeCode.Type;
                    scheduleItem.TimeCodeName = timeCode.Name;
                }
            }
        }

        private void LoadAttestEmployeeScheduleTimeScheduleType(List<TimeTreeScheduleBlockDTO> scheduleItems, List<TimeScheduleType> timeScheduleTypes)
        {
            if (timeScheduleTypes.IsNullOrEmpty())
                return;

            foreach (var scheduleItemsByTimeScheduleType in scheduleItems.Where(i => i.TimeScheduleTypeId.HasValue).GroupBy(i => i.TimeScheduleTypeId.Value))
            {
                TimeScheduleType timeScheduleType = timeScheduleTypes.FirstOrDefault(i => i.TimeScheduleTypeId == scheduleItemsByTimeScheduleType.Key);
                if (timeScheduleType == null)
                    continue;

                foreach (TimeTreeScheduleBlockDTO scheduleItem in scheduleItemsByTimeScheduleType)
                {
                    scheduleItem.TimeScheduleTypeName = timeScheduleType.Name;
                    scheduleItem.IsNotScheduleTime = timeScheduleType.IsNotScheduleTime;
                }
            }
        }

        private void LoadAttestEmployeeScheduleShiftType(List<TimeTreeScheduleBlockDTO> scheduleItems, List<ShiftType> shiftTypes)
        {
            if (shiftTypes.IsNullOrEmpty())
                return;

            foreach (var scheduleItemsByTimeScheduleType in scheduleItems.Where(i => i.ShiftTypeId.HasValue).GroupBy(i => i.ShiftTypeId.Value))
            {
                ShiftType shiftType = shiftTypes.FirstOrDefault(i => i.ShiftTypeId == scheduleItemsByTimeScheduleType.Key);
                if (shiftType == null)
                    continue;

                foreach (TimeTreeScheduleBlockDTO scheduleItem in scheduleItemsByTimeScheduleType)
                {
                    scheduleItem.ShiftTypeName = shiftType.Name;
                    scheduleItem.ShiftTypeColor = shiftType.Color;
                    scheduleItem.ShiftTypeDescription = shiftType.Description;
                }
            }
        }

        private (Employee, EmployeeAuthModelRepository, AccountRepository, CategoryRepository) LoadAttestDayEmployee(CompEntities entities, GetAttestEmployeeInput input)
        {
            if (input == null)
                return (null, null, null, null);

            EmployeeAuthModelRepository repository = null;

            Employee employee = input.Employee;
            if (employee == null)
            {
                //For now, we dont have a good way to validate extra (when input.FilterAccountIds has content)
                if (input.ValidateEmployee && input.FilterAccountIds.IsNullOrEmpty())
                    employee = EmployeeManager.GetEmployeesForUsersAttestRoles(entities, out repository, base.ActorCompanyId, base.UserId, base.RoleId, dateFrom: input.StartDate, dateTo: input.StopDate, employeeFilter: input.EmployeeId.ObjToList(), active: true, getVacant: input.DoGetHidden, getHidden: input.DoGetHidden)?.FirstOrDefault();
                else
                    employee = EmployeeManager.GetEmployee(entities, input.EmployeeId, input.ActorCompanyId, onlyActive: false, loadEmployment: true, getHidden: input.DoGetHidden);
            }

            if (repository is AccountRepository accountRepository)
                return (employee, repository, accountRepository, null);
            else if (repository is CategoryRepository categoryRepository)
                return (employee, repository, null, categoryRepository);
            else
                return (employee, repository, null, null);
        }

        private (List<DateTime> validDates, Dictionary<DateTime, EmployeeSchedule> dateEmployeeScheduleDict, Dictionary<int, List<GetTemplateSchedule_Result>> templatePeriodTemplateScheduleDict) LoadEmployeeDaysValidByEmployeeAuth(CompEntities entities, GetAttestEmployeeInput input, Employee employee, List<EmployeeSchedule> employeeSchedules, AccountRepository accountRepository, CategoryRepository categoryRepository, bool useAccountHierarchy, bool isMySelf)
        {
            List<DateTime> validDates = new List<DateTime>();
            Dictionary<DateTime, EmployeeSchedule> dateEmployeeScheduleDict = new Dictionary<DateTime, EmployeeSchedule>();
            Dictionary<int, List<GetTemplateSchedule_Result>> templateHeadScheduleDict = new Dictionary<int, List<GetTemplateSchedule_Result>>();

            if (input != null && employee != null && (input.IsScenario || !employeeSchedules.IsNullOrEmpty()))
            {
                List<EmployeeAccount> employeeAccountsForEmployee = null;
                List<CompanyCategoryRecord> categoryRecordsForEmployee = null;

                if (useAccountHierarchy && !isMySelf && !employee.Hidden)
                {
                    if (accountRepository == null)
                        accountRepository = AccountManager.GetAccountHierarchyRepositoryByUserSetting(entities, input.ActorCompanyId, input.RoleId, input.UserId, input.StartDate, input.StopDate);
                    if (accountRepository != null && accountRepository.EmployeeAccounts == null)
                        accountRepository.SetEmployeeAccounts(GetEmployeeAccountsFromCache(entities, CacheConfig.Company(input.ActorCompanyId, 60)));
                    employeeAccountsForEmployee = accountRepository?.GetEmployeeAccounts(employee.EmployeeId) ?? EmployeeManager.GetEmployeeAccounts(entities, input.ActorCompanyId, employee.EmployeeId);
                }
                else if (!useAccountHierarchy)
                {
                    categoryRecordsForEmployee = input.CategoryRecordsForEmployee ?? CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, input.ActorCompanyId, true, input.StartDate, input.StopDate);
                }

                DateTime currentDate = input.StartDate.Date;
                int totalDays = input.StopDate.Subtract(input.StartDate).Days + 1;
                for (int day = 1; day <= totalDays; day++)
                {
                    try
                    {
                        if (!employee.HasEmployment(currentDate))
                            continue;

                        if (!isMySelf && !employee.Hidden)
                        {
                            if (useAccountHierarchy)
                            {
                                if (!input.HasFilterAccountIds && accountRepository != null && employeeAccountsForEmployee != null)
                                {
                                    List<AccountDTO> validAccounts = employeeAccountsForEmployee.GetValidAccounts(employee.EmployeeId, currentDate, currentDate, accountRepository.AllAccountInternalsDict, accountRepository.GetAccountsDict(true), onlyDefaultAccounts: !input.DoGetSecondaryAccounts);
                                    if (validAccounts.IsNullOrEmpty())
                                        continue;
                                    if (!accountRepository.HasAnyAttestRoleAnyAccount(employee.EmployeeId, employeeAccountsForEmployee, currentDate, currentDate, onlyDefaultAccounts: !input.DoGetSecondaryAccounts))
                                        continue;
                                }
                            }
                            else
                            {
                                if (!input.HasFilterAccountIds && !employee.Hidden)
                                {
                                    List<CompanyCategoryRecord> validCategoryRecordsForEmployee = categoryRecordsForEmployee.GetCategoryRecords(employee.EmployeeId, currentDate);
                                    if (validCategoryRecordsForEmployee.IsNullOrEmpty())
                                        continue;
                                    if (categoryRepository != null && !categoryRepository.HasAnyAttestRoleAnyCategory(validCategoryRecordsForEmployee, date: currentDate))
                                        continue;
                                }
                            }
                        }
                        if (!input.IsScenario)
                        {
                            EmployeeSchedule employeeSchedule = employeeSchedules.Where(es => currentDate >= es.StartDate && currentDate <= es.StopDate).OrderBy(es => es.Created).FirstOrDefault();
                            if (employeeSchedule?.TimeScheduleTemplateHead == null || employeeSchedule.TimeScheduleTemplateHead.TimeScheduleTemplatePeriod.IsNullOrEmpty())
                                continue;

                            if (input.DoLoad(InputLoadType.TemplateSchedule) && !templateHeadScheduleDict.ContainsKey(employeeSchedule.TimeScheduleTemplateHeadId))
                                templateHeadScheduleDict.Add(employeeSchedule.TimeScheduleTemplateHeadId, entities.GetTemplateSchedule(employeeSchedule.TimeScheduleTemplateHeadId).ToList());

                            dateEmployeeScheduleDict.Add(currentDate, employeeSchedule);
                        }
                        validDates.Add(currentDate);
                    }
                    finally
                    {
                        currentDate = currentDate.AddDays(1);
                    }
                }
            }

            return (validDates, dateEmployeeScheduleDict, templateHeadScheduleDict);
        }

        private Dictionary<DateTime, bool> LoadEmployeeDaysValidByScheduleAndTransactions(CompEntities entities, GetAttestEmployeeInput input, ref List<DateTime> validDates, Employee employee, List<TimeTreeScheduleBlockDTO> scheduleBlocksEmployee, List<GetTimePayrollTransactionsForEmployee_Result> transactionsEmployee, List<GetTimePayrollTransactionAccountsForEmployee_Result> transactionAccountInternalsEmployee, List<AccountDTO> accountInternalsCompany, bool useAccountHierarchy)
        {
            if (input == null || employee == null || validDates == null)
                return new Dictionary<DateTime, bool>();

            int currentAccountId = AccountManager.GetAccountHierarchySettingAccountId(entities, useAccountHierarchy);
            if (currentAccountId <= 0)
                return null;

            scheduleBlocksEmployee.SetAccountOnZeroBlock(currentAccountId);
            transactionsEmployee.SetAccountInternalIds(transactionAccountInternalsEmployee);

            Dictionary<DateTime, bool> validDatesByScheduleAndTransactions = AccountManager.GetValidDatesOnGivenAccounts(employee.EmployeeId, input.StartDate, input.StopDate, currentAccountId, accountInternalsCompany, scheduleBlocksEmployee, transactionsEmployee);
            if (input.DoNotShowDaysOutsideEmployeeAccount && !input.HasFilterAccountIds)
                validDates = validDates.Intersect(validDatesByScheduleAndTransactions.Keys).ToList();

            return validDatesByScheduleAndTransactions;
        }

        private List<AttestEmployeeDayDTO> LoadDaysOutOfRange(CompEntities entities, GetAttestEmployeeInput input, Dictionary<DateTime, EmployeeSchedule> dateEmployeeScheduleDict, List<TimeBlockDate> timeBlockDates, List<GetTimePayrollTransactionsForEmployee_Result> transactionEmployee, List<AccountDTO> transactionAccountStdsEmployee, List<GetTimePayrollTransactionAccountsForEmployee_Result> transactionAccountInternalsEmployee, List<GetAttestTransitionLogsForEmployeeResult> attestTransitionLogsEmployee)
        {
            List<AttestEmployeeDayDTO> attestEmployeeDays = new List<AttestEmployeeDayDTO>();

            if (input == null || transactionEmployee.IsNullOrEmpty())
                return attestEmployeeDays;

            foreach (var timePayrollTransactionGrouping in transactionEmployee.Where(i => i.Date < input.StartDate || i.Date > input.StopDate).GroupBy(i => i.Date))
            {
                DateTime date = timePayrollTransactionGrouping.Key;

                TimeBlockDate timeBlockDate = timeBlockDates?.FirstOrDefault(i => i.Date == date);
                if (timeBlockDate == null)
                    continue;

                EmployeeSchedule employeeScheduleForDay = dateEmployeeScheduleDict[date];
                if (employeeScheduleForDay == null)
                    continue;

                TimeScheduleTemplatePeriod templatePeriod = TimeScheduleManager.GetTimeScheduleTemplatePeriod(entities, input.EmployeeId, date, employeeScheduleForDay);
                if (templatePeriod == null)
                    continue;

                AttestEmployeeDayDTO attestEmployeeDay = new AttestEmployeeDayDTO(input.EmployeeId, date);
                SetEmployeeDayCalendar(attestEmployeeDay, templatePeriod, null, timeBlockDate, null, null);
                SetEmployeeDayTransactionsForDayOutOfRange(attestEmployeeDay, input, transactionAccountStdsEmployee, transactionAccountInternalsEmployee, timePayrollTransactionGrouping.ToList(), attestTransitionLogsEmployee);

                attestEmployeeDays.Add(attestEmployeeDay);
            }

            return attestEmployeeDays;
        }

        #endregion

        #region Validation

        public SaveAttestEmployeeValidationDTO SaveAttestForEmployeeValidation(List<AttestEmployeeDayDTO> attestEmployeeDays, int attestStateToId, bool isMySelf, int employeeId, int actorCompanyId, int roleId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return SaveAttestForEmployeeValidation(entities, attestEmployeeDays, attestStateToId, isMySelf, employeeId, actorCompanyId, roleId, userId);
        }

        public SaveAttestEmployeeValidationDTO SaveAttestForEmployeeValidation(CompEntities entities, List<AttestEmployeeDayDTO> attestEmployeeDays, int attestStateToId, bool isMySelf, int employeeId, int actorCompanyId, int roleId, int userId)
        {
            try
            {
                #region Prereq

                if (attestEmployeeDays.IsNullOrEmpty())
                {
                    return new SaveAttestEmployeeValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = GetText(10075, "Dagar kunde inte attesteras"),
                        Message = GetText(10072, "Inga dagar hittades"),
                    };
                }

                Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true);
                if (employee == null)
                {
                    return new SaveAttestEmployeeValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = GetText(10075, "Dagar kunde inte attesteras"),
                        Message = GetText(10083, "Anställd hittades inte"),
                    };
                }

                List<DateTime> dates = attestEmployeeDays.OrderBy(i => i.Date).Select(i => i.Date).ToList();
                DateTime dateFrom = dates.First();
                DateTime dateTo = dates.Last();

                Employment employment = employee.GetEmployment(dateFrom, dateTo);
                if (employment == null)
                {
                    return new SaveAttestEmployeeValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = GetText(10075, "Dagar kunde inte attesteras"),
                        Message = GetText(10084, "Anställning hittades inte"),
                    };
                }

                AttestStateDTO attestStateTo = AttestManager.GetAttestState(entities, attestStateToId).ToDTO();
                if (attestStateTo == null)
                {
                    return new SaveAttestEmployeeValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = GetText(10075, "Dagar kunde inte attesteras"),
                        Message = GetText(10082, "Atteststatus hittades inte"),
                    };
                }

                List<AttestTransitionDTO> attestTransitions = new List<AttestTransitionDTO>();
                if (isMySelf)
                    attestTransitions.AddRange(AttestManager.GetAttestTransitionsForEmployeeGroup(entities, TermGroup_AttestEntity.PayrollTime, employment.GetEmployeeGroupId(dateFrom)).ToDTOs(true));
                else
                    attestTransitions.AddRange(AttestManager.GetAttestTransitionsForAttestRoleUser(entities, userId, actorCompanyId, entity: TermGroup_AttestEntity.PayrollTime, dateFrom: dateFrom, dateTo: dateTo).ToDTOs(true));

                Dictionary<CompanySettingType, int> companySettingAttestStateIds = AttestManager.GetPayrollLockedAttestStateSettings(entities, actorCompanyId);

                List<int> validAccountIdsByHiearchy = null;
                int employeeAccountDimId = 0;
                bool useValidAccountsByHiearchy = false;
                bool alsoAttestAdditionsFromTime = false;
                if (!isMySelf)
                {
                    useValidAccountsByHiearchy = AccountManager.TryGetAccountIdsForEmployeeAccountDim(entities, out AccountRepository accountRepository, actorCompanyId, roleId, userId, dateFrom, dateTo, out validAccountIdsByHiearchy, out employeeAccountDimId);
                    alsoAttestAdditionsFromTime = AttestManager.AlsoAttestAdditionsFromTime(entities, userId, actorCompanyId, dateFrom, repository: accountRepository);
                }
                else
                {
                    alsoAttestAdditionsFromTime = employment.GetEmployeeGroup()?.AlsoAttestAdditionsFromTime ?? false;
                }

                #endregion

                #region Validation

                var validItems = new List<SaveAttestEmployeeDayDTO>();
                var preliminaryItems = new List<SaveAttestEmployeeDayDTO>();
                var noTransitionItems = new List<SaveAttestEmployeeDayDTO>();
                var lockedItems = new List<SaveAttestEmployeeDayDTO>();
                var unauthItems = new List<SaveAttestEmployeeDayDTO>();
                var invalidStampingStatusItems = new List<SaveAttestEmployeeDayDTO>();
                var additionDedutionItems = new List<AttestEmployeeDayDTO>();

                foreach (var day in attestEmployeeDays)
                {
                    if (!dates.Contains(day.Date) || day.AttestStates.IsNullOrEmpty() || day.AttestStates.ContainsTheSameId(attestStateToId))
                        continue;
                    if (day.ScheduleTime.TotalMinutes == 0 && day.AttestPayrollTransactions.IsNullOrEmpty() && (!day.PresenceTime.HasValue || day.PresenceTime.Value.TotalMinutes == 0))
                        continue;
                    if (!alsoAttestAdditionsFromTime && day.AttestPayrollTransactions.All(t => t.IsAdditionOrDeduction))
                        continue;

                    List<int> transactionEmployeeAccountIds = new List<int>();
                    if (useValidAccountsByHiearchy && !day.AttestPayrollTransactions.IsNullOrEmpty())
                    {
                        foreach (AttestPayrollTransactionDTO transaction in day.AttestPayrollTransactions)
                        {
                            AccountDTO account = transaction.AccountInternals?.FirstOrDefault(i => i.AccountDimId == employeeAccountDimId);
                            if (account != null && !transactionEmployeeAccountIds.Any(id => id == account.AccountId))
                                transactionEmployeeAccountIds.Add(account.AccountId);
                        }
                    }

                    var currentItem = new SaveAttestEmployeeDayDTO
                    {
                        OriginalUniqueId = day.UniqueId,
                        Date = day.Date,
                        TimeBlockDateId = day.TimeBlockDateId
                    };

                    if (day.IsPrel)
                        preliminaryItems.Add(currentItem);
                    else if (!day.HasAttestTransitionPermission(attestTransitions, attestStateToId))
                        noTransitionItems.Add(currentItem);
                    else if (day.AttestStates.ContainsTheSameId(companySettingAttestStateIds.Select(i => i.Value).ToArray()))
                        lockedItems.Add(currentItem);
                    else if (!TimeStampManager.IsTimeStampStatusValid(day.AutogenTimeblocks, day.TimeBlockDateStampingStatus, attestStateTo))
                        invalidStampingStatusItems.Add(currentItem);
                    else if (useValidAccountsByHiearchy && !transactionEmployeeAccountIds.IsNullOrEmpty() && !validAccountIdsByHiearchy.ContainsAll(transactionEmployeeAccountIds))
                    {
                        unauthItems.Add(currentItem);
                        if (validAccountIdsByHiearchy.ContainsAny(transactionEmployeeAccountIds))
                            validItems.Add(currentItem);
                    }
                    else
                        validItems.Add(currentItem);

                    if (alsoAttestAdditionsFromTime && day.HasAdditionOrDeductionTransactions())
                        additionDedutionItems.Add(day);
                }

                #endregion

                int noOfPreliminary = preliminaryItems?.Count ?? 0;
                int noOfNoTransition = noTransitionItems?.Count ?? 0;
                int nofOfLocked = lockedItems?.Count ?? 0;
                int noOfInvalidStampingStatus = invalidStampingStatusItems?.Count ?? 0;
                int noOfUnauths = unauthItems?.Count ?? 0;
                int noOfAdditionDeduction = additionDedutionItems?.Count ?? 0;
                int noOfValid = validItems?.Count ?? 0;
                StringBuilder message = new StringBuilder();

                if (noOfValid > 0)
                {
                    string title = "";
                    bool canSkipDialog = false;

                    if (noOfPreliminary > 0 || noOfNoTransition > 0 || nofOfLocked > 0 || noOfInvalidStampingStatus > 0 || noOfUnauths > 0 || noOfAdditionDeduction > 0)
                    {
                        #region Question (some valid)

                        message.Append(GetAttestForEmployeePreliminaryMessage(noOfPreliminary, attestStateTo));
                        message.Append(GetAttestForEmployeeNoTransitionMessage(noOfNoTransition, attestStateTo));
                        message.Append(GetAttestForEmployeeLockedMessage(nofOfLocked, attestStateTo));
                        message.Append(GetAttestForEmployeeUnauthMessage(noOfUnauths, attestStateTo));
                        message.Append(GetAttestForEmployeeInvalidStampingStatusMessage(noOfInvalidStampingStatus, attestStateTo));
                        message.Append(GetAttestForEmployeeValidMessage(noOfValid, additionDedutionItems, attestStateTo));

                        title = string.Format(GetText(110679, "Vissa dagar kunde inte få attestnivå {0}"), attestStateTo.Name);
                        canSkipDialog = false;

                        #endregion
                    }
                    else
                    {
                        #region Confirm (all valid)

                        message.Append(GetAttestForEmployeeValidMessage(noOfValid, additionDedutionItems, attestStateTo));

                        title = GetText(10074, "Kontrollfråga");
                        canSkipDialog = true;

                        #endregion
                    }

                    message.Append("\r\n");
                    message.Append(GetText(8494, "Vill du fortsätta?"));

                    return new SaveAttestEmployeeValidationDTO()
                    {
                        Success = true,
                        CanOverride = true,
                        CanSkipDialog = canSkipDialog,
                        Title = title,
                        Message = message.ToString(),
                        ValidItems = validItems,
                    };
                }
                else if (noOfPreliminary > 0 || noOfNoTransition > 0 || nofOfLocked > 0 || noOfInvalidStampingStatus > 0 || noOfUnauths > 0)
                {
                    #region Warning (none valid)

                    message.Append(GetAttestForEmployeePreliminaryMessage(noOfPreliminary, attestStateTo));
                    message.Append(GetAttestForEmployeeNoTransitionMessage(noOfNoTransition, attestStateTo));
                    message.Append(GetAttestForEmployeeLockedMessage(nofOfLocked, attestStateTo));
                    message.Append(GetAttestForEmployeeUnauthMessage(noOfUnauths, attestStateTo));
                    message.Append(GetAttestForEmployeeInvalidStampingStatusMessage(noOfInvalidStampingStatus, attestStateTo));
                    message.Append("\r\n");

                    return new SaveAttestEmployeeValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = string.Format(GetText(110678, "Inga dagar kunde få attestnivå {0}"), attestStateTo.Name),
                        Message = message.ToString(),
                        ValidItems = validItems,
                    };

                    #endregion
                }
                else
                {
                    return new SaveAttestEmployeeValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = GetText(11951, "Inget att attestera"),
                        Message = GetText(11960, "Det finns inga transaktioner att attestera för valda dagar"),
                        ValidItems = validItems,
                    };
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                throw;
            }
        }

        public SaveAttestTransactionsValidationDTO SaveAttestForTransactionsValidation(List<AttestPayrollTransactionDTO> transactionItem, int attestStateToId, bool isMySelf, int actorCompanyId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return SaveAttestForTransactionsValidation(entities, transactionItem, attestStateToId, isMySelf, actorCompanyId, userId);
        }

        public SaveAttestTransactionsValidationDTO SaveAttestForTransactionsValidation(CompEntities entities, List<AttestPayrollTransactionDTO> transactionItems, int attestStateToId, bool isMySelf, int actorCompanyId, int userId)
        {
            #region Prereq

            if (transactionItems.IsNullOrEmpty())
            {
                return new SaveAttestTransactionsValidationDTO()
                {
                    Success = false,
                    CanOverride = false,
                    Title = GetText(10075, "Dagar kunde inte attesteras"),
                    Message = GetText(10073, "Inga transaktioner hittades"),
                };
            }

            AttestStateDTO attestStateTo = AttestManager.GetAttestState(entities, attestStateToId).ToDTO();
            if (attestStateTo == null)
            {
                return new SaveAttestTransactionsValidationDTO()
                {
                    Success = false,
                    CanOverride = false,
                    Title = GetText(10075, "Dagar kunde inte attesteras"),
                    Message = GetText(10082, "Atteststatus hittades inte"),
                };
            }

            List<DateTime> dates = transactionItems.OrderBy(i => i.Date).Select(i => i.Date).ToList();
            DateTime dateFrom = dates.First();
            DateTime dateTo = dates.Last();

            Dictionary<CompanySettingType, int> lockedAttestStateSettings = AttestManager.GetPayrollLockedAttestStateSettings(entities, actorCompanyId);

            List<AttestTransitionDTO> attestTransitions = new List<AttestTransitionDTO>();
            if (!isMySelf)
                attestTransitions.AddRange(AttestManager.GetAttestTransitionsForAttestRoleUser(entities, userId, actorCompanyId, entity: TermGroup_AttestEntity.PayrollTime, dateFrom: dateFrom, dateTo: dateTo).ToDTOs(true));

            List<SaveAttestTransactionDTO> validItems = new List<SaveAttestTransactionDTO>();
            List<SaveAttestTransactionDTO> noTransitionItems = new List<SaveAttestTransactionDTO>();
            List<SaveAttestTransactionDTO> lockedItems = new List<SaveAttestTransactionDTO>();
            List<SaveAttestTransactionDTO> preliminaryItems = new List<SaveAttestTransactionDTO>();

            #endregion

            #region Validate per Employee

            foreach (var transactionItemsByEmployee in transactionItems.Where(i => i.EmployeeId > 0).GroupBy(i => i.EmployeeId))
            {
                #region Prereq

                int employeeId = transactionItemsByEmployee.Key;

                Employee employee = EmployeeManager.GetEmployee(employeeId, actorCompanyId, loadEmployment: true);
                if (employee == null)
                {
                    return new SaveAttestTransactionsValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = GetText(10075, "Dagar kunde inte attesteras"),
                        Message = GetText(10083, "Anställd hittades inte"),
                    };
                }

                Employment employment = employee.GetEmployment(dateFrom, dateTo);
                if (employment == null && isMySelf)
                {
                    return new SaveAttestTransactionsValidationDTO()
                    {
                        Success = false,
                        CanOverride = false,
                        Title = GetText(10075, "Dagar kunde inte attesteras"),
                        Message = GetText(10084, "Anställning hittades inte"),
                    };
                }

                if (isMySelf)
                    attestTransitions.AddRange(AttestManager.GetAttestTransitionsForEmployeeGroup(entities, TermGroup_AttestEntity.PayrollTime, employment.GetEmployeeGroupId()).ToDTOs(true));

                #endregion

                #region Validation

                foreach (var transactionItem in transactionItemsByEmployee.Where(i => !i.IsScheduleTransaction))
                {
                    if (!dates.Contains(transactionItem.Date) || transactionItem.AttestStateId == attestStateToId)
                        continue;

                    var currentItem = new SaveAttestTransactionDTO
                    {
                        OriginalUniqueId = transactionItem.GuidId,
                        EmployeeId = employee.EmployeeId,
                        Date = transactionItem.Date,
                        TimePayrollTransactionIds = transactionItem.AllTimePayrollTransactionIds ?? new List<int>() { transactionItem.TimePayrollTransactionId },
                    };

                    if (transactionItem.IsPreliminary)
                        preliminaryItems.Add(currentItem);
                    else if (!transactionItem.HasAttestTransitionPermission(attestTransitions, attestStateToId))
                        noTransitionItems.Add(currentItem);
                    else if (transactionItem.IsExported && transactionItem.AttestStateId == lockedAttestStateSettings.GetValue(CompanySettingType.SalaryExportPayrollResultingAttestStatus))
                        lockedItems.Add(currentItem);
                    else if (!transactionItem.IsExported && transactionItem.AttestStateId == lockedAttestStateSettings.GetValue(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId))
                        lockedItems.Add(currentItem);
                    else if (transactionItem.AttestStateId == lockedAttestStateSettings.GetValue(CompanySettingType.SalaryPaymentLockedAttestStateId) && !lockedAttestStateSettings.GetValues(CompanySettingType.SalaryPaymentApproved1AttestStateId, CompanySettingType.SalaryPaymentApproved2AttestStateId).Contains(attestStateToId))
                        lockedItems.Add(currentItem);
                    else if (attestStateToId == lockedAttestStateSettings.GetValue(CompanySettingType.SalaryPaymentLockedAttestStateId) && !lockedAttestStateSettings.GetValues(CompanySettingType.SalaryPaymentApproved1AttestStateId, CompanySettingType.SalaryPaymentApproved2AttestStateId).Contains(transactionItem.AttestStateId))
                        lockedItems.Add(currentItem);
                    else
                        validItems.Add(currentItem);
                }

                #endregion
            }

            #endregion

            #region Create result

            int noOfValid = validItems.GetNrOfTransactions();
            int noOfNoTransition = noTransitionItems.GetNrOfTransactions();
            int noOfLocked = lockedItems.GetNrOfTransactions();
            int noOfPreliminary = preliminaryItems.GetNrOfTransactions();
            string message = "";

            if (noOfValid > 0)
            {
                if (noOfNoTransition > 0 || noOfLocked > 0 || noOfPreliminary > 0)
                {
                    #region Question (some valid)

                    message += GetAttestForTransactionsPreliminaryMessage(noOfPreliminary, attestStateTo);
                    message += GetAttestForTransactionNoTransitionMessage(noOfNoTransition, attestStateTo);
                    message += GetAttestForTransactionsLockedMessage(noOfLocked, attestStateTo);
                    message += GetAttestForTransactionsValidMessage(noOfValid, attestStateTo);

                    #endregion
                }
                else
                {
                    #region Confirm (all valid)

                    message += GetAttestForTransactionsValidMessage(noOfValid, attestStateTo);

                    #endregion
                }

                message += "\r\n";
                message += GetText(8494, "Vill du fortsätta?");

                return new SaveAttestTransactionsValidationDTO()
                {
                    Success = true,
                    CanOverride = true,
                    Title = GetText(10074, "Kontrollfråga"),
                    Message = message,
                    ValidItems = validItems,
                };
            }
            else if (noOfNoTransition > 0 || noOfLocked > 0 || noOfPreliminary > 0)
            {
                #region Warning (none valid)

                message += GetAttestForTransactionNoTransitionMessage(noOfNoTransition, attestStateTo);
                message += GetAttestForTransactionsLockedMessage(noOfLocked, attestStateTo);
                message += GetAttestForTransactionsPreliminaryMessage(noOfPreliminary, attestStateTo);
                message += "\r\n";

                return new SaveAttestTransactionsValidationDTO()
                {
                    Success = false,
                    CanOverride = false,
                    Title = string.Format(GetText(110678, "Inga dagar kunde få attestnivå {0}"), attestStateTo.Name),
                    Message = message,
                    ValidItems = validItems,
                };

                #endregion
            }
            else
            {
                return new SaveAttestTransactionsValidationDTO()
                {
                    Success = false,
                    CanOverride = false,
                    Title = GetText(11951, "Inget att attestera"),
                    Message = GetText(11960, "Det finns inga transaktioner att attestera för valda dagar"),
                    ValidItems = validItems,
                };
            }

            #endregion
        }

        public SaveAttestTransactionsValidationDTO SaveAttestForAdditionDeductionsValidation(List<AttestEmployeeAdditionDeductionTransactionDTO> transactionItems, int attestStateToId, bool isMySelf, int employeeId, int actorCompanyId, int userId)
        {
            return SaveAttestForTransactionsValidation(transactionItems.ToAttestPayrollTransactionDTOs(employeeId), attestStateToId, isMySelf, actorCompanyId, userId);
        }

        public TimeAttestCalculationFunctionValidationDTO ApplyCalculationFunctionValidation(int currentEmployeeId, List<AttestEmployeeDayDTO> items, SoeTimeAttestFunctionOption option, int actorCompanyId, int userId)
        {
            var validDays = new List<AttestEmployeeDaySmallDTO>();

            var absenceRequestDays = new List<AttestEmployeeDayDTO>();
            var scheduledPlacementDays = new List<AttestEmployeeDayDTO>();
            var attestedDays = new List<AttestEmployeeDayDTO>();
            var lockedDays = new List<AttestEmployeeDayDTO>();
            var additionalDays = new List<AttestEmployeeDayDTO>();

            int initialAttestStateId = AttestManager.GetInitialAttestStateId(actorCompanyId, TermGroup_AttestEntity.PayrollTime);
            bool isRestore = option == SoeTimeAttestFunctionOption.RestoreToSchedule || option == SoeTimeAttestFunctionOption.RestoreScheduleToTemplate;
            bool isDelete = option == SoeTimeAttestFunctionOption.DeleteTimeBlocksAndTransactions;
            bool isRegenerate = option == SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest || option == SoeTimeAttestFunctionOption.RecalculateAccounting || option == SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps;

            items.ForEach(item => Validate(item));

            StringBuilder messageBuilder = new StringBuilder();
            if (validDays.Any())
                return HasAnyInvalid() ? GetWarning() : GetConfirmation();
            else
                return GetForbidden();

            void Validate(AttestEmployeeDayDTO day)
            {
                if (day.HasScheduledPlacement)
                    scheduledPlacementDays.Add(day);
                else if (!isRegenerate && day.HasAttestStateNoneInitial(initialAttestStateId))
                    attestedDays.Add(day);
                else if (day.TimeBlockDateStatus == (int)SoeTimeBlockDateStatus.Locked)
                    lockedDays.Add(day);
                else if (day.IsCompletelyAdditional && (isRestore || isDelete))
                    additionalDays.Add(day);
                else
                    AddValid(day);

                if (isRestore && (day.ShiftUserStatuses.Contains(TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested) || day.ShiftUserStatuses.Contains(TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved)))
                    absenceRequestDays.Add(day);
            }
            void AddValid(AttestEmployeeDayDTO item)
            {
                validDays.Add(new AttestEmployeeDaySmallDTO(currentEmployeeId, item.Date, item.TimeBlockDateId, item.TimeScheduleTemplatePeriodId));
            }
            bool HasAnyInvalid()
            {
                return scheduledPlacementDays.Any() || attestedDays.Any() || lockedDays.Any() || additionalDays.Any();
            }
            bool UseDisabledWarning()
            {
                return SettingManager.GetBoolSetting(SettingMainType.User, (int)UserSettingType.TimeDisableApplyRestoreWarning, userId, 0, 0);
            }
            void AddValidMessage()
            {
                if (validDays.Any())
                {
                    messageBuilder.Append($"{GetDaysTerm(validDays)} {GetText(9301, "kommer att behandlas")}");
                    AddNewRow();
                    if (absenceRequestDays.Any())
                    {
                        messageBuilder.Append($"{GetDaysTerm(absenceRequestDays)} {GetText(9302, "har frånvaroansökan på sig som kan påverkas. Rekommenderas att kontrollera frånvaroansökningar innan återställning")}");
                        AddNewRow();
                    }
                }
            }
            void AddScheduledPlacementMessage()
            {
                if (scheduledPlacementDays.Any())
                {
                    messageBuilder.Append($"{GetDaysTerm(scheduledPlacementDays)} {GetText(91905, "har schemalagd aktivering och kan ej behandlas")}. ");
                    AddNewRow();
                }
            }
            void AddAttestedMessage()
            {
                if (attestedDays.Any())
                {
                    messageBuilder.Append($"{GetDaysTerm(attestedDays)} {GetText(9303, "har attesterade transaktioner och kan ej behandlas")}. ");
                    AddNewRow();
                }
            }
            void AddLockedMessage()
            {
                if (lockedDays.Any())
                {
                    messageBuilder.Append($"{GetDaysTerm(lockedDays)} {GetText(91936, "är låsta och kan ej behandlas")}. ");
                    AddNewRow();
                }
            }
            void AddAdditionalMessage()
            {
                if (additionalDays.Any())
                {
                    messageBuilder.Append($"{GetDaysTerm(additionalDays)} {GetText(91906, "har endast transaktioner utanför din tillhörighet kan ej behandlas")}. ");
                    AddNewRow();
                }
            }
            void AddContinueQuestion()
            {
                AddNewRow();
                messageBuilder.Append(GetText(8494, "Vill du fortsätta?"));
            }
            void AddNewRow()
            {
                messageBuilder.Append("\r\n");
            }
            string GetDaysTerm<T>(List<T> l)
            {
                return TermManager.GetDaysTerm(l);
            }
            TimeAttestCalculationFunctionValidationDTO GetWarning()
            {
                //Always show warning if has absencerequest
                if (!absenceRequestDays.Any() && UseDisabledWarning())
                {
                    return new TimeAttestCalculationFunctionValidationDTO()
                    {
                        Success = true,
                        ApplySilent = true,
                        ValidItems = validDays,
                        Option = option,
                    };
                }
                else
                {
                    AddScheduledPlacementMessage();
                    AddAttestedMessage();
                    AddLockedMessage();
                    AddAdditionalMessage();
                    AddValidMessage();
                    AddContinueQuestion();

                    return new TimeAttestCalculationFunctionValidationDTO()
                    {
                        Success = true,
                        ApplySilent = false,
                        CanOverride = true,
                        Title = GetText(10074, "Kontrollfråga"),
                        Message = messageBuilder.ToString(),
                        ValidItems = validDays,
                        Option = option,
                    };
                }
            }
            TimeAttestCalculationFunctionValidationDTO GetForbidden()
            {
                AddScheduledPlacementMessage();
                AddAttestedMessage();
                AddLockedMessage();
                AddAdditionalMessage();
                AddNewRow();

                return new TimeAttestCalculationFunctionValidationDTO()
                {
                    Success = false,
                    ApplySilent = false,
                    CanOverride = false,
                    Title = GetText(91909, "Ej tillåtet"),
                    Message = messageBuilder.ToString(),
                    ValidItems = validDays,
                    Option = option,
                };
            }
            TimeAttestCalculationFunctionValidationDTO GetConfirmation()
            {
                if (!absenceRequestDays.Any() && UseDisabledWarning())
                {
                    return new TimeAttestCalculationFunctionValidationDTO()
                    {
                        Success = true,
                        ApplySilent = true,
                        ValidItems = validDays,
                        Option = option,
                    };
                }
                else
                {
                    AddValidMessage();
                    AddContinueQuestion();

                    return new TimeAttestCalculationFunctionValidationDTO()
                    {
                        Success = true,
                        ApplySilent = false,
                        CanOverride = true,
                        Title = GetText(10074, "Kontrollfråga"),
                        Message = messageBuilder.ToString(),
                        ValidItems = validDays,
                        Option = option,
                    };
                }
            }
        }

        #endregion

        #region Help-methods

        private (bool isPresence, bool isAbsence, bool isBreak) GetTimeBlockTypes(IEnumerable<int> timeCodeTypes, List<GetTimePayrollTransactionsForEmployee_Result> transactions, int timeBlockId)
        {
            bool isBreak = timeCodeTypes?.Any(type => type == (int)SoeTimeCodeType.Break) ?? false;
            bool isPresence = transactions?.IsTimeBlockPresence(timeBlockId) ?? false;
            bool isAbsence = transactions?.IsTimeBlockAbsence(timeBlockId) ?? false;

            if (!isAbsence && !isPresence)
                return GetTimeBlockTypes(timeCodeTypes);
            return (isPresence, isAbsence, isBreak);
        }

        private (bool isPresence, bool isAbsence, bool isBreak) GetTimeBlockTypes(IEnumerable<int> timeCodeTypes)
        {
            bool isPresence = timeCodeTypes?.Any(type => type == (int)SoeTimeCodeType.Work) ?? false;
            bool isAbsence = timeCodeTypes?.Any(type => type == (int)SoeTimeCodeType.Absense) ?? false;
            bool isBreak = timeCodeTypes?.Any(type => type == (int)SoeTimeCodeType.Break) ?? false;

            return (isPresence, isAbsence, isBreak);
        }

        private (TimeSpan total, TimeSpan insideSchedule, TimeSpan outsideSchedule) GetTimeBlockLengths(ITimeBlockObject timeBlock, DateTime scheduleIn, DateTime scheduleOut, DateTime? standbyStartTime, DateTime? standbyStopTime, int? standardTimeDeviationCauseId = null)
        {
            return GetTimeBlockLengths(scheduleIn, scheduleOut, standbyStartTime, standbyStopTime, timeBlock.StartTime, timeBlock.StopTime, timeBlock.TimeDeviationCauseStartId, standardTimeDeviationCauseId);
        }

        private (TimeSpan total, TimeSpan insideSchedule, TimeSpan outsideSchedule) GetTimeBlockLengths(DateTime scheduleIn, DateTime scheduleOut, DateTime? standbyStartTime, DateTime? standbyStopTime, DateTime startTime, DateTime stopTime, int? timeDeviationCauseId, int? standardTimeDeviationCauseId = null)
        {
            TimeSpan total, insideSchedule, outsideSchedule = new TimeSpan();

            total = stopTime.Subtract(startTime);
            insideSchedule = CalendarUtility.GetTimeSpanFromMinutes(CalendarUtility.GetOverlappingMinutes(scheduleIn, scheduleOut, startTime, stopTime));
            if (IsWorkBeforeSchedule(scheduleIn, standbyStartTime, startTime, timeDeviationCauseId, standardTimeDeviationCauseId))
                outsideSchedule += CalendarUtility.GetEarliestDate(stopTime, scheduleIn).Subtract(startTime);
            if (IsWorkAfterSchedule(scheduleOut, standbyStopTime, stopTime, timeDeviationCauseId, standardTimeDeviationCauseId))
                outsideSchedule += stopTime.Subtract(CalendarUtility.GetLatestDate(startTime, scheduleOut));

            return (total, insideSchedule, outsideSchedule);
        }

        private bool IsWorkBeforeSchedule(DateTime scheduleIn, DateTime? standbyStartTime, DateTime startTime, int? timeDeviationCauseId, int? standardTimeDeviationCauseId = null)
        {
            if (startTime >= scheduleIn)
                return false;
            if (standbyStartTime.HasValue && startTime >= standbyStartTime && HasStandardTimeDeviationCause(timeDeviationCauseId, standardTimeDeviationCauseId))
                return false;
            return true;
        }

        private bool IsWorkAfterSchedule(DateTime scheduleOut, DateTime? standbyStopTime, DateTime stopTime, int? timeDeviationCauseId, int? standardTimeDeviationCauseId = null)
        {
            if (stopTime <= scheduleOut)
                return false;
            if (standbyStopTime.HasValue && stopTime <= standbyStopTime && HasStandardTimeDeviationCause(timeDeviationCauseId, standardTimeDeviationCauseId))
                return false;
            return true;
        }

        private bool HasStandardTimeDeviationCause(int? timeDeviationCauseId, int? standardTimeDeviationCauseId)
        {
            return standardTimeDeviationCauseId.HasValue && (!timeDeviationCauseId.HasValue || timeDeviationCauseId.Value == standardTimeDeviationCauseId.Value);
        }

        #endregion
    }
}
