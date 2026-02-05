using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private MobileModifyBreakOutputDTO TaskMobileModifyBreak()
        {
            var (iDTO, oDTO) = InitTask<MobileModifyBreakInputDTO, MobileModifyBreakOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            List<TimeBlock> outputTimeBlocks = new List<TimeBlock>();
            List<TimeTransactionItem> outputTimeTransactionItems = new List<TimeTransactionItem>();
            TimeBlockDate timeBlockDate = null;

            #region Perform: Generate

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    timeBlockDate = GetTimeBlockDateFromCache(iDTO.EmployeeId, iDTO.Date, true);
                    if (timeBlockDate == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeBlockDate");
                        return oDTO;
                    }

                    #endregion

                    #region Perform

                    TimeScheduleTemplatePeriod templatePeriod = GetSequentialSchedule(iDTO.TimeScheduleTemplatePeriodId, iDTO.Date, iDTO.EmployeeId, true);
                    TimeScheduleTemplateBlock scheduleBreak = templatePeriod.TimeScheduleTemplateBlock.FirstOrDefault(b => b.TimeScheduleTemplateBlockId == iDTO.ScheduleBreakBlockId);
                    List<TimeBlock> existingTimeBlocks = GetSequentialDeviations(iDTO.EmployeeId, timeBlockDate.TimeBlockDateId).OrderBy(t => t.StartTime).ToList();
                    List<TimeBlock> affectedBreaks = existingTimeBlocks.Where(b => b.TimeScheduleTemplateBlockBreakId.HasValue && b.TimeScheduleTemplateBlockBreakId.Value == iDTO.ScheduleBreakBlockId).OrderBy(b => b.StartTime).ToList();

                    bool alignRightMaybeNeeded = false;
                    DateTime? oldStopTime = null;

                    if (scheduleBreak != null)
                    {
                        DateTime newStopTime = scheduleBreak.StartTime.AddMinutes(iDTO.TotalMinutes);
                        if (affectedBreaks.Count > 0)
                        {
                            TimeBlock lastBreak = affectedBreaks.LastOrDefault();
                            if (lastBreak != null)
                            {
                                oldStopTime = lastBreak.StopTime;
                                if (oldStopTime.Value > newStopTime)
                                    alignRightMaybeNeeded = true; //user has decreased break time, so we may have to align the block to the right
                            }
                        }

                        TimeBlock newBreak = new TimeBlock
                        {
                            TimeBlockId = 0,
                            StartTime = scheduleBreak.StartTime,
                            StopTime = newStopTime,
                            IsBreak = true,
                            Comment = String.Empty,

                            //Set FK
                            TimeScheduleTemplateBlockBreakId = iDTO.ScheduleBreakBlockId,

                            //Set references
                            TimeDeviationCauseStart = null,
                            TimeDeviationCauseStop = null,
                            TimeDeviationCauseStartId = null,
                            TimeDeviationCauseStopId = null,
                        };
                        SetCreatedProperties(newBreak);

                        List<TimeBlock> timeBlocksAfterBreakChange = new List<TimeBlock>();

                        foreach (TimeBlock existingTimeBlock in existingTimeBlocks)
                        {
                            if (affectedBreaks.Any(t => t.TimeBlockId == existingTimeBlock.TimeBlockId))
                                continue;

                            timeBlocksAfterBreakChange.Add(existingTimeBlock);
                        }

                        timeBlocksAfterBreakChange = timeBlocksAfterBreakChange.OrderBy(b => b.StartTime).ToList();

                        #region Align right TimeBlock if needed

                        if (alignRightMaybeNeeded && oldStopTime.HasValue)
                        {
                            TimeBlock timeBlockAlignedToTheRight = timeBlocksAfterBreakChange.FirstOrDefault(b => b.StartTime == oldStopTime.Value);
                            if (timeBlockAlignedToTheRight != null)
                                timeBlockAlignedToTheRight.StartTime = newBreak.StopTime;
                        }

                        #endregion

                        timeBlocksAfterBreakChange = RearrangeNewTimeBlockAgainstExisting(newBreak, timeBlocksAfterBreakChange, newBreak.TimeDeviationCauseStart);

                        oDTO.Result = GenerateDeviationsForPeriod(iDTO.Date, iDTO.TimeScheduleTemplatePeriodId, iDTO.EmployeeId, timeBlocksAfterBreakChange, out outputTimeBlocks, out outputTimeTransactionItems);
                    }
                    if (!oDTO.Result.Success)
                        return oDTO;

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            #endregion

            if (!oDTO.Result.Success)
                return oDTO;

            ClearCachedContent();

            #region Perform: Save

            if (timeBlockDate == null)
            {
                oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeBlockDate");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveGeneratedDeviationsForPeriod(iDTO.EmployeeId, iDTO.TimeScheduleTemplatePeriodId, timeBlockDate.TimeBlockDateId, outputTimeBlocks, outputTimeTransactionItems, null);

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

            #endregion

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private AddModifyTimeBlocksOutputDTO TaskAddModifyTimeBlocks()
        {
            var (iDTO, oDTO) = InitTask<AddModifyTimeBlocksInputDTO, AddModifyTimeBlocksOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            TimeBlockDate timeBlockDate = null;
            List<TimeBlock> timeBlocksForPeriod = null;
            List<TimeBlock> outputTimeBlocks = new List<TimeBlock>();
            List<TimeTransactionItem> outputTimeTransactionItems = new List<TimeTransactionItem>();
            List<ApplyAbsenceDTO> applyAbsenceDays = null;

            #region Perform: Generate

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                    TimeCode defaultCompanyTimeCode = GetTimeCodeFromCache(defaultTimeCodeId);
                    List<TimeBlock> inputTimeBlocks = iDTO.InputTimeBlocks.OrderBy(o => o.StartTime).ToList();
                    List<TimeScheduleTemplateBlock> allScheduleBlocks = GetScheduleBlocksFromCache(iDTO.EmployeeId, iDTO.Date);
                    timeBlockDate = GetTimeBlockDateFromCache(iDTO.EmployeeId, iDTO.Date, true);

                    #endregion

                    #region Input TimeBlocks

                    List<TimeBlock> clonedTimeBlocks = new List<TimeBlock>();
                    foreach (TimeBlock inputTimeBlock in inputTimeBlocks)
                    {
                        TimeBlock clone = new TimeBlock();
                        EntityUtil.Copy(clone, inputTimeBlock);
                        clone.TimeBlockId = inputTimeBlock.TimeBlockId;
                        clone.EmployeeChildId = inputTimeBlock.EmployeeChildId;
                        clone.ShiftTypeId = inputTimeBlock.ShiftTypeId;
                        clone.TimeScheduleTypeId = inputTimeBlock.TimeScheduleTypeId;
                        clone.CreateAsBlank = inputTimeBlock.CreateAsBlank;
                        clonedTimeBlocks.Add(clone);
                    }
                    inputTimeBlocks = clonedTimeBlocks;

                    foreach (TimeBlock inputTimeBlock in inputTimeBlocks)
                    {
                        TimeDeviationCause deviationStartCause = inputTimeBlock.TimeDeviationCauseStartId.HasValue ? GetTimeDeviationCauseWithTimeCodeFromCache(inputTimeBlock.TimeDeviationCauseStartId.Value) : null;
                        if (deviationStartCause == null)
                        {
                            oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10119, "Orsak hittades inte"));
                            return oDTO;
                        }

                        inputTimeBlock.TimeDeviationCauseStart = deviationStartCause;
                        inputTimeBlock.TimeDeviationCauseStop = deviationStartCause;

                        if (deviationStartCause.TimeCode != null)
                            inputTimeBlock.TimeCode.Add(deviationStartCause.TimeCode);
                        else if ((deviationStartCause.Type == (int)TermGroup_TimeDeviationCauseType.Presence || deviationStartCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence) && defaultCompanyTimeCode != null)
                            inputTimeBlock.TimeCode.Add(defaultCompanyTimeCode);
                    }

                    #endregion

                    #region Rearrange TimeBlocks

                    timeBlocksForPeriod = GetTimeBlocksWithTimeCodeAndTransactions(iDTO.EmployeeId, timeBlockDate.TimeBlockDateId);
                    SetIsAttested(timeBlocksForPeriod);

                    List<TimeBlock> timeBlockBlanks = inputTimeBlocks.Where(t => t.CreateAsBlank).ToList();
                    inputTimeBlocks = inputTimeBlocks.Where(t => !t.CreateAsBlank).ToList();
                    List<TimeBlock> deletedBlocks = inputTimeBlocks.Where(t => t.State == (int)SoeEntityState.Deleted && t.TimeBlockId != 0).ToList();
                    List<TimeBlock> updatedBlocks = inputTimeBlocks.Where(t => t.TimeBlockId != 0).ToList();
                    List<TimeBlock> newBlocks = inputTimeBlocks.Where(t => t.TimeBlockId == 0).ToList();

                    foreach (TimeBlock deletedBlock in deletedBlocks)
                    {
                        TimeBlock originalTimeBlock = timeBlocksForPeriod.FirstOrDefault(tb => tb.TimeBlockId == deletedBlock.TimeBlockId);
                        if (originalTimeBlock != null)
                            timeBlocksForPeriod.Remove(originalTimeBlock);
                    }

                    foreach (TimeBlock timeBlockBlank in timeBlockBlanks)
                    {
                        timeBlocksForPeriod = RearrangeNewTimeBlockAgainstExisting(timeBlockBlank, timeBlocksForPeriod, timeBlockBlank.TimeDeviationCauseStart, false, true);
                    }

                    foreach (TimeBlock inputTimeBlock in updatedBlocks)
                    {
                        TimeBlock originalTimeBlock = timeBlocksForPeriod.FirstOrDefault(tb => tb.TimeBlockId == inputTimeBlock.TimeBlockId);
                        if (originalTimeBlock != null)
                        {
                            timeBlocksForPeriod.Remove(originalTimeBlock);
                            timeBlocksForPeriod = RearrangeNewTimeBlockAgainstExisting(inputTimeBlock, timeBlocksForPeriod, inputTimeBlock.TimeDeviationCauseStart);
                        }
                        else
                        {
                            originalTimeBlock = timeBlocksForPeriod.FirstOrDefault(tb => tb.StartTime == inputTimeBlock.StartTime);
                            if (originalTimeBlock != null && originalTimeBlock.TimeBlockId == 0 && !inputTimeBlock.Comment.IsNullOrEmpty())
                                originalTimeBlock.Comment = inputTimeBlock.Comment;

                        }
                    }

                    foreach (TimeBlock inputTimeBlock in newBlocks)
                    {
                        timeBlocksForPeriod = RearrangeNewTimeBlockAgainstExisting(inputTimeBlock, timeBlocksForPeriod, inputTimeBlock.TimeDeviationCauseStart);
                    }

                    #endregion

                    #region Generate deviations

                    oDTO.Result = GenerateDeviationsForPeriod(iDTO.Date, iDTO.TimeScheduleTemplatePeriodId, iDTO.EmployeeId, timeBlocksForPeriod, out outputTimeBlocks, out outputTimeTransactionItems, scheduleBlocks: allScheduleBlocks);
                    if (!oDTO.Result.Success)
                        return oDTO;

                    applyAbsenceDays = ConvertToApplyAbsenceDayDTOs(GetDaysFromApplyAbsenceTracker());

                    #endregion

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

            #endregion

            if (!oDTO.Result.Success)
                return oDTO;

            ClearCachedContent();

            #region Perform:Save

            if (timeBlockDate == null)
            {
                oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeBlockDate");
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveGeneratedDeviationsForPeriod(iDTO.EmployeeId, iDTO.TimeScheduleTemplatePeriodId, timeBlockDate.TimeBlockDateId, outputTimeBlocks, outputTimeTransactionItems, applyAbsenceDays);

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

            #endregion

            return oDTO;
        }

        #endregion
    }
}
