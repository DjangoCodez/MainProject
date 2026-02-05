using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.TimeTree
{
    public class TimeTreePayrollManager : TimeTreeBaseManager
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public TimeTreePayrollManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Tree

        public TimeEmployeeTreeDTO GetPayrollCalculationTree(TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting, int timePeriodId, TimeEmployeeTreeSettings settings = null)
        {
            TimeEmployeeTreeDTO tree;

            try
            {
                #region Init

                settings = TimeEmployeeTreeSettings.Init(settings);

                if (string.IsNullOrEmpty(settings.CacheKeyToUse) || !Guid.TryParse(settings.CacheKeyToUse, out Guid cacheKey))
                    cacheKey = Guid.NewGuid();

                #endregion

                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, base.ActorCompanyId, loadTimePeriodHead: true);
                    tree = new TimeEmployeeTreeDTO(cacheKey, base.ActorCompanyId, timePeriod?.ToDTO(), SoeAttestTreeMode.PayrollCalculation, grouping, sorting, settings);
                    if (!tree.IsValid)
                        return tree;

                    bool usePayroll = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, base.ActorCompanyId, 0);
                    if (!usePayroll)
                        return tree;

                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                    List<PayrollGroup> payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                    List<AttestState> attestStates = GetAttestStatesForTimeFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                    AttestState highestAttestState = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.SalaryExportPayrollResultingAttestStatus, tree.Settings.DoNotShowCalculated);

                    List<Employee> employees = GetEmployeesWithRepositoryFromCache(entities,
                        CacheConfig.Company(base.ActorCompanyId, cacheKey, seconds: (int)CacheTTL.ThirtyMinutes, keepAlive: true),
                        out EmployeeAuthModelRepository repository,
                        base.UserId,
                        base.RoleId,
                        tree.GetTimePeriodEarliestStartDate(),
                        tree.GetTimePeriodLatestStopDate(),
                        settings.FilterEmployeeIds,
                        useDefaultEmployeeAccountDimEmployee: true,
                        includeEnded: settings.IncludeEnded,
                        searchPattern: settings.SearchPattern
                        )
                        .Where(x => !x.ExcludeFromPayroll).ToList();

                    //Transactions (only add to cache so warnings can use it, never reuse between reloads)
                    List<TimePayrollTransactionTreeDTO> transactionsCompany = GetTimePayrollTransactionsForTreeFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey), timePeriod: timePeriod, employeeIds: employees.GetIds(), onlyUseInPayroll: true, flushCache: true);
                    Dictionary<int, List<TimePayrollTransactionTreeDTO>> transactionsByEmployee = transactionsCompany?.GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList()) ?? new Dictionary<int, List<TimePayrollTransactionTreeDTO>>();
                    List<VacationYearEndRow> finalSalaries = timePeriod != null ? PayrollManager.GetVacationYearEndRows(entities, base.ActorCompanyId, timePeriod.TimePeriodId, SoeVacationYearEndType.FinalSalary) : null;

                    #endregion

                    #region Validate Employees

                    List<Employee> validEmployees = new List<Employee>();
                    List<Employee> endedEmployees = new List<Employee>();

                    foreach (Employee employee in employees)
                    {
                        List<TimePayrollTransactionTreeDTO> transactionsEmployee = TimeTransactionManager.FilterTimePayrollTransactions(transactionsByEmployee.GetList(employee.EmployeeId), employee, tree.StartDate, tree.StopDate);
                        if (!TrySetTimeTreeEmployeeProperties(tree, employee, tree.StartDate, tree.StopDate, timePeriod, attestStates, transactionsEmployee, out bool employeeIsEnded, false, employeeGroups, payrollGroups, highestAttestState, finalSalaries: finalSalaries))
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
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                throw;
            }

            return tree;
        }

        public TimeEmployeeTreeDTO RefreshPayrollCalculationTree(TimeEmployeeTreeDTO tree, int timePeriodId, TimeEmployeeTreeSettings settings = null)
        {
            if (tree == null)
                return tree;

            try
            {
                tree.SetSettings(settings);
                Guid cacheKey = tree.GetCacheKey(flush: true);

                using (CompEntities entities = new CompEntities())
                {
                    TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, base.ActorCompanyId, loadTimePeriodHead: true);
                    if (timePeriod == null)
                        return tree;

                    if (!tree.GroupNodes.IsNullOrEmpty())
                    {
                        List<AttestState> attestStates = AttestManager.GetAttestStates(entities, base.ActorCompanyId, TermGroup_AttestEntity.PayrollTime, SoeModule.Time);
                        AttestState highestAttestState = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.SalaryExportPayrollResultingAttestStatus, tree.Settings.DoNotShowCalculated);
                        List<TimePayrollTransactionTreeDTO> transactions = GetTimePayrollTransactionsForTreeFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey), timePeriod: timePeriod, employeeIds: settings.FilterEmployeeIds, onlyUseInPayroll: true);
                        List<VacationYearEndRow> finalSalaries = settings.DoRefreshFinalSalaryStatus ? PayrollManager.GetVacationYearEndRows(entities, base.ActorCompanyId, timePeriod.TimePeriodId, SoeVacationYearEndType.FinalSalary, employeeIds: settings.FilterEmployeeIds) : null;

                        foreach (TimeEmployeeTreeGroupNodeDTO groupNode in tree.GroupNodes.ToList())
                        {
                            RefreshGroupNode(entities, tree, groupNode, SoeAttestTreeMode.PayrollCalculation, transactions, attestStates, highestAttestState, finalSalaries: finalSalaries);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
                throw;
            }

            return tree;
        }

        public TimeEmployeeTreeDTO GetPayrollCalculationTreeWarnings(TimeEmployeeTreeDTO tree, List<int> employeeIds, int timePeriodId, TermGroup_TimeTreeWarningFilter warningFilter, bool flushCache = false)
        {
            if (tree == null)
                return tree;

            try
            {
                Guid cacheKey = tree.GetCacheKey(flushCache);

                tree.Settings = TimeEmployeeTreeSettings.Init();
                tree.Settings.WarningFilter = warningFilter;

                using (CompEntities entities = new CompEntities())
                {
                    #region Load company data

                    TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, base.ActorCompanyId, loadTimePeriodHead: true);
                    if (timePeriod == null || timePeriod.TimePeriodHead == null || !timePeriod.PaymentDate.HasValue)
                        return tree;

                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey));

                    if (employeeIds.IsNullOrEmpty())
                        employeeIds = tree.GetEmployeeIdsFromNodes();

                    var transactions = GetTimePayrollTransactionsForTreeFromCache(entities, CacheConfig.Company(base.ActorCompanyId, cacheKey), timePeriod: timePeriod, employeeIds: employeeIds);
                    var transactionsByEmployee = transactions?.GroupBy(i => i.EmployeeId).ToDictionary(i => i.Key, i => i.ToList()) ?? new Dictionary<int, List<TimePayrollTransactionTreeDTO>>();

                    #endregion

                    #region Parallell loading of warnings

                    TimeTreeWarningsRepository warningRepository = new TimeTreeWarningsRepository();

                    Parallel.Invoke(GetDefaultParallelOptions(), () =>
                    {
                        #region PayrollControlFunctionOutcomes
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.PayrollControlFunctionOutcome.NoTracking();

                            warningRepository.AddPayrollWarnings(PayrollManager.GetPayrollWarningsByType(entitiesParallel, base.ActorCompanyId, employeeIds, timePeriod.TimePeriodId));
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region TimeStamps with invalid status
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.TimeBlockDate.NoTracking();

                            warningRepository.AddTimeWarnings(SoeTimeAttestWarning.TimeStampErrors,
                                TimeBlockManager.GetTimeBlockDatesWithStampingErrors(entitiesParallel, base.ActorCompanyId, employeeIds, dateFrom: timePeriod.StartDate, dateTo: timePeriod.StopDate).Keys.ToList());
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region TimeStamps without transactions
                        if (transactionsByEmployee != null && employeeGroups.Any(i => i.AutogenTimeblocks))
                        {
                            using (CompEntities entitiesParallel = new CompEntities())
                            {
                                entitiesParallel.TimeStampEntry.AsNoTracking();

                                warningRepository.AddTimeWarnings(SoeTimeAttestWarning.TimeStampsWithoutTransactions,
                                    TimeTransactionManager.GetEmployeesWithTimeStampsWithoutTransactions(entitiesParallel, timePeriod.StartDate, timePeriod.StopDate > DateTime.Today ? CalendarUtility.GetEndOfDay(DateTime.Now.Date) : timePeriod.StopDate, transactionsByEmployee, employeeIds));
                            }
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region Schedule without transactions
                        if (transactionsByEmployee != null)
                        {
                            using (CompEntities entitiesParallel = new CompEntities())
                            {
                                entitiesParallel.TimeScheduleTemplateBlock.NoTracking();

                                warningRepository.AddTimeWarnings(SoeTimeAttestWarning.ScheduleWithoutTransactions,
                                    TimeTransactionManager.GetEmployeeWithScheduleWithoutTransactions(entitiesParallel, timePeriod.StartDate, timePeriod.StopDate, transactionsByEmployee, employeeIds));
                            }
                        }
                        #endregion
                    },
                    () =>
                    {
                        #region PayrollImport errors
                        using (CompEntities entitiesParallel = new CompEntities())
                        {
                            entitiesParallel.PayrollImportEmployeeTransaction.AsNoTracking();

                            warningRepository.AddTimeWarnings(SoeTimeAttestWarning.PayrollImport,
                                GetPayrollImportErrors(entitiesParallel, base.ActorCompanyId, employeeIds, timePeriod.StartDate, timePeriod.StopDate > DateTime.Today ? CalendarUtility.GetEndOfDay(DateTime.Now.Date) : timePeriod.StopDate)?.GetKeysWithValue());
                        }
                        #endregion
                    }
                    );

                    #endregion

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

        #endregion

        #region Employee

        public List<PayrollCalculationProductDTO> GetPayrollCalculationProducts(
            int actorCompanyId,
            int timePeriodId,
            int employeeId,
            bool showAllTransactions = false,
            bool applyEmploymentTaxMinimumRule = false,
            bool isPayrollSlip = false,
            List<EmployeeGroup> employeeGroups = null)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollCalculationProducts(entitiesReadOnly,
                actorCompanyId,
                timePeriodId,
                employeeId,
                showAllTransactions: showAllTransactions,
                applyEmploymentTaxMinimumRule: applyEmploymentTaxMinimumRule,
                isPayrollSlip: isPayrollSlip,
                employeeGroups: employeeGroups
            );
        }

        public List<PayrollCalculationProductDTO> GetPayrollCalculationProducts(
            CompEntities entities,
            int actorCompanyId,
            int timePeriodId,
            int employeeId,
            bool showAllTransactions = false,
            bool applyEmploymentTaxMinimumRule = false,
            bool isPayrollSlip = false,
            List<EmployeeGroup> employeeGroups = null)
        {
            List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(entities, actorCompanyId, new List<int> { employeeId }, loadEmployment: true);
            if (employees == null)
                return new List<PayrollCalculationProductDTO>();

            return GetPayrollCalculationProducts(
                entities,
                actorCompanyId,
                timePeriodId,
                employees,
                showAllTransactions: showAllTransactions,
                applyEmploymentTaxMinimumRule: applyEmploymentTaxMinimumRule,
                isPayrollSlip: isPayrollSlip,
                employeeGroups: employeeGroups);
        }

        public List<PayrollCalculationProductDTO> GetPayrollCalculationProducts(
            CompEntities entities,
            int actorCompanyId,
            int timePeriodId,
            List<Employee> employees,
            bool showAllTransactions = false,
            bool applyEmploymentTaxMinimumRule = false,
            bool ignoreAccounting = false,
            bool isPayrollSlip = false,
            List<EmployeeTimePeriod> employeeTimePeriods = null,
            List<AccountDTO> allAccountDTOs = null,
            List<EmployeeGroup> employeeGroups = null)
        {
            TimePeriod timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, actorCompanyId);
            if (timePeriod == null || !timePeriod.PaymentDate.HasValue)
                return new List<PayrollCalculationProductDTO>();

            List<EmploymentTaxTimePeriodHeadItemDTO> employmentTaxTimePeriodHeads = null;
            if (applyEmploymentTaxMinimumRule)
                employmentTaxTimePeriodHeads = TimePeriodManager.GetEmploymentTaxTimePeriodHeadDTOs(entities, actorCompanyId, timePeriod.PaymentDate.Value.Year, employees.Select(e => e.EmployeeId).ToList());

            return GetPayrollCalculationProducts(
                entities,
                actorCompanyId,
                timePeriod,
                employees,
                showAllTransactions: showAllTransactions,
                applyEmploymentTaxMinimumRule: applyEmploymentTaxMinimumRule,
                employmentTaxTimePeriodItems: employmentTaxTimePeriodHeads,
                employeeGroups: employeeGroups,
                employeeTimePeriods: employeeTimePeriods,
                allAccountDTOs: allAccountDTOs,
                ignoreAccounting: ignoreAccounting,
                isPayrollSlip: isPayrollSlip);
        }

        public List<PayrollCalculationProductDTO> GetPayrollCalculationProducts(
            CompEntities entities,
            int actorCompanyId,
            TimePeriod timePeriod,
            List<Employee> employees,
            bool showAllTransactions = false,
            bool applyEmploymentTaxMinimumRule = false,
            bool ignoreAccounting = false,
            bool isPayrollSlip = false,
            bool getEmployeeTimePeriodSettings = true,
            bool isAgd = false,
            List<GetTimePayrollTransactionsForEmployee_Result> timePayrollTransactionItems = null,
            List<AccountDTO> timePayrollTransactionAccountStds = null,
            List<GetTimePayrollTransactionAccountsForEmployee_Result> timePayrollTransactionAccountInternalItems = null,
            List<GetTimePayrollScheduleTransactionsForEmployee_Result> timePayrollScheduleTransactionItems = null,
            List<AccountDTO> timePayrollScheduleTransactionAccountStds = null,
            List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> timePayrollScheduleTransactionAccountInternalItems = null,
            List<EmploymentTaxTimePeriodHeadItemDTO> employmentTaxTimePeriodItems = null,
            List<EmployeeTimePeriod> employeeTimePeriods = null,
            List<AccountDTO> allAccountDTOs = null,
            List<EmployeeGroup> employeeGroups = null)
        {
            var dtos = new List<PayrollCalculationProductDTO>();

            bool usePayroll = SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, actorCompanyId, 0);
            if (!usePayroll)
                return dtos;

            var (transactionsParamStartDate, transactionsParamStopDate) = timePeriod.GetDates();
            List<int> employeeIds = employees.Select(e => e.EmployeeId).ToList();
            List<int> validEmployeeIdsForPeriod = employeeTimePeriods != null ? employeeTimePeriods.Where(s => s.TimePeriodId == timePeriod.TimePeriodId).Select(s => s.EmployeeId).Distinct().ToList() : employeeIds;
            string employeeIdsString = StringUtility.GetCommaSeparatedString<int>(validEmployeeIdsForPeriod.Distinct().ToList());
            List<AccountDimDTO> companyAccountDims = GetAccountDimsFromCache(entities, CacheConfig.Company(actorCompanyId));

            if (employeeGroups == null)
                employeeGroups = GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId));
            if (timePayrollTransactionItems == null)
                timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollTransactionItemsForEmployees(entities, employeeIdsString, timePeriod.TimePeriodId, transactionsParamStartDate, transactionsParamStopDate);
            if (timePayrollTransactionAccountStds == null && !ignoreAccounting)
                timePayrollTransactionAccountStds = AccountManager.GetAccountStds(entities, actorCompanyId, timePayrollTransactionItems.Select(i => i.AccountId).Distinct().ToList(), false, allAccountDTOs);
            if (timePayrollTransactionAccountInternalItems == null && !ignoreAccounting)
                timePayrollTransactionAccountInternalItems = TimeTransactionManager.GetTimePayrollTransactionAccountsForTimePeriodAndEmployees(entities, transactionsParamStartDate, transactionsParamStopDate, timePeriod.TimePeriodId, employeeIdsString);

            if (!timePeriod.ExtraPeriod)
            {
                if (timePayrollScheduleTransactionItems == null)
                    timePayrollScheduleTransactionItems = TimeTransactionManager.GetTimePayrollScheduleTransactionsForTimePeriodAndEmployees(entities, null, timePeriod.StartDate, timePeriod.StopDate, timePeriod.TimePeriodId, employeeIdsString).ToList();
                if (timePayrollScheduleTransactionAccountStds == null && !ignoreAccounting)
                    timePayrollScheduleTransactionAccountStds = AccountManager.GetAccountStds(entities, actorCompanyId, timePayrollScheduleTransactionItems.Select(i => i.AccountId).Distinct().ToList(), false, allAccountDTOs);
                if (timePayrollScheduleTransactionAccountInternalItems == null && !ignoreAccounting)
                    timePayrollScheduleTransactionAccountInternalItems = TimeTransactionManager.GetTimePayrollScheduleTransactionAccountsForTimePeriodAndEmployees(entities, null, timePeriod.StartDate, timePeriod.StopDate, timePeriod.TimePeriodId, employeeIdsString).ToList();
            }
            else
            {
                timePayrollScheduleTransactionItems = new List<GetTimePayrollScheduleTransactionsForEmployee_Result>();
                timePayrollScheduleTransactionAccountStds = new List<AccountDTO>();
                timePayrollScheduleTransactionAccountInternalItems = new List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result>();
            }

            foreach (Employee employee in employees.Where(w => validEmployeeIdsForPeriod.Contains(w.EmployeeId)))
            {
                dtos.AddRange(GetPayrollCalculationProducts(
                    entities,
                    actorCompanyId,
                    timePeriod,
                    employee,
                    showAllTransactions: showAllTransactions,
                    applyEmploymentTaxMinimumRule: applyEmploymentTaxMinimumRule,
                    ignoreAccounting: ignoreAccounting,
                    isPayrollSlip: isPayrollSlip,
                    getEmployeeTimePeriodSettings: getEmployeeTimePeriodSettings,
                    isAgd: isAgd,
                    timePayrollTransactionItems: timePayrollTransactionItems.Where(t => t.EmployeeId == employee.EmployeeId).ToList(),
                    timePayrollTransactionAccountStds: timePayrollTransactionAccountStds,
                    timePayrollTransactionAccountInternalItems: timePayrollTransactionAccountInternalItems?.Where(t => t.EmployeeId == employee.EmployeeId).ToList(),
                    timePayrollScheduleTransactionItems: timePayrollScheduleTransactionItems.Where(t => t.EmployeeId == employee.EmployeeId).ToList(),
                    timePayrollScheduleTransactionAccountStds: timePayrollScheduleTransactionAccountStds,
                    timePayrollScheduleTransactionAccountInternalItems: timePayrollScheduleTransactionAccountInternalItems?.Where(t => t.EmployeeId == employee.EmployeeId).ToList(),
                    employeeTimePeriods: employeeTimePeriods,
                    employmentTaxTimePeriodHeadItem: employmentTaxTimePeriodItems?.FirstOrDefault(p => p.EmployeeId == employee.EmployeeId && p.Year == timePeriod.PaymentDate.Value.Year),
                    companyAccountDims: companyAccountDims,
                    employeeGroups: employeeGroups));
            }

            return dtos;
        }

        public List<PayrollCalculationProductDTO> GetPayrollCalculationProducts(
            CompEntities entities,
            int actorCompanyId,
            TimePeriod timePeriod,
            Employee employee,
            bool showAllTransactions = false,
            bool applyEmploymentTaxMinimumRule = false,
            bool ignoreAccounting = false,
            bool isPayrollSlip = false,
            bool getEmployeeTimePeriodSettings = true,
            bool isAgd = false,
            List<GetTimePayrollTransactionsForEmployee_Result> timePayrollTransactionItems = null,
            List<AccountDTO> timePayrollTransactionAccountStds = null,
            List<GetTimePayrollTransactionAccountsForEmployee_Result> timePayrollTransactionAccountInternalItems = null,
            List<GetTimePayrollScheduleTransactionsForEmployee_Result> timePayrollScheduleTransactionItems = null,
            List<AccountDTO> timePayrollScheduleTransactionAccountStds = null,
            List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result> timePayrollScheduleTransactionAccountInternalItems = null,
            List<EmployeeTimePeriod> employeeTimePeriods = null,
            EmploymentTaxTimePeriodHeadItemDTO employmentTaxTimePeriodHeadItem = null,
            List<AccountDimDTO> companyAccountDims = null,
            List<EmployeeGroup> employeeGroups = null)
        {
            var payrollCalculationProducts = new List<PayrollCalculationProductDTO>();

            if (employee == null || timePeriod == null || !timePeriod.PaymentDate.HasValue)
                return payrollCalculationProducts;

            #region Prereq

            var (transactionsParamStartDate, transactionsParamStopDate) = timePeriod.GetDates();

            //Transactiondates
            List<DateTime> transactionDates = employee.GetEmploymentDates(timePeriod.StartDate, timePeriod.StopDate);

            //EmployeeGroup
            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timePeriod.StartDate, timePeriod.StopDate, employeeGroups);

            //EmployeeTimePeriod
            EmployeeTimePeriod currentEmployeeTimePeriod = null;
            if (employeeTimePeriods != null)
                currentEmployeeTimePeriod = TimePeriodManager.GetEmployeeTimePeriodWithValues(employeeTimePeriods, timePeriod.TimePeriodId, employee.EmployeeId, actorCompanyId);
            if (currentEmployeeTimePeriod == null)
                currentEmployeeTimePeriod = TimePeriodManager.GetEmployeeTimePeriodWithValues(entities, timePeriod.TimePeriodId, employee.EmployeeId, actorCompanyId);
            decimal currentEmploymentTaxBasis = currentEmployeeTimePeriod != null ? currentEmployeeTimePeriod.Getvalue(SoeEmployeeTimePeriodValueType.GrossSalary) + currentEmployeeTimePeriod.Getvalue(SoeEmployeeTimePeriodValueType.Benefit) : 0;
            bool hasReachedEmploymentTaxMinimumBeforeCurrentPeriod = employmentTaxTimePeriodHeadItem?.IsEmploymentTaxMinimumLimitReachedBeforeGivenPeriod(timePeriod.PaymentDate.Value, isAgd) ?? true;
            bool hasReachedEmploymentTaxMinimumInCurrentPeriod = employmentTaxTimePeriodHeadItem?.IsEmploymentTaxMinimumLimitReachedIncludingGivenPeriod(timePeriod.PaymentDate.Value, currentEmploymentTaxBasis) ?? true;

            //TimePayrollTransactions
            if (timePayrollTransactionItems == null)
                timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollTransactionItemsForEmployee(entities, employee.EmployeeId, transactionsParamStartDate, transactionsParamStopDate, timePeriod.TimePeriodId);
            timePayrollTransactionItems = timePayrollTransactionItems.Where(i => i.PayrollProductUseInPayroll && !i.PayrollStartValueRowId.HasValue).ToList();
            timePayrollTransactionItems = timePayrollTransactionItems.Where(i => transactionDates.Contains(i.Date) || i.TimePeriodId.HasValue).ToList();

            //TimePayrollTransactions AccountStds
            if (timePayrollTransactionAccountStds == null && !ignoreAccounting)
                timePayrollTransactionAccountStds = AccountManager.GetAccountStds(entities, base.ActorCompanyId, timePayrollTransactionItems.Select(i => i.AccountId).Distinct().ToList(), false);

            //TimePayrollTransactions AccountInternals
            if (timePayrollTransactionAccountInternalItems == null && !ignoreAccounting)
                timePayrollTransactionAccountInternalItems = TimeTransactionManager.GetTimePayrollTransactionAccountsForEmployee(entities, transactionsParamStartDate, transactionsParamStopDate, timePeriod.TimePeriodId, employee.EmployeeId);

            if (!timePeriod.ExtraPeriod)
            {
                //TimePayrollScheduleTransactions
                if (timePayrollScheduleTransactionItems == null)
                    timePayrollScheduleTransactionItems = TimeTransactionManager.GetTimePayrollScheduleTransactionsForEmployee(entities, null, timePeriod.StartDate, timePeriod.StopDate, timePeriod.TimePeriodId, employee.EmployeeId).ToList();

                //TimePayrollScheduleTransactions AccountStds
                if (timePayrollScheduleTransactionAccountStds == null && !ignoreAccounting)
                    timePayrollScheduleTransactionAccountStds = AccountManager.GetAccountStds(entities, base.ActorCompanyId, timePayrollScheduleTransactionItems.Select(i => i.AccountId).Distinct().ToList(), false);

                //TimePayrollScheduleTransactions AccountInternals
                if (timePayrollScheduleTransactionAccountInternalItems == null && !ignoreAccounting)
                    timePayrollScheduleTransactionAccountInternalItems = TimeTransactionManager.GetTimePayrollScheduleTransactionAccountsForEmployee(entities, null, timePeriod.StartDate, timePeriod.StopDate, timePeriod.TimePeriodId, employee.EmployeeId).ToList();
            }
            else
            {
                timePayrollScheduleTransactionItems = new List<GetTimePayrollScheduleTransactionsForEmployee_Result>();
                timePayrollScheduleTransactionAccountStds = new List<AccountDTO>();
                timePayrollScheduleTransactionAccountInternalItems = new List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result>();
            }

            //EmployeeTimePeriodProductSetting
            List<EmployeeTimePeriodProductSetting> employeeTimePeriodSettings = new List<EmployeeTimePeriodProductSetting>();

            if (getEmployeeTimePeriodSettings)
            {
                List<EmployeeTimePeriod> employeeTimePeriodsForEmployee = employeeTimePeriods?.Where(f => f.TimePeriodId == timePeriod.TimePeriodId && f.EmployeeId == employee.EmployeeId).ToList();
                if (!employeeTimePeriodsForEmployee.IsNullOrEmpty() && employeeTimePeriodsForEmployee.First().EmployeeTimePeriodProductSetting.IsLoaded)
                {
                    foreach (EmployeeTimePeriod employeeTimePeriod in employeeTimePeriodsForEmployee)
                    {
                        employeeTimePeriodSettings.AddRange(employeeTimePeriod.EmployeeTimePeriodProductSetting);
                    }
                }
                else
                {
                    employeeTimePeriodSettings = TimePeriodManager.GetEmployeeTimePeriodProductSettings(entities, employee.EmployeeId, timePeriod.TimePeriodId, base.ActorCompanyId);
                }
            }

            #endregion

            #region Retroactive transactionbasis (MonthlySalary)

            List<TimePeriod> monthlySalaryPeriodBasis = new List<TimePeriod>();
            List<RetroactivePayrollBasis> retroactivePayrollBasis = new List<RetroactivePayrollBasis>();

            List<int> monthlySalaryOutcomeIds = timePayrollTransactionItems.Where(x => x.RetroactivePayrollOutcomeId.HasValue && x.IsMonthlySalary()).Select(x => x.RetroactivePayrollOutcomeId.Value).ToList();
            if (monthlySalaryOutcomeIds.Any())
            {
                retroactivePayrollBasis = (from t in entities.RetroactivePayrollBasis
                                                .Include("BasisTimePayrollTransaction")
                                           where monthlySalaryOutcomeIds.Contains(t.RetroactivePayrollOutcomeId) &&
                                           t.State == (int)SoeEntityState.Active
                                           select t).ToList();

                List<int?> periodIds = new List<int?>();
                retroactivePayrollBasis.ForEach(x => periodIds.Add(x.BasisTimePayrollTransaction.TimePeriodId));
                monthlySalaryPeriodBasis = TimePeriodManager.GetTimePeriods(entities, periodIds.Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList(), actorCompanyId);
            }

            #endregion

            #region Validate TimePayrollTransactions

            List<AttestPayrollTransactionDTO> transactionItems = new List<AttestPayrollTransactionDTO>();

            foreach (var timePayrollTransactionGrouping in timePayrollTransactionItems.GroupBy(i => i.ProductId))
            {
                bool hasInfo = employeeTimePeriodSettings != null && employeeTimePeriodSettings.Where(x => x.PayrollProductId == timePayrollTransactionGrouping.Key).ToList().Count > 0;

                foreach (var timePayrollTransactionItem in timePayrollTransactionGrouping)
                {
                    List<AccountDTO> accountInternals = timePayrollTransactionAccountInternalItems?.GetAccountInternals(timePayrollTransactionItem.TimePayrollTransactionId);
                    AccountDTO accountStd = timePayrollTransactionAccountStds?.FirstOrDefault(i => i.AccountId == timePayrollTransactionItem.AccountId);

                    AttestPayrollTransactionDTO transactionItem = timePayrollTransactionItem.CreateTransactionItem(accountInternals, accountStd, companyAccountDims, hasInfo: hasInfo, ignoreAccounting: ignoreAccounting);
                    if (transactionItem != null && transactionItem.PayrollProductUseInPayroll)
                        transactionItems.Add(transactionItem);
                }
            }

            #endregion

            #region Validate TimePayrollScheduleTransactions

            var validTimePayrollScheduleTransactionItems = new List<GetTimePayrollScheduleTransactionsForEmployee_Result>();
            foreach (var timePayrollScheduleTransactionsByDate in timePayrollScheduleTransactionItems.GroupBy(i => i.TimeBlockDateId))
            {
                int timeBlockDateId = timePayrollScheduleTransactionsByDate.Key;

                var timePayrollScheduleTransactionsByType = (from tpt in timePayrollScheduleTransactionsByDate
                                                             where tpt.Type == (int)SoeTimePayrollScheduleTransactionType.Absence &&
                                                             tpt.TimeBlockDateId == timeBlockDateId
                                                             select tpt).ToList();

                if (!timePayrollScheduleTransactionsByType.Any() && employeeGroup != null && !employeeGroup.AutogenTimeblocks && !timePayrollTransactionItems.Any(x => x.TimeBlockDateId == timeBlockDateId && !x.IsNetSalary() && !x.IsEmploymentTax() && !x.IsTaxAndNotOptional()))
                {
                    //Show only for stamping and if no TimePayrollTransactions exists                                           
                    timePayrollScheduleTransactionsByType = (from tpt in timePayrollScheduleTransactionsByDate
                                                             where tpt.Type == (int)SoeTimePayrollScheduleTransactionType.Schedule &&
                                                             tpt.TimeBlockDateId == timeBlockDateId
                                                             select tpt).ToList();
                }

                if (timePayrollScheduleTransactionsByType.Any())
                    validTimePayrollScheduleTransactionItems.AddRange(timePayrollScheduleTransactionsByType);
            }

            foreach (var timePayrollScheduleTransactionsByProduct in validTimePayrollScheduleTransactionItems.GroupBy(i => i.ProductId))
            {
                int productId = timePayrollScheduleTransactionsByProduct.Key;
                bool hasInfo = employeeTimePeriodSettings?.Any(x => x.PayrollProductId == productId) ?? false;

                foreach (var timePayrollScheduleTransactionItem in timePayrollScheduleTransactionsByProduct)
                {
                    List<AccountDTO> accountInternals = timePayrollScheduleTransactionAccountInternalItems?.GetAccountInternals(timePayrollScheduleTransactionItem.TimePayrollScheduleTransactionId);
                    AccountDTO accountStd = timePayrollScheduleTransactionAccountStds?.FirstOrDefault(i => i.AccountId == timePayrollScheduleTransactionItem.AccountId);

                    AttestPayrollTransactionDTO transactionItem = timePayrollScheduleTransactionItem.CreateTransactionItem(accountInternals, accountStd, companyAccountDims, (SoeTimePayrollScheduleTransactionType)timePayrollScheduleTransactionItem.Type, attestStateName: GetText(5981, "Preliminär"), hasInfo: hasInfo);
                    if (transactionItem != null && transactionItem.PayrollProductUseInPayroll)
                        transactionItems.Add(transactionItem);
                }
            }

            #endregion

            #region Grouping

            var transactionItemsGrouped = new List<AttestPayrollTransactionDTO>();
            foreach (var transactionItemsGroupedByDay in transactionItems.GroupBy(i => i.Date))
            {
                transactionItemsGrouped.AddRange(PayrollRulesUtil.GroupTransactionsByDay(transactionItemsGroupedByDay.ToList(), showAllTransactions));
            }

            payrollCalculationProducts = PayrollRulesUtil.CreatePayrollCalculationProducts(transactionItemsGrouped, employee.EmployeeId, isPayrollSlip);

            foreach (PayrollCalculationProductDTO payrollCalculationProduct in payrollCalculationProducts)
            {

                if (payrollCalculationProduct.IsMonthlySalary())
                {
                    if (payrollCalculationProduct.IsRetroactive)
                    {
                        DateTime? dateFrom = null;
                        DateTime? dateTo = null;

                        foreach (var transaction in payrollCalculationProduct.AttestPayrollTransactions)
                        {
                            List<int> transactionIds = transaction.TimePayrollTransactionId.ObjToList();
                            if (transaction.AllTimePayrollTransactionIds != null)
                                transactionIds.AddRange(transaction.AllTimePayrollTransactionIds);

                            transactionIds = transactionIds.Distinct().ToList();

                            foreach (var transactionId in transactionIds)
                            {
                                RetroactivePayrollBasis transactionBasis = retroactivePayrollBasis.FirstOrDefault(x => x.RetroTimePayrollTransactionId == transactionId);
                                if (transactionBasis != null && transactionBasis.BasisTimePayrollTransaction != null)
                                {
                                    TimePeriod period = monthlySalaryPeriodBasis.FirstOrDefault(x => x.TimePeriodId == transactionBasis.BasisTimePayrollTransaction.TimePeriodId);
                                    if (period != null)
                                    {
                                        if (!dateFrom.HasValue)
                                            dateFrom = period.PayrollStartDate;
                                        if (!dateTo.HasValue)
                                            dateTo = period.PayrollStopDate;

                                        if (period.PayrollStartDate < dateFrom)
                                            dateFrom = period.PayrollStartDate;
                                        if (period.PayrollStopDate > dateTo)
                                            dateTo = period.PayrollStopDate;
                                    }
                                }
                            }
                        }

                        payrollCalculationProduct.DateFrom = dateFrom;
                        payrollCalculationProduct.DateTo = dateTo;

                    }
                    else
                    {
                        payrollCalculationProduct.DateFrom = timePeriod.PayrollStartDate;
                        payrollCalculationProduct.DateTo = timePeriod.PayrollStopDate;
                    }
                }
                else if (payrollCalculationProduct.IsEmployeeVehicleTransaction() || payrollCalculationProduct.IsAverageCalculated)
                {
                    payrollCalculationProduct.DateFrom = timePeriod.PayrollStartDate;
                    payrollCalculationProduct.DateTo = timePeriod.PayrollStopDate;
                }


                //this should not be loaded every time, decide when it is needed?
                if (payrollCalculationProduct.SysPayrollTypeLevel1.HasValue)
                    payrollCalculationProduct.SysPayrollTypeLevel1Name = GetText(payrollCalculationProduct.SysPayrollTypeLevel1.Value, (int)TermGroup.SysPayrollType);
                if (payrollCalculationProduct.SysPayrollTypeLevel2.HasValue)
                    payrollCalculationProduct.SysPayrollTypeLevel2Name = GetText(payrollCalculationProduct.SysPayrollTypeLevel2.Value, (int)TermGroup.SysPayrollType);
                if (payrollCalculationProduct.SysPayrollTypeLevel3.HasValue)
                    payrollCalculationProduct.SysPayrollTypeLevel3Name = GetText(payrollCalculationProduct.SysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType);
                if (payrollCalculationProduct.SysPayrollTypeLevel4.HasValue)
                    payrollCalculationProduct.SysPayrollTypeLevel4Name = GetText(payrollCalculationProduct.SysPayrollTypeLevel4.Value, (int)TermGroup.SysPayrollType);
            }

            #endregion

            #region EmploymentTaxLimitRuleHidden

            foreach (PayrollCalculationProductDTO payrollCalculationProduct in payrollCalculationProducts)
            {
                if (!hasReachedEmploymentTaxMinimumInCurrentPeriod && !hasReachedEmploymentTaxMinimumBeforeCurrentPeriod && (payrollCalculationProduct.IsEmploymentTax() || payrollCalculationProduct.IsEmploymentTaxBasis()))
                {
                    payrollCalculationProduct.IsBelowEmploymentTaxLimitRuleHidden = true;
                    foreach (var transaction in payrollCalculationProduct.AttestPayrollTransactions)
                    {
                        transaction.IsBelowEmploymentTaxLimitRuleHidden = true;
                    }
                }
            }

            #endregion

            #region EmploymentTax Minimum Rule

            bool isFirstPeriodEmploymenTaxMinimumRuleReached = !hasReachedEmploymentTaxMinimumBeforeCurrentPeriod && hasReachedEmploymentTaxMinimumInCurrentPeriod;
            if (applyEmploymentTaxMinimumRule && isFirstPeriodEmploymenTaxMinimumRuleReached && employmentTaxTimePeriodHeadItem != null)
            {
                var pastPeriods = employmentTaxTimePeriodHeadItem.GetPastPeriods(timePeriod.PaymentDate.Value, isAgd);
                foreach (var pastPeriod in pastPeriods)
                {
                    TimePeriod previousTimePeriod = TimePeriodManager.GetTimePeriod(entities, pastPeriod.TimePeriodId, actorCompanyId);

                    List<PayrollCalculationProductDTO> previousPeriodData = GetPayrollCalculationProducts(
                        entities,
                        actorCompanyId,
                        previousTimePeriod,
                        employee,
                        showAllTransactions: showAllTransactions,
                        employmentTaxTimePeriodHeadItem: employmentTaxTimePeriodHeadItem
,
                        companyAccountDims: companyAccountDims);

                    List<PayrollCalculationProductDTO> employmentTaxBelowHiddenDTOs = previousPeriodData.Where(x => x.IsBelowEmploymentTaxLimitRuleHidden).ToList();
                    foreach (PayrollCalculationProductDTO employmentTaxBelowHiddenDTO in employmentTaxBelowHiddenDTOs)
                    {
                        employmentTaxBelowHiddenDTO.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods = true;
                        employmentTaxBelowHiddenDTO.IsBelowEmploymentTaxLimitRuleHidden = false;
                        foreach (var transaction in employmentTaxBelowHiddenDTO.AttestPayrollTransactions)
                        {
                            transaction.IsBelowEmploymentTaxLimitRuleFromPreviousPeriods = true;
                            transaction.IsBelowEmploymentTaxLimitRuleHidden = false;
                        }
                    }
                    payrollCalculationProducts.AddRange(employmentTaxBelowHiddenDTOs);
                }
            }

            #endregion

            return payrollCalculationProducts;
        }

        public List<PeriodCalculationResultDTO> GetCalculationsFromPeriod(List<int> employeeIds, int timePeriodid)
        {
            List<PeriodCalculationResultDTO> result = new List<PeriodCalculationResultDTO>();

            var currentPeriod = TimePeriodManager.GetTimePeriod(timePeriodid, base.ActorCompanyId, loadTimePeriodHead: true);
            var parentPeriod = TimePeriodManager.GetPeriodsForCalculation(TermGroup_TimePeriodType.RuleWorkTime, currentPeriod.StopDate, currentPeriod.StopDate, base.ActorCompanyId, true).Periods.FirstOrDefault();

            var currentPeriodDict = new Dictionary<int, string>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var transactionsForPeriod = TimeTransactionManager.GetTimePayrollTransactionsWithPlanningPeriodCalculationId(entitiesReadOnly, employeeIds, currentPeriod.TimePeriodId, base.ActorCompanyId);
            if (transactionsForPeriod.Any())
                currentPeriodDict = TimePeriodManager.PayrollProductFromTransactionGroupDict(transactionsForPeriod);

            var parentPeriodDict = new Dictionary<int, string>();
            var transactionsForParentPeriod = parentPeriod != null ? TimeTransactionManager.GetTimePayrollTransactionsWithPlanningPeriodCalculationId(entitiesReadOnly, employeeIds, parentPeriod.TimePeriodId, base.ActorCompanyId) : null;
            if (transactionsForParentPeriod != null && transactionsForParentPeriod.Any())
                parentPeriodDict = TimePeriodManager.PayrollProductFromTransactionGroupDict(transactionsForParentPeriod);

            foreach (var employeeId in employeeIds)
            {
                if (!currentPeriodDict.Any(w => w.Key == employeeId) && !parentPeriodDict.Any(w => w.Key == employeeId))
                    continue;

                result.Add(new PeriodCalculationResultDTO
                {
                    EmployeeId = employeeId,
                    CurrentPeriod = currentPeriodDict.FirstOrDefault(w => w.Key == employeeId).Value ?? "",
                    ParentPeriod = parentPeriodDict.FirstOrDefault(w => w.Key == employeeId).Value ?? "",
                });
            }

            return result;
        }

        #endregion

        #region Group

        public List<PayrollCalculationEmployeePeriodDTO> GetPayrollCalculationEmployeePeriods(int actorCompanyId, int timePeriodId, List<int> visibleEmployeeIds, string cacheKeyToUse, bool flushCache, bool ignoreEmploymentStopDate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollCalculationEmployeePeriods(entities, actorCompanyId, timePeriodId, visibleEmployeeIds, cacheKeyToUse, flushCache, ignoreEmploymentStopDate);
        }

        public List<PayrollCalculationEmployeePeriodDTO> GetPayrollCalculationEmployeePeriods(CompEntities entities, int actorCompanyId, int timePeriodId, List<int> visibleEmployeeIds, string cacheKeyToUse, bool flushCache, bool ignoreEmploymentStopDate)
        {
            List<PayrollCalculationEmployeePeriodDTO> periodItems = new List<PayrollCalculationEmployeePeriodDTO>();

            try
            {
                if (flushCache || string.IsNullOrEmpty(cacheKeyToUse) || !Guid.TryParse(cacheKeyToUse, out Guid cacheKey))
                    cacheKey = Guid.Empty;

                var timePeriod = TimePeriodManager.GetTimePeriod(entities, timePeriodId, actorCompanyId, loadTimePeriodHead: true);
                if (timePeriod?.TimePeriodHead == null || !timePeriod.PaymentDate.HasValue || !timePeriod.PayrollStartDate.HasValue || !timePeriod.PayrollStopDate.HasValue)
                    return periodItems;

                var validEmployees = LoadValidEmployees();
                var employeeIds = validEmployees.Select(x => x.EmployeeId).ToList();
                var employeeTimePeriods = TimePeriodManager.GetEmployeeTimePeriodsWithValues(entities, timePeriod.TimePeriodId, employeeIds, actorCompanyId);
                var transactions = TimeTransactionManager.GetTimePayrollTransactionsForTree(entities, actorCompanyId, timePeriod.StartDate.Date, timePeriod.StopDate.Date, timePeriod, employeeIds, onlyUseInPayroll: true);
                var transactionsByEmployee = transactions.GroupBy(t => t.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
                var attestStateIds = transactions.Select(x => x.AttestStateId).Distinct().ToList();
                var attestStates = AttestManager.GetAttestStates(entities, actorCompanyId, attestStateIds).ToDTOs().ToList();
                validEmployees.ForEach(employee => Project(employee));
                periodItems = periodItems.SortAlphanumeric();

                List<Employee> LoadValidEmployees()
                {
                    var result = new List<Employee>();
                    var (startDate, stopDate) = CalculateDates();
                    var payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId));
                    var employees = GetEmployeesWithRepositoryFromCache(entities, CacheConfig.Company(actorCompanyId, cacheKey, seconds: (int)CacheTTL.ThirtyMinutes, keepAlive: true), out _, base.UserId, base.RoleId, startDate, stopDate, visibleEmployeeIds);
                    foreach (var employee in employees)
                    {
                        bool useTimePeriodDates = false;
                        var employment = employee.GetEmployment(startDate, stopDate, forward: false);
                        if (employment == null && timePeriod.StartDate != timePeriod.PayrollStartDate.Value)
                        {
                            useTimePeriodDates = true;
                            employment = employee.GetEmployment(timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value, forward: false);
                        }
                        if (employment == null && ignoreEmploymentStopDate)
                            employment = employee.GetLastEmployment();
                        if (employment == null)
                            continue;

                        PayrollGroup payrollGroup = employment.GetPayrollGroup(useTimePeriodDates ? timePeriod.PayrollStartDate.Value : startDate, useTimePeriodDates ? timePeriod.PayrollStopDate.Value : stopDate, payrollGroups, forward: false);
                        if (payrollGroup == null || payrollGroup.TimePeriodHeadId != timePeriod.TimePeriodHead.TimePeriodHeadId)
                            continue;

                        result.Add(employee);
                    }
                    return result;
                }
                (DateTime startDate, DateTime stopDate) CalculateDates()
                {
                    if (timePeriod.ExtraPeriod)
                        return (timePeriod.PaymentDate.Value.AddYears(-1).Date, timePeriod.PaymentDate.Value.Date);
                    else
                        return (timePeriod.StartDate, timePeriod.StopDate);

                }
                void Project(Employee employee)
                {
                    var employeeTransactionDates = employee.Employment.GetEmploymentDates(timePeriod.StartDate, timePeriod.StopDate);
                    var attestStateIdsForEmployee = transactionsByEmployee.GetList(employee.EmployeeId).Where(t => employeeTransactionDates.Contains(t.Date) || t.TimePeriodId.HasValue).Select(i => i.AttestStateId).Distinct().ToList();
                    var employeeTimePeriod = employeeTimePeriods.FirstOrDefault(i => i.EmployeeId == employee.EmployeeId);

                    var periodItem = new PayrollCalculationEmployeePeriodDTO
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeNr = employee.EmployeeNr,
                        EmployeeName = employee.Name,
                        TimePeriodId = timePeriod.TimePeriodId,
                        CreatedOrModified = employeeTimePeriod?.Modified ?? employeeTimePeriod?.Created,
                        PeriodSum = new PayrollCalculationPeriodSumDTO()
                        {
                            Gross = employeeTimePeriod?.GetGrossSalarySum() ?? 0,
                            BenefitInvertExcluded = employeeTimePeriod?.GetBenefitSum() ?? 0,
                            Tax = employeeTimePeriod?.GetTaxSum() ?? 0,
                            Compensation = employeeTimePeriod?.GetCompensationSum() ?? 0,
                            Deduction = employeeTimePeriod?.GetDeductionSum() ?? 0,
                            EmploymentTaxDebit = employeeTimePeriod?.GetEmploymentTaxCreditSum() ?? 0,
                            Net = employeeTimePeriod?.GetNetSum() ?? 0,
                        }
                    };
                    periodItem.SetAttestStates(attestStates.Filter(attestStateIdsForEmployee));
                    periodItems.Add(periodItem);
                }
            }
            catch (Exception ex)
            {
                base.LogError(ex, this.log);
            }

            return periodItems;
        }

        #endregion

        #region Chart

        public DateChart GetSalaryHistoryForEmployee(int actorCompanyId, int timePeriodId, int employeeId)
        {
            #region Prereq

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            TimePeriod currentTimePeriod = TimePeriodManager.GetTimePeriod(entitiesReadOnly, timePeriodId, actorCompanyId, loadTimePeriodHead: true);
            if (currentTimePeriod == null || !currentTimePeriod.PaymentDate.HasValue)
                return new DateChart();

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(currentTimePeriod.TimePeriodHead.TimePeriodHeadId, actorCompanyId);

            #endregion

            DateChart dto = new DateChart()
            {
                Data = new List<DateChartData>(),
            };

            bool currentTimePeriodFound = false;
            foreach (TimePeriod timePeriod in timePeriods)
            {
                if (currentTimePeriodFound && timePeriod.PaymentDate.HasValue && timePeriod.PaymentDate.Value < currentTimePeriod.PaymentDate.Value.AddMonths(-12))
                    break;

                // Loop until current period is found (backwards)
                if (timePeriod.TimePeriodId == currentTimePeriod.TimePeriodId)
                    currentTimePeriodFound = true;

                if (!currentTimePeriodFound)
                    continue;

                if (timePeriod.IsExtraPeriod())
                    continue;

                decimal grossSalary = 0;
                decimal netSalary = 0;
                decimal employmentTax = 0;

                // EmployeeTimePeriod
                EmployeeTimePeriod employeeTimePeriod = TimePeriodManager.GetEmployeeTimePeriod(employeeId, timePeriod.TimePeriodId, actorCompanyId, true);
                if (employeeTimePeriod != null)
                {
                    grossSalary = employeeTimePeriod.EmployeeTimePeriodValue.FirstOrDefault(v => v.Type == (int)SoeEmployeeTimePeriodValueType.GrossSalary)?.Value ?? 0;
                    netSalary = employeeTimePeriod.EmployeeTimePeriodValue.FirstOrDefault(v => v.Type == (int)SoeEmployeeTimePeriodValueType.NetSalary)?.Value ?? 0;
                    employmentTax = employeeTimePeriod.EmployeeTimePeriodValue.FirstOrDefault(v => v.Type == (int)SoeEmployeeTimePeriodValueType.EmploymentTaxCredit)?.Value ?? 0;
                }

                DateChartData series = new DateChartData(timePeriod.PaymentDate ?? timePeriod.StartDate)
                {
                    Values = new List<DateChartValue>
                    {
                        new DateChartValue((int)SoeEmployeeTimePeriodValueType.GrossSalary, grossSalary),
                        new DateChartValue((int)SoeEmployeeTimePeriodValueType.NetSalary, netSalary),
                        new DateChartValue((int)SoeEmployeeTimePeriodValueType.EmploymentTaxCredit, employmentTax * -1)
                    }
                };

                dto.Data.Add(series);
            }

            return dto;
        }

        public DateChart GetAverageSalaryCostForEmployees(int actorCompanyId, int timePeriodId, List<int> employeeIds)
        {
            #region Prereq

            TimePeriod currentTimePeriod = TimePeriodManager.GetTimePeriod(timePeriodId, actorCompanyId, loadTimePeriodHead: true);
            if (currentTimePeriod == null || !currentTimePeriod.PaymentDate.HasValue || currentTimePeriod.TimePeriodHead == null)
                return new DateChart();

            List<Employee> employees = EmployeeManager.GetAllEmployeesByIds(actorCompanyId, employeeIds);
            List<int> males = employees.Where(e => e.Sex == TermGroup_Sex.Male).Select(x => x.EmployeeId).ToList();
            List<int> females = employees.Where(e => e.Sex == TermGroup_Sex.Female).Select(x => x.EmployeeId).ToList();

            DateTime paymentDate = currentTimePeriod.PaymentDate.Value;
            DateTime fromDate = CalendarUtility.GetBeginningOfYear(paymentDate.AddYears(-1));

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<EmployeeTimePeriod> employeeTimePeriods = TimePeriodManager.GetLockedAndAboveEmployeesTimePeriodsWithValuesAndTimeperiod(entitiesReadOnly, actorCompanyId, employeeIds, out List<TimePeriod> timePeriods);

            timePeriods = timePeriods.Where(x => x.PaymentDate.HasValue && fromDate <= x.PaymentDate.Value && x.PaymentDate.Value <= paymentDate).ToList();
            List<TimePeriod> previousTimePeriods = timePeriods.Where(x => x.PaymentDate.Value.Year == fromDate.Year).ToList();
            List<TimePeriod> currentTimePeriods = timePeriods.Where(x => x.PaymentDate.Value.Year == paymentDate.Year).ToList();

            List<EmployeeTimePeriod> previousYearPeriods = employeeTimePeriods.Where(x => previousTimePeriods.Select(p => p.TimePeriodId).Contains(x.TimePeriodId)).ToList();
            List<EmployeeTimePeriod> currentYearPeriods = employeeTimePeriods.Where(x => currentTimePeriods.Select(p => p.TimePeriodId).Contains(x.TimePeriodId)).ToList();

            #endregion

            DateChart dto = new DateChart()
            {
                Data = new List<DateChartData>(),
            };

            DateChartData prevYearSeries = new DateChartData(paymentDate.AddYears(-1))
            {
                Values = new List<DateChartValue>()
                {
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Men, GetAverageValue(previousYearPeriods, previousTimePeriods, males, SoeEmployeeTimePeriodValueType.GrossSalary)),
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Women, GetAverageValue(previousYearPeriods, previousTimePeriods, females, SoeEmployeeTimePeriodValueType.GrossSalary)),
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Total, GetAverageValue(previousYearPeriods, previousTimePeriods, employeeIds, SoeEmployeeTimePeriodValueType.GrossSalary)),
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Median, GetMedianValue(previousYearPeriods, previousTimePeriods, employeeIds, SoeEmployeeTimePeriodValueType.GrossSalary)),
                }
            };
            dto.Data.Add(prevYearSeries);

            DateChartData currentYearSeries = new DateChartData(paymentDate)
            {
                Values = new List<DateChartValue>()
                {
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Men, GetAverageValue(currentYearPeriods, currentTimePeriods, males, SoeEmployeeTimePeriodValueType.GrossSalary)),
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Women, GetAverageValue(currentYearPeriods, currentTimePeriods, females, SoeEmployeeTimePeriodValueType.GrossSalary)),
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Total, GetAverageValue(currentYearPeriods, currentTimePeriods, employeeIds, SoeEmployeeTimePeriodValueType.GrossSalary)),
                    new DateChartValue((int)TermGroup_AverageSalaryCostChartSeriesType.Median, GetMedianValue(currentYearPeriods, currentTimePeriods, employeeIds, SoeEmployeeTimePeriodValueType.GrossSalary)),
                }
            };
            dto.Data.Add(currentYearSeries);

            return dto;
        }

        private decimal GetAverageValue(List<EmployeeTimePeriod> employeeTimePeriods, List<TimePeriod> timePeriods, List<int> employeeIds, SoeEmployeeTimePeriodValueType valueType)
        {
            List<decimal> monthAvrages = new List<decimal>();
            List<EmployeeTimePeriod> currentEmployeeTimePeriods = employeeTimePeriods.Where(etp => employeeIds.Contains(etp.EmployeeId) && etp.EmployeeTimePeriodValue.Any(etpv => etpv.Type == (int)valueType && etpv.Value > 0)).ToList();
            foreach (var timeperiodsByMonth in timePeriods.GroupBy(tp => tp.PaymentDate.Value.Month))
            {
                var periodsByMonth = currentEmployeeTimePeriods.Where(etp => timeperiodsByMonth.Select(x => x.TimePeriodId).Contains(etp.TimePeriodId)).ToList();
                if (periodsByMonth.Any())
                {
                    int employeesCount = periodsByMonth.Select(x => x.EmployeeId).Distinct().Count();
                    decimal typeValue = periodsByMonth.SelectMany(x => x.EmployeeTimePeriodValue).Where(etpv => etpv.Type == (int)valueType).Sum(etpv => etpv.Value);
                    monthAvrages.Add(typeValue / employeesCount);
                }
            }

            return monthAvrages.Any() ? monthAvrages.Average() : 0;
        }

        private decimal GetMedianValue(List<EmployeeTimePeriod> employeeTimePeriods, List<TimePeriod> timePeriods, List<int> employeeIds, SoeEmployeeTimePeriodValueType valueType)
        {
            List<decimal> monthMedians = new List<decimal>();
            List<EmployeeTimePeriod> currentEmployeeTimePeriods = employeeTimePeriods.Where(etp => employeeIds.Contains(etp.EmployeeId) && etp.EmployeeTimePeriodValue.Any(etpv => etpv.Type == (int)valueType && etpv.Value > 0)).ToList();
            foreach (var timeperiodsByMonth in timePeriods.GroupBy(tp => tp.PaymentDate.Value.Month))
            {
                var periodsByMonth = currentEmployeeTimePeriods.Where(etp => timeperiodsByMonth.Select(x => x.TimePeriodId).Contains(etp.TimePeriodId)).ToList();
                if (periodsByMonth.Any())
                {
                    List<decimal> currentMonthValues = new List<decimal>();
                    foreach (var periodsByMonthAndEmployee in periodsByMonth.GroupBy(x => x.EmployeeId))
                    {
                        currentMonthValues.Add(periodsByMonthAndEmployee.SelectMany(x => x.EmployeeTimePeriodValue).Where(etpv => etpv.Type == (int)valueType).Sum(etpv => etpv.Value));
                    }

                    if (currentMonthValues.Any())
                        monthMedians.Add(NumberUtility.GetMedianValue(currentMonthValues));
                }
            }

            return NumberUtility.GetMedianValue(monthMedians);
        }

        #endregion
    }
}
