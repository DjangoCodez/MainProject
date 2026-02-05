using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        private SaveOrderAssignmentOutputDTO TaskSaveOrderAssignments()
        {
            var (iDTO, oDTO) = InitTask<SaveOrderAssignmentInputDTO, SaveOrderAssignmentOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                List<TimeScheduleTemplateBlockDTO> templateBlocks = TimeScheduleManager.GetAllActiveTimeScheduleTemplateBlocks(iDTO.EmployeeId, iDTO.StartTime.Date, (iDTO.StopTime.HasValue ? CalendarUtility.GetEndOfDay(iDTO.StopTime.Value) : DateTime.Now.AddYears(10))).ToDTOs();
                if (!templateBlocks.Any() || (iDTO.StopTime.HasValue && iDTO.StartTime > iDTO.StopTime.Value))
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.TimeSchedulePlanning_ShiftIsNull);
                    return oDTO;
                }

                var order = InvoiceManager.GetOrder(iDTO.OrderId, false, false, base.ActorCompanyId);
                if (order == null || (order.RemainingTime == 0 && !order.KeepAsPlanned))
                {
                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull);
                    return oDTO;
                }

                bool ignoreBreaks = GetCompanyBoolSettingFromCache(CompanySettingType.OrderPlanningIgnoreScheduledBreaksOnAssignment);
                List<TimeScheduleTemplateBlockDTO> availableBlocks = GetAvailableBlocksForOrderAssignment(templateBlocks, iDTO.StartTime, iDTO.StopTime ?? templateBlocks.OrderByDescending(o => o.ActualStopTime).FirstOrDefault()?.ActualStopTime, iDTO.EmployeeId, iDTO.OrderId, iDTO.ShiftTypeId, iDTO.AssignmentTimeAdjustmentType == TermGroup_AssignmentTimeAdjustmentType.FillToZeroRemaining ? order.RemainingTime : int.MaxValue, ignoreBreaks);
                if (availableBlocks.IsNullOrEmpty())
                    return oDTO;

                var shifts = availableBlocks.ToTimeSchedulePlanningDayDTOs();

                OrderListDTO orderListDTO = TimeScheduleManager.GetOrderListDTO(base.ActorCompanyId, iDTO.OrderId);
                shifts.ForEach(f => f.Order = orderListDTO);

                #endregion

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        Guid batchId = GetNewBatchLink();

                        foreach (var shift in shifts)
                        {
                            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.TaskSaveOrderShift, null, false);
                            
                            oDTO.Result = SaveTimeScheduleShiftAndLogChanges(shift, logProperties);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        oDTO.Result = RestoreCurrentDaysToSchedule();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        if (!iDTO.SkipXEMailOnChanges)
                            SendXEMailOnDayChanged();

                        TrySaveAndCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
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

        private SaveOrderShiftOutputDTO TaskSaveOrderShift()
        {
            var (iDTO, oDTO) = InitTask<SaveOrderShiftInputDTO, SaveOrderShiftOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        Guid batchId = GetNewBatchLink();

                        foreach (TimeSchedulePlanningDayDTO shift in iDTO.Shifts)
                        {
                            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shift.TimeScheduleTemplateBlockId, TermGroup_ShiftHistoryType.TaskSaveOrderShift, null, false);

                            oDTO.Result = SaveTimeScheduleShiftAndLogChanges(shift, logProperties);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        oDTO.Result = RestoreCurrentDaysToSchedule();
                        if (!oDTO.Result.Success)
                            return oDTO;

                        SendXEMailOnDayChanged();

                        TrySaveAndCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    oDTO.Result.IntegerValue = 0;
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
        /// Saves TimeBlock's and transactions from ProjectTimeBlocks
        /// </summary>
        /// <param name="SaveDeviationsFromProjectTimeBlocksOutputDTO">Input DTO</param>
        /// <returns>Output DTO</returns>
        private SaveTimeBlocksFromProjectTimeBlocksOutputDTO TaskSaveTimeBlocksFromProjectTimeBlocks()
        {
            var (iDTO, oDTO) = InitTask<SaveTimeBlocksFromProjectTimeBlockInputDTO, SaveTimeBlocksFromProjectTimeBlocksOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.ProjectTimeBlocks == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "ProjectTimeBlock");
                return oDTO;
            }

            #region Init

            // Copy all inputs to a new list, so the iDTO list can be used to populate input to next call
            List<int> originalTargetEntriesId = new List<int>();
            originalTargetEntriesId.AddRange(iDTO.ProjectTimeBlocks.Select(s => s.ProjectTimeBlockId));
            iDTO.ProjectTimeBlocks.Clear();

            int recalculateAbsenceForEmployeeId = 0;
            int recalculateAbsenceForEmployeeIdDelete = 0;

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Prereq

                iDTO.ProjectTimeBlocks = entities.ProjectTimeBlock.Include("TimeBlockDate").Include("TimeDeviationCause").Where(w => originalTargetEntriesId.Contains(w.ProjectTimeBlockId)).ToList();
                List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

                #endregion

                try
                {
                    #region Perform

                    foreach (var projectTimeBlockGroupByDate in iDTO.ProjectTimeBlocks.GroupBy(g => g.TimeBlockDateId))
                    {
                        #region Prereq

                        oDTO.Result = new ActionResult();

                        bool hasCreatedExessTimeBlock = false;

                        TimeBlockDate timeBlockDate = projectTimeBlockGroupByDate.FirstOrDefault()?.TimeBlockDate;
                        if (timeBlockDate == null)
                            continue;

                        Employee currentEmployee = GetEmployeeWithContactPersonAndEmploymentFromCache(timeBlockDate.EmployeeId);
                        EmployeeGroup employeeGroup = currentEmployee.GetEmployeeGroup(timeBlockDate.Date.Date, employeeGroups: employeeGroups);

                        // Validate Employee and EmployeeGroup
                        if (currentEmployee.State != (int)SoeEntityState.Active || employeeGroup == null)
                        {
                            if (!oDTO.Result.Success)
                                return oDTO;

                            continue; // Next employee
                        }

                        TimeScheduleTemplatePeriod timeScheduleTemplatePeriod = GetTimeScheduleTemplatePeriodFromCache(currentEmployee.EmployeeId, timeBlockDate.Date);
                        List<int> projectTimeBlockIds = projectTimeBlockGroupByDate.Select(i => i.ProjectTimeBlockId).ToList();

                        #endregion

                        #region Create TimeBlocks

                        var timeBlocksToAdd = new List<TimeBlock>();
                        var additionalTimeCodeTransactions = new List<TimeCodeTransaction>();

                        int? accountId = GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost);
                        int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                        TimeCode timeCodeCompanySetting = GetTimeCodeFromCache(defaultTimeCodeId);
                        var additionalTimeTransactionsToDelete = new List<TimeCodeTransaction>();
                        var additionalTimePayrollTransactionsToDelete = new List<TimePayrollTransaction>();

                        foreach (var projectTimeBlock in projectTimeBlockGroupByDate.OrderBy(o => o.StartTime))
                        {
                            if ((projectTimeBlock.State == (int)SoeEntityState.Active) && (projectTimeBlock.StartTime != projectTimeBlock.StopTime))
                            {
                                #region Create transaction for Additional time

                                if (projectTimeBlock.TimeDeviationCause.CalculateAsOtherTimeInSales)
                                {
                                    HandleAdditionalTime(additionalTimeCodeTransactions, timeCodeCompanySetting, additionalTimeTransactionsToDelete, additionalTimePayrollTransactionsToDelete, projectTimeBlock);

                                    continue;
                                }

                                #endregion

                                #region Create TimeBlock

                                var timeBlock = new TimeBlock()
                                {
                                    //Set FK
                                    EmployeeId = projectTimeBlock.EmployeeId,
                                    TimeBlockDateId = projectTimeBlock.TimeBlockDateId,
                                    TimeDeviationCauseStartId = projectTimeBlock.TimeDeviationCauseId,
                                    TimeDeviationCauseStopId = projectTimeBlock.TimeDeviationCauseId,
                                    TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriod?.TimeScheduleTemplatePeriodId,
                                    TimeScheduleTemplatePeriod = timeScheduleTemplatePeriod,
                                    ProjectTimeBlockId = projectTimeBlock.ProjectTimeBlockId,
                                    AccountStdId = accountId,
                                    EmployeeChildId = projectTimeBlock.EmployeeChildId,

                                    StartTime = projectTimeBlock.StartTime,
                                    StopTime = projectTimeBlock.StopTime,
                                    IsPreliminary = false,
                                    IsFromOtherProjectTimeBlock = false,
                                    Comment = projectTimeBlock.InternalNote,
                                    Created = projectTimeBlock.Created,
                                    CreatedBy = projectTimeBlock.CreatedBy,
                                    Modified = projectTimeBlock.Modified,
                                    ModifiedBy = projectTimeBlock.ModifiedBy,
                                };
                                SetCreatedProperties(timeBlock);

                                TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseFromCache(projectTimeBlock.TimeDeviationCauseId);
                                TimeCode timeCode = GetTimeCodeFromDeviationCause(timeDeviationCause, 0) ?? timeCodeCompanySetting;
                                if (timeCode != null)
                                    timeBlock.TimeCode.Add(timeCode);

                                ApplyAccountingPrioOnTimeBlock(timeBlock, currentEmployee, true, projectTimeBlock.ProjectId ?? 0);
                                timeBlocksToAdd.Add(timeBlock);

                                if (timeDeviationCause != null && timeDeviationCause.IsAbsence)
                                {
                                    recalculateAbsenceForEmployeeId = projectTimeBlock.EmployeeId;

                                    if (timeDeviationCause.HasAttachZeroDaysNbrOfDaySetting)
                                    {
                                        List<DateTime> dates = TimeScheduleManager.AdjustDatesAccordingToAttachedDays(entities, null, timeDeviationCause.TimeDeviationCauseId, actorCompanyId, projectTimeBlock.EmployeeId, new List<DateTime> { projectTimeBlock.TimeBlockDate.Date });
                                        if (dates.Count > 1)
                                        {
                                            foreach (DateTime date in dates.Where(d => d != projectTimeBlock.TimeBlockDate.Date))
                                            {
                                                TimeBlock abscenseTimeBlock = CreateTimeBlockWithoutAccounting(timeBlock, CalendarUtility.DATETIME_DEFAULT, CalendarUtility.DATETIME_DEFAULT, timeCode?.TimeCodeId ?? 0, false);
                                                abscenseTimeBlock.EmployeeId = timeBlock.EmployeeId;
                                                abscenseTimeBlock.Created = projectTimeBlock.Created;
                                                abscenseTimeBlock.CreatedBy = projectTimeBlock.CreatedBy;
                                                abscenseTimeBlock.TimeBlockDate = GetTimeBlockDateFromCache(timeBlock.EmployeeId, date, true);

                                                TimeScheduleTemplatePeriod abscenseTimeScheduleTemplatePeriod = GetTimeScheduleTemplatePeriodFromCache(currentEmployee.EmployeeId, date);
                                                abscenseTimeBlock.TimeScheduleTemplatePeriodId = abscenseTimeScheduleTemplatePeriod?.TimeScheduleTemplatePeriodId;
                                                abscenseTimeBlock.TimeScheduleTemplatePeriod = abscenseTimeScheduleTemplatePeriod;

                                                timeBlocksToAdd.Add(abscenseTimeBlock);
                                            }
                                        }
                                    }
                                }

                                #endregion
                            }
                            else if (projectTimeBlock.State == (int)SoeEntityState.Deleted)
                            {
                                if (projectTimeBlock.TimeDeviationCause.CalculateAsOtherTimeInSales)
                                {
                                    //add previous transaction for later delete...has already state 2 from ProjectManager and can be muliple if time on projecttimeblock has been updated
                                    var oldTransactions = entities.TimeCodeTransaction.Include("TimePayrollTransaction").Where(x => x.ProjectTimeBlockId == projectTimeBlock.ProjectTimeBlockId && x.Type == (int)TimeCodeTransactionType.Time && x.TimeBlockId == null).ToList();
                                    if (!oldTransactions.IsNullOrEmpty())
                                    {
                                        additionalTimeTransactionsToDelete.AddRange(oldTransactions);
                                        foreach(var oldTransaction in oldTransactions)
                                        {
                                            additionalTimePayrollTransactionsToDelete.AddRange(oldTransaction.TimePayrollTransaction);
                                        }
                                    }
                                }
                                TimeDeviationCause timeDeviationCause = projectTimeBlock.TimeDeviationCause ?? GetTimeDeviationCauseFromCache(projectTimeBlock.TimeDeviationCauseId);
                                if (timeDeviationCause != null && timeDeviationCause.IsAbsence)
                                    recalculateAbsenceForEmployeeIdDelete = projectTimeBlock.EmployeeId;
                            }
                        }

                        #endregion

                        #region Create TimeBlocks from other ProjectTimeBlocks

                        List<ProjectTimeBlock> otherProjectTimeBlocksForDate = ProjectManager.GetProjectTimeBlocks(entities, projectTimeBlockGroupByDate.FirstOrDefault()?.TimeBlockDateId ?? 0,true, true).Where(w => !projectTimeBlockIds.Contains(w.ProjectTimeBlockId)).ToList();
                        if (!otherProjectTimeBlocksForDate.IsNullOrEmpty())
                        {
                            foreach (ProjectTimeBlock projectTimeBlock in otherProjectTimeBlocksForDate.OrderBy(o => o.StartTime))
                            {
                                #region Create transaction for Additional time

                                if (projectTimeBlock.TimeDeviationCause.CalculateAsOtherTimeInSales)
                                {
                                    HandleAdditionalTime(additionalTimeCodeTransactions, timeCodeCompanySetting, additionalTimeTransactionsToDelete, additionalTimePayrollTransactionsToDelete, projectTimeBlock);

                                    continue;
                                }

                                #endregion

                                #region Create TimeBlock

                                TimeBlock timeBlock = new TimeBlock()
                                {
                                    //Set FK
                                    EmployeeId = projectTimeBlock.EmployeeId,
                                    TimeBlockDateId = projectTimeBlock.TimeBlockDateId,
                                    TimeDeviationCauseStartId = projectTimeBlock.TimeDeviationCauseId,
                                    TimeDeviationCauseStopId = projectTimeBlock.TimeDeviationCauseId,
                                    TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriod?.TimeScheduleTemplatePeriodId,
                                    ProjectTimeBlockId = projectTimeBlock.ProjectTimeBlockId,
                                    AccountStdId = accountId,
                                    EmployeeChildId = projectTimeBlock.EmployeeChildId,

                                    //Set references
                                    TimeScheduleTemplatePeriod = timeScheduleTemplatePeriod,

                                    StartTime = projectTimeBlock.StartTime,
                                    StopTime = projectTimeBlock.StartTime <= projectTimeBlock.StopTime ? projectTimeBlock.StopTime : projectTimeBlock.StopTime.AddDays(1),
                                    IsPreliminary = false,
                                    IsFromOtherProjectTimeBlock = true,
                                    Comment = projectTimeBlock.InternalNote,
                                    Created = projectTimeBlock.Created,
                                    CreatedBy = projectTimeBlock.CreatedBy,
                                    Modified = projectTimeBlock.Modified,
                                    ModifiedBy = projectTimeBlock.ModifiedBy,                                   
                                };
                                SetCreatedProperties(timeBlock);

                                TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseFromCache(projectTimeBlock.TimeDeviationCauseId);
                                TimeCode timeCode = GetTimeCodeFromDeviationCause(timeDeviationCause, 0) ?? timeCodeCompanySetting;
                                if (timeCode != null)
                                    timeBlock.TimeCode.Add(timeCode);

                                ApplyAccountingPrioOnTimeBlock(timeBlock, currentEmployee, true, projectTimeBlock.ProjectId ?? 0);
                                timeBlocksToAdd.Add(timeBlock);

                                #endregion
                            }
                        }

                        #endregion

                        #region Delete existing

                        oDTO.Result = SetTimeBlocksAndTransactionsAndScheduleTransactionsToDeleted(timeBlockDate, currentEmployee.EmployeeId, clearScheduledAbsence: false, saveChanges: false);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        if (additionalTimeTransactionsToDelete.Any())
                        {
                            
                            oDTO.Result = SetTimeCodeTransactionsToDeleted(additionalTimeTransactionsToDelete, saveChanges: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            if (additionalTimePayrollTransactionsToDelete.Any())
                            {
                                oDTO.Result = SetTimePayrollTransactionsToDeleted(additionalTimePayrollTransactionsToDelete, saveChanges: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                            }
                        }

                        #endregion

                        #region Create breaks

                        CheckDuplicateTimeBlocks(timeBlocksToAdd, currentEmployee, timeBlockDate, "SaveTimeBlocksFromProjectTimeBlocks1", $"excess={hasCreatedExessTimeBlock}");
                        List<TimeScheduleTemplateBlock> scheduleTemplateBlocks = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(currentEmployee.EmployeeId, timeBlockDate.Date, null);
                        bool isZeroSchedule = scheduleTemplateBlocks.All(s => s.IsZero);

                        if (iDTO.AutoGenTimeAndBreakForProject)
                        {
                            #region Auto create breaks when autocreate timeblocks

                            List<TimeScheduleTemplateBlock> scheduleBreaks = scheduleTemplateBlocks.GetBreaks();
                            if (scheduleBreaks.Any() && timeBlocksToAdd.Any())
                            {
                                //Makes sure we dont have any duplicate breaks #49444
                                scheduleBreaks = scheduleBreaks.GroupBy(x => new { x.StartTime, x.StopTime }).Select(x => x.First()).OrderBy(y => y.StartTime).ToList();

                                List<TimeBlock> timeBlockBreaks = new List<TimeBlock>();
                                foreach (TimeScheduleTemplateBlock scheduleBreak in scheduleBreaks)
                                {
                                    #region Create break

                                    TimeBlock breakBlock = new TimeBlock
                                    {
                                        //Set FK
                                        EmployeeId = currentEmployee.EmployeeId,
                                        TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                                        TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriod?.TimeScheduleTemplatePeriodId,

                                        StartTime = scheduleBreak.StartTime,
                                        StopTime = scheduleBreak.StopTime,
                                        IsBreak = true,
                                        IsPreliminary = false,                                        
                                    };
                                    SetCreatedProperties(breakBlock);

                                    if (!scheduleBreak.TimeCodeReference.IsLoaded)
                                        scheduleBreak.TimeCodeReference.Load();

                                    breakBlock.TimeCode.Add(scheduleBreak.TimeCode);
                                    breakBlock.TimeScheduleTemplateBlockBreakId = scheduleBreak.TimeScheduleTemplateBlockId;

                                    entities.TimeBlock.AddObject(breakBlock);
                                    timeBlockBreaks.Add(breakBlock);

                                    #endregion
                                }

                                foreach (TimeBlock timeBlockBreak in timeBlockBreaks)
                                {
                                    timeBlocksToAdd = RearrangeNewTimeBlockAgainstExisting(timeBlockBreak, timeBlocksToAdd, timeBlockBreak.TimeDeviationCauseStart, true, true, false, true, true);
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            #region Create breaks from empty slots

                            timeBlocksToAdd = timeBlocksToAdd.Where(w => !w.IsBreak).OrderBy(o => o.StartTime).ToList();
                            int count = timeBlocksToAdd.Count;

                            for (int i = 0; i < count; i++)
                            {
                                TimeBlock timeblock = timeBlocksToAdd[i];
                                if (i < count - 1)
                                {
                                    TimeBlock nextBlock = timeBlocksToAdd[i + 1];
                                    if (nextBlock.StartTime > timeblock.StopTime)
                                    {
                                        #region Create break

                                        TimeBlock timeBlockBreak = new TimeBlock
                                        {
                                            //Set FK
                                            EmployeeId = nextBlock.EmployeeId,
                                            TimeBlockDateId = nextBlock.TimeBlockDateId,
                                            TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriod?.TimeScheduleTemplatePeriodId,

                                            StartTime = timeblock.StopTime,
                                            StopTime = nextBlock.StartTime,
                                            IsBreak = true,
                                            IsPreliminary = false,
                                        };
                                        SetCreatedProperties(timeBlockBreak);

                                        TimeScheduleTemplateBlock templateBlockBreak = GetScheduleBlockClosestBreak(scheduleTemplateBlocks, timeBlockBreak.StartTime, timeBlockBreak.StopTime);
                                        if (templateBlockBreak != null)
                                        {
                                            timeBlockBreak.TimeCode.Add(templateBlockBreak.TimeCode);
                                            timeBlockBreak.TimeScheduleTemplateBlockBreakId = templateBlockBreak.TimeScheduleTemplateBlockId;

                                            entities.TimeBlock.AddObject(timeBlockBreak);
                                            timeBlocksToAdd.Add(timeBlockBreak);
                                        }

                                        #endregion
                                    }
                                }
                            }

                            #endregion
                        }

                        #endregion

                        #region CreateExcessTimeBlocks

                        if (!isZeroSchedule && (timeBlocksToAdd.Any() || otherProjectTimeBlocksForDate.Any()))
                        {
                            #region Starting late

                            ProjectTimeBlock otherProjectTimeBlock = otherProjectTimeBlocksForDate.OrderBy(b => b.StartTime).FirstOrDefault();
                            if (timeBlocksToAdd.Any())
                            {
                                TimeBlock firstAddStartDate = timeBlocksToAdd.Where(b => !b.IsBreak()).OrderBy(a => a.StartTime).FirstOrDefault();
                                DateTime firstStartTime = otherProjectTimeBlock == null ? firstAddStartDate.StartTime : CalendarUtility.GetEarliestDate(firstAddStartDate.StartTime, otherProjectTimeBlock.StartTime);
                                DateTime scheduleIn = scheduleTemplateBlocks.GetScheduleIn();
                                if (firstStartTime > scheduleIn)
                                {
                                    #region Create excess TimeBlock

                                    TimeBlock excessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, scheduleIn, firstStartTime, timeBlockDate, employeeGroup, timeScheduleTemplatePeriod?.TimeScheduleTemplatePeriodId ?? 0, scheduleTemplateBlocks);
                                    if (excessTimeBlock != null)
                                    {
                                        ApplyAccountingPrioOnTimeBlock(excessTimeBlock, currentEmployee);
                                        timeBlocksToAdd.Add(excessTimeBlock);

                                        List<TimeBlock> overlappingTimeBlockBreaks = timeBlocksToAdd.Where(t => t.IsBreak() && CalendarUtility.IsDatesOverlapping(t.StartTime, t.StopTime, excessTimeBlock.StartTime, excessTimeBlock.StopTime)).ToList();
                                        if (overlappingTimeBlockBreaks.Any())
                                        {
                                            timeBlocksToAdd.RemoveRange(overlappingTimeBlockBreaks);
                                            foreach (TimeBlock overlappingTimeBlockBreak in overlappingTimeBlockBreaks)
                                            {
                                                timeBlocksToAdd = RearrangeNewTimeBlockAgainstExisting(overlappingTimeBlockBreak, timeBlocksToAdd, overlappingTimeBlockBreak.TimeDeviationCauseStart, true, true, false, false, true);
                                            }
                                        }
                                        hasCreatedExessTimeBlock = true;
                                    }

                                    #endregion
                                }

                                #region Ending early

                                TimeBlock lastAddStopDate = timeBlocksToAdd.Where(b => !b.IsBreak()).OrderBy(a => a.StopTime).LastOrDefault();
                                ProjectTimeBlock lastOtherStopDate = otherProjectTimeBlocksForDate.OrderBy(b => b.StopTime).LastOrDefault();
                                DateTime lastStopTime = lastOtherStopDate == null ? lastAddStopDate.StopTime : CalendarUtility.GetLatestDate(lastAddStopDate.StopTime, lastOtherStopDate.StopTime);
                                DateTime scheduleOut = scheduleTemplateBlocks.GetScheduleOut();
                                if (lastStopTime < scheduleOut)
                                {
                                    #region Create excess TimeBlock

                                    TimeBlock excessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, lastStopTime, scheduleOut, timeBlockDate, employeeGroup, timeScheduleTemplatePeriod?.TimeScheduleTemplatePeriodId ?? 0, scheduleTemplateBlocks);
                                    if (excessTimeBlock != null)
                                    {
                                        ApplyAccountingPrioOnTimeBlock(excessTimeBlock, currentEmployee);
                                        timeBlocksToAdd.Add(excessTimeBlock);

                                        List<TimeBlock> overlappingTimeBlockBreaks = timeBlocksToAdd.Where(t => t.IsBreak() && CalendarUtility.IsDatesOverlapping(t.StartTime, t.StopTime, excessTimeBlock.StartTime, excessTimeBlock.StopTime)).ToList();
                                        if (overlappingTimeBlockBreaks.Any())
                                        {
                                            timeBlocksToAdd.RemoveRange(overlappingTimeBlockBreaks);
                                            foreach (TimeBlock overlappingBreakBlock in overlappingTimeBlockBreaks)
                                            {
                                                timeBlocksToAdd = RearrangeNewTimeBlockAgainstExisting(overlappingBreakBlock, timeBlocksToAdd, overlappingBreakBlock.TimeDeviationCauseStart, true, true, false, false, true);
                                            }
                                        }
                                        hasCreatedExessTimeBlock = true;
                                    }

                                    #endregion
                                }

                                #endregion
                            }

                            #endregion
                        }

                        #endregion

                        #region Save TimeBlocks and Transactions

                        CheckDuplicateTimeBlocks(timeBlocksToAdd, currentEmployee, timeBlockDate, "SaveTimeBlocksFromProjectTimeBlock2", $"excess={hasCreatedExessTimeBlock}");
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            try
                            {
                                oDTO.Result = SaveTimeBlocks(out List<TimeEngineDay> days, timeBlocksToAdd, additionalTimeCodeTransactions);
                                if (oDTO.Result.Success)
                                    oDTO.Result = SaveTransactionsForPeriods(days);

                                timeBlocksToAdd.Clear();
                            }
                            catch (Exception ex)
                            {
                                oDTO.Result.Exception = ex;
                                LogError(ex);
                            }

                            if (!TryCommit(oDTO))
                                Rollback();
                        }

                        #endregion
                    }

                    #endregion

                    #region Recalculate

                    if (oDTO.Result.Success)
                    {
                        if (recalculateAbsenceForEmployeeId > 0 && recalculateAbsenceForEmployeeIdDelete > 0)
                            oDTO.Result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.ApplyAndRestore, recalculateAbsenceForEmployeeId);
                        else if (recalculateAbsenceForEmployeeId > 0)
                            oDTO.Result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Apply, recalculateAbsenceForEmployeeId);
                        else if (recalculateAbsenceForEmployeeIdDelete > 0)
                            oDTO.Result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Restore, recalculateAbsenceForEmployeeIdDelete);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    oDTO.Result.Success = false;
                    oDTO.Result.ErrorMessage = ex.Message;
                }
            }

            return oDTO;
        }

        private void HandleAdditionalTime(List<TimeCodeTransaction> additionalTimeCodeTransactions, TimeCode timeCodeCompanySetting, List<TimeCodeTransaction> additionalTimeTransactionsToDelete, List<TimePayrollTransaction> additionalTimePayrollTransactionsToDelete, ProjectTimeBlock projectTimeBlock)
        {
            //add previous transaction for later delete.
            var oldTransaction = entities.TimeCodeTransaction.Include("TimePayrollTransaction").FirstOrDefault(x => x.ProjectTimeBlockId == projectTimeBlock.ProjectTimeBlockId && x.Type == (int)TimeCodeTransactionType.Time && x.State == (int)SoeEntityState.Active && x.TimeBlockId == null);
            if (oldTransaction != null)
            {
                additionalTimeTransactionsToDelete.Add(oldTransaction);
                additionalTimePayrollTransactionsToDelete.AddRange(oldTransaction.TimePayrollTransaction.Where(w => w.State == (int)SoeEntityState.Active));
            }

            var timeCodeAdditionalTime = GetTimeCodeFromDeviationCause(projectTimeBlock.TimeDeviationCause, 0) ?? timeCodeCompanySetting;

            var timeTransaction = CreateTimeCodeTransaction(
                timeCodeId: timeCodeAdditionalTime.TimeCodeId,
                transactionType: TimeCodeTransactionType.Time,
                quantity: CalendarUtility.TimeSpanToMinutes(projectTimeBlock.StopTime, projectTimeBlock.StartTime),
                start: projectTimeBlock.StartTime, 
                stop: projectTimeBlock.StopTime, 
                timeBlockDate: GetTimeBlockDateFromCache(projectTimeBlock.EmployeeId, projectTimeBlock.TimeBlockDateId),
                projectTimeBlockId: projectTimeBlock.ProjectTimeBlockId
                );
            SetCreatedProperties(timeTransaction);
            additionalTimeCodeTransactions.Add(timeTransaction);
        }

        #endregion

        #region Order

        private void UpdateOrderRemainingTime(int length, int actorCompanyId, int orderId)
        {
            if (length != 0)
            {
                CustomerInvoice order = (from c in this.entities.Invoice.OfType<CustomerInvoice>()
                                         where c.Origin.ActorCompanyId == actorCompanyId &&
                                         c.InvoiceId == orderId &&
                                         c.State == (int)SoeEntityState.Active
                                         select c).FirstOrDefault();
                if (order != null)
                {
                    order.RemainingTime -= length;
                    if (order.RemainingTime < 0)
                        order.RemainingTime = 0;
                    SetModifiedProperties(order);
                }
            }
        }

        private void SyncOriginUserWithScheduleBlocks(int orderId)
        {
            // Get all active users on specified order
            var originUsers = (from ou in this.entities.OriginUser
                               where ou.Origin.OriginId == orderId &&
                               ou.UserId.HasValue &&
                               ou.State == (int)SoeEntityState.Active
                               select ou);

            // Get all users connected to specified order through schedule blocks
            List<int> userIds = (from b in this.entities.TimeScheduleTemplateBlock
                                 where b.CustomerInvoiceId == orderId &&
                                 b.EmployeeId.HasValue &&
                                 b.Employee.UserId.HasValue &&
                                 b.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None &&
                                 b.StartTime != b.StopTime &&
                                 b.State == (int)SoeEntityState.Active
                                 select b.Employee.UserId.Value).Distinct().ToList();

            // Update/Delete OriginUsers
            foreach (OriginUser originUser in originUsers)
            {
                if (userIds.Contains(originUser.UserId.Value))
                {
                    // User still exist, keep it
                    // Remove from userIds collection to prevent from adding it below
                    userIds.Remove(originUser.UserId.Value);
                }
                // Code removed due to bug 92359
                /*else
                {
                    // User does not exist on any active block, remove the OriginUser if it's not the Main user
                    if (!originUser.Main)
                    {
                        originUser.State = (int)SoeEntityState.Deleted;
                        SetModifiedProperties(originUser);
                    }
                }*/
            }

            // Add new users
            foreach (int uid in userIds)
            {
                OriginUser originUser = new OriginUser()
                {
                    OriginId = orderId,
                    UserId = uid
                };
                SetCreatedProperties(originUser);
                this.entities.OriginUser.AddObject(originUser);
            }
        }

        #endregion
    }
}
