using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
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

        private GenerateDeviationsFromTimeIntervalOutputDTO TaskGenerateDeviationsFromTimeInterval()
        {
            var (iDTO, oDTO) = InitTask<GenerateDeviationsFromTimeIntervalInputDTO, GenerateDeviationsFromTimeIntervalOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region ReadMe

            //Possible cases:

            //Displaydate = 17:e

            //        From   To	
            //case 1: 16:e - 17:e //spans midnight, displaydate dont have to be included could be 15:e - 16:e or 18:e - 19:e
            //case 2: 17:e - 17:e //same day as displaydate
            //case 3: 16:e - 16:e //same day but not display date
            //case 4: 16:e - 18:e //spans multiple days, displaydate dont have to be included could be 14:e - 16:e or 18:e - 20:e

            #endregion

            #region Init

            if ((iDTO == null) || (iDTO.EmployeeId <= 0) || iDTO.TimeDeviationCauseStartId <= 0 || iDTO.TimeDeviationCauseStopId <= 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultDelete.InsufficientInput);
                return oDTO;
            }

            //Whole days
            List<DateTime> wholeDayDates = new List<DateTime>();

            //First and last day
            TimeScheduleTemplatePeriod templatePeriodForFirstDay = null;
            TimeScheduleTemplatePeriod templatePeriodForLastDay = null;
            TimeBlockDTO wholeDayTimeBlocks = new TimeBlockDTO();
            List<TimeBlock> timeBlocksForFirstDay;
            List<TimeBlock> timeBlocksForLastDay;
            List<TimeBlock> outputTimeBlocksForFirstDay = null;
            List<TimeBlock> outputTimeBlocksForLastDay = null;
            TimeBlock newTimeBlockForFirstDay = null;
            TimeBlock newTimeBlockForLastDay = null;
            TimeBlockDate timeBlockDateFirstDay = null;
            TimeBlockDate timeBlockDateLastDay = null;
            List<TimeTransactionItem> outputTimeTransactionItemsForFirstDay = null;
            List<TimeTransactionItem> outputTimeTransactionItemsForLastDay = null;
            List<ApplyAbsenceDTO> applyAbsenceDays = null;
            bool adjustFirstTimeBlockStopToScheduleOut = false;
            bool adjustLastTimeBlockStartToScheduleIn = false;

            #endregion
           
            #region Prereq

            //Check if we need to created more than one timeblock
            bool onlyOneTimeBlockNeeded = true;
            bool isSameDay = iDTO.Start.Date == iDTO.Stop.Date;
            if (!isSameDay)
            {
                //If start and stop only cross one midnight we still need to create only one TimeBlock. 
                //This is not completly true! We also need to check if start and stop spans more than one period. If more than one period we need to create one timeblock per period.
                onlyOneTimeBlockNeeded = (iDTO.Start.Date.AddDays(1) == iDTO.Stop.Date);
            }

            if (onlyOneTimeBlockNeeded)
            {
                #region Only one TimeBlock needed

                newTimeBlockForFirstDay = new TimeBlock()
                {
                    Comment = iDTO.DeviationComment,
                    StartTime = CalendarUtility.GetScheduleTime(iDTO.Start),
                    StopTime = CalendarUtility.GetScheduleTime(iDTO.Stop),
                };
                SetCreatedProperties(newTimeBlockForFirstDay);

                if (iDTO.DisplayedDate.HasValue)
                {
                    newTimeBlockForFirstDay.StartTime = newTimeBlockForFirstDay.StartTime.AddDays((iDTO.Start.Date - iDTO.DisplayedDate.Value.Date).Days);
                    newTimeBlockForFirstDay.StopTime = newTimeBlockForFirstDay.StopTime.AddDays((iDTO.Stop.Date - iDTO.DisplayedDate.Value.Date).Days);
                }
                #endregion
            }
            else
            {
                DateTime startTime;

                #region 1. Assume all days between start and start is whole days

                DateTime dayInInterval = iDTO.Start.AddDays(1).Date;
                while (dayInInterval < iDTO.Stop.Date)
                {
                    wholeDayDates.Add(dayInInterval);
                    dayInInterval = dayInInterval.AddDays(1);
                }

                #endregion

                #region 2. Create a TimeBlock that spans over all whole days

                if (wholeDayDates.Count > 0)
                {
                    wholeDayTimeBlocks.StartTime = wholeDayDates.First().Date;                         //xxxx-xx-xx 00:00                
                    wholeDayTimeBlocks.StopTime = CalendarUtility.GetEndOfDay(wholeDayDates.Last().Date);  //xxxx-xx-xx 23:59                             
                }

                #endregion

                #region 3. Assume that the user MAY not have choosen the whole first and last day.

                startTime = CalendarUtility.GetScheduleTime(iDTO.Start);
                newTimeBlockForFirstDay = new TimeBlock()
                {
                    Comment = iDTO.DeviationComment,
                    StartTime = startTime,
                    StopTime = CalendarUtility.GetScheduleTime(CalendarUtility.GetEndOfDay(startTime)),
                };
                SetCreatedProperties(newTimeBlockForFirstDay);
                adjustFirstTimeBlockStopToScheduleOut = true; //To handle TimeScheduleTemplateBlock's that ends on the next day

                startTime = CalendarUtility.GetScheduleTime(CalendarUtility.GetBeginningOfDay(iDTO.Stop));
                newTimeBlockForLastDay = new TimeBlock()
                {
                    Comment = iDTO.DeviationComment,
                    StartTime = startTime,
                    StopTime = CalendarUtility.GetScheduleTime(iDTO.Stop),
                };
                SetCreatedProperties(newTimeBlockForLastDay);
                adjustLastTimeBlockStartToScheduleIn = true; //So that we create TimeBlock only during schedule time

                #endregion
            }

            #endregion

            #region Perform: Generate

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(iDTO.EmployeeId, iDTO.Start);
                if (timeBlockDate != null)
                {
                    if (timeBlockDate.IsLocked)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved);
                        return oDTO;
                    }
                    else
                    {
                        AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                        if (attestStateInitial == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);
                            return oDTO;
                        }

                        List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsConnectedToTimeBlock(iDTO.EmployeeId, timeBlockDate.TimeBlockDateId);
                        if (timePayrollTransactions.Any(x => x.AttestStateId != attestStateInitial.AttestStateId))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved);
                            return oDTO;
                        }
                    }

                }

                try
                {
                    #region Prereq

                    TimeDeviationCause deviationStartCause = GetTimeDeviationCauseWithTimeCodeFromCache(iDTO.TimeDeviationCauseStartId);
                    if (deviationStartCause == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");
                        return oDTO;
                    }

                    int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);
                    TimeCode defaultCompanyTimeCode = GetTimeCodeFromCache(defaultTimeCodeId);

                    #endregion

                    if (newTimeBlockForFirstDay != null)
                    {
                        #region Generate deviations for first day

                        // Date and TimeSchedulePeriod must be decided
                        DateTime newDate;
                        int? timeScheduleTemplatePeriodId = null;

                        if (isSameDay)
                        {
                            if (iDTO.DisplayedDate.HasValue)
                            {
                                #region Covers case 2

                                // Use the input values, no need to search for nearest scheduleperiod
                                timeScheduleTemplatePeriodId = iDTO.TimeScheduleTemplatePeriodId;
                                newDate = iDTO.DisplayedDate.Value.Date;

                                #endregion
                            }
                            else
                            {
                                timeScheduleTemplatePeriodId = GetNearestTimeScheduleTemplatePeriodIdForSpecificDayForTimeBlock(iDTO.EmployeeId, iDTO.Start.Date, newTimeBlockForFirstDay);
                                newDate = iDTO.Start.Date;
                            }
                        }
                        else
                        {
                            #region Covers case 1 and 4

                            timeScheduleTemplatePeriodId = GetNearestTimeScheduleTemplatePeriodIdForSpecificDayForTimeBlock(iDTO.EmployeeId, iDTO.Start.Date, newTimeBlockForFirstDay);
                            newDate = iDTO.Start.Date;

                            #endregion
                        }

                        if (timeScheduleTemplatePeriodId.HasValue)
                            templatePeriodForFirstDay = GetTimeScheduleTemplatePeriodFromCache(timeScheduleTemplatePeriodId.Value);
                        if (templatePeriodForFirstDay == null)
                        {
                            oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeScheduleTemplatePeriodId");
                            return oDTO;
                        }

                        timeBlockDateFirstDay = GetTimeBlockDateFromCache(iDTO.EmployeeId, newDate, true);
                        timeBlocksForFirstDay = GetTimeBlocksWithTimeCodeAndTransactions(iDTO.EmployeeId, timeBlockDateFirstDay.TimeBlockDateId);
                        SetIsAttested(timeBlocksForFirstDay);

                        newTimeBlockForFirstDay.TimeDeviationCauseStart = deviationStartCause;
                        newTimeBlockForFirstDay.TimeDeviationCauseStop = deviationStartCause;
                        newTimeBlockForFirstDay.TimeDeviationCauseStartId = deviationStartCause.TimeDeviationCauseId;
                        newTimeBlockForFirstDay.TimeDeviationCauseStopId = deviationStartCause.TimeDeviationCauseId;
                        newTimeBlockForFirstDay.EmployeeChildId = iDTO.EmployeeChildId;
                        newTimeBlockForFirstDay.ShiftTypeId = iDTO.ShiftTypeId;
                        newTimeBlockForFirstDay.TimeScheduleTypeId = iDTO.TimeScheduleTypeId;
                        newTimeBlockForFirstDay.AddTimeDeviationCauseOrDefault(deviationStartCause, iDTO.ChoosenDeviationCauseType, defaultCompanyTimeCode);
                        if (iDTO.ChoosenDeviationCauseType == TermGroup_TimeDeviationCauseType.Absence || adjustFirstTimeBlockStopToScheduleOut)
                            AdjustAfterScheduleInOut(newTimeBlockForFirstDay, deviationStartCause, newDate, iDTO.EmployeeId, adjustFirstTimeBlockStopToScheduleOut, iDTO.ChoosenDeviationCauseType);

                        timeBlocksForFirstDay = timeBlocksForFirstDay.OrderBy(o => o.StartTime).ToList();
                        timeBlocksForFirstDay = RearrangeNewTimeBlockAgainstExisting(newTimeBlockForFirstDay, timeBlocksForFirstDay, deviationStartCause);
                        timeBlocksForFirstDay.AddTimeDeviationCauseOrDefaultIfMissing(deviationStartCause, iDTO.ChoosenDeviationCauseType, defaultCompanyTimeCode);

                        oDTO.Result = GenerateDeviationsForPeriod(newDate, templatePeriodForFirstDay.TimeScheduleTemplatePeriodId, iDTO.EmployeeId, timeBlocksForFirstDay, out outputTimeBlocksForFirstDay, out outputTimeTransactionItemsForFirstDay);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion
                    }
                    if (newTimeBlockForLastDay != null)
                    {
                        #region Generate deviations for last day

                        DateTime newDate = iDTO.Stop.Date;
                        int? timeScheduleTemplatePeriodId = GetNearestTimeScheduleTemplatePeriodIdForSpecificDayForTimeBlock(iDTO.EmployeeId, newDate, newTimeBlockForLastDay);
                        templatePeriodForLastDay = timeScheduleTemplatePeriodId.HasValue ? GetTimeScheduleTemplatePeriodFromCache(timeScheduleTemplatePeriodId.Value) : null;
                        timeBlockDateLastDay = GetTimeBlockDateFromCache(iDTO.EmployeeId, newDate, true);
                        timeBlocksForLastDay = GetTimeBlocksWithTimeCodeAndTransactions(iDTO.EmployeeId, timeBlockDateLastDay.TimeBlockDateId);
                        SetIsAttested(timeBlocksForLastDay);

                        newTimeBlockForLastDay.TimeDeviationCauseStart = deviationStartCause;
                        newTimeBlockForLastDay.TimeDeviationCauseStop = deviationStartCause;
                        newTimeBlockForLastDay.TimeDeviationCauseStartId = deviationStartCause.TimeDeviationCauseId;
                        newTimeBlockForLastDay.TimeDeviationCauseStopId = deviationStartCause.TimeDeviationCauseId;
                        newTimeBlockForLastDay.EmployeeChildId = iDTO.EmployeeChildId;
                        newTimeBlockForFirstDay.ShiftTypeId = iDTO.ShiftTypeId;
                        newTimeBlockForFirstDay.TimeScheduleTypeId = iDTO.TimeScheduleTypeId;
                        if (deviationStartCause.TimeCode != null)
                            newTimeBlockForLastDay.TimeCode.Add(deviationStartCause.TimeCode);
                        else if (iDTO.ChoosenDeviationCauseType == TermGroup_TimeDeviationCauseType.Presence && defaultCompanyTimeCode != null)
                            newTimeBlockForLastDay.TimeCode.Add(defaultCompanyTimeCode);
                        if (iDTO.ChoosenDeviationCauseType == TermGroup_TimeDeviationCauseType.Absence || adjustLastTimeBlockStartToScheduleIn)
                            AdjustAfterScheduleInOut(newTimeBlockForLastDay, deviationStartCause, newDate, iDTO.EmployeeId, adjustFirstTimeBlockStopToScheduleOut, iDTO.ChoosenDeviationCauseType);

                        timeBlocksForLastDay = timeBlocksForLastDay.OrderBy(o => o.StartTime).ToList();
                        timeBlocksForLastDay = RearrangeNewTimeBlockAgainstExisting(newTimeBlockForLastDay, timeBlocksForLastDay, deviationStartCause);
                        timeBlocksForLastDay.AddTimeDeviationCauseOrDefaultIfMissing(deviationStartCause, iDTO.ChoosenDeviationCauseType, defaultCompanyTimeCode);

                        oDTO.Result = GenerateDeviationsForPeriod(newDate, templatePeriodForLastDay.TimeScheduleTemplatePeriodId, iDTO.EmployeeId, timeBlocksForLastDay, out outputTimeBlocksForLastDay, out outputTimeTransactionItemsForLastDay);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion
                    }

                    applyAbsenceDays = ConvertToApplyAbsenceDayDTOs(GetDaysFromApplyAbsenceTracker());
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

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        if (iDTO.ChoosenDeviationCauseType != TermGroup_TimeDeviationCauseType.Presence && wholeDayDates.Count > 0)
                        {
                            #region Save whole day deviations

                            List<TimeBlockDTO> inputTimeBlocks = new List<TimeBlockDTO> { wholeDayTimeBlocks };

                            oDTO.Result = SaveWholedayDeviations(inputTimeBlocks, iDTO.TimeDeviationCauseStartId, iDTO.TimeDeviationCauseStopId, iDTO.DeviationComment, iDTO.ChoosenDeviationCauseType, iDTO.EmployeeId, iDTO.EmployeeChildId);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion
                        }

                        if (newTimeBlockForFirstDay != null && timeBlockDateFirstDay != null)
                        {
                            #region Save deviations for first day

                            if (timeBlockDateFirstDay.TimeBlockDateId == 0)
                            {
                                timeBlockDateFirstDay = GetTimeBlockDateFromCache(iDTO.EmployeeId, timeBlockDateFirstDay.Date, true);
                                Save();
                            }

                            oDTO.Result = SaveGeneratedDeviationsForPeriod(iDTO.EmployeeId, templatePeriodForFirstDay.TimeScheduleTemplatePeriodId, timeBlockDateFirstDay.TimeBlockDateId, outputTimeBlocksForFirstDay, outputTimeTransactionItemsForFirstDay, applyAbsenceDays);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            #endregion
                        }

                        if (newTimeBlockForLastDay != null && timeBlockDateLastDay != null)
                        {
                            #region Save deviations for last day

                            if (timeBlockDateLastDay.TimeBlockDateId == 0)
                            {
                                timeBlockDateLastDay = GetTimeBlockDateFromCache(iDTO.EmployeeId, timeBlockDateLastDay.Date, true);
                                Save();
                            }

                            oDTO.Result = SaveGeneratedDeviationsForPeriod(iDTO.EmployeeId, templatePeriodForLastDay.TimeScheduleTemplatePeriodId, timeBlockDateLastDay.TimeBlockDateId, outputTimeBlocksForLastDay, outputTimeTransactionItemsForLastDay, applyAbsenceDays);
                            if (!oDTO.Result.Success)
                                return oDTO;

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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            #endregion

            if (oDTO.Result.Success && this.localDoNotCollectDaysForRecalculationLevel2.HasValue && this.localDoNotCollectDaysForRecalculationLevel2.Value == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence && this.localDoNotCollectDaysForRecalculationLevel3.HasValue && this.localDoNotCollectDaysForRecalculationLevel3Reversed != null && this.localDoNotCollectDaysForRecalculationLevel3Reversed.Any(i => i != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick))
                oDTO.Result.InfoMessage = GetText(11509, "Kontrollera dagar både framåt och bakåt med samma frånvarotyp. Perioden är låst och därför måste dessa dagar hanteras manuellt");

            return oDTO;
        }

        private SaveGeneratedDeviationsOutputDTO TaskSaveGeneratedDeviations()
        {
            var (iDTO, oDTO) = InitTask<SaveGeneratedDeviationsInputDTO, SaveGeneratedDeviationsOutputDTO>();
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

                        #region Prereq

                        TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(iDTO.EmployeeId, iDTO.TimeBlockDateId);
                        if (timeBlockDate == null)
                        {
                            oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeBlockDate");
                            return oDTO;
                        }
                        else if (timeBlockDate.IsLocked)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.Locked, GetText(91937, "Dagen är låst och kan ej behandlas"));
                            return oDTO;
                        }
                        else if (iDTO.TimeBlocks == null)
                        {
                            oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeBlocks");
                            return oDTO;
                        }

                        foreach (AttestEmployeeDayTimeBlockDTO timeBlock in iDTO.TimeBlocks)
                        {
                            DateTime startTime = timeBlock.StartTime;
                            int minutes = (int)timeBlock.StopTime.Subtract(startTime).TotalMinutes;
                            timeBlock.StartTime = CalendarUtility.GetScheduleTime(startTime, timeBlockDate.Date, startTime.Date);
                            timeBlock.StopTime = timeBlock.StartTime.AddMinutes(minutes);
                        }

                        AddOrUpdateDaysToApplyAbsenceTracker(iDTO.ApplyAbsenceItems, timeBlockDate);

                        #endregion

                        #region Perform

                        oDTO.Result = SaveGeneratedDeviationsForPeriod(iDTO.EmployeeId, iDTO.TimeScheduleTemplatePeriodId, iDTO.TimeBlockDateId, iDTO.TimeBlocks, iDTO.TimeCodeTransactions, iDTO.TimePayrollTransactions, iDTO.ApplyAbsenceItems, iDTO.PayrollImportEmployeeTransactionIds);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        #endregion

                        #region Notify

                        if (oDTO.Result.Success)
                        {
                            DoNotifyChangeOfDeviations();
                            DoInitiatePayrollWarnings();
                        }

                        if (oDTO.Result.Success)
                        {
                            EvaluateDeviationsAgainstWorkRules evaluateDeviationsAgainstWorkRulesResult = EvaluateDeviationsAgainstWorkRulesAndSendXEMail(iDTO.EmployeeId, timeBlockDate.Date);
                            if (!string.IsNullOrEmpty(evaluateDeviationsAgainstWorkRulesResult.InfoMessage))
                                oDTO.Result.InfoMessage = evaluateDeviationsAgainstWorkRulesResult.InfoMessage;
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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private SaveWholedayDeviationsOutputDTO TaskSaveWholedayDeviations()
        {
            var (iDTO, oDTO) = InitTask<SaveWholedayDeviationsInputDTO, SaveWholedayDeviationsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.InputTimeBlocks == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlock");
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

                        oDTO.Result = SaveWholedayDeviations(iDTO.InputTimeBlocks, iDTO.TimeDeviationCauseStartId, iDTO.TimeDeviationCauseStopId, iDTO.DeviationComment, TermGroup_TimeDeviationCauseType.Absence, iDTO.EmployeeId, iDTO.EmployeeChildId);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        if (oDTO.Result.Success && this.localDoNotCollectDaysForRecalculationLevel2.HasValue && this.localDoNotCollectDaysForRecalculationLevel2.Value == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence && this.localDoNotCollectDaysForRecalculationLevel3.HasValue && this.localDoNotCollectDaysForRecalculationLevel3Reversed != null && this.localDoNotCollectDaysForRecalculationLevel3Reversed.Any(i => i != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick))
                            oDTO.Result.InfoMessage = GetText(11509, "Kontrollera dagar både framåt och bakåt med samma frånvarotyp. Perioden är låst och därför måste dessa dagar hanteras manuellt");

                        #region Notify

                        if (oDTO.Result.Success)
                        {
                            DoNotifyChangeOfDeviations();
                            DoInitiatePayrollWarnings();
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
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private ValidateDeviationChangeOutputDTO TaskValidateDeviationChange()
        {
            var (iDTO, oDTO) = InitTask<ValidateDeviationChangeInputDTO, ValidateDeviationChangeOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TimeBlocks == null)
            {
                oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorInvalidInputTimeBlocks, "TimeBlocks"));
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(iDTO.EmployeeId);
                    if (employee == null)
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeBlockDateNotFound, GetText(10083, "Anställd hittades inte")));
                        return oDTO;
                    }

                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(iDTO.Date, GetEmployeeGroupsFromCache());
                    if (employeeGroup == null)
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeBlockDateNotFound, GetText(8539, "Tidavtal hittades inte")));
                        return oDTO;
                    }
                    if (!employeeGroup.AutogenTimeblocks)
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeBlockDateNotFound, GetText(10114, "Tidavtal använder stämpling, inte avvikelserapportering")));
                        return oDTO;
                    }

                    TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(iDTO.EmployeeId, iDTO.Date);
                    if (timeBlockDate == null)
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeBlockDateNotFound, GetText(10115, "Dagen hittades inte")));
                        return oDTO;
                    }

                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorAttestStateInitialNotFound, GetText(4880, "Startnivå för attestnivå med typ löneart saknas")));
                        return oDTO;
                    }

                    TimeCode defaultCompanyTimeCode = GetTimeCodeFromCache(GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode));
                    List<TimeDeviationCause> timeDeviationCauses = GetTimeDeviationCausesByEmployeeGroup(employeeGroup.EmployeeGroupId, iDTO.OnlyUseInTimeTerminal);

                    int minutes = (int)iDTO.StopTime.Subtract(iDTO.StartTime).TotalMinutes;
                    int daysOffset = (iDTO.StartTime.Date - iDTO.Date).Days;
                    iDTO.StartTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT.AddDays(daysOffset), iDTO.StartTime);
                    iDTO.StopTime = iDTO.StartTime.AddMinutes(minutes);

                    #endregion

                    #region TimeBlocks

                    List<TimeBlock> timeBlocks = iDTO.TimeBlocks.FromDTOs(iDTO.Date).ToList();
                    if (timeBlocks.Any())
                    {
                        List<AttestPayrollTransactionDTO> timePayrollTransactions = GetTimePayrollTransactionsForDay(employee.EmployeeId, timeBlockDate.TimeBlockDateId).ToDTOs();

                        timeBlocks.SetIsAttestedPayroll(attestStateInitial, timePayrollTransactions);
                        if (timeBlocks.Any(i => i.IsAttested))
                        {
                            oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorDayIsAttested, GetText(10118, "Dagen är attesterad och kan ej ändras")));
                            return oDTO;
                        }
                    }

                    TimeBlock currentTimeBlock = null;
                    if (iDTO.TimeBlockId != 0 || !string.IsNullOrEmpty(iDTO.TimeBlockGuidId))
                    {
                        currentTimeBlock = timeBlocks.Get(iDTO.TimeBlockId, iDTO.TimeBlockGuidId);
                        if (currentTimeBlock == null)
                        {
                            oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeBlockNotFound, GetText(10116, "Tidblock hittades inte")));
                            return oDTO;
                        }
                    }
                    else
                    {
                        if (!iDTO.TimeDeviationCauseId.HasValue)
                        {
                            oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeDeviationCauseNotSpecified, GetText(10122, "Orsak ej angiven")));
                            return oDTO;
                        }

                        TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(iDTO.TimeDeviationCauseId.Value);
                        if (timeDeviationCause == null)
                        {
                            oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeDeviationCauseNotFound, GetText(10119, "Orsak hittades inte")));
                            return oDTO;
                        }

                        currentTimeBlock = new TimeBlock()
                        {
                            IsNew = true,
                            GuidId = Guid.NewGuid(),
                            StartTime = iDTO.StartTime,
                            StopTime = iDTO.StopTime,
                            IsBreak = false,
                            IsPreliminary = false,
                            Comment = (!String.IsNullOrEmpty(iDTO.Comment)) ? iDTO.Comment : "",

                            //Set FK
                            EmployeeId = timeBlockDate.EmployeeId,
                            TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                            TimeScheduleTemplatePeriodId = iDTO.TimeScheduleTemplatePeriodId,
                            TimeDeviationCauseStart = timeDeviationCause,
                            TimeDeviationCauseStop = timeDeviationCause,
                            TimeDeviationCauseStartId = timeDeviationCause.TimeDeviationCauseId,
                            TimeDeviationCauseStopId = timeDeviationCause.TimeDeviationCauseId,
                            EmployeeChildId = iDTO.EmployeeChildId,
                            ShiftTypeId = iDTO.ShiftTypeId,
                            TimeScheduleTypeId = iDTO.TimeScheduleTypeId,
                        };
                        SetCreatedProperties(currentTimeBlock);

                        if (currentTimeBlock.TimeCode == null)
                            currentTimeBlock.TimeCode = new EntityCollection<TimeCode>();
                        if (!currentTimeBlock.TryAddTimeCode(timeDeviationCause.TimeCode))
                            currentTimeBlock.TryAddTimeCode(GetTimeCodeFromCache(GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode)));
                    }

                    SoeTimeBlockDeviationChange deviationChange = currentTimeBlock.GetDeviationChange(iDTO.StartTime, iDTO.StopTime, clientChange: iDTO.ClientChange);
                    if (deviationChange == SoeTimeBlockDeviationChange.Move || deviationChange == SoeTimeBlockDeviationChange.ResizeBothSides)
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorInvalidChange, GetText(10117, "Otillåten ändring")));
                        return oDTO;
                    }
                    if (!timeBlocks.IsValidDeviationChange(iDTO.StartTime, iDTO.StopTime, deviationChange))
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorInvalidChange, GetText(10117, "Otillåten ändring")));
                        return oDTO;
                    }

                    #endregion

                    #region TimeScheduleTemplateBlock

                    List<TimeScheduleTemplateBlock> allScheduleBlocks = GetScheduleBlocksFromCache(iDTO.EmployeeId, iDTO.Date);
                    DecideTimeBlockStandby(currentTimeBlock, allScheduleBlocks, employeeGroup, timeDeviationCauses);
                    List<TimeScheduleTemplateBlock> validScheduleBlocks = FilterScheduleBasedOnStandby(allScheduleBlocks, currentTimeBlock);
                    DateTime scheduleIn = validScheduleBlocks.GetScheduleIn();
                    DateTime scheduleOut = validScheduleBlocks.GetScheduleOut();
                    bool isInsideSchedule = iDTO.StartTime >= scheduleIn && iDTO.StopTime <= scheduleOut;
                    bool isStartBetweenSchedule = allScheduleBlocks.IsBetweenSchedule(iDTO.StartTime); //both between two regular scheduleblocks and between regular and standby scheduleblock
                    bool isStopBetweenSchedule = allScheduleBlocks.IsBetweenSchedule(iDTO.StopTime); //both between two regular scheduleblocks and between regular and standby scheduleblock

                    #endregion

                    #region TimeDeviationCauses

                    if (!iDTO.HasTimeDeviationCauseId)
                    {
                        bool showTimeDeviationCauses = false;
                        List<TimeDeviationCause> timeDeviationCausesValid = new List<TimeDeviationCause>();

                        #region Find TimeDeviationCause

                        if (deviationChange == SoeTimeBlockDeviationChange.None)
                            iDTO.TimeDeviationCauseId = currentTimeBlock.TimeDeviationCauseStartId.Value;
                        else if (currentTimeBlock.TimeDeviationCauseStartId.HasValue && (currentTimeBlock.IsAbsence() || currentTimeBlock.IsBreak))
                            iDTO.TimeDeviationCauseId = currentTimeBlock.TimeDeviationCauseStartId.Value;
                        else if (deviationChange == SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft && currentTimeBlock.IsPresence() && isInsideSchedule && !isStartBetweenSchedule)
                            iDTO.TimeDeviationCauseId = currentTimeBlock.TimeDeviationCauseStartId.Value;
                        else if (deviationChange == SoeTimeBlockDeviationChange.ResizeStartRightDragRight && currentTimeBlock.IsPresence() && isInsideSchedule && !isStopBetweenSchedule)
                            iDTO.TimeDeviationCauseId = currentTimeBlock.TimeDeviationCauseStartId.Value;
                        else if (!timeDeviationCauses.IsNullOrEmpty())
                        {
                            
                            timeDeviationCausesValid.AddRange(timeDeviationCauses.Filter(TermGroup_TimeDeviationCauseType.PresenceAndAbsence));

                            if (deviationChange == SoeTimeBlockDeviationChange.DeleteTimeBlock || deviationChange == SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromLeft || deviationChange == SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromRight)
                            {
                                if (allScheduleBlocks.IsBetweenSchedule(iDTO.StopTime) && currentTimeBlock.TimeDeviationCauseStartId.HasValue)
                                {
                                    iDTO.TimeDeviationCauseId = currentTimeBlock.TimeDeviationCauseStartId.Value;
                                }
                                else
                                {
                                    timeDeviationCausesValid.AddRange(timeDeviationCauses.Filter(TermGroup_TimeDeviationCauseType.Absence));
                                    showTimeDeviationCauses = true;
                                }
                            }
                            else
                            {
                                bool hasChangedStartTime = currentTimeBlock.StartTime != iDTO.StartTime && !timeBlocks.HasAnyToTheLeft(iDTO.StartTime, iDTO.TimeBlockId, discardStandby: true);
                                bool hasChangedStopTime = currentTimeBlock.StopTime != iDTO.StopTime && !timeBlocks.HasAnyToTheRight(iDTO.StopTime, iDTO.TimeBlockId, discardStandby: true);
                                if (hasChangedStartTime || hasChangedStopTime)
                                {
                                    if (hasChangedStartTime)
                                    {
                                        if (iDTO.StartTime < scheduleIn || validScheduleBlocks.IsBetweenSchedule(iDTO.StartTime))
                                            timeDeviationCausesValid.AddRange(timeDeviationCauses.Filter(TermGroup_TimeDeviationCauseType.Presence));
                                        else if (iDTO.StartTime > scheduleIn)
                                            timeDeviationCausesValid.AddRange(timeDeviationCauses.Filter(TermGroup_TimeDeviationCauseType.Absence));
                                        showTimeDeviationCauses = true;
                                    }
                                    else if (hasChangedStopTime)
                                    {
                                        //use all blocks to catch between regular schedule and standby
                                        bool isReducingPresenceBetweenschedule = deviationChange == SoeTimeBlockDeviationChange.ResizeStartRightDragLeft && allScheduleBlocks.IsBetweenSchedule(iDTO.StopTime);
                                        if (!isReducingPresenceBetweenschedule)
                                        {
                                            if (iDTO.StopTime > scheduleOut || validScheduleBlocks.IsBetweenSchedule(iDTO.StopTime))
                                                timeDeviationCausesValid.AddRange(timeDeviationCauses.Filter(TermGroup_TimeDeviationCauseType.Presence));
                                            else if (iDTO.StopTime < scheduleOut)
                                                timeDeviationCausesValid.AddRange(timeDeviationCauses.Filter(TermGroup_TimeDeviationCauseType.Absence));
                                            showTimeDeviationCauses = true;
                                        }

                                    }
                                }
                            }

                            if (!showTimeDeviationCauses)
                            {
                                if (deviationChange == SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft || deviationChange == SoeTimeBlockDeviationChange.ResizeStartLeftDragRight)
                                    iDTO.TimeDeviationCauseId = currentTimeBlock.TimeDeviationCauseStartId.Value;
                                else if (deviationChange == SoeTimeBlockDeviationChange.ResizeStartRightDragLeft || deviationChange == SoeTimeBlockDeviationChange.ResizeStartRightDragRight)
                                    iDTO.TimeDeviationCauseId = currentTimeBlock.TimeDeviationCauseStartId.Value;
                            }
                        }

                        #endregion

                        #region Result

                        if (showTimeDeviationCauses)
                        {
                            oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ChooseDeviationCause, timeDeviationCauses: timeDeviationCausesValid.ToGridDTOs().ToList()));
                            SetGeneratedTimeBlocks(oDTO, timeBlocks, timeBlockDate, timeDeviationCauses, scheduleIn, scheduleOut);
                            return oDTO;
                        }

                        #endregion
                    }

                    #endregion

                    #region Process change

                    if (iDTO.HasTimeDeviationCauseId)
                    {
                        TimeDeviationCause timeDeviationCause = timeDeviationCauses.FirstOrDefault(i => i.TimeDeviationCauseId == iDTO.TimeDeviationCauseId.Value);
                        if (timeDeviationCause == null)
                        {
                            oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeDeviationCauseNotFound, GetText(10119, "Orsak hittades inte")));
                            return oDTO;
                        }

                        List<TimeBlock> validTimeBlocks = new List<TimeBlock>();

                        #region Create excess TimeBlock

                        TimeBlock excessTimeBlock = null;

                        switch (deviationChange)
                        {
                            case SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft:
                                #region ResizeStartLeftDragLeft

                                if (iDTO.StartTime < scheduleIn)
                                {
                                    excessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Presence, iDTO.StartTime, scheduleIn, timeBlockDate, employeeGroup, iDTO.TimeScheduleTemplatePeriodId, validScheduleBlocks, timeDeviationCause, employeeChildId: iDTO.EmployeeChildId, comment: iDTO.Comment);
                                    if (excessTimeBlock != null)
                                        currentTimeBlock.StartTime = scheduleIn;
                                    else
                                        currentTimeBlock.StartTime = iDTO.StartTime;
                                }

                                if (excessTimeBlock != null)
                                {
                                    foreach (TimeBlock timeBlock in timeBlocks.GetAllBefore(scheduleIn))
                                    {
                                        if (CalendarUtility.IsCurrentOverlappedByNew(excessTimeBlock.StartTime, excessTimeBlock.StopTime, timeBlock.StartTime, timeBlock.StopTime))
                                            SetTimeBlockAndTransactionsToDeleted(timeBlock, saveChanges: false);
                                        else if (timeBlock.StopTime > excessTimeBlock.StartTime)
                                            timeBlock.StopTime = excessTimeBlock.StartTime;
                                    }
                                }

                                #endregion
                                break;
                            case SoeTimeBlockDeviationChange.ResizeStartLeftDragRight:
                                #region ResizeStartLeftDragRight

                                if (timeDeviationCause.TimeDeviationCauseId != currentTimeBlock.TimeDeviationCauseStartId)
                                {
                                    SoeTimeRuleType timeRuleType = currentTimeBlock.GetDeviationRuleTypeForStartChange(timeDeviationCause, scheduleIn, iDTO.StartTime);
                                    excessTimeBlock = CreateExcessTimeBlock(timeRuleType, currentTimeBlock.StartTime, iDTO.StartTime, timeBlockDate, employeeGroup, iDTO.TimeScheduleTemplatePeriodId, validScheduleBlocks, timeDeviationCause, employeeChildId: iDTO.EmployeeChildId, comment: iDTO.Comment);
                                    currentTimeBlock.StartTime = iDTO.StartTime;
                                }
                                #endregion
                                break;
                            case SoeTimeBlockDeviationChange.ResizeStartRightDragLeft:
                                #region ResizeStartRightDragLeft

                                if (timeDeviationCause.TimeDeviationCauseId != currentTimeBlock.TimeDeviationCauseStartId)
                                {
                                    SoeTimeRuleType timeRuleType = currentTimeBlock.GetDeviationRuleTypeForStopChange(timeDeviationCause, scheduleOut, iDTO.StopTime);
                                    excessTimeBlock = CreateExcessTimeBlock(timeRuleType, iDTO.StopTime, currentTimeBlock.StopTime, timeBlockDate, employeeGroup, iDTO.TimeScheduleTemplatePeriodId, validScheduleBlocks, timeDeviationCause, employeeChildId: iDTO.EmployeeChildId, comment: iDTO.Comment);
                                }
                                currentTimeBlock.StopTime = iDTO.StopTime;
                                break;

                            #endregion
                            case SoeTimeBlockDeviationChange.ResizeStartRightDragRight:
                                #region ResizeStartRightDragRight

                                if (iDTO.StopTime > scheduleOut)
                                {
                                    excessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Presence, scheduleOut, iDTO.StopTime, timeBlockDate, employeeGroup, iDTO.TimeScheduleTemplatePeriodId, validScheduleBlocks, timeDeviationCause, currentTimeBlock.TimeDeviationCauseStartId, employeeChildId: iDTO.EmployeeChildId, comment: iDTO.Comment);
                                    if (excessTimeBlock != null)
                                    {
                                        currentTimeBlock.StopTime = scheduleOut;
                                        foreach (TimeBlock timeBlock in timeBlocks.GetAllAfter(scheduleOut))
                                        {
                                            if (CalendarUtility.IsCurrentOverlappedByNew(excessTimeBlock.StartTime, excessTimeBlock.StopTime, timeBlock.StartTime, timeBlock.StopTime))
                                                SetTimeBlockAndTransactionsToDeleted(timeBlock, saveChanges: false);
                                            else if (timeBlock.StartTime < excessTimeBlock.StopTime)
                                                timeBlock.StartTime = excessTimeBlock.StopTime;
                                        }
                                    }
                                    else
                                    {
                                        currentTimeBlock.StopTime = iDTO.StopTime;
                                    }
                                }

                                #endregion
                                break;
                            case SoeTimeBlockDeviationChange.NewTimeBlock:
                                #region NewTimeBlock

                                timeBlocks = RearrangeNewTimeBlockAgainstExisting(currentTimeBlock, timeBlocks, timeDeviationCause, addNewTimeBlock: false);
                                foreach (var timeBlock in timeBlocks)
                                {
                                    if (defaultCompanyTimeCode != null && timeBlock.TimeCode.IsNullOrEmpty())
                                    {
                                        if (timeBlock.TimeCode == null)
                                            timeBlock.TimeCode = new EntityCollection<TimeCode>();
                                        timeBlock.TimeCode.Add(defaultCompanyTimeCode);
                                    }
                                }

                                #endregion
                                break;
                            case SoeTimeBlockDeviationChange.DeleteTimeBlock:
                                #region DeleteTimeBlock

                                DateTime oldStopTime = currentTimeBlock.StopTime;
                                currentTimeBlock.StopTime = currentTimeBlock.StartTime;
                                ChangeEntityState(currentTimeBlock, SoeEntityState.Deleted);
                                if (!allScheduleBlocks.IsBetweenSchedule(currentTimeBlock.StopTime))
                                {
                                    DateTime newStopTime = timeBlocks.Where(i => i.StartTime >= oldStopTime).OrderBy(i => i.StartTime).FirstOrDefault()?.StartTime ?? scheduleOut;
                                    excessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, currentTimeBlock.StopTime, newStopTime, timeBlockDate, employeeGroup, iDTO.TimeScheduleTemplatePeriodId, validScheduleBlocks, timeDeviationCause, employeeChildId: iDTO.EmployeeChildId, comment: iDTO.Comment);
                                }

                                #endregion
                                break;
                            case SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromLeft:
                                #region DeleteTimeBlockAdvancedFromLeft
                                {
                                    currentTimeBlock.StartTime = currentTimeBlock.StopTime;
                                    foreach (TimeBlock overlappedTimeBlock in timeBlocks.Where(i => i.StopTime <= iDTO.StartTime))
                                    {
                                        ChangeEntityState(overlappedTimeBlock, SoeEntityState.Deleted);
                                    }
                                    TimeBlock splittedTimeBlock = timeBlocks.FirstOrDefault(tb => tb.StopTime > iDTO.StartTime && tb.StartTime < iDTO.StartTime);
                                    if (splittedTimeBlock != null)
                                    {
                                        splittedTimeBlock.StartTime = iDTO.StartTime;
                                        SetModifiedProperties(splittedTimeBlock);
                                    }
                                    excessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, scheduleIn, iDTO.StartTime, timeBlockDate, employeeGroup, iDTO.TimeScheduleTemplatePeriodId, validScheduleBlocks, timeDeviationCause, employeeChildId: iDTO.EmployeeChildId, comment: iDTO.Comment);
                                }
                                #endregion
                                break;
                            case SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromRight:
                                #region DeleteTimeBlockAdvancedFromRight
                                {
                                    currentTimeBlock.StopTime = currentTimeBlock.StartTime;
                                    foreach (TimeBlock overlappedTimeBlock in timeBlocks.Where(i => i.StartTime >= iDTO.StopTime))
                                    {
                                        ChangeEntityState(overlappedTimeBlock, SoeEntityState.Deleted);
                                    }
                                    TimeBlock splittedTimeBlock = timeBlocks.FirstOrDefault(tb => tb.StartTime < iDTO.StopTime && tb.StopTime > iDTO.StopTime);
                                    if (splittedTimeBlock != null)
                                    {
                                        splittedTimeBlock.StopTime = iDTO.StopTime;
                                        SetModifiedProperties(splittedTimeBlock);
                                    }
                                    excessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, iDTO.StopTime, scheduleOut, timeBlockDate, employeeGroup, iDTO.TimeScheduleTemplatePeriodId, validScheduleBlocks, timeDeviationCause, employeeChildId: iDTO.EmployeeChildId, comment: iDTO.Comment);
                                }
                                #endregion
                                break;
                        }

                        if (excessTimeBlock != null)
                            validTimeBlocks.Add(excessTimeBlock);

                        #endregion

                        #region Adjust current TimeBlock

                        if (excessTimeBlock == null)
                        {
                            switch (deviationChange)
                            {
                                case SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft:
                                case SoeTimeBlockDeviationChange.ResizeStartLeftDragRight:
                                    if (iDTO.StartTime != currentTimeBlock.StartTime)
                                    {
                                        TimeBlock prevTimeBlock = timeBlocks.GetPrev(currentTimeBlock);
                                        if (prevTimeBlock != null && !isStartBetweenSchedule)
                                            prevTimeBlock.StopTime = iDTO.StartTime;
                                        currentTimeBlock.StartTime = iDTO.StartTime;
                                    }
                                    break;
                                case SoeTimeBlockDeviationChange.ResizeStartRightDragLeft:
                                case SoeTimeBlockDeviationChange.ResizeStartRightDragRight:
                                    if (iDTO.StopTime != currentTimeBlock.StopTime)
                                    {
                                        TimeBlock nextTimeBlock = timeBlocks.GetNext(currentTimeBlock);
                                        if (nextTimeBlock != null && !isStopBetweenSchedule)
                                            nextTimeBlock.StartTime = iDTO.StopTime;
                                        currentTimeBlock.StopTime = iDTO.StopTime;
                                    }
                                    break;
                            }
                        }

                        currentTimeBlock.Comment = iDTO.Comment;
                        validTimeBlocks.Add(currentTimeBlock);

                        #endregion

                        #region Check other TimeBlocks

                        foreach (TimeBlock timeBlock in timeBlocks.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime))
                        {
                            //Already handled
                            if (timeBlock.GuidId == currentTimeBlock.GuidId)
                                continue;

                            //Skip invalid TimeBlocks
                            if (timeBlock.StartTime >= timeBlock.StopTime)
                                continue;

                            if (!timeBlock.IsInvolvedInDeviationChange(currentTimeBlock, deviationChange))
                                validTimeBlocks.Add(timeBlock);
                            else if (timeBlock.TryAdjustAfterCurrent(currentTimeBlock, deviationChange))
                                validTimeBlocks.Add(timeBlock);
                        }

                        validTimeBlocks = validTimeBlocks.Where(e => e.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime).ToList();

                        #endregion

                        #region Generate deviations

                        CheckDuplicateTimeBlocks(validTimeBlocks, employee, timeBlockDate, "TaskValidateDeviationChange", ((int)deviationChange).ToString(), iDTO.ToString());
                        var timeIntervalAccounting = iDTO.AccountSetting?.CreateTimeIntervalAccountingDTO(excessTimeBlock?.StartTime ?? iDTO.StartTime, excessTimeBlock?.StopTime ?? iDTO.StopTime);
                        if (timeIntervalAccounting == null)
                            validTimeBlocks.ForEach(tb => tb.DeviationAccounts = null);

                        if (!GenerateDeviationsForPeriod(iDTO.Date, iDTO.TimeScheduleTemplatePeriodId, iDTO.EmployeeId, validTimeBlocks, out List<TimeBlock> outputTimeBlocks, out List<TimeTransactionItem> outputTimeTransactionItems, scheduleBlocks: allScheduleBlocks, timeIntervalAccounting: timeIntervalAccounting).Success)
                        {
                            oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorFailedGenerate, "Utfall kunde ej genereras"));
                            return oDTO;
                        }

                        outputTimeBlocks = outputTimeBlocks.Where(tb => tb.State == (int)SoeEntityState.Active).ToList();

                        #endregion

                        #region Return result

                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.Generated));
                        SetGeneratedTimeCodeTransactions(oDTO, outputTimeTransactionItems);
                        SetGeneratedTimaPayrollTransactions(oDTO, outputTimeBlocks, outputTimeTransactionItems);
                        SetGeneratedTimeBlocks(oDTO, outputTimeBlocks, timeBlockDate, timeDeviationCauses, scheduleIn, scheduleOut);
                        SetApplyAbsenceItems(oDTO);
                        return oDTO;

                        #endregion
                    }
                    else
                    {
                        oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.ErrorTimeDeviationCauseOnTimeBlockNotFound, GetText(10120, "Orsak saknas på valt tidblock")));
                        return oDTO;
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    oDTO = new ValidateDeviationChangeOutputDTO(new ValidateDeviationChangeResult(SoeValidateDeviationChangeResultCode.Unknown));
                }
            }

            return oDTO;
        }

        private TaskRecalculateUnhandledShiftChangesOutputDTO TaskRecalculateUnhandledShiftChanges()
        {
            var (iDTO, oDTO) = InitTask<RecalculateUnhandledShiftChangesInputDTO, TaskRecalculateUnhandledShiftChangesOutputDTO>();
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

                        oDTO.Result = RecalculateUnhandledShiftChanges(iDTO.UnhandledShiftChanges, iDTO.DoRecalculateShifts, iDTO.DoRecalculateExtraShifts);

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

        private GetDayOfAbsenceNumberOutputDTO TaskGetDayOfAbsenceNumber()
        {
            var (iDTO, oDTO) = InitTask<GetDayOfAbsenceNumberInputDTO, GetDayOfAbsenceNumberOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    int absenceDayNumber = GetDayOfAbsenceNumberCoherent(iDTO.EmployeeId, iDTO.Date, (int)iDTO.SysPayrollTypeLevel3, iDTO.MaxDays, iDTO.Interval, out DateTime qualifyingDate, out _, out _);

                    oDTO.Result = new ActionResult(absenceDayNumber > 0);
                    if (oDTO.Result.Success)
                    {
                        oDTO.Result.IntegerValue = absenceDayNumber;
                        oDTO.Result.DateTimeValue = qualifyingDate;
                    }

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
            return oDTO;
        }

        private CreateAbsenceDetailsOutputDTO TaskCreateAbsenceDetails()
        {
            var (iDTO, oDTO) = InitTask<CreateAbsenceDetailsInputDTO, CreateAbsenceDetailsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    string createdBy = "SoftOne";
                    DateTime date = iDTO.StartDate;
                    while (date <= iDTO.StopDate)
                    {
                        DateTime batchTimeStamp = DateTime.Now;
                        DateTime dateFrom = date;
                        DateTime dateTo = CalendarUtility.GetEarliestDate(dateFrom.AddDays(iDTO.BatchInterval), iDTO.StopDate);

                        var transactions = (from t in entities.TimePayrollTransaction
                                                .Include("TimeBlock")
                                            where t.ActorCompanyId == actorCompanyId &&
                                            t.TimeBlockDate.EmployeeId == (iDTO.EmployeeId.HasValue ? iDTO.EmployeeId.Value : t.EmployeeId) &&
                                            t.TimeBlockDate.Date >= dateFrom &&
                                            t.TimeBlockDate.Date <= dateTo &&
                                            t.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                                            t.State == (int)SoeEntityState.Active
                                            select new
                                            {
                                                t.EmployeeId,
                                                t.TimeBlockDateId,
                                                t.TimeBlockId,
                                                t.SysPayrollTypeLevel3,
                                                TimeDeviationCauseId = t.TimeBlock != null ? t.TimeBlock.TimeDeviationCauseStartId : null,
                                            }).ToList();

                        foreach (var transactionsByEmployee in transactions.GroupBy(t => t.EmployeeId))
                        {
                            CreateAbsenceDetailResultDTO employeeResult = oDTO.AbsenceResults.GetEmployeeResult(transactionsByEmployee.Key);

                            List<int> timeBlockDateIds = transactionsByEmployee.Select(t => t.TimeBlockDateId).Distinct().ToList();
                            List<TimeBlockDate> timeBlockDates = entities.TimeBlockDate
                                .Include("TimeBlockDateDetail")
                                .Where(tbd => timeBlockDateIds
                                .Contains(tbd.TimeBlockDateId) && !tbd.TimeBlockDateDetail.Any())
                                .ToList();

                            if (!timeBlockDates.IsNullOrEmpty())
                            {
                                var transactionsByDay = transactionsByEmployee
                                    .GroupBy(t => t.TimeBlockDateId)
                                    .ToDictionary(k => k.Key, v => v.ToList());

                                bool created = false;
                                foreach (TimeBlockDate timeBlockDate in timeBlockDates.OrderBy(tbd => tbd.Date))
                                {
                                    if (!transactionsByDay.ContainsKey(timeBlockDate.TimeBlockDateId))
                                        continue;

                                    foreach (var transactionsByType in transactionsByDay[timeBlockDate.TimeBlockDateId].GroupBy(t => t.SysPayrollTypeLevel3))
                                    {
                                        foreach (var transactionsByDeviationCause in transactionsByType.GroupBy(t => t.TimeDeviationCauseId))
                                        {
                                            CreateTimeBlockDetail(timeBlockDate, SoeTimeBlockDateDetailType.Absence, transactionsByDeviationCause.Key, transactionsByType.Key, null, false, batchTimeStamp, createdBy);
                                            employeeResult.SetDayResult(true, timeBlockDate.Date);
                                            created = true;
                                        }
                                    }
                                }

                                if (created)
                                {
                                    oDTO.Result = Save();
                                    if (!oDTO.Result.Success)
                                        employeeResult.SetDayResult(false, timeBlockDates.Select(d => d.Date).ToArray());
                                }
                            }

                            oDTO.AbsenceResults.Add(employeeResult);
                        }

                        date = dateTo.AddDays(1);
                    }
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

            return oDTO;
        }

        public SaveAbsenceDetailsRatioOutputDTO TaskSaveAbsenceDetailsRatio()
        {
            var (iDTO, oDTO) = InitTask<SaveAbsenceDetailsRatioInputDTO, SaveAbsenceDetailsRatioOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            #region Init

            if (iDTO.TimeAbsenceDetails.IsNullOrEmpty())
                return oDTO;

            if (iDTO.TimeAbsenceDetails.Any(d => d.Ratio < 1 || d.Ratio > 100))
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.IncorrectInput, GetText(91946, "Frånvaroomfattning måste vara mellan 1-100"));
                return oDTO;
            }

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    bool changed = false;
                    DateTime batchTimeStamp = DateTime.Now;
                    List<int> timeBlockDateIds = iDTO.TimeAbsenceDetails.Select(d => d.TimeBlockDateId).ToList();
                    List<TimeBlockDate> timeBlockDates = GetTimeBlockDatesWithDetails(iDTO.EmployeeId, timeBlockDateIds);

                    foreach (var timeAbsenceDetailsByDay in iDTO.TimeAbsenceDetails.GroupBy(d => d.TimeBlockDateId))
                    {
                        TimeBlockDate timeBlockDate = timeBlockDates.FirstOrDefault(tbd => tbd.TimeBlockDateId == timeAbsenceDetailsByDay.Key);
                        if (timeBlockDate == null)
                            continue;

                        foreach (TimeAbsenceDetailDTO timeAbsenceDetail in timeAbsenceDetailsByDay.Where(d => d.Ratio.HasValue))
                        {
                            TimeBlockDateDetail timeBlockDateDetail = timeBlockDate.TimeBlockDateDetail?.FirstOrDefault(tbd => tbd.TimeBlockDateDetailId == timeAbsenceDetail.TimeBlockDateDetailId);
                            if (timeBlockDateDetail == null || timeAbsenceDetail.Ratio.Value == timeBlockDateDetail.Ratio)
                                continue;

                            timeBlockDateDetail.Ratio = timeAbsenceDetail.Ratio;
                            timeBlockDateDetail.ManuallyAdjusted = true;
                            SetModifiedProperties(timeBlockDateDetail, modified: batchTimeStamp);
                            changed = true;
                        }
                    }

                    if (changed)
                        oDTO.Result = Save();
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

            return oDTO;
        }

        public GetDeviationsAfterEmploymentOutputDTO TaskGetDeviationsAfterEmployment()
        {
            var (iDTO, oDTO) = InitTask<GetDeviationsAfterEmploymentInputDTO, GetDeviationsAfterEmploymentOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    var employees = (from e in entities.Employee
                                     where e.ActorCompanyId == ActorCompanyId &&
                                     !e.Employment.Any(emp => !emp.DateTo.HasValue && emp.State == (int)SoeEntityState.Active) &&
                                     e.State == (int)SoeEntityState.Active
                                     select new
                                     {
                                         e.EmployeeId,
                                         e.EmployeeNr,
                                         e.ContactPerson.FirstName,
                                         e.ContactPerson.LastName,
                                         Employments = e.Employment.ToList()
                                     }).ToList();

                    if (!iDTO.EmployeeIds.IsNullOrEmpty())
                        employees = employees.Where(e => iDTO.EmployeeIds.Contains(e.EmployeeId)).ToList();

                    foreach (var employee in employees.Where(e => e.Employments != null).OrderBy(e => e.EmployeeNr))
                    {
                        if (employee.Employments.Any(e => !e.DateTo.HasValue && e.State == (int)SoeEntityState.Active))
                            continue;

                        var employment = employee.Employments
                           .Where(emp => emp.DateTo.HasValue && emp.State == (int)SoeEntityState.Active)
                           .OrderBy(emp => emp.DateTo.Value)
                           .LastOrDefault();
                        if (employment == null)
                            continue;

                        var timePayrollTransactions = GetTimePayrollTransactionsAfterDateWithTimeBlockDate(employee.EmployeeId, employment.DateTo.Value);
                        var timePayrollScheduleTransactions = GetTimePayrollScheduleTransactionsAfterDateWithTimeBlockDate(employee.EmployeeId, employment.DateTo.Value);

                        if (!timePayrollTransactions.IsNullOrEmpty() || !timePayrollScheduleTransactions.IsNullOrEmpty())
                        {
                            oDTO.Deviations.Add(new EmployeeDeviationAfterEmploymentDTO(
                                employee.EmployeeId,
                                employee.EmployeeNr,
                                employee.FirstName,
                                employee.LastName,
                                employment.DateTo.Value,
                                timePayrollTransactions.Select(t => t.TimeBlockDate.Date).Concat(timePayrollScheduleTransactions.Select(t => t.TimeBlockDate.Date)).Distinct().ToList(),
                                timePayrollTransactions.Select(t => t.TimePayrollTransactionId).ToList(),
                                timePayrollScheduleTransactions.Select(t => t.TimePayrollScheduleTransactionId).ToList()
                                ));
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
                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        public DeleteDeviationsDaysAfterEmploymentOutputDTO TaskDeleteDeviationsDaysAfterEmployment()
        {
            var (iDTO, oDTO) = InitTask<DeleteDeviationsDaysAfterEmploymentInputDTO, DeleteDeviationsDaysAfterEmploymentOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.Deviations.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    bool hasChanges = false;
                    foreach (EmployeeDeviationAfterEmploymentDTO employeeTransactionDates in iDTO.Deviations)
                    {
                        Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeTransactionDates.EmployeeId);
                        if (employee?.Employment == null)
                            continue;

                        Employment lastEmployment = employee.GetLastEmployment();
                        if (lastEmployment == null || !lastEmployment.DateTo.HasValue)
                            continue;

                        List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsWithTimeBlockDate(employeeTransactionDates.EmployeeId, employeeTransactionDates.TimePayrollTransactionIds);
                        if (!timePayrollTransactions.IsNullOrEmpty() && timePayrollTransactions.All(t => t.TimeBlockDate.Date > lastEmployment.DateTo))
                        {
                            oDTO.Result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions, saveChanges: false, discardCheckes: true);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            hasChanges = true;
                        }

                        List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = GetTimePayrollScheduleTransactionsWithTimeBlockDate(employee.EmployeeId, employeeTransactionDates.TimeSchedulePayrollTransactionIds);
                        if (!timePayrollScheduleTransactions.IsNullOrEmpty() && timePayrollScheduleTransactions.All(t => t.TimeBlockDate.Date > lastEmployment.DateTo))
                        {
                            oDTO.Result = SetTimePayrollScheduleTransactionsToDeleted(timePayrollScheduleTransactions, saveChanges: false);
                            if (!oDTO.Result.Success)
                                return oDTO;

                            hasChanges = true;
                        }
                    }

                    if (hasChanges)
                        oDTO.Result = Save();
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

            return oDTO;
        }

        #endregion

        #region Deviations

        private List<TimeBlock> GetSequentialDeviations(int employeeId, int timeBlockDateId)
        {
            List<TimeBlock> timeBlocks = (from tb in entities.TimeBlock
                                            .Include("TimeInvoiceTransaction")
                                            .Include("TimePayrollTransaction")
                                            .Include("TimeCodeTransaction")
                                            .Include("EmployeeChild")
                                          where tb.EmployeeId == employeeId &&
                                          tb.TimeBlockDateId == timeBlockDateId &&
                                          tb.StartTime != tb.StopTime &&
                                          tb.State == (int)SoeEntityState.Active
                                          select tb).ToList<TimeBlock>();

            //Load references (needed by GUI)
            foreach (TimeBlock timeBlock in timeBlocks)
            {
                if (!timeBlock.TimeCode.IsLoaded)
                    timeBlock.TimeCode.Load();
                if (!timeBlock.TimeDeviationCauseStartReference.IsLoaded)
                    timeBlock.TimeDeviationCauseStartReference.Load();
                if (!timeBlock.TimeDeviationCauseStopReference.IsLoaded)
                    timeBlock.TimeDeviationCauseStopReference.Load();
            }

            SetIsAttested(timeBlocks);
            SetIsTransferedToSalary(timeBlocks);

            foreach (TimeBlock timeBlock in timeBlocks)
            {
                timeBlock.TransactionTimeCodeIds = new List<int>();

                foreach (TimeCodeTransaction timeCodeTransaction in timeBlock.TimeCodeTransaction)
                {
                    if (!timeBlock.TransactionTimeCodeIds.Contains(timeCodeTransaction.TimeCodeId))
                        timeBlock.TransactionTimeCodeIds.Add(timeCodeTransaction.TimeCodeId);
                }
            }

            return timeBlocks;
        }

        private ActionResult SaveDeviationsFromShifts(List<TimeSchedulePlanningDayDTO> shifts, int employeeId, int deviationCauseId, bool useAbsenceStartAndStop, int? employeeChildId, int? timeScheduleScenarioHeadId, bool recalculateRelatedDays = true, int? shiftTypeId = null, int? timeScheduleTypeId = null, string comment = "", bool useDeviationcauseFromInputShift = false, bool checkExistingWholeDayAbsence = true, decimal? ratio = null)
        {
            #region Prereq

            if (shifts.IsNullOrEmpty())
                return new ActionResult(true);
            if (timeScheduleScenarioHeadId.HasValue) //Deviations shuld not be created for scenario
                return new ActionResult(true);

            ActionResult result = ValidateLockedDays(employeeId, shifts.Select(x => x.ActualDate).ToList());
            if (!result.Success)
                return result;

            result = ValidateShiftsAgainstGivenScenario(shifts, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            Employee employee = GetEmployeeFromCache(employeeId, false);
            if (employee == null)
            {
                result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
                return result;
            }
            List<EmployeeGroup> employeeGroups = new List<EmployeeGroup>();

            bool checkIfWholeDay = false;
            if (!useDeviationcauseFromInputShift && GetCompanyBoolSettingFromCache(CompanySettingType.ValidateVacationWholeDayWhenSaving))
            {
                TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(deviationCauseId);
                if (timeDeviationCause != null && timeDeviationCause.OnlyWholeDay && timeDeviationCause.IsVacation)
                    checkIfWholeDay = true;
            }

            #endregion

            #region Perform

            List<TimeEngineDay> days = new List<TimeEngineDay>();

            bool recalculateAfterRestore = false;
            int currentEmployeeId = employeeId;
            var shiftsGroupedByDate = shifts.Where(x => x.TimeScheduleTemplatePeriodId.HasValue && !x.IsOnDuty()).GroupBy(x => x.ActualDate.Date).ToList();

            #region TimeBlocks

            foreach (var shiftsForDate in shiftsGroupedByDate.OrderBy(g => g.Key))
            {
                #region Prereq

                DateTime currentDate = shiftsForDate.Key;

                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(currentEmployeeId, currentDate, true);
                if (timeBlockDate.TimeBlockDateId == 0)
                {
                    result = Save();
                    if (!result.Success)
                        return result;
                }

                List<IGrouping<int, TimeSchedulePlanningDayDTO>> shiftsForDateGroupedByPeriodId = shiftsForDate.GroupBy(x => x.TimeScheduleTemplatePeriodId.Value).ToList();

                #endregion

                foreach (var shiftsForDateAndPeriod in shiftsForDateGroupedByPeriodId)
                {
                    #region Prereq

                    int currentPeriodId = shiftsForDateAndPeriod.Key;

                    TimeScheduleTemplatePeriod templatePeriod = GetTimeScheduleTemplatePeriodFromCache(currentPeriodId);
                    if (templatePeriod == null)
                    {
                        result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeScheduleTemplatePeriodId");
                        return result;
                    }

                    List<TimeBlock> timeBlocksForPeriod = GetTimeBlocksWithTimeCodeAndTransactions(currentEmployeeId, timeBlockDate.TimeBlockDateId);
                    result = SetTransactionsToDeleted(timeBlocksForPeriod, saveChanges: true);
                    if (!result.Success)
                        return result;

                    this.SetInitiatedAbsenceExisting(employee.EmployeeId, timeBlockDate.Date, timeBlocksForPeriod.GetTimePayrollTransactionLevel3Ids());
                    if (checkExistingWholeDayAbsence)
                    {
                        //Restore day first if it was other absence before
                        int? wholedayDeviationTimeDeviationCauseId = timeBlocksForPeriod.GetWholedayDeviationTimeDeviationCauseId();
                        if (wholedayDeviationTimeDeviationCauseId.HasValue)
                        {
                            TimeDeviationCause wholedayTimeDeviationCause = GetTimeDeviationCauseFromCache(wholedayDeviationTimeDeviationCauseId.Value);
                            if (wholedayTimeDeviationCause?.TimeDeviationCauseId != deviationCauseId && wholedayTimeDeviationCause?.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                            {
                                result = RestoreDayToSchedule(timeBlockDate, clearScheduledAbsence: true);
                                if (!result.Success)
                                    return result;

                                //Reload data
                                ClearEmployeeSicknessPeriodFromCache(employee.EmployeeId, timeBlockDate.Date);
                                timeBlocksForPeriod = GetTimeBlocksWithTimeCodeAndTransactions(currentEmployeeId, timeBlockDate.TimeBlockDateId);
                                recalculateAfterRestore = true;
                            }
                        }
                    }

                    List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingDiscardZeroFromCache(timeScheduleScenarioHeadId, employee.EmployeeId, timeBlockDate.Date.Date, includeStandBy: checkIfWholeDay).Where(x => !x.IsOnDuty()).ToList();
                    if (checkIfWholeDay)
                    {
                        bool useAccountHierarchy = UseAccountHierarchy();
                        List<int> accountIds = null;
                        if (useAccountHierarchy)
                        {
                            List<AttestRoleUser> attestRolesForUsers = AttestManager.GetAttestRoleUsers(entities, actorCompanyId, userId, timeBlockDate.Date.Date, timeBlockDate.Date.Date, includeAttestRole: true, ignoreDates: false, onlyDefaultAccounts: true);
                            if (!attestRolesForUsers.Any(x => x.AttestRole.StaffingByEmployeeAccount))
                            {
                                var accounts = GetAccountInternalsFromCache();
                                var employeeAccounts = AccountManager.GetSelectableEmployeeShiftAccounts(entities, userId, actorCompanyId, employee.EmployeeId, timeBlockDate.Date.Date, accounts, GetAccountDimInternalsWithParentFromCache(), useEmployeeAccountIfNoAttestRole: true);
                                accountIds = employeeAccounts.Select(x => x.AccountId).ToList();
                            }
                        }

                        if ((accountIds == null && scheduleBlocks.Count(x => !x.IsBreak) != shiftsForDateAndPeriod.Count()) || (accountIds != null && scheduleBlocks.Count(x => !x.IsBreak && x.AccountId.HasValue && accountIds.Contains(x.AccountId.Value)) != shiftsForDateAndPeriod.Count()))
                            return new ActionResult(GetText(10931, "Semester måste anges på alla pass på dagen"));

                        scheduleBlocks = scheduleBlocks.Where(x => !x.IsStandby()).ToList();
                    }

                    //Attest
                    SetIsAttested(timeBlocksForPeriod);

                    #endregion

                    foreach (var shift in shiftsForDateAndPeriod)
                    {
                        if (useDeviationcauseFromInputShift && !shift.TimeDeviationCauseId.HasValue)
                            continue;

                        TimeDeviationCause deviationStartCause = useDeviationcauseFromInputShift ? GetTimeDeviationCauseWithTimeCodeFromCache(shift.TimeDeviationCauseId.Value) : GetTimeDeviationCauseWithTimeCodeFromCache(deviationCauseId);
                        if (deviationStartCause == null)
                        {
                            result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");
                            return result;
                        }

                        #region Orderplanning

                        if (shift.Order != null)
                        {
                            int length = (int)(CalendarUtility.GetScheduleTime(shift.AbsenceStopTime, shift.StartTime.Date, shift.StopTime.Date) - CalendarUtility.GetScheduleTime(shift.AbsenceStartTime)).TotalMinutes;
                            length *= -1;
                            UpdateOrderRemainingTime(length, this.actorCompanyId, shift.Order.OrderId);
                        }

                        #endregion

                        #region Generate TimeBlocks

                        TimeBlock newTimeBlock = new TimeBlock()
                        {
                            EmployeeId = employeeId,
                            AccountStdId = GetCompanyNullableIntSettingFromCache(CompanySettingType.AccountEmployeeGroupCost),
                            IsPreliminary = shift.IsPreliminary,
                            Comment = comment.NullToEmpty()
                        };
                        SetCreatedProperties(newTimeBlock);

                        if (useAbsenceStartAndStop)
                        {
                            newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(shift.AbsenceStartTime);
                            newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(shift.AbsenceStopTime, shift.AbsenceStartTime.Date, shift.AbsenceStopTime.Date);
                        }
                        else
                        {
                            newTimeBlock.StartTime = CalendarUtility.GetScheduleTime(shift.StartTime);
                            newTimeBlock.StopTime = CalendarUtility.GetScheduleTime(shift.StopTime, shift.StartTime.Date, shift.StopTime.Date);
                        }

                        if (shift.BelongsToPreviousDay)
                        {
                            newTimeBlock.StartTime = newTimeBlock.StartTime.AddDays(1);
                            newTimeBlock.StopTime = newTimeBlock.StopTime.AddDays(1);
                        }
                        else if (shift.BelongsToNextDay)
                        {
                            newTimeBlock.StartTime = newTimeBlock.StartTime.AddDays(-1);
                            newTimeBlock.StopTime = newTimeBlock.StopTime.AddDays(-1);
                        }

                        //TimeDeviationCause
                        newTimeBlock.TimeDeviationCauseStart = deviationStartCause;
                        newTimeBlock.TimeDeviationCauseStop = deviationStartCause;
                        newTimeBlock.TimeDeviationCauseStartId = deviationStartCause.TimeDeviationCauseId;
                        newTimeBlock.TimeDeviationCauseStopId = deviationStartCause.TimeDeviationCauseId;
                        newTimeBlock.EmployeeChildId = useDeviationcauseFromInputShift ? shift.EmployeeChildId.ToNullable() : employeeChildId;
                        newTimeBlock.ShiftTypeId = shiftTypeId;
                        newTimeBlock.TimeScheduleTypeId = timeScheduleTypeId;

                        //TimeCode
                        if (deviationStartCause.TimeCode != null)
                            newTimeBlock.TimeCode.Add(deviationStartCause.TimeCode);

                        //Sort
                        timeBlocksForPeriod = timeBlocksForPeriod.OrderBy(o => o.StartTime).ToList();

                        //Rearrange
                        decimal? existingRatio = ratio.HasValue ? GetTimeBlockDateDetailRatioFromDeviationCause(timeBlockDate, deviationStartCause.TimeDeviationCauseId) : null;
                        timeBlocksForPeriod = RearrangeNewTimeBlockAgainstExisting(newTimeBlock, timeBlocksForPeriod, deviationStartCause, deleteBreaksOverlappedByAbsence: true);
                        if (existingRatio.HasValue && existingRatio > ratio)
                        {
                            TimeBlock timeBlock = timeBlocksForPeriod.FirstOrDefault(tb => tb.StartTime == newTimeBlock.StopTime);
                            if (timeBlock != null && timeBlock.TimeDeviationCauseStartId == deviationStartCause.TimeDeviationCauseId)
                            {
                                TimeDeviationCause standardTimeDeviationCause = GetTimeDeviationCauseFromPrio(employee, employee.GetEmployeeGroup(timeBlockDate.Date, employeeGroups), null);
                                if (standardTimeDeviationCause != null)
                                {
                                    if (!standardTimeDeviationCause.TimeCodeReference.IsLoaded)
                                        standardTimeDeviationCause.TimeCodeReference.Load();

                                    timeBlock.TimeCode.Clear();
                                    TimeCode timeCode = standardTimeDeviationCause.TimeCode ?? GetTimeCodeFromCache(GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode));
                                    if (timeCode != null)
                                        timeBlock.TimeCode.Add(timeCode);

                                    timeBlock.TimeDeviationCauseStart = standardTimeDeviationCause;
                                    timeBlock.TimeDeviationCauseStop = standardTimeDeviationCause;
                                }
                            }
                        }

                        foreach (TimeBlock timeBlock in timeBlocksForPeriod)
                        {
                            #region Handle new timeblocks

                            if (timeBlock.TimeBlockId != 0)
                                continue;

                            timeBlock.TimeBlockDateId = timeBlockDate.TimeBlockDateId;
                            timeBlock.TimeScheduleTemplatePeriodId = templatePeriod.TimeScheduleTemplatePeriodId;
                            SetCreatedProperties(timeBlock);
                            entities.TimeBlock.AddObject(timeBlock);

                            #endregion
                        }

                        //save so that new timeblocks recieve a timeBlockid
                        result = Save();
                        if (!result.Success)
                            return result;

                        #endregion
                    }

                    #region Absence

                    CreateAbsenceTimeBlocksDuringScheduleTime(ref timeBlocksForPeriod, timeBlockDate, templatePeriod.TimeScheduleTemplatePeriodId, scheduleBlocks, employee);

                    #endregion

                    #region Accounting

                    foreach (TimeBlock timeBlock in timeBlocksForPeriod)
                    {
                        if (timeBlock.StartTime == timeBlock.StopTime)//set zero timeblocks to deleted
                            SetTimeBlockAndTransactionsToDeleted(timeBlock, saveChanges: false);
                        else
                            ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, scheduleBlocks.GetClosest(timeBlock.StartTime, timeBlock.StopTime), employee);
                    }

                    #endregion

                    days.AddDay(
                        templatePeriodId: templatePeriod.TimeScheduleTemplatePeriodId,
                        timeBlockDate: timeBlockDate, 
                        timeBlocks: timeBlocksForPeriod
                        );
                }
            }

            result = Save();
            if (!result.Success)
                return result;

            #endregion

            #region Transactions

            //Save transactions
            result = SaveTransactionsForPeriods(days);
            if (!result.Success)
                return result;

            if (recalculateRelatedDays)
                result = ReCalculateRelatedDays(recalculateAfterRestore ? ReCalculateRelatedDaysOption.ApplyAndRestore : ReCalculateRelatedDaysOption.Apply, employeeId);

            #endregion

            #endregion

            return result;
        }

        private ActionResult SaveGeneratedDeviationsForPeriod(int employeeId, int templatePeriodId, int timeBlockDateId, List<TimeBlock> timeBlocks, List<TimeTransactionItem> timeTransactionItems, List<ApplyAbsenceDTO> applyAbsenceDays)
        {
            ActionResult result = null;

            #region Prereq

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, timeBlockDateId);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            //Input collections
            List<TimeTransactionItem> timeCodeTransactionItems = timeTransactionItems.Where(t => t.TransactionType == SoeTimeTransactionType.TimeCode).ToList();
            List<TimeTransactionItem> timeInvoiceTransactionItems = timeTransactionItems.Where(t => t.TransactionType == SoeTimeTransactionType.TimeInvoice).ToList();
            List<TimeTransactionItem> timePayrollTransactionItems = timeTransactionItems.Where(t => t.TransactionType == SoeTimeTransactionType.TimePayroll && !t.IsScheduleTransaction).ToList();
            List<TimeTransactionItem> timePayrollScheduleTransactionItems = timeTransactionItems.Where(t => t.TransactionType == SoeTimeTransactionType.TimePayroll && t.IsScheduleTransaction).ToList();

            #endregion

            #region TimeBlock

            //Saved collections
            List<TimeBlock> savedTimeblocks = new List<TimeBlock>();
            List<TimeCodeTransaction> savedTimeCodeTransactions = new List<TimeCodeTransaction>();

            //If a TimeBlock's dont have a Guid, it's not been added/modified
            if (timeBlocks.Any(i => i.GuidId.HasValue))
            {
                // Get all TimeBlock's for current date. Update if exists otherwise delete
                List<TimeBlock> existingTimeBlocks = GetTimeBlocksWithTimeCodeAndAccountInternal(employee.EmployeeId, timeBlockDateId, templatePeriodId);
                foreach (TimeBlock timeBlock in existingTimeBlocks)
                {
                    TimeBlock inputTimeBlock = timeBlocks.FirstOrDefault(i => i.TimeBlockId == timeBlock.TimeBlockId);
                    if (inputTimeBlock != null)
                    {
                        #region Update TimeBlock

                        bool manuallyAdjusted = false;
                        if ((timeBlock.StartTime != inputTimeBlock.StartTime) || (timeBlock.StopTime != inputTimeBlock.StopTime))
                            manuallyAdjusted = true;

                        //Update
                        timeBlock.StartTime = inputTimeBlock.StartTime;
                        timeBlock.StopTime = inputTimeBlock.StopTime;
                        timeBlock.Comment = inputTimeBlock.Comment;
                        timeBlock.ManuallyAdjusted = timeBlock.ManuallyAdjusted || manuallyAdjusted;
                        timeBlock.State = (int)SoeEntityState.Active; //Re-activate
                        SetModifiedProperties(timeBlock);

                        //Set FK
                        timeBlock.EmployeeId = employee.EmployeeId;
                        timeBlock.TimeBlockDateId = timeBlockDate.TimeBlockDateId;
                        timeBlock.TimeScheduleTemplatePeriodId = templatePeriodId;
                        timeBlock.TimeDeviationCauseStartId = inputTimeBlock.TimeDeviationCauseStartId;
                        timeBlock.TimeDeviationCauseStopId = inputTimeBlock.TimeDeviationCauseStopId;
                        timeBlock.TimeScheduleTemplateBlockBreakId = inputTimeBlock.TimeScheduleTemplateBlockBreakId;
                        timeBlock.EmployeeChildId = inputTimeBlock.EmployeeChildId;
                        timeBlock.ShiftTypeId = inputTimeBlock.ShiftTypeId;
                        timeBlock.TimeScheduleTypeId = inputTimeBlock.TimeScheduleTypeId;

                        //Update TimeCode's
                        foreach (TimeCode timeCode in inputTimeBlock.TimeCode)
                        {
                            if (timeBlock.TimeCode.Any(i => i.TimeCodeId == timeCode.TimeCodeId))
                                continue;

                            TimeCode originalTimeCode = GetTimeCodeFromCache(timeCode.TimeCodeId);
                            if (originalTimeCode != null)
                                timeBlock.TimeCode.Add(originalTimeCode);
                        }

                        //Preserve Guid, needed later
                        timeBlock.GuidId = inputTimeBlock.GuidId;

                        //Add to save collection
                        savedTimeblocks.Add(timeBlock);

                        #endregion
                    }
                    else
                    {
                        #region Delete TimeBlock and transactions

                        // Transaction does not exist in item collection, delete it
                        result = SetTimeBlockAndTransactionsToDeleted(timeBlock, saveChanges: false);
                        if (!result.Success)
                            return result;

                        #endregion
                    }

                    SetModifiedProperties(timeBlock);
                }

                #region Add TimeBlock

                foreach (TimeBlock newTimeBlock in timeBlocks.GetNew(false))
                {
                    bool isBreak = newTimeBlock.IsBreak();

                    TimeBlock timeBlock = new TimeBlock()
                    {
                        StartTime = newTimeBlock.StartTime,
                        StopTime = newTimeBlock.StopTime,
                        Comment = newTimeBlock.Comment,
                        IsBreak = isBreak,
                        IsPreliminary = newTimeBlock.IsPreliminary,
                        ManuallyAdjusted = !isBreak || newTimeBlock.ManuallyAdjusted,

                        //Set FK
                        EmployeeId = employee.EmployeeId,
                        AccountStdId = newTimeBlock.AccountStdId.ToNullable(),
                        TimeBlockDateId = timeBlockDate.TimeBlockDateId,
                        TimeScheduleTemplatePeriodId = templatePeriodId,
                        TimeDeviationCauseStartId = (newTimeBlock.TimeDeviationCauseStart != null) ? newTimeBlock.TimeDeviationCauseStart.TimeDeviationCauseId : newTimeBlock.TimeDeviationCauseStartId,
                        TimeDeviationCauseStopId = (newTimeBlock.TimeDeviationCauseStop != null) ? newTimeBlock.TimeDeviationCauseStop.TimeDeviationCauseId : newTimeBlock.TimeDeviationCauseStopId,
                        TimeScheduleTemplateBlockBreakId = newTimeBlock.TimeScheduleTemplateBlockBreakId,
                        EmployeeChildId = newTimeBlock.EmployeeChildId,
                        ShiftTypeId = newTimeBlock.ShiftTypeId,
                        TimeScheduleTypeId = newTimeBlock.TimeScheduleTypeId,
                    };
                    SetCreatedProperties(timeBlock);
                    entities.TimeBlock.AddObject(timeBlock);

                    //Save TimeCode's
                    foreach (TimeCode timeCode in newTimeBlock.TimeCode)
                    {
                        TimeCode originalTimeCode = GetTimeCodeFromCache(timeCode.TimeCodeId);
                        if (originalTimeCode != null)
                            timeBlock.TimeCode.Add(originalTimeCode);
                    }

                    //Preserve Guid, needed later
                    timeBlock.GuidId = newTimeBlock.GuidId;

                    //Add to save collection
                    savedTimeblocks.Add(timeBlock);
                }

                #endregion

                //Save TimeBlock's
                result = Save();
                if (!result.Success)
                    return result;
            }

            #endregion

            #region Internal transactions

            //If no TimeBlock's where saved, no TimeCodeTransaction's has been added/modified
            if (savedTimeblocks.Any())
            {
                foreach (TimeTransactionItem timeCodeTransactionItem in timeCodeTransactionItems)
                {
                    #region TimeBlock

                    TimeBlock timeBlock = null;
                    if (timeCodeTransactionItem.TimeBlockId > 0)
                    {
                        //Already saved TimeCodeTransaction's have TimeBlockId relation
                        timeBlock = savedTimeblocks.FirstOrDefault(i => i.TimeBlockId == timeCodeTransactionItem.TimeBlockId);
                    }
                    else
                    {
                        //TimeCodeTransaction's for new TimeBlock's only have Guid relation
                        if (timeCodeTransactionItem.GuidTimeBlockFK.HasValue)
                            timeBlock = savedTimeblocks.FirstOrDefault(i => i.GuidId.HasValue && i.GuidId.Value == timeCodeTransactionItem.GuidTimeBlockFK.Value);
                    }

                    if (timeBlock == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlock");

                    #endregion

                    #region TimeCodeTransaction

                    TimeCodeTransaction timeCodeTransaction = timeCodeTransactionItem.TimeTransactionId > 0 ? GetTimeCodeTransaction(timeCodeTransactionItem.TimeTransactionId, false) : null;
                    if (timeCodeTransaction == null)
                    {
                        timeCodeTransaction = CreateTimeCodeTransaction(timeCodeTransactionItem.TimeCodeId, TimeCodeTransactionType.Time, timeCodeTransactionItem.Quantity, timeBlock.StartTime, timeBlock.StopTime, timeBlockId: timeBlock.TimeBlockId, timeBlockDate: timeBlockDate);
                    }
                    else
                    {
                        timeCodeTransaction.Type = (int)TimeCodeTransactionType.Time;
                        timeCodeTransaction.Quantity = timeCodeTransactionItem.Quantity;
                        timeCodeTransaction.Start = timeBlock.StartTime;
                        timeCodeTransaction.Stop = timeBlock.StopTime;
                        timeCodeTransaction.TimeBlockId = timeBlock.TimeBlockId;
                        timeCodeTransaction.TimeCodeId = timeCodeTransactionItem.TimeCodeId;
                        SetModifiedProperties(timeCodeTransaction);
                    }

                    if (timeCodeTransactionItem.TimeBlockId == 0)
                        timeCodeTransactionItem.TimeBlockId = timeBlock.TimeBlockId;
                    if (timeCodeTransactionItem.TimeRuleId > 0)
                        timeCodeTransaction.TimeRuleId = timeCodeTransactionItem.TimeRuleId;

                    //To keep tracking when saving TimeInvoiceTransaction's and TimePayrollTransaction's
                    timeCodeTransaction.SetIdentifier(timeCodeTransactionItem.GuidInternalPK);
                    savedTimeCodeTransactions.Add(timeCodeTransaction);

                    #endregion
                }
            }

            result = Save();
            if (!result.Success)
                return result;

            #region Escalate TimeBlockI's down to TimeExternalTransactionItems

            foreach (TimeTransactionItem timeCodeTransactionItem in timeCodeTransactionItems)
            {
                if (timeCodeTransactionItem.GuidInternalPK.HasValue)
                {
                    List<TimeTransactionItem> externalTransactions = new List<TimeTransactionItem>();
                    externalTransactions.AddRange(timeInvoiceTransactionItems.Where(i => i.GuidInternalFK.HasValue && i.GuidInternalFK.Value == timeCodeTransactionItem.GuidInternalPK.Value).ToList());
                    externalTransactions.AddRange(timePayrollTransactionItems.Where(i => i.GuidInternalFK.HasValue && i.GuidInternalFK.Value == timeCodeTransactionItem.GuidInternalPK.Value).ToList());
                    foreach (TimeTransactionItem externalTransaction in externalTransactions)
                    {
                        if (externalTransaction.TimeBlockId == 0)
                            externalTransaction.TimeBlockId = timeCodeTransactionItem.TimeBlockId;
                    }
                }
            }

            #endregion

            #endregion

            #region External transactions

            result = SaveTimeInvoiceTransactions(timeInvoiceTransactionItems, savedTimeCodeTransactions, timeBlockDate.Date, timeBlockDate.Date, employee.EmployeeId);
            if (!result.Success)
                return result;

            result = SaveTimePayrollTransactions(timePayrollTransactionItems, savedTimeCodeTransactions, timeBlockDate.Date, timeBlockDate.Date, employee.EmployeeId);
            if (!result.Success)
                return result;

            result = SaveTimePayrollScheduleTransactions(timePayrollScheduleTransactionItems, timeBlockDate.Date, timeBlockDate.Date, employee.EmployeeId);
            if (!result.Success)
                return result;

            result = CreateTimeWorkReductionTransactions(employee, timeBlockDate.Date);
            if (!result.Success)
                return result;

            #endregion

            #region TimeBlockDateDetail

            if (this.useInitiatedAbsenceDays && !applyAbsenceDays.IsNullOrEmpty())
            {
                CreateTimeBlockDetailOutcome(timeBlockDate, applyAbsenceDays);
                result = Save();
            }

            #endregion

            if (result.Success)
                result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Apply, employeeId);

            return result;
        }

        private ActionResult SaveGeneratedDeviationsForPeriod(int employeeId, int templatePeriodId, int timeBlockDateId, List<AttestEmployeeDayTimeBlockDTO> inputTimeBlocks, List<AttestEmployeeDayTimeCodeTransactionDTO> inputTimeCodeTransactions, List<AttestPayrollTransactionDTO> inputTimePayrollTransactions, List<ApplyAbsenceDTO> applyAbsenceDays, List<int> inputPayrollImportEmployeeTransactionIds)
        {
            #region Prereq

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employeeId, timeBlockDateId);
            if (timeBlockDate == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeBlockDate");

            EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, GetEmployeeGroupsFromCache());
            List<TimeBlock> savedTimeBlocks = new List<TimeBlock>();
            List<TimeCodeTransaction> savedTimeCodeTransactions = new List<TimeCodeTransaction>();
            List<TimePayrollTransaction> savedTimePayrollTransactions = new List<TimePayrollTransaction>();

            #endregion

            #region TimeBlock

            ActionResult result = SaveTimeBlocks(inputTimeBlocks, employee, timeBlockDate, templatePeriodId);
            if (!result.Success)
                return result;

            if (result.Value != null)
                savedTimeBlocks = result.Value as List<TimeBlock>;

            #endregion

            #region Internal transsactions

            if (savedTimeBlocks.Any())
            {
                result = SaveTimeCodeTransactions(inputTimeCodeTransactions, savedTimeBlocks, timeBlockDate);
                if (!result.Success)
                    return result;

                if (result.Value is List<TimeCodeTransaction> timeCodeTransactions)
                    savedTimeCodeTransactions = timeCodeTransactions;
            }

            //Escalate TimeBlockId's down to TimeExternalTransactionItem's
            foreach (AttestEmployeeDayTimeCodeTransactionDTO timeCodeTransactionDTO in inputTimeCodeTransactions)
            {
                if (String.IsNullOrEmpty(timeCodeTransactionDTO.GuidId))
                    continue;

                List<AttestPayrollTransactionDTO> externalTransactions = inputTimePayrollTransactions.Where(i => i.GuidIdTimeCodeTransaction == timeCodeTransactionDTO.GuidId).ToList();
                foreach (AttestPayrollTransactionDTO externalTransaction in externalTransactions.Where(i => !i.TimeBlockId.HasValue || i.TimeBlockId.Value == 0))
                {
                    externalTransaction.TimeBlockId = timeCodeTransactionDTO.TimeBlockId;
                }
            }

            #endregion

            #region External transactions

            result = SaveTimePayrollTransactions(employee, inputTimePayrollTransactions.Where(i => !i.IsScheduleTransaction && !i.IsAdditionOrDeduction).ToList(), savedTimeCodeTransactions, timeBlockDate.Date, timeBlockDate.Date);
            if (!result.Success)
                return result;

            if (result.Value is List<TimePayrollTransaction> payrollTransactions)
                savedTimePayrollTransactions = payrollTransactions;

            result = SaveTimePayrollScheduleTransactions(inputTimePayrollTransactions.Where(i => i.IsScheduleTransaction).ToList(), timeBlockDate.Date, timeBlockDate.Date, employee);
            if (!result.Success)
                return result;

            result = CreateTimeWorkReductionTransactions(employee, timeBlockDate.Date);
            if (!result.Success)
                return result;

            #endregion

            #region PayrollImportEmployeeTransaction

            if (!inputPayrollImportEmployeeTransactionIds.IsNullOrEmpty())
            {
                List<PayrollImportEmployeeTransaction> transactions = (from t in entities.PayrollImportEmployeeTransaction
                                                                       where t.PayrollImportEmployee.PayrollImportHead.ActorCompanyId == actorCompanyId &&
                                                                       inputPayrollImportEmployeeTransactionIds.Contains(t.PayrollImportEmployeeTransactionId)
                                                                       select t).ToList();

                foreach (PayrollImportEmployeeTransaction transaction in transactions)
                {
                    transaction.Status = (int)TermGroup_PayrollImportEmployeeTransactionStatus.Processed;
                }

                result = Save();
                if (!result.Success)
                    return result;
            }

            #endregion

            #region TimeBlockDateDetail

            if (this.useInitiatedAbsenceDays && !applyAbsenceDays.IsNullOrEmpty())
            {
                CreateTimeBlockDetailOutcome(timeBlockDate, applyAbsenceDays);
                result = Save();
            }

            #endregion

            #region Recalculate

            if (result.Success)
                result = ReCalculateRelatedDays(ReCalculateRelatedDaysOption.Apply, employeeId);

            #endregion

            AddCurrentDayNotifyChangeOfDeviations(timeBlockDate, employeeGroup);
            AddCurrentDayPayrollWarning(timeBlockDate);
            ApplyPlausibilityCheck(employee, timeBlockDate.Date, savedTimeCodeTransactions, savedTimePayrollTransactions, savedTimeBlocks);

            return result;
        }

        private ActionResult SaveWholedayDeviations(List<TimeBlockDTO> timeBlockInputs, int standardTimeDeviationCauseStartId, int standardTimeDeviationCauseStopId, string deviationComment, TermGroup_TimeDeviationCauseType timeDeviationCauseType, int employeeId, int? employeeChildId, bool scheduledAbsence = false, bool recalculateRelatedDays = true, int? shiftTypeId = null, int? timeScheduleTypeId = null)
        {
            ActionResult result;

            //TimeBlock's passed as input has startime and stoptime that says which span absence should be saved
            var first = timeBlockInputs?.OrderBy(i => i.StartTime).FirstOrDefault();
            var last = timeBlockInputs?.OrderByDescending(i => i.StopTime).FirstOrDefault();
            if (first == null || last == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlock");

            Employee employee = GetEmployeeFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            DateTime startDate = first.StartTime;
            DateTime stopDate = last.StopTime;

            try
            {
                #region Prereq

                List<EmployeeSchedule> employeeSchedules = GetEmployeeSchedulesForEmployeeWithScheduleTemplate(employee.EmployeeId, startDate, stopDate);
                List<TimeScheduleTemplateBlock> scheduleBlocks = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, startDate, stopDate, includeStandBy: true);
                TimeDeviationCause standardTimeDeviationCauseStart = standardTimeDeviationCauseStartId > 0 && !scheduledAbsence ? GetTimeDeviationCauseWithTimeCodeFromCache(standardTimeDeviationCauseStartId) : null;
                int defaultTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCode);

                bool recalculateAfterRestore = false;
                List<TimeEngineDay> days = new List<TimeEngineDay>();

                #endregion

                #region Perform

                foreach (TimeBlockDTO timeBlockInput in timeBlockInputs)
                {
                    #region TimeDeviationCause

                    int timeDeviationCauseStartId = standardTimeDeviationCauseStartId;
                    if (timeDeviationCauseStartId == 0 && timeBlockInput.TimeDeviationCauseStartId.HasValue)
                        timeDeviationCauseStartId = timeBlockInput.TimeDeviationCauseStartId.Value;

                    int timeDeviationCauseStopId = standardTimeDeviationCauseStopId;
                    if (timeDeviationCauseStopId == 0 && timeBlockInput.TimeDeviationCauseStopId.HasValue)
                        timeDeviationCauseStopId = timeBlockInput.TimeDeviationCauseStopId.Value;

                    //Need to load one of the TimeDeviationCause's to get the target TimeCode
                    TimeDeviationCause timeDeviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(timeDeviationCauseStartId);
                    if (timeDeviationCause == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "TimeDeviationCause");

                    #endregion

                    #region Days

                    startDate = timeBlockInput.StartTime;
                    stopDate = timeBlockInput.StopTime;
                    DateTime date = startDate.Date;
                    int nrOfdays = stopDate.Subtract(startDate).Days + 1;

                    for (int day = 1; day <= nrOfdays; day++)
                    {
                        #region Day

                        EmployeeSchedule employeeSchedule = employeeSchedules?.Where(es => date >= es.StartDate && date <= es.StopDate).OrderByDescending(i => i.Created).FirstOrDefault();
                        if (employeeSchedule == null)
                            continue;

                        List<TimeScheduleTemplatePeriod> templatePeriods = employeeSchedule.TimeScheduleTemplateHead?.TimeScheduleTemplatePeriod?.Where(tp => tp.State == (int)SoeEntityState.Active).ToList();
                        if (templatePeriods.IsNullOrEmpty())
                            continue;

                        List<TimeScheduleTemplateBlock> scheduleBlocksForDate = scheduleBlocks?.Where(tb => tb.Date.HasValue && tb.Date.Value == date && tb.State == (int)SoeEntityState.Active).ToList();
                        if (scheduleBlocksForDate.IsNullOrEmpty())
                            continue;

                        TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, date, true);
                        if (timeBlockDate == null)
                            continue;

                        foreach (var templateBlocksByPeriod in scheduleBlocksForDate.GroupBy(i => i.TimeScheduleTemplatePeriodId))
                        {
                            TimeScheduleTemplatePeriod templatePeriod = templatePeriods.FirstOrDefault(tp => tp.TimeScheduleTemplatePeriodId == templateBlocksByPeriod.Key);
                            if (templatePeriod == null)
                                continue;

                            #region Restore day first if it was other absence before

                            List<TimeBlock> timeBlocksForDate = GetTimeBlocksWithTransactions(employeeId, timeBlockDate.TimeBlockDateId);
                            this.SetInitiatedAbsenceExisting(employee.EmployeeId, timeBlockDate.Date, timeBlocksForDate.GetTimePayrollTransactionLevel3Ids());
                            int? wholedayDeviationTimeDeviationCauseId = timeBlocksForDate.GetWholedayDeviationTimeDeviationCauseId();
                            if (wholedayDeviationTimeDeviationCauseId.HasValue)
                            {
                                TimeDeviationCause wholedayTimeDeviationCause = GetTimeDeviationCauseFromCache(wholedayDeviationTimeDeviationCauseId.Value);
                                if (wholedayTimeDeviationCause?.TimeDeviationCauseId != standardTimeDeviationCauseStartId && wholedayTimeDeviationCause?.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                                {
                                    result = RestoreDayToSchedule(timeBlockDate, clearScheduledAbsence: true, scheduleBlocks: scheduleBlocksForDate);
                                    if (!result.Success)
                                        return result;

                                    //Reload data
                                    ClearEmployeeSicknessPeriodFromCache(employee.EmployeeId, timeBlockDate.Date);
                                    scheduleBlocksForDate = GetScheduleBlocksWithTimeCodeAndStaffingFromCache(employee.EmployeeId, date, date);
                                    timeBlocksForDate = GetTimeBlocksWithTransactions(employeeId, timeBlockDate.TimeBlockDateId);
                                    recalculateAfterRestore = true;
                                }
                            }

                            #endregion

                            #region Delete existing deviations

                            result = SetTimeBlocksAndTransactionsToDeleted(timeBlocksForDate, discardCheckes: false);
                            if (!result.Success)
                                return result;

                            #endregion

                            #region Set absence on Schedule

                            if (standardTimeDeviationCauseStart != null && standardTimeDeviationCauseStart.IsAbsence)
                            {
                                foreach (TimeScheduleTemplateBlock templateBlock in scheduleBlocksForDate.Where(b => !b.IsBreak))
                                {
                                    templateBlock.TimeDeviationCauseId = standardTimeDeviationCauseStartId;
                                    templateBlock.TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.Standard;
                                    templateBlock.EmployeeChildId = employeeChildId;
                                    SetShiftStatus(templateBlock);
                                    SetShiftUserStatus(templateBlock, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved);
                                    SetModifiedProperties(templateBlock);
                                }
                            }

                            #endregion

                            #region Create new TimeBlocks

                            List<TimeScheduleTemplateBlock> templateBlockBreaks = scheduleBlocksForDate.Where(i => i.IsBreak).ToList();
                            foreach (TimeScheduleTemplateBlock templateBlock in scheduleBlocksForDate)
                            {
                                #region TimeScheduleTemplateBlock

                                DateTime templateBlockDate = templateBlock.Date ?? timeBlockInput.StartTime;
                                DateTime templateBlockStart = CalendarUtility.GetDateTime(templateBlockDate, templateBlock.StartTime);
                                DateTime templateBlockStop = CalendarUtility.GetDateTime(templateBlockDate, templateBlock.StopTime);
                                DateTime start = templateBlock.StartTime;
                                DateTime stop = templateBlock.StopTime;

                                //Don't check schedule blocks not within timeblock input
                                if (templateBlockStop < timeBlockInput.StartTime || templateBlockStart > timeBlockInput.StopTime)
                                    continue;

                                if (templateBlockStart < timeBlockInput.StartTime)
                                    start = timeBlockInput.StartTime;
                                if (templateBlockStop > timeBlockInput.StopTime)
                                    stop = timeBlockInput.StopTime;

                                #endregion

                                #region TimeBlocks

                                List<TimeBlock> newTimeBlocks = new List<TimeBlock>();

                                if (!templateBlock.TimeCode.IsBreak())
                                {
                                    TimeBlock timeBlock = new TimeBlock
                                    {
                                        StartTime = templateBlock.StartTime,
                                        StopTime = templateBlock.StopTime,
                                        IsBreak = false,
                                        IsPreliminary = false,
                                    };
                                    SetCreatedProperties(timeBlock);
                                    newTimeBlocks.AddRange(DivideTimeBlockAgainstTemplateBlockBreaks(ref timeBlock, templateBlockBreaks, new List<TimeBlock>()));
                                    newTimeBlocks.Add(timeBlock);
                                }
                                else
                                {
                                    TimeBlock timeBlock = new TimeBlock
                                    {
                                        StartTime = start,
                                        StopTime = stop,
                                        IsBreak = true,
                                    };
                                    SetCreatedProperties(timeBlock);
                                    newTimeBlocks.Add(timeBlock);
                                }

                                foreach (TimeBlock timeBlock in newTimeBlocks)
                                {
                                    #region TimeBlock

                                    SetCreatedProperties(timeBlock);
                                    entities.TimeBlock.AddObject(timeBlock);

                                    if (!timeBlock.IsBreak)
                                        timeBlock.Comment = deviationComment;

                                    //Set FK
                                    timeBlock.EmployeeId = employee.EmployeeId;
                                    timeBlock.TimeDeviationCauseStartId = timeDeviationCauseStartId;
                                    timeBlock.TimeDeviationCauseStopId = timeDeviationCauseStopId;
                                    timeBlock.TimeScheduleTemplatePeriodId = templatePeriod.TimeScheduleTemplatePeriodId;
                                    timeBlock.EmployeeChildId = employeeChildId;
                                    timeBlock.ShiftTypeId = shiftTypeId;
                                    timeBlock.TimeScheduleTypeId = timeScheduleTypeId;

                                    //Set relations
                                    timeBlock.TimeBlockDate = timeBlockDate;

                                    //Break
                                    timeBlock.IsBreak = templateBlock.IsBreak;
                                    if (timeBlock.IsBreak)
                                    {
                                        timeBlock.TimeScheduleTemplateBlockBreakId = templateBlock.TimeScheduleTemplateBlockId;

                                        TimeCode timeCode = GetTimeCodeFromCache(templateBlock.TimeCodeId);
                                        if (timeCode != null)
                                            timeBlock.TimeCode.Add(timeCode);
                                    }
                                    else
                                    {
                                        if (timeDeviationCause.TimeCode != null)
                                        {
                                            timeBlock.TimeCode.Add(timeDeviationCause.TimeCode);
                                        }
                                        else if (timeDeviationCauseType == TermGroup_TimeDeviationCauseType.Presence && defaultTimeCodeId != 0)
                                        {
                                            TimeCode defaultCompanyTimeCode = GetTimeCodeFromCache(defaultTimeCodeId);
                                            timeBlock.TimeCode.Add(defaultCompanyTimeCode);
                                        }
                                    }

                                    // Accounting
                                    ApplyAccountingOnTimeBlockFromTemplateBlock(timeBlock, scheduleBlocksForDate.GetFirst(), employee);
                                    if (timeBlock.AccountInternal.IsNullOrEmpty())
                                        ApplyAccountingPrioOnTimeBlock(timeBlock, employee, overwriteAccountStd: false);

                                    #endregion
                                }

                                #endregion

                                days.AddDay(
                                    templatePeriodId: templatePeriod.TimeScheduleTemplatePeriodId, 
                                    timeBlockDate: timeBlockDate, 
                                    timeBlocks: newTimeBlocks
                                    );
                            }

                            #endregion
                        }

                        date = date.AddDays(1);

                        #endregion
                    }

                    #endregion
                }

                #endregion

                #region Save

                result = Save();
                if (!result.Success)
                    return result;

                #endregion

                #region Transactions

                InitAbsenceDays(employee.EmployeeId, days.Select(i => i.Date).ToList(), timeDeviationCauseId: standardTimeDeviationCauseStartId);
                InitEmployeeSettingsCache(employee.EmployeeId, days.GetStartDate(), days.GetStopDate());

                foreach (TimeEngineDay day in days.OrderBy(i => i.Date))
                {
                    result = SaveTransactionsForPeriods(new List<TimeEngineDay> { day });
                    if (!result.Success)
                        return result;

                    var evaluatePriceFormulaInputDTO = GetEvaluatePriceFormulaInputDTOFromCache(onlyFromCache: true);
                    evaluatePriceFormulaInputDTO?.SetDayAsDirty(day.EmployeeId, day.Date);
                }

                #endregion

                #region Recalculate

                if (result.Success && recalculateRelatedDays)
                    result = ReCalculateRelatedDays(recalculateAfterRestore ? ReCalculateRelatedDaysOption.ApplyAndRestore : ReCalculateRelatedDaysOption.Apply, employeeId);

                #endregion
            }
            catch (Exception ex)
            {
                LogError(ex, this.log);
                LogError(new Exception($"SaveWholedayDeviations employeeId:{employeeId};startDate:{startDate.ToShortDateString()};startCauseId:{stopDate.ToShortDateString()};startCauseId:{standardTimeDeviationCauseStartId};stopCauseId:{standardTimeDeviationCauseStopId};employeeId:{employeeId};employeeChildId:{employeeChildId};employeeId:{employeeId};recalculateRelatedDays:{recalculateRelatedDays};shiftTypeId:{shiftTypeId};timeScheduleTypeId:{timeScheduleTypeId};", ex));
                return new ActionResult(ex);
            }
            return result;
        }

        private ActionResult PerformAbsencePlanning(List<TimeSchedulePlanningDayDTO> shiftsInput, Guid batchId, bool skipXEMailOnShiftChanges, int? timeScheduleScenarioHeadId)
        {
            if (shiftsInput.IsNullOrEmpty())
                return new ActionResult(true);

            #region Prereq

            ActionResult result = ValidateShiftsAgainstGivenScenario(shiftsInput, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            List<TimeSchedulePlanningDayDTO> shifts = shiftsInput.Where(x => !x.IsLended).ToList();
            int hiddenEmployeeId = GetHiddenEmployeeIdFromCache();

            #endregion

            #region Perform

            DecideAbsencePlanningAction(shifts, hiddenEmployeeId);

            List<IGrouping<DateTime, TimeSchedulePlanningDayDTO>> shiftsGroupedByDate = shifts.GroupBy(g => g.StartTime.Date).ToList();
            foreach (IGrouping<DateTime, TimeSchedulePlanningDayDTO> shiftsForDate in shiftsGroupedByDate)
            {
                List<IGrouping<int, TimeSchedulePlanningDayDTO>> shiftsGroupedByDateEmployee = shiftsForDate.GroupBy(g => g.EmployeeId).ToList();

                foreach (var shiftsForSpecificDateAndEmployee in shiftsGroupedByDateEmployee.ToList())
                {
                    Guid link = GetNewShiftLink();
                    foreach (var shift in shiftsForSpecificDateAndEmployee)
                    {
                        switch ((AbsenceRequestShiftPlanningAction)shift.AbsenceRequestShiftPlanningAction)
                        {
                            case AbsenceRequestShiftPlanningAction.ReplaceWithOtherEmployee:
                                result = AbsencePlanningActionCopyShiftToGivenEmployee(shift, batchId, link, skipXEMailOnShiftChanges, timeScheduleScenarioHeadId);
                                break;
                            case AbsenceRequestShiftPlanningAction.ReplaceWithHiddenEmployee:
                                result = AbsencePlanningActionCopyShiftToHiddenEmployee(shift, batchId, link, skipXEMailOnShiftChanges, timeScheduleScenarioHeadId);
                                break;
                            case AbsenceRequestShiftPlanningAction.NoReplacement:
                                result = new ActionResult(true);
                                break;
                            case AbsenceRequestShiftPlanningAction.NotApproved:
                                result = new ActionResult(true);
                                break;
                            default:

                                break;
                        }

                        if (!result.Success)
                            return result;
                    }
                }
            }

            #endregion

            return result;
        }

        private ActionResult AbsencePlanningActionCopyShiftToGivenEmployee(TimeSchedulePlanningDayDTO shift, Guid batchId, Guid link, bool skipXEMailOnShiftChanges, int? timeScheduleScenarioHeadId)
        {
            int sourceShiftId = shift.TimeScheduleTemplateBlockId;

            #region Update ShiftUserStatus

            TimeScheduleTemplateBlock originalShift = GetScheduleBlock(sourceShiftId);
            if (originalShift == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");

            TimeSchedulePlanningDayDTO originalShiftDto = originalShift.ToTimeSchedulePlanningDayDTO();

            ActionResult result = ValidateShiftAgainstGivenScenario(originalShiftDto, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            shift.Link = link;

            #endregion

            #region Copy Schedule to given employee

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, sourceShiftId, TermGroup_ShiftHistoryType.AbsencePlanning, null, skipXEMailOnShiftChanges);

            result = CopyTimeScheduleShiftToGivenEmployee(logProperties, sourceShiftId, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Tasks (At this point shift has recived a new TimeScheduleTemplateBlockId => is is a new shift)

            List<TimeScheduleTemplateBlockTaskDTO> shiftTasks = GetTimeScheduleTemplateBlockTasks(sourceShiftId).ToDTOs().ToList();

            result = ConnectTasksWithinShift(shiftTasks, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Create TemplateBlockHistory

            result = CreateTimeScheduleTemplateBlockHistoryEntry(originalShiftDto, logProperties);
            if (!result.Success)
                return result;

            #endregion

            return result;
        }

        private ActionResult AbsencePlanningActionCopyShiftToHiddenEmployee(TimeSchedulePlanningDayDTO shift, Guid bactId, Guid link, bool skipXEMailOnShiftChanges, int? timeScheduleScenarioHeadId)
        {
            int sourceShiftId = shift.TimeScheduleTemplateBlockId;

            #region Update ShiftUserStatus

            TimeScheduleTemplateBlock originalShift = GetScheduleBlock(sourceShiftId);
            if (originalShift == null)
                return new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");

            TimeSchedulePlanningDayDTO originalShiftDto = originalShift.ToTimeSchedulePlanningDayDTO();

            ActionResult result = ValidateShiftAgainstGivenScenario(originalShiftDto, timeScheduleScenarioHeadId);
            if (!result.Success)
                return result;

            shift.Link = link;

            #endregion

            #region Copy Schedule to hidden employee

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(bactId, sourceShiftId, TermGroup_ShiftHistoryType.AbsencePlanning, null, skipXEMailOnShiftChanges);

            result = CopyTimeScheduleShiftToHiddenEmployee(logProperties, sourceShiftId, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Tasks (At this point shift has recived a new TimeScheduleTemplateBlockId => is is a new shift)

            List<TimeScheduleTemplateBlockTaskDTO> shiftTasks = GetTimeScheduleTemplateBlockTasks(sourceShiftId).ToDTOs().ToList();

            result = ConnectTasksWithinShift(shiftTasks, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Create TemplateBlockHistory

            result = CreateTimeScheduleTemplateBlockHistoryEntry(originalShiftDto, logProperties);
            if (!result.Success)
                return result;

            #endregion

            return result;
        }

        private ActionResult AbsenceAnnouncementActionCopyShiftToHiddenEmployee(TimeSchedulePlanningDayDTO shift, int timeDeviationCauseId, Guid batchId, Guid link, int? employeeChildId = null)
        {
            int shiftBlockId = shift.TimeScheduleTemplateBlockId;

            #region Copy Schedule to hidden employee

            TimeDeviationCause deviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(timeDeviationCauseId);
            if (deviationCause == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeDeviationCause");

            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(shiftBlockId);
            if (scheduleBlock == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");

            TimeSchedulePlanningDayDTO shiftCopy = scheduleBlock.ToTimeSchedulePlanningDayDTO();

            ActionResult result = ValidateShiftAgainstGivenScenario(shiftCopy, null);
            if (!result.Success)
                return result;

            shift.Link = link;

            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shiftBlockId, TermGroup_ShiftHistoryType.HandleShiftActionAbsenceAnnouncement, null, false);
            result = CopyTimeScheduleShiftToHiddenEmployee(logProperties, shiftBlockId, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Update Shift status, flags and deviationcause on scheduleblock

            scheduleBlock.TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.Planned;
            scheduleBlock.TimeDeviationCauseId = timeDeviationCauseId;
            scheduleBlock.EmployeeChildId = employeeChildId;
            if (deviationCause.TimeCode != null)
            {
                scheduleBlock.TimeCode = deviationCause.TimeCode;
                scheduleBlock.TimeCodeId = deviationCause.TimeCode.TimeCodeId;
            }

            result = SetShiftUserStatus(shiftBlockId, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved);
            if (!result.Success)
                return result;

            #endregion

            #region Create TemplateBlockHistory

            result = CreateTimeScheduleTemplateBlockHistoryEntry(shiftCopy, logProperties);
            if (!result.Success)
                return result;

            #endregion

            #region Clear Queue

            result = ClearShiftQueue(shiftBlockId);
            if (!result.Success)
                return result;

            #endregion

            return result;
        }

        private ActionResult DragActionAbsence(TimeSchedulePlanningDayDTO shift, int timeDeviationCauseId, Guid batchId, Guid link, bool skipXEMailOnChanges, int? employeeChildId)
        {
            int shiftBlockId = shift.TimeScheduleTemplateBlockId;

            #region Copy Schedule to given employee

            TimeScheduleTemplateBlock scheduleBlock = GetScheduleBlock(shiftBlockId);
            if (scheduleBlock == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeScheduleTemplateBlock");

            TimeSchedulePlanningDayDTO shiftCopy = scheduleBlock.ToTimeSchedulePlanningDayDTO();

            ActionResult result = ValidateShiftAgainstGivenScenario(shiftCopy, null);
            if (!result.Success)
                return result;

            shift.Link = link;
            ShiftHistoryLogCallStackProperties logProperties = new ShiftHistoryLogCallStackProperties(batchId, shiftBlockId, TermGroup_ShiftHistoryType.DragShiftActionAbsence, null, skipXEMailOnChanges);

            result = CopyTimeScheduleShiftToGivenEmployee(logProperties, shiftBlockId, shift);
            if (!result.Success)
                return result;

            #endregion

            #region Update Shift status, flags and deviationcause on scheduleblock

            TimeDeviationCause deviationCause = GetTimeDeviationCauseWithTimeCodeFromCache(timeDeviationCauseId);
            if (deviationCause == null)
            {
                result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, "TimeDeviationCause");
                return result;
            }
            scheduleBlock.TimeDeviationCauseId = timeDeviationCauseId;
            scheduleBlock.EmployeeChildId = employeeChildId;
            if (deviationCause.TimeCode != null)
            {
                scheduleBlock.TimeCode = deviationCause.TimeCode;
                scheduleBlock.TimeCodeId = deviationCause.TimeCode.TimeCodeId;
            }

            scheduleBlock.TimeDeviationCauseStatus = (int)SoeTimeScheduleDeviationCauseStatus.Planned;

            result = SetShiftUserStatus(shiftBlockId, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved);
            if (!result.Success)
                return result;

            #endregion

            #region Create TemplateBlockHistory

            result = CreateTimeScheduleTemplateBlockHistoryEntry(shiftCopy, logProperties);
            if (!result.Success)
                return result;


            #endregion

            #region Shift request

            // Check if there is an existing shift request
            result = InactivateShiftRequest(shiftBlockId);
            if (!result.Success)
                return result;

            #endregion

            #region Clear Queue

            result = ClearShiftQueue(shiftBlockId);
            if (!result.Success)
                return result;

            #endregion

            return result;
        }

        private void DecideAbsencePlanningAction(List<TimeSchedulePlanningDayDTO> shifts, int hiddenEmployeeId)
        {
            foreach (var shift in shifts)
            {
                if (shift.ApprovalTypeId == (int)TermGroup_YesNo.Yes)
                {
                    if (shift.EmployeeId == hiddenEmployeeId)
                    {
                        shift.AbsenceRequestShiftPlanningAction = (int)AbsenceRequestShiftPlanningAction.ReplaceWithHiddenEmployee;
                    }
                    else if (shift.EmployeeId == Constants.NO_REPLACEMENT_EMPLOYEEID)
                    {
                        shift.AbsenceRequestShiftPlanningAction = (int)AbsenceRequestShiftPlanningAction.NoReplacement;
                    }
                    else if (shift.EmployeeId != 0)
                    {
                        shift.AbsenceRequestShiftPlanningAction = (int)AbsenceRequestShiftPlanningAction.ReplaceWithOtherEmployee;
                    }
                }
                else if (shift.ApprovalTypeId == (int)TermGroup_YesNo.No)
                {
                    shift.AbsenceRequestShiftPlanningAction = (int)AbsenceRequestShiftPlanningAction.NotApproved;
                }
                else
                {
                    shift.AbsenceRequestShiftPlanningAction = (int)AbsenceRequestShiftPlanningAction.Undefined;
                }
            }
        }

        private void CreateAbsenceTimeBlocksDuringScheduleTime(ref List<TimeBlock> inputTimeBlocks, TimeBlockDate timeBlockDate, int timeScheduleTemplatePeriodId, List<TimeScheduleTemplateBlock> scheduleBlocks, Employee employee)
        {
            if (scheduleBlocks.IsNullOrEmpty() || scheduleBlocks.All(b => b.StartTime == b.StopTime) || inputTimeBlocks.IsNullOrEmpty())
                return;

            //Create excess timeblocks relative to schedule
            DateTime scheduleIn = scheduleBlocks.GetScheduleIn();
            DateTime scheduleOut = scheduleBlocks.GetScheduleOut();
            TimeBlock firstTimeBlock = inputTimeBlocks.GetFirst();
            TimeBlock lastTimeBlock = inputTimeBlocks.GetLast();
            DateTime presenceIn = firstTimeBlock.StartTime;
            DateTime presenceOut = lastTimeBlock.StopTime;

            if (presenceIn != scheduleIn || presenceOut != scheduleOut)
            {
                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(timeBlockDate.Date, employeeGroups: GetEmployeeGroupsFromCache());
                if (presenceIn > scheduleIn)
                {
                    TimeBlock startExcessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, scheduleIn, presenceIn, timeBlockDate, employeeGroup, timeScheduleTemplatePeriodId, scheduleBlocks: scheduleBlocks, comment: firstTimeBlock.Comment);
                    if (startExcessTimeBlock != null && !startExcessTimeBlock.TimeCode.IsNullOrEmpty())
                    {
                        if (firstTimeBlock.CopyCommentToExcessBlockIfCreated)
                        {
                            startExcessTimeBlock.Comment = firstTimeBlock.Comment;
                            firstTimeBlock.CopyCommentToExcessBlockIfCreated = false;
                            firstTimeBlock.Comment = String.Empty;
                        }

                        inputTimeBlocks.Add(startExcessTimeBlock);
                    }
                }
                if (presenceOut < scheduleOut)
                {
                    TimeBlock stopExcessTimeBlock = CreateExcessTimeBlock(SoeTimeRuleType.Absence, presenceOut, scheduleOut, timeBlockDate, employeeGroup, timeScheduleTemplatePeriodId, scheduleBlocks: scheduleBlocks, comment: lastTimeBlock.Comment);
                    if (stopExcessTimeBlock != null && !stopExcessTimeBlock.TimeCode.IsNullOrEmpty())
                    {
                        if (lastTimeBlock.CopyCommentToExcessBlockIfCreated)
                        {
                            stopExcessTimeBlock.Comment = lastTimeBlock.Comment;
                            lastTimeBlock.CopyCommentToExcessBlockIfCreated = false;
                            lastTimeBlock.Comment = String.Empty;
                        }

                        inputTimeBlocks.Add(stopExcessTimeBlock);
                    }
                }
            }
        }

        private bool HasAttestedDeviations(int timeBlockDateId, int employeeId)
        {
            // TimeInvoiceTransaction
            AttestStateDTO attestStateInitialInvoice = GetAttestStateInitialFromCache(TermGroup_AttestEntity.InvoiceTime);
            if (attestStateInitialInvoice != null)
            {
                bool hasAttestTransactions = (from t in entities.TimeInvoiceTransaction
                                              where t.EmployeeId == employeeId &&
                                              t.AttestStateId != attestStateInitialInvoice.AttestStateId &&
                                              t.TimeBlockDateId == timeBlockDateId &&
                                              t.State == (int)SoeEntityState.Active
                                              select t.TimeInvoiceTransactionId).Any();

                if (hasAttestTransactions)
                    return true;
            }

            // TimePayrollTransaction
            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll != null)
            {
                bool hasAttestTransactions = (from tpt in entities.TimePayrollTransaction
                                              where tpt.EmployeeId == employeeId &&
                                              tpt.AttestStateId != attestStateInitialPayroll.AttestStateId &&
                                              tpt.TimeBlockDateId == timeBlockDateId &&
                                              tpt.State == (int)SoeEntityState.Active
                                              select tpt.TimePayrollTransactionId).Any();

                if (hasAttestTransactions)
                    return true;
            }

            return false;
        }

        private void SetGeneratedTimaPayrollTransactions(ValidateDeviationChangeOutputDTO oDTO, List<TimeBlock> outputTimeBlocks, List<TimeTransactionItem> outputTimeTransactionItems)
        {
            oDTO.ValidationResult.GeneratedTimePayrollTransactions = new List<AttestPayrollTransactionDTO>();

            var accountDims = GetAccountDimsFromCache().ToDTOs();

            foreach (TimeTransactionItem transactionItem in outputTimeTransactionItems.Where(i => i.TransactionType == SoeTimeTransactionType.TimePayroll))
            {
                TimeBlock timeBlock = transactionItem.GetTimeBlock(outputTimeBlocks);
                AttestEmployeeDayTimeCodeTransactionDTO timeCodeTransactionDTO = transactionItem.GetTimeCodeTransaction(oDTO.ValidationResult.GeneratedTimeCodeTransactions);
                AttestPayrollTransactionDTO timePayrollTransactionDTO = transactionItem.ToAttestEmployeeTimePayrollTransactionDTO(accountDims, timeBlock, timeCodeTransactionDTO);
                if (timePayrollTransactionDTO != null)
                    oDTO.ValidationResult.GeneratedTimePayrollTransactions.Add(timePayrollTransactionDTO);
            }
        }

        private void SetGeneratedTimeCodeTransactions(ValidateDeviationChangeOutputDTO oDTO, List<TimeTransactionItem> outputTimeTransactionItems)
        {
            oDTO.ValidationResult.GeneratedTimeCodeTransactions = new List<AttestEmployeeDayTimeCodeTransactionDTO>();
            foreach (TimeTransactionItem transactionItem in outputTimeTransactionItems.Where(i => i.TransactionType == SoeTimeTransactionType.TimeCode))
            {
                TimeCode timeCode = GetTimeCodeFromCache(transactionItem.TimeCodeId);
                AttestEmployeeDayTimeCodeTransactionDTO timeCodeTransactionDTO = transactionItem.ToAttestEmployeeTimeCodeTransactionDTO(timeCode);
                if (timeCodeTransactionDTO != null)
                    oDTO.ValidationResult.GeneratedTimeCodeTransactions.Add(timeCodeTransactionDTO);
            }
        }

        private static void SetGeneratedTimeBlocks(ValidateDeviationChangeOutputDTO oDTO, List<TimeBlock> outputTimeBlocks, TimeBlockDate timeBlockDate, List<TimeDeviationCause> timeDeviationCauses, DateTime scheduleIn, DateTime scheduleOut)
        {
            oDTO.ValidationResult.GeneratedTimeBlocks = outputTimeBlocks.ToAttestEmployeeTimeBlockDTOs(timeBlockDate, scheduleIn, scheduleOut, oDTO.ValidationResult.GeneratedTimePayrollTransactions, timeDeviationCauses).ToList();
        }

        private void SetApplyAbsenceItems(ValidateDeviationChangeOutputDTO oDTO)
        {
            oDTO.ValidationResult.ApplyAbsenceItems = ConvertToApplyAbsenceDayDTOs(GetDaysFromApplyAbsenceTracker());
        }

        #endregion

        #region UnhandledShiftChanges

        private ActionResult RecalculateUnhandledShiftChanges(List<TimeUnhandledShiftChangesEmployeeDTO> unhandledShiftChanges, bool doRecalculateShifts, bool doRecalculateExtraShifts)
        {
            if (unhandledShiftChanges.IsNullOrEmpty() || !unhandledShiftChanges.Any(u => u.HasDays))
                return new ActionResult((int)ActionResultSave.EntityIsNull, "UnhandledShiftChangesDTO");

            bool doRecalculateOvertimeDays = DoUseUnhandledShiftChangesByOvertime(out List<int> timeDeviationCauseIdsOvertime) && doRecalculateShifts;
            bool doRecalculateSickDays = DoUseUnhandledShiftChangesBySick() && doRecalculateExtraShifts;
            if (!doRecalculateOvertimeDays && !doRecalculateSickDays)
                return new ActionResult(false);

            this.globalDoNotTryRestoreUnhandledShiftsChanges = true;

            ActionResult result = new ActionResult(true);
            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

            foreach (var unhandledEmployee in unhandledShiftChanges.Where(u => u.HasDays))
            {
                List<int> errorNumbers = new List<int>();
                DateTime dateFrom = unhandledEmployee.Weeks.Min(week => week.DateFrom);
                DateTime dateTo = unhandledEmployee.Weeks.Max(week => week.DateTo);

                if (doRecalculateOvertimeDays)
                {
                    result = RecalculateOvertimeDays();
                    if (!result.Success)
                        return result;
                }
                if (doRecalculateSickDays)
                {
                    result = RecalculateSickDays();
                    if (!result.Success)
                        return result;
                }

                ActionResult RecalculateOvertimeDays()
                {
                    List<TimeBlock> overtimeTimeBlocks = doRecalculateOvertimeDays ? GetTimeBlocksWithTimeBlockDate(unhandledEmployee.EmployeeId, dateFrom, dateTo).FilterOvertime(timeDeviationCauseIdsOvertime) : null;
                    if (overtimeTimeBlocks.IsNullOrEmpty())
                        return new ActionResult(true);

                    ActionResult subResult = new ActionResult(true);

                    foreach (var unhandledWeek in unhandledEmployee.Weeks.Where(week => !week.ShiftDays.IsNullOrEmpty()))
                    {
                        List<TimeBlock> overtimeTimeBlocksForWeek = overtimeTimeBlocks.Filter(unhandledWeek.DateFrom, unhandledWeek.DateTo);
                        if (overtimeTimeBlocksForWeek.IsNullOrEmpty())
                            continue;

                        List<TimeBlockDate> overtimeDaysForWeek = GetTimeBlockDatesFromCache(unhandledEmployee.EmployeeId, overtimeTimeBlocksForWeek.Select(i => i.TimeBlockDateId));
                        subResult = ReCalculateTransactionsDiscardAttest(overtimeDaysForWeek, errorNumbers, currentDate: unhandledWeek.DateTo);
                        if (!subResult.Success)
                            return subResult;

                        List<TimeBlockDate> shiftChangeDaysForWeek = GetTimeBlockDatesFromCache(unhandledEmployee.EmployeeId, unhandledWeek.ShiftDays.Select(i => i.TimeBlockDateId));
                        subResult = RestoreDaysWithUnhandledShiftsChanges(shiftChangeDaysForWeek);
                        if (!subResult.Success)
                            return subResult;
                    }

                    return subResult;
                }
                ActionResult RecalculateSickDays()
                {
                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(unhandledEmployee.EmployeeId);
                    if (employee == null)
                        return new ActionResult(true);

                    List<TimePayrollTransaction> sickTransactions = doRecalculateSickDays ? GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, dateFrom, dateTo, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick) : null;
                    if (sickTransactions.IsNullOrEmpty())
                        return new ActionResult(true);

                    ActionResult subResult = new ActionResult(true);

                    foreach (var unhandledWeek in unhandledEmployee.Weeks.Where(week => !week.ExtraShiftDays.IsNullOrEmpty()))
                    {
                        EmployeeGroup employeeGroup = employee.GetEmployeeGroup(unhandledWeek.DateFrom, unhandledWeek.DateTo, employeeGroups);
                        if (!employeeGroup.UseQualifyingDayCalculationRuleWorkTimeWeekPlusExtraShifts())
                            continue;

                        List<TimePayrollTransaction> sickTransactionsForWeek = sickTransactions.Filter(unhandledWeek.DateFrom, unhandledWeek.DateTo);
                        if (sickTransactionsForWeek.IsNullOrEmpty())
                            continue;

                        List<TimeBlockDate> sickDaysForWeek = GetTimeBlockDatesFromCache(employee.EmployeeId, sickTransactionsForWeek.Select(i => i.TimeBlockDateId));
                        subResult = ReCalculateTransactionsDiscardAttest(sickDaysForWeek, errorNumbers, currentDate: unhandledWeek.DateTo);
                        if (!subResult.Success)
                            return subResult;

                        List<TimeBlockDate> extraShiftChangeDaysForWeek = GetTimeBlockDatesFromCache(employee.EmployeeId, unhandledWeek.ExtraShiftDays.Select(i => i.TimeBlockDateId));
                        subResult = RestoreDaysWithUnhandledExtraShiftsChanges(extraShiftChangeDaysForWeek);
                        if (!subResult.Success)
                            return subResult;
                    }

                    return subResult;
                }
            }

            this.globalDoNotTryRestoreUnhandledShiftsChanges = false;

            return result;
        }

        private ActionResult TryRestoreUnhandledShiftsChanges(int employeeId, List<int> timeBlockDateIdsRecalculated, bool force = false)
        {
            return TryRestoreUnhandledShiftsChanges(GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId), timeBlockDateIdsRecalculated, force: force);
        }

        private ActionResult TryRestoreUnhandledShiftsChanges(Employee employee, List<int> timeBlockDateIdsRecalculated, bool force = false)
        {
            if (employee == null || timeBlockDateIdsRecalculated.IsNullOrEmpty())
                return new ActionResult(true);

            return TryRestoreUnhandledShiftsChanges(employee, GetTimeBlockDatesFromCache(employee.EmployeeId, timeBlockDateIdsRecalculated), force: force);
        }

        private ActionResult TryRestoreUnhandledShiftsChanges(Employee employee, List<TimeBlockDate> timeBlockDatesRecalculated, bool force = false)
        {
            if (employee == null || timeBlockDatesRecalculated.IsNullOrEmpty())
                return new ActionResult(true);

            List<EmployeeGroup> employeeGroups = GetEmployeeGroupsFromCache();

            ActionResult result = RestoreOvertimeDaysWithUnhandledShiftsChanges();
            if (!result.Success)
                return result;

            result = RestoreSickDaysWithUnhandledExtraShiftsChanges();
            return result;

            ActionResult RestoreOvertimeDaysWithUnhandledShiftsChanges()
            {
                if (timeBlockDatesRecalculated.All(tbd => !tbd.HasUnhandledShiftChanges) || !DoTryRestoreOvertimeDaysWithUnhandledShiftsChanges())
                    return new ActionResult(true);

                ActionResult subResult = new ActionResult();

                foreach (var week in GetWeeks())
                {
                    List<TimeBlockDate> timeBlockDatesRecalculatedForWeek = timeBlockDatesRecalculated.Filter(week.WeekStart, week.WeekStop);
                    if (timeBlockDatesRecalculatedForWeek.IsNullOrEmpty())
                        continue;

                    List<TimeBlockDate> timeBlockDatesUnhandledForWeek = GetTimeBlockDatesFromCache(employee.EmployeeId, week.WeekStart, week.WeekStop).Where(i => i.HasUnhandledShiftChanges).ToList();
                    if (timeBlockDatesUnhandledForWeek.IsNullOrEmpty())
                        continue;

                    subResult = RestoreDaysWithUnhandledShiftsChanges(timeBlockDatesUnhandledForWeek);
                    if (subResult.Success)
                        return subResult;
                }

                return subResult;
            }
            ActionResult RestoreSickDaysWithUnhandledExtraShiftsChanges()
            {
                if (timeBlockDatesRecalculated.All(tbd => !tbd.HasUnhandledExtraShiftChanges) || !DoTryRestoreSickDaysWithUnhandledExtraShiftsChanges())
                    return new ActionResult(true);

                if (!force)
                {
                    timeBlockDatesRecalculated = GetDaysWithAbsenceFromCache(employee.EmployeeId, timeBlockDatesRecalculated, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick);
                    if (timeBlockDatesRecalculated.IsNullOrEmpty())
                        return new ActionResult(true);
                }

                ActionResult subResult = new ActionResult();

                foreach (var week in GetWeeks())
                {
                    EmployeeGroup employeeGroup = employee.GetEmployeeGroup(week.WeekStart, week.WeekStop, employeeGroups);
                    if (!employeeGroup.UseQualifyingDayCalculationRuleWorkTimeWeekPlusExtraShifts())
                        continue;

                    List<TimeBlockDate> timeBlockDatesRecalculatedForWeek = timeBlockDatesRecalculated.Filter(week.WeekStart, week.WeekStop);
                    if (timeBlockDatesRecalculatedForWeek.IsNullOrEmpty())
                        continue;

                    List<TimeBlockDate> timeBlockDatesUnhandledForWeek = GetTimeBlockDatesFromCache(employee.EmployeeId, week.WeekStart, week.WeekStop).Where(i => i.HasUnhandledExtraShiftChanges).ToList();
                    if (timeBlockDatesUnhandledForWeek.IsNullOrEmpty())
                        continue;

                    if (!force)
                    {
                        List<TimePayrollTransaction> timePayrollTransactionsSickForWeek = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(employee.EmployeeId, week.WeekStart, week.WeekStop, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick);
                        if (timePayrollTransactionsSickForWeek.IsNullOrEmpty())
                            continue;

                        List<int> sickDaysNotRecalculated = timePayrollTransactionsSickForWeek.Where(tpt => !timeBlockDatesRecalculatedForWeek.Any(tbd => tbd.TimeBlockDateId == tpt.TimeBlockDate.TimeBlockDateId)).Select(i => i.TimeBlockDateId).Distinct().ToList();
                        if (sickDaysNotRecalculated.Any())
                            continue; //Not all sick days in week are recacalculated
                    }

                    subResult = RestoreDaysWithUnhandledExtraShiftsChanges(timeBlockDatesUnhandledForWeek);
                    if (subResult.Success)
                        return subResult;
                }

                return subResult;
            }
            List<(DateTime WeekStart, DateTime WeekStop)> GetWeeks()
            {
                return CalendarUtility.GetWeeks(timeBlockDatesRecalculated.First().Date, timeBlockDatesRecalculated.Last().Date);
            }
        }

        private ActionResult RestoreDaysWithUnhandledShiftsChanges(List<TimeBlockDate> timeBlockDates)
        {
            List<TimeBlockDate> validTimeBlockDates = timeBlockDates?.Where(i => i.HasUnhandledShiftChanges).ToList() ?? new List<TimeBlockDate>();
            if (!validTimeBlockDates.Any())
                return new ActionResult(true);

            foreach (TimeBlockDate timeBlockDate in validTimeBlockDates)
            {
                timeBlockDate.HasUnhandledShiftChanges = false;
                SetModifiedProperties(timeBlockDate);
            }

            return Save();
        }

        private ActionResult RestoreDaysWithUnhandledExtraShiftsChanges(List<TimeBlockDate> timeBlockDates)
        {
            List<TimeBlockDate> validTimeBlockDates = timeBlockDates?.Where(i => i.HasUnhandledExtraShiftChanges).ToList() ?? new List<TimeBlockDate>();
            if (!validTimeBlockDates.Any())
                return new ActionResult(true);

            foreach (TimeBlockDate timeBlockDate in validTimeBlockDates)
            {
                timeBlockDate.HasUnhandledExtraShiftChanges = false;
                SetModifiedProperties(timeBlockDate);
            }

            return Save();
        }

        private bool DoTryRestoreOvertimeDaysWithUnhandledShiftsChanges()
        {
            if (this.globalDoNotTryRestoreUnhandledShiftsChanges || !DoUseUnhandledShiftChangesByOvertime())
                return false;
            return true;
        }

        private bool DoTryRestoreSickDaysWithUnhandledExtraShiftsChanges()
        {
            if (this.globalDoNotTryRestoreUnhandledShiftsChanges || !DoUseUnhandledShiftChangesBySick())
                return false;
            return true;
        }

        private bool DoUseUnhandledShiftChangesByOvertime()
        {
            return DoUseUnhandledShiftChangesByOvertime(out _);
        }

        private bool DoUseUnhandledShiftChangesByOvertime(out List<int> timeDeviationCausesOvertime)
        {
            timeDeviationCausesOvertime = GetTimeDeviationCausesFromCache().Where(t => t.CandidateForOvertime).Select(t => t.TimeDeviationCauseId).ToList();
            return timeDeviationCausesOvertime.Any();
        }

        private bool DoUseUnhandledShiftChangesBySick()
        {
            return GetCompanyBoolSettingFromCache(CompanySettingType.TimeSchedulePlanningSetShiftAsExtra);
        }

        #endregion
    }
}
