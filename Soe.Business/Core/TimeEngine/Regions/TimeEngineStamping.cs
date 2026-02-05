using log4net.Core;
using Newtonsoft.Json;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using ZXing;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Synchronizes TimeStampEntrys and then saves TimeBlock's and transactions from them
        /// </summary>
        /// <returns>Output DTO</returns>
        private SynchTimeStampsOutputDTO TaskSynchTimeStamps()
        {
            var (iDTO, oDTO) = InitTask<SynchTimeStampsInputDTO, SynchTimeStampsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TSTimeStampEntryItems == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "TSTimeStampEntry");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Check accountDimId

                if (iDTO.AccountDimId == 0)
                    iDTO.AccountDimId = GetTimeTerminalAccountDimId(iDTO.TimeTerminalId);

                #endregion

                List<int> invalidTerminalIds = entities.TimeTerminal.Where(w => w.TerminalVersion == "2019.8.23.2" && w.ActorCompanyId == actorCompanyId).Select(s => s.TimeTerminalId).ToList();
                if (invalidTerminalIds.Contains(iDTO.TimeTerminalId))
                {
                    return oDTO;
                }

                #region Perform

                #region Save TimeStampEntry

                // Convert TimeStampEntryItems into TimeStampEntries
                foreach (TSTimeStampEntryItem entryItem in iDTO.TSTimeStampEntryItems.Where(w => w.Time > DateTime.Now.AddYears(-10) && w.Time < DateTime.Now.AddYears(10)))
                {
                    //Validate employeeid 
                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(entryItem.EmployeeId);

                    if (employee == null || employee.ActorCompanyId != actorCompanyId)
                        continue;

                    // Get TimeBlockDate
                    int minutesAfterMidnight = GetBreakDayMinutesAfterMidnightFromCache(employee.EmployeeId, entryItem.Time);
                    TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, entryItem.Time.AddMinutes(-minutesAfterMidnight).Date, true);

                    if (timeBlockDate == null)
                        continue;

                    TimeStampEntry entry = new TimeStampEntry()
                    {
                        Type = entryItem.Type,
                        Time = entryItem.Time,
                        OriginalTime = entryItem.Time,
                        Created = entryItem.Created,
                        Status = entryItem.Status,
                        OriginType = (int)TermGroup_TimeStampEntryOriginType.TerminalUnspecified,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = entryItem.EmployeeId,
                        TimeTerminalId = iDTO.TimeTerminalId.ToNullable(),

                        //Set references
                        TimeBlockDate = timeBlockDate,
                    };
                    entities.TimeStampEntry.AddObject(entry);

                    //Optional

                    //Check if InternalAccount setting on terminal
                    if (entryItem.AccountId.HasValue && entryItem.AccountId.Value != 0)
                        entry.AccountId = entryItem.AccountId.Value;
                    else
                    {
                        var internalAccountIdfromSetting = TimeStampManager.GetTimeTerminalSetting(TimeTerminalSettingType.InternalAccountDim1Id, iDTO.TimeTerminalId);
                        if (internalAccountIdfromSetting != null)
                            entry.AccountId = (internalAccountIdfromSetting.IntData.HasValue && internalAccountIdfromSetting.IntData.Value != 0) ? internalAccountIdfromSetting.IntData : (int?)null;
                    }

                    if (entryItem.TimeDeviationCauseId.HasValidValue())
                        entry.TimeDeviationCauseId = entryItem.TimeDeviationCauseId;
                    if (entryItem.TimeScheduleTemplatePeriodId.HasValidValue())
                        entry.TimeScheduleTemplatePeriodId = entryItem.TimeScheduleTemplatePeriodId.Value;

                    //Keep track of relation
                    oDTO.UpdatedTimeStampEntries[entryItem.TimeStampEntryInternalId] = entry;

                    //Keep track of employee and status
                    if (oDTO.SuccessfulAddedTimeStampEmployeeIds.ContainsKey(entryItem.EmployeeId))
                    {
                        var employeeInfo = oDTO.SuccessfulAddedTimeStampEmployeeIds.First(f => f.Key == entryItem.EmployeeId);
                        employeeInfo.Value.Add(entryItem.Time);
                    }
                    else
                        oDTO.SuccessfulAddedTimeStampEmployeeIds.Add(entryItem.EmployeeId, new List<DateTime>() { entryItem.Time });
                }

                Save();

                #endregion

                #endregion
            }

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SynchGTSTimeStampsOutputDTO TaskSynchGTSTimeStamps()
        {
            var (iDTO, oDTO) = InitTask<SynchGTSTimeStampsInputDTO, SynchGTSTimeStampsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TimeStampEntryItems == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampEntry");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Perform

                #region Save TimeStampEntry

                DateTime created = DateTime.Now;
                List<TimeStampEntry> pubSubEntries = new List<TimeStampEntry>();

                // Convert GoTimeStampTimeStamp into TimeStampEntries
                foreach (GoTimeStampTimeStamp entryItem in iDTO.TimeStampEntryItems.OrderBy(e => e.EmployeeId).ThenBy(e => e.TimeStamp))
                {
                    // Validate employee
                    Employee employee = GetEmployeeWithContactPersonAndEmployment(entryItem.EmployeeId);
                    if (employee == null || employee.ActorCompanyId != actorCompanyId)
                        continue;

                    // TimeStamps are sent with UTC-time
                    // Adjust them based on time zone setting on terminal
                    entryItem.TimeStamp = CalendarUtility.ClearSeconds(TimeStampManager.GetLocalTimeForTerminal(entryItem.TimeStamp, iDTO.TimeTerminalId));

                    // Get TimeBlockDate
                    int minutesAfterMidnight = GetBreakDayMinutesAfterMidnightFromCache(entryItem.EmployeeId, entryItem.TimeStamp);
                    TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(entryItem.EmployeeId, entryItem.TimeStamp.AddMinutes(-minutesAfterMidnight).Date, true);

                    TimeStampEntry entry = new TimeStampEntry()
                    {
                        Type = (int)entryItem.EntryType,
                        Time = entryItem.TimeStamp,
                        OriginalTime = entryItem.TimeStamp,
                        Created = created,
                        Status = (int)TermGroup_TimeStampEntryStatus.New,
                        OriginType = (int)entryItem.OriginType,
                        IsBreak = entryItem.IsBreak && entryItem.EntryType == TimeStampEntryType.Out,
                        IsPaidBreak = entryItem.IsPaidBreak && entryItem.EntryType == TimeStampEntryType.Out,
                        IsDistanceWork = entryItem.IsDistanceWork,

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = entryItem.EmployeeId,
                        TimeTerminalId = iDTO.TimeTerminalId.ToNullable(),

                        //Set references
                        TimeBlockDate = timeBlockDate,
                    };
                    entities.TimeStampEntry.AddObject(entry);

                    //Optional

                    //Check if InternalAccount setting on terminal
                    if (entryItem.AccountId.HasValue && entryItem.AccountId.Value != 0)
                        entry.AccountId = entryItem.AccountId.Value;
                    else
                    {
                        var internalAccountIdfromSetting = TimeStampManager.GetTimeTerminalSetting(TimeTerminalSettingType.InternalAccountDim1Id, iDTO.TimeTerminalId);
                        if (internalAccountIdfromSetting != null)
                            entry.AccountId = (internalAccountIdfromSetting.IntData.HasValue && internalAccountIdfromSetting.IntData.Value != 0) ? internalAccountIdfromSetting.IntData : (int?)null;
                    }

                    if (entryItem.DeviationCauseId.HasValue)
                        entry.TimeDeviationCauseId = entryItem.DeviationCauseId;

                    // Check IP filter
                    if (entryItem.InvalidIPAddress)
                    {
                        entry.ManuallyAdjusted = true;
                        entry.Note = GetText(12048, "Stämpling skapad med ogiltig IP-adress");
                    }

                    // Save client information (OS, Browser etc)
                    if (!string.IsNullOrEmpty(entryItem.Data))
                        entry.TerminalStampData = entryItem.Data.Left(512);

                    // Extended
                    if (!entryItem.Extended.IsNullOrEmpty())
                    {
                        foreach (GoTimeStampTimeStampEntryExtended extendedItem in entryItem.Extended)
                        {
                            TimeStampEntryExtended extended = new TimeStampEntryExtended()
                            {
                                TimeStampEntry = entry,
                                TimeScheduleTypeId = extendedItem.TimeScheduleTypeId,
                                TimeCodeId = extendedItem.TimeCodeId,
                                AccountId = extendedItem.AccountId,
                                Quantity = extendedItem.Quantity,
                                Created = created,
                            };
                        }
                    }

                    //Keep track of employee and status
                    GoTimeStampEmployeeStampStatus employeeStatus = oDTO.EmployeeStampStatuses.FirstOrDefault(e => e.Employee.EmployeeId == entryItem.EmployeeId);
                    if (employeeStatus == null)
                    {
                        employeeStatus = new GoTimeStampEmployeeStampStatus
                        {
                            Employee = employee.ToGoTimeStampDTO()
                        };
                        oDTO.EmployeeStampStatuses.Add(employeeStatus);
                    }

                    // Always update employee status with latest entry
                    employeeStatus.TimeStamp = entryItem.TimeStamp;
                    employeeStatus.StampedIn = entryItem.EntryType == TimeStampEntryType.In;
                    employeeStatus.OnBreak = entryItem.IsBreak && entryItem.EntryType == TimeStampEntryType.Out;
                    employeeStatus.OnPaidBreak = entryItem.IsPaidBreak && entryItem.EntryType == TimeStampEntryType.Out;
                    employeeStatus.OnDistanceWork = entryItem.IsDistanceWork && entryItem.EntryType == TimeStampEntryType.In;

                    // Only add one timestamp per employee
                    if (!pubSubEntries.Any(e => e.EmployeeId == entry.EmployeeId))
                        pubSubEntries.Add(entry);
                }

                oDTO.Result = Save();

                #endregion

                #endregion

                #region WebPubSub

                if (oDTO.Result.Success && pubSubEntries.Any())
                {
                    Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() =>
                    {
                        using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                        List<int> terminalIds = TimeStampManager.GetTimeTerminalIdsForPubSub(entitiesReadOnly, actorCompanyId);
                        foreach (TimeStampEntry entry in pubSubEntries)
                        {
                            TimeStampManager.SendWebPubSubMessage(entitiesReadOnly, entry, WebPubSubMessageAction.Insert, terminalIds);
                        }
                    }));
                }

                #endregion
            }

            return oDTO;
        }

        /// <summary>
        /// Saves TimeBlock's and transactions from TimeStampEntrys passed
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveDeviationsFromStampingOutputDTO TaskReGenerateDayBasedOnTimeStamps()
        {
            var (iDTO, oDTO) = InitTask<SaveDeviationsFromStampingInputDTO, SaveDeviationsFromStampingOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TimeStampEntryInputs == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampEntry");
                return oDTO;
            }

            #region Init

            // Copy all inputs to a new list, so the iDTO list can be used to populate input to next call
            List<TimeStampEntry> originalTargetEntries = new List<TimeStampEntry>();
            originalTargetEntries.AddRange(iDTO.TimeStampEntryInputs);

            iDTO.TimeStampEntryInputs.Clear();

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Perform

                List<TimeStampEntry> timeStampEntries = GetTimeStampEntriesToProcess(originalTargetEntries);
                iDTO.TimeStampEntryInputs.AddRange(timeStampEntries);

                // Keep current status if we need to revert
                iDTO.TimeStampEntryInputs.ForEach(i => i.OldStatus = i.Status);

                // Change current status and save, to prevent other processes to use them
                oDTO.Result = SaveTimeStampEntryStatus(iDTO.TimeStampEntryInputs, TermGroup_TimeStampEntryStatus.Processing);
                if (!oDTO.Result.Success)
                    return oDTO;

                oDTO.Result = TaskSaveDeviationsFromTimeStamps().Result;
                if (oDTO.Result.Success && timeStampEntries.Any() && !oDTO.OvertimeTimeBlockDateIds.IsNullOrEmpty())
                    oDTO.Result = TryRestoreUnhandledShiftsChanges(timeStampEntries.First().EmployeeId, oDTO.OvertimeTimeBlockDateIds);

                #endregion
            }

            return oDTO;
        }

        /// <summary>
        /// Saves TimeBlock's and transactions from TimeStampEntrys passed
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveDeviationsFromStampingJobOutputDTO TaskSaveDeviationsFromStampingJob()
        {
            var (iDTO, oDTO) = InitTask<SaveDeviationsFromStampingJobInputDTO, SaveDeviationsFromStampingJobOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TimeStampEntryInputs == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStampEntry");
                return oDTO;
            }

            #region Init

            // Copy all inputs to a new list, so the iDTO list can be used to populate input to next call
            List<TimeStampEntry> originalTargetEntries = new List<TimeStampEntry>();
            originalTargetEntries.AddRange(iDTO.TimeStampEntryInputs);

            iDTO.TimeStampEntryInputs.Clear();

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Perform

                List<TimeStampEntry> timeStampEntries = GetTimeStampEntriesToProcess(originalTargetEntries);
                iDTO.TimeStampEntryInputs.AddRange(timeStampEntries);

                #region Set entry status

                // Keep current status if we need to revert
                iDTO.TimeStampEntryInputs.ForEach(i => i.OldStatus = i.Status);

                // Change current status and save, to prevent other processes to use them
                oDTO.Result = SaveTimeStampEntryStatus(iDTO.TimeStampEntryInputs, TermGroup_TimeStampEntryStatus.Processing);
                if (!oDTO.Result.Success)
                    return oDTO;

                #endregion

                oDTO.Result = TaskSaveDeviationsFromTimeStamps().Result;

                #endregion
            }

            return oDTO;
        }

        /// <summary>
        /// Saves deviations from TimeStamps
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveTimeStampsOutputDTO TaskSaveDeviationsFromTimeStamps()
        {
            var (iDTO, oDTO) = InitTask<SaveTimeStampsInputDTO, SaveTimeStampsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TimeStampEntryInputs == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "TimeStamp");
                return oDTO;
            }

            //Because of this this function is depending on that deleted TimeStampEntrys are passed in aswell, otherwise it wont work if all TimeStampEntrys where deleted
            if (!iDTO.TimeStampEntryInputs.Any())
            {
                oDTO.Result = new ActionResult();
                return oDTO;
            }

            try
            {
                #region Prereq

                List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();
                bool doNotModifyTimeStampEntryType = GetCompanyBoolSettingFromCache(CompanySettingType.TimeDoNotModifyTimeStampEntryType);
                List<int> employeeIds = iDTO.TimeStampEntryInputs.Select(s => s.EmployeeId).Distinct().ToList();

                IQueryable<Employee> query = (from e in entities.Employee
                                                    .Include("ContactPerson")
                                                    .Include("Employment.EmploymentChangeBatch.EmploymentChange")
                                                    .Include("Employment.OriginalEmployeeGroup")
                                                    .Include("Employment.OriginalPayrollGroup")
                                                    .Include("Employment.OriginalAnnualLeaveGroup")
                                              where e.ActorCompanyId == actorCompanyId &&
                                              employeeIds.Contains(e.EmployeeId)
                                              select e);

                if (base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId))
                    query = query.Include("EmployeeAccount.Account.AccountInternal");

                List<Employee> employees = query.ToList();

                List<AttestState> attestStates = GetAttestStatesForTimeFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                AttestState attestStateAttested = AttestManager.GetAttestState(entities, attestStates, CompanySettingType.SalaryExportPayrollMinimumAttestStatus);
                AttestState attestStateInitial = attestStates.FirstOrDefault(a => a.Initial);

                List<TrackChangesDTO> trackChangesItems = new List<TrackChangesDTO>();
                TermGroup_TrackChangesActionMethod actionMethod = TermGroup_TrackChangesActionMethod.TimeStampEntry_Job;

                #endregion

                #region Perform

                foreach (var timeStampEntrysByEmployee in iDTO.TimeStampEntryInputs.Where(t => t.TimeBlockDateId.HasValue).GroupBy(i => i.EmployeeId))
                {
                    Employee employee = employees.FirstOrDefault(i => i.EmployeeId == timeStampEntrysByEmployee.Key);
                    if (employee == null)
                        continue;

                    foreach (var timeStampEntrysByEmployeeAndDate in timeStampEntrysByEmployee.GroupBy(i => i.TimeBlockDateId.Value))
                    {
                        oDTO.Result = new ActionResult();

                        // Get all entries for current employee and current date (even deleted to make deletion of TimeBlocks and transactions to work)
                        List<TimeStampEntry> allTimeStampEntrysForEmployeeAndDate = timeStampEntrysByEmployeeAndDate.ToList();
                        List<TimeStampEntry> timeStampEntrysForEmployeeAndDate = allTimeStampEntrysForEmployeeAndDate.Where(tse => tse.State == (int)SoeEntityState.Active).OrderBy(e => e.Time).ThenBy(i => i.TimeStampEntryId).ToList();
                        string employeeInfo = "";

                        try
                        {
                            #region Validate Employee

                            TimeBlockDate timeBlockDate = timeStampEntrysByEmployeeAndDate.First().TimeBlockDate ?? GetTimeBlockDateFromCache(employee.EmployeeId, timeStampEntrysByEmployeeAndDate.Key);
                            if (timeBlockDate == null)
                                continue;

                            employeeInfo = employee.GetNameOrNumberAndDateString(timeBlockDate, actorCompanyId);

                            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, employeeGroups: employeeGroups);
                            if (employee.State != (int)SoeEntityState.Active || employeeGroup == null)
                            {
                                // Change current status and save, to prevent entrys to be handled again
                                oDTO.Result = SaveTimeStampEntryStatus(timeStampEntrysByEmployeeAndDate.ToList(), TermGroup_TimeStampEntryStatus.ProcessedWithNoResult);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                continue;
                            }

                            #endregion

                            #region Validate TimeStampEntrys

                            if (timeStampEntrysForEmployeeAndDate.Any())
                            {
                                if (IsDayAttested(attestStateAttested, employee.EmployeeId, timeBlockDate.TimeBlockDateId))
                                {
                                    bool removedStamps = false;
                                    foreach (TimeStampEntry entry in timeStampEntrysForEmployeeAndDate.Where(t => t.OldStatus == (int)TermGroup_TimeStampEntryStatus.New))
                                    {
                                        string note = GetText(12163, "Borttagen p.g.a. attesterad dag");

                                        trackChangesItems.Add(TrackChangesManager.InitTrackChanges(entities, actorCompanyId, actionMethod, TermGroup_TrackChangesAction.Delete, SoeEntityType.Employee, entry.EmployeeId, SoeEntityType.TimeStampEntry, entry.TimeStampEntryId, SettingDataType.String, null, TermGroup_TrackChangesColumnType.Unspecified, entry.Note, note));

                                        ChangeEntityState(entry, SoeEntityState.Deleted);
                                        entry.OldStatus = (int)TermGroup_TimeStampEntryStatus.ProcessedWithNoResult;
                                        entry.Note = note;
                                        removedStamps = true;
                                    }

                                    if (removedStamps)
                                    {
                                        #region TrackChanges

                                        // Add track changes
                                        if (trackChangesItems.Any())
                                        {
                                            ActionResult trackResult = TrackChangesManager.AddTrackChanges(entities, this.currentTransaction, trackChangesItems);
                                            if (trackResult.Success)
                                                trackChangesItems.Clear();
                                        }

                                        #endregion

                                        SaveTimeStampEntryStatus(allTimeStampEntrysForEmployeeAndDate, null);
                                        continue;
                                    }
                                }

                                if (HasTimeStampsError(timeStampEntrysForEmployeeAndDate, out TermGroup_TimeBlockDateStampingStatus errorStampingStatus, doNotModifyTimeStampEntryType))
                                {
                                    RevertTimeStampEntryAndTimeBlockDateStatus(allTimeStampEntrysForEmployeeAndDate, employee, timeBlockDate, errorStampingStatus, MethodBase.GetCurrentMethod().Name);
                                    continue;
                                }
                                else if (!HasTimeStampsError(timeStampEntrysForEmployeeAndDate, TermGroup_TimeBlockDateStampingStatus.OddNumberOfStamps) && timeStampEntrysForEmployeeAndDate.All(i => i.OldStatus == (int)TermGroup_TimeStampEntryStatus.Partial))
                                {
                                    LogAndRevertEntryStatus(oDTO.Result, allTimeStampEntrysForEmployeeAndDate, employee, timeBlockDate, Level.Info, MethodBase.GetCurrentMethod().Name, "Fixed partial stamps");
                                    SaveTimeStampEntryStatus(timeStampEntrysForEmployeeAndDate, TermGroup_TimeStampEntryStatus.Processed);
                                    continue;
                                }
                            }

                            #endregion

                            #region Delete existing TimeBlocks and Transactions

                            List<TimeBlock> timeBlocksToDelete = GetTimeBlocksWithTransactions(employee.EmployeeId, timeBlockDate.TimeBlockDateId, onlyActive: false);
                            List<TimeBlock> timeBlocksBefore = timeBlocksToDelete.Where(t => t.State == (int)SoeEntityState.Active).ToList();
                            int? preservedAttestStateId = null;
                            if (iDTO.DiscardAttesteState)
                            {
                                List<TimePayrollTransaction> timePayrollTransactionsForDay = timeBlocksToDelete.GetTimePayrollTransactions(discardTimeBlockState: true);

                                int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();
                                List<AttestStateDTO> payrollLockedAttestStates = GetAttestStatesFromCache(payrollLockedAttestStateIds);

                                if (!timePayrollTransactionsForDay.GetAttestStateIds().IsEqualToAny(payrollLockedAttestStates.Select(i => i.AttestStateId).ToArray()))
                                    preservedAttestStateId = PreserveTransactionsAttestStateId(timePayrollTransactionsForDay, attestStateInitial.AttestStateId);
                            }

                            oDTO.Result = SetTimeBlocksAndTransactionsAndScheduleTransactionsToDeleted(timeBlocksToDelete, timeBlockDate, employee.EmployeeId, doDeleteTimeBlockDateDetailsWithoutRatio: true, clearScheduledAbsence: false, saveChanges: false);
                            if (!oDTO.Result.Success)
                            {
                                LogAndRevertEntryStatus(oDTO.Result, allTimeStampEntrysForEmployeeAndDate, employee, timeBlockDate, Level.Warn, MethodBase.GetCurrentMethod().Name, "SetTimeBlocksAndTransactionsToDeletedForDay");
                                if (timeBlockDate.Date <= DateTime.Today.AddDays(-7))
                                    SaveTimeStampEntryStatus(timeStampEntrysForEmployeeAndDate, TermGroup_TimeStampEntryStatus.ProcessedWithNoResult);

                                continue;
                            }

                            oDTO.Result = Save();

                            if (!oDTO.Result.Success)
                            {
                                LogAndRevertEntryStatus(oDTO.Result, allTimeStampEntrysForEmployeeAndDate, employee, timeBlockDate, Level.Warn, MethodBase.GetCurrentMethod().Name, "DeleteTimeBlocksAndTransactions");
                                continue;
                            }

                            if (!timeStampEntrysForEmployeeAndDate.Any())
                            {
                                oDTO.Result = SaveTimeBlockDateStampingStatus(timeBlockDate, TermGroup_TimeBlockDateStampingStatus.NoStamps);
                                if(oDTO.Result.Success)
                                    DoInitiatePayrollWarnings();

                                continue;
                            }

                            #endregion

                            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                            {
                                InitTransaction(transaction);

                                if (TryCreateTimeBlocksFromTimeStamps(out List<TimeBlock> timeBlocksToAdd, ref oDTO, allTimeStampEntrysForEmployeeAndDate, timeStampEntrysForEmployeeAndDate, employee, employeeGroup, timeBlockDate, doLogAndRevertErrors: true))
                                    ApplySaveDeviationsFromTimeStamps(oDTO, timeBlockDate, employee, allTimeStampEntrysForEmployeeAndDate, timeBlocksBefore, timeBlocksToAdd, iDTO.DiscardBreakEvaluation, preservedAttestStateId);

                                if (TimeStampManager.HasExtendedTimeStamps(entities, ActorCompanyId) && timeStampEntrysForEmployeeAndDate.Any(a => !a.TimeStampEntryExtended.IsNullOrEmpty()))
                                {
                                    List<TimeStampEntryExtended> expenseRows = timeStampEntrysByEmployeeAndDate.Where(w => !w.TimeStampEntryExtended.IsNullOrEmpty()).SelectMany(s => s.TimeStampEntryExtended).Where(e => e.TimeCodeId.HasValue).ToList();
                                    if (!expenseRows.IsNullOrEmpty())
                                        SaveExpenseFromTimeStampExtended(employee, timeBlockDate, expenseRows);
                                }

                                DoInitiatePayrollWarnings();

                                if (!TryCommit(oDTO))
                                    Rollback();
                            }
                        }
                        catch (Exception ex)
                        {
                            oDTO.Result.Exception = ex;
                            LogError(ex);
                            LogCollector.LogError($"{MethodBase.GetCurrentMethod().Name} failed on " + employeeInfo);
                        }
                        finally
                        {
                            //Revert if failed
                            if (!oDTO.Result.Success)
                                RevertTimeStampEntryStatus(allTimeStampEntrysForEmployeeAndDate, oDTO.Result);
                        }
                    }

                    if (oDTO.Result.Success)
                        oDTO.Result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.ApplyAndRestore, employee.EmployeeId);
                }

                if (oDTO.Result.Success && this.localDoNotCollectDaysForRecalculationLevel2.HasValue && this.localDoNotCollectDaysForRecalculationLevel2.Value == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence && this.localDoNotCollectDaysForRecalculationLevel3.HasValue && this.localDoNotCollectDaysForRecalculationLevel3Reversed != null && this.localDoNotCollectDaysForRecalculationLevel3Reversed.Any(i => i != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick))
                    oDTO.Result.InfoMessage = GetText(11509, "Kontrollera dagar både framåt och bakåt med samma frånvarotyp. Perioden är låst och därför måste dessa dagar hanteras manuellt");

                #endregion
            }
            catch (Exception ex)
            {
                oDTO.Result.Exception = ex;
            }
            finally
            {
                if (!oDTO.Result.Success)
                    LogTransactionFailed(this.ToString());
            }

            return oDTO;
        }

        #endregion

        #region SaveDeviationsFromTimeStamps helpers

        private EmployeeChild GetTimeStampEmployeeChild(TimeStampEntry timeStampEntry)
        {
            if (timeStampEntry == null || !timeStampEntry.EmployeeChildId.HasValue)
                return null;

            if (!timeStampEntry.EmployeeChildReference.IsLoaded)
                timeStampEntry.EmployeeChildReference.Load();
            return timeStampEntry.EmployeeChild;
        }

        private AccountInternal GetTimeStampAccountInternal(TimeStampEntry timeStampEntry, bool useAccountHierarchy)
        {
            if (timeStampEntry == null)
                return null;

            AccountInternal accountInternal = null;
            if (timeStampEntry.AccountId.HasValue)
                accountInternal = GetAccountInternalWithAccountFromCache(timeStampEntry.AccountId.Value);
            else if (useAccountHierarchy && timeStampEntry.TimeTerminalAccountId.HasValue)
                accountInternal = GetAccountInternalWithAccountFromCache(timeStampEntry.TimeTerminalAccountId.Value);
            return accountInternal;
        }

        private List<AccountInternal> GetTimeStampAccountInternals(TimeStampEntry timeStampEntry, bool useAccountHierarchy)
        {
            if (timeStampEntry == null)
                return null;

            List<AccountInternal> accountInternals = new List<AccountInternal>();
            AccountInternal onTimeStampEntry = GetTimeStampAccountInternal(timeStampEntry, useAccountHierarchy);

            if (onTimeStampEntry != null)
                accountInternals.Add(onTimeStampEntry);

            if (timeStampEntry.TimeStampEntryExtended.IsLoaded && !timeStampEntry.TimeStampEntryExtended.IsNullOrEmpty())
            {
                var accountIds = timeStampEntry.TimeStampEntryExtended.Where(w => w.AccountId.HasValue).Select(s => s.AccountId.Value).Distinct().ToList();

                if (accountIds.Any())
                {
                    List<AccountInternal> accountInternalsExtended = GetAccountInternalsWithAccountFromCache(accountIds);
                    if (!accountInternalsExtended.IsNullOrEmpty())
                        accountInternals.AddRange(accountInternalsExtended.Where(w => w != null));
                }
            }

            return accountInternals;
        }

        private bool TryCreateTimeBlocksFromTimeStamps<T>(out List<TimeBlock> timeBlocksToAdd, ref T output, List<TimeStampEntry> timeStampEntrysForEmployeeAndDate, Employee employee, EmployeeGroup employeeGroup, TimeBlockDate timeBlockDate) where T : TimeEngineOutputDTO
        {
            return TryCreateTimeBlocksFromTimeStamps(out timeBlocksToAdd, ref output, null, timeStampEntrysForEmployeeAndDate, employee, employeeGroup, timeBlockDate, false);
        }

        private bool TryCreateTimeBlocksFromTimeStamps<T>(out List<TimeBlock> timeBlocksToAdd, ref T output, List<TimeStampEntry> allTimeStampEntrysForEmployeeAndDate, List<TimeStampEntry> timeStampEntrysForEmployeeAndDate, Employee employee, EmployeeGroup employeeGroup, TimeBlockDate timeBlockDate, bool doLogAndRevertErrors) where T : TimeEngineOutputDTO
        {
            timeBlocksToAdd = new List<TimeBlock>();

            if (timeStampEntrysForEmployeeAndDate.IsNullOrEmpty())
                return true;

            bool useTimeScheduleTypeFromTime = GetCompanyBoolSettingFromCache(CompanySettingType.UseTimeScheduleTypeFromTime);
            bool doNotModifyTimeStampEntryType = GetCompanyBoolSettingFromCache(CompanySettingType.TimeDoNotModifyTimeStampEntryType);
            bool useAccountHierarchy = UseAccountHierarchy();
            int defaultEmployeeAccountDimEmployeeAccountDimId = useAccountHierarchy ? GetCompanyIntSettingFromCache(CompanySettingType.DefaultEmployeeAccountDimEmployee) : 0;
            int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);

            List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesFromCache();
            TimeDeviationCause timeDeviationCauseEmployee = GetTimeDeviationCauseFromPrio(employee, employeeGroup, null);
            TimeScheduleTemplatePeriod templatePeriod = GetTimeScheduleTemplatePeriodFromCache(employee.EmployeeId, timeBlockDate.Date);
            List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, employee.EmployeeId, timeBlockDate.Date, includeStandBy: true);
            DateTime scheduleIn = scheduleBlocks.GetScheduleIn();
            DateTime scheduleOut = scheduleBlocks.GetScheduleOut();

            TimeStampEntry stampIn = null;
            TimeStampEntry stampOut = null;
            List<TimeStampEntry> stampOutPaidBrakes = new List<TimeStampEntry>();
            List<AccountInternal> accountInternals = new List<AccountInternal>();
            bool isFirstTimeStamp = true;
            bool hasEntryError = false;
            int entryCounter = 0;

            foreach (TimeStampEntry timeStampEntry in timeStampEntrysForEmployeeAndDate)
            {
                entryCounter++;

                #region Validate type

                TimeTerminal timeTerminal = null;
                if (doNotModifyTimeStampEntryType && timeStampEntry.TimeTerminalId.HasValue)
                    timeTerminal = timeStampEntry.TimeTerminal ?? GetTimeTerminalFromCache(timeStampEntry.TimeTerminalId.Value, discardState: true);

                bool recalculateType = !TimeStampManager.DoNotRecalulateTimeStampEntryType(doNotModifyTimeStampEntryType, timeStampEntry, timeTerminal);

                if (stampIn == null)
                {
                    stampIn = timeStampEntry;
                    if (recalculateType)
                        stampIn.Type = (int)TimeStampEntryType.In;
                    continue;
                }
                else
                {
                    stampOut = timeStampEntry;
                    if (recalculateType)
                        stampOut.Type = (int)TimeStampEntryType.Out;

                    if (stampOut.IsBreak && stampOut.IsPaidBreak)
                        stampOutPaidBrakes.Add(stampOut);
                }

                #endregion

                #region Check double-stamp

                bool isValidDoubleStamp = false;
                if (stampIn.Time == stampOut.Time && stampIn.TimeDeviationCauseId == stampOut.TimeDeviationCauseId)
                {
                    isValidDoubleStamp = stampIn.Time == timeStampEntrysForEmployeeAndDate[0].Time || stampIn.Time == timeStampEntrysForEmployeeAndDate[timeStampEntrysForEmployeeAndDate.Count - 1].Time;
                    if (!isValidDoubleStamp)
                    {
                        stampIn.Invalid = true;
                        stampOut.Invalid = true;
                        stampIn = null;
                        SaveTimeBlockDateStampingStatus(timeBlockDate, TermGroup_TimeBlockDateStampingStatus.InvalidDoubleStamp, save: false);
                        continue;
                    }
                }

                #endregion

                #region In

                DateTime start = CalendarUtility.GetScheduleTime(stampIn.Time, timeBlockDate.Date, stampIn.Time);

                // TimeDeviationCause start
                TimeDeviationCause deviationCauseStart = GetTimeDeviationCauseFromPrio(employee, employeeGroup, stampIn);
                if (deviationCauseStart == null)
                {
                    output.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCauseStart");
                    hasEntryError = true;
                    if (doLogAndRevertErrors)
                        LogAndRevertEntryStatus(output.Result, allTimeStampEntrysForEmployeeAndDate, employee, timeBlockDate, Level.Error, MethodBase.GetCurrentMethod().Name, "GetTimeDeviationCauseStartFromPrio");
                    break;
                }

                //TimeScheduleTemplatePeriod
                if (templatePeriod == null)
                {
                    hasEntryError = true;
                    if (doLogAndRevertErrors)
                    {
                        AddTimeStampSystemInfoLog(employee, timeBlockDate);
                        SaveTimeStampEntryStatus(allTimeStampEntrysForEmployeeAndDate, TermGroup_TimeStampEntryStatus.ProcessedWithNoResult);
                    }
                    break;
                }

                // TimeSpot terminals do not set the period when the stamp entry is saved
                if (stampIn.TimeScheduleTemplatePeriod == null)
                    stampIn.TimeScheduleTemplatePeriod = templatePeriod;

                // TimeCode
                if (!deviationCauseStart.TimeCodeReference.IsLoaded)
                    deviationCauseStart.TimeCodeReference.Load();
                TimeCode timeCode = GetTimeCodeFromDeviationCause(deviationCauseStart, defaultTimeCodeId);

                #endregion

                #region Paid break (between previous out and in)

                if (stampOutPaidBrakes.Any() && stampIn.Time > stampOutPaidBrakes[0].Time)
                {
                    // Previous stamp out was on a paid break
                    // Create a timeblock for that break (hole between out and in)
                    TimeStampEntry stampOutPaidBrake = stampOutPaidBrakes[0];

                    DateTime breakTimeBlockStart = CalendarUtility.GetScheduleTime(stampOutPaidBrake.Time, timeBlockDate.Date, stampOutPaidBrake.Time);
                    DateTime breakTimeBlockStop = CalendarUtility.GetScheduleTime(stampIn.Time, timeBlockDate.Date, stampIn.Time);

                    // Take deviation cause from stamp (configured in terminal setting)
                    TimeDeviationCause terminalDeviationCause = null;
                    if (stampOutPaidBrake.TimeDeviationCauseId.HasValue)
                        terminalDeviationCause = timeDeviationCauses.FirstOrDefault(t => t.TimeDeviationCauseId == stampOutPaidBrake.TimeDeviationCauseId.Value);

                    //Create TimeBlock
                    TimeBlock timeBlockToAdd = CreateTimeBlockFromStamping(breakTimeBlockStart, breakTimeBlockStop, timeBlockDate, stampOutPaidBrake, stampIn, scheduleOut, null, employee, terminalDeviationCause ?? deviationCauseStart, terminalDeviationCause ?? deviationCauseStart, timeDeviationCauseEmployee, templatePeriod, null, timeCode, null, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S11");
                    AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                    // Break handled, clear it
                    stampOutPaidBrakes.Remove(stampOutPaidBrake);
                }

                #endregion

                #region Out

                DateTime stop = start + (stampOut.Time - stampIn.Time);

                // TimeDeviationCause stop
                TimeDeviationCause deviationCauseStop = GetTimeDeviationCauseFromPrio(employee, employeeGroup, stampOut);
                if (deviationCauseStop == null)
                {
                    output.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCauseStop");
                    hasEntryError = true;
                    if (doLogAndRevertErrors)
                        LogAndRevertEntryStatus(output.Result, allTimeStampEntrysForEmployeeAndDate, employee, timeBlockDate, Level.Error, MethodBase.GetCurrentMethod().Name, "GetTimeDeviationCauseStopFromPrio");
                    break;
                }

                // TimeSpot terminals do not set the period when the stamp entry is saved
                if (stampOut.TimeScheduleTemplatePeriod == null)
                    stampOut.TimeScheduleTemplatePeriod = templatePeriod;

                // Fix stamp out before in
                // If stamp In occurs after schedule out, the temporary out time will be less than in.
                // In this case do not create a temporary block, wait until real stamp out occurs.
                if (stop < start)
                {
                    hasEntryError = true;
                    if (doLogAndRevertErrors)
                        RevertTimeStampEntryStatus(allTimeStampEntrysForEmployeeAndDate);
                    break;
                }

                #endregion

                EmployeeChild employeeChild = GetTimeStampEmployeeChild(timeStampEntry);
                ApplyTimeStampRounding(employeeGroup, timeDeviationCauseEmployee, scheduleIn, scheduleOut, ref start, ref stop, ref deviationCauseStart, ref deviationCauseStop, out TimeStampRounding rounding);
                ApplyTimeStampAccountInternals(stampIn, employee, ref accountInternals, defaultEmployeeAccountDimEmployeeAccountDimId, useAccountHierarchy);
                ApplyTimeStampShiftType(stampIn, useTimeScheduleTypeFromTime);

                #region Create TimeBlocks

                DateTime timeBlockStart;
                DateTime timeBlockStop;
                TimeBlock prevBlock = timeBlocksToAdd.OrderBy(i => i.StopTime).LastOrDefault(b => b.StopTime > scheduleIn);

                #region Create breaks

                // All holes between the timeblocks within schedule are filled with breaks
                // Check if we have any previous block(s) and if there is a hole between current block and previous
                if (prevBlock != null && prevBlock.StopTime < start)
                {
                    if (!prevBlock.TimeDeviationCauseStopReference.IsLoaded)
                        prevBlock.TimeDeviationCauseStopReference.Load();
                    if (!prevBlock.TimeDeviationCauseStop.TimeCodeReference.IsLoaded)
                        prevBlock.TimeDeviationCauseStop.TimeCodeReference.Load();

                    // Check if previously stamped out or currently stamped in with absence cause
                    if (prevBlock.TimeDeviationCauseStop != null && (prevBlock.TimeDeviationCauseStop.TimeCode is TimeCodeAbsense || deviationCauseStart.TimeCode is TimeCodeAbsense))
                    {
                        //Only add TimeBlock if has schedule for minute after previous block stops (i.e. not has schedule hole). Do better solution to check if in schedule hole?
                        TimeScheduleTemplateBlock scheduleBlock = scheduleBlocks.FirstOrDefault(i => !i.IsBreak && i.StartTime <= prevBlock.StopTime.AddMinutes(1) && i.StopTime >= prevBlock.StopTime.AddMinutes(1));
                        if (scheduleBlock != null)
                        {
                            TimeDeviationCause deviationCauseAbsence = prevBlock.TimeDeviationCauseStop.TimeCode is TimeCodeAbsense ? prevBlock.TimeDeviationCauseStop : deviationCauseStart;

                            timeBlockStart = prevBlock.StopTime;
                            timeBlockStop = (start > scheduleOut ? scheduleOut : start);

                            //Check if is before hole, then dont create excess during hole
                            if (scheduleBlock.IsStopBeforeScheduleHole(scheduleBlocks))
                                timeBlockStop = scheduleBlock.StopTime;

                            // Create absence TimeBlock
                            TimeBlock timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, null, employee, deviationCauseAbsence, deviationCauseAbsence, timeDeviationCauseEmployee, templatePeriod, scheduleBlocks.GetClosest(start, stop), deviationCauseAbsence.TimeCode, employeeChild: employeeChild, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S1");
                            AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);
                        }
                    }
                    else
                    {
                        foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks.Where(s => !s.IsBreak))
                        {
                            // Check that break is within schedule
                            if (prevBlock.StopTime >= scheduleBlock.StartTime && start <= scheduleBlock.StopTime && prevBlock.StopTime < start)
                            {
                                TimeScheduleTemplateBlock closestScheduleBreak = GetScheduleBlockClosestBreak(scheduleBlocks, prevBlock.StopTime, start);
                                if (closestScheduleBreak != null && !timeBlocksToAdd.Any(i => i.StartTime == prevBlock.StopTime && i.StopTime == start))
                                {
                                    if (scheduleBlock.IsScheduledAbsenceAroundBreak(closestScheduleBreak, scheduleIn, scheduleOut))
                                    {
                                        List<TimeBlock> timeBlockBreaks = CreateBreakWithSurroundedAbsence(scheduleBlock, closestScheduleBreak, prevBlock.StopTime, start, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, timeDeviationCauseEmployee);
                                        AddTimeBlocksToCollection(timeBlocksToAdd, timeBlockBreaks);
                                    }
                                    else
                                    {
                                        TimeBlock timeBlockBreak = CreateBreakTimeBlockFromTimeStamps(prevBlock.StopTime, start, employee, scheduleBlock, closestScheduleBreak, timeBlockDate, stampIn, stampOut, timeDeviationCauseEmployee.TimeDeviationCauseId, debugInfo: "B3D");
                                        AddTimeBlockToCollection(timeBlocksToAdd, timeBlockBreak);
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Create excess

                if (scheduleIn != scheduleOut)
                {
                    #region Outside schedule

                    if ((start < scheduleIn && stop < scheduleIn) || start > scheduleOut)
                    {
                        // Complete block outside schedule, add extra time block (presence)
                        TimeScheduleTemplateBlock scheduleBlock;
                        if (start < scheduleIn && stop < scheduleIn)
                            scheduleBlock = scheduleBlocks.Where(s => !s.IsBreak && s.StartTime > start).OrderBy(s => s.StartTime).FirstOrDefault();
                        else
                            scheduleBlock = scheduleBlocks.Where(s => !s.IsBreak && s.StopTime < stop).OrderByDescending(s => s.StopTime).FirstOrDefault();

                        timeBlockStart = start;
                        timeBlockStop = stop;

                        //Create TimeBlock
                        TimeBlock timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, null, employee, deviationCauseStart, deviationCauseStart, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S2", addTimeStampEntryExtendedDetails: true);
                        AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                        accountInternals.Clear();

                        stampIn = null;
                        stampOut = null; //NOSONAR

                        // Next entry
                        continue;
                    }

                    #endregion

                    #region Within schedule

                    #region First within schedule

                    // If this entry is the first today, compare the time with schedule in.
                    // If stamped in before schedule in, create an extra presence time block from stamp in to schedule in.
                    // If stamped in after schedule in, create an extra absence time block from schedule in to stamp in.
                    if (isFirstTimeStamp && !isValidDoubleStamp)
                    {
                        isFirstTimeStamp = false;

                        timeCode = GetTimeCodeFromDeviationCause(deviationCauseStart, defaultTimeCodeId);
                        if (start < scheduleIn && stop >= scheduleIn)
                        {
                            #region Stamped in before schedule in, add presence TimeBlock

                            // If cause was absence take standard (wrong stamped)
                            if (deviationCauseStart.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                            {
                                deviationCauseStart = timeDeviationCauseEmployee;
                                timeCode = GetTimeCodeFromDeviationCause(deviationCauseStart, defaultTimeCodeId);
                            }

                            TimeScheduleTemplateBlock scheduleBlock = scheduleBlocks.Where(s => !s.IsBreak && s.StartTime > start).OrderBy(s => s.StartTime).FirstOrDefault();

                            timeBlockStart = start;
                            timeBlockStop = scheduleIn;

                            //Create TimeBlock
                            TimeBlock timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, null, employee, deviationCauseStart, deviationCauseStart, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S3", addTimeStampEntryExtendedDetails: true);
                            AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                            bool isOvertimeInScheduleAsNotSchedule = deviationCauseStart.CandidateForOvertime && scheduleBlock?.TimeScheduleTypeId != null && GetTimeScheduleTypeFromCache(scheduleBlock.TimeScheduleTypeId.Value)?.IsNotScheduleTime == true;

                            // New start time on actual block
                            start = scheduleIn;
                            if (!isOvertimeInScheduleAsNotSchedule)
                                deviationCauseStart = timeDeviationCauseEmployee;

                            #endregion
                        }
                        else if (start > scheduleIn && start <= scheduleOut)
                        {
                            #region Stamped in after schedule in, add absence TimeBlock

                            //Check if is after hole, then dont create excess during hole
                            DateTime excessStopTime = start; //dont overwrite original stop
                            TimeDeviationCause excessTimeDeviationCause = deviationCauseStart; //dont overwrite original TimeDeviationCause
                            if (scheduleBlocks.IsStartAfterScheduleHole(start, out TimeScheduleTemplateBlock prevScheduleBlock))
                            {
                                excessStopTime = prevScheduleBlock.StopTime;
                                if (deviationCauseStart != null && prevScheduleBlock.TimeDeviationCauseId.HasValue && deviationCauseStart.TimeDeviationCauseId != prevScheduleBlock.TimeDeviationCauseId.Value)
                                    excessTimeDeviationCause = prevScheduleBlock.TimeDeviationCause;
                            }

                            TimeBlock timeBlockToAdd = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, scheduleIn, excessStopTime, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, excessTimeDeviationCause, employeeChild?.EmployeeChildId, "E1", defaultTimeCode: timeCode, connectToStamps: true, addAccountingFromStart: true);
                            AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                            if (timeBlockToAdd != null)
                                deviationCauseStart = timeDeviationCauseEmployee;

                            #endregion
                        }
                    }

                    #endregion

                    #region Last within schedule

                    // If this entry is the last today, compare the time with schedule out.
                    // If stamped out before schedule out, create an extra absence time block from stamp out to schedule out.
                    // If stamped out after schedule out, create an extra presence time block from schedule out to stamp out.
                    bool isLastWithinSchedule = timeStampEntrysForEmployeeAndDate.Count(e => CalendarUtility.GetScheduleTime(e.Time, timeBlockDate.Date, e.Time.Date) <= scheduleOut) == entryCounter;
                    if (isLastWithinSchedule)
                    {
                        timeCode = GetTimeCodeFromDeviationCause(deviationCauseStop, defaultTimeCodeId);
                        if (stop >= scheduleIn && stop < scheduleOut)
                        {
                            #region Stamped out before schedule out, add absence TimeBlock

                            //Check if is before hole, then dont create excess during hole
                            DateTime excessStartTime = stop; //dont overwrite original stopc
                            TimeDeviationCause excessTimeDeviationCause = deviationCauseStop; //dont overwrite original TimeDeviationCause
                            if (scheduleBlocks.IsStopBeforeScheduleHole(stop, out TimeScheduleTemplateBlock nextScheduleBlock))
                            {
                                excessStartTime = nextScheduleBlock.StartTime;
                                if (deviationCauseStart != null && deviationCauseStart.TimeDeviationCauseId != nextScheduleBlock.TimeDeviationCauseId)
                                    excessTimeDeviationCause = nextScheduleBlock.TimeDeviationCause;
                            }

                            //Create excess TimeBlock
                            TimeBlock timeBlockToAdd = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, excessStartTime, scheduleOut, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, excessTimeDeviationCause, employeeChild?.EmployeeChildId, "E2", defaultTimeCode: timeCode, connectToStamps: true, addAccountingFromStop: true);
                            AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                            if (isValidDoubleStamp && timeBlockToAdd != null && stampIn.Time == timeStampEntrysForEmployeeAndDate.Last().Time)
                            {
                                TimeBlock prevTimeBlock = timeBlocksToAdd.Where(i => i.StopTime < timeBlockToAdd.StartTime).OrderByDescending(i => i.StopTime).FirstOrDefault();
                                if (prevTimeBlock != null && !timeBlocksToAdd.Any(tb => tb.StartTime == prevTimeBlock.StopTime && tb.StopTime == timeBlockToAdd.StartTime))
                                {
                                    TimeScheduleTemplateBlock scheduleBlock = scheduleBlocks.GetClosest(prevTimeBlock.StopTime, timeBlockToAdd.StartTime);
                                    if (scheduleBlock != null)
                                    {
                                        //Create excess TimeBlock
                                        TimeBlock excessTimeBlockInHole = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, prevTimeBlock.StopTime, timeBlockToAdd.StartTime, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, excessTimeDeviationCause, employeeChild?.EmployeeChildId, "E9", defaultTimeCode: timeCode, connectToStamps: true, addAccountingFromClosest: true);
                                        AddTimeBlockToCollection(timeBlocksToAdd, excessTimeBlockInHole);
                                    }
                                }
                            }

                            #endregion
                        }
                        else if (start <= scheduleOut && stop > scheduleOut)
                        {
                            #region Stamped out after schedule out, add presence TimeBlock

                            // If cause was absence take standard (wrong stamped)
                            if (deviationCauseStop.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                            {
                                deviationCauseStop = timeDeviationCauseEmployee;
                                timeCode = GetTimeCodeFromDeviationCause(deviationCauseStop, defaultTimeCodeId);
                            }

                            TimeScheduleTemplateBlock scheduleBlock = scheduleBlocks.Where(s => !s.IsBreak && s.StopTime < stop).OrderByDescending(s => s.StopTime).FirstOrDefault();

                            timeBlockStart = scheduleOut;
                            timeBlockStop = stop;

                            TimeBlock timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, null, employee, deviationCauseStop, deviationCauseStop, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S5", addTimeStampEntryExtendedDetails: true);
                            AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                            // New stop time on actual block
                            stop = scheduleOut;

                            #endregion
                        }

                        if (start > scheduleIn && stop <= scheduleOut && timeStampEntrysForEmployeeAndDate.IsPrevTimeStampOnOrBefore(stampIn, TimeStampEntryType.Out, CalendarUtility.GetDateTime(timeBlockDate.Date, scheduleIn), out TimeStampEntry prevTimeStamp))
                        {
                            #region Stamped in within schedule and prev stamp out is before or on schedule in, add absence TimeBlock

                            //Create excess TimeBlock
                            TimeBlock excessTimeBlockInHole = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, scheduleIn, start, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, prevTimeStamp.TimeDeviationCause, employeeChild?.EmployeeChildId, "E10", defaultTimeCode: timeCode, connectToStamps: true, addAccountingFromClosest: true);
                            AddTimeBlockToCollection(timeBlocksToAdd, excessTimeBlockInHole);

                            #endregion
                        }
                    }

                    #endregion

                    #endregion
                }

                #endregion

                #region Accounting from prio

                ApplyAccountingPrioOnTimeBlocks(timeBlocksToAdd, employee, overwriteAccountStd: true);

                #endregion

                #region Split according to schedule (different accounting)

                timeCode = GetTimeCodeFromCache(defaultTimeCodeId);
                bool getNextAccounting = false;

                foreach (TimeScheduleTemplateBlock scheduleBlock in scheduleBlocks.Where(s => !s.IsBreak).OrderBy(s => s.StartTime))
                {
                    TimeBlock timeBlockToAdd = null;

                    bool isStartAfterScheduleHole = scheduleBlock.IsStartAfterScheduleHole(scheduleBlocks);
                    bool isStopBeforeScheduleHole = scheduleBlock.IsStopBeforeScheduleHole(scheduleBlocks);

                    DateTime? scheduleStartAfterHole = isStartAfterScheduleHole ? scheduleBlock.StartTime : (DateTime?)null;

                    if (prevBlock != null && prevBlock.StopTime < scheduleBlock.StopTime && start > scheduleBlock.StopTime)
                    {
                        #region Has previous block

                        if (scheduleOut > scheduleBlock.StopTime && !isStopBeforeScheduleHole)
                        {
                            TimeScheduleTemplateBlock closestScheduleBreak = GetScheduleBlockClosestBreak(scheduleBlocks, prevBlock.StopTime, start);
                            if (closestScheduleBreak != null)
                            {
                                TimeBlock timeBlockFillBreak = CreateBreakTimeBlockFromTimeStamps(prevBlock.StopTime, scheduleBlock.StopTime, employee, scheduleBlock, closestScheduleBreak, timeBlockDate, stampIn, stampOut, timeDeviationCauseEmployee.TimeDeviationCauseId, existingTimeBlocks: timeBlocksToAdd, debugInfo: "B1");
                                if (AddTimeBlockToCollection(timeBlocksToAdd, timeBlockFillBreak) && scheduleBlock.StopTime < start)
                                {
                                    TimeBlock timeBlockHoleBreak = CreateBreakTimeBlockFromTimeStamps(scheduleBlock.StopTime, start, employee, scheduleBlock, closestScheduleBreak, timeBlockDate, stampIn, stampOut, timeDeviationCauseEmployee.TimeDeviationCauseId, existingTimeBlocks: timeBlocksToAdd, debugInfo: "B2");
                                    AddTimeBlockToCollection(timeBlocksToAdd, timeBlockHoleBreak);
                                }
                            }
                        }
                        else
                        {
                            //Do not create block if any block was already created with same starttime
                            if (!timeBlocksToAdd.Any(i => i.StartTime < i.StopTime && i.StartTime == prevBlock.StopTime))
                            {
                                // Create excess TimeBlock. Occurs when there is a hole in the schedule and the user stamps out before first schedule block ends
                                timeBlockToAdd = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, prevBlock.StopTime, scheduleBlock.StopTime, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, deviationCauseStart, employeeChild?.EmployeeChildId, "E3", addAccountingFromClosest: true);
                                AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);
                            }
                        }

                        #endregion
                    }

                    // Schedule is splitted within current block, split current block and apply correct accounting
                    if (start < scheduleBlock.StopTime)
                    {
                        #region Start before schedule stops

                        // Create additional block with accounting from schedule
                        getNextAccounting = true;
                        if (stop >= scheduleBlock.StopTime)
                        {
                            if (isStartAfterScheduleHole)
                            {
                                #region Schedule after schedule hole

                                if (start < scheduleBlock.StartTime)
                                {
                                    #region Start before schedule in inside hole

                                    timeBlockStart = start;
                                    timeBlockStop = scheduleBlock.StartTime;

                                    //Create TimeBlock
                                    timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, scheduleStartAfterHole, employee, deviationCauseStart, deviationCauseStart, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S7", addTimeStampEntryExtendedDetails: true);
                                    AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                                    // Move start time on actual block
                                    start = scheduleBlock.StartTime;

                                    #endregion
                                }
                                else if (start > scheduleBlock.StartTime)
                                {
                                    #region Start after schedule after hole

                                    timeBlockStart = scheduleBlock.StartTime;
                                    timeBlockStop = start;

                                    //Only add excess TimeBlock if no block with same starttime exists
                                    bool hasMatchingBlock = timeBlocksToAdd.Any(i => i.StartTime == timeBlockStart);
                                    if (!hasMatchingBlock)
                                    {
                                        //Create TimeBlock
                                        timeBlockToAdd = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, GetTimeDeviationCauseFromPrio(employee, employeeGroup, stampIn), employeeChild?.EmployeeChildId, "E4", addAccountingFromClosest: true);
                                        AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);
                                    }

                                    #endregion
                                }

                                #endregion
                            }

                            #region Stop at or after schedule start

                            timeBlockStart = start;
                            timeBlockStop = scheduleBlock.StopTime;

                            //Fix for when stamping in less than rounding minutes before scheduleout and then stamp out on scheduleout (ex: 17:12-17:15 when scheduleout is 17:15). Resulted earlier in overlapping timeblocks
                            bool isTimeBeforeScheduleOutLesseThanRounding = timeBlockStop == scheduleOut && timeBlockStop.Subtract(timeBlockStart).TotalMinutes < rounding?.RoundOutNeg && timeBlocksToAdd.Any(a => CalendarUtility.GetOverlappingMinutes(timeBlockStart, timeBlockStop, a.StartTime, a.StopTime) > 0);
                            if (!isTimeBeforeScheduleOutLesseThanRounding)
                            {
                                // Create TimeBlock
                                timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, scheduleStartAfterHole, employee, deviationCauseStart, deviationCauseStop, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S8", addTimeStampEntryExtendedDetails: true);
                                AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);
                            }

                            // Move start time on actual block
                            start = scheduleBlock.StopTime;

                            #endregion
                        }
                        else if (start < scheduleBlock.StartTime && stop > scheduleBlock.StartTime)
                        {
                            #region Start before schedule start and stop after schedule start

                            timeBlockStart = start;
                            timeBlockStop = scheduleBlock.StartTime;

                            //Create TimeBlock
                            if (isStartAfterScheduleHole)
                            {
                                SoeTimeRuleType type = timeBlockStart > scheduleBlock.StartTime ? SoeTimeRuleType.Absence : SoeTimeRuleType.Presence;
                                timeBlockToAdd = CreateExcessTimeBlockFromStamping(type, timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, GetTimeDeviationCauseFromPrio(employee, employeeGroup, stampIn), employeeChild?.EmployeeChildId, "E5", addAccountingFromClosest: true);
                            }
                            else
                                timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, scheduleStartAfterHole, employee, deviationCauseStart, deviationCauseStop, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S9", addTimeStampEntryExtendedDetails: true);
                            AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                            // Move start time on actual block
                            start = scheduleBlock.StartTime;

                            if (start < stop)
                            {
                                timeBlockStart = start;
                                timeBlockStop = stop;

                                //Create TimeBlock with start and stop cause
                                timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, scheduleStartAfterHole, employee, deviationCauseStart, deviationCauseStop, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S10", addTimeStampEntryExtendedDetails: true);
                                AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                                start = stop;
                            }

                            #endregion
                        }
                        else
                        {
                            if (start > scheduleBlock.StartTime && (prevBlock?.StartTime < scheduleBlock.StartTime || isStartAfterScheduleHole))
                            {
                                #region Start after schedule and has previous added block before current schedule

                                timeBlockStart = scheduleBlock.StartTime;
                                timeBlockStop = start;

                                //Only add excess TimeBlock if no timeblock with same time exists
                                if (!timeBlocksToAdd.Any(i => i.StopTime == timeBlockStop))
                                {
                                    // Occurs when there is a whole in the schedule and the user stamps in after last schedule block starts
                                    timeBlockToAdd = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, deviationCauseStart, employeeChild?.EmployeeChildId, "E6", addAccountingFromClosest: true);
                                    AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);
                                }

                                #endregion
                            }
                            else if (stop < scheduleBlock.StopTime && isStopBeforeScheduleHole)
                            {
                                #region Stop before schedule stops and before hole

                                //Only create absence if no other stamp in exists before scheduleblock stops
                                if (!timeStampEntrysForEmployeeAndDate.HasAnyTimeStamp(stop, scheduleBlock.StopTime, TimeStampEntryType.In))
                                {
                                    timeBlockStart = stop;
                                    timeBlockStop = scheduleBlock.StopTime;
                                    timeBlockToAdd = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, deviationCauseStop, employeeChild?.EmployeeChildId, "E7", addAccountingFromClosest: true);
                                    AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);
                                }

                                #endregion
                            }
                            else if (start > scheduleBlock.StartTime && isStartAfterScheduleHole)
                            {
                                #region Start after schedule starts and after hole

                                timeBlockStart = scheduleBlock.StartTime;
                                timeBlockStop = start;

                                timeBlockToAdd = CreateExcessTimeBlockFromStamping(SoeTimeRuleType.Absence, timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, employee, employeeGroup, templatePeriod, scheduleBlocks, GetTimeDeviationCauseFromPrio(employee, employeeGroup, stampIn), employeeChild?.EmployeeChildId, "E8", addAccountingFromClosest: true);
                                AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                                #endregion
                            }

                            #region Rest

                            //Handle stamp out after schedule out in hole (if yes, use stop cause)
                            bool useStopAsStartCause = isStartAfterScheduleHole && start < scheduleBlock.StartTime;

                            timeBlockStart = start;
                            timeBlockStop = stop;

                            //Create TimeBlock with start and stop cause
                            timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, scheduleStartAfterHole, employee, (useStopAsStartCause ? deviationCauseStop : deviationCauseStart), deviationCauseStop, timeDeviationCauseEmployee, templatePeriod, scheduleBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S6", addTimeStampEntryExtendedDetails: true);
                            AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);

                            // Move start time on actual block
                            start = stop;

                            #endregion
                        }

                        #endregion
                    }
                    else if (getNextAccounting)
                    {
                        #region GetNextAccounting

                        if (!scheduleBlock.AccountInternal.IsLoaded)
                            scheduleBlock.AccountInternal.Load();

                        if (scheduleBlock.AccountInternal != null)
                        {
                            foreach (var accInt in scheduleBlock.AccountInternal)
                            {
                                if (!accInt.AccountReference.IsLoaded)
                                    accInt.AccountReference.Load();

                                if (!accountInternals.Select(a => a.Account.AccountDimId).Contains(accInt.Account.AccountDimId))
                                    accountInternals.Add(accInt);
                            }
                        }

                        getNextAccounting = false;

                        #endregion
                    }
                }

                #endregion

                #region Last block

                // Create presence block. 
                if (start != stop)
                {
                    timeCode = GetTimeCodeFromDeviationCause(deviationCauseStop, defaultTimeCodeId);

                    //Find closest schedule block to get accounting from
                    TimeScheduleTemplateBlock timeScheduleTemplateBlock = scheduleBlocks.GetClosest(start, stop);

                    timeBlockStart = start;
                    timeBlockStop = stop;

                    TimeBlock timeBlockToAdd = CreateTimeBlockFromStamping(timeBlockStart, timeBlockStop, timeBlockDate, stampIn, stampOut, scheduleOut, null, employee, deviationCauseStart, deviationCauseStop, timeDeviationCauseEmployee, templatePeriod, timeScheduleTemplateBlock, timeCode, employeeChild, accountInternals, timeScheduleTypeId: stampIn.TimeScheduleTypeId, debugInfo: "S4", addTimeStampEntryExtendedDetails: true);
                    AddTimeBlockToCollection(timeBlocksToAdd, timeBlockToAdd);
                }

                #endregion

                #region Accounting from TemplateBlocks (on TimeBlocks without accounting)

                ApplyAccountingOnTimeBlockFromTemplateBlockIfMissing(timeBlocksToAdd, null, employee);

                #endregion

                #endregion

                accountInternals.Clear();
                stampIn = null;
                stampOut = null; //NOSONAR
            }

            // Detach schedule blocks to prevent them from being saved
            DetachSchedule(scheduleBlocks);
            AdjustTimeBlockAccordingToPlannedAbsence(defaultTimeCodeId, timeDeviationCauses, employee, timeStampEntrysForEmployeeAndDate, ref templatePeriod, ref timeBlocksToAdd);

            return !hasEntryError;
        }

        private ActionResult SaveExpenseFromTimeStampExtended(Employee employee, TimeBlockDate timeBlockDate, List<TimeStampEntryExtended> timeStampEntryExtendeds)
        {
            foreach (TimeStampEntryExtended ext in timeStampEntryExtendeds.Where(w => w.TimeCodeId.HasValue))
            {
                ExpenseRowDTO expenseRowDTO = entities.ExpenseRow.FirstOrDefault(f => f.TimeStampEntryExtendedId == ext.TimeStampEntryExtendedId && f.State == (int)SoeEntityState.Active)?.ToDTO() ?? new ExpenseRowDTO();

                if (ext.TimeStampEntry.State == (int)SoeEntityState.Deleted)
                {
                    DeleteExpenseRow(expenseRowDTO.ExpenseRowId, true);
                }
                else
                {
                    expenseRowDTO.ActorCompanyId = ActorCompanyId;
                    expenseRowDTO.EmployeeId = employee.EmployeeId;
                    expenseRowDTO.Quantity = ext.Quantity ?? 0;
                    expenseRowDTO.TimeCodeId = ext.TimeCodeId.Value;
                    expenseRowDTO.Start = timeBlockDate.Date;
                    expenseRowDTO.Stop = timeBlockDate.Date;
                    expenseRowDTO.StandOnDate = timeBlockDate.Date;

                    ActionResult result = SaveExpense(expenseRowDTO, null, ext.TimeStampEntryExtendedId, false);
                    if (!result.Success)
                        return result;
                }
            }

            return new ActionResult(true);
        }

        private void ApplyTimeStampRounding(EmployeeGroup employeeGroup, TimeDeviationCause deviationCauseEmployee, DateTime scheduleIn, DateTime scheduleOut, ref DateTime start, ref DateTime stop, ref TimeDeviationCause deviationCauseStart, ref TimeDeviationCause deviationCauseStop, out TimeStampRounding rounding)
        {
            if (employeeGroup != null && !employeeGroup.TimeStampRounding.IsLoaded)
                employeeGroup.TimeStampRounding.Load();

            rounding = employeeGroup?.TimeStampRounding?.FirstOrDefault();
            if (rounding != null && rounding.HasRounding())
            {
                // Stamp in before schedule in
                if (rounding.RoundInNeg != 0 && start < scheduleIn && start.AddMinutes(rounding.RoundInNeg) >= scheduleIn)
                {
                    start = scheduleIn;
                    deviationCauseStart = deviationCauseEmployee;
                }
                // Stamp in after schedule in
                if (rounding.RoundInPos != 0 && start > scheduleIn && start.AddMinutes(-rounding.RoundInPos) <= scheduleIn)
                {
                    start = scheduleIn;
                    deviationCauseStart = deviationCauseEmployee;
                }
                // Stamp out before schedule out
                if (rounding.RoundOutNeg != 0 && stop < scheduleOut && stop.AddMinutes(rounding.RoundOutNeg) >= scheduleOut)
                {
                    stop = scheduleOut;
                    deviationCauseStop = deviationCauseEmployee;
                }
                // Stamp out after schedule out
                if (rounding.RoundOutPos != 0 && stop > scheduleOut && stop.AddMinutes(-rounding.RoundOutPos) <= scheduleOut)
                {
                    stop = scheduleOut;
                    deviationCauseStop = deviationCauseEmployee;
                }
            }
        }

        private void ApplyTimeStampShiftType(TimeStampEntry timeStampEntry, bool useTimeScheduleTypeFromTime)
        {
            if (timeStampEntry == null)
                return;

            if (useTimeScheduleTypeFromTime && timeStampEntry.AccountId.HasValue)
            {
                int? timeScheduleTypeId = timeStampEntry.TimeScheduleTypeId;
                TimeStampManager.SetShiftTypeAndTimeScheduleType(entities, timeStampEntry, actorCompanyId);

                // Revert TimeScheduleType if it was set from the beginning
                if (timeScheduleTypeId.HasValue && timeScheduleTypeId.Value != timeStampEntry.TimeScheduleTypeId)
                    timeStampEntry.TimeScheduleTypeId = timeScheduleTypeId.Value;
            }
        }

        private void ApplyTimeStampAccountInternals(TimeStampEntry timeStampEntry, Employee currentEmployee, ref List<AccountInternal> accountInternals, int defaultEmployeeAccountDimEmployeeAccountDimId, bool useAccountHierarchy)
        {
            if (timeStampEntry == null)
                return;

            var accountInternalsOnTimeStamp = GetTimeStampAccountInternals(timeStampEntry, useAccountHierarchy);
            var accountInternal = accountInternalsOnTimeStamp.FirstOrDefault();
            bool useEmployeeAccountInPrio = GetCompanyBoolSettingFromCache(CompanySettingType.FallbackOnEmployeeAccountInPrio);
            if (useEmployeeAccountInPrio)
            {
                // we should change this so it works in the manner for all customers
                accountInternals = accountInternalsOnTimeStamp.GroupBy(g => g.Account.AccountDimId).Select(s => s.FirstOrDefault()).ToList();
                var prio = GetAccountingPrioByEmployeeFromCache(timeStampEntry.TimeBlockDate.Date, currentEmployee);

                if (prio != null)
                {
                    foreach (var account in prio.AccountInternals)
                    {
                        foreach (var ai in accountInternalsOnTimeStamp)
                        {
                            if (accountInternals.Any(s => ai.Account.AccountDimId == account.AccountDimId))
                                continue;

                            TryAddAccountInternal(ref accountInternals, account.AccountId, replaceOnSameDim: true, discardState: true);
                        }
                    }
                }
            }
            else if (accountInternal?.Account != null)
            {
                if (useAccountHierarchy && (timeStampEntry.AccountId.HasValue || timeStampEntry.TimeTerminalAccountId.HasValue))
                {
                    //Add accounts from ShiftType
                    if (timeStampEntry.AccountId.HasValue)
                    {
                        ShiftType shiftType = GetShiftTypeByAccountsWithAccountsFromCache(timeStampEntry.AccountId.Value);
                        if (shiftType?.AccountInternal != null)
                            accountInternals.AddRange(shiftType.AccountInternal);
                    }

                    //Add accounts from TimeTerminal
                    if (timeStampEntry.TimeTerminalAccountId.HasValue)
                        TryAddAccountInternal(ref accountInternals, timeStampEntry.TimeTerminalAccountId.Value, replaceOnSameDim: true, discardState: true);

                    //Add accounts from EmployeeAccounts
                    if (!accountInternals.Any(s => s.Account.AccountDimId == defaultEmployeeAccountDimEmployeeAccountDimId))
                    {
                        List<EmployeeAccount> employeeAccountsForDate = currentEmployee.EmployeeAccount.GetEmployeeAccounts(timeStampEntry.Time.Date);
                        foreach (Account accountForDate in employeeAccountsForDate.Select(a => a.Account))
                        {
                            if (accountInternals.Any(s => s.Account.AccountDimId == defaultEmployeeAccountDimEmployeeAccountDimId))
                                continue;

                            accountInternals.Add(accountForDate.AccountInternal);
                            if (accountForDate.ParentAccountId.HasValue)
                                TryAddAccountInternal(ref accountInternals, accountForDate.ParentAccountId.Value, replaceOnSameDim: true, discardState: true);
                        }
                    }
                }
                else
                {
                    List<AccountInternal> accountInternalsForDim = accountInternals.Where(a => a.Account?.AccountDimId == accountInternal.Account.AccountDimId).ToList();
                    if (accountInternalsForDim.IsNullOrEmpty())
                    {
                        accountInternals.Add(accountInternal);
                    }
                    else
                    {
                        accountInternals.Remove(accountInternalsForDim.FirstOrDefault());
                        accountInternals.Add(accountInternal);
                    }
                }
            }

            if (TimeStampManager.HasExtendedTimeStamps(entities, ActorCompanyId) && !timeStampEntry.TimeStampEntryExtended.IsNullOrEmpty())
            {
                var accountIds = timeStampEntry.TimeStampEntryExtended.Where(w => w.AccountId.HasValue).Select(s => s.AccountId.Value).ToList();

                foreach (var accountId in accountIds)
                {
                    TryAddAccountInternal(ref accountInternals, accountId, replaceOnSameDim: true, discardState: true);
                }
            }
        }

        private void AddTimeStampEntryExtendedDetailsToTimeBlock(TimeStampEntry timeStamp, TimeBlock timeBlock)
        {
            if (!timeStamp.TimeStampEntryExtended.IsNullOrEmpty())
            {
                var extendedDetails = timeStamp.TimeStampEntryExtended.Where(t => (t.TimeScheduleTypeId.HasValue || t.AccountId.HasValue || t.TimeCodeId.HasValue) && t.State == (int)SoeEntityState.Active).ToExtendedDetailsDTOs();
                if (extendedDetails.Any())
                    timeBlock.TimeStampEntryExtendedDetails = JsonConvert.SerializeObject(extendedDetails);
            }
        }

        private void ApplySaveDeviationsFromTimeStamps(
            SaveTimeStampsOutputDTO oDTO, 
            TimeBlockDate timeBlockDate, 
            Employee currentEmployee, 
            List<TimeStampEntry> allEntrysForEmployeeAndDate, 
            List<TimeBlock> timeBlocksBefore, 
            List<TimeBlock> timeBlocksToAdd, 
            bool? discardBreakEvaluation, 
            int? preservedAttestStateId
            )
        {
            try
            {
                List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesFromCache();
                List<int> timeDeviationCausesOvertime = timeDeviationCauses.GetOvertimeDeviationCauseIds();

                AddDaysToRestoreDayTrackerBasedOnDeviationCauseChanged(timeBlockDate.Date, currentEmployee.EmployeeId, timeBlocksBefore, timeBlocksToAdd, timeDeviationCauses);
                SetCommentOnTimeBlockFromTimeStamps(timeBlocksToAdd);
                CheckDuplicateTimeBlocks(timeBlocksToAdd, currentEmployee, timeBlockDate, "SaveDeviationsFromTimeStamps");

                oDTO.Result = SaveTimeBlocks(out List<TimeEngineDay> days, timeBlocksToAdd);
                if (oDTO.Result.Success)
                {
                    oDTO.Result = SaveTransactionsForPeriods(days, discardBreakEvaluation: discardBreakEvaluation);
                    if (oDTO.Result.Success)
                    {
                        oDTO.Result = SaveTimeStampEntryStatus(allEntrysForEmployeeAndDate, TermGroup_TimeStampEntryStatus.Processed);
                        if (oDTO.Result.Success)
                        {
                            oDTO.Result = SaveTimeBlockDateStampingStatus(timeBlockDate, TermGroup_TimeBlockDateStampingStatus.Complete);
                            if (oDTO.Result.Success && timeBlocksToAdd.ContainsTimeDeviationCause(timeDeviationCausesOvertime))
                                oDTO.AddOvertimeTimeBlockDateId(timeBlockDate.TimeBlockDateId);
                            if (oDTO.Result.Success && preservedAttestStateId.HasValue)
                                oDTO.Result = RestorePreservedAttestState(timeBlockDate.EmployeeId, timeBlockDate.Date, preservedAttestStateId.Value);
                        }
                    }
                }

                timeBlocksToAdd.Clear();
            }
            catch (Exception ex)
            {
                oDTO.Result.Exception = ex;
                LogError(GetTimeStampsErrorMessage(currentEmployee, timeBlockDate));
                LogError(ex);
            }
        }

        private bool IsDayAttested(AttestState attestedAttestState, int employeeId, int timeBlockDateId)
        {
            if (attestedAttestState == null)
                return false;

            List<TimePayrollTransaction> transactions = GetTimePayrollTransactionsWithAttestState(employeeId, timeBlockDateId).Where(t => !t.IsExcludedInTime()).ToList();
            if (transactions.IsNullOrEmpty())
                return false;

            // If any transaction has a state less than attested, they day will not count as attested.
            // All transactions must have state attested or higher for the day to count as attested.

            return !transactions.Any(t => t.AttestState.Sort < attestedAttestState.Sort);
        }

        #endregion

        #region Planned absence

        private void AdjustTimeBlockAccordingToPlannedAbsence(int defaultTimeCodeId, List<TimeDeviationCause> timeDeviationCauses, Employee currentEmployee, List<TimeStampEntry> activeEntrysForEmployeeAndDate, ref TimeScheduleTemplatePeriod templatePeriod, ref List<TimeBlock> timeBlocksToAdd)
        {
            if (templatePeriod == null || timeDeviationCauses == null || !timeDeviationCauses.Any(i => i.ChangeDeviationCauseAccordingToPlannedAbsence))
                return;

            foreach (var entryGroup in activeEntrysForEmployeeAndDate.OrderBy(e => e.Time).ThenBy(i => i.TimeStampEntryId).GroupBy(g => g.TimeBlockDate.Date))
            {
                TimeStampEntry firstEntry = entryGroup.First();
                if (firstEntry.TimeBlockDate == null)
                    continue;

                templatePeriod = GetTimeScheduleTemplatePeriodFromCache(currentEmployee.EmployeeId, firstEntry.TimeBlockDate.Date);
                if (templatePeriod == null)
                    continue;

                List<TimeScheduleTemplateBlock> scheduleBlocksForPlannedAbsence = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(null, currentEmployee.EmployeeId, firstEntry.TimeBlockDate.Date);
                if (!AdjustTimeBlockAccordingToPlannedAbsence(scheduleBlocksForPlannedAbsence, entryGroup.ToList(), defaultTimeCodeId))
                    continue;

                timeBlocksToAdd = timeBlocksToAdd.Where(tb => tb.State != (int)SoeEntityState.Deleted).ToList();

                foreach (TimeStampEntry timeStampEntry in entryGroup.Where(tse => tse.State != (int)SoeEntityState.Deleted && tse.TimeBlock != null))
                {
                    foreach (TimeBlock timeBlock in timeStampEntry.TimeBlock.Where(tb => tb.State != (int)SoeEntityState.Deleted))
                    {
                        if (timeBlock.TimeBlockId == 0 && !timeBlocksToAdd.Contains(timeBlock))
                        {
                            if (timeBlocksToAdd.IsNewOverlappedByCurrent(timeBlock.StartTime, timeBlock.StopTime))
                                timeBlock.State = (int)SoeEntityState.Deleted;
                            else
                                AddTimeBlockToCollection(timeBlocksToAdd, timeBlock);
                        }

                    }
                }
            }
        }

        private bool AdjustTimeBlockAccordingToPlannedAbsence(List<TimeScheduleTemplateBlock> timeScheduleTemplateBlocks, List<TimeStampEntry> timeStampEntries, int defaultTimeCodeId)
        {
            bool hasChangeDeviationCauseAccordingToPlannedAbsence = false;

            if (!timeScheduleTemplateBlocks.Any(a => a.TimeDeviationCauseId.HasValue))
                return hasChangeDeviationCauseAccordingToPlannedAbsence;

            List<DateTime> handledTimes = new List<DateTime>();

            var absenceIntervals = timeScheduleTemplateBlocks.GetPlannedAbsenceIntervals();

            foreach (var absenceInterval in absenceIntervals.OrderBy(o => o.ActualStartTime))
            {
                TimeDeviationCause deviationCause = GetTimeDeviationCauseFromCache(absenceInterval.TimeDeviationCauseId);
                if (deviationCause == null || !deviationCause.ChangeDeviationCauseAccordingToPlannedAbsence)
                    continue;

                TimeCode timeCode = GetTimeCodeFromDeviationCause(deviationCause, defaultTimeCodeId);
                if (timeCode == null)
                    continue;

                hasChangeDeviationCauseAccordingToPlannedAbsence = true;
                TimeStampEntry onTimeTimeStamp = timeStampEntries.FirstOrDefault(a => a.Time == absenceInterval.ActualStartTime || a.Time == absenceInterval.ActualStopTime);

                if (onTimeTimeStamp != null && !timeScheduleTemplateBlocks.Where(t => !t.IsBreak).All(t => t.TimeDeviationCauseId.HasValue)) //Check if no adjustment of timeblock is needed. Only adjustment of timedeviation
                {
                    if (deviationCause.AdjustTimeInsideOfPlannedAbsence > 0 && onTimeTimeStamp.Time == absenceInterval.ActualStopTime)
                    {
                        handledTimes.Add(absenceInterval.ActualStopTime);
                        if (absenceInterval.TimeScheduleTemplateBlocks.Count == 1 && absenceIntervals.Count == 1)
                            onTimeTimeStamp.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, absenceInterval.ActualStartTime, absenceInterval.ActualStopTime, timeCode, startsWith: false);
                        else
                            SplitTimeBlocksAccordingToPlannedAbsenceTimeScheduleBlocks(onTimeTimeStamp, absenceInterval, timeCode, timeScheduleTemplateBlocks);
                        continue;
                    }
                    else if (deviationCause.AdjustTimeInsideOfPlannedAbsence > 0 && onTimeTimeStamp.Time == absenceInterval.ActualStartTime)
                    {
                        handledTimes.Add(absenceInterval.ActualStartTime);
                        if (absenceInterval.TimeScheduleTemplateBlocks.Count == 1 && absenceIntervals.Count == 1)
                            onTimeTimeStamp.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, absenceInterval.ActualStartTime, absenceInterval.ActualStopTime, timeCode, startsWith: true);
                        else
                            SplitTimeBlocksAccordingToPlannedAbsenceTimeScheduleBlocks(onTimeTimeStamp, absenceInterval, timeCode, timeScheduleTemplateBlocks);
                        continue;
                    }
                }

                if (timeStampEntries.Any(a => a.Time >= absenceInterval.ActualStartTime && a.Time <= absenceInterval.ActualStopTime)) //TimeStamp inside plannedAbsence
                {
                    bool hasScheduleTimeAfterAbsenceInterval = timeScheduleTemplateBlocks.Any(a => !a.TimeDeviationCauseId.HasValue && a.ActualStartTime >= absenceInterval.ActualStopTime);
                    bool hasScheduleTimeBeforeAbsenceInterval = timeScheduleTemplateBlocks.Any(a => !a.TimeDeviationCauseId.HasValue && a.ActualStartTime < absenceInterval.ActualStartTime);

                    if (hasScheduleTimeAfterAbsenceInterval && hasScheduleTimeBeforeAbsenceInterval)
                        continue;

                    TimeStampEntry timeStampEntryFirst = timeStampEntries.FirstOrDefault(a => a.Time >= absenceInterval.ActualStartTime && a.Time <= absenceInterval.ActualStopTime);

                    DateTime validStartTimeStartOfDay = absenceInterval.ActualStartTime;
                    DateTime validStopTimeStartOfDay = absenceInterval.ActualStartTime.AddMinutes(deviationCause.ChangeCauseInsideOfPlannedAbsence);

                    if (hasScheduleTimeBeforeAbsenceInterval && timeStampEntryFirst.Type == (int)TimeStampEntryType.Out && CalendarUtility.IsDateInRange(timeStampEntryFirst.Time, validStartTimeStartOfDay, validStopTimeStartOfDay))
                    {
                        if (validStopTimeStartOfDay > absenceInterval.ActualStopTime)
                            validStopTimeStartOfDay = absenceInterval.ActualStopTime;

                        var validAdjustStopTime = validStartTimeStartOfDay.AddMinutes(deviationCause.AdjustTimeInsideOfPlannedAbsence);

                        if (deviationCause.AdjustTimeInsideOfPlannedAbsence > 0 && timeStampEntryFirst.Time <= validAdjustStopTime)
                        {
                            timeStampEntryFirst.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, validStartTimeStartOfDay, validStopTimeStartOfDay, timeCode);
                            SplitTimeBlocksAccordingToPlannedAbsenceTimeScheduleBlocks(timeStampEntryFirst, absenceInterval, timeCode, timeScheduleTemplateBlocks);
                        }
                        else
                        {
                            timeStampEntryFirst.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, timeStampEntryFirst.Time, timeCode);
                        }
                    }
                    else if (hasScheduleTimeAfterAbsenceInterval)
                    {
                        TimeStampEntry timeStampEntryLast = timeStampEntries.Last(a => a.Time >= absenceInterval.ActualStartTime && a.Time <= absenceInterval.ActualStopTime);
                        DateTime validStartTimeEndOfDay = absenceInterval.ActualStopTime.AddMinutes(-deviationCause.ChangeCauseInsideOfPlannedAbsence);
                        DateTime validStopTimeEndOfDay = absenceInterval.ActualStopTime;

                        if (timeStampEntryLast.Type == (int)TimeStampEntryType.In && CalendarUtility.IsDateInRange(timeStampEntryLast.Time, validStartTimeEndOfDay, validStopTimeEndOfDay))
                        {
                            if (validStartTimeEndOfDay < absenceInterval.ActualStartTime)
                                validStartTimeEndOfDay = absenceInterval.ActualStartTime;

                            var validAdjustStartTime = validStopTimeEndOfDay.AddMinutes(-deviationCause.AdjustTimeInsideOfPlannedAbsence);

                            if (deviationCause.AdjustTimeInsideOfPlannedAbsence > 0 && timeStampEntryLast.Time >= validAdjustStartTime)
                            {
                                timeStampEntryLast.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, validStartTimeEndOfDay, validStopTimeEndOfDay, timeCode, startsWith: false);
                                SplitTimeBlocksAccordingToPlannedAbsenceTimeScheduleBlocks(timeStampEntryLast, absenceInterval, timeCode, timeScheduleTemplateBlocks);
                            }
                            else
                            {
                                timeStampEntryLast.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, timeStampEntryLast.Time, timeCode);
                            }
                        }
                    }
                }
                else if (timeStampEntries.Any(a => a.Time < absenceInterval.ActualStartTime)) //TimeStamp before plannedAbsence
                {
                    TimeStampEntry timeStampEntryLastBefore = timeStampEntries.Last(f => f.Time < absenceInterval.ActualStartTime);
                    DateTime validStartTimeStartOfDay = absenceInterval.ActualStartTime.AddMinutes(-deviationCause.ChangeCauseOutsideOfPlannedAbsence);

                    if (CalendarUtility.IsDateInRange(timeStampEntryLastBefore.Time, validStartTimeStartOfDay, absenceInterval.ActualStartTime))
                    {
                        if (!handledTimes.Contains(timeStampEntryLastBefore.Time))
                            timeStampEntryLastBefore.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, timeStampEntryLastBefore.Time, timeCode);
                        else if (timeStampEntryLastBefore.TimeBlock.Any(a => a.ActualStartTime == absenceInterval.ActualStartTime))
                            timeStampEntryLastBefore.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, absenceInterval.ActualStartTime, absenceInterval.ActualStopTime, timeCode);

                        if (deviationCause.AdjustTimeOutsideOfPlannedAbsence > 0 || deviationCause.AllowGapToPlannedAbsence)
                        {
                            validStartTimeStartOfDay = absenceInterval.ActualStartTime.AddMinutes(-deviationCause.AdjustTimeOutsideOfPlannedAbsence);

                            if (deviationCause.AdjustTimeOutsideOfPlannedAbsence > 0 && CalendarUtility.IsDateInRange(timeStampEntryLastBefore.Time, validStartTimeStartOfDay, absenceInterval.ActualStartTime))
                            {
                                timeStampEntryLastBefore.SetTimeOnTimeBlocks(timeStampEntryLastBefore.Time, absenceInterval.ActualStartTime);
                            }
                            else if (deviationCause.AllowGapToPlannedAbsence)
                            {

                                if (!handledTimes.Contains(timeStampEntryLastBefore.Time))
                                {
                                    DateTime startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, timeStampEntryLastBefore.Time);
                                    DateTime stopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, absenceInterval.ActualStartTime);
                                    TimeDeviationCause timeDeviationCauseOnLastBefore = timeStampEntryLastBefore.TimeDeviationCauseId.HasValue ? GetTimeDeviationCauseFromCache(timeStampEntryLastBefore.TimeDeviationCauseId.Value) : null;
                                    timeDeviationCauseOnLastBefore = timeDeviationCauseOnLastBefore != null && timeDeviationCauseOnLastBefore.Type == (int)TermGroup_TimeDeviationCauseType.Absence ? timeDeviationCauseOnLastBefore : null;
                                    TimeBlockDate timeblockDate = GetTimeBlockDateFromCache(absenceInterval.EmployeeId, absenceInterval.Date);
                                    TimeBlock timeBlockExcess = null;
                                    if (timeStampEntryLastBefore.TimeBlock.Any(a => CalendarUtility.GetOverlappingMinutes(startTime, stopTime, a.StartTime, a.StopTime) > 0 && a.StartTime == startTime))
                                        timeBlockExcess = CreateExcessTimeBlock(SoeTimeRuleType.Absence, startTime, stopTime, absenceInterval.Date, timeblockDate.TimeBlockDateId, absenceInterval.EmployeeId, GetEmployeeGroupFromCache(absenceInterval.EmployeeId, timeblockDate.Date), absenceInterval.TimeScheduleTemplateBlocks.First().TimeScheduleTemplatePeriodId, timeDeviationCause: timeDeviationCauseOnLastBefore);
                                    if (timeBlockExcess != null)
                                    {
                                        TimeBlock timeBlockSameStart = timeStampEntryLastBefore.TimeBlock.FirstOrDefault(w => w.StartTime == startTime);
                                        if (timeBlockSameStart != null)
                                        {
                                            if (timeBlockSameStart.AccountInternal != null)
                                                AddAccountInternalsToTimeBlock(timeBlockExcess, timeBlockSameStart);

                                            timeBlockSameStart.UpdateStartTime(stopTime);
                                            timeStampEntryLastBefore.TimeBlock.Add(timeBlockExcess);
                                        }
                                    }
                                    handledTimes.Add(timeStampEntryLastBefore.Time);
                                }
                            }
                        }

                        SplitTimeBlocksAccordingToPlannedAbsenceTimeScheduleBlocks(timeStampEntryLastBefore, absenceInterval, timeCode, timeScheduleTemplateBlocks);
                    }
                }
                else if (timeStampEntries.Any(a => a.Time > absenceInterval.ActualStopTime)) //TimeStamp after plannedAbsence
                {
                    TimeStampEntry timeStampEntryFirstAfter = timeStampEntries.First(f => f.Time > absenceInterval.ActualStopTime);
                    DateTime validStopTimeEndOfDay = absenceInterval.ActualStopTime.AddMinutes(deviationCause.ChangeCauseOutsideOfPlannedAbsence);

                    if (CalendarUtility.IsDateInRange(timeStampEntryFirstAfter.Time, absenceInterval.ActualStopTime, validStopTimeEndOfDay) || deviationCause.AllowGapToPlannedAbsence)
                    {
                        timeStampEntryFirstAfter.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, timeStampEntryFirstAfter.Time, timeCode);

                        if (!handledTimes.Contains(timeStampEntryFirstAfter.Time))
                            timeStampEntryFirstAfter.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, timeStampEntryFirstAfter.Time, timeCode);
                        else if (timeStampEntryFirstAfter.TimeBlock.Any(a => a.ActualStopTime == absenceInterval.ActualStopTime))
                            timeStampEntryFirstAfter.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, absenceInterval.ActualStartTime, absenceInterval.ActualStopTime, timeCode, false);

                        if (deviationCause.AdjustTimeOutsideOfPlannedAbsence > 0 || deviationCause.AllowGapToPlannedAbsence)
                        {
                            validStopTimeEndOfDay = absenceInterval.ActualStopTime.AddMinutes(deviationCause.AdjustTimeOutsideOfPlannedAbsence);

                            if (deviationCause.AdjustTimeOutsideOfPlannedAbsence > 0 && CalendarUtility.IsDateInRange(timeStampEntryFirstAfter.Time, absenceInterval.ActualStopTime, validStopTimeEndOfDay))
                            {
                                timeStampEntryFirstAfter.SetTimeOnTimeBlocks(timeStampEntryFirstAfter.Time, absenceInterval.ActualStopTime);
                            }
                            else if (deviationCause.AllowGapToPlannedAbsence)
                            {
                                if (!handledTimes.Contains(timeStampEntryFirstAfter.Time))
                                {
                                    DateTime startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, absenceInterval.ActualStopTime);
                                    DateTime stopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, timeStampEntryFirstAfter.Time);
                                    TimeDeviationCause timeDeviationCauseOnFirstAfter = timeStampEntryFirstAfter.TimeDeviationCauseId.HasValue ? GetTimeDeviationCauseFromCache(timeStampEntryFirstAfter.TimeDeviationCauseId.Value) : null;
                                    timeDeviationCauseOnFirstAfter = timeDeviationCauseOnFirstAfter != null && timeDeviationCauseOnFirstAfter.Type == (int)TermGroup_TimeDeviationCauseType.Absence ? timeDeviationCauseOnFirstAfter : null;
                                    TimeBlockDate timeblockDate = GetTimeBlockDateFromCache(absenceInterval.EmployeeId, absenceInterval.Date);
                                    TimeBlock timeBlockExcess = null;
                                    if (timeStampEntryFirstAfter.TimeBlock.Any(a => CalendarUtility.GetOverlappingMinutes(startTime, stopTime, a.StartTime, a.StopTime) > 0 && a.StopTime == stopTime))
                                        timeBlockExcess = CreateExcessTimeBlock(SoeTimeRuleType.Absence, startTime, stopTime, absenceInterval.Date, timeblockDate.TimeBlockDateId, absenceInterval.EmployeeId, GetEmployeeGroupFromCache(absenceInterval.EmployeeId, timeblockDate.Date), absenceInterval.TimeScheduleTemplateBlocks.First().TimeScheduleTemplatePeriodId, timeDeviationCause: timeDeviationCauseOnFirstAfter);
                                    if (timeBlockExcess != null)
                                    {
                                        TimeBlock timeBlockSameStop = timeStampEntryFirstAfter.TimeBlock.FirstOrDefault(w => w.StopTime == stopTime);
                                        if (timeBlockSameStop != null)
                                        {
                                            if (timeBlockSameStop.AccountInternal != null)
                                                AddAccountInternalsToTimeBlock(timeBlockExcess, timeBlockSameStop);

                                            timeBlockSameStop.UpdateStopTime(startTime);
                                        }
                                        timeStampEntryFirstAfter.TimeBlock.Add(timeBlockExcess);
                                    }

                                    handledTimes.Add(timeStampEntryFirstAfter.Time);
                                }
                            }
                        }

                        SplitTimeBlocksAccordingToPlannedAbsenceTimeScheduleBlocks(timeStampEntryFirstAfter, absenceInterval, timeCode, timeScheduleTemplateBlocks);
                    }
                }
            }

            return hasChangeDeviationCauseAccordingToPlannedAbsence;
        }

        private void SplitTimeBlocksAccordingToPlannedAbsenceTimeScheduleBlocks(TimeStampEntry timeStampEntry, PlannedAbsenceIntervalDTO absenceInterval, TimeCode timeCode, List<TimeScheduleTemplateBlock> timeScheduleTemplateBlocks)
        {
            timeStampEntry.SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(absenceInterval.TimeDeviationCauseId, absenceInterval.ActualStartTime, absenceInterval.ActualStartTime, timeCode, startsWith: false);
            var timeScheduleBlocksInterval = absenceInterval.TimeScheduleTemplateBlocks.OrderBy(o => o.StartTime).Where(f => !f.IsBreak).ToList();
            TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseFromCache(absenceInterval.TimeDeviationCauseId);

            if (timeDeviationCause != null)
            {
                foreach (var nextBlock in timeScheduleBlocksInterval.OrderBy(o => o.StartTime))
                {
                    var timeBlockExcess = CreateExcessTimeBlock(SoeTimeRuleType.Absence, nextBlock.StartTime, nextBlock.StopTime, absenceInterval.Date, timeStampEntry.TimeBlockDateId.Value, absenceInterval.EmployeeId, GetEmployeeGroupFromCache(absenceInterval.EmployeeId, timeStampEntry.Time.Date), absenceInterval.TimeScheduleTemplateBlocks.First().TimeScheduleTemplatePeriodId, timeDeviationCause: timeDeviationCause);
                    if (timeBlockExcess != null)
                    {
                        TimeBlock timeBlockSameStart = timeStampEntry.TimeBlock.FirstOrDefault(w => w.StartTime == timeBlockExcess.StartTime);

                        if (timeBlockSameStart != null && timeBlockSameStart.ActualStopTime <= absenceInterval.ActualStopTime)
                        {
                            timeBlockSameStart.State = (int)SoeEntityState.Deleted;
                            timeStampEntry.TimeBlock.Remove(timeBlockSameStart);
                        }
                        else if (timeBlockSameStart != null && timeBlockSameStart.ActualStopTime > absenceInterval.ActualStopTime)
                        {
                            timeBlockSameStart.StartTime = timeBlockExcess.StopTime;
                        }

                        TimeBlock timeBlockSameStop = timeStampEntry.TimeBlock.FirstOrDefault(w => w.StopTime == timeBlockExcess.StopTime);

                        if (timeBlockSameStop != null && timeBlockSameStop.ActualStartTime >= absenceInterval.ActualStartTime)
                        {
                            timeBlockSameStop.State = (int)SoeEntityState.Deleted;
                            timeStampEntry.TimeBlock.Remove(timeBlockSameStop);
                        }
                        else if (timeBlockSameStop != null && timeBlockSameStop.ActualStartTime < absenceInterval.ActualStartTime)
                        {
                            timeBlockSameStop.StopTime = timeBlockExcess.StartTime;
                        }

                        var currentScheduleBlock = timeScheduleTemplateBlocks.FirstOrDefault(f => f.TimeScheduleTemplateBlockId == nextBlock.TimeScheduleTemplateBlockId);

                        if (currentScheduleBlock != null)
                        {
                            if (!currentScheduleBlock.AccountInternal.IsLoaded)
                                currentScheduleBlock.AccountInternal.Load();

                            if (currentScheduleBlock.AccountInternal != null)
                                AddAccountInternalsToTimeBlock(timeBlockExcess, currentScheduleBlock.AccountInternal.Select(s => s.AccountId));
                        }

                        //if (timeBlockSameStart != null)
                        //    timeBlockSameStart.UpdateStartTime(timeBlockExcess.StopTime, setToZeroIfInvalid: true);
                        //if (timeBlockSameStop != null)
                        //    timeBlockSameStop.UpdateStopTime(timeBlockExcess.StartTime, setToZeroIfInvalid: true);

                        timeStampEntry.TimeBlock.Add(timeBlockExcess);
                    }
                }
            }
        }
        #endregion

        #region TimeStampEntry

        private List<TimeStampEntry> GetTimeStampEntriesToProcess(List<TimeStampEntry> originalTargetEntries)
        {
            List<TimeStampEntry> resultEntries = new List<TimeStampEntry>();

            int currentEmployeeId = 0;
            int timeBlockDateId = 0;

            List<TimeStampEntry> employeeTargetEntries = originalTargetEntries.Where(t => t.EmployeeId != 0).OrderBy(t => t.EmployeeId).ThenBy(t => t.Time).ToList();
            foreach (var employeeTargetEntriesGrouping in employeeTargetEntries.Where(t => t.TimeBlockDateId.HasValue).GroupBy(g => $"{g.EmployeeId}#{g.TimeBlockDateId}"))
            {
                TimeStampEntry employeeTargetEntry = employeeTargetEntriesGrouping.First();
                currentEmployeeId = employeeTargetEntry.EmployeeId;
                timeBlockDateId = employeeTargetEntry.TimeBlockDateId.Value;

                // Get all entries for current employee and current date (even deleted to make deletion of TimeBlocks and transactions to work)
                List<TimeStampEntry> employeeEntries = (from e in entities.TimeStampEntry
                                                        .Include("TimeStampEntryExtended")
                                                        .Include("TimeBlockDate")
                                                        where e.EmployeeId == currentEmployeeId &&
                                                        e.TimeBlockDateId == timeBlockDateId
                                                        select e).ToList();

                // Entrys for current employee and date is beeing processed by another process, skip them
                if (!employeeEntries.Any(e => (e.State == (int)SoeEntityState.Deleted) || (e.State == (int)SoeEntityState.Active && e.Status != (int)TermGroup_TimeStampEntryStatus.Processing)))
                    continue;

                // Add entries to collection
                resultEntries.AddRange(employeeEntries);
            }

            return resultEntries;
        }

        private List<DateTime> GetEmployeeStampingDates(Employee employee, DateTime dateFrom, DateTime dateTo, List<EmployeeGroup> employeeGroups)
        {
            List<DateTime> stampingDates = employee?.GetStampingDates(dateFrom, dateTo, employeeGroups);
            if (stampingDates.IsNullOrEmpty())
                return new List<DateTime>();
            return stampingDates.Where(date => HasTimeStampEntrys(employee.EmployeeId, date)).ToList();
        }

        private bool HasTimeStampEntrys(int employeeId, DateTime date)
        {
            return entities.TimeStampEntry.Any(t => t.EmployeeId == employeeId && t.State == (int)SoeEntityState.Active && t.TimeBlockDate.Date == date);
        }

        private bool HasTimeStampsError(List<TimeStampEntry> timeStampEntries, out TermGroup_TimeBlockDateStampingStatus errorStatus, bool doNotModifyTimeStampTypesCompSetting)
        {
            errorStatus = TermGroup_TimeBlockDateStampingStatus.NoStamps;

            if (HasTimeStampsError(timeStampEntries, TermGroup_TimeBlockDateStampingStatus.OddNumberOfStamps))
                errorStatus = TermGroup_TimeBlockDateStampingStatus.OddNumberOfStamps;

            if (doNotModifyTimeStampTypesCompSetting)
            {
                // This section emplies only on XETimeStamp so filter out those
                List<TimeStampEntry> entries = timeStampEntries.Where(ts => ts.TimeTerminal == null || ts.TimeTerminal.Type == (int)TimeTerminalType.GoTimeStamp || ts.TimeTerminal.Type == (int)TimeTerminalType.XETimeStamp || ts.TimeTerminal.Type == (int)TimeTerminalType.WebTimeStamp).ToList();

                if (HasTimeStampsError(entries, TermGroup_TimeBlockDateStampingStatus.FirstStampIsNotIn))
                    errorStatus = TermGroup_TimeBlockDateStampingStatus.FirstStampIsNotIn;
                else if (HasTimeStampsError(entries, TermGroup_TimeBlockDateStampingStatus.InvalidSequenceOfStamps))
                    errorStatus = TermGroup_TimeBlockDateStampingStatus.InvalidSequenceOfStamps;
                else if (HasTimeStampsError(entries, TermGroup_TimeBlockDateStampingStatus.StampsWithInvalidType))
                    errorStatus = TermGroup_TimeBlockDateStampingStatus.StampsWithInvalidType;
            }

            return errorStatus != TermGroup_TimeBlockDateStampingStatus.NoStamps;
        }

        private bool HasTimeStampsError(List<TimeStampEntry> timeStampEntries, TermGroup_TimeBlockDateStampingStatus status)
        {
            bool hasError = false;

            if (timeStampEntries != null)
            {
                switch (status)
                {
                    // First entry each day must be of type IN
                    case TermGroup_TimeBlockDateStampingStatus.FirstStampIsNotIn:
                        hasError = timeStampEntries.Any() && timeStampEntries.First().Type != (int)TimeStampEntryType.In;
                        break;
                    // Must be an even number of entrys each day
                    case TermGroup_TimeBlockDateStampingStatus.OddNumberOfStamps:
                        hasError = timeStampEntries.Count % 2 != 0;
                        break;
                    // Must not be two entrys with same type after another
                    case TermGroup_TimeBlockDateStampingStatus.InvalidSequenceOfStamps:
                        int? previousType = null;
                        foreach (int entryType in timeStampEntries.OrderBy(i => i.Time).Select(t => t.Type))
                        {
                            if (!previousType.HasValue)
                            {
                                previousType = entryType;
                                continue;
                            }

                            if (previousType.Value == entryType)
                            {
                                hasError = true;
                                break;
                            }
                            else
                            {
                                previousType = entryType;
                            }
                        }
                        break;
                    // Must not be any entrys with unknown type
                    case TermGroup_TimeBlockDateStampingStatus.StampsWithInvalidType:
                        hasError = timeStampEntries.Any(i => i.Type == (int)TermGroup_TimeStampEntryType.Unknown);
                        break;
                }
            }

            return hasError;
        }

        private ActionResult SaveTimeStampEntryStatus(List<TimeStampEntry> timeStampEntries, TermGroup_TimeStampEntryStatus? status)
        {
            if (timeStampEntries.IsNullOrEmpty())
                return new ActionResult(true);

            // If status is not specified, revert back to OldStatus
            foreach (TimeStampEntry timeStampEntry in timeStampEntries)
            {
                if (status.HasValue)
                    timeStampEntry.Status = (int)status.Value;
                else
                    timeStampEntry.Status = timeStampEntry.OldStatus;
            }

            ActionResult result = BulkUpdate(entities, timeStampEntries.Where(w => w.TimeStampEntryId != 0), this.currentTransaction);
            if (!result.Success)
            {
                result.ErrorNumber = (int)ActionResultSave.TimeStampFailedToUpdateStatuses;
            }
            else
            {
                result = BulkInsert(entities, timeStampEntries.Where(w => w.TimeStampEntryId == 0), this.currentTransaction);
                if (!result.Success)
                    result.ErrorNumber = (int)ActionResultSave.TimeStampFailedToUpdateStatuses;
            }

            return result;
        }

        private ActionResult SaveTimeBlockDateStampingStatus(TimeBlockDate timeBlockDate, TermGroup_TimeBlockDateStampingStatus errorStampingStatus, bool save = true)
        {
            //Preserve StampingStatus if set to a "completed with error" status during this session
            if (errorStampingStatus == TermGroup_TimeBlockDateStampingStatus.Complete &&
                timeBlockDate.StampingStatus > (int)TermGroup_TimeBlockDateStampingStatus.COMPLETED_WITH_ERRORS &&
                timeBlockDate.HasSetToCompletedWithErrorsStatus)
                return new ActionResult(true);

            timeBlockDate.StampingStatus = (int)errorStampingStatus;
            if (errorStampingStatus > TermGroup_TimeBlockDateStampingStatus.COMPLETED_WITH_ERRORS)
                timeBlockDate.HasSetToCompletedWithErrorsStatus = true;
            SetModifiedProperties(timeBlockDate);

            return save ? Save() : new ActionResult(true);
        }

        private ActionResult RevertTimeStampEntryStatus(List<TimeStampEntry> timeStampEntries, ActionResult result = null)
        {
            if (timeStampEntries.IsNullOrEmpty())
                return new ActionResult(true);

            if (!IsCannotDeleteTransactionsError(result))
                timeStampEntries.Where(t => t.OldStatus == (int)TermGroup_TimeStampEntryStatus.Processed).ToList().ForEach(t => t.OldStatus = (int)TermGroup_TimeStampEntryStatus.Partial);
            timeStampEntries.Where(t => t.Status == (int)TermGroup_TimeStampEntryStatus.Processing).ToList().ForEach(t => t.Status = (int)TermGroup_TimeStampEntryStatus.Processed);
            return SaveTimeStampEntryStatus(timeStampEntries, null);
        }

        private ActionResult RevertTimeBlockDateStatus(TimeBlockDate timeBlockDate, TermGroup_TimeBlockDateStampingStatus errorStampingStatus)
        {
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");

            return SaveTimeBlockDateStampingStatus(timeBlockDate, errorStampingStatus);
        }

        private void RevertTimeStampEntryAndTimeBlockDateStatus(List<TimeStampEntry> timeStampEntries, Employee employee, TimeBlockDate timeBlockDate, TermGroup_TimeBlockDateStampingStatus errorStampingStatus, string method)
        {
            ActionResult result = RevertTimeStampEntryStatus(timeStampEntries);
            if (!result.Success)
                LogAndRevertEntryStatus(result, timeStampEntries, employee, timeBlockDate, Level.Warn, method, "SaveTimeStampEntryStatus");

            result = RevertTimeBlockDateStatus(timeBlockDate, errorStampingStatus);
            if (!result.Success)
                LogAndRevertEntryStatus(result, timeStampEntries, employee, timeBlockDate, Level.Warn, method, "SaveTimeBlockDateStampingStatus");
        }

        private void LogAndRevertEntryStatus(ActionResult result, List<TimeStampEntry> timeStampEntries, Employee employee, TimeBlockDate timeBlockDate, Level level, string method, string action)
        {
            if (!IsCannotDeleteTransactionsError(result))
                Log(level, new Exception($"{method}: {action} ErrorCode: {result.ErrorNumber} ({result.ErrorMessage}). {employee.GetNrAndDateString(timeBlockDate, actorCompanyId)}"));
            RevertTimeStampEntryStatus(timeStampEntries, result);
        }

        private void AddTimeStampSystemInfoLog(Employee currentEmployee, TimeBlockDate timeBlockDate)
        {
            SystemInfoLog systemInfoLog = new SystemInfoLog()
            {
                Entity = (int)SoeEntityType.TimeScheduleEmployeePeriod,
                LogLevel = (int)SystemInfoLogLevel.Warning,
                RecordId = 0,
                Date = DateTime.Now,
                Text = "{0} " + currentEmployee.EmployeeNrAndName + " {1} " + timeBlockDate.Date.ToShortDateString(),
                Type = (int)SystemInfoType.TimeStamp_TimeScheduleTemplatePeriodMissing,
                DeleteManually = true,

                //Set FK
                ActorCompanyId = actorCompanyId,
                EmployeeId = currentEmployee.EmployeeId,
            };

            GeneralManager.AddSystemInfoLogEntry(this.currentTransaction, entities, systemInfoLog);
        }

        private bool IsCannotDeleteTransactionsError(ActionResult result)
        {
            return result?.ErrorNumber == (int)ActionResultSave.TimePayrollTransactionCannotDeleteNotInitialAttestState || result?.ErrorNumber == (int)ActionResultSave.TimePayrollTransactionCannotDeleteIsPayroll;
        }

        #endregion
    }
}
