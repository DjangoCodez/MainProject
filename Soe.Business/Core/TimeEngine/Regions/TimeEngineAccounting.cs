using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Recalculates accounting from prio on TimePayrolltransactions and TimePayrollScheduleTranscations for period
        /// </summary>
        /// <returns>Output DTO</returns>
        private RecalculateAccountingFromPayrollOutputDTO TaskRecalculateAccountingFromPayroll()
        {
            var (iDTO, oDTO) = InitTask<RecalculateAccountingFromPayrollInputDTO, RecalculateAccountingFromPayrollOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null || iDTO.EmployeeIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    TimePeriod timePeriod = GetTimePeriod(iDTO.TimePeriodId);
                    if (timePeriod == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                        return oDTO;
                    }

                    List<AccountDim> accountDims = GetAccountDims();

                    #endregion

                    foreach (int employeeId in iDTO.EmployeeIds)
                    {
                        InitContext(taskEntities);

                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
                            if (employee == null)
                                continue;

                            int nrOfChangedEmployeeTransactions = 0;

                            #region TimePayrollTransaction

                            List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForDayWithExtendedAndTimeCodeAndAccounting(employee.EmployeeId, timePeriod.StartDate, timePeriod.StopDate, timePeriod);
                            foreach (var timePayrollTransactionsByDate in timePayrollTransactions.GroupBy(i => i.TimeBlockDateId))
                            {
                                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, timePayrollTransactionsByDate.Key);
                                if (timeBlockDate == null)
                                    continue;

                                Employment employment = employee.GetEmployment(timeBlockDate.Date);
                                if (employment == null || employment.FixedAccounting)
                                    continue;

                                foreach (var timePayrollTransactionsByProduct in timePayrollTransactionsByDate.GroupBy(i => i.ProductId))
                                {
                                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransactionsByProduct.Key);
                                    if (payrollProduct == null)
                                        continue;

                                    foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsByProduct)
                                    {
                                        if (ApplyAccountingPrioOnTimePayrollTransaction(timePayrollTransaction, payrollProduct, employee, timeBlockDate, accountDims))
                                            nrOfChangedEmployeeTransactions++;
                                    }
                                }
                            }

                            #endregion

                            #region TimePayrollScheduleTransaction

                            List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = GetTimePayrollScheduleTransactionsWithAccounting(timePeriod, timePeriod.StartDate, timePeriod.StopDate, employee.EmployeeId);
                            foreach (var timePayrollScheduleTransactionsByDate in timePayrollScheduleTransactions.GroupBy(i => i.TimeBlockDateId))
                            {
                                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, timePayrollScheduleTransactionsByDate.Key);
                                if (timeBlockDate == null)
                                    continue;

                                Employment employment = employee.GetEmployment(timeBlockDate.Date);
                                if (employment == null || employment.FixedAccounting)
                                    continue;

                                foreach (var timePayrollScheduleTransactionsByProduct in timePayrollScheduleTransactionsByDate.GroupBy(i => i.ProductId))
                                {
                                    PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollScheduleTransactionsByProduct.Key);
                                    if (payrollProduct == null)
                                        continue;

                                    foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in timePayrollScheduleTransactionsByProduct)
                                    {
                                        if (ApplyAccountingPrioOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction, payrollProduct, employee, timeBlockDate, accountDims))
                                            nrOfChangedEmployeeTransactions++;
                                    }
                                }
                            }

                            #endregion

                            if (nrOfChangedEmployeeTransactions > 0)
                                TrySaveAndCommit(oDTO);
                        }
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
        /// Recalculates accounting on TimePayrolltransactions and TimePayrollScheduleTranscations for period
        /// </summary>
        /// <returns>Output DTO</returns>
        private RecalculateAccountingOutputDTO TaskRecalculateAccounting()
        {
            var (iDTO, oDTO) = InitTask<RecalculateAccountingInputDTO, RecalculateAccountingOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null || iDTO.Items.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                List<DateTime> failedDates = new List<DateTime>();

                try
                {
                    var employeeGroups = GetEmployeeGroupsFromCache();

                    foreach (var itemsByEmployee in iDTO.Items.GroupBy(i => i.EmployeeId))
                    {
                        var employee = GetEmployeeWithContactPersonAndEmploymentFromCache(itemsByEmployee.Key);
                        if (employee == null)
                            continue;

                        var period = ConvertToTimeEnginePeriod(employee, itemsByEmployee.ToList(), employeeGroups);

                        LoadScheduleBlocks();
                        LoadTimeBlocks();

                        foreach (var day in period.Days)
                        {
                            if (day.TimeBlockDate.IsLocked)
                            {
                                AddDateAsFailed(day.Date);
                            }
                            else if (iDTO.DoRecalculateFromShiftType)
                            {
                                oDTO.Result = ProcessRecalculateFromShiftType(day);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
                            else if (iDTO.DoRecalculateFromSchedule)
                            {
                                oDTO.Result = ProcessRecalculateFromSchedule(day);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
                            else if (iDTO.DoRecalculateFromTime)
                            {
                                oDTO.Result = ProcessRecalculateFromTime(day);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
                        }

                        Dictionary<DateTime, List<TimeScheduleTemplateBlock>> scheduleBlocksByDay;
                        List<TimeScheduleTemplateBlock> GetScheduleBlocks(TimeEngineDay day)
                        {
                            var scheduleBlocksForDay = scheduleBlocksByDay.GetList(day.Date);
                            if (!scheduleBlocksForDay.IsNullOrEmpty() && scheduleBlocksForDay.Any(b => b.IsDetached()))
                                scheduleBlocksForDay = GetScheduleBlocksForEmployeeOnDatesWithTimeCodeAndPeriodAndAccounting(employee.EmployeeId, day.Date);
                            return scheduleBlocksForDay;
                        }

                        void LoadScheduleBlocks()
                        {
                            var scheduleBlocksForPeriod = GetScheduleBlocksForEmployeeOnDatesWithTimeCodeAndPeriodAndAccounting(employee.EmployeeId, period.Dates);
                            ClearAccountIdIfTemplateHeadIsNotPersonal(scheduleBlocksForPeriod);
                            scheduleBlocksByDay = scheduleBlocksForPeriod.Where(b => b.Date.HasValue).GroupBy(b => b.Date.Value).ToDictionary(k => k.Key, v => v.ToList());                            
                        }

                        Dictionary<int, List<TimeBlock>> timeBlocksByDay;
                        List<TimeBlock> GetTimeBlocks(TimeEngineDay day) => timeBlocksByDay.GetList(day.TimeBlockDateId);
                        void LoadTimeBlocks()
                        {
                            var timeBlocksForPeriod = GetTimeBlocksWithTimePayrollTransactionsAndAccounting(employee.EmployeeId, period.TimeBlockDateIds);
                            LoadDeviationAccounts(timeBlocksForPeriod.Where(timeBlock => period.DeviationDates.Contains(timeBlock.TimeBlockDate.Date)).ToList());
                            timeBlocksByDay = timeBlocksForPeriod.GroupBy(b => b.TimeBlockDateId).ToDictionary(k => k.Key, v => v.ToList());                       
                        }

                        ActionResult ProcessRecalculateFromShiftType(TimeEngineDay day)
                        {
                            ApplyAccountingOnScheduleBlocks(day);

                            var result = Save();
                            if (!result.Success)
                                return result;

                            ApplyAccountingOnTimeBlocks(day);

                            result = Save();
                            if (!result.Success)
                                return result;

                            RecalculateAccountingFromTime(day);

                            result = Save();
                            return result;
                        }
                        ActionResult ProcessRecalculateFromSchedule(TimeEngineDay day)
                        {
                            ApplyAccountingOnTimeBlocks(day);

                            var result = Save();
                            if (!result.Success)
                                return result;

                            RecalculateAccountingFromTime(day);

                            result = Save();
                            return result;
                        }
                        ActionResult ProcessRecalculateFromTime(TimeEngineDay day)
                        {
                            RecalculateAccountingFromTime(day);

                            var result = Save();
                            return result;
                        }

                        void RecalculateAccountingFromTime(TimeEngineDay day)
                        {
                            day.EmployeeGroup = employee.GetEmployeeGroup(day.Date, GetEmployeeGroupsFromCache());
                            if (day.EmployeeGroup == null)
                                return;

                            if (day.EmployeeGroup.AutogenTimeblocks)
                                RecalculateAccountingFromTimeBlock(day);
                            else
                                RecalculateAccountingForDayFromTimeStamp(day);
                        }
                        void RecalculateAccountingFromTimeBlock(TimeEngineDay day)
                        {
                            var timeBlocksForDay = GetTimeBlocks(day);
                            if (timeBlocksForDay.IsNullOrEmpty())
                                return;

                            if (TryGetDeviationTimeBlockAccounting(day, out var accountStdIdByTimeBlock, out var accountInternalsByTimeBlock))
                                ApplyAccountingOnTimePayrollTransactions(day, accountStdIdByTimeBlock, accountInternalsByTimeBlock);
                            else
                                AddDateAsFailed(day.Date);
                        }
                        void RecalculateAccountingForDayFromTimeStamp(TimeEngineDay day)
                        {
                            var timeStampEntriesForDay = TimeStampManager.GetTimeStampEntriesForRecalculation(entities, day.TimeBlockDateId).ToList();
                            if (timeStampEntriesForDay.IsNullOrEmpty())
                            {
                                RecalculateAccountingFromTimeBlock(day);
                            }
                            else
                            {
                                if (TryGetStampingTimeBlockAccounting(day, timeStampEntriesForDay, out var accountStdIdByTimeBlock, out var accountInternalsByTimeBlock))
                                    ApplyAccountingOnTimePayrollTransactions(day, accountStdIdByTimeBlock, accountInternalsByTimeBlock);
                                else
                                    AddDateAsFailed(day.Date);
                            }
                        }

                        void ApplyAccountingOnScheduleBlocks(TimeEngineDay day)
                        {
                            var scheduleBlocksForDay = GetScheduleBlocks(day);

                            foreach (var scheduleBlock in scheduleBlocksForDay)
                            {
                                List<int> accountIdsFromTemplateSchedule = null; //TODO
                                ApplyAccountingOnTimeScheduleTemplateBlock(scheduleBlock, employee, scheduleBlock.ShiftTypeId, accountIdsFromTemplateSchedule, null);
                            }
                        }
                        void ApplyAccountingOnTimeBlocks(TimeEngineDay day)
                        {
                            var scheduleBlocksForDay = GetScheduleBlocks(day);
                            var timeBlocksForDay = GetTimeBlocks(day);

                            foreach (TimeBlock timeBlock in timeBlocksForDay.GetWork(excludeGeneratedFromBreak: true))
                            {
                                var scheduleBlock = scheduleBlocksForDay.GetMatchingScheduleBlock(timeBlock, false);
                                if (scheduleBlock == null)
                                    continue;

                                ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, scheduleBlock, employee, accountInternals: timeBlock.DeviationAccounts);
                            }
                        }
                        void ApplyAccountingOnTimePayrollTransactions(TimeEngineDay day, Dictionary<int, int?> accountStdIdByTimeBlock = null, Dictionary<int, List<AccountInternal>> accountInternalsByTimeBlock = null)
                        {
                            var timeBlocksForDay = GetTimeBlocks(day);
                            var timePayrollTransactionsForDay = timeBlocksForDay.GetTimePayrollTransactions();

                            foreach (var timePayrollTransactionsForDayByProduct in timePayrollTransactionsForDay.GroupBy(tpt => tpt.ProductId))
                            {
                                var payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransactionsForDayByProduct.Key);
                                if (payrollProduct == null)
                                    continue;

                                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactionsForDayByProduct)
                                {
                                    if (timePayrollTransaction.IsExcludedInRecalculateAccounting())
                                        continue;

                                    int? accountStdId = accountStdIdByTimeBlock.GetValue(timePayrollTransaction.TimeBlock.TimeBlockId);
                                    List<AccountInternal> accountInternals = accountInternalsByTimeBlock.GetList(timePayrollTransaction.TimeBlock.TimeBlockId, nullIfNotFound: true);

                                    ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, day.Date, payrollProduct, timePayrollTransaction.TimeBlock, accountStdId, accountInternals);
                                }
                            }
                        }
                        
                        bool TryGetDeviationTimeBlockAccounting(TimeEngineDay day, out Dictionary<int, int?> accountStdIdByTimeBlock, out Dictionary<int, List<AccountInternal>> accountInternalsByTimeBlock)
                        {
                            accountStdIdByTimeBlock = new Dictionary<int, int?>();
                            accountInternalsByTimeBlock = new Dictionary<int, List<AccountInternal>>();

                            var timeBlocksForDay = GetTimeBlocks(day);

                            foreach (TimeBlock timeBlock in timeBlocksForDay)
                            {
                                if (timeBlock.DeviationAccounts.IsNullOrEmpty())
                                    continue;
                                if (accountInternalsByTimeBlock.ContainsKey(timeBlock.TimeBlockId))
                                    return false;

                                accountStdIdByTimeBlock.Add(timeBlock.TimeBlockId, timeBlock.AccountStdId);
                                accountInternalsByTimeBlock.Add(timeBlock.TimeBlockId, timeBlock.DeviationAccounts.ToList());
                            }

                            return true;
                        }
                        bool TryGetStampingTimeBlockAccounting(TimeEngineDay day, List<TimeStampEntry> timeStampEntries, out Dictionary<int, int?> accountStdIdByTimeBlock, out Dictionary<int, List<AccountInternal>> accountInternalsByTimeBlock)
                        {
                            accountStdIdByTimeBlock = new Dictionary<int, int?>();
                            accountInternalsByTimeBlock = new Dictionary<int, List<AccountInternal>>();

                            List<TimeBlock> stampingTimeBlocks = null;

                            try
                            {
                                var timeBlocksForDay = GetTimeBlocks(day);

                                if (!TryCreateTimeBlocksFromTimeStamps(out stampingTimeBlocks, ref oDTO, timeStampEntries, employee, day.EmployeeGroup, day.TimeBlockDate) || timeBlocksForDay.IsNullOrEmpty() || stampingTimeBlocks.IsNullOrEmpty())
                                    return false;

                                stampingTimeBlocks = EvaluateBreaksRules(day.EmployeeGroup, day.TimeBlockDate, GetScheduleBlocks(day), stampingTimeBlocks);
                                if (timeBlocksForDay.Count != stampingTimeBlocks.Count)
                                    return false;

                                foreach (TimeBlock stampingTimeBlock in stampingTimeBlocks)
                                {
                                    TimeBlock timeBlock = timeBlocksForDay.FirstOrDefault(tb => tb.StartTime == stampingTimeBlock.StartTime && tb.StopTime == stampingTimeBlock.StopTime);
                                    if (timeBlock == null || accountInternalsByTimeBlock.ContainsKey(timeBlock.TimeBlockId))
                                        return false;

                                    accountStdIdByTimeBlock.Add(timeBlock.TimeBlockId, stampingTimeBlock.AccountStdId);
                                    accountInternalsByTimeBlock.Add(timeBlock.TimeBlockId, stampingTimeBlock.AccountInternal?.ToList());
                                }
                            }
                            finally
                            {
                                stampingTimeBlocks?
                                    .Where(tb => tb.TimeBlockDate != null && tb.TimeBlockDate.Status != (int)SoeTimeBlockDateStatus.None)
                                    .ToList()
                                    .ForEach(tb => tb.TimeBlockDate.Status = (int)SoeTimeBlockDateStatus.None);
                                base.TryDetachEntitys(entities, stampingTimeBlocks);
                            }

                            return true;
                        }               

                        void AddDateAsFailed(DateTime date)
                        {
                            if (!failedDates.Contains(date))
                                failedDates.Add(date);
                        }
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

                    if (!failedDates.IsNullOrEmpty())
                        oDTO.Result.Dates = failedDates;

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Saves account provision for AccountInternals and TimePeriods
        /// </summary>
        /// <returns></returns>
        private SaveAccountProvisionBaseOutputDTO TaskSaveAccountProvisionBase()
        {
            var (iDTO, oDTO) = InitTask<SaveAccountProvisionBaseInputDTO, SaveAccountProvisionBaseOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        foreach (AccountProvisionBaseDTO provision in iDTO.InputProvisions)
                        {
                            //Can only edit the last period
                            TimePeriodAccountValueDTO dto = new TimePeriodAccountValueDTO()
                            {
                                TimePeriodAccountValueId = provision.TimePeriodAccountValueId,
                                ActorCompanyId = ActorCompanyId,
                                TimePeriodId = provision.TimePeriodId,
                                AccountId = provision.AccountId,
                                Type = SoeTimePeriodAccountValueType.Provision,
                                Status = provision.IsLocked ? SoeTimePeriodAccountValueStatus.Locked : SoeTimePeriodAccountValueStatus.Open,
                                Value = provision.Period12Value,
                            };

                            var result = CreateOrUpdateTimePeriodAccountValue(dto);
                            if (!result.Success)
                            {
                                //For now allow that row isnt saved without message to the client
                            }
                        }

                        oDTO.Result = Save();

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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Locks TimePeriodAccountValues
        /// </summary>
        /// <returns></returns>
        private LockUnlockPayrollPeriodOutputDTO TaskLockAccountProvisionBase()
        {
            var (iDTO, oDTO) = InitTask<LockUnlockAccountProvisionBaseInputDTO, LockUnlockPayrollPeriodOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);
                entities.CommandTimeout = 500;

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        TimePeriod timePeriod = GetTimePeriod(iDTO.TimePeriodId);
                        if (timePeriod == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                            return oDTO;
                        }

                        #endregion

                        #region Perform

                        oDTO.Result = SetTimePeriodAccountValueStatus(timePeriod.TimePeriodId, SoeTimePeriodAccountValueStatus.Locked);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = SaveTimePayrollTransactionsForAccountProvision(timePeriod);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = Save();
                        if (!oDTO.Result.Success)
                            return oDTO;

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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// UnLocks TimePeriodAccountValues
        /// </summary>
        /// <returns></returns>
        private LockUnlockPayrollPeriodOutputDTO TaskUnLockAccountProvisionBase()
        {
            var (iDTO, oDTO) = InitTask<LockUnlockAccountProvisionBaseInputDTO, LockUnlockPayrollPeriodOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #endregion

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Prereq

                        AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                        if (attestStateInitial == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                            return oDTO;
                        }

                        TimePeriod timePeriod = GetTimePeriod(iDTO.TimePeriodId);
                        if (timePeriod == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                            return oDTO;
                        }

                        List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForCompanyAndAccountProvision(timePeriod.StartDate, timePeriod.StopDate);

                        //Check that any transactions isnt attested
                        if (timePayrollTransactions.Any(i => i.AttestStateId != attestStateInitial.AttestStateId))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.AccountProvisionUnlockFailedAttestedTransactions, GetText(10041, "Kan inte låsa upp underlaget. Det finns transaktioner som inte har lägsta attestnivå"));
                            return oDTO;
                        }

                        #endregion

                        #region Perform

                        oDTO.Result = SetTimePeriodAccountValueStatus(timePeriod.TimePeriodId, SoeTimePeriodAccountValueStatus.Open);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions, saveChanges: false);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        oDTO.Result = Save();
                        if (!oDTO.Result.Success)
                            return oDTO;

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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Update TimePayrollTransactions from AccountProvisions
        /// </summary>
        /// <returns></returns>
        private UpdateAccountProvisionTransactionOutputDTO TaskUpdateAccountProvisionTransactions()
        {
            var (iDTO, oDTO) = InitTask<UpdateAccountProvisionTransactionsInputDTO, UpdateAccountProvisionTransactionOutputDTO>();
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

                        var inputTimePayrollTransactions = iDTO.InputTransactions;

                        var timePayrollTransctions = GetTimePayrollTransactionsForCompanyAndAccountProvisionWithTimeCodeAndAccountInternal(inputTimePayrollTransactions.Select(tr => tr.TimePayrollTransactionId).ToList());
                        if (timePayrollTransctions != null && timePayrollTransctions.Count > 0)
                        {
                            //Get valid AttestState Transitions, One for each different transistion beeing made.
                            var userValidTransitions = new List<AttestUserRoleView>();

                            foreach (var attestStateId in inputTimePayrollTransactions.Select(p => p.AttestStateId).Distinct())
                            {
                                var view = AttestManager.GetAttestUserRoleViews(entities, base.UserId, actorCompanyId, attestStateId);
                                if (view != null)
                                    userValidTransitions.AddRange(view);
                            }

                            foreach (var timePayrollTransction in timePayrollTransctions)
                            {
                                //Find the right accountProvisionTransactionDTO, Should always be just one.
                                var inputTimePayrollTransaction = inputTimePayrollTransactions.FirstOrDefault(a => a.TimePayrollTransactionId == timePayrollTransction.TimePayrollTransactionId);
                                if (inputTimePayrollTransaction != null)
                                {
                                    //Set changed values
                                    timePayrollTransction.Amount = inputTimePayrollTransaction.Amount;
                                    timePayrollTransction.Quantity = inputTimePayrollTransaction.Quantity;
                                    timePayrollTransction.Comment = inputTimePayrollTransaction.Comment;
                                    SetModifiedProperties(timePayrollTransction);
                                }
                            }
                        }

                        oDTO.Result = Save();

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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }
            return oDTO;
        }

        #endregion

        #region Load references

        private void TryLoadAccountInternalsOnTimeScheduleTemplateBlock(ref TimeScheduleTemplateBlock templateBlock)
        {
            try
            {
                if (templateBlock?.AccountInternal == null || templateBlock.AccountInternal.IsLoaded || templateBlock.AccountInternal.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, templateBlock))
                {
                    templateBlock.AccountInternal.Load();
                    templateBlock.AccountInternal.Where(ai => !ai.AccountReference.IsLoaded).ToList().ForEach(ai => ai.AccountReference.Load());
                }
                else if (templateBlock.TimeScheduleTemplateBlockId > 0)
                {
                    var templateBlockFromDb = GetTimeScheduleTemplateBlockWithAccountsFromCache(templateBlock.TimeScheduleTemplateBlockId);
                    if (templateBlockFromDb?.AccountInternal != null)
                    {
                        templateBlock.AccountInternal.AddRange(templateBlockFromDb.AccountInternal);
                        ((IRelatedEnd)templateBlock.AccountInternal).IsLoaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        private void TryLoadAccountInternalsOnTimeBlock(ref TimeBlock timeBlock)
        {
            try
            {
                if (timeBlock?.AccountInternal == null || timeBlock.AccountInternal.IsLoaded || timeBlock.AccountInternal.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, timeBlock))
                {
                    timeBlock.AccountInternal.Load();
                    timeBlock.AccountInternal.Where(ai => !ai.AccountReference.IsLoaded).ToList().ForEach(ai => ai.AccountReference.Load());
                }
                else if (timeBlock.TimeBlockId > 0)
                {
                    var timeBlockFromDb = GetTimeBlockWithAccountsDiscardedStateFromCache(timeBlock.TimeBlockId);
                    if (timeBlockFromDb?.AccountInternal != null)
                    {
                        timeBlock.AccountInternal.AddRange(timeBlockFromDb.AccountInternal);
                        ((IRelatedEnd)timeBlock.AccountInternal).IsLoaded = true;
                    }
                }

            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        private void TryLoadPayrollProductSetting(PayrollProduct payrollProduct)
        {
            try
            {
                if (payrollProduct?.PayrollProductSetting == null || payrollProduct.PayrollProductSetting.IsLoaded || payrollProduct.PayrollProductSetting.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, payrollProduct))
                    payrollProduct.PayrollProductSetting.Load();
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        private void TryLoadAccountStdOnPayrollProductSetting(PayrollProductSetting payrollProductSetting)
        {
            try
            {
                if (payrollProductSetting?.PayrollProductAccountStd == null || payrollProductSetting.PayrollProductAccountStd.IsLoaded || payrollProductSetting.PayrollProductAccountStd.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, payrollProductSetting))
                    payrollProductSetting.PayrollProductAccountStd.Load();
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        private void TryLoadAccountInternalOnPayrollProductAccountStd(PayrollProductAccountStd payrollProductAccountStd)
        {
            try
            {
                if (payrollProductAccountStd?.AccountInternal == null || payrollProductAccountStd.AccountInternal.IsLoaded || payrollProductAccountStd.AccountInternal.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, payrollProductAccountStd))
                    payrollProductAccountStd.AccountInternal.Load();
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        private void TryLoadAccountInternalsOnTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction)
        {
            try
            {
                if (timePayrollTransaction?.AccountInternal == null || timePayrollTransaction.AccountInternal.IsLoaded || timePayrollTransaction.AccountInternal.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, timePayrollTransaction))
                    timePayrollTransaction.AccountInternal.Load();
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        private void TryLoadAccountInternalsOnTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction)
        {
            try
            {
                if (timePayrollScheduleTransaction?.AccountInternal == null || timePayrollScheduleTransaction.AccountInternal.IsLoaded || timePayrollScheduleTransaction.AccountInternal.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, timePayrollScheduleTransaction))
                    timePayrollScheduleTransaction.AccountInternal.Load();
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        private void TryLoadAccountInternalsOnTimeInvoiceTransaction(TimeInvoiceTransaction timeInvoiceTransaction)
        {
            try
            {
                if (timeInvoiceTransaction?.AccountInternal == null || timeInvoiceTransaction.AccountInternal.IsLoaded || timeInvoiceTransaction.AccountInternal.Any())
                    return;

                if (base.CanEntityLoadReferences(entities, timeInvoiceTransaction))
                    timeInvoiceTransaction.AccountInternal.Load();
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
            }
        }

        #endregion

        #region Apply Accounting from entity

        private void ApplyAccountingFromShiftType(List<TimeScheduleTemplateBlockDTO> templateBlockItems)
        {
            // Set accounting from shift type
            List<ShiftType> shiftTypes = TimeScheduleManager.GetShiftTypes(entities, actorCompanyId, loadAccountInternals: true);
            foreach (TimeScheduleTemplateBlockDTO item in templateBlockItems)
            {
                if (item.ShiftTypeId.HasValue && item.ShiftTypeId.Value != 0)
                {
                    ShiftType shiftType = shiftTypes.FirstOrDefault(s => s.ShiftTypeId == item.ShiftTypeId.Value);
                    if (shiftType != null)
                    {
                        int index = 0;
                        foreach (AccountInternal accInt in shiftType.AccountInternal)
                        {
                            switch (index)
                            {
                                case 0:
                                    item.Dim2Id = accInt.AccountId;
                                    break;
                                case 1:
                                    item.Dim3Id = accInt.AccountId;
                                    break;
                                case 2:
                                    item.Dim4Id = accInt.AccountId;
                                    break;
                                case 3:
                                    item.Dim5Id = accInt.AccountId;
                                    break;
                                case 4:
                                    item.Dim6Id = accInt.AccountId;
                                    break;
                            }
                            index++;
                        }
                    }
                }
            }
        }

        private void ApplyAccountingOnTimeScheduleTemplateBlock(TimeScheduleTemplateBlock templateBlock, Employee employee, int? shiftTypeId, List<int> accountIdsFromTemplateSchedule, List<int> accountIdsFromPlacement)
        {
            if (templateBlock == null)
                return;

            #region AccountStd

            // NA

            #endregion

            #region AccountInternal

            // Load existing accounting
             TryLoadAccountInternalsOnTimeScheduleTemplateBlock(ref templateBlock);

            // Clear existing accounting
            templateBlock.AccountInternal.Clear();
            if (templateBlock.AccountInternal == null)
                templateBlock.AccountInternal = new EntityCollection<AccountInternal>();

            #region Prio 1: Accounting from ShiftType

            if (shiftTypeId.HasValue)
            {
                ShiftType shiftType = GetShiftTypeWithAccountsFromCache(shiftTypeId.Value);
                if (shiftType != null && shiftType.AccountInternal != null)
                    AddAccountInternalsToTimeScheduleTemplateBlock(templateBlock, shiftType.AccountInternal, setPredefinedAccountOnTemplateBlock: false);
            }

            #endregion

            #region Prio 2: Accounting from placement

            if (!templateBlock.AccountInternal.Any() && !accountIdsFromPlacement.IsNullOrEmpty())
            {
                foreach (int accountId in accountIdsFromPlacement)
                {
                    TryAddAccountInternalToTimeScheduleTemplateBlock(templateBlock, GetAccountInternalWithAccountFromCache(accountId));
                }
            }

            #endregion

            #region Prio 3: Accounting from template schedule

            if (!templateBlock.AccountInternal.Any() && !accountIdsFromTemplateSchedule.IsNullOrEmpty())
            {
                foreach (int accountId in accountIdsFromTemplateSchedule)
                {
                    AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(accountId);
                    if (accountInternal != null)
                        templateBlock.AccountInternal.Add(accountInternal);
                }
            }

            #endregion

            #endregion

            SetPredefinedAccountOnTemplateBlock(templateBlock);
            ApplyAccountingPrioOnTimeScheduleTemplateBlock(templateBlock, employee);
            SetAccountIdOnBlockFromAccounting(templateBlock, employee);
        }

        private bool UseSetPredefinedAccountOnTemplateBlock(TimeScheduleTemplateBlock templateBlock)
        {
            if (templateBlock == null || !templateBlock.AccountId.HasValue)
                return false;

            string key = $"UseSetPredefinedAccountOnTemplateBlock{templateBlock.AccountId}";

            bool? useSetPredefinedAccountOnTemplateBlock = BusinessMemoryCache<bool?>.Get(key);

            if (!useSetPredefinedAccountOnTemplateBlock.HasValue)
            {
                var accounts = GetAccountInternalsFromCache();

                if (!accounts.IsNullOrEmpty())
                {
                    var accountids = accounts.Where(w => w.HierarchyOnly).Select(s => s.AccountId).ToList();
                    if (accountids.Contains(templateBlock.AccountId.Value))
                    {
                        BusinessMemoryCache<bool?>.Set(key, false, 60 * 60);
                        return false;
                    }
                }

                BusinessMemoryCache<bool?>.Set(key, true, 60 * 60);
                return true;
            }
            else
                return useSetPredefinedAccountOnTemplateBlock.Value;
        }

        private void SetAccountIdOnBlockFromAccounting(TimeScheduleTemplateBlock block, Employee employee)
        {
            if (base.UseAccountHierarchyOnCompanyFromCache(this.entities, ActorCompanyId) && !block.AccountId.HasValue && !block.AccountInternal.IsNullOrEmpty())
            {
                // AccountId must be set on all shifts if using account hierarchy,
                // unless setting on employee group allows it
                bool allowShiftsWithoutAccount = employee.GetEmployment(block.Date)?.GetEmployeeGroup(block.Date)?.AllowShiftsWithoutAccount ?? false;
                if (allowShiftsWithoutAccount) return;

                var setting = GetCompanyNullableIntSettingFromCache(CompanySettingType.DefaultEmployeeAccountDimEmployee);

                if (setting.HasValue)
                {
                    var accountDims = GetAccountDimsFromCache();
                    var accountDim = accountDims.FirstOrDefault(f => f.AccountDimId == setting.Value);

                    if (accountDim != null)
                    {
                        var accountIds = block.AccountInternal.Select(s => s.AccountId).ToList();
                        var internalAccounts = GetAccountInternalsFromCache();
                        var accountInternal = internalAccounts.Where(w => accountDim.AccountDimId == w.AccountDimId && accountIds.Contains(w.AccountId)).ToList();

                        if (accountInternal.Count == 1)
                            block.AccountId = accountInternal.First().AccountId;
                    }
                }
            }
        }

        private static readonly Dictionary<int, TimeScheduleTemplateHead> _templateHeadCache = new Dictionary<int, TimeScheduleTemplateHead>();
        private void ClearAccountIdIfTemplateHeadIsNotPersonal(List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (!scheduleBlocks.IsNullOrEmpty())
                scheduleBlocks.ForEach(scheduleBlock => ClearAccountIdIfTemplateHeadIsNotPersonal(scheduleBlock));
        }
        private void ClearAccountIdIfTemplateHeadIsNotPersonal(TimeScheduleTemplateBlock scheduleBlock)
        {
            if (scheduleBlock == null || !scheduleBlock.AccountId.HasValue)
                return;

            if (!_templateHeadCache.TryGetValue(scheduleBlock.TimeScheduleTemplatePeriodId.Value, out TimeScheduleTemplateHead templateHead))
            {
                templateHead = entities.TimeScheduleTemplatePeriod
                    .Where(w => w.TimeScheduleTemplatePeriodId == scheduleBlock.TimeScheduleTemplatePeriodId)
                    .Select(w => w.TimeScheduleTemplateHead)
                    .FirstOrDefault();

                _templateHeadCache[scheduleBlock.TimeScheduleTemplatePeriodId.Value] = templateHead;
            }

            if (templateHead != null && !templateHead.EmployeeId.HasValue)
            {
                scheduleBlock.AccountId = null;
            }
        }

        private void SetPredefinedAccountOnTemplateBlock(TimeScheduleTemplateBlock templateBlock)
        {
            if (!UseSetPredefinedAccountOnTemplateBlock(templateBlock))
                return;

            AccountInternal predefinedAccountInternal = GetAccountInternalWithAccountFromCache(templateBlock.AccountId.Value);
            if (predefinedAccountInternal == null || predefinedAccountInternal.Account == null)
                return;
            if (templateBlock.AccountInternal.Any(i => i.AccountId == predefinedAccountInternal.AccountId))
                return;

            AccountInternal existingAccountInternalInDim = null;
            foreach (AccountInternal accountInternal in templateBlock.AccountInternal)
            {
                if (!accountInternal.AccountReference.IsLoaded)
                    accountInternal.AccountReference.Load();

                if (accountInternal.Account != null && accountInternal.Account.AccountDimId == predefinedAccountInternal.Account.AccountDimId)
                {
                    existingAccountInternalInDim = accountInternal;
                    break;
                }
            }

            if (existingAccountInternalInDim != null)
            {
                List<AccountInternal> validAccountInternals = templateBlock.AccountInternal.Where(i => i.AccountId != existingAccountInternalInDim.AccountId).ToList();
                templateBlock.AccountInternal.Clear();
                templateBlock.AccountInternal.AddRange(validAccountInternals);
            }
            templateBlock.AccountInternal.Add(predefinedAccountInternal);

            if (predefinedAccountInternal.Account.ParentAccountId.HasValue)
            {
                AccountInternal predefinedAccountInternalParent = GetAccountInternalWithAccountFromCache(predefinedAccountInternal.Account.ParentAccountId.Value);
                if (predefinedAccountInternalParent == null || predefinedAccountInternalParent.Account == null)
                    return;
                if (templateBlock.AccountInternal.Any(i => i.AccountId == predefinedAccountInternalParent.AccountId))
                    return;

                AccountInternal existingAccountInternalParentInDim = null;
                foreach (AccountInternal accountInternal in templateBlock.AccountInternal)
                {
                    if (!accountInternal.AccountReference.IsLoaded)
                        accountInternal.AccountReference.Load();

                    if (accountInternal.Account != null && accountInternal.Account.AccountDimId == predefinedAccountInternalParent.Account.AccountDimId)
                    {
                        existingAccountInternalParentInDim = accountInternal;
                        break;
                    }
                }

                if (existingAccountInternalParentInDim != null)
                {
                    List<AccountInternal> validAccountInternals = templateBlock.AccountInternal.Where(i => i.AccountId != existingAccountInternalParentInDim.AccountId).ToList();
                    templateBlock.AccountInternal.Clear();
                    templateBlock.AccountInternal.AddRange(validAccountInternals);
                }
                templateBlock.AccountInternal.Add(predefinedAccountInternalParent);

                if (!predefinedAccountInternalParent.AccountReference.IsLoaded)
                    predefinedAccountInternalParent.AccountReference.Load();

                if (predefinedAccountInternalParent.Account.ParentAccountId.HasValue)
                {
                    AccountInternal predefinedAccountInternalParentParent = GetAccountInternalWithAccountFromCache(predefinedAccountInternalParent.Account.ParentAccountId.Value);
                    if (predefinedAccountInternalParentParent == null || predefinedAccountInternalParentParent.Account == null)
                        return;
                    if (templateBlock.AccountInternal.Any(i => i.AccountId == predefinedAccountInternalParentParent.AccountId))
                        return;

                    AccountInternal existingAccountInternalParentParentInDim = null;
                    foreach (AccountInternal accountInternal in templateBlock.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();

                        if (accountInternal.Account != null && accountInternal.Account.AccountDimId == predefinedAccountInternalParentParent.Account.AccountDimId)
                        {
                            existingAccountInternalParentParentInDim = accountInternal;
                            break;
                        }
                    }

                    if (existingAccountInternalParentParentInDim != null)
                    {
                        List<AccountInternal> validAccountInternals = templateBlock.AccountInternal.Where(i => i.AccountId != existingAccountInternalParentParentInDim.AccountId).ToList();
                        templateBlock.AccountInternal.Clear();
                        templateBlock.AccountInternal.AddRange(validAccountInternals);
                    }
                    templateBlock.AccountInternal.Add(predefinedAccountInternalParentParent);
                }
            }
        }

        private void ApplyAccountingOnTimeBlockFromTemplateBlocks(Employee employee, List<TimeScheduleTemplateBlock> scheduleBlocks, List<TimeBlock> timeBlocks, bool setAccountingOnExcessToDockedTimeBlockOrNearestSchedule = false, TimeIntervalAccountingDTO timeIntervalAccounting = null)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            timeBlocks = timeBlocks.OrderBy(b => b.StartTime).ToList();
            scheduleBlocks = scheduleBlocks.OrderBy(b => b.StartTime).ToList();

            var timeBlockTemplateBlockDict = timeBlocks.ToDictionary(timeBlock => timeBlock, timeBlock => scheduleBlocks.GetTemplateBlockFromTimeBlock(timeBlock));
            var scheduleWork = scheduleBlocks.GetWork();
            var scheduleIn = scheduleWork.GetScheduleIn();
            var scheduleOut = scheduleWork.GetScheduleIn();
            var timeIntervalAccounts = timeIntervalAccounting != null ? AccountManager.GetAccountInternals(entities, timeIntervalAccounting.AccountInternalIds, actorCompanyId, loadAccount: true) : new List<AccountInternal>();

            timeBlockTemplateBlockDict.Where(p => p.Value != null).ToList().ForEach(p => ApplyAccounting(p));
            timeBlockTemplateBlockDict.Where(p => p.Value == null).ToList().ForEach(p => ApplyAccounting(p));

            void ApplyAccounting(KeyValuePair<TimeBlock, TimeScheduleTemplateBlock> pair)
            {
                var timeBlock = pair.Key;
                var templateBlock = pair.Value;

                if (timeIntervalAccounting != null && timeIntervalAccounting.IsInInterval(timeBlock.StartTime, timeBlock.StopTime))
                    timeBlock.SetDeviationAccounts(timeIntervalAccounts);

                if (templateBlock == null && setAccountingOnExcessToDockedTimeBlockOrNearestSchedule)
                {
                    if (timeBlock.StartTime < scheduleIn && timeBlock.StopTime <= scheduleIn && timeBlockTemplateBlockDict.Keys.Any(tb => tb.StartTime == timeBlock.StopTime))
                        AddAccountInternals(timeBlockTemplateBlockDict.FirstOrDefault(p => p.Key.StartTime == timeBlock.StopTime).Key.AccountInternal.ToList()); //Docked to schedule-in, set accounting from next timeBlock
                    else if (timeBlock.StartTime >= scheduleOut && timeBlock.StopTime > scheduleOut && timeBlockTemplateBlockDict.Keys.Any(tb => tb.StopTime == timeBlock.StartTime))
                        AddAccountInternals(timeBlockTemplateBlockDict.FirstOrDefault(p => p.Key.StopTime == timeBlock.StartTime).Key.AccountInternal.ToList()); //Docked to schedule-in, set accounting from prev timeBlock
                    else if (timeBlock.StartTime < scheduleIn && timeBlock.StopTime < scheduleIn)
                        templateBlock = scheduleWork.FirstOrDefault(); //Excess before schedule-in, set accounting from first scheduleBlock
                    else if (timeBlock.StartTime > scheduleOut && timeBlock.StopTime > scheduleOut)
                        templateBlock = scheduleWork.LastOrDefault(); //Excess before schedule-in, set accounting from last scheduleBlock
                }

                ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, templateBlock, employee);

                void AddAccountInternals(List<AccountInternal> accountInternalsFromConnectedBlock)
                {
                    if (!accountInternalsFromConnectedBlock.IsNullOrEmpty())
                        accountInternalsFromConnectedBlock.ForEach(accountInternal => AddAccountInternal(accountInternal));
                }
                void AddAccountInternal(AccountInternal accountInternal)
                {
                    if (accountInternal?.Account != null && (timeBlock.DeviationAccounts == null || !timeBlock.DeviationAccounts.Any(ai => ai.Account.AccountDimId == accountInternal.Account.AccountDimId)))
                    {
                        if (timeBlock.DeviationAccounts == null)
                            timeBlock.DeviationAccounts = new List<AccountInternal>();
                        timeBlock.DeviationAccounts.Add(accountInternal);
                    }                        
                }
            }
        }

        private void ApplyAccountingOnTimeBlockFromTemplateBlock(TimeBlock timeBlock, TimeScheduleTemplateBlock scheduleBlock, Employee employee, int? accountStdId = null, List<AccountInternal> accountInternals = null)
        {
            if (timeBlock == null || employee == null)
                return;

            #region AccountStd

            TryAddAccountStdToTimeBlock(timeBlock, accountStdId);

            #endregion

            #region AccountInternal

            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
            timeBlock.AccountInternal.Clear();

            if (accountInternals.IsNullOrEmpty() && timeBlock.HasDeviationAccounts)
                accountInternals = LoadDeviationAccounts(timeBlock);

            //Prio 1: Accounting from passed collection
            if (!timeBlock.AccountInternal.Any())
            {
                AddAccountInternalsToTimeBlock(timeBlock, accountInternals);
            }

            //Prio 2: Accounting from schedule
            if (scheduleBlock != null)
            {
                TryLoadAccountInternalsOnTimeScheduleTemplateBlock(ref scheduleBlock);
                AddAccountInternalsToTimeBlock(timeBlock, scheduleBlock.AccountInternal);
            }

            #endregion

            ApplyAccountingPrioOnTimeBlock(timeBlock, employee, overwriteAccountStd: true);
        }

        private void ApplyAccountingOnTimeBlockFromTemplateBlockIfMissing(List<TimeBlock> timeBlocks, TimeScheduleTemplateBlock templateBlock, Employee employee)
        {
            timeBlocks = timeBlocks?.Where(i => i.AccountInternal.IsNullOrEmpty()).ToList() ?? new List<TimeBlock>(); //Only missing accounting
            if (timeBlocks.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, templateBlock, employee);
            }
        }

        private void ApplyAccountingOnNewTimeBlockFromNearestTimeBlockIfMissing(List<TimeBlock> timeBlocks, Employee employee)
        {
            if (timeBlocks.IsNullOrEmpty() || employee == null)
                return;

            foreach (TimeBlock timeBlock in timeBlocks.GetNew(excludeBreak: false))
            {
                ApplyAccountingOnTimeBlockFromNearestTimeBlockIfMissing(timeBlock, timeBlocks, employee);
            }
        }

        private void ApplyAccountingOnTimeBlockFromNearestTimeBlockIfMissing(TimeBlock timeBlock, List<TimeBlock> timeBlocks, Employee employee, List<AccountInternal> accountInternals = null)
        {
            if (timeBlocks == null)
                return;
            if (!timeBlock.AccountInternal.IsNullOrEmpty())
                return; //Only missing accounting

            TimeBlock nearestTimeBlock = timeBlocks.GetNearest(timeBlock, excludeNew: true, excludeBreaks: true);

            #region AccountStd

            TryAddAccountStdToTimeBlock(timeBlock, nearestTimeBlock?.AccountStdId);

            #endregion

            #region AccountInternal

            // Load existing accounting
            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);

            // Clear existing accounting
            timeBlock.AccountInternal.Clear();
            if (timeBlock.AccountInternal == null)
                timeBlock.AccountInternal = new EntityCollection<AccountInternal>();

            if (accountInternals == null)
                accountInternals = new List<AccountInternal>();

            #region Prio 1: Accounting from passed collection

            if (!timeBlock.AccountInternal.Any())
            {
                AddAccountInternalsToTimeBlock(timeBlock, accountInternals);
            }

            #endregion

            #region Prio 2: Accounting from nearest TimeBlock

            if (!timeBlock.AccountInternal.Any() && nearestTimeBlock != null)
            {
                TryLoadAccountInternalsOnTimeBlock(ref nearestTimeBlock);
                AddAccountInternalsToTimeBlock(timeBlock, nearestTimeBlock.AccountInternal);
            }

            #endregion

            #endregion

            ApplyAccountingPrioOnTimeBlock(timeBlock, employee);
        }

        private void ApplyAccountingOnTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, Employee employee, DateTime date, PayrollProduct payrollProduct, TimeBlock timeBlock = null, int? accountStdId = null, List<AccountInternal> accountInternals = null, bool setAccountStd = true, bool setAccountInternal = true, int projectId = 0)
        {
            if (timePayrollTransaction == null || employee == null || payrollProduct == null)
                return;
            if (!setAccountStd && !setAccountInternal)
                return;

            #region Prereq

            PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, date);
            if (payrollProductSetting != null)
                TryLoadAccountStdOnPayrollProductSetting(payrollProductSetting);

            PayrollProductAccountStd payrollProductAccountStd = payrollProductSetting?.PayrollProductAccountStd?.FirstOrDefault(i => i.Type == (int)ProductAccountType.Purchase);
            if (payrollProductAccountStd != null)
                TryLoadAccountInternalOnPayrollProductAccountStd(payrollProductAccountStd);

            if (setAccountInternal)
            {
                TryLoadAccountInternalsOnTimePayrollTransaction(timePayrollTransaction);
                timePayrollTransaction.AccountInternal.Clear();
                if (!accountInternals.IsNullOrEmpty())
                    timePayrollTransaction.AccountInternal.AddRange(accountInternals);
            }
            if (setAccountStd)
            {
                if (accountStdId.HasValue)
                    timePayrollTransaction.AccountStdId = accountStdId.Value;
                else
                    timePayrollTransaction.AccountStdId = 0;
            }

            #endregion

            #region Perform

            AccountingPrioDTO accountingPrio = null;

            List<AccountDim> accountDims = GetAccountDimsFromCache();
            foreach (AccountDim accountDim in accountDims.OrderBy(i => i.AccountDimNr))
            {
                if (accountDim.IsStandard)
                {
                    #region AccountStd

                    if (!setAccountStd)
                        continue;

                    int payrollProductSettingPrio = payrollProductSetting != null ? AccountManager.GetPayrollProductAccountingPrio(payrollProductSetting, accountDim.AccountDimNr) : (int)TermGroup_PayrollProductAccountingPrio.NoAccounting;

                    #region Prio 1: Get from PayrollProduct

                    if (!timePayrollTransaction.HasAccountStd() && payrollProductSettingPrio != (int)TermGroup_PayrollProductAccountingPrio.NotUsed && payrollProductAccountStd != null && payrollProductAccountStd.AccountId.HasValue)
                        TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, payrollProductAccountStd.AccountId);

                    #endregion

                    #region Prio 2: Get from TimeBlock

                    if (!timePayrollTransaction.HasAccountStd() && timeBlock != null)
                    {
                        TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                        TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, timeBlock.AccountStdId);
                    }

                    #endregion

                    #region Prio 3: Get from AccountingPrio

                    if (!timePayrollTransaction.HasAccountStd())
                    {
                        if (accountingPrio == null)
                            accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, date);
                        if (accountingPrio != null)
                            TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, accountingPrio.AccountId);
                    }

                    #endregion

                    #region Prio 4: Get from company setting

                    if (!timePayrollTransaction.HasAccountStd())
                    {
                        if (payrollProduct.IsSupplementChargeCredit())
                            TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountPayrollOwnSupplementCharge));
                        else if (payrollProduct.IsEmploymentTaxDebit())
                            TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupIncome));
                        else
                            TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost));
                    }

                    #endregion

                    #endregion
                }
                else
                {
                    #region AccountInternal

                    if (!setAccountInternal)
                        continue;

                    int payrollProductSettingPrio = AccountManager.GetPayrollProductAccountingPrio(payrollProductSetting, accountDim.AccountDimNr);

                    #region Prio 1: Check that PayrollProduct hasnt set NoAccounting

                    if (payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.NoAccounting)
                    {
                        timePayrollTransaction.RemoveAccountInternal(accountDim.AccountDimId);
                        continue;
                    }

                    #endregion

                    #region Prio 2: Get from PayrollProduct

                    if (payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.PayrollProduct && !timePayrollTransaction.HasAccountInternal(accountDim.AccountDimId) && payrollProductAccountStd != null)
                        TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, payrollProductAccountStd.GetAccountInternal(accountDim.AccountDimId));

                    #endregion

                    #region Prio 3: Get from TimeBlock

                    if (payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.NotUsed && !timePayrollTransaction.HasAccountInternal(accountDim.AccountDimId) && timeBlock != null)
                    {
                        TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                        TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, timeBlock.GetAccountInternal(accountDim.AccountDimId));
                    }

                    #endregion

                    #region Prio 4: Get from AccountingPrio

                    if (!timePayrollTransaction.HasAccountInternal(accountDim.AccountDimId))
                    {
                        if (accountingPrio == null)
                            accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, date, projectId: projectId);
                        if (accountingPrio != null)
                            TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, GetAccountInternalWithAccountFromCache(accountingPrio.GetAccountInternalId(accountDim.AccountDimId)));
                    }

                    #endregion

                    #endregion
                }
            }

            #endregion
        }

        private void ApplyAccountingOnTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, TimeTransactionItem item, List<AccountInternal> accountInternals)
        {
            if (timePayrollTransaction == null || item == null)
                return;

            #region Prereq

            // Load existing accounting
            TryLoadAccountInternalsOnTimePayrollTransaction(timePayrollTransaction);

            // Clear existing accounting
            timePayrollTransaction.AccountInternal.Clear();
            if (timePayrollTransaction.AccountInternal == null)
                timePayrollTransaction.AccountInternal = new EntityCollection<AccountInternal>();

            #endregion

            #region AccountStd

            if (item.Dim1Id != 0)
                TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, GetAccountStdWithAccountFromCache(item.Dim1Id));

            #endregion

            #region AccountInternals

            if (accountInternals != null)
            {
                if (item.Dim2Id != 0)
                    TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim2Id));
                if (item.Dim3Id != 0)
                    TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim3Id));
                if (item.Dim4Id != 0)
                    TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim4Id));
                if (item.Dim5Id != 0)
                    TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim5Id));
                if (item.Dim6Id != 0)
                    TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim6Id));
            }

            #endregion
        }

        private void ApplyAccountingOnTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, AttestPayrollTransactionDTO timePayrollTransactionDTO, List<AccountInternal> accountInternals)
        {
            if (timePayrollTransaction == null || timePayrollTransactionDTO == null)
                return;

            #region Prereq

            // Load existing accounting
            TryLoadAccountInternalsOnTimePayrollTransaction(timePayrollTransaction);

            // Clear existing accounting
            timePayrollTransaction.AccountInternal.Clear();
            if (timePayrollTransaction.AccountInternal == null)
                timePayrollTransaction.AccountInternal = new EntityCollection<AccountInternal>();

            #endregion

            #region AccountStd

            if (timePayrollTransactionDTO.AccountStdId != 0)
                TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, GetAccountStdWithAccountFromCache(timePayrollTransactionDTO.AccountStdId));

            #endregion

            #region AccountInternals

            if (accountInternals != null && timePayrollTransactionDTO.AccountInternalIds != null)
            {
                foreach (int accountInternalId in timePayrollTransactionDTO.AccountInternalIds)
                {
                    if (accountInternalId != 0)
                        TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountInternals.FirstOrDefault(a => a.AccountId == accountInternalId));
                }
            }

            #endregion
        }

        private void ApplyAccountingOnTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, Employee employee, DateTime date, PayrollProduct payrollProduct, TimeBlock timeBlock = null, bool setAccountStd = true, bool setAccountInternal = true)
        {
            if (timePayrollScheduleTransaction == null || employee == null || payrollProduct == null)
                return;
            if (!setAccountStd && !setAccountInternal)
                return;

            #region Prereq

            PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, date);
            TryLoadAccountStdOnPayrollProductSetting(payrollProductSetting);

            PayrollProductAccountStd payrollProductAccountStd = payrollProductSetting?.PayrollProductAccountStd?.FirstOrDefault(i => i.Type == (int)ProductAccountType.Purchase);
            TryLoadAccountInternalOnPayrollProductAccountStd(payrollProductAccountStd);

            AccountingPrioDTO accountingPrio = null;

            if (setAccountInternal)
            {
                // Load existing accounting
                TryLoadAccountInternalsOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction);

                // Clear existing accounting
                timePayrollScheduleTransaction.AccountInternal.Clear();
                if (timePayrollScheduleTransaction.AccountInternal == null)
                    timePayrollScheduleTransaction.AccountInternal = new EntityCollection<AccountInternal>();
            }

            #endregion

            int accountDimCounter = 1;
            List<AccountDim> accountDims = GetAccountDimsFromCache();
            foreach (AccountDim accountDim in accountDims.OrderBy(i => i.AccountDimNr))
            {
                if (accountDim.IsStandard)
                {
                    #region AccountStd

                    if (setAccountStd)
                    {
                        int payrollProductSettingPrio = payrollProductSetting != null ? AccountManager.GetPayrollProductAccountingPrio(payrollProductSetting, accountDim.AccountDimNr) : (int)TermGroup_PayrollProductAccountingPrio.NoAccounting;

                        if (!timePayrollScheduleTransaction.HasAccountStd() && payrollProductSettingPrio != (int)TermGroup_PayrollProductAccountingPrio.NotUsed && payrollProductAccountStd != null && payrollProductAccountStd.AccountId.HasValue)
                        {
                            //Prio 1: Get from PayrollProduct
                            TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, payrollProductAccountStd.AccountId);
                        }
                        if (!timePayrollScheduleTransaction.HasAccountStd() && timeBlock != null)
                        {
                            //Prio 2: Get from TimeBlock
                            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                            TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, timeBlock.AccountStdId);
                        }
                        if (!timePayrollScheduleTransaction.HasAccountStd())
                        {
                            //Prio 3: Get from AccountingPrio
                            if (accountingPrio == null)
                                accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, date);
                            if (accountingPrio != null)
                                TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountingPrio.AccountId);
                        }
                        if (!timePayrollScheduleTransaction.HasAccountStd())
                        {
                            //Prio 4: Get from company setting
                            if (payrollProduct.IsSupplementChargeCredit())
                                TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountPayrollOwnSupplementCharge));
                            else if (payrollProduct.IsEmploymentTaxDebit())
                                TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupIncome));
                            else
                                TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost));
                        }
                    }

                    #endregion
                }
                else
                {
                    #region AccountInternal

                    if (setAccountInternal)
                    {
                        int payrollProductSettingPrio = AccountManager.GetPayrollProductAccountingPrio(payrollProductSetting, accountDim.AccountDimNr);

                        if (payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.NoAccounting)
                        {
                            //Prio 1: Check that PayrollProduct hasnt set NoAccounting
                            timePayrollScheduleTransaction.RemoveAccountInternal(accountDim.AccountDimId);
                            continue;
                        }
                        if (!timePayrollScheduleTransaction.HasAccountInternal(accountDim.AccountDimId) && payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.PayrollProduct && payrollProductAccountStd != null)
                        {
                            //Prio 2: Get from PayrollProduct
                            TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, payrollProductAccountStd.GetAccountInternal(accountDim.AccountDimId));
                        }
                        if (!timePayrollScheduleTransaction.HasAccountInternal(accountDim.AccountDimId) && payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.NotUsed && timeBlock != null)
                        {
                            //Prio 3: Get from TimeBlock
                            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                            TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, timeBlock.GetAccountInternal(accountDim.AccountDimId));
                        }
                        if (!timePayrollScheduleTransaction.HasAccountInternal(accountDim.AccountDimId))
                        {
                            //Prio 4: Get from AccountingPrio
                            if (accountingPrio == null)
                                accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, date);
                            if (accountingPrio != null)
                                TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, GetAccountInternalWithAccountFromCache(accountingPrio.GetAccountInternalId(accountDim.AccountDimId)));
                        }
                    }

                    #endregion
                }

                accountDimCounter++;
            }
        }

        private void ApplyAccountingOnTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, TimeTransactionItem item, List<AccountInternal> accountInternals)
        {
            if (timePayrollScheduleTransaction == null || item == null)
                return;

            #region Prereq

            // Load existing accounting
            TryLoadAccountInternalsOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction);

            // Clear existing accounting
            timePayrollScheduleTransaction.AccountInternal.Clear();
            if (timePayrollScheduleTransaction.AccountInternal == null)
                timePayrollScheduleTransaction.AccountInternal = new EntityCollection<AccountInternal>();

            #endregion

            #region AccountStd

            if (item.Dim1Id != 0)
                TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, GetAccountStdWithAccountFromCache(item.Dim1Id));

            #endregion

            #region AccountInternals

            if (accountInternals != null)
            {
                if (item.Dim2Id != 0)
                    TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim2Id));
                if (item.Dim3Id != 0)
                    TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim3Id));
                if (item.Dim4Id != 0)
                    TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim4Id));
                if (item.Dim5Id != 0)
                    TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim5Id));
                if (item.Dim6Id != 0)
                    TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim6Id));
            }

            #endregion
        }

        private void ApplyAccountingOnTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, AttestPayrollTransactionDTO timePayrollScheduleTransactionDTO, List<AccountInternal> accountInternals)
        {
            if (timePayrollScheduleTransaction == null || timePayrollScheduleTransactionDTO == null)
                return;

            #region Prereq

            // Load existing accounting
            TryLoadAccountInternalsOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction);

            // Clear existing accounting
            timePayrollScheduleTransaction.AccountInternal.Clear();
            if (timePayrollScheduleTransaction.AccountInternal == null)
                timePayrollScheduleTransaction.AccountInternal = new EntityCollection<AccountInternal>();

            #endregion

            #region AccountStd

            if (timePayrollScheduleTransactionDTO.AccountStdId != 0)
                TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, GetAccountStdWithAccountFromCache(timePayrollScheduleTransactionDTO.AccountStdId));

            #endregion

            #region AccountInternals

            if (accountInternals != null && timePayrollScheduleTransactionDTO.AccountInternalIds != null)
            {
                foreach (int accountInternalId in timePayrollScheduleTransactionDTO.AccountInternalIds)
                {
                    if (accountInternalId != 0)
                        TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountInternals.FirstOrDefault(a => a.AccountId == accountInternalId));
                }
            }

            #endregion
        }

        private void ApplyAccountingOnTimeInvoiceTransaction(TimeInvoiceTransaction timeInvoiceTransaction, TimeBlock timeBlock)
        {
            if (timeInvoiceTransaction == null)
                return;

            #region Prereq

            // Load existing accounting
            TryLoadAccountInternalsOnTimeInvoiceTransaction(timeInvoiceTransaction);

            // Clear existing accounting
            timeInvoiceTransaction.AccountInternal.Clear();
            if (timeInvoiceTransaction.AccountInternal == null)
                timeInvoiceTransaction.AccountInternal = new EntityCollection<AccountInternal>();

            #endregion

            #region AccountStd

            if (!timeInvoiceTransaction.HasAccountStd() && timeBlock != null)
            {
                //Prio 1: Get from TimeBlock
                TryAddAccountStdToTimeInvoiceTransaction(timeInvoiceTransaction, timeBlock.AccountStdId);
            }
            if (!timeInvoiceTransaction.HasAccountStd())
            {
                //Prio 3: Get from company setting
                TryAddAccountStdToTimeInvoiceTransaction(timeInvoiceTransaction, GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost));
            }

            #endregion

            #region AccountInternal

            if (!timeInvoiceTransaction.HasAccountInternals() && timeBlock != null)
            {
                //Prio 1: Get from TimeBlock
                TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                AddAccountInternalsToTimeInvoiceTransaction(timeInvoiceTransaction, timeBlock.AccountInternal);
            }

            #endregion
        }

        private void ApplyAccountingOnTimeInvoiceTransaction(TimeInvoiceTransaction timeInvoiceTransaction, TimeTransactionItem item, List<AccountInternal> accountInternals)
        {
            if (timeInvoiceTransaction == null || item == null)
                return;

            #region Prereq

            // Load existing accounting
            TryLoadAccountInternalsOnTimeInvoiceTransaction(timeInvoiceTransaction);

            // Clear existing accounting
            timeInvoiceTransaction.AccountInternal.Clear();
            if (timeInvoiceTransaction.AccountInternal == null)
                timeInvoiceTransaction.AccountInternal = new EntityCollection<AccountInternal>();

            #endregion

            #region AccountStd

            if (item.Dim1Id != 0)
                TryAddAccountStdToTimeInvoiceTransaction(timeInvoiceTransaction, GetAccountStdWithAccountFromCache(item.Dim1Id));

            #endregion

            #region AccountInternals

            if (accountInternals != null)
            {
                if (item.Dim2Id != 0)
                    TryAddAccountInternalToTimeInvoiceTransaction(timeInvoiceTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim2Id));
                if (item.Dim3Id != 0)
                    TryAddAccountInternalToTimeInvoiceTransaction(timeInvoiceTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim3Id));
                if (item.Dim4Id != 0)
                    TryAddAccountInternalToTimeInvoiceTransaction(timeInvoiceTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim4Id));
                if (item.Dim5Id != 0)
                    TryAddAccountInternalToTimeInvoiceTransaction(timeInvoiceTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim5Id));
                if (item.Dim6Id != 0)
                    TryAddAccountInternalToTimeInvoiceTransaction(timeInvoiceTransaction, accountInternals.FirstOrDefault(a => a.AccountId == item.Dim6Id));
            }

            #endregion
        }

        private void ApplyAccountingOnTimeTransactionItem(TimeTransactionItem timeTransactionItem, Employee employee, PayrollProduct payrollProduct, DateTime date, TimeBlock timeBlock = null, bool setAccountStd = true, bool setAccountInternal = true, int projectId = 0)
        {
            if (timeTransactionItem == null)
                return;

            #region Prereq

            PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, date);
            TryLoadAccountStdOnPayrollProductSetting(payrollProductSetting);

            PayrollProductAccountStd payrollProductAccountStd = payrollProductSetting?.PayrollProductAccountStd?.FirstOrDefault(i => i.Type == (int)ProductAccountType.Purchase);
            TryLoadAccountInternalOnPayrollProductAccountStd(payrollProductAccountStd);

            AccountingPrioDTO accountingPrio = null;

            #endregion

            int accountDimCounter = 1;
            List<AccountDim> accountDims = GetAccountDimsFromCache();
            foreach (AccountDim accountDim in accountDims.OrderBy(i => i.AccountDimNr))
            {
                if (accountDim.IsStandard)
                {
                    #region AccountStd

                    if (setAccountStd)
                    {
                        int payrollProductSettingPrio = AccountManager.GetPayrollProductAccountingPrio(payrollProductSetting, accountDim.AccountDimNr);

                        if (!timeTransactionItem.HasAccountStd() && payrollProductSettingPrio != (int)TermGroup_PayrollProductAccountingPrio.NotUsed && payrollProductAccountStd != null && payrollProductAccountStd.AccountId.HasValue)
                        {
                            //Prio 2: Get from PayrollProduct
                            AddAccountStdToTimeTransactionItem(timeTransactionItem, GetAccountStdWithAccountFromCache(payrollProductAccountStd.AccountId));
                        }
                        if (!timeTransactionItem.HasAccountStd() && timeBlock != null)
                        {
                            //Prio 3: Get from TimeBlock
                            AddAccountStdToTimeTransactionItem(timeTransactionItem, (timeBlock.AccountStd != null && timeBlock.AccountStd.Account != null) ? timeBlock.AccountStd : GetAccountStdWithAccountFromCache(timeBlock.AccountStdId));
                        }
                        if (!timeTransactionItem.HasAccountStd())
                        {
                            //Prio 4: Get from AccountingPrio
                            if (accountingPrio == null)
                                accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, date, projectId: projectId);
                            if (accountingPrio != null)
                                AddAccountStdToTimeTransactionItem(timeTransactionItem, GetAccountStdWithAccountFromCache(accountingPrio.AccountId));
                        }
                    }

                    #endregion
                }
                else
                {
                    #region AccountInternal

                    if (setAccountInternal)
                    {
                        int payrollProductSettingPrio = AccountManager.GetPayrollProductAccountingPrio(payrollProductSetting, accountDim.AccountDimNr);

                        //Prio 1: Check that PayrollProduct hasnt set NoAccounting
                        if (payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.NoAccounting)
                        {
                            timeTransactionItem.RemoveAccountInternal(accountDim.AccountDimId);
                            continue;
                        }
                        if (!timeTransactionItem.HasAccountInternal(accountDimCounter) && payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.PayrollProduct && payrollProductAccountStd != null)
                        {
                            //Prio 2: Get from PayrollProduct
                            AddAccountInternalToTimeTransactionItem(timeTransactionItem, payrollProductAccountStd.GetAccountInternal(accountDim.AccountDimId), accountDimCounter);
                        }
                        if (!timeTransactionItem.HasAccountInternal(accountDimCounter) && payrollProductSettingPrio == (int)TermGroup_PayrollProductAccountingPrio.NotUsed && timeBlock != null)
                        {
                            //Prio 3: Get from TimeBlock
                            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                            AddAccountInternalToTimeTransactionItem(timeTransactionItem, timeBlock.GetAccountInternal(accountDim.AccountDimId), accountDimCounter);
                        }
                        if (!timeTransactionItem.HasAccountInternal(accountDimCounter))
                        {
                            //Prio 4: Get from AccountingPrio
                            if (accountingPrio == null)
                                accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, date, projectId: projectId);
                            if (accountingPrio != null)
                                AddAccountInternalToTimeTransactionItem(timeTransactionItem, GetAccountInternalWithAccountFromCache(accountingPrio.GetAccountInternalId(accountDim.AccountDimId)), accountDimCounter);
                        }
                    }

                    #endregion
                }

                accountDimCounter++;
            }
        }

        private void ApplyAccountingOnTimeTransactionItem(TimeTransactionItem timeTransactionItem, TimeBlock timeBlock = null, bool setAccountStd = true, bool setAccountInternal = true)
        {
            if (timeTransactionItem == null || timeBlock == null)
                return;

            int accountDimCounter = 1;
            List<AccountDim> accountDims = GetAccountDimsFromCache();
            foreach (AccountDim accountDim in accountDims)
            {
                if (accountDim.IsStandard)
                {
                    #region AccountStd

                    if (setAccountStd)
                    {
                        if (!timeTransactionItem.HasAccountStd() && timeBlock != null)
                        {
                            //Prio 1: Get from TimeBlock
                            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                            AddAccountStdToTimeTransactionItem(timeTransactionItem, (timeBlock.AccountStd != null && timeBlock.AccountStd.Account != null) ? timeBlock.AccountStd : GetAccountStdWithAccountFromCache(timeBlock.AccountStdId));
                        }
                        if (!timeTransactionItem.HasAccountStd())
                        {
                            //Prio 2: Get from AccountingPrio
                            AddAccountStdToTimeTransactionItem(timeTransactionItem, GetAccountStdWithAccountFromCache(GetCompanyIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost)));
                        }
                    }

                    #endregion
                }
                else
                {
                    #region AccountInternal

                    if (setAccountInternal)
                    {
                        if (!timeTransactionItem.HasAccountInternal(accountDimCounter) && timeBlock != null)
                        {
                            //Prio 1: Get from TimeBlock
                            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);
                            AddAccountInternalToTimeTransactionItem(timeTransactionItem, timeBlock.GetAccountInternal(accountDim.AccountDimId), accountDim.AccountDimId);
                        }
                        else if (!timeTransactionItem.HasAccountInternal(accountDimCounter))
                        {
                            //Prio 2: Get from AccountingPrio
                            //Not implemented
                        }
                    }

                    #endregion
                }

                accountDimCounter++;
            }
        }

        private DateTime GetPayrollAccountingDateIfEmployeeNotEmployedOnTransactionDate(TimePeriod timePeriod, Employee employee, DateTime transactionDate)
        {
            if (timePeriod == null || employee == null)
                return transactionDate;

            DateTime accountingDate = transactionDate;
            if (employee.GetEmployment(transactionDate) == null && timePeriod.PayrollStartDate.HasValue && timePeriod.PayrollStopDate.HasValue)
            {
                // Fix: innevarande månad problem (anställningen börjar day augusti och man ska ha lön day augusti men avräkningen är juli)
                DateTime? latestDate = employee.GetNearestEmployment(transactionDate)?.GetEndDate();

                if (!latestDate.HasValue)
                {
                    // Fix: om anställningen är inte avslutad och det inte finns någon ny anställning
                    latestDate = employee.GetNearestEmployment(transactionDate)?.DateFrom;
                }

                if (latestDate.HasValue)
                    accountingDate = latestDate.Value;
            }

            return accountingDate;
        }

        #endregion

        #region Apply Accounting from prio

        private void ApplyAccountingPrioOnTimeScheduleTemplateBlock(TimeScheduleTemplateBlock templateBlock, Employee employee)
        {
            if (templateBlock == null)
                return;

            bool useEmployeeAccountInPrio = GetCompanyBoolSettingFromCache(CompanySettingType.FallbackOnEmployeeAccountInPrio);
            if (templateBlock.HasAccountInternals() && !useEmployeeAccountInPrio)
                return;

            var accountingPrio = GetAccountingPrioByEmployeeFromCache(templateBlock.Date, employee);

            #region AccountStd

            // Not available

            #endregion

            #region AccountInternal

            if (accountingPrio?.AccountInternals != null)
            {
                // even if scheduleBlock has accountinternals, it will not remove those. It will only add on dims missing accounting.
                AddAccountInternalsToTimeScheduleTemplateBlock(templateBlock, GetAccountInternalsWithAccountFromCache(accountingPrio.AccountInternals));
            }
            else
            {
                // Not mandatory, dont set default
            }

            #endregion
        }

        private void ApplyAccountingPrioOnTimeBlocks(List<TimeBlock> timeBlocks, Employee employee, bool overwriteAccountStd = false, int projectId = 0)
        {
            if (timeBlocks.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                ApplyAccountingPrioOnTimeBlock(timeBlock, employee, overwriteAccountStd: overwriteAccountStd, projectId: projectId);
            }
        }

        private void ApplyAccountingPrioOnTimeBlock(TimeBlock timeBlock, Employee employee, bool overwriteAccountStd = false, int projectId = 0)
        {
            if (timeBlock == null || employee == null)
                return;
            if (!overwriteAccountStd && timeBlock.HasAccountStd() && timeBlock.HasAccountInternals())
                return;

            var accountingPrio = GetAccountingPrioByEmployeeFromCache(timeBlock.TimeBlockDate != null ? timeBlock.TimeBlockDate.Date : (DateTime?)null, employee, projectId);

            #region AccountStd

            if (overwriteAccountStd || !timeBlock.HasAccountStd())
            {
                if (accountingPrio != null && accountingPrio.AccountId.HasValue)
                {
                    timeBlock.AccountStd = GetAccountStdWithAccountFromCache(accountingPrio.AccountId.Value);
                }
                else
                {
                    timeBlock.AccountStdId = GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost);
                }
            }

            #endregion

            #region AccountInternal

            if (!timeBlock.HasAccountInternals())
            {
                if (accountingPrio != null && accountingPrio.AccountInternals != null)
                {
                    AddAccountInternalsToTimeBlock(timeBlock, GetAccountInternalsWithAccountFromCache(accountingPrio.AccountInternals));
                }
                else
                {
                    // Not mandatory, dont set default
                }
            }

            #endregion
        }

        private bool ApplyAccountingPrioOnTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, PayrollProduct payrollProduct, Employee employee, TimeBlockDate timeBlockDate, List<AccountDim> accountDims)
        {
            if (timePayrollTransaction == null || payrollProduct == null || employee == null || timeBlockDate == null || accountDims == null)
                return false;
            if (timePayrollTransaction.IsExcludedInRecalculateAccounting())
                return false;

            AccountingPrioDTO accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, timeBlockDate.Date);
            if (accountingPrio == null)
                return false;

            bool changed = false;
            if (TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, accountingPrio.AccountId))
                changed = true;
            if (TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountingPrio, accountDims))
                changed = true;
            return changed;
        }

        private bool ApplyAccountingPrioOnTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, PayrollProduct payrollProduct, Employee employee, TimeBlockDate timeBlockDate, List<AccountDim> accountDims)
        {
            if (timePayrollScheduleTransaction == null || payrollProduct == null || employee == null || timeBlockDate == null || accountDims == null)
                return false;
            if (timePayrollScheduleTransaction.IsExcludedInRecalculateAccounting())
                return false;

            AccountingPrioDTO accountingPrio = GetAccountingPrioByPayrollProductFromCache(payrollProduct, employee, timeBlockDate.Date);
            if (accountingPrio == null)
                return false;

            bool changed = false;
            if (TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountingPrio.AccountId))
                changed = true;
            if (TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountingPrio, accountDims))
                changed = true;
            return changed;
        }

        #endregion

        #region Add AccountStd

        private bool TryAddAccountStdToTimeBlock(TimeBlock timeBlock, int? accountStdId) //NOSONAR
        {
            if (timeBlock == null || !accountStdId.HasValue)
                return false;

            timeBlock.AccountStdId = accountStdId.Value;
            return true;
        }

        private bool TryAddAccountStdToTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, AccountStd accountStd) //NOSONAR
        {
            return TryAddAccountStdToTimePayrollTransaction(timePayrollTransaction, accountStd?.AccountId);
        }

        private bool TryAddAccountStdToTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, int? accountStdId) //NOSONAR
        {
            if (timePayrollTransaction == null || !accountStdId.HasValue)
                return false;

            timePayrollTransaction.AccountStdId = accountStdId.Value;
            return true;
        }

        private bool TryAddAccountStdToTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, AccountStd accountStd) //NOSONAR
        {
            return TryAddAccountStdToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountStd?.AccountId);
        }

        private bool TryAddAccountStdToTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, int? accountStdId) //NOSONAR
        {
            if (timePayrollScheduleTransaction == null || !accountStdId.HasValue)
                return false;

            timePayrollScheduleTransaction.AccountStdId = accountStdId.Value;
            return true;
        }

        private bool TryAddAccountStdToTimeInvoiceTransaction(TimeInvoiceTransaction timeInvoiceTransaction, AccountStd accountStd) //NOSONAR
        {
            return TryAddAccountStdToTimeInvoiceTransaction(timeInvoiceTransaction, accountStd?.AccountId);
        }

        private bool TryAddAccountStdToTimeInvoiceTransaction(TimeInvoiceTransaction timeInvoiceTransaction, int? accountStdId) //NOSONAR
        {
            if (timeInvoiceTransaction == null || !accountStdId.HasValue)
                return false;

            timeInvoiceTransaction.AccountStdId = accountStdId.Value;
            return true;
        }

        private void AddAccountStdToTimeTransactionItem(TimeTransactionItem timeTransactionItem, AccountStd accountStd)
        {
            if (timeTransactionItem == null || accountStd == null)
                return;

            if (!accountStd.AccountReference.IsLoaded)
                accountStd.AccountReference.Load();

            AddAccountStdToTimeTransactionItem(timeTransactionItem, accountStd.AccountId, accountStd.Account?.AccountNr ?? String.Empty, accountStd.Account?.Name ?? String.Empty);
        }

        private void AddAccountStdToTimeTransactionItem(TimeTransactionItem timeTransactionItem, int? accountId, string accountNr = "", string accountName = "")
        {
            if (timeTransactionItem == null)
                return;

            if (accountId.HasValue)
                timeTransactionItem.Dim1Id = accountId.Value;
            timeTransactionItem.Dim1Nr = accountNr;
            timeTransactionItem.Dim1Name = accountName;
        }

        private void AddAccountStdToTimeTransactionItem(TimeTransactionItem toTimeTransactionItem, TimeTransactionItem fromTimeTransactionItem)
        {
            if (toTimeTransactionItem == null || fromTimeTransactionItem == null)
                return;

            toTimeTransactionItem.Dim1Id = fromTimeTransactionItem.Dim1Id;
            toTimeTransactionItem.Dim1Nr = fromTimeTransactionItem.Dim1Nr;
            toTimeTransactionItem.Dim1Name = fromTimeTransactionItem.Dim1Name;
        }

        #endregion

        #region Add AccountInternals

        private void AddAccountInternalsToTimeScheduleTemplateBlock(TimeScheduleTemplateBlock templateBlock, IEnumerable<AccountInternal> accountInternals, bool setPredefinedAccountOnTemplateBlock = true)
        {
            if (templateBlock == null || accountInternals == null)
                return;

            foreach (AccountInternal accountInternal in accountInternals)
            {
                TryAddAccountInternalToTimeScheduleTemplateBlock(templateBlock, accountInternal);
            }

            if (setPredefinedAccountOnTemplateBlock)
                SetPredefinedAccountOnTemplateBlock(templateBlock);
        }

        private bool TryAddAccountInternalToTimeScheduleTemplateBlock(TimeScheduleTemplateBlock templateBlock, AccountInternal accountInternal) //NOSONAR
        {
            if (accountInternal == null || templateBlock == null || accountInternal.AccountId == 0)
                return false;

            if (templateBlock.AccountInternal == null)
                templateBlock.AccountInternal = new EntityCollection<AccountInternal>();
            if (IsAccountFromDimAlreadyAdded(accountInternal, templateBlock.AccountInternal))
                return false;

            AccountInternal attachedAccountInternal = GetAttachedAccountInternal(accountInternal);
            if (attachedAccountInternal == null)
                return false;

            templateBlock.AccountInternal.Add(attachedAccountInternal);
            return true;
        }

        private void AddAccountInternalsToTimeBlock(TimeBlock timeBlock, IEnumerable<int> accountIds)
        {
            if (timeBlock == null || accountIds == null)
                return;

            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);

            foreach (int accountId in accountIds)
            {
                AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(accountId);
                if (accountInternal != null)
                    TryAddAccountInternalToTimeBlock(timeBlock, accountInternal);
            }
        }

        private void AddAccountInternalsToTimeBlock(TimeBlock timeBlock, IEnumerable<AccountInternal> accountInternals)
        {
            if (timeBlock == null || accountInternals == null)
                return;

            TryLoadAccountInternalsOnTimeBlock(ref timeBlock);

            foreach (AccountInternal accountInternal in accountInternals)
            {
                TryAddAccountInternalToTimeBlock(timeBlock, accountInternal);
            }
        }

        private bool TryAddAccountInternalToTimeBlock(TimeBlock timeBlock, AccountInternal accountInternal) //NOSONAR
        {
            if (timeBlock == null || accountInternal == null)
                return false;

            if (timeBlock.AccountInternal == null)
                timeBlock.AccountInternal = new EntityCollection<AccountInternal>();
            if (IsAccountFromDimAlreadyAdded(accountInternal, timeBlock.AccountInternal))
                return false;

            AccountInternal attachedAccountInternal = GetAttachedAccountInternal(accountInternal);
            if (attachedAccountInternal == null)
                return false;

            timeBlock.AccountInternal.Add(attachedAccountInternal);
            return true;
        }

        private void AddAccountInternalsToTimeInvoiceTransaction(TimeInvoiceTransaction timeInvoiceTransaction, IEnumerable<AccountInternal> accountInternals)
        {
            if (timeInvoiceTransaction == null || accountInternals == null)
                return;

            TryLoadAccountInternalsOnTimeInvoiceTransaction(timeInvoiceTransaction);

            foreach (AccountInternal accountInternal in accountInternals)
            {
                TryAddAccountInternalToTimeInvoiceTransaction(timeInvoiceTransaction, accountInternal);
            }
        }

        private bool TryAddAccountInternalToTimeInvoiceTransaction(TimeInvoiceTransaction timeInvoiceTransaction, AccountInternal accountInternal) //NOSONAR
        {
            if (timeInvoiceTransaction == null || accountInternal == null)
                return false;

            if (timeInvoiceTransaction.AccountInternal == null)
                timeInvoiceTransaction.AccountInternal = new EntityCollection<AccountInternal>();
            if (IsAccountFromDimAlreadyAdded(accountInternal, timeInvoiceTransaction.AccountInternal))
                return false;

            AccountInternal attachedAccountInternal = GetAttachedAccountInternal(accountInternal);
            if (attachedAccountInternal == null)
                return false;

            timeInvoiceTransaction.AccountInternal.Add(attachedAccountInternal);
            return true;
        }

        private void AddAccountInternalsToTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, IEnumerable<AccountInternal> accountInternals)
        {
            if (timePayrollTransaction == null || accountInternals == null)
                return;

            TryLoadAccountInternalsOnTimePayrollTransaction(timePayrollTransaction);

            foreach (AccountInternal accountInternal in accountInternals)
            {
                TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, accountInternal);
            }
        }

        private bool TryAddAccountInternalToTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, AccountingPrioDTO accountingPrio, List<AccountDim> accountDims) //NOSONAR
        {
            if (timePayrollTransaction == null || accountingPrio == null || accountDims.IsNullOrEmpty())
                return false;

            bool isChanged = false;
            foreach (AccountDim accountDim in accountDims.Where(i => !i.IsStandard))
            {
                if (TryAddAccountInternalToTimePayrollTransaction(timePayrollTransaction, GetAccountInternalWithAccountFromCache(accountingPrio.GetAccountInternalId(accountDim.AccountDimId))))
                    isChanged = true;
            }

            return isChanged;
        }

        private bool TryAddAccountInternalToTimePayrollTransaction(TimePayrollTransaction timePayrollTransaction, AccountInternal accountInternal)
        {
            if (timePayrollTransaction == null || accountInternal == null)
                return false;

            if (timePayrollTransaction.AccountInternal == null)
                timePayrollTransaction.AccountInternal = new EntityCollection<AccountInternal>();
            if (IsAccountFromDimAlreadyAdded(accountInternal, timePayrollTransaction.AccountInternal))
                return false;

            AccountInternal attachedAccountInternal = GetAttachedAccountInternal(accountInternal);
            if (attachedAccountInternal == null)
                return false;

            timePayrollTransaction.AccountInternal.Add(attachedAccountInternal);
            return true;
        }

        private void AddAccountInternalsToTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, IEnumerable<AccountInternal> accountInternals)
        {
            if (timePayrollScheduleTransaction == null || accountInternals == null)
                return;

             TryLoadAccountInternalsOnTimePayrollScheduleTransaction(timePayrollScheduleTransaction);

            foreach (AccountInternal accountInternal in accountInternals)
            {
                TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, accountInternal);
            }
        }

        private bool TryAddAccountInternalToTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, AccountingPrioDTO accountingPrio, List<AccountDim> accountDims)
        {
            if (timePayrollScheduleTransaction == null || accountingPrio == null || accountDims.IsNullOrEmpty())
                return false;

            foreach (AccountDim accountDim in accountDims.Where(i => !i.IsStandard))
            {
                if (TryAddAccountInternalToTimePayrollScheduleTransaction(timePayrollScheduleTransaction, GetAccountInternalWithAccountFromCache(accountingPrio.GetAccountInternalId(accountDim.AccountDimId))))
                    return true;
            }

            return false;
        }

        private bool TryAddAccountInternalToTimePayrollScheduleTransaction(TimePayrollScheduleTransaction timePayrollScheduleTransaction, AccountInternal accountInternal)
        {
            if (timePayrollScheduleTransaction == null || accountInternal == null)
                return false;

            if (timePayrollScheduleTransaction.AccountInternal == null)
                timePayrollScheduleTransaction.AccountInternal = new EntityCollection<AccountInternal>();
            if (IsAccountFromDimAlreadyAdded(accountInternal, timePayrollScheduleTransaction.AccountInternal))
                return false;

            AccountInternal attachedAccountInternal = GetAttachedAccountInternal(accountInternal);
            if (attachedAccountInternal == null)
                return false;

            timePayrollScheduleTransaction.AccountInternal.Add(attachedAccountInternal);
            return true;
        }

        private void AddAccountInternalsToTimeTransactionItem(TimeTransactionItem item, List<AccountInternal> accountInternals)
        {
            if (item == null || accountInternals == null)
                return;

            #region AccountInternal

            for (int position = 0; position < accountInternals.Count; position++)
            {
                //1 = AccountStd, 2-6 = AccountInternal
                AddAccountInternalToTimeTransactionItem(item, accountInternals[position], position + 2);
            }

            #endregion
        }

        private void AddAccountInternalsToTimeTransactionItem(TimeTransactionItem item, TimeTransactionItem prototype)
        {
            if (item == null || prototype == null)
                return;

            item.Dim2Id = prototype.Dim2Id;
            item.Dim2Nr = prototype.Dim2Nr;
            item.Dim2Name = prototype.Dim2Name;

            item.Dim3Id = prototype.Dim3Id;
            item.Dim3Nr = prototype.Dim3Nr;
            item.Dim3Name = prototype.Dim3Name;

            item.Dim4Id = prototype.Dim4Id;
            item.Dim4Nr = prototype.Dim4Nr;
            item.Dim4Name = prototype.Dim4Name;

            item.Dim5Id = prototype.Dim5Id;
            item.Dim5Nr = prototype.Dim5Nr;
            item.Dim5Name = prototype.Dim5Name;

            item.Dim6Id = prototype.Dim6Id;
            item.Dim6Nr = prototype.Dim6Nr;
            item.Dim6Name = prototype.Dim6Name;
        }

        private void AddAccountInternalToTimeTransactionItem(TimeTransactionItem item, AccountInternal accountInternal, int accountDimPosition)
        {
            if (item == null || accountInternal == null || accountDimPosition > 6)
                return;

            if (!accountInternal.AccountReference.IsLoaded && base.CanEntityLoadReferences(entities, accountInternal))
                accountInternal.AccountReference.Load();

            if (accountDimPosition == 2)
            {
                item.Dim2Id = accountInternal.AccountId;
                item.Dim2Nr = accountInternal.Account.AccountNr;
                item.Dim2Name = accountInternal.Account.Name;
            }
            else if (accountDimPosition == 3)
            {
                item.Dim3Id = accountInternal.AccountId;
                item.Dim3Nr = accountInternal.Account.AccountNr;
                item.Dim3Name = accountInternal.Account.Name;
            }
            else if (accountDimPosition == 4)
            {
                item.Dim4Id = accountInternal.AccountId;
                item.Dim4Nr = accountInternal.Account.AccountNr;
                item.Dim4Name = accountInternal.Account.Name;
            }
            else if (accountDimPosition == 5)
            {
                item.Dim5Id = accountInternal.AccountId;
                item.Dim5Nr = accountInternal.Account.AccountNr;
                item.Dim5Name = accountInternal.Account.Name;
            }
            else if (accountDimPosition == 6)
            {
                item.Dim6Id = accountInternal.AccountId;
                item.Dim6Nr = accountInternal.Account.AccountNr;
                item.Dim6Name = accountInternal.Account.Name;
            }
        }

        private void TryAddAccountInternal(ref List<AccountInternal> accountInternals, int accountId, bool replaceOnSameDim, bool discardState)
        {
            if (accountInternals == null)
                accountInternals = new List<AccountInternal>();

            AccountInternal accountInternal = GetAccountInternalWithAccountFromCache(accountId, discardState: discardState);
            if (accountInternal != null)
            {
                if (replaceOnSameDim)
                {
                    AccountInternal accountInternalForDim = accountInternals.FirstOrDefault(a => a.Account.AccountDimId == accountInternal.Account.AccountDimId);
                    if (accountInternalForDim != null)
                        accountInternals.Remove(accountInternalForDim);
                }

                accountInternals.Add(accountInternal);
            }
        }

        #endregion

        #region Account

        private List<int> GetAccountHierarchySettingAccounts(bool useAccountHierarchy, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return AccountManager.GetAccountHierarchySettingAccounts(entities, useAccountHierarchy, this.actorCompanyId, this.userId, dateFrom, dateTo);
        }

        #endregion

        #region AccountInternal

        private List<Account> GetAccountInternals()
        {
            return (from a in entities.Account
                    where a.ActorCompanyId == actorCompanyId &&
                    a.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD &&
                    a.State == (int)SoeEntityState.Active
                    select a).ToList();
        }

        private List<AccountInternal> GetAccountInternalsWithAccount()
        {
            return (from a in entities.AccountInternal
                        .Include("Account")
                    where a.Account.ActorCompanyId == actorCompanyId &&
                    a.Account.State == (int)SoeEntityState.Active
                    select a).ToList();
        }

        private AccountInternal GetAccountInternalWithAccount(int accountId, bool discardState)
        {
            return (from a in entities.AccountInternal
                        .Include("Account")
                    where a.AccountId == accountId &&
                    (discardState || a.Account.State == (int)SoeEntityState.Active)
                    select a).FirstOrDefault();
        }

        private AccountInternal GetAttachedAccountInternal(AccountInternal accountInternal)
        {
            if (base.IsEntityAvailableInContext(entities, accountInternal))
                return accountInternal;

            //Detached - must refetch
            return GetAccountInternalWithAccountFromCache(accountInternal.AccountId);
        }

        private bool IsAccountFromDimAlreadyAdded(AccountInternal accountInternal, IEnumerable<AccountInternal> accountInternals)
        {
            if (accountInternal == null || accountInternals.IsNullOrEmpty())
                return false;

            //Check if same account already added
            if (accountInternals.Any(i => i.AccountId == accountInternal.AccountId))
                return true;

            //Check if account from same dim already added
            if (accountInternal.Account != null && accountInternals.All(i => i.Account != null))
            {
                //Accounts are loaded
                return accountInternals.Any(i => i.Account.AccountDimId == accountInternal.Account.AccountDimId);
            }
            else
            {
                //Accounts are not loaded
                List<AccountInternal> allAccountInternals = GetAccountInternalsWithAccountFromCache();
                foreach (AccountInternal ai in accountInternals)
                {
                    AccountInternal account1 = allAccountInternals?.FirstOrDefault(i => i.AccountId == ai.AccountId);
                    AccountInternal account2 = allAccountInternals?.FirstOrDefault(i => i.AccountId == accountInternal.AccountId);
                    if (account1 != null && account2 != null && account1.Account.AccountDimId == account2.Account.AccountDimId)
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region AccountStd

        private AccountStd GetAccountStd(int accountId)
        {
            return (from a in entities.AccountStd
                    where a.AccountId == accountId &&
                    a.Account.AccountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD &&
                    a.Account.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault<AccountStd>();
        }

        private AccountStd GetAccountStdWithAccount(int accountId)
        {
            return (from a in entities.AccountStd
                    .Include("Account")
                    where a.AccountId == accountId &&
                    a.Account.AccountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD &&
                    a.Account.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault<AccountStd>();
        }

        #endregion

        #region AccountDim

        private List<AccountDimDTO> GetAccountDimInternalsWithParent()
        {
            return AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: true, loadParentOrCalculateLevels: true).ToDTOs();
        }

        private List<AccountDim> GetAccountDims()
        {
            return (from ad in entities.AccountDim
                    where ad.ActorCompanyId == actorCompanyId &&
                    ad.State == (int)SoeEntityState.Active
                    orderby ad.AccountDimNr ascending
                    select ad).ToList();
        }

        private List<AccountDim> GetAccountDimsWitAccount()
        {
            return (from ad in entities.AccountDim
                    .Include("Account")
                    where ad.ActorCompanyId == actorCompanyId &&
                    ad.State == (int)SoeEntityState.Active
                    orderby ad.AccountDimNr ascending
                    select ad).ToList();
        }

        private AccountDim GetAccountDim(int accountDimId)
        {
            return (from ad in entities.AccountDim
                    where ad.ActorCompanyId == actorCompanyId &&
                    ad.AccountDimId == accountDimId &&
                    ad.State == (int)SoeEntityState.Active
                    orderby ad.AccountDimNr ascending
                    select ad).FirstOrDefault();
        }

        #endregion
    }
}
