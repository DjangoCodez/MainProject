using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Saves attest for a Employee
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveAttestForEmployeeOutputDTO TaskSaveAttestForEmployee()
        {
            var (iDTO, oDTO) = InitTask<SaveAttestForEmployeeInputDTO, SaveAttestForEmployeeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            SaveAttestEmployeeDayDTO first = iDTO.InputItems?.OrderBy(i => i.Date).FirstOrDefault();
            SaveAttestEmployeeDayDTO last = iDTO.InputItems?.OrderByDescending(i => i.Date).FirstOrDefault();
            if (first == null || last == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11952, "Välj dag att attestera"));
                return oDTO;
            }

            #endregion

            int noOfValidTransactions = 0;
            int noOfInvalidTransactions = 0;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                AttestStateDTO attestStateTo = GetAttestStateFromCache(iDTO.AttestStateId);
                if (attestStateTo == null)
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));
                    return oDTO;
                }

                //Get all AttestTransitions that goes to given AttestState
                List<AttestTransitionDTO> attestTransitionsToState = GetAttestTransitionsToState(iDTO.AttestStateId);
                if (attestTransitionsToState.IsNullOrEmpty())
                {
                    oDTO.Result = new ActionResult(false, (int)ActionResultSave.SaveAttestNoAttestTransitions, GetText(10086, "Inga attestövergångar hittades"));
                    return oDTO;
                }

                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(iDTO.EmployeeId);
                if (employee == null)
                {
                    oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
                    return oDTO;
                }

                #endregion

                EmployeeAttestResult employeeResult = new EmployeeAttestResult(employee);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        DateTime dateFrom = first.Date;
                        DateTime dateTo = last.Date;
                        DateTime logDate = DateTime.Now;

                        #region Load AttestTransitions

                        GetValidAttestTransitions(employee, dateFrom, dateTo, iDTO.IsMySelf, out var userValidTransitions, out var employeeGroupTransitions);
                        if (iDTO.IsMySelf && employeeGroupTransitions.IsNullOrEmpty())
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsEmployeeGroup, GetText(10092, "Inga attestövergångar hittades"));
                            return oDTO;
                        }

                        #endregion

                        #region Load accounts

                        List<int> validAccountIdsByHiearchy = new List<int>();
                        int employeeAccountDimId = 0;
                        bool useValidAccountsByHiearchy = false;
                        bool alsoAttestAdditionsFromTime = iDTO.IsMySelf; //If is my self, it will be decided later date by date depending on EmployeeGroup setting
                        if (!iDTO.ForceWholeDay && !iDTO.IsMySelf)
                        {
                            useValidAccountsByHiearchy = AccountManager.TryGetAccountIdsForEmployeeAccountDim(entities, out AccountRepository accountRepository, actorCompanyId, RoleId, userId, dateFrom, dateTo, out validAccountIdsByHiearchy, out employeeAccountDimId);
                            alsoAttestAdditionsFromTime = AttestManager.AlsoAttestAdditionsFromTime(entities, userId, actorCompanyId, dateFrom, repository: accountRepository);
                        }

                        List<AccountDTO> accountInternals = useValidAccountsByHiearchy ? GetAccountInternalsFromCache() : null;

                        #endregion

                        #region Load TimeBlocks

                        //TimeBlockDates
                        List<int> timeBlockDateIds = iDTO.InputItems.Select(i => i.TimeBlockDateId).Distinct().ToList();

                        //TimeBlocks for employee and period
                        List<TimeBlock> timeBlocks = useValidAccountsByHiearchy ?
                            GetTimeBlocksWithDateAndTimePayrollTransactionAndAccountInternals(employee.EmployeeId, timeBlockDateIds) :
                            GetTimeBlocksWithDateAndTimePayrollTransaction(employee.EmployeeId, timeBlockDateIds);
                        AddTimeBlockDatesToCache(employee.EmployeeId, timeBlocks.Select(tb => tb.TimeBlockDate).Distinct().ToList());

                        #endregion

                        #region Load TimePayrollTransactions

                        //TimePayrollTransactions without TimeBlocks for employee and period
                        List<TimePayrollTransaction> timePayrollTransactionsWithoutTimeBlock = useValidAccountsByHiearchy ?
                            GetTimePayrollTransactionsWithAccountInternals(employee.EmployeeId, timeBlockDateIds, onlyUseInPayroll: false, onlyTransactionsWithoutTimeBlocks: true) :
                            GetTimePayrollTransactions(employee.EmployeeId, timeBlockDateIds, onlyUseInPayroll: false, onlyTransactionsWithoutTimeBlocks: true);

                        if (!iDTO.IsPayrollAttest)
                            timePayrollTransactionsWithoutTimeBlock = timePayrollTransactionsWithoutTimeBlock.Where(t => !t.IsExcludedInTime()).ToList();
                        if (!alsoAttestAdditionsFromTime)
                            timePayrollTransactionsWithoutTimeBlock = timePayrollTransactionsWithoutTimeBlock.Where(t => !t.IsAdditionOrDeduction).ToList();
                        timePayrollTransactionsWithoutTimeBlock = timePayrollTransactionsWithoutTimeBlock.ExcludeStartValues();

                        int payrollResultingAttestStateId = !iDTO.IsPayrollAttest ? GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus) : 0;
                        int payrollFileCreatedAttestStateId = iDTO.IsPayrollAttest ? GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId) : 0;

                        #endregion

                        #region Load EmployeeGroups

                        List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

                        #endregion

                        #region Perform

                        var inputDays = iDTO.InputItems.ToDictionary(k => k.TimeBlockDateId, v => v.Date);

                        foreach (var day in inputDays)
                        {
                            #region Day

                            int timeBlockDateId = day.Key;
                            DateTime date = day.Value;

                            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups: employeeGroups);
                            if (employeeGroup == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12079, "Tidavtal saknas för anställd {0} den {1}"), employee.EmployeeNr, date.ToShortDateString()));
                                return oDTO;
                            }

                            List<TimeBlock> timeBlocksForDate = timeBlocks.Where(tb => tb.TimeBlockDateId == timeBlockDateId).ToList();

                            //Duplicate check
                            if (!attestStateTo.Initial && timeBlocksForDate.ContainsDuplicateTimeBlocks())
                            {
                                oDTO.Result = GetAttestFailedDuplicateTimeBlocksResult(employee, timeBlocksForDate);
                                return oDTO;
                            }

                            //TimePayrollTransactions without TimeBlock
                            List<TimePayrollTransaction> timePayrollTransactionsForDate = iDTO.IsMySelf && !employeeGroup.AlsoAttestAdditionsFromTime
                                ? timePayrollTransactionsWithoutTimeBlock.Where(tb => tb.TimeBlockDateId == timeBlockDateId && !tb.IsAdditionOrDeduction).ToList()
                                : timePayrollTransactionsWithoutTimeBlock.Where(tb => tb.TimeBlockDateId == timeBlockDateId).ToList();

                            //TimePayrollTransactions with TimeBlock
                            timePayrollTransactionsForDate.AddRange(GetValidTransactionsFromTimeBlocksForAttest(employeeResult, timeBlocksForDate, employee, attestStateTo));

                            //Plausibility check before attest
                            if (useValidAccountsByHiearchy)
                                ApplyPlausibilityCheck(employee, date, new List<TimeCodeTransaction>(), timePayrollTransactionsForDate, timeBlocksForDate, "Before attest");

                            //Validate transactions
                            List<TimePayrollTransaction> validTimePayrollTransactions = useValidAccountsByHiearchy
                                ? GetValidTransactionsByAccountHieararchyForAttest(employeeResult, timePayrollTransactionsForDate, employeeAccountDimId, validAccountIdsByHiearchy, accountInternals)
                                : timePayrollTransactionsForDate;

                            //ForceWholeDay
                            if (!useValidAccountsByHiearchy && iDTO.ForceWholeDay)
                            {
                                List<AttestTransitionLog> attestTransitionLogs = GetAttestTransitionLogs(timePayrollTransactionsForDate.Select(t => t.TimePayrollTransactionId).ToList());

                                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsForDate)
                                {
                                    AttestTransitionLog attestTransitionLog = attestTransitionLogs.FirstOrDefault(i => i.RecordId == timePayrollTransaction.TimePayrollTransactionId);
                                    if (attestTransitionLog?.AttestTransition != null && attestTransitionLog.AttestTransition.AttestStateToId != iDTO.AttestStateId)
                                        AddCurrentDayAsUnlocked(attestTransitionLog.UserId, date);
                                }
                            }

                            noOfInvalidTransactions += employeeResult.NoOfTranscationsFailed;

                            //Set AttestState
                            oDTO.Result = TrySetTimePayrollTransactionsAttestState(
                                validTimePayrollTransactions,
                                attestStateTo, 
                                attestTransitionsToState,
                                userValidTransitions, 
                                employeeGroupTransitions, 
                                logDate, 
                                isMySelf: iDTO.IsMySelf,
                                noOfValidTransactions: ref noOfValidTransactions,
                                noOfInvalidTransactions: ref noOfInvalidTransactions,
                                validatePayrollLockedAttestState: true,
                                tryNotifyEmployee: true, 
                                payrollResultingAttestStateId, payrollFileCreatedAttestStateId
                                );
                            if (!oDTO.Result.Success)
                                return oDTO;

                            //Plausibility check after attest
                            if (useValidAccountsByHiearchy)
                                ApplyPlausibilityCheck(employee, date, new List<TimeCodeTransaction>(), validTimePayrollTransactions, timeBlocksForDate, "After attest");

                            //ForceWholeDay validation
                            if (iDTO.ForceWholeDay && noOfValidTransactions == 0 && noOfInvalidTransactions > 0)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransition, GetText(11830, "Giltig attestövergång saknas"));
                                return oDTO;
                            }

                            #endregion
                        }

                        #endregion

                        #region Save and notify

                        oDTO.Result = Save();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        DoNotifyChangeOfAttestState();
                        DoNotifyUnlockedDays(employee);

                        TryCommit(oDTO);

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                    {
                        oDTO.Result.IntegerValue = noOfValidTransactions;
                        oDTO.Result.IntegerValue2 = noOfInvalidTransactions;
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }

                return oDTO;
            }
        }

        /// <summary>
        /// Saves attest for a collection of Employees
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveAttestForEmployeesOutputDTO TaskSaveAttestForEmployees()
        {
            var (iDTO, oDTO) = InitTask<SaveAttestForEmployeesInputDTO, SaveAttestForEmployeesOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            EmployeesAttestResult result = new EmployeesAttestResult(iDTO.AttestStateId);

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                AttestStateDTO attestStateTo = GetAttestState(iDTO.AttestStateId);
                if (attestStateTo == null)
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));
                    return oDTO;
                }

                List<AttestTransitionDTO> attestTransitionsToState = GetAttestTransitionsToState(iDTO.AttestStateId);
                if (attestTransitionsToState.IsNullOrEmpty())
                {
                    oDTO.Result = new ActionResult(false, (int)ActionResultSave.SaveAttestNoAttestTransitions, GetText(10086, "Inga attestövergångar hittades"));
                    return oDTO;
                }

                #endregion

                try
                {
                    DateTime dateFrom = CalendarUtility.DATETIME_DEFAULT;
                    DateTime dateTo = CalendarUtility.DATETIME_DEFAULT;
                    DateTime logDate = DateTime.Now;

                    #region Load TimePeriod

                    if (iDTO.TimePeriodId.HasValue)
                    {
                        TimePeriod timePeriod = GetTimePeriodFromCache(iDTO.TimePeriodId.Value);
                        if (timePeriod == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                            return oDTO;
                        }

                        dateFrom = CalendarUtility.GetBeginningOfDay(timePeriod.StartDate);
                        dateTo = CalendarUtility.GetEndOfDay(timePeriod.StopDate);
                    }
                    else
                    {
                        if (iDTO.StartDate.HasValue)
                            dateFrom = iDTO.StartDate.Value;
                        if (iDTO.StopDate.HasValue)
                            dateTo = iDTO.StopDate.Value;
                    }

                    #endregion

                    #region Load AttestTransitions

                    List<AttestUserRoleView> userValidTransitions = GetAttestUserRoleViews(dateFrom, dateTo);
                    if (userValidTransitions.IsNullOrEmpty())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsAttestRole, GetText(10086, "Inga attestövergångar hittades"));
                        return oDTO;
                    }

                    #endregion

                    #region Load Accounts

                    bool useValidAccountsByHiearchy = AccountManager.TryGetAccountIdsForEmployeeAccountDim(entities, out _, actorCompanyId, RoleId, userId, dateFrom, dateTo, out List<int> validAccountIdsByHiearchy, out int employeeAccountDimId);
                    List<AccountDTO> accountInternals = useValidAccountsByHiearchy ? GetAccountInternalsFromCache() : null;

                    #endregion

                    #region Load settings

                    int payrollResultingAttestStateId = !iDTO.IsPayrollAttest ? GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus) : 0;
                    int payrollFileCreatedAttestStateId = iDTO.IsPayrollAttest ? GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId) : 0;

                    #endregion

                    #region Perform

                    foreach (int employeeId in iDTO.EmployeeIds)
                    {
                        #region Employee

                        Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, false);
                        if (employee == null)
                            continue;

                        EmployeeAttestResult employeeResult = new EmployeeAttestResult(employee);

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            #region Load data for Employee

                            //AttestTransitions
                            List<AttestTransitionDTO> employeeGroupTransitions = employee.EmployeeId == iDTO.CurrentEmployeeId ? GetAttestTransitionsForEmployee(iDTO.CurrentEmployeeId, TermGroup_AttestEntity.PayrollTime) : new List<AttestTransitionDTO>();

                            //TimeBlocks for employee and period
                            List<TimeBlock> allTimeBlocks = useValidAccountsByHiearchy ?
                                GetTimeBlocksWithDateAndTimePayrollTransactionAndAccountInternals(employeeId, dateFrom, dateTo) :
                                GetTimeBlocksWithDateAndTimePayrollTransaction(employeeId, dateFrom, dateTo);
                            AddTimeBlockDatesToCache(employee.EmployeeId, allTimeBlocks.Select(tb => tb.TimeBlockDate).Distinct().ToList());

                            //TimePayrollTransactions without TimeBlocks for employee (only time transactions - exclude transactions with periodid)
                            List<TimePayrollTransaction> timePayrollTransactionsWithoutTimeBlocks = useValidAccountsByHiearchy ?
                                GetTimePayrollTransactionsWithAccountInternals(employee.EmployeeId, dateFrom, dateTo, onlyTransactionsWithoutTimeBlocks: true) :
                                GetTimePayrollTransactions(employee.EmployeeId, dateFrom, dateTo, onlyTransactionsWithoutTimeBlocks: true);
                            timePayrollTransactionsWithoutTimeBlocks = timePayrollTransactionsWithoutTimeBlocks
                                .Where(t => !t.TimePeriodId.HasValue || t.IsAdditionOrDeduction || t.IsRetroactive)
                                .ExcludeStartValues()
                                .ToList();

                            //TimePayrollTransactions with periodId 
                            List<TimePayrollTransaction> timePayrollTransactionsWithPeriod = new List<TimePayrollTransaction>();
                            if (iDTO.IsPayrollAttest && iDTO.TimePeriodId.HasValue)
                            {
                                timePayrollTransactionsWithPeriod = useValidAccountsByHiearchy ?
                                    GetTimePayrollTransactionsWithAccountInternals(employee.EmployeeId, iDTO.TimePeriodId.Value) :
                                    GetTimePayrollTransactions(employee.EmployeeId, iDTO.TimePeriodId.Value);
                                timePayrollTransactionsWithPeriod = timePayrollTransactionsWithPeriod.ExcludeStartValues();
                            }

                            //TimeBlockDates
                            List<int> timeBlockDateIds = allTimeBlocks
                                .Select(i => i.TimeBlockDateId)
                                .Concat(timePayrollTransactionsWithoutTimeBlocks.Select(i => i.TimeBlockDateId))
                                .Concat(timePayrollTransactionsWithPeriod.Select(x => x.TimeBlockDateId))
                                .Distinct()
                                .ToList();

                            #endregion

                            #region Validate Employee

                            List<TimePayrollTransaction> validTimePayrollTransactions = new List<TimePayrollTransaction>();

                            foreach (int timeBlockDateId in timeBlockDateIds)
                            {
                                List<TimeBlock> timeBlocksForDate = allTimeBlocks.Where(tb => tb.TimeBlockDateId == timeBlockDateId).ToList();
                                if (!attestStateTo.Initial && timeBlocksForDate.ContainsDuplicateTimeBlocks())
                                {
                                    oDTO.Result = GetAttestFailedDuplicateTimeBlocksResult(employee, timeBlocksForDate);
                                    return oDTO;
                                }

                                //TimePayrollTransactions
                                List<TimePayrollTransaction> timePayrollTransactionsForDate = new List<TimePayrollTransaction>();
                                timePayrollTransactionsForDate.AddRange(timePayrollTransactionsWithoutTimeBlocks.Where(tb => tb.TimeBlockDateId == timeBlockDateId));
                                timePayrollTransactionsForDate.AddRange(timePayrollTransactionsWithPeriod.Where(tb => tb.TimeBlockDateId == timeBlockDateId));
                                timePayrollTransactionsForDate.AddRange(GetValidTransactionsFromTimeBlocksForAttest(employeeResult, timeBlocksForDate, employee, attestStateTo));
                                timePayrollTransactionsForDate = timePayrollTransactionsForDate.Distinct().ToList();
                                if (iDTO.IsPayrollAttest)
                                    timePayrollTransactionsForDate = FilterTimePayrollTransactionsByUseInPayroll(timePayrollTransactionsForDate);

                                //Plausibility check before attest
                                if (useValidAccountsByHiearchy)
                                    ApplyPlausibilityCheck(employee, GetTimeBlockDateFromCache(employee.EmployeeId, timeBlockDateId)?.Date ?? CalendarUtility.DATETIME_DEFAULT, new List<TimeCodeTransaction>(), timePayrollTransactionsForDate, timeBlocksForDate, "Before attest");

                                //Validate transactions
                                validTimePayrollTransactions.AddRange(useValidAccountsByHiearchy
                                    ? GetValidTransactionsByAccountHieararchyForAttest(employeeResult, timePayrollTransactionsForDate, employeeAccountDimId, validAccountIdsByHiearchy, accountInternals)
                                    : timePayrollTransactionsForDate);
                            }

                            #endregion

                            #region Set AttestState

                            oDTO.Result = TrySaveTimePayrollTransactionsAttestState(
                                employeeResult,
                                validTimePayrollTransactions,
                                attestStateTo,
                                attestTransitionsToState,
                                userValidTransitions,
                                employeeGroupTransitions,
                                logDate,
                                isMySelf: employee.EmployeeId == iDTO.CurrentEmployeeId,
                                validatePayrollLockedAttestState: true,
                                tryNotifyEmployee: true,
                                payrollResultingAttestStateId, payrollFileCreatedAttestStateId
                                );
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion

                            #region Plausability Check After Attest

                            if (useValidAccountsByHiearchy && base.LicenseId == 281 && base.ActorCompanyId == 326666)
                            {
                                foreach (var groupedItems in validTimePayrollTransactions.GroupBy(x=> x.TimeBlockDateId))
                                {
                                    DateTime date = GetTimeBlockDateFromCache(employee.EmployeeId, groupedItems.Key)?.Date ?? CalendarUtility.DATETIME_DEFAULT;
                                    ApplyPlausibilityCheck(employee, date, new List<TimeCodeTransaction>(), groupedItems.ToList(), allTimeBlocks.Where(x => x.TimeBlockDateId == groupedItems.Key).ToList(), "After attest");
                                }                                
                            }

                            #endregion

                            #region Save and notify

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                                break;

                            DoNotifyChangeOfAttestState();

                            #endregion

                            TryCommit(oDTO);
                        }

                        result.AddEmployeeResult(employeeResult);

                        #endregion
                    }

                    //Temporary solution for mobile app
                    oDTO.Result.IntegerValue = result.EmployeeResults.Sum(s => s.NoOfTranscationsAttested);
                    oDTO.Result.IntegerValue2 = result.EmployeeResults.Sum(s => s.NoOfTranscationsFailed);

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                        oDTO.Result.Value = result;
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Saves attest for a transactions
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveAttestForTransactionsOutputDTO TaskSaveAttestForTransactions()
        {
            var (iDTO, oDTO) = InitTask<SaveAttestForTransactionsInputDTO, SaveAttestForTransactionsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            SaveAttestTransactionDTO first = iDTO.InputItems?.OrderBy(i => i.Date).FirstOrDefault();
            SaveAttestTransactionDTO last = iDTO.InputItems?.OrderByDescending(i => i.Date).FirstOrDefault();
            if (first == null || last == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "SaveAttestForTransactionsInputDTO");
                return oDTO;
            }

            int noOfValidTransactions = 0;
            int noOfInvalidTransactions = 0;

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        DateTime dateFrom = first.Date;
                        DateTime dateTo = last.Date;
                        DateTime logDate = DateTime.Now;

                        foreach (var itemsByEmployee in iDTO.InputItems.GroupBy(i => i.EmployeeId))
                        {
                            Employee employee = GetEmployeeFromCache(itemsByEmployee.Key);
                            if (employee == null)
                            {
                                oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10083, "Anställd hittades inte"));
                                return oDTO;
                            }

                            oDTO.Result = SaveTimePayrollTransactionsAttestState(
                                employee.EmployeeId,
                                dateFrom,
                                dateTo,
                                logDate,
                                transactionItems: iDTO.InputItems,
                                isMySelf: iDTO.IsMySelf,
                                attestStateFromId: null,
                                attestStateToId: iDTO.AttestStateId,
                                noOfValidTransactions: ref noOfValidTransactions,
                                noOfInvalidTransactions: ref noOfInvalidTransactions
                                );
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                    {
                        oDTO.Result.IntegerValue = noOfValidTransactions;
                        oDTO.Result.IntegerValue2 = noOfInvalidTransactions;
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }

                return oDTO;
            }
        }

        /// <summary>
        /// Update TimePayrollTransactions from AccountProvisions
        /// </summary>
        /// <returns></returns>
        private SaveAttestForAccountProvisionOutputDTO TaskSaveAttestForAccountProvision()
        {
            var (iDTO, oDTO) = InitTask<SaveAttestForAccountProvisionInputDTO, SaveAttestForAccountProvisionOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null || iDTO.InputTransactions == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        DateTime logDate = DateTime.Now;

                        var inputTimePayrollTransactions = iDTO.InputTransactions;
                        var timePayrollTransctions = GetTimePayrollTransactionsForCompanyAndAccountProvisionWithTimeCodeAndAccountInternal(inputTimePayrollTransactions.Select(tr => tr.TimePayrollTransactionId).ToList());
                        if (timePayrollTransctions.IsNullOrEmpty())
                            return oDTO;

                        var userValidTransitions = new List<AttestUserRoleView>();
                        foreach (var attestStateId in inputTimePayrollTransactions.Select(p => p.AttestStateId).Distinct())
                        {
                            var userValidTransition = AttestManager.GetAttestUserRoleViews(entities, base.UserId, actorCompanyId, attestStateId);
                            if (userValidTransition != null)
                                userValidTransitions.AddRange(userValidTransition);
                        }
                        if (!userValidTransitions.Any())
                            return oDTO;

                        List<AttestTransitionDTO> attestTransitions = AttestManager.GetAttestTransitions(entities, TermGroup_AttestEntity.PayrollTime, SoeModule.Time, false, actorCompanyId, false).ToDTOs(false).ToList();

                        foreach (TimePayrollTransaction timePayrollTransction in timePayrollTransctions)
                        {
                            var inputTimePayrollTransaction = inputTimePayrollTransactions.FirstOrDefault(a => a.TimePayrollTransactionId == timePayrollTransction.TimePayrollTransactionId);
                            if (inputTimePayrollTransaction == null || timePayrollTransction.AttestStateId == inputTimePayrollTransaction.AttestStateId)
                                continue;

                            AttestTransitionDTO attestTransition = attestTransitions.FirstOrDefault(i => i.AttestStateFromId == timePayrollTransction.AttestStateId && i.AttestStateToId == inputTimePayrollTransaction.AttestStateId);
                            if (attestTransition == null)
                                continue;
                            if (!userValidTransitions.HasValidTransition(attestTransition, inputTimePayrollTransaction.Date))
                                continue;

                            if (timePayrollTransction.AttestStateId != attestTransition.AttestStateToId && !timePayrollTransction.IsReversed)
                                TryUpdateTimePayrollTransactionAttestState(entities, actorCompanyId, timePayrollTransction, attestTransition, logDate);
                        }

                        TrySaveAndCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        /// <summary>
        /// Run auto-attest for a collection of Employees
        /// </summary>
        /// <returns>Output DTO</returns>
        private RunAutoAttestOutputDTO TaskRunAutoAttest()
        {
            var (iDTO, oDTO) = InitTask<RunAutoAttestInputDTO, RunAutoAttestOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            int nrOfEmployeesUpdated = 0;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        if (iDTO.EmployeeIds.IsNullOrEmpty())
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(3516, "Inga anställda att köra automatattest på"));
                            return oDTO;
                        }

                        List<AttestRuleHead> attestRuleHeads = GetAttestRuleHeadsWithRowsAndEmployeeGroup(SoeModule.Time, iDTO.ScheduleJobHeadIds);
                        if (attestRuleHeads.IsNullOrEmpty())
                        {
                            oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(11864, "Inga regler för automatattest hittades"));
                            return oDTO;
                        }

                        int sourceAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.TimeAutoAttestSourceAttestStateId);
                        AttestStateDTO sourceAttestState = GetAttestStateWithTransitions(sourceAttestStateId);
                        if (sourceAttestState == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(3514, "Attestnivå ej satt eller ej funnen (AttestStateId = {0})"), sourceAttestStateId));
                            return oDTO;
                        }

                        int sourceAttestStateId2 = GetCompanyIntSettingFromCache(CompanySettingType.TimeAutoAttestSourceAttestStateId2);
                        AttestStateDTO sourceAttestState2 = GetAttestStateWithTransitions(sourceAttestStateId2);
                        if (sourceAttestState2 == null)
                            sourceAttestStateId2 = 0;

                        int targetAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.TimeAutoAttestTargetAttestStateId);
                        AttestStateDTO targetAttestState = GetAttestStateWithTransitions(targetAttestStateId);
                        if (targetAttestState == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(3515, "Attestnivå för lyckad körning ej satt eller ej funnen (AttestStateId = {0})"), targetAttestStateId));
                            return oDTO;
                        }

                        AttestTransitionDTO attestTransition = GetUserAttestTransitionForState(TermGroup_AttestEntity.PayrollTime, sourceAttestStateId, targetAttestStateId);
                        if (attestTransition == null)
                        {
                            oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8160, "Giltig attestövergång för löneartstransaktioner kunde inte hittas"));
                            return oDTO;
                        }

                        List<Employee> employees = GetEmployeesWithEmployment(iDTO.EmployeeIds, excludeEmployeeId: GetCurrentEmployee()?.EmployeeId);
                        if (!employees.IsNullOrEmpty() && !iDTO.ScheduleJobHeadIds.IsNullOrEmpty())
                        {
                            List<Employee> filteredList = new List<Employee>();
                            foreach (AttestRuleHead attestRuleHead in attestRuleHeads.Where(i => i.EmployeeGroup == null))
                            {
                                List<int> employeeGroupIds = attestRuleHead.EmployeeGroup.Select(s => s.EmployeeGroupId).Distinct().ToList();
                                foreach (Employee employee in employees.Where(w => !filteredList.Contains(w)))
                                {
                                    if (employeeGroupIds.Contains(employee.GetEmployeeGroupId()))
                                        filteredList.Add(employee);
                                }
                            }
                            employees = filteredList.Distinct().ToList();
                        }
                        if (employees.IsNullOrEmpty())
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(3516, "Inga anställda att köra automatattest på"));
                            return oDTO;
                        }

                        bool autoAttestEmployeeManuallyAdjustedTimeStamps = GetCompanyBoolSettingFromCache(CompanySettingType.TimeAutoAttestEmployeeManuallyAdjustedTimeStamps);
                        bool loadTimeCodeTranscations = attestRuleHeads.ContainsAnyRowWithType(TermGroup_AttestRuleRowLeftValueType.TimeCode, TermGroup_AttestRuleRowRightValueType.TimeCode);
                        bool useAccountHierarchy = UseAccountHierarchy();
                        int accountId = 0;
                        if(!iDTO.AutoAttestJob)
                            accountId = AccountManager.GetAccountHierarchySettingAccountId(entities, useAccountHierarchy);

                        List<AccountDTO> accountInternals = accountId > 0 ? base.GetAccountInternalsFromCache(entities, CacheConfig.Company(base.ActorCompanyId)) : null;

                        #endregion

                        #region Perform

                        foreach (Employee employee in employees)
                        {
                            #region Validate

                            var scheduleBlocksByDate = GetScheduleBlocksForEmployee(null, employee.EmployeeId, iDTO.StartDate, iDTO.StopDate).GroupBy(i => i.Date.Value).ToDictionary(k => k.Key, v => v.ToList());
                            var timeBlocksByDate = GetTimeBlocksWithTimeBlockDate(employee.EmployeeId, iDTO.StartDate, iDTO.StopDate).GroupBy(i => i.TimeBlockDate.Date).ToDictionary(k => k.Key, v => v.ToList());
                            var timeBlockDates = GetTimeBlockDates(employee.EmployeeId, iDTO.StartDate, iDTO.StopDate);
                            
                            List<TimeBlockDate> validTimeBlockDates = GetValidDaysForAutoAttest(timeBlockDates, employee, scheduleBlocksByDate, timeBlocksByDate, targetAttestState);
                            if (validTimeBlockDates.IsNullOrEmpty())
                                continue;

                            var timePayrollTransactionsByDay = TimeTransactionManager.GetTimePayrollTransactionItemsForEmployee(entities, employee.EmployeeId, validTimeBlockDates).GroupBy(i => i.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());

                            if (useAccountHierarchy && accountId > 0)
                            {
                                validTimeBlockDates = FilterValidDaysForAutoAttestByLended(entities, validTimeBlockDates, employee, scheduleBlocksByDate, timePayrollTransactionsByDay, accountId, accountInternals);
                                if (!validTimeBlockDates.Any())
                                    continue;
                            }

                            var validTimeBlockDateIds = validTimeBlockDates.Select(i => i.TimeBlockDateId).ToList();
                            var timeCodeTransactionsByDay = loadTimeCodeTranscations ? GetTimeCodeTransactions(validTimeBlockDateIds).GroupBy(i => i.TimeBlockDateId.Value).ToDictionary(k => k.Key, v => v.ToList()) : new Dictionary<int, List<TimeCodeTransaction>>();
                            var timeStampEntrysSummaryItems = validTimeBlockDates.Any(i => i.CalculatedAutogenTimeblocks == true) ? TimeStampManager.GetTimeStampEntrysEmployeeSummary(entities, validTimeBlockDates, employee.EmployeeId) : new List<GetTimeStampEntrysEmployeeSummaryResult>();

                            #endregion

                            #region Analyze

                            DateTime logDate = DateTime.Now;
                            List<int> passedTimeBlockDateIds = new List<int>();

                            foreach (TimeBlockDate timeBlockDate in validTimeBlockDates)
                            {
                                var timePayrollTransactionsForDay = timePayrollTransactionsByDay.ContainsKey(timeBlockDate.TimeBlockDateId) ? timePayrollTransactionsByDay[timeBlockDate.TimeBlockDateId] : new List<GetTimePayrollTransactionsForEmployee_Result>();

                                //Never attest manuallt adjusted timestamps (if company has that setting)
                                if (timeBlockDate.CalculatedAutogenTimeblocks == false && !timePayrollTransactionsForDay.IsNullOrEmpty() && !autoAttestEmployeeManuallyAdjustedTimeStamps && timeStampEntrysSummaryItems.HasDateEmployeeManuallyAdjustedTimeStamps(timeBlockDate.TimeBlockDateId))
                                    continue;

                                List<TimeScheduleTemplateBlock> scheduleBlocksForDay = scheduleBlocksByDate.GetList(timeBlockDate.Date);
                                List<TimeBlock> timeBlocksForDay = timeBlocksByDate.GetList(timeBlockDate.Date);
                                List<TimeCodeTransaction> timeCodeTransactionsForDay = timeCodeTransactionsByDay.GetList(timeBlockDate.TimeBlockDateId);

                                int scheduleMinutes = scheduleBlocksForDay.GetWorkMinutes();
                                int scheduleBreakMinutes = scheduleBlocksForDay.GetBreakMinutes();
                                int presenceTime = timeBlocksForDay.GetWorkMinutes(true);
                                int presenceBreakMinutes = timeBlocksForDay.GetBreakMinutes();
                                int workedTime = timePayrollTransactionsForDay.Where(i => i.IsWork()).SumQuantity();
                                int presenceWorkOutsideScheduleTime = timePayrollTransactionsForDay.Where(i => i.NoOfPresenceWorkOutsideScheduleTime > 0).SumQuantity();
                                int workedInsideScheduledTime = workedTime - presenceWorkOutsideScheduleTime;

                                bool passed = DoDayPassAutoAttestRules(timeBlockDate, employee, timePayrollTransactionsForDay, timeCodeTransactionsForDay, attestRuleHeads, sourceAttestStateId, sourceAttestStateId2, targetAttestStateId, scheduleMinutes, scheduleBreakMinutes, presenceTime, presenceBreakMinutes, workedInsideScheduledTime);
                                if (passed)
                                    passedTimeBlockDateIds.Add(timeBlockDate.TimeBlockDateId);
                            }

                            if (!passedTimeBlockDateIds.Any())
                                continue;

                            #endregion

                            #region Update

                            bool isAnyTransactionUpdate = false;

                            List<TimePayrollTransaction> passedTimePayrollTransactions = (from tpt in entities.TimePayrollTransaction
                                                                                            .Include("TimeCodeTransaction")
                                                                                          where tpt.EmployeeId == employee.EmployeeId &&
                                                                                          passedTimeBlockDateIds.Contains(tpt.TimeBlockDateId) &&
                                                                                          tpt.State == (int)SoeEntityState.Active
                                                                                          select tpt).ToList();

                            passedTimePayrollTransactions = passedTimePayrollTransactions.Where(t => t.AttestStateId != targetAttestStateId && !t.IsReversed).ToList();

                            foreach (var passedTimePayrollTransactionsByDay in passedTimePayrollTransactions.GroupBy(i => i.TimeBlockDateId))
                            {
                                foreach (TimePayrollTransaction timePayrollTransaction in passedTimePayrollTransactionsByDay)
                                {
                                    if (!timePayrollTransaction.TimeBlockId.HasValue || timePayrollTransaction.IsAdditionOrDeduction)
                                        continue;

                                    TryUpdateTimePayrollTransactionAttestState(entities, actorCompanyId, timePayrollTransaction, attestTransition, logDate);
                                    isAnyTransactionUpdate = true;
                                }
                            }

                            if (isAnyTransactionUpdate)
                            {
                                oDTO.Result = Save();
                                nrOfEmployeesUpdated++;
                            }

                            #endregion
                        }

                        #endregion

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                        oDTO.Result.IntegerValue = nrOfEmployeesUpdated;
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Send attest reminders to executives and employees
        /// </summary>
        /// <returns>Output DTO</returns>
        private SendAttestReminderOutputDTO TaskSendAttestReminder()
        {
            var (iDTO, oDTO) = InitTask<SendAttestReminderInputDTO, SendAttestReminderOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.EmployeeIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8743, "Inga anställda hittades"));
                return oDTO;
            }
            if (!iDTO.DoSendToExecutive && !iDTO.DoSendToEmployee)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(12058, "Välj vilka som ska få påminnelse"));
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                if (!UseAccountHierarchy())
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.NotSupported, GetText(12056, "Funktionen stöds endast för ekonomisk struktur"));
                    return oDTO;
                }

                List<AttestState> attestStatesForCompany = GetAttestStates();
                Dictionary<int, List<TimePayrollTransactionTreeDTO>> transactionsByEmployee = GetTimePayrollTransactionsForTreeByEmployee(iDTO.EmployeeIds, iDTO.StartDate, iDTO.StopDate, includeAccounting: true);
                Dictionary<int, List<EmployeeAccount>> employeeAccountsByEmployee = GetEmployeeAccountsByEmployee(iDTO.EmployeeIds, iDTO.StartDate, iDTO.StopDate, onlyDefault: true);
                List<EmployeeGroup> employeeGroups = iDTO.DoSendToEmployee ? GetEmployeeGroupsFromCache() : null;
                List<AttestRole> attestRoles = iDTO.DoSendToExecutive ? GetAttestRolesWitUser() : null;
                List<AttestRoleUser> attestRoleUsers = attestRoles?.SelectMany(ar => ar.AttestRoleUser.Where(aru => aru.State == (int)SoeEntityState.Active && aru.AccountId.HasValue && aru.IsExecutive)).ToList();
                var useEmployeeAccountsOnly = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.SendReminderToExecutivesBasedOnEmployeeAccountOnly, userId, actorCompanyId, 0);

                #endregion

                #region Flow

                List<(Employee Employee, AttestState AttestStateReminder, EmployeeGroup EmployeeGroup)> employeeReminders = new List<(Employee, AttestState, EmployeeGroup)>();
                List<(Employee Employee, AttestState AttestStateReminder, AttestRoleUser AttestRoleUser)> executiveReminders = new List<(Employee, AttestState, AttestRoleUser)>();

                CreateReminders();
                TrySendReminderToEmployee();
                TrySendReminderToExecutives();

                #endregion

                #region Workers

                void CreateReminders()
                {
                    foreach (int employeeId in iDTO.EmployeeIds)
                    {
                        if (!transactionsByEmployee.ContainsKey(employeeId))
                            continue;

                        List<EmployeeAccount> employeeAccounts = employeeAccountsByEmployee.ContainsKey(employeeId) ? employeeAccountsByEmployee[employeeId] : null;
                        if (employeeAccounts.IsNullOrEmpty())
                            continue;

                        Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
                        if (employee == null)
                            continue;

                        foreach (var transactionsByEmployeeAndDate in transactionsByEmployee[employeeId].GroupBy(i => i.Date))
                        {
                            DateTime date = transactionsByEmployeeAndDate.Key;
                            List<TimePayrollTransactionTreeDTO> transactions = transactionsByEmployeeAndDate.ToList();
                            List<AttestState> attestStates = GetAttestStatesForEmployeeAndDay(transactions);

                            if (iDTO.DoSendToEmployee)
                                TryAddReminderToEmployee(employee, date, attestStates);

                            if (iDTO.DoSendToExecutive)
                                TryAddReminderToExecutive(employee, date, attestStates, employeeAccounts, transactions, useEmployeeAccountsOnly);
                        }
                    }
                }

                List<AttestState> GetAttestStatesForEmployeeAndDay(List<TimePayrollTransactionTreeDTO> transactions)
                {
                    List<int> attestStateIds = transactions.Select(transaction => transaction.AttestStateId).Distinct().ToList();
                    return attestStatesForCompany?.Where(attestState => attestStateIds.Contains(attestState.AttestStateId)).ToList() ?? new List<AttestState>();
                }

                void TryAddReminderToEmployee(Employee employee, DateTime date, List<AttestState> attestStates)
                {
                    if (employee == null || attestStates.IsNullOrEmpty() || employeeReminders.Any(i => i.Employee.EmployeeId == employee.EmployeeId))
                        return;

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups: employeeGroups);
                    if (employeeGroup == null)
                        return;

                    AttestState attestStateReminder = attestStatesForCompany.FirstOrDefault(i => i.AttestStateId == employeeGroup.ReminderAttestStateId);
                    if (attestStateReminder == null || !attestStates.DoRemind(attestStateReminder))
                        return;

                    employeeReminders.Add((employee, attestStateReminder, employeeGroup));
                }

                void TryAddReminderToExecutive(Employee employee, DateTime date, List<AttestState> attestStates, List<EmployeeAccount> employeeAccounts, List<TimePayrollTransactionTreeDTO> transactions, bool useEmployeeAccountsOnly)
                {
                    if (employee == null || attestStates.IsNullOrEmpty() || employeeAccounts.IsNullOrEmpty() || transactions.IsNullOrEmpty())
                        return;

                    List<int> transactionAccountIdsForday = transactions.SelectMany(i => i.AccountInternalIds).Distinct().ToList();
                    List<int> accountIdsForDay = employeeAccounts.GetEmployeeAccounts(date, accountIds: !useEmployeeAccountsOnly ? transactionAccountIdsForday : null).Select(i => i.AccountId.Value).Distinct().ToList();
                    List<AttestRoleUser> attestRoleUsersForAccounts = attestRoleUsers.Filter(iDTO.StartDate, iDTO.StopDate, accountIds: accountIdsForDay);

                    foreach (AttestRoleUser attestRoleUser in attestRoleUsersForAccounts)
                    {
                        if (executiveReminders.Any(i => i.Employee.EmployeeId == employee.EmployeeId && i.AttestRoleUser.UserId == attestRoleUser.UserId))
                            continue;

                        AttestRole attestRole = attestRoles.FirstOrDefault(i => i.AttestRoleId == attestRoleUser.AttestRoleId);
                        if (attestRole == null || attestRole.ReminderAttestStateId.IsNullOrEmpty() || attestRole.ReminderPeriodType.IsNullOrEmpty())
                            continue;

                        AttestState attestStateReminder = attestStatesForCompany.FirstOrDefault(i => i.AttestStateId == attestRole.ReminderAttestStateId.Value);
                        if (attestStateReminder == null || !attestStates.DoRemind(attestStateReminder))
                            continue;

                        executiveReminders.Add((employee, attestStateReminder, attestRoleUser));
                    }
                }

                void TrySendReminderToEmployee()
                {
                    if (!employeeReminders.Any())
                        return;

                    foreach (var employeeReminder in employeeReminders)
                    {
                        SendXEMailToEmployeeToRemindToAttest(employeeReminder.Employee, employeeReminder.EmployeeGroup.ReminderPeriodType.Value, employeeReminder.AttestStateReminder, iDTO.StopDate, null);
                    }
                }

                void TrySendReminderToExecutives()
                {
                    if (!executiveReminders.Any())
                        return;

                    foreach (var executiveReminderByUser in executiveReminders.GroupBy(i => i.AttestRoleUser.UserId))
                    {
                        User user = GetUser(executiveReminderByUser.Key);
                        if (user == null)
                            continue;

                        SendXEMailToExecutiveToRemindToAttest(user, executiveReminderByUser.Where(i => i.Employee.UserId != user.UserId).Select(i => i.Employee.EmployeeNrAndName).Distinct().ToList());
                    }
                }

                #endregion
            }

            return oDTO;
        }

        #endregion

        #region AttestRole

        private List<AttestRole> GetAttestRolesWitUser()
        {
            int moduleId = (int)SoeModule.Time;

            return (from ar in entities.AttestRole
                        .Include("AttestRoleUser")
                    where ar.ActorCompanyId == actorCompanyId &&
                    ar.Module == moduleId &&
                    ar.State == (int)SoeEntityState.Active
                    orderby ar.Name
                    select ar).ToList();
        }

        private List<AttestUserRoleView> GetAttestUserRoleViews(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                return (from v in entities.AttestUserRoleView
                        where v.UserId == userId &&
                        v.DateFrom <= dateFrom.Value &&
                        v.DateTo >= dateTo.Value
                        select v).ToList();
            }
            else
            {
                return (from v in entities.AttestUserRoleView
                        where v.UserId == userId
                        select v).ToList();
            }
        }

        #endregion

        #region AttestState

        private List<AttestState> GetAttestStates()
        {
            int companyId = base.ActorCompanyId;

            return (from a in entities.AttestState
                    where a.ActorCompanyId == companyId &&
                    a.State == (int)SoeEntityState.Active
                    select a).ToList();
        }

        private AttestStateDTO GetAttestState(int attestStateId)
        {
            return (from a in entities.AttestState
                    where a.AttestStateId == attestStateId &&
                    a.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault().ToDTO();
        }

        private AttestStateDTO GetAttestStateWithTransitions(int attestStateId)
        {
            return (from a in entities.AttestState
                        .Include("AttestTransitionFrom")
                        .Include("AttestTransitionTo")
                    where a.AttestStateId == attestStateId &&
                    a.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault().ToDTO();
        }

        private AttestStateDTO GetAttestStateInitial(TermGroup_AttestEntity entity)
        {
            int entityId = (int)entity;

            return (from a in entities.AttestState
                    where a.ActorCompanyId == actorCompanyId &&
                    a.Entity == entityId &&
                    a.Initial
                    select a).FirstOrDefault().ToDTO();
        }

        #endregion

        #region AttestTransition

        private List<AttestTransitionDTO> GetAttestTransitionsToState(int attestStateId)
        {
            return (from a in base.GetAttestTransitionsFromCache(entities, CacheConfig.Company(actorCompanyId))
                    where a.AttestStateToId == attestStateId
                    select a).OrderByDescending(a => a.AttestStateTo.Initial).ThenBy(a => a.AttestStateTo.Sort).ToList();
        }

        private List<AttestTransitionDTO> GetAttestTransitionsForEmployee(int employeeId, TermGroup_AttestEntity entity)
        {
            List<AttestTransitionDTO> attestTransitions = new List<AttestTransitionDTO>();

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return attestTransitions;

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(null, employeeGroups: GetEmployeeGroupsFromCache());
            if (employeeGroup == null)
                return attestTransitions;

            return GetAttestTransitionsForEmployeeGroupFromCache(employeeGroup.EmployeeGroupId, entity);
        }

        private List<AttestTransitionDTO> GetUserAttestTransitions(TermGroup_AttestEntity entity, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            List<AttestTransition> attestTransitions = new List<AttestTransition>();

            var userRolesQuery = from v in entities.AttestUserRoleView
                                 where ((v.ActorCompanyId == actorCompanyId) &&
                                 (v.UserId == userId) &&
                                 (!dateFrom.HasValue || v.DateFrom >= dateFrom.Value) &&
                                 (!dateTo.HasValue || v.DateTo <= dateTo.Value))
                                 select v;

            if (userRolesQuery == null)
                return new List<AttestTransitionDTO>();

            List<AttestTransition> companyAttestTransitions = (from a in entities.AttestTransition
                                                               join r in userRolesQuery on a.AttestTransitionId equals r.AttestTransitionId
                                                               where a.Company.ActorCompanyId == actorCompanyId
                                                               select a).ToList();

            int entityId = (int)entity;
            foreach (AttestTransition attestTransition in companyAttestTransitions)
            {
                if (!attestTransition.AttestStateFromReference.IsLoaded)
                    attestTransition.AttestStateFromReference.Load();

                if (entityId != (int)TermGroup_AttestEntity.Unknown && entityId != attestTransition.AttestStateFrom.Entity)
                    continue;

                if (!attestTransitions.Any(i => i.AttestTransitionId == attestTransition.AttestTransitionId))
                    attestTransitions.Add(attestTransition);
            }

            return attestTransitions.ToDTOs(true).ToList();
        }

        private AttestTransitionDTO GetUserAttestTransitionForState(TermGroup_AttestEntity entity, int attestStateFromId, int attestStateToId)
        {
            List<AttestTransitionDTO> attestTransitions = GetUserAttestTransitions(entity);

            return (from at in attestTransitions
                    where at.AttestStateFromId == attestStateFromId &&
                    at.AttestStateToId == attestStateToId
                    select at).FirstOrDefault();
        }

        private AttestTransitionDTO GetAttestTransition(int attestStateFromId, int attestStateToId)
        {
            return (from a in GetAttestTransitionsFromCache()
                    where a.AttestStateFromId == attestStateFromId &&
                    a.AttestStateToId == attestStateToId
                    select a).OrderByDescending(a => a.AttestStateTo.Initial).ThenBy(a => a.AttestStateTo.Sort).FirstOrDefault();
        }

        private void CreateTimePayrollTransactionAttestTransitionLog(TimePayrollTransaction timePayrollTransaction, int newAttestStateId)
        {
            if (timePayrollTransaction.AttestStateId == 0 || timePayrollTransaction.AttestStateId != newAttestStateId)
            {
                int previousAttestStateId = timePayrollTransaction.AttestStateId;
                if (newAttestStateId > 0)
                {
                    AttestTransitionDTO attestTransition = GetUserAttestTransitionForState(TermGroup_AttestEntity.PayrollTime, previousAttestStateId, newAttestStateId);
                    if (attestTransition == null)
                        timePayrollTransaction.AttestStateId = previousAttestStateId;
                    else
                        TryUpdateTimePayrollTransactionAttestState(entities, actorCompanyId, timePayrollTransaction, attestTransition);
                }
            }
        }

        private void AddAttestTransitionLog(CompEntities entities, int actorCompanyId, int recordId, int transitionId, TermGroup_AttestEntity entity, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            AttestTransitionLog attestTransitionLog = new AttestTransitionLog()
            {
                UserId = parameterObject != null && parameterObject.SupportUserId.HasValue ? parameterObject.SupportUserId.Value : userId,
                RecordId = recordId,
                Date = date.Value,
                Entity = (int)entity,

                //Set FK
                ActorCompanyId = actorCompanyId,
                AttestTransitionId = transitionId,
            };
            entities.AttestTransitionLog.AddObject(attestTransitionLog);
        }

        #endregion

        #region AttestTransitionLogs

        private List<AttestTransitionLog> GetAttestTransitionLogs(List<int> timePayrollTransactionIds)
        {
            return (from atl in entities.AttestTransitionLog
                        .Include("AttestTransition")
                    where atl.Entity == (int)TermGroup_AttestEntity.PayrollTime &&
                    timePayrollTransactionIds.Contains(atl.RecordId)
                    select atl).ToList();
        }

        #endregion

        #region Auto-attest

        private List<AttestRuleHead> GetAttestRuleHeadsWithRowsAndEmployeeGroup(SoeModule module, List<int> scheduleJobHeadIds = null)
        {
            int moduleId = (int)module;

            List<AttestRuleHead> heads = (from a in entities.AttestRuleHead
                                           .Include("EmployeeGroup")
                                           .Include("AttestRuleRow")
                                          where a.ActorCompanyId == actorCompanyId &&
                                          a.Module == moduleId &&
                                          a.State == (int)SoeEntityState.Active
                                          orderby a.Name
                                          select a).ToList();

            if (scheduleJobHeadIds.IsNullOrEmpty())
                return heads.Where(w => !w.ScheduledJobHeadId.HasValue).ToList();
            else
                return heads.Where(w => w.ScheduledJobHeadId.HasValue && scheduleJobHeadIds.Contains(w.ScheduledJobHeadId.Value)).ToList();
        }

        private List<TimeBlockDate> GetValidDaysForAutoAttest(List<TimeBlockDate> timeBlockDates, Employee employee, Dictionary<DateTime, List<TimeScheduleTemplateBlock>> scheduleBlocksByDate, Dictionary<DateTime, List<TimeBlock>> timeBlocksByDate, AttestStateDTO targetAttestState)
        {
            if (timeBlockDates.IsNullOrEmpty())
                return new List<TimeBlockDate>();

            List<TimeBlockDate> validTimeBlockDates = new List<TimeBlockDate>();
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

            DateTime currentDate = timeBlockDates.Select(i => i.Date).Min();
            DateTime stopDate = timeBlockDates.Select(i => i.Date).Max();
            while (currentDate <= stopDate)
            {
                try
                {
                    TimeBlockDate timeBlockDate = timeBlockDates.FirstOrDefault(i => i.Date == currentDate);
                    if (timeBlockDate == null)
                        continue;

                    // Never attest days that are preliminary
                    List<TimeScheduleTemplateBlock> scheduleBlocksForDate = scheduleBlocksByDate.GetList(currentDate);
                    if (scheduleBlocksForDate.Any(i => i.IsPreliminary))
                        continue;

                    // Never attest days with duplicate TimeBlocks
                    List<TimeBlock> timeBlocksForDate = timeBlocksByDate.GetList(currentDate);
                    if (!targetAttestState.Initial && timeBlocksForDate.ContainsDuplicateTimeBlocks())
                        continue;

                    // Never attest days with invalid TimeStamps
                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(currentDate, employeeGroups: employeeGroups);
                    if (!IsTimeStampStatusValid(employeeGroup, timeBlockDate.StampingStatus, targetAttestState))
                        continue;

                    //Valid
                    if (employeeGroup != null)
                        timeBlockDate.CalculatedAutogenTimeblocks = employeeGroup.AutogenTimeblocks;
                    validTimeBlockDates.Add(timeBlockDate);
                }
                finally
                {
                    currentDate = currentDate.AddDays(1);
                }
            }

            return validTimeBlockDates;
        }

        private List<TimeBlockDate> FilterValidDaysForAutoAttestByLended(CompEntities entities, List<TimeBlockDate> timeBlockDates, Employee employee, Dictionary<DateTime, List<TimeScheduleTemplateBlock>> scheduleBlocksByDate, Dictionary<int, List<GetTimePayrollTransactionsForEmployee_Result>> timePayrollTransactionsByDate, int accountId, List<AccountDTO> accountInternals)
        {
            if (timeBlockDates.IsNullOrEmpty() || employee == null || accountId == 0)
                return new List<TimeBlockDate>();

            List<DateTime> validDates = timeBlockDates.Select(d => d.Date).ToList();
            List<int> timeBlockDateIds = timeBlockDates.Select(d => d.TimeBlockDateId).ToList();
            List<TimeScheduleTemplateBlock> validScheduleBlocks = scheduleBlocksByDate.GetList(validDates);
            List<GetTimePayrollTransactionsForEmployee_Result> validTimePayrollTransactions = timePayrollTransactionsByDate.GetList(timeBlockDateIds);
            Dictionary<DateTime, bool> validDatesByAccounts = AccountManager.GetValidDatesOnGivenAccounts(entities, employee.EmployeeId, validDates, accountId, accountInternals, validScheduleBlocks, validTimePayrollTransactions);
            validDates = validDatesByAccounts.Where(p => p.Value).Select(k => k.Key).ToList(); //Only accept not lended, partly or completely lended should be excluded

            return timeBlockDates.Where(tbd => validDates.Contains(tbd.Date)).ToList();
        }

        private bool DoDayPassAutoAttestRules(TimeBlockDate timeBlockDate, Employee employee, List<GetTimePayrollTransactionsForEmployee_Result> transactionItemsForDay, List<TimeCodeTransaction> timeCodeTransactionsForDay, List<AttestRuleHead> attestRuleHeads,
            int sourceAttestStateId, int sourceAttestStateId2, int targetAttestStateId,
            int scheduleMinutes, int scheduleBreakMinutes, int presenceTime, int presenceBreakMinutes, int workedInsideScheduledTime)
        {
            if (employee == null || timeBlockDate == null || transactionItemsForDay.IsNullOrEmpty() || attestRuleHeads.IsNullOrEmpty())
                return false;

            DayType dayType = null;

            foreach (var transactionItem in transactionItemsForDay)
            {
                if (transactionItem.AttestStateId == targetAttestStateId)
                    continue;//skip transaction

                if (transactionItem.AttestStateId != sourceAttestStateId && transactionItem.AttestStateId != sourceAttestStateId2)
                    return false; //skip whole day

                foreach (AttestRuleHead attestRuleHead in attestRuleHeads)
                {
                    #region EmployeeGroup condition

                    int employeeGroupId = employee.GetEmployeeGroupId(transactionItem.Date);
                    if (attestRuleHead.EmployeeGroup.Any() && !attestRuleHead.EmployeeGroup.Any(e => e.EmployeeGroupId == employeeGroupId))
                        continue;

                    #endregion

                    #region DayType condition

                    // Check if rule has a day type condition
                    if (attestRuleHead.DayTypeId.HasValue)
                    {
                        // Only fetch day type once per employee and date
                        if (transactionItem.StartTime.HasValue)
                        {
                            if (dayType == null)
                                dayType = GetDayTypeForEmployeeFromCache(employee.EmployeeId, transactionItem.StartTime.Value);
                        }
                        else
                            dayType = null;

                        // If day type on transaction does not match the rule, skip it
                        if (dayType?.DayTypeId != attestRuleHead.DayTypeId)
                            continue;
                    }

                    #endregion

                    #region AttestRuleRows

                    // Loop throught rule rows
                    foreach (AttestRuleRow row in attestRuleHead.AttestRuleRow.Where(r => r.State == (int)SoeEntityState.Active))
                    {
                        int leftValue = 0;
                        int rightValue = 0;
                        bool leftValueSet = false;
                        bool rightValueSet = false;

                        #region Left value

                        switch ((TermGroup_AttestRuleRowLeftValueType)row.LeftValueType)
                        {
                            // Ingen vald
                            case TermGroup_AttestRuleRowLeftValueType.None:
                                // Skip row
                                continue;
                            // Närvarotid
                            case TermGroup_AttestRuleRowLeftValueType.PresenceTime:
                                leftValue = presenceTime;
                                leftValueSet = true;
                                break;
                            // Schematid
                            case TermGroup_AttestRuleRowLeftValueType.ScheduledTime:
                                leftValue = scheduleMinutes;
                                leftValueSet = true;
                                break;
                            // Närvaro inom schema
                            case TermGroup_AttestRuleRowLeftValueType.WorkedInsideScheduledTime:
                                leftValue = workedInsideScheduledTime;
                                leftValueSet = true;
                                break;
                            // Schemalagd rast
                            case TermGroup_AttestRuleRowLeftValueType.ScheduledBreakTime:
                                leftValue = scheduleBreakMinutes;
                                leftValueSet = true;
                                break;
                            // Total rast
                            case TermGroup_AttestRuleRowLeftValueType.TotalBreakTime:
                                leftValue = presenceBreakMinutes;
                                leftValueSet = true;
                                break;
                            // Tidkod
                            case TermGroup_AttestRuleRowLeftValueType.TimeCode:
                                var timeCodeTransactionsForTimeCode = timeCodeTransactionsForDay?.Where(i => i.TimeCodeId == row.LeftValueId).ToList();
                                if (timeCodeTransactionsForTimeCode.IsNullOrEmpty())
                                    continue;

                                leftValue = timeCodeTransactionsForTimeCode.SumQuantity();
                                leftValueSet = true;
                                break;
                            // Löneart
                            case TermGroup_AttestRuleRowLeftValueType.PayrollProduct:
                                var transactionItemsForProduct = transactionItemsForDay?.Where(i => i.ProductId == row.LeftValueId).ToList();
                                if (transactionItemsForProduct.IsNullOrEmpty())
                                    continue;

                                leftValue = transactionItemsForProduct.SumQuantity();
                                leftValueSet = true;
                                break;
                            // Artikel
                            case TermGroup_AttestRuleRowLeftValueType.InvoiceProduct:
                                // Not implemented yet
                                continue;
                            default:
                                continue;
                        }

                        #endregion

                        #region Right value

                        switch ((TermGroup_AttestRuleRowRightValueType)row.RightValueType)
                        {
                            // Ingen vald
                            case TermGroup_AttestRuleRowRightValueType.None:
                                // No value selected, just use minutes added below
                                rightValue = 0;
                                rightValueSet = true;
                                break;
                            // Närvarotid
                            case TermGroup_AttestRuleRowRightValueType.PresenceTime:
                                rightValue = presenceTime;
                                rightValueSet = true;
                                break;
                            // Schematid
                            case TermGroup_AttestRuleRowRightValueType.ScheduledTime:
                                rightValue = scheduleMinutes;
                                rightValueSet = true;
                                break;
                            // Närvaro inom schema
                            case TermGroup_AttestRuleRowRightValueType.WorkedInsideScheduledTime:
                                rightValue = workedInsideScheduledTime;
                                rightValueSet = true;
                                break;
                            // Schemalagd rast
                            case TermGroup_AttestRuleRowRightValueType.ScheduledBreakTime:
                                rightValue = scheduleBreakMinutes;
                                rightValueSet = true;
                                break;
                            // Total rast
                            case TermGroup_AttestRuleRowRightValueType.TotalBreakTime:
                                rightValue = presenceBreakMinutes;
                                rightValueSet = true;
                                break;
                            // Tidkod
                            case TermGroup_AttestRuleRowRightValueType.TimeCode:
                                rightValue = timeCodeTransactionsForDay.Where(i => i.TimeCodeId == row.RightValueId).SumQuantity();
                                if (rightValue == 0)
                                    continue;
                                rightValueSet = true;
                                break;
                            // Löneart
                            case TermGroup_AttestRuleRowRightValueType.PayrollProduct:
                                rightValue = transactionItemsForDay?.Where(i => i.ProductId == row.RightValueId).SumQuantity() ?? 0;
                                if (rightValue == 0)
                                    continue;
                                rightValueSet = true;
                                break;
                            // Artikel
                            case TermGroup_AttestRuleRowRightValueType.InvoiceProduct:
                                // Not implemented yet
                                continue;
                        }

                        #endregion

                        if (leftValueSet && rightValueSet)
                        {
                            WildCard wildCard = (WildCard)row.ComparisonOperator;
                            bool passed = !wildCard.Compare(leftValue, rightValue + row.Minutes);
                            if (!passed)
                                return false; // No need to continue if one transaction fails
                        }
                    }

                    #endregion
                }
            }

            return true;
        }

        #endregion

        #region Validation

        private void GetValidAttestTransitions(Employee employee, DateTime dateFrom, DateTime dateTo, bool isMySelf, out List<AttestUserRoleView> userValidTransitions, out List<AttestTransitionDTO> employeeGroupTransitions)
        {
            if (isMySelf)
            {
                employeeGroupTransitions = GetAttestTransitionsForEmployee(employee.EmployeeId, TermGroup_AttestEntity.PayrollTime);
                userValidTransitions = new List<AttestUserRoleView>();
            }
            else
            {
                userValidTransitions = GetAttestUserRoleViews(dateFrom, dateTo);
                employeeGroupTransitions = new List<AttestTransitionDTO>();
            }
        }

        private List<TimePayrollTransaction> GetValidTransactionsFromTimeBlocksForAttest(EmployeeAttestResult employeeResult, List<TimeBlock> timeBlocks, Employee employee, AttestStateDTO attestStateTo)
        {
            List<TimePayrollTransaction> validTimePayrollTransactions = new List<TimePayrollTransaction>();

            timeBlocks = timeBlocks.Where(i => i.TimePayrollTransaction != null).ToList();
            List<DateTime> prelTimeBlockDates = timeBlocks.Where(tb => tb.IsPreliminary).GetDates();
            List<DateTime> prelScheduleBlockDates = prelTimeBlockDates.Any() ? GetScheduleBlocksForEmployee(employee.EmployeeId, prelTimeBlockDates.Select(tb => tb.Date)).Where(b => b.IsPreliminary).GetDates() : new List<DateTime>();
            
            foreach (var timeBlocksByDate in timeBlocks.GroupBy(tb => tb.TimeBlockDate))
            {
                TimeBlockDate timeBlockDate = timeBlocksByDate.Key;
                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, employeeGroups: GetEmployeeGroupsFromCache());
                if (employeeGroup == null)
                    continue;

                List<TimePayrollTransaction> timePayrollTransactionsForTimeBlockDate = timeBlocksByDate
                    .SelectMany(tb => tb.TimePayrollTransaction
                        .Where(tpt => tpt.State == (int)SoeEntityState.Active)
                        .ExcludeStartValues())
                    .ToList();

                bool isAnyTimeBlockPrel = timeBlocksByDate.Any(tb => tb.IsPreliminary);
                bool isDayPrel = isAnyTimeBlockPrel && prelScheduleBlockDates.Contains(timeBlockDate.Date);
                bool isStampingInvalid = !IsTimeStampStatusValid(employeeGroup, timeBlockDate.StampingStatus, attestStateTo);
                bool isInvalid = isDayPrel || isStampingInvalid;

                if (isStampingInvalid)
                    employeeResult.AddStampingError(timeBlockDate);

                if (!isInvalid && isAnyTimeBlockPrel)
                {
                    SetTimeBlocksToPreliminary(timeBlocksByDate.Where(b => b.IsPreliminary).ToList(), preliminary: false);
                    SetTimePayrollTransactionsToPreliminary(timePayrollTransactionsForTimeBlockDate.Where(t => t.IsPreliminary).ToList(), preliminary: false);
                }

                if (isInvalid)
                    employeeResult.AddTransactionsFailed(timePayrollTransactionsForTimeBlockDate);
                else
                    validTimePayrollTransactions.AddRange(timePayrollTransactionsForTimeBlockDate);
            }

            return validTimePayrollTransactions;
        }

        private List<TimePayrollTransaction> GetValidTransactionsByAccountHieararchyForAttest(EmployeeAttestResult employeeResult, List<TimePayrollTransaction> timePayrollTransactions, int employeeAccountDimId, List<int> validAccountIdsByHiearchy, List<AccountDTO> accountInternals)
        {
            List<TimePayrollTransaction> validTimePayrollTransactions = new List<TimePayrollTransaction>();

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
            {
                if (HasValidAccountForAttest(timePayrollTransaction, employeeAccountDimId, validAccountIdsByHiearchy, accountInternals))
                    validTimePayrollTransactions.Add(timePayrollTransaction);
                else
                    employeeResult.AddTransactionFailed(timePayrollTransaction);
            }

            return validTimePayrollTransactions;
        }

        #endregion

        #region Transactions-attest

        private bool HasValidAccountForAttest(TimePayrollTransaction timePayrollTransaction, int accountDimId, List<int> validAccountIds, List<AccountDTO> allAccountInternals)
        {
            AccountDTO accountInternalForDim = timePayrollTransaction.GetAccountInternal(accountDimId, allAccountInternals);
            return accountInternalForDim == null || validAccountIds.Contains(accountInternalForDim.AccountId);
        }

        private bool TryUpdateTimePayrollTransactionAttestState(CompEntities entities, int actorCompanyId, TimePayrollTransaction timePayrollTransaction, AttestTransitionDTO attestTransition, DateTime? date = null)
        {
            if (timePayrollTransaction.AttestStateId == attestTransition.AttestStateToId)
                return false;

            timePayrollTransaction.AttestStateId = attestTransition.AttestStateToId;
            SetModifiedProperties(timePayrollTransaction);

            AddAttestTransitionLog(entities, actorCompanyId, timePayrollTransaction.TimePayrollTransactionId, attestTransition.AttestTransitionId, TermGroup_AttestEntity.PayrollTime, date);
            return true;
        }

        private void UpdateTimeInvoiceTransactionAttestState(CompEntities entities, int actorCompanyId, TimeInvoiceTransaction timeInvoiceTransaction, AttestTransitionDTO attestTransition, DateTime? date = null)
        {
            if (timeInvoiceTransaction.AttestStateId == attestTransition.AttestStateToId)
                return;

            timeInvoiceTransaction.AttestStateId = attestTransition.AttestStateToId;
            SetModifiedProperties(timeInvoiceTransaction);

            AddAttestTransitionLog(entities, actorCompanyId, timeInvoiceTransaction.TimeInvoiceTransactionId, attestTransition.AttestTransitionId, TermGroup_AttestEntity.InvoiceTime, date);
        }

        private ActionResult SaveTimePayrollTransactionsAttestState(
            int employeeId, 
            DateTime dateFrom, 
            DateTime dateTo, 
            DateTime logDate, 
            List<SaveAttestTransactionDTO> transactionItems, 
            bool isMySelf, 
            int? attestStateFromId, 
            int attestStateToId, 
            ref int noOfValidTransactions, 
            ref int noOfInvalidTransactions, 
            AttestState attestStateInitial = null, 
            bool validatePayrollLockedAttestState = true
            )
        {
            #region Prereq

            List<AttestTransitionDTO> attestTransitionsToState = new List<AttestTransitionDTO>();

            AttestStateDTO attestStateTo = GetAttestState(attestStateToId);
            if (attestStateTo == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));

            if (attestStateFromId.HasValue)
            {
                AttestStateDTO attestStateFrom = GetAttestState(attestStateFromId.Value);
                if (attestStateFrom == null)
                    return new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));

                //Get AttestTransition from given AttestStates
                AttestTransitionDTO attestTransition = GetAttestTransition(attestStateFrom.AttestStateId, attestStateTo.AttestStateId);
                if (attestTransition == null)
                    return new ActionResult(false, (int)ActionResultSave.SaveAttestNoAttestTransitions, GetText(10086, "Inga attestövergångar hittades"));

                attestTransitionsToState.Add(attestTransition);

                if (attestStateInitial != null)
                {
                    AttestTransitionDTO attestTransitionInitial = GetAttestTransition(attestStateInitial.AttestStateId, attestStateTo.AttestStateId);
                    if (attestTransitionInitial != null)
                        attestTransitionsToState.Add(attestTransitionInitial);
                }
            }
            else
            {
                //Get all AttestTransitions that goes to given AttestState
                attestTransitionsToState = GetAttestTransitionsToState(attestStateTo.AttestStateId);
                if (attestTransitionsToState.IsNullOrEmpty())
                    return new ActionResult(false, (int)ActionResultSave.SaveAttestNoAttestTransitions, GetText(10086, "Inga attestövergångar hittades"));
            }

            //Valid AttestTransitions
            List<AttestUserRoleView> userValidTransitions = new List<AttestUserRoleView>();
            List<AttestTransitionDTO> employeeGroupTransitions = new List<AttestTransitionDTO>();
            if (isMySelf)
            {
                //Valid AttestTransitions EmployeeGroup
                employeeGroupTransitions = GetAttestTransitionsForEmployee(employeeId, TermGroup_AttestEntity.PayrollTime);
                if (employeeGroupTransitions.IsNullOrEmpty())
                    return new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsEmployeeGroup, GetText(10092, "Inga giltiga attestövergångar för avtalsgruppen hittades"));
            }
            else
            {
                userValidTransitions = GetAttestUserRoleViews(dateFrom, dateTo);
                if (userValidTransitions.IsNullOrEmpty())
                    return new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsAttestRole, GetText(10086, "Inga attestövergångar hittades"));
            }

            #endregion

            #region Perform

            List<TimePayrollTransaction> allTimePayrollTransactions = GetTimePayrollTransactionsWithTimeBlockDate(employeeId, dateFrom, dateTo).ExcludeStartValues();
            List<TimePayrollTransaction> validTimePayrollTransactions = new List<TimePayrollTransaction>();
            foreach (SaveAttestTransactionDTO transactionItem in transactionItems)
            {
                foreach (int timePayrollTransactionId in transactionItem.TimePayrollTransactionIds)
                {
                    TimePayrollTransaction timePayrollTransaction = allTimePayrollTransactions.FirstOrDefault(i => i.TimePayrollTransactionId == timePayrollTransactionId);
                    if (timePayrollTransaction != null && !validTimePayrollTransactions.Any(i => i.TimePayrollTransactionId == timePayrollTransaction.TimePayrollTransactionId))
                        validTimePayrollTransactions.Add(timePayrollTransaction);
                }
            }

            ActionResult result = TrySetTimePayrollTransactionsAttestState(
                validTimePayrollTransactions, 
                attestStateTo, 
                attestTransitionsToState, 
                userValidTransitions, 
                employeeGroupTransitions, 
                logDate, 
                isMySelf: isMySelf,
                noOfValidTransactions: ref noOfValidTransactions,
                noOfInvalidTransactions: ref noOfInvalidTransactions,
                validatePayrollLockedAttestState: validatePayrollLockedAttestState
                );
            if (!result.Success)
                return result;

            result = Save();
            if (!result.Success)
                return result;

            #endregion

            return result;
        }
        
        private ActionResult TrySaveTimePayrollTransactionsAttestState(
            EmployeeAttestResult employeeResult,
            List<TimePayrollTransaction> timePayrollTransactions, 
            AttestStateDTO attestStateTo, 
            List<AttestTransitionDTO> attestTransitionsToState, 
            List<AttestUserRoleView> userValidTransitions, 
            List<AttestTransitionDTO> employeeGroupTransitions, 
            DateTime logDate,
            bool isMySelf,
            bool validatePayrollLockedAttestState = true, 
            bool tryNotifyEmployee = false, 
            params int[] invalidAttestStateIds
            )
        {
            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);

            List<TimePayrollTransaction> validTimePayrollTransactions = new List<TimePayrollTransaction>();

            if (!UsePayroll())
                validatePayrollLockedAttestState = false;

            LoadLockedAttestStateIds(validatePayrollLockedAttestState, out int companyPayrollLockedAttestStateId, out int companyPayrollApproved1AttestStateId, out int companyPayrollApproved2AttestStateId);

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions.Where(i => i.AttestStateId != attestStateTo.AttestStateId))
            {
                bool isTransitionValid = false;

                try
                {
                    if (!TryGetAttestTransition(timePayrollTransaction, attestTransitionsToState, out var attestTransition))
                        continue;
                    if (!IsTransactionValidForAttest(timePayrollTransaction, attestTransition, invalidAttestStateIds, validatePayrollLockedAttestState, companyPayrollLockedAttestStateId, companyPayrollApproved1AttestStateId, companyPayrollApproved2AttestStateId))
                        continue;
                    if (!HasValidAttestTransition(isMySelf, attestTransition, employeeGroupTransitions, userValidTransitions))
                        continue;

                    isTransitionValid = true;
                    validTimePayrollTransactions.Add(timePayrollTransaction);

                    if (tryNotifyEmployee)
                        AddCurrentDayAsAttested(attestTransition, timePayrollTransaction.TimeBlockDate ?? GetTimeBlockDateFromCache(timePayrollTransaction.EmployeeId, timePayrollTransaction.TimeBlockDateId));
                }
                finally
                {
                    if (isTransitionValid)
                        employeeResult.AddTransactionAttested(timePayrollTransaction);
                    else
                        employeeResult.AddTransactionFailed(timePayrollTransaction);
                }
            }

            try
            {
                if (!validTimePayrollTransactions.Any())
                    return new ActionResult();

                List<SqlParameter> sqlParameters = new List<SqlParameter>()
                {
                    new SqlParameter("@modified", CalendarUtility.ToSqlFriendlyDateTime(DateTime.Now)),
                    new SqlParameter("@modifiedby", FullName),
                    new SqlParameter("@attestStateId", attestStateTo.AttestStateId),
                };
                string sql = $"update timepayrollTransaction set modified = @modified, modifiedby = @modifiedby, attestStateId = @attestStateId where timepayrollTransactionId in ({validTimePayrollTransactions.Select(s => s.TimePayrollTransactionId).JoinToString<int>(",")})";
                var result = SqlCommandUtil.ExecuteSqlUpsertCommand(entities, sql, sqlParameters);

                if (!result.Success)
                    return result;

                var sqlsb = new StringBuilder();
                string into = "INSERT INTO [dbo].[AttestTransitionLog]([ActorCompanyId],[AttestTransitionId],[Entity],[UserId],[Date],[RecordId]) ";
                int user = parameterObject != null && parameterObject.SupportUserId.HasValue ? parameterObject.SupportUserId.Value : userId;
                foreach (var validTimePayrollTransaction in validTimePayrollTransactions)
                {
                    AttestTransitionDTO attestTransition = attestTransitionsToState.FirstOrDefault(i => i.AttestStateFromId == validTimePayrollTransaction.AttestStateId);
                    sqlsb.Append($"{into} Values({actorCompanyId}, {attestTransition?.AttestTransitionId},{ (int)TermGroup_AttestEntity.PayrollTime},{user}, '{CalendarUtility.ToSqlFriendlyDateTime(logDate)}', {validTimePayrollTransaction.TimePayrollTransactionId})");
                    sqlsb.Append(Environment.NewLine);
                    if (validTimePayrollTransaction.EntityState != EntityState.Unchanged)
                        entities.ObjectStateManager.ChangeObjectState(validTimePayrollTransaction, EntityState.Unchanged);
                }

                return SqlCommandUtil.ExecuteSqlUpsertCommand(entities, sqlsb.ToString());
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ActionResult(ex);
            }
        }

        private ActionResult TrySetTimePayrollTransactionsAttestState(
            List<TimePayrollTransaction> timePayrollTransactions,
            AttestStateDTO attestStateTo, 
            List<AttestTransitionDTO> attestTransitionsToState, 
            List<AttestUserRoleView> userValidTransitions, 
            List<AttestTransitionDTO> employeeGroupTransitions, 
            DateTime logDate,
            bool isMySelf, 
            ref int noOfValidTransactions, 
            ref int noOfInvalidTransactions, 
            bool validatePayrollLockedAttestState = true, 
            bool tryNotifyEmployee = false, 
            params int[] invalidAttestStateIds
            )
        {
            if (timePayrollTransactions.IsNullOrEmpty())
                return new ActionResult(true);
            if (attestStateTo == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(11830, "Giltig attestövergång saknas"));

            ActionResult result = new ActionResult(true);

            if (!UsePayroll())
                validatePayrollLockedAttestState = false;

            LoadLockedAttestStateIds(validatePayrollLockedAttestState, out int companyPayrollLockedAttestStateId, out int companyPayrollApproved1AttestStateId, out int companyPayrollApproved2AttestStateId);

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions.Where(i => i.AttestStateId != attestStateTo.AttestStateId))
            {
                bool isTransitionValid = false;

                try
                {
                    if (!TryGetAttestTransition(timePayrollTransaction, attestTransitionsToState, out var attestTransition))
                        continue;
                    if (!IsTransactionValidForAttest(timePayrollTransaction, attestTransition, invalidAttestStateIds, validatePayrollLockedAttestState, companyPayrollLockedAttestStateId, companyPayrollApproved1AttestStateId, companyPayrollApproved2AttestStateId))
                        continue;
                    if (!HasValidAttestTransition(isMySelf, attestTransition, employeeGroupTransitions, userValidTransitions))
                        continue;
                    if (!TryUpdateTimePayrollTransactionAttestState(entities, actorCompanyId, timePayrollTransaction, attestTransition, logDate))
                        continue;

                    isTransitionValid = true;

                    if (tryNotifyEmployee)
                        AddCurrentDayAsAttested(attestTransition, timePayrollTransaction.TimeBlockDate ?? GetTimeBlockDateFromCache(timePayrollTransaction.EmployeeId, timePayrollTransaction.TimeBlockDateId));
                }
                finally
                {
                    if (isTransitionValid)
                        noOfValidTransactions++;
                    else
                        noOfInvalidTransactions++;
                }
            }

            return result;
        }

        private void LoadLockedAttestStateIds(bool validatePayrollLockedAttestState, out int companyPayrollLockedAttestStateId, out int companyPayrollApproved1AttestStateId, out int companyPayrollApproved2AttestStateId)
        {
            if (!validatePayrollLockedAttestState)
            {
                companyPayrollLockedAttestStateId = 0;
                companyPayrollApproved1AttestStateId = 0;
                companyPayrollApproved2AttestStateId = 0;
                return;
            }

            companyPayrollLockedAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentLockedAttestStateId);
            companyPayrollApproved1AttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentApproved1AttestStateId);
            companyPayrollApproved2AttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentApproved2AttestStateId);
        }

        private static bool TryGetAttestTransition(TimePayrollTransaction timePayrollTransaction, List<AttestTransitionDTO> attestTransitions, out AttestTransitionDTO attestTransition)
        {
            attestTransition = timePayrollTransaction != null ? attestTransitions?.FirstOrDefault(i => i.AttestStateFromId == timePayrollTransaction.AttestStateId) : null;
            return attestTransition != null && timePayrollTransaction.AttestStateId != attestTransition.AttestStateToId;
        }

        private static bool IsTransactionValidForAttest(TimePayrollTransaction timePayrollTransaction, AttestTransitionDTO attestTransition, int[] invalidAttestStateIds, bool validatePayrollLockedAttestState = false, int companyPayrollLockedAttestStateId = 0, int companyPayrollApproved1AttestStateId = 0, int companyPayrollApproved2AttestStateId = 0)
        {
            if (timePayrollTransaction == null || attestTransition == null)
                return false;
            if (timePayrollTransaction.AttestStateId == attestTransition.AttestStateToId)
                return false;
            if (invalidAttestStateIds.Contains(timePayrollTransaction.AttestStateId))
                return false;
            if (timePayrollTransaction.IsReversed && timePayrollTransaction.Exported)
                return false;

            if (validatePayrollLockedAttestState)
            {
                if (timePayrollTransaction.AttestStateId == companyPayrollLockedAttestStateId && attestTransition.AttestStateToId != companyPayrollApproved1AttestStateId && attestTransition.AttestStateToId != companyPayrollApproved2AttestStateId)
                    return false;
                if (attestTransition.AttestStateToId == companyPayrollLockedAttestStateId && timePayrollTransaction.AttestStateId != companyPayrollApproved1AttestStateId && timePayrollTransaction.AttestStateId != companyPayrollApproved2AttestStateId)
                    return false;
            }

            return true;
        }

        private static bool HasValidAttestTransition(bool isMySelf, AttestTransitionDTO attestTransition, List<AttestTransitionDTO> employeeGroupTransitions = null, List<AttestUserRoleView> userValidTransitions = null)
        {
            if (isMySelf)
                return employeeGroupTransitions?.Any(i => i.AttestStateFromId == attestTransition.AttestStateFromId && i.AttestStateToId == attestTransition.AttestStateToId) ?? false;
            else
                return userValidTransitions?.Any(i => i.AttestStateFromId == attestTransition.AttestStateFromId && i.AttestStateToId == attestTransition.AttestStateToId) ?? false;
        }

        #endregion

        #region Notify

        private void DoNotifyChangeOfDeviations()
        {
            List<TimeBlockDate> days = GetCurrentNotifyChangeOfDeviationsDays();
            if (days.IsNullOrEmpty())
                return;

            foreach (var daysByEmployee in days.GroupBy(i => i.EmployeeId))
            {
                Employee employee = GetEmployeeFromCache(daysByEmployee.Key);
                if (employee == null || employee.DontNotifyChangeOfDeviations)
                    continue;

                string datesDescription = daysByEmployee.Select(i => i.Date).GetCoherentDateRangesDescription();
                SendXEMailOnDeviationsChanged(employee, datesDescription);
            }

            ClearCurrentNotifyChangeOfDeviationsDays();
        }


        private void DoInitiatePayrollWarnings()
        {
            List<TimeBlockDate> days = GetCurrentPayrollWarningDays();                                       
            if (days.IsNullOrEmpty())
                return;

            foreach (var daysByEmployee in days.GroupBy(i => i.EmployeeId))
            {
                Employee employee = GetEmployeeFromCache(daysByEmployee.Key);
                if (employee == null)
                    continue;
                
                ActivateWarningPayrollPeriodHasChanged(employee, daysByEmployee.Select(i => i.Date).ToList());
            }

            ClearCurrentPayrollWarningDays();
        }


        private void DoNotifyChangeOfAttestState()
        {
            List<TimeBlockDate> days = GetCurrentAttestedDays();
            if (days.IsNullOrEmpty())
                return;

            foreach (var daysByEmployee in days.GroupBy(i => i.EmployeeId))
            {
                Employee employee = GetEmployeeFromCache(daysByEmployee.Key);
                if (employee == null || employee.DontNotifyChangeOfAttestState)
                    continue;

                string datesDescription = daysByEmployee.Select(i => i.Date).GetCoherentDateRangesDescription();
                SendXEMailOnAttestStateChanged(employee, datesDescription);
            }

            ClearCurrentAttestedDays();
        }

        private void DoNotifyUnlockedDays(Employee employee)
        {
            if (employee == null)
                return;

            Dictionary<int, List<DateTime>> days = GetCurrentUnlockedDays();
            if (days.IsNullOrEmpty())
                return;

            foreach (var day in days)
            {
                User receiver = GetUser(day.Key);
                if (receiver == null)
                    continue;

                string datesDescription = day.Value.GetCoherentDateRangesDescription();
                SendXEMailOnDayUnlocked(employee, receiver, datesDescription);
            }

            ClearCurrentUnlockedDays();
        }

        #endregion
    }
}
